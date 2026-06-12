using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;

namespace MYB51.Editor
{
    public static class MYB51PerformanceBudgetValidator
    {
        private const string ScenePath = "Assets/Scenes/MYB89UnityMcpProbe.unity";
        private const string ResultsDirectory = "_bmad-output/unity-test-results";
        private const string JsonReportName = "myb-51-performance-baseline.json";
        private const string TextReportName = "myb-51-performance-baseline.txt";

        private const int FpsTarget = 60;
        private const int FpsWarningBelow = 45;
        private const int FpsRedBelow = 30;
        private const long BuildTargetBytes = 500L * 1024L * 1024L;
        private const long BuildWarningBytes = 750L * 1024L * 1024L;
        private const long BuildRedBytes = 1024L * 1024L * 1024L;
        private const int LaunchTargetSeconds = 10;
        private const int LaunchWarningSeconds = 20;
        private const int LaunchRedSeconds = 30;

        [MenuItem("Tools/MYB-51/Validate Performance Baseline")]
        public static string ValidatePerformanceBaselineCli()
        {
            var stopwatch = Stopwatch.StartNew();
            var hardFailures = new List<string>();
            var softSignals = new List<string>();

            OpenProbeScene(hardFailures);
            var report = BuildReport(stopwatch, hardFailures, softSignals);
            WriteReports(report);

            if (hardFailures.Count > 0)
            {
                throw new InvalidOperationException("MYB-51 performance baseline validation failed. See " + report.TextReportPath);
            }

            Debug.Log("MYB-51 performance baseline validation passed. Report: " + report.TextReportPath);
            return report.TextReportPath;
        }

        private static void OpenProbeScene(ICollection<string> hardFailures)
        {
            if (!File.Exists(ProjectRelativeToAbsolute(ScenePath)))
            {
                hardFailures.Add("Missing canonical scene: " + ScenePath + ".");
                return;
            }

            var activeScene = SceneManager.GetActiveScene();
            if (activeScene.path != ScenePath)
            {
                EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            }
        }

        private static PerformanceReport BuildReport(
            Stopwatch stopwatch,
            List<string> hardFailures,
            List<string> softSignals)
        {
            var repoRoot = GetRepoRoot();
            var outputDirectory = Path.Combine(repoRoot, ResultsDirectory);
            Directory.CreateDirectory(outputDirectory);

            var report = new PerformanceReport
            {
                Ticket = "MYB-51",
                Title = "Unity macOS-first performance baseline",
                TimestampUtc = DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture),
                UnityVersion = Application.unityVersion,
                ProjectPath = Directory.GetParent(Application.dataPath)?.FullName ?? Application.dataPath,
                ScenePath = ScenePath,
                ActiveScenePath = SceneManager.GetActiveScene().path,
                BaselineHardware = "MacBook Pro Mac14,9 / Apple M2 Pro / macOS 26.5.1",
                BaselineUnity = "Unity 6000.4.10f1",
                WebGlScope = "Secondary; not measured unless a ticket explicitly asks for browser proof.",
                JsonReportPath = Path.Combine(outputDirectory, JsonReportName),
                TextReportPath = Path.Combine(outputDirectory, TextReportName),
                Budgets = CreateBudgets(),
                Measurements = CaptureMeasurements(stopwatch, softSignals),
                HardFailures = hardFailures.ToArray(),
                SoftSignals = softSignals.ToArray(),
                Notes = new[]
                {
                    "Budget red statuses are decision signals, not automatic blockers in MYB-51.",
                    "Editor macOS validation is mandatory for this baseline.",
                    "macOS build size and launch time are opportunistic measurements and may be unmeasured in this ticket.",
                    "This is a lightweight guardrail, not an exhaustive profiler or optimization pipeline."
                }
            };

            if (report.ActiveScenePath != ScenePath)
            {
                hardFailures.Add("Active scene is not the canonical probe scene: " + report.ActiveScenePath + ".");
            }

            if (report.Measurements.ActiveRendererCount <= 0)
            {
                hardFailures.Add("No active renderers found in the canonical scene.");
            }

            if (!report.Measurements.HasMainCamera)
            {
                hardFailures.Add("Missing Main Camera in the canonical scene.");
            }

            if (!report.Measurements.HasRouteRoad)
            {
                hardFailures.Add("Missing MYB89_RouteRoad mesh in the canonical scene.");
            }

            report.HardFailures = hardFailures.ToArray();
            report.Status = hardFailures.Count == 0 ? "PASS" : "FAIL";
            report.OverallSoftStatus = WorstSoftStatus(report.Measurements.FpsStatus, report.Measurements.BuildSizeStatus, report.Measurements.LaunchTimeStatus);

            return report;
        }

        private static PerformanceMeasurements CaptureMeasurements(Stopwatch stopwatch, ICollection<string> softSignals)
        {
            var activeRenderers = UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsInactive.Exclude)
                .Where(renderer => renderer.enabled && renderer.gameObject.activeInHierarchy)
                .ToArray();
            var meshFilters = UnityEngine.Object.FindObjectsByType<MeshFilter>(FindObjectsInactive.Exclude)
                .Where(filter => filter.sharedMesh != null && filter.gameObject.activeInHierarchy)
                .ToArray();
            var skinnedMeshes = UnityEngine.Object.FindObjectsByType<SkinnedMeshRenderer>(FindObjectsInactive.Exclude)
                .Where(renderer => renderer.sharedMesh != null && renderer.enabled && renderer.gameObject.activeInHierarchy)
                .ToArray();
            var lights = UnityEngine.Object.FindObjectsByType<Light>(FindObjectsInactive.Exclude)
                .Where(light => light.enabled && light.gameObject.activeInHierarchy)
                .ToArray();

            var materials = new HashSet<int>();
            var textures = new Dictionary<int, TextureInfo>();
            foreach (var renderer in activeRenderers)
            {
                foreach (var material in renderer.sharedMaterials)
                {
                    if (material == null)
                    {
                        continue;
                    }

                    materials.Add(material.GetInstanceID());
                    CaptureMaterialTextures(material, textures);
                }
            }

            var estimatedTextureBytes = textures.Values.Sum(texture => texture.EstimatedBytes);
            var maxTextureDimension = textures.Values.Count == 0 ? 0 : textures.Values.Max(texture => Math.Max(texture.Width, texture.Height));
            var elapsedSeconds = Math.Max(0.001d, stopwatch.Elapsed.TotalSeconds);

            softSignals.Add("Editor rendering FPS is not sampled by this synchronous validator; use manual Play Mode or profiler capture if a future issue makes FPS blocking.");
            softSignals.Add("macOS build size and launch time are recorded as not measured unless a local build is produced during the ticket.");

            var measurements = new PerformanceMeasurements
            {
                MeasurementMode = "Unity Editor static scene baseline",
                MeasurementSeconds = elapsedSeconds,
                FpsMeasured = false,
                FpsAverage = 0f,
                FpsStatus = "not-measured",
                BuildSizeMeasured = false,
                BuildSizeBytes = 0L,
                BuildSizeStatus = "not-measured",
                LaunchTimeMeasured = false,
                LaunchTimeSeconds = 0f,
                LaunchTimeStatus = "not-measured",
                ActiveRendererCount = activeRenderers.Length,
                MeshFilterCount = meshFilters.Length,
                SkinnedMeshRendererCount = skinnedMeshes.Length,
                EstimatedTriangleCount = CountMeshFilterTriangles(meshFilters) + CountSkinnedMeshTriangles(skinnedMeshes),
                ActiveLightCount = lights.Length,
                UniqueMaterialCount = materials.Count,
                UniqueTextureCount = textures.Count,
                MaxTextureDimension = maxTextureDimension,
                EstimatedTextureBytes = estimatedTextureBytes,
                EstimatedTextureMiB = BytesToMib(estimatedTextureBytes),
                HasMainCamera = GameObject.Find("Main Camera") != null,
                HasRouteRoad = GameObject.Find("MYB89_RouteRoad") != null,
                QualityLevel = QualitySettings.names.Length == 0
                    ? QualitySettings.GetQualityLevel().ToString(CultureInfo.InvariantCulture)
                    : QualitySettings.names[QualitySettings.GetQualityLevel()],
                VSyncCount = QualitySettings.vSyncCount,
                ApplicationTargetFrameRate = Application.targetFrameRate,
                ActiveBuildTarget = EditorUserBuildSettings.activeBuildTarget.ToString(),
                StandaloneOsxBuildSupported = BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Standalone, BuildTarget.StandaloneOSX)
            };

            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.StandaloneOSX)
            {
                softSignals.Add("Active Unity build target is " + measurements.ActiveBuildTarget + "; MYB-51 records this without switching targets to avoid import churn.");
            }

            return measurements;
        }

        private static void CaptureMaterialTextures(Material material, IDictionary<int, TextureInfo> textures)
        {
            string[] texturePropertyNames;
            try
            {
                texturePropertyNames = material.GetTexturePropertyNames();
            }
            catch (Exception)
            {
                return;
            }

            foreach (var propertyName in texturePropertyNames)
            {
                var texture = material.GetTexture(propertyName);
                if (texture == null || textures.ContainsKey(texture.GetInstanceID()))
                {
                    continue;
                }

                textures.Add(texture.GetInstanceID(), new TextureInfo
                {
                    Width = texture.width,
                    Height = texture.height,
                    EstimatedBytes = Math.Max(0L, Profiler.GetRuntimeMemorySizeLong(texture))
                });
            }
        }

        private static int CountMeshFilterTriangles(IEnumerable<MeshFilter> meshFilters)
        {
            var triangles = 0;
            foreach (var filter in meshFilters)
            {
                var renderer = filter.GetComponent<Renderer>();
                if (renderer == null || !renderer.enabled)
                {
                    continue;
                }

                triangles += CountMeshTriangles(filter.sharedMesh);
            }

            return triangles;
        }

        private static int CountSkinnedMeshTriangles(IEnumerable<SkinnedMeshRenderer> renderers)
        {
            var triangles = 0;
            foreach (var renderer in renderers)
            {
                triangles += CountMeshTriangles(renderer.sharedMesh);
            }

            return triangles;
        }

        private static int CountMeshTriangles(Mesh mesh)
        {
            if (mesh == null)
            {
                return 0;
            }

            var triangles = 0;
            for (var submesh = 0; submesh < mesh.subMeshCount; submesh++)
            {
                triangles += (int)(mesh.GetIndexCount(submesh) / 3);
            }

            return triangles;
        }

        private static MetricBudget[] CreateBudgets()
        {
            return new[]
            {
                new MetricBudget
                {
                    Metric = "editor_fps",
                    Unit = "frames_per_second",
                    Target = FpsTarget.ToString(CultureInfo.InvariantCulture) + " fps",
                    Warning = "below " + FpsWarningBelow.ToString(CultureInfo.InvariantCulture) + " fps",
                    Red = "below " + FpsRedBelow.ToString(CultureInfo.InvariantCulture) + " fps",
                    RedBlocking = false
                },
                new MetricBudget
                {
                    Metric = "macos_build_size",
                    Unit = "bytes",
                    Target = "<= " + BuildTargetBytes.ToString(CultureInfo.InvariantCulture),
                    Warning = "> " + BuildWarningBytes.ToString(CultureInfo.InvariantCulture),
                    Red = "> " + BuildRedBytes.ToString(CultureInfo.InvariantCulture),
                    RedBlocking = false
                },
                new MetricBudget
                {
                    Metric = "local_launch_time",
                    Unit = "seconds",
                    Target = "<= " + LaunchTargetSeconds.ToString(CultureInfo.InvariantCulture),
                    Warning = "> " + LaunchWarningSeconds.ToString(CultureInfo.InvariantCulture),
                    Red = "> " + LaunchRedSeconds.ToString(CultureInfo.InvariantCulture),
                    RedBlocking = false
                }
            };
        }

        private static string WorstSoftStatus(params string[] statuses)
        {
            if (statuses.Any(status => string.Equals(status, "red", StringComparison.OrdinalIgnoreCase)))
            {
                return "red";
            }

            if (statuses.Any(status => string.Equals(status, "warning", StringComparison.OrdinalIgnoreCase)))
            {
                return "warning";
            }

            if (statuses.All(status => string.Equals(status, "target", StringComparison.OrdinalIgnoreCase)))
            {
                return "target";
            }

            return "not-measured";
        }

        private static void WriteReports(PerformanceReport report)
        {
            File.WriteAllText(report.JsonReportPath, JsonUtility.ToJson(report, true));

            using (var writer = new StreamWriter(report.TextReportPath, false))
            {
                writer.WriteLine("MYB-51 Unity macOS-first performance baseline");
                writer.WriteLine("Timestamp UTC: " + report.TimestampUtc);
                writer.WriteLine("Unity: " + report.UnityVersion);
                writer.WriteLine("Scene: " + report.ScenePath);
                writer.WriteLine("Status: " + report.Status);
                writer.WriteLine("Overall soft status: " + report.OverallSoftStatus);
                writer.WriteLine("Baseline hardware: " + report.BaselineHardware);
                writer.WriteLine("WebGL scope: " + report.WebGlScope);
                writer.WriteLine();
                writer.WriteLine("Budgets:");
                foreach (var budget in report.Budgets)
                {
                    writer.WriteLine("- " + budget.Metric + ": target " + budget.Target + ", warning " + budget.Warning + ", red " + budget.Red + ", redBlocking=" + budget.RedBlocking);
                }

                writer.WriteLine();
                writer.WriteLine("Measurements:");
                writer.WriteLine("- mode: " + report.Measurements.MeasurementMode);
                writer.WriteLine("- measurement seconds: " + report.Measurements.MeasurementSeconds.ToString("0.000", CultureInfo.InvariantCulture));
                writer.WriteLine("- fps status: " + report.Measurements.FpsStatus + " measured=" + report.Measurements.FpsMeasured);
                writer.WriteLine("- build size status: " + report.Measurements.BuildSizeStatus + " measured=" + report.Measurements.BuildSizeMeasured);
                writer.WriteLine("- launch time status: " + report.Measurements.LaunchTimeStatus + " measured=" + report.Measurements.LaunchTimeMeasured);
                writer.WriteLine("- active renderers: " + report.Measurements.ActiveRendererCount);
                writer.WriteLine("- mesh filters: " + report.Measurements.MeshFilterCount);
                writer.WriteLine("- skinned mesh renderers: " + report.Measurements.SkinnedMeshRendererCount);
                writer.WriteLine("- estimated triangles: " + report.Measurements.EstimatedTriangleCount);
                writer.WriteLine("- active lights: " + report.Measurements.ActiveLightCount);
                writer.WriteLine("- unique materials: " + report.Measurements.UniqueMaterialCount);
                writer.WriteLine("- unique textures: " + report.Measurements.UniqueTextureCount);
                writer.WriteLine("- max texture dimension: " + report.Measurements.MaxTextureDimension);
                writer.WriteLine("- estimated texture MiB: " + report.Measurements.EstimatedTextureMiB.ToString("0.00", CultureInfo.InvariantCulture));
                writer.WriteLine("- quality level: " + report.Measurements.QualityLevel);
                writer.WriteLine("- vSync count: " + report.Measurements.VSyncCount);
                writer.WriteLine("- application target frame rate: " + report.Measurements.ApplicationTargetFrameRate);
                writer.WriteLine("- active build target: " + report.Measurements.ActiveBuildTarget);
                writer.WriteLine("- standalone macOS build supported: " + report.Measurements.StandaloneOsxBuildSupported);

                WriteSection(writer, "Hard failures", report.HardFailures);
                WriteSection(writer, "Soft signals", report.SoftSignals);
                WriteSection(writer, "Notes", report.Notes);
            }
        }

        private static void WriteSection(TextWriter writer, string title, IReadOnlyCollection<string> values)
        {
            if (values.Count == 0)
            {
                return;
            }

            writer.WriteLine();
            writer.WriteLine(title + ":");
            foreach (var value in values)
            {
                writer.WriteLine("- " + value);
            }
        }

        private static double BytesToMib(long bytes)
        {
            return bytes / 1024d / 1024d;
        }

        private static string ProjectRelativeToAbsolute(string assetPath)
        {
            var projectRoot = Directory.GetParent(Application.dataPath);
            return Path.Combine(projectRoot == null ? Application.dataPath : projectRoot.FullName, assetPath);
        }

        private static string GetRepoRoot()
        {
            var projectRoot = Directory.GetParent(Application.dataPath);
            if (projectRoot == null || projectRoot.Parent == null || projectRoot.Parent.Parent == null)
            {
                return projectRoot == null ? Application.dataPath : projectRoot.FullName;
            }

            return projectRoot.Parent.Parent.FullName;
        }

        private sealed class TextureInfo
        {
            public int Width;
            public int Height;
            public long EstimatedBytes;
        }

        [Serializable]
        private sealed class PerformanceReport
        {
            public string Ticket = string.Empty;
            public string Title = string.Empty;
            public string TimestampUtc = string.Empty;
            public string UnityVersion = string.Empty;
            public string ProjectPath = string.Empty;
            public string ScenePath = string.Empty;
            public string ActiveScenePath = string.Empty;
            public string BaselineHardware = string.Empty;
            public string BaselineUnity = string.Empty;
            public string WebGlScope = string.Empty;
            public string Status = string.Empty;
            public string OverallSoftStatus = string.Empty;
            public string JsonReportPath = string.Empty;
            public string TextReportPath = string.Empty;
            public MetricBudget[] Budgets = Array.Empty<MetricBudget>();
            public PerformanceMeasurements Measurements = new PerformanceMeasurements();
            public string[] HardFailures = Array.Empty<string>();
            public string[] SoftSignals = Array.Empty<string>();
            public string[] Notes = Array.Empty<string>();
        }

        [Serializable]
        private sealed class MetricBudget
        {
            public string Metric = string.Empty;
            public string Unit = string.Empty;
            public string Target = string.Empty;
            public string Warning = string.Empty;
            public string Red = string.Empty;
            public bool RedBlocking;
        }

        [Serializable]
        private sealed class PerformanceMeasurements
        {
            public string MeasurementMode = string.Empty;
            public double MeasurementSeconds;
            public bool FpsMeasured;
            public float FpsAverage;
            public string FpsStatus = string.Empty;
            public bool BuildSizeMeasured;
            public long BuildSizeBytes;
            public string BuildSizeStatus = string.Empty;
            public bool LaunchTimeMeasured;
            public float LaunchTimeSeconds;
            public string LaunchTimeStatus = string.Empty;
            public int ActiveRendererCount;
            public int MeshFilterCount;
            public int SkinnedMeshRendererCount;
            public int EstimatedTriangleCount;
            public int ActiveLightCount;
            public int UniqueMaterialCount;
            public int UniqueTextureCount;
            public int MaxTextureDimension;
            public long EstimatedTextureBytes;
            public double EstimatedTextureMiB;
            public bool HasMainCamera;
            public bool HasRouteRoad;
            public string QualityLevel = string.Empty;
            public int VSyncCount;
            public int ApplicationTargetFrameRate;
            public string ActiveBuildTarget = string.Empty;
            public bool StandaloneOsxBuildSupported;
        }
    }
}

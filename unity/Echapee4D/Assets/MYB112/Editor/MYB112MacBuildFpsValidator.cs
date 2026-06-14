using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using MYB112;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;

namespace MYB112.Editor
{
    public static class MYB112MacBuildFpsValidator
    {
        private const string CanonicalScenePath = "Assets/Scenes/MYB89UnityMcpProbe.unity";
        private const string BuildRootRelative = "_bmad-output/unity-macos-builds/myb-112";
        private const string ResultsRootRelative = "_bmad-output/unity-test-results/myb-112";
        private const string SummaryReportRelative = ResultsRootRelative + "/myb-112-build-fps-comparison.txt";
        private const int TargetFps = 60;
        private const int WarningBelowFps = 45;
        private const int RedBelowFps = 30;

        [MenuItem("Tools/MYB-112/Build macOS FPS Comparison")]
        public static void BuildMacOsFpsComparisonFromMenu()
        {
            Debug.Log(BuildAndValidateCli());
        }

        public static string BuildAndValidateCli()
        {
            var reportPath = BuildAndValidate();
            Debug.Log("MYB-112 macOS build FPS comparison validated: " + reportPath);
            return reportPath;
        }

        public static string BuildAndValidate()
        {
            var reports = new List<RunReport>();
            var previousProductName = PlayerSettings.productName;
            try
            {
                MYB112PremiumTreeRuntimeSet.BuildAndValidate();

                reports.Add(BuildRunAndMeasure("baseline", useLegacyBaseline: true));
                reports.Add(BuildRunAndMeasure("after", useLegacyBaseline: false));

                MYB112PremiumTreeRuntimeSet.UseLegacyBaselineComparison = false;
                MYB106.Editor.MYB106Passage01LookDev.ApplyAndValidate();
                RemoveProbeFromScene();
                EditorSceneManager.SaveScene(SceneManager.GetActiveScene());

                var summaryPath = WriteSummary(reports);
                if (reports.Any(report => report.Status.Equals("red", StringComparison.OrdinalIgnoreCase)))
                {
                    throw new InvalidOperationException("MYB-112 FPS comparison has red status. See " + summaryPath);
                }

                return summaryPath;
            }
            finally
            {
                MYB112PremiumTreeRuntimeSet.UseLegacyBaselineComparison = false;
                PlayerSettings.productName = previousProductName;
            }
        }

        private static RunReport BuildRunAndMeasure(string label, bool useLegacyBaseline)
        {
            MYB112PremiumTreeRuntimeSet.UseLegacyBaselineComparison = useLegacyBaseline;
            MYB106.Editor.MYB106Passage01LookDev.ApplyAndValidate();
            AddProbeToScene(label);
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            PlayerSettings.productName = "Echappee MYB112 " + label;
            EnsureMacStandaloneTarget();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            WaitForEditorIdle("before macOS build " + label);

            var repoRoot = GetRepoRoot();
            var buildDirectory = Path.Combine(repoRoot, BuildRootRelative, label);
            var resultsDirectory = Path.Combine(repoRoot, ResultsRootRelative);
            if (Directory.Exists(buildDirectory))
            {
                Directory.Delete(buildDirectory, true);
            }

            Directory.CreateDirectory(buildDirectory);
            Directory.CreateDirectory(resultsDirectory);

            var appPath = Path.Combine(buildDirectory, "EchappeeMYB112_" + label + ".app");
            var buildReport = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { CanonicalScenePath },
                locationPathName = appPath,
                target = BuildTarget.StandaloneOSX,
                options = BuildOptions.None
            });

            if (buildReport.summary.result != BuildResult.Succeeded)
            {
                throw new InvalidOperationException("MYB-112 macOS build failed for " + label + ": " + buildReport.summary.result);
            }

            WaitForEditorIdle("after macOS build " + label);
            ValidateBuiltApp(appPath, label);

            var fpsReportPath = Path.Combine(resultsDirectory, "myb-112-runtime-fps-" + label + ".txt");
            if (File.Exists(fpsReportPath))
            {
                File.Delete(fpsReportPath);
            }

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "/usr/bin/open",
                    Arguments = "-n " + Quote(appPath)
                        + " --args -screen-width 1280 -screen-height 720 -screen-fullscreen 0 -myb112FpsReport "
                        + Quote(fpsReportPath)
                        + " -myb112FpsLabel " + label,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            if (!process.WaitForExit(10000))
            {
                try
                {
                    process.Kill();
                }
                catch (Exception)
                {
                    // Best effort cleanup for a local validation process.
                }

                throw new TimeoutException("MYB-112 macOS app launcher timed out for " + label);
            }

            if (process.ExitCode != 0)
            {
                throw new InvalidOperationException("MYB-112 macOS app launcher exited unexpectedly for " + label + " with code " + process.ExitCode.ToString(CultureInfo.InvariantCulture));
            }

            WaitForFpsReport(fpsReportPath, label);
            return ParseRunReport(label, fpsReportPath, buildReport.summary.totalSize);
        }

        private static void WaitForFpsReport(string fpsReportPath, string label)
        {
            var deadline = DateTime.UtcNow.AddSeconds(60);
            while (!File.Exists(fpsReportPath))
            {
                if (DateTime.UtcNow > deadline)
                {
                    throw new TimeoutException("MYB-112 runtime FPS probe timed out for " + label + ".");
                }

                System.Threading.Thread.Sleep(500);
            }

            var firstSize = new FileInfo(fpsReportPath).Length;
            System.Threading.Thread.Sleep(500);
            var secondSize = new FileInfo(fpsReportPath).Length;
            if (firstSize <= 0L || firstSize != secondSize)
            {
                throw new InvalidOperationException("MYB-112 FPS report is not stable for " + label + ": " + fpsReportPath);
            }
        }

        private static void WaitForEditorIdle(string phase)
        {
            var deadline = DateTime.UtcNow.AddMinutes(3);
            while (EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                if (DateTime.UtcNow > deadline)
                {
                    throw new TimeoutException("Unity did not become idle " + phase + ".");
                }

                System.Threading.Thread.Sleep(500);
            }
        }

        private static void EnsureMacStandaloneTarget()
        {
            if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneOSX)
            {
                return;
            }

            if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Standalone, BuildTarget.StandaloneOSX))
            {
                throw new InvalidOperationException("MYB-112 requires a supported macOS Standalone build target for FPS validation.");
            }

            if (!EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneOSX))
            {
                throw new InvalidOperationException("MYB-112 could not switch Unity to StandaloneOSX before building the FPS validation app.");
            }
        }

        private static void ValidateBuiltApp(string appPath, string label)
        {
            var dataDirectory = Path.Combine(appPath, "Contents", "Resources", "Data");
            var levelPath = Path.Combine(dataDirectory, "level0");
            if (!Directory.Exists(appPath))
            {
                throw new DirectoryNotFoundException("MYB-112 macOS app was not produced for " + label + ": " + appPath);
            }

            if (!File.Exists(levelPath))
            {
                throw new FileNotFoundException("MYB-112 build scene data is missing for " + label, levelPath);
            }

            var firstSize = new FileInfo(levelPath).Length;
            System.Threading.Thread.Sleep(1000);
            var secondSize = new FileInfo(levelPath).Length;
            if (firstSize <= 0L || firstSize != secondSize)
            {
                throw new InvalidOperationException("MYB-112 build scene data is not stable for " + label + ": " + firstSize.ToString(CultureInfo.InvariantCulture) + " -> " + secondSize.ToString(CultureInfo.InvariantCulture));
            }
        }

        private static void AddProbeToScene(string label)
        {
            RemoveProbeFromScene();
            var probe = new GameObject("MYB112_RuntimeFpsProbe");
            var component = probe.AddComponent<MYB112RuntimeFpsProbe>();
            component.label = label;
            component.width = 1280;
            component.height = 720;
            component.targetFps = TargetFps;
            component.warningBelowFps = WarningBelowFps;
            component.redBelowFps = RedBelowFps;
            component.startMeters = 18f;
            component.endMeters = 72f;
            component.warmupSeconds = 2f;
            component.measurementSeconds = 12f;
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }

        private static void RemoveProbeFromScene()
        {
            while (true)
            {
                var probe = GameObject.Find("MYB112_RuntimeFpsProbe");
                if (probe == null)
                {
                    return;
                }

                UnityEngine.Object.DestroyImmediate(probe);
                EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            }
        }

        private static RunReport ParseRunReport(string label, string path, ulong buildSizeBytes)
        {
            var lines = File.ReadAllLines(path);
            return new RunReport(
                label,
                path,
                buildSizeBytes,
                ValueFor(lines, "Status"),
                FloatFor(lines, "Average FPS"),
                FloatFor(lines, "1 percent low FPS"),
                FloatFor(lines, "Minimum FPS"));
        }

        private static string WriteSummary(IReadOnlyList<RunReport> reports)
        {
            var repoRoot = GetRepoRoot();
            var path = Path.Combine(repoRoot, SummaryReportRelative);
            Directory.CreateDirectory(Path.GetDirectoryName(path) ?? repoRoot);

            var after = reports.FirstOrDefault(report => report.Label == "after");
            var baseline = reports.FirstOrDefault(report => report.Label == "baseline");
            var delta = after != null && baseline != null ? after.AverageFps - baseline.AverageFps : 0f;
            var lines = new List<string>
            {
                "MYB-112 macOS build FPS comparison",
                "Generated UTC: " + DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture),
                "Scene: " + CanonicalScenePath,
                "Resolution: 1280x720",
                "Mode: compiled macOS build; Unity Editor is pre-check only.",
                "Video FPS: not used for performance validation.",
                "Budget: target 60 fps, warning below 45 fps, red blocking below 30 fps.",
                "Average FPS delta after-baseline: " + delta.ToString("0.0", CultureInfo.InvariantCulture),
                string.Empty,
                "Runs:"
            };

            foreach (var report in reports)
            {
                lines.Add("- " + report.Label + ": status " + report.Status
                    + ", avg " + report.AverageFps.ToString("0.0", CultureInfo.InvariantCulture)
                    + ", 1% low " + report.OnePercentLowFps.ToString("0.0", CultureInfo.InvariantCulture)
                    + ", min " + report.MinFps.ToString("0.0", CultureInfo.InvariantCulture)
                    + ", build bytes " + report.BuildSizeBytes.ToString(CultureInfo.InvariantCulture)
                    + ", report `" + RelativeToRepo(report.ReportPath) + "`");
            }

            File.WriteAllLines(path, lines);
            return path;
        }

        private static string ValueFor(IEnumerable<string> lines, string key)
        {
            var prefix = key + ": ";
            return lines.FirstOrDefault(line => line.StartsWith(prefix, StringComparison.Ordinal))?.Substring(prefix.Length) ?? string.Empty;
        }

        private static float FloatFor(IEnumerable<string> lines, string key)
        {
            return float.TryParse(ValueFor(lines, key), NumberStyles.Float, CultureInfo.InvariantCulture, out var value) ? value : 0f;
        }

        private static string Quote(string value)
        {
            return "\"" + value.Replace("\"", "\\\"") + "\"";
        }

        private static string RelativeToRepo(string path)
        {
            var repoRoot = GetRepoRoot();
            return Path.GetRelativePath(repoRoot, path).Replace('\\', '/');
        }

        private static string GetRepoRoot()
        {
            var directory = new DirectoryInfo(Application.dataPath);
            while (directory != null && !Directory.Exists(Path.Combine(directory.FullName, ".git")))
            {
                directory = directory.Parent;
            }

            return directory?.FullName ?? Directory.GetCurrentDirectory();
        }

        private sealed class RunReport
        {
            public RunReport(string label, string reportPath, ulong buildSizeBytes, string status, float averageFps, float onePercentLowFps, float minFps)
            {
                Label = label;
                ReportPath = reportPath;
                BuildSizeBytes = buildSizeBytes;
                Status = status;
                AverageFps = averageFps;
                OnePercentLowFps = onePercentLowFps;
                MinFps = minFps;
            }

            public string Label { get; }
            public string ReportPath { get; }
            public ulong BuildSizeBytes { get; }
            public string Status { get; }
            public float AverageFps { get; }
            public float OnePercentLowFps { get; }
            public float MinFps { get; }
        }
    }
}

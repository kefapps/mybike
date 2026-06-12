using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MYB89;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MYB91.Editor
{
    public static class MYB91CanonicalBaselineValidator
    {
        private const string ScenePath = "Assets/Scenes/MYB89UnityMcpProbe.unity";
        private const string ReportRelative = "_bmad-output/unity-test-results/myb-91-canonical-baseline.txt";
        private const string WebGLBuildReportRelative = "_bmad-output/unity-test-results/myb-90-unity-webgl-build.txt";

        [MenuItem("Tools/MYB-91/Validate Canonical Unity Baseline")]
        public static string ValidateCanonicalBaseline()
        {
            var failures = new List<string>();
            var notes = new List<string>();
            var report = new BaselineReport
            {
                UnityVersion = Application.unityVersion,
                ProjectPath = Directory.GetParent(Application.dataPath)?.FullName ?? string.Empty,
                ScenePath = ScenePath,
                WebGLBuilderType = typeof(MYB90.Editor.MYB90WebGLReadinessBuilder).FullName ?? string.Empty
            };

            ValidateVersionedRoots(failures, notes, report);
            ValidatePackages(failures, report);
            ValidateBuildSettings(failures, report);
            ValidateScene(failures, report);
            ValidateWebGLReadiness(failures, notes, report);

            var reportPath = WriteReport(failures, notes, report);
            if (failures.Count > 0)
            {
                throw new InvalidOperationException("MYB-91 canonical baseline validation failed. See " + reportPath);
            }

            Debug.Log("MYB-91 canonical baseline validation passed. Report: " + reportPath);
            return reportPath;
        }

        public static void ValidateCanonicalBaselineCli()
        {
            try
            {
                ValidateCanonicalBaseline();
                if (Application.isBatchMode)
                {
                    EditorApplication.Exit(0);
                }
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                if (Application.isBatchMode)
                {
                    EditorApplication.Exit(1);
                }

                throw;
            }
        }

        private static void ValidateVersionedRoots(ICollection<string> failures, ICollection<string> notes, BaselineReport report)
        {
            var projectRoot = Directory.GetParent(Application.dataPath)?.FullName ?? string.Empty;
            foreach (var requiredRoot in new[] { "Assets", "Packages", "ProjectSettings" })
            {
                var absolutePath = Path.Combine(projectRoot, requiredRoot);
                if (!Directory.Exists(absolutePath))
                {
                    failures.Add("Missing versioned Unity root: " + requiredRoot);
                }
            }

            var generatedRoots = new[] { "Library", "Temp", "Logs", "UserSettings", "Obj", "Build", "Builds", "MemoryCaptures", "Recordings" };
            report.LocalGeneratedRoots = generatedRoots
                .Where(root => Directory.Exists(Path.Combine(projectRoot, root)))
                .OrderBy(root => root, StringComparer.Ordinal)
                .ToArray();

            if (report.LocalGeneratedRoots.Length > 0)
            {
                notes.Add("Generated Unity roots exist locally and must remain ignored by git: " + string.Join(", ", report.LocalGeneratedRoots));
            }
        }

        private static void ValidatePackages(ICollection<string> failures, BaselineReport report)
        {
            report.UnityMcpPackageVersion = PackageVersion("com.ivanmurzak.unity.mcp");
            report.UrpPackageVersion = PackageVersion("com.unity.render-pipelines.universal");

            if (string.IsNullOrEmpty(report.UnityMcpPackageVersion))
            {
                failures.Add("Missing package com.ivanmurzak.unity.mcp.");
            }

            if (string.IsNullOrEmpty(report.UrpPackageVersion))
            {
                failures.Add("Missing package com.unity.render-pipelines.universal.");
            }
        }

        private static string PackageVersion(string packageName)
        {
            var info = UnityEditor.PackageManager.PackageInfo.FindForPackageName(packageName);
            return info == null ? string.Empty : info.version;
        }

        private static void ValidateBuildSettings(ICollection<string> failures, BaselineReport report)
        {
            report.EnabledBuildScenes = EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .Select(scene => scene.path)
                .ToArray();

            if (report.EnabledBuildScenes.Length == 0)
            {
                failures.Add("Build Settings has no enabled scene.");
                return;
            }

            if (report.EnabledBuildScenes[0] != ScenePath)
            {
                failures.Add("First enabled Build Settings scene must be " + ScenePath + " but was " + report.EnabledBuildScenes[0] + ".");
            }

            if (!report.EnabledBuildScenes.Contains(ScenePath))
            {
                failures.Add("Canonical scene is not enabled in Build Settings: " + ScenePath);
            }
        }

        private static void ValidateScene(ICollection<string> failures, BaselineReport report)
        {
            var absoluteScenePath = ProjectRelativeToAbsolute(ScenePath);
            if (!File.Exists(absoluteScenePath))
            {
                failures.Add("Missing canonical scene asset: " + ScenePath);
                return;
            }

            if (SceneManager.GetActiveScene().path != ScenePath)
            {
                EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            }

            report.ActiveScenePath = SceneManager.GetActiveScene().path;
            if (report.ActiveScenePath != ScenePath)
            {
                failures.Add("Active scene after open is not canonical scene: " + report.ActiveScenePath);
            }

            var ride = UnityEngine.Object.FindAnyObjectByType<MYB89ProbeRide>();
            var camera = Camera.main;
            var road = GameObject.Find("MYB89_RouteRoad");
            var hud = GameObject.Find("MYB89_HUD");
            var renderers = UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsInactive.Exclude);

            report.RendererCount = renderers.Length;
            report.HasMainCamera = camera != null;
            report.HasRoad = road != null;
            report.HasHud = hud != null;

            if (ride == null)
            {
                failures.Add("Missing MYB89ProbeRide component.");
                return;
            }

            ride.RebuildRouteCache();
            report.RouteMarkerCount = ride.routeMarkers == null ? 0 : ride.routeMarkers.Count(marker => marker != null);
            report.RouteLengthMeters = ride.RouteLength;
            report.SpeedMetersPerSecond = ride.speedMetersPerSecond;
            ride.SetPreviewProgress(ride.RouteLength * 0.42f);
            report.HudLabelsWired = ride.distanceLabel != null
                && ride.speedLabel != null
                && ride.difficultyLabel != null
                && ride.gradeLabel != null
                && ride.segmentLabel != null
                && ride.verdictLabel != null;
            report.HudDifficultyText = ride.difficultyLabel == null ? string.Empty : ride.difficultyLabel.text;
            report.HudGradeText = ride.gradeLabel == null ? string.Empty : ride.gradeLabel.text;
            report.HudSegmentText = ride.segmentLabel == null ? string.Empty : ride.segmentLabel.text;

            if (report.RouteMarkerCount < 8)
            {
                failures.Add("Route needs at least 8 route markers.");
            }

            if (report.RouteLengthMeters < 200f)
            {
                failures.Add("Route length is too short: " + report.RouteLengthMeters.ToString("0.0") + " m.");
            }

            if (report.SpeedMetersPerSecond <= 0f)
            {
                failures.Add("Probe ride speed must be positive.");
            }

            if (!report.HudLabelsWired)
            {
                failures.Add("HUD labels are not wired to MYB89ProbeRide.");
            }

            if (!report.HudDifficultyText.Contains("Effort:"))
            {
                failures.Add("HUD difficulty label is missing the effort readout.");
            }

            if (!report.HudGradeText.Contains("Pente:"))
            {
                failures.Add("HUD grade label is missing the slope readout.");
            }

            if (!report.HudSegmentText.Contains("Segment: Climb"))
            {
                failures.Add("HUD segment label does not report the current climb segment.");
            }

            if (!report.HasMainCamera)
            {
                failures.Add("Missing Main Camera.");
            }

            if (!report.HasRoad)
            {
                failures.Add("Missing MYB89_RouteRoad mesh.");
            }

            if (!report.HasHud)
            {
                failures.Add("Missing MYB89_HUD canvas.");
            }

            if (report.RendererCount < 55)
            {
                failures.Add("Scene has too few renderers for readable motion: " + report.RendererCount + ".");
            }
        }

        private static void ValidateWebGLReadiness(ICollection<string> failures, ICollection<string> notes, BaselineReport report)
        {
            report.WebGLSupported = BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.WebGL, BuildTarget.WebGL);
            if (!report.WebGLSupported)
            {
                failures.Add("WebGL build target is not supported by this Unity installation.");
            }

            var webGlBuildReport = Path.Combine(GetRepoRoot(), WebGLBuildReportRelative);
            report.PreviousWebGLBuildReportExists = File.Exists(webGlBuildReport);
            if (!report.PreviousWebGLBuildReportExists)
            {
                notes.Add("Previous MYB-90 WebGL build report was not found; rebuild with Tools/MYB-90/Build WebGL Readiness Probe.");
                return;
            }

            report.PreviousWebGLBuildStatus = ReadWebGLBuildStatus(webGlBuildReport);
            if (report.PreviousWebGLBuildStatus != "Succeeded")
            {
                failures.Add("Previous MYB-90 WebGL build report is not successful: " + EmptyAsMissing(report.PreviousWebGLBuildStatus) + ".");
            }
        }

        private static string ReadWebGLBuildStatus(string reportPath)
        {
            foreach (var line in File.ReadLines(reportPath))
            {
                if (line.StartsWith("Status: ", StringComparison.Ordinal))
                {
                    return line.Substring("Status: ".Length).Trim();
                }
            }

            return string.Empty;
        }

        private static string WriteReport(IReadOnlyList<string> failures, IReadOnlyList<string> notes, BaselineReport report)
        {
            var repoRoot = GetRepoRoot();
            var reportPath = Path.Combine(repoRoot, ReportRelative);
            Directory.CreateDirectory(Path.GetDirectoryName(reportPath) ?? repoRoot);

            using (var writer = new StreamWriter(reportPath, false))
            {
                writer.WriteLine("MYB-91 Unity canonical baseline validation");
                writer.WriteLine("Timestamp UTC: " + DateTime.UtcNow.ToString("O"));
                writer.WriteLine("Status: " + (failures.Count == 0 ? "PASS" : "FAIL"));
                writer.WriteLine("Unity: " + report.UnityVersion);
                writer.WriteLine("Project: " + report.ProjectPath);
                writer.WriteLine("Scene: " + report.ScenePath);
                writer.WriteLine("Active scene: " + report.ActiveScenePath);
                writer.WriteLine("Build settings scenes: " + string.Join(", ", report.EnabledBuildScenes));
                writer.WriteLine("Unity-MCP package: " + EmptyAsMissing(report.UnityMcpPackageVersion));
                writer.WriteLine("URP package: " + EmptyAsMissing(report.UrpPackageVersion));
                writer.WriteLine("WebGL supported: " + report.WebGLSupported);
                writer.WriteLine("WebGL builder: " + report.WebGLBuilderType);
                writer.WriteLine("Previous MYB-90 WebGL report exists: " + report.PreviousWebGLBuildReportExists);
                writer.WriteLine("Previous MYB-90 WebGL build status: " + EmptyAsMissing(report.PreviousWebGLBuildStatus));
                writer.WriteLine("Route markers: " + report.RouteMarkerCount);
                writer.WriteLine("Route length: " + report.RouteLengthMeters.ToString("0.0") + " m");
                writer.WriteLine("Speed: " + report.SpeedMetersPerSecond.ToString("0.0") + " m/s");
                writer.WriteLine("Renderer count: " + report.RendererCount);
                writer.WriteLine("Main camera: " + report.HasMainCamera);
                writer.WriteLine("Road mesh: " + report.HasRoad);
                writer.WriteLine("HUD canvas: " + report.HasHud);
                writer.WriteLine("HUD labels wired: " + report.HudLabelsWired);
                writer.WriteLine("HUD difficulty: " + EmptyAsMissing(report.HudDifficultyText));
                writer.WriteLine("HUD grade: " + EmptyAsMissing(report.HudGradeText));
                writer.WriteLine("HUD segment: " + EmptyAsMissing(report.HudSegmentText));
                writer.WriteLine("Local generated roots present: " + (report.LocalGeneratedRoots.Length == 0 ? "none" : string.Join(", ", report.LocalGeneratedRoots)));

                if (notes.Count > 0)
                {
                    writer.WriteLine();
                    writer.WriteLine("Notes:");
                    foreach (var note in notes)
                    {
                        writer.WriteLine("- " + note);
                    }
                }

                if (failures.Count > 0)
                {
                    writer.WriteLine();
                    writer.WriteLine("Failures:");
                    foreach (var failure in failures)
                    {
                        writer.WriteLine("- " + failure);
                    }
                }
            }

            return reportPath;
        }

        private static string EmptyAsMissing(string value)
        {
            return string.IsNullOrEmpty(value) ? "missing" : value;
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

        private sealed class BaselineReport
        {
            public string UnityVersion = string.Empty;
            public string ProjectPath = string.Empty;
            public string ScenePath = string.Empty;
            public string ActiveScenePath = string.Empty;
            public string[] EnabledBuildScenes = Array.Empty<string>();
            public string UnityMcpPackageVersion = string.Empty;
            public string UrpPackageVersion = string.Empty;
            public bool WebGLSupported;
            public string WebGLBuilderType = string.Empty;
            public bool PreviousWebGLBuildReportExists;
            public string PreviousWebGLBuildStatus = string.Empty;
            public int RouteMarkerCount;
            public float RouteLengthMeters;
            public float SpeedMetersPerSecond;
            public int RendererCount;
            public bool HasMainCamera;
            public bool HasRoad;
            public bool HasHud;
            public bool HudLabelsWired;
            public string HudDifficultyText = string.Empty;
            public string HudGradeText = string.Empty;
            public string HudSegmentText = string.Empty;
            public string[] LocalGeneratedRoots = Array.Empty<string>();
        }
    }
}

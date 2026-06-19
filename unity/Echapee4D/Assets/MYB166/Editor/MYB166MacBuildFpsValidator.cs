using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using MYB89;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;

public static class MYB166MacBuildFpsValidator
{
    private const string ScenePath = "Assets/Scenes/MYB89UnityMcpProbe.unity";
    private const string BuildRootRelative = "_bmad-output/unity-macos-builds/MYB-166";
    private const string ResultsRootRelative = "_bmad-output/unity-test-results/MYB-166";
    private const string SummaryReportRelative = ResultsRootRelative + "/myb-166-macos-build-fps-summary.md";
    private const int TargetFps = 60;
    private const int WarningBelowFps = 45;
    private const int RedBelowFps = 30;

    [MenuItem("Tools/MyBike/MYB-166/Build macOS FPS Validation")]
    public static void RunFromMenu()
    {
        Debug.Log(RunBuildFpsValidation());
    }

    public static string RunBuildFpsValidation()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            throw new InvalidOperationException("Stop Play Mode before running MYB-166 build FPS validation.");
        }

        Directory.CreateDirectory(ToRepoPath(BuildRootRelative));
        Directory.CreateDirectory(ToRepoPath(ResultsRootRelative));
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        var routeLength = CurrentRouteLengthMeters();
        var appPath = BuildCurrentScene();

        var reports = new[]
        {
            RunScenario(appPath, "route-camera-worst-case-slice", 0f, Mathf.Min(routeLength, 320f), 2f, 45f),
            RunScenario(appPath, "full-route-3min-validation", 0f, routeLength, 2f, Mathf.Clamp(routeLength / 12.5f, 120f, 240f))
        };

        var summaryPath = WriteSummary(routeLength, appPath, reports);
        if (reports.Any(report => report.Status.Equals("red", StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException("MYB-166 macOS FPS validation has red status. See " + summaryPath);
        }

        Debug.Log("MYB-166 macOS build FPS validation wrote " + summaryPath);
        return RelativeToRepo(summaryPath);
    }

    private static string BuildCurrentScene()
    {
        EnsureMacStandaloneTarget();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        WaitForEditorIdle("before MYB-166 macOS build");

        var buildDirectory = ToRepoPath(BuildRootRelative);
        if (Directory.Exists(buildDirectory))
        {
            Directory.Delete(buildDirectory, true);
        }

        Directory.CreateDirectory(buildDirectory);
        var appPath = Path.Combine(buildDirectory, "EchappeeMYB166.app");
        var buildReport = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = new[] { ScenePath },
            locationPathName = appPath,
            target = BuildTarget.StandaloneOSX,
            options = BuildOptions.None
        });

        if (buildReport.summary.result != BuildResult.Succeeded)
        {
            throw new InvalidOperationException("MYB-166 macOS build failed: " + buildReport.summary.result);
        }

        WaitForEditorIdle("after MYB-166 macOS build");
        ValidateBuiltApp(appPath);
        return appPath;
    }

    private static RunReport RunScenario(string appPath, string label, float startMeters, float endMeters, float warmupSeconds, float measurementSeconds)
    {
        var reportPath = ToRepoPath(ResultsRootRelative + "/myb-166-runtime-fps-" + label + ".txt");
        if (File.Exists(reportPath))
        {
            File.Delete(reportPath);
        }

        var args = "-n " + Quote(appPath)
            + " --args -screen-width 1280 -screen-height 720 -screen-fullscreen 0"
            + " -myb166FpsReport " + Quote(reportPath)
            + " -myb166FpsLabel " + label
            + " -myb166StartMeters " + Format(startMeters)
            + " -myb166EndMeters " + Format(endMeters)
            + " -myb166WarmupSeconds " + Format(warmupSeconds)
            + " -myb166MeasurementSeconds " + Format(measurementSeconds)
            + " -myb166Width 1280 -myb166Height 720 -myb166TargetFps " + TargetFps.ToString(CultureInfo.InvariantCulture);

        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "/usr/bin/open",
                Arguments = args,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.Start();
        if (!process.WaitForExit(10000))
        {
            TryKill(process);
            throw new TimeoutException("MYB-166 macOS app launcher timed out for " + label);
        }

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException("MYB-166 macOS app launcher exited with " + process.ExitCode.ToString(CultureInfo.InvariantCulture) + " for " + label);
        }

        WaitForFpsReport(reportPath, label, measurementSeconds + warmupSeconds + 90f);
        return ParseRunReport(label, reportPath);
    }

    private static float CurrentRouteLengthMeters()
    {
        var markerRoot = GameObject.Find("MYB89_RouteMarkers");
        if (markerRoot == null)
        {
            throw new InvalidOperationException("Missing MYB89_RouteMarkers in " + ScenePath);
        }

        var markers = markerRoot
            .GetComponentsInChildren<Transform>(true)
            .Where(transform => transform != markerRoot.transform && transform.name.StartsWith("RouteMarker_", StringComparison.Ordinal))
            .OrderBy(transform => transform.name, StringComparer.Ordinal)
            .ToArray();
        var route = MYB89RideTrajectory.BuildSmoothedPoints(markers, MYB89RideTrajectory.DefaultSamplesPerSegment);
        return MYB89RideTrajectory.Length(route);
    }

    private static void WaitForFpsReport(string fpsReportPath, string label, float timeoutSeconds)
    {
        var deadline = DateTime.UtcNow.AddSeconds(Mathf.Max(30f, timeoutSeconds));
        while (!File.Exists(fpsReportPath))
        {
            if (DateTime.UtcNow > deadline)
            {
                throw new TimeoutException("MYB-166 runtime FPS probe timed out for " + label + ".");
            }

            System.Threading.Thread.Sleep(500);
        }

        var firstSize = new FileInfo(fpsReportPath).Length;
        System.Threading.Thread.Sleep(500);
        var secondSize = new FileInfo(fpsReportPath).Length;
        if (firstSize <= 0L || firstSize != secondSize)
        {
            throw new InvalidOperationException("MYB-166 FPS report is not stable for " + label + ": " + fpsReportPath);
        }
    }

    private static RunReport ParseRunReport(string label, string path)
    {
        var lines = File.ReadAllLines(path);
        return new RunReport(
            label,
            path,
            ValueFor(lines, "Status"),
            FloatFor(lines, "Average FPS"),
            FloatFor(lines, "1 percent low FPS"),
            FloatFor(lines, "Minimum FPS"),
            ValueFor(lines, "Sequence meters"));
    }

    private static string WriteSummary(float routeLength, string appPath, IReadOnlyList<RunReport> reports)
    {
        var path = ToRepoPath(SummaryReportRelative);
        Directory.CreateDirectory(Path.GetDirectoryName(path) ?? GetRepoRoot());
        var lines = new List<string>
        {
            "# MYB-166 macOS Build FPS Summary",
            string.Empty,
            "- Scene: `" + ScenePath + "`",
            "- App: `" + RelativeToRepo(appPath) + "`",
            "- Route length: `" + Format(routeLength) + "m`",
            "- Mode: compiled macOS build; Editor Play Mode/MCP is diagnostic only.",
            "- Budget: target 60 FPS, warning below 45 FPS, red below 30 FPS.",
            string.Empty,
            "## Runs",
            string.Empty
        };

        foreach (var report in reports)
        {
            lines.Add("- `" + report.Label + "`: status `" + report.Status
                + "`, avg `" + Format(report.AverageFps)
                + "`, 1% low `" + Format(report.OnePercentLowFps)
                + "`, min `" + Format(report.MinFps)
                + "`, meters `" + report.SequenceMeters
                + "`, report `" + RelativeToRepo(report.ReportPath) + "`");
        }

        File.WriteAllLines(path, lines);
        return path;
    }

    private static void EnsureMacStandaloneTarget()
    {
        if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneOSX)
        {
            return;
        }

        if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Standalone, BuildTarget.StandaloneOSX)
            || !EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneOSX))
        {
            throw new InvalidOperationException("MYB-166 could not switch Unity to StandaloneOSX.");
        }
    }

    private static void ValidateBuiltApp(string appPath)
    {
        var levelPath = Path.Combine(appPath, "Contents", "Resources", "Data", "level0");
        if (!Directory.Exists(appPath))
        {
            throw new DirectoryNotFoundException("MYB-166 macOS app was not produced: " + appPath);
        }

        if (!File.Exists(levelPath))
        {
            throw new FileNotFoundException("MYB-166 build scene data is missing.", levelPath);
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

    private static string ValueFor(IEnumerable<string> lines, string key)
    {
        var prefix = key + ": ";
        return lines.FirstOrDefault(line => line.StartsWith(prefix, StringComparison.Ordinal))?.Substring(prefix.Length) ?? string.Empty;
    }

    private static float FloatFor(IEnumerable<string> lines, string key)
    {
        return float.TryParse(ValueFor(lines, key), NumberStyles.Float, CultureInfo.InvariantCulture, out var value) ? value : 0f;
    }

    private static void TryKill(Process process)
    {
        try
        {
            process.Kill();
        }
        catch (Exception)
        {
            // Best effort cleanup for a local validation process.
        }
    }

    private static string Quote(string value)
    {
        return "\"" + value.Replace("\"", "\\\"") + "\"";
    }

    private static string Format(float value)
    {
        return value.ToString("0.###", CultureInfo.InvariantCulture);
    }

    private static string RelativeToRepo(string path)
    {
        return Path.GetRelativePath(GetRepoRoot(), path).Replace('\\', '/');
    }

    private static string ToRepoPath(string relativePath)
    {
        return Path.Combine(GetRepoRoot(), relativePath);
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
        public RunReport(string label, string reportPath, string status, float averageFps, float onePercentLowFps, float minFps, string sequenceMeters)
        {
            Label = label;
            ReportPath = reportPath;
            Status = status;
            AverageFps = averageFps;
            OnePercentLowFps = onePercentLowFps;
            MinFps = minFps;
            SequenceMeters = sequenceMeters;
        }

        public string Label { get; }
        public string ReportPath { get; }
        public string Status { get; }
        public float AverageFps { get; }
        public float OnePercentLowFps { get; }
        public float MinFps { get; }
        public string SequenceMeters { get; }
    }
}

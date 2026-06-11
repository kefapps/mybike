using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace MYB90.Editor
{
    public static class MYB90WebGLReadinessBuilder
    {
        private const string ScenePath = "Assets/Scenes/MYB89UnityMcpProbe.unity";
        private const string BuildRootRelative = "_bmad-output/unity-webgl-builds/myb-90-unity-mcp-probe";
        private const string BuildReportRelative = "_bmad-output/unity-test-results/myb-90-unity-webgl-build.txt";

        [MenuItem("Tools/MYB-90/Build WebGL Readiness Probe")]
        public static string BuildForMcp()
        {
            var stopwatch = Stopwatch.StartNew();
            var repoRoot = GetRepoRoot();
            var buildPath = Path.Combine(repoRoot, BuildRootRelative);
            var reportPath = Path.Combine(repoRoot, BuildReportRelative);
            Directory.CreateDirectory(buildPath);
            Directory.CreateDirectory(Path.GetDirectoryName(reportPath) ?? repoRoot);

            var lines = new List<string>
            {
                "MYB-90 Unity WebGL readiness build",
                "Timestamp UTC: " + DateTime.UtcNow.ToString("O"),
                "Unity: " + Application.unityVersion,
                "Project: " + Directory.GetParent(Application.dataPath)?.FullName,
                "Scene: " + ScenePath,
                "Build target supported: " + BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.WebGL, BuildTarget.WebGL),
                "Initial active build target: " + EditorUserBuildSettings.activeBuildTarget,
                "Configured server note: unity-mcp-cli status should treat local 20376 as skipped when 8081 is configured.",
                ""
            };

            try
            {
                if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.WebGL, BuildTarget.WebGL))
                {
                    lines.Add("Status: BLOCKED");
                    lines.Add("Reason: WebGL build support is not installed for this Unity Editor.");
                    File.WriteAllLines(reportPath, lines);
                    return reportPath;
                }

                if (!File.Exists(ProjectRelativeToAbsolute(ScenePath)))
                {
                    lines.Add("Status: BLOCKED");
                    lines.Add("Reason: Scene is missing: " + ScenePath);
                    File.WriteAllLines(reportPath, lines);
                    return reportPath;
                }

                EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
                EditorSceneManager.SaveOpenScenes();

                if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.WebGL)
                {
                    lines.Add("Switching active build target to WebGL...");
                    if (!EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.WebGL, BuildTarget.WebGL))
                    {
                        lines.Add("Status: BLOCKED");
                        lines.Add("Reason: SwitchActiveBuildTarget returned false.");
                        File.WriteAllLines(reportPath, lines);
                        return reportPath;
                    }
                }

                PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
                PlayerSettings.WebGL.decompressionFallback = true;
                PlayerSettings.WebGL.memorySize = 128;
                PlayerSettings.WebGL.linkerTarget = WebGLLinkerTarget.Wasm;
                PlayerSettings.productName = "MYB90 Unity WebGL Readiness";

                CleanDirectory(buildPath);

                var buildOptions = new BuildPlayerOptions
                {
                    scenes = new[] { ScenePath },
                    locationPathName = buildPath,
                    target = BuildTarget.WebGL,
                    targetGroup = BuildTargetGroup.WebGL,
                    options = BuildOptions.None
                };

                var report = BuildPipeline.BuildPlayer(buildOptions);
                stopwatch.Stop();

                lines.Add("Status: " + report.summary.result);
                lines.Add("Build path: " + buildPath);
                lines.Add("Duration seconds: " + stopwatch.Elapsed.TotalSeconds.ToString("0.0"));
                lines.Add("Total size bytes: " + report.summary.totalSize);
                lines.Add("Warnings: " + report.summary.totalWarnings);
                lines.Add("Errors: " + report.summary.totalErrors);
                lines.Add("");
                lines.Add("Files:");

                foreach (var file in EnumerateBuildFiles(buildPath))
                {
                    var info = new FileInfo(file);
                    var relativePath = Path.GetRelativePath(buildPath, file);
                    lines.Add("- " + relativePath + " :: " + info.Length + " bytes");
                }
            }
            catch (Exception exception)
            {
                stopwatch.Stop();
                lines.Add("Status: EXCEPTION");
                lines.Add("Duration seconds: " + stopwatch.Elapsed.TotalSeconds.ToString("0.0"));
                lines.Add("Exception: " + exception.GetType().FullName);
                lines.Add("Message: " + exception.Message);
                lines.Add(exception.StackTrace ?? string.Empty);
            }

            File.WriteAllLines(reportPath, lines);
            UnityEngine.Debug.Log("MYB-90 WebGL readiness build report: " + reportPath);
            return reportPath;
        }

        private static IEnumerable<string> EnumerateBuildFiles(string buildPath)
        {
            if (!Directory.Exists(buildPath))
            {
                return Array.Empty<string>();
            }

            return Directory.EnumerateFiles(buildPath, "*", SearchOption.AllDirectories)
                .OrderBy(path => path, StringComparer.Ordinal);
        }

        private static void CleanDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                return;
            }

            foreach (var file in Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories))
            {
                File.Delete(file);
            }

            foreach (var directory in Directory.EnumerateDirectories(path, "*", SearchOption.AllDirectories).OrderByDescending(path => path.Length))
            {
                Directory.Delete(directory, false);
            }
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
    }
}

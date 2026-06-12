using System;
using System.Collections.Generic;
using System.IO;
using MYB48;
using MYB89.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MYB44.Editor
{
    public static class MYB44ScenicCorridorValidator
    {
        private const string ScenePath = "Assets/Scenes/MYB89UnityMcpProbe.unity";
        private const string ReportRelativePath = "_bmad-output/unity-test-results/myb-44-scenic-corridor.txt";

        [MenuItem("Tools/MYB-44/Validate Scenic Corridor")]
        public static string ValidateScenicCorridor()
        {
            OpenOrBuildScene();

            var failures = new List<string>();
            var scenicRoot = GameObject.Find("MYB44_ScenicCorridor");
            var horizon = GameObject.Find("MYB44_Horizon");
            var premiumSignal = GameObject.Find("MYB44_PremiumSignal_RelicLantern");
            var road = GameObject.Find("MYB89_RouteRoad");
            var hud = GameObject.Find("MYB89_HUD");
            var difficultyCues = UnityEngine.Object.FindAnyObjectByType<MYB48RouteDifficultyCueController>();

            var scenicRenderers = scenicRoot == null
                ? Array.Empty<Renderer>()
                : scenicRoot.GetComponentsInChildren<Renderer>(false);
            var horizonRenderers = horizon == null
                ? Array.Empty<Renderer>()
                : horizon.GetComponentsInChildren<Renderer>(false);
            var premiumLights = premiumSignal == null
                ? Array.Empty<Light>()
                : premiumSignal.GetComponentsInChildren<Light>(false);

            if (scenicRoot == null)
            {
                failures.Add("Missing MYB44_ScenicCorridor root.");
            }

            if (scenicRenderers.Length < 45)
            {
                failures.Add("Scenic corridor needs at least 45 renderers; found " + scenicRenderers.Length + ".");
            }

            if (horizon == null)
            {
                failures.Add("Missing MYB44_Horizon root.");
            }

            if (horizonRenderers.Length < 10)
            {
                failures.Add("Horizon needs at least 10 renderers; found " + horizonRenderers.Length + ".");
            }

            if (premiumSignal == null)
            {
                failures.Add("Missing single MYB44 premium signal.");
            }

            if (premiumLights.Length != 1)
            {
                failures.Add("MYB44 premium signal must contain exactly one light; found " + premiumLights.Length + ".");
            }

            if (road == null)
            {
                failures.Add("Missing MYB89 route road after scenic corridor build.");
            }

            if (hud == null)
            {
                failures.Add("Missing MYB89 HUD after scenic corridor build.");
            }

            if (difficultyCues == null || difficultyCues.GeneratedCueCount < 11)
            {
                failures.Add("MYB48 Route Difficulty Cues must remain present and complete.");
            }

            var report = WriteReport(failures, scenicRenderers.Length, horizonRenderers.Length, premiumLights.Length);
            if (failures.Count > 0)
            {
                throw new InvalidOperationException("MYB-44 scenic corridor validation failed. See " + report);
            }

            Debug.Log("MYB-44 scenic corridor validation passed. Report: " + report);
            return report;
        }

        public static void ValidateScenicCorridorCli()
        {
            try
            {
                ValidateScenicCorridor();
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

        private static void OpenOrBuildScene()
        {
            if (!File.Exists(ProjectRelativeToAbsolute(ScenePath)))
            {
                MYB89ProbeBuilder.BuildScene();
                return;
            }

            if (SceneManager.GetActiveScene().path != ScenePath)
            {
                EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            }
        }

        private static string WriteReport(IReadOnlyList<string> failures, int scenicRendererCount, int horizonRendererCount, int premiumLightCount)
        {
            var repoRoot = GetRepoRoot();
            var reportPath = Path.Combine(repoRoot, ReportRelativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(reportPath) ?? repoRoot);

            using (var writer = new StreamWriter(reportPath, false))
            {
                writer.WriteLine("MYB-44 Scenic Corridor validation");
                writer.WriteLine("Timestamp UTC: " + DateTime.UtcNow.ToString("O"));
                writer.WriteLine("Unity: " + Application.unityVersion);
                writer.WriteLine("Scene: " + ScenePath);
                writer.WriteLine("Status: " + (failures.Count == 0 ? "PASS" : "FAIL"));
                writer.WriteLine("Scenic corridor renderers: " + scenicRendererCount);
                writer.WriteLine("Horizon renderers: " + horizonRendererCount);
                writer.WriteLine("Premium signal lights: " + premiumLightCount);
                writer.WriteLine("Corridor: Village / campagne pavee");
                writer.WriteLine("Validation target: Unity Editor/macOS local proof only");

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

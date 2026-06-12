using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MYB48;
using MYB89.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MYB48.Editor
{
    public static class MYB48RouteDifficultyCueValidator
    {
        private const string ScenePath = "Assets/Scenes/MYB89UnityMcpProbe.unity";

        [MenuItem("Tools/MYB-48/Validate Route Difficulty Cues")]
        public static string ValidateRouteDifficultyCuesCli()
        {
            OpenProbeScene();

            var failures = new List<string>();
            var controller = UnityEngine.Object.FindAnyObjectByType<MYB48RouteDifficultyCueController>();
            var renderers = UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsInactive.Exclude)
                .Where(renderer => renderer.name.StartsWith("MYB48_Cue_", StringComparison.Ordinal))
                .ToArray();
            var lights = UnityEngine.Object.FindObjectsByType<Light>(FindObjectsInactive.Exclude)
                .Where(light => light.name.StartsWith("MYB48_Cue_", StringComparison.Ordinal))
                .ToArray();
            var warmupObjects = UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsInactive.Include)
                .Where(transform => transform.name.Contains("Warmup"))
                .Select(transform => transform.name)
                .ToArray();

            if (controller == null)
            {
                failures.Add("Missing MYB48RouteDifficultyCueController.");
            }
            else
            {
                if (controller.routeMarkers == null || controller.routeMarkers.Count(marker => marker != null) < 8)
                {
                    failures.Add("Route difficulty cues are not wired to route markers.");
                }

                if (controller.climbMaterial == null || controller.sprintMaterial == null || controller.recoveryMaterial == null)
                {
                    failures.Add("Route difficulty cue materials are not assigned.");
                }

                if (controller.GeneratedCueCount < 11)
                {
                    failures.Add("Expected at least 11 route difficulty cue placements.");
                }

                if (controller.ClimbCueCount < 4)
                {
                    failures.Add("Expected climb entry plus secondary cues.");
                }

                if (controller.SprintCueCount < 4)
                {
                    failures.Add("Expected sprint entry plus secondary cues.");
                }

                if (controller.RecoveryCueCount < 3)
                {
                    failures.Add("Expected recovery entry plus secondary cues.");
                }

                if (controller.pulseAmplitude <= 0f)
                {
                    failures.Add("Route difficulty cue pulse amplitude must be positive.");
                }
            }

            if (renderers.Length < 24)
            {
                failures.Add("Route difficulty cues need enough visible primitive renderers: " + renderers.Length + ".");
            }

            if (lights.Length < 3)
            {
                failures.Add("Route difficulty cues need entry glow lights for climb, sprint and recovery.");
            }

            if (warmupObjects.Length > 0)
            {
                failures.Add("Warmup should not have dedicated MYB-48 route difficulty cues: " + string.Join(", ", warmupObjects));
            }

            var report = WriteReport(failures, controller, renderers.Length, lights.Length);
            if (failures.Count > 0)
            {
                throw new InvalidOperationException("MYB-48 route difficulty cue validation failed. See " + report);
            }

            Debug.Log("MYB-48 route difficulty cue validation passed. Report: " + report);
            return report;
        }

        private static void OpenProbeScene()
        {
            if (!File.Exists(ProjectRelativeToAbsolute(ScenePath)))
            {
                MYB89ProbeBuilder.BuildScene();
                return;
            }

            var activeScene = SceneManager.GetActiveScene();
            if (activeScene.path != ScenePath)
            {
                EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            }
        }

        private static string WriteReport(IReadOnlyList<string> failures, MYB48RouteDifficultyCueController controller, int rendererCount, int lightCount)
        {
            var repoRoot = GetRepoRoot();
            var reportDir = Path.Combine(repoRoot, "_bmad-output", "unity-test-results");
            Directory.CreateDirectory(reportDir);

            var reportPath = Path.Combine(reportDir, "myb-48-route-difficulty-cues.txt");
            using (var writer = new StreamWriter(reportPath, false))
            {
                writer.WriteLine("MYB-48 Route Difficulty Cue validation");
                writer.WriteLine("Timestamp UTC: " + DateTime.UtcNow.ToString("O"));
                writer.WriteLine("Unity: " + Application.unityVersion);
                writer.WriteLine("Scene: " + ScenePath);
                writer.WriteLine("Status: " + (failures.Count == 0 ? "PASS" : "FAIL"));
                writer.WriteLine("Controller present: " + (controller != null));
                writer.WriteLine("Generated cues: " + (controller == null ? 0 : controller.GeneratedCueCount));
                writer.WriteLine("Climb cues: " + (controller == null ? 0 : controller.ClimbCueCount));
                writer.WriteLine("Sprint cues: " + (controller == null ? 0 : controller.SprintCueCount));
                writer.WriteLine("Recovery cues: " + (controller == null ? 0 : controller.RecoveryCueCount));
                writer.WriteLine("Cue renderers: " + rendererCount);
                writer.WriteLine("Cue lights: " + lightCount);
                writer.WriteLine("Pulse amplitude: " + (controller == null ? 0f : controller.pulseAmplitude).ToString("0.00"));
                writer.WriteLine("Pulse frequency: " + (controller == null ? 0f : controller.pulseFrequency).ToString("0.00"));

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

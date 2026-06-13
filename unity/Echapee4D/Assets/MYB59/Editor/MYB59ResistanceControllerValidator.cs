using System;
using System.Collections.Generic;
using System.IO;
using MYB57;
using MYB89;
using MYB89.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MYB59.Editor
{
    public static class MYB59ResistanceControllerValidator
    {
        private const string ScenePath = "Assets/Scenes/MYB89UnityMcpProbe.unity";

        [MenuItem("Tools/MYB-59/Validate Resistance Controller")]
        public static string ValidateResistanceControllerCli()
        {
            var failures = new List<string>();
            var notes = new List<string>();

            ValidatePureController(failures, notes);
            ValidateSceneIntegration(failures, notes);

            var report = WriteReport(failures, notes);
            if (failures.Count > 0)
            {
                throw new InvalidOperationException("MYB-59 resistance controller validation failed. See " + report);
            }

            Debug.Log("MYB-59 resistance controller validation passed. Report: " + report);
            return report;
        }

        private static void ValidatePureController(ICollection<string> failures, ICollection<string> notes)
        {
            var highDemand = new MYB59ResistanceDemand(1.25f, "over-limit");
            if (Mathf.Abs(highDemand.Resistance01 - 1f) > 0.0001f || highDemand.ResistanceLevel != 100)
            {
                failures.Add("Resistance demand must clamp high values to 1.0 / 100.");
            }

            var lowDemand = MYB59ResistanceDemand.FromLevel(-8, "under-limit");
            if (Mathf.Abs(lowDemand.Resistance01) > 0.0001f || lowDemand.ResistanceLevel != 0)
            {
                failures.Add("Resistance demand must clamp low levels to 0.");
            }

            var climbDemand = MYB59ResistanceDemand.FromLevel(66, "climb");
            var controllerObject = new GameObject("MYB59_ResistanceControllerValidator");
            try
            {
                var controller = controllerObject.AddComponent<MYB59ResistanceController>();
                controller.ControllerLabel = "Validator mock";
                controller.ControllerAvailable = true;
                controller.UseFallbackWhenUnavailable = true;
                controller.FallbackResistance01 = 0.18f;

                var applied = controller.ApplyResistance(climbDemand, 0.016f);
                if (!applied.IsApplied || applied.AppliedResistanceLevel != 66)
                {
                    failures.Add("Available mock controller must apply requested resistance immediately.");
                }

                controller.ControllerAvailable = false;
                var fallback = controller.ApplyResistance(climbDemand, 0.016f);
                if (!fallback.IsFallback || fallback.AppliedResistanceLevel != 18 || fallback.Demand.ResistanceLevel != 66)
                {
                    failures.Add("Unavailable controller with fallback must preserve demand and expose fallback applied resistance.");
                }

                controller.UseFallbackWhenUnavailable = false;
                var unavailable = controller.ApplyResistance(climbDemand, 0.016f);
                if (!unavailable.IsUnavailable || unavailable.AppliedResistanceLevel != 0)
                {
                    failures.Add("Unavailable controller without fallback must report unavailable and zero applied resistance.");
                }

                notes.Add($"Pure controller applied: demand {applied.Demand.ResistanceLevel}, applied {applied.AppliedResistanceLevel}, status {applied.StatusLabel}.");
                notes.Add($"Pure controller fallback: demand {fallback.Demand.ResistanceLevel}, applied {fallback.AppliedResistanceLevel}, status {fallback.StatusLabel}.");
                notes.Add($"Pure controller unavailable: demand {unavailable.Demand.ResistanceLevel}, applied {unavailable.AppliedResistanceLevel}, status {unavailable.StatusLabel}.");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(controllerObject);
            }
        }

        private static void ValidateSceneIntegration(ICollection<string> failures, ICollection<string> notes)
        {
            OpenProbeScene();

            var ride = UnityEngine.Object.FindAnyObjectByType<MYB89ProbeRide>();
            if (ride == null)
            {
                failures.Add("Missing MYB89ProbeRide in canonical scene.");
                return;
            }

            var controller = UnityEngine.Object.FindAnyObjectByType<MYB59ResistanceController>();
            if (controller == null)
            {
                failures.Add("Missing MYB59ResistanceController in canonical scene.");
                return;
            }

            var originalController = ride.resistanceController;
            if (originalController != controller)
            {
                failures.Add("Scene MYB89ProbeRide must have MYB59ResistanceController wired before validation.");
            }

            try
            {
                ride.resistanceController = controller;
                ride.useEffortSimulator = true;
                ride.trainerMode = MYB57TrainerMode.Simulated;
                ride.trainerSourcePreset = MYB57TrainerSourcePreset.PowerAvailable;
                ride.fatigue01 = 0f;
                ride.RebuildRouteCache();

                controller.ControllerAvailable = true;
                controller.UseFallbackWhenUnavailable = true;
                controller.FallbackResistance01 = 0.18f;
                ride.SetPreviewProgress(ride.RouteLength * 0.42f);
                Canvas.ForceUpdateCanvases();

                var effort = ride.LastEffortSnapshot;
                var resistance = ride.LastResistanceSnapshot;
                if (resistance.Demand.ResistanceLevel != effort.TargetResistanceLevel)
                {
                    failures.Add("Scene resistance demand must come from the MYB-57 target resistance.");
                }

                if (!resistance.IsApplied)
                {
                    failures.Add("Scene resistance controller must apply the available mock demand.");
                }

                if (resistance.AppliedResistanceLevel < 45)
                {
                    failures.Add("Scene applied resistance should reflect climb difficulty.");
                }

                if (ride.gradeLabel == null || !ride.gradeLabel.text.Contains("Resistance"))
                {
                    failures.Add("Scene HUD grade label must show readable applied resistance.");
                }

                if (ride.verdictLabel == null || !ride.verdictLabel.text.Contains("Source: Power"))
                {
                    failures.Add("Scene HUD verdict label must show the trainer source without controller debug wording.");
                }

                if (ride.verdictLabel == null || !ride.verdictLabel.text.Contains("Fatigue"))
                {
                    failures.Add("Scene HUD verdict label must show readable fatigue.");
                }

                var appliedHudText = ride.verdictLabel == null ? string.Empty : ride.verdictLabel.text;

                controller.ControllerAvailable = false;
                ride.SetPreviewProgress(ride.RouteLength * 0.42f);
                Canvas.ForceUpdateCanvases();
                var fallbackResistance = ride.LastResistanceSnapshot;
                if (!fallbackResistance.IsFallback || fallbackResistance.AppliedResistanceLevel != 18)
                {
                    failures.Add("Scene fallback must preserve mock playability when the controller is unavailable.");
                }

                var fallbackHudText = ride.verdictLabel == null ? string.Empty : ride.verdictLabel.text;
                if (!fallbackHudText.Contains("Resistance 18"))
                {
                    failures.Add("Scene HUD verdict label must show fallback applied resistance without controller debug wording.");
                }

                controller.ControllerAvailable = true;
                ride.SetPreviewProgress(ride.RouteLength * 0.42f);

                notes.Add($"Scene demand/applied: {resistance.Demand.ResistanceLevel}->{resistance.AppliedResistanceLevel} ({resistance.StatusLabel}).");
                notes.Add("Scene HUD applied: " + appliedHudText);
                notes.Add("Scene HUD fallback: " + fallbackHudText);
            }
            finally
            {
                ride.resistanceController = originalController;
            }
        }

        private static void OpenProbeScene()
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

        private static string WriteReport(IReadOnlyList<string> failures, IReadOnlyList<string> notes)
        {
            var reportDir = Path.Combine(GetRepoRoot(), "_bmad-output", "unity-test-results");
            Directory.CreateDirectory(reportDir);

            var reportPath = Path.Combine(reportDir, "myb-59-resistance-controller.txt");
            using (var writer = new StreamWriter(reportPath, false))
            {
                writer.WriteLine("MYB-59 Resistance Controller validation");
                writer.WriteLine("Timestamp UTC: " + DateTime.UtcNow.ToString("O"));
                writer.WriteLine("Unity: " + Application.unityVersion);
                writer.WriteLine("Scene: " + ScenePath);
                writer.WriteLine("Status: " + (failures.Count == 0 ? "PASS" : "FAIL"));
                writer.WriteLine("Contract: demand 0..1 canonical, level 0..100 derived");
                writer.WriteLine("Statuses: Applied, Fallback, Unavailable");
                writer.WriteLine("Hardware: no BLE, FTMS, CoreBluetooth, or trainer control");

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

        private static string ProjectRelativeToAbsolute(string assetPath)
        {
            var projectRoot = Directory.GetParent(Application.dataPath);
            return Path.Combine(projectRoot == null ? Application.dataPath : projectRoot.FullName, assetPath);
        }

        private static string GetRepoRoot()
        {
            var projectRoot = Directory.GetParent(Application.dataPath);
            var unityFolder = projectRoot?.Parent;
            var repoRoot = unityFolder?.Parent;
            return repoRoot == null ? Directory.GetCurrentDirectory() : repoRoot.FullName;
        }
    }
}

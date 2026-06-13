using System;
using System.Collections.Generic;
using System.IO;
using MYB57;
using MYB59;
using MYB60;
using MYB89;
using MYB89.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MYB64.Editor
{
    public static class MYB64NoTrainerFallbackValidator
    {
        private const string ScenePath = "Assets/Scenes/MYB89UnityMcpProbe.unity";

        [MenuItem("Tools/MYB-64/Validate No-Trainer Fallback")]
        public static string ValidateNoTrainerFallbackCli()
        {
            var failures = new List<string>();
            var notes = new List<string>();

            ValidateManualFallbackModel(failures, notes);
            ValidateSceneFallback(failures, notes);

            var report = WriteReport(failures, notes);
            if (failures.Count > 0)
            {
                throw new InvalidOperationException("MYB-64 no-trainer fallback validation failed. See " + report);
            }

            Debug.Log("MYB-64 no-trainer fallback validation passed. Report: " + report);
            return report;
        }

        private static void ValidateManualFallbackModel(ICollection<string> failures, ICollection<string> notes)
        {
            var input = new MYB57EffortInput(
                MYB57TrainerMode.Simulated,
                MYB57TrainerSourcePreset.ManualFallback,
                MYB57RouteSegment.Climb,
                5f,
                0f,
                1f);
            var effort = MYB57EffortSimulator.EvaluateManualFallback(input, 0.62f);

            if (effort.SourcePreset != MYB57TrainerSourcePreset.ManualFallback)
            {
                failures.Add("Manual fallback effort must preserve the ManualFallback source preset.");
            }

            if (effort.SourceBadge != "Manual")
            {
                failures.Add("Manual fallback effort must expose a Manual source badge.");
            }

            if (Mathf.Abs(effort.PedalEffort01 - 0.62f) > 0.01f)
            {
                failures.Add("Manual fallback effort must use the configured manual effort value.");
            }

            if (effort.SpeedMetersPerSecond <= 0.1f)
            {
                failures.Add("Manual fallback effort must keep the ride moving without trainer data.");
            }

            notes.Add($"Manual model: effort {effort.PedalEffort01 * 100f:00}%, speed {effort.SpeedMetersPerSecond * 3.6f:00} km/h, source {effort.SourceBadge}.");
        }

        private static void ValidateSceneFallback(ICollection<string> failures, ICollection<string> notes)
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

            var mapper = UnityEngine.Object.FindAnyObjectByType<MYB60ResistanceMapper>();
            if (mapper == null)
            {
                failures.Add("Missing MYB60ResistanceMapper in canonical scene.");
                return;
            }

            var originalUseEffortSimulator = ride.useEffortSimulator;
            var originalUseNoTrainerFallback = ride.useNoTrainerFallback;
            var originalManualEffort = ride.manualFallbackEffort01;
            var originalTrainerPreset = ride.trainerSourcePreset;
            var originalFatigue = ride.fatigue01;
            var originalControllerAvailable = controller.ControllerAvailable;
            var originalUseFallback = controller.UseFallbackWhenUnavailable;
            var originalFallbackResistance = controller.FallbackResistance01;

            try
            {
                ride.useEffortSimulator = true;
                ride.useNoTrainerFallback = true;
                ride.manualFallbackEffort01 = 0.62f;
                ride.trainerSourcePreset = MYB57TrainerSourcePreset.PowerAvailable;
                ride.fatigue01 = 0f;
                controller.ControllerAvailable = false;
                controller.UseFallbackWhenUnavailable = true;
                controller.FallbackResistance01 = MYB59ResistanceController.DefaultFallbackResistance01;
                mapper.ResetSmoothing();
                ride.RebuildRouteCache();
                ride.SetPreviewProgress(ride.RouteLength * 0.42f);
                Canvas.ForceUpdateCanvases();

                var effort = ride.LastEffortSnapshot;
                var resistance = ride.LastResistanceSnapshot;

                if (!ride.IsNoTrainerFallbackActive)
                {
                    failures.Add("Scene ride must report no-trainer fallback active.");
                }

                if (effort.SourcePreset != MYB57TrainerSourcePreset.ManualFallback || effort.SourceBadge != "Manual")
                {
                    failures.Add("Scene fallback effort must use the ManualFallback source.");
                }

                if (Mathf.Abs(effort.PedalEffort01 - ride.manualFallbackEffort01) > 0.01f)
                {
                    failures.Add("Scene fallback effort must use the configured manual effort.");
                }

                if (effort.SpeedMetersPerSecond <= 0.1f)
                {
                    failures.Add("Scene fallback effort must keep route progression playable.");
                }

                if (resistance.Status != MYB59ResistanceControllerStatus.Fallback)
                {
                    failures.Add("Scene fallback must keep applying the MYB-59 local resistance fallback when no controller is available.");
                }

                if (ride.difficultyLabel == null || !ride.difficultyLabel.text.Contains("Manual fallback"))
                {
                    failures.Add("HUD difficulty label must explain the manual fallback effort mode.");
                }

                if (ride.verdictLabel == null
                    || !ride.verdictLabel.text.Contains("Sans capteur: Manual")
                    || !ride.verdictLabel.text.Contains("Resistance")
                    || !ride.verdictLabel.text.Contains("Fatigue"))
                {
                    failures.Add("HUD verdict label must explain no-trainer mode without controller debug wording.");
                }

                notes.Add($"Scene fallback: effort {effort.PedalEffort01 * 100f:00}%, speed {effort.SpeedMetersPerSecond * 3.6f:00} km/h, controller {resistance.StatusLabel} {resistance.AppliedResistanceLevel}.");
                notes.Add("Scene HUD difficulty: " + (ride.difficultyLabel == null ? string.Empty : ride.difficultyLabel.text));
                notes.Add("Scene HUD verdict: " + (ride.verdictLabel == null ? string.Empty : ride.verdictLabel.text));

                ride.useNoTrainerFallback = false;
                ride.trainerSourcePreset = MYB57TrainerSourcePreset.ManualFallback;
                ride.manualFallbackEffort01 = 0.37f;
                mapper.ResetSmoothing();
                ride.SetPreviewProgress(ride.RouteLength * 0.42f);
                Canvas.ForceUpdateCanvases();

                effort = ride.LastEffortSnapshot;
                if (!ride.IsNoTrainerFallbackActive)
                {
                    failures.Add("Scene ride must treat the ManualFallback source preset as no-trainer fallback.");
                }

                if (effort.SourcePreset != MYB57TrainerSourcePreset.ManualFallback
                    || effort.SourceBadge != "Manual"
                    || Mathf.Abs(effort.PedalEffort01 - ride.manualFallbackEffort01) > 0.01f)
                {
                    failures.Add("ManualFallback source preset must use the configured manual effort path.");
                }

                if (ride.verdictLabel == null || !ride.verdictLabel.text.Contains("Sans capteur: Manual"))
                {
                    failures.Add("ManualFallback source preset must show the no-trainer HUD label.");
                }

                notes.Add($"Preset fallback: effort {effort.PedalEffort01 * 100f:00}%, source {effort.SourceBadge}.");
            }
            finally
            {
                ride.useEffortSimulator = originalUseEffortSimulator;
                ride.useNoTrainerFallback = originalUseNoTrainerFallback;
                ride.manualFallbackEffort01 = originalManualEffort;
                ride.trainerSourcePreset = originalTrainerPreset;
                ride.fatigue01 = originalFatigue;
                controller.ControllerAvailable = originalControllerAvailable;
                controller.UseFallbackWhenUnavailable = originalUseFallback;
                controller.FallbackResistance01 = originalFallbackResistance;
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

            var reportPath = Path.Combine(reportDir, "myb-64-no-trainer-fallback.txt");
            using (var writer = new StreamWriter(reportPath, false))
            {
                writer.WriteLine("MYB-64 No-Trainer Fallback validation");
                writer.WriteLine("Timestamp UTC: " + DateTime.UtcNow.ToString("O"));
                writer.WriteLine("Unity: " + Application.unityVersion);
                writer.WriteLine("Scene: " + ScenePath);
                writer.WriteLine("Status: " + (failures.Count == 0 ? "PASS" : "FAIL"));
                writer.WriteLine("Contract: manual effort keeps Unity playable without trainer hardware");
                writer.WriteLine("Hardware: no BLE, FTMS, CoreBluetooth, pairing, or required smart trainer");

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

        private static string ProjectRelativeToAbsolute(string projectRelativePath)
        {
            return Path.Combine(Directory.GetCurrentDirectory(), projectRelativePath);
        }

        private static string GetRepoRoot()
        {
            var current = new DirectoryInfo(Directory.GetCurrentDirectory());
            while (current != null)
            {
                if (Directory.Exists(Path.Combine(current.FullName, ".git")))
                {
                    return current.FullName;
                }

                current = current.Parent;
            }

            return Directory.GetCurrentDirectory();
        }
    }
}

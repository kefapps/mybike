using System;
using System.Collections.Generic;
using System.IO;
using MYB57;
using MYB59;
using MYB89;
using MYB89.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MYB60.Editor
{
    public static class MYB60ResistanceMapperValidator
    {
        private const string ScenePath = "Assets/Scenes/MYB89UnityMcpProbe.unity";

        [MenuItem("Tools/MYB-60/Validate Resistance Mapper")]
        public static string ValidateResistanceMapperCli()
        {
            var failures = new List<string>();
            var notes = new List<string>();

            ValidatePureMapper(failures, notes);
            ValidateSceneIntegration(failures, notes);

            var report = WriteReport(failures, notes);
            if (failures.Count > 0)
            {
                throw new InvalidOperationException("MYB-60 resistance mapper validation failed. See " + report);
            }

            Debug.Log("MYB-60 resistance mapper validation passed. Report: " + report);
            return report;
        }

        private static void ValidatePureMapper(ICollection<string> failures, ICollection<string> notes)
        {
            var warmupHigh = MYB60ResistanceMapper.LocalFallback(
                new MYB60ResistanceMappingInput(MYB57RouteSegment.Warmup, 0f, 0.9f, 0f),
                0f,
                false);
            if (!warmupHigh.IsLimited || warmupHigh.SmoothedResistanceLevel != 55)
            {
                failures.Add("Warmup resistance must be capped at the comfort warmup limit.");
            }

            var recoveryHigh = MYB60ResistanceMapper.LocalFallback(
                new MYB60ResistanceMappingInput(MYB57RouteSegment.Recovery, 3f, 0.7f, 0f),
                0f,
                false);
            if (!recoveryHigh.IsLimited || recoveryHigh.SmoothedResistanceLevel != 42)
            {
                failures.Add("Recovery resistance must be capped at the recovery comfort limit.");
            }

            var climbTarget = MYB60ResistanceMapper.LocalFallback(
                new MYB60ResistanceMappingInput(MYB57RouteSegment.Climb, 5f, 0.82f, 0f),
                0f,
                false);
            var rampingUp = MYB60ResistanceMapper.LocalFallback(
                new MYB60ResistanceMappingInput(MYB57RouteSegment.Climb, 5f, 0.82f, 0.5f),
                0.24f,
                true);
            if (!rampingUp.IsRamping || rampingUp.SmoothedResistance01 >= climbTarget.SmoothedResistance01)
            {
                failures.Add("Climb resistance must ramp up instead of jumping to the target.");
            }

            var rampingDown = MYB60ResistanceMapper.LocalFallback(
                new MYB60ResistanceMappingInput(MYB57RouteSegment.Recovery, -2f, 0.18f, 0.1f),
                0.82f,
                true);
            if (!rampingDown.IsRamping
                || rampingDown.SmoothedResistance01 <= 0.18f
                || rampingDown.SmoothedResistanceLevel > 42)
            {
                failures.Add("Recovery resistance must ramp down without exceeding the recovery comfort limit.");
            }

            var zeroDelta = MYB60ResistanceMapper.LocalFallback(
                new MYB60ResistanceMappingInput(MYB57RouteSegment.Climb, 5f, 0.82f, 0f),
                0.24f,
                true);
            if (!zeroDelta.IsRamping || zeroDelta.SmoothedResistanceLevel != 24)
            {
                failures.Add("Zero-delta resistance sampling must preserve the previous smoothed value.");
            }

            notes.Add($"Warmup limit: raw {warmupHigh.Input.TargetResistanceLevel}, smooth {warmupHigh.SmoothedResistanceLevel}, status {warmupHigh.StatusLabel}.");
            notes.Add($"Recovery limit: raw {recoveryHigh.Input.TargetResistanceLevel}, smooth {recoveryHigh.SmoothedResistanceLevel}, status {recoveryHigh.StatusLabel}.");
            notes.Add($"Ramp up: previous 24, target {climbTarget.SmoothedResistanceLevel}, smooth {rampingUp.SmoothedResistanceLevel}.");
            notes.Add($"Ramp down: previous 82, target 18, smooth {rampingDown.SmoothedResistanceLevel}.");
            notes.Add($"Zero-delta hold: previous 24, target 82, smooth {zeroDelta.SmoothedResistanceLevel}.");
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

            var mapper = UnityEngine.Object.FindAnyObjectByType<MYB60ResistanceMapper>();
            if (mapper == null)
            {
                failures.Add("Missing MYB60ResistanceMapper in canonical scene.");
                return;
            }

            var controller = UnityEngine.Object.FindAnyObjectByType<MYB59ResistanceController>();
            if (controller == null)
            {
                failures.Add("Missing MYB59ResistanceController in canonical scene.");
                return;
            }

            var originalMapper = ride.resistanceMapper;
            var originalController = ride.resistanceController;
            var originalMaxResistance01 = mapper.MaxResistance01;
            if (originalMapper != mapper)
            {
                failures.Add("Scene MYB89ProbeRide must have MYB60ResistanceMapper wired before validation.");
            }

            if (originalController != controller)
            {
                failures.Add("Scene MYB89ProbeRide must have MYB59ResistanceController wired before validation.");
            }

            try
            {
                ride.resistanceMapper = mapper;
                ride.resistanceController = controller;
                ride.useEffortSimulator = true;
                ride.trainerMode = MYB57TrainerMode.Simulated;
                ride.trainerSourcePreset = MYB57TrainerSourcePreset.PowerAvailable;
                ride.fatigue01 = 0f;
                ride.RebuildRouteCache();
                mapper.ResetSmoothing();

                controller.ControllerAvailable = true;
                controller.UseFallbackWhenUnavailable = true;
                ride.SetPreviewProgress(ride.RouteLength * 0.42f);
                Canvas.ForceUpdateCanvases();

                var mapping = ride.LastResistanceMappingSnapshot;
                var resistance = ride.LastResistanceSnapshot;
                if (mapping.SmoothedResistanceLevel != resistance.Demand.ResistanceLevel)
                {
                    failures.Add("Scene resistance demand must use the MYB-60 smoothed resistance.");
                }

                if (resistance.Status != MYB59ResistanceControllerStatus.Applied)
                {
                    failures.Add("Scene resistance controller must apply the MYB-60 mapped demand.");
                }

                if (ride.gradeLabel == null || !ride.gradeLabel.text.Contains("~") || !ride.gradeLabel.text.Contains("->"))
                {
                    failures.Add("Scene HUD grade label must expose raw-to-smoothed-to-applied resistance.");
                }

                if (ride.verdictLabel == null || !ride.verdictLabel.text.Contains("Map "))
                {
                    failures.Add("Scene HUD verdict label must expose MYB-60 mapping status.");
                }

                var first = mapper.MapResistance(new MYB60ResistanceMappingInput(MYB57RouteSegment.Warmup, 0f, 0.24f, 0f));
                var second = mapper.MapResistance(new MYB60ResistanceMappingInput(MYB57RouteSegment.Climb, 6f, 0.86f, 0.25f));
                if (!second.IsRamping || second.SmoothedResistance01 <= first.SmoothedResistance01)
                {
                    failures.Add("Scene mapper must smooth a warmup-to-climb resistance jump.");
                }

                mapper.ResetSmoothing();
                ride.progressMeters = ride.RouteLength * 0.12f;
                SampleRideEffort(ride, 0f);
                ride.progressMeters = ride.RouteLength * 0.42f;
                var rampEffort = SampleRideEffort(ride, 0.25f);
                var rampMapping = ride.LastResistanceMappingSnapshot;
                var rampResistance = ride.LastResistanceSnapshot;
                if (!rampMapping.IsRamping
                    || rampResistance.Demand.ResistanceLevel != rampMapping.SmoothedResistanceLevel
                    || rampResistance.Demand.ResistanceLevel >= rampEffort.TargetResistanceLevel)
                {
                    failures.Add("Scene ride path must send smoothed ramping resistance to the MYB-59 controller.");
                }

                mapper.MaxResistance01 = 0.4f;
                mapper.ResetSmoothing();
                ride.SetPreviewProgress(ride.RouteLength * 0.42f);
                Canvas.ForceUpdateCanvases();
                var limitedMapping = ride.LastResistanceMappingSnapshot;
                var limitedResistance = ride.LastResistanceSnapshot;
                if (!limitedMapping.IsLimited
                    || limitedMapping.SmoothedResistanceLevel != 40
                    || limitedResistance.Demand.ResistanceLevel != limitedMapping.SmoothedResistanceLevel)
                {
                    failures.Add("Scene ride path must send comfort-limited resistance to the MYB-59 controller.");
                }

                mapper.MaxResistance01 = originalMaxResistance01;
                mapper.ResetSmoothing();
                ride.SetPreviewProgress(ride.RouteLength * 0.255f);
                var beforeAdvance = ride.LastEffortSnapshot;
                var advanceDelta = (ride.RouteLength * 0.02f) / Mathf.Max(0.1f, beforeAdvance.SpeedMetersPerSecond);
                AdvanceAutoplayFrame(ride, advanceDelta);
                var afterAdvance = ride.LastEffortSnapshot;
                var advancedProgress01 = Mathf.Clamp01(Mathf.Repeat(ride.progressMeters, ride.RouteLength) / ride.RouteLength);
                var expectedSegment = MYB57EffortSimulator.SegmentForProgress(advancedProgress01);
                if (beforeAdvance.RouteSegment != MYB57RouteSegment.Warmup
                    || expectedSegment != MYB57RouteSegment.Climb
                    || afterAdvance.RouteSegment != expectedSegment)
                {
                    failures.Add("Autoplay effort sampling must use the advanced route position before pose and HUD update.");
                }

                notes.Add($"Scene mapping: raw {mapping.Input.TargetResistanceLevel}, smooth {mapping.SmoothedResistanceLevel}, applied {resistance.AppliedResistanceLevel}, status {mapping.StatusLabel}.");
                notes.Add("Scene HUD grade: " + (ride.gradeLabel == null ? string.Empty : ride.gradeLabel.text));
                notes.Add("Scene HUD verdict: " + (ride.verdictLabel == null ? string.Empty : ride.verdictLabel.text));
                notes.Add($"Scene smoothing probe: {first.SmoothedResistanceLevel}->{second.SmoothedResistanceLevel} ({second.StatusLabel}).");
                notes.Add($"Scene ride ramp demand: raw {rampEffort.TargetResistanceLevel}, smooth {rampMapping.SmoothedResistanceLevel}, demand {rampResistance.Demand.ResistanceLevel}.");
                notes.Add($"Scene ride limit demand: raw {limitedMapping.Input.TargetResistanceLevel}, limited {limitedMapping.LimitedResistanceLevel}, demand {limitedResistance.Demand.ResistanceLevel}.");
                notes.Add($"Scene autoplay alignment: {beforeAdvance.RouteSegment}->{afterAdvance.RouteSegment} at {advancedProgress01 * 100f:00}%.");
            }
            finally
            {
                mapper.MaxResistance01 = originalMaxResistance01;
                ride.resistanceMapper = originalMapper;
                ride.resistanceController = originalController;
            }
        }

        private static MYB57EffortSnapshot SampleRideEffort(MYB89ProbeRide ride, float deltaTimeSeconds)
        {
            var method = typeof(MYB89ProbeRide).GetMethod(
                "SampleEffort",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            if (method == null)
            {
                throw new MissingMethodException(nameof(MYB89ProbeRide), "SampleEffort");
            }

            return (MYB57EffortSnapshot)method.Invoke(ride, new object[] { deltaTimeSeconds, false });
        }

        private static void AdvanceAutoplayFrame(MYB89ProbeRide ride, float deltaTimeSeconds)
        {
            var method = typeof(MYB89ProbeRide).GetMethod(
                "AdvanceAutoplayFrame",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            if (method == null)
            {
                throw new MissingMethodException(nameof(MYB89ProbeRide), "AdvanceAutoplayFrame");
            }

            method.Invoke(ride, new object[] { deltaTimeSeconds });
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

            var reportPath = Path.Combine(reportDir, "myb-60-resistance-mapper.txt");
            using (var writer = new StreamWriter(reportPath, false))
            {
                writer.WriteLine("MYB-60 Resistance Mapper validation");
                writer.WriteLine("Timestamp UTC: " + DateTime.UtcNow.ToString("O"));
                writer.WriteLine("Unity: " + Application.unityVersion);
                writer.WriteLine("Scene: " + ScenePath);
                writer.WriteLine("Status: " + (failures.Count == 0 ? "PASS" : "FAIL"));
                writer.WriteLine("Contract: raw MYB-57 target -> comfort-limited -> smoothed MYB-59 demand");
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

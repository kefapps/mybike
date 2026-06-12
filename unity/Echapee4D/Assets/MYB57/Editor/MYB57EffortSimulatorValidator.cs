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

namespace MYB57.Editor
{
    public static class MYB57EffortSimulatorValidator
    {
        private const string ScenePath = "Assets/Scenes/MYB89UnityMcpProbe.unity";

        [MenuItem("Tools/MYB-57/Validate Effort Simulator")]
        public static string ValidateEffortSimulatorCli()
        {
            var failures = new List<string>();
            var notes = new List<string>();

            ValidatePureModel(failures, notes);
            ValidateSceneHud(failures, notes);

            var report = WriteReport(failures, notes);
            if (failures.Count > 0)
            {
                throw new InvalidOperationException("MYB-57 effort simulator validation failed. See " + report);
            }

            Debug.Log("MYB-57 effort simulator validation passed. Report: " + report);
            return report;
        }

        private static void ValidatePureModel(ICollection<string> failures, ICollection<string> notes)
        {
            var climbPower = Snapshot(MYB57TrainerSourcePreset.PowerAvailable, MYB57RouteSegment.Climb, 4f, 0f, 1f);
            var climbCadenceResistance = Snapshot(MYB57TrainerSourcePreset.CadenceResistance, MYB57RouteSegment.Climb, 4f, 0f, 1f);
            var climbCadenceOnly = Snapshot(MYB57TrainerSourcePreset.CadenceOnly, MYB57RouteSegment.Climb, 4f, 0f, 1f);
            var warmup = Snapshot(MYB57TrainerSourcePreset.PowerAvailable, MYB57RouteSegment.Warmup, 0f, 0f, 1f);
            var recovery = Snapshot(MYB57TrainerSourcePreset.PowerAvailable, MYB57RouteSegment.Recovery, -3f, 0.25f, 1f);

            if (climbPower.SourceBadge != "Power" || !climbPower.TrainerSample.HasPowerWatts)
            {
                failures.Add("PowerAvailable preset must expose power data.");
            }

            if (climbCadenceResistance.SourceBadge != "Cad+Res"
                || climbCadenceResistance.TrainerSample.HasPowerWatts
                || !climbCadenceResistance.TrainerSample.HasResistanceLevel)
            {
                failures.Add("CadenceResistance preset must expose cadence plus resistance without power.");
            }

            if (climbCadenceOnly.SourceBadge != "Cadence"
                || climbCadenceOnly.TrainerSample.HasPowerWatts
                || climbCadenceOnly.TrainerSample.HasResistanceLevel)
            {
                failures.Add("CadenceOnly preset must expose cadence without power or resistance.");
            }

            if (climbPower.TargetResistanceLevel <= warmup.TargetResistanceLevel)
            {
                failures.Add("Climb target resistance must be higher than warmup.");
            }

            if (recovery.TargetResistanceLevel >= climbPower.TargetResistanceLevel)
            {
                failures.Add("Recovery/descent target resistance must be lower than climb.");
            }

            var lowCadenceSample = new MYB57TrainerSample(false, 0f, true, 42f, true, climbPower.TargetResistanceLevel, 1f);
            var highCadenceSample = new MYB57TrainerSample(false, 0f, true, 96f, true, climbPower.TargetResistanceLevel, 1f);
            var input = new MYB57EffortInput(
                MYB57TrainerMode.Simulated,
                MYB57TrainerSourcePreset.CadenceResistance,
                MYB57RouteSegment.Climb,
                4f,
                0f,
                1f);
            var lowCadence = MYB57EffortSimulator.Evaluate(input, lowCadenceSample);
            var highCadence = MYB57EffortSimulator.Evaluate(input, highCadenceSample);
            if (highCadence.SpeedMetersPerSecond <= lowCadence.SpeedMetersPerSecond)
            {
                failures.Add("Higher cadence at equal resistance must produce higher speed.");
            }

            if (recovery.Fatigue01 >= 0.25f)
            {
                failures.Add("Recovery/descent should reduce fatigue.");
            }

            notes.Add($"Power preset climb: speed {climbPower.SpeedMetersPerSecond:0.00} m/s, resistance {climbPower.TargetResistanceLevel}, effort {climbPower.PedalEffort01:0.00}.");
            notes.Add($"Cad+Res climb: speed {climbCadenceResistance.SpeedMetersPerSecond:0.00} m/s, source {climbCadenceResistance.SourceBadge}.");
            notes.Add($"Cadence-only climb: speed {climbCadenceOnly.SpeedMetersPerSecond:0.00} m/s, source {climbCadenceOnly.SourceBadge}.");
            notes.Add($"Low/high cadence speed check: {lowCadence.SpeedMetersPerSecond:0.00} -> {highCadence.SpeedMetersPerSecond:0.00} m/s.");
        }

        private static void ValidateSceneHud(ICollection<string> failures, ICollection<string> notes)
        {
            OpenProbeScene();

            var ride = UnityEngine.Object.FindAnyObjectByType<MYB89ProbeRide>();
            if (ride == null)
            {
                failures.Add("Missing MYB89ProbeRide in canonical scene.");
                return;
            }

            ride.useEffortSimulator = true;
            ride.trainerMode = MYB57TrainerMode.Simulated;
            ride.trainerSourcePreset = MYB57TrainerSourcePreset.PowerAvailable;
            ride.fatigue01 = 0f;
            ride.RebuildRouteCache();
            ride.SetPreviewProgress(ride.RouteLength * 0.42f);
            Canvas.ForceUpdateCanvases();

            var snapshot = ride.LastEffortSnapshot;
            if (snapshot.TrainerMode != MYB57TrainerMode.Simulated)
            {
                failures.Add("Scene effort snapshot must stay in simulated trainer mode for MYB-57.");
            }

            if (snapshot.SourceBadge != "Power")
            {
                failures.Add("Scene default trainer source should be PowerAvailable.");
            }

            if (snapshot.TargetResistanceLevel < 45)
            {
                failures.Add("Climb preview should expose a meaningful target resistance.");
            }

            if (snapshot.SpeedMetersPerSecond <= 0f)
            {
                failures.Add("Effort simulator must produce positive scene speed.");
            }

            if (ride.difficultyLabel == null || !ride.difficultyLabel.text.Contains("Effort:"))
            {
                failures.Add("HUD difficulty label must include effort text.");
            }

            if (ride.gradeLabel == null || !ride.gradeLabel.text.Contains("Res:"))
            {
                failures.Add("HUD grade label must include target resistance.");
            }

            if (ride.verdictLabel == null || !ride.verdictLabel.text.Contains("Mock trainer: Power"))
            {
                failures.Add("HUD verdict label must include the mock trainer source badge.");
            }

            notes.Add("Scene HUD difficulty: " + (ride.difficultyLabel == null ? string.Empty : ride.difficultyLabel.text));
            notes.Add("Scene HUD grade/resistance: " + (ride.gradeLabel == null ? string.Empty : ride.gradeLabel.text));
            notes.Add("Scene HUD source/fatigue: " + (ride.verdictLabel == null ? string.Empty : ride.verdictLabel.text));
        }

        private static MYB57EffortSnapshot Snapshot(
            MYB57TrainerSourcePreset preset,
            MYB57RouteSegment segment,
            float gradePercent,
            float previousFatigue01,
            float deltaTimeSeconds)
        {
            var input = new MYB57EffortInput(
                MYB57TrainerMode.Simulated,
                preset,
                segment,
                gradePercent,
                previousFatigue01,
                deltaTimeSeconds);
            return MYB57EffortSimulator.EvaluateMock(input);
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

            var reportPath = Path.Combine(reportDir, "myb-57-effort-simulator.txt");
            using (var writer = new StreamWriter(reportPath, false))
            {
                writer.WriteLine("MYB-57 Effort Simulator validation");
                writer.WriteLine("Timestamp UTC: " + DateTime.UtcNow.ToString("O"));
                writer.WriteLine("Unity: " + Application.unityVersion);
                writer.WriteLine("Scene: " + ScenePath);
                writer.WriteLine("Status: " + (failures.Count == 0 ? "PASS" : "FAIL"));
                writer.WriteLine("Trainer modes modeled: simulated, readOnly, controllable");
                writer.WriteLine("Implemented mode: simulated");
                writer.WriteLine("Source priority: powerWatts -> cadenceRpm + resistanceLevel -> cadenceRpm");
                writer.WriteLine("Mock presets: PowerAvailable, CadenceResistance, CadenceOnly");

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
            if (projectRoot == null || projectRoot.Parent == null || projectRoot.Parent.Parent == null)
            {
                return projectRoot == null ? Application.dataPath : projectRoot.FullName;
            }

            return projectRoot.Parent.Parent.FullName;
        }
    }
}

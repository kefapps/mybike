using System;
using System.Collections.Generic;
using System.IO;
using MYB73;
using MYB89;
using MYB89.Editor;
using UnityEditor;
using UnityEngine;

namespace MYB80.Editor
{
    public static class MYB80HudCadenceValidator
    {
        private const string ReportRelativePath = "_bmad-output/unity-test-results/myb-80-hud-cadence-validator.txt";
        private const int SimulatedFrameCount = 60;
        private const float SimulatedDeltaSeconds = 1f / 60f;

        [MenuItem("Tools/MYB-80/Validate HUD Cadence")]
        public static string ValidateHudCadenceCli()
        {
            MYB89ProbeBuilder.BuildScene();

            var failures = new List<string>();
            var ride = UnityEngine.Object.FindAnyObjectByType<MYB89ProbeRide>();
            var preview = UnityEngine.Object.FindAnyObjectByType<MYB73RoutePreviewPanel>();

            if (ride == null)
            {
                failures.Add("Missing MYB89ProbeRide.");
            }

            if (preview != null)
            {
                preview.ShowPreview();
                if (IsHudVisible(ride))
                {
                    failures.Add("Ride HUD should remain hidden while the welcome screen is visible.");
                }

                preview.LaunchRide();
            }

            HudCadenceSnapshot snapshot = default;
            if (ride != null)
            {
                ride.RebuildRouteCache();
                ride.autoplay = true;
                ride.ResetHudCadenceCounters(0f);

                var previousFastCopy = FastHudCopy(ride);
                var previousSlowCopy = SlowHudCopy(ride);
                var fastTextMutations = 0;
                var slowTextMutations = 0;
                var hudTimeSeconds = 0f;

                for (var frame = 0; frame < SimulatedFrameCount; frame++)
                {
                    hudTimeSeconds += SimulatedDeltaSeconds;
                    ride.TickHudCadenceForValidation(SimulatedDeltaSeconds, hudTimeSeconds);

                    var fastCopy = FastHudCopy(ride);
                    if (!string.Equals(previousFastCopy, fastCopy, StringComparison.Ordinal))
                    {
                        fastTextMutations++;
                        previousFastCopy = fastCopy;
                    }

                    var slowCopy = SlowHudCopy(ride);
                    if (!string.Equals(previousSlowCopy, slowCopy, StringComparison.Ordinal))
                    {
                        slowTextMutations++;
                        previousSlowCopy = slowCopy;
                    }
                }

                snapshot = new HudCadenceSnapshot(
                    ride.HudFastRefreshIntervalSeconds,
                    ride.HudSlowRefreshIntervalSeconds,
                    ride.HudFastTextUpdateCount,
                    ride.HudSlowTextUpdateCount,
                    fastTextMutations,
                    slowTextMutations,
                    AllHudCopy(ride),
                    ride.progressMeters);

                if (ride.HudFastTextUpdateCount < 3 || ride.HudFastTextUpdateCount > 5)
                {
                    failures.Add($"Fast HUD refresh count should stay near 4 Hz, got {ride.HudFastTextUpdateCount}.");
                }

                if (ride.HudSlowTextUpdateCount < 1 || ride.HudSlowTextUpdateCount > 3)
                {
                    failures.Add($"Slow HUD refresh count should stay near 2 Hz, got {ride.HudSlowTextUpdateCount}.");
                }

                if (ride.HudFastRefreshIntervalSeconds < 0.24f || ride.HudFastRefreshIntervalSeconds > 0.26f)
                {
                    failures.Add($"Fast HUD interval should be 0.25s, got {ride.HudFastRefreshIntervalSeconds:0.000}s.");
                }

                if (ride.HudSlowRefreshIntervalSeconds < 0.49f || ride.HudSlowRefreshIntervalSeconds > 0.51f)
                {
                    failures.Add($"Slow HUD interval should be 0.50s, got {ride.HudSlowRefreshIntervalSeconds:0.000}s.");
                }

                if (snapshot.ProgressMeters < 7.5f)
                {
                    failures.Add($"Ride simulation should keep advancing while HUD is throttled, got {snapshot.ProgressMeters:0.0}m.");
                }

                if (fastTextMutations > 5)
                {
                    failures.Add($"Fast HUD text mutated too often: {fastTextMutations}.");
                }

                if (ContainsForbiddenHudDebugCopy(snapshot.VisibleCopy))
                {
                    failures.Add("HUD visible copy should not expose Mock, Map, Ctrl, running, or raw resistance arrows.");
                }
            }

            var report = WriteReport(failures, snapshot);
            if (failures.Count > 0)
            {
                throw new InvalidOperationException("MYB-80 HUD cadence validation failed. See " + report);
            }

            Debug.Log("MYB-80 HUD cadence validation passed. Report: " + report);
            return report;
        }

        private static bool IsHudVisible(MYB89ProbeRide ride)
        {
            if (ride == null)
            {
                return false;
            }

            return IsActive(ride.distanceLabel)
                || IsActive(ride.speedLabel)
                || IsActive(ride.difficultyLabel)
                || IsActive(ride.gradeLabel)
                || IsActive(ride.segmentLabel)
                || IsActive(ride.verdictLabel);
        }

        private static bool IsActive(Component component)
        {
            return component != null && component.gameObject.activeInHierarchy;
        }

        private static string FastHudCopy(MYB89ProbeRide ride)
        {
            return string.Join(
                " ",
                ride.distanceLabel == null ? string.Empty : ride.distanceLabel.text,
                ride.speedLabel == null ? string.Empty : ride.speedLabel.text);
        }

        private static string SlowHudCopy(MYB89ProbeRide ride)
        {
            return string.Join(
                " ",
                ride.difficultyLabel == null ? string.Empty : ride.difficultyLabel.text,
                ride.gradeLabel == null ? string.Empty : ride.gradeLabel.text,
                ride.segmentLabel == null ? string.Empty : ride.segmentLabel.text,
                ride.verdictLabel == null ? string.Empty : ride.verdictLabel.text);
        }

        private static string AllHudCopy(MYB89ProbeRide ride)
        {
            return string.Join(" ", FastHudCopy(ride), SlowHudCopy(ride));
        }

        private static bool ContainsForbiddenHudDebugCopy(string copy)
        {
            if (string.IsNullOrWhiteSpace(copy))
            {
                return false;
            }

            var normalized = copy.ToLowerInvariant();
            return normalized.Contains("mock")
                || normalized.Contains("map ")
                || normalized.Contains("ctrl")
                || normalized.Contains("running")
                || normalized.Contains("->")
                || normalized.Contains("~");
        }

        private static string WriteReport(IReadOnlyList<string> failures, HudCadenceSnapshot snapshot)
        {
            var repoRoot = GetRepoRoot();
            var reportPath = Path.Combine(repoRoot, ReportRelativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(reportPath) ?? repoRoot);

            using (var writer = new StreamWriter(reportPath, false))
            {
                writer.WriteLine("MYB-80 Unity HUD cadence validation");
                writer.WriteLine("Timestamp UTC: " + DateTime.UtcNow.ToString("O"));
                writer.WriteLine("Unity: " + Application.unityVersion);
                writer.WriteLine("Status: " + (failures.Count == 0 ? "PASS" : "FAIL"));
                writer.WriteLine("Simulated frames: " + SimulatedFrameCount);
                writer.WriteLine("Simulated duration: " + (SimulatedFrameCount * SimulatedDeltaSeconds).ToString("0.000") + " s");
                writer.WriteLine("Fast interval: " + snapshot.FastIntervalSeconds.ToString("0.000") + " s");
                writer.WriteLine("Slow interval: " + snapshot.SlowIntervalSeconds.ToString("0.000") + " s");
                writer.WriteLine("Fast refresh count: " + snapshot.FastRefreshCount);
                writer.WriteLine("Slow refresh count: " + snapshot.SlowRefreshCount);
                writer.WriteLine("Fast text mutations: " + snapshot.FastTextMutations);
                writer.WriteLine("Slow text mutations: " + snapshot.SlowTextMutations);
                writer.WriteLine("Progress after simulation: " + snapshot.ProgressMeters.ToString("0.0") + " m");
                writer.WriteLine("Visible HUD copy: " + snapshot.VisibleCopy);

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

        private static string GetRepoRoot()
        {
            var projectRoot = Directory.GetParent(Application.dataPath);
            if (projectRoot == null || projectRoot.Parent == null || projectRoot.Parent.Parent == null)
            {
                return projectRoot == null ? Application.dataPath : projectRoot.FullName;
            }

            return projectRoot.Parent.Parent.FullName;
        }

        private readonly struct HudCadenceSnapshot
        {
            public HudCadenceSnapshot(
                float fastIntervalSeconds,
                float slowIntervalSeconds,
                int fastRefreshCount,
                int slowRefreshCount,
                int fastTextMutations,
                int slowTextMutations,
                string visibleCopy,
                float progressMeters)
            {
                FastIntervalSeconds = fastIntervalSeconds;
                SlowIntervalSeconds = slowIntervalSeconds;
                FastRefreshCount = fastRefreshCount;
                SlowRefreshCount = slowRefreshCount;
                FastTextMutations = fastTextMutations;
                SlowTextMutations = slowTextMutations;
                VisibleCopy = visibleCopy;
                ProgressMeters = progressMeters;
            }

            public float FastIntervalSeconds { get; }
            public float SlowIntervalSeconds { get; }
            public int FastRefreshCount { get; }
            public int SlowRefreshCount { get; }
            public int FastTextMutations { get; }
            public int SlowTextMutations { get; }
            public string VisibleCopy { get; }
            public float ProgressMeters { get; }
        }
    }
}

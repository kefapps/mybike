using System;
using System.Text;
using MYB89;
using UnityEngine;
using UnityEngine.UI;

namespace MYB73
{
    public sealed class MYB73RoutePreviewPanel : MonoBehaviour
    {
        private const int MaxPassageCount = 3;

        [Header("Bindings")]
        public GameObject panelRoot;
        public MYB89ProbeRide ride;
        public Text titleLabel;
        public Text subtitleLabel;
        public Text statsLabel;
        public Text difficultyLabel;
        public Text biomesLabel;
        public Text passagesLabel;
        public Button launchButton;

        [Header("Editorial Route Metadata")]
        public string routeTitle = "L'Echappee des lumieres";
        [TextArea]
        public string routeSubtitle = "Foret claire, village pave et cote lumineuse pour une premiere balade premium.";
        public string overallDifficulty = "Moderee";
        public string[] biomes = { "Foret claire", "Village pave", "Cote lumineuse" };
        public string[] passages = { "Foret d'echauffement", "Montee lumineuse", "Sprint vers le village" };

        [Header("Calculated Stats")]
        public float referenceSpeedMetersPerSecond = 2.2f;

        public bool IsVisible => Root.activeSelf;

        public int PassageCount => CountEntries(passages, MaxPassageCount);

        public float RouteLengthMeters
        {
            get
            {
                if (ride == null)
                {
                    return 0f;
                }

                ride.RebuildRouteCache();
                return ride.RouteLength;
            }
        }

        public float EstimatedDurationSeconds =>
            RouteLengthMeters / Mathf.Max(0.1f, referenceSpeedMetersPerSecond);

        private GameObject Root => panelRoot == null ? gameObject : panelRoot;

        private void Awake()
        {
            WireLaunchButton();
        }

        private void Start()
        {
            ShowPreview();
        }

        private void OnValidate()
        {
            referenceSpeedMetersPerSecond = Mathf.Max(0.1f, referenceSpeedMetersPerSecond);
        }

        public void ShowPreview()
        {
            WireLaunchButton();
            ride?.PrepareRoutePreview();
            SetRideHudVisible(false);
            Root.SetActive(true);
            Refresh();
        }

        public void LaunchRide()
        {
            ride?.LaunchRoutePreviewRide();
            SetRideHudVisible(true);
            Root.SetActive(false);
        }

        public MYB73RoutePreviewSnapshot CaptureSnapshot()
        {
            return new MYB73RoutePreviewSnapshot(
                RouteLengthMeters,
                EstimatedDurationSeconds,
                overallDifficulty,
                JoinEntries(biomes, 0),
                JoinEntries(passages, MaxPassageCount),
                PassageCount);
        }

        public void Refresh()
        {
            var snapshot = CaptureSnapshot();

            if (titleLabel != null)
            {
                titleLabel.text = Clean(routeTitle, "L'Echappee des lumieres");
            }

            if (subtitleLabel != null)
            {
                subtitleLabel.text = Clean(routeSubtitle, string.Empty);
            }

            if (statsLabel != null)
            {
                statsLabel.text = $"{snapshot.RouteLengthMeters:0} m | env. {FormatDuration(snapshot.EstimatedDurationSeconds)} | difficulte {Clean(snapshot.OverallDifficulty, "Moderee").ToLowerInvariant()}";
            }

            if (difficultyLabel != null)
            {
                difficultyLabel.text = "Balade scenique, effort lisible";
            }

            if (biomesLabel != null)
            {
                biomesLabel.text = "Decors: " + Clean(snapshot.BiomesText, "Foret claire");
            }

            if (passagesLabel != null)
            {
                passagesLabel.text = "Passages: " + Clean(snapshot.PassagesText, "Foret d'echauffement");
            }
        }

        private void WireLaunchButton()
        {
            if (launchButton == null)
            {
                return;
            }

            launchButton.onClick.RemoveListener(LaunchRide);
            launchButton.onClick.AddListener(LaunchRide);
        }

        private void SetRideHudVisible(bool visible)
        {
            if (ride == null)
            {
                return;
            }

            SetLabelVisible(ride.distanceLabel, visible);
            SetLabelVisible(ride.speedLabel, visible);
            SetLabelVisible(ride.difficultyLabel, visible);
            SetLabelVisible(ride.gradeLabel, visible);
            SetLabelVisible(ride.segmentLabel, visible);
            SetLabelVisible(ride.verdictLabel, visible);
        }

        private static void SetLabelVisible(Text label, bool visible)
        {
            if (label != null)
            {
                label.gameObject.SetActive(visible);
            }
        }

        private static string FormatDuration(float seconds)
        {
            var roundedSeconds = Mathf.Max(1, Mathf.RoundToInt(seconds));
            var minutes = roundedSeconds / 60;
            var remainder = roundedSeconds % 60;
            if (minutes <= 0)
            {
                return remainder + " s";
            }

            return remainder == 0 ? minutes + " min" : minutes + " min " + remainder + " s";
        }

        private static string JoinEntries(string[] entries, int limit)
        {
            if (entries == null || entries.Length == 0)
            {
                return string.Empty;
            }

            var builder = new StringBuilder();
            var count = 0;
            for (var i = 0; i < entries.Length; i++)
            {
                var entry = Clean(entries[i], string.Empty);
                if (string.IsNullOrEmpty(entry))
                {
                    continue;
                }

                if (count > 0)
                {
                    builder.Append(" | ");
                }

                builder.Append(entry);
                count++;

                if (limit > 0 && count >= limit)
                {
                    break;
                }
            }

            return builder.ToString();
        }

        private static int CountEntries(string[] entries, int limit)
        {
            if (entries == null)
            {
                return 0;
            }

            var count = 0;
            for (var i = 0; i < entries.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(entries[i]))
                {
                    continue;
                }

                count++;
                if (limit > 0 && count >= limit)
                {
                    return count;
                }
            }

            return count;
        }

        private static string Clean(string value, string fallback)
        {
            return string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
        }
    }

    [Serializable]
    public readonly struct MYB73RoutePreviewSnapshot
    {
        public MYB73RoutePreviewSnapshot(
            float routeLengthMeters,
            float estimatedDurationSeconds,
            string overallDifficulty,
            string biomesText,
            string passagesText,
            int passageCount)
        {
            RouteLengthMeters = routeLengthMeters;
            EstimatedDurationSeconds = estimatedDurationSeconds;
            OverallDifficulty = overallDifficulty;
            BiomesText = biomesText;
            PassagesText = passagesText;
            PassageCount = passageCount;
        }

        public float RouteLengthMeters { get; }
        public float EstimatedDurationSeconds { get; }
        public string OverallDifficulty { get; }
        public string BiomesText { get; }
        public string PassagesText { get; }
        public int PassageCount { get; }
        public string MomentsClesText => PassagesText;
        public int MomentCleCount => PassageCount;
    }
}

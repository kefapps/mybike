using MyBike.Echappee3D.Core;
using MyBike.Echappee3D.Ride;
using UnityEngine;
using UnityEngine.UI;

namespace MyBike.Echappee3D.UI
{
    public sealed class HudController : MonoBehaviour
    {
        [SerializeField] private Text speedText;
        [SerializeField] private Text effortText;
        [SerializeField] private Text progressText;
        [SerializeField] private Text distanceText;
        [SerializeField] private Text timeText;
        [SerializeField] private Text sourceText;
        [SerializeField] private Text phaseText;
        [SerializeField] private Text summaryText;

        public void Render(RideFrameSnapshot snapshot, RideSummary? summary)
        {
            if (speedText != null)
            {
                speedText.text = $"Speed {snapshot.SpeedMps * 3.6f:0.0} km/h";
            }

            if (effortText != null)
            {
                effortText.text = $"Effort {snapshot.Input.Effort01 * 100f:0}%";
            }

            if (progressText != null)
            {
                progressText.text = $"Progress {snapshot.Route.Progress01 * 100f:0}%";
            }

            if (distanceText != null)
            {
                distanceText.text = $"Distance {snapshot.Route.DistanceMeters:0} m";
            }

            if (timeText != null)
            {
                timeText.text = $"Time {snapshot.ElapsedSeconds:0}s";
            }

            if (sourceText != null)
            {
                sourceText.text = $"Input {snapshot.Input.Source}";
            }

            if (phaseText != null)
            {
                phaseText.text = $"State {snapshot.Phase}";
            }

            if (summaryText != null)
            {
                summaryText.text = summary.HasValue
                    ? $"Summary {summary.Value.ElapsedSeconds:0}s | {summary.Value.DistanceMeters:0} m | avg {summary.Value.AverageSpeedMps * 3.6f:0.0} km/h"
                    : "Summary -";
            }
        }
    }
}

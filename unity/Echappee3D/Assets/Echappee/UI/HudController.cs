using MyBike.Echappee3D.Core;
using UnityEngine;
using UnityEngine.UI;

namespace MyBike.Echappee3D.UI
{
    public sealed class HudController : MonoBehaviour
    {
        [SerializeField] private Text speedText;
        [SerializeField] private Text distanceText;
        [SerializeField] private Text timeText;
        [SerializeField] private Text sourceText;
        [SerializeField] private Text phaseText;

        public void Render(RideFrameSnapshot snapshot)
        {
            if (speedText != null)
            {
                speedText.text = $"{snapshot.SpeedMps * 3.6f:0.0} km/h";
            }

            if (distanceText != null)
            {
                distanceText.text = $"{snapshot.Route.DistanceMeters:0} m";
            }

            if (timeText != null)
            {
                timeText.text = $"{snapshot.ElapsedSeconds:0}s";
            }

            if (sourceText != null)
            {
                sourceText.text = "mock";
            }

            if (phaseText != null)
            {
                phaseText.text = snapshot.Phase.ToString();
            }
        }
    }
}

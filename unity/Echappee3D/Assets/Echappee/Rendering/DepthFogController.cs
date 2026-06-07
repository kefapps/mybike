using UnityEngine;

namespace MyBike.Echappee3D.Rendering
{
    public sealed class DepthFogController : MonoBehaviour
    {
        [SerializeField] private Color fogColor = new Color(0.62f, 0.72f, 0.78f);
        [SerializeField] private float fogStartDistance = 45f;
        [SerializeField] private float fogEndDistance = 220f;

        public void ApplyDefaultFog()
        {
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = fogColor;
            RenderSettings.fogStartDistance = fogStartDistance;
            RenderSettings.fogEndDistance = fogEndDistance;
        }
    }
}

using UnityEngine;

namespace MYB95
{
    [DisallowMultipleComponent]
    public sealed class MYB95RelicFountainPulse : MonoBehaviour
    {
        private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

        [SerializeField] private Light[] pulseLights = System.Array.Empty<Light>();
        [SerializeField] private Renderer[] emissiveRenderers = System.Array.Empty<Renderer>();
        [SerializeField] private Transform[] orbitingRunes = System.Array.Empty<Transform>();
        [SerializeField] private Color energyColor = new Color(0.08f, 0.58f, 1f, 1f);
        [SerializeField] private float baseIntensity = 1.25f;
        [SerializeField] private float intensityJitter = 0.36f;
        [SerializeField] private float pulseSpeed = 1.15f;
        [SerializeField] private float orbitSpeedDegrees = 18f;
        [SerializeField] private float emissionMultiplier = 1.8f;
        [SerializeField] private float emissionJitter = 0.55f;

        private MaterialPropertyBlock propertyBlock;

        public float BaseIntensity => baseIntensity;
        public float IntensityJitter => intensityJitter;

        public void Configure(Light[] lights, Renderer[] renderers, Transform[] runes)
        {
            pulseLights = lights ?? System.Array.Empty<Light>();
            emissiveRenderers = renderers ?? System.Array.Empty<Renderer>();
            orbitingRunes = runes ?? System.Array.Empty<Transform>();
            ApplyPreview(0f);
        }

        public void ApplyPreview(float timeSeconds)
        {
            EnsureState();

            var pulse = Mathf.Sin(timeSeconds * Mathf.Max(0.01f, pulseSpeed) * Mathf.PI * 2f) * 0.5f + 0.5f;
            var signedPulse = (pulse - 0.5f) * 2f;

            foreach (var pulseLight in pulseLights)
            {
                if (pulseLight == null)
                {
                    continue;
                }

                pulseLight.intensity = Mathf.Max(0f, baseIntensity + signedPulse * intensityJitter);
            }

            var emissionColor = energyColor * Mathf.Max(0f, emissionMultiplier + signedPulse * emissionJitter);
            foreach (var emissiveRenderer in emissiveRenderers)
            {
                if (emissiveRenderer == null)
                {
                    continue;
                }

                emissiveRenderer.GetPropertyBlock(propertyBlock);
                propertyBlock.SetColor(EmissionColorId, emissionColor);
                emissiveRenderer.SetPropertyBlock(propertyBlock);
            }

            foreach (var rune in orbitingRunes)
            {
                if (rune == null)
                {
                    continue;
                }

                rune.localRotation = Quaternion.Euler(0f, timeSeconds * orbitSpeedDegrees, 0f);
            }
        }

        private void Awake()
        {
            EnsureState();
        }

        private void Reset()
        {
            pulseLights = GetComponentsInChildren<Light>();
            emissiveRenderers = GetComponentsInChildren<Renderer>();
            ApplyPreview(0f);
        }

        private void OnValidate()
        {
            baseIntensity = Mathf.Max(0f, baseIntensity);
            intensityJitter = Mathf.Max(0f, intensityJitter);
            pulseSpeed = Mathf.Max(0.01f, pulseSpeed);
            emissionMultiplier = Mathf.Max(0f, emissionMultiplier);
            emissionJitter = Mathf.Max(0f, emissionJitter);
            ApplyPreview(0.37f);
        }

        private void Update()
        {
            ApplyPreview(Time.time);
        }

        private void EnsureState()
        {
            if (propertyBlock == null)
            {
                propertyBlock = new MaterialPropertyBlock();
            }
        }
    }
}

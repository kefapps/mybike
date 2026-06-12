using UnityEngine;

namespace MYB95
{
    [DisallowMultipleComponent]
    public sealed class MYB95LanternFlicker : MonoBehaviour
    {
        private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

        [SerializeField] private Light targetLight;
        [SerializeField] private Renderer[] emissiveRenderers = System.Array.Empty<Renderer>();
        [SerializeField] private Color flameColor = new Color(1f, 0.58f, 0.22f, 1f);
        [SerializeField] private float baseIntensity = 1.65f;
        [SerializeField] private float intensityJitter = 0.42f;
        [SerializeField] private float baseRange = 3.15f;
        [SerializeField] private float rangeJitter = 0.28f;
        [SerializeField] private float flickerSpeed = 2.9f;
        [SerializeField] private float emissionMultiplier = 2.4f;
        [SerializeField] private float emissionJitter = 0.85f;

        private MaterialPropertyBlock propertyBlock;
        private float seed;

        public Light TargetLight => targetLight;
        public float BaseIntensity => baseIntensity;
        public float IntensityJitter => intensityJitter;

        public void Configure(Light light, Renderer[] renderers)
        {
            targetLight = light;
            emissiveRenderers = renderers ?? System.Array.Empty<Renderer>();
            ApplyPreview(0f);
        }

        public void ApplyPreview(float timeSeconds)
        {
            EnsureState();

            var speed = Mathf.Max(0.01f, flickerSpeed);
            var noise = Mathf.PerlinNoise(seed, timeSeconds * speed);
            var pulse = Mathf.Sin((timeSeconds + seed) * speed * 4.73f) * 0.5f + 0.5f;
            var flicker = Mathf.Clamp01(noise * 0.72f + pulse * 0.28f);
            var signedFlicker = (flicker - 0.5f) * 2f;

            if (targetLight != null)
            {
                targetLight.intensity = Mathf.Max(0f, baseIntensity + signedFlicker * intensityJitter);
                targetLight.range = Mathf.Max(0.1f, baseRange + signedFlicker * rangeJitter);
            }

            var emissionStrength = Mathf.Max(0f, emissionMultiplier + signedFlicker * emissionJitter);
            var emissionColor = flameColor * emissionStrength;

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
        }

        private void Awake()
        {
            seed = Mathf.Abs(StableHash(name) + transform.GetSiblingIndex() * 37) * 0.001f;
            EnsureState();
        }

        private void Reset()
        {
            targetLight = GetComponentInChildren<Light>();
            emissiveRenderers = GetComponentsInChildren<Renderer>();
            ApplyPreview(0f);
        }

        private void OnValidate()
        {
            baseIntensity = Mathf.Max(0f, baseIntensity);
            intensityJitter = Mathf.Max(0f, intensityJitter);
            baseRange = Mathf.Max(0.1f, baseRange);
            rangeJitter = Mathf.Max(0f, rangeJitter);
            flickerSpeed = Mathf.Max(0.01f, flickerSpeed);
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

            if (targetLight == null)
            {
                targetLight = GetComponentInChildren<Light>();
            }
        }

        private static int StableHash(string value)
        {
            unchecked
            {
                var hash = 17;
                for (var index = 0; index < value.Length; index++)
                {
                    hash = hash * 31 + value[index];
                }

                return hash;
            }
        }
    }
}

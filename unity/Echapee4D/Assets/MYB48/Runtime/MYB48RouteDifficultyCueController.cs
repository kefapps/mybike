using System;
using System.Collections.Generic;
using UnityEngine;

namespace MYB48
{
    public enum MYB48RouteDifficultyCueKind
    {
        Climb,
        Sprint,
        Recovery
    }

    [ExecuteAlways]
    public sealed class MYB48RouteDifficultyCueController : MonoBehaviour
    {
        public Transform[] routeMarkers = Array.Empty<Transform>();

        [Header("Materials")]
        public Material climbMaterial;
        public Material sprintMaterial;
        public Material recoveryMaterial;

        [Header("Placement")]
        public float lateralOffsetMeters = 5.4f;
        public float entryHeightMeters = 2.8f;
        public float secondaryHeightMeters = 1.25f;

        [Header("Pulse")]
        public float pulseAmplitude = 0.22f;
        public float pulseFrequency = 0.85f;

        [SerializeField] private int generatedCueCount;
        [SerializeField] private int climbCueCount;
        [SerializeField] private int sprintCueCount;
        [SerializeField] private int recoveryCueCount;

        private readonly List<CueRenderer> cueRenderers = new List<CueRenderer>();
        private readonly List<CueLight> cueLights = new List<CueLight>();
        private MaterialPropertyBlock propertyBlock;

        public int GeneratedCueCount => generatedCueCount;
        public int ClimbCueCount => climbCueCount;
        public int SprintCueCount => sprintCueCount;
        public int RecoveryCueCount => recoveryCueCount;

        private void OnEnable()
        {
            CacheGeneratedCues();
        }

        private void OnValidate()
        {
            lateralOffsetMeters = Mathf.Max(3.8f, lateralOffsetMeters);
            entryHeightMeters = Mathf.Clamp(entryHeightMeters, 1.8f, 4.2f);
            secondaryHeightMeters = Mathf.Clamp(secondaryHeightMeters, 0.7f, 2.2f);
            pulseAmplitude = Mathf.Clamp(pulseAmplitude, 0f, 0.45f);
            pulseFrequency = Mathf.Clamp(pulseFrequency, 0.1f, 2.5f);
        }

        private void Update()
        {
            if (cueRenderers.Count == 0 && cueLights.Count == 0)
            {
                CacheGeneratedCues();
            }

            AnimateCues(Time.time);
        }

        public void RebuildCues()
        {
            ClearGeneratedChildren();

            generatedCueCount = 0;
            climbCueCount = 0;
            sprintCueCount = 0;
            recoveryCueCount = 0;

            var routeLength = RouteLength();
            if (routeLength <= 0.01f || routeMarkers == null || routeMarkers.Length < 2)
            {
                CacheGeneratedCues();
                return;
            }

            BuildCueBand(MYB48RouteDifficultyCueKind.Climb, 0.26f, 0.56f, routeLength);
            BuildCueBand(MYB48RouteDifficultyCueKind.Sprint, 0.56f, 0.84f, routeLength);
            BuildCueBand(MYB48RouteDifficultyCueKind.Recovery, 0.84f, 0.98f, routeLength);
            CacheGeneratedCues();
        }

        private void BuildCueBand(MYB48RouteDifficultyCueKind kind, float start01, float end01, float routeLength)
        {
            var material = MaterialFor(kind);
            var color = ColorFor(kind);
            var prefix = kind.ToString();

            if (TrySampleRoute(start01 * routeLength, out var entryPosition, out var forward, out var right))
            {
                CreateEntryGate(prefix, entryPosition, forward, right, material, color, kind);
            }

            var secondaryCount = kind == MYB48RouteDifficultyCueKind.Recovery ? 2 : 3;
            for (var i = 0; i < secondaryCount; i++)
            {
                var t = (i + 1f) / (secondaryCount + 1f);
                var progress01 = Mathf.Lerp(start01, end01, t);
                if (!TrySampleRoute(progress01 * routeLength, out var position, out forward, out right))
                {
                    continue;
                }

                var side = i % 2 == 0 ? -1f : 1f;
                CreateSecondaryBeacon(prefix + "_Beacon_" + (i + 1).ToString("00"), position, forward, right * side, material, color, kind);
            }
        }

        private void CreateEntryGate(
            string prefix,
            Vector3 position,
            Vector3 forward,
            Vector3 right,
            Material material,
            Color color,
            MYB48RouteDifficultyCueKind kind)
        {
            var rotation = Quaternion.LookRotation(forward, Vector3.up);
            var leftPost = position - right * lateralOffsetMeters + Vector3.up * (entryHeightMeters * 0.5f);
            var rightPost = position + right * lateralOffsetMeters + Vector3.up * (entryHeightMeters * 0.5f);
            var top = position + Vector3.up * entryHeightMeters;

            CreatePrimitive(prefix + "_Entry_LeftPost", PrimitiveType.Cylinder, leftPost, rotation, new Vector3(0.22f, entryHeightMeters * 0.5f, 0.22f), material, kind);
            CreatePrimitive(prefix + "_Entry_RightPost", PrimitiveType.Cylinder, rightPost, rotation, new Vector3(0.22f, entryHeightMeters * 0.5f, 0.22f), material, kind);
            CreatePrimitive(prefix + "_Entry_TopRune", PrimitiveType.Cube, top, rotation, new Vector3(lateralOffsetMeters * 2.05f, 0.13f, 0.2f), material, kind);

            var lightObject = new GameObject("MYB48_Cue_" + prefix + "_EntryGlow");
            lightObject.transform.SetParent(transform);
            lightObject.transform.SetPositionAndRotation(top, Quaternion.identity);
            var point = lightObject.AddComponent<Light>();
            point.type = LightType.Point;
            point.color = color;
            point.intensity = kind == MYB48RouteDifficultyCueKind.Sprint ? 1.35f : 1.05f;
            point.range = kind == MYB48RouteDifficultyCueKind.Recovery ? 7f : 8.5f;

            RegisterCue(kind);
        }

        private void CreateSecondaryBeacon(
            string name,
            Vector3 position,
            Vector3 forward,
            Vector3 side,
            Material material,
            Color color,
            MYB48RouteDifficultyCueKind kind)
        {
            var beaconPosition = position + side.normalized * lateralOffsetMeters + Vector3.up * (secondaryHeightMeters * 0.5f);
            var rotation = Quaternion.LookRotation(forward, Vector3.up) * Quaternion.Euler(0f, 45f, 0f);
            CreatePrimitive(name, PrimitiveType.Cube, beaconPosition, rotation, new Vector3(0.46f, secondaryHeightMeters, 0.46f), material, kind);

            var cap = CreatePrimitive(name + "_GlowCap", PrimitiveType.Sphere, beaconPosition + Vector3.up * (secondaryHeightMeters * 0.62f), Quaternion.identity, Vector3.one * 0.34f, material, kind);
            var capRenderer = cap.GetComponent<Renderer>();
            if (capRenderer != null)
            {
                capRenderer.sharedMaterial = material;
            }

            RegisterCue(kind);
        }

        private GameObject CreatePrimitive(
            string name,
            PrimitiveType primitiveType,
            Vector3 position,
            Quaternion rotation,
            Vector3 scale,
            Material material,
            MYB48RouteDifficultyCueKind kind)
        {
            var gameObject = GameObject.CreatePrimitive(primitiveType);
            gameObject.name = "MYB48_Cue_" + name;
            gameObject.transform.SetParent(transform);
            gameObject.transform.SetPositionAndRotation(position, rotation);
            gameObject.transform.localScale = scale;

            var renderer = gameObject.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = material;
            }

            var collider = gameObject.GetComponent<Collider>();
            if (collider != null)
            {
                DestroyImmediateSafe(collider);
            }

            var tag = gameObject.AddComponent<MYB48RouteDifficultyCueTag>();
            tag.kind = kind;
            return gameObject;
        }

        private void RegisterCue(MYB48RouteDifficultyCueKind kind)
        {
            generatedCueCount++;
            switch (kind)
            {
                case MYB48RouteDifficultyCueKind.Climb:
                    climbCueCount++;
                    break;
                case MYB48RouteDifficultyCueKind.Sprint:
                    sprintCueCount++;
                    break;
                case MYB48RouteDifficultyCueKind.Recovery:
                    recoveryCueCount++;
                    break;
            }
        }

        private void CacheGeneratedCues()
        {
            cueRenderers.Clear();
            cueLights.Clear();

            var renderers = GetComponentsInChildren<Renderer>(true);
            foreach (var renderer in renderers)
            {
                var tag = renderer.GetComponent<MYB48RouteDifficultyCueTag>();
                if (tag == null)
                {
                    continue;
                }

                cueRenderers.Add(new CueRenderer(renderer, ColorFor(tag.kind), PhaseFor(tag.kind), IntensityFor(tag.kind)));
            }

            var lights = GetComponentsInChildren<Light>(true);
            foreach (var cueLight in lights)
            {
                var kind = KindFromName(cueLight.gameObject.name);
                cueLights.Add(new CueLight(cueLight, PhaseFor(kind), cueLight.intensity));
            }
        }

        private void AnimateCues(float time)
        {
            if (propertyBlock == null)
            {
                propertyBlock = new MaterialPropertyBlock();
            }

            for (var i = 0; i < cueRenderers.Count; i++)
            {
                var cue = cueRenderers[i];
                if (cue.Renderer == null)
                {
                    continue;
                }

                var pulse = 1f + Mathf.Sin(time * Mathf.PI * 2f * pulseFrequency + cue.Phase) * pulseAmplitude;
                var emissive = cue.Color * (cue.Intensity * pulse);
                cue.Renderer.GetPropertyBlock(propertyBlock);
                propertyBlock.SetColor("_BaseColor", Color.Lerp(cue.Color, Color.white, 0.08f));
                propertyBlock.SetColor("_Color", Color.Lerp(cue.Color, Color.white, 0.08f));
                propertyBlock.SetColor("_EmissionColor", emissive);
                cue.Renderer.SetPropertyBlock(propertyBlock);
            }

            for (var i = 0; i < cueLights.Count; i++)
            {
                var cue = cueLights[i];
                if (cue.Light == null)
                {
                    continue;
                }

                var pulse = 1f + Mathf.Sin(time * Mathf.PI * 2f * pulseFrequency + cue.Phase) * (pulseAmplitude * 0.7f);
                cue.Light.intensity = cue.BaseIntensity * pulse;
            }
        }

        private Material MaterialFor(MYB48RouteDifficultyCueKind kind)
        {
            switch (kind)
            {
                case MYB48RouteDifficultyCueKind.Climb:
                    return climbMaterial;
                case MYB48RouteDifficultyCueKind.Sprint:
                    return sprintMaterial;
                case MYB48RouteDifficultyCueKind.Recovery:
                    return recoveryMaterial;
                default:
                    return climbMaterial;
            }
        }

        private static Color ColorFor(MYB48RouteDifficultyCueKind kind)
        {
            switch (kind)
            {
                case MYB48RouteDifficultyCueKind.Climb:
                    return new Color(1f, 0.58f, 0.24f, 1f);
                case MYB48RouteDifficultyCueKind.Sprint:
                    return new Color(1f, 0.18f, 0.16f, 1f);
                case MYB48RouteDifficultyCueKind.Recovery:
                    return new Color(0.28f, 0.78f, 1f, 1f);
                default:
                    return Color.white;
            }
        }

        private static float IntensityFor(MYB48RouteDifficultyCueKind kind)
        {
            switch (kind)
            {
                case MYB48RouteDifficultyCueKind.Sprint:
                    return 2.35f;
                case MYB48RouteDifficultyCueKind.Climb:
                    return 1.85f;
                case MYB48RouteDifficultyCueKind.Recovery:
                    return 1.35f;
                default:
                    return 1f;
            }
        }

        private static float PhaseFor(MYB48RouteDifficultyCueKind kind)
        {
            switch (kind)
            {
                case MYB48RouteDifficultyCueKind.Climb:
                    return 0.2f;
                case MYB48RouteDifficultyCueKind.Sprint:
                    return 1.3f;
                case MYB48RouteDifficultyCueKind.Recovery:
                    return 2.4f;
                default:
                    return 0f;
            }
        }

        private static MYB48RouteDifficultyCueKind KindFromName(string objectName)
        {
            if (objectName.Contains("Sprint"))
            {
                return MYB48RouteDifficultyCueKind.Sprint;
            }

            if (objectName.Contains("Recovery"))
            {
                return MYB48RouteDifficultyCueKind.Recovery;
            }

            return MYB48RouteDifficultyCueKind.Climb;
        }

        private float RouteLength()
        {
            if (routeMarkers == null || routeMarkers.Length < 2)
            {
                return 0f;
            }

            var length = 0f;
            for (var i = 0; i < routeMarkers.Length - 1; i++)
            {
                var a = routeMarkers[i];
                var b = routeMarkers[i + 1];
                if (a == null || b == null)
                {
                    continue;
                }

                length += Vector3.Distance(a.position, b.position);
            }

            return length;
        }

        private bool TrySampleRoute(float meters, out Vector3 position, out Vector3 forward, out Vector3 right)
        {
            position = Vector3.zero;
            forward = Vector3.forward;
            right = Vector3.right;

            var routeLength = RouteLength();
            if (routeMarkers == null || routeMarkers.Length < 2 || routeLength <= 0.01f)
            {
                return false;
            }

            var wrappedMeters = Mathf.Clamp(meters, 0f, routeLength);
            var cursor = 0f;

            for (var i = 0; i < routeMarkers.Length - 1; i++)
            {
                var aTransform = routeMarkers[i];
                var bTransform = routeMarkers[i + 1];
                if (aTransform == null || bTransform == null)
                {
                    continue;
                }

                var a = aTransform.position;
                var b = bTransform.position;
                var segmentLength = Vector3.Distance(a, b);
                if (segmentLength <= 0.01f)
                {
                    continue;
                }

                if (wrappedMeters <= cursor + segmentLength || i == routeMarkers.Length - 2)
                {
                    var t = Mathf.InverseLerp(cursor, cursor + segmentLength, wrappedMeters);
                    position = Vector3.Lerp(a, b, Mathf.SmoothStep(0f, 1f, t));
                    forward = (b - a).normalized;
                    right = Vector3.Cross(Vector3.up, forward).normalized;
                    return true;
                }

                cursor += segmentLength;
            }

            return false;
        }

        private void ClearGeneratedChildren()
        {
            for (var i = transform.childCount - 1; i >= 0; i--)
            {
                DestroyImmediateSafe(transform.GetChild(i).gameObject);
            }

            cueRenderers.Clear();
            cueLights.Clear();
        }

        private static void DestroyImmediateSafe(UnityEngine.Object target)
        {
            if (Application.isPlaying)
            {
                Destroy(target);
            }
            else
            {
                DestroyImmediate(target);
            }
        }

        private readonly struct CueRenderer
        {
            public CueRenderer(Renderer renderer, Color color, float phase, float intensity)
            {
                Renderer = renderer;
                Color = color;
                Phase = phase;
                Intensity = intensity;
            }

            public Renderer Renderer { get; }
            public Color Color { get; }
            public float Phase { get; }
            public float Intensity { get; }
        }

        private readonly struct CueLight
        {
            public CueLight(Light light, float phase, float baseIntensity)
            {
                Light = light;
                Phase = phase;
                BaseIntensity = baseIntensity;
            }

            public Light Light { get; }
            public float Phase { get; }
            public float BaseIntensity { get; }
        }
    }

    public sealed class MYB48RouteDifficultyCueTag : MonoBehaviour
    {
        public MYB48RouteDifficultyCueKind kind;
    }
}

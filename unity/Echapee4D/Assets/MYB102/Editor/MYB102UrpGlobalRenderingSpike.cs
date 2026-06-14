using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using MYB89;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace MYB102.Editor
{
    public static class MYB102UrpGlobalRenderingSpike
    {
        private const string CanonicalScenePath = "Assets/Scenes/MYB89UnityMcpProbe.unity";
        private const string PcRpAssetPath = "Assets/Settings/PC_RPAsset.asset";
        private const string PcRendererPath = "Assets/Settings/PC_Renderer.asset";
        private const string SampleSceneProfilePath = "Assets/Settings/SampleSceneProfile.asset";
        private const string OutputDirectory = "_bmad-output/unity-test-results/myb-102";
        private const string ReportRelativePath = OutputDirectory + "/myb-102-urp-global-rendering-spike.txt";

        private static readonly PassageSpec[] Passages =
        {
            new PassageSpec("passage-01-foret-claire", "Foret claire", 0.16f),
            new PassageSpec("passage-02-village-route-de-col", "Village / route de col", 0.50f),
            new PassageSpec("passage-03-panorama-signal-fantasy", "Panorama / signal fantasy", 0.84f)
        };

        [MenuItem("Tools/MYB-102/Capture URP Global Rendering Spike")]
        public static string CaptureUrpGlobalRenderingSpikeCli()
        {
            var reportPath = CaptureUrpGlobalRenderingSpike();
            Debug.Log("MYB-102 URP global rendering spike captured: " + reportPath);
            return reportPath;
        }

        public static string CaptureUrpGlobalRenderingSpike()
        {
            EditorSceneManager.OpenScene(CanonicalScenePath, OpenSceneMode.Single);

            var report = new SpikeReport();
            var state = new TemporarySpikeState(report);
            AssetDatabase.ImportAsset(PcRpAssetPath, ImportAssetOptions.ForceUpdate);
            AssetDatabase.ImportAsset(PcRendererPath, ImportAssetOptions.ForceUpdate);
            var pcRpAsset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(PcRpAssetPath);
            var pcRenderer = AssetDatabase.LoadAssetAtPath<ScriptableObject>(PcRendererPath);
            var pcSsaoFeature = LoadRendererFeature(PcRendererPath, "ScreenSpaceAmbientOcclusion");
            var sampleSceneProfile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(SampleSceneProfilePath);

            if (pcRpAsset == null)
            {
                report.Failures.Add("Missing PC URP asset: " + PcRpAssetPath);
            }

            if (pcRenderer == null)
            {
                report.Failures.Add("Missing PC renderer asset: " + PcRendererPath);
            }
            else if (pcSsaoFeature == null)
            {
                report.Warnings.Add("Missing ScreenSpaceAmbientOcclusion renderer feature in " + PcRendererPath);
            }

            if (sampleSceneProfile == null)
            {
                report.Warnings.Add("Missing sample scene volume profile: " + SampleSceneProfilePath);
            }

            var ride = UnityEngine.Object.FindAnyObjectByType<MYB89ProbeRide>();
            var camera = Camera.main;
            if (ride == null)
            {
                report.Failures.Add("Missing MYB89ProbeRide in canonical scene.");
            }

            if (camera == null)
            {
                report.Failures.Add("Missing Main Camera in canonical scene.");
            }

            if (report.Failures.Count == 0 && ride != null && camera != null && pcRpAsset != null && pcRenderer != null)
            {
                ride.RebuildRouteCache();
                NormalizeProjectAssetsFromDisk(pcRpAsset, pcSsaoFeature);
                report.InitialSettings = CaptureSettingsSummary(pcRpAsset, pcRenderer, pcSsaoFeature);

                var variants = BuildVariants(pcRpAsset, pcRenderer, pcSsaoFeature, sampleSceneProfile);
                var canvasStates = DisableCanvases();
                try
                {
                    foreach (var variant in variants)
                    {
                        state.RestoreVariantState();
                        variant.Apply(state, ride);
                        CaptureVariant(variant, ride, camera, report);
                    }
                }
                finally
                {
                    RestoreCanvases(canvasStates);
                    state.RestoreAll();
                    EditorSceneManager.OpenScene(CanonicalScenePath, OpenSceneMode.Single);
                }

                report.RestorationStatus = state.HasUnrestoredChanges ? "FAIL" : "PASS";
                if (state.HasUnrestoredChanges)
                {
                    report.Failures.Add("Temporary project rendering changes were not fully restored.");
                }
            }

            var absoluteReportPath = Path.Combine(GetRepoRoot(), ReportRelativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(absoluteReportPath) ?? GetRepoRoot());
            File.WriteAllLines(absoluteReportPath, BuildReportLines(report));

            if (report.Failures.Count > 0)
            {
                throw new InvalidOperationException("MYB-102 URP global rendering spike failed. See " + absoluteReportPath);
            }

            return absoluteReportPath;
        }

        private static IReadOnlyCollection<VariantSpec> BuildVariants(
            ScriptableObject pcRpAsset,
            ScriptableObject pcRenderer,
            UnityEngine.Object pcSsaoFeature,
            VolumeProfile sampleSceneProfile)
        {
            return new[]
            {
                new VariantSpec(
                    "baseline",
                    "Baseline actuelle",
                    "Current Socle de Rendu Projet, without temporary overrides.",
                    "Baseline",
                    (state, ride) => { }),
                new VariantSpec(
                    "project-depth",
                    "Project Depth",
                    "Temporary project-depth pass: stronger shadow range, MSAA, and SSAO depth separation.",
                    "Project defaults",
                    (state, ride) =>
                    {
                        state.SetSerializedInt(pcRpAsset, "m_MSAA", 4);
                        state.SetSerializedFloat(pcRpAsset, "m_ShadowDistance", 70f);
                        state.SetSerializedInt(pcRpAsset, "m_MainLightShadowmapResolution", 4096);
                        state.SetSerializedFloat(pcSsaoFeature, "m_Settings.Intensity", 0.68f);
                        state.SetSerializedFloat(pcSsaoFeature, "m_Settings.Radius", 0.46f);
                        state.SetSerializedInt(pcSsaoFeature, "m_Settings.Samples", 2);
                        state.SetSerializedInt(pcSsaoFeature, "m_Settings.BlurQuality", 1);
                    }),
                new VariantSpec(
                    "polish-volume",
                    "Polish Volume",
                    "Temporary global volume using the existing sample tone/bloom/vignette profile.",
                    "Post-process baseline",
                    (state, ride) =>
                    {
                        if (sampleSceneProfile == null)
                        {
                            state.Report.Warnings.Add("Polish Volume skipped because " + SampleSceneProfilePath + " is missing.");
                            return;
                        }

                        var volumeObject = new GameObject("MYB102_TemporaryGlobalPolishVolume");
                        var volume = volumeObject.AddComponent<Volume>();
                        volume.isGlobal = true;
                        volume.priority = 250f;
                        volume.weight = 0.72f;
                        volume.sharedProfile = sampleSceneProfile;
                        state.RegisterTemporaryObject(volumeObject);
                    }),
                new VariantSpec(
                    "probe-ambient",
                    "Probe/Ambient",
                    "Temporary ambient/probe pass: warmer trilight ambient, light fog depth, and local reflection probes.",
                    "Probe strategy",
                    (state, ride) =>
                    {
                        state.SetAmbient(
                            AmbientMode.Trilight,
                            new Color(0.64f, 0.70f, 0.82f),
                            new Color(0.54f, 0.50f, 0.42f),
                            new Color(0.28f, 0.24f, 0.20f),
                            true,
                            FogMode.ExponentialSquared,
                            new Color(0.56f, 0.63f, 0.72f),
                            0.0065f);

                        foreach (var passage in Passages)
                        {
                            var meters = Mathf.Max(1f, ride.RouteLength) * Mathf.Clamp01(passage.Progress01);
                            ride.SetPreviewProgress(meters);
                            var probeObject = new GameObject("MYB102_TemporaryReflectionProbe_" + passage.Slug);
                            probeObject.transform.position = ride.transform.position + Vector3.up * 3.5f;
                            var probe = probeObject.AddComponent<ReflectionProbe>();
                            probe.mode = UnityEngine.Rendering.ReflectionProbeMode.Realtime;
                            probe.refreshMode = UnityEngine.Rendering.ReflectionProbeRefreshMode.OnAwake;
                            probe.timeSlicingMode = UnityEngine.Rendering.ReflectionProbeTimeSlicingMode.IndividualFaces;
                            probe.size = new Vector3(36f, 18f, 36f);
                            probe.intensity = 0.45f;
                            probe.resolution = 64;
                            probe.boxProjection = true;
                            state.RegisterTemporaryObject(probeObject);
                        }
                    })
            };
        }

        private static void CaptureVariant(
            VariantSpec variant,
            MYB89ProbeRide ride,
            Camera camera,
            SpikeReport report)
        {
            var variantReport = new VariantReport(variant.Slug, variant.Label, variant.Family, variant.Description);
            var routeLength = Mathf.Max(1f, ride.RouteLength);
            foreach (var passage in Passages)
            {
                var meters = routeLength * Mathf.Clamp01(passage.Progress01);
                ride.SetPreviewProgress(meters);
                Canvas.ForceUpdateCanvases();

                var relativePath = OutputDirectory + "/" + variant.Slug + "/" + passage.Slug + ".png";
                var absolutePath = Path.Combine(GetRepoRoot(), relativePath);
                var imageMetrics = RenderCameraToPng(camera, 1280, 720, absolutePath);
                var frustumRendererCount = CountFrustumRenderers(camera);
                variantReport.Captures.Add(new CaptureReport(
                    passage.Slug,
                    passage.Label,
                    relativePath,
                    meters,
                    routeLength,
                    camera.transform.position,
                    camera.transform.forward,
                    frustumRendererCount,
                    imageMetrics));
            }

            variantReport.Metrics = CaptureSceneMetrics();
            report.Variants.Add(variantReport);
        }

        private static UnityEngine.Object LoadRendererFeature(string rendererPath, string featureName)
        {
            return AssetDatabase.LoadAllAssetsAtPath(rendererPath)
                .FirstOrDefault(asset => asset != null && string.Equals(asset.name, featureName, StringComparison.Ordinal));
        }

        private static void NormalizeProjectAssetsFromDisk(ScriptableObject pcRpAsset, UnityEngine.Object pcSsaoFeature)
        {
            SetSerializedIntDirect(pcRpAsset, "m_MSAA", ReadIntFromAssetFile(PcRpAssetPath, "m_MSAA", 1));
            SetSerializedIntDirect(pcRpAsset, "m_MainLightShadowmapResolution", ReadIntFromAssetFile(PcRpAssetPath, "m_MainLightShadowmapResolution", 2048));
            SetSerializedFloatDirect(pcRpAsset, "m_ShadowDistance", ReadFloatFromAssetFile(PcRpAssetPath, "m_ShadowDistance", 50f));

            if (pcSsaoFeature != null)
            {
                SetSerializedFloatDirect(pcSsaoFeature, "m_Settings.Intensity", ReadFloatFromAssetFile(PcRendererPath, "Intensity", 0.4f));
                SetSerializedFloatDirect(pcSsaoFeature, "m_Settings.Radius", ReadFloatFromAssetFile(PcRendererPath, "Radius", 0.3f));
                SetSerializedIntDirect(pcSsaoFeature, "m_Settings.Samples", ReadIntFromAssetFile(PcRendererPath, "Samples", 1));
                SetSerializedIntDirect(pcSsaoFeature, "m_Settings.BlurQuality", ReadIntFromAssetFile(PcRendererPath, "BlurQuality", 0));
            }
        }

        private static int ReadIntFromAssetFile(string assetPath, string yamlKey, int fallback)
        {
            var rawValue = ReadScalarFromAssetFile(assetPath, yamlKey);
            return int.TryParse(rawValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value) ? value : fallback;
        }

        private static float ReadFloatFromAssetFile(string assetPath, string yamlKey, float fallback)
        {
            var rawValue = ReadScalarFromAssetFile(assetPath, yamlKey);
            return float.TryParse(rawValue, NumberStyles.Float, CultureInfo.InvariantCulture, out var value) ? value : fallback;
        }

        private static string ReadScalarFromAssetFile(string assetPath, string yamlKey)
        {
            var absolutePath = Path.Combine(Directory.GetParent(Application.dataPath)?.FullName ?? Application.dataPath, assetPath);
            if (!File.Exists(absolutePath))
            {
                return string.Empty;
            }

            var prefix = yamlKey + ":";
            foreach (var line in File.ReadLines(absolutePath))
            {
                var trimmed = line.Trim();
                if (trimmed.StartsWith(prefix, StringComparison.Ordinal))
                {
                    return trimmed.Substring(prefix.Length).Trim();
                }
            }

            return string.Empty;
        }

        private static void SetSerializedIntDirect(UnityEngine.Object target, string path, int value)
        {
            SetSerializedDirect(target, path, property => property.intValue = value);
        }

        private static void SetSerializedFloatDirect(UnityEngine.Object target, string path, float value)
        {
            SetSerializedDirect(target, path, property => property.floatValue = value);
        }

        private static void SetSerializedDirect(UnityEngine.Object target, string path, Action<SerializedProperty> setValue)
        {
            if (target == null)
            {
                return;
            }

            var serializedObject = new SerializedObject(target);
            var property = serializedObject.FindProperty(path);
            if (property == null)
            {
                return;
            }

            setValue(property);
            serializedObject.ApplyModifiedProperties();
        }

        private static SettingsSummary CaptureSettingsSummary(
            ScriptableObject pcRpAsset,
            ScriptableObject pcRenderer,
            UnityEngine.Object pcSsaoFeature)
        {
            return new SettingsSummary(
                ReadSerialized(pcRpAsset, "m_MSAA"),
                ReadSerialized(pcRpAsset, "m_RenderScale"),
                ReadSerialized(pcRpAsset, "m_ShadowDistance"),
                ReadSerialized(pcRpAsset, "m_ShadowCascadeCount"),
                ReadSerialized(pcRpAsset, "m_MainLightShadowmapResolution"),
                ReadSerialized(pcRpAsset, "m_ReflectionProbeBlending"),
                ReadSerialized(pcRpAsset, "m_ReflectionProbeBoxProjection"),
                ReadSerialized(pcRenderer, "m_RenderingMode"),
                ReadSerialized(pcSsaoFeature, "m_Settings.Intensity"),
                ReadSerialized(pcSsaoFeature, "m_Settings.Radius"),
                ReadSerialized(pcSsaoFeature, "m_Settings.Samples"));
        }

        private static string ReadSerialized(UnityEngine.Object target, string propertyPath)
        {
            if (target == null)
            {
                return "missing";
            }

            var serializedObject = new SerializedObject(target);
            var property = serializedObject.FindProperty(propertyPath);
            if (property == null)
            {
                return "missing:" + propertyPath;
            }

            switch (property.propertyType)
            {
                case SerializedPropertyType.Boolean:
                    return property.boolValue ? "true" : "false";
                case SerializedPropertyType.Integer:
                    return property.intValue.ToString(CultureInfo.InvariantCulture);
                case SerializedPropertyType.Enum:
                    return property.intValue.ToString(CultureInfo.InvariantCulture);
                case SerializedPropertyType.Float:
                    return property.floatValue.ToString("0.###", CultureInfo.InvariantCulture);
                default:
                    return property.propertyType.ToString();
            }
        }

        private static SceneMetrics CaptureSceneMetrics()
        {
            var renderers = UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsInactive.Exclude);
            var lights = UnityEngine.Object.FindObjectsByType<Light>(FindObjectsInactive.Exclude);
            var volumes = UnityEngine.Object.FindObjectsByType<Volume>(FindObjectsInactive.Exclude);
            var reflectionProbes = UnityEngine.Object.FindObjectsByType<ReflectionProbe>(FindObjectsInactive.Exclude);
            var lightProbeGroups = UnityEngine.Object.FindObjectsByType<LightProbeGroup>(FindObjectsInactive.Exclude);
            return new SceneMetrics(
                renderers.Length,
                lights.Length,
                lights.Count(light => light.shadows != LightShadows.None),
                volumes.Length,
                reflectionProbes.Length,
                lightProbeGroups.Length);
        }

        private static int CountFrustumRenderers(Camera camera)
        {
            var planes = GeometryUtility.CalculateFrustumPlanes(camera);
            return UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsInactive.Exclude)
                .Count(renderer => renderer.enabled && GeometryUtility.TestPlanesAABB(planes, renderer.bounds));
        }

        private static IEnumerable<string> BuildReportLines(SpikeReport report)
        {
            var lines = new List<string>
            {
                "# MYB-102 URP Global Rendering Spike",
                "",
                "Status: " + (report.Failures.Count == 0 ? "PASS" : "FAIL"),
                "Generated at UTC: " + DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture),
                "Canonical scene: " + CanonicalScenePath,
                "Capture size: 1280x720",
                "Scope: baseline plus temporary variants; project rendering assets are restored before completion.",
                "Restoration status: " + report.RestorationStatus,
                "",
                "## Official documentation consulted",
                "- Unity URP Asset: https://docs.unity3d.com/Manual/urp/universalrp-asset.html",
                "- Unity URP Renderer Features: https://docs.unity3d.com/Manual/urp/urp-renderer-feature.html",
                "- Unity post-processing and volumes: https://docs.unity3d.com/Manual/urp/volumes-landing.html",
                "- Unity reflection probes: https://docs.unity3d.com/Manual/class-ReflectionProbe.html",
                "",
                "## Initial Socle de Rendu Projet",
                "- PC_RPAsset: " + PcRpAssetPath,
                "- PC_Renderer: " + PcRendererPath
            };

            if (report.InitialSettings != null)
            {
                lines.AddRange(report.InitialSettings.ToLines());
            }

            lines.Add("");
            lines.Add("## Decision rubric");
            lines.Add("- Recommend a global change only when at least two global families show a visible reusable gain across the three Passages without obvious performance or ride-readability debt.");
            lines.Add("- Otherwise keep global defaults stable and push improvements into scene, asset, or local-volume tickets.");
            lines.Add("- MYB-102 does not persist global rendering changes; it produces evidence for a follow-up decision.");
            lines.Add("");
            lines.Add("## Variants");

            foreach (var variant in report.Variants)
            {
                lines.Add("### " + variant.Label);
                lines.Add("- Slug: " + variant.Slug);
                lines.Add("- Family: " + variant.Family);
                lines.Add("- Description: " + variant.Description);
                if (variant.Metrics != null)
                {
                    lines.AddRange(variant.Metrics.ToLines());
                }

                foreach (var capture in variant.Captures)
                {
                    lines.Add("- " + capture.Label + ": `" + capture.RelativePath + "`");
                    lines.Add("  - Route position: " + capture.Meters.ToString("0.0", CultureInfo.InvariantCulture) + " m / " + capture.RouteLength.ToString("0.0", CultureInfo.InvariantCulture) + " m");
                    lines.Add("  - Camera position: " + FormatVector(capture.CameraPosition));
                    lines.Add("  - Camera forward: " + FormatVector(capture.CameraForward));
                    lines.Add("  - Frustum renderers: " + capture.FrustumRendererCount.ToString(CultureInfo.InvariantCulture));
                    lines.Add("  - Image metrics: " + capture.ImageMetrics.ToSummary());
                }
            }

            if (report.Warnings.Count > 0)
            {
                lines.Add("");
                lines.Add("## Warnings");
                foreach (var warning in report.Warnings)
                {
                    lines.Add("- " + warning);
                }
            }

            if (report.Failures.Count > 0)
            {
                lines.Add("");
                lines.Add("## Failures");
                foreach (var failure in report.Failures)
                {
                    lines.Add("- " + failure);
                }
            }

            return lines;
        }

        private static List<CanvasState> DisableCanvases()
        {
            var canvases = UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include);
            var states = new List<CanvasState>(canvases.Length);
            foreach (var canvas in canvases)
            {
                states.Add(new CanvasState(canvas, canvas.enabled));
                canvas.enabled = false;
            }

            return states;
        }

        private static void RestoreCanvases(IEnumerable<CanvasState> states)
        {
            foreach (var state in states)
            {
                if (state.Canvas != null)
                {
                    state.Canvas.enabled = state.Enabled;
                }
            }
        }

        private static ImageMetrics RenderCameraToPng(Camera camera, int width, int height, string path)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path) ?? GetRepoRoot());
            var previousTarget = camera.targetTexture;
            var previousActive = RenderTexture.active;
            var renderTexture = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);

            try
            {
                camera.targetTexture = renderTexture;
                RenderTexture.active = renderTexture;
                camera.Render();
                texture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                texture.Apply();
                File.WriteAllBytes(path, texture.EncodeToPNG());
                return ImageMetrics.FromTexture(texture);
            }
            finally
            {
                camera.targetTexture = previousTarget;
                RenderTexture.active = previousActive;
                renderTexture.Release();
                UnityEngine.Object.DestroyImmediate(renderTexture);
                UnityEngine.Object.DestroyImmediate(texture);
            }
        }

        private static string FormatVector(Vector3 value)
        {
            return string.Format(CultureInfo.InvariantCulture, "({0:0.0}, {1:0.0}, {2:0.0})", value.x, value.y, value.z);
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

        private sealed class TemporarySpikeState
        {
            private readonly List<SerializedFieldSnapshot> snapshots = new List<SerializedFieldSnapshot>();
            private readonly List<GameObject> temporaryObjects = new List<GameObject>();
            private RenderSettingsSnapshot renderSettingsSnapshot;

            public TemporarySpikeState(SpikeReport report)
            {
                Report = report;
                renderSettingsSnapshot = RenderSettingsSnapshot.Capture();
            }

            public SpikeReport Report { get; }
            public bool HasUnrestoredChanges { get; private set; }

            public void SetSerializedFloat(UnityEngine.Object target, string path, float value)
            {
                SetSerializedValue(target, path, property => property.floatValue = value);
            }

            public void SetSerializedInt(UnityEngine.Object target, string path, int value)
            {
                SetSerializedValue(target, path, property => property.intValue = value);
            }

            public void SetAmbient(
                AmbientMode ambientMode,
                Color skyColor,
                Color equatorColor,
                Color groundColor,
                bool fog,
                FogMode fogMode,
                Color fogColor,
                float fogDensity)
            {
                RenderSettings.ambientMode = ambientMode;
                RenderSettings.ambientSkyColor = skyColor;
                RenderSettings.ambientEquatorColor = equatorColor;
                RenderSettings.ambientGroundColor = groundColor;
                RenderSettings.fog = fog;
                RenderSettings.fogMode = fogMode;
                RenderSettings.fogColor = fogColor;
                RenderSettings.fogDensity = fogDensity;
            }

            public void RegisterTemporaryObject(GameObject temporaryObject)
            {
                if (temporaryObject != null)
                {
                    temporaryObjects.Add(temporaryObject);
                }
            }

            public void RestoreVariantState()
            {
                DestroyTemporaryObjects();
                renderSettingsSnapshot.Restore();
                RestoreSerializedSnapshots();
            }

            public void RestoreAll()
            {
                DestroyTemporaryObjects();
                renderSettingsSnapshot.Restore();
                RestoreSerializedSnapshots();
            }

            private void RestoreSerializedSnapshots()
            {
                for (var i = snapshots.Count - 1; i >= 0; i--)
                {
                    if (!snapshots[i].Restore())
                    {
                        HasUnrestoredChanges = true;
                    }
                }
            }

            private void SetSerializedValue(UnityEngine.Object target, string path, Action<SerializedProperty> setValue)
            {
                if (target == null)
                {
                    Report.Warnings.Add("Cannot set serialized property on missing target: " + path);
                    return;
                }

                var serializedObject = new SerializedObject(target);
                var property = serializedObject.FindProperty(path);
                if (property == null)
                {
                    Report.Warnings.Add("Missing serialized property: " + target.name + "." + path);
                    return;
                }

                if (!snapshots.Any(snapshot => snapshot.Matches(target, path)))
                {
                    snapshots.Add(SerializedFieldSnapshot.Capture(target, path, property));
                }

                setValue(property);
                serializedObject.ApplyModifiedProperties();
            }

            private void DestroyTemporaryObjects()
            {
                for (var i = temporaryObjects.Count - 1; i >= 0; i--)
                {
                    if (temporaryObjects[i] != null)
                    {
                        UnityEngine.Object.DestroyImmediate(temporaryObjects[i]);
                    }
                }

                temporaryObjects.Clear();
            }
        }

        private readonly struct SerializedFieldSnapshot
        {
            private readonly UnityEngine.Object target;
            private readonly string path;
            private readonly SerializedPropertyType type;
            private readonly int intValue;
            private readonly bool boolValue;
            private readonly float floatValue;

            private SerializedFieldSnapshot(
                UnityEngine.Object target,
                string path,
                SerializedPropertyType type,
                int intValue,
                bool boolValue,
                float floatValue)
            {
                this.target = target;
                this.path = path;
                this.type = type;
                this.intValue = intValue;
                this.boolValue = boolValue;
                this.floatValue = floatValue;
            }

            public static SerializedFieldSnapshot Capture(UnityEngine.Object target, string path, SerializedProperty property)
            {
                return new SerializedFieldSnapshot(
                    target,
                    path,
                    property.propertyType,
                    property.propertyType == SerializedPropertyType.Integer || property.propertyType == SerializedPropertyType.Enum ? property.intValue : 0,
                    property.propertyType == SerializedPropertyType.Boolean && property.boolValue,
                    property.propertyType == SerializedPropertyType.Float ? property.floatValue : 0f);
            }

            public bool Matches(UnityEngine.Object otherTarget, string otherPath)
            {
                return target == otherTarget && string.Equals(path, otherPath, StringComparison.Ordinal);
            }

            public bool Restore()
            {
                if (target == null)
                {
                    return false;
                }

                var serializedObject = new SerializedObject(target);
                var property = serializedObject.FindProperty(path);
                if (property == null)
                {
                    return false;
                }

                switch (type)
                {
                    case SerializedPropertyType.Integer:
                        property.intValue = intValue;
                        break;
                    case SerializedPropertyType.Enum:
                        property.intValue = intValue;
                        break;
                    case SerializedPropertyType.Boolean:
                        property.boolValue = boolValue;
                        break;
                    case SerializedPropertyType.Float:
                        property.floatValue = floatValue;
                        break;
                    default:
                        return false;
                }

                serializedObject.ApplyModifiedProperties();
                return true;
            }
        }

        private readonly struct RenderSettingsSnapshot
        {
            private readonly AmbientMode ambientMode;
            private readonly Color ambientLight;
            private readonly Color ambientSkyColor;
            private readonly Color ambientEquatorColor;
            private readonly Color ambientGroundColor;
            private readonly bool fog;
            private readonly FogMode fogMode;
            private readonly Color fogColor;
            private readonly float fogDensity;

            private RenderSettingsSnapshot(
                AmbientMode ambientMode,
                Color ambientLight,
                Color ambientSkyColor,
                Color ambientEquatorColor,
                Color ambientGroundColor,
                bool fog,
                FogMode fogMode,
                Color fogColor,
                float fogDensity)
            {
                this.ambientMode = ambientMode;
                this.ambientLight = ambientLight;
                this.ambientSkyColor = ambientSkyColor;
                this.ambientEquatorColor = ambientEquatorColor;
                this.ambientGroundColor = ambientGroundColor;
                this.fog = fog;
                this.fogMode = fogMode;
                this.fogColor = fogColor;
                this.fogDensity = fogDensity;
            }

            public static RenderSettingsSnapshot Capture()
            {
                return new RenderSettingsSnapshot(
                    RenderSettings.ambientMode,
                    RenderSettings.ambientLight,
                    RenderSettings.ambientSkyColor,
                    RenderSettings.ambientEquatorColor,
                    RenderSettings.ambientGroundColor,
                    RenderSettings.fog,
                    RenderSettings.fogMode,
                    RenderSettings.fogColor,
                    RenderSettings.fogDensity);
            }

            public void Restore()
            {
                RenderSettings.ambientMode = ambientMode;
                RenderSettings.ambientLight = ambientLight;
                RenderSettings.ambientSkyColor = ambientSkyColor;
                RenderSettings.ambientEquatorColor = ambientEquatorColor;
                RenderSettings.ambientGroundColor = ambientGroundColor;
                RenderSettings.fog = fog;
                RenderSettings.fogMode = fogMode;
                RenderSettings.fogColor = fogColor;
                RenderSettings.fogDensity = fogDensity;
            }
        }

        private sealed class SpikeReport
        {
            public SpikeReport()
            {
                RestorationStatus = "NOT_RUN";
            }

            public SettingsSummary InitialSettings { get; set; }
            public string RestorationStatus { get; set; }
            public List<VariantReport> Variants { get; } = new List<VariantReport>();
            public List<string> Warnings { get; } = new List<string>();
            public List<string> Failures { get; } = new List<string>();
        }

        private sealed class VariantReport
        {
            public VariantReport(string slug, string label, string family, string description)
            {
                Slug = slug;
                Label = label;
                Family = family;
                Description = description;
            }

            public string Slug { get; }
            public string Label { get; }
            public string Family { get; }
            public string Description { get; }
            public SceneMetrics Metrics { get; set; }
            public List<CaptureReport> Captures { get; } = new List<CaptureReport>();
        }

        private sealed class VariantSpec
        {
            public VariantSpec(string slug, string label, string description, string family, Action<TemporarySpikeState, MYB89ProbeRide> apply)
            {
                Slug = slug;
                Label = label;
                Description = description;
                Family = family;
                Apply = apply;
            }

            public string Slug { get; }
            public string Label { get; }
            public string Description { get; }
            public string Family { get; }
            public Action<TemporarySpikeState, MYB89ProbeRide> Apply { get; }
        }

        private sealed class SettingsSummary
        {
            public SettingsSummary(
                string msaa,
                string renderScale,
                string shadowDistance,
                string shadowCascadeCount,
                string mainLightShadowmapResolution,
                string reflectionProbeBlending,
                string reflectionProbeBoxProjection,
                string renderingMode,
                string ssaoIntensity,
                string ssaoRadius,
                string ssaoSamples)
            {
                Msaa = msaa;
                RenderScale = renderScale;
                ShadowDistance = shadowDistance;
                ShadowCascadeCount = shadowCascadeCount;
                MainLightShadowmapResolution = mainLightShadowmapResolution;
                ReflectionProbeBlending = reflectionProbeBlending;
                ReflectionProbeBoxProjection = reflectionProbeBoxProjection;
                RenderingMode = renderingMode;
                SsaoIntensity = ssaoIntensity;
                SsaoRadius = ssaoRadius;
                SsaoSamples = ssaoSamples;
            }

            private string Msaa { get; }
            private string RenderScale { get; }
            private string ShadowDistance { get; }
            private string ShadowCascadeCount { get; }
            private string MainLightShadowmapResolution { get; }
            private string ReflectionProbeBlending { get; }
            private string ReflectionProbeBoxProjection { get; }
            private string RenderingMode { get; }
            private string SsaoIntensity { get; }
            private string SsaoRadius { get; }
            private string SsaoSamples { get; }

            public IEnumerable<string> ToLines()
            {
                yield return "- MSAA: " + Msaa;
                yield return "- Render scale: " + RenderScale;
                yield return "- Shadow distance: " + ShadowDistance;
                yield return "- Shadow cascades: " + ShadowCascadeCount;
                yield return "- Main light shadowmap resolution: " + MainLightShadowmapResolution;
                yield return "- Reflection probe blending: " + ReflectionProbeBlending;
                yield return "- Reflection probe box projection: " + ReflectionProbeBoxProjection;
                yield return "- Renderer mode: " + RenderingMode;
                yield return "- SSAO intensity: " + SsaoIntensity;
                yield return "- SSAO radius: " + SsaoRadius;
                yield return "- SSAO samples: " + SsaoSamples;
            }
        }

        private sealed class CaptureReport
        {
            public CaptureReport(
                string slug,
                string label,
                string relativePath,
                float meters,
                float routeLength,
                Vector3 cameraPosition,
                Vector3 cameraForward,
                int frustumRendererCount,
                ImageMetrics imageMetrics)
            {
                Slug = slug;
                Label = label;
                RelativePath = relativePath;
                Meters = meters;
                RouteLength = routeLength;
                CameraPosition = cameraPosition;
                CameraForward = cameraForward;
                FrustumRendererCount = frustumRendererCount;
                ImageMetrics = imageMetrics;
            }

            public string Slug { get; }
            public string Label { get; }
            public string RelativePath { get; }
            public float Meters { get; }
            public float RouteLength { get; }
            public Vector3 CameraPosition { get; }
            public Vector3 CameraForward { get; }
            public int FrustumRendererCount { get; }
            public ImageMetrics ImageMetrics { get; }
        }

        private sealed class SceneMetrics
        {
            public SceneMetrics(int rendererCount, int lightCount, int shadowLightCount, int volumeCount, int reflectionProbeCount, int lightProbeGroupCount)
            {
                RendererCount = rendererCount;
                LightCount = lightCount;
                ShadowLightCount = shadowLightCount;
                VolumeCount = volumeCount;
                ReflectionProbeCount = reflectionProbeCount;
                LightProbeGroupCount = lightProbeGroupCount;
            }

            private int RendererCount { get; }
            private int LightCount { get; }
            private int ShadowLightCount { get; }
            private int VolumeCount { get; }
            private int ReflectionProbeCount { get; }
            private int LightProbeGroupCount { get; }

            public IEnumerable<string> ToLines()
            {
                yield return "- Renderers: " + RendererCount.ToString(CultureInfo.InvariantCulture);
                yield return "- Lights: " + LightCount.ToString(CultureInfo.InvariantCulture);
                yield return "- Shadow-casting lights: " + ShadowLightCount.ToString(CultureInfo.InvariantCulture);
                yield return "- Volumes: " + VolumeCount.ToString(CultureInfo.InvariantCulture);
                yield return "- Reflection probes: " + ReflectionProbeCount.ToString(CultureInfo.InvariantCulture);
                yield return "- Light probe groups: " + LightProbeGroupCount.ToString(CultureInfo.InvariantCulture);
            }
        }

        private readonly struct ImageMetrics
        {
            private ImageMetrics(float averageLuminance, float contrast, float darkPixelPercent, float brightPixelPercent)
            {
                AverageLuminance = averageLuminance;
                Contrast = contrast;
                DarkPixelPercent = darkPixelPercent;
                BrightPixelPercent = brightPixelPercent;
            }

            private float AverageLuminance { get; }
            private float Contrast { get; }
            private float DarkPixelPercent { get; }
            private float BrightPixelPercent { get; }

            public static ImageMetrics FromTexture(Texture2D texture)
            {
                double sum = 0d;
                double sumSquares = 0d;
                var darkPixels = 0;
                var brightPixels = 0;
                var samples = 0;

                for (var y = 0; y < texture.height; y += 4)
                {
                    for (var x = 0; x < texture.width; x += 4)
                    {
                        var color = texture.GetPixel(x, y);
                        var luminance = color.linear.r * 0.2126f + color.linear.g * 0.7152f + color.linear.b * 0.0722f;
                        sum += luminance;
                        sumSquares += luminance * luminance;
                        if (luminance < 0.08f)
                        {
                            darkPixels++;
                        }

                        if (luminance > 0.82f)
                        {
                            brightPixels++;
                        }

                        samples++;
                    }
                }

                if (samples == 0)
                {
                    return new ImageMetrics(0f, 0f, 0f, 0f);
                }

                var average = sum / samples;
                var variance = Math.Max(0d, sumSquares / samples - average * average);
                return new ImageMetrics(
                    (float)average,
                    (float)Math.Sqrt(variance),
                    (float)(darkPixels * 100d / samples),
                    (float)(brightPixels * 100d / samples));
            }

            public string ToSummary()
            {
                return string.Format(
                    CultureInfo.InvariantCulture,
                    "avg luma {0:0.000}, contrast {1:0.000}, dark {2:0.0}%, bright {3:0.0}%",
                    AverageLuminance,
                    Contrast,
                    DarkPixelPercent,
                    BrightPixelPercent);
            }
        }

        private readonly struct PassageSpec
        {
            public PassageSpec(string slug, string label, float progress01)
            {
                Slug = slug;
                Label = label;
                Progress01 = progress01;
            }

            public string Slug { get; }
            public string Label { get; }
            public float Progress01 { get; }
        }

        private readonly struct CanvasState
        {
            public CanvasState(Canvas canvas, bool enabled)
            {
                Canvas = canvas;
                Enabled = enabled;
            }

            public Canvas Canvas { get; }
            public bool Enabled { get; }
        }
    }
}

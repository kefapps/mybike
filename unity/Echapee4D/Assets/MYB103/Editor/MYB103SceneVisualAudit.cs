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

namespace MYB103.Editor
{
    public static class MYB103SceneVisualAudit
    {
        private const string CanonicalScenePath = "Assets/Scenes/MYB89UnityMcpProbe.unity";
        private const string OutputDirectory = "_bmad-output/unity-test-results/myb-103";
        private const string ReportRelativePath = OutputDirectory + "/myb-103-scene-visual-audit.txt";

        private static readonly PassageSpec[] Passages =
        {
            new PassageSpec("passage-01-foret-claire", "Foret claire", 0.16f),
            new PassageSpec("passage-02-village-route-de-col", "Village / route de col", 0.50f),
            new PassageSpec("passage-03-panorama-signal-fantasy", "Panorama / signal fantasy", 0.84f)
        };

        [MenuItem("Tools/MYB-103/Capture Scene Visual Audit")]
        public static string CaptureSceneVisualAuditCli()
        {
            var reportPath = CaptureSceneVisualAudit();
            Debug.Log("MYB-103 scene visual audit captured: " + reportPath);
            return reportPath;
        }

        public static string CaptureSceneVisualAudit()
        {
            EditorSceneManager.OpenScene(CanonicalScenePath, OpenSceneMode.Single);

            var failures = new List<string>();
            var captures = new List<PassageCapture>();
            var ride = UnityEngine.Object.FindAnyObjectByType<MYB89ProbeRide>();
            var camera = Camera.main;

            if (ride == null)
            {
                failures.Add("Missing MYB89ProbeRide in canonical scene.");
            }

            if (camera == null)
            {
                failures.Add("Missing Main Camera in canonical scene.");
            }

            if (ride != null && camera != null)
            {
                ride.RebuildRouteCache();
                var routeLength = Mathf.Max(1f, ride.RouteLength);
                var canvasStates = DisableCanvases();

                try
                {
                    foreach (var passage in Passages)
                    {
                        var meters = routeLength * Mathf.Clamp01(passage.Progress01);
                        ride.SetPreviewProgress(meters);
                        Canvas.ForceUpdateCanvases();

                        var relativePath = OutputDirectory + "/" + passage.Slug + ".png";
                        RenderCameraToPng(camera, 1280, 720, Path.Combine(GetRepoRoot(), relativePath));
                        captures.Add(CapturePassage(passage, relativePath, meters, routeLength, camera));
                    }
                }
                finally
                {
                    RestoreCanvases(canvasStates);
                }
            }

            var sceneMetrics = CaptureSceneMetrics();
            var absoluteReportPath = Path.Combine(GetRepoRoot(), ReportRelativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(absoluteReportPath) ?? GetRepoRoot());
            File.WriteAllLines(absoluteReportPath, BuildReportLines(sceneMetrics, captures, failures));

            if (failures.Count > 0)
            {
                throw new InvalidOperationException("MYB-103 scene visual audit capture failed. See " + absoluteReportPath);
            }

            return absoluteReportPath;
        }

        private static PassageCapture CapturePassage(
            PassageSpec passage,
            string relativePath,
            float meters,
            float routeLength,
            Camera camera)
        {
            var planes = GeometryUtility.CalculateFrustumPlanes(camera);
            var renderers = UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsInactive.Exclude);
            var visibleRenderers = renderers
                .Where(renderer => renderer.enabled && GeometryUtility.TestPlanesAABB(planes, renderer.bounds))
                .OrderBy(renderer => Vector3.Distance(camera.transform.position, renderer.bounds.center))
                .ToArray();
            var sampleNames = visibleRenderers
                .Take(16)
                .Select(renderer => renderer.gameObject.name)
                .ToArray();

            return new PassageCapture(
                passage.Slug,
                passage.Label,
                relativePath,
                meters,
                routeLength,
                camera.transform.position,
                camera.transform.forward,
                visibleRenderers.Length,
                sampleNames);
        }

        private static SceneMetrics CaptureSceneMetrics()
        {
            var renderers = UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsInactive.Exclude);
            var materials = renderers
                .SelectMany(renderer => renderer.sharedMaterials ?? Array.Empty<Material>())
                .Where(material => material != null)
                .Select(material => material.name)
                .Distinct(StringComparer.Ordinal)
                .OrderBy(name => name, StringComparer.Ordinal)
                .ToArray();
            var lights = UnityEngine.Object.FindObjectsByType<Light>(FindObjectsInactive.Exclude);
            var volumes = UnityEngine.Object.FindObjectsByType<Volume>(FindObjectsInactive.Exclude);
            var reflectionProbes = UnityEngine.Object.FindObjectsByType<ReflectionProbe>(FindObjectsInactive.Exclude);
            var lightProbeGroups = UnityEngine.Object.FindObjectsByType<LightProbeGroup>(FindObjectsInactive.Exclude);

            var hasBounds = false;
            var bounds = new Bounds();
            foreach (var renderer in renderers)
            {
                if (!hasBounds)
                {
                    bounds = renderer.bounds;
                    hasBounds = true;
                }
                else
                {
                    bounds.Encapsulate(renderer.bounds);
                }
            }

            return new SceneMetrics(
                renderers.Length,
                materials.Length,
                materials.Take(20).ToArray(),
                lights.Length,
                lights.Count(light => light.shadows != LightShadows.None),
                volumes.Length,
                reflectionProbes.Length,
                lightProbeGroups.Length,
                hasBounds ? bounds.size : Vector3.zero);
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

        private static IEnumerable<string> BuildReportLines(
            SceneMetrics metrics,
            IReadOnlyCollection<PassageCapture> captures,
            IReadOnlyCollection<string> failures)
        {
            var lines = new List<string>
            {
                "# MYB-103 Scene Visual Audit Capture",
                "",
                "Status: " + (failures.Count == 0 ? "PASS" : "FAIL"),
                "Generated at UTC: " + DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture),
                "Canonical scene: " + CanonicalScenePath,
                "Capture size: 1280x720",
                "HUD/canvases: disabled for visual audit captures",
                "Passage sampling: current route thirds used as audit proxies for the future three one-minute target sequence.",
                "",
                "## Scene metrics",
                "- Renderers: " + metrics.RendererCount.ToString(CultureInfo.InvariantCulture),
                "- Visible material names: " + metrics.MaterialNameCount.ToString(CultureInfo.InvariantCulture),
                "- Sample material names: " + string.Join(", ", metrics.SampleMaterialNames),
                "- Lights: " + metrics.LightCount.ToString(CultureInfo.InvariantCulture),
                "- Shadow-casting lights: " + metrics.ShadowLightCount.ToString(CultureInfo.InvariantCulture),
                "- Volumes: " + metrics.VolumeCount.ToString(CultureInfo.InvariantCulture),
                "- Reflection probes: " + metrics.ReflectionProbeCount.ToString(CultureInfo.InvariantCulture),
                "- Light probe groups: " + metrics.LightProbeGroupCount.ToString(CultureInfo.InvariantCulture),
                "- Scene bounds: " + FormatVector(metrics.SceneBounds),
                "",
                "## Passage captures"
            };

            foreach (var capture in captures)
            {
                lines.Add("### " + capture.Label);
                lines.Add("- Slug: " + capture.Slug);
                lines.Add("- Capture: " + capture.RelativePath);
                lines.Add("- Route position: " + capture.Meters.ToString("0.0", CultureInfo.InvariantCulture) + " m / " + capture.RouteLength.ToString("0.0", CultureInfo.InvariantCulture) + " m");
                lines.Add("- Camera position: " + FormatVector(capture.CameraPosition));
                lines.Add("- Camera forward: " + FormatVector(capture.CameraForward));
                lines.Add("- Frustum renderer count: " + capture.FrustumRendererCount.ToString(CultureInfo.InvariantCulture));
                lines.Add("- Closest visible objects: " + string.Join(", ", capture.SampleRendererNames));
                lines.Add("");
            }

            if (failures.Count > 0)
            {
                lines.Add("## Failures");
                foreach (var failure in failures)
                {
                    lines.Add("- " + failure);
                }
            }

            return lines;
        }

        private static string FormatVector(Vector3 value)
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "({0:0.0}, {1:0.0}, {2:0.0})",
                value.x,
                value.y,
                value.z);
        }

        private static void RenderCameraToPng(Camera camera, int width, int height, string path)
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

        private static string GetRepoRoot()
        {
            var projectRoot = Directory.GetParent(Application.dataPath);
            if (projectRoot == null || projectRoot.Parent == null || projectRoot.Parent.Parent == null)
            {
                return projectRoot == null ? Application.dataPath : projectRoot.FullName;
            }

            return projectRoot.Parent.Parent.FullName;
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

        private readonly struct PassageCapture
        {
            public PassageCapture(
                string slug,
                string label,
                string relativePath,
                float meters,
                float routeLength,
                Vector3 cameraPosition,
                Vector3 cameraForward,
                int frustumRendererCount,
                IReadOnlyCollection<string> sampleRendererNames)
            {
                Slug = slug;
                Label = label;
                RelativePath = relativePath;
                Meters = meters;
                RouteLength = routeLength;
                CameraPosition = cameraPosition;
                CameraForward = cameraForward;
                FrustumRendererCount = frustumRendererCount;
                SampleRendererNames = sampleRendererNames;
            }

            public string Slug { get; }
            public string Label { get; }
            public string RelativePath { get; }
            public float Meters { get; }
            public float RouteLength { get; }
            public Vector3 CameraPosition { get; }
            public Vector3 CameraForward { get; }
            public int FrustumRendererCount { get; }
            public IReadOnlyCollection<string> SampleRendererNames { get; }
        }

        private readonly struct SceneMetrics
        {
            public SceneMetrics(
                int rendererCount,
                int materialNameCount,
                IReadOnlyCollection<string> sampleMaterialNames,
                int lightCount,
                int shadowLightCount,
                int volumeCount,
                int reflectionProbeCount,
                int lightProbeGroupCount,
                Vector3 sceneBounds)
            {
                RendererCount = rendererCount;
                MaterialNameCount = materialNameCount;
                SampleMaterialNames = sampleMaterialNames;
                LightCount = lightCount;
                ShadowLightCount = shadowLightCount;
                VolumeCount = volumeCount;
                ReflectionProbeCount = reflectionProbeCount;
                LightProbeGroupCount = lightProbeGroupCount;
                SceneBounds = sceneBounds;
            }

            public int RendererCount { get; }
            public int MaterialNameCount { get; }
            public IReadOnlyCollection<string> SampleMaterialNames { get; }
            public int LightCount { get; }
            public int ShadowLightCount { get; }
            public int VolumeCount { get; }
            public int ReflectionProbeCount { get; }
            public int LightProbeGroupCount { get; }
            public Vector3 SceneBounds { get; }
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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using MYB89;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;

public static class MYB166PerformanceProbe
{
    private const string ScenePath = "Assets/Scenes/MYB89UnityMcpProbe.unity";
    private const string OutputRootRelative = "_bmad-output/implementation-artifacts/MYB-166";
    private const string JsonRelativePath = OutputRootRelative + "/myb-166-route-camera-render-probe.json";
    private const string ReportRelativePath = OutputRootRelative + "/myb-166-route-camera-render-probe.md";
    private const int RenderWidth = 1280;
    private const int RenderHeight = 720;
    private const float ProbeCameraHeight = 1.30f;
    private const float ProbeCameraBackwardMeters = 0.92f;
    private const float ProbeCameraPitchDegrees = 5.5f;
    private const float ProbeCameraFov = 68f;
    private const float ProbeCameraNearClip = 0.03f;
    private const float ProbeCameraFarClip = 520f;

    [MenuItem("Tools/MyBike/MYB-166/Run Route Camera Render Probe")]
    public static void RunFromMenu()
    {
        Debug.Log(RunBatchRouteCameraRenderProbe());
    }

    public static string RunBatchRouteCameraRenderProbe()
    {
        Directory.CreateDirectory(ToRepoPath(OutputRootRelative));
        OpenScene();

        var route = LoadRoutePoints();
        var routeLength = MYB89RideTrajectory.Length(route);
        var camera = FindProbeCamera();
        var allRenderers = UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsInactive.Exclude)
            .Where(renderer => renderer.enabled && renderer.gameObject.activeInHierarchy)
            .ToArray();
        var allLights = UnityEngine.Object.FindObjectsByType<Light>(FindObjectsInactive.Exclude)
            .Where(light => light.enabled && light.gameObject.activeInHierarchy)
            .ToArray();
        var allMaterials = UniqueMaterials(allRenderers);

        var originalCameraState = CameraState.Capture(camera);
        var previousActive = RenderTexture.active;
        var renderTexture = new RenderTexture(RenderWidth, RenderHeight, 24, RenderTextureFormat.ARGB32);
        var report = new RenderProbeReport
        {
            Ticket = "MYB-166",
            TimestampUtc = DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture),
            ScenePath = ScenePath,
            RouteLengthMeters = routeLength,
            RenderWidth = RenderWidth,
            RenderHeight = RenderHeight,
            RendererCount = allRenderers.Length,
            ShadowCasterCount = allRenderers.Count(IsShadowCaster),
            ShadowReceiverCount = allRenderers.Count(renderer => renderer.receiveShadows),
            LodGroupCount = UnityEngine.Object.FindObjectsByType<LODGroup>(FindObjectsInactive.Exclude).Length,
            LightCount = allLights.Length,
            MaterialCount = allMaterials.Count,
            QualityLevel = QualitySettings.names.Length == 0
                ? QualitySettings.GetQualityLevel().ToString(CultureInfo.InvariantCulture)
                : QualitySettings.names[QualitySettings.GetQualityLevel()],
            VSyncCount = QualitySettings.vSyncCount,
            TargetFrameRate = Application.targetFrameRate,
            Notes = new[]
            {
                "Render probe uses Camera.Render wall-clock timings in the Unity Editor.",
                "This is a route-camera render-cost proxy, not a complete Play Mode FPS benchmark.",
                "Use this to locate heavy sections and compare reversible optimization candidates before removing content."
            }
        };

        try
        {
            camera.targetTexture = renderTexture;
            camera.fieldOfView = ProbeCameraFov;
            camera.nearClipPlane = ProbeCameraNearClip;
            camera.farClipPlane = ProbeCameraFarClip;
            camera.orthographic = false;
            RenderTexture.active = renderTexture;

            WarmupCamera(camera, route);
            report.DiscoverySamples = MeasureScenario(
                "discovery-full-route-every-40m",
                route,
                camera,
                allRenderers,
                Distances(0f, routeLength, 40f));

            var worst = report.DiscoverySamples.OrderByDescending(sample => sample.RenderMs).FirstOrDefault();
            var worstMeters = worst == null ? 0f : worst.Meters;
            report.WorstCaseCenterMeters = worstMeters;
            report.WorstCaseSamples = MeasureScenario(
                "route-camera-worst-case-slice-every-5m",
                route,
                camera,
                allRenderers,
                Distances(Mathf.Max(0f, worstMeters - 75f), Mathf.Min(routeLength, worstMeters + 75f), 5f));

            report.FullRouteValidationSamples = MeasureScenario(
                "full-route-3min-validation-every-25m",
                route,
                camera,
                allRenderers,
                Distances(0f, routeLength, 25f));

            report.DiscoverySummary = Summarize(report.DiscoverySamples);
            report.WorstCaseSummary = Summarize(report.WorstCaseSamples);
            report.FullRouteSummary = Summarize(report.FullRouteValidationSamples);
            report.Interpretation = Interpret(report);

            WriteJson(report);
            WriteMarkdown(report);
        }
        finally
        {
            originalCameraState.Restore(camera);
            RenderTexture.active = previousActive;
            renderTexture.Release();
            UnityEngine.Object.DestroyImmediate(renderTexture);
        }

        Debug.Log("MYB-166 route-camera render probe wrote " + JsonRelativePath);
        return JsonRelativePath;
    }

    private static void OpenScene()
    {
        var active = SceneManager.GetActiveScene();
        if (active.path != ScenePath)
        {
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        }
    }

    private static Vector3[] LoadRoutePoints()
    {
        var markerRoot = GameObject.Find("MYB89_RouteMarkers");
        if (markerRoot == null)
        {
            throw new InvalidOperationException("Missing MYB89_RouteMarkers in canonical scene.");
        }

        var markerPositions = markerRoot
            .GetComponentsInChildren<Transform>(true)
            .Where(transform => transform != markerRoot.transform && transform.name.StartsWith("RouteMarker_", StringComparison.Ordinal))
            .OrderBy(transform => transform.name, StringComparer.Ordinal)
            .Select(transform => transform.position)
            .ToArray();
        if (markerPositions.Length < 2)
        {
            throw new InvalidOperationException("MYB89_RouteMarkers must contain at least two RouteMarker_* children.");
        }

        return MYB89RideTrajectory.BuildSmoothedPoints(markerPositions, MYB89RideTrajectory.DefaultSamplesPerSegment);
    }

    private static Camera FindProbeCamera()
    {
        var mainCameraObject = GameObject.Find("Main Camera");
        if (mainCameraObject != null && mainCameraObject.TryGetComponent(out Camera mainCamera))
        {
            return mainCamera;
        }

        if (Camera.main != null)
        {
            return Camera.main;
        }

        var anyCamera = UnityEngine.Object.FindAnyObjectByType<Camera>(FindObjectsInactive.Exclude);
        if (anyCamera == null)
        {
            throw new InvalidOperationException("No Camera found for MYB-166 performance probe.");
        }

        return anyCamera;
    }

    private static void WarmupCamera(Camera camera, IReadOnlyList<Vector3> route)
    {
        var warmupDistances = new[] { 0f, 28f, Mathf.Min(120f, MYB89RideTrajectory.Length(route)) };
        foreach (var meters in warmupDistances)
        {
            PositionCamera(camera, route, meters);
            camera.Render();
            GL.Flush();
        }
    }

    private static List<FrameSample> MeasureScenario(
        string scenario,
        IReadOnlyList<Vector3> route,
        Camera camera,
        IReadOnlyList<Renderer> renderers,
        IEnumerable<float> distances)
    {
        var samples = new List<FrameSample>();
        var stopwatch = new Stopwatch();
        foreach (var meters in distances)
        {
            PositionCamera(camera, route, meters);
            Canvas.ForceUpdateCanvases();

            var visibleStats = CountVisible(renderers, camera);
            stopwatch.Restart();
            camera.Render();
            GL.Flush();
            stopwatch.Stop();

            samples.Add(new FrameSample
            {
                Scenario = scenario,
                Meters = meters,
                RenderMs = stopwatch.Elapsed.TotalMilliseconds,
                EstimatedFps = stopwatch.Elapsed.TotalMilliseconds <= 0.0001d ? 0d : 1000d / stopwatch.Elapsed.TotalMilliseconds,
                VisibleRendererCount = visibleStats.VisibleRendererCount,
                VisibleShadowCasterCount = visibleStats.VisibleShadowCasterCount,
                VisibleShadowReceiverCount = visibleStats.VisibleShadowReceiverCount,
                VisibleTriangleCount = visibleStats.VisibleTriangleCount,
                VisibleMaterialCount = visibleStats.VisibleMaterialCount
            });
        }

        return samples;
    }

    private static void PositionCamera(Camera camera, IReadOnlyList<Vector3> route, float meters)
    {
        if (!MYB89RideTrajectory.TrySample(route, meters, false, out var sample))
        {
            throw new InvalidOperationException("Unable to sample route at " + FormatFloat(meters) + "m.");
        }

        MYB89RideTrajectory.TrySample(route, Mathf.Min(MYB89RideTrajectory.Length(route), meters + 18f), false, out var lookAhead);
        var forward = Vector3.Slerp(sample.Forward, lookAhead.Forward, 0.65f).normalized;
        camera.transform.position = sample.Position - forward * ProbeCameraBackwardMeters + Vector3.up * ProbeCameraHeight;
        camera.transform.rotation = Quaternion.LookRotation(forward, Vector3.up) * Quaternion.Euler(ProbeCameraPitchDegrees, 0f, 0f);
    }

    private static VisibleStats CountVisible(IEnumerable<Renderer> renderers, Camera camera)
    {
        var planes = GeometryUtility.CalculateFrustumPlanes(camera);
        var materials = new HashSet<Material>();
        var stats = new VisibleStats();
        foreach (var renderer in renderers)
        {
            if (renderer == null || !GeometryUtility.TestPlanesAABB(planes, renderer.bounds))
            {
                continue;
            }

            stats.VisibleRendererCount++;
            if (IsShadowCaster(renderer))
            {
                stats.VisibleShadowCasterCount++;
            }

            if (renderer.receiveShadows)
            {
                stats.VisibleShadowReceiverCount++;
            }

            stats.VisibleTriangleCount += TriangleCount(renderer);
            foreach (var material in renderer.sharedMaterials)
            {
                if (material != null)
                {
                    materials.Add(material);
                }
            }
        }

        stats.VisibleMaterialCount = materials.Count;
        return stats;
    }

    private static int TriangleCount(Renderer renderer)
    {
        if (renderer.TryGetComponent(out MeshFilter meshFilter) && meshFilter.sharedMesh != null)
        {
            return meshFilter.sharedMesh.triangles.Length / 3;
        }

        if (renderer is SkinnedMeshRenderer skinned && skinned.sharedMesh != null)
        {
            return skinned.sharedMesh.triangles.Length / 3;
        }

        return 0;
    }

    private static bool IsShadowCaster(Renderer renderer)
    {
        return renderer.shadowCastingMode != ShadowCastingMode.Off;
    }

    private static HashSet<Material> UniqueMaterials(IEnumerable<Renderer> renderers)
    {
        var materials = new HashSet<Material>();
        foreach (var renderer in renderers)
        {
            foreach (var material in renderer.sharedMaterials)
            {
                if (material != null)
                {
                    materials.Add(material);
                }
            }
        }

        return materials;
    }

    private static IEnumerable<float> Distances(float start, float end, float step)
    {
        var safeStep = Mathf.Max(0.1f, step);
        for (var meters = start; meters <= end; meters += safeStep)
        {
            yield return meters;
        }

        if (end > start)
        {
            yield return end;
        }
    }

    private static ScenarioSummary Summarize(IReadOnlyList<FrameSample> samples)
    {
        if (samples.Count == 0)
        {
            return new ScenarioSummary();
        }

        var ordered = samples.Select(sample => sample.RenderMs).OrderBy(value => value).ToArray();
        var worst = samples.OrderByDescending(sample => sample.RenderMs).First();
        return new ScenarioSummary
        {
            SampleCount = samples.Count,
            AverageRenderMs = samples.Average(sample => sample.RenderMs),
            MinRenderMs = samples.Min(sample => sample.RenderMs),
            MaxRenderMs = samples.Max(sample => sample.RenderMs),
            P95RenderMs = Percentile(ordered, 0.95d),
            AverageEstimatedFps = samples.Average(sample => sample.EstimatedFps),
            WorstMeters = worst.Meters,
            WorstVisibleRendererCount = worst.VisibleRendererCount,
            WorstVisibleShadowCasterCount = worst.VisibleShadowCasterCount,
            WorstVisibleTriangleCount = worst.VisibleTriangleCount,
            AverageVisibleRendererCount = samples.Average(sample => sample.VisibleRendererCount),
            AverageVisibleShadowCasterCount = samples.Average(sample => sample.VisibleShadowCasterCount),
            AverageVisibleTriangleCount = samples.Average(sample => sample.VisibleTriangleCount)
        };
    }

    private static double Percentile(IReadOnlyList<double> sortedValues, double percentile)
    {
        if (sortedValues.Count == 0)
        {
            return 0d;
        }

        var index = Math.Max(0d, Math.Min(sortedValues.Count - 1, (sortedValues.Count - 1) * percentile));
        var lower = (int)Math.Floor(index);
        var upper = (int)Math.Ceiling(index);
        if (lower == upper)
        {
            return sortedValues[lower];
        }

        var t = index - lower;
        return sortedValues[lower] * (1d - t) + sortedValues[upper] * t;
    }

    private static string[] Interpret(RenderProbeReport report)
    {
        var notes = new List<string>();
        var worstSummary = report.WorstCaseSummary;
        if (worstSummary.MaxRenderMs > 33.33d)
        {
            notes.Add("Worst-case render proxy exceeds 30 FPS frame budget.");
        }

        if (worstSummary.MaxRenderMs > 16.67d)
        {
            notes.Add("Worst-case render proxy exceeds 60 FPS frame budget.");
        }

        if (report.ShadowCasterCount == report.RendererCount && report.RendererCount > 0)
        {
            notes.Add("All active renderers cast shadows; shadow policy is a high-priority optimization candidate.");
        }

        if (report.LodGroupCount == 0)
        {
            notes.Add("No LODGroups are present; far scenery LOD/impostor work is a high-priority optimization candidate.");
        }

        return notes.ToArray();
    }

    private static void WriteJson(RenderProbeReport report)
    {
        File.WriteAllText(ToRepoPath(JsonRelativePath), ToJson(report));
    }

    private static void WriteMarkdown(RenderProbeReport report)
    {
        var builder = new StringBuilder();
        builder.AppendLine("# MYB-166 Route Camera Render Probe");
        builder.AppendLine();
        builder.AppendLine("- Scene: `" + report.ScenePath + "`");
        builder.AppendLine("- Route length: `" + FormatDouble(report.RouteLengthMeters) + "m`");
        builder.AppendLine("- Render target: `" + report.RenderWidth + "x" + report.RenderHeight + "`");
        builder.AppendLine("- Renderers: `" + report.RendererCount + "`");
        builder.AppendLine("- Shadow casters: `" + report.ShadowCasterCount + "`");
        builder.AppendLine("- Shadow receivers: `" + report.ShadowReceiverCount + "`");
        builder.AppendLine("- LODGroups: `" + report.LodGroupCount + "`");
        builder.AppendLine("- Active lights: `" + report.LightCount + "`");
        builder.AppendLine("- Materials: `" + report.MaterialCount + "`");
        builder.AppendLine();
        AppendSummary(builder, "Discovery", report.DiscoverySummary);
        AppendSummary(builder, "Worst-Case Slice", report.WorstCaseSummary);
        AppendSummary(builder, "Full Route Validation", report.FullRouteSummary);
        builder.AppendLine("## Interpretation");
        builder.AppendLine();
        foreach (var note in report.Interpretation)
        {
            builder.AppendLine("- " + note);
        }

        builder.AppendLine();
        builder.AppendLine("## Caveat");
        builder.AppendLine();
        builder.AppendLine("This probe uses `Camera.Render` wall-clock timings in the Unity Editor. It is a render-cost proxy for comparing candidate optimizations, not a final Play Mode FPS benchmark.");
        File.WriteAllText(ToRepoPath(ReportRelativePath), builder.ToString());
    }

    private static void AppendSummary(StringBuilder builder, string title, ScenarioSummary summary)
    {
        builder.AppendLine("## " + title);
        builder.AppendLine();
        builder.AppendLine("- Samples: `" + summary.SampleCount + "`");
        builder.AppendLine("- Average render: `" + FormatDouble(summary.AverageRenderMs) + "ms`");
        builder.AppendLine("- P95 render: `" + FormatDouble(summary.P95RenderMs) + "ms`");
        builder.AppendLine("- Max render: `" + FormatDouble(summary.MaxRenderMs) + "ms` at `" + FormatDouble(summary.WorstMeters) + "m`");
        builder.AppendLine("- Average estimated FPS proxy: `" + FormatDouble(summary.AverageEstimatedFps) + "`");
        builder.AppendLine("- Worst visible renderers: `" + summary.WorstVisibleRendererCount + "`");
        builder.AppendLine("- Worst visible shadow casters: `" + summary.WorstVisibleShadowCasterCount + "`");
        builder.AppendLine("- Worst visible triangles: `" + summary.WorstVisibleTriangleCount + "`");
        builder.AppendLine();
    }

    private static string ToJson(RenderProbeReport report)
    {
        var builder = new StringBuilder();
        builder.AppendLine("{");
        JsonProperty(builder, "ticket", report.Ticket, 1, true);
        JsonProperty(builder, "timestampUtc", report.TimestampUtc, 1, true);
        JsonProperty(builder, "scenePath", report.ScenePath, 1, true);
        JsonProperty(builder, "routeLengthMeters", report.RouteLengthMeters, 1, true);
        JsonProperty(builder, "renderWidth", report.RenderWidth, 1, true);
        JsonProperty(builder, "renderHeight", report.RenderHeight, 1, true);
        JsonProperty(builder, "rendererCount", report.RendererCount, 1, true);
        JsonProperty(builder, "shadowCasterCount", report.ShadowCasterCount, 1, true);
        JsonProperty(builder, "shadowReceiverCount", report.ShadowReceiverCount, 1, true);
        JsonProperty(builder, "lodGroupCount", report.LodGroupCount, 1, true);
        JsonProperty(builder, "lightCount", report.LightCount, 1, true);
        JsonProperty(builder, "materialCount", report.MaterialCount, 1, true);
        JsonProperty(builder, "qualityLevel", report.QualityLevel, 1, true);
        JsonProperty(builder, "vSyncCount", report.VSyncCount, 1, true);
        JsonProperty(builder, "targetFrameRate", report.TargetFrameRate, 1, true);
        JsonProperty(builder, "worstCaseCenterMeters", report.WorstCaseCenterMeters, 1, true);
        JsonSummary(builder, "discoverySummary", report.DiscoverySummary, 1, true);
        JsonSummary(builder, "worstCaseSummary", report.WorstCaseSummary, 1, true);
        JsonSummary(builder, "fullRouteSummary", report.FullRouteSummary, 1, true);
        JsonSamples(builder, "discoverySamples", report.DiscoverySamples, 1, true);
        JsonSamples(builder, "worstCaseSamples", report.WorstCaseSamples, 1, true);
        JsonSamples(builder, "fullRouteValidationSamples", report.FullRouteValidationSamples, 1, true);
        JsonArray(builder, "interpretation", report.Interpretation, 1, true);
        JsonArray(builder, "notes", report.Notes, 1, false);
        builder.AppendLine("}");
        return builder.ToString();
    }

    private static void JsonSummary(StringBuilder builder, string name, ScenarioSummary summary, int indent, bool comma)
    {
        Indent(builder, indent).Append("\"").Append(name).AppendLine("\": {");
        JsonProperty(builder, "sampleCount", summary.SampleCount, indent + 1, true);
        JsonProperty(builder, "averageRenderMs", summary.AverageRenderMs, indent + 1, true);
        JsonProperty(builder, "minRenderMs", summary.MinRenderMs, indent + 1, true);
        JsonProperty(builder, "maxRenderMs", summary.MaxRenderMs, indent + 1, true);
        JsonProperty(builder, "p95RenderMs", summary.P95RenderMs, indent + 1, true);
        JsonProperty(builder, "averageEstimatedFps", summary.AverageEstimatedFps, indent + 1, true);
        JsonProperty(builder, "worstMeters", summary.WorstMeters, indent + 1, true);
        JsonProperty(builder, "worstVisibleRendererCount", summary.WorstVisibleRendererCount, indent + 1, true);
        JsonProperty(builder, "worstVisibleShadowCasterCount", summary.WorstVisibleShadowCasterCount, indent + 1, true);
        JsonProperty(builder, "worstVisibleTriangleCount", summary.WorstVisibleTriangleCount, indent + 1, true);
        JsonProperty(builder, "averageVisibleRendererCount", summary.AverageVisibleRendererCount, indent + 1, true);
        JsonProperty(builder, "averageVisibleShadowCasterCount", summary.AverageVisibleShadowCasterCount, indent + 1, true);
        JsonProperty(builder, "averageVisibleTriangleCount", summary.AverageVisibleTriangleCount, indent + 1, false);
        Indent(builder, indent).Append("}");
        builder.AppendLine(comma ? "," : string.Empty);
    }

    private static void JsonSamples(StringBuilder builder, string name, IReadOnlyList<FrameSample> samples, int indent, bool comma)
    {
        Indent(builder, indent).Append("\"").Append(name).AppendLine("\": [");
        for (var i = 0; i < samples.Count; i++)
        {
            var sample = samples[i];
            Indent(builder, indent + 1).AppendLine("{");
            JsonProperty(builder, "scenario", sample.Scenario, indent + 2, true);
            JsonProperty(builder, "meters", sample.Meters, indent + 2, true);
            JsonProperty(builder, "renderMs", sample.RenderMs, indent + 2, true);
            JsonProperty(builder, "estimatedFps", sample.EstimatedFps, indent + 2, true);
            JsonProperty(builder, "visibleRendererCount", sample.VisibleRendererCount, indent + 2, true);
            JsonProperty(builder, "visibleShadowCasterCount", sample.VisibleShadowCasterCount, indent + 2, true);
            JsonProperty(builder, "visibleShadowReceiverCount", sample.VisibleShadowReceiverCount, indent + 2, true);
            JsonProperty(builder, "visibleTriangleCount", sample.VisibleTriangleCount, indent + 2, true);
            JsonProperty(builder, "visibleMaterialCount", sample.VisibleMaterialCount, indent + 2, false);
            Indent(builder, indent + 1).Append("}");
            builder.AppendLine(i == samples.Count - 1 ? string.Empty : ",");
        }

        Indent(builder, indent).Append("]");
        builder.AppendLine(comma ? "," : string.Empty);
    }

    private static void JsonArray(StringBuilder builder, string name, IReadOnlyList<string> values, int indent, bool comma)
    {
        Indent(builder, indent).Append("\"").Append(name).AppendLine("\": [");
        for (var i = 0; i < values.Count; i++)
        {
            Indent(builder, indent + 1).Append("\"").Append(EscapeJson(values[i])).Append("\"");
            builder.AppendLine(i == values.Count - 1 ? string.Empty : ",");
        }

        Indent(builder, indent).Append("]");
        builder.AppendLine(comma ? "," : string.Empty);
    }

    private static void JsonProperty(StringBuilder builder, string name, string value, int indent, bool comma)
    {
        Indent(builder, indent).Append("\"").Append(name).Append("\": \"").Append(EscapeJson(value)).Append("\"");
        builder.AppendLine(comma ? "," : string.Empty);
    }

    private static void JsonProperty(StringBuilder builder, string name, int value, int indent, bool comma)
    {
        Indent(builder, indent).Append("\"").Append(name).Append("\": ").Append(value.ToString(CultureInfo.InvariantCulture));
        builder.AppendLine(comma ? "," : string.Empty);
    }

    private static void JsonProperty(StringBuilder builder, string name, double value, int indent, bool comma)
    {
        Indent(builder, indent).Append("\"").Append(name).Append("\": ").Append(FormatDouble(value));
        builder.AppendLine(comma ? "," : string.Empty);
    }

    private static StringBuilder Indent(StringBuilder builder, int indent)
    {
        return builder.Append(' ', indent * 2);
    }

    private static string EscapeJson(string value)
    {
        return (value ?? string.Empty)
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"")
            .Replace("\n", "\\n")
            .Replace("\r", "\\r");
    }

    private static string FormatFloat(float value)
    {
        return value.ToString("0.###", CultureInfo.InvariantCulture);
    }

    private static string FormatDouble(double value)
    {
        return value.ToString("0.###", CultureInfo.InvariantCulture);
    }

    private static string ToRepoPath(string relativePath)
    {
        return Path.GetFullPath(Path.Combine(Application.dataPath, "../../..", relativePath));
    }

    private sealed class CameraState
    {
        public Vector3 Position;
        public Quaternion Rotation;
        public float FieldOfView;
        public float NearClip;
        public float FarClip;
        public bool Orthographic;
        public RenderTexture TargetTexture;

        public static CameraState Capture(Camera camera)
        {
            return new CameraState
            {
                Position = camera.transform.position,
                Rotation = camera.transform.rotation,
                FieldOfView = camera.fieldOfView,
                NearClip = camera.nearClipPlane,
                FarClip = camera.farClipPlane,
                Orthographic = camera.orthographic,
                TargetTexture = camera.targetTexture
            };
        }

        public void Restore(Camera camera)
        {
            camera.transform.position = Position;
            camera.transform.rotation = Rotation;
            camera.fieldOfView = FieldOfView;
            camera.nearClipPlane = NearClip;
            camera.farClipPlane = FarClip;
            camera.orthographic = Orthographic;
            camera.targetTexture = TargetTexture;
        }
    }

    private sealed class VisibleStats
    {
        public int VisibleRendererCount;
        public int VisibleShadowCasterCount;
        public int VisibleShadowReceiverCount;
        public int VisibleTriangleCount;
        public int VisibleMaterialCount;
    }

    private sealed class FrameSample
    {
        public string Scenario = string.Empty;
        public float Meters;
        public double RenderMs;
        public double EstimatedFps;
        public int VisibleRendererCount;
        public int VisibleShadowCasterCount;
        public int VisibleShadowReceiverCount;
        public int VisibleTriangleCount;
        public int VisibleMaterialCount;
    }

    private sealed class ScenarioSummary
    {
        public int SampleCount;
        public double AverageRenderMs;
        public double MinRenderMs;
        public double MaxRenderMs;
        public double P95RenderMs;
        public double AverageEstimatedFps;
        public double WorstMeters;
        public int WorstVisibleRendererCount;
        public int WorstVisibleShadowCasterCount;
        public int WorstVisibleTriangleCount;
        public double AverageVisibleRendererCount;
        public double AverageVisibleShadowCasterCount;
        public double AverageVisibleTriangleCount;
    }

    private sealed class RenderProbeReport
    {
        public string Ticket = string.Empty;
        public string TimestampUtc = string.Empty;
        public string ScenePath = string.Empty;
        public float RouteLengthMeters;
        public int RenderWidth;
        public int RenderHeight;
        public int RendererCount;
        public int ShadowCasterCount;
        public int ShadowReceiverCount;
        public int LodGroupCount;
        public int LightCount;
        public int MaterialCount;
        public string QualityLevel = string.Empty;
        public int VSyncCount;
        public int TargetFrameRate;
        public float WorstCaseCenterMeters;
        public ScenarioSummary DiscoverySummary = new ScenarioSummary();
        public ScenarioSummary WorstCaseSummary = new ScenarioSummary();
        public ScenarioSummary FullRouteSummary = new ScenarioSummary();
        public List<FrameSample> DiscoverySamples = new List<FrameSample>();
        public List<FrameSample> WorstCaseSamples = new List<FrameSample>();
        public List<FrameSample> FullRouteValidationSamples = new List<FrameSample>();
        public string[] Interpretation = Array.Empty<string>();
        public string[] Notes = Array.Empty<string>();
    }
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class MYB156VisualSupportValidator
{
    private const string MenuPath = "Tools/MyBike/Validation/MYB-156 Visual Support Validator";
    private const string DefaultScenePath = "Assets/Scenes/MYB149GroundMaterialPreview.unity";
    private const string ScatterRootName = "MYB148_RouteFirstScatterAssets";
    private const string RouteCameraName = "RouteCamera";
    private const string ReportRelativePath = "_bmad-output/unity-test-results/myb-156-visual-support-validator-report.md";
    private const string MetricsRelativePath = "_bmad-output/unity-test-results/myb-156-visual-support-metrics.json";
    private const string ImplementationReportRelativePath = "_bmad-output/implementation-artifacts/MYB-156/myb-156-visual-support-validator-report.md";
    private const string EvidenceRouteBeforeAfter = "_bmad-output/visual-checkpoints/MYB-149/2026-06-16T19-30-40Z-route-before-after.png";
    private const string EvidenceOverviewBeforeAfter = "_bmad-output/visual-checkpoints/MYB-149/2026-06-16T19-30-40Z-overview-before-after.png";
    private const float SupportSearchRadiusMeters = 4.0f;
    private const float SupportVerticalGapMeters = 0.75f;

    [MenuItem(MenuPath)]
    public static void RunFromMenu()
    {
        var result = RunValidation("Menu");
        var summary = result.ToConsoleSummary();
        if (result.ErrorCount > 0)
        {
            Debug.LogError(summary);
            return;
        }

        if (result.WarningCount > 0)
        {
            Debug.LogWarning(summary);
            return;
        }

        Debug.Log(summary);
    }

    public static void RunBatch()
    {
        var result = RunValidation("Batch");
        var summary = result.ToConsoleSummary();
        if (result.ErrorCount > 0)
        {
            Debug.LogError(summary);
            EditorApplication.Exit(1);
            return;
        }

        if (result.WarningCount > 0)
        {
            Debug.LogWarning(summary);
            EditorApplication.Exit(0);
            return;
        }

        Debug.Log(summary);
        EditorApplication.Exit(0);
    }

    public static ValidationResult RunValidation()
    {
        return RunValidation("Unknown");
    }

    public static ValidationResult RunValidation(string executionMode)
    {
        var result = new ValidationResult
        {
            ExecutionMode = string.IsNullOrWhiteSpace(executionMode) ? "Unknown" : executionMode,
            GeneratedAt = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture),
            RepoRoot = GetRepoRoot(),
            ScenePath = DefaultScenePath,
            ReportPathRelative = ReportRelativePath,
            MetricsPathRelative = MetricsRelativePath,
            ImplementationReportPathRelative = ImplementationReportRelativePath,
            VisualSupportMethod = "Name-classified supportedAboveGround canopy assets require nearby trunk bounds support.",
            RouteCameraVisibilityMethod = "GeometryUtility.TestPlanesAABB against RouteCamera frustum.",
            SupportSearchRadiusMeters = SupportSearchRadiusMeters,
            SupportVerticalGapMeters = SupportVerticalGapMeters
        };

        try
        {
            OpenTargetScene(result);
            AnalyzeScene(result);
        }
        catch (Exception exception)
        {
            result.AddError(
                "VALIDATOR_EXCEPTION",
                "MYB-156 validator",
                exception.GetType().FullName + ": " + exception.Message,
                "Fix the validator exception before trusting visual-support output.");
            result.AddInfo("VALIDATOR_EXCEPTION_DETAIL", exception.ToString());
        }

        try
        {
            WriteReport(result);
            WriteMetricsJson(result);
            WriteImplementationReport(result);
        }
        catch (Exception exception)
        {
            result.AddError(
                "REPORT_WRITE_FAILED",
                ReportRelativePath,
                exception.GetType().FullName + ": " + exception.Message,
                "Ensure the report directory is writable.");
        }

        return result;
    }

    private static void OpenTargetScene(ValidationResult result)
    {
        if (!File.Exists(ToProjectPath(DefaultScenePath)))
        {
            result.AddError(
                "SCENE_MISSING",
                DefaultScenePath,
                "Default MYB-156 validation scene is missing.",
                "Build MYB-149 or pass an equivalent scene into a future validator entry point.");
            return;
        }

        var active = SceneManager.GetActiveScene();
        if (!string.Equals(active.path, DefaultScenePath, StringComparison.Ordinal))
        {
            EditorSceneManager.OpenScene(DefaultScenePath, OpenSceneMode.Single);
        }

        result.ScenePath = SceneManager.GetActiveScene().path;
        result.SceneDirtyAfterValidation = SceneManager.GetActiveScene().isDirty;
    }

    private static void AnalyzeScene(ValidationResult result)
    {
        var scene = SceneManager.GetActiveScene();
        result.ScenePath = scene.path;

        var scatterRoot = GameObject.Find(ScatterRootName);
        if (scatterRoot == null)
        {
            result.AddError(
                "SCATTER_ROOT_MISSING",
                ScatterRootName,
                "Scene does not contain the MYB-148 scatter root expected by MYB-156.",
                "Run this validator on a MYB-148/MYB-149 style forest corridor preview scene.");
            return;
        }

        var routeCameraObject = GameObject.Find(RouteCameraName);
        var routeCamera = routeCameraObject == null ? null : routeCameraObject.GetComponent<Camera>();
        if (routeCamera == null)
        {
            result.AddWarning(
                "ROUTE_CAMERA_MISSING",
                RouteCameraName,
                "RouteCamera was not found; route-visible blocker classification is degraded.",
                "Use the canonical route camera for visual-support validation.");
        }

        var routePlanes = routeCamera == null ? null : GeometryUtility.CalculateFrustumPlanes(routeCamera);
        var assets = scatterRoot.GetComponentsInChildren<Transform>(true)
            .Where(transform => transform.parent == scatterRoot.transform && transform.gameObject.activeInHierarchy)
            .Select(CreateAssetRecord)
            .Where(record => record != null)
            .OrderBy(record => record.Name, StringComparer.Ordinal)
            .ToList();

        result.AssetRecords.AddRange(assets);
        result.AssetCount = assets.Count;
        result.GroundedAssetCount = assets.Count(record => record.Role == VisualRole.Grounded);
        result.SupportedAboveGroundAssetCount = assets.Count(record => record.Role == VisualRole.SupportedAboveGround);

        foreach (var asset in assets)
        {
            asset.RouteVisible = routePlanes != null && GeometryUtility.TestPlanesAABB(routePlanes, asset.Bounds);
        }

        var supportCandidates = assets
            .Where(record => record.Role == VisualRole.Grounded && record.Family == "trunk")
            .ToList();

        foreach (var asset in assets.Where(record => record.Role == VisualRole.SupportedAboveGround))
        {
            EvaluateAboveGroundSupport(result, asset, supportCandidates);
        }

        result.UnsupportedCanopyCount = result.SupportFindings.Count(finding => finding.IsUnsupported && !finding.HasDocumentedException);
        result.RouteVisibleUnsupportedCanopyCount = result.SupportFindings.Count(finding => finding.IsUnsupported && finding.RouteVisible && !finding.HasDocumentedException);
        result.CanopyWithoutTrunkCount = result.SupportFindings.Count(finding => !finding.HasSupportWithinRadius);
        result.FloatingVisualRiskCount = result.SupportFindings.Count(finding => finding.IsUnsupported && finding.RouteVisible && !finding.HasDocumentedException);
        result.DocumentedFloatingExceptionCount = result.SupportFindings.Count(finding => finding.HasDocumentedException);
        result.RouteVisibleFloatingExceptionCount = result.SupportFindings.Count(finding => finding.HasDocumentedException && finding.RouteVisible);
        result.MaxCanopySupportGap = result.SupportFindings.Count == 0
            ? 0f
            : result.SupportFindings.Max(finding => finding.SupportVerticalGapMeters);

        foreach (var finding in result.SupportFindings.Where(finding => finding.IsUnsupported && !finding.HasDocumentedException))
        {
            if (finding.RouteVisible)
            {
                result.AddError(
                    "ROUTE_VISIBLE_UNSUPPORTED_CANOPY",
                    finding.AssetName,
                    "Route-visible canopy/leaf mass lacks credible trunk support. verticalGap="
                    + FormatFloat(finding.SupportVerticalGapMeters)
                    + "m, horizontalGap="
                    + FormatFloat(finding.SupportHorizontalGapMeters)
                    + "m.",
                    "Attach the canopy to a credible visual support, add explicit accepted exception evidence, or remove/rework the unsupported overhead asset.");
            }
            else
            {
                result.AddWarning(
                    "UNSUPPORTED_CANOPY_OFF_CAMERA",
                    finding.AssetName,
                    "Canopy/leaf mass lacks credible support but is not route-visible in the current camera frustum.",
                    "Keep as follow-up risk if another route camera or capture angle can see it.");
            }
        }

        result.SceneDirtyAfterValidation = scene.isDirty;
    }

    private static AssetRecord CreateAssetRecord(Transform transform)
    {
        var renderers = transform.GetComponentsInChildren<Renderer>(true)
            .Where(renderer => renderer.enabled && renderer.gameObject.activeInHierarchy)
            .ToArray();
        if (renderers.Length == 0)
        {
            return null;
        }

        var bounds = renderers[0].bounds;
        for (var i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }

        var family = FamilyOf(transform.name);
        var role = family == "canopy" ? VisualRole.SupportedAboveGround : VisualRole.Grounded;
        return new AssetRecord
        {
            Name = transform.name,
            Path = PathOf(transform.gameObject),
            Family = family,
            Role = role,
            Bounds = bounds,
            RendererCount = renderers.Length
        };
    }

    private static void EvaluateAboveGroundSupport(
        ValidationResult result,
        AssetRecord asset,
        IReadOnlyList<AssetRecord> supportCandidates)
    {
        SupportFinding finding;
        if (supportCandidates.Count == 0)
        {
            finding = new SupportFinding
            {
                AssetName = asset.Name,
                AssetPath = asset.Path,
                RouteVisible = asset.RouteVisible,
                HasDocumentedException = HasDocumentedException(asset.Name),
                HasSupportWithinRadius = false,
                NearestSupportName = "",
                SupportHorizontalGapMeters = SupportSearchRadiusMeters,
                SupportVerticalGapMeters = asset.Bounds.min.y,
                IsUnsupported = true
            };
            result.SupportFindings.Add(finding);
            return;
        }

        var nearest = supportCandidates
            .Select(candidate => new SupportCandidate
            {
                Record = candidate,
                HorizontalGap = HorizontalGap(asset.Bounds, candidate.Bounds),
                VerticalGap = asset.Bounds.min.y - candidate.Bounds.max.y
            })
            .OrderBy(candidate => candidate.HorizontalGap)
            .ThenBy(candidate => Mathf.Max(0f, candidate.VerticalGap))
            .First();

        var hasSupportWithinRadius = nearest.HorizontalGap <= SupportSearchRadiusMeters;
        var hasVerticalSupport = nearest.VerticalGap <= SupportVerticalGapMeters;
        finding = new SupportFinding
        {
            AssetName = asset.Name,
            AssetPath = asset.Path,
            RouteVisible = asset.RouteVisible,
            HasDocumentedException = HasDocumentedException(asset.Name),
            HasSupportWithinRadius = hasSupportWithinRadius,
            NearestSupportName = nearest.Record.Name,
            SupportHorizontalGapMeters = nearest.HorizontalGap,
            SupportVerticalGapMeters = Mathf.Max(0f, nearest.VerticalGap),
            IsUnsupported = !hasSupportWithinRadius || !hasVerticalSupport
        };
        result.SupportFindings.Add(finding);
    }

    private static float HorizontalGap(Bounds a, Bounds b)
    {
        var xGap = Mathf.Max(0f, Mathf.Max(a.min.x - b.max.x, b.min.x - a.max.x));
        var zGap = Mathf.Max(0f, Mathf.Max(a.min.z - b.max.z, b.min.z - a.max.z));
        return Mathf.Sqrt(xGap * xGap + zGap * zGap);
    }

    private static bool HasDocumentedException(string name)
    {
        return name.IndexOf("visual_support_exception", StringComparison.OrdinalIgnoreCase) >= 0
            || name.IndexOf("floating_exception", StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private static string FamilyOf(string name)
    {
        var lower = name.ToLowerInvariant();
        if (lower.Contains("trunk")) return "trunk";
        if (lower.Contains("canopy")) return "canopy";
        if (lower.Contains("rock")) return "rock";
        if (lower.Contains("root")) return "root";
        if (lower.Contains("fern")) return "fern";
        if (lower.Contains("leaf_moss") || lower.Contains("moss_mat")) return "leaf/moss mat";
        if (lower.Contains("fallen_log")) return "fallen log";
        if (lower.Contains("dead_branch")) return "dead branch";
        return "other";
    }

    private static string PathOf(GameObject go)
    {
        var names = new List<string>();
        var transform = go.transform;
        while (transform != null)
        {
            names.Add(transform.name);
            transform = transform.parent;
        }

        names.Reverse();
        return string.Join("/", names);
    }

    private static void WriteReport(ValidationResult result)
    {
        var path = ToRepoPath(ReportRelativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(path) ?? result.RepoRoot);
        File.WriteAllText(path, BuildMarkdownReport(result, "MYB-156 Visual Support Validator Report"));
    }

    private static void WriteImplementationReport(ValidationResult result)
    {
        var path = ToRepoPath(ImplementationReportRelativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(path) ?? result.RepoRoot);
        File.WriteAllText(path, BuildMarkdownReport(result, "MYB-156 Implementation Report"));
    }

    private static string BuildMarkdownReport(ValidationResult result, string title)
    {
        var builder = new StringBuilder();
        builder.AppendLine("# " + title);
        builder.AppendLine();
        builder.AppendLine("Status:");
        builder.AppendLine("- " + result.Verdict);
        builder.AppendLine("- MYB-156 should stay In Review until Julien validates the visual-support wording and blocker policy.");
        builder.AppendLine();
        builder.AppendLine("Generated at:");
        builder.AppendLine("- " + result.GeneratedAt);
        builder.AppendLine();
        builder.AppendLine("Scope:");
        builder.AppendLine("- Validator/governance hardening for route-visible visual support.");
        builder.AppendLine("- No Unity scene save.");
        builder.AppendLine("- No gameplay change.");
        builder.AppendLine("- No generated or imported assets.");
        builder.AppendLine("- No Blender, Meshy, Tripo, Poly Haven, or external asset call.");
        builder.AppendLine();
        builder.AppendLine("Scene:");
        builder.AppendLine("- `" + result.ScenePath + "`");
        builder.AppendLine("- Dirty after validation: " + YesNo(result.SceneDirtyAfterValidation));
        builder.AppendLine();
        builder.AppendLine("Evidence that triggered MYB-156:");
        builder.AppendLine("- Route before/after: `" + EvidenceRouteBeforeAfter + "`");
        builder.AppendLine("- Overview before/after: `" + EvidenceOverviewBeforeAfter + "`");
        builder.AppendLine();
        builder.AppendLine("## Detection Model");
        builder.AppendLine();
        builder.AppendLine("- `grounded`: trunks, rocks, roots, ferns, fallen logs/branches, leaf/moss mats, other non-canopy scatter assets.");
        builder.AppendLine("- `supportedAboveGround`: canopy or elevated leaf mass assets.");
        builder.AppendLine("- `exemptFloating`: object names containing `visual_support_exception` or `floating_exception`.");
        builder.AppendLine();
        builder.AppendLine("Support rule:");
        builder.AppendLine("- route-visible `supportedAboveGround` assets need a nearby trunk support.");
        builder.AppendLine("- support search radius: " + FormatFloat(SupportSearchRadiusMeters) + "m.");
        builder.AppendLine("- maximum allowed vertical gap between trunk top and canopy bottom: " + FormatFloat(SupportVerticalGapMeters) + "m.");
        builder.AppendLine();
        builder.AppendLine("Why MYB-155 alone missed this:");
        builder.AppendLine("- MYB-155 measures visual-bottom ground contact for grounded assets.");
        builder.AppendLine("- The MYB-149 scan found no floating grounded assets, but route-visible canopy masses can still read as unsupported because their issue is visual support, not ground contact.");
        builder.AppendLine();
        builder.AppendLine("## Metrics");
        builder.AppendLine();
        builder.AppendLine("- assetCount: " + result.AssetCount);
        builder.AppendLine("- groundedAssetCount: " + result.GroundedAssetCount);
        builder.AppendLine("- supportedAboveGroundAssetCount: " + result.SupportedAboveGroundAssetCount);
        builder.AppendLine("- unsupportedCanopyCount: " + result.UnsupportedCanopyCount);
        builder.AppendLine("- routeVisibleUnsupportedCanopyCount: " + result.RouteVisibleUnsupportedCanopyCount);
        builder.AppendLine("- maxCanopySupportGap: " + FormatFloat(result.MaxCanopySupportGap) + "m");
        builder.AppendLine("- canopyWithoutTrunkCount: " + result.CanopyWithoutTrunkCount);
        builder.AppendLine("- floatingVisualRiskCount: " + result.FloatingVisualRiskCount);
        builder.AppendLine("- documentedFloatingExceptionCount: " + result.DocumentedFloatingExceptionCount);
        builder.AppendLine("- routeVisibleFloatingExceptionCount: " + result.RouteVisibleFloatingExceptionCount);
        builder.AppendLine("- visualSupportMethod: " + result.VisualSupportMethod);
        builder.AppendLine("- routeCameraVisibilityMethod: " + result.RouteCameraVisibilityMethod);
        builder.AppendLine("- supportSearchRadiusMeters: " + FormatFloat(result.SupportSearchRadiusMeters));
        builder.AppendLine("- supportVerticalGapMeters: " + FormatFloat(result.SupportVerticalGapMeters));
        builder.AppendLine("- metricsJson: `" + MetricsRelativePath + "`");
        builder.AppendLine();
        builder.AppendLine("## Findings");
        builder.AppendLine();
        AppendMessages(builder, "Errors", result.Errors);
        AppendMessages(builder, "Warnings", result.Warnings);
        AppendMessages(builder, "Info", result.Info);
        builder.AppendLine();
        builder.AppendLine("## Above-Ground Assets");
        builder.AppendLine();
        builder.AppendLine("| Asset | Route visible | Unsupported | Nearest support | Horizontal gap | Vertical gap | Exception |");
        builder.AppendLine("|---|---:|---:|---|---:|---:|---:|");
        foreach (var finding in result.SupportFindings.OrderByDescending(finding => finding.RouteVisible).ThenByDescending(finding => finding.SupportVerticalGapMeters))
        {
            builder.AppendLine("| `" + finding.AssetName + "` | "
                + YesNo(finding.RouteVisible) + " | "
                + YesNo(finding.IsUnsupported) + " | `"
                + EscapeMarkdown(finding.NearestSupportName) + "` | "
                + FormatFloat(finding.SupportHorizontalGapMeters) + "m | "
                + FormatFloat(finding.SupportVerticalGapMeters) + "m | "
                + YesNo(finding.HasDocumentedException) + " |");
        }
        builder.AppendLine();
        builder.AppendLine("## Verdict");
        builder.AppendLine();
        builder.AppendLine("- " + result.Verdict);
        builder.AppendLine("- Route-visible unsupported canopy/leaf mass is blocking unless Julien accepts a documented exception.");
        return builder.ToString();
    }

    private static void AppendMessages(StringBuilder builder, string title, IReadOnlyList<ValidationMessage> messages)
    {
        builder.AppendLine("### " + title);
        builder.AppendLine();
        if (messages.Count == 0)
        {
            builder.AppendLine("- None.");
            builder.AppendLine();
            return;
        }

        foreach (var message in messages)
        {
            builder.AppendLine("- `" + message.Code + "` `" + message.Subject + "` - " + message.Message + " Action: " + message.Action);
        }
        builder.AppendLine();
    }

    private static void WriteMetricsJson(ValidationResult result)
    {
        var path = ToRepoPath(MetricsRelativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(path) ?? result.RepoRoot);
        var builder = new StringBuilder();
        builder.AppendLine("{");
        builder.AppendLine("  \"schemaVersion\": 1,");
        builder.AppendLine("  \"ticket\": \"MYB-156\",");
        builder.AppendLine("  \"generatedAt\": \"" + EscapeJson(result.GeneratedAt) + "\",");
        builder.AppendLine("  \"scene\": \"" + EscapeJson(result.ScenePath) + "\",");
        builder.AppendLine("  \"verdict\": \"" + EscapeJson(result.Verdict) + "\",");
        builder.AppendLine("  \"visualSupportMethod\": \"" + EscapeJson(result.VisualSupportMethod) + "\",");
        builder.AppendLine("  \"routeCameraVisibilityMethod\": \"" + EscapeJson(result.RouteCameraVisibilityMethod) + "\",");
        builder.AppendLine("  \"supportSearchRadiusMeters\": " + FormatJson(result.SupportSearchRadiusMeters) + ",");
        builder.AppendLine("  \"supportVerticalGapMeters\": " + FormatJson(result.SupportVerticalGapMeters) + ",");
        builder.AppendLine("  \"assetCount\": " + result.AssetCount + ",");
        builder.AppendLine("  \"groundedAssetCount\": " + result.GroundedAssetCount + ",");
        builder.AppendLine("  \"supportedAboveGroundAssetCount\": " + result.SupportedAboveGroundAssetCount + ",");
        builder.AppendLine("  \"unsupportedCanopyCount\": " + result.UnsupportedCanopyCount + ",");
        builder.AppendLine("  \"routeVisibleUnsupportedCanopyCount\": " + result.RouteVisibleUnsupportedCanopyCount + ",");
        builder.AppendLine("  \"maxCanopySupportGap\": " + FormatJson(result.MaxCanopySupportGap) + ",");
        builder.AppendLine("  \"canopyWithoutTrunkCount\": " + result.CanopyWithoutTrunkCount + ",");
        builder.AppendLine("  \"floatingVisualRiskCount\": " + result.FloatingVisualRiskCount + ",");
        builder.AppendLine("  \"documentedFloatingExceptionCount\": " + result.DocumentedFloatingExceptionCount + ",");
        builder.AppendLine("  \"routeVisibleFloatingExceptionCount\": " + result.RouteVisibleFloatingExceptionCount + ",");
        builder.AppendLine("  \"errors\": " + result.ErrorCount + ",");
        builder.AppendLine("  \"warnings\": " + result.WarningCount + ",");
        builder.AppendLine("  \"aboveGroundAssets\": [");
        for (var i = 0; i < result.SupportFindings.Count; i++)
        {
            var finding = result.SupportFindings[i];
            builder.AppendLine("    {");
            builder.AppendLine("      \"assetName\": \"" + EscapeJson(finding.AssetName) + "\",");
            builder.AppendLine("      \"assetPath\": \"" + EscapeJson(finding.AssetPath) + "\",");
            builder.AppendLine("      \"routeVisible\": " + BoolJson(finding.RouteVisible) + ",");
            builder.AppendLine("      \"unsupported\": " + BoolJson(finding.IsUnsupported) + ",");
            builder.AppendLine("      \"nearestSupportName\": \"" + EscapeJson(finding.NearestSupportName) + "\",");
            builder.AppendLine("      \"hasSupportWithinRadius\": " + BoolJson(finding.HasSupportWithinRadius) + ",");
            builder.AppendLine("      \"supportHorizontalGapMeters\": " + FormatJson(finding.SupportHorizontalGapMeters) + ",");
            builder.AppendLine("      \"supportVerticalGapMeters\": " + FormatJson(finding.SupportVerticalGapMeters) + ",");
            builder.AppendLine("      \"documentedException\": " + BoolJson(finding.HasDocumentedException));
            builder.Append("    }");
            if (i < result.SupportFindings.Count - 1)
            {
                builder.Append(",");
            }
            builder.AppendLine();
        }
        builder.AppendLine("  ]");
        builder.AppendLine("}");
        File.WriteAllText(path, builder.ToString());
    }

    private static string GetRepoRoot()
    {
        var assets = new DirectoryInfo(Application.dataPath);
        return assets.Parent?.Parent?.Parent?.FullName ?? Directory.GetCurrentDirectory();
    }

    private static string ToProjectPath(string assetRelativePath)
    {
        var normalized = assetRelativePath.Replace('\\', '/');
        if (!normalized.StartsWith("Assets/", StringComparison.Ordinal))
        {
            return normalized;
        }

        return Path.Combine(Application.dataPath, normalized.Substring("Assets/".Length).Replace('/', Path.DirectorySeparatorChar));
    }

    private static string ToRepoPath(string relativePath)
    {
        return Path.Combine(GetRepoRoot(), relativePath.Replace('/', Path.DirectorySeparatorChar));
    }

    private static string FormatFloat(float value)
    {
        return value.ToString("0.###", CultureInfo.InvariantCulture);
    }

    private static string FormatJson(float value)
    {
        return value.ToString("0.######", CultureInfo.InvariantCulture);
    }

    private static string BoolJson(bool value)
    {
        return value ? "true" : "false";
    }

    private static string YesNo(bool value)
    {
        return value ? "Yes" : "No";
    }

    private static string EscapeJson(string value)
    {
        return (value ?? string.Empty)
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"")
            .Replace("\r", "\\r")
            .Replace("\n", "\\n");
    }

    private static string EscapeMarkdown(string value)
    {
        return (value ?? string.Empty).Replace("|", "\\|");
    }

    public enum VisualRole
    {
        Grounded,
        SupportedAboveGround
    }

    public sealed class AssetRecord
    {
        public string Name = "";
        public string Path = "";
        public string Family = "";
        public VisualRole Role;
        public Bounds Bounds;
        public int RendererCount;
        public bool RouteVisible;
    }

    private sealed class SupportCandidate
    {
        public AssetRecord Record;
        public float HorizontalGap;
        public float VerticalGap;
    }

    public sealed class SupportFinding
    {
        public string AssetName = "";
        public string AssetPath = "";
        public bool RouteVisible;
        public bool IsUnsupported;
        public bool HasSupportWithinRadius;
        public bool HasDocumentedException;
        public string NearestSupportName = "";
        public float SupportHorizontalGapMeters;
        public float SupportVerticalGapMeters;
    }

    public sealed class ValidationMessage
    {
        public string Code = "";
        public string Subject = "";
        public string Message = "";
        public string Action = "";
    }

    public sealed class ValidationResult
    {
        public string ExecutionMode = "";
        public string GeneratedAt = "";
        public string RepoRoot = "";
        public string ScenePath = "";
        public string ReportPathRelative = "";
        public string MetricsPathRelative = "";
        public string ImplementationReportPathRelative = "";
        public string VisualSupportMethod = "";
        public string RouteCameraVisibilityMethod = "";
        public float SupportSearchRadiusMeters;
        public float SupportVerticalGapMeters;
        public bool SceneDirtyAfterValidation;
        public int AssetCount;
        public int GroundedAssetCount;
        public int SupportedAboveGroundAssetCount;
        public int UnsupportedCanopyCount;
        public int RouteVisibleUnsupportedCanopyCount;
        public int CanopyWithoutTrunkCount;
        public int FloatingVisualRiskCount;
        public int DocumentedFloatingExceptionCount;
        public int RouteVisibleFloatingExceptionCount;
        public float MaxCanopySupportGap;
        public readonly List<ValidationMessage> Errors = new List<ValidationMessage>();
        public readonly List<ValidationMessage> Warnings = new List<ValidationMessage>();
        public readonly List<ValidationMessage> Info = new List<ValidationMessage>();
        public readonly List<AssetRecord> AssetRecords = new List<AssetRecord>();
        public readonly List<SupportFinding> SupportFindings = new List<SupportFinding>();

        public int ErrorCount => Errors.Count;
        public int WarningCount => Warnings.Count;
        public string Verdict => ErrorCount > 0 ? "FAIL" : WarningCount > 0 ? "PASS_WITH_WARNINGS" : "PASS";

        public void AddError(string code, string subject, string message, string action)
        {
            Errors.Add(new ValidationMessage { Code = code, Subject = subject, Message = message, Action = action });
        }

        public void AddWarning(string code, string subject, string message, string action)
        {
            Warnings.Add(new ValidationMessage { Code = code, Subject = subject, Message = message, Action = action });
        }

        public void AddInfo(string code, string message)
        {
            Info.Add(new ValidationMessage { Code = code, Subject = "Info", Message = message, Action = "None." });
        }

        public string ToConsoleSummary()
        {
            return "MYB-156 visual support validator: "
                + Verdict
                + " | routeVisibleUnsupportedCanopyCount="
                + RouteVisibleUnsupportedCanopyCount
                + " | unsupportedCanopyCount="
                + UnsupportedCanopyCount
                + " | report="
                + ReportPathRelative;
        }
    }
}

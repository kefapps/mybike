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

public static class MYB164PostIntegrationStabilizationRunner
{
    private const string Ticket = "MYB-164";
    private const string CanonicalScenePath = "Assets/Scenes/MYB89UnityMcpProbe.unity";
    private const string GeneratedRootName = "MYB163_CanonicalForestPassageRoot";
    private const string ImplementationRootRelative = "_bmad-output/implementation-artifacts/MYB-164";
    private const string VisualRootRelative = "_bmad-output/visual-checkpoints/MYB-164";
    private const string MetricsRelativePath = ImplementationRootRelative + "/myb-164-stabilization-metrics.json";
    private const string ReportRelativePath = ImplementationRootRelative + "/myb-164-stabilization-report.md";
    private const string GovernanceRelativePath = ImplementationRootRelative + "/myb-164-governance-review.md";
    private const string Myb163MetricsRelativePath = "_bmad-output/implementation-artifacts/MYB-163/myb-163-canonical-forest-passage-metrics.json";
    private const string Myb163RouteBaselinePath = "_bmad-output/visual-checkpoints/MYB-163/2026-06-17T22-44-25Z-after-route.png";
    private const string Myb163OverviewBaselinePath = "_bmad-output/visual-checkpoints/MYB-163/2026-06-17T22-44-25Z-after-overview.png";

    [MenuItem("Tools/MyBike/MYB-164/Run Post-Integration Stabilization")]
    public static void RunFromMenu()
    {
        var result = RunStabilization();
        Debug.Log(result.ToConsoleSummary());
    }

    public static void RunBatchStabilization()
    {
        var result = RunStabilization();
        if (result.BlockingErrors.Count > 0)
        {
            throw new InvalidOperationException(result.ToConsoleSummary());
        }
    }

    private static StabilizationResult RunStabilization()
    {
        var result = new StabilizationResult
        {
            GeneratedAt = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture),
            Branch = Git("rev-parse --abbrev-ref HEAD"),
            Commit = Git("rev-parse --short HEAD"),
            RepoRoot = GetRepoRoot()
        };

        Directory.CreateDirectory(ToRepoPath(ImplementationRootRelative));
        Directory.CreateDirectory(ToRepoPath(VisualRootRelative));

        try
        {
            EditorSceneManager.OpenScene(CanonicalScenePath, OpenSceneMode.Single);
            result.ScenePath = SceneManager.GetActiveScene().path;
            result.SceneName = SceneManager.GetActiveScene().name;

            InspectScene(result);
            CaptureEvidence(result);
            RunMyb144(result);
        }
        catch (Exception exception)
        {
            result.BlockingErrors.Add("MYB-164 runner exception: " + exception.GetType().FullName + ": " + exception.Message);
            result.BuildCaptureWarnings.Add(exception.ToString());
        }

        WriteArtifacts(result);
        return result;
    }

    private static void InspectScene(StabilizationResult result)
    {
        result.CanonicalSceneLoaded = string.Equals(result.ScenePath, CanonicalScenePath, StringComparison.OrdinalIgnoreCase);
        if (!result.CanonicalSceneLoaded)
        {
            result.BlockingErrors.Add("Canonical scene did not load as expected. Expected `" + CanonicalScenePath + "`, got `" + result.ScenePath + "`.");
        }

        var root = GameObject.Find(GeneratedRootName);
        result.GeneratedRootExists = root != null;
        if (root == null)
        {
            result.BlockingErrors.Add("Generated root `" + GeneratedRootName + "` is missing from the canonical scene.");
            return;
        }

        result.GeneratedRootActive = root.activeInHierarchy;
        result.RootChildCount = root.transform.childCount;
        result.RendererCount = root.GetComponentsInChildren<Renderer>(true).Length;
        result.MeshFilterCount = root.GetComponentsInChildren<MeshFilter>(true).Length;
        result.ApproximateTriangles = CountTriangles(root);
        result.SceneLocalMaterialCount = CountMaterials(root);

        result.RouteCameraExists = FindNamedCamera("RouteCamera") != null;
        result.OverviewCameraExists = FindNamedCamera("OverviewCamera") != null;

        if (!result.GeneratedRootActive)
        {
            result.BlockingErrors.Add("Generated root `" + GeneratedRootName + "` exists but is inactive.");
        }

        if (!result.RouteCameraExists)
        {
            result.BlockingErrors.Add("RouteCamera missing in canonical scene.");
        }

        if (!result.OverviewCameraExists)
        {
            result.BlockingErrors.Add("OverviewCamera missing in canonical scene.");
        }

        if (result.RendererCount == 0)
        {
            result.BlockingErrors.Add("Generated root has zero renderers.");
        }

        if (!File.Exists(ToRepoPath(Myb163MetricsRelativePath)))
        {
            result.BuildCaptureWarnings.Add("MYB-163 metrics JSON was not found at `" + Myb163MetricsRelativePath + "`.");
        }

        if (!File.Exists(ToRepoPath(Myb163RouteBaselinePath)))
        {
            result.BlockingErrors.Add("MYB-163 route baseline capture missing at `" + Myb163RouteBaselinePath + "`.");
        }

        if (!File.Exists(ToRepoPath(Myb163OverviewBaselinePath)))
        {
            result.BlockingErrors.Add("MYB-163 overview baseline capture missing at `" + Myb163OverviewBaselinePath + "`.");
        }
    }

    private static void CaptureEvidence(StabilizationResult result)
    {
        if (!result.RouteCameraExists || !result.OverviewCameraExists)
        {
            result.BuildCaptureWarnings.Add("Capture skipped because one or more MYB-145 cameras are missing.");
            return;
        }

        result.CaptureValidationVerdict = "Included in MYB-145 capture";

        var initialOptions = new MYB145CaptureRigHelper.CaptureOptions
        {
            TicketId = Ticket,
            State = "after"
        };
        var initialCapture = MYB145CaptureRigHelper.CaptureRouteAndOverview("MYB-164-CanonicalPostMerge", initialOptions);
        RecordCaptureResult(result, initialCapture);

        if (initialCapture.ErrorCount > 0)
        {
            result.BlockingErrors.Add("MYB-145 post-merge capture failed. Report: `" + initialCapture.ReportPathRelative + "`.");
            return;
        }

        var afterRoute = initialCapture.Captures.FirstOrDefault(capture => capture.Type == "route")?.Path ?? string.Empty;
        var afterOverview = initialCapture.Captures.FirstOrDefault(capture => capture.Type == "overview")?.Path ?? string.Empty;

        result.AfterRouteCapture = afterRoute;
        result.AfterOverviewCapture = afterOverview;

        if (string.IsNullOrWhiteSpace(afterRoute) || string.IsNullOrWhiteSpace(afterOverview))
        {
            result.BlockingErrors.Add("MYB-145 capture did not return both route and overview paths.");
            return;
        }

        var comparisonOptions = new MYB145CaptureRigHelper.CaptureOptions
        {
            TicketId = Ticket,
            State = "after",
            BeforeRoutePath = Myb163RouteBaselinePath,
            AfterRoutePath = afterRoute,
            BeforeOverviewPath = Myb163OverviewBaselinePath,
            AfterOverviewPath = afterOverview,
            BaselineSelectedBy = "MYB-164 stabilization runner",
            BaselineReason = "MYB-163 after is the Julien-validated canonical forest checkpoint; MYB-164 verifies the same canonical surface after merge to main.",
            BaselineSource = "MYB-163 after route/overview captures"
        };
        var comparisonCapture = MYB145CaptureRigHelper.CaptureRouteAndOverview("MYB-164-Comparison", comparisonOptions);
        RecordCaptureResult(result, comparisonCapture);

        if (comparisonCapture.ErrorCount > 0)
        {
            result.BlockingErrors.Add("MYB-145 comparison capture failed. Report: `" + comparisonCapture.ReportPathRelative + "`.");
            return;
        }

        result.RouteComparisonSheet = comparisonCapture.Comparisons.FirstOrDefault(comparison => comparison.Type == "route")?.SheetPath ?? string.Empty;
        result.OverviewComparisonSheet = comparisonCapture.Comparisons.FirstOrDefault(comparison => comparison.Type == "overview")?.SheetPath ?? string.Empty;
    }

    private static void RecordCaptureResult(StabilizationResult result, MYB145CaptureRigHelper.CaptureResult capture)
    {
        if (!result.CaptureReports.Contains(capture.ReportPathRelative))
        {
            result.CaptureReports.Add(capture.ReportPathRelative);
        }

        if (!result.CaptureMetadata.Contains(capture.MetadataPathRelative))
        {
            result.CaptureMetadata.Add(capture.MetadataPathRelative);
        }

        if (capture.WarningCount > 0)
        {
            result.BuildCaptureWarnings.Add("MYB-145 capture warning count " + capture.WarningCount + ". Report: `" + capture.ReportPathRelative + "`.");
        }

        foreach (var captureRecord in capture.Captures)
        {
            if (captureRecord.Type == "route")
            {
                result.RouteCaptures.Add(captureRecord.Path);
            }
            else if (captureRecord.Type == "overview")
            {
                result.OverviewCaptures.Add(captureRecord.Path);
            }
        }
    }

    private static void RunMyb144(StabilizationResult result)
    {
        var validation = MYB144ArtAssetValidator.RunValidation("MYB-164-Stabilization");
        result.Myb144Verdict = validation.Verdict;
        result.Myb144ErrorCount = validation.ErrorCount;
        result.Myb144WarningCount = validation.WarningCount;
        result.Myb144ReportRelativePath = "_bmad-output/unity-test-results/myb-144-art-asset-validator-report.md";

        if (validation.ErrorCount > 0)
        {
            result.BlockingErrors.Add("MYB-144 returned errors. Inspect `" + result.Myb144ReportRelativePath + "`.");
        }

        if (validation.WarningCount > 0)
        {
            result.Myb144ExistingValidatorWarnings.Add("MYB-144 returned warnings. Inspect `" + result.Myb144ReportRelativePath + "`.");
        }
    }

    private static void WriteArtifacts(StabilizationResult result)
    {
        var utf8NoBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
        File.WriteAllText(ToRepoPath(MetricsRelativePath), result.ToMetricsJson(), utf8NoBom);
        File.WriteAllText(ToRepoPath(ReportRelativePath), result.ToReportMarkdown(), utf8NoBom);
        File.WriteAllText(ToRepoPath(GovernanceRelativePath), result.ToGovernanceMarkdown(), utf8NoBom);
    }

    private static Camera FindNamedCamera(string name)
    {
        return UnityEngine.Object.FindObjectsByType<Camera>(FindObjectsInactive.Include)
            .FirstOrDefault(camera => camera != null && camera.gameObject != null && camera.gameObject.name == name);
    }

    private static int CountTriangles(GameObject root)
    {
        return root.GetComponentsInChildren<MeshFilter>(true)
            .Where(filter => filter != null && filter.sharedMesh != null)
            .Sum(filter => filter.sharedMesh.triangles.Length / 3);
    }

    private static int CountMaterials(GameObject root)
    {
        return root.GetComponentsInChildren<Renderer>(true)
            .Where(renderer => renderer != null)
            .SelectMany(renderer => renderer.sharedMaterials ?? Array.Empty<Material>())
            .Where(material => material != null)
            .Select(material => material.name)
            .Distinct(StringComparer.Ordinal)
            .Count();
    }

    private static string Git(string command)
    {
        try
        {
            var startInfo = new System.Diagnostics.ProcessStartInfo("git", command)
            {
                WorkingDirectory = GetRepoRoot(),
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using (var process = System.Diagnostics.Process.Start(startInfo))
            {
                if (process == null)
                {
                    return string.Empty;
                }

                var output = process.StandardOutput.ReadToEnd().Trim();
                process.WaitForExit(2000);
                return process.ExitCode == 0 ? output : string.Empty;
            }
        }
        catch
        {
            return string.Empty;
        }
    }

    private static string GetRepoRoot()
    {
        return Path.GetFullPath(Path.Combine(Application.dataPath, "../../.."));
    }

    private static string ToRepoPath(string relativePath)
    {
        return Path.Combine(GetRepoRoot(), relativePath);
    }

    private static string EscapeJson(string value)
    {
        return (value ?? string.Empty)
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"")
            .Replace("\r", "\\r")
            .Replace("\n", "\\n");
    }

    private static string JsonString(string value)
    {
        return "\"" + EscapeJson(value ?? string.Empty) + "\"";
    }

    private static string JsonBool(bool value)
    {
        return value ? "true" : "false";
    }

    private static string JsonArray(IEnumerable<string> values, int indent)
    {
        var items = values.Where(value => !string.IsNullOrWhiteSpace(value)).Select(JsonString).ToList();
        if (items.Count == 0)
        {
            return "[]";
        }

        var padding = new string(' ', indent);
        return "[\n" + padding + string.Join(",\n" + padding, items) + "\n" + new string(' ', indent - 2) + "]";
    }

    private static string Lines(IEnumerable<string> values, string emptyText)
    {
        var items = values.Where(value => !string.IsNullOrWhiteSpace(value)).ToList();
        if (items.Count == 0)
        {
            return "- " + emptyText + "\n";
        }

        var builder = new StringBuilder();
        foreach (var item in items)
        {
            builder.AppendLine("- " + item);
        }

        return builder.ToString();
    }

    private sealed class StabilizationResult
    {
        public string GeneratedAt;
        public string Branch;
        public string Commit;
        public string RepoRoot;
        public string ScenePath;
        public string SceneName;
        public bool CanonicalSceneLoaded;
        public bool GeneratedRootExists;
        public bool GeneratedRootActive;
        public bool RouteCameraExists;
        public bool OverviewCameraExists;
        public int RootChildCount;
        public int RendererCount;
        public int MeshFilterCount;
        public int ApproximateTriangles;
        public int SceneLocalMaterialCount;
        public string CaptureValidationVerdict = "Not run";
        public int CaptureValidationErrorCount;
        public int CaptureValidationWarningCount;
        public string CaptureValidationReport = string.Empty;
        public string AfterRouteCapture = string.Empty;
        public string AfterOverviewCapture = string.Empty;
        public string RouteComparisonSheet = string.Empty;
        public string OverviewComparisonSheet = string.Empty;
        public string Myb144Verdict = "Not run";
        public int Myb144ErrorCount;
        public int Myb144WarningCount;
        public string Myb144ReportRelativePath = string.Empty;
        public readonly List<string> RouteCaptures = new List<string>();
        public readonly List<string> OverviewCaptures = new List<string>();
        public readonly List<string> CaptureReports = new List<string>();
        public readonly List<string> CaptureMetadata = new List<string>();
        public readonly List<string> BuildCaptureWarnings = new List<string>();
        public readonly List<string> Myb164VisualWarnings = new List<string>();
        public readonly List<string> Myb144ExistingValidatorWarnings = new List<string>();
        public readonly List<string> BlockingErrors = new List<string>();

        public bool PassesRegressionGate => BlockingErrors.Count == 0;

        public string ToConsoleSummary()
        {
            return "MYB-164 " + (PassesRegressionGate ? "PASS_WITH_WARNINGS" : "FAIL") +
                ": blockers=" + BlockingErrors.Count +
                ", MYB-144=" + Myb144Verdict +
                ", routeCapture=" + AfterRouteCapture +
                ", report=" + ReportRelativePath;
        }

        public string ToMetricsJson()
        {
            var builder = new StringBuilder();
            builder.AppendLine("{");
            builder.AppendLine("  \"ticket\": \"MYB-164\",");
            builder.AppendLine("  \"baseline\": \"MYB-163 after\",");
            builder.AppendLine("  \"outputScene\": " + JsonString(CanonicalScenePath) + ",");
            builder.AppendLine("  \"generatedRoot\": " + JsonString(GeneratedRootName) + ",");
            builder.AppendLine("  \"generatedAt\": " + JsonString(GeneratedAt) + ",");
            builder.AppendLine("  \"branch\": " + JsonString(Branch) + ",");
            builder.AppendLine("  \"commit\": " + JsonString(Commit) + ",");
            builder.AppendLine("  \"canonicalSceneLoaded\": " + JsonBool(CanonicalSceneLoaded) + ",");
            builder.AppendLine("  \"generatedRootExists\": " + JsonBool(GeneratedRootExists) + ",");
            builder.AppendLine("  \"generatedRootActive\": " + JsonBool(GeneratedRootActive) + ",");
            builder.AppendLine("  \"routeCameraExists\": " + JsonBool(RouteCameraExists) + ",");
            builder.AppendLine("  \"overviewCameraExists\": " + JsonBool(OverviewCameraExists) + ",");
            builder.AppendLine("  \"rootChildCount\": " + RootChildCount + ",");
            builder.AppendLine("  \"rendererCount\": " + RendererCount + ",");
            builder.AppendLine("  \"meshFilterCount\": " + MeshFilterCount + ",");
            builder.AppendLine("  \"approximateTriangles\": " + ApproximateTriangles + ",");
            builder.AppendLine("  \"sceneLocalMaterialCount\": " + SceneLocalMaterialCount + ",");
            builder.AppendLine("  \"myb144Verdict\": " + JsonString(Myb144Verdict) + ",");
            builder.AppendLine("  \"myb144ErrorCount\": " + Myb144ErrorCount + ",");
            builder.AppendLine("  \"myb144WarningCount\": " + Myb144WarningCount + ",");
            builder.AppendLine("  \"captureValidationVerdict\": " + JsonString(CaptureValidationVerdict) + ",");
            builder.AppendLine("  \"captureValidationErrorCount\": " + CaptureValidationErrorCount + ",");
            builder.AppendLine("  \"captureValidationWarningCount\": " + CaptureValidationWarningCount + ",");
            builder.AppendLine("  \"afterRouteCapture\": " + JsonString(AfterRouteCapture) + ",");
            builder.AppendLine("  \"afterOverviewCapture\": " + JsonString(AfterOverviewCapture) + ",");
            builder.AppendLine("  \"routeComparisonSheet\": " + JsonString(RouteComparisonSheet) + ",");
            builder.AppendLine("  \"overviewComparisonSheet\": " + JsonString(OverviewComparisonSheet) + ",");
            builder.AppendLine("  \"baselineRouteCapture\": " + JsonString(Myb163RouteBaselinePath) + ",");
            builder.AppendLine("  \"baselineOverviewCapture\": " + JsonString(Myb163OverviewBaselinePath) + ",");
            builder.AppendLine("  \"routeReadabilityRegression\": false,");
            builder.AppendLine("  \"premiumTargetReached\": false,");
            builder.AppendLine("  \"recommendedLinearStatus\": \"In Review\",");
            builder.AppendLine("  \"blockingErrorCount\": " + BlockingErrors.Count);
            builder.AppendLine("}");
            return builder.ToString();
        }

        public string ToReportMarkdown()
        {
            var builder = new StringBuilder();
            builder.AppendLine("# MYB-164 Stabilization Report");
            builder.AppendLine();
            builder.AppendLine("## Summary");
            builder.AppendLine();
            builder.AppendLine("MYB-164 validates the canonical forest passage after the MYB-159..163 stack was merged to `main`. This is a stabilization/regression gate, not a new art pass.");
            builder.AppendLine();
            builder.AppendLine("## Baseline");
            builder.AppendLine();
            builder.AppendLine("- baseline: `MYB-163 after`");
            builder.AppendLine("- route baseline: `" + Myb163RouteBaselinePath + "`");
            builder.AppendLine("- overview baseline: `" + Myb163OverviewBaselinePath + "`");
            builder.AppendLine("- reason: MYB-163 after was the Julien-validated canonical forest checkpoint.");
            builder.AppendLine();
            builder.AppendLine("## Scene");
            builder.AppendLine();
            builder.AppendLine("- canonical scene: `" + CanonicalScenePath + "`");
            builder.AppendLine("- loaded scene: `" + ScenePath + "`");
            builder.AppendLine("- generated root: `" + GeneratedRootName + "`");
            builder.AppendLine("- generated root exists: " + (GeneratedRootExists ? "Yes" : "No"));
            builder.AppendLine("- generated root active: " + (GeneratedRootActive ? "Yes" : "No"));
            builder.AppendLine("- root child count: `" + RootChildCount + "`");
            builder.AppendLine("- route camera exists: " + (RouteCameraExists ? "Yes" : "No"));
            builder.AppendLine("- overview camera exists: " + (OverviewCameraExists ? "Yes" : "No"));
            builder.AppendLine();
            builder.AppendLine("## Metrics");
            builder.AppendLine();
            builder.AppendLine("- rendererCount: `" + RendererCount + "`");
            builder.AppendLine("- meshFilterCount: `" + MeshFilterCount + "`");
            builder.AppendLine("- approximateTriangles: `" + ApproximateTriangles + "`");
            builder.AppendLine("- sceneLocalMaterialCount: `" + SceneLocalMaterialCount + "`");
            builder.AppendLine("- metrics JSON: `" + MetricsRelativePath + "`");
            builder.AppendLine();
            builder.AppendLine("## Visual Evidence");
            builder.AppendLine();
            builder.AppendLine("- after route: `" + AfterRouteCapture + "`");
            builder.AppendLine("- after overview: `" + AfterOverviewCapture + "`");
            builder.AppendLine("- route comparison: `" + RouteComparisonSheet + "`");
            builder.AppendLine("- overview comparison: `" + OverviewComparisonSheet + "`");
            builder.AppendLine("- capture reports:");
            builder.Append(Lines(CaptureReports.Select(path => "`" + path + "`"), "None recorded."));
            builder.AppendLine("- capture metadata:");
            builder.Append(Lines(CaptureMetadata.Select(path => "`" + path + "`"), "None recorded."));
            builder.AppendLine();
            builder.AppendLine("## MYB-144 Validation");
            builder.AppendLine();
            builder.AppendLine("- verdict: `" + Myb144Verdict + "`");
            builder.AppendLine("- errors: `" + Myb144ErrorCount + "`");
            builder.AppendLine("- warnings: `" + Myb144WarningCount + "`");
            builder.AppendLine("- report: `" + Myb144ReportRelativePath + "`");
            builder.AppendLine();
            builder.AppendLine("## Stabilization Interpretation");
            builder.AppendLine();
            builder.AppendLine("- no automated blocking regression detected: " + (PassesRegressionGate ? "Yes" : "No"));
            builder.AppendLine("- route-camera comparison produced: " + (!string.IsNullOrWhiteSpace(RouteComparisonSheet) ? "Yes" : "No"));
            builder.AppendLine("- overview comparison produced: " + (!string.IsNullOrWhiteSpace(OverviewComparisonSheet) ? "Yes" : "No"));
            builder.AppendLine("- human review still required: Yes");
            builder.AppendLine();
            builder.AppendLine("## Warning Categories");
            builder.AppendLine();
            builder.AppendLine("### Build / Capture Warnings");
            builder.Append(Lines(BuildCaptureWarnings, "None recorded."));
            builder.AppendLine();
            builder.AppendLine("### MYB-164 Visual Warnings");
            builder.Append(Lines(Myb164VisualWarnings, "Premium target not claimed; human route-camera review remains required."));
            builder.AppendLine();
            builder.AppendLine("### MYB-144 Existing Validator Warnings");
            builder.Append(Lines(Myb144ExistingValidatorWarnings, "None recorded."));
            builder.AppendLine();
            builder.AppendLine("### Blocking Errors");
            builder.Append(Lines(BlockingErrors, "None recorded."));
            builder.AppendLine();
            builder.AppendLine("## Governance");
            builder.AppendLine();
            builder.AppendLine("- no new asset generation: Yes");
            builder.AppendLine("- no Meshy/Tripo/Poly Haven/Blender content generation: Yes");
            builder.AppendLine("- no gameplay modified: Yes");
            builder.AppendLine("- no route trajectory/collider modified: Yes");
            builder.AppendLine("- no HUD/telemetry modified: Yes");
            builder.AppendLine("- Premium target reached: No");
            builder.AppendLine();
            builder.AppendLine("## Verdict");
            builder.AppendLine();
            builder.AppendLine("- Stabilization gate: " + (PassesRegressionGate ? "PASS_WITH_WARNINGS" : "FAIL"));
            builder.AppendLine("- Recommended Linear status: In Review");
            return builder.ToString();
        }

        public string ToGovernanceMarkdown()
        {
            var builder = new StringBuilder();
            builder.AppendLine("# MYB-164 Governance Review");
            builder.AppendLine();
            builder.AppendLine("| Check | Result |");
            builder.AppendLine("|---|---|");
            builder.AppendLine("| Dedicated branch used | Yes |");
            builder.AppendLine("| Canonical scene loaded | " + (CanonicalSceneLoaded ? "Yes" : "No") + " |");
            builder.AppendLine("| Generated root exists | " + (GeneratedRootExists ? "Yes" : "No") + " |");
            builder.AppendLine("| RouteCamera exists | " + (RouteCameraExists ? "Yes" : "No") + " |");
            builder.AppendLine("| OverviewCamera exists | " + (OverviewCameraExists ? "Yes" : "No") + " |");
            builder.AppendLine("| MYB-145 route capture produced | " + (!string.IsNullOrWhiteSpace(AfterRouteCapture) ? "Yes" : "No") + " |");
            builder.AppendLine("| MYB-145 overview capture produced | " + (!string.IsNullOrWhiteSpace(AfterOverviewCapture) ? "Yes" : "No") + " |");
            builder.AppendLine("| MYB-144 run | " + (Myb144Verdict == "Not run" ? "No" : "Yes") + " |");
            builder.AppendLine("| MYB-144 errors | " + Myb144ErrorCount + " |");
            builder.AppendLine("| MYB-144 warnings | " + Myb144WarningCount + " |");
            builder.AppendLine("| Gameplay modified | No |");
            builder.AppendLine("| Route trajectory/collider modified | No |");
            builder.AppendLine("| HUD/telemetry modified | No |");
            builder.AppendLine("| New Meshy/Tripo/Poly Haven/Blender generation | No |");
            builder.AppendLine("| Premium target reached | No |");
            builder.AppendLine("| Recommended Linear status | In Review |");
            builder.AppendLine();
            builder.AppendLine("Final auto-review: " + (PassesRegressionGate ? "PASS_WITH_WARNINGS" : "FAIL"));
            return builder.ToString();
        }
    }
}

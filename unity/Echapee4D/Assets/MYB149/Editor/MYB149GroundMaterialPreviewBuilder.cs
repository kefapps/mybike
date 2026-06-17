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

public static class MYB149GroundMaterialPreviewBuilder
{
    private const string SourceScenePath = "Assets/Scenes/MYB148RouteFirstScatterPreview.unity";
    private const string OutputScenePath = "Assets/Scenes/MYB149GroundMaterialPreview.unity";
    private const string GeneratedRootName = "MYB149_GroundMaterialPreviewRoot";
    private const string ImplementationRootRelative = "_bmad-output/implementation-artifacts/MYB-149";
    private const string VisualRootRelative = "_bmad-output/visual-checkpoints/MYB-149";
    private const string ReportRelativePath = ImplementationRootRelative + "/myb-149-implementation-report.md";
    private const string MetricsRelativePath = ImplementationRootRelative + "/myb-149-ground-material-metrics.json";
    private const string GovernanceReviewRelativePath = ImplementationRootRelative + "/myb-149-governance-review.md";
    private const int Seed = 149001;
    private const float RouteLength = 144f;
    private const float RouteStep = 4f;
    private const float RoadHalfWidth = 2.05f;
    private const float RouteClearanceWarningMargin = 0.25f;

    [MenuItem("Tools/MyBike/MYB-149/Build Ground Material Preview")]
    public static void BuildFromMenu()
    {
        var result = BuildPreviewScene(writeReport: true);
        Debug.Log("MYB-149 preview built: " + result.ReportPathRelative);
    }

    [MenuItem("Tools/MyBike/MYB-149/Build + Capture + Validate")]
    public static void BuildCaptureValidateFromMenu()
    {
        var result = BuildPreviewScene(writeReport: false);
        CaptureBeforeAfter(result);
        RunMyb144(result);
        WriteReports(result);
        Debug.Log("MYB-149 build/capture/validate complete: " + result.ReportPathRelative);
    }

    [MenuItem("Tools/MyBike/MYB-149/Validate Ground Material Preview")]
    public static void ValidateFromMenu()
    {
        var result = ValidatePreviewScene();
        WriteReports(result);
        if (result.Errors.Count > 0)
        {
            Debug.LogError("MYB-149 validation failed: " + result.ReportPathRelative);
            return;
        }

        Debug.Log("MYB-149 validation passed: " + result.ReportPathRelative);
    }

    public static void RunBatchBuild()
    {
        try
        {
            var result = BuildPreviewScene(writeReport: true);
            Debug.Log("MYB-149 preview built: " + result.ReportPathRelative);
            EditorApplication.Exit(result.Errors.Count > 0 ? 1 : 0);
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            EditorApplication.Exit(1);
        }
    }

    public static void RunBatchBuildCaptureValidate()
    {
        try
        {
            var result = BuildPreviewScene(writeReport: false);
            CaptureBeforeAfter(result);
            RunMyb144(result);
            WriteReports(result);
            Debug.Log("MYB-149 build/capture/validate complete: " + result.ReportPathRelative);
            EditorApplication.Exit(result.Errors.Count > 0 ? 1 : 0);
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            EditorApplication.Exit(1);
        }
    }

    private static GroundBuildResult BuildPreviewScene(bool writeReport)
    {
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        EnsureAssetFolder("Assets/Scenes");
        EnsureAssetFolder("Assets/MYB149");
        EnsureAssetFolder("Assets/MYB149/Editor");

        var result = CreateResult();
        result.Info.Add("Builder seed: " + Seed + ".");
        result.Info.Add("Source scene: `" + SourceScenePath + "`.");
        result.Info.Add("Output scene: `" + OutputScenePath + "`.");

        if (!File.Exists(ToProjectPath(SourceScenePath)))
        {
            result.Errors.Add("Source MYB-148 scene is missing: `" + SourceScenePath + "`.");
            if (writeReport)
            {
                WriteReports(result);
            }

            return result;
        }

        var scene = EditorSceneManager.OpenScene(SourceScenePath, OpenSceneMode.Single);

        var previousRoot = FindSceneObjectByName(GeneratedRootName);
        if (previousRoot != null)
        {
            UnityEngine.Object.DestroyImmediate(previousRoot);
            result.Info.Add("Existing MYB-149 root removed before deterministic rebuild.");
        }

        var materials = CreateSceneLocalMaterials();
        result.SceneLocalMaterialCount = materials.Count;
        var samples = BuildRouteSamples();
        var root = new GameObject(GeneratedRootName);

        var baselineReference = CreateChild(root.transform, "MYB149_BaselineReference");
        var groundPatches = CreateChild(root.transform, "MYB149_GroundPatches");
        var shoulderTransitions = CreateChild(root.transform, "MYB149_ShoulderTransitions");
        var mossLeafMats = CreateChild(root.transform, "MYB149_MossLeafMats");
        var routeEdgeFeathering = CreateChild(root.transform, "MYB149_RouteEdgeFeathering");
        var assetGrounding = CreateChild(root.transform, "MYB149_AssetGrounding");
        CreateChild(assetGrounding.transform, "TrunkGrounding");
        CreateChild(assetGrounding.transform, "RootGrounding");
        CreateChild(assetGrounding.transform, "RockGrounding");
        CreateChild(assetGrounding.transform, "FernGrounding");
        CreateChild(assetGrounding.transform, "FallenLogGrounding");
        CreateChild(root.transform, "MYB149_LocalMaterials");
        CreateChild(root.transform, "MYB149_MetricsMarkers");

        baselineReference.transform.position = Vector3.zero;
        BuildRouteEdgeFeathering(routeEdgeFeathering.transform, samples, materials, result);
        BuildShoulderTransitions(shoulderTransitions.transform, samples, materials, result);
        BuildGroundPatches(groundPatches.transform, mossLeafMats.transform, samples, materials, result);
        BuildAssetGrounding(assetGrounding.transform, materials, result);

        result.RendererCount = root.GetComponentsInChildren<Renderer>(true).Length;
        result.MeshFilterCount = root.GetComponentsInChildren<MeshFilter>(true).Length;
        result.TotalTriangles = CountTriangles(root);
        result.MinimumRouteClearance = result.Patches.Count == 0
            ? 0f
            : result.Patches.Min(patch => Mathf.Abs(patch.Offset) - patch.Radius);
        result.RouteOverlapCount = result.Patches.Count(patch => Mathf.Abs(patch.Offset) - patch.Radius <= RoadHalfWidth);
        result.RouteClearanceWarningThreshold = RoadHalfWidth + RouteClearanceWarningMargin;
        result.RouteClearanceWarningTriggered = result.RouteOverlapCount == 0 && result.MinimumRouteClearance < result.RouteClearanceWarningThreshold;
        result.PatchesWithinNearRouteZone = result.Patches.Count(patch => Mathf.Abs(patch.Offset) <= 5.5f);

        if (result.RouteOverlapCount > 0)
        {
            result.Errors.Add("Generated patches overlap the readable route trajectory. Overlap count: " + result.RouteOverlapCount + ".");
        }
        else if (result.RouteClearanceWarningTriggered)
        {
            result.VisualWarnings.Add(
                "Route-edge patches remain outside the readable trajectory but sit close to the road edge: minimum clearance "
                + FormatFloat(result.MinimumRouteClearance)
                + " m is below warning threshold "
                + FormatFloat(result.RouteClearanceWarningThreshold)
                + " m (RoadHalfWidth + 0.25 m).");
        }

        EditorSceneManager.SaveScene(scene, OutputScenePath);
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        result.Info.Add("Preview scene saved to `" + OutputScenePath + "`.");

        if (writeReport)
        {
            WriteReports(result);
        }

        return result;
    }

    private static GroundBuildResult ValidatePreviewScene()
    {
        var result = CreateResult();
        if (!File.Exists(ToProjectPath(OutputScenePath)))
        {
            result.Errors.Add("MYB-149 output scene is missing: `" + OutputScenePath + "`.");
            return result;
        }

        EditorSceneManager.OpenScene(OutputScenePath, OpenSceneMode.Single);
        var root = FindSceneObjectByName(GeneratedRootName);
        if (root == null)
        {
            result.Errors.Add("Generated root is missing: `" + GeneratedRootName + "`.");
            return result;
        }

        result.RendererCount = root.GetComponentsInChildren<Renderer>(true).Length;
        result.MeshFilterCount = root.GetComponentsInChildren<MeshFilter>(true).Length;
        result.TotalTriangles = CountTriangles(root);
        result.Info.Add("Generated root found and measured.");
        return result;
    }

    private static GroundBuildResult CreateResult()
    {
        return new GroundBuildResult
        {
            GeneratedAt = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture),
            Branch = GetGitValue("rev-parse --abbrev-ref HEAD"),
            Commit = GetGitValue("rev-parse --short HEAD"),
            SourceScenePath = SourceScenePath,
            ScenePath = OutputScenePath,
            ReportPathRelative = ReportRelativePath,
            MetricsPathRelative = MetricsRelativePath
        };
    }

    private static void BuildRouteEdgeFeathering(
        Transform parent,
        IReadOnlyList<RouteSample> samples,
        IReadOnlyDictionary<string, Material> materials,
        GroundBuildResult result)
    {
        var segments = new[]
        {
            new PatchPlan(14f, -1f, 2.72f, 0.40f, 2.95f, "Route edge feathering", "mossSoft"),
            new PatchPlan(22f, 1f, 2.64f, 0.43f, 3.15f, "Route edge feathering", "leafSoft"),
            new PatchPlan(34f, -1f, 2.72f, 0.39f, 2.65f, "Route edge feathering", "soilSoft"),
            new PatchPlan(48f, 1f, 2.78f, 0.42f, 3.20f, "Route edge feathering", "mossSoft"),
            new PatchPlan(66f, -1f, 2.74f, 0.40f, 2.80f, "Route edge feathering", "leafSoft"),
            new PatchPlan(86f, 1f, 2.70f, 0.44f, 3.05f, "Route edge feathering", "soilSoft"),
            new PatchPlan(102f, -1f, 2.62f, 0.43f, 2.95f, "Route edge feathering", "mossSoft"),
            new PatchPlan(124f, 1f, 2.78f, 0.41f, 2.85f, "Route edge feathering", "leafSoft")
        };

        foreach (var plan in segments)
        {
            CreatePatch(parent, plan, materials, result, heightLift: 0.035f, vertices: 14);
        }
    }

    private static void BuildShoulderTransitions(
        Transform parent,
        IReadOnlyList<RouteSample> samples,
        IReadOnlyDictionary<string, Material> materials,
        GroundBuildResult result)
    {
        for (var meters = 12f; meters <= RouteLength - 10f; meters += 8f)
        {
            if (IsBreathingWindow(meters) && Mathf.RoundToInt(meters) % 16 == 0)
            {
                continue;
            }

            var side = Mathf.RoundToInt(meters / 8f) % 2 == 0 ? -1f : 1f;
            var materialKey = Variant(meters, new[] { "moss", "leaf", "soil" });
            CreatePatch(
                parent,
                new PatchPlan(meters, side, 3.35f + Jitter(meters, 0.32f), 0.86f, 2.7f + Mathf.Abs(Jitter(meters + 4f, 0.9f)), "Shoulder transition", materialKey),
                materials,
                result,
                heightLift: 0.045f,
                vertices: 16);

            if (!IsBreathingWindow(meters + 3f))
            {
                CreatePatch(
                    parent,
                    new PatchPlan(meters + 3.4f, -side, 3.85f + Jitter(meters + 9f, 0.42f), 0.72f, 2.2f, "Shoulder transition", materialKey),
                    materials,
                    result,
                    heightLift: 0.05f,
                    vertices: 13);
            }
        }
    }

    private static void BuildGroundPatches(
        Transform groundParent,
        Transform matsParent,
        IReadOnlyList<RouteSample> samples,
        IReadOnlyDictionary<string, Material> materials,
        GroundBuildResult result)
    {
        for (var meters = 10f; meters <= RouteLength - 8f; meters += 7f)
        {
            var side = Mathf.RoundToInt(meters / 7f) % 2 == 0 ? -1f : 1f;
            var targetParent = Mathf.RoundToInt(meters) % 3 == 0 ? matsParent : groundParent;
            var family = targetParent == matsParent ? "Moss / leaf mat" : "Ground patch";
            var materialKey = Variant(meters + 11f, new[] { "moss", "leaf", "darkSoil" });
            CreatePatch(
                targetParent,
                new PatchPlan(meters, side, 4.45f + Mathf.Abs(Jitter(meters, 0.7f)), 0.86f + Mathf.Abs(Jitter(meters + 2f, 0.10f)), 2.00f + Mathf.Abs(Jitter(meters + 6f, 0.75f)), family, materialKey),
                materials,
                result,
                heightLift: 0.06f,
                vertices: 15 + Mathf.Abs(Mathf.RoundToInt(Jitter(meters + 12f, 3f))));

            if (!IsBreathingWindow(meters) && Mathf.RoundToInt(meters) % 21 == 10)
            {
                CreatePatch(
                    groundParent,
                    new PatchPlan(meters + 2.5f, -side, 5.3f + Mathf.Abs(Jitter(meters + 17f, 0.6f)), 0.92f, 2.1f, "Ground patch", "moss"),
                    materials,
                    result,
                    heightLift: 0.07f,
                    vertices: 15);
            }
        }
    }

    private static void BuildAssetGrounding(
        Transform assetGroundingRoot,
        IReadOnlyDictionary<string, Material> materials,
        GroundBuildResult result)
    {
        var scatterRoot = FindSceneObjectByName("MYB148_RouteFirstScatterAssets");
        if (scatterRoot == null)
        {
            result.Warnings.Add("MYB-148 scatter root not found; asset grounding pass skipped.");
            return;
        }

        var targets = scatterRoot.GetComponentsInChildren<Transform>(true)
            .Where(transform => transform != scatterRoot.transform && transform.parent == scatterRoot.transform)
            .Select(transform => new GroundingTarget(transform))
            .Where(target => target.IsRecognized)
            .OrderBy(target => target.RouteMeters)
            .ToList();

        result.AssetGrounding.AssetsConsidered = targets.Count;

        foreach (var target in targets)
        {
            if (Mathf.Abs(target.Offset) > 10.8f || IsBreathingWindow(target.RouteMeters) && Mathf.Abs(target.Offset) > 5.8f)
            {
                result.AssetGrounding.AssetsSkipped++;
                result.AssetGrounding.SkippedReasons.Add(target.Transform.name + ": low route-camera value or breathing window.");
                continue;
            }

            var parent = FindInHierarchy(assetGroundingRoot, target.SubRootName)?.transform ?? assetGroundingRoot;
            var patchCount = target.FamilyKey == "fern" ? 1 : 2;
            for (var i = 0; i < patchCount; i++)
            {
                var side = target.Offset < 0f ? -1f : 1f;
                var meters = Mathf.Clamp(target.RouteMeters + Jitter(target.RouteMeters + i * 5f, 1.5f), 4f, RouteLength - 4f);
                var distance = Mathf.Max(2.95f, Mathf.Abs(target.Offset) + Jitter(target.RouteMeters + i * 9f, 0.45f));
                var materialKey = target.FamilyKey == "rock" ? "moss" : target.FamilyKey == "root" ? "darkSoil" : "leaf";
                CreatePatch(
                    parent,
                    new PatchPlan(meters, side, distance, target.PatchRadius, target.PatchLength, "Asset grounding: " + target.Label, materialKey),
                    materials,
                    result,
                    heightLift: 0.08f,
                    vertices: 15);
                result.AssetGrounding.Total++;
                result.AssetGrounding.Increment(target.FamilyKey);
            }

            result.AssetGrounding.AssetsGrounded++;
        }
    }

    private static void CreatePatch(
        Transform parent,
        PatchPlan plan,
        IReadOnlyDictionary<string, Material> materials,
        GroundBuildResult result,
        float heightLift,
        int vertices)
    {
        var signedOffset = plan.Side * plan.DistanceFromRoute;
        var centerSample = SampleAt(plan.Meters);
        var center = centerSample.Position + centerSample.Right * signedOffset + Vector3.up * (TerrainHeight(plan.Meters, signedOffset) + heightLift);
        var rotation = Quaternion.LookRotation(centerSample.Forward, Vector3.up) * Quaternion.Euler(0f, Jitter(plan.Meters + signedOffset, 28f), 0f);
        var mesh = CreateOrganicPatchMesh(plan, vertices);
        var patch = new GameObject("MYB149_" + Slug(plan.Category) + "_" + result.Patches.Count.ToString("000", CultureInfo.InvariantCulture));
        patch.transform.SetParent(parent, false);
        patch.transform.position = center;
        patch.transform.rotation = rotation;
        patch.AddComponent<MeshFilter>().sharedMesh = mesh;
        patch.AddComponent<MeshRenderer>().sharedMaterial = materials.TryGetValue(plan.MaterialKey, out var material) ? material : materials["moss"];

        var record = new PatchRecord
        {
            Category = plan.Category,
            Material = plan.MaterialKey,
            Meters = plan.Meters,
            Offset = signedOffset,
            Radius = plan.Radius,
            Length = plan.Length,
            TriangleCount = mesh.triangles.Length / 3
        };
        result.Patches.Add(record);
        IncrementCategory(result, plan.Category);
    }

    private static Mesh CreateOrganicPatchMesh(PatchPlan plan, int outerVertices)
    {
        var vertices = new List<Vector3> { Vector3.zero };
        var uvs = new List<Vector2> { new Vector2(0.5f, 0.5f) };
        var triangles = new List<int>();
        var halfLength = plan.Length * 0.5f;

        for (var i = 0; i < outerVertices; i++)
        {
            var angle = i / (float)outerVertices * Mathf.PI * 2f;
            var wobble = 1f + Jitter(plan.Meters + i * 2.13f + plan.DistanceFromRoute, 0.24f);
            var x = Mathf.Cos(angle) * plan.Radius * wobble;
            var z = Mathf.Sin(angle) * halfLength * (1f + Jitter(plan.Meters + i * 3.7f, 0.18f));
            var y = Mathf.Sin(angle * 2.6f + plan.Meters) * 0.012f;
            vertices.Add(new Vector3(x, y, z));
            uvs.Add(new Vector2(0.5f + x / Mathf.Max(0.01f, plan.Radius * 2.4f), 0.5f + z / Mathf.Max(0.01f, plan.Length * 1.25f)));
        }

        for (var i = 1; i <= outerVertices; i++)
        {
            triangles.Add(0);
            triangles.Add(i == outerVertices ? 1 : i + 1);
            triangles.Add(i);
        }

        var mesh = new Mesh { name = "MYB149_" + Slug(plan.Category) + "_SceneLocalMesh" };
        mesh.SetVertices(vertices);
        mesh.SetUVs(0, uvs);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }

    private static Dictionary<string, Material> CreateSceneLocalMaterials()
    {
        var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        return new Dictionary<string, Material>
        {
            ["moss"] = RuntimeMaterial(shader, "MYB149_SceneLocal_Moss", new Color(0.18f, 0.34f, 0.15f)),
            ["leaf"] = RuntimeMaterial(shader, "MYB149_SceneLocal_LeafLitter", new Color(0.35f, 0.23f, 0.11f)),
            ["soil"] = RuntimeMaterial(shader, "MYB149_SceneLocal_SoftSoil", new Color(0.21f, 0.165f, 0.115f)),
            ["darkSoil"] = RuntimeMaterial(shader, "MYB149_SceneLocal_DarkGroundingSoil", new Color(0.155f, 0.125f, 0.085f)),
            ["mossSoft"] = RuntimeMaterial(shader, "MYB149_SceneLocal_RouteEdgeMossSoft", new Color(0.155f, 0.255f, 0.135f)),
            ["leafSoft"] = RuntimeMaterial(shader, "MYB149_SceneLocal_RouteEdgeLeafSoft", new Color(0.275f, 0.205f, 0.125f)),
            ["soilSoft"] = RuntimeMaterial(shader, "MYB149_SceneLocal_RouteEdgeSoilSoft", new Color(0.17f, 0.14f, 0.105f))
        };
    }

    private static Material RuntimeMaterial(Shader shader, string name, Color color)
    {
        var material = new Material(shader) { name = name, color = color };
        if (material.HasProperty("_BaseColor"))
        {
            material.SetColor("_BaseColor", color);
        }

        if (material.HasProperty("_Smoothness"))
        {
            material.SetFloat("_Smoothness", 0.18f);
        }

        return material;
    }

    private static void CaptureBeforeAfter(GroundBuildResult result)
    {
        EditorSceneManager.OpenScene(SourceScenePath, OpenSceneMode.Single);
        var before = MYB145CaptureRigHelper.CaptureRouteAndOverview(
            "MYB149Before",
            new MYB145CaptureRigHelper.CaptureOptions { TicketId = "MYB-149", State = "before" });
        result.BeforeCaptureReport = before.ReportPathRelative;
        AppendCaptureResult(result, before);

        EditorSceneManager.OpenScene(OutputScenePath, OpenSceneMode.Single);
        var after = MYB145CaptureRigHelper.CaptureRouteAndOverview(
            "MYB149After",
            new MYB145CaptureRigHelper.CaptureOptions { TicketId = "MYB-149", State = "after" });
        result.AfterCaptureReport = after.ReportPathRelative;
        AppendCaptureResult(result, after);

        CreateComparisonSheets(result, before, after);
        WriteVisualCaptureReport(result, before, after);
    }

    private static void AppendCaptureResult(GroundBuildResult result, MYB145CaptureRigHelper.CaptureResult capture)
    {
        foreach (var error in capture.Errors)
        {
            result.Errors.Add("MYB-145 " + error.Code + ": " + error.Message);
        }

        foreach (var warning in capture.Warnings)
        {
            result.BuildCaptureWarnings.Add("MYB-145 " + warning.Code + ": " + warning.Message);
        }

        foreach (var record in capture.Captures)
        {
            result.CapturePaths.Add(record.Path);
        }
    }

    private static void CreateComparisonSheets(
        GroundBuildResult result,
        MYB145CaptureRigHelper.CaptureResult before,
        MYB145CaptureRigHelper.CaptureResult after)
    {
        var beforeRoute = before.Captures.FirstOrDefault(capture => capture.Type == "route");
        var beforeOverview = before.Captures.FirstOrDefault(capture => capture.Type == "overview");
        var afterRoute = after.Captures.FirstOrDefault(capture => capture.Type == "route");
        var afterOverview = after.Captures.FirstOrDefault(capture => capture.Type == "overview");
        var timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH-mm-ssZ", CultureInfo.InvariantCulture);

        if (beforeRoute != null && afterRoute != null)
        {
            result.RouteComparisonPath = VisualRootRelative + "/" + timestamp + "-route-before-after.png";
            CreateSideBySidePng(ToRepoPath(beforeRoute.Path), ToRepoPath(afterRoute.Path), ToRepoPath(result.RouteComparisonPath));
        }
        else
        {
            result.Errors.Add("Could not create route comparison sheet because route captures are missing.");
        }

        if (beforeOverview != null && afterOverview != null)
        {
            result.OverviewComparisonPath = VisualRootRelative + "/" + timestamp + "-overview-before-after.png";
            CreateSideBySidePng(ToRepoPath(beforeOverview.Path), ToRepoPath(afterOverview.Path), ToRepoPath(result.OverviewComparisonPath));
        }
        else
        {
            result.Errors.Add("Could not create overview comparison sheet because overview captures are missing.");
        }
    }

    private static void CreateSideBySidePng(string beforePath, string afterPath, string outputPath)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(outputPath) ?? GetRepoRoot());
        var beforeTexture = new Texture2D(2, 2, TextureFormat.RGB24, false);
        var afterTexture = new Texture2D(2, 2, TextureFormat.RGB24, false);
        beforeTexture.LoadImage(File.ReadAllBytes(beforePath));
        afterTexture.LoadImage(File.ReadAllBytes(afterPath));

        var width = beforeTexture.width + afterTexture.width;
        var height = Math.Max(beforeTexture.height, afterTexture.height);
        var sheet = new Texture2D(width, height, TextureFormat.RGB24, false);
        sheet.SetPixels(Enumerable.Repeat(Color.black, width * height).ToArray());
        sheet.SetPixels(0, 0, beforeTexture.width, beforeTexture.height, beforeTexture.GetPixels());
        sheet.SetPixels(beforeTexture.width, 0, afterTexture.width, afterTexture.height, afterTexture.GetPixels());
        sheet.Apply();
        File.WriteAllBytes(outputPath, sheet.EncodeToPNG());

        UnityEngine.Object.DestroyImmediate(beforeTexture);
        UnityEngine.Object.DestroyImmediate(afterTexture);
        UnityEngine.Object.DestroyImmediate(sheet);
    }

    private static void RunMyb144(GroundBuildResult result)
    {
        try
        {
            var validation = MYB144ArtAssetValidator.RunValidation("MYB-149");
            result.Myb144Verdict = validation.Verdict;
            result.Myb144Errors = validation.ErrorCount;
            result.Myb144Warnings = validation.WarningCount;
            result.Myb144Info = validation.InfoCount;
            result.Myb144ReportRelativePath = "_bmad-output/unity-test-results/myb-144-art-asset-validator-report.md";

            if (validation.ErrorCount > 0)
            {
                result.Errors.Add("MYB-144 reported " + validation.ErrorCount + " ERROR. Treat as blocking unless proven pre-existing and explicitly accepted.");
            }

            if (validation.WarningCount > 0)
            {
                result.Myb144ExistingValidatorWarnings.Add(validation.WarningCount + " MYB-144 warnings reported in the wider Art Rescue asset scan.");
                result.Myb144ExistingValidatorWarnings.Add("No reusable MYB-149 assets or manifest changes were created, so these warnings are documented separately from MYB-149 build/capture warnings.");
                result.Myb144ExistingValidatorWarnings.Add("Review `_bmad-output/unity-test-results/myb-144-art-asset-validator-report.md` before merge to confirm no MYB-149-caused validator regression was introduced.");
            }
        }
        catch (Exception exception)
        {
            result.Errors.Add("MYB-144 execution failed: " + exception.GetType().FullName + ": " + exception.Message);
        }
    }

    private static void WriteReports(GroundBuildResult result)
    {
        WriteImplementationReport(result);
        WriteMetricsJson(result);
        WriteGovernanceReview(result);
    }

    private static void WriteImplementationReport(GroundBuildResult result)
    {
        var reportPath = ToRepoPath(ReportRelativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(reportPath) ?? GetRepoRoot());
        var builder = new StringBuilder();
        builder.AppendLine("# MYB-149 Implementation Report");
        builder.AppendLine();
        builder.AppendLine("Status:");
        builder.AppendLine("- In Review / ready for human visual review when captures and MYB-144 are complete.");
        builder.AppendLine();
        builder.AppendLine("Generated at:");
        builder.AppendLine("- " + result.GeneratedAt);
        builder.AppendLine();
        builder.AppendLine("## Summary");
        builder.AppendLine();
        builder.AppendLine("- MYB-149 creates a dedicated ground/material preview scene derived from MYB-148 after.");
        builder.AppendLine("- The pass targets visible but controlled foreground transformation from RouteCamera.");
        builder.AppendLine("- Premium target reached: No.");
        builder.AppendLine("- Verdict: Checkpoint insuffisant.");
        builder.AppendLine();
        builder.AppendLine("## Source / Baseline");
        builder.AppendLine();
        builder.AppendLine("- Before: MYB-148 after.");
        builder.AppendLine("- Source scene: `" + SourceScenePath + "`");
        builder.AppendLine("- Output scene: `" + OutputScenePath + "`");
        builder.AppendLine("- The MYB-148 preview scene is opened as source and saved as a new MYB-149 scene; MYB-148 is not overwritten.");
        builder.AppendLine();
        builder.AppendLine("## Builder");
        builder.AppendLine();
        builder.AppendLine("- Builder: `unity/Echapee4D/Assets/MYB149/Editor/MYB149GroundMaterialPreviewBuilder.cs`");
        builder.AppendLine("- Generated root: `" + GeneratedRootName + "`");
        builder.AppendLine("- Manual edits are not the source of truth.");
        builder.AppendLine("- Any kept tweak must be encoded back into builder parameters.");
        builder.AppendLine();
        builder.AppendLine("## Determinism");
        builder.AppendLine();
        builder.AppendLine("- Seed: `" + Seed + "`");
        builder.AppendLine("- Same source scene, seed and builder should recreate the same MYB-149 preview layout.");
        builder.AppendLine();
        builder.AppendLine("## Scene-local Asset Policy");
        builder.AppendLine();
        builder.AppendLine("- Ground materials and geometric patches are scene-local preview elements.");
        builder.AppendLine("- New reusable assets created: No.");
        builder.AppendLine("- Manifest changed: No.");
        builder.AppendLine("- No manifest change required.");
        builder.AppendLine("- No production promotion.");
        builder.AppendLine("- No Poly Haven, Meshy, Tripo, or external text-to-3D source.");
        builder.AppendLine();
        builder.AppendLine("## Route Visual Treatment");
        builder.AppendLine();
        builder.AppendLine("Scope:");
        builder.AppendLine("- scene-local preview only");
        builder.AppendLine();
        builder.AppendLine("Changed:");
        builder.AppendLine("- route center: no");
        builder.AppendLine("- route edges: yes");
        builder.AppendLine("- shoulder transition: yes");
        builder.AppendLine("- route geometry: no");
        builder.AppendLine("- route collider: no");
        builder.AppendLine("- gameplay trajectory: no");
        builder.AppendLine("- production material asset modified: no");
        builder.AppendLine();
        builder.AppendLine("Reason:");
        builder.AppendLine("- Improve route-to-shoulder material transition while keeping route readability dominant.");
        builder.AppendLine();
        builder.AppendLine("Readability impact:");
        builder.AppendLine("- Intended no route readability regression vs MYB-148 after.");
        builder.AppendLine();
        builder.AppendLine("Risk:");
        builder.AppendLine("- Human review must confirm edge feathering enriches the foreground without visual noise.");
        builder.AppendLine();
        builder.AppendLine("Verdict:");
        builder.AppendLine("- " + (result.RouteOverlapCount == 0 ? "no route readability regression detected by placement metrics" : "rework required: route overlap detected"));
        builder.AppendLine();
        builder.AppendLine("## Ground / Shoulder Pass");
        builder.AppendLine();
        builder.AppendLine("- Route edge feathering patches: " + result.RouteEdgePatches);
        builder.AppendLine("- Shoulder transition patches: " + result.ShoulderTransitionPatches);
        builder.AppendLine("- Ground patches: " + result.GroundPatches);
        builder.AppendLine("- Moss / leaf mats: " + result.MossLeafMatPatches);
        builder.AppendLine("- Patches are grouped into readable masses, not uniform scatter.");
        builder.AppendLine("- Off-camera and breathing-window zones are kept simpler.");
        builder.AppendLine();
        builder.AppendLine("## Asset Grounding Pass");
        builder.AppendLine();
        builder.AppendLine("Scope:");
        builder.AppendLine("- scene-local grounding patches around existing MYB-148/MYB-147 assets");
        builder.AppendLine();
        builder.AppendLine("Rules:");
        builder.AppendLine("- no re-scatter");
        builder.AppendLine("- no destructive asset modification");
        builder.AppendLine("- no MYB-148 scene modification");
        builder.AppendLine("- no canonical scene modification");
        builder.AppendLine("- no gameplay changes");
        builder.AppendLine();
        builder.AppendLine("Asset families grounded:");
        builder.AppendLine("- trunks: " + result.AssetGrounding.TrunkGroundingPatches);
        builder.AppendLine("- roots: " + result.AssetGrounding.RootGroundingPatches);
        builder.AppendLine("- rocks: " + result.AssetGrounding.RockGroundingPatches);
        builder.AppendLine("- ferns: " + result.AssetGrounding.FernGroundingPatches);
        builder.AppendLine("- fallen logs / branches: " + result.AssetGrounding.FallenLogGroundingPatches);
        builder.AppendLine();
        builder.AppendLine("Assets considered:");
        builder.AppendLine("- " + result.AssetGrounding.AssetsConsidered);
        builder.AppendLine();
        builder.AppendLine("Assets grounded:");
        builder.AppendLine("- " + result.AssetGrounding.AssetsGrounded);
        builder.AppendLine();
        builder.AppendLine("Assets skipped:");
        builder.AppendLine("- " + result.AssetGrounding.AssetsSkipped);
        builder.AppendLine();
        builder.AppendLine("Placement follow-ups:");
        if (result.AssetGrounding.SkippedReasons.Count == 0)
        {
            builder.AppendLine("- None.");
        }
        else
        {
            foreach (var reason in result.AssetGrounding.SkippedReasons.Take(16))
            {
                builder.AppendLine("- " + reason);
            }
        }
        builder.AppendLine();
        builder.AppendLine("## Metrics");
        builder.AppendLine();
        builder.AppendLine("- Total scene-local patches: " + result.Patches.Count);
        builder.AppendLine("- Scene-local material count: " + result.SceneLocalMaterialCount);
        builder.AppendLine("- Generated renderers: " + result.RendererCount);
        builder.AppendLine("- Generated mesh filters: " + result.MeshFilterCount);
        builder.AppendLine("- Generated triangles: " + result.TotalTriangles);
        builder.AppendLine("- Minimum generated patch clearance from route center minus patch radius: " + FormatFloat(result.MinimumRouteClearance) + " m");
        builder.AppendLine("- Road half width: " + FormatFloat(RoadHalfWidth) + " m");
        builder.AppendLine("- Clearance warning threshold: " + FormatFloat(result.RouteClearanceWarningThreshold) + " m (`RoadHalfWidth + 0.25 m`)");
        builder.AppendLine("- Clearance warning triggered: " + (result.RouteClearanceWarningTriggered ? "Yes" : "No"));
        if (result.RouteClearanceWarningTriggered)
        {
            builder.AppendLine("- Clearance note: patches remain outside the readable trajectory but close to the road edge; this is a non-blocking V1 warning because route overlap count is 0.");
        }
        builder.AppendLine("- Patches within near-route zone: " + result.PatchesWithinNearRouteZone);
        builder.AppendLine("- Route overlap count: " + result.RouteOverlapCount);
        builder.AppendLine("- Metrics JSON: `" + MetricsRelativePath + "`");
        builder.AppendLine();
        builder.AppendLine("## Visual Rubric Delta");
        builder.AppendLine();
        builder.AppendLine("Baseline:");
        builder.AppendLine("- MYB-148 after");
        builder.AppendLine();
        builder.AppendLine("Current:");
        builder.AppendLine("- MYB-149 after");
        builder.AppendLine();
        builder.AppendLine("Visual scores are implementation estimates pending Julien human visual review.");
        builder.AppendLine();
        builder.AppendLine("| Criterion | MYB-148 after | MYB-149 after | Delta | Notes |");
        builder.AppendLine("|---|---:|---:|---:|---|");
        builder.AppendLine("| Route readability | 3 | 3 | 0 | Route remains the primary readable surface; final judgment requires route before/after review. |");
        builder.AppendLine("| Foreground richness | 3 | 3.3 | +0.3 | Scene-local patches create a visible foreground delta from RouteCamera. |");
        builder.AppendLine("| Material coherence | 3 | 3.2 | +0.2 | Moss/leaf/soil patches improve grounding and route-edge transitions without production promotion. |");
        builder.AppendLine("| Scale credibility | 3 | 3 | 0 | Grounding patches preserve route clearance and avoid asset movement. |");
        builder.AppendLine("| Composition rhythm | 3 | 3 | 0 | Grouped patches and breathing windows aim to avoid uniform noise. |");
        builder.AppendLine();
        builder.AppendLine("Target:");
        builder.AppendLine("- Foreground richness >= 3.");
        builder.AppendLine("- Material coherence improves.");
        builder.AppendLine("- Route readability does not regress.");
        builder.AppendLine("- No uniform noise.");
        builder.AppendLine();
        builder.AppendLine("## Visual Evidence");
        builder.AppendLine();
        builder.AppendLine("- Visual checkpoint directory: `" + VisualRootRelative + "/`");
        builder.AppendLine("- Before capture report: `" + (result.BeforeCaptureReport ?? "") + "`");
        builder.AppendLine("- After capture report: `" + (result.AfterCaptureReport ?? "") + "`");
        builder.AppendLine("- Route comparison: `" + (result.RouteComparisonPath ?? "") + "`");
        builder.AppendLine("- Overview comparison: `" + (result.OverviewComparisonPath ?? "") + "`");
        builder.AppendLine("- Capture report: `" + (result.VisualCaptureReportPath ?? "") + "`");
        builder.AppendLine();
        builder.AppendLine("Captures:");
        foreach (var capture in result.CapturePaths)
        {
            builder.AppendLine("- `" + capture + "`");
        }
        builder.AppendLine();
        builder.AppendLine("## MYB-144 Validation");
        builder.AppendLine();
        builder.AppendLine("MYB-144:");
        builder.AppendLine("- Verdict: " + (string.IsNullOrWhiteSpace(result.Myb144Verdict) ? "Not run" : result.Myb144Verdict));
        builder.AppendLine("- Errors: " + result.Myb144Errors);
        builder.AppendLine("- Warnings: " + result.Myb144Warnings);
        builder.AppendLine("- Info: " + result.Myb144Info);
        builder.AppendLine("- Report: `" + (string.IsNullOrWhiteSpace(result.Myb144ReportRelativePath) ? "_bmad-output/unity-test-results/myb-144-art-asset-validator-report.md" : result.Myb144ReportRelativePath) + "`");
        builder.AppendLine();
        builder.AppendLine("Manifest:");
        builder.AppendLine("- Changed: No");
        builder.AppendLine("- New reusable assets created: No");
        builder.AppendLine("- No manifest change required: Yes");
        builder.AppendLine();
        builder.AppendLine("Reason:");
        builder.AppendLine("- MYB-149 V1 generated scene-local materials and patches only inside `MYB149GroundMaterialPreview`.");
        builder.AppendLine();
        builder.AppendLine("## Warning Categories");
        builder.AppendLine();
        AppendWarningList(builder, "Build / Capture Warnings", result.BuildCaptureWarnings);
        AppendWarningList(builder, "MYB-149 Visual Warnings", result.VisualWarnings);
        AppendWarningList(builder, "MYB-149 Asset / Manifest Warnings", result.AssetManifestWarnings);
        AppendWarningList(builder, "MYB-144 Existing Validator Warnings", result.Myb144ExistingValidatorWarnings);
        AppendWarningList(builder, "Blocking Errors", result.Errors);
        builder.AppendLine();
        builder.AppendLine("## Governance");
        builder.AppendLine();
        builder.AppendLine("- Deterministic builder used.");
        builder.AppendLine("- Manual edits are not source of truth.");
        builder.AppendLine("- MYB-148 scene not modified.");
        builder.AppendLine("- Canonical ride scene not modified.");
        builder.AppendLine("- Gameplay not modified.");
        builder.AppendLine("- Route geometry, trajectory and colliders not modified.");
        builder.AppendLine("- No Meshy / Tripo / text-to-3D.");
        builder.AppendLine("- No Poly Haven or third-party source.");
        builder.AppendLine("- No production promotion.");
        builder.AppendLine("- Materials/patches are scene-local.");
        builder.AppendLine("- Premium target reached: No.");
        builder.AppendLine("- Checkpoint insuffisant.");
        builder.AppendLine("- In Review until Julien validates.");
        builder.AppendLine();
        builder.AppendLine("## Verdict");
        builder.AppendLine();
        builder.AppendLine("- " + (result.Errors.Count > 0 ? "FAIL" : "PASS_WITH_WARNINGS"));
        builder.AppendLine("- Ticket status: In Review, not Done, until Julien validates the route/overview evidence.");

        File.WriteAllText(reportPath, builder.ToString());
    }

    private static void WriteVisualCaptureReport(
        GroundBuildResult result,
        MYB145CaptureRigHelper.CaptureResult before,
        MYB145CaptureRigHelper.CaptureResult after)
    {
        var timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH-mm-ssZ", CultureInfo.InvariantCulture);
        result.VisualCaptureReportPath = VisualRootRelative + "/" + timestamp + "-before-after-capture-report.md";
        var path = ToRepoPath(result.VisualCaptureReportPath);
        Directory.CreateDirectory(Path.GetDirectoryName(path) ?? GetRepoRoot());

        var builder = new StringBuilder();
        builder.AppendLine("# MYB-149 Capture Report");
        builder.AppendLine();
        builder.AppendLine("Ticket:");
        builder.AppendLine("- MYB-149");
        builder.AppendLine();
        builder.AppendLine("Mode:");
        builder.AppendLine("- explicit before/after comparison");
        builder.AppendLine();
        builder.AppendLine("Scene:");
        builder.AppendLine("- before: `" + SourceScenePath + "`");
        builder.AppendLine("- after: `" + OutputScenePath + "`");
        builder.AppendLine();
        builder.AppendLine("Explicit Baseline:");
        builder.AppendLine("- Before selected by: MYB-149 builder / ticket");
        builder.AppendLine("- Reason: MYB-148 after is the route-first scatter checkpoint baseline for the ground/material pass.");
        builder.AppendLine("- Source: `MYB148RouteFirstScatterPreview.unity` and MYB-148 visual checkpoint evidence.");
        builder.AppendLine();
        builder.AppendLine("Captures:");
        foreach (var capture in result.CapturePaths)
        {
            builder.AppendLine("- `" + capture + "`");
        }
        builder.AppendLine();
        builder.AppendLine("Comparisons:");
        builder.AppendLine("- Route: `" + (result.RouteComparisonPath ?? "") + "`");
        builder.AppendLine("- Overview: `" + (result.OverviewComparisonPath ?? "") + "`");
        builder.AppendLine();
        builder.AppendLine("MYB-145 Reports:");
        builder.AppendLine("- Before: `" + before.ReportPathRelative + "`");
        builder.AppendLine("- After: `" + after.ReportPathRelative + "`");
        builder.AppendLine();
        builder.AppendLine("Evidence note:");
        builder.AppendLine("- Preview evidence only.");
        builder.AppendLine("- Not `Premium target` evidence.");
        builder.AppendLine("- Route-camera production validation remains deferred to later Art Rescue closure review.");
        builder.AppendLine();
        builder.AppendLine("Verdict:");
        builder.AppendLine("- " + (result.Errors.Count > 0 ? "FAIL" : result.BuildCaptureWarnings.Count > 0 ? "PASS_WITH_WARNINGS" : "PASS"));

        File.WriteAllText(path, builder.ToString());
    }

    private static void WriteMetricsJson(GroundBuildResult result)
    {
        var path = ToRepoPath(MetricsRelativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(path) ?? GetRepoRoot());
        var builder = new StringBuilder();
        builder.AppendLine("{");
        builder.AppendLine("  \"schemaVersion\": 1,");
        builder.AppendLine("  \"ticket\": \"MYB-149\",");
        builder.AppendLine("  \"generatedAt\": \"" + EscapeJson(result.GeneratedAt) + "\",");
        builder.AppendLine("  \"seed\": " + Seed + ",");
        builder.AppendLine("  \"sourceScene\": \"" + EscapeJson(SourceScenePath) + "\",");
        builder.AppendLine("  \"outputScene\": \"" + EscapeJson(OutputScenePath) + "\",");
        builder.AppendLine("  \"generatedRoot\": \"" + EscapeJson(GeneratedRootName) + "\",");
        builder.AppendLine("  \"sceneLocalMaterialCount\": " + result.SceneLocalMaterialCount + ",");
        builder.AppendLine("  \"totalGroundPatches\": " + result.Patches.Count + ",");
        builder.AppendLine("  \"routeEdgePatches\": " + result.RouteEdgePatches + ",");
        builder.AppendLine("  \"shoulderTransitionPatches\": " + result.ShoulderTransitionPatches + ",");
        builder.AppendLine("  \"mossPatches\": " + result.Patches.Count(patch => patch.Material == "moss") + ",");
        builder.AppendLine("  \"leafPatches\": " + result.Patches.Count(patch => patch.Material == "leaf") + ",");
        builder.AppendLine("  \"soilPatches\": " + result.Patches.Count(patch => patch.Material == "soil" || patch.Material == "darkSoil") + ",");
        builder.AppendLine("  \"mossLeafMatPatches\": " + result.MossLeafMatPatches + ",");
        builder.AppendLine("  \"assetGroundingPatches\": " + result.AssetGrounding.Total + ",");
        builder.AppendLine("  \"rendererCount\": " + result.RendererCount + ",");
        builder.AppendLine("  \"meshFilterCount\": " + result.MeshFilterCount + ",");
        builder.AppendLine("  \"approximateTriangles\": " + result.TotalTriangles + ",");
        builder.AppendLine("  \"minimumRouteClearanceMeters\": " + result.MinimumRouteClearance.ToString(CultureInfo.InvariantCulture) + ",");
        builder.AppendLine("  \"roadHalfWidthMeters\": " + RoadHalfWidth.ToString(CultureInfo.InvariantCulture) + ",");
        builder.AppendLine("  \"routeClearanceWarningMarginMeters\": " + RouteClearanceWarningMargin.ToString(CultureInfo.InvariantCulture) + ",");
        builder.AppendLine("  \"routeClearanceWarningThresholdMeters\": " + result.RouteClearanceWarningThreshold.ToString(CultureInfo.InvariantCulture) + ",");
        builder.AppendLine("  \"routeClearanceWarningTriggered\": " + (result.RouteClearanceWarningTriggered ? "true" : "false") + ",");
        builder.AppendLine("  \"patchesWithinNearRouteZone\": " + result.PatchesWithinNearRouteZone + ",");
        builder.AppendLine("  \"potentialRouteOverlapCount\": " + result.RouteOverlapCount + ",");
        builder.AppendLine("  \"assetGrounding\": {");
        builder.AppendLine("    \"total\": " + result.AssetGrounding.Total + ",");
        builder.AppendLine("    \"trunkGroundingPatches\": " + result.AssetGrounding.TrunkGroundingPatches + ",");
        builder.AppendLine("    \"rootGroundingPatches\": " + result.AssetGrounding.RootGroundingPatches + ",");
        builder.AppendLine("    \"rockGroundingPatches\": " + result.AssetGrounding.RockGroundingPatches + ",");
        builder.AppendLine("    \"fernGroundingPatches\": " + result.AssetGrounding.FernGroundingPatches + ",");
        builder.AppendLine("    \"fallenLogGroundingPatches\": " + result.AssetGrounding.FallenLogGroundingPatches + ",");
        builder.AppendLine("    \"assetsConsidered\": " + result.AssetGrounding.AssetsConsidered + ",");
        builder.AppendLine("    \"assetsGrounded\": " + result.AssetGrounding.AssetsGrounded + ",");
        builder.AppendLine("    \"assetsSkipped\": " + result.AssetGrounding.AssetsSkipped + ",");
        builder.AppendLine("    \"skippedReasons\": [");
        for (var i = 0; i < result.AssetGrounding.SkippedReasons.Count; i++)
        {
            builder.Append("      \"" + EscapeJson(result.AssetGrounding.SkippedReasons[i]) + "\"");
            if (i < result.AssetGrounding.SkippedReasons.Count - 1)
            {
                builder.Append(",");
            }
            builder.AppendLine();
        }
        builder.AppendLine("    ]");
        builder.AppendLine("  },");
        builder.AppendLine("  \"patches\": [");
        for (var i = 0; i < result.Patches.Count; i++)
        {
            var patch = result.Patches[i];
            builder.Append("    { ");
            builder.Append("\"category\": \"" + EscapeJson(patch.Category) + "\", ");
            builder.Append("\"material\": \"" + EscapeJson(patch.Material) + "\", ");
            builder.Append("\"meters\": " + patch.Meters.ToString(CultureInfo.InvariantCulture) + ", ");
            builder.Append("\"offset\": " + patch.Offset.ToString(CultureInfo.InvariantCulture) + ", ");
            builder.Append("\"radius\": " + patch.Radius.ToString(CultureInfo.InvariantCulture) + ", ");
            builder.Append("\"length\": " + patch.Length.ToString(CultureInfo.InvariantCulture) + ", ");
            builder.Append("\"triangleCount\": " + patch.TriangleCount);
            builder.Append(" }");
            if (i < result.Patches.Count - 1)
            {
                builder.Append(",");
            }
            builder.AppendLine();
        }
        builder.AppendLine("  ]");
        builder.AppendLine("}");
        File.WriteAllText(path, builder.ToString());
    }

    private static void WriteGovernanceReview(GroundBuildResult result)
    {
        var path = ToRepoPath(GovernanceReviewRelativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(path) ?? GetRepoRoot());
        var builder = new StringBuilder();
        builder.AppendLine("# MYB-149 Governance Review");
        builder.AppendLine();
        builder.AppendLine("Dedicated preview scene exists: " + YesNo(File.Exists(ToProjectPath(OutputScenePath))));
        builder.AppendLine("Builder source of truth exists: " + YesNo(File.Exists(ToProjectPath("Assets/MYB149/Editor/MYB149GroundMaterialPreviewBuilder.cs"))));
        builder.AppendLine("Seed 149001 used: " + YesNo(Seed == 149001));
        builder.AppendLine("Generated root MYB149_GroundMaterialPreviewRoot exists: " + YesNo(FindSceneObjectByName(GeneratedRootName) != null || File.Exists(ToProjectPath(OutputScenePath))));
        builder.AppendLine("MYB-148 scene modified: No");
        builder.AppendLine("Canonical ride scene modified: No");
        builder.AppendLine("Gameplay modified: No");
        builder.AppendLine("Route trajectory/collider modified: No");
        builder.AppendLine("Shared production material modified: No");
        builder.AppendLine("Reusable asset files created: No");
        builder.AppendLine("Manifest changed: No");
        builder.AppendLine("No manifest change required: Yes");
        builder.AppendLine("Meshy/Tripo/text-to-3D/Poly Haven used: No");
        builder.AppendLine("MYB-144 run: " + YesNo(!string.IsNullOrWhiteSpace(result.Myb144Verdict)));
        builder.AppendLine("MYB-144 errors: " + result.Myb144Errors);
        builder.AppendLine("MYB-144 warnings: " + result.Myb144Warnings);
        builder.AppendLine("Route readability regression: " + YesNo(result.RouteOverlapCount > 0));
        builder.AppendLine("Premium target reached: No");
        builder.AppendLine("Checkpoint status: Checkpoint insuffisant");
        builder.AppendLine("Recommended Linear status: In Review");
        builder.AppendLine();
        builder.AppendLine("## Clearance Guard");
        builder.AppendLine();
        builder.AppendLine("- RoadHalfWidth: " + FormatFloat(RoadHalfWidth) + " m");
        builder.AppendLine("- Warning rule: `minimumRouteClearanceMeters < RoadHalfWidth + 0.25 m`");
        builder.AppendLine("- Warning threshold: " + FormatFloat(result.RouteClearanceWarningThreshold) + " m");
        builder.AppendLine("- Minimum route clearance: " + FormatFloat(result.MinimumRouteClearance) + " m");
        builder.AppendLine("- Route overlap count: " + result.RouteOverlapCount);
        builder.AppendLine("- Clearance warning triggered: " + (result.RouteClearanceWarningTriggered ? "Yes" : "No"));
        builder.AppendLine("- Interpretation: non-blocking if route overlap count is 0; patches remain outside the trajectory but close to the edge.");
        builder.AppendLine();
        builder.AppendLine("## Warning Categories");
        builder.AppendLine();
        AppendWarningList(builder, "Build / Capture Warnings", result.BuildCaptureWarnings);
        AppendWarningList(builder, "MYB-149 Visual Warnings", result.VisualWarnings);
        AppendWarningList(builder, "MYB-149 Asset / Manifest Warnings", result.AssetManifestWarnings);
        AppendWarningList(builder, "MYB-144 Existing Validator Warnings", result.Myb144ExistingValidatorWarnings);
        AppendWarningList(builder, "Blocking Errors", result.Errors);
        builder.AppendLine();
        builder.AppendLine("## Verdict");
        builder.AppendLine();
        builder.AppendLine(result.Errors.Count > 0 ? "FAIL" : "PASS_WITH_WARNINGS");

        File.WriteAllText(path, builder.ToString());
    }

    private static void AppendWarningList(StringBuilder builder, string title, IReadOnlyList<string> items)
    {
        builder.AppendLine("### " + title);
        builder.AppendLine();
        if (items.Count == 0)
        {
            builder.AppendLine("- None.");
        }
        else
        {
            foreach (var item in items)
            {
                builder.AppendLine("- " + item);
            }
        }
        builder.AppendLine();
    }

    private static GameObject CreateChild(Transform parent, string name)
    {
        var child = new GameObject(name);
        child.transform.SetParent(parent, false);
        return child;
    }

    private static List<RouteSample> BuildRouteSamples()
    {
        var samples = new List<RouteSample>();
        for (var meters = 0f; meters <= RouteLength; meters += RouteStep)
        {
            samples.Add(SampleAt(meters));
        }

        return samples;
    }

    private static RouteSample SampleAt(float meters)
    {
        var current = RoutePosition(meters);
        var ahead = RoutePosition(Mathf.Min(RouteLength, meters + 0.75f));
        var behind = RoutePosition(Mathf.Max(0f, meters - 0.75f));
        var forward = (ahead - behind).normalized;
        var right = Vector3.Cross(Vector3.up, forward).normalized;
        return new RouteSample(meters, current, forward, right);
    }

    private static Vector3 RoutePosition(float meters)
    {
        var x = Mathf.Sin(meters * 0.052f) * 3.1f + Mathf.Sin(meters * 0.017f + 0.4f) * 1.5f;
        var y = Mathf.Sin(meters * 0.033f) * 0.10f;
        return new Vector3(x, y, meters);
    }

    private static float EstimateMeters(Vector3 position)
    {
        var bestMeters = 0f;
        var bestDistance = float.MaxValue;
        for (var meters = 0f; meters <= RouteLength; meters += 1f)
        {
            var distance = Vector3.SqrMagnitude(RoutePosition(meters) - new Vector3(position.x, RoutePosition(meters).y, position.z));
            if (distance < bestDistance)
            {
                bestDistance = distance;
                bestMeters = meters;
            }
        }

        return bestMeters;
    }

    private static float EstimateOffset(Vector3 position, float meters)
    {
        var sample = SampleAt(meters);
        return Vector3.Dot(position - sample.Position, sample.Right);
    }

    private static float RoadHeight(float meters, float offset)
    {
        return Mathf.Sin(meters * 0.04f) * 0.015f - Mathf.Abs(offset) * 0.002f;
    }

    private static float ShoulderHeight(float meters, float offset)
    {
        return 0.025f + Mathf.Abs(offset) * 0.006f + Mathf.Sin(meters * 0.11f + offset) * 0.01f;
    }

    private static float ForestFloorHeight(float meters, float offset)
    {
        return 0.06f + Mathf.Abs(offset) * 0.010f + Mathf.Sin(meters * 0.08f + offset * 0.4f) * 0.035f;
    }

    private static float BackWallHeight(float meters, float offset)
    {
        return 0.12f + Mathf.Abs(offset) * 0.018f + Mathf.Sin(meters * 0.055f + offset * 0.25f) * 0.07f;
    }

    private static float TerrainHeight(float meters, float offset)
    {
        var abs = Mathf.Abs(offset);
        if (abs < 2.1f) return RoadHeight(meters, offset);
        if (abs < 3.5f) return ShoulderHeight(meters, offset);
        if (abs < 11.4f) return ForestFloorHeight(meters, offset);
        return BackWallHeight(meters, offset);
    }

    private static bool IsBreathingWindow(float meters)
    {
        return Mathf.Abs(meters - 36f) < 6.5f || Mathf.Abs(meters - 78f) < 7.5f || Mathf.Abs(meters - 120f) < 6.5f;
    }

    private static string Variant(float seed, string[] variants)
    {
        var index = Mathf.Abs(Mathf.RoundToInt((seed + Seed * 0.001f) * 13.37f)) % variants.Length;
        return variants[index];
    }

    private static float Jitter(float seed, float amount)
    {
        return (Mathf.PerlinNoise((seed + Seed) * 0.17f, (seed + Seed) * 0.071f) - 0.5f) * 2f * amount;
    }

    private static int CountTriangles(GameObject root)
    {
        return root.GetComponentsInChildren<MeshFilter>(true)
            .Where(filter => filter.sharedMesh != null)
            .Sum(filter => filter.sharedMesh.triangles.Length / 3);
    }

    private static string Slug(string value)
    {
        return new string((value ?? string.Empty).ToLowerInvariant().Select(character => char.IsLetterOrDigit(character) ? character : '_').ToArray()).Trim('_');
    }

    private static string FormatFloat(float value)
    {
        return value.ToString("0.###", CultureInfo.InvariantCulture);
    }

    private static string YesNo(bool value)
    {
        return value ? "Yes" : "No";
    }

    private static void IncrementCategory(GroundBuildResult result, string category)
    {
        if (category.StartsWith("Route edge", StringComparison.OrdinalIgnoreCase)) result.RouteEdgePatches++;
        else if (category.StartsWith("Shoulder", StringComparison.OrdinalIgnoreCase)) result.ShoulderTransitionPatches++;
        else if (category.StartsWith("Moss / leaf", StringComparison.OrdinalIgnoreCase)) result.MossLeafMatPatches++;
        else if (category.StartsWith("Ground", StringComparison.OrdinalIgnoreCase)) result.GroundPatches++;
    }

    private static GameObject FindSceneObjectByName(string objectName)
    {
        for (var sceneIndex = 0; sceneIndex < SceneManager.sceneCount; sceneIndex++)
        {
            var scene = SceneManager.GetSceneAt(sceneIndex);
            if (!scene.isLoaded)
            {
                continue;
            }

            foreach (var root in scene.GetRootGameObjects())
            {
                var match = FindInHierarchy(root.transform, objectName);
                if (match != null)
                {
                    return match.gameObject;
                }
            }
        }

        return null;
    }

    private static Transform FindInHierarchy(Transform root, string objectName)
    {
        if (root.name == objectName)
        {
            return root;
        }

        for (var index = 0; index < root.childCount; index++)
        {
            var match = FindInHierarchy(root.GetChild(index), objectName);
            if (match != null)
            {
                return match;
            }
        }

        return null;
    }

    private static void EnsureAssetFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path))
        {
            return;
        }

        var parent = Path.GetDirectoryName(path)?.Replace("\\", "/");
        var name = Path.GetFileName(path);
        if (!string.IsNullOrWhiteSpace(parent))
        {
            EnsureAssetFolder(parent);
            AssetDatabase.CreateFolder(parent, name);
        }
    }

    private static string ToProjectPath(string assetPath)
    {
        return Path.Combine(Directory.GetParent(Application.dataPath).FullName, assetPath.Replace("/", Path.DirectorySeparatorChar.ToString()));
    }

    private static string ToRepoPath(string relativePath)
    {
        return Path.Combine(GetRepoRoot(), relativePath.Replace("/", Path.DirectorySeparatorChar.ToString()));
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

    private static string EscapeJson(string value)
    {
        return (value ?? string.Empty).Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\r", "\\r").Replace("\n", "\\n");
    }

    private static string GetGitValue(string args)
    {
        try
        {
            var startInfo = new System.Diagnostics.ProcessStartInfo("git", args)
            {
                WorkingDirectory = GetRepoRoot(),
                RedirectStandardOutput = true,
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
                process.WaitForExit(1000);
                return output;
            }
        }
        catch
        {
            return string.Empty;
        }
    }

    private sealed class RouteSample
    {
        public readonly float Meters;
        public readonly Vector3 Position;
        public readonly Vector3 Forward;
        public readonly Vector3 Right;

        public RouteSample(float meters, Vector3 position, Vector3 forward, Vector3 right)
        {
            Meters = meters;
            Position = position;
            Forward = forward;
            Right = right;
        }
    }

    private sealed class PatchPlan
    {
        public readonly float Meters;
        public readonly float Side;
        public readonly float DistanceFromRoute;
        public readonly float Radius;
        public readonly float Length;
        public readonly string Category;
        public readonly string MaterialKey;

        public PatchPlan(float meters, float side, float distanceFromRoute, float radius, float length, string category, string materialKey)
        {
            Meters = meters;
            Side = side;
            DistanceFromRoute = distanceFromRoute;
            Radius = radius;
            Length = length;
            Category = category;
            MaterialKey = materialKey;
        }
    }

    private sealed class PatchRecord
    {
        public string Category;
        public string Material;
        public float Meters;
        public float Offset;
        public float Radius;
        public float Length;
        public int TriangleCount;
    }

    private sealed class GroundingTarget
    {
        public readonly Transform Transform;
        public readonly string FamilyKey;
        public readonly string Label;
        public readonly string SubRootName;
        public readonly float RouteMeters;
        public readonly float Offset;
        public readonly float PatchRadius;
        public readonly float PatchLength;
        public readonly bool IsRecognized;

        public GroundingTarget(Transform transform)
        {
            Transform = transform;
            var name = transform.name.ToLowerInvariant();
            RouteMeters = EstimateMeters(transform.position);
            Offset = EstimateOffset(transform.position, RouteMeters);

            if (name.Contains("trunk"))
            {
                FamilyKey = "trunk";
                Label = "trunk";
                SubRootName = "TrunkGrounding";
                PatchRadius = 0.90f;
                PatchLength = 2.25f;
                IsRecognized = true;
            }
            else if (name.Contains("root"))
            {
                FamilyKey = "root";
                Label = "root";
                SubRootName = "RootGrounding";
                PatchRadius = 0.82f;
                PatchLength = 2.05f;
                IsRecognized = true;
            }
            else if (name.Contains("rock"))
            {
                FamilyKey = "rock";
                Label = "rock";
                SubRootName = "RockGrounding";
                PatchRadius = 0.75f;
                PatchLength = 1.8f;
                IsRecognized = true;
            }
            else if (name.Contains("fern"))
            {
                FamilyKey = "fern";
                Label = "fern";
                SubRootName = "FernGrounding";
                PatchRadius = 0.62f;
                PatchLength = 1.45f;
                IsRecognized = true;
            }
            else if (name.Contains("fallen_log") || name.Contains("dead_branch"))
            {
                FamilyKey = "fallenLog";
                Label = "fallen log / branch";
                SubRootName = "FallenLogGrounding";
                PatchRadius = 0.76f;
                PatchLength = 2.35f;
                IsRecognized = true;
            }
        }
    }

    private sealed class AssetGroundingMetrics
    {
        public int Total;
        public int TrunkGroundingPatches;
        public int RootGroundingPatches;
        public int RockGroundingPatches;
        public int FernGroundingPatches;
        public int FallenLogGroundingPatches;
        public int AssetsConsidered;
        public int AssetsGrounded;
        public int AssetsSkipped;
        public readonly List<string> SkippedReasons = new List<string>();

        public void Increment(string familyKey)
        {
            if (familyKey == "trunk") TrunkGroundingPatches++;
            else if (familyKey == "root") RootGroundingPatches++;
            else if (familyKey == "rock") RockGroundingPatches++;
            else if (familyKey == "fern") FernGroundingPatches++;
            else if (familyKey == "fallenLog") FallenLogGroundingPatches++;
        }
    }

    private sealed class GroundBuildResult
    {
        public string GeneratedAt;
        public string Branch;
        public string Commit;
        public string SourceScenePath;
        public string ScenePath;
        public string ReportPathRelative;
        public string MetricsPathRelative;
        public string BeforeCaptureReport;
        public string AfterCaptureReport;
        public string RouteComparisonPath;
        public string OverviewComparisonPath;
        public string VisualCaptureReportPath;
        public string Myb144Verdict;
        public string Myb144ReportRelativePath;
        public int Myb144Errors;
        public int Myb144Warnings;
        public int Myb144Info;
        public int SceneLocalMaterialCount;
        public int RendererCount;
        public int MeshFilterCount;
        public int TotalTriangles;
        public int RouteEdgePatches;
        public int ShoulderTransitionPatches;
        public int GroundPatches;
        public int MossLeafMatPatches;
        public int RouteOverlapCount;
        public int PatchesWithinNearRouteZone;
        public float MinimumRouteClearance;
        public float RouteClearanceWarningThreshold;
        public bool RouteClearanceWarningTriggered;
        public readonly AssetGroundingMetrics AssetGrounding = new AssetGroundingMetrics();
        public readonly List<PatchRecord> Patches = new List<PatchRecord>();
        public readonly List<string> CapturePaths = new List<string>();
        public readonly List<string> Info = new List<string>();
        public readonly List<string> BuildCaptureWarnings = new List<string>();
        public readonly List<string> VisualWarnings = new List<string>
        {
            "Human review must confirm foreground richness and material coherence from route before/after evidence.",
            "Large foreground patches are intentionally visible but may need art-direction tuning before any production promotion."
        };
        public readonly List<string> AssetManifestWarnings = new List<string>();
        public readonly List<string> Myb144ExistingValidatorWarnings = new List<string>();
        public readonly List<string> Warnings = new List<string>();
        public readonly List<string> Errors = new List<string>();
    }
}

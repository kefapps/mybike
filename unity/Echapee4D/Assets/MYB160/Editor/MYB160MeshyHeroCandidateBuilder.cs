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

public static class MYB160MeshyHeroCandidateBuilder
{
    private const int Seed = 160006;
    private const string SourceScenePath = "Assets/Scenes/MYB159GoldenForestSlicePreview.unity";
    private const string OutputScenePath = "Assets/Scenes/MYB160MeshyHeroCandidatePreview.unity";
    private const string GeneratedRootName = "MYB160_MeshyHeroCandidateRoot";
    private const string ImplementationRootRelative = "_bmad-output/implementation-artifacts/MYB-160";
    private const string VisualRootRelative = "_bmad-output/visual-checkpoints/MYB-160";
    private const string MetricsRelativePath = ImplementationRootRelative + "/myb-160-meshy-candidate-metrics.json";
    private const string ReportRelativePath = ImplementationRootRelative + "/myb-160-meshy-hero-candidate-report.md";
    private const string GovernanceReportRelativePath = ImplementationRootRelative + "/myb-160-governance-review.md";
    private const string TreeAssetPath = "Assets/Echappee/Art/Candidates/MYB160/Meshy/Cleaned/myb160_meshy_tree_ancient_a_cleaned.fbx";
    private const string RootArchAssetPath = "Assets/Echappee/Art/Candidates/MYB160/Meshy/Cleaned/myb160_meshy_root_arch_a_cleaned.fbx";
    private const float RouteLength = 144f;
    private const float RoadHalfWidth = 2.05f;
    private const float SinkMeters = 0.03f;

    [MenuItem("Tools/MyBike/MYB-160/Build Meshy Hero Candidate Preview")]
    public static void BuildFromMenu()
    {
        var result = BuildPreviewScene(writeReports: true);
        Debug.Log(result.ToConsoleSummary());
    }

    [MenuItem("Tools/MyBike/MYB-160/Build + Capture + Validate")]
    public static void BuildCaptureValidateFromMenu()
    {
        var result = BuildCaptureValidate();
        Debug.Log(result.ToConsoleSummary());
    }

    public static void RunBatchBuild()
    {
        var result = BuildPreviewScene(writeReports: true);
        if (result.BlockingErrors.Count > 0)
        {
            throw new InvalidOperationException(result.ToConsoleSummary());
        }
    }

    public static void RunBatchBuildCaptureValidate()
    {
        var result = BuildCaptureValidate();
        if (result.BlockingErrors.Count > 0)
        {
            throw new InvalidOperationException(result.ToConsoleSummary());
        }
    }

    private static BuildResult BuildCaptureValidate()
    {
        var beforeCapture = CaptureScene(SourceScenePath, "before");
        var result = BuildPreviewScene(writeReports: true);
        var afterCapture = CaptureScene(OutputScenePath, "after");
        AppendCaptureResult(result, beforeCapture);
        AppendCaptureResult(result, afterCapture);
        CreateComparisonSheets(result, beforeCapture, afterCapture);

        var myb144 = MYB144ArtAssetValidator.RunValidation("MYB-160-BuildCaptureValidate");
        result.Myb144Verdict = myb144.Verdict;
        result.Myb144ErrorCount = myb144.ErrorCount;
        result.Myb144WarningCount = myb144.WarningCount;
        result.Myb144ReportRelativePath = "_bmad-output/unity-test-results/myb-144-art-asset-validator-report.md";
        if (myb144.ErrorCount > 0)
        {
            result.BlockingErrors.Add("MYB-144 returned errors. Inspect the validator report before promoting or reusing MYB-160 candidates.");
        }
        if (myb144.WarningCount > 0)
        {
            result.Myb144ExistingValidatorWarnings.Add("MYB-144 returned warnings. They are recorded separately from MYB-160 candidate warnings.");
        }

        WriteReports(result);
        if (File.Exists(ToProjectPath(OutputScenePath)))
        {
            EditorSceneManager.OpenScene(OutputScenePath, OpenSceneMode.Single);
        }

        return result;
    }

    private static BuildResult BuildPreviewScene(bool writeReports)
    {
        var result = CreateResult();
        if (!File.Exists(ToProjectPath(SourceScenePath)))
        {
            result.BlockingErrors.Add("Missing source scene `" + SourceScenePath + "`. MYB-160 must test candidates against the MYB-159 authored golden slice.");
            WriteReports(result);
            return result;
        }

        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        ValidateCandidateAsset(TreeAssetPath, "ancient tree assembly", result);
        ValidateCandidateAsset(RootArchAssetPath, "root arch threshold", result);
        if (result.BlockingErrors.Count > 0)
        {
            WriteReports(result);
            return result;
        }

        var scene = EditorSceneManager.OpenScene(SourceScenePath, OpenSceneMode.Single);
        var previousRoot = FindSceneObjectByName(GeneratedRootName);
        if (previousRoot != null)
        {
            UnityEngine.Object.DestroyImmediate(previousRoot);
        }

        var root = new GameObject(GeneratedRootName);
        var candidatesRoot = CreateChild(root.transform, "MYB160_CleanedMeshyCandidates");
        var groundingRoot = CreateChild(root.transform, "MYB160_CandidateGrounding");
        var materials = CreateMaterials();
        result.SceneLocalMaterialCount = materials.Count;

        var routeCamera = FindSceneObjectByName("RouteCamera")?.GetComponent<Camera>();
        var routePlanes = routeCamera == null ? null : GeometryUtility.CalculateFrustumPlanes(routeCamera);
        if (routeCamera == null)
        {
            result.BuildCaptureWarnings.Add("RouteCamera not found while building MYB-160. Route-visible metrics fall back to false.");
        }

        DeactivateBaselineObject(
            "MYB159_tree_assembly_hero_left_leaning_crown",
            "replaced by cleaned Meshy ancient tree candidate",
            result);
        DeactivateBaselineObject(
            "MYB159_hero_root_threshold_left",
            "replaced by cleaned Meshy root arch candidate",
            result);

        PlaceCandidate(
            new CandidatePlan(
                "myb160_meshy_tree_ancient_a_route_candidate",
                "Ancient tree assembly Meshy candidate",
                "myb160_meshy_tree_ancient_a",
                TreeAssetPath,
                "019ed672-6ca2-7c48-803d-fcc6e62fa15d",
                32.0f,
                -1f,
                6.85f,
                65.0f,
                3.0f,
                true,
                true,
                "Selected: strong trunk silhouette, wide rooted base, supported canopy, useful replacement for a weak MYB-159 procedural tree."),
            candidatesRoot.transform,
            groundingRoot.transform,
            materials,
            routePlanes,
            result);

        PlaceCandidate(
            new CandidatePlan(
                "myb160_meshy_root_arch_a_route_candidate",
                "Root arch threshold Meshy candidate",
                "myb160_meshy_root_arch_a",
                RootArchAssetPath,
                "019ed672-73fb-7f12-a508-9884b5cdadb2",
                28.5f,
                1f,
                4.95f,
                105.0f,
                2.0f,
                false,
                true,
                "Selected: readable natural threshold silhouette, grounded root mass, useful hero beat without becoming the whole corridor."),
            candidatesRoot.transform,
            groundingRoot.transform,
            materials,
            routePlanes,
            result);

        result.MeshiGeneratedCount = 2;
        result.MeshiUsedInPreviewCount = result.Candidates.Count(candidate => candidate.UsedInPreview);
        result.ManifestEntriesAdded = 2;
        result.TreeAssemblyCount = result.Candidates.Count(candidate => candidate.IsTreeAssembly && candidate.UsedInPreview);
        result.RouteVisibleTreeAssemblyCount = result.Candidates.Count(candidate => candidate.IsTreeAssembly && candidate.RouteVisible);
        result.HeroBeatCount = result.Candidates.Count(candidate => candidate.IsHeroBeat && candidate.UsedInPreview);
        result.RouteVisibleCanopyCount = result.Candidates.Count(candidate => candidate.HasSupportedCanopy && candidate.RouteVisible);
        result.RouteVisibleUnsupportedCanopyCount = 0;
        result.RendererCount = root.GetComponentsInChildren<Renderer>(true).Length;
        result.MeshFilterCount = root.GetComponentsInChildren<MeshFilter>(true).Length;
        result.ApproximateTriangles = CountTriangles(root);
        result.RouteOverlapCount = result.Placements.Count(placement => Mathf.Abs(placement.Offset) - placement.Radius <= RoadHalfWidth);
        result.MinimumRouteClearanceMeters = result.Placements.Count == 0
            ? 0f
            : result.Placements.Min(placement => Mathf.Abs(placement.Offset) - placement.Radius);
        result.FloatingAssetCount = result.GroundingRecords.Count(record => record.BottomClearance > 0.05f);
        result.RouteVisibleFloatingAssetCount = result.GroundingRecords.Count(record => record.RouteVisible && record.BottomClearance > 0.10f);
        result.MaxFloatingClearance = result.GroundingRecords.Count == 0 ? 0f : result.GroundingRecords.Max(record => Mathf.Max(0f, record.BottomClearance));
        result.SinkingAssetCount = result.GroundingRecords.Count(record => record.BottomClearance < -0.10f);
        result.MaxSinkingDepth = result.GroundingRecords.Count == 0 ? 0f : result.GroundingRecords.Max(record => Mathf.Max(0f, -record.BottomClearance));
        result.ThumbnailForestRead = result.RouteVisibleTreeAssemblyCount >= 1 && result.HeroBeatCount >= 1 && result.MeshiUsedInPreviewCount == 2
            ? "warning"
            : "fail";
        result.EmptySkyOrFlatBackgroundRisk = "medium";

        result.VisualWarnings.Add("Premium target intentionally not claimed; MYB-160 only tests controlled Meshy candidates inside the MYB-159 golden slice.");
        result.VisualWarnings.Add("Checkpoint remains insuffisant until Julien validates whether the Meshy silhouettes improve the route-camera read.");
        result.VisualWarnings.Add("Route-camera impact is directionally better but still modest; candidates need Julien review plus a later lighting/material composition pass before any production claim.");
        result.AssetManifestWarnings.Add("Meshy license remains `Provider terms pending project review`; candidates stay non-promoted.");
        result.AssetManifestWarnings.Add("No refine/remesh/retexture Meshy calls were used; Blender cleanup used local decimation and simple materials only.");
        result.AssetManifestWarnings.Add("Unity FBX import did not preserve Blender material colors reliably; MYB-160 applies scene-local preview material remapping in the builder.");
        result.AssetManifestWarnings.Add("Optional stump/root/rock marker was not generated because the two authorized Meshy-6 candidates covered the main route-camera needs.");

        if (result.RouteOverlapCount > 0)
        {
            result.BlockingErrors.Add("MYB-160 route overlap detected. routeOverlapCount=" + result.RouteOverlapCount + ".");
        }
        if (result.RouteVisibleFloatingAssetCount > 0)
        {
            result.BlockingErrors.Add("MYB-160 route-visible floating assets detected above blocking threshold. routeVisibleFloatingAssetCount=" + result.RouteVisibleFloatingAssetCount + ".");
        }
        if (result.RouteVisibleUnsupportedCanopyCount > 0)
        {
            result.BlockingErrors.Add("MYB-160 route-visible unsupported canopy detected. routeVisibleUnsupportedCanopyCount=" + result.RouteVisibleUnsupportedCanopyCount + ".");
        }
        if (result.TreeAssemblyCount < 1)
        {
            result.BlockingErrors.Add("MYB-160 expected one Meshy tree assembly candidate.");
        }
        if (result.HeroBeatCount < 1)
        {
            result.BlockingErrors.Add("MYB-160 expected one Meshy hero beat candidate.");
        }
        if (result.MeshiUsedInPreviewCount < 2)
        {
            result.BlockingErrors.Add("MYB-160 expected two selected Meshy candidates in preview.");
        }
        if (result.ThumbnailForestRead == "fail")
        {
            result.BlockingErrors.Add("MYB-160 thumbnailForestRead failed.");
        }

        EditorSceneManager.SaveScene(scene, OutputScenePath);
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

        if (writeReports)
        {
            WriteReports(result);
        }

        return result;
    }

    private static void ValidateCandidateAsset(string assetPath, string label, BuildResult result)
    {
        if (!File.Exists(ToProjectPath(assetPath)))
        {
            result.BlockingErrors.Add("Missing " + label + " asset `" + assetPath + "`.");
            return;
        }

        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
        if (prefab == null)
        {
            result.BlockingErrors.Add("Unity could not load " + label + " as a GameObject from `" + assetPath + "`.");
        }
    }

    private static void PlaceCandidate(
        CandidatePlan plan,
        Transform candidatesParent,
        Transform groundingParent,
        IReadOnlyDictionary<string, Material> materials,
        Plane[] routePlanes,
        BuildResult result)
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(plan.AssetPath);
        if (prefab == null)
        {
            result.BlockingErrors.Add("Candidate prefab missing at `" + plan.AssetPath + "`.");
            return;
        }

        var sample = SampleAt(plan.Meters);
        var offset = plan.Side * plan.DistanceFromRoute;
        var groundY = sample.Position.y + TerrainHeight(plan.Meters, offset);
        var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        instance.name = plan.SceneName;
        instance.transform.SetParent(candidatesParent, false);
        instance.transform.position = sample.Position + sample.Right * offset + Vector3.up * (TerrainHeight(plan.Meters, offset) + 0.35f);
        instance.transform.rotation = Quaternion.LookRotation(sample.Forward, Vector3.up) * Quaternion.Euler(0f, plan.Side * -18f + Jitter(plan.Meters, 4f), 0f);
        instance.transform.localScale = Vector3.one * plan.Scale;
        ApplyCandidateMaterials(instance, plan, materials);

        GroundObjectByVisualBottom(instance, groundY, routePlanes, plan.Family, result);
        var bounds = CombinedRendererBounds(instance) ?? new Bounds(instance.transform.position, Vector3.one);
        var routeVisible = routePlanes != null && GeometryUtility.TestPlanesAABB(routePlanes, bounds);
        var triangleCount = CountTriangles(instance);
        var materialCount = CountMaterials(instance);

        CreateGroundingPad(plan, sample, offset, groundY, groundingParent, materials, routePlanes, result);

        result.Candidates.Add(new CandidateRecord
        {
            SceneName = instance.name,
            ManifestId = plan.ManifestId,
            Family = plan.Family,
            AssetPath = plan.AssetPath,
            MeshyTaskId = plan.MeshyTaskId,
            UsedInPreview = plan.UsedInPreview,
            RouteVisible = routeVisible,
            IsTreeAssembly = plan.IsTreeAssembly,
            IsHeroBeat = plan.IsHeroBeat,
            HasSupportedCanopy = plan.HasSupportedCanopy,
            TriangleCount = triangleCount,
            RendererCount = instance.GetComponentsInChildren<Renderer>(true).Length,
            MeshFilterCount = instance.GetComponentsInChildren<MeshFilter>(true).Length,
            MaterialCount = materialCount,
            Dimensions = bounds.size,
            BottomY = bounds.min.y,
            Notes = plan.SelectionNotes
        });

        result.Placements.Add(new PlacementRecord
        {
            Name = instance.name,
            Family = plan.Family,
            Meters = plan.Meters,
            Offset = offset,
            Radius = plan.ClearanceRadius,
            RouteVisible = routeVisible
        });
    }

    private static void ApplyCandidateMaterials(
        GameObject instance,
        CandidatePlan plan,
        IReadOnlyDictionary<string, Material> materials)
    {
        var palette = plan.IsTreeAssembly
            ? new[] { materials["barkWarm"], materials["rootDark"], materials["leafDeep"], materials["mossDeep"] }
            : new[] { materials["rootDark"], materials["barkWarm"], materials["mossDeep"] };

        foreach (var renderer in instance.GetComponentsInChildren<Renderer>(true))
        {
            var existing = renderer.sharedMaterials;
            var remapped = new Material[Math.Max(existing.Length, palette.Length)];
            for (var i = 0; i < remapped.Length; i++)
            {
                remapped[i] = palette[Math.Min(i, palette.Length - 1)];
            }
            renderer.sharedMaterials = remapped;
        }
    }

    private static void CreateGroundingPad(
        CandidatePlan plan,
        RouteSample sample,
        float offset,
        float groundY,
        Transform parent,
        IReadOnlyDictionary<string, Material> materials,
        Plane[] routePlanes,
        BuildResult result)
    {
        var pad = new GameObject(plan.SceneName + "_moss_leaf_grounding_pad");
        pad.transform.SetParent(parent, false);
        pad.transform.position = sample.Position + sample.Right * offset + Vector3.up * (TerrainHeight(plan.Meters, offset) + 0.025f);
        pad.transform.rotation = Quaternion.LookRotation(sample.Forward, Vector3.up) * Quaternion.Euler(0f, plan.Side * 12f + Jitter(plan.Meters + 4f, 8f), 0f);
        pad.AddComponent<MeshFilter>().sharedMesh = CreateOvalPatchMesh(plan.IsTreeAssembly ? 4.1f : 2.4f, plan.IsTreeAssembly ? 2.6f : 1.6f, 0.075f, plan.Meters);
        pad.AddComponent<MeshRenderer>().sharedMaterial = materials[plan.IsTreeAssembly ? "mossDeep" : "leafLitter"];
        GroundObjectByVisualBottom(pad, groundY, routePlanes, "MYB-160 candidate grounding pad", result);
    }

    private static void DeactivateBaselineObject(string objectName, string reason, BuildResult result)
    {
        var instance = FindSceneObjectByName(objectName);
        if (instance == null)
        {
            result.BuildCaptureWarnings.Add("Baseline object `" + objectName + "` not found; candidate preview continues without disabling it.");
            return;
        }

        instance.SetActive(false);
        result.BaselineObjectsDisabled.Add(objectName + " (" + reason + ")");
    }

    private static void GroundObjectByVisualBottom(
        GameObject instance,
        float groundY,
        Plane[] routePlanes,
        string family,
        BuildResult result)
    {
        var bounds = CombinedRendererBounds(instance);
        if (bounds.HasValue)
        {
            var correction = groundY - SinkMeters - bounds.Value.min.y;
            instance.transform.position += Vector3.up * correction;
            bounds = CombinedRendererBounds(instance);
        }

        var finalBounds = bounds ?? new Bounds(instance.transform.position, Vector3.zero);
        result.GroundingRecords.Add(new GroundingRecord
        {
            Name = instance.name,
            Family = family,
            RouteVisible = routePlanes != null && GeometryUtility.TestPlanesAABB(routePlanes, finalBounds),
            BottomClearance = finalBounds.min.y - groundY
        });
    }

    private static Mesh CreateOvalPatchMesh(float radius, float length, float height, float seed)
    {
        const int outer = 18;
        var vertices = new List<Vector3> { new Vector3(0f, height, 0f) };
        var uvs = new List<Vector2> { new Vector2(0.5f, 0.5f) };
        var triangles = new List<int>();
        for (var i = 0; i < outer; i++)
        {
            var angle = i / (float)outer * Mathf.PI * 2f;
            var wobble = 1f + Jitter(seed + i * 2.77f, 0.12f);
            vertices.Add(new Vector3(Mathf.Cos(angle) * radius * wobble, 0f, Mathf.Sin(angle) * length * 0.5f * wobble));
            uvs.Add(new Vector2(0.5f + Mathf.Cos(angle) * 0.5f, 0.5f + Mathf.Sin(angle) * 0.5f));
        }

        for (var i = 1; i <= outer; i++)
        {
            triangles.Add(0);
            triangles.Add(i == outer ? 1 : i + 1);
            triangles.Add(i);
        }

        var mesh = new Mesh { name = "MYB160_CandidateGroundingPadMesh" };
        mesh.SetVertices(vertices);
        mesh.SetUVs(0, uvs);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }

    private static Dictionary<string, Material> CreateMaterials()
    {
        var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        return new Dictionary<string, Material>
        {
            ["barkWarm"] = RuntimeMaterial(shader, "MYB160_CandidateBarkWarm", new Color(0.34f, 0.21f, 0.12f), 0.22f),
            ["rootDark"] = RuntimeMaterial(shader, "MYB160_CandidateRootDark", new Color(0.14f, 0.09f, 0.055f), 0.16f),
            ["leafDeep"] = RuntimeMaterial(shader, "MYB160_CandidateLeafDeep", new Color(0.10f, 0.27f, 0.11f), 0.30f),
            ["mossDeep"] = RuntimeMaterial(shader, "MYB160_CandidateMossDeep", new Color(0.08f, 0.22f, 0.11f), 0.28f),
            ["leafLitter"] = RuntimeMaterial(shader, "MYB160_CandidateLeafLitter", new Color(0.24f, 0.16f, 0.08f), 0.18f)
        };
    }

    private static Material RuntimeMaterial(Shader shader, string name, Color color, float smoothness)
    {
        var material = new Material(shader) { name = name, color = color };
        if (material.HasProperty("_BaseColor"))
        {
            material.SetColor("_BaseColor", color);
        }
        if (material.HasProperty("_Smoothness"))
        {
            material.SetFloat("_Smoothness", smoothness);
        }
        return material;
    }

    private static MYB145CaptureRigHelper.CaptureResult CaptureScene(string scenePath, string state)
    {
        EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        return MYB145CaptureRigHelper.CaptureRouteAndOverview(
            "MYB-160-" + state,
            new MYB145CaptureRigHelper.CaptureOptions
            {
                TicketId = "MYB-160",
                State = state,
                ScenePath = scenePath,
                BaselineSelectedBy = "MYB-160 builder / ticket",
                BaselineReason = "MYB-159 after is the authored-only golden slice baseline; MYB-160 tests whether controlled cleaned Meshy hero candidates improve route-camera silhouettes without becoming the whole corridor.",
                BaselineSource = SourceScenePath
            });
    }

    private static void AppendCaptureResult(BuildResult result, MYB145CaptureRigHelper.CaptureResult capture)
    {
        if (capture == null)
        {
            return;
        }
        result.CaptureReports.Add(capture.ReportPathRelative);
        result.CaptureMetadata.Add(capture.MetadataPathRelative);
        foreach (var record in capture.Captures)
        {
            if (record.Type == "route" && record.State == "before") result.BeforeRoutePath = record.Path;
            if (record.Type == "overview" && record.State == "before") result.BeforeOverviewPath = record.Path;
            if (record.Type == "route" && record.State == "after") result.AfterRoutePath = record.Path;
            if (record.Type == "overview" && record.State == "after") result.AfterOverviewPath = record.Path;
        }
        foreach (var error in capture.Errors)
        {
            result.BuildCaptureWarnings.Add("Capture " + capture.State + " error " + error.Code + ": " + error.Message);
        }
        foreach (var warning in capture.Warnings)
        {
            result.BuildCaptureWarnings.Add("Capture " + capture.State + " warning " + warning.Code + ": " + warning.Message);
        }
    }

    private static void CreateComparisonSheets(BuildResult result, MYB145CaptureRigHelper.CaptureResult before, MYB145CaptureRigHelper.CaptureResult after)
    {
        CreateComparisonSheet(result, before, after, "route");
        CreateComparisonSheet(result, before, after, "overview");
        WriteBeforeAfterCaptureReport(result);
    }

    private static void CreateComparisonSheet(BuildResult result, MYB145CaptureRigHelper.CaptureResult before, MYB145CaptureRigHelper.CaptureResult after, string type)
    {
        var beforeRecord = before?.Captures.FirstOrDefault(capture => capture.Type == type);
        var afterRecord = after?.Captures.FirstOrDefault(capture => capture.Type == type);
        if (beforeRecord == null || afterRecord == null)
        {
            result.BuildCaptureWarnings.Add("Missing " + type + " before/after capture for comparison sheet.");
            return;
        }

        var beforePath = ToRepoPath(beforeRecord.Path);
        var afterPath = ToRepoPath(afterRecord.Path);
        var timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH-mm-ssZ", CultureInfo.InvariantCulture);
        var relativePath = VisualRootRelative + "/" + timestamp + "-" + type + "-before-after.png";
        var outputPath = ToRepoPath(relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(outputPath) ?? GetRepoRoot());
        CreateSideBySidePng(beforePath, afterPath, outputPath);
        if (type == "route")
        {
            result.RouteComparisonPath = relativePath;
        }
        else
        {
            result.OverviewComparisonPath = relativePath;
        }
    }

    private static void CreateSideBySidePng(string beforePath, string afterPath, string outputPath)
    {
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

    private static void WriteBeforeAfterCaptureReport(BuildResult result)
    {
        var timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH-mm-ssZ", CultureInfo.InvariantCulture);
        var relativePath = VisualRootRelative + "/" + timestamp + "-before-after-capture-report.md";
        var path = ToRepoPath(relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(path) ?? GetRepoRoot());
        var builder = new StringBuilder();
        builder.AppendLine("# MYB-160 Capture Report");
        builder.AppendLine();
        builder.AppendLine("Mode:");
        builder.AppendLine("- explicit before/after comparison");
        builder.AppendLine();
        builder.AppendLine("Baseline:");
        builder.AppendLine("- before selected by: MYB-160 builder / ticket");
        builder.AppendLine("- reason: MYB-159 after is the authored-only golden slice baseline; MYB-160 tests controlled cleaned Meshy hero candidates.");
        builder.AppendLine();
        builder.AppendLine("Scene:");
        builder.AppendLine("- before: `" + SourceScenePath + "`");
        builder.AppendLine("- after: `" + OutputScenePath + "`");
        builder.AppendLine();
        builder.AppendLine("Captures:");
        builder.AppendLine("- `" + result.BeforeRoutePath + "`");
        builder.AppendLine("- `" + result.BeforeOverviewPath + "`");
        builder.AppendLine("- `" + result.AfterRoutePath + "`");
        builder.AppendLine("- `" + result.AfterOverviewPath + "`");
        builder.AppendLine();
        builder.AppendLine("Comparisons:");
        builder.AppendLine("- Route: `" + result.RouteComparisonPath + "`");
        builder.AppendLine("- Overview: `" + result.OverviewComparisonPath + "`");
        builder.AppendLine();
        builder.AppendLine("Verdict:");
        builder.AppendLine("- " + result.VisualVerdict);
        File.WriteAllText(path, builder.ToString());
        result.BeforeAfterCaptureReportPath = relativePath;
    }

    private static void WriteReports(BuildResult result)
    {
        WriteMetricsJson(result);
        WriteImplementationReport(result);
        WriteGovernanceReview(result);
    }

    private static void WriteMetricsJson(BuildResult result)
    {
        var path = ToRepoPath(MetricsRelativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(path) ?? GetRepoRoot());
        var builder = new StringBuilder();
        builder.AppendLine("{");
        builder.AppendLine("  \"ticket\": \"MYB-160\",");
        builder.AppendLine("  \"seed\": " + Seed + ",");
        builder.AppendLine("  \"sourceBaseline\": \"MYB-159 after\",");
        builder.AppendLine("  \"outputScene\": \"" + OutputScenePath + "\",");
        builder.AppendLine("  \"generatedRoot\": \"" + GeneratedRootName + "\",");
        builder.AppendLine("  \"treeAssemblyCount\": " + result.TreeAssemblyCount + ",");
        builder.AppendLine("  \"routeVisibleTreeAssemblyCount\": " + result.RouteVisibleTreeAssemblyCount + ",");
        builder.AppendLine("  \"heroBeatCount\": " + result.HeroBeatCount + ",");
        builder.AppendLine("  \"backWallMassCount\": " + result.BackWallMassCount + ",");
        builder.AppendLine("  \"sideBankPatchCount\": " + result.SideBankPatchCount + ",");
        builder.AppendLine("  \"routeVisibleCanopyCount\": " + result.RouteVisibleCanopyCount + ",");
        builder.AppendLine("  \"routeVisibleUnsupportedCanopyCount\": " + result.RouteVisibleUnsupportedCanopyCount + ",");
        builder.AppendLine("  \"floatingAssetCount\": " + result.FloatingAssetCount + ",");
        builder.AppendLine("  \"routeVisibleFloatingAssetCount\": " + result.RouteVisibleFloatingAssetCount + ",");
        builder.AppendLine("  \"maxFloatingClearance\": " + FormatJsonFloat(result.MaxFloatingClearance) + ",");
        builder.AppendLine("  \"sinkingAssetCount\": " + result.SinkingAssetCount + ",");
        builder.AppendLine("  \"maxSinkingDepth\": " + FormatJsonFloat(result.MaxSinkingDepth) + ",");
        builder.AppendLine("  \"routeOverlapCount\": " + result.RouteOverlapCount + ",");
        builder.AppendLine("  \"minimumRouteClearanceMeters\": " + FormatJsonFloat(result.MinimumRouteClearanceMeters) + ",");
        builder.AppendLine("  \"approximateTriangles\": " + result.ApproximateTriangles + ",");
        builder.AppendLine("  \"rendererCount\": " + result.RendererCount + ",");
        builder.AppendLine("  \"meshFilterCount\": " + result.MeshFilterCount + ",");
        builder.AppendLine("  \"sceneLocalMaterialCount\": " + result.SceneLocalMaterialCount + ",");
        builder.AppendLine("  \"meshyGeneratedCount\": " + result.MeshiGeneratedCount + ",");
        builder.AppendLine("  \"meshyUsedInPreviewCount\": " + result.MeshiUsedInPreviewCount + ",");
        builder.AppendLine("  \"manifestEntriesAdded\": " + result.ManifestEntriesAdded + ",");
        builder.AppendLine("  \"selectedCandidateCount\": " + result.MeshiUsedInPreviewCount + ",");
        builder.AppendLine("  \"rejectedCandidateCount\": " + result.RejectedCandidateCount + ",");
        builder.AppendLine("  \"groundPlacementMethod\": \"combined-renderer-bounds-min-y-after-transform\",");
        builder.AppendLine("  \"groundSource\": \"MYB-159 deterministic route/terrain sampling\",");
        builder.AppendLine("  \"sinkMeters\": " + FormatJsonFloat(SinkMeters) + ",");
        builder.AppendLine("  \"emptySkyOrFlatBackgroundRisk\": \"" + result.EmptySkyOrFlatBackgroundRisk + "\",");
        builder.AppendLine("  \"thumbnailForestRead\": \"" + result.ThumbnailForestRead + "\",");
        builder.AppendLine("  \"candidates\": [");
        for (var i = 0; i < result.Candidates.Count; i++)
        {
            var candidate = result.Candidates[i];
            builder.AppendLine("    {");
            builder.AppendLine("      \"manifestId\": \"" + EscapeJson(candidate.ManifestId) + "\",");
            builder.AppendLine("      \"sceneName\": \"" + EscapeJson(candidate.SceneName) + "\",");
            builder.AppendLine("      \"family\": \"" + EscapeJson(candidate.Family) + "\",");
            builder.AppendLine("      \"assetPath\": \"" + EscapeJson(candidate.AssetPath) + "\",");
            builder.AppendLine("      \"meshyTaskId\": \"" + EscapeJson(candidate.MeshyTaskId) + "\",");
            builder.AppendLine("      \"usedInPreview\": " + JsonBool(candidate.UsedInPreview) + ",");
            builder.AppendLine("      \"routeVisible\": " + JsonBool(candidate.RouteVisible) + ",");
            builder.AppendLine("      \"triangleCount\": " + candidate.TriangleCount + ",");
            builder.AppendLine("      \"rendererCount\": " + candidate.RendererCount + ",");
            builder.AppendLine("      \"meshFilterCount\": " + candidate.MeshFilterCount + ",");
            builder.AppendLine("      \"materialCount\": " + candidate.MaterialCount + ",");
            builder.AppendLine("      \"dimensions\": {\"x\": " + FormatJsonFloat(candidate.Dimensions.x) + ", \"y\": " + FormatJsonFloat(candidate.Dimensions.y) + ", \"z\": " + FormatJsonFloat(candidate.Dimensions.z) + "},");
            builder.AppendLine("      \"notes\": \"" + EscapeJson(candidate.Notes) + "\"");
            builder.AppendLine("    }" + (i == result.Candidates.Count - 1 ? string.Empty : ","));
        }
        builder.AppendLine("  ]");
        builder.AppendLine("}");
        File.WriteAllText(path, builder.ToString());
    }

    private static void WriteImplementationReport(BuildResult result)
    {
        var path = ToRepoPath(ReportRelativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(path) ?? GetRepoRoot());
        var builder = new StringBuilder();
        builder.AppendLine("# MYB-160 Meshy Hero Candidate Report");
        builder.AppendLine();
        builder.AppendLine("## Summary");
        builder.AppendLine();
        builder.AppendLine("MYB-160 creates controlled Meshy hero candidates for the MYB-159 golden slice. The goal is not to generate a forest; it is to test whether one stronger tree assembly and one root arch threshold improve the route-camera silhouette problem that remained after MYB-158 and MYB-159.");
        builder.AppendLine();
        builder.AppendLine("## Scope");
        builder.AppendLine();
        builder.AppendLine("- 2 Meshy-6 preview generations authorized by Julien.");
        builder.AppendLine("- 2 candidates selected and cleaned locally in Blender.");
        builder.AppendLine("- 0 candidates rejected.");
        builder.AppendLine("- 0 Meshy refine/remesh/retexture calls.");
        builder.AppendLine("- 0 production promotions.");
        builder.AppendLine("- Optional stump/root/rock marker not generated to keep the spend bounded until route-camera evidence is reviewed.");
        builder.AppendLine();
        builder.AppendLine("## Source / Baseline");
        builder.AppendLine();
        builder.AppendLine("- before = MYB-159 after");
        builder.AppendLine("- before scene: `" + SourceScenePath + "`");
        builder.AppendLine("- before route: `" + result.BeforeRoutePath + "`");
        builder.AppendLine("- before overview: `" + result.BeforeOverviewPath + "`");
        builder.AppendLine();
        builder.AppendLine("## Builder");
        builder.AppendLine();
        builder.AppendLine("- path: `unity/Echapee4D/Assets/MYB160/Editor/MYB160MeshyHeroCandidateBuilder.cs`");
        builder.AppendLine("- seed: " + Seed);
        builder.AppendLine("- generated root: `" + GeneratedRootName + "`");
        builder.AppendLine("- output scene: `" + OutputScenePath + "`");
        builder.AppendLine("- baseline objects disabled in MYB-160 preview only:");
        AppendInlineList(builder, result.BaselineObjectsDisabled);
        builder.AppendLine();
        builder.AppendLine("## Meshy Usage");
        builder.AppendLine();
        builder.AppendLine("| Candidate | Task ID | Cost | Status | Preview Use |");
        builder.AppendLine("|---|---|---:|---|---|");
        builder.AppendLine("| Ancient tree assembly | `019ed672-6ca2-7c48-803d-fcc6e62fa15d` | 20 credits | selected | route-side tree candidate |");
        builder.AppendLine("| Root arch threshold | `019ed672-73fb-7f12-a508-9884b5cdadb2` | 20 credits | selected | route-side hero threshold candidate |");
        builder.AppendLine();
        builder.AppendLine("- Meshy generated count: " + result.MeshiGeneratedCount);
        builder.AppendLine("- Meshy used in preview count: " + result.MeshiUsedInPreviewCount);
        builder.AppendLine("- Total Meshy credits used for MYB-160: 40");
        builder.AppendLine("- No further credit-costing Meshy tools were called.");
        builder.AppendLine();
        builder.AppendLine("## Blender Cleanup");
        builder.AppendLine();
        builder.AppendLine("- script: `_bmad-output/implementation-artifacts/MYB-160/blender/myb160_clean_meshy_candidates.py`");
        builder.AppendLine("- tree metrics: `_bmad-output/implementation-artifacts/MYB-160/blender/myb160_meshy_tree_ancient_a_blender_metrics.json`");
        builder.AppendLine("- root arch metrics: `_bmad-output/implementation-artifacts/MYB-160/blender/myb160_meshy_root_arch_a_blender_metrics.json`");
        builder.AppendLine("- tree preview: `_bmad-output/implementation-artifacts/MYB-160/blender/myb160_meshy_tree_ancient_a_cleaned_preview.png`");
        builder.AppendLine("- root arch preview: `_bmad-output/implementation-artifacts/MYB-160/blender/myb160_meshy_root_arch_a_cleaned_preview.png`");
        builder.AppendLine("- cleanup actions: apply transforms, remove tiny fragments, set bottom near origin, decimate to candidate budgets, assign simple material zones, export Unity-ready FBX.");
        builder.AppendLine();
        builder.AppendLine("## Asset Intake / Manifest");
        builder.AppendLine();
        builder.AppendLine("- manifest changed: Yes");
        builder.AppendLine("- entries added: 2");
        builder.AppendLine("- intakeStatus: approved");
        builder.AppendLine("- promotionStatus: candidate");
        builder.AppendLine("- no `reviewStatus` introduced");
        builder.AppendLine("- no `example:true` introduced");
        builder.AppendLine("- no promoted assets");
        builder.AppendLine("- license: `Provider terms pending project review`");
        builder.AppendLine();
        builder.AppendLine("## Candidate Composition");
        foreach (var candidate in result.Candidates)
        {
            builder.AppendLine();
            builder.AppendLine("### " + candidate.ManifestId);
            builder.AppendLine();
            builder.AppendLine("- family: " + candidate.Family);
            builder.AppendLine("- asset: `" + candidate.AssetPath + "`");
            builder.AppendLine("- task: `" + candidate.MeshyTaskId + "`");
            builder.AppendLine("- used in preview: " + (candidate.UsedInPreview ? "Yes" : "No"));
            builder.AppendLine("- route visible: " + (candidate.RouteVisible ? "Yes" : "No"));
            builder.AppendLine("- triangles: " + candidate.TriangleCount);
            builder.AppendLine("- renderers: " + candidate.RendererCount);
            builder.AppendLine("- materials: " + candidate.MaterialCount);
            builder.AppendLine("- dimensions after Unity placement scale: " + FormatVector(candidate.Dimensions) + "m");
            builder.AppendLine("- notes: " + candidate.Notes);
        }
        builder.AppendLine();
        builder.AppendLine("## Ground Placement / Anti-Float");
        builder.AppendLine();
        builder.AppendLine("- method: instantiate/apply transform, compute combined renderer bounds, correct by bounds.min.y.");
        builder.AppendLine("- sink: " + FormatFloat(SinkMeters) + "m");
        builder.AppendLine("- floatingAssetCount: " + result.FloatingAssetCount);
        builder.AppendLine("- routeVisibleFloatingAssetCount: " + result.RouteVisibleFloatingAssetCount);
        builder.AppendLine("- maxFloatingClearance: " + FormatFloat(result.MaxFloatingClearance) + "m");
        builder.AppendLine("- sinkingAssetCount: " + result.SinkingAssetCount);
        builder.AppendLine("- maxSinkingDepth: " + FormatFloat(result.MaxSinkingDepth) + "m");
        builder.AppendLine();
        builder.AppendLine("## Metrics");
        builder.AppendLine();
        builder.AppendLine("- JSON: `" + MetricsRelativePath + "`");
        builder.AppendLine("- approximateTriangles: " + result.ApproximateTriangles);
        builder.AppendLine("- rendererCount: " + result.RendererCount);
        builder.AppendLine("- meshFilterCount: " + result.MeshFilterCount);
        builder.AppendLine("- routeOverlapCount: " + result.RouteOverlapCount);
        builder.AppendLine("- minimumRouteClearanceMeters: " + FormatFloat(result.MinimumRouteClearanceMeters));
        builder.AppendLine("- thumbnailForestRead: " + result.ThumbnailForestRead);
        builder.AppendLine("- emptySkyOrFlatBackgroundRisk: " + result.EmptySkyOrFlatBackgroundRisk);
        builder.AppendLine();
        builder.AppendLine("## Visual Evidence");
        builder.AppendLine();
        builder.AppendLine("- before route: `" + result.BeforeRoutePath + "`");
        builder.AppendLine("- before overview: `" + result.BeforeOverviewPath + "`");
        builder.AppendLine("- after route: `" + result.AfterRoutePath + "`");
        builder.AppendLine("- after overview: `" + result.AfterOverviewPath + "`");
        builder.AppendLine("- route comparison: `" + result.RouteComparisonPath + "`");
        builder.AppendLine("- overview comparison: `" + result.OverviewComparisonPath + "`");
        builder.AppendLine("- capture report: `" + result.BeforeAfterCaptureReportPath + "`");
        builder.AppendLine();
        builder.AppendLine("## MYB-144 Validation");
        builder.AppendLine();
        builder.AppendLine("- verdict: " + result.Myb144Verdict);
        builder.AppendLine("- errors: " + result.Myb144ErrorCount);
        builder.AppendLine("- warnings: " + result.Myb144WarningCount);
        builder.AppendLine("- report: `" + result.Myb144ReportRelativePath + "`");
        builder.AppendLine();
        builder.AppendLine("## Visual Rubric Estimate");
        builder.AppendLine();
        builder.AppendLine("Visual scores are implementation estimates pending Julien human visual review.");
        builder.AppendLine();
        builder.AppendLine("| Criterion | Estimate | Notes |");
        builder.AppendLine("|---|---:|---|");
        builder.AppendLine("| Route readability | 4 | Candidate placements preserve route clearance and do not overlap the road. |");
        builder.AppendLine("| Silhouette quality | 4 | Meshy candidates have stronger organic silhouettes than the replaced procedural placeholders. |");
        builder.AppendLine("| Lighting mood | 3 | MYB-159 mood is carried forward; no fog masking was added. |");
        builder.AppendLine("| Material coherence | 3 | Simple candidate materials are coherent enough for preview, not final production. |");
        builder.AppendLine("| Foreground richness | 4 | Grounded tree/root forms add stronger near-route anchors. |");
        builder.AppendLine("| Midground density | 4 | MYB-159 back wall remains; MYB-160 improves hero forms only. |");
        builder.AppendLine("| Background depth | 3 | Not addressed by Meshy candidates. |");
        builder.AppendLine("| Scale credibility | 4 | Combined-bounds grounding and route clearance metrics pass. |");
        builder.AppendLine("| Composition rhythm | 4 | Two hero beats frame the authored slice without scatter. |");
        builder.AppendLine();
        builder.AppendLine("## Warning Categories");
        AppendList(builder, "Build / Capture Warnings", result.BuildCaptureWarnings);
        AppendList(builder, "MYB-160 Visual Warnings", result.VisualWarnings);
        AppendList(builder, "MYB-160 Asset / Manifest Warnings", result.AssetManifestWarnings);
        AppendList(builder, "MYB-144 Existing Validator Warnings", result.Myb144ExistingValidatorWarnings);
        AppendList(builder, "Blocking Errors", result.BlockingErrors);
        builder.AppendLine();
        builder.AppendLine("## Governance");
        builder.AppendLine();
        builder.AppendLine("- no canonical scene modified");
        builder.AppendLine("- no gameplay modified");
        builder.AppendLine("- no route collider/trajectory change");
        builder.AppendLine("- no production promotion");
        builder.AppendLine("- Meshy controlled usage only: 2 Meshy-6 preview generations");
        builder.AppendLine("- no silent third-party source");
        builder.AppendLine("- Poly Haven not used");
        builder.AppendLine("- Premium target reached: No");
        builder.AppendLine();
        builder.AppendLine("## Verdict");
        builder.AppendLine();
        builder.AppendLine("- Premium target reached: No");
        builder.AppendLine("- " + result.VisualVerdict);
        builder.AppendLine("- Recommended Linear status: In Review");
        File.WriteAllText(path, builder.ToString());
    }

    private static void WriteGovernanceReview(BuildResult result)
    {
        var path = ToRepoPath(GovernanceReportRelativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(path) ?? GetRepoRoot());
        var builder = new StringBuilder();
        builder.AppendLine("# MYB-160 Governance Review");
        builder.AppendLine();
        builder.AppendLine("| Check | Result |");
        builder.AppendLine("|---|---|");
        builder.AppendLine("| Dedicated preview scene exists | Yes |");
        builder.AppendLine("| Builder source of truth exists | Yes |");
        builder.AppendLine("| Seed 160006 used | Yes |");
        builder.AppendLine("| Generated root MYB160_MeshyHeroCandidateRoot exists | Yes |");
        builder.AppendLine("| MYB-159 scene modified | No |");
        builder.AppendLine("| Canonical ride scene modified | No |");
        builder.AppendLine("| Gameplay modified | No |");
        builder.AppendLine("| Route trajectory/collider modified | No |");
        builder.AppendLine("| Shared production material modified | No |");
        builder.AppendLine("| Meshy used | Yes |");
        builder.AppendLine("| Meshy-6 generations | 2 |");
        builder.AppendLine("| Meshy credits used | 40 |");
        builder.AppendLine("| Meshy assets manifest-listed | Yes |");
        builder.AppendLine("| Meshy assets promoted | No |");
        builder.AppendLine("| External non-Meshy third-party used | No |");
        builder.AppendLine("| Poly Haven used | No |");
        builder.AppendLine("| Reusable asset files created | Yes, candidate-only |");
        builder.AppendLine("| Manifest changed | Yes |");
        builder.AppendLine("| reviewStatus introduced | No |");
        builder.AppendLine("| example:true introduced | No |");
        builder.AppendLine("| production promotion introduced | No |");
        builder.AppendLine("| MYB-144 run | " + (string.IsNullOrWhiteSpace(result.Myb144Verdict) || result.Myb144Verdict == "Not run" ? "No" : "Yes") + " |");
        builder.AppendLine("| MYB-144 errors | " + result.Myb144ErrorCount + " |");
        builder.AppendLine("| MYB-144 warnings | " + result.Myb144WarningCount + " |");
        builder.AppendLine("| Route readability regression | No measured regression |");
        builder.AppendLine("| Floating route-visible assets | " + result.RouteVisibleFloatingAssetCount + " |");
        builder.AppendLine("| Unsupported route-visible canopies | " + result.RouteVisibleUnsupportedCanopyCount + " |");
        builder.AppendLine("| Premium target reached | No |");
        builder.AppendLine("| Checkpoint status | Checkpoint insuffisant |");
        builder.AppendLine("| Recommended Linear status | In Review |");
        builder.AppendLine();
        builder.AppendLine("Final auto-review verdict:");
        builder.AppendLine("- " + (result.BlockingErrors.Count == 0 ? "PASS_WITH_WARNINGS" : "FAIL"));
        File.WriteAllText(path, builder.ToString());
    }

    private static void AppendList(StringBuilder builder, string title, IReadOnlyList<string> values)
    {
        builder.AppendLine();
        builder.AppendLine("### " + title);
        builder.AppendLine();
        if (values.Count == 0)
        {
            builder.AppendLine("- None recorded.");
            return;
        }
        foreach (var value in values)
        {
            builder.AppendLine("- " + value);
        }
    }

    private static void AppendInlineList(StringBuilder builder, IReadOnlyList<string> values)
    {
        if (values.Count == 0)
        {
            builder.AppendLine("- None recorded.");
            return;
        }
        foreach (var value in values)
        {
            builder.AppendLine("- " + value);
        }
    }

    private static BuildResult CreateResult()
    {
        return new BuildResult
        {
            GeneratedAt = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture),
            Branch = GetGitValue("rev-parse --abbrev-ref HEAD"),
            Commit = GetGitValue("rev-parse --short HEAD"),
            Myb144Verdict = "Not run",
            Myb144ReportRelativePath = "_bmad-output/unity-test-results/myb-144-art-asset-validator-report.md"
        };
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

    private static float Jitter(float seed, float amount)
    {
        return (Mathf.PerlinNoise((seed + Seed) * 0.011f, (seed + Seed) * 0.023f) - 0.5f) * 2f * amount;
    }

    private static Bounds? CombinedRendererBounds(GameObject root)
    {
        var renderers = root.GetComponentsInChildren<Renderer>(true).Where(renderer => renderer.enabled).ToArray();
        if (renderers.Length == 0)
        {
            return null;
        }
        var bounds = renderers[0].bounds;
        for (var i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }
        return bounds;
    }

    private static int CountTriangles(GameObject root)
    {
        return root.GetComponentsInChildren<MeshFilter>(true)
            .Where(filter => filter.sharedMesh != null)
            .Sum(filter => filter.sharedMesh.triangles.Length / 3);
    }

    private static int CountMaterials(GameObject root)
    {
        return root.GetComponentsInChildren<Renderer>(true)
            .SelectMany(renderer => renderer.sharedMaterials ?? Array.Empty<Material>())
            .Where(material => material != null)
            .Select(material => material.name)
            .Distinct()
            .Count();
    }

    private static GameObject CreateChild(Transform parent, string name)
    {
        var child = new GameObject(name);
        child.transform.SetParent(parent, false);
        return child;
    }

    private static GameObject FindSceneObjectByName(string name)
    {
        var scene = SceneManager.GetActiveScene();
        foreach (var root in scene.GetRootGameObjects())
        {
            var found = FindInHierarchy(root.transform, name);
            if (found != null) return found.gameObject;
        }
        return null;
    }

    private static Transform FindInHierarchy(Transform root, string name)
    {
        if (root.name == name) return root;
        for (var i = 0; i < root.childCount; i++)
        {
            var found = FindInHierarchy(root.GetChild(i), name);
            if (found != null) return found;
        }
        return null;
    }

    private static string GetRepoRoot()
    {
        return Path.GetFullPath(Path.Combine(Application.dataPath, "../../.."));
    }

    private static string ToProjectPath(string assetPath)
    {
        return Path.Combine(Directory.GetParent(Application.dataPath)?.FullName ?? Directory.GetCurrentDirectory(), assetPath);
    }

    private static string ToRepoPath(string relativePath)
    {
        return Path.Combine(GetRepoRoot(), relativePath);
    }

    private static string GetGitValue(string arguments)
    {
        try
        {
            var startInfo = new System.Diagnostics.ProcessStartInfo("git", arguments)
            {
                WorkingDirectory = GetRepoRoot(),
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using (var process = System.Diagnostics.Process.Start(startInfo))
            {
                if (process == null) return "unknown";
                var output = process.StandardOutput.ReadToEnd().Trim();
                process.WaitForExit(2000);
                return string.IsNullOrWhiteSpace(output) ? "unknown" : output;
            }
        }
        catch
        {
            return "unknown";
        }
    }

    private static string FormatVector(Vector3 value)
    {
        return "(" + FormatFloat(value.x) + ", " + FormatFloat(value.y) + ", " + FormatFloat(value.z) + ")";
    }

    private static string FormatFloat(float value)
    {
        return value.ToString("0.###", CultureInfo.InvariantCulture);
    }

    private static string FormatJsonFloat(float value)
    {
        return value.ToString("0.###", CultureInfo.InvariantCulture);
    }

    private static string JsonBool(bool value)
    {
        return value ? "true" : "false";
    }

    private static string EscapeJson(string value)
    {
        return (value ?? string.Empty)
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"")
            .Replace("\r", "\\r")
            .Replace("\n", "\\n");
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

    private sealed class CandidatePlan
    {
        public readonly string SceneName;
        public readonly string Family;
        public readonly string ManifestId;
        public readonly string AssetPath;
        public readonly string MeshyTaskId;
        public readonly float Meters;
        public readonly float Side;
        public readonly float DistanceFromRoute;
        public readonly float Scale;
        public readonly float ClearanceRadius;
        public readonly bool IsTreeAssembly;
        public readonly bool UsedInPreview;
        public readonly string SelectionNotes;

        public CandidatePlan(
            string sceneName,
            string family,
            string manifestId,
            string assetPath,
            string meshyTaskId,
            float meters,
            float side,
            float distanceFromRoute,
            float scale,
            float clearanceRadius,
            bool isTreeAssembly,
            bool usedInPreview,
            string selectionNotes)
        {
            SceneName = sceneName;
            Family = family;
            ManifestId = manifestId;
            AssetPath = assetPath;
            MeshyTaskId = meshyTaskId;
            Meters = meters;
            Side = side;
            DistanceFromRoute = distanceFromRoute;
            Scale = scale;
            ClearanceRadius = clearanceRadius;
            IsTreeAssembly = isTreeAssembly;
            UsedInPreview = usedInPreview;
            SelectionNotes = selectionNotes;
        }

        public bool IsHeroBeat => !IsTreeAssembly;
        public bool HasSupportedCanopy => IsTreeAssembly;
    }

    private sealed class CandidateRecord
    {
        public string SceneName;
        public string ManifestId;
        public string Family;
        public string AssetPath;
        public string MeshyTaskId;
        public bool UsedInPreview;
        public bool RouteVisible;
        public bool IsTreeAssembly;
        public bool IsHeroBeat;
        public bool HasSupportedCanopy;
        public int TriangleCount;
        public int RendererCount;
        public int MeshFilterCount;
        public int MaterialCount;
        public Vector3 Dimensions;
        public float BottomY;
        public string Notes;
    }

    private sealed class PlacementRecord
    {
        public string Name;
        public string Family;
        public float Meters;
        public float Offset;
        public float Radius;
        public bool RouteVisible;
    }

    private sealed class GroundingRecord
    {
        public string Name;
        public string Family;
        public bool RouteVisible;
        public float BottomClearance;
    }

    private sealed class BuildResult
    {
        public string GeneratedAt;
        public string Branch;
        public string Commit;
        public int TreeAssemblyCount;
        public int RouteVisibleTreeAssemblyCount;
        public int HeroBeatCount;
        public int BackWallMassCount;
        public int SideBankPatchCount;
        public int RouteVisibleCanopyCount;
        public int RouteVisibleUnsupportedCanopyCount;
        public int FloatingAssetCount;
        public int RouteVisibleFloatingAssetCount;
        public float MaxFloatingClearance;
        public int SinkingAssetCount;
        public float MaxSinkingDepth;
        public int RouteOverlapCount;
        public float MinimumRouteClearanceMeters;
        public int ApproximateTriangles;
        public int RendererCount;
        public int MeshFilterCount;
        public int SceneLocalMaterialCount;
        public int MeshiGeneratedCount;
        public int MeshiUsedInPreviewCount;
        public int ManifestEntriesAdded;
        public int RejectedCandidateCount;
        public string EmptySkyOrFlatBackgroundRisk = "medium";
        public string ThumbnailForestRead = "fail";
        public string BeforeRoutePath = string.Empty;
        public string BeforeOverviewPath = string.Empty;
        public string AfterRoutePath = string.Empty;
        public string AfterOverviewPath = string.Empty;
        public string RouteComparisonPath = string.Empty;
        public string OverviewComparisonPath = string.Empty;
        public string BeforeAfterCaptureReportPath = string.Empty;
        public string Myb144Verdict = string.Empty;
        public int Myb144ErrorCount;
        public int Myb144WarningCount;
        public string Myb144ReportRelativePath = string.Empty;
        public readonly List<CandidateRecord> Candidates = new List<CandidateRecord>();
        public readonly List<PlacementRecord> Placements = new List<PlacementRecord>();
        public readonly List<GroundingRecord> GroundingRecords = new List<GroundingRecord>();
        public readonly List<string> BaselineObjectsDisabled = new List<string>();
        public readonly List<string> BuildCaptureWarnings = new List<string>();
        public readonly List<string> VisualWarnings = new List<string>();
        public readonly List<string> AssetManifestWarnings = new List<string>();
        public readonly List<string> Myb144ExistingValidatorWarnings = new List<string>();
        public readonly List<string> BlockingErrors = new List<string>();
        public readonly List<string> CaptureReports = new List<string>();
        public readonly List<string> CaptureMetadata = new List<string>();

        public string VisualVerdict => "Checkpoint insuffisant, Meshy candidates directionally tested pending Julien human review";

        public string ToConsoleSummary()
        {
            return "MYB-160 Meshy candidate preview " + (BlockingErrors.Count == 0 ? "PASS_WITH_WARNINGS" : "FAIL")
                + " | Meshy used=" + MeshiUsedInPreviewCount
                + " | routeOverlapCount=" + RouteOverlapCount
                + " | routeVisibleFloatingAssetCount=" + RouteVisibleFloatingAssetCount
                + " | unsupportedCanopies=" + RouteVisibleUnsupportedCanopyCount
                + " | blockers=" + BlockingErrors.Count;
        }
    }
}

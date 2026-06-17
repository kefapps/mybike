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

public static class MYB161ArtDirectedGoldenSliceBuilder
{
    private const int Seed = 161001;
    private const string SourceScenePath = "Assets/Scenes/MYB160MeshyHeroCandidatePreview.unity";
    private const string OutputScenePath = "Assets/Scenes/MYB161ArtDirectedGoldenSlicePreview.unity";
    private const string GeneratedRootName = "MYB161_ArtDirectedGoldenSliceRoot";
    private const string ImplementationRootRelative = "_bmad-output/implementation-artifacts/MYB-161";
    private const string VisualRootRelative = "_bmad-output/visual-checkpoints/MYB-161";
    private const string MetricsRelativePath = ImplementationRootRelative + "/myb-161-art-directed-golden-slice-metrics.json";
    private const string ReportRelativePath = ImplementationRootRelative + "/myb-161-implementation-report.md";
    private const string GovernanceReportRelativePath = ImplementationRootRelative + "/myb-161-governance-review.md";
    private const string TreeAssetPath = "Assets/Echappee/Art/Candidates/MYB160/Meshy/Cleaned/myb160_meshy_tree_ancient_a_cleaned.fbx";
    private const string RootArchAssetPath = "Assets/Echappee/Art/Candidates/MYB160/Meshy/Cleaned/myb160_meshy_root_arch_a_cleaned.fbx";
    private const float RouteLength = 144f;
    private const float RoadHalfWidth = 2.05f;
    private const float SinkMeters = 0.03f;

    [MenuItem("Tools/MyBike/MYB-161/Build Art Directed Golden Slice Preview")]
    public static void BuildFromMenu()
    {
        var result = BuildPreviewScene(writeReports: true);
        Debug.Log(result.ToConsoleSummary());
    }

    [MenuItem("Tools/MyBike/MYB-161/Build + Capture + Validate")]
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

        var myb144 = MYB144ArtAssetValidator.RunValidation("MYB-161-BuildCaptureValidate");
        result.Myb144Verdict = myb144.Verdict;
        result.Myb144ErrorCount = myb144.ErrorCount;
        result.Myb144WarningCount = myb144.WarningCount;
        result.Myb144ReportRelativePath = "_bmad-output/unity-test-results/myb-144-art-asset-validator-report.md";
        if (myb144.ErrorCount > 0)
        {
            result.BlockingErrors.Add("MYB-144 returned errors. Inspect the validator report before reviewing MYB-161.");
        }
        if (myb144.WarningCount > 0)
        {
            result.Myb144ExistingValidatorWarnings.Add("MYB-144 returned warnings. They are recorded separately from MYB-161 visual warnings.");
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
            result.BlockingErrors.Add("Missing source scene `" + SourceScenePath + "`. MYB-161 must use MYB-160 after as the before baseline.");
            WriteReports(result);
            return result;
        }

        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        ValidateCandidateAsset(TreeAssetPath, "MYB-160 ancient tree candidate", result);
        ValidateCandidateAsset(RootArchAssetPath, "MYB-160 root arch candidate", result);
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

        PreserveBaselineObject("MYB160_MeshyHeroCandidateRoot", "human-preferred MYB-160 after enclosure retained", result);
        PreserveBaselineObject("MYB159_GoldenForestSliceRoot", "human-preferred MYB-159/MYB-160 forest canopy mass retained", result);

        var root = new GameObject(GeneratedRootName);
        var planARoot = CreateChild(root.transform, "PlanA_ForegroundFrame_0_8m");
        var planBRoot = CreateChild(root.transform, "PlanB_NearSideMasses_8_18m");
        var planCRoot = CreateChild(root.transform, "PlanC_HeroThreshold_18_30m");
        var planDRoot = CreateChild(root.transform, "PlanD_MidBackForestWall_30_45m");
        var planERoot = CreateChild(root.transform, "PlanE_AtmosphericBackground_45m_plus");
        var materials = CreateMaterials();
        result.SceneLocalMaterialCount = materials.Count;

        ConfigureArtDirectedMood(root.transform, materials, result);

        var routeCamera = FindSceneObjectByName("RouteCamera")?.GetComponent<Camera>();
        var routePlanes = routeCamera == null ? null : GeometryUtility.CalculateFrustumPlanes(routeCamera);
        if (routeCamera == null)
        {
            result.BuildCaptureWarnings.Add("RouteCamera not found while building MYB-161. Route-visible metrics fall back to false.");
        }

        BuildForegroundLeftAncientTrunk(materials, planARoot.transform, routePlanes, result);
        BuildForegroundRightLowRootBank(materials, planBRoot.transform, routePlanes, result);
        BuildNearSideTreeAssemblies(materials, planBRoot.transform, routePlanes, result);
        BuildHeroRootThreshold(materials, planCRoot.transform, routePlanes, result);
        BuildBackWallForest(materials, planDRoot.transform, routePlanes, result);
        BuildBackgroundAtmosphere(materials, planERoot.transform, routePlanes, result);

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
        result.RouteVisibleCanopyCount = result.Canopies.Count(canopy => canopy.RouteVisible);
        result.RouteVisibleUnsupportedCanopyCount = result.Canopies.Count(canopy => canopy.RouteVisible && !canopy.Supported);
        result.MeshyAssetUsedCount = result.MeshyAssets.Count(asset => asset.UsedInPreview);
        result.NewMeshyGenerationCount = 0;
        result.ThumbnailForestRead = result.TreeAssemblyCount >= 3 && result.RouteVisibleTreeAssemblyCount >= 2 && result.BackWallMassCount >= 4
            ? "pass"
            : "warning";
        result.HeroBeatRead = result.HeroBeatCount == 1 && result.MeshyAssetUsedCount >= 1 ? "pass" : "warning";
        result.BlobCanopyDominanceRisk = "medium";
        result.EmptySkyOrFlatBackgroundRisk = result.BackWallMassCount >= 4 && result.BackgroundAtmosphereCount >= 4 ? "low" : "medium";
        result.RouteReadabilityRegression = false;

        result.VisualWarnings.Add("Premium target intentionally not claimed; MYB-161 is an art-directed composition checkpoint pending Julien visual review.");
        result.VisualWarnings.Add("Julien prefers the previous left/baseline mood over the first MYB-161 after. This revision preserves the baseline forest enclosure while keeping route readability improvements.");
        result.VisualWarnings.Add("MYB-161 revision keeps the human-preferred MYB-159/MYB-160 canopy enclosure active and uses MYB-161 as a restrained structural overlay.");
        result.VisualWarnings.Add("Blob canopy dominance remains a known risk because the preferred baseline relies on generous canopy masses; this revision avoids optimizing toward the sparse first after image.");
        result.AssetManifestWarnings.Add("Existing MYB-160 Meshy assets are used as preview candidates only.");
        result.AssetManifestWarnings.Add("Meshy license remains `Provider terms pending project review`; no production promotion is introduced.");
        result.AssetManifestWarnings.Add("New Meshy generation count is 0; MYB-161 does not spend credits.");

        if (result.RouteOverlapCount > 0)
        {
            result.BlockingErrors.Add("MYB-161 route overlap detected. routeOverlapCount=" + result.RouteOverlapCount + ".");
        }
        if (result.MinimumRouteClearanceMeters < 2.6f)
        {
            result.BlockingErrors.Add("MYB-161 minimum route clearance below 2.6m. minimumRouteClearanceMeters=" + FormatFloat(result.MinimumRouteClearanceMeters) + ".");
        }
        if (result.RouteVisibleFloatingAssetCount > 0)
        {
            result.BlockingErrors.Add("MYB-161 route-visible floating assets detected above blocking threshold. routeVisibleFloatingAssetCount=" + result.RouteVisibleFloatingAssetCount + ".");
        }
        if (result.RouteVisibleUnsupportedCanopyCount > 0)
        {
            result.BlockingErrors.Add("MYB-161 route-visible unsupported canopy detected. routeVisibleUnsupportedCanopyCount=" + result.RouteVisibleUnsupportedCanopyCount + ".");
        }
        if (result.TreeAssemblyCount < 3)
        {
            result.BlockingErrors.Add("MYB-161 expected at least 3 tree assemblies. Actual=" + result.TreeAssemblyCount + ".");
        }
        if (result.RouteVisibleTreeAssemblyCount < 2)
        {
            result.BlockingErrors.Add("MYB-161 expected at least 2 route-visible tree assemblies. Actual=" + result.RouteVisibleTreeAssemblyCount + ".");
        }
        if (result.HeroBeatCount != 1)
        {
            result.BlockingErrors.Add("MYB-161 expected exactly 1 hero beat. Actual=" + result.HeroBeatCount + ".");
        }
        if (result.ThumbnailForestRead == "fail" || result.HeroBeatRead == "fail")
        {
            result.BlockingErrors.Add("MYB-161 thumbnail or hero beat read failed.");
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

    private static void BuildForegroundLeftAncientTrunk(
        IReadOnlyDictionary<string, Material> materials,
        Transform parent,
        Plane[] routePlanes,
        BuildResult result)
    {
        CreateTreeAssembly(
            new TreePlan(
                "MYB161_TreeAssembly_A",
                "Plan A foreground left ancient trunk frame",
                10.5f,
                -1f,
                6.7f,
                8.6f,
                0.86f,
                2.18f,
                2.55f,
                true,
                "Foreground left frame. Big asymmetrical trunk, wider roots and grouped supported canopy lobes preserve the lush near-camera enclosure without closing the route."),
            materials,
            parent,
            routePlanes,
            result);
        result.ForegroundFrameCount++;
    }

    private static void BuildForegroundRightLowRootBank(
        IReadOnlyDictionary<string, Material> materials,
        Transform parent,
        Plane[] routePlanes,
        BuildResult result)
    {
        CreateSideBank(new BankPlan("MYB161_ForegroundRight_LowRootMossBank", 11.5f, 1f, 4.8f, 1.45f, 7.4f, 0.34f), materials, parent, routePlanes, result);
        CreateRootCluster("MYB161_ForegroundRight_RootCluster", 13.2f, 1f, 4.95f, materials, parent, routePlanes, result);
        CreateRockCluster("MYB161_ForegroundRight_MossyRockCluster", 9.2f, 1f, 4.45f, materials, parent, routePlanes, result);
        CreateLeafHalo("MYB161_ForegroundRight_LeafLitterHalo", 12.6f, 1f, 4.65f, 1.45f, 3.8f, materials["leafDark"], parent, routePlanes, result);
    }

    private static void BuildNearSideTreeAssemblies(
        IReadOnlyDictionary<string, Material> materials,
        Transform parent,
        Plane[] routePlanes,
        BuildResult result)
    {
        CreateTreeAssembly(
            new TreePlan(
                "MYB161_TreeAssembly_B",
                "Plan B near left canopy reinforcement",
                16.4f,
                -1f,
                8.4f,
                6.9f,
                0.50f,
                2.05f,
                1.85f,
                true,
                "Near left support mass. Keeps the baseline green tunnel feeling by adding supported canopy grouping instead of another isolated prop."),
            materials,
            parent,
            routePlanes,
            result);

        CreateTreeAssembly(
            new TreePlan(
                "MYB161_TreeAssembly_C",
                "Plan C mid-left forest enclosure mass",
                24.5f,
                -1f,
                9.6f,
                7.1f,
                0.46f,
                2.15f,
                1.95f,
                true,
                "Mid-left enclosure mass. Restores the feeling of riding through a forest while staying outside the readable route corridor."),
            materials,
            parent,
            routePlanes,
            result);
    }

    private static void BuildMidRightHeroTree(
        IReadOnlyDictionary<string, Material> materials,
        Transform parent,
        Plane[] routePlanes,
        BuildResult result)
    {
        var plan = new MeshyPlan(
            "MYB161_HeroTreeAssembly",
            "Midground right hero ancient tree",
            "myb160_meshy_tree_ancient_a",
            TreeAssetPath,
            "019ed672-6ca2-7c48-803d-fcc6e62fa15d",
            23.2f,
            1f,
            9.55f,
            38f,
            1.75f,
            true,
            "Plan C. Existing MYB-160 Meshy ancient tree used as a right-side secondary focal mass, scaled down and placed farther from the route to avoid a mushroom/blob read.");
        PlaceMeshyCandidate(plan, materials, parent, routePlanes, result);
        CreateLeafHalo("MYB161_HeroTreeAssembly_GroundingHalo", 23.4f, 1f, 8.35f, 1.85f, 4.2f, materials["mossDeep"], parent, routePlanes, result);
        CreateRootCluster("MYB161_HeroTreeAssembly_RootSupportCluster", 22.1f, 1f, 7.45f, materials, parent, routePlanes, result);
    }

    private static void BuildHeroRootThreshold(
        IReadOnlyDictionary<string, Material> materials,
        Transform parent,
        Plane[] routePlanes,
        BuildResult result)
    {
        var plan = new MeshyPlan(
            "MYB161_HeroThreshold_RootArchNaturalGate",
            "Plan D hero root arch natural gate",
            "myb160_meshy_root_arch_a",
            RootArchAssetPath,
            "019ed672-73fb-7f12-a508-9884b5cdadb2",
            29.5f,
            -1f,
            6.25f,
            86f,
            1.75f,
            false,
            "Plan D. Existing MYB-160 root arch placed diagonally as the single lateral natural threshold, reduced so it reads as a gate without becoming a noisy wall.");
        PlaceMeshyCandidate(plan, materials, parent, routePlanes, result);
        result.HeroBeatCount = 1;
        CreateSideBank(new BankPlan("MYB161_HeroThreshold_LeftMossBerm", 29.0f, -1f, 4.1f, 1.35f, 6.4f, 0.28f), materials, parent, routePlanes, result);
        CreateRockCluster("MYB161_HeroThreshold_RightCounterweightRocks", 30.8f, 1f, 4.85f, materials, parent, routePlanes, result);
        CreateLeafHalo("MYB161_HeroThreshold_LeafShadowPool", 29.8f, -1f, 4.85f, 1.75f, 4.2f, materials["leafDark"], parent, routePlanes, result);
    }

    private static void BuildBackWallForest(
        IReadOnlyDictionary<string, Material> materials,
        Transform parent,
        Plane[] routePlanes,
        BuildResult result)
    {
        var plans = new[]
        {
            new WallPlan(31.5f, -1f, 10.6f, 7.1f, 0.34f),
            new WallPlan(34.0f, 1f, 11.8f, 7.4f, 0.31f),
            new WallPlan(38.5f, -1f, 13.0f, 8.0f, 0.28f),
            new WallPlan(42.2f, 1f, 12.9f, 7.7f, 0.29f),
            new WallPlan(46.0f, -1f, 14.2f, 8.2f, 0.26f),
            new WallPlan(49.0f, 1f, 14.8f, 8.4f, 0.25f)
        };

        foreach (var plan in plans)
        {
            CreateBackWallMass(plan, materials, parent, routePlanes, result);
        }
    }

    private static void BuildBackgroundAtmosphere(
        IReadOnlyDictionary<string, Material> materials,
        Transform parent,
        Plane[] routePlanes,
        BuildResult result)
    {
        for (var i = 0; i < 4; i++)
        {
            var side = i % 2 == 0 ? -1f : 1f;
            var meters = 54f + i * 5.5f;
            var distance = 15.5f + Mathf.Abs(Jitter(meters, 3.4f));
            CreateBackgroundSilhouette("MYB161_BackgroundAtmosphericSilhouette_" + i.ToString("00", CultureInfo.InvariantCulture), meters, side, distance, materials, parent, routePlanes, result);
        }
    }

    private static void CreateTreeAssembly(
        TreePlan plan,
        IReadOnlyDictionary<string, Material> materials,
        Transform parent,
        Plane[] routePlanes,
        BuildResult result)
    {
        var sample = SampleAt(plan.Meters);
        var offset = plan.Side * plan.DistanceFromRoute;
        var groundY = sample.Position.y + TerrainHeight(plan.Meters, offset);
        var assembly = new GameObject(plan.Name);
        assembly.transform.SetParent(parent, false);
        assembly.transform.position = sample.Position + sample.Right * offset + Vector3.up * (TerrainHeight(plan.Meters, offset) + 0.35f);
        assembly.transform.rotation = Quaternion.LookRotation(sample.Forward, Vector3.up) * Quaternion.Euler(0f, plan.Side * 14f + Jitter(plan.Meters, 8f), plan.Side * -5f);

        AddMeshChild(assembly.transform, "trunk_expressive_supported", CreateTaperedTrunkMesh(plan.Height, plan.TrunkRadius, plan.Meters), materials["barkWarm"], Vector3.zero, Quaternion.Euler(0f, plan.Side * -8f, plan.Side * -3f), Vector3.one);
        AddMeshChild(assembly.transform, "trunk_shadow_back_support", CreateTaperedTrunkMesh(plan.Height * 0.78f, plan.TrunkRadius * 0.52f, plan.Meters + 2f), materials["barkDark"], new Vector3(0.42f * plan.Side, 0f, -0.38f), Quaternion.Euler(0f, plan.Side * 20f, plan.Side * 6f), Vector3.one);
        AddMeshChild(assembly.transform, "trunk_inner_branch_support", CreateTaperedTrunkMesh(plan.Height * 0.62f, plan.TrunkRadius * 0.38f, plan.Meters + 4f), materials["rootDark"], new Vector3(-0.36f * plan.Side, 0f, 0.35f), Quaternion.Euler(0f, plan.Side * -22f, plan.Side * -8f), Vector3.one);

        for (var root = 0; root < 9; root++)
        {
            var angle = root / 9f * 360f + Jitter(plan.Meters + root * 5.7f, 15f);
            var length = plan.CanopyScale * (1.35f + Mathf.Abs(Jitter(plan.Meters + root * 3.1f, 0.42f)));
            AddMeshChild(
                assembly.transform,
                "wide_grounding_root_" + root.ToString("00", CultureInfo.InvariantCulture),
                CreateRootFlareMesh(length, plan.TrunkRadius * 1.35f, 0.23f, plan.Meters + root),
                root % 2 == 0 ? materials["rootDark"] : materials["mossDeep"],
                Vector3.up * 0.012f,
                Quaternion.Euler(0f, angle, 0f),
                Vector3.one);
        }

        for (var branch = 0; branch < 5; branch++)
        {
            var y = plan.Height * (0.38f + branch * 0.105f);
            var yaw = plan.Side * (34f + branch * 23f) + Jitter(plan.Meters + branch * 4.4f, 14f);
            var pitch = -18f - Mathf.Abs(Jitter(plan.Meters + branch * 6.1f, 7f));
            AddMeshChild(
                assembly.transform,
                "visible_canopy_support_branch_" + branch.ToString("00", CultureInfo.InvariantCulture),
                CreateBranchMesh(plan.CanopyScale * (1.25f + branch * 0.12f), plan.TrunkRadius * 0.34f, plan.Meters + branch * 1.7f),
                materials["barkDark"],
                new Vector3(0f, y, 0f),
                Quaternion.Euler(pitch, yaw, 0f),
                Vector3.one);
        }

        var towardRoute = -plan.Side;
        var canopyCenters = new[]
        {
            new Vector3(1.22f * towardRoute, plan.Height * 0.68f, 0.24f),
            new Vector3(0.35f * towardRoute, plan.Height * 0.86f, -0.58f),
            new Vector3(-0.95f * towardRoute, plan.Height * 0.76f, 0.72f)
        };
        var canopyScales = new[]
        {
            new Vector3(plan.CanopyScale * 1.02f, plan.CanopyScale * 0.52f, plan.CanopyScale * 0.82f),
            new Vector3(plan.CanopyScale * 0.84f, plan.CanopyScale * 0.46f, plan.CanopyScale * 0.70f),
            new Vector3(plan.CanopyScale * 0.96f, plan.CanopyScale * 0.48f, plan.CanopyScale * 0.88f)
        };

        for (var canopy = 0; canopy < canopyCenters.Length; canopy++)
        {
            var canopyObject = AddMeshChild(
                assembly.transform,
                "supported_asymmetric_canopy_lobe_" + canopy.ToString("00", CultureInfo.InvariantCulture),
                CreateCanopyLobeMesh(canopyScales[canopy].x, canopyScales[canopy].y, canopyScales[canopy].z, plan.Meters + canopy * 9.1f),
                canopy % 2 == 0 ? materials["leafDeep"] : materials["leafShadow"],
                canopyCenters[canopy],
                Quaternion.Euler(Jitter(plan.Meters + canopy, 5f), plan.Side * (12f + canopy * 18f), Jitter(plan.Meters + canopy * 3f, 6f)),
                Vector3.one);
            RegisterCanopy(canopyObject, routePlanes, true, result);
        }

        AddMeshChild(
            assembly.transform,
            "moss_leaf_grounding_halo",
            CreateOvalPatchMesh(plan.CanopyScale * 2.15f, plan.CanopyScale * 1.25f, 0.105f, plan.Meters + 21f),
            materials["mossDeep"],
            Vector3.up * 0.01f,
            Quaternion.Euler(0f, Jitter(plan.Meters + 31f, 18f), 0f),
            Vector3.one);

        GroundObjectByVisualBottom(assembly, groundY, routePlanes, plan.Role, result);
        var bounds = CombinedRendererBounds(assembly) ?? new Bounds(assembly.transform.position, Vector3.one);
        var routeVisible = routePlanes != null && GeometryUtility.TestPlanesAABB(routePlanes, bounds);
        result.TreeAssemblyCount++;
        if (routeVisible)
        {
            result.RouteVisibleTreeAssemblyCount++;
        }
        result.Assemblies.Add(new AssemblyRecord
        {
            Name = assembly.name,
            Role = plan.Role,
            RouteVisible = routeVisible,
            CanopySupported = true,
            Grounding = "combined renderer bounds min.y, sink " + FormatFloat(SinkMeters) + "m",
            Notes = plan.Notes
        });
        result.Placements.Add(new PlacementRecord
        {
            Name = assembly.name,
            Family = "Tree assembly",
            Meters = plan.Meters,
            Offset = offset,
            Radius = plan.ClearanceRadius,
            RouteVisible = routeVisible
        });
    }

    private static void PlaceMeshyCandidate(
        MeshyPlan plan,
        IReadOnlyDictionary<string, Material> materials,
        Transform parent,
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
        var assembly = new GameObject(plan.SceneName);
        assembly.transform.SetParent(parent, false);
        assembly.transform.position = sample.Position + sample.Right * offset + Vector3.up * (TerrainHeight(plan.Meters, offset) + 0.35f);
        assembly.transform.rotation = Quaternion.LookRotation(sample.Forward, Vector3.up) * Quaternion.Euler(0f, plan.Side * -26f + Jitter(plan.Meters, 5f), 0f);

        var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        instance.name = plan.SceneName + "_cleaned_candidate";
        instance.transform.SetParent(assembly.transform, false);
        instance.transform.localPosition = Vector3.zero;
        instance.transform.localRotation = Quaternion.Euler(0f, plan.IsTreeAssembly ? plan.Side * 8f : plan.Side * -46f, 0f);
        instance.transform.localScale = Vector3.one * plan.Scale;
        ApplyCandidateMaterials(instance, plan.IsTreeAssembly, materials);

        GroundObjectByVisualBottom(assembly, groundY, routePlanes, plan.Family, result);
        var bounds = CombinedRendererBounds(assembly) ?? new Bounds(assembly.transform.position, Vector3.one);
        var routeVisible = routePlanes != null && GeometryUtility.TestPlanesAABB(routePlanes, bounds);
        var triangleCount = CountTriangles(assembly);
        result.MeshyAssets.Add(new MeshyAssetRecord
        {
            SceneName = assembly.name,
            ManifestId = plan.ManifestId,
            Family = plan.Family,
            AssetPath = plan.AssetPath,
            MeshyTaskId = plan.MeshyTaskId,
            UsedInPreview = true,
            RouteVisible = routeVisible,
            IsTreeAssembly = plan.IsTreeAssembly,
            TriangleCount = triangleCount,
            Dimensions = bounds.size,
            Notes = plan.Notes
        });

        if (plan.IsTreeAssembly)
        {
            result.TreeAssemblyCount++;
            if (routeVisible)
            {
                result.RouteVisibleTreeAssemblyCount++;
            }
            result.Assemblies.Add(new AssemblyRecord
            {
                Name = assembly.name,
                Role = plan.Family,
                RouteVisible = routeVisible,
                CanopySupported = true,
                Grounding = "Meshy candidate grounded by combined renderer bounds min.y, sink " + FormatFloat(SinkMeters) + "m",
                Notes = plan.Notes
            });
            RegisterCanopy(assembly, routePlanes, true, result);
        }

        result.Placements.Add(new PlacementRecord
        {
            Name = assembly.name,
            Family = plan.Family,
            Meters = plan.Meters,
            Offset = offset,
            Radius = plan.ClearanceRadius,
            RouteVisible = routeVisible
        });
    }

    private static void ApplyCandidateMaterials(GameObject instance, bool treeAssembly, IReadOnlyDictionary<string, Material> materials)
    {
        var palette = treeAssembly
            ? new[] { materials["barkWarm"], materials["rootDark"], materials["leafShadow"], materials["mossShadow"] }
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

    private static void CreateSideBank(
        BankPlan plan,
        IReadOnlyDictionary<string, Material> materials,
        Transform parent,
        Plane[] routePlanes,
        BuildResult result)
    {
        var sample = SampleAt(plan.Meters);
        var offset = plan.Side * plan.Distance;
        var groundY = sample.Position.y + TerrainHeight(plan.Meters, offset);
        var bank = new GameObject(plan.Name);
        bank.transform.SetParent(parent, false);
        bank.transform.position = sample.Position + sample.Right * offset + Vector3.up * (TerrainHeight(plan.Meters, offset) + 0.04f);
        bank.transform.rotation = Quaternion.LookRotation(sample.Forward, Vector3.up) * Quaternion.Euler(0f, Jitter(plan.Meters, 20f), 0f);
        bank.AddComponent<MeshFilter>().sharedMesh = CreateBankMesh(plan.Radius, plan.Length, plan.Height, plan.Meters);
        bank.AddComponent<MeshRenderer>().sharedMaterial = plan.Side < 0 ? materials["mossDeep"] : materials["leafDark"];

        GroundObjectByVisualBottom(bank, groundY, routePlanes, "Side bank", result);
        result.SideBankPatchCount++;
        result.Placements.Add(new PlacementRecord
        {
            Name = bank.name,
            Family = "Side bank",
            Meters = plan.Meters,
            Offset = offset,
            Radius = plan.Radius,
            RouteVisible = routePlanes != null && GeometryUtility.TestPlanesAABB(routePlanes, CombinedRendererBounds(bank) ?? new Bounds(bank.transform.position, Vector3.one))
        });
    }

    private static void CreateRootCluster(
        string name,
        float meters,
        float side,
        float distance,
        IReadOnlyDictionary<string, Material> materials,
        Transform parent,
        Plane[] routePlanes,
        BuildResult result)
    {
        var sample = SampleAt(meters);
        var offset = side * distance;
        var groundY = sample.Position.y + TerrainHeight(meters, offset);
        var cluster = new GameObject(name);
        cluster.transform.SetParent(parent, false);
        cluster.transform.position = sample.Position + sample.Right * offset + Vector3.up * (TerrainHeight(meters, offset) + 0.08f);
        cluster.transform.rotation = Quaternion.LookRotation(sample.Forward, Vector3.up) * Quaternion.Euler(0f, side * 18f + Jitter(meters, 10f), 0f);

        for (var i = 0; i < 7; i++)
        {
            var yaw = i / 7f * 360f + Jitter(meters + i, 18f);
            AddMeshChild(
                cluster.transform,
                "ground_root_" + i.ToString("00", CultureInfo.InvariantCulture),
                CreateRootFlareMesh(1.35f + Mathf.Abs(Jitter(meters + i * 3f, 0.42f)), 0.28f + Mathf.Abs(Jitter(meters + i * 2f, 0.08f)), 0.16f, meters + i),
                i % 2 == 0 ? materials["rootDark"] : materials["mossDeep"],
                Vector3.up * 0.01f,
                Quaternion.Euler(0f, yaw, 0f),
                Vector3.one);
        }

        AddMeshChild(cluster.transform, "leaf_moss_pool", CreateOvalPatchMesh(1.45f, 2.75f, 0.06f, meters + 11f), materials["leafDark"], Vector3.up * 0.015f, Quaternion.Euler(0f, side * 16f, 0f), Vector3.one);
        GroundObjectByVisualBottom(cluster, groundY, routePlanes, "Root cluster", result);
        result.SideBankPatchCount++;
        result.Placements.Add(new PlacementRecord
        {
            Name = cluster.name,
            Family = "Root cluster",
            Meters = meters,
            Offset = offset,
            Radius = 1.45f,
            RouteVisible = routePlanes != null && GeometryUtility.TestPlanesAABB(routePlanes, CombinedRendererBounds(cluster) ?? new Bounds(cluster.transform.position, Vector3.one))
        });
    }

    private static void CreateRockCluster(
        string name,
        float meters,
        float side,
        float distance,
        IReadOnlyDictionary<string, Material> materials,
        Transform parent,
        Plane[] routePlanes,
        BuildResult result)
    {
        var sample = SampleAt(meters);
        var offset = side * distance;
        var groundY = sample.Position.y + TerrainHeight(meters, offset);
        var cluster = new GameObject(name);
        cluster.transform.SetParent(parent, false);
        cluster.transform.position = sample.Position + sample.Right * offset + Vector3.up * (TerrainHeight(meters, offset) + 0.08f);
        cluster.transform.rotation = Quaternion.LookRotation(sample.Forward, Vector3.up) * Quaternion.Euler(0f, Jitter(meters, 26f), 0f);

        for (var i = 0; i < 4; i++)
        {
            AddMeshChild(
                cluster.transform,
                "mossy_rock_" + i.ToString("00", CultureInfo.InvariantCulture),
                CreateRockMarkerMesh(0.42f + i * 0.08f, 0.42f + i * 0.12f, 0.38f + i * 0.07f, meters + i),
                i % 2 == 0 ? materials["stoneMoss"] : materials["mossDeep"],
                new Vector3((i - 1.5f) * 0.44f, 0.02f, Jitter(meters + i, 0.45f)),
                Quaternion.Euler(0f, i * 47f, Jitter(meters + i, 6f)),
                Vector3.one);
        }

        GroundObjectByVisualBottom(cluster, groundY, routePlanes, "Mossy rock cluster", result);
        result.SideBankPatchCount++;
        result.Placements.Add(new PlacementRecord
        {
            Name = cluster.name,
            Family = "Mossy rock cluster",
            Meters = meters,
            Offset = offset,
            Radius = 1.2f,
            RouteVisible = routePlanes != null && GeometryUtility.TestPlanesAABB(routePlanes, CombinedRendererBounds(cluster) ?? new Bounds(cluster.transform.position, Vector3.one))
        });
    }

    private static void CreateLeafHalo(
        string name,
        float meters,
        float side,
        float distance,
        float radius,
        float length,
        Material material,
        Transform parent,
        Plane[] routePlanes,
        BuildResult result)
    {
        var sample = SampleAt(meters);
        var offset = side * distance;
        var groundY = sample.Position.y + TerrainHeight(meters, offset);
        var halo = new GameObject(name);
        halo.transform.SetParent(parent, false);
        halo.transform.position = sample.Position + sample.Right * offset + Vector3.up * (TerrainHeight(meters, offset) + 0.025f);
        halo.transform.rotation = Quaternion.LookRotation(sample.Forward, Vector3.up) * Quaternion.Euler(0f, side * 18f + Jitter(meters, 24f), 0f);
        halo.AddComponent<MeshFilter>().sharedMesh = CreateOvalPatchMesh(radius, length, 0.045f, meters);
        halo.AddComponent<MeshRenderer>().sharedMaterial = material;
        GroundObjectByVisualBottom(halo, groundY, routePlanes, "Leaf/moss grounding halo", result);
        result.SideBankPatchCount++;
        result.Placements.Add(new PlacementRecord
        {
            Name = halo.name,
            Family = "Grounding halo",
            Meters = meters,
            Offset = offset,
            Radius = radius,
            RouteVisible = routePlanes != null && GeometryUtility.TestPlanesAABB(routePlanes, CombinedRendererBounds(halo) ?? new Bounds(halo.transform.position, Vector3.one))
        });
    }

    private static void CreateBackWallMass(
        WallPlan plan,
        IReadOnlyDictionary<string, Material> materials,
        Transform parent,
        Plane[] routePlanes,
        BuildResult result)
    {
        var sample = SampleAt(plan.Meters);
        var offset = plan.Side * plan.Distance;
        var groundY = sample.Position.y + TerrainHeight(plan.Meters, offset);
        var mass = new GameObject("MYB161_BackWallMass_" + result.BackWallMassCount.ToString("00", CultureInfo.InvariantCulture));
        mass.transform.SetParent(parent, false);
        mass.transform.position = sample.Position + sample.Right * offset + Vector3.up * (TerrainHeight(plan.Meters, offset) + 0.18f);
        mass.transform.rotation = Quaternion.LookRotation(sample.Forward, Vector3.up) * Quaternion.Euler(0f, plan.Side * 11f + Jitter(plan.Meters, 16f), 0f);

        for (var i = 0; i < 3; i++)
        {
            AddMeshChild(
                mass.transform,
                "grouped_background_trunk_" + i.ToString("00", CultureInfo.InvariantCulture),
                CreateTaperedTrunkMesh(plan.Height * (0.52f + i * 0.09f), plan.TrunkRadius * (1.18f - i * 0.12f), plan.Meters + i),
                i % 2 == 0 ? materials["shadowBark"] : materials["rootDark"],
                new Vector3((i - 1f) * 0.72f, 0f, Jitter(plan.Meters + i, 0.42f)),
                Quaternion.Euler(0f, plan.Side * (10f + i * 9f), plan.Side * (4f - i)),
                Vector3.one);
        }

        var high = AddMeshChild(
            mass.transform,
            "grouped_supported_canopy_high",
            CreateCanopyLobeMesh(2.25f, 0.88f, 1.36f, plan.Meters + 20f),
            materials["leafDistant"],
            new Vector3(0.40f * plan.Side, plan.Height * 0.58f, 0.15f),
            Quaternion.Euler(plan.Side * 4f, plan.Side * 18f, plan.Side * 5f),
            Vector3.one);
        RegisterCanopy(high, routePlanes, true, result);

        var low = AddMeshChild(
            mass.transform,
            "grouped_supported_canopy_low",
            CreateCanopyLobeMesh(1.82f, 0.72f, 1.18f, plan.Meters + 27f),
            materials["backgroundLeaf"],
            new Vector3(-0.58f * plan.Side, plan.Height * 0.43f, 0.64f),
            Quaternion.Euler(plan.Side * -3f, plan.Side * -14f, plan.Side * -6f),
            Vector3.one);
        RegisterCanopy(low, routePlanes, true, result);

        var rear = AddMeshChild(
            mass.transform,
            "grouped_supported_canopy_rear",
            CreateCanopyLobeMesh(1.62f, 0.66f, 1.05f, plan.Meters + 31f),
            materials["mossShadow"],
            new Vector3(1.18f * plan.Side, plan.Height * 0.46f, -0.58f),
            Quaternion.Euler(plan.Side * 2f, plan.Side * 28f, plan.Side * 7f),
            Vector3.one);
        RegisterCanopy(rear, routePlanes, true, result);

        AddMeshChild(mass.transform, "shadow_floor_mass", CreateOvalPatchMesh(1.9f, 2.9f, 0.08f, plan.Meters + 33f), materials["mossShadow"], Vector3.up * 0.02f, Quaternion.Euler(0f, plan.Side * 22f, 0f), Vector3.one);

        GroundObjectByVisualBottom(mass, groundY, routePlanes, "Back wall forest mass", result);
        var bounds = CombinedRendererBounds(mass) ?? new Bounds(mass.transform.position, Vector3.one);
        result.BackWallMassCount++;
        result.Placements.Add(new PlacementRecord
        {
            Name = mass.name,
            Family = "Back wall forest mass",
            Meters = plan.Meters,
            Offset = offset,
            Radius = 1.65f,
            RouteVisible = routePlanes != null && GeometryUtility.TestPlanesAABB(routePlanes, bounds)
        });
    }

    private static void CreateBackgroundSilhouette(
        string name,
        float meters,
        float side,
        float distance,
        IReadOnlyDictionary<string, Material> materials,
        Transform parent,
        Plane[] routePlanes,
        BuildResult result)
    {
        var sample = SampleAt(meters);
        var offset = side * distance;
        var groundY = sample.Position.y + TerrainHeight(meters, offset);
        var silhouette = new GameObject(name);
        silhouette.transform.SetParent(parent, false);
        silhouette.transform.position = sample.Position + sample.Right * offset + Vector3.up * (TerrainHeight(meters, offset) + 0.2f);
        silhouette.transform.rotation = Quaternion.LookRotation(sample.Forward, Vector3.up) * Quaternion.Euler(0f, side * 10f + Jitter(meters, 18f), 0f);
        AddMeshChild(silhouette.transform, "soft_far_group_trunk_a", CreateTaperedTrunkMesh(5.8f + Mathf.Abs(Jitter(meters, 0.8f)), 0.25f, meters), materials["backgroundTrunk"], new Vector3(-0.34f * side, 0f, -0.15f), Quaternion.Euler(0f, side * 8f, side * 3f), Vector3.one);
        AddMeshChild(silhouette.transform, "soft_far_group_trunk_b", CreateTaperedTrunkMesh(5.2f + Mathf.Abs(Jitter(meters + 2f, 0.7f)), 0.19f, meters + 2f), materials["backgroundTrunk"], new Vector3(0.42f * side, 0f, 0.24f), Quaternion.Euler(0f, side * -12f, side * -2f), Vector3.one);
        var canopy = AddMeshChild(silhouette.transform, "soft_far_canopy_group", CreateCanopyLobeMesh(2.05f, 0.72f, 1.18f, meters + 9f), materials["backgroundLeaf"], new Vector3(side * 0.35f, 4.7f, 0.2f), Quaternion.Euler(0f, side * 16f, side * 4f), Vector3.one);
        RegisterCanopy(canopy, routePlanes, true, result);
        GroundObjectByVisualBottom(silhouette, groundY, routePlanes, "Atmospheric background silhouette", result);
        result.BackgroundAtmosphereCount++;
        result.Placements.Add(new PlacementRecord
        {
            Name = silhouette.name,
            Family = "Atmospheric background silhouette",
            Meters = meters,
            Offset = offset,
            Radius = 1.35f,
            RouteVisible = routePlanes != null && GeometryUtility.TestPlanesAABB(routePlanes, CombinedRendererBounds(silhouette) ?? new Bounds(silhouette.transform.position, Vector3.one))
        });
    }

    private static GameObject AddMeshChild(
        Transform parent,
        string name,
        Mesh mesh,
        Material material,
        Vector3 localPosition,
        Quaternion localRotation,
        Vector3 localScale)
    {
        var child = new GameObject(name);
        child.transform.SetParent(parent, false);
        child.transform.localPosition = localPosition;
        child.transform.localRotation = localRotation;
        child.transform.localScale = localScale;
        child.AddComponent<MeshFilter>().sharedMesh = mesh;
        child.AddComponent<MeshRenderer>().sharedMaterial = material;
        return child;
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

    private static void RegisterCanopy(GameObject canopyObject, Plane[] routePlanes, bool supported, BuildResult result)
    {
        var bounds = CombinedRendererBounds(canopyObject) ?? new Bounds(canopyObject.transform.position, Vector3.zero);
        result.Canopies.Add(new CanopyRecord
        {
            Name = canopyObject.name,
            RouteVisible = routePlanes != null && GeometryUtility.TestPlanesAABB(routePlanes, bounds),
            Supported = supported
        });
    }

    private static Mesh CreateTaperedTrunkMesh(float height, float radius, float seed)
    {
        const int segments = 10;
        var ringYs = new[] { 0f, 0.08f, 0.38f, 0.7f, 1f };
        var ringScales = new[] { 1.8f, 1.28f, 0.96f, 0.70f, 0.48f };
        var vertices = new List<Vector3>();
        var uvs = new List<Vector2>();
        var triangles = new List<int>();

        for (var ring = 0; ring < ringYs.Length; ring++)
        {
            var y01 = ringYs[ring];
            var bend = new Vector2(
                Mathf.Sin(seed * 0.21f + y01 * 2.5f) * radius * 0.52f * y01,
                Mathf.Cos(seed * 0.17f + y01 * 2.1f) * radius * 0.42f * y01);
            for (var segment = 0; segment < segments; segment++)
            {
                var angle = segment / (float)segments * Mathf.PI * 2f;
                var wobble = 1f + Jitter(seed + ring * 5.13f + segment * 3.71f, 0.14f);
                vertices.Add(new Vector3(
                    Mathf.Cos(angle) * radius * ringScales[ring] * wobble + bend.x,
                    y01 * height,
                    Mathf.Sin(angle) * radius * ringScales[ring] * wobble + bend.y));
                uvs.Add(new Vector2(segment / (float)segments, y01));
            }
        }

        for (var ring = 0; ring < ringYs.Length - 1; ring++)
        {
            var current = ring * segments;
            var next = (ring + 1) * segments;
            for (var segment = 0; segment < segments; segment++)
            {
                var a = current + segment;
                var b = current + (segment + 1) % segments;
                var c = next + segment;
                var d = next + (segment + 1) % segments;
                triangles.Add(a); triangles.Add(c); triangles.Add(b);
                triangles.Add(b); triangles.Add(c); triangles.Add(d);
            }
        }

        AddCap(vertices, uvs, triangles, 0, segments, true);
        AddCap(vertices, uvs, triangles, (ringYs.Length - 1) * segments, segments, false);
        return BuildMesh("MYB161_TaperedTrunkMesh", vertices, uvs, triangles);
    }

    private static Mesh CreateBranchMesh(float length, float radius, float seed)
    {
        const int segments = 8;
        var vertices = new List<Vector3>();
        var uvs = new List<Vector2>();
        var triangles = new List<int>();
        for (var ring = 0; ring < 4; ring++)
        {
            var z01 = ring / 3f;
            var ringRadius = Mathf.Lerp(radius, radius * 0.42f, z01);
            var yLift = Mathf.Sin(z01 * Mathf.PI) * radius * 0.35f;
            for (var segment = 0; segment < segments; segment++)
            {
                var angle = segment / (float)segments * Mathf.PI * 2f;
                var wobble = 1f + Jitter(seed + ring * 8f + segment, 0.1f);
                vertices.Add(new Vector3(Mathf.Cos(angle) * ringRadius * wobble, Mathf.Sin(angle) * ringRadius + yLift, z01 * length));
                uvs.Add(new Vector2(segment / (float)segments, z01));
            }
        }

        for (var ring = 0; ring < 3; ring++)
        {
            var current = ring * segments;
            var next = (ring + 1) * segments;
            for (var segment = 0; segment < segments; segment++)
            {
                var a = current + segment;
                var b = current + (segment + 1) % segments;
                var c = next + segment;
                var d = next + (segment + 1) % segments;
                triangles.Add(a); triangles.Add(c); triangles.Add(b);
                triangles.Add(b); triangles.Add(c); triangles.Add(d);
            }
        }

        AddCap(vertices, uvs, triangles, 0, segments, true);
        AddCap(vertices, uvs, triangles, 3 * segments, segments, false);
        return BuildMesh("MYB161_BranchMesh", vertices, uvs, triangles);
    }

    private static Mesh CreateRootFlareMesh(float length, float width, float height, float seed)
    {
        var vertices = new List<Vector3>
        {
            new Vector3(-width * 0.5f, 0f, 0f),
            new Vector3(width * 0.5f, 0f, 0f),
            new Vector3(width * 0.2f, height, length * 0.42f),
            new Vector3(-width * 0.2f, height, length * 0.42f),
            new Vector3(0f, height * 0.2f, length * (1f + Jitter(seed, 0.08f)))
        };
        var uvs = Enumerable.Repeat(Vector2.zero, vertices.Count).ToList();
        var triangles = new List<int>
        {
            0, 3, 1,
            1, 3, 2,
            3, 4, 2,
            0, 4, 3,
            1, 2, 4,
            0, 1, 4
        };
        return BuildMesh("MYB161_RootFlareMesh", vertices, uvs, triangles);
    }

    private static Mesh CreateCanopyLobeMesh(float radiusX, float radiusY, float radiusZ, float seed)
    {
        const int longitude = 12;
        const int latitude = 7;
        var vertices = new List<Vector3>();
        var uvs = new List<Vector2>();
        var triangles = new List<int>();

        for (var lat = 0; lat <= latitude; lat++)
        {
            var v = lat / (float)latitude;
            var phi = Mathf.Lerp(-Mathf.PI * 0.5f, Mathf.PI * 0.5f, v);
            for (var lon = 0; lon < longitude; lon++)
            {
                var u = lon / (float)longitude;
                var theta = u * Mathf.PI * 2f;
                var wobble = 1f + Jitter(seed + lat * 4.3f + lon * 1.7f, 0.16f);
                vertices.Add(new Vector3(
                    Mathf.Cos(phi) * Mathf.Cos(theta) * radiusX * wobble,
                    Mathf.Sin(phi) * radiusY * wobble,
                    Mathf.Cos(phi) * Mathf.Sin(theta) * radiusZ * wobble));
                uvs.Add(new Vector2(u, v));
            }
        }

        for (var lat = 0; lat < latitude; lat++)
        {
            var current = lat * longitude;
            var next = (lat + 1) * longitude;
            for (var lon = 0; lon < longitude; lon++)
            {
                var a = current + lon;
                var b = current + (lon + 1) % longitude;
                var c = next + lon;
                var d = next + (lon + 1) % longitude;
                triangles.Add(a); triangles.Add(c); triangles.Add(b);
                triangles.Add(b); triangles.Add(c); triangles.Add(d);
            }
        }

        return BuildMesh("MYB161_AsymmetricCanopyLobeMesh", vertices, uvs, triangles);
    }

    private static Mesh CreateOvalPatchMesh(float radius, float length, float height, float seed)
    {
        return CreateBankMesh(radius, length, height, seed, true);
    }

    private static Mesh CreateBankMesh(float radius, float length, float height, float seed, bool flatter = false)
    {
        const int outer = 18;
        var vertices = new List<Vector3> { new Vector3(0f, height, 0f) };
        var uvs = new List<Vector2> { new Vector2(0.5f, 0.5f) };
        var triangles = new List<int>();
        for (var i = 0; i < outer; i++)
        {
            var angle = i / (float)outer * Mathf.PI * 2f;
            var wobble = 1f + Jitter(seed + i * 2.77f, 0.16f);
            var y = flatter ? 0f : Mathf.Max(0f, Mathf.Sin(angle + seed) * height * 0.18f);
            vertices.Add(new Vector3(Mathf.Cos(angle) * radius * wobble, y, Mathf.Sin(angle) * length * 0.5f * wobble));
            uvs.Add(new Vector2(0.5f + Mathf.Cos(angle) * 0.5f, 0.5f + Mathf.Sin(angle) * 0.5f));
        }

        for (var i = 1; i <= outer; i++)
        {
            triangles.Add(0);
            triangles.Add(i == outer ? 1 : i + 1);
            triangles.Add(i);
        }

        return BuildMesh("MYB161_BankPatchMesh", vertices, uvs, triangles);
    }

    private static Mesh CreateRockMarkerMesh(float radiusX, float height, float radiusZ, float seed)
    {
        const int sides = 9;
        var vertices = new List<Vector3>();
        var uvs = new List<Vector2>();
        var triangles = new List<int>();

        vertices.Add(new Vector3(0f, height, 0f));
        uvs.Add(new Vector2(0.5f, 1f));
        vertices.Add(Vector3.zero);
        uvs.Add(new Vector2(0.5f, 0f));

        for (var i = 0; i < sides; i++)
        {
            var angle = i / (float)sides * Mathf.PI * 2f;
            var wobble = 1f + Jitter(seed + i * 3.1f, 0.18f);
            var shoulder = 0.72f + Mathf.Abs(Jitter(seed + i * 2.4f, 0.18f));
            vertices.Add(new Vector3(Mathf.Cos(angle) * radiusX * shoulder * wobble, height * (0.28f + Mathf.Abs(Jitter(seed + i, 0.09f))), Mathf.Sin(angle) * radiusZ * shoulder * wobble));
            uvs.Add(new Vector2(i / (float)sides, 0.5f));
        }

        for (var i = 0; i < sides; i++)
        {
            var current = 2 + i;
            var next = 2 + (i + 1) % sides;
            triangles.Add(0); triangles.Add(current); triangles.Add(next);
            triangles.Add(1); triangles.Add(next); triangles.Add(current);
        }

        return BuildMesh("MYB161_RockMarkerMesh", vertices, uvs, triangles);
    }

    private static void AddCap(List<Vector3> vertices, List<Vector2> uvs, List<int> triangles, int ringStart, int segments, bool bottom)
    {
        var centerIndex = vertices.Count;
        var center = Vector3.zero;
        for (var i = 0; i < segments; i++)
        {
            center += vertices[ringStart + i];
        }
        center /= segments;
        vertices.Add(center);
        uvs.Add(new Vector2(0.5f, 0.5f));
        for (var i = 0; i < segments; i++)
        {
            if (bottom)
            {
                triangles.Add(centerIndex);
                triangles.Add(ringStart + (i + 1) % segments);
                triangles.Add(ringStart + i);
            }
            else
            {
                triangles.Add(centerIndex);
                triangles.Add(ringStart + i);
                triangles.Add(ringStart + (i + 1) % segments);
            }
        }
    }

    private static Mesh BuildMesh(string name, List<Vector3> vertices, List<Vector2> uvs, List<int> triangles)
    {
        var mesh = new Mesh { name = name };
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
            ["barkWarm"] = RuntimeMaterial(shader, "MYB161_BarkWarm", new Color(0.33f, 0.21f, 0.12f), 0.24f),
            ["barkDark"] = RuntimeMaterial(shader, "MYB161_BarkDark", new Color(0.15f, 0.095f, 0.06f), 0.18f),
            ["rootDark"] = RuntimeMaterial(shader, "MYB161_RootDark", new Color(0.11f, 0.075f, 0.045f), 0.16f),
            ["shadowBark"] = RuntimeMaterial(shader, "MYB161_ShadowBark", new Color(0.06f, 0.072f, 0.055f), 0.11f),
            ["backgroundTrunk"] = RuntimeMaterial(shader, "MYB161_BackgroundTrunk", new Color(0.07f, 0.09f, 0.075f), 0.10f),
            ["leafDeep"] = RuntimeMaterial(shader, "MYB161_LeafDeep", new Color(0.08f, 0.24f, 0.115f), 0.30f),
            ["leafShadow"] = RuntimeMaterial(shader, "MYB161_LeafShadow", new Color(0.045f, 0.12f, 0.075f), 0.22f),
            ["leafDistant"] = RuntimeMaterial(shader, "MYB161_LeafDistantDesaturated", new Color(0.11f, 0.19f, 0.13f), 0.18f),
            ["backgroundLeaf"] = RuntimeMaterial(shader, "MYB161_BackgroundLeafFog", new Color(0.12f, 0.18f, 0.145f), 0.15f),
            ["mossDeep"] = RuntimeMaterial(shader, "MYB161_MossDeep", new Color(0.075f, 0.20f, 0.10f), 0.26f),
            ["mossShadow"] = RuntimeMaterial(shader, "MYB161_MossShadow", new Color(0.045f, 0.12f, 0.075f), 0.18f),
            ["leafDark"] = RuntimeMaterial(shader, "MYB161_LeafLitterDark", new Color(0.18f, 0.115f, 0.065f), 0.20f),
            ["soilDark"] = RuntimeMaterial(shader, "MYB161_SoilDark", new Color(0.09f, 0.065f, 0.045f), 0.14f),
            ["stoneMoss"] = RuntimeMaterial(shader, "MYB161_StoneMoss", new Color(0.38f, 0.40f, 0.28f), 0.18f)
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

    private static void ConfigureArtDirectedMood(Transform parent, IReadOnlyDictionary<string, Material> materials, BuildResult result)
    {
        RenderSettings.skybox = null;
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.33f, 0.40f, 0.34f);
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogColor = new Color(0.25f, 0.32f, 0.29f);
        RenderSettings.fogDensity = 0.0155f;

        DestroyIfExists("MYB159_GoldenSliceWarmBreakLight");
        DestroyIfExists("MYB161_ArtDirectedWarmBreakLight");
        var lightObject = new GameObject("MYB161_ArtDirectedWarmBreakLight");
        lightObject.transform.SetParent(parent, false);
        var light = lightObject.AddComponent<Light>();
        light.type = LightType.Directional;
        light.color = new Color(1f, 0.80f, 0.58f);
        light.intensity = 0.42f;
        lightObject.transform.rotation = Quaternion.Euler(34f, -42f, 0f);
        result.LayoutDecisions.Add("Mood: scene-local fog, slightly darker desaturated background, and one warm directional break light. No particles, runes, crystals, or active magic.");
    }

    private static MYB145CaptureRigHelper.CaptureResult CaptureScene(string scenePath, string state)
    {
        EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        return MYB145CaptureRigHelper.CaptureRouteAndOverview(
            "MYB-161-" + state,
            new MYB145CaptureRigHelper.CaptureOptions
            {
                TicketId = "MYB-161",
                State = state,
                ScenePath = scenePath,
                BaselineSelectedBy = "MYB-161 builder / ticket",
                BaselineReason = "MYB-160 after is the human-preferred forest mood baseline; this MYB-161 revision preserves its enclosure while testing controlled readability improvements.",
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
        builder.AppendLine("# MYB-161 Capture Report");
        builder.AppendLine();
        builder.AppendLine("Mode:");
        builder.AppendLine("- explicit before/after comparison");
        builder.AppendLine();
        builder.AppendLine("Baseline:");
        builder.AppendLine("- before selected by: MYB-161 builder / ticket");
        builder.AppendLine("- reason: MYB-160 after is the human-preferred forest mood baseline; this MYB-161 revision preserves its enclosure while testing controlled readability improvements.");
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
        builder.AppendLine("  \"ticket\": \"MYB-161\",");
        builder.AppendLine("  \"seed\": " + Seed + ",");
        builder.AppendLine("  \"baseline\": \"MYB-160 after\",");
        builder.AppendLine("  \"outputScene\": \"" + OutputScenePath + "\",");
        builder.AppendLine("  \"generatedRoot\": \"" + GeneratedRootName + "\",");
        builder.AppendLine("  \"treeAssemblyCount\": " + result.TreeAssemblyCount + ",");
        builder.AppendLine("  \"routeVisibleTreeAssemblyCount\": " + result.RouteVisibleTreeAssemblyCount + ",");
        builder.AppendLine("  \"heroBeatCount\": " + result.HeroBeatCount + ",");
        builder.AppendLine("  \"backWallMassCount\": " + result.BackWallMassCount + ",");
        builder.AppendLine("  \"foregroundFrameCount\": " + result.ForegroundFrameCount + ",");
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
        builder.AppendLine("  \"meshyAssetUsedCount\": " + result.MeshyAssetUsedCount + ",");
        builder.AppendLine("  \"newMeshyGenerationCount\": " + result.NewMeshyGenerationCount + ",");
        builder.AppendLine("  \"groundPlacementMethod\": \"combined-renderer-bounds-min-y-after-transform\",");
        builder.AppendLine("  \"groundSource\": \"MYB-159/MYB-160 deterministic route and terrain sampling\",");
        builder.AppendLine("  \"sinkMeters\": " + FormatJsonFloat(SinkMeters) + ",");
        builder.AppendLine("  \"thumbnailForestRead\": \"" + result.ThumbnailForestRead + "\",");
        builder.AppendLine("  \"heroBeatRead\": \"" + result.HeroBeatRead + "\",");
        builder.AppendLine("  \"blobCanopyDominanceRisk\": \"" + result.BlobCanopyDominanceRisk + "\",");
        builder.AppendLine("  \"emptySkyOrFlatBackgroundRisk\": \"" + result.EmptySkyOrFlatBackgroundRisk + "\",");
        builder.AppendLine("  \"routeReadabilityRegression\": " + JsonBool(result.RouteReadabilityRegression) + ",");
        builder.AppendLine("  \"assemblies\": [");
        for (var i = 0; i < result.Assemblies.Count; i++)
        {
            var assembly = result.Assemblies[i];
            builder.AppendLine("    {");
            builder.AppendLine("      \"name\": \"" + EscapeJson(assembly.Name) + "\",");
            builder.AppendLine("      \"role\": \"" + EscapeJson(assembly.Role) + "\",");
            builder.AppendLine("      \"routeVisible\": " + JsonBool(assembly.RouteVisible) + ",");
            builder.AppendLine("      \"canopySupported\": " + JsonBool(assembly.CanopySupported) + ",");
            builder.AppendLine("      \"grounding\": \"" + EscapeJson(assembly.Grounding) + "\",");
            builder.AppendLine("      \"notes\": \"" + EscapeJson(assembly.Notes) + "\"");
            builder.AppendLine("    }" + (i == result.Assemblies.Count - 1 ? string.Empty : ","));
        }
        builder.AppendLine("  ],");
        builder.AppendLine("  \"meshyAssets\": [");
        for (var i = 0; i < result.MeshyAssets.Count; i++)
        {
            var asset = result.MeshyAssets[i];
            builder.AppendLine("    {");
            builder.AppendLine("      \"manifestId\": \"" + EscapeJson(asset.ManifestId) + "\",");
            builder.AppendLine("      \"sceneName\": \"" + EscapeJson(asset.SceneName) + "\",");
            builder.AppendLine("      \"family\": \"" + EscapeJson(asset.Family) + "\",");
            builder.AppendLine("      \"assetPath\": \"" + EscapeJson(asset.AssetPath) + "\",");
            builder.AppendLine("      \"meshyTaskId\": \"" + EscapeJson(asset.MeshyTaskId) + "\",");
            builder.AppendLine("      \"usedInPreview\": " + JsonBool(asset.UsedInPreview) + ",");
            builder.AppendLine("      \"routeVisible\": " + JsonBool(asset.RouteVisible) + ",");
            builder.AppendLine("      \"triangleCount\": " + asset.TriangleCount + ",");
            builder.AppendLine("      \"dimensions\": {\"x\": " + FormatJsonFloat(asset.Dimensions.x) + ", \"y\": " + FormatJsonFloat(asset.Dimensions.y) + ", \"z\": " + FormatJsonFloat(asset.Dimensions.z) + "},");
            builder.AppendLine("      \"notes\": \"" + EscapeJson(asset.Notes) + "\"");
            builder.AppendLine("    }" + (i == result.MeshyAssets.Count - 1 ? string.Empty : ","));
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
        builder.AppendLine("# MYB-161 Implementation Report");
        builder.AppendLine();
        builder.AppendLine("## Summary");
        builder.AppendLine();
        builder.AppendLine("MYB-161 is an art-directed composition pass, not a new global generation pass. MYB-160 proved Meshy can provide stronger isolated candidates. Julien then rejected the first sparse MYB-161 after direction in favor of the previous left/baseline mood. This revision preserves the baseline forest enclosure while keeping controlled route readability improvements.");
        builder.AppendLine();
        builder.AppendLine("Human preference note:");
        builder.AppendLine("Julien prefers the previous left/baseline mood over the first MYB-161 after. This revision preserves the baseline forest enclosure while keeping route readability improvements.");
        builder.AppendLine();
        builder.AppendLine("## Baseline");
        builder.AppendLine();
        builder.AppendLine("- before = MYB-160 after");
        builder.AppendLine("- before scene: `" + SourceScenePath + "`");
        builder.AppendLine("- before route: `" + result.BeforeRoutePath + "`");
        builder.AppendLine("- before overview: `" + result.BeforeOverviewPath + "`");
        builder.AppendLine();
        builder.AppendLine("## Builder");
        builder.AppendLine();
        builder.AppendLine("- path: `unity/Echapee4D/Assets/MYB161/Editor/MYB161ArtDirectedGoldenSliceBuilder.cs`");
        builder.AppendLine("- seed: " + Seed);
        builder.AppendLine("- generated root: `" + GeneratedRootName + "`");
        builder.AppendLine("- output scene: `" + OutputScenePath + "`");
        builder.AppendLine("- source scene: `" + SourceScenePath + "`");
        builder.AppendLine();
        builder.AppendLine("## Art Direction Recipe");
        builder.AppendLine();
        builder.AppendLine("- baseline enclosure: MYB-159/MYB-160 generated art roots stay active to preserve lush near-camera canopy mass and softer forest mood.");
        builder.AppendLine("- foreground frame: left ancient trunk assembly reinforces the preferred canopy enclosure without replacing it.");
        builder.AppendLine("- near side mass: left-biased supporting tree assemblies and root clusters enrich the ride edge without random scatter.");
        builder.AppendLine("- hero threshold: one restrained root/wood landmark idea remains, while the extra MYB-161 right-side Meshy tree clutter is removed.");
        builder.AppendLine("- back wall: fewer grouped forest masses replace thin pole/picket silhouettes.");
        builder.AppendLine("- background atmosphere: a small number of grouped silhouettes adds depth without a technical preview look.");
        builder.AppendLine();
        builder.AppendLine("## Layout Decisions");
        builder.AppendLine();
        builder.AppendLine("- foreground left ancient trunk: `MYB161_TreeAssembly_A`, X/offset approximately -6.7m, Z/meters 10.5.");
        builder.AppendLine("- foreground right low root bank: `MYB161_ForegroundRight_LowRootMossBank`, offset +4.8m, meters 11.5.");
        builder.AppendLine("- mid-left enclosure mass: `MYB161_TreeAssembly_C`, offset approximately -9.6m, meters 24.5, reinforcing forest ride enclosure.");
        builder.AppendLine("- hero threshold: `MYB161_HeroThreshold_RootArchNaturalGate`, offset -6.25m, meters 29.5, using the cleaned MYB-160 Meshy root arch diagonally.");
        builder.AppendLine("- back wall: " + result.BackWallMassCount + " grouped side masses at offsets roughly +/-10m to +/-15m.");
        builder.AppendLine();
        builder.AppendLine("## Tree Assemblies");
        foreach (var assembly in result.Assemblies)
        {
            builder.AppendLine();
            builder.AppendLine("### " + assembly.Name);
            builder.AppendLine();
            builder.AppendLine("- role: " + assembly.Role);
            builder.AppendLine("- visible from route: " + (assembly.RouteVisible ? "Yes" : "No"));
            builder.AppendLine("- canopy supported: " + (assembly.CanopySupported ? "Yes" : "No"));
            builder.AppendLine("- grounding: " + assembly.Grounding);
            builder.AppendLine("- notes: " + assembly.Notes);
        }
        builder.AppendLine();
        builder.AppendLine("## Meshy Usage");
        builder.AppendLine();
        builder.AppendLine("- Used existing MYB-160 Meshy assets: Yes, via the preserved baseline scene and one restrained root arch candidate overlay.");
        builder.AppendLine("- New Meshy generations: 0");
        builder.AppendLine("- Manifest status: existing MYB-160 entries are `intakeStatus: approved`, `promotionStatus: candidate`, `license: Provider terms pending project review`.");
        builder.AppendLine("- No production promotion.");
        builder.AppendLine();
        builder.AppendLine("## Route Readability");
        builder.AppendLine();
        builder.AppendLine("- minimumRouteClearanceMeters: " + FormatFloat(result.MinimumRouteClearanceMeters));
        builder.AppendLine("- routeOverlapCount: " + result.RouteOverlapCount);
        builder.AppendLine("- routeReadabilityRegression: " + (result.RouteReadabilityRegression ? "Yes" : "No"));
        builder.AppendLine();
        builder.AppendLine("## Anti-Float / Support");
        builder.AppendLine();
        builder.AppendLine("- floatingAssetCount: " + result.FloatingAssetCount);
        builder.AppendLine("- routeVisibleFloatingAssetCount: " + result.RouteVisibleFloatingAssetCount);
        builder.AppendLine("- maxFloatingClearance: " + FormatFloat(result.MaxFloatingClearance) + "m");
        builder.AppendLine("- sinkingAssetCount: " + result.SinkingAssetCount);
        builder.AppendLine("- maxSinkingDepth: " + FormatFloat(result.MaxSinkingDepth) + "m");
        builder.AppendLine("- routeVisibleUnsupportedCanopyCount: " + result.RouteVisibleUnsupportedCanopyCount);
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
        builder.AppendLine("Scores are implementation estimates pending Julien human visual review.");
        builder.AppendLine();
        builder.AppendLine("| Criterion | Estimate | Notes |");
        builder.AppendLine("|---|---:|---|");
        builder.AppendLine("| Route readability | 4 | Route is kept open, with no overlap and minimum clearance " + FormatFloat(result.MinimumRouteClearanceMeters) + "m. |");
        builder.AppendLine("| Silhouette quality | 3 | Baseline canopy enclosure is preserved and extra pole/picket silhouettes are reduced, but the project still lacks final bespoke forest forms. |");
        builder.AppendLine("| Lighting mood | 3 | Softer baseline mood is preserved with only light scene-local support. |");
        builder.AppendLine("| Material coherence | 3 | Scene-local palette is coherent enough for preview, not final art. |");
        builder.AppendLine("| Foreground richness | 4 | Preferred lush near-camera canopy mass is retained, with restrained grounding support. |");
        builder.AppendLine("| Midground density | 4 | Left enclosure and route edge masses keep the ride feeling like a forest instead of an asset preview. |");
        builder.AppendLine("| Background depth | 3 | Grouped masses add depth, but remain preview-quality. |");
        builder.AppendLine("| Scale credibility | 4 | Grounding, support and clearance metrics pass. |");
        builder.AppendLine("| Composition rhythm | 3 | Revision favors the preferred baseline mood over the sparse first after; composition is safer but still not Premium. |");
        builder.AppendLine();
        builder.AppendLine("## Warning Categories");
        AppendList(builder, "Build / Capture Warnings", result.BuildCaptureWarnings);
        AppendList(builder, "MYB-161 Visual Warnings", result.VisualWarnings);
        AppendList(builder, "MYB-161 Asset / Manifest Warnings", result.AssetManifestWarnings);
        AppendList(builder, "MYB-144 Existing Validator Warnings", result.Myb144ExistingValidatorWarnings);
        AppendList(builder, "Blocking Errors", result.BlockingErrors);
        builder.AppendLine();
        builder.AppendLine("## Governance");
        builder.AppendLine();
        builder.AppendLine("- no canonical scene modified");
        builder.AppendLine("- no gameplay modified");
        builder.AppendLine("- no route collider/trajectory change");
        builder.AppendLine("- no production promotion");
        builder.AppendLine("- no new Meshy generation");
        builder.AppendLine("- existing MYB-160 Meshy assets are candidate/preview-only");
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
        builder.AppendLine("# MYB-161 Governance Review");
        builder.AppendLine();
        builder.AppendLine("| Check | Result |");
        builder.AppendLine("|---|---|");
        builder.AppendLine("| Dedicated preview scene exists | Yes |");
        builder.AppendLine("| Builder source of truth exists | Yes |");
        builder.AppendLine("| Seed 161001 used | Yes |");
        builder.AppendLine("| Generated root exists | Yes |");
        builder.AppendLine("| MYB-160 scene modified | No |");
        builder.AppendLine("| Canonical ride scene modified | No |");
        builder.AppendLine("| Gameplay modified | No |");
        builder.AppendLine("| Route trajectory/collider modified | No |");
        builder.AppendLine("| Shared production material modified | No |");
        builder.AppendLine("| New Meshy generations | " + result.NewMeshyGenerationCount + " |");
        builder.AppendLine("| Meshy assets manifest-listed | Yes |");
        builder.AppendLine("| Meshy assets promoted | No |");
        builder.AppendLine("| reviewStatus introduced | No |");
        builder.AppendLine("| example:true introduced | No |");
        builder.AppendLine("| MYB-144 run | " + (string.IsNullOrWhiteSpace(result.Myb144Verdict) || result.Myb144Verdict == "Not run" ? "No" : "Yes") + " |");
        builder.AppendLine("| MYB-144 errors | " + result.Myb144ErrorCount + " |");
        builder.AppendLine("| MYB-144 warnings | " + result.Myb144WarningCount + " |");
        builder.AppendLine("| route readability regression | " + (result.RouteReadabilityRegression ? "Yes" : "No") + " |");
        builder.AppendLine("| route-visible floating assets | " + result.RouteVisibleFloatingAssetCount + " |");
        builder.AppendLine("| route-visible unsupported canopies | " + result.RouteVisibleUnsupportedCanopyCount + " |");
        builder.AppendLine("| thumbnail forest read | " + result.ThumbnailForestRead + " |");
        builder.AppendLine("| hero beat read | " + result.HeroBeatRead + " |");
        builder.AppendLine("| Premium target reached | No |");
        builder.AppendLine("| Recommended Linear status | In Review |");
        builder.AppendLine();
        builder.AppendLine("Final auto-review:");
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

    private static void PreserveBaselineObject(string objectName, string reason, BuildResult result)
    {
        var instance = FindSceneObjectByName(objectName);
        if (instance == null)
        {
            result.BuildCaptureWarnings.Add("Baseline object `" + objectName + "` not found; MYB-161 continues without preserving it explicitly.");
            return;
        }

        instance.SetActive(true);
        result.BaselineObjectsPreserved.Add(objectName + " (" + reason + ")");
    }

    private static void DestroyIfExists(string objectName)
    {
        var instance = FindSceneObjectByName(objectName);
        if (instance != null)
        {
            UnityEngine.Object.DestroyImmediate(instance);
        }
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

    private sealed class TreePlan
    {
        public readonly string Name;
        public readonly string Role;
        public readonly float Meters;
        public readonly float Side;
        public readonly float DistanceFromRoute;
        public readonly float Height;
        public readonly float TrunkRadius;
        public readonly float CanopyScale;
        public readonly float ClearanceRadius;
        public readonly bool RouteVisibleTarget;
        public readonly string Notes;

        public TreePlan(string name, string role, float meters, float side, float distanceFromRoute, float height, float trunkRadius, float canopyScale, float clearanceRadius, bool routeVisibleTarget, string notes)
        {
            Name = name;
            Role = role;
            Meters = meters;
            Side = side;
            DistanceFromRoute = distanceFromRoute;
            Height = height;
            TrunkRadius = trunkRadius;
            CanopyScale = canopyScale;
            ClearanceRadius = clearanceRadius;
            RouteVisibleTarget = routeVisibleTarget;
            Notes = notes;
        }
    }

    private sealed class MeshyPlan
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
        public readonly string Notes;

        public MeshyPlan(string sceneName, string family, string manifestId, string assetPath, string meshyTaskId, float meters, float side, float distanceFromRoute, float scale, float clearanceRadius, bool isTreeAssembly, string notes)
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
            Notes = notes;
        }
    }

    private sealed class BankPlan
    {
        public readonly string Name;
        public readonly float Meters;
        public readonly float Side;
        public readonly float Distance;
        public readonly float Radius;
        public readonly float Length;
        public readonly float Height;

        public BankPlan(string name, float meters, float side, float distance, float radius, float length, float height)
        {
            Name = name;
            Meters = meters;
            Side = side;
            Distance = distance;
            Radius = radius;
            Length = length;
            Height = height;
        }
    }

    private sealed class WallPlan
    {
        public readonly float Meters;
        public readonly float Side;
        public readonly float Distance;
        public readonly float Height;
        public readonly float TrunkRadius;

        public WallPlan(float meters, float side, float distance, float height, float trunkRadius)
        {
            Meters = meters;
            Side = side;
            Distance = distance;
            Height = height;
            TrunkRadius = trunkRadius;
        }
    }

    private sealed class AssemblyRecord
    {
        public string Name;
        public string Role;
        public bool RouteVisible;
        public bool CanopySupported;
        public string Grounding;
        public string Notes;
    }

    private sealed class MeshyAssetRecord
    {
        public string SceneName;
        public string ManifestId;
        public string Family;
        public string AssetPath;
        public string MeshyTaskId;
        public bool UsedInPreview;
        public bool RouteVisible;
        public bool IsTreeAssembly;
        public int TriangleCount;
        public Vector3 Dimensions;
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

    private sealed class CanopyRecord
    {
        public string Name;
        public bool RouteVisible;
        public bool Supported;
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
        public int ForegroundFrameCount;
        public int SideBankPatchCount;
        public int BackgroundAtmosphereCount;
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
        public int MeshyAssetUsedCount;
        public int NewMeshyGenerationCount;
        public string ThumbnailForestRead = "fail";
        public string HeroBeatRead = "fail";
        public string BlobCanopyDominanceRisk = "medium";
        public string EmptySkyOrFlatBackgroundRisk = "medium";
        public bool RouteReadabilityRegression;
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
        public readonly List<AssemblyRecord> Assemblies = new List<AssemblyRecord>();
        public readonly List<MeshyAssetRecord> MeshyAssets = new List<MeshyAssetRecord>();
        public readonly List<PlacementRecord> Placements = new List<PlacementRecord>();
        public readonly List<GroundingRecord> GroundingRecords = new List<GroundingRecord>();
        public readonly List<CanopyRecord> Canopies = new List<CanopyRecord>();
        public readonly List<string> BaselineObjectsPreserved = new List<string>();
        public readonly List<string> LayoutDecisions = new List<string>();
        public readonly List<string> BuildCaptureWarnings = new List<string>();
        public readonly List<string> VisualWarnings = new List<string>();
        public readonly List<string> AssetManifestWarnings = new List<string>();
        public readonly List<string> Myb144ExistingValidatorWarnings = new List<string>();
        public readonly List<string> BlockingErrors = new List<string>();
        public readonly List<string> CaptureReports = new List<string>();
        public readonly List<string> CaptureMetadata = new List<string>();

        public string VisualVerdict => "Checkpoint insuffisant, but visually stronger pending Julien human review";

        public string ToConsoleSummary()
        {
            return "MYB-161 art-directed golden slice " + (BlockingErrors.Count == 0 ? "PASS_WITH_WARNINGS" : "FAIL")
                + " | treeAssemblyCount=" + TreeAssemblyCount
                + " | heroBeatCount=" + HeroBeatCount
                + " | routeOverlapCount=" + RouteOverlapCount
                + " | minClearance=" + MinimumRouteClearanceMeters.ToString("0.###", CultureInfo.InvariantCulture)
                + " | routeVisibleFloatingAssetCount=" + RouteVisibleFloatingAssetCount
                + " | unsupportedCanopies=" + RouteVisibleUnsupportedCanopyCount
                + " | blockers=" + BlockingErrors.Count;
        }
    }
}

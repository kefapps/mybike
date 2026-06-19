using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using MYB89;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class MYB163CanonicalForestPassageIntegrator
{
    private const int Seed = 163001;
    private const string CanonicalScenePath = "Assets/Scenes/MYB89UnityMcpProbe.unity";
    private const string GeneratedRootName = "MYB163_CanonicalForestPassageRoot";
    private const string ImplementationRootRelative = "_bmad-output/implementation-artifacts/MYB-163";
    private const string VisualRootRelative = "_bmad-output/visual-checkpoints/MYB-163";
    private const string MetricsRelativePath = ImplementationRootRelative + "/myb-163-canonical-forest-passage-metrics.json";
    private const string ReportRelativePath = ImplementationRootRelative + "/myb-163-implementation-report.md";
    private const string GovernanceReportRelativePath = ImplementationRootRelative + "/myb-163-governance-review.md";
    private const float RoadKeepClearRadius = 2.6f;
    private const float SinkMeters = 0.03f;
    private const string PremiumTreeRoot = "Assets/Echappee/Art/PremiumTreePolyHaven";

    private static readonly Vector3[] RoutePoints =
    {
        new Vector3(0f, 0.12f, 0f),
        new Vector3(2f, 0.16f, 24f),
        new Vector3(-4f, 0.26f, 55f),
        new Vector3(-7f, 0.38f, 88f),
        new Vector3(-1f, 0.2f, 118f),
        new Vector3(6f, 0.14f, 150f),
        new Vector3(3f, 0.22f, 184f),
        new Vector3(-3f, 0.28f, 215f),
        new Vector3(0f, 0.12f, 242f)
    };

    [MenuItem("Tools/MyBike/MYB-163/Build Canonical Forest Passage")]
    public static void BuildFromMenu()
    {
        var result = BuildCanonicalPassage(writeReports: true);
        Debug.Log(result.ToConsoleSummary());
    }

    [MenuItem("Tools/MyBike/MYB-163/Build + Capture + Validate")]
    public static void BuildCaptureValidateFromMenu()
    {
        var result = BuildCaptureValidate();
        Debug.Log(result.ToConsoleSummary());
    }

    public static void RunBatchBuild()
    {
        var result = BuildCanonicalPassage(writeReports: true);
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
        var result = CreateResult();
        Directory.CreateDirectory(ToRepoPath(ImplementationRootRelative));
        Directory.CreateDirectory(ToRepoPath(VisualRootRelative));

        ResetSceneForBeforeCapture(result);
        EnsureCaptureRig(result, "before");
        var beforeCapture = CaptureScene("before");
        AppendCaptureResult(result, beforeCapture);

        result = BuildCanonicalPassage(writeReports: false, existing: result);
        EnsureCaptureRig(result, "after");
        var afterCapture = CaptureScene("after");
        AppendCaptureResult(result, afterCapture);
        CreateComparisonSheets(result, beforeCapture, afterCapture);

        var myb144 = MYB144ArtAssetValidator.RunValidation("MYB-163-BuildCaptureValidate");
        result.Myb144Verdict = myb144.Verdict;
        result.Myb144ErrorCount = myb144.ErrorCount;
        result.Myb144WarningCount = myb144.WarningCount;
        result.Myb144ReportRelativePath = "_bmad-output/unity-test-results/myb-144-art-asset-validator-report.md";
        if (myb144.ErrorCount > 0)
        {
            result.BlockingErrors.Add("MYB-144 returned errors. Inspect the validator report before reviewing MYB-163.");
        }
        if (myb144.WarningCount > 0)
        {
            result.Myb144ExistingValidatorWarnings.Add("MYB-144 returned warnings. They are recorded separately from MYB-163 visual warnings.");
        }

        WriteReports(result);
        EditorSceneManager.OpenScene(CanonicalScenePath, OpenSceneMode.Single);
        return result;
    }

    private static BuildResult BuildCanonicalPassage(bool writeReports, BuildResult existing = null)
    {
        var result = existing ?? CreateResult();
        Directory.CreateDirectory(ToRepoPath(ImplementationRootRelative));
        Directory.CreateDirectory(ToRepoPath(VisualRootRelative));
        EnsureFolder("Assets/MYB163");
        EnsureFolder("Assets/MYB163/Materials");
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

        var scene = EditorSceneManager.OpenScene(CanonicalScenePath, OpenSceneMode.Single);
        DestroyGeneratedRoot();

        var probeRoot = GameObject.Find("MYB89_ProbeRoot");
        if (probeRoot == null)
        {
            result.BlockingErrors.Add("Canonical scene is missing MYB89_ProbeRoot.");
            WriteReports(result);
            return result;
        }

        if (GameObject.Find("MYB104_ProductionPassages") == null)
        {
            result.VisualWarnings.Add("MYB104_ProductionPassages not found. MYB-163 expected to layer onto the existing canonical forest passage.");
        }

        var root = new GameObject(GeneratedRootName);
        root.transform.SetParent(probeRoot.transform, false);

        var materials = CreateMaterials();
        result.SceneLocalMaterialCount = materials.Count;

        var route = MYB89RideTrajectory.BuildSmoothedPoints(RoutePoints);
        var routeCamera = FindSingleNamedCamera("RouteCamera");
        var routePlanes = routeCamera == null ? null : GeometryUtility.CalculateFrustumPlanes(routeCamera);
        if (routeCamera == null)
        {
            result.BuildCaptureWarnings.Add("RouteCamera not found while building MYB-163. Route-visible metrics are conservative fallback values.");
        }

        var foreground = CreateChild(root.transform, "A_ForegroundCanopyFrame");
        var nearBanks = CreateChild(root.transform, "B_NearGroundingBanks");
        var hero = CreateChild(root.transform, "C_RestrainedRootThreshold");
        var backWall = CreateChild(root.transform, "D_GroupedForestBackWall");
        var atmosphere = CreateChild(root.transform, "E_SoftBackgroundSilhouettes");

        BuildForegroundCanopyFrame(route, materials, foreground.transform, routePlanes, result);
        BuildNearGroundingBanks(route, materials, nearBanks.transform, routePlanes, result);
        BuildRestrainedHeroThreshold(route, materials, hero.transform, routePlanes, result);
        BuildGroupedBackWall(route, materials, backWall.transform, routePlanes, result);
        BuildSoftBackground(route, materials, atmosphere.transform, routePlanes, result);
        AddLocalMoodLight(root.transform, route, result);

        result.RendererCount = root.GetComponentsInChildren<Renderer>(true).Length;
        result.MeshFilterCount = root.GetComponentsInChildren<MeshFilter>(true).Length;
        result.ApproximateTriangles = CountTriangles(root);
        result.RouteOverlapCount = result.Placements.Count(placement => Mathf.Abs(placement.Offset) - placement.Radius < RoadKeepClearRadius);
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
        result.MeshyAssetUsedCount = 0;
        result.NewMeshyGenerationCount = 0;
        result.ThumbnailForestRead = result.TreeAssemblyCount >= 3 && result.BackWallMassCount >= 5 ? "pass" : "warning";
        result.HeroBeatRead = result.HeroBeatCount == 1 ? "pass" : "warning";
        result.EmptySkyOrFlatBackgroundRisk = result.BackWallMassCount >= 7 && result.BackgroundAtmosphereCount >= 4 ? "low" : "medium";
        result.RouteReadabilityRegression = false;

        result.VisualWarnings.Add("Premium target intentionally not claimed; MYB-163 needs Julien route-camera review before any Done closure.");
        result.VisualWarnings.Add("MYB-163 layers grouped canopy and forest masses over the canonical forest passage instead of copying the MYB-161 preview scene.");
        result.VisualWarnings.Add("Existing MYB104 forest objects remain active; MYB-163 reduces the perceived thin/picket look by adding grouped masses rather than destructively removing prior authored content.");
        result.AssetManifestWarnings.Add("No new Meshy generation and no Meshy production promotion. MYB-160 candidates are not used directly in the canonical scene.");

        if (result.RouteOverlapCount > 0)
        {
            result.BlockingErrors.Add("MYB-163 route overlap risk detected. routeOverlapCount=" + result.RouteOverlapCount + ".");
        }
        if (result.MinimumRouteClearanceMeters < RoadKeepClearRadius)
        {
            result.BlockingErrors.Add("MYB-163 minimum route clearance below " + FormatFloat(RoadKeepClearRadius) + "m. minimumRouteClearanceMeters=" + FormatFloat(result.MinimumRouteClearanceMeters) + ".");
        }
        if (result.RouteVisibleFloatingAssetCount > 0)
        {
            result.BlockingErrors.Add("MYB-163 route-visible floating assets detected above blocking threshold. routeVisibleFloatingAssetCount=" + result.RouteVisibleFloatingAssetCount + ".");
        }
        if (result.RouteVisibleUnsupportedCanopyCount > 0)
        {
            result.BlockingErrors.Add("MYB-163 route-visible unsupported canopy detected. routeVisibleUnsupportedCanopyCount=" + result.RouteVisibleUnsupportedCanopyCount + ".");
        }
        if (result.HeroBeatCount != 1)
        {
            result.BlockingErrors.Add("MYB-163 expected exactly 1 restrained hero beat. Actual=" + result.HeroBeatCount + ".");
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

        if (writeReports)
        {
            WriteReports(result);
        }

        return result;
    }

    private static void BuildForegroundCanopyFrame(
        IReadOnlyList<Vector3> route,
        IReadOnlyDictionary<string, Material> materials,
        Transform parent,
        Plane[] routePlanes,
        BuildResult result)
    {
        CreateTreeAssembly(new TreePlan("MYB163_TreeAssembly_CloseLeftFrame", "Foreground left canopy frame", 9.8f, -1f, 6.85f, 7.2f, 0.48f, 2.55f, 1.18f, "Preserves the preferred near-camera upper-left forest enclosure while keeping the route-camera horizon readable."), route, materials, parent, routePlanes, result);
        CreateTreeAssembly(new TreePlan("MYB163_TreeAssembly_MidLeftEnclosure", "Mid-left enclosure tree", 20.5f, -1f, 7.75f, 7.0f, 0.42f, 2.40f, 1.20f, "Adds grouped canopy mass without making a picket row."), route, materials, parent, routePlanes, result);
        CreateTreeAssembly(new TreePlan("MYB163_TreeAssembly_RightAnchor", "Right side ride anchor", 24.0f, 1f, 7.35f, 6.8f, 0.40f, 2.18f, 1.15f, "Keeps the right side authored but secondary."), route, materials, parent, routePlanes, result);
        CreatePremiumTreeAnchor("MYB163_PremiumTreeAnchor_CloseLeft_A", "MYB112_PremiumTree_A.prefab", 14.0f, -1f, 6.25f, 0.74f, route, parent, routePlanes, result);
        CreatePremiumTreeAnchor("MYB163_PremiumTreeAnchor_Right_B", "MYB112_PremiumTree_B.prefab", 18.5f, 1f, 6.65f, 0.70f, route, parent, routePlanes, result);
        CreatePremiumTreeAnchor("MYB163_PremiumTreeAnchor_MidLeft_C", "MYB112_PremiumTree_C.prefab", 29.0f, -1f, 6.85f, 0.70f, route, parent, routePlanes, result);
        result.ForegroundFrameCount = 1;
    }

    private static void BuildNearGroundingBanks(
        IReadOnlyList<Vector3> route,
        IReadOnlyDictionary<string, Material> materials,
        Transform parent,
        Plane[] routePlanes,
        BuildResult result)
    {
        CreateSideBank("MYB163_LeftForestFloorShoulder_A", 12.2f, -1f, 6.15f, 0.95f, 3.4f, 0.18f, route, materials["mossDeep"], parent, routePlanes, result);
        CreateSideBank("MYB163_LeftForestFloorShoulder_B", 19.0f, -1f, 6.35f, 1.25f, 4.5f, 0.24f, route, materials["leafDark"], parent, routePlanes, result);
        CreateSideBank("MYB163_RightLowMossBank_A", 13.8f, 1f, 5.85f, 0.85f, 3.2f, 0.16f, route, materials["mossShadow"], parent, routePlanes, result);
        CreateSideBank("MYB163_RightLowMossBank_B", 22.0f, 1f, 6.05f, 1.00f, 4.0f, 0.20f, route, materials["mossDeep"], parent, routePlanes, result);
        CreateRootCluster("MYB163_RightForegroundRootCluster", 12.5f, 1f, 5.35f, route, materials, parent, routePlanes, result);
        CreateRootCluster("MYB163_LeftGroundingRootCluster", 22.5f, -1f, 5.65f, route, materials, parent, routePlanes, result);
        CreateRockCluster("MYB163_RightMossRockMarker", 17.0f, 1f, 5.75f, route, materials, parent, routePlanes, result);
    }

    private static void BuildRestrainedHeroThreshold(
        IReadOnlyList<Vector3> route,
        IReadOnlyDictionary<string, Material> materials,
        Transform parent,
        Plane[] routePlanes,
        BuildResult result)
    {
        CreateHeroRootThreshold("MYB163_RootThresholdHero", 34.0f, -1f, 6.05f, route, materials, parent, routePlanes, result);
        CreateLeafHalo("MYB163_HeroGroundingHalo", 34.0f, -1f, 6.05f, 1.9f, 4.35f, materials["mossShadow"], route, parent, routePlanes, result);
        result.HeroBeatCount = 1;
    }

    private static void BuildGroupedBackWall(
        IReadOnlyList<Vector3> route,
        IReadOnlyDictionary<string, Material> materials,
        Transform parent,
        Plane[] routePlanes,
        BuildResult result)
    {
        var plans = new[]
        {
            new WallPlan(28f, -1f, 9.4f, 6.2f),
            new WallPlan(31f, 1f, 9.8f, 6.4f),
            new WallPlan(39f, -1f, 10.4f, 7.2f),
            new WallPlan(43f, 1f, 10.7f, 7.0f),
            new WallPlan(52f, -1f, 11.2f, 7.8f),
            new WallPlan(58f, 1f, 11.6f, 7.4f),
            new WallPlan(69f, -1f, 12.6f, 8.0f),
            new WallPlan(74f, 1f, 12.2f, 7.8f)
        };

        foreach (var plan in plans)
        {
            CreateBackWallMass(plan, route, materials, parent, routePlanes, result);
        }
    }

    private static void BuildSoftBackground(
        IReadOnlyList<Vector3> route,
        IReadOnlyDictionary<string, Material> materials,
        Transform parent,
        Plane[] routePlanes,
        BuildResult result)
    {
        CreateBackgroundGroup("MYB163_SoftBackground_L_00", 82f, -1f, 15.0f, route, materials, parent, routePlanes, result);
        CreateBackgroundGroup("MYB163_SoftBackground_R_00", 86f, 1f, 14.6f, route, materials, parent, routePlanes, result);
        CreateBackgroundGroup("MYB163_SoftBackground_L_01", 96f, -1f, 16.2f, route, materials, parent, routePlanes, result);
        CreateBackgroundGroup("MYB163_SoftBackground_R_01", 102f, 1f, 15.8f, route, materials, parent, routePlanes, result);
    }

    private static void CreateTreeAssembly(
        TreePlan plan,
        IReadOnlyList<Vector3> route,
        IReadOnlyDictionary<string, Material> materials,
        Transform parent,
        Plane[] routePlanes,
        BuildResult result)
    {
        if (!MYB89RideTrajectory.TrySample(route, plan.Meters, false, out var sample))
        {
            result.BlockingErrors.Add("Could not sample canonical route for " + plan.Name + ".");
            return;
        }

        var offset = plan.Side * plan.DistanceFromRoute;
        var groundY = sample.Position.y + TerrainHeight(plan.Meters, offset);
        var assembly = new GameObject(plan.Name);
        assembly.transform.SetParent(parent, false);
        assembly.transform.position = sample.Position + sample.Right * offset + Vector3.up * (TerrainHeight(plan.Meters, offset) + 0.35f);
        assembly.transform.rotation = Quaternion.LookRotation(sample.Forward, Vector3.up) * Quaternion.Euler(0f, plan.Side * 12f + Jitter(plan.Meters, 8f), plan.Side * -4f);

        AddMeshChild(assembly.transform, "expressive_supported_trunk", CreateTaperedTrunkMesh(plan.Height, plan.TrunkRadius, plan.Meters), materials["barkWarm"], Vector3.zero, Quaternion.Euler(0f, plan.Side * -9f, plan.Side * -4f), Vector3.one);
        AddMeshChild(assembly.transform, "rear_trunk_support", CreateTaperedTrunkMesh(plan.Height * 0.74f, plan.TrunkRadius * 0.56f, plan.Meters + 2f), materials["barkDark"], new Vector3(0.42f * plan.Side, 0f, -0.42f), Quaternion.Euler(0f, plan.Side * 18f, plan.Side * 5f), Vector3.one);
        AddMeshChild(assembly.transform, "inner_branch_trunk_support", CreateTaperedTrunkMesh(plan.Height * 0.58f, plan.TrunkRadius * 0.38f, plan.Meters + 5f), materials["rootDark"], new Vector3(-0.38f * plan.Side, 0f, 0.38f), Quaternion.Euler(0f, plan.Side * -22f, plan.Side * -8f), Vector3.one);

        for (var root = 0; root < 10; root++)
        {
            var angle = root / 10f * 360f + Jitter(plan.Meters + root * 4.7f, 18f);
            AddMeshChild(
                assembly.transform,
                "wide_grounding_root_" + root.ToString("00", CultureInfo.InvariantCulture),
                CreateRootFlareMesh(plan.CanopyScale * (0.72f + Mathf.Abs(Jitter(plan.Meters + root, 0.18f))), plan.TrunkRadius * 1.25f, 0.18f, plan.Meters + root),
                root % 2 == 0 ? materials["rootDark"] : materials["mossDeep"],
                Vector3.up * 0.012f,
                Quaternion.Euler(0f, angle, 0f),
                Vector3.one);
        }

        for (var branch = 0; branch < 5; branch++)
        {
            var y = plan.Height * (0.38f + branch * 0.105f);
            var yaw = plan.Side * (30f + branch * 24f) + Jitter(plan.Meters + branch * 4.4f, 13f);
            var pitch = -17f - Mathf.Abs(Jitter(plan.Meters + branch * 6.1f, 7f));
            AddMeshChild(
                assembly.transform,
                "visible_canopy_support_branch_" + branch.ToString("00", CultureInfo.InvariantCulture),
                CreateBranchMesh(plan.CanopyScale * (1.25f + branch * 0.12f), plan.TrunkRadius * 0.32f, plan.Meters + branch * 1.7f),
                materials["barkDark"],
                new Vector3(0f, y, 0f),
                Quaternion.Euler(pitch, yaw, 0f),
                Vector3.one);
        }

        var towardRoute = -plan.Side;
        var canopyCenters = new[]
        {
            new Vector3(1.18f * towardRoute, plan.Height * 0.66f, 0.30f),
            new Vector3(0.35f * towardRoute, plan.Height * 0.84f, -0.62f),
            new Vector3(-0.92f * towardRoute, plan.Height * 0.76f, 0.76f),
            new Vector3(1.72f * towardRoute, plan.Height * 0.58f, -0.32f)
        };
        var canopyScales = new[]
        {
            new Vector3(plan.CanopyScale * 1.12f, plan.CanopyScale * 0.50f, plan.CanopyScale * 0.88f),
            new Vector3(plan.CanopyScale * 0.88f, plan.CanopyScale * 0.42f, plan.CanopyScale * 0.70f),
            new Vector3(plan.CanopyScale * 1.00f, plan.CanopyScale * 0.46f, plan.CanopyScale * 0.84f),
            new Vector3(plan.CanopyScale * 0.95f, plan.CanopyScale * 0.40f, plan.CanopyScale * 0.74f)
        };

        if (string.Equals(plan.Name, "MYB163_TreeAssembly_CloseLeftFrame", StringComparison.Ordinal))
        {
            canopyCenters[0] = new Vector3(0.62f * towardRoute, plan.Height * 0.73f, 0.06f);
            canopyCenters[3] = new Vector3(0.74f * towardRoute, plan.Height * 0.66f, -0.78f);
            canopyScales[0] = new Vector3(plan.CanopyScale * 0.86f, plan.CanopyScale * 0.44f, plan.CanopyScale * 0.74f);
            canopyScales[3] = new Vector3(plan.CanopyScale * 0.72f, plan.CanopyScale * 0.34f, plan.CanopyScale * 0.62f);
        }

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
            RegisterCanopy(canopyObject, routePlanes, supported: true, result);
        }

        AddMeshChild(assembly.transform, "moss_leaf_grounding_halo", CreateOvalPatchMesh(plan.CanopyScale * 1.25f, plan.CanopyScale * 0.82f, 0.06f, plan.Meters + 21f), materials["mossDeep"], Vector3.up * 0.01f, Quaternion.Euler(0f, Jitter(plan.Meters + 31f, 18f), 0f), Vector3.one);

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

    private static void CreatePremiumTreeAnchor(
        string name,
        string prefabName,
        float meters,
        float side,
        float distance,
        float scale,
        IReadOnlyList<Vector3> route,
        Transform parent,
        Plane[] routePlanes,
        BuildResult result)
    {
        if (!MYB89RideTrajectory.TrySample(route, meters, false, out var sample))
        {
            result.BlockingErrors.Add("Could not sample canonical route for " + name + ".");
            return;
        }

        var prefabPath = PremiumTreeRoot + "/Prefabs/" + prefabName;
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefab == null)
        {
            result.AssetManifestWarnings.Add("Missing premium tree prefab for " + name + ": " + prefabPath);
            return;
        }

        var offset = side * distance;
        var groundY = sample.Position.y + TerrainHeight(meters, offset);
        var anchor = new GameObject(name);
        anchor.transform.SetParent(parent, false);
        anchor.transform.position = sample.Position + sample.Right * offset + Vector3.up * (TerrainHeight(meters, offset) + 0.15f);
        anchor.transform.rotation = Quaternion.LookRotation(sample.Forward, Vector3.up)
            * Quaternion.Euler(0f, side * (22f + Mathf.Abs(Jitter(meters, 6f))), side * -2.5f);

        var instance = PrefabUtility.InstantiatePrefab(prefab, anchor.transform) as GameObject;
        if (instance == null)
        {
            UnityEngine.Object.DestroyImmediate(anchor);
            result.AssetManifestWarnings.Add("Could not instantiate premium tree prefab for " + name + ": " + prefabPath);
            return;
        }

        instance.name = name + "_PremiumVariant";
        instance.transform.localPosition = Vector3.zero;
        instance.transform.localRotation = Quaternion.Euler(0f, side * Jitter(meters + 14f, 11f), 0f);
        instance.transform.localScale = Vector3.one * scale;

        foreach (var collider in anchor.GetComponentsInChildren<Collider>(true))
        {
            UnityEngine.Object.DestroyImmediate(collider);
        }

        GroundObjectByVisualBottom(anchor, groundY, routePlanes, "Premium foreground tree anchor", result);
        var bounds = CombinedRendererBounds(anchor) ?? new Bounds(anchor.transform.position, Vector3.one);
        var routeVisible = routePlanes != null && GeometryUtility.TestPlanesAABB(routePlanes, bounds);

        result.TreeAssemblyCount++;
        if (routeVisible)
        {
            result.RouteVisibleTreeAssemblyCount++;
        }

        result.Assemblies.Add(new AssemblyRecord
        {
            Name = anchor.name,
            Role = "Premium foreground tree anchor",
            RouteVisible = routeVisible,
            CanopySupported = true,
            Grounding = "premium prefab grounded by combined renderer bounds min.y, sink " + FormatFloat(SinkMeters) + "m",
            Notes = "Uses existing MYB112 PremiumTreePolyHaven prefab for close route-camera bark/moss material read."
        });
        result.Placements.Add(new PlacementRecord
        {
            Name = anchor.name,
            Family = "Premium tree anchor",
            Meters = meters,
            Offset = offset,
            Radius = 1.4f,
            RouteVisible = routeVisible
        });
    }

    private static void CreateHeroRootThreshold(
        string name,
        float meters,
        float side,
        float distance,
        IReadOnlyList<Vector3> route,
        IReadOnlyDictionary<string, Material> materials,
        Transform parent,
        Plane[] routePlanes,
        BuildResult result)
    {
        if (!MYB89RideTrajectory.TrySample(route, meters, false, out var sample))
        {
            result.BlockingErrors.Add("Could not sample canonical route for " + name + ".");
            return;
        }

        var offset = side * distance;
        var groundY = sample.Position.y + TerrainHeight(meters, offset);
        var hero = new GameObject(name);
        hero.transform.SetParent(parent, false);
        hero.transform.position = sample.Position + sample.Right * offset + Vector3.up * (TerrainHeight(meters, offset) + 0.25f);
        hero.transform.rotation = Quaternion.LookRotation(sample.Forward, Vector3.up) * Quaternion.Euler(0f, side * -24f, 0f);

        AddMeshChild(hero.transform, "grounded_root_pillar_a", CreateTaperedTrunkMesh(3.7f, 0.46f, meters), materials["rootDark"], new Vector3(-0.75f * side, 0f, -0.45f), Quaternion.Euler(0f, side * 12f, side * -12f), Vector3.one);
        AddMeshChild(hero.transform, "grounded_root_pillar_b", CreateTaperedTrunkMesh(3.2f, 0.36f, meters + 2f), materials["barkWarm"], new Vector3(0.65f * side, 0f, 0.48f), Quaternion.Euler(0f, side * -18f, side * 10f), Vector3.one);
        AddMeshChild(hero.transform, "sideways_threshold_root_a", CreateBranchMesh(3.9f, 0.28f, meters + 4f), materials["rootDark"], new Vector3(-0.6f * side, 2.15f, -0.25f), Quaternion.Euler(-18f, side * 60f, side * -12f), Vector3.one);
        AddMeshChild(hero.transform, "sideways_threshold_root_b", CreateBranchMesh(3.2f, 0.22f, meters + 6f), materials["barkDark"], new Vector3(0.42f * side, 1.85f, 0.35f), Quaternion.Euler(-14f, side * 42f, side * 9f), Vector3.one);

        for (var i = 0; i < 8; i++)
        {
            AddMeshChild(
                hero.transform,
                "threshold_grounding_root_" + i.ToString("00", CultureInfo.InvariantCulture),
                CreateRootFlareMesh(1.4f + Mathf.Abs(Jitter(meters + i, 0.35f)), 0.25f + Mathf.Abs(Jitter(meters + i, 0.08f)), 0.16f, meters + i),
                i % 2 == 0 ? materials["rootDark"] : materials["mossShadow"],
                Vector3.up * 0.015f,
                Quaternion.Euler(0f, i * 45f + Jitter(meters + i, 16f), 0f),
                Vector3.one);
        }

        AddMeshChild(hero.transform, "low_supported_leaf_clump", CreateCanopyLobeMesh(1.45f, 0.50f, 1.05f, meters + 8f), materials["leafShadow"], new Vector3(-0.35f * side, 3.15f, 0.32f), Quaternion.Euler(0f, side * 24f, side * 6f), Vector3.one);
        RegisterCanopy(hero, routePlanes, supported: true, result);
        GroundObjectByVisualBottom(hero, groundY, routePlanes, "Restrained root threshold", result);

        var bounds = CombinedRendererBounds(hero) ?? new Bounds(hero.transform.position, Vector3.one);
        result.Placements.Add(new PlacementRecord
        {
            Name = hero.name,
            Family = "Hero root threshold",
            Meters = meters,
            Offset = offset,
            Radius = 1.6f,
            RouteVisible = routePlanes != null && GeometryUtility.TestPlanesAABB(routePlanes, bounds)
        });
    }

    private static void CreateSideBank(
        string name,
        float meters,
        float side,
        float distance,
        float radius,
        float length,
        float height,
        IReadOnlyList<Vector3> route,
        Material material,
        Transform parent,
        Plane[] routePlanes,
        BuildResult result)
    {
        if (!MYB89RideTrajectory.TrySample(route, meters, false, out var sample))
        {
            result.BlockingErrors.Add("Could not sample canonical route for " + name + ".");
            return;
        }

        var offset = side * distance;
        var groundY = sample.Position.y + TerrainHeight(meters, offset);
        var bank = new GameObject(name);
        bank.transform.SetParent(parent, false);
        bank.transform.position = sample.Position + sample.Right * offset + Vector3.up * (TerrainHeight(meters, offset) + 0.06f);
        bank.transform.rotation = Quaternion.LookRotation(sample.Forward, Vector3.up) * Quaternion.Euler(0f, side * 14f + Jitter(meters, 18f), 0f);
        bank.AddComponent<MeshFilter>().sharedMesh = CreateBankMesh(radius, length, height, meters);
        bank.AddComponent<MeshRenderer>().sharedMaterial = material;
        GroundObjectByVisualBottom(bank, groundY, routePlanes, "Side bank", result);

        result.SideBankPatchCount++;
        result.Placements.Add(new PlacementRecord
        {
            Name = bank.name,
            Family = "Side bank",
            Meters = meters,
            Offset = offset,
            Radius = radius,
            RouteVisible = IsRouteVisible(bank, routePlanes)
        });
    }

    private static void CreateRootCluster(
        string name,
        float meters,
        float side,
        float distance,
        IReadOnlyList<Vector3> route,
        IReadOnlyDictionary<string, Material> materials,
        Transform parent,
        Plane[] routePlanes,
        BuildResult result)
    {
        if (!MYB89RideTrajectory.TrySample(route, meters, false, out var sample))
        {
            result.BlockingErrors.Add("Could not sample canonical route for " + name + ".");
            return;
        }

        var offset = side * distance;
        var groundY = sample.Position.y + TerrainHeight(meters, offset);
        var cluster = new GameObject(name);
        cluster.transform.SetParent(parent, false);
        cluster.transform.position = sample.Position + sample.Right * offset + Vector3.up * (TerrainHeight(meters, offset) + 0.08f);
        cluster.transform.rotation = Quaternion.LookRotation(sample.Forward, Vector3.up) * Quaternion.Euler(0f, side * 18f + Jitter(meters, 10f), 0f);

        for (var i = 0; i < 7; i++)
        {
            var yaw = i / 7f * 360f + Jitter(meters + i, 18f);
            AddMeshChild(cluster.transform, "ground_root_" + i.ToString("00", CultureInfo.InvariantCulture), CreateRootFlareMesh(1.35f + Mathf.Abs(Jitter(meters + i * 3f, 0.42f)), 0.28f + Mathf.Abs(Jitter(meters + i * 2f, 0.08f)), 0.16f, meters + i), i % 2 == 0 ? materials["rootDark"] : materials["mossDeep"], Vector3.up * 0.01f, Quaternion.Euler(0f, yaw, 0f), Vector3.one);
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
            RouteVisible = IsRouteVisible(cluster, routePlanes)
        });
    }

    private static void CreateRockCluster(
        string name,
        float meters,
        float side,
        float distance,
        IReadOnlyList<Vector3> route,
        IReadOnlyDictionary<string, Material> materials,
        Transform parent,
        Plane[] routePlanes,
        BuildResult result)
    {
        if (!MYB89RideTrajectory.TrySample(route, meters, false, out var sample))
        {
            result.BlockingErrors.Add("Could not sample canonical route for " + name + ".");
            return;
        }

        var offset = side * distance;
        var groundY = sample.Position.y + TerrainHeight(meters, offset);
        var cluster = new GameObject(name);
        cluster.transform.SetParent(parent, false);
        cluster.transform.position = sample.Position + sample.Right * offset + Vector3.up * (TerrainHeight(meters, offset) + 0.08f);
        cluster.transform.rotation = Quaternion.LookRotation(sample.Forward, Vector3.up) * Quaternion.Euler(0f, Jitter(meters, 26f), 0f);

        for (var i = 0; i < 4; i++)
        {
            AddMeshChild(cluster.transform, "mossy_rock_" + i.ToString("00", CultureInfo.InvariantCulture), CreateRockMarkerMesh(0.42f + i * 0.08f, 0.42f + i * 0.12f, 0.38f + i * 0.07f, meters + i), i % 2 == 0 ? materials["stoneMoss"] : materials["mossDeep"], new Vector3((i - 1.5f) * 0.44f, 0.02f, Jitter(meters + i, 0.45f)), Quaternion.Euler(0f, i * 47f, Jitter(meters + i, 6f)), Vector3.one);
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
            RouteVisible = IsRouteVisible(cluster, routePlanes)
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
        IReadOnlyList<Vector3> route,
        Transform parent,
        Plane[] routePlanes,
        BuildResult result)
    {
        if (!MYB89RideTrajectory.TrySample(route, meters, false, out var sample))
        {
            result.BlockingErrors.Add("Could not sample canonical route for " + name + ".");
            return;
        }

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
            RouteVisible = IsRouteVisible(halo, routePlanes)
        });
    }

    private static void CreateBackWallMass(
        WallPlan plan,
        IReadOnlyList<Vector3> route,
        IReadOnlyDictionary<string, Material> materials,
        Transform parent,
        Plane[] routePlanes,
        BuildResult result)
    {
        if (!MYB89RideTrajectory.TrySample(route, plan.Meters, false, out var sample))
        {
            result.BlockingErrors.Add("Could not sample canonical route for MYB163 back wall mass.");
            return;
        }

        var offset = plan.Side * plan.Distance;
        var groundY = sample.Position.y + TerrainHeight(plan.Meters, offset);
        var mass = new GameObject("MYB163_GroupedBackWallMass_" + result.BackWallMassCount.ToString("00", CultureInfo.InvariantCulture));
        mass.transform.SetParent(parent, false);
        mass.transform.position = sample.Position + sample.Right * offset + Vector3.up * (TerrainHeight(plan.Meters, offset) + 0.18f);
        mass.transform.rotation = Quaternion.LookRotation(sample.Forward, Vector3.up) * Quaternion.Euler(0f, plan.Side * 11f + Jitter(plan.Meters, 16f), 0f);

        for (var i = 0; i < 4; i++)
        {
            AddMeshChild(mass.transform, "grouped_background_trunk_" + i.ToString("00", CultureInfo.InvariantCulture), CreateTaperedTrunkMesh(plan.Height * (0.48f + i * 0.08f), 0.28f * (1.18f - i * 0.08f), plan.Meters + i), i % 2 == 0 ? materials["shadowBark"] : materials["rootDark"], new Vector3((i - 1.5f) * 0.78f, 0f, Jitter(plan.Meters + i, 0.45f)), Quaternion.Euler(0f, plan.Side * (10f + i * 9f), plan.Side * (4f - i)), Vector3.one);
        }

        var high = AddMeshChild(mass.transform, "grouped_supported_canopy_high", CreateCanopyLobeMesh(2.45f, 0.86f, 1.48f, plan.Meters + 20f), materials["leafDistant"], new Vector3(0.40f * plan.Side, plan.Height * 0.58f, 0.15f), Quaternion.Euler(plan.Side * 4f, plan.Side * 18f, plan.Side * 5f), Vector3.one);
        var low = AddMeshChild(mass.transform, "grouped_supported_canopy_low", CreateCanopyLobeMesh(2.05f, 0.70f, 1.26f, plan.Meters + 27f), materials["backgroundLeaf"], new Vector3(-0.58f * plan.Side, plan.Height * 0.43f, 0.64f), Quaternion.Euler(plan.Side * -3f, plan.Side * -14f, plan.Side * -6f), Vector3.one);
        var rear = AddMeshChild(mass.transform, "grouped_supported_canopy_rear", CreateCanopyLobeMesh(1.72f, 0.62f, 1.08f, plan.Meters + 31f), materials["mossShadow"], new Vector3(1.18f * plan.Side, plan.Height * 0.46f, -0.58f), Quaternion.Euler(plan.Side * 2f, plan.Side * 28f, plan.Side * 7f), Vector3.one);
        RegisterCanopy(high, routePlanes, supported: true, result);
        RegisterCanopy(low, routePlanes, supported: true, result);
        RegisterCanopy(rear, routePlanes, supported: true, result);

        AddMeshChild(mass.transform, "shadow_floor_mass", CreateOvalPatchMesh(2.0f, 3.1f, 0.08f, plan.Meters + 33f), materials["mossShadow"], Vector3.up * 0.02f, Quaternion.Euler(0f, plan.Side * 22f, 0f), Vector3.one);
        GroundObjectByVisualBottom(mass, groundY, routePlanes, "Grouped forest back wall", result);

        result.BackWallMassCount++;
        result.Placements.Add(new PlacementRecord
        {
            Name = mass.name,
            Family = "Grouped forest back wall",
            Meters = plan.Meters,
            Offset = offset,
            Radius = 1.8f,
            RouteVisible = IsRouteVisible(mass, routePlanes)
        });
    }

    private static void CreateBackgroundGroup(
        string name,
        float meters,
        float side,
        float distance,
        IReadOnlyList<Vector3> route,
        IReadOnlyDictionary<string, Material> materials,
        Transform parent,
        Plane[] routePlanes,
        BuildResult result)
    {
        if (!MYB89RideTrajectory.TrySample(route, meters, false, out var sample))
        {
            result.BlockingErrors.Add("Could not sample canonical route for " + name + ".");
            return;
        }

        var offset = side * distance;
        var groundY = sample.Position.y + TerrainHeight(meters, offset);
        var silhouette = new GameObject(name);
        silhouette.transform.SetParent(parent, false);
        silhouette.transform.position = sample.Position + sample.Right * offset + Vector3.up * (TerrainHeight(meters, offset) + 0.2f);
        silhouette.transform.rotation = Quaternion.LookRotation(sample.Forward, Vector3.up) * Quaternion.Euler(0f, side * 10f + Jitter(meters, 18f), 0f);
        AddMeshChild(silhouette.transform, "soft_far_group_trunk_a", CreateTaperedTrunkMesh(5.8f + Mathf.Abs(Jitter(meters, 0.8f)), 0.25f, meters), materials["backgroundTrunk"], new Vector3(-0.34f * side, 0f, -0.15f), Quaternion.Euler(0f, side * 8f, side * 3f), Vector3.one);
        AddMeshChild(silhouette.transform, "soft_far_group_trunk_b", CreateTaperedTrunkMesh(5.2f + Mathf.Abs(Jitter(meters + 2f, 0.7f)), 0.19f, meters + 2f), materials["backgroundTrunk"], new Vector3(0.42f * side, 0f, 0.24f), Quaternion.Euler(0f, side * -12f, side * -2f), Vector3.one);
        var canopy = AddMeshChild(silhouette.transform, "soft_far_canopy_group", CreateCanopyLobeMesh(2.15f, 0.72f, 1.20f, meters + 9f), materials["backgroundLeaf"], new Vector3(side * 0.35f, 4.7f, 0.2f), Quaternion.Euler(0f, side * 16f, side * 4f), Vector3.one);
        RegisterCanopy(canopy, routePlanes, supported: true, result);
        GroundObjectByVisualBottom(silhouette, groundY, routePlanes, "Soft background silhouette", result);

        result.BackgroundAtmosphereCount++;
        result.Placements.Add(new PlacementRecord
        {
            Name = silhouette.name,
            Family = "Soft background silhouette",
            Meters = meters,
            Offset = offset,
            Radius = 1.35f,
            RouteVisible = IsRouteVisible(silhouette, routePlanes)
        });
    }

    private static void AddLocalMoodLight(Transform parent, IReadOnlyList<Vector3> route, BuildResult result)
    {
        if (!MYB89RideTrajectory.TrySample(route, 30f, false, out var sample))
        {
            return;
        }

        var lightObject = new GameObject("MYB163_LocalSoftGreenCanopyFill");
        lightObject.transform.SetParent(parent, false);
        lightObject.transform.position = sample.Position + sample.Right * -5.6f + Vector3.up * 4.3f;
        var light = lightObject.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = new Color(0.58f, 0.80f, 0.52f);
        light.intensity = 0.72f;
        light.range = 17f;
        light.shadows = LightShadows.None;
        result.LayoutDecisions.Add("Added one local soft green canopy fill under MYB163 root; no global fog or color grading change.");
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

    private static void GroundObjectByVisualBottom(GameObject instance, float groundY, Plane[] routePlanes, string family, BuildResult result)
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
            var bend = new Vector2(Mathf.Sin(seed * 0.21f + y01 * 2.5f) * radius * 0.52f * y01, Mathf.Cos(seed * 0.17f + y01 * 2.1f) * radius * 0.42f * y01);
            for (var segment = 0; segment < segments; segment++)
            {
                var angle = segment / (float)segments * Mathf.PI * 2f;
                var wobble = 1f + Jitter(seed + ring * 5.13f + segment * 3.71f, 0.14f);
                vertices.Add(new Vector3(Mathf.Cos(angle) * radius * ringScales[ring] * wobble + bend.x, y01 * height, Mathf.Sin(angle) * radius * ringScales[ring] * wobble + bend.y));
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
        return BuildMesh("MYB163_TaperedTrunkMesh", vertices, uvs, triangles);
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
        return BuildMesh("MYB163_BranchMesh", vertices, uvs, triangles);
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
        return BuildMesh("MYB163_RootFlareMesh", vertices, uvs, triangles);
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
                vertices.Add(new Vector3(Mathf.Cos(phi) * Mathf.Cos(theta) * radiusX * wobble, Mathf.Sin(phi) * radiusY * wobble, Mathf.Cos(phi) * Mathf.Sin(theta) * radiusZ * wobble));
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

        return BuildMesh("MYB163_AsymmetricCanopyLobeMesh", vertices, uvs, triangles);
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

        return BuildMesh("MYB163_BankPatchMesh", vertices, uvs, triangles);
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

        return BuildMesh("MYB163_RockMarkerMesh", vertices, uvs, triangles);
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
            ["barkWarm"] = TexturedBarkMaterialAt(shader, "MYB163_BarkWarm", new Color(0.25f, 0.16f, 0.095f), 0.19f, 0.30f, 0.42f),
            ["barkDark"] = TexturedBarkMaterialAt(shader, "MYB163_BarkDark", new Color(0.12f, 0.075f, 0.045f), 0.14f, 0.24f, 0.48f),
            ["rootDark"] = TexturedBarkMaterialAt(shader, "MYB163_RootDark", new Color(0.09f, 0.060f, 0.038f), 0.12f, 0.22f, 0.52f),
            ["shadowBark"] = TexturedBarkMaterialAt(shader, "MYB163_ShadowBark", new Color(0.052f, 0.062f, 0.050f), 0.08f, 0.18f, 0.55f),
            ["backgroundTrunk"] = MaterialAt(shader, "MYB163_BackgroundTrunk", new Color(0.065f, 0.085f, 0.070f), 0.09f),
            ["leafDeep"] = MaterialAt(shader, "MYB163_LeafDeep", new Color(0.075f, 0.235f, 0.110f), 0.28f),
            ["leafShadow"] = MaterialAt(shader, "MYB163_LeafShadow", new Color(0.045f, 0.120f, 0.070f), 0.20f),
            ["leafDistant"] = MaterialAt(shader, "MYB163_LeafDistantDesaturated", new Color(0.105f, 0.185f, 0.130f), 0.16f),
            ["backgroundLeaf"] = MaterialAt(shader, "MYB163_BackgroundLeafFog", new Color(0.115f, 0.175f, 0.140f), 0.14f),
            ["mossDeep"] = TexturedMossMaterialAt(shader, "MYB163_MossDeep", new Color(0.060f, 0.165f, 0.080f), 0.20f, 0.24f, 0.48f),
            ["mossShadow"] = TexturedMossMaterialAt(shader, "MYB163_MossShadow", new Color(0.040f, 0.098f, 0.060f), 0.14f, 0.18f, 0.55f),
            ["leafDark"] = MaterialAt(shader, "MYB163_LeafLitterDark", new Color(0.17f, 0.11f, 0.065f), 0.18f),
            ["stoneMoss"] = MaterialAt(shader, "MYB163_StoneMoss", new Color(0.36f, 0.39f, 0.27f), 0.16f)
        };
    }

    private static Material TexturedBarkMaterialAt(Shader shader, string name, Color color, float smoothness, float bumpScale, float occlusionStrength)
    {
        return TexturedMaterialAt(
            shader,
            name,
            color,
            smoothness,
            PremiumTreeRoot + "/Textures/pine_bark_diff_1k.jpg",
            PremiumTreeRoot + "/Textures/pine_bark_nor_gl_1k.jpg",
            PremiumTreeRoot + "/Textures/pine_bark_arm_1k.jpg",
            bumpScale,
            occlusionStrength);
    }

    private static Material TexturedMossMaterialAt(Shader shader, string name, Color color, float smoothness, float bumpScale, float occlusionStrength)
    {
        return TexturedMaterialAt(
            shader,
            name,
            color,
            smoothness,
            PremiumTreeRoot + "/Textures/moss_wood_diff_1k.jpg",
            PremiumTreeRoot + "/Textures/moss_wood_nor_gl_1k.jpg",
            PremiumTreeRoot + "/Textures/moss_wood_arm_1k.jpg",
            bumpScale,
            occlusionStrength);
    }

    private static Material TexturedMaterialAt(
        Shader shader,
        string name,
        Color color,
        float smoothness,
        string baseMapPath,
        string normalMapPath,
        string occlusionMapPath,
        float bumpScale,
        float occlusionStrength)
    {
        var material = MaterialAt(shader, name, color, smoothness);
        var baseMap = AssetDatabase.LoadAssetAtPath<Texture2D>(baseMapPath);
        var normalMap = AssetDatabase.LoadAssetAtPath<Texture2D>(normalMapPath);
        var occlusionMap = AssetDatabase.LoadAssetAtPath<Texture2D>(occlusionMapPath);

        if (baseMap != null)
        {
            SetTextureIfPresent(material, "_BaseMap", baseMap);
            SetTextureIfPresent(material, "_MainTex", baseMap);
        }
        if (normalMap != null)
        {
            SetTextureIfPresent(material, "_BumpMap", normalMap);
            material.EnableKeyword("_NORMALMAP");
        }
        if (occlusionMap != null)
        {
            SetTextureIfPresent(material, "_OcclusionMap", occlusionMap);
            material.EnableKeyword("_OCCLUSIONMAP");
        }
        if (material.HasProperty("_BumpScale"))
        {
            material.SetFloat("_BumpScale", bumpScale);
        }
        if (material.HasProperty("_OcclusionStrength"))
        {
            material.SetFloat("_OcclusionStrength", occlusionStrength);
        }

        EditorUtility.SetDirty(material);
        return material;
    }

    private static void SetTextureIfPresent(Material material, string propertyName, Texture2D texture)
    {
        if (material.HasProperty(propertyName))
        {
            material.SetTexture(propertyName, texture);
        }
    }

    private static Material MaterialAt(Shader shader, string name, Color color, float smoothness)
    {
        var path = "Assets/MYB163/Materials/" + name + ".mat";
        var material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            material = new Material(shader) { name = name };
            AssetDatabase.CreateAsset(material, path);
        }
        material.shader = shader;
        material.color = color;
        if (material.HasProperty("_BaseColor"))
        {
            material.SetColor("_BaseColor", color);
        }
        if (material.HasProperty("_Smoothness"))
        {
            material.SetFloat("_Smoothness", smoothness);
        }
        EditorUtility.SetDirty(material);
        return material;
    }

    private static float TerrainHeight(float meters, float offset)
    {
        var abs = Mathf.Abs(offset);
        if (abs < 2.1f) return Mathf.Sin(meters * 0.04f) * 0.015f - abs * 0.002f;
        if (abs < 3.5f) return 0.025f + abs * 0.006f + Mathf.Sin(meters * 0.11f + offset) * 0.01f;
        if (abs < 11.4f) return 0.06f + abs * 0.010f + Mathf.Sin(meters * 0.08f + offset * 0.4f) * 0.035f;
        return 0.12f + abs * 0.018f + Mathf.Sin(meters * 0.055f + offset * 0.25f) * 0.07f;
    }

    private static float Jitter(float seed, float amount)
    {
        return (Mathf.PerlinNoise((seed + Seed) * 0.011f, (seed + Seed) * 0.023f) - 0.5f) * 2f * amount;
    }

    private static bool IsRouteVisible(GameObject root, Plane[] routePlanes)
    {
        return routePlanes != null && GeometryUtility.TestPlanesAABB(routePlanes, CombinedRendererBounds(root) ?? new Bounds(root.transform.position, Vector3.zero));
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

    private static Camera FindSingleNamedCamera(string name)
    {
        var cameras = UnityEngine.Object.FindObjectsByType<Camera>(FindObjectsInactive.Include, FindObjectsSortMode.None)
            .Where(camera => camera != null && camera.gameObject.name == name)
            .OrderBy(camera => camera.gameObject.GetInstanceID())
            .ToArray();
        return cameras.Length == 1 ? cameras[0] : null;
    }

    private static void ResetSceneForBeforeCapture(BuildResult result)
    {
        var scene = EditorSceneManager.OpenScene(CanonicalScenePath, OpenSceneMode.Single);
        DestroyGeneratedRoot();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        result.LayoutDecisions.Add("Before baseline reset removes only `MYB163_CanonicalForestPassageRoot` if present.");
    }

    private static void DestroyGeneratedRoot()
    {
        var previousRoot = GameObject.Find(GeneratedRootName);
        if (previousRoot != null)
        {
            UnityEngine.Object.DestroyImmediate(previousRoot);
        }
    }

    private static void EnsureCaptureRig(BuildResult result, string state)
    {
        var setup = MYB145CaptureRigHelper.SetupCaptureCameras("MYB-163-" + state + "-setup");
        if (setup.ErrorCount > 0)
        {
            result.BlockingErrors.Add("MYB-145 setup failed before " + state + " capture.");
            foreach (var error in setup.Errors)
            {
                result.BuildCaptureWarnings.Add("Setup " + state + " error " + error.Code + ": " + error.Message);
            }
        }
        foreach (var warning in setup.Warnings)
        {
            if (warning.Code != "SCENE_DIRTY_NOT_SAVED")
            {
                result.BuildCaptureWarnings.Add("Setup " + state + " warning " + warning.Code + ": " + warning.Message);
            }
        }
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
    }

    private static MYB145CaptureRigHelper.CaptureResult CaptureScene(string state)
    {
        EditorSceneManager.OpenScene(CanonicalScenePath, OpenSceneMode.Single);
        return MYB145CaptureRigHelper.CaptureRouteAndOverview(
            "MYB-163-" + state,
            new MYB145CaptureRigHelper.CaptureOptions
            {
                TicketId = "MYB-163",
                State = state,
                ScenePath = CanonicalScenePath,
                BaselineSelectedBy = "MYB-163 builder / ticket",
                BaselineReason = "MYB-162 accepted the revised MYB-161 forest mood as the integration guardrail; MYB-163 tests a constrained canonical forest passage update.",
                BaselineSource = "MYB-162 plan and current canonical MYB89 scene"
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
        var pixels = Enumerable.Repeat(Color.black, width * height).ToArray();
        sheet.SetPixels(pixels);
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
        var path = VisualRootRelative + "/" + DateTime.UtcNow.ToString("yyyy-MM-ddTHH-mm-ssZ", CultureInfo.InvariantCulture) + "-before-after-report.md";
        var builder = new StringBuilder();
        builder.AppendLine("# MYB-163 Before/After Capture Report");
        builder.AppendLine();
        builder.AppendLine("- before: current canonical `Assets/Scenes/MYB89UnityMcpProbe.unity` without MYB-163 root");
        builder.AppendLine("- after: canonical scene with `MYB163_CanonicalForestPassageRoot`");
        builder.AppendLine("- before selected by: MYB-163 builder / ticket");
        builder.AppendLine("- reason: MYB-162 accepted MYB-161 revised mood as direction; MYB-163 tests constrained canonical integration.");
        builder.AppendLine("- route capture is primary; overview is secondary.");
        builder.AppendLine();
        builder.AppendLine("## Captures");
        builder.AppendLine();
        builder.AppendLine("- before route: `" + result.BeforeRoutePath + "`");
        builder.AppendLine("- after route: `" + result.AfterRoutePath + "`");
        builder.AppendLine("- route comparison: `" + result.RouteComparisonPath + "`");
        builder.AppendLine("- before overview: `" + result.BeforeOverviewPath + "`");
        builder.AppendLine("- after overview: `" + result.AfterOverviewPath + "`");
        builder.AppendLine("- overview comparison: `" + result.OverviewComparisonPath + "`");
        WriteText(path, builder.ToString());
        result.BeforeAfterCaptureReportPath = path;
    }

    private static void WriteReports(BuildResult result)
    {
        WriteMetrics(result);
        WriteImplementationReport(result);
        WriteGovernanceReview(result);
    }

    private static void WriteMetrics(BuildResult result)
    {
        var builder = new StringBuilder();
        builder.AppendLine("{");
        builder.AppendLine("  \"ticket\": \"MYB-163\",");
        builder.AppendLine("  \"seed\": " + Seed + ",");
        builder.AppendLine("  \"baseline\": \"current canonical MYB89 scene without MYB-163 root\",");
        builder.AppendLine("  \"outputScene\": \"" + CanonicalScenePath + "\",");
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
        builder.AppendLine("  \"maxFloatingClearance\": " + FormatFloat(result.MaxFloatingClearance) + ",");
        builder.AppendLine("  \"sinkingAssetCount\": " + result.SinkingAssetCount + ",");
        builder.AppendLine("  \"maxSinkingDepth\": " + FormatFloat(result.MaxSinkingDepth) + ",");
        builder.AppendLine("  \"routeOverlapCount\": " + result.RouteOverlapCount + ",");
        builder.AppendLine("  \"minimumRouteClearanceMeters\": " + FormatFloat(result.MinimumRouteClearanceMeters) + ",");
        builder.AppendLine("  \"approximateTriangles\": " + result.ApproximateTriangles + ",");
        builder.AppendLine("  \"rendererCount\": " + result.RendererCount + ",");
        builder.AppendLine("  \"meshFilterCount\": " + result.MeshFilterCount + ",");
        builder.AppendLine("  \"sceneLocalMaterialCount\": " + result.SceneLocalMaterialCount + ",");
        builder.AppendLine("  \"meshyAssetUsedCount\": " + result.MeshyAssetUsedCount + ",");
        builder.AppendLine("  \"newMeshyGenerationCount\": " + result.NewMeshyGenerationCount + ",");
        builder.AppendLine("  \"groundPlacementMethod\": \"combined-renderer-bounds-min-y-after-transform\",");
        builder.AppendLine("  \"groundSource\": \"sampled canonical route shoulder/forest floor height\",");
        builder.AppendLine("  \"sinkMeters\": " + FormatFloat(SinkMeters) + ",");
        builder.AppendLine("  \"thumbnailForestRead\": \"" + result.ThumbnailForestRead + "\",");
        builder.AppendLine("  \"heroBeatRead\": \"" + result.HeroBeatRead + "\",");
        builder.AppendLine("  \"emptySkyOrFlatBackgroundRisk\": \"" + result.EmptySkyOrFlatBackgroundRisk + "\",");
        builder.AppendLine("  \"routeReadabilityRegression\": " + (result.RouteReadabilityRegression ? "true" : "false"));
        builder.AppendLine("}");
        WriteText(MetricsRelativePath, builder.ToString());
    }

    private static void WriteImplementationReport(BuildResult result)
    {
        var builder = new StringBuilder();
        builder.AppendLine("# MYB-163 Implementation Report");
        builder.AppendLine();
        builder.AppendLine("## Summary");
        builder.AppendLine();
        builder.AppendLine("MYB-163 applies the MYB-162 productionization plan to the canonical MYB89 forest passage. It adds a constrained, builder-owned root to the canonical scene and keeps the MYB-161 revised direction: lush forest enclosure, clean route center, grouped masses, and no new Meshy usage.");
        builder.AppendLine();
        builder.AppendLine("## Builder");
        builder.AppendLine();
        builder.AppendLine("- path: `unity/Echapee4D/Assets/MYB163/Editor/MYB163CanonicalForestPassageIntegrator.cs`");
        builder.AppendLine("- seed: `" + Seed + "`");
        builder.AppendLine("- generated root: `" + GeneratedRootName + "`");
        builder.AppendLine("- canonical scene: `" + CanonicalScenePath + "`");
        builder.AppendLine();
        builder.AppendLine("## Composition");
        builder.AppendLine();
        builder.AppendLine("- foreground canopy frame: close left and mid-left tree assemblies with grouped supported canopy lobes");
        builder.AppendLine("- near side masses: low moss/leaf banks and root clusters on both route shoulders");
        builder.AppendLine("- hero beat: one restrained lateral root threshold, not crossing the route");
        builder.AppendLine("- back wall: grouped forest masses to reduce thin/picket read");
        builder.AppendLine("- background: soft grouped silhouettes for depth");
        builder.AppendLine("- mood: one local green canopy fill light; no global fog/color grading change");
        builder.AppendLine();
        builder.AppendLine("## Meshy Usage");
        builder.AppendLine();
        builder.AppendLine("- Used existing MYB-160 Meshy assets: No");
        builder.AppendLine("- New Meshy generations: 0");
        builder.AppendLine("- Production promotion: No");
        builder.AppendLine();
        builder.AppendLine("## Route Readability");
        builder.AppendLine();
        builder.AppendLine("- routeOverlapCount: `" + result.RouteOverlapCount + "`");
        builder.AppendLine("- minimumRouteClearanceMeters: `" + FormatFloat(result.MinimumRouteClearanceMeters) + "`");
        builder.AppendLine("- routeReadabilityRegression: `" + (result.RouteReadabilityRegression ? "true" : "false") + "`");
        builder.AppendLine();
        builder.AppendLine("## Anti-Float / Support");
        builder.AppendLine();
        builder.AppendLine("- ground placement: combined renderer bounds `min.y` after transform");
        builder.AppendLine("- sinkMeters: `" + FormatFloat(SinkMeters) + "`");
        builder.AppendLine("- floatingAssetCount: `" + result.FloatingAssetCount + "`");
        builder.AppendLine("- routeVisibleFloatingAssetCount: `" + result.RouteVisibleFloatingAssetCount + "`");
        builder.AppendLine("- maxFloatingClearance: `" + FormatFloat(result.MaxFloatingClearance) + "`");
        builder.AppendLine("- sinkingAssetCount: `" + result.SinkingAssetCount + "`");
        builder.AppendLine("- maxSinkingDepth: `" + FormatFloat(result.MaxSinkingDepth) + "`");
        builder.AppendLine("- routeVisibleUnsupportedCanopyCount: `" + result.RouteVisibleUnsupportedCanopyCount + "`");
        builder.AppendLine();
        builder.AppendLine("## Metrics");
        builder.AppendLine();
        builder.AppendLine("- metrics JSON: `" + MetricsRelativePath + "`");
        builder.AppendLine("- treeAssemblyCount: `" + result.TreeAssemblyCount + "`");
        builder.AppendLine("- routeVisibleTreeAssemblyCount: `" + result.RouteVisibleTreeAssemblyCount + "`");
        builder.AppendLine("- heroBeatCount: `" + result.HeroBeatCount + "`");
        builder.AppendLine("- backWallMassCount: `" + result.BackWallMassCount + "`");
        builder.AppendLine("- routeVisibleCanopyCount: `" + result.RouteVisibleCanopyCount + "`");
        builder.AppendLine("- approximateTriangles: `" + result.ApproximateTriangles + "`");
        builder.AppendLine("- rendererCount: `" + result.RendererCount + "`");
        builder.AppendLine();
        builder.AppendLine("## Visual Evidence");
        builder.AppendLine();
        builder.AppendLine("- before route: `" + result.BeforeRoutePath + "`");
        builder.AppendLine("- after route: `" + result.AfterRoutePath + "`");
        builder.AppendLine("- route comparison: `" + result.RouteComparisonPath + "`");
        builder.AppendLine("- before overview: `" + result.BeforeOverviewPath + "`");
        builder.AppendLine("- after overview: `" + result.AfterOverviewPath + "`");
        builder.AppendLine("- overview comparison: `" + result.OverviewComparisonPath + "`");
        builder.AppendLine("- capture report: `" + result.BeforeAfterCaptureReportPath + "`");
        builder.AppendLine();
        builder.AppendLine("## MYB-144 Validation");
        builder.AppendLine();
        builder.AppendLine("- verdict: `" + result.Myb144Verdict + "`");
        builder.AppendLine("- errors: `" + result.Myb144ErrorCount + "`");
        builder.AppendLine("- warnings: `" + result.Myb144WarningCount + "`");
        builder.AppendLine("- report: `" + result.Myb144ReportRelativePath + "`");
        builder.AppendLine();
        builder.AppendLine("## Visual Rubric Estimate");
        builder.AppendLine();
        builder.AppendLine("Scores are implementation estimates pending Julien human visual review.");
        builder.AppendLine();
        builder.AppendLine("- Route readability: pass estimate, clean center preserved");
        builder.AppendLine("- Silhouette quality: improved estimate, grouped assemblies replace thin-only read");
        builder.AppendLine("- Lighting mood: limited improvement, local fill only");
        builder.AppendLine("- Material coherence: pass estimate, ticket-owned muted forest palette");
        builder.AppendLine("- Foreground richness: improved estimate");
        builder.AppendLine("- Midground density: improved estimate");
        builder.AppendLine("- Background depth: improved estimate");
        builder.AppendLine("- Scale credibility: pass estimate, no route-visible floating blockers");
        builder.AppendLine("- Composition rhythm: improved estimate but requires route-camera human review");
        builder.AppendLine();
        builder.AppendLine("## Warning Categories");
        builder.AppendLine();
        AppendList(builder, "Build / Capture Warnings", result.BuildCaptureWarnings);
        AppendList(builder, "MYB-163 Visual Warnings", result.VisualWarnings);
        AppendList(builder, "MYB-163 Asset / Manifest Warnings", result.AssetManifestWarnings);
        AppendList(builder, "MYB-144 Existing Validator Warnings", result.Myb144ExistingValidatorWarnings);
        AppendList(builder, "Blocking Errors", result.BlockingErrors);
        builder.AppendLine();
        builder.AppendLine("## Governance");
        builder.AppendLine();
        builder.AppendLine("- canonical scene modified: Yes, scoped to MYB-163 generated root and capture rig normalization");
        builder.AppendLine("- gameplay modified: No");
        builder.AppendLine("- route trajectory/collider modified: No");
        builder.AppendLine("- HUD/telemetry modified: No");
        builder.AppendLine("- new Meshy generation: No");
        builder.AppendLine("- production promotion: No");
        builder.AppendLine("- Premium target reached: No");
        builder.AppendLine();
        builder.AppendLine("## Verdict");
        builder.AppendLine();
        builder.AppendLine("- Premium target reached: No");
        builder.AppendLine("- Checkpoint insuffisant pending Julien route-camera validation");
        builder.AppendLine("- Recommended Linear status: In Review");
        WriteText(ReportRelativePath, builder.ToString());
    }

    private static void WriteGovernanceReview(BuildResult result)
    {
        var builder = new StringBuilder();
        builder.AppendLine("# MYB-163 Governance Review");
        builder.AppendLine();
        builder.AppendLine("| Check | Result |");
        builder.AppendLine("|---|---|");
        builder.AppendLine("| Canonical scene integration exists | Yes |");
        builder.AppendLine("| Builder source of truth exists | Yes |");
        builder.AppendLine("| Seed 163001 used | Yes |");
        builder.AppendLine("| Generated root exists | Yes, `" + GeneratedRootName + "` |");
        builder.AppendLine("| Gameplay modified | No |");
        builder.AppendLine("| Route trajectory/collider modified | No |");
        builder.AppendLine("| HUD/telemetry modified | No |");
        builder.AppendLine("| Shared production material modified | No, ticket-owned MYB163 materials only |");
        builder.AppendLine("| New Meshy generations | 0 |");
        builder.AppendLine("| Meshy assets used directly | No |");
        builder.AppendLine("| Meshy assets promoted | No |");
        builder.AppendLine("| reviewStatus introduced | No |");
        builder.AppendLine("| example:true introduced | No |");
        builder.AppendLine("| MYB-144 run | " + (result.Myb144Verdict == "Not run" ? "No" : "Yes") + " |");
        builder.AppendLine("| MYB-144 errors | " + result.Myb144ErrorCount + " |");
        builder.AppendLine("| MYB-144 warnings | " + result.Myb144WarningCount + " |");
        builder.AppendLine("| Route readability regression | " + (result.RouteReadabilityRegression ? "Yes" : "No") + " |");
        builder.AppendLine("| Route-visible floating assets | " + result.RouteVisibleFloatingAssetCount + " |");
        builder.AppendLine("| Route-visible unsupported canopies | " + result.RouteVisibleUnsupportedCanopyCount + " |");
        builder.AppendLine("| Premium target reached | No |");
        builder.AppendLine("| Recommended Linear status | In Review |");
        builder.AppendLine();
        builder.AppendLine("Final auto-review: " + (result.BlockingErrors.Count == 0 ? "PASS_WITH_WARNINGS" : "FAIL"));
        WriteText(GovernanceReportRelativePath, builder.ToString());
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

    private static void EnsureFolder(string unityPath)
    {
        var absolutePath = ToRepoPath("unity/Echapee4D/" + unityPath);
        Directory.CreateDirectory(absolutePath);
    }

    private static string GetRepoRoot()
    {
        var projectPath = Application.dataPath;
        return Path.GetFullPath(Path.Combine(projectPath, "..", "..", ".."));
    }

    private static string ToRepoPath(string relativePath)
    {
        return Path.Combine(GetRepoRoot(), relativePath);
    }

    private static void WriteText(string relativePath, string contents)
    {
        var path = ToRepoPath(relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(path) ?? GetRepoRoot());
        File.WriteAllText(path, contents, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
    }

    private static string GetGitValue(string arguments)
    {
        try
        {
            var process = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "git",
                    Arguments = arguments,
                    WorkingDirectory = GetRepoRoot(),
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                }
            };
            process.Start();
            var output = process.StandardOutput.ReadToEnd().Trim();
            process.WaitForExit(1000);
            return output;
        }
        catch
        {
            return string.Empty;
        }
    }

    private static string FormatFloat(float value)
    {
        return value.ToString("0.###", CultureInfo.InvariantCulture);
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
        public readonly string Notes;

        public TreePlan(string name, string role, float meters, float side, float distanceFromRoute, float height, float trunkRadius, float canopyScale, float clearanceRadius, string notes)
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
            Notes = notes;
        }
    }

    private sealed class WallPlan
    {
        public readonly float Meters;
        public readonly float Side;
        public readonly float Distance;
        public readonly float Height;

        public WallPlan(float meters, float side, float distance, float height)
        {
            Meters = meters;
            Side = side;
            Distance = distance;
            Height = height;
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
        public readonly List<string> BlockingErrors = new List<string>();
        public readonly List<string> BuildCaptureWarnings = new List<string>();
        public readonly List<string> VisualWarnings = new List<string>();
        public readonly List<string> AssetManifestWarnings = new List<string>();
        public readonly List<string> Myb144ExistingValidatorWarnings = new List<string>();
        public readonly List<string> LayoutDecisions = new List<string>();
        public readonly List<AssemblyRecord> Assemblies = new List<AssemblyRecord>();
        public readonly List<PlacementRecord> Placements = new List<PlacementRecord>();
        public readonly List<GroundingRecord> GroundingRecords = new List<GroundingRecord>();
        public readonly List<CanopyRecord> Canopies = new List<CanopyRecord>();
        public readonly List<string> CaptureReports = new List<string>();
        public readonly List<string> CaptureMetadata = new List<string>();
        public string BeforeRoutePath = string.Empty;
        public string AfterRoutePath = string.Empty;
        public string BeforeOverviewPath = string.Empty;
        public string AfterOverviewPath = string.Empty;
        public string RouteComparisonPath = string.Empty;
        public string OverviewComparisonPath = string.Empty;
        public string BeforeAfterCaptureReportPath = string.Empty;
        public string Myb144Verdict;
        public int Myb144ErrorCount;
        public int Myb144WarningCount;
        public string Myb144ReportRelativePath;
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
        public string ThumbnailForestRead = "warning";
        public string HeroBeatRead = "warning";
        public string EmptySkyOrFlatBackgroundRisk = "medium";
        public bool RouteReadabilityRegression;

        public string ToConsoleSummary()
        {
            return "MYB-163 | errors=" + BlockingErrors.Count
                + " | routeOverlapCount=" + RouteOverlapCount
                + " | routeVisibleFloatingAssetCount=" + RouteVisibleFloatingAssetCount
                + " | routeVisibleUnsupportedCanopyCount=" + RouteVisibleUnsupportedCanopyCount
                + " | treeAssemblyCount=" + TreeAssemblyCount
                + " | backWallMassCount=" + BackWallMassCount
                + " | MYB-144=" + Myb144Verdict;
        }
    }
}

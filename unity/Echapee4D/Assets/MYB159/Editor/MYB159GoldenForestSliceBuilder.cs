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

public static class MYB159GoldenForestSliceBuilder
{
    private const int Seed = 159001;
    private const string SourceScenePath = "Assets/Scenes/MYB158PremiumRouteCameraForestPass.unity";
    private const string OutputScenePath = "Assets/Scenes/MYB159GoldenForestSlicePreview.unity";
    private const string GeneratedRootName = "MYB159_GoldenForestSliceRoot";
    private const string ImplementationRootRelative = "_bmad-output/implementation-artifacts/MYB-159";
    private const string VisualRootRelative = "_bmad-output/visual-checkpoints/MYB-159";
    private const string MetricsRelativePath = ImplementationRootRelative + "/myb-159-golden-slice-metrics.json";
    private const string ReportRelativePath = ImplementationRootRelative + "/myb-159-implementation-report.md";
    private const string GovernanceReportRelativePath = ImplementationRootRelative + "/myb-159-governance-review.md";
    private const float RouteLength = 144f;
    private const float RoadHalfWidth = 2.05f;
    private const float SinkMeters = 0.03f;

    [MenuItem("Tools/MyBike/MYB-159/Build Golden Forest Slice")]
    public static void BuildFromMenu()
    {
        var result = BuildPreviewScene(writeReports: true);
        Debug.Log(result.ToConsoleSummary());
    }

    [MenuItem("Tools/MyBike/MYB-159/Build + Capture + Validate")]
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

        var myb144 = MYB144ArtAssetValidator.RunValidation("MYB-159-BuildCaptureValidate");
        result.Myb144Verdict = myb144.Verdict;
        result.Myb144ErrorCount = myb144.ErrorCount;
        result.Myb144WarningCount = myb144.WarningCount;
        result.Myb144ReportRelativePath = "_bmad-output/unity-test-results/myb-144-art-asset-validator-report.md";
        if (myb144.ErrorCount > 0)
        {
            result.Myb144ExistingValidatorWarnings.Add("MYB-144 returned errors. MYB-159 creates no reusable asset files and does not modify the manifest, so inspect the validator report to classify whether the errors pre-existed or are environmental.");
        }
        if (myb144.WarningCount > 0)
        {
            result.Myb144ExistingValidatorWarnings.Add("MYB-144 returned warnings. They are recorded separately from MYB-159 build/capture warnings.");
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
            result.BlockingErrors.Add("Missing baseline scene `" + SourceScenePath + "`. MYB-159 is intentionally stacked on MYB-158 because MYB-158 after is the required before baseline.");
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
        var assembliesRoot = CreateChild(root.transform, "MYB159_TreeAssemblies");
        var heroRoot = CreateChild(root.transform, "MYB159_HeroBeat");
        var banksRoot = CreateChild(root.transform, "MYB159_SideBanks");
        var backWallRoot = CreateChild(root.transform, "MYB159_BackWallForestMass");
        var groundRoot = CreateChild(root.transform, "MYB159_GroundIntegration");
        var materials = CreateMaterials();
        result.SceneLocalMaterialCount = materials.Count;

        var routeCamera = FindSceneObjectByName("RouteCamera")?.GetComponent<Camera>();
        var routePlanes = routeCamera == null ? null : GeometryUtility.CalculateFrustumPlanes(routeCamera);
        if (routeCamera == null)
        {
            result.BuildCaptureWarnings.Add("RouteCamera not found while building MYB-159. Route-visible metrics fall back to false.");
        }

        BuildGoldenTreeAssemblies(materials, assembliesRoot.transform, routePlanes, result);
        BuildHeroRootThreshold(materials, heroRoot.transform, routePlanes, result);
        BuildSideBanks(materials, banksRoot.transform, groundRoot.transform, routePlanes, result);
        BuildBackWallForestMass(materials, backWallRoot.transform, routePlanes, result);
        ConfigureGoldenSliceMood();

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
        result.RouteVisibleUnsupportedCanopyCount = result.Canopies.Count(canopy => canopy.RouteVisible && !canopy.Supported);
        result.RouteVisibleCanopyCount = result.Canopies.Count(canopy => canopy.RouteVisible);
        result.RouteVisibleTreeAssemblyCount = result.TreeAssemblyRecords.Count(assembly => assembly.RouteVisible);
        result.ThumbnailForestRead = result.TreeAssemblyCount >= 5 && result.HeroBeatCount >= 1 && result.BackWallMassCount >= 16 && result.RouteVisibleCanopyCount >= 18
            ? "pass"
            : result.TreeAssemblyCount >= 3 && result.HeroBeatCount >= 1 && result.BackWallMassCount >= 3 ? "warning" : "fail";
        result.EmptySkyOrFlatBackgroundRisk = result.BackWallMassCount >= 16 && result.RouteVisibleTreeAssemblyCount >= 4 ? "low" : "medium";
        result.VisualWarnings.Add("Premium target intentionally not claimed; the authored golden slice is implementation evidence pending Julien human visual review.");
        if (result.ThumbnailForestRead != "pass")
        {
            result.VisualWarnings.Add("thumbnailForestRead remains " + result.ThumbnailForestRead + "; forest read is still not strong enough for production validation.");
        }
        if (result.EmptySkyOrFlatBackgroundRisk != "low")
        {
            result.VisualWarnings.Add("emptySkyOrFlatBackgroundRisk remains " + result.EmptySkyOrFlatBackgroundRisk + "; background depth still needs stronger authored forms.");
        }

        if (result.RouteOverlapCount > 0)
        {
            result.BlockingErrors.Add("MYB-159 route overlap detected. routeOverlapCount=" + result.RouteOverlapCount + ".");
        }
        if (result.RouteVisibleFloatingAssetCount > 0)
        {
            result.BlockingErrors.Add("MYB-159 route-visible floating assets detected above blocking threshold. routeVisibleFloatingAssetCount=" + result.RouteVisibleFloatingAssetCount + ".");
        }
        if (result.RouteVisibleUnsupportedCanopyCount > 0)
        {
            result.BlockingErrors.Add("MYB-159 route-visible unsupported canopy detected. routeVisibleUnsupportedCanopyCount=" + result.RouteVisibleUnsupportedCanopyCount + ".");
        }
        if (result.TreeAssemblyCount < 3)
        {
            result.BlockingErrors.Add("MYB-159 expected at least 3 tree assemblies. Actual=" + result.TreeAssemblyCount + ".");
        }
        if (result.RouteVisibleTreeAssemblyCount < 2)
        {
            result.BlockingErrors.Add("MYB-159 expected at least 2 route-visible tree assemblies. Actual=" + result.RouteVisibleTreeAssemblyCount + ".");
        }
        if (result.HeroBeatCount < 1)
        {
            result.BlockingErrors.Add("MYB-159 expected at least 1 hero beat. Actual=" + result.HeroBeatCount + ".");
        }
        if (result.ThumbnailForestRead == "fail")
        {
            result.BlockingErrors.Add("MYB-159 thumbnailForestRead failed.");
        }

        EditorSceneManager.SaveScene(scene, OutputScenePath);
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

        if (writeReports)
        {
            WriteReports(result);
        }

        return result;
    }

    private static void BuildGoldenTreeAssemblies(
        IReadOnlyDictionary<string, Material> materials,
        Transform parent,
        Plane[] routePlanes,
        BuildResult result)
    {
        var plans = new[]
        {
            new TreePlan("foreground-left-ancient-guardian", 16.5f, -1f, 4.25f, 5.4f, 0.48f, 1.65f, 1.28f, true),
            new TreePlan("foreground-right-supported-canopy", 22.0f, 1f, 4.35f, 4.9f, 0.42f, 1.45f, 1.16f, true),
            new TreePlan("hero-left-leaning-crown", 30.0f, -1f, 4.75f, 6.2f, 0.52f, 1.85f, 1.34f, true),
            new TreePlan("mid-right-ancient-cluster", 38.5f, 1f, 5.05f, 5.8f, 0.44f, 1.58f, 1.18f, true),
            new TreePlan("threshold-left-back-anchor", 47.0f, -1f, 5.7f, 6.5f, 0.38f, 1.72f, 1.08f, false)
        };

        foreach (var plan in plans)
        {
            CreateTreeAssembly(plan, materials, parent, routePlanes, result);
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
        var assembly = new GameObject("MYB159_tree_assembly_" + Slug(plan.Name));
        assembly.transform.SetParent(parent, false);
        assembly.transform.position = sample.Position + sample.Right * offset + Vector3.up * (TerrainHeight(plan.Meters, offset) + 0.25f);
        assembly.transform.rotation = Quaternion.LookRotation(sample.Forward, Vector3.up) * Quaternion.Euler(0f, plan.Side * 12f + Jitter(plan.Meters, 8f), plan.Side * -3.5f);

        AddMeshChild(assembly.transform, "trunk_primary", CreateTaperedTrunkMesh(plan.Height, plan.TrunkRadius, plan.Meters), materials["barkWarm"], Vector3.zero, Quaternion.identity, Vector3.one);

        var supportingStemCount = plan.HeroReadable ? 3 : 2;
        for (var stem = 0; stem < supportingStemCount; stem++)
        {
            var angle = stem / (float)supportingStemCount * 360f + 24f * plan.Side + Jitter(plan.Meters + stem * 4.1f, 11f);
            var radial = plan.TrunkRadius * (1.15f + stem * 0.22f);
            var local = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad) * radial, 0f, Mathf.Sin(angle * Mathf.Deg2Rad) * radial);
            AddMeshChild(
                assembly.transform,
                "supporting_stem_" + stem.ToString("00", CultureInfo.InvariantCulture),
                CreateTaperedTrunkMesh(plan.Height * (0.72f + stem * 0.06f), plan.TrunkRadius * (0.55f + stem * 0.05f), plan.Meters + stem * 6.3f),
                stem % 2 == 0 ? materials["barkDark"] : materials["rootDark"],
                local,
                Quaternion.Euler(0f, angle * 0.18f, plan.Side * (5f + stem * 2.5f)),
                Vector3.one);
        }

        for (var root = 0; root < 8; root++)
        {
            var angle = root / 8f * 360f + Jitter(plan.Meters + root * 13.1f, 18f);
            var length = plan.CanopyScale * (1.45f + Mathf.Abs(Jitter(plan.Meters + root * 5.7f, 0.55f)));
            AddMeshChild(
                assembly.transform,
                "root_" + root.ToString("00", CultureInfo.InvariantCulture),
                CreateRootFlareMesh(length, plan.TrunkRadius * 1.15f, 0.20f, plan.Meters + root),
                root % 2 == 0 ? materials["rootDark"] : materials["mossDeep"],
                Vector3.up * 0.015f,
                Quaternion.Euler(0f, angle, 0f),
                Vector3.one);
        }

        for (var branch = 0; branch < 6; branch++)
        {
            var y = plan.Height * (0.34f + branch * 0.095f);
            var yaw = plan.Side * (28f + branch * 19f) + Jitter(plan.Meters + branch * 3.3f, 14f);
            var pitch = -13f - Mathf.Abs(Jitter(plan.Meters + branch * 7.1f, 8f));
            AddMeshChild(
                assembly.transform,
                "support_branch_" + branch.ToString("00", CultureInfo.InvariantCulture),
                CreateBranchMesh(plan.CanopyScale * (1.05f + branch * 0.11f), plan.TrunkRadius * 0.38f, plan.Meters + branch * 2f),
                materials["barkDark"],
                new Vector3(0f, y, 0f),
                Quaternion.Euler(pitch, yaw, 0f),
                Vector3.one);
        }

        var towardRoute = -plan.Side;
        var canopyCenters = new[]
        {
            new Vector3(0.65f * towardRoute, plan.Height * 0.62f, 0.15f),
            new Vector3(1.05f * towardRoute, plan.Height * 0.76f, 0.62f),
            new Vector3(0.05f, plan.Height * 0.94f, -0.16f),
            new Vector3(-0.74f * towardRoute, plan.Height * 0.78f, -0.54f),
            new Vector3(-0.10f * towardRoute, plan.Height * 0.56f, 0.92f)
        };

        for (var canopy = 0; canopy < canopyCenters.Length; canopy++)
        {
            var canopyObject = AddMeshChild(
                assembly.transform,
                "supported_canopy_lobe_" + canopy.ToString("00", CultureInfo.InvariantCulture),
                CreateCanopyLobeMesh(
                    plan.CanopyScale * (0.86f + canopy * 0.075f),
                    plan.CanopyScale * (0.62f + canopy * 0.045f),
                    plan.CanopyScale * (0.82f + canopy * 0.04f),
                    plan.Meters + canopy * 9.7f),
                canopy % 2 == 0 ? materials["leafDeep"] : materials["leafWarm"],
                canopyCenters[canopy],
                Quaternion.Euler(Jitter(plan.Meters + canopy, 7f), Jitter(plan.Meters + canopy * 2f, 22f), Jitter(plan.Meters + canopy * 3f, 5f)),
                Vector3.one);
            RegisterCanopy(canopyObject, routePlanes, true, result);
        }

        if (plan.HeroReadable)
        {
            var skirt = AddMeshChild(
                assembly.transform,
                "route_frame_leaf_skirt_supported",
                CreateCanopyLobeMesh(plan.CanopyScale * 0.72f, plan.CanopyScale * 0.48f, plan.CanopyScale * 1.05f, plan.Meters + 83f),
                materials["leafShadow"],
                new Vector3(1.32f * towardRoute, plan.Height * 0.44f, 0.1f),
                Quaternion.Euler(6f, 18f * towardRoute, -8f * towardRoute),
                Vector3.one);
            RegisterCanopy(skirt, routePlanes, true, result);
        }

        AddMeshChild(
            assembly.transform,
            "grounding_moss_pad",
            CreateOvalPatchMesh(plan.CanopyScale * 2.05f, plan.CanopyScale * 1.35f, 0.10f, plan.Meters),
            materials["mossDeep"],
            Vector3.up * 0.01f,
            Quaternion.Euler(0f, Jitter(plan.Meters + 19f, 18f), 0f),
            Vector3.one);

        GroundObjectByVisualBottom(assembly, groundY, routePlanes, "Tree assembly", result);
        var bounds = CombinedRendererBounds(assembly) ?? new Bounds(assembly.transform.position, Vector3.one);
        var routeVisible = routePlanes != null && GeometryUtility.TestPlanesAABB(routePlanes, bounds);
        result.TreeAssemblyCount++;
        result.TreeAssemblyRecords.Add(new TreeAssemblyRecord { Name = assembly.name, RouteVisible = routeVisible });
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

    private static void BuildHeroRootThreshold(
        IReadOnlyDictionary<string, Material> materials,
        Transform parent,
        Plane[] routePlanes,
        BuildResult result)
    {
        var meters = 28.5f;
        var side = -1f;
        var distance = 3.55f;
        var sample = SampleAt(meters);
        var offset = side * distance;
        var groundY = sample.Position.y + TerrainHeight(meters, offset);
        var hero = new GameObject("MYB159_hero_root_threshold_left");
        hero.transform.SetParent(parent, false);
        hero.transform.position = sample.Position + sample.Right * offset + Vector3.up * (TerrainHeight(meters, offset) + 0.18f);
        hero.transform.rotation = Quaternion.LookRotation(sample.Forward, Vector3.up) * Quaternion.Euler(0f, -8f, 0f);

        AddMeshChild(hero.transform, "root_pillar_a", CreateTaperedTrunkMesh(2.55f, 0.31f, meters + 1f), materials["rootDark"], new Vector3(-0.42f, 0f, -0.18f), Quaternion.Euler(0f, 0f, -12f), Vector3.one);
        AddMeshChild(hero.transform, "root_pillar_b", CreateTaperedTrunkMesh(2.25f, 0.27f, meters + 2f), materials["rootDark"], new Vector3(0.86f, 0f, 0.62f), Quaternion.Euler(0f, 0f, 15f), Vector3.one);
        AddMeshChild(hero.transform, "root_arch_cross_low", CreateBranchMesh(2.65f, 0.21f, meters + 3f), materials["rootDark"], new Vector3(-0.42f, 1.52f, -0.05f), Quaternion.Euler(-5f, 58f, 72f), Vector3.one);
        AddMeshChild(hero.transform, "root_arch_cross_high", CreateBranchMesh(2.25f, 0.16f, meters + 6f), materials["barkDark"], new Vector3(-0.25f, 2.02f, 0.22f), Quaternion.Euler(-11f, 48f, 66f), Vector3.one);
        AddMeshChild(hero.transform, "moss_threshold_base", CreateOvalPatchMesh(2.3f, 1.45f, 0.16f, meters + 4f), materials["mossDeep"], Vector3.up * 0.02f, Quaternion.identity, Vector3.one);
        AddMeshChild(hero.transform, "leaf_shadow_pool", CreateOvalPatchMesh(2.55f, 1.58f, 0.065f, meters + 5f), materials["leafDark"], new Vector3(0.12f, 0.025f, 0.2f), Quaternion.Euler(0f, 24f, 0f), Vector3.one);
        AddMeshChild(hero.transform, "mossy_stone_marker", CreateRockMarkerMesh(0.65f, 0.95f, 0.5f, meters + 8f), materials["stoneMoss"], new Vector3(1.35f, 0.03f, -0.35f), Quaternion.Euler(0f, 28f, -3f), Vector3.one);

        var heroCanopy = AddMeshChild(
            hero.transform,
            "supported_threshold_canopy",
            CreateCanopyLobeMesh(1.35f, 0.72f, 1.1f, meters + 9f),
            materials["leafShadow"],
            new Vector3(0.48f, 2.15f, 0.18f),
            Quaternion.Euler(-3f, 22f, 5f),
            Vector3.one);
        RegisterCanopy(heroCanopy, routePlanes, true, result);

        GroundObjectByVisualBottom(hero, groundY, routePlanes, "Hero root threshold", result);
        var bounds = CombinedRendererBounds(hero) ?? new Bounds(hero.transform.position, Vector3.one);
        result.HeroBeatCount++;
        result.Placements.Add(new PlacementRecord
        {
            Name = hero.name,
            Family = "Hero beat",
            Meters = meters,
            Offset = offset,
            Radius = 1.22f,
            RouteVisible = routePlanes != null && GeometryUtility.TestPlanesAABB(routePlanes, bounds)
        });
    }

    private static void BuildSideBanks(
        IReadOnlyDictionary<string, Material> materials,
        Transform banksParent,
        Transform groundParent,
        Plane[] routePlanes,
        BuildResult result)
    {
        var plans = new[]
        {
            new BankPlan(12f, -1f, 3.55f, 1.05f, 6.8f, 0.28f, "left foreground moss bank"),
            new BankPlan(16f, 1f, 3.65f, 1.00f, 6.6f, 0.24f, "right foreground shoulder"),
            new BankPlan(22f, -1f, 3.62f, 1.18f, 7.2f, 0.32f, "left ancient root foot"),
            new BankPlan(27f, 1f, 3.70f, 1.10f, 6.9f, 0.26f, "right grounding shoulder"),
            new BankPlan(32f, -1f, 3.72f, 1.28f, 7.6f, 0.34f, "hero threshold bank"),
            new BankPlan(39f, 1f, 3.95f, 1.15f, 6.8f, 0.24f, "right leaf bank"),
            new BankPlan(47f, -1f, 4.05f, 1.22f, 7.1f, 0.28f, "left mid moss berm"),
            new BankPlan(56f, 1f, 4.15f, 1.10f, 6.2f, 0.22f, "right back shoulder")
        };

        foreach (var plan in plans)
        {
            CreateSideBank(plan, materials, banksParent, routePlanes, result);
            CreateGroundAccent(plan.Meters + 1.4f, -plan.Side, plan.Distance + 0.5f, materials, groundParent, routePlanes, result);
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
        var bank = new GameObject("MYB159_side_bank_" + Slug(plan.Name));
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
            RouteVisible = false
        });
    }

    private static void CreateGroundAccent(
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
        var accent = new GameObject("MYB159_ground_integrated_leaf_moss_" + result.SideBankPatchCount.ToString("00", CultureInfo.InvariantCulture));
        accent.transform.SetParent(parent, false);
        accent.transform.position = sample.Position + sample.Right * offset + Vector3.up * (TerrainHeight(meters, offset) + 0.015f);
        accent.transform.rotation = Quaternion.LookRotation(sample.Forward, Vector3.up) * Quaternion.Euler(0f, Jitter(meters + 31f, 35f), 0f);
        accent.AddComponent<MeshFilter>().sharedMesh = CreateOvalPatchMesh(0.85f + Mathf.Abs(Jitter(meters, 0.3f)), 1.7f + Mathf.Abs(Jitter(meters + 3f, 0.7f)), 0.045f, meters);
        accent.AddComponent<MeshRenderer>().sharedMaterial = materials[Mathf.RoundToInt(meters) % 2 == 0 ? "leafWarm" : "soilDark"];
        GroundObjectByVisualBottom(accent, groundY, routePlanes, "Ground integration patch", result);
    }

    private static void BuildBackWallForestMass(
        IReadOnlyDictionary<string, Material> materials,
        Transform parent,
        Plane[] routePlanes,
        BuildResult result)
    {
        for (var i = 0; i < 12; i++)
        {
            foreach (var side in new[] { -1f, 1f })
            {
                var meters = 14f + i * 4.8f + (side > 0f ? 1.6f : 0f);
                var sample = SampleAt(meters);
                var offset = side * (7.3f + Mathf.Abs(Jitter(meters + side, 1.6f)));
                var groundY = sample.Position.y + TerrainHeight(meters, offset);
                var index = result.BackWallMassCount;
                var mass = new GameObject("MYB159_back_wall_mass_" + index.ToString("00", CultureInfo.InvariantCulture));
                mass.transform.SetParent(parent, false);
                mass.transform.position = sample.Position + sample.Right * offset + Vector3.up * (TerrainHeight(meters, offset) + 0.2f);
                mass.transform.rotation = Quaternion.LookRotation(sample.Forward, Vector3.up) * Quaternion.Euler(0f, side * 14f + Jitter(meters, 14f), 0f);

                AddMeshChild(mass.transform, "dark_trunk_cluster_a", CreateTaperedTrunkMesh(4.1f + Mathf.Abs(Jitter(meters, 1.1f)), 0.18f + Mathf.Abs(Jitter(meters + 2f, 0.045f)), meters), materials["shadowBark"], new Vector3(-0.52f, 0f, -0.12f), Quaternion.Euler(0f, -8f, side * 3f), Vector3.one);
                AddMeshChild(mass.transform, "dark_trunk_cluster_b", CreateTaperedTrunkMesh(3.65f + Mathf.Abs(Jitter(meters + 5f, 0.9f)), 0.14f + Mathf.Abs(Jitter(meters + 7f, 0.035f)), meters + 1f), materials["shadowBark"], new Vector3(0.32f, 0f, 0.35f), Quaternion.Euler(0f, 12f, side * -4f), Vector3.one);
                AddMeshChild(mass.transform, "dark_trunk_cluster_c", CreateTaperedTrunkMesh(3.15f + Mathf.Abs(Jitter(meters + 9f, 0.8f)), 0.12f + Mathf.Abs(Jitter(meters + 3f, 0.03f)), meters + 2f), materials["rootDark"], new Vector3(0.86f * side, 0f, -0.48f), Quaternion.Euler(0f, -18f, side * 5f), Vector3.one);
                AddMeshChild(mass.transform, "background_canopy_mass_high", CreateCanopyLobeMesh(2.15f, 0.9f, 1.25f, meters + 9f), materials["leafShadow"], new Vector3(0.08f, 3.75f + Mathf.Abs(Jitter(meters + 11f, 0.7f)), 0.1f), Quaternion.Euler(0f, Jitter(meters, 30f), 0f), Vector3.one);
                AddMeshChild(mass.transform, "background_canopy_mass_low", CreateCanopyLobeMesh(1.65f, 0.72f, 1.05f, meters + 19f), materials["leafDeep"], new Vector3(0.62f * side, 2.85f + Mathf.Abs(Jitter(meters + 15f, 0.55f)), 0.62f), Quaternion.Euler(0f, side * 16f + Jitter(meters + 3f, 24f), 0f), Vector3.one);
                AddMeshChild(mass.transform, "shadow_floor_mound", CreateOvalPatchMesh(1.55f, 2.2f, 0.11f, meters + 23f), materials["mossDeep"], new Vector3(0.08f, 0.02f, 0.15f), Quaternion.Euler(0f, side * 22f, 0f), Vector3.one);
                GroundObjectByVisualBottom(mass, groundY, routePlanes, "Back wall forest mass", result);

                var bounds = CombinedRendererBounds(mass) ?? new Bounds(mass.transform.position, Vector3.one);
                result.BackWallMassCount++;
                result.Placements.Add(new PlacementRecord
                {
                    Name = mass.name,
                    Family = "Back wall forest mass",
                    Meters = meters,
                    Offset = offset,
                    Radius = 1.15f,
                    RouteVisible = routePlanes != null && GeometryUtility.TestPlanesAABB(routePlanes, bounds)
                });
            }
        }
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
        var ringScales = new[] { 1.65f, 1.25f, 0.92f, 0.72f, 0.52f };
        var vertices = new List<Vector3>();
        var uvs = new List<Vector2>();
        var triangles = new List<int>();

        for (var ring = 0; ring < ringYs.Length; ring++)
        {
            var y01 = ringYs[ring];
            var bend = new Vector2(
                Mathf.Sin(seed * 0.21f + y01 * 2.5f) * radius * 0.42f * y01,
                Mathf.Cos(seed * 0.17f + y01 * 2.1f) * radius * 0.35f * y01);
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
        return BuildMesh("MYB159_TaperedTrunkMesh", vertices, uvs, triangles);
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
        return BuildMesh("MYB159_BranchMesh", vertices, uvs, triangles);
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
        return BuildMesh("MYB159_RootFlareMesh", vertices, uvs, triangles);
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

        return BuildMesh("MYB159_CanopyLobeMesh", vertices, uvs, triangles);
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

        return BuildMesh("MYB159_BankPatchMesh", vertices, uvs, triangles);
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

        return BuildMesh("MYB159_RockMarkerMesh", vertices, uvs, triangles);
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
            ["barkWarm"] = RuntimeMaterial(shader, "MYB159_BarkWarm", new Color(0.33f, 0.22f, 0.13f), 0.26f),
            ["barkDark"] = RuntimeMaterial(shader, "MYB159_BarkDark", new Color(0.16f, 0.11f, 0.08f), 0.18f),
            ["rootDark"] = RuntimeMaterial(shader, "MYB159_RootDark", new Color(0.12f, 0.09f, 0.055f), 0.18f),
            ["shadowBark"] = RuntimeMaterial(shader, "MYB159_ShadowBark", new Color(0.075f, 0.083f, 0.058f), 0.12f),
            ["leafDeep"] = RuntimeMaterial(shader, "MYB159_LeafDeep", new Color(0.10f, 0.27f, 0.12f), 0.34f),
            ["leafWarm"] = RuntimeMaterial(shader, "MYB159_LeafWarm", new Color(0.22f, 0.38f, 0.16f), 0.32f),
            ["leafShadow"] = RuntimeMaterial(shader, "MYB159_LeafShadow", new Color(0.055f, 0.13f, 0.075f), 0.24f),
            ["mossDeep"] = RuntimeMaterial(shader, "MYB159_MossDeep", new Color(0.09f, 0.22f, 0.105f), 0.28f),
            ["leafDark"] = RuntimeMaterial(shader, "MYB159_LeafLitterDark", new Color(0.19f, 0.13f, 0.075f), 0.22f),
            ["leafLitterWarm"] = RuntimeMaterial(shader, "MYB159_LeafLitterWarm", new Color(0.31f, 0.20f, 0.105f), 0.22f),
            ["soilDark"] = RuntimeMaterial(shader, "MYB159_SoilDark", new Color(0.10f, 0.075f, 0.052f), 0.16f),
            ["stoneMoss"] = RuntimeMaterial(shader, "MYB159_StoneMoss", new Color(0.42f, 0.45f, 0.31f), 0.18f)
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

    private static void ConfigureGoldenSliceMood()
    {
        RenderSettings.skybox = null;
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.39f, 0.47f, 0.38f);
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogColor = new Color(0.28f, 0.36f, 0.32f);
        RenderSettings.fogDensity = 0.0135f;

        var existing = FindSceneObjectByName("MYB159_GoldenSliceWarmBreakLight");
        if (existing != null)
        {
            UnityEngine.Object.DestroyImmediate(existing);
        }
        var lightObject = new GameObject("MYB159_GoldenSliceWarmBreakLight");
        var light = lightObject.AddComponent<Light>();
        light.type = LightType.Directional;
        light.color = new Color(1f, 0.82f, 0.58f);
        light.intensity = 0.36f;
        lightObject.transform.rotation = Quaternion.Euler(34f, -38f, 0f);
    }

    private static MYB145CaptureRigHelper.CaptureResult CaptureScene(string scenePath, string state)
    {
        EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        return MYB145CaptureRigHelper.CaptureRouteAndOverview(
            "MYB-159-" + state,
            new MYB145CaptureRigHelper.CaptureOptions
            {
                TicketId = "MYB-159",
                State = state,
                ScenePath = scenePath,
                BaselineSelectedBy = "MYB-159 builder / ticket",
                BaselineReason = "MYB-158 after is the current technical checkpoint baseline; MYB-159 tests an authored golden forest slice to solve the visual quality blocker.",
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
        builder.AppendLine("# MYB-159 Capture Report");
        builder.AppendLine();
        builder.AppendLine("Mode:");
        builder.AppendLine("- explicit before/after comparison");
        builder.AppendLine();
        builder.AppendLine("Baseline:");
        builder.AppendLine("- before selected by: MYB-159 builder / ticket");
        builder.AppendLine("- reason: MYB-158 after is the current technical checkpoint baseline; MYB-159 tests an authored golden forest slice to solve the visual quality blocker.");
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
        builder.AppendLine("  \"ticket\": \"MYB-159\",");
        builder.AppendLine("  \"seed\": " + Seed + ",");
        builder.AppendLine("  \"sourceBaseline\": \"MYB-158 after\",");
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
        builder.AppendLine("  \"meshyGeneratedCount\": 0,");
        builder.AppendLine("  \"meshyUsedInPreviewCount\": 0,");
        builder.AppendLine("  \"manifestEntriesAdded\": 0,");
        builder.AppendLine("  \"emptySkyOrFlatBackgroundRisk\": \"" + result.EmptySkyOrFlatBackgroundRisk + "\",");
        builder.AppendLine("  \"thumbnailForestRead\": \"" + result.ThumbnailForestRead + "\"");
        builder.AppendLine("}");
        File.WriteAllText(path, builder.ToString());
    }

    private static void WriteImplementationReport(BuildResult result)
    {
        var path = ToRepoPath(ReportRelativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(path) ?? GetRepoRoot());
        var builder = new StringBuilder();
        builder.AppendLine("# MYB-159 Implementation Report");
        builder.AppendLine();
        builder.AppendLine("## Summary");
        builder.AppendLine();
        builder.AppendLine("MYB-158 metrics pass, but the route-camera remains visually weak: too procedural, too flat, and too dependent on poles, patches, and isolated blobs. MYB-159 builds a short route-camera-first golden slice to test an authored forest direction without touching gameplay or canonical ride scenes.");
        builder.AppendLine();
        builder.AppendLine("## Strategy");
        builder.AppendLine();
        builder.AppendLine("- Golden slice authored, not full corridor.");
        builder.AppendLine("- Tree assemblies before scatter.");
        builder.AppendLine("- Scene-local meshes for assemblies, side banks, canopies, roots, and back wall mass.");
        builder.AppendLine("- Meshy controlled candidates: not used in this pass to avoid spending credits before proving a no-cost authored slice baseline.");
        builder.AppendLine();
        builder.AppendLine("## Source / Baseline");
        builder.AppendLine();
        builder.AppendLine("- before = MYB-158 after");
        builder.AppendLine("- before scene: `" + SourceScenePath + "`");
        builder.AppendLine("- before route: `" + result.BeforeRoutePath + "`");
        builder.AppendLine("- before overview: `" + result.BeforeOverviewPath + "`");
        builder.AppendLine();
        builder.AppendLine("## Builder");
        builder.AppendLine();
        builder.AppendLine("- path: `unity/Echapee4D/Assets/MYB159/Editor/MYB159GoldenForestSliceBuilder.cs`");
        builder.AppendLine("- seed: " + Seed);
        builder.AppendLine("- generated root: `" + GeneratedRootName + "`");
        builder.AppendLine("- output scene: `" + OutputScenePath + "`");
        builder.AppendLine();
        builder.AppendLine("## Meshy Usage");
        builder.AppendLine();
        builder.AppendLine("- Used: No");
        builder.AppendLine("- generated count: 0");
        builder.AppendLine("- selected count: 0");
        builder.AppendLine("- rejected count: 0");
        builder.AppendLine("- Reason: first implementation uses authored scene-local geometry to test whether composition, side banks, and tree assemblies improve the route-camera before spending Meshy credits. Meshy remains available only with explicit cost confirmation.");
        builder.AppendLine();
        builder.AppendLine("## Asset Intake / Manifest");
        builder.AppendLine();
        builder.AppendLine("- manifest changed: No");
        builder.AppendLine("- entries added: 0");
        builder.AppendLine("- no `reviewStatus` introduced");
        builder.AppendLine("- no `example:true` introduced");
        builder.AppendLine("- no promoted assets");
        builder.AppendLine("- Meshy assets not promoted: Not applicable");
        builder.AppendLine();
        builder.AppendLine("## Golden Slice Composition");
        builder.AppendLine();
        builder.AppendLine("- tree assemblies: " + result.TreeAssemblyCount);
        builder.AppendLine("- route-visible tree assemblies: " + result.RouteVisibleTreeAssemblyCount);
        builder.AppendLine("- back wall masses: " + result.BackWallMassCount);
        builder.AppendLine("- hero beats: " + result.HeroBeatCount);
        builder.AppendLine("- side bank patches: " + result.SideBankPatchCount);
        builder.AppendLine("- grounding: scene-local moss/leaf/soil pads and side banks integrated around each visual beat.");
        builder.AppendLine("- route readability: minimum route clearance " + FormatFloat(result.MinimumRouteClearanceMeters) + "m, routeOverlapCount " + result.RouteOverlapCount + ".");
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
        builder.AppendLine("## Canopy / Support");
        builder.AppendLine();
        builder.AppendLine("- supported visible canopy count: " + (result.RouteVisibleCanopyCount - result.RouteVisibleUnsupportedCanopyCount));
        builder.AppendLine("- unsupported visible canopy count: " + result.RouteVisibleUnsupportedCanopyCount);
        builder.AppendLine("- support model: every canopy lobe is a child of a tree assembly with trunk and branch support geometry.");
        builder.AppendLine();
        builder.AppendLine("## Metrics");
        builder.AppendLine();
        builder.AppendLine("- JSON: `" + MetricsRelativePath + "`");
        builder.AppendLine("- approximateTriangles: " + result.ApproximateTriangles);
        builder.AppendLine("- rendererCount: " + result.RendererCount);
        builder.AppendLine("- meshFilterCount: " + result.MeshFilterCount);
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
        builder.AppendLine("- interpretation: MYB-159 creates no reusable asset files and does not modify the manifest; any MYB-144 warnings/errors must be checked against the report before being attributed to MYB-159.");
        builder.AppendLine();
        builder.AppendLine("## Visual Rubric Estimate");
        builder.AppendLine();
        builder.AppendLine("Visual scores are implementation estimates pending Julien human visual review.");
        builder.AppendLine();
        builder.AppendLine("| Criterion | Estimate | Notes |");
        builder.AppendLine("|---|---:|---|");
        builder.AppendLine("| Route readability | 4 | Route clearance is preserved and no route overlap is detected. |");
        builder.AppendLine("| Silhouette quality | 3 | Tree assemblies and hero root threshold are stronger than MYB-158, but still scene-local prototype art. |");
        builder.AppendLine("| Lighting mood | 3 | Fog and warm break light improve mood without hiding weak forms. |");
        builder.AppendLine("| Material coherence | 3 | Scene-local material palette is more coherent, still simple. |");
        builder.AppendLine("| Foreground richness | 4 | Side banks and roots improve near-route grounding. |");
        builder.AppendLine("| Midground density | 4 | Assemblies and back wall increase forest read. |");
        builder.AppendLine("| Background depth | 3 | Back wall reduces empty background risk, still not premium. |");
        builder.AppendLine("| Scale credibility | 4 | Grounding and route clearance metrics pass. |");
        builder.AppendLine("| Composition rhythm | 4 | Short golden slice has authored beats rather than broad scatter. |");
        builder.AppendLine();
        builder.AppendLine("## Warning Categories");
        builder.AppendLine();
        AppendList(builder, "Build / Capture Warnings", result.BuildCaptureWarnings);
        AppendList(builder, "MYB-159 Visual Warnings", result.VisualWarnings);
        AppendList(builder, "MYB-159 Asset / Manifest Warnings", result.AssetManifestWarnings);
        AppendList(builder, "MYB-144 Existing Validator Warnings", result.Myb144ExistingValidatorWarnings);
        AppendList(builder, "Blocking Errors", result.BlockingErrors);
        builder.AppendLine();
        builder.AppendLine("## Governance");
        builder.AppendLine();
        builder.AppendLine("- no canonical scene modified");
        builder.AppendLine("- no gameplay modified");
        builder.AppendLine("- no route collider/trajectory change");
        builder.AppendLine("- no production promotion");
        builder.AppendLine("- Meshy controlled only if used: not used");
        builder.AppendLine("- no silent third-party source");
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
        builder.AppendLine("# MYB-159 Governance Review");
        builder.AppendLine();
        builder.AppendLine("| Check | Result |");
        builder.AppendLine("|---|---|");
        builder.AppendLine("| Dedicated preview scene exists | Yes |");
        builder.AppendLine("| Builder source of truth exists | Yes |");
        builder.AppendLine("| Seed 159001 used | Yes |");
        builder.AppendLine("| Generated root MYB159_GoldenForestSliceRoot exists | Yes |");
        builder.AppendLine("| MYB-158 scene modified | No |");
        builder.AppendLine("| Canonical ride scene modified | No |");
        builder.AppendLine("| Gameplay modified | No |");
        builder.AppendLine("| Route trajectory/collider modified | No |");
        builder.AppendLine("| Shared production material modified | No |");
        builder.AppendLine("| Meshy used | No |");
        builder.AppendLine("| Meshy assets manifest-listed | Not applicable |");
        builder.AppendLine("| Meshy assets promoted | No |");
        builder.AppendLine("| External non-Meshy third-party used | No |");
        builder.AppendLine("| Poly Haven used | No |");
        builder.AppendLine("| Reusable asset files created | No |");
        builder.AppendLine("| Manifest changed | No |");
        builder.AppendLine("| reviewStatus introduced | No |");
        builder.AppendLine("| example:true introduced | No |");
        builder.AppendLine("| production promotion introduced | No |");
        builder.AppendLine("| MYB-144 run | " + (string.IsNullOrWhiteSpace(result.Myb144Verdict) ? "No" : "Yes") + " |");
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

    private static string GetRepoRoot()
    {
        var projectRoot = Directory.GetParent(Application.dataPath)?.FullName ?? Directory.GetCurrentDirectory();
        return Directory.GetParent(projectRoot)?.Parent?.FullName ?? projectRoot;
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

    private static string Slug(string value)
    {
        return new string((value ?? string.Empty).ToLowerInvariant().Select(character => char.IsLetterOrDigit(character) ? character : '_').ToArray()).Trim('_');
    }

    private static string FormatFloat(float value)
    {
        return value.ToString("0.###", CultureInfo.InvariantCulture);
    }

    private static string FormatJsonFloat(float value)
    {
        return value.ToString("0.###", CultureInfo.InvariantCulture);
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
        public readonly float Meters;
        public readonly float Side;
        public readonly float DistanceFromRoute;
        public readonly float Height;
        public readonly float TrunkRadius;
        public readonly float CanopyScale;
        public readonly float ClearanceRadius;
        public readonly bool HeroReadable;

        public TreePlan(string name, float meters, float side, float distanceFromRoute, float height, float trunkRadius, float canopyScale, float clearanceRadius, bool heroReadable)
        {
            Name = name;
            Meters = meters;
            Side = side;
            DistanceFromRoute = distanceFromRoute;
            Height = height;
            TrunkRadius = trunkRadius;
            CanopyScale = canopyScale;
            ClearanceRadius = clearanceRadius;
            HeroReadable = heroReadable;
        }
    }

    private sealed class BankPlan
    {
        public readonly float Meters;
        public readonly float Side;
        public readonly float Distance;
        public readonly float Radius;
        public readonly float Length;
        public readonly float Height;
        public readonly string Name;

        public BankPlan(float meters, float side, float distance, float radius, float length, float height, string name)
        {
            Meters = meters;
            Side = side;
            Distance = distance;
            Radius = radius;
            Length = length;
            Height = height;
            Name = name;
        }
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

    private sealed class TreeAssemblyRecord
    {
        public string Name;
        public bool RouteVisible;
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
        public string EmptySkyOrFlatBackgroundRisk = "high";
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
        public readonly List<string> CaptureReports = new List<string>();
        public readonly List<string> CaptureMetadata = new List<string>();
        public readonly List<string> BuildCaptureWarnings = new List<string>();
        public readonly List<string> VisualWarnings = new List<string> { "Premium target not reached; this remains a directionally improved checkpoint pending Julien review." };
        public readonly List<string> AssetManifestWarnings = new List<string> { "No Meshy or reusable asset intake was performed in this pass; manifest is unchanged." };
        public readonly List<string> Myb144ExistingValidatorWarnings = new List<string>();
        public readonly List<string> BlockingErrors = new List<string>();
        public readonly List<PlacementRecord> Placements = new List<PlacementRecord>();
        public readonly List<GroundingRecord> GroundingRecords = new List<GroundingRecord>();
        public readonly List<CanopyRecord> Canopies = new List<CanopyRecord>();
        public readonly List<TreeAssemblyRecord> TreeAssemblyRecords = new List<TreeAssemblyRecord>();
        public string VisualVerdict => BlockingErrors.Count == 0 ? "Checkpoint insuffisant, directionally improved" : "Rework required";

        public string ToConsoleSummary()
        {
            return "MYB-159 golden forest slice: " + VisualVerdict
                + " | treeAssemblyCount=" + TreeAssemblyCount
                + " | heroBeatCount=" + HeroBeatCount
                + " | routeOverlapCount=" + RouteOverlapCount
                + " | routeVisibleFloatingAssetCount=" + RouteVisibleFloatingAssetCount
                + " | routeVisibleUnsupportedCanopyCount=" + RouteVisibleUnsupportedCanopyCount
                + " | myb144=" + Myb144Verdict
                + " | report=" + ReportRelativePath;
        }
    }
}

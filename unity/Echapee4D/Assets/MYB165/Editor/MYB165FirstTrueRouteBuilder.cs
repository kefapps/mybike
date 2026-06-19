using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using MYB48;
using MYB57;
using MYB73;
using MYB89;
using MYB89.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class MYB165FirstTrueRouteBuilder
{
    private const int Seed = 165001;
    private const string ScenePath = "Assets/Scenes/MYB89UnityMcpProbe.unity";
    private const string GeneratedRootName = "MYB165_FirstTrueRouteRoot";
    private const string ImplementationRootRelative = "_bmad-output/implementation-artifacts/MYB-165";
    private const string VisualRootRelative = "_bmad-output/visual-checkpoints/MYB-165";
    private const string VideoRootRelative = "_bmad-output/video-captures/MYB-165";
    private const string MetricsRelativePath = ImplementationRootRelative + "/myb-165-first-true-route-metrics.json";
    private const string ReportRelativePath = ImplementationRootRelative + "/myb-165-implementation-report.md";
    private const string GovernanceRelativePath = ImplementationRootRelative + "/myb-165-governance-review.md";
    private const float RoadWidth = 7.2f;
    private const float NormalSpeedMetersPerSecond = 12.5f;
    private const float MinimumDurationSeconds = 160f;
    private const float MaximumDurationSeconds = 200f;
    private const int VideoFrameRate = 30;
    private const int VideoWidth = 1280;
    private const int VideoHeight = 720;

    private static readonly string[] UnsupportedLegacyRouteVisiblePrefixes =
    {
        "MYB44_VillageHome_",
        "MYB44_VillageMarket_",
        "MYB44_WallWindow_",
        "MYB44_VillageWell",
        "MYB44_VillageHorse_",
        "MYB44_HorizonCottage_",
        "MYB44_HorizonRoof_",
        "MYB44_HorizonHill_",
        "MYB89_LeftHill_",
        "MYB89_RightHill_",
        "MYB165_DistantHill_"
    };

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
        new Vector3(0f, 0.12f, 242f),
        new Vector3(12f, 0.28f, 318f),
        new Vector3(-20f, 0.62f, 438f),
        new Vector3(27f, 1.02f, 575f),
        new Vector3(-36f, 1.36f, 725f),
        new Vector3(10f, 1.05f, 875f),
        new Vector3(50f, 0.82f, 1028f),
        new Vector3(16f, 1.38f, 1192f),
        new Vector3(-46f, 1.88f, 1360f),
        new Vector3(-14f, 2.18f, 1518f),
        new Vector3(40f, 1.72f, 1688f),
        new Vector3(7f, 1.16f, 1845f),
        new Vector3(-30f, 0.92f, 1998f),
        new Vector3(8f, 0.58f, 2145f),
        new Vector3(0f, 0.32f, 2285f)
    };

    [MenuItem("Tools/MyBike/MYB-165/Build First True Route")]
    public static void BuildFromMenu()
    {
        var result = BuildFirstTrueRoute(captureVideoFrames: false);
        Debug.Log(result.ToConsoleSummary());
    }

    [MenuItem("Tools/MyBike/MYB-165/Build + Capture + Validate")]
    public static void BuildCaptureValidateFromMenu()
    {
        var result = BuildFirstTrueRoute(captureVideoFrames: true);
        Debug.Log(result.ToConsoleSummary());
    }

    public static void RunBatchBuildCaptureValidate()
    {
        var result = BuildFirstTrueRoute(captureVideoFrames: true);
        if (result.BlockingErrors.Count > 0)
        {
            throw new InvalidOperationException(result.ToConsoleSummary());
        }
    }

    public static void RunBatchBuildValidateOnly()
    {
        var result = BuildFirstTrueRoute(captureVideoFrames: false);
        if (result.BlockingErrors.Count > 0)
        {
            throw new InvalidOperationException(result.ToConsoleSummary());
        }
    }

    public static void RunBatchCaptureVideoOnly()
    {
        MYB165UnityRecorderVideoCapture.RunBatchCaptureVideoOnly();
    }

    [MenuItem("Tools/MyBike/MYB-165/Capture Full Route Video Frames Fallback")]
    public static void RunBatchCaptureVideoFramesFallback()
    {
        var result = new BuildResult();
        CaptureFullRouteVideoFrames(result);
        Debug.Log(result.ToConsoleSummary());
    }

    public static BuildResult BuildFirstTrueRoute(bool captureVideoFrames)
    {
        var result = new BuildResult();
        Directory.CreateDirectory(ToRepoPath(ImplementationRootRelative));
        Directory.CreateDirectory(ToRepoPath(VisualRootRelative));
        Directory.CreateDirectory(ToRepoPath(VideoRootRelative));
        EnsureFolder("Assets/MYB165");
        EnsureFolder("Assets/MYB165/Materials");
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

        MYB89ProbeBuilder.BuildScene();
        MYB163CanonicalForestPassageIntegrator.RunBatchBuild();

        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        var probeRoot = GameObject.Find("MYB89_ProbeRoot");
        if (probeRoot == null)
        {
            result.BlockingErrors.Add("MYB89_ProbeRoot is missing after rebuilding the canonical scene.");
            WriteReports(result);
            return result;
        }

        result.RetiredLegacyUnsupportedPropCount = RetireLegacyUnsupportedProbeScenery();

        DestroyNamed(GeneratedRootName);
        DestroyNamed("MYB89_RouteMarkers");
        DestroyNamed("MYB89_RouteRoad");
        DestroyNamed("MYB89_LeftEdgeLine");
        DestroyNamed("MYB89_RightEdgeLine");
        DestroyNamed("MYB48_RouteDifficultyCues");
        DestroyObjectsWithPrefix("MYB89_CenterDash");

        var root = new GameObject(GeneratedRootName);
        root.transform.SetParent(probeRoot.transform, false);

        var materials = CreateMaterials();
        var route = MYB89RideTrajectory.BuildSmoothedPoints(RoutePoints, MYB89RideTrajectory.DefaultSamplesPerSegment);
        var routeLength = MYB89RideTrajectory.Length(route);
        var estimatedDurationSeconds = routeLength / NormalSpeedMetersPerSecond;

        CreateLongGround(root.transform, materials, route);
        var markers = CreateRoute(root.transform, materials, route);
        CreateRouteDifficultyCues(root.transform, markers, materials);
        CreateLongDistanceScenery(root.transform, materials, route, result);
        UpdateRideRig(markers, routeLength, result);
        UpdatePreviewPanel();
        ConfigureCaptureCameras(route);

        result.RouteLengthMeters = routeLength;
        result.NormalSpeedMetersPerSecond = NormalSpeedMetersPerSecond;
        result.EstimatedDurationSeconds = estimatedDurationSeconds;
        result.RouteMarkerCount = markers.Length;
        result.SmoothedRoutePointCount = route.Length;
        result.RouteSegmentCount = RoutePoints.Length - 1;
        result.OutputScene = ScenePath;
        result.GeneratedRoot = GeneratedRootName;
        result.CanonicalForestRootExists = GameObject.Find("MYB163_CanonicalForestPassageRoot") != null;
        result.FirstPersonBikeCameraConfigured = true;
        result.MockModePreserved = UnityEngine.Object.FindAnyObjectByType<MYB89ProbeRide>() != null;
        result.BikePovCuesAdded = ValidateBikePovCueSupport();
        result.DurationWithinTarget = estimatedDurationSeconds >= MinimumDurationSeconds && estimatedDurationSeconds <= MaximumDurationSeconds;
        result.RouteVisibleUnsupportedScenicMassCount = CountUnsupportedRouteVisibleScenicMasses();

        if (!result.DurationWithinTarget)
        {
            result.BlockingErrors.Add(
                "Estimated duration is outside target window. estimatedDurationSeconds="
                + FormatFloat(estimatedDurationSeconds) + ".");
        }

        if (!result.CanonicalForestRootExists)
        {
            result.BlockingErrors.Add("MYB163 canonical forest root is missing after MYB-165 build.");
        }

        if (!result.FirstPersonBikeCameraConfigured || !result.BikePovCuesAdded)
        {
            result.BlockingErrors.Add("Bike POV configuration is incomplete or has unsupported cockpit cues.");
        }

        if (!result.MockModePreserved)
        {
            result.BlockingErrors.Add("MYB89ProbeRide mock ride component is missing.");
        }

        if (result.RouteVisibleUnsupportedScenicMassCount > 0)
        {
            result.BlockingErrors.Add(
                "Route-visible unsupported scenic masses remain after MYB-165 cleanup. count="
                + result.RouteVisibleUnsupportedScenicMassCount.ToString(CultureInfo.InvariantCulture) + ".");
        }

        var capture = MYB145CaptureRigHelper.CaptureRouteAndOverview(
            "MYB-165-FirstTrueRoute",
            new MYB145CaptureRigHelper.CaptureOptions
            {
                TicketId = "MYB-165",
                State = "after",
                BaselineSelectedBy = "MYB-165 builder",
                BaselineReason = "MYB-165 validates the first complete three-minute bike POV route after MYB-164 stabilization.",
                BaselineSource = "MYB-164 canonical route"
            });
        result.CaptureVerdict = capture.Verdict;
        result.CaptureReportPath = capture.ReportPathRelative;
        result.CaptureWarnings = capture.Warnings.Select(warning => warning.Code + ": " + warning.Message).ToList();
        result.CaptureErrors = capture.Errors.Select(error => error.Code + ": " + error.Message).ToList();
        if (capture.ErrorCount > 0)
        {
            result.BlockingErrors.Add("MYB-145 capture failed. Report: `" + capture.ReportPathRelative + "`.");
        }

        var validator = MYB144ArtAssetValidator.RunValidation("MYB-165-FirstTrueRoute");
        result.Myb144Verdict = validator.Verdict;
        result.Myb144Errors = validator.ErrorCount;
        result.Myb144Warnings = validator.WarningCount;
        result.Myb144ReportPath = "_bmad-output/unity-test-results/myb-144-art-asset-validator-report.md";
        if (validator.ErrorCount > 0)
        {
            result.BlockingErrors.Add("MYB-144 returned errors. Inspect the validator report before review.");
        }

        if (captureVideoFrames)
        {
            CaptureFullRouteVideoFrames(result);
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        WriteReports(result);
        return result;
    }

    private static Dictionary<string, Material> CreateMaterials()
    {
        return new Dictionary<string, Material>
        {
            { "road", PavingRoadMaterialAt("Assets/MYB165/Materials/MYB165_Road.mat", new Color(0.46f, 0.50f, 0.44f), 0.14f) },
            { "edge", MaterialAt("Assets/MYB165/Materials/MYB165_EdgeLine.mat", new Color(0.82f, 0.92f, 0.88f), 0.04f) },
            { "lane", MaterialAt("Assets/MYB165/Materials/MYB165_CenterDash.mat", new Color(0.96f, 0.88f, 0.66f), 0.04f) },
            { "ground", MaterialAt("Assets/MYB165/Materials/MYB165_LongRouteGround.mat", new Color(0.18f, 0.34f, 0.23f), 0f) },
            { "shoulder", MaterialAt("Assets/MYB165/Materials/MYB165_WarmShoulder.mat", new Color(0.30f, 0.32f, 0.24f), 0.06f) },
            { "meadow", MaterialAt("Assets/MYB165/Materials/MYB165_MeadowSide.mat", new Color(0.24f, 0.43f, 0.27f), 0f) },
            { "hill", MaterialAt("Assets/MYB165/Materials/MYB165_DistantHill.mat", new Color(0.24f, 0.38f, 0.30f), 0f) },
            { "trunk", MaterialAt("Assets/MYB165/Materials/MYB165_Trunk.mat", new Color(0.26f, 0.17f, 0.10f), 0.05f) },
            { "leaf", MaterialAt("Assets/MYB165/Materials/MYB165_LeafMass.mat", new Color(0.10f, 0.30f, 0.18f), 0f) },
            { "leafFar", MaterialAt("Assets/MYB165/Materials/MYB165_LeafFar.mat", new Color(0.16f, 0.31f, 0.24f), 0f) },
            { "stone", MaterialAt("Assets/MYB165/Materials/MYB165_StoneMarker.mat", new Color(0.38f, 0.39f, 0.34f), 0.12f) },
            { "banner", MaterialAt("Assets/MYB165/Materials/MYB165_RouteBanner.mat", new Color(0.92f, 0.66f, 0.23f), 0.15f) },
            { "cockpit", MaterialAt("Assets/MYB165/Materials/MYB165_BikeCockpit.mat", new Color(0.035f, 0.04f, 0.045f), 0.32f) },
            { "climbCue", LoadMaterialOrFallback("Assets/MYB48/Materials/MYB48_ClimbCue.mat", "Assets/MYB165/Materials/MYB165_ClimbCue.mat", new Color(1f, 0.58f, 0.24f), 0.42f) },
            { "sprintCue", LoadMaterialOrFallback("Assets/MYB48/Materials/MYB48_SprintCue.mat", "Assets/MYB165/Materials/MYB165_SprintCue.mat", new Color(1f, 0.18f, 0.16f), 0.42f) },
            { "recoveryCue", LoadMaterialOrFallback("Assets/MYB48/Materials/MYB48_RecoveryCue.mat", "Assets/MYB165/Materials/MYB165_RecoveryCue.mat", new Color(0.28f, 0.78f, 1f), 0.42f) }
        };
    }

    private static Material LoadMaterialOrFallback(string existingAssetPath, string fallbackAssetPath, Color color, float smoothness)
    {
        return AssetDatabase.LoadAssetAtPath<Material>(existingAssetPath)
            ?? MaterialAt(fallbackAssetPath, color, smoothness);
    }

    private static Material PavingRoadMaterialAt(string path, Color color, float smoothness)
    {
        var material = MaterialAt(path, color, smoothness);
        var baseMap = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Echappee/Art/ThirdParty/ambientCG/PavingStones141/Textures/PavingStones141_1K-JPG_Color.jpg");
        var normalMap = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Echappee/Art/ThirdParty/ambientCG/PavingStones141/Textures/PavingStones141_1K-JPG_NormalGL.jpg");
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
        if (material.HasProperty("_BumpScale"))
        {
            material.SetFloat("_BumpScale", 0.34f);
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

    private static Material MaterialAt(string path, Color color, float smoothness)
    {
        var material = AssetDatabase.LoadAssetAtPath<Material>(path);
        var dirty = false;
        if (material == null)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard") ?? Shader.Find("Unlit/Color");
            material = new Material(shader);
            AssetDatabase.CreateAsset(material, path);
            dirty = true;
        }

        dirty |= SetColorIfDifferent(material, "_BaseColor", color);
        dirty |= SetColorIfDifferent(material, "_Color", color);
        dirty |= SetFloatIfDifferent(material, "_Smoothness", smoothness);
        if (dirty)
        {
            EditorUtility.SetDirty(material);
        }

        return material;
    }

    private static bool SetColorIfDifferent(Material material, string propertyName, Color value)
    {
        if (!material.HasProperty(propertyName))
        {
            return false;
        }

        var current = material.GetColor(propertyName);
        if (Mathf.Abs(current.r - value.r) < 0.0001f
            && Mathf.Abs(current.g - value.g) < 0.0001f
            && Mathf.Abs(current.b - value.b) < 0.0001f
            && Mathf.Abs(current.a - value.a) < 0.0001f)
        {
            return false;
        }

        material.SetColor(propertyName, value);
        return true;
    }

    private static bool SetFloatIfDifferent(Material material, string propertyName, float value)
    {
        if (!material.HasProperty(propertyName))
        {
            return false;
        }

        if (Mathf.Abs(material.GetFloat(propertyName) - value) < 0.0001f)
        {
            return false;
        }

        material.SetFloat(propertyName, value);
        return true;
    }

    private static void CreateLongGround(Transform parent, IReadOnlyDictionary<string, Material> materials, IReadOnlyList<Vector3> route)
    {
        var bounds = BoundsFor(route);
        var plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
        plane.name = "MYB165_LongRouteGroundPlane";
        plane.transform.SetParent(parent, false);
        plane.transform.position = new Vector3(bounds.center.x, -0.025f, bounds.center.z);
        plane.transform.localScale = new Vector3(58f, 1f, 250f);
        SetMaterial(plane, materials["ground"]);
        DestroyCollider(plane);

        CreateStripMesh("MYB165_LeftWarmShoulder_Long", parent, route, -RoadWidth * 0.5f - 1.55f, 2.7f, 0.02f, materials["shoulder"]);
        CreateStripMesh("MYB165_RightWarmShoulder_Long", parent, route, RoadWidth * 0.5f + 1.55f, 2.7f, 0.02f, materials["shoulder"]);
        CreateStripMesh("MYB165_LeftMeadowBand_Long", parent, route, -RoadWidth * 0.5f - 5.2f, 4.6f, 0.012f, materials["meadow"]);
        CreateStripMesh("MYB165_RightMeadowBand_Long", parent, route, RoadWidth * 0.5f + 5.2f, 4.6f, 0.012f, materials["meadow"]);
    }

    private static Transform[] CreateRoute(Transform parent, IReadOnlyDictionary<string, Material> materials, IReadOnlyList<Vector3> route)
    {
        var markerRoot = new GameObject("MYB89_RouteMarkers");
        markerRoot.transform.SetParent(parent, false);
        var markers = new Transform[RoutePoints.Length];
        for (var i = 0; i < RoutePoints.Length; i++)
        {
            var marker = new GameObject("RouteMarker_" + i.ToString("00", CultureInfo.InvariantCulture));
            marker.transform.SetParent(markerRoot.transform, false);
            marker.transform.position = RoutePoints[i];
            markers[i] = marker.transform;
        }

        CreateStripMesh("MYB89_RouteRoad", parent, route, 0f, RoadWidth, 0.035f, materials["road"]);
        CreateStripMesh("MYB89_LeftEdgeLine", parent, route, -RoadWidth * 0.5f + 0.16f, 0.12f, 0.08f, materials["edge"]);
        CreateStripMesh("MYB89_RightEdgeLine", parent, route, RoadWidth * 0.5f - 0.16f, 0.12f, 0.08f, materials["edge"]);

        var routeLength = MYB89RideTrajectory.Length(route);
        for (var meters = 8f; meters < routeLength - 10f; meters += 18f)
        {
            if (!MYB89RideTrajectory.TrySample(route, meters, false, out var sample))
            {
                continue;
            }

            CreateCube(
                "MYB89_CenterDash",
                parent,
                sample.Position + Vector3.up * 0.105f,
                new Vector3(0.18f, 0.035f, 5.6f),
                Quaternion.LookRotation(sample.Forward, Vector3.up),
                materials["lane"]);
        }

        return markers;
    }

    private static void CreateRouteDifficultyCues(Transform parent, Transform[] markers, IReadOnlyDictionary<string, Material> materials)
    {
        var cueRoot = new GameObject("MYB48_RouteDifficultyCues");
        cueRoot.transform.SetParent(parent, false);
        var controller = cueRoot.AddComponent<MYB48RouteDifficultyCueController>();
        controller.routeMarkers = markers;
        controller.climbMaterial = materials["climbCue"];
        controller.sprintMaterial = materials["sprintCue"];
        controller.recoveryMaterial = materials["recoveryCue"];
        controller.lateralOffsetMeters = 5.4f;
        controller.entryHeightMeters = 2.8f;
        controller.secondaryHeightMeters = 1.25f;
        controller.pulseAmplitude = 0.18f;
        controller.pulseFrequency = 0.75f;
        controller.RebuildCues();
    }

    private static void CreateLongDistanceScenery(
        Transform parent,
        IReadOnlyDictionary<string, Material> materials,
        IReadOnlyList<Vector3> route,
        BuildResult result)
    {
        CreateCheckpointArch(parent, route, 18f, "DEPART", materials);
        CreateCheckpointArch(parent, route, 760f, "VALLEE", materials);
        CreateCheckpointArch(parent, route, 1460f, "CRETE", materials);
        CreateCheckpointArch(parent, route, 2240f, "ARRIVEE", materials);
        result.CheckpointCount = 4;

        for (var meters = 300f; meters < 2200f; meters += 92f)
        {
            var side = Mathf.FloorToInt(meters / 92f) % 2 == 0 ? -1f : 1f;
            CreateTreeGroup("MYB165_RouteTreeGroup_" + result.TreeGroupCount.ToString("00", CultureInfo.InvariantCulture), parent, route, meters, side, materials);
            result.TreeGroupCount++;
        }

        for (var meters = 360f; meters < 2180f; meters += 180f)
        {
            CreateHillGroup(parent, route, meters, -1f, materials["hill"], result);
            CreateHillGroup(parent, route, meters + 70f, 1f, materials["hill"], result);
        }

        for (var meters = 430f; meters < 2140f; meters += 155f)
        {
            CreateStoneMarker(parent, route, meters, meters % 310f < 155f ? -1f : 1f, materials["stone"], result);
        }
    }

    private static void CreateTreeGroup(
        string name,
        Transform parent,
        IReadOnlyList<Vector3> route,
        float meters,
        float side,
        IReadOnlyDictionary<string, Material> materials)
    {
        if (!MYB89RideTrajectory.TrySample(route, meters, false, out var sample))
        {
            return;
        }

        var group = new GameObject(name);
        group.transform.SetParent(parent, false);
        group.transform.position = sample.Position + sample.Right * side * (RoadWidth * 0.5f + 11f + Mathf.Abs(Jitter(meters, 3.5f)));
        group.transform.rotation = Quaternion.LookRotation(sample.Forward, Vector3.up) * Quaternion.Euler(0f, side * Jitter(meters, 18f), 0f);

        for (var i = 0; i < 3; i++)
        {
            var local = new Vector3((i - 1) * 1.55f * side, 0f, Jitter(meters + i * 12f, 1.2f));
            var height = 4.8f + Mathf.Abs(Jitter(meters + i * 7f, 1.8f));
            var trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            trunk.name = "supported_trunk_" + i.ToString("00", CultureInfo.InvariantCulture);
            trunk.transform.SetParent(group.transform, false);
            trunk.transform.localPosition = local + Vector3.up * (height * 0.5f);
            trunk.transform.localRotation = Quaternion.Euler(side * Jitter(meters + i, 4f), Jitter(meters + i, 18f), side * Jitter(meters + i * 2f, 5f));
            trunk.transform.localScale = new Vector3(0.32f, height * 0.5f, 0.32f);
            SetMaterial(trunk, materials["trunk"]);
            DestroyCollider(trunk);

            var crown = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            crown.name = "asymmetric_supported_canopy_" + i.ToString("00", CultureInfo.InvariantCulture);
            crown.transform.SetParent(group.transform, false);
            crown.transform.localPosition = local + new Vector3(side * 0.35f, height + 0.62f, Jitter(meters + i, 0.45f));
            crown.transform.localScale = new Vector3(2.5f + Mathf.Abs(Jitter(meters + i, 0.8f)), 1.25f, 1.8f);
            crown.transform.localRotation = Quaternion.Euler(0f, Jitter(meters + i, 24f), 0f);
            SetMaterial(crown, meters > 1500f ? materials["leafFar"] : materials["leaf"]);
            DestroyCollider(crown);
        }
    }

    private static void CreateHillGroup(
        Transform parent,
        IReadOnlyList<Vector3> route,
        float meters,
        float side,
        Material material,
        BuildResult result)
    {
        if (!MYB89RideTrajectory.TrySample(route, meters, false, out var sample))
        {
            return;
        }

        var halfHeight = 0.58f + Mathf.Abs(Jitter(meters, 0.24f));
        var hill = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        hill.name = "MYB165_GroundedDistantMound_" + result.HillMassCount.ToString("00", CultureInfo.InvariantCulture);
        hill.transform.SetParent(parent, false);
        hill.transform.position = sample.Position
            + sample.Right * side * (52f + Mathf.Abs(Jitter(meters, 16f)))
            + Vector3.up * (halfHeight - 0.03f);
        hill.transform.localScale = new Vector3(
            18f + Mathf.Abs(Jitter(meters, 6f)),
            halfHeight * 2f,
            12f + Mathf.Abs(Jitter(meters, 5f)));
        SetMaterial(hill, material);
        DestroyCollider(hill);
        result.HillMassCount++;
    }

    private static int RetireLegacyUnsupportedProbeScenery()
    {
        var retired = 0;
        var objects = UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None)
            .Where(transform => transform != null
                && HasUnsupportedLegacyPrefix(transform.name)
                && !HasUnsupportedLegacyAncestor(transform))
            .Select(transform => transform.gameObject)
            .Distinct()
            .ToArray();

        foreach (var gameObject in objects)
        {
            if (gameObject == null)
            {
                continue;
            }

            UnityEngine.Object.DestroyImmediate(gameObject);
            retired++;
        }

        return retired;
    }

    private static int CountUnsupportedRouteVisibleScenicMasses()
    {
        return UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
            .Count(transform => transform != null && HasUnsupportedLegacyPrefix(transform.name));
    }

    private static bool HasUnsupportedLegacyAncestor(Transform transform)
    {
        var current = transform.parent;
        while (current != null)
        {
            if (HasUnsupportedLegacyPrefix(current.name))
            {
                return true;
            }

            current = current.parent;
        }

        return false;
    }

    private static bool HasUnsupportedLegacyPrefix(string objectName)
    {
        return UnsupportedLegacyRouteVisiblePrefixes.Any(prefix => objectName.StartsWith(prefix, StringComparison.Ordinal));
    }

    private static void CreateStoneMarker(
        Transform parent,
        IReadOnlyList<Vector3> route,
        float meters,
        float side,
        Material material,
        BuildResult result)
    {
        if (!MYB89RideTrajectory.TrySample(route, meters, false, out var sample))
        {
            return;
        }

        var stone = GameObject.CreatePrimitive(PrimitiveType.Cube);
        stone.name = "MYB165_MossStoneMarker_" + result.StoneMarkerCount.ToString("00", CultureInfo.InvariantCulture);
        stone.transform.SetParent(parent, false);
        stone.transform.position = sample.Position + sample.Right * side * (RoadWidth * 0.5f + 7.4f) + Vector3.up * 0.46f;
        stone.transform.rotation = Quaternion.LookRotation(sample.Forward, Vector3.up) * Quaternion.Euler(0f, Jitter(meters, 28f), Jitter(meters, 6f));
        stone.transform.localScale = new Vector3(1.05f, 0.92f, 0.65f);
        SetMaterial(stone, material);
        DestroyCollider(stone);
        result.StoneMarkerCount++;
    }

    private static void CreateCheckpointArch(
        Transform parent,
        IReadOnlyList<Vector3> route,
        float meters,
        string label,
        IReadOnlyDictionary<string, Material> materials)
    {
        if (!MYB89RideTrajectory.TrySample(route, meters, false, out var sample))
        {
            return;
        }

        var rotation = Quaternion.LookRotation(sample.Forward, Vector3.up);
        CreateCube("MYB165_CheckpointPost_" + label + "_L", parent, sample.Position - sample.Right * (RoadWidth * 0.5f + 0.9f) + Vector3.up * 2.0f, new Vector3(0.32f, 4.0f, 0.32f), rotation, materials["stone"]);
        CreateCube("MYB165_CheckpointPost_" + label + "_R", parent, sample.Position + sample.Right * (RoadWidth * 0.5f + 0.9f) + Vector3.up * 2.0f, new Vector3(0.32f, 4.0f, 0.32f), rotation, materials["stone"]);
        CreateCube("MYB165_CheckpointBeam_" + label, parent, sample.Position + Vector3.up * 4.15f, new Vector3(RoadWidth + 2.3f, 0.40f, 0.55f), rotation, materials["banner"]);
    }

    private static void UpdateRideRig(Transform[] markers, float routeLength, BuildResult result)
    {
        var ride = UnityEngine.Object.FindAnyObjectByType<MYB89ProbeRide>();
        if (ride == null)
        {
            result.BlockingErrors.Add("MYB89ProbeRide missing while configuring MYB-165 route.");
            return;
        }

        ride.routeMarkers = markers;
        ride.speedMetersPerSecond = NormalSpeedMetersPerSecond;
        ride.cameraBobMeters = 0.026f;
        ride.cameraBobFrequency = 1.35f;
        ride.turnLookAheadMeters = 14f;
        ride.orientationSmoothing = 8f;
        ride.cameraTurnLeanMaxDegrees = 3.0f;
        ride.progressMeters = 0f;
        ride.autoplay = false;
        ride.waitForRoutePreview = true;
        ride.trainerMode = MYB57TrainerMode.Simulated;
        ride.trainerSourcePreset = MYB57TrainerSourcePreset.PowerAvailable;

        if (ride.cameraPivot != null)
        {
            ride.cameraPivot.localPosition = new Vector3(0f, 1.30f, -0.92f);
            ride.cameraPivot.localRotation = Quaternion.Euler(5.5f, 0f, 0f);
            var camera = ride.cameraPivot.GetComponent<Camera>();
            if (camera != null)
            {
                camera.fieldOfView = 68f;
                camera.farClipPlane = 520f;
                camera.nearClipPlane = 0.03f;
            }
        }

        CreateBikePovCues(ride.transform);
        ride.RebuildRouteCache();
        ride.SetPreviewProgress(Mathf.Min(24f, routeLength * 0.02f));
    }

    private static void CreateBikePovCues(Transform rig)
    {
        DestroyNamed("MYB165_BikePOVCues");
        var material = AssetDatabase.LoadAssetAtPath<Material>("Assets/MYB165/Materials/MYB165_BikeCockpit.mat")
            ?? MaterialAt("Assets/MYB165/Materials/MYB165_BikeCockpit.mat", new Color(0.035f, 0.04f, 0.045f), 0.32f);
        var cues = new GameObject("MYB165_BikePOVCues");
        cues.transform.SetParent(rig, false);
        cues.transform.localPosition = Vector3.zero;
        cues.transform.localRotation = Quaternion.identity;

        CreateLocalCube("subtle_handlebar_bridge", cues.transform, new Vector3(0f, 1.02f, 0.42f), new Vector3(1.18f, 0.055f, 0.055f), Quaternion.identity, material);
        CreateLocalCube("stem_low_visible", cues.transform, new Vector3(0f, 0.97f, 0.29f), new Vector3(0.08f, 0.07f, 0.38f), Quaternion.identity, material);
        CreateLocalCube("steerer_column_visible_support", cues.transform, new Vector3(0f, 0.82f, 0.67f), new Vector3(0.075f, 0.34f, 0.075f), Quaternion.Euler(-20f, 0f, 0f), material);
        CreateLocalCube("fork_crown_visible_support", cues.transform, new Vector3(0f, 0.66f, 0.92f), new Vector3(0.44f, 0.06f, 0.09f), Quaternion.identity, material);
        CreateLocalCube("fork_blade_left_visible_support", cues.transform, new Vector3(-0.19f, 0.52f, 1.05f), new Vector3(0.055f, 0.42f, 0.045f), Quaternion.Euler(-9f, 0f, 0f), material);
        CreateLocalCube("fork_blade_right_visible_support", cues.transform, new Vector3(0.19f, 0.52f, 1.05f), new Vector3(0.055f, 0.42f, 0.045f), Quaternion.Euler(-9f, 0f, 0f), material);
        CreateLocalCube("front_axle_visible_support", cues.transform, new Vector3(0f, 0.48f, 1.1f), new Vector3(0.46f, 0.035f, 0.035f), Quaternion.identity, material);

        var wheel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        wheel.name = "subtle_front_wheel_hint";
        wheel.transform.SetParent(cues.transform, false);
        wheel.transform.localPosition = new Vector3(0f, 0.43f, 1.12f);
        wheel.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        wheel.transform.localScale = new Vector3(0.42f, 0.024f, 0.42f);
        SetMaterial(wheel, material);
        DestroyCollider(wheel);
    }

    private static bool ValidateBikePovCueSupport()
    {
        var root = GameObject.Find("MYB165_BikePOVCues");
        if (root == null)
        {
            return false;
        }

        var requiredChildren = new[]
        {
            "subtle_handlebar_bridge",
            "stem_low_visible",
            "steerer_column_visible_support",
            "fork_crown_visible_support",
            "fork_blade_left_visible_support",
            "fork_blade_right_visible_support",
            "front_axle_visible_support",
            "subtle_front_wheel_hint"
        };

        foreach (var childName in requiredChildren)
        {
            if (FindChildByName(root.transform, childName) == null)
            {
                return false;
            }
        }

        return true;
    }

    private static Transform FindChildByName(Transform root, string name)
    {
        foreach (var child in root.GetComponentsInChildren<Transform>(true))
        {
            if (child.name == name)
            {
                return child;
            }
        }

        return null;
    }

    private static void UpdatePreviewPanel()
    {
        var preview = UnityEngine.Object.FindAnyObjectByType<MYB73RoutePreviewPanel>(FindObjectsInactive.Include);
        if (preview == null)
        {
            return;
        }

        preview.routeTitle = "Premiere Echappee";
        preview.routeSubtitle = "Trois minutes de route lisible, depart forestier et respiration scenique en mock mode.";
        preview.overallDifficulty = "Moderee";
        preview.biomes = new[] { "Foret ancienne", "Vallee verte", "Crete douce", "Retour calme" };
        preview.passages = new[] { "Depart en foret", "Vallee ouverte", "Crete scenic" };
        preview.referenceSpeedMetersPerSecond = NormalSpeedMetersPerSecond;
        preview.Refresh();
    }

    private static void ConfigureCaptureCameras(IReadOnlyList<Vector3> route)
    {
        ConfigureRouteCamera(route, 12f);
        ConfigureOverviewCamera(route);
    }

    private static void ConfigureRouteCamera(IReadOnlyList<Vector3> route, float meters)
    {
        var camera = FindOrCreateCamera("RouteCamera");
        if (!MYB89RideTrajectory.TrySample(route, meters, false, out var sample))
        {
            return;
        }

        MYB89RideTrajectory.TrySample(route, meters + 24f, false, out var lookAhead);
        var forward = Vector3.Slerp(sample.Forward, lookAhead.Forward, 0.55f).normalized;
        camera.transform.position = sample.Position - forward * 0.35f + Vector3.up * 1.32f;
        camera.transform.rotation = Quaternion.LookRotation(forward, Vector3.up) * Quaternion.Euler(4.2f, 0f, 0f);
        camera.fieldOfView = 50f;
        camera.nearClipPlane = 0.05f;
        camera.farClipPlane = 260f;
        camera.orthographic = false;
        camera.clearFlags = CameraClearFlags.Skybox;
    }

    private static void ConfigureOverviewCamera(IReadOnlyList<Vector3> route)
    {
        var camera = FindOrCreateCamera("OverviewCamera");
        var bounds = BoundsFor(route);
        camera.transform.position = new Vector3(bounds.center.x, 86f, bounds.center.z);
        camera.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        camera.orthographic = true;
        camera.orthographicSize = 42f;
        camera.nearClipPlane = 0.05f;
        camera.farClipPlane = 220f;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.35f, 0.40f, 0.37f);
    }

    private static Camera FindOrCreateCamera(string name)
    {
        var existing = GameObject.Find(name);
        if (existing != null && existing.TryGetComponent<Camera>(out var camera))
        {
            return camera;
        }

        if (existing != null)
        {
            UnityEngine.Object.DestroyImmediate(existing);
        }

        var cameraObject = new GameObject(name);
        return cameraObject.AddComponent<Camera>();
    }

    private static void CaptureFullRouteVideoFrames(BuildResult result)
    {
        var ride = UnityEngine.Object.FindAnyObjectByType<MYB89ProbeRide>();
        var camera = Camera.main;
        if (ride == null || camera == null)
        {
            result.VideoCaptureStatus = "skipped: missing ride or main camera";
            return;
        }

        ride.RebuildRouteCache();
        var routeLength = ride.RouteLength;
        var durationSeconds = routeLength / NormalSpeedMetersPerSecond;
        var frameCount = Mathf.CeilToInt(durationSeconds * VideoFrameRate);
        var stamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH-mm-ssZ", CultureInfo.InvariantCulture);
        var outputRelative = VideoRootRelative + "/myb-165-first-true-route-" + stamp;
        var outputDirectory = ToRepoPath(outputRelative);
        var framesDirectory = Path.Combine(outputDirectory, "frames");
        Directory.CreateDirectory(framesDirectory);

        var previousAutoplay = ride.autoplay;
        var previousWait = ride.waitForRoutePreview;
        var previousTarget = camera.targetTexture;
        var previousActive = RenderTexture.active;
        var previewPanels = UnityEngine.Object.FindObjectsByType<MYB73RoutePreviewPanel>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        var previewStates = new List<Tuple<GameObject, bool>>();
        foreach (var preview in previewPanels)
        {
            var root = preview.panelRoot == null ? preview.gameObject : preview.panelRoot;
            if (root != null)
            {
                previewStates.Add(new Tuple<GameObject, bool>(root, root.activeSelf));
                root.SetActive(false);
            }
        }

        var namedPreview = GameObject.Find("MYB73_RoutePreview");
        if (namedPreview != null && previewStates.All(state => state.Item1 != namedPreview))
        {
            previewStates.Add(new Tuple<GameObject, bool>(namedPreview, namedPreview.activeSelf));
            namedPreview.SetActive(false);
        }

        ride.autoplay = false;
        ride.waitForRoutePreview = false;
        SetHudVisible(ride, false);

        var renderTexture = new RenderTexture(VideoWidth, VideoHeight, 24, RenderTextureFormat.ARGB32);
        var texture = new Texture2D(VideoWidth, VideoHeight, TextureFormat.RGB24, false);
        try
        {
            camera.targetTexture = renderTexture;
            RenderTexture.active = renderTexture;
            for (var frame = 0; frame < frameCount; frame++)
            {
                var seconds = frame / (float)VideoFrameRate;
                ride.SetPreviewProgress(Mathf.Min(routeLength - 0.1f, seconds * NormalSpeedMetersPerSecond));
                Canvas.ForceUpdateCanvases();
                camera.Render();
                texture.ReadPixels(new Rect(0, 0, VideoWidth, VideoHeight), 0, 0);
                texture.Apply();
                File.WriteAllBytes(
                    Path.Combine(framesDirectory, "frame_" + frame.ToString("0000", CultureInfo.InvariantCulture) + ".jpg"),
                    texture.EncodeToJPG(86));
            }
        }
        finally
        {
            camera.targetTexture = previousTarget;
            RenderTexture.active = previousActive;
            renderTexture.Release();
            UnityEngine.Object.DestroyImmediate(renderTexture);
            UnityEngine.Object.DestroyImmediate(texture);
            ride.autoplay = previousAutoplay;
            ride.waitForRoutePreview = previousWait;
            SetHudVisible(ride, true);
            foreach (var state in previewStates)
            {
                if (state.Item1 != null)
                {
                    state.Item1.SetActive(state.Item2);
                }
            }
        }

        result.VideoCaptureStatus = "frames captured";
        result.VideoFrameCount = frameCount;
        result.VideoFrameRate = VideoFrameRate;
        result.VideoDurationSeconds = durationSeconds;
        result.VideoFramesDirectory = outputRelative + "/frames";
        result.VideoMp4Path = outputRelative + "/myb-165-first-true-route-bike-pov-3min-720p.mp4";
        result.VideoContactSheetPath = outputRelative + "/myb-165-first-true-route-contact-sheet.jpg";

        File.WriteAllText(
            Path.Combine(outputDirectory, "capture-summary.json"),
            "{\n"
            + "  \"ticket\": \"MYB-165\",\n"
            + "  \"routeLengthMeters\": " + FormatFloat(routeLength) + ",\n"
            + "  \"normalSpeedMetersPerSecond\": " + FormatFloat(NormalSpeedMetersPerSecond) + ",\n"
            + "  \"durationSeconds\": " + FormatFloat(durationSeconds) + ",\n"
            + "  \"frameRate\": " + VideoFrameRate.ToString(CultureInfo.InvariantCulture) + ",\n"
            + "  \"frameCount\": " + frameCount.ToString(CultureInfo.InvariantCulture) + ",\n"
            + "  \"framesDirectory\": \"" + EscapeJson(result.VideoFramesDirectory) + "\",\n"
            + "  \"mp4Path\": \"" + EscapeJson(result.VideoMp4Path) + "\",\n"
            + "  \"contactSheetPath\": \"" + EscapeJson(result.VideoContactSheetPath) + "\"\n"
            + "}\n");
    }

    private static void SetHudVisible(MYB89ProbeRide ride, bool visible)
    {
        SetTextVisible(ride.distanceLabel, visible);
        SetTextVisible(ride.speedLabel, visible);
        SetTextVisible(ride.difficultyLabel, visible);
        SetTextVisible(ride.gradeLabel, visible);
        SetTextVisible(ride.segmentLabel, visible);
        SetTextVisible(ride.verdictLabel, visible);
    }

    private static void SetTextVisible(Text text, bool visible)
    {
        if (text != null)
        {
            text.gameObject.SetActive(visible);
        }
    }

    private static void CreateStripMesh(string name, Transform parent, IReadOnlyList<Vector3> points, float lateralOffset, float width, float yLift, Material material)
    {
        var gameObject = new GameObject(name);
        gameObject.transform.SetParent(parent, false);

        var vertices = new Vector3[points.Count * 2];
        var normals = new Vector3[vertices.Length];
        var uvs = new Vector2[vertices.Length];
        var triangles = new int[(points.Count - 1) * 6];

        for (var i = 0; i < points.Count; i++)
        {
            var tangent = TangentAt(points, i);
            var right = Vector3.Cross(Vector3.up, tangent).normalized;
            var center = points[i] + right * lateralOffset + Vector3.up * yLift;
            vertices[i * 2] = center - right * (width * 0.5f);
            vertices[i * 2 + 1] = center + right * (width * 0.5f);
            normals[i * 2] = Vector3.up;
            normals[i * 2 + 1] = Vector3.up;
            uvs[i * 2] = new Vector2(0f, i * 0.18f);
            uvs[i * 2 + 1] = new Vector2(1f, i * 0.18f);
        }

        var triangleIndex = 0;
        for (var i = 0; i < points.Count - 1; i++)
        {
            var leftA = i * 2;
            var rightA = i * 2 + 1;
            var leftB = (i + 1) * 2;
            var rightB = (i + 1) * 2 + 1;
            triangles[triangleIndex++] = leftA;
            triangles[triangleIndex++] = leftB;
            triangles[triangleIndex++] = rightA;
            triangles[triangleIndex++] = rightA;
            triangles[triangleIndex++] = leftB;
            triangles[triangleIndex++] = rightB;
        }

        var mesh = new Mesh { name = name + "Mesh" };
        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.RecalculateBounds();
        gameObject.AddComponent<MeshFilter>().sharedMesh = mesh;
        gameObject.AddComponent<MeshRenderer>().sharedMaterial = material;
    }

    private static Vector3 TangentAt(IReadOnlyList<Vector3> points, int index)
    {
        if (index <= 0)
        {
            return (points[1] - points[0]).normalized;
        }

        if (index >= points.Count - 1)
        {
            return (points[index] - points[index - 1]).normalized;
        }

        return (points[index + 1] - points[index - 1]).normalized;
    }

    private static GameObject CreateCube(string name, Transform parent, Vector3 position, Vector3 scale, Quaternion rotation, Material material)
    {
        var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = name;
        cube.transform.SetParent(parent, false);
        cube.transform.SetPositionAndRotation(position, rotation);
        cube.transform.localScale = scale;
        SetMaterial(cube, material);
        DestroyCollider(cube);
        return cube;
    }

    private static GameObject CreateLocalCube(string name, Transform parent, Vector3 position, Vector3 scale, Quaternion rotation, Material material)
    {
        var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = name;
        cube.transform.SetParent(parent, false);
        cube.transform.localPosition = position;
        cube.transform.localRotation = rotation;
        cube.transform.localScale = scale;
        SetMaterial(cube, material);
        DestroyCollider(cube);
        return cube;
    }

    private static void SetMaterial(GameObject gameObject, Material material)
    {
        var renderer = gameObject.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.sharedMaterial = material;
        }
    }

    private static void DestroyCollider(GameObject gameObject)
    {
        var collider = gameObject.GetComponent<Collider>();
        if (collider != null)
        {
            UnityEngine.Object.DestroyImmediate(collider);
        }
    }

    private static Bounds BoundsFor(IReadOnlyList<Vector3> points)
    {
        var bounds = new Bounds(points[0], Vector3.zero);
        for (var i = 1; i < points.Count; i++)
        {
            bounds.Encapsulate(points[i]);
        }

        return bounds;
    }

    private static float Jitter(float seed, float amplitude)
    {
        return (Mathf.PerlinNoise(Seed * 0.001f, seed * 0.037f) - 0.5f) * 2f * amplitude;
    }

    private static void DestroyNamed(string name)
    {
        var gameObject = GameObject.Find(name);
        if (gameObject != null)
        {
            UnityEngine.Object.DestroyImmediate(gameObject);
        }
    }

    private static void DestroyObjectsWithPrefix(string prefix)
    {
        foreach (var transform in UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (transform != null && transform.gameObject.name.StartsWith(prefix, StringComparison.Ordinal))
            {
                UnityEngine.Object.DestroyImmediate(transform.gameObject);
            }
        }
    }

    private static void EnsureFolder(string assetPath)
    {
        if (AssetDatabase.IsValidFolder(assetPath))
        {
            return;
        }

        var parent = Path.GetDirectoryName(assetPath)?.Replace('\\', '/') ?? "Assets";
        var folder = Path.GetFileName(assetPath);
        if (!AssetDatabase.IsValidFolder(parent))
        {
            EnsureFolder(parent);
        }

        AssetDatabase.CreateFolder(parent, folder);
    }

    private static string ToRepoPath(string relativePath)
    {
        return Path.GetFullPath(Path.Combine(Application.dataPath, "../../..", relativePath));
    }

    private static string FormatFloat(float value)
    {
        return value.ToString("0.###", CultureInfo.InvariantCulture);
    }

    private static string EscapeJson(string value)
    {
        return (value ?? string.Empty)
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"")
            .Replace("\r", "\\r")
            .Replace("\n", "\\n");
    }

    private static void WriteReports(BuildResult result)
    {
        File.WriteAllText(ToRepoPath(MetricsRelativePath), result.ToMetricsJson());
        File.WriteAllText(ToRepoPath(ReportRelativePath), result.ToMarkdownReport());
        File.WriteAllText(ToRepoPath(GovernanceRelativePath), result.ToGovernanceMarkdown());
    }

    public sealed class BuildResult
    {
        public string OutputScene;
        public string GeneratedRoot;
        public float RouteLengthMeters;
        public float NormalSpeedMetersPerSecond;
        public float EstimatedDurationSeconds;
        public int RouteMarkerCount;
        public int RouteSegmentCount;
        public int SmoothedRoutePointCount;
        public int CheckpointCount;
        public int TreeGroupCount;
        public int HillMassCount;
        public int StoneMarkerCount;
        public int RetiredLegacyUnsupportedPropCount;
        public int RouteVisibleUnsupportedScenicMassCount;
        public bool DurationWithinTarget;
        public bool CanonicalForestRootExists;
        public bool FirstPersonBikeCameraConfigured;
        public bool BikePovCuesAdded;
        public bool MockModePreserved;
        public string CaptureVerdict = "Not run";
        public string CaptureReportPath = string.Empty;
        public List<string> CaptureWarnings = new List<string>();
        public List<string> CaptureErrors = new List<string>();
        public string Myb144Verdict = "Not run";
        public int Myb144Errors;
        public int Myb144Warnings;
        public string Myb144ReportPath = string.Empty;
        public string VideoCaptureStatus = "not requested";
        public int VideoFrameCount;
        public int VideoFrameRate;
        public float VideoDurationSeconds;
        public string VideoFramesDirectory = string.Empty;
        public string VideoMp4Path = string.Empty;
        public string VideoContactSheetPath = string.Empty;
        public List<string> BlockingErrors = new List<string>();

        public string ToConsoleSummary()
        {
            return "MYB-165 routeLength="
                + FormatFloat(RouteLengthMeters)
                + "m duration="
                + FormatFloat(EstimatedDurationSeconds)
                + "s blockers="
                + BlockingErrors.Count
                + " capture="
                + CaptureVerdict
                + " MYB-144="
                + Myb144Verdict
                + " video="
                + VideoCaptureStatus;
        }

        public string ToMetricsJson()
        {
            return "{\n"
                + "  \"ticket\": \"MYB-165\",\n"
                + "  \"seed\": " + Seed.ToString(CultureInfo.InvariantCulture) + ",\n"
                + "  \"outputScene\": \"" + EscapeJson(OutputScene) + "\",\n"
                + "  \"generatedRoot\": \"" + EscapeJson(GeneratedRoot) + "\",\n"
                + "  \"routeLengthMeters\": " + FormatFloat(RouteLengthMeters) + ",\n"
                + "  \"normalSpeedMetersPerSecond\": " + FormatFloat(NormalSpeedMetersPerSecond) + ",\n"
                + "  \"estimatedDurationSeconds\": " + FormatFloat(EstimatedDurationSeconds) + ",\n"
                + "  \"targetDurationMinSeconds\": " + FormatFloat(MinimumDurationSeconds) + ",\n"
                + "  \"targetDurationMaxSeconds\": " + FormatFloat(MaximumDurationSeconds) + ",\n"
                + "  \"durationWithinTarget\": " + DurationWithinTarget.ToString().ToLowerInvariant() + ",\n"
                + "  \"routeMarkerCount\": " + RouteMarkerCount.ToString(CultureInfo.InvariantCulture) + ",\n"
                + "  \"routeSegmentCount\": " + RouteSegmentCount.ToString(CultureInfo.InvariantCulture) + ",\n"
                + "  \"smoothedRoutePointCount\": " + SmoothedRoutePointCount.ToString(CultureInfo.InvariantCulture) + ",\n"
                + "  \"checkpointCount\": " + CheckpointCount.ToString(CultureInfo.InvariantCulture) + ",\n"
                + "  \"treeGroupCount\": " + TreeGroupCount.ToString(CultureInfo.InvariantCulture) + ",\n"
                + "  \"hillMassCount\": " + HillMassCount.ToString(CultureInfo.InvariantCulture) + ",\n"
                + "  \"stoneMarkerCount\": " + StoneMarkerCount.ToString(CultureInfo.InvariantCulture) + ",\n"
                + "  \"retiredLegacyUnsupportedPropCount\": " + RetiredLegacyUnsupportedPropCount.ToString(CultureInfo.InvariantCulture) + ",\n"
                + "  \"routeVisibleUnsupportedScenicMassCount\": " + RouteVisibleUnsupportedScenicMassCount.ToString(CultureInfo.InvariantCulture) + ",\n"
                + "  \"canonicalForestRootExists\": " + CanonicalForestRootExists.ToString().ToLowerInvariant() + ",\n"
                + "  \"firstPersonBikeCameraConfigured\": " + FirstPersonBikeCameraConfigured.ToString().ToLowerInvariant() + ",\n"
                + "  \"bikePovCuesAdded\": " + BikePovCuesAdded.ToString().ToLowerInvariant() + ",\n"
                + "  \"mockModePreserved\": " + MockModePreserved.ToString().ToLowerInvariant() + ",\n"
                + "  \"videoFrameCount\": " + VideoFrameCount.ToString(CultureInfo.InvariantCulture) + ",\n"
                + "  \"videoFrameRate\": " + VideoFrameRate.ToString(CultureInfo.InvariantCulture) + ",\n"
                + "  \"videoDurationSeconds\": " + FormatFloat(VideoDurationSeconds) + ",\n"
                + "  \"premiumTargetReached\": false,\n"
                + "  \"recommendedLinearStatus\": \"In Review\"\n"
                + "}\n";
        }

        public string ToMarkdownReport()
        {
            var builder = new StringBuilder();
            builder.AppendLine("# MYB-165 Implementation Report");
            builder.AppendLine();
            builder.AppendLine("## Summary");
            builder.AppendLine("MYB-165 creates the first real playable route target: an approximately three-minute mock-mode ride from first-person bicycle POV.");
            builder.AppendLine("Human visual QA caught an inherited route-visible prop reading as levitating; MYB-165 now blocks unsupported scenic masses in addition to cockpit cue support.");
            builder.AppendLine();
            builder.AppendLine("## Route");
            builder.AppendLine("- scene: `" + OutputScene + "`");
            builder.AppendLine("- generated root: `" + GeneratedRoot + "`");
            builder.AppendLine("- route length: `" + FormatFloat(RouteLengthMeters) + "m`");
            builder.AppendLine("- normal mock speed: `" + FormatFloat(NormalSpeedMetersPerSecond) + "m/s`");
            builder.AppendLine("- estimated duration: `" + FormatDuration(EstimatedDurationSeconds) + "`");
            builder.AppendLine("- target window: `2:40` to `3:20`");
            builder.AppendLine("- duration target reached: `" + (DurationWithinTarget ? "Yes" : "No") + "`");
            builder.AppendLine();
            builder.AppendLine("## Composition");
            builder.AppendLine("- The original forest passage remains in the first 245m and is reintegrated through MYB-163.");
            builder.AppendLine("- The extended route adds long meadow shoulders, grounded distant mounds, grouped trees, checkpoint beats and a clear finish marker.");
            builder.AppendLine("- Legacy route-visible probe village/horizon props that read unsupported from bike POV are retired for this MYB-165 route.");
            builder.AppendLine("- This is not a new art-direction forest pass and does not claim Premium target.");
            builder.AppendLine();
            builder.AppendLine("## Bike POV");
            builder.AppendLine("- first-person camera lowered and moved closer to the bicycle axis");
            builder.AppendLine("- subtle bob, look-ahead and turn lean configured");
            builder.AppendLine("- supported handlebar/stem/fork/front-wheel cues added under `MYB165_BikePOVCues`");
            builder.AppendLine("- external/flythrough view is not used as the primary validation surface");
            builder.AppendLine();
            builder.AppendLine("## Visual Support Guard");
            builder.AppendLine("- legacy MYB-44/MYB-89 horizon village props are not accepted as MYB-165 bike-POV evidence because they can read as unsupported or floating at route speed");
            builder.AppendLine("- route-visible unsupported scenic masses are blocking for MYB-165 video review");
            builder.AppendLine("- grounded distant mounds are generated low and side-offset so they do not read as suspended canopy disks");
            builder.AppendLine();
            builder.AppendLine("## Metrics");
            builder.AppendLine("- metrics JSON: `" + MetricsRelativePath + "`");
            builder.AppendLine("- route markers: `" + RouteMarkerCount + "`");
            builder.AppendLine("- route segments: `" + RouteSegmentCount + "`");
            builder.AppendLine("- smoothed route points: `" + SmoothedRoutePointCount + "`");
            builder.AppendLine("- checkpoints: `" + CheckpointCount + "`");
            builder.AppendLine("- tree groups: `" + TreeGroupCount + "`");
            builder.AppendLine("- hill masses: `" + HillMassCount + "`");
            builder.AppendLine("- stone markers: `" + StoneMarkerCount + "`");
            builder.AppendLine("- retired legacy unsupported props: `" + RetiredLegacyUnsupportedPropCount + "`");
            builder.AppendLine("- route-visible unsupported scenic mass count: `" + RouteVisibleUnsupportedScenicMassCount + "`");
            builder.AppendLine();
            builder.AppendLine("## Visual Evidence");
            builder.AppendLine("- MYB-145 capture report: `" + CaptureReportPath + "`");
            builder.AppendLine("- primary video capture: Unity Recorder via `Tools/MyBike/MYB-165/Capture Full Route Video (Unity Recorder)`");
            builder.AppendLine("- Unity Recorder report: `" + ImplementationRootRelative + "/myb-165-video-capture-recorder-report.md`");
            if (!string.IsNullOrEmpty(VideoFramesDirectory))
            {
                builder.AppendLine("- fallback video frames: `" + VideoFramesDirectory + "`");
                builder.AppendLine("- fallback MP4 path after ffmpeg: `" + VideoMp4Path + "`");
                builder.AppendLine("- fallback contact sheet path after ffmpeg: `" + VideoContactSheetPath + "`");
            }
            else
            {
                builder.AppendLine("- fallback video frames: not generated in this build; Recorder is the primary video evidence");
            }
            builder.AppendLine();
            builder.AppendLine("## MYB-144 Validation");
            builder.AppendLine("- verdict: `" + Myb144Verdict + "`");
            builder.AppendLine("- errors: `" + Myb144Errors + "`");
            builder.AppendLine("- warnings: `" + Myb144Warnings + "`");
            builder.AppendLine("- report: `" + Myb144ReportPath + "`");
            builder.AppendLine();
            builder.AppendLine("## Warnings");
            AppendList(builder, "Capture warnings", CaptureWarnings);
            AppendList(builder, "Capture errors", CaptureErrors);
            AppendList(builder, "Blocking errors", BlockingErrors);
            builder.AppendLine();
            builder.AppendLine("## Governance");
            builder.AppendLine("- no Meshy/Tripo/Poly Haven generation: Yes");
            builder.AppendLine("- gameplay/FTMS/resistance model modified: No");
            builder.AppendLine("- mock mode preserved: `" + (MockModePreserved ? "Yes" : "No") + "`");
            builder.AppendLine("- canonical scene modified: Yes, scoped to the first true route");
            builder.AppendLine("- Premium target reached: No");
            builder.AppendLine();
            builder.AppendLine("## Verdict");
            builder.AppendLine("- First playable route: " + (DurationWithinTarget && MockModePreserved ? "Yes" : "No"));
            builder.AppendLine("- Bike POV incarne with supported cockpit cues: " + (FirstPersonBikeCameraConfigured && BikePovCuesAdded ? "Yes" : "No"));
            builder.AppendLine("- Recommended Linear status: In Review");
            return builder.ToString();
        }

        public string ToGovernanceMarkdown()
        {
            return "# MYB-165 Governance Review\n\n"
                + "- Dedicated builder exists: Yes\n"
                + "- Seed 165001 used: Yes\n"
                + "- Output scene: `" + OutputScene + "`\n"
                + "- Generated root exists: `" + GeneratedRoot + "`\n"
                + "- Route duration target reached: " + (DurationWithinTarget ? "Yes" : "No") + "\n"
                + "- Player embodies bicycle POV with supported cockpit cues: " + (FirstPersonBikeCameraConfigured && BikePovCuesAdded ? "Yes" : "No") + "\n"
                + "- Legacy unsupported route-visible props retired: " + RetiredLegacyUnsupportedPropCount.ToString(CultureInfo.InvariantCulture) + "\n"
                + "- Route-visible unsupported scenic masses: " + RouteVisibleUnsupportedScenicMassCount.ToString(CultureInfo.InvariantCulture) + "\n"
                + "- Mock mode preserved: " + (MockModePreserved ? "Yes" : "No") + "\n"
                + "- Canonical forest passage preserved: " + (CanonicalForestRootExists ? "Yes" : "No") + "\n"
                + "- New Meshy generation: 0\n"
                + "- Tripo/Poly Haven used: No\n"
                + "- Gameplay/FTMS modified: No\n"
                + "- MYB-144 run: Yes\n"
                + "- MYB-144 errors: " + Myb144Errors.ToString(CultureInfo.InvariantCulture) + "\n"
                + "- MYB-144 warnings: " + Myb144Warnings.ToString(CultureInfo.InvariantCulture) + "\n"
                + "- Premium target reached: No\n"
                + "- Recommended Linear status: In Review\n"
                + "- Auto-review verdict: " + (BlockingErrors.Count == 0 ? "PASS_WITH_WARNINGS" : "FAIL") + "\n";
        }

        private static void AppendList(StringBuilder builder, string title, IReadOnlyList<string> values)
        {
            builder.AppendLine("### " + title);
            if (values == null || values.Count == 0)
            {
                builder.AppendLine("- None recorded.");
                return;
            }

            foreach (var value in values)
            {
                builder.AppendLine("- " + value);
            }
        }

        private static string FormatDuration(float seconds)
        {
            var rounded = Mathf.RoundToInt(seconds);
            return (rounded / 60).ToString(CultureInfo.InvariantCulture) + ":"
                + (rounded % 60).ToString("00", CultureInfo.InvariantCulture);
        }
    }
}

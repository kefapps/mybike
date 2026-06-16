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

public static class MYB148RouteFirstScatterBuilder
{
    private const string ScenePath = "Assets/Scenes/MYB148RouteFirstScatterPreview.unity";
    private const string KitRoot = "Assets/Echappee/Art/Candidates/MYB_ForestKit_V0";
    private const string ImplementationRootRelative = "_bmad-output/implementation-artifacts/MYB-148";
    private const string VisualRootRelative = "_bmad-output/visual-checkpoints/MYB-148";
    private const string ReportRelativePath = ImplementationRootRelative + "/myb-148-implementation-report.md";
    private const string MetricsRelativePath = ImplementationRootRelative + "/myb-148-scatter-metrics.json";
    private const float RouteLength = 144f;
    private const float RouteStep = 4f;
    private const float RouteClearance = 3.1f;

    [MenuItem("Tools/MyBike/MYB-148/Build Route-first Scatter Preview")]
    public static void BuildFromMenu()
    {
        var result = BuildPreviewScene(writeReport: true);
        Debug.Log("MYB-148 preview built: " + result.ReportPathRelative);
    }

    [MenuItem("Tools/MyBike/MYB-148/Build + Capture Route-first Scatter")]
    public static void BuildAndCaptureFromMenu()
    {
        var result = BuildPreviewScene(writeReport: false);
        CaptureBeforeAfter(result);
        WriteReports(result);
        Debug.Log("MYB-148 capture complete: " + result.ReportPathRelative);
    }

    public static void RunBatchBuild()
    {
        try
        {
            var result = BuildPreviewScene(writeReport: true);
            Debug.Log("MYB-148 preview built: " + result.ReportPathRelative);
            EditorApplication.Exit(result.Errors.Count > 0 ? 1 : 0);
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            EditorApplication.Exit(1);
        }
    }

    public static void RunBatchBuildCapture()
    {
        try
        {
            var result = BuildPreviewScene(writeReport: false);
            CaptureBeforeAfter(result);
            WriteReports(result);
            Debug.Log("MYB-148 build/capture complete: " + result.ReportPathRelative);
            EditorApplication.Exit(result.Errors.Count > 0 ? 1 : 0);
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            EditorApplication.Exit(1);
        }
    }

    private static ScatterBuildResult BuildPreviewScene(bool writeReport)
    {
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        EnsureAssetFolder("Assets/Scenes");
        EnsureAssetFolder("Assets/MYB148");
        EnsureAssetFolder("Assets/MYB148/Editor");

        var result = new ScatterBuildResult
        {
            GeneratedAt = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture),
            Branch = GetGitValue("rev-parse --abbrev-ref HEAD"),
            Commit = GetGitValue("rev-parse --short HEAD"),
            ScenePath = ScenePath,
            ReportPathRelative = ReportRelativePath,
            MetricsPathRelative = MetricsRelativePath
        };

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "MYB148RouteFirstScatterPreview";

        ConfigureRenderSettings();
        var materials = CreateRuntimeMaterials();
        var samples = BuildRouteSamples();
        var root = new GameObject("MYB148_RouteFirstScatterPreview");
        var baseRoot = new GameObject("MYB148_CorridorBase");
        baseRoot.transform.SetParent(root.transform, false);
        var scatterRoot = new GameObject("MYB148_RouteFirstScatterAssets");
        scatterRoot.transform.SetParent(root.transform, false);

        BuildCorridorBase(baseRoot.transform, samples, materials);
        BuildRouteMarkers(baseRoot.transform, samples, materials);
        BuildScatter(scatterRoot.transform, samples, result);
        CreateCameras();
        CreateLighting();

        result.RendererCount = root.GetComponentsInChildren<Renderer>(true).Length;
        result.MeshFilterCount = root.GetComponentsInChildren<MeshFilter>(true).Length;
        result.TotalTriangles = CountTriangles(root);
        result.MinimumRouteClearance = result.Placements.Count == 0
            ? 0f
            : result.Placements.Min(placement => Mathf.Abs(placement.Offset));
        if (result.MinimumRouteClearance < RouteClearance)
        {
            result.Errors.Add("Route clearance dropped below " + FormatFloat(RouteClearance) + " m.");
        }

        EditorSceneManager.SaveScene(scene, ScenePath);
        result.Info.Add("Preview scene saved to `" + ScenePath + "`.");

        if (writeReport)
        {
            WriteReports(result);
        }

        return result;
    }

    private static void CaptureBeforeAfter(ScatterBuildResult result)
    {
        SetScatterActive(false);
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        var before = MYB145CaptureRigHelper.CaptureRouteAndOverview(
            "MYB148Batch",
            new MYB145CaptureRigHelper.CaptureOptions { TicketId = "MYB-148", State = "before" });
        result.BeforeCaptureReport = before.ReportPathRelative;
        AppendCaptureResult(result, before);

        SetScatterActive(true);
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        var after = MYB145CaptureRigHelper.CaptureRouteAndOverview(
            "MYB148Batch",
            new MYB145CaptureRigHelper.CaptureOptions { TicketId = "MYB-148", State = "after" });
        result.AfterCaptureReport = after.ReportPathRelative;
        AppendCaptureResult(result, after);

        CreateComparisonSheets(result, before, after);
        WriteVisualCaptureReport(result, before, after);
    }

    private static void AppendCaptureResult(ScatterBuildResult result, MYB145CaptureRigHelper.CaptureResult capture)
    {
        foreach (var error in capture.Errors)
        {
            result.Errors.Add("MYB-145 " + error.Code + ": " + error.Message);
        }

        foreach (var warning in capture.Warnings)
        {
            result.Warnings.Add("MYB-145 " + warning.Code + ": " + warning.Message);
        }

        foreach (var record in capture.Captures)
        {
            result.CapturePaths.Add(record.Path);
        }
    }

    private static void SetScatterActive(bool active)
    {
        var scatterRoot = FindSceneObjectByName("MYB148_RouteFirstScatterAssets");
        if (scatterRoot == null)
        {
            throw new InvalidOperationException("MYB148_RouteFirstScatterAssets not found.");
        }

        scatterRoot.SetActive(active);
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

    private static void CreateComparisonSheets(
        ScatterBuildResult result,
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

    private static void BuildScatter(Transform parent, IReadOnlyList<RouteSample> samples, ScatterBuildResult result)
    {
        var kit = LoadKit(result);
        for (var meters = 10f; meters <= RouteLength - 8f; meters += 6f)
        {
            var side = ((int)(meters / 6f) % 2 == 0) ? -1f : 1f;
            if (!IsBreathingWindow(meters))
            {
                Place(kit, "myb_forest_leaf_moss_mat_" + Variant(meters, "abc"), "Forest floor", "shoulder", side, meters, 3.45f, 0.86f, parent, result);
                if (Mathf.RoundToInt(meters) % 12 == 4)
                {
                    Place(kit, "myb_forest_fern_" + Variant(meters + 3f, "abc"), "Ferns", "shoulder", -side, meters + 2f, 3.85f, 0.92f, parent, result);
                }
            }

            if (Mathf.RoundToInt(meters) % 18 == 10)
            {
                Place(kit, "myb_forest_rock_mossy_" + Variant(meters, "ab"), "Rocks", "close edge", side, meters, 5.45f, 0.96f, parent, result);
            }

            if (!IsBreathingWindow(meters) && Mathf.RoundToInt(meters) % 24 == 16)
            {
                Place(kit, "myb_forest_root_cluster_lateral_a", "Roots", "close edge", -side, meters, 5.9f, 1.04f, parent, result);
            }

            if (Mathf.RoundToInt(meters) % 18 == 4)
            {
                var trunk = TrunkVariant(meters);
                Place(kit, trunk, "Trees / trunks", "mid edge", side, meters, 8.6f + Jitter(meters, 1.1f), 1.0f + Jitter(meters + 5f, 0.12f), parent, result);
            }

            if (!IsBreathingWindow(meters) && Mathf.RoundToInt(meters) % 30 == 22)
            {
                Place(kit, "myb_forest_canopy_mass_" + Variant(meters, "ab"), "Canopy", "back wall", side, meters, 12.8f + Jitter(meters, 1.3f), 1.16f, parent, result, yOffset: 5.5f);
            }

            if (Mathf.RoundToInt(meters) % 36 == 28)
            {
                Place(kit, TrunkVariant(meters + 11f), "Trees / trunks", "silhouette line", -side, meters, 16.4f + Jitter(meters, 1.6f), 1.2f, parent, result);
                Place(kit, "myb_forest_canopy_mass_" + Variant(meters + 6f, "ab"), "Canopy", "silhouette line", -side, meters + 3f, 17.6f, 1.25f, parent, result, yOffset: 6.1f);
            }
        }

        Place(kit, "myb_forest_root_arch_a", "Roots / arch landmark", "close edge landmark", 1f, 54f, 6.2f, 1.18f, parent, result);
        Place(kit, "myb_forest_fallen_log_a", "Fallen log", "close edge landmark", -1f, 90f, 5.9f, 1.08f, parent, result);
        Place(kit, "myb_forest_rock_marker_a", "Marker rock", "mid edge landmark", 1f, 116f, 7.4f, 1.12f, parent, result);
        Place(kit, "myb_forest_dead_branch_a", "Dead branches", "shoulder detail", -1f, 32f, 3.9f, 0.94f, parent, result);
        Place(kit, "myb_forest_dead_branch_b", "Dead branches", "shoulder detail", 1f, 104f, 3.75f, 0.9f, parent, result);
    }

    private static Dictionary<string, GameObject> LoadKit(ScatterBuildResult result)
    {
        var ids = new[]
        {
            "myb_forest_trunk_ancient_a",
            "myb_forest_trunk_broken_a",
            "myb_forest_trunk_leaning_a",
            "myb_forest_trunk_knotted_a",
            "myb_forest_root_cluster_lateral_a",
            "myb_forest_root_cluster_ground_a",
            "myb_forest_root_arch_a",
            "myb_forest_rock_mossy_a",
            "myb_forest_rock_mossy_b",
            "myb_forest_rock_marker_a",
            "myb_forest_fern_a",
            "myb_forest_fern_b",
            "myb_forest_fern_c",
            "myb_forest_leaf_moss_mat_a",
            "myb_forest_leaf_moss_mat_b",
            "myb_forest_leaf_moss_mat_c",
            "myb_forest_dead_branch_a",
            "myb_forest_dead_branch_b",
            "myb_forest_canopy_mass_a",
            "myb_forest_canopy_mass_b",
            "myb_forest_fallen_log_a"
        };

        var kit = new Dictionary<string, GameObject>();
        foreach (var id in ids)
        {
            var path = KitRoot + "/" + id + ".fbx";
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (asset == null)
            {
                result.Errors.Add("Missing MYB-147 kit asset: `" + path + "`.");
            }
            else
            {
                kit[id] = asset;
            }
        }

        return kit;
    }

    private static void Place(
        IReadOnlyDictionary<string, GameObject> kit,
        string assetId,
        string family,
        string band,
        float side,
        float meters,
        float distanceFromRoute,
        float scale,
        Transform parent,
        ScatterBuildResult result,
        float yOffset = 0f)
    {
        if (!kit.TryGetValue(assetId, out var asset))
        {
            return;
        }

        var sample = SampleAt(meters);
        var signedOffset = side * distanceFromRoute;
        var position = sample.Position + sample.Right * signedOffset + Vector3.up * (TerrainHeight(meters, signedOffset) + yOffset);
        var yaw = Mathf.Atan2(sample.Forward.x, sample.Forward.z) * Mathf.Rad2Deg + 90f * side + Jitter(meters + signedOffset, 22f);
        var instance = PrefabUtility.InstantiatePrefab(asset) as GameObject;
        if (instance == null)
        {
            instance = UnityEngine.Object.Instantiate(asset);
        }

        instance.name = "MYB148_" + assetId + "_" + Slug(band) + "_" + result.Placements.Count.ToString("00", CultureInfo.InvariantCulture);
        instance.transform.SetParent(parent, false);
        instance.transform.position = position;
        instance.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        instance.transform.localScale = Vector3.one * Mathf.Max(0.1f, scale);

        var placement = new PlacementRecord
        {
            AssetId = assetId,
            Family = family,
            Band = band,
            Meters = meters,
            Offset = signedOffset,
            Scale = scale,
            RendererCount = instance.GetComponentsInChildren<Renderer>(true).Length,
            TriangleCount = CountTriangles(instance),
            MaterialCount = CountMaterials(instance)
        };
        result.Placements.Add(placement);
    }

    private static void BuildCorridorBase(Transform root, IReadOnlyList<RouteSample> samples, IReadOnlyDictionary<string, Material> materials)
    {
        CreateBand("Road_Ribbon", root, samples, -2.05f, 2.05f, materials["road"], RoadHeight);
        CreateBand("Left_Shoulder", root, samples, -3.45f, -2.05f, materials["shoulder"], ShoulderHeight);
        CreateBand("Right_Shoulder", root, samples, 2.05f, 3.45f, materials["shoulder"], ShoulderHeight);
        CreateBand("Left_CloseEdge", root, samples, -6.5f, -3.45f, materials["closeEdge"], ForestFloorHeight);
        CreateBand("Right_CloseEdge", root, samples, 3.45f, 6.5f, materials["closeEdge"], ForestFloorHeight);
        CreateBand("Left_MidEdge", root, samples, -11.4f, -6.5f, materials["midEdge"], ForestFloorHeight);
        CreateBand("Right_MidEdge", root, samples, 6.5f, 11.4f, materials["midEdge"], ForestFloorHeight);
        CreateBand("Left_BackWall", root, samples, -18.5f, -11.4f, materials["backWall"], BackWallHeight);
        CreateBand("Right_BackWall", root, samples, 11.4f, 18.5f, materials["backWall"], BackWallHeight);
    }

    private static void BuildRouteMarkers(Transform root, IReadOnlyList<RouteSample> samples, IReadOnlyDictionary<string, Material> materials)
    {
        foreach (var markerMeters in new[] { 36f, 78f, 120f })
        {
            var sample = SampleAt(markerMeters);
            for (var side = -1; side <= 1; side += 2)
            {
                var marker = GameObject.CreatePrimitive(PrimitiveType.Cube);
                marker.name = "MYB148_BreathingWindowMarker_" + markerMeters.ToString("0", CultureInfo.InvariantCulture) + "_" + (side < 0 ? "L" : "R");
                marker.transform.SetParent(root, false);
                marker.transform.position = sample.Position + sample.Right * side * 4.15f + Vector3.up * 0.025f;
                marker.transform.rotation = Quaternion.LookRotation(sample.Forward, Vector3.up);
                marker.transform.localScale = new Vector3(0.25f, 0.04f, 2.2f);
                marker.GetComponent<Renderer>().sharedMaterial = materials["windowMarker"];
                UnityEngine.Object.DestroyImmediate(marker.GetComponent<Collider>());
            }
        }
    }

    private static void CreateBand(
        string name,
        Transform parent,
        IReadOnlyList<RouteSample> samples,
        float innerOffset,
        float outerOffset,
        Material material,
        Func<float, float, float> heightFunc)
    {
        const int lateralSteps = 4;
        var vertices = new List<Vector3>();
        var uvs = new List<Vector2>();
        var triangles = new List<int>();

        for (var i = 0; i < samples.Count; i++)
        {
            var sample = samples[i];
            for (var j = 0; j <= lateralSteps; j++)
            {
                var t = j / (float)lateralSteps;
                var offset = Mathf.Lerp(innerOffset, outerOffset, t);
                var position = sample.Position + sample.Right * offset + Vector3.up * heightFunc(sample.Meters, offset);
                vertices.Add(position);
                uvs.Add(new Vector2(t, sample.Meters / 16f));
            }
        }

        var stride = lateralSteps + 1;
        for (var i = 0; i < samples.Count - 1; i++)
        {
            for (var j = 0; j < lateralSteps; j++)
            {
                var a = i * stride + j;
                var b = i * stride + j + 1;
                var c = (i + 1) * stride + j;
                var d = (i + 1) * stride + j + 1;
                triangles.Add(a);
                triangles.Add(c);
                triangles.Add(b);
                triangles.Add(b);
                triangles.Add(c);
                triangles.Add(d);
            }
        }

        var mesh = new Mesh { name = "MYB148_" + name + "_Mesh" };
        mesh.SetVertices(vertices);
        mesh.SetUVs(0, uvs);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        var go = new GameObject("MYB148_" + name);
        go.transform.SetParent(parent, false);
        go.AddComponent<MeshFilter>().sharedMesh = mesh;
        go.AddComponent<MeshRenderer>().sharedMaterial = material;
    }

    private static void CreateCameras()
    {
        var routePosition = RoutePosition(7.5f) + Vector3.up * 1.55f;
        var routeTarget = RoutePosition(42f) + Vector3.up * 1.15f;
        var routeObject = new GameObject("RouteCamera");
        var routeCamera = routeObject.AddComponent<Camera>();
        routeObject.transform.position = routePosition;
        routeObject.transform.rotation = Quaternion.LookRotation((routeTarget - routePosition).normalized, Vector3.up);
        routeCamera.fieldOfView = 50f;
        routeCamera.nearClipPlane = 0.05f;
        routeCamera.farClipPlane = 180f;
        routeCamera.clearFlags = CameraClearFlags.SolidColor;
        routeCamera.backgroundColor = new Color(0.35f, 0.40f, 0.37f);

        var overviewObject = new GameObject("OverviewCamera");
        var overviewCamera = overviewObject.AddComponent<Camera>();
        overviewObject.transform.position = new Vector3(0f, 86f, 66f);
        overviewObject.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        overviewCamera.orthographic = true;
        overviewCamera.orthographicSize = 42f;
        overviewCamera.nearClipPlane = 0.05f;
        overviewCamera.farClipPlane = 220f;
        overviewCamera.clearFlags = CameraClearFlags.SolidColor;
        overviewCamera.backgroundColor = new Color(0.35f, 0.40f, 0.37f);
    }

    private static void CreateLighting()
    {
        var sunObject = new GameObject("MYB148_SoftRouteSun");
        var sun = sunObject.AddComponent<Light>();
        sun.type = LightType.Directional;
        sun.color = new Color(1f, 0.91f, 0.76f);
        sun.intensity = 1.08f;
        sunObject.transform.rotation = Quaternion.Euler(38f, -28f, 0f);

        var fillObject = new GameObject("MYB148_CoolForestFill");
        var fill = fillObject.AddComponent<Light>();
        fill.type = LightType.Directional;
        fill.color = new Color(0.48f, 0.60f, 0.66f);
        fill.intensity = 0.22f;
        fillObject.transform.rotation = Quaternion.Euler(18f, 142f, 0f);
    }

    private static void ConfigureRenderSettings()
    {
        RenderSettings.skybox = null;
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.48f, 0.52f, 0.45f);
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogColor = new Color(0.34f, 0.39f, 0.36f);
        RenderSettings.fogDensity = 0.0085f;
    }

    private static Dictionary<string, Material> CreateRuntimeMaterials()
    {
        var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        return new Dictionary<string, Material>
        {
            ["road"] = RuntimeMaterial(shader, "MYB148_Road", new Color(0.18f, 0.17f, 0.145f)),
            ["shoulder"] = RuntimeMaterial(shader, "MYB148_Shoulder", new Color(0.30f, 0.25f, 0.17f)),
            ["closeEdge"] = RuntimeMaterial(shader, "MYB148_CloseEdge", new Color(0.17f, 0.26f, 0.12f)),
            ["midEdge"] = RuntimeMaterial(shader, "MYB148_MidEdge", new Color(0.12f, 0.18f, 0.10f)),
            ["backWall"] = RuntimeMaterial(shader, "MYB148_BackWall", new Color(0.09f, 0.13f, 0.08f)),
            ["windowMarker"] = RuntimeMaterial(shader, "MYB148_BreathingWindowMarker", new Color(0.22f, 0.33f, 0.15f))
        };
    }

    private static Material RuntimeMaterial(Shader shader, string name, Color color)
    {
        var material = new Material(shader) { name = name, color = color };
        if (material.HasProperty("_BaseColor"))
        {
            material.SetColor("_BaseColor", color);
        }

        return material;
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

    private static string Variant(float seed, string variants)
    {
        var index = Mathf.Abs(Mathf.RoundToInt(seed * 13.37f)) % variants.Length;
        return variants[index].ToString();
    }

    private static string TrunkVariant(float seed)
    {
        switch (Mathf.Abs(Mathf.RoundToInt(seed * 9.3f)) % 4)
        {
            case 0: return "myb_forest_trunk_ancient_a";
            case 1: return "myb_forest_trunk_broken_a";
            case 2: return "myb_forest_trunk_leaning_a";
            default: return "myb_forest_trunk_knotted_a";
        }
    }

    private static float Jitter(float seed, float amount)
    {
        return (Mathf.PerlinNoise(seed * 0.17f, seed * 0.071f) - 0.5f) * 2f * amount;
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
            .Sum(renderer => renderer.sharedMaterials == null ? 0 : renderer.sharedMaterials.Length);
    }

    private static string Slug(string value)
    {
        return new string((value ?? string.Empty).ToLowerInvariant().Select(character => char.IsLetterOrDigit(character) ? character : '_').ToArray()).Trim('_');
    }

    private static string FormatFloat(float value)
    {
        return value.ToString("0.###", CultureInfo.InvariantCulture);
    }

    private static void WriteReports(ScatterBuildResult result)
    {
        WriteImplementationReport(result);
        WriteMetricsJson(result);
    }

    private static void WriteImplementationReport(ScatterBuildResult result)
    {
        var reportPath = ToRepoPath(ReportRelativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(reportPath) ?? GetRepoRoot());
        var builder = new StringBuilder();
        builder.AppendLine("# MYB-148 Route-first Scatter Implementation Report");
        builder.AppendLine();
        builder.AppendLine("Status:");
        builder.AppendLine("- In Progress / ready for review evidence.");
        builder.AppendLine();
        builder.AppendLine("Generated at:");
        builder.AppendLine("- " + result.GeneratedAt);
        builder.AppendLine();
        builder.AppendLine("Scene:");
        builder.AppendLine("- `" + ScenePath + "`");
        builder.AppendLine();
        builder.AppendLine("Scope:");
        builder.AppendLine("- Route-first scatter preview using the MYB-147 candidate kit.");
        builder.AppendLine("- Dedicated preview scene only.");
        builder.AppendLine("- No canonical ride scene was overwritten.");
        builder.AppendLine("- No Meshy, Tripo, external text-to-3D, or new external asset source.");
        builder.AppendLine("- No production asset promotion.");
        builder.AppendLine();
        builder.AppendLine("Validation surface:");
        builder.AppendLine("- Diagnostic Surface / preview evidence for MYB-148.");
        builder.AppendLine("- Route-camera validation is required for future production closure.");
        builder.AppendLine("- Isolated or preview evidence does not prove `Premium target`.");
        builder.AppendLine();
        builder.AppendLine("## Band Policy");
        builder.AppendLine();
        builder.AppendLine("| Band | Offset from route | Primary families | Intent |");
        builder.AppendLine("|---|---:|---|---|");
        builder.AppendLine("| Shoulder | 3.1-4.2 m | leaf/moss mats, ferns, dead branches | Foreground richness without route obstruction. |");
        builder.AppendLine("| Close edge | 5-6.5 m | roots, mossy rocks, root arch | Natural side structure and scenic threshold beats. |");
        builder.AppendLine("| Mid edge | 7.5-10 m | trunks, rocks, roots | Main forest corridor body. |");
        builder.AppendLine("| Back wall | 11.5-14.5 m | trunks, canopy masses | Distant density and depth. |");
        builder.AppendLine("| Silhouette line | 15-18.5 m | tall trunks, canopy masses | Strong readable outer forest outline. |");
        builder.AppendLine();
        builder.AppendLine("Breathing windows:");
        builder.AppendLine("- 36 m, 78 m, 120 m keep reduced density so the route does not become wallpaper.");
        builder.AppendLine();
        builder.AppendLine("## Metrics");
        builder.AppendLine();
        builder.AppendLine("- Scatter placements: " + result.Placements.Count);
        builder.AppendLine("- Scene renderers: " + result.RendererCount);
        builder.AppendLine("- Scene mesh filters: " + result.MeshFilterCount);
        builder.AppendLine("- Approximate scene triangles: " + result.TotalTriangles);
        builder.AppendLine("- Minimum scatter distance from route center: " + FormatFloat(result.MinimumRouteClearance) + " m");
        builder.AppendLine("- Metrics JSON: `" + MetricsRelativePath + "`");
        builder.AppendLine();
        builder.AppendLine("### Placements By Band");
        builder.AppendLine();
        builder.AppendLine("| Band | Count |");
        builder.AppendLine("|---|---:|");
        foreach (var group in result.Placements.GroupBy(placement => placement.Band).OrderBy(group => group.Key))
        {
            builder.AppendLine("| " + group.Key + " | " + group.Count() + " |");
        }
        builder.AppendLine();
        builder.AppendLine("### Placements By Family");
        builder.AppendLine();
        builder.AppendLine("| Family | Count |");
        builder.AppendLine("|---|---:|");
        foreach (var group in result.Placements.GroupBy(placement => placement.Family).OrderBy(group => group.Key))
        {
            builder.AppendLine("| " + group.Key + " | " + group.Count() + " |");
        }
        builder.AppendLine();
        builder.AppendLine("## Capture Evidence");
        builder.AppendLine();
        builder.AppendLine("Explicit baseline:");
        builder.AppendLine("- Before state is the same MYB-148 preview scene with `MYB148_RouteFirstScatterAssets` disabled.");
        builder.AppendLine("- After state is the same scene with route-first scatter enabled.");
        builder.AppendLine("- Baseline is explicit and generated by the MYB-148 builder, not inferred from latest files.");
        builder.AppendLine();
        builder.AppendLine("MYB-145 capture reports:");
        builder.AppendLine("- Before: `" + (result.BeforeCaptureReport ?? "") + "`");
        builder.AppendLine("- After: `" + (result.AfterCaptureReport ?? "") + "`");
        builder.AppendLine();
        builder.AppendLine("Captures:");
        foreach (var capture in result.CapturePaths)
        {
            builder.AppendLine("- `" + capture + "`");
        }
        builder.AppendLine();
        builder.AppendLine("Comparison sheets:");
        builder.AppendLine("- Route: `" + (result.RouteComparisonPath ?? "") + "`");
        builder.AppendLine("- Overview: `" + (result.OverviewComparisonPath ?? "") + "`");
        builder.AppendLine("- Capture report: `" + (result.VisualCaptureReportPath ?? "") + "`");
        builder.AppendLine();
        builder.AppendLine("## Visual Rubric Checkpoint");
        builder.AppendLine();
        builder.AppendLine("| Criterion | Score | Note |");
        builder.AppendLine("|---|---:|---|");
        builder.AppendLine("| Route readability | 3 | Route remains visible in the preview, with reduced-density breathing windows. |");
        builder.AppendLine("| Silhouette quality | 3 | MYB-147 trunk/root silhouettes improve the corridor body, but this is not yet a premium setpiece pass. |");
        builder.AppendLine("| Lighting mood | 2 | Lighting/fog are neutral preview support only; MYB-151 owns final mood. |");
        builder.AppendLine("| Material coherence | 3 | Candidate kit materials remain simple but coherent enough for scatter testing. |");
        builder.AppendLine("| Foreground richness | 3 | Shoulders receive ferns, leaf/moss mats, and dead branches without blocking the road. |");
        builder.AppendLine("| Midground density | 3 | Density now follows bands instead of random placement. |");
        builder.AppendLine("| Background depth | 3 | Back wall and silhouette line add depth, still preview-grade. |");
        builder.AppendLine("| Scale credibility | 3 | Offsets and scale jitter stay plausible for route testing. |");
        builder.AppendLine("| Composition rhythm | 3 | Breathing windows and landmarks create first rhythm pass. |");
        builder.AppendLine();
        builder.AppendLine("Average:");
        builder.AppendLine("- 2.89");
        builder.AppendLine();
        builder.AppendLine("Premium target reached:");
        builder.AppendLine("- No");
        builder.AppendLine();
        builder.AppendLine("Verdict:");
        builder.AppendLine("- Checkpoint insuffisant");
        builder.AppendLine();
        builder.AppendLine("## Validator / Performance Notes");
        builder.AppendLine();
        builder.AppendLine("- Run MYB-144 after generating the scene to confirm manifest / asset gate remains without ERROR.");
        builder.AppendLine("- Run narrow Unity validation and capture checks before review.");
        builder.AppendLine("- Performance spend is intended to be visible from the route camera; reduce outside route-camera density before visible premium elements if needed.");
        builder.AppendLine();
        builder.AppendLine("## Warnings");
        if (result.Warnings.Count == 0)
        {
            builder.AppendLine("- None.");
        }
        else
        {
            foreach (var warning in result.Warnings)
            {
                builder.AppendLine("- " + warning);
            }
        }
        builder.AppendLine();
        builder.AppendLine("## Errors");
        if (result.Errors.Count == 0)
        {
            builder.AppendLine("- None.");
        }
        else
        {
            foreach (var error in result.Errors)
            {
                builder.AppendLine("- " + error);
            }
        }

        File.WriteAllText(reportPath, builder.ToString());
    }

    private static void WriteVisualCaptureReport(
        ScatterBuildResult result,
        MYB145CaptureRigHelper.CaptureResult before,
        MYB145CaptureRigHelper.CaptureResult after)
    {
        var timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH-mm-ssZ", CultureInfo.InvariantCulture);
        result.VisualCaptureReportPath = VisualRootRelative + "/" + timestamp + "-capture-report.md";
        var path = ToRepoPath(result.VisualCaptureReportPath);
        Directory.CreateDirectory(Path.GetDirectoryName(path) ?? GetRepoRoot());

        var builder = new StringBuilder();
        builder.AppendLine("# MYB-148 Capture Report");
        builder.AppendLine();
        builder.AppendLine("Ticket:");
        builder.AppendLine("- MYB-148");
        builder.AppendLine();
        builder.AppendLine("Mode:");
        builder.AppendLine("- explicit before/after comparison");
        builder.AppendLine();
        builder.AppendLine("Scene:");
        builder.AppendLine("- `" + ScenePath + "`");
        builder.AppendLine();
        builder.AppendLine("Explicit Baseline:");
        builder.AppendLine("- Before selected by: MYB-148 builder");
        builder.AppendLine("- Reason: same scene with route-first scatter disabled before comparing the MYB-147 kit placement pass.");
        builder.AppendLine("- Source: `MYB148_RouteFirstScatterBuilder.RunBatchBuildCapture`");
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
        builder.AppendLine("- Route-camera production validation remains deferred to future Art Rescue closure review.");
        builder.AppendLine();
        builder.AppendLine("Verdict:");
        builder.AppendLine("- " + (result.Errors.Count > 0 ? "FAIL" : result.Warnings.Count > 0 ? "PASS_WITH_WARNINGS" : "PASS"));

        File.WriteAllText(path, builder.ToString());
    }

    private static void WriteMetricsJson(ScatterBuildResult result)
    {
        var path = ToRepoPath(MetricsRelativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(path) ?? GetRepoRoot());
        var builder = new StringBuilder();
        builder.AppendLine("{");
        builder.AppendLine("  \"schemaVersion\": 1,");
        builder.AppendLine("  \"ticket\": \"MYB-148\",");
        builder.AppendLine("  \"generatedAt\": \"" + EscapeJson(result.GeneratedAt) + "\",");
        builder.AppendLine("  \"scenePath\": \"" + EscapeJson(ScenePath) + "\",");
        builder.AppendLine("  \"kitRoot\": \"" + EscapeJson(KitRoot) + "\",");
        builder.AppendLine("  \"placements\": " + result.Placements.Count + ",");
        builder.AppendLine("  \"rendererCount\": " + result.RendererCount + ",");
        builder.AppendLine("  \"meshFilterCount\": " + result.MeshFilterCount + ",");
        builder.AppendLine("  \"totalTriangles\": " + result.TotalTriangles + ",");
        builder.AppendLine("  \"minimumRouteClearanceMeters\": " + result.MinimumRouteClearance.ToString(CultureInfo.InvariantCulture) + ",");
        builder.AppendLine("  \"placementsDetail\": [");
        for (var i = 0; i < result.Placements.Count; i++)
        {
            var placement = result.Placements[i];
            builder.Append("    { ");
            builder.Append("\"assetId\": \"" + EscapeJson(placement.AssetId) + "\", ");
            builder.Append("\"family\": \"" + EscapeJson(placement.Family) + "\", ");
            builder.Append("\"band\": \"" + EscapeJson(placement.Band) + "\", ");
            builder.Append("\"meters\": " + placement.Meters.ToString(CultureInfo.InvariantCulture) + ", ");
            builder.Append("\"offset\": " + placement.Offset.ToString(CultureInfo.InvariantCulture) + ", ");
            builder.Append("\"scale\": " + placement.Scale.ToString(CultureInfo.InvariantCulture) + ", ");
            builder.Append("\"rendererCount\": " + placement.RendererCount + ", ");
            builder.Append("\"triangleCount\": " + placement.TriangleCount + ", ");
            builder.Append("\"materialCount\": " + placement.MaterialCount);
            builder.Append(" }");
            if (i < result.Placements.Count - 1)
            {
                builder.Append(",");
            }
            builder.AppendLine();
        }
        builder.AppendLine("  ]");
        builder.AppendLine("}");
        File.WriteAllText(path, builder.ToString());
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

    private sealed class PlacementRecord
    {
        public string AssetId;
        public string Family;
        public string Band;
        public float Meters;
        public float Offset;
        public float Scale;
        public int RendererCount;
        public int TriangleCount;
        public int MaterialCount;
    }

    private sealed class ScatterBuildResult
    {
        public string GeneratedAt;
        public string Branch;
        public string Commit;
        public string ScenePath;
        public string ReportPathRelative;
        public string MetricsPathRelative;
        public string BeforeCaptureReport;
        public string AfterCaptureReport;
        public string RouteComparisonPath;
        public string OverviewComparisonPath;
        public string VisualCaptureReportPath;
        public int RendererCount;
        public int MeshFilterCount;
        public int TotalTriangles;
        public float MinimumRouteClearance;
        public readonly List<PlacementRecord> Placements = new List<PlacementRecord>();
        public readonly List<string> CapturePaths = new List<string>();
        public readonly List<string> Info = new List<string>();
        public readonly List<string> Warnings = new List<string>();
        public readonly List<string> Errors = new List<string>();
    }
}

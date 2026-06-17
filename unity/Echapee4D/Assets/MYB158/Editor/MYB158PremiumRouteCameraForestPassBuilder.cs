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

public static class MYB158PremiumRouteCameraForestPassBuilder
{
    private const string SourceScenePath = "Assets/Scenes/MYB149GroundMaterialPreview.unity";
    private const string OutputScenePath = "Assets/Scenes/MYB158PremiumRouteCameraForestPass.unity";
    private const string GeneratedRootName = "MYB158_PremiumRouteCameraForestPassRoot";
    private const string KitRoot = "Assets/Echappee/Art/Candidates/MYB_ForestKit_V0";
    private const string ImplementationRootRelative = "_bmad-output/implementation-artifacts/MYB-158";
    private const string VisualRootRelative = "_bmad-output/visual-checkpoints/MYB-158";
    private const string ReportRelativePath = ImplementationRootRelative + "/myb-158-premium-route-camera-pass-report.md";
    private const string MetricsRelativePath = ImplementationRootRelative + "/myb-158-premium-route-camera-pass-metrics.json";
    private const float RouteLength = 144f;
    private const float RouteStep = 4f;
    private const float RoadHalfWidth = 2.05f;
    private const float SinkMeters = 0.035f;

    [MenuItem("Tools/MyBike/MYB-158/Build Premium Route Camera Forest Pass")]
    public static void BuildFromMenu()
    {
        var result = BuildPreviewScene(writeReport: true);
        Debug.Log(result.ToConsoleSummary());
    }

    [MenuItem("Tools/MyBike/MYB-158/Build + Capture + Validate")]
    public static void BuildCaptureValidateFromMenu()
    {
        var result = BuildCaptureValidate();
        Debug.Log(result.ToConsoleSummary());
    }

    public static void RunBatchBuild()
    {
        var result = BuildPreviewScene(writeReport: true);
        if (result.Errors.Count > 0)
        {
            throw new InvalidOperationException(result.ToConsoleSummary());
        }
    }

    public static void RunBatchBuildCaptureValidate()
    {
        var result = BuildCaptureValidate();
        if (result.Errors.Count > 0)
        {
            throw new InvalidOperationException(result.ToConsoleSummary());
        }
    }

    private static BuildResult BuildCaptureValidate()
    {
        var beforeCapture = CaptureScene(SourceScenePath, "before");
        var result = BuildPreviewScene(writeReport: true);
        var afterCapture = CaptureScene(OutputScenePath, "after");
        AppendCaptureResult(result, beforeCapture);
        AppendCaptureResult(result, afterCapture);
        CreateComparisonSheets(result, beforeCapture, afterCapture);

        var supportValidation = MYB156VisualSupportValidator.RunValidation("MYB-158-BuildCaptureValidate");
        result.VisualSupportVerdict = supportValidation.Verdict;
        result.RouteVisibleUnsupportedCanopyCount = supportValidation.RouteVisibleUnsupportedCanopyCount;
        result.UnsupportedCanopyCount = supportValidation.UnsupportedCanopyCount;
        result.VisualSupportReportRelativePath = "_bmad-output/unity-test-results/myb-156-visual-support-validator-report.md";

        if (supportValidation.RouteVisibleUnsupportedCanopyCount > 0 || supportValidation.UnsupportedCanopyCount > 0)
        {
            result.Errors.Add("MYB-156 visual-support validation no longer passes after MYB-158.");
        }

        WriteReports(result);
        if (File.Exists(ToProjectPath(OutputScenePath)))
        {
            EditorSceneManager.OpenScene(OutputScenePath, OpenSceneMode.Single);
        }

        return result;
    }

    private static BuildResult BuildPreviewScene(bool writeReport)
    {
        var result = CreateResult();
        if (!File.Exists(ToProjectPath(SourceScenePath)))
        {
            result.Errors.Add("Source scene is missing: `" + SourceScenePath + "`.");
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
            result.Info.Add("Existing MYB-158 root removed before deterministic rebuild.");
        }

        var root = new GameObject(GeneratedRootName);
        var sideMasses = CreateChild(root.transform, "MYB158_SideCorridorMasses");
        var trunkRhythm = CreateChild(root.transform, "MYB158_SceneLocalTrunkRhythm");
        var floorRelief = CreateChild(root.transform, "MYB158_ForestFloorRelief");
        var depthPockets = CreateChild(root.transform, "MYB158_DepthPockets");
        var localMaterials = CreateChild(root.transform, "MYB158_LocalMaterials");
        localMaterials.SetActive(false);

        var materials = CreateRuntimeMaterials();
        result.SceneLocalMaterialCount = materials.Count;
        var kit = LoadKit(result);
        var routeCamera = FindSceneObjectByName("RouteCamera")?.GetComponent<Camera>();
        var routePlanes = routeCamera == null ? null : GeometryUtility.CalculateFrustumPlanes(routeCamera);

        BuildSideCorridorMasses(kit, materials, sideMasses.transform, routePlanes, result);
        BuildSceneLocalTrunkRhythm(materials, trunkRhythm.transform, routePlanes, result);
        BuildForestFloorRelief(materials, floorRelief.transform, result);
        BuildDepthPockets(materials, depthPockets.transform, result);
        ConfigureRouteMood();

        result.RendererCount = root.GetComponentsInChildren<Renderer>(true).Length;
        result.MeshFilterCount = root.GetComponentsInChildren<MeshFilter>(true).Length;
        result.TotalTriangles = CountTriangles(root);
        result.MinimumRouteClearance = result.Placements.Count == 0
            ? 0f
            : result.Placements.Min(placement => Mathf.Abs(placement.Offset) - placement.Radius);
        result.RouteOverlapCount = result.Placements.Count(placement => Mathf.Abs(placement.Offset) - placement.Radius <= RoadHalfWidth);
        result.FloatingAssetCount = result.GroundingRecords.Count(record => record.BottomClearance > 0.05f);
        result.MaxFloatingClearance = result.GroundingRecords.Count == 0 ? 0f : result.GroundingRecords.Max(record => Mathf.Max(0f, record.BottomClearance));
        result.SinkingAssetCount = result.GroundingRecords.Count(record => record.BottomClearance < -0.10f);
        result.MaxSinkingDepth = result.GroundingRecords.Count == 0 ? 0f : result.GroundingRecords.Max(record => Mathf.Max(0f, -record.BottomClearance));
        result.RouteVisibleFloatingAssetCount = result.GroundingRecords.Count(record => record.RouteVisible && record.BottomClearance > 0.10f);

        if (result.RouteOverlapCount > 0)
        {
            result.Errors.Add("MYB-158 placements overlap the route readability corridor. Overlap count: " + result.RouteOverlapCount + ".");
        }

        if (result.RouteVisibleFloatingAssetCount > 0)
        {
            result.Errors.Add("MYB-158 has route-visible floating assets above MYB-155 blocking threshold.");
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

    private static void BuildSideCorridorMasses(
        IReadOnlyDictionary<string, GameObject> kit,
        IReadOnlyDictionary<string, Material> materials,
        Transform parent,
        Plane[] routePlanes,
        BuildResult result)
    {
        var beats = new[]
        {
            new MassBeat(18f, -1f, 5.2f),
            new MassBeat(24f, 1f, 5.6f),
            new MassBeat(42f, -1f, 5.5f),
            new MassBeat(52f, 1f, 5.4f),
            new MassBeat(68f, -1f, 5.8f),
            new MassBeat(84f, 1f, 5.6f),
            new MassBeat(98f, -1f, 5.5f),
            new MassBeat(112f, 1f, 5.7f),
            new MassBeat(128f, -1f, 5.9f)
        };

        foreach (var beat in beats)
        {
            PlaceGroundedKit(
                kit,
                "myb_forest_root_cluster_lateral_a",
                "Roots",
                "premium root grounding",
                beat.Side,
                beat.Meters + 1.35f,
                beat.Distance + 0.25f,
                0.88f + Mathf.Abs(Jitter(beat.Meters + 3f, 0.08f)),
                0.82f,
                parent,
                routePlanes,
                result);

            PlaceGroundedKit(
                kit,
                RockVariant(beat.Meters),
                "Rocks",
                "premium mossy stone",
                -beat.Side,
                beat.Meters + 2.2f,
                5.35f + Mathf.Abs(Jitter(beat.Meters + 9f, 0.55f)),
                0.72f,
                0.72f,
                parent,
                routePlanes,
                result);
        }

        var backlineMeters = new[] { 20f, 32f, 46f, 62f, 76f, 92f, 108f, 122f, 136f };
        foreach (var meters in backlineMeters)
        {
            for (var side = -1; side <= 1; side += 2)
            {
                if (IsBreathingWindow(meters) && side > 0)
                {
                    continue;
                }

                PlaceGroundedKit(
                    kit,
                    TrunkVariant(meters + side * 7f),
                    "Trees / trunks",
                    "premium distant kit accent",
                    side,
                    meters + Jitter(meters + side, 1.1f),
                    11.4f + Mathf.Abs(Jitter(meters + side * 11f, 1.1f)),
                    0.52f + Mathf.Abs(Jitter(meters + 4f, 0.08f)),
                    0.48f,
                    parent,
                    routePlanes,
                    result);
            }
        }

        PlaceGroundedKit(kit, "myb_forest_root_arch_a", "Roots / arch landmark", "premium threshold root arch", 1f, 66f, 6.75f, 0.88f, 0.82f, parent, routePlanes, result);
        PlaceGroundedKit(kit, "myb_forest_fallen_log_a", "Fallen log", "premium foreground fallen log", -1f, 102f, 6.2f, 0.82f, 0.64f, parent, routePlanes, result);
        PlaceGroundedKit(kit, "myb_forest_dead_branch_a", "Dead branches", "premium foreground dead branch", 1f, 31f, 4.45f, 0.86f, 0.55f, parent, routePlanes, result);
        PlaceGroundedKit(kit, "myb_forest_dead_branch_b", "Dead branches", "premium foreground dead branch", -1f, 118f, 4.55f, 0.82f, 0.55f, parent, routePlanes, result);
    }

    private static void BuildSceneLocalTrunkRhythm(
        IReadOnlyDictionary<string, Material> materials,
        Transform parent,
        Plane[] routePlanes,
        BuildResult result)
    {
        var nearMeters = new[] { 27f, 39f, 50f, 63f, 75f, 88f, 101f, 116f, 131f };
        for (var index = 0; index < nearMeters.Length; index++)
        {
            var meters = nearMeters[index];
            var side = index % 2 == 0 ? -1f : 1f;
            CreateSceneLocalTrunk(
                "premium vertical side trunk",
                side,
                meters + Jitter(meters, 1.1f),
                7.45f + Mathf.Abs(Jitter(meters + 2f, 0.85f)),
                2.22f + Mathf.Abs(Jitter(meters + 4f, 0.44f)),
                0.115f + Mathf.Abs(Jitter(meters + 6f, 0.024f)),
                materials[index % 3 == 0 ? "barkWarm" : "barkDark"],
                parent,
                routePlanes,
                result);

            CreateSceneLocalTrunk(
                "premium companion trunk",
                side,
                meters + 2.2f + Jitter(meters + 9f, 0.7f),
                8.85f + Mathf.Abs(Jitter(meters + 11f, 0.9f)),
                1.86f + Mathf.Abs(Jitter(meters + 13f, 0.36f)),
                0.082f + Mathf.Abs(Jitter(meters + 15f, 0.02f)),
                materials["barkDark"],
                parent,
                routePlanes,
                result);
        }

        for (var meters = 22f; meters <= 134f; meters += 16f)
        {
            for (var side = -1; side <= 1; side += 2)
            {
                if (IsBreathingWindow(meters) && side > 0)
                {
                    continue;
                }

                CreateSceneLocalTrunk(
                    "premium background trunk line",
                    side,
                    meters + Jitter(meters + side * 3f, 1.4f),
                    11.6f + Mathf.Abs(Jitter(meters + side * 5f, 1.0f)),
                    2.85f + Mathf.Abs(Jitter(meters + side * 7f, 0.58f)),
                    0.095f + Mathf.Abs(Jitter(meters + side * 9f, 0.026f)),
                    materials["shadowBark"],
                    parent,
                    routePlanes,
                    result);
            }
        }
    }

    private static void BuildForestFloorRelief(
        IReadOnlyDictionary<string, Material> materials,
        Transform parent,
        BuildResult result)
    {
        for (var meters = 14f; meters <= RouteLength - 10f; meters += 9f)
        {
            var side = Mathf.RoundToInt(meters / 9f) % 2 == 0 ? -1f : 1f;
            CreateRaisedPatch(
                parent,
                new PatchPlan(meters, side, 3.8f + Mathf.Abs(Jitter(meters, 0.36f)), 0.95f, 3.25f, "premium shoulder moss shelf", "mossDeep"),
                materials,
                result,
                18);

            CreateRaisedPatch(
                parent,
                new PatchPlan(meters + 3.6f, -side, 4.8f + Mathf.Abs(Jitter(meters + 7f, 0.55f)), 1.15f, 3.7f, "premium leaf soil mound", Variant(meters, new[] { "leafWarm", "soilDark", "mossDeep" })),
                materials,
                result,
                17);
        }
    }

    private static void BuildDepthPockets(
        IReadOnlyDictionary<string, Material> materials,
        Transform parent,
        BuildResult result)
    {
        for (var meters = 18f; meters <= RouteLength - 12f; meters += 14f)
        {
            for (var side = -1; side <= 1; side += 2)
            {
                if (IsBreathingWindow(meters) && side < 0)
                {
                    continue;
                }

                CreateRaisedPatch(
                    parent,
                    new PatchPlan(meters + Jitter(meters + side * 2f, 1.0f), side, 9.4f + Mathf.Abs(Jitter(meters + side, 1.2f)), 1.35f, 5.8f, "premium background depth pocket", "shadowGreen"),
                    materials,
                    result,
                    18);
            }
        }
    }

    private static void PlaceGroundedKit(
        IReadOnlyDictionary<string, GameObject> kit,
        string assetId,
        string family,
        string band,
        float side,
        float meters,
        float distanceFromRoute,
        float scale,
        float routeRadius,
        Transform parent,
        Plane[] routePlanes,
        BuildResult result)
    {
        if (!kit.TryGetValue(assetId, out var asset))
        {
            return;
        }

        var sample = SampleAt(meters);
        var signedOffset = side * distanceFromRoute;
        var groundHeight = sample.Position.y + TerrainHeight(meters, signedOffset);
        var position = sample.Position + sample.Right * signedOffset + Vector3.up * (TerrainHeight(meters, signedOffset) + 0.35f);
        var yaw = Mathf.Atan2(sample.Forward.x, sample.Forward.z) * Mathf.Rad2Deg + 90f * side + Jitter(meters + signedOffset, 24f);
        var instance = PrefabUtility.InstantiatePrefab(asset) as GameObject;
        if (instance == null)
        {
            instance = UnityEngine.Object.Instantiate(asset);
        }

        instance.name = "MYB158_" + assetId + "_" + Slug(band) + "_" + result.Placements.Count.ToString("00", CultureInfo.InvariantCulture);
        instance.transform.SetParent(parent, false);
        instance.transform.position = position;
        instance.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        instance.transform.localScale = Vector3.one * Mathf.Max(0.1f, scale);

        var bounds = CombinedRendererBounds(instance);
        if (bounds.HasValue)
        {
            var correction = groundHeight - SinkMeters - bounds.Value.min.y;
            instance.transform.position += Vector3.up * correction;
            bounds = CombinedRendererBounds(instance);
        }

        var finalBounds = bounds ?? new Bounds(instance.transform.position, Vector3.zero);
        var bottomClearance = finalBounds.min.y - groundHeight;
        var routeVisible = routePlanes != null && GeometryUtility.TestPlanesAABB(routePlanes, finalBounds);
        result.GroundingRecords.Add(new GroundingRecord
        {
            Name = instance.name,
            Family = family,
            RouteVisible = routeVisible,
            BottomClearance = bottomClearance
        });

        result.Placements.Add(new PlacementRecord
        {
            Name = instance.name,
            Family = family,
            Band = band,
            Meters = meters,
            Offset = signedOffset,
            Radius = routeRadius,
            TriangleCount = CountTriangles(instance),
            RouteVisible = routeVisible
        });
    }

    private static void CreateRaisedPatch(
        Transform parent,
        PatchPlan plan,
        IReadOnlyDictionary<string, Material> materials,
        BuildResult result,
        int outerVertices)
    {
        var signedOffset = plan.Side * plan.DistanceFromRoute;
        var sample = SampleAt(plan.Meters);
        var center = sample.Position + sample.Right * signedOffset + Vector3.up * (TerrainHeight(plan.Meters, signedOffset) + 0.035f);
        var rotation = Quaternion.LookRotation(sample.Forward, Vector3.up) * Quaternion.Euler(0f, Jitter(plan.Meters + signedOffset, 26f), 0f);
        var mesh = CreateRaisedPatchMesh(plan, outerVertices);
        var patch = new GameObject("MYB158_" + Slug(plan.Category) + "_" + result.Placements.Count.ToString("00", CultureInfo.InvariantCulture));
        patch.transform.SetParent(parent, false);
        patch.transform.position = center;
        patch.transform.rotation = rotation;
        patch.AddComponent<MeshFilter>().sharedMesh = mesh;
        patch.AddComponent<MeshRenderer>().sharedMaterial = materials.TryGetValue(plan.MaterialKey, out var material) ? material : materials["mossDeep"];

        result.Placements.Add(new PlacementRecord
        {
            Name = patch.name,
            Family = plan.Category,
            Band = plan.Category,
            Meters = plan.Meters,
            Offset = signedOffset,
            Radius = plan.Radius,
            TriangleCount = mesh.triangles.Length / 3,
            RouteVisible = false
        });
    }

    private static void CreateSceneLocalTrunk(
        string band,
        float side,
        float meters,
        float distanceFromRoute,
        float height,
        float radius,
        Material material,
        Transform parent,
        Plane[] routePlanes,
        BuildResult result)
    {
        var sample = SampleAt(meters);
        var signedOffset = side * distanceFromRoute;
        var groundHeight = sample.Position.y + TerrainHeight(meters, signedOffset);
        var mesh = CreateTaperedTrunkMesh(height, radius, meters + signedOffset);
        var trunk = new GameObject("MYB158_" + Slug(band) + "_" + result.Placements.Count.ToString("00", CultureInfo.InvariantCulture));
        trunk.transform.SetParent(parent, false);
        trunk.transform.position = sample.Position + sample.Right * signedOffset + Vector3.up * (TerrainHeight(meters, signedOffset) + 0.2f);
        trunk.transform.rotation = Quaternion.LookRotation(sample.Forward, Vector3.up)
            * Quaternion.Euler(Jitter(meters + 1f, 2.2f), Jitter(meters + signedOffset, 38f), side * (2.8f + Mathf.Abs(Jitter(meters + 3f, 2.4f))));
        trunk.AddComponent<MeshFilter>().sharedMesh = mesh;
        trunk.AddComponent<MeshRenderer>().sharedMaterial = material;

        var bounds = CombinedRendererBounds(trunk);
        if (bounds.HasValue)
        {
            var correction = groundHeight - SinkMeters - bounds.Value.min.y;
            trunk.transform.position += Vector3.up * correction;
            bounds = CombinedRendererBounds(trunk);
        }

        var finalBounds = bounds ?? new Bounds(trunk.transform.position, Vector3.zero);
        var bottomClearance = finalBounds.min.y - groundHeight;
        var routeVisible = routePlanes != null && GeometryUtility.TestPlanesAABB(routePlanes, finalBounds);
        result.GroundingRecords.Add(new GroundingRecord
        {
            Name = trunk.name,
            Family = "Scene-local trunks",
            RouteVisible = routeVisible,
            BottomClearance = bottomClearance
        });

        result.Placements.Add(new PlacementRecord
        {
            Name = trunk.name,
            Family = "Scene-local trunks",
            Band = band,
            Meters = meters,
            Offset = signedOffset,
            Radius = radius * 2.1f,
            TriangleCount = mesh.triangles.Length / 3,
            RouteVisible = routeVisible
        });
    }

    private static Mesh CreateRaisedPatchMesh(PatchPlan plan, int outerVertices)
    {
        var vertices = new List<Vector3> { new Vector3(0f, 0.035f, 0f) };
        var uvs = new List<Vector2> { new Vector2(0.5f, 0.5f) };
        var triangles = new List<int>();
        var halfLength = plan.Length * 0.5f;

        for (var i = 0; i < outerVertices; i++)
        {
            var angle = i / (float)outerVertices * Mathf.PI * 2f;
            var wobble = 1f + Jitter(plan.Meters + i * 2.31f + plan.DistanceFromRoute, 0.25f);
            var x = Mathf.Cos(angle) * plan.Radius * wobble;
            var z = Mathf.Sin(angle) * halfLength * (1f + Jitter(plan.Meters + i * 3.9f, 0.20f));
            var y = Mathf.Sin(angle * 2.2f + plan.Meters) * 0.018f;
            vertices.Add(new Vector3(x, y, z));
            uvs.Add(new Vector2(0.5f + x / Mathf.Max(0.01f, plan.Radius * 2.5f), 0.5f + z / Mathf.Max(0.01f, plan.Length * 1.35f)));
        }

        for (var i = 1; i <= outerVertices; i++)
        {
            triangles.Add(0);
            triangles.Add(i == outerVertices ? 1 : i + 1);
            triangles.Add(i);
        }

        var mesh = new Mesh { name = "MYB158_" + Slug(plan.Category) + "_RaisedSceneLocalMesh" };
        mesh.SetVertices(vertices);
        mesh.SetUVs(0, uvs);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }

    private static Mesh CreateTaperedTrunkMesh(float height, float radius, float seed)
    {
        const int segments = 9;
        var ringYs = new[] { 0f, 0.08f, 0.48f, 0.78f, 1f };
        var ringScales = new[] { 1.65f, 1.2f, 0.94f, 0.76f, 0.58f };
        var vertices = new List<Vector3>();
        var uvs = new List<Vector2>();
        var triangles = new List<int>();

        for (var ring = 0; ring < ringYs.Length; ring++)
        {
            var y01 = ringYs[ring];
            var bend = new Vector2(
                Mathf.Sin(seed * 0.33f + y01 * 1.9f) * radius * 0.28f * y01,
                Mathf.Cos(seed * 0.21f + y01 * 1.6f) * radius * 0.22f * y01);

            for (var segment = 0; segment < segments; segment++)
            {
                var angle = segment / (float)segments * Mathf.PI * 2f;
                var wobble = 1f + Jitter(seed + ring * 7.1f + segment * 2.4f, 0.12f);
                var ringRadius = radius * ringScales[ring] * wobble;
                vertices.Add(new Vector3(
                    Mathf.Cos(angle) * ringRadius + bend.x,
                    y01 * height,
                    Mathf.Sin(angle) * ringRadius + bend.y));
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
                triangles.Add(a);
                triangles.Add(c);
                triangles.Add(b);
                triangles.Add(b);
                triangles.Add(c);
                triangles.Add(d);
            }
        }

        var baseCenter = vertices.Count;
        vertices.Add(Vector3.zero);
        uvs.Add(new Vector2(0.5f, 0f));
        var topCenter = vertices.Count;
        vertices.Add(new Vector3(0f, height, 0f));
        uvs.Add(new Vector2(0.5f, 1f));
        var topStart = (ringYs.Length - 1) * segments;

        for (var segment = 0; segment < segments; segment++)
        {
            triangles.Add(baseCenter);
            triangles.Add((segment + 1) % segments);
            triangles.Add(segment);

            triangles.Add(topCenter);
            triangles.Add(topStart + segment);
            triangles.Add(topStart + (segment + 1) % segments);
        }

        for (var root = 0; root < 4; root++)
        {
            var angle = (root / 4f * Mathf.PI * 2f) + Jitter(seed + root * 5.7f, 0.35f);
            var tangent = new Vector3(-Mathf.Sin(angle), 0f, Mathf.Cos(angle));
            var direction = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
            var rootLength = radius * (2.15f + Mathf.Abs(Jitter(seed + root * 3.2f, 0.55f)));
            var rootWidth = radius * 0.36f;
            var a = vertices.Count;
            vertices.Add(direction * radius * 0.72f + tangent * rootWidth);
            vertices.Add(direction * radius * 0.72f - tangent * rootWidth);
            vertices.Add(direction * rootLength + Vector3.up * 0.018f);
            uvs.Add(new Vector2(0f, 0f));
            uvs.Add(new Vector2(1f, 0f));
            uvs.Add(new Vector2(0.5f, 1f));
            triangles.Add(a);
            triangles.Add(a + 2);
            triangles.Add(a + 1);
        }

        var mesh = new Mesh { name = "MYB158_SceneLocal_TaperedTrunkMesh" };
        mesh.SetVertices(vertices);
        mesh.SetUVs(0, uvs);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }

    private static Dictionary<string, GameObject> LoadKit(BuildResult result)
    {
        var ids = new[]
        {
            "myb_forest_trunk_ancient_a",
            "myb_forest_trunk_broken_a",
            "myb_forest_trunk_leaning_a",
            "myb_forest_trunk_knotted_a",
            "myb_forest_root_cluster_lateral_a",
            "myb_forest_root_arch_a",
            "myb_forest_rock_mossy_a",
            "myb_forest_rock_mossy_b",
            "myb_forest_dead_branch_a",
            "myb_forest_dead_branch_b",
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

    private static Dictionary<string, Material> CreateRuntimeMaterials()
    {
        var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        return new Dictionary<string, Material>
        {
            ["mossDeep"] = RuntimeMaterial(shader, "MYB158_SceneLocal_DeepMoss", new Color(0.105f, 0.24f, 0.105f)),
            ["leafWarm"] = RuntimeMaterial(shader, "MYB158_SceneLocal_WarmLeafLitter", new Color(0.31f, 0.205f, 0.105f)),
            ["soilDark"] = RuntimeMaterial(shader, "MYB158_SceneLocal_RootedDarkSoil", new Color(0.12f, 0.095f, 0.065f)),
            ["shadowGreen"] = RuntimeMaterial(shader, "MYB158_SceneLocal_DepthShadowGreen", new Color(0.065f, 0.12f, 0.075f)),
            ["barkDark"] = RuntimeMaterial(shader, "MYB158_SceneLocal_DarkBark", new Color(0.19f, 0.15f, 0.105f)),
            ["barkWarm"] = RuntimeMaterial(shader, "MYB158_SceneLocal_WarmBark", new Color(0.32f, 0.22f, 0.125f)),
            ["shadowBark"] = RuntimeMaterial(shader, "MYB158_SceneLocal_ShadowBark", new Color(0.105f, 0.12f, 0.085f))
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

    private static void ConfigureRouteMood()
    {
        RenderSettings.skybox = null;
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.42f, 0.48f, 0.40f);
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogColor = new Color(0.31f, 0.37f, 0.34f);
        RenderSettings.fogDensity = 0.0105f;

        var existing = FindSceneObjectByName("MYB158_WarmRouteBreakLight");
        if (existing != null)
        {
            UnityEngine.Object.DestroyImmediate(existing);
        }

        var warm = new GameObject("MYB158_WarmRouteBreakLight");
        var light = warm.AddComponent<Light>();
        light.type = LightType.Directional;
        light.color = new Color(1f, 0.84f, 0.62f);
        light.intensity = 0.28f;
        warm.transform.rotation = Quaternion.Euler(30f, -42f, 0f);
    }

    private static MYB145CaptureRigHelper.CaptureResult CaptureScene(string scenePath, string state)
    {
        EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        return MYB145CaptureRigHelper.CaptureRouteAndOverview(
            "MYB-158-" + state,
            new MYB145CaptureRigHelper.CaptureOptions
            {
                TicketId = "MYB-158",
                State = state
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
            if (record.Type == "route" && record.State == "before")
            {
                result.BeforeRoutePath = record.Path;
            }
            else if (record.Type == "overview" && record.State == "before")
            {
                result.BeforeOverviewPath = record.Path;
            }
            else if (record.Type == "route" && record.State == "after")
            {
                result.AfterRoutePath = record.Path;
            }
            else if (record.Type == "overview" && record.State == "after")
            {
                result.AfterOverviewPath = record.Path;
            }
        }

        foreach (var error in capture.Errors)
        {
            result.Errors.Add("Capture " + capture.State + " error " + error.Code + ": " + error.Message);
        }
        foreach (var warning in capture.Warnings)
        {
            result.Warnings.Add("Capture " + capture.State + " warning " + warning.Code + ": " + warning.Message);
        }
    }

    private static void CreateComparisonSheets(
        BuildResult result,
        MYB145CaptureRigHelper.CaptureResult before,
        MYB145CaptureRigHelper.CaptureResult after)
    {
        CreateComparisonSheet(result, before, after, "route");
        CreateComparisonSheet(result, before, after, "overview");
        WriteBeforeAfterCaptureReport(result);
    }

    private static void CreateComparisonSheet(
        BuildResult result,
        MYB145CaptureRigHelper.CaptureResult before,
        MYB145CaptureRigHelper.CaptureResult after,
        string type)
    {
        var beforeRecord = before?.Captures.FirstOrDefault(capture => capture.Type == type);
        var afterRecord = after?.Captures.FirstOrDefault(capture => capture.Type == type);
        if (beforeRecord == null || afterRecord == null)
        {
            result.Warnings.Add("Missing " + type + " before/after capture for comparison sheet.");
            return;
        }

        var beforePath = ToRepoPath(beforeRecord.Path);
        var afterPath = ToRepoPath(afterRecord.Path);
        if (!File.Exists(beforePath) || !File.Exists(afterPath))
        {
            result.Warnings.Add("Missing " + type + " PNG file for comparison sheet.");
            return;
        }

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
        builder.AppendLine("# MYB-158 Capture Report");
        builder.AppendLine();
        builder.AppendLine("Ticket:");
        builder.AppendLine("- MYB-158");
        builder.AppendLine();
        builder.AppendLine("Mode:");
        builder.AppendLine("- explicit before/after comparison");
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
        builder.AppendLine("- " + result.Verdict);
        File.WriteAllText(path, builder.ToString());
        result.BeforeAfterCaptureReportPath = relativePath;
    }

    private static BuildResult CreateResult()
    {
        return new BuildResult
        {
            GeneratedAt = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture),
            Branch = GetGitValue("rev-parse --abbrev-ref HEAD"),
            Commit = GetGitValue("rev-parse --short HEAD")
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

    private static bool IsBreathingWindow(float meters)
    {
        return Mathf.Abs(meters - 36f) < 6.5f || Mathf.Abs(meters - 78f) < 7.5f || Mathf.Abs(meters - 120f) < 6.5f;
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

    private static string RockVariant(float seed)
    {
        return Mathf.Abs(Mathf.RoundToInt(seed * 3.7f)) % 2 == 0 ? "myb_forest_rock_mossy_a" : "myb_forest_rock_mossy_b";
    }

    private static string Variant(float seed, IReadOnlyList<string> variants)
    {
        var index = Mathf.Abs(Mathf.RoundToInt(seed * 13.37f)) % variants.Count;
        return variants[index];
    }

    private static float Jitter(float seed, float amount)
    {
        return (Mathf.PerlinNoise(seed * 0.17f, seed * 0.071f) - 0.5f) * 2f * amount;
    }

    private static Bounds? CombinedRendererBounds(GameObject root)
    {
        var renderers = root.GetComponentsInChildren<Renderer>(true)
            .Where(renderer => renderer.enabled)
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
            if (found != null)
            {
                return found.gameObject;
            }
        }

        return null;
    }

    private static Transform FindInHierarchy(Transform root, string name)
    {
        if (root.name == name)
        {
            return root;
        }

        for (var i = 0; i < root.childCount; i++)
        {
            var found = FindInHierarchy(root.GetChild(i), name);
            if (found != null)
            {
                return found;
            }
        }

        return null;
    }

    private static string Slug(string value)
    {
        return new string((value ?? string.Empty).ToLowerInvariant().Select(character => char.IsLetterOrDigit(character) ? character : '_').ToArray()).Trim('_');
    }

    private static string FormatFloat(float value)
    {
        return value.ToString("0.###", CultureInfo.InvariantCulture);
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
                if (process == null)
                {
                    return "unknown";
                }

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

    private static void WriteReports(BuildResult result)
    {
        WriteImplementationReport(result);
        WriteMetricsJson(result);
    }

    private static void WriteImplementationReport(BuildResult result)
    {
        var reportPath = ToRepoPath(ReportRelativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(reportPath) ?? GetRepoRoot());
        var builder = new StringBuilder();
        builder.AppendLine("# MYB-158 Premium Route-Camera Forest Corridor Pass");
        builder.AppendLine();
        builder.AppendLine("Status:");
        builder.AppendLine("- In Progress / generated evidence for review.");
        builder.AppendLine("- Premium target reached: No, pending Julien review.");
        builder.AppendLine("- Verdict: " + result.VisualVerdict + ".");
        builder.AppendLine();
        builder.AppendLine("Generated at:");
        builder.AppendLine("- " + result.GeneratedAt);
        builder.AppendLine();
        builder.AppendLine("Branch / commit:");
        builder.AppendLine("- `" + result.Branch + "` / `" + result.Commit + "`");
        builder.AppendLine();
        builder.AppendLine("## Scope");
        builder.AppendLine();
        builder.AppendLine("- Dedicated MYB-158 preview scene derived from MYB-149.");
        builder.AppendLine("- Route-camera composition pass using existing MYB-147 kit accents and scene-local meshes.");
        builder.AppendLine("- No gameplay, ride loop, HUD, mock telemetry, external asset generation, Meshy, Tripo, Poly Haven, or Blender call.");
        builder.AppendLine();
        builder.AppendLine("## Scene");
        builder.AppendLine();
        builder.AppendLine("- Source: `" + SourceScenePath + "`");
        builder.AppendLine("- Output: `" + OutputScenePath + "`");
        builder.AppendLine("- Generated root: `" + GeneratedRootName + "`");
        builder.AppendLine();
        builder.AppendLine("## What Changed");
        builder.AppendLine();
        builder.AppendLine("- Added grounded scene-local vertical trunk rhythm to strengthen side-corridor massing without oversized foreground kit silhouettes.");
        builder.AppendLine("- Kept existing MYB-147 kit pieces as grounded roots, rocks, branch, fallen-log, and distant accent support.");
        builder.AppendLine("- Added raised scene-local forest-floor shelves and depth pockets instead of only flat decal-like patches.");
        builder.AppendLine("- Tuned ambient/fog mood inside the dedicated MYB-158 scene.");
        builder.AppendLine("- Added no new canopies, so MYB-156 visual-support risk is not expanded.");
        builder.AppendLine();
        builder.AppendLine("## Metrics");
        builder.AppendLine();
        builder.AppendLine("- Placements: " + result.Placements.Count);
        builder.AppendLine("- Grounded visual placements: " + result.GroundingRecords.Count);
        builder.AppendLine("- Scene-local material count: " + result.SceneLocalMaterialCount);
        builder.AppendLine("- Renderers: " + result.RendererCount);
        builder.AppendLine("- Mesh filters: " + result.MeshFilterCount);
        builder.AppendLine("- Approximate triangles: " + result.TotalTriangles);
        builder.AppendLine("- Minimum route clearance: " + FormatFloat(result.MinimumRouteClearance) + "m");
        builder.AppendLine("- Route overlap count: " + result.RouteOverlapCount);
        builder.AppendLine();
        builder.AppendLine("## Ground Placement Metrics");
        builder.AppendLine();
        builder.AppendLine("- floatingAssetCount: " + result.FloatingAssetCount);
        builder.AppendLine("- maxFloatingClearance: " + FormatFloat(result.MaxFloatingClearance) + "m");
        builder.AppendLine("- sinkingAssetCount: " + result.SinkingAssetCount);
        builder.AppendLine("- maxSinkingDepth: " + FormatFloat(result.MaxSinkingDepth) + "m");
        builder.AppendLine("- routeVisibleFloatingAssetCount: " + result.RouteVisibleFloatingAssetCount);
        builder.AppendLine("- groundPlacementMethod: combined renderer bounds min.y after rotation/scale for kit assets and scene-local meshes");
        builder.AppendLine("- groundSource: deterministic MYB-148/MYB-149 terrain height functions");
        builder.AppendLine("- sinkMeters: " + FormatFloat(SinkMeters));
        builder.AppendLine();
        builder.AppendLine("## Visual Support Validation");
        builder.AppendLine();
        builder.AppendLine("- MYB-156 verdict: " + (string.IsNullOrWhiteSpace(result.VisualSupportVerdict) ? "Not run yet" : result.VisualSupportVerdict));
        builder.AppendLine("- routeVisibleUnsupportedCanopyCount: " + result.RouteVisibleUnsupportedCanopyCount);
        builder.AppendLine("- unsupportedCanopyCount: " + result.UnsupportedCanopyCount);
        builder.AppendLine("- report: `" + result.VisualSupportReportRelativePath + "`");
        builder.AppendLine();
        builder.AppendLine("## Visual Evidence");
        builder.AppendLine();
        builder.AppendLine("- Before route: `" + result.BeforeRoutePath + "`");
        builder.AppendLine("- Before overview: `" + result.BeforeOverviewPath + "`");
        builder.AppendLine("- After route: `" + result.AfterRoutePath + "`");
        builder.AppendLine("- After overview: `" + result.AfterOverviewPath + "`");
        builder.AppendLine("- Route comparison: `" + result.RouteComparisonPath + "`");
        builder.AppendLine("- Overview comparison: `" + result.OverviewComparisonPath + "`");
        builder.AppendLine("- Before/after capture report: `" + result.BeforeAfterCaptureReportPath + "`");
        builder.AppendLine();
        builder.AppendLine("## Rubric Score");
        builder.AppendLine();
        builder.AppendLine("| # | Criterion | Type | Score | Notes |");
        builder.AppendLine("|---|---|---|---:|---|");
        builder.AppendLine("| 1 | Route readability | Blocking | 4 | Road remains readable and unobstructed. |");
        builder.AppendLine("| 2 | Silhouette quality | Blocking | 3 | Stronger vertical trunk rhythm, still not final premium tree/canopy quality. |");
        builder.AppendLine("| 3 | Lighting mood | Blocking | 3 | Fog/ambient improved, still modest. |");
        builder.AppendLine("| 4 | Material coherence | Blocking | 3 | More grounded, but scene-local materials remain simple. |");
        builder.AppendLine("| 5 | Foreground richness | Contributive | 4 | Near-route floor and roots are visibly richer. |");
        builder.AppendLine("| 6 | Midground density | Contributive | 3 | Better side massing, still not a full premium forest wall. |");
        builder.AppendLine("| 7 | Background depth | Contributive | 3 | Depth pockets improve layering, but background remains restrained. |");
        builder.AppendLine("| 8 | Scale credibility | Contributive | 4 | Added visual assets are grounded by visual bottom and keep support policy intact. |");
        builder.AppendLine("| 9 | Composition rhythm | Contributive | 3 | More authored beats, still needs art-direction review. |");
        builder.AppendLine();
        builder.AppendLine("Average: 3.33");
        builder.AppendLine();
        builder.AppendLine("Blocking criteria all >= 4:");
        builder.AppendLine("- No");
        builder.AppendLine();
        builder.AppendLine("Premium target reached:");
        builder.AppendLine("- No");
        builder.AppendLine();
        builder.AppendLine("Verdict:");
        builder.AppendLine("- " + result.VisualVerdict);
        builder.AppendLine();
        builder.AppendLine("## Follow-Up");
        builder.AppendLine();
        builder.AppendLine("- Julien route-camera review is required before closure.");
        builder.AppendLine("- If still insufficient, next blocker is higher-quality authored tree/canopy forms rather than placement governance.");
        File.WriteAllText(reportPath, builder.ToString());
    }

    private static void WriteMetricsJson(BuildResult result)
    {
        var metricsPath = ToRepoPath(MetricsRelativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(metricsPath) ?? GetRepoRoot());
        var builder = new StringBuilder();
        builder.AppendLine("{");
        builder.AppendLine("  \"schemaVersion\": 1,");
        builder.AppendLine("  \"ticket\": \"MYB-158\",");
        builder.AppendLine("  \"generatedAt\": \"" + EscapeJson(result.GeneratedAt) + "\",");
        builder.AppendLine("  \"sourceScene\": \"" + SourceScenePath + "\",");
        builder.AppendLine("  \"outputScene\": \"" + OutputScenePath + "\",");
        builder.AppendLine("  \"generatedRoot\": \"" + GeneratedRootName + "\",");
        builder.AppendLine("  \"placementCount\": " + result.Placements.Count + ",");
        builder.AppendLine("  \"groundedVisualPlacementCount\": " + result.GroundingRecords.Count + ",");
        builder.AppendLine("  \"sceneLocalMaterialCount\": " + result.SceneLocalMaterialCount + ",");
        builder.AppendLine("  \"rendererCount\": " + result.RendererCount + ",");
        builder.AppendLine("  \"meshFilterCount\": " + result.MeshFilterCount + ",");
        builder.AppendLine("  \"approximateTriangles\": " + result.TotalTriangles + ",");
        builder.AppendLine("  \"minimumRouteClearanceMeters\": " + result.MinimumRouteClearance.ToString("0.###", CultureInfo.InvariantCulture) + ",");
        builder.AppendLine("  \"routeOverlapCount\": " + result.RouteOverlapCount + ",");
        builder.AppendLine("  \"floatingAssetCount\": " + result.FloatingAssetCount + ",");
        builder.AppendLine("  \"maxFloatingClearance\": " + result.MaxFloatingClearance.ToString("0.###", CultureInfo.InvariantCulture) + ",");
        builder.AppendLine("  \"sinkingAssetCount\": " + result.SinkingAssetCount + ",");
        builder.AppendLine("  \"maxSinkingDepth\": " + result.MaxSinkingDepth.ToString("0.###", CultureInfo.InvariantCulture) + ",");
        builder.AppendLine("  \"routeVisibleFloatingAssetCount\": " + result.RouteVisibleFloatingAssetCount + ",");
        builder.AppendLine("  \"groundPlacementMethod\": \"combinedRendererBounds.min.y after rotation/scale for kit assets and scene-local meshes\",");
        builder.AppendLine("  \"groundSource\": \"MYB-148/MYB-149 deterministic terrain height functions\",");
        builder.AppendLine("  \"sinkMeters\": " + SinkMeters.ToString("0.###", CultureInfo.InvariantCulture) + ",");
        builder.AppendLine("  \"visualSupportVerdict\": \"" + EscapeJson(result.VisualSupportVerdict) + "\",");
        builder.AppendLine("  \"routeVisibleUnsupportedCanopyCount\": " + result.RouteVisibleUnsupportedCanopyCount + ",");
        builder.AppendLine("  \"unsupportedCanopyCount\": " + result.UnsupportedCanopyCount + ",");
        builder.AppendLine("  \"rubricAverage\": 3.33,");
        builder.AppendLine("  \"premiumTargetReached\": false,");
        builder.AppendLine("  \"visualVerdict\": \"" + EscapeJson(result.VisualVerdict) + "\"");
        builder.AppendLine("}");
        File.WriteAllText(metricsPath, builder.ToString());
    }

    private static string EscapeJson(string value)
    {
        return (value ?? string.Empty).Replace("\\", "\\\\").Replace("\"", "\\\"");
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

    private sealed class MassBeat
    {
        public readonly float Meters;
        public readonly float Side;
        public readonly float Distance;

        public MassBeat(float meters, float side, float distance)
        {
            Meters = meters;
            Side = side;
            Distance = distance;
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

    private sealed class PlacementRecord
    {
        public string Name;
        public string Family;
        public string Band;
        public float Meters;
        public float Offset;
        public float Radius;
        public int TriangleCount;
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
        public int SceneLocalMaterialCount;
        public int RendererCount;
        public int MeshFilterCount;
        public int TotalTriangles;
        public float MinimumRouteClearance;
        public int RouteOverlapCount;
        public int FloatingAssetCount;
        public float MaxFloatingClearance;
        public int SinkingAssetCount;
        public float MaxSinkingDepth;
        public int RouteVisibleFloatingAssetCount;
        public string VisualSupportVerdict = "Not run";
        public int RouteVisibleUnsupportedCanopyCount;
        public int UnsupportedCanopyCount;
        public string VisualSupportReportRelativePath = string.Empty;
        public string BeforeRoutePath = string.Empty;
        public string BeforeOverviewPath = string.Empty;
        public string AfterRoutePath = string.Empty;
        public string AfterOverviewPath = string.Empty;
        public string RouteComparisonPath = string.Empty;
        public string OverviewComparisonPath = string.Empty;
        public string BeforeAfterCaptureReportPath = string.Empty;
        public readonly List<string> CaptureReports = new List<string>();
        public readonly List<string> CaptureMetadata = new List<string>();
        public readonly List<string> Errors = new List<string>();
        public readonly List<string> Warnings = new List<string>();
        public readonly List<string> Info = new List<string>();
        public readonly List<PlacementRecord> Placements = new List<PlacementRecord>();
        public readonly List<GroundingRecord> GroundingRecords = new List<GroundingRecord>();
        public string VisualVerdict => Errors.Count > 0 ? "Rework required" : "Checkpoint insuffisant, improved";
        public string Verdict => Errors.Count > 0 ? "FAIL" : Warnings.Count > 0 ? "PASS_WITH_WARNINGS" : "PASS";

        public string ToConsoleSummary()
        {
            return "MYB-158 premium route-camera pass: " + Verdict
                + " | placements=" + Placements.Count
                + " | routeOverlapCount=" + RouteOverlapCount
                + " | routeVisibleFloatingAssetCount=" + RouteVisibleFloatingAssetCount
                + " | visualSupport=" + VisualSupportVerdict
                + " | report=" + ReportRelativePath;
        }
    }
}

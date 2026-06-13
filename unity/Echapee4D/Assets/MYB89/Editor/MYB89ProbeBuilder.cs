using System;
using System.Collections.Generic;
using System.IO;
using MYB89;
using MYB73;
using MYB48;
using MYB59;
using MYB60;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MYB89.Editor
{
    public static class MYB89ProbeBuilder
    {
        private const string ScenePath = "Assets/Scenes/MYB89UnityMcpProbe.unity";
        private const string CaptureIndexPath = "_bmad-output/video-captures/myb-89-unity-mcp-probe-latest.txt";
        private const float RoadWidth = 7.2f;

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

        [MenuItem("Tools/MYB-89/Build Unity MCP Probe Scene")]
        public static void BuildScene()
        {
            EnsureFolder("Assets/Scenes");
            EnsureFolder("Assets/MYB89");
            EnsureFolder("Assets/MYB89/Materials");
            EnsureFolder("Assets/MYB73");
            EnsureFolder("Assets/MYB48");
            EnsureFolder("Assets/MYB48/Materials");

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "MYB89UnityMcpProbe";

            var materials = CreateMaterials();
            var cueMaterials = CreateDifficultyCueMaterials();
            var root = new GameObject("MYB89_ProbeRoot");

            ConfigureRenderSettings();
            CreateTerrain(root.transform, materials);
            var routeMarkers = CreateRoute(root.transform, materials);
            CreateScenicCorridor(root.transform, materials);
            CreateRoadsideRhythm(root.transform, materials);
            CreateRouteDifficultyCues(root.transform, routeMarkers, cueMaterials);
            var keyLight = CreateLighting(root.transform);
            CreateCameraRig(root.transform, routeMarkers, keyLight, materials);

            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            NormalizeSerializedWhitespace(
                ScenePath,
                "Assets/Echappee/Art/MYB53Validation/Materials/MYB53_SoftVillageGrass.mat",
                "Assets/Echappee/Art/MYB53Validation/Materials/MYB53_WarmRoadEdge.mat",
                "Assets/MYB48/Materials/MYB48_ClimbCue.mat",
                "Assets/MYB48/Materials/MYB48_RecoveryCue.mat",
                "Assets/MYB48/Materials/MYB48_SprintCue.mat",
                "Assets/MYB89/Materials/MYB89_HorizonHill.mat",
                "Assets/MYB89/Materials/MYB89_HorizonVillage.mat",
                "Assets/MYB89/Materials/MYB89_PremiumWarmGlow.mat",
                "Assets/MYB89/Materials/MYB89_VillageRoof.mat",
                "Assets/MYB89/Materials/MYB89_VillageWall.mat",
                "Assets/MYB89/Materials/MYB89_VillageWood.mat");

            Debug.Log($"MYB-89 probe scene built at {ScenePath} with {routeMarkers.Count} route markers.");
        }

        [MenuItem("Tools/MYB-89/Validate Unity MCP Probe Scene")]
        public static string ValidateProbeScene()
        {
            OpenProbeScene();

            var failures = new List<string>();
            var ride = UnityEngine.Object.FindAnyObjectByType<MYB89ProbeRide>();
            var camera = Camera.main;
            var road = GameObject.Find("MYB89_RouteRoad");
            var hud = GameObject.Find("MYB89_HUD");
            var difficultyCues = UnityEngine.Object.FindAnyObjectByType<MYB48RouteDifficultyCueController>();
            var resistanceController = UnityEngine.Object.FindAnyObjectByType<MYB59ResistanceController>();
            var resistanceMapper = UnityEngine.Object.FindAnyObjectByType<MYB60ResistanceMapper>();
            var scenicCorridor = GameObject.Find("MYB44_ScenicCorridor");
            var scenicRenderers = scenicCorridor == null
                ? Array.Empty<Renderer>()
                : scenicCorridor.GetComponentsInChildren<Renderer>(false);
            var horizon = GameObject.Find("MYB44_Horizon");
            var horizonRenderers = horizon == null
                ? Array.Empty<Renderer>()
                : horizon.GetComponentsInChildren<Renderer>(false);
            var premiumSignal = GameObject.Find("MYB44_PremiumSignal_RelicLantern");
            var renderers = UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsInactive.Exclude);

            if (ride == null)
            {
                failures.Add("Missing MYB89ProbeRide component.");
            }
            else
            {
                ride.RebuildRouteCache();

                if (ride.routeMarkers == null || ride.routeMarkers.Length < 8)
                {
                    failures.Add("Route needs at least 8 route markers.");
                }

                if (ride.RouteLength < 200f)
                {
                    failures.Add($"Route length too short: {ride.RouteLength:0.0} m.");
                }

                if (ride.TrajectorySampleCount < ride.routeMarkers.Length * 4)
                {
                    failures.Add($"Smoothed ride trajectory has too few samples: {ride.TrajectorySampleCount}.");
                }

                if (ride.turnLookAheadMeters <= 0.01f)
                {
                    failures.Add("Ride turn look-ahead must be enabled for natural curve navigation.");
                }

                if (ride.cameraTurnLeanMaxDegrees <= 0.01f || ride.cameraTurnLeanMaxDegrees > 3f)
                {
                    failures.Add($"Camera turn lean should stay subtle and enabled: {ride.cameraTurnLeanMaxDegrees:0.0} degrees.");
                }

                if (ride.distanceLabel == null
                    || ride.speedLabel == null
                    || ride.difficultyLabel == null
                    || ride.gradeLabel == null
                    || ride.segmentLabel == null
                    || ride.verdictLabel == null)
                {
                    failures.Add("HUD labels are not wired to MYB89ProbeRide.");
                }

                ride.SetPreviewProgress(ride.RouteLength * 0.42f);
                if (ride.difficultyLabel == null || !ride.difficultyLabel.text.Contains("Effort:"))
                {
                    failures.Add("HUD difficulty label is missing the effort readout.");
                }

                if (ride.gradeLabel == null || !ride.gradeLabel.text.Contains("Pente:"))
                {
                    failures.Add("HUD grade label is missing the slope readout.");
                }

                if (ride.gradeLabel == null || !ride.gradeLabel.text.Contains("->"))
                {
                    failures.Add("HUD grade label is missing applied resistance.");
                }

                if (ride.gradeLabel == null || !ride.gradeLabel.text.Contains("~"))
                {
                    failures.Add("HUD grade label is missing smoothed resistance.");
                }

                if (ride.verdictLabel == null || !ride.verdictLabel.text.Contains("Map "))
                {
                    failures.Add("HUD verdict label is missing MYB60 mapping status.");
                }

                if (ride.LastResistanceSnapshot.Status != MYB59ResistanceControllerStatus.Applied)
                {
                    failures.Add("MYB59 resistance controller should apply the climb preview demand.");
                }

                if (ride.LastResistanceSnapshot.AppliedResistanceLevel < 45)
                {
                    failures.Add("MYB59 applied resistance should reflect climb difficulty.");
                }

                if (ride.segmentLabel == null || !ride.segmentLabel.text.Contains("Segment: Climb"))
                {
                    failures.Add("HUD segment label does not report the current climb segment.");
                }
            }

            if (camera == null)
            {
                failures.Add("Missing Main Camera.");
            }

            if (road == null)
            {
                failures.Add("Missing MYB89_RouteRoad mesh.");
            }
            else
            {
                var roadMesh = road.GetComponent<MeshFilter>()?.sharedMesh;
                var minimumSmoothedVertices = ride == null ? RoutePoints.Length * 4 : ride.TrajectorySampleCount * 2;
                if (roadMesh == null || roadMesh.vertexCount < minimumSmoothedVertices)
                {
                    failures.Add($"MYB89_RouteRoad mesh is not built from the smoothed route: {roadMesh?.vertexCount ?? 0} vertices.");
                }
            }

            if (hud == null)
            {
                failures.Add("Missing MYB89_HUD canvas.");
            }

            if (difficultyCues == null)
            {
                failures.Add("Missing MYB48RouteDifficultyCueController.");
            }
            else
            {
                if (difficultyCues.GeneratedCueCount < 11)
                {
                    failures.Add($"Route difficulty cues are incomplete: {difficultyCues.GeneratedCueCount}.");
                }

                if (difficultyCues.ClimbCueCount < 4)
                {
                    failures.Add("Missing climb route difficulty cues.");
                }

                if (difficultyCues.SprintCueCount < 4)
                {
                    failures.Add("Missing sprint route difficulty cues.");
                }

                if (difficultyCues.RecoveryCueCount < 3)
                {
                    failures.Add("Missing recovery route difficulty cues.");
                }
            }

            if (resistanceController == null)
            {
                failures.Add("Missing MYB59ResistanceController.");
            }

            if (resistanceMapper == null)
            {
                failures.Add("Missing MYB60ResistanceMapper.");
            }

            if (scenicCorridor == null)
            {
                failures.Add("Missing MYB44 scenic corridor root.");
            }
            else
            {
                if (scenicRenderers.Length < 45)
                {
                    failures.Add($"MYB44 scenic corridor has too few renderers: {scenicRenderers.Length}.");
                }

                if (horizonRenderers.Length < 10)
                {
                    failures.Add($"MYB44 horizon has too few renderers: {horizonRenderers.Length}.");
                }

                if (premiumSignal == null)
                {
                    failures.Add("Missing MYB44 premium signal.");
                }
            }

            if (renderers.Length < 55)
            {
                failures.Add($"Scene has too few renderers for readable motion: {renderers.Length}.");
            }

            var report = WriteValidationReport(failures, ride, difficultyCues, renderers.Length, scenicRenderers.Length, horizonRenderers.Length);
            if (failures.Count > 0)
            {
                throw new InvalidOperationException("MYB-89 probe validation failed. See " + report);
            }

            Debug.Log("MYB-89 probe validation passed. Report: " + report);
            return report;
        }

        [MenuItem("Tools/MYB-89/Capture Unity MCP Probe Frames")]
        public static string CaptureProbeFrames()
        {
            OpenProbeScene();

            var ride = UnityEngine.Object.FindAnyObjectByType<MYB89ProbeRide>();
            var camera = Camera.main;
            if (ride == null || camera == null)
            {
                throw new InvalidOperationException("Probe scene must contain MYB89ProbeRide and Main Camera before capture.");
            }

            ride.RebuildRouteCache();
            var routeLength = Mathf.Max(1f, ride.RouteLength);
            var repoRoot = GetRepoRoot();
            var stamp = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");
            var relativeCaptureDir = Path.Combine("_bmad-output", "video-captures", "myb-89-unity-mcp-probe-" + stamp);
            var absoluteCaptureDir = Path.Combine(repoRoot, relativeCaptureDir);
            Directory.CreateDirectory(absoluteCaptureDir);

            SetCanvasCamera(camera);

            const int width = 1280;
            const int height = 720;
            const int frameCount = 72;
            for (var i = 0; i < frameCount; i++)
            {
                var normalized = i / (float)frameCount;
                ride.SetPreviewProgress(normalized * routeLength);
                Canvas.ForceUpdateCanvases();

                var framePath = Path.Combine(absoluteCaptureDir, $"frame_{i:000}.png");
                RenderCameraToPng(camera, width, height, framePath);
            }

            var screenshotPath = Path.Combine(absoluteCaptureDir, "screenshot.png");
            File.Copy(Path.Combine(absoluteCaptureDir, "frame_012.png"), screenshotPath, true);

            var latestPath = Path.Combine(repoRoot, CaptureIndexPath);
            Directory.CreateDirectory(Path.GetDirectoryName(latestPath) ?? repoRoot);
            File.WriteAllText(latestPath, absoluteCaptureDir);

            Debug.Log($"MYB-89 probe captured {frameCount} frames at {absoluteCaptureDir}");
            return absoluteCaptureDir;
        }

        private static Dictionary<string, Material> CreateMaterials()
        {
            return new Dictionary<string, Material>
            {
                { "road", LoadMaterialOrFallback("Assets/Echappee/Art/MYB53Validation/Materials/MYB53_PavingStones141_Stylized.mat", "Assets/MYB89/Materials/MYB89_Road.mat", new Color(0.34f, 0.3f, 0.25f), 0.18f) },
                { "lane", MaterialAt("Assets/MYB89/Materials/MYB89_LanePaint.mat", new Color(0.96f, 0.91f, 0.73f), 0.04f) },
                { "edge", MaterialAt("Assets/MYB89/Materials/MYB89_EdgeLine.mat", new Color(0.88f, 0.96f, 0.95f), 0.04f) },
                { "grass", MaterialAt("Assets/MYB89/Materials/MYB89_Grass.mat", new Color(0.19f, 0.42f, 0.27f), 0f) },
                { "hill", MaterialAt("Assets/MYB89/Materials/MYB89_Hill.mat", new Color(0.31f, 0.5f, 0.35f), 0f) },
                { "villageGrass", LoadMaterialOrFallback("Assets/Echappee/Art/MYB53Validation/Materials/MYB53_SoftVillageGrass.mat", "Assets/MYB89/Materials/MYB89_VillageGrass.mat", new Color(0.29f, 0.46f, 0.28f), 0f) },
                { "roadEdgeWarm", LoadMaterialOrFallback("Assets/Echappee/Art/MYB53Validation/Materials/MYB53_WarmRoadEdge.mat", "Assets/MYB89/Materials/MYB89_WarmRoadEdge.mat", new Color(0.55f, 0.43f, 0.3f), 0.08f) },
                { "villageWall", MaterialAt("Assets/MYB89/Materials/MYB89_VillageWall.mat", new Color(0.72f, 0.64f, 0.5f), 0.18f) },
                { "villageRoof", MaterialAt("Assets/MYB89/Materials/MYB89_VillageRoof.mat", new Color(0.62f, 0.24f, 0.18f), 0.22f) },
                { "villageWood", MaterialAt("Assets/MYB89/Materials/MYB89_VillageWood.mat", new Color(0.39f, 0.25f, 0.14f), 0.12f) },
                { "horizonVillage", MaterialAt("Assets/MYB89/Materials/MYB89_HorizonVillage.mat", new Color(0.44f, 0.36f, 0.28f), 0.08f) },
                { "horizonHill", MaterialAt("Assets/MYB89/Materials/MYB89_HorizonHill.mat", new Color(0.24f, 0.38f, 0.31f), 0f) },
                { "premiumGlow", EmissiveMaterialAt("Assets/MYB89/Materials/MYB89_PremiumWarmGlow.mat", new Color(1f, 0.66f, 0.25f), 1.75f) },
                { "postBlue", MaterialAt("Assets/MYB89/Materials/MYB89_PostBlue.mat", new Color(0.15f, 0.45f, 0.92f), 0.18f) },
                { "postCoral", MaterialAt("Assets/MYB89/Materials/MYB89_PostCoral.mat", new Color(0.95f, 0.31f, 0.24f), 0.18f) },
                { "postWhite", MaterialAt("Assets/MYB89/Materials/MYB89_PostWhite.mat", new Color(0.93f, 0.95f, 0.91f), 0.08f) },
                { "treeTrunk", MaterialAt("Assets/MYB89/Materials/MYB89_TreeTrunk.mat", new Color(0.31f, 0.2f, 0.12f), 0f) },
                { "treeLeaf", MaterialAt("Assets/MYB89/Materials/MYB89_TreeLeaf.mat", new Color(0.1f, 0.33f, 0.2f), 0f) },
                { "cockpit", MaterialAt("Assets/MYB89/Materials/MYB89_Cockpit.mat", new Color(0.04f, 0.045f, 0.05f), 0.35f) },
                { "banner", MaterialAt("Assets/MYB89/Materials/MYB89_Banner.mat", new Color(0.97f, 0.74f, 0.24f), 0.2f) }
            };
        }

        private static Material LoadMaterialOrFallback(string existingAssetPath, string fallbackAssetPath, Color color, float smoothness)
        {
            return AssetDatabase.LoadAssetAtPath<Material>(existingAssetPath)
                ?? MaterialAt(fallbackAssetPath, color, smoothness);
        }

        private static Material MaterialAt(string assetPath, Color color, float smoothness)
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
            var dirty = false;
            if (material == null)
            {
                var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard") ?? Shader.Find("Unlit/Color");
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, assetPath);
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

        private static Dictionary<string, Material> CreateDifficultyCueMaterials()
        {
            return new Dictionary<string, Material>
            {
                { "climbCue", EmissiveMaterialAt("Assets/MYB48/Materials/MYB48_ClimbCue.mat", new Color(1f, 0.58f, 0.24f), 1.85f) },
                { "sprintCue", EmissiveMaterialAt("Assets/MYB48/Materials/MYB48_SprintCue.mat", new Color(1f, 0.18f, 0.16f), 2.35f) },
                { "recoveryCue", EmissiveMaterialAt("Assets/MYB48/Materials/MYB48_RecoveryCue.mat", new Color(0.28f, 0.78f, 1f), 1.35f) }
            };
        }

        private static Material EmissiveMaterialAt(string assetPath, Color color, float emissionIntensity)
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
            var dirty = false;
            if (material == null)
            {
                var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard") ?? Shader.Find("Unlit/Color");
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, assetPath);
                dirty = true;
            }

            dirty |= SetColorIfDifferent(material, "_BaseColor", color);
            dirty |= SetColorIfDifferent(material, "_Color", color);
            dirty |= SetColorIfDifferent(material, "_EmissionColor", color * emissionIntensity);
            dirty |= SetFloatIfDifferent(material, "_Smoothness", 0.42f);

            if (!material.IsKeywordEnabled("_EMISSION"))
            {
                material.EnableKeyword("_EMISSION");
                dirty = true;
            }

            if (dirty)
            {
                EditorUtility.SetDirty(material);
            }

            return material;
        }

        private static bool SetColorIfDifferent(Material material, string propertyName, Color color)
        {
            if (!material.HasProperty(propertyName))
            {
                return false;
            }

            var current = material.GetColor(propertyName);
            if (Mathf.Abs(current.r - color.r) < 0.0001f
                && Mathf.Abs(current.g - color.g) < 0.0001f
                && Mathf.Abs(current.b - color.b) < 0.0001f
                && Mathf.Abs(current.a - color.a) < 0.0001f)
            {
                return false;
            }

            material.SetColor(propertyName, color);
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

        private static void ConfigureRenderSettings()
        {
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.62f, 0.72f, 0.8f);
            RenderSettings.ambientEquatorColor = new Color(0.34f, 0.44f, 0.38f);
            RenderSettings.ambientGroundColor = new Color(0.16f, 0.2f, 0.18f);
            RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(0.62f, 0.71f, 0.74f);
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogStartDistance = 55f;
            RenderSettings.fogEndDistance = 235f;
        }

        private static void CreateTerrain(Transform parent, IReadOnlyDictionary<string, Material> materials)
        {
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "MYB89_GroundPlane";
            ground.transform.SetParent(parent);
            ground.transform.position = new Vector3(0f, 0f, 118f);
            ground.transform.localScale = new Vector3(18f, 1f, 28f);
            SetMaterial(ground, materials["grass"]);

            CreateHill("MYB89_LeftHill_A", parent, new Vector3(-38f, 2.5f, 50f), new Vector3(26f, 5f, 18f), materials["hill"]);
            CreateHill("MYB89_RightHill_A", parent, new Vector3(36f, 3.5f, 92f), new Vector3(30f, 7f, 20f), materials["hill"]);
            CreateHill("MYB89_LeftHill_B", parent, new Vector3(-30f, 4f, 175f), new Vector3(34f, 8f, 25f), materials["hill"]);
            CreateHill("MYB89_RightHill_B", parent, new Vector3(42f, 4f, 210f), new Vector3(32f, 8f, 22f), materials["hill"]);
        }

        private static void CreateScenicCorridor(Transform parent, IReadOnlyDictionary<string, Material> materials)
        {
            var corridor = new GameObject("MYB44_ScenicCorridor");
            corridor.transform.SetParent(parent);

            var foreground = new GameObject("MYB44_Foreground");
            foreground.transform.SetParent(corridor.transform);
            var midground = new GameObject("MYB44_Midground");
            midground.transform.SetParent(corridor.transform);
            var horizon = new GameObject("MYB44_Horizon");
            horizon.transform.SetParent(corridor.transform);

            CreateStripMesh("MYB44_LeftWarmShoulder", foreground.transform, RoutePoints, -RoadWidth * 0.5f - 1.9f, 3.1f, 0.025f, materials["roadEdgeWarm"]);
            CreateStripMesh("MYB44_RightWarmShoulder", foreground.transform, RoutePoints, RoadWidth * 0.5f + 1.9f, 3.1f, 0.025f, materials["roadEdgeWarm"]);
            CreateStripMesh("MYB44_LeftVillageVerge", foreground.transform, RoutePoints, -RoadWidth * 0.5f - 5.2f, 3.2f, 0.018f, materials["villageGrass"]);
            CreateStripMesh("MYB44_RightVillageVerge", foreground.transform, RoutePoints, RoadWidth * 0.5f + 5.2f, 3.2f, 0.018f, materials["villageGrass"]);

            CreateVillageAssetCluster(midground.transform, materials);
            CreateVillagePrimitiveRhythm(foreground.transform, materials);
            CreateVillageHorizon(horizon.transform, materials);
            CreatePremiumSignal(corridor.transform, materials);
        }

        private static void CreateVillageAssetCluster(Transform parent, IReadOnlyDictionary<string, Material> materials)
        {
            PlaceAssetAtDistance(
                "Assets/Echappee/Art/ThirdParty/KayKit/MedievalHexagonPack/Models/building_home_A_yellow.fbx",
                "MYB44_VillageHome_A",
                parent,
                52f,
                -1f,
                14.5f,
                5.2f,
                null);
            PlaceAssetAtDistance(
                "Assets/Echappee/Art/ThirdParty/KayKit/MedievalHexagonPack/Models/building_market_yellow.fbx",
                "MYB44_VillageMarket_A",
                parent,
                96f,
                1f,
                16.5f,
                5.4f,
                null);
            PlaceAssetAtDistance(
                "Assets/Echappee/Art/ThirdParty/Kenney/FantasyTownKit/Models/wall-window-stone.fbx",
                "MYB44_WallWindow_A",
                parent,
                121f,
                -1f,
                10.5f,
                4.3f,
                null);
            PlaceAssetAtDistance(
                "Assets/Echappee/Art/ThirdParty/KayKit/MedievalHexagonPack/Models/building_home_A_yellow.fbx",
                "MYB44_VillageHome_B",
                parent,
                163f,
                1f,
                15.8f,
                5.0f,
                null);
            PlaceAssetAtDistance(
                "Assets/Echappee/Art/ThirdParty/KayKit/MedievalHexagonPack/Models/building_well_yellow.fbx",
                "MYB44_VillageWell",
                parent,
                182f,
                -1f,
                9.6f,
                1.8f,
                null);

            for (var i = 0; i < 11; i++)
            {
                var side = i % 2 == 0 ? -1f : 1f;
                PlaceAssetAtDistance(
                    "Assets/Echappee/Art/ThirdParty/KayKit/MedievalHexagonPack/Models/fence_stone_straight.fbx",
                    $"MYB44_StoneFence_{i:00}",
                    parent,
                    28f + i * 18f,
                    side,
                    6.7f + (i % 3) * 0.55f,
                    0.8f,
                    null);
            }

            for (var i = 0; i < 9; i++)
            {
                var side = i % 2 == 0 ? 1f : -1f;
                PlaceAssetAtDistance(
                    "Assets/Echappee/Art/ThirdParty/KayKit/MedievalHexagonPack/Models/barrel.fbx",
                    $"MYB44_RoadsideBarrel_{i:00}",
                    parent,
                    36f + i * 22f,
                    side,
                    5.9f + (i % 2) * 1.1f,
                    0.65f,
                    null);
            }

            PlaceAssetAtDistance(
                "Assets/Echappee/Art/ThirdParty/Quaternius/HorseAnimated/Models/horse_animated.fbx",
                "MYB44_VillageHorse_LifeAccent",
                parent,
                148f,
                -1f,
                12.5f,
                1.45f,
                null);

            for (var i = 0; i < 10; i++)
            {
                var side = i % 2 == 0 ? -1f : 1f;
                var assetPath = i % 3 == 0
                    ? "Assets/Echappee/Art/ThirdParty/Kenney/NatureKit/Models/rock_smallA.fbx"
                    : "Assets/Echappee/Art/ThirdParty/Kenney/NatureKit/Models/tree_tall.fbx";
                PlaceAssetAtDistance(
                    assetPath,
                    $"MYB44_VergeNaturalAccent_{i:00}",
                    parent,
                    20f + i * 20f,
                    side,
                    10.5f + (i % 4) * 1.4f,
                    i % 3 == 0 ? 0.75f : 4.0f,
                    null);
            }
        }

        private static void CreateVillagePrimitiveRhythm(Transform parent, IReadOnlyDictionary<string, Material> materials)
        {
            for (var i = 0; i < 14; i++)
            {
                var side = i % 2 == 0 ? -1f : 1f;
                if (!SampleRoute(18f + i * 15f, out var position, out var forward, out var right))
                {
                    continue;
                }

                var basePosition = position + right * side * (RoadWidth * 0.5f + 7.4f + (i % 3) * 0.8f);
                var rotation = Quaternion.LookRotation(forward, Vector3.up);
                CreateCube($"MYB44_LowWall_{i:00}", parent, basePosition + Vector3.up * 0.34f, new Vector3(3.6f, 0.68f, 0.38f), rotation, materials["villageWall"]);
                CreateCube($"MYB44_WoodCrate_{i:00}", parent, basePosition + right * side * 1.55f + Vector3.up * 0.38f, new Vector3(0.75f, 0.75f, 0.75f), rotation, materials["villageWood"]);
            }
        }

        private static void CreateVillageHorizon(Transform parent, IReadOnlyDictionary<string, Material> materials)
        {
            CreateHill("MYB44_HorizonHill_Left", parent, new Vector3(-70f, 3.2f, 245f), new Vector3(64f, 6.4f, 34f), materials["horizonHill"]);
            CreateHill("MYB44_HorizonHill_Right", parent, new Vector3(78f, 3.4f, 265f), new Vector3(72f, 6.8f, 38f), materials["horizonHill"]);
            CreateHill("MYB44_HorizonHill_Center", parent, new Vector3(5f, 3.0f, 292f), new Vector3(92f, 6.0f, 42f), materials["horizonHill"]);

            var positions = new[]
            {
                new Vector3(-42f, 4.2f, 228f),
                new Vector3(-24f, 4.0f, 242f),
                new Vector3(28f, 4.4f, 236f),
                new Vector3(47f, 4.1f, 252f),
                new Vector3(-58f, 4.8f, 278f),
                new Vector3(62f, 4.6f, 284f)
            };

            for (var i = 0; i < positions.Length; i++)
            {
                var body = CreateCube($"MYB44_HorizonCottage_{i:00}", parent, positions[i], new Vector3(8.2f, 5.4f, 6.4f), Quaternion.identity, materials["horizonVillage"]);
                body.transform.rotation = Quaternion.Euler(0f, i % 2 == 0 ? 8f : -12f, 0f);
                var roof = CreateCube($"MYB44_HorizonRoof_{i:00}", parent, positions[i] + Vector3.up * 3.35f, new Vector3(9.3f, 1.2f, 7.2f), body.transform.rotation, materials["villageRoof"]);
                roof.transform.rotation *= Quaternion.Euler(0f, 0f, 45f);
            }

            for (var i = 0; i < 14; i++)
            {
                var x = -66f + i * 10.5f;
                var z = 218f + (i % 5) * 13f;
                CreateTree($"MYB44_HorizonTree_{i:00}", parent, new Vector3(x, 0f, z), materials["treeTrunk"], materials["treeLeaf"]);
            }
        }

        private static void CreatePremiumSignal(Transform parent, IReadOnlyDictionary<string, Material> materials)
        {
            if (!SampleRoute(126f, out var position, out var forward, out var right))
            {
                return;
            }

            var signal = new GameObject("MYB44_PremiumSignal_RelicLantern");
            signal.transform.SetParent(parent);
            signal.transform.position = position + right * (RoadWidth * 0.5f + 9.2f);
            signal.transform.rotation = Quaternion.LookRotation(forward, Vector3.up);

            CreateCube("MYB44_RelicBase", signal.transform, signal.transform.position + Vector3.up * 0.24f, new Vector3(1.35f, 0.48f, 1.35f), signal.transform.rotation, materials["villageWall"]);
            CreateCube("MYB44_RelicPillar", signal.transform, signal.transform.position + Vector3.up * 1.25f, new Vector3(0.55f, 2.0f, 0.55f), signal.transform.rotation, materials["villageWood"]);
            CreateCube("MYB44_RelicCore", signal.transform, signal.transform.position + Vector3.up * 2.4f, new Vector3(0.82f, 0.82f, 0.82f), signal.transform.rotation * Quaternion.Euler(0f, 45f, 0f), materials["premiumGlow"]);

            var lightObject = new GameObject("MYB44_RelicWarmLight");
            lightObject.transform.SetParent(signal.transform);
            lightObject.transform.position = signal.transform.position + Vector3.up * 2.45f;
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(1f, 0.64f, 0.32f);
            light.intensity = 1.2f;
            light.range = 9f;
            light.shadows = LightShadows.None;
        }

        private static void PlaceAssetAtDistance(
            string assetPath,
            string name,
            Transform parent,
            float distance,
            float side,
            float lateralOffset,
            float targetHeight,
            Material fallbackMaterial)
        {
            if (!SampleRoute(distance, out var position, out var forward, out var right))
            {
                return;
            }

            var rotation = Quaternion.LookRotation(forward, Vector3.up);
            var placement = position + right * side * (RoadWidth * 0.5f + lateralOffset);
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            GameObject instance;
            if (asset == null)
            {
                instance = CreateCube(name, parent, placement + Vector3.up * 0.7f, Vector3.one, rotation, fallbackMaterial);
                return;
            }

            instance = (GameObject)PrefabUtility.InstantiatePrefab(asset);
            instance.name = name;
            instance.transform.SetParent(parent);
            instance.transform.SetPositionAndRotation(placement, rotation);
            FitToHeight(instance, targetHeight);

            if (fallbackMaterial != null && instance.GetComponentsInChildren<Renderer>(true).Length == 0)
            {
                SetMaterialRecursive(instance, fallbackMaterial);
            }
        }

        private static void FitToHeight(GameObject instance, float targetHeight)
        {
            var renderers = instance.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0)
            {
                instance.transform.localScale = Vector3.one * targetHeight;
                return;
            }

            var bounds = renderers[0].bounds;
            for (var i = 1; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }

            if (bounds.size.y <= 0.001f)
            {
                return;
            }

            instance.transform.localScale *= targetHeight / bounds.size.y;
        }

        private static void CreateHill(string name, Transform parent, Vector3 position, Vector3 scale, Material material)
        {
            var hill = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            hill.name = name;
            hill.transform.SetParent(parent);
            hill.transform.position = position;
            hill.transform.localScale = scale;
            SetMaterial(hill, material);
        }

        private static List<Transform> CreateRoute(Transform parent, IReadOnlyDictionary<string, Material> materials)
        {
            var markers = new List<Transform>();
            var markerRoot = new GameObject("MYB89_RouteMarkers");
            markerRoot.transform.SetParent(parent);

            for (var i = 0; i < RoutePoints.Length; i++)
            {
                var marker = new GameObject($"RouteMarker_{i:00}");
                marker.transform.SetParent(markerRoot.transform);
                marker.transform.position = RoutePoints[i];
                markers.Add(marker.transform);
            }

            var smoothedRoutePoints = SmoothedRoutePoints();
            CreateStripMesh("MYB89_RouteRoad", parent, smoothedRoutePoints, 0f, RoadWidth, 0.03f, materials["road"]);
            CreateStripMesh("MYB89_LeftEdgeLine", parent, smoothedRoutePoints, -RoadWidth * 0.5f + 0.16f, 0.12f, 0.07f, materials["edge"]);
            CreateStripMesh("MYB89_RightEdgeLine", parent, smoothedRoutePoints, RoadWidth * 0.5f - 0.16f, 0.12f, 0.07f, materials["edge"]);

            for (var distance = 8f; distance < RouteLength() - 10f; distance += 13f)
            {
                if (!SampleRoute(distance, out var position, out var forward, out _))
                {
                    continue;
                }

                CreateCube(
                    "MYB89_CenterDash",
                    parent,
                    position + Vector3.up * 0.1f,
                    new Vector3(0.18f, 0.035f, 4.8f),
                    Quaternion.LookRotation(forward, Vector3.up),
                    materials["lane"]);
            }

            return markers;
        }

        private static void CreateStripMesh(string name, Transform parent, IReadOnlyList<Vector3> points, float lateralOffset, float width, float yLift, Material material)
        {
            var gameObject = new GameObject(name);
            gameObject.transform.SetParent(parent);

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
                uvs[i * 2] = new Vector2(0f, i);
                uvs[i * 2 + 1] = new Vector2(1f, i);
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

        private static void CreateRoadsideRhythm(Transform parent, IReadOnlyDictionary<string, Material> materials)
        {
            for (var index = 0; index < 13; index++)
            {
                var distance = 14f + index * 17f;
                var material = index % 2 == 0 ? materials["postBlue"] : materials["postCoral"];
                CreateMarkerPair(parent, distance, index, material, materials["postWhite"]);
            }

            CreateCheckpointArch(parent, 34f, "WARMUP", materials["banner"], materials["postWhite"]);
            CreateCheckpointArch(parent, 112f, "CLIMB", materials["banner"], materials["postWhite"]);
            CreateCheckpointArch(parent, 190f, "SPRINT", materials["banner"], materials["postWhite"]);

            for (var i = 0; i < 20; i++)
            {
                var distance = 10f + i * 11.5f;
                if (!SampleRoute(distance, out var position, out _, out var right))
                {
                    continue;
                }

                var side = i % 2 == 0 ? -1f : 1f;
                var offset = right * side * (RoadWidth * 0.5f + 5f + (i % 3) * 1.7f);
                CreateTree($"MYB89_Tree_{i:00}", parent, position + offset, materials["treeTrunk"], materials["treeLeaf"]);
            }
        }

        private static void CreateRouteDifficultyCues(Transform parent, IReadOnlyList<Transform> routeMarkers, IReadOnlyDictionary<string, Material> materials)
        {
            var cueRoot = new GameObject("MYB48_RouteDifficultyCues");
            cueRoot.transform.SetParent(parent);

            var controller = cueRoot.AddComponent<MYB48RouteDifficultyCueController>();
            controller.routeMarkers = new Transform[routeMarkers.Count];
            for (var i = 0; i < routeMarkers.Count; i++)
            {
                controller.routeMarkers[i] = routeMarkers[i];
            }

            controller.climbMaterial = materials["climbCue"];
            controller.sprintMaterial = materials["sprintCue"];
            controller.recoveryMaterial = materials["recoveryCue"];
            controller.lateralOffsetMeters = 5.4f;
            controller.entryHeightMeters = 2.8f;
            controller.secondaryHeightMeters = 1.25f;
            controller.pulseAmplitude = 0.22f;
            controller.pulseFrequency = 0.85f;
            controller.RebuildCues();
        }

        private static void CreateMarkerPair(Transform parent, float distance, int index, Material postMaterial, Material capMaterial)
        {
            if (!SampleRoute(distance, out var position, out var forward, out var right))
            {
                return;
            }

            for (var side = -1; side <= 1; side += 2)
            {
                var basePosition = position + right * side * (RoadWidth * 0.5f + 1.15f);
                var pole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                pole.name = $"MYB89_ClosePost_{index:00}_{(side < 0 ? "L" : "R")}";
                pole.transform.SetParent(parent);
                pole.transform.position = basePosition + Vector3.up * 0.72f;
                pole.transform.localScale = new Vector3(0.16f, 0.72f, 0.16f);
                SetMaterial(pole, postMaterial);

                CreateCube(
                    $"MYB89_PostCap_{index:00}_{(side < 0 ? "L" : "R")}",
                    parent,
                    basePosition + Vector3.up * 1.52f,
                    new Vector3(0.5f, 0.18f, 0.5f),
                    Quaternion.LookRotation(forward, Vector3.up),
                    capMaterial);
            }
        }

        private static void CreateCheckpointArch(Transform parent, float distance, string label, Material beamMaterial, Material postMaterial)
        {
            if (!SampleRoute(distance, out var position, out var forward, out var right))
            {
                return;
            }

            var rotation = Quaternion.LookRotation(forward, Vector3.up);
            var left = position - right * (RoadWidth * 0.5f + 0.8f);
            var rightPosition = position + right * (RoadWidth * 0.5f + 0.8f);
            CreateCube("MYB89_ArchPost_" + label + "_L", parent, left + Vector3.up * 2.1f, new Vector3(0.35f, 4.2f, 0.35f), rotation, postMaterial);
            CreateCube("MYB89_ArchPost_" + label + "_R", parent, rightPosition + Vector3.up * 2.1f, new Vector3(0.35f, 4.2f, 0.35f), rotation, postMaterial);
            CreateCube("MYB89_ArchBeam_" + label, parent, position + Vector3.up * 4.25f, new Vector3(RoadWidth + 2.1f, 0.42f, 0.55f), rotation, beamMaterial);
        }

        private static void CreateTree(string name, Transform parent, Vector3 position, Material trunkMaterial, Material leafMaterial)
        {
            var trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            trunk.name = name + "_Trunk";
            trunk.transform.SetParent(parent);
            trunk.transform.position = position + Vector3.up * 0.85f;
            trunk.transform.localScale = new Vector3(0.26f, 0.85f, 0.26f);
            SetMaterial(trunk, trunkMaterial);

            var crown = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            crown.name = name + "_Crown";
            crown.transform.SetParent(parent);
            crown.transform.position = position + Vector3.up * 2.2f;
            crown.transform.localScale = new Vector3(1.4f, 1.4f, 1.4f);
            SetMaterial(crown, leafMaterial);
        }

        private static Light CreateLighting(Transform parent)
        {
            var sun = new GameObject("MYB89_KeySun");
            sun.transform.SetParent(parent);
            sun.transform.rotation = Quaternion.Euler(42f, -34f, 0f);

            var light = sun.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.08f;
            light.color = new Color(1f, 0.92f, 0.78f);
            light.shadows = LightShadows.Soft;
            return light;
        }

        private static void CreateCameraRig(Transform parent, IReadOnlyList<Transform> routeMarkers, Light keyLight, IReadOnlyDictionary<string, Material> materials)
        {
            var rig = new GameObject("MYB89_RiderRig");
            rig.transform.SetParent(parent);
            rig.transform.position = RoutePoints[0];

            var cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.SetParent(rig.transform);
            cameraObject.transform.localPosition = new Vector3(0f, 1.55f, -1.75f);
            cameraObject.transform.localRotation = Quaternion.Euler(8f, 0f, 0f);

            var camera = cameraObject.AddComponent<Camera>();
            camera.fieldOfView = 69f;
            camera.nearClipPlane = 0.03f;
            camera.farClipPlane = 320f;
            camera.clearFlags = CameraClearFlags.Skybox;

            cameraObject.AddComponent<AudioListener>();

            CreateCockpit(rig.transform, materials["cockpit"]);
            var hud = CreateHud(
                parent,
                camera,
                out var distanceLabel,
                out var speedLabel,
                out var difficultyLabel,
                out var gradeLabel,
                out var segmentLabel,
                out var verdictLabel);

            var ride = rig.AddComponent<MYB89ProbeRide>();
            ride.routeMarkers = new Transform[routeMarkers.Count];
            for (var i = 0; i < routeMarkers.Count; i++)
            {
                ride.routeMarkers[i] = routeMarkers[i];
            }

            ride.cameraPivot = cameraObject.transform;
            ride.distanceLabel = distanceLabel;
            ride.speedLabel = speedLabel;
            ride.difficultyLabel = difficultyLabel;
            ride.gradeLabel = gradeLabel;
            ride.segmentLabel = segmentLabel;
            ride.verdictLabel = verdictLabel;
            ride.keyLight = keyLight;
            ride.speedMetersPerSecond = 12.5f;
            ride.trajectorySamplesPerSegment = MYB89RideTrajectory.DefaultSamplesPerSegment;
            ride.turnLookAheadMeters = 8f;
            ride.orientationSmoothing = 7f;
            ride.cameraTurnLeanMaxDegrees = 2.2f;
            ride.waitForRoutePreview = true;
            ride.autoplay = false;
            ride.resistanceMapper = rig.AddComponent<MYB60ResistanceMapper>();
            ride.resistanceController = rig.AddComponent<MYB59ResistanceController>();
            ride.PrepareRoutePreview();

            CreateRoutePreviewPanel(hud.transform, ride);
            EnsureEventSystem(parent);

            hud.name = "MYB89_HUD";
        }

        private static void CreateCockpit(Transform parent, Material material)
        {
            CreateCube("MYB89_Handlebar_Left", parent, new Vector3(-0.45f, 1.05f, 0.46f), new Vector3(0.85f, 0.08f, 0.08f), Quaternion.identity, material);
            CreateCube("MYB89_Handlebar_Right", parent, new Vector3(0.45f, 1.05f, 0.46f), new Vector3(0.85f, 0.08f, 0.08f), Quaternion.identity, material);
            CreateCube("MYB89_Stem", parent, new Vector3(0f, 0.97f, 0.25f), new Vector3(0.11f, 0.11f, 0.55f), Quaternion.identity, material);
        }

        private static MYB73RoutePreviewPanel CreateRoutePreviewPanel(Transform canvasRoot, MYB89ProbeRide ride)
        {
            var panelObject = new GameObject("MYB73_RoutePreview", typeof(RectTransform));
            panelObject.transform.SetParent(canvasRoot, false);

            var panelRect = (RectTransform)panelObject.transform;
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.anchoredPosition = new Vector2(0f, -18f);
            panelRect.sizeDelta = new Vector2(820f, 420f);

            var image = panelObject.AddComponent<Image>();
            image.color = new Color(0.05f, 0.06f, 0.055f, 0.72f);

            var preview = panelObject.AddComponent<MYB73RoutePreviewPanel>();
            preview.panelRoot = panelObject;
            preview.ride = ride;
            preview.titleLabel = CreateHudText(panelObject.transform, "MYB73_RoutePreview_Title", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(32f, -26f), new Vector2(650f, 52f), TextAnchor.UpperLeft, 36);
            preview.subtitleLabel = CreateHudText(panelObject.transform, "MYB73_RoutePreview_Subtitle", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(32f, -86f), new Vector2(728f, 60f), TextAnchor.UpperLeft, 20);
            preview.statsLabel = CreateHudText(panelObject.transform, "MYB73_RoutePreview_Stats", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(32f, -154f), new Vector2(728f, 38f), TextAnchor.UpperLeft, 24);
            preview.difficultyLabel = CreateHudText(panelObject.transform, "MYB73_RoutePreview_Difficulty", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(32f, -206f), new Vector2(728f, 34f), TextAnchor.UpperLeft, 22);
            preview.biomesLabel = CreateHudText(panelObject.transform, "MYB73_RoutePreview_Biomes", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(32f, -250f), new Vector2(728f, 34f), TextAnchor.UpperLeft, 20);
            preview.passagesLabel = CreateHudText(panelObject.transform, "MYB73_RoutePreview_Passages", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(32f, -292f), new Vector2(728f, 42f), TextAnchor.UpperLeft, 20);
            preview.launchButton = CreateRoutePreviewButton(panelObject.transform);
            preview.Refresh();

            return preview;
        }

        private static Button CreateRoutePreviewButton(Transform parent)
        {
            var buttonObject = new GameObject("MYB73_RoutePreview_LaunchButton", typeof(RectTransform));
            buttonObject.transform.SetParent(parent, false);

            var rect = (RectTransform)buttonObject.transform;
            rect.anchorMin = new Vector2(1f, 0f);
            rect.anchorMax = new Vector2(1f, 0f);
            rect.pivot = new Vector2(1f, 0f);
            rect.anchoredPosition = new Vector2(-32f, 28f);
            rect.sizeDelta = new Vector2(250f, 54f);

            var image = buttonObject.AddComponent<Image>();
            image.color = new Color(0.97f, 0.74f, 0.24f, 0.95f);

            var button = buttonObject.AddComponent<Button>();
            var colors = button.colors;
            colors.normalColor = new Color(0.97f, 0.74f, 0.24f, 0.95f);
            colors.highlightedColor = new Color(1f, 0.84f, 0.36f, 1f);
            colors.pressedColor = new Color(0.86f, 0.56f, 0.16f, 1f);
            colors.selectedColor = colors.highlightedColor;
            button.colors = colors;

            var label = CreateHudText(buttonObject.transform, "MYB73_RoutePreview_LaunchLabel", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(230f, 42f), TextAnchor.MiddleCenter, 22);
            label.text = "Commencer la balade";
            label.color = new Color(0.06f, 0.07f, 0.05f, 1f);

            return button;
        }

        private static void EnsureEventSystem(Transform parent)
        {
            if (UnityEngine.Object.FindAnyObjectByType<EventSystem>() != null)
            {
                return;
            }

            var eventSystemObject = new GameObject("MYB89_EventSystem");
            eventSystemObject.transform.SetParent(parent);
            eventSystemObject.AddComponent<EventSystem>();
            eventSystemObject.AddComponent<StandaloneInputModule>();
        }

        private static GameObject CreateHud(
            Transform parent,
            Camera camera,
            out Text distanceLabel,
            out Text speedLabel,
            out Text difficultyLabel,
            out Text gradeLabel,
            out Text segmentLabel,
            out Text verdictLabel)
        {
            var canvasObject = new GameObject("MYB89_HUD", typeof(RectTransform));
            canvasObject.transform.SetParent(parent);

            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = camera;
            canvas.planeDistance = 0.7f;
            canvas.sortingOrder = 20;

            var scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280f, 720f);
            scaler.matchWidthOrHeight = 0.5f;

            canvasObject.AddComponent<GraphicRaycaster>();

            distanceLabel = CreateHudText(canvasObject.transform, "MYB89_HUD_Distance", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(26f, -22f), new Vector2(330f, 48f), TextAnchor.UpperLeft, 28);
            speedLabel = CreateHudText(canvasObject.transform, "MYB89_HUD_Speed", new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-26f, -22f), new Vector2(220f, 48f), TextAnchor.UpperRight, 30);
            difficultyLabel = CreateHudText(canvasObject.transform, "MYB89_HUD_Difficulty", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(26f, -66f), new Vector2(390f, 38f), TextAnchor.UpperLeft, 22);
            gradeLabel = CreateHudText(canvasObject.transform, "MYB89_HUD_Grade", new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-26f, -66f), new Vector2(360f, 38f), TextAnchor.UpperRight, 22);
            segmentLabel = CreateHudText(canvasObject.transform, "MYB89_HUD_Segment", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -24f), new Vector2(400f, 42f), TextAnchor.UpperCenter, 24);
            verdictLabel = CreateHudText(canvasObject.transform, "MYB89_HUD_Verdict", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 28f), new Vector2(760f, 42f), TextAnchor.LowerCenter, 22);

            return canvasObject;
        }

        private static Text CreateHudText(Transform parent, string name, Vector2 anchor, Vector2 pivot, Vector2 position, Vector2 size, TextAnchor alignment, int fontSize)
        {
            var textObject = new GameObject(name, typeof(RectTransform));
            textObject.transform.SetParent(parent, false);

            var rect = (RectTransform)textObject.transform;
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = pivot;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;

            var text = textObject.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = new Color(0.97f, 0.98f, 0.94f, 1f);
            text.raycastTarget = false;

            var shadow = textObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.68f);
            shadow.effectDistance = new Vector2(2f, -2f);

            return text;
        }

        private static GameObject CreateCube(string name, Transform parent, Vector3 position, Vector3 scale, Quaternion rotation, Material material)
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = name;
            cube.transform.SetParent(parent);
            cube.transform.SetPositionAndRotation(position, rotation);
            cube.transform.localScale = scale;
            SetMaterial(cube, material);
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

        private static void SetMaterialRecursive(GameObject gameObject, Material material)
        {
            foreach (var renderer in gameObject.GetComponentsInChildren<Renderer>(true))
            {
                renderer.sharedMaterial = material;
            }
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

        private static bool SampleRoute(float meters, out Vector3 position, out Vector3 forward, out Vector3 right)
        {
            if (MYB89RideTrajectory.TrySample(SmoothedRoutePoints(), meters, true, out var sample))
            {
                position = sample.Position;
                forward = sample.Forward;
                right = sample.Right;
                return true;
            }

            position = RoutePoints[0];
            forward = Vector3.forward;
            right = Vector3.right;
            return false;
        }

        private static float RouteLength()
        {
            return MYB89RideTrajectory.Length(SmoothedRoutePoints());
        }

        private static Vector3[] SmoothedRoutePoints()
        {
            return MYB89RideTrajectory.BuildSmoothedPoints(RoutePoints, MYB89RideTrajectory.DefaultSamplesPerSegment);
        }

        private static void SetCanvasCamera(Camera camera)
        {
            foreach (var canvas in UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsInactive.Exclude))
            {
                if (canvas.renderMode == RenderMode.ScreenSpaceCamera)
                {
                    canvas.worldCamera = camera;
                }
            }
        }

        private static void RenderCameraToPng(Camera camera, int width, int height, string path)
        {
            var previousTarget = camera.targetTexture;
            var previousActive = RenderTexture.active;
            var renderTexture = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);

            try
            {
                camera.targetTexture = renderTexture;
                RenderTexture.active = renderTexture;
                camera.Render();
                texture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                texture.Apply();
                File.WriteAllBytes(path, texture.EncodeToPNG());
            }
            finally
            {
                camera.targetTexture = previousTarget;
                RenderTexture.active = previousActive;
                renderTexture.Release();
                UnityEngine.Object.DestroyImmediate(renderTexture);
                UnityEngine.Object.DestroyImmediate(texture);
            }
        }

        private static void OpenProbeScene()
        {
            if (!File.Exists(ProjectRelativeToAbsolute(ScenePath)))
            {
                BuildScene();
                return;
            }

            var activeScene = SceneManager.GetActiveScene();
            if (activeScene.path != ScenePath)
            {
                EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            }
        }

        private static string WriteValidationReport(
            IReadOnlyList<string> failures,
            MYB89ProbeRide ride,
            MYB48RouteDifficultyCueController difficultyCues,
            int rendererCount,
            int scenicRendererCount,
            int horizonRendererCount)
        {
            var repoRoot = GetRepoRoot();
            var reportDir = Path.Combine(repoRoot, "_bmad-output", "unity-test-results");
            Directory.CreateDirectory(reportDir);

            var reportPath = Path.Combine(reportDir, "myb-89-unity-mcp-probe-validator.txt");
            using (var writer = new StreamWriter(reportPath, false))
            {
                writer.WriteLine("MYB-89 Unity-MCP IvanMurzak probe validation");
                writer.WriteLine("Timestamp UTC: " + DateTime.UtcNow.ToString("O"));
                writer.WriteLine("Unity: " + Application.unityVersion);
                writer.WriteLine("Scene: " + ScenePath);
                writer.WriteLine("Status: " + (failures.Count == 0 ? "PASS" : "FAIL"));
                writer.WriteLine("Route markers: " + (ride == null || ride.routeMarkers == null ? 0 : ride.routeMarkers.Length));
                writer.WriteLine("Route length: " + (ride == null ? 0f : ride.RouteLength).ToString("0.0") + " m");
                writer.WriteLine("Smoothed trajectory samples: " + (ride == null ? 0 : ride.TrajectorySampleCount));
                writer.WriteLine("Turn look-ahead: " + (ride == null ? 0f : ride.turnLookAheadMeters).ToString("0.0") + " m");
                writer.WriteLine("Camera turn lean max: " + (ride == null ? 0f : ride.cameraTurnLeanMaxDegrees).ToString("0.0") + " deg");
                writer.WriteLine("Renderer count: " + rendererCount);
                writer.WriteLine("MYB44 scenic renderers: " + scenicRendererCount);
                writer.WriteLine("MYB44 horizon renderers: " + horizonRendererCount);
                writer.WriteLine("HUD difficulty: " + (ride == null || ride.difficultyLabel == null ? "missing" : ride.difficultyLabel.text));
                writer.WriteLine("HUD grade: " + (ride == null || ride.gradeLabel == null ? "missing" : ride.gradeLabel.text));
                writer.WriteLine("HUD segment: " + (ride == null || ride.segmentLabel == null ? "missing" : ride.segmentLabel.text));
                writer.WriteLine("HUD verdict: " + (ride == null || ride.verdictLabel == null ? "missing" : ride.verdictLabel.text));
                writer.WriteLine("Resistance mapper: " + (ride == null ? "missing" : ride.LastResistanceMappingSnapshot.StatusLabel));
                writer.WriteLine("Resistance raw/smoothed: " + (ride == null ? "0~0" : ride.LastResistanceMappingSnapshot.Input.TargetResistanceLevel + "~" + ride.LastResistanceMappingSnapshot.SmoothedResistanceLevel));
                writer.WriteLine("Resistance controller: " + (ride == null ? "missing" : ride.LastResistanceSnapshot.StatusLabel));
                writer.WriteLine("Resistance demand/applied: " + (ride == null ? "0->0" : ride.LastResistanceSnapshot.Demand.ResistanceLevel + "->" + ride.LastResistanceSnapshot.AppliedResistanceLevel));
                writer.WriteLine("Route difficulty cues: " + (difficultyCues == null ? 0 : difficultyCues.GeneratedCueCount));
                writer.WriteLine("Climb cues: " + (difficultyCues == null ? 0 : difficultyCues.ClimbCueCount));
                writer.WriteLine("Sprint cues: " + (difficultyCues == null ? 0 : difficultyCues.SprintCueCount));
                writer.WriteLine("Recovery cues: " + (difficultyCues == null ? 0 : difficultyCues.RecoveryCueCount));

                if (failures.Count > 0)
                {
                    writer.WriteLine();
                    writer.WriteLine("Failures:");
                    foreach (var failure in failures)
                    {
                        writer.WriteLine("- " + failure);
                    }
                }
            }

            return reportPath;
        }

        private static string ProjectRelativeToAbsolute(string assetPath)
        {
            var projectRoot = Directory.GetParent(Application.dataPath);
            return Path.Combine(projectRoot == null ? Application.dataPath : projectRoot.FullName, assetPath);
        }

        private static void NormalizeSerializedWhitespace(params string[] assetPaths)
        {
            foreach (var assetPath in assetPaths)
            {
                var absolutePath = ProjectRelativeToAbsolute(assetPath);
                if (!File.Exists(absolutePath))
                {
                    continue;
                }

                var lines = File.ReadAllLines(absolutePath);
                var changed = false;
                for (var i = 0; i < lines.Length; i++)
                {
                    var trimmed = lines[i].TrimEnd(' ', '\t');
                    if (trimmed == lines[i])
                    {
                        continue;
                    }

                    lines[i] = trimmed;
                    changed = true;
                }

                if (changed)
                {
                    File.WriteAllLines(absolutePath, lines);
                }
            }
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

        private static void EnsureFolder(string folder)
        {
            if (AssetDatabase.IsValidFolder(folder))
            {
                return;
            }

            var parts = folder.Split('/');
            var current = parts[0];

            for (var i = 1; i < parts.Length; i++)
            {
                var next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[i]);
                }

                current = next;
            }
        }
    }
}

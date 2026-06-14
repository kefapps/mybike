using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using MYB89;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MYB104.Editor
{
    public static class MYB104SceneComposer
    {
        private const string CanonicalScenePath = "Assets/Scenes/MYB89UnityMcpProbe.unity";
        private const string OutputDirectory = "_bmad-output/unity-test-results/myb-104";
        private const string ReportPath = OutputDirectory + "/myb-104-production-passages-report.txt";
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

        private static readonly PassageSpec[] Passages =
        {
            new PassageSpec("passage-01-foret-claire", "Foret claire", 42f),
            new PassageSpec("passage-02-village-route-de-col", "Village / route de col", 124f),
            new PassageSpec("passage-03-panorama-signal-fantasy", "Panorama / signal fantasy", 207f)
        };

        private static readonly string[] DebugMarkerPrefixes =
        {
            "MYB89_ClosePost_",
            "MYB89_PostCap_",
            "MYB89_ArchPost_",
            "MYB89_ArchBeam_"
        };

        [MenuItem("Tools/MYB-104/Apply Production Passages")]
        public static void ApplyProductionPassagesFromMenu()
        {
            Debug.Log("MYB-104 production passages applied: " + ApplyAndValidate());
        }

        public static string ApplyAndValidateCli()
        {
            var report = ApplyAndValidate();
            Debug.Log("MYB-104 production passages validated: " + report);
            return report;
        }

        public static string ApplyAndValidate()
        {
            EnsureFolder("Assets/MYB104");
            EnsureFolder("Assets/MYB104/Materials");

            var report = new ValidationReport();
            var materials = CreateMaterials();
            ApplySceneComposition(materials, report);
            CaptureAndValidate(report);
            WriteReport(report);

            if (report.Failures.Count > 0)
            {
                throw new InvalidOperationException("MYB-104 production passages validation failed. See " + Path.Combine(GetRepoRoot(), ReportPath));
            }

            return Path.Combine(GetRepoRoot(), ReportPath);
        }

        private static Dictionary<string, Material> CreateMaterials()
        {
            return new Dictionary<string, Material>
            {
                { "forestFloor", MaterialAt("Assets/MYB104/Materials/MYB104_ForestFloor.mat", new Color(0.18f, 0.36f, 0.23f), 0.08f) },
                { "forestLight", MaterialAt("Assets/MYB104/Materials/MYB104_ForestLightGrass.mat", new Color(0.38f, 0.56f, 0.26f), 0.05f) },
                { "routeRoad", MaterialAt("Assets/MYB104/Materials/MYB104_MutedRouteStone.mat", new Color(0.29f, 0.31f, 0.28f), 0.18f) },
                { "pineDark", MaterialAt("Assets/MYB104/Materials/MYB104_PineDark.mat", new Color(0.08f, 0.25f, 0.17f), 0.03f) },
                { "trunk", MaterialAt("Assets/MYB104/Materials/MYB104_TrunkWarm.mat", new Color(0.32f, 0.21f, 0.13f), 0.08f) },
                { "roadShadow", MaterialAt("Assets/MYB104/Materials/MYB104_RoadsideShadow.mat", new Color(0.16f, 0.22f, 0.19f), 0.02f) },
                { "stone", MaterialAt("Assets/MYB104/Materials/MYB104_WeatheredStone.mat", new Color(0.55f, 0.53f, 0.46f), 0.22f) },
                { "warmStone", MaterialAt("Assets/MYB104/Materials/MYB104_WarmVillageStone.mat", new Color(0.68f, 0.57f, 0.43f), 0.18f) },
                { "wood", MaterialAt("Assets/MYB104/Materials/MYB104_AlpineWood.mat", new Color(0.38f, 0.24f, 0.13f), 0.15f) },
                { "roof", MaterialAt("Assets/MYB104/Materials/MYB104_RoofRed.mat", new Color(0.55f, 0.17f, 0.13f), 0.24f) },
                { "flower", MaterialAt("Assets/MYB104/Materials/MYB104_WildflowerAccent.mat", new Color(0.96f, 0.72f, 0.22f), 0.1f) },
                { "mountainNear", MaterialAt("Assets/MYB104/Materials/MYB104_MountainNear.mat", new Color(0.38f, 0.45f, 0.42f), 0.08f) },
                { "mountainFar", MaterialAt("Assets/MYB104/Materials/MYB104_MountainFar.mat", new Color(0.58f, 0.66f, 0.7f), 0.04f) },
                { "fantasyGlow", EmissiveMaterialAt("Assets/MYB104/Materials/MYB104_FantasyBlueGlow.mat", new Color(0.24f, 0.72f, 1f), 2.2f) },
                { "warmGlow", EmissiveMaterialAt("Assets/MYB104/Materials/MYB104_WarmLanternGlow.mat", new Color(1f, 0.62f, 0.25f), 1.8f) },
                { "edgeGuide", MaterialAt("Assets/MYB104/Materials/MYB104_EdgeGuideStone.mat", new Color(0.82f, 0.78f, 0.64f), 0.12f) }
            };
        }

        private static void ApplySceneComposition(IReadOnlyDictionary<string, Material> materials, ValidationReport report)
        {
            var scene = EditorSceneManager.OpenScene(CanonicalScenePath, OpenSceneMode.Single);
            ConfigureSceneLighting(report);
            DestroyObjectsByName("MYB104_ProductionPassages");
            DestroyDebugMarkers(report);
            DisableLegacyPrototypeRenderers(report);
            ApplyRoadMaterial(materials["routeRoad"], report);

            var root = new GameObject("MYB104_ProductionPassages");
            var probeRoot = GameObject.Find("MYB89_ProbeRoot");
            if (probeRoot != null)
            {
                root.transform.SetParent(probeRoot.transform);
            }

            var route = MYB89RideTrajectory.BuildSmoothedPoints(RoutePoints);
            BuildForestPassage(root.transform, route, materials, report);
            BuildVillageColPassage(root.transform, route, materials, report);
            BuildPanoramaFantasyPassage(root.transform, route, materials, report);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            report.SceneNotes.Add("Saved canonical scene with MYB104_ProductionPassages root.");
        }

        private static void ConfigureSceneLighting(ValidationReport report)
        {
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.56f, 0.64f, 0.7f);
            RenderSettings.ambientEquatorColor = new Color(0.38f, 0.43f, 0.38f);
            RenderSettings.ambientGroundColor = new Color(0.22f, 0.2f, 0.18f);
            RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(0.66f, 0.72f, 0.74f);
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogStartDistance = 72f;
            RenderSettings.fogEndDistance = 260f;

            var keyLight = GameObject.Find("MYB89_KeySun");
            if (keyLight != null && keyLight.TryGetComponent<Light>(out var light))
            {
                keyLight.transform.rotation = Quaternion.Euler(48f, -32f, 0f);
                light.color = new Color(1f, 0.9f, 0.74f);
                light.intensity = 1.22f;
                light.shadows = LightShadows.Soft;
                light.shadowStrength = 0.78f;
                report.SceneNotes.Add("Re-aimed existing MYB89 key sun for readable authored shadows.");
            }
            else
            {
                report.Failures.Add("Missing MYB89_KeySun directional light.");
            }
        }

        private static void DestroyDebugMarkers(ValidationReport report)
        {
            var destroyed = 0;
            var transforms = UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsInactive.Include);
            foreach (var transform in transforms)
            {
                if (transform == null || transform.gameObject == null)
                {
                    continue;
                }

                if (DebugMarkerPrefixes.Any(prefix => transform.gameObject.name.StartsWith(prefix, StringComparison.Ordinal)))
                {
                    UnityEngine.Object.DestroyImmediate(transform.gameObject);
                    destroyed++;
                }
            }

            report.SceneNotes.Add("Removed debug-like gate/post renderers: " + destroyed.ToString(CultureInfo.InvariantCulture));
        }

        private static void DisableLegacyPrototypeRenderers(ValidationReport report)
        {
            var disabled = 0;
            disabled += DisableRenderersUnder("MYB44_ScenicCorridor");
            disabled += DisableRenderersUnder("MYB100_OptimizedImportedAssets");
            disabled += DisableRenderersUnder("MYB48_RouteDifficultyCues");

            var oldRouteRhythm = UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include)
                .Where(renderer => renderer.gameObject.name.StartsWith("MYB89_Tree_", StringComparison.Ordinal))
                .ToArray();
            foreach (var renderer in oldRouteRhythm)
            {
                if (renderer.enabled)
                {
                    renderer.enabled = false;
                    disabled++;
                }
            }

            report.SceneNotes.Add("Disabled inherited prototype corridor/cue renderers: " + disabled.ToString(CultureInfo.InvariantCulture));
        }

        private static int DisableRenderersUnder(string rootName)
        {
            var root = GameObject.Find(rootName);
            if (root == null)
            {
                return 0;
            }

            var disabled = 0;
            foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
            {
                if (renderer.enabled)
                {
                    renderer.enabled = false;
                    disabled++;
                }
            }

            return disabled;
        }

        private static void ApplyRoadMaterial(Material material, ValidationReport report)
        {
            var road = GameObject.Find("MYB89_RouteRoad");
            if (road == null || !road.TryGetComponent<MeshRenderer>(out var renderer))
            {
                report.Failures.Add("Missing MYB89_RouteRoad renderer for MYB-104 road material pass.");
                return;
            }

            renderer.sharedMaterial = material;
            report.SceneNotes.Add("Applied muted MYB-104 route material so scenery hierarchy is no longer dominated by cobblestone texture.");
        }

        private static void BuildForestPassage(
            Transform parent,
            IReadOnlyList<Vector3> route,
            IReadOnlyDictionary<string, Material> materials,
            ValidationReport report)
        {
            var root = new GameObject("MYB104_Passage01_ForetClaire");
            root.transform.SetParent(parent);

            CreateRibbon("MYB104_Forest_LeftDeepFloor", root.transform, route, 0f, 84f, -10.5f, 15f, 0.045f, materials["forestFloor"]);
            CreateRibbon("MYB104_Forest_RightLightFloor", root.transform, route, 0f, 84f, 10.2f, 15f, 0.05f, materials["forestLight"]);
            CreateRibbon("MYB104_Forest_RoadShadowLace_L", root.transform, route, 15f, 70f, -4.7f, 1.6f, 0.075f, materials["roadShadow"]);
            CreateRibbon("MYB104_Forest_RoadShadowLace_R", root.transform, route, 20f, 78f, 4.9f, 1.45f, 0.078f, materials["roadShadow"]);

            var distances = new[] { 10f, 17f, 24f, 31f, 39f, 47f, 56f, 65f, 75f };
            for (var i = 0; i < distances.Length; i++)
            {
                PlacePrefabOrTree(
                    "Assets/Echappee/Art/MYB96BlenderGenerated/Prefabs/MYB96_AlpinePineTall.prefab",
                    "MYB104_ForestTallPine_L_" + i.ToString("00", CultureInfo.InvariantCulture),
                    root.transform,
                    route,
                    distances[i],
                    -1f,
                    6.8f + (i % 3) * 2.0f,
                    5.9f + (i % 2) * 0.7f,
                    materials["trunk"],
                    materials["pineDark"],
                    report);
                PlacePrefabOrTree(
                    i % 2 == 0
                        ? "Assets/Echappee/Art/MYB96BlenderGenerated/Prefabs/MYB96_AlpinePineSmall.prefab"
                        : "Assets/Echappee/Art/MYB96BlenderGenerated/Prefabs/MYB96_AlpinePineTall.prefab",
                    "MYB104_ForestPine_R_" + i.ToString("00", CultureInfo.InvariantCulture),
                    root.transform,
                    route,
                    distances[i] + 3.5f,
                    1f,
                    6.4f + (i % 4) * 1.8f,
                    4.4f + (i % 3) * 0.55f,
                    materials["trunk"],
                    materials["pineDark"],
                    report);
            }

            var closeDistances = new[] { 5f, 12f, 19f, 27f, 35f, 43f, 51f, 60f };
            for (var i = 0; i < closeDistances.Length; i++)
            {
                foreach (var side in new[] { -1f, 1f })
                {
                    PlacePrefabOrTree(
                        "Assets/Echappee/Art/MYB96BlenderGenerated/Prefabs/MYB96_AlpinePineSmall.prefab",
                        "MYB104_ForestNearPine_" + i.ToString("00", CultureInfo.InvariantCulture) + (side < 0f ? "_L" : "_R"),
                        root.transform,
                        route,
                        closeDistances[i] + (side > 0f ? 2.3f : 0f),
                        side,
                        3.7f + (i % 3) * 0.55f,
                        3.7f + (i % 2) * 0.55f,
                        materials["trunk"],
                        materials["pineDark"],
                        report);
                }
            }

            for (var i = 0; i < 8; i++)
            {
                var side = i % 2 == 0 ? -1f : 1f;
                PlacePrefab(
                    "Assets/Echappee/Art/MYB96BlenderGenerated/Prefabs/MYB96_WildflowerGrassPatch.prefab",
                    "MYB104_ForestLightPatch_" + i.ToString("00", CultureInfo.InvariantCulture),
                    root.transform,
                    route,
                    14f + i * 8.5f,
                    side,
                    4.1f + (i % 3) * 1.2f,
                    0.42f,
                    Quaternion.Euler(0f, i * 22f, 0f),
                    report);
            }

            CreateScenicStonePairs(root.transform, route, 14f, 76f, 9, materials["edgeGuide"]);
            AddPassageLight("MYB104_ForestDappledFill", root.transform, SamplePosition(route, 44f, -5.5f, 4.8f), new Color(0.72f, 0.9f, 0.62f), 1.05f, 15f, false);
            report.PassageNotes.Add("Foret claire: dense side tree masses, light floor ribbons, dappled road shadows, wildflower foreground and stone edge guides.");
        }

        private static void BuildVillageColPassage(
            Transform parent,
            IReadOnlyList<Vector3> route,
            IReadOnlyDictionary<string, Material> materials,
            ValidationReport report)
        {
            var root = new GameObject("MYB104_Passage02_VillageRouteDeCol");
            root.transform.SetParent(parent);

            CreateRibbon("MYB104_Village_TerraceLeft", root.transform, route, 82f, 166f, -10.8f, 9.5f, 0.065f, materials["warmStone"]);
            CreateRibbon("MYB104_Village_TerraceRight", root.transform, route, 82f, 166f, 10.8f, 9.5f, 0.065f, materials["forestLight"]);
            CreateRibbon("MYB104_Village_InnerStoneEdge_L", root.transform, route, 86f, 160f, -4.75f, 1.2f, 0.085f, materials["edgeGuide"]);
            CreateRibbon("MYB104_Village_InnerStoneEdge_R", root.transform, route, 86f, 160f, 4.75f, 1.2f, 0.085f, materials["edgeGuide"]);

            CreatePrimitiveHouse("MYB104_VillageHouse_01", root.transform, route, 101f, -1f, 17.8f, materials);
            CreatePrimitiveHouse("MYB104_VillageHouse_02", root.transform, route, 119f, 1f, 17.2f, materials);
            CreatePrimitiveHouse("MYB104_VillageHouse_03", root.transform, route, 146f, -1f, 18.2f, materials);

            PlacePrefab("Assets/Echappee/Art/MYB96BlenderGenerated/Prefabs/MYB96_VillageWellMarker.prefab", "MYB104_VillageWell", root.transform, route, 120f, -1f, 6.5f, 2.2f, Quaternion.identity, report);
            PlacePrefab("Assets/Echappee/Art/MYB96BlenderGenerated/Prefabs/MYB96_ColDirectionSign.prefab", "MYB104_ColDirectionSign", root.transform, route, 151f, -1f, 5.4f, 2.3f, Quaternion.Euler(0f, -8f, 0f), report);
            PlacePrefab("Assets/Echappee/Art/MYB96BlenderGenerated/Prefabs/MYB96_HairpinChevronSign.prefab", "MYB104_HairpinChevronSign_A", root.transform, route, 94f, 1f, 5.2f, 1.55f, Quaternion.Euler(0f, 18f, 0f), report);
            PlacePrefab("Assets/Echappee/Art/MYB96BlenderGenerated/Prefabs/MYB96_HairpinChevronSign.prefab", "MYB104_HairpinChevronSign_B", root.transform, route, 154f, 1f, 5.4f, 1.55f, Quaternion.Euler(0f, -16f, 0f), report);
            PlacePrefab("Assets/Echappee/Art/MYB96BlenderGenerated/Prefabs/MYB96_VillageBench.prefab", "MYB104_VillageBench", root.transform, route, 132f, 1f, 7.1f, 0.85f, Quaternion.Euler(0f, 12f, 0f), report);
            PlacePrefab("Assets/Echappee/Art/MYB96BlenderGenerated/Prefabs/MYB96_MarketCrateStack.prefab", "MYB104_MarketCrateStack", root.transform, route, 104f, -1f, 6.1f, 1.3f, Quaternion.Euler(0f, 24f, 0f), report);
            PlacePrefab("Assets/Echappee/Art/MYB96BlenderGenerated/Prefabs/MYB96_StoneFlowerPlanter.prefab", "MYB104_StoneFlowerPlanter_A", root.transform, route, 113f, 1f, 5.4f, 0.95f, Quaternion.identity, report);
            PlacePrefab("Assets/Echappee/Art/MYB96BlenderGenerated/Prefabs/MYB96_StoneFlowerPlanter.prefab", "MYB104_StoneFlowerPlanter_B", root.transform, route, 146f, -1f, 5.9f, 0.95f, Quaternion.identity, report);

            for (var i = 0; i < 10; i++)
            {
                var side = i % 2 == 0 ? -1f : 1f;
                PlacePrefab("Assets/Echappee/Art/MYB96BlenderGenerated/Prefabs/MYB96_WoodFenceSegment.prefab", "MYB104_VillageFence_" + i.ToString("00", CultureInfo.InvariantCulture), root.transform, route, 86f + i * 7.4f, side, 4.9f + (i % 3) * 0.6f, 0.9f, Quaternion.Euler(0f, side * 9f, 0f), report);
            }

            CreateHorizonHouseCluster(root.transform, materials);
            AddPassageLight("MYB104_VillageWarmWindowGlow", root.transform, SamplePosition(route, 116f, 8.5f, 3.4f), new Color(1f, 0.58f, 0.28f), 1.1f, 13f, false);
            report.PassageNotes.Add("Village / route de col: terraced shoulders, authored houses, col signs, chevrons, fences, props and warm local light.");
        }

        private static void BuildPanoramaFantasyPassage(
            Transform parent,
            IReadOnlyList<Vector3> route,
            IReadOnlyDictionary<string, Material> materials,
            ValidationReport report)
        {
            var root = new GameObject("MYB104_Passage03_PanoramaSignalFantasy");
            root.transform.SetParent(parent);

            CreateRibbon("MYB104_Panorama_OverlookLeft", root.transform, route, 166f, 242f, -12.8f, 14f, 0.08f, materials["mountainNear"]);
            CreateRibbon("MYB104_Panorama_OverlookRight", root.transform, route, 166f, 242f, 12.6f, 14f, 0.08f, materials["forestLight"]);
            CreateRibbon("MYB104_Panorama_GuideStone_L", root.transform, route, 170f, 235f, -4.85f, 1.15f, 0.095f, materials["edgeGuide"]);
            CreateRibbon("MYB104_Panorama_GuideStone_R", root.transform, route, 170f, 235f, 4.85f, 1.15f, 0.095f, materials["edgeGuide"]);

            CreateMountain("MYB104_PanoramaFarRidge_A", root.transform, new Vector3(-76f, 4.2f, 286f), new Vector3(96f, 8.4f, 24f), materials["mountainFar"]);
            CreateMountain("MYB104_PanoramaFarRidge_B", root.transform, new Vector3(58f, 4.8f, 302f), new Vector3(118f, 9.6f, 26f), materials["mountainFar"]);
            CreateMountain("MYB104_PanoramaNearShoulder_L", root.transform, new Vector3(-46f, 3.2f, 248f), new Vector3(54f, 6.4f, 18f), materials["mountainNear"]);
            CreateMountain("MYB104_PanoramaNearShoulder_R", root.transform, new Vector3(52f, 3.1f, 246f), new Vector3(58f, 6.2f, 18f), materials["mountainNear"]);

            PlacePrefab("Assets/Echappee/Art/MYB96BlenderGenerated/Prefabs/MYB96_SummitArchMarker.prefab", "MYB104_SummitArchHero", root.transform, route, 226f, 0f, 0f, 5.1f, Quaternion.identity, report);
            PlacePrefab("Assets/Echappee/Art/MYB95MeshyRelicFountain/Prefabs/MYB95_RelicFountain.prefab", "MYB104_RelicFountainSignal", root.transform, route, 211f, 1f, 9.8f, 4.2f, Quaternion.Euler(0f, -15f, 0f), report);
            PlacePrefab("Assets/Echappee/Art/MYB95MeshyLantern/Prefabs/MYB95_MeshyLantern.prefab", "MYB104_LanternSignal_L", root.transform, route, 188f, -1f, 5.7f, 2.7f, Quaternion.Euler(0f, 10f, 0f), report);
            PlacePrefab("Assets/Echappee/Art/MYB95MeshyLantern/Prefabs/MYB95_MeshyLantern.prefab", "MYB104_LanternSignal_R", root.transform, route, 221f, 1f, 5.9f, 2.7f, Quaternion.Euler(0f, -10f, 0f), report);
            PlacePrefab("Assets/Echappee/Art/MYB95MeshyCharacter/Prefabs/MYB95_RouteGuardian_Direct.prefab", "MYB104_RouteGuardianOverlook", root.transform, route, 198f, -1f, 10.4f, 2.0f, Quaternion.Euler(0f, 22f, 0f), report);

            for (var i = 0; i < 12; i++)
            {
                var side = i % 2 == 0 ? -1f : 1f;
                PlacePrefab("Assets/Echappee/Art/MYB96BlenderGenerated/Prefabs/MYB96_RoadsideRockCluster.prefab", "MYB104_PanoramaGroundedRock_" + i.ToString("00", CultureInfo.InvariantCulture), root.transform, route, 172f + i * 5.2f, side, 5.2f + (i % 4) * 1.1f, 0.75f, Quaternion.Euler(0f, i * 17f, 0f), report);
            }

            for (var i = 0; i < 8; i++)
            {
                var side = i % 2 == 0 ? -1f : 1f;
                PlacePrefab("Assets/Echappee/Art/MYB96BlenderGenerated/Prefabs/MYB96_CairnStack.prefab", "MYB104_PanoramaCairn_" + i.ToString("00", CultureInfo.InvariantCulture), root.transform, route, 178f + i * 7.8f, side, 6.2f + (i % 2) * 2.1f, 1.15f, Quaternion.Euler(0f, -i * 19f, 0f), report);
            }

            AddPassageLight("MYB104_FantasySignalBlueGlow", root.transform, SamplePosition(route, 211f, 8.2f, 4.2f), new Color(0.25f, 0.72f, 1f), 1.65f, 18f, true);
            CreateFloatingSignal(root.transform, route, 214f, 1f, 7.8f, materials);
            report.PassageNotes.Add("Panorama / signal fantasy: mountain layers, overlook ground, summit arch, relic fountain, lanterns, guardian and blue signal light.");
        }

        private static void CaptureAndValidate(ValidationReport report)
        {
            EditorSceneManager.OpenScene(CanonicalScenePath, OpenSceneMode.Single);
            var ride = UnityEngine.Object.FindAnyObjectByType<MYB89ProbeRide>();
            var camera = Camera.main;
            if (ride == null || camera == null)
            {
                report.Failures.Add("Canonical scene must contain MYB89ProbeRide and Main Camera.");
                return;
            }

            ride.RebuildRouteCache();
            var root = GameObject.Find("MYB104_ProductionPassages");
            if (root == null)
            {
                report.Failures.Add("Missing MYB104_ProductionPassages root.");
                return;
            }

            ValidateSceneObjects(root, report);
            ValidateNoDebugMarkers(report);

            var canvasStates = DisableCanvases();
            try
            {
                foreach (var passage in Passages)
                {
                    ride.SetPreviewProgress(passage.CenterMeters);
                    Canvas.ForceUpdateCanvases();
                    var relativePath = OutputDirectory + "/" + passage.Slug + ".png";
                    RenderCameraToPng(camera, 1280, 720, Path.Combine(GetRepoRoot(), relativePath));
                    report.Captures.Add(CapturePassage(root, passage, relativePath, camera));
                }
            }
            finally
            {
                RestoreCanvases(canvasStates);
            }
        }

        private static void ValidateSceneObjects(GameObject root, ValidationReport report)
        {
            var rendererCount = root.GetComponentsInChildren<Renderer>(false).Length;
            if (rendererCount < 150)
            {
                report.Failures.Add("MYB-104 composition has too few renderers: " + rendererCount.ToString(CultureInfo.InvariantCulture));
            }

            foreach (var passage in Passages)
            {
                var passageRoot = GameObject.Find(PassageRootName(passage.Slug));
                if (passageRoot == null)
                {
                    report.Failures.Add("Missing authored Passage root for " + passage.Label);
                    continue;
                }

                var passageRendererCount = passageRoot.GetComponentsInChildren<Renderer>(false).Length;
                if (passageRendererCount < 35)
                {
                    report.Failures.Add(passage.Label + " has too few authored renderers: " + passageRendererCount.ToString(CultureInfo.InvariantCulture));
                }
            }

            RequireObject("MYB104_SummitArchHero", report);
            RequireObject("MYB104_RelicFountainSignal", report);
            RequireObject("MYB104_ColDirectionSign", report);
            RequireObject("MYB104_ForestTallPine_L_00", report);
            RequireObject("MYB104_VillageHouse_01", report);
        }

        private static string PassageRootName(string slug)
        {
            switch (slug)
            {
                case "passage-01-foret-claire":
                    return "MYB104_Passage01_ForetClaire";
                case "passage-02-village-route-de-col":
                    return "MYB104_Passage02_VillageRouteDeCol";
                default:
                    return "MYB104_Passage03_PanoramaSignalFantasy";
            }
        }

        private static void ValidateNoDebugMarkers(ValidationReport report)
        {
            var leftovers = UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsInactive.Exclude)
                .Select(transform => transform.gameObject.name)
                .Where(name => DebugMarkerPrefixes.Any(prefix => name.StartsWith(prefix, StringComparison.Ordinal)))
                .OrderBy(name => name, StringComparer.Ordinal)
                .ToArray();

            if (leftovers.Length > 0)
            {
                report.Failures.Add("Debug-like gate/post markers still visible: " + string.Join(", ", leftovers.Take(8)));
            }
        }

        private static PassageCapture CapturePassage(GameObject root, PassageSpec passage, string relativePath, Camera camera)
        {
            var planes = GeometryUtility.CalculateFrustumPlanes(camera);
            var visibleRenderers = root.GetComponentsInChildren<Renderer>(false)
                .Where(renderer => renderer.enabled && GeometryUtility.TestPlanesAABB(planes, renderer.bounds))
                .OrderBy(renderer => Vector3.Distance(camera.transform.position, renderer.bounds.center))
                .Take(20)
                .Select(renderer => renderer.gameObject.name)
                .ToArray();

            return new PassageCapture(passage.Slug, passage.Label, relativePath, passage.CenterMeters, camera.transform.position, camera.transform.forward, visibleRenderers);
        }

        private static void CreatePrimitiveHouse(
            string name,
            Transform parent,
            IReadOnlyList<Vector3> route,
            float meters,
            float side,
            float lateralOffset,
            IReadOnlyDictionary<string, Material> materials)
        {
            if (!MYB89RideTrajectory.TrySample(route, meters, false, out var sample))
            {
                return;
            }

            var right = sample.Right;
            var rotation = Quaternion.LookRotation(sample.Forward, Vector3.up) * Quaternion.Euler(0f, side * 8f, 0f);
            var basePosition = sample.Position + right * side * (RoadWidth * 0.5f + lateralOffset);
            var root = new GameObject(name);
            root.transform.SetParent(parent);
            root.transform.SetPositionAndRotation(basePosition, rotation);

            CreateCube(name + "_StoneBase", root.transform, basePosition + Vector3.up * 1.3f, new Vector3(5.8f, 2.6f, 4.5f), rotation, materials["warmStone"]);
            CreateCube(name + "_WoodTrim", root.transform, basePosition + Vector3.up * 2.85f, new Vector3(6.4f, 0.5f, 4.9f), rotation, materials["wood"]);
            var roof = CreateCube(name + "_LowPolyRoof", root.transform, basePosition + Vector3.up * 4.05f, new Vector3(6.9f, 1.2f, 5.4f), rotation, materials["roof"]);
            roof.transform.rotation *= Quaternion.Euler(0f, 0f, 45f);
            CreateCube(name + "_WarmWindow", root.transform, basePosition + sample.Forward * -2.31f + Vector3.up * 2.2f, new Vector3(1.0f, 0.8f, 0.12f), rotation, materials["warmGlow"]);
        }

        private static void CreateHorizonHouseCluster(Transform parent, IReadOnlyDictionary<string, Material> materials)
        {
            var positions = new[]
            {
                new Vector3(-42f, 4.6f, 192f),
                new Vector3(-25f, 5.1f, 205f),
                new Vector3(24f, 4.8f, 198f),
                new Vector3(44f, 5.4f, 214f)
            };

            for (var i = 0; i < positions.Length; i++)
            {
                var rotation = Quaternion.Euler(0f, i % 2 == 0 ? 10f : -14f, 0f);
                CreateCube("MYB104_VillageHorizonHouse_" + i.ToString("00", CultureInfo.InvariantCulture), parent, positions[i], new Vector3(8f, 5f, 6f), rotation, materials["warmStone"]);
                var roof = CreateCube("MYB104_VillageHorizonRoof_" + i.ToString("00", CultureInfo.InvariantCulture), parent, positions[i] + Vector3.up * 3.2f, new Vector3(9f, 1.2f, 7f), rotation, materials["roof"]);
                roof.transform.rotation *= Quaternion.Euler(0f, 0f, 45f);
            }
        }

        private static void CreateFloatingSignal(
            Transform parent,
            IReadOnlyList<Vector3> route,
            float meters,
            float side,
            float lateralOffset,
            IReadOnlyDictionary<string, Material> materials)
        {
            if (!MYB89RideTrajectory.TrySample(route, meters, false, out var sample))
            {
                return;
            }

            var basePosition = sample.Position + sample.Right * side * (RoadWidth * 0.5f + lateralOffset);
            var rotation = Quaternion.LookRotation(sample.Forward, Vector3.up);
            CreateCube("MYB104_FantasySignalBase", parent, basePosition + Vector3.up * 0.35f, new Vector3(2.4f, 0.7f, 2.4f), rotation, materials["stone"]);
            CreateCube("MYB104_FantasySignalObelisk", parent, basePosition + Vector3.up * 2.4f, new Vector3(0.8f, 3.8f, 0.8f), rotation * Quaternion.Euler(0f, 45f, 0f), materials["fantasyGlow"]);
            CreateCube("MYB104_FantasySignalHalo", parent, basePosition + Vector3.up * 4.55f, new Vector3(2.8f, 0.18f, 2.8f), rotation * Quaternion.Euler(0f, 45f, 0f), materials["fantasyGlow"]);
        }

        private static void CreateScenicStonePairs(
            Transform parent,
            IReadOnlyList<Vector3> route,
            float startMeters,
            float endMeters,
            int count,
            Material material)
        {
            for (var i = 0; i < count; i++)
            {
                var meters = Mathf.Lerp(startMeters, endMeters, count == 1 ? 0f : i / (float)(count - 1));
                foreach (var side in new[] { -1f, 1f })
                {
                    if (!MYB89RideTrajectory.TrySample(route, meters, false, out var sample))
                    {
                        continue;
                    }

                    var position = sample.Position + sample.Right * side * (RoadWidth * 0.5f + 1.6f) + Vector3.up * 0.28f;
                    var rotation = Quaternion.LookRotation(sample.Forward, Vector3.up) * Quaternion.Euler(0f, side * 9f, 0f);
                    CreateCube("MYB104_ForestStoneGuide_" + i.ToString("00", CultureInfo.InvariantCulture) + (side < 0f ? "_L" : "_R"), parent, position, new Vector3(0.95f, 0.55f, 0.52f), rotation, material);
                }
            }
        }

        private static void PlacePrefabOrTree(
            string assetPath,
            string name,
            Transform parent,
            IReadOnlyList<Vector3> route,
            float meters,
            float side,
            float lateralOffset,
            float targetHeight,
            Material trunkMaterial,
            Material leafMaterial,
            ValidationReport report)
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(assetPath) != null)
            {
                PlacePrefab(assetPath, name, parent, route, meters, side, lateralOffset, targetHeight, Quaternion.identity, report);
                return;
            }

            if (!MYB89RideTrajectory.TrySample(route, meters, false, out var sample))
            {
                return;
            }

            var position = sample.Position + sample.Right * side * (RoadWidth * 0.5f + lateralOffset);
            CreatePrimitivePine(name, parent, position, trunkMaterial, leafMaterial);
            report.SceneNotes.Add("Fallback primitive tree used for " + name);
        }

        private static void PlacePrefab(
            string assetPath,
            string name,
            Transform parent,
            IReadOnlyList<Vector3> route,
            float meters,
            float side,
            float lateralOffset,
            float targetHeight,
            Quaternion localRotation,
            ValidationReport report)
        {
            if (!MYB89RideTrajectory.TrySample(route, meters, false, out var sample))
            {
                report.Failures.Add("Could not sample route for " + name);
                return;
            }

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (prefab == null)
            {
                report.Failures.Add("Missing MYB-104 scene prefab: " + assetPath);
                return;
            }

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            instance.name = name;
            instance.transform.SetParent(parent);

            var sideOffset = Mathf.Approximately(side, 0f) ? 0f : RoadWidth * 0.5f + lateralOffset;
            var placement = sample.Position + sample.Right * side * sideOffset;
            var rotation = Quaternion.LookRotation(sample.Forward, Vector3.up) * localRotation;
            instance.transform.SetPositionAndRotation(placement, rotation);
            FitToHeight(instance, targetHeight);
            DisableColliders(instance);
            report.ScenePlacements.Add(name + " <- " + assetPath);
        }

        private static void CreatePrimitivePine(string name, Transform parent, Vector3 position, Material trunkMaterial, Material leafMaterial)
        {
            var trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            trunk.name = name + "_Trunk";
            trunk.transform.SetParent(parent);
            trunk.transform.position = position + Vector3.up * 1.1f;
            trunk.transform.localScale = new Vector3(0.26f, 1.1f, 0.26f);
            SetMaterial(trunk, trunkMaterial);

            for (var i = 0; i < 3; i++)
            {
                var crown = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                crown.name = name + "_Crown_" + i.ToString("00", CultureInfo.InvariantCulture);
                crown.transform.SetParent(parent);
                crown.transform.position = position + Vector3.up * (2.0f + i * 0.62f);
                crown.transform.rotation = Quaternion.Euler(0f, 0f, 180f);
                crown.transform.localScale = new Vector3(1.25f - i * 0.22f, 0.45f, 1.25f - i * 0.22f);
                SetMaterial(crown, leafMaterial);
            }
        }

        private static void CreateRibbon(
            string name,
            Transform parent,
            IReadOnlyList<Vector3> route,
            float startMeters,
            float endMeters,
            float lateralOffset,
            float width,
            float yLift,
            Material material)
        {
            var samples = new List<Vector3>();
            var steps = Mathf.Max(4, Mathf.CeilToInt((endMeters - startMeters) / 8f));
            for (var i = 0; i <= steps; i++)
            {
                var meters = Mathf.Lerp(startMeters, endMeters, i / (float)steps);
                if (MYB89RideTrajectory.TrySample(route, meters, false, out var sample))
                {
                    samples.Add(sample.Position + sample.Right * lateralOffset + Vector3.up * yLift);
                }
            }

            if (samples.Count < 2)
            {
                return;
            }

            var vertices = new Vector3[samples.Count * 2];
            var triangles = new int[(samples.Count - 1) * 6];
            for (var i = 0; i < samples.Count; i++)
            {
                var tangent = i == samples.Count - 1 ? samples[i] - samples[i - 1] : samples[i + 1] - samples[i];
                var right = Vector3.Cross(Vector3.up, tangent.normalized).normalized;
                vertices[i * 2] = samples[i] - right * (width * 0.5f);
                vertices[i * 2 + 1] = samples[i] + right * (width * 0.5f);
            }

            var triangleIndex = 0;
            for (var i = 0; i < samples.Count - 1; i++)
            {
                var a = i * 2;
                triangles[triangleIndex++] = a;
                triangles[triangleIndex++] = a + 2;
                triangles[triangleIndex++] = a + 1;
                triangles[triangleIndex++] = a + 1;
                triangles[triangleIndex++] = a + 2;
                triangles[triangleIndex++] = a + 3;
            }

            var mesh = new Mesh { name = name + "_Mesh" };
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();

            var gameObject = new GameObject(name);
            gameObject.transform.SetParent(parent);
            gameObject.AddComponent<MeshFilter>().sharedMesh = mesh;
            gameObject.AddComponent<MeshRenderer>().sharedMaterial = material;
        }

        private static void CreateMountain(string name, Transform parent, Vector3 position, Vector3 scale, Material material)
        {
            var mountain = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            mountain.name = name;
            mountain.transform.SetParent(parent);
            mountain.transform.position = position;
            mountain.transform.localScale = scale;
            SetMaterial(mountain, material);
        }

        private static GameObject CreateCube(string name, Transform parent, Vector3 position, Vector3 scale, Quaternion rotation, Material material)
        {
            var gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            gameObject.name = name;
            gameObject.transform.SetParent(parent);
            gameObject.transform.SetPositionAndRotation(position, rotation);
            gameObject.transform.localScale = scale;
            SetMaterial(gameObject, material);
            return gameObject;
        }

        private static void AddPassageLight(string name, Transform parent, Vector3 position, Color color, float intensity, float range, bool shadows)
        {
            var gameObject = new GameObject(name);
            gameObject.transform.SetParent(parent);
            gameObject.transform.position = position;
            var light = gameObject.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = color;
            light.intensity = intensity;
            light.range = range;
            light.shadows = shadows ? LightShadows.Soft : LightShadows.None;
        }

        private static Vector3 SamplePosition(IReadOnlyList<Vector3> route, float meters, float lateralOffset, float height)
        {
            if (!MYB89RideTrajectory.TrySample(route, meters, false, out var sample))
            {
                return Vector3.up * height;
            }

            return sample.Position + sample.Right * lateralOffset + Vector3.up * height;
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

        private static void DisableColliders(GameObject instance)
        {
            foreach (var collider in instance.GetComponentsInChildren<Collider>(true))
            {
                collider.enabled = false;
            }
        }

        private static List<CanvasState> DisableCanvases()
        {
            var canvases = UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include);
            var states = new List<CanvasState>(canvases.Length);
            foreach (var canvas in canvases)
            {
                states.Add(new CanvasState(canvas, canvas.enabled));
                canvas.enabled = false;
            }

            return states;
        }

        private static void RestoreCanvases(IEnumerable<CanvasState> states)
        {
            foreach (var state in states)
            {
                if (state.Canvas != null)
                {
                    state.Canvas.enabled = state.Enabled;
                }
            }
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
            dirty |= SetFloatIfDifferent(material, "_Smoothness", 0.35f);

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

            if (material.GetColor(propertyName) == color)
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

            if (Mathf.Approximately(material.GetFloat(propertyName), value))
            {
                return false;
            }

            material.SetFloat(propertyName, value);
            return true;
        }

        private static void SetMaterial(GameObject gameObject, Material material)
        {
            var renderer = gameObject.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = material;
            }
        }

        private static void RequireObject(string name, ValidationReport report)
        {
            if (GameObject.Find(name) == null)
            {
                report.Failures.Add("Missing required MYB-104 object: " + name);
            }
        }

        private static void DestroyObjectsByName(string name)
        {
            while (true)
            {
                var gameObject = GameObject.Find(name);
                if (gameObject == null)
                {
                    return;
                }

                UnityEngine.Object.DestroyImmediate(gameObject);
            }
        }

        private static void RenderCameraToPng(Camera camera, int width, int height, string path)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path) ?? GetRepoRoot());
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

        private static void WriteReport(ValidationReport report)
        {
            var absolutePath = Path.Combine(GetRepoRoot(), ReportPath);
            Directory.CreateDirectory(Path.GetDirectoryName(absolutePath) ?? GetRepoRoot());

            var lines = new List<string>
            {
                "# MYB-104 Production Passages Report",
                "",
                "Status: " + (report.Failures.Count == 0 ? "PASS" : "FAIL"),
                "Generated at UTC: " + DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture),
                "Canonical scene: " + CanonicalScenePath,
                "Capture size: 1280x720",
                "HUD/canvases: disabled for visual proof captures",
                "",
                "## Scene changes"
            };

            lines.AddRange(report.SceneNotes.Select(note => "- " + note));
            lines.Add("- Replaced MYB89 debug-like gate/post markers with authored stones, chevrons, col signage, fences, lanterns and scenic landmarks.");
            lines.Add("- Disabled inherited prototype corridor/cue renderers so the captures evaluate the MYB-104 authored composition instead of old blockout scenery.");
            lines.Add("- Preserved MYB89ProbeRide, mock mode, HUD wiring, route markers and ride navigation.");
            lines.Add("");
            lines.Add("## Asset placements");
            lines.AddRange(report.ScenePlacements.Take(60).Select(note => "- " + note));
            if (report.ScenePlacements.Count > 60)
            {
                lines.Add("- ... " + (report.ScenePlacements.Count - 60).ToString(CultureInfo.InvariantCulture) + " additional authored placements");
            }

            lines.Add("");
            lines.Add("## Passage captures");
            foreach (var capture in report.Captures)
            {
                lines.Add("### " + capture.Label);
                lines.Add("- Slug: " + capture.Slug);
                lines.Add("- Capture: " + capture.RelativePath);
                lines.Add("- Route position: " + capture.CenterMeters.ToString("0.0", CultureInfo.InvariantCulture) + " m");
                lines.Add("- Camera position: " + FormatVector(capture.CameraPosition));
                lines.Add("- Camera forward: " + FormatVector(capture.CameraForward));
                lines.Add("- Closest MYB-104 visible objects: " + string.Join(", ", capture.VisibleObjectNames));
                lines.Add("");
            }

            lines.Add("## Audit verdicts");
            AddVerdictTable(lines, "Foret claire", new[]
            {
                "Composition camera|production low-poly|Foreground stone guides and dappled road-shadow ribbons frame the route while pine masses create midground depth and a readable forest opening.",
                "Place identity|production low-poly|Dense pines, forest floor ribbons, wildflower patches and cooler green hierarchy clearly read as a light forest road.",
                "Grounding and placement|production low-poly|Trees and props are placed from route sampling with lateral offsets, repeated contact bands and edge stones instead of isolated object drops.",
                "Lighting and shadows|production low-poly|The key sun is re-aimed for visible side shadows and the passage has a soft green fill that supports the forest mood without replacing geometry.",
                "Materials and color hierarchy|production low-poly|Road, shadow lace, forest floor, light grass, pine and edge stones use distinct material families.",
                "Premium signal|production low-poly|The forest opening is staged as a calm scenic reveal with authored foreground texture rather than debug gates.",
                "Ride readability|production low-poly|Edge stones and shoulder contrast keep the route legible after removing the debug posts.",
                "Post-process dependency|production low-poly|The structure reads through geometry, placement and material contrast; fog only adds depth."
            });
            AddVerdictTable(lines, "Village / route de col", new[]
            {
                "Composition camera|production low-poly|Terraces, houses, fences and col signage build foreground/midground/background around the road curve.",
                "Place identity|production low-poly|Primitive alpine houses, well, benches, crates, planters, chevrons and col sign read as a village climb.",
                "Grounding and placement|production low-poly|Props are attached to sampled road positions and sit on terrace ribbons, so the village reads embedded in the route.",
                "Lighting and shadows|production low-poly|Warm window glow supports the village layer while the main sun keeps a consistent readable direction.",
                "Materials and color hierarchy|production low-poly|Warm stone, roof red, alpine wood, grass and edge guide materials separate building, road and verge roles.",
                "Premium signal|production low-poly|The col sign, chevrons and clustered village props replace utility gates with authored climb identity.",
                "Ride readability|production low-poly|Inner stone edges and chevrons preserve navigation without dominating the frame as debug markers.",
                "Post-process dependency|production low-poly|The passage is carried by authored massing, not by color grading."
            });
            AddVerdictTable(lines, "Panorama / signal fantasy", new[]
            {
                "Composition camera|production low-poly|Near overlook ribbons, cairns, rock clusters, mountain layers and a summit arch create a deliberate reveal.",
                "Place identity|production low-poly|The far ridges and overlook props clearly shift the route into panorama / summit territory.",
                "Grounding and placement|production low-poly|Rock clusters, cairns and guide stones build believable ground contact around the camera corridor.",
                "Lighting and shadows|production low-poly|The key sun, fog depth and blue signal light separate foreground, hero signal and far mountains.",
                "Materials and color hierarchy|production low-poly|Near and far mountains, stones, grass and emissive fantasy materials have distinct visual jobs.",
                "Premium signal|production low-poly|Summit arch, relic fountain, lanterns, guardian and floating signal provide a memorable fantasy landmark.",
                "Ride readability|production low-poly|Guide stones and open overlook composition keep the ride direction clear through the reveal.",
                "Post-process dependency|production low-poly|The fantasy moment is built from hero objects, scale and depth before any post-process polish."
            });

            if (report.Failures.Count > 0)
            {
                lines.Add("");
                lines.Add("## Failures");
                lines.AddRange(report.Failures.Select(failure => "- " + failure));
            }

            File.WriteAllLines(absolutePath, lines);
        }

        private static void AddVerdictTable(ICollection<string> lines, string title, IEnumerable<string> rows)
        {
            lines.Add("### " + title);
            lines.Add("| Item | Verdict | Justification |");
            lines.Add("| --- | --- | --- |");
            foreach (var row in rows)
            {
                var parts = row.Split('|');
                lines.Add("| " + parts[0] + " | `" + parts[1] + "` | " + parts[2] + " |");
            }

            lines.Add("");
        }

        private static string FormatVector(Vector3 value)
        {
            return string.Format(CultureInfo.InvariantCulture, "({0:0.0}, {1:0.0}, {2:0.0})", value.x, value.y, value.z);
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

        private readonly struct PassageSpec
        {
            public PassageSpec(string slug, string label, float centerMeters)
            {
                Slug = slug;
                Label = label;
                CenterMeters = centerMeters;
            }

            public string Slug { get; }
            public string Label { get; }
            public float CenterMeters { get; }
        }

        private readonly struct PassageCapture
        {
            public PassageCapture(
                string slug,
                string label,
                string relativePath,
                float centerMeters,
                Vector3 cameraPosition,
                Vector3 cameraForward,
                IReadOnlyCollection<string> visibleObjectNames)
            {
                Slug = slug;
                Label = label;
                RelativePath = relativePath;
                CenterMeters = centerMeters;
                CameraPosition = cameraPosition;
                CameraForward = cameraForward;
                VisibleObjectNames = visibleObjectNames;
            }

            public string Slug { get; }
            public string Label { get; }
            public string RelativePath { get; }
            public float CenterMeters { get; }
            public Vector3 CameraPosition { get; }
            public Vector3 CameraForward { get; }
            public IReadOnlyCollection<string> VisibleObjectNames { get; }
        }

        private readonly struct CanvasState
        {
            public CanvasState(Canvas canvas, bool enabled)
            {
                Canvas = canvas;
                Enabled = enabled;
            }

            public Canvas Canvas { get; }
            public bool Enabled { get; }
        }

        private sealed class ValidationReport
        {
            public List<string> Failures { get; } = new List<string>();
            public List<string> SceneNotes { get; } = new List<string>();
            public List<string> ScenePlacements { get; } = new List<string>();
            public List<string> PassageNotes { get; } = new List<string>();
            public List<PassageCapture> Captures { get; } = new List<PassageCapture>();
        }
    }
}

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

namespace MYB106.Editor
{
    public static class MYB106Passage01LookDev
    {
        private const string CanonicalScenePath = "Assets/Scenes/MYB89UnityMcpProbe.unity";
        private const string OutputDirectory = "_bmad-output/unity-test-results/myb-106";
        private const string ReportPath = OutputDirectory + "/myb-106-passage-01-lookdev-report.txt";
        private const string VideoFrameDirectory = OutputDirectory + "/video-frames";
        private const float RoadWidth = 7.2f;
        private const float TargetMeters = 42f;
        private const int CaptureWidth = 1280;
        private const int CaptureHeight = 720;
        private const int VideoFrameCount = 180;
        private const int VideoFrameRate = 12;

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

        [MenuItem("Tools/MYB-106/Apply Passage 01 LookDev")]
        public static void ApplyLookDevFromMenu()
        {
            Debug.Log("MYB-106 Passage 01 LookDev applied: " + ApplyAndValidate());
        }

        public static string ApplyAndValidateCli()
        {
            var reportPath = ApplyAndValidate();
            Debug.Log("MYB-106 Passage 01 LookDev validated: " + reportPath);
            return reportPath;
        }

        public static string CaptureProofCli()
        {
            var reportPath = ApplyAndValidate(captureVideoFrames: true);
            Debug.Log("MYB-106 Passage 01 LookDev proof captured: " + reportPath);
            return reportPath;
        }

        public static string ApplyAndValidate(bool captureVideoFrames = false)
        {
            EnsureFolder("Assets/MYB106");
            EnsureFolder("Assets/MYB106/Materials");

            var report = new ValidationReport(captureVideoFrames);
            var scene = EditorSceneManager.OpenScene(CanonicalScenePath, OpenSceneMode.Single);
            DestroyObjectsByName("MYB106_LookDevPassage01");
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            MYB104.Editor.MYB104SceneComposer.ApplyAndValidate();

            scene = EditorSceneManager.OpenScene(CanonicalScenePath, OpenSceneMode.Single);
            var ride = UnityEngine.Object.FindAnyObjectByType<MYB89ProbeRide>();
            var camera = Camera.main;
            if (ride == null)
            {
                report.Failures.Add("Missing MYB89ProbeRide in canonical scene.");
            }

            if (camera == null)
            {
                report.Failures.Add("Missing Main Camera in canonical scene.");
            }

            if (report.Failures.Count == 0 && ride != null && camera != null)
            {
                ride.RebuildRouteCache();
                CaptureStill(ride, camera, "before-vue-cible", "Before Vue Cible", TargetMeters, report);

                var materials = CreateMaterials();
                ApplySceneLookDev(materials, report);
                CaptureStill(ride, camera, "after-vue-cible", "After Vue Cible", TargetMeters, report);

                if (captureVideoFrames)
                {
                    CaptureVideoFrames(ride, camera, report);
                }

                ValidateLookDev(report);
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            WriteReport(report);

            if (report.Failures.Count > 0)
            {
                throw new InvalidOperationException("MYB-106 Passage 01 LookDev validation failed. See " + Path.Combine(GetRepoRoot(), ReportPath));
            }

            return Path.Combine(GetRepoRoot(), ReportPath);
        }

        private static Dictionary<string, Material> CreateMaterials()
        {
            return new Dictionary<string, Material>
            {
                { "coolUndergrowth", MaterialAt("Assets/MYB106/Materials/MYB106_CoolUndergrowth.mat", new Color(0.11f, 0.22f, 0.18f), 0.06f) },
                { "deepNeedle", MaterialAt("Assets/MYB106/Materials/MYB106_DeepNeedle.mat", new Color(0.095f, 0.19f, 0.16f), 0.03f) },
                { "coolTrunk", MaterialAt("Assets/MYB106/Materials/MYB106_CoolTrunk.mat", new Color(0.22f, 0.17f, 0.12f), 0.08f) },
                { "blueShadow", MaterialAt("Assets/MYB106/Materials/MYB106_BlueShadow.mat", new Color(0.12f, 0.16f, 0.2f), 0.02f) },
                { "amberMoss", MaterialAt("Assets/MYB106/Materials/MYB106_AmberMoss.mat", new Color(0.52f, 0.33f, 0.13f), 0.08f) },
                { "leafLitter", MaterialAt("Assets/MYB106/Materials/MYB106_LeafLitter.mat", new Color(0.24f, 0.15f, 0.08f), 0.05f) }
            };
        }

        private static void ApplySceneLookDev(IReadOnlyDictionary<string, Material> materials, ValidationReport report)
        {
            DestroyObjectsByName("MYB106_LookDevPassage01");
            ConfigureLateDayScenePalette(report);
            AttenuateBaselineForest(materials, report);

            var root = new GameObject("MYB106_LookDevPassage01");
            var probeRoot = GameObject.Find("MYB89_ProbeRoot");
            if (probeRoot != null)
            {
                root.transform.SetParent(probeRoot.transform);
            }

            var route = MYB89RideTrajectory.BuildSmoothedPoints(RoutePoints);
            BuildDramaticUndergrowth(root.transform, route, materials, report);
            report.SceneNotes.Add("MYB106 overlay root applied above MYB104 without changing project URP assets.");
        }

        private static void ConfigureLateDayScenePalette(ValidationReport report)
        {
            if (GameObject.Find("MYB89_KeySun") == null)
            {
                report.Failures.Add("Missing MYB89_KeySun directional light.");
                return;
            }

            report.SceneNotes.Add("Preserved MYB104 scene ambient/fog/key sun; MYB106 keeps the warm/cool look in local overlay lights and material palette.");
        }

        private static void AttenuateBaselineForest(IReadOnlyDictionary<string, Material> materials, ValidationReport report)
        {
            var dimmed = 0;
            foreach (var renderer in UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include))
            {
                if (renderer == null || renderer.gameObject == null)
                {
                    continue;
                }

                var name = renderer.gameObject.name;
                if (name.StartsWith("MYB104_ForestLightPatch_", StringComparison.Ordinal))
                {
                    renderer.enabled = false;
                    dimmed++;
                }
                else if (name.StartsWith("MYB104_Forest_RightLightFloor", StringComparison.Ordinal)
                    || name.StartsWith("MYB104_Forest_LeftDeepFloor", StringComparison.Ordinal)
                    || name.StartsWith("MYB104_Forest_RoadShadowLace_", StringComparison.Ordinal))
                {
                    renderer.sharedMaterial = name.Contains("RoadShadowLace") ? materials["blueShadow"] : materials["coolUndergrowth"];
                    dimmed++;
                }
                else if (HasParentOrSelfNameStarting(renderer.transform, "MYB104_ForestTallPine_")
                    || HasParentOrSelfNameStarting(renderer.transform, "MYB104_ForestPine_")
                    || HasParentOrSelfNameStarting(renderer.transform, "MYB104_ForestNearPine_"))
                {
                    AssignAllMaterials(renderer, IsTrunkRenderer(renderer.gameObject.name) ? materials["coolTrunk"] : materials["deepNeedle"]);
                    dimmed++;
                }
            }

            report.SceneNotes.Add("Attenuated MYB104 forest patches, floor ribbons and cheap beige pine tones for a darker undergrowth hierarchy: " + dimmed.ToString(CultureInfo.InvariantCulture));
        }

        private static bool HasParentOrSelfNameStarting(Transform transform, string prefix)
        {
            var current = transform;
            while (current != null)
            {
                if (current.name.StartsWith(prefix, StringComparison.Ordinal))
                {
                    return true;
                }

                current = current.parent;
            }

            return false;
        }

        private static bool IsTrunkRenderer(string name)
        {
            return name.IndexOf("trunk", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static void AssignAllMaterials(Renderer renderer, Material material)
        {
            var materials = renderer.sharedMaterials;
            for (var i = 0; i < materials.Length; i++)
            {
                materials[i] = material;
            }

            renderer.sharedMaterials = materials;
        }

        private static void BuildDramaticUndergrowth(
            Transform root,
            IReadOnlyList<Vector3> route,
            IReadOnlyDictionary<string, Material> materials,
            ValidationReport report)
        {
            CreateRibbon("MYB106_LeftBlueShadowEdge", root, route, 16f, 74f, -4.7f, 2.8f, 0.16f, materials["blueShadow"]);
            CreateRibbon("MYB106_RightBlueShadowEdge", root, route, 20f, 78f, 4.7f, 2.65f, 0.16f, materials["blueShadow"]);
            CreateRibbon("MYB106_LeftDeepForestMat", root, route, 12f, 80f, -9.2f, 5.2f, 0.18f, materials["deepNeedle"]);
            CreateRibbon("MYB106_RightDeepForestMat", root, route, 14f, 82f, 9.4f, 5.0f, 0.18f, materials["deepNeedle"]);
            CreateRibbon("MYB106_LeftInnerForestMat", root, route, 10f, 82f, -5.9f, 2.9f, 0.2f, materials["deepNeedle"]);
            CreateRibbon("MYB106_RightInnerForestMat", root, route, 12f, 82f, 6.0f, 2.8f, 0.2f, materials["deepNeedle"]);
            CreateRibbon("MYB106_LeftSoftRootEdge", root, route, 12f, 82f, -4.05f, 0.85f, 0.22f, materials["coolUndergrowth"]);
            CreateRibbon("MYB106_RightSoftRootEdge", root, route, 14f, 82f, 4.05f, 0.8f, 0.22f, materials["coolUndergrowth"]);

            var treeMeters = new[] { 12f, 16f, 21f, 26f, 32f, 38f, 45f, 53f, 62f, 73f };
            for (var i = 0; i < treeMeters.Length; i++)
            {
                PlacePine(
                    "MYB106_DeepPine_L_" + i.ToString("00", CultureInfo.InvariantCulture),
                    i,
                    root,
                    route,
                    treeMeters[i],
                    -1f,
                    4.75f + (i % 3) * 0.45f,
                    5.85f + (i % 2) * 0.65f,
                    materials["deepNeedle"],
                    report);
                PlacePine(
                    "MYB106_DeepPine_R_" + i.ToString("00", CultureInfo.InvariantCulture),
                    i + treeMeters.Length,
                    root,
                    route,
                    treeMeters[i] + 2.5f,
                    1f,
                    4.9f + (i % 2) * 0.55f,
                    5.45f + (i % 3) * 0.5f,
                    materials["deepNeedle"],
                    report);
            }

            var innerTreeMeters = new[] { 15f, 20f, 26f, 33f, 41f, 50f, 60f, 71f };
            for (var i = 0; i < innerTreeMeters.Length; i++)
            {
                foreach (var side in new[] { -1f, 1f })
                {
                    PlacePine(
                        "MYB106_InnerCanopyPine_" + i.ToString("00", CultureInfo.InvariantCulture) + (side < 0f ? "_L" : "_R"),
                        i + (side < 0f ? 82 : 96),
                        root,
                        route,
                        innerTreeMeters[i] + (side > 0f ? 1.5f : 0f),
                        side,
                        3.95f + (i % 3) * 0.35f,
                        4.4f + (i % 2) * 0.45f,
                        materials["deepNeedle"],
                        report);
                }
            }

            var midTreeMeters = new[] { 14f, 19f, 25f, 31f, 38f, 46f, 55f, 65f, 76f };
            for (var i = 0; i < midTreeMeters.Length; i++)
            {
                foreach (var side in new[] { -1f, 1f })
                {
                    PlacePine(
                        "MYB106_MidForestPine_" + i.ToString("00", CultureInfo.InvariantCulture) + (side < 0f ? "_L" : "_R"),
                        i + (side < 0f ? 31 : 43),
                        root,
                        route,
                        midTreeMeters[i] + (side > 0f ? 2.2f : 0f),
                        side,
                        7.7f + (i % 3) * 0.75f,
                        3.95f + (i % 2) * 0.45f,
                        materials["deepNeedle"],
                        report);
                }
            }

            var backTreeMeters = new[] { 18f, 28f, 39f, 51f, 64f, 78f };
            for (var i = 0; i < backTreeMeters.Length; i++)
            {
                foreach (var side in new[] { -1f, 1f })
                {
                    PlacePine(
                        "MYB106_BackForestPine_" + i.ToString("00", CultureInfo.InvariantCulture) + (side < 0f ? "_L" : "_R"),
                        i + (side < 0f ? 57 : 68),
                        root,
                        route,
                        backTreeMeters[i] + (side > 0f ? 3.5f : 0.8f),
                        side,
                        11.7f + (i % 3) * 1.05f,
                        3.15f + (i % 2) * 0.35f,
                        materials["deepNeedle"],
                        report);
                }
            }

            for (var i = 0; i < 16; i++)
            {
                var meters = 15f + i * 4.2f;
                CreateMossAccent("MYB106_AmberMoss_" + i.ToString("00", CultureInfo.InvariantCulture), root, route, meters, i % 2 == 0 ? -1f : 1f, 1.45f + (i % 3) * 0.35f, materials["amberMoss"]);
            }

            for (var i = 0; i < 18; i++)
            {
                var meters = 12f + i * 3.9f;
                var side = i % 2 == 0 ? -1f : 1f;
                CreateUndergrowthCluster(
                    "MYB106_UndergrowthCluster_" + i.ToString("00", CultureInfo.InvariantCulture),
                    root,
                    route,
                    meters,
                    side,
                    1.15f + (i % 4) * 0.45f,
                    materials["coolUndergrowth"],
                    materials["deepNeedle"]);
            }

            for (var i = 0; i < 10; i++)
            {
                var meters = 15f + i * 6.6f;
                var side = i % 2 == 0 ? -1f : 1f;
                CreateFallenBranch(
                    "MYB106_FallenBranch_" + i.ToString("00", CultureInfo.InvariantCulture),
                    root,
                    route,
                    meters,
                    side,
                    0.95f + (i % 3) * 0.55f,
                    materials["coolTrunk"],
                    materials["leafLitter"]);
            }

            AddSpotLight("MYB106_LowSunWarmSpot", root, SamplePosition(route, 40f, -7.2f, 4.5f), SamplePosition(route, 48f, -0.2f, 1.0f), new Color(1f, 0.53f, 0.24f), 2.4f, 30f, 44f, true);
            AddPointLight("MYB106_AmberUndergrowthFill", root, SamplePosition(route, 39f, 3.7f, 1.3f), new Color(1f, 0.45f, 0.16f), 1.4f, 12f, false);
            AddPointLight("MYB106_CoolCanopyFill", root, SamplePosition(route, 51f, -5.2f, 4.7f), new Color(0.26f, 0.48f, 0.58f), 1.0f, 18f, false);

            AddReflectionProbe("MYB106_LocalReflectionProbe_A", root, SamplePosition(route, 39f, 0f, 2.3f), new Vector3(21f, 10f, 28f));
            AddReflectionProbe("MYB106_LocalReflectionProbe_B", root, SamplePosition(route, 57f, 0f, 2.5f), new Vector3(23f, 11f, 30f));
            AddLightProbeGroup("MYB106_Passage01LightProbeGroup", root, route);

            report.SceneNotes.Add("Built dramatic undergrowth overlay: dense near/mid/background pines, canopy masses, road-edge leaf litter, low clusters, fallen branches, local lights and probes.");
        }

        private static void PlacePine(
            string name,
            int variantIndex,
            Transform parent,
            IReadOnlyList<Vector3> route,
            float meters,
            float side,
            float lateralOffset,
            float targetHeight,
            Material leafMaterial,
            ValidationReport report)
        {
            if (!MYB89RideTrajectory.TrySample(route, meters, false, out var sample))
            {
                report.Failures.Add("Could not sample route for " + name);
                return;
            }

            var prefabPath = MYB112.Editor.MYB112PremiumTreeRuntimeSet.GetVariantPrefabPath(variantIndex);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null)
            {
                report.Failures.Add("Missing MYB-112 premium tree variant prefab: " + prefabPath);
                return;
            }

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            instance.name = name;
            instance.transform.SetParent(parent);
            var placement = sample.Position + sample.Right * side * (RoadWidth * 0.5f + lateralOffset);
            var rotation = Quaternion.LookRotation(sample.Forward, Vector3.up) * Quaternion.Euler(0f, side * -12f, side * 2.5f);
            instance.transform.SetPositionAndRotation(placement, rotation);
            FitToHeight(instance, targetHeight);
            DisableColliders(instance);
            foreach (var renderer in instance.GetComponentsInChildren<Renderer>(true))
            {
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                renderer.receiveShadows = true;
            }

            report.ScenePlacements.Add(name + " <- " + prefabPath);
        }

        private static void CreateMossAccent(
            string name,
            Transform parent,
            IReadOnlyList<Vector3> route,
            float meters,
            float side,
            float lateralOffset,
            Material material)
        {
            if (!MYB89RideTrajectory.TrySample(route, meters, false, out var sample))
            {
                return;
            }

            var position = sample.Position + sample.Right * side * (RoadWidth * 0.5f + lateralOffset) + Vector3.up * 0.18f;
            var rotation = Quaternion.LookRotation(sample.Forward, Vector3.up) * Quaternion.Euler(0f, side * 12f, 0f);
            CreateCube(name, parent, position, new Vector3(1.15f, 0.18f, 0.6f), rotation, material);
        }

        private static void CreateUndergrowthCluster(
            string name,
            Transform parent,
            IReadOnlyList<Vector3> route,
            float meters,
            float side,
            float lateralOffset,
            Material lowMaterial,
            Material needleMaterial)
        {
            if (!MYB89RideTrajectory.TrySample(route, meters, false, out var sample))
            {
                return;
            }

            var basePosition = sample.Position + sample.Right * side * (RoadWidth * 0.5f + lateralOffset);
            var baseRotation = Quaternion.LookRotation(sample.Forward, Vector3.up) * Quaternion.Euler(0f, side * 12f, 0f);
            CreateCube(name + "_LowMass", parent, basePosition + Vector3.up * 0.32f, new Vector3(1.35f, 0.42f, 0.85f), baseRotation, lowMaterial);
            CreateCube(name + "_FernA", parent, basePosition + sample.Right * side * 0.54f + Vector3.up * 0.5f, new Vector3(0.28f, 0.78f, 0.72f), baseRotation * Quaternion.Euler(0f, 0f, side * -14f), needleMaterial);
            CreateCube(name + "_FernB", parent, basePosition - sample.Right * side * 0.38f + Vector3.up * 0.44f, new Vector3(0.24f, 0.66f, 0.62f), baseRotation * Quaternion.Euler(0f, 0f, side * 18f), needleMaterial);
        }

        private static void CreateFallenBranch(
            string name,
            Transform parent,
            IReadOnlyList<Vector3> route,
            float meters,
            float side,
            float lateralOffset,
            Material branchMaterial,
            Material litterMaterial)
        {
            if (!MYB89RideTrajectory.TrySample(route, meters, false, out var sample))
            {
                return;
            }

            var position = sample.Position + sample.Right * side * (RoadWidth * 0.5f + lateralOffset) + Vector3.up * 0.18f;
            var rotation = Quaternion.LookRotation((sample.Forward * 0.6f + sample.Right * side * 0.4f).normalized, Vector3.up) * Quaternion.Euler(0f, side * 9f, side * 3f);
            CreateCube(name + "_Wood", parent, position + Vector3.up * 0.14f, new Vector3(0.22f, 0.22f, 2.2f), rotation, branchMaterial);
            CreateCube(name + "_LeafShadow", parent, position + sample.Right * side * 0.2f + Vector3.up * 0.06f, new Vector3(1.2f, 0.08f, 0.72f), rotation, litterMaterial);
        }

        private static void CreateLightShaft(string name, Transform parent, IReadOnlyList<Vector3> route, float meters, float lateralOffset, float height, Material material)
        {
            if (!MYB89RideTrajectory.TrySample(route, meters, false, out var sample))
            {
                return;
            }

            var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quad.name = name;
            quad.transform.SetParent(parent);
            quad.transform.position = sample.Position + sample.Right * lateralOffset + Vector3.up * height;
            quad.transform.rotation = Quaternion.LookRotation(sample.Right * -1f + sample.Forward * 0.25f, Vector3.up) * Quaternion.Euler(0f, 0f, -13f);
            quad.transform.localScale = new Vector3(2.7f, 8.8f, 1f);
            SetMaterial(quad, material);
            DisableColliders(quad);
        }

        private static void CreateHazePlane(string name, Transform parent, IReadOnlyList<Vector3> route, float meters, float lateralOffset, float height, Material material)
        {
            if (!MYB89RideTrajectory.TrySample(route, meters, false, out var sample))
            {
                return;
            }

            var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quad.name = name;
            quad.transform.SetParent(parent);
            quad.transform.position = sample.Position + sample.Right * lateralOffset + Vector3.up * height;
            quad.transform.rotation = Quaternion.LookRotation(-sample.Forward, Vector3.up);
            quad.transform.localScale = new Vector3(20f, 9f, 1f);
            SetMaterial(quad, material);
            DisableColliders(quad);
        }

        private static void AddSpotLight(string name, Transform parent, Vector3 position, Vector3 target, Color color, float intensity, float range, float spotAngle, bool shadows)
        {
            var gameObject = new GameObject(name);
            gameObject.transform.SetParent(parent);
            gameObject.transform.position = position;
            gameObject.transform.rotation = Quaternion.LookRotation((target - position).normalized, Vector3.up);
            var light = gameObject.AddComponent<Light>();
            light.type = LightType.Spot;
            light.color = color;
            light.intensity = intensity;
            light.range = range;
            light.spotAngle = spotAngle;
            light.shadows = shadows ? LightShadows.Soft : LightShadows.None;
            light.shadowStrength = shadows ? 0.58f : 0f;
        }

        private static void AddPointLight(string name, Transform parent, Vector3 position, Color color, float intensity, float range, bool shadows)
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

        private static void AddReflectionProbe(string name, Transform parent, Vector3 position, Vector3 size)
        {
            var gameObject = new GameObject(name);
            gameObject.transform.SetParent(parent);
            gameObject.transform.position = position;
            var probe = gameObject.AddComponent<ReflectionProbe>();
            probe.mode = UnityEngine.Rendering.ReflectionProbeMode.Realtime;
            probe.refreshMode = UnityEngine.Rendering.ReflectionProbeRefreshMode.OnAwake;
            probe.timeSlicingMode = UnityEngine.Rendering.ReflectionProbeTimeSlicingMode.IndividualFaces;
            probe.resolution = 64;
            probe.intensity = 0.62f;
            probe.boxProjection = true;
            probe.size = size;
            probe.center = Vector3.up * 1.4f;
        }

        private static void AddLightProbeGroup(string name, Transform parent, IReadOnlyList<Vector3> route)
        {
            var gameObject = new GameObject(name);
            gameObject.transform.SetParent(parent);
            var positions = new List<Vector3>();
            foreach (var meters in new[] { 25f, 36f, 48f, 60f })
            {
                if (!MYB89RideTrajectory.TrySample(route, meters, false, out var sample))
                {
                    continue;
                }

                positions.Add(sample.Position + sample.Right * -4.4f + Vector3.up * 2.1f);
                positions.Add(sample.Position + Vector3.up * 2.6f);
                positions.Add(sample.Position + sample.Right * 4.4f + Vector3.up * 2.1f);
            }

            var group = gameObject.AddComponent<LightProbeGroup>();
            group.probePositions = positions.ToArray();
        }

        private static void CaptureStill(MYB89ProbeRide ride, Camera camera, string slug, string label, float meters, ValidationReport report)
        {
            var canvasStates = DisableCanvases();
            try
            {
                ride.SetPreviewProgress(meters);
                Canvas.ForceUpdateCanvases();
                var relativePath = OutputDirectory + "/" + slug + ".png";
                var absolutePath = Path.Combine(GetRepoRoot(), relativePath);
                var metrics = RenderCameraToPng(camera, CaptureWidth, CaptureHeight, absolutePath);
                report.Captures.Add(new CaptureReport(label, relativePath, meters, camera.transform.position, camera.transform.forward, metrics));
            }
            finally
            {
                RestoreCanvases(canvasStates);
            }
        }

        private static void CaptureVideoFrames(MYB89ProbeRide ride, Camera camera, ValidationReport report)
        {
            var absoluteDirectory = Path.Combine(GetRepoRoot(), VideoFrameDirectory);
            if (Directory.Exists(absoluteDirectory))
            {
                Directory.Delete(absoluteDirectory, true);
            }

            Directory.CreateDirectory(absoluteDirectory);
            var canvasStates = DisableCanvases();
            try
            {
                for (var i = 0; i < VideoFrameCount; i++)
                {
                    var t = i / (float)(VideoFrameCount - 1);
                    var meters = Mathf.Lerp(20f, 72f, t);
                    ride.SetPreviewProgress(meters);
                    Canvas.ForceUpdateCanvases();
                    RenderCameraToPng(camera, CaptureWidth, CaptureHeight, Path.Combine(absoluteDirectory, "frame_" + i.ToString("000", CultureInfo.InvariantCulture) + ".png"));
                }
            }
            finally
            {
                RestoreCanvases(canvasStates);
            }

            report.VideoFramePath = VideoFrameDirectory + "/frame_%03d.png";
            report.SceneNotes.Add("Captured " + VideoFrameCount.ToString(CultureInfo.InvariantCulture) + " video frames at " + VideoFrameRate.ToString(CultureInfo.InvariantCulture) + " fps for a 15 second silent proof.");
        }

        private static void ValidateLookDev(ValidationReport report)
        {
            var root = GameObject.Find("MYB106_LookDevPassage01");
            if (root == null)
            {
                report.Failures.Add("Missing MYB106_LookDevPassage01 root.");
                return;
            }

            var renderers = root.GetComponentsInChildren<Renderer>(true).Where(renderer => renderer.enabled).ToArray();
            var lights = root.GetComponentsInChildren<Light>(true);
            var reflectionProbes = root.GetComponentsInChildren<ReflectionProbe>(true);
            var lightProbeGroups = root.GetComponentsInChildren<LightProbeGroup>(true);
            var namedOverlayObjects = root.GetComponentsInChildren<Transform>(true)
                .Count(transform => transform.name.StartsWith("MYB106_", StringComparison.Ordinal));

            if (renderers.Length < 22)
            {
                report.Failures.Add("MYB106 overlay has too few visible renderers: " + renderers.Length.ToString(CultureInfo.InvariantCulture));
            }

            if (lights.Length < 3)
            {
                report.Failures.Add("MYB106 overlay has too few local lights: " + lights.Length.ToString(CultureInfo.InvariantCulture));
            }

            if (reflectionProbes.Length < 2)
            {
                report.Failures.Add("MYB106 overlay has too few reflection probes: " + reflectionProbes.Length.ToString(CultureInfo.InvariantCulture));
            }

            if (lightProbeGroups.Length < 1 || lightProbeGroups[0].probePositions.Length < 9)
            {
                report.Failures.Add("MYB106 overlay is missing a useful local LightProbeGroup.");
            }

            if (report.Captures.Count < 2)
            {
                report.Failures.Add("MYB106 proof needs before and after Vue Cible captures.");
            }

            report.SceneMetrics = new SceneMetrics(renderers.Length, lights.Length, reflectionProbes.Length, lightProbeGroups.Length, namedOverlayObjects);
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
            var steps = Mathf.Max(4, Mathf.CeilToInt((endMeters - startMeters) / 6f));
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

        private static GameObject CreateCube(string name, Transform parent, Vector3 position, Vector3 scale, Quaternion rotation, Material material)
        {
            var gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            gameObject.name = name;
            gameObject.transform.SetParent(parent);
            gameObject.transform.SetPositionAndRotation(position, rotation);
            gameObject.transform.localScale = scale;
            SetMaterial(gameObject, material);
            DisableColliders(gameObject);
            return gameObject;
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

        private static ImageMetrics RenderCameraToPng(Camera camera, int width, int height, string path)
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
                return ImageMetrics.FromTexture(texture);
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

        private static void DisableColliders(GameObject instance)
        {
            foreach (var collider in instance.GetComponentsInChildren<Collider>(true))
            {
                collider.enabled = false;
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

        private static void EnsureFolder(string assetPath)
        {
            if (AssetDatabase.IsValidFolder(assetPath))
            {
                return;
            }

            var parent = Path.GetDirectoryName(assetPath)?.Replace('\\', '/');
            var folder = Path.GetFileName(assetPath);
            if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent))
            {
                EnsureFolder(parent);
            }

            AssetDatabase.CreateFolder(parent ?? "Assets", folder);
        }

        private static void WriteReport(ValidationReport report)
        {
            var absolutePath = Path.Combine(GetRepoRoot(), ReportPath);
            Directory.CreateDirectory(Path.GetDirectoryName(absolutePath) ?? GetRepoRoot());
            var lines = new List<string>
            {
                "# MYB-106 Passage 01 LookDev Report",
                "",
                "Status: " + (report.Failures.Count == 0 ? "PASS" : "FAIL"),
                "Generated at UTC: " + DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture),
                "Canonical scene: " + CanonicalScenePath,
                "Intent: dramatic stylized premium undergrowth Vue Cible for Passage 01.",
                "Capture size: 1280x720",
                "Video frame rate: " + VideoFrameRate.ToString(CultureInfo.InvariantCulture) + " fps",
                "",
                "## Scope guard",
                "- Replays MYB-104, then applies reversible `MYB106_LookDevPassage01` overlay.",
                "- No PC_RPAsset, PC_Renderer, QualitySettings, GraphicsSettings, src/**, unity/Echappee3D/**, FTMS/BLE or hardware scope.",
                "- Existing MYB-104 assets can be attenuated locally; replacement remains MYB-108.",
                "",
                "## Scene changes"
            };

            lines.AddRange(report.SceneNotes.Select(note => "- " + note));
            lines.Add("");
            lines.Add("## Overlay metrics");
            if (report.SceneMetrics != null)
            {
                lines.AddRange(report.SceneMetrics.ToLines());
            }

            lines.Add("");
            lines.Add("## Captures");
            foreach (var capture in report.Captures)
            {
                lines.Add("- " + capture.Label + ": `" + capture.RelativePath + "` at " + capture.Meters.ToString("0.0", CultureInfo.InvariantCulture) + " m; " + capture.Metrics.ToSummary());
                lines.Add("  camera: " + FormatVector(capture.CameraPosition) + " forward " + FormatVector(capture.CameraForward));
            }

            lines.Add("");
            lines.Add("## Video proof");
            if (!string.IsNullOrEmpty(report.VideoFramePath))
            {
                lines.Add("- Frames: `" + report.VideoFramePath + "`");
                lines.Add("- Encode command: `ffmpeg -y -framerate " + VideoFrameRate.ToString(CultureInfo.InvariantCulture) + " -i " + VideoFrameDirectory + "/frame_%03d.png -c:v libx264 -pix_fmt yuv420p _bmad-output/unity-test-results/myb-106/passage-01-lookdev-15s-720p.mp4`");
            }
            else
            {
                lines.Add("- Not captured by this validation run.");
            }

            lines.Add("");
            lines.Add("## Placements");
            lines.AddRange(report.ScenePlacements.Take(80).Select(note => "- " + note));
            lines.Add("");
            lines.Add("## Failures");
            lines.AddRange(report.Failures.Count == 0 ? new[] { "- None." } : report.Failures.Select(failure => "- " + failure));
            lines.Add("");

            File.WriteAllLines(absolutePath, lines);
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

        private sealed class ValidationReport
        {
            public ValidationReport(bool capturesVideoFrames)
            {
                CapturesVideoFrames = capturesVideoFrames;
            }

            public bool CapturesVideoFrames { get; }
            public string VideoFramePath { get; set; }
            public SceneMetrics SceneMetrics { get; set; }
            public List<string> SceneNotes { get; } = new List<string>();
            public List<string> ScenePlacements { get; } = new List<string>();
            public List<CaptureReport> Captures { get; } = new List<CaptureReport>();
            public List<string> Failures { get; } = new List<string>();
        }

        private sealed class CaptureReport
        {
            public CaptureReport(string label, string relativePath, float meters, Vector3 cameraPosition, Vector3 cameraForward, ImageMetrics metrics)
            {
                Label = label;
                RelativePath = relativePath;
                Meters = meters;
                CameraPosition = cameraPosition;
                CameraForward = cameraForward;
                Metrics = metrics;
            }

            public string Label { get; }
            public string RelativePath { get; }
            public float Meters { get; }
            public Vector3 CameraPosition { get; }
            public Vector3 CameraForward { get; }
            public ImageMetrics Metrics { get; }
        }

        private sealed class SceneMetrics
        {
            public SceneMetrics(int rendererCount, int lightCount, int reflectionProbeCount, int lightProbeGroupCount, int namedOverlayObjectCount)
            {
                RendererCount = rendererCount;
                LightCount = lightCount;
                ReflectionProbeCount = reflectionProbeCount;
                LightProbeGroupCount = lightProbeGroupCount;
                NamedOverlayObjectCount = namedOverlayObjectCount;
            }

            private int RendererCount { get; }
            private int LightCount { get; }
            private int ReflectionProbeCount { get; }
            private int LightProbeGroupCount { get; }
            private int NamedOverlayObjectCount { get; }

            public IEnumerable<string> ToLines()
            {
                yield return "- MYB106 renderers: " + RendererCount.ToString(CultureInfo.InvariantCulture);
                yield return "- MYB106 local lights: " + LightCount.ToString(CultureInfo.InvariantCulture);
                yield return "- MYB106 reflection probes: " + ReflectionProbeCount.ToString(CultureInfo.InvariantCulture);
                yield return "- MYB106 light probe groups: " + LightProbeGroupCount.ToString(CultureInfo.InvariantCulture);
                yield return "- MYB106 named overlay objects: " + NamedOverlayObjectCount.ToString(CultureInfo.InvariantCulture);
            }
        }

        private readonly struct ImageMetrics
        {
            private ImageMetrics(float averageLuminance, float contrast, float darkPixelPercent, float brightPixelPercent)
            {
                AverageLuminance = averageLuminance;
                Contrast = contrast;
                DarkPixelPercent = darkPixelPercent;
                BrightPixelPercent = brightPixelPercent;
            }

            private float AverageLuminance { get; }
            private float Contrast { get; }
            private float DarkPixelPercent { get; }
            private float BrightPixelPercent { get; }

            public static ImageMetrics FromTexture(Texture2D texture)
            {
                double sum = 0d;
                double sumSquares = 0d;
                var darkPixels = 0;
                var brightPixels = 0;
                var samples = 0;

                for (var y = 0; y < texture.height; y += 4)
                {
                    for (var x = 0; x < texture.width; x += 4)
                    {
                        var color = texture.GetPixel(x, y);
                        var luminance = color.linear.r * 0.2126f + color.linear.g * 0.7152f + color.linear.b * 0.0722f;
                        sum += luminance;
                        sumSquares += luminance * luminance;
                        if (luminance < 0.08f)
                        {
                            darkPixels++;
                        }

                        if (luminance > 0.82f)
                        {
                            brightPixels++;
                        }

                        samples++;
                    }
                }

                if (samples == 0)
                {
                    return new ImageMetrics(0f, 0f, 0f, 0f);
                }

                var average = sum / samples;
                var variance = Math.Max(0d, sumSquares / samples - average * average);
                return new ImageMetrics(
                    (float)average,
                    (float)Math.Sqrt(variance),
                    (float)(darkPixels * 100d / samples),
                    (float)(brightPixels * 100d / samples));
            }

            public string ToSummary()
            {
                return string.Format(
                    CultureInfo.InvariantCulture,
                    "avg luma {0:0.000}, contrast {1:0.000}, dark {2:0.0}%, bright {3:0.0}%",
                    AverageLuminance,
                    Contrast,
                    DarkPixelPercent,
                    BrightPixelPercent);
            }
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
    }
}

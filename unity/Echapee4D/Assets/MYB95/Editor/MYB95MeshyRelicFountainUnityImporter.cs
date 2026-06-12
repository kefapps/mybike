using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using MYB95;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MYB95.Editor
{
    public static class MYB95MeshyRelicFountainUnityImporter
    {
        private const string ScenePath = "Assets/Scenes/MYB95MeshyRelicFountainPoc.unity";
        private const string CanonicalScenePath = "Assets/Scenes/MYB89UnityMcpProbe.unity";
        private const string Root = "Assets/Echappee/Art/MYB95MeshyRelicFountain";
        private const string HeroModelPath = Root + "/Models/MYB95_RelicFountain_Hero_80k.fbx";
        private const string Lod0ModelPath = Root + "/Models/MYB95_RelicFountain_LOD0_50k.fbx";
        private const string Lod1ModelPath = Root + "/Models/MYB95_RelicFountain_LOD1_30k.fbx";
        private const string BaseColorPath = Root + "/Textures/refined_base_color.png";
        private const string MetallicPath = Root + "/Textures/refined_metallic.png";
        private const string RoughnessPath = Root + "/Textures/refined_roughness.png";
        private const string NormalPath = Root + "/Textures/refined_normal.png";
        private const string EmissionPath = Root + "/Textures/refined_emission.png";
        private const string RelicMaterialPath = Root + "/Materials/MYB95_RelicFountain_PBR.mat";
        private const string EnergyMaterialPath = Root + "/Materials/MYB95_RelicFountain_Energy.mat";
        private const string WaterMaterialPath = Root + "/Materials/MYB95_RelicFountain_Water.mat";
        private const string PrefabPath = Root + "/Prefabs/MYB95_RelicFountain.prefab";
        private const string ReportRelativePath = "_bmad-output/unity-test-results/myb-95-meshy-relic-fountain-unity-import.txt";
        private const string CaptureRelativePath = "_bmad-output/unity-test-results/myb-95-meshy-relic-fountain-unity-import.png";

        private const float TargetHeightMeters = 2.25f;
        private const int HeroTriangleBudget = 90000;
        private const int Lod0TriangleBudget = 60000;
        private const int Lod1TriangleBudget = 40000;

        [MenuItem("Tools/MYB-95/Build Meshy Relic Fountain Unity Import")]
        public static void BuildAndValidateFromMenu()
        {
            var reportPath = BuildAndValidate();
            Debug.Log("MYB-95 Meshy relic fountain Unity import complete. Report: " + reportPath);
        }

        public static void BuildAndValidateCli()
        {
            var reportPath = BuildAndValidate();
            var reportText = File.ReadAllText(Path.Combine(GetRepoRoot(), reportPath));
            if (reportText.Contains("Status: FAIL", StringComparison.Ordinal))
            {
                throw new InvalidOperationException("MYB-95 Meshy relic fountain Unity import failed. See " + reportPath);
            }
        }

        public static string BuildAndValidate()
        {
            var report = new ValidationReport();

            try
            {
                AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
                EnsureFolders();
                ValidateSourceFiles(report);
                ConfigureImporters(report);

                var relicMaterial = CreateRelicMaterial(report);
                var energyMaterial = CreateEnergyMaterial(report);
                var waterMaterial = CreateWaterMaterial(report);
                CreatePrefab(relicMaterial, energyMaterial, waterMaterial, report);

                var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                BuildValidationScene(report);
                EditorSceneManager.SaveScene(scene, ScenePath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

                CaptureScenePreview(report);
                ValidateImportedAssets(report);
                ValidatePrefab(report);
                ValidateCanonicalBaselineBoundary(report);
                ValidateScene(report);
                AddVerdict(report);
            }
            catch (Exception exception)
            {
                report.Failures.Add(exception.GetType().FullName + ": " + exception.Message);
                Debug.LogException(exception);
            }

            return WriteReport(report);
        }

        private static void EnsureFolders()
        {
            CreateFolderRecursive(Root + "/Models");
            CreateFolderRecursive(Root + "/Textures");
            CreateFolderRecursive(Root + "/Materials");
            CreateFolderRecursive(Root + "/Prefabs");
        }

        private static void ValidateSourceFiles(ValidationReport report)
        {
            foreach (var assetPath in SourceAssetPaths())
            {
                var absolutePath = ProjectRelativeToAbsolute(assetPath);
                if (!File.Exists(absolutePath))
                {
                    report.Failures.Add("Missing source asset: " + assetPath);
                    continue;
                }

                report.ImportedFiles.Add(assetPath + " (" + FormatBytes(new FileInfo(absolutePath).Length) + ")");
            }
        }

        private static void ConfigureImporters(ValidationReport report)
        {
            foreach (var modelPath in ModelPaths())
            {
                ConfigureModelImporter(modelPath, report);
            }

            ConfigureTexture(BaseColorPath, TextureImporterType.Default, true, 2048, report);
            ConfigureTexture(MetallicPath, TextureImporterType.Default, false, 2048, report);
            ConfigureTexture(RoughnessPath, TextureImporterType.Default, false, 2048, report);
            ConfigureTexture(NormalPath, TextureImporterType.NormalMap, false, 2048, report);
            ConfigureTexture(EmissionPath, TextureImporterType.Default, true, 2048, report);
        }

        private static void ConfigureModelImporter(string modelPath, ValidationReport report)
        {
            var importer = AssetImporter.GetAtPath(modelPath) as ModelImporter;
            if (importer == null)
            {
                report.Failures.Add("Model importer missing: " + modelPath);
                return;
            }

            var changed = importer.globalScale < 0.99f ||
                importer.globalScale > 1.01f ||
                importer.importAnimation ||
                importer.importCameras ||
                importer.importLights ||
                importer.materialImportMode != ModelImporterMaterialImportMode.None;

            importer.globalScale = 1f;
            importer.importAnimation = false;
            importer.importCameras = false;
            importer.importLights = false;
            importer.materialImportMode = ModelImporterMaterialImportMode.None;
            importer.importNormals = ModelImporterNormals.Import;
            importer.importTangents = ModelImporterTangents.CalculateMikk;
            importer.isReadable = false;

            if (changed)
            {
                importer.SaveAndReimport();
            }
        }

        private static void ConfigureTexture(string assetPath, TextureImporterType type, bool srgb, int maxSize, ValidationReport report)
        {
            var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null)
            {
                report.Failures.Add("Texture importer missing: " + assetPath);
                return;
            }

            var changed = importer.textureType != type ||
                importer.sRGBTexture != srgb ||
                importer.maxTextureSize != maxSize ||
                importer.mipmapEnabled == false ||
                importer.textureCompression != TextureImporterCompression.CompressedHQ;

            importer.textureType = type;
            importer.sRGBTexture = srgb;
            importer.maxTextureSize = maxSize;
            importer.mipmapEnabled = true;
            importer.textureCompression = TextureImporterCompression.CompressedHQ;

            if (changed)
            {
                importer.SaveAndReimport();
            }
        }

        private static Material CreateRelicMaterial(ValidationReport report)
        {
            var material = MaterialAt(RelicMaterialPath, "Universal Render Pipeline/Lit", report);
            if (material == null)
            {
                return null;
            }

            SetTexture(material, "_BaseMap", LoadTexture(BaseColorPath, report));
            SetTexture(material, "_MainTex", LoadTexture(BaseColorPath, report));
            SetTexture(material, "_MetallicGlossMap", LoadTexture(MetallicPath, report));
            SetTexture(material, "_BumpMap", LoadTexture(NormalPath, report));
            SetTexture(material, "_EmissionMap", LoadTexture(EmissionPath, report));
            SetColor(material, "_BaseColor", Color.white);
            SetColor(material, "_Color", Color.white);
            SetColor(material, "_EmissionColor", new Color(0.15f, 0.68f, 1f, 1f) * 0.85f);
            SetFloat(material, "_Metallic", 0.12f);
            SetFloat(material, "_Smoothness", 0.48f);
            SetFloat(material, "_BumpScale", 0.35f);
            material.EnableKeyword("_NORMALMAP");
            material.EnableKeyword("_METALLICSPECGLOSSMAP");
            material.EnableKeyword("_EMISSION");
            material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            EditorUtility.SetDirty(material);

            report.Materials.Add(RelicMaterialPath);
            report.Notes.Add("Roughness map imported and kept next to the asset; Unity material uses fixed smoothness until metallic/smoothness packing is added.");
            return material;
        }

        private static Material CreateEnergyMaterial(ValidationReport report)
        {
            var material = MaterialAt(EnergyMaterialPath, "Universal Render Pipeline/Unlit", report);
            if (material == null)
            {
                return null;
            }

            var blue = new Color(0.04f, 0.62f, 1f, 1f);
            SetColor(material, "_BaseColor", blue);
            SetColor(material, "_Color", blue);
            SetColor(material, "_EmissionColor", blue * 1.8f);
            material.EnableKeyword("_EMISSION");
            material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            EditorUtility.SetDirty(material);
            report.Materials.Add(EnergyMaterialPath);
            return material;
        }

        private static Material CreateWaterMaterial(ValidationReport report)
        {
            var material = MaterialAt(WaterMaterialPath, "Universal Render Pipeline/Lit", report);
            if (material == null)
            {
                return null;
            }

            var color = new Color(0.08f, 0.72f, 1f, 0.72f);
            SetColor(material, "_BaseColor", color);
            SetColor(material, "_Color", color);
            SetColor(material, "_EmissionColor", color * 0.65f);
            SetFloat(material, "_Metallic", 0f);
            SetFloat(material, "_Smoothness", 0.82f);
            material.EnableKeyword("_EMISSION");
            material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            EditorUtility.SetDirty(material);
            report.Materials.Add(WaterMaterialPath);
            return material;
        }

        private static Material MaterialAt(string assetPath, string preferredShader, ValidationReport report)
        {
            var shader = Shader.Find(preferredShader) ??
                Shader.Find("Universal Render Pipeline/Lit") ??
                Shader.Find("Standard");
            if (shader == null)
            {
                report.Failures.Add("No compatible shader found for " + assetPath);
                return null;
            }

            var material = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
            if (material == null)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, assetPath);
            }
            else
            {
                material.shader = shader;
            }

            return material;
        }

        private static void CreatePrefab(Material relicMaterial, Material energyMaterial, Material waterMaterial, ValidationReport report)
        {
            AssetDatabase.DeleteAsset(PrefabPath);

            var root = new GameObject("MYB95_RelicFountain");
            try
            {
                var hero = InstantiateModel(HeroModelPath, "MYB95_RelicFountain_Hero80k", root.transform, relicMaterial, report);
                var lod0 = InstantiateModel(Lod0ModelPath, "MYB95_RelicFountain_LOD50k", root.transform, relicMaterial, report);
                var lod1 = InstantiateModel(Lod1ModelPath, "MYB95_RelicFountain_LOD30k", root.transform, relicMaterial, report);
                if (hero == null || lod0 == null || lod1 == null)
                {
                    return;
                }

                FitModelToHeight(hero, TargetHeightMeters);
                FitModelToHeight(lod0, TargetHeightMeters);
                FitModelToHeight(lod1, TargetHeightMeters);
                CreateLodGroup(root, hero, lod0, lod1);
                CreateUnityEnergyLayer(root.transform, energyMaterial, waterMaterial);

                var savedPrefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
                if (savedPrefab == null)
                {
                    report.Failures.Add("Failed to save prefab: " + PrefabPath);
                    return;
                }

                report.Prefabs.Add(PrefabPath);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        private static GameObject InstantiateModel(string modelPath, string name, Transform parent, Material material, ValidationReport report)
        {
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
            if (model == null)
            {
                report.Failures.Add("Could not load model asset: " + modelPath);
                return null;
            }

            var instance = PrefabUtility.InstantiatePrefab(model) as GameObject;
            if (instance == null)
            {
                report.Failures.Add("Could not instantiate model asset: " + modelPath);
                return null;
            }

            instance.name = name;
            instance.transform.SetParent(parent, false);
            AssignMaterial(instance, material);
            return instance;
        }

        private static void CreateLodGroup(GameObject root, GameObject hero, GameObject lod0, GameObject lod1)
        {
            var group = root.AddComponent<LODGroup>();
            group.SetLODs(new[]
            {
                new LOD(0.58f, hero.GetComponentsInChildren<Renderer>()),
                new LOD(0.32f, lod0.GetComponentsInChildren<Renderer>()),
                new LOD(0.12f, lod1.GetComponentsInChildren<Renderer>()),
            });
            group.animateCrossFading = true;
            group.RecalculateBounds();
        }

        private static void FitModelToHeight(GameObject modelInstance, float targetHeight)
        {
            var bounds = GetWorldBounds(modelInstance);
            if (bounds.size.y <= 0.001f)
            {
                return;
            }

            modelInstance.transform.localScale *= targetHeight / bounds.size.y;
            bounds = GetWorldBounds(modelInstance);
            modelInstance.transform.position -= new Vector3(bounds.center.x, bounds.min.y, bounds.center.z);
        }

        private static void CreateUnityEnergyLayer(Transform parent, Material energyMaterial, Material waterMaterial)
        {
            var water = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            water.name = "MYB95_UnityWaterGlow";
            water.transform.SetParent(parent, false);
            water.transform.localPosition = new Vector3(0f, 0.68f, -0.02f);
            water.transform.localScale = new Vector3(0.34f, 0.07f, 0.34f);
            AssignMaterial(water, waterMaterial);
            RemoveCollider(water);

            var ringA = CreateRuneRing("MYB95_UnityRuneOrbit_A", parent, energyMaterial, 0.98f, 0f);
            var ringB = CreateRuneRing("MYB95_UnityRuneOrbit_B", parent, energyMaterial, 1.12f, 90f);
            var crystal = GameObject.CreatePrimitive(PrimitiveType.Cube);
            crystal.name = "MYB95_UnityBlueCrystalCore";
            crystal.transform.SetParent(parent, false);
            crystal.transform.localPosition = new Vector3(0f, 0.96f, -0.02f);
            crystal.transform.localRotation = Quaternion.Euler(0f, 45f, 0f);
            crystal.transform.localScale = new Vector3(0.085f, 0.18f, 0.085f);
            AssignMaterial(crystal, energyMaterial);
            RemoveCollider(crystal);

            var lightObject = new GameObject("MYB95_UnityRelicEnergyLight");
            lightObject.transform.SetParent(parent, false);
            lightObject.transform.localPosition = new Vector3(0f, 1.06f, -0.06f);
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(0.10f, 0.62f, 1f, 1f);
            light.intensity = 1.25f;
            light.range = 4.6f;
            light.shadows = LightShadows.None;

            var warmLightObject = new GameObject("MYB95_UnityLanternWarmFill");
            warmLightObject.transform.SetParent(parent, false);
            warmLightObject.transform.localPosition = new Vector3(0f, 1.72f, -0.10f);
            var warmLight = warmLightObject.AddComponent<Light>();
            warmLight.type = LightType.Point;
            warmLight.color = new Color(1f, 0.74f, 0.38f, 1f);
            warmLight.intensity = 0.55f;
            warmLight.range = 3.2f;
            warmLight.shadows = LightShadows.None;

            var pulse = parent.gameObject.AddComponent<MYB95RelicFountainPulse>();
            var energyRenderers = new List<Renderer>
            {
                water.GetComponent<Renderer>(),
                crystal.GetComponent<Renderer>()
            };
            energyRenderers.AddRange(ringA.GetComponentsInChildren<Renderer>());
            energyRenderers.AddRange(ringB.GetComponentsInChildren<Renderer>());
            pulse.Configure(
                new[] { light, warmLight },
                energyRenderers.Where(renderer => renderer != null).ToArray(),
                new[] { ringA.transform, ringB.transform });
        }

        private static GameObject CreateRuneRing(string name, Transform parent, Material material, float height, float yaw)
        {
            var ring = new GameObject(name);
            ring.name = name;
            ring.transform.SetParent(parent, false);
            ring.transform.localPosition = new Vector3(0f, height, -0.02f);
            ring.transform.localRotation = Quaternion.Euler(0f, yaw, 8f);

            CreateRuneBar(name + "_Top", ring.transform, new Vector3(0f, 0.18f, 0f), new Vector3(0.54f, 0.014f, 0.020f), material);
            CreateRuneBar(name + "_Bottom", ring.transform, new Vector3(0f, -0.18f, 0f), new Vector3(0.54f, 0.014f, 0.020f), material);
            CreateRuneBar(name + "_Left", ring.transform, new Vector3(-0.28f, 0f, 0f), new Vector3(0.014f, 0.34f, 0.020f), material);
            CreateRuneBar(name + "_Right", ring.transform, new Vector3(0.28f, 0f, 0f), new Vector3(0.014f, 0.34f, 0.020f), material);
            return ring;
        }

        private static void CreateRuneBar(string name, Transform parent, Vector3 localPosition, Vector3 localScale, Material material)
        {
            var bar = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bar.name = name;
            bar.transform.SetParent(parent, false);
            bar.transform.localPosition = localPosition;
            bar.transform.localScale = localScale;
            AssignMaterial(bar, material);
            RemoveCollider(bar);
        }

        private static void BuildValidationScene(ValidationReport report)
        {
            ConfigureRenderSettings();
            CreateCamera();
            CreateSun();
            CreateValidationGround();

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            if (prefab == null)
            {
                report.Failures.Add("Prefab missing before scene build: " + PrefabPath);
                return;
            }

            var instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            if (instance == null)
            {
                report.Failures.Add("Could not instantiate prefab in validation scene: " + PrefabPath);
                return;
            }

            instance.name = "MYB95_RelicFountain_SceneProof";
            instance.transform.SetPositionAndRotation(new Vector3(0f, 0f, 1.35f), Quaternion.Euler(0f, -18f, 0f));
            report.SceneObjects.Add(instance.name);

            var distance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            if (distance != null)
            {
                distance.name = "MYB95_RelicFountain_DistanceCheck";
                distance.transform.SetPositionAndRotation(new Vector3(2.55f, 0f, 5.15f), Quaternion.Euler(0f, -36f, 0f));
                distance.transform.localScale = Vector3.one * 0.58f;
                report.SceneObjects.Add(distance.name);
            }
        }

        private static void ConfigureRenderSettings()
        {
            RenderSettings.skybox = null;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.58f, 0.67f, 0.72f);
            RenderSettings.ambientEquatorColor = new Color(0.34f, 0.32f, 0.28f);
            RenderSettings.ambientGroundColor = new Color(0.18f, 0.16f, 0.13f);
            RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(0.31f, 0.39f, 0.41f);
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogStartDistance = 10f;
            RenderSettings.fogEndDistance = 34f;
        }

        private static void CreateCamera()
        {
            var cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(0.14f, 1.28f, -4.65f);
            cameraObject.transform.rotation = Quaternion.Euler(9.5f, 0f, 0f);
            var camera = cameraObject.AddComponent<Camera>();
            camera.fieldOfView = 43f;
            camera.nearClipPlane = 0.03f;
            camera.farClipPlane = 85f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.26f, 0.34f, 0.36f);
        }

        private static void CreateSun()
        {
            var sunObject = new GameObject("MYB95_RelicLowWarmSun");
            sunObject.transform.rotation = Quaternion.Euler(35f, -35f, 0f);
            var sun = sunObject.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.color = new Color(1f, 0.82f, 0.62f);
            sun.intensity = 0.54f;
            sun.shadows = LightShadows.Soft;
        }

        private static void CreateValidationGround()
        {
            var paving = CreateMaterial("MYB95_RelicValidationPaving", new Color(0.50f, 0.42f, 0.34f), 0.16f);
            var grass = CreateMaterial("MYB95_RelicValidationGrass", new Color(0.22f, 0.36f, 0.26f), 0.05f);
            var edge = CreateMaterial("MYB95_RelicValidationRoadEdge", new Color(0.72f, 0.58f, 0.38f), 0.12f);

            CreateCube("MYB95_RelicVillageGround", new Vector3(0f, -0.08f, 2.1f), new Vector3(8.8f, 0.10f, 9.2f), grass);
            CreateCube("MYB95_RelicPavedRoadProof", new Vector3(0f, -0.02f, 2.1f), new Vector3(1.65f, 0.08f, 9.0f), paving);
            CreateCube("MYB95_RelicLeftRoadLip", new Vector3(-0.89f, 0.03f, 2.1f), new Vector3(0.08f, 0.10f, 9.0f), edge);
            CreateCube("MYB95_RelicRightRoadLip", new Vector3(0.89f, 0.03f, 2.1f), new Vector3(0.08f, 0.10f, 9.0f), edge);
        }

        private static Material CreateMaterial(string materialName, Color color, float smoothness)
        {
            var materialPath = Root + "/Materials/" + materialName + ".mat";
            var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            var material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (material == null)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, materialPath);
            }
            else
            {
                material.shader = shader;
            }

            SetColor(material, "_BaseColor", color);
            SetColor(material, "_Color", color);
            SetFloat(material, "_Metallic", 0f);
            SetFloat(material, "_Smoothness", smoothness);
            EditorUtility.SetDirty(material);
            return material;
        }

        private static GameObject CreateCube(string name, Vector3 position, Vector3 scale, Material material)
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = name;
            cube.transform.position = position;
            cube.transform.localScale = scale;
            AssignMaterial(cube, material);
            RemoveCollider(cube);
            return cube;
        }

        private static void CaptureScenePreview(ValidationReport report)
        {
            var camera = Camera.main;
            if (camera == null)
            {
                report.Failures.Add("Cannot capture MYB-95 scene: missing Main Camera.");
                return;
            }

            var absolutePath = Path.Combine(GetRepoRoot(), CaptureRelativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(absolutePath) ?? GetRepoRoot());

            var renderTexture = new RenderTexture(1280, 720, 24, RenderTextureFormat.ARGB32);
            var texture = new Texture2D(1280, 720, TextureFormat.RGB24, false);
            var previousActive = RenderTexture.active;
            var previousTarget = camera.targetTexture;

            try
            {
                camera.targetTexture = renderTexture;
                RenderTexture.active = renderTexture;
                camera.Render();
                texture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
                texture.Apply();
                File.WriteAllBytes(absolutePath, texture.EncodeToPNG());
                report.CapturePath = CaptureRelativePath;
            }
            finally
            {
                camera.targetTexture = previousTarget;
                RenderTexture.active = previousActive;
                UnityEngine.Object.DestroyImmediate(texture);
                renderTexture.Release();
                UnityEngine.Object.DestroyImmediate(renderTexture);
            }
        }

        private static void ValidateImportedAssets(ValidationReport report)
        {
            var heroTriangles = CountTrianglesInAsset(HeroModelPath);
            var lod0Triangles = CountTrianglesInAsset(Lod0ModelPath);
            var lod1Triangles = CountTrianglesInAsset(Lod1ModelPath);
            report.ModelMetrics.Add("Hero triangles: " + heroTriangles.ToString(CultureInfo.InvariantCulture));
            report.ModelMetrics.Add("LOD0 triangles: " + lod0Triangles.ToString(CultureInfo.InvariantCulture));
            report.ModelMetrics.Add("LOD1 triangles: " + lod1Triangles.ToString(CultureInfo.InvariantCulture));

            if (heroTriangles <= 0 || lod0Triangles <= 0 || lod1Triangles <= 0)
            {
                report.Failures.Add("One or more relic fountain LOD meshes imported with no triangles.");
            }
            if (heroTriangles > HeroTriangleBudget)
            {
                report.Failures.Add("Hero triangle count exceeds budget: " + heroTriangles);
            }
            if (lod0Triangles > Lod0TriangleBudget)
            {
                report.Failures.Add("LOD0 triangle count exceeds budget: " + lod0Triangles);
            }
            if (lod1Triangles > Lod1TriangleBudget)
            {
                report.Failures.Add("LOD1 triangle count exceeds budget: " + lod1Triangles);
            }

            foreach (var texturePath in TexturePaths())
            {
                var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
                if (texture == null)
                {
                    report.Failures.Add("Texture failed to import: " + texturePath);
                    continue;
                }

                report.TextureDimensions.Add(texturePath + " (" + texture.width + "x" + texture.height + ")");
                if (texture.width > 2048 || texture.height > 2048)
                {
                    report.Failures.Add("Texture exceeds MYB-95 2K POC budget: " + texturePath);
                }
            }
        }

        private static void ValidatePrefab(ValidationReport report)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            if (prefab == null)
            {
                report.Failures.Add("Prefab failed to load: " + PrefabPath);
                return;
            }

            var instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            if (instance == null)
            {
                report.Failures.Add("Prefab failed to instantiate: " + PrefabPath);
                return;
            }

            try
            {
                var lodGroup = instance.GetComponent<LODGroup>();
                var pulse = instance.GetComponent<MYB95RelicFountainPulse>();
                var renderers = instance.GetComponentsInChildren<Renderer>();
                var lights = instance.GetComponentsInChildren<Light>();

                report.ModelMetrics.Add("Prefab renderers: " + renderers.Length.ToString(CultureInfo.InvariantCulture));
                report.ModelMetrics.Add("Prefab total triangles across LOD assets and Unity VFX: " + CountTrianglesInHierarchy(instance).ToString(CultureInfo.InvariantCulture));
                report.ModelMetrics.Add("Prefab lights: " + lights.Length.ToString(CultureInfo.InvariantCulture));

                if (lodGroup == null || lodGroup.GetLODs().Length != 3)
                {
                    report.Failures.Add("Prefab should contain a 3-level LODGroup.");
                }
                if (renderers.Length < 7)
                {
                    report.Failures.Add("Prefab should contain LOD renderers plus Unity-authored energy/water renderers.");
                }
                if (lights.Length < 2)
                {
                    report.Failures.Add("Prefab should contain blue energy and warm lantern point lights.");
                }
                if (pulse == null)
                {
                    report.Failures.Add("Prefab missing MYB95RelicFountainPulse component.");
                }
                else
                {
                    var pointLight = lights.FirstOrDefault(light => light.type == LightType.Point);
                    pulse.ApplyPreview(0.17f);
                    var sampleA = pointLight == null ? 0f : pointLight.intensity;
                    pulse.ApplyPreview(1.31f);
                    var sampleB = pointLight == null ? 0f : pointLight.intensity;
                    report.ModelMetrics.Add("Pulse intensity sample: " + sampleA.ToString("0.000", CultureInfo.InvariantCulture) + " -> " + sampleB.ToString("0.000", CultureInfo.InvariantCulture));
                    if (pointLight == null || Mathf.Abs(sampleA - sampleB) < 0.015f)
                    {
                        report.Failures.Add("Relic fountain pulse script did not vary Point Light intensity.");
                    }
                }
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(instance);
            }
        }

        private static void ValidateCanonicalBaselineBoundary(ValidationReport report)
        {
            if (!File.Exists(ProjectRelativeToAbsolute(CanonicalScenePath)))
            {
                report.Failures.Add("Canonical scene missing: " + CanonicalScenePath);
            }

            var enabledScenes = EditorBuildSettings.scenes.Where(scene => scene.enabled).Select(scene => scene.path).ToArray();
            if (!enabledScenes.Contains(CanonicalScenePath, StringComparer.Ordinal))
            {
                report.Failures.Add("Canonical scene is no longer enabled in Build Settings.");
            }
            if (enabledScenes.Contains(ScenePath, StringComparer.Ordinal))
            {
                report.Failures.Add("MYB-95 relic POC scene should not be added to Build Settings.");
            }

            report.EnabledBuildScenes.AddRange(enabledScenes);
        }

        private static void ValidateScene(ValidationReport report)
        {
            if (!File.Exists(ProjectRelativeToAbsolute(ScenePath)))
            {
                report.Failures.Add("POC scene was not saved: " + ScenePath);
            }
            if (report.SceneObjects.Count < 2)
            {
                report.Failures.Add("POC scene should contain close and distance relic proof instances.");
            }
            if (string.IsNullOrWhiteSpace(report.CapturePath))
            {
                report.Failures.Add("POC capture was not produced.");
            }
        }

        private static void AddVerdict(ValidationReport report)
        {
            report.ArtVerdicts.Add("Direct Unity import is viable after Blender optimization: the prefab keeps an 80k close LOD, 50k mid LOD, 30k distance LOD, and stays sandboxed.");
            report.ArtVerdicts.Add("The refined material read supports a premium relic/fountain direction better than the untextured preview, but the base Meshy mesh is far too dense without Blender decimation.");
            report.ArtVerdicts.Add("Unity-authored blue energy, water glow, point lights and pulse animation are the recommended way to own fragile VFX instead of baking water/rune effects into Meshy geometry.");
        }

        private static string WriteReport(ValidationReport report)
        {
            var reportPath = Path.Combine(GetRepoRoot(), ReportRelativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(reportPath) ?? GetRepoRoot());

            var lines = new List<string>
            {
                "# MYB-95 Meshy Relic Fountain Unity Import",
                "GeneratedAt: " + DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture),
                "UnityVersion: " + Application.unityVersion,
                "Project: unity/Echapee4D",
                "Scene: " + ScenePath,
                "Prefab: " + PrefabPath,
                "Status: " + (report.Failures.Count == 0 ? "PASS" : "FAIL"),
                ""
            };

            AddSection(lines, "Imported files", report.ImportedFiles);
            AddSection(lines, "Texture dimensions", report.TextureDimensions);
            AddSection(lines, "Generated materials", report.Materials.Distinct(StringComparer.Ordinal));
            AddSection(lines, "Generated prefabs", report.Prefabs);
            AddSection(lines, "Model metrics", report.ModelMetrics);
            AddSection(lines, "Validation scene objects", report.SceneObjects);
            AddSection(lines, "Enabled build scenes", report.EnabledBuildScenes);
            AddSection(lines, "Verdict Artistique / Technique", report.ArtVerdicts);
            AddSection(lines, "Failures", report.Failures);
            AddSection(lines, "Notes", report.Notes.Concat(new[]
            {
                "MYB-95 relic scene is a sandbox proof and is intentionally not added to Build Settings.",
                string.IsNullOrWhiteSpace(report.CapturePath) ? "Capture: not produced." : "Capture: " + report.CapturePath
            }));

            File.WriteAllLines(reportPath, lines);
            return ReportRelativePath;
        }

        private static IEnumerable<string> SourceAssetPaths()
        {
            foreach (var modelPath in ModelPaths())
            {
                yield return modelPath;
            }
            foreach (var texturePath in TexturePaths())
            {
                yield return texturePath;
            }
        }

        private static IEnumerable<string> ModelPaths()
        {
            yield return HeroModelPath;
            yield return Lod0ModelPath;
            yield return Lod1ModelPath;
        }

        private static IEnumerable<string> TexturePaths()
        {
            yield return BaseColorPath;
            yield return MetallicPath;
            yield return RoughnessPath;
            yield return NormalPath;
            yield return EmissionPath;
        }

        private static Texture2D LoadTexture(string assetPath, ValidationReport report)
        {
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
            if (texture == null)
            {
                report.Failures.Add("Texture missing: " + assetPath);
            }
            return texture;
        }

        private static int CountTrianglesInAsset(string assetPath)
        {
            return AssetDatabase.LoadAllAssetsAtPath(assetPath)
                .OfType<Mesh>()
                .Sum(CountTriangles);
        }

        private static int CountTrianglesInHierarchy(GameObject gameObject)
        {
            return gameObject.GetComponentsInChildren<MeshFilter>()
                .Where(filter => filter.sharedMesh != null)
                .Sum(filter => CountTriangles(filter.sharedMesh));
        }

        private static int CountTriangles(Mesh mesh)
        {
            var triangleCount = 0;
            for (var submesh = 0; submesh < mesh.subMeshCount; submesh++)
            {
                triangleCount += (int)(mesh.GetIndexCount(submesh) / 3);
            }
            return triangleCount;
        }

        private static Bounds GetWorldBounds(GameObject gameObject)
        {
            var renderers = gameObject.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0)
            {
                return new Bounds(gameObject.transform.position, Vector3.one);
            }

            var bounds = renderers[0].bounds;
            for (var index = 1; index < renderers.Length; index++)
            {
                bounds.Encapsulate(renderers[index].bounds);
            }
            return bounds;
        }

        private static void AssignMaterial(GameObject gameObject, Material material)
        {
            if (material == null)
            {
                return;
            }

            foreach (var renderer in gameObject.GetComponentsInChildren<Renderer>())
            {
                renderer.sharedMaterial = material;
            }
        }

        private static void RemoveCollider(GameObject gameObject)
        {
            var collider = gameObject.GetComponent<Collider>();
            if (collider != null)
            {
                UnityEngine.Object.DestroyImmediate(collider);
            }
        }

        private static void SetTexture(Material material, string property, Texture texture)
        {
            if (texture != null && material.HasProperty(property))
            {
                material.SetTexture(property, texture);
            }
        }

        private static void SetColor(Material material, string property, Color color)
        {
            if (material.HasProperty(property))
            {
                material.SetColor(property, color);
            }
        }

        private static void SetFloat(Material material, string property, float value)
        {
            if (material.HasProperty(property))
            {
                material.SetFloat(property, value);
            }
        }

        private static void AddSection(ICollection<string> lines, string title, IEnumerable<string> values)
        {
            lines.Add("## " + title);
            var list = values.Where(value => !string.IsNullOrWhiteSpace(value)).ToArray();
            if (list.Length == 0)
            {
                lines.Add("- none");
            }
            else
            {
                foreach (var value in list)
                {
                    lines.Add("- " + value);
                }
            }
            lines.Add("");
        }

        private static void CreateFolderRecursive(string assetPath)
        {
            var parts = assetPath.Split('/');
            var current = parts[0];
            for (var index = 1; index < parts.Length; index++)
            {
                var next = current + "/" + parts[index];
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[index]);
                }
                current = next;
            }
        }

        private static string ProjectRelativeToAbsolute(string assetPath)
        {
            var projectRoot = Directory.GetParent(Application.dataPath);
            return Path.Combine(projectRoot == null ? Application.dataPath : projectRoot.FullName, assetPath);
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

        private static string FormatBytes(long bytes)
        {
            if (bytes < 1024)
            {
                return bytes + " B";
            }
            if (bytes < 1024 * 1024)
            {
                return (bytes / 1024f).ToString("0.0", CultureInfo.InvariantCulture) + " KB";
            }
            return (bytes / (1024f * 1024f)).ToString("0.0", CultureInfo.InvariantCulture) + " MB";
        }

        private sealed class ValidationReport
        {
            public List<string> ImportedFiles { get; } = new List<string>();
            public List<string> TextureDimensions { get; } = new List<string>();
            public List<string> Materials { get; } = new List<string>();
            public List<string> Prefabs { get; } = new List<string>();
            public List<string> ModelMetrics { get; } = new List<string>();
            public List<string> SceneObjects { get; } = new List<string>();
            public List<string> EnabledBuildScenes { get; } = new List<string>();
            public List<string> ArtVerdicts { get; } = new List<string>();
            public List<string> Failures { get; } = new List<string>();
            public List<string> Notes { get; } = new List<string>();
            public string CapturePath { get; set; }
        }
    }
}

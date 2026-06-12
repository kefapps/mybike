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
    public static class MYB95MeshyLanternUnityImporter
    {
        private const string ScenePath = "Assets/Scenes/MYB95MeshyLanternPoc.unity";
        private const string CanonicalScenePath = "Assets/Scenes/MYB89UnityMcpProbe.unity";
        private const string Root = "Assets/Echappee/Art/MYB95MeshyLantern";
        private const string ModelPath = Root + "/Models/MYB95_MeshyLantern_LOD0_20k.fbx";
        private const string BaseColorPath = Root + "/Textures/refined_base_color.png";
        private const string MetallicPath = Root + "/Textures/refined_metallic.png";
        private const string RoughnessPath = Root + "/Textures/refined_roughness.png";
        private const string NormalPath = Root + "/Textures/refined_normal.png";
        private const string EmissionPath = Root + "/Textures/refined_emission.png";
        private const string LanternMaterialPath = Root + "/Materials/MYB95_MeshyLantern_PBR.mat";
        private const string FlameMaterialPath = Root + "/Materials/MYB95_FlameGlow.mat";
        private const string RuneMaterialPath = Root + "/Materials/MYB95_BlueRuneGlow.mat";
        private const string PrefabPath = Root + "/Prefabs/MYB95_MeshyLantern.prefab";
        private const string ReportRelativePath = "_bmad-output/unity-test-results/myb-95-meshy-lantern-unity-import.txt";
        private const string CaptureRelativePath = "_bmad-output/unity-test-results/myb-95-meshy-lantern-unity-import.png";

        private const float TargetHeightMeters = 1.45f;
        private const int MaxModelTriangles = 25000;

        [MenuItem("Tools/MYB-95/Build Meshy Lantern Unity Import")]
        public static void BuildAndValidateFromMenu()
        {
            var reportPath = BuildAndValidate();
            Debug.Log("MYB-95 Meshy lantern Unity import complete. Report: " + reportPath);
        }

        public static void BuildAndValidateCli()
        {
            var reportPath = BuildAndValidate();
            var reportText = File.ReadAllText(Path.Combine(GetRepoRoot(), reportPath));
            if (reportText.Contains("Status: FAIL", StringComparison.Ordinal))
            {
                throw new InvalidOperationException("MYB-95 Meshy lantern Unity import failed. See " + reportPath);
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

                var lanternMaterial = CreateLanternMaterial(report);
                var flameMaterial = CreateEmissiveMaterial(
                    FlameMaterialPath,
                    new Color(1f, 0.60f, 0.22f, 1f),
                    2.4f,
                    report);
                var runeMaterial = CreateRuneMaterial(report);

                CreatePrefab(lanternMaterial, flameMaterial, runeMaterial, report);

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

                var info = new FileInfo(absolutePath);
                report.ImportedFiles.Add(assetPath + " (" + FormatBytes(info.Length) + ")");
            }
        }

        private static void ConfigureImporters(ValidationReport report)
        {
            ConfigureModelImporter(report);
            ConfigureTexture(BaseColorPath, TextureImporterType.Default, true, 2048, report);
            ConfigureTexture(MetallicPath, TextureImporterType.Default, false, 2048, report);
            ConfigureTexture(RoughnessPath, TextureImporterType.Default, false, 2048, report);
            ConfigureTexture(NormalPath, TextureImporterType.NormalMap, false, 2048, report);
            ConfigureTexture(EmissionPath, TextureImporterType.Default, true, 2048, report);
        }

        private static void ConfigureModelImporter(ValidationReport report)
        {
            var importer = AssetImporter.GetAtPath(ModelPath) as ModelImporter;
            if (importer == null)
            {
                report.Failures.Add("Model importer missing: " + ModelPath);
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

        private static Material CreateLanternMaterial(ValidationReport report)
        {
            var material = MaterialAt(LanternMaterialPath, report);
            if (material == null)
            {
                return null;
            }

            var baseColor = LoadTexture(BaseColorPath, report);
            var metallic = LoadTexture(MetallicPath, report);
            var normal = LoadTexture(NormalPath, report);
            var emission = LoadTexture(EmissionPath, report);

            SetTexture(material, "_BaseMap", baseColor);
            SetTexture(material, "_MainTex", baseColor);
            SetTexture(material, "_MetallicGlossMap", metallic);
            SetTexture(material, "_BumpMap", normal);
            SetTexture(material, "_EmissionMap", emission);
            SetColor(material, "_BaseColor", Color.white);
            SetColor(material, "_Color", Color.white);
            SetColor(material, "_EmissionColor", new Color(1f, 0.66f, 0.30f, 1f) * 1.35f);
            SetFloat(material, "_Metallic", 0.18f);
            SetFloat(material, "_Smoothness", 0.58f);
            SetFloat(material, "_BumpScale", 0.42f);
            material.EnableKeyword("_NORMALMAP");
            material.EnableKeyword("_METALLICSPECGLOSSMAP");
            material.EnableKeyword("_EMISSION");
            material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            EditorUtility.SetDirty(material);

            report.Materials.Add(LanternMaterialPath);
            report.Notes.Add("Roughness map imported and kept next to the asset; Unity material uses a fixed smoothness until we pack metallic+smoothness for production.");
            return material;
        }

        private static Material CreateEmissiveMaterial(string assetPath, Color color, float emissionStrength, ValidationReport report)
        {
            var material = MaterialAt(assetPath, report);
            if (material == null)
            {
                return null;
            }

            SetColor(material, "_BaseColor", color);
            SetColor(material, "_Color", color);
            SetColor(material, "_EmissionColor", color * emissionStrength);
            SetFloat(material, "_Metallic", 0f);
            SetFloat(material, "_Smoothness", 0.34f);
            material.EnableKeyword("_EMISSION");
            material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            EditorUtility.SetDirty(material);

            report.Materials.Add(assetPath);
            return material;
        }

        private static Material CreateRuneMaterial(ValidationReport report)
        {
            var shader = Shader.Find("Universal Render Pipeline/Unlit") ??
                Shader.Find("Unlit/Color") ??
                Shader.Find("Universal Render Pipeline/Lit") ??
                Shader.Find("Standard");
            if (shader == null)
            {
                report.Failures.Add("No compatible shader found for rune material.");
                return null;
            }

            var material = AssetDatabase.LoadAssetAtPath<Material>(RuneMaterialPath);
            if (material == null)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, RuneMaterialPath);
            }
            else
            {
                material.shader = shader;
            }

            var blue = new Color(0.08f, 0.45f, 1f, 1f);
            SetColor(material, "_BaseColor", blue);
            SetColor(material, "_Color", blue);
            SetColor(material, "_EmissionColor", blue * 1.25f);
            SetFloat(material, "_Metallic", 0f);
            SetFloat(material, "_Smoothness", 0.18f);
            material.EnableKeyword("_EMISSION");
            material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            EditorUtility.SetDirty(material);

            report.Materials.Add(RuneMaterialPath);
            return material;
        }

        private static Material MaterialAt(string assetPath, ValidationReport report)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            if (shader == null)
            {
                report.Failures.Add("No compatible Lit shader found.");
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

        private static void CreatePrefab(Material lanternMaterial, Material flameMaterial, Material runeMaterial, ValidationReport report)
        {
            AssetDatabase.DeleteAsset(PrefabPath);

            var model = AssetDatabase.LoadAssetAtPath<GameObject>(ModelPath);
            if (model == null)
            {
                report.Failures.Add("Could not load model asset: " + ModelPath);
                return;
            }

            var root = new GameObject("MYB95_MeshyLantern");
            var modelInstance = PrefabUtility.InstantiatePrefab(model) as GameObject;
            if (modelInstance == null)
            {
                UnityEngine.Object.DestroyImmediate(root);
                report.Failures.Add("Could not instantiate model asset: " + ModelPath);
                return;
            }

            modelInstance.name = "MYB95_LanternModel";
            modelInstance.transform.SetParent(root.transform, false);
            AssignMaterial(modelInstance, lanternMaterial);
            FitModelToHeight(modelInstance, TargetHeightMeters);

            var fittedBounds = GetWorldBounds(modelInstance);
            CreateFlameGlow(root.transform, flameMaterial, fittedBounds);
            CreateRuneGlow(root.transform, runeMaterial, fittedBounds);

            var lightObject = new GameObject("MYB95_AnimatedFlameLight");
            lightObject.transform.SetParent(root.transform, false);
            lightObject.transform.localPosition = new Vector3(0f, fittedBounds.size.y * 0.74f, 0f);
            var pointLight = lightObject.AddComponent<Light>();
            pointLight.type = LightType.Point;
            pointLight.color = new Color(1f, 0.62f, 0.26f, 1f);
            pointLight.intensity = 1.65f;
            pointLight.range = 3.15f;
            pointLight.shadows = LightShadows.None;

            var flicker = root.AddComponent<MYB95LanternFlicker>();
            var emissiveRenderers = root.GetComponentsInChildren<Renderer>();
            flicker.Configure(pointLight, emissiveRenderers);

            var savedPrefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            UnityEngine.Object.DestroyImmediate(root);

            if (savedPrefab == null)
            {
                report.Failures.Add("Failed to save prefab: " + PrefabPath);
                return;
            }

            report.Prefabs.Add(PrefabPath);
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

        private static void CreateFlameGlow(Transform parent, Material material, Bounds fittedBounds)
        {
            var glow = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            glow.name = "MYB95_FlameEmissiveCore";
            glow.transform.SetParent(parent, false);
            glow.transform.localPosition = new Vector3(0f, fittedBounds.size.y * 0.74f, 0f);
            glow.transform.localScale = Vector3.one * 0.085f;
            AssignMaterial(glow, material);
            RemoveCollider(glow);
        }

        private static void CreateRuneGlow(Transform parent, Material material, Bounds fittedBounds)
        {
            var rune = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rune.name = "MYB95_BlueRuneGlow";
            rune.transform.SetParent(parent, false);
            rune.transform.localPosition = new Vector3(0f, fittedBounds.size.y * 0.32f, -fittedBounds.extents.z - 0.012f);
            rune.transform.localRotation = Quaternion.Euler(0f, 0f, 45f);
            rune.transform.localScale = new Vector3(0.062f, 0.062f, 0.010f);
            AssignMaterial(rune, material);
            RemoveCollider(rune);
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

            instance.name = "MYB95_MeshyLantern_SceneProof";
            instance.transform.SetPositionAndRotation(new Vector3(-1.02f, 0f, 0.7f), Quaternion.Euler(0f, 18f, 0f));
            report.SceneObjects.Add(instance.name);

            var second = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            if (second != null)
            {
                second.name = "MYB95_MeshyLantern_DistanceCheck";
                second.transform.SetPositionAndRotation(new Vector3(1.18f, 0f, 4.45f), Quaternion.Euler(0f, -28f, 0f));
                second.transform.localScale = Vector3.one * 0.82f;
                report.SceneObjects.Add(second.name);
            }
        }

        private static void ConfigureRenderSettings()
        {
            RenderSettings.skybox = null;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.56f, 0.66f, 0.72f);
            RenderSettings.ambientEquatorColor = new Color(0.33f, 0.31f, 0.28f);
            RenderSettings.ambientGroundColor = new Color(0.17f, 0.15f, 0.13f);
            RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(0.32f, 0.39f, 0.41f);
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogStartDistance = 10f;
            RenderSettings.fogEndDistance = 32f;
        }

        private static void CreateCamera()
        {
            var cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(0.10f, 1.18f, -3.85f);
            cameraObject.transform.rotation = Quaternion.Euler(10.5f, 0f, 0f);
            var camera = cameraObject.AddComponent<Camera>();
            camera.fieldOfView = 45f;
            camera.nearClipPlane = 0.03f;
            camera.farClipPlane = 80f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.27f, 0.34f, 0.37f);
        }

        private static void CreateSun()
        {
            var sunObject = new GameObject("MYB95_LowWarmSun");
            sunObject.transform.rotation = Quaternion.Euler(34f, -32f, 0f);
            var sun = sunObject.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.color = new Color(1f, 0.83f, 0.62f);
            sun.intensity = 0.58f;
            sun.shadows = LightShadows.Soft;
        }

        private static void CreateValidationGround()
        {
            var paving = CreateMaterial("MYB95_ValidationPaving", new Color(0.50f, 0.42f, 0.34f), 0.16f);
            var grass = CreateMaterial("MYB95_ValidationGrass", new Color(0.22f, 0.36f, 0.26f), 0.05f);
            var edge = CreateMaterial("MYB95_ValidationRoadEdge", new Color(0.72f, 0.58f, 0.38f), 0.12f);

            CreateCube("MYB95_VillageGround", new Vector3(0f, -0.08f, 1.7f), new Vector3(7.4f, 0.10f, 8.5f), grass);
            CreateCube("MYB95_PavedRoadProof", new Vector3(0f, -0.02f, 1.7f), new Vector3(1.55f, 0.08f, 8.3f), paving);
            CreateCube("MYB95_LeftRoadLip", new Vector3(-0.83f, 0.03f, 1.7f), new Vector3(0.08f, 0.10f, 8.3f), edge);
            CreateCube("MYB95_RightRoadLip", new Vector3(0.83f, 0.03f, 1.7f), new Vector3(0.08f, 0.10f, 8.3f), edge);
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
            var modelTriangles = CountTrianglesInAsset(ModelPath);
            report.ModelMetrics.Add("Model triangles: " + modelTriangles.ToString(CultureInfo.InvariantCulture));
            if (modelTriangles <= 0)
            {
                report.Failures.Add("No mesh triangles found in model asset.");
            }
            else if (modelTriangles > MaxModelTriangles)
            {
                report.Failures.Add("Model triangle count exceeds MYB-95 budget: " + modelTriangles);
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
                var renderers = instance.GetComponentsInChildren<Renderer>();
                var pointLight = instance.GetComponentsInChildren<Light>().FirstOrDefault(light => light.type == LightType.Point);
                var flicker = instance.GetComponent<MYB95LanternFlicker>();

                report.ModelMetrics.Add("Prefab renderers: " + renderers.Length.ToString(CultureInfo.InvariantCulture));
                report.ModelMetrics.Add("Prefab total triangles: " + CountTrianglesInHierarchy(instance).ToString(CultureInfo.InvariantCulture));

                if (renderers.Length < 3)
                {
                    report.Failures.Add("Prefab should contain model, flame glow and rune glow renderers.");
                }

                if (pointLight == null)
                {
                    report.Failures.Add("Prefab missing animated Point Light.");
                }

                if (flicker == null)
                {
                    report.Failures.Add("Prefab missing MYB95LanternFlicker component.");
                }
                else if (pointLight != null)
                {
                    flicker.ApplyPreview(0.13f);
                    var sampleA = pointLight.intensity;
                    flicker.ApplyPreview(1.07f);
                    var sampleB = pointLight.intensity;
                    report.ModelMetrics.Add("Flicker intensity sample: " + sampleA.ToString("0.000", CultureInfo.InvariantCulture) + " -> " + sampleB.ToString("0.000", CultureInfo.InvariantCulture));

                    if (Mathf.Abs(sampleA - sampleB) < 0.015f)
                    {
                        report.Failures.Add("Flicker script did not vary Point Light intensity.");
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
                report.Failures.Add("MYB-95 POC scene should not be added to Build Settings.");
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
                report.Failures.Add("POC scene should contain close and distance lantern proof instances.");
            }

            if (string.IsNullOrWhiteSpace(report.CapturePath))
            {
                report.Failures.Add("POC capture was not produced.");
            }
        }

        private static void AddVerdict(ValidationReport report)
        {
            report.ArtVerdicts.Add("Blender 20k import is the current preferred path: acceptable silhouette, compact FBX, real Unity Point Light flicker, and reusable prefab proof.");
            report.ArtVerdicts.Add("The generated asset still needs art direction cleanup before production: warm limestone and blue rune are supported by Unity-side materials/children, not fully solved by the Meshy mesh.");
            report.ArtVerdicts.Add("The roughness map is kept but not packed into Unity metallic/smoothness yet; production import should add texture packing or a custom material workflow.");
        }

        private static string WriteReport(ValidationReport report)
        {
            var reportPath = Path.Combine(GetRepoRoot(), ReportRelativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(reportPath) ?? GetRepoRoot());

            var lines = new List<string>
            {
                "# MYB-95 Meshy Lantern Unity Import",
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
                "MYB-95 scene is a sandbox proof and is intentionally not added to Build Settings.",
                string.IsNullOrWhiteSpace(report.CapturePath) ? "Capture: not produced." : "Capture: " + report.CapturePath
            }));

            File.WriteAllLines(reportPath, lines);
            return ReportRelativePath;
        }

        private static IEnumerable<string> SourceAssetPaths()
        {
            yield return ModelPath;
            foreach (var texturePath in TexturePaths())
            {
                yield return texturePath;
            }
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
                .Sum(mesh => CountTriangles(mesh));
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

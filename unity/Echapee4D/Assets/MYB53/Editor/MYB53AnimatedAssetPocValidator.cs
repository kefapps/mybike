using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MYB53.Editor
{
    public static class MYB53AnimatedAssetPocValidator
    {
        private const string ScenePath = "Assets/Scenes/MYB53AnimatedAssetPoc.unity";
        private const string CanonicalScenePath = "Assets/Scenes/MYB89UnityMcpProbe.unity";
        private const string MaterialsRoot = "Assets/Echappee/Art/MYB53Validation/Materials";
        private const string AnimationsRoot = "Assets/Echappee/Art/MYB53Validation/Animations";
        private const string KayKitRoot = "Assets/Echappee/Art/ThirdParty/KayKit/MedievalHexagonPack";
        private const string HorseRoot = "Assets/Echappee/Art/ThirdParty/Quaternius/HorseAnimated";
        private const string KayKitAtlasPath = KayKitRoot + "/Textures/hexagons_medieval.png";
        private const string PavingColorPath = "Assets/Echappee/Art/ThirdParty/ambientCG/PavingStones141/Textures/PavingStones141_1K-JPG_Color.jpg";
        private const string PavingNormalPath = "Assets/Echappee/Art/ThirdParty/ambientCG/PavingStones141/Textures/PavingStones141_1K-JPG_NormalGL.jpg";
        private const string HorsePath = HorseRoot + "/Models/horse_animated.fbx";
        private const string HorseControllerPath = AnimationsRoot + "/MYB53_HorseIdle.controller";
        private const string ReportRelativePath = "_bmad-output/unity-test-results/myb-53-animated-asset-poc.txt";
        private const string CaptureRelativePath = "_bmad-output/unity-test-results/myb-53-animated-asset-poc.png";

        private static readonly AssetSpec[] AssetSpecs =
        {
            new AssetSpec(
                "kaykit-medieval-hexagon-pack-myb53-subset",
                "KayKit Medieval Hexagon subset",
                KayKitRoot,
                new[]
                {
                    KayKitRoot + "/Models/building_home_A_yellow.fbx",
                    KayKitRoot + "/Models/building_market_yellow.fbx",
                    KayKitRoot + "/Models/building_well_yellow.fbx",
                    KayKitRoot + "/Models/fence_stone_straight.fbx",
                    KayKitRoot + "/Models/barrel.fbx",
                    KayKitAtlasPath,
                    KayKitRoot + "/License/LICENSE.txt",
                    KayKitRoot + "/License/README.md",
                }),
            new AssetSpec(
                "quaternius-horse-animated-myb53",
                "Quaternius animated horse",
                HorseRoot,
                new[]
                {
                    HorsePath,
                    HorseRoot + "/License/SOURCE.txt",
                })
        };

        [MenuItem("Tools/MYB-53/Build Animated Asset POC Scene")]
        public static void BuildAndValidateFromMenu()
        {
            var reportPath = BuildAndValidate();
            Debug.Log("MYB-53 animated asset POC complete. Report: " + reportPath);
        }

        public static void BuildAndValidateCli()
        {
            var reportPath = BuildAndValidate();
            var reportText = File.ReadAllText(Path.Combine(GetRepoRoot(), reportPath));
            if (reportText.Contains("Status: FAIL", StringComparison.Ordinal))
            {
                throw new InvalidOperationException("MYB-53 animated asset POC failed. See " + reportPath);
            }
        }

        public static string BuildAndValidate()
        {
            var report = new ValidationReport();

            try
            {
                AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
                EnsureFolders();
                ConfigureImporters(report);

                var kayKitMaterial = CreateLitMaterial(
                    "MYB53_KayKitAtlas",
                    KayKitAtlasPath,
                    null,
                    new Color(1f, 0.93f, 0.82f),
                    0.28f,
                    report);

                var pavingMaterial = CreateLitMaterial(
                    "MYB53_PavingStones141_Stylized",
                    PavingColorPath,
                    PavingNormalPath,
                    new Color(0.76f, 0.60f, 0.45f),
                    0.06f,
                    report);

                var grassMaterial = CreateColorMaterial("MYB53_SoftVillageGrass", new Color(0.43f, 0.59f, 0.34f), 0.22f, report);
                var roadEdgeMaterial = CreateColorMaterial("MYB53_WarmRoadEdge", new Color(0.82f, 0.62f, 0.28f), 0.12f, report);
                var accentMaterial = CreateColorMaterial("MYB53_RelicWarmLight", new Color(1.0f, 0.70f, 0.34f), 0.12f, report, true);

                var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                BuildPocScene(kayKitMaterial, pavingMaterial, grassMaterial, roadEdgeMaterial, accentMaterial, report);
                EditorSceneManager.SaveScene(scene, ScenePath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

                CaptureScenePreview(report);
                ValidateManifest(report);
                ValidateAssetFiles(report);
                ValidateTextureBudgets(report);
                ValidateAnimationImport(report);
                ValidateCanonicalBaselineBoundary(report);
                ValidateScene(report);
                AddArtVerdicts(report);
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
            CreateFolderRecursive(MaterialsRoot);
            CreateFolderRecursive(AnimationsRoot);
        }

        private static void ConfigureImporters(ValidationReport report)
        {
            ConfigureTexture(KayKitAtlasPath, TextureImporterType.Default, true, report);
            ConfigureTexture(PavingColorPath, TextureImporterType.Default, true, report);
            ConfigureTexture(PavingNormalPath, TextureImporterType.NormalMap, false, report);
            ConfigureHorseImporter(report);
        }

        private static void ConfigureTexture(string assetPath, TextureImporterType type, bool srgb, ValidationReport report)
        {
            var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null)
            {
                report.Failures.Add("Texture importer missing: " + assetPath);
                return;
            }

            var changed = importer.textureType != type ||
                importer.sRGBTexture != srgb ||
                importer.maxTextureSize != 1024 ||
                importer.textureCompression != TextureImporterCompression.CompressedHQ;

            importer.textureType = type;
            importer.sRGBTexture = srgb;
            importer.maxTextureSize = 1024;
            importer.textureCompression = TextureImporterCompression.CompressedHQ;

            if (changed)
            {
                importer.SaveAndReimport();
            }
        }

        private static void ConfigureHorseImporter(ValidationReport report)
        {
            var importer = AssetImporter.GetAtPath(HorsePath) as ModelImporter;
            if (importer == null)
            {
                report.Failures.Add("Horse model importer missing: " + HorsePath);
                return;
            }

            var changed = !importer.importAnimation ||
                importer.animationType != ModelImporterAnimationType.Generic ||
                importer.materialImportMode != ModelImporterMaterialImportMode.None ||
                importer.globalScale > 1.01f ||
                importer.globalScale < 0.99f;

            importer.importAnimation = true;
            importer.animationType = ModelImporterAnimationType.Generic;
            importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
            importer.materialImportMode = ModelImporterMaterialImportMode.None;
            importer.globalScale = 1f;

            if (changed)
            {
                importer.SaveAndReimport();
            }
        }

        private static Material CreateLitMaterial(
            string materialName,
            string colorPath,
            string normalPath,
            Color tint,
            float smoothness,
            ValidationReport report)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            if (shader == null)
            {
                report.Failures.Add("No compatible Lit shader found.");
                return null;
            }

            var materialPath = MaterialsRoot + "/" + materialName + ".mat";
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

            var color = string.IsNullOrWhiteSpace(colorPath) ? null : AssetDatabase.LoadAssetAtPath<Texture2D>(colorPath);
            var normal = string.IsNullOrWhiteSpace(normalPath) ? null : AssetDatabase.LoadAssetAtPath<Texture2D>(normalPath);
            if (!string.IsNullOrWhiteSpace(colorPath) && color == null)
            {
                report.Failures.Add("Color texture missing: " + colorPath);
            }
            if (!string.IsNullOrWhiteSpace(normalPath) && normal == null)
            {
                report.Failures.Add("Normal texture missing: " + normalPath);
            }

            SetTexture(material, "_BaseMap", color);
            SetTexture(material, "_MainTex", color);
            SetTexture(material, "_BumpMap", normal);
            SetColor(material, "_BaseColor", tint);
            SetColor(material, "_Color", tint);
            SetFloat(material, "_BumpScale", normal == null ? 0f : 0.45f);
            SetFloat(material, "_Metallic", 0f);
            SetFloat(material, "_Smoothness", smoothness);
            EditorUtility.SetDirty(material);
            report.Materials.Add(materialPath);
            return material;
        }

        private static Material CreateColorMaterial(string materialName, Color color, float smoothness, ValidationReport report, bool emissive = false)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            if (shader == null)
            {
                report.Failures.Add("No compatible Lit shader found.");
                return null;
            }

            var materialPath = MaterialsRoot + "/" + materialName + ".mat";
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
            if (emissive)
            {
                material.EnableKeyword("_EMISSION");
                SetColor(material, "_EmissionColor", color * 1.6f);
            }
            EditorUtility.SetDirty(material);
            report.Materials.Add(materialPath);
            return material;
        }

        private static void BuildPocScene(
            Material kayKitMaterial,
            Material pavingMaterial,
            Material grassMaterial,
            Material roadEdgeMaterial,
            Material accentMaterial,
            ValidationReport report)
        {
            ConfigureRenderSettings();
            CreateCamera();
            CreateLight();
            CreateGround(grassMaterial, pavingMaterial, roadEdgeMaterial);

            InstantiateModel(
                "MYB53 Village Home",
                KayKitRoot + "/Models/building_home_A_yellow.fbx",
                new Vector3(-2.9f, 0.05f, 1.2f),
                new Vector3(0f, 23f, 0f),
                1.7f,
                kayKitMaterial,
                report);

            InstantiateModel(
                "MYB53 Village Market",
                KayKitRoot + "/Models/building_market_yellow.fbx",
                new Vector3(3.0f, 0.05f, 2.45f),
                new Vector3(0f, -32f, 0f),
                1.65f,
                kayKitMaterial,
                report);

            InstantiateModel(
                "MYB53 Village Well",
                KayKitRoot + "/Models/building_well_yellow.fbx",
                new Vector3(1.75f, 0.05f, -0.4f),
                new Vector3(0f, -12f, 0f),
                0.92f,
                kayKitMaterial,
                report);

            InstantiateModel(
                "MYB53 Stone Fence Left",
                KayKitRoot + "/Models/fence_stone_straight.fbx",
                new Vector3(-1.85f, 0.05f, -1.9f),
                new Vector3(0f, 8f, 0f),
                0.42f,
                kayKitMaterial,
                report);

            InstantiateModel(
                "MYB53 Barrel Roadside",
                KayKitRoot + "/Models/barrel.fbx",
                new Vector3(2.18f, 0.05f, -2.05f),
                new Vector3(0f, -18f, 0f),
                0.35f,
                kayKitMaterial,
                report);

            var horse = InstantiateModel(
                "MYB53 Animated Horse Accent",
                HorsePath,
                new Vector3(-2.05f, 0.05f, 3.0f),
                new Vector3(0f, 120f, 0f),
                1.05f,
                null,
                report);

            ConfigureHorseAnimator(horse, report);
            CreateAccentLight(new Vector3(1.75f, 1.25f, -0.4f), accentMaterial, "MYB53 Well Warm Relic");
            CreateAccentLight(new Vector3(-2.2f, 1.85f, 1.35f), accentMaterial, "MYB53 Home Window Glow");
        }

        private static void ConfigureRenderSettings()
        {
            RenderSettings.skybox = null;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.52f, 0.57f, 0.60f);
            RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(0.76f, 0.82f, 0.83f);
            RenderSettings.fogDensity = 0.015f;
        }

        private static void CreateCamera()
        {
            var cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(0f, 2.15f, -6.6f);
            cameraObject.transform.rotation = Quaternion.Euler(13.5f, 0f, 0f);
            var camera = cameraObject.AddComponent<Camera>();
            camera.fieldOfView = 45f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.65f, 0.78f, 0.83f);
        }

        private static void CreateLight()
        {
            var lightObject = new GameObject("MYB53 Low Sun Key Light");
            lightObject.transform.rotation = Quaternion.Euler(36f, -28f, 0f);
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 0.92f;
            light.color = new Color(1f, 0.86f, 0.68f);
        }

        private static void CreateGround(Material grassMaterial, Material pavingMaterial, Material roadEdgeMaterial)
        {
            CreateCube("MYB53 Soft Village Ground", new Vector3(0f, -0.06f, 0.8f), new Vector3(8.8f, 0.08f, 10.8f), grassMaterial);
            CreateCube("MYB53 Paved Ride Surface", new Vector3(0f, 0.0f, 0.2f), new Vector3(1.76f, 0.05f, 9.5f), pavingMaterial);
            CreateCube("MYB53 Left Warm Road Edge", new Vector3(-0.98f, 0.035f, 0.2f), new Vector3(0.05f, 0.04f, 9.5f), roadEdgeMaterial);
            CreateCube("MYB53 Right Warm Road Edge", new Vector3(0.98f, 0.035f, 0.2f), new Vector3(0.05f, 0.04f, 9.5f), roadEdgeMaterial);
        }

        private static GameObject InstantiateModel(
            string name,
            string assetPath,
            Vector3 position,
            Vector3 eulerAngles,
            float targetHeight,
            Material materialOverride,
            ValidationReport report)
        {
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (model == null)
            {
                report.Failures.Add("Model asset missing or failed to import: " + assetPath);
                return null;
            }

            var instance = PrefabUtility.InstantiatePrefab(model) as GameObject;
            if (instance == null)
            {
                report.Failures.Add("Could not instantiate model asset: " + assetPath);
                return null;
            }

            instance.name = name;
            instance.transform.position = position;
            instance.transform.rotation = Quaternion.Euler(eulerAngles);
            FitToHeight(instance, targetHeight);

            if (materialOverride != null)
            {
                foreach (var renderer in instance.GetComponentsInChildren<Renderer>())
                {
                    renderer.sharedMaterial = materialOverride;
                }
            }

            report.SceneObjects.Add(name);
            return instance;
        }

        private static void FitToHeight(GameObject instance, float targetHeight)
        {
            var renderers = instance.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0)
            {
                instance.transform.localScale = Vector3.one * targetHeight;
                return;
            }

            var bounds = renderers[0].bounds;
            foreach (var renderer in renderers.Skip(1))
            {
                bounds.Encapsulate(renderer.bounds);
            }

            if (bounds.size.y <= 0.001f)
            {
                return;
            }

            instance.transform.localScale *= targetHeight / bounds.size.y;
        }

        private static void ConfigureHorseAnimator(GameObject horse, ValidationReport report)
        {
            if (horse == null)
            {
                return;
            }

            var clips = LoadHorseClips(report);
            var idleClip = clips.FirstOrDefault(clip => clip.name.IndexOf("Idle", StringComparison.OrdinalIgnoreCase) >= 0) ??
                clips.FirstOrDefault();

            if (idleClip == null)
            {
                report.Failures.Add("No usable horse animation clip found.");
                return;
            }

            AssetDatabase.DeleteAsset(HorseControllerPath);
            var controller = AnimatorController.CreateAnimatorControllerAtPath(HorseControllerPath);
            var state = controller.AddMotion(idleClip);
            state.name = "MYB53 Horse " + idleClip.name;
            state.speed = 1f;
            AssetDatabase.SaveAssets();

            var animator = horse.GetComponent<Animator>() ?? horse.AddComponent<Animator>();
            animator.runtimeAnimatorController = controller;
            animator.applyRootMotion = false;
            report.AnimationController = HorseControllerPath;
        }

        private static AnimationClip[] LoadHorseClips(ValidationReport report)
        {
            var clips = AssetDatabase.LoadAllAssetsAtPath(HorsePath)
                .OfType<AnimationClip>()
                .Where(clip => clip != null && !clip.name.StartsWith("__preview__", StringComparison.Ordinal))
                .OrderBy(clip => clip.name, StringComparer.Ordinal)
                .ToArray();

            foreach (var clip in clips)
            {
                report.AnimationClips.Add(clip.name + " (" + clip.length.ToString("0.00", CultureInfo.InvariantCulture) + "s)");
            }

            return clips;
        }

        private static void CreateAccentLight(Vector3 position, Material material, string name)
        {
            var orb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            orb.name = name;
            orb.transform.position = position;
            orb.transform.localScale = Vector3.one * 0.16f;
            if (material != null)
            {
                orb.GetComponent<Renderer>().sharedMaterial = material;
            }

            var lightObject = new GameObject(name + " Light");
            lightObject.transform.position = position;
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Point;
            light.range = 2.8f;
            light.intensity = 1.35f;
            light.color = new Color(1f, 0.69f, 0.32f);
        }

        private static void CreateCube(string name, Vector3 position, Vector3 scale, Material material)
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = name;
            cube.transform.position = position;
            cube.transform.localScale = scale;
            if (material != null)
            {
                cube.GetComponent<Renderer>().sharedMaterial = material;
            }
        }

        private static void CaptureScenePreview(ValidationReport report)
        {
            var camera = Camera.main;
            if (camera == null)
            {
                report.Failures.Add("Cannot capture MYB-53 scene: missing Main Camera.");
                return;
            }

            var absolutePath = Path.Combine(GetRepoRoot(), CaptureRelativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(absolutePath));

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

        private static void ValidateManifest(ValidationReport report)
        {
            var manifestPath = "Assets/Echappee/Art/ThirdPartyAssets.assetmanifest.json";
            var absolutePath = ProjectRelativeToAbsolute(manifestPath);
            if (!File.Exists(absolutePath))
            {
                report.Failures.Add("Unity third-party manifest missing.");
                return;
            }

            var manifest = File.ReadAllText(absolutePath);
            foreach (var spec in AssetSpecs)
            {
                if (!manifest.Contains("\"id\": \"" + spec.Id + "\"", StringComparison.Ordinal))
                {
                    report.Failures.Add("Manifest missing asset id: " + spec.Id);
                }
            }

            if (manifest.Contains("\"status\": \"needs-review\"", StringComparison.Ordinal))
            {
                report.Failures.Add("Manifest still contains needs-review assets.");
            }
        }

        private static void ValidateAssetFiles(ValidationReport report)
        {
            foreach (var spec in AssetSpecs)
            {
                foreach (var assetPath in spec.AssetPaths)
                {
                    var absolutePath = ProjectRelativeToAbsolute(assetPath);
                    if (!File.Exists(absolutePath))
                    {
                        report.Failures.Add("Missing imported file: " + assetPath);
                        continue;
                    }

                    var info = new FileInfo(absolutePath);
                    report.ImportedFiles.Add(assetPath + " (" + FormatBytes(info.Length) + ")");
                }
            }
        }

        private static void ValidateTextureBudgets(ValidationReport report)
        {
            foreach (var texturePath in new[] { KayKitAtlasPath, PavingColorPath, PavingNormalPath })
            {
                var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
                if (texture == null)
                {
                    report.Failures.Add("Texture failed to import: " + texturePath);
                    continue;
                }

                report.TextureDimensions.Add(texturePath + " (" + texture.width + "x" + texture.height + ")");
                if (texture.width > 1024 || texture.height > 1024)
                {
                    report.Failures.Add("Texture exceeds MYB-53 1K guardrail: " + texturePath);
                }
            }
        }

        private static void ValidateAnimationImport(ValidationReport report)
        {
            var clips = LoadHorseClips(report);
            if (clips.Length == 0)
            {
                report.Failures.Add("Horse FBX imported without animation clips.");
            }

            if (!clips.Any(clip => clip.name.IndexOf("Idle", StringComparison.OrdinalIgnoreCase) >= 0) ||
                !clips.Any(clip => clip.name.IndexOf("Walk", StringComparison.OrdinalIgnoreCase) >= 0))
            {
                report.Failures.Add("Horse FBX does not expose both Idle and Walk animation evidence.");
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
                report.Failures.Add("MYB-53 POC scene should not be added to Build Settings.");
            }

            report.EnabledBuildScenes.AddRange(enabledScenes);
        }

        private static void ValidateScene(ValidationReport report)
        {
            if (!File.Exists(ProjectRelativeToAbsolute(ScenePath)))
            {
                report.Failures.Add("POC scene was not saved: " + ScenePath);
                return;
            }

            if (report.SceneObjects.Count < 6)
            {
                report.Failures.Add("POC scene does not contain the expected imported model instances.");
            }

            if (string.IsNullOrWhiteSpace(report.CapturePath))
            {
                report.Failures.Add("POC capture was not produced.");
            }
        }

        private static void AddArtVerdicts(ValidationReport report)
        {
            report.ArtVerdicts.Add("KayKit Medieval Hexagon subset: support/fallback - useful for fast village composition and warm silhouettes, but still reads grid/board-game if used as the primary V1 art source without stronger material/lighting polish.");
            report.ArtVerdicts.Add("Quaternius animated horse: support/fallback - animation import is valuable and the model adds light life to the village, but it is not a premium hero asset by itself.");
            report.ArtVerdicts.Add("Paving Stones 141 reuse: support/fallback - strong rough-surface signal for Village / campagne pavee, but needs a stylization pass before final adoption.");
        }

        private static string WriteReport(ValidationReport report)
        {
            var reportPath = Path.Combine(GetRepoRoot(), ReportRelativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(reportPath));

            var lines = new List<string>
            {
                "# MYB-53 Animated Asset POC",
                "GeneratedAt: " + DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture),
                "UnityVersion: " + Application.unityVersion,
                "Project: unity/Echapee4D",
                "Scene: " + ScenePath,
                "Status: " + (report.Failures.Count == 0 ? "PASS" : "FAIL"),
                ""
            };

            AddSection(lines, "Imported files", report.ImportedFiles);
            AddSection(lines, "Texture dimensions", report.TextureDimensions);
            AddSection(lines, "Generated materials", report.Materials);
            AddSection(lines, "Animation clips", Deduplicate(report.AnimationClips));
            AddSection(lines, "Animation controller", string.IsNullOrWhiteSpace(report.AnimationController) ? Array.Empty<string>() : new[] { report.AnimationController });
            AddSection(lines, "Validation scene objects", report.SceneObjects);
            AddSection(lines, "Enabled build scenes", report.EnabledBuildScenes);
            AddSection(lines, "Verdict Artistique V1", report.ArtVerdicts);
            AddSection(lines, "Failures", report.Failures);
            AddSection(lines, "Notes", new[]
            {
                "Manifest approved status is legal/technical POC import approval, not final V1 art adoption.",
                "MYB-53 scene is dedicated to the animated asset POC and is intentionally not added to Build Settings.",
                "No ambiguous free/pro asset was imported; Quaternius Medieval Village MegaKit remained source evidence only because direct file retrieval goes through Itch.",
                string.IsNullOrWhiteSpace(report.CapturePath) ? "Capture: not produced." : "Capture: " + report.CapturePath
            });

            File.WriteAllLines(reportPath, lines);
            return ReportRelativePath;
        }

        private static IEnumerable<string> Deduplicate(IEnumerable<string> values)
        {
            return values.Distinct(StringComparer.Ordinal).OrderBy(value => value, StringComparer.Ordinal);
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

        private sealed class AssetSpec
        {
            public AssetSpec(string id, string label, string rootPath, string[] assetPaths)
            {
                Id = id;
                Label = label;
                RootPath = rootPath;
                AssetPaths = assetPaths;
            }

            public string Id { get; }
            public string Label { get; }
            public string RootPath { get; }
            public string[] AssetPaths { get; }
        }

        private sealed class ValidationReport
        {
            public List<string> ImportedFiles { get; } = new List<string>();
            public List<string> TextureDimensions { get; } = new List<string>();
            public List<string> Materials { get; } = new List<string>();
            public List<string> AnimationClips { get; } = new List<string>();
            public List<string> SceneObjects { get; } = new List<string>();
            public List<string> EnabledBuildScenes { get; } = new List<string>();
            public List<string> ArtVerdicts { get; } = new List<string>();
            public List<string> Failures { get; } = new List<string>();
            public string AnimationController { get; set; }
            public string CapturePath { get; set; }
        }
    }
}

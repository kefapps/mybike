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

namespace MYB95.Editor
{
    public static class MYB95MeshyCharacterDirectImporter
    {
        private const string Root = "Assets/Echappee/Art/MYB95MeshyCharacter";
        private const string RiggedModelPath = Root + "/Models/MYB95_RouteGuardian_Rigged.fbx";
        private const string WalkingModelPath = Root + "/Models/MYB95_RouteGuardian_Walking.fbx";
        private const string RunningModelPath = Root + "/Models/MYB95_RouteGuardian_Running.fbx";
        private const string BaseColorPath = Root + "/Textures/MYB95_RouteGuardian_BaseColor.png";
        private const string MetallicPath = Root + "/Textures/MYB95_RouteGuardian_Metallic.png";
        private const string RoughnessPath = Root + "/Textures/MYB95_RouteGuardian_Roughness.png";
        private const string NormalPath = Root + "/Textures/MYB95_RouteGuardian_Normal.png";
        private const string EmissionPath = Root + "/Textures/MYB95_RouteGuardian_Emission.png";
        private const string MaterialPath = Root + "/Materials/MYB95_RouteGuardian_PBR.mat";
        private const string AnimatorPath = Root + "/Animations/MYB95_RouteGuardian.controller";
        private const string PrefabPath = Root + "/Prefabs/MYB95_RouteGuardian_Direct.prefab";
        private const string ScenePath = "Assets/Scenes/MYB95MeshyCharacterDirectImportPoc.unity";
        private const string ReportRelativePath = "_bmad-output/unity-test-results/myb-95-meshy-character-direct-import.txt";
        private const string CaptureRelativePath = "_bmad-output/unity-test-results/myb-95-meshy-character-direct-import.png";
        private const string AnimationFramesRelativeDir = "_bmad-output/unity-test-results/myb-95-meshy-character-direct-import-frames";
        private const string AnimationStripRelativePath = "_bmad-output/unity-test-results/myb-95-meshy-character-direct-import-walking-strip.png";

        private const float TargetHeightMeters = 1.75f;
        private const int MaxTrianglesForCharacterPoc = 130000;
        private const int AnimationFrameCount = 12;
        private const int AnimationFrameWidth = 640;
        private const int AnimationFrameHeight = 360;

        [MenuItem("Tools/MYB-95/Build Meshy Character Direct Import")]
        public static void BuildAndValidateFromMenu()
        {
            var reportPath = BuildAndValidate();
            Debug.Log("MYB-95 Meshy character direct import complete. Report: " + reportPath);
        }

        public static void BuildAndValidateCli()
        {
            var reportPath = BuildAndValidate();
            var reportText = File.ReadAllText(Path.Combine(GetRepoRoot(), reportPath));
            if (reportText.Contains("Status: FAIL", StringComparison.Ordinal))
            {
                throw new InvalidOperationException("MYB-95 Meshy character direct import failed. See " + reportPath);
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
                ConfigureTextureImporters(report);

                var humanAvatar = ConfigureHumanoidImport(report);
                var useHumanoid = humanAvatar != null && humanAvatar.isValid && humanAvatar.isHuman;
                if (!useHumanoid)
                {
                    report.Notes.Add("Humanoid avatar was not valid; falling back to Generic import for direct clip playback.");
                    ConfigureGenericImport(report);
                }
                else
                {
                    ConfigureAnimationImporter(WalkingModelPath, "Walking", ModelImporterAnimationType.Human, humanAvatar, report);
                    ConfigureAnimationImporter(RunningModelPath, "Running", ModelImporterAnimationType.Human, humanAvatar, report);
                }

                AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

                var avatar = LoadAvatar(RiggedModelPath);
                var material = CreateMaterial(report);
                var controller = CreateAnimatorController(report);
                CreatePrefab(material, avatar, controller, report);
                BuildValidationScene(report);
                CaptureScenePreview(report);
                CaptureWalkingAnimationPreview(report);
                ValidateImportedAssets(report, avatar, controller);
                AddVerdict(report);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            }
            catch (Exception exception)
            {
                report.Failures.Add(exception.GetType().FullName + ": " + exception.Message);
                Debug.LogException(exception);
            }

            return WriteReport(report);
        }

        private static Avatar ConfigureHumanoidImport(ValidationReport report)
        {
            ConfigureRigImporter(ModelImporterAnimationType.Human, report);
            var avatar = LoadAvatar(RiggedModelPath);
            report.ModelMetrics.Add("Humanoid avatar loaded: " + (avatar != null));
            if (avatar != null)
            {
                report.ModelMetrics.Add("Humanoid avatar valid: " + avatar.isValid);
                report.ModelMetrics.Add("Humanoid avatar isHuman: " + avatar.isHuman);
            }

            return avatar;
        }

        private static void ConfigureGenericImport(ValidationReport report)
        {
            ConfigureRigImporter(ModelImporterAnimationType.Generic, report);
            var genericAvatar = LoadAvatar(RiggedModelPath);
            ConfigureAnimationImporter(WalkingModelPath, "Walking", ModelImporterAnimationType.Generic, genericAvatar, report);
            ConfigureAnimationImporter(RunningModelPath, "Running", ModelImporterAnimationType.Generic, genericAvatar, report);
        }

        private static void ConfigureRigImporter(ModelImporterAnimationType animationType, ValidationReport report)
        {
            var importer = AssetImporter.GetAtPath(RiggedModelPath) as ModelImporter;
            if (importer == null)
            {
                report.Failures.Add("Missing rigged model importer: " + RiggedModelPath);
                return;
            }

            importer.globalScale = 1f;
            importer.useFileUnits = true;
            importer.importAnimation = true;
            importer.animationType = animationType;
            importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
            importer.optimizeGameObjects = false;
            importer.materialImportMode = ModelImporterMaterialImportMode.None;
            importer.importCameras = false;
            importer.importLights = false;
            importer.importBlendShapes = true;
            importer.importVisibility = false;
            importer.importNormals = ModelImporterNormals.Import;
            importer.importTangents = ModelImporterTangents.CalculateMikk;
            importer.isReadable = false;
            importer.SaveAndReimport();

            report.ImportSettings.Add("Rigged model animation type: " + animationType);
        }

        private static void ConfigureAnimationImporter(string assetPath, string clipName, ModelImporterAnimationType animationType, Avatar sourceAvatar, ValidationReport report)
        {
            var importer = AssetImporter.GetAtPath(assetPath) as ModelImporter;
            if (importer == null)
            {
                report.Failures.Add("Missing animation model importer: " + assetPath);
                return;
            }

            importer.globalScale = 1f;
            importer.useFileUnits = true;
            importer.importAnimation = true;
            importer.animationType = animationType;
            importer.avatarSetup = sourceAvatar != null && sourceAvatar.isValid
                ? ModelImporterAvatarSetup.CopyFromOther
                : ModelImporterAvatarSetup.CreateFromThisModel;
            importer.sourceAvatar = sourceAvatar;
            importer.optimizeGameObjects = false;
            importer.materialImportMode = ModelImporterMaterialImportMode.None;
            importer.importCameras = false;
            importer.importLights = false;
            importer.importBlendShapes = true;
            importer.importVisibility = false;
            importer.importNormals = ModelImporterNormals.Import;
            importer.importTangents = ModelImporterTangents.CalculateMikk;

            var clips = importer.defaultClipAnimations;
            if (clips.Length > 0)
            {
                for (var index = 0; index < clips.Length; index++)
                {
                    clips[index].name = clips.Length == 1 ? clipName : clipName + "_" + index;
                    clips[index].loopTime = true;
                    clips[index].loopPose = true;
                }

                importer.clipAnimations = clips;
            }

            importer.SaveAndReimport();
            report.ImportSettings.Add(clipName + " importer animation type: " + animationType);
            report.ImportSettings.Add(clipName + " source avatar assigned: " + (sourceAvatar != null));
        }

        private static void ConfigureTextureImporters(ValidationReport report)
        {
            ConfigureTexture(BaseColorPath, TextureImporterType.Default, true, report);
            ConfigureTexture(MetallicPath, TextureImporterType.Default, false, report);
            ConfigureTexture(RoughnessPath, TextureImporterType.Default, false, report);
            ConfigureTexture(NormalPath, TextureImporterType.NormalMap, false, report);
            ConfigureTexture(EmissionPath, TextureImporterType.Default, true, report);
        }

        private static void ConfigureTexture(string assetPath, TextureImporterType type, bool srgb, ValidationReport report)
        {
            var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null)
            {
                report.Failures.Add("Missing texture importer: " + assetPath);
                return;
            }

            importer.textureType = type;
            importer.sRGBTexture = srgb;
            importer.maxTextureSize = 2048;
            importer.mipmapEnabled = true;
            importer.textureCompression = TextureImporterCompression.CompressedHQ;
            importer.SaveAndReimport();
        }

        private static Material CreateMaterial(ValidationReport report)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            if (shader == null)
            {
                report.Failures.Add("No compatible Lit shader found.");
                return null;
            }

            var material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
            if (material == null)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, MaterialPath);
            }
            else
            {
                material.shader = shader;
            }

            SetTexture(material, "_BaseMap", LoadTexture(BaseColorPath, report));
            SetTexture(material, "_MainTex", LoadTexture(BaseColorPath, report));
            SetTexture(material, "_MetallicGlossMap", LoadTexture(MetallicPath, report));
            SetTexture(material, "_BumpMap", LoadTexture(NormalPath, report));
            SetTexture(material, "_EmissionMap", LoadTexture(EmissionPath, report));
            SetColor(material, "_BaseColor", Color.white);
            SetColor(material, "_Color", Color.white);
            SetColor(material, "_EmissionColor", new Color(0.24f, 0.82f, 1f, 1f) * 1.15f);
            SetFloat(material, "_Metallic", 0.08f);
            SetFloat(material, "_Smoothness", 0.46f);
            SetFloat(material, "_BumpScale", 0.42f);
            material.EnableKeyword("_NORMALMAP");
            material.EnableKeyword("_METALLICSPECGLOSSMAP");
            material.EnableKeyword("_EMISSION");
            material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            EditorUtility.SetDirty(material);

            report.GeneratedAssets.Add(MaterialPath);
            report.Notes.Add("Roughness texture is imported but not packed into Unity metallic/smoothness yet.");
            return material;
        }

        private static AnimatorController CreateAnimatorController(ValidationReport report)
        {
            AssetDatabase.DeleteAsset(AnimatorPath);
            var controller = AnimatorController.CreateAnimatorControllerAtPath(AnimatorPath);
            var layer = controller.layers[0];
            var stateMachine = layer.stateMachine;

            foreach (var state in stateMachine.states)
            {
                stateMachine.RemoveState(state.state);
            }

            var walking = FirstClip(WalkingModelPath);
            var running = FirstClip(RunningModelPath);

            if (walking != null)
            {
                var walkState = stateMachine.AddState("Walking");
                walkState.motion = walking;
                stateMachine.defaultState = walkState;
                report.AnimationClips.Add("Walking: " + walking.name + " length=" + walking.length.ToString("0.###", CultureInfo.InvariantCulture));
            }
            else
            {
                report.Failures.Add("Walking animation clip not found in " + WalkingModelPath);
            }

            if (running != null)
            {
                var runState = stateMachine.AddState("Running");
                runState.motion = running;
                report.AnimationClips.Add("Running: " + running.name + " length=" + running.length.ToString("0.###", CultureInfo.InvariantCulture));
            }
            else
            {
                report.Failures.Add("Running animation clip not found in " + RunningModelPath);
            }

            EditorUtility.SetDirty(controller);
            report.GeneratedAssets.Add(AnimatorPath);
            return controller;
        }

        private static void CreatePrefab(Material material, Avatar avatar, RuntimeAnimatorController controller, ValidationReport report)
        {
            AssetDatabase.DeleteAsset(PrefabPath);

            var model = AssetDatabase.LoadAssetAtPath<GameObject>(RiggedModelPath);
            if (model == null)
            {
                report.Failures.Add("Could not load rigged model: " + RiggedModelPath);
                return;
            }

            var instance = PrefabUtility.InstantiatePrefab(model) as GameObject;
            if (instance == null)
            {
                report.Failures.Add("Could not instantiate rigged model: " + RiggedModelPath);
                return;
            }

            instance.name = "MYB95_RouteGuardian_Direct";
            AssignMaterial(instance, material);
            FitToHeight(instance, TargetHeightMeters);

            var animator = instance.GetComponent<Animator>() ?? instance.AddComponent<Animator>();
            animator.avatar = avatar;
            animator.runtimeAnimatorController = controller;
            animator.applyRootMotion = false;
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;

            var prefab = PrefabUtility.SaveAsPrefabAsset(instance, PrefabPath);
            UnityEngine.Object.DestroyImmediate(instance);

            if (prefab == null)
            {
                report.Failures.Add("Failed to save prefab: " + PrefabPath);
                return;
            }

            report.GeneratedAssets.Add(PrefabPath);
        }

        private static void BuildValidationScene(ValidationReport report)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            ConfigureRenderSettings();
            CreateGround();
            CreateSun();
            CreateCamera();

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            if (prefab == null)
            {
                report.Failures.Add("Prefab missing before scene build: " + PrefabPath);
                return;
            }

            var instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            if (instance == null)
            {
                report.Failures.Add("Could not instantiate prefab in scene.");
                return;
            }

            instance.name = "MYB95_RouteGuardian_Direct_SceneProof";
            instance.transform.SetPositionAndRotation(Vector3.zero, Quaternion.Euler(0f, 198f, 0f));
            SampleWalkingPose(instance, report);
            report.SceneObjects.Add(instance.name);

            EditorSceneManager.SaveScene(scene, ScenePath);
            report.GeneratedAssets.Add(ScenePath);
        }

        private static void SampleWalkingPose(GameObject instance, ValidationReport report)
        {
            var walking = FirstClip(WalkingModelPath);
            if (walking == null)
            {
                return;
            }

            var sampleTime = Mathf.Clamp(walking.length * 0.38f, 0f, walking.length);
            walking.SampleAnimation(instance, sampleTime);
            report.AnimationClips.Add("Scene preview samples Walking at t=" + sampleTime.ToString("0.###", CultureInfo.InvariantCulture));
        }

        private static void ValidateImportedAssets(ValidationReport report, Avatar avatar, RuntimeAnimatorController controller)
        {
            report.ModelMetrics.Add("Rigged avatar exists: " + (avatar != null));
            if (avatar != null)
            {
                report.ModelMetrics.Add("Final avatar valid: " + avatar.isValid);
                report.ModelMetrics.Add("Final avatar isHuman: " + avatar.isHuman);
            }

            var model = AssetDatabase.LoadAssetAtPath<GameObject>(RiggedModelPath);
            if (model == null)
            {
                report.Failures.Add("Rigged model failed to load after import.");
                return;
            }

            var instance = PrefabUtility.InstantiatePrefab(model) as GameObject;
            if (instance == null)
            {
                report.Failures.Add("Rigged model failed to instantiate after import.");
                return;
            }

            try
            {
                var skinned = instance.GetComponentsInChildren<SkinnedMeshRenderer>(true);
                var meshFilters = instance.GetComponentsInChildren<MeshFilter>(true);
                var bones = instance.GetComponentsInChildren<Transform>(true)
                    .Count(t => t.name.IndexOf("bone", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        t.name.IndexOf("mixamorig", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        t.name.IndexOf("root", StringComparison.OrdinalIgnoreCase) >= 0);

                var triangles = CountTrianglesInHierarchy(instance);
                var bounds = GetWorldBounds(instance);

                report.ModelMetrics.Add("SkinnedMeshRenderer count: " + skinned.Length);
                report.ModelMetrics.Add("MeshFilter count: " + meshFilters.Length);
                report.ModelMetrics.Add("Estimated bone/root transform count: " + bones);
                report.ModelMetrics.Add("Prefab source triangles: " + triangles);
                report.ModelMetrics.Add("Prefab source bounds: " + FormatVector(bounds.size));

                if (skinned.Length == 0)
                {
                    report.Failures.Add("Direct import produced no SkinnedMeshRenderer.");
                }

                if (triangles <= 0)
                {
                    report.Failures.Add("Direct import produced no mesh triangles.");
                }
                else if (triangles > MaxTrianglesForCharacterPoc)
                {
                    report.Failures.Add("Direct import exceeds MYB-95 character POC triangle budget: " + triangles);
                }
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(instance);
            }

            if (controller == null)
            {
                report.Failures.Add("Animator controller missing.");
            }
            else if (controller.animationClips.Length == 0)
            {
                report.Failures.Add("Animator controller contains no clips.");
            }

            foreach (var texturePath in TexturePaths())
            {
                var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
                if (texture == null)
                {
                    report.Failures.Add("Texture failed to load: " + texturePath);
                    continue;
                }

                report.TextureMetrics.Add(texturePath + " " + texture.width + "x" + texture.height);
            }
        }

        private static void CaptureScenePreview(ValidationReport report)
        {
            var camera = Camera.main;
            if (camera == null)
            {
                report.Failures.Add("Cannot capture character scene: missing Main Camera.");
                return;
            }

            var absolutePath = Path.Combine(GetRepoRoot(), CaptureRelativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(absolutePath) ?? GetRepoRoot());

            var texture = RenderCameraToTexture(camera, 1280, 720);
            try
            {
                File.WriteAllBytes(absolutePath, texture.EncodeToPNG());
                report.CapturePath = CaptureRelativePath;
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(texture);
            }
        }

        private static void CaptureWalkingAnimationPreview(ValidationReport report)
        {
            var camera = Camera.main;
            if (camera == null)
            {
                report.Failures.Add("Cannot capture walking animation preview: missing Main Camera.");
                return;
            }

            var instance = GameObject.Find("MYB95_RouteGuardian_Direct_SceneProof");
            if (instance == null)
            {
                report.Failures.Add("Cannot capture walking animation preview: scene proof object missing.");
                return;
            }

            var walking = FirstClip(WalkingModelPath);
            if (walking == null)
            {
                report.Failures.Add("Cannot capture walking animation preview: Walking clip missing.");
                return;
            }

            var repoRoot = GetRepoRoot();
            var framesDirectory = Path.Combine(repoRoot, AnimationFramesRelativeDir);
            var stripPath = Path.Combine(repoRoot, AnimationStripRelativePath);
            if (Directory.Exists(framesDirectory))
            {
                Directory.Delete(framesDirectory, true);
            }
            Directory.CreateDirectory(framesDirectory);
            Directory.CreateDirectory(Path.GetDirectoryName(stripPath) ?? repoRoot);

            var frames = new List<Texture2D>(AnimationFrameCount);
            try
            {
                for (var index = 0; index < AnimationFrameCount; index++)
                {
                    var sampleTime = walking.length * index / AnimationFrameCount;
                    walking.SampleAnimation(instance, sampleTime);
                    var texture = RenderCameraToTexture(camera, AnimationFrameWidth, AnimationFrameHeight);
                    var frameName = "frame_" + index.ToString("000", CultureInfo.InvariantCulture) + ".png";
                    File.WriteAllBytes(Path.Combine(framesDirectory, frameName), texture.EncodeToPNG());
                    frames.Add(texture);
                }

                WriteAnimationStrip(frames, stripPath);
                report.AnimationFramesDirectory = AnimationFramesRelativeDir;
                report.AnimationStripPath = AnimationStripRelativePath;
                report.AnimationClips.Add("Walking animation preview: " + AnimationFrameCount + " sampled frames over " + walking.length.ToString("0.###", CultureInfo.InvariantCulture) + "s");
            }
            finally
            {
                foreach (var frame in frames)
                {
                    UnityEngine.Object.DestroyImmediate(frame);
                }
            }
        }

        private static void AddVerdict(ValidationReport report)
        {
            if (report.Failures.Count == 0)
            {
                report.Verdicts.Add("Direct FBX import is usable for the animated character POC: Unity sees a skinned mesh, an avatar and animation clips.");
            }
            else
            {
                report.Verdicts.Add("Direct FBX import is partially blocked; see Failures for the first broken link in the asset chain.");
            }

            report.Verdicts.Add("This path bypasses the Meshy Unity plugin and uses local downloaded outputs, so it is reproducible inside the repo.");
            report.Verdicts.Add("Material workflow is still POC-grade: roughness is imported but not packed into Unity smoothness.");
        }

        private static string WriteReport(ValidationReport report)
        {
            var reportPath = Path.Combine(GetRepoRoot(), ReportRelativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(reportPath) ?? GetRepoRoot());

            var lines = new List<string>
            {
                "# MYB-95 Meshy Character Direct Unity Import",
                "GeneratedAt: " + DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture),
                "UnityVersion: " + Application.unityVersion,
                "Project: unity/Echapee4D",
                "Scene: " + ScenePath,
                "Prefab: " + PrefabPath,
                "Status: " + (report.Failures.Count == 0 ? "PASS" : "FAIL"),
                ""
            };

            AddSection(lines, "Source files", report.SourceFiles);
            AddSection(lines, "Import settings", report.ImportSettings);
            AddSection(lines, "Generated assets", report.GeneratedAssets.Distinct(StringComparer.Ordinal));
            AddSection(lines, "Animation clips", report.AnimationClips);
            AddSection(lines, "Model metrics", report.ModelMetrics);
            AddSection(lines, "Texture metrics", report.TextureMetrics);
            AddSection(lines, "Scene objects", report.SceneObjects);
            AddSection(lines, "Verdict", report.Verdicts);
            AddSection(lines, "Failures", report.Failures);
            AddSection(lines, "Notes", report.Notes.Concat(new[]
            {
                string.IsNullOrWhiteSpace(report.CapturePath) ? "Capture: not produced." : "Capture: " + report.CapturePath,
                string.IsNullOrWhiteSpace(report.AnimationFramesDirectory) ? "Animation frames: not produced." : "Animation frames: " + report.AnimationFramesDirectory,
                string.IsNullOrWhiteSpace(report.AnimationStripPath) ? "Animation strip: not produced." : "Animation strip: " + report.AnimationStripPath
            }));

            File.WriteAllLines(reportPath, lines);
            return ReportRelativePath;
        }

        private static void EnsureFolders()
        {
            CreateFolderRecursive(Root + "/Models");
            CreateFolderRecursive(Root + "/Textures");
            CreateFolderRecursive(Root + "/Materials");
            CreateFolderRecursive(Root + "/Animations");
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
                report.SourceFiles.Add(assetPath + " (" + FormatBytes(info.Length) + ")");
            }
        }

        private static AnimationClip FirstClip(string assetPath)
        {
            return AssetDatabase.LoadAllAssetsAtPath(assetPath)
                .OfType<AnimationClip>()
                .Where(clip => !clip.name.StartsWith("__preview__", StringComparison.Ordinal) && clip.length > 0.001f)
                .OrderByDescending(clip => clip.length)
                .FirstOrDefault();
        }

        private static Avatar LoadAvatar(string assetPath)
        {
            return AssetDatabase.LoadAllAssetsAtPath(assetPath).OfType<Avatar>().FirstOrDefault();
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

        private static void AssignMaterial(GameObject gameObject, Material material)
        {
            if (material == null)
            {
                return;
            }

            foreach (var renderer in gameObject.GetComponentsInChildren<Renderer>(true))
            {
                var materials = renderer.sharedMaterials;
                for (var index = 0; index < materials.Length; index++)
                {
                    materials[index] = material;
                }
                renderer.sharedMaterials = materials;
            }
        }

        private static Texture2D RenderCameraToTexture(Camera camera, int width, int height)
        {
            var renderTexture = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
            var texture = new Texture2D(width, height, TextureFormat.RGB24, false);
            var previousActive = RenderTexture.active;
            var previousTarget = camera.targetTexture;

            try
            {
                camera.targetTexture = renderTexture;
                RenderTexture.active = renderTexture;
                camera.Render();
                texture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
                texture.Apply();
                return texture;
            }
            finally
            {
                camera.targetTexture = previousTarget;
                RenderTexture.active = previousActive;
                renderTexture.Release();
                UnityEngine.Object.DestroyImmediate(renderTexture);
            }
        }

        private static void WriteAnimationStrip(IReadOnlyList<Texture2D> frames, string absolutePath)
        {
            const int columns = 4;
            var rows = Mathf.CeilToInt(frames.Count / (float)columns);
            var strip = new Texture2D(AnimationFrameWidth * columns, AnimationFrameHeight * rows, TextureFormat.RGB24, false);

            try
            {
                var background = Enumerable
                    .Repeat(new Color(0.10f, 0.12f, 0.12f), strip.width * strip.height)
                    .ToArray();
                strip.SetPixels(background);

                for (var index = 0; index < frames.Count; index++)
                {
                    var column = index % columns;
                    var row = rows - 1 - (index / columns);
                    strip.SetPixels(
                        column * AnimationFrameWidth,
                        row * AnimationFrameHeight,
                        AnimationFrameWidth,
                        AnimationFrameHeight,
                        frames[index].GetPixels());
                }

                strip.Apply();
                File.WriteAllBytes(absolutePath, strip.EncodeToPNG());
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(strip);
            }
        }

        private static void FitToHeight(GameObject gameObject, float targetHeight)
        {
            var bounds = GetWorldBounds(gameObject);
            if (bounds.size.y <= 0.001f)
            {
                return;
            }

            gameObject.transform.localScale *= targetHeight / bounds.size.y;
            bounds = GetWorldBounds(gameObject);
            gameObject.transform.position -= new Vector3(bounds.center.x, bounds.min.y, bounds.center.z);
        }

        private static Bounds GetWorldBounds(GameObject gameObject)
        {
            var renderers = gameObject.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0)
            {
                return new Bounds(gameObject.transform.position, Vector3.zero);
            }

            var bounds = renderers[0].bounds;
            for (var index = 1; index < renderers.Length; index++)
            {
                bounds.Encapsulate(renderers[index].bounds);
            }

            return bounds;
        }

        private static int CountTrianglesInHierarchy(GameObject gameObject)
        {
            var meshFilterTriangles = gameObject.GetComponentsInChildren<MeshFilter>(true)
                .Where(filter => filter.sharedMesh != null)
                .Sum(filter => CountTriangles(filter.sharedMesh));
            var skinnedTriangles = gameObject.GetComponentsInChildren<SkinnedMeshRenderer>(true)
                .Where(renderer => renderer.sharedMesh != null)
                .Sum(renderer => CountTriangles(renderer.sharedMesh));
            return meshFilterTriangles + skinnedTriangles;
        }

        private static int CountTriangles(Mesh mesh)
        {
            var count = 0;
            for (var submesh = 0; submesh < mesh.subMeshCount; submesh++)
            {
                count += (int)(mesh.GetIndexCount(submesh) / 3);
            }

            return count;
        }

        private static void ConfigureRenderSettings()
        {
            RenderSettings.skybox = null;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.48f, 0.58f, 0.63f);
            RenderSettings.ambientEquatorColor = new Color(0.30f, 0.32f, 0.28f);
            RenderSettings.ambientGroundColor = new Color(0.14f, 0.16f, 0.13f);
            RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(0.30f, 0.37f, 0.38f);
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogStartDistance = 9f;
            RenderSettings.fogEndDistance = 28f;
        }

        private static void CreateCamera()
        {
            var cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(0.05f, 1.25f, -3.15f);
            cameraObject.transform.rotation = Quaternion.Euler(8.5f, 0f, 0f);
            var camera = cameraObject.AddComponent<Camera>();
            camera.fieldOfView = 42f;
            camera.nearClipPlane = 0.03f;
            camera.farClipPlane = 70f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.28f, 0.35f, 0.36f);
        }

        private static void CreateSun()
        {
            var sunObject = new GameObject("MYB95_WarmCharacterSun");
            sunObject.transform.rotation = Quaternion.Euler(38f, -30f, 0f);
            var sun = sunObject.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.color = new Color(1f, 0.84f, 0.66f);
            sun.intensity = 0.70f;
            sun.shadows = LightShadows.Soft;
        }

        private static void CreateGround()
        {
            var grass = CreateMaterial("MYB95_CharacterProofGrass", new Color(0.22f, 0.35f, 0.25f), 0.12f);
            var road = CreateMaterial("MYB95_CharacterProofPaving", new Color(0.50f, 0.43f, 0.36f), 0.18f);
            CreateCube("MYB95_CharacterProofGround", new Vector3(0f, -0.055f, 1.5f), new Vector3(6.4f, 0.10f, 7.2f), grass);
            CreateCube("MYB95_CharacterProofRoad", new Vector3(0f, -0.01f, 1.5f), new Vector3(1.42f, 0.08f, 7.0f), road);
        }

        private static Material CreateMaterial(string name, Color color, float smoothness)
        {
            var materialPath = Root + "/Materials/" + name + ".mat";
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
            var collider = cube.GetComponent<Collider>();
            if (collider != null)
            {
                UnityEngine.Object.DestroyImmediate(collider);
            }
            return cube;
        }

        private static IEnumerable<string> SourceAssetPaths()
        {
            yield return RiggedModelPath;
            yield return WalkingModelPath;
            yield return RunningModelPath;
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

        private static string FormatVector(Vector3 value)
        {
            return "(" +
                value.x.ToString("0.###", CultureInfo.InvariantCulture) + ", " +
                value.y.ToString("0.###", CultureInfo.InvariantCulture) + ", " +
                value.z.ToString("0.###", CultureInfo.InvariantCulture) + ")";
        }

        private sealed class ValidationReport
        {
            public List<string> SourceFiles { get; } = new List<string>();
            public List<string> ImportSettings { get; } = new List<string>();
            public List<string> GeneratedAssets { get; } = new List<string>();
            public List<string> AnimationClips { get; } = new List<string>();
            public List<string> ModelMetrics { get; } = new List<string>();
            public List<string> TextureMetrics { get; } = new List<string>();
            public List<string> SceneObjects { get; } = new List<string>();
            public List<string> Verdicts { get; } = new List<string>();
            public List<string> Failures { get; } = new List<string>();
            public List<string> Notes { get; } = new List<string>();
            public string CapturePath { get; set; }
            public string AnimationFramesDirectory { get; set; }
            public string AnimationStripPath { get; set; }
        }
    }
}

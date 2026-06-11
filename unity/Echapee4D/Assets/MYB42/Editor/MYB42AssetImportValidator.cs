using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MYB42.Editor
{
    public static class MYB42AssetImportValidator
    {
        private const string ScenePath = "Assets/Scenes/MYB42AssetImportValidation.unity";
        private const string ThirdPartyRoot = "Assets/Echappee/Art/ThirdParty";
        private const string MaterialsRoot = "Assets/Echappee/Art/MYB42Validation/Materials";
        private const string CanonicalScenePath = "Assets/Scenes/MYB89UnityMcpProbe.unity";
        private const string ReportRelativePath = "_bmad-output/unity-test-results/myb-42-asset-import-validation.txt";
        private const string CaptureRelativePath = "_bmad-output/unity-test-results/myb-42-asset-import-validation.png";

        private static readonly AssetSpec[] AssetSpecs =
        {
            new AssetSpec(
                "kenney-nature-kit-myb42-subset",
                "Kenney Nature Kit subset",
                "Assets/Echappee/Art/ThirdParty/Kenney/NatureKit",
                new[]
                {
                    "Assets/Echappee/Art/ThirdParty/Kenney/NatureKit/Models/tree_tall.fbx",
                    "Assets/Echappee/Art/ThirdParty/Kenney/NatureKit/Models/rock_smallA.fbx",
                    "Assets/Echappee/Art/ThirdParty/Kenney/NatureKit/License/License.txt",
                }),
            new AssetSpec(
                "kenney-fantasy-town-kit-myb42-subset",
                "Kenney Fantasy Town Kit subset",
                "Assets/Echappee/Art/ThirdParty/Kenney/FantasyTownKit",
                new[]
                {
                    "Assets/Echappee/Art/ThirdParty/Kenney/FantasyTownKit/Models/road.fbx",
                    "Assets/Echappee/Art/ThirdParty/Kenney/FantasyTownKit/Models/wall-window-stone.fbx",
                    "Assets/Echappee/Art/ThirdParty/Kenney/FantasyTownKit/License/License.txt",
                }),
            new AssetSpec(
                "ambientcg-paving-stones-141-1k-jpg-myb42",
                "ambientCG Paving Stones 141 subset",
                "Assets/Echappee/Art/ThirdParty/ambientCG/PavingStones141",
                new[]
                {
                    "Assets/Echappee/Art/ThirdParty/ambientCG/PavingStones141/Textures/PavingStones141_1K-JPG_Color.jpg",
                    "Assets/Echappee/Art/ThirdParty/ambientCG/PavingStones141/Textures/PavingStones141_1K-JPG_NormalGL.jpg",
                    "Assets/Echappee/Art/ThirdParty/ambientCG/PavingStones141/Textures/PavingStones141_1K-JPG_Roughness.jpg",
                }),
            new AssetSpec(
                "polyhaven-forest-ground-03-1k-jpg-myb42",
                "Poly Haven Forest Ground 03 subset",
                "Assets/Echappee/Art/ThirdParty/PolyHaven/ForestGround03",
                new[]
                {
                    "Assets/Echappee/Art/ThirdParty/PolyHaven/ForestGround03/Textures/forrest_ground_03_diff_1k.jpg",
                    "Assets/Echappee/Art/ThirdParty/PolyHaven/ForestGround03/Textures/forrest_ground_03_nor_gl_1k.jpg",
                    "Assets/Echappee/Art/ThirdParty/PolyHaven/ForestGround03/Textures/forrest_ground_03_rough_1k.jpg",
                })
        };

        [MenuItem("Tools/MYB-42/Build Asset Import Validation Scene")]
        public static void BuildAndValidateFromMenu()
        {
            var reportPath = BuildAndValidate();
            Debug.Log("MYB-42 asset import validation complete. Report: " + reportPath);
        }

        public static void BuildAndValidateCli()
        {
            var reportPath = BuildAndValidate();
            var reportText = File.ReadAllText(Path.Combine(GetRepoRoot(), reportPath));
            if (reportText.Contains("Status: FAIL", StringComparison.Ordinal))
            {
                throw new InvalidOperationException("MYB-42 asset import validation failed. See " + reportPath);
            }
        }

        public static string BuildAndValidate()
        {
            var report = new ValidationReport();

            try
            {
                AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
                EnsureFolders();
                ConfigureTextureImporters(report);

                var pavingMaterial = CreateLitMaterial(
                    "MYB42_PavingStones141",
                    "Assets/Echappee/Art/ThirdParty/ambientCG/PavingStones141/Textures/PavingStones141_1K-JPG_Color.jpg",
                    "Assets/Echappee/Art/ThirdParty/ambientCG/PavingStones141/Textures/PavingStones141_1K-JPG_NormalGL.jpg",
                    0.22f,
                    report);

                var forestMaterial = CreateLitMaterial(
                    "MYB42_ForestGround03",
                    "Assets/Echappee/Art/ThirdParty/PolyHaven/ForestGround03/Textures/forrest_ground_03_diff_1k.jpg",
                    "Assets/Echappee/Art/ThirdParty/PolyHaven/ForestGround03/Textures/forrest_ground_03_nor_gl_1k.jpg",
                    0.18f,
                    report);

                var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                BuildValidationScene(pavingMaterial, forestMaterial, report);
                EditorSceneManager.SaveScene(scene, ScenePath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

                CaptureScenePreview(report);
                ValidateManifest(report);
                ValidateAssetFiles(report);
                ValidateTextureBudgets(report);
                ValidateCanonicalBaselineBoundary(report);
                ValidateScene(report);
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
        }

        private static void ConfigureTextureImporters(ValidationReport report)
        {
            ConfigureTexture("Assets/Echappee/Art/ThirdParty/ambientCG/PavingStones141/Textures/PavingStones141_1K-JPG_Color.jpg", TextureImporterType.Default, true, report);
            ConfigureTexture("Assets/Echappee/Art/ThirdParty/ambientCG/PavingStones141/Textures/PavingStones141_1K-JPG_NormalGL.jpg", TextureImporterType.NormalMap, false, report);
            ConfigureTexture("Assets/Echappee/Art/ThirdParty/ambientCG/PavingStones141/Textures/PavingStones141_1K-JPG_Roughness.jpg", TextureImporterType.Default, false, report);
            ConfigureTexture("Assets/Echappee/Art/ThirdParty/PolyHaven/ForestGround03/Textures/forrest_ground_03_diff_1k.jpg", TextureImporterType.Default, true, report);
            ConfigureTexture("Assets/Echappee/Art/ThirdParty/PolyHaven/ForestGround03/Textures/forrest_ground_03_nor_gl_1k.jpg", TextureImporterType.NormalMap, false, report);
            ConfigureTexture("Assets/Echappee/Art/ThirdParty/PolyHaven/ForestGround03/Textures/forrest_ground_03_rough_1k.jpg", TextureImporterType.Default, false, report);
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

        private static Material CreateLitMaterial(string materialName, string colorPath, string normalPath, float smoothness, ValidationReport report)
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

            var color = AssetDatabase.LoadAssetAtPath<Texture2D>(colorPath);
            var normal = AssetDatabase.LoadAssetAtPath<Texture2D>(normalPath);
            if (color == null)
            {
                report.Failures.Add("Color texture missing: " + colorPath);
            }
            if (normal == null)
            {
                report.Failures.Add("Normal texture missing: " + normalPath);
            }

            SetTexture(material, "_BaseMap", color);
            SetTexture(material, "_MainTex", color);
            SetTexture(material, "_BumpMap", normal);
            SetFloat(material, "_BumpScale", 0.55f);
            SetFloat(material, "_Metallic", 0f);
            SetFloat(material, "_Smoothness", smoothness);
            EditorUtility.SetDirty(material);
            report.Materials.Add(materialPath);
            return material;
        }

        private static void BuildValidationScene(Material pavingMaterial, Material forestMaterial, ValidationReport report)
        {
            ConfigureRenderSettings();
            CreateCamera();
            CreateLight();
            CreateLabel("MYB-42 Asset Import Validation", new Vector3(0f, 3.2f, 3.2f), 0.28f);

            CreatePlane("Forest Ground 03 - 1K", new Vector3(-2.3f, 0f, 0f), new Vector3(0.22f, 1f, 0.22f), forestMaterial);
            CreatePlane("Paving Stones 141 - 1K", new Vector3(2.3f, 0f, 0f), new Vector3(0.22f, 1f, 0.22f), pavingMaterial);

            InstantiateModel("Tree Tall", "Assets/Echappee/Art/ThirdParty/Kenney/NatureKit/Models/tree_tall.fbx", new Vector3(-3.2f, 0.02f, -0.35f), new Vector3(0f, 35f, 0f), Vector3.one * 1.2f, null, report);
            InstantiateModel("Rock Small A", "Assets/Echappee/Art/ThirdParty/Kenney/NatureKit/Models/rock_smallA.fbx", new Vector3(-1.25f, 0.02f, 0.35f), new Vector3(0f, -20f, 0f), Vector3.one * 1.8f, null, report);
            InstantiateModel("Fantasy Town Road", "Assets/Echappee/Art/ThirdParty/Kenney/FantasyTownKit/Models/road.fbx", new Vector3(1.55f, 0.04f, -0.15f), Vector3.zero, Vector3.one * 1.2f, pavingMaterial, report);
            InstantiateModel("Wall Window Stone", "Assets/Echappee/Art/ThirdParty/Kenney/FantasyTownKit/Models/wall-window-stone.fbx", new Vector3(3.15f, 0.04f, 0.25f), new Vector3(0f, -30f, 0f), Vector3.one * 1.25f, null, report);
        }

        private static void ConfigureRenderSettings()
        {
            RenderSettings.skybox = null;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.72f, 0.76f, 0.82f);
            RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(0.78f, 0.84f, 0.86f);
            RenderSettings.fogDensity = 0.012f;
        }

        private static void CreateCamera()
        {
            var cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(0f, 3.1f, -6.2f);
            cameraObject.transform.rotation = Quaternion.Euler(23f, 0f, 0f);
            var camera = cameraObject.AddComponent<Camera>();
            camera.fieldOfView = 42f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.70f, 0.82f, 0.88f);
        }

        private static void CreateLight()
        {
            var lightObject = new GameObject("MYB42 Key Light");
            lightObject.transform.rotation = Quaternion.Euler(45f, -35f, 0f);
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.15f;
            light.color = new Color(1f, 0.95f, 0.86f);
        }

        private static void CreateLabel(string text, Vector3 position, float size)
        {
            var label = new GameObject(text);
            label.transform.position = position;
            label.transform.rotation = Quaternion.Euler(68f, 0f, 0f);
            var mesh = label.AddComponent<TextMesh>();
            mesh.text = text;
            mesh.anchor = TextAnchor.MiddleCenter;
            mesh.alignment = TextAlignment.Center;
            mesh.characterSize = size;
            mesh.fontSize = 34;
            mesh.color = new Color(0.08f, 0.12f, 0.16f);
        }

        private static void CreatePlane(string name, Vector3 position, Vector3 scale, Material material)
        {
            var plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
            plane.name = name;
            plane.transform.position = position;
            plane.transform.localScale = scale;
            if (material != null)
            {
                var renderer = plane.GetComponent<Renderer>();
                renderer.sharedMaterial = material;
            }
        }

        private static void InstantiateModel(string name, string assetPath, Vector3 position, Vector3 eulerAngles, Vector3 scale, Material materialOverride, ValidationReport report)
        {
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (model == null)
            {
                report.Failures.Add("Model asset missing or failed to import: " + assetPath);
                return;
            }

            var instance = PrefabUtility.InstantiatePrefab(model) as GameObject;
            if (instance == null)
            {
                report.Failures.Add("Could not instantiate model asset: " + assetPath);
                return;
            }

            instance.name = name;
            instance.transform.position = position;
            instance.transform.rotation = Quaternion.Euler(eulerAngles);
            instance.transform.localScale = scale;
            if (materialOverride != null)
            {
                foreach (var renderer in instance.GetComponentsInChildren<Renderer>())
                {
                    renderer.sharedMaterial = materialOverride;
                }
            }
            report.SceneObjects.Add(name);
        }

        private static void CaptureScenePreview(ValidationReport report)
        {
            var camera = Camera.main;
            if (camera == null)
            {
                report.Failures.Add("Cannot capture MYB-42 scene: missing Main Camera.");
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
            foreach (var texturePath in AssetSpecs.SelectMany(spec => spec.AssetPaths).Where(path => path.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase)))
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
                    report.Failures.Add("Texture exceeds MYB-42 1K guardrail: " + texturePath);
                }
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
                report.Failures.Add("MYB-42 validation scene should not be added to Build Settings.");
            }

            report.EnabledBuildScenes.AddRange(enabledScenes);
        }

        private static void ValidateScene(ValidationReport report)
        {
            if (!File.Exists(ProjectRelativeToAbsolute(ScenePath)))
            {
                report.Failures.Add("Validation scene was not saved: " + ScenePath);
                return;
            }

            if (report.SceneObjects.Count < 4)
            {
                report.Failures.Add("Validation scene does not contain all expected imported model instances.");
            }
        }

        private static string WriteReport(ValidationReport report)
        {
            var reportPath = Path.Combine(GetRepoRoot(), ReportRelativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(reportPath));

            var lines = new List<string>
            {
                "# MYB-42 Asset Import Validation",
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
            AddSection(lines, "Validation scene objects", report.SceneObjects);
            AddSection(lines, "Enabled build scenes", report.EnabledBuildScenes);
            AddSection(lines, "Failures", report.Failures);
            AddSection(lines, "Notes", new[]
            {
                "Manifest approved status is legal/technical POC import approval, not final V1 art adoption.",
                "Validation scene is dedicated to MYB-42 and is intentionally not added to Build Settings.",
                string.IsNullOrWhiteSpace(report.CapturePath) ? "Capture: not produced." : "Capture: " + report.CapturePath
            });

            File.WriteAllLines(reportPath, lines);
            return ReportRelativePath;
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
            public List<string> SceneObjects { get; } = new List<string>();
            public List<string> EnabledBuildScenes { get; } = new List<string>();
            public List<string> Failures { get; } = new List<string>();
            public string CapturePath { get; set; }
        }
    }
}

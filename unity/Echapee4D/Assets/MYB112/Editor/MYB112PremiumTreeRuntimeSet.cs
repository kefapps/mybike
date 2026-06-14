using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace MYB112.Editor
{
    public static class MYB112PremiumTreeRuntimeSet
    {
        public const int VariantCount = 5;
        public const string Root = "Assets/Echappee/Art/PremiumTreePolyHaven";
        public const string ReportPath = "_bmad-output/unity-test-results/myb-112-premium-tree-runtime-set.txt";

        private const string ModelPath = Root + "/Models/MYB_PremiumTreePolyHaven.fbx";
        private const string MaterialsFolder = Root + "/Materials";
        private const string PrefabsFolder = Root + "/Prefabs";
        private const string SourceManifestPath = Root + "/Source/MYB_PremiumTreePolyHaven.source.json";
        private const string BarkMaterialPath = MaterialsFolder + "/MYB_PremiumTree_BarkPolyHaven.mat";
        private const string MossMaterialPath = MaterialsFolder + "/MYB_PremiumTree_MossPolyHaven.mat";
        private const string FoliageMaterialPath = MaterialsFolder + "/MYB_PremiumTree_StylizedFoliage.mat";

        private static readonly string[] LegacyBaselineTreePrefabs =
        {
            "Assets/Echappee/Art/MYB96BlenderGenerated/Prefabs/MYB96_AlpinePineTall.prefab",
            "Assets/Echappee/Art/MYB96BlenderGenerated/Prefabs/MYB96_AlpinePineSmall.prefab"
        };

        private static readonly VariantSpec[] Variants =
        {
            new("A", 5.6f, 1.00f, 0f, 0.00f, new Color(0.12f, 0.34f, 0.22f, 1f)),
            new("B", 6.1f, 0.82f, -7f, 0.18f, new Color(0.10f, 0.29f, 0.20f, 1f)),
            new("C", 5.1f, 1.16f, 6f, -0.12f, new Color(0.16f, 0.38f, 0.21f, 1f)),
            new("D", 6.5f, 0.92f, 11f, 0.10f, new Color(0.09f, 0.25f, 0.18f, 1f)),
            new("E", 5.4f, 1.28f, -12f, -0.08f, new Color(0.18f, 0.33f, 0.18f, 1f))
        };

        public static bool UseLegacyBaselineComparison { get; set; }

        [MenuItem("Tools/MYB-112/Build Premium Tree Runtime Set")]
        public static void BuildRuntimeSetFromMenu()
        {
            Debug.Log(BuildAndValidate());
        }

        public static string BuildAndValidateCli()
        {
            return BuildAndValidate();
        }

        public static string BuildAndValidate()
        {
            var report = new ValidationReport();
            EnsureFolders();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            ValidateSources(report);
            ConfigureSourceImporters(report);

            var bark = CreatePbrMaterial(
                BarkMaterialPath,
                Root + "/Textures/pine_bark_diff_1k.jpg",
                Root + "/Textures/pine_bark_nor_gl_1k.jpg",
                Root + "/Textures/pine_bark_arm_1k.jpg",
                new Color(0.55f, 0.38f, 0.25f, 1f),
                0.28f,
                report);
            var moss = CreatePbrMaterial(
                MossMaterialPath,
                Root + "/Textures/moss_wood_diff_1k.jpg",
                Root + "/Textures/moss_wood_nor_gl_1k.jpg",
                Root + "/Textures/moss_wood_arm_1k.jpg",
                new Color(0.32f, 0.42f, 0.20f, 1f),
                0.22f,
                report);
            var foliage = CreateFoliageMaterial(report);

            for (var index = 0; index < Variants.Length; index++)
            {
                CreateVariantPrefab(index, bark, moss, foliage, report);
            }

            ValidateRuntimeSet(report);
            return WriteReport(report);
        }

        public static string GetVariantPrefabPath(int placementIndex)
        {
            if (UseLegacyBaselineComparison)
            {
                return LegacyBaselineTreePrefabs[Math.Abs(placementIndex) % LegacyBaselineTreePrefabs.Length];
            }

            var index = Math.Abs(placementIndex) % Variants.Length;
            return PrefabsFolder + "/MYB112_PremiumTree_" + Variants[index].Suffix + ".prefab";
        }

        public static string[] RuntimeVariantPrefabPaths()
        {
            return Variants
                .Select((variant, _) => PrefabsFolder + "/MYB112_PremiumTree_" + variant.Suffix + ".prefab")
                .ToArray();
        }

        private static void EnsureFolders()
        {
            CreateFolderRecursive(Root);
            CreateFolderRecursive(Root + "/Models");
            CreateFolderRecursive(Root + "/Textures");
            CreateFolderRecursive(MaterialsFolder);
            CreateFolderRecursive(PrefabsFolder);
            CreateFolderRecursive(Root + "/Source");
        }

        private static void ValidateSources(ValidationReport report)
        {
            foreach (var path in new[]
            {
                ModelPath,
                SourceManifestPath,
                Root + "/Textures/pine_bark_diff_1k.jpg",
                Root + "/Textures/pine_bark_nor_gl_1k.jpg",
                Root + "/Textures/pine_bark_arm_1k.jpg",
                Root + "/Textures/moss_wood_diff_1k.jpg",
                Root + "/Textures/moss_wood_nor_gl_1k.jpg",
                Root + "/Textures/moss_wood_arm_1k.jpg"
            })
            {
                if (!File.Exists(ProjectRelativeToAbsolute(path)))
                {
                    report.Failures.Add("Missing source file: " + path);
                }
            }
        }

        private static void ConfigureSourceImporters(ValidationReport report)
        {
            var importer = AssetImporter.GetAtPath(ModelPath) as ModelImporter;
            if (importer == null)
            {
                report.Failures.Add("Model importer missing: " + ModelPath);
            }
            else
            {
                importer.globalScale = 1f;
                importer.importAnimation = false;
                importer.importCameras = false;
                importer.importLights = false;
                importer.importNormals = ModelImporterNormals.Import;
                importer.importTangents = ModelImporterTangents.CalculateMikk;
                importer.materialImportMode = ModelImporterMaterialImportMode.None;
                importer.isReadable = false;
                importer.SaveAndReimport();
                report.Notes.Add("Configured source FBX without embedded materials; runtime variants share Poly Haven bark/moss materials and stylized foliage.");
            }

            ConfigureTexture(Root + "/Textures/pine_bark_diff_1k.jpg", TextureImporterType.Default, true, report);
            ConfigureTexture(Root + "/Textures/pine_bark_nor_gl_1k.jpg", TextureImporterType.NormalMap, false, report);
            ConfigureTexture(Root + "/Textures/pine_bark_arm_1k.jpg", TextureImporterType.Default, false, report);
            ConfigureTexture(Root + "/Textures/moss_wood_diff_1k.jpg", TextureImporterType.Default, true, report);
            ConfigureTexture(Root + "/Textures/moss_wood_nor_gl_1k.jpg", TextureImporterType.NormalMap, false, report);
            ConfigureTexture(Root + "/Textures/moss_wood_arm_1k.jpg", TextureImporterType.Default, false, report);
        }

        private static void ConfigureTexture(string path, TextureImporterType type, bool srgb, ValidationReport report)
        {
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
            {
                report.Failures.Add("Texture importer missing: " + path);
                return;
            }

            importer.textureType = type;
            importer.sRGBTexture = srgb;
            importer.maxTextureSize = 1024;
            importer.mipmapEnabled = true;
            importer.textureCompression = TextureImporterCompression.CompressedHQ;
            importer.SaveAndReimport();
            report.Textures.Add(path);
        }

        private static void CreateVariantPrefab(int index, Material bark, Material moss, Material foliage, ValidationReport report)
        {
            var spec = Variants[index];
            var path = GetVariantPrefabPath(index);
            AssetDatabase.DeleteAsset(path);

            var model = AssetDatabase.LoadAssetAtPath<GameObject>(ModelPath);
            if (model == null)
            {
                report.Failures.Add("Could not load source model: " + ModelPath);
                return;
            }

            var root = new GameObject("MYB112_PremiumTree_" + spec.Suffix);
            var lod0 = new GameObject("LOD0_Premium");
            lod0.transform.SetParent(root.transform, false);
            var modelInstance = PrefabUtility.InstantiatePrefab(model) as GameObject;
            if (modelInstance == null)
            {
                UnityEngine.Object.DestroyImmediate(root);
                report.Failures.Add("Could not instantiate source model for variant " + spec.Suffix);
                return;
            }

            modelInstance.name = "MYB112_PremiumTree_" + spec.Suffix + "_Model";
            modelInstance.transform.SetParent(lod0.transform, false);
            modelInstance.transform.localRotation = Quaternion.Euler(0f, spec.LeanDegrees, spec.CanopyOffset);
            AssignTreeMaterials(modelInstance, bark, moss, foliage, spec.FoliageTint);
            FitModelToHeight(modelInstance, spec.Height);

            var lod1 = new GameObject("LOD1_StylizedProxy");
            lod1.transform.SetParent(root.transform, false);
            CreateProxyTree(lod1.transform, spec, bark, foliage, 0.72f, 3);

            var lod2 = new GameObject("LOD2_DistantSilhouette");
            lod2.transform.SetParent(root.transform, false);
            CreateProxyTree(lod2.transform, spec, bark, foliage, 0.48f, 2);

            foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
            {
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                renderer.receiveShadows = true;
            }

            RemoveColliders(root);
            var lodGroup = root.AddComponent<LODGroup>();
            lodGroup.SetLODs(new[]
            {
                new LOD(0.48f, lod0.GetComponentsInChildren<Renderer>(true)),
                new LOD(0.18f, lod1.GetComponentsInChildren<Renderer>(true)),
                new LOD(0.04f, lod2.GetComponentsInChildren<Renderer>(true))
            });
            lodGroup.fadeMode = LODFadeMode.CrossFade;
            lodGroup.animateCrossFading = true;
            lodGroup.RecalculateBounds();

            var saved = PrefabUtility.SaveAsPrefabAsset(root, path);
            UnityEngine.Object.DestroyImmediate(root);
            if (saved == null)
            {
                report.Failures.Add("Failed to save variant prefab: " + path);
                return;
            }

            report.Prefabs.Add(path);
            report.Notes.Add("Variant " + spec.Suffix + ": height " + spec.Height.ToString("0.0", CultureInfo.InvariantCulture) + "m, width scale " + spec.WidthScale.ToString("0.00", CultureInfo.InvariantCulture) + ", LODGroup 0.48/0.18/0.04.");
        }

        private static void CreateProxyTree(Transform parent, VariantSpec spec, Material bark, Material foliage, float scale, int crownCount)
        {
            var trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            trunk.name = parent.name + "_Trunk";
            trunk.transform.SetParent(parent, false);
            trunk.transform.localPosition = Vector3.up * (spec.Height * 0.28f * scale);
            trunk.transform.localRotation = Quaternion.Euler(0f, spec.LeanDegrees * 0.4f, spec.CanopyOffset * 0.4f);
            trunk.transform.localScale = new Vector3(0.22f * spec.WidthScale * scale, spec.Height * 0.28f * scale, 0.22f * spec.WidthScale * scale);
            SetMaterial(trunk, bark);

            for (var i = 0; i < crownCount; i++)
            {
                var crown = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                crown.name = parent.name + "_Crown_" + i.ToString("00", CultureInfo.InvariantCulture);
                crown.transform.SetParent(parent, false);
                var crown01 = i / (float)Mathf.Max(1, crownCount - 1);
                crown.transform.localPosition = new Vector3(spec.CanopyOffset * 0.025f * i, spec.Height * (0.48f + crown01 * 0.18f) * scale, 0f);
                crown.transform.localRotation = Quaternion.Euler(180f, spec.LeanDegrees + i * 21f, 0f);
                var radius = (1.15f - crown01 * 0.28f) * spec.WidthScale * scale;
                crown.transform.localScale = new Vector3(radius, 0.34f * scale, radius);
                SetMaterial(crown, foliage);
            }
        }

        private static void AssignTreeMaterials(GameObject modelInstance, Material bark, Material moss, Material foliage, Color foliageTint)
        {
            SetColor(foliage, "_BaseColor", foliageTint);
            SetColor(foliage, "_Color", foliageTint);
            foreach (var renderer in modelInstance.GetComponentsInChildren<Renderer>(true))
            {
                var descriptor = (renderer.name + " " + GetMeshName(renderer)).ToLowerInvariant();
                var material = foliage;
                if (descriptor.Contains("wood") || descriptor.Contains("trunk") || descriptor.Contains("branch") || descriptor.Contains("bark"))
                {
                    material = bark;
                }
                else if (descriptor.Contains("moss"))
                {
                    material = moss;
                }

                var shared = renderer.sharedMaterials;
                for (var materialIndex = 0; materialIndex < shared.Length; materialIndex++)
                {
                    shared[materialIndex] = material;
                }

                renderer.sharedMaterials = shared.Length == 0 ? new[] { material } : shared;
            }
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

        private static void ValidateRuntimeSet(ValidationReport report)
        {
            var paths = RuntimeVariantPrefabPaths();
            if (paths.Length != VariantCount)
            {
                report.Failures.Add("Runtime set must expose exactly 5 variants.");
            }

            foreach (var path in paths)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null)
                {
                    report.Failures.Add("Missing runtime variant prefab: " + path);
                    continue;
                }

                var lodGroup = prefab.GetComponent<LODGroup>();
                if (lodGroup == null || lodGroup.GetLODs().Length != 3)
                {
                    report.Failures.Add("Variant missing 3-level LODGroup: " + path);
                }

                var triangles = CountTrianglesInHierarchy(prefab);
                report.Notes.Add(Path.GetFileNameWithoutExtension(path) + " total stored triangles across LODs: " + triangles.ToString(CultureInfo.InvariantCulture));
                if (triangles <= 0 || triangles > 19000)
                {
                    report.Failures.Add("Variant triangle guardrail failed for " + path + ": " + triangles.ToString(CultureInfo.InvariantCulture));
                }
            }

            if (!File.Exists(ProjectRelativeToAbsolute(SourceManifestPath)))
            {
                report.Failures.Add("Poly Haven source manifest missing from runtime asset folder.");
            }
        }

        private static Material CreatePbrMaterial(string materialPath, string albedoPath, string normalPath, string occlusionPath, Color tint, float smoothness, ValidationReport report)
        {
            var material = MaterialAt(materialPath);
            SetTexture(material, "_BaseMap", LoadTexture(albedoPath, report));
            SetTexture(material, "_MainTex", LoadTexture(albedoPath, report));
            SetTexture(material, "_BumpMap", LoadTexture(normalPath, report));
            SetTexture(material, "_OcclusionMap", LoadTexture(occlusionPath, report));
            SetColor(material, "_BaseColor", tint);
            SetColor(material, "_Color", tint);
            SetFloat(material, "_Smoothness", smoothness);
            SetFloat(material, "_BumpScale", 0.38f);
            SetFloat(material, "_OcclusionStrength", 0.48f);
            material.EnableKeyword("_NORMALMAP");
            EditorUtility.SetDirty(material);
            report.Materials.Add(materialPath);
            return material;
        }

        private static Material CreateFoliageMaterial(ValidationReport report)
        {
            var material = MaterialAt(FoliageMaterialPath);
            SetColor(material, "_BaseColor", new Color(0.12f, 0.34f, 0.22f, 1f));
            SetColor(material, "_Color", new Color(0.12f, 0.34f, 0.22f, 1f));
            SetFloat(material, "_Smoothness", 0.24f);
            EditorUtility.SetDirty(material);
            report.Materials.Add(FoliageMaterialPath);
            return material;
        }

        private static Material MaterialAt(string assetPath)
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
            if (material != null)
            {
                return material;
            }

            var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            material = new Material(shader)
            {
                name = Path.GetFileNameWithoutExtension(assetPath)
            };
            AssetDatabase.CreateAsset(material, assetPath);
            return material;
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

        private static void SetMaterial(GameObject gameObject, Material material)
        {
            var renderer = gameObject.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = material;
            }
        }

        private static string GetMeshName(Renderer renderer)
        {
            var filter = renderer.GetComponent<MeshFilter>();
            return filter != null && filter.sharedMesh != null ? filter.sharedMesh.name : string.Empty;
        }

        private static Bounds GetWorldBounds(GameObject gameObject)
        {
            var renderers = gameObject.GetComponentsInChildren<Renderer>(true);
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

        private static int CountTrianglesInHierarchy(GameObject gameObject)
        {
            return gameObject.GetComponentsInChildren<MeshFilter>(true)
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

        private static void RemoveColliders(GameObject gameObject)
        {
            foreach (var collider in gameObject.GetComponentsInChildren<Collider>(true))
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

        private static string WriteReport(ValidationReport report)
        {
            var reportFullPath = Path.Combine(GetRepoRoot(), ReportPath);
            Directory.CreateDirectory(Path.GetDirectoryName(reportFullPath) ?? GetRepoRoot());

            var lines = new List<string>
            {
                "MYB-112 Premium Tree Runtime Set",
                "Generated: " + DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture),
                "Status: " + (report.Failures.Count == 0 ? "PASS" : "FAIL"),
                "Variant count: " + VariantCount.ToString(CultureInfo.InvariantCulture),
                "LOD policy: LOD0 premium source model, LOD1 stylized proxy, LOD2 distant silhouette.",
                "Performance rule: 5 variants are fixed; density, LOD, shadows, foliage and texture cost are tuning levers.",
                string.Empty,
                "Prefabs:"
            };
            lines.AddRange(report.Prefabs.Select(path => "- " + path));
            lines.Add(string.Empty);
            lines.Add("Textures:");
            lines.AddRange(report.Textures.Select(path => "- " + path));
            lines.Add(string.Empty);
            lines.Add("Materials:");
            lines.AddRange(report.Materials.Select(path => "- " + path));
            lines.Add(string.Empty);
            lines.Add("Notes:");
            lines.AddRange(report.Notes.Select(note => "- " + note));
            lines.Add(string.Empty);
            lines.Add("Failures:");
            lines.AddRange(report.Failures.Count == 0 ? new[] { "- None." } : report.Failures.Select(failure => "- " + failure));

            File.WriteAllLines(reportFullPath, lines, Encoding.UTF8);
            if (report.Failures.Count > 0)
            {
                throw new InvalidOperationException("MYB-112 premium tree runtime set failed. See " + reportFullPath);
            }

            return reportFullPath;
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
            return Path.Combine(GetUnityProjectRoot(), assetPath);
        }

        private static string GetUnityProjectRoot()
        {
            return Directory.GetParent(Application.dataPath)?.FullName ?? Application.dataPath;
        }

        private static string GetRepoRoot()
        {
            var directory = new DirectoryInfo(Application.dataPath);
            while (directory != null && !Directory.Exists(Path.Combine(directory.FullName, ".git")))
            {
                directory = directory.Parent;
            }

            return directory?.FullName ?? Directory.GetCurrentDirectory();
        }

        private readonly struct VariantSpec
        {
            public VariantSpec(string suffix, float height, float widthScale, float leanDegrees, float canopyOffset, Color foliageTint)
            {
                Suffix = suffix;
                Height = height;
                WidthScale = widthScale;
                LeanDegrees = leanDegrees;
                CanopyOffset = canopyOffset;
                FoliageTint = foliageTint;
            }

            public string Suffix { get; }
            public float Height { get; }
            public float WidthScale { get; }
            public float LeanDegrees { get; }
            public float CanopyOffset { get; }
            public Color FoliageTint { get; }
        }

        private sealed class ValidationReport
        {
            public List<string> Textures { get; } = new();
            public List<string> Materials { get; } = new();
            public List<string> Prefabs { get; } = new();
            public List<string> Notes { get; } = new();
            public List<string> Failures { get; } = new();
        }
    }
}

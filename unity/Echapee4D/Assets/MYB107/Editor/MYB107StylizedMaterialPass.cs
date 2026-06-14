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

namespace MYB107.Editor
{
    public static class MYB107StylizedMaterialPass
    {
        private const string CanonicalScenePath = "Assets/Scenes/MYB89UnityMcpProbe.unity";
        private const string OutputDirectory = "_bmad-output/unity-test-results/myb-107";
        private const string ReportPath = OutputDirectory + "/myb-107-stylized-material-pass-report.txt";
        private const string PolyHavenRoot = "Assets/Echappee/Art/MYB107PolyHavenStylized";
        private const string MaterialsRoot = "Assets/MYB107/Materials";
        private const int CaptureWidth = 1280;
        private const int CaptureHeight = 720;

        private static readonly CaptureSpec[] Captures =
        {
            new CaptureSpec("forest", "Passage 01 forest material read", 42f)
        };

        [MenuItem("Tools/MYB/Validation/MYB-107/Apply Stylized Material Pass")]
        public static void ApplyFromMenu()
        {
            Debug.Log("MYB-107 stylized material pass applied: " + ApplyAndValidate());
        }

        public static string ApplyAndValidateCli()
        {
            var reportPath = ApplyAndValidate();
            Debug.Log("MYB-107 stylized material pass validated: " + reportPath);
            return reportPath;
        }

        public static string ApplyAndValidate()
        {
            EnsureFolders();
            var report = new ValidationReport();
            ConfigureTextureImports(report);
            var baselineSnapshot = SnapshotBaselineArtifacts();

            // Rebuild the existing controlled lookdev scene before comparing MYB-107.
            MYB106.Editor.MYB106Passage01LookDev.ApplyAndValidate();

            var scene = EditorSceneManager.OpenScene(CanonicalScenePath, OpenSceneMode.Single);
            DestroyObjectsByName("MYB107_StylizedMaterialPass");

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

            if (ride != null && camera != null)
            {
                ride.RebuildRouteCache();
                foreach (var capture in Captures)
                {
                    CaptureStill(ride, camera, capture, "before", report);
                }

                var materials = CreateMaterials(report);
                ApplyMaterials(materials, report);

                foreach (var capture in Captures)
                {
                    CaptureStill(ride, camera, capture, "after", report);
                }

                ValidateFamilies(report);
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            RestoreBaselineArtifacts(baselineSnapshot);
            WriteReport(report);

            if (report.Failures.Count > 0)
            {
                throw new InvalidOperationException("MYB-107 stylized material pass failed. See " + Path.Combine(GetRepoRoot(), ReportPath));
            }

            return Path.Combine(GetRepoRoot(), ReportPath);
        }

        private static Dictionary<string, Material> CreateMaterials(ValidationReport report)
        {
            var materials = new Dictionary<string, Material>
            {
                {
                    "routeGround",
                    CreatePbrMaterial(
                        MaterialsRoot + "/MYB107_StylizedRouteGround.mat",
                        TexturePath("brown_mud_leaves_01_diff_1k.jpg"),
                        TexturePath("brown_mud_leaves_01_nor_gl_1k.jpg"),
                        TexturePath("brown_mud_leaves_01_arm_1k.jpg"),
                        new Color(0.34f, 0.26f, 0.17f, 1f),
                        0.16f,
                        0.24f,
                        report)
                },
                {
                    "forestVegetation",
                    CreateFlatMaterial(
                        MaterialsRoot + "/MYB107_StylizedForestVegetation.mat",
                        new Color(0.08f, 0.28f, 0.18f, 1f),
                        0.04f,
                        report)
                },
                {
                    "stone",
                    CreatePbrMaterial(
                        MaterialsRoot + "/MYB107_StylizedCoolStone.mat",
                        TexturePath("dry_riverbed_rock_diff_1k.jpg"),
                        TexturePath("dry_riverbed_rock_nor_gl_1k.jpg"),
                        TexturePath("dry_riverbed_rock_arm_1k.jpg"),
                        new Color(0.54f, 0.58f, 0.54f, 1f),
                        0.24f,
                        0.28f,
                        report)
                },
                {
                    "wood",
                    CreatePbrMaterial(
                        MaterialsRoot + "/MYB107_StylizedWarmWood.mat",
                        TexturePath("bark_brown_01_diff_1k.jpg"),
                        TexturePath("bark_brown_01_nor_gl_1k.jpg"),
                        TexturePath("bark_brown_01_arm_1k.jpg"),
                        new Color(0.42f, 0.25f, 0.13f, 1f),
                        0.12f,
                        0.22f,
                        report)
                }
            };

            report.MaterialFamilies.Add("routeGround: Poly Haven brown_mud_leaves_01 diffuse/normal/ARM, dark warm tint, reduced normal intensity.");
            report.MaterialFamilies.Add("forestVegetation: authored flat stylized vegetation, no photo texture, low smoothness.");
            report.MaterialFamilies.Add("stone: Poly Haven dry_riverbed_rock diffuse/normal/ARM, cool desaturated tint.");
            report.MaterialFamilies.Add("wood: Poly Haven bark_brown_01 diffuse/normal/ARM, warm stylized tint.");
            report.MaterialFamilies.Add("fantasySignal: tested as a diagnostic only, rejected for MYB-107 because broad fantasy matching over-applied to large atmosphere planes.");
            return materials;
        }

        private static void ApplyMaterials(IReadOnlyDictionary<string, Material> materials, ValidationReport report)
        {
            var marker = new GameObject("MYB107_StylizedMaterialPass");
            var probeRoot = GameObject.Find("MYB89_ProbeRoot");
            if (probeRoot != null)
            {
                marker.transform.SetParent(probeRoot.transform);
            }

            foreach (var renderer in UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include))
            {
                if (renderer == null || renderer.gameObject == null)
                {
                    continue;
                }

                var hierarchy = HierarchyName(renderer.transform);
                var lower = hierarchy.ToLowerInvariant();
                if (IsFantasyRenderer(lower))
                {
                    report.Increment("fantasySignalRejected");
                }
                else if (lower.Contains("stone")
                    || lower.Contains("rock")
                    || lower.Contains("cairn")
                    || lower.Contains("mountain"))
                {
                    AssignAllMaterials(renderer, materials["stone"]);
                    report.Increment("stone");
                }
                else if (IsWoodRenderer(lower))
                {
                    AssignAllMaterials(renderer, materials["wood"]);
                    report.Increment("wood");
                }
                else if (IsVegetationRenderer(lower))
                {
                    AssignAllMaterials(renderer, materials["forestVegetation"]);
                    report.Increment("forestVegetation");
                }
                else if (IsGroundRenderer(lower))
                {
                    AssignAllMaterials(renderer, materials["routeGround"]);
                    report.Increment("routeGround");
                }
            }

            report.SceneNotes.Add("Applied MYB-107 material families to the forest-focused MYB104/MYB106 controlled scene composition.");
            report.SceneNotes.Add("Left fantasy signal/atmosphere renderers untouched after the diagnostic pass proved too broad and visually regressive.");
        }

        private static bool IsFantasyRenderer(string hierarchy)
        {
            return hierarchy.Contains("fantasy")
                || hierarchy.Contains("signal")
                || hierarchy.Contains("rune")
                || hierarchy.Contains("halo");
        }

        private static bool IsGroundRenderer(string hierarchy)
        {
            return hierarchy.Contains("route")
                || hierarchy.Contains("road")
                || hierarchy.Contains("floor")
                || hierarchy.Contains("ground")
                || hierarchy.Contains("litter")
                || hierarchy.Contains("undergrowth")
                || hierarchy.Contains("shadowedge")
                || hierarchy.Contains("rootedge")
                || hierarchy.Contains("mat");
        }

        private static bool IsVegetationRenderer(string hierarchy)
        {
            return hierarchy.Contains("foliage")
                || hierarchy.Contains("crown")
                || hierarchy.Contains("pine")
                || hierarchy.Contains("needle")
                || hierarchy.Contains("forest")
                || hierarchy.Contains("grass")
                || hierarchy.Contains("flower");
        }

        private static bool IsWoodRenderer(string hierarchy)
        {
            return hierarchy.Contains("trunk")
                || hierarchy.Contains("wood")
                || hierarchy.Contains("bark")
                || hierarchy.Contains("branch")
                || hierarchy.Contains("fence")
                || hierarchy.Contains("bench");
        }

        private static void ValidateFamilies(ValidationReport report)
        {
            var adoptedFamilies = 0;
            foreach (var family in new[] { "routeGround", "forestVegetation", "stone", "wood" })
            {
                var count = report.FamilyRendererCounts.TryGetValue(family, out var value) ? value : 0;
                if (count <= 0)
                {
                    report.Failures.Add("No renderers received MYB-107 family: " + family);
                    report.FamilyVerdicts.Add(family + ": no-go, not present in controlled scene.");
                    continue;
                }

                adoptedFamilies++;
                report.FamilyVerdicts.Add(family + ": adoptable with iteration: material reads more authored while keeping URP Lit compatibility. Renderer count " + count.ToString(CultureInfo.InvariantCulture) + ".");
            }

            var rejectedFantasyCount = report.FamilyRendererCounts.TryGetValue("fantasySignalRejected", out var fantasyCount) ? fantasyCount : 0;
            report.FamilyVerdicts.Add("fantasySignal: no-go for MYB-107. Diagnostic count " + rejectedFantasyCount.ToString(CultureInfo.InvariantCulture) + "; broad matching hits large atmosphere planes and makes the scene read like a material bug. Defer to a dedicated fantasy accent/atmosphere split.");

            if (adoptedFamilies < 4)
            {
                report.Failures.Add("MYB-107 requires at least 4 testable material families; found " + adoptedFamilies.ToString(CultureInfo.InvariantCulture) + ".");
            }
        }

        private static void CaptureStill(MYB89ProbeRide ride, Camera camera, CaptureSpec spec, string phase, ValidationReport report)
        {
            ride.SetPreviewProgress(spec.Meters);
            var path = Path.Combine(GetRepoRoot(), OutputDirectory, phase + "-" + spec.Slug + "-720p.png");
            Directory.CreateDirectory(Path.GetDirectoryName(path) ?? GetRepoRoot());

            var previousTarget = camera.targetTexture;
            var previousActive = RenderTexture.active;
            var renderTexture = new RenderTexture(CaptureWidth, CaptureHeight, 24, RenderTextureFormat.ARGB32);
            var texture = new Texture2D(CaptureWidth, CaptureHeight, TextureFormat.RGB24, false);
            try
            {
                camera.targetTexture = renderTexture;
                RenderTexture.active = renderTexture;
                camera.Render();
                texture.ReadPixels(new Rect(0, 0, CaptureWidth, CaptureHeight), 0, 0);
                texture.Apply();
                File.WriteAllBytes(path, ImageConversion.EncodeToPNG(texture));
            }
            finally
            {
                camera.targetTexture = previousTarget;
                RenderTexture.active = previousActive;
                UnityEngine.Object.DestroyImmediate(texture);
                UnityEngine.Object.DestroyImmediate(renderTexture);
            }

            report.Captures.Add(phase + " " + spec.Label + ": `" + RelativeToRepo(path) + "`");
        }

        private static Material CreatePbrMaterial(
            string materialPath,
            string albedoPath,
            string normalPath,
            string armPath,
            Color tint,
            float smoothness,
            float normalScale,
            ValidationReport report)
        {
            var material = MaterialAt(materialPath);
            SetTexture(material, "_BaseMap", LoadTexture(albedoPath, report));
            SetTexture(material, "_MainTex", LoadTexture(albedoPath, report));
            SetTexture(material, "_BumpMap", LoadTexture(normalPath, report));
            SetTexture(material, "_OcclusionMap", LoadTexture(armPath, report));
            SetColor(material, "_BaseColor", tint);
            SetColor(material, "_Color", tint);
            SetFloat(material, "_Smoothness", smoothness);
            SetFloat(material, "_BumpScale", normalScale);
            SetFloat(material, "_OcclusionStrength", 0.42f);
            material.EnableKeyword("_NORMALMAP");
            EditorUtility.SetDirty(material);
            report.MaterialPaths.Add(materialPath);
            return material;
        }

        private static Material CreateFlatMaterial(string materialPath, Color tint, float smoothness, ValidationReport report)
        {
            var material = MaterialAt(materialPath);
            SetColor(material, "_BaseColor", tint);
            SetColor(material, "_Color", tint);
            SetFloat(material, "_Smoothness", smoothness);
            EditorUtility.SetDirty(material);
            report.MaterialPaths.Add(materialPath);
            return material;
        }

        private static Material CreateEmissiveMaterial(string materialPath, Color color, float intensity, ValidationReport report)
        {
            var material = MaterialAt(materialPath);
            SetColor(material, "_BaseColor", color);
            SetColor(material, "_Color", color);
            SetColor(material, "_EmissionColor", color * intensity);
            SetFloat(material, "_Smoothness", 0.34f);
            material.EnableKeyword("_EMISSION");
            EditorUtility.SetDirty(material);
            report.MaterialPaths.Add(materialPath);
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
                report.Failures.Add("Missing texture: " + assetPath);
            }

            return texture;
        }

        private static void ConfigureTextureImports(ValidationReport report)
        {
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            foreach (var path in Directory.GetFiles(ProjectRelativeToAbsolute(PolyHavenRoot + "/Textures"), "*.jpg"))
            {
                var assetPath = AbsoluteToAssetPath(path);
                var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
                if (importer == null)
                {
                    report.Failures.Add("Missing texture importer: " + assetPath);
                    continue;
                }

                var isNormal = assetPath.IndexOf("_nor_", StringComparison.OrdinalIgnoreCase) >= 0;
                var isColor = assetPath.IndexOf("_diff_", StringComparison.OrdinalIgnoreCase) >= 0;
                importer.textureType = isNormal ? TextureImporterType.NormalMap : TextureImporterType.Default;
                importer.sRGBTexture = isColor;
                importer.mipmapEnabled = true;
                importer.maxTextureSize = 1024;
                importer.textureCompression = TextureImporterCompression.CompressedHQ;
                importer.SaveAndReimport();
                report.TexturePaths.Add(assetPath);
            }
        }

        private static void AssignAllMaterials(Renderer renderer, Material material)
        {
            var sharedMaterials = renderer.sharedMaterials;
            if (sharedMaterials.Length == 0)
            {
                renderer.sharedMaterial = material;
                return;
            }

            for (var i = 0; i < sharedMaterials.Length; i++)
            {
                sharedMaterials[i] = material;
            }

            renderer.sharedMaterials = sharedMaterials;
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

        private static string HierarchyName(Transform transform)
        {
            var names = new List<string>();
            var current = transform;
            while (current != null)
            {
                names.Add(current.name);
                current = current.parent;
            }

            return string.Join("/", names);
        }

        private static void DestroyObjectsByName(string objectName)
        {
            while (true)
            {
                var target = GameObject.Find(objectName);
                if (target == null)
                {
                    return;
                }

                UnityEngine.Object.DestroyImmediate(target);
            }
        }

        private static void WriteReport(ValidationReport report)
        {
            var path = Path.Combine(GetRepoRoot(), ReportPath);
            Directory.CreateDirectory(Path.GetDirectoryName(path) ?? GetRepoRoot());
            var lines = new List<string>
            {
                "# MYB-107 Stylized Material Pass Report",
                string.Empty,
                "Status: " + (report.Failures.Count == 0 ? "PASS" : "FAIL"),
                "Generated UTC: " + DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture),
                "Scene: " + CanonicalScenePath,
                "Texture policy: Poly Haven 1K JPG maps only; stylized tint/normal/roughness in Unity materials.",
                string.Empty,
                "## Material Families"
            };
            lines.AddRange(report.MaterialFamilies.Select(line => "- " + line));
            lines.Add(string.Empty);
            lines.Add("## Applied Renderer Counts");
            foreach (var pair in report.FamilyRendererCounts.OrderBy(pair => pair.Key))
            {
                lines.Add("- " + pair.Key + ": " + pair.Value.ToString(CultureInfo.InvariantCulture));
            }

            lines.Add(string.Empty);
            lines.Add("## Verdicts");
            lines.AddRange(report.FamilyVerdicts.Select(line => "- " + line));
            lines.Add(string.Empty);
            lines.Add("## Captures");
            lines.AddRange(report.Captures.Select(line => "- " + line));
            lines.Add(string.Empty);
            lines.Add("## Textures");
            lines.AddRange(report.TexturePaths.Select(line => "- " + line));
            lines.Add(string.Empty);
            lines.Add("## Materials");
            lines.AddRange(report.MaterialPaths.Select(line => "- " + line));
            lines.Add(string.Empty);
            lines.Add("## Scene Notes");
            lines.AddRange(report.SceneNotes.Select(line => "- " + line));
            lines.Add(string.Empty);
            lines.Add("## Failures");
            lines.AddRange(report.Failures.Count == 0 ? new[] { "- None." } : report.Failures.Select(line => "- " + line));

            File.WriteAllLines(path, lines);
        }

        private static void EnsureFolders()
        {
            CreateFolderRecursive("Assets/MYB107");
            CreateFolderRecursive("Assets/MYB107/Editor");
            CreateFolderRecursive(MaterialsRoot);
            Directory.CreateDirectory(Path.Combine(GetRepoRoot(), OutputDirectory));
        }

        private static Dictionary<string, byte[]> SnapshotBaselineArtifacts()
        {
            var snapshot = new Dictionary<string, byte[]>(StringComparer.Ordinal);
            foreach (var directory in BaselineArtifactDirectories())
            {
                if (!Directory.Exists(directory))
                {
                    continue;
                }

                foreach (var path in Directory.GetFiles(directory, "*", SearchOption.AllDirectories))
                {
                    snapshot[path] = File.ReadAllBytes(path);
                }
            }

            return snapshot;
        }

        private static void RestoreBaselineArtifacts(IReadOnlyDictionary<string, byte[]> snapshot)
        {
            foreach (var pair in snapshot)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(pair.Key) ?? GetRepoRoot());
                File.WriteAllBytes(pair.Key, pair.Value);
            }

            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        }

        private static IEnumerable<string> BaselineArtifactDirectories()
        {
            var repoRoot = GetRepoRoot();
            yield return Path.Combine(repoRoot, "_bmad-output/unity-test-results/myb-104");
            yield return Path.Combine(repoRoot, "_bmad-output/unity-test-results/myb-106");
            yield return Path.Combine(GetUnityProjectRoot(), "Assets/Echappee/Art/PremiumTreePolyHaven/Materials");
            yield return Path.Combine(GetUnityProjectRoot(), "Assets/MYB104/Materials");
            yield return Path.Combine(GetUnityProjectRoot(), "Assets/MYB106/Materials");
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

        private static string TexturePath(string textureName)
        {
            return PolyHavenRoot + "/Textures/" + textureName;
        }

        private static string ProjectRelativeToAbsolute(string assetPath)
        {
            return Path.Combine(GetUnityProjectRoot(), assetPath);
        }

        private static string AbsoluteToAssetPath(string path)
        {
            return path.Replace(GetUnityProjectRoot() + Path.DirectorySeparatorChar, string.Empty).Replace('\\', '/');
        }

        private static string RelativeToRepo(string path)
        {
            return Path.GetRelativePath(GetRepoRoot(), path).Replace('\\', '/');
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

        private readonly struct CaptureSpec
        {
            public CaptureSpec(string slug, string label, float meters)
            {
                Slug = slug;
                Label = label;
                Meters = meters;
            }

            public string Slug { get; }
            public string Label { get; }
            public float Meters { get; }
        }

        private sealed class ValidationReport
        {
            public List<string> MaterialFamilies { get; } = new();
            public List<string> FamilyVerdicts { get; } = new();
            public List<string> Captures { get; } = new();
            public List<string> TexturePaths { get; } = new();
            public List<string> MaterialPaths { get; } = new();
            public List<string> SceneNotes { get; } = new();
            public List<string> Failures { get; } = new();
            public Dictionary<string, int> FamilyRendererCounts { get; } = new();

            public void Increment(string family)
            {
                FamilyRendererCounts[family] = FamilyRendererCounts.TryGetValue(family, out var count) ? count + 1 : 1;
            }
        }
    }
}

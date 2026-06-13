using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using MYB89;
using MYB89.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MYB100.Editor
{
    public static class MYB100ImportedAssetOptimizer
    {
        private const string ArtRoot = "Assets/Echappee/Art";
        private const string CanonicalScenePath = "Assets/Scenes/MYB89UnityMcpProbe.unity";
        private const string MYB95ManifestPath = ArtRoot + "/MYB95GeneratedAssets.assetmanifest.json";
        private const string MYB96ManifestPath = ArtRoot + "/MYB96BlenderGenerated/GeneratedAssets.assetmanifest.json";
        private const string ThirdPartyManifestPath = ArtRoot + "/ThirdPartyAssets.assetmanifest.json";
        private const string ReportPath = "_bmad-output/unity-test-results/myb-100-imported-asset-adjustments.txt";
        private const string CapturePath = "_bmad-output/unity-test-results/myb-100-canonical-assets.png";
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

        private static readonly SceneAssetSpec[] SceneAssets =
        {
            new SceneAssetSpec(
                "Assets/Echappee/Art/MYB96BlenderGenerated/Prefabs/MYB96_HairpinChevronSign.prefab",
                "MYB100_HairpinChevronSign",
                78f,
                -1f,
                5.6f,
                1.6f),
            new SceneAssetSpec(
                "Assets/Echappee/Art/MYB96BlenderGenerated/Prefabs/MYB96_RoadReflectorPair.prefab",
                "MYB100_RoadReflectorPair",
                108f,
                1f,
                4.6f,
                1.0f),
            new SceneAssetSpec(
                "Assets/Echappee/Art/MYB95MeshyLantern/Prefabs/MYB95_MeshyLantern.prefab",
                "MYB100_MeshyLanternSignal",
                126f,
                1f,
                8.2f,
                2.6f),
            new SceneAssetSpec(
                "Assets/Echappee/Art/MYB95MeshyRelicFountain/Prefabs/MYB95_RelicFountain.prefab",
                "MYB100_MeshyRelicFountainSignal",
                132f,
                1f,
                10.6f,
                3.8f),
            new SceneAssetSpec(
                "Assets/Echappee/Art/MYB96BlenderGenerated/Prefabs/MYB96_ColDirectionSign.prefab",
                "MYB100_ColDirectionSign",
                150f,
                -1f,
                5.8f,
                2.2f),
            new SceneAssetSpec(
                "Assets/Echappee/Art/MYB95MeshyCharacter/Prefabs/MYB95_RouteGuardian_Direct.prefab",
                "MYB100_RouteGuardianLifeAccent",
                168f,
                -1f,
                11.5f,
                2.1f),
            new SceneAssetSpec(
                "Assets/Echappee/Art/MYB96BlenderGenerated/Prefabs/MYB96_WildflowerGrassPatch.prefab",
                "MYB100_WildflowerGrassPatch",
                186f,
                1f,
                4.8f,
                0.55f),
            new SceneAssetSpec(
                "Assets/Echappee/Art/MYB96BlenderGenerated/Prefabs/MYB96_AlpinePineTall.prefab",
                "MYB100_AlpinePineTall",
                202f,
                -1f,
                12.5f,
                5.2f),
            new SceneAssetSpec(
                "Assets/Echappee/Art/MYB96BlenderGenerated/Prefabs/MYB96_RoadsideRockCluster.prefab",
                "MYB100_RoadsideRockCluster",
                216f,
                1f,
                5.4f,
                0.7f),
            new SceneAssetSpec(
                "Assets/Echappee/Art/MYB96BlenderGenerated/Prefabs/MYB96_SummitArchMarker.prefab",
                "MYB100_SummitArchMarker",
                228f,
                0f,
                0f,
                3.2f)
        };

        [MenuItem("Tools/MYB-100/Apply Imported Asset Adjustments")]
        public static void ApplyAndValidateFromMenu()
        {
            ApplyAndValidate();
        }

        public static void ApplyAndValidateCli()
        {
            var reportPath = ApplyAndValidate();
            if (File.ReadAllText(Path.Combine(GetRepoRoot(), reportPath)).Contains("Status: fail"))
            {
                throw new InvalidOperationException("MYB-100 imported asset adjustment validation failed. See " + reportPath);
            }
        }

        public static string ApplyAndValidate()
        {
            var report = new ValidationReport();

            try
            {
                AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
                ConfigureTextureImporters(report);
                ConfigureModelImporters(report);
                WriteMYB95GeneratedManifest(report);

                MYB89ProbeBuilder.BuildScene();
                ApplyCanonicalSceneAdjustments(report);
                CaptureCanonicalScene(report);
                NormalizeSerializedWhitespace(report);

                ValidateManifests(report);
                ValidateImportSettings(report);
                ValidateCanonicalScene(report);
            }
            catch (Exception exception)
            {
                report.Failures.Add(exception.GetType().FullName + ": " + exception.Message);
                Debug.LogException(exception);
            }

            report.Status = report.Failures.Count == 0 ? "pass" : "fail";
            WriteReport(report);
            return ReportPath;
        }

        private static void ConfigureTextureImporters(ValidationReport report)
        {
            foreach (var assetPath in FindArtAssets(".png", ".jpg", ".jpeg"))
            {
                var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
                if (importer == null)
                {
                    report.Failures.Add("Missing texture importer: " + assetPath);
                    continue;
                }

                var textureType = IsNormalMap(assetPath) ? TextureImporterType.NormalMap : TextureImporterType.Default;
                var srgb = IsColorTexture(assetPath);
                var maxSize = TextureMaxSize(assetPath);
                var changed = importer.textureType != textureType ||
                    importer.sRGBTexture != srgb ||
                    importer.mipmapEnabled != true ||
                    importer.maxTextureSize != maxSize ||
                    importer.textureCompression != TextureImporterCompression.CompressedHQ;

                importer.textureType = textureType;
                importer.sRGBTexture = srgb;
                importer.mipmapEnabled = true;
                importer.maxTextureSize = maxSize;
                importer.textureCompression = TextureImporterCompression.CompressedHQ;

                if (changed)
                {
                    importer.SaveAndReimport();
                    report.TextureAdjustments.Add(assetPath + " -> " + TextureSummary(textureType, srgb, maxSize));
                }
                else
                {
                    report.TextureValidated.Add(assetPath + " -> " + TextureSummary(textureType, srgb, maxSize));
                }
            }
        }

        private static void ConfigureModelImporters(ValidationReport report)
        {
            foreach (var assetPath in FindArtAssets(".fbx"))
            {
                var importer = AssetImporter.GetAtPath(assetPath) as ModelImporter;
                if (importer == null)
                {
                    report.Failures.Add("Missing model importer: " + assetPath);
                    continue;
                }

                var animated = IsAnimatedModel(assetPath);
                var changed = importer.importAnimation != animated ||
                    importer.materialImportMode != ModelImporterMaterialImportMode.None ||
                    importer.importCameras ||
                    importer.importLights ||
                    importer.importVisibility ||
                    importer.importNormals != ModelImporterNormals.Import ||
                    importer.importTangents != ModelImporterTangents.CalculateMikk ||
                    importer.isReadable ||
                    !importer.useFileUnits;

                importer.importAnimation = animated;
                importer.useFileUnits = true;
                importer.materialImportMode = ModelImporterMaterialImportMode.None;
                importer.importCameras = false;
                importer.importLights = false;
                importer.importVisibility = false;
                importer.importNormals = ModelImporterNormals.Import;
                importer.importTangents = ModelImporterTangents.CalculateMikk;
                importer.isReadable = false;

                if (assetPath.Contains("HorseAnimated", StringComparison.OrdinalIgnoreCase))
                {
                    importer.animationType = ModelImporterAnimationType.Generic;
                    importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
                }

                if (changed)
                {
                    importer.SaveAndReimport();
                    report.ModelAdjustments.Add(assetPath + " -> " + (animated ? "animation kept" : "static import"));
                }
                else
                {
                    report.ModelValidated.Add(assetPath + " -> " + (animated ? "animation kept" : "static import"));
                }
            }
        }

        private static void WriteMYB95GeneratedManifest(ValidationReport report)
        {
            var manifest = string.Join("\n", new[]
            {
                "{",
                "  \"schemaVersion\": 1,",
                "  \"project\": \"mybike / Echappee 3D\",",
                "  \"ticket\": \"MYB-95\",",
                "  \"reviewedBy\": \"MYB-100\",",
                "  \"reviewedAt\": \"2026-06-13\",",
                "  \"generator\": {",
                "    \"tool\": \"Meshy\",",
                "    \"source\": \"generated during approved MYB-95 POC\",",
                "    \"externalServices\": [\"Meshy\"]",
                "  },",
                "  \"unityRoots\": [",
                "    \"Assets/Echappee/Art/MYB95MeshyLantern\",",
                "    \"Assets/Echappee/Art/MYB95MeshyRelicFountain\",",
                "    \"Assets/Echappee/Art/MYB95MeshyCharacter\"",
                "  ],",
                "  \"budgets\": {",
                "    \"maxTextureSize\": 2048,",
                "    \"texturePolicy\": \"2K cap for Meshy POC premium signal and animated actor textures; compressed HQ with mip maps.\"",
                "  },",
                "  \"assets\": [",
                "    {",
                "      \"id\": \"MYB95_MeshyLantern\",",
                "      \"family\": \"signal-premium\",",
                "      \"prefab\": \"Assets/Echappee/Art/MYB95MeshyLantern/Prefabs/MYB95_MeshyLantern.prefab\",",
                "      \"model\": \"Assets/Echappee/Art/MYB95MeshyLantern/Models/MYB95_MeshyLantern_LOD0_20k.fbx\",",
                "      \"textureRoot\": \"Assets/Echappee/Art/MYB95MeshyLantern/Textures\"",
                "    },",
                "    {",
                "      \"id\": \"MYB95_MeshyRelicFountain\",",
                "      \"family\": \"signal-premium\",",
                "      \"prefab\": \"Assets/Echappee/Art/MYB95MeshyRelicFountain/Prefabs/MYB95_RelicFountain.prefab\",",
                "      \"model\": \"Assets/Echappee/Art/MYB95MeshyRelicFountain/Models/MYB95_RelicFountain_LOD0_50k.fbx\",",
                "      \"textureRoot\": \"Assets/Echappee/Art/MYB95MeshyRelicFountain/Textures\"",
                "    },",
                "    {",
                "      \"id\": \"MYB95_RouteGuardian\",",
                "      \"family\": \"animated-actor\",",
                "      \"prefab\": \"Assets/Echappee/Art/MYB95MeshyCharacter/Prefabs/MYB95_RouteGuardian_Direct.prefab\",",
                "      \"models\": [",
                "        \"Assets/Echappee/Art/MYB95MeshyCharacter/Models/MYB95_RouteGuardian_Rigged.fbx\",",
                "        \"Assets/Echappee/Art/MYB95MeshyCharacter/Models/MYB95_RouteGuardian_Walking.fbx\",",
                "        \"Assets/Echappee/Art/MYB95MeshyCharacter/Models/MYB95_RouteGuardian_Running.fbx\"",
                "      ],",
                "      \"textureRoot\": \"Assets/Echappee/Art/MYB95MeshyCharacter/Textures\"",
                "    }",
                "  ]",
                "}",
                string.Empty
            });

            File.WriteAllText(ProjectRelativeToAbsolute(MYB95ManifestPath), manifest);
            AssetDatabase.ImportAsset(MYB95ManifestPath, ImportAssetOptions.ForceSynchronousImport);
            report.ManifestNotes.Add("Wrote generated-asset provenance manifest: " + MYB95ManifestPath);
        }

        private static void ApplyCanonicalSceneAdjustments(ValidationReport report)
        {
            var scene = EditorSceneManager.OpenScene(CanonicalScenePath, OpenSceneMode.Single);
            var primitiveSignal = GameObject.Find("MYB44_PremiumSignal_RelicLantern");
            if (primitiveSignal != null)
            {
                UnityEngine.Object.DestroyImmediate(primitiveSignal);
                report.SceneNotes.Add("Replaced primitive premium signal geometry.");
            }

            var existing = GameObject.Find("MYB100_OptimizedImportedAssets");
            if (existing != null)
            {
                UnityEngine.Object.DestroyImmediate(existing);
            }

            var root = new GameObject("MYB100_OptimizedImportedAssets");
            var premiumSignalAnchor = new GameObject("MYB44_PremiumSignal_RelicLantern");
            premiumSignalAnchor.transform.SetParent(root.transform);
            report.SceneNotes.Add("Kept canonical premium signal anchor for MYB-91 baseline compatibility.");

            var route = MYB89RideTrajectory.BuildSmoothedPoints(RoutePoints);
            foreach (var spec in SceneAssets)
            {
                PlaceSceneAsset(spec, root.transform, route, report);
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        }

        private static void PlaceSceneAsset(SceneAssetSpec spec, Transform parent, IReadOnlyList<Vector3> route, ValidationReport report)
        {
            if (!MYB89RideTrajectory.TrySample(route, spec.DistanceMeters, false, out var sample))
            {
                report.Failures.Add("Could not sample route for " + spec.Name);
                return;
            }

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(spec.AssetPath);
            if (prefab == null)
            {
                report.Failures.Add("Missing canonical scene prefab: " + spec.AssetPath);
                return;
            }

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            instance.name = spec.Name;
            instance.transform.SetParent(parent);

            var sideOffset = spec.Side == 0f ? 0f : RoadWidth * 0.5f + spec.LateralOffsetMeters;
            var placement = sample.Position + sample.Right * spec.Side * sideOffset;
            var rotation = Quaternion.LookRotation(sample.Forward, Vector3.up);
            instance.transform.SetPositionAndRotation(placement, rotation);
            FitToHeight(instance, spec.TargetHeightMeters);
            DisableColliders(instance);

            report.ScenePlacements.Add(spec.Name + " <- " + spec.AssetPath);
        }

        private static void CaptureCanonicalScene(ValidationReport report)
        {
            EditorSceneManager.OpenScene(CanonicalScenePath, OpenSceneMode.Single);
            var ride = UnityEngine.Object.FindAnyObjectByType<MYB89.MYB89ProbeRide>();
            var camera = Camera.main;
            if (ride == null || camera == null)
            {
                report.Failures.Add("Canonical scene must contain MYB89ProbeRide and Main Camera before MYB-100 capture.");
                return;
            }

            ride.SetPreviewProgress(124f);
            Canvas.ForceUpdateCanvases();

            var canvases = UnityEngine.Object.FindObjectsOfType<Canvas>(true);
            var previousStates = canvases.Select(canvas => canvas.enabled).ToArray();
            for (var i = 0; i < canvases.Length; i++)
            {
                canvases[i].enabled = false;
            }

            try
            {
                RenderCameraToPng(camera, 1280, 720, Path.Combine(GetRepoRoot(), CapturePath));
                report.SceneNotes.Add("Captured canonical asset proof: " + CapturePath);
            }
            finally
            {
                for (var i = 0; i < canvases.Length; i++)
                {
                    canvases[i].enabled = previousStates[i];
                }
            }
        }

        private static void ValidateManifests(ValidationReport report)
        {
            RequireAssetFile(ThirdPartyManifestPath, report);
            RequireAssetFile(MYB96ManifestPath, report);
            RequireAssetFile(MYB95ManifestPath, report);

            var thirdPartyManifest = File.Exists(ProjectRelativeToAbsolute(ThirdPartyManifestPath))
                ? File.ReadAllText(ProjectRelativeToAbsolute(ThirdPartyManifestPath))
                : string.Empty;
            if (thirdPartyManifest.Contains("MYB95", StringComparison.OrdinalIgnoreCase) ||
                thirdPartyManifest.Contains("MYB96", StringComparison.OrdinalIgnoreCase))
            {
                report.Failures.Add("Third-party manifest must stay third-party only.");
            }

            var myb95Manifest = File.Exists(ProjectRelativeToAbsolute(MYB95ManifestPath))
                ? File.ReadAllText(ProjectRelativeToAbsolute(MYB95ManifestPath))
                : string.Empty;
            foreach (var required in new[] { "MYB95_MeshyLantern", "MYB95_MeshyRelicFountain", "MYB95_RouteGuardian" })
            {
                if (!myb95Manifest.Contains(required, StringComparison.Ordinal))
                {
                    report.Failures.Add("MYB-95 generated manifest missing " + required);
                }
            }

            var myb96Manifest = File.Exists(ProjectRelativeToAbsolute(MYB96ManifestPath))
                ? File.ReadAllText(ProjectRelativeToAbsolute(MYB96ManifestPath))
                : string.Empty;
            if (!myb96Manifest.Contains("MYB96_SummitArchMarker", StringComparison.Ordinal) ||
                !myb96Manifest.Contains("MYB96_HairpinChevronSign", StringComparison.Ordinal))
            {
                report.Failures.Add("MYB-96 generated manifest missing route signage entries used by MYB-100.");
            }
        }

        private static void ValidateImportSettings(ValidationReport report)
        {
            foreach (var assetPath in FindArtAssets(".png", ".jpg", ".jpeg"))
            {
                var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
                if (importer == null)
                {
                    report.Failures.Add("Missing texture importer during validation: " + assetPath);
                    continue;
                }

                var expectedType = IsNormalMap(assetPath) ? TextureImporterType.NormalMap : TextureImporterType.Default;
                var expectedSrgb = IsColorTexture(assetPath);
                var expectedMax = TextureMaxSize(assetPath);
                if (importer.textureType != expectedType ||
                    importer.sRGBTexture != expectedSrgb ||
                    !importer.mipmapEnabled ||
                    importer.maxTextureSize != expectedMax ||
                    importer.textureCompression != TextureImporterCompression.CompressedHQ)
                {
                    report.Failures.Add("Texture importer does not match MYB-50 policy: " + assetPath);
                }
            }

            foreach (var assetPath in FindArtAssets(".fbx"))
            {
                var importer = AssetImporter.GetAtPath(assetPath) as ModelImporter;
                if (importer == null)
                {
                    report.Failures.Add("Missing model importer during validation: " + assetPath);
                    continue;
                }

                var expectedAnimation = IsAnimatedModel(assetPath);
                if (importer.importAnimation != expectedAnimation ||
                    importer.materialImportMode != ModelImporterMaterialImportMode.None ||
                    importer.importCameras ||
                    importer.importLights ||
                    importer.importVisibility ||
                    importer.importNormals != ModelImporterNormals.Import ||
                    importer.importTangents != ModelImporterTangents.CalculateMikk ||
                    importer.isReadable ||
                    !importer.useFileUnits)
                {
                    report.Failures.Add("Model importer does not match MYB-50 policy: " + assetPath);
                }
            }
        }

        private static void ValidateCanonicalScene(ValidationReport report)
        {
            EditorSceneManager.OpenScene(CanonicalScenePath, OpenSceneMode.Single);
            var root = GameObject.Find("MYB100_OptimizedImportedAssets");
            if (root == null)
            {
                report.Failures.Add("Missing MYB-100 optimized asset root in canonical scene.");
                return;
            }

            var placed = root.GetComponentsInChildren<Renderer>(true);
            if (SceneAssets.Any(spec => GameObject.Find(spec.Name) == null))
            {
                report.Failures.Add("One or more MYB-100 canonical scene assets are missing.");
            }

            var premiumSignalAnchor = GameObject.Find("MYB44_PremiumSignal_RelicLantern");
            if (premiumSignalAnchor == null)
            {
                report.Failures.Add("Missing canonical premium signal replacement anchor.");
            }
            else if (premiumSignalAnchor.transform.parent != root.transform)
            {
                report.Failures.Add("Primitive premium signal placeholder was not replaced by the MYB-100 anchor.");
            }

            if (placed.Length < 10)
            {
                report.Failures.Add("MYB-100 canonical scene has too few optimized asset renderers: " + placed.Length);
            }

            if (!File.Exists(Path.Combine(GetRepoRoot(), CapturePath)))
            {
                report.Failures.Add("Missing MYB-100 canonical PNG proof: " + CapturePath);
            }
        }

        private static void NormalizeSerializedWhitespace(ValidationReport report)
        {
            var normalizedFiles = new List<string>
            {
                CanonicalScenePath,
                MYB95ManifestPath
            };
            normalizedFiles.AddRange(FindArtAssets(".fbx").Select(path => path + ".meta"));
            normalizedFiles.AddRange(FindArtAssets(".mat")
                .Where(path =>
                    path.Contains("/MYB95", StringComparison.OrdinalIgnoreCase) ||
                    path.Contains("/MYB96", StringComparison.OrdinalIgnoreCase)));

            foreach (var assetPath in normalizedFiles)
            {
                var absolutePath = ProjectRelativeToAbsolute(assetPath);
                if (!File.Exists(absolutePath))
                {
                    continue;
                }

                var original = File.ReadAllText(absolutePath);
                var normalized = string.Join("\n", File.ReadAllLines(absolutePath).Select(line => line.TrimEnd(' ', '\t'))) + "\n";
                if (!string.Equals(original, normalized, StringComparison.Ordinal))
                {
                    File.WriteAllText(absolutePath, normalized);
                    report.SceneNotes.Add("Normalized serialized whitespace: " + assetPath);
                }
            }
        }

        private static IEnumerable<string> FindArtAssets(params string[] extensions)
        {
            var extensionSet = new HashSet<string>(extensions, StringComparer.OrdinalIgnoreCase);
            return AssetDatabase.FindAssets(string.Empty, new[] { ArtRoot })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(path => extensionSet.Contains(Path.GetExtension(path)))
                .OrderBy(path => path, StringComparer.Ordinal);
        }

        private static bool IsNormalMap(string assetPath)
        {
            return ContainsAny(assetPath, "normal", "normalgl", "_nor_", "_nor.");
        }

        private static bool IsColorTexture(string assetPath)
        {
            if (IsNormalMap(assetPath))
            {
                return false;
            }

            if (ContainsAny(assetPath, "roughness", "metallic", "occlusion", "_ao_", "_rough_", "_metal_"))
            {
                return false;
            }

            return true;
        }

        private static int TextureMaxSize(string assetPath)
        {
            return assetPath.Contains("/MYB95", StringComparison.OrdinalIgnoreCase) ? 2048 : 1024;
        }

        private static bool IsAnimatedModel(string assetPath)
        {
            return assetPath.Contains("HorseAnimated", StringComparison.OrdinalIgnoreCase) ||
                assetPath.Contains("MYB95MeshyCharacter", StringComparison.OrdinalIgnoreCase);
        }

        private static bool ContainsAny(string text, params string[] values)
        {
            return values.Any(value => text.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        private static string TextureSummary(TextureImporterType type, bool srgb, int maxSize)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}, sRGB={1}, max={2}, mipmaps, CompressedHQ", type, srgb, maxSize);
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

        private static void RenderCameraToPng(Camera camera, int width, int height, string path)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path) ?? GetRepoRoot());
            var previousTarget = camera.targetTexture;
            var renderTexture = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
            var texture = new Texture2D(width, height, TextureFormat.RGB24, false);
            try
            {
                camera.targetTexture = renderTexture;
                camera.Render();
                RenderTexture.active = renderTexture;
                texture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                texture.Apply();
                File.WriteAllBytes(path, texture.EncodeToPNG());
            }
            finally
            {
                camera.targetTexture = previousTarget;
                RenderTexture.active = null;
                renderTexture.Release();
                UnityEngine.Object.DestroyImmediate(renderTexture);
                UnityEngine.Object.DestroyImmediate(texture);
            }
        }

        private static void RequireAssetFile(string assetPath, ValidationReport report)
        {
            if (!File.Exists(ProjectRelativeToAbsolute(assetPath)))
            {
                report.Failures.Add("Missing required manifest: " + assetPath);
            }
        }

        private static void WriteReport(ValidationReport report)
        {
            var lines = new List<string>
            {
                "# MYB-100 Imported Asset Adjustments",
                "",
                "Status: " + report.Status,
                "Generated at: " + DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture),
                "Scope: apply MYB-50 import policy to existing MYB-42, MYB-53, MYB-95 and MYB-96 Unity assets.",
                "Canonical scene: " + CanonicalScenePath,
                "PNG proof: " + CapturePath,
                ""
            };

            AddSection(lines, "Texture adjustments", report.TextureAdjustments);
            AddSection(lines, "Texture settings already valid", report.TextureValidated);
            AddSection(lines, "Model adjustments", report.ModelAdjustments);
            AddSection(lines, "Model settings already valid", report.ModelValidated);
            AddSection(lines, "Generated asset manifests", report.ManifestNotes);
            AddSection(lines, "Canonical scene placements", report.ScenePlacements);
            AddSection(lines, "Scene notes", report.SceneNotes);
            AddSection(lines, "Failures", report.Failures);

            var absolutePath = Path.Combine(GetRepoRoot(), ReportPath);
            Directory.CreateDirectory(Path.GetDirectoryName(absolutePath) ?? GetRepoRoot());
            File.WriteAllLines(absolutePath, lines);
            Debug.Log("MYB-100 imported asset adjustment report: " + absolutePath);
        }

        private static void AddSection(ICollection<string> lines, string title, IReadOnlyCollection<string> values)
        {
            lines.Add("## " + title);
            if (values.Count == 0)
            {
                lines.Add("- none");
            }
            else
            {
                foreach (var value in values)
                {
                    lines.Add("- " + value);
                }
            }

            lines.Add("");
        }

        private static string ProjectRelativeToAbsolute(string assetPath)
        {
            var projectRoot = Directory.GetParent(Application.dataPath)?.FullName ?? Directory.GetCurrentDirectory();
            return Path.Combine(projectRoot, assetPath);
        }

        private static string GetRepoRoot()
        {
            var unityProjectRoot = Directory.GetParent(Application.dataPath)?.FullName ?? Directory.GetCurrentDirectory();
            return Directory.GetParent(Directory.GetParent(unityProjectRoot)?.FullName ?? unityProjectRoot)?.FullName ?? unityProjectRoot;
        }

        private sealed class SceneAssetSpec
        {
            public SceneAssetSpec(string assetPath, string name, float distanceMeters, float side, float lateralOffsetMeters, float targetHeightMeters)
            {
                AssetPath = assetPath;
                Name = name;
                DistanceMeters = distanceMeters;
                Side = side;
                LateralOffsetMeters = lateralOffsetMeters;
                TargetHeightMeters = targetHeightMeters;
            }

            public string AssetPath { get; }
            public string Name { get; }
            public float DistanceMeters { get; }
            public float Side { get; }
            public float LateralOffsetMeters { get; }
            public float TargetHeightMeters { get; }
        }

        private sealed class ValidationReport
        {
            public string Status { get; set; } = "fail";
            public List<string> TextureAdjustments { get; } = new List<string>();
            public List<string> TextureValidated { get; } = new List<string>();
            public List<string> ModelAdjustments { get; } = new List<string>();
            public List<string> ModelValidated { get; } = new List<string>();
            public List<string> ManifestNotes { get; } = new List<string>();
            public List<string> ScenePlacements { get; } = new List<string>();
            public List<string> SceneNotes { get; } = new List<string>();
            public List<string> Failures { get; } = new List<string>();
        }
    }
}

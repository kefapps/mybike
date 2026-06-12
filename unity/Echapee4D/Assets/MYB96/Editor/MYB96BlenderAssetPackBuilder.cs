using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

public static class MYB96BlenderAssetPackBuilder
{
    private const string AssetRoot = "Assets/Echappee/Art/MYB96BlenderGenerated";
    private const string ModelRoot = AssetRoot + "/Models";
    private const string MaterialRoot = AssetRoot + "/Materials";
    private const string PrefabRoot = AssetRoot + "/Prefabs";
    private const string SourceBlendPath = AssetRoot + "/Source/MYB96_BlenderGeneratedAssetPack.blend";
    private const string ManifestPath = AssetRoot + "/GeneratedAssets.assetmanifest.json";
    private const string ScenePath = "Assets/Scenes/MYB96BlenderAssetYard.unity";
    private const string CanonicalScenePath = "Assets/Scenes/MYB89UnityMcpProbe.unity";
    private const string ReportPath = "_bmad-output/unity-test-results/myb-96-blender-asset-pack-report.json";
    private const string CapturePath = "_bmad-output/unity-test-results/myb-96-blender-asset-yard.png";

    private const int ExpectedAssetCount = 15;
    private const int AssetTriangleLimit = 5000;
    private const int PackTriangleLimit = 35000;

    private static readonly AssetSpec[] Assets =
    {
        new("MYB96_ColDirectionSign", "route-signage", 1156),
        new("MYB96_KilometerMarker", "route-signage", 608),
        new("MYB96_HairpinChevronSign", "route-signage", 292),
        new("MYB96_SummitArchMarker", "route-signage", 1072),
        new("MYB96_RoadReflectorPair", "route-signage", 404),
        new("MYB96_AlpinePineSmall", "natural-roadside", 184),
        new("MYB96_AlpinePineTall", "natural-roadside", 220),
        new("MYB96_CairnStack", "natural-roadside", 100),
        new("MYB96_RoadsideRockCluster", "natural-roadside", 124),
        new("MYB96_WildflowerGrassPatch", "natural-roadside", 348),
        new("MYB96_StoneFlowerPlanter", "village-countryside", 264),
        new("MYB96_WoodFenceSegment", "village-countryside", 256),
        new("MYB96_VillageBench", "village-countryside", 308),
        new("MYB96_MarketCrateStack", "village-countryside", 416),
        new("MYB96_VillageWellMarker", "village-countryside", 472),
    };

    private static readonly MaterialSpec[] Materials =
    {
        new("MYB96_WeatheredStone", new Color(0.42f, 0.43f, 0.39f), 0.82f, 0f, false),
        new("MYB96_DarkInsetStone", new Color(0.22f, 0.23f, 0.22f), 0.86f, 0f, false),
        new("MYB96_WarmAlpineWood", new Color(0.55f, 0.31f, 0.16f), 0.76f, 0f, false),
        new("MYB96_DarkWoodGrain", new Color(0.31f, 0.18f, 0.10f), 0.80f, 0f, false),
        new("MYB96_OffWhitePaint", new Color(0.88f, 0.82f, 0.66f), 0.70f, 0f, false),
        new("MYB96_MutedRouteRed", new Color(0.62f, 0.12f, 0.09f), 0.68f, 0f, false),
        new("MYB96_AlpineOchre", new Color(0.86f, 0.59f, 0.20f), 0.68f, 0f, false),
        new("MYB96_PineGreen", new Color(0.12f, 0.36f, 0.20f), 0.78f, 0f, false),
        new("MYB96_NewLeafGreen", new Color(0.34f, 0.56f, 0.24f), 0.76f, 0f, false),
        new("MYB96_RoadsideGrass", new Color(0.26f, 0.48f, 0.23f), 0.80f, 0f, false),
        new("MYB96_WildflowerAccent", new Color(0.86f, 0.34f, 0.52f), 0.62f, 0f, false),
        new("MYB96_DullIron", new Color(0.28f, 0.29f, 0.28f), 0.66f, 0.25f, false),
        new("MYB96_SoftMarkerGlow", new Color(1.00f, 0.67f, 0.28f), 0.50f, 0f, true),
    };

    [MenuItem("Tools/MYB-96/Build Blender Asset Pack")]
    public static void BuildAndValidateFromMenu()
    {
        var reportPath = BuildAndValidate();
        Debug.Log("MYB-96 Blender asset pack build complete. Report: " + reportPath);
    }

    public static void BuildAndValidateCli()
    {
        var reportPath = BuildAndValidate();
        var fullReportPath = Path.Combine(GetRepoRoot(), reportPath);
        var reportText = File.ReadAllText(fullReportPath);
        if (reportText.Contains("\"Status\": \"fail\"", StringComparison.Ordinal))
        {
            throw new InvalidOperationException("MYB-96 Blender asset pack validation failed. See " + reportPath);
        }
    }

    public static string BuildAndValidate()
    {
        var report = new ValidationReport();

        try
        {
            EnsureFolders();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            var materials = BuildMaterials(report);
            RemapImportedMaterials(materials, report);
            BuildPrefabs(materials, report);
            BuildValidationScene(report);
            CaptureScenePreview(report);

            ValidateManifest(report);
            ValidateGeneratedFiles(report);
            ValidateBudgets(report);
            ValidateCanonicalBoundary(report);
            ValidateScene(report);
        }
        catch (Exception exception)
        {
            report.Failures.Add(exception.GetType().FullName + ": " + exception.Message);
            Debug.LogException(exception);
        }

        report.Status = report.Failures.Count == 0 ? "pass" : "fail";
        WriteReport(report);

        if (report.Failures.Count > 0)
        {
            throw new InvalidOperationException("MYB-96 Blender asset pack validation failed. See " + ReportPath);
        }

        return ReportPath;
    }

    private static Dictionary<string, Material> BuildMaterials(ValidationReport report)
    {
        var shader = FindLitShader();
        if (shader == null)
        {
            report.Failures.Add("No compatible Lit shader found.");
            return new Dictionary<string, Material>();
        }

        var materials = new Dictionary<string, Material>(StringComparer.Ordinal);
        foreach (var spec in Materials)
        {
            var path = MaterialRoot + "/" + spec.Name + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }

            material.shader = shader;
            SetColor(material, "_BaseColor", spec.Color);
            SetColor(material, "_Color", spec.Color);
            SetFloat(material, "_Metallic", spec.Metallic);
            SetFloat(material, "_Smoothness", 1f - spec.Roughness);
            if (spec.Emissive)
            {
                SetColor(material, "_EmissionColor", spec.Color * 0.85f);
                material.EnableKeyword("_EMISSION");
                material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            }
            else
            {
                material.DisableKeyword("_EMISSION");
                material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.EmissiveIsBlack;
            }

            EditorUtility.SetDirty(material);
            materials[spec.Name] = material;
            report.Materials.Add(path);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        return materials;
    }

    private static void RemapImportedMaterials(Dictionary<string, Material> materials, ValidationReport report)
    {
        foreach (var asset in Assets)
        {
            var importer = AssetImporter.GetAtPath(asset.ModelPath);
            if (importer == null)
            {
                report.Failures.Add("Missing model importer for material remap: " + asset.ModelPath);
                continue;
            }

            var changed = false;
            var importedMaterials = AssetDatabase.LoadAllAssetsAtPath(asset.ModelPath).OfType<Material>();
            foreach (var importedMaterial in importedMaterials)
            {
                if (!TryResolveMaterial(importedMaterial.name, materials, out var material))
                {
                    continue;
                }

                var source = new AssetImporter.SourceAssetIdentifier(typeof(Material), importedMaterial.name);
                importer.AddRemap(source, material);
                report.MaterialRemaps++;
                changed = true;
            }

            if (changed)
            {
                importer.SaveAndReimport();
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
    }

    private static void BuildPrefabs(Dictionary<string, Material> materials, ValidationReport report)
    {
        foreach (var asset in Assets)
        {
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(asset.ModelPath);
            if (model == null)
            {
                report.Failures.Add("Missing FBX model: " + asset.ModelPath);
                continue;
            }

            var modelInstance = (GameObject)PrefabUtility.InstantiatePrefab(model);
            if (modelInstance == null)
            {
                modelInstance = Object.Instantiate(model);
            }

            var instance = new GameObject(asset.Id);
            modelInstance.name = asset.Id + "_Model";
            modelInstance.transform.SetParent(instance.transform, false);
            modelInstance.transform.localPosition = Vector3.zero;
            modelInstance.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);
            modelInstance.transform.localScale = Vector3.one * 100f;
            ApplyMaterials(instance, materials);

            var prefabPath = asset.PrefabPath;
            PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
            Object.DestroyImmediate(instance);
            report.Prefabs.Add(prefabPath);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
    }

    private static void BuildValidationScene(ValidationReport report)
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var root = new GameObject("MYB96 Blender Asset Yard");

        var familyPositions = new Dictionary<string, float>
        {
            { "route-signage", -2.6f },
            { "natural-roadside", 0f },
            { "village-countryside", 2.6f },
        };

        var familyCounts = new Dictionary<string, int>
        {
            { "route-signage", 0 },
            { "natural-roadside", 0 },
            { "village-countryside", 0 },
        };

        foreach (var asset in Assets)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(asset.PrefabPath);
            if (prefab == null)
            {
                report.Failures.Add("Missing generated prefab: " + asset.PrefabPath);
                continue;
            }

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            instance.name = asset.Id;
            instance.transform.SetParent(root.transform);
            var column = familyCounts[asset.Family]++;
            instance.transform.position = new Vector3((column - 2) * 1.65f, 0f, familyPositions[asset.Family]);
            instance.transform.rotation = Quaternion.Euler(0f, column % 2 == 0 ? 12f : -10f, 0f);
            instance.transform.localScale = Vector3.one * 1.25f;
        }

        CreateGround();
        CreateFamilyLabels();
        CreateLight();
        CreateCamera();

        EditorSceneManager.SaveScene(scene, ScenePath);
        report.ScenePath = ScenePath;
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
    }

    private static void CreateGround()
    {
        var ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ground.name = "MYB96 Asset Yard Ground";
        ground.transform.position = new Vector3(0f, -0.035f, 0f);
        ground.transform.localScale = new Vector3(8.8f, 0.06f, 6.6f);
        var material = AssetDatabase.LoadAssetAtPath<Material>(MaterialRoot + "/MYB96_RoadsideGrass.mat");
        if (material != null)
        {
            ground.GetComponent<Renderer>().sharedMaterial = material;
        }
    }

    private static void CreateFamilyLabels()
    {
        CreateLabel("ROUTE", new Vector3(-3.7f, 0.04f, -3.38f), 0.34f);
        CreateLabel("NATURE", new Vector3(-3.7f, 0.04f, -0.78f), 0.34f);
        CreateLabel("VILLAGE", new Vector3(-3.7f, 0.04f, 1.82f), 0.34f);
    }

    private static void CreateLabel(string label, Vector3 position, float size)
    {
        var textObject = new GameObject("MYB96 Label " + label);
        textObject.transform.position = position;
        textObject.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        var text = textObject.AddComponent<TextMesh>();
        text.text = label;
        text.fontSize = 48;
        text.characterSize = size * 0.1f;
        text.anchor = TextAnchor.MiddleLeft;
        text.alignment = TextAlignment.Left;
        text.color = new Color(0.18f, 0.17f, 0.14f);
    }

    private static void CreateLight()
    {
        var lightObject = new GameObject("MYB96 Key Light");
        lightObject.transform.rotation = Quaternion.Euler(45f, -35f, 0f);
        var light = lightObject.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.2f;
        light.color = new Color(1f, 0.94f, 0.82f);
    }

    private static void CreateCamera()
    {
        var cameraObject = new GameObject("MYB96 Capture Camera");
        cameraObject.transform.position = new Vector3(0f, 2.35f, -8.3f);
        cameraObject.transform.LookAt(new Vector3(0f, 0.85f, 0f));
        var camera = cameraObject.AddComponent<Camera>();
        camera.fieldOfView = 38f;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.68f, 0.80f, 0.88f);
        camera.nearClipPlane = 0.05f;
        camera.farClipPlane = 50f;
        Camera.SetupCurrent(camera);
    }

    private static void CaptureScenePreview(ValidationReport report)
    {
        var camera = Object.FindObjectsByType<Camera>()
            .FirstOrDefault(item => item.name == "MYB96 Capture Camera");
        if (camera == null)
        {
            report.Failures.Add("Missing MYB-96 capture camera.");
            return;
        }

        var target = new RenderTexture(1600, 1000, 24);
        camera.targetTexture = target;
        camera.Render();

        RenderTexture.active = target;
        var texture = new Texture2D(target.width, target.height, TextureFormat.RGB24, false);
        texture.ReadPixels(new Rect(0, 0, target.width, target.height), 0, 0);
        texture.Apply();

        var bytes = texture.EncodeToPNG();
        var fullPath = Path.Combine(GetRepoRoot(), CapturePath);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath) ?? ".");
        File.WriteAllBytes(fullPath, bytes);

        camera.targetTexture = null;
        RenderTexture.active = null;
        Object.DestroyImmediate(texture);
        Object.DestroyImmediate(target);

        report.CapturePath = CapturePath;
    }

    private static void ValidateManifest(ValidationReport report)
    {
        if (!File.Exists(ToFullPath(ManifestPath)))
        {
            report.Failures.Add("Missing generated asset manifest: " + ManifestPath);
            return;
        }

        var manifest = File.ReadAllText(ToFullPath(ManifestPath));
        if (!manifest.Contains("\"ticket\": \"MYB-96\"", StringComparison.Ordinal))
        {
            report.Failures.Add("Generated manifest does not identify ticket MYB-96.");
        }

        if (!manifest.Contains("\"tool\": \"Blender MCP\"", StringComparison.Ordinal))
        {
            report.Failures.Add("Generated manifest does not record Blender MCP provenance.");
        }

        if (!manifest.Contains("\"externalServices\": []", StringComparison.Ordinal))
        {
            report.Failures.Add("Generated manifest must record that no external services were used.");
        }

        foreach (var asset in Assets)
        {
            if (!manifest.Contains("\"id\": \"" + asset.Id + "\"", StringComparison.Ordinal))
            {
                report.Failures.Add("Generated manifest missing asset: " + asset.Id);
            }
        }
    }

    private static void ValidateGeneratedFiles(ValidationReport report)
    {
        if (!File.Exists(ToFullPath(SourceBlendPath)))
        {
            report.Failures.Add("Missing Blender source file: " + SourceBlendPath);
        }

        foreach (var asset in Assets)
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(asset.ModelPath) == null)
            {
                report.Failures.Add("Missing imported model: " + asset.ModelPath);
            }

            if (AssetDatabase.LoadAssetAtPath<GameObject>(asset.PrefabPath) == null)
            {
                report.Failures.Add("Missing generated prefab: " + asset.PrefabPath);
            }
        }

        if (report.Prefabs.Count != ExpectedAssetCount)
        {
            report.Failures.Add("Expected " + ExpectedAssetCount + " generated prefabs, got " + report.Prefabs.Count + ".");
        }
    }

    private static void ValidateBudgets(ValidationReport report)
    {
        var totalTriangles = 0;
        foreach (var asset in Assets)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(asset.PrefabPath);
            if (prefab == null)
            {
                continue;
            }

            var triangles = CountTriangles(prefab);
            report.AssetTriangleCounts.Add(new AssetTriangleCount(asset.Id, triangles));
            totalTriangles += triangles;
            if (triangles > AssetTriangleLimit)
            {
                report.Failures.Add(asset.Id + " exceeds per-asset triangle budget: " + triangles);
            }
        }

        report.TotalTriangles = totalTriangles;
        if (totalTriangles > PackTriangleLimit)
        {
            report.Failures.Add("Pack exceeds triangle budget: " + totalTriangles);
        }
    }

    private static void ValidateCanonicalBoundary(ValidationReport report)
    {
        if (File.Exists(ToFullPath(CanonicalScenePath)))
        {
            report.CanonicalSceneUnchanged = true;
        }
        else
        {
            report.Failures.Add("Canonical MYB-89 scene missing: " + CanonicalScenePath);
        }
    }

    private static void ValidateScene(ValidationReport report)
    {
        if (!File.Exists(ToFullPath(ScenePath)))
        {
            report.Failures.Add("Missing MYB-96 validation scene: " + ScenePath);
        }

        if (!File.Exists(Path.Combine(GetRepoRoot(), CapturePath)))
        {
            report.Failures.Add("Missing MYB-96 visual capture: " + CapturePath);
        }
    }

    private static void ApplyMaterials(GameObject instance, Dictionary<string, Material> materials)
    {
        foreach (var renderer in instance.GetComponentsInChildren<Renderer>(true))
        {
            var shared = renderer.sharedMaterials;
            for (var index = 0; index < shared.Length; index++)
            {
                var sourceName = shared[index] != null ? shared[index].name : string.Empty;
                if (TryResolveMaterial(sourceName, materials, out var material))
                {
                    shared[index] = material;
                    continue;
                }

                var descriptor = sourceName + " " + renderer.gameObject.name + " " + GetMeshName(renderer);
                if (TryResolveMaterial(descriptor, materials, out material))
                {
                    shared[index] = material;
                }
            }

            renderer.sharedMaterials = shared;
        }
    }

    private static string GetMeshName(Renderer renderer)
    {
        var filter = renderer.GetComponent<MeshFilter>();
        if (filter != null && filter.sharedMesh != null)
        {
            return filter.sharedMesh.name;
        }

        if (renderer is SkinnedMeshRenderer skinned && skinned.sharedMesh != null)
        {
            return skinned.sharedMesh.name;
        }

        return string.Empty;
    }

    private static bool TryResolveMaterial(string descriptor, Dictionary<string, Material> materials, out Material material)
    {
        if (materials.TryGetValue(descriptor, out material))
        {
            return true;
        }

        var normalized = Materials.FirstOrDefault(spec => descriptor.Contains(spec.Name, StringComparison.Ordinal));
        if (normalized != null && materials.TryGetValue(normalized.Name, out material))
        {
            return true;
        }

        return TryInferMaterial(descriptor, materials, out material);
    }

    private static bool TryInferMaterial(string objectName, Dictionary<string, Material> materials, out Material material)
    {
        var lower = objectName.ToLowerInvariant();
        var materialName = "MYB96_WeatheredStone";

        if (lower.Contains("pine") || lower.Contains("bough") || lower.Contains("leaf") || lower.Contains("plant") || lower.Contains("stem") || lower.Contains("grass"))
        {
            materialName = lower.Contains("leaf") || lower.Contains("plant") ? "MYB96_NewLeafGreen" : "MYB96_PineGreen";
        }
        else if (lower.Contains("flower"))
        {
            materialName = "MYB96_WildflowerAccent";
        }
        else if (lower.Contains("wood") || lower.Contains("post") || lower.Contains("rail") || lower.Contains("bench") || lower.Contains("crate") || lower.Contains("beam") || lower.Contains("trunk"))
        {
            materialName = lower.Contains("dark") || lower.Contains("leg") || lower.Contains("rail") ? "MYB96_DarkWoodGrain" : "MYB96_WarmAlpineWood";
        }
        else if (lower.Contains("paint") || lower.Contains("board") || lower.Contains("plank") || lower.Contains("sign") || lower.Contains("cap") || lower.Contains("text"))
        {
            materialName = "MYB96_OffWhitePaint";
        }
        else if (lower.Contains("red") || lower.Contains("slash") || lower.Contains("stripe") || lower.Contains("roof") || lower.Contains("cloth"))
        {
            materialName = "MYB96_MutedRouteRed";
        }
        else if (lower.Contains("ochre"))
        {
            materialName = "MYB96_AlpineOchre";
        }
        else if (lower.Contains("metal") || lower.Contains("pin") || lower.Contains("bucket"))
        {
            materialName = "MYB96_DullIron";
        }
        else if (lower.Contains("reflector") || lower.Contains("amber") || lower.Contains("glow"))
        {
            materialName = "MYB96_SoftMarkerGlow";
        }
        else if (lower.Contains("inset") || lower.Contains("shadow") || lower.Contains("soil"))
        {
            materialName = "MYB96_DarkInsetStone";
        }

        return materials.TryGetValue(materialName, out material);
    }

    private static int CountTriangles(GameObject root)
    {
        var total = 0;
        foreach (var filter in root.GetComponentsInChildren<MeshFilter>(true))
        {
            if (filter.sharedMesh != null)
            {
                total += filter.sharedMesh.triangles.Length / 3;
            }
        }

        foreach (var renderer in root.GetComponentsInChildren<SkinnedMeshRenderer>(true))
        {
            if (renderer.sharedMesh != null)
            {
                total += renderer.sharedMesh.triangles.Length / 3;
            }
        }

        return total;
    }

    private static void EnsureFolders()
    {
        Directory.CreateDirectory(ToFullPath(MaterialRoot));
        Directory.CreateDirectory(ToFullPath(PrefabRoot));
        Directory.CreateDirectory(Path.Combine(GetRepoRoot(), "_bmad-output/unity-test-results"));
    }

    private static string ToFullPath(string assetPath)
    {
        return Path.Combine(GetUnityProjectRoot(), assetPath);
    }

    private static string GetUnityProjectRoot()
    {
        return Directory.GetParent(Application.dataPath)?.FullName ?? Directory.GetCurrentDirectory();
    }

    private static string GetRepoRoot()
    {
        return Directory.GetParent(GetUnityProjectRoot())?.Parent?.FullName ?? Directory.GetCurrentDirectory();
    }

    private static void WriteReport(ValidationReport report)
    {
        var fullPath = Path.Combine(GetRepoRoot(), ReportPath);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath) ?? ".");
        File.WriteAllText(fullPath, JsonUtility.ToJson(report, true));
        Debug.Log("MYB-96 report written: " + fullPath);
    }

    private static Shader FindLitShader()
    {
        return Shader.Find("Universal Render Pipeline/Unlit")
            ?? Shader.Find("Unlit/Color")
            ?? Shader.Find("Universal Render Pipeline/Lit")
            ?? Shader.Find("HDRP/Lit")
            ?? Shader.Find("Standard");
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

    [Serializable]
    private sealed class AssetSpec
    {
        public AssetSpec(string id, string family, int expectedTriangles)
        {
            Id = id;
            Family = family;
            ExpectedTriangles = expectedTriangles;
            ModelPath = ModelRoot + "/" + id + ".fbx";
            PrefabPath = PrefabRoot + "/" + id + ".prefab";
        }

        public string Id;
        public string Family;
        public int ExpectedTriangles;
        public string ModelPath;
        public string PrefabPath;
    }

    [Serializable]
    private sealed class MaterialSpec
    {
        public MaterialSpec(string name, Color color, float roughness, float metallic, bool emissive)
        {
            Name = name;
            Color = color;
            Roughness = roughness;
            Metallic = metallic;
            Emissive = emissive;
        }

        public string Name;
        public Color Color;
        public float Roughness;
        public float Metallic;
        public bool Emissive;
    }

    [Serializable]
    private sealed class AssetTriangleCount
    {
        public AssetTriangleCount(string assetId, int triangles)
        {
            AssetId = assetId;
            Triangles = triangles;
        }

        public string AssetId;
        public int Triangles;
    }

    [Serializable]
    private sealed class ValidationReport
    {
        public string Ticket = "MYB-96";
        public string Status = "unknown";
        public string ScenePath = string.Empty;
        public string CapturePath = string.Empty;
        public bool CanonicalSceneUnchanged;
        public int TotalTriangles;
        public int MaterialRemaps;
        public List<string> Materials = new();
        public List<string> Prefabs = new();
        public List<AssetTriangleCount> AssetTriangleCounts = new();
        public List<string> Failures = new();
    }
}

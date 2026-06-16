using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public static class MYB144ArtAssetValidator
{
    private const string MenuPath = "Tools/MyBike/Validation/MYB-144 Art Asset Validator";
    private const string ManifestRelativePath = "docs/manifests/art-rescue-asset-manifest.json";
    private const string SchemaRelativePath = "docs/schemas/third-party-asset-manifest.md";
    private const string ReportRelativePath = "_bmad-output/unity-test-results/myb-144-art-asset-validator-report.md";
    private const int SupportedSchemaVersion = 1;
    private const int TextureWarningDimension = 2048;
    private const int ComplexMeshColliderTriangleThreshold = 500;
    private const int MaterialCountWarningThreshold = 4;

    private static readonly string[] ScanRoots =
    {
        "Assets/Echappee/Art",
        "Assets/Echappee/ArtRescue"
    };

    private static readonly string[] IgnoredDirectoryNames =
    {
        "Editor",
        "Tests",
        "Test",
        "Validation",
        "Reports",
        "Docs",
        "Documentation"
    };

    private static readonly HashSet<string> CandidateExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        ".fbx",
        ".glb",
        ".gltf",
        ".obj",
        ".prefab",
        ".mat",
        ".asset",
        ".png",
        ".jpg",
        ".jpeg",
        ".tga",
        ".exr",
        ".psd"
    };

    private static readonly HashSet<string> IgnoredExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        ".meta",
        ".cs",
        ".asmdef",
        ".unity",
        ".md",
        ".txt",
        ".json"
    };

    private static readonly HashSet<string> AllowedSourceTypes = new HashSet<string>(StringComparer.Ordinal)
    {
        "third_party",
        "ai_generated",
        "ai_assisted",
        "internal",
        "blender_mcp",
        "in_house_authored",
        "unity_builtin_or_procedural",
        "derived",
        "unknown"
    };

    private static readonly HashSet<string> AllowedIntakeStatuses = new HashSet<string>(StringComparer.Ordinal)
    {
        "quarantine",
        "review",
        "approved",
        "rejected",
        "deprecated"
    };

    private static readonly HashSet<string> AllowedPromotionStatuses = new HashSet<string>(StringComparer.Ordinal)
    {
        "not_promoted",
        "candidate",
        "promoted"
    };

    private static readonly HashSet<string> AllowedUsageScopes = new HashSet<string>(StringComparer.Ordinal)
    {
        "forest_corridor",
        "art_rescue",
        "prototype_only",
        "editor_only",
        "reference_only",
        "quarantine_only",
        "global"
    };

    private static readonly HashSet<string> AllowedVisualImpacts = new HashSet<string>(StringComparer.Ordinal)
    {
        "visible",
        "technical",
        "none"
    };

    [MenuItem(MenuPath)]
    public static void RunFromMenu()
    {
        var result = RunValidation("Menu");
        var summary = result.ToConsoleSummary();

        if (result.ErrorCount > 0)
        {
            Debug.LogError(summary);
            return;
        }

        if (result.WarningCount > 0)
        {
            Debug.LogWarning(summary);
            return;
        }

        Debug.Log(summary);
    }

    public static void RunBatch()
    {
        var result = RunValidation("Batch");
        var summary = result.ToConsoleSummary();

        if (result.ErrorCount > 0)
        {
            Debug.LogError(summary);
            EditorApplication.Exit(1);
            return;
        }

        if (result.WarningCount > 0)
        {
            Debug.LogWarning(summary);
            EditorApplication.Exit(0);
            return;
        }

        Debug.Log(summary);
        EditorApplication.Exit(0);
    }

    public static ValidationResult RunValidation()
    {
        return RunValidation("Unknown");
    }

    public static ValidationResult RunValidation(string executionMode)
    {
        var result = new ValidationResult
        {
            ExecutionMode = string.IsNullOrWhiteSpace(executionMode) ? "Unknown" : executionMode,
            RepoRoot = GetRepoRoot()
        };

        result.ManifestPath = Path.Combine(result.RepoRoot, ManifestRelativePath);
        result.ReportPath = Path.Combine(result.RepoRoot, ReportRelativePath);

        try
        {
            var manifest = ValidateManifest(result);
            ScanUnityAssets(result, manifest);
        }
        catch (Exception exception)
        {
            result.AddError(
                "VALIDATOR_EXCEPTION",
                "Validator",
                exception.GetType().FullName + ": " + exception.Message,
                "Fix the validator exception before trusting the gate.");
            result.AddInfo("VALIDATOR_EXCEPTION_DETAIL", exception.ToString());
        }

        try
        {
            WriteReport(result);
        }
        catch (Exception exception)
        {
            result.AddError(
                "REPORT_WRITE_FAILED",
                ReportRelativePath,
                exception.GetType().FullName + ": " + exception.Message,
                "Ensure the report directory is writable.");
        }

        return result;
    }

    private static ManifestData ValidateManifest(ValidationResult result)
    {
        if (!File.Exists(result.ManifestPath))
        {
            result.AddError(
                "MANIFEST_MISSING",
                ManifestRelativePath,
                "Canonical Art Rescue asset manifest is missing.",
                "Create " + ManifestRelativePath + " as a versioned object.");
            return new ManifestData();
        }

        string json;
        try
        {
            json = File.ReadAllText(result.ManifestPath);
        }
        catch (Exception exception)
        {
            result.AddError(
                "MANIFEST_READ_FAILED",
                ManifestRelativePath,
                exception.Message,
                "Ensure the manifest can be read from the repository root.");
            return new ManifestData();
        }

        var trimmed = json.TrimStart();
        if (trimmed.StartsWith("[", StringComparison.Ordinal))
        {
            result.AddError(
                "MANIFEST_ROOT_NOT_OBJECT",
                ManifestRelativePath,
                "The manifest root is a list. MYB-143 requires a versioned object.",
                "Use { \"schemaVersion\": 1, \"updatedAt\": \"YYYY-MM-DD\", \"assets\": [] }.");
            return new ManifestData();
        }

        if (!trimmed.StartsWith("{", StringComparison.Ordinal))
        {
            result.AddError(
                "MANIFEST_INVALID_JSON",
                ManifestRelativePath,
                "The manifest does not start with a JSON object.",
                "Fix the manifest JSON syntax.");
            return new ManifestData();
        }

        if (Regex.IsMatch(json, "\"reviewStatus\"\\s*:", RegexOptions.CultureInvariant))
        {
            result.AddError(
                "ASSET_REVIEW_STATUS_FORBIDDEN",
                ManifestRelativePath,
                "`reviewStatus` is not a canonical MYB-143 field.",
                "Use `intakeStatus` and `promotionStatus` instead.");
        }

        if (Regex.IsMatch(json, "\"example\"\\s*:\\s*true", RegexOptions.CultureInvariant))
        {
            result.AddError(
                "ASSET_EXAMPLE_FORBIDDEN",
                ManifestRelativePath,
                "`example: true` is forbidden in the real production manifest.",
                "Move examples to documentation or fixtures.");
        }

        ManifestData manifest;
        try
        {
            manifest = JsonUtility.FromJson<ManifestData>(json);
        }
        catch (Exception exception)
        {
            result.AddError(
                "MANIFEST_INVALID_JSON",
                ManifestRelativePath,
                exception.Message,
                "Fix the manifest JSON syntax.");
            return new ManifestData();
        }

        if (manifest == null)
        {
            result.AddError(
                "MANIFEST_INVALID_JSON",
                ManifestRelativePath,
                "Unity JsonUtility returned null for the manifest.",
                "Fix the manifest JSON syntax.");
            return new ManifestData();
        }

        if (manifest.schemaVersion <= 0)
        {
            result.AddError(
                "MANIFEST_SCHEMA_VERSION_MISSING",
                ManifestRelativePath,
                "`schemaVersion` is missing or invalid.",
                "Set `schemaVersion` to 1.");
        }
        else if (manifest.schemaVersion != SupportedSchemaVersion)
        {
            result.AddError(
                "MANIFEST_SCHEMA_VERSION_UNSUPPORTED",
                ManifestRelativePath,
                "`schemaVersion` " + manifest.schemaVersion + " is not supported by MYB-144 V1.",
                "Use schemaVersion 1 or update the validator.");
        }

        if (string.IsNullOrWhiteSpace(manifest.updatedAt))
        {
            result.AddError(
                "MANIFEST_UPDATED_AT_MISSING",
                ManifestRelativePath,
                "`updatedAt` is missing.",
                "Set `updatedAt` to a YYYY-MM-DD date.");
        }
        else if (!DateTime.TryParseExact(
                     manifest.updatedAt,
                     "yyyy-MM-dd",
                     CultureInfo.InvariantCulture,
                     DateTimeStyles.None,
                     out _))
        {
            result.AddError(
                "MANIFEST_UPDATED_AT_INVALID",
                ManifestRelativePath,
                "`updatedAt` must use YYYY-MM-DD.",
                "Update the manifest date format.");
        }

        if (!Regex.IsMatch(json, "\"assets\"\\s*:", RegexOptions.CultureInvariant) ||
            !Regex.IsMatch(json, "\"assets\"\\s*:\\s*\\[", RegexOptions.CultureInvariant) ||
            manifest.assets == null)
        {
            result.AddError(
                "MANIFEST_ASSETS_NOT_ARRAY",
                ManifestRelativePath,
                "`assets` is missing or is not an array.",
                "Set `assets` to an array, empty if needed.");
            manifest.assets = Array.Empty<AssetEntry>();
        }

        result.ManifestSchemaVersion = manifest.schemaVersion;
        result.ManifestUpdatedAt = manifest.updatedAt ?? string.Empty;
        result.ManifestAssetCount = manifest.assets.Length;

        if (manifest.assets.Length == 0)
        {
            result.AddInfo(
                "MANIFEST_VALID_EMPTY",
                "The manifest is valid and currently contains no real asset entries.");
        }

        ValidateManifestAssets(result, manifest);
        return manifest;
    }

    private static void ValidateManifestAssets(ValidationResult result, ManifestData manifest)
    {
        var ids = new HashSet<string>(StringComparer.Ordinal);

        foreach (var asset in manifest.assets)
        {
            var assetId = string.IsNullOrWhiteSpace(asset.id) ? "(missing id)" : asset.id;

            if (string.IsNullOrWhiteSpace(asset.id))
            {
                result.AddError(
                    "ASSET_ID_MISSING",
                    assetId,
                    "Asset entry is missing `id`.",
                    "Add a stable unique machine-readable id.");
            }
            else if (!ids.Add(asset.id))
            {
                result.AddError(
                    "ASSET_ID_DUPLICATE",
                    asset.id,
                    "Duplicate asset id.",
                    "Make each manifest asset id unique.");
            }

            if (string.IsNullOrWhiteSpace(asset.sourceType) || !AllowedSourceTypes.Contains(asset.sourceType))
            {
                result.AddError(
                    "ASSET_SOURCE_TYPE_INVALID",
                    assetId,
                    "`sourceType` is missing or unsupported.",
                    "Use a sourceType documented in " + SchemaRelativePath + ".");
            }

            if (string.IsNullOrWhiteSpace(asset.usageScope) || !AllowedUsageScopes.Contains(asset.usageScope))
            {
                result.AddError(
                    "ASSET_USAGE_SCOPE_INVALID",
                    assetId,
                    "`usageScope` is missing or unsupported.",
                    "Use a usageScope documented in " + SchemaRelativePath + ".");
            }

            if (string.IsNullOrWhiteSpace(asset.visualImpact) || !AllowedVisualImpacts.Contains(asset.visualImpact))
            {
                result.AddError(
                    "ASSET_VISUAL_IMPACT_INVALID",
                    assetId,
                    "`visualImpact` is missing or unsupported.",
                    "Use visible, technical, or none.");
            }

            if (string.IsNullOrWhiteSpace(asset.intakeStatus) || !AllowedIntakeStatuses.Contains(asset.intakeStatus))
            {
                result.AddError(
                    "ASSET_INTAKE_STATUS_INVALID",
                    assetId,
                    "`intakeStatus` is missing or unsupported.",
                    "Use quarantine, review, approved, rejected, or deprecated.");
            }

            if (string.IsNullOrWhiteSpace(asset.promotionStatus) || !AllowedPromotionStatuses.Contains(asset.promotionStatus))
            {
                result.AddError(
                    "ASSET_PROMOTION_STATUS_INVALID",
                    assetId,
                    "`promotionStatus` is missing or unsupported.",
                    "Use not_promoted, candidate, or promoted.");
            }

            if (!IsAllowedStatusCombination(asset.intakeStatus, asset.promotionStatus))
            {
                result.AddError(
                    "ASSET_STATUS_COMBINATION_INVALID",
                    assetId,
                    "`" + asset.intakeStatus + "` cannot be combined with `" + asset.promotionStatus + "`.",
                    "Only approved intake can become candidate or promoted.");
            }

            if ((asset.promotionStatus == "candidate" || asset.promotionStatus == "promoted") && asset.intakeStatus != "approved")
            {
                result.AddError(
                    "ASSET_PROMOTED_WITHOUT_APPROVED_INTAKE",
                    assetId,
                    "`candidate` and `promoted` require `intakeStatus: approved`.",
                    "Fix the intake status or downgrade promotionStatus.");
            }

            if (asset.promotionStatus == "promoted")
            {
                ValidatePromotedManifestAsset(result, asset, assetId);
            }

            var statusKey = (string.IsNullOrWhiteSpace(asset.intakeStatus) ? "(missing)" : asset.intakeStatus) +
                " / " +
                (string.IsNullOrWhiteSpace(asset.promotionStatus) ? "(missing)" : asset.promotionStatus);
            result.IncrementStatus(statusKey);
        }
    }

    private static void ValidatePromotedManifestAsset(ValidationResult result, AssetEntry asset, string assetId)
    {
        if (asset.assetPaths == null || asset.assetPaths.Length == 0 || asset.assetPaths.All(string.IsNullOrWhiteSpace))
        {
            result.AddError(
                "ASSET_PROMOTED_PATH_MISSING",
                assetId,
                "Promoted asset has no `assetPaths`.",
                "List at least one Unity-relative Assets/... path.");
        }

        if (asset.sourceType == "unknown")
        {
            result.AddError(
                "ASSET_PROMOTED_SOURCE_UNKNOWN",
                assetId,
                "Promoted asset cannot use `sourceType: unknown`.",
                "Document source type before promotion.");
        }

        if (string.IsNullOrWhiteSpace(asset.license))
        {
            result.AddError(
                "ASSET_PROMOTED_LICENSE_MISSING",
                assetId,
                "Promoted asset has no license.",
                "Document license before production promotion.");
        }

        if (asset.aiGenerated && string.IsNullOrWhiteSpace(asset.notes))
        {
            result.AddError(
                "ASSET_AI_PROMOTED_WITHOUT_REVIEW",
                assetId,
                "Promoted AI asset needs explicit provenance/review notes.",
                "Document provider, provenance, terms, and promotion review evidence.");
        }

        if (asset.visualImpact == "visible")
        {
            if (asset.routeEvidence == null || asset.routeEvidence.Length == 0 || asset.routeEvidence.All(string.IsNullOrWhiteSpace))
            {
                result.AddError(
                    "ASSET_PROMOTED_ROUTE_EVIDENCE_MISSING",
                    assetId,
                    "Visible promoted asset has no route-camera evidence.",
                    "Add route-camera evidence or downgrade promotionStatus.");
            }

            if (asset.overviewEvidence == null || asset.overviewEvidence.Length == 0 || asset.overviewEvidence.All(string.IsNullOrWhiteSpace))
            {
                result.AddError(
                    "ASSET_PROMOTED_OVERVIEW_EVIDENCE_MISSING",
                    assetId,
                    "Visible promoted asset has no overview evidence.",
                    "Add overview evidence or downgrade promotionStatus.");
            }
        }

        foreach (var assetPath in asset.assetPaths ?? Array.Empty<string>())
        {
            if (string.IsNullOrWhiteSpace(assetPath))
            {
                continue;
            }

            if (!assetPath.StartsWith("Assets/", StringComparison.Ordinal))
            {
                result.AddError(
                    "ASSET_PROMOTED_PATH_INVALID",
                    assetId,
                    "`" + assetPath + "` is not a Unity-relative Assets/... path.",
                    "Use Unity-relative paths in the manifest.");
                continue;
            }

            var fullPath = Path.Combine(Application.dataPath, assetPath.Substring("Assets/".Length));
            if (!File.Exists(fullPath) && !Directory.Exists(fullPath))
            {
                result.AddError(
                    "ASSET_PROMOTED_PATH_NOT_FOUND",
                    assetId,
                    "Promoted asset path not found: `" + assetPath + "`.",
                    "Fix the manifest path or add the referenced asset.");
            }
        }
    }

    private static bool IsAllowedStatusCombination(string intakeStatus, string promotionStatus)
    {
        if (intakeStatus == "approved")
        {
            return promotionStatus == "not_promoted" ||
                promotionStatus == "candidate" ||
                promotionStatus == "promoted";
        }

        if (intakeStatus == "quarantine" ||
            intakeStatus == "review" ||
            intakeStatus == "rejected" ||
            intakeStatus == "deprecated")
        {
            return promotionStatus == "not_promoted";
        }

        return false;
    }

    private static void ScanUnityAssets(ValidationResult result, ManifestData manifest)
    {
        var manifestByPath = BuildManifestPathIndex(manifest);
        var scanRoots = GetExistingScanRoots(result);

        foreach (var root in scanRoots)
        {
            var fullRoot = Path.Combine(Application.dataPath, root.Substring("Assets/".Length));
            var candidatePaths = Directory
                .EnumerateFiles(fullRoot, "*", SearchOption.AllDirectories)
                .Select(ToUnityPath)
                .Where(path => !string.IsNullOrEmpty(path))
                .Where(path => !IsIgnoredByDirectory(path, manifestByPath))
                .Where(IsCandidateAssetPath)
                .OrderBy(path => path, StringComparer.Ordinal)
                .ToList();

            result.AddScanRoot(root, true, candidatePaths.Count, candidatePaths.Count == 0 ? "No V1 asset candidates found." : "Scanned.");

            if (candidatePaths.Count == 0)
            {
                result.AddInfo("UNITY_SCAN_ROOT_EMPTY", root + " exists but contains no V1 asset candidates.");
            }

            foreach (var assetPath in candidatePaths)
            {
                var entry = FindManifestEntryForPath(manifestByPath, assetPath);
                if (entry == null)
                {
                    AddUnmanifestedCandidate(result, assetPath);
                    ValidateUnityCandidate(result, assetPath, null);
                    continue;
                }

                ValidateUnityCandidate(result, assetPath, entry);
            }
        }
    }

    private static Dictionary<string, AssetEntry> BuildManifestPathIndex(ManifestData manifest)
    {
        var map = new Dictionary<string, AssetEntry>(StringComparer.OrdinalIgnoreCase);

        foreach (var asset in manifest.assets ?? Array.Empty<AssetEntry>())
        {
            foreach (var path in asset.assetPaths ?? Array.Empty<string>())
            {
                var normalizedPath = NormalizeUnityManifestPath(path);
                if (!string.IsNullOrWhiteSpace(normalizedPath) && !map.ContainsKey(normalizedPath))
                {
                    map.Add(normalizedPath, asset);
                }
            }
        }

        return map;
    }

    private static List<string> GetExistingScanRoots(ValidationResult result)
    {
        var roots = new List<string>();

        foreach (var root in ScanRoots)
        {
            if (AssetDatabase.IsValidFolder(root))
            {
                roots.Add(root);
            }
            else
            {
                result.AddScanRoot(root, false, 0, "Missing scan root. This is INFO in V1.");
                result.AddInfo("UNITY_SCAN_ROOT_MISSING", root + " is absent.");
            }
        }

        var mybRootFullPath = Application.dataPath;
        foreach (var directory in Directory.EnumerateDirectories(mybRootFullPath, "MYB*", SearchOption.TopDirectoryOnly))
        {
            var root = ToUnityPath(directory);
            if (!string.IsNullOrEmpty(root))
            {
                roots.Add(root);
            }
        }

        if (!roots.Any(root => root.StartsWith("Assets/MYB", StringComparison.Ordinal)))
        {
            result.AddInfo("UNITY_SCAN_ROOT_MISSING", "No Assets/MYB* ticket-local scan roots were found.");
        }

        return roots.Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(root => root, StringComparer.Ordinal).ToList();
    }

    private static bool IsIgnoredByDirectory(string unityPath, Dictionary<string, AssetEntry> manifestByPath)
    {
        if (FindManifestEntryForPath(manifestByPath, unityPath) != null)
        {
            return false;
        }

        var parts = unityPath.Split('/');
        return parts.Any(part => IgnoredDirectoryNames.Contains(part, StringComparer.OrdinalIgnoreCase));
    }

    private static bool IsCandidateAssetPath(string unityPath)
    {
        var extension = Path.GetExtension(unityPath).ToLowerInvariant();
        return CandidateExtensions.Contains(extension);
    }

    private static AssetEntry FindManifestEntryForPath(Dictionary<string, AssetEntry> manifestByPath, string assetPath)
    {
        var normalizedAssetPath = NormalizeUnityManifestPath(assetPath);
        if (manifestByPath.TryGetValue(normalizedAssetPath, out var exact))
        {
            return exact;
        }

        foreach (var manifestPath in manifestByPath.Keys)
        {
            if (!IsManifestDirectoryPath(manifestPath))
            {
                continue;
            }

            var directoryPrefix = manifestPath.TrimEnd('/') + "/";
            if (normalizedAssetPath.StartsWith(directoryPrefix, StringComparison.OrdinalIgnoreCase))
            {
                return manifestByPath[manifestPath];
            }
        }

        return null;
    }

    private static bool IsManifestDirectoryPath(string manifestPath)
    {
        if (string.IsNullOrWhiteSpace(manifestPath))
        {
            return false;
        }

        return manifestPath.EndsWith("/", StringComparison.Ordinal) ||
               string.IsNullOrEmpty(Path.GetExtension(manifestPath));
    }

    private static void AddUnmanifestedCandidate(ValidationResult result, string assetPath)
    {
        if (assetPath.IndexOf("/Quarantine/", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            result.AddInfo(
                "ASSET_QUARANTINE_UNMANIFESTED_INFO",
                "Quarantine asset candidate is not listed in the manifest yet: `" + assetPath + "`.");
            return;
        }

        var code = assetPath.IndexOf("/Production/", StringComparison.OrdinalIgnoreCase) >= 0
            ? "ASSET_CANDIDATE_UNMANIFESTED_PRODUCTION_PATH"
            : "ASSET_CANDIDATE_UNMANIFESTED";

        result.AddWarning(
            code,
            assetPath,
            "Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest.",
            "Add a manifest entry if this candidate should enter review or production.");
    }

    private static void ValidateUnityCandidate(ValidationResult result, string assetPath, AssetEntry entry)
    {
        var extension = Path.GetExtension(assetPath).ToLowerInvariant();
        var isPromoted = entry != null && entry.promotionStatus == "promoted";
        var mainAsset = AssetDatabase.LoadMainAssetAtPath(assetPath);

        if (mainAsset == null)
        {
            if (isPromoted)
            {
                result.AddError(
                    "UNITY_PROMOTED_ASSET_LOAD_FAILED",
                    AssetLabel(entry, assetPath),
                    "Promoted asset could not be loaded by AssetDatabase: `" + assetPath + "`.",
                    "Reimport, fix the path, or downgrade promotionStatus.");
            }
            else
            {
                result.AddWarning(
                    "UNITY_ASSET_LOAD_FAILED_WARNING",
                    assetPath,
                    "Asset candidate could not be loaded by AssetDatabase.",
                    "Check whether this file is a valid Unity asset candidate.");
            }

            return;
        }

        if (extension == ".png" ||
            extension == ".jpg" ||
            extension == ".jpeg" ||
            extension == ".tga" ||
            extension == ".exr" ||
            extension == ".psd")
        {
            ValidateTexture(result, assetPath, mainAsset as Texture2D, extension);
        }

        if (extension == ".prefab")
        {
            ValidatePrefab(result, assetPath, entry);
            return;
        }

        if (extension == ".fbx" ||
            extension == ".glb" ||
            extension == ".gltf" ||
            extension == ".obj")
        {
            ValidateModel(result, assetPath, entry);
            return;
        }

        if (extension == ".mat")
        {
            ValidateMaterial(result, assetPath, mainAsset as Material, entry);
            return;
        }

        if (extension == ".asset")
        {
            result.AddWarning(
                "ASSET_UNITY_ASSET_AMBIGUOUS_WARNING",
                assetPath,
                ".asset file found in an Art Rescue scan root. V1 treats it cautiously.",
                "Ensure this ScriptableObject-like asset belongs in the Art Rescue asset pipeline.");
        }
    }

    private static void ValidateTexture(ValidationResult result, string assetPath, Texture2D texture, string extension)
    {
        if (texture != null && Math.Max(texture.width, texture.height) > TextureWarningDimension)
        {
            result.AddWarning(
                "UNITY_TEXTURE_SIZE_WARNING",
                assetPath,
                "Texture is " + texture.width + "x" + texture.height + ", above V1 warning threshold " + TextureWarningDimension + ".",
                "Reduce texture size or document why the size is needed.");
        }

        if (extension == ".psd" && assetPath.IndexOf("/Production/", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            result.AddWarning(
                "UNITY_PSD_IN_PRODUCTION_WARNING",
                assetPath,
                "PSD source image is present under a Production path.",
                "Consider exporting a runtime texture and keeping PSD source outside Production.");
        }
    }

    private static void ValidateMaterial(ValidationResult result, string assetPath, Material material, AssetEntry entry)
    {
        if (material == null)
        {
            AddMaterialIssue(result, assetPath, entry, "Material asset could not be loaded.");
        }
    }

    private static void ValidatePrefab(ValidationResult result, string assetPath, AssetEntry entry)
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
        if (prefab == null)
        {
            if (entry != null && entry.promotionStatus == "promoted")
            {
                result.AddError(
                    "UNITY_PROMOTED_ASSET_LOAD_FAILED",
                    AssetLabel(entry, assetPath),
                    "Promoted prefab could not be loaded.",
                    "Fix the prefab or downgrade promotionStatus.");
            }

            return;
        }

        ValidateGameObjectRenderers(result, prefab, assetPath, entry);
        ValidateGameObjectColliders(result, prefab, assetPath, entry);
        ValidateMaterialCount(result, prefab, assetPath);
        ValidateBounds(result, prefab, assetPath);
    }

    private static void ValidateModel(ValidationResult result, string assetPath, AssetEntry entry)
    {
        var model = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
        if (model != null)
        {
            ValidateGameObjectRenderers(result, model, assetPath, entry);
            ValidateGameObjectColliders(result, model, assetPath, entry);
            ValidateMaterialCount(result, model, assetPath);
            ValidateBounds(result, model, assetPath);
            return;
        }
    }

    private static void ValidateGameObjectRenderers(ValidationResult result, GameObject root, string assetPath, AssetEntry entry)
    {
        foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
        {
            var materials = renderer.sharedMaterials;
            if (materials == null || materials.Length == 0 || materials.Any(material => material == null))
            {
                AddMaterialIssue(result, assetPath, entry, "Renderer `" + renderer.name + "` has missing material slots.");
            }
        }
    }

    private static void ValidateGameObjectColliders(ValidationResult result, GameObject root, string assetPath, AssetEntry entry)
    {
        foreach (var collider in root.GetComponentsInChildren<MeshCollider>(true))
        {
            var mesh = collider.sharedMesh;
            if (mesh == null)
            {
                continue;
            }

            var triangles = mesh.triangles.Length / 3;
            if (triangles <= ComplexMeshColliderTriangleThreshold)
            {
                continue;
            }

            if (entry != null && entry.promotionStatus == "promoted")
            {
                result.AddError(
                    "UNITY_PROMOTED_MESH_COLLIDER_COMPLEX",
                    AssetLabel(entry, assetPath),
                    "MeshCollider `" + collider.name + "` uses " + triangles + " triangles, above V1 complex threshold " + ComplexMeshColliderTriangleThreshold + ".",
                    "Replace with primitive or simplified colliders.");
            }
            else
            {
                result.AddWarning(
                    "UNITY_NON_PROMOTED_MESH_COLLIDER_COMPLEX_WARNING",
                    assetPath,
                    "MeshCollider `" + collider.name + "` uses " + triangles + " triangles.",
                    "Use simplified colliders before production promotion.");
            }
        }
    }

    private static void ValidateMaterialCount(ValidationResult result, GameObject root, string assetPath)
    {
        var materialCount = root
            .GetComponentsInChildren<Renderer>(true)
            .SelectMany(renderer => renderer.sharedMaterials ?? Array.Empty<Material>())
            .Where(material => material != null)
            .Distinct()
            .Count();

        if (materialCount > MaterialCountWarningThreshold)
        {
            result.AddWarning(
                "UNITY_MATERIAL_COUNT_WARNING",
                assetPath,
                "Asset uses " + materialCount + " distinct materials, above V1 warning threshold " + MaterialCountWarningThreshold + ".",
                "Reduce material count or document why this is needed.");
        }
    }

    private static void ValidateBounds(ValidationResult result, GameObject root, string assetPath)
    {
        var renderers = root.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length == 0)
        {
            return;
        }

        var bounds = renderers[0].bounds;
        for (var index = 1; index < renderers.Length; index++)
        {
            bounds.Encapsulate(renderers[index].bounds);
        }

        var size = bounds.size;
        var max = Mathf.Max(size.x, Mathf.Max(size.y, size.z));
        var min = Mathf.Min(size.x, Mathf.Min(size.y, size.z));

        if (max > 100f || (max > 0f && min > 0f && max / min > 100f))
        {
            result.AddWarning(
                "UNITY_BOUNDS_SUSPICIOUS_WARNING",
                assetPath,
                "Renderer bounds look suspicious: " + FormatVector(size) + ".",
                "Review scale/bounds manually; V1 does not hard-fail bounds.");
        }
    }

    private static void AddMaterialIssue(ValidationResult result, string assetPath, AssetEntry entry, string message)
    {
        if (entry != null && entry.promotionStatus == "promoted")
        {
            result.AddError(
                "UNITY_PROMOTED_MATERIAL_MISSING",
                AssetLabel(entry, assetPath),
                message,
                "Assign valid materials before production promotion.");
        }
        else
        {
            result.AddWarning(
                "UNITY_NON_PROMOTED_MATERIAL_MISSING",
                assetPath,
                message,
                "Assign valid materials before promotion.");
        }
    }

    private static string AssetLabel(AssetEntry entry, string fallbackPath)
    {
        if (entry == null)
        {
            return fallbackPath;
        }

        if (!string.IsNullOrWhiteSpace(entry.id))
        {
            return entry.id;
        }

        return fallbackPath;
    }

    private static string ToUnityPath(string fullPath)
    {
        if (string.IsNullOrWhiteSpace(fullPath))
        {
            return string.Empty;
        }

        var normalizedFullPath = fullPath.Replace('\\', '/');
        var normalizedAssetsPath = Application.dataPath.Replace('\\', '/');

        if (!normalizedFullPath.StartsWith(normalizedAssetsPath, StringComparison.OrdinalIgnoreCase))
        {
            return string.Empty;
        }

        var relative = normalizedFullPath.Substring(normalizedAssetsPath.Length).TrimStart('/');
        return string.IsNullOrEmpty(relative) ? "Assets" : "Assets/" + relative;
    }

    private static string NormalizeUnityManifestPath(string path)
    {
        return (path ?? string.Empty).Replace('\\', '/').Trim();
    }

    private static string GetRepoRoot()
    {
        return Path.GetFullPath(Path.Combine(Application.dataPath, "../../.."));
    }

    private static string FormatVector(Vector3 value)
    {
        return value.x.ToString("0.###", CultureInfo.InvariantCulture) + " x " +
            value.y.ToString("0.###", CultureInfo.InvariantCulture) + " x " +
            value.z.ToString("0.###", CultureInfo.InvariantCulture);
    }

    private static void WriteReport(ValidationResult result)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(result.ReportPath));
        File.WriteAllText(result.ReportPath, result.ToMarkdown(), new UTF8Encoding(false));
    }

    [Serializable]
    private sealed class ManifestData
    {
        public int schemaVersion;
        public string updatedAt;
        public AssetEntry[] assets = Array.Empty<AssetEntry>();
    }

    [Serializable]
    private sealed class AssetEntry
    {
        public string id;
        public string name;
        public string sourceType;
        public string provider;
        public string sourceUrl;
        public string license;
        public string licenseUrl;
        public string author;
        public string acquiredAt;
        public string intakeStatus;
        public string promotionStatus;
        public string usageScope;
        public string[] assetPaths = Array.Empty<string>();
        public string[] derivedFrom = Array.Empty<string>();
        public bool aiGenerated;
        public bool requiresAttribution;
        public string attributionText;
        public string visualImpact;
        public string[] routeEvidence = Array.Empty<string>();
        public string[] overviewEvidence = Array.Empty<string>();
        public string[] validatorEvidence = Array.Empty<string>();
        public string notes;
    }

    public sealed class ValidationResult
    {
        public string ExecutionMode;
        public string RepoRoot;
        public string ManifestPath;
        public string ReportPath;
        public int ManifestSchemaVersion;
        public string ManifestUpdatedAt;
        public int ManifestAssetCount;

        private readonly List<Finding> errors = new List<Finding>();
        private readonly List<Finding> warnings = new List<Finding>();
        private readonly List<InfoFinding> infos = new List<InfoFinding>();
        private readonly List<ScanRootSummary> scanRoots = new List<ScanRootSummary>();
        private readonly Dictionary<string, int> statusCounts = new Dictionary<string, int>(StringComparer.Ordinal);

        public int ErrorCount => errors.Count;
        public int WarningCount => warnings.Count;
        public int InfoCount => infos.Count;
        public string Verdict => ErrorCount > 0 ? "FAIL" : WarningCount > 0 ? "PASS_WITH_WARNINGS" : "PASS";

        public void AddError(string code, string assetOrPath, string message, string recommendedFix)
        {
            errors.Add(new Finding(code, assetOrPath, message, recommendedFix));
        }

        public void AddWarning(string code, string assetOrPath, string message, string recommendedFix)
        {
            warnings.Add(new Finding(code, assetOrPath, message, recommendedFix));
        }

        public void AddInfo(string code, string message)
        {
            infos.Add(new InfoFinding(code, message));
        }

        public void AddScanRoot(string root, bool exists, int assetsFound, string notes)
        {
            scanRoots.Add(new ScanRootSummary(root, exists, assetsFound, notes));
        }

        public void IncrementStatus(string key)
        {
            if (!statusCounts.ContainsKey(key))
            {
                statusCounts[key] = 0;
            }

            statusCounts[key]++;
        }

        public string ToConsoleSummary()
        {
            return "MYB-144 Art Asset Validator: " + Verdict +
                " (Errors: " + ErrorCount +
                ", Warnings: " + WarningCount +
                ", Info: " + InfoCount +
                "). Report: " + ReportPath;
        }

        public string ToMarkdown()
        {
            var builder = new StringBuilder();
            builder.AppendLine("# MYB-144 Art Asset Validator Report");
            builder.AppendLine();
            builder.AppendLine("Verdict: " + Verdict);
            builder.AppendLine();
            builder.AppendLine("Execution:");
            builder.AppendLine("- Mode: " + ExecutionMode);
            builder.AppendLine("- Batch exit code: " + (ErrorCount > 0 ? "1 when run through RunBatch" : "0 when run through RunBatch"));
            builder.AppendLine();
            builder.AppendLine("Summary:");
            builder.AppendLine("- Errors: " + ErrorCount);
            builder.AppendLine("- Warnings: " + WarningCount);
            builder.AppendLine("- Info: " + InfoCount);
            builder.AppendLine();
            builder.AppendLine("Manifest:");
            builder.AppendLine("- path: `" + ManifestRelativePath + "`");
            builder.AppendLine("- schemaVersion: " + ManifestSchemaVersion);
            builder.AppendLine("- updatedAt: " + (string.IsNullOrWhiteSpace(ManifestUpdatedAt) ? "(missing)" : ManifestUpdatedAt));
            builder.AppendLine("- asset count: " + ManifestAssetCount);
            builder.AppendLine("- schema reference: `" + SchemaRelativePath + "`");
            builder.AppendLine();
            builder.AppendLine("Report:");
            builder.AppendLine("- `" + ReportRelativePath + "`");
            builder.AppendLine();
            AppendCandidateExtensionPolicy(builder);
            AppendSeverityPolicy(builder);
            AppendScanRoots(builder);
            AppendStatusSummary(builder);
            AppendFindings(builder, "ERROR", errors);
            AppendFindings(builder, "WARNING", warnings);
            AppendInfos(builder);
            AppendDeferredChecks(builder);
            return builder.ToString();
        }

        private static void AppendCandidateExtensionPolicy(StringBuilder builder)
        {
            builder.AppendLine("## Candidate Extension Policy");
            builder.AppendLine();
            builder.AppendLine("Scanned asset candidate extensions:");
            foreach (var extension in CandidateExtensions.OrderBy(value => value, StringComparer.Ordinal))
            {
                builder.AppendLine("- `" + extension + "`");
            }

            builder.AppendLine();
            builder.AppendLine("Ignored by default:");
            foreach (var extension in IgnoredExtensions.OrderBy(value => value, StringComparer.Ordinal))
            {
                builder.AppendLine("- `" + extension + "`");
            }

            builder.AppendLine();
            builder.AppendLine("Notes:");
            builder.AppendLine("- Scene validation is out of MYB-144 V1 scope.");
            builder.AppendLine("- Non-manifest JSON files are ignored.");
            builder.AppendLine("- `.asset` files are scanned cautiously only inside Art Rescue roots.");
            builder.AppendLine();
        }

        private static void AppendSeverityPolicy(StringBuilder builder)
        {
            builder.AppendLine("## Severity Policy V1");
            builder.AppendLine();
            builder.AppendLine("Manifest errors are always ERROR.");
            builder.AppendLine();
            builder.AppendLine("Unity technical checks are ERROR only for `promotionStatus: promoted` assets.");
            builder.AppendLine();
            builder.AppendLine("For candidate, review, quarantine, non-manifested or ambiguous assets, technical issues are WARNING or INFO in V1.");
            builder.AppendLine();
            builder.AppendLine("Thresholds:");
            builder.AppendLine("- texture max dimension > 2048 => WARNING");
            builder.AppendLine("- `.psd` under Production path => WARNING");
            builder.AppendLine("- MeshCollider sharedMesh triangles > 500 => complex");
            builder.AppendLine("- complex MeshCollider on promoted asset => ERROR");
            builder.AppendLine("- complex MeshCollider on non-promoted asset => WARNING");
            builder.AppendLine("- material count > 4 => WARNING");
            builder.AppendLine("- triangle count and suspicious bounds => WARNING only in V1");
            builder.AppendLine();
        }

        private void AppendScanRoots(StringBuilder builder)
        {
            builder.AppendLine("## Scan Roots");
            builder.AppendLine();
            builder.AppendLine("| Root | Exists | Assets found | Notes |");
            builder.AppendLine("|---|---:|---:|---|");
            foreach (var root in scanRoots)
            {
                builder.AppendLine("| `" + root.Root + "` | " + (root.Exists ? "Yes" : "No") + " | " + root.AssetsFound + " | " + Escape(root.Notes) + " |");
            }

            builder.AppendLine();
        }

        private void AppendStatusSummary(StringBuilder builder)
        {
            builder.AppendLine("## Status Summary");
            builder.AppendLine();
            builder.AppendLine("| intakeStatus / promotionStatus | Count |");
            builder.AppendLine("|---|---:|");
            if (statusCounts.Count == 0)
            {
                builder.AppendLine("| (none) | 0 |");
            }
            else
            {
                foreach (var pair in statusCounts.OrderBy(pair => pair.Key, StringComparer.Ordinal))
                {
                    builder.AppendLine("| `" + pair.Key + "` | " + pair.Value + " |");
                }
            }

            builder.AppendLine();
        }

        private static void AppendFindings(StringBuilder builder, string heading, IReadOnlyList<Finding> findings)
        {
            builder.AppendLine("## " + heading);
            builder.AppendLine();
            builder.AppendLine("| Code | Asset id/path | Message | Recommended fix |");
            builder.AppendLine("|---|---|---|---|");
            if (findings.Count == 0)
            {
                builder.AppendLine("| - | - | None | - |");
            }
            else
            {
                foreach (var finding in findings)
                {
                    builder.AppendLine("| `" + finding.Code + "` | `" + Escape(finding.AssetOrPath) + "` | " + Escape(finding.Message) + " | " + Escape(finding.RecommendedFix) + " |");
                }
            }

            builder.AppendLine();
        }

        private void AppendInfos(StringBuilder builder)
        {
            builder.AppendLine("## INFO");
            builder.AppendLine();
            builder.AppendLine("| Code | Message |");
            builder.AppendLine("|---|---|");
            if (infos.Count == 0)
            {
                builder.AppendLine("| - | None |");
            }
            else
            {
                foreach (var info in infos)
                {
                    builder.AppendLine("| `" + info.Code + "` | " + Escape(info.Message) + " |");
                }
            }

            builder.AppendLine();
        }

        private static void AppendDeferredChecks(StringBuilder builder)
        {
            builder.AppendLine("## Deferred Checks");
            builder.AppendLine();
            builder.AppendLine("The following checks are intentionally out of V1 scope:");
            builder.AppendLine("- visual quality;");
            builder.AppendLine("- silhouette quality;");
            builder.AppendLine("- route-camera validation;");
            builder.AppendLine("- pivot heuristics;");
            builder.AppendLine("- full LOD policy;");
            builder.AppendLine("- broad project-wide scan.");
            builder.AppendLine();
        }

        private static string Escape(string value)
        {
            return (value ?? string.Empty).Replace("|", "\\|").Replace("\r", " ").Replace("\n", "<br>");
        }
    }

    private readonly struct Finding
    {
        public Finding(string code, string assetOrPath, string message, string recommendedFix)
        {
            Code = code;
            AssetOrPath = assetOrPath;
            Message = message;
            RecommendedFix = recommendedFix;
        }

        public string Code { get; }
        public string AssetOrPath { get; }
        public string Message { get; }
        public string RecommendedFix { get; }
    }

    private readonly struct InfoFinding
    {
        public InfoFinding(string code, string message)
        {
            Code = code;
            Message = message;
        }

        public string Code { get; }
        public string Message { get; }
    }

    private readonly struct ScanRootSummary
    {
        public ScanRootSummary(string root, bool exists, int assetsFound, string notes)
        {
            Root = root;
            Exists = exists;
            AssetsFound = assetsFound;
            Notes = notes;
        }

        public string Root { get; }
        public bool Exists { get; }
        public int AssetsFound { get; }
        public string Notes { get; }
    }
}

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
using UnityEngine.UI;

namespace MYB108.Editor
{
    public static class MYB108ForestAssetReview
    {
        private const string CanonicalScenePath = "Assets/Scenes/MYB89UnityMcpProbe.unity";
        private const string OutputDirectory = "_bmad-output/unity-test-results/myb-108";
        private const string ReviewPath = OutputDirectory + "/forest-asset-review.md";
        private const int CaptureWidth = 1280;
        private const int CaptureHeight = 720;
        private const int HalfWidth = CaptureWidth / 2;
        private const float IsolationOriginX = 2500f;
        private const float IsolationOriginZ = 2500f;

        private static readonly FamilySpec[] Families =
        {
            new FamilySpec(
                "forest-surface-ribbons",
                "Forest surface ribbons and material mats",
                42f,
                "Surfaces that shape the forest floor, roadside shadows, root edges and deep undergrowth mats.",
                new[]
                {
                    "MYB104_Forest_LeftDeepFloor",
                    "MYB104_Forest_RightLightFloor",
                    "MYB104_Forest_RoadShadowLace_",
                    "MYB104_Forest_InnerTreeMass_",
                    "MYB106_LeftBlueShadowEdge",
                    "MYB106_RightBlueShadowEdge",
                    "MYB106_LeftDeepForestMat",
                    "MYB106_RightDeepForestMat",
                    "MYB106_LeftInnerForestMat",
                    "MYB106_RightInnerForestMat",
                    "MYB106_LeftSoftRootEdge",
                    "MYB106_RightSoftRootEdge"
                }),
            new FamilySpec(
                "near-premium-trees",
                "Near ride premium trees",
                38f,
                "Tree families close enough to define the ride tunnel and first-person silhouette.",
                new[]
                {
                    "MYB104_ForestNearPine_",
                    "MYB106_DeepPine_",
                    "MYB106_InnerCanopyPine_"
                }),
            new FamilySpec(
                "mid-background-tree-curtain",
                "Mid and background tree curtain",
                50f,
                "Repeated side and background trees that create forest density and horizon framing.",
                new[]
                {
                    "MYB104_ForestTallPine_",
                    "MYB104_ForestPine_R_",
                    "MYB104_ForestInnerCanopy_",
                    "MYB104_ForestMidPine_",
                    "MYB104_ForestBackPine_",
                    "MYB106_MidForestPine_",
                    "MYB106_BackForestPine_"
                }),
            new FamilySpec(
                "undergrowth-clusters",
                "Low undergrowth clusters",
                40f,
                "Low masses and fern-like pieces added by the Passage 01 lookdev overlay.",
                new[]
                {
                    "MYB106_UndergrowthCluster_"
                }),
            new FamilySpec(
                "moss-and-fallen-branches",
                "Moss accents and fallen branches",
                44f,
                "Small ground accents that add forest detail close to the road edge.",
                new[]
                {
                    "MYB106_AmberMoss_",
                    "MYB106_FallenBranch_"
                }),
            new FamilySpec(
                "stone-edge-guides",
                "Stone edge guides",
                46f,
                "Small repeated stones that mark the ride edge in Passage 01.",
                new[]
                {
                    "MYB104_ForestStoneGuide_"
                })
        };

        [MenuItem("Tools/MYB/Validation/MYB-108/Capture Forest Asset Review")]
        public static void CaptureReviewFromMenu()
        {
            Debug.Log("MYB-108 forest asset review captured: " + CaptureReview());
        }

        public static string CaptureReviewCli()
        {
            var reviewPath = CaptureReview();
            Debug.Log("MYB-108 forest asset review captured: " + reviewPath);
            return reviewPath;
        }

        public static string CaptureCurrentSceneReviewCli()
        {
            var reviewPath = CaptureReview(false);
            Debug.Log("MYB-108 current scene forest asset review captured: " + reviewPath);
            return reviewPath;
        }

        public static string CaptureReview()
        {
            return CaptureReview(true);
        }

        private static string CaptureReview(bool rebuildVisualStack)
        {
            Directory.CreateDirectory(Path.Combine(GetRepoRoot(), OutputDirectory));

            if (rebuildVisualStack)
            {
                // Rebuild the latest controlled visual stack before taking review evidence.
                MYB107.Editor.MYB107StylizedMaterialPass.ApplyAndValidate();
            }

            var scene = EditorSceneManager.OpenScene(CanonicalScenePath, OpenSceneMode.Single);
            var ride = UnityEngine.Object.FindAnyObjectByType<MYB89ProbeRide>();
            var camera = Camera.main;
            var failures = new List<string>();
            if (ride == null)
            {
                failures.Add("Missing MYB89ProbeRide in canonical scene.");
            }

            if (camera == null)
            {
                failures.Add("Missing Main Camera in canonical scene.");
            }

            var reviewEntries = new List<ReviewEntry>();
            if (ride != null && camera != null)
            {
                ride.RebuildRouteCache();
                var canvasStates = DisableCanvases();
                try
                {
                    foreach (var family in Families)
                    {
                        reviewEntries.Add(CaptureFamily(family, ride, camera));
                    }
                }
                finally
                {
                    RestoreCanvases(canvasStates);
                }
            }

            WriteReviewMarkdown(reviewEntries, failures);
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            if (failures.Count > 0)
            {
                throw new InvalidOperationException("MYB-108 forest asset review failed. See " + Path.Combine(GetRepoRoot(), ReviewPath));
            }

            return Path.Combine(GetRepoRoot(), ReviewPath);
        }

        private static ReviewEntry CaptureFamily(FamilySpec family, MYB89ProbeRide ride, Camera rideCamera)
        {
            var roots = FindFamilyRoots(family).ToList();
            var samples = roots
                .OrderBy(root => Mathf.Abs(root.transform.position.z - family.ContextMeters))
                .ThenBy(root => root.name, StringComparer.Ordinal)
                .Take(3)
                .ToList();

            var contextPath = OutputPath(family.Slug + "-context.png");
            var isolatedPath = OutputPath(family.Slug + "-isolated.png");
            var sheetPath = OutputPath(family.Slug + "-sheet.png");

            ride.SetPreviewProgress(family.ContextMeters);
            RenderCameraToPng(rideCamera, CaptureWidth, CaptureHeight, contextPath);
            CaptureIsolatedSamples(samples, isolatedPath);
            ComposeSheet(isolatedPath, contextPath, sheetPath);

            return new ReviewEntry(
                family,
                roots,
                samples.Select(sample => sample.name).ToArray(),
                contextPath,
                isolatedPath,
                sheetPath);
        }

        private static IEnumerable<GameObject> FindFamilyRoots(FamilySpec family)
        {
            var allTransforms = UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var transform in allTransforms)
            {
                if (transform == null || transform.gameObject == null)
                {
                    continue;
                }

                if (!family.Prefixes.Any(prefix => transform.name.StartsWith(prefix, StringComparison.Ordinal)))
                {
                    continue;
                }

                if (transform.GetComponentsInChildren<Renderer>(true).Length == 0)
                {
                    continue;
                }

                yield return transform.gameObject;
            }
        }

        private static void CaptureIsolatedSamples(IReadOnlyList<GameObject> samples, string path)
        {
            var tempRoot = new GameObject("MYB108_TemporaryIsolationRoot");
            tempRoot.transform.position = new Vector3(IsolationOriginX, 0f, IsolationOriginZ);
            var tempCameraObject = new GameObject("MYB108_TemporaryIsolationCamera");
            var tempCamera = tempCameraObject.AddComponent<Camera>();
            var tempLightObject = new GameObject("MYB108_TemporaryIsolationLight");
            var tempLight = tempLightObject.AddComponent<Light>();
            tempLight.type = LightType.Directional;
            tempLight.intensity = 1.8f;
            tempLight.transform.rotation = Quaternion.Euler(48f, -28f, 0f);
            tempCamera.clearFlags = CameraClearFlags.SolidColor;
            tempCamera.backgroundColor = new Color(0.12f, 0.13f, 0.13f, 1f);
            tempCamera.fieldOfView = 32f;
            tempCamera.nearClipPlane = 0.03f;
            tempCamera.farClipPlane = 500f;

            try
            {
                if (samples.Count == 0)
                {
                    RenderCameraToPng(tempCamera, CaptureWidth, CaptureHeight, path);
                    return;
                }

                var spacing = 7.5f;
                for (var index = 0; index < samples.Count; index++)
                {
                    var clone = UnityEngine.Object.Instantiate(samples[index]);
                    clone.name = samples[index].name + "_ReviewClone";
                    clone.transform.SetParent(tempRoot.transform, false);
                    clone.transform.localPosition = Vector3.right * ((index - (samples.Count - 1) * 0.5f) * spacing);
                    clone.transform.localRotation = Quaternion.identity;
                    foreach (var lodGroup in clone.GetComponentsInChildren<LODGroup>(true))
                    {
                        lodGroup.ForceLOD(0);
                    }

                    DisableColliders(clone);
                }

                var bounds = GetWorldBounds(tempRoot);
                var maxSize = Mathf.Max(2f, bounds.size.x, bounds.size.y, bounds.size.z);
                var target = bounds.center + Vector3.up * Mathf.Max(0.2f, bounds.size.y * 0.05f);
                tempCamera.transform.position = target + new Vector3(0f, maxSize * 0.32f, -maxSize * 1.75f);
                tempCamera.transform.LookAt(target);
                RenderCameraToPng(tempCamera, CaptureWidth, CaptureHeight, path);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(tempCameraObject);
                UnityEngine.Object.DestroyImmediate(tempLightObject);
                UnityEngine.Object.DestroyImmediate(tempRoot);
            }
        }

        private static void ComposeSheet(string isolatedPath, string contextPath, string sheetPath)
        {
            var isolated = LoadPng(isolatedPath);
            var context = LoadPng(contextPath);
            var sheet = new Texture2D(CaptureWidth, CaptureHeight, TextureFormat.RGBA32, false);
            try
            {
                Fill(sheet, new Color32(24, 25, 25, 255));
                BlitScaled(isolated, sheet, new RectInt(0, 0, HalfWidth, CaptureHeight));
                BlitScaled(context, sheet, new RectInt(HalfWidth, 0, HalfWidth, CaptureHeight));
                sheet.Apply();
                File.WriteAllBytes(sheetPath, sheet.EncodeToPNG());
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(isolated);
                UnityEngine.Object.DestroyImmediate(context);
                UnityEngine.Object.DestroyImmediate(sheet);
            }
        }

        private static Texture2D LoadPng(string path)
        {
            var bytes = File.ReadAllBytes(path);
            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            texture.LoadImage(bytes);
            return texture;
        }

        private static void Fill(Texture2D texture, Color32 color)
        {
            var pixels = Enumerable.Repeat(color, texture.width * texture.height).ToArray();
            texture.SetPixels32(pixels);
        }

        private static void BlitScaled(Texture2D source, Texture2D destination, RectInt destinationRect)
        {
            var pixels = new Color[destinationRect.width * destinationRect.height];
            for (var y = 0; y < destinationRect.height; y++)
            {
                for (var x = 0; x < destinationRect.width; x++)
                {
                    var sourceX = Mathf.Clamp(Mathf.RoundToInt(x / (float)Mathf.Max(1, destinationRect.width - 1) * (source.width - 1)), 0, source.width - 1);
                    var sourceY = Mathf.Clamp(Mathf.RoundToInt(y / (float)Mathf.Max(1, destinationRect.height - 1) * (source.height - 1)), 0, source.height - 1);
                    pixels[y * destinationRect.width + x] = source.GetPixel(sourceX, sourceY);
                }
            }

            destination.SetPixels(destinationRect.x, destinationRect.y, destinationRect.width, destinationRect.height, pixels);
        }

        private static void RenderCameraToPng(Camera camera, int width, int height, string path)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path) ?? GetRepoRoot());
            var previousTarget = camera.targetTexture;
            var previousActive = RenderTexture.active;
            var renderTexture = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);

            try
            {
                camera.targetTexture = renderTexture;
                RenderTexture.active = renderTexture;
                camera.Render();
                texture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                texture.Apply();
                File.WriteAllBytes(path, texture.EncodeToPNG());
            }
            finally
            {
                camera.targetTexture = previousTarget;
                RenderTexture.active = previousActive;
                renderTexture.Release();
                UnityEngine.Object.DestroyImmediate(renderTexture);
                UnityEngine.Object.DestroyImmediate(texture);
            }
        }

        private static void WriteReviewMarkdown(IReadOnlyList<ReviewEntry> entries, IReadOnlyList<string> failures)
        {
            var lines = new List<string>
            {
                "# MYB-108 Forest Asset Review",
                string.Empty,
                "Phase A output only: inventory and review evidence for Passage 01 / forest. Do not delete, replace, or adjust scene assets before user verdicts.",
                string.Empty,
                "## Decision Table",
                string.Empty,
                "| Family | Roots | Renderers | Stored triangles | Sheet | Verdict | Adjustment notes |",
                "| --- | ---: | ---: | ---: | --- | --- | --- |"
            };

            foreach (var entry in entries)
            {
                lines.Add("| " + entry.Family.Title + " | "
                    + entry.RootCount.ToString(CultureInfo.InvariantCulture) + " | "
                    + entry.RendererCount.ToString(CultureInfo.InvariantCulture) + " | "
                    + entry.TriangleCount.ToString(CultureInfo.InvariantCulture) + " | "
                    + LinkFor(entry.SheetPath) + " | `supprimer` / `refaire-remplacer` / `garder mais ajuster` |  |");
            }

            lines.Add(string.Empty);
            lines.Add("## Families");
            foreach (var entry in entries)
            {
                lines.Add(string.Empty);
                lines.Add("### " + entry.Family.Title);
                lines.Add(string.Empty);
                lines.Add("Definition: " + entry.Family.Description);
                lines.Add(string.Empty);
                lines.Add("- Context meters: " + entry.Family.ContextMeters.ToString("0.0", CultureInfo.InvariantCulture));
                lines.Add("- Root objects: " + entry.RootCount.ToString(CultureInfo.InvariantCulture));
                lines.Add("- Renderer count: " + entry.RendererCount.ToString(CultureInfo.InvariantCulture));
                lines.Add("- Stored triangles: " + entry.TriangleCount.ToString(CultureInfo.InvariantCulture));
                lines.Add("- Materials: " + string.Join(", ", entry.MaterialNames));
                lines.Add("- Representative samples: " + string.Join(", ", entry.SampleNames));
                lines.Add("- Isolated capture: " + LinkFor(entry.IsolatedPath));
                lines.Add("- Context capture: " + LinkFor(entry.ContextPath));
                lines.Add("- Review sheet: " + LinkFor(entry.SheetPath));
            }

            if (failures.Count > 0)
            {
                lines.Add(string.Empty);
                lines.Add("## Failures");
                foreach (var failure in failures)
                {
                    lines.Add("- " + failure);
                }
            }

            File.WriteAllLines(Path.Combine(GetRepoRoot(), ReviewPath), lines);
        }

        private static string LinkFor(string absolutePath)
        {
            return Path.GetRelativePath(GetRepoRoot(), absolutePath).Replace('\\', '/');
        }

        private static Bounds GetWorldBounds(GameObject root)
        {
            var renderers = root.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0)
            {
                return new Bounds(root.transform.position, Vector3.one);
            }

            var bounds = renderers[0].bounds;
            for (var index = 1; index < renderers.Length; index++)
            {
                bounds.Encapsulate(renderers[index].bounds);
            }

            return bounds;
        }

        private static int CountTriangles(GameObject root)
        {
            return root.GetComponentsInChildren<MeshFilter>(true)
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

        private static string[] MaterialNames(IEnumerable<GameObject> roots)
        {
            return roots
                .SelectMany(root => root.GetComponentsInChildren<Renderer>(true))
                .SelectMany(renderer => renderer.sharedMaterials)
                .Where(material => material != null)
                .Select(material => material.name)
                .Distinct(StringComparer.Ordinal)
                .OrderBy(name => name, StringComparer.Ordinal)
                .ToArray();
        }

        private static int RendererCount(IEnumerable<GameObject> roots)
        {
            return roots.Sum(root => root.GetComponentsInChildren<Renderer>(true).Length);
        }

        private static void DisableColliders(GameObject root)
        {
            foreach (var collider in root.GetComponentsInChildren<Collider>(true))
            {
                collider.enabled = false;
            }
        }

        private static List<CanvasState> DisableCanvases()
        {
            var canvases = UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            var states = new List<CanvasState>(canvases.Length);
            foreach (var canvas in canvases)
            {
                states.Add(new CanvasState(canvas, canvas.enabled));
                canvas.enabled = false;
            }

            return states;
        }

        private static void RestoreCanvases(IEnumerable<CanvasState> states)
        {
            foreach (var state in states)
            {
                if (state.Canvas != null)
                {
                    state.Canvas.enabled = state.Enabled;
                }
            }
        }

        private static string OutputPath(string fileName)
        {
            return Path.Combine(GetRepoRoot(), OutputDirectory, fileName);
        }

        private static string GetRepoRoot()
        {
            var current = new DirectoryInfo(Application.dataPath);
            while (current != null && !Directory.Exists(Path.Combine(current.FullName, ".git")))
            {
                current = current.Parent;
            }

            return current?.FullName ?? Directory.GetCurrentDirectory();
        }

        private sealed class FamilySpec
        {
            public FamilySpec(string slug, string title, float contextMeters, string description, IReadOnlyList<string> prefixes)
            {
                Slug = slug;
                Title = title;
                ContextMeters = contextMeters;
                Description = description;
                Prefixes = prefixes;
            }

            public string Slug { get; }
            public string Title { get; }
            public float ContextMeters { get; }
            public string Description { get; }
            public IReadOnlyList<string> Prefixes { get; }
        }

        private sealed class ReviewEntry
        {
            public ReviewEntry(FamilySpec family, IReadOnlyList<GameObject> roots, IReadOnlyList<string> sampleNames, string contextPath, string isolatedPath, string sheetPath)
            {
                Family = family;
                RootCount = roots.Count;
                RendererCount = MYB108ForestAssetReview.RendererCount(roots);
                TriangleCount = roots.Sum(CountTriangles);
                MaterialNames = MYB108ForestAssetReview.MaterialNames(roots);
                SampleNames = sampleNames.Count == 0 ? new[] { "none" } : sampleNames.ToArray();
                ContextPath = contextPath;
                IsolatedPath = isolatedPath;
                SheetPath = sheetPath;
            }

            public FamilySpec Family { get; }
            public int RootCount { get; }
            public int RendererCount { get; }
            public int TriangleCount { get; }
            public string[] MaterialNames { get; }
            public string[] SampleNames { get; }
            public string ContextPath { get; }
            public string IsolatedPath { get; }
            public string SheetPath { get; }
        }

        private readonly struct CanvasState
        {
            public CanvasState(Canvas canvas, bool enabled)
            {
                Canvas = canvas;
                Enabled = enabled;
            }

            public Canvas Canvas { get; }
            public bool Enabled { get; }
        }
    }
}

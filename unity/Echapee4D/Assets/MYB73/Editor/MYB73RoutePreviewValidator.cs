using System;
using System.Collections.Generic;
using System.IO;
using MYB89;
using MYB89.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace MYB73.Editor
{
    public static class MYB73RoutePreviewValidator
    {
        private const string ScreenshotRelativePath = "_bmad-output/unity-test-results/myb-73-route-preview.png";
        private const string ReportRelativePath = "_bmad-output/unity-test-results/myb-73-route-preview-validator.txt";

        [MenuItem("Tools/MYB-73/Validate Route Preview")]
        public static string ValidateRoutePreviewCli()
        {
            MYB89ProbeBuilder.BuildScene();

            var failures = new List<string>();
            var ride = UnityEngine.Object.FindAnyObjectByType<MYB89ProbeRide>();
            var preview = UnityEngine.Object.FindAnyObjectByType<MYB73RoutePreviewPanel>();
            var camera = Camera.main;

            MYB73RoutePreviewSnapshot snapshot = default;
            if (ride == null)
            {
                failures.Add("Missing MYB89ProbeRide.");
            }

            if (preview == null)
            {
                failures.Add("Missing MYB73RoutePreviewPanel.");
            }

            if (camera == null)
            {
                failures.Add("Missing Main Camera for route preview proof.");
            }

            if (preview != null)
            {
                preview.ShowPreview();
                Canvas.ForceUpdateCanvases();
                snapshot = preview.CaptureSnapshot();

                if (!preview.IsVisible)
                {
                    failures.Add("Route preview should be visible before launch.");
                }

                if (preview.launchButton == null)
                {
                    failures.Add("Route preview launch button is not wired.");
                }

                if (preview.titleLabel == null || !preview.titleLabel.text.Contains("Apercu"))
                {
                    failures.Add("Route preview title should expose 'Apercu de l'echappee'.");
                }

                if (preview.statsLabel == null || !preview.statsLabel.text.Contains("env."))
                {
                    failures.Add("Route preview stats should show approximate duration.");
                }

                if (snapshot.RouteLengthMeters < 200f)
                {
                    failures.Add($"Route preview distance is too short: {snapshot.RouteLengthMeters:0.0} m.");
                }

                if (snapshot.EstimatedDurationSeconds < 60f || snapshot.EstimatedDurationSeconds > 180f)
                {
                    failures.Add($"Route preview estimated duration is outside MVP bounds: {snapshot.EstimatedDurationSeconds:0.0} s.");
                }

                if (string.IsNullOrWhiteSpace(snapshot.OverallDifficulty))
                {
                    failures.Add("Route preview difficulty is missing.");
                }

                if (string.IsNullOrWhiteSpace(snapshot.BiomesText) || !snapshot.BiomesText.Contains("Foret"))
                {
                    failures.Add("Route preview biomes are missing or not editorially useful.");
                }

                if (snapshot.MomentCleCount <= 0 || snapshot.MomentCleCount > 3)
                {
                    failures.Add($"Route preview should expose 1 to 3 moments cles, got {snapshot.MomentCleCount}.");
                }

                if (preview.momentsLabel == null || !preview.momentsLabel.text.Contains("Moments cles"))
                {
                    failures.Add("Route preview moments label is not populated.");
                }
            }

            if (ride != null)
            {
                if (!ride.IsRoutePreviewWaiting)
                {
                    failures.Add("Ride should wait on the route preview before launch.");
                }

                if (ride.autoplay)
                {
                    failures.Add("Ride autoplay should be disabled while the route preview is visible.");
                }
            }

            if (camera != null)
            {
                SetCanvasCamera(camera);
                Canvas.ForceUpdateCanvases();
                RenderCameraToPng(camera, 1280, 720, Path.Combine(GetRepoRoot(), ScreenshotRelativePath));
            }

            if (preview != null)
            {
                if (preview.launchButton == null)
                {
                    preview.LaunchRide();
                }
                else
                {
                    preview.launchButton.onClick.Invoke();
                }
            }

            if (preview != null && preview.IsVisible)
            {
                failures.Add("Route preview should hide after launch.");
            }

            if (ride != null)
            {
                if (ride.IsRoutePreviewWaiting)
                {
                    failures.Add("Ride should stop waiting after route preview launch.");
                }

                if (!ride.autoplay)
                {
                    failures.Add("Ride autoplay should resume after route preview launch.");
                }
            }

            var report = WriteReport(failures, snapshot, preview, ride);
            if (failures.Count > 0)
            {
                throw new InvalidOperationException("MYB-73 route preview validation failed. See " + report);
            }

            Debug.Log("MYB-73 route preview validation passed. Report: " + report);
            return report;
        }

        private static string WriteReport(
            IReadOnlyList<string> failures,
            MYB73RoutePreviewSnapshot snapshot,
            MYB73RoutePreviewPanel preview,
            MYB89ProbeRide ride)
        {
            var repoRoot = GetRepoRoot();
            var reportPath = Path.Combine(repoRoot, ReportRelativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(reportPath) ?? repoRoot);

            using (var writer = new StreamWriter(reportPath, false))
            {
                writer.WriteLine("MYB-73 Unity route preview validation");
                writer.WriteLine("Timestamp UTC: " + DateTime.UtcNow.ToString("O"));
                writer.WriteLine("Unity: " + Application.unityVersion);
                writer.WriteLine("Status: " + (failures.Count == 0 ? "PASS" : "FAIL"));
                writer.WriteLine("Screenshot: " + ScreenshotRelativePath);
                writer.WriteLine("Route length: " + snapshot.RouteLengthMeters.ToString("0.0") + " m");
                writer.WriteLine("Estimated duration: " + snapshot.EstimatedDurationSeconds.ToString("0.0") + " s");
                writer.WriteLine("Difficulty: " + snapshot.OverallDifficulty);
                writer.WriteLine("Biomes: " + snapshot.BiomesText);
                writer.WriteLine("Moments cles: " + snapshot.MomentsClesText);
                writer.WriteLine("Moment cle count: " + snapshot.MomentCleCount);
                writer.WriteLine("Preview visible after launch: " + (preview != null && preview.IsVisible));
                writer.WriteLine("Ride autoplay after launch: " + (ride != null && ride.autoplay));

                if (failures.Count > 0)
                {
                    writer.WriteLine();
                    writer.WriteLine("Failures:");
                    foreach (var failure in failures)
                    {
                        writer.WriteLine("- " + failure);
                    }
                }
            }

            return reportPath;
        }

        private static void SetCanvasCamera(Camera camera)
        {
            foreach (var canvas in UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include))
            {
                if (canvas.renderMode == RenderMode.ScreenSpaceCamera)
                {
                    canvas.worldCamera = camera;
                }
            }
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

        private static string GetRepoRoot()
        {
            var projectRoot = Directory.GetParent(Application.dataPath);
            if (projectRoot == null || projectRoot.Parent == null || projectRoot.Parent.Parent == null)
            {
                return projectRoot == null ? Application.dataPath : projectRoot.FullName;
            }

            return projectRoot.Parent.Parent.FullName;
        }
    }
}

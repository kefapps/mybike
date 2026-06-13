using System;
using System.Collections.Generic;
using System.IO;
using MYB73;
using MYB89;
using MYB89.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace MYB79.Editor
{
    public static class MYB79WelcomeScreenValidator
    {
        private const string ScreenshotRelativePath = "_bmad-output/unity-test-results/myb-79-welcome-screen.png";
        private const string ReportRelativePath = "_bmad-output/unity-test-results/myb-79-welcome-screen-validator.txt";

        [MenuItem("Tools/MYB-79/Validate Welcome Screen")]
        public static string ValidateWelcomeScreenCli()
        {
            MYB89ProbeBuilder.BuildScene();

            var failures = new List<string>();
            var ride = UnityEngine.Object.FindAnyObjectByType<MYB89ProbeRide>();
            var preview = UnityEngine.Object.FindAnyObjectByType<MYB73RoutePreviewPanel>();
            var camera = Camera.main;

            MYB73RoutePreviewSnapshot snapshot = default;
            string visibleCopy = string.Empty;
            string ctaCopy = string.Empty;

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
                failures.Add("Missing Main Camera for welcome screen proof.");
            }

            if (preview != null)
            {
                preview.ShowPreview();
                Canvas.ForceUpdateCanvases();
                snapshot = preview.CaptureSnapshot();
                visibleCopy = CollectSceneVisibleCopy();
                ctaCopy = CollectButtonCopy(preview.launchButton);

                if (!preview.IsVisible)
                {
                    failures.Add("Welcome screen should be visible before launch.");
                }

                if (preview.titleLabel == null || !preview.titleLabel.text.Contains("Echappee"))
                {
                    failures.Add("Welcome screen should sell the balade with an Echappee title.");
                }

                if (preview.subtitleLabel == null || !preview.subtitleLabel.text.Contains("balade"))
                {
                    failures.Add("Welcome screen subtitle should present the balade, not a debug state.");
                }

                if (preview.statsLabel == null
                    || !preview.statsLabel.text.Contains("m")
                    || !preview.statsLabel.text.Contains("env.")
                    || !preview.statsLabel.text.Contains("difficulte"))
                {
                    failures.Add("Welcome screen stats should show compact distance, estimated duration, and difficulty.");
                }

                if (preview.passagesLabel == null || !preview.passagesLabel.text.Contains("Passages"))
                {
                    failures.Add("Welcome screen should expose Passages.");
                }

                if (snapshot.PassageCount != 3)
                {
                    failures.Add($"Welcome screen should expose exactly 3 Passages, got {snapshot.PassageCount}.");
                }

                if (!snapshot.PassagesText.Contains("Foret")
                    || !snapshot.PassagesText.Contains("Montee")
                    || !snapshot.PassagesText.Contains("Sprint"))
                {
                    failures.Add("Welcome screen Passages should mix scenic language and light effort hints.");
                }

                if (!string.Equals(ctaCopy.Trim(), "Commencer la balade", StringComparison.Ordinal))
                {
                    failures.Add("Welcome screen CTA should be 'Commencer la balade'.");
                }

                var visibleCopyLower = visibleCopy.ToLowerInvariant();
                if (visibleCopyLower.Contains("mock")
                    || visibleCopyLower.Contains("unity")
                    || visibleCopyLower.Contains("debug")
                    || visibleCopyLower.Contains("prototype")
                    || visibleCopyLower.Contains("moments cles")
                    || visibleCopyLower.Contains("lancer le ride"))
                {
                    failures.Add("Welcome screen visible copy should not expose debug/mock/Unity/prototype or old wording.");
                }
            }

            if (ride != null)
            {
                if (!ride.IsRoutePreviewWaiting)
                {
                    failures.Add("Ride should wait on the welcome screen before launch.");
                }

                if (ride.autoplay)
                {
                    failures.Add("Ride autoplay should be disabled while the welcome screen is visible.");
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
                failures.Add("Welcome screen should hide after launch.");
            }

            if (ride != null)
            {
                if (ride.IsRoutePreviewWaiting)
                {
                    failures.Add("Ride should stop waiting after welcome screen launch.");
                }

                if (!ride.autoplay)
                {
                    failures.Add("Ride autoplay should resume after welcome screen launch.");
                }
            }

            var report = WriteReport(failures, snapshot, visibleCopy, ctaCopy, preview, ride);
            if (failures.Count > 0)
            {
                throw new InvalidOperationException("MYB-79 welcome screen validation failed. See " + report);
            }

            Debug.Log("MYB-79 welcome screen validation passed. Report: " + report);
            return report;
        }

        private static string CollectSceneVisibleCopy()
        {
            var labels = UnityEngine.Object.FindObjectsByType<Text>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            var lines = new List<string>();
            foreach (var label in labels)
            {
                if (label != null && !string.IsNullOrWhiteSpace(label.text))
                {
                    lines.Add(label.text.Trim());
                }
            }

            return string.Join(" ", lines);
        }

        private static string CollectButtonCopy(Button button)
        {
            if (button == null)
            {
                return string.Empty;
            }

            var label = button.GetComponentInChildren<Text>(true);
            return label == null ? string.Empty : label.text;
        }

        private static string WriteReport(
            IReadOnlyList<string> failures,
            MYB73RoutePreviewSnapshot snapshot,
            string visibleCopy,
            string ctaCopy,
            MYB73RoutePreviewPanel preview,
            MYB89ProbeRide ride)
        {
            var repoRoot = GetRepoRoot();
            var reportPath = Path.Combine(repoRoot, ReportRelativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(reportPath) ?? repoRoot);

            using (var writer = new StreamWriter(reportPath, false))
            {
                writer.WriteLine("MYB-79 Unity welcome screen validation");
                writer.WriteLine("Timestamp UTC: " + DateTime.UtcNow.ToString("O"));
                writer.WriteLine("Unity: " + Application.unityVersion);
                writer.WriteLine("Status: " + (failures.Count == 0 ? "PASS" : "FAIL"));
                writer.WriteLine("Screenshot: " + ScreenshotRelativePath);
                writer.WriteLine("Route length: " + snapshot.RouteLengthMeters.ToString("0.0") + " m");
                writer.WriteLine("Estimated duration: " + snapshot.EstimatedDurationSeconds.ToString("0.0") + " s");
                writer.WriteLine("Difficulty: " + snapshot.OverallDifficulty);
                writer.WriteLine("Biomes: " + snapshot.BiomesText);
                writer.WriteLine("Passages: " + snapshot.PassagesText);
                writer.WriteLine("Passage count: " + snapshot.PassageCount);
                writer.WriteLine("CTA: " + ctaCopy);
                writer.WriteLine("Visible copy: " + visibleCopy);
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

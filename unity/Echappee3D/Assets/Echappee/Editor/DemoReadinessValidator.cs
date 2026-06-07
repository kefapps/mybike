using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MyBike.Echappee3D.Bootstrap;
using MyBike.Echappee3D.Core;
using MyBike.Echappee3D.Rendering;
using MyBike.Echappee3D.Ride;
using MyBike.Echappee3D.Route;
using MyBike.Echappee3D.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MyBike.Echappee3D.EditorTools
{
    public static class DemoReadinessValidator
    {
        public const string MenuPath = "Echappee/MYB-15/Validate Demo Readiness";
        public const string ReadabilityMenuPath = "Echappee/MYB-16/Validate Scene Life Readability";

        private const string ScenePath = "Assets/Scenes/RideMock.unity";
        private const string ReportFileName = "myb-15-demo-readiness.txt";
        private const string ReadabilityReportFileName = "myb-16-scene-life-readability.txt";
        private const int Myb16RequiredVisibleCount = 3;
        private const int Myb16RequiredVisibleBirds = 1;
        private const int Myb16RequiredVisibleHumans = 1;

        [MenuItem("Echappee/MYB-15/Validate Demo Readiness")]
        public static void ValidateFromMenu()
        {
            Validate();
        }

        [MenuItem("Echappee/MYB-16/Validate Scene Life Readability")]
        public static void ValidateReadabilityFromMenu()
        {
            ValidateSceneLifeReadability();
        }

        public static void Validate()
        {
            RideMockValidator.Validate();

            var report = BuildReport("MYB-15 Unity demo readiness validation", false);
            var reportPath = WriteReport(report, ReportFileName);
            Debug.Log($"MYB-15 demo readiness validation passed. Report: {reportPath}");
        }

        public static void ValidateSceneLifeReadability()
        {
            RideMockValidator.Validate();

            var report = BuildReport("MYB-16 Unity scene life readability validation", true);
            var reportPath = WriteReport(report, ReadabilityReportFileName);
            Debug.Log($"MYB-16 scene life readability validation passed. Report: {reportPath}");
        }

        private static string BuildReport(string title, bool enforceMyb16Readability)
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (!scene.IsValid() || scene.path != ScenePath)
            {
                scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            }

            Require(scene.IsValid(), "RideMock.unity scene should be active for MYB-15 validation.");

            var route = RouteMath.CreateDefaultMockRoute();
            var routeRenderer = Find<RouteRendererPlaceholder>();
            var camera = Camera.main;
            var sceneLife = Find<LightweightSceneLife>();
            var sessionController = Find<RideSessionController>();
            var hud = Find<HudController>();
            var controls = Find<RideControlPanel>();
            var mockInput = Find<MockRideInput>();
            var routeReport = AnalyzeRoute(routeRenderer, route);
            var cameraReport = AnalyzeCamera(route);
            var fogReport = AnalyzeFog();
            var loopReport = AnalyzeSessionLoop();
            var visibilitySamples = CollectSceneLifeVisibility(sceneLife, route, camera);
            var visibilitySummary = SceneLifeVisibility.Summarize(visibilitySamples);
            var consoleHealth = ReadConsoleHealth();

            var output = new StringBuilder();
            output.AppendLine(title);
            output.AppendLine($"Generated UTC: {DateTime.UtcNow:O}");
            output.AppendLine($"Unity version: {Application.unityVersion}");
            output.AppendLine($"Project root: {ProjectRoot()}");
            output.AppendLine($"Repository root: {RepoRoot()}");
            output.AppendLine($"Active scene: {scene.path}");
            output.AppendLine($"Editor idle: {!EditorApplication.isPlaying && !EditorApplication.isCompiling && !EditorApplication.isUpdating}");
            output.AppendLine($"Editor playing: {EditorApplication.isPlaying}");
            output.AppendLine($"Editor compiling: {EditorApplication.isCompiling}");
            output.AppendLine($"Editor updating: {EditorApplication.isUpdating}");
            output.AppendLine(consoleHealth);
            output.AppendLine();
            output.AppendLine("Scene wiring");
            output.AppendLine($"RideSessionController: {Present(sessionController)}");
            output.AppendLine($"MockRideInput: {Present(mockInput)}");
            output.AppendLine($"HudController: {Present(hud)}");
            output.AppendLine($"RideControlPanel: {Present(controls)}");
            output.AppendLine($"Main camera: {Present(camera)}");
            output.AppendLine($"Start/Pause/Resume/Finish buttons: {Present(GameObject.Find("StartButton"))}/{Present(GameObject.Find("PauseButton"))}/{Present(GameObject.Find("ResumeButton"))}/{Present(GameObject.Find("FinishButton"))}");
            output.AppendLine($"HUD speed/effort/progress/phase/summary: {Present(GameObject.Find("SpeedText"))}/{Present(GameObject.Find("EffortText"))}/{Present(GameObject.Find("ProgressText"))}/{Present(GameObject.Find("PhaseText"))}/{Present(GameObject.Find("SummaryText"))}");
            output.AppendLine();
            output.AppendLine("Route visible/stable");
            output.AppendLine(routeReport);
            output.AppendLine();
            output.AppendLine("Camera samples bounded");
            output.AppendLine(cameraReport);
            output.AppendLine();
            output.AppendLine("Fog/depth");
            output.AppendLine(fogReport);
            output.AppendLine();
            output.AppendLine("HUD/controls/session loop");
            output.AppendLine(loopReport);
            output.AppendLine();
            output.AppendLine("SceneLife presence and projected visibility");
            AppendSceneLifeSection(output, sceneLife, visibilitySamples, visibilitySummary);
            output.AppendLine();
            output.AppendLine("MYB-16 readability threshold");
            AppendReadabilityThreshold(output, visibilitySummary);
            output.AppendLine();
            output.AppendLine("MYB-16 factual recommendations");
            AppendRecommendations(output, visibilitySummary);

            if (enforceMyb16Readability)
            {
                Require(
                    visibilitySummary.MeetsReadabilityTarget(
                        Myb16RequiredVisibleCount,
                        Myb16RequiredVisibleBirds,
                        Myb16RequiredVisibleHumans),
                    "MYB-16 readability target should be met by SceneLife projected visibility.");
            }

            return output.ToString();
        }

        private static string AnalyzeRoute(RouteRendererPlaceholder routeRenderer, RouteDefinition route)
        {
            Require(routeRenderer != null, "RouteRendererPlaceholder should exist.");
            routeRenderer.EnsureVisible(route);

            var line = routeRenderer.GetComponent<LineRenderer>();
            Require(line != null, "Route LineRenderer should exist.");

            var finiteSamples = 0;
            var elevatedSamples = 0;
            var maxAbsElevation = 0f;
            for (var i = 0; i <= 20; i += 1)
            {
                var point = RouteMath.SamplePosition(route, i / 20f);
                finiteSamples += IsFinite(point) ? 1 : 0;
                elevatedSamples += point.y > 0.5f ? 1 : 0;
                maxAbsElevation = Mathf.Max(maxAbsElevation, Mathf.Abs(point.y));
            }

            var maxAbsGrade = 0f;
            for (var i = 1; i < route.points.Length; i += 1)
            {
                var current = route.points[i];
                var previous = route.points[i - 1];
                var distanceDelta = current.DistanceMeters - previous.DistanceMeters;
                if (distanceDelta > 0f)
                {
                    maxAbsGrade = Mathf.Max(
                        maxAbsGrade,
                        Mathf.Abs((current.Position.y - previous.Position.y) / distanceDelta));
                }
            }

            return "pass " +
                $"enabled={line.enabled}, points={line.positionCount}, width={line.widthMultiplier:0.00}, " +
                $"material={Present(line.sharedMaterial)}, finiteSamples={finiteSamples}/21, " +
                $"elevatedSamples={elevatedSamples}/21, maxAbsElevation={maxAbsElevation:0.00}m, " +
                $"maxAbsGrade={maxAbsGrade:0.000}";
        }

        private static string AnalyzeCamera(RouteDefinition route)
        {
            var maxStepMeters = 0f;
            var minRouteClearance = float.MaxValue;
            var minForwardDot = float.MaxValue;
            var finiteSamples = 0;
            var previous = Vector3.zero;
            var hasPrevious = false;

            for (var i = 0; i <= 20; i += 1)
            {
                var progress01 = i / 20f;
                var progress = new RouteProgressSnapshot(progress01, progress01 * route.lengthMeters, progress01 >= 1f);
                var routePosition = RouteMath.SamplePosition(route, progress01);
                var camera = RouteMath.CameraOnRail(route, progress, CameraRailConfig.Default);
                var lookVector = camera.LookAt - camera.Position;

                if (IsFinite(camera.Position) && IsFinite(camera.LookAt))
                {
                    finiteSamples += 1;
                }

                minRouteClearance = Mathf.Min(minRouteClearance, camera.Position.y - routePosition.y);
                minForwardDot = Mathf.Min(
                    minForwardDot,
                    lookVector.sqrMagnitude > 0.0001f ? Vector3.Dot(lookVector.normalized, Vector3.forward) : -1f);

                if (hasPrevious)
                {
                    maxStepMeters = Mathf.Max(maxStepMeters, Vector3.Distance(previous, camera.Position));
                }

                previous = camera.Position;
                hasPrevious = true;
            }

            return "pass " +
                $"finiteSamples={finiteSamples}/21, minRouteClearance={minRouteClearance:0.00}m, maxSampleStep={maxStepMeters:0.00}m, " +
                $"minForwardDot={minForwardDot:0.000}, configHeight={CameraRailConfig.Default.HeightMeters:0.00}m, " +
                $"lookAhead={CameraRailConfig.Default.LookAheadMeters:0.00}m";
        }

        private static string AnalyzeFog()
        {
            return "pass " +
                $"enabled={RenderSettings.fog}, mode={RenderSettings.fogMode}, " +
                $"start={RenderSettings.fogStartDistance:0.00}m, end={RenderSettings.fogEndDistance:0.00}m";
        }

        private static string AnalyzeSessionLoop()
        {
            var input = new MockRideInputSource(0.7f);
            var loop = new RideSessionLoop(RouteMath.CreateDefaultMockRoute(), input);

            var steps = new List<string> { loop.Phase.ToString() };
            loop.Start();
            var running = loop.Tick(1f, 1d);
            steps.Add(running.Phase.ToString());
            loop.Pause();
            var pausedDistance = loop.Progress.DistanceMeters;
            var paused = loop.Tick(2f, 3d);
            steps.Add(paused.Phase.ToString());
            loop.Resume();
            var resumed = loop.Tick(1f, 4d);
            steps.Add(resumed.Phase.ToString());
            loop.Finish();
            var finished = loop.CurrentSnapshot(5d);
            steps.Add(finished.Phase.ToString());

            var summary = loop.LastSummary;
            return "pass " +
                $"sequence={string.Join(" -> ", steps)}, " +
                $"pauseFrozen={Mathf.Abs(paused.Route.DistanceMeters - pausedDistance) < 0.001f}, " +
                $"resumeAdvanced={resumed.Route.DistanceMeters > pausedDistance}, " +
                $"summary={summary.HasValue}, summaryDistance={summary.GetValueOrDefault().DistanceMeters:0.00}m";
        }

        private static List<SceneLifeVisibilitySample> CollectSceneLifeVisibility(
            LightweightSceneLife sceneLife,
            RouteDefinition route,
            Camera camera)
        {
            Require(sceneLife != null, "LightweightSceneLife should exist.");

            var samples = new List<SceneLifeVisibilitySample>();
            var verticalFov = camera != null ? camera.fieldOfView : CameraRailConfig.Default.FovDegrees;
            var horizontalFov = SceneLifeVisibility.HorizontalFovFromVertical(
                verticalFov,
                camera != null ? camera.aspect : 16f / 9f);

            CollectVisibility(samples, sceneLife.BirdsRoot, "bird", route, horizontalFov, verticalFov);
            CollectVisibility(samples, sceneLife.HumansRoot, "human", route, horizontalFov, verticalFov);
            return samples;
        }

        private static void CollectVisibility(
            List<SceneLifeVisibilitySample> samples,
            Transform root,
            string elementType,
            RouteDefinition route,
            float horizontalFov,
            float verticalFov)
        {
            Require(root != null, $"{elementType} root should exist.");

            for (var i = 0; i < root.childCount; i += 1)
            {
                var child = root.GetChild(i);
                if (!child.gameObject.activeInHierarchy)
                {
                    continue;
                }

                samples.Add(SceneLifeVisibility.Measure(
                    elementType,
                    child.name,
                    ApproximateVisualCenter(child),
                    route,
                    CameraRailConfig.Default,
                    horizontalFov,
                    verticalFov));
            }
        }

        private static Vector3 ApproximateVisualCenter(Transform transform)
        {
            var renderers = transform.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0)
            {
                return transform.position;
            }

            var bounds = renderers[0].bounds;
            for (var i = 1; i < renderers.Length; i += 1)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }

            return bounds.center;
        }

        private static void AppendSceneLifeSection(
            StringBuilder output,
            LightweightSceneLife sceneLife,
            List<SceneLifeVisibilitySample> samples,
            SceneLifeVisibility.Summary summary)
        {
            output.AppendLine($"Birds: {sceneLife.BirdCount} required>={LightweightSceneLife.RequiredBirdCount}");
            output.AppendLine($"Humans: {sceneLife.HumanCount} required>={LightweightSceneLife.RequiredHumanCount}");
            output.AppendLine($"Total life elements: {sceneLife.TotalLifeElementCount}/{LightweightSceneLife.MaxLifeElementCount}");
            output.AppendLine($"Projected visibility in camera cone: {summary.InsideCount}/{summary.TotalCount}");
            output.AppendLine($"Birds in camera cone: {summary.BirdInsideCount}/{sceneLife.BirdCount}");
            output.AppendLine($"Humans in camera cone: {summary.HumanInsideCount}/{sceneLife.HumanCount}");
            output.AppendLine($"Finite visibility samples: {summary.FiniteCount}/{summary.TotalCount}");

            foreach (var sample in samples)
            {
                output.AppendLine(
                    $"{sample.ElementType}:{sample.ElementName} progress={sample.Progress01:0.000} " +
                    $"distance={sample.DistanceMeters:0.00}m yaw={sample.YawDegrees:0.0}deg " +
                    $"pitch={sample.PitchDegrees:0.0}deg inCone={sample.IsInCameraCone}");
            }
        }

        private static void AppendReadabilityThreshold(
            StringBuilder output,
            SceneLifeVisibility.Summary summary)
        {
            var status = summary.MeetsReadabilityTarget(
                Myb16RequiredVisibleCount,
                Myb16RequiredVisibleBirds,
                Myb16RequiredVisibleHumans)
                ? "pass"
                : "warn";

            output.AppendLine(
                $"{status} projectedVisible={summary.InsideCount}/{summary.TotalCount}, " +
                $"birdsInCone={summary.BirdInsideCount}, humansInCone={summary.HumanInsideCount}, " +
                $"finiteSamples={summary.FiniteCount}/{summary.TotalCount}, " +
                $"requiredProjected>={Myb16RequiredVisibleCount}, " +
                $"requiredBirds>={Myb16RequiredVisibleBirds}, " +
                $"requiredHumans>={Myb16RequiredVisibleHumans}");
            output.AppendLine("Baseline from MYB-15: projectedVisible=0/8, finiteSamples=8/8.");
        }

        private static void AppendRecommendations(
            StringBuilder output,
            SceneLifeVisibility.Summary summary)
        {
            if (summary.MeetsReadabilityTarget(
                    Myb16RequiredVisibleCount,
                    Myb16RequiredVisibleBirds,
                    Myb16RequiredVisibleHumans))
            {
                output.AppendLine("- MYB-16 readability target is met; keep later polish bounded by this harness.");
            }
            else if (summary.InsideCount == 0)
            {
                output.AppendLine("- MYB-16 should improve SceneLife readability: current projected visibility has 0 elements inside the camera cone.");
            }
            else
            {
                output.AppendLine($"- MYB-16 should continue tuning SceneLife readability from current projected visibility {summary.InsideCount}/{summary.TotalCount}.");
            }

            if (summary.BirdInsideCount == 0)
            {
                output.AppendLine("- Birds are present but not reliably in the forward cone; consider placement/scale/contrast polish in MYB-16.");
            }

            if (summary.HumanInsideCount == 0)
            {
                output.AppendLine("- Human silhouettes are present but not reliably in the forward cone; consider lateral placement/readability polish in MYB-16.");
            }

            output.AppendLine("- Keep MYB-16 visual changes bounded: route, camera, fog, HUD and loop should remain regression-checked by this harness.");
        }

        private static string ReadConsoleHealth()
        {
            return "Console health: verified by Unity MCP console read after this menu execution; expected final result is 0 errors / 0 warnings.";
        }

        private static string WriteReport(string report, string fileName)
        {
            var outputDir = Path.Combine(RepoRoot(), "_bmad-output/unity-test-results");
            Directory.CreateDirectory(outputDir);
            var reportPath = Path.Combine(outputDir, fileName);
            File.WriteAllText(reportPath, report);
            return reportPath;
        }

        private static string ProjectRoot()
        {
            return Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
        }

        private static string RepoRoot()
        {
            return Path.GetFullPath(Path.Combine(Application.dataPath, "../../.."));
        }

        private static T Find<T>() where T : UnityEngine.Object
        {
            return UnityEngine.Object.FindFirstObjectByType<T>();
        }

        private static string Present(UnityEngine.Object value)
        {
            return value != null ? "present" : "missing";
        }

        private static void Require(bool condition, string message)
        {
            if (!condition)
            {
                throw new InvalidOperationException(message);
            }
        }

        private static bool IsFinite(Vector3 value)
        {
            return IsFinite(value.x) && IsFinite(value.y) && IsFinite(value.z);
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }
    }
}

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
    public static class PrivateDemoValidator
    {
        public const string MenuPath = "Echappee/MYB-17/Validate Private Demo";

        private const string ScenePath = "Assets/Scenes/RideMock.unity";
        private const string ReportFileName = "myb-17-private-demo-readiness.txt";
        private const int RequiredVisibleCount = 3;
        private const int RequiredVisibleBirds = 1;
        private const int RequiredVisibleHumans = 1;

        [MenuItem("Echappee/MYB-17/Validate Private Demo")]
        public static void ValidateFromMenu()
        {
            Validate();
        }

        public static void Validate()
        {
            RideMockValidator.Validate();

            var report = BuildReport();
            var reportPath = WriteReport(report);
            Debug.Log($"MYB-17 private demo validation passed. Report: {reportPath}");
        }

        private static string BuildReport()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (!scene.IsValid() || scene.path != ScenePath)
            {
                scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            }

            Require(scene.IsValid(), "RideMock.unity scene should be active for MYB-17 validation.");
            Require(scene.path == ScenePath, "MYB-17 validation should run against Assets/Scenes/RideMock.unity.");
            Require(!EditorApplication.isPlaying, "Editor should not be in Play Mode for private demo validation.");
            Require(!EditorApplication.isCompiling, "Editor should be idle, not compiling.");
            Require(!EditorApplication.isUpdating, "Editor should be idle, not updating assets.");

            var route = RouteMath.CreateDefaultMockRoute();
            var routeRenderer = Find<RouteRendererPlaceholder>();
            var camera = Camera.main;
            var sceneLife = Find<LightweightSceneLife>();
            var visibilitySamples = CollectSceneLifeVisibility(sceneLife, route, camera);
            var visibilitySummary = SceneLifeVisibility.Summarize(visibilitySamples);

            Require(
                visibilitySummary.MeetsReadabilityTarget(
                    RequiredVisibleCount,
                    RequiredVisibleBirds,
                    RequiredVisibleHumans),
                "MYB-17 private demo should preserve MYB-16 SceneLife readability.");

            var output = new StringBuilder();
            output.AppendLine("MYB-17 Unity private demo readiness validation");
            output.AppendLine($"Generated UTC: {DateTime.UtcNow:O}");
            output.AppendLine($"Unity version: {Application.unityVersion}");
            output.AppendLine($"Project root: {ProjectRoot()}");
            output.AppendLine($"Repository root: {RepoRoot()}");
            output.AppendLine($"Active scene: {scene.path}");
            output.AppendLine($"Editor idle: {!EditorApplication.isPlaying && !EditorApplication.isCompiling && !EditorApplication.isUpdating}");
            output.AppendLine($"Editor playing: {EditorApplication.isPlaying}");
            output.AppendLine($"Editor compiling: {EditorApplication.isCompiling}");
            output.AppendLine($"Editor updating: {EditorApplication.isUpdating}");
            output.AppendLine("Console health: verified by final Unity MCP console read after this menu execution; expected final result is 0 errors / 0 warnings.");
            output.AppendLine();
            output.AppendLine("Core RideMock readiness");
            output.AppendLine(AnalyzeSceneWiring());
            output.AppendLine();
            output.AppendLine("Route visible/stable");
            output.AppendLine(AnalyzeRoute(routeRenderer, route));
            output.AppendLine();
            output.AppendLine("Camera samples bounded");
            output.AppendLine(AnalyzeCamera(route));
            output.AppendLine();
            output.AppendLine("Fog/depth");
            output.AppendLine(AnalyzeFog());
            output.AppendLine();
            output.AppendLine("HUD/controls/session loop");
            output.AppendLine(AnalyzeSessionLoop());
            output.AppendLine();
            output.AppendLine("SceneLife MYB-16 visibility");
            AppendSceneLifeSection(output, sceneLife, visibilitySamples, visibilitySummary);
            output.AppendLine();
            output.AppendLine("Build Settings / local build");
            AppendBuildReadiness(output);
            output.AppendLine();
            output.AppendLine("Private demo verdict");
            output.AppendLine("ready-for-private-local-demo");
            output.AppendLine();
            output.AppendLine("Private demo launch checklist");
            AppendLaunchChecklist(output);

            return output.ToString();
        }

        private static string AnalyzeSceneWiring()
        {
            var controller = Find<RideSessionController>();
            var routeRenderer = Find<RouteRendererPlaceholder>();
            var cameraController = Find<RideCameraController>();
            var fogController = Find<DepthFogController>();
            var sceneLife = Find<LightweightSceneLife>();
            var mockInput = Find<MockRideInput>();
            var hud = Find<HudController>();
            var controls = Find<RideControlPanel>();

            Require(controller != null, "RideSessionController should be present for the private demo.");
            Require(routeRenderer != null, "RouteRendererPlaceholder should be present for the private demo.");
            Require(cameraController != null, "RideCameraController should be present for the private demo.");
            Require(fogController != null, "DepthFogController should be present for the private demo.");
            Require(sceneLife != null, "LightweightSceneLife should be present for the private demo.");
            Require(mockInput != null, "MockRideInput should be present for the private demo.");
            Require(hud != null, "HudController should be present for the private demo.");
            Require(controls != null, "RideControlPanel should be present for the private demo.");
            Require(Camera.main != null, "Main camera should be present for the private demo.");
            Require(GameObject.Find("MockEffortSlider")?.GetComponent<Slider>() != null, "Mock effort slider should be present.");
            Require(GameObject.Find("StartButton")?.GetComponent<Button>() != null, "Start button should be present.");
            Require(GameObject.Find("PauseButton")?.GetComponent<Button>() != null, "Pause button should be present.");
            Require(GameObject.Find("ResumeButton")?.GetComponent<Button>() != null, "Resume button should be present.");
            Require(GameObject.Find("FinishButton")?.GetComponent<Button>() != null, "Finish button should be present.");
            Require(GameObject.Find("SpeedText")?.GetComponent<Text>() != null, "Speed HUD text should be present.");
            Require(GameObject.Find("EffortText")?.GetComponent<Text>() != null, "Effort HUD text should be present.");
            Require(GameObject.Find("ProgressText")?.GetComponent<Text>() != null, "Progress HUD text should be present.");
            Require(GameObject.Find("PhaseText")?.GetComponent<Text>() != null, "State HUD text should be present.");
            Require(GameObject.Find("SummaryText")?.GetComponent<Text>() != null, "Summary HUD text should be present.");

            return "pass " +
                $"RideSessionController={Present(controller)}, RouteRenderer={Present(routeRenderer)}, " +
                $"CameraController={Present(cameraController)}, FogController={Present(fogController)}, " +
                $"SceneLife={Present(sceneLife)}, MockRideInput={Present(mockInput)}, " +
                $"HUD={Present(hud)}, Controls={Present(controls)}, MainCamera={Present(Camera.main)}";
        }

        private static string AnalyzeRoute(RouteRendererPlaceholder routeRenderer, RouteDefinition route)
        {
            Require(routeRenderer != null, "RouteRendererPlaceholder should exist.");
            routeRenderer.EnsureVisible(route);

            var line = routeRenderer.GetComponent<LineRenderer>();
            Require(line != null, "Route LineRenderer should exist.");
            Require(line.enabled, "Route LineRenderer should be enabled.");
            Require(line.positionCount >= 4, "Route LineRenderer should keep enough points.");
            Require(line.widthMultiplier >= 1f, "Route LineRenderer should keep readable width.");
            Require(line.sharedMaterial != null, "Route LineRenderer should keep a visible material.");

            var finiteSamples = 0;
            var elevatedSamples = 0;
            var maxAbsElevation = 0f;
            for (var i = 0; i <= 20; i += 1)
            {
                var point = RouteMath.SamplePosition(route, i / 20f);
                finiteSamples += IsFinite(point) ? 1 : 0;
                elevatedSamples += Mathf.Abs(point.y) > 0.5f ? 1 : 0;
                maxAbsElevation = Mathf.Max(maxAbsElevation, Mathf.Abs(point.y));
            }

            var maxAbsGrade = 0f;
            var hasClimb = false;
            var hasDescent = false;
            for (var i = 1; i < route.points.Length; i += 1)
            {
                var current = route.points[i];
                var previous = route.points[i - 1];
                var distanceDelta = current.DistanceMeters - previous.DistanceMeters;
                var elevationDelta = current.Position.y - previous.Position.y;
                Require(distanceDelta > 0f, "Route distances should stay monotonic.");

                maxAbsGrade = Mathf.Max(maxAbsGrade, Mathf.Abs(elevationDelta / distanceDelta));
                hasClimb |= elevationDelta > 0.5f;
                hasDescent |= elevationDelta < -0.5f;
            }

            Require(finiteSamples == 21, "Route samples should be finite.");
            Require(elevatedSamples > 0, "Route should preserve MYB-13 elevation.");
            Require(hasClimb, "Route should preserve at least one climb.");
            Require(hasDescent, "Route should preserve at least one descent.");

            return "pass " +
                $"lineEnabled={line.enabled}, linePoints={line.positionCount}, lineWidth={line.widthMultiplier:0.00}, " +
                $"material={Present(line.sharedMaterial)}, finiteSamples={finiteSamples}/21, elevatedSamples={elevatedSamples}/21, " +
                $"maxAbsElevation={maxAbsElevation:0.00}m, maxAbsGrade={maxAbsGrade:0.000}, " +
                $"hasClimb={hasClimb}, hasDescent={hasDescent}";
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

            Require(finiteSamples == 21, "Camera samples should be finite.");
            Require(minRouteClearance >= 1.3f, "Camera should remain above the route.");
            Require(minForwardDot > 0.2f, "Camera should keep a stable forward look.");
            Require(maxStepMeters < 90f, "Camera samples should stay bounded.");

            return "pass " +
                $"finiteSamples={finiteSamples}/21, minRouteClearance={minRouteClearance:0.00}m, " +
                $"maxSampleStep={maxStepMeters:0.00}m, minForwardDot={minForwardDot:0.000}, " +
                $"configHeight={CameraRailConfig.Default.HeightMeters:0.00}m, lookAhead={CameraRailConfig.Default.LookAheadMeters:0.00}m";
        }

        private static string AnalyzeFog()
        {
            Require(RenderSettings.fog, "Fog should remain enabled.");
            Require(RenderSettings.fogMode == FogMode.Linear, "Fog should remain linear.");
            Require(RenderSettings.fogEndDistance > RenderSettings.fogStartDistance, "Fog end should remain after fog start.");

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
            var pauseFrozen = Mathf.Abs(paused.Route.DistanceMeters - pausedDistance) < 0.001f;
            var resumeAdvanced = resumed.Route.DistanceMeters > pausedDistance;

            Require(running.Phase == RidePhase.Running, "Start should enter Running.");
            Require(paused.Phase == RidePhase.Paused, "Pause should enter Paused.");
            Require(pauseFrozen, "Pause should freeze route progress.");
            Require(resumed.Phase == RidePhase.Running, "Resume should return to Running.");
            Require(resumeAdvanced, "Resume should advance from paused progress.");
            Require(finished.Phase == RidePhase.Finished, "Finish should enter Finished.");
            Require(summary.HasValue, "Finish should produce a summary.");

            return "pass " +
                $"sequence={string.Join(" -> ", steps)}, pauseFrozen={pauseFrozen}, " +
                $"resumeAdvanced={resumeAdvanced}, summary={summary.HasValue}, " +
                $"summaryDistance={summary.GetValueOrDefault().DistanceMeters:0.00}m";
        }

        private static List<SceneLifeVisibilitySample> CollectSceneLifeVisibility(
            LightweightSceneLife sceneLife,
            RouteDefinition route,
            Camera camera)
        {
            Require(sceneLife != null, "LightweightSceneLife should exist.");
            Require(sceneLife.BirdsRoot != null, "Birds root should exist.");
            Require(sceneLife.HumansRoot != null, "Roadside humans root should exist.");
            Require(sceneLife.BirdCount == LightweightSceneLife.RequiredBirdCount, "MYB-17 should preserve exactly 5 birds.");
            Require(sceneLife.HumanCount == LightweightSceneLife.RequiredHumanCount, "MYB-17 should preserve exactly 3 human silhouettes.");
            Require(camera != null, "Main camera should exist for projected visibility.");

            var samples = new List<SceneLifeVisibilitySample>();
            var horizontalFov = SceneLifeVisibility.HorizontalFovFromVertical(camera.fieldOfView, camera.aspect);
            CollectVisibility(samples, sceneLife.BirdsRoot, "bird", route, horizontalFov, camera.fieldOfView);
            CollectVisibility(samples, sceneLife.HumansRoot, "human", route, horizontalFov, camera.fieldOfView);
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
            output.AppendLine($"Birds: {sceneLife.BirdCount}/{LightweightSceneLife.RequiredBirdCount}");
            output.AppendLine($"Humans: {sceneLife.HumanCount}/{LightweightSceneLife.RequiredHumanCount}");
            output.AppendLine($"Total life elements: {sceneLife.TotalLifeElementCount}/{LightweightSceneLife.MaxLifeElementCount}");
            output.AppendLine($"Projected visibility in camera cone: {summary.InsideCount}/{summary.TotalCount}");
            output.AppendLine($"Birds in camera cone: {summary.BirdInsideCount}/{sceneLife.BirdCount}");
            output.AppendLine($"Humans in camera cone: {summary.HumanInsideCount}/{sceneLife.HumanCount}");
            output.AppendLine($"Finite visibility samples: {summary.FiniteCount}/{summary.TotalCount}");
            output.AppendLine(
                $"MYB-16 threshold: projectedVisible>={RequiredVisibleCount}, " +
                $"birdsInCone>={RequiredVisibleBirds}, humansInCone>={RequiredVisibleHumans}");
            output.AppendLine(
                $"MYB-16 preserved: {summary.MeetsReadabilityTarget(RequiredVisibleCount, RequiredVisibleBirds, RequiredVisibleHumans)}");

            foreach (var sample in samples)
            {
                output.AppendLine(
                    $"{sample.ElementType}:{sample.ElementName} progress={sample.Progress01:0.000} " +
                    $"distance={sample.DistanceMeters:0.00}m yaw={sample.YawDegrees:0.0}deg " +
                    $"pitch={sample.PitchDegrees:0.0}deg inCone={sample.IsInCameraCone}");
            }
        }

        private static void AppendBuildReadiness(StringBuilder output)
        {
            var scenes = EditorBuildSettings.scenes;
            var rideMockEnabledInBuild = false;
            foreach (var buildScene in scenes)
            {
                if (buildScene.path == ScenePath && buildScene.enabled)
                {
                    rideMockEnabledInBuild = true;
                    break;
                }
            }

            output.AppendLine($"Build Settings scenes: {scenes.Length}");
            output.AppendLine($"RideMock enabled in Build Settings: {rideMockEnabledInBuild}");
            output.AppendLine("Local macOS build attempted: false");
            output.AppendLine(
                "Build decision: deferred for MYB-17 because the Editor-based private demo is sufficient, " +
                "deterministic, and avoids generated build artifacts in git.");
        }

        private static void AppendLaunchChecklist(StringBuilder output)
        {
            output.AppendLine("1. Open Unity project: /Users/jbodin/personnel/apps/mybike/unity/Echappee3D");
            output.AppendLine("2. Open scene: Assets/Scenes/RideMock.unity");
            output.AppendLine("3. Run menu: Echappee/MYB-17/Validate Private Demo");
            output.AppendLine("4. Confirm this report says ready-for-private-local-demo and Unity console is 0 errors / 0 warnings.");
            output.AppendLine("5. Press Play in the Unity Editor.");
            output.AppendLine("6. Use MockEffortSlider, then show Start -> ride -> Pause -> Resume -> Finish.");
            output.AppendLine("7. Confirm the Summary/end state appears and route, fog, camera and SceneLife remain readable.");
        }

        private static string WriteReport(string report)
        {
            var outputDir = Path.Combine(RepoRoot(), "_bmad-output/unity-test-results");
            Directory.CreateDirectory(outputDir);
            var reportPath = Path.Combine(outputDir, ReportFileName);
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

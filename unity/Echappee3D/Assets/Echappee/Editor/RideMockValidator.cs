using System;
using System.IO;
using MyBike.Echappee3D.Bootstrap;
using MyBike.Echappee3D.Core;
using MyBike.Echappee3D.Rendering;
using MyBike.Echappee3D.Ride;
using MyBike.Echappee3D.Route;
using MyBike.Echappee3D.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace MyBike.Echappee3D.EditorTools
{
    public static class RideMockValidator
    {
        private const string ExpectedUnityVersion = "6000.4.10f1";
        private const string ScenePath = "Assets/Scenes/RideMock.unity";
        private const float MaxRouteGrade = 0.08f;
        private const float MaxElevationMeters = 20f;
        private const float ExpectedFogStartDistance = 45f;
        private const float ExpectedFogEndDistance = 220f;

        [MenuItem("Echappee/MYB-13/Validate RideMock Scene")]
        public static void Validate()
        {
            Require(
                Application.unityVersion == ExpectedUnityVersion,
                $"Expected Unity {ExpectedUnityVersion}, got {Application.unityVersion}");

            ValidateRideMath();
            ValidateRouteMath();
            ValidateRideSessionLoop();
            ValidateScene();
            WriteReport();
        }

        [MenuItem("Echappee/MYB-12/Validate RideMock Scene")]
        public static void ValidateLegacy()
        {
            Validate();
        }

        private static void ValidateRideMath()
        {
            var config = SpeedMappingConfig.Default;
            var smoothing = SpeedSmoothingConfig.Default;

            Require(
                Approximately(
                    RideMath.MapMockInputToSpeed(
                        new RideInputSample(RideInputSourceKind.Mock, 0f, 0),
                        config),
                    1.5f),
                "Mock effort 0 should map to min speed.");

            Require(
                Approximately(
                    RideMath.MapMockInputToSpeed(
                        new RideInputSample(RideInputSourceKind.Mock, 1f, 0),
                        config),
                    9f),
                "Mock effort 1 should map to max speed.");

            Require(
                Approximately(RideMath.SmoothSpeed(0f, 9f, 1f, smoothing), 2.5f),
                "Acceleration smoothing should use 2.5 m/s2.");

            Require(
                Approximately(RideMath.SmoothSpeed(9f, 0f, 1f, smoothing), 5f),
                "Deceleration smoothing should use 4 m/s2.");
        }

        private static void ValidateRouteMath()
        {
            var route = RouteMath.CreateDefaultMockRoute();
            var progress = RouteMath.Advance(
                new RouteProgressSnapshot(0f, 0f, false),
                10f,
                5f,
                route);
            var camera = RouteMath.CameraOnRail(route, progress, CameraRailConfig.Default);

            Require(Approximately(progress.DistanceMeters, 50f), "Route progress should advance distance.");
            Require(IsFinite(camera.Position) && IsFinite(camera.LookAt), "Camera snapshot must be finite.");
            Require(RouteMath.SelectBiome(route, 0f) == "coast", "Start biome should be coast.");
            Require(RouteMath.SelectBiome(route, 0.5f) == "forest", "Mid-route biome should be forest.");
            ValidateRouteElevation(route);
            ValidateCameraSamples(route);

            Require(
                RouteMath.Resolve(null).id == "placeholder-route",
                "Invalid route should resolve to deterministic placeholder.");
        }

        private static void ValidateRideSessionLoop()
        {
            var input = new MockRideInputSource(0.7f);
            var loop = new RideSessionLoop(RouteMath.CreateDefaultMockRoute(), input);

            Require(loop.Phase == RidePhase.Idle, "Loop should start idle.");
            Require(Approximately(loop.Progress.Progress01, 0f), "Initial progress should be zero.");

            loop.Start();
            var running = loop.Tick(1f, 1d);
            Require(running.Phase == RidePhase.Running, "Start should enter Running.");
            Require(running.SpeedMps > 0f, "Running tick should update speed.");
            Require(running.Route.DistanceMeters > 0f, "Running tick should advance progress.");

            loop.Pause();
            var pausedDistance = loop.Progress.DistanceMeters;
            var paused = loop.Tick(5f, 6d);
            Require(paused.Phase == RidePhase.Paused, "Pause should enter Paused.");
            Require(Approximately(paused.Route.DistanceMeters, pausedDistance), "Paused tick should freeze progress.");

            loop.Resume();
            var resumed = loop.Tick(1f, 7d);
            Require(resumed.Phase == RidePhase.Running, "Resume should re-enter Running.");
            Require(resumed.Route.DistanceMeters > pausedDistance, "Resume should continue from current progress.");

            loop.Finish();
            var finished = loop.CurrentSnapshot(8d);
            Require(finished.Phase == RidePhase.Finished, "Finish should enter Finished.");
            Require(loop.LastSummary.HasValue, "Finish should produce a summary.");
            Require(loop.LastSummary.Value.DistanceMeters > 0f, "Summary should include distance.");
            Require(loop.LastSummary.Value.AverageSpeedMps > 0f, "Summary should include average speed.");
        }

        private static void ValidateScene()
        {
            Require(
                AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) != null,
                "RideMock.unity scene asset is missing.");

            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            Require(scene.IsValid(), "RideMock.unity did not open.");
            ValidateFogSettings();

            var controller = Find<RideSessionController>();
            var routeRenderer = Find<RouteRendererPlaceholder>();
            var cameraController = Find<RideCameraController>();
            var fogController = Find<DepthFogController>();
            var mockInput = Find<MockRideInput>();
            var hud = Find<HudController>();
            var controls = Find<RideControlPanel>();

            Require(controller != null, "RideSessionController is missing.");
            Require(routeRenderer != null, "RouteRendererPlaceholder is missing.");
            Require(cameraController != null, "RideCameraController is missing.");
            Require(fogController != null, "DepthFogController is missing.");
            Require(mockInput != null, "MockRideInput is missing.");
            Require(hud != null, "HudController is missing.");
            Require(controls != null, "RideControlPanel is missing.");
            Require(Camera.main != null, "Main camera is missing.");

            Require(GameObject.Find("Route") != null, "Route root is missing.");
            routeRenderer.EnsureVisible(RouteMath.CreateDefaultMockRoute());
            ValidateRouteRenderer(routeRenderer);
            var effortSlider = RequireComponent<Slider>("MockEffortSlider", "Mock effort slider is missing.");
            var startButton = RequireComponent<Button>("StartButton", "Start button is missing.");
            var pauseButton = RequireComponent<Button>("PauseButton", "Pause button is missing.");
            var resumeButton = RequireComponent<Button>("ResumeButton", "Resume button is missing.");
            var finishButton = RequireComponent<Button>("FinishButton", "Finish button is missing.");
            var speedText = RequireComponent<Text>("SpeedText", "Speed HUD text is missing.");
            var effortText = RequireComponent<Text>("EffortText", "Effort HUD text is missing.");
            var progressText = RequireComponent<Text>("ProgressText", "Progress HUD text is missing.");
            var phaseText = RequireComponent<Text>("PhaseText", "State HUD text is missing.");
            var summaryText = RequireComponent<Text>("SummaryText", "Summary HUD text is missing.");

            RequireReference(controller, "cameraController", cameraController, "RideSessionController camera reference is not wired.");
            RequireReference(controller, "routeRenderer", routeRenderer, "RideSessionController route reference is not wired.");
            RequireReference(controller, "fogController", fogController, "RideSessionController fog reference is not wired.");
            RequireReference(controller, "mockRideInput", mockInput, "RideSessionController input reference is not wired.");
            RequireReference(controller, "hudController", hud, "RideSessionController HUD reference is not wired.");
            RequireReference(controller, "controlPanel", controls, "RideSessionController controls reference is not wired.");
            RequireReference(mockInput, "effortSlider", effortSlider, "MockRideInput slider reference is not wired.");
            RequireReference(controls, "startButton", startButton, "Start button reference is not wired.");
            RequireReference(controls, "pauseButton", pauseButton, "Pause button reference is not wired.");
            RequireReference(controls, "resumeButton", resumeButton, "Resume button reference is not wired.");
            RequireReference(controls, "finishButton", finishButton, "Finish button reference is not wired.");
            RequireReference(hud, "speedText", speedText, "Speed HUD reference is not wired.");
            RequireReference(hud, "effortText", effortText, "Effort HUD reference is not wired.");
            RequireReference(hud, "progressText", progressText, "Progress HUD reference is not wired.");
            RequireReference(hud, "phaseText", phaseText, "State HUD reference is not wired.");
            RequireReference(hud, "summaryText", summaryText, "Summary HUD reference is not wired.");
        }

        private static void ValidateRouteElevation(RouteDefinition route)
        {
            Require(route.points.Length >= 4, "MYB-13 route should have enough points for climbs and descents.");
            Require(Approximately(route.points[0].DistanceMeters, 0f), "Route should start at distance 0.");
            Require(
                Approximately(route.points[route.points.Length - 1].DistanceMeters, route.lengthMeters),
                "Route should end at lengthMeters.");

            var hasClimb = false;
            var hasDescent = false;
            var hasElevation = false;

            for (var i = 0; i < route.points.Length; i += 1)
            {
                var point = route.points[i];
                Require(IsFinite(point.DistanceMeters), "Route point distance must be finite.");
                Require(IsFinite(point.Position), "Route point position must be finite.");
                Require(
                    Mathf.Abs(point.Position.y) <= MaxElevationMeters,
                    "Route elevation should stay bounded for the vertical slice.");

                if (Mathf.Abs(point.Position.y) > 0.5f)
                {
                    hasElevation = true;
                }

                if (i == 0)
                {
                    continue;
                }

                var previous = route.points[i - 1];
                var distanceDelta = point.DistanceMeters - previous.DistanceMeters;
                var elevationDelta = point.Position.y - previous.Position.y;
                Require(distanceDelta > 0f, "Route point distances must be strictly monotonic.");
                Require(
                    Mathf.Abs(elevationDelta / distanceDelta) <= MaxRouteGrade,
                    "Route grade should stay bounded for simple controlled slopes.");

                hasClimb |= elevationDelta > 0.5f;
                hasDescent |= elevationDelta < -0.5f;
            }

            Require(hasElevation, "MYB-13 route should include visible elevation.");
            Require(hasClimb, "MYB-13 route should include at least one climb.");
            Require(hasDescent, "MYB-13 route should include at least one descent.");
        }

        private static void ValidateCameraSamples(RouteDefinition route)
        {
            var previousPosition = Vector3.zero;
            var hasPrevious = false;

            for (var i = 0; i <= 20; i += 1)
            {
                var progress01 = i / 20f;
                var routePosition = RouteMath.SamplePosition(route, progress01);
                var progress = new RouteProgressSnapshot(
                    progress01,
                    progress01 * route.lengthMeters,
                    progress01 >= 1f);
                var camera = RouteMath.CameraOnRail(route, progress, CameraRailConfig.Default);
                var lookVector = camera.LookAt - camera.Position;

                Require(IsFinite(routePosition), "Route sample must be finite.");
                Require(IsFinite(camera.Position) && IsFinite(camera.LookAt), "Camera sample must be finite.");
                Require(
                    camera.Position.y - routePosition.y >= 1.3f,
                    "Camera should remain above the elevated route.");
                Require(lookVector.sqrMagnitude > 1f, "Camera should look ahead, not at itself.");
                Require(Vector3.Dot(lookVector.normalized, Vector3.forward) > 0.2f, "Camera should keep looking along the route.");

                if (hasPrevious)
                {
                    Require(
                        Vector3.Distance(previousPosition, camera.Position) < 90f,
                        "Camera samples should not jump abruptly along the route.");
                }

                previousPosition = camera.Position;
                hasPrevious = true;
            }
        }

        private static void ValidateRouteRenderer(RouteRendererPlaceholder routeRenderer)
        {
            var line = routeRenderer.GetComponent<LineRenderer>();
            Require(line != null, "Route LineRenderer is missing.");
            Require(line.enabled, "Route LineRenderer should be enabled.");
            Require(line.positionCount >= 4, "Route LineRenderer should contain the elevated route points.");
            Require(line.widthMultiplier >= 1f, "Route LineRenderer should remain readable.");
            Require(line.sharedMaterial != null, "Route LineRenderer should keep a visible material.");

            var hasElevation = false;
            for (var i = 0; i < line.positionCount; i += 1)
            {
                var point = line.GetPosition(i);
                Require(IsFinite(point), "Route LineRenderer point must be finite.");
                hasElevation |= point.y > 0.5f;
            }

            Require(hasElevation, "Route LineRenderer should show elevated route points.");
        }

        private static void ValidateFogSettings()
        {
            Require(RenderSettings.fog, "RideMock.unity should persist fog enabled.");
            Require(RenderSettings.fogMode == FogMode.Linear, "RideMock.unity should preserve linear depth fog.");
            Require(
                Approximately(RenderSettings.fogStartDistance, ExpectedFogStartDistance),
                "RideMock.unity should preserve bounded fog start distance.");
            Require(
                Approximately(RenderSettings.fogEndDistance, ExpectedFogEndDistance),
                "RideMock.unity should preserve bounded fog end distance.");
            Require(
                RenderSettings.fogEndDistance > RenderSettings.fogStartDistance,
                "RideMock.unity fog should preserve readable long-distance depth.");
        }

        private static T Find<T>() where T : UnityEngine.Object
        {
            return UnityEngine.Object.FindFirstObjectByType<T>();
        }

        private static T RequireComponent<T>(string objectName, string message) where T : Component
        {
            var component = GameObject.Find(objectName)?.GetComponent<T>();
            Require(component != null, message);
            return component;
        }

        private static void RequireReference(
            UnityEngine.Object target,
            string propertyName,
            UnityEngine.Object expected,
            string message)
        {
            var serialized = new SerializedObject(target);
            var property = serialized.FindProperty(propertyName);
            Require(property != null, $"{propertyName} serialized property is missing.");
            Require(property.objectReferenceValue == expected, message);
        }

        private static void WriteReport()
        {
            var repoRoot = Path.GetFullPath(Path.Combine(Application.dataPath, "../../.."));
            var outputDir = Path.Combine(repoRoot, "_bmad-output/unity-test-results");
            Directory.CreateDirectory(outputDir);
            File.WriteAllText(
                Path.Combine(outputDir, "myb-13-editor-validation.txt"),
                "MYB-13 Unity editor validation passed.\n" +
                $"Unity: {Application.unityVersion}\n" +
                "Checks: version, ride math, route math, elevated route, bounded grade, finite route samples, stable camera samples, route renderer visibility, playable session loop, pause freeze, resume continuity, finish summary, scene hierarchy, wired controls, wired HUD, camera, route and bounded fog.\n");
        }

        private static void Require(bool condition, string message)
        {
            if (!condition)
            {
                throw new InvalidOperationException(message);
            }
        }

        private static bool Approximately(float left, float right)
        {
            return Mathf.Abs(left - right) < 0.001f;
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

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

        [MenuItem("Echappee/MYB-12/Validate RideMock Scene")]
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
            foreach (var point in route.points)
            {
                Require(Approximately(point.Position.y, 0f), "MYB-12 route should remain flat.");
            }

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
            Require(RenderSettings.fog, "RideMock.unity should persist fog enabled.");

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
                Path.Combine(outputDir, "myb-12-editor-validation.txt"),
                "MYB-12 Unity editor validation passed.\n" +
                $"Unity: {Application.unityVersion}\n" +
                "Checks: version, ride math, route math, flat route, playable session loop, pause freeze, resume continuity, finish summary, scene hierarchy, wired controls, wired HUD, camera, route and fog.\n");
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

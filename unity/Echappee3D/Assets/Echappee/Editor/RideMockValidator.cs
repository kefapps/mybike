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

namespace MyBike.Echappee3D.EditorTools
{
    public static class RideMockValidator
    {
        private const string ExpectedUnityVersion = "6000.4.10f1";
        private const string ScenePath = "Assets/Scenes/RideMock.unity";

        public static void Validate()
        {
            Require(
                Application.unityVersion == ExpectedUnityVersion,
                $"Expected Unity {ExpectedUnityVersion}, got {Application.unityVersion}");

            ValidateRideMath();
            ValidateRouteMath();
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
            Require(
                RouteMath.Resolve(null).id == "placeholder-route",
                "Invalid route should resolve to deterministic placeholder.");
        }

        private static void ValidateScene()
        {
            Require(
                AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) != null,
                "RideMock.unity scene asset is missing.");

            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            Require(scene.IsValid(), "RideMock.unity did not open.");
            Require(RenderSettings.fog, "RideMock.unity should persist fog enabled.");
            Require(Find<RideSessionController>() != null, "RideSessionController is missing.");
            Require(Find<RouteRendererPlaceholder>() != null, "RouteRendererPlaceholder is missing.");
            Require(Find<RideCameraController>() != null, "RideCameraController is missing.");
            Require(Find<DepthFogController>() != null, "DepthFogController is missing.");
            Require(Find<MockRideInput>() != null, "MockRideInput is missing.");
            Require(Find<HudController>() != null, "HudController is missing.");
            Require(Camera.main != null, "Main camera is missing.");
        }

        private static T Find<T>() where T : UnityEngine.Object
        {
            return UnityEngine.Object.FindFirstObjectByType<T>();
        }

        private static void WriteReport()
        {
            var repoRoot = Path.GetFullPath(Path.Combine(Application.dataPath, "../../.."));
            var outputDir = Path.Combine(repoRoot, "_bmad-output/unity-test-results");
            Directory.CreateDirectory(outputDir);
            File.WriteAllText(
                Path.Combine(outputDir, "myb-11-editor-validation.txt"),
                "MYB-11 Unity editor validation passed.\n" +
                $"Unity: {Application.unityVersion}\n" +
                "Checks: version, ride math, route math, fallback route, scene, camera, route, HUD, mock input, fog.\n");
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

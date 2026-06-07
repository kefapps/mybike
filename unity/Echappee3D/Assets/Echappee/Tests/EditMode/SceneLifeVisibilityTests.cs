using MyBike.Echappee3D.Core;
using MyBike.Echappee3D.Rendering;
using MyBike.Echappee3D.Route;
using NUnit.Framework;
using UnityEngine;

namespace MyBike.Echappee3D.Tests
{
    public sealed class SceneLifeVisibilityTests
    {
        [Test]
        public void MeasuresForwardObjectInsideCameraCone()
        {
            var route = RouteMath.CreateDefaultMockRoute();
            var progress = new RouteProgressSnapshot(0.25f, route.lengthMeters * 0.25f, false);
            var camera = RouteMath.CameraOnRail(route, progress, CameraRailConfig.Default);
            var worldPosition = camera.Position + (camera.LookAt - camera.Position).normalized * 18f;

            var sample = SceneLifeVisibility.Measure(
                "bird",
                "ForwardBird",
                worldPosition,
                route,
                CameraRailConfig.Default,
                90f,
                70f);

            Assert.That(sample.IsFinite, Is.True);
            Assert.That(sample.IsInCameraCone, Is.True);
            Assert.That(Mathf.Abs(sample.YawDegrees), Is.LessThan(1f));
            Assert.That(Mathf.Abs(sample.PitchDegrees), Is.LessThan(1f));
        }

        [Test]
        public void MeasuresSideObjectOutsideCameraConeWithFiniteAngles()
        {
            var route = RouteMath.CreateDefaultMockRoute();
            var worldPosition = RouteMath.SamplePosition(route, 0.4f) + new Vector3(60f, 2f, 0f);

            var sample = SceneLifeVisibility.Measure(
                "human",
                "RoadsideHuman",
                worldPosition,
                route,
                CameraRailConfig.Default,
                60f,
                50f);

            Assert.That(sample.IsFinite, Is.True);
            Assert.That(sample.IsInCameraCone, Is.False);
            Assert.That(Mathf.Abs(sample.YawDegrees), Is.GreaterThan(30f));
            Assert.That(sample.DistanceMeters, Is.GreaterThan(1f));
        }

        [Test]
        public void ComputesHorizontalFovFromVerticalAndAspect()
        {
            var horizontalFov = SceneLifeVisibility.HorizontalFovFromVertical(70f, 16f / 9f);

            Assert.That(horizontalFov, Is.GreaterThan(90f));
            Assert.That(horizontalFov, Is.LessThan(110f));
        }

        [Test]
        public void SummarizesMyb16CandidatePlacementsAgainstReadabilityTarget()
        {
            var route = RouteMath.CreateDefaultMockRoute();
            var horizontalFov = SceneLifeVisibility.HorizontalFovFromVertical(70f, 16f / 9f);
            var samples = new[]
            {
                SceneLifeVisibility.Measure("bird", "Bird_01", new Vector3(-20f, 18f, 290f), route, CameraRailConfig.Default, horizontalFov, 70f),
                SceneLifeVisibility.Measure("bird", "Bird_02", new Vector3(-8f, 18f, 320f), route, CameraRailConfig.Default, horizontalFov, 70f),
                SceneLifeVisibility.Measure("bird", "Bird_03", new Vector3(38f, 19f, 430f), route, CameraRailConfig.Default, horizontalFov, 70f),
                SceneLifeVisibility.Measure("bird", "Bird_04", new Vector3(-34f, 24f, 620f), route, CameraRailConfig.Default, horizontalFov, 70f),
                SceneLifeVisibility.Measure("bird", "Bird_05", new Vector3(20f, 21f, 790f), route, CameraRailConfig.Default, horizontalFov, 70f),
                SceneLifeVisibility.Measure("human", "HumanSilhouette_01", HumanVisualCenter(route, -20f, 300f), route, CameraRailConfig.Default, horizontalFov, 70f),
                SceneLifeVisibility.Measure("human", "HumanSilhouette_02", HumanVisualCenter(route, -10.5f, 324f), route, CameraRailConfig.Default, horizontalFov, 70f),
                SceneLifeVisibility.Measure("human", "HumanSilhouette_03", HumanVisualCenter(route, 24f, 760f), route, CameraRailConfig.Default, horizontalFov, 70f)
            };

            var summary = SceneLifeVisibility.Summarize(samples);

            Assert.That(summary.TotalCount, Is.EqualTo(8));
            Assert.That(summary.FiniteCount, Is.EqualTo(8));
            Assert.That(summary.InsideCount, Is.GreaterThanOrEqualTo(3));
            Assert.That(summary.BirdInsideCount, Is.GreaterThanOrEqualTo(1));
            Assert.That(summary.HumanInsideCount, Is.GreaterThanOrEqualTo(1));
            Assert.That(summary.MeetsReadabilityTarget(3, 1, 1), Is.True);
        }

        private static Vector3 HumanVisualCenter(RouteDefinition route, float x, float z)
        {
            var ground = RouteMath.SamplePosition(route, z / route.lengthMeters);
            return new Vector3(x, ground.y + 1.1f, z);
        }
    }
}

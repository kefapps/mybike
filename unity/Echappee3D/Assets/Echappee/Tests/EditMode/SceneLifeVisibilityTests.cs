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
    }
}

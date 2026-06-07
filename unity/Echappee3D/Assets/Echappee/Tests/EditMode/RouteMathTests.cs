using MyBike.Echappee3D.Core;
using MyBike.Echappee3D.Route;
using NUnit.Framework;
using UnityEngine;

namespace MyBike.Echappee3D.Tests
{
    public sealed class RouteMathTests
    {
        [Test]
        public void AdvanceUpdatesDistanceAndCompletion()
        {
            var route = RouteMath.CreateDefaultMockRoute();
            var progress = new RouteProgressSnapshot(0f, 0f, false);

            var next = RouteMath.Advance(progress, 10f, 5f, route);

            Assert.That(next.DistanceMeters, Is.EqualTo(50f).Within(0.001f));
            Assert.That(next.Progress01, Is.EqualTo(0.05f).Within(0.001f));
            Assert.That(next.Completed, Is.False);
        }

        [Test]
        public void SamplePositionAndCameraRemainFiniteWithFallbackRoute()
        {
            var progress = new RouteProgressSnapshot(0.5f, 50f, false);

            var position = RouteMath.SamplePosition(null, 0.5f);
            var camera = RouteMath.CameraOnRail(null, progress, CameraRailConfig.Default);

            Assert.That(IsFinite(position.x), Is.True);
            Assert.That(IsFinite(position.y), Is.True);
            Assert.That(IsFinite(position.z), Is.True);
            Assert.That(Vector3.Distance(camera.Position, camera.LookAt), Is.GreaterThan(0.01f));
        }

        [Test]
        public void SelectBiomeChangesAlongMockRoute()
        {
            var route = RouteMath.CreateDefaultMockRoute();

            Assert.That(RouteMath.SelectBiome(route, 0f), Is.EqualTo("coast"));
            Assert.That(RouteMath.SelectBiome(route, 0.5f), Is.EqualTo("forest"));
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }
    }
}

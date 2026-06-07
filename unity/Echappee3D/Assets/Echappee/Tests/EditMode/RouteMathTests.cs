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

        [Test]
        public void DefaultMockRouteIncludesBoundedElevation()
        {
            var route = RouteMath.CreateDefaultMockRoute();
            var hasClimb = false;
            var hasDescent = false;
            var hasElevation = false;

            for (var i = 0; i < route.points.Length; i += 1)
            {
                var point = route.points[i];
                Assert.That(IsFinite(point.Position.y), Is.True);
                Assert.That(Mathf.Abs(point.Position.y), Is.LessThanOrEqualTo(20f));
                hasElevation |= Mathf.Abs(point.Position.y) > 0.5f;

                if (i == 0)
                {
                    continue;
                }

                var previous = route.points[i - 1];
                var distanceDelta = point.DistanceMeters - previous.DistanceMeters;
                var elevationDelta = point.Position.y - previous.Position.y;

                Assert.That(distanceDelta, Is.GreaterThan(0f));
                Assert.That(Mathf.Abs(elevationDelta / distanceDelta), Is.LessThanOrEqualTo(0.08f));
                hasClimb |= elevationDelta > 0.5f;
                hasDescent |= elevationDelta < -0.5f;
            }

            Assert.That(hasElevation, Is.True);
            Assert.That(hasClimb, Is.True);
            Assert.That(hasDescent, Is.True);
        }

        [Test]
        public void CameraOnRailLooksForwardAlongElevatedRoute()
        {
            var route = RouteMath.CreateDefaultMockRoute();

            for (var i = 0; i <= 10; i += 1)
            {
                var progress01 = i / 10f;
                var routePosition = RouteMath.SamplePosition(route, progress01);
                var progress = new RouteProgressSnapshot(progress01, progress01 * route.lengthMeters, progress01 >= 1f);
                var camera = RouteMath.CameraOnRail(route, progress, CameraRailConfig.Default);
                var lookVector = camera.LookAt - camera.Position;

                Assert.That(IsFinite(camera.Position.x), Is.True);
                Assert.That(IsFinite(camera.Position.y), Is.True);
                Assert.That(IsFinite(camera.Position.z), Is.True);
                Assert.That(camera.Position.y - routePosition.y, Is.GreaterThanOrEqualTo(1.3f));
                Assert.That(lookVector.sqrMagnitude, Is.GreaterThan(1f));
                Assert.That(Vector3.Dot(lookVector.normalized, Vector3.forward), Is.GreaterThan(0.2f));
            }
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }
    }
}

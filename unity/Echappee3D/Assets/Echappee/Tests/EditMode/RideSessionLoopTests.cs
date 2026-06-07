using MyBike.Echappee3D.Core;
using MyBike.Echappee3D.Ride;
using MyBike.Echappee3D.Route;
using NUnit.Framework;

namespace MyBike.Echappee3D.Tests
{
    public sealed class RideSessionLoopTests
    {
        [Test]
        public void StartPauseResumeFinishPreserveMockProgression()
        {
            var input = new MockRideInputSource(0.7f);
            var loop = new RideSessionLoop(RouteMath.CreateDefaultMockRoute(), input);

            Assert.That(loop.Phase, Is.EqualTo(RidePhase.Idle));
            Assert.That(loop.Progress.Progress01, Is.EqualTo(0f).Within(0.001f));

            loop.Start();
            var running = loop.Tick(1f, 1d);
            Assert.That(running.Phase, Is.EqualTo(RidePhase.Running));
            Assert.That(running.SpeedMps, Is.GreaterThan(0f));
            Assert.That(running.Route.DistanceMeters, Is.GreaterThan(0f));

            loop.Pause();
            var pausedDistance = loop.Progress.DistanceMeters;
            var paused = loop.Tick(5f, 6d);
            Assert.That(paused.Phase, Is.EqualTo(RidePhase.Paused));
            Assert.That(paused.Route.DistanceMeters, Is.EqualTo(pausedDistance).Within(0.001f));

            loop.Resume();
            var resumed = loop.Tick(1f, 7d);
            Assert.That(resumed.Phase, Is.EqualTo(RidePhase.Running));
            Assert.That(resumed.Route.DistanceMeters, Is.GreaterThan(pausedDistance));

            loop.Finish();
            var finished = loop.CurrentSnapshot(8d);
            Assert.That(finished.Phase, Is.EqualTo(RidePhase.Finished));
            Assert.That(loop.LastSummary.HasValue, Is.True);
            Assert.That(loop.LastSummary.Value.DistanceMeters, Is.GreaterThan(0f));
            Assert.That(loop.LastSummary.Value.AverageSpeedMps, Is.GreaterThan(0f));
        }
    }
}

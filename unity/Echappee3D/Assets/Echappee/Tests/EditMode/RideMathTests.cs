using MyBike.Echappee3D.Core;
using MyBike.Echappee3D.Ride;
using NUnit.Framework;

namespace MyBike.Echappee3D.Tests
{
    public sealed class RideMathTests
    {
        [Test]
        public void MapMockInputToSpeedClampsToConfiguredRange()
        {
            var config = SpeedMappingConfig.Default;

            Assert.That(
                RideMath.MapMockInputToSpeed(new RideInputSample(RideInputSourceKind.Mock, -1f, 0), config),
                Is.EqualTo(1.5f).Within(0.001f));
            Assert.That(
                RideMath.MapMockInputToSpeed(new RideInputSample(RideInputSourceKind.Mock, 1f, 0), config),
                Is.EqualTo(9f).Within(0.001f));
            Assert.That(
                RideMath.MapMockInputToSpeed(new RideInputSample(RideInputSourceKind.Mock, 0.5f, 0), config),
                Is.EqualTo(5.25f).Within(0.001f));
        }

        [Test]
        public void SmoothSpeedMovesWithoutOvershoot()
        {
            Assert.That(
                RideMath.SmoothSpeed(0f, 9f, 1f, SpeedSmoothingConfig.Default),
                Is.EqualTo(2.5f).Within(0.001f));
            Assert.That(
                RideMath.SmoothSpeed(9f, 0f, 1f, SpeedSmoothingConfig.Default),
                Is.EqualTo(5f).Within(0.001f));
        }
    }
}

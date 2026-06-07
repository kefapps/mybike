using MyBike.Echappee3D.Core;
using UnityEngine;

namespace MyBike.Echappee3D.Ride
{
    public readonly struct SpeedMappingConfig
    {
        public SpeedMappingConfig(float minSpeedMps, float maxSpeedMps)
        {
            MinSpeedMps = Mathf.Max(0f, minSpeedMps);
            MaxSpeedMps = Mathf.Max(MinSpeedMps, maxSpeedMps);
        }

        public float MinSpeedMps { get; }
        public float MaxSpeedMps { get; }

        public static SpeedMappingConfig Default => new SpeedMappingConfig(1.5f, 9f);
    }

    public readonly struct SpeedSmoothingConfig
    {
        public SpeedSmoothingConfig(float accelerationPerSecond, float decelerationPerSecond)
        {
            AccelerationPerSecond = Mathf.Max(0f, accelerationPerSecond);
            DecelerationPerSecond = Mathf.Max(0f, decelerationPerSecond);
        }

        public float AccelerationPerSecond { get; }
        public float DecelerationPerSecond { get; }

        public static SpeedSmoothingConfig Default => new SpeedSmoothingConfig(2.5f, 4f);
    }

    public readonly struct RideSummary
    {
        public RideSummary(double elapsedSeconds, float distanceMeters)
        {
            ElapsedSeconds = elapsedSeconds;
            DistanceMeters = Mathf.Max(0f, distanceMeters);
            AverageSpeedMps = elapsedSeconds > 0 ? DistanceMeters / (float)elapsedSeconds : 0f;
        }

        public double ElapsedSeconds { get; }
        public float DistanceMeters { get; }
        public float AverageSpeedMps { get; }
    }

    public static class RideMath
    {
        public static float MapMockInputToSpeed(RideInputSample sample, SpeedMappingConfig config)
        {
            var effort01 = Mathf.Clamp01(float.IsNaN(sample.Effort01) ? 0f : sample.Effort01);
            return Mathf.Lerp(config.MinSpeedMps, config.MaxSpeedMps, effort01);
        }

        public static float SmoothSpeed(
            float currentSpeedMps,
            float targetSpeedMps,
            float dtSeconds,
            SpeedSmoothingConfig config)
        {
            var current = Mathf.Max(0f, currentSpeedMps);
            var target = Mathf.Max(0f, targetSpeedMps);

            if (dtSeconds <= 0f || float.IsNaN(dtSeconds))
            {
                return current;
            }

            var rate = target >= current ? config.AccelerationPerSecond : config.DecelerationPerSecond;
            return Mathf.MoveTowards(current, target, rate * dtSeconds);
        }

        public static RideSummary CalculateRideStats(double elapsedSeconds, float distanceMeters)
        {
            return new RideSummary(elapsedSeconds, distanceMeters);
        }
    }
}

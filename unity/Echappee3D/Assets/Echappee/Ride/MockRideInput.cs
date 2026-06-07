using MyBike.Echappee3D.Core;
using UnityEngine;

namespace MyBike.Echappee3D.Ride
{
    public interface IRideInputSource
    {
        RideInputSourceKind Kind { get; }
        RideInputSample Read(double nowSeconds);
    }

    public sealed class MockRideInputSource : IRideInputSource
    {
        private float effort01;

        public MockRideInputSource(float initialEffort01 = 0f)
        {
            SetEffort01(initialEffort01);
        }

        public RideInputSourceKind Kind => RideInputSourceKind.Mock;

        public float Effort01 => effort01;

        public void SetEffort01(float nextEffort01)
        {
            effort01 = Mathf.Clamp01(float.IsNaN(nextEffort01) ? 0f : nextEffort01);
        }

        public RideInputSample Read(double nowSeconds)
        {
            return new RideInputSample(RideInputSourceKind.Mock, effort01, nowSeconds);
        }
    }
}

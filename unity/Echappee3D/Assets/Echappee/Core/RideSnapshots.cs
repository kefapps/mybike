using UnityEngine;

namespace MyBike.Echappee3D.Core
{
    public enum RideInputSourceKind
    {
        Mock
    }

    public readonly struct RideInputSample
    {
        public RideInputSample(RideInputSourceKind source, float effort01, double timestampSeconds)
        {
            Source = source;
            Effort01 = effort01;
            TimestampSeconds = timestampSeconds;
        }

        public RideInputSourceKind Source { get; }
        public float Effort01 { get; }
        public double TimestampSeconds { get; }
    }

    public readonly struct RouteProgressSnapshot
    {
        public RouteProgressSnapshot(float progress01, float distanceMeters, bool completed)
        {
            Progress01 = progress01;
            DistanceMeters = distanceMeters;
            Completed = completed;
        }

        public float Progress01 { get; }
        public float DistanceMeters { get; }
        public bool Completed { get; }
    }

    public readonly struct CameraRigSnapshot
    {
        public CameraRigSnapshot(Vector3 position, Vector3 lookAt, float fovDegrees)
        {
            Position = position;
            LookAt = lookAt;
            FovDegrees = fovDegrees;
        }

        public Vector3 Position { get; }
        public Vector3 LookAt { get; }
        public float FovDegrees { get; }
    }

    public readonly struct RideFrameSnapshot
    {
        public RideFrameSnapshot(
            RidePhase phase,
            RideInputSample input,
            float speedMps,
            RouteProgressSnapshot route,
            CameraRigSnapshot camera,
            string biomeId,
            double elapsedSeconds)
        {
            Phase = phase;
            Input = input;
            SpeedMps = speedMps;
            Route = route;
            Camera = camera;
            BiomeId = biomeId;
            ElapsedSeconds = elapsedSeconds;
        }

        public RidePhase Phase { get; }
        public RideInputSample Input { get; }
        public float SpeedMps { get; }
        public RouteProgressSnapshot Route { get; }
        public CameraRigSnapshot Camera { get; }
        public string BiomeId { get; }
        public double ElapsedSeconds { get; }
    }
}

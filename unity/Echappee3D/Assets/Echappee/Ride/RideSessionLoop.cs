using MyBike.Echappee3D.Core;
using MyBike.Echappee3D.Route;
using UnityEngine;

namespace MyBike.Echappee3D.Ride
{
    public sealed class RideSessionLoop
    {
        private readonly MockRideInputSource inputSource;
        private readonly RouteDefinition route;
        private readonly SpeedMappingConfig speedMapping;
        private readonly SpeedSmoothingConfig speedSmoothing;
        private readonly CameraRailConfig cameraRail;

        private RouteProgressSnapshot progress;
        private float speedMps;
        private double elapsedSeconds;

        public RideSessionLoop(RouteDefinition route, MockRideInputSource inputSource)
            : this(route, inputSource, SpeedMappingConfig.Default, SpeedSmoothingConfig.Default, CameraRailConfig.Default)
        {
        }

        public RideSessionLoop(
            RouteDefinition route,
            MockRideInputSource inputSource,
            SpeedMappingConfig speedMapping,
            SpeedSmoothingConfig speedSmoothing,
            CameraRailConfig cameraRail)
        {
            this.route = RouteMath.Resolve(route);
            this.inputSource = inputSource ?? new MockRideInputSource();
            this.speedMapping = speedMapping;
            this.speedSmoothing = speedSmoothing;
            this.cameraRail = cameraRail;
            Reset();
        }

        public RidePhase Phase { get; private set; }
        public RouteProgressSnapshot Progress => progress;
        public float SpeedMps => speedMps;
        public double ElapsedSeconds => elapsedSeconds;
        public RideSummary? LastSummary { get; private set; }

        public void Start()
        {
            if (Phase == RidePhase.Idle || Phase == RidePhase.Finished || Phase == RidePhase.Error)
            {
                Reset();
                Phase = RidePhase.Running;
            }
        }

        public void Pause()
        {
            if (Phase == RidePhase.Running)
            {
                Phase = RidePhase.Paused;
            }
        }

        public void Resume()
        {
            if (Phase == RidePhase.Paused)
            {
                Phase = RidePhase.Running;
            }
        }

        public void Finish()
        {
            if (Phase == RidePhase.Finished)
            {
                return;
            }

            Phase = RidePhase.Finished;
            LastSummary = RideMath.CalculateRideStats(elapsedSeconds, progress.DistanceMeters);
        }

        public RideFrameSnapshot Tick(float dtSeconds, double nowSeconds)
        {
            if (Phase != RidePhase.Running)
            {
                return CurrentSnapshot(nowSeconds);
            }

            var dt = Mathf.Max(0f, float.IsNaN(dtSeconds) ? 0f : dtSeconds);
            elapsedSeconds += dt;

            var sample = inputSource.Read(nowSeconds);
            var targetSpeed = RideMath.MapMockInputToSpeed(sample, speedMapping);
            speedMps = RideMath.SmoothSpeed(speedMps, targetSpeed, dt, speedSmoothing);
            progress = RouteMath.Advance(progress, speedMps, dt, route);

            if (progress.Completed)
            {
                Finish();
            }

            return BuildSnapshot(sample);
        }

        public RideFrameSnapshot CurrentSnapshot(double nowSeconds)
        {
            return BuildSnapshot(inputSource.Read(nowSeconds));
        }

        private void Reset()
        {
            Phase = RidePhase.Idle;
            progress = new RouteProgressSnapshot(0f, 0f, false);
            speedMps = 0f;
            elapsedSeconds = 0d;
            LastSummary = null;
        }

        private RideFrameSnapshot BuildSnapshot(RideInputSample sample)
        {
            var camera = RouteMath.CameraOnRail(route, progress, cameraRail);
            var biomeId = RouteMath.SelectBiome(route, progress.Progress01);

            return new RideFrameSnapshot(
                Phase,
                sample,
                speedMps,
                progress,
                camera,
                biomeId,
                elapsedSeconds);
        }
    }
}

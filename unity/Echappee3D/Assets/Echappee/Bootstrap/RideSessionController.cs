using MyBike.Echappee3D.Core;
using MyBike.Echappee3D.Rendering;
using MyBike.Echappee3D.Ride;
using MyBike.Echappee3D.Route;
using MyBike.Echappee3D.UI;
using UnityEngine;

namespace MyBike.Echappee3D.Bootstrap
{
    public sealed class RideSessionController : MonoBehaviour
    {
        [SerializeField] private RideCameraController cameraController;
        [SerializeField] private RouteRendererPlaceholder routeRenderer;
        [SerializeField] private DepthFogController fogController;
        [SerializeField] private MockRideInput mockRideInput;
        [SerializeField] private HudController hudController;

        private readonly MockRideInputSource inputSource = new MockRideInputSource(0.35f);
        private RouteDefinition route;
        private RouteProgressSnapshot progress;
        private RidePhase phase = RidePhase.Idle;
        private float speedMps;
        private double elapsedSeconds;

        private void Awake()
        {
            route = RouteMath.CreateDefaultMockRoute();
            progress = new RouteProgressSnapshot(0f, 0f, false);
            mockRideInput?.Bind(inputSource);
            routeRenderer?.EnsureVisible(route);
            fogController?.ApplyDefaultFog();
            StartRide();
        }

        private void Update()
        {
            if (phase != RidePhase.Running)
            {
                PublishFrame();
                return;
            }

            var dt = Mathf.Min(Time.deltaTime, 0.05f);
            elapsedSeconds += dt;
            var sample = inputSource.Read(Time.timeAsDouble);
            var targetSpeed = RideMath.MapMockInputToSpeed(sample, SpeedMappingConfig.Default);
            speedMps = RideMath.SmoothSpeed(speedMps, targetSpeed, dt, SpeedSmoothingConfig.Default);
            progress = RouteMath.Advance(progress, speedMps, dt, route);

            if (progress.Completed)
            {
                phase = RidePhase.Finished;
            }

            PublishFrame(sample);
        }

        public void StartRide()
        {
            if (phase == RidePhase.Idle || phase == RidePhase.Paused)
            {
                phase = RidePhase.Running;
            }
        }

        public void PauseRide()
        {
            if (phase == RidePhase.Running)
            {
                phase = RidePhase.Paused;
            }
        }

        public void FinishRide()
        {
            phase = RidePhase.Finished;
        }

        private void PublishFrame()
        {
            PublishFrame(inputSource.Read(Time.timeAsDouble));
        }

        private void PublishFrame(RideInputSample sample)
        {
            var camera = RouteMath.CameraOnRail(route, progress, CameraRailConfig.Default);
            var biomeId = RouteMath.SelectBiome(route, progress.Progress01);
            var snapshot = new RideFrameSnapshot(
                phase,
                sample,
                speedMps,
                progress,
                camera,
                biomeId,
                elapsedSeconds);

            cameraController?.Apply(snapshot.Camera);
            hudController?.Render(snapshot);
        }
    }
}

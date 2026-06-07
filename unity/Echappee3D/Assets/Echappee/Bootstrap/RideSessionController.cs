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
        [SerializeField] private RideControlPanel controlPanel;

        private readonly MockRideInputSource inputSource = new MockRideInputSource(0.35f);
        private RideSessionLoop session;
        private RouteDefinition route;
        private RideFrameSnapshot latestSnapshot;

        public RidePhase Phase => session?.Phase ?? RidePhase.Idle;
        public RouteProgressSnapshot Progress => session?.Progress ?? new RouteProgressSnapshot(0f, 0f, false);
        public RideSummary? Summary => session?.LastSummary;

        private void Awake()
        {
            route = RouteMath.CreateDefaultMockRoute();
            session = new RideSessionLoop(route, inputSource);

            mockRideInput?.Bind(inputSource);
            controlPanel?.Bind(this);
            routeRenderer?.EnsureVisible(route);
            fogController?.ApplyDefaultFog();

            PublishFrame(session.CurrentSnapshot(Time.timeAsDouble));
        }

        private void Update()
        {
            if (session == null)
            {
                return;
            }

            var dt = Mathf.Min(Time.deltaTime, 0.05f);
            PublishFrame(session.Tick(dt, Time.timeAsDouble));
        }

        public void StartRide()
        {
            session?.Start();
            PublishFrame(session?.CurrentSnapshot(Time.timeAsDouble));
        }

        public void PauseRide()
        {
            session?.Pause();
            PublishFrame(session?.CurrentSnapshot(Time.timeAsDouble));
        }

        public void ResumeRide()
        {
            session?.Resume();
            PublishFrame(session?.CurrentSnapshot(Time.timeAsDouble));
        }

        public void FinishRide()
        {
            session?.Finish();
            PublishFrame(session?.CurrentSnapshot(Time.timeAsDouble));
        }

        private void PublishFrame(RideFrameSnapshot? snapshot)
        {
            if (!snapshot.HasValue)
            {
                return;
            }

            latestSnapshot = snapshot.Value;
            cameraController?.Apply(latestSnapshot.Camera);
            hudController?.Render(latestSnapshot, session?.LastSummary);
            controlPanel?.Render(latestSnapshot.Phase);
        }
    }
}

using System;
using MYB57;
using MYB59;
using MYB60;
using UnityEngine;
using UnityEngine.UI;

namespace MYB89
{
    public sealed class MYB89ProbeRide : MonoBehaviour
    {
        [Header("Route")]
        public Transform[] routeMarkers = Array.Empty<Transform>();
        public float speedMetersPerSecond = 12.5f;
        public float progressMeters;
        public bool autoplay = true;

        [Header("MYB-73 Route Preview")]
        public bool waitForRoutePreview;

        [Header("View")]
        public Transform cameraPivot;
        public float cameraBobMeters = 0.045f;
        public float cameraBobFrequency = 1.8f;
        public int trajectorySamplesPerSegment = MYB89RideTrajectory.DefaultSamplesPerSegment;
        public float turnLookAheadMeters = 8f;
        public float orientationSmoothing = 7f;
        public float cameraTurnLeanMaxDegrees = 2.2f;

        [Header("HUD")]
        public Text distanceLabel;
        public Text speedLabel;
        public Text difficultyLabel;
        public Text gradeLabel;
        public Text segmentLabel;
        public Text verdictLabel;

        [Header("MYB-57 Mock Trainer")]
        public bool useEffortSimulator = true;
        public MYB57TrainerMode trainerMode = MYB57TrainerMode.Simulated;
        public MYB57TrainerSourcePreset trainerSourcePreset = MYB57TrainerSourcePreset.PowerAvailable;
        [Range(0f, 1f)]
        public float fatigue01;

        [Header("MYB-64 No-Trainer Fallback")]
        public bool useNoTrainerFallback;
        [Range(0f, 1f)]
        public float manualFallbackEffort01 = 0.45f;

        [Header("MYB-59 Resistance Controller")]
        public MYB59ResistanceController resistanceController;

        [Header("MYB-60 Resistance Mapper")]
        public MYB60ResistanceMapper resistanceMapper;

        [Header("Lighting")]
        public Light keyLight;

        private float[] segmentLengths = Array.Empty<float>();
        private float routeLength;
        private Vector3 cameraBaseLocalPosition;
        private Quaternion cameraBaseLocalRotation;
        private MYB57EffortSnapshot lastEffortSnapshot;
        private MYB59ResistanceSnapshot lastResistanceSnapshot;
        private MYB60ResistanceMappingSnapshot lastResistanceMappingSnapshot;
        private bool hasEffortSnapshot;
        private bool hasResistanceMappingSnapshot;
        private bool reuseEffortSnapshotForHud;
        private bool routePreviewLaunched;
        private Vector3[] smoothedRoutePoints = Array.Empty<Vector3>();
        private bool hasPoseRotation;
        private bool hasCameraBasePose;

        public float RouteLength => routeLength;
        public MYB57EffortSnapshot LastEffortSnapshot => lastEffortSnapshot;
        public MYB59ResistanceSnapshot LastResistanceSnapshot => lastResistanceSnapshot;
        public MYB60ResistanceMappingSnapshot LastResistanceMappingSnapshot => lastResistanceMappingSnapshot;
        public bool IsRoutePreviewWaiting => waitForRoutePreview && !routePreviewLaunched;
        public int TrajectorySampleCount => smoothedRoutePoints == null ? 0 : smoothedRoutePoints.Length;
        public bool IsNoTrainerFallbackActive =>
            useEffortSimulator && (useNoTrainerFallback || trainerSourcePreset == MYB57TrainerSourcePreset.ManualFallback);

        private void Awake()
        {
            CacheResistanceController();
            CacheResistanceMapper();
            RebuildRouteCache();
            CacheCameraBasePosition();
            ApplyRoutePreviewGate();
            ApplyPose(progressMeters);
            UpdateHud();
        }

        private void Start()
        {
            CacheResistanceController();
            CacheResistanceMapper();
            RebuildRouteCache();
            CacheCameraBasePosition();
            ApplyRoutePreviewGate();
            ApplyPose(progressMeters);
            UpdateHud();
        }

        private void OnValidate()
        {
            speedMetersPerSecond = Mathf.Max(0.1f, speedMetersPerSecond);
            cameraBobMeters = Mathf.Max(0f, cameraBobMeters);
            cameraBobFrequency = Mathf.Max(0.1f, cameraBobFrequency);
            trajectorySamplesPerSegment = Mathf.Clamp(trajectorySamplesPerSegment, 2, 24);
            turnLookAheadMeters = Mathf.Clamp(turnLookAheadMeters, 0f, 24f);
            orientationSmoothing = Mathf.Clamp(orientationSmoothing, 0f, 24f);
            cameraTurnLeanMaxDegrees = Mathf.Clamp(cameraTurnLeanMaxDegrees, 0f, 6f);
            manualFallbackEffort01 = Mathf.Clamp01(manualFallbackEffort01);
        }

        private void Update()
        {
            if (routeMarkers == null || routeMarkers.Length < 2)
            {
                return;
            }

            if (segmentLengths.Length != routeMarkers.Length - 1)
            {
                RebuildRouteCache();
            }

            if (Application.isPlaying && autoplay && !IsRoutePreviewWaiting)
            {
                AdvanceAutoplayFrame(Time.deltaTime);
            }

            ApplyPose(progressMeters);
            UpdateHud();
            AnimateKeyLight();
        }

        private void AdvanceAutoplayFrame(float deltaTimeSeconds)
        {
            var frameSpeed = useEffortSimulator && hasEffortSnapshot
                ? lastEffortSnapshot.SpeedMetersPerSecond
                : speedMetersPerSecond;
            progressMeters += frameSpeed * Mathf.Max(0f, deltaTimeSeconds);
            if (useEffortSimulator)
            {
                SampleEffort(deltaTimeSeconds, true);
                reuseEffortSnapshotForHud = true;
            }
        }

        public void SetPreviewProgress(float meters)
        {
            RebuildRouteCache();
            CacheCameraBasePosition();
            progressMeters = meters;
            hasEffortSnapshot = false;
            hasResistanceMappingSnapshot = false;
            reuseEffortSnapshotForHud = false;
            hasPoseRotation = false;
            CacheResistanceMapper();
            resistanceMapper?.ResetSmoothing();
            ApplyPose(progressMeters);
            UpdateHud();
        }

        public void PrepareRoutePreview()
        {
            waitForRoutePreview = true;
            routePreviewLaunched = false;
            autoplay = false;
            SetPreviewProgress(0f);
        }

        public void LaunchRoutePreviewRide()
        {
            routePreviewLaunched = true;
            autoplay = true;
            UpdateHud();
        }

        public void RebuildRouteCache()
        {
            if (routeMarkers == null || routeMarkers.Length < 2)
            {
                segmentLengths = Array.Empty<float>();
                smoothedRoutePoints = Array.Empty<Vector3>();
                routeLength = 0f;
                return;
            }

            segmentLengths = new float[routeMarkers.Length - 1];
            routeLength = 0f;

            for (var i = 0; i < routeMarkers.Length - 1; i++)
            {
                var a = routeMarkers[i];
                var b = routeMarkers[i + 1];
                var segmentLength = a == null || b == null ? 0f : Vector3.Distance(a.position, b.position);
                segmentLengths[i] = segmentLength;
                routeLength += segmentLength;
            }

            smoothedRoutePoints = MYB89RideTrajectory.BuildSmoothedPoints(routeMarkers, trajectorySamplesPerSegment);
            routeLength = MYB89RideTrajectory.Length(smoothedRoutePoints);
        }

        private void CacheCameraBasePosition()
        {
            if (cameraPivot != null && !hasCameraBasePose)
            {
                cameraBaseLocalPosition = cameraPivot.localPosition;
                cameraBaseLocalRotation = cameraPivot.localRotation;
                hasCameraBasePose = true;
            }
        }

        private void ApplyRoutePreviewGate()
        {
            if (!waitForRoutePreview || routePreviewLaunched)
            {
                return;
            }

            autoplay = false;
            progressMeters = 0f;
        }

        private void ApplyPose(float meters)
        {
            if (!TrySampleRoute(meters, out var position, out var forward))
            {
                return;
            }

            var turnLeanDegrees = 0f;
            if (turnLookAheadMeters > 0.01f && TrySampleRoute(meters + turnLookAheadMeters, out _, out var lookAheadForward))
            {
                var blendedForward = Vector3.Slerp(forward, lookAheadForward, 0.65f).normalized;
                if (blendedForward.sqrMagnitude > 0.01f)
                {
                    forward = blendedForward;
                }

                turnLeanDegrees = Mathf.Clamp(
                    -Vector3.SignedAngle(forward, lookAheadForward, Vector3.up) * 0.12f,
                    -cameraTurnLeanMaxDegrees,
                    cameraTurnLeanMaxDegrees);
            }

            var targetRotation = Quaternion.LookRotation(forward, Vector3.up);
            if (!hasPoseRotation || orientationSmoothing <= 0.01f || !Application.isPlaying)
            {
                transform.SetPositionAndRotation(position, targetRotation);
                hasPoseRotation = true;
            }
            else
            {
                var smoothing01 = 1f - Mathf.Exp(-orientationSmoothing * Time.deltaTime);
                transform.position = position;
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, smoothing01);
            }

            if (cameraPivot != null)
            {
                var bob = Mathf.Sin((meters / Mathf.Max(1f, speedMetersPerSecond)) * Mathf.PI * 2f * cameraBobFrequency) * cameraBobMeters;
                cameraPivot.localPosition = cameraBaseLocalPosition + new Vector3(0f, bob, 0f);
                cameraPivot.localRotation = cameraBaseLocalRotation * Quaternion.Euler(0f, 0f, turnLeanDegrees);
            }
        }

        private bool TrySampleRoute(float meters, out Vector3 position, out Vector3 forward)
        {
            return TrySampleRoute(meters, out position, out forward, out _);
        }

        private bool TrySampleRoute(float meters, out Vector3 position, out Vector3 forward, out int routeSegmentIndex)
        {
            position = Vector3.zero;
            forward = Vector3.forward;
            routeSegmentIndex = -1;

            if (smoothedRoutePoints == null || smoothedRoutePoints.Length < 2 || routeLength <= 0.01f)
            {
                return false;
            }

            if (!MYB89RideTrajectory.TrySample(smoothedRoutePoints, meters, true, out var sample))
            {
                return false;
            }

            position = sample.Position;
            forward = sample.Forward;
            routeSegmentIndex = sample.SegmentIndex;
            return true;
        }

        private void UpdateHud()
        {
            var total = routeLength <= 0.01f ? 1f : routeLength;
            var wrappedMeters = Mathf.Repeat(progressMeters, total);
            var progress01 = Mathf.Clamp01(wrappedMeters / total);
            var percent = progress01 * 100f;
            var routeHud = SampleRouteHud(wrappedMeters, progress01);
            var effort = useEffortSimulator && (!reuseEffortSnapshotForHud || !hasEffortSnapshot)
                ? SampleEffort(0f, false)
                : lastEffortSnapshot;
            reuseEffortSnapshotForHud = false;
            var resistance = lastResistanceSnapshot;
            var displaySpeed = useEffortSimulator ? effort.SpeedMetersPerSecond : speedMetersPerSecond;

            if (distanceLabel != null)
            {
                distanceLabel.text = $"{wrappedMeters:000} m / {routeLength:000} m";
            }

            if (speedLabel != null)
            {
                speedLabel.text = $"{displaySpeed * 3.6f:00} km/h";
            }

            if (difficultyLabel != null)
            {
                difficultyLabel.text = useEffortSimulator
                    ? $"Effort: {EffortHudLabel(effort)} {effort.PedalEffort01 * 100f:00}%"
                    : $"Effort: {routeHud.DifficultyLabel}";
            }

            if (gradeLabel != null)
            {
                gradeLabel.text = useEffortSimulator
                    ? $"Pente: {routeHud.GradePercent:+0.0;-0.0;0.0}% | Res: {effort.TargetResistanceLevel:00}~{lastResistanceMappingSnapshot.SmoothedResistanceLevel:00}->{resistance.AppliedResistanceLevel:00}"
                    : $"Pente: {routeHud.GradePercent:+0.0;-0.0;0.0}%";
            }

            if (segmentLabel != null)
            {
                segmentLabel.text = $"Segment: {routeHud.SegmentLabel}";
            }

            if (verdictLabel != null)
            {
                verdictLabel.text = useEffortSimulator
                    ? $"{TrainerModeHudLabel(effort)} | Map {lastResistanceMappingSnapshot.StatusLabel} {lastResistanceMappingSnapshot.SmoothedResistanceLevel:00} | Ctrl {resistance.StatusLabel} {resistance.AppliedResistanceLevel:00} | Fatigue {effort.Fatigue01 * 100f:00}% | {percent:00}%"
                    : $"Mock ride running | {percent:00}% | readable motion corridor";
            }
        }

        private MYB57EffortSnapshot SampleEffort(float deltaTimeSeconds, bool updateFatigue)
        {
            if (!useEffortSimulator)
            {
                return lastEffortSnapshot;
            }

            var total = routeLength <= 0.01f ? 1f : routeLength;
            var wrappedMeters = Mathf.Repeat(progressMeters, total);
            var progress01 = Mathf.Clamp01(wrappedMeters / total);
            var routeHud = SampleRouteHud(wrappedMeters, progress01);
            var sourcePreset = IsNoTrainerFallbackActive
                ? MYB57TrainerSourcePreset.ManualFallback
                : trainerSourcePreset;
            var input = new MYB57EffortInput(
                trainerMode,
                sourcePreset,
                routeHud.RouteSegment,
                routeHud.GradePercent,
                fatigue01,
                deltaTimeSeconds);

            lastEffortSnapshot = IsNoTrainerFallbackActive
                ? MYB57EffortSimulator.EvaluateManualFallback(input, manualFallbackEffort01)
                : MYB57EffortSimulator.EvaluateMock(input);
            lastResistanceSnapshot = ApplyResistance(lastEffortSnapshot, routeHud, deltaTimeSeconds);
            hasEffortSnapshot = true;
            if (updateFatigue)
            {
                fatigue01 = lastEffortSnapshot.Fatigue01;
            }

            return lastEffortSnapshot;
        }

        private string EffortHudLabel(MYB57EffortSnapshot effort)
        {
            return IsNoTrainerFallbackActive ? "Manual fallback" : effort.EffortLabel;
        }

        private string TrainerModeHudLabel(MYB57EffortSnapshot effort)
        {
            return IsNoTrainerFallbackActive
                ? $"No trainer: {effort.SourceBadge}"
                : $"Mock trainer: {effort.SourceBadge}";
        }

        private void CacheResistanceController()
        {
            if (resistanceController == null)
            {
                resistanceController = GetComponent<MYB59ResistanceController>();
            }
        }

        private void CacheResistanceMapper()
        {
            if (resistanceMapper == null)
            {
                resistanceMapper = GetComponent<MYB60ResistanceMapper>();
            }
        }

        private MYB59ResistanceSnapshot ApplyResistance(MYB57EffortSnapshot effortSnapshot, RouteHudSnapshot routeHud, float deltaTimeSeconds)
        {
            var mappingInput = new MYB60ResistanceMappingInput(
                routeHud.RouteSegment,
                routeHud.GradePercent,
                effortSnapshot.TargetResistance01,
                deltaTimeSeconds);
            CacheResistanceMapper();
            lastResistanceMappingSnapshot = resistanceMapper == null
                ? MYB60ResistanceMapper.LocalFallback(
                    mappingInput,
                    hasResistanceMappingSnapshot
                        ? lastResistanceMappingSnapshot.SmoothedResistance01
                        : mappingInput.TargetResistance01,
                    hasResistanceMappingSnapshot)
                : resistanceMapper.MapResistance(mappingInput);
            hasResistanceMappingSnapshot = true;

            var demand = new MYB59ResistanceDemand(
                lastResistanceMappingSnapshot.SmoothedResistance01,
                "MYB-60 smoothed route difficulty");
            CacheResistanceController();
            return resistanceController == null
                ? MYB59ResistanceController.LocalFallback(demand)
                : resistanceController.ApplyResistance(demand, deltaTimeSeconds);
        }

        private RouteHudSnapshot SampleRouteHud(float wrappedMeters, float progress01)
        {
            var gradePercent = 0f;
            var segmentIndex = -1;

            if (TrySampleRoute(wrappedMeters, out _, out var forward, out segmentIndex))
            {
                var horizontalMeters = new Vector2(forward.x, forward.z).magnitude;
                if (horizontalMeters > 0.01f)
                {
                    gradePercent = forward.y / horizontalMeters * 100f;
                }
            }

            var routeSegment = MYB57EffortSimulator.SegmentForProgress(progress01);
            var segmentLabel = SegmentLabelFor(routeSegment);
            var difficultyLabel = DifficultyLabelFor(routeSegment, gradePercent);
            return new RouteHudSnapshot(difficultyLabel, gradePercent, segmentLabel, routeSegment);
        }

        private static string SegmentLabelFor(MYB57RouteSegment routeSegment)
        {
            switch (routeSegment)
            {
                case MYB57RouteSegment.Warmup:
                    return "Warmup";
                case MYB57RouteSegment.Climb:
                    return "Climb";
                case MYB57RouteSegment.Sprint:
                    return "Sprint";
                case MYB57RouteSegment.Recovery:
                    return "Recovery";
                default:
                    return "Warmup";
            }
        }

        private static string DifficultyLabelFor(MYB57RouteSegment routeSegment, float gradePercent)
        {
            if (gradePercent >= 0.25f || routeSegment == MYB57RouteSegment.Climb)
            {
                return "Sustained climb";
            }

            if (routeSegment == MYB57RouteSegment.Sprint)
            {
                return "Hard sprint";
            }

            if (gradePercent < -0.25f || routeSegment == MYB57RouteSegment.Recovery)
            {
                return "Recovery";
            }

            return "Easy spin";
        }

        private readonly struct RouteHudSnapshot
        {
            public RouteHudSnapshot(string difficultyLabel, float gradePercent, string segmentLabel, MYB57RouteSegment routeSegment)
            {
                DifficultyLabel = difficultyLabel;
                GradePercent = gradePercent;
                SegmentLabel = segmentLabel;
                RouteSegment = routeSegment;
            }

            public string DifficultyLabel { get; }
            public float GradePercent { get; }
            public string SegmentLabel { get; }
            public MYB57RouteSegment RouteSegment { get; }
        }

        private void AnimateKeyLight()
        {
            if (keyLight == null)
            {
                return;
            }

            keyLight.intensity = 1.05f + Mathf.Sin(progressMeters * 0.035f) * 0.08f;
        }
    }
}

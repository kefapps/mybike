using MYB57;
using UnityEngine;

namespace MYB60
{
    public enum MYB60ResistanceMappingStatus
    {
        Stable,
        Ramping,
        Limited
    }

    public readonly struct MYB60ResistanceMappingInput
    {
        public MYB60ResistanceMappingInput(
            MYB57RouteSegment routeSegment,
            float gradePercent,
            float targetResistance01,
            float deltaTimeSeconds)
        {
            RouteSegment = routeSegment;
            GradePercent = gradePercent;
            TargetResistance01 = Mathf.Clamp01(targetResistance01);
            DeltaTimeSeconds = Mathf.Max(0f, deltaTimeSeconds);
        }

        public MYB57RouteSegment RouteSegment { get; }
        public float GradePercent { get; }
        public float TargetResistance01 { get; }
        public int TargetResistanceLevel => Mathf.RoundToInt(TargetResistance01 * 100f);
        public float DeltaTimeSeconds { get; }
    }

    public readonly struct MYB60ResistanceMappingSnapshot
    {
        public MYB60ResistanceMappingSnapshot(
            MYB60ResistanceMappingInput input,
            float limitedResistance01,
            float smoothedResistance01,
            MYB60ResistanceMappingStatus status)
        {
            Input = input;
            LimitedResistance01 = Mathf.Clamp01(limitedResistance01);
            SmoothedResistance01 = Mathf.Clamp01(smoothedResistance01);
            Status = status;
        }

        public MYB60ResistanceMappingInput Input { get; }
        public float LimitedResistance01 { get; }
        public int LimitedResistanceLevel => Mathf.RoundToInt(LimitedResistance01 * 100f);
        public float SmoothedResistance01 { get; }
        public int SmoothedResistanceLevel => Mathf.RoundToInt(SmoothedResistance01 * 100f);
        public MYB60ResistanceMappingStatus Status { get; }
        public bool IsLimited => Status == MYB60ResistanceMappingStatus.Limited;
        public bool IsRamping => Status == MYB60ResistanceMappingStatus.Ramping;

        public string StatusLabel
        {
            get
            {
                switch (Status)
                {
                    case MYB60ResistanceMappingStatus.Limited:
                        return "Limited";
                    case MYB60ResistanceMappingStatus.Ramping:
                        return "Ramping";
                    case MYB60ResistanceMappingStatus.Stable:
                        return "Stable";
                    default:
                        return "Unknown";
                }
            }
        }
    }

    public interface IMYB60ResistanceMapper
    {
        MYB60ResistanceMappingSnapshot LastMappingSnapshot { get; }
        MYB60ResistanceMappingSnapshot MapResistance(MYB60ResistanceMappingInput input);
        void ResetSmoothing();
    }

    public sealed class MYB60ResistanceMapper : MonoBehaviour, IMYB60ResistanceMapper
    {
        public const float DefaultMinResistance01 = 0.12f;
        public const float DefaultMaxResistance01 = 0.88f;
        public const float DefaultWarmupMaxResistance01 = 0.55f;
        public const float DefaultRecoveryMaxResistance01 = 0.42f;
        public const float DefaultSprintMaxResistance01 = 0.84f;
        public const float DefaultRampUpPerSecond01 = 0.35f;
        public const float DefaultRampDownPerSecond01 = 0.55f;

        [SerializeField]
        [Range(0f, 1f)]
        private float minResistance01 = DefaultMinResistance01;

        [SerializeField]
        [Range(0f, 1f)]
        private float maxResistance01 = DefaultMaxResistance01;

        [SerializeField]
        [Range(0f, 1f)]
        private float warmupMaxResistance01 = DefaultWarmupMaxResistance01;

        [SerializeField]
        [Range(0f, 1f)]
        private float recoveryMaxResistance01 = DefaultRecoveryMaxResistance01;

        [SerializeField]
        [Range(0f, 1f)]
        private float sprintMaxResistance01 = DefaultSprintMaxResistance01;

        [SerializeField]
        [Min(0f)]
        private float rampUpPerSecond01 = DefaultRampUpPerSecond01;

        [SerializeField]
        [Min(0f)]
        private float rampDownPerSecond01 = DefaultRampDownPerSecond01;

        private bool hasLastMappingSnapshot;
        private MYB60ResistanceMappingSnapshot lastMappingSnapshot;

        public float MinResistance01
        {
            get => minResistance01;
            set => minResistance01 = Mathf.Clamp01(value);
        }

        public float MaxResistance01
        {
            get => maxResistance01;
            set => maxResistance01 = Mathf.Clamp01(value);
        }

        public float RampUpPerSecond01
        {
            get => rampUpPerSecond01;
            set => rampUpPerSecond01 = Mathf.Max(0f, value);
        }

        public float RampDownPerSecond01
        {
            get => rampDownPerSecond01;
            set => rampDownPerSecond01 = Mathf.Max(0f, value);
        }

        public MYB60ResistanceMappingSnapshot LastMappingSnapshot => lastMappingSnapshot;

        private void Awake()
        {
            NormalizeSettings();
        }

        private void OnValidate()
        {
            NormalizeSettings();
        }

        public void ResetSmoothing()
        {
            hasLastMappingSnapshot = false;
        }

        public MYB60ResistanceMappingSnapshot MapResistance(MYB60ResistanceMappingInput input)
        {
            NormalizeSettings();
            lastMappingSnapshot = Map(
                input,
                hasLastMappingSnapshot ? lastMappingSnapshot.SmoothedResistance01 : input.TargetResistance01,
                hasLastMappingSnapshot,
                minResistance01,
                maxResistance01,
                warmupMaxResistance01,
                recoveryMaxResistance01,
                sprintMaxResistance01,
                rampUpPerSecond01,
                rampDownPerSecond01);
            hasLastMappingSnapshot = true;
            return lastMappingSnapshot;
        }

        public static MYB60ResistanceMappingSnapshot LocalFallback(
            MYB60ResistanceMappingInput input,
            float previousSmoothedResistance01,
            bool hasPrevious)
        {
            return Map(
                input,
                previousSmoothedResistance01,
                hasPrevious,
                DefaultMinResistance01,
                DefaultMaxResistance01,
                DefaultWarmupMaxResistance01,
                DefaultRecoveryMaxResistance01,
                DefaultSprintMaxResistance01,
                DefaultRampUpPerSecond01,
                DefaultRampDownPerSecond01);
        }

        public static MYB60ResistanceMappingSnapshot Map(
            MYB60ResistanceMappingInput input,
            float previousSmoothedResistance01,
            bool hasPrevious,
            float minResistance01,
            float maxResistance01,
            float warmupMaxResistance01,
            float recoveryMaxResistance01,
            float sprintMaxResistance01,
            float rampUpPerSecond01,
            float rampDownPerSecond01)
        {
            var normalizedMin = Mathf.Clamp01(Mathf.Min(minResistance01, maxResistance01));
            var normalizedMax = Mathf.Clamp01(Mathf.Max(minResistance01, maxResistance01));
            var segmentMax = SegmentMaxResistance(
                input.RouteSegment,
                normalizedMax,
                warmupMaxResistance01,
                recoveryMaxResistance01,
                sprintMaxResistance01);
            segmentMax = Mathf.Max(normalizedMin, segmentMax);
            var limitedResistance01 = Mathf.Clamp(input.TargetResistance01, normalizedMin, segmentMax);
            var smoothedResistance01 = limitedResistance01;
            if (hasPrevious)
            {
                var previous = Mathf.Clamp(previousSmoothedResistance01, normalizedMin, segmentMax);
                if (input.DeltaTimeSeconds > 0f)
                {
                    var rate = limitedResistance01 > previous ? rampUpPerSecond01 : rampDownPerSecond01;
                    smoothedResistance01 = Mathf.MoveTowards(
                        previous,
                        limitedResistance01,
                        Mathf.Max(0f, rate) * input.DeltaTimeSeconds);
                }
                else
                {
                    smoothedResistance01 = previous;
                }
            }

            var status = MYB60ResistanceMappingStatus.Stable;
            if (Mathf.Abs(limitedResistance01 - input.TargetResistance01) > 0.0001f)
            {
                status = MYB60ResistanceMappingStatus.Limited;
            }
            else if (Mathf.Abs(smoothedResistance01 - limitedResistance01) > 0.0001f)
            {
                status = MYB60ResistanceMappingStatus.Ramping;
            }

            return new MYB60ResistanceMappingSnapshot(input, limitedResistance01, smoothedResistance01, status);
        }

        private static float SegmentMaxResistance(
            MYB57RouteSegment routeSegment,
            float maxResistance01,
            float warmupMaxResistance01,
            float recoveryMaxResistance01,
            float sprintMaxResistance01)
        {
            switch (routeSegment)
            {
                case MYB57RouteSegment.Warmup:
                    return Mathf.Min(maxResistance01, Mathf.Clamp01(warmupMaxResistance01));
                case MYB57RouteSegment.Recovery:
                    return Mathf.Min(maxResistance01, Mathf.Clamp01(recoveryMaxResistance01));
                case MYB57RouteSegment.Sprint:
                    return Mathf.Min(maxResistance01, Mathf.Clamp01(sprintMaxResistance01));
                default:
                    return maxResistance01;
            }
        }

        private void NormalizeSettings()
        {
            minResistance01 = Mathf.Clamp01(minResistance01);
            maxResistance01 = Mathf.Clamp01(maxResistance01);
            if (minResistance01 > maxResistance01)
            {
                (minResistance01, maxResistance01) = (maxResistance01, minResistance01);
            }

            warmupMaxResistance01 = Mathf.Clamp(warmupMaxResistance01, minResistance01, maxResistance01);
            recoveryMaxResistance01 = Mathf.Clamp(recoveryMaxResistance01, minResistance01, maxResistance01);
            sprintMaxResistance01 = Mathf.Clamp(sprintMaxResistance01, minResistance01, maxResistance01);
            rampUpPerSecond01 = Mathf.Max(0f, rampUpPerSecond01);
            rampDownPerSecond01 = Mathf.Max(0f, rampDownPerSecond01);
        }
    }
}

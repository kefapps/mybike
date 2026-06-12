using System;
using UnityEngine;

namespace MYB57
{
    public enum MYB57TrainerMode
    {
        Simulated,
        ReadOnly,
        Controllable
    }

    public enum MYB57TrainerSourcePreset
    {
        PowerAvailable,
        CadenceResistance,
        CadenceOnly,
        ManualFallback
    }

    public enum MYB57RouteSegment
    {
        Warmup,
        Climb,
        Sprint,
        Recovery
    }

    public readonly struct MYB57EffortInput
    {
        public MYB57EffortInput(
            MYB57TrainerMode trainerMode,
            MYB57TrainerSourcePreset sourcePreset,
            MYB57RouteSegment routeSegment,
            float gradePercent,
            float previousFatigue01,
            float deltaTimeSeconds)
        {
            TrainerMode = trainerMode;
            SourcePreset = sourcePreset;
            RouteSegment = routeSegment;
            GradePercent = gradePercent;
            PreviousFatigue01 = Mathf.Clamp01(previousFatigue01);
            DeltaTimeSeconds = Mathf.Max(0f, deltaTimeSeconds);
        }

        public MYB57TrainerMode TrainerMode { get; }
        public MYB57TrainerSourcePreset SourcePreset { get; }
        public MYB57RouteSegment RouteSegment { get; }
        public float GradePercent { get; }
        public float PreviousFatigue01 { get; }
        public float DeltaTimeSeconds { get; }
    }

    public readonly struct MYB57TrainerSample
    {
        public MYB57TrainerSample(
            bool hasPowerWatts,
            float powerWatts,
            bool hasCadenceRpm,
            float cadenceRpm,
            bool hasResistanceLevel,
            float resistanceLevel,
            float elapsedTimeSeconds,
            bool isEstimated = false,
            float estimatedEffort01 = 0f)
        {
            HasPowerWatts = hasPowerWatts;
            PowerWatts = Mathf.Max(0f, powerWatts);
            HasCadenceRpm = hasCadenceRpm;
            CadenceRpm = Mathf.Max(0f, cadenceRpm);
            HasResistanceLevel = hasResistanceLevel;
            ResistanceLevel = Mathf.Clamp(resistanceLevel, 0f, 100f);
            ElapsedTimeSeconds = Mathf.Max(0f, elapsedTimeSeconds);
            IsEstimated = isEstimated;
            EstimatedEffort01 = Mathf.Clamp01(estimatedEffort01);
        }

        public bool HasPowerWatts { get; }
        public float PowerWatts { get; }
        public bool HasCadenceRpm { get; }
        public float CadenceRpm { get; }
        public bool HasResistanceLevel { get; }
        public float ResistanceLevel { get; }
        public float ElapsedTimeSeconds { get; }
        public bool IsEstimated { get; }
        public float EstimatedEffort01 { get; }
    }

    public readonly struct MYB57EffortSnapshot
    {
        public MYB57EffortSnapshot(
            MYB57TrainerMode trainerMode,
            MYB57TrainerSourcePreset sourcePreset,
            MYB57RouteSegment routeSegment,
            float gradePercent,
            float targetResistance01,
            int targetResistanceLevel,
            MYB57TrainerSample trainerSample,
            float pedalEffort01,
            float fatigue01,
            float speedMetersPerSecond,
            string sourceBadge,
            string effortLabel)
        {
            TrainerMode = trainerMode;
            SourcePreset = sourcePreset;
            RouteSegment = routeSegment;
            GradePercent = gradePercent;
            TargetResistance01 = Mathf.Clamp01(targetResistance01);
            TargetResistanceLevel = Mathf.Clamp(targetResistanceLevel, 0, 100);
            TrainerSample = trainerSample;
            PedalEffort01 = Mathf.Clamp01(pedalEffort01);
            Fatigue01 = Mathf.Clamp01(fatigue01);
            SpeedMetersPerSecond = Mathf.Max(0f, speedMetersPerSecond);
            SourceBadge = sourceBadge ?? string.Empty;
            EffortLabel = effortLabel ?? string.Empty;
        }

        public MYB57TrainerMode TrainerMode { get; }
        public MYB57TrainerSourcePreset SourcePreset { get; }
        public MYB57RouteSegment RouteSegment { get; }
        public float GradePercent { get; }
        public float TargetResistance01 { get; }
        public int TargetResistanceLevel { get; }
        public MYB57TrainerSample TrainerSample { get; }
        public float PedalEffort01 { get; }
        public float Fatigue01 { get; }
        public float SpeedMetersPerSecond { get; }
        public string SourceBadge { get; }
        public string EffortLabel { get; }
    }

    public static class MYB57EffortSimulator
    {
        public const float BaseSpeedMetersPerSecond = 12.5f;

        public static MYB57RouteSegment SegmentForProgress(float progress01)
        {
            if (progress01 < 0.26f)
            {
                return MYB57RouteSegment.Warmup;
            }

            if (progress01 < 0.56f)
            {
                return MYB57RouteSegment.Climb;
            }

            if (progress01 < 0.84f)
            {
                return MYB57RouteSegment.Sprint;
            }

            return MYB57RouteSegment.Recovery;
        }

        public static MYB57EffortSnapshot EvaluateMock(MYB57EffortInput input)
        {
            var targetResistance01 = TargetResistanceFor(input.RouteSegment, input.GradePercent);
            var targetResistanceLevel = ResistanceLevel(targetResistance01);
            var sample = MockTrainerSample(input.SourcePreset, input.RouteSegment, targetResistanceLevel, input.DeltaTimeSeconds);
            return Evaluate(input, sample);
        }

        public static MYB57EffortSnapshot EvaluateManualFallback(MYB57EffortInput input, float manualEffort01)
        {
            var effort01 = Mathf.Clamp01(manualEffort01);
            var cadenceRpm = Mathf.Lerp(54f, 106f, effort01);
            var sample = new MYB57TrainerSample(
                false,
                0f,
                true,
                cadenceRpm,
                false,
                0f,
                input.DeltaTimeSeconds,
                true,
                effort01);
            return Evaluate(input, sample);
        }

        public static MYB57EffortSnapshot Evaluate(MYB57EffortInput input, MYB57TrainerSample sample)
        {
            var targetResistance01 = TargetResistanceFor(input.RouteSegment, input.GradePercent);
            var targetResistanceLevel = ResistanceLevel(targetResistance01);
            var measuredEffort01 = MeasuredEffort(sample);
            var nextFatigue01 = NextFatigue(input, measuredEffort01, targetResistance01);
            var speed = SpeedFor(input.RouteSegment, measuredEffort01, targetResistance01, nextFatigue01, sample);

            return new MYB57EffortSnapshot(
                input.TrainerMode,
                input.SourcePreset,
                input.RouteSegment,
                input.GradePercent,
                targetResistance01,
                targetResistanceLevel,
                sample,
                measuredEffort01,
                nextFatigue01,
                speed,
                SourceBadge(sample),
                EffortLabel(measuredEffort01, targetResistance01, nextFatigue01));
        }

        public static float TargetResistanceFor(MYB57RouteSegment segment, float gradePercent)
        {
            var segmentBase = segment switch
            {
                MYB57RouteSegment.Warmup => 0.24f,
                MYB57RouteSegment.Climb => 0.66f,
                MYB57RouteSegment.Sprint => 0.72f,
                MYB57RouteSegment.Recovery => 0.18f,
                _ => 0.35f
            };

            var gradeInfluence = Mathf.Clamp(gradePercent / 12f, -0.28f, 0.32f);
            if (segment == MYB57RouteSegment.Recovery && gradeInfluence > 0f)
            {
                gradeInfluence *= 0.35f;
            }

            return Mathf.Clamp01(segmentBase + gradeInfluence);
        }

        public static int ResistanceLevel(float targetResistance01)
        {
            return Mathf.RoundToInt(Mathf.Clamp01(targetResistance01) * 100f);
        }

        private static MYB57TrainerSample MockTrainerSample(
            MYB57TrainerSourcePreset preset,
            MYB57RouteSegment segment,
            int targetResistanceLevel,
            float elapsedTimeSeconds)
        {
            var cadenceRpm = segment switch
            {
                MYB57RouteSegment.Warmup => 82f,
                MYB57RouteSegment.Climb => 72f,
                MYB57RouteSegment.Sprint => 98f,
                MYB57RouteSegment.Recovery => 76f,
                _ => 80f
            };

            var powerWatts = segment switch
            {
                MYB57RouteSegment.Warmup => 145f,
                MYB57RouteSegment.Climb => 220f,
                MYB57RouteSegment.Sprint => 285f,
                MYB57RouteSegment.Recovery => 95f,
                _ => 140f
            };

            switch (preset)
            {
                case MYB57TrainerSourcePreset.PowerAvailable:
                    return new MYB57TrainerSample(true, powerWatts, true, cadenceRpm, true, targetResistanceLevel, elapsedTimeSeconds);
                case MYB57TrainerSourcePreset.CadenceResistance:
                    return new MYB57TrainerSample(false, 0f, true, cadenceRpm, true, targetResistanceLevel, elapsedTimeSeconds);
                case MYB57TrainerSourcePreset.CadenceOnly:
                    return new MYB57TrainerSample(false, 0f, true, cadenceRpm, false, 0f, elapsedTimeSeconds);
                case MYB57TrainerSourcePreset.ManualFallback:
                    return new MYB57TrainerSample(false, 0f, true, 78f, false, 0f, elapsedTimeSeconds, true, 0.45f);
                default:
                    throw new ArgumentOutOfRangeException(nameof(preset), preset, null);
            }
        }

        private static float MeasuredEffort(MYB57TrainerSample sample)
        {
            if (sample.IsEstimated)
            {
                return sample.EstimatedEffort01;
            }

            if (sample.HasPowerWatts)
            {
                return Mathf.InverseLerp(55f, 340f, sample.PowerWatts);
            }

            if (sample.HasCadenceRpm && sample.HasResistanceLevel)
            {
                var cadence01 = Mathf.InverseLerp(35f, 115f, sample.CadenceRpm);
                var resistance01 = Mathf.Clamp01(sample.ResistanceLevel / 100f);
                return Mathf.Clamp01(cadence01 * Mathf.Lerp(0.48f, 1.08f, resistance01));
            }

            if (sample.HasCadenceRpm)
            {
                return Mathf.Clamp01(Mathf.InverseLerp(35f, 115f, sample.CadenceRpm) * 0.82f);
            }

            return 0f;
        }

        private static float NextFatigue(MYB57EffortInput input, float measuredEffort01, float targetResistance01)
        {
            var load01 = measuredEffort01 * Mathf.Lerp(0.35f, 1f, targetResistance01);
            var fatigue = input.PreviousFatigue01;
            var delta = input.DeltaTimeSeconds;

            const float fatigueBuildThreshold = 0.52f;
            const float fatigueRecoverThreshold = 0.58f;

            if (load01 > fatigueBuildThreshold)
            {
                fatigue += (load01 - fatigueBuildThreshold) * 0.18f * delta;
            }
            else
            {
                fatigue -= (fatigueRecoverThreshold - load01) * 0.12f * delta;
            }

            if (input.RouteSegment == MYB57RouteSegment.Recovery || input.GradePercent < -0.25f)
            {
                fatigue -= 0.08f * delta;
            }

            return Mathf.Clamp01(fatigue);
        }

        private static float SpeedFor(
            MYB57RouteSegment segment,
            float measuredEffort01,
            float targetResistance01,
            float fatigue01,
            MYB57TrainerSample sample)
        {
            if ((!sample.HasPowerWatts && !sample.HasCadenceRpm) || measuredEffort01 <= 0.01f)
            {
                return segment == MYB57RouteSegment.Recovery ? 0.7f : 0f;
            }

            var effortFactor = Mathf.Lerp(0.28f, 1.32f, measuredEffort01);
            var resistanceFactor = Mathf.Lerp(1.14f, 0.74f, targetResistance01);
            var fatigueFactor = Mathf.Lerp(1f, 0.78f, fatigue01);
            var coastBonus = segment == MYB57RouteSegment.Recovery && measuredEffort01 > 0.15f ? 0.08f : 0f;
            return BaseSpeedMetersPerSecond * Mathf.Clamp(effortFactor * resistanceFactor * fatigueFactor + coastBonus, 0.05f, 1.45f);
        }

        private static string SourceBadge(MYB57TrainerSample sample)
        {
            if (sample.IsEstimated)
            {
                return "Manual";
            }

            if (sample.HasPowerWatts)
            {
                return "Power";
            }

            return sample.HasResistanceLevel ? "Cad+Res" : "Cadence";
        }

        private static string EffortLabel(float measuredEffort01, float targetResistance01, float fatigue01)
        {
            if (fatigue01 > 0.72f)
            {
                return "Fatigued";
            }

            if (targetResistance01 >= 0.56f && measuredEffort01 >= 0.5f)
            {
                return "Sustained climb";
            }

            if (measuredEffort01 > 0.76f)
            {
                return "Hard sprint";
            }

            if (targetResistance01 < 0.26f)
            {
                return "Recovery";
            }

            return "Easy spin";
        }
    }
}

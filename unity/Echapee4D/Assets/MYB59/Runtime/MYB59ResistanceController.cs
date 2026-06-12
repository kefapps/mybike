using UnityEngine;

namespace MYB59
{
    public enum MYB59ResistanceControllerStatus
    {
        Applied,
        Fallback,
        Unavailable
    }

    public readonly struct MYB59ResistanceDemand
    {
        public MYB59ResistanceDemand(float resistance01, string sourceLabel)
        {
            Resistance01 = Mathf.Clamp01(resistance01);
            SourceLabel = sourceLabel ?? string.Empty;
        }

        public float Resistance01 { get; }
        public int ResistanceLevel => Mathf.RoundToInt(Resistance01 * 100f);
        public string SourceLabel { get; }

        public static MYB59ResistanceDemand FromLevel(int resistanceLevel, string sourceLabel)
        {
            return new MYB59ResistanceDemand(Mathf.Clamp(resistanceLevel, 0, 100) / 100f, sourceLabel);
        }
    }

    public readonly struct MYB59ResistanceSnapshot
    {
        public MYB59ResistanceSnapshot(
            MYB59ResistanceDemand demand,
            float appliedResistance01,
            MYB59ResistanceControllerStatus status,
            string controllerLabel)
        {
            Demand = demand;
            AppliedResistance01 = Mathf.Clamp01(appliedResistance01);
            Status = status;
            ControllerLabel = controllerLabel ?? string.Empty;
        }

        public MYB59ResistanceDemand Demand { get; }
        public float AppliedResistance01 { get; }
        public int AppliedResistanceLevel => Mathf.RoundToInt(AppliedResistance01 * 100f);
        public MYB59ResistanceControllerStatus Status { get; }
        public string ControllerLabel { get; }
        public bool IsApplied => Status == MYB59ResistanceControllerStatus.Applied;
        public bool IsFallback => Status == MYB59ResistanceControllerStatus.Fallback;
        public bool IsUnavailable => Status == MYB59ResistanceControllerStatus.Unavailable;

        public string StatusLabel
        {
            get
            {
                switch (Status)
                {
                    case MYB59ResistanceControllerStatus.Applied:
                        return "Applied";
                    case MYB59ResistanceControllerStatus.Fallback:
                        return "Fallback";
                    case MYB59ResistanceControllerStatus.Unavailable:
                        return "Unavailable";
                    default:
                        return "Unknown";
                }
            }
        }
    }

    public interface IMYB59ResistanceController
    {
        MYB59ResistanceSnapshot LastResistanceSnapshot { get; }
        MYB59ResistanceSnapshot ApplyResistance(MYB59ResistanceDemand demand, float deltaTimeSeconds);
    }

    public sealed class MYB59ResistanceController : MonoBehaviour, IMYB59ResistanceController
    {
        public const float DefaultFallbackResistance01 = 0.2f;

        [SerializeField]
        private bool controllerAvailable = true;

        [SerializeField]
        private bool useFallbackWhenUnavailable = true;

        [SerializeField]
        [Range(0f, 1f)]
        private float fallbackResistance01 = DefaultFallbackResistance01;

        [SerializeField]
        private string controllerLabel = "Mock resistance";

        private MYB59ResistanceSnapshot lastResistanceSnapshot;

        public bool ControllerAvailable
        {
            get => controllerAvailable;
            set => controllerAvailable = value;
        }

        public bool UseFallbackWhenUnavailable
        {
            get => useFallbackWhenUnavailable;
            set => useFallbackWhenUnavailable = value;
        }

        public float FallbackResistance01
        {
            get => fallbackResistance01;
            set => fallbackResistance01 = Mathf.Clamp01(value);
        }

        public string ControllerLabel
        {
            get => controllerLabel;
            set => controllerLabel = value ?? string.Empty;
        }

        public MYB59ResistanceSnapshot LastResistanceSnapshot => lastResistanceSnapshot;

        private void Awake()
        {
            fallbackResistance01 = Mathf.Clamp01(fallbackResistance01);
            lastResistanceSnapshot = new MYB59ResistanceSnapshot(
                new MYB59ResistanceDemand(fallbackResistance01, "initial fallback"),
                fallbackResistance01,
                MYB59ResistanceControllerStatus.Fallback,
                controllerLabel);
        }

        private void OnValidate()
        {
            fallbackResistance01 = Mathf.Clamp01(fallbackResistance01);
            controllerLabel ??= string.Empty;
        }

        public MYB59ResistanceSnapshot ApplyResistance(MYB59ResistanceDemand demand, float deltaTimeSeconds)
        {
            if (controllerAvailable)
            {
                lastResistanceSnapshot = new MYB59ResistanceSnapshot(
                    demand,
                    demand.Resistance01,
                    MYB59ResistanceControllerStatus.Applied,
                    controllerLabel);
                return lastResistanceSnapshot;
            }

            if (useFallbackWhenUnavailable)
            {
                lastResistanceSnapshot = new MYB59ResistanceSnapshot(
                    demand,
                    fallbackResistance01,
                    MYB59ResistanceControllerStatus.Fallback,
                    controllerLabel);
                return lastResistanceSnapshot;
            }

            lastResistanceSnapshot = new MYB59ResistanceSnapshot(
                demand,
                0f,
                MYB59ResistanceControllerStatus.Unavailable,
                controllerLabel);
            return lastResistanceSnapshot;
        }

        public static MYB59ResistanceSnapshot LocalFallback(MYB59ResistanceDemand demand)
        {
            return new MYB59ResistanceSnapshot(
                demand,
                DefaultFallbackResistance01,
                MYB59ResistanceControllerStatus.Fallback,
                "Local fallback");
        }
    }
}

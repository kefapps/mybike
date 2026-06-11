export type ChallengeKind = "daily" | "climb" | "exploration" | "chrono";

export type ChallengeTargetKind =
  | "distance"
  | "explore"
  | "climb"
  | "beat-time";

export type ChallengeTarget =
  | { kind: "distance"; meters: number }
  | { kind: "explore"; progress01: number }
  | { kind: "climb"; minEffort01: number; seconds: number }
  | { kind: "beat-time"; maxSeconds: number; minAverageSpeedMps: number };

export type ChallengeReward = {
  xp: number;
  badgeId: string | null;
};

export type Challenge = {
  id: string;
  kind: ChallengeKind;
  title: string;
  description: string;
  target: ChallengeTarget;
  reward: ChallengeReward;
  seed: number;
  issuedAtMs: number;
  expiresAtMs: number;
};

export type ChallengeProgress = {
  challengeId: string;
  meters: number;
  seconds: number;
  progress01: number;
  effortAboveThresholdSeconds: number;
  effortAboveThreshold: boolean;
  averageSpeedMps: number;
  completion01: number;
};

export type ChallengeResultReason =
  | "completed"
  | "missed-distance"
  | "missed-explore"
  | "missed-effort"
  | "missed-time"
  | "abandoned";

export type ChallengeResult = {
  challengeId: string;
  success: boolean;
  reason: ChallengeResultReason;
  finalProgress: ChallengeProgress;
  reward: ChallengeReward;
};

export const EMPTY_CHALLENGE_PROGRESS: ChallengeProgress = Object.freeze({
  challengeId: "",
  meters: 0,
  seconds: 0,
  progress01: 0,
  effortAboveThresholdSeconds: 0,
  effortAboveThreshold: false,
  averageSpeedMps: 0,
  completion01: 0
});

export const NO_CHALLENGE_REWARD: ChallengeReward = Object.freeze({
  xp: 0,
  badgeId: null
});

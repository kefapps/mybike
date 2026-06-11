import { isChallengeCompleteAt } from "./challengeProgress";
import type {
  Challenge,
  ChallengeProgress,
  ChallengeResult,
  ChallengeResultReason,
  ChallengeTarget
} from "./challengeTypes";

function reasonFor(
  target: ChallengeTarget,
  progress: ChallengeProgress
): ChallengeResultReason {
  switch (target.kind) {
    case "distance":
      return progress.meters >= target.meters
        ? "completed"
        : "missed-distance";
    case "explore":
      return progress.progress01 + 1e-6 >= target.progress01
        ? "completed"
        : "missed-explore";
    case "climb":
      return progress.effortAboveThresholdSeconds >= target.seconds
        ? "completed"
        : "missed-effort";
    case "beat-time":
      if (progress.averageSpeedMps + 1e-6 < target.minAverageSpeedMps) {
        return "missed-time";
      }

      if (progress.seconds > target.maxSeconds) {
        return "missed-time";
      }

      return "completed";
    default:
      return "abandoned";
  }
}

export function evaluateChallenge(
  challenge: Challenge,
  finalProgress: ChallengeProgress,
  options: { abandoned?: boolean } = {}
): ChallengeResult {
  const isComplete = !options.abandoned && isChallengeCompleteAt(challenge, finalProgress);
  const reason: ChallengeResultReason = options.abandoned
    ? "abandoned"
    : isComplete
    ? "completed"
    : reasonFor(challenge.target, finalProgress);
  const success = reason === "completed";

  return {
    challengeId: challenge.id,
    success,
    reason,
    finalProgress,
    reward: success ? challenge.reward : { xp: 0, badgeId: null }
  };
}

export function describeChallengeResult(reason: ChallengeResultReason): string {
  switch (reason) {
    case "completed":
      return "Defi reussi";
    case "missed-distance":
      return "Distance insuffisante";
    case "missed-explore":
      return "Progression insuffisante";
    case "missed-effort":
      return "Effort maintenu trop court";
    case "missed-time":
      return "Temps trop long";
    case "abandoned":
      return "Defi abandonne";
    default:
      return "Defi";
  }
}

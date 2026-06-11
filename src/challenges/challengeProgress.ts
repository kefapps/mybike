import type { RideFrameSnapshot } from "../ride";
import type { Challenge, ChallengeProgress, ChallengeTarget } from "./challengeTypes";

function safeNumber(value: number, fallback: number): number {
  if (!Number.isFinite(value)) {
    return fallback;
  }

  return Math.max(0, value);
}

function safeRange(value: number, min: number, max: number): number {
  if (!Number.isFinite(value)) {
    return min;
  }

  return Math.min(max, Math.max(min, value));
}

function getEffort01(snapshot: RideFrameSnapshot): number {
  return safeRange(snapshot.input.effort01, 0, 1);
}

function getPreviousProgress(
  challenge: Challenge,
  previous: ChallengeProgress | undefined
): ChallengeProgress {
  if (previous && previous.challengeId === challenge.id) {
    return previous;
  }

  return {
    challengeId: challenge.id,
    meters: 0,
    seconds: 0,
    progress01: 0,
    effortAboveThresholdSeconds: 0,
    effortAboveThreshold: false,
    averageSpeedMps: 0,
    completion01: 0
  };
}

function computeCompletion01(
  target: ChallengeTarget,
  next: { meters: number; seconds: number; progress01: number }
): number {
  switch (target.kind) {
    case "distance":
      return target.meters > 0
        ? safeRange(next.meters / target.meters, 0, 1)
        : 0;
    case "explore":
      return safeRange(next.progress01 / target.progress01, 0, 1);
    case "climb":
      return target.seconds > 0
        ? safeRange(next.seconds / target.seconds, 0, 1)
        : 0;
    case "beat-time":
      return target.maxSeconds > 0
        ? safeRange(next.seconds / target.maxSeconds, 0, 1)
        : 0;
    default:
      return 0;
  }
}

export function updateChallengeProgress(
  challenge: Challenge,
  previous: ChallengeProgress | undefined,
  snapshot: RideFrameSnapshot,
  deltaSecondsInput: number
): ChallengeProgress {
  const safeDelta = safeNumber(deltaSecondsInput, 0);
  const base = getPreviousProgress(challenge, previous);
  const meters = safeNumber(snapshot.progress.distanceMeters, base.meters);
  const seconds = safeNumber(snapshot.elapsedMs / 1000, base.seconds);
  const progress01 = safeRange(snapshot.progress.progress01, 0, 1);
  const averageSpeedMps = seconds > 0 ? meters / seconds : 0;
  const effort01 = getEffort01(snapshot);

  let effortAboveThresholdSeconds = base.effortAboveThresholdSeconds;
  let effortAboveThreshold = base.effortAboveThreshold;

  if (challenge.target.kind === "climb" && safeDelta > 0) {
    const threshold = challenge.target.minEffort01;
    const above = effort01 >= threshold;

    if (above) {
      effortAboveThresholdSeconds = safeNumber(
        base.effortAboveThresholdSeconds + safeDelta,
        safeDelta
      );
    }

    effortAboveThreshold = above;
  }

  const next: ChallengeProgress = {
    challengeId: challenge.id,
    meters,
    seconds,
    progress01,
    effortAboveThresholdSeconds,
    effortAboveThreshold,
    averageSpeedMps,
    completion01: 0
  };

  next.completion01 = computeCompletion01(challenge.target, next);

  return next;
}

export function isChallengeCompleteAt(
  challenge: Challenge,
  progress: ChallengeProgress
): boolean {
  switch (challenge.target.kind) {
    case "distance":
      return progress.meters >= challenge.target.meters;
    case "explore":
      return progress.progress01 + 1e-6 >= challenge.target.progress01;
    case "climb":
      return progress.effortAboveThresholdSeconds >= challenge.target.seconds;
    case "beat-time":
      return (
        progress.seconds <= challenge.target.maxSeconds &&
        progress.averageSpeedMps + 1e-6 >= challenge.target.minAverageSpeedMps
      );
    default:
      return false;
  }
}

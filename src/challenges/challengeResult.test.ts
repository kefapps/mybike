import { describe, expect, it } from "vitest";

import type { RideFrameSnapshot } from "../ride";
import type {
  Challenge,
  ChallengeProgress,
  ChallengeTarget
} from "./challengeTypes";
import { updateChallengeProgress } from "./challengeProgress";
import { generateDailyChallenge } from "./dailyChallenge";
import {
  describeChallengeResult,
  evaluateChallenge
} from "./challengeResult";

function snapshot(overrides: Partial<RideFrameSnapshot> = {}): RideFrameSnapshot {
  return {
    source: "mock",
    input: { source: "mock", effort01: 0.5, nowMs: 0 },
    targetSpeedMps: 4,
    speedMps: 4,
    progress: {
      distanceMeters: 0,
      progress01: 0,
      completed: false
    },
    elapsedMs: 0,
    stats: { elapsedMs: 0, distanceMeters: 0, averageSpeedMps: 0 },
    completed: false,
    ...overrides
  };
}

function findChallenge<K extends ChallengeTarget["kind"]>(
  targetKind: K
): Challenge & { target: Extract<ChallengeTarget, { kind: K }> } {
  const base = Date.UTC(2026, 0, 1, 12, 0, 0);
  const dayMs = 24 * 60 * 60 * 1000;

  for (let i = 0; i < 365; i += 1) {
    const challenge = generateDailyChallenge(base + i * dayMs);
    if (challenge.target.kind === targetKind) {
      return challenge as Challenge & {
        target: Extract<ChallengeTarget, { kind: K }>;
      };
    }
  }

  throw new Error(`No daily challenge of kind ${targetKind} found`);
}

function makeProgress(
  challengeId: string,
  overrides: Partial<ChallengeProgress> = {}
): ChallengeProgress {
  return {
    challengeId,
    meters: 0,
    seconds: 0,
    progress01: 0,
    effortAboveThresholdSeconds: 0,
    effortAboveThreshold: false,
    averageSpeedMps: 0,
    completion01: 0,
    ...overrides
  };
}

describe("evaluateChallenge", () => {
  it("marks a finished distance challenge as success", () => {
    const challenge = findChallenge("distance");

    const progress = updateChallengeProgress(
      challenge,
      undefined,
      snapshot({
        progress: {
          distanceMeters: challenge.target.meters,
          progress01: 1,
          completed: true
        },
        elapsedMs: 60_000
      }),
      60
    );

    const result = evaluateChallenge(challenge, progress);

    expect(result.success).toBe(true);
    expect(result.reason).toBe("completed");
    expect(result.reward.xp).toBe(challenge.reward.xp);
  });

  it("marks an under-target distance challenge as missed-distance", () => {
    const challenge = findChallenge("distance");
    const progress = makeProgress(challenge.id, {
      meters: Math.max(0, challenge.target.meters - 10)
    });

    const result = evaluateChallenge(challenge, progress);

    expect(result.success).toBe(false);
    expect(result.reason).toBe("missed-distance");
    expect(result.reward.xp).toBe(0);
  });

  it("marks a climb challenge with insufficient effort as missed-effort", () => {
    const challenge = findChallenge("climb");
    const progress = makeProgress(challenge.id, {
      effortAboveThresholdSeconds: Math.max(
        0,
        challenge.target.seconds - 5
      )
    });

    const result = evaluateChallenge(challenge, progress);

    expect(result.success).toBe(false);
    expect(result.reason).toBe("missed-effort");
  });

  it("marks a chrono challenge with too low average speed as missed-time", () => {
    const challenge = findChallenge("beat-time");
    const progress = makeProgress(challenge.id, {
      seconds: challenge.target.maxSeconds - 10,
      averageSpeedMps: challenge.target.minAverageSpeedMps - 0.5
    });

    const result = evaluateChallenge(challenge, progress);

    expect(result.success).toBe(false);
    expect(result.reason).toBe("missed-time");
  });

  it("marks an abandoned session as abandoned with no reward", () => {
    const challenge = findChallenge("distance");
    const progress = makeProgress(challenge.id, {
      meters: challenge.target.meters
    });

    const result = evaluateChallenge(challenge, progress, { abandoned: true });

    expect(result.success).toBe(false);
    expect(result.reason).toBe("abandoned");
    expect(result.reward.xp).toBe(0);
  });
});

describe("describeChallengeResult", () => {
  it("returns a human readable label for every reason", () => {
    expect(describeChallengeResult("completed")).toBeTruthy();
    expect(describeChallengeResult("missed-distance")).toBeTruthy();
    expect(describeChallengeResult("missed-explore")).toBeTruthy();
    expect(describeChallengeResult("missed-effort")).toBeTruthy();
    expect(describeChallengeResult("missed-time")).toBeTruthy();
    expect(describeChallengeResult("abandoned")).toBeTruthy();
  });
});

import { describe, expect, it } from "vitest";

import type { RideFrameSnapshot } from "../ride";
import type {
  Challenge,
  ChallengeProgress,
  ChallengeTarget
} from "./challengeTypes";
import {
  isChallengeCompleteAt,
  updateChallengeProgress
} from "./challengeProgress";
import { generateDailyChallenge } from "./dailyChallenge";

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

describe("updateChallengeProgress", () => {
  it("initializes a fresh progress when no previous state matches", () => {
    const challenge = findChallenge("distance");
    const previous: ChallengeProgress = {
      challengeId: "other",
      meters: 100,
      seconds: 50,
      progress01: 0.5,
      effortAboveThresholdSeconds: 10,
      effortAboveThreshold: true,
      averageSpeedMps: 2,
      completion01: 0.5
    };

    const next = updateChallengeProgress(
      challenge,
      previous,
      snapshot({
        progress: { distanceMeters: 0, progress01: 0, completed: false }
      }),
      0
    );

    expect(next.challengeId).toBe(challenge.id);
    expect(next.meters).toBe(0);
    expect(next.seconds).toBe(0);
    expect(next.completion01).toBe(0);
  });

  it("tracks distance and progress for a distance challenge", () => {
    const challenge = findChallenge("distance");
    const next = updateChallengeProgress(
      challenge,
      undefined,
      snapshot({
        progress: { distanceMeters: 200, progress01: 0.2, completed: false },
        elapsedMs: 30_000
      }),
      30
    );

    expect(next.meters).toBe(200);
    expect(next.seconds).toBe(30);
    expect(next.completion01).toBeGreaterThan(0);
    expect(next.averageSpeedMps).toBeCloseTo(200 / 30, 5);
  });

  it("accumulates effort-above-threshold seconds for climb challenges", () => {
    const challenge = findChallenge("climb");
    const baseSnapshot = snapshot({
      input: { source: "mock", effort01: 0.7, nowMs: 0 },
      progress: { distanceMeters: 0, progress01: 0, completed: false },
      elapsedMs: 1_000
    });

    const after = updateChallengeProgress(challenge, undefined, baseSnapshot, 1);

    expect(after.effortAboveThreshold).toBe(
      0.7 >= challenge.target.minEffort01
    );
  });

  it("completes the distance challenge when the target is met", () => {
    const challenge = findChallenge("distance");
    const target = challenge.target.meters;

    const final = updateChallengeProgress(
      challenge,
      undefined,
      snapshot({
        progress: {
          distanceMeters: target,
          progress01: 1,
          completed: true
        },
        elapsedMs: 60_000
      }),
      60
    );

    expect(isChallengeCompleteAt(challenge, final)).toBe(true);
    expect(final.completion01).toBe(1);
  });

  it("does not complete the distance challenge when under target", () => {
    const challenge = findChallenge("distance");
    const partial = updateChallengeProgress(
      challenge,
      undefined,
      snapshot({
        progress: {
          distanceMeters: Math.max(0, challenge.target.meters - 50),
          progress01: 0.1,
          completed: false
        },
        elapsedMs: 30_000
      }),
      30
    );

    expect(isChallengeCompleteAt(challenge, partial)).toBe(false);
    expect(partial.completion01).toBeLessThan(1);
  });

  it("clamps invalid delta and progress values without crashing", () => {
    const challenge = findChallenge("distance");
    const next = updateChallengeProgress(
      challenge,
      undefined,
      snapshot({
        progress: {
          distanceMeters: Number.NaN,
          progress01: Number.NaN,
          completed: false
        },
        elapsedMs: Number.NaN
      }),
      Number.NaN
    );

    expect(next.meters).toBe(0);
    expect(next.seconds).toBe(0);
    expect(next.completion01).toBe(0);
  });
});

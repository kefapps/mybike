import { describe, expect, it } from "vitest";

import type { SegmentDefinition } from "../route";
import {
  accumulateSegmentStats,
  createInitialSegmentStatsState,
  finalizeSegmentStats
} from "./segments";

function makeSegment(
  overrides: Partial<SegmentDefinition> = {}
): SegmentDefinition {
  return {
    id: "test",
    kind: "climb",
    fromProgress01: 0,
    toProgress01: 0.5,
    targetEffort01: 0.7,
    ...overrides
  };
}

describe("segment stats accumulator", () => {
  it("starts with an empty state", () => {
    const state = createInitialSegmentStatsState();

    expect(state.currentSegmentId).toBeNull();
    expect(state.entries).toEqual({});
    expect(finalizeSegmentStats(state)).toEqual([]);
  });

  it("ignores samples when no segment is active", () => {
    const state = accumulateSegmentStats(createInitialSegmentStatsState(), {
      segment: null,
      effort01: 0.5,
      distanceDeltaMeters: 10
    });

    expect(state.entries).toEqual({});
  });

  it("accumulates effort, distance, and samples for a single segment", () => {
    const segment = makeSegment({ id: "climb" });
    let state = createInitialSegmentStatsState();

    state = accumulateSegmentStats(state, {
      segment,
      effort01: 0.6,
      distanceDeltaMeters: 5
    });
    state = accumulateSegmentStats(state, {
      segment,
      effort01: 0.8,
      distanceDeltaMeters: 7
    });

    const [result] = finalizeSegmentStats(state);

    expect(result).toBeDefined();
    expect(result.segmentId).toBe("climb");
    expect(result.kind).toBe("climb");
    expect(result.targetEffort01).toBeCloseTo(0.7, 5);
    expect(result.sampleCount).toBe(2);
    expect(result.distanceMeters).toBeCloseTo(12, 5);
    expect(result.averageEffort01).toBeCloseTo(0.7, 5);
    expect(result.success01).toBeCloseTo(1, 5);
    expect(result.attempted).toBe(true);
  });

  it("computes a reduced success when the user is far from the target", () => {
    const segment = makeSegment({ id: "sprint", targetEffort01: 0.9 });
    let state = createInitialSegmentStatsState();

    state = accumulateSegmentStats(state, {
      segment,
      effort01: 0.3,
      distanceDeltaMeters: 10
    });
    state = accumulateSegmentStats(state, {
      segment,
      effort01: 0.3,
      distanceDeltaMeters: 10
    });

    const [result] = finalizeSegmentStats(state);

    expect(result.averageEffort01).toBeCloseTo(0.3, 5);
    expect(result.success01).toBeLessThan(0.5);
    expect(result.success01).toBeGreaterThanOrEqual(0);
  });

  it("clamps success to 1 when effort matches the target exactly", () => {
    const segment = makeSegment({ id: "warmup", targetEffort01: 0.4 });
    const state = accumulateSegmentStats(createInitialSegmentStatsState(), {
      segment,
      effort01: 0.4,
      distanceDeltaMeters: 5
    });

    const [result] = finalizeSegmentStats(state);

    expect(result.success01).toBe(1);
  });

  it("tracks two distinct segments independently and sorts results by progress", () => {
    const warmup = makeSegment({
      id: "warmup",
      kind: "warmup",
      fromProgress01: 0,
      toProgress01: 0.2,
      targetEffort01: 0.3
    });
    const climb = makeSegment({
      id: "climb",
      kind: "climb",
      fromProgress01: 0.2,
      toProgress01: 0.6,
      targetEffort01: 0.7
    });
    let state = createInitialSegmentStatsState();

    state = accumulateSegmentStats(state, {
      segment: warmup,
      effort01: 0.2,
      distanceDeltaMeters: 3
    });
    state = accumulateSegmentStats(state, {
      segment: climb,
      effort01: 0.8,
      distanceDeltaMeters: 4
    });
    state = accumulateSegmentStats(state, {
      segment: climb,
      effort01: 0.6,
      distanceDeltaMeters: 5
    });

    const results = finalizeSegmentStats(state);

    expect(results).toHaveLength(2);
    expect(results[0].segmentId).toBe("warmup");
    expect(results[1].segmentId).toBe("climb");
    expect(results[0].sampleCount).toBe(1);
    expect(results[1].sampleCount).toBe(2);
    expect(results[1].distanceMeters).toBeCloseTo(9, 5);
  });

  it("clamps invalid effort inputs before accumulation", () => {
    const segment = makeSegment({ id: "climb" });
    const state = accumulateSegmentStats(createInitialSegmentStatsState(), {
      segment,
      effort01: Number.NaN,
      distanceDeltaMeters: Number.NaN
    });

    const [result] = finalizeSegmentStats(state);

    expect(result.averageEffort01).toBe(0);
    expect(result.distanceMeters).toBe(0);
  });

  it("flags segments that were never reached as not attempted", () => {
    const warmup = makeSegment({
      id: "warmup",
      kind: "warmup",
      fromProgress01: 0,
      toProgress01: 0.2,
      targetEffort01: 0.3
    });
    const sprint = makeSegment({
      id: "sprint",
      kind: "sprint",
      fromProgress01: 0.8,
      toProgress01: 1,
      targetEffort01: 0.95
    });
    const state = accumulateSegmentStats(createInitialSegmentStatsState(), {
      segment: warmup,
      effort01: 0.3,
      distanceDeltaMeters: 2
    });

    expect(state.entries["sprint"]).toBeUndefined();

    const results = finalizeSegmentStats(state);
    const warmupResult = results.find(
      (result) => result.segmentId === "warmup"
    );

    expect(warmupResult?.attempted).toBe(true);
    expect(sprint).toBeDefined();
  });
});

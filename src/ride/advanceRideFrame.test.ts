import { describe, expect, it } from "vitest";

import {
  advanceRideFrame,
  createInitialRideFrameState,
  createMockRideInputSource
} from ".";
import type { RouteDefinition } from "../route/routeTypes";

describe("advanceRideFrame", () => {
  const config = {
    speedMapping: { minSpeedMps: 1, maxSpeedMps: 6 },
    speedSmoothing: {
      accelerationPerSecond: 2,
      decelerationPerSecond: 3
    },
    progress: { totalDistanceMeters: 100 }
  };

  it("composes mock input, speed mapping, smoothing, progress, stats, and snapshot", () => {
    const source = createMockRideInputSource(1);
    const initial = createInitialRideFrameState(1000);

    const first = advanceRideFrame(source, initial, 2000, 1, config);

    expect(first.state).toEqual({
      startedAtMs: 1000,
      speed: { speedMps: 2 },
      progress: { distanceMeters: 2, progress01: 0.02, completed: false },
      segmentStats: { currentSegmentId: null, entries: {} }
    });
    expect(first.snapshot).toEqual({
      source: "mock",
      input: { source: "mock", effort01: 1, nowMs: 2000 },
      targetSpeedMps: 6,
      speedMps: 2,
      progress: { distanceMeters: 2, progress01: 0.02, completed: false },
      elapsedMs: 1000,
      stats: { elapsedMs: 1000, distanceMeters: 2, averageSpeedMps: 2 },
      completed: false
    });

    source.setEffort01(0);
    const second = advanceRideFrame(source, first.state, 3000, 1, config);

    expect(second.snapshot.targetSpeedMps).toBe(1);
    expect(second.snapshot.speedMps).toBe(1);
    expect(second.snapshot.progress.distanceMeters).toBe(3);
    expect(second.snapshot.stats.averageSpeedMps).toBe(1.5);
  });

  it("derives the total distance from the route, removing the hardcoded 1000", () => {
    const source = createMockRideInputSource(1);
    const initial = createInitialRideFrameState(0);
    const route: RouteDefinition = {
      id: "test-route",
      lengthMeters: 1500,
      points: [
        { distanceMeters: 0, position: { x: 0, y: 0, z: 0 } },
        { distanceMeters: 1500, position: { x: 0, y: 0, z: 1500 } }
      ],
      biomes: [{ id: "forest", fromProgress01: 0, toProgress01: 1 }]
    };

    const result = advanceRideFrame(
      source,
      initial,
      10_000,
      1,
      config,
      route
    );

    expect(result.snapshot.progress.distanceMeters).toBe(2);
    expect(result.snapshot.progress.progress01).toBeCloseTo(2 / 1500, 6);
    expect(result.snapshot.completed).toBe(false);
  });

  it("falls back to the config distance when the route is missing or invalid", () => {
    const source = createMockRideInputSource(1);
    const initial = createInitialRideFrameState(0);

    const result = advanceRideFrame(
      source,
      initial,
      1000,
      1,
      config,
      null
    );

    expect(result.snapshot.progress.distanceMeters).toBe(2);
    expect(result.snapshot.progress.progress01).toBeCloseTo(2 / 100, 6);
  });
});

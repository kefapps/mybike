import { describe, expect, it } from "vitest";
import type { RideFrameSnapshot } from "../ride";
import { cameraOnRail } from "./cameraOnRail";
import { createRouteFrameSnapshot } from "./createRouteFrameSnapshot";
import { mockRouteDefinition } from "./mockRouteDefinition";
import type { RouteDefinition } from "./routeTypes";
import { resolveRouteDefinition } from "./routeValidation";
import { sampleRouteAt } from "./sampleRouteAt";
import { selectBiomeAtProgress } from "./selectBiomeAtProgress";

function expectFiniteVec3(position: { x: number; y: number; z: number }): void {
  expect(Number.isFinite(position.x)).toBe(true);
  expect(Number.isFinite(position.y)).toBe(true);
  expect(Number.isFinite(position.z)).toBe(true);
}

describe("route fallback", () => {
  it("resolves missing and invalid routes to a typed deterministic placeholder", () => {
    expect(resolveRouteDefinition(null)).toMatchObject({
      usedFallback: true,
      reason: "missing-route",
      route: { id: "placeholder-route", lengthMeters: 100 }
    });

    expect(
      resolveRouteDefinition({
        ...mockRouteDefinition,
        points: [
          { distanceMeters: 0, position: { x: 0, y: 0, z: 0 } },
          {
            distanceMeters: 1000,
            position: { x: Number.NaN, y: 0, z: 0 }
          }
        ]
      })
    ).toMatchObject({
      usedFallback: true,
      reason: "invalid-points",
      route: { id: "placeholder-route", lengthMeters: 100 }
    });
  });

  it("keeps sampling and camera finite when fallback is used", () => {
    const sample = sampleRouteAt(null, 0.75);
    const camera = cameraOnRail(null, 0.75);

    expect(sample.usedFallback).toBe(true);
    expect(camera.usedFallback).toBe(true);
    expectFiniteVec3(sample.position);
    expectFiniteVec3(camera.position);
    expectFiniteVec3(camera.lookAt);
  });

  it("does not throw on malformed route arrays", () => {
    const malformedPointsRoute = {
      ...mockRouteDefinition,
      points: [
        undefined,
        { distanceMeters: 1000, position: { x: 0, y: 0, z: 1000 } }
      ]
    } as unknown as RouteDefinition;
    const malformedBiomesRoute = {
      ...mockRouteDefinition,
      biomes: [null]
    } as unknown as RouteDefinition;

    expect(resolveRouteDefinition(malformedPointsRoute)).toMatchObject({
      usedFallback: true,
      reason: "invalid-points",
      route: { id: "placeholder-route" }
    });
    expect(resolveRouteDefinition(malformedBiomesRoute)).toMatchObject({
      usedFallback: true,
      reason: "invalid-biomes",
      route: { id: "placeholder-route" }
    });
  });

  it("rejects invalid biome entries instead of selecting them later", () => {
    const routeWithInvalidExtraBiome = {
      ...mockRouteDefinition,
      biomes: [
        { id: "", fromProgress01: 0, toProgress01: 0.1 },
        { id: "coast", fromProgress01: 0, toProgress01: 0.45 },
        { id: "forest", fromProgress01: 0.45, toProgress01: 1 }
      ]
    };

    expect(resolveRouteDefinition(routeWithInvalidExtraBiome)).toMatchObject({
      usedFallback: true,
      reason: "invalid-biomes",
      route: { id: "placeholder-route" }
    });
    expect(selectBiomeAtProgress(routeWithInvalidExtraBiome, 0.05)).toBe(
      "placeholder"
    );
  });
});

describe("createRouteFrameSnapshot", () => {
  it("consumes MYB-3 RideFrameSnapshot progress without recalculating ride values", () => {
    const rideSnapshot: RideFrameSnapshot = {
      source: "mock",
      input: { source: "mock", effort01: 1, nowMs: 2000 },
      targetSpeedMps: 99,
      speedMps: 42,
      progress: { distanceMeters: 500, progress01: 0.5, completed: false },
      elapsedMs: 1000,
      stats: { elapsedMs: 1000, distanceMeters: 500, averageSpeedMps: 500 },
      completed: false,
      segmentStatsResults: []
    };

    const routeFrame = createRouteFrameSnapshot(
      rideSnapshot,
      mockRouteDefinition
    );

    expect(routeFrame.sample.progress01).toBe(0.5);
    expect(routeFrame.sample.distanceMeters).toBe(500);
    expect(routeFrame.biomeId).toBe("forest");
    expect(routeFrame.camera.progress01).toBe(0.5);
    expect(routeFrame.segment.current?.kind).toBe("sprint");
    expect(routeFrame.segment.count).toBe(5);
  });
});

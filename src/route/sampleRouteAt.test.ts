import { describe, expect, it } from "vitest";
import { mockRouteDefinition } from "./mockRouteDefinition";
import { sampleRouteAt } from "./sampleRouteAt";

function expectFiniteVec3(position: { x: number; y: number; z: number }): void {
  expect(Number.isFinite(position.x)).toBe(true);
  expect(Number.isFinite(position.y)).toBe(true);
  expect(Number.isFinite(position.z)).toBe(true);
}

describe("sampleRouteAt", () => {
  it("samples deterministic start, middle, and end positions", () => {
    expect(sampleRouteAt(mockRouteDefinition, 0)).toMatchObject({
      progress01: 0,
      distanceMeters: 0,
      position: { x: 0, y: 0, z: 0 },
      usedFallback: false
    });

    expect(sampleRouteAt(mockRouteDefinition, 0.5)).toMatchObject({
      progress01: 0.5,
      distanceMeters: 500,
      position: { x: 36, y: 2, z: 496 },
      usedFallback: false
    });

    expect(sampleRouteAt(mockRouteDefinition, 1)).toMatchObject({
      progress01: 1,
      distanceMeters: 1000,
      position: { x: 0, y: 1, z: 1000 },
      usedFallback: false
    });
  });

  it("clamps invalid progress without producing invalid numbers", () => {
    const samples = [
      sampleRouteAt(mockRouteDefinition, -0.5),
      sampleRouteAt(mockRouteDefinition, Number.NaN),
      sampleRouteAt(mockRouteDefinition, Number.POSITIVE_INFINITY)
    ];

    expect(samples[0].progress01).toBe(0);
    expect(samples[1].progress01).toBe(0);
    expect(samples[2].progress01).toBe(1);

    for (const sample of samples) {
      expect(Number.isFinite(sample.distanceMeters)).toBe(true);
      expectFiniteVec3(sample.position);
      expectFiniteVec3(sample.direction);
      expectFiniteVec3(sample.lookAheadPosition);
    }
  });

  it("uses a deterministic placeholder for absent routes", () => {
    const sample = sampleRouteAt(undefined, 0.4);

    expect(sample.usedFallback).toBe(true);
    expect(sample.fallbackReason).toBe("missing-route");
    expect(sample.routeId).toBe("placeholder-route");
    expect(sample.progress01).toBe(0.4);
    expect(sample.distanceMeters).toBe(40);
    expectFiniteVec3(sample.position);
    expectFiniteVec3(sample.direction);
  });
});

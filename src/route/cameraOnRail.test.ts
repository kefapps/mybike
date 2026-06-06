import { describe, expect, it } from "vitest";
import { cameraOnRail, DEFAULT_CAMERA_RAIL_CONFIG } from "./cameraOnRail";
import { mockRouteDefinition } from "./mockRouteDefinition";

function expectFiniteVec3(position: { x: number; y: number; z: number }): void {
  expect(Number.isFinite(position.x)).toBe(true);
  expect(Number.isFinite(position.y)).toBe(true);
  expect(Number.isFinite(position.z)).toBe(true);
}

describe("cameraOnRail", () => {
  it("applies height, look-ahead, and FOV from config", () => {
    const camera = cameraOnRail(
      mockRouteDefinition,
      { distanceMeters: 500, progress01: 0.5, completed: false },
      { heightMeters: 2, lookAheadMeters: 50, fovDegrees: 80 }
    );

    expect(camera.position).toEqual({ x: 36, y: 4, z: 496 });
    expect(camera.lookAt.z).toBeGreaterThan(camera.position.z);
    expect(camera.fovDegrees).toBe(80);
    expect(camera.usedFallback).toBe(false);
    expectFiniteVec3(camera.position);
    expectFiniteVec3(camera.lookAt);
  });

  it("bounds invalid camera config to deterministic defaults", () => {
    const camera = cameraOnRail(mockRouteDefinition, 0.25, {
      heightMeters: Number.NaN,
      lookAheadMeters: -10,
      fovDegrees: Number.POSITIVE_INFINITY
    });

    expect(camera.fovDegrees).toBe(DEFAULT_CAMERA_RAIL_CONFIG.fovDegrees);
    expect(Number.isFinite(camera.position.y)).toBe(true);
    expect(Number.isFinite(camera.lookAt.y)).toBe(true);
  });

  it("uses deterministic defaults when camera config is null", () => {
    const camera = cameraOnRail(mockRouteDefinition, 0.25, null);

    expect(camera.fovDegrees).toBe(DEFAULT_CAMERA_RAIL_CONFIG.fovDegrees);
    expectFiniteVec3(camera.position);
    expectFiniteVec3(camera.lookAt);
  });

  it("keeps the look-at finite at the end of the rail", () => {
    const camera = cameraOnRail(mockRouteDefinition, {
      distanceMeters: 1000,
      progress01: 1,
      completed: true
    });

    expect(camera.progress01).toBe(1);
    expect(camera.lookAt.z).toBeGreaterThanOrEqual(camera.position.z);
    expectFiniteVec3(camera.position);
    expectFiniteVec3(camera.lookAt);
  });
});

import { describe, expect, it } from "vitest";

import { mockRouteDefinition } from "../route";
import type { RouteDefinition } from "../route";
import {
  getBiomePalette,
  getRouteCenterXAtZ,
  getRouteRenderPoints,
  toThreeVector3
} from "./renderHelpers";

describe("render helpers", () => {
  it("maps route Vec3 values to Three.js vectors without mutating input", () => {
    const vec = { x: 1, y: 2, z: 3 };
    const result = toThreeVector3(vec);

    expect(result.x).toBe(1);
    expect(result.y).toBe(2);
    expect(result.z).toBe(3);
    expect(vec).toEqual({ x: 1, y: 2, z: 3 });
  });

  it("selects readable palettes for coast, forest, placeholder, and unknown biomes", () => {
    expect(getBiomePalette("coast", false)).toMatchObject({
      sky: 0xf3c995,
      ground: 0x79b99b,
      horizon: 0x5fa6c8,
      fogNear: 18,
      fogFar: 430,
      ambientIntensity: 0.86,
      sunIntensity: 1.32,
      sunPosition: { x: -26, y: 42, z: -32 }
    });
    expect(getBiomePalette("forest", false)).toMatchObject({
      sky: 0x8fbf9a,
      ground: 0x234c34,
      horizon: 0x3f6b50,
      fogNear: 12,
      fogFar: 260,
      ambientIntensity: 0.56,
      sunIntensity: 0.82,
      sunPosition: { x: 22, y: 30, z: -18 }
    });
    expect(getBiomePalette("coast", true)).toMatchObject({
      route: 0x607080
    });
    expect(getBiomePalette("unknown", false)).toEqual(
      getBiomePalette("placeholder", false)
    );
  });

  it("creates deterministic route render points from the route definition", () => {
    const points = getRouteRenderPoints(mockRouteDefinition);

    expect(points).toHaveLength(mockRouteDefinition.points.length);
    expect(points[0]).toMatchObject({ x: 0, y: 0.03, z: 0 });
    expect(points.at(-1)).toMatchObject({ x: 0, y: 1.03, z: 1000 });
  });

  it("samples route center x for route-aware scenic offsets", () => {
    expect(getRouteCenterXAtZ(mockRouteDefinition, 148)).toBe(24);
    expect(getRouteCenterXAtZ(mockRouteDefinition, 231.5)).toBeCloseTo(3);
    expect(getRouteCenterXAtZ(mockRouteDefinition, -50)).toBe(0);
    expect(getRouteCenterXAtZ(mockRouteDefinition, 1200)).toBe(0);
  });

  it("uses placeholder route points when a malformed route reaches render", () => {
    const malformedRoute = {
      ...mockRouteDefinition,
      points: [undefined]
    } as unknown as RouteDefinition;

    const points = getRouteRenderPoints(malformedRoute);

    expect(points).toHaveLength(2);
    expect(points[0]).toMatchObject({ x: 0, y: 0.03, z: 0 });
    expect(points.at(-1)).toMatchObject({ x: 0, y: 0.03, z: 100 });
  });
});

import { describe, expect, it } from "vitest";

import { mockRouteDefinition } from "../route";
import type { RouteDefinition } from "../route";
import {
  getBiomePalette,
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
      sky: 0xb7d9ec,
      ground: 0x8cc7a5,
      horizon: 0x8fc4d7
    });
    expect(getBiomePalette("forest", false)).toMatchObject({
      sky: 0xa9c7b0,
      ground: 0x2f6f45,
      horizon: 0x5d8068
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

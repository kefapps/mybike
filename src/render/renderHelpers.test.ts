import { describe, expect, it } from "vitest";

import { mockRouteDefinition } from "../route";
import type { RouteDefinition } from "../route";
import {
  getBiomeBasePalette,
  getBiomePalette,
  getRouteCenterXAtZ,
  getRouteElevationAtZ,
  getRouteRenderPoints,
  resolveRenderPalette,
  resolveRenderPaletteForPresetId,
  toThreeVector3
} from "./renderHelpers";
import { getEnvironmentPreset } from "./environmentPresets";

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
    expect(points.at(-1)).toMatchObject({ x: 0, y: 3.03, z: 1000 });
  });

  it("samples route center x for route-aware scenic offsets", () => {
    expect(getRouteCenterXAtZ(mockRouteDefinition, 148)).toBe(24);
    expect(getRouteCenterXAtZ(mockRouteDefinition, 231.5)).toBeCloseTo(3);
    expect(getRouteCenterXAtZ(mockRouteDefinition, -50)).toBe(0);
    expect(getRouteCenterXAtZ(mockRouteDefinition, 1200)).toBe(0);
  });

  it("samples route elevation for terrain anchoring", () => {
    expect(getRouteElevationAtZ(mockRouteDefinition, 0)).toBe(0);
    expect(getRouteElevationAtZ(mockRouteDefinition, 148)).toBe(5);
    expect(getRouteElevationAtZ(mockRouteDefinition, 405.5)).toBeCloseTo(6);
    expect(getRouteElevationAtZ(mockRouteDefinition, -50)).toBe(0);
    expect(getRouteElevationAtZ(mockRouteDefinition, 1200)).toBe(3);
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

  it("returns biome base palettes for coast, forest, placeholder and unknown", () => {
    expect(getBiomeBasePalette("coast", false)).toEqual({
      ground: 0x79b99b,
      route: 0xeecf82,
      accent: 0x277fa8
    });
    expect(getBiomeBasePalette("forest", false)).toEqual({
      ground: 0x234c34,
      route: 0x8c7047,
      accent: 0x1a5f39
    });
    expect(getBiomeBasePalette("coast", true)).toEqual({
      ground: 0x8b9691,
      route: 0x607080,
      accent: 0x4d5a62
    });
    expect(getBiomeBasePalette("unknown", false)).toEqual(
      getBiomeBasePalette("placeholder", false)
    );
  });

  it("merges biome base colors with environment preset values", () => {
    const preset = getEnvironmentPreset("softNight");
    const merged = resolveRenderPalette("coast", false, preset);

    expect(merged.ground).toBe(0x79b99b);
    expect(merged.route).toBe(0xeecf82);
    expect(merged.accent).toBe(0x277fa8);
    expect(merged.sky).toBe(preset.sky);
    expect(merged.fog).toBe(preset.fog);
    expect(merged.fogNear).toBe(preset.fogNear);
    expect(merged.fogFar).toBe(preset.fogFar);
    expect(merged.ambientColor).toBe(preset.ambientColor);
    expect(merged.sunColor).toBe(preset.sunColor);
    expect(merged.sunIntensity).toBe(preset.sunIntensity);
    expect(merged.sunPosition).toEqual(preset.sunPosition);
  });

  it("falls back to placeholder base palette when route uses the fallback biome", () => {
    const preset = getEnvironmentPreset("morning");
    const merged = resolveRenderPalette("coast", true, preset);

    expect(merged.ground).toBe(0x8b9691);
    expect(merged.route).toBe(0x607080);
    expect(merged.accent).toBe(0x4d5a62);
    expect(merged.sky).toBe(preset.sky);
  });

  it("resolves palettes for preset ids including unknown values", () => {
    const merged = resolveRenderPaletteForPresetId("forest", false, "softNight");
    const preset = getEnvironmentPreset("softNight");

    expect(merged.ground).toBe(0x234c34);
    expect(merged.sky).toBe(preset.sky);
    expect(merged.fog).toBe(preset.fog);
    expect(resolveRenderPaletteForPresetId("forest", false, null)).toEqual(
      resolveRenderPalette("forest", false, getEnvironmentPreset("morning"))
    );
  });
});

import { describe, expect, it } from "vitest";

import {
  buildAutoPresetCycle,
  ENVIRONMENT_PRESET_IDS,
  getDefaultPresetForBiome,
  getEnvironmentPreset,
  isValidEnvironmentPresetId,
  isWeatherEnabled,
  listEnvironmentPresets,
  selectAutoPreset,
  type EnvironmentPresetId
} from "./environmentPresets";

describe("environmentPresets", () => {
  it("exposes at least three distinct ambiances and a low-performance preset", () => {
    const summaries = listEnvironmentPresets();
    const highQuality = summaries.filter((entry) => entry.quality === "high");

    expect(summaries.length).toBeGreaterThanOrEqual(4);
    expect(highQuality.length).toBeGreaterThanOrEqual(3);
    expect(summaries.find((entry) => entry.id === "lowPerformance")).toBeDefined();
  });

  it("returns distinct palette, weather and quality values per preset", () => {
    const seenSky = new Set<number>();
    const seenFog = new Set<number>();
    const seenWeather = new Set<string>();
    const seenQuality = new Set<string>();

    for (const id of ENVIRONMENT_PRESET_IDS) {
      const preset = getEnvironmentPreset(id);
      seenSky.add(preset.sky);
      seenFog.add(preset.fog);
      seenWeather.add(preset.weather);
      seenQuality.add(preset.quality);
    }

    expect(seenSky.size).toBeGreaterThanOrEqual(4);
    expect(seenFog.size).toBeGreaterThanOrEqual(4);
    expect(seenWeather.size).toBeGreaterThanOrEqual(2);
    expect(seenQuality.size).toBeGreaterThanOrEqual(2);
  });

  it("falls back to the morning preset for unknown or invalid ids", () => {
    const morning = getEnvironmentPreset("morning");

    expect(getEnvironmentPreset(undefined)).toBe(morning);
    expect(getEnvironmentPreset(null)).toBe(morning);
    expect(getEnvironmentPreset("")).toBe(morning);
    expect(getEnvironmentPreset("unknown")).toBe(morning);
  });

  it("keeps the soft night preset readable with adequate ambient lighting", () => {
    const softNight = getEnvironmentPreset("softNight");

    expect(softNight.sky).toBe(0x16243a);
    expect(softNight.fog).toBe(0x1d2f47);
    expect(softNight.ambientIntensity).toBeGreaterThanOrEqual(0.55);
    expect(softNight.sunIntensity).toBeGreaterThanOrEqual(0.55);
  });

  it("links each preset to a preferred biome", () => {
    expect(getDefaultPresetForBiome("coast")).toBe("morning");
    expect(getDefaultPresetForBiome("forest")).toBe("softNight");
    expect(getDefaultPresetForBiome("placeholder")).toBe("morning");
    expect(getDefaultPresetForBiome("unknown-biome")).toBe("morning");
  });

  it("builds a deterministic auto cycle across progress", () => {
    const cycle = buildAutoPresetCycle();

    expect(cycle.length).toBe(4);
    expect(cycle[0]).toMatchObject({
      presetId: "morning",
      startProgress01: 0,
      endProgress01: 0.25
    });
    expect(cycle[1]).toMatchObject({
      presetId: "goldenHour",
      startProgress01: 0.25,
      endProgress01: 0.5
    });
    expect(cycle[2]).toMatchObject({
      presetId: "softNight",
      startProgress01: 0.5,
      endProgress01: 0.75
    });
    expect(cycle[3]).toMatchObject({
      presetId: "lightRain",
      startProgress01: 0.75,
      endProgress01: 1
    });
  });

  it("respects the requested cycle step count and guards non-positive values", () => {
    expect(buildAutoPresetCycle(2)).toHaveLength(2);
    expect(buildAutoPresetCycle(0)).toHaveLength(4);
    expect(buildAutoPresetCycle(-1)).toHaveLength(4);
    expect(buildAutoPresetCycle(Number.NaN)).toHaveLength(4);
    expect(buildAutoPresetCycle(99)).toHaveLength(4);
  });

  it("selects auto presets by progress with edge clamping", () => {
    const cycle = buildAutoPresetCycle();

    expect(selectAutoPreset(cycle, 0)).toBe("morning");
    expect(selectAutoPreset(cycle, 0.249)).toBe("morning");
    expect(selectAutoPreset(cycle, 0.25)).toBe("goldenHour");
    expect(selectAutoPreset(cycle, 0.5)).toBe("softNight");
    expect(selectAutoPreset(cycle, 0.75)).toBe("lightRain");
    expect(selectAutoPreset(cycle, 1)).toBe("lightRain");
    expect(selectAutoPreset(cycle, -0.2)).toBe("morning");
    expect(selectAutoPreset(cycle, Number.NaN)).toBe("morning");
    expect(selectAutoPreset(cycle, Number.POSITIVE_INFINITY)).toBe("lightRain");
  });

  it("returns the morning preset when the cycle is empty", () => {
    expect(selectAutoPreset([], 0.5)).toBe("morning");
  });

  it("toggles weather based on the preset definition and the user override", () => {
    const rain = getEnvironmentPreset("lightRain");
    const morning = getEnvironmentPreset("morning");

    expect(isWeatherEnabled(rain, true)).toBe(true);
    expect(isWeatherEnabled(rain, false)).toBe(false);
    expect(isWeatherEnabled(morning, true)).toBe(false);
  });

  it("validates environment preset ids safely", () => {
    const ids: EnvironmentPresetId[] = [
      "morning",
      "goldenHour",
      "softNight",
      "lightRain",
      "lowPerformance"
    ];

    for (const id of ids) {
      expect(isValidEnvironmentPresetId(id)).toBe(true);
    }

    expect(isValidEnvironmentPresetId("")).toBe(false);
    expect(isValidEnvironmentPresetId(null)).toBe(false);
    expect(isValidEnvironmentPresetId(undefined)).toBe(false);
    expect(isValidEnvironmentPresetId("unknown")).toBe(false);
  });
});

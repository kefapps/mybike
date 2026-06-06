import { describe, expect, it } from "vitest";
import { mockRouteDefinition } from "./mockRouteDefinition";
import { selectBiomeAtProgress } from "./selectBiomeAtProgress";

describe("selectBiomeAtProgress", () => {
  it("selects deterministic biomes at bounds and transition", () => {
    expect(selectBiomeAtProgress(mockRouteDefinition, 0)).toBe("coast");
    expect(selectBiomeAtProgress(mockRouteDefinition, 0.44)).toBe("coast");
    expect(selectBiomeAtProgress(mockRouteDefinition, 0.45)).toBe("forest");
    expect(selectBiomeAtProgress(mockRouteDefinition, 1)).toBe("forest");
  });

  it("clamps progress before selecting a biome", () => {
    expect(selectBiomeAtProgress(mockRouteDefinition, -1)).toBe("coast");
    expect(selectBiomeAtProgress(mockRouteDefinition, Number.NaN)).toBe("coast");
    expect(selectBiomeAtProgress(mockRouteDefinition, Number.POSITIVE_INFINITY)).toBe(
      "forest"
    );
  });

  it("uses the placeholder biome when route biomes are invalid", () => {
    expect(
      selectBiomeAtProgress(
        {
          ...mockRouteDefinition,
          biomes: []
        },
        0.5
      )
    ).toBe("placeholder");
  });
});

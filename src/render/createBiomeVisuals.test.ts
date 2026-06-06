import { describe, expect, it } from "vitest";

import { createBiomeVisuals } from "./createBiomeVisuals";

describe("createBiomeVisuals", () => {
  it("creates dense reusable lateral scenery without external assets", () => {
    const visuals = createBiomeVisuals();
    const names = new Set(visuals.group.children.map((child) => child.name));

    expect(visuals.group.name).toBe("scenic-biome-visuals");
    expect(visuals.group.children.length).toBeGreaterThan(90);
    expect([...names]).toEqual(
      expect.arrayContaining([
        "scenic-coast-post",
        "scenic-grass-tuft",
        "scenic-rock",
        "scenic-tree-trunk",
        "scenic-tree-canopy",
        "scenic-road-sign"
      ])
    );
  });

  it("tracks shared resources for deterministic disposal", () => {
    const visuals = createBiomeVisuals();

    expect(visuals.geometries.length).toBeGreaterThanOrEqual(8);
    expect(visuals.materials.length).toBeGreaterThanOrEqual(7);
    expect(visuals.coastMaterial).toBe(visuals.materials[0]);
    expect(visuals.forestMaterial).toBe(visuals.materials[1]);
  });
});

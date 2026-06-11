import { describe, expect, it } from "vitest";
import {
  getAssetFromManifest,
  getManifestStats,
  filterAssetsByBiome,
  clearManifestCache
} from "./assetLoader";
import type { AssetManifest, AssetManifestEntry } from "./assetTypes";

function makeEntry(overrides?: Partial<AssetManifestEntry>): AssetManifestEntry {
  return {
    id: "tree-01",
    name: "Tree",
    file: "tree.glb",
    sourceUrl: "https://example.com/tree",
    author: "Author",
    license: "CC0",
    attributionRequired: false,
    tags: ["vegetation"],
    biomes: ["forest"],
    animated: false,
    sizeKb: 200,
    triangleBudget: 1000,
    scale: { x: 1, y: 1, z: 1 },
    ...overrides
  };
}

const sampleManifest: AssetManifest = {
  version: "1.0.0",
  assets: [
    makeEntry({ id: "tree-01", biomes: ["forest"] }),
    makeEntry({ id: "rock-01", biomes: ["mountain"], sizeKb: 150, triangleBudget: 600 }),
    makeEntry({
      id: "cottage-01",
      biomes: ["village", "forest"],
      sizeKb: 400,
      triangleBudget: 1800
    })
  ]
};

describe("getAssetFromManifest", () => {
  it("returns the entry and URL for a known id", () => {
    const result = getAssetFromManifest(sampleManifest, "tree-01");
    expect(result).not.toBeNull();
    expect(result!.entry.id).toBe("tree-01");
    expect(result!.url).toBe("assets/models/tree.glb");
  });

  it("returns null for an unknown id", () => {
    expect(getAssetFromManifest(sampleManifest, "unknown")).toBeNull();
  });
});

describe("getManifestStats", () => {
  it("computes correct stats", () => {
    const stats = getManifestStats(sampleManifest);
    expect(stats.assetCount).toBe(3);
    expect(stats.totalSizeKb).toBe(750);
    expect(stats.totalTriangles).toBe(3400);
    expect(stats.biomes).toEqual(["forest", "mountain", "village"]);
  });
});

describe("filterAssetsByBiome", () => {
  it("returns assets matching the biome", () => {
    const forest = filterAssetsByBiome(sampleManifest, "forest");
    expect(forest).toHaveLength(2);
    expect(forest.map((a) => a.id)).toEqual(["tree-01", "cottage-01"]);
  });

  it("returns empty array for unmatched biome", () => {
    expect(filterAssetsByBiome(sampleManifest, "ocean")).toHaveLength(0);
  });
});

describe("clearManifestCache", () => {
  it("clears without error", () => {
    clearManifestCache();
    expect(true).toBe(true);
  });
});

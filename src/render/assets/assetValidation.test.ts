import { describe, expect, it } from "vitest";
import { validateAssetManifest, validateAssetManifestEntry } from "./assetValidation";
import type { AssetManifest, AssetManifestEntry } from "./assetTypes";

function makeValidEntry(overrides?: Partial<AssetManifestEntry>): AssetManifestEntry {
  return {
    id: "test-asset-01",
    name: "Test Asset",
    file: "test.glb",
    sourceUrl: "https://example.com/test",
    author: "Test Author",
    license: "CC0",
    attributionRequired: false,
    tags: ["test"],
    biomes: ["forest"],
    animated: false,
    sizeKb: 100,
    triangleBudget: 500,
    scale: { x: 1, y: 1, z: 1 },
    ...overrides
  };
}

function makeValidManifest(assets?: AssetManifestEntry[]): AssetManifest {
  return {
    version: "1.0.0",
    assets: assets ?? [makeValidEntry()]
  };
}

describe("validateAssetManifestEntry", () => {
  it("accepts a valid entry", () => {
    expect(validateAssetManifestEntry(makeValidEntry())).toEqual([]);
  });

  it("accepts entry without optional scale", () => {
    const entry = makeValidEntry();
    delete (entry as Record<string, unknown>).scale;
    expect(validateAssetManifestEntry(entry)).toEqual([]);
  });

  it("rejects missing id", () => {
    const errors = validateAssetManifestEntry(makeValidEntry({ id: "" as unknown as string }));
    expect(errors).toContain("missing or invalid id");
  });

  it("rejects missing name", () => {
    const errors = validateAssetManifestEntry(makeValidEntry({ name: "" }));
    expect(errors).toContain("missing or invalid name");
  });

  it("rejects missing file", () => {
    const errors = validateAssetManifestEntry(makeValidEntry({ file: "" }));
    expect(errors).toContain("missing or invalid file");
  });

  it("rejects missing sourceUrl", () => {
    const errors = validateAssetManifestEntry(makeValidEntry({ sourceUrl: "" }));
    expect(errors).toContain("missing or invalid sourceUrl");
  });

  it("rejects missing author", () => {
    const errors = validateAssetManifestEntry(makeValidEntry({ author: "" }));
    expect(errors).toContain("missing or invalid author");
  });

  it("rejects missing license", () => {
    const errors = validateAssetManifestEntry(makeValidEntry({ license: "" }));
    expect(errors).toContain("missing or invalid license");
  });

  it("rejects missing attributionRequired", () => {
    const errors = validateAssetManifestEntry(
      makeValidEntry({ attributionRequired: undefined as unknown as boolean })
    );
    expect(errors).toContain("missing or invalid attributionRequired");
  });

  it("rejects empty tags array", () => {
    const errors = validateAssetManifestEntry(makeValidEntry({ tags: [] }));
    expect(errors).toContain("missing or invalid tags (must be a non-empty array of strings)");
  });

  it("rejects empty biomes array", () => {
    const errors = validateAssetManifestEntry(makeValidEntry({ biomes: [] }));
    expect(errors).toContain("missing or invalid biomes (must be a non-empty array of strings)");
  });

  it("rejects invalid animated", () => {
    const errors = validateAssetManifestEntry(
      makeValidEntry({ animated: undefined as unknown as boolean })
    );
    expect(errors).toContain("missing or invalid animated");
  });

  it("rejects zero sizeKb", () => {
    const errors = validateAssetManifestEntry(makeValidEntry({ sizeKb: 0 }));
    expect(errors).toContain("missing or invalid sizeKb (must be a positive number)");
  });

  it("rejects negative triangleBudget", () => {
    const errors = validateAssetManifestEntry(makeValidEntry({ triangleBudget: -1 }));
    expect(errors).toContain("missing or invalid triangleBudget (must be a positive number)");
  });

  it("rejects invalid scale", () => {
    const errors = validateAssetManifestEntry(
      makeValidEntry({ scale: { x: 1, y: NaN, z: 1 } })
    );
    expect(errors).toContain("invalid scale (must be {x, y, z} with finite numbers)");
  });

  it("rejects null entry", () => {
    const errors = validateAssetManifestEntry(null as unknown as AssetManifestEntry);
    expect(errors).toContain("entry is not an object");
  });
});

describe("validateAssetManifest", () => {
  it("accepts a valid manifest", () => {
    const result = validateAssetManifest(makeValidManifest());
    expect(result.valid).toBe(true);
    expect(result.errors).toHaveLength(0);
  });

  it("accepts a manifest with multiple valid assets", () => {
    const result = validateAssetManifest(
      makeValidManifest([
        makeValidEntry({ id: "a" }),
        makeValidEntry({ id: "b" }),
        makeValidEntry({ id: "c" })
      ])
    );
    expect(result.valid).toBe(true);
  });

  it("rejects null manifest", () => {
    const result = validateAssetManifest(null);
    expect(result.valid).toBe(false);
    expect(result.errors[0].errors).toContain("manifest is not an object");
  });

  it("rejects manifest without version", () => {
    const result = validateAssetManifest({ assets: [] });
    expect(result.valid).toBe(false);
    expect(result.errors[0].errors).toContain("missing or invalid version");
  });

  it("rejects manifest without assets array", () => {
    const result = validateAssetManifest({ version: "1.0.0" });
    expect(result.valid).toBe(false);
    expect(result.errors[0].errors).toContain("missing or invalid assets array");
  });

  it("reports all invalid entries", () => {
    const result = validateAssetManifest(
      makeValidManifest([
        makeValidEntry({ id: "valid" }),
        makeValidEntry({ id: "", license: "" })
      ])
    );
    expect(result.valid).toBe(false);
    expect(result.errors).toHaveLength(1);
    expect(result.errors[0].id).toBeUndefined();
    expect(result.errors[0].errors).toContain("missing or invalid id");
    expect(result.errors[0].errors).toContain("missing or invalid license");
  });

  it("fails when a required metadata is missing (license)", () => {
    const result = validateAssetManifest(
      makeValidManifest([makeValidEntry({ id: "x", license: "" })])
    );
    expect(result.valid).toBe(false);
    expect(result.errors[0].errors.some((e) => e.includes("license"))).toBe(true);
  });

  it("fails when a required metadata is missing (sourceUrl)", () => {
    const result = validateAssetManifest(
      makeValidManifest([makeValidEntry({ id: "x", sourceUrl: "" })])
    );
    expect(result.valid).toBe(false);
    expect(result.errors[0].errors.some((e) => e.includes("sourceUrl"))).toBe(true);
  });
});

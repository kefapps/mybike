import { describe, expect, it } from "vitest";

import { loadAssetManifest, loadAssetManifestBundle } from "./loadAssetManifest";

describe("loadAssetManifest", () => {
  it("returns the empty asset list for the current MVP", () => {
    const entries = loadAssetManifest();

    expect(entries).toEqual([]);
  });

  it("exposes the policy block on the bundle loader", () => {
    const bundle = loadAssetManifestBundle();

    expect(bundle.version).toBe(1);
    expect(bundle.policy.ticket).toBe("MYB-52");
    expect(bundle.policy.licensePolicyUrl).toContain("THIRD_PARTY_ASSETS.md");
    expect(bundle.assets).toEqual([]);
  });
});

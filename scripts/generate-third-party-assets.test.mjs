import { mkdtempSync, readFileSync, writeFileSync } from "node:fs";
import { tmpdir } from "node:os";
import { join } from "node:path";

import { describe, expect, it } from "vitest";

import { buildCreditsMarkdown, generateCreditsMarkdown } from "./generate-third-party-assets.mjs";

const SAMPLE_MANIFEST = {
  version: 1,
  policy: {
    ticket: "MYB-52",
    summary: "Politique d'attribution des assets tiers MyBike.",
    licensePolicyUrl: "./THIRD_PARTY_ASSETS.md"
  },
  assets: [
    {
      id: "rock-pack",
      name: "Rock pack low-poly",
      author: "Kenney.nl",
      source: "https://kenney.nl/assets/rock-pack",
      category: "3d-model",
      license: "CC0",
      modifications: "decoupe et retextured"
    },
    {
      id: "ambient-loop",
      name: "Forest ambient loop",
      author: "Alice Composer",
      source: "https://opengameart.org/example/forest-ambient",
      category: "audio",
      license: "CC-BY",
      licenseUrl: "https://creativecommons.org/licenses/by/4.0/",
      modifications: "boucle normalisee a -18 LUFS"
    }
  ]
};

describe("generate-third-party-assets", () => {
  it("renders the policy block and an asset table per entry", () => {
    const md = buildCreditsMarkdown(SAMPLE_MANIFEST);

    expect(md).toContain("# Crédits des assets tiers");
    expect(md).toContain("**MYB-52**");
    expect(md).toContain("Politique d'attribution");
    expect(md).toContain("| ID | Nom | Auteur |");
    expect(md).toContain("Rock pack low-poly");
    expect(md).toContain("Kenney.nl");
    expect(md).toContain("Forest ambient loop");
    expect(md).toContain("CC0 1.0 (domaine public)");
    expect(md).toContain("CC BY 4.0");
    expect(md).toContain("decoupe et retextured");
    expect(md).toContain("boucle normalisee a -18 LUFS");
  });

  it("renders the empty table placeholder when no asset is listed", () => {
    const md = buildCreditsMarkdown({
      version: 1,
      policy: { ticket: "MYB-52", summary: "ok", licensePolicyUrl: "./x.md" },
      assets: []
    });

    expect(md).toContain("| _aucun_ |");
  });

  it("writes the markdown file from the manifest on disk", () => {
    const dir = mkdtempSync(join(tmpdir(), "credits-"));
    const manifestPath = join(dir, "manifest.json");
    const outputPath = join(dir, "THIRD_PARTY_ASSETS.md");

    writeFileSync(manifestPath, JSON.stringify(SAMPLE_MANIFEST), "utf8");

    const result = generateCreditsMarkdown({ manifestPath, outputPath });

    expect(result.assetCount).toBe(2);
    expect(result.outputPath).toBe(outputPath);

    const written = readFileSync(outputPath, "utf8");
    expect(written).toContain("Rock pack low-poly");
    expect(written).toContain("Forest ambient loop");
  });
});

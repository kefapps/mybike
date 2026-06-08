import { describe, expect, it } from "vitest";
import { mkdtemp, rm } from "node:fs/promises";
import { mkdirSync, writeFileSync } from "node:fs";
import { tmpdir } from "node:os";
import { join } from "node:path";

import { buildBudgetReport, readJsChunkSizes } from "./validate-build-budgets.mjs";

describe("validate-build-budgets", () => {
  it("excludes map files from chunk sizing and reports deterministic chunk order", async () => {
    const root = await mkdtemp(join(tmpdir(), "myb25-budget-"));
    const assetsDir = join(root, "assets");
    const nestedDir = join(assetsDir, "nested");

    try {
      mkdirSync(nestedDir, { recursive: true });

      writeFileSync(join(assetsDir, "chunk-small.js"), Buffer.alloc(100 * 1024));
      writeFileSync(join(assetsDir, "chunk-small.js.map"), "{}");
      writeFileSync(join(assetsDir, "chunk-large.js"), Buffer.alloc(200 * 1024));
      writeFileSync(join(nestedDir, "chunk-medium.js"), Buffer.alloc(150 * 1024));

      const report = buildBudgetReport({ rootDir: assetsDir, maxChunkSizeKb: 150 });
      expect(report.totalChunks).toBe(3);
      expect(report.oversizedChunks).toBe(1);
      expect(report.chunks.every((chunk) => !chunk.name.endsWith(".map"))).toBe(true);
      expect(report.chunks.map((chunk) => chunk.name)).toEqual([
        "chunk-large.js",
        "chunk-medium.js",
        "chunk-small.js"
      ]);
    } finally {
      await rm(root, { force: true, recursive: true });
    }
  });

  it("can parse chunk file sizes deterministically", async () => {
    const root = await mkdtemp(join(tmpdir(), "myb25-budget-"));
    const chunkPath = join(root, "myb-25-deterministic-chunk.js");

    try {
      writeFileSync(chunkPath, Buffer.alloc(1024));

      const [read] = readJsChunkSizes([chunkPath]);
      expect(read.name).toBe("myb-25-deterministic-chunk.js");
      expect(read.sizeBytes).toBe(1024);
      expect(read.sizeKb).toBe(1);
    } finally {
      await rm(root, { force: true, recursive: true });
    }
  });
});

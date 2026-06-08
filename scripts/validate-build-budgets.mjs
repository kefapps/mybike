import { existsSync, mkdirSync, readdirSync, statSync, writeFileSync } from "node:fs";
import { basename, join, resolve } from "node:path";
import { fileURLToPath } from "node:url";

export const DEFAULT_MAX_CHUNK_SIZE_KB = 380;
export const DEFAULT_REPORT_PATH = "_bmad-output/web-test-results/myb-25-bundle-size-budget.txt";

const repoRoot = resolve(fileURLToPath(new URL("..", import.meta.url)));
const DIST_DIR = join(repoRoot, "dist");
const ASSETS_DIR = join(DIST_DIR, "assets");

export function collectChunkPaths(rootDir = ASSETS_DIR) {
  const entries = readdirSync(rootDir, { withFileTypes: true }).sort((a, b) =>
    a.name.localeCompare(b.name)
  );
  const chunks = [];

  for (const entry of entries) {
    const fullPath = join(rootDir, entry.name);

    if (entry.isDirectory()) {
      chunks.push(...collectChunkPaths(fullPath));
      continue;
    }

    if (!entry.isFile() || !entry.name.endsWith(".js")) {
      continue;
    }

    chunks.push(fullPath);
  }

  return chunks;
}

export function readJsChunkSizes(chunkPaths) {
  return chunkPaths.map((chunkPath) => {
    const size = statSync(chunkPath).size;
    const sizeKb = size / 1024;

    return {
      path: chunkPath,
      name: basename(chunkPath),
      sizeBytes: size,
      sizeKb: Number(sizeKb.toFixed(3))
    };
  });
}

export function buildBudgetReport({ maxChunkSizeKb = DEFAULT_MAX_CHUNK_SIZE_KB, rootDir = ASSETS_DIR } = {}) {
  if (!existsSync(rootDir)) {
    throw new Error(`Vite output assets directory not found: ${rootDir}. Run npm run build first.`);
  }

  const chunkPaths = collectChunkPaths(rootDir).filter((chunkPath) => !chunkPath.endsWith(".js.map"));
  const chunks = readJsChunkSizes(chunkPaths).sort((a, b) => {
    if (b.sizeBytes !== a.sizeBytes) {
      return b.sizeBytes - a.sizeBytes;
    }

    return a.name.localeCompare(b.name);
  });
  const violations = chunks.filter((chunk) => chunk.sizeKb > maxChunkSizeKb);

  return {
    generatedAt: new Date().toISOString(),
    source: "vite dist/assets",
    maxChunkSizeKb,
    totalChunks: chunks.length,
    oversizedChunks: violations.length,
    chunks
  };
}

function formatLineItem(chunk, maxChunkSizeKb) {
  const status = chunk.sizeKb > maxChunkSizeKb ? "FAIL" : "OK";
  return `${status}: ${chunk.name.padEnd(40)} (${chunk.sizeKb} KB)`;
}

function writeReport(report, outputPath = DEFAULT_REPORT_PATH) {
  const absolutePath = join(repoRoot, outputPath);
  const lines = [
    `[MYB-25] Bundle budget report`,
    `Generated: ${report.generatedAt}`,
    `Max JS chunk budget: ${report.maxChunkSizeKb} KB`,
    `Total JS assets: ${report.totalChunks}`,
    `Oversized chunks: ${report.oversizedChunks}`,
    "",
    ...report.chunks.map((chunk) => formatLineItem(chunk, report.maxChunkSizeKb)),
    ""
  ];

  mkdirSync(resolve(absolutePath, ".."), { recursive: true });
  writeFileSync(absolutePath, lines.join("\n"));

  return outputPath;
}

function run() {
  const report = buildBudgetReport();
  const reportPath = writeReport(report);

  console.log(`\n[MYB-25] Generated ${reportPath}`);
  console.log(`[MYB-25] Chunks over ${report.maxChunkSizeKb} KB: ${report.oversizedChunks}`);

  if (report.oversizedChunks > 0) {
    console.error(`[MYB-25] Budget check failed`);
    console.error(`Top failures:`);
    for (const chunk of report.chunks.filter((entry) => entry.sizeKb > report.maxChunkSizeKb)) {
      console.error(` - ${chunk.name}: ${chunk.sizeKb} KB`);
    }

    process.exitCode = 1;
  }
}

if (process.argv[1] === fileURLToPath(import.meta.url)) {
  run();
}

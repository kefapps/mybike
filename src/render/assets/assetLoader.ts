import type { AssetManifest, AssetLoadResult } from "./assetTypes";
import { validateAssetManifest } from "./assetValidation";

let cachedManifest: AssetManifest | null = null;
let cachedBasePath = "";

function resolveBasePath(filePath: string): string {
  if (import.meta.env?.PROD) {
    return import.meta.env.BASE_URL ?? "/";
  }
  return "/";
}

export async function loadAssetManifest(
  manifestUrl: string = "/assets/models/assets.manifest.json"
): Promise<AssetManifest> {
  if (cachedManifest) return cachedManifest;

  const response = await fetch(manifestUrl);
  if (!response.ok) {
    throw new Error(`Failed to load asset manifest: ${response.status} ${response.statusText}`);
  }

  const json: unknown = await response.json();
  const validation = validateAssetManifest(json);

  if (!validation.valid) {
    const details = validation.errors
      .map((e) => `  [${e.id ?? `index ${e.index}`}]: ${e.errors.join(", ")}`)
      .join("\n");
    throw new Error(`Invalid asset manifest:\n${details}`);
  }

  cachedManifest = json as AssetManifest;
  cachedBasePath = resolveBasePath(manifestUrl);
  return cachedManifest;
}

export function getAssetFromManifest(
  manifest: AssetManifest,
  id: string
): AssetLoadResult | null {
  const entry = manifest.assets.find((a) => a.id === id);
  if (!entry) return null;

  return {
    entry,
    url: `${cachedBasePath}assets/models/${entry.file}`
  };
}

export async function resolveAssetUrl(
  manifestUrl: string | undefined,
  manifest: AssetManifest | undefined,
  id: string
): Promise<AssetLoadResult | null> {
  const m = manifest ?? (await loadAssetManifest(manifestUrl));
  return getAssetFromManifest(m, id);
}

export function getManifestStats(manifest: AssetManifest): {
  assetCount: number;
  totalSizeKb: number;
  totalTriangles: number;
  biomes: string[];
} {
  const assetCount = manifest.assets.length;
  const totalSizeKb = manifest.assets.reduce((sum, a) => sum + a.sizeKb, 0);
  const totalTriangles = manifest.assets.reduce((sum, a) => sum + a.triangleBudget, 0);
  const biomes = [...new Set(manifest.assets.flatMap((a) => a.biomes))].sort();

  return { assetCount, totalSizeKb, totalTriangles, biomes };
}

export function filterAssetsByBiome(
  manifest: AssetManifest,
  biome: string
): AssetManifest["assets"] {
  return manifest.assets.filter((a) => a.biomes.includes(biome));
}

export function clearManifestCache(): void {
  cachedManifest = null;
  cachedBasePath = "";
}

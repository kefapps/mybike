import type { BiomeSceneryConfig, SceneryAssetKind, SceneryQuality } from "./sceneryTypes";

export const QUALITY_STEP_METERS: Record<SceneryQuality, number> = {
  low: 30,
  medium: 15,
  high: 8
};

const SHARED_FILL: SceneryAssetKind[] = [
  "rock",
  "grassTuft",
  "nearPost",
  "nearBlade",
  "nearStone"
];

const COAST_ASSETS: SceneryAssetKind[] = [...SHARED_FILL, "reed", "berm", "ridge", "farTree"];
const FOREST_ASSETS: SceneryAssetKind[] = [
  ...SHARED_FILL,
  "fern",
  "treeTrunk",
  "treeCanopy",
  "roadSign",
  "berm",
  "ridge",
  "farTree"
];

export const BIOME_SCENERY_CONFIGS: Record<string, BiomeSceneryConfig> = {
  coast: {
    assetKinds: COAST_ASSETS,
    minLateralDistance: 7,
    maxLateralDistance: 120,
    instancesPerSample: 4
  },
  forest: {
    assetKinds: FOREST_ASSETS,
    minLateralDistance: 7,
    maxLateralDistance: 120,
    instancesPerSample: 5
  },
  placeholder: {
    assetKinds: SHARED_FILL,
    minLateralDistance: 7,
    maxLateralDistance: 80,
    instancesPerSample: 2
  }
};

export const ASSET_PLACEMENT_BOUNDS: Record<SceneryAssetKind, { minSide: number; maxSide: number; minScale: number; maxScale: number }> = {
  treeTrunk: { minSide: 20, maxSide: 40, minScale: 0.84, maxScale: 1.24 },
  treeCanopy: { minSide: 20, maxSide: 40, minScale: 0.84, maxScale: 1.24 },
  rock: { minSide: 12, maxSide: 24, minScale: 0.8, maxScale: 1.4 },
  grassTuft: { minSide: 8, maxSide: 14, minScale: 0.65, maxScale: 1.1 },
  roadSign: { minSide: 11, maxSide: 16, minScale: 0.9, maxScale: 1.15 },
  berm: { minSide: 26, maxSide: 40, minScale: 0.72, maxScale: 1.0 },
  ridge: { minSide: 70, maxSide: 110, minScale: 0.82, maxScale: 1.1 },
  farTree: { minSide: 45, maxSide: 75, minScale: 0.82, maxScale: 0.98 },
  nearPost: { minSide: 6, maxSide: 8, minScale: 0.78, maxScale: 1.0 },
  nearBlade: { minSide: 7, maxSide: 9, minScale: 0.68, maxScale: 1.0 },
  nearStone: { minSide: 9, maxSide: 13, minScale: 0.9, maxScale: 1.35 },
  reed: { minSide: 10, maxSide: 14, minScale: 0.82, maxScale: 1.05 },
  fern: { minSide: 8, maxSide: 12, minScale: 0.72, maxScale: 1.1 }
};

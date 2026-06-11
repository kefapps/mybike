import type { Vec3 } from "../route";

export type SceneryAssetKind =
  | "treeTrunk"
  | "treeCanopy"
  | "rock"
  | "grassTuft"
  | "roadSign"
  | "berm"
  | "ridge"
  | "farTree"
  | "nearPost"
  | "nearBlade"
  | "nearStone"
  | "reed"
  | "fern";

export type SceneryInstance = {
  kind: SceneryAssetKind;
  position: Vec3;
  rotation: Vec3;
  scale: Vec3;
  biomeId: string;
  side: "left" | "right";
};

export type SceneryQuality = "low" | "medium" | "high";

export type BiomeSceneryConfig = {
  assetKinds: SceneryAssetKind[];
  minLateralDistance: number;
  maxLateralDistance: number;
  instancesPerSample: number;
};

export type SceneryPlacementBounds = {
  minLateralDistance: number;
  maxLateralDistance: number;
  minScale: number;
  maxScale: number;
};

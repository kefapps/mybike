export type AssetScale = {
  x: number;
  y: number;
  z: number;
};

export type AssetManifestEntry = {
  id: string;
  name: string;
  file: string;
  sourceUrl: string;
  author: string;
  license: string;
  attributionRequired: boolean;
  tags: string[];
  biomes: string[];
  animated: boolean;
  sizeKb: number;
  triangleBudget: number;
  scale: AssetScale;
  notes?: string;
};

export type AssetManifest = {
  version: string;
  assets: AssetManifestEntry[];
};

export type AssetLoadResult = {
  entry: AssetManifestEntry;
  url: string;
};

export type AssetValidationError = {
  index: number;
  id: string | undefined;
  errors: string[];
};

export type AssetManifestValidation = {
  valid: boolean;
  errors: AssetValidationError[];
};

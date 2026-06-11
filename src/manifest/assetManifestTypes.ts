export type AssetLicense =
  | "CC0"
  | "CC-BY"
  | "CC-BY-SA"
  | "CC-BY-NC"
  | "CC-BY-NC-SA"
  | "CC-BY-ND"
  | "CC-BY-NC-ND"
  | "MIT"
  | "Apache-2.0"
  | "OFL"
  | "Public Domain"
  | "Other";

export type AssetCategory =
  | "3d-model"
  | "texture"
  | "hdri"
  | "audio"
  | "font"
  | "sprite"
  | "code"
  | "other";

export type AssetManifestEntry = {
  id: string;
  name: string;
  author: string;
  source: string;
  category: AssetCategory;
  license: AssetLicense;
  licenseUrl?: string;
  modifications?: string;
  notes?: string;
};

export type AssetManifest = {
  version: number;
  policy: {
    ticket: string;
    summary: string;
    licensePolicyUrl: string;
  };
  assets: AssetManifestEntry[];
};

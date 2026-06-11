import rawManifest from "./assets-manifest.json";
import type {
  AssetLicense,
  AssetManifest,
  AssetManifestEntry
} from "./assetManifestTypes";

const ALLOWED_LICENSES: ReadonlySet<AssetLicense> = new Set([
  "CC0",
  "CC-BY",
  "CC-BY-SA",
  "CC-BY-NC",
  "CC-BY-NC-SA",
  "CC-BY-ND",
  "CC-BY-NC-ND",
  "MIT",
  "Apache-2.0",
  "OFL",
  "Public Domain",
  "Other"
]);

const ALLOWED_CATEGORIES = new Set([
  "3d-model",
  "texture",
  "hdri",
  "audio",
  "font",
  "sprite",
  "code",
  "other"
]);

function isString(value: unknown): value is string {
  return typeof value === "string" && value.length > 0;
}

function validateEntry(raw: unknown, index: number): AssetManifestEntry {
  if (typeof raw !== "object" || raw === null) {
    throw new Error(
      `assets-manifest: l'entree d'index ${index} n'est pas un objet.`
    );
  }

  const candidate = raw as Record<string, unknown>;

  if (!isString(candidate.id)) {
    throw new Error(
      `assets-manifest: l'entree d'index ${index} n'a pas de champ "id".`
    );
  }

  if (!isString(candidate.name)) {
    throw new Error(
      `assets-manifest: l'entree ${candidate.id} n'a pas de champ "name".`
    );
  }

  if (!isString(candidate.author)) {
    throw new Error(
      `assets-manifest: l'entree ${candidate.id} n'a pas de champ "author".`
    );
  }

  if (!isString(candidate.source)) {
    throw new Error(
      `assets-manifest: l'entree ${candidate.id} n'a pas de champ "source".`
    );
  }

  if (!isString(candidate.category)) {
    throw new Error(
      `assets-manifest: l'entree ${candidate.id} n'a pas de champ "category".`
    );
  }

  if (!ALLOWED_CATEGORIES.has(candidate.category)) {
    throw new Error(
      `assets-manifest: l'entree ${candidate.id} a une categorie inconnue "${candidate.category}".`
    );
  }

  if (!isString(candidate.license)) {
    throw new Error(
      `assets-manifest: l'entree ${candidate.id} n'a pas de champ "license".`
    );
  }

  if (!ALLOWED_LICENSES.has(candidate.license as AssetLicense)) {
    throw new Error(
      `assets-manifest: l'entree ${candidate.id} a une licence inconnue "${candidate.license}".`
    );
  }

  if (
    candidate.licenseUrl !== undefined &&
    typeof candidate.licenseUrl !== "string"
  ) {
    throw new Error(
      `assets-manifest: l'entree ${candidate.id} a un licenseUrl non textuel.`
    );
  }

  if (candidate.modifications !== undefined && typeof candidate.modifications !== "string") {
    throw new Error(
      `assets-manifest: l'entree ${candidate.id} a un champ modifications non textuel.`
    );
  }

  if (candidate.notes !== undefined && typeof candidate.notes !== "string") {
    throw new Error(
      `assets-manifest: l'entree ${candidate.id} a un champ notes non textuel.`
    );
  }

  return {
    id: candidate.id,
    name: candidate.name,
    author: candidate.author,
    source: candidate.source,
    category: candidate.category as AssetManifest["assets"][number]["category"],
    license: candidate.license as AssetLicense,
    licenseUrl: candidate.licenseUrl,
    modifications: candidate.modifications,
    notes: candidate.notes
  };
}

export function loadAssetManifest(): AssetManifestEntry[] {
  const raw = rawManifest as unknown as {
    version?: unknown;
    policy?: unknown;
    assets?: unknown;
  };

  if (typeof raw.version !== "number") {
    throw new Error('assets-manifest: champ "version" manquant ou non numerique.');
  }

  if (typeof raw.policy !== "object" || raw.policy === null) {
    throw new Error('assets-manifest: champ "policy" manquant.');
  }

  if (!Array.isArray(raw.assets)) {
    throw new Error('assets-manifest: champ "assets" absent ou non tableau.');
  }

  return raw.assets.map((entry, index) => validateEntry(entry, index));
}

export function loadAssetManifestBundle(): AssetManifest {
  const raw = rawManifest as unknown as {
    version: number;
    policy: { ticket: string; summary: string; licensePolicyUrl: string };
    assets: unknown[];
  };

  return {
    version: raw.version,
    policy: raw.policy,
    assets: loadAssetManifest()
  };
}

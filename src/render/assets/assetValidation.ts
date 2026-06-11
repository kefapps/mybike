import type {
  AssetManifest,
  AssetManifestEntry,
  AssetManifestValidation
} from "./assetTypes";

function isNonEmptyString(value: unknown): value is string {
  return typeof value === "string" && value.trim().length > 0;
}

function isFiniteNumber(value: unknown): value is number {
  return typeof value === "number" && Number.isFinite(value);
}

function isStringArray(value: unknown): value is string[] {
  if (!Array.isArray(value)) return false;
  return value.length > 0 && value.every((v) => typeof v === "string");
}

function isAssetScale(value: unknown): value is { x: number; y: number; z: number } {
  if (value === null || value === undefined || typeof value !== "object") return false;
  const s = value as Record<string, unknown>;
  return isFiniteNumber(s.x) && isFiniteNumber(s.y) && isFiniteNumber(s.z);
}

function validateEntry(entry: unknown, index: number): string[] {
  const errors: string[] = [];

  if (entry === null || entry === undefined || typeof entry !== "object") {
    errors.push("entry is not an object");
    return errors;
  }

  const e = entry as Record<string, unknown>;
  const id = isNonEmptyString(e.id) ? e.id : `asset[${index}]`;

  if (!isNonEmptyString(e.id)) {
    errors.push("missing or invalid id");
  }

  if (!isNonEmptyString(e.name)) {
    errors.push("missing or invalid name");
  }

  if (!isNonEmptyString(e.file)) {
    errors.push("missing or invalid file");
  }

  if (!isNonEmptyString(e.sourceUrl)) {
    errors.push("missing or invalid sourceUrl");
  }

  if (!isNonEmptyString(e.author)) {
    errors.push("missing or invalid author");
  }

  if (!isNonEmptyString(e.license)) {
    errors.push("missing or invalid license");
  }

  if (typeof e.attributionRequired !== "boolean") {
    errors.push("missing or invalid attributionRequired");
  }

  if (!isStringArray(e.tags)) {
    errors.push("missing or invalid tags (must be a non-empty array of strings)");
  }

  if (!isStringArray(e.biomes)) {
    errors.push("missing or invalid biomes (must be a non-empty array of strings)");
  }

  if (typeof e.animated !== "boolean") {
    errors.push("missing or invalid animated");
  }

  if (!isFiniteNumber(e.sizeKb) || (e.sizeKb as number) <= 0) {
    errors.push("missing or invalid sizeKb (must be a positive number)");
  }

  if (!isFiniteNumber(e.triangleBudget) || (e.triangleBudget as number) <= 0) {
    errors.push("missing or invalid triangleBudget (must be a positive number)");
  }

  if (e.scale !== undefined && !isAssetScale(e.scale)) {
    errors.push("invalid scale (must be {x, y, z} with finite numbers)");
  }

  return errors;
}

export function validateAssetManifest(
  manifest: unknown
): AssetManifestValidation {
  if (manifest === null || manifest === undefined || typeof manifest !== "object") {
    return {
      valid: false,
      errors: [{ index: -1, id: undefined, errors: ["manifest is not an object"] }]
    };
  }

  const m = manifest as Record<string, unknown>;

  if (!m.version || typeof m.version !== "string") {
    return {
      valid: false,
      errors: [{ index: -1, id: undefined, errors: ["missing or invalid version"] }]
    };
  }

  if (!Array.isArray(m.assets)) {
    return {
      valid: false,
      errors: [{ index: -1, id: undefined, errors: ["missing or invalid assets array"] }]
    };
  }

  const errors = (m.assets as unknown[])
    .map((entry, index) => {
      const entryErrors = validateEntry(entry, index);
      const e = entry as Record<string, unknown> | null;
      return {
        index,
        id: isNonEmptyString(e?.id) ? (e as Record<string, string>).id : undefined,
        errors: entryErrors
      };
    })
    .filter((e) => e.errors.length > 0);

  return {
    valid: errors.length === 0,
    errors
  };
}

export function validateAssetManifestEntry(
  entry: AssetManifestEntry
): string[] {
  return validateEntry(entry, 0);
}

export function isAssetManifest(value: unknown): value is AssetManifest {
  return validateAssetManifest(value).valid;
}

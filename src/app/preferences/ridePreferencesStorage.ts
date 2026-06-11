import {
  DEFAULT_RIDE_PREFERENCES,
  createInitialRidePreferences,
  withZenMode,
  type RidePreferences
} from "./ridePreferences";

const STORAGE_KEY = "mybike.ride-preferences.v1";

export type RidePreferencesStorage = {
  getItem(key: string): string | null;
  setItem(key: string, value: string): void;
  removeItem(key: string): void;
};

export type StorageLike = Pick<Window, "localStorage"> | {
  localStorage: RidePreferencesStorage;
};

export type LoadPreferencesResult = {
  preferences: RidePreferences;
  storageAvailable: boolean;
  restored: boolean;
};

export function loadRidePreferences(
  storage: RidePreferencesStorage | null
): LoadPreferencesResult {
  if (storage === null) {
    return {
      preferences: createInitialRidePreferences(),
      storageAvailable: false,
      restored: false
    };
  }

  let raw: string | null = null;
  try {
    raw = storage.getItem(STORAGE_KEY);
  } catch {
    return {
      preferences: createInitialRidePreferences(),
      storageAvailable: false,
      restored: false
    };
  }

  if (raw === null) {
    return {
      preferences: createInitialRidePreferences(),
      storageAvailable: true,
      restored: false
    };
  }

  const parsed = parsePreferencesJson(raw);
  if (parsed === null) {
    return {
      preferences: createInitialRidePreferences(),
      storageAvailable: true,
      restored: false
    };
  }

  return {
    preferences: parsed,
    storageAvailable: true,
    restored: true
  };
}

export function saveRidePreferences(
  storage: RidePreferencesStorage | null,
  preferences: RidePreferences
): boolean {
  if (storage === null) {
    return false;
  }

  try {
    storage.setItem(STORAGE_KEY, serializePreferences(preferences));
    return true;
  } catch {
    return false;
  }
}

export function clearRidePreferences(
  storage: RidePreferencesStorage | null
): boolean {
  if (storage === null) {
    return false;
  }

  try {
    storage.removeItem(STORAGE_KEY);
    return true;
  } catch {
    return false;
  }
}

export function resolveWindowStorage(
  target: StorageLike | null | undefined
): RidePreferencesStorage | null {
  if (target === null || target === undefined) {
    return null;
  }

  const candidate = target.localStorage;
  if (
    candidate === null ||
    candidate === undefined ||
    typeof candidate.getItem !== "function" ||
    typeof candidate.setItem !== "function" ||
    typeof candidate.removeItem !== "function"
  ) {
    return null;
  }

  return candidate;
}

export function serializePreferences(preferences: RidePreferences): string {
  return JSON.stringify({ zenMode: preferences.zenMode === true });
}

export function parsePreferencesJson(raw: string): RidePreferences | null {
  if (typeof raw !== "string" || raw.length === 0) {
    return null;
  }

  let parsed: unknown;
  try {
    parsed = JSON.parse(raw);
  } catch {
    return null;
  }

  if (!isRecord(parsed)) {
    return null;
  }

  const rawZenMode = parsed.zenMode;
  if (rawZenMode === true || rawZenMode === false) {
    return withZenMode(DEFAULT_RIDE_PREFERENCES, rawZenMode);
  }

  return null;
}

function isRecord(value: unknown): value is Record<string, unknown> {
  return typeof value === "object" && value !== null && !Array.isArray(value);
}

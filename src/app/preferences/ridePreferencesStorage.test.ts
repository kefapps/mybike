import { describe, expect, it, vi } from "vitest";

import type { RidePreferencesStorage } from "./ridePreferencesStorage";
import {
  clearRidePreferences,
  loadRidePreferences,
  parsePreferencesJson,
  resolveWindowStorage,
  saveRidePreferences,
  serializePreferences
} from "./ridePreferencesStorage";
import { DEFAULT_RIDE_PREFERENCES, withZenMode } from "./ridePreferences";

function createMemoryStorage(initial: Record<string, string> = {}): {
  storage: RidePreferencesStorage;
  entries: Record<string, string>;
} {
  const entries: Record<string, string> = { ...initial };
  const storage: RidePreferencesStorage = {
    getItem(key) {
      return Object.prototype.hasOwnProperty.call(entries, key)
        ? entries[key]
        : null;
    },
    setItem(key, value) {
      entries[key] = value;
    },
    removeItem(key) {
      delete entries[key];
    }
  };

  return { storage, entries };
}

describe("ridePreferencesStorage", () => {
  it("returns defaults when storage is unavailable", () => {
    const result = loadRidePreferences(null);

    expect(result.preferences).toEqual({ zenMode: false });
    expect(result.storageAvailable).toBe(false);
    expect(result.restored).toBe(false);
  });

  it("returns defaults when storage throws on read", () => {
    const storage: RidePreferencesStorage = {
      getItem: vi.fn(() => {
        throw new Error("blocked");
      }),
      setItem: vi.fn(),
      removeItem: vi.fn()
    };
    const result = loadRidePreferences(storage);

    expect(result.preferences).toEqual({ zenMode: false });
    expect(result.storageAvailable).toBe(false);
    expect(result.restored).toBe(false);
  });

  it("returns defaults when storage is empty", () => {
    const { storage } = createMemoryStorage();
    const result = loadRidePreferences(storage);

    expect(result.preferences).toEqual({ zenMode: false });
    expect(result.storageAvailable).toBe(true);
    expect(result.restored).toBe(false);
  });

  it("restores a previously saved zen preference", () => {
    const saved = withZenMode(DEFAULT_RIDE_PREFERENCES, true);
    const { storage } = createMemoryStorage({
      "mybike.ride-preferences.v1": serializePreferences(saved)
    });
    const result = loadRidePreferences(storage);

    expect(result.preferences).toEqual({ zenMode: true });
    expect(result.restored).toBe(true);
  });

  it("falls back to defaults on corrupted JSON", () => {
    const { storage } = createMemoryStorage({
      "mybike.ride-preferences.v1": "not json"
    });
    const result = loadRidePreferences(storage);

    expect(result.preferences).toEqual({ zenMode: false });
    expect(result.restored).toBe(false);
  });

  it("falls back to defaults on a JSON value of the wrong shape", () => {
    const { storage } = createMemoryStorage({
      "mybike.ride-preferences.v1": JSON.stringify({ zenMode: "yes" })
    });
    const result = loadRidePreferences(storage);

    expect(result.preferences).toEqual({ zenMode: false });
    expect(result.restored).toBe(false);
  });

  it("persists zen preferences on save and reports success", () => {
    const { storage, entries } = createMemoryStorage();
    const next = withZenMode(DEFAULT_RIDE_PREFERENCES, true);

    const saved = saveRidePreferences(storage, next);

    expect(saved).toBe(true);
    expect(entries["mybike.ride-preferences.v1"]).toBe(
      serializePreferences(next)
    );
  });

  it("returns false when storage is unavailable on save", () => {
    expect(saveRidePreferences(null, DEFAULT_RIDE_PREFERENCES)).toBe(false);
  });

  it("returns false when storage throws on write", () => {
    const storage: RidePreferencesStorage = {
      getItem: vi.fn(() => null),
      setItem: vi.fn(() => {
        throw new Error("quota");
      }),
      removeItem: vi.fn()
    };
    expect(saveRidePreferences(storage, DEFAULT_RIDE_PREFERENCES)).toBe(false);
  });

  it("clears stored preferences on request", () => {
    const { storage, entries } = createMemoryStorage({
      "mybike.ride-preferences.v1": serializePreferences(
        withZenMode(DEFAULT_RIDE_PREFERENCES, true)
      )
    });

    expect(clearRidePreferences(storage)).toBe(true);
    expect(entries["mybike.ride-preferences.v1"]).toBeUndefined();
    expect(clearRidePreferences(null)).toBe(false);
  });

  it("resolves window storage when localStorage is present and usable", () => {
    const { storage } = createMemoryStorage();
    const target = { localStorage: storage };

    expect(resolveWindowStorage(target)).toBe(storage);
  });

  it("returns null when window storage is missing or unusable", () => {
    expect(resolveWindowStorage(null)).toBeNull();
    expect(resolveWindowStorage(undefined)).toBeNull();
    expect(resolveWindowStorage({} as unknown as { localStorage: RidePreferencesStorage })).toBeNull();
    expect(
      resolveWindowStorage({
        localStorage: {
          getItem: "noop",
          setItem: () => undefined,
          removeItem: () => undefined
        } as unknown as RidePreferencesStorage
      })
    ).toBeNull();
  });

  it("round-trips serialization through parsePreferencesJson", () => {
    const source = withZenMode(DEFAULT_RIDE_PREFERENCES, true);
    const parsed = parsePreferencesJson(serializePreferences(source));

    expect(parsed).toEqual({ zenMode: true });
  });

  it("rejects empty or non-string input to parsePreferencesJson", () => {
    expect(parsePreferencesJson("")).toBeNull();
    expect(parsePreferencesJson(null as unknown as string)).toBeNull();
    expect(parsePreferencesJson(undefined as unknown as string)).toBeNull();
  });
});

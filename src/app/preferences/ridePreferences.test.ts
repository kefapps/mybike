import { describe, expect, it } from "vitest";

import {
  DEFAULT_RIDE_PREFERENCES,
  createInitialRidePreferences,
  isZenMode,
  toggleZenMode,
  withZenMode
} from "./ridePreferences";

describe("ridePreferences", () => {
  it("returns a fresh, zenMode-off initial state", () => {
    const initial = createInitialRidePreferences();

    expect(initial).toEqual({ zenMode: false });
    expect(initial).not.toBe(DEFAULT_RIDE_PREFERENCES);
    expect(isZenMode(initial)).toBe(false);
  });

  it("treats the default preferences as non-zen", () => {
    expect(isZenMode(DEFAULT_RIDE_PREFERENCES)).toBe(false);
  });

  it("returns a new preferences object when toggling zen", () => {
    const initial = createInitialRidePreferences();
    const zen = withZenMode(initial, true);
    const off = withZenMode(zen, false);

    expect(zen).toEqual({ zenMode: true });
    expect(isZenMode(zen)).toBe(true);
    expect(off).toEqual({ zenMode: false });
    expect(off).not.toBe(zen);
    expect(initial.zenMode).toBe(false);
  });

  it("ignores non-boolean zen mode inputs", () => {
    const initial = createInitialRidePreferences();

    expect(withZenMode(initial, "yes" as unknown as boolean)).toEqual({
      zenMode: false
    });
    expect(withZenMode(initial, 1 as unknown as boolean)).toEqual({
      zenMode: false
    });
    expect(withZenMode(initial, null as unknown as boolean)).toEqual({
      zenMode: false
    });
  });

  it("toggles zen mode in place without mutating the input", () => {
    const initial = createInitialRidePreferences();
    const next = toggleZenMode(initial);

    expect(next.zenMode).toBe(true);
    expect(initial.zenMode).toBe(false);

    const back = toggleZenMode(next);
    expect(back.zenMode).toBe(false);
    expect(next.zenMode).toBe(true);
  });
});

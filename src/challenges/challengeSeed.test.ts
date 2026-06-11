import { describe, expect, it } from "vitest";

import {
  hashStringToUint32,
  localDateKey,
  localDayWindow
} from "./challengeSeed";

describe("challengeSeed", () => {
  it("hashes the same string to the same uint32", () => {
    expect(hashStringToUint32("2026-06-10")).toBe(
      hashStringToUint32("2026-06-10")
    );
    expect(hashStringToUint32("defi")).toBe(hashStringToUint32("defi"));
  });

  it("hashes different strings to different uint32 values", () => {
    const a = hashStringToUint32("2026-06-10");
    const b = hashStringToUint32("2026-06-11");

    expect(a).not.toBe(b);
  });

  it("formats a local date key as YYYY-MM-DD", () => {
    expect(localDateKey(Date.UTC(2026, 5, 10, 23, 30, 0))).toMatch(
      /^\d{4}-\d{2}-\d{2}$/
    );
  });

  it("returns a 24h window for the local day", () => {
    const window = localDayWindow(Date.UTC(2026, 5, 10, 12, 0, 0));

    expect(window.endMs - window.startMs).toBe(24 * 60 * 60 * 1000);
    expect(window.dateKey).toMatch(/^\d{4}-\d{2}-\d{2}$/);
  });

  it("falls back to current time when given non-finite input", () => {
    const before = Date.now();

    const key = localDateKey(Number.NaN);
    const window = localDayWindow(Number.POSITIVE_INFINITY);

    expect(key).toMatch(/^\d{4}-\d{2}-\d{2}$/);
    expect(window.endMs).toBeGreaterThanOrEqual(before);
  });
});

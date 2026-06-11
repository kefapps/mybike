import { describe, expect, it } from "vitest";

import { createLcgRng } from "./challengeRng";

describe("createLcgRng", () => {
  it("returns deterministic values for the same seed", () => {
    const a = createLcgRng(42);
    const b = createLcgRng(42);

    expect([a.next(), a.next(), a.next()]).toEqual([
      b.next(),
      b.next(),
      b.next()
    ]);
  });

  it("returns different streams for different seeds", () => {
    const a = createLcgRng(1);
    const b = createLcgRng(2);

    expect(a.next()).not.toBe(b.next());
  });

  it("nextInt always returns a value in [0, maxExclusive)", () => {
    const rng = createLcgRng(7);

    for (let i = 0; i < 200; i += 1) {
      const value = rng.nextInt(5);

      expect(value).toBeGreaterThanOrEqual(0);
      expect(value).toBeLessThan(5);
      expect(Number.isInteger(value)).toBe(true);
    }
  });

  it("nextRange returns an integer in [minInclusive, maxInclusive]", () => {
    const rng = createLcgRng(99);

    for (let i = 0; i < 200; i += 1) {
      const value = rng.nextRange(3, 6);

      expect(value).toBeGreaterThanOrEqual(3);
      expect(value).toBeLessThanOrEqual(6);
      expect(Number.isInteger(value)).toBe(true);
    }
  });

  it("pick returns a value from the list", () => {
    const rng = createLcgRng(11);
    const list = ["a", "b", "c"] as const;

    for (let i = 0; i < 50; i += 1) {
      expect(list).toContain(rng.pick(list));
    }
  });

  it("falls back to seed 1 for non-finite or zero seed", () => {
    const rng = createLcgRng(0);
    const fallback = createLcgRng(1);

    expect(rng.next()).toBe(fallback.next());
  });
});

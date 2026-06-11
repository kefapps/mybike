import { describe, expect, it } from "vitest";

import { generateDailyChallenge, isDailyChallenge } from "./dailyChallenge";

describe("dailyChallenge", () => {
  it("is deterministic for the same local date", () => {
    const a = generateDailyChallenge(Date.UTC(2026, 5, 10, 12, 0, 0));
    const b = generateDailyChallenge(Date.UTC(2026, 5, 10, 18, 0, 0));

    expect(a).toEqual(b);
  });

  it("produces a different challenge for a different local date", () => {
    const a = generateDailyChallenge(Date.UTC(2026, 5, 10, 9, 0, 0));
    const b = generateDailyChallenge(Date.UTC(2026, 5, 11, 9, 0, 0));

    expect(a.id).not.toBe(b.id);
  });

  it("always produces a daily challenge with a 24h window", () => {
    const challenge = generateDailyChallenge(Date.UTC(2026, 5, 10, 9, 0, 0));

    expect(isDailyChallenge(challenge)).toBe(true);
    expect(challenge.expiresAtMs - challenge.issuedAtMs).toBe(
      24 * 60 * 60 * 1000
    );
    expect(challenge.title).toContain("du jour");
  });

  it("rotates the four target kinds across days", () => {
    const seenKinds = new Set<string>();
    const base = Date.UTC(2026, 0, 1, 12, 0, 0);
    const dayMs = 24 * 60 * 60 * 1000;

    for (let i = 0; i < 20; i += 1) {
      const challenge = generateDailyChallenge(base + i * dayMs);
      seenKinds.add(challenge.target.kind);
    }

    expect(seenKinds.size).toBeGreaterThanOrEqual(2);
    expect(["distance", "explore", "climb", "beat-time"]).toEqual(
      expect.arrayContaining(Array.from(seenKinds))
    );
  });

  it("uses stable, finite, non-negative reward values", () => {
    const base = Date.UTC(2026, 0, 1, 12, 0, 0);
    const dayMs = 24 * 60 * 60 * 1000;

    for (let i = 0; i < 14; i += 1) {
      const challenge = generateDailyChallenge(base + i * dayMs);
      expect(challenge.reward.xp).toBeGreaterThan(0);
      expect(Number.isFinite(challenge.reward.xp)).toBe(true);
      expect(Number.isInteger(challenge.reward.xp)).toBe(true);
    }
  });

  it("clamps non-finite nowMs to current time", () => {
    const challenge = generateDailyChallenge(Number.NaN);

    expect(isDailyChallenge(challenge)).toBe(true);
    expect(challenge.id).toMatch(/^daily-/);
  });
});

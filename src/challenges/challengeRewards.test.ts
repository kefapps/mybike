import { describe, expect, it } from "vitest";

import { generateDailyChallenge } from "./dailyChallenge";
import {
  challengeBadgeSeed,
  summarizeReward
} from "./challengeRewards";

describe("challengeRewards", () => {
  it("summarizes an xp reward without badge", () => {
    expect(summarizeReward({ xp: 30, badgeId: null })).toBe("+30 XP");
  });

  it("summarizes an xp reward with a badge", () => {
    expect(summarizeReward({ xp: 30, badgeId: "badge-x" })).toBe(
      "+30 XP · badge badge-x"
    );
  });

  it("returns a no-reward label for empty rewards", () => {
    expect(summarizeReward({ xp: 0, badgeId: null })).toBe(
      "Pas de recompense"
    );
  });

  it("produces a deterministic badge seed per challenge", () => {
    const base = Date.UTC(2026, 0, 1, 12, 0, 0);
    const dayMs = 24 * 60 * 60 * 1000;
    const a = generateDailyChallenge(base);
    const b = generateDailyChallenge(base);

    expect(challengeBadgeSeed(a)).toBe(challengeBadgeSeed(b));
  });
});

import type { Challenge, ChallengeReward, ChallengeTarget } from "./challengeTypes";

function targetBadgeSuffix(target: ChallengeTarget): string {
  switch (target.kind) {
    case "distance":
      return "distance";
    case "explore":
      return "explore";
    case "climb":
      return "climb";
    case "beat-time":
      return "chrono";
    default:
      return "defi";
  }
}

export function summarizeReward(reward: ChallengeReward): string {
  if (reward.xp <= 0 && !reward.badgeId) {
    return "Pas de recompense";
  }

  const parts: string[] = [];

  if (reward.xp > 0) {
    parts.push(`+${reward.xp} XP`);
  }

  if (reward.badgeId) {
    parts.push(`badge ${reward.badgeId}`);
  }

  return parts.join(" · ");
}

export function challengeBadgeSeed(challenge: Challenge): string {
  return `${targetBadgeSuffix(challenge.target)}-${challenge.seed.toString(16)}`;
}

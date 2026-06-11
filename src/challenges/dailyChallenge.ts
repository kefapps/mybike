import { hashStringToUint32, localDateKey, localDayWindow } from "./challengeSeed";
import { createLcgRng, type Rng } from "./challengeRng";
import type {
  Challenge,
  ChallengeKind,
  ChallengeTarget,
  ChallengeTargetKind
} from "./challengeTypes";

const DAILY_TARGET_KINDS: readonly ChallengeTargetKind[] = [
  "distance",
  "explore",
  "climb",
  "beat-time"
] as const;

const DAILY_KIND_LABELS: Record<ChallengeKind, string> = {
  daily: "Defi du jour",
  climb: "Montee",
  exploration: "Exploration",
  chrono: "Chrono"
};

const TARGET_TITLES: Record<ChallengeTargetKind, string> = {
  distance: "Distance",
  explore: "Exploration",
  climb: "Montee",
  "beat-time": "Chrono"
};

function buildDistanceChallenge(
  seed: number,
  issuedAtMs: number,
  expiresAtMs: number
): Challenge {
  const rng = createLcgRng(seed);
  const totalMeters = rng.nextRange(450, 950);
  const meters = Math.round(totalMeters / 50) * 50;
  const xp = computeXp("distance", meters);

  return {
    id: `daily-distance-${seed.toString(16)}`,
    kind: "daily",
    title: `${TARGET_TITLES.distance} du jour`,
    description: `Parcours au moins ${meters} m aujourd'hui.`,
    target: { kind: "distance", meters },
    reward: { xp, badgeId: meters >= 900 ? "badge-distance-long" : null },
    seed,
    issuedAtMs,
    expiresAtMs
  };
}

function buildExploreChallenge(
  seed: number,
  issuedAtMs: number,
  expiresAtMs: number
): Challenge {
  const rng = createLcgRng(seed);
  const progress01 = Math.min(0.95, Math.max(0.5, rng.next() * 0.6 + 0.5));
  const percent = Math.round(progress01 * 100);
  const xp = computeXp("explore", progress01);

  return {
    id: `daily-explore-${seed.toString(16)}`,
    kind: "daily",
    title: `${TARGET_TITLES.explore} du jour`,
    description: `Atteins ${percent} % du parcours aujourd'hui.`,
    target: { kind: "explore", progress01 },
    reward: { xp, badgeId: progress01 >= 0.9 ? "badge-explorer" : null },
    seed,
    issuedAtMs,
    expiresAtMs
  };
}

function buildClimbChallenge(
  seed: number,
  issuedAtMs: number,
  expiresAtMs: number
): Challenge {
  const rng = createLcgRng(seed);
  const minEffort01 = Math.round((0.55 + rng.next() * 0.3) * 100) / 100;
  const seconds = rng.nextRange(20, 45);
  const xp = computeXp("climb", seconds);

  return {
    id: `daily-climb-${seed.toString(16)}`,
    kind: "daily",
    title: `${TARGET_TITLES.climb} du jour`,
    description: `Maintiens un effort superieur a ${Math.round(
      minEffort01 * 100
    )} % pendant ${seconds} s.`,
    target: { kind: "climb", minEffort01, seconds },
    reward: { xp, badgeId: seconds >= 40 ? "badge-climb" : null },
    seed,
    issuedAtMs,
    expiresAtMs
  };
}

function buildBeatTimeChallenge(
  seed: number,
  issuedAtMs: number,
  expiresAtMs: number
): Challenge {
  const rng = createLcgRng(seed);
  const maxSeconds = rng.nextRange(75, 110);
  const minAverageSpeedMps = Math.round((3.5 + rng.next() * 1.5) * 10) / 10;
  const xp = computeXp("beat-time", maxSeconds);

  return {
    id: `daily-beat-time-${seed.toString(16)}`,
    kind: "daily",
    title: `${TARGET_TITLES["beat-time"]} du jour`,
    description: `Termine la balade en moins de ${maxSeconds} s (moyenne ${minAverageSpeedMps
      .toFixed(1)
      .replace(".", ",")} m/s).`,
    target: { kind: "beat-time", maxSeconds, minAverageSpeedMps },
    reward: { xp, badgeId: maxSeconds <= 80 ? "badge-sprinter" : null },
    seed,
    issuedAtMs,
    expiresAtMs
  };
}

function computeXp(kind: ChallengeTarget["kind"], primary: number): number {
  switch (kind) {
    case "distance":
      return Math.round(30 + primary / 25);
    case "explore":
      return Math.round(40 + primary * 50);
    case "climb":
      return Math.round(35 + primary * 1.5);
    case "beat-time":
      return Math.round(120 - primary * 0.6);
    default:
      return 30;
  }
}

function pickTargetKind(rng: Rng): ChallengeTargetKind {
  const index = rng.nextInt(DAILY_TARGET_KINDS.length);

  return DAILY_TARGET_KINDS[index] as ChallengeTargetKind;
}

function buildChallengeForTarget(
  kind: ChallengeTargetKind,
  seed: number,
  issuedAtMs: number,
  expiresAtMs: number
): Challenge {
  switch (kind) {
    case "distance":
      return buildDistanceChallenge(seed, issuedAtMs, expiresAtMs);
    case "explore":
      return buildExploreChallenge(seed, issuedAtMs, expiresAtMs);
    case "climb":
      return buildClimbChallenge(seed, issuedAtMs, expiresAtMs);
    case "beat-time":
      return buildBeatTimeChallenge(seed, issuedAtMs, expiresAtMs);
    default:
      return buildDistanceChallenge(seed, issuedAtMs, expiresAtMs);
  }
}

export function generateDailyChallenge(nowMs: number = Date.now()): Challenge {
  const window = localDayWindow(nowMs);
  const dateKey = window.dateKey;
  const seed = hashStringToUint32(dateKey);
  const rng = createLcgRng(seed);
  const targetKind = pickTargetKind(rng);

  return buildChallengeForTarget(targetKind, seed, window.startMs, window.endMs);
}

export function getDailyChallengeId(nowMs: number = Date.now()): string {
  return `daily-${localDateKey(nowMs)}`;
}

export function isDailyChallenge(challenge: Challenge): boolean {
  return challenge.kind === "daily";
}

export { DAILY_KIND_LABELS, TARGET_TITLES };

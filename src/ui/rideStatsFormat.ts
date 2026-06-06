import { clampEffort01, type RideStats } from "../ride";

export const EMPTY_RIDE_STATS: RideStats = Object.freeze({
  elapsedMs: 0,
  distanceMeters: 0,
  averageSpeedMps: 0
});

export function getRideStatsOrEmpty(stats?: RideStats): RideStats {
  return stats ?? EMPTY_RIDE_STATS;
}

export function formatDuration(elapsedMs: number): string {
  const totalSeconds = Math.floor(coerceNonNegative(elapsedMs) / 1000);
  const minutes = Math.floor(totalSeconds / 60);
  const seconds = totalSeconds % 60;

  return `${minutes} min ${seconds.toString().padStart(2, "0")} s`;
}

export function formatDistance(distanceMeters: number): string {
  const safeDistanceMeters = coerceNonNegative(distanceMeters);

  if (safeDistanceMeters < 1000) {
    return `${Math.round(safeDistanceMeters)} m`;
  }

  return `${formatDecimal(safeDistanceMeters / 1000)} km`;
}

export function formatSpeed(speedMps: number): string {
  return `${formatDecimal(coerceNonNegative(speedMps) * 3.6)} km/h`;
}

export function formatEffortPercent(effort01: number): string {
  return `${Math.round(clampEffort01(effort01) * 100)} %`;
}

function coerceNonNegative(value: number): number {
  if (!Number.isFinite(value)) {
    return 0;
  }

  return Math.max(0, value);
}

function formatDecimal(value: number): string {
  return value.toFixed(1).replace(".", ",");
}

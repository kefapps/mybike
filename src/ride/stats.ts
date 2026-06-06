import type { RideStats } from "./rideTypes";

function coerceNonNegative(value: number): number {
  if (!Number.isFinite(value)) {
    return 0;
  }

  return Math.max(0, value);
}

export function calculateRideStats(
  elapsedMs: number,
  distanceMeters: number
): RideStats {
  const safeElapsedMs = coerceNonNegative(elapsedMs);
  const safeDistanceMeters = coerceNonNegative(distanceMeters);
  const elapsedSeconds = safeElapsedMs / 1000;

  return {
    elapsedMs: safeElapsedMs,
    distanceMeters: safeDistanceMeters,
    averageSpeedMps:
      elapsedSeconds > 0 ? safeDistanceMeters / elapsedSeconds : 0
  };
}

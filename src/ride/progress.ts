import type { RideProgress, RideProgressConfig } from "./rideTypes";

function coerceNonNegative(value: number): number {
  if (!Number.isFinite(value)) {
    return 0;
  }

  return Math.max(0, value);
}

export function createInitialRideProgress(): RideProgress {
  return {
    distanceMeters: 0,
    progress01: 0,
    completed: false
  };
}

export function advanceRouteProgress(
  previous: RideProgress,
  speedMps: number,
  dtSeconds: number,
  config: RideProgressConfig
): RideProgress {
  const totalDistanceMeters = coerceNonNegative(config.totalDistanceMeters);

  if (totalDistanceMeters <= 0) {
    return {
      distanceMeters: 0,
      progress01: 1,
      completed: true
    };
  }

  const previousDistanceMeters = Math.min(
    totalDistanceMeters,
    coerceNonNegative(previous.distanceMeters)
  );

  if (previous.completed) {
    return {
      distanceMeters: previousDistanceMeters,
      progress01: previousDistanceMeters / totalDistanceMeters,
      completed: previousDistanceMeters >= totalDistanceMeters
    };
  }

  const safeSpeedMps = coerceNonNegative(speedMps);
  const safeDtSeconds =
    Number.isFinite(dtSeconds) && dtSeconds > 0 ? dtSeconds : 0;
  const distanceMeters = Math.min(
    totalDistanceMeters,
    previousDistanceMeters + safeSpeedMps * safeDtSeconds
  );

  return {
    distanceMeters,
    progress01: distanceMeters / totalDistanceMeters,
    completed: distanceMeters >= totalDistanceMeters
  };
}

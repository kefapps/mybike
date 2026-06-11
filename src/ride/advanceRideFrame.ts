import { advanceRouteProgress, createInitialRideProgress } from "./progress";
import { calculateRideStats } from "./stats";
import {
  DEFAULT_SPEED_MAPPING_CONFIG,
  DEFAULT_SPEED_SMOOTHING_CONFIG,
  mapMockInputToSpeed,
  smoothSpeed
} from "./speed";
import type { RouteDefinition } from "../route/routeTypes";
import type {
  RideFrameConfig,
  RideFrameResult,
  RideFrameState,
  RideInputSource
} from "./rideTypes";

export const DEFAULT_RIDE_FRAME_CONFIG: RideFrameConfig = {
  speedMapping: DEFAULT_SPEED_MAPPING_CONFIG,
  speedSmoothing: DEFAULT_SPEED_SMOOTHING_CONFIG,
  progress: { totalDistanceMeters: 0 }
};

function coerceNonNegative(value: number): number {
  if (!Number.isFinite(value)) {
    return 0;
  }

  return Math.max(0, value);
}

function resolveTotalDistanceMeters(
  config: RideFrameConfig,
  route: RouteDefinition | null | undefined
): number {
  if (route && Number.isFinite(route.lengthMeters) && route.lengthMeters > 0) {
    return route.lengthMeters;
  }

  return config.progress.totalDistanceMeters;
}

export function createInitialRideFrameState(
  startedAtMs = 0
): RideFrameState {
  return {
    startedAtMs: coerceNonNegative(startedAtMs),
    speed: { speedMps: 0 },
    progress: createInitialRideProgress(),
    segmentStats: { currentSegmentId: null, entries: {} }
  };
}

export function advanceRideFrame(
  inputSource: RideInputSource,
  previous: RideFrameState,
  nowMs: number,
  dtSeconds: number,
  config: RideFrameConfig | null | undefined = DEFAULT_RIDE_FRAME_CONFIG,
  route: RouteDefinition | null | undefined = null
): RideFrameResult {
  const safeConfig = config ?? DEFAULT_RIDE_FRAME_CONFIG;
  const totalDistanceMeters = resolveTotalDistanceMeters(safeConfig, route);
  const safeNowMs = coerceNonNegative(nowMs);
  const sample = inputSource.read(safeNowMs);
  const targetSpeedMps = mapMockInputToSpeed(sample, safeConfig.speedMapping);
  const speed = smoothSpeed(
    targetSpeedMps,
    previous.speed,
    dtSeconds,
    safeConfig.speedSmoothing
  );
  const progress = advanceRouteProgress(
    previous.progress,
    speed.speedMps,
    dtSeconds,
    { totalDistanceMeters }
  );
  const elapsedMs = Math.max(0, safeNowMs - coerceNonNegative(previous.startedAtMs));
  const stats = calculateRideStats(elapsedMs, progress.distanceMeters);
  const state: RideFrameState = {
    startedAtMs: coerceNonNegative(previous.startedAtMs),
    speed,
    progress,
    segmentStats: previous.segmentStats
  };

  return {
    state,
    snapshot: {
      source: sample.source,
      input: sample,
      targetSpeedMps,
      speedMps: speed.speedMps,
      progress,
      elapsedMs,
      stats,
      completed: progress.completed
    }
  };
}

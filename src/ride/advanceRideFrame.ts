import { advanceRouteProgress, createInitialRideProgress } from "./progress";
import { calculateRideStats } from "./stats";
import {
  DEFAULT_SPEED_MAPPING_CONFIG,
  DEFAULT_SPEED_SMOOTHING_CONFIG,
  mapMockInputToSpeed,
  smoothSpeed
} from "./speed";
import type {
  RideFrameConfig,
  RideFrameResult,
  RideFrameState,
  RideInputSource
} from "./rideTypes";

export const DEFAULT_RIDE_FRAME_CONFIG: RideFrameConfig = {
  speedMapping: DEFAULT_SPEED_MAPPING_CONFIG,
  speedSmoothing: DEFAULT_SPEED_SMOOTHING_CONFIG,
  progress: { totalDistanceMeters: 1000 }
};

function coerceNonNegative(value: number): number {
  if (!Number.isFinite(value)) {
    return 0;
  }

  return Math.max(0, value);
}

export function createInitialRideFrameState(
  startedAtMs = 0
): RideFrameState {
  return {
    startedAtMs: coerceNonNegative(startedAtMs),
    speed: { speedMps: 0 },
    progress: createInitialRideProgress()
  };
}

export function advanceRideFrame(
  inputSource: RideInputSource,
  previous: RideFrameState,
  nowMs: number,
  dtSeconds: number,
  config: RideFrameConfig = DEFAULT_RIDE_FRAME_CONFIG
): RideFrameResult {
  const safeNowMs = coerceNonNegative(nowMs);
  const sample = inputSource.read(safeNowMs);
  const targetSpeedMps = mapMockInputToSpeed(sample, config.speedMapping);
  const speed = smoothSpeed(
    targetSpeedMps,
    previous.speed,
    dtSeconds,
    config.speedSmoothing
  );
  const progress = advanceRouteProgress(
    previous.progress,
    speed.speedMps,
    dtSeconds,
    config.progress
  );
  const elapsedMs = Math.max(0, safeNowMs - coerceNonNegative(previous.startedAtMs));
  const stats = calculateRideStats(elapsedMs, progress.distanceMeters);
  const state = {
    startedAtMs: coerceNonNegative(previous.startedAtMs),
    speed,
    progress
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

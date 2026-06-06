import { clampEffort01 } from "./mockRideInputSource";
import type {
  RideInputSample,
  SmoothedSpeedState,
  SpeedMappingConfig,
  SpeedSmoothingConfig
} from "./rideTypes";

export const DEFAULT_SPEED_MAPPING_CONFIG: SpeedMappingConfig = {
  minSpeedMps: 1.5,
  maxSpeedMps: 9
};

export const DEFAULT_SPEED_SMOOTHING_CONFIG: SpeedSmoothingConfig = {
  accelerationPerSecond: 2.5,
  decelerationPerSecond: 4
};

function coerceNonNegative(value: number, fallback: number): number {
  if (!Number.isFinite(value)) {
    return fallback;
  }

  return Math.max(0, value);
}

export function mapMockInputToSpeed(
  sample: RideInputSample,
  config: SpeedMappingConfig = DEFAULT_SPEED_MAPPING_CONFIG
): number {
  const effort01 = clampEffort01(sample.effort01);
  const minSpeedMps = coerceNonNegative(
    config.minSpeedMps,
    DEFAULT_SPEED_MAPPING_CONFIG.minSpeedMps
  );
  const maxSpeedMps = Math.max(
    minSpeedMps,
    coerceNonNegative(
      config.maxSpeedMps,
      DEFAULT_SPEED_MAPPING_CONFIG.maxSpeedMps
    )
  );

  return minSpeedMps + (maxSpeedMps - minSpeedMps) * effort01;
}

export function smoothSpeed(
  targetSpeedMps: number,
  previous: SmoothedSpeedState,
  dtSeconds: number,
  config: SpeedSmoothingConfig = DEFAULT_SPEED_SMOOTHING_CONFIG
): SmoothedSpeedState {
  const currentSpeedMps = coerceNonNegative(previous.speedMps, 0);
  const safeTargetSpeedMps = coerceNonNegative(targetSpeedMps, 0);

  if (!Number.isFinite(dtSeconds) || dtSeconds <= 0) {
    return { speedMps: currentSpeedMps };
  }

  if (safeTargetSpeedMps > currentSpeedMps) {
    const maxAcceleration = coerceNonNegative(
      config.accelerationPerSecond,
      DEFAULT_SPEED_SMOOTHING_CONFIG.accelerationPerSecond
    );
    const nextSpeedMps = Math.min(
      safeTargetSpeedMps,
      currentSpeedMps + maxAcceleration * dtSeconds
    );

    return { speedMps: nextSpeedMps };
  }

  if (safeTargetSpeedMps < currentSpeedMps) {
    const maxDeceleration = coerceNonNegative(
      config.decelerationPerSecond,
      DEFAULT_SPEED_SMOOTHING_CONFIG.decelerationPerSecond
    );
    const nextSpeedMps = Math.max(
      safeTargetSpeedMps,
      currentSpeedMps - maxDeceleration * dtSeconds
    );

    return { speedMps: nextSpeedMps };
  }

  return { speedMps: currentSpeedMps };
}

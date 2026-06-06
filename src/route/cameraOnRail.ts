import type { RideFrameSnapshot, RideProgress } from "../ride";
import { addVec3, clampFinite, scaleVec3 } from "./routeMath";
import { sampleRouteAt } from "./sampleRouteAt";
import type {
  CameraRailConfig,
  CameraRigSnapshot,
  RouteDefinition,
  RouteProgressInput
} from "./routeTypes";

export const DEFAULT_CAMERA_RAIL_CONFIG: CameraRailConfig = {
  heightMeters: 1.6,
  lookAheadMeters: 12,
  fovDegrees: 70
};

export function cameraOnRail(
  route: RouteDefinition | null | undefined,
  progress: RouteProgressInput,
  config: Partial<CameraRailConfig> | null | undefined = {}
): CameraRigSnapshot {
  const safeConfig = sanitizeCameraConfig(config);
  const progress01 = extractProgress01(progress);
  const sample = sampleRouteAt(route, progress01, safeConfig.lookAheadMeters);
  const eyeOffset = { x: 0, y: safeConfig.heightMeters, z: 0 };
  const lookAtHeightOffset = {
    x: 0,
    y: safeConfig.heightMeters * 0.75,
    z: 0
  };
  const position = addVec3(sample.position, eyeOffset);
  const lookAtBase =
    sample.lookAheadPosition.x === sample.position.x &&
    sample.lookAheadPosition.z === sample.position.z
      ? addVec3(sample.position, scaleVec3(sample.direction, 1))
      : sample.lookAheadPosition;

  return {
    position,
    lookAt: addVec3(lookAtBase, lookAtHeightOffset),
    fovDegrees: safeConfig.fovDegrees,
    progress01: sample.progress01,
    usedFallback: sample.usedFallback,
    fallbackReason: sample.fallbackReason
  };
}

function sanitizeCameraConfig(
  config: Partial<CameraRailConfig> | null | undefined
): CameraRailConfig {
  const candidate = config ?? {};

  return {
    heightMeters: clampFinite(
      candidate.heightMeters ?? DEFAULT_CAMERA_RAIL_CONFIG.heightMeters,
      0.4,
      3,
      DEFAULT_CAMERA_RAIL_CONFIG.heightMeters
    ),
    lookAheadMeters: clampFinite(
      candidate.lookAheadMeters ?? DEFAULT_CAMERA_RAIL_CONFIG.lookAheadMeters,
      1,
      80,
      DEFAULT_CAMERA_RAIL_CONFIG.lookAheadMeters
    ),
    fovDegrees: clampFinite(
      candidate.fovDegrees ?? DEFAULT_CAMERA_RAIL_CONFIG.fovDegrees,
      45,
      95,
      DEFAULT_CAMERA_RAIL_CONFIG.fovDegrees
    )
  };
}

function extractProgress01(progress: RouteProgressInput): number {
  if (typeof progress === "number") {
    return progress;
  }

  if (isRideFrameSnapshot(progress)) {
    return progress.progress.progress01;
  }

  return progress.progress01;
}

function isRideFrameSnapshot(
  progress: RideProgress | RideFrameSnapshot
): progress is RideFrameSnapshot {
  return "progress" in progress;
}

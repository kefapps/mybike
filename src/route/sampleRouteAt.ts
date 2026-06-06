import {
  clampFinite,
  clampProgress01,
  lerpVec3,
  normalizeVec3,
  subtractVec3
} from "./routeMath";
import { resolveRouteDefinition } from "./routeValidation";
import type {
  RouteDefinition,
  RoutePoint,
  RouteSample,
  Vec3
} from "./routeTypes";

export const DEFAULT_ROUTE_SAMPLE_LOOK_AHEAD_METERS = 12;

export function sampleRouteAt(
  routeInput: RouteDefinition | null | undefined,
  progress01Input: number,
  lookAheadMetersInput = DEFAULT_ROUTE_SAMPLE_LOOK_AHEAD_METERS
): RouteSample {
  const resolution = resolveRouteDefinition(routeInput);
  const { route } = resolution;
  const progress01 = clampProgress01(progress01Input);
  const distanceMeters = progress01 * route.lengthMeters;
  const segment = sampleSegmentAtDistance(route.points, distanceMeters);
  const lookAheadMeters = clampFinite(
    lookAheadMetersInput,
    0,
    route.lengthMeters,
    DEFAULT_ROUTE_SAMPLE_LOOK_AHEAD_METERS
  );
  const lookAheadDistanceMeters = Math.min(
    route.lengthMeters,
    distanceMeters + lookAheadMeters
  );
  const lookAheadSegment = sampleSegmentAtDistance(
    route.points,
    lookAheadDistanceMeters
  );

  return {
    routeId: route.id,
    progress01,
    distanceMeters,
    position: segment.position,
    direction: segment.direction,
    lookAheadPosition: lookAheadSegment.position,
    segmentIndex: segment.segmentIndex,
    usedFallback: resolution.usedFallback,
    fallbackReason: resolution.reason
  };
}

function sampleSegmentAtDistance(
  points: RoutePoint[],
  distanceMeters: number
): {
  position: Vec3;
  direction: Vec3;
  segmentIndex: number;
} {
  const segmentIndex = findSegmentIndex(points, distanceMeters);
  const start = points[segmentIndex];
  const end = points[segmentIndex + 1];
  const spanMeters = end.distanceMeters - start.distanceMeters;
  const segmentProgress =
    spanMeters > 0 ? (distanceMeters - start.distanceMeters) / spanMeters : 0;
  const position = lerpVec3(start.position, end.position, segmentProgress);
  const direction = normalizeVec3(subtractVec3(end.position, start.position), {
    x: 0,
    y: 0,
    z: 1
  });

  return {
    position,
    direction,
    segmentIndex
  };
}

function findSegmentIndex(points: RoutePoint[], distanceMeters: number): number {
  const lastSegmentIndex = points.length - 2;

  if (distanceMeters >= points[points.length - 1].distanceMeters) {
    return lastSegmentIndex;
  }

  for (let index = 0; index < points.length - 1; index += 1) {
    if (distanceMeters <= points[index + 1].distanceMeters) {
      return index;
    }
  }

  return lastSegmentIndex;
}

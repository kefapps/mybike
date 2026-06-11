import { clampProgress01, lerp } from "./routeMath";
import type {
  DifficultySegmentType,
  RouteDifficultySegment,
  RouteDifficultySnapshot,
  RouteElevationPoint,
  RouteProfile
} from "./routeTypes";

export const DEFAULT_GRADE_WINDOW_METERS = 20;

const FALLBACK_DIFFICULTY: RouteDifficultySnapshot = {
  elevationMeters: 0,
  grade: 0,
  segmentType: "flat"
};

export function sampleRouteDifficulty(
  profile: RouteProfile | null | undefined,
  progress01: number,
  gradeWindowMeters = DEFAULT_GRADE_WINDOW_METERS
): RouteDifficultySnapshot {
  if (!profile || profile.elevationPoints.length === 0) {
    return { ...FALLBACK_DIFFICULTY };
  }

  const clampedProgress = clampProgress01(progress01);
  const totalElevationDistance =
    profile.elevationPoints[profile.elevationPoints.length - 1].distanceMeters;
  const distance = clampedProgress * totalElevationDistance;
  const elevation = interpolateElevation(profile.elevationPoints, distance);
  const grade = computeSmoothedGrade(
    profile.elevationPoints,
    distance,
    gradeWindowMeters
  );
  const segmentType = lookupDifficultySegment(
    profile.difficultySegments,
    clampedProgress
  );

  return {
    elevationMeters: elevation,
    grade,
    segmentType
  };
}

function interpolateElevation(
  points: RouteElevationPoint[],
  distanceMeters: number
): number {
  if (points.length === 0) {
    return 0;
  }

  if (distanceMeters <= points[0].distanceMeters) {
    return points[0].elevationMeters;
  }

  const last = points[points.length - 1];
  if (distanceMeters >= last.distanceMeters) {
    return last.elevationMeters;
  }

  for (let index = 0; index < points.length - 1; index += 1) {
    const start = points[index];
    const end = points[index + 1];

    if (distanceMeters <= end.distanceMeters) {
      const span = end.distanceMeters - start.distanceMeters;

      if (span <= 0) {
        return start.elevationMeters;
      }

      const t = (distanceMeters - start.distanceMeters) / span;

      return lerp(start.elevationMeters, end.elevationMeters, t);
    }
  }

  return last.elevationMeters;
}

function computeSmoothedGrade(
  points: RouteElevationPoint[],
  distanceMeters: number,
  windowMeters: number
): number {
  const totalDistance =
    points[points.length - 1].distanceMeters;
  const safeWindow = Math.max(1, Math.min(windowMeters, totalDistance * 0.5));
  const halfWindow = safeWindow / 2;

  const beforeDistance = Math.max(0, distanceMeters - halfWindow);
  const afterDistance = Math.min(totalDistance, distanceMeters + halfWindow);

  const elevationBefore = interpolateElevation(points, beforeDistance);
  const elevationAfter = interpolateElevation(points, afterDistance);
  const distanceSpan = afterDistance - beforeDistance;

  if (distanceSpan <= 0) {
    return 0;
  }

  return (elevationAfter - elevationBefore) / distanceSpan;
}

function lookupDifficultySegment(
  segments: RouteDifficultySegment[],
  progress01: number
): DifficultySegmentType {
  if (segments.length === 0) {
    return "flat";
  }

  for (const segment of segments) {
    const isEndpoint =
      progress01 === segment.toProgress01 &&
      segment.toProgress01 === 1;

    if (
      progress01 >= segment.fromProgress01 &&
      (progress01 < segment.toProgress01 || isEndpoint)
    ) {
      return segment.segmentType;
    }
  }

  if (progress01 < segments[0].fromProgress01) {
    return segments[0].segmentType;
  }

  return segments[segments.length - 1].segmentType;
}

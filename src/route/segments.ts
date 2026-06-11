import { clampProgress01, isFiniteNumber } from "./routeMath";
import { resolveRouteDefinition } from "./routeValidation";
import type { RouteDefinition } from "./routeTypes";

export const SEGMENT_KINDS = [
  "warmup",
  "climb",
  "sprint",
  "recovery",
  "cooldown"
] as const;

export type SegmentKind = (typeof SEGMENT_KINDS)[number];

export type SegmentDefinition = {
  id: string;
  kind: SegmentKind;
  fromProgress01: number;
  toProgress01: number;
  targetEffort01: number;
};

export type SegmentSelection = {
  current: SegmentDefinition | null;
  next: SegmentDefinition | null;
  index: number;
  count: number;
  progress01: number;
  remainingProgress01: number;
  remainingDistanceMeters: number;
  targetEffort01: number | null;
};

export const EMPTY_SEGMENT_SELECTION: SegmentSelection = Object.freeze({
  current: null,
  next: null,
  index: -1,
  count: 0,
  progress01: 0,
  remainingProgress01: 0,
  remainingDistanceMeters: 0,
  targetEffort01: null
});

export const FREE_RIDE_SEGMENT_ID = "free-ride";

export function isSegmentKind(value: unknown): value is SegmentKind {
  return (
    typeof value === "string" &&
    (SEGMENT_KINDS as readonly string[]).includes(value)
  );
}

export function isSegmentDefinition(value: unknown): value is SegmentDefinition {
  if (typeof value !== "object" || value === null) {
    return false;
  }

  const candidate = value as Partial<SegmentDefinition>;

  return (
    typeof candidate.id === "string" &&
    candidate.id.length > 0 &&
    isSegmentKind(candidate.kind) &&
    isFiniteNumber(candidate.fromProgress01) &&
    isFiniteNumber(candidate.toProgress01) &&
    isFiniteNumber(candidate.targetEffort01) &&
    candidate.fromProgress01 >= 0 &&
    candidate.toProgress01 <= 1 &&
    candidate.fromProgress01 < candidate.toProgress01 &&
    candidate.targetEffort01 >= 0 &&
    candidate.targetEffort01 <= 1
  );
}

export function getRouteSegments(
  routeInput: RouteDefinition | null | undefined
): SegmentDefinition[] {
  const { route } = resolveRouteDefinition(routeInput);

  if (!Array.isArray(route.segments)) {
    return [];
  }

  return route.segments.filter(isSegmentDefinition);
}

export function selectSegmentAtProgress(
  routeInput: RouteDefinition | null | undefined,
  progress01Input: number
): SegmentSelection {
  const segments = getRouteSegments(routeInput);

  if (segments.length === 0) {
    return { ...EMPTY_SEGMENT_SELECTION };
  }

  const progress01 = clampProgress01(progress01Input);
  const index = segments.findIndex(
    (segment) =>
      progress01 >= segment.fromProgress01 &&
      (progress01 < segment.toProgress01 ||
        (progress01 === 1 && segment.toProgress01 === 1))
  );
  const current = index >= 0 ? segments[index] : null;
  const next = index >= 0 ? segments[index + 1] ?? null : segments[0] ?? null;
  const segmentProgress01 = current
    ? Math.min(
        1,
        Math.max(
          0,
          (progress01 - current.fromProgress01) /
            Math.max(0.000001, current.toProgress01 - current.fromProgress01)
        )
      )
    : 0;
  const remainingProgress01 = current
    ? Math.max(0, current.toProgress01 - progress01)
    : 0;
  const { route } = resolveRouteDefinition(routeInput);
  const remainingDistanceMeters = remainingProgress01 * route.lengthMeters;

  return {
    current,
    next,
    index,
    count: segments.length,
    progress01: segmentProgress01,
    remainingProgress01,
    remainingDistanceMeters,
    targetEffort01: current ? current.targetEffort01 : null
  };
}

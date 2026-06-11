import { isFiniteNumber, isFiniteVec3 } from "./routeMath";
import { isSegmentDefinition } from "./segments";
import type {
  BiomeSegment,
  RouteDefinition,
  RouteFallbackReason,
  RouteResolution
} from "./routeTypes";

const EPSILON = 0.000001;

export const placeholderRouteDefinition: RouteDefinition = {
  id: "placeholder-route",
  lengthMeters: 100,
  points: [
    { distanceMeters: 0, position: { x: 0, y: 0, z: 0 } },
    { distanceMeters: 100, position: { x: 0, y: 0, z: 100 } }
  ],
  biomes: [{ id: "placeholder", fromProgress01: 0, toProgress01: 1 }]
};

export function resolveRouteDefinition(
  route: RouteDefinition | null | undefined
): RouteResolution {
  const reason = getInvalidRouteReason(route);

  if (reason === undefined && route !== null && route !== undefined) {
    return {
      route,
      usedFallback: false
    };
  }

  return {
    route: placeholderRouteDefinition,
    usedFallback: true,
    reason
  };
}

function getInvalidRouteReason(
  route: RouteDefinition | null | undefined
): RouteFallbackReason | undefined {
  if (route === null || route === undefined) {
    return "missing-route";
  }

  if (!isFiniteNumber(route.lengthMeters) || route.lengthMeters <= 0) {
    return "invalid-length";
  }

  if (!Array.isArray(route.points) || route.points.length < 2) {
    return "invalid-points";
  }

  if (!hasValidPoints(route)) {
    return "invalid-points";
  }

  if (!hasFullBiomeCoverage(route.biomes)) {
    return "invalid-biomes";
  }

  if (route.segments !== undefined && !hasValidSegments(route.segments)) {
    return "invalid-segments";
  }

  return undefined;
}

function hasValidPoints(route: RouteDefinition): boolean {
  let previousDistanceMeters = -EPSILON;

  for (const point of route.points) {
    if (!isValidRoutePoint(point, route.lengthMeters, previousDistanceMeters)) {
      return false;
    }

    previousDistanceMeters = point.distanceMeters;
  }

  const first = route.points[0];
  const last = route.points[route.points.length - 1];

  return (
    Math.abs(first.distanceMeters) <= EPSILON &&
    Math.abs(last.distanceMeters - route.lengthMeters) <= EPSILON
  );
}

function hasFullBiomeCoverage(biomes: BiomeSegment[]): boolean {
  if (!Array.isArray(biomes) || biomes.length === 0) {
    return false;
  }

  if (!biomes.every(isValidBiomeSegment)) {
    return false;
  }

  const sorted = [...biomes].sort(
    (left, right) => left.fromProgress01 - right.fromProgress01
  );

  if (sorted.length === 0 || sorted[0].fromProgress01 > EPSILON) {
    return false;
  }

  let coveredUntil = 0;

  for (const biome of sorted) {
    if (biome.fromProgress01 > coveredUntil + EPSILON) {
      return false;
    }

    coveredUntil = Math.max(coveredUntil, biome.toProgress01);
  }

  return coveredUntil >= 1 - EPSILON;
}

function isValidRoutePoint(
  point: unknown,
  routeLengthMeters: number,
  previousDistanceMeters: number
): point is RouteDefinition["points"][number] {
  if (typeof point !== "object" || point === null) {
    return false;
  }

  const candidate = point as Partial<RouteDefinition["points"][number]>;

  return (
    isFiniteNumber(candidate.distanceMeters) &&
    candidate.distanceMeters >= 0 &&
    candidate.distanceMeters <= routeLengthMeters &&
    candidate.distanceMeters > previousDistanceMeters &&
    isFiniteVec3(candidate.position)
  );
}

function isValidBiomeSegment(biome: unknown): biome is BiomeSegment {
  if (typeof biome !== "object" || biome === null) {
    return false;
  }

  const candidate = biome as Partial<BiomeSegment>;

  return (
    typeof candidate.id === "string" &&
    candidate.id.length > 0 &&
    isFiniteNumber(candidate.fromProgress01) &&
    isFiniteNumber(candidate.toProgress01) &&
    candidate.fromProgress01 >= 0 &&
    candidate.toProgress01 <= 1 &&
    candidate.fromProgress01 < candidate.toProgress01
  );
}

function hasValidSegments(
  segments: NonNullable<RouteDefinition["segments"]>
): boolean {
  if (!Array.isArray(segments)) {
    return false;
  }

  if (segments.length === 0) {
    return true;
  }

  if (!segments.every(isSegmentDefinition)) {
    return false;
  }

  const sorted = [...segments].sort(
    (left, right) => left.fromProgress01 - right.fromProgress01
  );

  if (sorted[0].fromProgress01 > EPSILON) {
    return false;
  }

  let coveredUntil = 0;

  for (const segment of sorted) {
    if (segment.fromProgress01 > coveredUntil + EPSILON) {
      return false;
    }

    coveredUntil = Math.max(coveredUntil, segment.toProgress01);
  }

  return coveredUntil >= 1 - EPSILON;
}

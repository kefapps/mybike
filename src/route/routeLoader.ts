import { resolveRouteDefinition } from "./routeValidation";
import {
  ROUTE_JSON_SCHEMA_VERSION,
  type RouteJson,
  type RouteJsonValidationIssue,
  type RouteJsonValidationResult,
  type RouteProfile
} from "./routeSchema";
import type { BiomeSegment, RouteDefinition } from "./routeTypes";

const ROUTE_PROFILES: ReadonlySet<RouteProfile> = new Set<RouteProfile>([
  "flat",
  "climb",
  "zen"
]);

const ROUTE_COLLECTIBLE_KINDS = new Set(["marker"]);

const EPSILON = 0.000001;

export function validateRouteJson(value: unknown): RouteJsonValidationResult {
  const issues: RouteJsonValidationIssue[] = [];
  const json = validateRouteJsonShape(value, issues);

  if (json === null) {
    return { ok: false, issues };
  }

  validateRouteJsonPoints(json, issues);
  validateRouteJsonSegments(json, issues);
  validateRouteJsonCollectibles(json, issues);

  if (issues.length > 0) {
    return { ok: false, issues };
  }

  return { ok: true };
}

export function routeJsonToDefinition(json: RouteJson): RouteDefinition {
  return {
    id: json.id,
    lengthMeters: json.lengthMeters,
    points: json.points.map((point) => ({
      distanceMeters: point.distanceMeters,
      position: {
        x: point.position.x,
        y: point.position.y,
        z: point.position.z
      }
    })),
    biomes: json.segments.map((segment) => ({
      id: segment.id,
      fromProgress01: segment.fromProgress01,
      toProgress01: segment.toProgress01
    }))
  };
}

export function loadRouteFromJson(value: unknown): {
  ok: boolean;
  definition: RouteDefinition;
  issues: RouteJsonValidationIssue[];
} {
  const validation = validateRouteJson(value);

  if (!validation.ok) {
    return {
      ok: false,
      definition: resolveRouteDefinition(null).route,
      issues: validation.issues
    };
  }

  return {
    ok: true,
    definition: routeJsonToDefinition(value as RouteJson),
    issues: []
  };
}

function validateRouteJsonShape(
  value: unknown,
  issues: RouteJsonValidationIssue[]
): RouteJson | null {
  if (typeof value !== "object" || value === null) {
    issues.push({ path: "$", message: "Route JSON must be an object." });
    return null;
  }

  const candidate = value as Partial<RouteJson> & Record<string, unknown>;

  if (candidate.schemaVersion !== ROUTE_JSON_SCHEMA_VERSION) {
    issues.push({
      path: "$.schemaVersion",
      message: `Route schemaVersion must be ${ROUTE_JSON_SCHEMA_VERSION}.`
    });
  }

  if (typeof candidate.id !== "string" || candidate.id.length === 0) {
    issues.push({ path: "$.id", message: "Route id must be a non-empty string." });
  }

  if (typeof candidate.displayName !== "string" || candidate.displayName.length === 0) {
    issues.push({
      path: "$.displayName",
      message: "Route displayName must be a non-empty string."
    });
  }

  if (typeof candidate.description !== "string") {
    issues.push({
      path: "$.description",
      message: "Route description must be a string."
    });
  }

  if (
    typeof candidate.profile !== "string" ||
    !ROUTE_PROFILES.has(candidate.profile as RouteProfile)
  ) {
    issues.push({
      path: "$.profile",
      message: "Route profile must be one of: flat, climb, zen."
    });
  }

  if (typeof candidate.biome !== "string" || candidate.biome.length === 0) {
    issues.push({
      path: "$.biome",
      message: "Route biome must be a non-empty string."
    });
  }

  if (!isFiniteNumber(candidate.lengthMeters) || candidate.lengthMeters <= 0) {
    issues.push({
      path: "$.lengthMeters",
      message: "Route lengthMeters must be a positive finite number."
    });
  }

  if (issues.length > 0) {
    return null;
  }

  return {
    schemaVersion: ROUTE_JSON_SCHEMA_VERSION,
    id: candidate.id as string,
    displayName: candidate.displayName as string,
    description: candidate.description as string,
    profile: candidate.profile as RouteProfile,
    biome: candidate.biome as string,
    lengthMeters: candidate.lengthMeters as number,
    points: candidate.points as RouteJson["points"],
    segments: candidate.segments as RouteJson["segments"],
    collectibles: candidate.collectibles as RouteJson["collectibles"]
  };
}

function validateRouteJsonPoints(
  json: RouteJson,
  issues: RouteJsonValidationIssue[]
): void {
  if (!Array.isArray(json.points) || json.points.length < 2) {
    issues.push({
      path: "$.points",
      message: "Route must declare at least two points."
    });
    return;
  }

  let previousDistanceMeters = -EPSILON;

  for (let index = 0; index < json.points.length; index += 1) {
    const point = json.points[index];
    const basePath = `$.points[${index}]`;

    if (typeof point !== "object" || point === null) {
      issues.push({
        path: basePath,
        message: "Route point must be an object."
      });
      continue;
    }

    if (!isFiniteNumber(point.distanceMeters)) {
      issues.push({
        path: `${basePath}.distanceMeters`,
        message: "Route point distanceMeters must be a finite number."
      });
    } else {
      if (point.distanceMeters < 0) {
        issues.push({
          path: `${basePath}.distanceMeters`,
          message: "Route point distanceMeters must be non-negative."
        });
      }

      if (point.distanceMeters > json.lengthMeters) {
        issues.push({
          path: `${basePath}.distanceMeters`,
          message: "Route point distanceMeters cannot exceed lengthMeters."
        });
      }

      if (point.distanceMeters <= previousDistanceMeters) {
        issues.push({
          path: `${basePath}.distanceMeters`,
          message: "Route point distanceMeters must be strictly increasing."
        });
      }
    }

    if (!isFiniteVec3(point.position)) {
      issues.push({
        path: `${basePath}.position`,
        message: "Route point position must be a finite Vec3."
      });
    }

    previousDistanceMeters = point.distanceMeters;
  }

  if (issues.some((issue) => issue.path.startsWith("$.points"))) {
    return;
  }

  const first = json.points[0];
  const last = json.points[json.points.length - 1];

  if (Math.abs(first.distanceMeters) > EPSILON) {
    issues.push({
      path: "$.points[0].distanceMeters",
      message: "First route point must be at distanceMeters 0."
    });
  }

  if (Math.abs(last.distanceMeters - json.lengthMeters) > EPSILON) {
    issues.push({
      path: `$.points[${json.points.length - 1}].distanceMeters`,
      message: "Last route point must be at lengthMeters."
    });
  }
}

function validateRouteJsonSegments(
  json: RouteJson,
  issues: RouteJsonValidationIssue[]
): void {
  if (!Array.isArray(json.segments) || json.segments.length === 0) {
    issues.push({
      path: "$.segments",
      message: "Route must declare at least one biome segment."
    });
    return;
  }

  const biomeSegments: BiomeSegment[] = [];

  for (let index = 0; index < json.segments.length; index += 1) {
    const segment = json.segments[index];
    const basePath = `$.segments[${index}]`;

    if (typeof segment !== "object" || segment === null) {
      issues.push({
        path: basePath,
        message: "Route segment must be an object."
      });
      continue;
    }

    if (typeof segment.id !== "string" || segment.id.length === 0) {
      issues.push({
        path: `${basePath}.id`,
        message: "Route segment id must be a non-empty string."
      });
    }

    if (!isFiniteNumber(segment.fromProgress01) || !isFiniteNumber(segment.toProgress01)) {
      issues.push({
        path: basePath,
        message: "Route segment progress bounds must be finite."
      });
      continue;
    }

    if (segment.fromProgress01 < 0 || segment.toProgress01 > 1) {
      issues.push({
        path: basePath,
        message: "Route segment progress bounds must stay in [0, 1]."
      });
    }

    if (segment.fromProgress01 >= segment.toProgress01) {
      issues.push({
        path: basePath,
        message: "Route segment fromProgress01 must be < toProgress01."
      });
    }

    biomeSegments.push({
      id: segment.id,
      fromProgress01: segment.fromProgress01,
      toProgress01: segment.toProgress01
    });
  }

  if (biomeSegments.length !== json.segments.length) {
    return;
  }

  const sorted = [...biomeSegments].sort(
    (left, right) => left.fromProgress01 - right.fromProgress01
  );

  if (sorted[0].fromProgress01 > EPSILON) {
    issues.push({
      path: "$.segments[0]",
      message: "Route segments must cover the full [0, 1] progress range."
    });
  }

  let coveredUntil = 0;

  for (const biome of sorted) {
    if (biome.fromProgress01 > coveredUntil + EPSILON) {
      issues.push({
        path: "$.segments",
        message: "Route segments must cover the full [0, 1] progress range."
      });
      return;
    }
    coveredUntil = Math.max(coveredUntil, biome.toProgress01);
  }

  if (coveredUntil < 1 - EPSILON) {
    issues.push({
      path: "$.segments",
      message: "Route segments must cover the full [0, 1] progress range."
    });
  }
}

function validateRouteJsonCollectibles(
  json: RouteJson,
  issues: RouteJsonValidationIssue[]
): void {
  if (json.collectibles === undefined) {
    return;
  }

  if (!Array.isArray(json.collectibles)) {
    issues.push({
      path: "$.collectibles",
      message: "Route collectibles must be an array when present."
    });
    return;
  }

  for (let index = 0; index < json.collectibles.length; index += 1) {
    const collectible = json.collectibles[index];
    const basePath = `$.collectibles[${index}]`;

    if (typeof collectible !== "object" || collectible === null) {
      issues.push({
        path: basePath,
        message: "Route collectible must be an object."
      });
      continue;
    }

    if (typeof collectible.id !== "string" || collectible.id.length === 0) {
      issues.push({
        path: `${basePath}.id`,
        message: "Route collectible id must be a non-empty string."
      });
    }

    if (
      typeof collectible.kind !== "string" ||
      !ROUTE_COLLECTIBLE_KINDS.has(collectible.kind)
    ) {
      issues.push({
        path: `${basePath}.kind`,
        message: "Route collectible kind must be a known kind."
      });
    }

    if (
      !isFiniteNumber(collectible.distanceMeters) ||
      collectible.distanceMeters < 0 ||
      collectible.distanceMeters > json.lengthMeters
    ) {
      issues.push({
        path: `${basePath}.distanceMeters`,
        message: "Route collectible distanceMeters must be within route length."
      });
    }

    if (!isFiniteVec3(collectible.position)) {
      issues.push({
        path: `${basePath}.position`,
        message: "Route collectible position must be a finite Vec3."
      });
    }
  }
}

function isFiniteNumber(value: unknown): value is number {
  return typeof value === "number" && Number.isFinite(value);
}

function isFiniteVec3(value: unknown): boolean {
  if (typeof value !== "object" || value === null) {
    return false;
  }

  const candidate = value as { x?: unknown; y?: unknown; z?: unknown };

  return (
    isFiniteNumber(candidate.x) &&
    isFiniteNumber(candidate.y) &&
    isFiniteNumber(candidate.z)
  );
}

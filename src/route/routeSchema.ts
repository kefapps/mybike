export const ROUTE_JSON_SCHEMA_VERSION = 1 as const;

export type RouteProfile = "flat" | "climb" | "zen";

export type RouteCollectibleKind = "marker";

export type RouteCollectible = {
  id: string;
  kind: RouteCollectibleKind;
  distanceMeters: number;
  position: { x: number; y: number; z: number };
};

export type RouteSegment = {
  id: string;
  fromProgress01: number;
  toProgress01: number;
};

export type RoutePoint = {
  distanceMeters: number;
  position: { x: number; y: number; z: number };
};

export type RouteJson = {
  schemaVersion: typeof ROUTE_JSON_SCHEMA_VERSION;
  id: string;
  displayName: string;
  description: string;
  profile: RouteProfile;
  biome: string;
  lengthMeters: number;
  points: RoutePoint[];
  segments: RouteSegment[];
  collectibles?: RouteCollectible[];
};

export type RouteJsonValidationIssue = {
  path: string;
  message: string;
};

export type RouteJsonValidationResult =
  | { ok: true }
  | { ok: false; issues: RouteJsonValidationIssue[] };

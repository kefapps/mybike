import {
  loadRouteFromJson,
  routeJsonToDefinition,
  validateRouteJson
} from "./routeLoader";
import type { RouteProfile } from "./routeSchema";
import type { RouteDefinition } from "./routeTypes";

import shortRouteJson from "./routes/short.json";
import climbRouteJson from "./routes/climb.json";
import zenRouteJson from "./routes/zen.json";

export const DEFAULT_ROUTE_ID = "echappee-mock-route";

export type RouteCatalogEntry = {
  id: string;
  displayName: string;
  description: string;
  profile: RouteProfile;
  biome: string;
  definition: RouteDefinition;
  collectibles: ReadonlyArray<{
    id: string;
    kind: string;
    distanceMeters: number;
    position: { x: number; y: number; z: number };
  }>;
};

type RawRouteJson = {
  schemaVersion: number;
  id: string;
  displayName: string;
  description: string;
  profile: string;
  biome: string;
  lengthMeters: number;
  points: ReadonlyArray<{
    distanceMeters: number;
    position: { x: number; y: number; z: number };
  }>;
  segments: ReadonlyArray<{
    id: string;
    fromProgress01: number;
    toProgress01: number;
  }>;
  collectibles?: ReadonlyArray<{
    id: string;
    kind: string;
    distanceMeters: number;
    position: { x: number; y: number; z: number };
  }>;
};

const RAW_ROUTES: ReadonlyArray<RawRouteJson> = [
  shortRouteJson as RawRouteJson,
  climbRouteJson as RawRouteJson,
  zenRouteJson as RawRouteJson
];

function buildRouteCatalog(): ReadonlyArray<RouteCatalogEntry> {
  const entries: RouteCatalogEntry[] = [];

  for (const raw of RAW_ROUTES) {
    const validation = validateRouteJson(raw);
    if (!validation.ok) {
      const detail = validation.issues
        .map((issue) => `${issue.path}: ${issue.message}`)
        .join("; ");

      console.error(
        `[routes] Invalid route JSON for "${raw?.id ?? "unknown"}": ${detail}`
      );
      throw new Error(
        `[routes] Invalid route JSON for "${raw?.id ?? "unknown"}": ${detail}`
      );
    }

    const result = loadRouteFromJson(raw);

    if (!result.ok) {
      const detail = result.issues
        .map((issue) => `${issue.path}: ${issue.message}`)
        .join("; ");
      console.error(`[routes] Failed to load route "${raw.id}": ${detail}`);
      throw new Error(`[routes] Failed to load route "${raw.id}": ${detail}`);
    }

    entries.push({
      id: raw.id,
      displayName: raw.displayName,
      description: raw.description,
      profile: raw.profile as RouteProfile,
      biome: raw.biome,
      definition: routeJsonToDefinition(
        raw as unknown as Parameters<typeof routeJsonToDefinition>[0]
      ),
      collectibles: (raw.collectibles ?? []).map((collectible) => ({
        id: collectible.id,
        kind: collectible.kind,
        distanceMeters: collectible.distanceMeters,
        position: {
          x: collectible.position.x,
          y: collectible.position.y,
          z: collectible.position.z
        }
      }))
    });
  }

  return entries;
}

export const availableRoutes: ReadonlyArray<RouteCatalogEntry> = buildRouteCatalog();

const routeIndex: ReadonlyMap<string, RouteCatalogEntry> = new Map(
  availableRoutes.map((entry) => [entry.id, entry])
);

export function getRouteEntryById(id: string): RouteCatalogEntry | undefined {
  return routeIndex.get(id);
}

export function getDefaultRouteEntry(): RouteCatalogEntry {
  const entry = routeIndex.get(DEFAULT_ROUTE_ID) ?? availableRoutes[0];
  if (!entry) {
    throw new Error(
      "[routes] No routes registered. Add at least one JSON file in src/route/routes/."
    );
  }
  return entry;
}

export function getRouteDefinitionById(id: string): RouteDefinition | undefined {
  return routeIndex.get(id)?.definition;
}

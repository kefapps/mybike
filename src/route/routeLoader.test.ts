import { describe, expect, it } from "vitest";

import { loadRouteFromJson, routeJsonToDefinition, validateRouteJson } from "./routeLoader";
import { ROUTE_JSON_SCHEMA_VERSION } from "./routeSchema";
import type { RouteJson } from "./routeSchema";
import { mockRouteDefinition } from "./mockRouteDefinition";

import shortRouteJson from "./routes/short.json";
import climbRouteJson from "./routes/climb.json";
import zenRouteJson from "./routes/zen.json";

function buildValidRoute(overrides: Partial<RouteJson> = {}): RouteJson {
  return {
    schemaVersion: ROUTE_JSON_SCHEMA_VERSION,
    id: "test-route",
    displayName: "Test route",
    description: "Synthetic route for tests.",
    profile: "flat",
    biome: "coast-forest",
    lengthMeters: 100,
    points: [
      { distanceMeters: 0, position: { x: 0, y: 0, z: 0 } },
      { distanceMeters: 100, position: { x: 0, y: 0, z: 100 } }
    ],
    segments: [{ id: "coast", fromProgress01: 0, toProgress01: 1 }],
    collectibles: [],
    ...overrides
  };
}

describe("validateRouteJson", () => {
  it("accepts the bundled route JSON files", () => {
    expect(validateRouteJson(shortRouteJson).ok).toBe(true);
    expect(validateRouteJson(climbRouteJson).ok).toBe(true);
    expect(validateRouteJson(zenRouteJson).ok).toBe(true);
  });

  it("rejects non-object values with a typed issue", () => {
    const result = validateRouteJson("not a route");

    expect(result.ok).toBe(false);
    if (!result.ok) {
      expect(result.issues[0]?.path).toBe("$");
    }
  });

  it("rejects routes with an unsupported schemaVersion", () => {
    const result = validateRouteJson(
      buildValidRoute({ schemaVersion: 99 as unknown as 1 })
    );

    expect(result.ok).toBe(false);
    if (!result.ok) {
      expect(result.issues).toEqual(
        expect.arrayContaining([
          expect.objectContaining({ path: "$.schemaVersion" })
        ])
      );
    }
  });

  it("rejects routes with non-increasing point distances", () => {
    const result = validateRouteJson(
      buildValidRoute({
        points: [
          { distanceMeters: 0, position: { x: 0, y: 0, z: 0 } },
          { distanceMeters: 60, position: { x: 0, y: 0, z: 60 } },
          { distanceMeters: 60, position: { x: 0, y: 0, z: 60 } }
        ]
      })
    );

    expect(result.ok).toBe(false);
    if (!result.ok) {
      expect(result.issues).toEqual(
        expect.arrayContaining([
          expect.objectContaining({ path: "$.points[2].distanceMeters" })
        ])
      );
    }
  });

  it("rejects routes whose endpoints do not match lengthMeters", () => {
    const result = validateRouteJson(
      buildValidRoute({
        points: [
          { distanceMeters: 5, position: { x: 0, y: 0, z: 0 } },
          { distanceMeters: 90, position: { x: 0, y: 0, z: 90 } }
        ]
      })
    );

    expect(result.ok).toBe(false);
    if (!result.ok) {
      expect(result.issues).toEqual(
        expect.arrayContaining([
          expect.objectContaining({ path: "$.points[0].distanceMeters" }),
          expect.objectContaining({ path: "$.points[1].distanceMeters" })
        ])
      );
    }
  });

  it("rejects routes whose biome segments do not cover the full progress range", () => {
    const result = validateRouteJson(
      buildValidRoute({
        segments: [{ id: "coast", fromProgress01: 0, toProgress01: 0.5 }]
      })
    );

    expect(result.ok).toBe(false);
    if (!result.ok) {
      expect(result.issues).toEqual(
        expect.arrayContaining([
          expect.objectContaining({ path: "$.segments" })
        ])
      );
    }
  });

  it("rejects unknown route profiles", () => {
    const result = validateRouteJson(
      buildValidRoute({ profile: "rocket" as unknown as "flat" })
    );

    expect(result.ok).toBe(false);
    if (!result.ok) {
      expect(result.issues).toEqual(
        expect.arrayContaining([
          expect.objectContaining({ path: "$.profile" })
        ])
      );
    }
  });

  it("rejects collectibles outside the route length", () => {
    const result = validateRouteJson(
      buildValidRoute({
        collectibles: [
          {
            id: "c1",
            kind: "marker",
            distanceMeters: 200,
            position: { x: 0, y: 0, z: 0 }
          }
        ]
      })
    );

    expect(result.ok).toBe(false);
    if (!result.ok) {
      expect(result.issues).toEqual(
        expect.arrayContaining([
          expect.objectContaining({
            path: "$.collectibles[0].distanceMeters"
          })
        ])
      );
    }
  });
});

describe("loadRouteFromJson", () => {
  it("returns a valid RouteDefinition for a correct JSON", () => {
    const result = loadRouteFromJson(shortRouteJson);

    expect(result.ok).toBe(true);
    expect(result.definition).toMatchObject({
      id: "echappee-mock-route",
      lengthMeters: 1000
    });
    expect(result.definition.biomes).toEqual([
      { id: "coast", fromProgress01: 0, toProgress01: 0.01 },
      { id: "forest", fromProgress01: 0.01, toProgress01: 1 }
    ]);
  });

  it("returns a placeholder definition and issues for an invalid JSON", () => {
    const result = loadRouteFromJson({ schemaVersion: 99 });

    expect(result.ok).toBe(false);
    expect(result.definition.id).toBe("placeholder-route");
    expect(result.issues.length).toBeGreaterThan(0);
  });
});

describe("routeJsonToDefinition", () => {
  it("maps route JSON segments to the existing biomes shape", () => {
    const definition = routeJsonToDefinition(shortRouteJson as RouteJson);

    expect(definition.id).toBe(shortRouteJson.id);
    expect(definition.lengthMeters).toBe(shortRouteJson.lengthMeters);
    expect(definition.points).toEqual(shortRouteJson.points);
    expect(definition.biomes).toEqual(shortRouteJson.segments);
  });
});

describe("mockRouteDefinition migration", () => {
  it("still exposes the original mock route id and length after the JSON migration", () => {
    expect(mockRouteDefinition.id).toBe("echappee-mock-route");
    expect(mockRouteDefinition.lengthMeters).toBe(1000);
    expect(mockRouteDefinition.points.length).toBeGreaterThanOrEqual(4);
  });
});

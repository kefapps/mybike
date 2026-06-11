import { describe, expect, it } from "vitest";

import { generateSceneryInstances } from "./generateSceneryInstances";
import type { SceneryInstance } from "./sceneryTypes";

function makeTestRoute() {
  return {
    id: "test-route",
    lengthMeters: 200,
    points: [
      { distanceMeters: 0, position: { x: 0, y: 0, z: 0 } },
      { distanceMeters: 50, position: { x: 5, y: 1, z: 50 } },
      { distanceMeters: 100, position: { x: 10, y: 2, z: 100 } },
      { distanceMeters: 150, position: { x: 5, y: 1, z: 150 } },
      { distanceMeters: 200, position: { x: 0, y: 0, z: 200 } }
    ],
    biomes: [
      { id: "coast", fromProgress01: 0, toProgress01: 0.5 },
      { id: "forest", fromProgress01: 0.5, toProgress01: 1 }
    ]
  };
}

function lateralDistance(instance: SceneryInstance): number {
  const routeX = interpolateTestRouteX(instance.position.z);

  return Math.abs(instance.position.x - routeX);
}

function interpolateTestRouteX(worldZ: number): number {
  const route = makeTestRoute();
  const { points } = route;

  if (worldZ <= 0) return 0;
  if (worldZ >= 200) return 0;

  if (worldZ <= 50) return (worldZ / 50) * 5;
  if (worldZ <= 100) return 5 + ((worldZ - 50) / 50) * 5;
  if (worldZ <= 150) return 10 + ((worldZ - 100) / 50) * (-5);
  return 5 + ((worldZ - 150) / 50) * (-5);
}

function distinctBiomes(instances: SceneryInstance[]): Set<string> {
  return new Set(instances.map((i) => i.biomeId));
}

function extractPositions(instances: SceneryInstance[]): string[] {
  return instances.map(
    (i) =>
      `${i.kind}:${i.position.x.toFixed(1)},${i.position.z.toFixed(1)},${i.side}`
  );
}

describe("generateSceneryInstances", () => {
  it("produces deterministic output for the same route and seed", () => {
    const route = makeTestRoute();
    const first = generateSceneryInstances(route, "test-seed", "medium");
    const second = generateSceneryInstances(route, "test-seed", "medium");

    expect(first).toHaveLength(second.length);
    expect(extractPositions(first)).toEqual(extractPositions(second));
  });

  it("produces different output for different seeds", () => {
    const route = makeTestRoute();
    const first = generateSceneryInstances(route, "seed-a", "medium");
    const second = generateSceneryInstances(route, "seed-b", "medium");

    expect(extractPositions(first)).not.toEqual(extractPositions(second));
  });

  it("produces different output for different routes with same seed", () => {
    const routeA = makeTestRoute();
    const routeB = {
      ...makeTestRoute(),
      points: [
        { distanceMeters: 0, position: { x: 10, y: 0, z: 0 } },
        { distanceMeters: 50, position: { x: 20, y: 2, z: 50 } },
        { distanceMeters: 100, position: { x: 15, y: 3, z: 100 } },
        { distanceMeters: 150, position: { x: 10, y: 1, z: 150 } },
        { distanceMeters: 200, position: { x: 5, y: 0, z: 200 } }
      ]
    };

    const first = generateSceneryInstances(routeA, "same-seed", "medium");
    const second = generateSceneryInstances(routeB, "same-seed", "medium");

    expect(extractPositions(first)).not.toEqual(extractPositions(second));
  });

  it("produces fewer instances at low quality vs high quality", () => {
    const route = makeTestRoute();
    const low = generateSceneryInstances(route, "q-seed", "low");
    const high = generateSceneryInstances(route, "q-seed", "high");

    expect(low.length).toBeLessThan(high.length);
  });

  it("places all instances within lateral bounds", () => {
    const route = makeTestRoute();
    const instances = generateSceneryInstances(route, "bounds-seed", "high");

    for (const instance of instances) {
      const dist = lateralDistance(instance);

      expect(dist).toBeGreaterThanOrEqual(4.5);
      expect(dist).toBeLessThanOrEqual(125);
    }
  });

  it("includes both left and right side instances", () => {
    const route = makeTestRoute();
    const instances = generateSceneryInstances(route, "side-seed", "medium");
    const sides = new Set(instances.map((i) => i.side));

    expect(sides.has("left")).toBe(true);
    expect(sides.has("right")).toBe(true);
  });

  it("covers multiple biomes when route spans biome boundaries", () => {
    const route = makeTestRoute();
    const instances = generateSceneryInstances(route, "biome-seed", "medium");
    const biomes = distinctBiomes(instances);

    expect(biomes.size).toBeGreaterThanOrEqual(2);
  });

  it("produces valid positions for all instances", () => {
    const route = makeTestRoute();
    const instances = generateSceneryInstances(route, "valid-seed", "high");

    for (const instance of instances) {
      expect(Number.isFinite(instance.position.x)).toBe(true);
      expect(Number.isFinite(instance.position.y)).toBe(true);
      expect(Number.isFinite(instance.position.z)).toBe(true);
      expect(instance.position.z).toBeGreaterThanOrEqual(0);
      expect(instance.position.z).toBeLessThanOrEqual(200);
      expect(["left", "right"]).toContain(instance.side);
      expect(typeof instance.kind).toBe("string");
      expect(instance.biomeId.length).toBeGreaterThan(0);
    }
  });

  it("handles empty route gracefully", () => {
    const route = {
      id: "empty",
      lengthMeters: 0,
      points: [{ distanceMeters: 0, position: { x: 0, y: 0, z: 0 } }],
      biomes: [{ id: "coast", fromProgress01: 0, toProgress01: 1 }]
    };

    const instances = generateSceneryInstances(route, "empty-seed", "medium");

    expect(instances).toBeDefined();
    expect(Array.isArray(instances)).toBe(true);
  });

  it("keeps instance count within reasonable bounds", () => {
    const route = makeTestRoute();
    const instances = generateSceneryInstances(route, "count-seed", "medium");

    expect(instances.length).toBeGreaterThanOrEqual(12);
    expect(instances.length).toBeLessThanOrEqual(150);
  });
});

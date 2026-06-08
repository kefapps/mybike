import { describe, expect, it } from "vitest";
import { mockRouteDefinition } from "./mockRouteDefinition";

function expectFiniteVec3(position: { x: number; y: number; z: number }): void {
  expect(Number.isFinite(position.x)).toBe(true);
  expect(Number.isFinite(position.y)).toBe(true);
  expect(Number.isFinite(position.z)).toBe(true);
}

describe("mockRouteDefinition", () => {
  it("defines a short finite MVP route with ordered points", () => {
    expect(mockRouteDefinition.id).toBe("echappee-mock-route");
    expect(mockRouteDefinition.lengthMeters).toBe(1000);
    expect(mockRouteDefinition.points.length).toBeGreaterThanOrEqual(4);

    let previousDistanceMeters = -1;
    for (const point of mockRouteDefinition.points) {
      expect(point.distanceMeters).toBeGreaterThanOrEqual(0);
      expect(point.distanceMeters).toBeGreaterThan(previousDistanceMeters);
      expect(point.distanceMeters).toBeLessThanOrEqual(
        mockRouteDefinition.lengthMeters
      );
      expectFiniteVec3(point.position);
      previousDistanceMeters = point.distanceMeters;
    }

    expect(mockRouteDefinition.points[0].distanceMeters).toBe(0);
    expect(
      mockRouteDefinition.points[mockRouteDefinition.points.length - 1]
        .distanceMeters
    ).toBe(mockRouteDefinition.lengthMeters);
  });

  it("covers the full ride with at least two deterministic biomes", () => {
    expect(mockRouteDefinition.biomes.map((biome) => biome.id)).toEqual([
      "coast",
      "forest"
    ]);
    expect(mockRouteDefinition.biomes[0]).toMatchObject({
      fromProgress01: 0,
      toProgress01: 0.01
    });
    expect(mockRouteDefinition.biomes[1]).toMatchObject({
      fromProgress01: 0.01,
      toProgress01: 1
    });
  });

  it("contains bounded climbs and descents for the Three.js elevation feel pass", () => {
    const elevations = mockRouteDefinition.points.map((point) => point.position.y);
    const grades = mockRouteDefinition.points
      .slice(1)
      .map((point, index) => {
        const previous = mockRouteDefinition.points[index];
        const elevationDelta = point.position.y - previous.position.y;
        const distanceDelta = point.distanceMeters - previous.distanceMeters;

        return elevationDelta / distanceDelta;
      });

    expect(Math.min(...elevations)).toBeGreaterThanOrEqual(0);
    expect(Math.max(...elevations)).toBeGreaterThanOrEqual(10);
    expect(Math.max(...elevations)).toBeLessThanOrEqual(10);
    expect(grades.some((grade) => grade > 0.03)).toBe(true);
    expect(grades.some((grade) => grade < -0.03)).toBe(true);
    expect(Math.max(...grades.map((grade) => Math.abs(grade)))).toBeLessThan(0.07);
  });
});

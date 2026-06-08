import { describe, expect, it } from "vitest";

import { mockRouteDefinition } from "../route";
import {
  calculateGroundHeight,
  GROUND_MESH_CENTER_Z
} from "./groundHeight";
import { getRouteCenterXAtZ, getRouteElevationAtZ } from "./renderHelpers";

describe("calculateGroundHeight", () => {
  it("anchors the road corridor to route elevation", () => {
    const worldZ = 496;
    const localPlaneY = GROUND_MESH_CENTER_Z - worldZ;
    const routeCenterX = getRouteCenterXAtZ(mockRouteDefinition, worldZ);
    const routeElevation = getRouteElevationAtZ(mockRouteDefinition, worldZ);

    expect(calculateGroundHeight(mockRouteDefinition, routeCenterX, localPlaneY)).toBeCloseTo(
      routeElevation
    );
    expect(
      calculateGroundHeight(mockRouteDefinition, routeCenterX + 6.9, localPlaneY)
    ).toBeCloseTo(routeElevation);
  });

  it("adds terrain shape outside the ride corridor without losing the elevation base", () => {
    const worldZ = 496;
    const localPlaneY = GROUND_MESH_CENTER_Z - worldZ;
    const routeCenterX = getRouteCenterXAtZ(mockRouteDefinition, worldZ);
    const routeElevation = getRouteElevationAtZ(mockRouteDefinition, worldZ);
    const bermHeight = calculateGroundHeight(
      mockRouteDefinition,
      routeCenterX + 18,
      localPlaneY
    );
    const farHeight = calculateGroundHeight(
      mockRouteDefinition,
      routeCenterX + 130,
      localPlaneY
    );

    expect(bermHeight).toBeGreaterThan(routeElevation);
    expect(farHeight).toBeGreaterThan(routeElevation * 0.25);
    expect(farHeight).toBeLessThan(routeElevation + 8);
    expect(Number.isFinite(bermHeight)).toBe(true);
    expect(Number.isFinite(farHeight)).toBe(true);
  });
});

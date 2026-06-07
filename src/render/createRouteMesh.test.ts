import { describe, expect, it } from "vitest";

import { mockRouteDefinition } from "../route";
import { createRouteMesh } from "./createRouteMesh";

describe("createRouteMesh", () => {
  it("builds a layered scenic route group with shoulders, markings, and surface detail", () => {
    const routeMesh = createRouteMesh(mockRouteDefinition);

    expect(routeMesh.group.name).toBe("scenic-route");
    expect(routeMesh.mesh.name).toBe("scenic-route-surface");
    expect(routeMesh.group.children.map((child) => child.name)).toEqual(
      expect.arrayContaining([
        "scenic-route-surface",
        "scenic-route-left-shoulder",
        "scenic-route-right-shoulder",
        "scenic-route-left-edge",
        "scenic-route-right-edge",
        "scenic-route-center-markings",
        "scenic-route-surface-bands",
        "scenic-route-shoulder-rhythm",
        "scenic-route-surface-grain"
      ])
    );
    expect(routeMesh.geometries.length).toBeGreaterThanOrEqual(9);
    expect(routeMesh.materials.length).toBeGreaterThanOrEqual(7);
  });

  it("keeps repeated visual markers bounded and procedural", () => {
    const routeMesh = createRouteMesh(mockRouteDefinition);
    const centerMarkings = routeMesh.group.getObjectByName(
      "scenic-route-center-markings"
    );
    const surfaceBands = routeMesh.group.getObjectByName(
      "scenic-route-surface-bands"
    );
    const shoulderRhythm = routeMesh.group.getObjectByName(
      "scenic-route-shoulder-rhythm"
    );
    const surfaceGrain = routeMesh.group.getObjectByName(
      "scenic-route-surface-grain"
    );

    expect(centerMarkings?.children.length).toBeGreaterThan(10);
    expect(centerMarkings?.children.length).toBeLessThan(40);
    expect(surfaceBands?.children.length).toBeGreaterThan(8);
    expect(surfaceBands?.children.length).toBeLessThan(35);
    expect(shoulderRhythm?.children.length).toBeGreaterThan(30);
    expect(shoulderRhythm?.children.length).toBeLessThan(90);
    expect(surfaceGrain?.children.length).toBeGreaterThan(24);
    expect(surfaceGrain?.children.length).toBeLessThan(80);
  });
});

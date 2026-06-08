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
        "scenic-route-left-verge",
        "scenic-route-right-verge",
        "scenic-route-left-contact-shadow",
        "scenic-route-right-contact-shadow",
        "scenic-route-center-markings",
        "scenic-route-surface-bands",
        "scenic-route-shoulder-rhythm",
        "scenic-route-surface-grain"
      ])
    );
    expect(routeMesh.geometries.length).toBeGreaterThanOrEqual(13);
    expect(routeMesh.materials.length).toBeGreaterThanOrEqual(9);
  });

  it("adds route-following grounding bands outside the ride corridor", () => {
    const routeMesh = createRouteMesh(mockRouteDefinition);
    const leftVerge = routeMesh.group.getObjectByName("scenic-route-left-verge");
    const rightVerge = routeMesh.group.getObjectByName("scenic-route-right-verge");
    const leftShadow = routeMesh.group.getObjectByName(
      "scenic-route-left-contact-shadow"
    );
    const rightShadow = routeMesh.group.getObjectByName(
      "scenic-route-right-contact-shadow"
    );

    expect(leftVerge?.type).toBe("Mesh");
    expect(rightVerge?.type).toBe("Mesh");
    expect(leftShadow?.type).toBe("Mesh");
    expect(rightShadow?.type).toBe("Mesh");
    expect(leftVerge?.position.y ?? 0).toBe(0);
    expect(rightVerge?.position.y ?? 0).toBe(0);
  });

  it("densifies the road ribbon so elevation does not render as broken panels", () => {
    const routeMesh = createRouteMesh(mockRouteDefinition);
    const positions = routeMesh.geometry.attributes.position;
    const renderedFrameCount = positions.count / 2;
    let maxSegmentDepthMeters = 0;

    for (let frameIndex = 1; frameIndex < renderedFrameCount; frameIndex += 1) {
      const previousLeftIndex = (frameIndex - 1) * 2;
      const currentLeftIndex = frameIndex * 2;
      maxSegmentDepthMeters = Math.max(
        maxSegmentDepthMeters,
        Math.abs(
          positions.getZ(currentLeftIndex) - positions.getZ(previousLeftIndex)
        )
      );
    }

    expect(renderedFrameCount).toBeGreaterThan(100);
    expect(maxSegmentDepthMeters).toBeLessThanOrEqual(10);
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

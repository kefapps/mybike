import { describe, expect, it } from "vitest";
import type { Object3D } from "three";

import { mockRouteDefinition } from "../route";
import { createBiomeVisuals } from "./createBiomeVisuals";
import { calculateGroundHeight, GROUND_MESH_CENTER_Z } from "./groundHeight";
import { getRouteCenterXAtZ } from "./renderHelpers";

describe("createBiomeVisuals", () => {
  it("creates dense reusable lateral scenery with lightweight local scene life", () => {
    const visuals = createBiomeVisuals();
    const names = collectObjectNames(visuals.group);
    const meshCount = countRenderableMeshes(visuals.group);

    expect(visuals.group.name).toBe("scenic-biome-visuals");
    expect(visuals.group.children.length).toBeGreaterThan(120);
    expect(meshCount).toBeLessThan(410);
    expect([...names]).toEqual(
      expect.arrayContaining([
        "scenic-coast-post",
        "scenic-coast-lighthouse",
        "scenic-coast-rock-arch",
        "scenic-near-berm",
        "scenic-terrain-ridge",
        "scenic-grass-tuft",
        "scenic-rock",
        "scenic-tree-trunk",
        "scenic-tree-canopy",
        "scenic-forest-canopy-tunnel",
        "scenic-road-sign",
        "scenic-near-road-motion-details",
        "scenic-life",
        "scenic-life-bird",
        "scenic-life-human"
      ])
    );
  });

  it("builds recognizable biome landmarks and layered terrain silhouettes", () => {
    const visuals = createBiomeVisuals();
    const lighthouse = visuals.group.getObjectByName("scenic-coast-lighthouse");
    const arch = visuals.group.getObjectByName("scenic-coast-rock-arch");
    const forestTunnel = visuals.group.getObjectByName(
      "scenic-forest-canopy-tunnel"
    );
    const firstTunnelTrunk = findObjectsByName(
      visuals.group,
      "scenic-forest-tunnel-trunk"
    )[0];
    const terrainRidges = findObjectsByName(
      visuals.group,
      "scenic-terrain-ridge"
    );
    const nearBerms = findObjectsByName(visuals.group, "scenic-near-berm");
    const firstBerm = nearBerms[0];

    expect(lighthouse?.children.length).toBeGreaterThanOrEqual(4);
    expect(arch?.children.length).toBeGreaterThanOrEqual(3);
    expect(forestTunnel?.children.length).toBeGreaterThanOrEqual(12);
    expect(firstTunnelTrunk.position.z).toBeLessThanOrEqual(250);
    expect(terrainRidges.length).toBeGreaterThanOrEqual(6);
    expect(nearBerms.length).toBeGreaterThanOrEqual(10);
    expect(
      Math.abs(
        firstBerm.position.x -
          getRouteCenterXAtZ(mockRouteDefinition, firstBerm.position.z)
      )
    ).toBeGreaterThanOrEqual(28);
  });

  it("adds bounded near-road parallax families outside the ride corridor", () => {
    const visuals = createBiomeVisuals();
    const speedPosts = findObjectsByName(visuals.group, "scenic-near-speed-post");
    const edgeGrass = findObjectsByName(visuals.group, "scenic-near-edge-grass");
    const edgeStones = findObjectsByName(visuals.group, "scenic-near-edge-stone");

    expect(speedPosts.length).toBeGreaterThanOrEqual(34);
    expect(speedPosts.length).toBeLessThanOrEqual(52);
    expect(edgeGrass.length).toBeGreaterThanOrEqual(48);
    expect(edgeGrass.length).toBeLessThanOrEqual(96);
    expect(edgeStones.length).toBeGreaterThanOrEqual(20);
    expect(edgeStones.length).toBeLessThanOrEqual(44);
    expect(Math.min(...speedPosts.map((post) => post.position.z))).toBeLessThan(42);
    expect(Math.max(...speedPosts.map((post) => post.position.z))).toBeGreaterThan(930);

    for (const object of [...speedPosts, ...edgeGrass, ...edgeStones]) {
      const lateralDistance = Math.abs(
        object.position.x - getRouteCenterXAtZ(mockRouteDefinition, object.position.z)
      );

      expect(lateralDistance).toBeGreaterThanOrEqual(6.2);
      expect(lateralDistance).toBeLessThanOrEqual(15.5);
    }
  });

  it("keeps coast and forest motion details visually distinct", () => {
    const visuals = createBiomeVisuals();
    const coastReeds = findObjectsByName(visuals.group, "scenic-coast-motion-reed");
    const forestFerns = findObjectsByName(visuals.group, "scenic-forest-motion-fern");

    expect(coastReeds.length).toBeGreaterThanOrEqual(8);
    expect(forestFerns.length).toBeGreaterThanOrEqual(14);
    expect(Math.max(...coastReeds.map((reed) => reed.position.z))).toBeLessThan(230);
    expect(Math.min(...forestFerns.map((fern) => fern.position.z))).toBeGreaterThan(235);
  });

  it("adds visible birds and roadside silhouettes without occupying the ride lane", () => {
    const visuals = createBiomeVisuals();
    const birds = findObjectsByName(visuals.group, "scenic-life-bird");
    const humans = findObjectsByName(visuals.group, "scenic-life-human");

    expect(birds).toHaveLength(5);
    expect(humans).toHaveLength(3);
    expect(Math.min(...birds.map((bird) => bird.position.y))).toBeGreaterThan(8);
    expect(Math.max(...birds.map((bird) => bird.position.y))).toBeLessThan(13);
    expect(Math.min(...birds.map((bird) => bird.position.z))).toBeLessThan(70);
    expect(Math.max(...birds.map((bird) => bird.position.z))).toBeGreaterThan(500);
    expect(Math.min(...humans.map((human) => human.position.z))).toBeLessThan(110);

    for (const human of humans) {
      const lateralDistance = Math.abs(
        human.position.x - getRouteCenterXAtZ(mockRouteDefinition, human.position.z)
      );

      expect(lateralDistance).toBeGreaterThanOrEqual(7.2);
      expect(lateralDistance).toBeLessThanOrEqual(8.7);
      expect(human.position.y).toBeCloseTo(
        calculateGroundHeight(
          mockRouteDefinition,
          human.position.x,
          GROUND_MESH_CENTER_Z - human.position.z
        ) + 0.02
      );
      expect(human.children.length).toBeGreaterThanOrEqual(4);
    }

    for (const object of [...birds, ...humans]) {
      expect(Number.isFinite(object.position.x)).toBe(true);
      expect(Number.isFinite(object.position.y)).toBe(true);
      expect(Number.isFinite(object.position.z)).toBe(true);
    }
  });

  it("tracks shared resources for deterministic disposal", () => {
    const visuals = createBiomeVisuals();

    expect(visuals.geometries.length).toBeGreaterThanOrEqual(29);
    expect(visuals.materials.length).toBeGreaterThanOrEqual(21);
    expect(visuals.fallbackTintMaterials.length).toBeGreaterThanOrEqual(6);
    expect(visuals.coastMaterial).toBe(visuals.materials[0]);
    expect(visuals.forestMaterial).toBe(visuals.materials[1]);
  });
});

function collectObjectNames(root: Object3D): Set<string> {
  const names = new Set<string>();
  root.traverse((object) => {
    if (object.name) {
      names.add(object.name);
    }
  });

  return names;
}

function findObjectsByName(root: Object3D, name: string): Object3D[] {
  const matches: Object3D[] = [];
  root.traverse((object) => {
    if (object.name === name) {
      matches.push(object);
    }
  });

  return matches;
}

function countRenderableMeshes(root: Object3D): number {
  let count = 0;
  root.traverse((object) => {
    if (object.type === "Mesh") {
      count += 1;
    }
  });

  return count;
}

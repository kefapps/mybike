import * as THREE from "three";

import { resolveRouteDefinition, type RouteDefinition, type Vec3 } from "../route";

export type BiomePalette = {
  sky: number;
  ground: number;
  route: number;
  fog: number;
  fogNear: number;
  fogFar: number;
  accent: number;
  horizon: number;
  horizonOpacity: number;
  ambientColor: number;
  ambientIntensity: number;
  sunColor: number;
  sunIntensity: number;
  sunPosition: Vec3;
};

const BIOME_PALETTES: Record<string, BiomePalette> = {
  coast: {
    sky: 0xf3c995,
    ground: 0x79b99b,
    route: 0xeecf82,
    fog: 0xf6dcc1,
    fogNear: 18,
    fogFar: 430,
    accent: 0x277fa8,
    horizon: 0x5fa6c8,
    horizonOpacity: 0.58,
    ambientColor: 0xfff2d0,
    ambientIntensity: 0.86,
    sunColor: 0xffefc2,
    sunIntensity: 1.32,
    sunPosition: { x: -26, y: 42, z: -32 }
  },
  forest: {
    sky: 0x8fbf9a,
    ground: 0x234c34,
    route: 0x8c7047,
    fog: 0x31513f,
    fogNear: 12,
    fogFar: 260,
    accent: 0x1a5f39,
    horizon: 0x3f6b50,
    horizonOpacity: 0.64,
    ambientColor: 0xcce5c0,
    ambientIntensity: 0.56,
    sunColor: 0xd9f0c0,
    sunIntensity: 0.82,
    sunPosition: { x: 22, y: 30, z: -18 }
  },
  placeholder: {
    sky: 0xcbd4d8,
    ground: 0x8b9691,
    route: 0x607080,
    fog: 0xd9dddf,
    fogNear: 20,
    fogFar: 360,
    accent: 0x4d5a62,
    horizon: 0xaab4b8,
    horizonOpacity: 0.5,
    ambientColor: 0xffffff,
    ambientIntensity: 0.72,
    sunColor: 0xffffff,
    sunIntensity: 1,
    sunPosition: { x: -14, y: 38, z: -18 }
  }
};

export function toThreeVector3(vec: Vec3, yOffset = 0): THREE.Vector3 {
  return new THREE.Vector3(vec.x, vec.y + yOffset, vec.z);
}

export function getBiomePalette(
  biomeId: string,
  usedFallback: boolean
): BiomePalette {
  if (usedFallback) {
    return BIOME_PALETTES.placeholder;
  }

  return BIOME_PALETTES[biomeId] ?? BIOME_PALETTES.placeholder;
}

export function getRouteRenderPoints(route: RouteDefinition): THREE.Vector3[] {
  const { route: resolvedRoute } = resolveRouteDefinition(route);

  return resolvedRoute.points.map((point) => toThreeVector3(point.position, 0.03));
}

export function getRouteCenterXAtZ(route: RouteDefinition, worldZ: number): number {
  return getRouteValueAtZ(route, worldZ, "x");
}

export function getRouteElevationAtZ(
  route: RouteDefinition,
  worldZ: number
): number {
  return getRouteValueAtZ(route, worldZ, "y");
}

function getRouteValueAtZ(
  route: RouteDefinition,
  worldZ: number,
  axis: "x" | "y"
): number {
  const { route: resolvedRoute } = resolveRouteDefinition(route);
  const first = resolvedRoute.points[0];
  const last = resolvedRoute.points.at(-1);

  if (!first || !last) {
    return 0;
  }

  if (worldZ <= first.position.z) {
    return first.position[axis];
  }

  if (worldZ >= last.position.z) {
    return last.position[axis];
  }

  for (let index = 0; index < resolvedRoute.points.length - 1; index += 1) {
    const current = resolvedRoute.points[index];
    const next = resolvedRoute.points[index + 1];

    if (worldZ >= current.position.z && worldZ <= next.position.z) {
      const spanZ = next.position.z - current.position.z || 1;
      const progress = (worldZ - current.position.z) / spanZ;

      return (
        current.position[axis] +
        (next.position[axis] - current.position[axis]) * progress
      );
    }
  }

  return last.position[axis];
}

import * as THREE from "three";

import { resolveRouteDefinition, type RouteDefinition, type Vec3 } from "../route";

export type BiomePalette = {
  sky: number;
  ground: number;
  route: number;
  fog: number;
  accent: number;
};

const BIOME_PALETTES: Record<string, BiomePalette> = {
  coast: {
    sky: 0xb7d9ec,
    ground: 0x8cc7a5,
    route: 0xf2d58b,
    fog: 0xd4eef5,
    accent: 0x4f9fcf
  },
  forest: {
    sky: 0xa9c7b0,
    ground: 0x2f6f45,
    route: 0x9a7b4f,
    fog: 0x6d8a74,
    accent: 0x174d2e
  },
  placeholder: {
    sky: 0xcbd4d8,
    ground: 0x8b9691,
    route: 0x607080,
    fog: 0xd9dddf,
    accent: 0x4d5a62
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

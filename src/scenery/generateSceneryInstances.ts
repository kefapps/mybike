import type { RouteDefinition, RoutePoint, Vec3 } from "../route";
import { selectBiomeAtProgress } from "../route";
import {
  BIOME_SCENERY_CONFIGS,
  QUALITY_STEP_METERS,
  ASSET_PLACEMENT_BOUNDS
} from "./biomeSceneryConfigs";
import {
  hashString,
  mulberry32,
  pickRandom,
  positionKey,
  sampleUniform
} from "./prng";
import type { SceneryInstance, SceneryQuality } from "./sceneryTypes";

export function generateSceneryInstances(
  route: RouteDefinition,
  seed: string,
  quality: SceneryQuality = "medium"
): SceneryInstance[] {
  const instances: SceneryInstance[] = [];
  const stepMeters = QUALITY_STEP_METERS[quality];

  for (
    let distance = 0;
    distance <= route.lengthMeters;
    distance += stepMeters
  ) {
    const progress01 = distance / route.lengthMeters;
    const biomeId = selectBiomeAtProgress(route, progress01);
    const biomeConfig =
      BIOME_SCENERY_CONFIGS[biomeId] ?? BIOME_SCENERY_CONFIGS.placeholder;

    const routePoint = interpolatePointAtDistance(route.points, distance);
    const routeZ = routePoint.z;

    for (const side of [-1, 1] as const) {
      const sideLabel = side === -1 ? "left" : "right";

      for (
        let instanceIndex = 0;
        instanceIndex < biomeConfig.instancesPerSample;
        instanceIndex += 1
      ) {
        const instanceSeed = hashString(
          positionKey(seed, distance, side, instanceIndex)
        );
        const rand = mulberry32(instanceSeed);

        const assetKind = pickRandom(rand, biomeConfig.assetKinds);
        const bounds = ASSET_PLACEMENT_BOUNDS[assetKind];

        if (!bounds) {
          continue;
        }

        const lateralDistance = sampleUniform(
          rand,
          bounds.minSide,
          bounds.maxSide
        );
        const routeX = interpolateRouteXAtZ(route, routeZ);
        const instanceX = routeX + side * lateralDistance;

        const scale = sampleUniform(rand, bounds.minScale, bounds.maxScale);
        const zJitter = sampleUniform(rand, -stepMeters * 0.45, stepMeters * 0.45);
        const instanceZ = Math.max(
          0,
          Math.min(route.lengthMeters, routeZ + zJitter)
        );

        const yOffset =
          assetKind === "treeCanopy"
            ? 8.4 * scale
            : assetKind === "treeTrunk"
              ? 2.5 * scale
              : assetKind === "ridge"
                ? 4.2
                : assetKind === "berm"
                  ? 1.15
                  : 0;

        instances.push({
          kind: assetKind,
          position: {
            x: instanceX,
            y: yOffset,
            z: instanceZ
          },
          rotation: {
            x: sampleUniform(rand, -0.12, 0.12),
            y: sampleUniform(rand, -0.35, 0.35),
            z: side * sampleUniform(rand, -0.1, 0.1)
          },
          scale: {
            x: scale,
            y: sampleUniform(rand, scale * 0.82, scale * 1.18),
            z: sampleUniform(rand, scale * 0.88, scale * 1.12)
          },
          biomeId,
          side: sideLabel
        });
      }
    }
  }

  return instances;
}

function interpolatePointAtDistance(
  points: RoutePoint[],
  distanceMeters: number
): Vec3 {
  if (points.length === 0) {
    return { x: 0, y: 0, z: 0 };
  }

  const first = points[0];
  const last = points[points.length - 1];

  if (distanceMeters <= first.distanceMeters) {
    return first.position;
  }

  if (distanceMeters >= last.distanceMeters) {
    return last.position;
  }

  for (let index = 0; index < points.length - 1; index += 1) {
    const current = points[index];
    const next = points[index + 1];

    if (
      distanceMeters >= current.distanceMeters &&
      distanceMeters <= next.distanceMeters
    ) {
      const span = next.distanceMeters - current.distanceMeters || 1;
      const t = (distanceMeters - current.distanceMeters) / span;

      return {
        x: current.position.x + (next.position.x - current.position.x) * t,
        y: current.position.y + (next.position.y - current.position.y) * t,
        z: current.position.z + (next.position.z - current.position.z) * t
      };
    }
  }

  return last.position;
}

function interpolateRouteXAtZ(route: RouteDefinition, worldZ: number): number {
  const { points } = route;

  if (points.length === 0) {
    return 0;
  }

  const first = points[0];
  const last = points[points.length - 1];

  if (worldZ <= first.position.z) {
    return first.position.x;
  }

  if (worldZ >= last.position.z) {
    return last.position.x;
  }

  for (let index = 0; index < points.length - 1; index += 1) {
    const current = points[index];
    const next = points[index + 1];

    if (worldZ >= current.position.z && worldZ <= next.position.z) {
      const spanZ = next.position.z - current.position.z || 1;
      const t = (worldZ - current.position.z) / spanZ;

      return current.position.x + (next.position.x - current.position.x) * t;
    }
  }

  return last.position.x;
}

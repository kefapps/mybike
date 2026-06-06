import { clampProgress01 } from "./routeMath";
import { resolveRouteDefinition } from "./routeValidation";
import type { RouteDefinition } from "./routeTypes";

export function selectBiomeAtProgress(
  routeInput: RouteDefinition | null | undefined,
  progress01Input: number
): string {
  const { route } = resolveRouteDefinition(routeInput);
  const progress01 = clampProgress01(progress01Input);

  const matchingBiome = route.biomes.find((biome) => {
    const isEnd = progress01 === 1 && biome.toProgress01 === 1;

    return (
      progress01 >= biome.fromProgress01 &&
      (progress01 < biome.toProgress01 || isEnd)
    );
  });

  if (matchingBiome !== undefined) {
    return matchingBiome.id;
  }

  if (progress01 < route.biomes[0].fromProgress01) {
    return route.biomes[0].id;
  }

  return route.biomes[route.biomes.length - 1].id;
}

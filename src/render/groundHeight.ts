import type { RouteDefinition } from "../route";
import { getRouteCenterXAtZ, getRouteElevationAtZ } from "./renderHelpers";

export const GROUND_MESH_CENTER_Z = 560;

export function calculateGroundHeight(
  route: RouteDefinition,
  localX: number,
  localPlaneY: number
): number {
  const worldZ = GROUND_MESH_CENTER_Z - localPlaneY;
  const routeCenterX = getRouteCenterXAtZ(route, worldZ);
  const routeElevation = getRouteElevationAtZ(route, worldZ);
  const lateralDistance = Math.abs(localX - routeCenterX);
  const roadClearance = Math.max(0, lateralDistance - 7);
  const elevationFollow =
    roadClearance === 0 ? 1 : 0.28 + 0.72 * Math.max(0, 1 - roadClearance / 110);
  const berm =
    Math.max(0, 1 - Math.abs(lateralDistance - 18) / 12) *
    (1.2 + Math.sin(localPlaneY * 0.021) * 0.35);
  const midSlope =
    Math.min(1, Math.max(0, (lateralDistance - 34) / 80)) * 4.8;
  const farRidge =
    Math.min(1, Math.max(0, (lateralDistance - 108) / 86)) * 6.4;
  const wave =
    Math.sin(localPlaneY * 0.018) * 0.38 +
    Math.cos((localX + localPlaneY) * 0.012) * 0.32 +
    Math.sin((localX - localPlaneY) * 0.007) * 0.28;
  const terrainDetail =
    roadClearance > 0 ? berm + midSlope + farRidge + wave * 1.25 : 0;

  return routeElevation * elevationFollow + terrainDetail;
}

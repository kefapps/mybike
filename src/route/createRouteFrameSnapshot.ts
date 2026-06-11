import { cameraOnRail } from "./cameraOnRail";
import { mockRouteDefinition } from "./mockRouteDefinition";
import { sampleRouteAt } from "./sampleRouteAt";
import { selectBiomeAtProgress } from "./selectBiomeAtProgress";
import { selectSegmentAtProgress } from "./segments";
import type {
  CameraRailConfig,
  RouteDefinition,
  RouteFrameSnapshot,
  RouteProgressInput
} from "./routeTypes";

export function createRouteFrameSnapshot(
  progress: RouteProgressInput,
  route: RouteDefinition | null | undefined = mockRouteDefinition,
  cameraConfig: Partial<CameraRailConfig> | null | undefined = {}
): RouteFrameSnapshot {
  const sample = sampleRouteAt(route, extractProgress01(progress));
  const camera = cameraOnRail(route, progress, cameraConfig);
  const biomeId = selectBiomeAtProgress(route, sample.progress01);
  const segment = selectSegmentAtProgress(route, sample.progress01);

  return {
    sample,
    camera,
    biomeId,
    segment,
    usedFallback: sample.usedFallback || camera.usedFallback,
    fallbackReason: sample.fallbackReason ?? camera.fallbackReason
  };
}

function extractProgress01(progress: RouteProgressInput): number {
  if (typeof progress === "number") {
    return progress;
  }

  if ("progress" in progress) {
    return progress.progress.progress01;
  }

  return progress.progress01;
}

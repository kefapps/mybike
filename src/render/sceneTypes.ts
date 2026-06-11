import type { RideFrameSnapshot } from "../ride";
import type { RouteFrameSnapshot } from "../route";
import type { EnvironmentPresetId } from "./environmentPresets";

export type RenderFrameSnapshot = {
  ride: RideFrameSnapshot;
  route: RouteFrameSnapshot;
  environmentPresetId?: EnvironmentPresetId | null | undefined;
  weatherEnabled?: boolean | null | undefined;
};

export type SceneController = {
  mount(canvas: HTMLCanvasElement): boolean;
  resize(width: number, height: number): void;
  update(snapshot: RenderFrameSnapshot): void;
  dispose(): void;
};

export type SceneControllerFactory = () => SceneController;

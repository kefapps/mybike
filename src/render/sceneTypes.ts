import type { RideFrameSnapshot } from "../ride";
import type { RouteFrameSnapshot } from "../route";

export type RenderFrameSnapshot = {
  ride: RideFrameSnapshot;
  route: RouteFrameSnapshot;
};

export type SceneController = {
  mount(canvas: HTMLCanvasElement): boolean;
  resize(width: number, height: number): void;
  update(snapshot: RenderFrameSnapshot): void;
  dispose(): void;
};

export type SceneControllerFactory = () => SceneController;

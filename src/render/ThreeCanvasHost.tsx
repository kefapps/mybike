import { useEffect, useRef } from "react";

import {
  advanceRideFrame,
  createInitialRideFrameState,
  createMockRideInputSource,
  type RideFrameState,
  type RideInputSource
} from "../ride";
import {
  createRouteFrameSnapshot,
  mockRouteDefinition,
  type RouteDefinition
} from "../route";
import { createSceneController } from "./SceneController";
import type {
  RenderFrameSnapshot,
  SceneController,
  SceneControllerFactory
} from "./sceneTypes";

type RideRenderPhase = "running" | "paused";

export type ThreeCanvasHostProps = {
  phase: RideRenderPhase;
  route?: RouteDefinition;
  controllerFactory?: SceneControllerFactory;
  createInputSource?: () => RideInputSource;
  now?: () => number;
  scheduleFrame?: (callback: FrameRequestCallback) => number;
  cancelFrame?: (handle: number) => void;
};

const DEFAULT_WIDTH = 640;
const DEFAULT_HEIGHT = 360;
const MAX_DT_SECONDS = 0.1;
const MOCK_EFFORT_FOR_VISIBLE_MOTION = 0.55;

export function ThreeCanvasHost({
  phase,
  route = mockRouteDefinition,
  controllerFactory,
  createInputSource,
  now = defaultNow,
  scheduleFrame = defaultScheduleFrame,
  cancelFrame = defaultCancelFrame
}: ThreeCanvasHostProps) {
  const canvasRef = useRef<HTMLCanvasElement | null>(null);
  const phaseRef = useRef<RideRenderPhase>(phase);

  useEffect(() => {
    phaseRef.current = phase;
  }, [phase]);

  useEffect(() => {
    const canvas = canvasRef.current;

    if (!canvas) {
      return undefined;
    }

    const controller =
      controllerFactory === undefined
        ? createSceneController({ route })
        : controllerFactory();
    const inputSource =
      createInputSource === undefined
        ? createMockRideInputSource(MOCK_EFFORT_FOR_VISIBLE_MOTION)
        : createInputSource();
    const startedAtMs = now();
    let rideState: RideFrameState = createInitialRideFrameState(startedAtMs);
    let lastFrameAtMs = startedAtMs;
    let frameHandle: number | undefined;
    let disposed = false;

    controller.mount(canvas);

    const resize = () => {
      const { width, height } = measureCanvas(canvas);
      controller.resize(width, height);
    };

    const emitFrame = (frameNowMs: number, dtSeconds: number) => {
      const result = advanceRideFrame(
        inputSource,
        rideState,
        frameNowMs,
        dtSeconds
      );
      rideState = result.state;

      const snapshot: RenderFrameSnapshot = {
        ride: result.snapshot,
        route: createRouteFrameSnapshot(result.snapshot, route)
      };

      controller.update(snapshot);
    };

    const scheduleNextFrame = () => {
      frameHandle = scheduleFrame((frameNowMs) => {
        if (disposed) {
          return;
        }

        const safeFrameNowMs = Number.isFinite(frameNowMs) ? frameNowMs : now();

        if (phaseRef.current === "running") {
          const dtSeconds = Math.min(
            MAX_DT_SECONDS,
            Math.max(0, (safeFrameNowMs - lastFrameAtMs) / 1000)
          );
          emitFrame(safeFrameNowMs, dtSeconds);
        }

        lastFrameAtMs = safeFrameNowMs;
        scheduleNextFrame();
      });
    };

    const resizeObserver =
      typeof ResizeObserver === "undefined"
        ? undefined
        : new ResizeObserver(resize);

    resize();
    emitFrame(startedAtMs, 0);
    resizeObserver?.observe(canvas);
    window.addEventListener("resize", resize);
    scheduleNextFrame();

    return () => {
      disposed = true;

      if (frameHandle !== undefined) {
        cancelFrame(frameHandle);
      }

      resizeObserver?.disconnect();
      window.removeEventListener("resize", resize);
      controller.dispose();
    };
  }, [cancelFrame, controllerFactory, createInputSource, now, route, scheduleFrame]);

  return (
    <canvas
      ref={canvasRef}
      className="three-canvas"
      aria-label="Rendu 3D de la balade mock"
    />
  );
}

function measureCanvas(canvas: HTMLCanvasElement): {
  width: number;
  height: number;
} {
  const rect = canvas.getBoundingClientRect();
  const width = canvas.clientWidth || rect.width || DEFAULT_WIDTH;
  const height = canvas.clientHeight || rect.height || DEFAULT_HEIGHT;

  return { width, height };
}

function defaultNow(): number {
  return typeof performance === "undefined" ? Date.now() : performance.now();
}

function defaultScheduleFrame(callback: FrameRequestCallback): number {
  return requestAnimationFrame(callback);
}

function defaultCancelFrame(handle: number): void {
  cancelAnimationFrame(handle);
}

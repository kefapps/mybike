import { useEffect, useRef } from "react";

import {
  advanceRideFrame,
  createInitialRideFrameState,
  createMockRideInputSource,
  type MockRideInputSource,
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
  effort01?: number;
  onRenderFailure?: (message: string) => void;
  onFrame?: (snapshot: RenderFrameSnapshot) => void;
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
  effort01,
  onRenderFailure,
  onFrame,
  now = defaultNow,
  scheduleFrame = defaultScheduleFrame,
  cancelFrame = defaultCancelFrame
}: ThreeCanvasHostProps) {
  const canvasRef = useRef<HTMLCanvasElement | null>(null);
  const phaseRef = useRef<RideRenderPhase>(phase);
  const effortRef = useRef(effort01 ?? MOCK_EFFORT_FOR_VISIBLE_MOTION);
  const shouldSyncEffortRef = useRef(
    createInputSource === undefined || effort01 !== undefined
  );
  const onFrameRef = useRef(onFrame);
  const onRenderFailureRef = useRef(onRenderFailure);

  useEffect(() => {
    phaseRef.current = phase;
  }, [phase]);

  useEffect(() => {
    effortRef.current = effort01 ?? MOCK_EFFORT_FOR_VISIBLE_MOTION;
    shouldSyncEffortRef.current =
      createInputSource === undefined || effort01 !== undefined;
  }, [createInputSource, effort01]);

  useEffect(() => {
    onFrameRef.current = onFrame;
  }, [onFrame]);

  useEffect(() => {
    onRenderFailureRef.current = onRenderFailure;
  }, [onRenderFailure]);

  useEffect(() => {
    const canvas = canvasRef.current;

    if (!canvas) {
      return undefined;
    }

    const controller =
      controllerFactory === undefined
        ? createSceneController({ route })
        : controllerFactory();
    const startedAtMs = now();
    let rideState: RideFrameState = createInitialRideFrameState(startedAtMs);
    let lastFrameAtMs = startedAtMs;
    let activeRideNowMs = startedAtMs;
    let frameHandle: number | undefined;
    let resizeObserver: ResizeObserver | undefined;
    let handleContextLost: ((event: Event) => void) | undefined;
    let handleContextRestored: (() => void) | undefined;
    let resize: (() => void) | undefined;
    const controllerFailedRef = { current: false };
    let disposed = false;

    const reportFailure = (error: unknown): void => {
      if (controllerFailedRef.current || disposed) {
        return;
      }

      controllerFailedRef.current = true;
      const message =
        error instanceof Error
          ? error.message
          : "La creation du rendu 3D a echoue.";
      onRenderFailureRef.current?.(message);
      stopLoop();
    };

    const stopLoop = () => {
      if (disposed) {
        return;
      }

      disposed = true;

      if (frameHandle !== undefined) {
        cancelFrame(frameHandle);
      }

      resizeObserver?.disconnect();
      if (resize) {
        window.removeEventListener("resize", resize);
      }
      if (handleContextLost) {
        canvas.removeEventListener("webglcontextlost", handleContextLost);
      }
      if (handleContextRestored) {
        canvas.removeEventListener("webglcontextrestored", handleContextRestored);
      }
      controller.dispose();
    };

    const onFrameError = (error: unknown) => {
      reportFailure(error);
    };

    let inputSource: RideInputSource;
    try {
      inputSource =
        createInputSource === undefined
          ? createMockRideInputSource(effortRef.current)
          : createInputSource();
    } catch (error) {
      reportFailure(error);
      return () => {
        stopLoop();
      };
    }

    let mounted = false;
    try {
      mounted = controller.mount(canvas);
    } catch (error) {
      reportFailure(error);
      return () => {
        stopLoop();
      };
    }

    if (!mounted) {
      reportFailure(
        new Error(
          "WebGL renderer introuvable ou indisponible. Verifiez que votre navigateur prend en charge WebGL."
        )
      );
      return () => {
        stopLoop();
      };
    }

    resize = () => {
      const { width, height } = measureCanvas(canvas);
      controller.resize(width, height);
    };

    const emitFrame = (frameNowMs: number, dtSeconds: number) => {
      if (shouldSyncEffortRef.current) {
        syncInputSourceEffort(inputSource, effortRef.current);
      }

      const result = advanceRideFrame(
        inputSource,
        rideState,
        frameNowMs,
        dtSeconds,
        undefined,
        route
      );
      rideState = result.state;

      const snapshot: RenderFrameSnapshot = {
        ride: result.snapshot,
        route: createRouteFrameSnapshot(result.snapshot, route)
      };

      controller.update(snapshot);
      onFrameRef.current?.(snapshot);
    };

    const safeEmitFrame = (frameNowMs: number, dtSeconds: number) => {
      try {
        emitFrame(frameNowMs, dtSeconds);
      } catch (error) {
        onFrameError(error);
      }
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
          activeRideNowMs += dtSeconds * 1000;
          safeEmitFrame(activeRideNowMs, dtSeconds);
        }

        lastFrameAtMs = safeFrameNowMs;
        scheduleNextFrame();
      });
    };

    handleContextLost = (event: Event) => {
      event.preventDefault();
      reportFailure(new Error("Le contexte WebGL a ete perdu."));
    };

    handleContextRestored = () => {
      if (controllerFailedRef.current || disposed) {
        return;
      }

      try {
        resize?.();
      } catch (error) {
        reportFailure(error);
      }
    };

    resizeObserver =
      typeof ResizeObserver === "undefined"
        ? undefined
        : new ResizeObserver(resize);

    resize();
    safeEmitFrame(startedAtMs, 0);
    resizeObserver?.observe(canvas);
    window.addEventListener("resize", resize);
    canvas.addEventListener("webglcontextlost", handleContextLost);
    canvas.addEventListener("webglcontextrestored", handleContextRestored);
    scheduleNextFrame();

    return () => {
      stopLoop();
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

function syncInputSourceEffort(
  inputSource: RideInputSource,
  effort01: number
): void {
  if (isMutableMockInputSource(inputSource)) {
    inputSource.setEffort01(effort01);
  }
}

function isMutableMockInputSource(
  inputSource: RideInputSource
): inputSource is MockRideInputSource {
  return (
    inputSource.kind === "mock" &&
    typeof (inputSource as Partial<MockRideInputSource>).setEffort01 ===
      "function"
  );
}

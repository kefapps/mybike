/* @vitest-environment jsdom */
import { act } from "react";
import { createRoot, type Root } from "react-dom/client";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";

import { createMockRideInputSource } from "../ride";
import { ThreeCanvasHost } from "./ThreeCanvasHost";
import type { RenderFrameSnapshot, SceneController } from "./sceneTypes";

function createFakeController(): SceneController & {
  updates: RenderFrameSnapshot[];
  resizes: Array<{ width: number; height: number }>;
} {
  return {
    updates: [],
    resizes: [],
    mount: vi.fn(),
    resize: vi.fn((width: number, height: number) => {
      fakeController.resizes.push({ width, height });
    }),
    update: vi.fn((snapshot: RenderFrameSnapshot) => {
      fakeController.updates.push(snapshot);
    }),
    dispose: vi.fn()
  };
}

let fakeController: ReturnType<typeof createFakeController>;

function renderHost(props?: Partial<Parameters<typeof ThreeCanvasHost>[0]>) {
  const container = document.createElement("div");
  document.body.append(container);
  const root = createRoot(container);
  const frameCallbacks: FrameRequestCallback[] = [];
  let nowMs = 0;

  fakeController = createFakeController();
  const controllerFactory = () => fakeController;
  const getNow = () => nowMs;
  const scheduleFrame = (callback: FrameRequestCallback) => {
    frameCallbacks.push(callback);
    return frameCallbacks.length;
  };
  const cancelFrame = vi.fn();
  let currentProps = props ?? {};

  function render(
    phase: "running" | "paused",
    nextProps?: Partial<Parameters<typeof ThreeCanvasHost>[0]>
  ) {
    currentProps = {
      ...currentProps,
      ...nextProps
    };

    root.render(
      <ThreeCanvasHost
        controllerFactory={controllerFactory}
        now={getNow}
        scheduleFrame={scheduleFrame}
        cancelFrame={cancelFrame}
        {...currentProps}
        phase={phase}
      />
    );
  }

  act(() => {
    render(props?.phase ?? "running");
  });

  return {
    container,
    root,
    tick(ms: number) {
      nowMs += ms;
      const callback = frameCallbacks.shift();

      if (!callback) {
        throw new Error("No frame callback scheduled");
      }

      act(() => {
        callback(nowMs);
      });
    },
    rerender(
      phase: "running" | "paused",
      nextProps?: Partial<Parameters<typeof ThreeCanvasHost>[0]>
    ) {
      act(() => {
        render(phase, nextProps);
      });
    }
  };
}

describe("ThreeCanvasHost", () => {
  let root: Root | undefined;

  beforeEach(() => {
    Object.assign(globalThis, { IS_REACT_ACT_ENVIRONMENT: true });
  });

  afterEach(() => {
    if (root) {
      act(() => {
        root?.unmount();
      });
    }

    document.body.replaceChildren();
    vi.restoreAllMocks();
    root = undefined;
  });

  it("mounts, renders an initial composed snapshot, resizes, and disposes", () => {
    const rendered = renderHost();
    root = rendered.root;

    const canvas = rendered.container.querySelector("canvas");
    expect(canvas).toBeInstanceOf(HTMLCanvasElement);
    expect(fakeController.mount).toHaveBeenCalledWith(canvas);
    expect(fakeController.resize).toHaveBeenCalledWith(640, 360);
    expect(fakeController.updates).toHaveLength(1);
    expect(fakeController.updates[0].ride.source).toBe("mock");
    expect(fakeController.updates[0].route.biomeId).toBe("coast");

    act(() => {
      window.dispatchEvent(new Event("resize"));
    });

    expect(fakeController.resizes.length).toBeGreaterThanOrEqual(2);

    act(() => {
      rendered.root.unmount();
    });

    expect(fakeController.dispose).toHaveBeenCalledTimes(1);
    root = undefined;
  });

  it("advances while running and keeps the image stable while paused", () => {
    const rendered = renderHost();
    root = rendered.root;

    rendered.tick(100);
    expect(fakeController.updates.length).toBeGreaterThan(1);
    const lastRunningProgress =
      fakeController.updates[fakeController.updates.length - 1].ride.progress
        .progress01;
    const lastRunningElapsedMs =
      fakeController.updates[fakeController.updates.length - 1].ride.elapsedMs;

    rendered.rerender("paused");
    rendered.tick(5000);

    expect(
      fakeController.updates[fakeController.updates.length - 1].ride.progress
        .progress01
    ).toBe(lastRunningProgress);
    expect(
      fakeController.updates[fakeController.updates.length - 1].ride.elapsedMs
    ).toBe(lastRunningElapsedMs);

    rendered.rerender("running");
    rendered.tick(100);

    expect(
      fakeController.updates[fakeController.updates.length - 1].ride.elapsedMs
    ).toBe(lastRunningElapsedMs + 100);
  });

  it("shares composed snapshots and applies controlled mock effort", () => {
    const frames: RenderFrameSnapshot[] = [];
    const rendered = renderHost({
      effort01: 0.2,
      onFrame: (snapshot) => frames.push(snapshot)
    });
    root = rendered.root;

    expect(frames).toHaveLength(1);
    expect(frames[0].ride.input.effort01).toBe(0.2);
    expect(fakeController.updates[0]).toBe(frames[0]);

    rendered.rerender("running", { effort01: 0.9 });
    rendered.tick(1000);

    expect(frames[frames.length - 1].ride.input.effort01).toBe(0.9);
    expect(frames[frames.length - 1].route.biomeId).toBe("coast");
  });

  it("preserves a caller-provided mock input source when effort is not controlled", () => {
    const frames: RenderFrameSnapshot[] = [];
    const rendered = renderHost({
      createInputSource: () => createMockRideInputSource(0.2),
      onFrame: (snapshot) => frames.push(snapshot)
    });
    root = rendered.root;

    expect(frames[0].ride.input.effort01).toBe(0.2);

    rendered.tick(100);

    expect(frames[frames.length - 1].ride.input.effort01).toBe(0.2);
  });
});

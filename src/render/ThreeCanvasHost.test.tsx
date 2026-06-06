/* @vitest-environment jsdom */
import { act } from "react";
import { createRoot, type Root } from "react-dom/client";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";

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

  function render(phase: "running" | "paused") {
    root.render(
      <ThreeCanvasHost
        phase={phase}
        controllerFactory={controllerFactory}
        now={getNow}
        scheduleFrame={scheduleFrame}
        cancelFrame={cancelFrame}
        {...props}
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
    rerender(phase: "running" | "paused") {
      act(() => {
        render(phase);
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

    rendered.tick(1000);
    expect(fakeController.updates.length).toBeGreaterThan(1);
    const lastRunningProgress =
      fakeController.updates[fakeController.updates.length - 1].ride.progress
        .progress01;

    rendered.rerender("paused");
    rendered.tick(200);

    expect(
      fakeController.updates[fakeController.updates.length - 1].ride.progress
        .progress01
    ).toBe(lastRunningProgress);
  });
});

/* @vitest-environment jsdom */
import { act } from "react";
import { createRoot, type Root } from "react-dom/client";
import { afterEach, beforeEach, describe, expect, it } from "vitest";

import type { RenderFrameSnapshot } from "../render";
import { RideHud } from "./RideHud";

const SNAPSHOT: RenderFrameSnapshot = {
  ride: {
    source: "mock",
    input: {
      source: "mock",
      effort01: 0.7,
      nowMs: 65_000
    },
    targetSpeedMps: 3,
    speedMps: 3,
    progress: {
      distanceMeters: 1_234,
      progress01: 0.4,
      completed: false
    },
    elapsedMs: 65_000,
    stats: {
      elapsedMs: 65_000,
      distanceMeters: 1_234,
      averageSpeedMps: 2.5
    },
    completed: false
  },
  route: {
    biomeId: "forest"
  } as RenderFrameSnapshot["route"]
};

function renderHud(snapshot?: RenderFrameSnapshot) {
  const container = document.createElement("div");
  document.body.append(container);
  const root = createRoot(container);

  act(() => {
    root.render(<RideHud phase="running" snapshot={snapshot} />);
  });

  return { container, root };
}

describe("RideHud", () => {
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
    root = undefined;
  });

  it("renders coherent zero values before the first snapshot", () => {
    const rendered = renderHud();
    root = rendered.root;

    expect(rendered.container.textContent).toContain("0,0 km/h");
    expect(rendered.container.textContent).toContain("0 m");
    expect(rendered.container.textContent).toContain("0 min 00 s");
    expect(rendered.container.textContent).toContain("Simulation");
    expect(rendered.container.textContent).toContain("Côte");
    expect(rendered.container.textContent).toContain("En selle");
  });

  it("renders speed, distance, time, source, biome and phase from a snapshot", () => {
    const rendered = renderHud(SNAPSHOT);
    root = rendered.root;

    expect(rendered.container.textContent).toContain("10,8 km/h");
    expect(rendered.container.textContent).toContain("1,2 km");
    expect(rendered.container.textContent).toContain("1 min 05 s");
    expect(rendered.container.textContent).toContain("Simulation");
    expect(rendered.container.textContent).toContain("Forêt");
    expect(rendered.container.textContent).toContain("En selle");
  });
});

/* @vitest-environment jsdom */
import { act } from "react";
import { createRoot, type Root } from "react-dom/client";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";

import { SummaryScreen } from "./SummaryScreen";

function renderSummary() {
  const container = document.createElement("div");
  document.body.append(container);
  const root = createRoot(container);
  const onReset = vi.fn();

  act(() => {
    root.render(
      <SummaryScreen
        onReset={onReset}
        summary={{
          elapsedMs: 65_000,
          distanceMeters: 1_234,
          averageSpeedMps: 3
        }}
      />
    );
  });

  return { container, onReset, root };
}

describe("SummaryScreen", () => {
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

  it("renders the final ride stats instead of placeholders", () => {
    const rendered = renderSummary();
    root = rendered.root;

    expect(rendered.container.textContent).not.toContain("Placeholder");
    expect(rendered.container.textContent).toContain("1 min 05 s");
    expect(rendered.container.textContent).toContain("1,2 km");
    expect(rendered.container.textContent).toContain("10,8 km/h");
  });

  it("keeps the reset action", () => {
    const rendered = renderSummary();
    root = rendered.root;
    const button = rendered.container.querySelector("button");

    act(() => {
      button?.click();
    });

    expect(rendered.onReset).toHaveBeenCalledTimes(1);
  });
});

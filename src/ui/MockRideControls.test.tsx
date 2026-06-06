/* @vitest-environment jsdom */
import { act } from "react";
import { createRoot, type Root } from "react-dom/client";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";

import { MockRideControls } from "./MockRideControls";

function renderControls(props: {
  effort01: number;
  onEffortChange: (effort01: number) => void;
  currentSpeedMps?: number;
  targetSpeedMps?: number;
}) {
  const container = document.createElement("div");
  document.body.append(container);
  const root = createRoot(container);

  act(() => {
    root.render(<MockRideControls {...props} />);
  });

  return { container, root };
}

function changeRange(input: HTMLInputElement, value: string) {
  act(() => {
    input.value = value;
    input.dispatchEvent(new Event("input", { bubbles: true }));
  });
}

describe("MockRideControls", () => {
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

  it("renders the controlled effort slider and value", () => {
    const rendered = renderControls({
      effort01: 0.55,
      onEffortChange: vi.fn()
    });
    root = rendered.root;

    const slider = rendered.container.querySelector<HTMLInputElement>(
      "input[type='range']"
    );

    expect(slider).toBeInstanceOf(HTMLInputElement);
    expect(slider?.min).toBe("0");
    expect(slider?.max).toBe("1");
    expect(slider?.step).toBe("0.01");
    expect(rendered.container.textContent).toContain("55 %");
    expect(rendered.container.textContent).toContain("Allure cible");
    expect(rendered.container.textContent).toContain("en attente");
    expect(rendered.container.textContent).toContain("affichée 0,0 km/h");
  });

  it("renders target and displayed speed feedback", () => {
    const rendered = renderControls({
      effort01: 0.6,
      currentSpeedMps: 3,
      targetSpeedMps: 6,
      onEffortChange: vi.fn()
    });
    root = rendered.root;

    expect(rendered.container.textContent).toContain("Allure cible 21,6 km/h");
    expect(rendered.container.textContent).toContain("affichée 10,8 km/h");
  });

  it("emits clamped effort values from the slider", () => {
    const onEffortChange = vi.fn();
    const rendered = renderControls({
      effort01: 0.55,
      onEffortChange
    });
    root = rendered.root;

    const slider = rendered.container.querySelector<HTMLInputElement>(
      "input[type='range']"
    );

    if (!slider) {
      throw new Error("Slider not found");
    }

    changeRange(slider, "0");
    changeRange(slider, "1");
    changeRange(slider, "0.42");

    expect(onEffortChange).toHaveBeenNthCalledWith(1, 0);
    expect(onEffortChange).toHaveBeenNthCalledWith(2, 1);
    expect(onEffortChange).toHaveBeenNthCalledWith(3, 0.42);
  });
});

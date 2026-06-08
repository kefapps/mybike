/* @vitest-environment jsdom */
import { act } from "react";
import { createRoot, type Root } from "react-dom/client";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";

import { App } from "./App";

vi.mock("three", async () => {
  const actual = await vi.importActual<typeof import("three")>("three");

  return {
    ...actual,
    WebGLRenderer: vi.fn(() => {
      return {
        setPixelRatio: vi.fn(),
        setClearColor: vi.fn(),
        setSize: vi.fn(),
        render: vi.fn(),
        dispose: vi.fn(),
        setAnimationLoop: vi.fn()
      } as unknown as import("three").WebGLRenderer;
    })
  };
});

function renderApp() {
  const container = document.createElement("div");
  document.body.append(container);
  const root = createRoot(container);

  act(() => {
    root.render(<App />);
  });

  return { container, root };
}

function clickButton(label: string) {
  const button = Array.from(document.querySelectorAll("button")).find(
    (candidate) => candidate.textContent?.includes(label)
  );

  if (!button) {
    throw new Error(`Button not found: ${label}`);
  }

  act(() => {
    button.click();
  });
}

function changeSlider(value: string) {
  const slider = document.querySelector<HTMLInputElement>("input[type='range']");

  if (!slider) {
    throw new Error("Slider not found");
  }

  act(() => {
    slider.value = value;
    slider.dispatchEvent(new Event("input", { bubbles: true }));
  });
}

describe("App", () => {
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

  it("renders the session flow from start to summary", () => {
    vi.spyOn(HTMLCanvasElement.prototype, "getContext").mockReturnValue(
      {} as RenderingContext
    );
    const rendered = renderApp();
    root = rendered.root;

    expect(rendered.container.textContent).toContain("Echappee 3D");

    clickButton("Lancer");
    expect(rendered.container.textContent).toContain("En selle");
    expect(rendered.container.textContent).toContain("running");
    expect(rendered.container.textContent).toContain("Effort mock");
    expect(rendered.container.textContent).toContain("55 %");
    expect(rendered.container.textContent).toContain("Vitesse");
    expect(rendered.container.textContent).toContain("Distance");
    expect(rendered.container.textContent).toContain("Temps");

    changeSlider("0.9");
    expect(rendered.container.textContent).toContain("90 %");

    clickButton("Pause");
    expect(rendered.container.textContent).toContain("paused");

    clickButton("Reprendre");
    expect(rendered.container.textContent).toContain("running");

    clickButton("Terminer");
    expect(rendered.container.textContent).toContain("Resume de balade");
    expect(rendered.container.textContent).not.toContain("Placeholder MYB-3");
    expect(rendered.container.textContent).toContain("Vitesse moyenne");
  });

  it("renders the WebGL fallback when a context cannot be created", () => {
    vi.spyOn(HTMLCanvasElement.prototype, "getContext").mockReturnValue(null);
    const rendered = renderApp();
    root = rendered.root;

    clickButton("Lancer");

    expect(rendered.container.textContent).toContain(
      "Impossible de lancer la balade"
    );
    expect(rendered.container.textContent).toContain("WebGL");
  });

  it("renders the WebGL fallback when the context is lost during a running ride", () => {
    vi.spyOn(HTMLCanvasElement.prototype, "getContext").mockReturnValue(
      {} as RenderingContext
    );
    const rendered = renderApp();
    root = rendered.root;

    clickButton("Lancer");
    expect(rendered.container.textContent).toContain("running");

    act(() => {
      const canvas = rendered.container.querySelector("canvas");
      if (!canvas) {
        throw new Error("Canvas not found");
      }

      canvas.dispatchEvent(new Event("webglcontextlost"));
    });

    expect(rendered.container.textContent).toContain(
      "Impossible de lancer la balade"
    );
    expect(rendered.container.textContent).toContain("Rendu indisponible");
  });
});

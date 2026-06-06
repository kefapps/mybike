/* @vitest-environment jsdom */
import { act } from "react";
import { createRoot, type Root } from "react-dom/client";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";

import { App } from "./App";

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

    clickButton("Pause");
    expect(rendered.container.textContent).toContain("paused");

    clickButton("Reprendre");
    expect(rendered.container.textContent).toContain("running");

    clickButton("Terminer");
    expect(rendered.container.textContent).toContain("Resume de balade");
    expect(rendered.container.textContent).toContain("Placeholder MYB-3");
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
});

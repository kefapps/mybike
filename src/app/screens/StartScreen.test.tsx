/* @vitest-environment jsdom */
import { act } from "react";
import { createRoot, type Root } from "react-dom/client";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";

import { StartScreen } from "./StartScreen";
import { availableRoutes } from "../../route";
import type { ComponentProps } from "react";

function renderStartScreen(
  props?: Partial<ComponentProps<typeof StartScreen>>
) {
  const container = document.createElement("div");
  document.body.append(container);
  const root = createRoot(container);
  const onStart = vi.fn();
  const onSelectRoute = vi.fn();

  const mergedProps = {
    routes: availableRoutes,
    selectedRouteId: availableRoutes[0].id,
    onSelectRoute,
    onStart,
    ...props
  };

  act(() => {
    root.render(<StartScreen {...mergedProps} />);
  });

  return { container, root, onStart, onSelectRoute };
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

function selectRouteByTitle(container: HTMLElement, title: string) {
  const labels = Array.from(
    container.querySelectorAll<HTMLLabelElement>("label.route-option")
  );
  const target = labels.find((label) =>
    label.textContent?.includes(title)
  );

  if (!target) {
    throw new Error(`Route option not found: ${title}`);
  }

  const input = target.querySelector<HTMLInputElement>("input[type='radio']");

  if (!input) {
    throw new Error(`Route radio input not found: ${title}`);
  }

  act(() => {
    input.click();
  });
}

describe("StartScreen", () => {
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

  it("renders one radio per available route and marks the selected route", () => {
    const rendered = renderStartScreen();
    root = rendered.root;

    const options = rendered.container.querySelectorAll<HTMLLabelElement>(
      "label.route-option"
    );

    expect(options.length).toBe(availableRoutes.length);
    expect(options[0]?.classList.contains("route-option--selected")).toBe(true);
    expect(rendered.container.textContent).toContain("Choisis un parcours");
  });

  it("invokes onSelectRoute when a different route is chosen", () => {
    const rendered = renderStartScreen();
    root = rendered.root;

    selectRouteByTitle(rendered.container, "Montee");
    expect(rendered.onSelectRoute).toHaveBeenCalledWith("echappee-montee");
  });

  it("triggers the start action without forcing a route change", () => {
    const rendered = renderStartScreen();
    root = rendered.root;

    clickButton("Lancer");
    expect(rendered.onStart).toHaveBeenCalledTimes(1);
  });
});

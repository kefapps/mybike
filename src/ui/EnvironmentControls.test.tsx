/* @vitest-environment jsdom */
import { act } from "react";
import { createRoot, type Root } from "react-dom/client";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";

import { EnvironmentControls } from "./EnvironmentControls";

function renderControls(props: {
  presetId: Parameters<typeof EnvironmentControls>[0]["presetId"];
  autoMode: boolean;
  weatherEnabled: boolean;
  onPresetChange: (presetId: Parameters<typeof EnvironmentControls>[0]["presetId"]) => void;
  onAutoModeChange: (autoMode: boolean) => void;
  onWeatherToggleChange: (weatherEnabled: boolean) => void;
}) {
  const container = document.createElement("div");
  document.body.append(container);
  const root = createRoot(container);

  act(() => {
    root.render(<EnvironmentControls {...props} />);
  });

  return { container, root };
}

function toggleCheckbox(input: HTMLInputElement, checked: boolean) {
  act(() => {
    const setter = Object.getOwnPropertyDescriptor(
      HTMLInputElement.prototype,
      "checked"
    )?.set;
    setter?.call(input, checked);
    input.dispatchEvent(new Event("click", { bubbles: true }));
  });
}

describe("EnvironmentControls", () => {
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

  it("lists all environment presets in the select", () => {
    const onPresetChange = vi.fn();
    const onAutoModeChange = vi.fn();
    const onWeatherToggleChange = vi.fn();

    const rendered = renderControls({
      presetId: "morning",
      autoMode: false,
      weatherEnabled: true,
      onPresetChange,
      onAutoModeChange,
      onWeatherToggleChange
    });
    root = rendered.root;

    const select = rendered.container.querySelector<HTMLSelectElement>(
      "select"
    );

    expect(select).toBeInstanceOf(HTMLSelectElement);
    const options = Array.from(select?.options ?? []).map(
      (option) => option.value
    );

    expect(options).toEqual([
      "morning",
      "goldenHour",
      "softNight",
      "lightRain",
      "lowPerformance"
    ]);
  });

  it("emits preset change events from the select", () => {
    const onPresetChange = vi.fn();
    const onAutoModeChange = vi.fn();
    const onWeatherToggleChange = vi.fn();

    const rendered = renderControls({
      presetId: "morning",
      autoMode: false,
      weatherEnabled: true,
      onPresetChange,
      onAutoModeChange,
      onWeatherToggleChange
    });
    root = rendered.root;

    const select = rendered.container.querySelector<HTMLSelectElement>(
      "select"
    );

    if (!select) {
      throw new Error("Select not found");
    }

    act(() => {
      select.value = "softNight";
      select.dispatchEvent(new Event("change", { bubbles: true }));
    });

    expect(onPresetChange).toHaveBeenCalledWith("softNight");
  });

  it("disables the select when auto mode is on", () => {
    const onPresetChange = vi.fn();

    const rendered = renderControls({
      presetId: "morning",
      autoMode: true,
      weatherEnabled: true,
      onPresetChange,
      onAutoModeChange: vi.fn(),
      onWeatherToggleChange: vi.fn()
    });
    root = rendered.root;

    const select = rendered.container.querySelector<HTMLSelectElement>(
      "select"
    );

    expect(select?.disabled).toBe(true);
  });

  it("toggles auto mode and weather via the checkboxes", () => {
    const onAutoModeChange = vi.fn();
    const onWeatherToggleChange = vi.fn();

    const rendered = renderControls({
      presetId: "morning",
      autoMode: false,
      weatherEnabled: false,
      onPresetChange: vi.fn(),
      onAutoModeChange,
      onWeatherToggleChange
    });
    root = rendered.root;

    const checkboxes = rendered.container.querySelectorAll<HTMLInputElement>(
      "input[type='checkbox']"
    );

    expect(checkboxes).toHaveLength(2);

    toggleCheckbox(checkboxes[0], true);
    toggleCheckbox(checkboxes[1], true);

    expect(onAutoModeChange).toHaveBeenCalledWith(true);
    expect(onWeatherToggleChange).toHaveBeenCalledWith(true);
  });

  it("exposes a status line that reflects the active preset or auto cycle", () => {
    const onPresetChange = vi.fn();

    const autoRendered = renderControls({
      presetId: "morning",
      autoMode: true,
      weatherEnabled: true,
      onPresetChange,
      onAutoModeChange: vi.fn(),
      onWeatherToggleChange: vi.fn()
    });
    root = autoRendered.root;
    let status = autoRendered.container.querySelector<HTMLElement>(
      "[data-testid='environment-controls-status']"
    );
    expect(status?.textContent).toContain("Cycle auto");

    act(() => {
      autoRendered.root.unmount();
    });
    root = undefined;

    const manualRendered = renderControls({
      presetId: "softNight",
      autoMode: false,
      weatherEnabled: true,
      onPresetChange,
      onAutoModeChange: vi.fn(),
      onWeatherToggleChange: vi.fn()
    });
    root = manualRendered.root;
    status = manualRendered.container.querySelector<HTMLElement>(
      "[data-testid='environment-controls-status']"
    );
    expect(status?.textContent).toContain("Nuit douce");
    expect(status?.textContent).toContain("selectionne");
  });
});

import { useId } from "react";

import {
  listEnvironmentPresets,
  type EnvironmentPresetId
} from "../render";

export type EnvironmentControlsProps = {
  presetId: EnvironmentPresetId;
  autoMode: boolean;
  weatherEnabled: boolean;
  onPresetChange: (presetId: EnvironmentPresetId) => void;
  onAutoModeChange: (autoMode: boolean) => void;
  onWeatherToggleChange: (weatherEnabled: boolean) => void;
  disabled?: boolean;
};

export function EnvironmentControls({
  presetId,
  autoMode,
  weatherEnabled,
  onPresetChange,
  onAutoModeChange,
  onWeatherToggleChange,
  disabled = false
}: EnvironmentControlsProps) {
  const selectId = useId();
  const presets = listEnvironmentPresets();
  const activePreset = presets.find((entry) => entry.id === presetId);
  const presetDescriptionId = `${selectId}-description`;
  const description = activePreset?.description ?? "";
  const showPresetChoices = !autoMode;

  return (
    <div className="environment-controls" aria-label="Controles d'ambiance">
      <div className="environment-controls__row">
        <label className="environment-controls__label" htmlFor={selectId}>
          Ambiance
        </label>
        <select
          id={selectId}
          className="environment-controls__select"
          aria-describedby={presetDescriptionId}
          disabled={disabled || autoMode}
          value={presetId}
          onChange={(event) =>
            onPresetChange(event.currentTarget.value as EnvironmentPresetId)
          }
        >
          {presets.map((entry) => (
            <option key={entry.id} value={entry.id}>
              {entry.label}
              {entry.quality === "low" ? " (low)" : ""}
            </option>
          ))}
        </select>
      </div>

      <p
        id={presetDescriptionId}
        className="environment-controls__description"
        aria-live="polite"
      >
        {description}
      </p>

      <div className="environment-controls__row environment-controls__row--inline">
        <label className="environment-controls__toggle">
          <input
            type="checkbox"
            checked={autoMode}
            disabled={disabled}
            onChange={(event) => onAutoModeChange(event.currentTarget.checked)}
          />
          <span>Auto</span>
        </label>

        <label className="environment-controls__toggle">
          <input
            type="checkbox"
            checked={weatherEnabled}
            disabled={disabled}
            onChange={(event) =>
              onWeatherToggleChange(event.currentTarget.checked)
            }
          />
          <span>Meteo</span>
        </label>
      </div>

      <p
        className="environment-controls__status"
        aria-live="polite"
        data-testid="environment-controls-status"
      >
        {autoMode
          ? `Cycle auto ${showPresetChoices ? "" : ""}(4 etapes).`
          : `Preset ${activePreset?.label ?? presetId} selectionne.`}
      </p>
    </div>
  );
}

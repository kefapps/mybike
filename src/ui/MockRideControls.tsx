import { useId } from "react";

import { clampEffort01 } from "../ride";
import { COPY } from "./copy";
import { formatEffortPercent, formatSpeed } from "./rideStatsFormat";

type MockRideControlsProps = {
  effort01: number;
  onEffortChange: (effort01: number) => void;
  currentSpeedMps?: number;
  targetSpeedMps?: number;
  disabled?: boolean;
};

export function MockRideControls({
  effort01,
  onEffortChange,
  currentSpeedMps = 0,
  targetSpeedMps,
  disabled = false
}: MockRideControlsProps) {
  const sliderId = useId();
  const safeEffort01 = clampEffort01(effort01);
  const targetSpeedLabel =
    targetSpeedMps === undefined
      ? COPY["mock.feedback.pending"]
      : formatSpeed(targetSpeedMps);

  return (
    <div className="mock-controls">
      <div className="mock-controls__header">
        <label htmlFor={sliderId}>{COPY["mock.slider.label"]}</label>
        <output htmlFor={sliderId}>{formatEffortPercent(safeEffort01)}</output>
      </div>
      <p className="mock-controls__feedback">
        {COPY["mock.feedback.template"]
          .replace("{target}", targetSpeedLabel)
          .replace("{current}", formatSpeed(currentSpeedMps))}
      </p>
      <input
        id={sliderId}
        aria-label={COPY["mock.slider.aria"]}
        type="range"
        min="0"
        max="1"
        step="0.01"
        value={safeEffort01}
        disabled={disabled}
        onInput={(event) =>
          onEffortChange(clampEffort01(event.currentTarget.valueAsNumber))
        }
      />
    </div>
  );
}

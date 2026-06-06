import { useId } from "react";

import { clampEffort01 } from "../ride";
import { formatEffortPercent } from "./rideStatsFormat";

type MockRideControlsProps = {
  effort01: number;
  onEffortChange: (effort01: number) => void;
  disabled?: boolean;
};

export function MockRideControls({
  effort01,
  onEffortChange,
  disabled = false
}: MockRideControlsProps) {
  const sliderId = useId();
  const safeEffort01 = clampEffort01(effort01);

  return (
    <div className="mock-controls">
      <div className="mock-controls__header">
        <label htmlFor={sliderId}>Effort mock</label>
        <output htmlFor={sliderId}>{formatEffortPercent(safeEffort01)}</output>
      </div>
      <input
        id={sliderId}
        aria-label="Effort mock"
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

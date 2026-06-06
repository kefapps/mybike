import { useCallback, useState } from "react";

import { MockRideControls } from "../../ui/MockRideControls";
import { RideHud } from "../../ui/RideHud";
import { EMPTY_RIDE_STATS } from "../../ui/rideStatsFormat";
import type { RidePhase } from "../session/sessionTypes";
import { ThreeCanvasHost, type RenderFrameSnapshot } from "../../render";
import type { RideStats } from "../../ride";

type RideScreenProps = {
  phase: Extract<RidePhase, "running" | "paused">;
  onPause: () => void;
  onResume: () => void;
  onFinish: (summary: RideStats) => void;
};

const DEFAULT_MOCK_EFFORT01 = 0.55;

export function RideScreen({
  phase,
  onPause,
  onResume,
  onFinish
}: RideScreenProps) {
  const isPaused = phase === "paused";
  const [effort01, setEffort01] = useState(DEFAULT_MOCK_EFFORT01);
  const [latestSnapshot, setLatestSnapshot] = useState<RenderFrameSnapshot>();
  const finishWithLatestSummary = useCallback(() => {
    onFinish(latestSnapshot?.ride.stats ?? EMPTY_RIDE_STATS);
  }, [latestSnapshot, onFinish]);

  return (
    <section className="screen ride-screen" aria-labelledby="ride-title">
      <header className="ride-header">
        <div>
          <p className="eyebrow">Balade mock</p>
          <h1 id="ride-title">{isPaused ? "Pause" : "En selle"}</h1>
        </div>
        <span className="phase-pill">{isPaused ? "paused" : "running"}</span>
      </header>

      <div className="ride-viewport" aria-label="Zone ride MVP">
        <ThreeCanvasHost
          phase={phase}
          effort01={effort01}
          onFrame={setLatestSnapshot}
        />
        <RideHud phase={phase} snapshot={latestSnapshot} />
      </div>

      <MockRideControls
        effort01={effort01}
        currentSpeedMps={latestSnapshot?.ride.speedMps}
        targetSpeedMps={latestSnapshot?.ride.targetSpeedMps}
        onEffortChange={setEffort01}
      />

      <div className="action-row">
        {isPaused ? (
          <button className="secondary-action" type="button" onClick={onResume}>
            Reprendre
          </button>
        ) : (
          <button className="secondary-action" type="button" onClick={onPause}>
            Pause
          </button>
        )}
        <button
          className="primary-action"
          type="button"
          onClick={finishWithLatestSummary}
        >
          Terminer
        </button>
      </div>
    </section>
  );
}

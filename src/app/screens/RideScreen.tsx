import { useCallback, useState } from "react";

import { MockRideControls } from "../../ui/MockRideControls";
import { RideHud } from "../../ui/RideHud";
import { COPY, getPhaseLabel } from "../../ui/copy";
import { EMPTY_RIDE_STATS } from "../../ui/rideStatsFormat";
import type { RidePhase } from "../session/sessionTypes";
import { ThreeCanvasHost, type RenderFrameSnapshot } from "../../render";
import type { RideStats } from "../../ride";
import type { RouteDefinition } from "../../route";

type RideScreenProps = {
  phase: Extract<RidePhase, "running" | "paused">;
  route: RouteDefinition;
  onPause: () => void;
  onResume: () => void;
  onFinish: (summary: RideStats) => void;
  onRenderFailure: (message: string) => void;
};

const DEFAULT_MOCK_EFFORT01 = 0.55;

export function RideScreen({
  phase,
  route,
  onPause,
  onResume,
  onFinish,
  onRenderFailure
}: RideScreenProps) {
  const isPaused = phase === "paused";
  const captureCleanHud = isCaptureCleanHudEnabled();
  const [effort01, setEffort01] = useState(DEFAULT_MOCK_EFFORT01);
  const [latestSnapshot, setLatestSnapshot] = useState<RenderFrameSnapshot>();
  const finishWithLatestSummary = useCallback(() => {
    onFinish(latestSnapshot?.ride.stats ?? EMPTY_RIDE_STATS);
  }, [latestSnapshot, onFinish]);

  return (
    <section
      className={`screen ride-screen${captureCleanHud ? " ride-screen--capture-clean" : ""}`}
      aria-labelledby="ride-title"
    >
      <header className="ride-header">
        <div>
          <p className="eyebrow">{COPY["ride.eyebrow"]}</p>
          <h1 id="ride-title">
            {isPaused ? COPY["ride.title.paused"] : COPY["ride.title.running"]}
          </h1>
        </div>
        <span className="phase-pill">{getPhaseLabel(phase)}</span>
      </header>

      <div className="ride-viewport" aria-label={COPY["ride.viewport.label"]}>
        <ThreeCanvasHost
          phase={phase}
          route={route}
          effort01={effort01}
          onFrame={setLatestSnapshot}
          onRenderFailure={onRenderFailure}
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
            {COPY["ride.button.resume"]}
          </button>
        ) : (
          <button className="secondary-action" type="button" onClick={onPause}>
            {COPY["ride.button.pause"]}
          </button>
        )}
        <button
          className="primary-action"
          type="button"
          onClick={finishWithLatestSummary}
        >
          {COPY["ride.button.finish"]}
        </button>
      </div>
    </section>
  );
}

export function isCaptureCleanHudEnabled(
  search = typeof window === "undefined" ? "" : window.location.search
): boolean {
  return new URLSearchParams(search).get("captureHud") === "hidden";
}

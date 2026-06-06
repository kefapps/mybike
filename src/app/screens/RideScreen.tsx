import type { RidePhase } from "../session/sessionTypes";

type RideScreenProps = {
  phase: Extract<RidePhase, "running" | "paused">;
  onPause: () => void;
  onResume: () => void;
  onFinish: () => void;
};

export function RideScreen({
  phase,
  onPause,
  onResume,
  onFinish
}: RideScreenProps) {
  const isPaused = phase === "paused";

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
        <div className="ride-status-mark" aria-hidden="true">
          <span />
          <span />
          <span />
        </div>
      </div>

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
        <button className="primary-action" type="button" onClick={onFinish}>
          Terminer
        </button>
      </div>
    </section>
  );
}

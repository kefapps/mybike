import type { RenderFrameSnapshot } from "../render";
import type { RidePhase } from "../app/session/sessionTypes";
import {
  formatDistance,
  formatDuration,
  formatSpeed,
  getRideStatsOrEmpty
} from "./rideStatsFormat";

type RideHudProps = {
  phase: Extract<RidePhase, "running" | "paused">;
  snapshot?: RenderFrameSnapshot;
};

export function RideHud({ phase, snapshot }: RideHudProps) {
  const stats = getRideStatsOrEmpty(snapshot?.ride.stats);
  const speedMps = snapshot?.ride.speedMps ?? 0;
  const source = snapshot?.ride.source ?? "mock";
  const biomeId = snapshot?.route.biomeId ?? "coast";

  return (
    <aside className="ride-hud" aria-label="HUD de balade">
      <dl className="ride-hud__metrics">
        <div className="ride-hud__metric ride-hud__metric--primary">
          <dt>Vitesse</dt>
          <dd>{formatSpeed(speedMps)}</dd>
        </div>
        <div className="ride-hud__metric ride-hud__metric--primary">
          <dt>Distance</dt>
          <dd>{formatDistance(stats.distanceMeters)}</dd>
        </div>
        <div className="ride-hud__metric ride-hud__metric--primary">
          <dt>Temps</dt>
          <dd>{formatDuration(stats.elapsedMs)}</dd>
        </div>
        <div className="ride-hud__metric ride-hud__metric--meta">
          <dt>Source</dt>
          <dd>{source}</dd>
        </div>
        <div className="ride-hud__metric ride-hud__metric--meta">
          <dt>Ambiance</dt>
          <dd>{biomeId}</dd>
        </div>
        <div className="ride-hud__metric ride-hud__metric--meta">
          <dt>Phase</dt>
          <dd>{phase}</dd>
        </div>
      </dl>
    </aside>
  );
}

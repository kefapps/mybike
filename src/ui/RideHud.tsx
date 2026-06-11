import type { RenderFrameSnapshot } from "../render";
import type { RidePhase } from "../app/session/sessionTypes";
import { COPY, getBiomeLabel, getPhaseLabel, getSourceLabel } from "./copy";
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
    <aside className="ride-hud" aria-label={COPY["hud.label"]}>
      <dl className="ride-hud__metrics">
        <div className="ride-hud__metric ride-hud__metric--primary">
          <dt>{COPY["hud.speed"]}</dt>
          <dd>{formatSpeed(speedMps)}</dd>
        </div>
        <div className="ride-hud__metric ride-hud__metric--primary">
          <dt>{COPY["hud.distance"]}</dt>
          <dd>{formatDistance(stats.distanceMeters)}</dd>
        </div>
        <div className="ride-hud__metric ride-hud__metric--primary">
          <dt>{COPY["hud.time"]}</dt>
          <dd>{formatDuration(stats.elapsedMs)}</dd>
        </div>
        <div className="ride-hud__metric ride-hud__metric--meta">
          <dt>{COPY["hud.source"]}</dt>
          <dd>{getSourceLabel(source)}</dd>
        </div>
        <div className="ride-hud__metric ride-hud__metric--meta">
          <dt>{COPY["hud.biome"]}</dt>
          <dd>{getBiomeLabel(biomeId)}</dd>
        </div>
        <div className="ride-hud__metric ride-hud__metric--meta">
          <dt>{COPY["hud.phase"]}</dt>
          <dd>{getPhaseLabel(phase)}</dd>
        </div>
      </dl>
    </aside>
  );
}

import type { RideStats } from "../../ride";
import {
  formatDistance,
  formatDuration,
  formatSpeed,
  getRideStatsOrEmpty
} from "../../ui/rideStatsFormat";

type SummaryScreenProps = {
  summary?: RideStats;
  onReset: () => void;
};

export function SummaryScreen({ summary, onReset }: SummaryScreenProps) {
  const stats = getRideStatsOrEmpty(summary);

  return (
    <section className="screen summary-screen" aria-labelledby="summary-title">
      <div className="screen-copy">
        <p className="eyebrow">Session terminee</p>
        <h1 id="summary-title">Resume de balade</h1>
      </div>

      <dl className="summary-grid">
        <div>
          <dt>Duree</dt>
          <dd>{formatDuration(stats.elapsedMs)}</dd>
        </div>
        <div>
          <dt>Distance</dt>
          <dd>{formatDistance(stats.distanceMeters)}</dd>
        </div>
        <div>
          <dt>Vitesse moyenne</dt>
          <dd>{formatSpeed(stats.averageSpeedMps)}</dd>
        </div>
      </dl>

      <button className="primary-action" type="button" onClick={onReset}>
        Retour au depart
      </button>
    </section>
  );
}

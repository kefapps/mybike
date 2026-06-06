const PLACEHOLDER_SUMMARY = {
  duration: "Placeholder MYB-3: 00 min 00 s",
  distance: "Placeholder MYB-3: 0,0 km",
  averageSpeed: "Placeholder MYB-3: 0,0 km/h"
};

type SummaryScreenProps = {
  onReset: () => void;
};

export function SummaryScreen({ onReset }: SummaryScreenProps) {
  return (
    <section className="screen summary-screen" aria-labelledby="summary-title">
      <div className="screen-copy">
        <p className="eyebrow">Session terminee</p>
        <h1 id="summary-title">Resume de balade</h1>
      </div>

      <dl className="summary-grid">
        <div>
          <dt>Duree</dt>
          <dd>{PLACEHOLDER_SUMMARY.duration}</dd>
        </div>
        <div>
          <dt>Distance</dt>
          <dd>{PLACEHOLDER_SUMMARY.distance}</dd>
        </div>
        <div>
          <dt>Vitesse moyenne</dt>
          <dd>{PLACEHOLDER_SUMMARY.averageSpeed}</dd>
        </div>
      </dl>

      <button className="primary-action" type="button" onClick={onReset}>
        Retour au depart
      </button>
    </section>
  );
}

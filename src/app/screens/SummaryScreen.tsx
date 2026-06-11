import type { Challenge, ChallengeResult } from "../../challenges";
import { describeChallengeResult, summarizeReward } from "../../challenges";
import type { RideStats } from "../../ride";
import { COPY } from "../../ui/copy";
import {
  formatDistance,
  formatDuration,
  formatSpeed,
  getRideStatsOrEmpty
} from "../../ui/rideStatsFormat";

type SummaryScreenProps = {
  summary?: RideStats;
  activeChallenge?: Challenge;
  challengeResult?: ChallengeResult;
  onReset: () => void;
};

export function SummaryScreen({
  summary,
  activeChallenge,
  challengeResult,
  onReset
}: SummaryScreenProps) {
  const stats = getRideStatsOrEmpty(summary);
  const showChallenge = Boolean(activeChallenge && challengeResult);
  const result = challengeResult;
  const challenge = activeChallenge;
  const resultClass = result
    ? result.success
      ? "challenge-summary challenge-summary--success"
      : "challenge-summary challenge-summary--failure"
    : null;

  return (
    <section className="screen summary-screen" aria-labelledby="summary-title">
      <div className="screen-copy">
        <p className="eyebrow">{COPY["summary.eyebrow"]}</p>
        <h1 id="summary-title">{COPY["summary.title"]}</h1>
      </div>

      {showChallenge && challenge && result && resultClass ? (
        <article className={resultClass} aria-labelledby="challenge-result-title">
          <p className="eyebrow">Defi du jour</p>
          <h2 id="challenge-result-title" className="challenge-summary__title">
            {challenge.title}
          </h2>
          <p
            className={`challenge-summary__status${
              result.success ? " challenge-summary__status--success" : ""
            }`}
          >
            {result.success ? "Reussi" : "Rate"} · {describeChallengeResult(result.reason)}
          </p>
          <p className="challenge-summary__reward">
            Recompense : {summarizeReward(result.reward)}
          </p>
        </article>
      ) : null}

      <dl className="summary-grid">
        <div>
          <dt>{COPY["summary.duration"]}</dt>
          <dd>{formatDuration(stats.elapsedMs)}</dd>
        </div>
        <div>
          <dt>{COPY["summary.distance"]}</dt>
          <dd>{formatDistance(stats.distanceMeters)}</dd>
        </div>
        <div>
          <dt>{COPY["summary.speed"]}</dt>
          <dd>{formatSpeed(stats.averageSpeedMps)}</dd>
        </div>
      </dl>

      <button className="primary-action" type="button" onClick={onReset}>
        {COPY["summary.button"]}
      </button>
    </section>
  );
}

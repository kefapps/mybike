/* @vitest-environment jsdom */
import { act } from "react";
import { createRoot, type Root } from "react-dom/client";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";

import type { Challenge, ChallengeResult } from "../../challenges";
import { SummaryScreen } from "./SummaryScreen";

function renderSummary(props: {
  activeChallenge?: Challenge;
  challengeResult?: ChallengeResult;
  summary?: {
    elapsedMs: number;
    distanceMeters: number;
    averageSpeedMps: number;
  };
} = {}) {
  const container = document.createElement("div");
  document.body.append(container);
  const root = createRoot(container);
  const onReset = vi.fn();

  act(() => {
    root.render(
      <SummaryScreen
        onReset={onReset}
        summary={props.summary ?? { elapsedMs: 0, distanceMeters: 0, averageSpeedMps: 0 }}
        activeChallenge={props.activeChallenge}
        challengeResult={props.challengeResult}
      />
    );
  });

  return { container, onReset, root };
}

const SAMPLE_CHALLENGE: Challenge = {
  id: "daily-test",
  kind: "daily",
  title: "Distance du jour",
  description: "Parcours au moins 800 m.",
  target: { kind: "distance", meters: 800 },
  reward: { xp: 60, badgeId: null },
  seed: 1,
  issuedAtMs: 0,
  expiresAtMs: 1
};

describe("SummaryScreen", () => {
  let root: Root | undefined;

  beforeEach(() => {
    Object.assign(globalThis, { IS_REACT_ACT_ENVIRONMENT: true });
  });

  afterEach(() => {
    if (root) {
      act(() => {
        root?.unmount();
      });
    }

    document.body.replaceChildren();
    vi.restoreAllMocks();
    root = undefined;
  });

  it("renders the final ride stats instead of placeholders", () => {
    const rendered = renderSummary({
      summary: { elapsedMs: 65_000, distanceMeters: 1_234, averageSpeedMps: 3 }
    });
    root = rendered.root;

    expect(rendered.container.textContent).not.toContain("Placeholder");
    expect(rendered.container.textContent).toContain("1 min 05 s");
    expect(rendered.container.textContent).toContain("1,2 km");
    expect(rendered.container.textContent).toContain("10,8 km/h");
  });

  it("keeps the reset action", () => {
    const rendered = renderSummary();
    root = rendered.root;
    const button = rendered.container.querySelector("button");

    act(() => {
      button?.click();
    });

    expect(rendered.onReset).toHaveBeenCalledTimes(1);
  });

  it("renders a successful challenge block when the daily challenge is met", () => {
    const rendered = renderSummary({
      activeChallenge: SAMPLE_CHALLENGE,
      challengeResult: {
        challengeId: SAMPLE_CHALLENGE.id,
        success: true,
        reason: "completed",
        finalProgress: {
          challengeId: SAMPLE_CHALLENGE.id,
          meters: 900,
          seconds: 80,
          progress01: 1,
          effortAboveThresholdSeconds: 0,
          effortAboveThreshold: false,
          averageSpeedMps: 11,
          completion01: 1
        },
        reward: SAMPLE_CHALLENGE.reward
      }
    });
    root = rendered.root;

    expect(rendered.container.textContent).toContain("Distance du jour");
    expect(rendered.container.textContent).toContain("Reussi");
    expect(rendered.container.textContent).toContain("+60 XP");
  });

  it("renders a failed challenge block when the daily challenge is missed", () => {
    const rendered = renderSummary({
      activeChallenge: SAMPLE_CHALLENGE,
      challengeResult: {
        challengeId: SAMPLE_CHALLENGE.id,
        success: false,
        reason: "missed-distance",
        finalProgress: {
          challengeId: SAMPLE_CHALLENGE.id,
          meters: 200,
          seconds: 80,
          progress01: 0.25,
          effortAboveThresholdSeconds: 0,
          effortAboveThreshold: false,
          averageSpeedMps: 2.5,
          completion01: 0.25
        },
        reward: { xp: 0, badgeId: null }
      }
    });
    root = rendered.root;

    expect(rendered.container.textContent).toContain("Rate");
    expect(rendered.container.textContent).toContain("Distance insuffisante");
    expect(rendered.container.textContent).toContain("Pas de recompense");
  });
});

import type { Challenge, ChallengeResult } from "../../challenges";
import type { RideStats } from "../../ride";
import type { RidePreferences } from "../preferences";

export type RidePhase = "idle" | "running" | "paused" | "finished" | "error";

export type SessionState = {
  phase: RidePhase;
  selectedRouteId?: string;
  startedAt?: number;
  finishedAt?: number;
  summary?: RideStats;
  errorMessage?: string;
  activeChallenge?: Challenge;
  challengeResult?: ChallengeResult;
  preferences?: RidePreferences;
};

export type SessionAction =
  | { type: "selectRoute"; routeId: string }
  | { type: "start"; now: number; challenge?: Challenge }
  | { type: "pause" }
  | { type: "resume" }
  | { type: "finish"; now: number; summary?: RideStats; challengeResult?: ChallengeResult }
  | { type: "fail"; message: string }
  | { type: "reset" }
  | { type: "setZenMode"; zenMode: boolean };

export type RidePhase = "idle" | "running" | "paused" | "finished" | "error";

export type SessionState = {
  phase: RidePhase;
  startedAt?: number;
  finishedAt?: number;
  errorMessage?: string;
};

export type SessionAction =
  | { type: "start"; now: number }
  | { type: "pause" }
  | { type: "resume" }
  | { type: "finish"; now: number }
  | { type: "fail"; message: string }
  | { type: "reset" };

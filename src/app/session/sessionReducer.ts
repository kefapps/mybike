import { createInitialRidePreferences, withZenMode } from "../preferences";
import type { SessionAction, SessionState } from "./sessionTypes";

export function createInitialSessionState(
  preferences = createInitialRidePreferences()
): SessionState {
  return { phase: "idle", preferences };
}

export function sessionReducer(
  state: SessionState,
  action: SessionAction
): SessionState {
  switch (action.type) {
    case "start":
      if (state.phase !== "idle") {
        return state;
      }

      return {
        ...state,
        phase: "running",
        startedAt: action.now
      };

    case "pause":
      if (state.phase !== "running") {
        return state;
      }

      return {
        ...state,
        phase: "paused"
      };

    case "resume":
      if (state.phase !== "paused") {
        return state;
      }

      return {
        ...state,
        phase: "running"
      };

    case "finish":
      if (state.phase !== "running" && state.phase !== "paused") {
        return state;
      }

      return {
        ...state,
        phase: "finished",
        finishedAt: action.now,
        summary: action.summary,
        errorMessage: undefined
      };

    case "fail":
      if (state.phase === "finished") {
        return state;
      }

      return {
        ...state,
        phase: "error",
        errorMessage: action.message
      };

    case "setZenMode":
      return {
        ...state,
        preferences: withZenMode(state.preferences ?? { zenMode: false }, action.zenMode === true)
      };

    case "reset":
      return createInitialSessionState(state.preferences);

    default:
      return state;
  }
}

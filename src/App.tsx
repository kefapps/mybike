import { useReducer } from "react";

import { RideScreen } from "./app/screens/RideScreen";
import { StartScreen } from "./app/screens/StartScreen";
import { SummaryScreen } from "./app/screens/SummaryScreen";
import { WebGlFallback } from "./app/screens/WebGlFallback";
import {
  createInitialSessionState,
  sessionReducer
} from "./app/session/sessionReducer";
import { isWebGlAvailable } from "./app/webgl/webglSupport";
import type { RideStats } from "./ride";

export function App() {
  const [session, dispatch] = useReducer(
    sessionReducer,
    createInitialSessionState()
  );

  const startRide = () => {
    if (!isWebGlAvailable()) {
      dispatch({
        type: "fail",
        message:
          "WebGL n'est pas disponible. La balade mock ne peut pas demarrer dans ce navigateur."
      });
      return;
    }

    dispatch({ type: "start", now: Date.now() });
  };

  const finishRide = (summary: RideStats) => {
    dispatch({ type: "finish", now: Date.now(), summary });
  };

  return (
    <main className="app-shell">
      {session.phase === "idle" ? <StartScreen onStart={startRide} /> : null}

      {session.phase === "running" || session.phase === "paused" ? (
        <RideScreen
          phase={session.phase}
          onPause={() => dispatch({ type: "pause" })}
          onResume={() => dispatch({ type: "resume" })}
          onFinish={finishRide}
        />
      ) : null}

      {session.phase === "finished" ? (
        <SummaryScreen
          summary={session.summary}
          onReset={() => dispatch({ type: "reset" })}
        />
      ) : null}

      {session.phase === "error" ? (
        <WebGlFallback
          message={session.errorMessage}
          onReset={() => dispatch({ type: "reset" })}
        />
      ) : null}
    </main>
  );
}

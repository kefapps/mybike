import { lazy, Suspense, useReducer } from "react";

import { StartScreen } from "./app/screens/StartScreen";
import { SummaryScreen } from "./app/screens/SummaryScreen";
import { WebGlFallback } from "./app/screens/WebGlFallback";
import {
  createInitialSessionState,
  sessionReducer
} from "./app/session/sessionReducer";
import { isWebGlAvailable } from "./app/webgl/webglSupport";
import type { RideStats } from "./ride";
import {
  availableRoutes,
  DEFAULT_ROUTE_ID,
  getRouteDefinitionById,
  getRouteEntryById
} from "./route";

const LazyRideScreen = lazy(() =>
  import("./app/screens/RideScreen").then((module) => ({
    default: module.RideScreen
  }))
);

const rideLoadingFallback = <div className="screen ride-screen">Chargement du rendu 3D…</div>;

export function App() {
  const [session, dispatch] = useReducer(
    sessionReducer,
    createInitialSessionState()
  );
  const selectedRouteId = session.selectedRouteId ?? DEFAULT_ROUTE_ID;
  const selectedRouteEntry = getRouteEntryById(selectedRouteId) ?? availableRoutes[0];
  const selectedRouteDefinition =
    getRouteDefinitionById(selectedRouteId) ?? selectedRouteEntry.definition;

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

  const handleRenderFailure = (message: string) => {
    dispatch({ type: "fail", message });
  };

  const handleSelectRoute = (routeId: string) => {
    dispatch({ type: "selectRoute", routeId });
  };

  return (
    <main className="app-shell">
      {session.phase === "idle" ? (
        <StartScreen
          routes={availableRoutes}
          selectedRouteId={selectedRouteEntry.id}
          onSelectRoute={handleSelectRoute}
          onStart={startRide}
        />
      ) : null}

      {session.phase === "running" || session.phase === "paused" ? (
        <Suspense fallback={rideLoadingFallback}>
          <LazyRideScreen
            phase={session.phase}
            route={selectedRouteDefinition}
            onPause={() => dispatch({ type: "pause" })}
            onResume={() => dispatch({ type: "resume" })}
            onFinish={finishRide}
            onRenderFailure={handleRenderFailure}
          />
        </Suspense>
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

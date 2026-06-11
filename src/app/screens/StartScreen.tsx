import { useId } from "react";

import type { RouteCatalogEntry } from "../../route";

type StartScreenProps = {
  routes: ReadonlyArray<RouteCatalogEntry>;
  selectedRouteId: string;
  onSelectRoute: (routeId: string) => void;
  onStart: () => void;
};

export function StartScreen({
  routes,
  selectedRouteId,
  onSelectRoute,
  onStart
}: StartScreenProps) {
  const legendId = useId();
  const groupId = useId();
  const safeSelectedRouteId = routes.some((route) => route.id === selectedRouteId)
    ? selectedRouteId
    : routes[0]?.id ?? "";

  return (
    <section className="screen start-screen" aria-labelledby="start-title">
      <div className="screen-copy">
        <p className="eyebrow">Vertical slice mock</p>
        <h1 id="start-title">Echappee 3D</h1>
        <p className="lead">Premiere sortie mock, cap clair, rythme doux.</p>
      </div>

      <fieldset
        className="route-selector"
        aria-labelledby={legendId}
        id={groupId}
      >
        <legend id={legendId} className="route-selector__legend">
          Choisis un parcours
        </legend>
        <p className="route-selector__hint">
          Trois parcours configurables. La distance totale vient du fichier route.
        </p>
        <div className="route-selector__options" role="radiogroup" aria-labelledby={legendId}>
          {routes.map((route) => {
            const inputId = `${groupId}-${route.id}`;
            const isSelected = route.id === safeSelectedRouteId;

            return (
              <label
                key={route.id}
                className={`route-option${isSelected ? " route-option--selected" : ""}`}
                htmlFor={inputId}
              >
                <input
                  id={inputId}
                  className="route-option__input"
                  type="radio"
                  name={groupId}
                  value={route.id}
                  checked={isSelected}
                  onChange={() => onSelectRoute(route.id)}
                />
                <span className="route-option__body">
                  <span className="route-option__title">{route.displayName}</span>
                  <span className="route-option__profile">{route.profile}</span>
                  <span className="route-option__description">{route.description}</span>
                  <span className="route-option__length">
                    {formatRouteLength(route.definition.lengthMeters)}
                  </span>
                </span>
              </label>
            );
          })}
        </div>
      </fieldset>

      <button
        className="primary-action"
        type="button"
        onClick={onStart}
        disabled={safeSelectedRouteId.length === 0}
      >
        Lancer la balade mock
      </button>
    </section>
  );
}

function formatRouteLength(lengthMeters: number): string {
  if (!Number.isFinite(lengthMeters) || lengthMeters <= 0) {
    return "Longueur inconnue";
  }

  if (lengthMeters >= 1000) {
    const kilometers = lengthMeters / 1000;
    return `${formatDecimal(kilometers)} km`;
  }

  return `${Math.round(lengthMeters)} m`;
}

function formatDecimal(value: number): string {
  return value.toFixed(1).replace(/\.0$/u, "");
}

/**
 * Microcopy utilisateur MyBike — IDs stables pour future i18n.
 * Ton : direct, chaleureux, motivant, pas coach toxique.
 */

import type { RidePhase } from "../app/session/sessionTypes";
import type { RideInputSourceKind } from "../ride";

/* ------------------------------------------------------------------ */
/*  Clés statiques                                                   */
/* ------------------------------------------------------------------ */

export const COPY = Object.freeze({
  /* Start */
  "start.eyebrow": "Démonstration",
  "start.title": "Échappée 3D",
  "start.lead":
    "Première sortie en simulation — objectif clair, rythme doux.",
  "start.button": "Démarrer",

  /* Ride */
  "ride.eyebrow": "Balade en cours",
  "ride.title.running": "En selle",
  "ride.title.paused": "En pause",
  "ride.button.resume": "Reprendre",
  "ride.button.pause": "Pause",
  "ride.button.finish": "Terminer",
  "ride.viewport.label": "Zone de vue de la balade",

  /* HUD */
  "hud.label": "Données de balade",
  "hud.speed": "Vitesse",
  "hud.distance": "Distance",
  "hud.time": "Temps",
  "hud.source": "Source",
  "hud.biome": "Ambiance",
  "hud.phase": "Phase",

  /* Mock controls */
  "mock.slider.label": "Effort",
  "mock.slider.aria": "Effort simulé",
  "mock.feedback.pending": "en attente",
  "mock.feedback.template":
    "Allure cible {target} \u00b7 affichée {current}",

  /* Summary */
  "summary.eyebrow": "Session terminée",
  "summary.title": "Résumé de balade",
  "summary.duration": "Durée",
  "summary.distance": "Distance",
  "summary.speed": "Vitesse moyenne",
  "summary.button": "Retour au départ",

  /* WebGL errors */
  "error.webgl.eyebrow": "Rendu indisponible",
  "error.webgl.title": "Impossible de lancer la balade",
  "error.webgl.default":
    "Le navigateur ne fournit pas le support WebGL requis pour afficher l\u2019expérience.",
  "error.webgl.loading": "Chargement du rendu 3D\u2026",
  "error.webgl.startup":
    "WebGL n\u2019est pas disponible. La balade en simulation ne peut pas démarrer dans ce navigateur.",
  "error.render.create": "La création du rendu 3D a échoué.",
  "error.render.notFound":
    "Rendu WebGL introuvable ou indisponible. Vérifiez que votre navigateur prend en charge WebGL.",
  "error.render.contextLost": "Le contexte WebGL a été perdu.",

  /* Canvas */
  "canvas.label": "Rendu 3D de la balade",

  /* Format units */
  "unit.min": "min",
  "unit.s": "s",
  "unit.m": "m",
  "unit.km": "km",
  "unit.kmh": "km/h",
  "unit.percent": "%",
});

/* ------------------------------------------------------------------ */
/*  Labels dynamiques : phase, source, biome                         */
/* ------------------------------------------------------------------ */

const PHASE_LABELS: Record<RidePhase, string> = Object.freeze({
  idle: "Prêt",
  running: "En selle",
  paused: "En pause",
  finished: "Terminé",
  error: "Erreur",
});

const BIOME_LABELS: Record<string, string> = Object.freeze({
  coast: "Côte",
  forest: "Forêt",
  placeholder: "\u2014",
});

const SOURCE_LABELS: Record<RideInputSourceKind, string> = Object.freeze({
  mock: "Simulation",
});

export function getPhaseLabel(phase: RidePhase): string {
  return PHASE_LABELS[phase] ?? phase;
}

export function getBiomeLabel(biomeId: string): string {
  return BIOME_LABELS[biomeId] ?? biomeId;
}

export function getSourceLabel(source: RideInputSourceKind): string {
  return SOURCE_LABELS[source] ?? source;
}

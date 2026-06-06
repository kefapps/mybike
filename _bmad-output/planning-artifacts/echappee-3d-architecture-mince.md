---
title: "Architecture mince - Echappee 3D"
project: "mybike"
date: "2026-06-06"
source: "_bmad-output/planning-artifacts/echappee-3d-gdd-court.md"
status: "draft"
scope: "vertical slice mock MVP"
---

# Architecture mince - Echappee 3D

## Intention

Cette architecture cible uniquement la vertical slice mock d'Echappee 3D: une balade 3D web en premiere personne sur rail, jouable sans velo connecte, avec entree mock, progression lissee, scene Three.js simple, HUD minimal et resume de session.

Flux principal verrouille:

```text
MockRideInputSource -> speed mapping/smoothing -> RouteProgress -> cameraOnRail -> SceneController -> Hud/Summary
```

La structure doit garder trois frontieres nettes:

- React gere l'interface, les etats d'ecran et le montage du canvas.
- La logique ride reste testable hors React et hors Three.js.
- Three.js gere seulement le rendu, a partir d'un snapshot deja calcule.

## Décisions techniques verrouillées

- Stack MVP: Vite + React + TypeScript + Three.js.
- Rendu Three.js direct pour le MVP: React monte le canvas, mais ne pilote pas les objets 3D a chaque composant UI.
- Pas de backend, pas de compte utilisateur, pas de synchronisation cloud.
- Pas de BLE, Web Bluetooth, FTMS ou parser Indoor Bike Data dans le MVP.
- Le mode mock est une source produit MVP, pas seulement un outil de debug.
- La source d'entree MVP unique est `mock`, pilotable par slider et/ou clavier.
- Le joueur ne dirige pas lateralement le velo; la camera suit un rail.
- La vitesse visuelle est derivee d'une entree mock puis lissee avant d'avancer la route.
- La progression de route est normalisee de `0` a `1`, avec distance en metres derivee de la route.
- La camera est calculee par une fonction pure `cameraOnRail`, sans dependance a `THREE.Vector3`.
- Les fonctions critiques a tester en priorite: mapping vitesse, smoothing, progression, stats, selection biome, camera rail.
- Les assets externes ne sont pas necessaires au MVP; chaque element visuel doit pouvoir tomber sur un placeholder procedural.
- Les donnees de session sont en memoire pendant la balade; le resume n'est pas persiste.

## Modules MVP

### `app`

Responsabilite: assembler l'application Vite/React, choisir l'ecran actif et brancher les providers legers si necessaire.

Elements attendus:

- `App`
- `StartScreen`
- `RideScreen`
- `SummaryScreen`
- `WebGlFallback`

### `ui`

Responsabilite: afficher et modifier l'etat ride sans connaitre Three.js.

Elements attendus:

- `MockRideControls`: slider d'effort/vitesse mock et raccourcis clavier optionnels.
- `RideHud`: vitesse simulee, distance, temps, source `mock`, etat pause/running.
- `PauseOverlay`: pause, reprise, fin.
- `RideSummary`: duree, distance, vitesse moyenne.

### `ride`

Responsabilite: logique pure de ride, machine d'etat et calculs testables.

Elements attendus:

- `MockRideInputSource`
- `mapMockInputToSpeed`
- `smoothSpeed`
- `advanceRideFrame`
- `calculateRideStats`
- `rideReducer` ou equivalent simple pour `idle/running/paused/finished/error`

Ce module ne doit importer ni React, ni Three.js.

### `route`

Responsabilite: definition de route MVP, sampling, progression, camera et biomes.

Elements attendus:

- `mockRouteDefinition`: route courte prefabriquee.
- `advanceRouteProgress`
- `sampleRouteAt`
- `cameraOnRail`
- `selectBiomeAtProgress`

Ce module manipule des types numeriques simples (`Vec3`, metres, secondes, progression normalisee), pas des classes Three.js.

### `render`

Responsabilite: cycle de vie Three.js et rendu de la scene a partir d'un snapshot.

Elements attendus:

- `ThreeCanvasHost`: composant React mince qui monte/demonte le canvas.
- `SceneController`: create, update, resize, dispose.
- `createRouteMesh`
- `createBiomeVisuals`
- `createLighting`
- `createPlaceholderAssets`

Ce module peut importer Three.js. Il ne doit pas contenir la logique de progression, de vitesse ou de stats.

### `test`

Responsabilite: garantir la boucle mock avec un effort de test proportionne au MVP.

Priorite unit tests:

- `mapMockInputToSpeed`: clamp, bornes min/max, source `mock`.
- `smoothSpeed`: montee/descente progressive, pas de saut brutal, stabilite avec `dt` variable.
- `advanceRouteProgress`: integration distance/vitesse, clamp fin de route, completion.
- `cameraOnRail`: position et look-ahead deterministes, absence de valeurs invalides.
- `selectBiomeAtProgress`: transition au bon seuil.
- `calculateRideStats`: duree, distance, moyenne.
- `rideReducer`: transitions start, pause, resume, finish.

Playwright:

- Un seul happy path mock: ouvrir l'app, lancer une balade, modifier l'entree mock, voir HUD progresser, pause/reprise, finir, voir le resume.

## Contrats minimum

Ces contrats sont volontairement petits. Ils fixent les frontieres; l'implementation peut les enrichir seulement si une story MVP en a besoin.

```ts
export type RidePhase = "idle" | "running" | "paused" | "finished" | "error";

export type RideInputSourceKind = "mock";

export interface Vec3 {
  x: number;
  y: number;
  z: number;
}

export interface RideInputSample {
  source: RideInputSourceKind;
  effort01: number;
  timestampMs: number;
}

export interface RideInputSource {
  kind: RideInputSourceKind;
  read(nowMs: number): RideInputSample;
}

export interface MockRideInputSource extends RideInputSource {
  kind: "mock";
  setEffort01(value: number): void;
}

export interface SpeedMappingConfig {
  minSpeedMps: number;
  maxSpeedMps: number;
}

export interface SpeedSmoothingConfig {
  accelerationPerSecond: number;
  decelerationPerSecond: number;
}

export interface SmoothedSpeedState {
  speedMps: number;
}

export interface RoutePoint {
  position: Vec3;
}

export interface BiomeSegment {
  id: string;
  fromProgress01: number;
  toProgress01: number;
}

export interface RouteDefinition {
  id: string;
  lengthMeters: number;
  points: RoutePoint[];
  biomes: BiomeSegment[];
}

export interface RouteProgress {
  progress01: number;
  distanceMeters: number;
  completed: boolean;
}

export interface CameraRailConfig {
  heightMeters: number;
  lookAheadMeters: number;
  fovDegrees: number;
}

export interface CameraRigSnapshot {
  position: Vec3;
  lookAt: Vec3;
  fovDegrees: number;
}

export interface RideFrameSnapshot {
  phase: RidePhase;
  source: RideInputSourceKind;
  elapsedMs: number;
  input: RideInputSample;
  speedMps: number;
  route: RouteProgress;
  camera: CameraRigSnapshot;
  biomeId: string;
}

export interface RideSummary {
  elapsedMs: number;
  distanceMeters: number;
  averageSpeedMps: number;
}

export interface SceneController {
  mount(target: HTMLCanvasElement): void;
  resize(width: number, height: number): void;
  update(snapshot: RideFrameSnapshot): void;
  dispose(): void;
}
```

Fonctions minimum attendues:

```ts
export function mapMockInputToSpeed(
  sample: RideInputSample,
  config: SpeedMappingConfig
): number;

export function smoothSpeed(
  targetSpeedMps: number,
  previous: SmoothedSpeedState,
  dtSeconds: number,
  config: SpeedSmoothingConfig
): SmoothedSpeedState;

export function advanceRouteProgress(
  previous: RouteProgress,
  speedMps: number,
  dtSeconds: number,
  route: RouteDefinition
): RouteProgress;

export function cameraOnRail(
  route: RouteDefinition,
  progress: RouteProgress,
  config: CameraRailConfig
): CameraRigSnapshot;

export function selectBiomeAtProgress(
  route: RouteDefinition,
  progress01: number
): string;

export function calculateRideStats(
  elapsedMs: number,
  distanceMeters: number
): RideSummary;
```

## Flux de données MVP

1. `MockRideControls` met a jour `MockRideInputSource`.
2. La boucle ride lit `RideInputSample` a chaque frame active.
3. `mapMockInputToSpeed` convertit `effort01` en vitesse cible.
4. `smoothSpeed` produit une vitesse visuelle stable.
5. `advanceRouteProgress` avance la distance et `progress01`.
6. `cameraOnRail` calcule position, look-at et FOV.
7. `selectBiomeAtProgress` choisit l'ambiance active.
8. `SceneController.update` applique route, camera, lumiere, fog et placeholders.
9. `RideHud` lit le snapshot courant; `RideSummary` lit le resume final.

## Hors scope technique

- Unity, Unreal, Godot ou autre moteur non web.
- Backend, API serveur, base de donnees, auth, compte utilisateur.
- BLE, Web Bluetooth, FTMS, Indoor Bike Data, resistance controlee.
- Persistance d'historique de sessions.
- Import GPX ou editeur de route.
- Pipeline Meshy obligatoire, generation d'assets IA obligatoire, asset manager lourd.
- Multijoueur, classements, ghosts, scoring competitif.
- VR, mobile natif, packaging desktop.
- Simulation physique velo realiste, collisions gameplay, steering lateral.
- Systemes de telemetry reelle ou calibration materiel.
- Suite Playwright exhaustive; seulement le happy path mock.

## Risques / mitigations

- Camera inconfortable: garder un rail prefabrique doux, look-ahead fixe, smoothing vitesse, tests unitaires sur `cameraOnRail` et validation visuelle du happy path.
- Couplage React/Three.js: limiter React au host canvas et a l'UI; isoler le cycle Three.js dans `SceneController`.
- Derive de scope vers BLE/assets/backlog Linear: conserver les contrats d'extension, mais ne pas implementer les sources ou pipelines post-MVP.
- Scene trop vide: livrer deux biomes placeholders avec lumiere/fog/couleur/terrain simples avant tout asset externe.
- Performance instable: geometrie simple, peu de meshes, disposal explicite, resize controle, pas de chargement asset lourd.
- Stats incoherentes: calculer distance, duree et moyenne depuis les fonctions pures de ride, pas depuis le rendu.
- Route/camera difficiles a tester si basees sur Three.js: conserver `Vec3` et les calculs route en TypeScript pur, adapter vers `THREE.Vector3` seulement dans `render`.

## Points d'extension post-MVP

- Nouvelle source `bleFtms` derriere `RideInputSource`, sans changer `RouteProgress`.
- Routes multiples via `RouteDefinition[]`, sans editeur au depart.
- Biomes plus riches via factories visuelles, sans changer `selectBiomeAtProgress`.
- Assets externes optionnels derriere `createPlaceholderAssets`.
- Historique local ou cloud seulement apres validation de la boucle mock.
- Smoothing telemetrie reelle separe du smoothing mock.

## Handoff pour gds-create-epics-and-stories

Contraintes pour l'etape suivante:

- Generer exactement 1 epic MVP, centre sur la boucle de ride mock jouable.
- Generer 3 a 6 stories maximum.
- Ne pas importer le backlog Linear complet.
- Ne pas creer de stories BLE/FTMS, compte, backend, Meshy, GPX ou assets lourds.
- Garder chaque story verifiable localement avec Vite/React/TypeScript/Three.js.
- Prioriser les tests unitaires sur `ride` et `route`; garder Playwright au happy path mock.

Decoupage conseille, sans details de stories a ce stade:

- App shell et etats de session.
- Input mock, mapping vitesse, smoothing, progression et stats.
- Route prefabriquee, camera sur rail et selection biome.
- Scene Three.js MVP avec route visible, deux ambiances et placeholders.
- HUD, pause/reprise, fin de session, resume et happy path Playwright.


---
story_id: "1.2"
story_key: "1-2-source-mock-vitesse-lissee-progression-et-stats"
linear_id: "MYB-3"
epic_linear_id: "MYB-1"
title: "Source mock, vitesse lissee, progression et stats"
status: "done"
created: "2026-06-06"
scope: "vertical slice mock - pure ride logic only"
source_epic: "_bmad-output/planning-artifacts/echappee-3d-mvp-epic-stories.md"
baseline_commit: "2cc5faf1874776a9c71ad3858297dbe0979d7d8c"
depends_on: "MYB-2"
---

# Story 1.2: Source mock, vitesse lissee, progression et stats

Status: done

## Story

As a developpeur de la vertical slice mock,
I want fournir une logique ride pure qui transforme une entree mock en vitesse lissee, progression et statistiques,
so that les stories de route, rendu et HUD puissent consommer des donnees coherentes sans melanger React, Three.js ou materiel reel dans le coeur de calcul.

## Contexte

`MYB-3` demarre apres `MYB-2`, qui a mis en place le shell Vite/React/TypeScript, les phases de session et les tests de base. Cette story ajoute uniquement le coeur de ride pur, testable hors React et hors Three.js.

L'objectif est de produire une petite API TypeScript stable pour:

- lire une source `mock`;
- mapper `effort01` vers une vitesse cible;
- lisser la vitesse frame par frame;
- avancer une progression numerique bornee;
- calculer les stats de resume;
- exposer un snapshot de ride utilisable plus tard par `MYB-4`, `MYB-5` et `MYB-6`.

## Scope strict MYB-3

Inclus:

- Module `src/ride/*` pur, sans import React ni Three.js.
- Types ride minimaux: source mock, sample input, configs vitesse/smoothing, etat de vitesse, progression, stats, snapshot.
- `MockRideInputSource` avec `effort01` borne entre `0` et `1`.
- `mapMockInputToSpeed` avec clamp et mapping monotone entre vitesse min et max.
- `smoothSpeed` avec acceleration et deceleration progressives, sans overshoot.
- `advanceRouteProgress` ou equivalent nomme, limite a une progression numerique avec longueur totale parametree.
- `advanceRideFrame` ou equivalent simple pour composer input -> vitesse cible -> vitesse lissee -> progression -> snapshot.
- `calculateRideStats` pour duree, distance et vitesse moyenne.
- Tests unitaires prioritaires sur les fonctions pures.

Exclus:

- Route prefabriquee, `mockRouteDefinition`, sampling route, biomes, `cameraOnRail`, `RouteDefinition` riche: `MYB-4`.
- Scene Three.js, canvas host, meshes, lighting, fog, assets placeholders: `MYB-5`.
- HUD final, slider mock UI, raccourcis clavier, resume UI reel, Playwright happy path: `MYB-6`.
- BLE, Web Bluetooth, FTMS, cadence, puissance, resistance, pente, calibration materiel.
- Persistance, historique multi-session, backend, compte utilisateur.
- Refactor large du shell MYB-2 ou changement des phases de session existantes.

## Acceptance Criteria

1. `MockRideInputSource` expose uniquement la source `mock`, conserve l'effort courant en memoire et retourne un `RideInputSample` deterministe pour un `nowMs` fourni.
2. Toute valeur `effort01` hors bornes est clampee entre `0` et `1`, sans produire `NaN`, `Infinity` ou vitesse negative.
3. `mapMockInputToSpeed` transforme `effort01 = 0` en vitesse minimale configuree, `effort01 = 1` en vitesse maximale configuree, et les valeurs intermediaires de facon monotone.
4. `smoothSpeed` fait monter ou descendre la vitesse progressivement selon `dtSeconds`, `accelerationPerSecond` et `decelerationPerSecond`, sans depasser la vitesse cible.
5. `advanceRouteProgress` integre `speedMps * dtSeconds`, met a jour `distanceMeters`, calcule `progress01` dans `[0, 1]` et marque `completed` exactement quand la distance atteint la longueur totale parametree.
6. La progression de `MYB-3` ne cree pas de route prefabriquee: elle consomme seulement une longueur totale numerique ou une config minimale equivalente.
7. `calculateRideStats` retourne `elapsedMs`, `distanceMeters` et `averageSpeedMps` coherents, y compris pour `elapsedMs <= 0` ou `distanceMeters = 0`.
8. `advanceRideFrame` ou equivalent compose les calculs purs en un snapshot stable avec source `mock`, input, vitesse lissee, progression, elapsed et completion.
9. Les modules `ride` n'importent ni `react`, ni `react-dom`, ni `three`, ni fichier de `src/render`, ni fichier de `src/route`.
10. Aucune implementation de `MYB-4`, `MYB-5` ou `MYB-6` n'est ajoutee au-dela des types numeriques necessaires a l'integration future.

## Tasks / Subtasks

- [x] Definir les types ride purs. (AC: 1, 5, 7, 8, 9)
  - [x] Creer `src/ride/rideTypes.ts`.
  - [x] Definir `RideInputSourceKind = "mock"`.
  - [x] Definir `RideInputSample`, `RideInputSource`, `MockRideInputSource`.
  - [x] Definir `SpeedMappingConfig`, `SpeedSmoothingConfig`, `SmoothedSpeedState`.
  - [x] Definir `RideProgress`, `RideProgressConfig`, `RideFrameState`, `RideFrameSnapshot`, `RideStats`.
- [x] Implementer la source mock pure. (AC: 1, 2)
  - [x] Creer `src/ride/mockRideInputSource.ts`.
  - [x] Ajouter `createMockRideInputSource(initialEffort01?: number)` ou une classe simple equivalente.
  - [x] Clamper l'effort a l'ecriture et a la lecture.
  - [x] Ne pas utiliser timer, DOM, storage, event listener ou UI.
- [x] Implementer mapping et smoothing de vitesse. (AC: 2, 3, 4)
  - [x] Creer `src/ride/speed.ts`.
  - [x] Ajouter `mapMockInputToSpeed(sample, config)`.
  - [x] Ajouter `smoothSpeed(targetSpeedMps, previous, dtSeconds, config)`.
  - [x] Geler les comportements invalides: `dtSeconds <= 0` ne fait pas avancer, configs negatives sont traitees sans valeurs invalides.
- [x] Implementer la progression numerique. (AC: 5, 6)
  - [x] Creer `src/ride/progress.ts`.
  - [x] Ajouter `createInitialRideProgress()` si utile.
  - [x] Ajouter `advanceRouteProgress(previous, speedMps, dtSeconds, config)`.
  - [x] Utiliser seulement `totalDistanceMeters` ou `lengthMeters`; ne pas creer de route, biome, point 3D ou camera.
- [x] Implementer les stats de ride. (AC: 7)
  - [x] Creer `src/ride/stats.ts`.
  - [x] Ajouter `calculateRideStats(elapsedMs, distanceMeters)`.
  - [x] Retourner une moyenne en m/s et laisser les conversions UI a `MYB-6`.
- [x] Implementer un composeur de frame pure. (AC: 8)
  - [x] Creer `src/ride/advanceRideFrame.ts`.
  - [x] Consommer explicitement `nowMs` et `dtSeconds`.
  - [x] Ne pas appeler `Date.now()` dans le coeur de calcul.
  - [x] Ne pas brancher React, HUD ou rendu.
- [x] Exporter l'API ride. (AC: 9, 10)
  - [x] Creer `src/ride/index.ts`.
  - [x] Exporter les types et fonctions utiles aux stories suivantes.
  - [x] Verifier qu'aucun fichier `src/route`, `src/render`, `src/ui` ou `tests/e2e` n'est cree.
- [x] Ajouter les tests unitaires prioritaires. (AC: 1-10)
  - [x] Tests `MockRideInputSource`: source `mock`, timestamp fourni, clamp bas/haut.
  - [x] Tests `mapMockInputToSpeed`: min, max, intermediaire, clamp hors bornes.
  - [x] Tests `smoothSpeed`: montee progressive, descente progressive, pas d'overshoot, `dt` variable ou nul.
  - [x] Tests `advanceRouteProgress`: integration distance/vitesse, clamp fin, completion, longueur invalide.
  - [x] Tests `calculateRideStats`: duree, distance, moyenne, distance nulle, duree nulle.
  - [x] Test `advanceRideFrame`: composition source -> vitesse -> progression -> snapshot.
- [x] Verifier localement la story. (AC: 1-10)
  - [x] `npm run typecheck`
  - [x] `npm run test`
  - [x] `npm run build`

## Dev Notes

### Contraintes d'architecture

- React gere l'interface et les etats d'ecran; `MYB-3` ne doit pas brancher de composants UI.
- La logique ride doit rester testable hors React et hors Three.js.
- Three.js ne doit pas etre importe dans `src/ride`.
- La progression de cette story est numerique et bornee; la route prefabriquee, le sampling, les biomes et la camera arrivent dans `MYB-4`.
- Les donnees de session restent en memoire; aucune persistance n'est attendue.
- Le mode mock est la seule source d'entree MVP dans cette story.

### Decisions de scope pour eviter les derives

- Ne pas creer `src/route/*`, `mockRouteDefinition`, `sampleRouteAt`, `cameraOnRail` ou `selectBiomeAtProgress`.
- Ne pas creer `src/render/*`, `ThreeCanvasHost`, `SceneController`, mesh, lumiere, fog ou placeholder asset.
- Ne pas creer `MockRideControls`, `RideHud`, `RideSummary` UI, slider, clavier ou Playwright.
- Ne pas modifier `src/App.tsx` pour afficher des stats; l'integration ecran/HUD appartient a `MYB-6`.
- Ne pas ajouter de dependance runtime.

### Valeurs par defaut proposees

Ces valeurs sont des constantes locales acceptables pour demarrer les tests; elles peuvent etre renommees mais doivent rester faciles a ajuster:

- `DEFAULT_SPEED_MAPPING_CONFIG = { minSpeedMps: 1.5, maxSpeedMps: 9 }`
- `DEFAULT_SPEED_SMOOTHING_CONFIG = { accelerationPerSecond: 2.5, decelerationPerSecond: 4 }`
- Une longueur numerique de test autour de `1000` metres est suffisante pour verifier la progression, sans creer de route prefabriquee.

### Points d'integration futurs a preparer seulement

- `MYB-4` consommera `RideProgress` pour relier la progression a une route prefabriquee, puis ajoutera camera et biomes.
- `MYB-5` consommera un snapshot deja calcule; il ne doit pas recalculer vitesse, progression ou stats dans le render.
- `MYB-6` branchera le slider mock, le HUD et le resume UI sur cette API.

## Tests attendus

Les tests unitaires sont prioritaires et doivent rester deterministes:

- `src/ride/mockRideInputSource.test.ts`
- `src/ride/speed.test.ts`
- `src/ride/progress.test.ts`
- `src/ride/stats.test.ts`
- `src/ride/advanceRideFrame.test.ts`

Chaque test doit eviter le DOM, React, Three.js, timers reels et randomness non controlee.

## Project Structure Notes

Fichiers probables a creer:

- `src/ride/rideTypes.ts`
- `src/ride/mockRideInputSource.ts`
- `src/ride/speed.ts`
- `src/ride/progress.ts`
- `src/ride/stats.ts`
- `src/ride/advanceRideFrame.ts`
- `src/ride/index.ts`
- `src/ride/*.test.ts`

Fichiers probables a ne pas modifier:

- `src/App.tsx`
- `src/app/screens/*`
- `src/app/session/sessionReducer.ts`
- `vite.config.ts`
- `package.json`

Fichiers a ne pas creer dans MYB-3:

- `src/route/*`
- `src/render/*`
- `src/ui/*`
- `tests/e2e/*`

## Project Context Rules

- Respecter `AGENTS.md`: scope strict Linear, mock mode preserve, ride logic pure et testable, React pour UI/screen state, Three.js pour rendu seulement plus tard.
- Ne pas creer ou modifier Linear sans confirmation.
- Ne pas importer le backlog complet ni anticiper les stories post-MYB-3.
- Ne jamais committer de secret.

## References

- `_bmad-output/planning-artifacts/echappee-3d-mvp-epic-stories.md` - `Story 1.2 - Source mock, vitesse lissee, progression et stats`
- `_bmad-output/planning-artifacts/echappee-3d-architecture-mince.md` - `Modules MVP / ride`
- `_bmad-output/planning-artifacts/echappee-3d-architecture-mince.md` - `Contrats minimum`
- `_bmad-output/planning-artifacts/implementation-readiness-report-2026-06-06.md` - `Evaluation` et `Dependencies`
- `_bmad-output/implementation-artifacts/sprint-status.yaml` - statut local sprint
- `_bmad-output/linear-sync.md` - mapping `MYB-3` et dependances

## Questions ouvertes sauvegardees

- Les valeurs exactes de vitesse et de smoothing sont des defaults d'implementation pour le MVP; elles pourront etre ajustees par playtest sans changer les contrats.
- Le nom final de la fonction de progression peut rester `advanceRouteProgress` pour suivre les artefacts, meme si le fichier vit dans `src/ride/progress.ts` et ne cree pas de module route.

## Dev Agent Record

### Agent Model Used

Codex GPT-5

### Implementation Plan

- Creer une API `src/ride` pure et modulaire: types, source mock, vitesse, progression, stats et composition de frame.
- Verrouiller les contrats par tests unitaires Vitest avant implementation.
- Garder toute integration React, Three.js, route riche, HUD, persistance et materiel reel hors de MYB-3.

### Debug Log References

- 2026-06-06T15:24+02:00 - `npm run test` execute apres ajout des tests rouges: 5 suites `src/ride/*` echouent par modules absents, comme attendu.
- 2026-06-06T15:25+02:00 - `npm run test` passe: 8 fichiers de test, 26 tests.
- 2026-06-06T15:25+02:00 - `npm run typecheck` passe.
- 2026-06-06T15:26+02:00 - `npm run build` passe.
- 2026-06-06T15:28+02:00 - Validation finale `npm run typecheck && npm run test && npm run build` passe.
- 2026-06-06T15:46+02:00 - Revue `gds-code-review` approuvee; validations relancees: `npm run typecheck`, `npm run test`, `npm run build`.

### Review Results

- Verdict: approved.
- Findings: aucun bug, hors-scope, trou de test bloquant ou probleme d'architecture releve.
- Scope confirme: `src/ride` reste pur, hors React et hors Three.js; aucun code route/camera/biomes/Three.js/HUD/slider/Playwright/persistence/BLE ajoute.
- Verification ciblee: clamp `effort01`, mapping vitesse, smoothing sans overshoot, progression numerique, stats et snapshots conformes aux criteres MYB-3.

### Completion Notes List

- API ride pure ajoutee sous `src/ride/*`: source mock, clamp effort, mapping vitesse, smoothing sans overshoot, progression numerique bornee, stats et snapshot compose.
- Tests unitaires deterministes ajoutes pour les fonctions pures, sans DOM, React, Three.js, timers reels ni randomness.
- Scope MYB-3 respecte: aucun fichier `src/route`, `src/render`, `src/ui` ou `tests/e2e` cree, et aucun branchement HUD/UI/persistance/BLE ajoute.
- Revue MYB-3 approuvee localement, avec validations finales vertes.

### File List

- `_bmad-output/implementation-artifacts/1-2-source-mock-vitesse-lissee-progression-et-stats.md`
- `_bmad-output/implementation-artifacts/sprint-status.yaml`
- `_bmad-output/linear-sync.md`
- `src/ride/advanceRideFrame.test.ts`
- `src/ride/advanceRideFrame.ts`
- `src/ride/index.ts`
- `src/ride/mockRideInputSource.test.ts`
- `src/ride/mockRideInputSource.ts`
- `src/ride/progress.test.ts`
- `src/ride/progress.ts`
- `src/ride/rideTypes.ts`
- `src/ride/speed.test.ts`
- `src/ride/speed.ts`
- `src/ride/stats.test.ts`
- `src/ride/stats.ts`

### Change Log

- 2026-06-06 - Story MYB-3 creee: source mock, vitesse lissee, progression numerique et stats, avec scope strict hors React, hors Three.js, hors route/camera/HUD.
- 2026-06-06 - Story MYB-3 implementee et passee en review: API ride pure, tests unitaires et validations locales vertes.
- 2026-06-06 - Story MYB-3 approuvee en revue et passee a done.

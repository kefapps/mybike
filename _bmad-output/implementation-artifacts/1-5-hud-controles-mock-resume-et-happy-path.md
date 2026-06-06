---
story_id: "1.5"
story_key: "1-5-hud-controles-mock-resume-et-happy-path"
linear_id: "MYB-6"
epic_linear_id: "MYB-1"
title: "HUD, controles mock, resume et happy path"
status: "done"
created: "2026-06-06"
scope: "vertical slice mock - final playable loop HUD, controls, summary and happy path"
source_epic: "_bmad-output/planning-artifacts/echappee-3d-mvp-epic-stories.md"
baseline_commit: "4f620f7"
depends_on:
  - "MYB-2"
  - "MYB-3"
  - "MYB-5"
transitive_context:
  - "MYB-4"
---

# Story 1.5: HUD, controles mock, resume et happy path

Status: done

## Story

As a joueur de la vertical slice mock,
I want controler l'effort mock, lire les donnees essentielles pendant la balade, mettre en pause/reprendre/terminer et voir un resume coherent,
so that la boucle Echappee 3D soit jouable de bout en bout sans velo connecte.

## Contexte

`MYB-6` est la derniere story de l'epic MVP 1. Elle demarre apres:

- `MYB-2`: app shell React, phases `idle/running/paused/finished/error`, `StartScreen`, `RideScreen`, `SummaryScreen`, `WebGlFallback`.
- `MYB-3`: logique pure `src/ride/*` pour source mock, mapping vitesse, smoothing, progression, stats et `RideFrameSnapshot`.
- `MYB-4`: route prefabriquee, camera sur rail, biomes, fallback `placeholder-route`.
- `MYB-5`: scene Three.js MVP visible sous `src/render/*`; `ThreeCanvasHost` compose actuellement `advanceRideFrame` + `createRouteFrameSnapshot` avec un effort mock fixe `0.55`.

Cette story finalise la boucle jouable. Elle ne doit pas creer une nouvelle simulation parallele pour le HUD: une seule source de snapshots doit alimenter a la fois le rendu Three.js, le HUD et le resume.

## Scope strict MYB-6

Inclus:

- Slider mock visible et obligatoire pour regler l'effort `0..1` pendant la ride.
- Raccourcis clavier optionnels seulement; ils ne remplacent pas le slider.
- HUD minimal lisible: vitesse simulee, distance, temps, source `mock`, phase `running/paused`.
- Pause/reprise/fin de session coherentes avec le HUD et le canvas.
- Resume final avec duree, distance et vitesse moyenne depuis les stats `src/ride`.
- Happy path raisonnable: ouvrir l'app, lancer, modifier le slider, observer le HUD, pause, reprendre, finir, voir le resume.
- Tests unitaires/React proportionnes; Playwright happy path si l'ajout reste raisonnable dans le setup.
- Refactor minimal necessaire pour partager les snapshots MYB-3/MYB-4 entre `ThreeCanvasHost`, HUD et summary.

Exclus:

- BLE, Web Bluetooth, FTMS, Indoor Bike Data, resistance ou telemetrie reelle.
- Persistance, backend, compte utilisateur, cloud sync, historique local riche.
- Assets IA/Meshy, GLTF/FBX/DRACO, loaders externes ou pipeline asset lourd.
- Routes multiples, selection route, import GPX, editeur de route.
- Gros systeme de settings, menus avances, presets complexes, cockpit sportif.
- HUD avance: cadence, puissance, calories, graphes, zones, scoring.
- Refactor large de `MYB-2` a `MYB-5`.
- Reimplementation de la logique ride, route ou render deja livree.
- Suite Playwright exhaustive, multi-navigateurs ou perf tooling avance.

## Acceptance Criteria

1. Le joueur peut modifier l'effort mock via un slider visible pendant la balade; le slider est le controle principal obligatoire.
2. Le slider met a jour la source mock effectivement consommee par la boucle ride/render; il ne modifie pas une copie UI sans effet.
3. Le HUD affiche vitesse simulee, distance, temps, source `mock` et phase courante.
4. Le HUD consomme les `RideFrameSnapshot`/stats existants de `src/ride`; il ne recalcule pas vitesse, smoothing, progression ou stats.
5. Le rendu Three.js continue de consommer les snapshots route/render existants; `SceneController` ne devient pas responsable du HUD ou des controles.
6. La pause garde une image et un HUD stables: pas d'avancee distance/progression/temps pendant `paused`.
7. La reprise continue la meme session avec le dernier etat connu; elle ne redemarre pas la ride a zero.
8. Le bouton terminer fonctionne depuis `running` et `paused`, conserve le dernier resume disponible, puis affiche `SummaryScreen`.
9. `SummaryScreen` n'affiche plus les placeholders MYB-3; il affiche duree, distance et vitesse moyenne coherentes.
10. Si la session est terminee avant qu'un snapshot utile existe, le resume reste robuste avec des valeurs zero/coherentes, sans crash.
11. Le parcours complet reste jouable sans velo connecte, backend, persistance ou asset externe.
12. Aucun changement profond n'est introduit dans `src/ride/*`, `src/route/*` ou `src/render/SceneController.ts`; seules de petites adaptations d'integration sont autorisees si necessaires.
13. Le WebGL fallback de `MYB-2` reste preserve: si WebGL est indisponible, l'app affiche le fallback au lieu de monter la ride UI/canvas.
14. Un happy path automatique couvre au minimum: start -> slider -> HUD visible/progressant -> pause -> resume -> finish -> summary.
15. Les tests unitaires/React couvrent les controles mock, l'affichage HUD, la stabilite pause et le resume final.

## Tasks / Subtasks

- [x] Verifier les contrats existants avant modification. (AC: 4, 5, 12, 13)
  - [x] Lire `src/App.tsx`, `src/app/session/sessionReducer.ts`, `src/app/session/sessionTypes.ts`.
  - [x] Lire `src/app/screens/RideScreen.tsx` et `SummaryScreen.tsx`.
  - [x] Lire `src/render/ThreeCanvasHost.tsx` et `src/render/sceneTypes.ts`.
  - [x] Lire `src/ride/advanceRideFrame.ts`, `mockRideInputSource.ts`, `rideTypes.ts`.
  - [x] Confirmer que `src/ride/*` et `src/route/*` restent purs et ne doivent pas importer React/Three.js.

- [x] Definir une seule source de verite pour la ride active. (AC: 2, 4, 5, 6, 7)
  - [x] Adapter minimalement l'integration pour que le meme snapshot compose `{ ride, route }` alimente `SceneController.update`, `RideHud` et le futur resume.
  - [x] Option recommandee a faible blast radius: etendre `ThreeCanvasHost` avec props controlees `effort01` et `onFrame(snapshot)` ou extraire un petit hook d'integration ride dans `src/app/*`.
  - [x] Interdire une deuxieme boucle `advanceRideFrame` uniquement pour le HUD.
  - [x] Pendant `paused`, ne pas avancer la simulation et ne pas emettre de nouveau snapshot progressant.
  - [x] Garder le clamp et les formules de vitesse dans `src/ride`, pas dans React.

- [x] Ajouter le controle mock obligatoire. (AC: 1, 2, 11)
  - [x] Creer `MockRideControls` ou equivalent avec un `input type="range"` de `0` a `1` (`step` fin, par exemple `0.01`).
  - [x] Afficher une valeur lisible de l'effort mock sans transformer le HUD en cockpit avance.
  - [x] Brancher le slider sur la source mock consommee par la boucle ride.
  - [x] Garder clavier/raccourcis comme optionnels; si ajoutes, ils doivent synchroniser le slider.
  - [x] Tester les bornes `0` et `1` et une valeur intermediaire.

- [x] Ajouter le HUD minimal. (AC: 3, 4, 6, 15)
  - [x] Creer `RideHud` ou equivalent sans dependance a Three.js.
  - [x] Afficher vitesse en km/h ou m/s de facon stable et lisible.
  - [x] Afficher distance en metres/km, temps ecoule, source `mock`, phase `running/paused`.
  - [x] Gérer l'etat initial avant premier snapshot avec valeurs zero/coherentes.
  - [x] Eviter de superposer le HUD de facon a masquer la route ou rendre le mobile illisible.

- [x] Stabiliser pause/reprise/finish avec resume. (AC: 6, 7, 8, 9, 10)
  - [x] Faire en sorte que `onFinish` transmette le dernier `RideStats`/resume connu a `App` ou au reducer.
  - [x] Etendre `SessionState` de facon minimale pour stocker un resume final en memoire.
  - [x] Remplacer les placeholders de `SummaryScreen` par des props derivees du resume final.
  - [x] Formater duree, distance et vitesse moyenne avec helpers testables.
  - [x] Conserver `Retour au depart` qui reset la session.

- [x] Integrer l'UI dans `RideScreen` sans gros refactor. (AC: 1, 3, 5, 11, 12)
  - [x] Conserver le canvas `ThreeCanvasHost` dans `.ride-viewport`.
  - [x] Ajouter controls/HUD autour du canvas ou en overlay discret, selon lisibilite desktop/mobile.
  - [x] Conserver les actions existantes Pause/Reprendre/Terminer, ou les deplacer dans un `PauseOverlay` minimal sans changer la machine d'etat inutilement.
  - [x] Adapter CSS pour mobile/desktop, sans cartes imbriquees ni texte qui recouvre incoheremment le rendu.

- [x] Ajouter tests unitaires et React. (AC: 1-10, 13, 15)
  - [x] Tester les helpers de formatage summary/HUD: duree, distance, vitesse moyenne.
  - [x] Tester `MockRideControls`: change slider -> callback effort borne.
  - [x] Tester `RideHud`: affiche source `mock`, phase, vitesse, distance, temps depuis un snapshot.
  - [x] Tester `SummaryScreen`: affiche les vraies stats, plus de placeholder.
  - [x] Tester le flow React start -> ride -> pause -> resume -> finish -> summary avec fake timers/scheduler si necessaire.
  - [x] Tester que WebGL indisponible garde le fallback existant.

- [x] Ajouter un happy path raisonnable. (AC: 14)
  - [x] Si Playwright peut etre ajoute sans setup lourd, ajouter `test:e2e`, `playwright.config.*` et un seul test happy path mock.
  - [x] Le happy path Playwright doit utiliser le slider comme controle principal, pas uniquement le clavier.
  - [x] Si Playwright est juge disproportionne dans l'implementation, creer au minimum un test React d'integration couvrant le meme parcours et documenter le report dans le Dev Agent Record.
  - [x] Ne pas creer une suite multi-browser exhaustive.

- [x] Valider. (AC: 1-15)
  - [x] `npm run typecheck`
  - [x] `npm run test`
  - [x] `npm run build`
  - [x] Si `test:e2e` est configure dans cette story: `npm run test:e2e`
  - [x] Validation manuelle: lancer ride, bouger slider, constater HUD/progression, pause stable, resume coherent.

## Dev Notes

### Etat actuel issu de MYB-2

- `src/App.tsx` utilise `useReducer(sessionReducer, createInitialSessionState())`.
- `SessionState` contient `phase`, `startedAt`, `finishedAt`, `errorMessage`; il ne contient pas encore de summary final.
- `RideScreen` recoit `phase`, `onPause`, `onResume`, `onFinish`.
- `SummaryScreen` affiche encore `PLACEHOLDER_SUMMARY` et ne recoit pas de stats.
- `WebGlFallback` et `isWebGlAvailable` existent deja; ne pas dupliquer la detection.
- `App.test.tsx` couvre le flow start/pause/resume/finish mais attend encore les placeholders de summary.

### Etat actuel issu de MYB-3

- `src/ride/index.ts` exporte `advanceRideFrame`, `createInitialRideFrameState`, `createMockRideInputSource`, types et helpers.
- `RideInputSample` utilise `nowMs`, pas `timestampMs`.
- `MockRideInputSource` expose `getEffort01()` et `setEffort01(effort01)`.
- `RideFrameSnapshot` contient `source`, `input`, `targetSpeedMps`, `speedMps`, `progress`, `elapsedMs`, `stats`, `completed`.
- `stats` contient `elapsedMs`, `distanceMeters`, `averageSpeedMps`.
- Les calculs `speed`, `progress`, `stats` sont deja couverts par tests unitaires; ne pas les recopier dans `ui`.

### Etat actuel issu de MYB-4

- `src/route/index.ts` exporte `createRouteFrameSnapshot`, `mockRouteDefinition`, sampling, camera et biome.
- `createRouteFrameSnapshot(progress, route?, cameraConfig?)` accepte directement un `RideFrameSnapshot`.
- `RouteFrameSnapshot` contient `sample`, `camera`, `biomeId`, `usedFallback`, `fallbackReason`.
- Le fallback route `placeholder-route` est deja en place; MYB-6 ne doit pas rouvrir ce chantier.

### Etat actuel issu de MYB-5

- `src/render/ThreeCanvasHost.tsx` cree actuellement sa propre `MockRideInputSource(0.55)` et son propre `RideFrameState`.
- `ThreeCanvasHost` appelle `advanceRideFrame` puis `createRouteFrameSnapshot`, construit `RenderFrameSnapshot`, et appelle `controller.update(snapshot)`.
- `ThreeCanvasHost` accepte deja des props de test `controllerFactory`, `createInputSource`, `now`, `scheduleFrame`, `cancelFrame`.
- `paused` est gere par `phaseRef`: la simulation n'avance pas quand `phaseRef.current !== "running"`.
- `SceneController` doit rester dans `src/render` et ne doit pas connaitre le HUD, le slider ou le summary.
- Tests render existants utilisent un fake controller et peuvent servir de modele.

### Architecture a respecter

- React gere UI, etats d'ecran, controls, HUD et summary.
- Three.js gere seulement le rendu de snapshots deja calcules.
- `src/ride/*` gere l'effort mock, vitesse, smoothing, progression et stats.
- `src/route/*` gere route, camera, biomes et fallback route.
- `src/render/*` adapte les snapshots vers WebGL; pas de logique UI finale dans `SceneController`.
- Pour MYB-6, le point cle est le partage de snapshot: ne pas avoir une simulation pour le canvas et une autre pour le HUD.

### UX / UI guidance

- Le ton UX doit rester sobre, balade avant competition.
- Le slider doit etre visible et fiable sur desktop/mobile.
- Le HUD doit etre minimal et scannable: vitesse, distance, temps, `mock`, phase.
- Eviter cockpit sportif avance, grands panneaux decoratifs ou controles denses.
- Les boutons gardent des cibles tactiles >= 44px.
- Aucun texte ne doit masquer de facon incoherente la route sur mobile.

### Tests attendus

- Vitest/jsdom pour controls, HUD, summary et flow React.
- Tests de lifecycle existants de `ThreeCanvasHost` a adapter si ses props changent.
- Si Playwright est ajoute: un seul happy path mock, avec slider comme controle principal.
- Ne pas tester un vrai contexte WebGL dans jsdom; continuer avec fakes/mocks.

### Project Structure Notes

Fichiers probables a modifier:

- `src/App.tsx`
- `src/app/session/sessionTypes.ts`
- `src/app/session/sessionReducer.ts`
- `src/app/screens/RideScreen.tsx`
- `src/app/screens/SummaryScreen.tsx`
- `src/render/ThreeCanvasHost.tsx`
- `src/render/ThreeCanvasHost.test.tsx`
- `src/styles.css`
- `src/App.test.tsx`
- `package.json` / `package-lock.json` seulement si Playwright est ajoute.

Fichiers probables a creer:

- `src/ui/MockRideControls.tsx`
- `src/ui/RideHud.tsx`
- `src/ui/PauseOverlay.tsx` si utile, sinon garder les actions existantes.
- `src/ui/RideSummary.tsx` ou helpers de formatage summary si plus simple que de renommer `SummaryScreen`.
- `src/ui/*.test.tsx` ou tests proches des composants.
- `tests/e2e/ride-happy-path.spec.ts` et `playwright.config.*` seulement si Playwright est retenu.

Fichiers a ne pas modifier sauf bug bloquant prouve:

- `src/ride/*`
- `src/route/*`
- `src/render/SceneController.ts`
- `src/render/createRouteMesh.ts`
- `src/render/createBiomeVisuals.ts`

Fichiers a ne pas creer dans MYB-6:

- Source BLE/FTMS.
- Backend/API/db/persistence files.
- Route selector/editor/import GPX.
- Asset pipeline, Meshy, loaders GLTF/DRACO/FBX.
- Settings system large.

### Project Context Rules

- Respecter `AGENTS.md`: scope strict Linear, mock mode preserve, React pour UI/screen state, Three.js pour rendu depuis snapshots.
- BMAD reste file-first: modifier les artefacts locaux avant sync Linear.
- Les mises a jour Linear non destructives sur les issues/docs MYB existants sont automatiques.
- Ne pas importer l'ancien backlog complet ni anticiper du post-MVP.
- Ne jamais committer de secrets, tokens Linear, Meshy, BLE/FTMS credentials ou API keys.

### References

- `_bmad-output/planning-artifacts/echappee-3d-gdd-court.md` - `Vertical Slice Mock`, `Experience cible`, `Decisions verrouillees`.
- `_bmad-output/planning-artifacts/echappee-3d-architecture-mince.md` - `Modules MVP / ui`, `Flux de donnees MVP`, `Playwright`.
- `_bmad-output/planning-artifacts/echappee-3d-mvp-epic-stories.md` - `Story 1.5 - HUD, controles mock, resume et happy path`.
- `_bmad-output/planning-artifacts/implementation-readiness-report-2026-06-06.md` - caveat MYB-6 slider obligatoire, clavier optionnel.
- `_bmad-output/implementation-artifacts/1-4-scene-three-js-mvp-et-rendu-de-la-balade.md` - etat actuel MYB-5 et garde-fous render.
- `_bmad-output/linear-sync.md` - mapping Linear `MYB-6`, dependances et sync policy.
- `src/App.tsx` - orchestration ecrans/session.
- `src/app/screens/RideScreen.tsx` - point d'integration canvas/HUD/controls.
- `src/app/screens/SummaryScreen.tsx` - placeholder a remplacer par summary reel.
- `src/ride/advanceRideFrame.ts` - production unique de `RideFrameSnapshot`.
- `src/ride/mockRideInputSource.ts` - source mock a piloter par slider.
- `src/render/ThreeCanvasHost.tsx` - boucle render actuelle a adapter minimalement.

## Questions ouvertes sauvegardees

- Playwright est souhaitable pour le happy path final. Si son installation est jugee trop lourde pendant implementation, documenter le report et livrer un test React d'integration couvrant le meme parcours.
- Le format exact des unites UI peut rester simple tant que les valeurs sont coherentes et lisibles: vitesse en km/h, distance en m/km, temps en mm:ss.
- Les raccourcis clavier restent optionnels. Le slider est obligatoire et doit rester le controle principal.

## Dev Agent Record

### Agent Model Used

Codex GPT-5

### Debug Log References

- 2026-06-06 - RED: `npm run test` a echoue comme attendu avant implementation sur les nouveaux tests MYB-6 (`MockRideControls`, `RideHud`, `rideStatsFormat`, `SummaryScreen`, App flow, `ThreeCanvasHost onFrame/effort01`).
- 2026-06-06 - GREEN: `npm run test` passe avec 60 tests / 19 fichiers.
- 2026-06-06 - `npm run typecheck` passe.
- 2026-06-06 - `npm run build` passe; warning Vite chunk Three.js >500 kB conserve comme attendu pour MYB-5/MYB-6.
- 2026-06-06 - Vite dev server HTTP 200 sur `http://127.0.0.1:5174/` (`5173` etait deja occupe).
- 2026-06-06 - Validation Chrome headless/CDP desktop: start -> slider 90 % -> pause -> resume -> finish -> summary; HUD visible, summary sans placeholder, canvas crop ImageMagick non blank (`mean=0.595`, `stddev=0.117921`).
- 2026-06-06 - Validation Chrome headless/CDP mobile 390x844 DPR 2: start -> slider 90 % -> pause -> resume -> finish -> summary; HUD visible, summary sans placeholder, canvas crop ImageMagick non blank (`mean=0.624303`, `stddev=0.097109`).
- 2026-06-06 - `test:e2e` non lance car non configure; Playwright non ajoute pour eviter une dependance/setup lourd, couvert par test React d'integration et validation CDP reelle.
- 2026-06-06 - Review patch: `ThreeCanvasHost` preserve les `createInputSource` mock custom quand `effort01` n'est pas explicitement controle.
- 2026-06-06 - Review patch: l'horloge ride active n'avance plus pendant `paused`, donc HUD et summary n'incluent pas la duree de pause.
- 2026-06-06 - Validation post-review: `npm run typecheck`, `npm run test` avec 61 tests / 19 fichiers, `npm run build` OK avec warning Vite Three.js >500 kB attendu.
- 2026-06-06 - Validation browser post-review Chrome headless/CDP desktop et mobile: effort 90 %, summary reel, canvas crop non blank (`desktop mean=0.594989 stddev=0.117837`, `mobile mean=0.624251 stddev=0.0969858`).

### Completion Notes List

- Partage de snapshot unique ajoute via `ThreeCanvasHost` props `effort01` et `onFrame(snapshot)`: le meme `RenderFrameSnapshot` alimente `SceneController.update`, le HUD et le summary final.
- Slider mock obligatoire ajoute avec `input type="range"` `0..1` step `0.01`; il pilote la `MockRideInputSource` existante sans recalculer la logique ride dans React.
- HUD minimal ajoute sans dependance Three.js: vitesse, distance, temps, source `mock`, phase `running/paused`, avec valeurs zero coherentes avant premier snapshot.
- Summary final remplace les placeholders MYB-3 par les `RideStats` conserves dans l'etat de session; finish depuis `running` ou `paused` reste robuste avec resume zero si necessaire.
- Pause/reprise conserve la session et le dernier HUD; aucun changement dans `src/ride/*`, `src/route/*` ou `SceneController.ts`.
- Review MYB-6 approuvee apres patchs: effort custom non controle preserve et temps de pause exclu de l'horloge ride active.
- Point note non bloquant: un echec interne `WebGLRenderer` apres le preflight reste un comportement preexistant de MYB-5; le fallback existant WebGL indisponible de MYB-2 reste preserve et teste.

### File List

- `_bmad-output/implementation-artifacts/1-5-hud-controles-mock-resume-et-happy-path.md`
- `_bmad-output/implementation-artifacts/sprint-status.yaml`
- `_bmad-output/linear-sync.md`
- `src/App.test.tsx`
- `src/App.tsx`
- `src/app/screens/RideScreen.tsx`
- `src/app/screens/SummaryScreen.test.tsx`
- `src/app/screens/SummaryScreen.tsx`
- `src/app/session/sessionReducer.ts`
- `src/app/session/sessionTypes.ts`
- `src/render/ThreeCanvasHost.test.tsx`
- `src/render/ThreeCanvasHost.tsx`
- `src/styles.css`
- `src/ui/MockRideControls.test.tsx`
- `src/ui/MockRideControls.tsx`
- `src/ui/RideHud.test.tsx`
- `src/ui/RideHud.tsx`
- `src/ui/rideStatsFormat.test.ts`
- `src/ui/rideStatsFormat.ts`

### Change Log

- 2026-06-06 - Review MYB-6 approuvee: corrections source custom/horloge pause, validations 61 tests, Linear Done, next_action commit-MYB-6.
- 2026-06-06 - Story MYB-6 implementee: HUD minimal, slider mock pilote la source active, summary final reel, tests React/unitaires, validation CDP desktop/mobile.
- 2026-06-06 - Story MYB-6 creee: HUD minimal, slider mock obligatoire, summary reel, happy path final, garde-fous pour partager les snapshots sans dupliquer ride/route/render.

## Senior Developer Review (AI)

### Verdict

Approved - MYB-6 conforme apres corrections.

### Findings

- [x] [Review][Patch] `ThreeCanvasHost` ecrasait les sources mock custom fournies via `createInputSource` quand `effort01` n'etait pas explicitement fourni. Corrige en ne synchronisant l'effort que pour la source interne ou quand `effort01` est explicitement controle.
- [x] [Review][Patch] La duree de pause pouvait etre incluse dans `RideFrameSnapshot.elapsedMs` apres reprise. Corrige avec une horloge ride active qui n'avance que pendant `running`.
- [x] [Review][Defer] Echec `WebGLRenderer` apres preflight: comportement preexistant MYB-5, non introduit par MYB-6. Le fallback WebGL indisponible de MYB-2 reste preserve et teste; durcissement post-preflight a traiter hors scope si necessaire.

### Review Validation

- `npm run typecheck` OK.
- `npm run test` OK, 61 tests / 19 fichiers.
- `npm run build` OK, avec warning Vite chunk Three.js >500 kB attendu.
- Vite HTTP 200 sur `http://127.0.0.1:5174/`.
- Chrome headless/CDP desktop et mobile OK: effort modifie a 90 %, pause/reprise, summary reel, canvas crop non blank.

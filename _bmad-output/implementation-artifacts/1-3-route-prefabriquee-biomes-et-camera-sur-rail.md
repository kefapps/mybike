---
story_id: "1.3"
story_key: "1-3-route-prefabriquee-biomes-et-camera-sur-rail"
linear_id: "MYB-4"
epic_linear_id: "MYB-1"
title: "Route prefabriquee, biomes et camera sur rail"
status: "done"
created: "2026-06-06"
scope: "vertical slice mock - pure route, biome and camera logic only"
source_epic: "_bmad-output/planning-artifacts/echappee-3d-mvp-epic-stories.md"
baseline_commit: "0447f95"
depends_on: "MYB-3"
---

# Story 1.3: Route prefabriquee, biomes et camera sur rail

Status: done

## Story

As a developpeur de la vertical slice mock,
I want fournir une route courte prefabriquee avec sampling deterministe, selection de biome et camera premiere personne sur rail,
so that les stories de rendu et d'integration puissent consommer une position route, une camera et une ambiance coherentes sans melanger React, Three.js ou assets externes dans le coeur de calcul.

## Contexte

`MYB-4` demarre apres `MYB-3`, qui a ajoute une API pure sous `src/ride/*`: source mock, vitesse cible, smoothing, progression numerique, stats et `RideFrameSnapshot`. Cette story ne remplace pas ce coeur ride. Elle ajoute la couche route pure qui consomme la progression `0..1` deja produite par `MYB-3`.

L'objectif est de produire une petite API TypeScript stable pour:

- definir une route MVP courte et prefabriquee;
- echantillonner une position et une direction le long de cette route;
- calculer une camera premiere personne avec hauteur, look-ahead et FOV configures;
- selectionner un biome actif parmi au moins deux segments;
- gerer explicitement une route absente ou invalide par placeholder deterministe ou erreur typee;
- rester utilisable plus tard par `MYB-5` sans importer Three.js maintenant.

## Scope strict MYB-4

Inclus:

- Module `src/route/*` pur, sans import React ni Three.js.
- Types route minimaux: `Vec3`, point/segment de route, definition de route, segment biome, sample route, config camera, snapshot camera.
- `mockRouteDefinition` courte, finie, stable, adaptee a une demo de 1 a 3 minutes.
- `sampleRouteAt` ou equivalent pour echantillonner debut, milieu, fin et progressions hors bornes.
- `cameraOnRail` avec hauteur, look-ahead et FOV configures.
- `selectBiomeAtProgress` avec au moins deux biomes et une transition testable.
- Petit adaptateur pur optionnel qui consomme `RideProgress` ou `RideFrameSnapshot.progress` de `src/ride` pour produire route sample, camera et biome actif.
- Tests unitaires deterministes sur sampling, camera, biome et fallback route absente/invalide.

Exclus:

- Scene Three.js complete, canvas, meshes, lumiere, fog, terrain, render loop, `SceneController`: `MYB-5`.
- HUD, slider mock UI, raccourcis clavier, resume UI, Playwright happy path: `MYB-6`.
- Modification large de `src/ride/*`; seules de petites adaptations de types/export sont acceptables si elles evitent une duplication et ne changent pas les comportements MYB-3.
- Plusieurs routes, editeur de route, import GPX, generation procedurale avancee.
- Steering lateral, collisions gameplay, physique velo realiste, pente, resistance.
- Biomes complets, assets externes obligatoires, Meshy ou pipeline IA.
- BLE, Web Bluetooth, FTMS, backend, persistance, compte utilisateur, historique riche.

## Acceptance Criteria

1. `mockRouteDefinition` expose une route courte prefabriquee avec `id`, `lengthMeters`, points finis et au moins deux segments de biome couvrant la balade.
2. `sampleRouteAt` ou equivalent echantillonne la route de maniere deterministe pour une progression normalisee entre `0` et `1`.
3. Les progressions hors bornes, `NaN`, `Infinity` ou negatives sont clampees ou traitees sans produire de position invalide.
4. Le sampling retourne des valeurs finies au debut, au milieu et a la fin de la route, avec une direction/look-ahead exploitable par la camera.
5. `cameraOnRail` retourne toujours une position, un point de regard et un FOV valides et finis pour la route MVP.
6. La camera applique une hauteur, un look-ahead et un FOV configures; les configs invalides utilisent des defaults deterministes ou sont bornees.
7. `selectBiomeAtProgress` retourne un biome deterministe, change au moins une fois pendant la balade, et gere les bornes `0` et `1`.
8. La couche route consomme la progression/snapshot de `MYB-3` sans recalculer vitesse, smoothing, stats ou phases de session.
9. Une route absente ou invalide ne provoque pas de crash: le module route retourne un placeholder deterministe ou une erreur typee exploitable, sans `NaN`, `Infinity` ni valeurs numeriques invalides.
10. Les modules `route` n'importent ni `react`, ni `react-dom`, ni `three`, ni fichier de `src/render`, `src/ui` ou `tests/e2e`.
11. Aucun code de `MYB-5` ou `MYB-6` n'est ajoute: pas de rendu final, pas de HUD, pas de slider, pas de Playwright.

## Tasks / Subtasks

- [x] Analyser le contrat `src/ride` existant. (AC: 8, 10)
  - [x] Verifier `RideProgress`, `RideFrameSnapshot`, `advanceRideFrame` et les exports `src/ride/index.ts`.
  - [x] Confirmer que la route consomme `progress.progress01` et `progress.distanceMeters` sans modifier le mapping vitesse, le smoothing ou les stats.
  - [x] Garder le champ actuel `RideInputSample.nowMs`; ne pas inventer un contrat `timestampMs` divergent.
- [x] Definir les types route purs. (AC: 1, 4, 5, 9, 10)
  - [x] Creer `src/route/routeTypes.ts` ou equivalent.
  - [x] Definir `Vec3`, `RoutePoint`, `BiomeSegment`, `RouteDefinition`, `RouteSample`, `CameraRailConfig`, `CameraRigSnapshot`.
  - [x] Definir un type de resolution/fallback si utile, par exemple route prete, placeholder deterministe ou erreur typee.
  - [x] Ne pas importer `THREE.Vector3`; garder des objets numeriques simples `{ x, y, z }`.
- [x] Creer la route MVP prefabriquee. (AC: 1, 7)
  - [x] Creer `mockRouteDefinition` dans `src/route/mockRouteDefinition.ts` ou equivalent.
  - [x] Utiliser une longueur courte et coherent avec une demo de 1 a 3 minutes.
  - [x] Fournir assez de points pour un rail lisible, doux et stable.
  - [x] Ajouter au moins deux biomes simples, par exemple `coast` puis `forest`, avec segments couvrant `[0, 1]`.
- [x] Implementer la validation/fallback route. (AC: 3, 9)
  - [x] Detecter route absente, longueur invalide, points insuffisants, coordonnees non finies ou biomes incoherents.
  - [x] Choisir explicitement un comportement: placeholder deterministe ou erreur typee exploitable par l'app.
  - [x] Verrouiller par test que ce comportement ne crashe pas et ne retourne pas de nombres invalides.
- [x] Implementer le sampling route. (AC: 2, 3, 4)
  - [x] Ajouter `sampleRouteAt(route, progress01)` ou equivalent.
  - [x] Clamper la progression dans `[0, 1]`.
  - [x] Interpoler une position et une direction/look-ahead suffisante pour la camera.
  - [x] Tester debut, milieu, fin, valeurs hors bornes et valeurs non finies.
- [x] Implementer la camera sur rail. (AC: 5, 6)
  - [x] Ajouter `cameraOnRail(route, progress, config)` ou equivalent, consommant le `RideProgress` MYB-3 quand c'est pertinent.
  - [x] Appliquer hauteur, look-ahead et FOV sans mouvement brutal sur la route exemple.
  - [x] Borner les configs invalides pour garder position, look-at et FOV finis.
  - [x] Tester position/look-at deterministes, valeurs finies et look-ahead borne en fin de route.
- [x] Implementer la selection de biome. (AC: 7)
  - [x] Ajouter `selectBiomeAtProgress(route, progress01)`.
  - [x] Gerer les seuils, les bornes `0` / `1` et les segments invalides sans crash.
  - [x] Tester qu'au moins une transition intervient sur la route MVP.
- [x] Ajouter un composeur route pur optionnel. (AC: 8)
  - [x] Si utile, creer `createRouteFrameSnapshot(rideSnapshotOrProgress, route, cameraConfig)` ou nom equivalent.
  - [x] Retourner sample route, camera et biome actif sans modifier `advanceRideFrame`.
  - [x] Garder ce composeur hors React et hors Three.js.
- [x] Exporter l'API route. (AC: 10, 11)
  - [x] Creer `src/route/index.ts`.
  - [x] Exporter types, `mockRouteDefinition`, sampling, camera et biome.
  - [x] Ne pas creer `src/render/*`, `src/ui/*` ou `tests/e2e/*`.
- [x] Ajouter les tests unitaires prioritaires. (AC: 1-11)
  - [x] Tests `mockRouteDefinition`: longueur, points, biomes, valeurs finies.
  - [x] Tests `sampleRouteAt`: debut, milieu, fin, clamp et invalides.
  - [x] Tests `cameraOnRail`: position/look-at deterministes, FOV fini, look-ahead borne.
  - [x] Tests `selectBiomeAtProgress`: seuils, bornes et transition.
  - [x] Tests fallback route absente/invalide: placeholder deterministe ou erreur typee, sans crash ni valeurs invalides.
- [x] Verifier localement la story. (AC: 1-11)
  - [x] `npm run typecheck`
  - [x] `npm run test`
  - [x] `npm run build`

### Review Findings

- [x] [Review][Patch] Rejeter les `points` / `biomes` malformes sans throw et sans selectionner un biome invalide — corrige dans `src/route/routeValidation.ts`, `src/route/routeMath.ts` et `src/route/routeFallback.test.ts`.
- [x] [Review][Patch] Normaliser une config camera `null` vers les defaults deterministes — corrige dans `src/route/cameraOnRail.ts`, `src/route/createRouteFrameSnapshot.ts` et `src/route/cameraOnRail.test.ts`.

## Dev Notes

### Etat actuel issu de MYB-3

- `src/ride/rideTypes.ts` expose deja `RideProgress` avec `distanceMeters`, `progress01` et `completed`.
- `src/ride/advanceRideFrame.ts` compose input mock, vitesse cible, smoothing, progression numerique, stats et `RideFrameSnapshot`.
- `RideFrameSnapshot` contient `progress`, `speedMps`, `elapsedMs`, `stats` et `completed`; il ne contient pas encore camera ou biome.
- La config actuelle de progression est numerique: `progress: { totalDistanceMeters: 1000 }`.
- `src/ride` est pur et n'importe ni React ni Three.js; cette frontiere doit rester intacte.

### Contraintes d'architecture

- React gere les ecrans et l'UI; `MYB-4` ne doit pas brancher de composant UI.
- Three.js gere le rendu plus tard; `MYB-4` ne doit pas importer Three.js ni creer de mesh.
- La logique route/camera/biome doit rester testable hors React et hors Three.js.
- Les types route manipulent des metres, secondes, progression normalisee et `Vec3` simples.
- `MYB-5` adaptera ces donnees vers le rendu Three.js; `MYB-4` doit seulement fournir des snapshots purs.

### Decisions de scope pour eviter les derives

- Ne pas recalculer vitesse, smoothing, stats ou completion dans `src/route`; consommer la progression existante.
- Ne pas modifier `src/App.tsx`, `src/app/screens/*`, `src/ui/*` ou `src/render/*` pour afficher la route.
- Ne pas ajouter de slider, controle clavier, HUD ou resume UI.
- Ne pas ajouter de dependance runtime.
- Ne pas ajouter Playwright: la validation de MYB-4 est unitaire uniquement.

### Valeurs par defaut proposees

Ces valeurs peuvent etre ajustees par implementation tant que les tests restent deterministes:

- `DEFAULT_CAMERA_RAIL_CONFIG = { heightMeters: 1.6, lookAheadMeters: 12, fovDegrees: 70 }`
- Route MVP autour de `900` a `1200` metres, coherent avec le `totalDistanceMeters: 1000` de MYB-3.
- Deux biomes minimum: `coast` sur le debut, `forest` sur la suite.

### Caveat readiness obligatoire

Le caveat deja synchronise dans Linear doit rester explicite dans l'implementation:

- Route absente/invalide -> placeholder deterministe ou erreur typee.
- Aucun crash runtime.
- Aucune valeur `NaN`, `Infinity`, distance negative, progression hors borne non controlee, camera invalide ou FOV invalide.

## Tests attendus

Les tests unitaires sont prioritaires et doivent rester deterministes:

- `src/route/mockRouteDefinition.test.ts`
- `src/route/sampleRouteAt.test.ts`
- `src/route/cameraOnRail.test.ts`
- `src/route/selectBiomeAtProgress.test.ts`
- `src/route/routeFallback.test.ts` ou couverture equivalente dans les tests precedents.

Chaque test doit eviter le DOM, React, Three.js, timers reels et randomness non controlee.

## Project Structure Notes

Fichiers probables a creer:

- `src/route/routeTypes.ts`
- `src/route/mockRouteDefinition.ts`
- `src/route/routeValidation.ts`
- `src/route/sampleRouteAt.ts`
- `src/route/cameraOnRail.ts`
- `src/route/selectBiomeAtProgress.ts`
- `src/route/index.ts`
- `src/route/*.test.ts`

Fichiers a lire avant modification eventuelle:

- `src/ride/rideTypes.ts`
- `src/ride/progress.ts`
- `src/ride/advanceRideFrame.ts`
- `src/ride/index.ts`

Fichiers probables a ne pas modifier:

- `src/App.tsx`
- `src/app/screens/*`
- `src/app/session/sessionReducer.ts`
- `src/ride/speed.ts`
- `src/ride/stats.ts`
- `vite.config.ts`
- `package.json`

Fichiers a ne pas creer dans MYB-4:

- `src/render/*`
- `src/ui/*`
- `tests/e2e/*`

## Project Context Rules

- Respecter `AGENTS.md`: scope strict Linear, mock mode preserve, ride logic pure et testable, React pour UI/screen state, Three.js pour rendu seulement.
- Les mises a jour Linear non destructives sur les issues/docs MYB existants sont automatiques.
- Ne pas importer le backlog complet ni anticiper les stories post-MYB-4.
- Ne jamais committer de secret.

## References

- `_bmad-output/planning-artifacts/echappee-3d-mvp-epic-stories.md` - `Story 1.3 - Route prefabriquee, biomes et camera sur rail`
- `_bmad-output/planning-artifacts/echappee-3d-architecture-mince.md` - `Modules MVP / route`
- `_bmad-output/planning-artifacts/echappee-3d-architecture-mince.md` - `Contrats minimum`
- `_bmad-output/planning-artifacts/implementation-readiness-report-2026-06-06.md` - caveat fallback route absente/invalide
- `_bmad-output/implementation-artifacts/1-2-source-mock-vitesse-lissee-progression-et-stats.md` - contexte et API MYB-3
- `_bmad-output/implementation-artifacts/sprint-status.yaml` - statut local sprint
- `_bmad-output/linear-sync.md` - mapping `MYB-4` et dependances
- `src/ride/rideTypes.ts` - contrat `RideProgress` et `RideFrameSnapshot`
- `src/ride/advanceRideFrame.ts` - snapshot ride MYB-3 a consommer

## Questions ouvertes sauvegardees

- Les points exacts de la route prefabriquee ne sont pas imposes; ils doivent seulement produire un rail doux, fini et testable.
- Le choix placeholder deterministe vs erreur typee est laisse a l'implementation, mais l'un des deux doit etre explicite, teste et exploitable.
- La camera peut rester simple; le confort final sera valide visuellement dans `MYB-5`, pas dans cette story.

## Dev Agent Record

### Agent Model Used

Codex GPT-5

### Implementation Plan

- Creer une API `src/route` pure et modulaire: types, route MVP, validation/fallback, sampling, camera et biome.
- Verrouiller les contrats par tests unitaires Vitest avant implementation.
- Consommer la progression MYB-3 sans modifier vitesse, smoothing, stats, rendu, HUD ou controles.

### Debug Log References

- 2026-06-06 - RED: `npm run test -- src/route` a echoue comme attendu sur modules `src/route` absents.
- 2026-06-06 - GREEN: `npm run test -- src/route` passe, 5 fichiers de tests, 14 tests.
- 2026-06-06 - Validation: `npm run typecheck` passe.
- 2026-06-06 - Validation: `npm run test` passe, 13 fichiers de tests, 40 tests.
- 2026-06-06 - Validation: `npm run build` passe.
- 2026-06-06 - Review fix: durcissement route invalide malformee (`points`/`biomes`) et config camera `null`; `npm run test -- src/route` passe, 5 fichiers de tests, 17 tests.
- 2026-06-06 - Review validation finale: `npm run typecheck` passe.
- 2026-06-06 - Review validation finale: `npm run test` passe, 13 fichiers de tests, 43 tests.
- 2026-06-06 - Review validation finale: `npm run build` passe.

### Completion Notes List

- Ajout de l'API pure `src/route` pour route prefabriquee, validation/fallback, sampling, camera sur rail, selection biome et composeur de snapshot route.
- Le fallback choisi est un placeholder deterministe `placeholder-route`, retourne via `RouteResolution` avec raison typee, sans crash ni nombres invalides.
- `createRouteFrameSnapshot` consomme `RideProgress` ou `RideFrameSnapshot` MYB-3 sans recalculer vitesse, smoothing, stats ou etats de session.
- Aucun import React, Three.js, `src/render`, `src/ui` ou Playwright n'a ete ajoute dans `src/route`.
- Revue locale `gds-code-review` approuvee apres correction des edge cases route malformee et config camera `null`.

### File List

- `_bmad-output/implementation-artifacts/1-3-route-prefabriquee-biomes-et-camera-sur-rail.md`
- `_bmad-output/implementation-artifacts/sprint-status.yaml`
- `_bmad-output/linear-sync.md`
- `src/route/cameraOnRail.test.ts`
- `src/route/cameraOnRail.ts`
- `src/route/createRouteFrameSnapshot.ts`
- `src/route/index.ts`
- `src/route/mockRouteDefinition.test.ts`
- `src/route/mockRouteDefinition.ts`
- `src/route/routeFallback.test.ts`
- `src/route/routeMath.ts`
- `src/route/routeTypes.ts`
- `src/route/routeValidation.ts`
- `src/route/sampleRouteAt.test.ts`
- `src/route/sampleRouteAt.ts`
- `src/route/selectBiomeAtProgress.test.ts`
- `src/route/selectBiomeAtProgress.ts`

### Change Log

- 2026-06-06 - Story MYB-4 creee: route prefabriquee, biomes et camera sur rail, avec scope strict hors Three.js, hors HUD/slider et hors Playwright.
- 2026-06-06 - Story MYB-4 implementee: API route pure, route MVP, biomes, sampling, camera sur rail, fallback placeholder et tests unitaires.
- 2026-06-06 - Story MYB-4 revue et approuvee: status done, validations finales typecheck/test/build vertes.

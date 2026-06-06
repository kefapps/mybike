---
story_id: "1.4"
story_key: "1-4-scene-three-js-mvp-et-rendu-de-la-balade"
linear_id: "MYB-5"
epic_linear_id: "MYB-1"
title: "Scene Three.js MVP et rendu de la balade"
status: "done"
created: "2026-06-06"
scope: "vertical slice mock - Three.js scene and readable ride rendering only"
source_epic: "_bmad-output/planning-artifacts/echappee-3d-mvp-epic-stories.md"
baseline_commit: "dabf108"
depends_on:
  - "MYB-2"
  - "MYB-3"
  - "MYB-4"
---

# Story 1.4: Scene Three.js MVP et rendu de la balade

Status: done

## Story

As a developpeur de la vertical slice mock,
I want monter une scene Three.js MVP lisible qui rend la route, la camera et les biomes depuis les snapshots ride/route existants,
so that la balade mock devienne visuellement jouable sans melanger la logique ride/route avec le rendu 3D.

## Contexte

`MYB-5` demarre apres:

- `MYB-2`: app shell React, phases `idle/running/paused/finished/error`, fallback WebGL, `RideScreen` avec placeholder visuel.
- `MYB-3`: API pure `src/ride/*` pour source mock, vitesse cible, smoothing, progression, stats et `RideFrameSnapshot`.
- `MYB-4`: API pure `src/route/*` pour route prefabriquee, sampling, biomes, camera sur rail, `RouteFrameSnapshot` et fallback `placeholder-route`.

Cette story remplace le placeholder visuel du ride par une scene Three.js simple et vivante. Elle ne cree pas le HUD final, le slider mock, le happy path Playwright ou les controles ride UI: ces elements restent pour `MYB-6`.

## Scope strict MYB-5

Inclus:

- Ajout de la dependance runtime `three` pour le rendu MVP, avec lockfile mis a jour.
- Module `src/render/*` dedie au cycle de vie Three.js.
- `ThreeCanvasHost`: composant React mince qui monte/demonte un canvas et branche le `SceneController`.
- `SceneController` minimal avec API `mount`, `resize`, `update`, `dispose` ou equivalent.
- Integration dans `RideScreen` pour afficher le canvas pendant les phases `running` et `paused`.
- Boucle de rendu lisible alimentee par `advanceRideFrame` et `createRouteFrameSnapshot`, sans recalculer ride/route dans le rendu.
- Route visible, sol/horizon placeholders, reperes de biomes `coast` / `forest`, camera lisible.
- Animation courte et deterministe suffisante pour constater l'avancee de la camera en mode mock.
- Fallback placeholder si la route resolved est `placeholder-route` ou `usedFallback`.
- Resize canvas propre et dispose explicite des ressources Three.js.
- Validation manuelle pragmatique de performance: canvas non vide, resize correct, mouvement fluide sur machine de dev, sans profiler ni tooling avance.

Exclus:

- HUD final, vitesse/distance/temps/source `mock` visibles en UI: `MYB-6`.
- Slider mock, controles ride UI, raccourcis clavier, presets d'effort: `MYB-6`.
- Happy path Playwright ou installation Playwright: `MYB-6`.
- Resume final enrichi, stats UI branchees au rendu, pause overlay final: `MYB-6`.
- Persistance, backend, compte utilisateur, historique local riche.
- BLE, Web Bluetooth, FTMS, Indoor Bike Data, resistance ou telemetrie reelle.
- Assets IA, Meshy, GLTF/FBX/DRACO, pipeline asset lourd, loaders externes.
- Plusieurs routes, editeur de route, import GPX, generation procedurale avancee.
- Camera libre, orbit controls, controles 3D joueur, steering lateral, collisions, physique velo.
- Effets complexes: post-processing, meteo, audio, cycle jour/nuit, ombres avancees.

## Acceptance Criteria

1. `three` est ajoute comme dependance runtime et le projet build avec Vite/TypeScript.
2. `src/render/*` contient le cycle de vie Three.js; aucun code Three.js n'est ajoute dans `src/ride/*` ou `src/route/*`.
3. `ThreeCanvasHost` monte un canvas dans `RideScreen`, cree le `SceneController`, gere resize et appelle `dispose` au demontage.
4. `SceneController` expose une API testable `mount`/`resize`/`update`/`dispose` ou equivalent, et garde la logique WebGL hors des composants UI.
5. Le rendu consomme `RideFrameSnapshot` de `src/ride` et `RouteFrameSnapshot` de `src/route`; il ne recalcule pas vitesse, smoothing, progression, camera ou biome.
6. La scene affiche une route visible et un sol/horizon placeholder non vide sans asset externe.
7. La camera Three.js applique les positions `camera.position`, `camera.lookAt` et `fovDegrees` fournies par `createRouteFrameSnapshot`.
8. Les biomes `coast` et `forest` produisent une difference visuelle lisible via couleurs, fog, lumiere ou formes procedurales simples.
9. La phase `paused` garde une image stable: la scene reste montee, mais la progression/camera n'avance pas.
10. Une route absente/invalide ou `usedFallback` n'entraine pas de crash ni canvas vide; la scene affiche le placeholder route/ambiance fallback.
11. Le canvas se redimensionne correctement avec sa zone d'affichage et garde une camera non deformee.
12. Les ressources principales sont disposees au demontage: renderer, geometries, materials, textures/event listeners/animation loop.
13. Aucun code de `MYB-6` n'est ajoute: pas de HUD final, pas de slider, pas de Playwright, pas de resume enrichi.
14. La validation de performance reste pragmatique et manuelle: canvas non vide, resize correct, mouvement fluide sur machine de dev, sans profiler ni tooling perf avance.

## Tasks / Subtasks

- [x] Verifier l'etat courant et les contrats a consommer. (AC: 2, 5, 13)
  - [x] Lire `src/app/screens/RideScreen.tsx`: placeholder actuel `.ride-viewport` / `.ride-status-mark`.
  - [x] Lire `src/app/webgl/webglSupport.ts`: garder le fallback WebGL de `MYB-2`, ne pas dupliquer une detection lourde.
  - [x] Lire `src/ride/index.ts`, `advanceRideFrame.ts`, `rideTypes.ts`, `mockRideInputSource.ts`.
  - [x] Lire `src/route/index.ts`, `createRouteFrameSnapshot.ts`, `routeTypes.ts`, `mockRouteDefinition.ts`.
  - [x] Confirmer qu'aucune modification de `src/ride/*` et `src/route/*` n'est necessaire sauf bug bloquant prouve.
- [x] Ajouter Three.js de facon minimale. (AC: 1)
  - [x] Installer `three` comme dependance runtime, version courante verifiee: `0.184.0`.
  - [x] Mettre a jour `package.json` et `package-lock.json` uniquement; ne pas ajouter React Three Fiber, drei, loaders ou plugins.
  - [x] Si TypeScript signale des declarations manquantes, ajouter `@types/three` seulement comme devDependency et documenter pourquoi.
- [x] Definir le module render. (AC: 2, 4, 12)
  - [x] Creer `src/render/sceneTypes.ts` ou equivalent pour les types de snapshots consommes par la scene.
  - [x] Creer `src/render/SceneController.ts` ou `createSceneController.ts`.
  - [x] Creer `src/render/index.ts` pour les exports publics utiles.
  - [x] Limiter les imports `three` a `src/render/*` et aux tests render si necessaire.
- [x] Creer `ThreeCanvasHost`. (AC: 3, 9, 11, 12)
  - [x] Creer `src/render/ThreeCanvasHost.tsx`.
  - [x] Utiliser un `canvas` controle par React et passer ce canvas au controller.
  - [x] Monter le controller une seule fois par mount, puis le disposer au unmount.
  - [x] Gerer resize via `ResizeObserver` si disponible, avec fallback `window.resize`.
  - [x] Pendant `paused`, continuer a rendre la derniere frame sans avancer le state ride.
- [x] Creer une boucle ride -> route -> render strictement composee. (AC: 5, 7, 9, 10, 13)
  - [x] Pour le MVP MYB-5, utiliser une source mock fixe sans UI, par exemple `createMockRideInputSource(0.55)`, afin d'avoir un mouvement visible avant `MYB-6`.
  - [x] Avancer la simulation via `advanceRideFrame`; ne pas refaire les formules de vitesse, smoothing, progression ou stats dans `render`.
  - [x] Convertir chaque `RideFrameSnapshot` en `RouteFrameSnapshot` via `createRouteFrameSnapshot`.
  - [x] Passer au controller un snapshot compose, par exemple `{ ride, route }`.
  - [x] Ne pas ajouter de slider, clavier, HUD ou affichage stats.
- [x] Implementer `SceneController` MVP. (AC: 4, 6, 7, 8, 10, 11, 12)
  - [x] Creer `THREE.WebGLRenderer({ canvas, antialias: true })` et une `THREE.Scene`.
  - [x] Creer une `THREE.PerspectiveCamera`; appliquer `fovDegrees`, aspect, position et `lookAt` depuis le snapshot route.
  - [x] Implementer `resize(width, height)` avec `renderer.setSize(width, height, false)` et `camera.updateProjectionMatrix()`.
  - [x] Implementer `update(snapshot)` sans muter les snapshots recus.
  - [x] Implementer `dispose()` qui arrete l'animation loop/listeners et dispose renderer, geometries, materials et textures creees.
- [x] Creer les placeholders visuels. (AC: 6, 8, 10, 14)
  - [x] Creer `createRouteMesh` ou equivalent a partir de `mockRouteDefinition.points` / `RouteSample` sans recalculer la route.
  - [x] Afficher une ligne ou ruban de route visible devant la camera.
  - [x] Creer un sol/horizon simple: plane, couleur de ciel, fog legere, lumiere ambiante/directionnelle.
  - [x] Creer des reperes biome simples: `coast` plus clair/bleute, `forest` plus vert/ombrage doux.
  - [x] Prevoir un style `placeholder` si `route.usedFallback` ou `route.biomeId === "placeholder"`.
- [x] Integrer dans `RideScreen` sans anticiper MYB-6. (AC: 3, 9, 13)
  - [x] Remplacer la marque statique `.ride-status-mark` par `ThreeCanvasHost`.
  - [x] Garder les boutons existants `Pause/Reprendre/Terminer` de `MYB-2`.
  - [x] Ne pas ajouter de HUD, slider, controles mock UI ou resume enrichi.
  - [x] Adapter CSS pour canvas full-bleed dans `.ride-viewport` sans carte imbriquee ni texte qui masque le rendu.
- [x] Ajouter tests proportionnes. (AC: 3, 4, 9, 11, 12)
  - [x] Tester `ThreeCanvasHost` avec un controller fake: mount, resize/update, pause stable, dispose au unmount.
  - [x] Tester les helpers purs render si ajoutes: conversion `Vec3` -> `THREE.Vector3`, selection de palette biome, fallback placeholder.
  - [x] Eviter les tests jsdom qui exigent un vrai contexte WebGL.
  - [x] Ne pas ajouter Playwright dans cette story.
- [x] Faire la validation manuelle et technique. (AC: 1-14)
  - [x] `npm run typecheck`
  - [x] `npm run test`
  - [x] `npm run build`
  - [x] Lancer le dev server et verifier manuellement: canvas non vide, mouvement visible en running, image stable en paused, resize correct.
  - [x] Noter dans le Dev Agent Record: validation performance pragmatique effectuee sans profiler/tooling avance.

### Review Findings

- [x] [Review][Patch] Render fallback for malformed routes now resolves placeholder route points before mesh creation. [`src/render/renderHelpers.ts`]
- [x] [Review][Patch] Canvas viewport now has stable responsive height on desktop/mobile and action buttons keep their natural size. [`src/styles.css`]

## Dev Notes

### Etat actuel issu de MYB-2

- `src/App.tsx` choisit l'ecran selon `session.phase`.
- `RideScreen` existe deja et recoit `phase`, `onPause`, `onResume`, `onFinish`.
- `RideScreen` affiche actuellement un placeholder visuel dans `.ride-viewport`; c'est le point d'integration principal.
- `WebGlFallback` et `isWebGlAvailable` existent deja; ne pas recreer un nouveau fallback global.
- `SummaryScreen` contient encore des placeholders de stats; ne pas l'enrichir ici.

### Etat actuel issu de MYB-3

- `src/ride/index.ts` exporte `advanceRideFrame`, `createInitialRideFrameState`, `createMockRideInputSource`, types et helpers.
- `RideInputSample` utilise le champ `nowMs`, pas `timestampMs`; ne pas inventer un contrat different.
- `RideFrameSnapshot` contient `source`, `input`, `targetSpeedMps`, `speedMps`, `progress`, `elapsedMs`, `stats`, `completed`.
- `DEFAULT_RIDE_FRAME_CONFIG.progress.totalDistanceMeters` vaut `1000`, coherent avec la route MVP MYB-4.
- `src/ride/*` doit rester pur, hors React et hors Three.js.

### Etat actuel issu de MYB-4

- `src/route/index.ts` exporte `mockRouteDefinition`, `sampleRouteAt`, `cameraOnRail`, `selectBiomeAtProgress`, `createRouteFrameSnapshot`, `resolveRouteDefinition`.
- `createRouteFrameSnapshot(progress, route?, cameraConfig?)` accepte un `RideProgress` ou `RideFrameSnapshot`.
- `RouteFrameSnapshot` contient `sample`, `camera`, `biomeId`, `usedFallback`, `fallbackReason`.
- Le fallback route est `placeholder-route`; les routes invalides/malformees ne doivent pas faire crasher le rendu.
- `src/route/*` doit rester pur, hors React et hors Three.js; l'adaptation en `THREE.Vector3` se fait uniquement dans `src/render/*`.

### Architecture a respecter

- React gere le montage du canvas et les boutons d'ecran.
- Three.js gere seulement le rendu depuis des snapshots deja calcules.
- `SceneController.update` applique route, camera, lumiere, fog et placeholders.
- Les stats restent calculees par `src/ride`, pas par le rendu.
- Les biomes restent selectionnes par `src/route`, pas par le rendu.
- Les assets externes sont hors scope; tous les visuels doivent etre proceduraux.

### Consignes Three.js actuelles

- Le projet n'a pas encore `three` dans `package.json`; MYB-5 est la story qui l'ajoute.
- Version npm verifiee le 2026-06-06: `three@0.184.0`.
- La doc officielle recommande l'installation npm avec un build tool comme Vite pour les projets modernes.
- Utiliser `WebGLRenderer`, pas `WebGPURenderer`, pour rester compatible avec le fallback WebGL existant et le scope MVP.
- Pour le resize, laisser CSS definir la taille affichee et appeler `renderer.setSize(width, height, false)`.
- Limiter le pixel ratio ou eviter une resolution interne trop elevee si necessaire pour garder la validation performance pragmatique.
- Three.js ne nettoie pas automatiquement les ressources WebGL: disposer explicitement geometries, materials, textures et renderer.

### Performance caveat obligatoire

Le caveat readiness de `MYB-5` doit rester explicite:

- Validation manuelle seulement: canvas non vide, resize correct, mouvement fluide sur machine de dev.
- Pas de FPS cible chiffre, profiler, stats.js, WebGL inspector, budget draw calls detaille ou tooling perf avance.
- Garder la scene simple: peu de meshes, geometries basiques, pas de textures chargees, pas de shadows avancees, pas de post-processing.

### Decisions de scope pour eviter les derives

- Le mouvement mock peut etre a effort fixe pour MYB-5; le controle utilisateur arrive en MYB-6.
- Ne pas ajouter `MockRideControls`, `RideHud`, `PauseOverlay` final ou `RideSummary` final.
- Ne pas installer Playwright.
- Ne pas ajouter React Three Fiber ou abstractions lourdes: Three.js direct suffit pour le MVP.
- Ne pas ajouter loaders GLTF/DRACO/FBX, Meshy, public assets ou pipeline.
- Ne pas modifier `src/ride/*` ou `src/route/*` pour rendre la scene sauf bug bloquant prouve.

## Tests attendus

- Tests React/jsdom de `ThreeCanvasHost` avec un fake `SceneController` injecte ou factory injectable.
- Tests unitaires des helpers render purs, par exemple:
  - conversion `{ x, y, z }` vers `THREE.Vector3`;
  - palette biome `coast` / `forest` / `placeholder`;
  - creation de snapshot compose depuis `RideFrameSnapshot` + `RouteFrameSnapshot` si helper dedie.
- Tests de lifecycle:
  - mount appelle controller mount;
  - resize appelle controller resize;
  - unmount appelle dispose;
  - pause ne fait pas avancer la progression.
- Ne pas tester WebGL reel dans jsdom; valider le vrai canvas manuellement via dev server.
- Ne pas ajouter Playwright dans MYB-5.

## Project Structure Notes

Fichiers probables a creer:

- `src/render/ThreeCanvasHost.tsx`
- `src/render/SceneController.ts` ou `src/render/createSceneController.ts`
- `src/render/sceneTypes.ts`
- `src/render/createRouteMesh.ts`
- `src/render/createBiomeVisuals.ts`
- `src/render/createLighting.ts`
- `src/render/createPlaceholderAssets.ts`
- `src/render/index.ts`
- `src/render/*.test.ts` pour helpers/lifecycle sans WebGL reel

Fichiers probables a modifier:

- `package.json`
- `package-lock.json`
- `src/app/screens/RideScreen.tsx`
- `src/styles.css`

Fichiers a lire avant modification:

- `src/App.tsx`
- `src/app/screens/RideScreen.tsx`
- `src/app/webgl/webglSupport.ts`
- `src/app/session/sessionReducer.ts`
- `src/ride/index.ts`
- `src/ride/advanceRideFrame.ts`
- `src/ride/mockRideInputSource.ts`
- `src/ride/rideTypes.ts`
- `src/route/index.ts`
- `src/route/createRouteFrameSnapshot.ts`
- `src/route/mockRouteDefinition.ts`
- `src/route/routeTypes.ts`

Fichiers a ne pas modifier sauf bug bloquant prouve:

- `src/ride/*`
- `src/route/*`
- `src/app/session/sessionReducer.ts`

Fichiers a ne pas creer dans MYB-5:

- `src/ui/MockRideControls.tsx`
- `src/ui/RideHud.tsx`
- `src/ui/PauseOverlay.tsx`
- `tests/e2e/*`
- `playwright.config.*`
- `public/assets/*` pour assets externes lourds

## Project Context Rules

- Respecter `AGENTS.md`: scope strict Linear, mock mode preserve, React pour UI/screen state, Three.js pour rendu depuis snapshots.
- BMAD reste file-first: modifier les artifacts localement avant sync Linear.
- Les mises a jour Linear non destructives sur les issues/docs MYB existants sont automatiques.
- Ne pas importer l'ancien backlog complet ni anticiper les stories post-MYB-5.
- Ne jamais committer de secrets, tokens Linear, Meshy, BLE/FTMS credentials ou API keys.

## References

- `_bmad-output/planning-artifacts/echappee-3d-gdd-court.md` - `Vertical Slice Mock`, `Piliers`, `Decisions verrouillees`
- `_bmad-output/planning-artifacts/echappee-3d-architecture-mince.md` - `Modules MVP / render`, `Flux de donnees MVP`, `Risques / mitigations`
- `_bmad-output/planning-artifacts/echappee-3d-mvp-epic-stories.md` - `Story 1.4 - Scene Three.js MVP et rendu de la balade`
- `_bmad-output/planning-artifacts/implementation-readiness-report-2026-06-06.md` - caveat performance MYB-5
- `_bmad-output/implementation-artifacts/1-3-route-prefabriquee-biomes-et-camera-sur-rail.md` - API route et review fixes MYB-4
- `_bmad-output/implementation-artifacts/sprint-status.yaml` - statut local sprint
- `_bmad-output/linear-sync.md` - mapping Linear MYB-5 et dependances
- `src/app/screens/RideScreen.tsx` - point d'integration canvas
- `src/app/webgl/webglSupport.ts` - fallback WebGL existant
- `src/ride/advanceRideFrame.ts` - production de `RideFrameSnapshot`
- `src/route/createRouteFrameSnapshot.ts` - production de `RouteFrameSnapshot`
- Three.js installation docs: https://threejs.org/manual/en/installation.html
- Three.js responsive docs: https://threejs.org/manual/en/responsive.html
- Three.js WebGLRenderer docs: https://threejs.org/docs/pages/WebGLRenderer.html
- Three.js cleanup docs: https://threejs.org/manual/en/cleanup.html

## Questions ouvertes sauvegardees

- La validation performance reste volontairement qualitative pour MYB-5; aucun seuil FPS n'est impose tant que le caveat readiness reste respecte.
- Le mouvement mock a effort fixe est acceptable pour rendre la scene visible avant MYB-6; le controle joueur final arrive avec le slider obligatoire de MYB-6.
- Le niveau exact de decoration biome peut rester minimal tant que `coast`, `forest` et `placeholder` sont visuellement distinguables.

## Dev Agent Record

### Agent Model Used

Codex GPT-5

### Implementation Plan

- Ajouter Three.js minimalement et isoler le cycle de vie WebGL dans `src/render/*`.
- Remplacer le placeholder de `RideScreen` par un `ThreeCanvasHost` mince.
- Composer `advanceRideFrame` + `createRouteFrameSnapshot` pour alimenter le rendu sans dupliquer la logique ride/route.
- Livrer route visible, camera sur rail, biomes placeholders et cleanup/responsive basiques.

### Debug Log References

- 2026-06-06T17:48+02:00 - RED: `npm run test -- src/render` echoue comme attendu car `ThreeCanvasHost` et `renderHelpers` n'existent pas encore.
- 2026-06-06T17:51+02:00 - GREEN cible: `npm run test -- src/render` passe avec 5 tests render.
- 2026-06-06T17:57+02:00 - Validation finale: `npm run typecheck`, `npm run test`, `npm run build`.
- 2026-06-06T17:57+02:00 - Dev server Vite sur `http://127.0.0.1:5174/`, HTTP `200 OK`.
- 2026-06-06T17:58+02:00 - Validation Chrome headless/CDP: captures desktop `/tmp/mybike-myb5-desktop.png` et mobile `/tmp/mybike-myb5-mobile.png`, canvas visible et dimensionne.
- 2026-06-06T18:13+02:00 - Revue `gds-code-review`: patch fallback route malformee cote render et hauteur canvas responsive mobile/desktop.
- 2026-06-06T18:13+02:00 - Validation revue: `npm run typecheck`, `npm run test` (49 tests / 15 fichiers), `npm run build` avec warning Vite chunk Three.js >500 kB attendu.
- 2026-06-06T18:13+02:00 - Validation visuelle revue: Vite HTTP 200, Chrome headless/CDP desktop/mobile, canvas non blank et cadre; crops canvas ImageMagick desktop 228 couleurs, mobile 925 couleurs.

### Completion Notes List

- Ajout de `three@0.184.0` en dependance runtime et de `@types/three@0.184.0` en devDependency car TypeScript exige les declarations.
- Nouveau module `src/render/*`: `ThreeCanvasHost`, `createSceneController`, helpers de palette/conversion, mesh de route, marqueurs biomes et types de snapshot render.
- `ThreeCanvasHost` compose `advanceRideFrame` + `createRouteFrameSnapshot` avec une source mock fixe `0.55`, sans slider, HUD, clavier ni stats UI MYB-6.
- `SceneController` applique la camera route (`position`, `lookAt`, `fovDegrees`), resize la camera/renderer, rend sol/ciel/route/biomes/fallback et dispose renderer, geometries et materials.
- `RideScreen` remplace le placeholder statique par le canvas Three.js en conservant les boutons existants Pause/Reprendre/Terminer et le fallback WebGL global de MYB-2.
- Validation performance pragmatique effectuee sans profiler/tooling avance: ride court en Chrome headless/CDP, canvas non vide visuellement, resize desktop/mobile correct, pas de freeze/crash observe.
- Revue locale passee apres corrections: fallback route malformee securise cote render et hauteur canvas stabilisee pour mobile/desktop.

### File List

- `_bmad-output/implementation-artifacts/1-4-scene-three-js-mvp-et-rendu-de-la-balade.md`
- `_bmad-output/implementation-artifacts/sprint-status.yaml`
- `_bmad-output/linear-sync.md`
- `package.json`
- `package-lock.json`
- `src/app/screens/RideScreen.tsx`
- `src/render/SceneController.ts`
- `src/render/ThreeCanvasHost.test.tsx`
- `src/render/ThreeCanvasHost.tsx`
- `src/render/createBiomeVisuals.ts`
- `src/render/createRouteMesh.ts`
- `src/render/index.ts`
- `src/render/renderHelpers.test.ts`
- `src/render/renderHelpers.ts`
- `src/render/sceneTypes.ts`
- `src/styles.css`

### Change Log

- 2026-06-06 - Story MYB-5 creee: scene Three.js MVP, canvas host, SceneController, placeholders, integration snapshots MYB-3/MYB-4 et caveat performance manuel.
- 2026-06-06 - Implementation MYB-5 terminee: Three.js direct, host React, scene route/sol/biomes/camera, tests render, validation technique et validation visuelle pragmatique.
- 2026-06-06 - Revue MYB-5 approuvee apres deux petites corrections render/CSS et validations finales.

---
story_id: "post-mvp-polish-1"
story_key: "myb-7-polish-ux-sensation-de-ride-mock"
linear_id: "MYB-7"
title: "Polish UX / sensation de ride mock"
status: "done"
created: "2026-06-06"
scope: "post-MVP polish UX / sensation de ride mock"
source_mini_scope: "_bmad-output/implementation-artifacts/polish-ux-ride-feel-mini-scope-2026-06-06.md"
source_change_proposal: "_bmad-output/planning-artifacts/sprint-change-proposal-polish-ux-ride-feel-2026-06-06.md"
source_playtest_results: "_bmad-output/implementation-artifacts/playtest-polish-ux-ride-feel-results-2026-06-06.md"
baseline_commit: "b45b578"
depends_on:
  - "MYB-6"
---

# Story MYB-7: Polish UX / sensation de ride mock

Status: done

## Story

As a joueur de la vertical slice mock,
I want ressentir plus clairement la balade sur mobile, l'effet de mon effort et
la variation d'ambiance,
so that un court playtest juge mieux la sensation de ride sans velo connecte.

## Contexte

`MYB-7` est une story post-MVP unique, creee apres la validation de la boucle
mock jouable. Elle transforme la decision mini-scope polish en travail
implementable, sans rouvrir Epic 1 et sans creer un backlog.

Sources:

- `_bmad-output/implementation-artifacts/polish-ux-ride-feel-mini-scope-2026-06-06.md`
- `_bmad-output/planning-artifacts/sprint-change-proposal-polish-ux-ride-feel-2026-06-06.md`
- `_bmad-output/implementation-artifacts/playtest-polish-ux-ride-feel-results-2026-06-06.md`
- `_bmad-output/implementation-artifacts/sprint-status.yaml`
- `_bmad-output/linear-sync.md`

Preuve de depart du self-playtest:

- HUD mobile lisible mais trop dominant.
- Slider fonctionnel, mais effet effort -> vitesse percu comme trop indirect a
  cause du smoothing.
- Route lisible, mais transition de biome non observable dans le run court.

## Scope strict

Inclus exactement:

1. Compacter le HUD mobile pour liberer la scene.
2. Rendre la reponse du slider plus comprehensible.
3. Rendre le changement de biome observable pendant un playtest court.

Exclus:

- BLE, FTMS, Web Bluetooth, velo connecte.
- Deploiement.
- Bundle/perf avancee.
- Assets IA/Meshy ou pipeline asset lourd.
- Routes multiples, selection route, import GPX, editeur.
- Nouveau systeme de settings.
- Gros redesign HUD ou cockpit sportif.
- Nouvelle boucle ride.
- Nouveau backlog Linear.
- Refactor profond de `src/ride`, `src/route` ou `src/render`.

## Acceptance Criteria

1. En viewport mobile, le HUD occupe moins d'espace vertical et laisse la route
   et le ciel plus visibles pendant la balade.
2. Le HUD conserve les informations essentielles deja livrees: vitesse,
   distance, temps, source `mock`, phase.
3. Le HUD desktop reste lisible et ne regresse pas.
4. Le slider rend plus explicite le lien entre effort mock et allure, par une
   microcopy, un affichage cible/instantane ou une calibration legere.
5. La modification du slider continue de piloter la source mock effectivement
   consommee par `ThreeCanvasHost`; aucune deuxieme simulation n'est creee.
6. Le smoothing reste progressif et sans overshoot; si la calibration change,
   les tests `src/ride/speed.test.ts` documentent les nouvelles bornes.
7. Un playtest court peut observer au moins une variation d'ambiance/biome sans
   nouvelle route complete.
8. Le changement de biome reste gere par les donnees/fonctions de `src/route`;
   `SceneController` ne devient pas responsable du choix de progression.
9. Les changements restent petits et reversibles, focalises sur les fichiers UI,
   CSS, et au plus de petites constantes/tests route ou ride.
10. Le parcours mock start -> ride -> slider -> pause -> resume -> finish ->
    summary reste jouable sans backend, asset externe ou materiel.

## Tasks / Subtasks

- [x] Verifier les contrats existants avant modification. (AC: 5, 8, 9)
  - [x] Lire `src/App.tsx` et `src/app/screens/RideScreen.tsx`.
  - [x] Lire `src/ui/RideHud.tsx`, `src/ui/MockRideControls.tsx` et
    `src/styles.css`.
  - [x] Lire `src/render/ThreeCanvasHost.tsx` pour confirmer la source unique de
    snapshot.
  - [x] Lire `src/ride/speed.ts`, `src/route/mockRouteDefinition.ts` et
    `src/route/selectBiomeAtProgress.ts`.

- [x] Compacter le HUD mobile. (AC: 1, 2, 3)
  - [x] Adapter d'abord `src/styles.css` et/ou le rendu `RideHud` sans gros
    redesign.
  - [x] Garder les donnees essentielles, mais autoriser une presentation mobile
    plus dense ou une hierarchie plus forte.
  - [x] Eviter tout texte qui recouvre incoheremment la route sur petit ecran.
  - [x] Mettre a jour les tests `RideHud` si la structure du texte change.

- [x] Clarifier la reponse du slider. (AC: 4, 5, 6)
  - [x] Preferer une solution UX legere: libelle plus parlant, valeur d'effort
    mieux nommee, ou indication de vitesse cible si disponible.
  - [x] Si le smoothing est ajuste, changer seulement les constantes necessaires
    dans `src/ride/speed.ts` ou la config d'appel, puis mettre a jour les tests.
  - [x] Ne pas calculer vitesse/progression dans React pour expliquer le slider.

- [x] Rendre le biome observable en playtest court. (AC: 7, 8, 9)
  - [x] Preferer une petite adaptation de `mockRouteDefinition.biomes` ou de la
    longueur/seuil de route existant.
  - [x] Ne pas ajouter de route multiple ni de systeme de selection.
  - [x] Mettre a jour les tests `mockRouteDefinition` et
    `selectBiomeAtProgress` si les seuils changent.

- [x] Valider le parcours mock. (AC: 1-10)
  - [x] Lancer les tests proportionnes aux fichiers touches.
  - [x] Si seuls docs/tracking changent, ne pas lancer npm; si code change,
    lancer au minimum `npm run typecheck` et `npm run test`.
  - [x] Faire une validation visuelle courte desktop/mobile si possible:
    route visible, slider comprehensible, biome observe, resume coherent.

### Review Findings

- [x] [Review][Patch] Eviter le calcul d'allure cible dans React — fixe en
  passant `latestSnapshot.ride.targetSpeedMps` a `MockRideControls`.
- [x] [Review][Patch] Eviter les annonces continues de vitesse — fixe en
  retirant `aria-live` du feedback slider.
- [x] [Review][Patch] Preserver la lisibilite mobile du HUD compact — fixe en
  remontant les tailles de texte mobiles tout en gardant le HUD autour de 20%
  du viewport ride.

## Dev Notes

### Etat applicatif actuel

- `src/App.tsx` garde la machine d'ecran via `sessionReducer` et monte
  `RideScreen` seulement en `running/paused`.
- `src/app/screens/RideScreen.tsx` possede `effort01`, recoit les snapshots de
  `ThreeCanvasHost`, affiche `RideHud`, puis rend `MockRideControls`.
- `src/render/ThreeCanvasHost.tsx` cree une seule boucle ride/render, synchronise
  l'effort via `syncInputSourceEffort`, appelle `advanceRideFrame`, compose
  `createRouteFrameSnapshot`, puis emet `onFrame`.
- `src/ui/RideHud.tsx` affiche actuellement 5 metriques dans une grille:
  vitesse, distance, temps, source, phase.
- `src/ui/MockRideControls.tsx` est un `input type="range"` controle de `0` a
  `1`, avec affichage du pourcentage d'effort.
- `src/styles.css` met le HUD en overlay dans `.ride-viewport`; en mobile, le HUD
  passe en grille 2 colonnes, ce qui explique son emprise verticale.
- `src/ride/speed.ts` mappe `effort01` vers `1.5..9 m/s` et lisse avec
  acceleration `2.5 m/s/s`, deceleration `4 m/s/s`.
- `src/route/mockRouteDefinition.ts` a une route de `1000m` et un changement
  `coast -> forest` a `0.45`, donc environ `450m`.
- `src/render/SceneController.ts` applique deja des palettes visuelles selon
  `snapshot.route.biomeId`; ne pas y deplacer la logique de seuil.

### Fichiers probables a modifier

- `src/ui/RideHud.tsx`
- `src/ui/RideHud.test.tsx`
- `src/ui/MockRideControls.tsx`
- `src/ui/MockRideControls.test.tsx`
- `src/styles.css`
- `src/ride/speed.ts`
- `src/ride/speed.test.ts`
- `src/route/mockRouteDefinition.ts`
- `src/route/mockRouteDefinition.test.ts`
- `src/route/selectBiomeAtProgress.test.ts`

Cette liste est indicative. Ne modifier `src/ride` ou `src/route` que si le
polish l'exige vraiment, et garder chaque changement testable.

### Architecture a respecter

- React gere UI, controles, HUD, summary et etat d'ecran.
- `src/ride/*` reste pur, sans React ni Three.js.
- `src/route/*` reste pur, sans React ni Three.js.
- `src/render/*` rend des snapshots; il ne gere ni HUD, ni microcopy, ni
  controles.
- Le mode mock reste le produit MVP; aucune source BLE/FTMS n'est introduite.

### Tests attendus

- Tests React/jsdom pour HUD et slider si le texte ou la structure change.
- Tests unitaires ride si smoothing ou constantes vitesse changent.
- Tests unitaires route si seuil/longueur biome changent.
- Validation manuelle desktop/mobile courte apres implementation code.

## Definition of Done

- Les 3 ajustements retenus sont implementes, sans ajout de scope.
- Les tests proportionnes aux fichiers modifies passent.
- Le parcours mock reste jouable de bout en bout.
- Le Dev Agent Record documente les fichiers modifies et la validation executee.

## Dev Agent Record

### Agent Model Used

GPT-5 Codex

### Debug Log References

- `npm run typecheck` — passed.
- `npm run test` — passed, 19 files / 62 tests.
- `npm run build` — passed; Vite chunk-size warning retained for the Three.js
  MVP bundle.
- Vite local server `http://127.0.0.1:5174/` — HTTP 200.
- Chrome/CDP desktop ride — WebGL true, canvas non blank, HUD readable,
  slider feedback visible, `Ambiance forest` visible.
- Chrome/CDP mobile-sized ride — WebGL true, screenshot non blank
  (`colors=3816`), HUD ratio about 23% of ride viewport, pause/resume/finish
  summary validated.
- Console desktop/mobile validation — no warnings or errors observed.
- Review rerun after fixes — `npm run typecheck`, `npm run test` (19 files /
  62 tests), and `npm run build` passed; Vite chunk-size warning retained for
  the Three.js MVP bundle.
- Review Chrome/CDP desktop + mobile — WebGL true, slider target/displayed speed
  fed from the ride snapshot, `Ambiance forest` observable in about 4 seconds,
  pause/resume/finish/summary OK, mobile HUD ratio about 20%, no warning/error.

### Completion Notes

- HUD mobile compacted with denser mobile CSS while preserving speed, distance,
  time, source and phase. Added `Ambiance` as a lightweight snapshot readout so
  the biome transition is directly observable during a short playtest.
- Slider now displays target speed versus currently displayed speed, making the
  smoothing response understandable without adding a second simulation loop.
- Biome transition threshold moved from 45% to 1% of the existing mock route;
  route selection still lives in `src/route`, and `SceneController` remains a
  renderer of snapshots.
- No BLE/FTMS, deployment, new route, new settings system, large redesign or new
  Linear issue was introduced.
- Code review fixes kept inside MYB-7 scope:
  - removed continuous `aria-live` feedback from the changing speed readout;
  - raised mobile HUD text sizes while keeping the HUD compact;
  - moved slider target speed display to `latestSnapshot.ride.targetSpeedMps`
    instead of calculating a parallel target in React.

### File List

- `src/app/screens/RideScreen.tsx`
- `src/ui/RideHud.tsx`
- `src/ui/RideHud.test.tsx`
- `src/ui/MockRideControls.tsx`
- `src/ui/MockRideControls.test.tsx`
- `src/styles.css`
- `src/route/mockRouteDefinition.ts`
- `src/route/mockRouteDefinition.test.ts`
- `src/route/selectBiomeAtProgress.test.ts`
- `_bmad-output/implementation-artifacts/myb-7-polish-ux-sensation-de-ride-mock.md`

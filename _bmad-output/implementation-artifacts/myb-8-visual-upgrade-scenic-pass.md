---
story_id: "post-mvp-visual-1"
story_key: "myb-8-visual-upgrade-scenic-pass"
linear_id: "MYB-8"
linear_url: "https://linear.app/kefjbo/issue/MYB-8/visual-upgrade-scenic-pass"
title: "Visual Upgrade Scenic Pass"
status: "done"
created: "2026-06-06"
baseline_commit: "48b647e0613b2d7362ec4f65d808ccdc911a1c6c"
scope: "post-MVP visual scenic pass"
source_change_proposal: "_bmad-output/planning-artifacts/sprint-change-proposal-visual-upgrade-scenic-pass-2026-06-06.md"
source_visual_audit: "_bmad-output/implementation-artifacts/visual-audit-video-ride-2026-06-06.md"
source_previous_retro: "_bmad-output/implementation-artifacts/myb-7-polish-mini-retro-2026-06-06.md"
depends_on:
  - "MYB-7"
---

# Story MYB-8: Visual Upgrade Scenic Pass

Status: done

## Story

As a joueur de la vertical slice mock,
I want la route et le paysage donnent immediatement une impression de balade
scenic stylisee,
so that une capture courte montre une scene plus riche, plus profonde et plus
agreable sans changer la boucle de ride.

## Contexte

`MYB-8` est une story post-MVP unique issue du cadrage
`Visual Upgrade Scenic Pass`. La vertical slice est jouable et MYB-7 a clarifie
la sensation de ride, mais l'audit video montre que l'image reste trop plate:
route ruban, decor lateral sparse, peu de reperes proches, horizon faible et
manque de profondeur.

Cette story ne doit pas ouvrir un nouvel epic graphique ni un backlog. Elle doit
ameliorer la composition de scene avec des moyens simples, principalement dans
`src/render/*`, en gardant le mode mock et le happy path existants.

Sources:

- `_bmad-output/planning-artifacts/sprint-change-proposal-visual-upgrade-scenic-pass-2026-06-06.md`
- `_bmad-output/implementation-artifacts/visual-audit-video-ride-2026-06-06.md`
- `_bmad-output/implementation-artifacts/myb-7-polish-mini-retro-2026-06-06.md`
- `_bmad-output/implementation-artifacts/sprint-status.yaml`
- `_bmad-output/linear-sync.md`
- `src/render/*`
- `src/route/*`
- `src/ride/*`

## Scope strict

Inclus, avec 4 leviers maximum:

1. Route enrichie: bas-cotes, surface/texture simple, marquage central/lateral
   ou variation visuelle legere.
2. Decor lateral procedural: arbres, rochers, herbes, poteaux ou panneaux
   simples, instancies le long de la route.
3. Profondeur scenic: horizon plus travaille, brume, plans proches/moyens/
   lointains et variation terrain simple.
4. Meshy optionnel et cible: 2 a 4 assets low-poly maximum, seulement s'ils sont
   utiles, instanciables, remplacables et non bloquants.

Exclus:

- BLE, FTMS, Web Bluetooth, velo connecte.
- Deploiement.
- Routes multiples, selection route, editeur ou import GPX.
- Refonte HUD ou cockpit sportif.
- Optimisation bundle avancee, sauf blocage reel.
- Meshy massif ou bibliotheque d'assets IA.
- Pipeline glTF complet si non necessaire.
- Pipeline asset lourd ou asset manager.
- Nouveau gameplay, nouvelle boucle ride ou nouveau backlog Linear.
- Refactor profond de `src/ride`.
- Appel Meshy payant sans confirmation explicite du cout, du format cible et de
  l'usage prevu.

## Acceptance Criteria

1. Une capture locale courte montre une route moins unie, avec bas-cotes,
   surface, marquage ou variation clairement visible.
2. La scene contient des reperes lateraux proches et moyens qui renforcent la
   sensation de mouvement et de parallax sans changer le gameplay.
3. L'horizon, la brume, le terrain ou les plans lointains donnent plus de
   profondeur que la scene actuelle.
4. Les biomes restent compatibles avec les snapshots existants:
   `SceneController` rend `snapshot.route.biomeId`, il ne choisit pas la
   progression, la vitesse ou les stats.
5. Les ajouts visuels restent proceduraux ou remplacables par placeholders; si
   Meshy est utilise, il reste optionnel, limite a 2-4 assets low-poly
   instancies et sans pipeline lourd.
6. Le mode mock et le happy path start -> ride -> slider -> pause -> resume ->
   finish -> summary restent jouables sans materiel, backend, asset externe
   obligatoire ou nouveau service.
7. `src/ride` n'est pas modifie profondement; toute modification ride doit etre
   justifiee, pure, testee et strictement necessaire a la sensation visuelle.
8. Les ressources Three.js ajoutees sont trackees et disposees au demontage.
9. Les tests proportionnes aux fichiers touches passent, avec au minimum
   `npm run typecheck`, `npm run test` et `npm run build` si du code applicatif
   est modifie.
10. Une preuve visuelle courte desktop, et mobile si raisonnable, confirme que
    le canvas reste non vide et que la scene gagne en densite/profondeur.

## Tasks / Subtasks

- [x] Verifier les contrats existants avant modification. (AC: 4, 6, 7)
  - [x] Lire `src/render/SceneController.ts`.
  - [x] Lire `src/render/createRouteMesh.ts`.
  - [x] Lire `src/render/createBiomeVisuals.ts`.
  - [x] Lire `src/render/renderHelpers.ts` et `src/render/sceneTypes.ts`.
  - [x] Lire `src/route/mockRouteDefinition.ts` et
    `src/route/createRouteFrameSnapshot.ts`.
  - [x] Lire `src/ride/rideTypes.ts` uniquement pour verifier les snapshots;
    ne pas concevoir la story depuis `src/ride`.

- [x] Enrichir la route sans changer la boucle ride. (AC: 1, 4, 6)
  - [x] Ajouter une solution simple de bas-cotes, bandes laterales, marquage ou
    variation de surface dans le rendu.
  - [x] Garder `createRouteMesh` ou un helper voisin responsable de la geometrie
    visuelle, sans deplacer la progression dans le rendu.
  - [x] Eviter tout systeme de route multiple ou generation procedurale lourde.

- [x] Ajouter du decor lateral procedural. (AC: 2, 4, 5, 8)
  - [x] Ajouter quelques familles simples et lisibles: arbres, rochers, herbes,
    poteaux ou panneaux.
  - [x] Preferer l'instanciation/reutilisation de geometries et materiaux.
  - [x] Varier positions, tailles et plans proches/moyens sans charger d'assets
    externes obligatoires.
  - [x] Disposer toutes les geometries/materiaux ajoutes.

- [x] Renforcer la profondeur scenic. (AC: 3, 10)
  - [x] Ajuster horizon, fog, ground/terrain ou plans lointains pour reduire
    l'effet d'aplats.
  - [x] Garder une scene confortable en premiere personne: pas de camera libre,
    pas de mouvement brutal.
  - [x] Ne modifier camera/FOV que si necessaire et avec validation visuelle.

- [x] Garder Meshy optionnel. (AC: 5)
  - [x] Si Meshy est utile, limiter a 2-4 assets low-poly maximum.
  - [x] Garder un fallback procedural equivalent et une integration simple.
  - [x] Ne pas creer de pipeline glTF complet, d'asset manager ou de workflow
    generation obligatoire.
  - [x] Avant tout appel Meshy payant, confirmer le cout en credits, l'usage
    cible et le format attendu; sinon rester sur les placeholders proceduraux.

- [x] Valider le happy path et la scene. (AC: 6, 9, 10)
  - [x] Lancer `npm run typecheck`.
  - [x] Lancer `npm run test`.
  - [x] Lancer `npm run build`.
  - [x] Faire une validation visuelle courte desktop, et mobile si possible:
    route enrichie, reperes lateraux, horizon/profondeur, pause/reprise/finish
    toujours OK.

### Review Findings

- [x] [Review][Approved] Clean review — aucun patch requis; les 4 leviers
  MYB-8 sont visibles, le scope reste borne, `src/ride/*` est intact, Meshy n'a
  pas ete utilise et les ressources Three.js ajoutees sont trackees pour
  disposal.

## Dev Notes

### Etat applicatif actuel

- `src/render/SceneController.ts` cree une scene Three.js directe avec ground,
  horizon, route mesh et groupe de marqueurs biome. Il applique background, fog,
  couleurs route/ground/horizon et camera depuis `RenderFrameSnapshot`.
- `src/render/createRouteMesh.ts` cree aujourd'hui un ruban de route unique de
  5 m de large depuis les points de route. C'est le meilleur point de depart
  pour enrichir route, bas-cotes ou marquages sans toucher a la progression.
- `src/render/createBiomeVisuals.ts` contient deja des placeholders simples:
  marqueurs cote et arbres forest. Il peut etre etendu ou remplace par un helper
  procedural plus riche tant que les ressources restent disposees.
- `src/render/renderHelpers.ts` centralise les palettes et la conversion `Vec3`
  -> `THREE.Vector3`. Les palettes peuvent porter fog/couleurs plus riches.
- `src/route/mockRouteDefinition.ts` definit une route de 1000 m avec transition
  coast -> forest a 1% depuis MYB-7 pour rendre le biome observable en playtest
  court.
- `src/route/createRouteFrameSnapshot.ts` compose sample, camera et biome. MYB-8
  doit consommer ce snapshot, pas refaire la selection de biome dans le rendu.
- `src/ride/rideTypes.ts` expose `RideFrameSnapshot` avec input, target speed,
  speed, progress, elapsed et stats. MYB-8 ne doit pas recalculer ces valeurs.
- Le workflow d'audit visuel a deja ajoute `npm run capture:ride-video` et
  `scripts/capture-ride-video.mjs`; le reutiliser si une capture 30s est utile
  au lieu d'inventer un nouveau harnais.

### Architecture a respecter

- React gere UI, controles, HUD, summary et etat d'ecran.
- `src/ride/*` reste pur, sans React ni Three.js.
- `src/route/*` reste pur, sans React ni Three.js.
- `src/render/*` peut importer Three.js et doit rendre des snapshots existants.
- Les assets externes ou generes doivent rester optionnels et remplacables.
- Le mode mock reste le mode produit MVP; aucune source BLE/FTMS n'est ajoutee.

### Fichiers probables a modifier

- `src/render/SceneController.ts`
- `src/render/createRouteMesh.ts`
- `src/render/createBiomeVisuals.ts`
- `src/render/renderHelpers.ts`
- `src/render/renderHelpers.test.ts`
- `src/render/ThreeCanvasHost.test.tsx` si le contrat de rendu change
- `src/route/mockRouteDefinition.ts` seulement pour petites donnees scenic
- tests `src/route/*` seulement si les donnees route changent

Cette liste est indicative. Eviter `src/ride/*` sauf besoin strictement justifie.

### Tests attendus

- Tests render/helper si de nouveaux helpers calculent positions, palettes ou
  ressources.
- Tests route si les donnees de route ou biomes changent.
- Validation browser courte pour verifier densite visuelle, profondeur, canvas
  non vide et happy path mock.
- `npm run typecheck`, `npm run test`, `npm run build` des qu'un fichier code est
  modifie.

## Definition of Done

- Les 3 leviers principaux route + decor lateral + profondeur sont visibles.
- Le levier Meshy reste absent ou strictement optionnel/cible.
- Aucun pipeline asset lourd ni nouveau backlog n'est cree.
- Le happy path MYB-7 reste jouable.
- Les validations executees sont notees dans le Dev Agent Record.
- Linear MYB-8 et `_bmad-output/linear-sync.md` restent synchronises.

## Dev Agent Record

### Agent Model Used

GPT-5 Codex

### Debug Log References

- `npm run test -- src/render/createRouteMesh.test.ts src/render/createBiomeVisuals.test.ts`: RED confirme avant implementation, 4 echecs attendus.
- `npm run test -- src/render/createRouteMesh.test.ts src/render/createBiomeVisuals.test.ts src/render/renderHelpers.test.ts`: GREEN, 8 tests render.
- `npm run typecheck`: passe.
- `npm run test`: passe, 21 fichiers / 66 tests.
- `npm run build`: passe avec l'avertissement Vite connu sur le chunk Three.js > 500 kB.
- Vite local: HTTP 200 sur `http://127.0.0.1:5174/`.
- Capture video: `_bmad-output/video-captures/ride-visual-audit-2026-06-06T20-56-54-899Z/ride-visual-audit-30s.mp4`.
- Contact sheet: `_bmad-output/video-captures/ride-visual-audit-2026-06-06T20-56-54-899Z/ride-visual-audit-contact-sheet.jpg`.
- Review `npm run typecheck`: passe.
- Review `npm run test`: passe, 21 fichiers / 66 tests.
- Review `npm run build`: passe avec l'avertissement Vite connu sur le chunk
  Three.js > 500 kB.
- Review HTTP 200 sur `http://127.0.0.1:5174/`.
- Review capture video:
  `_bmad-output/video-captures/ride-visual-audit-2026-06-06T21-06-51-582Z/ride-visual-audit-30s.mp4`.
- Review contact sheet:
  `_bmad-output/video-captures/ride-visual-audit-2026-06-06T21-06-51-582Z/ride-visual-audit-contact-sheet.jpg`.
- Review non-blank evidence: final frame `1440x900 mean=53106.4 stddev=15014.9`,
  contact sheet `1920x200 mean=53599.1 stddev=13965.9`.

### Completion Notes

- Route enrichie dans `createRouteMesh`: groupe scenic avec surface, bas-cotes,
  bandes laterales, marquage central et variations de surface procedurales.
- Decor lateral procedural densifie dans `createBiomeVisuals`: poteaux coast,
  herbes, rochers, arbres proches/moyens, panneaux, arbres et collines lointains,
  avec geometries/materiaux partages et trackes pour disposal.
- Profondeur scenic renforcee dans `SceneController`: route group complete,
  terrain plus large avec variation douce, fog plus profonde et horizon plus
  present via palette dediee.
- Meshy non utilise: aucun cout, aucun asset externe, aucun pipeline glTF ou
  asset manager ajoute.
- `src/ride/*` non modifie; la progression, la vitesse et les stats restent
  consommees depuis les snapshots existants.
- Validation visuelle desktop: canvas non vide, route plus riche, elements
  lateraux visibles, horizon/profondeur ameliores, mouvement plus perceptible,
  aucune page error; un 404 isole reste observe comme sur l'audit precedent.
- Validation mobile: HTTP 200, start -> slider -> pause -> resume -> finish ->
  summary OK, screenshot non blank; warnings WebGL limites au screenshot
  `ReadPixels`.
- Code review approved: les 4 leviers sont presents, la densite procedural reste
  raisonnable, aucune fuite evidente de geometrie/materiau n'a ete relevee et
  aucune correction code n'a ete necessaire.

### File List

- `_bmad-output/implementation-artifacts/myb-8-visual-upgrade-scenic-pass.md`
- `_bmad-output/implementation-artifacts/sprint-status.yaml`
- `_bmad-output/linear-sync.md`
- `src/render/SceneController.ts`
- `src/render/createBiomeVisuals.ts`
- `src/render/createBiomeVisuals.test.ts`
- `src/render/createRouteMesh.ts`
- `src/render/createRouteMesh.test.ts`
- `src/render/renderHelpers.ts`
- `src/render/renderHelpers.test.ts`

### Change Log

- 2026-06-06: Implementation MYB-8 terminee et mise en review avec route
  enrichie, decor lateral procedural, profondeur scenic et validations completes.
- 2026-06-06: Code review MYB-8 approuvee; story passee a done.

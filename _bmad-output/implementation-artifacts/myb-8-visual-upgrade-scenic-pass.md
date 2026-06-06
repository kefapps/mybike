---
story_id: "post-mvp-visual-1"
story_key: "myb-8-visual-upgrade-scenic-pass"
linear_id: "MYB-8"
linear_url: "https://linear.app/kefjbo/issue/MYB-8/visual-upgrade-scenic-pass"
title: "Visual Upgrade Scenic Pass"
status: "ready-for-dev"
created: "2026-06-06"
scope: "post-MVP visual scenic pass"
source_change_proposal: "_bmad-output/planning-artifacts/sprint-change-proposal-visual-upgrade-scenic-pass-2026-06-06.md"
source_visual_audit: "_bmad-output/implementation-artifacts/visual-audit-video-ride-2026-06-06.md"
source_previous_retro: "_bmad-output/implementation-artifacts/myb-7-polish-mini-retro-2026-06-06.md"
depends_on:
  - "MYB-7"
---

# Story MYB-8: Visual Upgrade Scenic Pass

Status: ready-for-dev

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

- [ ] Verifier les contrats existants avant modification. (AC: 4, 6, 7)
  - [ ] Lire `src/render/SceneController.ts`.
  - [ ] Lire `src/render/createRouteMesh.ts`.
  - [ ] Lire `src/render/createBiomeVisuals.ts`.
  - [ ] Lire `src/render/renderHelpers.ts` et `src/render/sceneTypes.ts`.
  - [ ] Lire `src/route/mockRouteDefinition.ts` et
    `src/route/createRouteFrameSnapshot.ts`.
  - [ ] Lire `src/ride/rideTypes.ts` uniquement pour verifier les snapshots;
    ne pas concevoir la story depuis `src/ride`.

- [ ] Enrichir la route sans changer la boucle ride. (AC: 1, 4, 6)
  - [ ] Ajouter une solution simple de bas-cotes, bandes laterales, marquage ou
    variation de surface dans le rendu.
  - [ ] Garder `createRouteMesh` ou un helper voisin responsable de la geometrie
    visuelle, sans deplacer la progression dans le rendu.
  - [ ] Eviter tout systeme de route multiple ou generation procedurale lourde.

- [ ] Ajouter du decor lateral procedural. (AC: 2, 4, 5, 8)
  - [ ] Ajouter quelques familles simples et lisibles: arbres, rochers, herbes,
    poteaux ou panneaux.
  - [ ] Preferer l'instanciation/reutilisation de geometries et materiaux.
  - [ ] Varier positions, tailles et plans proches/moyens sans charger d'assets
    externes obligatoires.
  - [ ] Disposer toutes les geometries/materiaux ajoutes.

- [ ] Renforcer la profondeur scenic. (AC: 3, 10)
  - [ ] Ajuster horizon, fog, ground/terrain ou plans lointains pour reduire
    l'effet d'aplats.
  - [ ] Garder une scene confortable en premiere personne: pas de camera libre,
    pas de mouvement brutal.
  - [ ] Ne modifier camera/FOV que si necessaire et avec validation visuelle.

- [ ] Garder Meshy optionnel. (AC: 5)
  - [ ] Si Meshy est utile, limiter a 2-4 assets low-poly maximum.
  - [ ] Garder un fallback procedural equivalent et une integration simple.
  - [ ] Ne pas creer de pipeline glTF complet, d'asset manager ou de workflow
    generation obligatoire.
  - [ ] Avant tout appel Meshy payant, confirmer le cout en credits, l'usage
    cible et le format attendu; sinon rester sur les placeholders proceduraux.

- [ ] Valider le happy path et la scene. (AC: 6, 9, 10)
  - [ ] Lancer `npm run typecheck`.
  - [ ] Lancer `npm run test`.
  - [ ] Lancer `npm run build`.
  - [ ] Faire une validation visuelle courte desktop, et mobile si possible:
    route enrichie, reperes lateraux, horizon/profondeur, pause/reprise/finish
    toujours OK.

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

TBD

### Debug Log References

TBD

### Completion Notes

TBD

### File List

TBD

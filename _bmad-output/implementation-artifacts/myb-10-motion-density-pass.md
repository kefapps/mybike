---
story_id: "post-mvp-visual-3"
story_key: "myb-10-motion-density-pass"
linear_id: "MYB-10"
linear_url: "https://linear.app/kefjbo/issue/MYB-10/motion-and-density-pass"
title: "Motion & Density Pass"
status: "ready-for-dev"
created: "2026-06-07"
baseline_commit: "eb51de7fc92e12ce486b414c4bd74f1c8f8972ce"
source_previous_commit: "4963300704e51da458f714809498aa355fba869b"
scope: "post-MYB-9 motion, parallax and roadside density pass"
source_proposal: "_bmad-output/planning-artifacts/proposition-motion-density-pass-2026-06-07.md"
source_capture: "_bmad-output/video-captures/ride-visual-audit-2026-06-07T05-24-07-932Z/"
source_comparison_stack: "_bmad-output/video-captures/ride-visual-audit-2026-06-07T05-24-07-932Z/visual-comparison-stack.jpg"
depends_on:
  - "MYB-9"
---

# Story MYB-10: Motion & Density Pass

Status: ready-for-dev

## Story

As a joueur de la vertical slice mock,
I want la scene proche, la route et les bas-cotes donnent une sensation plus
dense et plus rapide en mouvement,
so that une capture 30s paraisse moins prototype sans changer la logique de
ride, sans Meshy et sans asset externe.

## Contexte

`MYB-9` est Done et commit (`4963300704e51da458f714809498aa355fba869b`).
La comparaison actuelle confirme qu'il n'y a pas de regression depuis MYB-9 et
que le rendu est plus identifiable qu'avant MYB-8/MYB-9. Le manque restant est
plus specifique: le decor reste repetitif, les materiaux sont plats, il y a peu
de vie proche camera et la sensation de vitesse reste limitee.

Cette story transforme la decision `Motion & Density Pass` en travail
implementable. Elle doit rester une seule petite story de rendu procedural, sans
rouvrir un backlog visuel large. Meshy n'est pas prioritaire maintenant: le
besoin est mouvement, densite, parallax, variations et lisibilite de scene.

Sources:

- `_bmad-output/planning-artifacts/proposition-motion-density-pass-2026-06-07.md`
- `_bmad-output/video-captures/ride-visual-audit-2026-06-07T05-24-07-932Z/`
- `_bmad-output/video-captures/ride-visual-audit-2026-06-07T05-24-07-932Z/visual-comparison-stack.jpg`
- `_bmad-output/implementation-artifacts/myb-9-scenic-mood-pass-procedural.md`
- `_bmad-output/implementation-artifacts/sprint-status.yaml`
- `_bmad-output/linear-sync.md`
- `src/render/*`
- `scripts/capture-ride-video.mjs`

## Scope strict

Inclus:

1. Parallax proche visible.
   - Ajouter des elements proches camera qui defilent lisiblement sur les
     bords de route.
   - Leur densite doit etre bornee et composee, pas une multiplication aleatoire
     de props.
2. Densite roadside controlee.
   - Augmenter la vie proche/moyenne route avec herbes, potelets, petits
     rochers, talus, panneaux ou silhouettes simples.
   - Conserver une lecture claire du chemin et des landmarks MYB-9.
3. Variations visuelles route/sol.
   - Ajouter des motifs proceduraux simples de surface, accotements, bandes,
     ruptures legeres ou variations de sol.
   - Eviter les materiaux plats sans introduire de texture externe.
4. Sensation de vitesse cote rendu.
   - Renforcer le ressenti par parallax, repetition composee, placements proches
     et eventuels micro-reperes visuels.
   - Ne pas modifier le mapping vitesse, le smoothing, la progression ou les
     stats dans `src/ride/*`.
5. Option capture sans HUD.
   - Si simple et utile, ajouter un mode de capture sans HUD pour analyser le
     rendu.
   - L'option doit etre legere, isolee au chemin capture/UI, et ne doit pas
     devenir une refonte HUD.

Exclus:

- Meshy.
- Asset externe, image externe, glTF ou pipeline asset lourd.
- BLE, FTMS, Web Bluetooth, backend, persistance ou deploiement.
- Route multiple, route selector, editeur ou import GPX.
- Refonte HUD, sauf option capture sans HUD tres legere.
- Nouveau gameplay, steering lateral, collisions ou physique velo.
- Modification de `src/ride/*`.
- Creation d'un backlog large, nouvel epic ou plusieurs stories.

## Acceptance Criteria

1. Une capture 30s montre au moins deux familles d'elements proches camera qui
   defilent avec un parallax lisible sur les bas-cotes.
2. La densite roadside augmente sans nuire a la lisibilite de la route: le
   chemin principal reste clair, et les motifs MYB-9 restent visibles.
3. La route et/ou le sol affichent des variations procedurales visibles:
   accotements, bandes, motifs de surface, ruptures legeres ou variations de
   couleur/relief, sans texture externe.
4. La sensation de vitesse est renforcee uniquement par le rendu: aucune
   modification dans `src/ride/*`, aucune alteration de vitesse, smoothing,
   progression, stats ou input mock.
5. Les ajouts restent bornes et maintenables: pas de generation non controlee,
   pas d'explosion de mesh/material, pas de dependance externe.
6. Les ressources Three.js ajoutees sont trackees et disposees au demontage via
   les mecanismes existants ou un helper equivalent.
7. Le rendu continue de consommer les snapshots existants:
   `snapshot.route.camera`, `snapshot.route.biomeId` et `usedFallback`; il ne
   recalcule pas la logique ride ou route.
8. Le fallback route/biome reste lisible et ne crashe pas si une route invalide
   active le placeholder.
9. Si le mode capture sans HUD est ajoute, il reste opt-in et ne modifie pas le
   HUD normal du joueur.
10. Les validations d'implementation passent: `npm run typecheck`,
    `npm run test`, `npm run build`, puis `npm run capture:ride-video` pour une
    preuve visuelle comparable.
11. La capture finale documentee contient HTTP `200`, aucune page error, canvas
    non vide, et une comparaison visuelle indiquant parallax/densite/variations.

## Tasks / Subtasks

- [ ] Verifier les contrats existants avant implementation. (AC: 4, 6, 7, 8)
  - [ ] Lire `src/render/SceneController.ts`.
  - [ ] Lire `src/render/createBiomeVisuals.ts`.
  - [ ] Lire `src/render/createRouteMesh.ts`.
  - [ ] Lire `src/render/renderHelpers.ts` et `src/render/sceneTypes.ts`.
  - [ ] Lire les tests render existants avant de modifier.
  - [ ] Confirmer que `src/ride/*` reste hors surface d'implementation.

- [ ] Ajouter du parallax proche borne. (AC: 1, 2, 5, 6)
  - [ ] Ajouter ou extraire un helper procedural pour les near-road details si
    `createBiomeVisuals.ts` devient trop gros.
  - [ ] Placer des objets proches a intervalles controles le long de la route,
    en utilisant `getRouteCenterXAtZ` ou les frames route existantes.
  - [ ] Garder les geometries et materials partages, nommes, et trackes pour
    disposal.
  - [ ] Eviter les placements qui traversent la route ou masquent le centre.

- [ ] Ajouter une densite roadside composee. (AC: 2, 5, 8)
  - [ ] Varier les familles proches/moyennes sans depasser une petite liste de
    props proceduraux simples.
  - [ ] Distinguer les zones coast/forest par composition si possible, sans
    redefinir les biomes.
  - [ ] Preserver les landmarks MYB-9: phare, arche, tunnel forest et relief.

- [ ] Ajouter des variations route/sol. (AC: 3, 5, 6)
  - [ ] Enrichir `createRouteMesh.ts` avec motifs de surface, accotements ou
    micro-bandes si c'est le meilleur point d'extension.
  - [ ] Ou enrichir le ground/terrain dans `SceneController.ts` si la variation
    de sol est plus claire cote terrain.
  - [ ] Tester que les geometries/materiaux ajoutes sont inclus dans le retour
    `RouteMeshResult` ou disposes par le controller.

- [ ] Renforcer la sensation de vitesse sans toucher au ride. (AC: 4, 7)
  - [ ] Utiliser uniquement des reperes visuels proches, densite, echelle,
    parallax et composition.
  - [ ] Ne pas modifier `mapMockInputToSpeed`, `smoothSpeed`,
    `advanceRideFrame`, `advanceRouteProgress` ou les stats.
  - [ ] Si un changement de camera est envisage, verifier qu'il reste cote
    render/capture et ne remplace pas `cameraOnRail`.

- [ ] Option capture sans HUD, seulement si simple. (AC: 9)
  - [ ] Preferer un flag capture/query param/env dans le chemin de capture plutot
    qu'une option UI visible.
  - [ ] Isoler le changement a `scripts/capture-ride-video.mjs`,
    `src/app/screens/RideScreen.tsx` ou `src/styles.css` si necessaire.
  - [ ] Ne pas remodeler `RideHud` ou les controles mock pour le joueur.

- [ ] Ajouter les tests attendus. (AC: 1, 2, 3, 5, 6, 9)
  - [ ] Tests render sur comptage/noms des near-road details et densite bornee.
  - [ ] Tests route mesh/ground si de nouvelles variations ou geometries sont
    ajoutees.
  - [ ] Tests option capture sans HUD uniquement si l'option est implementee.
  - [ ] Verifier qu'aucun test n'importe `src/ride/*` depuis `src/render/*`.

- [ ] Valider et documenter. (AC: 10, 11)
  - [ ] Lancer `npm run typecheck`.
  - [ ] Lancer `npm run test`.
  - [ ] Lancer `npm run build`.
  - [ ] Lancer `npm run capture:ride-video`.
  - [ ] Comparer la nouvelle capture a
    `_bmad-output/video-captures/ride-visual-audit-2026-06-07T05-24-07-932Z/visual-comparison-stack.jpg`
    ou produire une nouvelle comparaison si necessaire.
  - [ ] Mettre a jour cette story, `sprint-status.yaml` et
    `_bmad-output/linear-sync.md` avec les preuves.

## Dev Notes

### Etat actuel apres MYB-9

- `src/render/SceneController.ts` cree la scene Three.js, le ground procedural,
  l'horizon, la fog, la lumiere, la route scenic et les visuels biome. Le ground
  contient deja bermes, slopes, ridges et waves.
- `src/render/createBiomeVisuals.ts` contient deja decor lateral, lighthouse,
  rock arch, berms, ridges, grass, rocks, trees, forest tunnel, signs, far trees
  et hills. MYB-10 doit composer plus finement les elements proches, pas refaire
  toute l'identite scenic.
- `src/render/createRouteMesh.ts` contient surface, shoulders, edge bands,
  center markings et surface bands. C'est le point naturel pour variations
  route/sol si elles sont liees a la route.
- `src/render/renderHelpers.ts` porte palettes, conversion Three.js et
  `getRouteCenterXAtZ`. Ne modifier les palettes que si cela sert clairement la
  lisibilite des nouveaux motifs.
- `scripts/capture-ride-video.mjs` lance Vite, joue un scenario slider
  45/72/92, genere mp4/webm/contact sheet/frames et `capture-summary.json`.
  C'est le point naturel pour une option capture sans HUD si elle reste simple.

### Architecture a respecter

- React gere UI, HUD, controles et etats d'ecran.
- `src/ride/*` reste pur, sans React ni Three.js, et hors scope MYB-10.
- `src/route/*` reste pur, sans React ni Three.js. MYB-10 ne doit pas creer de
  route multiple ni modifier la progression.
- `src/render/*` peut importer Three.js et doit rendre des snapshots existants.
- Le mode mock reste le produit cible de la vertical slice.
- Tout mesh/material/geometry ajoute doit etre tracke pour disposal.

### Fichiers probables

- `src/render/createBiomeVisuals.ts`
- `src/render/createBiomeVisuals.test.ts`
- `src/render/createRouteMesh.ts`
- `src/render/createRouteMesh.test.ts`
- `src/render/SceneController.ts`
- `src/render/renderHelpers.ts` et `src/render/renderHelpers.test.ts` seulement
  si une variation palette/mood est strictement utile
- `scripts/capture-ride-video.mjs` seulement pour l'option capture sans HUD
- `src/app/screens/RideScreen.tsx` ou `src/styles.css` seulement si l'option
  capture sans HUD exige un flag UI/CSS minimal

Eviter:

- `src/ride/*`
- BLE/FTMS/backend/persistence files, s'ils apparaissent plus tard
- assets externes ou dossiers de pipeline

### Tests attendus

- Tests render unitaires sur near-road details: noms, quantite minimale,
  densite bornee, offsets hors route et disposal resources.
- Tests route mesh si les variations route/sol ajoutent de nouvelles geometries
  ou materials.
- Tests capture-HUD uniquement si l'option capture sans HUD est implementee.
- Validation visuelle obligatoire via `npm run capture:ride-video` pendant
  implementation.

## Definition of Done

- MYB-10 ajoute un parallax proche lisible en capture 30s.
- MYB-10 ajoute une densite roadside plus vivante mais controlee.
- MYB-10 ajoute des variations route/sol sans asset externe.
- La sensation de vitesse est renforcee sans aucun changement `src/ride/*`.
- Aucun Meshy, aucun cout credit, aucun pipeline lourd.
- Pas de route multiple, pas de HUD redesign, pas de nouveau backlog.
- `npm run typecheck`, `npm run test`, `npm run build` et
  `npm run capture:ride-video` sont executes pendant implementation et les
  preuves sont notees.
- Linear MYB-10, `_bmad-output/implementation-artifacts/sprint-status.yaml` et
  `_bmad-output/linear-sync.md` restent synchronises.

## Dev Agent Record

### Agent Model Used

TBD

### Debug Log References

- Story/tracking creation only: npm validations intentionally not run.

### Completion Notes

TBD

### File List

- `_bmad-output/implementation-artifacts/myb-10-motion-density-pass.md`
- `_bmad-output/implementation-artifacts/sprint-status.yaml`
- `_bmad-output/linear-sync.md`

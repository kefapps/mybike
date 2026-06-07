---
story_id: "post-mvp-visual-2"
story_key: "myb-9-scenic-mood-pass-procedural"
linear_id: "MYB-9"
linear_url: "https://linear.app/kefjbo/issue/MYB-9/scenic-mood-pass-procedural"
title: "Scenic Mood Pass procedural"
status: "done"
created: "2026-06-06"
baseline_commit: "4f2ce690a5e2f9d365742f8693dd82dda15fca88"
scope: "post-MYB-8 procedural scenic mood pass"
source_decision: "_bmad-output/planning-artifacts/proposition-scenic-mood-pass-procedural-2026-06-06.md"
source_comparison: "_bmad-output/implementation-artifacts/visual-upgrade-comparison-2026-06-06.md"
source_previous_story: "_bmad-output/implementation-artifacts/myb-8-visual-upgrade-scenic-pass.md"
depends_on:
  - "MYB-8"
---

# Story MYB-9: Scenic Mood Pass procedural

Status: done

## Story

As a joueur de la vertical slice mock,
I want chaque biome ait un motif memorable, du relief lateral et un mood
lumineux identifiable,
so that une capture courte donne une impression de lieu compose plutot qu'un
decor procedural generique.

## Contexte

`MYB-8` a transforme la scene en premiere balade scenic stylisee: route enrichie,
bas-cotes, decor lateral, horizon et parallax plus lisibles. La comparaison
avant/apres montre cependant que la scene reste trop systemique: il manque une
identite de lieu forte par biome, une silhouette terrain plus composee, une
lumiere directionnelle plus assumee, du fog/ciel distinctif et un ou deux motifs
visuels qu'on peut reconnaitre en capture.

La decision locale est `Scenic Mood Pass procedural`. Cette story doit rester
une seule story visuelle bornee, sans creer de backlog, sans asset pipeline
lourd et sans Meshy. Meshy reste seulement une option future pour 1 a 2
landmarks precis, avec confirmation du cout avant tout appel.

Sources:

- `_bmad-output/planning-artifacts/proposition-scenic-mood-pass-procedural-2026-06-06.md`
- `_bmad-output/implementation-artifacts/visual-upgrade-comparison-2026-06-06.md`
- `_bmad-output/implementation-artifacts/myb-8-visual-upgrade-scenic-pass.md`
- `_bmad-output/implementation-artifacts/sprint-status.yaml`
- `_bmad-output/linear-sync.md`
- `src/render/*`
- `src/route/*`

## Scope strict

Inclus:

1. Un landmark ou motif fort par biome existant.
   - Coast: phare lointain, arche/falaise rocheuse, ponton ou signal maritime
     low-poly procedural.
   - Forest: tunnel vegetal, clairiere composee, canopee marquee ou silhouettes
     de troncs plus reconnaissables.
2. Relief, bermes et silhouettes de terrain autour de la route.
   - Talus proches, buttes laterales, ridge lines ou plans terrain near/mid/far.
   - Variation visible autour de la route sans changer la progression.
3. Lumiere, fog, ciel et mood identifiable.
   - Palette par biome plus expressive.
   - Direction de lumiere, fog distance/couleur et horizon ajustes pour rendre
     coast et forest immediatement differents.
4. Tests et preuves proportionnes aux fichiers touches.

Exclus:

- Meshy, sauf si l'implementation demontre explicitement qu'un landmark precis
  ne peut pas etre fait correctement en procedural. Dans ce cas, stopper avant
  tout appel et demander confirmation du cout.
- Pipeline asset lourd, glTF obligatoire, asset manager ou dependance externe.
- Routes multiples, route selector, editeur ou import GPX.
- Refonte HUD, nouveau gameplay ou nouveau mode.
- Optimisation bundle avancee.
- BLE, FTMS, Web Bluetooth, backend, persistance ou deploiement.
- Modification de `src/ride/*`, sauf justification tres forte, pure et testee.
- Creation d'un nouveau backlog Linear.

## Acceptance Criteria

1. Chaque biome rendu par la route mock possede un motif fort et reconnaissable
   en capture courte, avec au moins un motif coast et un motif forest.
2. La scene montre du relief lateral autour de la route: talus, bermes, buttes,
   ridge lines ou silhouettes terrain visibles, sans route multiple ni
   generation de nouvelle boucle de ride.
3. Le mood par biome est identifiable via ciel, fog, horizon, direction de
   lumiere et palette; coast et forest ne doivent pas seulement differer par la
   couleur de quelques props.
4. L'implementation reste procedural Three.js et mock-first: aucun asset externe
   obligatoire, aucun service, aucun Meshy et aucun cout credit.
5. `SceneController` continue de consommer `snapshot.route.biomeId`,
   `snapshot.route.camera` et les snapshots existants; il ne choisit pas la
   progression, la vitesse ou les stats.
6. `src/ride/*` n'est pas modifie. Toute exception doit etre justifiee comme
   strictement necessaire, pure, testee et sans changer la boucle mock.
7. Les ressources Three.js ajoutees sont referencees et disposees au demontage
   via les mecanismes existants ou un helper equivalent.
8. Les ajouts restent bornes: pas de bibliotheque d'assets, pas de route
   supplementaire, pas de HUD redesign, pas de systeme meteo/jour-nuit.
9. Si du code applicatif est modifie, lancer au minimum `npm run typecheck`,
   `npm run test` et `npm run build`.
10. Une preuve visuelle courte, idealement `npm run capture:ride-video` ou des
    screenshots comparables, confirme canvas non vide, motif par biome, relief
    lateral et mood plus identifiable.

## Tasks / Subtasks

- [x] Verifier les contrats existants avant modification. (AC: 4, 5, 6, 7)
  - [x] Lire `src/render/SceneController.ts`.
  - [x] Lire `src/render/createBiomeVisuals.ts`.
  - [x] Lire `src/render/createRouteMesh.ts`.
  - [x] Lire `src/render/renderHelpers.ts` et `src/render/sceneTypes.ts`.
  - [x] Lire `src/route/mockRouteDefinition.ts` et
    `src/route/createRouteFrameSnapshot.ts`.
  - [x] Ne lire `src/ride/*` que pour verifier un type si necessaire; ne pas en
    faire une surface d'implementation.

- [x] Ajouter un motif fort par biome. (AC: 1, 4, 7)
  - [x] Creer des landmarks proceduraux simples et instanciables dans
    `src/render/createBiomeVisuals.ts` ou un helper voisin.
  - [x] Coast: ajouter un motif maritime ou rocheux identifiable.
  - [x] Forest: ajouter un motif vegetal/silhouette identifiable.
  - [x] Garder geometries et materiaux partages et trackes pour disposal.

- [x] Composer le relief lateral et les silhouettes terrain. (AC: 2, 4, 7, 8)
  - [x] Etendre le terrain/horizon dans `SceneController` ou via un helper
    render dedie.
  - [x] Ajouter bermes, talus ou silhouettes near/mid/far autour de la route.
  - [x] Eviter toute generation de route, tout changement de progression et tout
    asset externe obligatoire.

- [x] Renforcer lumiere, fog, ciel et mood par biome. (AC: 3, 5)
  - [x] Ajuster `getBiomePalette` ou une structure voisine pour porter les
    besoins de mood.
  - [x] Modifier intensite/couleur/direction de lumiere et fog selon biome, en
    restant lisible et confortable en premiere personne.
  - [x] Conserver le fallback placeholder lisible pour route absente/invalide.

- [x] Garder Meshy hors scope. (AC: 4, 8)
  - [x] Ne pas appeler Meshy.
  - [x] Documenter dans le Dev Agent Record si un landmark futur pourrait
    beneficier de Meshy, sans declencher de generation.
  - [x] Si Meshy devient indispensable, stopper et demander confirmation du cout
    avant tout appel outil.

- [x] Valider et documenter. (AC: 9, 10)
  - [x] Lancer `npm run typecheck` si du code applicatif est modifie.
  - [x] Lancer `npm run test` si du code applicatif est modifie.
  - [x] Lancer `npm run build` si du code applicatif est modifie.
  - [x] Produire une capture courte ou screenshots comparables si l'outil est
    disponible.
  - [x] Mettre a jour cette story, `sprint-status.yaml` et `_bmad-output/linear-sync.md`.

## Dev Notes

### Etat actuel apres MYB-8

- `src/render/SceneController.ts` cree la scene Three.js, le ground procedural,
  l'horizon, la fog, la lumiere, la route scenic et les visuels biome. Il met a
  jour palette, fog, lumiere et camera depuis `RenderFrameSnapshot`.
- `src/render/createRouteMesh.ts` cree deja un groupe `scenic-route` avec
  surface, shoulders, edge bands, center markings et surface bands. MYB-9 ne
  doit pas refaire ce travail sauf petite integration visuelle avec le relief.
- `src/render/createBiomeVisuals.ts` cree le decor lateral procedural de MYB-8:
  posts coast, herbes, rochers, arbres, panneaux, far trees et distant hills.
  C'est le point d'extension principal pour des landmarks biome distinctifs.
- `src/render/renderHelpers.ts` porte les palettes `coast`, `forest` et
  `placeholder`. MYB-9 peut enrichir ces palettes pour mood/fog/horizon/lumiere
  si cela reste simple.
- `src/route/mockRouteDefinition.ts` expose une route de 1000 m avec transition
  `coast -> forest` a 1% pour rendre le biome observable rapidement. Modifier
  ce fichier seulement si la preuve visuelle exige une petite donnee scenic ou
  un marqueur purement route, et justifier.
- `src/route/createRouteFrameSnapshot.ts` compose sample, camera et biome depuis
  la progression existante. Ne pas y deplacer la logique de rendu.

### Architecture a respecter

- React gere UI, HUD, controles et etats d'ecran.
- `src/ride/*` reste pur, sans React ni Three.js.
- `src/route/*` reste pur, sans React ni Three.js.
- `src/render/*` peut importer Three.js et doit rendre des snapshots existants.
- `SceneController` doit continuer de rendre `snapshot.route.biomeId`, pas
  inventer une progression ou un etat de gameplay parallele.
- Tout mesh/material/geometry ajoute doit etre tracke pour disposal.
- Le mode mock reste le produit cible de la vertical slice.

### Fichiers probables a modifier

- `src/render/createBiomeVisuals.ts`
- `src/render/createBiomeVisuals.test.ts`
- `src/render/SceneController.ts`
- `src/render/renderHelpers.ts`
- `src/render/renderHelpers.test.ts`
- `src/render/createRouteMesh.ts` et `src/render/createRouteMesh.test.ts`
  seulement si relief/bermes doit se connecter a la route
- `src/route/mockRouteDefinition.ts` et tests route seulement avec justification
  forte et changement purement data

Eviter `src/ride/*`.

### Tests attendus

- Tests render sur landmarks proceduraux, bornes de densite et tracking des
  ressources.
- Tests palette/mood si `renderHelpers` change.
- Tests route uniquement si `mockRouteDefinition` ou un contrat route change.
- Validation browser/capture pour confirmer motif coast, motif forest, relief,
  mood, canvas non vide et happy path mock preserve.

## Definition of Done

- MYB-9 ajoute un motif fort par biome existant.
- MYB-9 ajoute relief/bermes/silhouettes terrain visibles autour de la route.
- MYB-9 rend lumiere, fog, ciel et mood plus distinctifs par biome.
- Aucun Meshy, aucun cout credit, aucun asset pipeline lourd.
- Pas de route multiple, pas de HUD redesign, pas de nouveau backlog.
- `src/ride/*` reste intact sauf justification exceptionnelle.
- Validations et preuve visuelle sont notees dans le Dev Agent Record.
- Linear MYB-9, `_bmad-output/implementation-artifacts/sprint-status.yaml` et
  `_bmad-output/linear-sync.md` restent synchronises.

## Dev Agent Record

### Agent Model Used

GPT-5 Codex

### Debug Log References

- `npm run test -- src/render/createBiomeVisuals.test.ts src/render/renderHelpers.test.ts`:
  RED confirme avant implementation, 4 echecs attendus sur landmarks, relief et
  palette mood.
- `npm run test -- src/render/createBiomeVisuals.test.ts src/render/renderHelpers.test.ts`:
  GREEN apres implementation, 7 tests render.
- `npm run typecheck`: passe.
- `npm run test`: passe, 21 fichiers / 68 tests.
- `npm run build`: passe avec l'avertissement Vite connu sur le chunk Three.js >
  500 kB.
- Vite local: HTTP 200 sur `http://127.0.0.1:5174/`.
- Capture browser: canvas non blank, aucune page error; un 404 isole connu cote
  capture.
- Capture video finale review:
  `_bmad-output/video-captures/ride-visual-audit-2026-06-06T23-54-18-055Z/ride-visual-audit-30s.mp4`.
- Contact sheet finale review:
  `_bmad-output/video-captures/ride-visual-audit-2026-06-06T23-54-18-055Z/ride-visual-audit-contact-sheet.jpg`.
- Capture summary: HTTP 200, 6 frames, aucune page error, un 404 isole deja connu
  cote capture.
- Nonblank final: `final-frame.png` 1440x900 mean=50628.3 stddev=18742.4;
  contact sheet 1920x200 mean=51378.5 stddev=17634.3.

### Completion Notes

- Ajout d'un motif coast fort en procedural: phare low-poly et arche rocheuse,
  visibles en debut de capture.
- Ajout d'un motif forest fort: tunnel vegetal/silhouettes de troncs et canopee
  sur le segment forest.
- Ajout de bermes proches, ridges lointains et relief ground plus marque pour
  composer des plans near/mid/far autour de la route.
- Renforcement des palettes de biome: ciel, fog near/far, horizon, intensite et
  direction de lumiere sont maintenant portes par `getBiomePalette`.
- Review gds-code-review: tunnel forest avance dans la capture 30s, ground et
  props laterales rendus route-aware pour eviter le clipping avec la route
  courbe, `Color`/`Fog` reutilises par frame et nouveaux materiaux scenic teintes
  en fallback placeholder.
- Aucun Meshy, aucun asset externe, aucun pipeline glTF et aucun changement
  `src/ride/*`; un futur landmark Meshy pourrait rester utile seulement pour un
  objet precise apres confirmation du cout.

### File List

- `_bmad-output/implementation-artifacts/myb-9-scenic-mood-pass-procedural.md`
- `_bmad-output/implementation-artifacts/sprint-status.yaml`
- `_bmad-output/linear-sync.md`
- `src/render/SceneController.ts`
- `src/render/createBiomeVisuals.ts`
- `src/render/createBiomeVisuals.test.ts`
- `src/render/renderHelpers.ts`
- `src/render/renderHelpers.test.ts`

### Change Log

- 2026-06-06: Story MYB-9 creee depuis la decision Scenic Mood Pass procedural;
  status ready-for-dev, implementation non lancee.
- 2026-06-06: MYB-9 implemente; story mise en review avec validations et preuve
  video.
- 2026-06-07: Revue gds-code-review approuvee apres corrections route-aware,
  perf/cleanup et preuve video finale; status done.

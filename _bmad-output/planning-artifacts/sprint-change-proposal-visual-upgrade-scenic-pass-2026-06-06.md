---
title: "Sprint Change Proposal - Visual Upgrade Scenic Pass"
project: "mybike"
date: "2026-06-06"
workflow: "gds-correct-course"
status: "decision-ready"
source_visual_audit: "_bmad-output/implementation-artifacts/visual-audit-video-ride-2026-06-06.md"
source_capture: "_bmad-output/video-captures/ride-visual-audit-2026-06-06T20-16-57-308Z/"
next_action: "create-visual-upgrade-story"
---

# Sprint Change Proposal - Visual Upgrade Scenic Pass

## 1. Declencheur

La vertical slice mock est jouable, MYB-7 a ameliore la sensation de ride, mais
l'audit video 30s montre que la scene reste graphiquement trop pauvre:

- route unie, sans bas-cotes, marquage, texture ou variation lisible;
- decor lateral presque vide, avec tres peu de reperes proches;
- profondeur faible: horizon plat, peu de plans proches/moyens/lointains;
- silhouettes lointaines trop discretes pour transformer la scene en balade.

## 2. Decision

Definir une seule direction: **Visual Upgrade Scenic Pass**.

Cette direction vise a faire passer la scene de "route ruban sur plan" a une
balade scenic stylisee, toujours mock, toujours sur rail, sans ouvrir un gros
chantier asset ou contenu.

## 3. Pourquoi Meshy seul ne suffit pas

Meshy peut aider a produire quelques props low-poly, mais il ne resout pas le
probleme principal observe dans la video: la composition de scene.

Sans direction visuelle claire, quelques assets importes resteraient poses dans
un monde plat. Le besoin prioritaire est donc d'abord systemique et lisible:
route plus riche, densite laterale, profondeur, horizon et repetition
procedurale maitrisee. Meshy doit rester un appoint cible pour 2 a 4 assets
reutilisables et instanciables, pas le moteur du changement.

## 4. Scope retenu

Story unique proposee: **MYB-8 - Visual Upgrade Scenic Pass**.

Ameliorations visibles maximum:

1. **Route enrichie**: bas-cotes, texture simple, marquage central/lateral ou
   variation de surface legere.
2. **Decor lateral procedural**: familles simples d'arbres, rochers, herbes,
   poteaux ou panneaux, instanciees le long de la route.
3. **Profondeur scenic**: horizon plus travaille, brume legere, plans
   proches/moyens/lointains, variation terrain simple.
4. **Meshy cible optionnel**: 2 a 4 assets low-poly maximum, seulement s'ils
   remplacent avantageusement des placeholders proceduraux et restent
   reutilisables.

## 5. Impact sur les artefacts

### GDD

Pas de changement de boucle produit. Le mode mock reste le coeur jouable. La
montee graphique sert la lisibilite et l'envie de rouler, pas une nouvelle
feature de gameplay.

### Architecture

Les frontieres actuelles restent valides:

- React conserve HUD, controles et etats d'ecran;
- `ride` et `route` restent purs et testables;
- Three.js rend un snapshot et gere les elements visuels;
- les assets externes doivent garder un fallback procedural.

La story MYB-8 devra rester dans `render` et les helpers visuels associes, sauf
petit ajustement de donnees route strictement necessaire a la variation scenic.

### Epics / stories

Ne pas creer d'epic graphique et ne pas recreer un backlog post-MVP. MYB-8 est
une story unique de passage scenic, issue de l'audit video.

### Linear

Ajouter une note projet pour consigner la decision. Ne pas creer l'issue Linear
MYB-8 dans ce cadrage; la prochaine action est de creer la story unique quand
le scope est accepte.

## 6. Hors scope

- Pipeline asset lourd.
- Meshy massif ou generation d'une bibliotheque complete.
- Routes multiples.
- Deploiement.
- BLE, FTMS, Web Bluetooth ou velo connecte.
- Refonte HUD.
- Optimisation bundle avancee, sauf blocage reel pendant implementation.
- Nouveau mode de gameplay.
- Gros backlog graphique ou nouvel epic.

## 7. Handoff story MYB-8

Titre propose:

```text
Visual Upgrade Scenic Pass
```

Objectif:

```text
Rendre la balade mock nettement plus scenic dans une capture 30s, avec route
enrichie, decor lateral procedural et profondeur visuelle, sans changer la
boucle de ride ni ouvrir un pipeline asset lourd.
```

Critere de succes principal:

```text
Sur une capture locale 30s comparable, l'image doit montrer une route moins
unie, des reperes lateraux proches, un horizon plus profond, et une scene qui
lit immediatement comme une balade scenic stylisee.
```

Validation attendue:

- `npm run typecheck`
- `npm run test`
- `npm run build`
- capture video courte ou screenshots desktop/mobile si l'outil de capture est
  disponible

## 8. Prochaine action

`create-visual-upgrade-story`

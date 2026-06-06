---
title: "Mini-scope polish - UX / sensation de ride"
project: "mybike"
date: "2026-06-06"
scope: "post-MVP polish UX / sensation de ride"
status: "selected-for-story"
source_results: "_bmad-output/implementation-artifacts/playtest-polish-ux-ride-feel-results-2026-06-06.md"
change_proposal: "_bmad-output/planning-artifacts/sprint-change-proposal-polish-ux-ride-feel-2026-06-06.md"
---

# Mini-scope polish - UX / sensation de ride

## Decision

Retenir un mini-scope unique pour le prochain passage: une seule petite story
polish avec 3 ajustements maximum, sans creer de nouveau backlog large:

1. Compacter le HUD mobile pour liberer la scene.
2. Rendre la reponse du slider plus comprehensible.
3. Rendre le changement de biome observable pendant un playtest court.

## Critere de selection

Ces ajustements sont retenus parce qu'ils sont directement observables dans le
self-playtest et restent dans le MVP mock:

- le HUD mobile masque trop la scene;
- le slider fonctionne, mais le smoothing rend le lien effort -> vitesse trop
  indirect;
- la route est lisible, mais la transition de biome n'est pas observable dans un
  run court.

## Definition des ajustements

### 1. HUD mobile plus compact

- Objectif: rendre la scene plus presente sur petit ecran.
- Changement attendu: reduire l'emprise verticale du HUD mobile et prioriser les
  donnees utiles pendant le ride.
- Validation: en viewport mobile, la route et le ciel restent visibles pendant
  le ride, sans perdre pause/resume ni la lecture des stats principales.

### 2. Feedback slider plus lisible

- Objectif: clarifier que l'action joueur modifie l'allure.
- Changement attendu: rendre la variation effort -> vitesse plus perceptible,
  par microcopy, affichage ou calibration legere du smoothing, sans casser la
  logique pure existante.
- Validation: en passant bas -> moyen -> haut, le joueur voit rapidement que la
  vitesse cible et/ou l'allure repond.

### 3. Biome observable en test court

- Objectif: permettre au playtest de juger la lisibilite route / biomes sans
  rallonger fortement la session.
- Changement attendu: rendre une transition visuelle atteignable dans un court
  run mock, via donnees de route ou seuil de presentation, sans ajouter de
  nouvelle route ni pipeline asset.
- Validation: un ride court permet d'observer au moins une variation
  d'ambiance/biome.

## Hors scope

- Pas de BLE/FTMS.
- Pas de Meshy ni pipeline asset lourd.
- Pas de nouvelle route complete.
- Pas de refonte UI.
- Pas de nouvelle issue Linear tant que cette selection n'est pas transformee en
  story ou micro-epic approuve.
- Pas de hardening `WebGLRenderer`, car aucun echec reel n'a ete observe.

## Story future

La story future doit rester petite et regrouper ces 3 ajustements comme un seul
polish coherent de sensation de ride mock. Elle ne doit pas creer plusieurs
issues, ni rouvrir Epic 1.

Titre propose:

```text
Polish UX / sensation de ride mock
```

Prompt de creation de story:

```text
gds-create-story

Creer une petite story polish post-MVP pour Echappee 3D.

Sources:
- _bmad-output/planning-artifacts/sprint-change-proposal-polish-ux-ride-feel-2026-06-06.md
- _bmad-output/implementation-artifacts/polish-ux-ride-feel-mini-scope-2026-06-06.md
- _bmad-output/implementation-artifacts/playtest-polish-ux-ride-feel-results-2026-06-06.md
- _bmad-output/implementation-artifacts/sprint-status.yaml
- _bmad-output/linear-sync.md

Objectif:
- Creer une seule petite story polish, pas un backlog.
- Inclure exactement les 3 ajustements selectionnes: HUD mobile compact,
  feedback slider plus comprehensible, biome observable en playtest court.
- Garder mock mode obligatoire et respecter les frontieres React / ride / route /
  render.
- Ne pas creer d'issue Linear sauf demande explicite.
```

## Prochaine action

Creer la story locale BMAD polish: `create-polish-story`.

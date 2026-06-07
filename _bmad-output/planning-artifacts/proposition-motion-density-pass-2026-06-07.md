---
title: "Proposition - MYB-10 Motion & Density Pass"
project: "mybike"
date: "2026-06-07"
workflow: "gds-correct-course"
status: "decision-ready"
current_capture: "_bmad-output/video-captures/ride-visual-audit-2026-06-07T05-24-07-932Z/"
comparison_stack: "_bmad-output/video-captures/ride-visual-audit-2026-06-07T05-24-07-932Z/visual-comparison-stack.jpg"
selected_direction: "MYB-10 Motion & Density Pass"
next_action: "create-MYB-10-motion-density-story"
---

# Proposition - MYB-10 Motion & Density Pass

## Constat

La capture actuelle ne montre pas de regression depuis MYB-9 et la scene est
plus identifiable qu'avant MYB-8/MYB-9. Le rendu reste cependant trop
prototype pour le prochain signal de qualite: decor repetitif, materiaux plats,
peu de vie proche camera et sensation de vitesse encore limitee.

Le besoin principal n'est pas Meshy maintenant. Le meilleur levier est un pass
procedural court sur le mouvement, la densite controlee, le parallax et les
variations lisibles de scene.

## Decision

Preparer une seule petite story: **MYB-10 Motion & Density Pass**.

La story doit renforcer la sensation de balade en mouvement sans toucher au
coeur ride pur ni rouvrir un backlog visuel large.

## Scope strict

1. Ajouter du parallax proche avec des elements lisibles qui defilent pres de la
   camera.
2. Ajouter une densite roadside controlee, variee mais bornee, sans remplir la
   scene au hasard.
3. Ajouter des variations visuelles de route et de sol: bords, bandes,
   textures procedurales simples, ruptures legeres ou motifs de surface.
4. Renforcer la sensation de vitesse cote rendu uniquement, sans modifier
   `src/ride/*`.
5. Option simple si utile: mode capture sans HUD pour mieux analyser les
   captures video.

## Hors scope

- Meshy.
- Asset externe.
- Pipeline asset lourd.
- Nouvelle route, routes multiples, route selector, GPX ou editeur.
- BLE, FTMS, Web Bluetooth, backend, persistance ou deploiement.
- Changement de logique dans `src/ride/*`.
- Refonte HUD ou nouveau mode de gameplay.
- Creation de backlog large, epic graphique ou plusieurs stories.

## Impact attendu

- GDD: pas de changement produit; le mode mock reste le coeur jouable.
- Architecture: React conserve HUD/etat; Three.js porte le rendu et les
  variations visuelles; `src/ride/*` reste hors scope.
- Linear: ajouter une note projet maintenant, mais ne pas creer l'issue MYB-10
  dans cette etape.
- Validation future de la story: typecheck, tests, build et capture comparable.

## Prompt de story suivant

```text
gds-create-story

Creer une story unique: MYB-10 Motion & Density Pass.

Contexte:
- Projet Echappee 3D / mybike.
- MYB-9 est Done.
- Nouvelle capture 30s:
  _bmad-output/video-captures/ride-visual-audit-2026-06-07T05-24-07-932Z/
- Comparaison empilee:
  _bmad-output/video-captures/ride-visual-audit-2026-06-07T05-24-07-932Z/visual-comparison-stack.jpg
- Proposition source:
  _bmad-output/planning-artifacts/proposition-motion-density-pass-2026-06-07.md

Objectif:
- Une seule petite story, pas de backlog.
- Ajouter du parallax proche.
- Ajouter de la densite roadside controlee.
- Ajouter des variations visuelles de route/sol.
- Renforcer la sensation de vitesse sans toucher a src/ride/*.
- Option: mode capture sans HUD si simple et utile pour analyser le rendu.

Hors scope:
- Aucun Meshy.
- Aucun asset externe.
- Aucun pipeline lourd.
- Ne pas modifier BLE/FTMS, backend, persistance, routes multiples.
- Ne pas toucher a src/ride/*.

Validation attendue:
- npm run typecheck
- npm run test
- npm run build
- capture video 30s ou screenshots comparables si l'outil est disponible
```

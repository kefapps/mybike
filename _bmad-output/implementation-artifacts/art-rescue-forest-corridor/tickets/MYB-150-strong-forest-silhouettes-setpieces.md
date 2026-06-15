# MYB-150 — Silhouettes fortes et setpieces forêt

## Linear metadata

- **Priority**: P1
- **Labels**: art-production, composition, setpieces, silhouettes
- **Estimate**: 8 pts
- **Depends on**: MYB-147, MYB-148
- **Parent / Project suggéré**: `Art Rescue — Forest Corridor Vertical Slice`

## Contexte

MYB-137 ajoute des formes verticales, mais elles ne racontent rien. Il faut des moments visuels mémorables : arbre penché, souche massive, arche de racines, rocher moussu, tronc tombé. La forêt doit avoir des signatures, pas seulement du remplissage.

## Objectif

Créer et placer 3 à 5 silhouettes fortes visibles depuis la route, sans obstruer la conduite.

## Tâches


- Définir 5 setpieces candidats.
- Produire les assets via Blender/procédural ou kit existant.
- Les placer à des distances route différentes : foreground safe, midground hero, background landmark.
- Tester la lisibilité depuis caméra route.
- Captures et rapport avec annotations textuelles.


## Critères d'acceptation


- Au moins 3 silhouettes sont immédiatement identifiables en capture route.
- Elles ne lisent pas comme des poteaux.
- Elles donnent une identité au segment forêt.
- La route reste claire et non obstruée.


## Visual Progress Rule

If this ticket improves the visual result but does not reach Premium target, it
must not be closed as `Done`.

Allowed outcomes:

- remain `In Review` with `Checkpoint insufficient`;
- return to `In Progress` with a targeted corrective sub-ticket;
- close only with Julien's explicit documented exception;
- rollback/rework if the result moves the scene in the wrong direction.

A real improvement is valuable evidence, but it is not enough for `Done`.


## Canonical Visual Validation Surface

This ticket changes visible Art Rescue output.

Final visual validation must be done in the canonical ride corridor, from the
canonical ride camera.

Required final evidence:

- route screenshot from canonical ride camera;
- overview screenshot from the same scene state;
- visual rubric score;
- human validation when subjective.

Preview scenes, Blender renders, Meshy/Tripo previews, turntables and isolated
asset screenshots are allowed as intermediate evidence only. They cannot close
this ticket as `Done`.

The route screenshot is blocking. The overview screenshot is required but
secondary.


## Visual / Performance Arbitration

This ticket must prioritize `Stylisé Premium de Production`, then measure
performance. Performance is a guardrail, not an excuse for poor visuals.

Allowed:

- spend more geometry/material/density if the gain is visible from the canonical
  route camera;
- enrich while the scene meets FPS target and validators show no blocking
  `ERROR`.

If performance fails, reduce in this order:

1. unnecessary collisions;
2. oversized textures/material excess;
3. density outside the route camera;
4. LOD/culling/distance;
5. non-essential near-route density;
6. premium route-camera-visible elements last.

Required for review:

- route screenshot;
- overview screenshot;
- performance/validator result if this ticket changes density, assets,
  materials, collisions or lighting;
- note explaining whether the visual spend is visible from the route camera.


## Validation attendue


- Captures route avec liste des setpieces visibles.
- Visual rubric : silhouette quality au moins 4/5.
- Asset validator sans ERROR.


## Hors scope


- Pas de boss fight arbre possédé. Même si, entre nous, tentant.
- Pas de Meshy/Tripo sauf dérogation documentée.
- Pas d’animation complexe.


## Prompt Codex prêt à coller

```txt
Ajoute des silhouettes fortes au corridor forêt.

Créer/placer 3 à 5 setpieces:
- arbre penché;
- souche/root-base massive;
- fallen log;
- rocher moussu;
- arche légère de branches ou racines.

Contraintes:
- visibles depuis caméra route;
- pas d’obstruction gameplay;
- pas de forme cylindre/poteau brute;
- cohérence avec art bible;
- validator OK.

Livrables:
- assets/prefabs ou composition dédiée;
- scène preview;
- captures route/overview;
- rapport listant chaque silhouette et son rôle.
```

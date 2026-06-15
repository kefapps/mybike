# MYB-152 — POC Meshy/Tripo en quarantaine pour un prop isolé

## Linear metadata

- **Priority**: P2
- **Labels**: ai-assets, meshy, tripo, quarantine, asset-pipeline
- **Estimate**: 5 pts
- **Depends on**: MYB-143, MYB-144
- **Parent / Project suggéré**: `Art Rescue — Forest Corridor Vertical Slice`

## Contexte

Meshy/Tripo ne sont pas bannis. Ils sont mis au régime. Leur bon rôle : props isolés, hero objects, variantes rapides. Leur mauvais rôle : route, terrain, forêt complète, gameplay core.

## Objectif

Tester un seul prop IA, proprement documenté, nettoyé et validé, pour prouver le pipeline sans polluer la prod.

## Tâches


- Choisir un prop isolé : panneau forestier, vieille souche sculptée, petit abri, rocher hero.
- Générer max 3 candidats Meshy/Tripo.
- Mettre dans `Quarantine`.
- Nettoyer dans Blender : échelle, pivot, topologie si possible, matériaux.
- Importer en Unity dans dossier de review.
- Remplir manifest.
- Lancer validator.
- Promouvoir 0 ou 1 asset seulement.


## Critères d'acceptation


- L’asset IA ne touche pas au corridor principal avant validation.
- Le manifest est complet.
- Le validator ne remonte pas d’ERROR.
- La décision `approved` ou `rejected` est documentée.
- Si la qualité est mauvaise, le ticket est quand même réussi si le rejet est proprement prouvé.


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


- Rapport d’intake.
- Capture de l’asset dans une scène preview neutre.
- Rapport validator.


## Hors scope


- Pas de génération de forêt complète.
- Pas de route/terrain Meshy/Tripo.
- Pas de promotion de plusieurs assets.
- Pas d’asset sans licence/source claire.


## Prompt Codex prêt à coller

```txt
Teste le pipeline Meshy/Tripo avec un seul prop isolé.

Important:
- utiliser le workflow de quarantaine;
- ne pas intégrer directement en production;
- ne pas générer route, terrain ou forêt;
- ne promouvoir qu’un asset maximum.

Livrables:
- manifest complet;
- asset en quarantaine/review;
- rapport Blender cleanup;
- rapport Unity validator;
- décision finale `approved` ou `rejected`.

Le prop recommandé: panneau forestier stylisé ou souche/root-base hero.
```

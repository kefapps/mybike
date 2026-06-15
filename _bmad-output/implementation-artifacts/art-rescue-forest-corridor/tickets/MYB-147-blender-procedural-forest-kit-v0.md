# MYB-147 — Générer un kit procédural Blender forêt v0

## Linear metadata

- **Priority**: P1
- **Labels**: blender-mcp, art-production, forest-kit
- **Estimate**: 8 pts
- **Depends on**: MYB-141, MYB-144
- **Parent / Project suggéré**: `Art Rescue — Forest Corridor Vertical Slice`

## Contexte

Le décor principal ne doit pas venir de Meshy/Tripo. Il doit venir d’un kit
modulaire `Stylisé Premium de Production`, cohérent, avec géométrie optimisée,
facile à valider et à répéter. Moins spectaculaire en preview, beaucoup plus
utile en jeu.

## Objectif

Créer un premier kit forestier procédural Unity-ready : troncs, racines, rochers, fougères, feuilles, branches mortes, canopée simple.

## Tâches


- Générer via Blender MCP un kit `MYB_ForestKit_V0`.
- Assets minimum : 4 troncs, 3 racines, 3 rochers, 3 fougères, 3 leaf/moss mats, 2 branches mortes, 2 canopée masses, 1 fallen log.
- Appliquer transforms, pivots au sol, unités mètres, flat shading contrôlé.
- Exporter en FBX ou GLB selon convention du projet.
- Générer un manifest local avec dimensions, triangles, matériaux.
- Importer en Unity dans un dossier de quarantaine/prod selon workflow.
- Passer le validator MYB-144.


## Critères d'acceptation


- Les assets ne ressemblent pas à des cylindres bruts.
- Chaque famille a des variantes lisibles.
- Aucun asset courant ne dépasse le budget défini par l’art bible.
- Les pivots et bounds sont propres.
- Le kit est assez cohérent pour remplacer les poteaux MYB-137.


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


- Rapport Blender ou Unity listant dimensions, triangles et matériaux.
- Rapport `myb-art-asset-validation.md` sans ERROR bloquant.
- Une scène preview optionnelle peut montrer le kit, sans remplacer encore le corridor final.


## Hors scope


- Pas de Meshy/Tripo.
- Pas de génération text-to-3D externe.
- Pas de scène finale complète.
- Pas de shader complexe.


## Prompt Codex prêt à coller

```txt
Utilise Blender via MCP pour produire un kit procédural `Stylisé Premium de
Production` Unity-ready, avec géométrie optimisée.

Créer `MYB_ForestKit_V0` avec:
- 4 troncs irréguliers, dont 1 cassé et 1 incliné;
- 3 root clusters;
- 3 rochers;
- 3 fougères;
- 3 leaf/moss mats;
- 2 branches mortes;
- 2 masses de canopée stylisées;
- 1 fallen log.

Contraintes:
- unités en mètres;
- pivots au sol;
- transforms appliqués;
- noms `myb_forest_[family]_[variant]`;
- flat shading stylisé;
- max 2 matériaux par asset courant;
- pas de détails microscopiques;
- export Unity-ready;
- manifest JSON/Markdown avec dimensions, triangles et matériaux.

Ensuite importer dans Unity et lancer le validator d’assets.
```

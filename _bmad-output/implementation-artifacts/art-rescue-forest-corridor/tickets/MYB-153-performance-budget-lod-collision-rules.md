# MYB-153 — Budgets performance, LOD et collision pour le corridor forêt

## Linear metadata

- **Priority**: P1
- **Labels**: performance, unity, validation, lod
- **Estimate**: 5 pts
- **Depends on**: MYB-144
- **Parent / Project suggéré**: `Art Rescue — Forest Corridor Vertical Slice`

## Contexte

Un corridor forêt peut vite devenir une brocante de triangles. Même si les chiffres MYB-137 sont encore faibles, il faut poser les budgets avant d’ajouter densité, canopée et setpieces.

## Objectif

Définir et valider les budgets perf/LOD/collision pour la vertical slice forêt.

## Tâches


- Créer `docs/technical/performance-budget-forest-corridor.md`.
- Définir budgets triangle par asset/famille/scène.
- Définir règles LOD ou absence de LOD selon distance.
- Définir règles collider : primitive, simplified mesh, no collider.
- Étendre le validator pour signaler MeshCollider complexe et assets hors budget.
- Ajouter métriques dans rapport capture.


## Critères d'acceptation


- Budgets simples, lisibles, réalistes pour vertical slice.
- Les règles de collision empêchent les MeshColliders absurdes sur props décoratifs.
- Les rapports art indiquent renderers/triangles au minimum.
- Les budgets sont formulés comme garde-fous mesurés, pas comme plafond
  esthétique aveugle.
- La règle de réduction protège les éléments premium visibles depuis la caméra
  route en dernier.


## Visual / Performance Arbitration

The goal is not to minimize visual cost. The goal is to spend performance budget
where it improves the canonical route camera.

As long as the scene reaches the defined FPS target, defaulting to 60 FPS for
Art Rescue, and no blocking validator `ERROR` is reported, additional geometry,
materials or density are allowed when they visibly improve the route-camera
Premium target.

If performance falls below target or a blocking `ERROR` appears, reduce in this
order:

1. unnecessary collisions;
2. oversized textures and excessive materials;
3. density outside the route camera;
4. LOD, culling and distance settings;
5. non-essential near-route density;
6. premium route-camera-visible elements last.

Performance is a measured guardrail, not an excuse for poor visuals.


## Validation attendue


- Rapport validator avec section performance.
- Document budget relu et référencé.
- Note expliquant comment le budget protège la capture route premium.


## Hors scope


- Pas d’optimisation prématurée profonde.
- Pas de profiling console/mobile.
- Pas de refonte du renderer.


## Prompt Codex prêt à coller

```txt
Définis les budgets performance/LOD/collision pour le corridor forêt.

Livrables:
- `docs/technical/performance-budget-forest-corridor.md`;
- extension du validator si pertinent;
- exemple de rapport dans `_bmad-output/unity-test-results/`.

Inclure:
- budget triangles par famille;
- budget textures;
- rules collision;
- rules LOD;
- seuils warning/error;
- arbitrage visuel/performance;
- ordre de réduction si la perf échoue;
- note sur vertical slice vs production future.
```

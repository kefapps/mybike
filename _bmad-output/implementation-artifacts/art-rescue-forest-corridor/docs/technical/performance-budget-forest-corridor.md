# Performance Budget Validator Spec

## Objectif

Surveiller la complexité du corridor forêt avant qu’il devienne un musée du triangle.

## Métriques scène

- Renderers total.
- Triangles total visibles/statique.
- Matériaux uniques.
- Textures > 1024.
- Textures > 2048.
- Lights actives.
- MeshColliders.
- Objets sans LOD à distance si politique activée.

## Budgets scène de départ

Ces budgets sont des garde-fous pour vertical slice, pas une vérité éternelle gravée dans le carbone.

La cible FPS par défaut pour Art Rescue est 60 FPS. Tant que la scène tient cette
cible et qu’aucun validator ne remonte d’`ERROR` bloquant, l’enrichissement
visuel est autorisé quand le gain est visible depuis la caméra route canonique.

| Métrique | Warning | Error |
|---|---:|---:|
| Renderers corridor preview | 600 | 1200 |
| Triangles corridor preview | 150k | 300k |
| Matériaux uniques | 40 | 80 |
| Textures 2K | 10 | 25 |
| Textures >2K | 1 | 5 |
| MeshColliders décoratifs | 10 | 30 |

## Visual / Performance Arbitration

La cible n’est pas de minimiser le coût visuel. La cible est de dépenser le
budget performance là où il améliore la caméra route canonique.

Règle canonique :

```txt
Visual ambition first.
Measured performance second.
Route-camera premium quality protected first.
```

Si la performance tombe sous la cible FPS ou si un `ERROR` bloquant apparaît,
réduire dans cet ordre :

1. collisions inutiles ;
2. textures trop grandes et matériaux excessifs ;
3. densité hors caméra route ;
4. LOD, culling et distances ;
5. densité proche route non essentielle ;
6. éléments premium visibles depuis la route en dernier.

Une dégradation visible d’un élément premium doit être justifiée par une mesure :
FPS observé ou métrique pertinente, validator concerné, catégorie suspecte,
impact visuel avant/après si applicable, justification du downgrade et
alternative envisagée.

## Matrice de décision

| État visuel | État perf | Décision |
|---|---|---|
| Premium target atteint | FPS cible tenu, aucun ERROR | `Done` possible après validation |
| Premium target atteint | FPS sous cible ou ERROR | pas `Done`, optimisation ciblée |
| Checkpoint insuffisant | FPS cible tenu | enrichir / corriger visuellement |
| Checkpoint insuffisant | FPS sous cible ou ERROR | rework + optimisation mesurée |
| Rendu pauvre | FPS excellent | pas `Done`, optimisation prématurée suspecte |
| Rendu dense mais confus | FPS mauvais | rollback/rework probable |

## Visual / Performance Exception

Une exception est possible uniquement si Julien accepte explicitement un
compromis. Elle ne change pas la règle canonique.

```md
## Visual / Performance Exception

Status: Accepted / Rejected
Accepted by: Julien
Date:

Reason:
-

Current visual state:
-

Current performance state:
-

Compromise accepted:
-

Risk:
-

Follow-up required:
-
```

## Métriques asset

Voir `docs/art-direction/05-asset-taxonomy-and-budgets.md`.

## Collisions

Par défaut, les assets décoratifs n’ont pas de collider. Les éléments proches de la route peuvent avoir des colliders simples si le gameplay le demande.

## Rapport

Inclure une section performance dans chaque rapport art.

Chaque rapport qui change densité, assets, matériaux, collisions ou lumière doit
indiquer :

- FPS cible ;
- FPS observé ou métrique perf disponible ;
- résultat validator ;
- dépense visuelle principale ;
- preuve que cette dépense améliore la caméra route, ou raison de quarantaine.


## Note

Ce document sert de version budget lisible côté production. La spécification validator correspondante est dans `docs/validators/performance-budget-validator-spec.md`.

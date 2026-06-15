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

La cible FPS par défaut pour Art Rescue est 60 FPS. Les budgets ci-dessous
servent à détecter le risque et les dérives ; ils ne doivent pas pousser à
produire un rendu pauvre quand la scène tient la cible mesurée.

| Métrique | Warning | Error |
|---|---:|---:|
| Renderers corridor preview | 600 | 1200 |
| Triangles corridor preview | 150k | 300k |
| Matériaux uniques | 40 | 80 |
| Textures 2K | 10 | 25 |
| Textures >2K | 1 | 5 |
| MeshColliders décoratifs | 10 | 30 |

## Règle d’arbitrage

Le validator doit signaler les coûts, pas remplacer le jugement visuel. Tant que
la scène tient la cible FPS et qu’aucun `ERROR` bloquant n’existe, une dépense
visuelle est acceptable si elle améliore la capture route canonique.

Si une optimisation est requise, recommander l’ordre de réduction suivant :

1. collisions inutiles ;
2. textures/matériaux excessifs ;
3. densité hors caméra route ;
4. LOD/culling/distance ;
5. densité proche route non essentielle ;
6. éléments premium visibles en dernier.

No visible premium downgrade without measurement.

## Métriques asset

Voir `docs/art-direction/05-asset-taxonomy-and-budgets.md`.

## Collisions

Par défaut, les assets décoratifs n’ont pas de collider. Les éléments proches de la route peuvent avoir des colliders simples si le gameplay le demande.

## Rapport

Inclure une section performance dans chaque rapport art.

Le rapport doit mentionner la cible FPS, le FPS observé ou la métrique perf
disponible, les `WARNING`/`ERROR`, et la catégorie de coût la plus probable.

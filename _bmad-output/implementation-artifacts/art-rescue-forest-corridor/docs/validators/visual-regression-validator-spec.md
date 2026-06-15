# Visual Regression Validator Spec

## Objectif

S’assurer que chaque ticket art produit des preuves visuelles comparables.

## Checks

- `route.png` existe.
- `overview.png` existe.
- `report.md` existe.
- Le rapport contient un verdict visuel.
- Le rapport contient les métriques minimales.
- Si une baseline est fournie, les sheets before/after existent.

## Non-objectifs

Ce validator ne doit pas décider automatiquement que l’image est belle. Hélas. Il vérifie que les preuves existent et sont comparables.

## Rapport

```txt
_bmad-output/unity-test-results/myb-visual-regression-validation.md
```

## Métriques possibles

- résolution image ;
- hash fichier ;
- date génération ;
- scène source ;
- caméra source ;
- renderers ;
- triangles.

## Utilisation

À lancer à la fin de chaque ticket art avant résumé.

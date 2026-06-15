# Blender Procedural Kit Workflow

## Objectif

Créer des assets `Stylisé Premium de Production`, propres, répétables et
validables, avec géométrie optimisée, sans dépendre de génération IA 3D opaque.

## Entrées

- Art bible.
- Liste de familles d’assets.
- Budgets triangle/matériaux.
- Convention de nommage.

## Étapes

1. Créer collection par famille : trunks, roots, rocks, ferns, moss, branches, canopy.
2. Utiliser mètres comme unité.
3. Construire formes simples mais asymétriques.
4. Éviter cylindres parfaits.
5. Appliquer transforms.
6. Placer pivot au sol.
7. Assigner 1–2 matériaux maximum.
8. Exporter individuellement.
9. Générer manifest technique.
10. Importer dans Unity Review/Generated.
11. Lancer validator.

## Manifest technique minimal

```json
{
  "assetId": "myb_forest_trunk_broken_a",
  "family": "trunk_common",
  "source": "blender_mcp_procedural",
  "dimensionsMeters": { "x": 0.8, "y": 4.2, "z": 0.7 },
  "triangles": 620,
  "materials": ["myb_bark_warm"],
  "pivot": "ground_center",
  "status": "review"
}
```

## Critères qualité

- Silhouette reconnaissable en noir et blanc.
- Taille plausible en mètres.
- Pas de micro-détails invisibles depuis la route.
- Pas d’éléments flottants.
- Pas de faces cachées massives inutiles.
- Pas de collision par défaut sur décor purement visuel.

## Validation visuelle

Blender MCP produit des candidats. La caméra de ride valide la qualité visuelle
de production.

La preview Blender ou Unity peut valider la propreté géométrique, le pivot,
l’échelle, la silhouette isolée, le budget et l’export. Elle ne valide pas
`Premium target`.

Un kit doit être intégré dans le corridor de ride canonique, puis capturé depuis
la caméra route canonique avant de pouvoir fermer un ticket visuel de production.

## Export

Préférer un format stable avec Unity dans le projet courant. Le choix FBX/GLB doit rester cohérent avec le pipeline existant.

## Erreurs fréquentes

- Pivot au centre de l’objet au lieu du sol.
- Scale non appliqué.
- Variantes trop similaires.
- Matériaux trop nombreux.
- Asset joli seul, inutile dans la caméra route.

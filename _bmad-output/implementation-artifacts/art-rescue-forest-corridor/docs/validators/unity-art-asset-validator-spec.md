# Unity Art Asset Validator Spec

## Objectif

Scanner les assets artistiques pour détecter les problèmes avant qu’ils polluent les scènes.

## Entrées

Dossiers recommandés :

```txt
Assets/Echappee/Art/
Assets/MYB*/
```

Manifest recommandé :

```txt
Assets/Echappee/Art/ThirdPartyAssets.assetmanifest.json
```

## Sortie

```txt
_bmad-output/unity-test-results/myb-art-asset-validation.md
```

## Niveaux

- `ERROR` : doit bloquer promotion/merge.
- `WARNING` : doit être revu.
- `INFO` : métrique utile.

## Checks

### Naming

- Préfixe `myb_` pour assets production.
- Pas d’espaces.
- Pas de `final`, `new`, `test`, `good`, `copy`.
- Famille connue dans le nom ou metadata.

### Mesh

- Triangles par mesh.
- Submeshes.
- Bounds plausibles.
- Scale extrême.
- Mesh vide.
- Mesh sans renderer utile.

### Materials

- Matériau manquant.
- Matériau rose/magenta si détectable.
- Nombre de matériaux > budget.
- Shader incohérent si le projet impose un shader cible.

### Textures

- Taille > budget.
- Texture sans compression si politique définie.
- Texture dans mauvais dossier.
- Texture orpheline si détectable.

### Colliders

- MeshCollider sur asset décoratif courant.
- MeshCollider non convex sur prop simple.
- Trop de colliders sur prefab courant.

### Manifest

- Asset tiers/IA sans entrée manifest.
- Manifest sans source.
- Manifest sans licence.
- Statut non approuvé en production.

### Import settings

- Read/Write enabled inutile si détectable.
- Generate colliders automatique non souhaité.
- Scale factor incohérent.

## Budgets par défaut

Voir `docs/art-direction/05-asset-taxonomy-and-budgets.md`.

## Rapport attendu

Exemple :

```md
# MyBike Art Asset Validation

Generated: 2026-06-15
Project: unity/Echapee4D

## Summary

- Assets scanned: 42
- Errors: 2
- Warnings: 9

## Errors

| Asset | Check | Message |
|---|---|---|
| myb_forest_trunk_raw_ai | manifest | AI asset in production without approved manifest |

## Warnings

| Asset | Check | Message |
|---|---|---|
| myb_forest_rock_mossy_a | triangles | 1700 triangles > warning 800 |
```

## Batchmode

Prévoir une méthode Editor statique appelable par batchmode, par exemple :

```txt
Unity -batchmode -quit -projectPath unity/Echapee4D -executeMethod MyBikeArtAssetValidator.RunBatch
```

Le nom exact peut suivre les conventions du projet.

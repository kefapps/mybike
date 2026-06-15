# Asset Taxonomy and Budgets

## Catégories

### Production-safe

Assets approuvés, manifestés, validés, utilisables dans scènes candidates.

Chemin recommandé :

```txt
Assets/Echappee/Art/Production/Forest/...
```

### Review

Assets nettoyés mais en attente de validation visuelle ou technique.

```txt
Assets/Echappee/Art/Review/...
```

### Quarantine

Assets bruts externes/IA. Interdits en scène production.

```txt
Assets/Echappee/Art/Quarantine/...
```

### Generated procedural

Assets créés par Blender MCP ou scripts internes, avec manifest technique.

```txt
Assets/Echappee/Art/Generated/...
```

## Convention de nommage

```txt
myb_[biome]_[family]_[descriptor]_[variant]
```

Exemples :

```txt
myb_forest_trunk_broken_a.fbx
myb_forest_root_cluster_low_b.fbx
myb_forest_fern_patch_wide_a.prefab
myb_forest_rock_mossy_medium_c.prefab
```

## Budgets

| Famille | Triangles Warning | Triangles Error | Matériaux max | Texture max |
|---|---:|---:|---:|---:|
| fern_patch | 300 | 600 | 1 | 1024 |
| leaf_moss_mat | 250 | 500 | 1 | 1024 |
| rock_small | 400 | 800 | 1 | 1024 |
| rock_medium | 800 | 1500 | 1 | 1024 |
| trunk_common | 800 | 1500 | 2 | 1024 |
| root_cluster | 1000 | 2000 | 2 | 1024 |
| canopy_mass | 800 | 1500 | 1 | 1024 |
| hero_setpiece | 5000 | 9000 | 3 | 2048 |

## Colliders

| Asset | Collider recommandé |
|---|---|
| feuilles / mousse | aucun |
| fougère | aucun ou capsule simple si gameplay requis |
| rocher small/medium | primitive ou proxy simplifié |
| tronc décor | capsule/box approximative |
| fallen log près route | capsule/box composée |
| hero setpiece | proxy simplifié |

## Texture rules

- 512 ou 1024 par défaut.
- 2048 seulement hero asset explicitement validé.
- Pas de textures 4K dans la vertical slice forêt.
- Si une texture photoréaliste est utilisée, elle doit être stylisée/harmonisée.

## Pivot rules

- Pivot au sol pour assets placés par scatter.
- Pivot centré pour masses flottantes/canopée seulement si documenté.
- Transform appliqué avant export.
- Bounds plausibles en mètres.

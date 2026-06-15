# Scene Composition Validator Spec

## Objectif

Vérifier qu’une scène de preview ou candidate respecte les contraintes de composition route-first.

## Checks

### Scène

- Scène dans le projet canonique.
- Nom de scène explicite.
- Pas de dépendance externe non manifestée.
- Pas de Missing Script.
- Pas de Missing Material.

### Route

- Route object présent.
- Route camera présente.
- Route lisible / capture possible.
- Aucun prop dans une zone interdite proche du centre route.

### Caméras

- `RouteCamera` ou équivalent.
- `OverviewCamera` ou équivalent.
- FOV stable/documenté.
- Position de capture stable.

### Mock gameplay

- Source mock non cassée.
- Aucun changement gameplay requis par la scène art.

### Composition

- Densité par bande documentée.
- Objets proches à distance de sécurité.
- Aucun mur opaque continu sur toute la route.
- Silhouette line présente si ticket concerné.

### Lighting/fog

- Fog activé/désactivé explicitement documenté.
- Directional light principale présente si la scène le nécessite.
- Pas de gris uniforme comme seul outil de profondeur.

## Rapport

```txt
_bmad-output/unity-test-results/myb-scene-composition-validation.md
```

## Critères bloquants

- Missing script.
- Missing material visible.
- Route camera absente.
- Capture impossible.
- Prop qui obstrue directement la route.
- Asset IA en production sans manifest approuvé.

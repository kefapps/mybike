# Linear Import Guide

## Issues Linear créées

Les documents de ce pack utilisent directement les IDs Linear réels :
`MYB-141` à `MYB-153`.

Les fichiers dans `tickets/` sont donc les fiches de référence locales pour ces
issues. Toute mise à jour de cadrage doit conserver ces IDs.

## Labels suggérés

- `art-direction`
- `asset-pipeline`
- `unity`
- `blender-mcp`
- `ai-assets`
- `validation`
- `visual-regression`
- `forest-corridor`
- `performance`
- `workflow`

## Project Linear

`Art Rescue — Forest Corridor Vertical Slice`

## Ordre conseillé

Commencer par `MYB-141`, `MYB-142`, `MYB-143`, `MYB-144`, `MYB-145`, `MYB-146`.

Ces tickets créent la piste, les barrières et les panneaux. Ensuite seulement, on remet le vélo en descente avec les assets.

## Utilisation avec Codex

Chaque ticket contient un bloc `Prompt Codex prêt à coller`. Le plus efficace :

1. Ouvrir l’issue Linear existante.
2. Copier le prompt dans Codex avec le contexte du repo.
3. Demander à Codex de produire rapport + captures + validator.
4. Coller le résumé de sortie dans Linear.
5. Ne fermer qu’après validation visuelle.

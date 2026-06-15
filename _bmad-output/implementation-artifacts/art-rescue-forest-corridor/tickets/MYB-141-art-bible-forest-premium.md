# MYB-141 — Verrouiller l’art bible forêt premium

## Linear metadata

- **Priority**: P0
- **Labels**: art-direction, foundation, forest-corridor
- **Estimate**: 2 pts
- **Depends on**: Aucune
- **Parent / Project suggéré**: `Art Rescue — Forest Corridor Vertical Slice`

## Contexte

Le checkpoint MYB-137 prouve que le corridor peut être plus enveloppant, mais il ne définit pas encore ce que “beau”, “premium” et “stylisé” veulent dire pour MyBike. Sans contrat visuel, chaque outil improvise et la scène finit en soupe de poteaux marron.

## Objectif

Créer une art bible courte, dure, utilisable par Codex, Unity MCP, Blender MCP et les humains. Elle doit définir le style, les interdits, les familles d’assets, les palettes, les budgets et les captures de référence.

## Tâches


- Créer `docs/art-direction/mybike-forest-art-bible-v0.md`.
- Définir 5 principes visuels non négociables.
- Définir 3 palettes : forêt humide, clairière chaude, segment brumeux.
- Définir les formes autorisées/interdites : troncs, racines, fougères, rochers, canopée.
- Définir les budgets par asset courant et hero asset.
- Ajouter une section “ce qui doit être visible depuis la caméra route”.
- Ajouter une section “ce qui rend immédiatement l’image cheap”.


## Critères d'acceptation


- Le document tient en moins de 8 pages Markdown.
- Un agent Codex peut s’en servir sans poser de question stylistique majeure.
- Le document contient une grille de décision : `keep`, `rework`, `reject`.
- Les captures MYB-137 sont explicitement utilisées comme contre-exemples et non comme target final.


## Validation attendue


- Relecture humaine.
- La doc est référencée depuis `README.md` ou l’index docs du projet.
- Aucun asset n’est modifié dans ce ticket.


## Hors scope


- Pas de refonte gameplay.
- Pas de nouveau projet Unity.
- Pas de modification de `src/**` sauf besoin explicitement justifié.
- Pas d'appel à Meshy/Tripo sauf ticket dédié.
- Pas de clôture automatique du ticket Linear sans validation visuelle utilisateur.


## Prompt Codex prêt à coller

```txt
Tu travailles dans `kefapps/mybike`, projet Unity canonique `unity/Echapee4D`.

Objectif: créer une art bible courte pour le corridor forêt premium de MyBike.

Contraintes:
- Ne modifie pas la scène Unity.
- Ne génère aucun asset.
- Ne lance pas Meshy/Tripo.
- Utilise les rapports MYB-114 V4 et MYB-137 comme contexte.
- Traite MYB-137 comme volume proof, pas comme qualité cible.

Livrable:
- `docs/art-direction/mybike-forest-art-bible-v0.md`

Le document doit couvrir:
1. style cible;
2. interdits visuels;
3. palette;
4. familles d’assets;
5. budgets par asset;
6. grille keep/rework/reject;
7. critères de validation depuis la caméra route.
```

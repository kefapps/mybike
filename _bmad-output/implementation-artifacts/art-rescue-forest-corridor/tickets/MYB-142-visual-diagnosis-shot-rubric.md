# MYB-142 — Audit visuel MYB-137 + grille de validation par capture

## Linear metadata

- **Priority**: P0
- **Labels**: art-direction, qa, visual-regression
- **Estimate**: 3 pts
- **Depends on**: MYB-141
- **Parent / Project suggéré**: `Art Rescue — Forest Corridor Vertical Slice`

## Contexte

La progression V2 → V3 → V4 → MYB-137 est structurellement bonne, mais la lecture route reste faible. Il faut un diagnostic stable, pas juste “c’est moche” — même si là, oui, la forêt fait un peu chantier de mikados après tempête.

## Objectif

Écrire un audit visuel actionnable et une grille de scoring pour comparer les prochaines captures sans repartir au feeling à chaque ticket.

## Tâches


- Créer `docs/art-direction/myb-137-visual-diagnosis.md`.
- Décrire les problèmes visibles : poteaux, sol, fog, lumière, silhouette, hiérarchie, matériaux.
- Créer `docs/validation/forest-corridor-shot-rubric.md`.
- Définir une note de 1 à 5 sur : route readability, foreground framing, midground density, background depth, ground richness, silhouette quality, lighting mood.
- Ajouter les seuils : `prototype`, `acceptable vertical slice`, `target premium slice`.


## Critères d'acceptation


- Les problèmes sont reliés à des actions concrètes.
- La grille de scoring peut être utilisée sur toute capture future.
- Le document indique quels défauts MYB-137 doit résoudre ensuite et lesquels ne relèvent pas de MYB-137.


## Validation attendue


- Un screenshot route et un screenshot overview peuvent être scorés avec la grille.
- La grille produit au moins 5 actions prioritaires.


## Hors scope


- Pas de refonte gameplay.
- Pas de nouveau projet Unity.
- Pas de modification de `src/**` sauf besoin explicitement justifié.
- Pas d'appel à Meshy/Tripo sauf ticket dédié.
- Pas de clôture automatique du ticket Linear sans validation visuelle utilisateur.


## Prompt Codex prêt à coller

```txt
Analyse les captures et rapports MYB-137 sans modifier le projet.

Objectif: produire un diagnostic visuel exploitable et une grille de validation par capture.

Livrables:
- `docs/art-direction/myb-137-visual-diagnosis.md`
- `docs/validation/forest-corridor-shot-rubric.md`

Inclure:
- ce qui ne va pas visuellement;
- pourquoi le volume vertical ne suffit pas;
- quelles familles d’assets manquent;
- les checks route camera vs overview;
- un score 1-5 par critère;
- le seuil minimum pour considérer une passe comme validable.
```

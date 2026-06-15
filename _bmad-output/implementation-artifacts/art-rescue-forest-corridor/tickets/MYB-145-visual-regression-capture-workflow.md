# MYB-145 — Workflow captures avant/après + régression visuelle

## Linear metadata

- **Priority**: P0
- **Labels**: unity, qa, visual-regression, screenshots
- **Estimate**: 5 pts
- **Depends on**: MYB-142
- **Parent / Project suggéré**: `Art Rescue — Forest Corridor Vertical Slice`

## Contexte

Les tickets art doivent prouver leur effet avec des captures canoniques. Sinon
on se retrouve à débattre à l’œil nu autour d’un rendu qui lit encore comme un
prototype.

## Objectif

Standardiser les captures route/overview avant-après et les rapports de checkpoint.

## Tâches


- Définir deux caméras canoniques : `RouteCamera` et `OverviewCamera`.
- Générer automatiquement : route, overview, before-after route, before-after overview.
- Créer un rapport Markdown de métriques : renderers, triangles, familles d’assets, nombre de props, lumière/fog, erreurs.
- Écrire la documentation dans `docs/workflows/visual-checkpoint-workflow.md`.
- Ajouter un template de rapport dans `docs/templates/art-checkpoint-report-template.md`.


## Critères d'acceptation


- Chaque futur ticket art peut produire les mêmes 4 captures.
- Les captures vont dans `_bmad-output/implementation-artifacts/<ticket>/`.
- Le rapport indique explicitement si la passe est un proof, un checkpoint ou une candidate de production.


## Validation attendue


- Produire un exemple avec les scènes MYB-114 V4 et MYB-137 si disponibles.
- Aucun changement visuel de production requis.


## Hors scope


- Pas de refonte gameplay.
- Pas de nouveau projet Unity.
- Pas de modification de `src/**` sauf besoin explicitement justifié.
- Pas d'appel à Meshy/Tripo sauf ticket dédié.
- Pas de clôture automatique du ticket Linear sans validation visuelle utilisateur.


## Prompt Codex prêt à coller

```txt
Crée ou standardise le workflow de captures visuelles pour les tickets art.

Livrables:
- outils ou scripts Unity nécessaires;
- `docs/workflows/visual-checkpoint-workflow.md`;
- `docs/templates/art-checkpoint-report-template.md`;
- exemple de rapport dans `_bmad-output/unity-test-results/` si possible.

Contraintes:
- Ne pas modifier le gameplay.
- Ne pas faire de polish visuel dans ce ticket.
- Ne pas utiliser d’asset externe.

Le workflow doit produire:
- route capture;
- overview capture;
- before/after route sheet;
- before/after overview sheet;
- métriques textuelles.
```

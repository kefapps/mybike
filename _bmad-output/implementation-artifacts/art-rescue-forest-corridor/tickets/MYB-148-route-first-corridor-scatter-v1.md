# MYB-148 — Scatter route-first contrôlé par bandes

## Linear metadata

- **Priority**: P1
- **Labels**: unity, art-production, forest-corridor, scatter
- **Estimate**: 8 pts
- **Depends on**: MYB-147, MYB-145
- **Parent / Project suggéré**: `Art Rescue — Forest Corridor Vertical Slice`

## Contexte

MYB-114 a eu raison de passer au route-first. Il faut maintenant remplacer la pose “un peu partout” par des bandes contrôlées selon la distance à la route et la lisibilité caméra.

## Objectif

Créer un scatter contrôlé qui enrichit le corridor sans boucher la route ni produire une répétition évidente.

## Tâches


- Définir des bandes : shoulder, close edge, mid edge, back wall, silhouette line.
- Associer familles d’assets et densités par bande.
- Ajouter jitter contrôlé : position, rotation, scale, variant.
- Garder des fenêtres visuelles sur la route.
- Produire une scène preview dédiée, sans écraser MYB-137.
- Générer captures route/overview et rapport métriques.


## Critères d'acceptation


- Le corridor lit comme forêt, pas comme alignement de props.
- La route reste lisible à 20-40 m.
- La répétition d’asset n’est pas flagrante depuis la caméra route.
- Les objets proches ne traversent pas la route et ne clippent pas la caméra.


## Visual Progress Rule

If this ticket improves the visual result but does not reach Premium target, it
must not be closed as `Done`.

Allowed outcomes:

- remain `In Review` with `Checkpoint insufficient`;
- return to `In Progress` with a targeted corrective sub-ticket;
- close only with Julien's explicit documented exception;
- rollback/rework if the result moves the scene in the wrong direction.

A real improvement is valuable evidence, but it is not enough for `Done`.


## Canonical Visual Validation Surface

This ticket changes visible Art Rescue output.

Final visual validation must be done in the canonical ride corridor, from the
canonical ride camera.

Required final evidence:

- route screenshot from canonical ride camera;
- overview screenshot from the same scene state;
- visual rubric score;
- human validation when subjective.

Preview scenes, Blender renders, Meshy/Tripo previews, turntables and isolated
asset screenshots are allowed as intermediate evidence only. They cannot close
this ticket as `Done`.

The route screenshot is blocking. The overview screenshot is required but
secondary.


## Visual / Performance Arbitration

This ticket must prioritize `Stylisé Premium de Production`, then measure
performance. Performance is a guardrail, not an excuse for poor visuals.

Allowed:

- spend more geometry/material/density if the gain is visible from the canonical
  route camera;
- enrich while the scene meets FPS target and validators show no blocking
  `ERROR`.

If performance fails, reduce in this order:

1. unnecessary collisions;
2. oversized textures/material excess;
3. density outside the route camera;
4. LOD/culling/distance;
5. non-essential near-route density;
6. premium route-camera-visible elements last.

Required for review:

- route screenshot;
- overview screenshot;
- performance/validator result if this ticket changes density, assets,
  materials, collisions or lighting;
- note explaining whether the visual spend is visible from the route camera.


## Validation attendue


- Captures route/overview avant-après.
- Rapport indiquant densité par bande et familles utilisées.
- Scene composition validator sans ERROR bloquant.


## Hors scope


- Pas de lighting final.
- Pas de Meshy/Tripo.
- Pas de terrain infini.
- Pas de nouveau système runtime complexe si un builder Editor suffit.


## Prompt Codex prêt à coller

```txt
Dans Unity, créer une passe de scatter route-first contrôlée par bandes pour le corridor forêt.

Utilise le kit validé MYB-147 si présent. Sinon utilise des placeholders propres, mais documente la limitation.

Contraintes:
- Ne pas écraser les scènes MYB-114/MYB-137 existantes.
- Créer une scène preview dédiée.
- Route lisible depuis la caméra première personne.
- Densité par bande, pas placement aléatoire total.

Livrables:
- scène preview;
- builder/editor script si nécessaire;
- captures route/overview;
- rapport `_bmad-output/implementation-artifacts/MYB-148/...`;
- notes de densité par bande.
```

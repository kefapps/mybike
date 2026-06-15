# MYB-149 — Pass matière sol forestier

## Linear metadata

- **Priority**: P1
- **Labels**: art-production, materials, forest-floor
- **Estimate**: 5 pts
- **Depends on**: MYB-147, MYB-148
- **Parent / Project suggéré**: `Art Rescue — Forest Corridor Vertical Slice`

## Contexte

Le sol actuel est l’un des plus gros tue-l’amour visuels : il lit comme une
masse marron/grise, pas comme un sous-bois. Il faut de la matière crédible, mais
pas une bouillie de textures réalistes collées sur des formes symboliques.

## Objectif

Créer une richesse de sol stylisée : feuilles, mousse, terre, pierres, racines basses, transitions propres route/berme.

## Tâches


- Définir 3 matériaux sol stylisés : terre humide, feuilles mortes, mousse/lichen.
- Ajouter des patches géométriques optimisés plutôt que tout miser sur texture.
- Améliorer la transition route → shoulder → berme.
- Ajouter variations de teinte contrôlées.
- Préserver la lisibilité de la route.
- Générer captures et rapport.


## Critères d'acceptation


- Le sol ne lit plus comme un ruban plat.
- Les patches ont une logique de placement par bande.
- La matière enrichit le premier plan sans bruit visuel excessif.
- Pas de textures énormes ou matériaux non manifestés.


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


- Asset validator OK.
- Visual rubric : ground richness au moins 3/5.
- Captures route/overview.


## Hors scope


- Pas de shader terrain complexe.
- Pas de photogrammétrie brute.
- Pas de Meshy/Tripo.
- Pas de refonte route mesh.


## Prompt Codex prêt à coller

```txt
Effectue un pass matière du sol forestier dans la scène de corridor route-first.

Objectif visuel:
- terre + feuilles + mousse + racines basses;
- pas de photoréalisme;
- stylisé premium;
- lisible depuis caméra route.

Livrables:
- matériaux/prefabs nécessaires;
- scène preview ou update de la preview de production;
- captures route/overview;
- rapport métriques;
- note sur les textures, tailles et matériaux.

Contraintes:
- pas de Meshy/Tripo;
- pas de textures > budget;
- préserver route mock/ride.
```

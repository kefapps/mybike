# MYB-151 — Pass lumière, fog et ambiance

## Linear metadata

- **Priority**: P1
- **Labels**: lighting, fog, atmosphere, art-production
- **Estimate**: 5 pts
- **Depends on**: MYB-145, MYB-149
- **Parent / Project suggéré**: `Art Rescue — Forest Corridor Vertical Slice`

## Contexte

La scène actuelle est uniformément grisâtre. Le fog cache plus qu’il ne compose. La lumière ne sculpte pas les volumes. À ce stade, même les bons assets auraient l’air fatigués.

## Objectif

Créer une ambiance lisible et premium avec profondeur, séparation des plans, lumière douce et fog contrôlé.

## Tâches


- Définir 2 presets : `ForestMorningMist` et `WarmClearing`.
- Régler fog couleur/densité selon profondeur.
- Ajouter une direction de lumière claire.
- Créer séparation foreground/midground/background.
- Vérifier que les silhouettes restent lisibles.
- Captures route/overview pour chaque preset ou preset principal.


## Critères d'acceptation


- L’image a une profondeur claire.
- Le fog ne transforme pas tout en gris uniforme.
- Les assets proches gardent une valeur lisible.
- Le ciel/fond ne ressemble plus à un mur plat.


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


- Visual rubric : lighting mood au moins 3/5 et background depth au moins 3/5.
- Rapport mentionnant paramètres fog/lumière principaux.
- Captures avant/après.


## Hors scope


- Pas de post-process lourd si non nécessaire.
- Pas de benchmark GPU complet sauf anomalie.
- Pas de réécriture pipeline de rendu.


## Prompt Codex prêt à coller

```txt
Crée un pass lumière/fog/ambiance pour le corridor forêt.

Contraintes:
- Garder le style stylisé premium.
- Ne pas masquer les défauts par brouillard excessif.
- Préserver les performances et le mode mock.

Livrables:
- preset ou configuration documentée;
- captures route/overview avant-après;
- rapport `_bmad-output/implementation-artifacts/MYB-151/...`;
- valeurs principales de fog/lumière dans le rapport.

Vérifier:
- profondeur;
- contraste doux;
- lisibilité route;
- silhouettes;
- absence de matériau rose/cassé.
```

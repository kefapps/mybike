---
title: "Proposition - Scenic Mood Pass procedural"
project: "mybike"
date: "2026-06-06"
workflow: "gds-correct-course"
status: "decision-ready"
source_comparison: "_bmad-output/implementation-artifacts/visual-upgrade-comparison-2026-06-06.md"
baseline_capture: "_bmad-output/video-captures/ride-visual-audit-2026-06-06T20-16-57-308Z/"
post_myb8_capture: "_bmad-output/video-captures/ride-visual-audit-2026-06-06T21-15-32-492Z/"
selected_direction: "Scenic Mood Pass procedural"
next_action: "create-next-visual-story"
---

# Proposition - Scenic Mood Pass procedural

## Decision

Choisir une seule direction: **Scenic Mood Pass procedural**.

MYB-8 a fait passer la scene de prototype plat a premiere balade scenic
stylisee: route plus lisible, bas-cotes, objets lateraux et meilleure profondeur.
La comparaison video montre cependant que le prochain manque n'est pas une
nouvelle feature de gameplay, ni un pipeline asset, ni d'abord la camera. Le
manque restant est une identite de lieu forte: relief, silhouettes, lumiere,
fog, ciel et motifs memorables par biome.

## Comparaison des options

1. **Scenic Mood Pass procedural - retenu**
   - Meilleur levier maintenant car il attaque directement les manques observes:
     monde encore generique, terrain horizontal, atmosphere trop plate.
   - Compatible avec la vertical slice mock: pas de nouvelle route, pas de
     hardware, pas de service externe.
   - Produit une story bornable et verifiable par capture 30s.

2. **Meshy cible pour landmarks - plus tard**
   - Utile seulement si un landmark precis devient difficile a produire en
     procedural.
   - Maximum envisage: 1 a 2 assets reutilisables et instanciables.
   - Toute utilisation Meshy devra etre confirmee avant appel, avec rappel du
     cout en credits.
   - Pas utile comme direction principale maintenant: des assets poses dans une
     composition encore plate ne regleraient pas le probleme de mood.

3. **Camera/speed feel pass - non retenu maintenant**
   - MYB-7 a deja traite la sensation de ride et MYB-8 a ajoute du parallax.
   - La capture post-MYB-8 pointe surtout un deficit de paysage identifiable,
     pas une urgence de camera.
   - A garder pour plus tard si un playtest signale que la scene progresse mais
     que la vitesse reste peu ressentie.

## Mini-scope borne

Story unique proposee, sans creation d'issue Linear a ce stade:

1. **Motif fort par biome**
   - Coast: phare lointain, ponton, falaise ou arche rocheuse procedural.
   - Forest: tunnel vegetal, clairiere, troncs silhouettes ou canopee marquee.
   - Village/valley si present dans les donnees: silhouettes de maisons,
     belvedere, muret ou panneau distinctif.

2. **Relief et composition**
   - Bermes, talus, buttes laterales ou silhouettes de terrain autour de la
     route.
   - Near/mid/far planes plus lisibles sans multiplier les assets.
   - Variation de hauteur visible dans la capture 30s.

3. **Lumiere, fog, ciel, mood**
   - Mood identifiable par biome via couleur de ciel, fog, direction de lumiere
     et contraste doux.
   - Rester stylise et lisible, sans cycle jour/nuit ni systeme meteo.

## Hors scope

- Nouvelle route, route selector, GPX ou editeur.
- BLE, FTMS, Web Bluetooth, backend, persistance ou deploiement.
- Refonte HUD ou nouveau mode de gameplay.
- Pipeline asset lourd, glTF obligatoire, bibliotheque AI/Meshy.
- Plus d'une story visuelle ou creation d'un gros backlog.
- Appel Meshy sans confirmation explicite du cout.

## Meshy

Meshy n'est **pas utile maintenant** pour cadrer la prochaine story. Il devient
utile plus tard seulement si le prompt de story identifie un landmark precis,
simple et instanciable qu'un mesh procedural ferait mal.

Exemples acceptables plus tard: phare low-poly, arche rocheuse, borne cyclable,
petit pont ou belvedere simple. Maximum: 1 a 2 assets. Avant tout appel Meshy,
demander confirmation du cout en credits.

## Prompt de story suivant

```text
gds-create-story

Creer une story unique post-MYB-8 pour "Scenic Mood Pass procedural".

Contexte:
- MYB-8 est Done et commit.
- Decision choisie:
  _bmad-output/planning-artifacts/proposition-scenic-mood-pass-procedural-2026-06-06.md
- Comparaison source:
  _bmad-output/implementation-artifacts/visual-upgrade-comparison-2026-06-06.md
- Baseline:
  _bmad-output/video-captures/ride-visual-audit-2026-06-06T20-16-57-308Z/
- Post-MYB-8:
  _bmad-output/video-captures/ride-visual-audit-2026-06-06T21-15-32-492Z/

Objectif:
- Une seule story, pas de backlog.
- Ameliorer l'identite scenic par biome: motif fort, relief/silhouettes,
  lumiere/fog/ciel/mood.
- Rester procedural et mock-first.
- Meshy hors scope par defaut; possible seulement plus tard pour 1 a 2
  landmarks precis, avec confirmation du cout avant tout appel.

Validation attendue:
- npm run typecheck
- npm run test
- npm run build
- capture video 30s ou screenshots comparables si l'outil est disponible
```

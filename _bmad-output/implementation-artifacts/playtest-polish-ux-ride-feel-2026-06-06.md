---
title: "Plan de playtest court - Polish UX / sensation de ride"
project: "mybike"
date: "2026-06-06"
scope: "post-MVP polish UX / sensation de ride"
status: "ready-to-run"
source_commit: "39188669ac8a8cee9c45dacafdf5173e09dbc2fa"
---

# Plan de playtest court - Polish UX / sensation de ride

## Objectif

Valider rapidement le ressenti de la vertical slice mock avant de creer une story ou un micro-epic. Le playtest doit produire 3 a 5 ajustements maximum, pas un backlog large.

## Format

- Duree cible: 10 a 15 minutes.
- Plateformes: desktop puis mobile.
- Build: app locale MVP mock.
- Participants: 1 personne a la fois, interne ou proche projet.
- Mode: observation silencieuse, intervention seulement si blocage technique.

## Parcours

1. Ouvrir l'app.
2. Lancer la balade mock.
3. Observer le ride pendant quelques instants avec l'effort par defaut.
4. Bouger le slider vers une valeur basse, puis moyenne, puis haute.
5. Observer la route, les biomes, le HUD et la camera.
6. Mettre en pause, reprendre, puis terminer.
7. Lire le resume de session.
8. Refaire rapidement le meme parcours sur mobile.

## Zones observees

| Zone | Question | Note 1-3 | Observation courte |
|---|---|---:|---|
| Confort camera | La camera est-elle douce et comprehensible, sans mouvement brutal ? |  |  |
| Route / biomes | La route et le changement coast/forest sont-ils lisibles ? |  |  |
| Slider | Le lien effort -> vitesse ressentie est-il clair ? |  |  |
| HUD | Vitesse, distance, temps, source et phase sont-ils lisibles pendant le ride ? |  |  |
| Resume | Duree, distance et vitesse moyenne semblent-ils coherents apres finish ? |  |  |
| Mobile | Le parcours reste-t-il jouable et lisible sur petit ecran ? |  |  |

Notation:

- 1 = gene ou confusion nette.
- 2 = acceptable mais ajustement souhaitable.
- 3 = clair, confortable, pas d'action necessaire.

## Notes d'observation

Utiliser des phrases courtes:

```text
[desktop/mobile] [zone] [note] [observation] [idee d'ajustement si evidente]
desktop camera 2 leger inconfort dans les virages reduire look-ahead ou smoothing
mobile HUD 1 chiffres difficiles a lire agrandir/espacer les metriques
```

## Criteres de selection des ajustements

Apres le playtest, retenir 3 a 5 ajustements maximum avec cette priorite:

1. Corriger tout blocage ou confusion qui empeche de finir le parcours.
2. Prioriser les notes `1` observees sur desktop et mobile.
3. Prioriser les problemes touches par plusieurs zones, par exemple camera + route.
4. Garder les ajustements proches de l'experience existante: camera, route visible, slider, HUD, resume.
5. Rejeter les idees qui ajoutent une feature, un systeme, BLE/FTMS, deploiement, assets lourds ou backlog large.

Le hardening `WebGLRenderer` n'entre dans les ajustements que si un echec reel est observe pendant le lancement ou le ride. Sinon, il reste un risque technique a surveiller, pas une action de polish prioritaire.

## Decision attendue

Sortie du playtest:

- 3 a 5 ajustements maximum, chacun avec zone, preuve observee, impact et taille percue.
- Ou decision de ne rien creer si les notes sont toutes `3` et aucun blocage n'apparait.

Prochaine etape apres observation: creer une petite story ou un micro-epic seulement si les ajustements retenus justifient une implementation.

---
title: "Resultats self-playtest - Polish UX / sensation de ride"
project: "mybike"
date: "2026-06-06"
scope: "post-MVP polish UX / sensation de ride"
status: "completed"
plan: "_bmad-output/implementation-artifacts/playtest-polish-ux-ride-feel-2026-06-06.md"
source_commit: "8839bff567ae1577849d82c8694fbe546c127e52"
---

# Resultats self-playtest - Polish UX / sensation de ride

## Setup

- App lancee localement via Vite: `http://127.0.0.1:5174/`.
- Validation HTTP: `HTTP/1.1 200 OK`.
- Parcours desktop et mobile execute via Chrome headless/CDP.
- Parcours couvert: start -> ride -> slider bas/moyen/haut -> pause -> resume -> finish -> summary.
- Captures temporaires:
  - `/tmp/mybike-polish-desktop-ride.png`
  - `/tmp/mybike-polish-desktop-summary.png`
  - `/tmp/mybike-polish-mobile-ride.png`
  - `/tmp/mybike-polish-mobile-summary.png`

## Preuves legeres

ImageMagick sur captures:

```text
mybike-polish-desktop-ride.png 1280x800 colors=2216 mean=0.846692 stddev=0.169318
mybike-polish-desktop-summary.png 1280x800 colors=1636 mean=0.914803 stddev=0.119809
mybike-polish-mobile-ride.png 780x1688 colors=3486 mean=0.796108 stddev=0.214854
mybike-polish-mobile-summary.png 780x1688 colors=1922 mean=0.886518 stddev=0.185692
```

Notes:

- Les screenshots desktop/mobile sont non blank.
- La lecture directe du canvas WebGL via `drawImage/getImageData` retourne noir, probablement lie au buffer WebGL; les preuves non blank viennent donc des captures page.
- Aucun echec `WebGLRenderer` observe pendant lancement ou ride.

## Observations

| Zone | Note | Observation |
|---|---:|---|
| Confort camera | 3 | Camera stable et douce pendant le parcours court; pas de mouvement brutal observe. |
| Route / biomes | 2 | Route centrale lisible; biome/ambiance peu qualifiable dans ce run court car la transition n'est pas atteinte visiblement. |
| Ressenti slider | 2 | Le slider reagit et la vitesse augmente bas -> moyen -> haut, mais l'effet peut sembler indirect a cause du smoothing. |
| Lisibilite HUD desktop | 3 | HUD lisible, compact, n'empeche pas de voir la route. |
| Lisibilite HUD mobile | 1 | HUD lisible mais trop dominant; il occupe une grande partie du haut du canvas et reduit la lecture de la scene. |
| Coherence resume | 3 | Summary coherent avec duree, distance et vitesse moyenne apres finish. |

## Validation attendue

- HTTP 200: OK.
- Canvas non blank desktop/mobile: OK par captures.
- Slider reagit: OK, vitesse observee en hausse jusqu'a 25,7 km/h desktop et 22,1 km/h mobile pendant le run.
- Pause/resume coherent: OK, HUD stable pendant pause, progression repart apres resume.
- Summary coherent: OK, desktop `0 min 04 s`, `18 m`, `14,6 km/h`; mobile `0 min 03 s`, `13 m`, `13,8 km/h`.

## Ajustements proposes

Retenir 3 ajustements maximum:

1. Compacter le HUD mobile pour liberer la scene.
   - Preuve: en mobile, le HUD prend une grande zone au-dessus de la route et coupe la lecture du ciel/route.
   - Impact: meilleur ressenti de ride et meilleure lisibilite de la scene sur petit ecran.

2. Rendre la reponse du slider plus comprehensible.
   - Preuve: la vitesse reagit bien, mais le smoothing rend le lien effort -> vitesse moins immediat a percevoir.
   - Impact: le joueur comprend mieux que son action modifie l'allure.

3. Rendre le changement de biome observable pendant un playtest court.
   - Preuve: la route est lisible, mais la transition coast/forest n'est pas atteinte dans ce run outille court.
   - Impact: le playtest peut vraiment juger la lisibilite route / biomes sans rallonger fortement la session.

## Hors scope maintenu

- Pas de nouvelle feature de gameplay.
- Pas de BLE/FTMS.
- Pas de deploiement.
- Pas de gros backlog.
- Pas de nouvelle issue Linear a ce stade.
- Pas de hardening `WebGLRenderer` retenu, car aucun echec reel n'a ete observe.

## Decision

Prochaine etape recommandee: choisir lesquels de ces 3 ajustements deviennent le mini-scope polish. Ne pas creer plus de 3 a 5 actions, et ne creer une story ou un micro-epic qu'apres cette selection.

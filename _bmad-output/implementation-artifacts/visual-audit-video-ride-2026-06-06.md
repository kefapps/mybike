---
title: "Audit video visuel - ride mock"
project: "mybike"
date: "2026-06-06"
source: "capture video Playwright + ffmpeg"
status: "completed"
---

# Audit video visuel - ride mock

## Capture

- Capture locale: `_bmad-output/video-captures/ride-visual-audit-2026-06-06T20-16-57-308Z/`
- Video: `ride-visual-audit-30s.mp4`
- Contact sheet: `ride-visual-audit-contact-sheet.jpg`
- Duree mesuree: 32.36 s
- Viewport: 1440 x 900
- HTTP: 200 OK sur `http://127.0.0.1:5174/`
- Frames extraites: 6
- Page errors: aucune
- Console: un 404 isole observe, probablement favicon/asset absent; aucun crash rendu.

Les fichiers video et frames sont volontairement ignores par git via
`_bmad-output/video-captures/`.

## Verdict court

La vertical slice est jouable et lisible, mais l'image reste trop pauvre pour
porter une ambition de balade 3D. Le probleme n'est pas seulement le manque
d'assets: la scene manque de composition, de profondeur, de matiere, de relief
et d'elements proches qui donnent une sensation de mouvement.

## Constats visuels

1. **Image trop plate**
   - Le sol, le ciel et la route sont de grands aplats.
   - La route ressemble a un ruban uni sans texture, bas-cotes, marquage,
     deformation ou integration dans le terrain.

2. **Profondeur insuffisante**
   - L'horizon est lisible mais vide.
   - Les rares elements de biome sont petits, lointains et peu contrastes.
   - Il manque des plans proches, moyens et lointains pour donner de l'echelle.

3. **Mouvement peu spectaculaire**
   - La vitesse est fonctionnelle dans le HUD, mais pas assez ressentie a l'image.
   - Il manque des repaires lateraux proches, parallax, variation de route ou
     micro-relief pour traduire le deplacement.

4. **Identite de lieu trop faible**
   - Coast/forest est observable, mais les biomes ne racontent pas encore un lieu.
   - Les couleurs changent, mais la silhouette et la densite de decor changent peu.

5. **HUD utile mais dominant**
   - Le HUD et les controles sont clairs.
   - Sur desktop, l'interface encadre beaucoup l'experience alors que la scene
     devrait devenir le signal principal.

## Direction visuelle recommandee

Passer d'une scene "route ruban sur plan" a une **balade scenic stylisee**:

- route avec bas-cotes, texture ou bandes simples;
- terrain proche avec relief leger ou variations de couleur;
- objets lateraux simples mais nombreux: arbres, rochers, poteaux, herbes,
  barrieres ou panneaux;
- brume/horizon/ciel plus travaillés pour creer de la profondeur;
- densite de biome plus forte et plus reconnaissable;
- camera et FOV ajustes pour augmenter la sensation de vitesse sans nuire au confort.

## Proposition de prochain scope

Creer une seule story de **visual upgrade scenic pass**, limitee a 3 ou 4
ameliorations visibles:

1. Enrichir la route et les bas-cotes.
2. Ajouter une population laterale procedural simple par biome.
3. Ajouter profondeur atmospherique / horizon / variation terrain.
4. Ajuster camera/FOV/vitesse percue si necessaire.

Hors scope a conserver:

- assets IA/Meshy;
- routes multiples;
- BLE/FTMS;
- deploiement;
- pipeline asset lourd;
- optimisation bundle avancee;
- refonte HUD.

## Prochaine action

Utiliser `gds-correct-course` pour transformer cet audit video en mini-scope
visuel borne, puis creer une seule story si le scope est accepte.

# Route-first Corridor Grammar

## Principe

La route est la colonne vertébrale. Tout le décor doit être composé par rapport à elle.

On ne place pas une forêt puis une route dedans. On compose une expérience de ride : route lisible, bords riches, plans de profondeur, silhouettes contrôlées.

## Bandes de composition

### 1. Road surface

Rôle : gameplay, lecture de direction, vitesse.

Règles :

- propre ;
- contrastée mais pas flashy ;
- pas de props ;
- marques/variations discrètes.

### 2. Shoulder / verge

Rôle : transition route → forêt.

Assets :

- petites feuilles ;
- mousse ;
- racines basses ;
- petits cailloux ;
- herbes/fougères basses.

Règles :

- densité modérée ;
- pas d’objet haut collé à la caméra ;
- variations de couleur pour casser les rubans.

### 3. Close edge

Rôle : enveloppement et vitesse perçue.

Assets :

- troncs inclinés ;
- stumps ;
- roots ;
- rochers moyens.

Règles :

- silhouettes expressives ;
- distance de sécurité caméra ;
- éviter les cylindres droits ;
- alternance dense/ouvert.

### 4. Mid edge

Rôle : densité forestière.

Assets :

- troncs variés ;
- canopée basse ;
- masses sombres ;
- rochers ;
- setpieces secondaires.

Règles :

- plus dense que close edge ;
- garder des fenêtres visuelles ;
- limiter répétition visible.

### 5. Back wall / silhouette line

Rôle : profondeur et horizon.

Assets :

- arbres lointains ;
- masses de canopée ;
- formes verticales simplifiées.

Règles :

- valeurs plus douces ;
- fog contrôlé ;
- pas de détails inutiles.

## Densité recommandée

| Bande | Densité | Taille visuelle | Risque |
|---|---:|---|---|
| Road | 0 | n/a | obstruction |
| Shoulder | faible à moyenne | très basse | bruit |
| Close edge | moyenne | moyenne/haute | clipping / poteaux |
| Mid edge | moyenne à forte | haute | répétition |
| Back wall | forte mais simplifiée | grande masse | mur plat |

## Règle de variation

Chaque famille répétée doit varier au moins deux paramètres :

- variant mesh ;
- rotation ;
- scale ;
- teinte ;
- inclinaison ;
- offset latéral ;
- hauteur de base.

## Règle de respiration

Tous les 30–50 mètres, garder une fenêtre visuelle ou un changement de rythme. Une forêt dense en continu devient un papier peint, ce qui est injuste pour les arbres et pour les yeux.

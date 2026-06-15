> Non-canonical imported Art Rescue source material.
>
> The canonical product art bible lives at:
> `docs/art-direction/mybike-forest-art-bible-v0.md`
>
> If this file conflicts with the canonical art bible, the canonical art bible wins.

# MyBike Forest Corridor — Art Bible v0

## Intention

Une balade indoor stylisée premium dans un corridor forestier lisible, enveloppant, calme et élégant. Le joueur doit sentir la progression sur une route de forêt, pas traverser un showroom d’assets posés sur un tapis marron.

## Style cible

- Stylisé Premium de Production.
- Simple, lisible, crédible, pas cheap.
- Formes simples mais expressives.
- Matière suggérée par volumes et palettes, pas par bruit texture excessif.
- Route toujours lisible.
- Environnement enveloppant, mais jamais oppressant au point de gêner la conduite.

## Interdits visuels

- Cylindres bruts utilisés comme troncs finaux.
- Route/terrain générés par IA 3D externe.
- Photogrammétrie brute mélangée avec un rendu stylisé premium.
- Textures hyper détaillées sur meshes symboliques.
- Fog gris uniforme qui lave toute l’image.
- Surdensité de props sans hiérarchie.
- Objets qui frôlent trop la caméra route.
- Assets sans manifest, source ou licence.

## Palette v0

### Forest Morning Mist

- Sol : brun froid, terre humide, feuilles désaturées.
- Végétation : verts doux, mousse légèrement lumineuse.
- Bois : brun chaud moyen, valeurs séparées du sol.
- Fog : bleu/vert très désaturé.
- Accent : petits verts frais sur fougères et mousses.

### Warm Clearing

- Sol : brun chaud, feuilles orange/dorées légères.
- Lumière : jaune doux, direction latérale.
- Végétation : vert olive.
- Fog : chaud léger, moins dense.

### Deep Edge

- Arrière forêt : verts sombres et bleutés.
- Foreground : contraste plus fort.
- Midground : silhouettes claires, pas noires absolues.

## Familles d’assets

### Core corridor

- segments route ;
- shoulders ;
- bermes ;
- talus ;
- ridges ;
- transitions route/sol.

### Forest floor

- feuilles basses ;
- moss mats ;
- racines basses ;
- fougères ;
- branches mortes ;
- rochers semi-enterrés.

### Vertical volume

- troncs droits irréguliers ;
- troncs penchés ;
- troncs cassés ;
- stumps ;
- bases évasées ;
- root clusters.

### Silhouettes fortes

- fallen log ;
- arche légère de branches/racines ;
- souche massive ;
- rocher moussu ;
- arbre penché au-dessus du bord route ;
- masse de canopée.

## Budgets de départ

| Type | Triangles cible | Matériaux | Texture |
|---|---:|---:|---:|
| Fern/leaf patch | 50–250 | 1 | aucune ou 512 |
| Rocher courant | 100–500 | 1 | 512–1024 |
| Tronc courant | 200–800 | 1–2 | 512–1024 |
| Root cluster | 200–1000 | 1–2 | 512–1024 |
| Hero prop | 1000–5000 | 1–3 | 1024–2048 |
| Canopy mass | 100–800 | 1 | aucune ou 512 |

## Règle route camera

Depuis la caméra route :

- la route doit être lisible à 20–40 mètres ;
- le premier plan doit avoir de la matière mais ne pas voler toute l’attention ;
- les bords doivent former un corridor, pas une barrière ;
- au moins une silhouette forte doit être visible dans une capture route de référence ;
- aucun objet ne doit lire comme cylindre par défaut au premier plan.

## Grille keep / rework / reject

### Keep

- silhouette lisible ;
- scale plausible ;
- matériaux cohérents ;
- pivot propre ;
- respecte budget ;
- source/manifest OK.

### Rework

- bonne idée mais forme trop simple ;
- matériaux à harmoniser ;
- pivot/bounds corrigibles ;
- texture trop grande mais réduisible ;
- silhouette lisible seulement sous certains angles.

### Reject

- style réaliste incohérent ;
- mesh trop sale ;
- licence/source floue ;
- pivots/échelle absurdes ;
- texture ou matériaux ingérables ;
- asset qui ne contribue pas à la route camera.

# MYB-41 - Shortlist assets 3D gratuits V1 compatibles Unity

Date: 2026-06-11
Ticket Linear: https://linear.app/kefjbo/issue/MYB-41/myb-007-shortlister-les-modeles-3d-gratuits-v1-compatibles-unity
Statut: review

## Objectif

Produire une shortlist d'assets gratuits compatibles avec la direction Unity
macOS-first, prete pour un POC dans `unity/Echapee4D`.

## Decisions de cadrage

- Scan large: 25 a 30 candidats.
- Shortlist finale: 8 a 12 retenus.
- Les candidats retenus doivent couvrir quatre familles:
  - kits de biome;
  - surfaces de route / materiaux;
  - props / ambiance;
  - signaux premium / hero assets.
- Les deux biomes V1 prioritaires sont `Foret claire` et
  `Village / campagne pavee`.
- La shortlist finale compte uniquement les candidats avec source, auteur,
  licence et metadonnees verifiables selon MYB-38.
- Licences acceptees par defaut: `CC0`, `Public Domain`, `CC-BY`.
- Les candidats Unity Asset Store gratuits vont en annexe `needs-review /
  exception possible`, sauf validation explicite.
- Les candidats prometteurs mais flous restent hors quota final.
- Les assets finalistes doivent etre majoritairement Unity-ready.
- Les assets a transformer via Blender/pipeline restent minoritaires et doivent
  justifier une forte valeur artistique.
- Les signaux premium peuvent etre plus lourds ou ambitieux si leur role est
  explicite et le risque performance est note.

## Inspection obligatoire

MYB-41 ne se contente pas des descriptions de packs.

Chaque finaliste doit inclure:

- inspection visuelle: style, silhouette, densite, lisibilite, risque cheap,
  adequation `Stylise Premium`;
- inspection technique: source, auteur, licence, formats, taille si disponible,
  textures/materiaux, animation si disponible, effort d'integration Unity,
  risque macOS/WebGL;
- verdict: `shortlist`, `needs-review`, ou `rejected`;
- usage V1: biome, famille et role dans un futur POC.

L'inspection ne signifie pas import Unity dans ce ticket. L'import, les reglages
Unity et les captures en scene appartiennent aux tickets POC/import suivants.

## Grille d'evaluation

| Champ | Attendu |
| --- | --- |
| Candidat | Nom exact de l'asset, pack ou source. |
| Source | URL canonique. |
| Auteur | Auteur ou organisation source. |
| Licence | Licence visible sur la source. |
| Famille | Biome kit, surface/material, prop/ambiance, signal premium. |
| Biome cible | Foret claire, Village / campagne pavee, commun, ou annexe. |
| Inspection visuelle | Ce qui est vu, pas seulement decrit. |
| Inspection technique | Formats, taille, textures, animation, effort Unity/macOS. |
| Risque | Licence, style, performance, integration, WebGL si applicable. |
| Verdict | shortlist, needs-review, rejected. |

## Resultats

### Methode d'inspection

Inspection effectuee depuis les pages sources officielles ou les pages auteur
quand elles exposent a la fois apercus, licence et details techniques.

Ce ticket ne valide pas encore le rendu in-engine. Les mentions "vu" ci-dessous
signifient: apercus/renders/pages source inspectes, pas import Unity.

### Niveau de preuve technique

- Archives inspectees en streaming quand un zip public direct etait disponible:
  `Kenney Nature Kit`, `Kenney Fantasy Town Kit`,
  `ambientCG Paving Stones 141` et `ambientCG Paving Stones 057`.
- Arborescences ou API inspectees quand disponibles sans telechargement durable:
  `KayKit Medieval Hexagon Pack` via GitHub tree, `Poly Haven Forest Ground 03`
  via API Poly Haven.
- Pages sources et captures/previews inspectees quand les telechargements sont
  controles par Itch/JS ou par une page purchase: Quaternius MegaKits, KayKit
  Medieval Builder, Poly Pizza Farm Animal Pack.
- Aucun asset n'a ete ajoute au repo et aucun import Unity n'a ete effectue.

## Shortlist finale proposee

11 candidats sont retenus. C'est volontairement dans la fourchette 8-12, avec
une marge pour que le POC suivant puisse choisir sans repartir de zero.

| ID | Candidat | Famille / biome | Licence | Inspection visuelle | Inspection technique | Verdict POC |
| --- | --- | --- | --- | --- | --- | --- |
| S1 | [Quaternius - Stylized Nature MegaKit](https://quaternius.itch.io/stylized-nature-megakit) | Kit de biome / `Foret claire` | CC0 | Screenshots source inspectes: forets claires, sentier, rochers, fleurs, arbres a grandes masses colorees. Le rendu est le plus proche de `Stylise Premium` pour une foret V1, avec potentiel de passages ouverts puis plus denses. | Page source inspectee: 116 modeles annonces, FBX/OBJ/glTF, Standard 99 MB; la capture "Standard Free" indique 81 assets gratuits. Source payante avec scenes Unity URP et shaders. Risque: verifier la frontiere exact free/pro/source avant import, puis limiter vegetation/shaders. | `shortlist` prioritaire foret. |
| S2 | [Kenney - Nature Kit](https://kenney.nl/assets/nature-kit) | Props / `Foret claire` | CC0 | Apercus simples, lisibles, trees/rocks/foliage tres propres mais moins premium. Bon fond de scene, pas hero asset. | Archive zip inspectee en streaming: 329 `fbx`, 329 `obj`, 329 `glb`, 329 `dae`, 329 `stl`, 1640 `png`, 1 `txt`. Integration probablement facile; risque de rendu cheap si utilise seul sans lumiere/material pass. | `shortlist` support forestier. |
| S3 | [Quaternius - Medieval Village MegaKit](https://quaternius.itch.io/medieval-village-megakit) | Kit de biome / `Village / campagne pavee` | CC0 | Screenshots source inspectes: village pierre/bois/toits rouges, verdure, modules de murs/toits/details. Le rendu est nettement plus premium que les kits basiques. | Page source inspectee: 300+ modeles annonces, FBX/OBJ/glTF, Standard 153 MB; capture "Standard Free" indique 170 assets gratuits. Source payante avec Unity URP, shaders et collisions. Risque: verifier le perimetre gratuit et ne pas importer tout le kit. | `shortlist` prioritaire village. |
| S4 | [Kenney - Fantasy Town Kit](https://kenney.nl/assets/fantasy-town-kit) | Kit de biome / `Village / campagne pavee` | CC0 | Apercus de bourg medieval simple et coherent; style propre mais plus "game jam" que premium. Utile comme kit de composition ou fallback. | Archive zip inspectee en streaming: 167 `fbx`, 167 `glb`, 167 `obj`, 167 `mtl`, 174 `png`, 1 `txt`. Integration facile; necessite polish lumiere/materials pour eviter "pack depose". | `shortlist` village leger. |
| S5 | [KayKit - Medieval Builder Pack](https://kaylousberg.itch.io/kaykit-medieval-builder-pack) | Kit village + route / `Village / campagne pavee` | CC0 | Apercus hex/square/free placement avec batiments, murs, routes, eau, decor. Lecture tres claire; peut aider a prototyper des villages et chemins. | Page source/purchase inspectee: pack legacy, 200+ assets annonces, download `KayKit Medieval Builder Pack 1.0.zip` environ 14 MB, FBX/OBJ/DAE/GLTF. Risque: archive non streamable sans passer par Itch; verifier avant import, et adapter les routes tile/hex a une route first-person continue. | `shortlist` village/chemins. |
| S6 | [KayKit - Medieval Hexagon Pack](https://kaylousberg.itch.io/kaykit-medieval-hexagon) | Kit village + props / `Village / campagne pavee` | CC0 | Apercus tres riches: moulins, eglise, marche, murs, villages colores sur hexagones. Tres bon reservoir d'objets, moins bon pour la route elle-meme. | GitHub tree inspecte: 221 `gltf`, 221 `bin`, 442 `fbx`, 263 `obj`, 263 `mtl`, 54 `png`, 5 `jpg`, guide PDF; atlas 1024 downsampleable 128. Risque: couleur des toits tres "jeu de plateau"; choisir pieces, pas tout le style. | `shortlist` props/signaux village. |
| S7 | [ambientCG - Paving Stones 141](https://ambientcg.com/view?id=pavingstones141) | Surface route / `Village / campagne pavee` | CC0 | Preview/materiau inspecte: vieux pave/cobblestone medieval, parfait pour ressentir route rugueuse/village. Visuel realiste a styliser via shader/couleur. | Archive `1K-JPG` inspectee: `Color`, `AmbientOcclusion`, `Displacement`, `NormalDX`, `NormalGL`, `Roughness`, plus `blend`, `mtlx`, `tres`, `usdc`. Downloads 1K-8K JPG/PNG. Risque: photorealisme trop fort sans stylisation. | `shortlist` surface pavee principale. |
| S8 | [ambientCG - Paving Stones 057](https://ambientcg.com/view?id=PavingStones057) | Surface route / `Village / campagne pavee` | CC0 | Preview/materiau inspecte: petits paves type Prague, plus fin et dense que S7, interessant pour ruelles/courtes sections. | Archive `1K-JPG` inspectee: `Color`, `AmbientOcclusion`, `Displacement`, `NormalDX`, `NormalGL`, `Roughness`, plus `blend`, `mtlx`, `tres`, `usdc`. Risque: motif petit et repetitif; a tester en echelle camera basse. | `shortlist` surface pavee alternative. |
| S9 | [Poly Haven - Forest Ground 03](https://polyhaven.com/a/forrest_ground_03) | Surface sol / `Foret claire` | CC0 | Preview inspectee: sol de foret sec avec aiguilles, feuilles, brindilles; bon support pour bas-cotes et chemin naturel. Realiste mais lisible. | API Poly Haven inspectee: maps `Diffuse`, `nor_dx`, `nor_gl`, `AO`, `Rough`, `Bump`, `Displacement`, `arm`, `spec`, plus `blend`, `gltf`, `mtlx`. Diffuse 1K ~0.8 MB, 2K ~3.5 MB. Risque: partir 1K/2K et styliser. | `shortlist` sol foret. |
| S10 | [Quaternius - Fantasy Props MegaKit](https://quaternius.com/packs/fantasypropsmegakit.html) | Props + signaux premium / commun | CC0 | Preview source inspectee: props medieval/fantasy, potions, livres, coffres, etals, outils. Bon reservoir pour accents premium et points d'interet, pas un hero asset unique. | Page source inspectee: 211 modeles, non anime, textures presentes, FBX/OBJ/glTF, 4 texture sets partages, 60-70% free; source/pro ajoutent Unity URP, collisions et variantes usees. Risque: verifier la frontiere free/pro avant usage. | `shortlist` accents premium. |
| S11 | [Quaternius - Farm Animal Pack](https://poly.pizza/bundle/Farm-Animal-Pack-1kUvRTPLzT) | Vie / `Village / campagne pavee` | CC0 | Previews inspectees: animaux low-poly simples; mouton/vache/cheval peuvent donner vie sans voler la scene. Pas un signal principal. | Page/SSR Poly Pizza inspectee: 7 modeles listes, `Llama`, `Pig`, `Pug`, `Sheep`, `Horse`, `Cow`, `Zebra`; boutons FBX/GLTF; animations run/idle/jump/walk annoncees; CC0. Risque: endpoint de download JS et animation import a verifier en Unity. | `shortlist` vie legere. |

## Scan large

30 candidats inspectes. Les finalistes ci-dessus sont notes `shortlist`.

| # | Candidat | Source | Famille | Inspection rapide | Verdict |
| --- | --- | --- | --- | --- | --- |
| 1 | Quaternius Stylized Nature MegaKit | https://quaternius.itch.io/stylized-nature-megakit | biome foret | Visuel premium fort; FBX/OBJ/glTF; CC0; pack gros mais pertinent. | shortlist S1 |
| 2 | Kenney Nature Kit | https://kenney.nl/assets/nature-kit | props foret | Tres propre, 330 fichiers CC0; moins premium seul. | shortlist S2 |
| 3 | Quaternius Ultimate Stylized Nature Pack | https://quaternius.com/packs/ultimatestylizednature.html | biome foret | Bon plus petit kit nature, textures/normal maps; probablement alternative a S1. | alternate |
| 4 | Quaternius Ultimate Nature Pack | https://quaternius.com/packs/ultimatenature.html | biome foret | 150 modeles, FBX/OBJ/Blend; plus ancien et moins stylise premium. | alternate |
| 5 | Quaternius Texture Fantasy Nature Pack | https://quaternius.com/packs/texturedfantasynature.html | fantasy foret | Couleurs tres saturees/candy; peut servir micro-signal, risque trop toy. | needs-review |
| 6 | OpenGameArt 3D Nature Pack | https://opengameart.org/content/3d-nature-pack | props foret | Unity package + OBJ/MTL; no textures, ancien rendu plus placeholder. | rejected |
| 7 | Sketchfab Low Poly Nature Asset Pack | https://sketchfab.com/3d-models/low-poly-nature-asset-pack-f3d8c126e9eb478599fc255be8dee091 | props foret | Visuel attrayant, CC-BY; format/poids et origine doivent etre verifies avant final. | needs-review |
| 8 | Unity Asset Store Low-Poly Simple Nature Pack | https://assetstore.unity.com/packages/3d/environments/landscapes/low-poly-simple-nature-pack-162153 | biome foret | Gratuit et leger, mais licence Standard Unity Asset Store EULA hors MYB-38. | needs-review |
| 9 | Quaternius Medieval Village MegaKit | https://quaternius.itch.io/medieval-village-megakit | biome village | Visuel village premium, 300+ modeles, FBX/OBJ/glTF, CC0. | shortlist S3 |
| 10 | Kenney Fantasy Town Kit | https://kenney.nl/assets/fantasy-town-kit | biome village | 160 fichiers CC0, simple et coherent; doit etre polish. | shortlist S4 |
| 11 | KayKit Medieval Builder Pack | https://kaylousberg.itch.io/kaykit-medieval-builder-pack | biome village | 200+ assets, routes/eau/batiments, FBX/OBJ/DAE/GLTF, CC0. | shortlist S5 |
| 12 | KayKit Medieval Hexagon Pack | https://kaylousberg.itch.io/kaykit-medieval-hexagon | biome village | Tres riche visuellement, atlas 1024, FBX/GLTF/OBJ; attention effet jeu de plateau. | shortlist S6 |
| 13 | Quaternius Medieval Village Pack | https://quaternius.com/packs/medievalvillage.html | biome village | Plus petit que MegaKit; utile si S3 trop lourd, mais moins complet. | alternate |
| 14 | OpenGameArt Medieval Town base | https://opengameart.org/content/medieval-town-base-3d-assets | biome village | Modularite utile, Unity package, no textures; rendu ancien/sans premium. | rejected |
| 15 | OpenGameArt Modular 3D Buildings | https://opengameart.org/content/modular-3d-buildings | buildings | Ville/generic, OBJ/MTL/SKP/Unity package; hors priorite village premium. | rejected |
| 16 | KayKit City Builder Bits | https://kaylousberg.itch.io/city-builder-bits | props village/route | 32+ modeles CC0, atlas 1024, OBJ/FBX/GLTF; utile mais urbain. | alternate |
| 17 | Kenney Retro Medieval Kit | https://kenney-assets.itch.io/retro-medieval-kit | village retro | OBJ/FBX/glTF, CC0; visuel PS1/low-res trop risque "cheap". | rejected |
| 18 | ambientCG Paving Stones 141 | https://ambientcg.com/view?id=pavingstones141 | surface pavee | Vieux cobblestone medieval; PBR 1K-8K; CC0. | shortlist S7 |
| 19 | ambientCG Paving Stones 057 | https://ambientcg.com/view?id=PavingStones057 | surface pavee | Petits paves type Prague; CC0; bon test ruelle. | shortlist S8 |
| 20 | ambientCG Paving Stones 070 | https://ambientcg.com/view?id=PavingStones070 | surface pavee | Old pavement, fichiers tres lourds jusqu'a 16K; moins prioritaire que S7/S8. | alternate |
| 21 | ambientCG Ground024 | https://ambientcg.com/view?id=Ground024 | sol foret | CC0; candidat sol naturel mais inspection visuelle moins concluante que Poly Haven. | alternate |
| 22 | ambientCG Ground071 | https://ambientcg.com/view?id=Ground071 | sol foret | CC0; autre sol naturel, a garder si S9 ne s'adapte pas. | alternate |
| 23 | Poly Haven Forest Ground 03 | https://polyhaven.com/a/forrest_ground_03 | sol foret | Apercu detaille foret seche; formats PBR complets; CC0. | shortlist S9 |
| 24 | Poly Haven Asphalt 07 | https://polyhaven.com/a/asphalt_07 | route asphalt | Bon asphalt use, mais MYB-37 priorise foret/village pave; pas V1 prioritaire. | alternate |
| 25 | Quaternius Fantasy Props MegaKit | https://quaternius.com/packs/fantasypropsmegakit.html | props/signaux | Gros reservoir fantasy, shared textures, FBX/OBJ/glTF, CC0; verifier free/pro. | shortlist S10 |
| 26 | KayKit Dungeon Remastered | https://kaylousberg.com/game-assets/dungeon-remastered | props | Techniquement excellent CC0, mais langage dungeon hors biomes V1. | needs-review |
| 27 | KayKit Adventurers | https://kaylousberg.com/game-assets/characters-adventurers | personnages | Rigged/animated, CC0, atlas 1024; personnages trop scope/attention pour MYB-41 V1. | needs-review |
| 28 | Quaternius Farm Animal Pack | https://poly.pizza/bundle/Farm-Animal-Pack-1kUvRTPLzT | vie village | 7 animaux animes, FBX/GLTF, CC0; bon accent campagne. | shortlist S11 |
| 29 | Quaternius LowPoly Animated Animals | https://quaternius.itch.io/lowpoly-animated-animals | vie village | CC0, 6.6 MB, mais commentaires signalent possible doute sur certaines animations. | alternate |
| 30 | Sketchfab Low Poly Farm/Village Medieval Wood Pack | https://sketchfab.com/3d-models/low-poly-farm-villagemedieval-wood-asset-pack-e440b93204c24a8481fb23265673afe7 | props village | CC-BY, props bois utiles; besoin d'attribution et verification technique avant final. | needs-review |

## Refuses vite

- Kenney Retro Medieval Kit: techniquement sain, mais le rendu retro low-res est
  trop proche du risque "cheap" pour notre V1.
- OpenGameArt 3D Nature Pack / Medieval Town / Modular Buildings: utiles comme
  references historiques CC0, mais trop anciens et moins premium que les options
  Quaternius/KayKit/Kenney actuelles.
- Unity Asset Store gratuits: interessants techniquement, mais hors licences
  MYB-38 par defaut. Garder en annexe d'exception seulement.

## Risques ouverts

- Les finalistes Quaternius MegaKit ont des versions Standard/Pro/Source: avant
  import, verifier que les assets choisis appartiennent bien a la partie
  gratuite CC0, car le total du pack et le nombre d'assets gratuits different.
- Les textures photorealistes ambientCG/Poly Haven doivent etre stylisees dans
  Unity pour ne pas casser la DA `Stylise Premium`.
- Les packs hex/grid KayKit doivent fournir des props et silhouettes, pas dicter
  une route en tuiles visible facon plateau.
- Les animaux animes doivent rester accents de vie, pas devenir un systeme de
  gameplay ou une foule.

## Recommandation de POC suivant

Pour MYB-53/MYB-42, tester d'abord:

1. `Foret claire`: S1 + S2 + S9.
2. `Village / campagne pavee`: S3 + S5 + S7.
3. Accents premium: S10, puis S11 si l'import animation est peu couteux.

Limiter le POC a 6-8 assets reels importes, pas tout un pack.

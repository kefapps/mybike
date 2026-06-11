# GDD court - Echappee 3D

Date: 2026-06-11
Mode: Correct-course Unity canonical
Source: decision utilisateur apres video MYB-90, artefacts MYB-89/MYB-90, et
synthese selective de `/Users/jbodin/Downloads/echappee_3d_linear_package_v2.zip`
sans import de l'archive dans le repo.

## Intention

Echappee 3D est un jeu Unity WebGL en premiere personne ou le joueur vit une
balade scenique a velo sur rail. Le joueur ne dirige pas le velo et ne cherche
pas a gagner une course. La promesse est de transformer une session statique en
petite echappee visuelle fluide, d'abord avec une entree mock, puis plus tard
avec un velo connecte.

Type de jeu le plus proche: simulateur cycliste scenique custom, proche
simulation/racing par la camera, la vitesse et la route, mais sans adversaires,
classement, chrono competitif ni physique realiste.

Stack cible verrouillee: Unity WebGL, projet canonique `unity/Echapee4D`,
workflow IvanMurzak Unity-MCP. Le prototype React/Vite/Three.js reste en
parking historique.

## Piliers

1. Balade avant competition: evasion, emerveillement, rythme confortable et
   lisibilite suffisante pour le ride sur rail.
2. Mock first: le jeu doit etre jouable sans materiel externe.
3. Rail clair: route automatique, camera stable, progression lisible.
4. Scenique premium mais leger: primitives/prefabs Unity d'abord, assets
   complexes plus tard, dans le cadre MYB-37 `stylise premium avec socle
   low-poly de production`.
5. WebGL demonstrable: chaque increment Unity doit rester exportable et visible
   dans un navigateur local.

## Experience cible

Le joueur ouvre la demo Unity WebGL, lance un ride en mode mock, avance sur une
route courte, module son effort avec un controle UI ou clavier, traverse quelques
ambiances visuelles, met en pause si besoin, termine la session et voit un
resume simple.

Le ton UX doit rester sobre: pas de cockpit sportif dense, pas de simulation
avancee, pas de menu lourd. L'ecran principal doit montrer rapidement la route
et le mouvement.

## Vertical Slice Mock Unity

Strict minimum jouable:

- Une scene Unity WebGL locale qui demarre sur une action principale: lancer une
  balade mock.
- Une route visible avec corridor scenique lisible.
- Une camera premiere personne sur rail, avec hauteur et look-ahead controles.
- Une progression de route avancee par vitesse simulee.
- Une source mock unique: slider, bouton, clavier ou controle in-game simple.
- Un HUD minimal: vitesse simulee, distance, temps, etat source `mock`.
- Pause, reprise, fin de session.
- Un resume de session: duree, distance, vitesse moyenne.
- Deux ambiances visuelles minimum pour prouver le systeme de biomes et une
  transition simple.
- Fallbacks basiques: chargement WebGL, canvas nonblank, erreurs console
  documentees, scene lisible meme avec placeholders provisoires. Ces
  placeholders prouvent le flux et la performance; ils ne sont pas une cible DA
  finale.

Critere de validation du vertical slice:

- Une personne peut lancer la build Unity WebGL, rouler en mock pendant 1 a 3
  minutes, voir la route defiler, constater un changement d'ambiance, terminer
  la session et obtenir un resume coherent.
- Le velo connecte n'est jamais requis pour ce parcours.
- Les controles mock restent visibles et fiables pendant toute la session.
- La camera ne provoque pas de mouvement brutal sur la route exemple.
- Une capture courte prouve que le navigateur affiche une scene non vide.

## Scope MVP resserre

Le MVP ne cherche pas a couvrir tout le paquet Linear d'origine. Il doit prouver
une seule boucle jouable:

- Demarrer une balade mock dans Unity WebGL.
- Convertir une entree mock en vitesse visuelle lissee.
- Avancer automatiquement sur une route.
- Afficher un paysage simple mais identifiable, conforme au cadrage MYB-37.
- Afficher les donnees essentielles pendant et apres la session.
- Garder des validations locales repetables pour ride loop, route, HUD, WebGL
  build et capture navigateur.

La route MVP peut etre courte et prefabriquee. Les paysages peuvent etre
stylises, proceduralement simples et peu nombreux. La qualite recherchee est
"stylisee premium, fluide et agreable", pas "realiste" ni "cheap".

## Epics conceptuels

### Epic 1 - Boucle de ride mock Unity

Objectif: rendre une balade complete jouable sans velo connecte dans Unity
WebGL.

Contenu conceptuel: scene Unity, camera sur rail, progression de route, source
mock, HUD, etats de session, resume, validators et build WebGL.

### Epic 2 - Route scenique legere

Objectif: donner a la balade une identite visuelle suffisante sans pipeline
asset lourd.

Contenu conceptuel: route continue simple, biomes provisoires compatibles DA,
lumiere/fog, terrain/horizon minimal, budget performance WebGL, erreurs
utilisateur de base.

## Hors scope / Parking

- Velo connecte reel.
- Web Bluetooth.
- FTMS.
- Parser Indoor Bike Data.
- Smoothing de telemetrie reelle.
- Resistance controlee par pente.
- Multijoueur.
- Comptes utilisateur.
- Cloud sync.
- Classements, ghosts, scoring competitif avance.
- VR.
- Editeur de route.
- Import GPX.
- Meshy comme dependance MVP.
- Assets IA complexes ou pipeline de generation obligatoire.
- Historique riche de sessions.
- Backlog Linear complet et stories detaillees.
- Nouveaux travaux React/Vite/Three.js, sauf ticket explicite pour le prototype
  parke.

Ces sujets restent parked pour post-MVP ou spikes dedies. Aucun ne doit bloquer
la premiere architecture Unity.

## Decisions verrouillees

- Le moteur actif est Unity WebGL.
- Le projet Unity canonique est `unity/Echapee4D`.
- `unity/Echappee3D` est reference historique uniquement.
- `src/**` et la stack React/Vite/Three.js sont reference historique uniquement.
- Le MVP est jouable sans velo connecte.
- Le mode mock est un mode produit, pas seulement un outil de debug.
- La camera est en premiere personne et suit un rail.
- Le joueur ne dirige pas lateralement le velo.
- L'experience est non competitive.
- Les assets externes ou generes doivent avoir un fallback placeholder de
  validation. Le fallback ne remplace pas l'exigence de qualite `stylise
  premium` pour les assets retenus.
- BLE/FTMS est post-MVP, meme si l'architecture garde une extension future.
- Chaque increment Unity demo-facing doit rester validable par build/capture
  WebGL locale.

## Questions ouvertes

- Quelle duree cible exacte pour la route mock: 1-3 minutes pour demo rapide ou
  8-15 minutes comme intention PRD plus large ?
- Le vertical slice doit-il inclure seulement deux ambiances, ou imposer deja
  les quatre ambiances du PRD initial ?
- Quel niveau de controle mock est suffisant au depart: slider unique, clavier,
  presets, ou combinaison minimale ?
- Faut-il afficher la cadence dans le vertical slice, ou la garder optionnelle
  tant que la vitesse suffit a jouer ?
- Quel seuil performance initial viser pour Unity WebGL: FPS cible, temps de
  chargement, poids `.wasm/.data`, qualite low ?
- Le nom affiche en UI reste-t-il "Echappee 3D" ou faut-il valider une variante
  accentuee/brandee avant implementation ?

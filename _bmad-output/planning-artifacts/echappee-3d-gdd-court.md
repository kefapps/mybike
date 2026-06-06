# GDD court - Echappee 3D

Date: 2026-06-06
Mode: Express
Source: synthese selective de `/Users/jbodin/Downloads/echappee_3d_linear_package_v2.zip`, sans import de l'archive dans le repo.
Prochaine etape visee: `gds-game-architecture` en architecture mince.

## Intention

Echappee 3D est un jeu web 3D en premiere personne ou le joueur vit une balade scenique a velo sur rail. Le joueur ne dirige pas le velo et ne cherche pas a gagner une course. La promesse est de transformer une session statique en petite echappee visuelle fluide, d'abord avec une entree mock, puis plus tard avec un velo connecte.

Type de jeu le plus proche: simulateur cycliste scenique custom, proche simulation/racing par la camera, la vitesse et la route, mais sans adversaires, classement, chrono competitif ni physique realiste.

Stack cible verrouillee: Vite, React, TypeScript, Three.js.

## Piliers

1. Balade avant competition: rythme calme, lisibilite, confort camera.
2. Mock first: le jeu doit etre jouable sans materiel externe.
3. Rail clair: route automatique, camera stable, progression lisible.
4. Scenique mais leger: placeholders/procedural d'abord, assets complexes plus tard.
5. Architecture extensible: l'entree mock ne doit pas bloquer une future source BLE/FTMS, mais cette future source ne pilote pas le MVP.

## Experience cible

Le joueur ouvre l'app, lance un ride en mode mock, avance sur une route courte, module son effort avec clavier ou controle UI, traverse quelques ambiances visuelles, met en pause si besoin, termine la session et voit un resume simple.

Le ton UX doit rester sobre: pas de cockpit sportif, pas de simulation avancee, pas de menu dense. L'ecran principal doit montrer rapidement la route et le mouvement.

## Vertical Slice Mock

Strict minimum jouable:

- Une app web locale qui demarre sur un menu simple avec une action principale: lancer une balade mock.
- Une scene Three.js plein ecran avec une route visible.
- Une camera premiere personne sur rail, avec hauteur et look-ahead fixes ou config simples.
- Une progression de route de 0 a 1, avancee par vitesse simulee.
- Une source mock unique: slider ou clavier pour regler l'effort/vitesse.
- Un HUD minimal: vitesse simulee, distance, temps, etat source `mock`.
- Pause, reprise, fin de session.
- Un resume de session: duree, distance, vitesse moyenne.
- Deux ambiances visuelles minimum pour prouver le systeme de biomes et une transition simple: par exemple cote lumineuse puis foret douce.
- Fallbacks basiques: WebGL indisponible, route introuvable, asset placeholder manquant.

Critere de validation du vertical slice:

- Une personne peut lancer l'app, rouler en mock pendant 1 a 3 minutes, voir la route defiler, constater un changement d'ambiance, terminer la session et obtenir un resume coherent.
- Le velo connecte n'est jamais requis pour ce parcours.
- Les controles mock restent visibles et fiables pendant toute la session.
- La camera ne provoque pas de mouvement brutal sur la route exemple.

## Scope MVP resserre

Le MVP ne cherche pas a couvrir tout le paquet Linear d'origine. Il doit prouver une seule boucle jouable:

- Demarrer une balade mock.
- Convertir une entree mock en vitesse visuelle lissee.
- Avancer automatiquement sur une route.
- Afficher un paysage simple mais identifiable.
- Afficher les donnees essentielles pendant et apres la session.
- Garder des fonctions pures testables pour progression, mapping vitesse et stats.

La route MVP peut etre courte et prefabriquee. Les paysages peuvent etre stylises, proceduralement simples et peu nombreux. La qualite recherchee est "fluide et agreable", pas "realiste".

## Epics conceptuels

### Epic 1 - Boucle de ride mock

Objectif: rendre une balade complete jouable sans velo connecte.

Contenu conceptuel: canvas Three.js, render loop, camera sur rail, progression de route, source mock, HUD, etats de session, resume.

### Epic 2 - Route scenique legere

Objectif: donner a la balade une identite visuelle suffisante sans pipeline asset lourd.

Contenu conceptuel: route continue simple, biomes placeholders, lumiere/fog, terrain/horizon minimal, budget performance, erreurs utilisateur de base.

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

Ces sujets restent parked pour post-MVP ou spikes dedies. Aucun ne doit bloquer la premiere architecture.

## Decisions verrouillees

- Le MVP est jouable sans velo connecte.
- Le mode mock est un mode produit, pas seulement un outil de debug.
- La camera est en premiere personne et suit un rail.
- Le joueur ne dirige pas lateralement le velo.
- L'experience est non competitive.
- Le rendu 3D est web, avec Vite, React, TypeScript et Three.js.
- Les calculs critiques doivent rester testables hors Three.js: vitesse, progression, stats, selection biome.
- Les assets externes ou generes doivent avoir un fallback placeholder.
- BLE/FTMS est post-MVP, meme si l'architecture garde une extension future.
- Le prochain document doit rester une architecture mince, orientee vertical slice, pas une plateforme complete.

## Questions ouvertes

- Quelle duree cible exacte pour la route mock: 1-3 minutes pour demo rapide ou 8-15 minutes comme intention PRD plus large ?
- Le vertical slice doit-il inclure seulement deux ambiances, ou imposer deja les quatre ambiances du PRD initial ?
- Quel niveau de controle mock est suffisant au depart: slider unique, clavier, presets, ou combinaison minimale slider + clavier ?
- Faut-il afficher la cadence dans le vertical slice, ou la garder optionnelle tant que la vitesse suffit a jouer ?
- Quel seuil performance initial viser sur laptop standard: FPS cible, qualite low, budget draw calls ?
- Le nom affiche en UI reste-t-il "Echappee 3D" ou faut-il valider une variante accentuee/brandee avant implementation ?

## Handoff architecture mince

L'architecture suivante doit prioriser le chemin critique:

`MockRideInputSource -> speed mapping/smoothing -> RouteProgress -> cameraOnRail -> SceneController -> Hud/Summary`

Elle doit decrire les modules necessaires au vertical slice, les contrats TypeScript minimum, les tests attendus et les points d'extension post-MVP, sans importer le backlog complet ni concevoir BLE/FTMS maintenant.

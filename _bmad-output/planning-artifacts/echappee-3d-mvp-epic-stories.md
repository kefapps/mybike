---
title: "Epic et stories MVP - Echappee 3D"
project: "mybike"
date: "2026-06-06"
status: "draft"
scope: "vertical slice mock jouable"
stepsCompleted:
  - "step-01-validate-prerequisites"
  - "reduced-mvp-epic-story-generation"
inputDocuments:
  - "_bmad-output/planning-artifacts/echappee-3d-gdd-court.md"
  - "_bmad-output/planning-artifacts/echappee-3d-architecture-mince.md"
---

# Epic et stories MVP - Echappee 3D

## Cadrage

Ce document decompose uniquement la vertical slice mock jouable d'Echappee 3D. Il ne reprend pas le backlog complet ni les tickets de l'archive d'origine.

Flux cible:

```text
MockRideInputSource -> speed mapping/smoothing -> RouteProgress -> cameraOnRail -> SceneController -> Hud/Summary
```

## Epic MVP 1 - Boucle de ride mock jouable

Objectif: livrer une application web locale Vite/React/TypeScript avec une balade 3D courte, jouable sans velo connecte, pilotable par une source mock, affichant une scene Three.js simple, une progression lisible, un HUD minimal, pause/reprise/fin et un resume coherent.

Scope de l'epic:

- App minimale Vite, React, TypeScript.
- Canvas Three.js plein ecran avec scene simple et route visible.
- Source unique `mock` via slider et/ou clavier.
- Mapping d'effort vers vitesse, smoothing, progression de route et stats.
- Route courte prefabriquee, `cameraOnRail`, deux ambiances visuelles simples.
- HUD minimal, controle start/pause/resume/finish et resume de session.
- Tests unitaires critiques sur la logique pure.
- Un happy path Playwright pour la boucle mock, integre en derniere story si raisonnable.

Hors scope de l'epic:

- BLE, Web Bluetooth, FTMS, velo reel ou parser Indoor Bike Data.
- Backend, compte utilisateur, cloud sync, historique local riche.
- Meshy, pipeline assets IA, assets externes obligatoires.
- Plusieurs routes, editeur de route, import GPX.
- Menus avances, audio, biomes complets, performance tooling avance.
- Backlog post-MVP ou reprise des tickets de l'archive.

## Story 1.1 - App shell MVP et etats de session

### Objectif

Mettre en place l'application minimale et les ecrans necessaires pour lancer, interrompre, reprendre, finir et revoir une balade mock.

### Scope

- Initialiser l'app Vite/React/TypeScript minimale.
- Creer les ecrans `StartScreen`, `RideScreen`, `SummaryScreen` et un fallback WebGL simple.
- Definir un etat de session clair: `idle`, `running`, `paused`, `finished`, `error`.
- Brancher les actions principales: start, pause, resume, finish.
- Garder l'etat de session en memoire uniquement.

### Hors scope

- Persistance de session.
- Menus avances, profils, reglages complets.
- Routage multi-page complexe.
- Integration BLE ou source autre que `mock`.

### Critères d'acceptation

- Depuis l'ecran de depart, le joueur peut lancer une balade mock.
- L'ecran ride peut passer en pause puis reprendre sans perdre l'etat courant.
- Le joueur peut finir la session et atteindre un ecran de resume.
- Une erreur WebGL detectee affiche un fallback comprehensible au lieu d'un canvas vide.
- L'app ne depend d'aucun backend ni d'aucun materiel externe.

### Tests attendus

- Test unitaire du reducer ou equivalent pour les transitions `idle -> running -> paused -> running -> finished`.
- Test unitaire d'une transition d'erreur simple si WebGL est indisponible ou si la scene ne peut pas monter.

### Dépendances

- Aucune dependance applicative prealable.
- Sert de base aux stories 1.2, 1.4 et 1.5.

## Story 1.2 - Source mock, vitesse lissee, progression et stats

### Objectif

Rendre la boucle de ride calculable hors React et hors Three.js, depuis une entree mock jusqu'a une progression de route et des statistiques coherentes.

### Scope

- Implementer `MockRideInputSource` avec `effort01` borne entre `0` et `1`.
- Implementer `mapMockInputToSpeed`.
- Implementer `smoothSpeed` avec acceleration et deceleration progressives.
- Implementer `advanceRouteProgress` avec progression normalisee `0..1`, distance en metres et completion.
- Implementer `calculateRideStats` pour duree, distance et vitesse moyenne.
- Garder ces fonctions dans des modules purs sans import React ni Three.js.

### Hors scope

- Cadence, puissance, resistance, pente ou donnees de capteur reel.
- Smoothing de telemetrie BLE/FTMS.
- Calibration materiel.
- Historique detaille ou agregats multi-session.

### Critères d'acceptation

- Une valeur mock faible produit une vitesse faible et une valeur forte produit une vitesse proche du maximum configure.
- Les entrees hors bornes sont clampées sans produire de vitesse invalide.
- La vitesse affichee evolue progressivement, sans saut brutal quand l'effort change.
- La route avance selon la vitesse et le delta temps.
- La progression atteint exactement l'etat complete en fin de route.
- Les stats finales restent coherentes avec le temps ecoule et la distance parcourue.

### Tests attendus

- Tests unitaires `mapMockInputToSpeed`: clamp, min, max, valeurs intermediaires.
- Tests unitaires `smoothSpeed`: montee, descente, stabilite avec `dt` variable.
- Tests unitaires `advanceRouteProgress`: integration distance/vitesse, clamp fin de route, completion.
- Tests unitaires `calculateRideStats`: duree, distance, vitesse moyenne, cas distance nulle.

### Dépendances

- Story 1.1 pour l'integration dans les phases de session.
- Fournit les snapshots de ride aux stories 1.3, 1.4 et 1.5.

## Story 1.3 - Route prefabriquee, biomes et camera sur rail

### Objectif

Definir une route courte et stable qui permet une balade automatique en premiere personne avec camera deterministe et transition d'ambiance simple.

### Scope

- Creer `mockRouteDefinition` avec une longueur courte adaptee a une demo de 1 a 3 minutes.
- Implementer `sampleRouteAt` ou equivalent pour echantillonner la position sur la route.
- Implementer `cameraOnRail` avec hauteur, look-ahead et FOV configures.
- Implementer `selectBiomeAtProgress` avec au moins deux segments visuels.
- Garder les types de route en donnees simples (`Vec3`, metres, secondes, progression normalisee).

### Hors scope

- Plusieurs routes.
- Editeur de route, import GPX ou generation procedurale avancee.
- Steering lateral, collisions gameplay ou physique velo realiste.
- Biomes complets ou assets externes obligatoires.

### Critères d'acceptation

- La route peut etre echantillonnee de maniere deterministe entre `0` et `1`.
- La camera produit toujours une position, un point de regard et un FOV valides.
- La camera reste stable sur la route exemple et n'introduit pas de valeurs invalides.
- La selection de biome change au moins une fois pendant la balade.
- Les calculs de route ne dependent pas de classes Three.js.

### Tests attendus

- Tests unitaires `cameraOnRail`: position/look-at deterministes, valeurs finies, look-ahead borne.
- Tests unitaires `selectBiomeAtProgress`: seuils de transition et bornes `0` / `1`.
- Tests unitaires de sampling route: debut, milieu, fin, clamp des progressions hors bornes.

### Dépendances

- Story 1.2 pour consommer `RouteProgress`.
- Fournit la route, la camera et le biome actif a la story 1.4.

## Story 1.4 - Scene Three.js MVP et rendu de la balade

### Objectif

Afficher une scene 3D simple mais jouable, avec route visible, camera sur rail, placeholders proceduraux et deux ambiances lisibles.

### Scope

- Creer `ThreeCanvasHost` pour monter, redimensionner et demonter le canvas.
- Creer `SceneController` avec `mount`, `resize`, `update`, `dispose`.
- Afficher une route visible et un horizon/terrain placeholder.
- Appliquer la camera calculee par `cameraOnRail`.
- Ajouter deux ambiances simples via lumiere, fog, couleur et quelques formes procedurales.
- Garantir un rendu non vide avec des assets placeholder, sans pipeline externe.

### Hors scope

- Chargement d'assets lourds.
- Meshy ou generation IA.
- Optimisation avancee, profiler, budget draw calls detaille.
- Effets visuels complexes, audio, meteo ou cycle jour/nuit.
- Camera libre ou controles 3D du joueur.

### Critères d'acceptation

- Le canvas Three.js s'affiche pendant la phase `running`.
- La route est visible et la camera avance selon les snapshots de ride.
- Le changement de biome modifie visiblement l'ambiance.
- Le canvas se redimensionne correctement avec la fenetre.
- Les ressources Three.js principales sont disposees au demontage.
- En absence d'asset externe, la scene reste jouable avec placeholders.

### Tests attendus

- Tests unitaires legers ou tests d'integration DOM pour le montage/demontage du host si l'environnement de test le permet.
- Tests manuels documentes pendant cette story: canvas non vide, resize, pause visuelle stable, progression de camera.
- Les tests purement mathematiques restent couverts par les stories 1.2 et 1.3.

### Dépendances

- Story 1.1 pour le montage dans `RideScreen`.
- Story 1.2 pour les snapshots de vitesse/progression.
- Story 1.3 pour la route, la camera et le biome actif.

## Story 1.5 - HUD, controles mock, resume et happy path

### Objectif

Finaliser la boucle jouable de bout en bout: controler l'effort mock, lire les donnees essentielles, mettre en pause, reprendre, finir et verifier le resume.

### Scope

- Ajouter `MockRideControls` avec slider et/ou raccourcis clavier simples.
- Afficher `RideHud`: vitesse simulee, distance, temps, source `mock`, etat running/paused.
- Ajouter `PauseOverlay` avec reprise et fin.
- Ajouter `RideSummary`: duree, distance, vitesse moyenne.
- Integrer un happy path Playwright si raisonnable dans le setup du projet.
- S'assurer que le parcours complet est jouable sans velo connecte.

### Hors scope

- HUD sportif avance, cadence, puissance, calories, graphes.
- Menu de configuration complet.
- Historique local riche.
- Suite Playwright exhaustive ou tests multi-navigateurs.
- Source BLE/FTMS ou bascule de source.

### Critères d'acceptation

- Le joueur peut modifier l'effort mock pendant la balade.
- Le HUD met a jour vitesse, distance et temps pendant la phase active.
- La source affichee est explicitement `mock`.
- La pause stoppe l'avancement visible et la reprise continue la session.
- Le joueur peut finir la session et voir un resume avec duree, distance et vitesse moyenne.
- Le happy path couvre au minimum: ouvrir l'app, lancer une balade, modifier l'entree mock, observer le HUD, pause, resume, finish, resume.

### Tests attendus

- Tests unitaires ou integration React pour les controles mock et l'affichage HUD si la structure du projet le permet.
- Tests unitaires restants du reducer de session si non couverts en story 1.1.
- Un test Playwright happy path mock, ou a defaut une tache explicite dans cette story si l'installation Playwright est repoussee.

### Dépendances

- Story 1.1 pour les ecrans et phases de session.
- Story 1.2 pour les donnees de ride et les stats.
- Story 1.4 pour verifier le parcours avec scene 3D montee.

## Couverture du scope obligatoire

- App Vite/React/TypeScript minimale: Story 1.1.
- Three.js canvas + scene simple: Story 1.4.
- `MockRideInputSource`: Story 1.2.
- Mapping/smoothing vitesse: Story 1.2.
- `RouteProgress` + `cameraOnRail`: Stories 1.2 et 1.3.
- HUD minimal: Story 1.5.
- Start / pause / resume / finish: Stories 1.1 et 1.5.
- Summary screen: Stories 1.1 et 1.5.
- Tests unitaires critiques: Stories 1.1, 1.2, 1.3 et 1.5.
- Happy path Playwright: Story 1.5.

## Ce qui a été volontairement exclu

- BLE, Web Bluetooth, FTMS et velo reel.
- Parser Indoor Bike Data, cadence, puissance, resistance controlee et calibration materiel.
- Meshy, pipeline assets IA, asset manager lourd ou dependance a des assets externes.
- Historique local riche, comptes utilisateur, backend, cloud sync.
- Menus avances, reglages detailles, plusieurs routes, editeur de route et import GPX.
- Biomes complets, audio, meteo, cycle jour/nuit et effets visuels avances.
- Performance tooling avance, suite Playwright exhaustive et backlog post-MVP.
- Reprise des 32 tickets de l'archive d'origine.

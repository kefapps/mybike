---
title: "Epic et stories MVP - Echappee 3D Unity"
project: "mybike"
date: "2026-06-11"
status: "draft"
scope: "vertical slice mock Unity macOS-first jouable"
stepsCompleted:
  - "correct-course-unity-canonical"
inputDocuments:
  - "_bmad-output/planning-artifacts/echappee-3d-gdd-court.md"
  - "_bmad-output/planning-artifacts/echappee-3d-architecture-mince.md"
  - "_bmad-output/planning-artifacts/sprint-change-proposal-unity-canonical-2026-06-11.md"
---

# Epic et stories MVP - Echappee 3D Unity

## Cadrage

Ce document decompose la vertical slice mock Unity macOS-first d'Echappee 3D.
Il ne reprend pas le backlog complet ni les tickets de l'archive d'origine.

Decision active du 2026-06-11:

- Unity reste la cible active.
- Unity macOS devient la cible runtime prioritaire via `MYB-94`.
- WebGL devient une cible secondaire de validation/demo locale, pas la plateforme
  produit principale.
- `unity/Echapee4D` devient le projet canonique.
- React/Vite/Three.js reste en parking historique.
- Les tickets ouverts doivent etre relus en Unity-first avant implementation.

Flux cible:

```text
MockRideInput -> SpeedModel -> RouteProgress -> CameraRail -> UnityScene -> HUD/Summary -> PlatformValidation
```

## Historique conserve

Les anciennes stories MVP web `MYB-1` a `MYB-10`, puis les tickets web
post-recovery `MYB-18` a `MYB-24` et `MYB-28`, restent des preuves terminees.
Elles ne sont pas supprimees et ne doivent pas etre reouvertes pour la roadmap
active.

Les anciens tickets Unity `MYB-11` a `MYB-17` restent une reference historique
utile, mais le projet associe `unity/Echappee3D` n'est plus la cible active.

## Epic MVP Unity - Boucle de ride mock macOS-first jouable

Objectif: livrer une scene Unity locale courte, jouable sans velo
connecte, pilotable par une source mock, affichant une route suffisamment
lisible dans une scene stylisee premium, un HUD minimal, pause/reprise/fin,
resume coherent et preuves de validation de plateforme.

Scope de l'epic:

- Projet canonique `unity/Echapee4D`.
- Scene Unity jouable avec route visible, cible macOS prioritaire.
- Source unique `mock` via UI et/ou clavier.
- Mapping d'effort vers vitesse, smoothing, progression de route et stats.
- Route courte prefabriquee, camera rail, deux ambiances visuelles simples.
- Direction artistique alignee MYB-37: `stylise premium avec socle low-poly de
  production`; les placeholders restent provisoires.
- HUD minimal, controle start/pause/resume/finish et resume de session.
- Validators Unity ou tests C# critiques.
- Build/capture macOS ou validation Editor quand la story touche la boucle
  jouable; build/capture WebGL seulement quand la story cible une preuve
  navigateur.

Hors scope de l'epic:

- BLE, Web Bluetooth, FTMS, velo reel ou parser Indoor Bike Data.
- Backend, compte utilisateur, cloud sync, historique local riche.
- Meshy, pipeline assets IA, assets externes obligatoires.
- Plusieurs routes, editeur de route, import GPX.
- Menus avances, audio, biomes complets, performance tooling avance.
- Nouveaux developpements React/Vite/Three.js.
- Backlog post-MVP ou reprise brute des tickets de l'archive.

## Story U1 - Baseline Unity canonique et hygiene repo

### Objectif

Stabiliser `unity/Echapee4D` comme projet actif et reproductible.

### Scope

- Verifier que seuls `Assets`, `Packages` et `ProjectSettings` sont candidates
  au tracking.
- Documenter le lancement Unity-MCP et le status attendu.
- Renommer/creer une scene canonique de vertical slice si necessaire.
- Ajouter ou confirmer un validator minimal de scene.
- Produire un rapport local de baseline.

### Critères d'acceptation

- `unity-mcp-cli status unity/Echapee4D --timeout 10000` est sain.
- Les dossiers `Library`, `Temp`, `Logs`, `UserSettings`, builds et captures ne
  sont pas trackes.
- La scene canonique s'ouvre et contient route, camera, ride loop/HUD ou leurs
  placeholders explicites de validation, sans les traiter comme qualite DA
  finale.
- Un rapport local liste commandes, version Unity, package Unity-MCP et caveats.

## Story U2 - Boucle mock Unity jouable

### Objectif

Faire tourner start -> ride -> pause -> resume -> finish -> summary dans Unity.

### Scope

- Entree mock bornee.
- Vitesse cible et smoothing.
- Progression de route.
- Etats de session.
- HUD minimal.
- Resume final.

### Critères d'acceptation

- La boucle fonctionne dans l'Editor et reste sans materiel externe.
- Pause stoppe l'avancement; resume reprend sans reset non voulu.
- Finish produit duree, distance et vitesse moyenne coherentes.
- Validator ou test C# couvre les transitions principales.

## Story U3 - Route, camera et ambiances

### Objectif

Rendre la balade confortable, suffisamment lisible et visuellement evocatrice en
premiere personne.

### Scope

- Route courte prefabriquee ou generee localement.
- Camera rail stable.
- Deux ambiances/biomes simples.
- Corridor scenique provisoire compatible avec la DA MYB-37.

### Critères d'acceptation

- Route visible et ancree.
- Camera sans mouvement brutal sur le parcours exemple.
- Au moins un changement d'ambiance visible.
- Capture ou screenshot valide la lisibilite.

## Story U4 - Platform build readiness

### Objectif

Garder la vertical slice Unity validable sur la cible prioritaire macOS, avec
WebGL comme preuve secondaire quand un ticket le demande.

### Scope

- Preflight macOS local: version Unity, OS, support de build et contraintes
  connues.
- Build ou lancement macOS quand le support local est disponible.
- Capture/screenshot/video de la cible locale.
- WebGL local, serveur statique et capture Playwright seulement pour tickets
  qui demandent une preuve navigateur.
- Rapport build/capture avec limites explicites.

### Critères d'acceptation

- La cible macOS prioritaire est documentee et validable localement, ou le
  manque de module/support est explicite avec action suivante.
- Console errors 0; warnings documentes.
- Screenshot, video ou contact sheet prouve route/HUD/mouvement pour la cible
  testee.
- Si WebGL est dans le scope du ticket: build reussit, HTTP 200, failed requests
  0, page errors 0, canvas/screenshot nonblank.

## Story U5 - Polish MVP et decision demo

### Objectif

Transformer la preuve Unity macOS en demo courte montrable.

### Scope

- Nettoyer warnings Unity prioritaires sur la cible active.
- Reduire poids ou documenter budget.
- Ameliorer lisibilite route/HUD/camera.
- README local Unity macOS-first.

### Critères d'acceptation

- Demo locale lancable avec checklist courte.
- Rapport final indique `ready`, `not-ready`, ou `blocked`.
- Les caveats restants sont des tickets explicites, pas des surprises.

## Couverture du scope obligatoire

- Projet Unity canonique: Story U1.
- Mode mock et ride loop: Story U2.
- Route/camera/biomes: Story U3.
- Build/capture plateforme: Story U4.
- Demo readiness: Story U5.

## Alignement Linear 2026-06-11

- `MYB-39`: ADR moteur final, Done. Unity devient la cible active.
- `MYB-89`: spike IvanMurzak Unity-MCP, Done.
- `MYB-90`: spike Unity WebGL readiness, Done; preuve technique secondaire.
- `MYB-91`: baseline Unity canonique, Done, mappe Story U1.
- `MYB-94`: decision plateforme, Done. Unity macOS devient la cible prioritaire;
  WebGL devient secondaire.
- Tickets actifs recalés vers Unity: `MYB-30`, `MYB-31`, `MYB-32`, `MYB-34`,
  `MYB-37`, `MYB-38`, `MYB-41`, `MYB-42`, `MYB-44`, `MYB-45`, `MYB-47`,
  `MYB-48`, `MYB-50`, `MYB-51`, `MYB-53`, `MYB-55`, `MYB-57`, `MYB-59`,
  `MYB-60`, `MYB-63`, `MYB-64`, `MYB-73`, `MYB-79`, `MYB-80`, `MYB-82`,
  `MYB-83`, `MYB-84`, `MYB-87`, `MYB-88`.
- Tickets materiel/FTMS a requalifier apres MYB-94: `MYB-40`, `MYB-58`,
  `MYB-61`.
- Les tickets web termines restent historiques et ne sont pas supprimes.

## Ce qui a ete volontairement exclu

- Support complet BLE/Web Bluetooth/FTMS et velo reel dans le MVP mock.
- Parser Indoor Bike Data, cadence, puissance, resistance controlee et
  calibration materiel.
- Meshy, pipeline assets IA, asset manager lourd ou dependance a des assets
  externes.
- Historique local riche, comptes utilisateur, backend, cloud sync.
- Plusieurs routes, editeur de route et import GPX.
- Suite Playwright/WebGL exhaustive et backlog post-MVP.
- Reprise des 32 tickets de l'archive d'origine.

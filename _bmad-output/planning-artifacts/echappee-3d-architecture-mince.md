---
title: "Architecture mince - Echappee 3D Unity"
project: "mybike"
date: "2026-06-11"
source: "_bmad-output/planning-artifacts/echappee-3d-gdd-court.md"
status: "draft"
scope: "vertical slice mock Unity WebGL"
---

# Architecture mince - Echappee 3D Unity

## Intention

Cette architecture cible uniquement la vertical slice mock Unity WebGL
d'Echappee 3D: une balade 3D en premiere personne sur rail, jouable sans velo
connecte, avec entree mock, progression lissee, scene Unity stylisee premium,
lisibilite suffisante, HUD minimal et resume de session.

Flux principal verrouille:

```text
MockRideInput -> SpeedModel -> RouteProgress -> CameraRail -> UnityScene -> HUD/Summary -> WebGLCapture
```

La structure doit garder quatre frontieres nettes:

- Unity porte l'experience active: scene, runtime, HUD, ride loop et build WebGL.
- La logique de ride reste testable par code C# pur ou validators Unity.
- Les assets et prefabs restent optionnels tant que des placeholders Unity
  lisibles existent pour valider le flux. Ces placeholders restent provisoires
  et ne remplacent pas la cible DA MYB-37.
- React/Vite/Three.js est reference historique, pas surface d'implementation
  active.

## Decisions techniques verrouillees

- Stack MVP active: Unity `6000.4.10f1`, WebGL, projet `unity/Echapee4D`.
- Workflow Unity prefere: IvanMurzak Unity-MCP / `unity-mcp-cli`.
- Scene de depart reference: `Assets/Scenes/MYB89UnityMcpProbe.unity`, a
  transformer ou remplacer par une scene canonique de vertical slice.
- Pas de backend, pas de compte utilisateur, pas de synchronisation cloud.
- Pas de BLE, Web Bluetooth, FTMS ou parser Indoor Bike Data dans le MVP.
- Le mode mock est une source produit MVP, pas seulement un outil de debug.
- La source d'entree MVP unique est `mock`, pilotable par UI et/ou clavier.
- Le joueur ne dirige pas lateralement le velo; la camera suit un rail.
- La vitesse visuelle est derivee d'une entree mock puis lissee avant d'avancer
  la route.
- La route est prefabriquee ou generee localement pour la demo; plusieurs routes
  restent post-MVP sauf ticket explicite.
- Chaque story demo-facing doit produire une preuve: validator, screenshot,
  video/contact sheet ou capture WebGL.

## Modules MVP Unity

### `Runtime/Ride`

Responsabilite: logique de ride testable et modele de progression.

Elements attendus:

- entree mock bornee;
- mapping effort -> vitesse;
- smoothing vitesse;
- progression route;
- stats de session;
- transitions start, pause, resume, finish.

Ce module ne doit pas dependre de prefabs visuels pour pouvoir etre valide par
tests ou validators.

### `Runtime/Route`

Responsabilite: definition de route, sampling, segments, biomes et camera.

Elements attendus:

- route courte de demo;
- points/elevation/segments lisibles;
- sampling deterministe;
- camera rail stable;
- selection biome/ambiance.

### `Runtime/Scene`

Responsabilite: rendu Unity lisible a partir de l'etat de ride.

Elements attendus:

- route visible;
- terrain/verges/horizon;
- props simples ou placeholders provisoires;
- lumiere/fog/couleurs par ambiance;
- cockpit ou repere velo minimal.

### `Runtime/UI`

Responsabilite: interactions mock et feedback joueur.

Elements attendus:

- start/pause/resume/finish;
- effort mock;
- vitesse, distance, temps;
- etat `mock`;
- resume final.

### `Editor/Validation`

Responsabilite: preuves repetables pour chaque increment.

Elements attendus:

- builders de scene;
- validators route/camera/HUD/scene;
- rapports dans `_bmad-output/unity-test-results/`;
- build WebGL local quand necessaire.

### `Scripts/Capture`

Responsabilite: validation navigateur WebGL hors Unity Editor.

Elements attendus:

- serveur local statique;
- capture Playwright;
- HTTP status, page errors, console errors/warnings;
- canvas/screenshot nonblank;
- video/contact sheet.

## Flux de donnees MVP

1. Le controle mock met a jour l'effort courant.
2. Le ride loop lit l'entree mock pendant `running`.
3. Le modele de vitesse calcule et lisse la vitesse.
4. La progression avance sur la route.
5. La camera rail calcule position/look-at/FOV.
6. La scene Unity applique route, camera, lumiere, fog et elements visuels
   provisoires.
7. Le HUD lit l'etat courant; le resume lit les stats finales.
8. Les validators et captures prouvent l'etat pour review.

## Hors scope technique

- Nouveaux travaux React/Vite/Three.js.
- Backend, API serveur, base de donnees, auth, compte utilisateur.
- BLE, Web Bluetooth, FTMS, Indoor Bike Data, resistance controlee.
- Persistance d'historique de sessions.
- Import GPX ou editeur de route.
- Pipeline Meshy obligatoire, generation d'assets IA obligatoire, asset manager
  lourd.
- Multijoueur, classements, ghosts, scoring competitif.
- VR, mobile natif, packaging desktop.
- Simulation physique velo realiste, collisions gameplay, steering lateral.

## Risques / mitigations

- WebGL lourd ou lent: garder les assets simples, mesurer `.wasm/.data`, limiter
  les effets URP non supportes et capturer le navigateur a chaque increment.
- Iteration Unity lente: garder les builders/validators Editor scriptables et
  documenter chaque commande.
- Drift entre deux projets Unity: `unity/Echapee4D` est canonique;
  `unity/Echappee3D` est reference seulement.
- Scene trop vide: livrer corridor, route, props simples et lumiere/fog avant
  tout asset externe, en gardant la cible `stylise premium` visible dans les
  criteres d'acceptation.
- Stats incoherentes: calculer distance, duree et moyenne depuis le modele ride,
  pas depuis le rendu.

## Points d'extension post-MVP

- Nouvelle source BLE/FTMS derriere une abstraction d'entree ou resistance.
- Routes multiples via donnees Unity/JSON importees dans le projet Unity.
- Biomes plus riches via prefabs, Addressables ou scenes additives si justifie.
- Assets externes optimises apres politique licence/performance.
- Wrapper web ou page marketing autour du build Unity seulement si un ticket le
  demande explicitement.

## Handoff pour backlog Unity

Contraintes pour l'etape suivante:

- Garder `unity/Echapee4D` comme projet unique actif.
- Ne pas recreer un large backlog.
- Rebaseliner les tickets ouverts pour Unity avant implementation.
- Prioriser: hygiene repo Unity, scene canonique, validator, build/capture WebGL.
- Ne pas supprimer `src/**` ni `unity/Echappee3D/**` dans ce correct-course.

---
title: "Architecture mince - Echappee 3D Unity"
project: "mybike"
date: "2026-06-11"
source: "_bmad-output/planning-artifacts/echappee-3d-gdd-court.md"
status: "draft"
scope: "vertical slice mock Unity macOS-first"
---

# Architecture mince - Echappee 3D Unity

## Intention

Cette architecture cible uniquement la vertical slice mock Unity macOS-first
d'Echappee 3D: une balade 3D en premiere personne sur rail, jouable sans velo
connecte, avec entree mock, progression lissee, scene Unity stylisee premium,
lisibilite suffisante, HUD minimal et resume de session. WebGL reste une cible
secondaire de validation/demo locale.

Flux principal verrouille:

```text
MockRideInput -> SpeedModel -> RouteProgress -> CameraRail -> UnityScene -> HUD/Summary -> PlatformValidation
```

La structure doit garder quatre frontieres nettes:

- Unity porte l'experience active: scene, runtime, HUD, ride loop et cible
  macOS.
- La logique de ride reste testable par code C# pur ou validators Unity.
- Les assets et prefabs restent optionnels tant que des placeholders Unity
  lisibles existent pour valider le flux. Ces placeholders restent provisoires
  et ne remplacent pas la cible DA MYB-37.
- React/Vite/Three.js est reference historique, pas surface d'implementation
  active.

## Decisions techniques verrouillees

- Stack MVP active: Unity `6000.4.10f1`, macOS first, projet
  `unity/Echapee4D`.
- WebGL reste disponible comme cible secondaire pour preuve navigateur,
  regression visuelle ou demo privee quand un ticket le demande.
- Workflow Unity prefere: IvanMurzak Unity-MCP / `unity-mcp-cli`.
- Scene de depart reference: `Assets/Scenes/MYB89UnityMcpProbe.unity`, a
  transformer ou remplacer par une scene canonique de vertical slice.
- Pas de backend, pas de compte utilisateur, pas de synchronisation cloud.
- Pas de BLE/Web Bluetooth/FTMS complet ou parser Indoor Bike Data dans le MVP
  mock. La faisabilite macOS/CoreBluetooth/FTMS doit etre prouvee avec un
  peripherique reel avant de devenir une dependance d'implementation.
- Android reste candidat secondaire; le module Android n'est pas present dans
  l'installation Unity locale du 2026-06-11.
- Le mode mock est une source produit MVP, pas seulement un outil de debug.
- La source d'entree MVP unique est `mock`, pilotable par UI et/ou clavier.
- Le joueur ne dirige pas lateralement le velo; la camera suit un rail.
- La vitesse visuelle est derivee d'une entree mock puis lissee avant d'avancer
  la route.
- La route est prefabriquee ou generee localement pour la demo; plusieurs routes
  restent post-MVP sauf ticket explicite.
- Chaque story demo-facing doit produire une preuve: validator, screenshot,
  video/contact sheet, build/capture macOS, ou capture WebGL quand applicable.

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
- build macOS ou WebGL local quand necessaire.

### `Scripts/Capture`

Responsabilite: validation navigateur WebGL hors Unity Editor quand un ticket
demande une preuve browser.

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
- BLE, Web Bluetooth, FTMS complet, Indoor Bike Data, resistance controlee.
- Persistance d'historique de sessions.
- Import GPX ou editeur de route.
- Pipeline Meshy obligatoire, generation d'assets IA obligatoire, asset manager
  lourd.
- Multijoueur, classements, ghosts, scoring competitif.
- VR, Android natif avant ticket dedie.
- Simulation physique velo realiste, collisions gameplay, steering lateral.

## Risques / mitigations

- macOS FTMS non prouve: garder le mock produit, documenter le preflight local
  et exiger un test peripherique reel avant tout ticket dependant du materiel.
- WebGL lourd ou lent si utilise comme preuve secondaire: garder les assets
  simples, mesurer `.wasm/.data`, limiter les effets URP non supportes et
  capturer le navigateur seulement quand le ticket le demande.
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

- Nouvelle source CoreBluetooth/FTMS macOS derriere une abstraction d'entree ou
  resistance.
- Routes multiples via donnees Unity/JSON importees dans le projet Unity.
- Biomes plus riches via prefabs, Addressables ou scenes additives si justifie.
- Assets externes optimises apres politique licence/performance.
- WebGL, Android ou wrapper web autour du build Unity seulement si un ticket le
  demande explicitement.

## Handoff pour backlog Unity

Contraintes pour l'etape suivante:

- Garder `unity/Echapee4D` comme projet unique actif.
- Ne pas recreer un large backlog.
- Rebaseliner les tickets ouverts pour Unity avant implementation.
- Prioriser: hygiene repo Unity, scene canonique, validator, cible macOS, puis
  WebGL seulement comme validation secondaire.
- Ne pas toucher `src/**` sauf ticket explicite et ne pas recreer
  `unity/Echappee3D/**`.

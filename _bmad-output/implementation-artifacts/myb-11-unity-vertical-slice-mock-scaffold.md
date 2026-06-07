---
story_id: "post-myb10-unity-1"
story_key: "myb-11-unity-vertical-slice-mock-scaffold"
linear_id: "MYB-11"
linear_url: "https://linear.app/kefjbo/issue/MYB-11/unity-vertical-slice-mock-scaffold"
title: "Unity vertical slice mock scaffold"
status: "done"
created: "2026-06-07"
baseline_commit: "1b77e6d060f209b482d8fd62a95e272b465bf3d2"
scope: "post-MYB-10 isolated Unity scaffold for mock vertical slice"
source_change_proposal: "_bmad-output/planning-artifacts/sprint-change-proposal-unity-migration-2026-06-07.md"
source_architecture: "_bmad-output/planning-artifacts/echappee-3d-architecture-unity-mince-2026-06-07.md"
source_playtest: "_bmad-output/implementation-artifacts/playtest-human-lite-post-myb10-resultats-2026-06-07.md"
source_reference_story: "_bmad-output/implementation-artifacts/myb-10-motion-density-pass.md"
depends_on:
  - "MYB-10"
---

# Story MYB-11: Unity vertical slice mock scaffold

Status: done

## Story

As a dev de la migration Echappee 3D,
I want un projet Unity isole et minimal qui reprend la boucle mock de reference,
so that on puisse verifier l'outillage Unity, le chemin repo et les premiers
contrats de vertical slice sans migrer tout le jeu.

## Contexte

Le changement de cap est acte: React/Vite/Three.js reste le prototype de
reference jusqu'a MYB-10, et Unity devient la cible de production. MYB-11 ne
doit pas refaire le jeu. Elle doit seulement creer un socle Unity isole pour
savoir comment Codex/BMAD travaille avec Unity dans ce repo et pour prouver la
boucle mock minimale.

Les retours humains post-MYB-10 signalent route qui disparait, flickering,
besoin de relief/vie plus tard, rendu encore POC, et brouillard/profondeur
apprecie. Pour MYB-11, ces retours se traduisent en priorite scaffold:
route stable visible, camera simple, input mock, HUD minimal et fog/depth. Les
pentes, oiseaux, humains, vehicules et assets finalises restent hors scope.

Sources:

- `_bmad-output/planning-artifacts/sprint-change-proposal-unity-migration-2026-06-07.md`
- `_bmad-output/planning-artifacts/echappee-3d-architecture-unity-mince-2026-06-07.md`
- `_bmad-output/implementation-artifacts/playtest-human-lite-post-myb10-resultats-2026-06-07.md`
- `_bmad-output/implementation-artifacts/myb-10-motion-density-pass.md`
- `_bmad-output/linear-sync.md`
- `_bmad-output/implementation-artifacts/sprint-status.yaml`

## Scope strict

Inclus:

1. Projet Unity isole dans le repo.
   - Chemin cible: `unity/Echappee3D/`.
   - Ajouter les ignores Git Unity necessaires avant de versionner des fichiers.
   - Preserver le prototype React/Three existant comme reference; ne pas le
     supprimer, deplacer ou reecrire.
2. Version Unity documentee.
   - Cible locale MYB-11: Unity `6000.4.10f1`, car c'est l'Editor installe.
   - Reference architecture initiale supersedee pour cette story:
     `6000.3.17f1`.
   - Preflight local constate le 2026-06-07: `6000.4.10f1` installe avec
     binaire Unity disponible; `6000.3.17f1` non installe; pas de CLI `unityhub`
     visible dans le `PATH`.
   - Ne pas revenir silencieusement vers `6000.3.17f1`; documenter tout ecart.
3. Scaffold projet minimal.
   - Structure projet Unity minimale sous `Assets/`, `Packages/` et
     `ProjectSettings/` si l'Editor peut creer/ouvrir le projet.
   - `README.md` Unity expliquant version cible, commandes, fallback et limites.
   - Scene principale candidate: `Assets/Scenes/RideMock.unity`.
4. Scripts C# minimaux et testables.
   - Modules ou dossiers minimum: `Echappee/Core`, `Echappee/Ride`,
     `Echappee/Route`, `Echappee/Rendering`, `Echappee/UI`,
     `Echappee/Bootstrap`.
   - Logique ride/route minimale hors `MonoBehaviour` quand possible.
   - State machine conceptuelle ou minimale: `Idle`, `Running`, `Paused`,
     `Finished`, `Error`.
5. Boucle mock scaffold.
   - Start / ride / pause / finish prevus par code minimal, README ou scene.
   - Source input unique `mock`, idealement slider si l'Editor permet la scene.
   - Progression simple et distance/temps affichables.
6. Route, camera, HUD et profondeur minimum.
   - Route stable simple et visible.
   - Camera route ou premiere personne simple, sans Cinemachine au depart.
   - HUD minimal: vitesse, distance, temps, source `mock`, phase.
   - Fog/depth active ou documentee pour conserver le point positif du playtest.

Exclus:

- Migration complete du jeu Unity.
- Meshy, asset externe, glTF, image externe ou pipeline asset lourd.
- Unity AI generation.
- Vehicules, humains, oiseaux ou monde vivant.
- BLE, Web Bluetooth, FTMS, velo reel ou resistance controlee.
- Backend, auth, compte, cloud sync, historique riche ou deploiement public.
- Suppression, remplacement ou hardening large du prototype React/Three.
- Plusieurs routes, editeur de route, import GPX.
- Cinemachine, Addressables, ECS/DOTS, DI container ou framework Unity lourd.
- Creation d'un epic ou d'un backlog Unity complet.

## Acceptance Criteria

1. Le repo contient un dossier Unity isole sous `unity/Echappee3D/` ou un ecart
   documente et justifie par l'architecture.
2. Les dossiers Unity non versionnables sont ignores: au minimum `Library/`,
   `Temp/`, `Obj/`, `Build/`, `Builds/`, `Logs/` et `UserSettings/` sous le
   projet Unity.
3. La version cible est documentee dans le README Unity: Unity `6000.4.10f1`,
   disponibilite locale constatee, ancienne reference `6000.3.17f1`
   supersedee, et fallback clair si la version exacte manque.
4. Le scaffold contient uniquement les fichiers necessaires au premier socle:
   structure projet, scene principale si possible, scripts C# minimaux,
   assemblies/tests si raisonnable, README.
5. Le prototype React/Three existant reste intact et reference comme source de
   comportement; aucun fichier `src/` n'est supprime ou migre dans cette story.
6. La boucle mock start / ride / pause / finish est representee par scripts
   minimaux ou par README + stubs C# explicites si l'Editor n'est pas disponible.
7. Une route stable simple et une camera mock sont prevues sans rechercher la
   qualite finale; aucune pente, vehicule, humain, oiseau ou asset externe n'est
   ajoute.
8. Le HUD minimal attendu est defini ou present: vitesse, distance, temps,
   source `mock`, phase.
9. Le brouillard/profondeur est configure si la scene Unity est creee, ou note
   comme exigence de la premiere ouverture Editor si le scaffold reste textuel.
10. Les modules Unity gardent la logique ride/route testable hors
    `MonoBehaviour` autant que possible.
11. Les validations Unity disponibles localement sont lancees. Si Unity Editor,
    la version cible ou Unity MCP manque, le blocage est documente avec la
    commande de validation attendue.
12. Aucun Meshy, BLE/FTMS, backend, deploiement public, asset externe, Unity AI,
    suppression React/Three ou gros backlog n'est introduit.

## Tasks / Subtasks

- [x] Preflight Unity local et decision scaffold. (AC: 3, 11)
  - [x] Verifier les versions sous `/Applications/Unity/Hub/Editor/`.
  - [x] Verifier si `6000.4.10f1` est disponible localement.
  - [x] Verifier si une CLI Unity/Unity Hub est disponible dans le `PATH`.
  - [x] Documenter l'ecart constate avec l'ancienne reference `6000.3.17f1`.
  - [x] Decider explicitement: creer avec l'Editor disponible `6000.4.10f1`.

- [x] Ajouter le conteneur Unity isole. (AC: 1, 2, 4, 5)
  - [x] Creer `unity/Echappee3D/`.
  - [x] Ajouter ou etendre `.gitignore` pour les dossiers Unity generes.
  - [x] Ajouter `unity/Echappee3D/README.md` avec version, commandes et fallback.
  - [x] Ne modifier aucun fichier applicatif React/Three sauf necessite
    documentaire explicite.

- [x] Creer la structure projet minimale. (AC: 4, 10)
  - [x] Creer `Assets/Echappee/Core/`.
  - [x] Creer `Assets/Echappee/Ride/`.
  - [x] Creer `Assets/Echappee/Route/`.
  - [x] Creer `Assets/Echappee/Rendering/`.
  - [x] Creer `Assets/Echappee/UI/`.
  - [x] Creer `Assets/Echappee/Bootstrap/`.
  - [x] Creer `Assets/Echappee/Tests/EditMode/` et `Assets/Echappee/Tests/PlayMode/`
    si compatible avec le niveau de scaffold.
  - [x] Creer `Assets/Scenes/`, `Assets/Data/Routes/`,
    `Assets/Data/Config/`, `Assets/Materials` et `Assets/Prefabs/`.

- [x] Ajouter les contrats C# minimaux. (AC: 6, 10)
  - [x] Ajouter `RidePhase` avec `Idle`, `Running`, `Paused`, `Finished`,
    `Error`.
  - [x] Ajouter les snapshots simples: input mock, progress route, camera,
    frame ride.
  - [x] Ajouter `RideMath` ou equivalent pour mapping mock -> vitesse et
    smoothing minimal.
  - [x] Ajouter `RouteMath` ou equivalent pour progression, sampling et camera
    rail simple.
  - [x] Ajouter un fallback route invalide: placeholder deterministe ou etat
    `Error`, jamais scene vide silencieuse.

- [x] Ajouter la scene ou les stubs de scene. (AC: 6, 7, 8, 9)
  - [x] Si l'Editor est disponible, creer `RideMock.unity`.
  - [x] Ajouter une route visible simple et un terrain/fond minimal.
  - [x] Ajouter une camera active avec look-ahead simple.
  - [x] Ajouter un Canvas/HUD minimal ou stubs presenters documentes.
  - [x] Ajouter un input mock visible ou stubbable.
  - [x] Configurer ou documenter le fog/depth.

- [x] Ajouter validations et preuves. (AC: 3, 11, 12)
  - [x] Lancer EditMode tests si le projet Unity peut les executer.
  - [x] Lancer PlayMode smoke si la scene existe et l'Editor est disponible, ou
    validation Editor de scene si le Test Runner CLI ne produit pas de XML.
  - [x] Si les validations ne peuvent pas tourner, noter la cause exacte dans la
    story et le README Unity.
  - [x] Verifier `git diff --check`.
  - [x] Verifier qu'aucune capture video ou dossier Unity genere n'est stage.
  - [x] Mettre a jour cette story, `sprint-status.yaml` et
    `_bmad-output/linear-sync.md` avec les preuves.

## Dev Notes

### Direction produit et migration

- React/Vite/Three.js reste le prototype de reference jusqu'a MYB-10.
- Unity devient la cible de production.
- Les artifacts MYB-2 a MYB-10 sont des specs de migration, pas un backlog a
  recreer tel quel.
- MYB-11 est un scaffold Unity, pas une implementation finale du jeu.

### Contrats a traduire depuis le prototype

- `MockRideInputSource` devient input mock C# + slider ou stub UI.
- `mapMockInputToSpeed` devient `RideMath.MapMockInputToSpeed`.
- `smoothSpeed` devient `RideMath.SmoothSpeed`.
- `advanceRouteProgress` devient `RouteMath.Advance`.
- `mockRouteDefinition` devient une route asset/donnee simple sous
  `Assets/Data/Routes/`.
- `sampleRouteAt` devient `RouteMath.SamplePosition`.
- `cameraOnRail` devient `RouteMath.CameraOnRail` + controller camera.
- `RideHud` et `MockRideControls` deviennent HUD presenter + input mock.

### Structure cible recommandee

```text
unity/Echappee3D/
  Assets/
    Echappee/
      Core/
      Ride/
      Route/
      Rendering/
      UI/
      Bootstrap/
      Tests/
        EditMode/
        PlayMode/
    Scenes/
      RideMock.unity
    Data/
      Routes/
      Config/
    Materials/
    Prefabs/
  Packages/
  ProjectSettings/
  README.md
```

### Fichiers probables

- `.gitignore`
- `unity/Echappee3D/README.md`
- `unity/Echappee3D/Packages/manifest.json`
- `unity/Echappee3D/Packages/packages-lock.json` si Unity le genere
- `unity/Echappee3D/ProjectSettings/*` seulement si genere proprement par Unity
- `unity/Echappee3D/Assets/Scenes/RideMock.unity`
- `unity/Echappee3D/Assets/Echappee/Core/*.cs`
- `unity/Echappee3D/Assets/Echappee/Ride/*.cs`
- `unity/Echappee3D/Assets/Echappee/Route/*.cs`
- `unity/Echappee3D/Assets/Echappee/Rendering/*.cs`
- `unity/Echappee3D/Assets/Echappee/UI/*.cs`
- `unity/Echappee3D/Assets/Echappee/Bootstrap/*.cs`
- `unity/Echappee3D/Assets/Echappee/Tests/EditMode/*.cs`
- `unity/Echappee3D/Assets/Echappee/Tests/PlayMode/*.cs`

Eviter:

- `src/*`, sauf documentation de reference explicitement necessaire.
- `_bmad-output/video-captures/*`.
- `unity/Echappee3D/Library/`, `Temp/`, `Obj/`, `Build/`, `Builds/`, `Logs/`,
  `UserSettings/`.
- Assets externes, Meshy, packages non necessaires, backend ou BLE/FTMS.

### Validation attendue

Si `6000.4.10f1` est installe:

```bash
/Applications/Unity/Hub/Editor/6000.4.10f1/Unity.app/Contents/MacOS/Unity \
  -batchmode \
  -projectPath unity/Echappee3D \
  -runTests \
  -testPlatform EditMode \
  -testResults _bmad-output/unity-test-results/myb-11-editmode.xml \
  -quit

/Applications/Unity/Hub/Editor/6000.4.10f1/Unity.app/Contents/MacOS/Unity \
  -batchmode \
  -projectPath unity/Echappee3D \
  -runTests \
  -testPlatform PlayMode \
  -testResults _bmad-output/unity-test-results/myb-11-playmode.xml \
  -quit
```

Preflight local observe pendant l'implementation:

- `/Applications/Unity/Hub/Editor/6000.3.17f1/Unity.app/...` non trouve.
- `/Applications/Unity/Hub/Editor/6000.4.10f1/Unity.app/Contents/MacOS/Unity`
  present et retenu comme cible MYB-11.
- `unityhub` non trouve dans le `PATH`.

Fallback si l'Editor cible n'est pas disponible:

- creer uniquement les fichiers/scaffold textuels versionnables;
- documenter dans `unity/Echappee3D/README.md` que la creation/validation Editor
  est bloquee par l'absence de `6000.4.10f1`;
- ne pas produire de faux resultat Unity;
- ne pas utiliser une autre version Unity comme nouvelle cible sans le noter
  explicitement dans la story et les preuves de validation.

### Tests attendus apres scaffold utilisable

- EditMode: mapping mock -> vitesse, smoothing, progression route, camera rail.
- EditMode: fallback route invalide -> placeholder deterministe ou `Error`.
- PlayMode smoke: `RideMock.unity` charge, route renderer visible, camera active,
  HUD present, input mock present, fog/depth actif.
- Verification Git: aucun dossier genere Unity non versionnable n'est stage.

## Definition of Done

- MYB-11 cree un socle Unity isole sous `unity/Echappee3D/`.
- La version cible Unity et la disponibilite locale sont documentees.
- Le scaffold contient la structure, les scripts C# minimaux, la scene/stubs et
  le README Unity necessaires au prochain travail.
- La boucle mock est representee sans migration complete du jeu.
- Le prototype React/Three reste intact comme reference.
- Les validations Unity disponibles sont executees, ou le blocage exact est
  documente avec les commandes attendues.
- Aucun Meshy, asset externe, BLE/FTMS, backend, deploiement public, Unity AI,
  vehicule/humain/oiseau, suppression React/Three ou gros backlog n'est ajoute.
- Linear MYB-11, `_bmad-output/implementation-artifacts/sprint-status.yaml` et
  `_bmad-output/linear-sync.md` restent synchronises.

## Dev Agent Record

### Agent Model Used

GPT-5 Codex

### Debug Log References

- Story/tracking creation only: npm validations intentionally not run.
- Local Unity preflight at implementation found Unity Hub app present,
  `6000.4.10f1` installed and selected as MYB-11 target, `6000.3.17f1` absent,
  and no `unityhub` or `Unity` CLI shim in `PATH`.
- Official Unity MCP verified after user configuration: `mcp__unity_mybike`
  exposed in this Codex session, `GetProjectRoot` resolved
  `/Users/jbodin/personnel/apps/mybike/unity/Echappee3D`, Editor state reported
  Unity `6000.4.10f1` reachable and idle, `RideMock.unity` was listed and loaded,
  scene hierarchy was read, and the Unity console returned 0 errors/warnings.
- Story/tracking setup committed before implementation:
  `cec0e38af3318c4f5c8bf5612c3b8a8548867e22`
  (`MYB-11 create Unity scaffold story`).
- Unity import/compile command run with `/Applications/Unity/Hub/Editor/6000.4.10f1/Unity.app/Contents/MacOS/Unity`.
  `MyBike.Echappee3D.Runtime.dll` and `MyBike.Echappee3D.EditModeTests.dll`
  were generated under ignored `Library/ScriptAssemblies/`.
- `-runTests -testPlatform EditMode` was attempted twice; Unity returned code
  0 but did not produce `_bmad-output/unity-test-results/myb-11-editmode.xml`.
  Logs show script compilation succeeded and licensing recovered after transient
  handshake/access-token messages.
- Scene generation command passed with code 0:
  `-executeMethod MyBike.Echappee3D.EditorTools.RideMockSceneBuilder.Build`.
- Editor validation command passed with code 0:
  `-executeMethod MyBike.Echappee3D.EditorTools.RideMockValidator.Validate`.
- Validation report:
  `_bmad-output/unity-test-results/myb-11-editor-validation.txt`.
- Final Git validation passed: `git diff --check`, anti-secret scan, no staged
  files, no `src/ride/*`, `src/render/*` or `src/app/*` changes, and no staged
  video capture or ignored Unity-generated folder.
- `npm run typecheck`, `npm run test` and `npm run build` intentionally not run:
  no web files were changed.
- Review correction: `unity/Echappee3D/README.md` now records the real Unity MCP
  preflight instead of saying no MCP was exposed.
- Review validation rerun on 2026-06-07: `RideMockValidator.Validate` passed in
  Unity `6000.4.10f1` batchmode with code 0; report stayed
  `_bmad-output/unity-test-results/myb-11-editor-validation.txt`.
- Review MCP note: after a Unity package refresh/restart, the local Unity MCP
  bridge reported `Connection revoked` because the Unity plan state no longer
  allowed MCP connections. The successful MCP proof above remains the review
  evidence for project root, idle Editor, active scene, hierarchy and 0
  console errors/warnings before the reload; batch validation covers the final
  local scaffold.
- Review triage: `com.unity.ai.assistant` is retained as the Unity MCP bridge
  package for validation tooling. No Unity asset generation tool was called and
  no Unity AI generated asset was added.

### Completion Notes List

- Cible MYB-11 corrigee vers Unity `6000.4.10f1`, l'Editor local disponible.
- Scaffold Unity isole cree sous `unity/Echappee3D/` avec `Packages/`,
  `ProjectSettings/`, `Assets/`, README, ignores Unity et scene `RideMock.unity`.
- Contrats C# minimaux ajoutes: phase ride, snapshots, input mock, mapping
  vitesse, smoothing, progression route, sampling, camera rail, fallback route,
  HUD, input slider, fog et orchestrateur de session.
- Scene `RideMock.unity` generee par Unity via builder Editor: camera principale,
  route placeholder visible, Canvas/HUD, slider mock, fog/depth et session ride.
- Validation Editor passee sur Unity `6000.4.10f1`: version, ride math, route
  math, fallback route, scene, camera, route, HUD, mock input et fog.
- MCP Unity officiel verifie apres configuration: projet `Echappee3D` joignable,
  scene `RideMock.unity` chargee, hierarchie lisible, console Unity sans erreur
  ni warning.
- README Unity corrige pendant la revue pour aligner la note MCP avec le
  preflight reel.
- Prototype React/Three conserve intact; aucun `src/ride/*`, `src/render/*` ou
  `src/app/*` modifie.
- Aucun Meshy, asset externe, Unity AI generation, BLE/FTMS, backend,
  vehicule, humain, oiseau ou backlog Unity ajoute.

### File List

- `.gitignore`
- `_bmad-output/implementation-artifacts/myb-11-unity-vertical-slice-mock-scaffold.md`
- `_bmad-output/implementation-artifacts/sprint-status.yaml`
- `_bmad-output/linear-sync.md`
- `_bmad-output/unity-test-results/myb-11-editor-validation.txt`
- `unity/Echappee3D/README.md`
- `unity/Echappee3D/Packages/manifest.json`
- `unity/Echappee3D/Packages/packages-lock.json`
- `unity/Echappee3D/ProjectSettings/AudioManager.asset`
- `unity/Echappee3D/ProjectSettings/ClusterInputManager.asset`
- `unity/Echappee3D/ProjectSettings/DynamicsManager.asset`
- `unity/Echappee3D/ProjectSettings/EditorBuildSettings.asset`
- `unity/Echappee3D/ProjectSettings/EditorSettings.asset`
- `unity/Echappee3D/ProjectSettings/GraphicsSettings.asset`
- `unity/Echappee3D/ProjectSettings/InputManager.asset`
- `unity/Echappee3D/ProjectSettings/MemorySettings.asset`
- `unity/Echappee3D/ProjectSettings/MultiplayerManager.asset`
- `unity/Echappee3D/ProjectSettings/NavMeshAreas.asset`
- `unity/Echappee3D/ProjectSettings/PackageManagerSettings.asset`
- `unity/Echappee3D/ProjectSettings/Packages/com.unity.ai.assistant/Settings.json`
- `unity/Echappee3D/ProjectSettings/Physics2DSettings.asset`
- `unity/Echappee3D/ProjectSettings/PresetManager.asset`
- `unity/Echappee3D/ProjectSettings/ProjectSettings.asset`
- `unity/Echappee3D/ProjectSettings/ProjectVersion.txt`
- `unity/Echappee3D/ProjectSettings/QualitySettings.asset`
- `unity/Echappee3D/ProjectSettings/SceneTemplateSettings.json`
- `unity/Echappee3D/ProjectSettings/TagManager.asset`
- `unity/Echappee3D/ProjectSettings/TimeManager.asset`
- `unity/Echappee3D/ProjectSettings/UnityConnectSettings.asset`
- `unity/Echappee3D/ProjectSettings/VFXManager.asset`
- `unity/Echappee3D/ProjectSettings/VersionControlSettings.asset`
- `unity/Echappee3D/Assets/Data/Config/ride-config.md`
- `unity/Echappee3D/Assets/Data/Routes/mock-route.md`
- `unity/Echappee3D/Assets/Echappee/Echappee.Runtime.asmdef`
- `unity/Echappee3D/Assets/Echappee/Bootstrap/RideSessionController.cs`
- `unity/Echappee3D/Assets/Echappee/Core/RidePhase.cs`
- `unity/Echappee3D/Assets/Echappee/Core/RideSnapshots.cs`
- `unity/Echappee3D/Assets/Echappee/Editor/RideMockSceneBuilder.cs`
- `unity/Echappee3D/Assets/Echappee/Editor/RideMockValidator.cs`
- `unity/Echappee3D/Assets/Echappee/Rendering/DepthFogController.cs`
- `unity/Echappee3D/Assets/Echappee/Rendering/RideCameraController.cs`
- `unity/Echappee3D/Assets/Echappee/Rendering/RouteRendererPlaceholder.cs`
- `unity/Echappee3D/Assets/Echappee/Ride/MockRideInput.cs`
- `unity/Echappee3D/Assets/Echappee/Ride/RideMath.cs`
- `unity/Echappee3D/Assets/Echappee/Route/RouteDefinition.cs`
- `unity/Echappee3D/Assets/Echappee/Route/RouteMath.cs`
- `unity/Echappee3D/Assets/Echappee/Tests/EditMode/Echappee.EditModeTests.asmdef`
- `unity/Echappee3D/Assets/Echappee/Tests/EditMode/RideMathTests.cs`
- `unity/Echappee3D/Assets/Echappee/Tests/EditMode/RouteMathTests.cs`
- `unity/Echappee3D/Assets/Echappee/Tests/PlayMode/RideMockSmokeTest.md`
- `unity/Echappee3D/Assets/Echappee/UI/HudController.cs`
- `unity/Echappee3D/Assets/Echappee/UI/MockRideInput.cs`
- `unity/Echappee3D/Assets/Materials/RoutePlaceholder.mat`
- `unity/Echappee3D/Assets/Scenes/RideMock.scene-plan.md`
- `unity/Echappee3D/Assets/Scenes/RideMock.unity`
- Unity-generated `.meta` files next to every new `Assets/` file and folder
  listed above.

### Change Log

- 2026-06-07: Implemented MYB-11 Unity `6000.4.10f1` scaffold, generated
  `RideMock.unity`, added validation evidence, and moved story to review.
- 2026-06-07: Corrected Unity MCP preflight after official MCP configuration and
  verified project/scene reachability through MCP.
- 2026-06-07: gds-code-review approved after README MCP correction, rerun Unity
  batch validation, Linear Done sync, and final commit preparation.

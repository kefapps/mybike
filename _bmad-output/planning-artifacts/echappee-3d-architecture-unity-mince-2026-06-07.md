---
title: "Architecture Unity mince - Migration progressive Echappee 3D"
project: "mybike / Echappee 3D"
date: "2026-06-07"
workflow: "gds-game-architecture"
status: "complete"
scope: "premier increment Unity vertical slice mock equivalente"
engine_target: "Unity 6.3 LTS"
editor_reference: "Unity 6000.3.17f1"
prototype_reference: "React + Vite + TypeScript + Three.js jusqu'a MYB-10"
production_target: "Unity"
source_change_proposal: "_bmad-output/planning-artifacts/sprint-change-proposal-unity-migration-2026-06-07.md"
source_gdd: "_bmad-output/planning-artifacts/echappee-3d-gdd-court.md"
source_architecture: "_bmad-output/planning-artifacts/echappee-3d-architecture-mince.md"
source_epic_stories: "_bmad-output/planning-artifacts/echappee-3d-mvp-epic-stories.md"
source_playtest: "_bmad-output/implementation-artifacts/playtest-human-lite-post-myb10-resultats-2026-06-07.md"
next_action: "create-unity-vertical-slice-mock-story"
---

# Architecture Unity mince - Migration progressive Echappee 3D

## Resume executif

React/Vite/Three.js devient le prototype de reference. Unity devient la cible
de production.

Cette architecture ne jette pas les artifacts existants. Elle traduit les
contrats de MYB-2 a MYB-10 en un premier increment Unity isole, verifiable et
volontairement petit:

```text
Mock input -> vitesse lissee -> progression route -> camera route -> rendu Unity -> HUD minimal
```

Le premier increment Unity doit prouver une vertical slice mock equivalente:
route visible stable, camera premiere personne ou camera route, slider/input
mock, progression simple, HUD minimal et brouillard/profondeur preserve. Il ne
doit pas encore traiter Meshy, BLE/FTMS, assets lourds, pentes, vehicules,
oiseaux, humains, backend, historique riche ou grand backlog.

## Sources traduites en specs de migration

| Source | Statut dans la migration |
| --- | --- |
| GDD court | Devient spec produit historique du prototype web et garde les piliers: balade calme, mock first, rail clair, scenique leger. |
| Architecture mince web | Devient spec fonctionnelle de frontieres: logique ride pure, route/camera testables, rendu depuis snapshot, HUD hors rendu. |
| Epic/stories MVP | Deviennent inventaire de capacites a repliquer dans Unity, pas un backlog a recreer tel quel. |
| MYB-10 + captures | Deviennent reference visuelle/prototype: motion, densite, brouillard, limites route/flicker. |
| Playtest humain post-MYB-10 | Devient evidence de priorite: route stable et flicker reduit avant pentes/vie/Meshy. |

## Decisions d'architecture

| Categorie | Decision | Rationale |
| --- | --- | --- |
| Moteur | Unity 6.3 LTS comme cible de production initiale. | LTS actuelle verifiee le 2026-06-07; support LTS jusqu'en decembre 2027 selon Unity. |
| Editeur de reference | Unity 6000.3.17f1 comme version de depart a installer si disponible localement. | Release officielle du 2026-06-04; a confirmer dans Unity Hub au moment de creer le projet. |
| Pipeline rendu | URP, scene 3D stylisee, fog/profondeur natifs Unity. | Suffisant pour scenic 3D leger; evite HDRP et les couts visuels prematurement lourds. |
| Projet | Projet Unity isole sous `unity/Echappee3D/`. | Separe clairement la cible Unity du prototype web existant et evite de casser Vite/React. |
| UI | HUD minimal uGUI Canvas + TextMeshPro si disponible via Unity Package Manager. | Plus rapide et plus stable pour vitesse/distance/temps/source mock qu'une UI runtime complexe. |
| Camera | Camera Unity pilotee par un snapshot `CameraRigSnapshot`; pas de Cinemachine au premier increment. | La camera du prototype est deja un contrat sur rail; eviter une dependance avant d'avoir reproduit la boucle. |
| Input | Slider mock comme input principal; clavier optionnel seulement si simple. | Le mode mock est un mode produit et la validation humaine repose sur un controle visible. |
| Donnees de route | Route prefabriquee dans un ScriptableObject ou asset JSON interne simple. | Permet route stable, inspectable et testable sans editeur de route. |
| Etat ride | State machine minimale en C# pur: `Idle`, `Running`, `Paused`, `Finished`, `Error`. | Reprend les contrats MYB-2/MYB-6 sans React. |
| Tests | EditMode tests pour logique pure; PlayMode smoke pour scene/HUD/camera. | Les bugs signales sont visuels, mais les frontieres doivent rester testables. |
| Assets | Placeholders proceduraux uniquement. | Pas d'asset lourd, pas de Meshy, pas de pipeline IA dans ce premier increment. |

## Structure de projet cible

Le repo garde le prototype web a la racine. Le projet Unity vit dans un dossier
isole:

```text
mybike/
├── src/                         # Prototype reference React/Vite/Three.js
├── _bmad-output/                # BMAD artifacts
└── unity/
    └── Echappee3D/
        ├── Assets/
        │   ├── Echappee/
        │   │   ├── Core/
        │   │   ├── Ride/
        │   │   ├── Route/
        │   │   ├── Rendering/
        │   │   ├── UI/
        │   │   ├── Bootstrap/
        │   │   └── Tests/
        │   │       ├── EditMode/
        │   │       └── PlayMode/
        │   ├── Scenes/
        │   │   ├── Boot.unity
        │   │   └── RideMock.unity
        │   ├── Data/
        │   │   ├── Routes/
        │   │   └── Config/
        │   ├── Materials/
        │   └── Prefabs/
        ├── Packages/
        ├── ProjectSettings/
        └── README.md
```

Regle Git obligatoire pour la story de creation projet:

```text
unity/Echappee3D/Library/
unity/Echappee3D/Temp/
unity/Echappee3D/Obj/
unity/Echappee3D/Build/
unity/Echappee3D/Builds/
unity/Echappee3D/Logs/
unity/Echappee3D/UserSettings/
```

Ces dossiers ne doivent pas etre versionnes.

## Modules Unity minimum

### `Echappee.Core`

Responsabilite: types purs partages, units, snapshots et resultats d'erreur.

Contraintes:

- pas de `MonoBehaviour`;
- pas de reference a des objets de scene;
- pas de BLE, backend, persistence ou asset loading;
- preferer des structs/classes simples pour pouvoir tester en EditMode.

Contrats:

```csharp
namespace MyBike.Echappee3D.Core;

public enum RidePhase
{
    Idle,
    Running,
    Paused,
    Finished,
    Error
}

public enum RideInputSourceKind
{
    Mock
}

public readonly struct Vec3
{
    public readonly float X;
    public readonly float Y;
    public readonly float Z;
}

public readonly struct RideInputSample
{
    public readonly RideInputSourceKind Source;
    public readonly float Effort01;
    public readonly double TimestampSeconds;
}

public readonly struct RouteProgressSnapshot
{
    public readonly float Progress01;
    public readonly float DistanceMeters;
    public readonly bool Completed;
}

public readonly struct CameraRigSnapshot
{
    public readonly Vec3 Position;
    public readonly Vec3 LookAt;
    public readonly float FovDegrees;
}

public readonly struct RideFrameSnapshot
{
    public readonly RidePhase Phase;
    public readonly RideInputSample Input;
    public readonly float SpeedMps;
    public readonly RouteProgressSnapshot Route;
    public readonly CameraRigSnapshot Camera;
    public readonly string BiomeId;
    public readonly double ElapsedSeconds;
}
```

### `Echappee.Ride`

Responsabilite: input mock, mapping vitesse, smoothing, progression ride, stats
et state machine.

Contrats:

```csharp
namespace MyBike.Echappee3D.Ride;

public interface IRideInputSource
{
    RideInputSourceKind Kind { get; }
    RideInputSample Read(double nowSeconds);
}

public sealed class MockRideInputSource : IRideInputSource
{
    public RideInputSourceKind Kind => RideInputSourceKind.Mock;
    public void SetEffort01(float effort01);
    public RideInputSample Read(double nowSeconds);
}

public readonly struct SpeedMappingConfig
{
    public readonly float MinSpeedMps;
    public readonly float MaxSpeedMps;
}

public readonly struct SpeedSmoothingConfig
{
    public readonly float AccelerationPerSecond;
    public readonly float DecelerationPerSecond;
}

public readonly struct RideSummary
{
    public readonly double ElapsedSeconds;
    public readonly float DistanceMeters;
    public readonly float AverageSpeedMps;
}

public static class RideMath
{
    public static float MapMockInputToSpeed(RideInputSample sample, SpeedMappingConfig config);
    public static float SmoothSpeed(float currentSpeedMps, float targetSpeedMps, float dtSeconds, SpeedSmoothingConfig config);
    public static RideSummary CalculateRideStats(double elapsedSeconds, float distanceMeters);
}
```

Regle de migration: les valeurs et comportements doivent rester comparables aux
fonctions TypeScript existantes, mais le code Unity n'importe pas le prototype
web.

### `Echappee.Route`

Responsabilite: route prefabriquee stable, sampling, camera sur rail et biome
actif.

Contrats:

```csharp
namespace MyBike.Echappee3D.Route;

public sealed class RouteDefinition
{
    public string Id;
    public float LengthMeters;
    public RoutePoint[] Points;
    public BiomeSegment[] Biomes;
}

public readonly struct RoutePoint
{
    public readonly Vec3 Position;
}

public readonly struct BiomeSegment
{
    public readonly string Id;
    public readonly float FromProgress01;
    public readonly float ToProgress01;
}

public readonly struct CameraRailConfig
{
    public readonly float HeightMeters;
    public readonly float LookAheadMeters;
    public readonly float FovDegrees;
}

public static class RouteMath
{
    public static RouteProgressSnapshot Advance(RouteProgressSnapshot previous, float speedMps, float dtSeconds, RouteDefinition route);
    public static Vec3 SamplePosition(RouteDefinition route, float progress01);
    public static CameraRigSnapshot CameraOnRail(RouteDefinition route, RouteProgressSnapshot progress, CameraRailConfig config);
    public static string SelectBiome(RouteDefinition route, float progress01);
}
```

Regles:

- `SamplePosition` et `CameraOnRail` doivent clampler `progress01`;
- aucune valeur `NaN` ou infinie ne doit sortir;
- une route invalide doit produire un fallback deterministe ou un etat `Error`
  exploitable, jamais une scene vide silencieuse.

### `Echappee.Rendering`

Responsabilite: scene Unity visible depuis les snapshots, route mesh, fog,
materiaux, placeholders et camera runtime.

Composants minimum:

- `RouteMeshBuilder`: construit une route continue depuis `RouteDefinition`.
- `RouteVisualController`: garde la route visible et applique materiaux/largeur.
- `BiomeVisualController`: change sky/fog/light/color selon `BiomeId`.
- `RideCameraController`: applique `CameraRigSnapshot` sur la camera Unity.
- `DepthFogController`: configure brouillard/profondeur et garde l'effet apprecie.

Regles:

- pas de calcul de vitesse/progression dans `Rendering`;
- pas de generation procedural lourde dans `Update`;
- pas de `Instantiate`/`Destroy` par frame;
- route mesh genere une fois au chargement de `RideMock.unity`;
- placeholders uniquement: terrain simple, horizon, quelques volumes statiques.

### `Echappee.UI`

Responsabilite: HUD minimal et controle mock.

Composants minimum:

- `MockInputSlider`: ecrit `effort01` dans `MockRideInputSource`.
- `RideHudPresenter`: affiche vitesse, distance, temps, source `mock`, phase.
- `PauseResumePresenter`: boutons pause, reprise, fin si inclus dans la story.
- `SummaryPresenter`: resume final si inclus dans la story.

Regles:

- UI lit un snapshot ou un modele de presentation, pas les objets de rendu;
- slider mock obligatoire dans le premier increment;
- clavier optionnel et non bloquant;
- aucune cadence/puissance/calories avant story dediee.

### `Echappee.Bootstrap`

Responsabilite: scene d'entree, cablage explicite et boucle ride.

Composants minimum:

- `RideBootstrap`: instancie/configure route, input mock, state machine.
- `RideLoopController`: orchestre `Update`, lit input, calcule snapshot, notifie HUD/rendu.
- `RideConfigAsset`: ScriptableObject pour vitesses, smoothing, camera, fog.

Regles:

- un seul caller principal de la boucle ride dans Unity, equivalent au role de
  `ThreeCanvasHost` dans le prototype web;
- `Update` orchestre, les calculs restent dans `RideMath` / `RouteMath`;
- `Time.deltaTime` est converti explicitement en secondes et borne si besoin.

## Flux runtime

```text
RideBootstrap
  -> charge RouteDefinition + RideConfigAsset
  -> cree MockRideInputSource
  -> initialise RideLoopController

Frame Update si Running:
  -> MockRideInputSource.Read(now)
  -> RideMath.MapMockInputToSpeed(...)
  -> RideMath.SmoothSpeed(...)
  -> RouteMath.Advance(...)
  -> RouteMath.CameraOnRail(...)
  -> RouteMath.SelectBiome(...)
  -> RideFrameSnapshot
  -> RideCameraController.Apply(snapshot.Camera)
  -> RouteVisualController.EnsureVisible(snapshot.Route)
  -> BiomeVisualController.Apply(snapshot.BiomeId)
  -> RideHudPresenter.Render(snapshot)
```

Pause:

- garde le dernier snapshot;
- stoppe l'avancement route;
- garde HUD et route visibles;
- ne detruit pas la scene.

Fin:

- calcule `RideSummary`;
- affiche resume minimal ou log de validation si le resume UI est repousse.

## Mapping prototype web vers Unity

| Prototype reference | Contrat Unity equivalent |
| --- | --- |
| `MockRideInputSource` | `MockRideInputSource` C# + `MockInputSlider`. |
| `mapMockInputToSpeed` | `RideMath.MapMockInputToSpeed`. |
| `smoothSpeed` | `RideMath.SmoothSpeed`. |
| `advanceRouteProgress` | `RouteMath.Advance`. |
| `mockRouteDefinition` | `RouteDefinition` asset sous `Assets/Data/Routes/`. |
| `sampleRouteAt` | `RouteMath.SamplePosition`. |
| `cameraOnRail` | `RouteMath.CameraOnRail` + `RideCameraController`. |
| `selectBiomeAtProgress` | `RouteMath.SelectBiome` + `BiomeVisualController`. |
| `SceneController.update(snapshot)` | Controllers Unity qui appliquent `RideFrameSnapshot`. |
| `RideHud` / `MockRideControls` | `RideHudPresenter` + `MockInputSlider`. |

## Hors scope verrouille

- Meshy, generation IA, asset pipeline lourd.
- BLE, Web Bluetooth, FTMS, velo reel, resistance controlee.
- Pentes, slopes gameplay, simulation physique velo.
- Vehicules, humains, oiseaux ou monde vivant, sauf placeholder immobile si
  strictement necessaire pour cadrer une scene.
- Backend, auth, compte, cloud sync, historique riche.
- Plusieurs routes, editeur de route, import GPX.
- Cinemachine, Addressables, ECS/DOTS, VContainer/Zenject, audio avance.
- Optimisation web/Three.js comme destination finale.
- Creation d'un gros backlog Unity.

## Risques et mitigations

| Risque | Impact | Mitigation premier increment |
| --- | --- | --- |
| Route encore instable ou invisible | Bloque la valeur de migration. | Route mesh statique genere au chargement, materiau opaque, PlayMode smoke test qui verifie presence renderer/camera/HUD. |
| Flickering remplace par un autre artefact Unity | Playtest ne valide pas la migration. | Eviter z-fighting: route legerement au-dessus terrain, materiaux simples, pas de surfaces coplanaires proches. |
| Projet Unity pollue le repo | Git bruite et risque de commits enormes. | Dossier isole + `.gitignore` Unity avant creation du projet. |
| Trop de dependances Unity des le depart | Premier increment devient trop large. | Pas de Cinemachine/Addressables/DI/ECS/Meshy; packages officiels seulement si necessaires. |
| Perte des contrats testables | Agents futurs melangent rendu et logique. | Assemblies `Core`, `Ride`, `Route` testees en EditMode; `Rendering` applique seulement des snapshots. |
| Camera inconfortable | Reproduit une faiblesse du prototype. | Look-ahead fixe, hauteur simple, FOV configure, validation manuelle 1-3 minutes. |
| Brouillard/profondeur perdu | Perte du point positif du playtest. | `DepthFogController` inclus dans l'acceptance du premier increment. |
| Story trop grosse | Retour du backlog implicite. | Une seule story, objectif vertical slice mock equivalente, exclusions explicites. |

## Validations attendues

### Validation doc/tracking de cette passe

- Aucun code applicatif modifie.
- Pas de `npm run typecheck`, `npm run test` ou `npm run build`: docs/tracking uniquement.
- Verification attendue: diff propre, pas de secret, artifact present sous
  `_bmad-output/planning-artifacts/`.

### Validation de la future story Unity

Commandes indicatives, a ajuster quand le projet Unity existe:

```bash
/Applications/Unity/Hub/Editor/6000.3.17f1/Unity.app/Contents/MacOS/Unity \
  -batchmode \
  -projectPath unity/Echappee3D \
  -runTests \
  -testPlatform EditMode \
  -testResults _bmad-output/unity-test-results/editmode.xml \
  -quit

/Applications/Unity/Hub/Editor/6000.3.17f1/Unity.app/Contents/MacOS/Unity \
  -batchmode \
  -projectPath unity/Echappee3D \
  -runTests \
  -testPlatform PlayMode \
  -testResults _bmad-output/unity-test-results/playmode.xml \
  -quit
```

Tests minimum:

- `MapMockInputToSpeed`: clamp, min/max, valeur intermediaire.
- `SmoothSpeed`: acceleration/deceleration sans overshoot.
- `RouteMath.Advance`: progression, distance, completion.
- `SamplePosition` / `CameraOnRail`: valeurs finies, look-ahead borne.
- `SelectBiome`: changement au moins une fois.
- PlayMode smoke: `RideMock.unity` charge, route renderer visible, camera active,
  HUD present, slider mock present, fog active.

Validation manuelle:

- lancer `RideMock.unity` en Play;
- slider mock visible et utilisable;
- route visible pendant 1 a 3 minutes;
- camera avance sans saut brutal;
- vitesse/distance/temps changent dans le HUD;
- brouillard/profondeur visible;
- pause garde la route visible si pause incluse;
- aucune dependance materiel externe.

## Handoff pour creer une seule petite story Unity

Workflow cible suivant: `gds-create-story`.

Creer une seule story, pas un epic et pas un backlog complet.

Titre candidat:

```text
Unity vertical slice mock equivalente
```

Objectif:

```text
Creer un projet Unity isole sous `unity/Echappee3D/` et prouver la boucle
mock minimale: route visible stable, camera route, slider mock, progression
simple, HUD minimal et brouillard/profondeur.
```

Acceptance criteria candidats:

1. Le repo contient un projet Unity isole et les dossiers Unity non versionnables
   sont ignores.
2. `RideMock.unity` affiche une route visible stable avec camera premiere
   personne ou camera route.
3. Un slider mock modifie l'effort et fait avancer une progression simple.
4. Le HUD affiche au minimum vitesse, distance, temps, source `mock`.
5. Le brouillard/profondeur est visible et preserve le point positif du playtest.
6. La logique ride/route minimale vit dans des modules testables hors
   `MonoBehaviour`.
7. EditMode tests couvrent mapping vitesse, smoothing, progression route et
   camera rail.
8. Aucun Meshy, BLE/FTMS, asset lourd, pentes, vie du monde, backend ou nouveau
   backlog n'est ajoute.

Prompt de handoff:

```text
gds-create-story

Creer une seule petite story Unity pour le premier increment de migration
Echappee 3D / mybike.

Source architecture:
- _bmad-output/planning-artifacts/echappee-3d-architecture-unity-mince-2026-06-07.md

Scope strict:
- projet Unity isole sous unity/Echappee3D/;
- route visible stable;
- camera premiere personne ou camera route;
- slider/input mock;
- progression simple;
- HUD minimal;
- brouillard/profondeur;
- tests Unity minimum;
- pas de Meshy, BLE/FTMS, assets lourds, pentes, vie, backend, gros backlog.
```

## References version moteur

- Unity release support officiel: Unity 6.3 LTS est la LTS actuelle et supportee
  jusqu'en decembre 2027.
- Unity release officielle verifiee: Unity 6000.3.17f1, release du 2026-06-04.

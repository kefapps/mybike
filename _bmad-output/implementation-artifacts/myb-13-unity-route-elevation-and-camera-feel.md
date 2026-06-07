# Story MYB-13: Unity route elevation and camera feel

## Metadata

- Linear issue: `MYB-13`
- Linear URL: https://linear.app/kefjbo/issue/MYB-13/unity-route-elevation-and-camera-feel
- Local status: `ready-for-dev`
- Linear status: `In Progress`
- Created: `2026-06-07`
- Baseline commit: `0ff3e4d74ef3a3731df70a446d7cfae257e88c6f`
- Depends on: `MYB-12`
- Unity project: `/Users/jbodin/personnel/apps/mybike/unity/Echappee3D`
- Unity scene: `Assets/Scenes/RideMock.unity`
- Source story: `_bmad-output/implementation-artifacts/myb-12-unity-playable-parity-mock-loop.md`
- Architecture source: `_bmad-output/planning-artifacts/echappee-3d-architecture-unity-mince-2026-06-07.md`
- Human feedback source: `_bmad-output/implementation-artifacts/playtest-human-lite-post-myb10-resultats-2026-06-07.md`
- Previous validation source: `_bmad-output/unity-test-results/myb-12-editor-validation.txt`

## Story

As a mock rider,
I want the Unity ride route to include simple climbs and descents with a camera that follows them comfortably,
so that the production-target vertical slice gains terrain feel while preserving the stable, foggy, readable ride loop from MYB-12.

## Context

MYB-12 delivered the Unity-only playable parity loop on a flat route: start -> ride -> pause -> resume -> finish -> summary/end state, wired mock input, HUD, visible route, stable camera and fog validation. MYB-13 deliberately changes only the route/camera feel layer by adding controlled elevation variations.

This story responds to post-MYB-10 human feedback: slopes are desirable, the old React/Three route disappearing or flickering must not return, and fog/depth should be preserved. Unity is now the production target. The React/Three prototype remains a reference and must not be deleted or changed.

## Acceptance Criteria

1. Unity-only scope is respected: all implementation changes are under `unity/Echappee3D/`, plus BMAD/tracking artifacts. No `src/ride/*`, `src/render/*`, `src/app/*` change is allowed.
2. `Assets/Scenes/RideMock.unity` uses a mock route with simple controlled elevation: at least one climb and one descent are present, route distances stay monotonic, and every point/sample is finite.
3. Elevation remains bounded for a vertical slice mock: no extreme grade, no terrain generation system, no procedural world pipeline, and no final terrain art.
4. The route remains visible and stable for the whole mock route: no intended z-fighting, flickering, clipping through the road visual, disappearance, or unreadable narrow/hidden route segment.
5. The ride camera follows the elevated route with a simple stable feel: camera position remains finite, above the route, looking forward along the route, without abrupt jumps or clipping through the route.
6. Camera behavior remains intentionally simple: no cinematic rig, no camera shake system, no advanced physics, and no new dependency.
7. Fog/depth from MYB-12 is preserved: the scene still has enabled fog with bounded density/distance settings and the long-distance feel remains part of validation.
8. The MYB-12 playable loop still works: start -> ride -> pause -> resume -> finish -> summary/end state remains wired through the existing mock input, controls and HUD.
9. `RideMockValidator` or an equivalent Unity editor validator is extended for MYB-13 and covers, at minimum, route elevation presence, route distance monotonicity, finite samples, bounded grade/elevation, route renderer visibility, stable camera samples, fog enabled, scene hierarchy and console health.
10. Final validation evidence is produced under `_bmad-output/unity-test-results/`, preferably as `myb-13-editor-validation.txt`.
11. Scope exclusions are enforced: no birds, humans, vehicles, Meshy, external asset, Unity AI generation, BLE/FTMS, backend, public deployment, full migration, broad backlog, or prototype web deletion.

## Implementation Tasks

- [ ] Preflight the Unity project before implementation:
  - confirm project root `/Users/jbodin/personnel/apps/mybike/unity/Echappee3D`;
  - confirm active scene `Assets/Scenes/RideMock.unity`;
  - confirm Editor idle, not compiling, not in Play Mode;
  - prefer MCP Unity if available, fallback to batchmode only if MCP is unavailable.
- [ ] Extend the route definition used by `RideMock.unity`:
  - reuse `Assets/Echappee/Route/RouteMath.cs` and existing `RouteDefinition` data structures;
  - replace the MYB-12 flat-only default with a small hand-authored elevation profile;
  - keep distance progression monotonic and samples finite;
  - avoid terrain mesh generation or asset imports.
- [ ] Improve camera-on-rail feel for slopes:
  - reuse the existing route sampling and camera rail code;
  - ensure look-ahead samples track the elevated route;
  - keep the camera above the route and visually stable;
  - avoid new camera systems or cinematic dependencies.
- [ ] Keep route rendering stable:
  - reuse or minimally extend `RouteRendererPlaceholder`;
  - keep road width/offset readable on climbs and descents;
  - avoid z-fighting-prone placement and avoid any route disappearance regression.
- [ ] Preserve scene wiring:
  - keep MYB-12 `RideSessionController`, mock input, control panel, HUD, camera, route and fog wiring intact;
  - rebuild `RideMock.unity` only through existing Unity/editor builder patterns if scene changes are required.
- [ ] Extend validation:
  - update `RideMockValidator` menu/action for MYB-13 or make the existing validator cover MYB-13 checks clearly;
  - validate route elevation, grade bounds, finite route/camera samples, route renderer, camera stability, fog and scene hierarchy;
  - write/update `_bmad-output/unity-test-results/myb-13-editor-validation.txt`.
- [ ] Run final safety checks:
  - Unity validation via MCP Unity if available, otherwise batchmode Unity;
  - Unity console 0 error / 0 warning;
  - `git diff --check`;
  - high-confidence secret scan;
  - verify no `src/ride/*`, `src/render/*`, `src/app/*` changes;
  - verify no video capture staged.

## Probable Files

- `unity/Echappee3D/Assets/Echappee/Route/RouteMath.cs`
- `unity/Echappee3D/Assets/Echappee/Route/RouteDefinition.cs`
- `unity/Echappee3D/Assets/Echappee/Rendering/RideCameraController.cs`
- `unity/Echappee3D/Assets/Echappee/Rendering/RouteRendererPlaceholder.cs`
- `unity/Echappee3D/Assets/Echappee/Rendering/DepthFogController.cs`
- `unity/Echappee3D/Assets/Echappee/Editor/RideMockSceneBuilder.cs`
- `unity/Echappee3D/Assets/Echappee/Editor/RideMockValidator.cs`
- `unity/Echappee3D/Assets/Scenes/RideMock.unity`
- `_bmad-output/unity-test-results/myb-13-editor-validation.txt`

## Validation Plan

- Unity MCP preferred:
  - confirm project root;
  - confirm active scene `Assets/Scenes/RideMock.unity`;
  - confirm Editor idle, not compiling, not in Play Mode;
  - read hierarchy;
  - execute or read MYB-13 `RideMockValidator`;
  - confirm Unity console 0 error / 0 warning.
- Batchmode Unity fallback is allowed only if MCP Unity is unavailable for MYB-13 validation.
- `git diff --check`.
- High-confidence secret scan against the diff/staged diff before commit.
- Verify no `src/ride/*`, `src/render/*`, `src/app/*` file changed.
- Do not run `npm run typecheck`, `npm run test`, or `npm run build` unless web source changes are detected.

## Guardrails

- Treat MYB-13 as a vertical-slice mock increment, not the start of final terrain.
- Keep elevation hand-authored or otherwise tightly controlled.
- Reuse existing Unity systems from MYB-11/MYB-12 before adding code.
- Preserve the React/Three prototype as a reference.
- Do not introduce Meshy, credit-cost tools, external assets, Unity AI generation, BLE/FTMS, backend, public deployment, birds, humans, vehicles or broad backlog work.

## Assumed Limits

- The route can be visually simple.
- The terrain can remain placeholder or absent.
- Camera feel only needs to be stable and readable, not cinematic.
- Route elevation does not need rich procedural generation.
- No life, traffic, riders, pedestrians or wildlife are expected in this story.

## Dev Agent Record

### Status

`ready-for-dev`

### Notes

- MYB-12 intentionally validated a flat route; MYB-13 replaces that constraint with bounded elevation validation.
- The route disappearing/flickering feedback came from the web prototype and must be treated as a regression risk for the Unity target.
- Fog/depth was appreciated in human feedback and is a required preservation check.

### Completion Evidence

- Pending implementation.

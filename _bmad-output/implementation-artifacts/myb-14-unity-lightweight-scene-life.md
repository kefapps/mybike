# Story MYB-14: Unity lightweight scene life

## Metadata

- Linear issue: `MYB-14`
- Linear URL: https://linear.app/kefjbo/issue/MYB-14/unity-lightweight-scene-life
- Local status: `ready-for-dev`
- Linear status: `In Progress`
- Linear sync comment: `831d4989-4c5c-4353-a196-d1a3b969b7a0`
- Created: `2026-06-07`
- Baseline commit: `6473a9d55975cdac699e21a71e14283ef9fb1d94`
- Depends on: `MYB-13`
- Unity project: `/Users/jbodin/personnel/apps/mybike/unity/Echappee3D`
- Unity scene: `Assets/Scenes/RideMock.unity`
- Source story: `_bmad-output/implementation-artifacts/myb-13-unity-route-elevation-and-camera-feel.md`
- Architecture source: `_bmad-output/planning-artifacts/echappee-3d-architecture-unity-mince-2026-06-07.md`
- Human feedback source: `_bmad-output/implementation-artifacts/playtest-human-lite-post-myb10-resultats-2026-06-07.md`
- Meshy cost decision: approved by user for this story only, 2 preview assets maximum with `meshy-5`, `target_formats: ["fbx"]`, 10 credits total maximum.

## Story

As a mock rider,
I want the Unity ride scene to include a first lightweight layer of life with birds and roadside human silhouettes,
so that the vertical slice feels less empty while preserving the stable route, slope camera feel, fog depth and mock ride loop.

## Context

MYB-13 delivered controlled route elevation and slope-aware camera feel in Unity. MYB-14 deliberately adds only a small scene-life layer: stylized birds in the sky and static human silhouettes off the route. It does not start traffic, pedestrian simulation, collision gameplay, final art direction or a broad asset pipeline.

The original Unity migration architecture excluded Meshy and life for the first scaffold. That exclusion remains the default rule, but MYB-14 has an explicit, user-approved exception for a bounded comparison: generate two Meshy FBX preview candidates, compare them against Unity primitive/procedural assets, and keep whichever option is more stable and readable for the POC. Meshy output is not mandatory for the final scene.

Unity remains the production target. The React/Three prototype remains a reference and must not be deleted or changed.

## Acceptance Criteria

1. Unity-only scope is respected: implementation changes stay under `unity/Echappee3D/`, plus BMAD/tracking artifacts. No `src/ride/*`, `src/render/*`, `src/app/*` change is allowed.
2. `Assets/Scenes/RideMock.unity` contains a first lightweight scene-life layer with exactly the two approved types for this story: stylized birds and static roadside human silhouettes.
3. Vehicle scope is excluded from MYB-14: no oncoming vehicle, overtaking vehicle, traffic system, vehicle AI, gameplay collision or lane interaction is added.
4. Unity-generated baseline assets are created through Unity MCP / Unity primitives / simple procedural meshes, with no asset store, downloaded pack, external stock model or Unity AI generation.
5. Meshy comparison is tightly bounded and cost-approved for this story only:
   - at most two `meshy_text_to_3d` preview tasks;
   - `ai_model: "meshy-5"`;
   - `target_formats: ["fbx"]`;
   - one stylized low-poly bird candidate;
   - one low-poly roadside human/spectator silhouette candidate;
   - 10 credits total maximum for this initial comparison;
   - no refine, remesh, retexture, animation or additional paid Meshy call without a new explicit user confirmation.
6. Meshy assets are optional for the shipped MYB-14 scene: the implementation documents the Unity-vs-Meshy comparison and can keep Unity primitives if they are clearer, lighter, more stable or more consistent with the current POC.
7. Scene-life placement stays outside the player lane and does not harm route readability:
   - human silhouettes are placed beside the route, not on the road surface;
   - birds stay above or away from the ride path, not in the near camera path;
   - no life element intentionally clips through the route, camera, HUD, or player trajectory.
8. Route, MYB-13 elevation, camera feel, fog/depth, HUD, mock input controls and the MYB-12 loop remain intact: start -> ride -> pause -> resume -> finish -> summary/end state.
9. Animation is optional and must remain simple if included: no complex flocking, pedestrian animation, navigation, AI, physics or per-frame instantiate/destroy loop.
10. `RideMockValidator` or an equivalent Unity editor validator is extended for MYB-14 and covers, at minimum, life root presence, bird count, human silhouette count, renderer/material presence, placement off route, small density bounds, route/camera/fog preservation, wiring, and console health.
11. Final Unity validation evidence is produced under `_bmad-output/unity-test-results/`, preferably as `myb-14-editor-validation.txt`, and reports whether Unity primitives or Meshy candidates were retained.

## Implementation Tasks

- [ ] Preflight the Unity project before implementation:
  - confirm project root `/Users/jbodin/personnel/apps/mybike/unity/Echappee3D`;
  - confirm active scene `Assets/Scenes/RideMock.unity`;
  - confirm Editor idle, not compiling, not in Play Mode;
  - use MCP Unity if available, with batchmode fallback only if MCP Unity is unavailable.
- [ ] Create a Unity primitive/procedural baseline for scene life:
  - add a small `SceneLife` root or equivalent under `RideMock.unity`;
  - add a bird group in the sky using simple primitives/procedural meshes;
  - add at least two static human silhouettes beside the route;
  - keep materials simple, matte and readable through fog;
  - avoid any asset store or downloaded asset.
- [ ] Generate and compare the Meshy candidates within the approved limit:
  - generate one stylized low-poly bird FBX preview candidate;
  - generate one low-poly roadside human/spectator silhouette FBX preview candidate;
  - use `meshy-5` and `target_formats: ["fbx"]`;
  - stop before any refine/remesh/retexture/additional paid call;
  - import or inspect the candidates only under `unity/Echappee3D/` if they are used for comparison;
  - document the Meshy task IDs, credit use and final keep/discard decision in this story.
- [ ] Integrate the selected life approach:
  - choose Unity primitives, Meshy candidates, or a mix based on readability and stability;
  - keep counts small and placement sparse;
  - keep all life elements outside the road lane and away from the near camera path;
  - preserve route, camera, fog, HUD and controls.
- [ ] Extend the scene builder:
  - add or update `Echappee/MYB-14/Rebuild RideMock Scene`;
  - preserve MYB-13 builder behavior and legacy menu aliases if needed;
  - keep `RideMock.scene-plan.md` aligned with the hierarchy.
- [ ] Extend validation:
  - add or update `Echappee/MYB-14/Validate RideMock Scene`;
  - validate life root, bird/human counts, renderer/material presence, off-route placement and density bounds;
  - continue validating route elevation, stable camera samples, fog, HUD/control wiring and MYB-12 session loop;
  - write `_bmad-output/unity-test-results/myb-14-editor-validation.txt`.
- [ ] Run final safety checks:
  - Unity validation via MCP Unity if available, otherwise batchmode Unity;
  - Unity console 0 error / 0 warning;
  - `git diff --check`;
  - high-confidence secret scan;
  - verify no `src/ride/*`, `src/render/*`, `src/app/*` changes;
  - verify no video capture staged;
  - do not run `npm run typecheck`, `npm run test`, or `npm run build` unless web source changes are detected.

## Probable Files

- `unity/Echappee3D/Assets/Echappee/Editor/RideMockSceneBuilder.cs`
- `unity/Echappee3D/Assets/Echappee/Editor/RideMockValidator.cs`
- `unity/Echappee3D/Assets/Echappee/Rendering/RouteRendererPlaceholder.cs`
- `unity/Echappee3D/Assets/Echappee/Rendering/RideCameraController.cs`
- `unity/Echappee3D/Assets/Echappee/Rendering/DepthFogController.cs`
- `unity/Echappee3D/Assets/Echappee/Rendering/LightweightSceneLife.cs` (probable new file)
- `unity/Echappee3D/Assets/Materials/`
- `unity/Echappee3D/Assets/Generated/Meshy/MYB14/` (only if Meshy candidates are imported/retained)
- `unity/Echappee3D/Assets/Scenes/RideMock.unity`
- `unity/Echappee3D/Assets/Scenes/RideMock.scene-plan.md`
- `_bmad-output/unity-test-results/myb-14-editor-validation.txt`

## Meshy Prompts To Use During Implementation

Use these prompts only within the approved 10-credit limit and only for preview FBX generation.

Bird candidate:

```text
Low-poly stylized bird for a calm scenic cycling game, simple readable silhouette, small game placeholder, minimal geometry, single neutral material, wings spread, no base, no scene, no realistic feather detail, no text.
```

Human silhouette candidate:

```text
Low-poly static roadside spectator silhouette for a calm scenic cycling game, simple human placeholder, standing pose, readable from distance, minimal geometry, single neutral material, no face detail, no base, no scene, no text.
```

## Validation Plan

- Unity MCP preferred:
  - confirm project root;
  - confirm active scene `Assets/Scenes/RideMock.unity`;
  - confirm Editor idle, not compiling, not in Play Mode;
  - read hierarchy;
  - execute or read MYB-14 `RideMockValidator`;
  - confirm Unity console 0 error / 0 warning.
- Batchmode Unity fallback is allowed only if MCP Unity is unavailable.
- `git diff --check`.
- High-confidence secret scan against the diff/staged diff before commit.
- Verify no `src/ride/*`, `src/render/*`, `src/app/*` file changed.
- Verify no video capture staged.
- Do not run `npm run typecheck`, `npm run test`, or `npm run build` unless web source changes are detected.

## Guardrails

- Treat MYB-14 as a vertical-slice mock increment, not a final art pass.
- Keep the life layer sparse, readable and easy to remove.
- Keep Meshy as a bounded comparison only: no refine, remesh, retexture, extra candidate, animation or texture pipeline without new user approval.
- Do not introduce vehicles, traffic AI, flocking AI, pedestrian AI, collisions, gameplay interactions, BLE/FTMS, backend, public deployment or broad backlog work.
- Do not use Unity AI generation, asset store packs, downloaded model packs, or unrelated external assets.
- Preserve the React/Three prototype as reference.

## Assumed Limits

- Birds can be stylized placeholders, not final wildlife.
- Human silhouettes can be static, faceless and non-interactive.
- Meshy output may be discarded if it does not beat the Unity primitive baseline.
- No final terrain, crowd system, traffic system, character rigging, walking animation or rich procedural life system is expected.

## Dev Agent Record

### Status

`ready-for-dev`

### Notes

- User validated scope option 1A/2A/3A on 2026-06-07:
  - low-cost Meshy comparison;
  - bird + human silhouette candidates;
  - Unity primitives may win.
- The approved Meshy budget is 10 credits total for this initial comparison.
- MYB-14 is the first story that intentionally reopens a tiny Meshy allowance after earlier stories excluded Meshy; that allowance is limited to comparison and does not create a general asset pipeline.

### Completion Evidence

- Story/tracking only; no Unity implementation performed in this pass.
- Linear issue created in `In Progress`: https://linear.app/kefjbo/issue/MYB-14/unity-lightweight-scene-life
- Linear sync comment: `831d4989-4c5c-4353-a196-d1a3b969b7a0`

### File List

- `_bmad-output/implementation-artifacts/myb-14-unity-lightweight-scene-life.md`
- `_bmad-output/implementation-artifacts/sprint-status.yaml`
- `_bmad-output/linear-sync.md`

### Change Log

- 2026-06-07: Created MYB-14 story and tracking with approved Unity-vs-Meshy lightweight scene-life comparison.

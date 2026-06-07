# Story MYB-16: Unity scene life readability polish

## Metadata

- Linear issue: `MYB-16`
- Linear URL: https://linear.app/kefjbo/issue/MYB-16/unity-scene-life-readability-polish
- Local status: `ready-for-dev`
- Linear status: `In Progress`
- Linear sync comment: `e06e80e3-cf29-48fd-bb6a-116377d92185`
- Created: `2026-06-07`
- Baseline commit: `849ae61dd4fed8215cbfb38f7c37076c60c28168`
- Depends on: `MYB-15`
- Unity project: `/Users/jbodin/personnel/apps/mybike/unity/Echappee3D`
- Unity scene: `Assets/Scenes/RideMock.unity`
- Source report: `_bmad-output/unity-test-results/myb-15-demo-readiness.txt`
- Architecture source: `_bmad-output/planning-artifacts/echappee-3d-architecture-unity-mince-2026-06-07.md`

## Story

As a developer preparing a private Unity demo,
I want the existing birds and roadside human silhouettes to be readable from the rider camera,
so that the scene feels less empty while staying within the lightweight POC slice.

## Context

MYB-12 established the playable Unity mock loop. MYB-13 added controlled route elevation and stable camera feel. MYB-14 added a lightweight life layer with 5 stylized birds and 3 static roadside human silhouettes. MYB-15 added the repeatable demo-readiness harness.

The MYB-15 report proves the scene is wired and stable, but also exposes the next problem:

- `Projected visibility in camera cone: 0/8`
- `Finite visibility samples: 8/8`
- Birds are present but outside the forward cone at representative checks.
- Human silhouettes are present but outside the forward cone at representative checks.

MYB-16 must improve readability using the existing life objects only. It is a bounded visual polish story, not a new life simulation system.

## Current Scene-Life Evidence

From `_bmad-output/unity-test-results/myb-15-demo-readiness.txt`:

- Birds: `5 required>=5`
- Humans: `3 required>=3`
- Total life elements: `8/12`
- Projected visibility in camera cone: `0/8`
- Finite visibility samples: `8/8`
- Example bird yaw values are roughly `-97`, `92.8`, `63.6`, `-77.7`, `98.4` degrees.
- Example human yaw values are roughly `-71.2`, `67.0`, `97.5` degrees.

The implementation target is therefore simple: bring a bounded subset of existing elements into the camera cone while preserving off-road placement and the established ride scene.

## Acceptance Criteria

1. Unity-only scope is respected: implementation changes stay under `unity/Echappee3D/`, plus BMAD/tracking/report artifacts. No `src/ride/*`, `src/render/*`, or `src/app/*` change is allowed.
2. The scene still contains exactly the MYB-14 life types: 5 stylized birds and 3 static roadside human silhouettes.
3. No vehicle, traffic system, AI, collision gameplay, flocking system, complex pedestrian animation, Meshy call, external asset, Unity AI generation, BLE/FTMS, backend, public deployment, or full migration is introduced.
4. Life readability improves measurably: validation reports at least `3/8` life elements inside the camera cone.
5. The visible subset includes at least 1 bird and at least 1 roadside human silhouette inside the camera cone.
6. `Finite visibility samples` remains `8/8`.
7. Birds remain stylized, simple, in the sky, and above `LightweightSceneLife.MinBirdHeightMeters`.
8. Human silhouettes remain static, roadside, outside the player lane, and at or beyond `LightweightSceneLife.MinHumanRouteClearanceMeters` from the route center.
9. Life elements do not clip through the road, block the rider view, or become large foreground occluders.
10. Polish is limited to placement, scale, orientation, and simple material/contrast adjustments of existing Unity primitive/procedural life elements.
11. Route, pentes MYB-13, camera rail behavior, fog/depth settings, HUD, controls, and MYB-12 start -> ride -> pause -> resume -> finish -> summary loop remain functionally unchanged.
12. `RideMockValidator` and/or `DemoReadinessValidator` is extended or complemented so the MYB-16 threshold is validated repeatably.
13. A validation report records the before/after readability result, including projected visibility count, bird visible count, human visible count, and finite sample count.
14. Unity console is confirmed 0 error / 0 warning after validation through MCP Unity if available, otherwise batchmode fallback plus log review.

## Implementation Tasks

- [ ] Preflight the local state:
  - confirm worktree status before edits;
  - confirm Unity project root `/Users/jbodin/personnel/apps/mybike/unity/Echappee3D`;
  - confirm active scene `Assets/Scenes/RideMock.unity`;
  - confirm Editor idle, not compiling, not in Play Mode.
- [ ] Inspect the existing scene-life implementation:
  - `LightweightSceneLife.cs` for count/clearance/height constants;
  - `RideMockSceneBuilder.CreateSceneLife()` for bird and human positions;
  - `SceneLifeVisibility.cs` for camera-cone metric semantics;
  - `DemoReadinessValidator.cs` for current report generation.
- [ ] Adjust only the existing life objects:
  - tune bird positions, scale, yaw and/or material contrast so at least 1 bird is in the camera cone;
  - tune human positions, scale, facing and/or material contrast so at least 1 roadside human is in the camera cone;
  - target at least 3 visible life elements total;
  - keep all objects off-route and non-blocking.
- [ ] Keep the builder and scene coherent:
  - update `RideMockSceneBuilder` if positions/materials are generated there;
  - update `Assets/Scenes/RideMock.unity` through Unity tooling if the scene instances need to match the builder;
  - avoid manual YAML edits unless absolutely necessary and review them carefully.
- [ ] Extend validation:
  - add a MYB-16 validator/menu or extend `DemoReadinessValidator` with a MYB-16 readiness section;
  - enforce `projected visible >= 3`;
  - enforce `visible birds >= 1`;
  - enforce `visible humans >= 1`;
  - enforce finite samples `8/8`;
  - preserve existing MYB-12/MYB-13/MYB-14/MYB-15 validation coverage.
- [ ] Generate or update a report:
  - preferred: `_bmad-output/unity-test-results/myb-16-scene-life-readability.txt`;
  - acceptable if cleaner: extend `_bmad-output/unity-test-results/myb-15-demo-readiness.txt` with MYB-16 threshold evidence;
  - include before/after note derived from MYB-15 baseline `0/8`.
- [ ] Final hygiene:
  - `git diff --check`;
  - high-confidence secret scan;
  - verify no `src/ride/*`, `src/render/*`, `src/app/*` changes;
  - verify no video capture staged;
  - do not run `npm run typecheck`, `npm run test`, or `npm run build` unless web source changes are detected.

## Developer Notes

Current scene-life generation is intentionally small and procedural:

- `LightweightSceneLife.RequiredBirdCount = 5`
- `LightweightSceneLife.RequiredHumanCount = 3`
- `LightweightSceneLife.MinHumanRouteClearanceMeters = 6f`
- `LightweightSceneLife.MinBirdHeightMeters = 12f`
- `LightweightSceneLife.MaxLifeElementCount = 12`

Current generated positions in `RideMockSceneBuilder.CreateSceneLife()` are the likely first edit point:

- birds:
  - `(-20, 18, 120)`
  - `(18, 22, 260)`
  - `(38, 19, 430)`
  - `(-34, 24, 620)`
  - `(20, 21, 790)`
- humans:
  - `(-14, 0, 180)`
  - `(52, 0, 420)`
  - `(24, 0, 760)`

MYB-15 computes visibility through `SceneLifeVisibility.Measure()`. It estimates route progress from `worldPosition.z`, samples `RouteMath.CameraOnRail()`, computes yaw/pitch relative to the camera forward vector, and marks an element in-cone only when it is finite, forward of camera, and inside horizontal/vertical FOV halves.

Do not change the camera just to make this pass. If the life is invisible because it is too lateral for the current camera, move or scale the life elements within the allowed off-route bounds.

## Probable Files

- `unity/Echappee3D/Assets/Echappee/Editor/RideMockSceneBuilder.cs`
- `unity/Echappee3D/Assets/Echappee/Editor/RideMockValidator.cs`
- `unity/Echappee3D/Assets/Echappee/Editor/DemoReadinessValidator.cs`
- `unity/Echappee3D/Assets/Echappee/Rendering/LightweightSceneLife.cs`
- `unity/Echappee3D/Assets/Echappee/Rendering/SceneLifeVisibility.cs`
- `unity/Echappee3D/Assets/Echappee/Tests/EditMode/SceneLifeVisibilityTests.cs`
- `unity/Echappee3D/Assets/Scenes/RideMock.unity`
- `unity/Echappee3D/Assets/Scenes/RideMock.scene-plan.md`
- `_bmad-output/unity-test-results/myb-16-scene-life-readability.txt`

## Validation Plan

- Unity MCP preferred:
  - confirm project root `/Users/jbodin/personnel/apps/mybike/unity/Echappee3D`;
  - confirm active scene `Assets/Scenes/RideMock.unity`;
  - confirm Editor idle, not compiling, not in Play Mode;
  - run the MYB-16 validator/menu or the extended MYB-15 demo readiness menu;
  - read the generated report;
  - confirm `projected visibility >= 3/8`;
  - confirm at least 1 bird and at least 1 human silhouette are in the camera cone;
  - confirm finite visibility samples `8/8`;
  - confirm Unity console 0 error / 0 warning.
- Batchmode Unity fallback is allowed only if MCP Unity is unavailable.
- `git diff --check`.
- High-confidence secret scan against the diff/staged diff before commit.
- Verify no `src/ride/*`, `src/render/*`, `src/app/*` file changed.
- Verify no video capture staged.
- Do not run `npm run typecheck`, `npm run test`, or `npm run build` unless web source changes are detected.

## Guardrails

- This is a readability polish, not a life-system rewrite.
- Keep the life layer deterministic and authored/procedural.
- Do not add more life types or increase density beyond MYB-14 bounds.
- Do not add Meshy or asset pipeline work.
- Do not tune route, camera or fog to hide the problem; improve life placement/readability instead.
- Preserve the React/Three prototype as reference and do not touch web source.

## Assumed Limits

- The goal is demo readability, not final art direction.
- Placeholders can remain primitive/procedural.
- The visibility metric is approximate but must remain deterministic, finite and useful for regression checks.
- A short text report is enough; no video capture is required for MYB-16 acceptance.

## Dev Agent Record

### Status

`ready-for-dev`

### Notes

- User validated the MYB-16 direction after MYB-15 reported `Projected visibility in camera cone: 0/8`.
- Story intentionally keeps the threshold modest: `>=3/8` total visible, including at least 1 bird and 1 human.
- The implementation should prefer moving/scaling/orienting existing elements over changing camera or route behavior.

### File List

- `_bmad-output/implementation-artifacts/myb-16-unity-scene-life-readability-polish.md`
- `_bmad-output/implementation-artifacts/sprint-status.yaml`
- `_bmad-output/linear-sync.md`

### Change Log

- 2026-06-07: Created MYB-16 story and tracking for Unity scene-life readability polish.

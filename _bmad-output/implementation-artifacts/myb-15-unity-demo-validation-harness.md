# Story MYB-15: Unity demo validation harness

## Metadata

- Linear issue: `MYB-15`
- Linear URL: https://linear.app/kefjbo/issue/MYB-15/unity-demo-validation-harness
- Local status: `done`
- Linear status: `Done`
- Linear sync comment: `5c6cbedb-1fd2-4953-a42e-f0bcd23b6d09`
- Linear implementation comment: `93fad4fa-df20-45e1-ac00-112101752c10`
- Linear review blocker comment: `332597d7-2506-4466-845d-e8ace0a14b1e`
- Linear review comment: `06c126c1-ce0c-4b7f-b62f-451ab35de746`
- Created: `2026-06-07`
- Baseline commit: `fe98bc828c09dda470f6ca28167b5b99cef82d3d`
- Depends on: `MYB-14`
- Unity project: `/Users/jbodin/personnel/apps/mybike/unity/Echappee3D`
- Unity scene: `Assets/Scenes/RideMock.unity`
- Source story: `_bmad-output/implementation-artifacts/myb-14-unity-lightweight-scene-life.md`
- Architecture source: `_bmad-output/planning-artifacts/echappee-3d-architecture-unity-mince-2026-06-07.md`
- Previous validation source: `_bmad-output/unity-test-results/myb-14-editor-validation.txt`
- Next intended story: `MYB-16 Unity private demo readability polish` (not created in MYB-15)

## Story

As a developer preparing a private Unity demo,
I want a repeatable demo-readiness validation harness for `RideMock.unity`,
so that route, camera, fog, HUD, mock loop and scene-life visibility can be evaluated with factual evidence before the next readability polish.

## Context

MYB-12 proved the playable Unity mock loop. MYB-13 added controlled route elevation and stable slope camera feel. MYB-14 added lightweight scene life with 5 stylized birds and 3 static roadside human silhouettes, while preserving route, camera, fog, HUD and controls.

The post-MYB-14 review found that the scene is structurally healthy, but the life elements are often outside the rider's forward camera cone. That is useful evidence for a future readability polish, but MYB-15 must not move or recolor those elements yet. MYB-15 exists to make that evidence repeatable and usable.

Unity remains the production target. The React/Three prototype remains a reference and must not be deleted or changed.

## Acceptance Criteria

1. Unity-only scope is respected: implementation changes stay under `unity/Echappee3D/`, plus BMAD/tracking artifacts. No `src/ride/*`, `src/render/*`, `src/app/*` change is allowed.
2. A Unity editor menu exists for demo-readiness validation, preferably `Echappee/MYB-15/Validate Demo Readiness`.
3. The MYB-15 harness generates `_bmad-output/unity-test-results/myb-15-demo-readiness.txt`.
4. The report includes project root, active scene path, editor idle state, Unity version and validation timestamp.
5. The report verifies route demo readiness: route renderer exists, has visible material, enough points, readable width, finite route samples, bounded elevation and stable route visibility.
6. The report verifies camera demo readiness: camera exists, sample positions and look targets are finite, camera stays above route samples, looks forward, and samples do not jump abruptly.
7. The report verifies fog/depth: fog is enabled, linear, start/end distances are bounded and end is greater than start.
8. The report verifies HUD, mock controls and the MYB-12 loop wiring: slider, Start/Pause/Resume/Finish buttons, HUD texts and start -> ride -> pause -> resume -> finish -> summary/end state are still covered.
9. The report verifies `SceneLife` presence and counts: birds and roadside human silhouettes exist, are active, have renderers and remain within density bounds.
10. The report includes a simple projected-visibility or camera-cone metric for `SceneLife`, such as per-element distance, yaw, pitch and whether the element is inside a configured camera cone at a representative route sample.
11. Low scene-life visibility should be reported as a factual recommendation for MYB-16, not silently ignored. It does not need to fail MYB-15 unless the metric cannot be produced or the scene-life objects are missing.
12. The report includes a short factual `MYB-16 recommendations` section derived from the metrics, without creating MYB-16.
13. The existing MYB-14 validator remains usable or is cleanly aliased; MYB-15 must not remove MYB-12/MYB-13/MYB-14 validation coverage.
14. Unity console is confirmed 0 error / 0 warning after validation through MCP Unity if available, otherwise batchmode fallback plus log review.
15. Scope exclusions are enforced: no visual polish, no repositioning birds/humans, no route/fog/camera behavior changes except obvious validator correctness fixes, no Meshy, no external asset, no Unity AI generation, no vehicle, traffic, AI, BLE/FTMS, backend, public deploy or full migration.

## Implementation Tasks

- [x] Preflight the Unity project:
  - confirm project root `/Users/jbodin/personnel/apps/mybike/unity/Echappee3D`;
  - confirm active scene `Assets/Scenes/RideMock.unity`;
  - confirm Editor idle, not compiling, not in Play Mode;
  - use MCP Unity if available, with batchmode fallback only if MCP Unity is unavailable.
- [x] Add the demo-readiness harness:
  - extend `RideMockValidator` or add a focused editor validator class;
  - expose `Echappee/MYB-15/Validate Demo Readiness`;
  - preserve existing MYB-14 validation entry points.
- [x] Generate the report:
  - write `_bmad-output/unity-test-results/myb-15-demo-readiness.txt`;
  - include pass/fail checks and factual metric lines;
  - include a compact `MYB-16 recommendations` section.
- [x] Implement route/camera/fog/HUD/session coverage by reusing existing validation logic:
  - avoid duplicating logic where helper methods can stay private and clear;
  - do not change runtime behavior unless a validator bug prevents correct measurement.
- [x] Add `SceneLife` projected visibility metrics:
  - measure active birds and humans against representative route/camera samples;
  - report distance, yaw, pitch and in-cone boolean;
  - keep thresholds simple and explicit in code.
- [x] Add or extend EditMode tests if helpful:
  - prefer small tests for visibility math if it is extracted from the editor class;
  - avoid broad PlayMode or capture automation in MYB-15 unless already trivial.
- [x] Update scene plan or local docs only if the validation menu/report changes the documented scene workflow.
- [x] Run final safety checks:
  - Unity validation via MCP Unity if available, otherwise batchmode Unity;
  - execute MYB-15 menu;
  - confirm report exists and contains recommendations;
  - confirm Unity console 0 error / 0 warning after validation;
  - `git diff --check`;
  - high-confidence secret scan;
  - verify no `src/ride/*`, `src/render/*`, `src/app/*` changes;
  - verify no video capture staged;
  - do not run `npm run typecheck`, `npm run test`, or `npm run build` unless web source changes are detected.

## Probable Files

- `unity/Echappee3D/Assets/Echappee/Editor/RideMockValidator.cs`
- `unity/Echappee3D/Assets/Echappee/Editor/DemoReadinessValidator.cs` (optional new file)
- `unity/Echappee3D/Assets/Echappee/Tests/EditMode/` (optional visibility math tests)
- `unity/Echappee3D/Assets/Scenes/RideMock.scene-plan.md` (only if menu/report workflow needs documentation)
- `_bmad-output/unity-test-results/myb-15-demo-readiness.txt`

## Validation Plan

- Unity MCP preferred:
  - confirm project root;
  - confirm active scene `Assets/Scenes/RideMock.unity`;
  - confirm Editor idle, not compiling, not in Play Mode;
  - execute `Echappee/MYB-15/Validate Demo Readiness`;
  - read or verify `_bmad-output/unity-test-results/myb-15-demo-readiness.txt`;
  - confirm Unity console 0 error / 0 warning after validation.
- Batchmode Unity fallback is allowed only if MCP Unity is unavailable.
- `git diff --check`.
- High-confidence secret scan against the diff/staged diff before commit.
- Verify no `src/ride/*`, `src/render/*`, `src/app/*` file changed.
- Verify no video capture staged.
- Do not run `npm run typecheck`, `npm run test`, or `npm run build` unless web source changes are detected.

## Guardrails

- Treat MYB-15 as a validation harness, not a player-facing polish story.
- Do not reposition, resize, recolor or animate the MYB-14 birds or human silhouettes.
- Do not change route, camera or fog feel unless a tiny correction is required to make validation truthful.
- Do not introduce Meshy, external assets, Unity AI generation, vehicles, traffic, AI, collisions, BLE/FTMS, backend, public deployment or broad backlog work.
- Do not create MYB-16 in this pass. Mention it only as the next intended readability polish.
- Preserve the React/Three prototype as reference.

## Assumed Limits

- The harness can be text-report based; no full video capture is required.
- Projected visibility can be approximate, as long as it is deterministic, finite and useful for comparing MYB-16 changes later.
- The report can include warnings/recommendations for low visibility without failing the whole validation.
- This is a POC demo-readiness report, not a final QA framework.

## Dev Agent Record

### Status

`done`

### Notes

- User chose direction D from the post-MYB-14 review: build a validation harness first, then do readability polish in MYB-16.
- Post-MYB-14 MCP audit observed active scene `Assets/Scenes/RideMock.unity`, clean editor state, route/fog present and `SceneLife` containing 5 birds and 3 human silhouettes.
- The rough projected-visibility audit found the life elements often outside the rider forward cone, with yaw values around 60 to 100 degrees in representative checks.
- That visibility issue is intended evidence for MYB-16, not scope for MYB-15.
- Implementation added a MYB-15 demo-readiness validator and a pure `SceneLifeVisibility` metric helper without repositioning birds/humans or changing route/camera/fog behavior.
- Unity console initially exposed missing `MyBike.Echappee3D.Core` imports in the new visibility helper/test; those scoped compile errors were fixed before final validation.
- Final report records `Projected visibility in camera cone: 0/8`, confirming MYB-16 should focus on readability polish.
- Review finding corrected: route/camera metrics are now more explicit in the report with `maxAbsElevation`, `maxAbsGrade` and `minForwardDot`.
- Review finding corrected: fragile internal Unity console reflection was removed from the report because MCP console read is the authoritative acceptance proof.
- Post-correction Unity validation is blocked: `unity_mcp` connection is revoked, and batchmode fallback aborts because the project is open in the Unity Editor.
- Final review resumed after MCP cleanup/restart; MYB-15 validation passed through Unity MCP.
- Review approved after 2 scoped corrections, with no remaining blockers.

### Completion Evidence

- Story/tracking commit completed first:
  - `18ace8588346a3c7d471bb35a3ca6a5459e039bd`
  - `MYB-15 create demo validation harness story`
- Linear issue moved to `Done`: https://linear.app/kefjbo/issue/MYB-15/unity-demo-validation-harness
- Linear sync comment: `5c6cbedb-1fd2-4953-a42e-f0bcd23b6d09`.
- Linear implementation comment: `93fad4fa-df20-45e1-ac00-112101752c10`.
- Linear review blocker comment: `332597d7-2506-4466-845d-e8ace0a14b1e`.
- Unity MCP validation:
  - Project root: `/Users/jbodin/personnel/apps/mybike/unity/Echappee3D`.
  - Active scene: `Assets/Scenes/RideMock.unity`.
  - Editor idle, not playing, not compiling and not updating.
  - Hierarchy includes `Main Camera`, `Route`, `Fog`, `SceneLife`, `Canvas`, `EventSystem` and `RideSession`.
  - `Echappee/MYB-15/Validate Demo Readiness` executed via MCP.
  - Unity console returned 0 errors / 0 warnings after validation.
- Report generated:
  - `_bmad-output/unity-test-results/myb-15-demo-readiness.txt`.
  - Includes project root, active scene, editor idle, console health, route/camera/fog/HUD/session loop checks, `SceneLife` counts and projected visibility.
  - `SceneLife` projected visibility: `0/8` in camera cone, finite samples `8/8`.
  - MYB-16 factual recommendations included.
- npm validation intentionally skipped because no web source changed.
- Review rerun blocker:
  - `unity_mcp` returns `Connection revoked. Go to Unity Editor > Project Settings > AI > Unity MCP to change approval.`
  - Batchmode fallback command with Unity `6000.4.10f1` aborts because another Unity instance has `/Users/jbodin/personnel/apps/mybike/unity/Echappee3D` open.
  - Required next action: re-approve Unity MCP in the Editor, or close the project so batchmode can run.
- Final review evidence:
  - Unity MCP root: `/Users/jbodin/personnel/apps/mybike/unity/Echappee3D`.
  - Active scene: `Assets/Scenes/RideMock.unity`.
  - Editor idle, not playing, not compiling, not updating; scene not dirty.
  - `Echappee/MYB-15/Validate Demo Readiness` executed via MCP after corrections.
  - Regenerated report includes `maxAbsElevation=15.00m`, `maxAbsGrade=0.060`, `minForwardDot=0.957`, `Projected visibility in camera cone: 0/8`, finite visibility samples `8/8`, and MYB-16 recommendations.
  - Unity console returned 0 errors / 0 warnings after validation.

### File List

- `_bmad-output/implementation-artifacts/myb-15-unity-demo-validation-harness.md`
- `_bmad-output/implementation-artifacts/sprint-status.yaml`
- `_bmad-output/linear-sync.md`
- `_bmad-output/unity-test-results/myb-15-demo-readiness.txt`
- `unity/Echappee3D/Assets/Echappee/Editor/DemoReadinessValidator.cs`
- `unity/Echappee3D/Assets/Echappee/Editor/DemoReadinessValidator.cs.meta`
- `unity/Echappee3D/Assets/Echappee/Editor/RideMockValidator.cs`
- `unity/Echappee3D/Assets/Echappee/Rendering/SceneLifeVisibility.cs`
- `unity/Echappee3D/Assets/Echappee/Rendering/SceneLifeVisibility.cs.meta`
- `unity/Echappee3D/Assets/Echappee/Tests/EditMode/SceneLifeVisibilityTests.cs`
- `unity/Echappee3D/Assets/Echappee/Tests/EditMode/SceneLifeVisibilityTests.cs.meta`

### Change Log

- 2026-06-07: Created MYB-15 story and tracking for Unity demo validation harness.
- 2026-06-07: Implemented Unity demo-readiness harness, generated MYB-15 report and moved story to review.
- 2026-06-07: Review corrected report metrics and console-health wording; final post-correction Unity validation blocked by revoked MCP / open project batchmode lock.
- 2026-06-07: Unity MCP validation resumed and MYB-15 review approved.

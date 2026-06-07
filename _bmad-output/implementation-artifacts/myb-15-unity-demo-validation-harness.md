# Story MYB-15: Unity demo validation harness

## Metadata

- Linear issue: `MYB-15`
- Linear URL: https://linear.app/kefjbo/issue/MYB-15/unity-demo-validation-harness
- Local status: `ready-for-dev`
- Linear status: `In Progress`
- Linear sync comment: `5c6cbedb-1fd2-4953-a42e-f0bcd23b6d09`
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

- [ ] Preflight the Unity project:
  - confirm project root `/Users/jbodin/personnel/apps/mybike/unity/Echappee3D`;
  - confirm active scene `Assets/Scenes/RideMock.unity`;
  - confirm Editor idle, not compiling, not in Play Mode;
  - use MCP Unity if available, with batchmode fallback only if MCP Unity is unavailable.
- [ ] Add the demo-readiness harness:
  - extend `RideMockValidator` or add a focused editor validator class;
  - expose `Echappee/MYB-15/Validate Demo Readiness`;
  - preserve existing MYB-14 validation entry points.
- [ ] Generate the report:
  - write `_bmad-output/unity-test-results/myb-15-demo-readiness.txt`;
  - include pass/fail checks and factual metric lines;
  - include a compact `MYB-16 recommendations` section.
- [ ] Implement route/camera/fog/HUD/session coverage by reusing existing validation logic:
  - avoid duplicating logic where helper methods can stay private and clear;
  - do not change runtime behavior unless a validator bug prevents correct measurement.
- [ ] Add `SceneLife` projected visibility metrics:
  - measure active birds and humans against representative route/camera samples;
  - report distance, yaw, pitch and in-cone boolean;
  - keep thresholds simple and explicit in code.
- [ ] Add or extend EditMode tests if helpful:
  - prefer small tests for visibility math if it is extracted from the editor class;
  - avoid broad PlayMode or capture automation in MYB-15 unless already trivial.
- [ ] Update scene plan or local docs only if the validation menu/report changes the documented scene workflow.
- [ ] Run final safety checks:
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

`ready-for-dev`

### Notes

- User chose direction D from the post-MYB-14 review: build a validation harness first, then do readability polish in MYB-16.
- Post-MYB-14 MCP audit observed active scene `Assets/Scenes/RideMock.unity`, clean editor state, route/fog present and `SceneLife` containing 5 birds and 3 human silhouettes.
- The rough projected-visibility audit found the life elements often outside the rider forward cone, with yaw values around 60 to 100 degrees in representative checks.
- That visibility issue is intended evidence for MYB-16, not scope for MYB-15.

### Completion Evidence

- Story/tracking only; no Unity implementation performed in this pass.
- Linear issue created in `In Progress`: https://linear.app/kefjbo/issue/MYB-15/unity-demo-validation-harness
- Linear sync comment: `5c6cbedb-1fd2-4953-a42e-f0bcd23b6d09`.
- npm validation intentionally skipped because this pass is story/tracking only and no web source changed.

### File List

- `_bmad-output/implementation-artifacts/myb-15-unity-demo-validation-harness.md`
- `_bmad-output/implementation-artifacts/sprint-status.yaml`
- `_bmad-output/linear-sync.md`

### Change Log

- 2026-06-07: Created MYB-15 story and tracking for Unity demo validation harness.

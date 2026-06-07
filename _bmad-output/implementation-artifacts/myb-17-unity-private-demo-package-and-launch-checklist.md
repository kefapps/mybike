# Story MYB-17: Unity private demo package and launch checklist

## Metadata

- Linear issue: `MYB-17`
- Linear URL: https://linear.app/kefjbo/issue/MYB-17/unity-private-demo-package-and-launch-checklist
- Local status: `ready-for-dev`
- Linear status: `In Progress`
- Linear sync comment: `0f2b2dd3-1af3-4eff-8d05-bc49887956aa`
- Created: `2026-06-07`
- Baseline commit: `4de2bc8d50ccf0583920cf129f72ee5ba3a51a11`
- Depends on: `MYB-16`
- Unity project: `/Users/jbodin/personnel/apps/mybike/unity/Echappee3D`
- Unity scene: `Assets/Scenes/RideMock.unity`
- Source report: `_bmad-output/unity-test-results/myb-16-scene-life-readability.txt`
- Architecture source: `_bmad-output/planning-artifacts/echappee-3d-architecture-unity-mince-2026-06-07.md`

## Story

As a developer preparing a private local demo,
I want a single Unity readiness menu, report and launch checklist,
so that the current vertical slice can be shown reliably without adding gameplay or visual scope.

## Context

MYB-12 established the playable Unity mock loop. MYB-13 added controlled elevation and stable camera feel. MYB-14 added lightweight scene life. MYB-15 added a repeatable demo-readiness harness. MYB-16 improved scene-life readability and was approved with:

- `projectedVisible=4/8`
- `birdsInCone=2`
- `humansInCone=2`
- `finiteSamples=8/8`
- Unity console `0 error / 0 warning`

The Unity vertical slice is now locally demoable in the Editor. MYB-17 should make that demo state explicit, repeatable and easy to launch. It is a packaging/checklist story, not a new feature story.

## Demo Readiness Target

MYB-17 should answer one practical question: "Can Julien launch and show the Unity slice privately from this repo today?"

The expected answer after implementation is a factual report with a clear private-demo verdict, not a public release certification. If a local macOS build is stable and low-risk, the story may validate build settings or produce a local build. If build packaging becomes noisy or brittle, the story should stop at an Editor-based private-demo checklist and document the limitation.

## Acceptance Criteria

1. Unity-only scope is respected: implementation changes stay under `unity/Echappee3D/`, plus BMAD/tracking/report artifacts. No `src/ride/*`, `src/render/*`, or `src/app/*` change is allowed.
2. A Unity editor menu exists for private demo validation, preferably `Echappee/MYB-17/Validate Private Demo`.
3. The MYB-17 validation generates `_bmad-output/unity-test-results/myb-17-private-demo-readiness.txt`.
4. The report includes project root, active scene path, Unity version, generated timestamp and Editor state.
5. The report confirms the active scene is `Assets/Scenes/RideMock.unity`.
6. The report confirms console health through the final Unity MCP console read, with expected final result `0 error / 0 warning`.
7. The report confirms route readiness: route visible/stable, route renderer present, enough points, finite route samples, bounded elevation and grade inherited from MYB-13.
8. The report confirms camera/fog readiness: camera samples finite and bounded, camera remains above route, forward look is stable, fog/depth settings are present and bounded.
9. The report confirms HUD, controls and the mock loop: effort slider, Start/Pause/Resume/Finish buttons, HUD texts and start -> ride -> pause -> resume -> finish -> summary path remain validated.
10. The report confirms SceneLife readiness inherited from MYB-16: 5 birds, 3 human silhouettes, projected visibility at least `3/8`, at least 1 bird in cone, at least 1 human in cone and finite samples `8/8`.
11. The report includes a short private-demo launch checklist with the exact scene/menu to use and the expected happy path to show.
12. The report includes a clear readiness verdict such as `ready-for-private-local-demo` or a factual blocker/recommendation if something fails.
13. If implementation validates Build Settings or produces a local macOS build, the report records the path/status and keeps build artifacts out of git unless explicitly intended and small metadata only.
14. If a local build is not attempted or is deferred, the report records the reason without failing the Editor-based private-demo readiness if all Editor acceptance criteria pass.
15. Existing MYB-15 and MYB-16 validation coverage remains usable; MYB-17 must not remove or weaken previous menus.
16. No new gameplay, visual polish, vehicle, traffic system, AI, collision gameplay, flocking, animation complexity, Meshy call, external asset, Unity AI generation, BLE/FTMS, backend, public deployment, or broad backlog work is introduced.

## Implementation Tasks

- [ ] Preflight the local state:
  - confirm clean or understood worktree before edits;
  - confirm Unity project root `/Users/jbodin/personnel/apps/mybike/unity/Echappee3D`;
  - confirm active scene `Assets/Scenes/RideMock.unity`;
  - confirm Editor idle, not compiling, not in Play Mode.
- [ ] Inspect existing validation harnesses:
  - `DemoReadinessValidator.cs` for MYB-15/MYB-16 report helpers;
  - `RideMockValidator.cs` for route/camera/fog/HUD/control/session coverage;
  - `SceneLifeVisibility.cs` for MYB-16 visibility metrics;
  - existing reports under `_bmad-output/unity-test-results/`.
- [ ] Add or extend private demo validation:
  - add menu `Echappee/MYB-17/Validate Private Demo`;
  - reuse MYB-15/MYB-16 checks instead of duplicating fragile logic;
  - write `_bmad-output/unity-test-results/myb-17-private-demo-readiness.txt`;
  - include private-demo verdict and launch checklist.
- [ ] Keep demo packaging bounded:
  - validate Build Settings if cheap and deterministic;
  - optionally run a local macOS build only if stable and not noisy;
  - do not commit generated builds, videos or large artifacts;
  - document any deferred build step factually.
- [ ] Preserve previous behavior:
  - do not change route, camera, fog, HUD, controls, mock loop or SceneLife placement/visuals;
  - do not remove MYB-15 or MYB-16 menu items;
  - keep the React/Three prototype untouched.
- [ ] Final validation:
  - run MYB-17 menu through Unity MCP if available, otherwise batchmode fallback;
  - read the generated report and confirm private-demo verdict;
  - confirm Unity console 0 error / 0 warning after validation;
  - `git diff --check`;
  - high-confidence secret scan;
  - verify no `src/ride/*`, `src/render/*`, `src/app/*` changes;
  - verify no video capture staged;
  - do not run `npm run typecheck`, `npm run test`, or `npm run build` unless web source changes are detected.

## Probable Files

- `unity/Echappee3D/Assets/Echappee/Editor/DemoReadinessValidator.cs`
- `unity/Echappee3D/Assets/Echappee/Editor/RideMockValidator.cs`
- `unity/Echappee3D/Assets/Echappee/Editor/PrivateDemoValidator.cs` if a separate file is cleaner
- `unity/Echappee3D/Assets/Echappee/Tests/EditMode/` if small pure helper tests are added
- `unity/Echappee3D/Assets/Scenes/RideMock.scene-plan.md` only if launch checklist documentation belongs beside the scene
- `_bmad-output/unity-test-results/myb-17-private-demo-readiness.txt`

## Validation Plan

- Unity MCP preferred:
  - confirm project root `/Users/jbodin/personnel/apps/mybike/unity/Echappee3D`;
  - confirm active scene `Assets/Scenes/RideMock.unity`;
  - confirm Editor idle, not compiling, not in Play Mode;
  - verify menu `Echappee/MYB-17/Validate Private Demo` exists;
  - execute the menu;
  - read `_bmad-output/unity-test-results/myb-17-private-demo-readiness.txt`;
  - confirm route/camera/fog/HUD/controls/session loop/SceneLife readiness;
  - confirm private-demo verdict;
  - confirm Unity console 0 error / 0 warning.
- Batchmode Unity fallback is allowed only if MCP Unity is unavailable.
- `git diff --check`.
- High-confidence secret scan against diff/staged diff.
- Verify no `src/ride/*`, `src/render/*`, `src/app/*` file changed.
- Verify no video capture staged.
- Do not run `npm run typecheck`, `npm run test`, or `npm run build` unless web source changes are detected.

## Guardrails

- This is a local private-demo readiness story, not a gameplay expansion.
- Do not add vehicles, traffic, AI, pedestrians, new life types, Meshy, external assets or Unity AI generation.
- Do not tune visuals to make the report look better; MYB-17 should certify the current vertical slice.
- Do not create a public release, deployment pipeline, cloud backend, installer, notarization or distribution workflow.
- Do not create MYB-18 or a new backlog in this pass.
- Preserve the React/Three prototype as reference and do not touch web source.

## Assumed Limits

- Editor-based private demo readiness is acceptable if build packaging is not stable enough for this story.
- Build Settings or local macOS build validation is optional and should be included only if it remains low-risk.
- The report can be text-only; no video capture is required for MYB-17 acceptance.
- This is not final QA, final art direction or a public release gate.

## Dev Agent Record

### Status

`ready-for-dev`

### Notes

- MYB-17 follows MYB-16 because the vertical slice now has a measurable readiness signal and needs a launchable private-demo wrapper.
- Keep implementation focused on validating and documenting the current demo state.
- Prefer reusing `DemoReadinessValidator` and `RideMockValidator` rather than adding another parallel validation system.
- Treat local build packaging as opportunistic, not mandatory.

### File List

- `_bmad-output/implementation-artifacts/myb-17-unity-private-demo-package-and-launch-checklist.md`
- `_bmad-output/implementation-artifacts/sprint-status.yaml`
- `_bmad-output/linear-sync.md`

### Change Log

- 2026-06-07: Created MYB-17 story and tracking for Unity private demo package and launch checklist.

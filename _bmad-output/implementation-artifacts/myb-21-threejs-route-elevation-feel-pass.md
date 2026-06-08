# Story MYB-21: Three.js route elevation feel pass

## Metadata

- Linear issue: `MYB-21`
- Linear URL: https://linear.app/kefjbo/issue/MYB-21/threejs-route-elevation-feel-pass
- Local status: `done`
- Linear status: `Done`
- Created: `2026-06-08`
- Linear sync comment: `140168c5-b8af-43d1-b9c6-08ee4accaa49`
- Linear implementation comment: `1ff40e6c-b22e-44d6-9be1-1efedb6940eb`
- Linear review comment: `c046114b-3f16-46e3-af4d-49903d3698a7`
- Baseline commit: `40ef48073e66683e7216ddd209c8cf2c1385e06d`
- Depends on: `MYB-20`
- Active target: React/Vite/TypeScript/Three.js web demo
- Unity status: parking/research reference only, no implementation edit

## Story

As a private demo owner,
I want the Three.js route to have perceptible climbs and descents while staying grounded,
so that the ride no longer feels almost flat and the road does not read as a floating ribbon.

## Context

MYB-18 recovered the Three.js demo after Unity WebGL felt floaty. MYB-19 made the browser demo launch-ready, and MYB-20 restored lightweight birds and human silhouettes. The current Three.js route technically has elevation, but the values are only about 0 to 2 meters across a 1000 meter route, so the visual slope feel is too subtle.

MYB-21 should amplify simple controlled route elevation in the web demo. The important risk is grounding: raising the road without adapting nearby terrain would recreate the Unity/WebGL "road in the void" feeling. The pass must therefore make the route and terrain/verges read as one grounded surface.

## Acceptance Criteria

1. Web-only scope is respected: implementation changes stay in React/Vite/Three.js, tests, BMAD/tracking and web validation evidence.
2. No `unity/Echappee3D/**` implementation change is made.
3. The mock route has visibly stronger elevation variation than MYB-20, with at least one climb and one descent.
4. Route elevation is bounded and finite; max absolute elevation stays within a POC-safe range.
5. Route grade is bounded and finite; no spike creates clipping, nausea, or unreadable geometry.
6. The road remains grounded: nearby terrain/verges follow the route elevation closely enough that the route does not appear to float.
7. Camera-on-rail continues to follow the elevated route without clipping through the road or jumping.
8. MYB-18 grounding cues are preserved: shoulders, verges, contact shadows, road markings and near-road anchors remain readable.
9. MYB-20 scene life is preserved: birds and human silhouettes remain present and visible enough in capture.
10. HUD, controls and mock loop remain usable: start, ride, pause, resume, finish and summary are not regressed.
11. Fog/depth remains active and does not hide the road immediately.
12. Deterministic tests cover route elevation bounds, climb/descent presence, terrain elevation following, and existing route mesh finiteness.
13. Validation passes: `npm run typecheck`, `npm run test`, `npm run build`, `npm run validate:private-demo`.
14. A 60s capture/contact sheet is generated and reviewed; the route should read more sloped while remaining grounded.
15. Hygiene passes: `git diff --check`, high-confidence secret scan, no Unity implementation diff staged, and no video capture artifact staged.
16. Tracking stays aligned: this story, `sprint-status.yaml`, `_bmad-output/linear-sync.md`, and Linear MYB-21 are updated with implementation and review proof.

## Implementation Tasks

- [x] Preflight and protect scope. (AC: 1, 2, 15)
  - [x] Check `git status --short` and identify unrelated Unity/WebGL leftovers.
  - [x] Confirm no intended write under `unity/Echappee3D/**`.
  - [x] Confirm `src/ride/*` is not part of the intended change surface.

- [x] Inspect current route elevation pipeline. (AC: 3, 6, 7, 8)
  - [x] Read `src/route/mockRouteDefinition.ts`.
  - [x] Read route sampling/camera helpers.
  - [x] Read `src/render/createRouteMesh.ts` and `SceneController` terrain generation.
  - [x] Identify where terrain must follow route elevation to preserve grounding.

- [x] Amplify controlled elevation. (AC: 3, 4, 5, 7)
  - [x] Increase mock route `position.y` values within bounded POC-safe limits.
  - [x] Preserve monotonic distances and finite samples.
  - [x] Keep at least one climb and one descent.
  - [x] Avoid extreme grades.

- [x] Anchor terrain to route elevation. (AC: 6, 8, 11)
  - [x] Add or reuse a deterministic route elevation interpolation helper.
  - [x] Make near-road ground height follow route elevation.
  - [x] Preserve lateral berm/ridge shaping without creating route/terrain gaps.

- [x] Preserve scenic life and ride loop. (AC: 9, 10)
  - [x] Avoid moving birds/human silhouettes unless a route elevation change makes a placement visibly broken.
  - [x] Preserve HUD/controls/session behavior.

- [x] Add or update tests. (AC: 4, 5, 6, 7, 12)
  - [x] Cover route elevation range and climb/descent signs.
  - [x] Cover grade bounds.
  - [x] Cover ground/terrain helper alignment with route elevation near the road.
  - [x] Keep existing render tests green.

- [x] Validate and capture. (AC: 13, 14, 15)
  - [x] Run `npm run typecheck`.
  - [x] Run `npm run test`.
  - [x] Run `npm run build`.
  - [x] Run `npm run validate:private-demo`.
  - [x] Review the generated contact sheet or screenshots.
  - [x] Verify no page errors, no console errors and canvas nonblank.
  - [x] Run `git diff --check`.
  - [x] Run a high-confidence secret scan.

- [x] Update tracking after implementation. (AC: 16)
  - [x] Move MYB-21 to review in this story and `sprint-status.yaml`.
  - [x] Add implementation evidence to `_bmad-output/linear-sync.md`.
  - [x] Add a Linear implementation comment and set MYB-21 to In Review.

## Probable Files

Likely implementation files:

- `src/route/mockRouteDefinition.ts`
- `src/route/mockRouteDefinition.test.ts`
- `src/route/sampleRouteAt.test.ts`
- `src/render/renderHelpers.ts`
- `src/render/renderHelpers.test.ts`
- `src/render/SceneController.ts`
- `src/render/createRouteMesh.test.ts` if mesh elevation coverage needs expansion

Likely validation/tracking files:

- `_bmad-output/implementation-artifacts/myb-21-threejs-route-elevation-feel-pass.md`
- `_bmad-output/implementation-artifacts/sprint-status.yaml`
- `_bmad-output/linear-sync.md`
- `_bmad-output/web-test-results/myb-19-private-demo-readiness.txt`
- `_bmad-output/video-captures/<new-capture-dir>/` as evidence only, not staged

Forbidden implementation files:

- `unity/Echappee3D/**`
- backend/deployment files
- BLE/FTMS integration files

## Developer Notes

### Current Elevation Problem

Current route points use very small `y` values:

```text
0, 1, 0, 2, 1, 0, 1
```

That is technically non-flat, but it is visually too subtle over a 1000 meter route. Raising the route alone is not enough; the terrain near the route must also follow the same elevation envelope.

### Suggested Elevation Shape

Keep it simple and controlled. A POC-safe target is roughly 0 to 12 meters across the route, with slopes that are noticeable but not roller-coaster-like.

Example shape to consider:

```text
0, 6, 2, 12, 4, 9, 3
```

Use the actual implementation and tests to decide final values.

### Grounding Rule

Near-road terrain should use route elevation as its base height. Berms/ridges can add lateral shape on top of that base, but the road corridor itself should not leave a visible vertical gap.

## Validation Plan

Implementation must run:

```bash
npm run typecheck
npm run test
npm run build
npm run validate:private-demo
```

Final hygiene:

```bash
git diff --check
git diff --name-only -- unity/Echappee3D src/ride
git diff | <high-confidence secret scan>
```

Do not stage capture videos or screenshots unless explicitly requested.

## Guardrails

- Do not create MYB-22 or a broad backlog.
- Do not edit Unity.
- Do not use Meshy or add external assets.
- Do not add vehicles, traffic, AI, physics cycling, collision gameplay, BLE/FTMS, backend, public deployment, auth or cloud sync.
- Do not turn this into a full terrain system. This is a controlled route elevation feel pass only.

## Definition of Done

- Three.js route has perceptible but bounded climbs and descents.
- Nearby terrain remains visually anchored to the road.
- MYB-20 birds/human silhouettes remain present.
- MYB-19 readiness remains `ready-for-private-web-demo`.
- Required npm validations pass.
- 60s capture evidence exists and is reviewed.
- No Unity implementation edit is made.
- Linear, story file, `sprint-status.yaml`, and `_bmad-output/linear-sync.md` are aligned.

## Dev Agent Record

### Status

`done`

### Agent Model Used

Codex GPT-5

### Debug Log References

- Story/tracking creation only: npm validations intentionally not run.
- Story/tracking commit: `0e735d6e5cd83b40e5b7961c34e4efb5c63dd60d`.
- Targeted route/render validation: `npx vitest run src/route/mockRouteDefinition.test.ts src/route/sampleRouteAt.test.ts src/route/cameraOnRail.test.ts src/render/renderHelpers.test.ts src/render/groundHeight.test.ts src/render/createRouteMesh.test.ts`.
- Short capture `_bmad-output/video-captures/ride-visual-audit-2026-06-08T06-55-18-735Z/` showed the first elevation attempt was too aggressive and hid the road.
- Corrected terrain lateral falloff, moderated the elevation profile, raised camera height/lookahead slightly, and densified the ground mesh.
- Final full validation: `npm run typecheck`, `npm run test`, `npm run build`, `npm run validate:private-demo`.
- Final readiness report: `_bmad-output/web-test-results/myb-19-private-demo-readiness.txt`.
- Implementation capture: `_bmad-output/video-captures/ride-visual-audit-2026-06-08T07-00-23-806Z/`.
- Review finding: human silhouettes needed to follow elevated terrain instead of staying at absolute `y=0`.
- Review validation: `npm run validate:private-demo`.
- Review capture: `_bmad-output/video-captures/ride-visual-audit-2026-06-08T07-04-58-032Z/`.
- Post-review human feedback: the first route elevation pass could read as broken road panels instead of smooth slopes.
- Post-review route continuity fix: densified the Three.js route ribbon to approximately 9 m rendered samples and added regression coverage in `createRouteMesh.test.ts`.
- Post-review validation: targeted route/render tests, `npm run typecheck`, `npm run test`, `npm run build`, `npm run validate:private-demo`.
- Post-review capture: `_bmad-output/video-captures/ride-visual-audit-2026-06-08T07-21-11-816Z/`.

### Completion Notes

- Amplified the route elevation from roughly 0-2 m to 0-10 m with bounded climbs and descents.
- Added route elevation interpolation for rendering helpers.
- Added `calculateGroundHeight` so the road corridor follows route elevation and lateral terrain falls off smoothly.
- Densified the ground mesh to prevent broad ground triangles from covering the road on slopes.
- Adjusted default camera height/lookahead to keep crests readable without returning to a flying viewpoint.
- Preserved MYB-20 birds/human silhouettes and MYB-19 private demo readiness.
- Final contact sheet shows a readable, grounded road over the full capture, with stronger slope feel and visible birds.
- After human browser feedback, corrected the route ribbon itself so pentes render as continuous road instead of large broken panels.

### File List

- `_bmad-output/implementation-artifacts/myb-21-threejs-route-elevation-feel-pass.md`
- `_bmad-output/implementation-artifacts/sprint-status.yaml`
- `_bmad-output/linear-sync.md`
- `src/route/mockRouteDefinition.ts`
- `src/route/mockRouteDefinition.test.ts`
- `src/route/sampleRouteAt.test.ts`
- `src/route/cameraOnRail.ts`
- `src/route/cameraOnRail.test.ts`
- `src/render/renderHelpers.ts`
- `src/render/renderHelpers.test.ts`
- `src/render/groundHeight.ts`
- `src/render/groundHeight.test.ts`
- `src/render/createRouteMesh.ts`
- `src/render/createRouteMesh.test.ts`
- `src/render/SceneController.ts`
- `_bmad-output/web-test-results/myb-19-private-demo-readiness.txt`
- `_bmad-output/video-captures/ride-visual-audit-2026-06-08T07-00-23-806Z/` (implementation evidence, not staged)
- `_bmad-output/video-captures/ride-visual-audit-2026-06-08T07-04-58-032Z/` (review evidence, not staged)
- `_bmad-output/video-captures/ride-visual-audit-2026-06-08T07-21-11-816Z/` (post-review continuity evidence, not staged)

### Change Log

- 2026-06-08: Created MYB-21 story for Three.js route elevation feel.
- 2026-06-08: Implemented Three.js route elevation feel pass and moved MYB-21 to review.
- 2026-06-08: Review approved MYB-21 after anchoring human silhouettes to elevated terrain.
- 2026-06-08: Corrected post-review route continuity after human browser feedback showed the initial slope mesh could read as broken panels.

## Senior Developer Review (AI)

### Verdict

Approved on 2026-06-08 after one scoped correction.

### Findings

- Fixed: MYB-20 human silhouettes were still placed at absolute `y≈0`, which could bury them under the elevated MYB-21 terrain. They now use `calculateGroundHeight(...) + 0.02`.
- Fixed post-review: the elevated road ribbon was under-sampled and could read as broken panels. `createRouteMesh` now renders the ribbon with approximately 9 m samples, covered by a regression test.

### Review Evidence

- `npm run validate:private-demo`: pass.
- Harness executed `npm run typecheck`: pass.
- Harness executed `npm run test`: pass, 24 files / 81 tests.
- Harness executed `npm run build`: pass, Vite chunk-size warning only.
- Harness executed `npm run capture:ride-video`: pass, 60s capture.
- Review report: `_bmad-output/web-test-results/myb-19-private-demo-readiness.txt`.
- Review verdict: `ready-for-private-web-demo`.
- Review capture: `_bmad-output/video-captures/ride-visual-audit-2026-06-08T07-04-58-032Z/`.
- Review contact sheet shows a visible, grounded route over stronger slopes, visible birds, and a readable roadside human silhouette.
- HTTP 200, `canvasNonBlank: true`, `pageErrors: []`, `consoleMessages: []`.
- Post-review route/render tests: pass, 6 files / 26 tests.
- Post-review `npm run typecheck`: pass.
- Post-review `npm run test`: pass, 24 files / 82 tests.
- Post-review `npm run build`: pass, Vite chunk-size warning only.
- Post-review `npm run validate:private-demo`: pass, `ready-for-private-web-demo`.
- Post-review capture: `_bmad-output/video-captures/ride-visual-audit-2026-06-08T07-21-11-816Z/`.
- Post-review contact sheet shows the route as a continuous ribbon over the stronger elevation pass.

### Scope Checks

- No `src/ride/*` edits.
- No `unity/Echappee3D/**` implementation edits.
- No Meshy, external asset, backend, BLE/FTMS, public deployment or engine migration.
- Capture artifacts remain evidence-only and are not staged.

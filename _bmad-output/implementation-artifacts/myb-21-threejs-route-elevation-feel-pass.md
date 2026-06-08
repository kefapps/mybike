# Story MYB-21: Three.js route elevation feel pass

## Metadata

- Linear issue: `MYB-21`
- Linear URL: https://linear.app/kefjbo/issue/MYB-21/threejs-route-elevation-feel-pass
- Local status: `ready-for-dev`
- Linear status: `In Progress`
- Created: `2026-06-08`
- Linear sync comment: `140168c5-b8af-43d1-b9c6-08ee4accaa49`
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

- [ ] Preflight and protect scope. (AC: 1, 2, 15)
  - [ ] Check `git status --short` and identify unrelated Unity/WebGL leftovers.
  - [ ] Confirm no intended write under `unity/Echappee3D/**`.
  - [ ] Confirm `src/ride/*` is not part of the intended change surface.

- [ ] Inspect current route elevation pipeline. (AC: 3, 6, 7, 8)
  - [ ] Read `src/route/mockRouteDefinition.ts`.
  - [ ] Read route sampling/camera helpers.
  - [ ] Read `src/render/createRouteMesh.ts` and `SceneController` terrain generation.
  - [ ] Identify where terrain must follow route elevation to preserve grounding.

- [ ] Amplify controlled elevation. (AC: 3, 4, 5, 7)
  - [ ] Increase mock route `position.y` values within bounded POC-safe limits.
  - [ ] Preserve monotonic distances and finite samples.
  - [ ] Keep at least one climb and one descent.
  - [ ] Avoid extreme grades.

- [ ] Anchor terrain to route elevation. (AC: 6, 8, 11)
  - [ ] Add or reuse a deterministic route elevation interpolation helper.
  - [ ] Make near-road ground height follow route elevation.
  - [ ] Preserve lateral berm/ridge shaping without creating route/terrain gaps.

- [ ] Preserve scenic life and ride loop. (AC: 9, 10)
  - [ ] Avoid moving birds/human silhouettes unless a route elevation change makes a placement visibly broken.
  - [ ] Preserve HUD/controls/session behavior.

- [ ] Add or update tests. (AC: 4, 5, 6, 7, 12)
  - [ ] Cover route elevation range and climb/descent signs.
  - [ ] Cover grade bounds.
  - [ ] Cover ground/terrain helper alignment with route elevation near the road.
  - [ ] Keep existing render tests green.

- [ ] Validate and capture. (AC: 13, 14, 15)
  - [ ] Run `npm run typecheck`.
  - [ ] Run `npm run test`.
  - [ ] Run `npm run build`.
  - [ ] Run `npm run validate:private-demo`.
  - [ ] Review the generated contact sheet or screenshots.
  - [ ] Verify no page errors, no console errors and canvas nonblank.
  - [ ] Run `git diff --check`.
  - [ ] Run a high-confidence secret scan.

- [ ] Update tracking after implementation. (AC: 16)
  - [ ] Move MYB-21 to review in this story and `sprint-status.yaml`.
  - [ ] Add implementation evidence to `_bmad-output/linear-sync.md`.
  - [ ] Add a Linear implementation comment and set MYB-21 to In Review.

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

`ready-for-dev`

### Agent Model Used

Codex GPT-5

### Debug Log References

- Story/tracking creation only: npm validations intentionally not run.

### Completion Notes

- Pending implementation.

### File List

- `_bmad-output/implementation-artifacts/myb-21-threejs-route-elevation-feel-pass.md`
- `_bmad-output/implementation-artifacts/sprint-status.yaml`
- `_bmad-output/linear-sync.md`

### Change Log

- 2026-06-08: Created MYB-21 story for Three.js route elevation feel.

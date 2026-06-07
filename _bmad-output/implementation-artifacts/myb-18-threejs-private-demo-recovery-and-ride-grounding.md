# Story MYB-18: Three.js private demo recovery and ride grounding

## Metadata

- Linear issue: `MYB-18`
- Linear URL: https://linear.app/kefjbo/issue/MYB-18/threejs-private-demo-recovery-and-ride-grounding
- Local status: `ready-for-dev`
- Linear status: `In Progress`
- Created: `2026-06-07`
- Baseline commit: `6794394b5d878ac431a888dcad94a783dca59577`
- Depends on: `MYB-10`, `MYB-17`
- Correct-course source: `_bmad-output/planning-artifacts/sprint-change-proposal-threejs-demo-recovery-2026-06-07.md`
- Unity WebGL comparison capture: `_bmad-output/video-captures/unity-webgl-running-2026-06-07T19-59-58-250Z/`
- Active architecture source: `_bmad-output/planning-artifacts/echappee-3d-architecture-mince.md`
- Unity architecture status: parking/research reference only

## Story

As a private demo owner,
I want the React/Vite/Three.js ride to feel grounded again with a stable road, comfortable camera and nearby visual anchors,
so that the next private demo is convincing without continuing the Unity migration.

## Context

MYB-10 produced the strongest current web reference for motion, density and scenic mood. MYB-11 through MYB-17 then explored Unity as a production target. The Unity WebGL demo from MYB-17 loads and advances, but the real 60s browser capture showed a weak ride feel:

- the HUD and distance prove the ride advances;
- the view feels like flying rather than riding;
- the road reads as a ribbon in empty space;
- nearby ground/proximity anchors are too weak;
- Unity iteration is slower than the web loop for demo polish.

Correct-course decision:

```text
Three.js redevient la cible principale de la demo privee.
Unity reste conserve en parking/recherche, sans suppression.
```

MYB-18 is the first recovery story after that decision. It should not become a new visual backlog. Its job is to make the web demo feel grounded enough for a private demo and to prove that with real browser capture evidence.

## Recovery Target

MYB-18 should answer one practical question:

```text
Can Julien run the web demo and feel like the camera is riding along a grounded road, not floating above an abstract strip?
```

The expected answer after implementation is a visual proof bundle:

- Vite local URL responding with HTTP 200;
- 60s video capture;
- screenshots/contact sheet;
- no page errors;
- route visible and grounded throughout the run;
- no major flickering or route disappearance.

## Acceptance Criteria

1. Web-only scope is respected: implementation changes stay in the React/Vite/Three.js surface plus BMAD/tracking/capture artifacts. No `unity/Echappee3D/**` change is allowed.
2. Three.js remains the active private-demo target. Unity is not deleted, edited, rebuilt, or used as proof for MYB-18 acceptance.
3. The route feels grounded in the web demo: visible shoulders, edges, ground contact, roadside anchors, shadow/contrast bands, or equivalent procedural cues make the road read as sitting in a world rather than floating.
4. The camera feel is improved for the private demo: lower/closer, more road-attached, or otherwise less floaty, while remaining comfortable and without abrupt motion.
5. Any camera or route changes preserve the pure route architecture. `cameraOnRail`, route sampling and route definitions remain deterministic, finite and covered by tests.
6. The route remains visible for the full capture and does not show major flickering, disappearance, z-fighting, clipping, or sudden blank frames.
7. Fog/depth from MYB-10 is preserved or improved; the appreciated long-distance haze should not be removed.
8. Nearby visual anchors are present and bounded. They may include shoulders, verge bands, posts, grass/stone clusters, simple life silhouettes, or other procedural markers, but must not obscure the ride corridor.
9. Simple slope/elevation cues may be added or strengthened only if they help grounding and remain deterministic. No terrain generation system, route editor, GPX import or route selector is introduced.
10. Lightweight life may be added or repositioned only if it directly supports readability in the web demo. No vehicle/traffic/AI system is added.
11. The ride logic remains product-mock stable. Do not change `src/ride/*` speed mapping, smoothing, progression, stats, input source or session loop unless a small test-backed fix is strictly necessary and documented.
12. No Meshy, external asset, glTF, image texture, Unity AI generation, backend, BLE/FTMS, public deployment, or new engine migration work is introduced.
13. Existing MYB-10 capture-clean-HUD flow remains usable. If the capture script is adjusted, it remains local and deterministic.
14. Automated validation passes: `npm run typecheck`, `npm run test`, and `npm run build`.
15. Visual validation passes: run a local Vite server and generate a 60s browser capture plus screenshots/contact sheet under `_bmad-output/video-captures/`.
16. The final capture evidence includes HTTP 200, canvas nonblank, no page errors, and a short written assessment comparing the result against the MYB-17 Unity WebGL capture.
17. Tracking stays aligned: this story, `sprint-status.yaml`, `_bmad-output/linear-sync.md`, and Linear MYB-18 are updated with implementation proof after `gds-dev-story`.

## Implementation Tasks

- [ ] Preflight and protect scope. (AC: 1, 2, 11, 12)
  - [ ] Check `git status --short` and identify unrelated existing WebGL/Unity artifacts before editing.
  - [ ] Confirm no planned edit under `unity/Echappee3D/**`.
  - [ ] Confirm `src/ride/*` is not part of the intended change surface.
  - [ ] Keep the story implementation focused on one demo recovery pass, not a broad visual roadmap.

- [ ] Inspect the active web rendering path. (AC: 3, 4, 5, 7, 8)
  - [ ] Read `src/render/SceneController.ts`.
  - [ ] Read `src/render/createRouteMesh.ts`.
  - [ ] Read `src/render/createBiomeVisuals.ts`.
  - [ ] Read `src/render/renderHelpers.ts` and `src/render/sceneTypes.ts`.
  - [ ] Read `src/route/cameraOnRail.ts`, `src/route/mockRouteDefinition.ts`, and relevant route tests before changing camera/route behavior.
  - [ ] Read `scripts/capture-ride-video.mjs` and existing capture outputs before changing capture flow.

- [ ] Improve grounded road perception. (AC: 3, 6, 8, 9)
  - [ ] Strengthen road contact cues with bounded procedural elements: shoulders, verge strips, ground bands, edge markings, contact shadows, low roadside reference points, or equivalent.
  - [ ] Ensure elements track the route center with existing helpers such as `getRouteCenterXAtZ`.
  - [ ] Keep anchors outside the ride corridor and avoid occluding the road center.
  - [ ] Avoid z-fighting by using clear vertical offsets/material choices.

- [ ] Improve camera feel without breaking route contracts. (AC: 4, 5, 6, 11)
  - [ ] Tune only the minimal camera parameters or render framing needed to reduce the flying sensation.
  - [ ] Preserve deterministic `cameraOnRail` behavior and add/update tests for any changed defaults.
  - [ ] Keep FOV/look-ahead/height finite and bounded at route start, midpoints and end.
  - [ ] Do not fake speed by changing ride math.

- [ ] Rapatrier only useful Unity learnings. (AC: 7, 8, 9, 10, 12)
  - [ ] Use the Unity WebGL capture as a negative comparison for what to avoid: ribbon road, weak ground anchors, life too far/line-like.
  - [ ] Add or strengthen simple slope/elevation cues only if they improve web grounding.
  - [ ] Add or strengthen lightweight life only if it supports readability and stays procedural.
  - [ ] Preserve fog/depth and scenic mood from MYB-10.

- [ ] Add focused tests. (AC: 5, 6, 8, 13)
  - [ ] Extend route mesh tests for any new road grounding elements, counts and resource tracking.
  - [ ] Extend biome visual tests for bounded anchors/life placement if touched.
  - [ ] Extend camera route tests if camera defaults or camera config change.
  - [ ] Keep tests deterministic and independent of screenshots.

- [ ] Produce real demo proof. (AC: 14, 15, 16)
  - [ ] Run `npm run typecheck`.
  - [ ] Run `npm run test`.
  - [ ] Run `npm run build`.
  - [ ] Start/let the capture script start a local Vite server.
  - [ ] Run `npm run capture:ride-video` or an equivalent 60s capture flow.
  - [ ] Verify HTTP 200, no page errors, nonblank canvas and screenshot/video artifacts.
  - [ ] Write a short comparison note against `_bmad-output/video-captures/unity-webgl-running-2026-06-07T19-59-58-250Z/`.

- [ ] Update tracking after implementation. (AC: 17)
  - [ ] Move MYB-18 to review in the story and `sprint-status.yaml`.
  - [ ] Add capture paths and validation evidence to `_bmad-output/linear-sync.md`.
  - [ ] Add a Linear implementation comment and set MYB-18 to In Review.

## Probable Files

Likely implementation files:

- `src/render/SceneController.ts`
- `src/render/createRouteMesh.ts`
- `src/render/createRouteMesh.test.ts`
- `src/render/createBiomeVisuals.ts`
- `src/render/createBiomeVisuals.test.ts`
- `src/render/renderHelpers.ts` only if palette/grounding helpers need small additions
- `src/render/renderHelpers.test.ts` if helper behavior changes
- `src/route/cameraOnRail.ts` only if camera defaults/config change
- `src/route/cameraOnRail.test.ts` if camera behavior changes
- `src/route/mockRouteDefinition.ts` only if simple elevation/route data changes
- `src/route/sampleRouteAt.test.ts` or `src/route/mockRouteDefinition.test.ts` if route data changes
- `scripts/capture-ride-video.mjs` only for capture duration/report tweaks
- `src/app/screens/RideScreen.tsx` / `src/styles.css` only if capture HUD or viewport presentation needs tiny support

Tracking/evidence files:

- `_bmad-output/implementation-artifacts/myb-18-threejs-private-demo-recovery-and-ride-grounding.md`
- `_bmad-output/implementation-artifacts/sprint-status.yaml`
- `_bmad-output/linear-sync.md`
- `_bmad-output/video-captures/<new-capture-dir>/`

Forbidden for this story:

- `unity/Echappee3D/**`
- Meshy-generated assets
- external images/textures/models
- backend/deployment files
- BLE/FTMS integration files

## Developer Notes

### Current Web State

- `SceneController.ts` owns the Three.js scene, renderer, camera application, fog/background updates, ground and horizon. It already tracks and disposes ground/horizon/route/biome resources.
- `createRouteMesh.ts` already builds layered route geometry with surface, shoulders, edge bands, center markings, surface bands, shoulder rhythm and surface grain.
- `createBiomeVisuals.ts` already creates dense procedural scenery and near-road motion details. Existing tests expect bounded counts and route-aware offsets outside the corridor.
- `cameraOnRail.ts` is pure route math. Current tests assert deterministic position/look-ahead/FOV, finite values, null config defaults and end-of-route stability.
- `RideScreen.tsx` already has `captureHud=hidden` support used by the capture flow.
- `scripts/capture-ride-video.mjs` already produces video/contact sheet/frame evidence and should remain the primary proof path.

### Architecture Guardrails

- React owns UI and screen/session state.
- `src/ride/*` owns pure ride logic and should remain out of scope.
- `src/route/*` owns pure route/camera math. If touched, preserve deterministic finite outputs and add tests.
- `src/render/*` may import Three.js and should apply snapshots rather than compute ride progression.
- Any new mesh/material/geometry must be tracked for disposal.
- Procedural visuals must remain bounded; no random unbounded generation in the render loop.

### Lessons From Unity Spike

Use as learnings, not code to port:

- positive: simple pentes, fog/depth, life/readability metrics, validation reports;
- negative: camera felt floaty, road read as a ribbon in void, visual validators missed perceived quality, WebGL iteration was slow;
- action: acceptance must be a browser capture humans can inspect.

### Validation Evidence Expected

Implementation should record:

- `npm run typecheck`: pass;
- `npm run test`: pass;
- `npm run build`: pass, with any known Vite chunk warning noted if present;
- capture directory under `_bmad-output/video-captures/`;
- `capture-summary.json` path and key results;
- final screenshot/contact sheet/video path;
- short human-readable statement: grounded enough / still blocked, with concrete visual reasons.

## Guardrails

- Do not create MYB-19 or a new backlog in this story.
- Do not continue Unity polish inside MYB-18.
- Do not delete Unity.
- Do not use Meshy or any paid generation tool.
- Do not add vehicles, traffic, AI, physics cycling, collision gameplay, BLE/FTMS, backend, public deployment, auth or cloud sync.
- Do not claim demo readiness from unit tests alone; visual capture is required.

## Definition of Done

- MYB-18 has a web demo capture that reads more grounded than the MYB-17 Unity WebGL capture.
- Route visibility is stable for the captured run.
- Camera feel is less floaty without abrupt motion.
- Fog/depth remains present.
- Nearby anchors improve scale and speed perception without cluttering the ride corridor.
- `npm run typecheck`, `npm run test`, and `npm run build` pass.
- Capture evidence is saved under `_bmad-output/video-captures/`.
- No `unity/Echappee3D/**` change is made for implementation.
- No Meshy, external asset, backend, BLE/FTMS, deployment or new migration work is added.
- Linear, story file, `sprint-status.yaml`, and `_bmad-output/linear-sync.md` are aligned.

## Dev Agent Record

### Agent Model Used

TBD during implementation.

### Debug Log References

- Story/tracking creation only: npm validations intentionally not run.

### Completion Notes

TBD during implementation.

### File List

TBD during implementation.

### Change Log

- 2026-06-07: Created MYB-18 story from post-MYB-17 correct-course; Three.js restored as private demo target, Unity parked/research.

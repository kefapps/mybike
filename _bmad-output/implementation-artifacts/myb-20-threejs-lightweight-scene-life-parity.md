# Story MYB-20: Three.js lightweight scene life parity

## Metadata

- Linear issue: `MYB-20`
- Linear URL: https://linear.app/kefjbo/issue/MYB-20/threejs-lightweight-scene-life-parity
- Local status: `done`
- Linear status: `Done`
- Created: `2026-06-08`
- Linear sync comment: `45b4e1c3-cef5-4ad0-b496-d468635493c4`
- Linear implementation comment: `7dadf92c-b211-4e64-9fce-ad2f09cb68be`
- Linear review comment: `5ec7770e-9c84-4a0f-abb4-5028a391396c`
- Baseline commit: `6059f95b8ad170b88b6ee8a382db5647779ca737`
- Depends on: `MYB-19`
- Active target: React/Vite/TypeScript/Three.js web demo
- Unity status: parking/research reference only, no implementation edit
- Existing Meshy references:
  - `unity/Echappee3D/Assets/Generated/Meshy/MYB14/meshy_bird_preview.fbx`
  - `unity/Echappee3D/Assets/Generated/Meshy/MYB14/meshy_human_silhouette_preview.fbx`

## Story

As a private demo owner,
I want the Three.js demo to regain the lightweight birds and human silhouettes that exist in the Unity slice,
so that the browser demo feels less empty while preserving the grounded MYB-18 ride and MYB-19 launch readiness.

## Context

MYB-18 and MYB-19 restored Three.js as the active private-demo target and made it locally demonstrable with a readiness harness. However, the Unity-only MYB-14/MYB-16 scene-life work did not come back to the web demo. The current browser build has a grounded road, fog/depth, HUD and controls, but it does not yet show birds or human silhouettes.

MYB-20 is a bounded parity pass. It should add visible, lightweight scene life to the Three.js renderer without reopening Unity migration, without adding traffic or AI, and without turning the story into a broad visual overhaul.

The MYB-14 Meshy assets are allowed as existing references because they were already generated and paid for:

- Bird preview task: `019ea2e1-4bc6-7aa6-a8c2-229f16fd7a6a`.
- Human silhouette preview task: `019ea2e1-522e-7aac-b1c2-cad77720ea26`.
- Model: `meshy-5`.
- Formats requested: `fbx`.
- Credits already used: `10 total`.
- No refine/remesh/retexture/rig/animation call was used.

Do not make any new Meshy call in MYB-20 unless Julien explicitly validates the additional credit cost.

## Acceptance Criteria

1. Web-only implementation scope is respected: changes stay in React/Vite/Three.js, local scripts/tests, BMAD/tracking and web validation evidence.
2. `unity/Echappee3D/**` is not modified. The existing MYB-14 Meshy FBX files may be read or referenced only.
3. The Three.js scene contains lightweight scene-life parity: 5 stylized birds and 3 static human silhouettes, or an equivalent explicitly documented count if a technical constraint proves a smaller visible count is better.
4. Birds are visible in the forward ride view during the 60s browser capture without dominating the horizon or blocking the road.
5. Human silhouettes are visible near the roadside during the 60s browser capture, off the road and outside the player lane.
6. No vehicles, traffic system, AI, collision gameplay, flocking system, animation system, backend, BLE/FTMS, public deploy or new engine migration is introduced.
7. Existing Meshy MYB-14 FBX assets are evaluated for web reuse. If they are not used at runtime, the implementation documents why the lighter Three.js procedural path won.
8. No new Meshy generation/refine/remesh/retexture/rig/animation call is made without explicit cost confirmation.
9. MYB-18 ride grounding is preserved: road verges/contact cues, camera framing and fog/depth remain readable.
10. MYB-19 launch readiness remains valid: the report still reaches `ready-for-private-web-demo`.
11. HUD, controls and mock loop remain usable: start, ride, pause, resume, finish and summary are not regressed.
12. Deterministic tests cover scene-life counts, broad placement constraints and basic material/visibility assumptions.
13. Validation passes: `npm run typecheck`, `npm run test`, `npm run build`, `npm run validate:private-demo`.
14. A 60s capture and screenshots are generated and reviewed; the capture evidence must show or strongly support that birds and silhouettes are present in the browser demo.
15. Hygiene passes: `git diff --check`, high-confidence secret scan, no Unity implementation diff staged, and no video capture artifact staged.
16. Tracking stays aligned: this story, `sprint-status.yaml`, `_bmad-output/linear-sync.md`, and Linear MYB-20 are updated with implementation and review proof.

## Implementation Tasks

- [x] Preflight and protect scope. (AC: 1, 2, 6, 8, 15)
  - [x] Check `git status --short` and identify unrelated Unity/WebGL leftovers.
  - [x] Confirm no intended write under `unity/Echappee3D/**`.
  - [x] Confirm no new Meshy call is needed before implementation.
  - [x] Confirm `src/ride/*` is not part of the intended change surface.

- [x] Inspect the current Three.js scene-life opportunity. (AC: 3, 4, 5, 9, 11)
  - [x] Read the renderer entrypoints and existing scenic visual helpers.
  - [x] Identify where route-side anchors can be placed deterministically.
  - [x] Verify the camera/capture framing likely shows near-road elements.

- [x] Evaluate existing Meshy MYB-14 assets. (AC: 2, 7, 8)
  - [x] Inspect the existing FBX sizes and README.
  - [x] Decide whether direct runtime FBX loading is worth the added web complexity.
  - [x] Document the decision in this story and Linear implementation comment.

- [x] Add lightweight Three.js birds and human silhouettes. (AC: 3, 4, 5, 6, 9, 11)
  - [x] Prefer deterministic procedural primitives unless the FBX path is clearly simpler and stable.
  - [x] Place birds in the forward sky/near horizon with enough contrast to read through fog.
  - [x] Place human silhouettes on the roadside, off the player lane and away from the road center.
  - [x] Keep materials simple and lightweight.
  - [x] Avoid new animation, flocking, traffic or collision systems.

- [x] Add or update tests. (AC: 3, 5, 6, 12)
  - [x] Cover expected bird/human counts.
  - [x] Cover off-road or broad placement constraints.
  - [x] Cover that scene-life materials/objects are deterministic enough for validation.

- [x] Validate and capture. (AC: 10, 13, 14, 15)
  - [x] Run `npm run typecheck`.
  - [x] Run `npm run test`.
  - [x] Run `npm run build`.
  - [x] Run `npm run validate:private-demo`.
  - [x] Review the generated contact sheet or screenshots.
  - [x] Verify no page errors, no console errors and canvas nonblank.
  - [x] Run `git diff --check`.
  - [x] Run a high-confidence secret scan.

- [x] Update tracking after implementation. (AC: 16)
  - [x] Move MYB-20 to review in this story and `sprint-status.yaml`.
  - [x] Add implementation evidence to `_bmad-output/linear-sync.md`.
  - [x] Add a Linear implementation comment and set MYB-20 to In Review.

## Probable Files

Likely implementation files:

- `src/render/createBiomeVisuals.ts`
- `src/render/createBiomeVisuals.test.ts`
- `src/render/SceneController.ts` only if needed for wiring.
- `src/render/sceneTypes.ts` only if needed for typed object names or config.

Likely validation/tracking files:

- `_bmad-output/implementation-artifacts/myb-20-threejs-lightweight-scene-life-parity.md`
- `_bmad-output/implementation-artifacts/sprint-status.yaml`
- `_bmad-output/linear-sync.md`
- `_bmad-output/web-test-results/myb-19-private-demo-readiness.txt`
- `_bmad-output/video-captures/<new-capture-dir>/` as evidence only, not staged.

Reference-only files:

- `unity/Echappee3D/Assets/Generated/Meshy/MYB14/README.md`
- `unity/Echappee3D/Assets/Generated/Meshy/MYB14/meshy_bird_preview.fbx`
- `unity/Echappee3D/Assets/Generated/Meshy/MYB14/meshy_human_silhouette_preview.fbx`

Forbidden implementation files:

- `unity/Echappee3D/**`
- `src/ride/*` unless a strict, test-backed correction is unavoidable
- backend/deployment files
- BLE/FTMS integration files

## Developer Notes

### Current Web State

- MYB-18 made the route read as grounded with route-following verge bands and contact-shadow strips.
- MYB-18 adjusted the camera defaults to reduce floatiness.
- MYB-19 added `npm run validate:private-demo` and `_bmad-output/web-test-results/myb-19-private-demo-readiness.txt`.
- MYB-19 proof shows HTTP 200, canvas nonblank, no page errors and no console messages.
- The Three.js demo currently lacks the Unity MYB-14/MYB-16 birds and human silhouettes.

### Meshy Decision Guidance

The existing FBX previews are relatively large for a private web demo reference path:

- Bird preview: about 5.0 MB.
- Human silhouette preview: about 8.2 MB.

Direct runtime FBX loading may add loader complexity, async asset lifecycle risk and bundle/static-asset overhead. A procedural primitive implementation is acceptable and likely preferred if it gives clearer, more stable browser proof. The important requirement is to evaluate and document the decision, not to force FBX loading into a lightweight parity story.

### Suggested Visual Shape

- Birds: small stylized V or low-poly wing groups above/near the road horizon, with deterministic positions and enough contrast through fog.
- Humans: simple static silhouette groups with body/head primitives on one or both roadsides, scaled to read at ride speed without blocking the road.
- Keep counts low and stable: 5 birds, 3 humans.
- Keep all elements outside the route center and outside the player's lane.

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

- Do not create MYB-21 or a broad backlog.
- Do not delete or edit Unity.
- Do not make new Meshy calls without explicit cost confirmation.
- Do not add vehicles, traffic, AI, physics cycling, collision gameplay, BLE/FTMS, backend, public deployment, auth or cloud sync.
- Do not turn this into a full art pass. This is lightweight demo parity only.

## Definition of Done

- Three.js browser demo shows lightweight birds and human silhouettes.
- MYB-19 readiness remains `ready-for-private-web-demo`.
- Required npm validations pass.
- 60s capture evidence exists and is reviewed.
- Meshy MYB-14 reuse decision is documented.
- No Unity implementation edit is made.
- Linear, story file, `sprint-status.yaml`, and `_bmad-output/linear-sync.md` are aligned.

## Dev Agent Record

### Status

`done`

### Agent Model Used

Codex GPT-5

### Debug Log References

- Story/tracking creation only: npm validations intentionally not run.
- Story/tracking commit: `55505a6e46cef808704ccf2759da3485a0f7c2c5`.
- Existing Meshy references evaluated from `unity/Echappee3D/Assets/Generated/Meshy/MYB14/README.md`.
- Targeted validation: `npx vitest run src/render/createBiomeVisuals.test.ts`.
- Full validation: `npm run typecheck`, `npm run test`, `npm run build`, `npm run validate:private-demo`.
- First 60s capture proved birds clearly but human silhouettes were too subtle; adjusted human placement, scale and contrast within scope.
- Final readiness report: `_bmad-output/web-test-results/myb-19-private-demo-readiness.txt`.
- Implementation capture: `_bmad-output/video-captures/ride-visual-audit-2026-06-08T04-18-08-245Z/`.
- Review validation: `npm run validate:private-demo`.
- Review capture: `_bmad-output/video-captures/ride-visual-audit-2026-06-08T04-22-32-862Z/`.

### Completion Notes

- Added deterministic procedural scene life in the Three.js renderer: 5 birds and 3 static human silhouettes.
- Kept existing MYB-14 Meshy FBX previews as references only: direct FBX runtime loading was not selected because the files are comparatively heavy for the private web demo and would add loader/async risk.
- Used no new Meshy call and consumed no new Meshy credits.
- Preserved MYB-18 route grounding, camera framing, fog/depth, HUD, controls and mock loop.
- The final contact sheet shows birds and at least one readable human silhouette on the roadside without occluding the route.
- MYB-19 readiness remains `ready-for-private-web-demo` with HTTP 200, canvas nonblank, no page errors and no console messages.

### File List

- `_bmad-output/implementation-artifacts/myb-20-threejs-lightweight-scene-life-parity.md`
- `_bmad-output/implementation-artifacts/sprint-status.yaml`
- `_bmad-output/linear-sync.md`
- `src/render/createBiomeVisuals.ts`
- `src/render/createBiomeVisuals.test.ts`
- `_bmad-output/web-test-results/myb-19-private-demo-readiness.txt`
- `_bmad-output/video-captures/ride-visual-audit-2026-06-08T04-18-08-245Z/` (implementation evidence, not staged)
- `_bmad-output/video-captures/ride-visual-audit-2026-06-08T04-22-32-862Z/` (review evidence, not staged)

### Change Log

- 2026-06-08: Created MYB-20 story for Three.js lightweight scene-life parity.
- 2026-06-08: Implemented lightweight Three.js scene-life parity and moved MYB-20 to review.
- 2026-06-08: Review approved MYB-20 with no code corrections.

## Senior Developer Review (AI)

### Verdict

Approved on 2026-06-08 with no code corrections.

### Findings

- None. The implementation satisfies the MYB-20 acceptance criteria and scope guardrails.

### Review Evidence

- `npm run validate:private-demo`: pass.
- Harness executed `npm run typecheck`: pass.
- Harness executed `npm run test`: pass, 23 files / 77 tests.
- Harness executed `npm run build`: pass, Vite chunk-size warning only.
- Harness executed `npm run capture:ride-video`: pass, 60s capture.
- Review report: `_bmad-output/web-test-results/myb-19-private-demo-readiness.txt`.
- Review verdict: `ready-for-private-web-demo`.
- Review capture: `_bmad-output/video-captures/ride-visual-audit-2026-06-08T04-22-32-862Z/`.
- Review contact sheet shows birds and a readable roadside human silhouette.
- HTTP 200, `canvasNonBlank: true`, `pageErrors: []`, `consoleMessages: []`.

### Scope Checks

- No `src/ride/*` edits.
- No `unity/Echappee3D/**` implementation edits.
- No new Meshy call and no new Meshy credit consumed.
- No vehicles, traffic, AI, collision gameplay, backend, BLE/FTMS, public deployment or engine migration.
- Capture artifacts remain evidence-only and are not staged.

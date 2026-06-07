# Story MYB-19: Three.js private demo launch readiness

## Metadata

- Linear issue: `MYB-19`
- Linear URL: https://linear.app/kefjbo/issue/MYB-19/threejs-private-demo-launch-readiness
- Local status: `ready-for-dev`
- Linear status: `In Progress`
- Created: `2026-06-08`
- Linear sync comment: `efa4e6ae-00f7-44a8-955f-386e2e8bce57`
- Baseline commit: `0611461c3a230394abb976e17681105fac99289f`
- Depends on: `MYB-18`
- Source capture: `_bmad-output/video-captures/ride-visual-audit-2026-06-07T20-45-29-441Z/`
- Expected readiness report: `_bmad-output/web-test-results/myb-19-private-demo-readiness.txt`
- Active target: React/Vite/TypeScript/Three.js web demo
- Unity status: parking/research reference only, no edit

## Story

As a private demo owner,
I want the recovered Three.js vertical slice to have a repeatable local launch checklist and readiness report,
so that I can confidently show the demo from a browser without re-opening the Unity migration track.

## Context

MYB-18 restored Three.js as the active private-demo target after the MYB-17 Unity WebGL demo felt technically functional but visually floaty. The reviewed MYB-18 capture proved a grounded road, tighter camera framing, preserved fog/depth, HTTP 200, no page errors, no console messages and a nonblank canvas over a 60s capture.

MYB-19 should not add another visual polish pass. Its job is to package the current web vertical slice as a private local demo that can be launched, validated and shown with a short checklist and a deterministic readiness report.

Relevant MYB-18 proof:

- Commit: `0611461c3a230394abb976e17681105fac99289f`.
- Review comment: `852213dc-d8c0-4b03-89b3-792685178f3b`.
- Review capture: `_bmad-output/video-captures/ride-visual-audit-2026-06-07T20-45-29-441Z/`.
- Capture metrics: HTTP 200, `consoleMessages: []`, `pageErrors: []`, `durationMs: 60000`, `frameCount: 12`, final frame `YMIN=30`, `YAVG=128.639`, `YMAX=245`.

## Launch Readiness Target

MYB-19 should answer one practical question:

```text
Can Julien start the local Three.js demo, verify it quickly, and show it privately with confidence?
```

The expected implementation output is:

- a local readiness command or script if useful;
- a report at `_bmad-output/web-test-results/myb-19-private-demo-readiness.txt`;
- a short launch checklist in that report;
- a 60s capture proof from the browser;
- no Unity changes and no public deployment.

## Acceptance Criteria

1. Web-only scope is respected: implementation changes stay in React/Vite/Three.js, local scripts, tests, BMAD/tracking and web validation evidence. No `unity/Echappee3D/**` change is allowed.
2. Three.js remains the private-demo target. Unity is not deleted, edited, rebuilt, or used as acceptance proof for MYB-19.
3. MYB-18 ride feel is preserved: grounded route cues, camera framing, fog/depth, HUD, controls and mock session loop remain usable.
4. No new gameplay, visual polish pass, vehicle/traffic/AI, Meshy, external asset, backend, BLE/FTMS, public deployment or engine migration work is introduced.
5. A local readiness report is generated at `_bmad-output/web-test-results/myb-19-private-demo-readiness.txt`.
6. The report confirms project root, baseline commit, generated timestamp, target URL, build mode and local launch command used.
7. The report confirms `npm run typecheck`, `npm run test`, and `npm run build` results.
8. The report confirms a local Vite server responds with HTTP 200.
9. The report confirms browser health: canvas nonblank, no page errors, no console errors.
10. The report confirms a 60s capture exists and references its directory, video, contact sheet and capture summary.
11. The report includes a short launch checklist suitable for a private demo operator.
12. The report includes the final verdict string `ready-for-private-web-demo` only if all required checks pass.
13. If any check fails, the report must use a non-ready verdict and include factual next actions instead of hiding the failure.
14. Existing validation stays green: `npm run typecheck`, `npm run test`, `npm run build`.
15. Capture validation stays green: `npm run capture:ride-video` or the readiness script's equivalent 60s capture flow.
16. No `src/ride/*` changes are made unless a small, test-backed correction is strictly necessary and documented.
17. Tracking stays aligned: this story, `sprint-status.yaml`, `_bmad-output/linear-sync.md`, and Linear MYB-19 are updated with implementation proof after `gds-dev-story`.

## Implementation Tasks

- [ ] Preflight and protect scope. (AC: 1, 2, 4, 16)
  - [ ] Check `git status --short` and identify unrelated existing Unity/WebGL artifacts before editing.
  - [ ] Confirm no intended edit under `unity/Echappee3D/**`.
  - [ ] Confirm `src/ride/*` is not part of the intended change surface.
  - [ ] Keep this story focused on launch readiness, not new polish.

- [ ] Inspect the current web demo and capture tooling. (AC: 3, 5, 8, 9, 10)
  - [ ] Read `package.json` scripts.
  - [ ] Read `scripts/capture-ride-video.mjs`.
  - [ ] Read current capture summary format from the MYB-18 review capture.
  - [ ] Read the app/render entrypoints only as needed to verify canvas and page health signals.

- [ ] Add or extend a lightweight readiness harness. (AC: 5, 6, 7, 8, 9, 10, 12, 13)
  - [ ] Prefer a simple Node script under `scripts/` if the existing capture script does not already provide enough report data.
  - [ ] Generate `_bmad-output/web-test-results/myb-19-private-demo-readiness.txt`.
  - [ ] Include explicit pass/fail lines for typecheck, tests, build, HTTP 200, browser errors, console errors, canvas nonblank and capture evidence.
  - [ ] Make the verdict deterministic: `ready-for-private-web-demo` only when all required checks pass.
  - [ ] Do not require public hosting or deployment.

- [ ] Add a concise launch checklist. (AC: 6, 11, 12, 13)
  - [ ] Include install/build assumption if relevant.
  - [ ] Include the local command to start or preview the demo.
  - [ ] Include the URL to open.
  - [ ] Include the minimal operator path: start ride, adjust mock effort, pause/resume, finish, check summary.
  - [ ] Include what to do if the report is not ready.

- [ ] Preserve the existing demo behavior. (AC: 3, 4, 14, 15, 16)
  - [ ] Avoid rendering or camera edits unless a readiness blocker proves a tiny correction is necessary.
  - [ ] Preserve MYB-18 grounded route cues and camera defaults.
  - [ ] Preserve fog/depth and the capture-clean-HUD flow.
  - [ ] Keep all tests deterministic.

- [ ] Validate and record proof. (AC: 7, 8, 9, 10, 14, 15)
  - [ ] Run `npm run typecheck`.
  - [ ] Run `npm run test`.
  - [ ] Run `npm run build`.
  - [ ] Run the MYB-19 readiness command or script.
  - [ ] Run `npm run capture:ride-video` if not already covered by the readiness command.
  - [ ] Verify the readiness report includes the final verdict and links to the capture artifacts.

- [ ] Update tracking after implementation. (AC: 17)
  - [ ] Move MYB-19 to review in this story and `sprint-status.yaml`.
  - [ ] Add validation evidence to `_bmad-output/linear-sync.md`.
  - [ ] Add a Linear implementation comment and set MYB-19 to In Review.

## Probable Files

Likely implementation files:

- `package.json` only if adding a `validate:private-demo` style script.
- `scripts/capture-ride-video.mjs` only if existing capture metadata needs a small extension.
- `scripts/validate-private-demo-readiness.mjs` or similar new script if a separate readiness report is cleaner.
- `src/render/**` only if a tiny readiness-blocking fix is proven necessary.
- `src/app/**` only if a tiny launch/readiness blocker is proven necessary.
- Tests beside any touched script/helper if logic is nontrivial.

Tracking/evidence files:

- `_bmad-output/implementation-artifacts/myb-19-threejs-private-demo-launch-readiness.md`
- `_bmad-output/implementation-artifacts/sprint-status.yaml`
- `_bmad-output/linear-sync.md`
- `_bmad-output/web-test-results/myb-19-private-demo-readiness.txt`
- `_bmad-output/video-captures/<new-capture-dir>/` (validation evidence, do not stage unless explicitly requested)

Forbidden for this story:

- `unity/Echappee3D/**`
- `src/ride/*` unless a strict, test-backed correction is unavoidable
- Meshy-generated assets
- external images/textures/models
- backend/deployment files
- BLE/FTMS integration files

## Developer Notes

### Current Web State

- MYB-18 made the route read as grounded by adding route-following verges and contact-shadow strips in `src/render/createRouteMesh.ts`.
- MYB-18 lowered/tightened default camera framing in `src/route/cameraOnRail.ts`: `heightMeters=1.25`, `lookAheadMeters=18`, `fovDegrees=66`.
- `scripts/capture-ride-video.mjs` already produces a 60s capture by default with video, contact sheet, final frame and capture summary.
- `index.html` has an inline favicon to avoid favicon 404 noise during browser capture.
- Existing tests after MYB-18: 22 files / 73 tests.

### Architecture Constraints

- React owns UI and screen state.
- Three.js owns rendering from snapshots.
- Ride logic remains pure and testable outside React and Three.js.
- `src/ride/*` should remain untouched for launch readiness unless a strict bug is found.
- Capture and readiness evidence should be local and repeatable; public deployment is explicitly out of scope.

### Readiness Report Suggested Shape

The report should be plain text and easy to read in the terminal or Linear comment. Suggested sections:

```text
MYB-19 Private Web Demo Readiness
Generated: <timestamp>
Project root: <path>
Baseline commit: <sha>
Target URL: <url>

Checks
- typecheck: pass|fail
- test: pass|fail
- build: pass|fail
- HTTP 200: pass|fail
- canvas nonblank: pass|fail
- page errors: pass|fail
- console errors: pass|fail
- 60s capture: pass|fail

Capture Evidence
- directory: <path>
- video: <path>
- contact sheet: <path>
- summary: <path>

Launch Checklist
1. <short operator step>

Verdict: ready-for-private-web-demo|not-ready
```

## Validation Plan

Implementation must run:

```bash
npm run typecheck
npm run test
npm run build
npm run capture:ride-video
```

If MYB-19 adds a dedicated readiness script, run it as well and document the command in the report.

Final hygiene:

```bash
git diff --check
git diff --name-only -- unity/Echappee3D src/ride
git diff | <high-confidence secret scan>
```

Do not run Unity MCP or Unity batchmode for MYB-19. Unity is not acceptance proof for this web-only story.

## Guardrails

- Do not create MYB-20 or a new backlog in this story.
- Do not turn readiness into another visual polish pass.
- Do not delete or edit Unity.
- Do not use Meshy or any paid generation tool.
- Do not add vehicles, traffic, AI, physics cycling, collision gameplay, BLE/FTMS, backend, public deployment, auth or cloud sync.
- Do not claim readiness without a report and real browser/capture evidence.

## Definition of Done

- MYB-19 has a generated private web demo readiness report.
- The report contains `ready-for-private-web-demo` only when required checks pass.
- The report includes a short launch checklist.
- A local browser/capture proof exists and is referenced.
- `npm run typecheck`, `npm run test`, `npm run build`, and capture validation pass.
- No `unity/Echappee3D/**` change is made.
- No Meshy, external asset, backend, BLE/FTMS, deployment or new migration work is added.
- Linear, story file, `sprint-status.yaml`, and `_bmad-output/linear-sync.md` are aligned.

## Dev Agent Record

### Status

`ready-for-dev`

### Agent Model Used

TBD

### Debug Log References

- Story/tracking creation only: npm validations intentionally not run.

### Completion Notes

TBD during implementation.

### File List

TBD during implementation.

### Change Log

- 2026-06-08: Created MYB-19 story for private Three.js demo launch readiness.

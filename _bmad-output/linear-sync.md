# Linear Sync - mybike / Echappee 3D

Last sync: 2026-06-06

## Workspace

- Team: `MYB` / `MyBike`
- Project: `Echappee 3D - Vertical Slice Mock`
- Project URL: https://linear.app/kefjbo/project/echappee-3d-vertical-slice-mock-5187315b9c6d
- Project ID: `7abb762d-4eb3-4089-af1f-0b973eeadd65`

## Source Artifacts

- GDD: `_bmad-output/planning-artifacts/echappee-3d-gdd-court.md`
- Architecture: `_bmad-output/planning-artifacts/echappee-3d-architecture-mince.md`
- Epic and stories: `_bmad-output/planning-artifacts/echappee-3d-mvp-epic-stories.md`
- Epic 1 retrospective: `_bmad-output/implementation-artifacts/epic-1-retro-2026-06-06.md`
- Polish UX playtest plan: `_bmad-output/implementation-artifacts/playtest-polish-ux-ride-feel-2026-06-06.md`
- Polish UX playtest results: `_bmad-output/implementation-artifacts/playtest-polish-ux-ride-feel-results-2026-06-06.md`
- Polish UX mini-scope: `_bmad-output/implementation-artifacts/polish-ux-ride-feel-mini-scope-2026-06-06.md`
- Polish UX change proposal: `_bmad-output/planning-artifacts/sprint-change-proposal-polish-ux-ride-feel-2026-06-06.md`

## Linear Documents

- GDD court: https://linear.app/kefjbo/document/gdd-court-echappee-3d-13a4887d3166
- Architecture mince: https://linear.app/kefjbo/document/architecture-mince-echappee-3d-53b40d669c6f
- Mini-backlog BMAD: https://linear.app/kefjbo/document/mini-backlog-bmad-epic-et-stories-mvp-6806d059d836

## Linear Issues

| Local source | Linear | Title | URL |
|---|---|---|---|
| Epic MVP 1 | `MYB-1` | EPIC 1 - Boucle de ride mock jouable | https://linear.app/kefjbo/issue/MYB-1/epic-1-boucle-de-ride-mock-jouable |
| Story 1.1 | `MYB-2` | App shell MVP et etats de session | https://linear.app/kefjbo/issue/MYB-2/story-11-app-shell-mvp-et-etats-de-session |
| Story 1.2 | `MYB-3` | Source mock, vitesse lissee, progression et stats | https://linear.app/kefjbo/issue/MYB-3/story-12-source-mock-vitesse-lissee-progression-et-stats |
| Story 1.3 | `MYB-4` | Route prefabriquee, biomes et camera sur rail | https://linear.app/kefjbo/issue/MYB-4/story-13-route-prefabriquee-biomes-et-camera-sur-rail |
| Story 1.4 | `MYB-5` | Scene Three.js MVP et rendu de la balade | https://linear.app/kefjbo/issue/MYB-5/story-14-scene-threejs-mvp-et-rendu-de-la-balade |
| Story 1.5 | `MYB-6` | HUD, controles mock, resume et happy path | https://linear.app/kefjbo/issue/MYB-6/story-15-hud-controles-mock-resume-et-happy-path |

## Dependency Mapping

- `MYB-3` is blocked by `MYB-2`.
- `MYB-4` is blocked by `MYB-3`.
- `MYB-5` is blocked by `MYB-2`, `MYB-3`, and `MYB-4`.
- `MYB-6` is blocked by `MYB-2`, `MYB-3`, and `MYB-5`.

## Readiness Caveats Synced

Source: `_bmad-output/planning-artifacts/implementation-readiness-report-2026-06-06.md`

Synced on 2026-06-06:

- `MYB-4`: clarified route absent/invalid fallback. The route module must either
  return a deterministic placeholder route or signal a typed `error` state that
  the app can use, without runtime crashes or invalid numeric values.
- `MYB-5`: clarified performance validation as pragmatic manual validation only:
  no visible freeze or obvious fluidity drop during a short mock ride, without
  adding profiler/tooling scope.
- `MYB-6`: clarified mock slider as mandatory and keyboard shortcuts as optional;
  any Playwright happy path should use the slider as the primary mock control.

## Implementation Status Synced

Synced on 2026-06-06:

- `MYB-2`: moved to Linear status `In Review` after local BMAD status moved to
  review in `_bmad-output/implementation-artifacts/1-1-app-shell-mvp-et-etats-de-session.md`
  and `_bmad-output/implementation-artifacts/sprint-status.yaml`.
- `MYB-2` validation evidence recorded in Linear:
  - `npm run typecheck`
  - `npm run test` with 12 tests
  - `npm run build`
- `MYB-2` implementation summary recorded in Linear: Vite/React/TypeScript shell,
  `StartScreen`, `RideScreen`, `SummaryScreen`, `WebGlFallback`, pure session
  reducer, testable WebGL helper, and jsdom unit/flow tests.

Synced on 2026-06-06:

- `MYB-2`: moved from Linear status `In Review` to `Done` after local
  `gds-code-review` returned `approved` with no findings.
- Final `MYB-2` validation evidence recorded in Linear:
  - `npm run typecheck`
  - `npm run test` with 12 tests
  - `npm run build`
  - HTTP check on `http://127.0.0.1:5174/` returned 200.
- `MYB-2` scope confirmation recorded in Linear: no full Three.js scene, route,
  ride input, final HUD, Playwright, persistence, BLE/FTMS, or Meshy work.

Synced on 2026-06-06:

- `MYB-3`: moved to Linear status `In Progress` after BMAD created the dev story
  `_bmad-output/implementation-artifacts/1-2-source-mock-vitesse-lissee-progression-et-stats.md`
  and updated `_bmad-output/implementation-artifacts/sprint-status.yaml` with
  `next_action: implement-MYB-3`.

Synced on 2026-06-06:

- `MYB-3`: moved from Linear status `In Progress` to `In Review` after local
  BMAD status moved to review in
  `_bmad-output/implementation-artifacts/1-2-source-mock-vitesse-lissee-progression-et-stats.md`
  and `_bmad-output/implementation-artifacts/sprint-status.yaml`.
- `MYB-3` validation evidence recorded in Linear:
  - `npm run typecheck`
  - `npm run test` with 26 tests across 8 files
  - `npm run build`
- `MYB-3` implementation summary recorded in Linear: pure `src/ride/*` API for
  mock input, effort clamp, speed mapping, smoothing, numeric progress, ride
  stats, frame snapshots, and deterministic unit tests.
- `MYB-3` scope confirmation recorded in Linear: no route/camera/biome,
  Three.js/render, HUD/slider UI, Playwright, persistence, BLE/Web
  Bluetooth/FTMS, Meshy, or asset-pipeline work.

Synced on 2026-06-06:

- `MYB-3`: moved from Linear status `In Review` to `Done` after local
  `gds-code-review` returned `approved` with no findings.
- Final `MYB-3` validation evidence recorded in Linear:
  - `npm run typecheck`
  - `npm run test` with 26 tests across 8 files
  - `npm run build`
- `MYB-3` review notes recorded in Linear: clamp `effort01`, speed mapping,
  smoothing without overshoot, numeric progress, stats and frame snapshots all
  conform to the story scope.
- `MYB-3` local BMAD status recorded as `done` in
  `_bmad-output/implementation-artifacts/1-2-source-mock-vitesse-lissee-progression-et-stats.md`
  and `_bmad-output/implementation-artifacts/sprint-status.yaml`; next local
  action is `commit-MYB-3`.

Synced on 2026-06-06:

- `MYB-4`: moved from Linear status `Backlog` to `In Progress` after BMAD
  created the dev story
  `_bmad-output/implementation-artifacts/1-3-route-prefabriquee-biomes-et-camera-sur-rail.md`
  and updated `_bmad-output/implementation-artifacts/sprint-status.yaml` with
  `1-3-route-prefabriquee-biomes-et-camera-sur-rail: ready-for-dev` and
  `next_action: implement-MYB-4`.
- `MYB-4` Linear sync evidence:
  - URL: https://linear.app/kefjbo/issue/MYB-4/story-13-route-prefabriquee-biomes-et-camera-sur-rail
  - Status returned by Linear: `In Progress`
  - Comment ID: `7c68c142-aea9-4226-bdf5-26809b9192f2`
- `MYB-4` scope confirmation recorded in Linear: route prefabriquee minimale,
  route sampling, biomes, camera sur rail; consumes MYB-3 progression/snapshot
  without changing speed, smoothing, stats, or session logic except small
  type/export adaptations if needed.
- `MYB-4` caveat confirmation recorded in Linear: route absent/invalid must
  return a deterministic placeholder or typed error, with no crash and no
  invalid numeric values.
- `MYB-4` exclusions recorded in Linear: no full Three.js scene, final rendering,
  HUD, slider, Playwright, BLE/Web Bluetooth/FTMS, persistence, backend, Meshy,
  or asset pipeline.

Synced on 2026-06-06:

- `MYB-4`: moved from Linear status `In Progress` to `In Review` after local
  BMAD status moved to review in
  `_bmad-output/implementation-artifacts/1-3-route-prefabriquee-biomes-et-camera-sur-rail.md`
  and `_bmad-output/implementation-artifacts/sprint-status.yaml` with
  `next_action: review-MYB-4`.
- `MYB-4` Linear sync evidence:
  - URL: https://linear.app/kefjbo/issue/MYB-4/story-13-route-prefabriquee-biomes-et-camera-sur-rail
  - Status returned by Linear: `In Review`
  - Comment ID: `400269eb-fc2f-4461-9186-524532b04f09`
- `MYB-4` validation evidence recorded in Linear:
  - `npm run typecheck`
  - `npm run test` with 40 tests across 13 files
  - `npm run build`
- `MYB-4` implementation summary recorded in Linear: pure `src/route/*` API for
  mock route definition, deterministic sampling, biome selection, camera on rail,
  typed route resolution and placeholder fallback for absent/invalid routes.
- `MYB-4` scope confirmation recorded in Linear: no full Three.js scene, final
  rendering, mesh, lighting, fog, assets, HUD, slider, mock controls, Playwright,
  BLE/Web Bluetooth/FTMS, persistence, backend, Meshy, or asset pipeline.

Synced on 2026-06-06:

- `MYB-4`: moved from Linear status `In Review` to `Done` after local
  `gds-code-review` returned `approved` following review patches.
- `MYB-4` Linear sync evidence:
  - URL: https://linear.app/kefjbo/issue/MYB-4/story-13-route-prefabriquee-biomes-et-camera-sur-rail
  - Status returned by Linear: `Done`
  - Comment ID: `2dc652d4-5636-4b91-a76a-49b8a9d6cae3`
- Final `MYB-4` validation evidence recorded in Linear:
  - `npm run typecheck`
  - `npm run test` with 43 tests across 13 files
  - `npm run build`
- `MYB-4` review fixes recorded in Linear:
  - malformed `points` / `biomes` entries now resolve to `placeholder-route`
    instead of throwing;
  - invalid biome entries are rejected during validation instead of being
    selectable later;
  - `null` camera config now uses deterministic defaults.
- `MYB-4` local BMAD status recorded as `done` in
  `_bmad-output/implementation-artifacts/1-3-route-prefabriquee-biomes-et-camera-sur-rail.md`
  and `_bmad-output/implementation-artifacts/sprint-status.yaml`; next local
  action is `commit-MYB-4`.
- `MYB-4` final scope confirmation recorded in Linear: pure `src/route/*`, no
  `src/ride/*` changes, no React/Three.js/render/UI/e2e imports, no full scene,
  HUD, slider, Playwright, persistence, BLE/Web Bluetooth/FTMS, backend, Meshy,
  or asset pipeline.

Synced on 2026-06-06:

- `MYB-5`: moved from Linear status `Backlog` to `In Progress` after BMAD
  created the dev story
  `_bmad-output/implementation-artifacts/1-4-scene-three-js-mvp-et-rendu-de-la-balade.md`
  and updated `_bmad-output/implementation-artifacts/sprint-status.yaml` with
  `1-4-scene-three-js-mvp-et-rendu-de-la-balade: ready-for-dev` and
  `next_action: implement-MYB-5`.
- `MYB-5` Linear sync evidence:
  - URL: https://linear.app/kefjbo/issue/MYB-5/story-14-scene-threejs-mvp-et-rendu-de-la-balade
  - Status returned by Linear: `In Progress`
  - Comment ID: `936248b4-5793-4fb0-b227-f18fe508e8f7`
- `MYB-5` scope confirmation recorded in Linear: Three.js MVP scene,
  `ThreeCanvasHost`, minimal `SceneController`, readable ride rendering from
  MYB-3/MYB-4 snapshots, simple ground/route/horizon/biome placeholders, and
  camera fed by the route snapshot.
- `MYB-5` architecture confirmation recorded in Linear: rendering consumes
  `src/ride` and `src/route` outputs without recalculating ride or route logic;
  `src/ride/*` and `src/route/*` remain pure/testable and without React or
  Three.js dependencies.
- `MYB-5` performance caveat recorded in Linear: pragmatic manual validation
  only, with no profiler or advanced performance tooling scope.
- `MYB-5` exclusions recorded in Linear: no final HUD, mock slider or ride UI
  controls, Playwright happy path, persistence/backend, BLE/Web Bluetooth/FTMS,
  AI/Meshy assets or heavy asset pipeline, and no MYB-6 anticipation beyond
  necessary integration points.
- `MYB-5` validation note recorded in Linear: `npm run typecheck`,
  `npm run test`, and `npm run build` were not run because only BMAD artifacts
  changed, with no application code modification.

Synced on 2026-06-06:

- `MYB-5`: moved from Linear status `In Progress` to `In Review` after local
  BMAD status moved to review in
  `_bmad-output/implementation-artifacts/1-4-scene-three-js-mvp-et-rendu-de-la-balade.md`
  and `_bmad-output/implementation-artifacts/sprint-status.yaml` with
  `next_action: review-MYB-5`.
- `MYB-5` Linear sync evidence:
  - URL: https://linear.app/kefjbo/issue/MYB-5/story-14-scene-threejs-mvp-et-rendu-de-la-balade
  - Status returned by Linear: `In Review`
  - Comment ID: `408f48a8-e41e-4e2e-90cb-e6d86df5d268`
- `MYB-5` validation evidence recorded in Linear:
  - `npm run typecheck`
  - `npm run test` with 48 tests across 15 files
  - `npm run build`
  - HTTP check on `http://127.0.0.1:5174/` returned 200
  - Chrome headless/CDP visual validation captured non-blank desktop and mobile
    canvas views after launching the mock ride.
- `MYB-5` implementation summary recorded in Linear: direct Three.js render
  module under `src/render/*`, `ThreeCanvasHost`, `createSceneController`,
  route ribbon, ground/sky placeholders, biome markers, fallback palette, and
  camera fed by `createRouteFrameSnapshot`.
- `MYB-5` performance caveat recorded in Linear: pragmatic qualitative
  validation only, with no profiler, stats.js, WebGL inspector, draw-call budget,
  or advanced performance tooling.
- `MYB-5` scope confirmation recorded in Linear: no HUD final, mock slider,
  ride UI controls, Playwright happy path, persistence/backend, BLE/Web
  Bluetooth/FTMS, Meshy, AI assets, loaders or heavy asset pipeline; no changes
  to `src/ride/*` or `src/route/*`.

Synced on 2026-06-06:

- `MYB-5`: moved from Linear status `In Review` to `Done` after local
  `gds-code-review` returned `approved` following review patches.
- `MYB-5` Linear sync evidence:
  - URL: https://linear.app/kefjbo/issue/MYB-5/story-14-scene-threejs-mvp-et-rendu-de-la-balade
  - Status returned by Linear: `Done`
  - Comment ID: `4c53834e-7351-4da7-a9d3-4396141ca592`
- Final `MYB-5` validation evidence recorded in Linear:
  - `npm run typecheck`
  - `npm run test` with 49 tests across 15 files
  - `npm run build` with the expected Vite warning for a Three.js chunk larger
    than 500 kB
  - HTTP check on `http://127.0.0.1:5174/` returned 200
  - Chrome headless/CDP desktop and mobile screenshots showed a framed,
    non-blank canvas with route, ground, sky and biome markers visible
  - ImageMagick crop checks counted 228 desktop canvas colors and 925 mobile
    canvas colors
- `MYB-5` review fixes recorded in Linear:
  - malformed routes reaching render now resolve to `placeholder-route` before
    route mesh point creation;
  - the canvas viewport now has a stable responsive height on desktop/mobile,
    and action buttons no longer stretch after the viewport is stabilized.
- `MYB-5` local BMAD status recorded as `done` in
  `_bmad-output/implementation-artifacts/1-4-scene-three-js-mvp-et-rendu-de-la-balade.md`
  and `_bmad-output/implementation-artifacts/sprint-status.yaml`; next local
  action is `commit-MYB-5`.
- `MYB-5` final scope confirmation recorded in Linear: Three.js remains limited
  to `src/render/*`, render consumes MYB-3/MYB-4 snapshots without recalculating
  ride/route logic, `src/ride/*` and `src/route/*` were not changed, and there
  is no HUD final, mock slider, ride controls UI, Playwright happy path,
  persistence/backend, BLE/Web Bluetooth/FTMS, Meshy, AI assets, loaders or
  heavy asset pipeline.

Synced on 2026-06-06:

- `MYB-6`: moved from Linear status `Backlog` to `In Progress` after BMAD
  created the dev story
  `_bmad-output/implementation-artifacts/1-5-hud-controles-mock-resume-et-happy-path.md`
  and updated `_bmad-output/implementation-artifacts/sprint-status.yaml` with
  `1-5-hud-controles-mock-resume-et-happy-path: ready-for-dev` and
  `next_action: implement-MYB-6`.
- `MYB-6` Linear sync evidence:
  - URL: https://linear.app/kefjbo/issue/MYB-6/story-15-hud-controles-mock-resume-et-happy-path
  - Status returned by Linear: `In Progress`
  - Comment ID: `9bc46f2d-d52e-4b79-96ce-5e34b32c732e`
- `MYB-6` scope confirmation recorded in Linear: HUD minimal, mandatory mock
  slider, optional keyboard only, final summary from existing ride stats, and a
  reasonable mock happy path from start through summary.
- `MYB-6` architecture confirmation recorded in Linear: one snapshot stream must
  feed render, HUD and summary; HUD consumes MYB-3 snapshots/stats without
  recalculating speed, smoothing, progress or stats; Three.js remains under
  `src/render/*`.
- `MYB-6` exclusions recorded in Linear: no BLE/Web Bluetooth/FTMS,
  backend/persistence, Meshy/AI assets, multiple routes, broad settings system,
  or deep refactor of MYB-2 through MYB-5.
- `MYB-6` validation note recorded in Linear: `npm run typecheck`,
  `npm run test`, and `npm run build` were not run because only BMAD/sync
  artifacts changed, with no application code modification.

Synced on 2026-06-06:

- `MYB-6`: moved from Linear status `In Progress` to `In Review` after local
  implementation of
  `_bmad-output/implementation-artifacts/1-5-hud-controles-mock-resume-et-happy-path.md`
  and sprint update
  `_bmad-output/implementation-artifacts/sprint-status.yaml`.
- `MYB-6` Linear sync evidence:
  - URL: https://linear.app/kefjbo/issue/MYB-6/story-15-hud-controles-mock-resume-et-happy-path
  - Status returned by Linear: `In Review`
  - Comment ID: `5954669c-1520-418d-9762-b53a2e8ac252`
- `MYB-6` local BMAD status recorded as `review`; sprint
  `next_action: review-MYB-6`.
- `MYB-6` implementation summary recorded in Linear: minimal ride HUD, mandatory
  effort slider `0..1`, shared `ThreeCanvasHost` snapshot stream through
  `onFrame(snapshot)`, real final summary from `RideStats`, and happy path
  start -> ride visible -> slider -> pause -> resume -> finish -> summary.
- `MYB-6` scope confirmation recorded in Linear: no BLE/Web Bluetooth/FTMS,
  backend/persistence, Meshy/AI assets, route selection, settings system, sports
  HUD, broad MYB-2..MYB-5 refactor, or changes to `src/ride/*`, `src/route/*`
  or `src/render/SceneController.ts`.
- `MYB-6` validation evidence recorded in Linear:
  - RED tests confirmed before implementation with expected MYB-6 failures
  - `npm run test`: 60 tests / 19 files
  - `npm run typecheck`
  - `npm run build` with expected Vite warning for Three.js chunk >500 kB
  - Vite HTTP 200 on `http://127.0.0.1:5174/` (`5173` was occupied)
  - Chrome headless/CDP desktop happy path with slider 55% -> 90%, pause/resume,
    real summary and canvas crop nonblank (`mean=0.595`, `stddev=0.117921`)
  - Chrome headless/CDP mobile 390x844 DPR 2 happy path with canvas crop
    nonblank (`mean=0.624303`, `stddev=0.097109`)
  - `test:e2e` not run because Playwright is not configured; coverage supplied
    by React integration tests and real Chrome CDP validation.

Synced on 2026-06-06:

- `MYB-6`: code review approved and Linear moved from `In Review` to `Done`.
- `MYB-6` Linear sync evidence:
  - URL: https://linear.app/kefjbo/issue/MYB-6/story-15-hud-controles-mock-resume-et-happy-path
  - Status returned by Linear: `Done`
  - Review comment ID: `6f582ac7-59c8-4972-a097-0743817c45f2`
- `MYB-6` local BMAD status recorded as `done` in
  `_bmad-output/implementation-artifacts/1-5-hud-controles-mock-resume-et-happy-path.md`
  and `_bmad-output/implementation-artifacts/sprint-status.yaml`; sprint
  `next_action: commit-MYB-6`.
- `MYB-6` review findings recorded in Linear:
  - fixed `ThreeCanvasHost` so caller-provided mock `createInputSource` values
    are not overwritten by default controlled effort unless `effort01` is
    explicitly supplied;
  - fixed pause timing so HUD and final summary exclude paused duration;
  - deferred a non-blocking pre-existing MYB-5 hardening gap where
    `WebGLRenderer` construction can fail after initial preflight; the MYB-2
    WebGL-unavailable fallback remains preserved and tested.
- `MYB-6` final validation evidence recorded in Linear:
  - `npm run typecheck`
  - `npm run test`: 61 tests / 19 files
  - `npm run build` with expected Vite warning for Three.js chunk >500 kB
  - Vite HTTP 200 on `http://127.0.0.1:5174/`
  - Chrome headless/CDP desktop happy path with slider to 90%, pause/resume,
    real summary and canvas crop nonblank (`mean=0.594989`,
    `stddev=0.117837`)
  - Chrome headless/CDP mobile 390x844 DPR 2 happy path with slider to 90%,
    pause/resume, real summary and canvas crop nonblank (`mean=0.624251`,
    `stddev=0.0969858`)
- `MYB-6` final scope confirmation recorded in Linear: no BLE/Web
  Bluetooth/FTMS, backend/persistence, Meshy/AI assets, route selection,
  settings system, sports HUD, large refactor, or changes to `src/ride/*`,
  `src/route/*`, or `src/render/SceneController.ts`.

Synced on 2026-06-06:

- `MYB-1`: moved from Linear status `Backlog` to `Done` after all MVP stories
  `MYB-2` through `MYB-6` were Done, reviewed, committed and synced.
- `MYB-1` closure sync evidence:
  - URL: https://linear.app/kefjbo/issue/MYB-1/epic-1-boucle-de-ride-mock-jouable
  - Status returned by Linear: `Done`
  - Closure comment ID: `be183c0b-bcd8-4a7d-b6db-4ffb46009bc2`
- Project closure note recorded on existing Linear project:
  - Project: `Echappee 3D - Vertical Slice Mock`
  - Project ID: `7abb762d-4eb3-4089-af1f-0b973eeadd65`
  - Project comment ID: `213e56b1-87ab-4d65-9d14-eef709177d5d`
  - No new Linear issues were created.
- Local BMAD status recorded as `done` for `epic-1` and
  `epic-1-retrospective` in
  `_bmad-output/implementation-artifacts/sprint-status.yaml`; sprint
  `next_action: choose-post-MVP-option`.
- Retrospective artifact recorded locally:
  `_bmad-output/implementation-artifacts/epic-1-retro-2026-06-06.md`.
- Final validation evidence summarized in Linear:
  - `npm run typecheck`
  - `npm run test`: 61 tests / 19 files
  - `npm run build` with expected Vite warning for Three.js chunk >500 kB
  - Vite HTTP 200 on `http://127.0.0.1:5174/`
  - Chrome headless/CDP desktop and mobile happy paths with slider, pause/resume,
    real summary and nonblank canvas crops
  - `test:e2e` not run because Playwright is not configured; coverage supplied
    by React/Vitest tests and Chrome CDP validation.
- Post-MVP options recorded without creating a broad backlog:
  hardening, mock UX playtest, visual/content polish, telemetry preparation, and
  performance/assets strategy.

Synced on 2026-06-06:

- Post-MVP direction decision recorded on the existing Linear project.
- Chosen direction: `polish UX / sensation de ride`.
- Decision rationale recorded in Linear:
  - validates the core playable mock loop before public deployment;
  - keeps work close to the shipped vertical slice without new hardware,
    backend, deployment or asset-pipeline dependencies;
  - reduces risk before BLE/FTMS by first qualifying camera comfort, route
    readability, slider feel, HUD readability and summary clarity;
  - treats the Three.js bundle warning and WebGL hardening as watch items rather
    than the primary next direction.
- Project decision note:
  - Project: `Echappee 3D - Vertical Slice Mock`
  - Project ID: `7abb762d-4eb3-4089-af1f-0b973eeadd65`
  - Project comment ID: `11c5ea30-e877-43f2-8510-8700aded99fd`
  - No new Linear issues were created.
- Local BMAD sprint status updated in
  `_bmad-output/implementation-artifacts/sprint-status.yaml`:
  - `next_action: define-polish-ux-ride-feel-mini-scope`
  - `post_mvp_direction: polish UX / sensation de ride`
  - `first_story_to_start: none`
- Proposed next mini-scope, without broad backlog creation: define a short mock
  playtest for desktop/mobile, evaluate camera/route/slider/HUD/summary, identify
  3 to 5 maximum adjustments, include only minimal WebGL hardening if playtest
  makes it blocking, then decide whether to create a small story or micro-epic.

Synced on 2026-06-06:

- Short polish UX / ride-feel playtest plan created locally:
  `_bmad-output/implementation-artifacts/playtest-polish-ux-ride-feel-2026-06-06.md`.
- Plan scope recorded in Linear:
  - duration: 10 to 15 minutes;
  - platforms: desktop then mobile;
  - observed zones only: camera comfort, route/biome readability, slider feel,
    HUD readability, summary coherence and mobile readability;
  - scoring: note 1 to 3 plus short observations;
  - selection rule: keep 3 to 5 maximum adjustments, with no broad backlog;
  - `WebGLRenderer` hardening included only if a real launch or ride failure is
    observed.
- Project playtest note:
  - Project: `Echappee 3D - Vertical Slice Mock`
  - Project ID: `7abb762d-4eb3-4089-af1f-0b973eeadd65`
  - Project comment ID: `831b0046-366d-4bb8-ad8c-f28b66d2af7c`
  - No new Linear issues were created.
- Local BMAD sprint status updated in
  `_bmad-output/implementation-artifacts/sprint-status.yaml`:
  - `next_action: run-polish-playtest`
  - `post_mvp_playtest_plan: _bmad-output/implementation-artifacts/playtest-polish-ux-ride-feel-2026-06-06.md`
  - `first_story_to_start: none`

Synced on 2026-06-06:

- Self-playtest results recorded locally:
  `_bmad-output/implementation-artifacts/playtest-polish-ux-ride-feel-results-2026-06-06.md`.
- Validation evidence recorded in Linear:
  - HTTP 200 on `http://127.0.0.1:5174/`;
  - desktop and mobile happy path through start, ride, slider low/mid/high,
    pause, resume, finish and summary;
  - screenshots captured via Chrome headless/CDP and checked with ImageMagick as
    nonblank;
  - slider response observed, pause/resume coherent, summary coherent;
  - no `WebGLRenderer` failure observed, so hardening was not selected.
- Proposed polish adjustments, capped to 3 for now:
  1. compact mobile HUD to free the scene;
  2. make slider response easier to understand;
  3. make biome change observable during a short playtest.
- Project self-playtest summary note:
  - Project: `Echappee 3D - Vertical Slice Mock`
  - Project ID: `7abb762d-4eb3-4089-af1f-0b973eeadd65`
  - Project comment ID: `63d987b7-469f-4775-9ad1-a4d9cf9a0672`
  - No new Linear issues were created.
- Local BMAD sprint status updated in
  `_bmad-output/implementation-artifacts/sprint-status.yaml`:
  - `next_action: choose-polish-adjustments`
  - `post_mvp_playtest_results: _bmad-output/implementation-artifacts/playtest-polish-ux-ride-feel-results-2026-06-06.md`
  - `first_story_to_start: none`

Synced on 2026-06-06:

- Mini-scope polish selected locally:
  `_bmad-output/implementation-artifacts/polish-ux-ride-feel-mini-scope-2026-06-06.md`.
- Selected adjustments, capped to 3:
  1. compact mobile HUD to free the scene;
  2. make slider response easier to understand;
  3. make biome change observable during a short playtest.
- Scope guardrails:
  - no BLE/FTMS;
  - no Meshy or heavy asset pipeline;
  - no full new route;
  - no UI redesign;
  - no `WebGLRenderer` hardening without observed failure;
  - no new Linear issue created at this selection step.
- Project self-playtest note updated with the selected mini-scope:
  - Project: `Echappee 3D - Vertical Slice Mock`
  - Project ID: `7abb762d-4eb3-4089-af1f-0b973eeadd65`
  - Project comment ID: `63d987b7-469f-4775-9ad1-a4d9cf9a0672`
- Local BMAD sprint status updated in
  `_bmad-output/implementation-artifacts/sprint-status.yaml`:
  - `next_action: prepare-polish-story-or-micro-epic`
  - `post_mvp_polish_mini_scope: _bmad-output/implementation-artifacts/polish-ux-ride-feel-mini-scope-2026-06-06.md`
  - `first_story_to_start: none`

Synced on 2026-06-06:

- `gds-correct-course` decision recorded locally:
  `_bmad-output/planning-artifacts/sprint-change-proposal-polish-ux-ride-feel-2026-06-06.md`.
- Decision: keep the 3 self-playtest adjustments together, but implement them
  as one small polish story, not a broad backlog:
  1. compact mobile HUD to free the scene;
  2. make slider response easier to understand;
  3. make biome change observable during a short playtest.
- Story creation prompt prepared in:
  `_bmad-output/implementation-artifacts/polish-ux-ride-feel-mini-scope-2026-06-06.md`.
- Out of scope remains:
  - BLE/FTMS/Web Bluetooth;
  - Meshy or heavy asset pipeline;
  - full new route;
  - global UI redesign;
  - new gameplay mode;
  - `WebGLRenderer` hardening without observed failure;
  - broad backlog or multiple Linear issues.
- Project decision note:
  - Project: `Echappee 3D - Vertical Slice Mock`
  - Project ID: `7abb762d-4eb3-4089-af1f-0b973eeadd65`
  - Project comment ID: `bac58598-d82c-4d90-8eb3-e1b184e5bebe`
  - No new Linear issue was created.
- Local BMAD sprint status updated in
  `_bmad-output/implementation-artifacts/sprint-status.yaml`:
  - `next_action: create-polish-story`
  - `post_mvp_change_proposal: _bmad-output/planning-artifacts/sprint-change-proposal-polish-ux-ride-feel-2026-06-06.md`
  - `post_mvp_polish_mini_scope: _bmad-output/implementation-artifacts/polish-ux-ride-feel-mini-scope-2026-06-06.md`
  - `first_story_to_start: none`

## Sync Policy

BMAD artifacts remain the local, versionable source of truth. Linear is the
execution tracker. After BMAD changes planning or implementation artifacts,
compare this mapping and automatically sync non-destructive updates to existing
Linear records in team `MYB` / project `Echappee 3D - Vertical Slice Mock`.

Non-destructive sync includes existing issue status changes, existing issue
descriptions, existing project documents, implementation notes, validation
evidence, review verdicts, and BMAD/local artifact references.

Ask for confirmation before destructive or scope-changing Linear operations:
creating new projects, initiatives, epics, or broad backlog; archiving/deleting;
renaming canonical trackers; changing team/project ownership; or expanding beyond
the current one MVP epic and five vertical-slice stories.

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

## Sync Policy

BMAD artifacts remain the local, versionable source of truth. Linear is the
execution tracker. After BMAD changes planning artifacts, compare this mapping
before creating or updating Linear records.

Do not auto-sync broad changes. Ask for confirmation before creating,
renaming, deleting, or materially changing Linear projects, issues, or docs.

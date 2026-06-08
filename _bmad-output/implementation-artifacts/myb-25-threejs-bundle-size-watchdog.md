# Story MYB-25: Three.js bundle size watchdog
## Metadata
- Linear issue: `MYB-24` (follow-up)
- Linear URL: https://linear.app/kefjbo/issue/MYB-24/optimiser-la-taille-de-bundle-code-splitting-budget-de-build
- Local status: `done`
- Depends on: `MYB-24`
- Active target: React/Vite/Three.js web demo
- Created: `2026-06-08`
- Updated: `2026-06-08`

## Story
As a vertical-slice maintainer,
I want an explicit local bundle budget check,
so that the Three.js demo can detect regressions in chunk growth before they enter review.

## Acceptance Mapping
- Web-only scope is preserved; no Unity source edits.
- `npm run build` remains the source of truth for generated chunks.
- `npm run validate:bundle-budget` must fail when any JS chunk exceeds the configured budget and pass otherwise.
- The budget report records chunk names and sizes for deterministic trend checks.
- Existing mock behavior and visual scope remain unchanged.

## Implementation Tasks completed
- Added `scripts/validate-build-bundles.mjs` style budget checker as
  `scripts/validate-build-budgets.mjs`.
- Added `npm run validate:bundle-budget` command.
- Added deterministic budget report output at
  `_bmad-output/web-test-results/myb-25-bundle-size-budget.txt`.
- Added unit coverage in `scripts/validate-build-budgets.test.mjs`.
- Preserved MYB-24 hardening scope by reusing the `380 KB` threshold.

## File list
- `scripts/validate-build-budgets.mjs`
- `scripts/validate-build-budgets.test.mjs`
- `package.json`
- `_bmad-output/implementation-artifacts/sprint-status.yaml`
- `_bmad-output/linear-sync.md`

## Validation status
## Validation status
- `npm run typecheck`: pass.
- `npm run test`: pass, 25 files / 94 tests.
- `npm run build`: pass, 15 JS chunks, max 352.684 KB.
- `npm run validate:bundle-budget`: pass, no oversized chunks, report generated.
- `npm run capture:ride-video`: pass (60s), HTTP 200, canvasNonBlank true.
- Budget report: `_bmad-output/web-test-results/myb-25-bundle-size-budget.txt`.

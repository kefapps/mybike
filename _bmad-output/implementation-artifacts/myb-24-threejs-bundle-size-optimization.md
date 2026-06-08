# Story MYB-24: Three.js bundle size optimization

## Metadata

- Linear issue: `MYB-24`
- Linear URL: https://linear.app/kefjbo/issue/MYB-24/optimiser-la-taille-de-bundle-code-splitting-budget-de-build
- Local status: `done`
- Depends on: `MYB-21`
- Active target: React/Vite/Three.js web demo
- Created: `2026-06-08`
- Updated: `2026-06-08`

## Story

As a vertical-slice maintainer,
I want to reduce bundle pressure and stabilize build quality,
so that the production warning profile is measurable and easier to maintain while preserving mock ride behavior.

## Acceptance Mapping

- Web-only scope is preserved; no Unity source edits.
- Three.js and route renderer remain visually stable in default ride flow.
- Build output is split for heavy dependencies and exposes budgets/checks that make size regressions visible.
- No functional regressions in ride controls, HUD, or summary.

## Implementation Tasks completed

- `vite.config.ts`:
  - Added build chunking with explicit `manualChunks` groups:
    - `vendor-react` for React + ReactDOM + scheduler.
    - `vendor-three` for Three.js.
    - `vendor` for remaining `node_modules` modules.
  - Set `chunkSizeWarningLimit` to `380` (KB) as the practical build budget.
  - Split `three` imports away from `three` barrel by using internal module imports (e.g. `three/src/...`) in renderers, which generates smaller per-domain chunks.
- `src/App.tsx`:
  - Swapped direct `RideScreen` import to `React.lazy`.
  - Wrapped running/paused render path in `Suspense` with a lightweight loading fallback.
- `src/render/SceneController.ts`, `src/render/createBiomeVisuals.ts`, `src/render/createRouteMesh.ts`, `src/render/renderHelpers.ts`:
  - Replaced `import * as THREE from "three"` with explicit named imports from `three/src/...` paths to reduce monolithic Three.js chunking.
- `src/App.test.tsx`:
  - Updated async session tests with microtask flushing so the lazy-loaded ride screen can resolve before assertions.
- `vite.config.ts` warning budget:
  - With deep imports, `vite build` now emits split `three` chunks across domains, with the largest at `vendor-three-renderers` (~361 KB).
  - This is now below the configured warning limit of 380 KB.

## File list

- `_bmad-output/implementation-artifacts/myb-24-threejs-bundle-size-optimization.md`
- `src/App.tsx`
- `src/App.test.tsx`
- `src/render/SceneController.ts`
- `src/render/createBiomeVisuals.ts`
- `src/render/createRouteMesh.ts`
- `src/render/renderHelpers.ts`
- `vite.config.ts`

## Validation status

- Validation status:
  - `npm run build` ✅ (passes; main `three` chunks split, no size warning under 380 KB threshold).
  - `npm run typecheck` ✅.
  - `npm run test` ✅ (24 tests files, 92 tests).

## Notes

- Initial optimization is intentionally scoped to code-splitting + build budget controls while preserving mock behavior and existing game-loop purity.
- Circular chunk dependency warnings from Rollup (`vendor-three-*`) can be addressed later if stronger hard budgets are required.

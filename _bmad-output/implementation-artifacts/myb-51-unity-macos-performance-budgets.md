# MYB-51 - Unity macOS Performance Budgets

Status: implemented
Linear: https://linear.app/kefjbo/issue/MYB-51/myb-019-definir-et-mesurer-les-budgets-performance-unity-macos-first
Branch: `myb-51-unity-macos-performance-budgets`

## Goal

Define lightweight performance budgets for the canonical Unity macOS-first
vertical slice in `unity/Echapee4D`, then produce a reproducible local baseline
that future visual, HUD, asset and delivery tickets can compare against.

## Grill-with-docs decisions

- `Budget performance` thresholds are soft decision signals in MYB-51.
- A red result is not blocking by default; it means the direction needs review.
- Hard blockers stay limited to structural validation: Unity reachable, scene
  valid, no missing canonical objects, and no console error found by the wider
  validation flow.
- Mandatory measurement target: Unity Editor on macOS.
- Opportunistic target: macOS build size and launch time, only if a local build
  is produced without turning MYB-51 into an infra ticket.
- WebGL is secondary and out of scope unless a later ticket explicitly asks for
  browser proof.
- Baseline machine: MacBook Pro Mac14,9 / Apple M2 Pro / macOS 26.5.1 / Unity
  6000.4.10f1.
- FPS budget: 60 fps target, warning below 45 fps, red below 30 fps.
- macOS build size budget: target <= 500 MB, warning > 750 MB, red > 1 GB.
- Local launch budget: target <= 10 s, warning > 20 s, red > 30 s.
- Lightweight scene counters: renderers, estimated triangles, active lights,
  unique materials, texture count, max texture dimension, estimated texture
  memory, quality settings and build target support.

## Implementation shape

- Added `Baseline performance`, `Budget performance` and
  `Garde-fou performance` glossary terms to `CONTEXT.md`.
- Added `MYB51PerformanceBudgetValidator` as an Editor-only validator.
- The validator writes:
  - `_bmad-output/unity-test-results/myb-51-performance-baseline.json`
  - `_bmad-output/unity-test-results/myb-51-performance-baseline.txt`
- The JSON is machine-readable enough for later tickets to compare budget drift.
- The TXT is the compact human validation evidence.

## Baseline result

- Structural status: PASS.
- Overall soft status: `not-measured`.
- Active renderers: 271.
- Mesh filters: 270.
- Skinned mesh renderers: 1.
- Estimated triangles: 58,611.
- Active lights: 6.
- Unique materials: 34.
- Unique textures: 3.
- Max texture dimension: 1024.
- Estimated texture memory: 5.34 MiB.
- Quality level: PC.
- vSync count: 0.
- Application target frame rate: -1.
- Active build target: WebGL.
- Standalone macOS build supported: true.

## Soft signals

- Editor rendering FPS is not sampled by the synchronous validator.
- macOS build size and launch time are not measured unless a local build is
  produced in a later pass.
- Active Unity build target is currently `WebGL`; MYB-51 records this without
  switching targets to avoid import churn.
- None of these soft signals blocks MYB-51.

## Validation executed

- `unity-mcp-cli status unity/Echapee4D --timeout 10000`
- `MYB51PerformanceBudgetValidator.ValidatePerformanceBaselineCli()`
- `MYB91CanonicalBaselineValidator.ValidateCanonicalBaselineCli()`

## Evidence

- `_bmad-output/unity-test-results/myb-51-performance-baseline.json`
- `_bmad-output/unity-test-results/myb-51-performance-baseline.txt`
- `_bmad-output/unity-test-results/myb-91-canonical-baseline.txt`

## Scope notes

- No MYB-96 asset pack work in this branch.
- No WebGL build in this ticket unless explicitly requested later.
- No exhaustive profiling pipeline.
- No CI gate that fails on soft red budgets.

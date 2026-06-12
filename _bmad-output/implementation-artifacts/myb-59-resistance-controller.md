# MYB-59 - Unity ResistanceController Hardware-Independent Boundary

Status: implemented locally

## Scope Delivered

- Added a hardware-independent resistance boundary under `Assets/MYB59`.
- Added canonical runtime terms in `unity/Echapee4D/CONTEXT.md`:
  - `Demande de Resistance`
  - `Resistance Appliquee`
  - `Resistance Mesuree`
- Added ADR `unity/Echapee4D/docs/adr/0001-resistance-controller-boundary.md`.
- Added `MYB59ResistanceDemand` with `0..1` canonical resistance and derived
  `0..100` level.
- Added `MYB59ResistanceSnapshot` with `Applied`, `Fallback` and `Unavailable`
  statuses.
- Added `MYB59ResistanceController` as a mock/fallback Unity component.
- Connected `MYB89ProbeRide` so MYB-57 continues to calculate resistance demand,
  effort, fatigue and speed, while MYB-59 applies or falls back that demand.
- Updated the probe HUD and report to expose target-to-applied resistance and
  controller status.
- Added deterministic MYB-59 validation for pure controller behavior and
  scene/HUD integration.

## Decisions Applied

- MYB-57 remains the source of resistance demand and gameplay equations.
- MYB-59 does not change speed or fatigue equations.
- Missing or unavailable controller state does not block mock ride playability.
- No BLE, FTMS, CoreBluetooth, real trainer control, firmware, WebGL or React
  work is included.

## Review Fixes

- Reused the autoplay effort/resistance snapshot for HUD rendering so the mock
  controller is applied once per frame.
- Made the scene validator fail if `MYB89ProbeRide` is not already wired to the
  scene `MYB59ResistanceController`.
- Added the MYB-59 validator to `npm run validate:local-ci`.
- Normalized local missing-controller fallback resistance to the same default as
  the mock controller fallback.

## Validation

- `MYB89ProbeBuilder.BuildScene()`: PASS via Unity batchmode.
- `MYB59ResistanceControllerValidator.ValidateResistanceControllerCli()`: PASS
  via Unity batchmode.
- `MYB89ProbeBuilder.ValidateProbeScene()`: PASS via Unity batchmode.
- `MYB57EffortSimulatorValidator.ValidateEffortSimulatorCli()`: PASS via Unity
  batchmode.
- `MYB91CanonicalBaselineValidator.ValidateCanonicalBaselineCli()`: PASS via
  Unity batchmode.
- `git diff --check`: PASS.
- `npm run validate:local-ci`: PASS, including the MYB-59 resistance controller
  validator.

## Evidence

- `_bmad-output/unity-test-results/myb-59-resistance-controller.txt`
- `_bmad-output/unity-test-results/myb-89-unity-mcp-probe-validator.txt`
- `_bmad-output/unity-test-results/myb-57-effort-simulator.txt`
- `_bmad-output/unity-test-results/myb-91-canonical-baseline.txt`
- `_bmad-output/unity-test-results/myb-83-local-ci.txt`

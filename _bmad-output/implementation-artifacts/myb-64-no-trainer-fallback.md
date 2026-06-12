# MYB-64 - Unity No-Trainer Fallback

Status: implemented locally

## Scope Delivered

- Added `MYB57TrainerSourcePreset.ManualFallback`.
- Added estimated manual-effort trainer samples to `MYB57EffortSimulator`.
- Added `useNoTrainerFallback` and `manualFallbackEffort01` to
  `MYB89ProbeRide`.
- Treating the public `ManualFallback` trainer source preset as no-trainer
  fallback too, so inspector selection and the explicit toggle share the same
  HUD and manual-effort path.
- Updated the Unity HUD to show explicit no-trainer mode copy:
  `No trainer: Manual` and `Effort: Manual fallback`.
- Kept the MYB-59 resistance controller boundary active; when the controller is
  unavailable, the ride continues through the existing local resistance fallback.
- Added `MYB64NoTrainerFallbackValidator`.
- Added MYB-64 to `npm run validate:local-ci`.

## Decisions Applied

- Manual fallback is an estimated local input, not real trainer telemetry.
- The fallback keeps the route moving without BLE, FTMS, CoreBluetooth, pairing
  or required hardware.
- Existing mock trainer presets remain unchanged for the default demo path.
- WebGL and React remain untouched.

## Validation

- `MYB64NoTrainerFallbackValidator.ValidateNoTrainerFallbackCli()`: PASS via
  Unity-MCP script execution, including the explicit toggle path and direct
  `ManualFallback` preset path.
- `npm run validate:local-ci`: PASS, including MYB-64 no-trainer fallback.
- `git diff --check`: PASS.

## Evidence

- `_bmad-output/unity-test-results/myb-64-no-trainer-fallback.txt`
- `_bmad-output/unity-test-results/myb-83-local-ci.txt`

# MYB-60 - Unity Mock Resistance Mapping and Smoothing

Status: implemented locally

## Scope Delivered

- Added `Assets/MYB60/Runtime/MYB60ResistanceMapper.cs`.
- Added a pure mapping contract from raw MYB-57 target resistance to:
  - comfort-limited resistance,
  - smoothed resistance,
  - mapping status `Stable`, `Ramping` or `Limited`.
- Integrated MYB-60 into `MYB89ProbeRide` before the MYB-59 controller boundary.
- Updated the probe HUD to expose raw-to-smoothed-to-applied resistance:
  `raw~smoothed->applied`.
- Updated the probe verdict HUD to expose MYB-60 mapping status.
- Added `MYB60ResistanceMapperValidator` with pure model, scene wiring, HUD and
  smoothing checks.
- Added MYB-60 to `npm run validate:local-ci`.

## Decisions Applied

- MYB-57 remains the source of raw route/grade target resistance and still owns
  speed, effort and fatigue equations.
- MYB-60 owns mock resistance comfort limits and smoothing before the MYB-59
  controller receives a demand.
- MYB-59 remains the controller boundary that applies, falls back or reports
  unavailable status.
- No BLE, FTMS, CoreBluetooth, real trainer writes, WebGL or React work is
  included.

## Review Fixes

- Enforced segment comfort caps on the previous smoothed resistance before
  ramp-down, so recovery/warmup caps remain hard caps on applied mock demand.
- Preserved the previous smoothed resistance when sampling with zero delta time,
  avoiding HUD/editor refresh snaps to the raw target.
- Strengthened the MYB-60 validator to prove smoothed and comfort-limited ride
  path demands reach the MYB-59 controller.
- Added a scene wiring assertion for the MYB-59 controller reference before the
  validator mutates scene objects.
- Aligned autoplay sampling so effort/resistance, pose and HUD use the same
  advanced route position within a frame.
- Added post-Unity drift checks to `npm run validate:local-ci`.

## Validation

- `MYB89ProbeBuilder.BuildScene()`: PASS via Unity-MCP script execution.
- `MYB60ResistanceMapperValidator.ValidateResistanceMapperCli()`: PASS via
  Unity-MCP script execution.
- `MYB59ResistanceControllerValidator.ValidateResistanceControllerCli()`: PASS
  via Unity-MCP script execution.
- `MYB89ProbeBuilder.ValidateProbeScene()`: PASS via Unity-MCP script execution.
- `MYB57EffortSimulatorValidator.ValidateEffortSimulatorCli()`: PASS via
  Unity-MCP script execution.
- `git diff --check`: PASS.
- `npm run validate:local-ci`: PASS, including the MYB-60 resistance mapper
  validator and post-Unity drift checks.

## Evidence

- `_bmad-output/unity-test-results/myb-60-resistance-mapper.txt`
- `_bmad-output/unity-test-results/myb-59-resistance-controller.txt`
- `_bmad-output/unity-test-results/myb-89-unity-mcp-probe-validator.txt`
- `_bmad-output/unity-test-results/myb-57-effort-simulator.txt`
- `_bmad-output/unity-test-results/myb-83-local-ci.txt`

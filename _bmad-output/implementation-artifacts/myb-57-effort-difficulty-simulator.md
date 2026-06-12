# MYB-57 - Unity Mock Effort/Difficulty Simulator

Status: implemented

## Scope Delivered

- Added an FTMS-friendly mock effort model under `Assets/MYB57/Runtime`.
- Modeled trainer modes as `Simulated`, `ReadOnly`, and `Controllable`.
- Implemented only `Simulated` mode for this ticket.
- Added mock source presets:
  - `PowerAvailable`
  - `CadenceResistance`
  - `CadenceOnly`
- Kept source priority aligned with trainer data:
  `powerWatts -> cadenceRpm + resistanceLevel -> cadenceRpm`.
- Kept `pedalEffort01` as a derived UI/debug value, not the canonical source.
- Converted route segment and grade into `targetResistance01` and
  `targetResistanceLevel` (`0..100`).
- Connected `MYB89ProbeRide` speed and HUD to the MYB-57 effort snapshot.
- Added fatigue that rises from measured effort weighted by target resistance
  and recovers on recovery/descent/low effort.
- Added a deterministic MYB-57 validator plus scene/HUD validation.

## Runtime Integration

- `MYB57EffortSimulator` is pure runtime logic and can evaluate mock trainer
  samples without scene dependencies.
- `MYB89ProbeRide` now defaults to the simulated trainer preset
  `PowerAvailable`.
- The existing HUD remains compact:
  - speed uses the effort simulator output;
  - effort shows the label and derived effort percentage;
  - grade also shows target resistance;
  - verdict shows mock trainer source, fatigue, and route progress.

## Decisions Applied

- Pente/segment drive target resistance instead of directly hacking speed.
- Descent reduces resistance and fatigue and only adds a small coast bonus.
- No BLE, CoreBluetooth, real FTMS, smart trainer control, ESP32 firmware, or
  hardware bridge implementation is included.
- ESP32-WROOM/read-only/controllable trainer exploration is tracked separately
  by `MYB-97`.

## Validation

- `unity-mcp-cli status unity/Echapee4D --timeout 10000`: PASS.
- `MYB89ProbeBuilder.ValidateProbeScene()`: PASS.
- `MYB57EffortSimulatorValidator.ValidateEffortSimulatorCli()`: PASS.
- `MYB91CanonicalBaselineValidator.ValidateCanonicalBaselineCli()`: PASS.
- `npm run validate:local-ci`: PASS.
- `git diff --check`: PASS.

## Evidence

- `_bmad-output/unity-test-results/myb-57-effort-simulator.txt`
- `_bmad-output/unity-test-results/myb-89-unity-mcp-probe-validator.txt`
- `_bmad-output/unity-test-results/myb-91-canonical-baseline.txt`
- `_bmad-output/unity-test-results/myb-83-local-ci.txt`

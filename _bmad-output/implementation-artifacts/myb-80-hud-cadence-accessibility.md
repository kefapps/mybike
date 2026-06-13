# MYB-80 - HUD Cadence And Readability

## Scope

MYB-80 reduces visual noise in the canonical Unity ride HUD by decoupling
visible text refresh cadence from the ride simulation loop.

It keeps mock mode playable and does not add native macOS accessibility,
VoiceOver, keyboard navigation, connected-bike telemetry, React/WebGL work, or a
full HUD redesign.

## Decisions

- `Cadence HUD` means visible HUD text refresh frequency, not simulation tick,
  camera motion, ride trajectory, effort simulation, or resistance control.
- Fast ride metrics refresh at 4 Hz.
- Slower context and state labels refresh at 2 Hz.
- Main ride HUD copy no longer exposes technical wording such as `Mock`, `Map`,
  `Ctrl`, `running`, `->`, or `~`.
- Resistance and fatigue stay visible in short readable form.
- The ride HUD remains hidden while the MYB-79 welcome screen is visible.

## Implementation

- Added the `Cadence HUD` glossary entry to `unity/Echapee4D/CONTEXT.md`.
- Added fast and slow HUD cadence settings to `MYB89ProbeRide`, with validation
  counters and a validator-only tick path.
- Updated HUD copy:
  - distance includes route progress percent;
  - grade shows readable applied resistance;
  - verdict shows source, resistance, and fatigue without controller/mapping
    debug strings;
  - no-trainer fallback uses `Sans capteur`.
- Updated `MYB89ProbeBuilder` defaults and scene validation expectations for
  the 4 Hz / 2 Hz cadence contract.
- Added `MYB80HudCadenceValidator` and wired it into
  `scripts/validate-local-ci.mjs`.
- Aligned MYB-57, MYB-59, MYB-60, and MYB-64 validators with the new HUD copy
  while keeping their behavior-level snapshot checks.

## Validation

Local validation on 2026-06-13:

- `unity-mcp-cli run-tool script-execute ...`: PASS after MYB-99 upgraded the
  Unity-MCP package to `0.81.0`.
- `npm run validate:local-ci`: PASS, including Unity-MCP status and MYB-91,
  MYB-59, MYB-60, MYB-64, MYB-73, MYB-79, MYB-80, and MYB-98 validators via
  Unity-MCP script execution.
- `git diff --check`: PASS.

Proof paths:

- `_bmad-output/unity-test-results/myb-83-local-ci.txt`
- `_bmad-output/unity-test-results/myb-80-hud-cadence-validator.txt`
- `_bmad-output/unity-test-results/myb-91-canonical-baseline.txt`
- `_bmad-output/unity-test-results/myb-89-unity-mcp-probe-validator.txt`
- `_bmad-output/unity-test-results/myb-57-effort-simulator.txt`
- `_bmad-output/unity-test-results/myb-59-resistance-controller.txt`
- `_bmad-output/unity-test-results/myb-60-resistance-mapper.txt`
- `_bmad-output/unity-test-results/myb-64-no-trainer-fallback.txt`

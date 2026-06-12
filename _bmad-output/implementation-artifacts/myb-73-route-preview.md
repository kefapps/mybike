# MYB-73 - Unity Route Preview

## Scope

MYB-73 adds a lightweight pre-ride `Fiche Route` to the canonical Unity scene
`unity/Echapee4D`. It stays inside the mock-mode vertical slice and does not
introduce route selection, WebGL work, connected-bike telemetry, or a heavy menu.

## Implementation

- Added `MYB73RoutePreviewPanel`, a dedicated Unity UI component fed by:
  - calculated route length from `MYB89ProbeRide`;
  - estimated duration from a stable reference speed;
  - local editorial metadata for biomes, overall difficulty, and three moments
    cles.
- Extended `MYB89ProbeRide` with a route-preview gate:
  - preview state disables autoplay and resets the ride to the route start;
  - launch hides the preview and resumes autoplay mock riding.
- Updated `MYB89ProbeBuilder` so the canonical scene builds with:
  - `MYB73_RoutePreview` on the existing HUD Canvas;
  - a functional launch button;
  - an EventSystem for button input.
- Added `MYB73RoutePreviewValidator`, which rebuilds the canonical scene,
  validates the pre-launch and post-launch states, and writes a PNG proof.
- Added MYB-73 to `scripts/validate-local-ci.mjs`.

## Validation

Local validation passed on 2026-06-12:

- `unity-mcp-cli run-tool script-execute unity/Echapee4D --input <MYB73 validator>`
  - PASS, route length `245.0 m`, estimated duration `111.4 s`, three moments
    cles, preview hidden after launch, ride autoplay resumed.
- `npm run validate:local-ci`
  - PASS, including MYB-91, MYB-59, MYB-60, MYB-64, and MYB-73 Unity
    validators.

Proof paths:

- `_bmad-output/unity-test-results/myb-73-route-preview-validator.txt`
- `_bmad-output/unity-test-results/myb-73-route-preview.png`

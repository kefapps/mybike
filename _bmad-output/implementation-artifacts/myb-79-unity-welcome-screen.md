# MYB-79 - Unity Welcome Screen

## Scope

MYB-79 refactors the existing Unity pre-ride `Fiche Route` into a product-facing
welcome screen for the canonical Unity scene `unity/Echapee4D`.

It keeps the MYB-73 launch flow and mock-mode ride gate. It does not add a
separate menu, route browser, React screen, WebGL proof, connected-bike
telemetry, or multi-route system.

## Implementation

- Renamed the pre-ride route highlights language from `Moment Cle` to `Passage`
  in the visual-direction glossary.
- Updated `MYB73RoutePreviewPanel` so the visible copy sells the balade first:
  - title: `L'Echappee des lumieres`;
  - CTA: `Commencer la balade`;
  - compact stats: distance, estimated duration, and difficulty;
  - three scenic-first `Passages` with light effort hints.
- Hid the ride HUD while the welcome screen is visible, then restored it after
  launch so debug/mock ride readouts are not visible on the product entry.
- Updated `MYB89ProbeBuilder` to generate a lighter, more immersive overlay with
  the Unity scene visible behind it.
- Added `MYB79WelcomeScreenValidator`, including a 1280x720 PNG proof and a
  scan of all active scene text for forbidden debug/mock/prototype wording.
- Added the MYB-79 validator to `scripts/validate-local-ci.mjs`.

## Validation

Local validation on 2026-06-13:

- `MYB79WelcomeScreenValidator.ValidateWelcomeScreenCli()`: PASS via direct
  Unity-MCP script execution.
- `MYB73RoutePreviewValidator.ValidateRoutePreviewCli()`: PASS via direct
  Unity-MCP script execution.
- `MYB89ProbeBuilder.ValidateProbeScene()`: PASS via direct Unity-MCP script
  execution.
- `MYB91CanonicalBaselineValidator.ValidateCanonicalBaselineCli()`: PASS via
  direct Unity-MCP script execution.
- `node --check scripts/validate-local-ci.mjs`: PASS.
- `git diff --check`: PASS.
- `npm run validate:local-ci`: PASS after restoring the local `unity-mcp-cli`
  wrapper.

Proof paths:

- `_bmad-output/unity-test-results/myb-79-welcome-screen-validator.txt`
- `_bmad-output/unity-test-results/myb-79-welcome-screen.png`
- `_bmad-output/unity-test-results/myb-73-route-preview-validator.txt`
- `_bmad-output/unity-test-results/myb-89-unity-mcp-probe-validator.txt`
- `_bmad-output/unity-test-results/myb-91-canonical-baseline.txt`
- `_bmad-output/unity-test-results/myb-83-local-ci.txt`

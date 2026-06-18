# MYB-164 Stabilization Report

## Summary

MYB-164 validates the canonical forest passage after the MYB-159..163 stack was merged to `main`. This is a stabilization/regression gate, not a new art pass.

## Baseline

- baseline: `MYB-163 after`
- route baseline: `_bmad-output/visual-checkpoints/MYB-163/2026-06-17T22-44-25Z-after-route.png`
- overview baseline: `_bmad-output/visual-checkpoints/MYB-163/2026-06-17T22-44-25Z-after-overview.png`
- reason: MYB-163 after was the Julien-validated canonical forest checkpoint.

## Scene

- canonical scene: `Assets/Scenes/MYB89UnityMcpProbe.unity`
- loaded scene: `Assets/Scenes/MYB89UnityMcpProbe.unity`
- generated root: `MYB163_CanonicalForestPassageRoot`
- generated root exists: Yes
- generated root active: Yes
- root child count: `6`
- route camera exists: Yes
- overview camera exists: Yes

## Metrics

- rendererCount: `183`
- meshFilterCount: `183`
- approximateTriangles: `13784`
- sceneLocalMaterialCount: `13`
- metrics JSON: `_bmad-output/implementation-artifacts/MYB-164/myb-164-stabilization-metrics.json`

## Visual Evidence

- after route: `_bmad-output/visual-checkpoints/MYB-164/2026-06-18T05-49-54Z-after-route.png`
- after overview: `_bmad-output/visual-checkpoints/MYB-164/2026-06-18T05-49-54Z-after-overview.png`
- route comparison: `_bmad-output/visual-checkpoints/MYB-164/2026-06-18T05-49-55Z-route-before-after.png`
- overview comparison: `_bmad-output/visual-checkpoints/MYB-164/2026-06-18T05-49-55Z-overview-before-after.png`
- capture reports:
- `_bmad-output/visual-checkpoints/MYB-164/2026-06-18T05-49-54Z-capture-report.md`
- `_bmad-output/visual-checkpoints/MYB-164/2026-06-18T05-49-55Z-capture-report.md`
- capture metadata:
- `_bmad-output/visual-checkpoints/MYB-164/2026-06-18T05-49-54Z-capture-metadata.json`
- `_bmad-output/visual-checkpoints/MYB-164/2026-06-18T05-49-55Z-capture-metadata.json`

## MYB-144 Validation

- verdict: `PASS`
- errors: `0`
- warnings: `0`
- report: `_bmad-output/unity-test-results/myb-144-art-asset-validator-report.md`

## Stabilization Interpretation

- no automated blocking regression detected: Yes
- route-camera comparison produced: Yes
- overview comparison produced: Yes
- human review still required: Yes

## Warning Categories

### Build / Capture Warnings
- None recorded.

### MYB-164 Visual Warnings
- Premium target not claimed; human route-camera review remains required.

### MYB-144 Existing Validator Warnings
- None recorded.

### Blocking Errors
- None recorded.

## Governance

- no new asset generation: Yes
- no Meshy/Tripo/Poly Haven/Blender content generation: Yes
- no gameplay modified: Yes
- no route trajectory/collider modified: Yes
- no HUD/telemetry modified: Yes
- Premium target reached: No

## Verdict

- Stabilization gate: PASS_WITH_WARNINGS
- Recommended Linear status: In Review

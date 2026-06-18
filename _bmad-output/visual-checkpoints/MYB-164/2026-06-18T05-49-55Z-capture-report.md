# MYB-145 Capture Report

Ticket:
- `MYB-164`

Generated at:
- 2026-06-18T05:49:55.0629040Z

Output directory:
- `_bmad-output/visual-checkpoints/MYB-164/`

Metadata:
- `_bmad-output/visual-checkpoints/MYB-164/2026-06-18T05-49-55Z-capture-metadata.json`

Mode:
- capture

State:
- after

Execution:
- Mode: MYB-164-Comparison
- Branch: `MYB-164-post-integration-canonical-forest-stabilization`
- Commit: `aebd603`

## Scene

Scene:
- `Assets/Scenes/MYB89UnityMcpProbe.unity`

## Cameras

| Camera | Role | Found | Position | Rotation | FOV/Size | Notes |
|---|---|---:|---|---|---:|---|
| RouteCamera | blocking validation | Yes | (-0.85, 1.55, 7.5) | (0.664, 1.411, 0) | 50 | perspective |
| OverviewCamera | secondary context | Yes | (0, 86, 66) | (90, 0, 0) | 42 | orthographic |

## Captures

| State | Type | Path | Scene | Camera | Resolution |
|---|---|---|---|---|---|
| after | route | `_bmad-output/visual-checkpoints/MYB-164/2026-06-18T05-49-55Z-after-route.png` | `Assets/Scenes/MYB89UnityMcpProbe.unity` | RouteCamera | 1600x900 |
| after | overview | `_bmad-output/visual-checkpoints/MYB-164/2026-06-18T05-49-55Z-after-overview.png` | `Assets/Scenes/MYB89UnityMcpProbe.unity` | OverviewCamera | 1600x900 |

## Comparisons

| Type | Before | After | Sheet |
|---|---|---|---|
| route | `_bmad-output/visual-checkpoints/MYB-163/2026-06-17T22-44-25Z-after-route.png` | `_bmad-output/visual-checkpoints/MYB-164/2026-06-18T05-49-54Z-after-route.png` | `_bmad-output/visual-checkpoints/MYB-164/2026-06-18T05-49-55Z-route-before-after.png` |
| overview | `_bmad-output/visual-checkpoints/MYB-163/2026-06-17T22-44-25Z-after-overview.png` | `_bmad-output/visual-checkpoints/MYB-164/2026-06-18T05-49-54Z-after-overview.png` | `_bmad-output/visual-checkpoints/MYB-164/2026-06-18T05-49-55Z-overview-before-after.png` |

## Explicit Baseline

Before selected by:
- MYB-164 stabilization runner

Reason:
- MYB-163 after is the Julien-validated canonical forest checkpoint; MYB-164 verifies the same canonical surface after merge to main.

Source:
- MYB-163 after route/overview captures

## Errors

- None

## Warnings

- None

## Info

| Code | Message |
|---|---|
| CAPTURE_WRITTEN | route after capture written to `_bmad-output/visual-checkpoints/MYB-164/2026-06-18T05-49-55Z-after-route.png`. |
| CAPTURE_WRITTEN | overview after capture written to `_bmad-output/visual-checkpoints/MYB-164/2026-06-18T05-49-55Z-after-overview.png`. |
| COMPARISON_WRITTEN | route before/after sheet written to `_bmad-output/visual-checkpoints/MYB-164/2026-06-18T05-49-55Z-route-before-after.png`. |
| COMPARISON_WRITTEN | overview before/after sheet written to `_bmad-output/visual-checkpoints/MYB-164/2026-06-18T05-49-55Z-overview-before-after.png`. |

## Verdict

- PASS

RouteCamera is the blocking validation camera. OverviewCamera is required as secondary context. MYB-145 does not judge Premium target.

# MYB-145 Capture Report

Ticket:
- `MYB-158`

Generated at:
- 2026-06-17T10:16:31.8042490Z

Output directory:
- `_bmad-output/visual-checkpoints/MYB-158/`

Metadata:
- `_bmad-output/visual-checkpoints/MYB-158/2026-06-17T10-16-31Z-capture-metadata.json`

Mode:
- capture

State:
- after

Execution:
- Mode: MYB-158-after
- Branch: `MYB-158-premium-route-camera-forest-corridor-pass`
- Commit: `3f1ada8`

## Scene

Scene:
- `Assets/Scenes/MYB158PremiumRouteCameraForestPass.unity`

## Cameras

| Camera | Role | Found | Position | Rotation | FOV/Size | Notes |
|---|---|---:|---|---|---:|---|
| RouteCamera | blocking validation | Yes | (1.934, 1.574, 7.5) | (0.541, 3.231, 0) | 50 | perspective |
| OverviewCamera | secondary context | Yes | (0, 86, 66) | (90, 0, 0) | 42 | orthographic |

## Captures

| State | Type | Path | Scene | Camera | Resolution |
|---|---|---|---|---|---|
| after | route | `_bmad-output/visual-checkpoints/MYB-158/2026-06-17T10-16-31Z-after-route.png` | `Assets/Scenes/MYB158PremiumRouteCameraForestPass.unity` | RouteCamera | 1600x900 |
| after | overview | `_bmad-output/visual-checkpoints/MYB-158/2026-06-17T10-16-31Z-after-overview.png` | `Assets/Scenes/MYB158PremiumRouteCameraForestPass.unity` | OverviewCamera | 1600x900 |

## Comparisons

| Type | Before | After | Sheet |
|---|---|---|---|

## Explicit Baseline

Before selected by:
- (not provided)

Reason:
- (not provided)

Source:
- (not provided)

## Errors

- None

## Warnings

- None

## Info

| Code | Message |
|---|---|
| CAPTURE_WRITTEN | route after capture written to `_bmad-output/visual-checkpoints/MYB-158/2026-06-17T10-16-31Z-after-route.png`. |
| CAPTURE_WRITTEN | overview after capture written to `_bmad-output/visual-checkpoints/MYB-158/2026-06-17T10-16-31Z-after-overview.png`. |
| COMPARISON_NOT_REQUESTED | route before/after comparison was not requested. |
| COMPARISON_NOT_REQUESTED | overview before/after comparison was not requested. |

## Verdict

- PASS

RouteCamera is the blocking validation camera. OverviewCamera is required as secondary context. MYB-145 does not judge Premium target.

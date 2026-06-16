# MYB-145 Capture Report

Ticket:
- `MYB-148`

Generated at:
- 2026-06-16T15:33:50.3545650Z

Output directory:
- `_bmad-output/visual-checkpoints/MYB-148/`

Metadata:
- `_bmad-output/visual-checkpoints/MYB-148/2026-06-16T15-33-50Z-capture-metadata.json`

Mode:
- capture

State:
- after

Execution:
- Mode: MYB148Batch
- Branch: `MYB-148-scatter-route-first-controle-par-bandes`
- Commit: `5dc9fb7`

## Scene

Scene:
- `Assets/Scenes/MYB148RouteFirstScatterPreview.unity`

## Cameras

| Camera | Role | Found | Position | Rotation | FOV/Size | Notes |
|---|---|---:|---|---|---:|---|
| RouteCamera | blocking validation | Yes | (1.934, 1.574, 7.5) | (0.541, 3.231, 0) | 50 | perspective |
| OverviewCamera | secondary context | Yes | (0, 86, 66) | (90, 0, 0) | 42 | orthographic |

## Captures

| State | Type | Path | Scene | Camera | Resolution |
|---|---|---|---|---|---|
| after | route | `_bmad-output/visual-checkpoints/MYB-148/2026-06-16T15-33-50Z-after-route.png` | `Assets/Scenes/MYB148RouteFirstScatterPreview.unity` | RouteCamera | 1600x900 |
| after | overview | `_bmad-output/visual-checkpoints/MYB-148/2026-06-16T15-33-50Z-after-overview.png` | `Assets/Scenes/MYB148RouteFirstScatterPreview.unity` | OverviewCamera | 1600x900 |

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
| CAPTURE_WRITTEN | route after capture written to `_bmad-output/visual-checkpoints/MYB-148/2026-06-16T15-33-50Z-after-route.png`. |
| CAPTURE_WRITTEN | overview after capture written to `_bmad-output/visual-checkpoints/MYB-148/2026-06-16T15-33-50Z-after-overview.png`. |
| COMPARISON_NOT_REQUESTED | route before/after comparison was not requested. |
| COMPARISON_NOT_REQUESTED | overview before/after comparison was not requested. |

## Verdict

- PASS

RouteCamera is the blocking validation camera. OverviewCamera is required as secondary context. MYB-145 does not judge Premium target.

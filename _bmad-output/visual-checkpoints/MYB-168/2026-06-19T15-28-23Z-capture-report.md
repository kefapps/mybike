# MYB-145 Capture Report

Ticket:
- `MYB-168`

Generated at:
- 2026-06-19T15:28:23.9590520Z

Output directory:
- `_bmad-output/visual-checkpoints/MYB-168/`

Metadata:
- `_bmad-output/visual-checkpoints/MYB-168/2026-06-19T15-28-23Z-capture-metadata.json`

Mode:
- capture

State:
- after

Execution:
- Mode: Batch
- Branch: `MYB-168-fix-closeleftframe-canopy-route-readability`
- Commit: `85e2054`

## Scene

Scene:
- `Assets/Scenes/MYB89UnityMcpProbe.unity`

## Cameras

| Camera | Role | Found | Position | Rotation | FOV/Size | Notes |
|---|---|---:|---|---|---:|---|
| RouteCamera | blocking validation | Yes | (1.503, 1.49, 27.513) | (4.019, 349.28, 0) | 50 | perspective |
| OverviewCamera | secondary context | Yes | (2, 86, 1142.5) | (90, 0, 0) | 42 | orthographic |

## Captures

| State | Type | Path | Scene | Camera | Resolution |
|---|---|---|---|---|---|
| after | route | `_bmad-output/visual-checkpoints/MYB-168/2026-06-19T15-28-23Z-after-route.png` | `Assets/Scenes/MYB89UnityMcpProbe.unity` | RouteCamera | 1600x900 |
| after | overview | `_bmad-output/visual-checkpoints/MYB-168/2026-06-19T15-28-23Z-after-overview.png` | `Assets/Scenes/MYB89UnityMcpProbe.unity` | OverviewCamera | 1600x900 |

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
| CAPTURE_WRITTEN | route after capture written to `_bmad-output/visual-checkpoints/MYB-168/2026-06-19T15-28-23Z-after-route.png`. |
| CAPTURE_WRITTEN | overview after capture written to `_bmad-output/visual-checkpoints/MYB-168/2026-06-19T15-28-23Z-after-overview.png`. |
| COMPARISON_NOT_REQUESTED | route before/after comparison was not requested. |
| COMPARISON_NOT_REQUESTED | overview before/after comparison was not requested. |

## Verdict

- PASS

RouteCamera is the blocking validation camera. OverviewCamera is required as secondary context. MYB-145 does not judge Premium target.

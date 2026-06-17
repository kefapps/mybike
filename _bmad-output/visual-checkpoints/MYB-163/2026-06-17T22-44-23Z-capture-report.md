# MYB-145 Capture Report

Ticket:
- `MYB-163`

Generated at:
- 2026-06-17T22:44:23.4213710Z

Output directory:
- `_bmad-output/visual-checkpoints/MYB-163/`

Metadata:
- `_bmad-output/visual-checkpoints/MYB-163/2026-06-17T22-44-23Z-capture-metadata.json`

Mode:
- capture

State:
- before

Execution:
- Mode: MYB-163-before
- Branch: `MYB-163-canonical-forest-passage-integration`
- Commit: `f7fb40b`

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
| before | route | `_bmad-output/visual-checkpoints/MYB-163/2026-06-17T22-44-23Z-before-route.png` | `Assets/Scenes/MYB89UnityMcpProbe.unity` | RouteCamera | 1600x900 |
| before | overview | `_bmad-output/visual-checkpoints/MYB-163/2026-06-17T22-44-23Z-before-overview.png` | `Assets/Scenes/MYB89UnityMcpProbe.unity` | OverviewCamera | 1600x900 |

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
| CAPTURE_WRITTEN | route before capture written to `_bmad-output/visual-checkpoints/MYB-163/2026-06-17T22-44-23Z-before-route.png`. |
| CAPTURE_WRITTEN | overview before capture written to `_bmad-output/visual-checkpoints/MYB-163/2026-06-17T22-44-23Z-before-overview.png`. |
| COMPARISON_NOT_REQUESTED | route before/after comparison was not requested. |
| COMPARISON_NOT_REQUESTED | overview before/after comparison was not requested. |

## Verdict

- PASS

RouteCamera is the blocking validation camera. OverviewCamera is required as secondary context. MYB-145 does not judge Premium target.

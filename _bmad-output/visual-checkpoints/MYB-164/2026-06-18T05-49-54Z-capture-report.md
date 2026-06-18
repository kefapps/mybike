# MYB-145 Capture Report

Ticket:
- `MYB-164`

Generated at:
- 2026-06-18T05:49:54.8137860Z

Output directory:
- `_bmad-output/visual-checkpoints/MYB-164/`

Metadata:
- `_bmad-output/visual-checkpoints/MYB-164/2026-06-18T05-49-54Z-capture-metadata.json`

Mode:
- capture

State:
- after

Execution:
- Mode: MYB-164-CanonicalPostMerge
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
| after | route | `_bmad-output/visual-checkpoints/MYB-164/2026-06-18T05-49-54Z-after-route.png` | `Assets/Scenes/MYB89UnityMcpProbe.unity` | RouteCamera | 1600x900 |
| after | overview | `_bmad-output/visual-checkpoints/MYB-164/2026-06-18T05-49-54Z-after-overview.png` | `Assets/Scenes/MYB89UnityMcpProbe.unity` | OverviewCamera | 1600x900 |

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
| CAPTURE_WRITTEN | route after capture written to `_bmad-output/visual-checkpoints/MYB-164/2026-06-18T05-49-54Z-after-route.png`. |
| CAPTURE_WRITTEN | overview after capture written to `_bmad-output/visual-checkpoints/MYB-164/2026-06-18T05-49-54Z-after-overview.png`. |
| COMPARISON_NOT_REQUESTED | route before/after comparison was not requested. |
| COMPARISON_NOT_REQUESTED | overview before/after comparison was not requested. |

## Verdict

- PASS

RouteCamera is the blocking validation camera. OverviewCamera is required as secondary context. MYB-145 does not judge Premium target.

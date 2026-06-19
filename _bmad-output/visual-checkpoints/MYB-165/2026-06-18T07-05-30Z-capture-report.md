# MYB-145 Capture Report

Ticket:
- `MYB-165`

Generated at:
- 2026-06-18T07:05:30.8776980Z

Output directory:
- `_bmad-output/visual-checkpoints/MYB-165/`

Metadata:
- `_bmad-output/visual-checkpoints/MYB-165/2026-06-18T07-05-30Z-capture-metadata.json`

Mode:
- capture

State:
- after

Execution:
- Mode: MYB-165-FirstTrueRoute
- Branch: `MYB-165-premier-vrai-parcours-jouable-3-minutes`
- Commit: `6b6991a`

## Scene

Scene:
- `Assets/Scenes/MYB89UnityMcpProbe.unity`

## Cameras

| Camera | Role | Found | Position | Rotation | FOV/Size | Notes |
|---|---|---:|---|---|---:|---|
| RouteCamera | blocking validation | Yes | (1.503, 1.49, 27.513) | (4.019, 349.28, 0) | 50 | perspective |
| OverviewCamera | secondary context | Yes | (2, 460, 1142.5) | (90, 0, 0) | 1260 | orthographic |

## Captures

| State | Type | Path | Scene | Camera | Resolution |
|---|---|---|---|---|---|
| after | route | `_bmad-output/visual-checkpoints/MYB-165/2026-06-18T07-05-30Z-after-route.png` | `Assets/Scenes/MYB89UnityMcpProbe.unity` | RouteCamera | 1600x900 |
| after | overview | `_bmad-output/visual-checkpoints/MYB-165/2026-06-18T07-05-30Z-after-overview.png` | `Assets/Scenes/MYB89UnityMcpProbe.unity` | OverviewCamera | 1600x900 |

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

| Code | Message |
|---|---|
| OVERVIEW_CAMERA_SIZE_NON_CANONICAL | OverviewCamera orthographic size is 1260; V1 default is 42. |

## Info

| Code | Message |
|---|---|
| CAPTURE_WRITTEN | route after capture written to `_bmad-output/visual-checkpoints/MYB-165/2026-06-18T07-05-30Z-after-route.png`. |
| CAPTURE_WRITTEN | overview after capture written to `_bmad-output/visual-checkpoints/MYB-165/2026-06-18T07-05-30Z-after-overview.png`. |
| COMPARISON_NOT_REQUESTED | route before/after comparison was not requested. |
| COMPARISON_NOT_REQUESTED | overview before/after comparison was not requested. |

## Verdict

- PASS_WITH_WARNINGS

RouteCamera is the blocking validation camera. OverviewCamera is required as secondary context. MYB-145 does not judge Premium target.

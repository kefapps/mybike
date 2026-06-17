# MYB-145 Capture Report

Ticket:
- `MYB-161`

Generated at:
- 2026-06-17T18:58:45.9334210Z

Output directory:
- `_bmad-output/visual-checkpoints/MYB-161/`

Metadata:
- `_bmad-output/visual-checkpoints/MYB-161/2026-06-17T18-58-45Z-capture-metadata.json`

Mode:
- capture

State:
- before

Execution:
- Mode: MYB-161-before
- Branch: `MYB-161-art-directed-golden-slice-composition`
- Commit: `a689aca`

## Scene

Scene:
- `Assets/Scenes/MYB160MeshyHeroCandidatePreview.unity`

## Cameras

| Camera | Role | Found | Position | Rotation | FOV/Size | Notes |
|---|---|---:|---|---|---:|---|
| RouteCamera | blocking validation | Yes | (1.934, 1.574, 7.5) | (0.541, 3.231, 0) | 50 | perspective |
| OverviewCamera | secondary context | Yes | (0, 86, 66) | (90, 0, 0) | 42 | orthographic |

## Captures

| State | Type | Path | Scene | Camera | Resolution |
|---|---|---|---|---|---|
| before | route | `_bmad-output/visual-checkpoints/MYB-161/2026-06-17T18-58-45Z-before-route.png` | `Assets/Scenes/MYB160MeshyHeroCandidatePreview.unity` | RouteCamera | 1600x900 |
| before | overview | `_bmad-output/visual-checkpoints/MYB-161/2026-06-17T18-58-45Z-before-overview.png` | `Assets/Scenes/MYB160MeshyHeroCandidatePreview.unity` | OverviewCamera | 1600x900 |

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
| CAPTURE_WRITTEN | route before capture written to `_bmad-output/visual-checkpoints/MYB-161/2026-06-17T18-58-45Z-before-route.png`. |
| CAPTURE_WRITTEN | overview before capture written to `_bmad-output/visual-checkpoints/MYB-161/2026-06-17T18-58-45Z-before-overview.png`. |
| COMPARISON_NOT_REQUESTED | route before/after comparison was not requested. |
| COMPARISON_NOT_REQUESTED | overview before/after comparison was not requested. |

## Verdict

- PASS

RouteCamera is the blocking validation camera. OverviewCamera is required as secondary context. MYB-145 does not judge Premium target.

# MYB-147 Capture Report

Ticket:
- MYB-147

Generated at:
- 2026-06-16T12:11:01Z

Output directory:
- `_bmad-output/visual-checkpoints/MYB-147/`

Mode:
- isolated kit preview

## Status

This report documents isolated preview evidence for `MYB_ForestKit_V0`.

Preview evidence only.
Not Premium target evidence.
Route-camera validation is deferred to MYB-148 / MYB-150 / MYB-151.

## Scene / Source

Source:
- `_bmad-output/implementation-artifacts/MYB-147/MYB_ForestKit_V0.blend`

Generation script:
- `_bmad-output/implementation-artifacts/MYB-147/generate_myb_forest_kit_v0.py`

Unity candidate directory:
- `unity/Echapee4D/Assets/Echappee/Art/Candidates/MYB_ForestKit_V0/`

Note:
- Blender MCP connection was unavailable in this environment, so the kit was generated with deterministic local Blender batch execution from the checked-in procedural script.
- No Meshy, Tripo, external text-to-3D, or external asset source was used.

## Cameras

| Camera | Role | Found | Position | Rotation | FOV | Notes |
|---|---|---:|---|---|---:|---|
| Preview camera | isolated preview only | Yes | Blender procedural preview camera | Blender procedural preview orientation | 55 | Used only for contact sheet preview. |
| RouteCamera | blocking validation | No | - | - | - | Not used by MYB-147. Route-camera validation is deferred. |
| OverviewCamera | secondary context | No | - | - | - | Not used by MYB-147. |

## Captures

| State | Type | Path | Scene | Camera | Resolution |
|---|---|---|---|---|---|
| current | isolated kit contact sheet | `_bmad-output/visual-checkpoints/MYB-147/2026-06-16T12-11-01Z-kit-contact-sheet.png` | `MYB_ForestKit_V0.blend` | Preview camera | 1800 x 1200 |

## Kit Coverage

| Family | Count | Notes |
|---|---:|---|
| Trunks | 4 | ancient, broken, leaning, knotted/fantasy scenic |
| Roots / arches | 3 | lateral, ground cluster, small arch |
| Rocks / mossy stones | 3 | two mossy rocks and one marker rock |
| Ferns / clumps | 3 | simple readable silhouettes |
| Leaf / moss mats | 3 | stylized forest floor enrichment pieces |
| Dead branches | 2 | near-road small silhouettes |
| Canopy masses | 2 | stylized readable masses |
| Fallen log | 1 | natural landmark / route edge candidate |

## Verdict

- PASS_WITH_WARNINGS

## Warnings

- This is isolated preview evidence only.
- No route-camera capture was produced by MYB-147.
- No canonical corridor scatter was performed.
- Final visual validation must happen in later tickets from the canonical route camera.

## Errors

- None.

# MYB-149 Implementation Report

Status:
- In Review / ready for human visual review when captures and MYB-144 are complete.

Generated at:
- 2026-06-17T09:20:28.8685530Z

## Summary

- MYB-149 creates a dedicated ground/material preview scene derived from MYB-148 after.
- The pass targets visible but controlled foreground transformation from RouteCamera.
- Premium target reached: No.
- Verdict: Checkpoint insuffisant.

## Source / Baseline

- Before: MYB-148 after.
- Source scene: `Assets/Scenes/MYB148RouteFirstScatterPreview.unity`
- Output scene: `Assets/Scenes/MYB149GroundMaterialPreview.unity`
- The MYB-148 preview scene is opened as source and saved as a new MYB-149 scene; MYB-148 is not overwritten.

## Builder

- Builder: `unity/Echapee4D/Assets/MYB149/Editor/MYB149GroundMaterialPreviewBuilder.cs`
- Generated root: `MYB149_GroundMaterialPreviewRoot`
- Manual edits are not the source of truth.
- Any kept tweak must be encoded back into builder parameters.

## Determinism

- Seed: `149001`
- Same source scene, seed and builder should recreate the same MYB-149 preview layout.

## Scene-local Asset Policy

- Ground materials and geometric patches are scene-local preview elements.
- New reusable assets created: No.
- Manifest changed: No.
- No manifest change required.
- No production promotion.
- No Poly Haven, Meshy, Tripo, or external text-to-3D source.

## Route Visual Treatment

Scope:
- scene-local preview only

Changed:
- route center: no
- route edges: yes
- shoulder transition: yes
- route geometry: no
- route collider: no
- gameplay trajectory: no
- production material asset modified: no

Reason:
- Improve route-to-shoulder material transition while keeping route readability dominant.

Readability impact:
- Intended no route readability regression vs MYB-148 after.

Risk:
- Human review must confirm edge feathering enriches the foreground without visual noise.

Verdict:
- no route readability regression detected by placement metrics

## Ground / Shoulder Pass

- Route edge feathering patches: 8
- Shoulder transition patches: 27
- Ground patches: 17
- Moss / leaf mats: 6
- Patches are grouped into readable masses, not uniform scatter.
- Off-camera and breathing-window zones are kept simpler.

## Asset Grounding Pass

Scope:
- scene-local grounding patches around existing MYB-148/MYB-147 assets

Rules:
- no re-scatter
- no destructive asset modification
- no MYB-148 scene modification
- no canonical scene modification
- no gameplay changes

Asset families grounded:
- trunks: 10
- roots: 12
- rocks: 16
- ferns: 8
- fallen logs / branches: 6

Assets considered:
- 40

Assets grounded:
- 30

Assets skipped:
- 10

Placement follow-ups:
- MYB148_myb_forest_trunk_leaning_a_back_wall_canopy_support_07: low route-camera value or breathing window.
- MYB148_myb_forest_trunk_knotted_a_silhouette_line_12: low route-camera value or breathing window.
- MYB148_myb_forest_trunk_ancient_a_mid_edge_14: low route-camera value or breathing window.
- MYB148_myb_forest_trunk_ancient_a_back_wall_canopy_support_19: low route-camera value or breathing window.
- MYB148_myb_forest_trunk_leaning_a_silhouette_line_27: low route-camera value or breathing window.
- MYB148_myb_forest_trunk_knotted_a_mid_edge_30: low route-camera value or breathing window.
- MYB148_myb_forest_trunk_ancient_a_silhouette_line_40: low route-camera value or breathing window.
- MYB148_myb_forest_trunk_leaning_a_back_wall_canopy_support_47: low route-camera value or breathing window.
- MYB148_myb_forest_rock_marker_a_mid_edge_landmark_60: low route-camera value or breathing window.
- MYB148_myb_forest_trunk_knotted_a_silhouette_line_56: low route-camera value or breathing window.

## Metrics

- Total scene-local patches: 110
- Scene-local material count: 7
- Generated renderers: 110
- Generated mesh filters: 110
- Generated triangles: 1647
- Minimum generated patch clearance from route center minus patch radius: 2.19 m
- Road half width: 2.05 m
- Clearance warning threshold: 2.3 m (`RoadHalfWidth + 0.25 m`)
- Clearance warning triggered: Yes
- Clearance note: patches remain outside the readable trajectory but close to the road edge; this is a non-blocking V1 warning because route overlap count is 0.
- Patches within near-route zone: 82
- Route overlap count: 0
- Metrics JSON: `_bmad-output/implementation-artifacts/MYB-149/myb-149-ground-material-metrics.json`

## Visual Rubric Delta

Baseline:
- MYB-148 after

Current:
- MYB-149 after

Visual scores are implementation estimates pending Julien human visual review.

| Criterion | MYB-148 after | MYB-149 after | Delta | Notes |
|---|---:|---:|---:|---|
| Route readability | 3 | 3 | 0 | Route remains the primary readable surface; final judgment requires route before/after review. |
| Foreground richness | 3 | 3.3 | +0.3 | Scene-local patches create a visible foreground delta from RouteCamera. |
| Material coherence | 3 | 3.2 | +0.2 | Moss/leaf/soil patches improve grounding and route-edge transitions without production promotion. |
| Scale credibility | 3 | 3 | 0 | Grounding patches preserve route clearance and avoid asset movement. |
| Composition rhythm | 3 | 3 | 0 | Grouped patches and breathing windows aim to avoid uniform noise. |

Target:
- Foreground richness >= 3.
- Material coherence improves.
- Route readability does not regress.
- No uniform noise.

## Visual Evidence

- Visual checkpoint directory: `_bmad-output/visual-checkpoints/MYB-149/`
- Before capture report: `_bmad-output/visual-checkpoints/MYB-149/2026-06-17T09-20-31Z-capture-report.md`
- After capture report: `_bmad-output/visual-checkpoints/MYB-149/2026-06-17T09-20-32Z-capture-report.md`
- Route comparison: `_bmad-output/visual-checkpoints/MYB-149/2026-06-17T09-20-33Z-route-before-after.png`
- Overview comparison: `_bmad-output/visual-checkpoints/MYB-149/2026-06-17T09-20-33Z-overview-before-after.png`
- Capture report: `_bmad-output/visual-checkpoints/MYB-149/2026-06-17T09-20-33Z-before-after-capture-report.md`

Captures:
- `_bmad-output/visual-checkpoints/MYB-149/2026-06-17T09-20-31Z-before-route.png`
- `_bmad-output/visual-checkpoints/MYB-149/2026-06-17T09-20-31Z-before-overview.png`
- `_bmad-output/visual-checkpoints/MYB-149/2026-06-17T09-20-32Z-after-route.png`
- `_bmad-output/visual-checkpoints/MYB-149/2026-06-17T09-20-32Z-after-overview.png`

## MYB-144 Validation

MYB-144:
- Verdict: PASS
- Errors: 0
- Warnings: 0
- Info: 27
- Report: `_bmad-output/unity-test-results/myb-144-art-asset-validator-report.md`

Manifest:
- Changed: No
- New reusable assets created: No
- No manifest change required: Yes

Reason:
- MYB-149 V1 generated scene-local materials and patches only inside `MYB149GroundMaterialPreview`.

## Warning Categories

### Build / Capture Warnings

- None.

### MYB-149 Visual Warnings

- Human review must confirm foreground richness and material coherence from route before/after evidence.
- Large foreground patches are intentionally visible but may need art-direction tuning before any production promotion.
- Route-edge patches remain outside the readable trajectory but sit close to the road edge: minimum clearance 2.19 m is below warning threshold 2.3 m (RoadHalfWidth + 0.25 m).

### MYB-149 Asset / Manifest Warnings

- None.

### MYB-144 Existing Validator Warnings

- None.

### Blocking Errors

- None.


## Governance

- Deterministic builder used.
- Manual edits are not source of truth.
- MYB-148 scene not modified.
- Canonical ride scene not modified.
- Gameplay not modified.
- Route geometry, trajectory and colliders not modified.
- No Meshy / Tripo / text-to-3D.
- No Poly Haven or third-party source.
- No production promotion.
- Materials/patches are scene-local.
- Premium target reached: No.
- Checkpoint insuffisant.
- MYB-149 accepted as checkpoint with reservations; MYB-157 covers the follow-up
  visual-support fix.

## Verdict

- PASS_WITH_WARNINGS
- Ticket status: accepted checkpoint with reservations; not `Premium target`.

# MYB-149 Implementation Report

Status:
- In Review / ready for human visual review when captures and MYB-144 are complete.

Generated at:
- 2026-06-16T20:08:10.5392050Z

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
- 37

Assets grounded:
- 30

Assets skipped:
- 7

Placement follow-ups:
- MYB148_myb_forest_trunk_knotted_a_silhouette_line_11: low route-camera value or breathing window.
- MYB148_myb_forest_trunk_ancient_a_mid_edge_13: low route-camera value or breathing window.
- MYB148_myb_forest_trunk_leaning_a_silhouette_line_25: low route-camera value or breathing window.
- MYB148_myb_forest_trunk_knotted_a_mid_edge_28: low route-camera value or breathing window.
- MYB148_myb_forest_trunk_ancient_a_silhouette_line_38: low route-camera value or breathing window.
- MYB148_myb_forest_rock_marker_a_mid_edge_landmark_57: low route-camera value or breathing window.
- MYB148_myb_forest_trunk_knotted_a_silhouette_line_53: low route-camera value or breathing window.

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
- Before capture report: `_bmad-output/visual-checkpoints/MYB-149/2026-06-16T20-08-11Z-capture-report.md`
- After capture report: `_bmad-output/visual-checkpoints/MYB-149/2026-06-16T20-08-16Z-capture-report.md`
- Route comparison: `_bmad-output/visual-checkpoints/MYB-149/2026-06-16T20-08-16Z-route-before-after.png`
- Overview comparison: `_bmad-output/visual-checkpoints/MYB-149/2026-06-16T20-08-16Z-overview-before-after.png`
- Capture report: `_bmad-output/visual-checkpoints/MYB-149/2026-06-16T20-08-17Z-before-after-capture-report.md`

Captures:
- `_bmad-output/visual-checkpoints/MYB-149/2026-06-16T20-08-11Z-before-route.png`
- `_bmad-output/visual-checkpoints/MYB-149/2026-06-16T20-08-11Z-before-overview.png`
- `_bmad-output/visual-checkpoints/MYB-149/2026-06-16T20-08-16Z-after-route.png`
- `_bmad-output/visual-checkpoints/MYB-149/2026-06-16T20-08-16Z-after-overview.png`

## MYB-144 Validation

MYB-144:
- Verdict: PASS
- Errors: 0
- Warnings: 0
- Info: 26
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

- Julien accepted the checkpoint with reservations on 2026-06-16.
- The result is still not visually strong: "pas tres beau" remains the human review note.
- Some objects read as trees floating in the sky; this must be investigated in the next visual cleanup / scatter validation pass.
- Large foreground patches are intentionally visible but may need art-direction tuning before any production promotion.
- Route-edge patches remain outside the readable trajectory but sit close to the road edge: minimum clearance 2.19 m is below warning threshold 2.3 m (RoadHalfWidth + 0.25 m).

### MYB-149 Asset / Manifest Warnings

- None.

### MYB-144 Existing Validator Warnings

- None.

### Blocking Errors

- None.


## Human Visual Review

Reviewer:
- Julien

Decision:
- Accepted as MYB-149 checkpoint with reservations.

Human review notes:
- The result is accepted, but it is not very beautiful yet.
- Some objects appear to float in the sky even though they read as trees.

Interpretation:
- MYB-149 may be accepted as a ground/material checkpoint.
- This is not `Premium target` evidence.
- The floating tree/object issue must be treated as follow-up visual/scatter cleanup before any production-quality claim.
- Future tickets must not use this acceptance to hide the remaining visual defects.

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
- Human checkpoint accepted with reservations by Julien on 2026-06-16.

## Verdict

- PASS_WITH_WARNINGS
- Ticket status: accepted checkpoint with reservations; ready for closure after PR merge if the reservations are preserved in follow-up planning.

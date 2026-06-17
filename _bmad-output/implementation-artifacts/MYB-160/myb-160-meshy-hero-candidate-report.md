# MYB-160 Meshy Hero Candidate Report

## Summary

MYB-160 creates controlled Meshy hero candidates for the MYB-159 golden slice. The goal is not to generate a forest; it is to test whether one stronger tree assembly and one root arch threshold improve the route-camera silhouette problem that remained after MYB-158 and MYB-159.

Closure note:
Julien validated the downstream MYB-161 revised checkpoint on 2026-06-18. MYB-160 is accepted as a controlled candidate/support pass for that validated direction. The Meshy assets remain candidate/preview-only and are not production-promoted.

## Scope

- 2 Meshy-6 preview generations authorized by Julien.
- 2 candidates selected and cleaned locally in Blender.
- 0 candidates rejected.
- 0 Meshy refine/remesh/retexture calls.
- 0 production promotions.
- Optional stump/root/rock marker not generated to keep the spend bounded until route-camera evidence is reviewed.

## Source / Baseline

- before = MYB-159 after
- before scene: `Assets/Scenes/MYB159GoldenForestSlicePreview.unity`
- before route: `_bmad-output/visual-checkpoints/MYB-160/2026-06-17T17-09-54Z-before-route.png`
- before overview: `_bmad-output/visual-checkpoints/MYB-160/2026-06-17T17-09-54Z-before-overview.png`

## Builder

- path: `unity/Echapee4D/Assets/MYB160/Editor/MYB160MeshyHeroCandidateBuilder.cs`
- seed: 160006
- generated root: `MYB160_MeshyHeroCandidateRoot`
- output scene: `Assets/Scenes/MYB160MeshyHeroCandidatePreview.unity`
- baseline objects disabled in MYB-160 preview only:
- MYB159_tree_assembly_hero_left_leaning_crown (replaced by cleaned Meshy ancient tree candidate)
- MYB159_hero_root_threshold_left (replaced by cleaned Meshy root arch candidate)

## Meshy Usage

| Candidate | Task ID | Cost | Status | Preview Use |
|---|---|---:|---|---|
| Ancient tree assembly | `019ed672-6ca2-7c48-803d-fcc6e62fa15d` | 20 credits | selected | route-side tree candidate |
| Root arch threshold | `019ed672-73fb-7f12-a508-9884b5cdadb2` | 20 credits | selected | route-side hero threshold candidate |

- Meshy generated count: 2
- Meshy used in preview count: 2
- Total Meshy credits used for MYB-160: 40
- No further credit-costing Meshy tools were called.

## Blender Cleanup

- script: `_bmad-output/implementation-artifacts/MYB-160/blender/myb160_clean_meshy_candidates.py`
- tree metrics: `_bmad-output/implementation-artifacts/MYB-160/blender/myb160_meshy_tree_ancient_a_blender_metrics.json`
- root arch metrics: `_bmad-output/implementation-artifacts/MYB-160/blender/myb160_meshy_root_arch_a_blender_metrics.json`
- tree preview: `_bmad-output/implementation-artifacts/MYB-160/blender/myb160_meshy_tree_ancient_a_cleaned_preview.png`
- root arch preview: `_bmad-output/implementation-artifacts/MYB-160/blender/myb160_meshy_root_arch_a_cleaned_preview.png`
- cleanup actions: apply transforms, remove tiny fragments, set bottom near origin, decimate to candidate budgets, assign simple material zones, export Unity-ready FBX.

## Asset Intake / Manifest

- manifest changed: Yes
- entries added: 2
- intakeStatus: approved
- promotionStatus: candidate
- no `reviewStatus` introduced
- no `example:true` introduced
- no promoted assets
- license: `Provider terms pending project review`

## Candidate Composition

### myb160_meshy_tree_ancient_a

- family: Ancient tree assembly Meshy candidate
- asset: `Assets/Echappee/Art/Candidates/MYB160/Meshy/Cleaned/myb160_meshy_tree_ancient_a_cleaned.fbx`
- task: `019ed672-6ca2-7c48-803d-fcc6e62fa15d`
- used in preview: Yes
- route visible: Yes
- triangles: 30000
- renderers: 1
- materials: 4
- dimensions after Unity placement scale: (8.366, 5.244, 9.281)m
- notes: Selected: strong trunk silhouette, wide rooted base, supported canopy, useful replacement for a weak MYB-159 procedural tree.

### myb160_meshy_root_arch_a

- family: Root arch threshold Meshy candidate
- asset: `Assets/Echappee/Art/Candidates/MYB160/Meshy/Cleaned/myb160_meshy_root_arch_a_cleaned.fbx`
- task: `019ed672-73fb-7f12-a508-9884b5cdadb2`
- used in preview: Yes
- route visible: Yes
- triangles: 24999
- renderers: 1
- materials: 3
- dimensions after Unity placement scale: (4.189, 2.594, 3.599)m
- notes: Selected: readable natural threshold silhouette, grounded root mass, useful hero beat without becoming the whole corridor.

## Ground Placement / Anti-Float

- method: instantiate/apply transform, compute combined renderer bounds, correct by bounds.min.y.
- sink: 0.03m
- floatingAssetCount: 0
- routeVisibleFloatingAssetCount: 0
- maxFloatingClearance: 0m
- sinkingAssetCount: 0
- maxSinkingDepth: 0.03m

## Metrics

- JSON: `_bmad-output/implementation-artifacts/MYB-160/myb-160-meshy-candidate-metrics.json`
- approximateTriangles: 55035
- rendererCount: 4
- meshFilterCount: 4
- routeOverlapCount: 0
- minimumRouteClearanceMeters: 2.95
- thumbnailForestRead: warning
- emptySkyOrFlatBackgroundRisk: medium

## Visual Evidence

- before route: `_bmad-output/visual-checkpoints/MYB-160/2026-06-17T17-09-54Z-before-route.png`
- before overview: `_bmad-output/visual-checkpoints/MYB-160/2026-06-17T17-09-54Z-before-overview.png`
- after route: `_bmad-output/visual-checkpoints/MYB-160/2026-06-17T17-09-56Z-after-route.png`
- after overview: `_bmad-output/visual-checkpoints/MYB-160/2026-06-17T17-09-56Z-after-overview.png`
- route comparison: `_bmad-output/visual-checkpoints/MYB-160/2026-06-17T17-09-56Z-route-before-after.png`
- overview comparison: `_bmad-output/visual-checkpoints/MYB-160/2026-06-17T17-09-56Z-overview-before-after.png`
- capture report: `_bmad-output/visual-checkpoints/MYB-160/2026-06-17T17-09-56Z-before-after-capture-report.md`

## MYB-144 Validation

- verdict: PASS
- errors: 0
- warnings: 0
- report: `_bmad-output/unity-test-results/myb-144-art-asset-validator-report.md`

## Visual Rubric Estimate

Visual scores are implementation estimates retained as context after the downstream MYB-161 human validation. MYB-160 is accepted as a support pass, not as standalone Premium target evidence.

| Criterion | Estimate | Notes |
|---|---:|---|
| Route readability | 4 | Candidate placements preserve route clearance and do not overlap the road. |
| Silhouette quality | 4 | Meshy candidates have stronger organic silhouettes than the replaced procedural placeholders. |
| Lighting mood | 3 | MYB-159 mood is carried forward; no fog masking was added. |
| Material coherence | 3 | Simple candidate materials are coherent enough for preview, not final production. |
| Foreground richness | 4 | Grounded tree/root forms add stronger near-route anchors. |
| Midground density | 4 | MYB-159 back wall remains; MYB-160 improves hero forms only. |
| Background depth | 3 | Not addressed by Meshy candidates. |
| Scale credibility | 4 | Combined-bounds grounding and route clearance metrics pass. |
| Composition rhythm | 4 | Two hero beats frame the authored slice without scatter. |

## Warning Categories

### Build / Capture Warnings

- None recorded.

### MYB-160 Visual Warnings

- Premium target intentionally not claimed; MYB-160 only tests controlled Meshy candidates inside the MYB-159 golden slice.
- Julien validated the downstream MYB-161 revised checkpoint on 2026-06-18; MYB-160 is accepted as a support pass for that direction.
- Route-camera impact is directionally better but still modest; candidates need Julien review plus a later lighting/material composition pass before any production claim.

### MYB-160 Asset / Manifest Warnings

- Meshy license remains `Provider terms pending project review`; candidates stay non-promoted.
- No refine/remesh/retexture Meshy calls were used; Blender cleanup used local decimation and simple materials only.
- Unity FBX import did not preserve Blender material colors reliably; MYB-160 applies scene-local preview material remapping in the builder.
- Optional stump/root/rock marker was not generated because the two authorized Meshy-6 candidates covered the main route-camera needs.

### MYB-144 Existing Validator Warnings

- None recorded.

### Blocking Errors

- None recorded.

## Governance

- no canonical scene modified
- no gameplay modified
- no route collider/trajectory change
- no production promotion
- Meshy controlled usage only: 2 Meshy-6 preview generations
- no silent third-party source
- Poly Haven not used
- Premium target reached: No

## Verdict

- Premium target reached: No
- Checkpoint insuffisant, Meshy candidates accepted as support for the MYB-161 validated direction
- Recommended Linear status: Done

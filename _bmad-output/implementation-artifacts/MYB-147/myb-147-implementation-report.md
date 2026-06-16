# MYB-147 Implementation Report

Status: accepted candidate kit v0 with warnings.

Linear target status:
- In Review

Human visual review status:
- Accepted as candidate kit v0 with warnings.

## Summary

MYB-147 produced and reworked `MYB_ForestKit_V0`, a procedural forest kit candidate for the Art Rescue forest corridor.

The first pass was structurally correct but not strong enough for `Stylise Premium de Production`: trunks read too much like poles, root clusters were too plank-like, ferns and moss mats were too symbolic/flat, canopy masses read as generic blobs, and the fantasy scenic signal was weak.

This revision keeps the same kit families and improves silhouette intent without adding scatter, production promotion, Meshy, Tripo, text-to-3D, external assets, shaders, or canonical scene changes.

Final human decision:
- MYB-147 current ForestKit V0 is accepted as a candidate kit with warnings.
- It is acceptable for MYB-148 / MYB-150 route-camera testing.
- It is not `Premium target` evidence and must not be promoted to production.
- Do not keep polishing the Blender assets unless required by validator `ERROR`.

The kit contains 21 Unity-ready FBX assets across the requested families:
- 4 irregular trunks;
- 3 root clusters / small arch pieces;
- 3 mossy rocks / stones;
- 3 ferns / vegetation clumps;
- 3 leaf / moss mats;
- 2 dead branches;
- 2 stylized canopy masses;
- 1 fallen log.

Candidate import path:
- `unity/Echapee4D/Assets/Echappee/Art/Candidates/MYB_ForestKit_V0/`

## Rework Applied

P0 trunks:
- Added wider bases, buttress roots, stronger asymmetry, bark ridge planes, knots and broken silhouette language.
- Pushed the `trunk_knotted_a` direction across the trunk family.

P0 root clusters:
- Replaced straighter root rods with multi-segment tapered curves.
- Added stronger ground contact and organic spread.
- Reworked `root_arch_a` as a clearer scenic threshold signal while keeping it simple and route-readable.

P1 ferns / moss mats / canopy:
- Enlarged fern silhouettes for better read at route-camera distance.
- Gave moss mats more organic outlines, slight height variation, moss patches and leaf ridges.
- Rebuilt canopy masses from varied lobes and leaf shelf planes to avoid a single generic green blob.

P2 rocks:
- Added stronger asymmetry, side moss and a subtle marker lip on the marker rock.

## Generation

Source / procedure:
- `_bmad-output/implementation-artifacts/MYB-147/generate_myb_forest_kit_v0.py`

Blender source:
- `_bmad-output/implementation-artifacts/MYB-147/MYB_ForestKit_V0.blend`

Local kit manifests:
- `_bmad-output/implementation-artifacts/MYB-147/myb-forest-kit-v0-manifest.json`
- `_bmad-output/implementation-artifacts/MYB-147/myb-forest-kit-v0-manifest.md`

Generation note:
- The kit was generated with deterministic local Blender batch execution using the checked-in procedural script.
- No external asset source, Meshy, Tripo, or external text-to-3D generation was used.

Export format:
- FBX

Reason:
- Static Unity-ready candidate models, no stronger existing project convention found for this ticket, and MYB-144 can scan the resulting FBX paths directly.

## Asset Summary

| Family | Count | Total triangles | Max triangles | Max materials |
|---|---:|---:|---:|---:|
| Trunks | 4 | 2272 | 628 | 2 |
| Roots / arches | 3 | 2372 | 852 | 2 |
| Rocks / mossy stones | 3 | 376 | 136 | 2 |
| Ferns / clumps | 3 | 204 | 68 | 2 |
| Leaf / moss mats | 3 | 264 | 88 | 2 |
| Dead branches | 2 | 128 | 64 | 2 |
| Canopy masses | 2 | 972 | 486 | 2 |
| Fallen log | 1 | 116 | 116 | 2 |

Overall:
- assets: 21
- total triangles: 6704
- maximum triangles on one asset: 852
- maximum material count on one asset: 2

All assets stay under the V0 fallback triangle budgets documented for MYB-147.

## Asset List

| Asset id | Family | Unity path | Dimensions m | Triangles | Materials |
|---|---|---|---:|---:|---:|
| `myb_forest_trunk_ancient_a` | trunks | `Assets/Echappee/Art/Candidates/MYB_ForestKit_V0/myb_forest_trunk_ancient_a.fbx` | 2.615 x 2.309 x 3.4 | 580 / 1200 | 2 |
| `myb_forest_trunk_broken_a` | trunks | `Assets/Echappee/Art/Candidates/MYB_ForestKit_V0/myb_forest_trunk_broken_a.fbx` | 2.576 x 2.269 x 2.623 | 532 / 1200 | 2 |
| `myb_forest_trunk_leaning_a` | trunks | `Assets/Echappee/Art/Candidates/MYB_ForestKit_V0/myb_forest_trunk_leaning_a.fbx` | 2.538 x 2.229 x 3.1 | 532 / 1200 | 2 |
| `myb_forest_trunk_knotted_a` | trunks | `Assets/Echappee/Art/Candidates/MYB_ForestKit_V0/myb_forest_trunk_knotted_a.fbx` | 2.845 x 2.546 x 3.021 | 628 / 1200 | 2 |
| `myb_forest_root_cluster_lateral_a` | roots | `Assets/Echappee/Art/Candidates/MYB_ForestKit_V0/myb_forest_root_cluster_lateral_a.fbx` | 2.403 x 1.265 x 0.417 | 712 / 900 | 2 |
| `myb_forest_root_cluster_ground_a` | roots | `Assets/Echappee/Art/Candidates/MYB_ForestKit_V0/myb_forest_root_cluster_ground_a.fbx` | 2.53 x 2.073 x 0.419 | 808 / 900 | 2 |
| `myb_forest_root_arch_a` | roots | `Assets/Echappee/Art/Candidates/MYB_ForestKit_V0/myb_forest_root_arch_a.fbx` | 3.28 x 1.451 x 1.502 | 852 / 900 | 2 |
| `myb_forest_rock_mossy_a` | rocks | `Assets/Echappee/Art/Candidates/MYB_ForestKit_V0/myb_forest_rock_mossy_a.fbx` | 1.928 x 1.276 x 1.0 | 120 / 700 | 2 |
| `myb_forest_rock_mossy_b` | rocks | `Assets/Echappee/Art/Candidates/MYB_ForestKit_V0/myb_forest_rock_mossy_b.fbx` | 1.504 x 1.467 x 1.0 | 120 / 700 | 2 |
| `myb_forest_rock_marker_a` | rocks | `Assets/Echappee/Art/Candidates/MYB_ForestKit_V0/myb_forest_rock_marker_a.fbx` | 1.549 x 1.389 x 2.049 | 136 / 700 | 2 |
| `myb_forest_fern_a` | ferns | `Assets/Echappee/Art/Candidates/MYB_ForestKit_V0/myb_forest_fern_a.fbx` | 0.617 x 0.823 x 0.844 | 68 / 600 | 2 |
| `myb_forest_fern_b` | ferns | `Assets/Echappee/Art/Candidates/MYB_ForestKit_V0/myb_forest_fern_b.fbx` | 0.538 x 0.675 x 0.689 | 68 / 600 | 2 |
| `myb_forest_fern_c` | ferns | `Assets/Echappee/Art/Candidates/MYB_ForestKit_V0/myb_forest_fern_c.fbx` | 0.669 x 0.922 x 0.947 | 68 / 600 | 2 |
| `myb_forest_leaf_moss_mat_a` | floor_mats | `Assets/Echappee/Art/Candidates/MYB_ForestKit_V0/myb_forest_leaf_moss_mat_a.fbx` | 1.845 x 1.103 x 0.122 | 88 / 200 | 2 |
| `myb_forest_leaf_moss_mat_b` | floor_mats | `Assets/Echappee/Art/Candidates/MYB_ForestKit_V0/myb_forest_leaf_moss_mat_b.fbx` | 1.664 x 1.238 x 0.122 | 88 / 200 | 2 |
| `myb_forest_leaf_moss_mat_c` | floor_mats | `Assets/Echappee/Art/Candidates/MYB_ForestKit_V0/myb_forest_leaf_moss_mat_c.fbx` | 2.27 x 0.891 x 0.122 | 88 / 200 | 2 |
| `myb_forest_dead_branch_a` | branches | `Assets/Echappee/Art/Candidates/MYB_ForestKit_V0/myb_forest_dead_branch_a.fbx` | 1.31 x 0.758 x 0.241 | 64 / 400 | 2 |
| `myb_forest_dead_branch_b` | branches | `Assets/Echappee/Art/Candidates/MYB_ForestKit_V0/myb_forest_dead_branch_b.fbx` | 1.31 x 0.756 x 0.241 | 64 / 400 | 2 |
| `myb_forest_canopy_mass_a` | canopy | `Assets/Echappee/Art/Candidates/MYB_ForestKit_V0/myb_forest_canopy_mass_a.fbx` | 2.766 x 1.708 x 1.05 | 486 / 1000 | 2 |
| `myb_forest_canopy_mass_b` | canopy | `Assets/Echappee/Art/Candidates/MYB_ForestKit_V0/myb_forest_canopy_mass_b.fbx` | 2.784 x 1.804 x 1.05 | 486 / 1000 | 2 |
| `myb_forest_fallen_log_a` | logs | `Assets/Echappee/Art/Candidates/MYB_ForestKit_V0/myb_forest_fallen_log_a.fbx` | 2.737 x 0.846 x 0.809 | 116 / 1200 | 2 |

## Unity Import

Candidate import path:
- `unity/Echapee4D/Assets/Echappee/Art/Candidates/MYB_ForestKit_V0/`

Unity generated `.meta` files for the FBX candidates during MYB-144 batch validation.

No scene file was created or modified for MYB-147.

## Canonical Manifest

Canonical manifest:
- `docs/manifests/art-rescue-asset-manifest.json`

Manifest status:
- 21 MYB-147 entries added / updated.
- `sourceType`: `internal`
- `provider`: `Blender MCP / procedural`
- `intakeStatus`: `approved`
- `promotionStatus`: `candidate`
- `usageScope`: `forest_corridor`
- `aiGenerated`: `false`
- `license`: `Project-owned`
- `author`: `Kefapps / procedural Blender MCP`

Important:
- `approved` intake does not mean production promotion.
- No MYB-147 asset is marked `promotionStatus: promoted`.
- No `reviewStatus` field is used.
- No `example: true` field is used.

## MYB-144 Validator

Command:

```bash
/Applications/Unity/Hub/Editor/6000.4.10f1/Unity.app/Contents/MacOS/Unity \
  -batchmode \
  -quit \
  -projectPath unity/Echapee4D \
  -executeMethod MYB144ArtAssetValidator.RunBatch \
  -logFile _bmad-output/unity-test-results/myb-147-myb144-validator.log
```

Validator report:
- `_bmad-output/unity-test-results/myb-144-art-asset-validator-report.md`

Result:
- Verdict: `PASS_WITH_WARNINGS`
- Errors: 0
- Warnings: 211
- Info: 24
- Batch exit code: 0

Warning summary:
- 211 `ASSET_CANDIDATE_UNMANIFESTED` warnings from pre-existing scanned Art Rescue / MYB asset roots.
- No warning or error references `MYB_ForestKit_V0` or `myb_forest_`.

MYB-147 readiness note:
- MYB-144 produced no blocking ERROR.
- The warning debt is historical / cross-root manifest coverage noise and is not introduced by the MYB-147 kit.

## Visual Evidence

Preview contact sheet:
- `_bmad-output/visual-checkpoints/MYB-147/2026-06-16T14-16-29Z-kit-contact-sheet.png`

Capture report:
- `_bmad-output/visual-checkpoints/MYB-147/2026-06-16T14-16-29Z-capture-report.md`

Review page:
- `_bmad-output/visual-checkpoints/MYB-147/myb-147-review.html`

Evidence status:
- Preview evidence only.
- Not `Premium target` evidence.
- Route-camera validation is deferred to MYB-148 / MYB-150 / MYB-151.

## Known Warnings / Limitations

- Human review warning: canopy masses still read as generic green blobs.
- Human review warning: rocks remain usable but generic.
- Human review warning: trunk material language is still simple.
- Human review warning: root clusters may need better ground integration during scatter.
- Human review warning: isolated preview does not validate route-camera quality.
- The kit is asset-level candidate material and has not been scattered into the canonical ride corridor.
- No route-camera validation was performed in MYB-147.
- No final art claim is made from the isolated contact sheet.
- The generator emits a Blender API deprecation warning about `Material.use_nodes`; this does not affect the generated FBX output for MYB-147.
- MYB-147 stays `In Review`; it is not closed as `Done` by this candidate acceptance.

## Governance

- No Meshy / Tripo used.
- No external text-to-3D generation used.
- No external asset source used.
- No canonical corridor scatter performed.
- No production promotion performed.
- Assets are candidate kit pieces, not promoted production.
- Isolated previews are intermediate evidence only.
- Route-camera validation is deferred to MYB-148 / MYB-150 / MYB-151.
- Linear status should remain In Review, not Done.

## Final Status Rule

MYB-147 is candidate-kit-v0 complete when the candidate kit, local manifests,
canonical manifest entries, isolated preview evidence, and MYB-144 validation
evidence are present with no validator `ERROR`.

It remains `In Review`, not `Done`. Future route-camera validation belongs to
MYB-148 / MYB-150.

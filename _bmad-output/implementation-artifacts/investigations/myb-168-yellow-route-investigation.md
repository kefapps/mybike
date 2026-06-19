# Investigation: MYB-168 Yellow Route / Trees Too Far

## Hand-off Brief

1. **What happened.** Julien rejected PR #32 because the final MYB-168 route capture is analytically clean but visually worse: trees read too far from the road and the image has shifted toward yellow/orange.
2. **Where the case stands.** Root cause split is now high-confidence. The yellow/open look is already present in MYB-165, before the final MYB-168 all-warning cleanup. The tree-distance complaint is caused by the cleanup strategy and guardrail interpretation: valid close forest framing was treated as route-readability debt.
3. **What's needed next.** Revise PR #32 rather than merge it: restore close tree framing, remove only road/horizon masking geometry, and update MYB-167 so it distinguishes acceptable close forest from actual route obstruction.

## Case Info

| Field | Value |
|---|---|
| Ticket | MYB-168 |
| Date opened | 2026-06-19 |
| Status | Active |
| System | Unity `unity/Echapee4D`, branch `MYB-168-fix-closeleftframe-canopy-route-readability`, commit `f88845b` |
| Evidence sources | User visual rejection, MYB-168 captures, MYB-167 metrics/report, git diff, Unity scene |

## Problem Statement

User report: "Non validé. Les arbres peuvent être bien plus proches de la route. Tout est devenu jaune. Invesstigue"

## Evidence Inventory

| Source | Status | Notes |
|---|---|---|
| User visual report | Available | Rejects PR #32 and identifies two symptoms: trees too far, image too yellow. |
| Final MYB-168 route capture | Available | `_bmad-output/visual-checkpoints/MYB-168/2026-06-19T15-28-23Z-after-route.png`. |
| Final MYB-168 overview capture | Available | `_bmad-output/visual-checkpoints/MYB-168/2026-06-19T15-28-23Z-after-overview.png`. |
| MYB-167 metrics/report | Available | Shows validation `PASS` with zero route-camera warnings after the rejected visual state. |
| Git diff / commit | Available | Commit `f88845b` contains the visual-relevant generator changes. |
| Older comparable route captures | Available | MYB-163 is dark/close/green; MYB-165 is already yellow/open and very close to MYB-168. |
| `project-context.md` persistent facts | Missing | No matching file found in repo. |

## Investigation Backlog

| # | Path to Explore | Priority | Status | Notes |
|---|---|---|---|---|
| 1 | Compare current MYB-168 route capture against earlier route captures | High | Done | MYB-165 already has the yellow/open look; MYB-163 is the close/green reference. |
| 2 | Inspect commit `f88845b` source changes for spacing/color/camera effects | High | Done | Final cleanup moved/reshaped route-visible trees and warning objects; no evidence of direct material recolor. |
| 3 | Inspect scene/capture camera differences | Medium | Done | MYB-163 and MYB-165/MYB-168 route captures use different route-camera positions. |
| 4 | Inspect material/lighting diffs | Medium | Done | MYB-165 material constants are mostly green/brown; scene still uses warm MYB89 sun/sky. |
| 5 | Reconcile MYB-167 guardrail with acceptable near-tree composition | High | Done | Validator warning model does not encode desired close-forest enclosure. |
| 6 | Inspect current tree material/texture quality | High | Done | Canonical scene is dominated by flat procedural tree materials; existing PremiumTree textured assets are not referenced. |

## Timeline of Events

| Time | Event | Source | Confidence |
|---|---|---|---|
| 2026-06-19 15:40 UTC | MYB-168 cleanup produced MYB-167 `PASS` with zero route-camera warnings. | Memory / local metrics | Confirmed |
| 2026-06-19 15:28 UTC | Final route and overview captures generated. | MYB-145 report | Confirmed |
| 2026-06-19 16:03 UTC | Commit `f88845b` pushed and PR #32 opened. | Git/Gitea | Confirmed |
| 2026-06-19 16:06 UTC | Julien rejected the visual result. | User message | Confirmed |

## Confirmed Findings

### Finding 1: The analytical gate is green on a visually rejected state

**Evidence:** `_bmad-output/implementation-artifacts/MYB-167/myb-167-route-visible-support-report.md`, final MYB-168 capture.

**Detail:** MYB-167 reports no route-camera warnings, while Julien reports the result is not acceptable.

### Finding 2: The yellow/open route image predates the final MYB-168 cleanup

**Evidence:** Route captures for MYB-163, MYB-165, and MYB-168.

**Detail:** MYB-163 route capture is dark green and enclosed. MYB-165 route capture is already warm/yellow, open, and visually very close to MYB-168. Final MYB-168 did not create the base yellow palette; it preserved an MYB-165-era look.

**Quantification:** Average image color moved from MYB-163 `srgb(19.7%,22.4%,19.7%)` to MYB-165 `srgb(58.1%,51.1%,42.6%)`; MYB-168 is effectively the same at `srgb(58.1%,51.3%,42.8%)`. Lower-frame color follows the same pattern.

### Finding 3: The compared route captures are not the same camera moment

**Evidence:** MYB-163 and MYB-165/MYB-168 capture reports.

**Detail:** MYB-163 route capture uses camera position `(-0.85, 1.55, 7.5)` and rotation `(0.664, 1.411, 0)`. MYB-165/MYB-168 use position `(1.503, 1.49, 27.513)` and rotation `(4.019, 349.28, 0)`. The before/after visual comparison therefore mixed route-camera positions and can hide regressions.

### Finding 4: The MYB-167 zero-warning target over-constrained valid close trees

**Evidence:** `MYB167RouteVisibleSupportValidator.cs` and final MYB-168 cleanup behavior.

**Detail:** MYB-167 uses broad protected viewport overlap and route-corridor proximity warnings. The final cleanup treated every warning as something to eliminate, so close forest masses were moved/reshaped farther from the route. Julien's rejection confirms close trees are acceptable, and the real problem is road/horizon masking, not proximity itself.

### Finding 5: No direct final-commit material recolor explains the yellow result

**Evidence:** Final commit scope, MYB-165 builder materials, and scene lighting trace.

**Detail:** The MYB-165 material constants remain green/brown (`ground`, `meadow`, `leaf`) with warm brown shoulders. The scene uses a warm key sun (`MYB89_KeySun`, color `(1, 0.92, 0.78)`, intensity `1.08`) and sky/fog configuration. The yellow result appears to come from camera/framing, exposed road/shoulder/ground bands, lighting, and long-route scene composition rather than a single accidental material color change in MYB-168.

### Finding 6: Current route trees mostly use flat color materials

**Evidence:** Material files under `Assets/MYB89/Materials`, `Assets/MYB163/Materials`, and `Assets/MYB165/Materials`.

**Detail:** MYB89, MYB163, and MYB165 tree/leaf/bark materials have texture slots serialized but no nonzero texture references. They are URP Lit flat-color materials with smoothness values, not textured bark/leaf assets.

**Implication:** Even if placement is corrected, close trees will still read as simplified procedural volumes unless foreground trees receive better materials or premium assets.

### Finding 7: A reusable textured premium tree set already exists but is not used in the canonical scene

**Evidence:** `Assets/Echappee/Art/PremiumTreePolyHaven`, MYB-112 report, and scene GUID search.

**Detail:** The PremiumTree set contains 5 prefab variants, LOD groups, and 1k bark/moss textures with diffuse, normal, and ARM maps. The MYB-112 report records `PASS`, 5 variants, and `15954` stored triangles per prefab across LODs. A GUID search found no PremiumTree prefab/material references in `Assets/Scenes/MYB89UnityMcpProbe.unity`.

**Implication:** The right improvement path is not external generation. Use a few existing PremiumTree variants as close route-camera anchors, then keep cheaper procedural/proxy trees for background density.

## Deduced Conclusions

### Deduction 1: Passing MYB-167 is necessary but not sufficient for the forest composition

**Based on:** Finding 1.

**Reasoning:** The gate detects obstruction/corridor intrusion, not the desired close-forest enclosure or palette quality.

**Conclusion:** The fix likely over-optimized for zero warnings and under-optimized for the route-camera art direction.

### Deduction 2: PR #32 should not be merged as-is

**Based on:** Findings 1, 2, and 4.

**Reasoning:** The branch is analytically clean but player-facing rejected. It also encodes the wrong correction strategy: pushing forest away instead of distinguishing close scenic enclosure from route obstruction.

**Conclusion:** Keep PR #32 open and revise it in place.

### Deduction 3: Yellowing and tree-distance are related visually but have different immediate causes

**Based on:** Findings 2, 3, and 5.

**Reasoning:** The yellow/open look is already present before final cleanup, while the tree-distance complaint is directly amplified by the all-warning cleanup. Treating both as one placement bug would miss the MYB-165/camera/lighting scene-composition regression.

**Conclusion:** Fix validation in two layers: comparable route-camera checkpoints for palette/composition, and smarter geometry rules for road readability.

### Deduction 4: Tree beauty work should be asset-tiered, not global texture replacement

**Based on:** Findings 6 and 7.

**Reasoning:** Applying 1k bark/normal/ARM materials or high-triangle prefabs to every tree would be wasteful. Keeping all foreground trees flat will keep the close route-camera image cheap. The scene needs a tier split.

**Conclusion:** Use premium/textured trees only for route-camera foreground anchors and hero enclosure, use stylized flat/proxy canopy materials for mid/far masses, and tune lighting/palette around that hierarchy.

## Hypothesized Paths

### Hypothesis 1: Near-route tree masses were moved too far laterally

**Status:** Confirmed as a contributing cause

**Theory:** The MYB-168 cleanup pushed MYB-163 trees and hero masses out enough to clear the gate, but also removed close forest enclosure and exposed more yellow ground.

**Supporting indicators:** The commit intentionally changed tree distances from 6.75/7.85/7.45/5.95m to 8.05/9.15/9.05/7.25m.

**Would confirm:** Capture comparison shows loss of close trees exactly after those distance changes; reverting distance while reshaping canopy clears visibility.

**Would refute:** Earlier captures already had the same lack of proximity and yellow palette before the distance changes.

**Resolution:** Confirmed for the "trees too far" symptom. Not sufficient to explain the whole yellow/open shift because MYB-165 was already yellow/open.

### Hypothesis 2: Overview camera normalization is unrelated to route yellowing

**Status:** Mostly confirmed

**Theory:** The MYB-165 OverviewCamera change affects only overview captures, not the RouteCamera image.

**Supporting indicators:** RouteCamera position/FOV in MYB-145 final report remains the route camera, while OverviewCamera fields changed separately.

**Would confirm:** Scene/capture metadata shows RouteCamera unchanged between prior and final captures.

**Would refute:** MYB-165 rebuild changed RouteCamera framing or lighting at the same time.

**Resolution:** Mostly confirmed. The important camera finding is not the overview normalization; it is that MYB-163 versus MYB-165/MYB-168 route captures are taken from different route positions.

### Hypothesis 3: No direct material color change caused the yellow shift

**Status:** Supported

**Theory:** The final commit did not intentionally alter materials; yellowing is from exposed sand/leaf/road-side surfaces after moving forest masses, or scene rebuild context, not direct material edits.

**Supporting indicators:** Material serialization noise was restored before commit; staged diff has no `.mat` file changes.

**Would confirm:** `git diff f88845b^..f88845b` has no material/lighting color changes.

**Would refute:** Scene diff or builder changed lighting, material assignment, camera clear flags, or ground surfaces.

**Resolution:** Supported by current evidence. Yellowing appears driven by camera/framing/scene composition, exposed warm surfaces, and warm lighting rather than a direct MYB-168 material recolor.

## Missing Evidence

| Gap | Impact | How to Obtain |
|---|---|---|
| Preserved route capture immediately before final all-warning cleanup | Would isolate the visual delta from CloseLeft-only fix to final cleanup | Still useful, but no longer required to explain yellowing because MYB-165 already shows it. |
| Acceptable near-tree threshold | Needed to tune validator without making forests sterile | Define guardrail rule: allow close trunks/foliage if they do not cover road/horizon. |
| Comparable route-camera checkpoints | Needed to prevent false before/after visual comparisons | Capture at fixed route distances, including start/forest-enclosure frame and the MYB-165 28m frame. |
| Foreground tree material target | Needed before implementation | Define which route-visible trees become premium/textured anchors versus procedural/proxy masses. |

## Source Code Trace

| Element | Detail |
|---|---|
| Error origin | Split: MYB-165 route-camera/long-route scene composition for yellow/open look; MYB-168 all-warning cleanup for trees-too-far symptom |
| Trigger | MYB-165 rebuild/capture uses a later RouteCamera at 28m; MYB-168 then optimizes warnings by moving/reshaping route-visible forest masses |
| Condition | MYB-167 treats protected-overlap/corridor proximity warnings as safety debt without a positive rule for close scenic framing |
| Related files | `MYB163CanonicalForestPassageIntegrator.cs`, `MYB48RouteDifficultyCueController.cs`, `MYB89ProbeBuilder.cs`, `MYB167RouteVisibleSupportValidator.cs`, `MYB165FirstTrueRouteBuilder.cs` |

## Conclusion

**Confidence:** High

Julien's rejection is valid. The final branch proves the validator can pass a visually worse scene. The main process failure was treating "zero route-camera warnings" as an art-safe target. The yellow/open look appears to have entered before the last MYB-168 cleanup, around MYB-165's long-route/camera composition; the tree-distance issue was then made worse by the final cleanup.

## Recommended Next Steps

### Fix direction

Revise PR #32: keep trees close, reshape/raise/cut only the obstructing canopy volumes, and adjust MYB-167 so it distinguishes valid close forest framing from route/horizon masking.

Concrete guardrail adjustment:

1. Add a positive allowance for close side framing: route-visible trunks/canopies can be close to the road if projected overlap avoids the road surface and critical forward horizon band.
2. Split warnings by intent: `routeObstruction` for protected road/horizon masking; `closeScenicFraming` as informational or allowed when support/grounding is credible.
3. Validate more than one canonical route-camera position, at minimum the close forest/start frame and the MYB-165 28m frame.
4. Add palette/composition regression evidence from the actual scene captures, not fixtures.

Asset/material adjustment:

1. Promote a small number of existing `MYB112_PremiumTree_*` prefabs into the route-camera foreground as anchor trees.
2. Keep procedural MYB163-style tree assemblies for custom silhouettes, but upgrade bark/root materials by reusing controlled 1k bark/moss material families or a stylized detail material.
3. Do not texture every canopy. Preserve stylized foliage as shaped color masses, with 2-3 tuned green values and shadow-side variants.
4. Use MYB-167 route-readability validation plus route captures to confirm premium trees are close, grounded, supported, and not masking the road/horizon.

### Diagnostic

Run the next correction against real scene captures. Compare MYB-163-style close/green enclosure and MYB-165/MYB-168 28m frame separately, because they are different visual checkpoints.

## Reproduction Plan

1. Checkout or inspect branch `MYB-168-fix-closeleftframe-canopy-route-readability` at `f88845b`.
2. Open `_bmad-output/visual-checkpoints/MYB-168/2026-06-19T15-28-23Z-after-route.png`.
3. Compare against the nearest pre-cleanup route capture from MYB-163/MYB-165/MYB-168.
4. Re-run MYB-167 to confirm the green analytical state if needed.

## Side Findings

- PR #32 should remain open. It is useful evidence but not visually accepted.
- The scene contains old MYB89/MYB165 long-route objects in the rejected frame; final MYB-168 content is not the only visual contributor.
- Existing PremiumTree assets are a viable local source for better bark/moss material response, but they are currently absent from the canonical scene.

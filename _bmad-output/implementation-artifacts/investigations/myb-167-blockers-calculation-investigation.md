# Investigation: MYB-167 route-visible support blockers calculation

## Hand-off Brief

1. **What happened.** Julien asked whether the 50 MYB-167 route-visible support blockers are real blockers or validator calculation problems; the evidence shows they are mostly calculation/classification false positives.
2. **Where the case stands.** Concluded and implemented; MYB-167 now uses local MYB-163 forest-floor ground, same-assembly base/root support, and strict MYB-154 thresholds instead of globally tolerating vertical supports up to 0.58m/0.68m.
3. **What's needed next.** Review the remaining route-camera safety warnings visually; support blockers are resolved and fixture coverage still catches a true floating vertical support.

## Case Info

| Field | Value |
| --- | --- |
| Ticket | MYB-167 |
| Date opened | 2026-06-18 |
| Status | Concluded / implemented |
| System | macOS / Unity 6000.4.10f1 project `unity/Echapee4D` |
| Evidence sources | MYB-167 metrics/report, MYB-167 validator source, MYB-163 builder/metrics, MYB-44 relic builder source |

## Problem Statement

User-reported question: analyze the MYB-167 blockers to know whether they are true blockers or caused by calculation problems.

## Evidence Inventory

| Source | Status | Notes |
| --- | --- | --- |
| `HEAD:_bmad-output/implementation-artifacts/MYB-167/myb-167-route-visible-support-metrics.json` | Available | PR evidence: `unsupportedBlockingCount=50`, `unsupportedWarningCount=2`. |
| Worktree `_bmad-output/implementation-artifacts/MYB-167/myb-167-route-visible-support-metrics.json` | Available | Dirty worktree evidence: `unsupportedBlockingCount=0`; result changed by threshold edits, not scene correction. |
| `unity/Echapee4D/Assets/MYB167/Editor/MYB167RouteVisibleSupportValidator.cs` | Available | `HEAD` blocks vertical support-shaped route-visible renderers above +0.10m; dirty worktree tolerates them through 0.58m and blocks above 0.68m. |
| `unity/Echapee4D/Assets/MYB163/Editor/MYB163CanonicalForestPassageIntegrator.cs` | Available | MYB-163 uses `TerrainHeight(meters, offset)` and grounds parent assemblies by combined renderer bounds. |
| `_bmad-output/implementation-artifacts/MYB-163/myb-163-canonical-forest-passage-metrics.json` | Available | MYB-163 reports `floatingAssetCount=0`, `routeVisibleFloatingAssetCount=0`, and ground source `sampled canonical route shoulder/forest floor height`. |
| `unity/Echapee4D/Assets/MYB89/Editor/MYB89ProbeBuilder.cs` | Available | MYB-44 relic pillar has an explicit base under the pillar. |

## Investigation Backlog

| # | Path to Explore | Priority | Status | Notes |
| - | - | - | - | - |
| 1 | Parse blocker list and group by object/role/reason | High | Done | 52 support suspects: 50 blocking, 2 warning; all are vertical support bottom-clearance findings. |
| 2 | Inspect bottom-clearance calculation | High | Done | MYB-167 uses nearest route Y only, while MYB-163 side assets use local `TerrainHeight`. |
| 3 | Inspect support co-visibility logic | High | Done | Not the source of these 50 blockers; `EvaluateFloatingVerticalSupport` returns before normal support matching. |
| 4 | Confirm with targeted builder evidence | Medium | Done | Static comparison against MYB-163 terrain formula explains the majority of clearances. |

## Timeline of Events

| Time | Event | Source | Confidence |
| --- | --- | --- | --- |
| 2026-06-18 | MYB-167 validator correction pushed; batch mode intentionally fails with 50 support blockers. | `HEAD` metrics | Confirmed |
| 2026-06-18 | Dirty worktree metrics show `unsupportedBlockingCount=0` after local threshold changes. | Worktree metrics / source diff | Confirmed |
| 2026-06-18 | MYB-167 local-ground/assembly-support correction applied; `RunBatchValidate` exits 0 with `PASS_WITH_WARNINGS`. | Unity batch validation | Confirmed |

## Confirmed Findings

### Finding 1: `HEAD`/PR has 50 support blockers, but the dirty worktree has zero because the threshold was changed.

**Evidence:** `HEAD:_bmad-output/implementation-artifacts/MYB-167/myb-167-route-visible-support-metrics.json` reports `unsupportedBlockingCount=50`; worktree metrics report `unsupportedBlockingCount=0`. `git diff HEAD -- unity/Echapee4D/Assets/MYB167/Editor/MYB167RouteVisibleSupportValidator.cs` shows `EvaluateFloatingVerticalSupport` changed from warning above `0.05m` / blocking above `0.10m` to tolerating through `SupportCandidateMaxBottomClearanceMeters` (`0.58m`) and blocking above `0.68m`.

**Detail:** The scene was not corrected; the local validator threshold was relaxed enough to suppress all prior blockers.

### Finding 2: The 50 blockers are not fixtures, UI, route surfaces, or support co-visibility failures.

**Evidence:** The `HEAD` metrics `suspects` list contains 52 entries: 50 `Blocking`, 2 `Warning`, all with reason `vertical support-shaped renderer floats above the ground proxy; bottomClearance=...m`.

**Detail:** Object groups are MYB-163 tree/back-wall/background trunks plus one MYB-44 relic pillar. There are no `support required but no ground-connected vertical support found` findings in the blocker set.

### Finding 3: Most MYB-163 back-wall/background blockers are explained by MYB-167 using route Y instead of the local side-ground height.

**Evidence:** `MYB163CanonicalForestPassageIntegrator.TerrainHeight(meters, offset)` produces expected side-ground heights matching MYB-167 bottom clearances for back-wall/background trunks within roughly -0.03m to +0.013m.

**Detail:** Examples: `MYB163_GroupedBackWallMass_06` expected local terrain height is about `0.389m`; blocker clearances are `0.366m`, `0.379m`, `0.391m`, `0.402m`. `MYB163_SoftBackground_L_01` expected local terrain height is about `0.478m`; blockers are `0.444m` and `0.455m`.

### Finding 4: Some foreground tree assembly child trunks sit above the MYB-163 local terrain by 0.06m to 0.17m, but the parent assembly is grounded as a compound asset.

**Evidence:** `CreateTreeAssembly` creates trunks, rear supports, inner supports, wide roots, branches, canopies, and moss/leaf grounding under one parent, then `GroundObjectByVisualBottom` grounds the combined renderer bounds. MYB-167 evaluates each child `MeshRenderer` independently.

**Detail:** These may be small per-child gaps or intentional root/base support inside a compound tree. They should be inspected, but the current MYB-167 blocker reason is overconfident because it does not account for same-assembly root/base support before flagging the vertical child.

### Finding 5: The MYB-44 relic pillar is supported by a base, but MYB-167 blocks the pillar child before support matching.

**Evidence:** `MYB89ProbeBuilder.CreatePremiumSignal` creates `MYB44_RelicBase` under the pillar, then creates `MYB44_RelicPillar`. MYB-167 `EvaluateFloatingVerticalSupport` returns before `FindSupport`.

**Detail:** The relic pillar is a false positive for "unsupported floating vertical support" unless visual inspection shows the base is not visible/credible.

## Deduced Conclusions

### Deduction 1: The 50 blockers are mostly false positives from a ground-source mismatch.

**Based on:** Findings 2 and 3.

**Reasoning:** MYB-167 computes `BottomClearance = renderer.bounds.min.y - nearestRouteY`; MYB-163 deliberately raises side/back forest elements using local shoulder/forest-floor height. The reported "float" amount matches the expected local terrain height, not accidental vertical placement.

**Conclusion:** MYB-167 should not block side/back forest trunks purely because they are above route centerline Y.

### Deduction 2: The local dirty-worktree threshold change hides the symptom but is not a correct fix.

**Based on:** Finding 1 and the MYB-154 policy thresholds.

**Reasoning:** MYB-154 says route-visible floating above +0.10m blocks unless documented. Raising global vertical support tolerance to 0.58m/0.68m makes the validator blind to real near-route levitation below that amount.

**Conclusion:** Keep the +0.05m/+0.10m policy for exact or credible local ground; improve ground estimation and assembly support classification.

## Hypothesized Paths

### Hypothesis 1: Most blockers are true floating/support defects.

**Status:** Refuted

**Theory:** The validator correctly sees route-visible vertical supports or elevated assets with bottom clearance over the blocking threshold.

**Resolution:** Refuted for the majority of blockers by the MYB-163 terrain-height comparison. Some foreground child trunks still require visual/sample-level inspection, but the count of 50 is inflated.

### Hypothesis 2: The blocker count is inflated by calculation or classification false positives.

**Status:** Confirmed

**Theory:** Some objects are measured against the wrong ground source, split into renderer fragments, or denied support because co-visibility sampling is too strict.

**Resolution:** Confirmed. The count is inflated by nearest-route-Y ground proxy and by evaluating compound assembly child renderers before checking local/same-assembly support.

## Missing Evidence

| Gap | Impact | How to Obtain |
| --- | --- | --- |
| Visual correlation for the 9 foreground tree child trunk findings | Would decide if there is a small real visual gap hidden by root flares or a placement bug in those assemblies. | Targeted route-camera screenshots or a debug gizmo/export showing child bounds and local ground. |
| Exact local ground source for non-MYB-163 assets | Needed for the MYB-44 relic and future arbitrary assets. | Physics raycast with explicit ground layer or scene-local ground surface sampling. |

## Source Code Trace

| Element | Detail |
| --- | --- |
| Error origin | `MYB167RouteVisibleSupportValidator.RunBatchValidate` reports failure when `UnsupportedBlockingCount > 0`. |
| Trigger | Unity batch validator scans active route-visible renderers. |
| Condition | `EvaluateFloatingVerticalSupport` classifies a route-visible vertical renderer as floating before normal support matching. |
| Related files | MYB-167 metrics/report, MYB-163 builder/metrics, MYB-44 builder, visual support policy, route-camera safety gate docs. |

## Conclusion

**Confidence:** Medium-High

The 50 MYB-167 support blockers in `HEAD` are mostly not true scene blockers. They are caused by MYB-167 measuring child renderer bottoms against a nearest-route-Y ground proxy while MYB-163 intentionally places side/back forest elements on elevated shoulder/forest-floor terrain and compound root/base assemblies. The current dirty-worktree change that removes the blockers by tolerating vertical supports through `0.58m` and blocking at `0.68m` is not the right fix because it weakens the anti-float policy globally.

Implementation update: the final correction keeps strict `+0.05m` warning / `+0.10m` blocking thresholds when local ground is credible, replaces MYB-163 route-Y false positives with authored local forest-floor ground, allows same-assembly co-visible base/root support, and leaves the true floating-vertical-support fixture blocking.

## Recommended Next Steps

### Fix direction

1. Restore the MYB-154 route-visible floating thresholds for credible local ground: warning above +0.05m, blocking above +0.10m.
2. Replace nearest-route-Y as the sole bottom-clearance ground source with a local ground source:
   - preferred: physics raycast against explicit ground layer, ignoring triggers and generated props;
   - acceptable for generated MYB-163: read or reproduce the builder's local shoulder/forest-floor height when the generated root is known;
   - fallback: classify as warning/ambiguous, not blocking, when only route-Y proxy is available for side/back assets.
3. Evaluate compound assemblies at the parent/asset level before blocking child vertical renderers.
4. Allow a vertical renderer to pass if a same-assembly base/root/halo/support credibly touches its lower bounds and is co-visible.
5. Keep a fixture for a truly floating vertical post with no base/support so the original levitation class remains detected.

### Diagnostic

Add diagnostic fields to support suspects: `groundY`, `boundsMinY`, `localGroundY`, `groundSource`, `assetKey`, `nearestSupportPath`, `supportHorizontalGapMeters`, `supportVerticalGapMeters`, and `supportCoVisibleMeters`. Then rerun MYB-167 and confirm that MYB-163 back-wall/background trunks no longer block solely due to route-Y delta.

## Reproduction Plan

1. Run MYB-167 on `HEAD` behavior to reproduce 50 blockers.
2. Run with local-ground/assembly-aware calculation.
3. Expected result: MYB-163 side/back forest trunk false positives disappear or downgrade to documented warnings; a synthetic floating post remains blocking; any genuinely unsupported foreground child remains listed with precise support/ground evidence.

## Side Findings

- The dirty worktree currently differs from the pushed PR and changes MYB-167 semantics materially. Treat it as unreviewed local work until corrected or intentionally committed.
- The MYB-167 report's support blocker message is misleading for vertical-floating findings: it says "no credible geometric support," but the actual reason can be "bottomClearance over threshold." The report should include the exact support finding reason in the blocking message.

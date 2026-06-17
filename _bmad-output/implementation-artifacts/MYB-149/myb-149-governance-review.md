# MYB-149 Governance Review

Dedicated preview scene exists: Yes
Builder source of truth exists: Yes
Seed 149001 used: Yes
Generated root MYB149_GroundMaterialPreviewRoot exists: Yes
MYB-148 scene modified: No
Canonical ride scene modified: No
Gameplay modified: No
Route trajectory/collider modified: No
Shared production material modified: No
Reusable asset files created: No
Manifest changed: No
No manifest change required: Yes
Meshy/Tripo/text-to-3D/Poly Haven used: No
MYB-144 run: Yes
MYB-144 errors: 0
MYB-144 warnings: 0
Route readability regression: No
Premium target reached: No
Checkpoint status: Checkpoint insuffisant
Human visual review: Accepted with reservations by Julien on 2026-06-16
Recommended Linear status: ready for closure after PR merge, with reservations preserved as follow-up

## Clearance Guard

- RoadHalfWidth: 2.05 m
- Warning rule: `minimumRouteClearanceMeters < RoadHalfWidth + 0.25 m`
- Warning threshold: 2.3 m
- Minimum route clearance: 2.19 m
- Route overlap count: 0
- Clearance warning triggered: Yes
- Interpretation: non-blocking if route overlap count is 0; patches remain outside the trajectory but close to the edge.

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

- Reviewer: Julien
- Decision: accepted as MYB-149 checkpoint with reservations.
- Human note: the result is not very beautiful yet.
- Human note: some objects appear to float in the sky even though they read as trees.
- Interpretation: accepted checkpoint only; not Premium target evidence.
- Follow-up: investigate floating tree/object placement and visual quality in the next visual cleanup / scatter validation pass.

## Verdict

PASS_WITH_WARNINGS

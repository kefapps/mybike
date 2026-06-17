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
Recommended Linear status: accepted checkpoint with reservations

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

- Julien accepted the checkpoint with reservations; MYB-157 covers the
  route-visible visual-support follow-up.
- Large foreground patches are intentionally visible but may need art-direction tuning before any production promotion.
- Route-edge patches remain outside the readable trajectory but sit close to the road edge: minimum clearance 2.19 m is below warning threshold 2.3 m (RoadHalfWidth + 0.25 m).

### MYB-149 Asset / Manifest Warnings

- None.

### MYB-144 Existing Validator Warnings

- None.

### Blocking Errors

- None.


## Verdict

PASS_WITH_WARNINGS

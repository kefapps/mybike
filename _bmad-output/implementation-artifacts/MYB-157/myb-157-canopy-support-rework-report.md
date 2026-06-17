# MYB-157 Canopy Support Rework Report

Status:
- In Review.
- Visual-support blocker fixed by validator metrics.
- Premium target reached: No.
- Verdict: Checkpoint insuffisant, improved.

Generated at:
- 2026-06-17T09:20:33Z

## Scope

- Reworked MYB-148 route-first scatter placement for route-visible canopies.
- Regenerated MYB-148 and MYB-149 preview scenes through deterministic builders.
- Reran MYB-156 visual-support validation on the regenerated MYB-149 scene.

Out of scope:

- no gameplay change;
- no canonical ride scene modification;
- no new reusable asset files;
- no Blender, Meshy, Tripo, Poly Haven, or external asset service;
- no production promotion.

## Implementation

Builder changed:

- `unity/Echapee4D/Assets/MYB148/Editor/MYB148RouteFirstScatterBuilder.cs`

Changes:

- back-wall canopy placement now creates a same-position trunk support before
  placing the canopy;
- back-wall canopy `yOffset` changed from `5.5m` to `2.0m`;
- silhouette-line canopy `yOffset` changed from `6.1m` to `2.5m`;
- MYB-156 family classification now treats names containing `trunk` as trunk
  support before checking `canopy`, so support names like
  `back_wall_canopy_support` are not misclassified.

## Regenerated Scenes

- `unity/Echapee4D/Assets/Scenes/MYB148RouteFirstScatterPreview.unity`
- `unity/Echapee4D/Assets/Scenes/MYB149GroundMaterialPreview.unity`

## Metrics

MYB-148 scatter:

- placements: 63;
- renderers: 78;
- mesh filters: 78;
- total triangles: 21,742;
- minimum route clearance: 3.45m.

MYB-149 ground/material pass:

- total ground patches: 110;
- route-edge patches: 8;
- shoulder transition patches: 27;
- asset grounding patches: 52;
- renderers: 110;
- mesh filters: 110;
- generated triangles: 1,647;
- minimum route clearance: 2.19m;
- route overlap count: 0.

MYB-156 visual-support validation after rework:

- verdict: PASS;
- assetCount: 63;
- groundedAssetCount: 56;
- supportedAboveGroundAssetCount: 7;
- unsupportedCanopyCount: 0;
- routeVisibleUnsupportedCanopyCount: 0;
- maxCanopySupportGap: 0.494m;
- canopyWithoutTrunkCount: 0;
- floatingVisualRiskCount: 0;
- documentedFloatingExceptionCount: 0;
- routeVisibleFloatingExceptionCount: 0.

## Visual Evidence

MYB-148:

- route comparison:
  `_bmad-output/visual-checkpoints/MYB-148/2026-06-17T09-20-13Z-route-before-after.png`
- overview comparison:
  `_bmad-output/visual-checkpoints/MYB-148/2026-06-17T09-20-13Z-overview-before-after.png`
- capture report:
  `_bmad-output/visual-checkpoints/MYB-148/2026-06-17T09-20-13Z-capture-report.md`

MYB-149:

- route comparison:
  `_bmad-output/visual-checkpoints/MYB-149/2026-06-17T09-20-33Z-route-before-after.png`
- overview comparison:
  `_bmad-output/visual-checkpoints/MYB-149/2026-06-17T09-20-33Z-overview-before-after.png`
- before/after capture report:
  `_bmad-output/visual-checkpoints/MYB-149/2026-06-17T09-20-33Z-before-after-capture-report.md`

## Visual Verdict

The route-visible floating-canopy failure is fixed according to MYB-156.

The route image is still not Premium target:

- the forest remains sparse and preview-like;
- ground/material patches still read as flat scene-local decals in places;
- trunk/canopy silhouettes are more coherent but still simple;
- lighting/fog/material depth still needs later Art Rescue passes.

The correct status is `Checkpoint insuffisant, improved`, not `Premium target`.

## Validation

Commands/evidence:

- Unity MCP `MYB148RouteFirstScatterBuilder.BuildAndCaptureFromMenu()`: PASS;
- Unity MCP `MYB149GroundMaterialPreviewBuilder.BuildCaptureValidateFromMenu()`:
  PASS;
- Unity MCP `MYB156VisualSupportValidator.RunValidation("MYB-157-ClassifierFix")`:
  PASS;
- `routeVisibleUnsupportedCanopyCount`: 0.

## Follow-Up

- Keep MYB-157 In Review until Julien validates the updated route/overview
  evidence.
- Next visual work should target overall premium image quality rather than
  visual-support blocking: stronger trunk/canopy asset quality, richer side
  corridor massing, and less flat ground material presentation.

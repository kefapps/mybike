# MYB-156 Visual Support Validator Report

Status:
- PASS
- MYB-156 should stay In Review until Julien validates the visual-support wording and blocker policy.

Generated at:
- 2026-06-17T09:21:33.3321560Z

Scope:
- Validator/governance hardening for route-visible visual support.
- No Unity scene save.
- No gameplay change.
- No generated or imported assets.
- No Blender, Meshy, Tripo, Poly Haven, or external asset call.

Scene:
- `Assets/Scenes/MYB149GroundMaterialPreview.unity`
- Dirty after validation: No

Evidence that triggered MYB-156:
- Route before/after: `_bmad-output/visual-checkpoints/MYB-149/2026-06-16T19-30-40Z-route-before-after.png`
- Overview before/after: `_bmad-output/visual-checkpoints/MYB-149/2026-06-16T19-30-40Z-overview-before-after.png`

## Detection Model

- `grounded`: trunks, rocks, roots, ferns, fallen logs/branches, leaf/moss mats, other non-canopy scatter assets.
- `supportedAboveGround`: canopy or elevated leaf mass assets.
- `exemptFloating`: object names containing `visual_support_exception` or `floating_exception`.

Support rule:
- route-visible `supportedAboveGround` assets need a nearby trunk support.
- support search radius: 4m.
- maximum allowed vertical gap between trunk top and canopy bottom: 0.75m.

Why MYB-155 alone missed this:
- MYB-155 measures visual-bottom ground contact for grounded assets.
- The MYB-149 scan found no floating grounded assets, but route-visible canopy masses can still read as unsupported because their issue is visual support, not ground contact.

## Metrics

- assetCount: 63
- groundedAssetCount: 56
- supportedAboveGroundAssetCount: 7
- unsupportedCanopyCount: 0
- routeVisibleUnsupportedCanopyCount: 0
- maxCanopySupportGap: 0.494m
- canopyWithoutTrunkCount: 0
- floatingVisualRiskCount: 0
- documentedFloatingExceptionCount: 0
- routeVisibleFloatingExceptionCount: 0
- visualSupportMethod: Name-classified supportedAboveGround canopy assets require nearby trunk bounds support.
- routeCameraVisibilityMethod: GeometryUtility.TestPlanesAABB against RouteCamera frustum.
- supportSearchRadiusMeters: 4
- supportVerticalGapMeters: 0.75
- metricsJson: `_bmad-output/unity-test-results/myb-156-visual-support-metrics.json`

## Findings

### Errors

- None.

### Warnings

- None.

### Info

- None.


## Above-Ground Assets

| Asset | Route visible | Unsupported | Nearest support | Horizontal gap | Vertical gap | Exception |
|---|---:|---:|---|---:|---:|---:|
| `MYB148_myb_forest_canopy_mass_a_silhouette_line_28` | Yes | No | `MYB148_myb_forest_trunk_leaning_a_silhouette_line_27` | 0.364m | 0.494m | No |
| `MYB148_myb_forest_canopy_mass_b_silhouette_line_41` | Yes | No | `MYB148_myb_forest_trunk_ancient_a_silhouette_line_40` | 0m | 0.28m | No |
| `MYB148_myb_forest_canopy_mass_b_silhouette_line_13` | Yes | No | `MYB148_myb_forest_trunk_knotted_a_silhouette_line_12` | 0m | 0.233m | No |
| `MYB148_myb_forest_canopy_mass_b_silhouette_line_57` | Yes | No | `MYB148_myb_forest_trunk_knotted_a_silhouette_line_56` | 0m | 0.216m | No |
| `MYB148_myb_forest_canopy_mass_a_back_wall_08` | Yes | No | `MYB148_myb_forest_trunk_leaning_a_back_wall_canopy_support_07` | 0m | 0.064m | No |
| `MYB148_myb_forest_canopy_mass_b_back_wall_20` | Yes | No | `MYB148_myb_forest_trunk_ancient_a_back_wall_canopy_support_19` | 0m | 0m | No |
| `MYB148_myb_forest_canopy_mass_b_back_wall_48` | Yes | No | `MYB148_myb_forest_trunk_leaning_a_back_wall_canopy_support_47` | 0m | 0m | No |

## Verdict

- PASS
- Route-visible unsupported canopy/leaf mass is blocking unless Julien accepts a documented exception.

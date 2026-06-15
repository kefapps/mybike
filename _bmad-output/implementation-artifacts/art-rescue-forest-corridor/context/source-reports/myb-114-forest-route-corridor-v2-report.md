# MYB-114 Forest Route Corridor V2 Report

## Objective

Replace the MYB-114 forest ground ribbon direction with a route-first forest corridor preview, following the terrain-3d recommendation: author the playable route corridor first, then layer close terrain, berms, talus and embedded roots around it.

## Generated Assets

- Scene: `unity/Echapee4D/Assets/Scenes/MYB114ForestRouteCorridorV2Preview.unity`
- Builder: `unity/Echapee4D/Assets/MYB114/Editor/MYB114ForestRouteCorridorV2PreviewBuilder.cs`
- Route capture: `_bmad-output/implementation-artifacts/myb-114-art-directions/forest-route-corridor-v2/myb-114-forest-route-corridor-v2-route.png`
- Overview capture: `_bmad-output/implementation-artifacts/myb-114-art-directions/forest-route-corridor-v2/myb-114-forest-route-corridor-v2-overview.png`
- Validation report: `_bmad-output/implementation-artifacts/myb-114-art-directions/forest-route-corridor-v2/myb-114-forest-route-corridor-v2-preview.md`

## Metrics

- Renderers: 81
- Triangles: 5,664
- Corridor bands: 9
- Embedded root features: 4
- Sculpted continuous ridges: 2
- Buttress root fans: 12
- Sparse tree line trunks: 30
- Forest edge masses: 0
- Surface cues: visual-only

## What Changed From V1

V2 removes the blocky forest edge masses from V1 and replaces them with continuous sculpted ridges, lower berm profiles, repeated buttress-root fans and sparse trunk silhouettes at the outer corridor edge.

This better matches the accepted route-first strategy: the forest floor is no longer treated as isolated decorative props, and the side terrain now belongs to the ride corridor.

## Visual Verdict

The direction is structurally better than V1, but it is still a greybox-quality preview.

The good part: the route reads as a continuous authored corridor, the side masses are no longer rectangular blocks, and the embedded roots now belong to the terrain shape rather than floating as isolated objects.

The weak part: the image is still too grey, too sparse and too poor in material variation. It demonstrates the terrain strategy, not the final forest quality target.

## Recommendation

Keep this as the MYB-114 technical/art-direction baseline for the forest route corridor, but do not treat it as a finished visual asset family.

The next production step should use this corridor grammar with richer authored surfaces: denser root clusters, leaf litter, exposed soil, moss patches, varied trunk silhouettes and better light/fog composition.

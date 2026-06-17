# Unity Ground Placement Policy

Status: Canonical Art Rescue anti-floating placement policy.

Scope: visible Art Rescue assets placed in Unity builders, preview scenes, and
production-candidate forest corridor work.

This policy is documentation/governance. It does not modify scenes, gameplay, or
assets by itself.

Companion policy: `docs/validation/unity-visual-support-policy.md` covers
route-visible elevated assets, such as canopies, that can read as floating even
when ground-contact metrics are technically outside the problem.

## Goal

Visible assets must belong to the forest floor. Trees, rocks, roots, patches,
signs, and other route-visible objects must not float above the ground or sink so
far that scale credibility breaks.

The canonical placement rule is:

```txt
Ground by visual bottom after transform.
```

## Correct Placement Method

For each placed asset:

1. Instantiate the asset.
2. Apply its final rotation and scale.
3. Compute the combined renderer bounds for all relevant renderers.
4. Use `bounds.min.y` to compute the pivot-to-visual-bottom offset.
5. Move the instance so the visual bottom meets the sampled ground height.
6. Apply a small documented sink, usually 0.02m to 0.05m.
7. Report bottomClearance metrics when the asset is visible from the route.

`bottomClearance` is positive when the asset bottom floats above the sampled
ground and negative when the asset bottom is sunk below the sampled ground.

## Forbidden Final Offsets

Do not use these as the final vertical placement policy:

- `bounds.extents.y`;
- `bounds.size.y / 2`;
- fixed half-height offsets;
- hand-authored magic numbers that pretend to be asset height.

Those values can be useful for diagnostics, but they are not final grounding.
They commonly place assets several meters above the floor when pivots,
transforms, nested renderers, or prefab geometry do not match the assumed height.

## Raycast Ground Detection

If placement uses raycasts:

- use an explicit ground layer mask, or document the non-raycast ground source;
- ignore triggers;
- avoid raycasts hitting generated assets, material patches, props, or the asset
  being placed;
- prefer terrain, authored ground, route shoulder, or documented floor surfaces
  as the ground source;
- record `groundLayerMask` or `groundSource` in builder metrics.

## Thresholds

| Metric | Threshold | Severity |
| --- | ---: | --- |
| Target bottomClearance | -0.05m to +0.05m | Pass |
| Floating asset | > +0.05m | Warning |
| Route-visible floating asset | > +0.10m | Blocking |
| Sinking asset | < -0.10m | Warning |
| Route-visible sinking asset | < -0.25m | Blocking |

Route-visible floating assets above +0.10m block checkpoint review unless Julien
explicitly accepts a documented exception. Route-visible sinking below -0.25m is
also blocking unless the exception is documented.

## Required Future Builder Metrics

Future Art Rescue builders that place visible assets should report:

- `floatingAssetCount`;
- `maxFloatingClearance`;
- `sinkingAssetCount`;
- `maxSinkingDepth`;
- `routeVisibleFloatingAssetCount`;
- `groundPlacementMethod`;
- `groundLayerMask` or `groundSource`;
- `sinkMeters`.

Recommended optional metrics:

- `bottomClearanceMin`;
- `bottomClearanceMax`;
- `bottomClearanceAverage`;
- `routeVisibleSinkingAssetCount`;
- `groundRaycastMissCount`;
- `groundRaycastRejectedHitCount`.

## Review Policy

Ground contact is part of Scale credibility in
`docs/validation/forest-corridor-shot-rubric.md`.

Grounded assets are also part of the forest art target in
`docs/art-direction/mybike-forest-art-bible-v0.md`: assets must belong to the
forest floor, not float above it.

For visual checkpoints:

- report known bottomClearance metrics when builders provide them;
- mark route-visible floating above +0.10m as a checkpoint blocker;
- mark route-visible sinking below -0.25m as a checkpoint blocker;
- also check the visual-support policy for canopies, elevated foliage, and
  other above-ground assets that need credible support from the route camera;
- do not close visible Art Rescue work as `Done` when a ground contact blocker
  is present unless Julien accepts a documented exception.

## Follow-Up Adoption

Existing builders should adopt this policy when they next touch placement,
especially MYB-148 scatter, MYB-149 ground material, and MYB-150 lighting/fog or
scene composition builders if they place or adjust route-visible assets.

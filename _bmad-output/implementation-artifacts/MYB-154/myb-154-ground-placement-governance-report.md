# MYB-154 Ground Placement / Anti-Float Governance Report

Status: In Review

Linear tracker: `MYB-155` because Linear `MYB-154` already exists as a completed
different ticket. Local artifact path keeps the requested MYB-154 label.

## Scope

Documentation/governance hardening only.

No Unity scenes, gameplay, generated assets, Blender, Meshy, Tripo, Poly Haven,
or asset imports were modified or called.

## Files Changed

- `AGENTS.md`
- `CONTEXT.md`
- `CONTEXT-MAP.md`
- `docs/art-direction/mybike-forest-art-bible-v0.md`
- `docs/validation/forest-corridor-shot-rubric.md`
- `docs/validation/unity-ground-placement-policy.md`
- `_bmad-output/implementation-artifacts/MYB-154/myb-154-ground-placement-governance-report.md`
- `_bmad-output/linear-sync.md`

## Policy Summary

Visible Art Rescue assets must be grounded by visual bottom. Unity placement
builders should instantiate the asset, apply rotation and scale, compute
combined renderer bounds, then use `bounds.min.y` to compute the
pivot-to-visual-bottom offset.

Do not use `bounds.extents.y`, `bounds.size.y / 2`, fixed half-height offsets,
or equivalent magic numbers as the final vertical placement policy.

Builders should apply a small documented sink, usually 0.02m to 0.05m, so assets
belong to soil, moss, or leaf litter.

If raycasting, builders must use an explicit ground layer mask or documented
ground source, ignore triggers, and avoid raycasts hitting generated assets,
patches, props, or the asset being placed.

## Thresholds Added

- Target bottomClearance: -0.05m to +0.05m.
- Warning floating: > +0.05m.
- Blocking floating for route-visible assets: > +0.10m.
- Warning sinking: < -0.10m.
- Blocking sinking for route-visible assets: < -0.25m.

Route-visible floating assets above +0.10m block checkpoint review unless Julien
explicitly accepts a documented exception. Route-visible sinking below -0.25m is
also blocking unless the exception is documented.

## Required Future Builder Metrics

- `floatingAssetCount`
- `maxFloatingClearance`
- `sinkingAssetCount`
- `maxSinkingDepth`
- `routeVisibleFloatingAssetCount`
- `groundPlacementMethod`
- `groundLayerMask` / `groundSource`
- `sinkMeters`

## Code / Scene / Asset Modification Check

- Code modified: No.
- Unity scenes modified: No.
- Gameplay modified: No.
- Assets generated or imported: No.
- Blender called: No.
- Meshy called: No.
- Tripo called: No.
- Poly Haven called: No.

Confirmation: this was doc-only governance work.

## Follow-Up Recommendation

MYB-148, MYB-149, and MYB-150 builders should adopt the new ground placement
metrics if they do not already report them. In particular, future route-visible
asset placement should report `bottomClearance`, floating/sinking counts, ground
source or layer mask, and documented `sinkMeters`.

## Final Status

In Review, not Done, until Julien validates the governance wording.

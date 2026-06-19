# Unity Visual Support Policy

Status: Canonical Art Rescue visual-support policy for elevated visible assets.

Scope: route-visible Art Rescue forest corridor assets placed in Unity builders,
preview scenes, and production-candidate reviews.

This policy complements `docs/validation/unity-ground-placement-policy.md`.
Ground placement catches bottom-to-floor errors. Visual support catches assets
that may be intentionally above the floor but still read as floating because the
route camera cannot see a credible support.

## Goal

Visible assets must belong to the forest world, not merely occupy plausible
coordinates. A canopy, elevated leaf mass, hanging root, branch cluster, or
overhead scenic element must either be supported by a readable visual structure
or carry an explicit documented exception.

Canonical rule:

```txt
Above-ground route-visible assets need visible support.
```

## Visual Roles

Classify visible Art Rescue assets by role before applying support checks:

- `grounded`: trunks, rocks, roots, ferns, fallen branches/logs, moss/leaf mats,
  ground patches, signs, and other assets that should contact the ground;
- `supportedAboveGround`: canopies, elevated leaf masses, hanging or overhead
  scenic elements, and any asset intended to sit above the floor;
- `exemptFloating`: rare explicit exception with documented rationale and
  Julien acceptance.

`supportedAboveGround` is not a free pass. It means the asset is expected to be
above the floor and therefore needs a support relationship.

## Support Evidence

A route-visible `supportedAboveGround` asset must provide at least one credible
support signal:

- explicit metadata or naming that links it to a support object;
- nearby trunk or authored support within a documented horizontal radius;
- vertical overlap or small documented vertical gap between support top and
  elevated asset bottom;
- route-camera evidence showing the support reads visually from the cyclist
  view, ideally in the same sampled route-camera pose as the supported asset;
- support geometry that is itself visually grounded, with floating supports
  reported separately instead of being treated as clean evidence;
- documented exception accepted by Julien.

When a validator uses an approximate route-ground proxy instead of physics
raycasts, a small support-bottom tolerance is allowed so real trunks/posts are
not rejected by terrain slope or sampling error. MYB-167 V1 uses `0.58m` as the
support-candidate association tolerance only. Floating vertical supports still
follow the MYB-154 bottomClearance policy when measured against a credible local
ground source: warning above `+0.05m`, blocking above `+0.10m`. If only an
approximate route-Y proxy is available, the validator should improve the local
ground source or downgrade the case to ambiguous review instead of weakening the
global anti-float thresholds.

If the support is only visible in overview but not from the route camera, it is
not enough for production visual validation.

## Blocking Rule

Route-visible `supportedAboveGround` assets without credible support block visual
checkpoint review unless Julien accepts a documented exception.

Examples of blocking failures:

- canopy mass floating several meters above a short trunk;
- leaf blob above the corridor with no visible trunk or branch below it;
- overhead scenic prop placed by `yOffset` with no support metadata;
- a support that is present in the scene but too far away or too low to read as
  connected.

## Required Metrics

Future validators and builders that place elevated visible assets should report:

- `unsupportedCanopyCount`;
- `routeVisibleUnsupportedCanopyCount`;
- `maxCanopySupportGap`;
- `canopyWithoutTrunkCount`;
- `floatingVisualRiskCount`;
- `documentedFloatingExceptionCount`;
- `routeVisibleFloatingExceptionCount`;
- `visualSupportMethod`;
- `routeCameraVisibilityMethod`;
- `supportSearchRadiusMeters`;
- `supportVerticalGapMeters`.

Recommended optional metrics:

- `aboveGroundAssetCount`;
- `supportCandidateCount`;
- `nearestSupportName`;
- `supportHorizontalGapMeters`;
- `supportVerticalGapMeters`;
- `visualSupportExceptionId`.

## Review Policy

The route screenshot is the authoritative surface for visual support. Overview
captures may explain why a support relation was intended, but they do not prove
that the route camera reads it.

For Art Rescue visual checkpoints:

- separate bottomClearance failures from visual-support failures;
- treat route-visible unsupported canopies as blockers;
- document any accepted exception next to the route capture;
- keep subjective cases in review until Julien validates the wording and visual
  exception.

## MYB-156 Reference

`MYB156VisualSupportValidator` is the first ticket-local implementation of this
policy. It validates MYB-148/MYB-149 style scenes by classifying canopy assets,
checking route-camera visibility, finding nearby trunk support, measuring
horizontal and vertical support gaps, and writing a report under
`_bmad-output/unity-test-results/`.

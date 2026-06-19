# Route Camera Safety Gate

Status: canonical validation policy for playable Unity route scenes.

Owner: Art Rescue visual validation / Unity route-camera governance.

## Purpose

The route must remain readable from the canonical bike POV route camera before a
scene reaches visual review. This gate detects route-obscuring geometry from
route/camera/bounds math, without requiring a rendered video.

Video and screenshots remain human-facing proof. The gate below is the
machine-readable early warning layer that should fail obvious blockers before
capture work begins.

## Scope

Apply this policy to visible/playable Unity route scenes, forest corridor visual
tickets, and any future route scene that claims route-camera validation.

The current V1 implementation lives at:

- `unity/Echapee4D/Assets/MYB167/Editor/MYB167RouteVisibleSupportValidator.cs`

Generated evidence currently lives at:

- `_bmad-output/implementation-artifacts/MYB-167/myb-167-route-visible-support-report.md`
- `_bmad-output/implementation-artifacts/MYB-167/myb-167-route-visible-support-metrics.json`

## Canonical Rule

Every active enabled scene `Renderer` is in scope by default. A renderer can pass
only if it is explicitly exempt by role, does not intrude into the route safety
corridor, and does not dominate the protected route-camera readability zone.

The gate is not a fixed prefab-name deny-list. Names may identify explicit
system roles, but they must not be the detection source for scenic blockers.

## Exempt Roles

Exemptions must be explicit role/category exemptions, not ad hoc visual-asset
exceptions:

- `routeSurface`: road, route edge, shoulder, floor, leaf, moss, and other
  authored ground/route surfaces.
- `bikePovCue`: cockpit, HUD, rider rig, and camera-attached bike POV cues.
- `intendedGateOrSignage`: explicit route furniture that is meant to be crossed
  or read.
- `gameplayCue`: intentional gameplay-readable cue geometry.
- `captureHelper`: capture/debug helper geometry that must not be judged as
  scene art.

If an object is not clearly one of these roles, it stays in scope.

Route surfaces are exempt from route-camera safety because they are the visual
reference for the ride path. This exemption needs both role intent and geometric
evidence: the renderer should be a ground/route mesh with mostly upward-facing
surface normals, near-ground bottom clearance, and a route/ground-like footprint.
Long route ribbons may have large world-space Y bounds because the route climbs
or descends; that is acceptable only when the mesh still reads as an up-facing
surface. Do not tag tall scenic props, blockers, canopies, walls, or general
forest art as route surfaces to escape the gate; fix the role/naming instead.

## Detection Model

The implementation should sample the canonical route/bike camera along the full
route, using route spacing suitable for the ticket. MYB-167 V1 uses about 24m
spacing.

For each sampled pose:

1. Compute camera frustum planes and camera view/projection matrices.
2. Collect every active enabled scene `Renderer`.
3. Test renderer bounds against sampled route-camera frustums.
4. Project visible renderer bounds into viewport space.
5. Measure projected viewport rectangle, protected-zone overlap, minimum visible
   camera distance, and route-centerline clearance in XZ.

If a renderer intersects the camera near plane or contains the camera, the
projection must use a conservative full-viewport fallback. Near-plane blockers
must not disappear because no bounds corner can be projected normally.

The protected route-camera zone should cover the central route/horizon reading
area, not the entire screen. MYB-167 V1 uses viewport `x=0.18..0.82` and
`y=0.20..0.78`.

V1 also evaluates an upper route/horizon occlusion band for real scene
renderers. MYB-167 uses viewport `x=0.12..0.88` and `y=0.56..0.88` to catch
near-corridor elevated masses that sit above the road and hide the forward route
read, even when their overlap with the central protected zone is only partial.
This is a scene-validation warning by default. It becomes blocking only when the
renderer also has hard route-camera or corridor evidence, such as very close
corridor clearance, central protected-zone overlap, or strong viewport
dominance. Synthetic fixtures may prove the math, but they do not replace the
scene verdict.

## Issue Types

`routeCorridorIntrusion`

The renderer bounds enter the route safety corridor in XZ. This catches objects
whose projected image might not be huge but whose world position or footprint
can physically occupy the ride path.

`routeCameraReadabilityBlocker`

The renderer may be outside the route corridor, but its projected bounds dominate
the protected route/horizon reading zone. This catches large overhead masses,
walls, canopies, or side objects that visually cover the road.

For elevated route/horizon occlusion, a real non-exempt scene renderer can warn
when it is near the route corridor and substantially overlaps the upper
route/horizon band. It should block only when the same renderer also has hard
evidence that route readability is genuinely compromised. This catches canopy or
overhead masses like the MYB-167 trigger case before a video is recorded without
automatically blocking authored scenic enclosure that still leaves the road
readable.

## Severity

V1 should be conservative.

- `ERROR`: obvious route-camera blocker, deep route corridor intrusion, or a
  failing synthetic regression fixture.
- `WARNING`: moderate projected dominance, close corridor clearance, or ambiguous
  case that requires route-camera review.
- `INFO`: exempt roles, non-impacting visible objects, and governance notes.

Warnings are allowed to keep a ticket in review. Errors block production visual
approval unless Julien accepts a documented exception.

For corridor clearance, V1 should warn when non-exempt bounds are close to the
ride line and block only when bounds clearly cross the protected centerline or
combine with strong route-camera dominance. This keeps authored verge, root, and
near-ground richness reviewable without hiding real route blockers.

## Required Metrics

Reports and JSON should include:

- `routeCameraSafetyVerdict`
- `routeCameraSampleCount`
- `totalRendererCount`
- `routeVisibleRendererCount`
- `routeCorridorIntrusionCount`
- `routeReadabilityWarningCount`
- `routeReadabilityBlockingCount`
- `worstRouteVisibilityScore`

Each top issue should include:

- object name/path;
- role/exemption status;
- rule;
- route meters and estimated route time;
- minimum visible camera distance;
- route corridor clearance;
- projected viewport rectangle;
- viewport dominance ratio;
- protected-zone overlap ratio;
- recommended action.

## Relationship To Other Gates

`docs/validation/unity-ground-placement-policy.md` owns visual-bottom grounding
and bottomClearance.

`docs/validation/unity-visual-support-policy.md` owns believable support for
route-visible elevated elements.

The support policy also owns co-visible support evidence: a support that exists
in the scene but is not visible with the supported asset from the route camera is
not enough for production validation.

This policy owns route-camera readability and route-corridor intrusion. A scene
can pass ground placement and visual support while still failing this gate if a
large, supported object covers the route from the bike POV.

`docs/workflows/visual-checkpoint-workflow.md` still owns capture evidence. This
gate should run before capture when possible, because it can detect many blockers
without rendering video.

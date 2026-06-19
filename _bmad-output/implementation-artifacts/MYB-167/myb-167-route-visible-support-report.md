# MYB-167 Route-Camera Safety Gate Report

## Summary
MYB-167 implements a generic route-camera safety gate for route-visible renderers. It scans all enabled scene `Renderer` instances, samples the bike POV route camera across the route, classifies visible renderers by bounds/support geometry, projected screen dominance, protected route-zone overlap, and route corridor clearance, then proves arbitrary synthetic fixtures are detected without known prefixes.

## Scope
- Validator path: `unity/Echapee4D/Assets/MYB167/Editor/MYB167RouteVisibleSupportValidator.cs`
- Scene validated: `Assets/Scenes/MYB89UnityMcpProbe.unity`
- Report: `_bmad-output/implementation-artifacts/MYB-167/myb-167-route-visible-support-report.md`
- Metrics: `_bmad-output/implementation-artifacts/MYB-167/myb-167-route-visible-support-metrics.json`
- Code/scene/assets modified by validator run: No scene save, no gameplay change, no route/collider/HUD/telemetry change, no assets generated.

## Files Changed
- `unity/Echapee4D/Assets/MYB167/Editor/MYB167RouteVisibleSupportValidator.cs`
- `_bmad-output/implementation-artifacts/MYB-167/myb-167-route-visible-support-report.md`
- `_bmad-output/implementation-artifacts/MYB-167/myb-167-route-visible-support-metrics.json`
- `docs/validation/route-camera-safety-gate.md`
- `docs/validation/unity-visual-support-policy.md`
- `CONTEXT.md`
- `CONTEXT-MAP.md`
- `_bmad-output/linear-sync.md`
- `_bmad-output/unity-test-results/myb-144-art-asset-validator-report.md` refreshed by MYB-144.

## Detection Model
- Visibility: `GeometryUtility.TestPlanesAABB` against sampled bike POV route-camera frustums.
- Projection: renderer world bounds are projected through sampled route-camera view/projection matrices into viewport rectangles.
- Protected route-camera zone: `x=0.18..0.82, y=0.20..0.78`; objects that dominate this zone become route-camera readability suspects.
- Route samples: `100` at about `24m` spacing.
- Ground source: `local generated forest floor / same-assembly base support when available; nearest smoothed route sample y fallback`.
- Floating vertical support rule: route-visible vertical support-shaped renderers warn above `0.05m` and block above `0.1m` when measured against a credible local ground source.
- Local ground correction: known MYB-163 generated forest masses use their authored shoulder/forest-floor height instead of route centerline Y, so side banks and back-wall trunks are not false-blocked for being above the road plane.
- Base support rule: vertical supports may pass when a same-assembly base/root/grounding element is route-camera co-visible and physically overlaps the support footprint.
- Support-required rule: elevated route-visible bounds with bottomClearance above `0.45m` need nearby ground-connected vertical geometry that is co-visible in the route camera sample.
- Support candidate tolerance: candidate supports may be associated up to `0.58m` bottomClearance because ground is approximate; the support renderer is still evaluated separately by the floating vertical support rule.
- routeCorridorIntrusion rule: non-exempt renderer bounds with route-centerline clearance below `4.2m` warn, and below `0m` block.
- routeCameraReadabilityBlocker rule: non-exempt renderer bounds with viewport dominance/protected-zone overlap above `0.07`/`0.1` warn, and above `0.12`/`0.18` block.
- Elevated route occlusion rule: real scene renderers near the route corridor warn when they dominate at least `0.1` of the viewport and overlap at least `0.18` of the upper route/horizon band `x=0.12..0.88, y=0.56..0.88`; they block only with hard corridor, protected-zone, or strong-dominance evidence.
- closeScenicFramingPass rule: authored MYB-163/MYB-112 forest framing may be close to the road when it remains outside hard corridor intrusion and below protected route/horizon overlap limits `0.16`/`0.16`.
- Fixed-prefix-only detection used: `false`.

## Allowlist / Exclusions
These exclusions are category-level system/route exemptions, not a visual-asset deny-list.

| Rule | Reason |
|---|---|
| `hud-or-preview-ui` | Hierarchy contains HUD, Canvas, EventSystem, or MYB73 route preview UI. |
| `bike-pov-cockpit-system` | Hierarchy contains MYB165_BikePOVCues or camera-attached cockpit support cues. |
| `intended-gate-or-signage` | Hierarchy explicitly marks a route gate or signage object as intended route furniture. |
| `gameplay-cue` | Hierarchy explicitly marks gameplay-readable cue geometry. |
| `capture-helper` | Hierarchy explicitly marks screenshot/video capture helper geometry. |
| `route-line-renderer` | LineRenderer route/debug/difficulty cue surfaces are not scenic support assets. |
| `route-ground-surface` | Named road, route, edge, shoulder, floor, leaf, or moss ground surfaces are route/ground context. |

## Metrics
- totalRendererCount: `758`
- routeVisibleRendererCount: `758`
- routeVisibleAssetCount: `106`
- excludedSystemRendererCount: `217`
- groundedPassCount: `333`
- supportRequiredCount: `208`
- supportedPassCount: `208`
- unsupportedWarningCount: `0`
- unsupportedBlockingCount: `0`
- maxUnsupportedBottomClearance: `0m`
- maxUnsupportedVisibleDistanceMeters: `0m`
- routeCorridorIntrusionCount: `0`
- routeReadabilityWarningCount: `0`
- routeReadabilityBlockingCount: `0`
- worstRouteVisibilityScore: `0.096`
- routeCameraSafetyVerdict: `PASS`

## Support Regression Fixture
- Verdict: `PASS`
- Unsupported arbitrary object detected: `Yes`
- Supported counterpart passed: `Yes`
- Floating vertical support detected: `Yes`
- Fixture writes scene objects: `No`; the fixture is synthetic bounds evaluated through the same frustum/support logic.

- `MYB167_Fixture/AetherPanel_NoKnownPrefix`: `Blocking` - support required but no ground-connected vertical support found
- `MYB167_Fixture/SupportedBeam_NoKnownPrefix`: `SupportedPass` - co-visible support found: MYB167_Fixture/LeftPost_NoKnownPrefix at route m 0
- `MYB167_Fixture/FloatingVerticalPost_NoKnownPrefix`: `Blocking` - vertical support-shaped renderer floats above the local ground source; bottomClearance=0.755m, groundSource=synthetic-ground

## Route-Camera Safety Fixture
- Verdict: `PASS`
- Dominant arbitrary blocker detected: `Yes`
- Corridor arbitrary intruder detected: `Yes`
- Near-plane arbitrary blocker detected: `Yes`
- Benign small marker passed: `Yes`
- Close scenic forest framing passed: `Yes`
- Fixture writes scene objects: `No`; the fixture is synthetic bounds evaluated through the same route-camera projection/corridor logic.

- `MYB167_RouteCameraSafetyFixture/DominantMound_NoKnownPrefix`: `Blocking` `routeCameraReadabilityBlocker` - Renderer projected bounds dominate the protected route-camera readability zone.
- `MYB167_RouteCameraSafetyFixture/CorridorIntruder_NoKnownPrefix`: `Blocking` `routeCorridorIntrusion` - Renderer bounds intrude into the route safety corridor.
- `MYB167_RouteCameraSafetyFixture/NearPlaneCanopy_NoKnownPrefix`: `Blocking` `routeCameraReadabilityBlocker` - Renderer projected bounds dominate the protected route-camera readability zone.
- `MYB167_RouteCameraSafetyFixture/BenignSmallMarker_NoKnownPrefix`: `GroundedPass` `routeCameraSafetyPass` - Renderer does not intrude into route corridor or dominate the protected route-camera zone.
- `MYB167_RouteCameraSafetyFixture/MYB163_TreeAssembly_CloseLeftFrame/CloseScenicFrame_KnownSupported`: `GroundedPass` `closeScenicFramingPass` - Renderer is an authored close forest framing element and does not mask the protected route/horizon bands.

## Suspects
### Route-Camera Safety

- None.

### Blocking

- None.

### Warnings

- None.

## Comparison With MYB-165 Fix
MYB-165 removed known inherited unsupported route-visible props using a fixed prefix cleanup. MYB-167 does not use that fixed-prefix list as the detection source: it samples the bike POV route-camera, collects every enabled scene `Renderer` visible in at least one route frustum, evaluates bounds/support geometry, and projects bounds into the route-camera viewport to detect corridor intrusion or readability blockers. Prefixes are used only for documented system/route/cockpit/explicit-role exclusions.

## MYB-144
- Verdict: `PASS`
- Errors: `0`
- Warnings: `0`
- Report: `_bmad-output/unity-test-results/myb-144-art-asset-validator-report.md`

## Findings
### Blocking Errors

- None.

### Warnings

- None.

### Info

- `GROUND_SOURCE_APPROXIMATE` `Info` - MYB-167 prefers known local/generated ground and same-assembly base support, but still falls back to nearest route Y when no better local ground source exists. Keep MYB-154 bottomClearance/raycast checks for exact placement where exact placement is required. Action: None.

## Governance
- This is a validation/governance hardening ticket.
- No scene correction or asset deletion was performed by MYB-167.
- No Meshy, Tripo, Blender, or Poly Haven call was made.
- The validator improves on the MYB-165 fixed-prefix cleanup by scanning all route-visible renderers and using geometric support, corridor clearance, and route-camera projection evidence.
- Route-camera video remains useful human proof, but this gate is analytical and can fail before recording a video.
- Premium target reached: `No`.
- Recommended Linear status: `In Review` until Julien validates the severity wording.

## Verdict
- Validator verdict: `PASS`
- Scene support gate: `PASS`
- RouteCameraSafetyGate: `PASS`
- Support fixture gate: `PASS`
- Route-camera safety fixture gate: `PASS`

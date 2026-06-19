# MYB-168 Route-Camera Scenic Framing Finalization
Date: 2026-06-19
Status: PR finalization candidate, pending human visual review
Branch: `MYB-168-fix-closeleftframe-canopy-route-readability`

## Scope

MYB-168 fixes the real-scene route-camera failure that originally let a
CloseLeftFrame tree canopy visually cover the road. The follow-up review rejected
the first cleanup because it made the forest feel too distant and yellow. This
finalization keeps close forest framing allowed, but requires it to be measured on
the rebuilt production scene rather than on fixtures alone.

This ticket does not add external generated assets, Meshy/Tripo content, route
logic changes, HUD changes, or gameplay changes.

## Root Cause

The original bug happened because the route-visible support guardrail detected
unsupported or intrusive geometry only in bounded cases. It did not distinguish
between:

- a close, supported scenic tree that frames the route without blocking the ride
  corridor;
- an unsupported elevated canopy or scenic mass that masks the route camera view;
- authored forest content on the real scene versus isolated validation fixtures.

The first MYB-168 correction overreacted by moving several tree assemblies farther
from the route. That made the analytical warnings disappear, but it damaged the
intended forest-corridor composition. The yellow route capture came from the
MYB-165 road/shoulder material palette plus a route-camera capture point that was
valid for geometry validation but visually emphasized the open, warm road section.

## Implementation

Updated deterministic scene builders and the validator:

- `unity/Echapee4D/Assets/MYB163/Editor/MYB163CanonicalForestPassageIntegrator.cs`
- `unity/Echapee4D/Assets/MYB165/Editor/MYB165FirstTrueRouteBuilder.cs`
- `unity/Echapee4D/Assets/MYB167/Editor/MYB167RouteVisibleSupportValidator.cs`
- `unity/Echapee4D/Assets/Scenes/MYB89UnityMcpProbe.unity`

Changes:

- Restored MYB-163 CloseLeftFrame, MidLeftEnclosure, RightAnchor, and
  RootThresholdHero as close route-framing forest elements instead of pushing the
  corridor sterile.
- Added three close premium MYB-112 tree anchors inside the MYB-163 forest pass,
  reusing existing local premium tree prefabs and grounding them from combined
  renderer bounds.
- Reused existing local PremiumTreePolyHaven bark and moss textures for MYB-163
  bark/root/moss materials while keeping stylized foliage colors controlled.
- Replaced the inherited warm MYB-165 validation road material with a local
  cooler paving material using existing ambientCG textures.
- Tuned the MYB-165 shoulder material cooler/darker so the route capture no
  longer reads as a broad yellow band.
- Moved the MYB-165 route camera validation point to the forest-entry section so
  the canonical capture validates the close trees and route readability together.
- Added MYB-167 `closeScenicFramingPass` handling: authored close forest framing
  can pass only when it stays below hard route corridor, protected-band,
  elevated-overlap, and dominance limits.
- Added a real route-camera safety fixture that proves supported close scenic
  framing is not treated as a blocker.
- Treated integrated MYB-112 premium tree sub-renderers as supported when they
  belong to a grounded MYB-163 premium anchor.

## Validation

Fresh scene rebuild:

```bash
/Applications/Unity/Hub/Editor/6000.4.10f1/Unity.app/Contents/MacOS/Unity -batchmode -nographics -quit -projectPath unity/Echapee4D -executeMethod MYB165FirstTrueRouteBuilder.RunBatchBuildValidateOnly -logFile _bmad-output/unity-test-results/myb-168-finalize-build-validate.log
```

Result: `PASS`, exit code `0`.

MYB-167 route-visible support validator on rebuilt scene:

```bash
/Applications/Unity/Hub/Editor/6000.4.10f1/Unity.app/Contents/MacOS/Unity -batchmode -nographics -quit -projectPath unity/Echapee4D -executeMethod MYB167RouteVisibleSupportValidator.RunBatchValidate -logFile _bmad-output/unity-test-results/MYB-167/unity-batch-myb-168-finalize-route-visible-support.log
```

Result: `PASS`, exit code `0`.

Key metrics:

- `unsupportedBlockingCount`: `0`
- `unsupportedWarningCount`: `0`
- `routeReadabilityBlockingCount`: `0`
- `routeReadabilityWarningCount`: `0`
- `routeCorridorIntrusionCount`: `0`
- `routeCameraSafetyVerdict`: `PASS`
- `routeCameraSafetyFixtureCloseScenicFramingPassed`: `true`
- `myb144Verdict`: `PASS`
- `myb144Errors`: `0`
- `myb144Warnings`: `0`

Capture was rerun without `-nographics` because the offscreen batch capture path
produced blank grey frames. The non-nographics batch capture succeeded:

```bash
/Applications/Unity/Hub/Editor/6000.4.10f1/Unity.app/Contents/MacOS/Unity -batchmode -quit -projectPath unity/Echapee4D -executeMethod MYB145CaptureRigHelper.RunBatchCapture -myb145Ticket MYB-168 -myb145State after -myb145Scene Assets/Scenes/MYB89UnityMcpProbe.unity -logFile _bmad-output/unity-test-results/myb-168-finalize-capture.log
```

Result: `PASS`, exit code `0`.

Final evidence:

- Route capture:
  `_bmad-output/visual-checkpoints/MYB-168/2026-06-19T16-51-33Z-after-route.png`
- Overview capture:
  `_bmad-output/visual-checkpoints/MYB-168/2026-06-19T16-51-33Z-after-overview.png`
- Capture metadata:
  `_bmad-output/visual-checkpoints/MYB-168/2026-06-19T16-51-33Z-capture-metadata.json`
- Capture report:
  `_bmad-output/visual-checkpoints/MYB-168/2026-06-19T16-51-33Z-capture-report.md`

## Visual Verdict

The route-camera capture now keeps the road readable while allowing trees to sit
close to the ride corridor. The scene no longer relies on pushing forest assets
far away to satisfy the guardrail, and the road/shoulder palette is less yellow.

This is a targeted route-readability and guardrail finalization. It is not a
claim that the full scene has reached final `Premium target` art quality.

## Residual Risk

The remaining visual limitations are broader art-direction polish: checkpoint
props, route-side material richness, and overall forest asset fidelity can still
be improved in later visual tickets. MYB-168 specifically closes the analytical
gap that allowed an intrusive canopy to mask the route while preserving close
forest framing.

# MYB-165 Implementation Report

## Summary
MYB-165 creates the first real playable route target: an approximately three-minute mock-mode ride from first-person bicycle POV.
Human visual QA caught an inherited route-visible prop reading as levitating; MYB-165 now blocks unsupported scenic masses in addition to cockpit cue support.

## Route
- scene: `Assets/Scenes/MYB89UnityMcpProbe.unity`
- generated root: `MYB165_FirstTrueRouteRoot`
- route length: `2370.048m`
- normal mock speed: `12.5m/s`
- estimated duration: `3:10`
- target window: `2:40` to `3:20`
- duration target reached: `Yes`

## Composition
- The original forest passage remains in the first 245m and is reintegrated through MYB-163.
- The extended route adds long meadow shoulders, grounded distant mounds, grouped trees, checkpoint beats and a clear finish marker.
- Legacy route-visible probe village/horizon props that read unsupported from bike POV are retired for this MYB-165 route.
- This is not a new art-direction forest pass and does not claim Premium target.

## Bike POV
- first-person camera lowered and moved closer to the bicycle axis
- subtle bob, look-ahead and turn lean configured
- supported handlebar/stem/fork/front-wheel cues added under `MYB165_BikePOVCues`
- external/flythrough view is not used as the primary validation surface

## Visual Support Guard
- legacy MYB-44/MYB-89 horizon village props are not accepted as MYB-165 bike-POV evidence because they can read as unsupported or floating at route speed
- route-visible unsupported scenic masses are blocking for MYB-165 video review
- grounded distant mounds are generated low and side-offset so they do not read as suspended canopy disks

## Metrics
- metrics JSON: `_bmad-output/implementation-artifacts/MYB-165/myb-165-first-true-route-metrics.json`
- route markers: `23`
- route segments: `22`
- smoothed route points: `177`
- checkpoints: `4`
- tree groups: `21`
- hill masses: `22`
- stone markers: `12`
- retired legacy unsupported props: `25`
- route-visible unsupported scenic mass count: `0`

## Visual Evidence
- MYB-145 capture report: `_bmad-output/visual-checkpoints/MYB-165/2026-06-19T16-50-44Z-capture-report.md`
- primary video capture: Unity Recorder via `Tools/MyBike/MYB-165/Capture Full Route Video (Unity Recorder)`
- Unity Recorder report: `_bmad-output/implementation-artifacts/MYB-165/myb-165-video-capture-recorder-report.md`
- fallback video frames: not generated in this build; Recorder is the primary video evidence

## MYB-144 Validation
- verdict: `PASS`
- errors: `0`
- warnings: `0`
- report: `_bmad-output/unity-test-results/myb-144-art-asset-validator-report.md`

## Warnings
### Capture warnings
- None recorded.
### Capture errors
- None recorded.
### Blocking errors
- None recorded.

## Governance
- no Meshy/Tripo/Poly Haven generation: Yes
- gameplay/FTMS/resistance model modified: No
- mock mode preserved: `Yes`
- canonical scene modified: Yes, scoped to the first true route
- Premium target reached: No

## Verdict
- First playable route: Yes
- Bike POV incarne with supported cockpit cues: Yes
- Recommended Linear status: In Review

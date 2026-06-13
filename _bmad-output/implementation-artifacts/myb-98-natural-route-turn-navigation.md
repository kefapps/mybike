# MYB-98 - Natural Route Turn Navigation

## Status

Implemented locally on branch `MYB-98-natural-route-turn-navigation`.

## Summary

- Added a shared Unity route trajectory sampler that builds a bounded smoothed
  path from the existing route control markers.
- Updated the rider/camera pose to follow the smoothed path, use a short
  look-ahead through turns, and apply a subtle configurable lean.
- Rebuilt the visible route mesh, edge lines, center dashes, roadside rhythm,
  marker arches, and difficulty cues from the same smoothed trajectory.
- Added `MYB98RideTrajectoryValidator` as a pure Unity unit validator for the
  trajectory sampler.
- Preserved mock-mode ride, HUD difficulty, resistance mapping, and the existing
  route control markers as readable authoring/debug anchors.
- Added Unity context terms for `Trajectoire de Ride` and `Virage Lisible`.

## Validation

- `unity-mcp-cli status unity/Echapee4D --timeout 10000`: PASS before scene
  rebuild and validation.
- `MYB89ProbeBuilder.BuildScene()`: PASS via Unity-MCP script execution.
- `MYB98RideTrajectoryValidator.ValidateRideTrajectoryCli()`: PASS via
  Unity-MCP script execution.
- `MYB89ProbeBuilder.ValidateProbeScene()`: PASS via Unity-MCP script execution.
- `node --check scripts/validate-local-ci.mjs`: PASS.
- `git diff --check`: PASS.

Evidence:

- `_bmad-output/unity-test-results/myb-89-unity-mcp-probe-validator.txt`
  records route length `245.2 m`, `65` smoothed trajectory samples, `8.0 m`
  turn look-ahead, `2.2 deg` max camera lean, and complete route difficulty cues.
- `_bmad-output/unity-test-results/myb-98-ride-trajectory-validator.txt`
  records pure sampler coverage for empty/single routes, straight route
  sampling, curved route length/deviation, wrap/clamp sampling, and null
  transform marker filtering.
- `_bmad-output/video-captures/myb-98-natural-turns-20260613-093900/myb-98-natural-turns-720p-muted.mp4`
  is a 6-second H.264 capture at 1280x720, 24 fps, with no audio stream.

## Capture

The MYB-98 capture was rendered from the Unity scene with canvases disabled, so
no extra text overlay is added. The encoded MP4 is muted via `-an` and validated
with `ffprobe` as `AUDIO_STREAMS:0`.

## Scope Guard

- No WebGL or React prototype work.
- No connected-bike, BLE, FTMS, firmware, route selection, or multi-route system.
- No wide scenic-corridor rewrite beyond route-bound placement alignment.

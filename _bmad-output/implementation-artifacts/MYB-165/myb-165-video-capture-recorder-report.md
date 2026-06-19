# MYB-165 Unity Recorder Video Capture Report

## Summary
MYB-165 now has a Unity Recorder based capture path for the full first-person route video.

## Capture
- method: `Unity Recorder com.unity.recorder`
- status: `complete`
- output scene: `Assets/Scenes/MYB89UnityMcpProbe.unity`
- camera source: `MainCamera`
- resolution: `1280x720`
- frame rate: `30fps`
- route speed: `12.5m/s`
- duration: `189.604s`
- frame count: `5689`

## Output
- MP4: `_bmad-output/video-captures/MYB-165/myb-165-first-true-route-recorder-2026-06-18T10-03-51Z/myb-165-first-true-route-bike-pov-3min-720p-30fps.mp4`
- summary: `_bmad-output/video-captures/MYB-165/myb-165-first-true-route-recorder-2026-06-18T10-03-51Z/capture-summary.json`
- contact sheet: `_bmad-output/video-captures/MYB-165/myb-165-first-true-route-recorder-2026-06-18T10-03-51Z/myb-165-first-true-route-contact-sheet-recorder-30fps.jpg`
- frame check 5s: `_bmad-output/video-captures/MYB-165/myb-165-first-true-route-recorder-2026-06-18T10-03-51Z/frame-check-5s.jpg`
- frame check 20s: `_bmad-output/video-captures/MYB-165/myb-165-first-true-route-recorder-2026-06-18T10-03-51Z/frame-check-20s.jpg`
- frame check 40s: `_bmad-output/video-captures/MYB-165/myb-165-first-true-route-recorder-2026-06-18T10-03-51Z/frame-check-40s.jpg`
- frame check 120s: `_bmad-output/video-captures/MYB-165/myb-165-first-true-route-recorder-2026-06-18T10-03-51Z/frame-check-120s.jpg`

## Verification
- ffprobe: `h264`, `1280x720`, `30/1`, `5689` frames, `189.633333s`
- visual frame check: nonblank first-person bike POV; route and supported cockpit cue visible
- visual support check: inherited red/brown horizon/village prop from the reported levitation screenshot is absent at the 20s checkpoint frame
- remaining visual risk: the route still reads prototype and does not claim Premium target

## Fallback
The previous RenderTexture JPG sequence path remains available as `Tools/MyBike/MYB-165/Capture Full Route Video Frames Fallback`.

## Errors
- None recorded.

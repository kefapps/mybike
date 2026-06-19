# MYB-166 macOS Build FPS Summary

- Scene: `Assets/Scenes/MYB89UnityMcpProbe.unity`
- App: `_bmad-output/unity-macos-builds/MYB-166/EchappeeMYB166.app`
- Route length: `2370.048m`
- Mode: compiled macOS build; Editor Play Mode/MCP is diagnostic only.
- Budget: target 60 FPS, warning below 45 FPS, red below 30 FPS.

## Runs

- `route-camera-worst-case-slice`: status `target`, avg `56.6`, 1% low `56.8`, min `0.5`, meters `0.0 -> 320.0`, report `_bmad-output/implementation-artifacts/MYB-166/myb-166-runtime-fps-route-camera-worst-case-slice.txt`
- `full-route-3min-validation`: status `target`, avg `58.5`, 1% low `56.8`, min `0.3`, meters `0.0 -> 2370.0`, report `_bmad-output/implementation-artifacts/MYB-166/myb-166-runtime-fps-full-route-3min-validation.txt`

## Notes

- Editor Play Mode/MCP measurements are diagnostic only for this issue because they stayed around 5-10 FPS even after renderers, point/spot lights, ride controllers, cue controllers, and UI were disabled in Play Mode.
- The compiled macOS player is the product FPS surface for MYB-166.
- The first-frame minimum is retained in the raw report, but the 1% low is the more useful stability signal after warmup.

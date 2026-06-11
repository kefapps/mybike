# Unity WebGL Readiness Spike

Date: 2026-06-11

Linear issue: `MYB-90`

URL: https://linear.app/kefjbo/issue/MYB-90/myb-007-spike-unity-webgl-readiness-depuis-scene-unity-mcp-propre

## Intent

Create a bounded follow-up spike after MYB-89 to test whether the clean
Unity-MCP probe scene can survive a real Unity WebGL export and browser
readiness check.

## Source Evidence

- MYB-89 report: `_bmad-output/implementation-artifacts/myb-89-unity-mcp-ivanmurzak-probe.md`
- MYB-89 scene: `unity/Echapee4D/Assets/Scenes/MYB89UnityMcpProbe.unity`
- MYB-89 video: `_bmad-output/video-captures/myb-89-unity-mcp-probe-20260611-080446/unity-mcp-probe-6s.mp4`
- MYB-39 audit: `_bmad-output/implementation-artifacts/myb-39-unity-webgl-audit-2026-06-11.md`

## Scope

- Use the clean Unity-MCP probe project `unity/Echapee4D`.
- Export only the MYB-89 probe scene to WebGL.
- Serve the generated build locally.
- Validate browser load, canvas nonblank, runtime console, motion/readability,
  and capture a short video/contact sheet.
- Document bundle/build size and exact commands.

## Out Of Scope

- Reopening the old `unity/Echappee3D` demo.
- Changing React/Vite/Three.js production code.
- Migrating MyBike to Unity.
- Adding Meshy, BLE/FTMS, backend, asset pipeline, real bike hardware, or public deploy.

## Acceptance

1. WebGL build is produced from `unity/Echapee4D`.
2. Local server loads the build in a browser.
3. Browser evidence includes HTTP status, console errors/warnings, canvas
   nonblank check, and screenshot/video.
4. Visual evidence shows whether the route/corridor remains readable after
   WebGL export.
5. Report concludes with `ready`, `not-ready`, or `blocked`.
6. MYB-39 receives an engine-decision recommendation based on the result.

## Hard Stop

Stop and document `blocked` if WebGL tooling, Unity package state, MCP
connection, build size, local serving, or browser runtime failures consume the
timebox without a playable/readable proof.

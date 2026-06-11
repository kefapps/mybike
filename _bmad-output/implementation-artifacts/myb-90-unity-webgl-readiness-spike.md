# MYB-90 - Unity WebGL Readiness Spike

Date: 2026-06-11

Linear issue: `MYB-90`

URL: https://linear.app/kefjbo/issue/MYB-90/myb-007-spike-unity-webgl-readiness-depuis-scene-unity-mcp-propre

Parent decision context: `MYB-39`

## Verdict

`ready`

The clean `unity/Echapee4D` Unity-MCP probe can be exported to WebGL, served
locally, loaded in Chromium, and captured as a readable moving ride proof.
This clears the narrow WebGL-readiness spike.

This is not enough to recommend pivoting the current MyBike MVP away from the
Three.js track. The recommendation for `MYB-39` is:

- Keep the current Three.js/Web app path as the private-demo/MVP default.
- Continue Unity as a separate exploration lane now that Unity-MCP plus WebGL
  can produce a browser-visible proof.
- Only reconsider a Unity-first pivot after a hardening ticket covers load
  time, build size, WebGL warning cleanup, responsiveness, deterministic
  automation, and integration boundaries with React/session state.

## Scope Guardrails

- No `src/**` edits.
- No `unity/Echappee3D/**` edits.
- No Meshy, BLE/FTMS, backend, external asset pipeline, real-bike hardware, or
  public deployment work.
- Used the clean Unity-MCP project from `MYB-89`: `unity/Echapee4D`.

## Setup

- Unity project: `unity/Echapee4D`
- Unity version: `6000.4.10f1`
- Unity-MCP package / CLI observed previously: `0.80.0`
- Source scene: `Assets/Scenes/MYB89UnityMcpProbe.unity`
- Build helper: `unity/Echapee4D/Assets/MYB90/Editor/MYB90WebGLReadinessBuilder.cs`
- Capture helper: `scripts/capture-unity-webgl-readiness.mjs`

`unity-mcp-cli status unity/Echapee4D --timeout 10000` is healthy after the
status fix: the configured server at `http://localhost:8081` is connected, and
the local MCP URL `http://localhost:20376` is reported as skipped/info because
the fixed configured server is in use.

## Build Evidence

Build report:
`_bmad-output/unity-test-results/myb-90-unity-webgl-build.txt`

Generated build:
`_bmad-output/unity-webgl-builds/myb-90-unity-mcp-probe`

Result:

- Status: `Succeeded`
- Build target supported: `True`
- Unity: `6000.4.10f1`
- Reported build duration: `4.4s` on the final cached run
- Build report warnings/errors: `0` warnings, `0` errors
- Total reported size: `78,075,757` bytes
- Build directory size on disk: about `75M`
- Largest files:
  - `Build/myb-90-unity-mcp-probe.wasm`: `61,032,893` bytes
  - `Build/myb-90-unity-mcp-probe.data`: `16,505,871` bytes
  - `Build/myb-90-unity-mcp-probe.framework.js`: `491,616` bytes

The build is viable for a local proof. The size is still too heavy to treat as
production-ready without a separate optimization pass.

## Browser Evidence

Capture summary:
`_bmad-output/video-captures/myb-90-unity-webgl-readiness-2026-06-11T08-46-03-838Z/capture-summary.json`

Visual evidence:

- Video: `_bmad-output/video-captures/myb-90-unity-webgl-readiness-2026-06-11T08-46-03-838Z/unity-webgl-readiness.mp4`
- Contact sheet: `_bmad-output/video-captures/myb-90-unity-webgl-readiness-2026-06-11T08-46-03-838Z/unity-webgl-readiness-contact-sheet.jpg`
- Canvas screenshot: `_bmad-output/video-captures/myb-90-unity-webgl-readiness-2026-06-11T08-46-03-838Z/canvas-frame.png`
- Page screenshot: `_bmad-output/video-captures/myb-90-unity-webgl-readiness-2026-06-11T08-46-03-838Z/page-frame.png`

Measured result:

- Local URL: `http://127.0.0.1:8090/`
- HTTP status: `200`
- Failed requests: `0`
- Page errors: `0`
- Console errors: `0`
- Console warnings: `7`
- Canvas screenshot nonblank: `true`
- Canvas screenshot luma: avg `100.352`, min `22`, max `255`
- Video: `33.24s`, `1280x720`, `25fps`, H.264 MP4

The rendered route is readable after WebGL export: road, lane marker, route
props, cockpit silhouette, distance/speed HUD, and forward motion are visible
in both screenshot and video evidence.

## Caveats

- Browser console warnings remain:
  - repeated WebGL `INVALID_ENUM getInternalformatParameter` warnings
  - one URP warning: `Hidden/Universal Render Pipeline/Edge Adaptive Spatial Upsampling` is not supported
- The capture script's direct final in-page canvas pixel read returned blank,
  while the saved canvas screenshot, page screenshot, and video are visibly
  nonblank. Treat this as a WebGL readback/preserveDrawingBuffer caveat, not as
  a runtime failure.
- Unity splash/loading time and WebGL payload size still need explicit product
  thresholds before Unity can be considered for a demo-facing path.

## Commands Run

```bash
unity-mcp-cli status unity/Echapee4D --timeout 10000
```

```text
MCP script_execute -> MYB90.Editor.MYB90WebGLReadinessBuilder.BuildForMcp()
```

```bash
node scripts/capture-unity-webgl-readiness.mjs
```

```bash
git diff --check
```

`npm run typecheck`, `npm run test`, and `npm run build` were intentionally not
run for this spike because no React/Vite/Three.js production code changed.


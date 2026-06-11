# MYB-89 - Unity-MCP IvanMurzak Probe

Date: 2026-06-11

## Verdict

`promising`, with scope limits.

IvanMurzak Unity-MCP was able to drive a fresh Unity project from Codex well enough to create, validate, run, and capture a small readable demo. The result is materially better than the earlier Unity WebGL artifact for proving a simple first-person ride feel: the route is a real corridor surface, close roadside markers move past camera, the cockpit/HUD are visible, and a 6 second proof video was produced.

This does not justify moving the current V1/private demo away from Three.js. It does support keeping Unity-MCP as a viable Unity exploration path for later production-target probes.

## Project

- Unity project: `unity/Echapee4D`
- Unity version: `6000.4.10f1`
- Unity-MCP CLI: `unity-mcp-cli` `0.80.0`
- Unity package: `com.ivanmurzak.unity.mcp` `0.80.0`
- MCP server used: `http://localhost:8081`
- Caveat: `unity-mcp-cli status` still probes local MCP URL `http://localhost:20376`, which returns connection refused, but the configured server on `8081` is reachable and real tool calls work.

## Implemented Probe

- Scene: `unity/Echapee4D/Assets/Scenes/MYB89UnityMcpProbe.unity`
- Runtime: `unity/Echapee4D/Assets/MYB89/Runtime/MYB89ProbeRide.cs`
- Builder/validator/capture: `unity/Echapee4D/Assets/MYB89/Editor/MYB89ProbeBuilder.cs`
- Content:
  - 245 m route mesh corridor with edge lines and center dash markers.
  - 9 route markers.
  - 130 renderers.
  - Close roadside posts, arches, trees, hills, cockpit bars, camera and HUD.
  - Runtime autoplay movement at 12.5 m/s with HUD distance/speed/progress.

## MCP Actions Proven

- `console_get_logs`: checked recent Unity errors/warnings.
- `scene_get_data`: inspected open scene hierarchy before and after generation.
- `script_execute`: refreshed assets, built the scene, validated the scene, captured frames, entered Play mode, read runtime progress, and exited Play mode.
- CLI status: `unity-mcp-cli status unity/Echapee4D --timeout 10000`.

## Evidence

- Validator report: `_bmad-output/unity-test-results/myb-89-unity-mcp-probe-validator.txt`
- Screenshot: `_bmad-output/video-captures/myb-89-unity-mcp-probe-20260611-080446/screenshot.png`
- Video: `_bmad-output/video-captures/myb-89-unity-mcp-probe-20260611-080446/unity-mcp-probe-6s.mp4`
- Contact sheet: `_bmad-output/video-captures/myb-89-unity-mcp-probe-20260611-080446/contact-sheet.jpg`

Video metadata:

- Codec: H.264
- Resolution: 1280x720
- Frame rate: 12 fps
- Duration: 6.0 s
- Size: 504 KB

Visual review:

- Screenshot is nonblank and correctly framed.
- Route surface, lane dashes, edge lines, trees/posts, cockpit and HUD are readable.
- Contact sheet shows progression through roadside markers and arches.

Play mode check:

- Entered Play mode via MCP `script_execute`.
- Runtime readback: `isPlaying=True; progressMeters=242.1; routeLength=245.0; speed=12.5`.
- Exited Play mode via MCP `script_execute`.

## Validation

- Unity validator: pass.
  - Route markers: 9.
  - Route length: 245.0 m.
  - Renderer count: 130.
- Unity console after rebuild/validation/capture/play check:
  - Errors: none in final recent window.
  - Warnings: none in final recent window.
- `git diff --check`: pass.
- Targeted trailing whitespace scan on edited text/C# files: pass.
- High-confidence secret scan on edited text/C#/Unity scene files: pass.
- `unity/Echappee3D/**`: unchanged.
- `src/**`: unchanged.
- `npm run typecheck`, `npm run test`, and `npm run build` were not run
  because this spike did not modify the React/Vite/Three.js app.

## Assessment Against The Prior Unity WebGL Attempt

Better:

- The workflow produced a visible, readable first-person scene quickly from a fresh project.
- MCP tool calls were concrete and repeatable enough to build, validate and capture evidence.
- The output avoids the previous "dark ribbon in empty world" failure mode.

Still limited:

- This is Editor/camera-render evidence, not a WebGL build proof.
- The Unity project remains heavier and noisier than the web stack.
- The CLI status path is confusing because `20376` fails while `8081` works.
- There is still no proof that a Unity WebGL export would be small, stable, or pleasant enough for the current private demo.

## Recommendation For MYB-39

Keep Three.js as the V1/private-demo engine.

Keep Unity-MCP IvanMurzak as a promising Unity authoring/probe workflow, with the next Unity-only question being a strict WebGL export/readiness test from this clean scene. Do not restart a broad Unity migration from this spike alone.

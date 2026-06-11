# MYB-39 - Unity WebGL gameplay audit

Date: 2026-06-11
Scope: existing MYB-17 Unity WebGL build, not a rebuild
Build audited: `_bmad-output/unity-webgl-builds/myb-17-private-demo/`
Capture: `_bmad-output/video-captures/unity-webgl-audit-2026-06-11T06-00-59-710Z/`

## Evidence

- Video: `_bmad-output/video-captures/unity-webgl-audit-2026-06-11T06-00-59-710Z/unity-webgl-audit-30s.mp4`
- Contact sheet: `_bmad-output/video-captures/unity-webgl-audit-2026-06-11T06-00-59-710Z/unity-webgl-audit-contact-sheet.jpg`
- Capture report: `_bmad-output/video-captures/unity-webgl-audit-2026-06-11T06-00-59-710Z/capture-report.json`
- Screenshots: `_bmad-output/video-captures/unity-webgl-audit-2026-06-11T06-00-59-710Z/screenshots/`

Method:

- Served the existing WebGL output on `http://127.0.0.1:8087/`.
- Opened the page with Playwright at `1920x1080`.
- Waited for the Unity canvas, clicked Start inside the canvas, captured the run, then generated screenshots and a contact sheet.
- Stopped the local server after capture.

Measured facts:

- WebGL build directory size: `34M`.
- Main payloads: `myb-17-private-demo.wasm` is `28M`, `myb-17-private-demo.data` is `5.3M`.
- Video duration after conversion: `41.96s` at `25fps`, including Unity load plus running session.
- Capture report: `httpStatus=200`, `pageErrors=0`.
- Console warnings: `6`, all `WebGL: INVALID_ENUM: getInternalformatParameter: invalid internalformat`.
- Canvas box: `960x600` centered inside a `1920x1080` page. The default Unity template frame remains visible.

## Verdict

The build is not usable as a WebGL gameplay demo.

It is technically alive: the page loads, Unity initializes, the session enters `Running`, and the HUD counters advance. But the player-facing result is a route placeholder moving through an empty scene. It does not prove a ride experience.

## Findings

### 1. The capture validates a session tick, not a playable ride

Evidence:

- The fresh capture reaches `State Running`.
- At `t+30s`, HUD reports `Time 30s`, `Distance 122 m`, `Progress 12%`.
- The same frame still shows an empty horizon, a dark route strip, flat terrain, and no readable near-side motion cues.

Severity: High.

Why it matters: for Echappee 3D, the demo value is not "a timer advances". It is "the user believes they are riding through a scenic route". This build fails that need.

### 2. The route is a placeholder ribbon

Evidence:

- `unity/Echappee3D/Assets/Echappee/Rendering/RouteRendererPlaceholder.cs` uses a Unity `LineRenderer`.
- The report records `linePoints=7` and `lineWidth=4.00`.
- The screenshots show the route as a dark triangular strip occupying the bottom center of the frame.

Severity: High.

Cause: the route renderer produces visibility, not road readability. There are no shoulders, lane edges, ground blending, roadside margins, terrain deformation, or near markers.

### 3. The scene-life validator overstates the player-facing result

Evidence:

- MYB-17 reports `Projected visibility in camera cone: 4/8`.
- The fresh contact sheet does not show birds or humans as readable gameplay elements.
- Scene life objects are tiny simple primitives placed mostly far along the route, for example birds around `z=290..790` and humans around `z=300..760`.

Severity: High.

Cause: the validator checks geometric cone inclusion. It does not check on-screen size, contrast, occlusion by the route, silhouette readability, or whether an object affects the perception of motion.

### 4. MYB-17 was scoped to Editor readiness, not WebGL readiness

Evidence:

- MYB-17 story says the report can be text-only and no video capture is required.
- MYB-17 report says `Build Settings scenes: 0`, `RideMock enabled in Build Settings: False`.
- MYB-17 report says local macOS build was not attempted and was deferred.
- The verdict still says `ready-for-private-local-demo`.

Severity: Medium.

Cause: the acceptance criteria certified "can be shown locally from the Editor", not "can be packaged and played as a clean WebGL demo".

### 5. Unity WebGL adds friction without yet adding player value

Evidence:

- Existing MYB-17 WebGL build log reported success, but with a build duration around `129s`.
- The build output is `34M`, with a `28M` wasm payload.
- Fresh capture produced 6 WebGL warnings during init.
- Prior capture also logged GPU stall warnings around `ReadPixels`.

Severity: Medium.

This is not fatal by itself. It becomes a problem because the gameplay result is still weaker than the current Three.js path, while the iteration loop is heavier.

## Visual QA score

Scope: specific scene, Unity WebGL ride mock
Art style: 3D stylized prototype

- First impression: 2/10
- Art consistency: 3/10
- UI polish: 3/10
- Animation / motion readability: 2/10
- Screen adaptation: 3/10
- Performance visual: 5/10

Weighted visual score: 3.0/10

Status: DONE_WITH_CONCERNS

## Recommendation for MYB-39

Do not describe the MYB-17 WebGL artifact as functional. It is a running technical proof, not a usable gameplay demo.

ADR recommendation:

- Keep Web / Three.js as the V1 and private-demo engine.
- Keep the Unity project as spike evidence, not as the active route.
- Allow a new Unity attempt only as a clean, time-boxed probe with hard gates:
  - no dependency on the official Unity MCP for the critical path;
  - real build settings from day one;
  - batchmode validation and capture from a clean checkout;
  - route rendered as road/corridor/terrain, not a `LineRenderer`;
  - video evidence required before any "ready" verdict;
  - success defined as matching or beating the current Three.js ride feel within the timebox.

If Unity is tried again, do not iterate from this WebGL scene as-is. Start from a narrow "grounded route corridor" probe and treat the current scene as a failed packaging/feel reference.

# MYB-166 First Pass Performance Report

## Summary

MYB-166 investigated the FPS drop reported after MYB-165. The first pass found
one real runtime correctness/performance issue and separated Editor/MCP
throttling from compiled player performance.

## Changes Applied

- Replaced `MYB89_EventSystem` legacy `StandaloneInputModule` with
  `InputSystemUIInputModule`.
- Updated `MYB89ProbeBuilder` so future scene regeneration keeps the Input
  System-compatible UI module.
- Added MYB-166 ticket-local render and macOS player FPS probes.

No scenery was removed. No route trajectory, colliders, gameplay, HUD design, or
assets were intentionally reduced.

## Key Finding

Editor Play Mode via MCP is not a reliable final FPS signal for this route. It
reported approximately 5 FPS even when all renderers, point/spot lights, ride
controllers, cue controllers, and UI were disabled in the Play Mode session.

The compiled macOS player build gives the reliable product signal:

- `route-camera-worst-case-slice`: avg `56.6 FPS`, 1% low `56.8 FPS`.
- `full-route-3min-validation`: avg `58.5 FPS`, 1% low `56.8 FPS`.

This is not a hard red failure anymore, but it is still below a clean stable 60
FPS target.

## Validation Evidence

- Build FPS summary:
  `_bmad-output/unity-test-results/MYB-166/myb-166-macos-build-fps-summary.md`
- Worst-case route-camera report:
  `_bmad-output/implementation-artifacts/MYB-166/myb-166-runtime-fps-route-camera-worst-case-slice.txt`
- Full-route report:
  `_bmad-output/implementation-artifacts/MYB-166/myb-166-runtime-fps-full-route-3min-validation.txt`
- Render-cost proxy:
  `_bmad-output/implementation-artifacts/MYB-166/myb-166-route-camera-render-probe.md`

## Remaining Optimization Targets

The player build is close to target but not stable 60. Next MYB-166 work should
A/B test reversible optimizations before content removal:

- scene shadow policy;
- URP additional light/shadow settings;
- static flags and batching;
- culling distances for small/far scenery;
- LOD/impostors for far forest and hills;
- asset import settings from MYB-50.

## Verdict

- Performance regression understood: Partially.
- First reversible optimization/fix applied: Yes.
- Compiled player red failure: No.
- Stable 60 FPS reached: No.
- Human performance acceptance: Yes, Julien accepted the measured Player
  performance as OK on 2026-06-18.
- Recommended MYB-166 status: `Done`.
- Recommended MYB-165 status: resume `In Review`; the FPS blocker is no longer
  blocking human validation.

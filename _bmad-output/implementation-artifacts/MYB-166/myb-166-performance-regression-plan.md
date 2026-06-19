# MYB-166 Performance Regression Plan

Date: 2026-06-18
Linear: https://linear.app/kefjbo/issue/MYB-166/performance-regression-gate-first-true-route-optimization
Branch: `MYB-166-performance-regression-first-route-optimization`

## Summary

MYB-166 investigates the FPS drop observed after MYB-165 introduced the first
complete playable route. MYB-165 remains `In Review` until the performance
regression is understood and Julien validates the bike POV route experience.

This ticket must search for missed Unity and asset optimizations before removing
visible content. Content reduction is a last resort, not the first response.

## Baseline

- Source branch: `MYB-165-premier-vrai-parcours-jouable-3-minutes`.
- Starting commit: `cfa4e8a`.
- Canonical scene: `Assets/Scenes/MYB89UnityMcpProbe.unity`.
- Product view: route-camera / first-person bike POV.
- Target platform: Unity macOS first, Editor Play Mode first.

## Performance Targets

- Target: 60 FPS.
- Warning: below 45 FPS.
- Red: below 30 FPS.

These are decision signals inherited from MYB-51. A red result blocks MYB-165
human validation until investigated.

## Required Scenarios

### route-camera-worst-case-slice

- Duration: 30 to 45 seconds.
- Scope: heaviest visible route-camera section.
- Purpose: fast iteration on CPU/GPU/rendering/shadow causes.

### full-route-3min-validation

- Duration: full MYB-165 route, approximately 189.6 seconds.
- Purpose: validate that optimizations hold across the complete playable route.

## Metrics To Capture

- FPS average / minimum / p95 frame time when available.
- CPU-bound vs GPU-bound indication when available.
- Batches.
- SetPass calls.
- Draw calls.
- Triangles / vertices.
- Shadow caster count.
- Renderer count.
- Material count.
- Active lights.
- Shadow settings.
- URP asset / renderer snapshot.
- Memory and texture/mesh pressure when available.

## Optimization Matrix

Test these before removing assets:

1. URP experimental profile
   - Create or use a reversible MYB-166 performance probe profile.
   - Compare against current PC profile before any global adoption.

2. URP render passes
   - Depth Texture.
   - Opaque Texture.
   - SSAO.
   - Render Scale.

3. Shadows
   - Main light shadow resolution.
   - Shadow distance.
   - Cascade count.
   - Soft shadow quality.
   - Additional light shadows.
   - Per-family cast/receive shadow policy.

4. Static rendering
   - Static flags for immobile scenery.
   - Static batching where it wins.
   - Avoid blind batching if profiling shows culling gets worse.

5. Culling
   - Camera layer cull distances for small props and far scenery.
   - GPU occlusion URP if compatible.
   - Baked occlusion only if the scene shape makes it worthwhile.

6. LODs / impostors
   - Hills.
   - Back wall forest masses.
   - Far tree groups.
   - Small markers and stones.

7. Asset import / materials
   - Texture mipmaps and compression.
   - Read/Write disabled where possible.
   - Mesh compression only after visual check.
   - Instancing tested against SRP Batcher, not assumed.

## Guardrails

- Preserve bike POV.
- Preserve route readability.
- Preserve mock mode.
- Do not modify gameplay, route trajectory, route colliders, HUD, telemetry, or
  FTMS.
- Do not generate Meshy, Tripo, Poly Haven, or Blender assets in this ticket.
- Do not globally change the PC URP profile without measurement and visual
  comparison.
- Do not delete or reduce route-visible premium scenery before the optimization
  matrix has been tested.

## Initial Deliverables

- `myb-166-performance-regression-plan.md`
- `myb-166-initial-research-findings.md`
- `myb-166-static-scene-scan.json`

## Expected Verdict

- Performance regression understood: Yes/No.
- First reversible optimization wave applied: Yes/No.
- Bike POV preserved: Yes/No.
- Route readability regression: Yes/No.
- Recommended MYB-165 status impact: remain `In Review` or ready for human
  validation.

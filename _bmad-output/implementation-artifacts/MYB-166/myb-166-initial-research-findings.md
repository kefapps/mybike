# MYB-166 Initial Research Findings

Date: 2026-06-18
Scope: research and static scan only. No Unity scene, gameplay, asset, or render
pipeline settings have been modified.

## Unity Documentation Findings

Unity's graphics performance guidance recommends profiling before changing the
scene, because CPU-bound and GPU-bound rendering problems require different
fixes:

- https://docs.unity3d.com/6000.4/Documentation/Manual/OptimizingGraphicsPerformance.html

Unity's Rendering Profiler exposes the metrics MYB-166 needs to capture:
Batches, SetPass Calls, Draw Calls, Triangles, Vertices, Render Textures, and
Shadow Casters:

- https://docs.unity3d.com/6000.4/Documentation/Manual/ProfilerRendering.html

Unity's Highlights Profiler helps identify whether missed frame targets are CPU
or GPU bound:

- https://docs.unity3d.com/6000.0/Documentation/Manual/ProfilerHighlights.html

URP performance guidance specifically calls out these settings as optimization
levers:

- Additional light shadows.
- Additional lights per-object limit.
- Shadow atlas and shadow resolution.
- Cascade count.
- Fast sRGB/Linear conversion.
- LUT size.
- Main light shadows.
- Shadow max distance.
- Opaque texture/downsampling.
- Render scale.
- Soft shadows.

Source:

- https://docs.unity3d.com/6000.4/Documentation/Manual/urp/optimize-for-better-performance.html

URP shadow optimization identifies the main cost drivers as visible shadow
casters, visible shadow receivers, shadow-casting lights, cascade count, shadow
resolution, and soft shadows:

- https://docs.unity3d.com/6000.0/Documentation/Manual/shadows-optimization.html
- https://docs.unity3d.com/6000.0/Documentation/Manual/shadow-cascades-performance.html

Unity documents SRP Batcher as a CPU-side render-state optimization for SRP
projects. It is enabled in the current PC URP asset, so instancing must be tested
instead of assumed:

- https://docs.unity3d.com/6000.4/Documentation/Manual/SRPBatcher.html

Unity documents static batching and LODs as relevant optimizations for many
static meshes, with trade-offs:

- https://docs.unity3d.com/6000.4/Documentation/Manual/DrawCallBatching.html
- https://docs.unity3d.com/6000.3/Documentation/Manual/lod-group-configure.html

Unity documents per-layer camera culling distances as a way to cull small
objects earlier without deleting them:

- https://docs.unity3d.com/6000.4/Documentation/ScriptReference/Camera-layerCullDistances.html

Unity documents mip maps, texture import limits, compression, and async texture /
mesh upload eligibility as asset-side optimization levers:

- https://docs.unity3d.com/2022.3/Documentation/Manual/class-TextureImporter.html
- https://docs.unity3d.com/560/Documentation/Manual/ImportingTextures.html
- https://docs.unity3d.com/2018.4/Documentation/Manual/LoadingTextureandMeshData.html

## Project Findings

Current static scan of `Assets/Scenes/MYB89UnityMcpProbe.unity` from the MYB-165
branch:

- MeshRenderers: 709.
- GameObjects: 815.
- Renderers casting shadows: 709.
- Renderers receiving shadows: 709.
- GameObjects with `m_StaticEditorFlags: 0`: 815.
- LODGroups: 0.
- MeshColliders: 1.
- BoxColliders: 81.
- SphereColliders: 41.

Current `Assets/Settings/PC_RPAsset.asset` notable settings:

- Depth Texture: enabled.
- Opaque Texture: enabled.
- Render Scale: 1.
- Main light shadows: enabled.
- Main light shadowmap resolution: 2048.
- Additional lights: enabled.
- Additional lights per-object limit: 4.
- Additional light shadows: enabled.
- Additional light shadowmap resolution: 2048.
- Shadow distance: 50.
- Shadow cascade count: 4.
- Soft shadows: enabled.
- Soft shadow quality: 3.
- SRP Batcher: enabled.
- Dynamic batching: disabled.
- Adaptive performance: enabled.
- GPU Resident Drawer: enabled.
- GPU occlusion culling in cameras: disabled.

Current `Assets/Settings/PC_Renderer.asset` notable settings:

- SSAO renderer feature: active.
- SSAO downsample: disabled.
- SSAO source: normals.
- Rendering mode: deferred.
- Shadow transparent receive: enabled.

Current material instancing scan:

- MYB-165 materials: 12 total, 12 with instancing disabled.
- MYB-163 materials: 13 total, 13 with instancing disabled.
- Project materials with instancing disabled: 110.

## Likely Missed Optimizations Before Removing Assets

1. Shadow policy is the highest-confidence suspect.
   - The scene currently has 709 renderers casting and receiving shadows.
   - Many far hills, distant forest masses, stones, and route markers likely do
     not need both cast and receive shadows.

2. URP experimental profile should be tested before visual cuts.
   - Disable or reduce SSAO, opaque texture, depth texture, soft shadows,
     cascades, and additional shadows in a reversible MYB-166 profile.

3. Static flags and batching are unexploited.
   - The scenery is mostly static, but the scene scan reports 815 zero static
     flags.

4. Culling is underused.
   - The scene is a 2.37 km route. Layer culling distances can reduce small and
     far props without deleting them.

5. LODs are absent.
   - The route now has distant hills and back-wall forest masses but zero
     LODGroups.

6. Instancing is worth testing but not guaranteed.
   - SRP Batcher is already enabled, so the correct move is an A/B test rather
     than globally enabling instancing.

7. Asset import settings should be audited.
   - MYB-50 already defines texture/model/material guardrails.
   - Read/Write, mipmaps, compression, and mesh compression should be audited
     before content cuts.

## Recommended First Probe Order

1. Capture current Unity status and available profiler snapshots.
2. Add a ticket-local performance probe runner if built-in MCP profiler
   snapshots are insufficient for route-camera FPS.
3. Measure current MYB-165 baseline.
4. A/B test reversible URP settings.
5. A/B test shadow policy.
6. A/B test static flags / batching / culling.
7. Only then consider content reduction.

## Initial Unity MCP Profiler Snapshot

Unity MCP status is reachable for `unity/Echapee4D`.

The initial MCP profiler snapshots reported:

- frame time: 302.442 ms;
- FPS: 3.306;
- vSync: 0;
- targetFrameRate: -1;
- rendering threading mode: MultiThreaded;
- graphics device: Metal;
- graphics memory: 249.659 MB;
- total allocated memory: 387.263 MB;
- Mono / GC memory: approximately 1138 MB.

Interpretation:

- This confirms a severe performance problem in the current Editor state.
- The MCP profiler tool is a single-frame snapshot, not a route-camera benchmark.
- MYB-166 still needs a route-camera benchmark runner to measure average, min,
  and p95 frame time for the worst-case slice and full route.

Snapshot artifact:

- `_bmad-output/implementation-artifacts/MYB-166/myb-166-initial-unity-profiler-snapshot.json`

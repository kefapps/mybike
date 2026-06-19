# MYB-166 Route Camera Render Probe

- Scene: `Assets/Scenes/MYB89UnityMcpProbe.unity`
- Route length: `2370.048m`
- Render target: `1280x720`
- Renderers: `747`
- Shadow casters: `747`
- Shadow receivers: `747`
- LODGroups: `0`
- Active lights: `6`
- Materials: `47`

## Discovery

- Samples: `61`
- Average render: `1.077ms`
- P95 render: `1.927ms`
- Max render: `4.887ms` at `0m`
- Average estimated FPS proxy: `1108.489`
- Worst visible renderers: `467`
- Worst visible shadow casters: `467`
- Worst visible triangles: `77579`

## Worst-Case Slice

- Samples: `17`
- Average render: `1.591ms`
- P95 render: `2.313ms`
- Max render: `2.686ms` at `55m`
- Average estimated FPS proxy: `710.535`
- Worst visible renderers: `277`
- Worst visible shadow casters: `277`
- Worst visible triangles: `62512`

## Full Route Validation

- Samples: `96`
- Average render: `0.891ms`
- P95 render: `1.59ms`
- Max render: `7.701ms` at `150m`
- Average estimated FPS proxy: `1476.165`
- Worst visible renderers: `180`
- Worst visible shadow casters: `180`
- Worst visible triangles: `46113`

## Interpretation

- All active renderers cast shadows; shadow policy is a high-priority optimization candidate.
- No LODGroups are present; far scenery LOD/impostor work is a high-priority optimization candidate.

## Caveat

This probe uses `Camera.Render` wall-clock timings in the Unity Editor. It is a render-cost proxy for comparing candidate optimizations, not a final Play Mode FPS benchmark.

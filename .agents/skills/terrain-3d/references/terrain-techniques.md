# Terrain Techniques Reference

This reference keeps reusable terrain techniques separate from the
Echappee-specific instructions in `SKILL.md`.

Use it for terrain implementation planning and review. Adapt techniques to
Unity and to the current Linear issue before writing production code.

## Terrain Representations

### Heightmap

A heightmap stores one elevation per X/Z sample. It is efficient, easy to edit
and works well for hills, valleys, broad slopes and distant landscape masses.

Good for:

- broad background terrain;
- fast terrain prototypes;
- imported or generated elevation data;
- height, slope and biome masks.

Limits:

- no true caves, arches, tunnels or overhangs;
- vertical cliffs are difficult without extra meshes;
- route geometry may need to be cut, flattened or overlaid separately.

### Route-First Mesh

A route-first mesh builds terrain from the rail/corridor outward. It usually
starts from route samples and lateral offsets: road center, road edges,
shoulders, verges and nearby scenic terrain.

Good for:

- first-person ride readability;
- deterministic road surface and height sampling;
- stylized silhouettes close to the camera;
- bridges, walls, cliffs, arches and authored scenic beats;
- controlling vertex count per route segment.

Limits:

- requires explicit mesh generation/import rules;
- needs seam handling between segments;
- needs a separate strategy for distant landscape.

### Volumetric Or Density Terrain

Density fields, marching cubes and similar techniques can create caves and
overhangs, but they are heavier and harder to art-direct. Use them only for
small, explicitly budgeted features. For Echappee 3D, authored cave/cliff
meshes are usually the safer first choice.

## Route Corridor Generation

For a corridor mesh, generate data from route samples:

1. Sample the route centerline at stable spacing.
2. Compute tangent, normal/right vector and smoothed grade.
3. Create lateral bands: road, edge, shoulder, verge, terrain blend.
4. Assign height offsets and crossfall per band.
5. Generate UVs or vertex colors from distance and surface type.
6. Store surface tags for future ride feel.
7. Create collision only where gameplay or camera needs it.

Keep the corridor deterministic: the same route definition should produce the
same mesh, height samples and surface tags.

## Height Sampling

A height sample should return enough information for placement, camera
composition and future feel:

```text
position -> height, normal, surfaceType, biomeId, confidence
```

Near the route, sample the route-first mesh or route profile directly. Far from
the route, sample supporting terrain, distant meshes or fallback planes.

Use bilinear interpolation for grid/heightmap data. Use triangle barycentric
sampling or explicit segment interpolation for custom meshes. Clamp or flag
out-of-bounds samples instead of returning invalid numbers.

## Noise Toolkit

Use noise as a shaping tool, not as the whole terrain design.

- FBM: rolling hills and broad organic variation.
- Ridged noise: mountain ridges, rocks and crag silhouettes.
- Domain warping: less regular landforms.
- Worley/cellular noise: rocky cells, cracks, material breakup.
- Masks: control where detail appears by biome, slope, height or distance from
  the route.

For Echappee 3D, noise should serve authored route moments. Prefer a readable
silhouette at low resolution over dense detail that the camera cannot parse.

## Biome And Surface Data

Terrain can carry multiple data layers:

- biome id;
- surface type;
- height;
- slope;
- moisture or vegetation mask;
- scenic density mask;
- premium signal slots.

In Unity, these can be represented as ScriptableObjects, serialized route
segments, vertex colors, UV channels, compact textures or authoring metadata.
Choose the simplest representation that the current issue can validate.

For `MYB-37`, keep V1 focused on:

- `Foret claire`;
- `Village / campagne pavee`.

## Texturing And Materials

### Vertex Colors

Fast and useful for low-poly production art. Good for broad color variation,
biome tinting, slope hints and masks. Limited when the surface needs crisp
material detail.

### UV Atlas

Good for stylized road and ground pieces. Keeps material count low and works
well with custom corridor meshes. Requires consistent UV rules per band.

### Slope/Height Shader

Blend grass, rock, dirt or moss by height and normal. Useful for distant masses
or supporting terrain. Keep shader branching and texture samples bounded.

### Splatmap Or Surface Mask

Use when several terrain materials must blend over a broad area. On mobile or
secondary WebGL validation, keep layer counts low and avoid making splatmaps
mandatory for the close route corridor.

## Erosion

Erosion is best treated as editor-time or offline shaping unless a ticket
explicitly budgets runtime generation.

Hydraulic erosion can carve valleys and drainage patterns by moving sediment
downhill. It is visually powerful but can be expensive and parameter-sensitive.

Thermal erosion moves material down steep slopes and can soften cliffs or
create talus. It is simpler and often useful for stylized rocky terrain.

For Echappee 3D, erosion should be a shape authoring aid. It should not make the
route uncomfortable, unreadable or hard to sample.

## LOD, Chunks And Culling

Use explicit distance bands:

- close corridor: highest silhouette quality, collision where needed;
- midground: lower density mesh or simplified props;
- distant mass: broad shapes, cheap materials, no gameplay collision.

Guidelines:

- chunk by route distance or scenic segment, not arbitrary world grids, when
  the ride is on rails;
- avoid loading many heavy terrain chunks around the player if only the route
  ahead matters;
- fade or swap LOD at low-attention moments where possible;
- pool repeated scatter props;
- keep colliders simpler than render meshes.

## Caves, Cliffs And Overhangs

Heightmaps cannot represent multiple heights at the same X/Z coordinate. Use:

- authored cliff meshes for steep transitions;
- arch or tunnel meshes for premium moments;
- small local density/marching-cubes features only when budgeted;
- fake cave entrances with placed geometry if the player never enters.

For a ride-on-rail MVP, visual illusion is often enough.

## Platform Notes

### Unity Native-First

Prefer terrain data that can ship on native Unity targets:

- serialized meshes or editor-generated assets;
- simple runtime assembly;
- predictable memory;
- bounded material and texture counts;
- clear LOD and culling.

### Android Candidate

Assume mobile GPU and thermal limits:

- avoid many terrain layers;
- minimize overdraw from vegetation;
- batch or instance scatter props only when validated;
- keep texture memory small;
- validate with device-oriented budgets once Android is selected.

### macOS Candidate

macOS can be a good first native target, especially for local FTMS exploration,
but do not let extra GPU headroom hide mobile costs. Keep terrain architecture
portable to Android unless `MYB-94` decides otherwise.

### WebGL Secondary

WebGL evidence exists in the project history, but new terrain guidance should
not be WebGL-first. If a future issue asks for WebGL validation, treat it as an
extra compatibility pass with stricter memory, shader and loading constraints.

## Useful References

- Unity Manual: Terrain heightmaps -
  https://docs.unity3d.com/6000.4/Documentation/Manual/terrain-Heightmaps.html
- Bluetooth SIG FTMS, relevant to platform/input planning rather than terrain -
  https://www.bluetooth.com/specifications/specs/fitness-machine-service-1-0/
- Dandrino: terrain erosion techniques -
  https://github.com/dandrino/terrain-erosion-3-ways
- HERE TIN Terrain, heightmap to optimized mesh reference -
  https://github.com/heremaps/tin-terrain
- MiniMax shader-dev terrain rendering reference -
  https://github.com/MiniMax-AI/skills/blob/main/skills/shader-dev/reference/terrain-rendering.md
- MIT realtime procedural terrain paper -
  https://web.mit.edu/cesium/Public/terrain.pdf

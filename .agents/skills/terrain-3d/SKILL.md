---
name: terrain-3d
description: "Unity terrain guidance for Echappee 3D in unity/Echapee4D. Use when creating or reviewing terrain, route corridors, heightmaps, height sampling, terrain mesh resolution, terrain colliders, biomes, road surfaces, vertex colors, splatmaps, erosion, LOD, chunks, cliffs, caves, overhangs, terrain noise, procedural landscape generation, terrain performance, or visual terrain quality. Also use for requests like make the terrain look better, add a cave, bigger landscape, or more interesting ground."
---

# 3D Terrain Generation Expert

You are a terrain specialist for Echappee 3D, the Unity project at
`unity/Echapee4D`.

Use this skill for terrain planning, implementation review, and future terrain
tasks. Do not treat the historical React/Vite/Three.js prototype under `src/**`
as the active terrain target unless a Linear issue explicitly says to work on
that parked prototype.

Read the detailed reference when you need technique details:

```text
references/terrain-techniques.md
```

## Active Project Context

- Active project: `unity/Echapee4D`.
- Current platform posture: Unity native-first. macOS and Android are active
  platform candidates pending `MYB-94`; WebGL is a secondary proof/debug path,
  not the terrain architecture default.
- Visual direction: `MYB-37`, `stylise premium avec socle low-poly de
  production`.
- V1 priority biomes: `Foret claire` and `Village / campagne pavee`.
- Road surfaces may vary by biome and may suggest future physical feel, but do
  not promise a physics mechanic unless the current issue implements one.
- Mock mode must remain playable unless the current issue explicitly changes
  ride input scope.

## Prescriptive Defaults

1. Build terrain around the ride route first.
   The route corridor is the gameplay spine: road ribbon, shoulders, verges,
   close terrain, camera comfort, and surface cues take priority over generic
   open-world terrain generation.
2. Prefer custom meshes for the playable corridor.
   Use route-driven custom meshes for the road, near-ground silhouettes,
   berms, cuts, talus, bridges, walls, and authored premium moments.
3. Use Unity Terrain only as support.
   Unity Terrain is acceptable for prototyping, broad distant masses, or
   background landscape, but it must not become the authoritative source for
   route progression, ride comfort, or surface feel without a specific decision.
4. Keep the DA production-grade.
   Low-poly means optimized and stylized, not placeholder. Terrain should have
   clean silhouettes, controlled density, intentional materials, and readable
   composition.
5. Keep platform budgets native and mobile-aware.
   Assume mobile-class GPU and memory constraints until `MYB-94` settles the
   priority order. Favor precomputed/editor-time terrain data, simple materials,
   bounded chunks, and explicit LOD.

## Unity Terrain vs Custom Mesh

### Custom Mesh Corridor

Use this for:

- road ribbon and road edges;
- shoulders, verges, ditches, retaining walls, talus and embankments;
- close terrain visible from the first-person camera;
- surface variation such as paved road, dirt, gravel, asphalt or relic slabs;
- deterministic height and normal sampling near the route;
- premium scenic beats that need authored silhouettes.

Strengths:

- strong control over MYB-37 stylized premium art direction;
- predictable performance and vertex budgets;
- route readability and camera comfort stay under project control;
- easy surface tags for future ride feel;
- supports non-heightmap shapes such as cliffs, bridges and arches.

Costs:

- requires our own generation, authoring or import pipeline;
- requires explicit collision, LOD, sampling and chunk rules;
- needs careful seams between route segments and distant landscape.

### Unity Terrain

Use this for:

- quick landscape prototyping;
- large background masses;
- distant valleys, hills and horizon shapes;
- optional authoring tools when a ticket budgets them.

Strengths:

- built-in heightmap tooling, terrain collider and terrain layers;
- fast editor iteration for broad landscape forms;
- useful for rough scenic composition.

Costs:

- heightmap-based, so no true overhangs, caves or vertical complexity;
- less direct control over low-poly premium silhouettes;
- can encourage terrain-first decisions that fight the route corridor;
- materials, details and vegetation can become expensive on mobile/native
  targets if left unbounded.

### Hybrid Rule

If both are used, the custom route corridor wins. Blend Unity Terrain or distant
meshes into the corridor visually, but keep gameplay sampling, surface tags and
camera-critical geometry tied to the route-first mesh layer.

## Core Terrain Contracts

Future terrain code should make these concepts explicit when the issue needs
them:

- `Route corridor`: the playable scenic band around the rail, including road,
  shoulder, verge and close terrain.
- `Height sample`: a deterministic query returning height, normal and surface
  information at or near the route.
- `Surface type`: a road or ground material category that can later feed audio,
  vibration, effort feedback or handling.
- `Biome segment`: a route interval with visual rules, surface tendencies,
  scatter rules and premium signal budget.
- `Distant mass`: non-gameplay landscape used for framing, horizon and scale.

Keep runtime ride logic independent from terrain rendering where practical.
Terrain should consume route/biome data; it should not own ride progression,
mock input, session state or HUD behavior.

## Biomes And Surfaces

For V1, terrain guidance should support:

- `Foret claire`: open and dense alternation, readable route framing, foliage
  and ground cover that feel premium rather than noisy.
- `Village / campagne pavee`: paved or mixed surfaces, walls, paths, fields,
  village edges and road texture cues that imply physical texture.

Surface examples:

- smooth asphalt: speed and comfort;
- paved road: vibration, village age, texture;
- dirt: softer nature passage;
- gravel: mild instability and adventure;
- relic slab: ceremonial or magical passage.

These are visual and semantic cues unless a current issue implements physical
feedback.

## Platform Guidance

Until `MYB-94` is closed:

- write terrain recommendations as Unity native-first;
- keep macOS and Android viable;
- treat WebGL as secondary validation or historical proof, not the default;
- prefer editor-time/pre-build processing over heavy runtime terrain generation;
- avoid mandatory compute-shader terrain pipelines for core functionality;
- keep material count, texture memory, draw calls and overdraw controlled.

For Android/mobile viability:

- use simple shaders and small texture sets;
- prefer atlases, vertex colors, baked masks or compact surface maps;
- chunk close terrain explicitly;
- use pooled/instanced scatter only when budgeted;
- define LOD and culling rules before adding density.

For macOS FTMS viability:

- terrain can assume more local GPU headroom than Android, but must not hide
  future mobile costs;
- do not couple terrain architecture to Bluetooth or FTMS input code.

## What To Avoid

- Presenting `src/world/terrain.ts` or any `src/**` Three.js file as active
  project terrain.
- Creating a terrain system that requires connected-bike hardware.
- Adding heavy asset, shader or runtime generation dependencies without a
  Linear issue.
- Treating low-poly as unfinished placeholder art.
- Letting procedural terrain obscure the route, HUD, effort feedback or camera
  comfort for long stretches.
- Encoding WebGL-first constraints as the primary design target.

## Quick Review Checklist

- Does the plan target `unity/Echapee4D`?
- Is the route corridor the first-class terrain object?
- Is custom mesh the default for close playable terrain?
- Is Unity Terrain optional/supporting rather than authoritative?
- Are `MYB-37` DA terms respected?
- Are `Foret claire` and `Village / campagne pavee` supported first?
- Are surface cues separated from unimplemented physics?
- Are native/mobile budgets explicit?
- Is WebGL treated as secondary until `MYB-94` decides otherwise?

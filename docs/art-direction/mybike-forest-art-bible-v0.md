# MyBike Forest Art Bible v0

Status: Canonical product art bible for the Art Rescue forest corridor.

Canonical path:
`docs/art-direction/mybike-forest-art-bible-v0.md`

Scope:
This document defines the visual contract for the forest ride corridor only. It
inherits global MyBike visual governance from `AGENTS.md`, `CONTEXT.md`, and
applicable ADR decisions. It does not define every future MyBike biome.

Source material:
`_bmad-output/implementation-artifacts/art-rescue-forest-corridor/`

Imported Art Rescue docs are source material and implementation artifacts, not
the final canonical art bible. If they conflict with this document, this
canonical art bible wins unless a later ADR explicitly resolves the conflict.

## Global Rules Inherited

- `Stylisé Premium de Production` is the canonical production visual direction.
- `Low-poly` is legacy when used as a final aesthetic target or quality bar.
- Route-camera-first validation is mandatory.
- `Premium target` is the closure threshold for visible Art Rescue production.
- `Surface Canonique de Validation Visuelle` is the route screenshot in the
  canonical ride corridor from the canonical ride camera.
- `Checkpoint insuffisant` preserves useful progress without allowing `Done`.
- `Ambition Visuelle Mesurée` means aim premium first, then measure performance.
- Grounded assets are part of the visual target. Visible trees, roots, rocks,
  patches, and props must belong to the forest floor, not float above it.
- Unity placement must follow
  `docs/validation/unity-ground-placement-policy.md`.
- Cheap, prototype-looking, preview-only, or generic forest output is rejected.

`Low-poly` may remain only as a local technical qualifier for a proxy, LOD,
collider mesh, mesh budget, or modeling method. It must never define the final
look of the forest corridor.

## Five Non-Negotiable Visual Principles

These principles are mandatory production rules, not soft advice. They sit above
asset-family details, budgets, palettes, and tool prompts.

### 1. Route Camera Decides

The canonical route camera is the blocking validation surface.

An asset, kit, or scene is production-valid only if it improves the
player-facing ride view in the canonical forest corridor. A Blender render,
Meshy/Tripo preview, turntable, overview, hero shot, sandbox scene, or isolated
asset screenshot is intermediate evidence only.

Overview explains. Route decides.

Passes:

- route screenshot is readable, premium, and coherent;
- road stays clear at ride speed;
- assets frame the trajectory instead of competing with it;
- overview confirms global coherence without replacing route proof.

Fails:

- preview asset looks good but harms or disappears in route camera;
- forest looks rich in overview but poor from the route;
- route is drowned by scenery;
- final validation has no canonical route screenshot.

### 2. Fantasy Scenic Premium Lisible

The forest corridor must carry a real fantasy mood.

The target is not a generic realistic forest and not overloaded theme-park
fantasy. The target is `Fantasy Scenic Premium Lisible`: enchanted, premium,
atmospheric, naturalistic-stylized, memorable, and readable from the route
camera.

Fantasy must frame the ride, not fight the ride.

Passes:

- ancient expressive trees;
- root arches;
- mossy stones;
- carved wooden signs;
- subtle engraved rocks;
- warm/cool fog pockets;
- scenic light shafts;
- natural thresholds and memorable forest landmarks.

Fails:

- generic realistic forest as final target;
- fantasy reduced to a few decorative props;
- runes everywhere;
- glowing crystals as default language;
- active magic everywhere;
- overloaded storybook clutter;
- fantasy props that steal focus from the road.

### 3. Silhouettes Before Detail

Masses, silhouettes, volumes, and composition matter before micro-detail.

The forest must read clearly at ride speed. A strong readable silhouette is more
valuable than many small details that do not improve the route camera.

Passes:

- simple but expressive tree;
- readable trunk base;
- clear canopy mass;
- rock with strong volume;
- root arch readable from distance;
- sign that reads as a composed signal, not noise;
- simplified but composed background.

Fails:

- small details on a weak shape;
- dense forest without strong silhouettes;
- trunks that read as posts;
- unshaped canopy blobs;
- random scatter;
- many props with weak composition;
- asset accepted because it is optimized while looking cheap.

### 4. Mood Is Built by Light, Fog, Material and Composition

The fantasy mood must be built first through light, fog, material coherence,
depth, values, rhythm, and composition. Fantasy props may reinforce the mood,
but they must not carry it alone.

If removing special props makes the corridor generic, the mood is not strong
enough.

Passes:

- fog gives depth without hiding the road;
- soft light feels slightly enchanted;
- warm/cool contrast stays subtle;
- materials are stylized and credible;
- road, shoulders, forest floor, and background read as one place.

Fails:

- flat lighting;
- fog hides the road;
- cheap or incoherent materials;
- fantasy props compensate for a scene with no mood;
- decoration works only in overview;
- scene is technically correct but visually empty.

### 5. Measured Ambition, No Cheap Fallback

The target is `Stylisé Premium de Production`.

Aim premium first, then measure. Performance is a measured guardrail, not an
excuse for poor visuals.

As long as the scene reaches the defined FPS target, defaulting to 60 FPS for
Art Rescue, and no blocking validator `ERROR` exists, visual enrichment is
allowed when the gain is visible from the canonical route camera.

If performance fails, reduce first:

1. unnecessary collisions;
2. excessive textures/materials;
3. density outside the route camera;
4. LOD/culling/distance;
5. non-essential near-route density;
6. premium route-camera-visible elements last.

Passes:

- spend more geometry when route-camera silhouette improves;
- enrich near-route elements while the FPS target holds;
- remove unnecessary collisions first;
- optimize off-camera cost before visible premium elements;
- document compromises with captures and measurements.

Fails:

- scene is smooth but empty;
- poor rendering is justified by performance;
- visible downgrade happens without measurement;
- prototype-looking visuals return under vertical-slice pressure.

## Direction Visuelle

The forest corridor is a `Stylisé Premium de Production` application of the
broader `Stylise Premium` direction. It should feel like a premium indoor scenic
ride through an enchanted forest corridor: readable, controlled, expressive, and
materially credible.

The visual target is:

- simple but not poor;
- optimized but not visually cheap;
- stylized but credible;
- fantasy-forward but route-camera-first;
- atmospheric without hiding the road;
- dense enough to feel authored, controlled enough to remain playable.

Reject:

- placeholder primitives presented as art;
- raw generated assets imported without style coherence;
- generic forest realism with no MyBike identity;
- preview beauty that fails in the ride camera;
- visual economy used as a final quality bar.

## Fantasy Scenic Premium Lisible

Fantasy must be visible, memorable, and directional. It should come first from
light, fog, silhouettes, materials, and composition, then be reinforced by a few
chosen landmarks.

Recommended density:

- standard segment: global fantasy mood plus 1 to 3 premium signals;
- rich segment or transition: 3 to 5 signals maximum, possibly one hero beat;
- calm segment: fewer props, with fantasy carried by fog, light, and silhouettes.

Accepted signals:

- ancient expressive trees;
- root arches near or above the road;
- mossy stones arranged like traces of an old path;
- carved wooden signs and subtle waypoints;
- discreet engraved rocks;
- scenic warm/cool fog pockets;
- light breaks, clearings, and natural thresholds.

Rejected signals:

- active magic as default language;
- noisy particles;
- glowing crystals as systematic decoration;
- runes everywhere;
- cartoon clutter;
- massive fantasy architecture;
- props that make the road secondary.

If the fantasy makes the scene memorable but less readable, rework it. If the
scene is clean but generic, it remains a checkpoint, not a production win.

## Surface Canonique de Validation Visuelle

Final visual validation for this corridor uses:

- primary proof: route screenshot from the canonical ride camera in the canonical
  ride corridor;
- secondary proof: overview screenshot from the same scene state;
- optional dynamic proof: short playthrough video for speed, rhythm, popping,
  and readability.

Intermediate evidence only:

- isolated asset screenshots;
- preview scenes;
- quarantine scenes;
- Blender renders;
- Meshy/Tripo previews;
- turntables;
- hero shots;
- overview-only captures.

Asset preview proves the asset exists. Route camera proves it belongs.

## Ground Contact / Anti-Float

Ground contact is part of `Fantasy Scenic Premium Lisible`.

Visible forest assets must feel rooted in the terrain. Trees, trunk bases, root
clusters, rocks, moss mats, leaf patches, signs, and waypoints should visually
belong to the forest floor. Floating assets immediately break scale,
material credibility, and route-camera premium quality.

Canonical Unity placement policy:

- ground visible assets by visual bottom;
- instantiate, apply rotation and scale, compute combined renderer bounds, then
  use `bounds.min.y` to compute the pivot-to-bottom correction;
- do not use `bounds.extents.y`, `bounds.size.y / 2`, or fixed half-height
  offsets as the final vertical placement policy;
- apply a small documented sink, usually 0.02m to 0.05m, so assets visually
  settle into soil, moss, or leaf litter;
- when raycasting, use an explicit ground layer mask or documented ground
  source, ignore triggers, and avoid hits against generated assets, patches, or
  props;
- report bottomClearance metrics for route-visible assets.

Thresholds:

- target bottomClearance: -0.05m to +0.05m;
- warning floating: > +0.05m;
- blocking floating for route-visible assets: > +0.10m;
- warning sinking: < -0.10m;
- blocking sinking for route-visible assets: < -0.25m.

Route-visible floating assets above +0.10m block checkpoint review unless Julien
explicitly accepts a documented exception.

## Premium Target and Checkpoint Insuffisant

Visible Art Rescue production is closable only when `Premium target` is reached
or when Julien explicitly accepts a documented exception.

`Premium target`:

- global rubric average >= 4;
- `Silhouette quality >= 4`;
- `Lighting mood >= 4`;
- `Material coherence >= 4`;
- no route-visible asset violates the blocking ground contact thresholds;
- human validation recorded when the judgment is subjective.

`Acceptable vertical slice` may be an intermediate checkpoint. It is not enough
to close visible Art Rescue production.

If a ticket improves the corridor but does not reach `Premium target`, mark it
as `Checkpoint insuffisant`, keep the evidence, and choose one:

- create a targeted corrective sub-ticket;
- request a documented exception accepted by Julien;
- rollback or rework if the direction is wrong.

Useful progress is evidence. It is not `Done`.

## Palettes and Mood Layers

### Forest Morning Mist

Canonical base palette.

- Ground: cool brown, damp soil, desaturated leaf litter.
- Vegetation: soft greens, lightly luminous moss.
- Wood: medium warm brown separated from ground values.
- Fog: very desaturated blue/green.
- Accent: restrained fresh moss and fern greens.

### Deep Edge

Depth, mystery, and silhouette layer.

- Background forest: dark blue-green.
- Foreground: stronger contrast without black holes.
- Midground: readable silhouettes, never absolute black.
- Use: edges, depth pockets, forest walls, controlled mystery.

### Warm Clearing

Accent, breathing, and hero-beat palette.

- Ground: warmer brown with light orange/gold leaf hints.
- Light: soft yellow or amber, preferably lateral.
- Vegetation: olive green.
- Fog: lighter and warmer than the base palette.
- Use: clearings, transitions, memorable route beats.

## Asset Families in Scope

This forest art bible covers exactly seven visual families.

### 1. Route and Shoulders

Role:
Main player-reading surface. It must stay clear, premium, and readable at 20 to
40 meters.

Rules:
keep road/shoulder contrast; use organic but clean edges; make transitions to
forest floor credible; prevent fog, shadow, or decoration from drowning the
trajectory.

Passes:
readable stylized road, believable wear, shoulders that frame depth.

Fails:
gray placeholder road, road lost in ground detail, noisy shoulders, fantasy
decoration more readable than the trajectory.

### 2. Forest Floor

Role:
Ground the corridor and connect the road to vegetation masses without becoming a
noisy carpet.

Rules:
group moss, leaves, soil, stones, and branches in readable masses; enrich
near-route floor only when readable at ride speed; simplify far and off-camera
surfaces.

Passes:
living but simple floor, moss/leaf masses, soft color variation, breathing zones.

Fails:
dirty texture noise, flat floor with no material identity, details invisible at
ride speed, realism that clashes with the stylized premium target.

### 3. Trees / Trunks / Canopy

Role:
Main vertical rhythm, silhouette, and forest identity.

Rules:
use expressive trunks instead of generic posts; vary height, lean, base, and
canopy shape; make near and mid silhouettes readable; frame the road without
suffocating it.

Passes:
ancient expressive tree, readable trunk/root base, shaped canopy masses, grouped
silhouettes that guide the road.

Fails:
identical repeated trunks, trunk-as-cylinder final art, unshaped canopy blobs,
forest dense in overview but poor from route camera.

### 4. Roots / Arches / Root Clusters

Role:
Strongest natural fantasy signals for the corridor.

Rules:
use arches and clusters sparingly; integrate roots into ground and trees; keep
route visibility open; treat root landmarks as composed beats.

Passes:
root arch that frames the road, ancient roots grounded in terrain, cluster that
adds shoulder relief and fantasy mood.

Fails:
arch that hides road reading, roots everywhere with no composition, cartoon
forms, isolated root prop that does not belong to terrain.

### 5. Rocks / Mossy Stones / Markers

Role:
Structure borders, add depth, and carry subtle fantasy history.

Rules:
favor simple credible masses; use moss as integration, not noise; group stones
intentionally; use engraved rocks rarely and discreetly.

Passes:
mossy stones grouped near a transition, marker rock that helps corridor rhythm,
subtle rare engraving, volume that frames the road.

Fails:
tiny stones everywhere, hero rock outside the style, systematic luminous runes,
material too realistic, too flat, or incoherent.

### 6. Signs / Fences / Sculpted Waypoints

Role:
Light human intention, rhythm, and premium adventure language.

Rules:
use carved wood, rustic barriers, and small waypoints sparingly; place them to
reinforce composition; avoid modern signage or cartoon signal language.

Passes:
sober carved wooden sign, fence that guides road reading, small sculpted
waypoint integrated into the shoulder.

Fails:
signs everywhere, theme-park decoration, modern prop language, object that pulls
the eye away from the trajectory.

### 7. Lighting / Fog / Atmospheric Background

Role:
Core art direction, not a technical afterthought. Fantasy mood must come first
from light, fog, depth, and background composition.

Rules:
use light-to-medium fog for depth, not concealment; keep the road readable;
create warm/cool contrast with restraint; simplify far forest into atmospheric
layers.

Passes:
fog adds depth without hiding the road, soft almost-enchanted light, composed
background layers, mood visible even without hero props.

Fails:
flat lighting, fog that washes or hides everything, empty background, aggressive
contrast, noisy magical particles, generic realistic mood with no fantasy
identity.

Priority:

1. Route readability.
2. Lighting/fog mood.
3. Trees/canopy silhouettes.
4. Shoulders/forest floor.
5. Roots/arches.
6. Rocks/markers.
7. Signs/fences/waypoints.

Route wins in readability. Fantasy mood wins in identity. Performance wins only
after measurement.

## Out of Detailed Scope

- UI and HUD;
- player bike and avatar;
- villages, mountains, coast, and future biomes;
- advanced weather and seasons;
- characters and creatures;
- massive fantasy architecture;
- active magic and portals;
- combat, narrative systems, menus, garage, or shop.

The corridor may suggest a broader fantasy world through mood, silhouettes,
materials, and subtle landmarks. It must not become an encyclopedia for every
future MyBike environment.

## Budgets and Measured Ambition

Budgets are guardrails, not aesthetic ceilings.

| Type | Triangle target | Materials | Texture guidance |
| --- | ---: | ---: | --- |
| Leaf/fern patch | 50-250 | 1 | none or 512 |
| Common rock | 100-500 | 1 | 512-1024 |
| Common trunk | 200-800 | 1-2 | 512-1024 |
| Root cluster | 200-1000 | 1-2 | 512-1024 |
| Canopy mass | 100-800 | 1 | none or 512 |
| Hero landmark | 1000-5000 | 1-3 | 1024-2048 |

Spend visual budget on route-camera gains: stronger near-route silhouettes,
richer visible material grounding, clearer depth layers, stronger fantasy mood,
and composed landmarks that support road reading.

Do not spend visual budget on micro-details invisible at ride speed, off-camera
density, 4K textures on small props, expensive overview-only decoration, or
complexity that makes the style less coherent.

## Keep / Rework / Reject

### Keep

- improves route-camera composition;
- silhouette reads at ride speed;
- fantasy mood supports the corridor without overload;
- scale, pivot, bounds, materials, and provenance are clean;
- budget is reasonable or explicitly justified;
- overview confirms global coherence.

### Rework

- good idea but weak silhouette;
- useful mood but poor material coherence;
- readable in preview but not in route camera;
- correct family but wrong placement or density;
- performance warning with a clear optimization path;
- `Checkpoint insuffisant` with a targeted next action.

### Reject

- source/provenance/license unclear;
- looks cheap, generic, or prototype-like in route camera;
- route-visible asset floats above the ground or sinks enough to break scale
  credibility;
- validates only in preview, turntable, or overview;
- hides the road or competes with the ride;
- breaks `Fantasy Scenic Premium Lisible`;
- uses `low-poly` as final visual justification;
- adds cost without route-camera gain.

## What Must Be Visible From the Route Camera

- road readable at 20 to 40 meters;
- coherent road/shoulder/forest-floor transition;
- at least one strong silhouette or massing idea;
- visible lighting/fog mood;
- credible material separation between road, ground, vegetation, and wood;
- controlled density that frames without smothering;
- at least one readable fantasy signal on standard or rich segments, unless the
  segment is intentionally calm and mood-led.

## What Immediately Reads Cheap

- trunk posts or raw cylinders used as final art;
- ungrounded props placed beside the road;
- noisy material pasted on a poor shape;
- flat lighting and gray fog;
- random scatter without hierarchy;
- generic forest with no fantasy identity;
- fantasy prop overload that makes the route secondary;
- scene that is smooth but empty;
- raw generated asset used as if production-ready;
- final validation based on preview beauty instead of ride-camera proof.

## MYB-141 Closure Rule

MYB-141 is doc-only governance. It must not modify Unity, generate assets, import
assets, create Blender output, call Meshy/Tripo, or change a scene.

This ticket can move to `In Review` when:

- this canonical art bible exists and is hardened;
- imported copies are marked non-canonical;
- `AGENTS.md`, `CONTEXT.md`, and `CONTEXT-MAP.md` point to this canonical path;
- the Linear ticket contains the Q1-Q8 summary and review checklist;
- text verification confirms the required terms and forbids `low-poly` as final
  aesthetic target.

`Done` is allowed only after Julien validates this art bible as the current
visual governance source.

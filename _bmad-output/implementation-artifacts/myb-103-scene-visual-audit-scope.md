# MYB-103 - Scene Visual Audit Scope

Date: 2026-06-13

## Trigger

MYB-101 produced three comparison videos (`daylight`, `golden-hour`, and
`fantasy-dusk`), but the product verdict is that all three are visually poor.
The issue is not choosing the least bad lighting preset. The canonical Unity
scene needs a visual audit and art-direction reset before another visual pass.

## Current Scene Audit

Scene: `Assets/Scenes/MYB89UnityMcpProbe.unity`

Unity script audit result:

- Renderers: 337.
- Visible material names: 34.
- Shaders: `Universal Render Pipeline/Lit`,
  `Universal Render Pipeline/Unlit`.
- Lights: 11.
- Lights with shadows: 1.
- Volumes: 1.
- Cameras: 1.
- Cameras with post-processing enabled: 1.
- Reflection probes: 0.
- Light probe groups: 0.
- Approximate scene bounds: `(216.0, 12.0, 335.0)`.

## Diagnosis

The scene currently reads as primitives and imported objects placed along a
test route, then color-graded. The visual failure is structural:

- Weak composition: no deliberate foreground, midground, background, or horizon
  staging.
- Weak environmental identity: the scene does not yet say alpine ride, village
  climb, col road, forest road, or any other strong place.
- Weak material hierarchy: too many surfaces read as flat local color, and the
  road dominates without enough surrounding shape language.
- Weak lighting foundation: many lights exist, but only one casts shadows; there
  are no reflection probes and no light probes.
- Weak contact and grounding: props often feel pasted onto the scene rather
  than embedded in it.
- Post-processing is being used as compensation instead of finishing polish.

## Unity References Consulted

- URP post-processing is Volume-based and can improve the final frame, but it is
  not a substitute for authored scene structure:
  `https://docs.unity3d.com/6000.4/Documentation/Manual/urp/integration-with-post-processing.html`.
- URP SSAO is a Renderer Feature intended to darken creases, intersections, and
  close surfaces:
  `https://docs.unity3d.com/6000.4/Documentation/Manual/urp/post-processing-ssao-landing.html`.
- Unity Light Probes store baked light information through the scene space and
  improve indirect lighting for moving objects and LOD scenery:
  `https://docs.unity3d.com/6000.4/Documentation/Manual/LightProbes.html`.
- Reflection Probes sample the visual environment so reflective materials react
  to local surroundings instead of a single static sky/environment:
  `https://docs.unity3d.com/6000.4/Documentation/Manual/ReflectionProbes.html`.
- URP shadows documentation explicitly separates shadow resolution,
  screen-space shadows, SSAO, and shadow cascade troubleshooting:
  `https://docs.unity3d.com/6000.4/Documentation/Manual/urp/Shadows-in-URP.html`.
- Unity 6 Adaptive Probe Volumes can provide per-pixel probe sampling and better
  lighting consistency, but this belongs in the separate URP/global-lighting
  spike unless explicitly scoped:
  `https://docs.unity3d.com/6000.4/Documentation/Manual/urp/probevolumes-concept.html`.

## Ticket Created

Linear: `MYB-103`

URL:
`https://linear.app/kefjbo/issue/MYB-103/auditer-et-recadrer-la-scene-unity-canonique-avant-nouvelle-passe`

## MYB-103 Intent

Produce a local audit and a concrete visual target before another
implementation pass. The next implementation ticket must start from a chosen
scene direction and rejection criteria, not from arbitrary lighting variants.

## Grill Decisions

- MYB-103 is an audit and cadrage ticket only. It must not modify the canonical
  Unity scene except for inspection, captures, or audit evidence.
- The target sequence is three successive one-minute Passages in the canonical
  ride:
  - minute 0 to 1: foret claire;
  - minute 1 to 2: village / route de col;
  - minute 2 to 3: panorama / signal fantasy.
- These Passages are real sections of the canonical ride, not a disconnected
  moodboard.
- The next implementation ticket may reconstruct visible scene portions around
  the ride camera while preserving the ride loop and gameplay mechanics.
- The main rejection criterion is `Lecture Prototype`: the scene must not look
  like primitives, raw assets, opportunistic placement, flat lighting, or
  post-process compensation.
- Each Passage gets its own audit grid. Each item receives a verdict:
  `prototype`, `limite`, or `production low-poly`.
- Every verdict must include a short visible-evidence justification. An item
  cannot be marked `prototype` or `production low-poly` without explaining what
  is visible in the capture or video.
- Unity documentation is used as technical support for the analysis, but MYB-103
  translates it into visual criteria instead of a mandatory URP feature
  checklist.
- MYB-103 prepares the next visual implementation ticket content, but that issue
  is created only after the audit is reviewed and validated.

## Audit Grid

Each one-minute Passage must be evaluated with the following items:

| Item | Verdict | Required justification |
| --- | --- | --- |
| Composition camera | `prototype` / `limite` / `production low-poly` | Foreground, midground, background, horizon, road framing, and visible depth. |
| Place identity | `prototype` / `limite` / `production low-poly` | Whether the Passage clearly reads as forest, village/col road, or panorama/fantasy. |
| Grounding and placement | `prototype` / `limite` / `production low-poly` | Contact with ground, scale, density, repetition, and placement logic. |
| Lighting and shadows | `prototype` / `limite` / `production low-poly` | Directional light, readable shadow shapes, depth cues, and atmosphere. |
| Materials and color hierarchy | `prototype` / `limite` / `production low-poly` | Separation between road, terrain, props, and hero accents. |
| Premium signal | `prototype` / `limite` / `production low-poly` | Presence and quality of a memorable scenic or fantasy element in camera view. |
| Ride readability | `prototype` / `limite` / `production low-poly` | Whether the route remains understandable in motion without dominating the aesthetic. |
| Post-process dependency | `prototype` / `limite` / `production low-poly` | Whether fog, bloom, or color grading polish a strong scene or hide a weak one. |

## Audit Execution

Capture report:
`_bmad-output/unity-test-results/myb-103/myb-103-scene-visual-audit.txt`.

Captured PNG evidence:

- `_bmad-output/unity-test-results/myb-103/passage-01-foret-claire.png`.
- `_bmad-output/unity-test-results/myb-103/passage-02-village-route-de-col.png`.
- `_bmad-output/unity-test-results/myb-103/passage-03-panorama-signal-fantasy.png`.

Capture note: the current canonical route is about `245.2 m`, so the audit
uses current route thirds as visual proxies for the approved future target of
three one-minute Passages.

Observed metrics:

- Renderers: `337`.
- Visible material names: `30`.
- Lights: `7`.
- Shadow-casting lights: `1`.
- Volumes: `0`.
- Reflection probes: `0`.
- Light probe groups: `0`.
- Scene bounds: `(216.0, 12.0, 335.0)`.

## Passage 1 Audit - Foret claire

Capture:
`_bmad-output/unity-test-results/myb-103/passage-01-foret-claire.png`.

| Item | Verdict | Justification |
| --- | --- | --- |
| Composition camera | `limite` | The road creates a clear centerline and depth, but most of the frame is road, flat shoulder bands, and open sky; foreground/midground/background are present but not deliberately staged as a forest opening. |
| Place identity | `prototype` | The Passage does not read as `foret claire`; it reads as open village road with a few isolated round trees and buildings. |
| Grounding and placement | `prototype` | Trees, posts, fences, and buildings feel placed beside the route as individual objects; contact, scale, and density do not make a believable wooded corridor. |
| Lighting and shadows | `prototype` | The image is mostly flat daylight with weak shadow design, no atmospheric depth, and no clear lighting direction selling a premium forest mood. |
| Materials and color hierarchy | `prototype` | The cobblestone road texture dominates the frame, while yellow shoulders and green terrain read as large flat color bands. |
| Premium signal | `prototype` | The colored posts and orange gates feel like debug/checkpoint markers rather than a memorable premium scenic or fantasy signal. |
| Ride readability | `production low-poly` | The route, lane markings, and next curve are very readable from the camera. This succeeds as navigation, but it dominates the aesthetic. |
| Post-process dependency | `limite` | The capture does not appear to hide behind heavy post-process, but the base scene is too raw to benefit from polish alone. |

## Passage 2 Audit - Village / route de col

Capture:
`_bmad-output/unity-test-results/myb-103/passage-02-village-route-de-col.png`.

| Item | Verdict | Justification |
| --- | --- | --- |
| Composition camera | `limite` | The curve gives the frame a usable path, but the horizon, sky, and side masses remain sparse and blocky; the camera is not framed around a strong village climb moment. |
| Place identity | `limite` | Houses and roadside props suggest a village, but the scene does not read as route de col because terrain relief, slope drama, and alpine composition are weak. |
| Grounding and placement | `prototype` | Props and buildings sit on broad flat bands with little contact treatment, clustering logic, or transition between road, verge, and scenery. |
| Lighting and shadows | `prototype` | Shadows are not shaping the village or road; the lighting stays even and old-looking, with no premium time-of-day or depth cue. |
| Materials and color hierarchy | `prototype` | Road, shoulders, grass, buildings, and trees separate by flat local color rather than material quality or art-directed contrast. |
| Premium signal | `prototype` | Red gates and colored posts are visible but read as gameplay/debug markers, not as authored village landmarks or fantasy moments. |
| Ride readability | `production low-poly` | The road curve and lane markings are easy to follow, and the ride path is more successful than the scenic staging. |
| Post-process dependency | `limite` | The weakness is structural rather than a missing color grade: composition, materials, and lighting would still read raw after a preset. |

## Passage 3 Audit - Panorama / signal fantasy

Capture:
`_bmad-output/unity-test-results/myb-103/passage-03-panorama-signal-fantasy.png`.

| Item | Verdict | Justification |
| --- | --- | --- |
| Composition camera | `prototype` | The frame has oversized cut-off buildings at the edges, a flat open center, and a small distant arch; it does not create a deliberate panorama reveal. |
| Place identity | `prototype` | The Passage does not read as panorama or fantasy signal; it reads as the same road corridor with sparse props and oversized blockout buildings. |
| Grounding and placement | `prototype` | Houses, trees, rocks, and posts feel dropped around the road; the side objects do not compose a believable viewpoint or destination. |
| Lighting and shadows | `prototype` | The scene remains flat and sky-heavy, with little shadow shape or atmospheric depth to sell distance or spectacle. |
| Materials and color hierarchy | `prototype` | The road texture still carries most of the visual detail, while terrain and buildings remain simple flat masses. |
| Premium signal | `prototype` | The distant arch and blue posts are too small and utilitarian to become a memorable fantasy/premium moment. |
| Ride readability | `production low-poly` | The road and bend are readable, and the ride direction is clear even though the scene around it lacks ambition. |
| Post-process dependency | `limite` | A stronger grade could improve mood, but the capture lacks the authored landmark, depth, and composition needed before post-process. |

## Audit Verdict

MYB-103 confirms a structural `Lecture Prototype` on the current canonical
scene. The ride path is readable, but the visual direction fails as
`Stylise Premium` and `Low-poly de Production` because the camera sees a road
prototype with scattered props, flat lighting, weak material hierarchy, and no
strong Passage identity.

The next implementation ticket should not start with more lighting presets. It
should rebuild the visible corridor around the camera for the three target
Passages, using lighting and post-processing only after the scene reads as an
intentional place.

## Proposed Follow-Up Ticket

Title:
`Recomposer la scene canonique en trois Passages low-poly de production`.

Linear:
`MYB-104` -
`https://linear.app/kefjbo/issue/MYB-104/recomposer-la-scene-canonique-en-trois-passages-low-poly-de-production`.

Intent:
turn the MYB-103 audit into a visible scene pass that replaces the current
prototype read with three authored one-minute Passages: foret claire,
village / route de col, and panorama / signal fantasy.

Scope:

- Preserve the canonical ride loop, mock mode, HUD behavior, and route
  navigation.
- Recompose foreground, midground, background, and horizon for each Passage.
- Replace debug-like gates/posts with authored scenic or fantasy signals.
- Improve grounding and density around the ride camera before adding polish.
- Establish lighting direction, readable shadows, material hierarchy, and depth
  cues per Passage.
- Keep MYB-102 separate for global URP defaults and probe strategy.
- Do not import new large asset packs or spend Meshy credits unless explicitly
  approved by the follow-up issue.

Acceptance criteria:

- Three 720p captures or one muted 720p comparison video show the three target
  Passages from the ride camera.
- Each Passage has no `prototype` verdict for composition, place identity,
  grounding, lighting, materials, premium signal, or ride readability.
- Ride readability remains at least `limite`, preferably
  `production low-poly`.
- Any remaining `limite` verdict has a concrete follow-up reason.
- Validation includes Unity-MCP status, the MYB-103 capture/audit tool or its
  successor, `git diff --check`, and no `src/**` or `unity/Echappee3D/**`
  changes.

## Scope Guard

- Do not merge the current MYB-101 visuals as final.
- Do not make global URP defaults changes in MYB-103.
- Keep MYB-102 as the separate global URP spike.
- Do not import new large asset packs.
- Do not spend Meshy credits.
- Do not touch `src/**`.
- Do not recreate `unity/Echappee3D/**`.

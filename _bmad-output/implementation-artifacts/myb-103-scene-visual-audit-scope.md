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

## Scope Guard

- Do not merge the current MYB-101 visuals as final.
- Do not make global URP defaults changes in MYB-103.
- Keep MYB-102 as the separate global URP spike.
- Do not import new large asset packs.
- Do not spend Meshy credits.
- Do not touch `src/**`.
- Do not recreate `unity/Echappee3D/**`.

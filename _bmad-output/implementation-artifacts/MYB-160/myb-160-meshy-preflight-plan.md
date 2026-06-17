# MYB-160 Meshy Hero Forest Candidates Preflight

## Ticket

MYB-160 - Meshy hero forest candidates pour Golden Slice

## Current State

- Canonical source ticket: MYB-160.
- Duplicate cleanup: MYB-161 was created by a retry after a temporary Linear 502
  and is marked Duplicate of MYB-160.
- Working branch: `MYB-160-meshy-hero-forest-candidates`.
- Baseline: MYB-159 golden slice.
- Premium target reached: No.
- Expected final status: In Review, not Done.

## Objective

Create 2 to 3 controlled Meshy hero candidates, clean retained candidates in
Blender, import them as Art Rescue candidates, and test them in the MYB-159
golden slice route-camera context.

## Non-Goals

- No full Meshy forest.
- No global scatter.
- No production promotion.
- No canonical ride scene modification.
- No gameplay, HUD, telemetry, route trajectory, or route collider changes.
- No Poly Haven.
- No silent third-party imports.

## Credit Gate

No Meshy credit-costing tool can be called without explicit Julien confirmation.

Available Meshy costs:

| Tool | Cost |
| --- | ---: |
| `meshy_text_to_3d`, `ai_model:"meshy-5"` | 5 credits each |
| `meshy_text_to_3d`, `ai_model:"meshy-6"` or `latest` | 20 credits each |
| `meshy_text_to_3d_refine` | 10 credits each |
| `meshy_remesh` | 5 credits each |
| `meshy_retexture` | 10 credits each |

Recommended first pass:

- 3 preview generations with `meshy-5`.
- Output format: `fbx`.
- Total initial cost: 15 credits.
- No refine/remesh/retexture until route-camera candidate quality is inspected.

Higher-quality alternative:

- 2 preview generations with `meshy-6`.
- Output format: `fbx`.
- Total initial cost: 40 credits.
- Use only if Julien chooses quality over cost for the first pass.

## Generation Targets

### Candidate A - Ancient Tree Assembly

Purpose:

- Replace the weakest procedural tree assembly read in MYB-159 with a stronger
  hero silhouette candidate.

Prompt:

> Stylized premium fantasy forest ancient tree assembly for a first-person cycling game. Strong readable silhouette, expressive irregular trunk, wide grounded root base, supported leafy canopy, naturalistic enchanted forest mood, mossy base, readable from route camera, optimized game asset, clean geometry, no tiny details, no photorealism, no horror, no glowing crystals, no runes, no cartoon style, no full environment, single reusable tree asset.

Proposed Meshy settings:

- tool: `meshy_text_to_3d`
- target_formats: `["fbx"]`
- origin_at: `bottom`
- auto_size: `true`
- topology: `triangle`
- target_polycount: `30000`
- symmetry_mode: `off`
- should_remesh: `false` for `meshy-6`, `true` only if using `meshy-5` default

### Candidate B - Root Arch / Forest Threshold

Purpose:

- Create a memorable hero beat near the route without blocking readability.

Prompt:

> Stylized premium fantasy forest root arch landmark, natural root formation forming a readable arch near a forest road, grounded mossy roots, ancient enchanted forest mood, strong silhouette, naturalistic stylized shapes, route-camera readable, optimized game asset, clean geometry, no active magic, no glowing runes, no crystals, no cartoon mushrooms, no full environment, single reusable landmark asset.

Proposed Meshy settings:

- tool: `meshy_text_to_3d`
- target_formats: `["fbx"]`
- origin_at: `bottom`
- auto_size: `true`
- topology: `triangle`
- target_polycount: `25000`
- symmetry_mode: `off`

### Candidate C - Stump / Roots / Rock Marker

Purpose:

- Optional foreground or side-bank landmark if A or B is weak, or if a third
  candidate is useful for route-side grounding.

Prompt:

> Stylized premium ancient forest stump with exposed roots and moss, strong grounded silhouette, naturalistic fantasy forest mood, readable from route camera, suitable as foreground landmark, optimized game asset, clean geometry, no tiny bark details, no horror face, no glowing magic, no full environment.

Fallback prompt if stump quality is poor:

> Stylized premium mossy forest rock marker for an enchanted cycling route, simple strong silhouette, ancient natural stone, subtle carved shape but no glowing runes, moss integrated at base, grounded forest floor contact, readable from first-person route camera, optimized game asset, clean geometry, naturalistic fantasy style, no photorealism, no full environment.

Proposed Meshy settings:

- tool: `meshy_text_to_3d`
- target_formats: `["fbx"]`
- origin_at: `bottom`
- auto_size: `true`
- topology: `triangle`
- target_polycount: `15000`
- symmetry_mode: `off`

## Candidate Intake Paths

Raw/candidate import root:

- `unity/Echapee4D/Assets/Echappee/Art/Candidates/MYB160/Meshy/`

Expected local generation record root:

- `_bmad-output/implementation-artifacts/MYB-160/meshy-source-records/`

Do not place raw Meshy assets in:

- `Assets/Echappee/Art/Production/`

## Manifest Rules

Every imported Meshy asset must be listed in:

- `docs/manifests/art-rescue-asset-manifest.json`

Initial raw candidate fields:

```json
{
  "sourceType": "ai_generated",
  "provider": "Meshy",
  "aiGenerated": true,
  "intakeStatus": "review",
  "promotionStatus": "not_promoted",
  "usageScope": "forest_corridor",
  "license": "Provider terms pending project review",
  "author": "AI-generated via Meshy"
}
```

If cleaned and used in the preview:

```json
{
  "intakeStatus": "approved",
  "promotionStatus": "candidate"
}
```

Forbidden:

- `reviewStatus`
- `example:true`
- `promotionStatus: promoted`
- `sourceType: unknown`
- unmanifested Meshy assets in active preview

## Blender Cleanup Gate

Every retained candidate must pass through Blender cleanup before Unity preview
placement:

- inspect geometry;
- verify meter scale;
- apply transforms;
- set or document bottom pivot;
- remove microscopic debris where possible;
- simplify if too heavy;
- correct orientation;
- export Unity-ready FBX;
- record dimensions, triangle count, renderer/material count.

## Unity Preview Plan

Preferred preview surface:

- create `Assets/Scenes/MYB160MeshyHeroCandidatePreview.unity` derived from
  MYB-159, or keep MYB-159 untouched and build a ticket-local MYB-160 preview
  scene.

Builder target:

- `unity/Echapee4D/Assets/MYB160/Editor/MYB160MeshyHeroCandidateBuilder.cs`

Generated root:

- `MYB160_MeshyHeroCandidateRoot`

Do not modify:

- canonical ride scene;
- MYB-158 scene;
- MYB-159 scene unless explicitly using it only as an opened source to save a
  MYB-160 preview copy.

## Selection Criteria

Reject candidate if:

- it reads as full environment instead of single asset;
- it contains horror faces, runes, crystals, mushrooms, or theme-park fantasy;
- it is too photorealistic;
- it has no grounded base;
- canopy/root structure reads as unsupported;
- it is too noisy from route camera;
- it cannot be cleaned within ticket scope;
- it needs production promotion to be useful.

Retain candidate if:

- silhouette is readable from RouteCamera;
- bottom contact is clear;
- asset scale is plausible in meters;
- it improves the MYB-159 route shot more than the scene-local placeholder;
- it can remain `candidate`, not `promoted`;
- MYB-144 reports no MYB-160-caused ERROR.

## Evidence To Produce After Confirmation

- `_bmad-output/implementation-artifacts/MYB-160/myb-160-meshy-hero-candidate-report.md`
- `_bmad-output/implementation-artifacts/MYB-160/myb-160-meshy-candidate-metrics.json`
- `_bmad-output/implementation-artifacts/MYB-160/myb-160-governance-review.md`
- `_bmad-output/visual-checkpoints/MYB-160/`
- `_bmad-output/unity-test-results/myb-144-art-asset-validator-report.md`

## Current Blocker

Waiting for Julien credit confirmation before any Meshy generation call.

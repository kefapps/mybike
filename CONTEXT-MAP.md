# Context Map: MyBike

## Purpose

This map points agents and contributors to the right glossary before changing
planning artifacts or runtime behavior.

## Contexts

### Visual Direction

File: `CONTEXT.md`

Owns the shared visual language for the active Unity vertical slice, including
the relationship between `Stylisé Premium de Production`, `Stylise Premium`,
`Scenic Fantasy Lisible`, and `Lecture Prototype`. `Socle Low-Poly de
Production` is legacy language, not the visual quality bar.

Related ADR: `docs/adr/0001-art-rescue-visual-governance.md`.

Forest corridor art bible:

- `docs/art-direction/mybike-forest-art-bible-v0.md`

This is the canonical product art bible for the Art Rescue forest corridor. It
inherits global visual governance from `CONTEXT.md`, `AGENTS.md`, and applicable
ADR decisions. It must not be treated as the final art bible for every future
biome.

Imported Art Rescue source material:

- `_bmad-output/implementation-artifacts/art-rescue-forest-corridor/`

If imported Art Rescue docs conflict with the canonical forest corridor art
bible, `docs/art-direction/mybike-forest-art-bible-v0.md` wins.

MYB-142 outputs:

- `docs/art-direction/myb-137-visual-diagnosis.md`
  - MYB-137 dated diagnostic and calibration baseline;
  - `Diagnostic Surface`, not final `Premium target` evidence.
- `docs/validation/forest-corridor-shot-rubric.md`
  - reusable 9-criterion rubric for future Art Rescue forest corridor visual
    reviews;
  - canonical scoring mechanics and review templates.

Relationship:

- The MYB-137 diagnosis applies the rubric to a historical checkpoint.
- The forest corridor shot rubric is reusable for future visual tickets.
- The forest art bible wins for art direction; the shot rubric wins for scoring
  mechanics.

MYB-143 outputs:

- `docs/schemas/third-party-asset-manifest.md`
  - canonical schema documentation for Art Rescue asset intake and promotion;
  - defines `Intake Status`, `Promotion Status`, source types, usage scopes, and
    MYB-144 validator expectations.
- `docs/manifests/art-rescue-asset-manifest.json`
  - canonical machine-readable Art Rescue asset manifest;
  - versioned object, initially allowed to have an empty `assets` array.
- `docs/workflows/meshy-tripo-quarantine-workflow.md`
  - quarantine-first workflow for Meshy, Tripo, external, Blender MCP,
    in-house, and procedural Art Rescue asset candidates.

Relationship:

- `docs/schemas/third-party-asset-manifest.md` defines the manifest shape.
- `docs/manifests/art-rescue-asset-manifest.json` is the data MYB-144 should
  validate.
- `docs/workflows/meshy-tripo-quarantine-workflow.md` defines how candidates
  move from quarantine to review, candidate, promotion, rejection, or
  deprecation.
- Existing Unity ticket-local manifests under `unity/Echapee4D/Assets/...`
  remain historical or ticket-specific evidence unless explicitly migrated.

### Unity Ride Runtime

File: `unity/Echapee4D/CONTEXT.md`

Owns the ride-loop language used by the Unity mock experience: effort,
resistance demand, applied resistance, and measured resistance.

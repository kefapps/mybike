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
- `docs/validators/unity-art-asset-validator-spec.md`
  - canonical Unity validator specification;
  - defines bounded scan roots, candidate extensions, severity policy, batch
    behavior, and report format.
- `docs/workflows/meshy-tripo-quarantine-workflow.md`
  - quarantine-first workflow for Meshy, Tripo, external, Blender MCP,
    in-house, and procedural Art Rescue asset candidates.

MYB-144 outputs:

- `unity/Echapee4D/Assets/MYB144/Editor/MYB144ArtAssetValidator.cs`
  - ticket-local Unity Editor validator V1.
- `_bmad-output/unity-test-results/myb-144-art-asset-validator-report.md`
  - generated validation report path.

Relationship:

- `docs/schemas/third-party-asset-manifest.md` defines the manifest shape.
- `docs/manifests/art-rescue-asset-manifest.json` is the data MYB-144 should
  validate.
- `docs/validators/unity-art-asset-validator-spec.md` defines how MYB-144
  validates the manifest and bounded Unity asset roots.
- `docs/workflows/meshy-tripo-quarantine-workflow.md` defines how candidates
  move from quarantine to review, candidate, promotion, rejection, or
  deprecation.
- Existing Unity ticket-local manifests under `unity/Echapee4D/Assets/...`
  remain historical or ticket-specific evidence unless explicitly migrated.

MYB-145 outputs:

- `docs/workflows/visual-checkpoint-workflow.md`
  - canonical workflow for route/overview visual checkpoint captures;
  - defines explicit `current`, `before`, and `after` states.
- `docs/templates/art-checkpoint-report-template.md`
  - reusable human-readable capture report template.
- `unity/Echapee4D/Assets/MYB145/Editor/MYB145CaptureRigHelper.cs`
  - ticket-local Unity Editor helper for setup, validation, and capture.
- `_bmad-output/visual-checkpoints/<ticket-id>/`
  - canonical output path for comparable visual evidence.

Relationship:

- MYB-142 defines how visual evidence is judged.
- MYB-145 defines where and how comparable visual evidence is produced.
- `_bmad-output/implementation-artifacts/` remains for general artifacts and
  history; `visual-checkpoints` wins for standardized route/overview captures,
  before/after sheets, reports, and metadata.

MYB-155 / requested MYB-154 output:

- `docs/validation/unity-ground-placement-policy.md`
  - canonical Art Rescue anti-floating and ground contact policy;
  - defines visual-bottom grounding by combined renderer bounds `min.y`;
  - forbids final offsets based on `bounds.extents.y`, `bounds.size.y / 2`, or
    fixed half-height placement;
  - defines `bottomClearance` thresholds and future builder metrics.
- `_bmad-output/implementation-artifacts/MYB-154/myb-154-ground-placement-governance-report.md`
  - doc-only implementation report for the requested MYB-154 governance label.

Relationship:

- The forest art bible owns the visual target: assets must belong to the forest
  floor.
- The shot rubric owns review gating: route-visible floating above +0.10m blocks
  checkpoint review unless Julien accepts a documented exception.
- The ground placement policy owns technical placement mechanics and metrics.

MYB-156 output:

- `docs/validation/unity-visual-support-policy.md`
  - canonical Art Rescue visual-support policy for elevated route-visible
    assets;
  - defines `supportedAboveGround`, support evidence, blocker policy, and
    visual-support metrics.
- `unity/Echapee4D/Assets/MYB156/Editor/MYB156VisualSupportValidator.cs`
  - ticket-local Unity Editor validator for MYB-148/MYB-149 style scenes.
- `_bmad-output/unity-test-results/myb-156-visual-support-validator-report.md`
  - generated validation report path.

Relationship:

- MYB-155 catches ground-contact errors via bottomClearance.
- MYB-156 catches route-visible elevated assets that read as unsupported or
  floating even when MYB-155 ground-contact metrics do not flag them.
- The shot rubric uses both policies when judging Scale credibility.

### Unity Ride Runtime

File: `unity/Echapee4D/CONTEXT.md`

Owns the ride-loop language used by the Unity mock experience: effort,
resistance demand, applied resistance, and measured resistance.

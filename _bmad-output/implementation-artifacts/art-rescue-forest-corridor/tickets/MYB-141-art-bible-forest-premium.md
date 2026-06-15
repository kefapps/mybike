# MYB-141 — Verrouiller l’art bible forêt premium

## Linear metadata

- **Priority**: P0
- **Labels**: art-direction, foundation, forest-corridor
- **Estimate**: 2 pts
- **Project**: `Art Rescue — Forest Corridor Vertical Slice`
- **Status target after implementation**: `In Review`
- **Done gate**: Julien validates the canonical art bible.

## Scope

MYB-141 is a doc-only governance ticket.

Allowed outputs:

- Markdown files;
- reference links;
- canonical / non-canonical status banners;
- pointer updates in `AGENTS.md`, `CONTEXT.md`, and `CONTEXT-MAP.md`;
- Linear checklist and decision summary;
- text verification.

Forbidden outputs:

- Unity scene changes;
- asset generation;
- Blender output;
- Meshy/Tripo generation;
- asset import;
- production visual changes.

## Objective

Create and harden the canonical product art bible for the Art Rescue forest
corridor:

`docs/art-direction/mybike-forest-art-bible-v0.md`

The result must be short, hard, operational, and usable by Codex, Unity MCP,
Blender MCP, and human visual reviews. It must not simply copy the imported Art
Rescue art bible.

## MYB-141 Decision Summary Q1-Q8

### Q1 — Canonical art bible location

Decision:
The canonical product art bible lives at:

`docs/art-direction/mybike-forest-art-bible-v0.md`

The imported Art Rescue docs under
`_bmad-output/implementation-artifacts/art-rescue-forest-corridor/` are source
material, not the final source of truth.

If there is a conflict, the canonical art bible wins.

### Q2 — Scope of MYB-141 work

Decision:
MYB-141 must not simply copy the imported art bible.

It must create the canonical file and harden the content into an operational
visual contract aligned with the ADR decisions.

The document should stay concise, under roughly 8 Markdown pages.

### Q3 — Forest corridor, not all MyBike

Decision:
The art bible is specific to the Art Rescue forest corridor.

It inherits global MyBike visual governance from `AGENTS.md`, `CONTEXT.md`, and
applicable ADR decisions, but it does not define every future biome.

### Q4 — Fantasy dosage

Decision:
The forest corridor targets `Fantasy Scenic Premium Lisible`.

It must carry a real fantasy mood, visible from the canonical route camera.

The target is not a generic realistic forest and not an overloaded theme-park
fantasy scene.

Fantasy must frame the ride, not fight the ride.

### Q5 — Visual progress below Premium target

Decision:
A visual improvement that does not reach `Premium target` is a
`Checkpoint insuffisant`, not `Done`.

Allowed outcomes:

- corrective sub-ticket;
- documented exception accepted by Julien;
- rollback/rework if the change moves in the wrong direction.

### Q6 — Asset families in scope

Decision:
MYB-141 structures the forest art bible around exactly 7 families:

1. route and shoulders;
2. forest floor;
3. trees / trunks / canopy;
4. roots / arches / root clusters;
5. rocks / mossy stones / markers;
6. signs / fences / sculpted waypoints;
7. lighting / fog / atmospheric background.

Out of detailed scope:
UI, player bike, avatar, village, mountain, advanced weather, creatures, massive
fantasy architecture, active magic.

### Q7 — Five non-negotiable principles

Decision:
The art bible must put these 5 principles at the top, in this order:

1. Route Camera Decides
2. Fantasy Scenic Premium Lisible
3. Silhouettes Before Detail
4. Mood Is Built by Light, Fog, Material and Composition
5. Measured Ambition, No Cheap Fallback

These are mandatory production rules, not soft advice.

### Q8 — Validation and delivery

Decision:
MYB-141 is doc-only.

No Unity, no assets, no generation.

Minimum delivery:

1. canonical art bible created and hardened;
2. imported copies marked non-canonical;
3. `AGENTS.md`, `CONTEXT.md`, and `CONTEXT-MAP.md` point to the canonical art
   bible;
4. Linear ticket receives this Q1-Q8 summary and review checklist;
5. text verification confirms required terms and rejects `low-poly` as final
   aesthetic target;
6. final status is `In Review`, not `Done`, until Julien validates the art
   bible.

## MYB-141 Review Checklist

### Scope

- [ ] This ticket is doc-only.
- [ ] No Unity scene was modified.
- [ ] No asset was generated.
- [ ] No Blender/Meshy/Tripo output was created.
- [ ] No production visual change was made.

### Canonical file

- [ ] `docs/art-direction/mybike-forest-art-bible-v0.md` exists.
- [ ] It is marked as the canonical product art bible for the Art Rescue forest
      corridor.
- [ ] It is specific to the forest corridor and does not claim to define all
      future MyBike biomes.
- [ ] It references `_bmad-output/implementation-artifacts/art-rescue-forest-corridor/`
      as source material only.

### Non-canonical imported copies

- [ ] Imported Art Rescue art bible copies are marked as non-canonical.
- [ ] They point to `docs/art-direction/mybike-forest-art-bible-v0.md`.
- [ ] They state that the canonical art bible wins in case of conflict.

### Governance pointers

- [ ] `AGENTS.md` points to the canonical art bible.
- [ ] `CONTEXT.md` points to the canonical art bible.
- [ ] `CONTEXT-MAP.md` points to the canonical art bible.
- [ ] All three documents distinguish canonical product art bible from imported
      source material.

### Required content

- [ ] Uses `Stylisé Premium de Production` as the canonical visual direction.
- [ ] Does not use `low-poly` as a final aesthetic target.
- [ ] Defines `Fantasy Scenic Premium Lisible`.
- [ ] Contains the 5 non-negotiable visual principles in the approved order.
- [ ] Contains the 7 canonical asset families.
- [ ] Defines `Premium target`.
- [ ] Defines `Checkpoint insuffisant`.
- [ ] Defines the Canonical Visual Validation Surface.
- [ ] States that the route camera screenshot is blocking.
- [ ] States that overview explains but route decides.
- [ ] Defines measured visual ambition / performance as a guardrail.
- [ ] Explicitly rejects cheap/prototype visuals.

### Human review

- [ ] Julien reviewed the canonical art bible.
- [ ] Julien accepted the art bible as the current visual governance source.
- [ ] Only after this validation can MYB-141 move from `In Review` to `Done`.

## Acceptance Criteria

- [ ] `docs/art-direction/mybike-forest-art-bible-v0.md` exists.
- [ ] It is marked as the canonical product art bible for the Art Rescue forest
      corridor.
- [ ] It is hardened beyond the imported Art Rescue draft.
- [ ] It stays focused on the forest corridor and does not define all future
      MyBike biomes.
- [ ] It includes the 5 non-negotiable visual principles in the approved order.
- [ ] It includes the 7 canonical asset families.
- [ ] It defines `Stylisé Premium de Production`.
- [ ] It defines `Fantasy Scenic Premium Lisible`.
- [ ] It defines `Premium target`.
- [ ] It defines `Checkpoint insuffisant`.
- [ ] It defines the Canonical Visual Validation Surface.
- [ ] It states that route camera validation is blocking.
- [ ] It states that isolated previews are intermediate evidence only.
- [ ] It defines measured ambition and performance as a guardrail.
- [ ] It explicitly rejects cheap/prototype visuals.
- [ ] It does not use `low-poly` as a final aesthetic target.
- [ ] Imported Art Rescue copies are marked non-canonical.
- [ ] `AGENTS.md`, `CONTEXT.md`, and `CONTEXT-MAP.md` point to the canonical art
      bible.
- [ ] Linear MYB-141 contains the Q1-Q8 summary and review checklist.
- [ ] The ticket ends in `In Review`, not `Done`, until Julien validates the art
      bible.

## Text Verification

Required checks:

```bash
grep -RIn "low-poly" docs/art-direction AGENTS.md CONTEXT.md CONTEXT-MAP.md _bmad-output/implementation-artifacts/art-rescue-forest-corridor || true

grep -RIn "Stylisé Premium de Production" docs/art-direction AGENTS.md CONTEXT.md CONTEXT-MAP.md
grep -RIn "Fantasy Scenic Premium Lisible" docs/art-direction AGENTS.md CONTEXT.md CONTEXT-MAP.md
grep -RIn "Route Camera Decides" docs/art-direction
grep -RIn "Silhouettes Before Detail" docs/art-direction
grep -RIn "Mood Is Built by Light, Fog, Material and Composition" docs/art-direction
grep -RIn "Measured Ambition, No Cheap Fallback" docs/art-direction
grep -RIn "Premium target" docs/art-direction
grep -RIn "Checkpoint insuffisant" docs/art-direction
grep -RIn "Canonical Visual Validation Surface\\|Surface Canonique de Validation Visuelle" docs/art-direction
```

The presence of `low-poly` is allowed only as legacy note, proxy, LOD, collider,
mesh budget, or negative example. It is forbidden as a final visual target,
quality bar, or canonical direction.

## Anti-criteria

MYB-141 must not be accepted if:

- it only copies the imported art bible;
- it modifies Unity;
- it generates assets;
- it creates Blender/Meshy/Tripo output;
- it leaves ambiguous canonical paths;
- it keeps imported copies looking canonical;
- it uses `low-poly` as a final visual goal;
- it omits the 5 principles;
- it omits the 7 families;
- it omits `Premium target`;
- it omits `Checkpoint insuffisant`;
- it omits route-camera validation;
- it allows preview-only final validation;
- it closes as `Done` before human review.

## Implementation Notes

This ticket should end in `In Review`. `Done` requires explicit human validation
from Julien.

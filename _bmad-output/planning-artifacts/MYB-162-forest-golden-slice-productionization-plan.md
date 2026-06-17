# MYB-162 Forest Golden Slice Productionization Plan

## Summary

MYB-162 converts the MYB-159 / MYB-160 / MYB-161 learning loop into a canonical integration readiness plan.

The validated result is not a production scene to copy directly. The validated result is a visual direction:

- preserve the lush baseline forest mood;
- keep route-camera readability;
- use authored composition instead of random density;
- keep Meshy assets candidate-only until a separate promotion gate exists.

Julien validated the revised MYB-161 checkpoint on 2026-06-18. The accepted verdict remains:

- Premium target reached: No
- Checkpoint insuffisant
- Accepted as direction/checkpoint evidence, not final premium production art

## Baseline Evidence Map

| Ticket | Role | Key Evidence | Interpretation |
|---|---|---|---|
| MYB-159 | Golden slice authored route-camera preview | `_bmad-output/implementation-artifacts/MYB-159/` and `_bmad-output/visual-checkpoints/MYB-159/` | Proved a dedicated authored preview is needed; still too procedural and visually weak. |
| MYB-160 | Controlled Meshy candidate pass | `_bmad-output/implementation-artifacts/MYB-160/myb-160-meshy-hero-candidate-report.md` | Meshy can provide useful hero candidates, but only as controlled preview candidates. |
| MYB-161 first after | Sparse art-directed layout | `_bmad-output/visual-checkpoints/MYB-161/2026-06-17T18-58-*` historical evidence | Rejected direction: too sparse, technical, asset-preview-like. |
| MYB-161 revised after | Human-validated checkpoint direction | `_bmad-output/visual-checkpoints/MYB-161/2026-06-17T19-26-12Z-route-before-after.png` | Accepted direction: preserve baseline enclosure while keeping route readability improvements. |

Primary final evidence:

- route comparison: `_bmad-output/visual-checkpoints/MYB-161/2026-06-17T19-26-12Z-route-before-after.png`
- overview comparison: `_bmad-output/visual-checkpoints/MYB-161/2026-06-17T19-26-13Z-overview-before-after.png`
- MYB-161 report: `_bmad-output/implementation-artifacts/MYB-161/myb-161-implementation-report.md`
- MYB-160 report: `_bmad-output/implementation-artifacts/MYB-160/myb-160-meshy-hero-candidate-report.md`
- MYB-144 validator report: `_bmad-output/unity-test-results/myb-144-art-asset-validator-report.md`

## Accepted Visual Principles

The canonical integration pass should preserve these principles from the validated direction:

- route-camera decides;
- baseline forest mood is a strength, not noise to remove;
- generous near-camera canopy mass is desirable when it frames rather than blocks the road;
- green scenic enclosure matters more than isolated asset quality;
- route center must remain clean and readable;
- forest masses should be grouped and organic, not thin pickets;
- one readable natural hero beat is enough;
- composition should feel like riding through a forest, not inspecting an asset pack.

## Explicitly Rejected Patterns

The canonical integration pass must avoid these MYB-159 / MYB-160 / MYB-161 failure modes:

- sparse technical preview feeling;
- background thin pole or picket fence silhouettes;
- mushroom-cap tree dominance;
- brown branch clutter that reads as noise;
- random density added to compensate for weak composition;
- copying preview scene hierarchy into canonical ride scene;
- using Meshy output as production art by default;
- adding new Meshy spend before production-readiness decisions;
- hiding weak forms behind fog;
- claiming Premium target reached without human route-camera validation.

## Candidate Asset Status

MYB-160 Meshy candidates remain candidate-only:

| Asset | Status | Production Use |
|---|---|---|
| `myb160_meshy_tree_ancient_a_cleaned.fbx` | candidate, preview-only | Not promoted. May inform silhouette and tree assembly design. |
| `myb160_meshy_root_arch_a_cleaned.fbx` | candidate, preview-only | Not promoted. May inform one natural threshold / root landmark. |

Manifest state:

- `intakeStatus`: approved
- `promotionStatus`: candidate
- `license`: `Provider terms pending project review`
- no `reviewStatus`
- no `example:true`
- no production promotion

Production integration must either:

- keep these assets out of canonical production scenes; or
- open a separate promotion ticket with source, license, scale, pivot, material, collider, performance, and route-camera evidence.

## Canonical Integration Constraints

The next implementation ticket must not copy the preview scene wholesale. It should rebuild a small canonical forest passage using the validated direction.

Required constraints:

- no gameplay changes;
- no route trajectory changes;
- no route collider changes;
- no HUD or telemetry changes;
- no global scatter pass;
- no new Meshy generation by default;
- no production promotion of Meshy candidates;
- deterministic builder or clearly owned scene-local integration path;
- route-camera capture is the primary evidence;
- overview capture is secondary debugging evidence.

Placement rules:

- use combined renderer bounds `min.y` after transform for grounding;
- do not use `bounds.extents.y`, `bounds.size.y / 2`, or fixed half-height offsets as final placement;
- use a documented sink, normally 0.02m to 0.05m;
- report bottomClearance and route-visible floating counts;
- route-visible unsupported canopy count must be 0.

## Readiness Decision

MYB-162 recommends one next implementation ticket, not a broad art rewrite.

The next ticket should be narrow:

`MYB-163 - Canonical Forest Passage Integration From Validated Golden Slice Direction`

Purpose:

- apply the validated forest enclosure direction to the canonical forest passage;
- preserve ride readability;
- rebuild composition intentionally rather than copying preview objects;
- produce before/after route and overview captures;
- keep Meshy candidates preview-only unless separately promoted.

## Proposed MYB-163 Scope

Create a deterministic canonical forest passage integration pass with:

- baseline canonical route capture before changes;
- scene-local or builder-owned composition updates;
- richer near-camera canopy frame;
- grouped forest masses replacing thin/picket silhouettes;
- one restrained natural landmark or threshold idea;
- stronger ground integration around tree bases;
- no route overlap;
- no route-visible floating assets;
- no route-visible unsupported canopies.

Expected outputs:

- updated canonical scene or ticket-owned canonical integration builder;
- MYB-145 route/overview before-after captures;
- MYB-144 validator report;
- metrics JSON with route, grounding, support, and visual-read fields;
- implementation report;
- governance review;
- final human visual review gate.

Expected verdict:

- Premium target reached: No unless Julien explicitly validates otherwise;
- In Review until route-camera human validation.

## Capture And Validation Checklist

Before any canonical integration is accepted:

- capture current canonical route view before changes;
- capture current canonical overview before changes;
- apply only the scoped forest passage change;
- capture route after;
- capture overview after;
- produce route before-after sheet;
- produce overview before-after sheet;
- run MYB-144;
- report any validator warnings truthfully;
- record routeVisibleFloatingAssetCount;
- record routeVisibleUnsupportedCanopyCount;
- record routeOverlapCount;
- record minimumRouteClearanceMeters;
- report whether Meshy assets are used directly, indirectly, or only as references.

Route-camera review must answer:

- does the road still read immediately?
- does the image feel more like a forest ride?
- does the canopy enclosure preserve the preferred baseline mood?
- are thin pickets reduced?
- are mushroom-cap silhouettes reduced or better supported?
- does the hero beat read without becoming clutter?

## Rollback And Non-Regression Criteria

Rollback or rework if any of these occur:

- route readability regresses;
- route overlap count is greater than 0;
- route-visible floating asset count is greater than 0;
- route-visible unsupported canopy count is greater than 0;
- the route view becomes sparse and technical again;
- the forest reads like isolated props beside a road;
- Meshy candidate assets appear as production-promoted content without a promotion ticket;
- the canonical scene loses the baseline forest enclosure Julien preferred.

Non-regression evidence must include:

- before/after route comparison;
- before/after overview comparison;
- MYB-144 PASS or documented non-blocking warnings;
- explicit human review status.

## Final Recommendation

Proceed with MYB-163 only after accepting this MYB-162 plan as the integration guardrail.

Do not restart broad asset generation. The next useful move is a constrained canonical integration pass that protects the human-validated baseline mood and turns the preview learning into controlled production evidence.

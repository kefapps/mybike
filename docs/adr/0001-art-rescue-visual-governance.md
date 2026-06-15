# ADR 0001: Art Rescue Visual Governance

Date: 2026-06-15
Status: Accepted

## Context

The Art Rescue work started after the MYB-114 to MYB-137 forest-corridor
experiments showed a clear pattern: the scene could become denser and more
enveloping while still looking poor, prototype-like, or cheaply optimized.

The previous "low-poly" framing pulled production toward visual economy as an
aesthetic goal. That made it too easy to accept weak silhouettes, flat lighting,
cheap materials, and isolated assets that looked acceptable in previews but did
not improve the player-facing ride camera.

Future Art Rescue tickets need a shared governance layer before further visible
production work: a canonical visual target, clear gates, a closure rubric, a
validation surface, and a measured visual/performance arbitration rule.

## Decision

The canonical visual direction for MyBike Art Rescue is `Stylisé Premium de
Production`.

`Socle Low-Poly de Production` is legacy language. `Low-poly` must not be used
as the final aesthetic target, quality bar, or governance term. It may remain
only as a local technical qualifier for a mesh budget, proxy, LOD, or modeling
method.

`MYB-141` to `MYB-146` form the blocking Art Rescue governance layer:

- Hard gate before visible production: `MYB-141`, `MYB-142`, `MYB-145`,
  `MYB-146`.
- Hard gate before asset promotion: `MYB-143`, `MYB-144`.

For visible Art Rescue tickets, `Done` requires `Premium target` on the route
capture, plus human validation when the judgment is subjective, unless Julien
explicitly accepts a documented exception.

`Premium target` means:

- global rubric average >= 4;
- `Silhouette quality >= 4`;
- `Lighting mood >= 4`;
- `Material coherence >= 4`;
- human validation recorded when subjective.

If a ticket improves the scene but does not reach `Premium target`, the result is
a documented `Checkpoint insuffisant`, not `Done`. The next step must be one of:

- targeted corrective sub-ticket;
- documented exception accepted by Julien;
- rollback or rework if the change moves the scene in the wrong direction.

The canonical visual validation surface is the route screenshot taken in the
canonical ride corridor from the canonical ride camera. The overview screenshot
is required but secondary. Preview scenes, Blender renders, Meshy/Tripo previews,
quarantine scenes, turntables, isolated asset screenshots, and overview-only
captures are intermediate evidence only.

Art Rescue optimizes for visual ambition first, then measured performance. The
default Art Rescue target is 60 FPS unless a ticket defines another target.
Performance budgets are guardrails, not excuses for poor visuals.

As long as the scene reaches the FPS target and no blocking validator `ERROR` is
reported, more geometry, material complexity, density, or composition work is
allowed when the gain is visible from the canonical route camera.

If performance falls below target or a blocking validator `ERROR` appears,
reduce in this order:

1. unnecessary collisions;
2. oversized textures and excessive materials;
3. density outside the route camera;
4. LOD, culling, and distance settings;
5. non-essential near-route density;
6. premium route-camera-visible elements last.

No visible premium downgrade is allowed without measurement.

## Consequences

`Done` becomes a strong quality signal for visible Art Rescue tickets. Useful
progress can still be preserved as evidence, but it cannot lower the visual
closure bar.

Future agents must validate visual production in the player-facing ride camera,
not in isolated asset previews or flattering screenshots.

Performance work must protect the route-camera premium result. Optimization
should remove invisible cost and waste before degrading visible premium elements.

The governance layer intentionally slows the start of production work so the
team avoids another cycle of producing assets, integrating them, and discovering
that the scene still looks prototype-like.

This ADR is operationalized by:

- `AGENTS.md`;
- `CONTEXT.md`;
- `CONTEXT-MAP.md`;
- `_bmad-output/implementation-artifacts/art-rescue-forest-corridor/`;
- Linear issues `MYB-141` through `MYB-153`.

## Alternatives considered

- Keep `low-poly` as the visual target - rejected because it incentivized cheap
  silhouettes and premature visual economy.
- Allow `Acceptable vertical slice` to close visible Art Rescue tickets -
  rejected because it would normalize progress below the stated premium target.
- Validate assets in preview scenes or isolated renders - rejected because
  player-facing quality is decided by the canonical ride camera.
- Treat performance budgets as hard aesthetic ceilings - rejected because the
  project needs measured ambition, not optimization into poor visuals.
- Ignore performance until the end - rejected because the target remains a
  real-time Unity vertical slice with measured FPS and validator guardrails.

# Forest Corridor Shot Rubric

Status: Canonical reusable visual rubric for Art Rescue forest corridor reviews.

Scope: future forest corridor visual production tickets.

Type: validation rubric.

This rubric defines scoring mechanics and review workflow. The forest corridor
art direction remains owned by `docs/art-direction/mybike-forest-art-bible-v0.md`.

If the two documents conflict:

- `docs/art-direction/mybike-forest-art-bible-v0.md` wins for art direction.
- `docs/validation/forest-corridor-shot-rubric.md` wins for scoring mechanics.

## 1. Canonical Validation Surface

Final visual validation for production Art Rescue tickets must use:

- route screenshot from the canonical ride camera;
- overview screenshot from the same scene state.

Rule:

```txt
Overview explains. Route decides.
```

Preview scenes, Blender renders, Meshy/Tripo previews, turntables, sandbox
captures, quarantine scenes, hero shots, overview-only captures, and isolated
asset screenshots are intermediate evidence only. They cannot prove `Premium
target` and cannot close a visible production ticket.

## 2. Diagnostic Surface

A Diagnostic Surface is evidence used to audit, calibrate, compare, and learn.

Examples:

- MYB-137 captures;
- route/overview screenshots from experiments;
- proof-of-volume scenes;
- sandbox or quarantine scenes;
- audit reports;
- counter-example captures.

Allowed uses:

- scoring initial quality;
- calibrating the rubric;
- identifying failure modes;
- producing corrective actions;
- comparing progress qualitatively.

Forbidden uses:

- closing a visible production ticket as `Done`;
- declaring `Premium target` reached;
- promoting an asset to production;
- replacing canonical route-camera validation.

## 3. 9-Criterion Rubric

Score each criterion from 1 to 5.

### Blocking Criteria

The blocking criteria must all reach 4 or higher for `Premium target`.

1. Route readability
2. Silhouette quality
3. Lighting mood
4. Material coherence

### Contributive Criteria

The contributive criteria shape the average and explain what is missing. A major
contradiction in a contributive criterion can still block human validation.

5. Foreground richness
6. Midground density
7. Background depth
8. Scale credibility
9. Composition rhythm

## 4. Criterion Definitions

| # | Criterion | Type | What it judges | Premium requirement |
|---|---|---|---|---|
| 1 | Route readability | Blocking | Road clarity, trajectory, contrast, ride readability at 20-40 m, absence of obstruction. | >= 4 |
| 2 | Silhouette quality | Blocking | Strong readable masses for trees, trunks, canopy, roots, rocks, markers, and landmarks. | >= 4 |
| 3 | Lighting mood | Blocking | Fantasy mood, fog, depth, value rhythm, warm/cool balance, and readability despite atmosphere. | >= 4 |
| 4 | Material coherence | Blocking | Consistent stylized materials for route, soil, moss, wood, stone, foliage, and signs. | >= 4 |
| 5 | Foreground richness | Contributive | Near-camera richness without visual noise: shoulders, roots, moss, leaves, stones, ground detail. | Contributes to average |
| 6 | Midground density | Contributive | Forest corridor body, side massing, tree distribution, and controlled density. | Contributes to average |
| 7 | Background depth | Contributive | Distant silhouettes, fog layers, atmospheric backdrop, horizon treatment, and forest depth. | Contributes to average |
| 8 | Scale credibility | Contributive | Believable proportions and ground contact across route, trees, roots, rocks, signs, camera height, and ride speed. | Contributes to average; no major contradiction; route-visible ground contact blockers apply |
| 9 | Composition rhythm | Contributive | Alternation of breathing room, density, landmarks, scenic cadence, and route-first guidance. | Contributes to average; no major contradiction |

### Ground Contact Gate

Scale credibility includes asset-to-ground contact. A route-visible asset that
floats above the forest floor usually reads as prototype, regardless of its
silhouette or material quality.

Canonical policy:

- Ground visible Art Rescue assets by combined renderer bounds `min.y` after
  transform.
- Do not use `bounds.extents.y`, `bounds.size.y / 2`, or fixed half-height
  offsets as the final vertical placement policy.
- Apply a documented sink, usually 0.02m to 0.05m.
- If raycasting, use an explicit ground layer mask or documented ground source,
  ignore triggers, and avoid generated assets, patches, or props as ground hits.

bottomClearance thresholds:

- target: -0.05m to +0.05m;
- warning floating: > +0.05m;
- blocking floating for route-visible assets: > +0.10m;
- warning sinking: < -0.10m;
- blocking sinking for route-visible assets: < -0.25m.

Rule:

```txt
Route-visible floating assets above +0.10m block checkpoint review unless
Julien explicitly accepts a documented exception.
```

### Visual Support Gate

Scale credibility also includes visible support for elevated assets. Canopies,
leaf masses, hanging forms, and overhead scenic elements can pass bottomClearance
logic while still reading as floating from the route camera.

Canonical policy:

- Use `docs/validation/unity-visual-support-policy.md` for above-ground visual
  support checks.
- Classify elevated canopies and leaf masses as `supportedAboveGround`, not as
  silently exempt.
- Require credible trunk/support evidence from the route camera, or a documented
  Julien-accepted exception.

Rule:

```txt
Route-visible unsupported canopies or elevated leaf masses block checkpoint
review unless Julien explicitly accepts a documented exception.
```

## 5. Score Scale

| Score | Meaning | Description |
|---:|---|---|
| 1 | Failed / prototype poor | The criterion clearly fails and reads as placeholder, broken, or unmanaged. |
| 2 | Weak / visible issue | The criterion exists but the problem is visible without deep analysis. |
| 3 | Acceptable checkpoint | The criterion shows recognizable progress, but not enough to close visual production. |
| 4 | Premium target | The criterion reaches the expected production Art Rescue bar. |
| 5 | Strong premium / reference quality | The criterion exceeds target and can guide future work. |

## 6. Verdicts And Closure

The rubric can produce both a score band and a closure verdict.

This matters because a diagnostic baseline can show prototype-level weaknesses
while still being a useful `Checkpoint insuffisant` if it demonstrates real
progress.

### Prototype

Typical condition:

- average < 3;
- or at least one blocking criterion <= 2;
- or route readability <= 2.

Usage:

- diagnostic only;
- no visual closure;
- rollback/rework likely unless the ticket is explicitly a proof or calibration
  ticket.

### Acceptable Checkpoint

Typical condition:

- average >= 3;
- no blocking criterion <= 2;
- route readability >= 3.

Usage:

- progress evidence;
- not enough for `Done`;
- can generate targeted corrective actions.

### Checkpoint Insuffisant

Used when:

- visible improvement exists;
- evidence is documented;
- `Premium target` is not reached.

Allowed outcomes:

- corrective sub-ticket;
- documented exception accepted by Julien;
- rollback/rework if the result moves in the wrong direction.

### Premium Target

Required condition:

- global average >= 4;
- Route readability >= 4;
- Silhouette quality >= 4;
- Lighting mood >= 4;
- Material coherence >= 4;
- no major contradiction in contributive criteria;
- no route-visible ground contact blocker;
- human validation when the judgment is subjective.

Usage:

- `Done` is possible for visible Art Rescue production tickets only when all
  ticket-specific technical criteria are also satisfied.

## 7. Major Contributive Contradictions

A contributive criterion can block human validation when it strongly contradicts
the route-camera result.

Examples:

- Scale credibility = 2 because trees, rocks, or signs feel like toy props.
- Scale credibility is blocked because route-visible assets float more than
  +0.10m above the ground or sink below -0.25m without a documented exception.
- Composition rhythm = 2 because the corridor is uniform wallpaper with no
  scenic cadence.
- Background depth = 1 because the horizon is empty, broken, or visually flat.
- Foreground richness = 1 because the route is surrounded by prototype void.
- Midground density = 1 because there is no convincing forest corridor.

Rule:

```txt
Blocking criteria decide the minimum bar.
Contributive criteria explain the image.
A major contradiction can still block human validation.
```

## 8. Rubric Criteria To Asset Families

| Rubric criterion | Most relevant MYB-141 asset families |
|---|---|
| Route readability | Route and shoulders; Forest floor; Lighting / fog / atmospheric background |
| Silhouette quality | Trees / trunks / canopy; Roots / arches / root clusters; Rocks / mossy stones / markers; Lighting / fog / atmospheric background |
| Lighting mood | Lighting / fog / atmospheric background; Route and shoulders; Forest floor |
| Material coherence | Route and shoulders; Forest floor; Trees / trunks / canopy; Rocks / mossy stones / markers; Signs / fences / sculpted waypoints |
| Foreground richness | Forest floor; Roots / arches / root clusters; Rocks / mossy stones / markers; Route and shoulders |
| Midground density | Trees / trunks / canopy; Roots / arches / root clusters; Rocks / mossy stones / markers; Signs / fences / sculpted waypoints |
| Background depth | Lighting / fog / atmospheric background; Trees / trunks / canopy |
| Scale credibility | Route and shoulders; Trees / trunks / canopy; Rocks / mossy stones / markers; Signs / fences / sculpted waypoints |
| Composition rhythm | All corridor families, especially trees, roots, landmarks, and lighting |

## 9. Reusable Review Template

```md
## Visual Rubric Score

Validation surface:
- Diagnostic Surface / Canonical Validation Surface:

Evidence:
- Route screenshot:
- Overview screenshot:
- Ticket:
- Commit / branch:
- Scene:
- Camera:
- Date:

Ground placement metrics:
- floatingAssetCount:
- maxFloatingClearance:
- sinkingAssetCount:
- maxSinkingDepth:
- routeVisibleFloatingAssetCount:
- groundPlacementMethod:
- groundLayerMask / groundSource:
- sinkMeters:

| # | Criterion | Type | Score 1-5 | Notes |
|---|---|---|---:|---|
| 1 | Route readability | Blocking |  |  |
| 2 | Silhouette quality | Blocking |  |  |
| 3 | Lighting mood | Blocking |  |  |
| 4 | Material coherence | Blocking |  |  |
| 5 | Foreground richness | Contributive |  |  |
| 6 | Midground density | Contributive |  |  |
| 7 | Background depth | Contributive |  |  |
| 8 | Scale credibility | Contributive |  |  |
| 9 | Composition rhythm | Contributive |  |  |

Average:

Blocking criteria all >= 4:
- Yes / No

Contributive contradiction:
- Yes / No

Ground contact blocker:
- Yes / No

Premium target reached:
- Yes / No

Verdict:
- Prototype
- Acceptable checkpoint
- Checkpoint insuffisant
- Premium target
- Rollback / rework required

Human validation:
- Required: Yes / No
- Reviewer:
- Decision:
```

## 10. Visual Checkpoint Template

```md
## Visual Checkpoint Verdict

Status: Checkpoint insuffisant

Premium target reached:
- No

Screenshots:
- Route:
- Overview:

Rubric:
- Global average:
- Route readability:
- Silhouette quality:
- Lighting mood:
- Material coherence:

What improved:
-

Why it is not Done:
-

Primary blocker:
-

Decision:
- [ ] Create corrective sub-ticket
- [ ] Request documented exception
- [ ] Rollback / rework

Follow-up:
-
```

## 11. Visual Closure Exception Template

An exception is possible only when Julien explicitly accepts it.

```md
## Visual Closure Exception

Status: Accepted / Rejected
Accepted by: Julien
Date:

Reason:
-

Scope:
-

Current rubric:
- Global average:
- Route readability:
- Silhouette quality:
- Lighting mood:
- Material coherence:

Why Premium target is not required for this ticket:
-

Risk accepted:
-

Required follow-up:
-
```

An exception closes a ticket only inside its documented scope. It does not change
the canonical `Premium target` bar.

## 12. Review Checklist

### Required Evidence

- [ ] Route screenshot is provided.
- [ ] Overview screenshot is provided.
- [ ] Validation surface is identified.
- [ ] Scene, camera, branch/commit, and date are documented.
- [ ] Preview-only or overview-only evidence is not treated as final proof.

### Rubric

- [ ] All 9 criteria are scored.
- [ ] The 4 blocking criteria are identified.
- [ ] The 5 contributive criteria are identified.
- [ ] Average is calculated.
- [ ] Blocking criteria all >= 4 is marked Yes/No.
- [ ] Contributive contradiction is marked Yes/No.
- [ ] Ground contact blocker is marked Yes/No for route-visible assets.
- [ ] Verdict is explicit.

### Premium Target

- [ ] Average >= 4.
- [ ] Route readability >= 4.
- [ ] Silhouette quality >= 4.
- [ ] Lighting mood >= 4.
- [ ] Material coherence >= 4.
- [ ] No major contributive contradiction exists.
- [ ] No route-visible asset floats above +0.10m or sinks below -0.25m unless
      Julien accepted a documented exception.
- [ ] Human validation is complete when subjective.

### Closure Rule

- [ ] `Done` is not used when `Premium target` is missing, unless Julien accepted
      a documented exception.
- [ ] Progress below `Premium target` is documented as `Checkpoint insuffisant`.
- [ ] Follow-up action is explicit.

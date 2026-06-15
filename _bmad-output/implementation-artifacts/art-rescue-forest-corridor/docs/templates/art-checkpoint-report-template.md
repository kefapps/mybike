# <TICKET> — Art Checkpoint Report

## Objective

<What this pass tries to prove.>

## Scope

- Scene(s): `<path>`
- Builder/tooling: `<path>`
- Baseline: `<ticket or scene>`

## Generated / Modified Assets

| Asset | Type | Path | Notes |
|---|---|---|---|
| | | | |

## Metrics

| Metric | Value |
|---|---:|
| Renderers | |
| Triangles | |
| Materials | |
| Textures > 1024 | |
| Textures > 2048 | |
| MeshColliders | |
| FPS Target | |
| Observed FPS / Perf Metric | |
| Validator Errors | |
| Validator Warnings | |

## Visual / Performance Arbitration

- Route-camera visual spend: `<what changed that should be visible from route camera>`
- Is the gain visible from canonical route camera: `<Yes | No | Not evaluated>`
- Blocking validator ERROR: `<Yes | No>`
- FPS target met: `<Yes | No | Not measured>`
- If performance failed, reduction order used: `<collisions | textures/materials | off-camera density | LOD/culling/distance | non-essential near-route density | premium visible elements>`
- Visible premium downgrade measured: `<Yes | No | N/A>`

## Visual / Performance Exception

Status: <Accepted | Rejected | N/A>
Accepted by: <Julien | N/A>
Date:

Reason:
-

Current visual state:
-

Current performance state:
-

Compromise accepted:
-

Risk:
-

Follow-up required:
-

## Captures

- Route: `<path>`
- Overview: `<path>`
- Before/After route: `<path>`
- Before/After overview: `<path>`

## Canonical Visual Validation Surface

- Unity project: `unity/Echapee4D`
- Validation context: `<canonical ride corridor scene/path>`
- Route camera: `<canonical ride camera>`
- Same scene state for route and overview: `<Yes | No>`
- Non-final evidence used: `<preview scene | Blender render | Meshy/Tripo preview | quarantine scene | turntable | isolated screenshot | none>`

Route screenshot is blocking. Overview is required for context but cannot
compensate for a weak route view.

## Visual Verdict

<Direct visual conclusion.>

## Visual Checkpoint Verdict

Status: <Not evaluated | Checkpoint insufficient | Premium target reached | Done with documented exception | Rollback / rework required>

Premium target reached: <Yes | No>

Screenshots:
- Route: `<path>`
- Overview: `<path>`

Rubric:
- Global average:
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

## Rubric Score

| Criterion | Score 1-5 | Notes |
|---|---:|---|
| Route readability | | |
| Foreground richness | | |
| Midground density | | |
| Background depth | | |
| Silhouette quality | | |
| Lighting mood | | |
| Material coherence | | |

## Known Issues

- <Known issue or `None`.>

## Recommendation

<Keep, rework, reject, or validate as checkpoint.>

## Status

Do not close until Premium target is reached and user visual validation is
recorded, unless Julien explicitly accepts a documented exception.

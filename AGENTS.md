# AGENTS.md - mybike / Echappee 3D

## Agent Contract

This repository is currently driven by narrow Linear tickets, BMAD artifacts, and
Unity-first implementation. Coding agents must optimize for a playable,
validated vertical slice, not for broad exploration.

Default behavior:

1. Stay scoped to the current Linear issue.
2. Preserve mock-mode playability at all times.
3. Treat `unity/Echapee4D` as the canonical active Unity project.
4. Treat `src/**` as the parked historical React/Vite/Three.js prototype.
5. Prefer small, reviewable changes with local validation evidence.
6. Do not solve visual quality by bulk-importing generated assets.
7. Do not call Meshy, Tripo, or other external generation services unless the
   current Linear issue explicitly authorizes that pipeline.
8. Never commit API keys, Linear tokens, Meshy tokens, Tripo tokens, Unity
   credentials, or other secrets.

When instructions conflict, use this priority order:

1. Direct human instruction in the current task.
2. The current Linear issue scope and acceptance criteria.
3. This `AGENTS.md` file.
4. Current BMAD planning and implementation artifacts.
5. Historical reports, archived artifacts, and old prototypes.

If a conflict would expand scope or change product direction, stop and ask for
confirmation instead of silently widening the work.

## Project Context

mybike is a Unity-first first-person scenic cycling game, currently scoped to
the Echappee 3D vertical slice. The priority runtime target is macOS through
the canonical Unity project; WebGL remains a secondary validation/demo path.

As of 2026-06-11, `MYB-94` records the active platform decision: Unity macOS
first. The IvanMurzak Unity-MCP probe and WebGL readiness evidence from
`MYB-89` and `MYB-90` remain useful technical proof, but they no longer make
WebGL the primary product target.

Canonical Unity project:

- `unity/Echapee4D`

Removed or reference-only legacy areas:

- `unity/Echappee3D` was removed by `MYB-92`; references to it in old BMAD
  artifacts are historical evidence only.
- `src/**` is the historical React/Vite/Three.js prototype. Do not extend it
  unless a Linear issue explicitly says to touch the parked web prototype.

The MVP must remain playable entirely in mock mode, without connected-bike
hardware. macOS/CoreBluetooth/FTMS is a product direction for connected-bike
work, but real-bike telemetry, Meshy, heavy asset pipelines, rich history,
multiple routes, Web Bluetooth, Android delivery, and post-MVP backlog work are
out of scope unless a Linear issue explicitly says otherwise.

## Canonical Paths

Use these paths consistently:

- Unity project: `unity/Echapee4D`
- Unity scenes: `unity/Echapee4D/Assets/Scenes`
- Ticket-specific Unity editor builders: prefer the existing ticket-local
  convention, for example `unity/Echapee4D/Assets/MYB###/Editor/`
- BMAD planning artifacts: `_bmad-output/planning-artifacts/`
- BMAD implementation artifacts: `_bmad-output/implementation-artifacts/`
- Unity validation output: `_bmad-output/unity-test-results/`
- Linear sync ledger: `_bmad-output/linear-sync.md`
- Canonical forest corridor art bible:
  `docs/art-direction/mybike-forest-art-bible-v0.md`
- Art rescue docs, when imported locally:
  `_bmad-output/implementation-artifacts/art-rescue-forest-corridor/`

Do not create a new Unity project, new root-level art pipeline, or broad backlog
folder unless the current issue explicitly asks for it.

## Active Planning Sources

Use these files as the planning source of truth:

- `_bmad-output/planning-artifacts/echappee-3d-gdd-court.md`
- `_bmad-output/planning-artifacts/echappee-3d-architecture-mince.md`
- `_bmad-output/planning-artifacts/echappee-3d-mvp-epic-stories.md`
- `_bmad-output/planning-artifacts/sprint-change-proposal-unity-canonical-2026-06-11.md`
- `_bmad-output/linear-sync.md`

The old archive at `/Users/jbodin/Downloads/echappee_3d_linear_package_v2.zip`
is reference material only. Do not import it wholesale and do not recreate its
full 32-ticket backlog.

## Art Rescue / Forest Corridor Sources

As of the MYB-114 to MYB-137 visual work, the forest corridor direction is
route-first: author the ride corridor from the route camera first, then layer
near-ground material, berms, roots, trunks, silhouettes, fog, and lighting
around that readable ride path.

Canonical forest corridor art bible:

- `docs/art-direction/mybike-forest-art-bible-v0.md`

This file is the source of truth for Art Rescue forest corridor visual
production. It is specific to the forest ride corridor and inherits global
MyBike visual governance from `CONTEXT.md`, this file, and applicable ADR
decisions. It does not define every future MyBike biome.

Use the imported art-rescue pack as source material, implementation evidence,
and historical reference:

- `docs/adr/0001-art-rescue-visual-governance.md`
- `_bmad-output/implementation-artifacts/art-rescue-forest-corridor/README.md`
- `_bmad-output/implementation-artifacts/art-rescue-forest-corridor/ticket-index.md`
- `_bmad-output/implementation-artifacts/art-rescue-forest-corridor/docs/00-executive-summary.md`
- `_bmad-output/implementation-artifacts/art-rescue-forest-corridor/docs/art-direction/01-visual-diagnosis-myb-137.md`
- `_bmad-output/implementation-artifacts/art-rescue-forest-corridor/docs/art-direction/02-art-bible-v0.md`
- `_bmad-output/implementation-artifacts/art-rescue-forest-corridor/docs/art-direction/04-route-first-corridor-grammar.md`
- `_bmad-output/implementation-artifacts/art-rescue-forest-corridor/docs/art-direction/05-asset-taxonomy-and-budgets.md`
- `_bmad-output/implementation-artifacts/art-rescue-forest-corridor/docs/workflows/codex-unity-blender-mcp-playbook.md`
- `_bmad-output/implementation-artifacts/art-rescue-forest-corridor/docs/workflows/blender-procedural-kit-workflow.md`
- `_bmad-output/implementation-artifacts/art-rescue-forest-corridor/docs/workflows/meshy-tripo-quarantine-workflow.md`
- `_bmad-output/implementation-artifacts/art-rescue-forest-corridor/docs/workflows/unity-import-promotion-workflow.md`
- `_bmad-output/implementation-artifacts/art-rescue-forest-corridor/docs/workflows/visual-checkpoint-workflow.md`
- `_bmad-output/implementation-artifacts/art-rescue-forest-corridor/docs/validators/unity-art-asset-validator-spec.md`
- `_bmad-output/implementation-artifacts/art-rescue-forest-corridor/docs/validators/scene-composition-validator-spec.md`
- `_bmad-output/implementation-artifacts/art-rescue-forest-corridor/docs/validators/visual-regression-validator-spec.md`
- `_bmad-output/implementation-artifacts/art-rescue-forest-corridor/docs/validators/third-party-asset-manifest-validator-spec.md`
- `_bmad-output/implementation-artifacts/art-rescue-forest-corridor/docs/validators/performance-budget-validator-spec.md`

If imported Art Rescue docs conflict with
`docs/art-direction/mybike-forest-art-bible-v0.md`, the canonical art bible wins.
For forest corridor tickets, apply both global MyBike visual rules and the
forest-specific art bible rules.

MYB-142 visual audit and rubric outputs:

- `docs/art-direction/myb-137-visual-diagnosis.md`
  - dated diagnostic and calibration audit for MYB-137;
  - not reusable as the generic rubric;
  - not `Premium target` evidence.
- `docs/validation/forest-corridor-shot-rubric.md`
  - reusable canonical rubric for future Art Rescue forest corridor visual
    reviews;
  - defines the 9 criteria, thresholds, validation surfaces, and review
    templates.

MYB-137 calibrates the rubric. The canonical route camera validates production
quality.

MYB-143 asset intake and promotion outputs:

- `docs/schemas/third-party-asset-manifest.md`
  - canonical schema documentation for Art Rescue asset intake;
  - defines `intakeStatus`, `promotionStatus`, source types, usage scopes, and
    MYB-144 validation rules.
- `docs/manifests/art-rescue-asset-manifest.json`
  - canonical machine-readable manifest data for Art Rescue asset candidates;
  - versioned root object with `schemaVersion`, `updatedAt`, and `assets`.
- `docs/workflows/meshy-tripo-quarantine-workflow.md`
  - quarantine-first intake workflow for Meshy, Tripo, external, Blender MCP,
    in-house, and procedural asset candidates.

An asset with `intakeStatus: approved` is not automatically production content.
Visible assets need `promotionStatus: promoted`, validator evidence, and
route-camera evidence before they can be treated as production-valid in the
forest corridor.

If the imported art-rescue docs are missing, do not invent their contents. Use
the current Linear issue, existing MYB-114/MYB-137 reports, and this file, then
write down the missing reference in the implementation notes.

## Visual Production Rules

The canonical visual direction term for MyBike is `Stylisé Premium de
Production`.

For Art Rescue forest corridor work, the canonical product art bible is
`docs/art-direction/mybike-forest-art-bible-v0.md`. It defines the local
`Fantasy Scenic Premium Lisible` target, the five non-negotiable visual
principles, and the seven forest corridor asset families.

`Socle Low-Poly de Production` is legacy language. `Low-poly` must not be used
as the final aesthetic target or quality bar in project governance. The target
is not cheaper-looking art; it is a stylized, readable, performant, controlled
image: strong silhouettes, clear route, camera-first composition, coherent
lighting/fog, credible materials, and controlled density.

Geometry constraints remain valid, but they serve `Stylisé Premium de
Production`; they do not replace it. `Low-poly` may appear only as a local
technical qualifier for geometry budget or modeling method, never as the
canonical art direction.

For forest-corridor work, the route-camera capture is more important than the
overview capture. Overview images are useful for debugging placement, but they
must not override first-person ride readability.

Prioritize visual quality in this order:

1. Route readability from the cyclist camera.
2. Strong side-corridor shape language: berms, roots, trunk bases, rising forms.
3. Ground material richness: moss, leaf litter, exposed soil, dead branches.
4. Vertical rhythm: close trunks, leaning trunks, grouped silhouettes.
5. Lighting, fog, and palette coherence.
6. Secondary hero props and setpieces.

Avoid these failure modes:

- flat decorative ribbons beside the road;
- isolated road-side props that do not belong to the terrain;
- tree trunks that read as simple posts;
- symbolic cones, tufts, or placeholder blobs presented as final art;
- dense clutter that hides the route or creates camera noise;
- mixing photoreal, legacy low-poly aesthetic, AI-generated, and placeholder
  assets without an explicit art-direction gate;
- treating Meshy/Tripo output as automatically game-ready.

Visual tickets are not Done merely because a scene compiles. They need route and
overview captures, a short visual verdict, validation output, and human visual
approval when the ticket is subjective or art-directional.

For Art Rescue forest corridor reviews, use the reusable shot rubric at
`docs/validation/forest-corridor-shot-rubric.md`. It defines 9 visual criteria,
with blocking criteria for route readability, silhouette quality, lighting mood,
and material coherence. Use `docs/art-direction/myb-137-visual-diagnosis.md` as
MYB-137 calibration evidence only, not as final production validation.

For visible Art Rescue tickets, useful progress is evidence, not closure. If the
route capture does not reach `Premium target`, the ticket cannot be closed as
`Done` unless Julien explicitly accepts a documented exception. Mark the result
as `Checkpoint insuffisant`, then create a targeted corrective sub-ticket, ask
for a documented exception, or rollback/rework if the change moves the scene in
the wrong direction.

The canonical visual validation surface for Art Rescue is the route screenshot
taken in the canonical ride corridor from the canonical ride camera. Preview
scenes, Blender renders, Meshy/Tripo previews, quarantine scenes, turntables,
overview-only captures, and isolated asset screenshots are intermediate
evidence only. They do not validate `Premium target` and cannot close a
production visual ticket.

Art Rescue optimizes for `Stylisé Premium de Production` first, then measures
performance. Performance budgets are measured vertical-slice guardrails, not an
excuse for poor, empty, or cheap visuals. If the scene reaches the defined FPS
target, defaulting to 60 FPS for Art Rescue, and validators report no blocking
`ERROR`, visual enrichment is allowed when it visibly improves the canonical
route-camera screenshot.

If performance falls below target or a blocking `ERROR` appears, reduce in this
order: unnecessary collisions; oversized textures and excessive materials;
density outside the route camera; LOD, culling, and distance settings;
non-essential near-route density; premium route-camera-visible elements last. No
visible premium downgrade is allowed without measurement.

## Asset Pipeline Rules

Use this default pipeline unless the current issue says otherwise:

1. Build greybox and procedural structure in Unity.
2. Generate clean modular kit pieces with Blender MCP when asset geometry is
   needed.
3. Import assets into controlled Unity folders using stable names.
4. Run validators and capture the route view.
5. Promote assets only after manifest, scale, pivot, material, collider, and
   visual-readability checks pass.

Meshy/Tripo usage is quarantine-first:

- Allowed only for explicitly scoped single-prop experiments or approved hero
  props.
- Not allowed for the route, core terrain, forest mass, gameplay-critical
  geometry, camera rail, HUD, or ride loop.
- Every generated asset needs source, date, license/terms note, prompt or
  generation settings when available, import path, dimensions, triangle count,
  material/texture summary, and acceptance verdict.
- Failed generated assets should be documented and removed from active scenes,
  not left as hidden clutter.

A usable real-time asset should normally have:

- plausible meter scale;
- origin/pivot at the base or intentionally documented;
- applied transforms;
- clean bounds;
- stable naming, for example `myb_forest_trunk_base_a`;
- low material count;
- no missing textures or pink materials;
- primitive or simplified colliders;
- no microscopic disconnected fragments;
- no heavy textures unless justified for a hero asset.

## BMAD Workflow

BMAD is file-first. Generate and update BMAD artifacts locally under
`_bmad-output/` first, then sync the approved planning surface to Linear.

After any BMAD workflow that changes planning scope, GDD, architecture, epics,
stories, readiness, implementation evidence, art-direction checkpoints, or
sprint status:

1. Summarize the local artifact changes.
2. Compare them with `_bmad-output/linear-sync.md`.
3. Automatically sync non-destructive Linear updates inside the existing `MYB`
   team and `Echappee 3D - Vertical Slice Mock` project.
4. Update `_bmad-output/linear-sync.md` with returned Linear IDs, URLs, status
   changes, and validation evidence.

Non-destructive Linear sync includes updating existing issue status, existing
issue descriptions, existing project documents, implementation notes,
validation evidence, review verdicts, and BMAD/local artifact references.

Ask for confirmation before destructive or scope-changing Linear operations:
creating new projects, initiatives, epics, or broad backlog; deleting/archiving;
renaming canonical trackers; changing team/project ownership; or expanding
beyond the approved Unity macOS-first roadmap.

## Linear Tracking

Linear team: `MYB` / `MyBike`

Linear project: `Echappee 3D - Vertical Slice Mock`

Current Unity decision tracker:

- `MYB-39` - ADR moteur, Unity/Echapee4D devient la cible active
- `MYB-89` - Spike Unity-MCP IvanMurzak, projet Unity vierge et preuve de demo
- `MYB-90` - Spike Unity WebGL readiness depuis scene Unity-MCP propre
- `MYB-91` - Baseline Unity canonique Echapee4D, hygiene repo, validation et
  WebGL reproductible
- `MYB-92` - Supprimer le projet Unity legacy `unity/Echappee3D` apres la
  baseline canonique
- `MYB-94` - Cadrer la plateforme cible prioritaire: Unity macOS first, WebGL
  secondaire

Current forest-corridor evidence:

- `MYB-114` - Route-first forest corridor art-direction baseline and preview
  progression.
- `MYB-135` to `MYB-140` - Targeted forest richness child tickets covering
  density, variation, vertical volume, ground material, lighting/fog, and strong
  silhouettes.
- `MYB-137` - Vertical volume checkpoint. Do not treat it as final production
  art; it is evidence that the corridor reads better with vertical rhythm.

Potential imported art-rescue tickets:

- `MYB-141` to `MYB-153` may exist if the art-rescue pack has been imported
  into Linear. Do not assume those IDs are active until `_bmad-output/linear-sync.md`
  confirms them.

Historical web MVP tracker:

- `MYB-1` to `MYB-10`, `MYB-18` to `MYB-24`, and `MYB-28` remain completed
  React/Vite/Three.js evidence. Treat them as product/reference history, not as
  the active implementation target.

Prefer Linear issue IDs in branch names, summaries, generated reports, capture
folders, implementation notes, and PR descriptions.

## Git Hosting

Gitea is the active Git hosting and review system for this repository.

- Do not use GitHub for new remotes, pushes, pull requests, reviews, or merges.
- Do not recreate a GitHub remote. If a GitHub remote exists locally, remove it.
- Historical GitHub links in old artifacts remain reference evidence only; they
  are not the active workflow.
- Use Gitea for branch publication, pull requests, review comments, and merge
  operations.
- If no Gitea remote is configured, ask for the canonical Gitea remote URL
  before pushing.
- Linear remains the product tracking system; Git hosting/review happens in
  Gitea.

## Branch And Completion Workflow

- Each Linear ticket must be implemented on a dedicated branch whose name starts
  with the issue ID.
- Keep the branch scoped to that ticket; unrelated dirty work must be left
  untouched unless the ticket explicitly owns it.
- Put implementation artifacts under a ticket-specific folder, for example
  `_bmad-output/implementation-artifacts/myb-137-volume-vertical/` or the
  art-rescue root when the issue explicitly belongs to that pack.
- Merge the ticket branch only when the work is considered complete.
- Move the Linear issue to `Done` in the same completion step as the merge, so
  Git history and Linear status stay aligned.
- For subjective art-direction tickets, prefer `In Review` or equivalent until
  route/overview captures and human visual validation are recorded.

## Engineering Rules

- Keep scope strict to the current Linear issue.
- Preserve mock mode at all times.
- Unity owns runtime, scene, ride loop, HUD, visuals, and macOS delivery for new
  active work.
- WebGL delivery is secondary and should be run when a ticket explicitly targets
  browser proof, regression evidence, or legacy comparison.
- Use IvanMurzak Unity-MCP / `unity-mcp-cli` as the preferred automation path
  for Unity Editor work.
- Keep Unity gameplay logic testable with C# validators or Unity tests where
  practical.
- Do not add heavy dependencies or external services without a clear issue.
- Do not touch `src/**` unless the issue explicitly says to update the parked
  React/Vite/Three.js prototype.
- Do not recreate `unity/Echappee3D/**`; use old BMAD artifacts or git history
  for historical reference.
- Do not scatter generated scripts at random Unity paths. Prefer existing
  ticket-local conventions and keep editor-only builders under `Editor/`.
- Do not leave temporary generated assets, failed imports, or hidden scene junk
  in active folders.
- For existing `MYB` project records, keep Linear synced automatically after
  BMAD implementation/review/status changes and update `_bmad-output/linear-sync.md`.
- Never commit API keys, Linear tokens, Meshy tokens, Tripo tokens, or other
  secrets.

## Expected Validation

For Unity work, start with the narrowest applicable validation:

```bash
unity-mcp-cli status unity/Echapee4D --timeout 10000
```

When applicable, run Unity validators, Editor play/build checks, macOS build
checks, and browser/WebGL capture scripts tied to the current issue.

Recommended validation by work type:

| Work type | Required evidence |
| --- | --- |
| Unity scene/builders | Unity status, no compile errors, generated scene path, route capture, overview capture |
| Visual/art-direction pass | Route capture, overview capture, short visual verdict, known limitations, human review status |
| Asset import | Asset manifest entry, triangle/material/texture checks, scale/pivot/bounds checks, no missing materials |
| Validator work | Validator output under `_bmad-output/unity-test-results/`, plus a short report explaining pass/fail coverage |
| BMAD planning change | Updated local artifact, summary, Linear sync update |
| Parked web prototype only | `npm run typecheck`, `npm run test`, `npm run build` |
| E2E when configured and relevant | `npm run test:e2e` |

For parked web prototype work only, run:

```bash
npm run typecheck
npm run test
npm run build
```

For E2E when configured and relevant:

```bash
npm run test:e2e
```

## Capture And Report Rules

For any visual Unity ticket, produce at least:

- one route-camera capture;
- one overview/debug capture;
- one markdown report with scope, generated assets, metrics, visual verdict,
  validation, and status;
- a clear note saying whether the result is final art, production candidate,
  prototype evidence, or rejected experiment.

For forest-corridor tickets, put captures and reports in a ticket-specific
folder under `_bmad-output/implementation-artifacts/`, unless the current issue
explicitly says to use the art-rescue import root.

Do not close an art-direction ticket with only an overview screenshot. The ride
camera is the product view.

For Art Rescue visual work, `Done` is allowed only when the route capture reaches
`Premium target` or when Julien accepts a documented exception. A visible
improvement below that bar must be reported as `Checkpoint insuffisant` with the
rubric score, route/overview captures, the primary blocker, and the next action.

The route capture must come from the canonical ride camera in the canonical ride
corridor. The overview capture is required for global context and density, but it
is secondary: overview explains, route decides. Do not use a sandbox, quarantine
scene, turntable, Blender render, Meshy/Tripo preview, isolated asset screenshot,
or flattering hero shot as final `Premium target` evidence.

For visual/performance trade-offs, protect the route-camera `Premium target`
before overview beauty, isolated asset beauty, or theoretical optimization.
Performance failures require measured diagnosis before downgrading visible
premium elements: observed FPS or relevant metric, validator result, suspected
asset/category, visual before/after when applicable, downgrade rationale, and at
least one considered alternative.

## Codex / MCP Operating Notes

Before editing, agents should read:

1. the current Linear issue or local ticket markdown;
2. this `AGENTS.md` file;
3. `_bmad-output/linear-sync.md`;
4. relevant BMAD planning docs;
5. relevant implementation reports and captures;
6. relevant art-rescue docs if working on forest corridor visuals.

When using Unity MCP:

- check editor/project status before making scene changes;
- prefer scripted deterministic builders over manual, unrepeatable scene edits;
- save scenes and generated assets in predictable ticket-owned paths;
- record validation evidence after generation.

When using Blender MCP:

- generate small modular Unity-ready assets;
- apply transforms;
- use meter scale;
- keep pivots useful;
- export individual assets with stable names;
- write or update a manifest with dimensions, triangle counts, materials, and
  intended usage.

When using Meshy/Tripo:

- only proceed if the current issue explicitly authorizes it;
- create one prop at a time;
- document source/generation settings and license/terms status;
- validate in Unity before promotion;
- remove failed experiments from active scenes.

<!-- lean-ctx -->
## lean-ctx

Prefer lean-ctx MCP tools over native equivalents for token savings.
Full rules: @LEAN-CTX.md
<!-- /lean-ctx -->

# AGENTS.md - mybike / Echappee 3D

## Project Context

mybike is a Unity WebGL-first first-person scenic cycling game, currently
scoped to the Echappee 3D vertical slice.

As of 2026-06-11, the active engine direction is Unity, based on the
IvanMurzak Unity-MCP probe and WebGL readiness evidence from `MYB-89` and
`MYB-90`.

Canonical Unity project:

- `unity/Echapee4D`

Removed or reference-only legacy areas:

- `unity/Echappee3D` was removed by `MYB-92`; references to it in old BMAD
  artifacts are historical evidence only.
- `src/**` is the historical React/Vite/Three.js prototype. Do not extend it
  unless a Linear issue explicitly says to touch the parked web prototype.

The MVP must be playable entirely in mock mode, without connected-bike
hardware. BLE, Web Bluetooth, FTMS, real-bike telemetry, Meshy, heavy asset
pipelines, rich history, multiple routes, and post-MVP backlog work are out of
scope unless a Linear issue explicitly says otherwise.

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

## BMAD Workflow

BMAD is file-first. Generate and update BMAD artifacts locally under
`_bmad-output/` first, then sync the approved planning surface to Linear.

After any BMAD workflow that changes planning scope, GDD, architecture, epics,
stories, readiness, or sprint status:

1. Summarize the local artifact changes.
2. Compare them with `_bmad-output/linear-sync.md`.
3. Automatically sync non-destructive Linear updates inside the existing
   `MYB` team and `Echappee 3D - Vertical Slice Mock` project.
4. Update `_bmad-output/linear-sync.md` with returned Linear IDs, URLs, status
   changes, and validation evidence.

Non-destructive Linear sync includes updating existing issue status, existing
issue descriptions, existing project documents, implementation notes,
validation evidence, review verdicts, and BMAD/local artifact references.

Ask for confirmation before destructive or scope-changing Linear operations:
creating new projects, initiatives, epics, or broad backlog; deleting/archiving;
renaming canonical trackers; changing team/project ownership; or expanding
beyond the approved Unity-first roadmap.

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

Next active implementation issue:

- `MYB-91` and `MYB-92` are the baseline/cleanup gate before gameplay, assets,
  HUD, CI, or performance work.
- Hardware/BLE/FTMS tickets are post-baseline backlog unless a Linear issue
  explicitly pulls them back into scope.

Historical web MVP tracker:

- `MYB-1` to `MYB-10`, `MYB-18` to `MYB-24`, and `MYB-28` remain completed
  React/Vite/Three.js evidence. Treat them as product/reference history, not as
  the active implementation target.

Prefer Linear issue IDs in branch names, summaries, and implementation notes.

## Branch And Completion Workflow

- Each Linear ticket must be implemented on a dedicated branch whose name starts
  with the issue ID.
- Keep the branch scoped to that ticket; unrelated dirty work must be left
  untouched unless the ticket explicitly owns it.
- Merge the ticket branch only when the work is considered complete.
- Move the Linear issue to `Done` in the same completion step as the merge, so
  Git history and Linear status stay aligned.

## Engineering Rules

- Keep scope strict to the current Linear issue.
- Preserve mock mode at all times.
- Unity owns runtime, scene, ride loop, HUD, visuals, and WebGL delivery for
  new active work.
- Use IvanMurzak Unity-MCP / `unity-mcp-cli` as the preferred automation path
  for Unity Editor work.
- Keep Unity gameplay logic testable with C# validators or Unity tests where
  practical.
- Do not add heavy dependencies or external services without a clear issue.
- Do not touch `src/**` unless the issue explicitly says to update the parked
  React/Vite/Three.js prototype.
- Do not recreate `unity/Echappee3D/**`; use old BMAD artifacts or git history
  for historical reference.
- For existing `MYB` project records, keep Linear synced automatically after
  BMAD implementation/review/status changes and update `_bmad-output/linear-sync.md`.
- Never commit API keys, Linear tokens, Meshy tokens, or other secrets.

## Expected Validation

For Unity work, prefer the narrowest applicable validation:

```bash
unity-mcp-cli status unity/Echapee4D --timeout 10000
```

When applicable, run Unity validators, Editor play/build checks, WebGL build
checks, and browser capture scripts tied to the current issue.

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

<!-- lean-ctx -->
## lean-ctx

Prefer lean-ctx MCP tools over native equivalents for token savings.
Full rules: @LEAN-CTX.md
<!-- /lean-ctx -->

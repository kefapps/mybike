# AGENTS.md - mybike / Echappee 3D

## Project Context

mybike is a web 3D first-person scenic cycling game, currently scoped to
the Echappee 3D vertical slice.

The MVP must be playable entirely in mock mode, without connected-bike
hardware. BLE, Web Bluetooth, FTMS, real-bike telemetry, Meshy, heavy asset
pipelines, rich history, multiple routes, and post-MVP backlog work are out of
scope unless a Linear issue explicitly says otherwise.

## Active Planning Sources

Use these files as the planning source of truth:

- `_bmad-output/planning-artifacts/echappee-3d-gdd-court.md`
- `_bmad-output/planning-artifacts/echappee-3d-architecture-mince.md`
- `_bmad-output/planning-artifacts/echappee-3d-mvp-epic-stories.md`
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
issue descriptions, existing project documents, implementation notes, validation
evidence, review verdicts, and BMAD/local artifact references.

Ask for confirmation before destructive or scope-changing Linear operations:
creating new projects, initiatives, epics, or broad backlog; deleting/archiving;
renaming canonical trackers; changing team/project ownership; or expanding beyond
the current one MVP epic and five vertical-slice stories.

## Linear Tracking

Linear team: `MYB` / `MyBike`

Linear project: `Echappee 3D - Vertical Slice Mock`

Current tracker:

- `MYB-1` - Epic 1, boucle de ride mock jouable
- `MYB-2` - Story 1.1, app shell MVP et etats de session
- `MYB-3` - Story 1.2, source mock, vitesse lissee, progression et stats
- `MYB-4` - Story 1.3, route prefabriquee, biomes et camera sur rail
- `MYB-5` - Story 1.4, scene Three.js MVP et rendu de la balade
- `MYB-6` - Story 1.5, HUD, controles mock, resume et happy path

Prefer Linear issue IDs in branch names, summaries, and implementation notes.

## Engineering Rules

- Keep scope strict to the current Linear issue.
- Preserve mock mode at all times.
- Keep ride logic pure and testable outside React and Three.js.
- React owns UI and screen state; Three.js owns rendering from snapshots.
- Do not add heavy dependencies or external services without a clear issue.
- Run relevant validation commands when they exist; otherwise explain why not.
- For existing `MYB` project records, keep Linear synced automatically after
  BMAD implementation/review/status changes and update `_bmad-output/linear-sync.md`.
- Never commit API keys, Linear tokens, Meshy tokens, or other secrets.

## Expected Validation

When applicable:

```bash
npm run typecheck
npm run test
npm run build
```

For E2E when configured:

```bash
npm run test:e2e
```

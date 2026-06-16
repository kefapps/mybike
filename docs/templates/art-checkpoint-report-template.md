# <TICKET> Capture Report

Ticket:
-

Generated at:
-

Output directory:
`_bmad-output/visual-checkpoints/<ticket-id>/`

Metadata:
`_bmad-output/visual-checkpoints/<ticket-id>/<timestamp>-capture-metadata.json`

Mode:
- current
- before
- after
- explicit before/after comparison

## Scene

Scene:
-

Branch:
-

Commit:
-

## Cameras

| Camera | Role | Found | Position | Rotation | FOV/Size | Notes |
|---|---|---:|---|---|---:|---|
| RouteCamera | blocking validation |  |  |  |  |  |
| OverviewCamera | secondary context |  |  |  |  |  |

## Captures

| State | Type | Path | Scene | Camera | Resolution |
|---|---|---|---|---|---|
| before | route |  |  | RouteCamera |  |
| before | overview |  |  | OverviewCamera |  |
| after | route |  |  | RouteCamera |  |
| after | overview |  |  | OverviewCamera |  |

## Comparisons

| Type | Before | After | Sheet |
|---|---|---|---|
| route |  |  |  |
| overview |  |  |  |

## Explicit Baseline

Before selected by:
-

Reason:
-

Source:
-

## Visual Rubric Link

Rubric:
`docs/validation/forest-corridor-shot-rubric.md`

This report provides comparable evidence. It does not itself declare
`Premium target`.

## Verdict

- PASS
- PASS_WITH_WARNINGS
- FAIL

## Warnings

-

## Errors

-

## Review Status

RouteCamera is the blocking visual evidence. OverviewCamera is required for
context but secondary.

Human visual validation:
-

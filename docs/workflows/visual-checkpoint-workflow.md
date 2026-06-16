# Visual Checkpoint Workflow

Status: Canonical MYB-145 workflow for comparable Art Rescue visual evidence.

Scope: forest corridor visual checkpoint captures and before/after comparison
reports.

MYB-145 standardizes what future visual tickets must show. It does not judge
`Premium target`, replace the MYB-142 rubric, or replace human visual review.

## Canonical Output

Comparable visual evidence lives under:

`_bmad-output/visual-checkpoints/<ticket-id>/`

Use this folder for:

- route screenshots;
- overview screenshots;
- route before/after sheets;
- overview before/after sheets;
- capture reports;
- capture metadata JSON;
- explicit baseline notes.

`_bmad-output/implementation-artifacts/` remains valid for general
implementation artifacts, imported docs, historical reports, and non-visual
ticket evidence. For standardized MYB-145 and MYB-147+ visual proof,
`visual-checkpoints` wins.

## Canonical Cameras

MYB-145 defines two checkpoint cameras:

| Camera | Role | Validation meaning |
|---|---|---|
| `RouteCamera` | Blocking validation surface | Route-camera proof for future Art Rescue visual reviews. |
| `OverviewCamera` | Secondary context | Global placement, density, and scene organization context. |

Rule:

`Overview explains. Route decides.`

## Unity Entry Points

The V1 helper lives at:

`unity/Echapee4D/Assets/MYB145/Editor/MYB145CaptureRigHelper.cs`

Menus:

- `Tools/MyBike/Capture/MYB-145 Setup Capture Cameras`
- `Tools/MyBike/Capture/MYB-145 Validate Capture Cameras`
- `Tools/MyBike/Capture/MYB-145 Capture Route + Overview`

Batch methods:

- `MYB145CaptureRigHelper.RunBatchValidate`
- `MYB145CaptureRigHelper.RunBatchCapture`

Optional batch setup helper:

- `MYB145CaptureRigHelper.RunBatchSetup`

Example-only batch helper:

- `MYB145CaptureRigHelper.RunBatchExample`

`RunBatchExample` performs explicit setup and then captures in the same Unity
session so MYB-145 can produce an example without saving scene changes. Future
visual tickets should prefer `RunBatchValidate` and `RunBatchCapture` after
their checkpoint cameras are already explicit.

Batch arguments:

```txt
-myb145Ticket <ticket-id>
-myb145State current|before|after
-myb145Scene <Assets/.../*.unity>
-myb145BeforeRoute <path>
-myb145AfterRoute <path>
-myb145BeforeOverview <path>
-myb145AfterOverview <path>
-myb145BaselineSelectedBy <name>
-myb145BaselineReason <reason>
-myb145BaselineSource <source>
```

## Modes

### Setup / Normalize

Setup mode may create or normalize `RouteCamera` and `OverviewCamera`.

Allowed:

- create missing checkpoint cameras;
- normalize camera names;
- normalize camera transforms and capture parameters;
- mark the scene dirty;
- report what changed.

Forbidden:

- lighting changes;
- fog changes;
- material changes;
- asset changes;
- scatter changes;
- route or gameplay changes;
- automatic visual polish.

Setup mode must not silently save the scene. Save explicitly if the rig should
persist.

### Validate

Validate mode checks that `RouteCamera` and `OverviewCamera` exist and are
usable.

It does not create cameras.

### Capture

Capture mode is read-only.

It must:

- find `RouteCamera`;
- find `OverviewCamera`;
- capture route and overview PNGs;
- write a Markdown report;
- write a metadata JSON file;
- generate explicit before/after sheets when paths are provided.

It must not:

- create missing cameras;
- normalize cameras;
- choose a baseline automatically;
- modify the scene.

## Capture States

Supported states:

- `current`;
- `before`;
- `after`.

`current` is useful for a checkpoint without comparison.

`before` and `after` are explicit states. MYB-145 V1 must not choose the
baseline automatically.

## Explicit Before / After

Before/after comparison requires explicit paths.

Minimum:

- ticket id;
- capture type: route or overview;
- before path;
- after path;
- baseline reason.

Forbidden in V1:

- latest capture lookup;
- automatic baseline selection;
- filename-only baseline inference;
- mixing route and overview without an error;
- comparison without an `Explicit Baseline` section.

Automation may suggest a baseline in a future version. It must not silently
choose one.

## File Naming

Simple captures:

```txt
_bmad-output/visual-checkpoints/<ticket-id>/<timestamp>-current-route.png
_bmad-output/visual-checkpoints/<ticket-id>/<timestamp>-current-overview.png
_bmad-output/visual-checkpoints/<ticket-id>/<timestamp>-before-route.png
_bmad-output/visual-checkpoints/<ticket-id>/<timestamp>-before-overview.png
_bmad-output/visual-checkpoints/<ticket-id>/<timestamp>-after-route.png
_bmad-output/visual-checkpoints/<ticket-id>/<timestamp>-after-overview.png
```

Comparisons:

```txt
_bmad-output/visual-checkpoints/<ticket-id>/<timestamp>-route-before-after.png
_bmad-output/visual-checkpoints/<ticket-id>/<timestamp>-overview-before-after.png
```

Reports:

```txt
_bmad-output/visual-checkpoints/<ticket-id>/<timestamp>-capture-report.md
_bmad-output/visual-checkpoints/<ticket-id>/<timestamp>-capture-metadata.json
```

## Severity

### ERROR

Use `ERROR` when:

- a route capture is requested and `RouteCamera` is missing;
- an overview capture is requested and `OverviewCamera` is missing;
- multiple canonical cameras exist;
- a comparison is requested without explicit before and after paths;
- an explicit baseline reason is missing;
- a capture file cannot be read or written;
- the canonical output directory cannot be created;
- capture state is not `current`, `before`, or `after`.

### WARNING

Use `WARNING` when:

- setup changed cameras and marked the scene dirty;
- camera settings differ from MYB-145 V1 defaults;
- metadata is incomplete;
- before/after image resolutions differ;
- historical baseline paths are referenced from outside `visual-checkpoints`;
- `current` capture is produced without a baseline.

### INFO

Use `INFO` when:

- setup created or normalized cameras;
- cameras are found and valid;
- a capture is written;
- a comparison sheet is written;
- comparison was not requested.

## Batch Behavior

Batch validate/capture exits with:

- `1` when `ERROR >= 1`;
- `0` when there are no errors.

Warnings do not fail batch in V1.

MYB-145 batch does not judge `Premium target`.

## Future Ticket Usage

Future visual Art Rescue tickets should include this evidence in Linear:

```md
## Capture Evidence

State:
- before / after / current

Before:
-

After:
-

Route comparison:
-

Overview comparison:
-

Baseline reason:
-
```

Use the MYB-142 shot rubric after the MYB-145 evidence exists.

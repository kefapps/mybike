# MYB-147 Governance Review

Status: strict rework self-review.

Reviewed on: 2026-06-16

Reviewed branch:
- `MYB-147-blender-procedural-forest-kit-v0`

Reviewed PR:
- `http://localhost:3000/kefapps/mybike/pulls/10`

## Human Review Input

Human verdict:
- Accept as candidate kit v0 with warnings

Human decision:
- Keep `MYB_ForestKit_V0` as a candidate kit v0.
- Accept it for MYB-148 / MYB-150 route-camera testing.
- Do not keep polishing Blender assets unless required by validator `ERROR`.
- Do not move MYB-147 to `Done`; final status remains `In Review`.

Human warnings called out:
- canopy masses still read as generic green blobs;
- rocks remain usable but generic;
- trunk material language is still simple;
- root clusters may need better ground integration during scatter;
- isolated preview does not validate route-camera quality.

## Rework Evidence

Implementation report:
- `_bmad-output/implementation-artifacts/MYB-147/myb-147-implementation-report.md`

Local kit manifests:
- `_bmad-output/implementation-artifacts/MYB-147/myb-forest-kit-v0-manifest.md`
- `_bmad-output/implementation-artifacts/MYB-147/myb-forest-kit-v0-manifest.json`

Canonical manifest:
- `docs/manifests/art-rescue-asset-manifest.json`

Unity candidate assets:
- `unity/Echapee4D/Assets/Echappee/Art/Candidates/MYB_ForestKit_V0/`

Preview evidence:
- `_bmad-output/visual-checkpoints/MYB-147/2026-06-16T14-16-29Z-kit-contact-sheet.png`
- `_bmad-output/visual-checkpoints/MYB-147/2026-06-16T14-16-29Z-capture-report.md`

MYB-144 validator report:
- `_bmad-output/unity-test-results/myb-144-art-asset-validator-report.md`

Review page:
- `_bmad-output/visual-checkpoints/MYB-147/myb-147-review.html`

## Scope Review

| Check | Result | Evidence |
|---|---|---|
| Kit remains reusable assets, not complete decor | PASS | 21 standalone FBX candidates remain in `MYB_ForestKit_V0`. |
| No scatter in canonical scene | PASS | No MYB-147 scene scatter was produced. |
| No final complete scene created | PASS | No MYB-147 `.unity` scene is part of the ticket work. |
| No asset promoted to production | PASS | 0 MYB-147 assets have `promotionStatus: promoted`. |
| No Meshy / Tripo / text-to-3D | PASS | Rework used the local procedural Blender script; no external generator or API source. |
| No complex shader introduced | PASS | No shader/shadergraph/compute asset added. |
| Ticket remains In Review, not Done | PASS | Human decision accepts candidate v0 but keeps final status `In Review`. |

## Rework Checklist

| Priority | Requirement | Result | Evidence |
|---|---|---|---|
| P0 | Rework 4 trunks away from pole read | PASS | Wider bases, buttress roots, asymmetry, bark ridges, knots and broken silhouette language added. |
| P0 | Push `trunk_knotted_a` language across trunk family | PASS | All trunks now share stronger base/root/ridge language. |
| P0 | Rework root clusters away from plank-like shapes | PASS | Root clusters now use multi-segment tapered curves, broader spread and better ground contact. |
| P0 | Make `root_arch_a` a clearer fantasy scenic signal | PASS | `root_arch_a` now has a larger arch, secondary root and threshold moss. |
| P1 | Improve fern silhouettes | PASS | Ferns are taller and broader with two stems and larger frond planes. |
| P1 | Improve leaf/moss mats | PASS | Mats now have organic outlines, height variation, moss patches and a ridge detail. |
| P1 | Rework canopy masses away from generic blobs | PASS | Canopy masses now use varied lobes plus leaf shelf planes. |
| P2 | Improve mossy rocks / marker rock | PASS | Rocks have stronger distortion, side moss and marker lip detail. |

## Kit Content Review

| Required family | Required | Found | Result |
|---|---:|---:|---|
| Irregular trunks | 4 | 4 | PASS |
| Root clusters | 3 | 3 | PASS |
| Mossy rocks / stones | 3 | 3 | PASS |
| Ferns / vegetation clumps | 3 | 3 | PASS |
| Leaf / moss mats | 3 | 3 | PASS |
| Dead branches | 2 | 2 | PASS |
| Stylized canopy masses | 2 | 2 | PASS |
| Fallen log | 1 | 1 | PASS |

## Asset Quality Review

| Check | Result | Evidence |
|---|---|---|
| Stable names `myb_forest_[family]_[variant]` | PASS | Local manifest ids keep stable `myb_forest_` names. |
| Meter-scale units documented | PASS | Local manifest dimensions are in meters. |
| Transforms applied | PASS | Generator applies transforms before export. |
| Ground pivots documented | PASS | Every entry includes a pivot/origin note. |
| Bounds documented | PASS | Every entry includes dimensions and bounds note. |
| Max 2 materials per current asset | PASS | Maximum material count is 2. |
| Triangle budgets respected | PASS | Maximum triangle count is 852 / 900 on `myb_forest_root_arch_a`. |
| No microscopic detail dependency | PASS | Geometry stays simple, silhouette-led and texture-free. |

Asset metrics:
- Asset count: 21
- Total triangles: 6704
- Maximum triangle count: 852
- Maximum material count: 2
- Assets over V0 triangle budget: 0
- Assets over 2 materials: 0

## Manifest Review

| Check | Result | Evidence |
|---|---|---|
| Canonical manifest valid JSON | PASS | `docs/manifests/art-rescue-asset-manifest.json` parses. |
| Manifest root has `schemaVersion`, `updatedAt`, `assets` | PASS | `schemaVersion: 1`, `updatedAt: 2026-06-16`, `assets` array. |
| Unique MYB-147 ids | PASS | 21 stable MYB-147 ids. |
| No `reviewStatus` | PASS | No MYB-147 entry uses `reviewStatus`. |
| No `example:true` | PASS | No MYB-147 entry uses `example:true`. |
| `intakeStatus: approved` | PASS | 21 / 21 MYB-147 entries are `approved`. |
| `promotionStatus: candidate` | PASS | 21 / 21 MYB-147 entries are `candidate`. |
| No `promotionStatus: promoted` | PASS | 0 MYB-147 entries are `promoted`. |
| `sourceType: internal` | PASS | 21 / 21 MYB-147 entries use the accepted internal/project-owned source type with provider `Blender MCP / procedural`. |
| All `assetPaths` exist | PASS | MYB-144 resolves the manifest paths without MYB-147 path errors. |

## MYB-144 Validator Review

| Check | Result | Evidence |
|---|---|---|
| MYB-144 executed | PASS | Batch report exists and reports batch mode. |
| MYB-144 has no `ERROR` | PASS | Summary shows `Errors: 0`. |
| Warnings listed and justified | PASS_WITH_WARNINGS | 211 warnings are existing scanned-root manifest coverage warnings. |
| No MYB-147 kit warning/error | PASS | No report lines reference `MYB_ForestKit_V0` or `myb_forest_` as a problem. |

## Visual Evidence Review

| Check | Result | Evidence |
|---|---|---|
| Preview contact sheet updated | PASS | `2026-06-16T14-16-29Z-kit-contact-sheet.png`. |
| Preview report updated | PASS | `2026-06-16T14-16-29Z-capture-report.md`. |
| Preview is marked intermediate only | PASS | Capture report says not `Premium target` evidence. |
| Route-camera validation deferred | PASS | Capture and implementation reports defer to MYB-148 / MYB-150 / MYB-151. |

## Human Decision Status

| Check | Result | Evidence |
|---|---|---|
| Julien inspected first contact sheet | PASS | Human Visual Review - MYB-147 was provided. |
| Julien inspected revised contact sheet | PASS | Human review accepted candidate kit v0 with warnings. |
| Julien accepts revised kit as candidate for MYB-148/MYB-150 | PASS | Accepted for MYB-148 / MYB-150 route-camera testing. |
| Ticket remains In Review | PASS | Candidate acceptance does not close MYB-147 as `Done`. |

## Findings

No blocking governance failure was found after the rework.

Warnings to keep visible:
- The validator result is `PASS_WITH_WARNINGS` because of 211 existing scanned-root manifest warnings outside the MYB-147 kit.
- The revised kit is still isolated preview evidence only.
- The revised kit cannot be treated as `Premium target` evidence.
- MYB-147 remains `In Review`, not `Done`.

## Required Corrections

None unless MYB-144 reports a validator `ERROR`.

Do not keep polishing the Blender assets for MYB-147. Route-camera validation and integration issues move to MYB-148 / MYB-150.

## Final Verdict

PASS_WITH_WARNINGS

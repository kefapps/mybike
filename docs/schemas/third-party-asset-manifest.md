# Third-Party / AI Asset Manifest Schema

Status: Canonical schema documentation for MYB-143.

Canonical manifest data:
`docs/manifests/art-rescue-asset-manifest.json`

Scope: Art Rescue asset candidates that may be promoted into the Unity forest
corridor, including third-party assets, AI-generated assets, Blender MCP assets,
in-house authored assets, and controlled Unity procedural candidates.

This document keeps the historical MYB-143 title "third-party / AI" but the
manifest gate applies to every Art Rescue asset candidate. Source-specific
evidence differs by source type; production promotion does not.

## Root Object

The real manifest must be a versioned object, not a flat list.

```json
{
  "schemaVersion": 1,
  "updatedAt": "2026-06-16",
  "assets": []
}
```

| Field | Type | Required | Rule |
|---|---|---|---|
| `schemaVersion` | number | yes | Positive integer. Starts at `1`. |
| `updatedAt` | string | yes | `YYYY-MM-DD`; update only when real manifest data changes. |
| `assets` | array | yes | Real asset entries only. May be empty. |

The real production manifest must not contain documentation examples. If an
example appears in docs or fixtures, mark it with `"example": true`. The real
manifest must reject `example: true`.

## Asset Entry

Every real asset entry must answer two separate questions:

- `intakeStatus`: do we trust the source, license, provenance, and intake
  review?
- `promotionStatus`: is the asset allowed to be used as production content?

`approved` intake is not production promotion.

```json
{
  "id": "asset_unique_stable_id",
  "name": "Human readable asset name",
  "sourceType": "third_party",
  "provider": "Provider or marketplace name",
  "sourceUrl": "https://source.example",
  "license": "License identifier or license name",
  "licenseUrl": "https://license.example",
  "author": "Author or studio name",
  "acquiredAt": "2026-06-16",
  "intakeStatus": "review",
  "promotionStatus": "not_promoted",
  "usageScope": "forest_corridor",
  "assetPaths": [],
  "derivedFrom": [],
  "aiGenerated": false,
  "requiresAttribution": false,
  "attributionText": "",
  "visualImpact": "visible",
  "routeEvidence": [],
  "overviewEvidence": [],
  "validatorEvidence": [],
  "notes": ""
}
```

## Required Fields

| Field | Type | Required | Rule |
|---|---|---|---|
| `id` | string | yes | Stable unique machine-readable id. |
| `name` | string | yes | Human-readable name. |
| `sourceType` | string | yes | One allowed source type. |
| `provider` | string | yes | Provider, tool, marketplace, or authoring source. |
| `sourceUrl` | string | external only | Source URL or provider job id URL when applicable. |
| `license` | string | yes | License or usage terms summary. |
| `licenseUrl` | string | recommended | Link to license or terms when available. |
| `author` | string | recommended | Author, studio, or generation source. |
| `acquiredAt` | string | yes | `YYYY-MM-DD`. |
| `intakeStatus` | string | yes | Intake state. |
| `promotionStatus` | string | yes | Production promotion state. |
| `usageScope` | string | yes | Where the asset may be used. |
| `assetPaths` | array | yes | Unity-relative or repo-relative paths. |
| `derivedFrom` | array | yes | Parent source ids or URLs. May be empty. |
| `aiGenerated` | boolean | yes | True for AI-generated or heavily AI-transformed assets. |
| `requiresAttribution` | boolean | yes | Whether attribution is required. |
| `attributionText` | string | required if attribution | Attribution text to preserve. |
| `visualImpact` | string | yes | `visible`, `technical`, or `none`. |
| `routeEvidence` | array | yes | Route-camera evidence references. May be empty until promotion. |
| `overviewEvidence` | array | yes | Overview evidence references. |
| `validatorEvidence` | array | yes | Validator report references. |
| `notes` | string | no | Review notes, warnings, exceptions, or constraints. |

## Source Types

Allowed `sourceType` values:

- `third_party`
- `ai_generated`
- `ai_assisted`
- `internal`
- `blender_mcp`
- `in_house_authored`
- `unity_builtin_or_procedural`
- `derived`
- `unknown`

`unknown` is allowed only before production promotion. It must not appear on a
`candidate` or `promoted` asset.

`internal` is for project-owned, non-external assets authored or generated
inside the project pipeline, including procedural Blender kit assets. It can use
`provider` to record the tool path, for example `Blender MCP / procedural`.

## Intake Status

Allowed `intakeStatus` values:

- `quarantine`
- `review`
- `approved`
- `rejected`
- `deprecated`

`quarantine` means raw, incomplete, untrusted, or not yet reviewed.

`review` means provenance, license, technical quality, or visual fit is being
evaluated.

`approved` means the intake record is trusted enough for candidate review. It
does not mean production promotion.

`rejected` means the asset must not be used in production.

`deprecated` means the asset should not be used for new work and existing usage
should be documented or migrated.

## Promotion Status

Allowed `promotionStatus` values:

- `not_promoted`
- `candidate`
- `promoted`

Allowed combinations:

| `intakeStatus` | Allowed `promotionStatus` |
|---|---|
| `quarantine` | `not_promoted` |
| `review` | `not_promoted` |
| `approved` | `not_promoted`, `candidate`, `promoted` |
| `rejected` | `not_promoted` |
| `deprecated` | `not_promoted` |

`candidate` and `promoted` require `intakeStatus: approved`.

`promoted` requires:

- provenance and license accepted;
- clear `usageScope`;
- valid `assetPaths`;
- validator evidence with no blocking `ERROR`, or documented warnings;
- route-camera evidence for visible assets;
- overview evidence for visible assets;
- human validation when the visual judgment is subjective.

Invisible or purely technical assets may omit route-camera evidence only when
`visualImpact` is `none` and the exception is documented in `notes`.

## Usage Scope

Allowed initial `usageScope` values:

- `forest_corridor`
- `art_rescue`
- `prototype_only`
- `editor_only`
- `reference_only`
- `quarantine_only`
- `global`

`prototype_only`, `reference_only`, and `quarantine_only` must not be treated as
production-ready scopes.

## Unity Asset Zones

The canonical Unity project root is `unity/Echapee4D`. Manifest paths should use
Unity-relative `Assets/...` paths.

Canonical Art Rescue zones:

- `Assets/Echappee/Art/Quarantine/...`
- `Assets/Echappee/Art/Review/...`
- `Assets/Echappee/Art/Production/...`

Rules:

- AI-generated assets enter `Quarantine` first.
- Cleaned candidates may move to `Review`.
- Only `promotionStatus: promoted` assets may live in `Production`.
- Visible assets in `Production` require route-camera evidence or a documented
  visual-surface exception.

## Deprecated Field Mapping

`reviewStatus` is not a canonical manifest field.

If older notes, drafts, or scripts use `reviewStatus`, map them as follows:

| Old value | New `intakeStatus` | New `promotionStatus` |
|---|---|---|
| `pending` | `quarantine` | `not_promoted` |
| `needs_review` | `review` | `not_promoted` |
| `quarantine` | `quarantine` | `not_promoted` |
| `approved` | `approved` | `not_promoted` by default |
| `rejected` | `rejected` | `not_promoted` |
| `deprecated` | `deprecated` | `not_promoted` |

Do not migrate old `approved` to `promoted` without explicit production evidence.

`reviewStatus` in the real manifest is an `ERROR` for MYB-144.

## AI Rules

For Meshy, Tripo, or equivalent AI-generated assets:

- default `sourceType`: `ai_generated`;
- default `aiGenerated`: `true`;
- default `intakeStatus`: `quarantine`;
- default `promotionStatus`: `not_promoted`;
- required evidence includes provider, source URL or job id, acquisition date,
  license or terms note, prompt summary in `notes` or source metadata, local
  paths, and review decision.

Meshy/Tripo output is source evidence, not production validation. It must not
carry route, core terrain, forest mass, gameplay-critical geometry, camera rail,
HUD, or the ride loop.

## Documentation Example Only

This example is documentation-only. Do not copy it into the real manifest.

```json
{
  "id": "example_kenney_nature_tree_01",
  "example": true,
  "name": "Example Tree Asset",
  "sourceType": "third_party",
  "provider": "Kenney",
  "sourceUrl": "https://example.invalid/source",
  "license": "CC0-1.0",
  "licenseUrl": "https://example.invalid/license",
  "author": "Example Author",
  "acquiredAt": "2026-06-16",
  "intakeStatus": "approved",
  "promotionStatus": "not_promoted",
  "usageScope": "forest_corridor",
  "assetPaths": [
    "Assets/Echappee/Art/Review/Example/example_tree.fbx"
  ],
  "derivedFrom": [],
  "aiGenerated": false,
  "requiresAttribution": false,
  "attributionText": "",
  "visualImpact": "visible",
  "routeEvidence": [],
  "overviewEvidence": [],
  "validatorEvidence": [],
  "notes": "Documentation-only example. Intake approved is not production promotion."
}
```

## MYB-144 Validator Requirements

MYB-144 must be able to validate:

- manifest root is an object, not a list;
- `schemaVersion` exists and is supported;
- `updatedAt` exists and uses `YYYY-MM-DD`;
- `assets` exists and is an array;
- no real asset has `example: true`;
- no real asset uses `reviewStatus`;
- each asset has a unique `id`;
- each asset has allowed `sourceType`, `intakeStatus`, `promotionStatus`, and
  `usageScope`;
- `candidate` and `promoted` require `intakeStatus: approved`;
- `quarantine`, `review`, `rejected`, and `deprecated` require
  `promotionStatus: not_promoted`;
- `promoted` assets have non-empty `assetPaths`;
- `promoted` assets do not use `sourceType: unknown`;
- `promoted` assets have non-empty `license`;
- visible promoted assets have route and overview evidence or a documented
  exception;
- AI promoted assets have explicit provenance and review notes.

The canonical Unity validator behavior, scan roots, severity model, thresholds,
batch behavior, and report format are defined in:

`docs/validators/unity-art-asset-validator-spec.md`

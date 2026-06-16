# Meshy / Tripo Quarantine Workflow

Status: Canonical MYB-143 workflow for AI and external asset intake.

Canonical manifest:
`docs/manifests/art-rescue-asset-manifest.json`

Schema documentation:
`docs/schemas/third-party-asset-manifest.md`

This workflow is doc-only governance. It does not authorize generating,
importing, or promoting assets by itself.

## Core Rule

No third-party, AI-generated, Blender MCP, in-house authored, or procedural Art
Rescue asset candidate may be promoted into production without a manifest entry.

For visible assets:

```txt
manifest intake + validators is not enough.
Route-camera evidence decides production promotion.
```

## Intake Stages

### 1. Source Decision

Before acquiring or generating an asset, record the intended source type:

- `third_party`
- `ai_generated`
- `ai_assisted`
- `blender_mcp`
- `in_house_authored`
- `unity_builtin_or_procedural`
- `derived`

Meshy/Tripo are allowed only when the current Linear ticket explicitly
authorizes that pipeline.

Meshy/Tripo must not fabricate the main route, core terrain, forest mass,
gameplay-critical geometry, camera rail, HUD, or ride loop.

### 2. Quarantine

All AI-generated output starts as:

```json
{
  "sourceType": "ai_generated",
  "aiGenerated": true,
  "intakeStatus": "quarantine",
  "promotionStatus": "not_promoted"
}
```

Raw candidates belong under:

```txt
Assets/Echappee/Art/Quarantine/...
```

Quarantine assets must not be placed directly in production scenes.

### 3. Manifest Entry

Create or update an entry in:

```txt
docs/manifests/art-rescue-asset-manifest.json
```

Required evidence includes:

- stable `id`;
- human-readable `name`;
- `sourceType`;
- provider/tool;
- source URL or provider job id when applicable;
- license or terms note;
- acquisition date;
- `usageScope`;
- Unity-relative `assetPaths`;
- `derivedFrom`;
- `aiGenerated`;
- attribution requirement;
- `intakeStatus`;
- `promotionStatus`;
- technical and visual evidence references when available.

The real manifest must not contain documentation examples or `example: true`.

### 4. Blender Cleanup

AI and external candidates must be cleaned before production consideration.

Expected cleanup checks:

- applied transforms;
- useful pivot/origin;
- plausible meter scale;
- clean bounds;
- no microscopic fragments;
- controlled triangle count;
- controlled material count;
- textures reduced or documented;
- no missing or pink materials;
- simplified or primitive colliders when needed.

Cleanup output may move to:

```txt
Assets/Echappee/Art/Review/...
```

Only cleaned candidates should be used in preview or candidate scenes.

### 5. Unity Validation

Before an asset can become `candidate`, MYB-144 validators must pass or document
warnings.

Minimum validation themes:

- manifest entry exists;
- status combination is valid;
- asset paths exist;
- license/provenance fields are present;
- source type is not `unknown` for candidate or promoted assets;
- AI-generated assets include explicit source/provenance notes;
- production zone assets are `promoted`;
- no visible asset is promoted without visual evidence.

### 6. Candidate Review

An asset can move to:

```json
{
  "intakeStatus": "approved",
  "promotionStatus": "candidate"
}
```

only when intake is approved and the asset is ready to test in a controlled
Unity context.

Candidate evidence can include:

- isolated preview;
- technical validation;
- candidate scene;
- route-camera test capture;
- overview capture;
- reviewer notes.

Preview evidence is intermediate. It cannot close production promotion.

### 7. Production Promotion

An asset can move to:

```json
{
  "intakeStatus": "approved",
  "promotionStatus": "promoted"
}
```

only when:

- provenance and license are acceptable;
- validator evidence has no blocking `ERROR`;
- `usageScope` is clear;
- production path is under `Assets/Echappee/Art/Production/...`;
- visible assets have route-camera evidence;
- visible assets have overview evidence;
- subjective visual fit has human validation;
- warnings or exceptions are documented.

Promotion is scoped. An asset promoted for `forest_corridor` is not
automatically promoted for future biomes.

### 8. Rejection Or Deprecation

Rejected assets must remain:

```json
{
  "intakeStatus": "rejected",
  "promotionStatus": "not_promoted"
}
```

Deprecated assets must remain:

```json
{
  "intakeStatus": "deprecated",
  "promotionStatus": "not_promoted"
}
```

Rejected or deprecated assets must not be used in production. Existing scene
usage must be removed, migrated, or documented as debt.

## Allowed Evidence By Surface

| Surface | Allowed use | Forbidden use |
|---|---|---|
| Meshy/Tripo preview | Decide whether a raw candidate is worth quarantine. | Production promotion. |
| Blender render/turntable | Fabrication and silhouette evidence. | Final visual validation. |
| Unity preview scene | Scale, pivot, bounds, material, import checks. | Premium target closure. |
| Canonical route capture | Production visual validation. | Replacing manifest/provenance gates. |
| Overview capture | Density and global context. | Compensating for a weak route view. |

## Review Checklist

- [ ] Asset has a manifest entry.
- [ ] Manifest root is versioned data.
- [ ] No `example: true` exists in the real manifest.
- [ ] No `reviewStatus` exists in the real manifest.
- [ ] `intakeStatus` and `promotionStatus` combination is valid.
- [ ] Source type is accurate.
- [ ] License and provenance are documented.
- [ ] Asset zone matches status.
- [ ] Blender cleanup is done or not applicable.
- [ ] MYB-144 validator result is linked.
- [ ] Visible candidate has route and overview evidence before promotion.
- [ ] Human validation is recorded when visual fit is subjective.

## MYB-143 Boundary

MYB-143 creates the manifest and workflow gate only.

Out of scope:

- generating assets;
- importing real assets;
- modifying Unity scenes;
- promoting assets;
- running Meshy, Tripo, or Blender generation;
- changing gameplay.

# Unity Art Asset Validator Spec

Status: Canonical validator specification for MYB-144 V1.

Scope: bounded Unity Editor validation for Art Rescue asset intake and promotion.

Unity project:
`unity/Echapee4D`

Validator code:
`unity/Echapee4D/Assets/MYB144/Editor/MYB144ArtAssetValidator.cs`

Unity menu:
`Tools/MyBike/Validation/MYB-144 Art Asset Validator`

Report:
`_bmad-output/unity-test-results/myb-144-art-asset-validator-report.md`

Manifest:
`docs/manifests/art-rescue-asset-manifest.json`

Manifest schema:
`docs/schemas/third-party-asset-manifest.md`

## 1. V1 Scope

MYB-144 V1 is a bounded Unity Editor validator.

It is not only a standalone JSON checker, because the project needs a real Unity
AssetDatabase gate. It is also not a broad universal asset scanner.

Mandatory V1 scope:

- read and validate the canonical Art Rescue asset manifest;
- scan bounded Art Rescue Unity roots if they exist;
- validate only obvious visual art asset candidates;
- run safe deterministic Unity checks;
- write a Markdown report;
- classify findings as `ERROR`, `WARNING`, or `INFO`;
- expose a menu entry and a batch entry.

Deferred from V1:

- subjective visual quality;
- `Premium target` validation;
- route-camera validation;
- silhouette quality;
- pivot heuristics;
- broad project-wide scan;
- full LOD policy;
- auto-repair or asset modification.

MYB-144 reports. It does not repair.

## 2. Execution Model

The validator has three execution layers:

1. `RunValidation()`
   - shared validation logic;
   - writes the Markdown report;
   - returns a structured result;
   - does not call `EditorApplication.Exit()`.

2. `RunFromMenu()`
   - Unity Editor menu entry;
   - calls `RunValidation()`;
   - logs `PASS`, `PASS_WITH_WARNINGS`, or `FAIL`;
   - never closes Unity.

3. `RunBatch()`
   - batch / CI entry;
   - calls `RunValidation()`;
   - exits `1` when `ERROR >= 1`;
   - exits `0` when `ERROR == 0`.

WARNING-only results are `PASS_WITH_WARNINGS` and exit `0` in V1.

## 3. Verdicts

| Condition | Verdict | Batch exit |
|---|---|---:|
| No `ERROR`, no `WARNING` | `PASS` | 0 |
| `WARNING` only | `PASS_WITH_WARNINGS` | 0 |
| At least one `ERROR` | `FAIL` | 1 |

Unexpected validator exceptions must be recorded as `ERROR`. In batch mode, they
must exit `1`.

## 4. Manifest Gate

Manifest checks are strict. Manifest errors are always `ERROR`.

The validator must check:

- manifest exists;
- root is an object, not a list;
- JSON is readable by Unity;
- `schemaVersion` exists and is supported;
- `updatedAt` exists and uses `YYYY-MM-DD`;
- `assets` exists and is an array;
- each asset id is unique;
- `sourceType` is allowed;
- `usageScope` is allowed;
- `visualImpact` is allowed;
- `intakeStatus` is present and allowed;
- `promotionStatus` is present and allowed;
- `reviewStatus` is forbidden;
- `example: true` is forbidden in the real manifest;
- status combinations are valid;
- `candidate` and `promoted` require `intakeStatus: approved`;
- promoted assets have non-empty `assetPaths`;
- promoted assets have non-empty `license`;
- promoted asset paths exist when Unity can resolve them;
- promoted assets do not use `sourceType: unknown`;
- promoted AI assets have explicit provenance/review notes.
- visible promoted assets have route and overview evidence.

`intakeStatus: approved` does not mean production.

Only `promotionStatus: promoted` is production promotion.

## 5. Scan Roots

MYB-144 V1 scans only bounded Art Rescue roots:

- `Assets/Echappee/Art`
- `Assets/Echappee/ArtRescue`
- `Assets/MYB*`

Missing roots are `INFO`, not `ERROR`.

The validator must not fail simply because no Art Rescue production assets exist
yet.

Ignored technical subfolders, unless explicitly referenced by the manifest:

- `Editor`
- `Tests`
- `Test`
- `Validation`
- `Reports`
- `Docs`
- `Documentation`

Explicit manifest paths win over default ignores.

## 6. Asset Candidate Extensions

Scanned asset candidate extensions:

- `.fbx`
- `.glb`
- `.gltf`
- `.obj`
- `.prefab`
- `.mat`
- `.asset`
- `.png`
- `.jpg`
- `.jpeg`
- `.tga`
- `.exr`
- `.psd`

Ignored by default:

- `.meta`
- `.cs`
- `.asmdef`
- `.unity`
- `.md`
- `.txt`
- `.json`, except explicitly configured manifest paths

Extension matching is case-insensitive.

MYB-144 V1 does not treat Unity scenes, scripts, `.meta` files, docs, reports,
or non-manifest JSON files as art asset candidates.

## 7. Zone Policy

The source of truth for production status is `promotionStatus`, not folder name.

Zone policy:

- `Production` path with missing promoted manifest evidence should be highly
  visible in the report;
- `Review` path is diagnostic by default;
- `Quarantine` path is non-production by default;
- `Assets/MYB*` paths are ticket-local and should not be treated as production
  merely because they exist.

V1 severity:

- unmanifested candidate in Production path: `WARNING`;
- unmanifested candidate in Review path: `WARNING`;
- unmanifested candidate in Quarantine path: `INFO` or `WARNING` when clearly
  suspicious;
- unmanifested candidate in `Assets/MYB*`: `WARNING` only for obvious asset-art
  files.

## 8. Severity Policy V1

Manifest errors are always `ERROR`.

Unity technical checks are `ERROR` only for `promotionStatus: promoted` assets.

For candidate, review, quarantine, non-manifested, or ambiguous assets,
technical issues are `WARNING` or `INFO` in V1.

### ERROR

Examples:

- manifest missing or invalid;
- root manifest is a list;
- unsupported `schemaVersion`;
- `assets` missing or not an array;
- duplicate asset id;
- `reviewStatus` present;
- `example: true` present;
- invalid `intakeStatus` or `promotionStatus`;
- invalid status combination;
- `candidate` / `promoted` without approved intake;
- promoted asset path missing or not found;
- promoted source type unknown;
- promoted license missing;
- promoted AI asset without explicit provenance/review notes;
- visible promoted asset without route or overview evidence;
- promoted prefab/model with missing material;
- promoted prefab/model with complex MeshCollider;
- promoted asset cannot be loaded by AssetDatabase.

### WARNING

Examples:

- unmanifested asset candidate;
- missing material on candidate/review/non-manifested asset;
- complex MeshCollider on candidate/review/non-manifested asset;
- texture max dimension greater than 2048;
- `.psd` under Production path;
- more than 4 materials on a model/prefab;
- suspicious bounds;
- ambiguous `.asset` file under Art Rescue roots.

### INFO

Examples:

- missing scan root;
- empty scan root;
- valid empty manifest;
- ignored extension;
- quarantine-only asset skipped;
- check deferred to future hardening.

## 9. V1 Thresholds

| Check | Threshold | Severity |
|---|---:|---|
| Texture max dimension | `> 2048` | `WARNING` |
| `.psd` under Production path | present | `WARNING` |
| MeshCollider sharedMesh triangles | `> 500` | `ERROR` for promoted, `WARNING` otherwise |
| Material count | `> 4` | `WARNING` |
| Triangle budgets | no hard V1 budget | deferred / warning only |
| Suspicious bounds | heuristic only | `WARNING` |

Triangle budgets by family are deferred until Art Rescue asset families and
production examples are stable enough to avoid noisy global thresholds.

## 10. Report Format

The report must include:

- final verdict;
- execution mode;
- expected batch exit behavior;
- error/warning/info counts;
- manifest path, schema version, updated date, and asset count;
- scan roots and missing roots;
- status summary;
- extension policy;
- severity policy;
- `ERROR` table;
- `WARNING` table;
- `INFO` table;
- deferred checks.

Canonical report path:

`_bmad-output/unity-test-results/myb-144-art-asset-validator-report.md`

## 11. Batch Command

Indicative batch invocation:

```bash
/Applications/Unity/Hub/Editor/<VERSION>/Unity.app/Contents/MacOS/Unity \
  -batchmode \
  -quit \
  -projectPath unity/Echapee4D \
  -executeMethod MYB144ArtAssetValidator.RunBatch \
  -logFile _bmad-output/unity-test-results/myb-144-unity-batch.log
```

If a future namespace is introduced, the `-executeMethod` value must use the
fully qualified method name.

## 12. Non-Modification Rule

MYB-144 must not:

- modify the manifest automatically;
- create Unity asset folders as a repair action;
- import assets;
- modify materials;
- remove colliders;
- move files;
- promote or demote assets;
- change scenes;
- generate assets;
- call Blender, Meshy, or Tripo.

Allowed outputs:

- Markdown validation report;
- Unity console logs;
- optional batch log.

## 13. Acceptance Criteria

- [ ] The validator code lives at
      `unity/Echapee4D/Assets/MYB144/Editor/MYB144ArtAssetValidator.cs`.
- [ ] The validator is Editor-only.
- [ ] The validator exposes
      `Tools/MyBike/Validation/MYB-144 Art Asset Validator`.
- [ ] The validator exposes `RunValidation()` and `RunBatch()` or equivalent.
- [ ] The validator reads `docs/manifests/art-rescue-asset-manifest.json`.
- [ ] Manifest schema problems are `ERROR`.
- [ ] `reviewStatus` is `ERROR`.
- [ ] `example: true` is `ERROR`.
- [ ] Status combinations are enforced.
- [ ] Promoted asset paths are required and checked.
- [ ] Scan roots are bounded.
- [ ] Missing scan roots are `INFO`.
- [ ] Candidate extensions are allowlisted.
- [ ] Ignored extensions do not trigger manifest violations.
- [ ] Unity technical `ERROR`s apply only to promoted assets.
- [ ] WARNING-only results exit `0` in batch.
- [ ] ERROR results exit `1` in batch.
- [ ] The report is written to `_bmad-output/unity-test-results/`.
- [ ] The validator does not modify scenes or assets.

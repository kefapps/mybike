# MYB-144 Art Asset Validator Report

Verdict: PASS

Execution:
- Mode: MYB-167-RouteVisibleSupportValidator
- Batch exit code: 0 when run through RunBatch

Summary:
- Errors: 0
- Warnings: 0
- Info: 34

Manifest:
- path: `docs/manifests/art-rescue-asset-manifest.json`
- schemaVersion: 1
- updatedAt: 2026-06-17
- asset count: 23
- schema reference: `docs/schemas/third-party-asset-manifest.md`

Report:
- `_bmad-output/unity-test-results/myb-144-art-asset-validator-report.md`

## Candidate Extension Policy

Scanned asset candidate extensions:
- `.asset`
- `.exr`
- `.fbx`
- `.glb`
- `.gltf`
- `.jpeg`
- `.jpg`
- `.mat`
- `.obj`
- `.png`
- `.prefab`
- `.psd`
- `.tga`

Ignored by default:
- `.asmdef`
- `.cs`
- `.json`
- `.md`
- `.meta`
- `.txt`
- `.unity`

Notes:
- Scene validation is out of MYB-144 V1 scope.
- Non-manifest JSON files are ignored.
- `.asset` files are scanned cautiously only inside Art Rescue roots.

## Severity Policy V1

Manifest errors are always ERROR.

Unity technical checks are ERROR only for `promotionStatus: promoted` assets.

For active candidate, review, non-manifested or ambiguous assets, technical issues are WARNING or INFO in V1.

Legacy/prototype unmanifested inventory is INFO, aggregated by folder, and is not treated as active candidate debt.

Thresholds:
- texture max dimension > 2048 => WARNING
- `.psd` under Production path => WARNING
- MeshCollider sharedMesh triangles > 500 => complex
- complex MeshCollider on promoted asset => ERROR
- complex MeshCollider on non-promoted asset => WARNING
- material count > 4 => WARNING
- triangle count and suspicious bounds => WARNING only in V1

## Unmanifested Asset Classification Policy

Active unmanifested candidate assets remain WARNING.
Legacy/prototype unmanifested inventory is reported as INFO in aggregate.

Active candidate paths include:
- `Assets/Echappee/Art/Candidates/...`
- `Assets/Echappee/Art/Production/...`

Legacy/prototype inventory paths include:
- `Assets/MYB*` ticket-local historical roots;
- `Assets/Echappee/Art/MYB*` historical/prototype roots;
- `Assets/Echappee/Art/ThirdParty/...`;
- `Assets/Echappee/Art/PremiumTreePolyHaven/...`.

Promoted assets and manifest schema issues remain strict ERROR regardless of this classification.

## Scan Roots

| Root | Exists | Assets found | Notes |
|---|---:|---:|---|
| `Assets/Echappee/ArtRescue` | No | 0 | Missing scan root. This is INFO in V1. |
| `Assets/Echappee/Art` | Yes | 155 | Scanned. |
| `Assets/MYB100` | Yes | 0 | No V1 asset candidates found. |
| `Assets/MYB102` | Yes | 0 | No V1 asset candidates found. |
| `Assets/MYB103` | Yes | 0 | No V1 asset candidates found. |
| `Assets/MYB104` | Yes | 16 | Scanned. |
| `Assets/MYB106` | Yes | 6 | Scanned. |
| `Assets/MYB107` | Yes | 4 | Scanned. |
| `Assets/MYB108` | Yes | 0 | No V1 asset candidates found. |
| `Assets/MYB112` | Yes | 0 | No V1 asset candidates found. |
| `Assets/MYB144` | Yes | 0 | No V1 asset candidates found. |
| `Assets/MYB145` | Yes | 0 | No V1 asset candidates found. |
| `Assets/MYB148` | Yes | 0 | No V1 asset candidates found. |
| `Assets/MYB149` | Yes | 0 | No V1 asset candidates found. |
| `Assets/MYB156` | Yes | 0 | No V1 asset candidates found. |
| `Assets/MYB158` | Yes | 0 | No V1 asset candidates found. |
| `Assets/MYB159` | Yes | 0 | No V1 asset candidates found. |
| `Assets/MYB160` | Yes | 0 | No V1 asset candidates found. |
| `Assets/MYB161` | Yes | 0 | No V1 asset candidates found. |
| `Assets/MYB163` | Yes | 13 | Scanned. |
| `Assets/MYB164` | Yes | 0 | No V1 asset candidates found. |
| `Assets/MYB165` | Yes | 13 | Scanned. |
| `Assets/MYB166` | Yes | 0 | No V1 asset candidates found. |
| `Assets/MYB167` | Yes | 0 | No V1 asset candidates found. |
| `Assets/MYB42` | Yes | 0 | No V1 asset candidates found. |
| `Assets/MYB44` | Yes | 0 | No V1 asset candidates found. |
| `Assets/MYB48` | Yes | 3 | Scanned. |
| `Assets/MYB51` | Yes | 0 | No V1 asset candidates found. |
| `Assets/MYB53` | Yes | 0 | No V1 asset candidates found. |
| `Assets/MYB57` | Yes | 0 | No V1 asset candidates found. |
| `Assets/MYB59` | Yes | 0 | No V1 asset candidates found. |
| `Assets/MYB60` | Yes | 0 | No V1 asset candidates found. |
| `Assets/MYB64` | Yes | 0 | No V1 asset candidates found. |
| `Assets/MYB73` | Yes | 0 | No V1 asset candidates found. |
| `Assets/MYB79` | Yes | 0 | No V1 asset candidates found. |
| `Assets/MYB80` | Yes | 0 | No V1 asset candidates found. |
| `Assets/MYB89` | Yes | 18 | Scanned. |
| `Assets/MYB90` | Yes | 0 | No V1 asset candidates found. |
| `Assets/MYB91` | Yes | 0 | No V1 asset candidates found. |
| `Assets/MYB95` | Yes | 0 | No V1 asset candidates found. |
| `Assets/MYB96` | Yes | 0 | No V1 asset candidates found. |
| `Assets/MYB98` | Yes | 0 | No V1 asset candidates found. |

## Status Summary

| intakeStatus / promotionStatus | Count |
|---|---:|
| `approved / candidate` | 23 |

## Legacy / Prototype Inventory Info

| Folder | Unmanifested asset candidates |
|---|---:|
| `Assets/Echappee/Art/MYB96BlenderGenerated` | 43 |
| `Assets/MYB89` | 18 |
| `Assets/Echappee/Art/ThirdParty` | 17 |
| `Assets/Echappee/Art/PremiumTreePolyHaven` | 16 |
| `Assets/MYB104` | 16 |
| `Assets/Echappee/Art/MYB95MeshyRelicFountain` | 15 |
| `Assets/Echappee/Art/MYB95MeshyLantern` | 13 |
| `Assets/MYB163` | 13 |
| `Assets/MYB165` | 13 |
| `Assets/Echappee/Art/MYB95MeshyCharacter` | 12 |
| `Assets/Echappee/Art/MYB107PolyHavenStylized` | 9 |
| `Assets/MYB106` | 6 |
| `Assets/Echappee/Art/MYB53Validation` | 5 |
| `Assets/MYB107` | 4 |
| `Assets/MYB48` | 3 |
| `Assets/Echappee/Art/MYB42Validation` | 2 |
| **Total legacy/prototype inventory** | **205** |

## ERROR

| Code | Asset id/path | Message | Recommended fix |
|---|---|---|---|
| - | - | None | - |

## WARNING

| Code | Asset id/path | Message | Recommended fix |
|---|---|---|---|
| - | - | None | - |

## INFO

| Code | Message |
|---|---|
| `UNITY_SCAN_ROOT_MISSING` | Assets/Echappee/ArtRescue is absent. |
| `UNITY_SCAN_ROOT_EMPTY` | Assets/MYB100 exists but contains no V1 asset candidates. |
| `UNITY_SCAN_ROOT_EMPTY` | Assets/MYB102 exists but contains no V1 asset candidates. |
| `UNITY_SCAN_ROOT_EMPTY` | Assets/MYB103 exists but contains no V1 asset candidates. |
| `UNITY_SCAN_ROOT_EMPTY` | Assets/MYB108 exists but contains no V1 asset candidates. |
| `UNITY_SCAN_ROOT_EMPTY` | Assets/MYB112 exists but contains no V1 asset candidates. |
| `UNITY_SCAN_ROOT_EMPTY` | Assets/MYB144 exists but contains no V1 asset candidates. |
| `UNITY_SCAN_ROOT_EMPTY` | Assets/MYB145 exists but contains no V1 asset candidates. |
| `UNITY_SCAN_ROOT_EMPTY` | Assets/MYB148 exists but contains no V1 asset candidates. |
| `UNITY_SCAN_ROOT_EMPTY` | Assets/MYB149 exists but contains no V1 asset candidates. |
| `UNITY_SCAN_ROOT_EMPTY` | Assets/MYB156 exists but contains no V1 asset candidates. |
| `UNITY_SCAN_ROOT_EMPTY` | Assets/MYB158 exists but contains no V1 asset candidates. |
| `UNITY_SCAN_ROOT_EMPTY` | Assets/MYB159 exists but contains no V1 asset candidates. |
| `UNITY_SCAN_ROOT_EMPTY` | Assets/MYB160 exists but contains no V1 asset candidates. |
| `UNITY_SCAN_ROOT_EMPTY` | Assets/MYB161 exists but contains no V1 asset candidates. |
| `UNITY_SCAN_ROOT_EMPTY` | Assets/MYB164 exists but contains no V1 asset candidates. |
| `UNITY_SCAN_ROOT_EMPTY` | Assets/MYB166 exists but contains no V1 asset candidates. |
| `UNITY_SCAN_ROOT_EMPTY` | Assets/MYB167 exists but contains no V1 asset candidates. |
| `UNITY_SCAN_ROOT_EMPTY` | Assets/MYB42 exists but contains no V1 asset candidates. |
| `UNITY_SCAN_ROOT_EMPTY` | Assets/MYB44 exists but contains no V1 asset candidates. |
| `UNITY_SCAN_ROOT_EMPTY` | Assets/MYB51 exists but contains no V1 asset candidates. |
| `UNITY_SCAN_ROOT_EMPTY` | Assets/MYB53 exists but contains no V1 asset candidates. |
| `UNITY_SCAN_ROOT_EMPTY` | Assets/MYB57 exists but contains no V1 asset candidates. |
| `UNITY_SCAN_ROOT_EMPTY` | Assets/MYB59 exists but contains no V1 asset candidates. |
| `UNITY_SCAN_ROOT_EMPTY` | Assets/MYB60 exists but contains no V1 asset candidates. |
| `UNITY_SCAN_ROOT_EMPTY` | Assets/MYB64 exists but contains no V1 asset candidates. |
| `UNITY_SCAN_ROOT_EMPTY` | Assets/MYB73 exists but contains no V1 asset candidates. |
| `UNITY_SCAN_ROOT_EMPTY` | Assets/MYB79 exists but contains no V1 asset candidates. |
| `UNITY_SCAN_ROOT_EMPTY` | Assets/MYB80 exists but contains no V1 asset candidates. |
| `UNITY_SCAN_ROOT_EMPTY` | Assets/MYB90 exists but contains no V1 asset candidates. |
| `UNITY_SCAN_ROOT_EMPTY` | Assets/MYB91 exists but contains no V1 asset candidates. |
| `UNITY_SCAN_ROOT_EMPTY` | Assets/MYB95 exists but contains no V1 asset candidates. |
| `UNITY_SCAN_ROOT_EMPTY` | Assets/MYB96 exists but contains no V1 asset candidates. |
| `UNITY_SCAN_ROOT_EMPTY` | Assets/MYB98 exists but contains no V1 asset candidates. |

## Deferred Checks

The following checks are intentionally out of V1 scope:
- visual quality;
- silhouette quality;
- route-camera validation;
- pivot heuristics;
- full LOD policy;
- broad project-wide scan.


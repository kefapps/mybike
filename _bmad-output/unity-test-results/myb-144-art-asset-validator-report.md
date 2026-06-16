# MYB-144 Art Asset Validator Report

Verdict: PASS_WITH_WARNINGS

Execution:
- Mode: Batch
- Batch exit code: 0 when run through RunBatch

Summary:
- Errors: 0
- Warnings: 211
- Info: 24

Manifest:
- path: `docs/manifests/art-rescue-asset-manifest.json`
- schemaVersion: 1
- updatedAt: 2026-06-16
- asset count: 21
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

For candidate, review, quarantine, non-manifested or ambiguous assets, technical issues are WARNING or INFO in V1.

Thresholds:
- texture max dimension > 2048 => WARNING
- `.psd` under Production path => WARNING
- MeshCollider sharedMesh triangles > 500 => complex
- complex MeshCollider on promoted asset => ERROR
- complex MeshCollider on non-promoted asset => WARNING
- material count > 4 => WARNING
- triangle count and suspicious bounds => WARNING only in V1

## Scan Roots

| Root | Exists | Assets found | Notes |
|---|---:|---:|---|
| `Assets/Echappee/ArtRescue` | No | 0 | Missing scan root. This is INFO in V1. |
| `Assets/Echappee/Art` | Yes | 153 | Scanned. |
| `Assets/MYB100` | Yes | 0 | No V1 asset candidates found. |
| `Assets/MYB102` | Yes | 0 | No V1 asset candidates found. |
| `Assets/MYB103` | Yes | 0 | No V1 asset candidates found. |
| `Assets/MYB104` | Yes | 16 | Scanned. |
| `Assets/MYB106` | Yes | 6 | Scanned. |
| `Assets/MYB107` | Yes | 4 | Scanned. |
| `Assets/MYB108` | Yes | 0 | No V1 asset candidates found. |
| `Assets/MYB112` | Yes | 0 | No V1 asset candidates found. |
| `Assets/MYB114` | Yes | 32 | Scanned. |
| `Assets/MYB144` | Yes | 0 | No V1 asset candidates found. |
| `Assets/MYB145` | Yes | 0 | No V1 asset candidates found. |
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
| `approved / candidate` | 21 |

## ERROR

| Code | Asset id/path | Message | Recommended fix |
|---|---|---|---|
| - | - | None | - |

## WARNING

| Code | Asset id/path | Message | Recommended fix |
|---|---|---|---|
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/Echappee/Art/MYB107PolyHavenStylized/Textures/bark_brown_01_arm_1k.jpg` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/Echappee/Art/MYB107PolyHavenStylized/Textures/bark_brown_01_diff_1k.jpg` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/Echappee/Art/MYB107PolyHavenStylized/Textures/bark_brown_01_nor_gl_1k.jpg` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/Echappee/Art/MYB107PolyHavenStylized/Textures/brown_mud_leaves_01_arm_1k.jpg` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/Echappee/Art/MYB107PolyHavenStylized/Textures/brown_mud_leaves_01_diff_1k.jpg` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/Echappee/Art/MYB107PolyHavenStylized/Textures/brown_mud_leaves_01_nor_gl_1k.jpg` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/Echappee/Art/MYB107PolyHavenStylized/Textures/dry_riverbed_rock_arm_1k.jpg` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/Echappee/Art/MYB107PolyHavenStylized/Textures/dry_riverbed_rock_diff_1k.jpg` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/Echappee/Art/MYB107PolyHavenStylized/Textures/dry_riverbed_rock_nor_gl_1k.jpg` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/Echappee/Art/MYB42Validation/Materials/MYB42_ForestGround03.mat` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/Echappee/Art/MYB42Validation/Materials/MYB42_PavingStones141.mat` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/Echappee/Art/MYB53Validation/Materials/MYB53_KayKitAtlas.mat` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/Echappee/Art/MYB53Validation/Materials/MYB53_PavingStones141_Stylized.mat` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/Echappee/Art/MYB53Validation/Materials/MYB53_RelicWarmLight.mat` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/Echappee/Art/MYB53Validation/Materials/MYB53_SoftVillageGrass.mat` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/Echappee/Art/MYB53Validation/Materials/MYB53_WarmRoadEdge.mat` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/Echappee/Art/MYB95MeshyCharacter/Materials/MYB95_CharacterProofGrass.mat` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/Echappee/Art/MYB95MeshyCharacter/Materials/MYB95_CharacterProofPaving.mat` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/Echappee/Art/MYB95MeshyCharacter/Materials/MYB95_RouteGuardian_PBR.mat` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/Echappee/Art/MYB95MeshyCharacter/Models/MYB95_RouteGuardian_Rigged.fbx` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/Echappee/Art/MYB95MeshyCharacter/Models/MYB95_RouteGuardian_Running.fbx` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/Echappee/Art/MYB95MeshyCharacter/Models/MYB95_RouteGuardian_Walking.fbx` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/Echappee/Art/MYB95MeshyCharacter/Prefabs/MYB95_RouteGuardian_Direct.prefab` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/Echappee/Art/MYB95MeshyCharacter/Textures/MYB95_RouteGuardian_BaseColor.png` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/Echappee/Art/MYB95MeshyCharacter/Textures/MYB95_RouteGuardian_Emission.png` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/Echappee/Art/MYB95MeshyCharacter/Textures/MYB95_RouteGuardian_Metallic.png` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/Echappee/Art/MYB95MeshyCharacter/Textures/MYB95_RouteGuardian_Normal.png` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/Echappee/Art/MYB95MeshyCharacter/Textures/MYB95_RouteGuardian_Roughness.png` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/Echappee/Art/MYB95MeshyLantern/Materials/MYB95_BlueRuneGlow.mat` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/Echappee/Art/MYB95MeshyLantern/Materials/MYB95_FlameGlow.mat` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/Echappee/Art/MYB95MeshyLantern/Materials/MYB95_MeshyLantern_PBR.mat` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/Echappee/Art/MYB95MeshyLantern/Materials/MYB95_ValidationGrass.mat` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/Echappee/Art/MYB95MeshyLantern/Materials/MYB95_ValidationPaving.mat` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/Echappee/Art/MYB95MeshyLantern/Materials/MYB95_ValidationRoadEdge.mat` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/Echappee/Art/MYB95MeshyLantern/Models/MYB95_MeshyLantern_LOD0_20k.fbx` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/Echappee/Art/MYB95MeshyLantern/Prefabs/MYB95_MeshyLantern.prefab` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/Echappee/Art/MYB95MeshyLantern/Textures/refined_base_color.png` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/Echappee/Art/MYB95MeshyLantern/Textures/refined_emission.png` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/Echappee/Art/MYB95MeshyLantern/Textures/refined_metallic.png` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/Echappee/Art/MYB95MeshyLantern/Textures/refined_normal.png` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/Echappee/Art/MYB95MeshyLantern/Textures/refined_roughness.png` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/Echappee/Art/MYB95MeshyRelicFountain/Materials/MYB95_RelicFountain_Energy.mat` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/Echappee/Art/MYB95MeshyRelicFountain/Materials/MYB95_RelicFountain_PBR.mat` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/Echappee/Art/MYB95MeshyRelicFountain/Materials/MYB95_RelicFountain_Water.mat` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/Echappee/Art/MYB95MeshyRelicFountain/Materials/MYB95_RelicValidationGrass.mat` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/Echappee/Art/MYB95MeshyRelicFountain/Materials/MYB95_RelicValidationPaving.mat` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/Echappee/Art/MYB95MeshyRelicFountain/Materials/MYB95_RelicValidationRoadEdge.mat` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/Echappee/Art/MYB95MeshyRelicFountain/Models/MYB95_RelicFountain_Hero_80k.fbx` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/Echappee/Art/MYB95MeshyRelicFountain/Models/MYB95_RelicFountain_LOD0_50k.fbx` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/Echappee/Art/MYB95MeshyRelicFountain/Models/MYB95_RelicFountain_LOD1_30k.fbx` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/Echappee/Art/MYB95MeshyRelicFountain/Prefabs/MYB95_RelicFountain.prefab` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/Echappee/Art/MYB95MeshyRelicFountain/Textures/refined_base_color.png` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/Echappee/Art/MYB95MeshyRelicFountain/Textures/refined_emission.png` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/Echappee/Art/MYB95MeshyRelicFountain/Textures/refined_metallic.png` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/Echappee/Art/MYB95MeshyRelicFountain/Textures/refined_normal.png` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/Echappee/Art/MYB95MeshyRelicFountain/Textures/refined_roughness.png` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/Echappee/Art/MYB96BlenderGenerated/Materials/MYB96_AlpineOchre.mat` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/Echappee/Art/MYB96BlenderGenerated/Materials/MYB96_DarkInsetStone.mat` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/Echappee/Art/MYB96BlenderGenerated/Materials/MYB96_DarkWoodGrain.mat` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/Echappee/Art/MYB96BlenderGenerated/Materials/MYB96_DullIron.mat` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/Echappee/Art/MYB96BlenderGenerated/Materials/MYB96_MutedRouteRed.mat` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/Echappee/Art/MYB96BlenderGenerated/Materials/MYB96_NewLeafGreen.mat` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/Echappee/Art/MYB96BlenderGenerated/Materials/MYB96_OffWhitePaint.mat` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/Echappee/Art/MYB96BlenderGenerated/Materials/MYB96_PineGreen.mat` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/Echappee/Art/MYB96BlenderGenerated/Materials/MYB96_RoadsideGrass.mat` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/Echappee/Art/MYB96BlenderGenerated/Materials/MYB96_SoftMarkerGlow.mat` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/Echappee/Art/MYB96BlenderGenerated/Materials/MYB96_WarmAlpineWood.mat` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/Echappee/Art/MYB96BlenderGenerated/Materials/MYB96_WeatheredStone.mat` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/Echappee/Art/MYB96BlenderGenerated/Materials/MYB96_WildflowerAccent.mat` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/Echappee/Art/MYB96BlenderGenerated/Models/MYB96_AlpinePineSmall.fbx` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/Echappee/Art/MYB96BlenderGenerated/Models/MYB96_AlpinePineTall.fbx` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/Echappee/Art/MYB96BlenderGenerated/Models/MYB96_CairnStack.fbx` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/Echappee/Art/MYB96BlenderGenerated/Models/MYB96_ColDirectionSign.fbx` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/Echappee/Art/MYB96BlenderGenerated/Models/MYB96_HairpinChevronSign.fbx` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/Echappee/Art/MYB96BlenderGenerated/Models/MYB96_KilometerMarker.fbx` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/Echappee/Art/MYB96BlenderGenerated/Models/MYB96_MarketCrateStack.fbx` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/Echappee/Art/MYB96BlenderGenerated/Models/MYB96_RoadReflectorPair.fbx` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/Echappee/Art/MYB96BlenderGenerated/Models/MYB96_RoadsideRockCluster.fbx` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/Echappee/Art/MYB96BlenderGenerated/Models/MYB96_StoneFlowerPlanter.fbx` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/Echappee/Art/MYB96BlenderGenerated/Models/MYB96_SummitArchMarker.fbx` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/Echappee/Art/MYB96BlenderGenerated/Models/MYB96_VillageBench.fbx` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/Echappee/Art/MYB96BlenderGenerated/Models/MYB96_VillageWellMarker.fbx` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/Echappee/Art/MYB96BlenderGenerated/Models/MYB96_WildflowerGrassPatch.fbx` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/Echappee/Art/MYB96BlenderGenerated/Models/MYB96_WoodFenceSegment.fbx` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/Echappee/Art/MYB96BlenderGenerated/Prefabs/MYB96_AlpinePineSmall.prefab` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/Echappee/Art/MYB96BlenderGenerated/Prefabs/MYB96_AlpinePineTall.prefab` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/Echappee/Art/MYB96BlenderGenerated/Prefabs/MYB96_CairnStack.prefab` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/Echappee/Art/MYB96BlenderGenerated/Prefabs/MYB96_ColDirectionSign.prefab` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/Echappee/Art/MYB96BlenderGenerated/Prefabs/MYB96_HairpinChevronSign.prefab` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/Echappee/Art/MYB96BlenderGenerated/Prefabs/MYB96_KilometerMarker.prefab` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/Echappee/Art/MYB96BlenderGenerated/Prefabs/MYB96_MarketCrateStack.prefab` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/Echappee/Art/MYB96BlenderGenerated/Prefabs/MYB96_RoadReflectorPair.prefab` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/Echappee/Art/MYB96BlenderGenerated/Prefabs/MYB96_RoadsideRockCluster.prefab` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/Echappee/Art/MYB96BlenderGenerated/Prefabs/MYB96_StoneFlowerPlanter.prefab` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/Echappee/Art/MYB96BlenderGenerated/Prefabs/MYB96_SummitArchMarker.prefab` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/Echappee/Art/MYB96BlenderGenerated/Prefabs/MYB96_VillageBench.prefab` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/Echappee/Art/MYB96BlenderGenerated/Prefabs/MYB96_VillageWellMarker.prefab` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/Echappee/Art/MYB96BlenderGenerated/Prefabs/MYB96_WildflowerGrassPatch.prefab` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/Echappee/Art/MYB96BlenderGenerated/Prefabs/MYB96_WoodFenceSegment.prefab` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/Echappee/Art/PremiumTreePolyHaven/Materials/MYB_PremiumTree_BarkPolyHaven.mat` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/Echappee/Art/PremiumTreePolyHaven/Materials/MYB_PremiumTree_MossPolyHaven.mat` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/Echappee/Art/PremiumTreePolyHaven/Materials/MYB_PremiumTree_StylizedFoliage.mat` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/Echappee/Art/PremiumTreePolyHaven/Models/MYB_PremiumTreePolyHaven.fbx` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/Echappee/Art/PremiumTreePolyHaven/Prefabs/MYB112_PremiumTree_A.prefab` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/Echappee/Art/PremiumTreePolyHaven/Prefabs/MYB112_PremiumTree_B.prefab` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/Echappee/Art/PremiumTreePolyHaven/Prefabs/MYB112_PremiumTree_C.prefab` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/Echappee/Art/PremiumTreePolyHaven/Prefabs/MYB112_PremiumTree_D.prefab` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/Echappee/Art/PremiumTreePolyHaven/Prefabs/MYB112_PremiumTree_E.prefab` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/Echappee/Art/PremiumTreePolyHaven/Prefabs/MYB_PremiumTreePolyHaven.prefab` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/Echappee/Art/PremiumTreePolyHaven/Textures/moss_wood_arm_1k.jpg` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/Echappee/Art/PremiumTreePolyHaven/Textures/moss_wood_diff_1k.jpg` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/Echappee/Art/PremiumTreePolyHaven/Textures/moss_wood_nor_gl_1k.jpg` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/Echappee/Art/PremiumTreePolyHaven/Textures/pine_bark_arm_1k.jpg` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/Echappee/Art/PremiumTreePolyHaven/Textures/pine_bark_diff_1k.jpg` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/Echappee/Art/PremiumTreePolyHaven/Textures/pine_bark_nor_gl_1k.jpg` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/Echappee/Art/ThirdParty/KayKit/MedievalHexagonPack/Models/barrel.fbx` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/Echappee/Art/ThirdParty/KayKit/MedievalHexagonPack/Models/building_home_A_yellow.fbx` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/Echappee/Art/ThirdParty/KayKit/MedievalHexagonPack/Models/building_market_yellow.fbx` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/Echappee/Art/ThirdParty/KayKit/MedievalHexagonPack/Models/building_well_yellow.fbx` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/Echappee/Art/ThirdParty/KayKit/MedievalHexagonPack/Models/fence_stone_straight.fbx` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/Echappee/Art/ThirdParty/KayKit/MedievalHexagonPack/Textures/hexagons_medieval.png` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/Echappee/Art/ThirdParty/Kenney/FantasyTownKit/Models/road.fbx` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/Echappee/Art/ThirdParty/Kenney/FantasyTownKit/Models/wall-window-stone.fbx` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/Echappee/Art/ThirdParty/Kenney/NatureKit/Models/rock_smallA.fbx` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/Echappee/Art/ThirdParty/Kenney/NatureKit/Models/tree_tall.fbx` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/Echappee/Art/ThirdParty/PolyHaven/ForestGround03/Textures/forrest_ground_03_diff_1k.jpg` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/Echappee/Art/ThirdParty/PolyHaven/ForestGround03/Textures/forrest_ground_03_nor_gl_1k.jpg` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/Echappee/Art/ThirdParty/PolyHaven/ForestGround03/Textures/forrest_ground_03_rough_1k.jpg` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/Echappee/Art/ThirdParty/Quaternius/HorseAnimated/Models/horse_animated.fbx` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/Echappee/Art/ThirdParty/ambientCG/PavingStones141/Textures/PavingStones141_1K-JPG_Color.jpg` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/Echappee/Art/ThirdParty/ambientCG/PavingStones141/Textures/PavingStones141_1K-JPG_NormalGL.jpg` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/Echappee/Art/ThirdParty/ambientCG/PavingStones141/Textures/PavingStones141_1K-JPG_Roughness.jpg` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/MYB104/Materials/MYB104_AlpineWood.mat` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/MYB104/Materials/MYB104_EdgeGuideStone.mat` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/MYB104/Materials/MYB104_FantasyBlueGlow.mat` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/MYB104/Materials/MYB104_ForestFloor.mat` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/MYB104/Materials/MYB104_ForestLightGrass.mat` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/MYB104/Materials/MYB104_MountainFar.mat` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/MYB104/Materials/MYB104_MountainNear.mat` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/MYB104/Materials/MYB104_MutedRouteStone.mat` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/MYB104/Materials/MYB104_PineDark.mat` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/MYB104/Materials/MYB104_RoadsideShadow.mat` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/MYB104/Materials/MYB104_RoofRed.mat` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/MYB104/Materials/MYB104_TrunkWarm.mat` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/MYB104/Materials/MYB104_WarmLanternGlow.mat` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/MYB104/Materials/MYB104_WarmVillageStone.mat` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/MYB104/Materials/MYB104_WeatheredStone.mat` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/MYB104/Materials/MYB104_WildflowerAccent.mat` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/MYB106/Materials/MYB106_AmberMoss.mat` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/MYB106/Materials/MYB106_BlueShadow.mat` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/MYB106/Materials/MYB106_CoolTrunk.mat` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/MYB106/Materials/MYB106_CoolUndergrowth.mat` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/MYB106/Materials/MYB106_DeepNeedle.mat` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/MYB106/Materials/MYB106_LeafLitter.mat` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/MYB107/Materials/MYB107_StylizedCoolStone.mat` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/MYB107/Materials/MYB107_StylizedForestVegetation.mat` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/MYB107/Materials/MYB107_StylizedRouteGround.mat` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/MYB107/Materials/MYB107_StylizedWarmWood.mat` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/MYB114/Imported/AuthoredRootFamily/MYB114_RootArchA_TallLateral.fbx` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/MYB114/Imported/AuthoredRootFamily/MYB114_RootArchB_LowDense.fbx` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/MYB114/Imported/AuthoredRootFamily/MYB114_RootClusterC_SideStump.fbx` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/MYB114/Imported/TripoHalfArch/MYB114_TripoHalfArch_CoreModuleCleanup.fbx` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/MYB114/Imported/TripoHalfArch/MYB114_TripoHalfArch_CoreModuleCleanup.glb` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/MYB114/Imported/TripoHalfArch/MYB114_TripoHalfArch_OrientedFull.fbx` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/MYB114/Imported/TripoHalfArch/MYB114_TripoHalfArch_OrientedFull.glb` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/MYB114/Imported/TripoHalfArch/MYB114_TripoHalfArch_SafeGroundCleanup.fbx` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/MYB114/Imported/TripoHalfArch/MYB114_TripoHalfArch_SafeGroundCleanup.glb` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/MYB114/Materials/MYB114_Corridor_Berm.mat` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/MYB114/Materials/MYB114_Corridor_CanopyShadow.mat` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/MYB114/Materials/MYB114_Corridor_DeadBranch.mat` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/MYB114/Materials/MYB114_Corridor_EmbeddedRoot.mat` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/MYB114/Materials/MYB114_Corridor_ExposedSoil.mat` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/MYB114/Materials/MYB114_Corridor_Fern.mat` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/MYB114/Materials/MYB114_Corridor_ForestFloor.mat` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/MYB114/Materials/MYB114_Corridor_LeafLitter.mat` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/MYB114/Materials/MYB114_Corridor_LeafPile.mat` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/MYB114/Materials/MYB114_Corridor_Moss.mat` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/MYB114/Materials/MYB114_Corridor_Road.mat` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/MYB114/Materials/MYB114_Corridor_Shoulder.mat` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/MYB114/Materials/MYB114_Corridor_TrunkBase.mat` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/MYB114/Materials/MYB114_Corridor_TrunkSilhouette.mat` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/MYB114/Materials/MYB114_Corridor_Understory.mat` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/MYB114/Materials/MYB114_Corridor_Verge.mat` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/MYB114/Materials/MYB114_Preview_AssetClay.mat` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/MYB114/Materials/MYB114_Preview_Ground.mat` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/MYB114/Materials/MYB114_Preview_Road.mat` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/MYB114/Materials/MYB114_Preview_Shoulder.mat` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/MYB114/Materials/MYB137_LeaningTrunk.mat` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/MYB114/Materials/MYB137_RootRise.mat` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/MYB114/Materials/MYB137_VerticalTrunk.mat` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/MYB48/Materials/MYB48_ClimbCue.mat` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/MYB48/Materials/MYB48_RecoveryCue.mat` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/MYB48/Materials/MYB48_SprintCue.mat` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/MYB89/Materials/MYB89_Banner.mat` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/MYB89/Materials/MYB89_Cockpit.mat` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/MYB89/Materials/MYB89_EdgeLine.mat` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/MYB89/Materials/MYB89_Grass.mat` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/MYB89/Materials/MYB89_Hill.mat` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/MYB89/Materials/MYB89_HorizonHill.mat` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/MYB89/Materials/MYB89_HorizonVillage.mat` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/MYB89/Materials/MYB89_LanePaint.mat` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/MYB89/Materials/MYB89_PostBlue.mat` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/MYB89/Materials/MYB89_PostCoral.mat` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/MYB89/Materials/MYB89_PostWhite.mat` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/MYB89/Materials/MYB89_PremiumWarmGlow.mat` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/MYB89/Materials/MYB89_Road.mat` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/MYB89/Materials/MYB89_TreeLeaf.mat` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/MYB89/Materials/MYB89_TreeTrunk.mat` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/MYB89/Materials/MYB89_VillageRoof.mat` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/MYB89/Materials/MYB89_VillageWall.mat` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |
| `ASSET_CANDIDATE_UNMANIFESTED` | `Assets/MYB89/Materials/MYB89_VillageWood.mat` | Asset candidate is present in a scanned Art Rescue root but is not listed in the manifest. | Add a manifest entry if this candidate should enter review or production. |

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


# MYB-154 Implementation Report

## Summary

MYB-154 hardens MYB-144 so historical/prototype unmanifested assets no longer
pollute every Art Rescue ticket as active warnings.

The validator now separates:

- active unmanifested candidates: `WARNING`;
- legacy/prototype unmanifested inventory: aggregate `INFO`;
- manifest and promoted asset problems: strict `ERROR`.

## Scope

Changed:

- `unity/Echapee4D/Assets/MYB144/Editor/MYB144ArtAssetValidator.cs`

Not changed:

- canonical asset manifest;
- MYB-149 scene, reports or captures;
- asset promotion statuses;
- Unity scenes or art assets.

## Classification Policy

Active candidate paths remain warnings when unmanifested:

- `Assets/Echappee/Art/Candidates/...`
- `Assets/Echappee/Art/Production/...`

Legacy/prototype paths become aggregate info when unmanifested:

- `Assets/MYB*`
- `Assets/Echappee/Art/MYB*`
- `Assets/Echappee/Art/ThirdParty/...`
- `Assets/Echappee/Art/PremiumTreePolyHaven/...`

Promoted assets and manifest schema errors are not relaxed.

## Expected Validator Impact

Before MYB-154, MYB-144 repeatedly reported the same historical unmanifested
candidate files as warnings, including old Meshy, Poly Haven, validation,
prototype and ticket-local roots.

After MYB-154, those files are still visible in the report under:

`Legacy / Prototype Inventory Info`

but they no longer count as active warnings.

## Validation

Validation target:

`_bmad-output/unity-test-results/myb-144-art-asset-validator-report.md`

Command:

```bash
/Applications/Unity/Hub/Editor/6000.4.10f1/Unity.app/Contents/MacOS/Unity \
  -batchmode \
  -quit \
  -projectPath /Users/jbodin/personnel/apps/mybike-myb-154/unity/Echapee4D \
  -executeMethod MYB144ArtAssetValidator.RunBatch \
  -logFile /Users/jbodin/personnel/apps/mybike-myb-154/_bmad-output/unity-test-results/myb-154-myb144-batch.log
```

Observed result:

- MYB-144 verdict: `PASS`
- MYB-144 errors: 0
- MYB-144 warnings: 0
- MYB-144 info: 25
- legacy/prototype unmanifested inventory: 179 assets reported as aggregate INFO
- active unmanifested candidate warnings: 0

## Governance

- No manifest mass-fill was performed.
- No historical assets were promoted.
- No active candidate leak is hidden.
- MYB-144 remains strict for promoted assets and manifest schema issues.

## Verdict

PASS

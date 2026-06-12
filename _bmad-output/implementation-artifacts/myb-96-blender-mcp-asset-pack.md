# MYB-96 - Blender MCP asset pack

Date: 2026-06-12
Branch: `myb-96-blender-mcp-assets`
Linear: https://linear.app/kefjbo/issue/MYB-96/generer-un-pack-dassets-blender-via-mcp-pour-la-vertical-slice-unity
Status: In Review

## Scope

MYB-96 generates a local Blender MCP asset pack for the canonical Unity project
`unity/Echapee4D`. The pack follows the grill-me decisions:

- 15 generated assets, not the original 3 to 6 ticket baseline.
- 5 `route-signage` assets.
- 5 `natural-roadside` assets.
- 5 `village-countryside` assets.
- Procedural low/mid-poly Blender generation with Unity materials, no bitmap
  textures and no external generation service.
- FBX is the Unity import format; the Blender source is kept in the asset
  folder for local regeneration.

## Generated Assets

Route and signage:

- `MYB96_ColDirectionSign`
- `MYB96_KilometerMarker`
- `MYB96_HairpinChevronSign`
- `MYB96_SummitArchMarker`
- `MYB96_RoadReflectorPair`

Natural roadside:

- `MYB96_AlpinePineSmall`
- `MYB96_AlpinePineTall`
- `MYB96_CairnStack`
- `MYB96_RoadsideRockCluster`
- `MYB96_WildflowerGrassPatch`

Village and countryside:

- `MYB96_StoneFlowerPlanter`
- `MYB96_WoodFenceSegment`
- `MYB96_VillageBench`
- `MYB96_MarketCrateStack`
- `MYB96_VillageWellMarker`

## Unity Integration

- Root folder:
  `unity/Echapee4D/Assets/Echappee/Art/MYB96BlenderGenerated`
- Models:
  `Assets/Echappee/Art/MYB96BlenderGenerated/Models/*.fbx`
- Materials:
  `Assets/Echappee/Art/MYB96BlenderGenerated/Materials/*.mat`
- Prefabs:
  `Assets/Echappee/Art/MYB96BlenderGenerated/Prefabs/*.prefab`
- Blender source:
  `Assets/Echappee/Art/MYB96BlenderGenerated/Source/MYB96_BlenderGeneratedAssetPack.blend`
- Internal manifest:
  `Assets/Echappee/Art/MYB96BlenderGenerated/GeneratedAssets.assetmanifest.json`
- Validation scene:
  `Assets/Scenes/MYB96BlenderAssetYard.unity`
- Editor builder and validator:
  `Assets/MYB96/Editor/MYB96BlenderAssetPackBuilder.cs`

## Provenance

The manifest records the source as local procedural generation through Blender
MCP `execute_blender_code`. It explicitly records `externalServices: []`.

The pack does not use Meshy, Hunyuan3D, Hyper3D Rodin, paid generation, third
party downloaded models, connected-bike hardware, gameplay changes, WebGL work
or React/Three.js prototype changes.

## Validation Evidence

- Unity batch build/validation:
  `/Applications/Unity/Hub/Editor/6000.4.10f1/Unity.app/Contents/MacOS/Unity -batchmode -quit -projectPath unity/Echapee4D -executeMethod MYB96BlenderAssetPackBuilder.BuildAndValidateCli -logFile _bmad-output/unity-test-results/myb-96-unity-batch-remap-materials.log`
- Final report:
  `_bmad-output/unity-test-results/myb-96-blender-asset-pack-report.json`
- Blender metrics:
  `_bmad-output/unity-test-results/myb-96-blender-generated-asset-metrics.json`
- Visual capture:
  `_bmad-output/unity-test-results/myb-96-blender-asset-yard.png`

Report summary:

- Status: `pass`
- Prefabs: `15`
- Materials: `13`
- Material remaps: `56` on the first remap pass
- Total triangles: `6224`
- Per-asset triangle budget: `5000`
- Pack triangle budget: `35000`
- Canonical MYB-89 scene presence check: `pass`

Later validation passes may report `MaterialRemaps: 0` because the FBX importer
remaps are already persisted in the `.fbx.meta` files.

## Known Caveat

Unity batchmode logs a non-blocking licensing handshake warning and repeated
IvanMurzak Unity-MCP token/handshake errors during shutdown in this worktree.
The MYB-96 builder itself completes and writes a `pass` report. Some runs exit
with code 0; later final runs after the unlit material review pass wrote a
fresh `pass` report and capture, then exited non-zero when the Unity-MCP plugin
reported `Authorization failed. Token may be missing, invalid, or revoked.`

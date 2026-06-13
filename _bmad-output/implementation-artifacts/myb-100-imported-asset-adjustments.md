# MYB-100 - Imported Unity Asset Adjustments

Date: 2026-06-13

## Scope

Applied the MYB-50 macOS-first Unity asset pipeline to assets already imported
by MYB-42, MYB-53, MYB-95 and MYB-96.

## Delivered

- Added `MYB100ImportedAssetOptimizer`, a Unity Editor validator that applies
  and validates importer policy for existing textures and FBX models under
  `Assets/Echappee/Art`.
- Kept `ThirdPartyAssets.assetmanifest.json` third-party only.
- Added `MYB95GeneratedAssets.assetmanifest.json` for Meshy-generated MYB-95
  provenance and the 2K texture cap used by the POC assets.
- Rebuilt the canonical ride scene and replaced the primitive premium signal
  placeholder with optimized imported assets.
- Added ten `MYB100_` scene placements using MYB-95 Meshy assets and MYB-96
  Blender-generated route props.
- Added MYB-100 to `npm run validate:local-ci`.

## Policy Applied

- Third-party texture maps remain capped at 1024.
- MYB-95 generated premium signal / animated actor textures are capped at 2048.
- Normal maps use `TextureImporterType.NormalMap` with sRGB disabled.
- Roughness and metallic maps keep sRGB disabled.
- Color and emission maps keep sRGB enabled.
- All 3D textures use mip maps and `CompressedHQ`.
- Static FBX models import without animation, embedded materials, cameras,
  lights, visibility curves, or readable meshes.
- Horse and Route Guardian animated models keep animation import enabled.

## Evidence

- `_bmad-output/unity-test-results/myb-100-imported-asset-adjustments.txt`
- `_bmad-output/unity-test-results/myb-100-canonical-assets.png`
- `_bmad-output/unity-test-results/myb-83-local-ci.txt`

## Validation

- `jq empty unity/Echapee4D/Assets/Echappee/Art/ThirdPartyAssets.assetmanifest.json unity/Echapee4D/Assets/Echappee/Art/MYB95GeneratedAssets.assetmanifest.json unity/Echapee4D/Assets/Echappee/Art/MYB96BlenderGenerated/GeneratedAssets.assetmanifest.json`: PASS
- `unity-mcp-cli status unity/Echapee4D --timeout 10000`: PASS
- `MYB100ImportedAssetOptimizer.ApplyAndValidateCli()`: PASS via Unity-MCP
- `MYB91CanonicalBaselineValidator.ValidateCanonicalBaselineCli()`: PASS via Unity-MCP
- `npm run validate:local-ci`: PASS
- `git diff --check`: PASS

## Scope Guard

- No new mass import.
- No new biome or corridor rewrite.
- No `src/**` changes.
- No `unity/Echappee3D/**` recreation.
- No Meshy credit usage.

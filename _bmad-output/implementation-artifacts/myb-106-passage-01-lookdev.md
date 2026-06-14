# MYB-106 Passage 01 LookDev

## Summary

MYB-106 adds a local reversible LookDev overlay for Passage 01 in the canonical
Unity scene. The pass targets a dramatic stylized premium undergrowth `Vue
Cible` without changing global URP project defaults.

## Implementation

- Added `MYB106Passage01LookDev`, a Unity Editor composer/validator.
- Replays MYB-104 first, then removes and rebuilds `MYB106_LookDevPassage01`.
- Preserves MYB-104 ambient/fog/key sun and keeps the lookdev work in local
  overlay lights, probes, composition, and material palette.
- Attenuates MYB-104 forest patches, floor ribbons, and beige pine tones in
  Passage 01.
- Adds denser local pines, dark edge shadow accents, amber moss accents, local
  spot/fill lights, two reflection probes, and one LightProbeGroup.
- Adds MYB-106 to `npm run validate:local-ci` after MYB-104 so local CI leaves
  the scene in the active MYB-106 visual state.

## Evidence

- Static before capture:
  `_bmad-output/unity-test-results/myb-106/before-vue-cible.png`
- Static after capture:
  `_bmad-output/unity-test-results/myb-106/after-vue-cible.png`
- Unity report:
  `_bmad-output/unity-test-results/myb-106/myb-106-passage-01-lookdev-report.txt`
- Silent 720p proof video:
  `_bmad-output/unity-test-results/myb-106/passage-01-lookdev-15s-720p.mp4`
- Video frames were generated transiently for encoding and removed from the PR
  payload.

## Validation

- `node --check scripts/validate-local-ci.mjs`: PASS
- `unity-mcp-cli status unity/Echapee4D --timeout 10000`: PASS before the heavy
  video capture.
- `MYB106Passage01LookDev.ApplyAndValidateCli()`: PASS
- `MYB106Passage01LookDev.CaptureProofCli()`: generated all 180 frames, but the
  Unity-MCP client timed out before returning the response.
- `ffmpeg` encoded the 180 frames into a silent 1280x720 MP4.

## Limits

- This improves local palette hierarchy and density, but it does not solve the
  whole visual quality problem alone.
- The scene still needs stronger material/shader work and better source assets;
  those remain in MYB-107 and MYB-108.
- No global URP default change was made; MYB-102 remains the evidence that URP
  defaults alone were not the primary lever.

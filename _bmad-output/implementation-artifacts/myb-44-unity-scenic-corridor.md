# MYB-44 - Unity Scenic Corridor

Status: implemented
Linear: https://linear.app/kefjbo/issue/MYB-44/myb-012-remplacer-les-placeholders-unity-par-un-premier-scenic
Branch: `myb-44-scenic-corridor`

## Goal

Replace part of the Unity placeholder environment with a first playable
`Scenic Corridor` in the canonical `unity/Echapee4D` scene while preserving the
route, camera, HUD, mock mode and MYB-48 Route Difficulty Cues.

## Grill-with-docs decisions

- Canonical term: `Scenic Corridor`.
- First corridor: `Village / campagne pavee`.
- Composition: approved/imported assets as identity base, Unity primitives for
  composition, silhouettes, grounding and polish.
- Premium accent: one discreet `Signal Fantasy Premium`, off the ride lane and
  subordinate to route, HUD and Route Difficulty Cues.
- Target scene: canonical `Assets/Scenes/MYB89UnityMcpProbe.unity`, generated
  through `MYB89ProbeBuilder`.
- Spatial scope: foreground, roadside, mid-ground, background and horizon.
- Horizon tone: warm countryside/village, readable and soft, with controlled
  fantasy accent only.
- Density: moderate but composed, macOS-first, no massive asset import.
- Validation: Unity Editor/macOS local proof only; no WebGL proof.

## Implementation

- Added a `Scenic Corridor` glossary entry to `CONTEXT.md`.
- Extended `MYB89ProbeBuilder` to generate `MYB44_ScenicCorridor` inside the
  canonical scene.
- Reused approved/imported third-party POC assets already present in the repo:
  KayKit village buildings/fence/barrel, Kenney tree/rock, Quaternius horse,
  and the MYB-53 stylized paving material.
- Added warm paved shoulders, village verges, roadside props, village silhouettes,
  foreground/mid-ground composition, background cottages, horizon trees, softer
  rolling horizon hills and one primitive warm relic lantern.
- Added `MYB44ScenicCorridorValidator` with a dedicated validation report.
- Extended MYB-89 and MYB-91 validation reports with scenic/horizon/premium
  counters.

## Validation

- `unity-mcp-cli status unity/Echapee4D --timeout 10000`: PASS.
- `MYB89ProbeBuilder.BuildScene()`: PASS.
- `MYB89ProbeBuilder.ValidateProbeScene()`: PASS.
- `MYB44ScenicCorridorValidator.ValidateScenicCorridor()`: PASS.
- `MYB91CanonicalBaselineValidator.ValidateCanonicalBaseline()`: PASS.
- `git diff --check`: PASS before final review sync.

## Evidence

- `_bmad-output/unity-test-results/myb-44-scenic-corridor.txt`
- `_bmad-output/unity-test-results/myb-44-scenic-corridor.png`
- `_bmad-output/unity-test-results/myb-89-unity-mcp-probe-validator.txt`
- `_bmad-output/unity-test-results/myb-91-canonical-baseline.txt`
- Latest local capture folder:
  `_bmad-output/video-captures/myb-89-unity-mcp-probe-20260612-075952/`

## Final counters

- Total scene renderers: 271.
- MYB44 scenic corridor renderers: 116.
- MYB44 horizon renderers: 43.
- Premium signal lights: 1.
- Route difficulty cues preserved: 11 total, Climb 4, Sprint 4, Recovery 3.

## Scope notes

- No new external asset pack was imported.
- No Meshy generation or paid service call was used.
- No `src/**` or legacy Unity project files were touched.
- WebGL was intentionally not rebuilt for this ticket.

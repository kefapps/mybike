# MYB-161 Implementation Report

## Summary

MYB-161 is an art-directed composition pass, not a new global generation pass. MYB-160 proved Meshy can provide stronger isolated candidates, but the route-camera image stayed weak when the slice remained mostly object placement. MYB-161 rebuilds the preview as a five-plane route-camera composition.

## Baseline

- before = MYB-160 after
- before scene: `Assets/Scenes/MYB160MeshyHeroCandidatePreview.unity`
- before route: `_bmad-output/visual-checkpoints/MYB-161/2026-06-17T18-58-45Z-before-route.png`
- before overview: `_bmad-output/visual-checkpoints/MYB-161/2026-06-17T18-58-45Z-before-overview.png`

## Builder

- path: `unity/Echapee4D/Assets/MYB161/Editor/MYB161ArtDirectedGoldenSliceBuilder.cs`
- seed: 161001
- generated root: `MYB161_ArtDirectedGoldenSliceRoot`
- output scene: `Assets/Scenes/MYB161ArtDirectedGoldenSlicePreview.unity`
- source scene: `Assets/Scenes/MYB160MeshyHeroCandidatePreview.unity`

## Art Direction Recipe

- foreground frame: large left ancient trunk assembly at 0-8/near meters, plus low right root/moss bank.
- near side mass: supporting side tree assemblies and root clusters from 8-18m.
- hero threshold: existing MYB-160 Meshy hero tree plus lateral root arch, treated as one natural gate moment.
- back wall: taller desaturated side masses at 30-45m.
- background atmosphere: low-cost distant silhouettes beyond 45m with fog/desaturation.

## Layout Decisions

- foreground left ancient trunk: `MYB161_TreeAssembly_A`, X/offset approximately -6.1m, Z/meters 10.5.
- foreground right low root bank: `MYB161_ForegroundRight_LowRootMossBank`, offset +4.8m, meters 11.5.
- mid right hero tree: `MYB161_HeroTreeAssembly`, offset +9.55m, meters 23.2, using the cleaned MYB-160 Meshy tree as a restrained candidate mass.
- hero threshold: `MYB161_HeroThreshold_RootArchNaturalGate`, offset -6.25m, meters 29.5, using the cleaned MYB-160 Meshy root arch diagonally.
- back wall: 10 side masses at offsets roughly +/-9m to +/-14m.

## Tree Assemblies

### MYB161_TreeAssembly_A

- role: Plan A foreground left ancient trunk frame
- visible from route: Yes
- canopy supported: Yes
- grounding: combined renderer bounds min.y, sink 0.03m
- notes: Foreground left frame. Big asymmetrical trunk, roots and horizontal supported canopy lobes frame the route without closing it.

### MYB161_TreeAssembly_B

- role: Plan B near left supporting side mass
- visible from route: Yes
- canopy supported: Yes
- grounding: combined renderer bounds min.y, sink 0.03m
- notes: Near left support mass. Smaller than foreground frame, used to make the left side dense without repeating a straight row.

### MYB161_TreeAssembly_C

- role: Plan B/C right mid support mass
- visible from route: Yes
- canopy supported: Yes
- grounding: combined renderer bounds min.y, sink 0.03m
- notes: Right side support mass behind the foreground bank. It supports the hero tree area without becoming a second hero beat.

### MYB161_HeroTreeAssembly

- role: Midground right hero ancient tree
- visible from route: Yes
- canopy supported: Yes
- grounding: Meshy candidate grounded by combined renderer bounds min.y, sink 0.03m
- notes: Plan C. Existing MYB-160 Meshy ancient tree used as a right-side secondary focal mass, scaled down and placed farther from the route to avoid a mushroom/blob read.

## Meshy Usage

- Used existing MYB-160 Meshy assets: Yes
- New Meshy generations: 0
- Manifest status: existing MYB-160 entries are `intakeStatus: approved`, `promotionStatus: candidate`, `license: Provider terms pending project review`.
- No production promotion.

## Route Readability

- minimumRouteClearanceMeters: 2.75
- routeOverlapCount: 0
- routeReadabilityRegression: No

## Anti-Float / Support

- floatingAssetCount: 0
- routeVisibleFloatingAssetCount: 0
- maxFloatingClearance: 0m
- sinkingAssetCount: 0
- maxSinkingDepth: 0.03m
- routeVisibleUnsupportedCanopyCount: 0

## Visual Evidence

- before route: `_bmad-output/visual-checkpoints/MYB-161/2026-06-17T18-58-45Z-before-route.png`
- before overview: `_bmad-output/visual-checkpoints/MYB-161/2026-06-17T18-58-45Z-before-overview.png`
- after route: `_bmad-output/visual-checkpoints/MYB-161/2026-06-17T18-58-47Z-after-route.png`
- after overview: `_bmad-output/visual-checkpoints/MYB-161/2026-06-17T18-58-47Z-after-overview.png`
- route comparison: `_bmad-output/visual-checkpoints/MYB-161/2026-06-17T18-58-47Z-route-before-after.png`
- overview comparison: `_bmad-output/visual-checkpoints/MYB-161/2026-06-17T18-58-47Z-overview-before-after.png`
- capture report: `_bmad-output/visual-checkpoints/MYB-161/2026-06-17T18-58-47Z-before-after-capture-report.md`

## MYB-144 Validation

- verdict: PASS
- errors: 0
- warnings: 0
- report: `_bmad-output/unity-test-results/myb-144-art-asset-validator-report.md`

## Visual Rubric Estimate

Scores are implementation estimates pending Julien human visual review.

| Criterion | Estimate | Notes |
|---|---:|---|
| Route readability | 4 | Route is kept open, with no overlap and minimum clearance 2.75m. |
| Silhouette quality | 3 | Foreground and hero silhouettes are more directed, but still use simple procedural and candidate materials. |
| Lighting mood | 3 | Slightly deeper fog and warmer break light improve depth without hiding the road. |
| Material coherence | 3 | Scene-local palette is coherent enough for preview, not final art. |
| Foreground richness | 4 | Left frame and right root bank make the first 8-14m less empty and less prop-like. |
| Midground density | 4 | Hero tree, threshold and side masses improve corridor body. |
| Background depth | 4 | Back wall and distant silhouettes reduce empty sky/flat background risk. |
| Scale credibility | 4 | Grounding, support and clearance metrics pass. |
| Composition rhythm | 4 | Five-plane layout replaces uniform scatter with foreground, side mass, hero threshold and depth. |

## Warning Categories

### Build / Capture Warnings

- None recorded.

### MYB-161 Visual Warnings

- Premium target intentionally not claimed; MYB-161 is an art-directed composition checkpoint pending Julien visual review.
- MYB-161 reduces isolated prop placement by disabling MYB-159/MYB-160 generated art roots in the preview output and rebuilding the slice with five explicit route-camera planes.
- Blob canopy dominance is reduced but still not eliminated; the source project still lacks final bespoke forest canopy assets.

### MYB-161 Asset / Manifest Warnings

- Existing MYB-160 Meshy assets are used as preview candidates only.
- Meshy license remains `Provider terms pending project review`; no production promotion is introduced.
- New Meshy generation count is 0; MYB-161 does not spend credits.

### MYB-144 Existing Validator Warnings

- None recorded.

### Blocking Errors

- None recorded.

## Governance

- no canonical scene modified
- no gameplay modified
- no route collider/trajectory change
- no production promotion
- no new Meshy generation
- existing MYB-160 Meshy assets are candidate/preview-only
- Premium target reached: No

## Verdict

- Premium target reached: No
- Checkpoint insuffisant, but visually stronger pending Julien human review
- Recommended Linear status: In Review

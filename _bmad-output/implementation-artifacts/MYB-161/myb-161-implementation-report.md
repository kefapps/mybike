# MYB-161 Implementation Report

## Summary

MYB-161 is an art-directed composition pass, not a new global generation pass. MYB-160 proved Meshy can provide stronger isolated candidates. Julien then rejected the first sparse MYB-161 after direction in favor of the previous left/baseline mood. This revision preserves the baseline forest enclosure while keeping controlled route readability improvements.

Human preference note:
Julien prefers the previous left/baseline mood over the first MYB-161 after. This revision preserves the baseline forest enclosure while keeping route readability improvements.

Human validation note:
Julien validated this revised MYB-161 checkpoint on 2026-06-18. The validation accepts the revised direction as a checkpoint despite `Premium target reached: No`; it does not promote the result as final premium production art.

## Baseline

- before = MYB-160 after
- before scene: `Assets/Scenes/MYB160MeshyHeroCandidatePreview.unity`
- before route: `_bmad-output/visual-checkpoints/MYB-161/2026-06-17T19-26-11Z-before-route.png`
- before overview: `_bmad-output/visual-checkpoints/MYB-161/2026-06-17T19-26-11Z-before-overview.png`

## Builder

- path: `unity/Echapee4D/Assets/MYB161/Editor/MYB161ArtDirectedGoldenSliceBuilder.cs`
- seed: 161001
- generated root: `MYB161_ArtDirectedGoldenSliceRoot`
- output scene: `Assets/Scenes/MYB161ArtDirectedGoldenSlicePreview.unity`
- source scene: `Assets/Scenes/MYB160MeshyHeroCandidatePreview.unity`

## Art Direction Recipe

- baseline enclosure: MYB-159/MYB-160 generated art roots stay active to preserve lush near-camera canopy mass and softer forest mood.
- foreground frame: left ancient trunk assembly reinforces the preferred canopy enclosure without replacing it.
- near side mass: left-biased supporting tree assemblies and root clusters enrich the ride edge without random scatter.
- hero threshold: one restrained root/wood landmark idea remains, while the extra MYB-161 right-side Meshy tree clutter is removed.
- back wall: fewer grouped forest masses replace thin pole/picket silhouettes.
- background atmosphere: a small number of grouped silhouettes adds depth without a technical preview look.

## Layout Decisions

- foreground left ancient trunk: `MYB161_TreeAssembly_A`, X/offset approximately -6.7m, Z/meters 10.5.
- foreground right low root bank: `MYB161_ForegroundRight_LowRootMossBank`, offset +4.8m, meters 11.5.
- mid-left enclosure mass: `MYB161_TreeAssembly_C`, offset approximately -9.6m, meters 24.5, reinforcing forest ride enclosure.
- hero threshold: `MYB161_HeroThreshold_RootArchNaturalGate`, offset -6.25m, meters 29.5, using the cleaned MYB-160 Meshy root arch diagonally.
- back wall: 6 grouped side masses at offsets roughly +/-10m to +/-15m.

## Tree Assemblies

### MYB161_TreeAssembly_A

- role: Plan A foreground left ancient trunk frame
- visible from route: Yes
- canopy supported: Yes
- grounding: combined renderer bounds min.y, sink 0.03m
- notes: Foreground left frame. Big asymmetrical trunk, wider roots and grouped supported canopy lobes preserve the lush near-camera enclosure without closing the route.

### MYB161_TreeAssembly_B

- role: Plan B near left canopy reinforcement
- visible from route: Yes
- canopy supported: Yes
- grounding: combined renderer bounds min.y, sink 0.03m
- notes: Near left support mass. Keeps the baseline green tunnel feeling by adding supported canopy grouping instead of another isolated prop.

### MYB161_TreeAssembly_C

- role: Plan C mid-left forest enclosure mass
- visible from route: Yes
- canopy supported: Yes
- grounding: combined renderer bounds min.y, sink 0.03m
- notes: Mid-left enclosure mass. Restores the feeling of riding through a forest while staying outside the readable route corridor.

## Meshy Usage

- Used existing MYB-160 Meshy assets: Yes, via the preserved baseline scene and one restrained root arch candidate overlay.
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

- before route: `_bmad-output/visual-checkpoints/MYB-161/2026-06-17T19-26-11Z-before-route.png`
- before overview: `_bmad-output/visual-checkpoints/MYB-161/2026-06-17T19-26-11Z-before-overview.png`
- after route: `_bmad-output/visual-checkpoints/MYB-161/2026-06-17T19-26-12Z-after-route.png`
- after overview: `_bmad-output/visual-checkpoints/MYB-161/2026-06-17T19-26-12Z-after-overview.png`
- route comparison: `_bmad-output/visual-checkpoints/MYB-161/2026-06-17T19-26-12Z-route-before-after.png`
- overview comparison: `_bmad-output/visual-checkpoints/MYB-161/2026-06-17T19-26-13Z-overview-before-after.png`
- capture report: `_bmad-output/visual-checkpoints/MYB-161/2026-06-17T19-26-13Z-before-after-capture-report.md`

## MYB-144 Validation

- verdict: PASS
- errors: 0
- warnings: 0
- report: `_bmad-output/unity-test-results/myb-144-art-asset-validator-report.md`

## Visual Rubric Estimate

Scores are implementation estimates retained as context after Julien human validation. Julien validated the revised direction, not a final Premium target score.

| Criterion | Estimate | Notes |
|---|---:|---|
| Route readability | 4 | Route is kept open, with no overlap and minimum clearance 2.75m. |
| Silhouette quality | 3 | Baseline canopy enclosure is preserved and extra pole/picket silhouettes are reduced, but the project still lacks final bespoke forest forms. |
| Lighting mood | 3 | Softer baseline mood is preserved with only light scene-local support. |
| Material coherence | 3 | Scene-local palette is coherent enough for preview, not final art. |
| Foreground richness | 4 | Preferred lush near-camera canopy mass is retained, with restrained grounding support. |
| Midground density | 4 | Left enclosure and route edge masses keep the ride feeling like a forest instead of an asset preview. |
| Background depth | 3 | Grouped masses add depth, but remain preview-quality. |
| Scale credibility | 4 | Grounding, support and clearance metrics pass. |
| Composition rhythm | 3 | Revision favors the preferred baseline mood over the sparse first after; composition is safer but still not Premium. |

## Warning Categories

### Build / Capture Warnings

- None recorded.

### MYB-161 Visual Warnings

- Premium target intentionally not claimed; MYB-161 is an art-directed composition checkpoint pending Julien visual review.
- Julien validated the revised checkpoint on 2026-06-18; Premium target remains intentionally not claimed.
- Julien prefers the previous left/baseline mood over the first MYB-161 after. This revision preserves the baseline forest enclosure while keeping route readability improvements.
- MYB-161 revision keeps the human-preferred MYB-159/MYB-160 canopy enclosure active and uses MYB-161 as a restrained structural overlay.
- Blob canopy dominance remains a known risk because the preferred baseline relies on generous canopy masses; this revision avoids optimizing toward the sparse first after image.

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
- Checkpoint insuffisant, visually accepted by Julien on 2026-06-18
- Recommended Linear status: Done

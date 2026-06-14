# MYB-102 - Spike rendu URP global

Date: 2026-06-14

## Verdict

Ne pas modifier le `Socle de Rendu Projet` dans MYB-102 et ne pas ouvrir de
ticket d'implementation globale URP pour l'instant.

Le seuil valide pendant le grill-with-docs n'est pas atteint: il faudrait au
moins deux familles globales avec un gain visible et reutilisable sur les trois
Passages, sans dette evidente de performance ou de lisibilite. Les preuves
montrent un leger signal positif sur `Probe/Ambient`, mais `Project Depth` et
`Polish Volume` restent visuellement quasi identiques a la baseline.

## Evidence

Rapport genere:

- `_bmad-output/unity-test-results/myb-102/myb-102-urp-global-rendering-spike.txt`

Captures comparatives:

- `_bmad-output/unity-test-results/myb-102/myb-102-contact-sheet.png`
- `_bmad-output/unity-test-results/myb-102/baseline/passage-01-foret-claire.png`
- `_bmad-output/unity-test-results/myb-102/baseline/passage-02-village-route-de-col.png`
- `_bmad-output/unity-test-results/myb-102/baseline/passage-03-panorama-signal-fantasy.png`
- `_bmad-output/unity-test-results/myb-102/project-depth/passage-01-foret-claire.png`
- `_bmad-output/unity-test-results/myb-102/project-depth/passage-02-village-route-de-col.png`
- `_bmad-output/unity-test-results/myb-102/project-depth/passage-03-panorama-signal-fantasy.png`
- `_bmad-output/unity-test-results/myb-102/polish-volume/passage-01-foret-claire.png`
- `_bmad-output/unity-test-results/myb-102/polish-volume/passage-02-village-route-de-col.png`
- `_bmad-output/unity-test-results/myb-102/polish-volume/passage-03-panorama-signal-fantasy.png`
- `_bmad-output/unity-test-results/myb-102/probe-ambient/passage-01-foret-claire.png`
- `_bmad-output/unity-test-results/myb-102/probe-ambient/passage-02-village-route-de-col.png`
- `_bmad-output/unity-test-results/myb-102/probe-ambient/passage-03-panorama-signal-fantasy.png`

Unity documentation consulted by the spike report:

- URP Asset: https://docs.unity3d.com/Manual/urp/universalrp-asset.html
- URP Renderer Features: https://docs.unity3d.com/Manual/urp/urp-renderer-feature.html
- Volumes and post-processing: https://docs.unity3d.com/Manual/urp/volumes-landing.html
- Reflection Probes: https://docs.unity3d.com/Manual/class-ReflectionProbe.html

## Baseline observed

Current `PC_RPAsset` / `PC_Renderer` baseline:

- MSAA: `1`
- Render scale: `1`
- Shadow distance: `50`
- Shadow cascades: `4`
- Main light shadowmap resolution: `2048`
- Reflection probe blending: `true`
- Reflection probe box projection: `true`
- Renderer mode: `2`
- SSAO intensity: `0.4`
- SSAO radius: `0.3`
- SSAO samples: `1`

The baseline is not a bare URP default. It already has HDR, deferred renderer
mode, SSAO, soft shadows, reflection probe support and four cascades in the PC
pipeline asset. The visible weakness after MYB-104 is therefore not explained
by a single missing project-level URP switch.

## Variant comparison

| Variant | Family | Evidence | Verdict |
| --- | --- | --- | --- |
| Baseline actuelle | Baseline | Stable and readable, but still simple. | Reference only |
| Project Depth | Project defaults | Stronger MSAA/shadowmap/shadow range/SSAO produced negligible visible change in the 720p captures. | Do not adopt globally now |
| Polish Volume | Post-process baseline | Existing sample tone/bloom/vignette global volume produced no visible improvement in the capture path. | Do not adopt globally now |
| Probe/Ambient | Probe strategy | Slightly brighter Passages and fewer dark pixels across all three captures. Still not enough to change the overall quality bar alone. | Keep as local-scene follow-up candidate, not global default |

`Probe/Ambient` metrics reduced dark pixels from about `9.8-10.1%` to about
`8.7-8.8%` and raised average luminance by roughly `0.007-0.009` on the three
Passages. This is measurable but modest; it improves readability a little
without turning the scene into a different quality tier.

## Decision

No ADR is created for MYB-102 because the durable architectural decision is to
avoid a global change for now. The decision is intentionally reversible: a
future ticket can revisit the `Socle de Rendu Projet` after more scene-local
lighting/probe work provides stronger evidence.

Recommended next action:

- Keep `PC_RPAsset`, `PC_Renderer`, `QualitySettings`, `GraphicsSettings`, and
  the default project renderer strategy unchanged.
- Prefer local scene lighting/probe experiments when a visual ticket needs
  them, especially around ambient balance and passage-specific depth.
- Do not use global post-processing as a shortcut for `Lecture Prototype`;
  MYB-104's authored composition should remain the quality foundation.

## Validation

- `unity-mcp-cli status unity/Echapee4D --timeout 10000`: PASS.
- `MYB102UrpGlobalRenderingSpike.CaptureUrpGlobalRenderingSpikeCli()`: PASS via
  Unity-MCP script execution.
- Generated baseline + three temporary variants across three Passages.
- Restoration status in generated report: PASS.
- Confirmed no persisted diff in `Assets/Settings/PC_RPAsset.asset` or
  `Assets/Settings/PC_Renderer.asset`.
- `git diff --check`: PASS.

# MYB-95 - Preflight Meshy et Unity Bridge

Date: 2026-06-11
Ticket Linear: https://linear.app/kefjbo/issue/MYB-95/poc-meshy-modeles-3d-animes-custom-et-effort-dintegration-unity
Branche: `myb-95-meshy-preflight`
Credit Meshy consomme: 124

## Verdict court

Le POC Meshy est faisable dans `unity/Echapee4D`, et le MCP Meshy authentifie devient la voie principale pour MYB-95. Le preflight montre deux decisions a prendre avant generation:

- L'absence de `MESHY_API_KEY` dans l'environnement local bloque seulement les scripts API locaux; elle ne bloque pas MYB-95 si on utilise le MCP Meshy authentifie.
- Le plugin Unity Meshy installe par l'utilisateur fonctionne comme bridge d'import depuis le workspace Meshy vers Unity. Il est utile pour importer vite, mais il ne remplace pas le pipeline API si on veut mesurer cout, formats, temps, rigging et effort d'integration de facon reproductible.
- Le plugin Unity Meshy est declare en `GPL-3.0`. Avant de le versionner dans le repo, il faut decider si on accepte cette dependance comme outil de dev, ou si on garde le bridge installe localement et qu'on commit seulement les assets produits/importes.

Recommandation: lancer MYB-95 avec generation via MCP Meshy, sortie prioritaire `fbx`, import Unity mesure dans un dossier sandbox `Assets/Echappee/Art/MYB95MeshyPoc/`, et utiliser le bridge Unity uniquement comme comparaison d'ergonomie ou secours d'import.

## Preuves locales

- Branche dediee creee: `myb-95-meshy-preflight`.
- Linear `MYB-95` passe en `In Progress`.
- `unity-mcp-cli status unity/Echapee4D --timeout 10000`: MCP reachable, Unity Editor actif.
- `assets-refresh` Unity: succes, `AssetDatabase refreshed successfully`.
- Validateur baseline Unity: `MYB91.Editor.MYB91CanonicalBaselineValidator.ValidateCanonicalBaseline()` -> `PASS`.
- Rapport baseline: `_bmad-output/unity-test-results/myb-91-canonical-baseline.txt`.
- `meshy_check_balance`: succes, balance initiale `1490` credits.
- Appels Meshy payants effectues avec validation utilisateur explicite:
  preview 20 credits, refine 10 credits, remesh comparaison 5 credits,
  text-to-image 9 credits, image-to-3D compare 15 + 30 credits,
  auto-rigging candidate_120k 5 credits.
- Balance Meshy apres rigging candidate_120k: `1396` credits.

## Etat credentials

- `MESHY_API_KEY`: absent de l'environnement courant.
- `OPENAI_API_KEY`: absent de l'environnement courant.
- MCP Meshy: authentifie, balance `1490` credits.
- Scan local sur `.agents/skills` et `unity/Echapee4D/Assets/Packages/ai.meshy`: pas de cle reelle detectee, seulement des exemples et instructions.

Consequence: les scripts locaux qui lisent `MESHY_API_KEY` restent inutilisables tels quels, mais MYB-95 peut passer par le MCP Meshy authentifie.

## Skills Meshy disponibles

Les nouveaux skills sont installes et verrouilles dans `skills-lock.json`:

- `.agents/skills/meshy-3d-agent/`
- `.agents/skills/meshy-3d-generation/`

Ils couvrent text-to-3d, image-to-3d, multi-image, retexture, remesh, rigging et animation. Les instructions locales imposent de confirmer explicitement le cout avant tout appel consommant des credits.

## Plugin Unity Meshy installe

Fichiers detectes:

- `unity/Echapee4D/Assets/Packages/ai.meshy/package.json`
- `unity/Echapee4D/Assets/Packages/ai.meshy/README.txt`
- `unity/Echapee4D/Assets/Packages/ai.meshy/Editor/Script/MeshyBridgeWindow.cs`
- `unity/Echapee4D/Assets/Packages/ai.meshy/Editor/Script/MeshyAssembly.asmdef`

Constats:

- Package local: `ai.meshy`, version `0.2.1`, display name `Meshy Bridge`.
- Menu Unity expose: `Meshy/Bridge`.
- Le bridge ouvre un listener local sur le port `5326`.
- Le plugin telecharge puis importe les modeles via `AssetDatabase.ImportAsset`.
- Le dossier cible du bridge est `Assets/MeshyImports/`.
- Le README local presente le plugin comme "Bridge Edition" et annonce le support GLB, FBX et ZIP.
- Le plugin declare une dependance `com.unity.formats.fbx: 4.1.3`, mais cette dependance n'est pas dans `unity/Echapee4D/Packages/manifest.json` ni `packages-lock.json`.
- Le script compile et le refresh Unity passe dans l'etat local actuel malgre cette dependance non versionnee.
- Licence plugin: `GPL-3.0`; notice tierce: `glTFast` sous Apache 2.0.

Point de vigilance: si on commit le plugin lui-meme, la licence GPL-3.0 doit etre acceptee consciemment. Sinon, garder le plugin comme outil local et versionner seulement les assets finalises.

## Verification docs Meshy

Sources officielles consultees le 2026-06-11:

- Pricing API: https://docs.meshy.ai/en/api/pricing
- Unity plugin introduction: https://docs.meshy.ai/en/unity-plugin/introduction
- Bridge to Unity: https://docs.meshy.ai/en/unity-plugin/bridge-to-unity
- Animated models Unity: https://docs.meshy.ai/en/unity-plugin/animated-models
- Asset retention: https://docs.meshy.ai/en/api/asset-retention
- Rigging API: https://docs.meshy.ai/en/api/rigging

Ce que les docs confirment:

- Le plugin Unity sert a importer des modeles du workspace Meshy vers Unity via "Send to Unity".
- Le bridge Unity est annonce pour les membres Pro et plus.
- Les modeles animes sont supportes par import et les donnees d'animation sont preservees.
- Les assets generes par API sont conserves au maximum 3 jours hors Enterprise; il faut telecharger et stocker localement les resultats utiles.
- Le rigging programmatique cible surtout les humanoides bipedes textures; les modeles non humanoides ou trop ambigus sont a risque.
- Les modeles avec plus de 300 000 faces ne sont pas supportes en rigging via `input_task_id`.

## Cout API a confirmer avant chaque etape

Snapshot docs officielles 2026-06-11:

- Text to 3D Preview: 20 credits pour Meshy-6/low poly, 5 credits pour les autres modeles.
- Text to 3D Refine: 10 credits.
- Image to 3D: 20/30 credits pour Meshy-6 sans/avec texture, 5/15 credits pour les autres modeles.
- Multi Image to 3D: meme ordre que Image to 3D.
- Retexture: 10 credits.
- Remesh: 5 credits.
- Auto-rigging: 5 credits.
- Animation: 3 credits.
- Text to Image: 3 a 9 credits selon modele.

Regle MYB-95: annoncer le cout maximum de l'etape suivante et attendre validation explicite avant appel API payant.

## Plan POC recommande

### Cas simple

Objectif: un prop premium statique ou legerement anime par Unity, par exemple une borne-lanterne de village ou un petit signal magique de route.

- Generation: text-to-3d avec sortie `fbx`.
- Texture/refine: oui si preview exploitable.
- Animation: pas de rig Meshy; animation Unity simple sur GameObject enfant, lumiere, emissive ou rotation lente.
- Mesure: temps generation, taille FBX/textures, import Unity, rendu URP, prefab, nombre de renderers/materials.
- Cout a confirmer: preview 5 a 20 credits + refine 10 credits; remesh 5 credits seulement si necessaire.

### Cas moyen

Objectif: un humanoide stylise utilisable comme signal premium route-side, cree en T-pose puis rigge.

- Generation: text-to-3d avec `pose_mode: t-pose`, sortie `fbx`.
- Texture/refine: oui.
- Rigging: Meshy auto-rigging pour recuperer idle/walk/run si le modele est humanoide propre.
- Import Unity: verifier clips, avatar/rig, prefab, Animator minimal.
- Mesure: taux de succes rig, qualite deformation, effort correctif Unity, poids asset.
- Cout a confirmer: preview 5 a 20 + refine 10 + rig 5; remesh 5 si > budget ou import lourd.

### Cas super chiade

Objectif: un signal fantasy premium spectaculaire pour foret claire ou village/campagne pavee, avec modele detaille, shaders/lumieres/particles Unity et animation visible.

- Generation: text-to-3d ou image-to-3d selon la direction artistique choisie.
- Retexture/remesh: selon resultat.
- Animation: soit rig humanoide si applicable, soit animation Unity + VFX sur plusieurs sous-objets.
- Import Unity: prefab complet dans une mini-scene MYB-95, capture et analyse visuelle/technique.
- Mesure: cout total, temps de boucle, volume de corrections manuelles, impact perf, reutilisabilite.
- Cout a confirmer: probablement 35 a 60+ credits selon generation, refine, remesh, retexture, rig/animation.

## Decisions avant generation

- Utiliser le MCP Meshy authentifie comme pipeline principal de generation.
- Fournir `MESHY_API_KEY` localement seulement si on veut executer ou comparer les scripts API locaux.
- Decider si le plugin Unity Meshy GPL-3.0 peut etre committe, ou s'il reste outil local non versionne.
- Si le plugin est committe, rendre sa dependance FBX explicite dans `Packages/manifest.json` ou documenter pourquoi elle n'est pas necessaire.
- Garder la sortie principale en `fbx` pour Unity; `glb` peut rester format secondaire pour Meshy/rigging mais ne doit pas devenir dependance d'import Unity tant qu'on ne valide pas le support.
- Creer `Assets/Echappee/Art/MYB95MeshyPoc/` seulement au moment de la premiere generation/import validee.

## Notes de preflight

- Une requete de logs Unity "toutes severites" avec `logTypeFilter: null` a genere une erreur MCP de validation de parametre. Les checks propres avant cette requete ne remontaient aucun warning ni erreur apres refresh. Cette erreur est du bruit d'outillage, pas un symptome du plugin Meshy.
- Les modifications de materiaux MYB-53 presentes dans le worktree sont des changements de serialisation Unity preexistants au preflight Meshy, avec un point a surveiller: `_EMISSION` retire de `MYB53_RelicWarmLight`.

## Preview 1 - Borne-lanterne village pave

Date: 2026-06-11
Decision utilisateur: concept valide avant appel Meshy, sortie `fbx`, preview Meshy-6.

Prompt valide:

```text
Single premium stylized fantasy roadside village lantern marker: a waist-high carved warm limestone bollard made of stacked beveled stone blocks, rounded square base, moss in cracks, worn tactile edges. On top, an elegant brass lantern cage with four amber glass panels, polished brass rim and handle, visible small irregular flame core inside. Add one tiny blue rune accent carved on the front stone. Handcrafted high-quality Unity game prop, clear silhouette, not cheap low-poly, not toy-like, isolated object.
```

Execution:

- Tool: `meshy_text_to_3d`
- Task ID: `019eb7af-0ff6-78b8-9109-02885cfb5537`
- Model: `meshy-6`
- Format: `fbx`
- Cost confirmed before call: 20 credits
- Balance before: 1490 credits
- Balance after: 1470 credits
- Status: `SUCCEEDED`
- Wait time: 119 seconds
- Local folder:
  `meshy_output/20260611_191606_single-premium-stylized-fantas_019eb7af/`
- Local FBX:
  `meshy_output/20260611_191606_single-premium-stylized-fantas_019eb7af/preview.fbx`
- Local thumbnail:
  `meshy_output/20260611_191606_single-premium-stylized-fantas_019eb7af/thumbnail.png`

Blender inspection:

- FBX version: 7400
- File size: 35,073,980 bytes
- Mesh objects: 1
- Vertices: 408,166
- Faces / estimated triangles: 816,457
- Materials in preview FBX: 0
- Bounds: about 0.46m x 0.49m x 1.0m after auto-size

Visual verdict:

- Positive: the silhouette reads clearly as a lantern on a stone pedestal, close to the validated concept's general structure.
- Missing at preview stage: warm stone/brass/glass material read, blue rune accent, flame color, moss color, and Unity lighting intent. This is expected for an untextured preview.
- Risk: polygon count is far above a comfortable route-side prop budget. If this asset is kept, plan a remesh/optimization pass before V1 use.
- Recommendation before refine: user visual review of the thumbnail. If accepted, run `meshy_text_to_3d_refine` for 10 credits, then import into a Unity sandbox with a real animated Point Light.

## Refine 1 - Borne-lanterne village pave

Date: 2026-06-11
Decision utilisateur: silhouette preview validee, refine texturé lance.

Texture prompt:

```text
Warm aged limestone blocks with moss in cracks, polished brass lantern cage and handle, amber translucent glass panels, small warm flame core inside, one tiny glowing blue rune carved into the front stone, premium stylized handcrafted fantasy Unity prop, painterly but tactile materials, no cheap plastic look.
```

Execution:

- Tool: `meshy_text_to_3d_refine`
- Preview task ID: `019eb7af-0ff6-78b8-9109-02885cfb5537`
- Refine task ID: `019eb7b4-e8d8-7b20-8cd8-4725c8279863`
- Model: `meshy-6`
- Format: `fbx`
- PBR: enabled
- Cost confirmed before call: 10 credits
- Balance before: 1470 credits
- Balance after: 1460 credits
- Status: `SUCCEEDED`
- Local FBX:
  `meshy_output/20260611_191606_single-premium-stylized-fantas_019eb7af/refined.fbx`
- Local textures:
  - `refined_base_color.png`
  - `refined_metallic.png`
  - `refined_roughness.png`
  - `refined_normal.png`
  - `refined_emission.png`
- Local capture:
  `_bmad-output/meshy-captures/myb-95-lantern-refined.png`

File and texture inspection:

- Refined FBX size: 28,941,692 bytes
- Texture maps: 2048 x 2048
- Base color: 8.2 MB
- Normal: 8.8 MB
- Roughness: 1.4 MB
- Metallic: 784 KB
- Emission: 94 KB

Blender inspection:

- FBX version: 7400
- Mesh objects: 1
- Vertices: 270,672
- Faces / estimated triangles: 541,618
- Materials: 1 (`Material.001`)
- Embedded/imported texture seen by Blender: `texture_0.png`, 2048 x 2048
- Bounds: about 0.18m x 0.20m x 0.40m after refine auto-size

Visual verdict:

- Positive: the refined model has a strong readable lantern silhouette, visible internal flame, warm brass/gold cage, detailed stone base and premium handcrafted feel. It is much closer to the target than the free asset fallback tested in MYB-53.
- Mismatch: the base reads more as dark/black and reddish irregular rocks than warm limestone blocks.
- Mismatch: the tiny blue rune accent is not clearly visible in the generated capture.
- Technical risk: geometry remains too dense for repeated route-side use without optimization. It may still be acceptable for a single hero prop or a POC import, but not as-is for multiple instances.
- Recommended next step: import only into a MYB-95 Unity sandbox, add a real animated `Point Light` / emissive flicker, capture in-engine, then decide whether to remesh for a target budget.

## Blender MCP setup

Date: 2026-06-11
Goal: prepare a free Blender-based optimization path before spending Meshy remesh credits.

Source reviewed:

- GitHub: https://github.com/ahujasid/blender-mcp

Setup performed:

- `uvx` already installed at `/Users/jbodin/.local/bin/uvx`.
- `blender-mcp` verified with uv-managed Python 3.11.
- Codex MCP config updated in `/Users/jbodin/.codex/config.toml`:
  - server: `blender`
  - command: `/Users/jbodin/.local/bin/uvx`
  - args: `--python 3.11 blender-mcp`
  - env: `BLENDER_HOST=localhost`, `BLENDER_PORT=9876`,
    `DISABLE_TELEMETRY=true`, `UV_PYTHON_PREFERENCE=only-managed`
- Codex config backup:
  `/Users/jbodin/.codex/config.toml.backup-blender-mcp-20260611T174531Z`
- Blender addon updated from the official `addon.py` into:
  `/Users/jbodin/Library/Application Support/Blender/5.1/scripts/addons/blender_mcp_addon.py`
- Previous Blender addon backup:
  `/Users/jbodin/Library/Application Support/Blender/5.1/scripts/addons/blender_mcp_addon.py.backup-20260611T174640Z`
- Addon enabled and user preferences saved for Blender 5.1.
- Blender GUI launched; addon server is listening on `127.0.0.1:9876`.
- Socket smoke test: `get_scene_info` returned a successful scene response.

Notes:

- No Blender operation was applied to the Meshy model.
- The Blender addon refuses to start its server in `blender --background`, so the
  MCP requires Blender GUI to be open.
- The newly added Codex MCP server may require a Codex restart or MCP reload
  before `mcp__blender` tools are exposed in the active tool list.

## Blender optimization pass 1

Date: 2026-06-11
Goal: reduce the Meshy refined lantern from about 542k faces to practical Unity
prop budgets without spending Meshy remesh credits.

MCP documentation reviewed:

- https://github.com/ahujasid/blender-mcp

Doc notes followed:

- The MCP exposes scene/object inspection, viewport screenshots and arbitrary
  Blender Python execution.
- Complex operations should be split into smaller steps.
- `execute_blender_code` is powerful and should be used carefully.
- Telemetry remains disabled through `DISABLE_TELEMETRY=true`.

Source asset:

- `meshy_output/20260611_191606_single-premium-stylized-fantas_019eb7af/refined.fbx`
- Source metrics in Blender:
  - 270,672 vertices
  - 541,618 faces / estimated triangles
  - 1 material

Method:

- Imported the refined FBX through Blender MCP.
- Preserved the high-poly source object in the `.blend` scene.
- Created decimated copies with Blender's Decimate modifier (`COLLAPSE`) and
  applied Weighted Normals.
- Exported FBX variants plus copied the existing PBR maps next to them.
- No Meshy remesh credit was used.

Output folder:

- `meshy_output/20260611_191606_single-premium-stylized-fantas_019eb7af/blender_optimized/`

Generated files:

- `myb95_lantern_lod0_30k.fbx`
- `myb95_lantern_lod0_20k.fbx`
- `myb95_lantern_lod1_12k.fbx`
- `myb95_lantern_lod2_5k.fbx`
- `myb95_lantern_optimization.blend`
- `optimization_metrics.json`
- copied texture maps:
  - `refined_base_color.png`
  - `refined_metallic.png`
  - `refined_roughness.png`
  - `refined_normal.png`
  - `refined_emission.png`

Metrics:

| Variant | Faces | Vertices | Face reduction | FBX size |
| --- | ---: | ---: | ---: | ---: |
| Source refined | 541,618 | 270,672 | - | 28.9 MB |
| LOD0 30k | 29,999 | 14,814 | 94.46% | 1.37 MB |
| LOD0 20k | 19,999 | 9,810 | 96.31% | 0.95 MB |
| LOD1 12k | 11,999 | 5,810 | 97.78% | 0.60 MB |
| LOD2 target 5k | 10,823 | 5,222 | 98.00% | 0.54 MB |

Validation:

- Re-imported `myb95_lantern_lod0_20k.fbx` in Blender:
  - 1 mesh object
  - 9,810 vertices
  - 19,963 faces
  - material present
- `git diff --check`: pending final run after this documentation update.

Captures:

- Comparison:
  `_bmad-output/meshy-captures/myb-95-lantern-blender-optimized-comparison.png`
- Candidate LOD0 20k:
  `_bmad-output/meshy-captures/myb-95-lantern-blender-optimized-20k.png`

Visual verdict:

- `LOD0 20k` is the best current candidate: strong visual reduction while
  preserving the lantern silhouette, crossed cage, roof, flame and stone base.
- `LOD0 30k` keeps slightly more detail but may be unnecessary for a route-side
  reusable prop.
- `LOD1 12k` is usable as a distance LOD but visibly facets the stone base and
  loses some premium softness.
- The `LOD2 target 5k` pass bottomed out at about 10.8k faces with this simple
  decimate method; a more aggressive handmade LOD would need a dedicated retopo
  or selective part-based simplification.

Recommendation:

- Use `myb95_lantern_lod0_20k.fbx` as the first Unity sandbox import candidate.
- Keep `myb95_lantern_lod0_30k.fbx` as fallback if 20k loses too much quality in
  Unity lighting.
- Use `myb95_lantern_lod1_12k.fbx` as a future LOD1 candidate.
- Do not use the original 542k refined FBX directly in Unity except as a
  high-poly reference.

## Meshy remesh comparison 1

Date: 2026-06-11
Goal: compare Blender's free optimization against a Meshy remesh at the same
target budget.

Execution:

- Tool: `meshy_remesh`
- Input task ID: `019eb7b4-e8d8-7b20-8cd8-4725c8279863`
- Remesh task ID: `019eb7d8-490e-75a8-9d3a-6846e0c31ea4`
- Target polycount: 20,000
- Topology: `triangle`
- Format: `fbx`
- Cost confirmed before call: 5 credits
- Balance before: 1460 credits
- Balance after: 1455 credits
- Status: `SUCCEEDED`

Outputs:

- Local FBX:
  `meshy_output/20260611_191606_single-premium-stylized-fantas_019eb7af/remeshed.fbx`
- Local textures:
  - `remeshed_base_color.png`
  - `remeshed_metallic.png`
  - `remeshed_roughness.png`
  - `remeshed_normal.png`
  - `remeshed_metallic_roughness.png`
- Comparison metrics:
  `meshy_output/20260611_191606_single-premium-stylized-fantas_019eb7af/blender_optimized/meshy_remesh_comparison_metrics.json`
- Captures:
  - `_bmad-output/meshy-captures/myb-95-lantern-blender-vs-meshy-remesh.png`
  - `_bmad-output/meshy-captures/myb-95-lantern-meshy-remesh-20k.png`

Metrics:

| Variant | Faces | Vertices | FBX size | Visual |
| --- | ---: | ---: | ---: | --- |
| Blender LOD0 20k | 19,999 | 9,810 | 0.95 MB | Preserves silhouette and materials acceptably |
| Meshy remesh 20k | 18,487 | 9,214 | 11.93 MB | Visibly broken / noisy surfaces and texture/shading artifacts |

Verdict:

- Meshy remesh respected the target budget numerically, but the visual result is
  not acceptable for Unity import.
- The Meshy remesh FBX is also much heavier than the Blender 20k FBX despite a
  similar face count.
- Likely issue: texture/UV reprojection or material/shading artifacts during
  remesh.
- Keep Blender LOD0 20k as the recommended Unity sandbox candidate.
- Do not spend more credits on Meshy remesh for this asset unless a different
  configuration is explicitly chosen for investigation.

## Unity import pass 1

Date: 2026-06-11
Goal: import the recommended Blender `LOD0 20k` candidate into the canonical
Unity project and prove a reusable prefab with real animated light.

Unity asset folder:

- `unity/Echapee4D/Assets/Echappee/Art/MYB95MeshyLantern/`

Imported candidate:

- Source: `myb95_lantern_lod0_20k.fbx`
- Unity path:
  `Assets/Echappee/Art/MYB95MeshyLantern/Models/MYB95_MeshyLantern_LOD0_20k.fbx`
- Texture maps copied:
  - `refined_base_color.png`
  - `refined_metallic.png`
  - `refined_roughness.png`
  - `refined_normal.png`
  - `refined_emission.png`

Unity implementation:

- Runtime script:
  `Assets/MYB95/Runtime/MYB95LanternFlicker.cs`
- Editor builder/validator:
  `Assets/MYB95/Editor/MYB95MeshyLanternUnityImporter.cs`
- Prefab:
  `Assets/Echappee/Art/MYB95MeshyLantern/Prefabs/MYB95_MeshyLantern.prefab`
- Scene:
  `Assets/Scenes/MYB95MeshyLanternPoc.unity`
- Validation report:
  `_bmad-output/unity-test-results/myb-95-meshy-lantern-unity-import.txt`
- Validation capture:
  `_bmad-output/unity-test-results/myb-95-meshy-lantern-unity-import.png`

Validation:

- `unity-mcp-cli status unity/Echapee4D --timeout 10000`: PASS
- `MYB95MeshyLanternUnityImporter.BuildAndValidateCli`: PASS
- Model triangles: 19,999
- Prefab total triangles: 20,779
- Texture dimensions: 2048 x 2048
- Prefab renderers: 3
- Flicker intensity sample: 1.673 -> 1.733
- Canonical build scene remains `Assets/Scenes/MYB89UnityMcpProbe.unity`; the
  MYB-95 POC scene is intentionally not added to Build Settings.

Visual verdict:

- The Unity import is usable as a sandbox proof: the lantern reads well, the
  warm flame/glass signal survives the 20k optimization, and the route-side
  scale is plausible for a premium village/campagne pavee marker.
- A real Unity `Point Light` with scripted flicker is now part of the prefab,
  instead of relying on the static Meshy mesh alone.
- The small blue rune had to be added as a Unity-side child because Meshy did
  not preserve it clearly in the generated mesh.
- The base still reads more dark/red-rock than warm limestone. This is fine for
  the POC, but not final art direction.

Technical caveats:

- The roughness texture is imported and kept, but the current Unity material
  uses fixed smoothness. Production should pack metallic/smoothness or define a
  material workflow for Meshy PBR maps.
- The 2K maps are acceptable for this POC/hero prop proof, but repeated route
  use needs compression/atlasing/LOD policy.
- Meshy remesh is not recommended for this asset; Blender 20k remains the
  preferred optimization path.

## Model 2 - Humanoid route guardian reference

Date: 2026-06-11
Goal: prepare the medium-complexity POC case before spending Meshy credits:
humanoid generation, rigging, basic animation, and Unity import effort.

Reference generation:

- Tool: local Codex image generation, no Meshy credits consumed.
- User-selected option: option 2.
- Local reference image:
  `_bmad-output/meshy-captures/myb-95-character-reference-option-2.png`

Concept:

- Premium stylized fantasy route-side guide / path guardian.
- Strict T-pose, front view, clean neutral background.
- Moss green hooded short cape, ochre scarf, cream tunic, leather belt,
  satchel, boots, small brass lantern clipped to belt, cyan rune pendant.
- Intended biome fit: village/campagne pavee and foret claire.

Why option 2 is preferred:

- It keeps a clean rig-friendly humanoid silhouette.
- The cape is short enough to reduce rigging/animation risk compared with a
  long cloak.
- The lantern and rune are readable but not so ornate that image-to-3D should
  overfit tiny details.
- It is less fragile than the more detailed option 1 and less cloth-heavy than
  option 3.

Next Meshy comparison to confirm before any paid call:

- Either generate a Meshy text-to-image reference for the same prompt, then
  compare image quality before 3D conversion.
- Or use the selected local image directly in Meshy `image-to-3d` with
  `target_formats: ["fbx"]`, `enable_pbr: true`, `pose_mode: "t-pose"`,
  `origin_at: "bottom"`, and no Meshy remesh by default.

## Model 2 - Meshy text-to-image comparison attempt

Date: 2026-06-11
Goal: ask Meshy to generate its own image reference for comparison against the
local option 2 before spending image-to-3D credits.

Execution:

- Tool: `meshy_text_to_image`
- Task ID: `019eb7f6-b539-7e2c-aecf-814dce6d70fd`
- Model: `nano-banana-pro`
- Pose: `t-pose`
- Aspect ratio: `3:4`
- Cost confirmed before call: up to 9 credits
- Balance before: 1455 credits
- Balance after: 1446 credits
- Status: `SUCCEEDED`

Prompt:

```text
Rig-friendly premium stylized fantasy route-side guide character for a Unity scenic cycling game. Adult humanoid path guardian in strict T-pose, clean neutral background, full body visible. Moss green hooded short cape, ochre scarf, warm cream tunic, brown leather belt and satchel, sturdy boots, small brass lantern clipped to belt, tiny cyan rune pendant. Clear optimized 3D game silhouette, friendly calm face, not childish, no weapon, no long cloak, no staff, no wings, no cropped limbs, no busy background.
```

Tooling issue:

- Meshy accepted the task and reported `SUCCEEDED`.
- `meshy_get_task_status` returned only task status/progress and did not expose
  an image URL or local file path.
- `meshy_list_tasks` confirms the task exists and succeeded, but also does not
  expose the generated image URL.
- `meshy_download_model` does not support text-to-image tasks and returned
  `Task completed but no model URLs available.`
- No generated Meshy image file was found under `meshy_output/`.

Impact:

- The Meshy-vs-local visual comparison was blocked by MCP result retrieval, not
  by generation failure.
- The generated image was recovered through the direct Meshy API once the user
  added the API key in 1Password.
- Do not spend image-to-3D credits until the user explicitly validates whether
  to use the recovered Meshy image or the local option 2 reference.

## Meshy API key storage setup

Date: 2026-06-11
Goal: prepare a secure local API-key path to call the Meshy API directly when
the MCP response surface does not expose generated image URLs.

1Password item created:

- Vault: `Personal Ops`
- Item: `Meshy API`
- Item ID: `s43ubhxj5ebzjsfxzrdubp25mu`
- Category: `ApiCredentials`
- Concealed field: `MESHY_API_KEY`
- Secret reference for local command injection:
  `op://Personal Ops/Meshy API/MESHY_API_KEY`

Notes:

- The item was created with a placeholder value, then filled by the user
  directly in 1Password.
- Do not paste the Meshy API key in chat or commit it to the repository.
- Preferred path for future sessions: 1Password `workspace_command_run` with
  `envSecretRefs` to inject `MESHY_API_KEY` into a local script without
  revealing plaintext in the transcript.
- Local Codex config was updated to add the workspace trust manifest for
  1Password MCP:
  `/Users/jbodin/.onepassword-mcp/workspace-trust.json`.
- The active 1Password MCP process still needs a restart before
  `workspace_command_run` can use that new manifest. For this pass, a local
  Connect-backed wrapper injected the secret in memory and did not print it.

## Model 2 - Meshy image recovered through direct API

Date: 2026-06-11
Goal: recover the generated Meshy text-to-image output without exposing the API
key, then decide whether it is good enough for the next paid image-to-3D step.

Execution:

- Direct API wrapper:
  `_bmad-output/scripts/fetch_meshy_text_to_image_via_1password_connect.py`
- Meshy retrieval script:
  `_bmad-output/scripts/fetch_meshy_text_to_image.py`
- Secret source: 1Password Connect item `Personal Ops / Meshy API /
  MESHY_API_KEY`, injected in memory only.
- Task ID: `019eb7f6-b539-7e2c-aecf-814dce6d70fd`
- Balance check after retrieval: `1446` credits.
- Meshy credits consumed by retrieval: `0`.

Outputs:

- Recovered Meshy image:
  `_bmad-output/meshy-captures/myb-95-character-reference-meshy-text-to-image.png`
- Sanitized API summary:
  `_bmad-output/meshy-captures/myb-95-character-reference-meshy-text-to-image-summary.json`
- Image bytes: `1,120,232`
- API field path: `$.image_urls[0]`

Visual verdict:

- Positive: clean full-body stylized humanoid, neutral background, strong
  route-side guide read, hood/cape/scarf/tunic/belt/satchel/boots present,
  lantern present, cyan pendant present.
- Risk: the pose is closer to a relaxed T/A-pose than a strict T-pose, so
  rigging may need extra tolerance or regeneration.
- Risk: hands/fingers are a weak point in the reference and may produce noisy
  geometry.
- Recommendation: user visual validation before the next paid call. If accepted,
  use this recovered Meshy image for `image-to-3d` because it is already a
  clean full-body neutral-background reference. Otherwise, use the local option
  2 reference or run a revised text-to-image prompt before 3D conversion.

## Model 2 - Image-to-3D comparison, 15 credits vs 30 credits

Date: 2026-06-11
Goal: compare Meshy image-to-3D quality/cost for the humanoid route guardian
before deciding which output is worth taking into Unity/rigging work.

Decision utilisateur: run both variants for comparison:

- 15 credits: previous generation `meshy-5` with textures.
- 30 credits: `meshy-6` with textures.

Execution:

- Source image:
  `_bmad-output/meshy-captures/myb-95-character-reference-meshy-text-to-image.png`
- Script:
  `_bmad-output/scripts/run_meshy_image_to_3d_comparison_via_1password_connect.py`
- Secret source: 1Password Connect item `Personal Ops / Meshy API /
  MESHY_API_KEY`, injected in memory only.
- Format: `fbx`
- Texture/PBR: enabled.
- Pose mode requested: `t-pose`.
- Auto-size/origin: enabled, `origin_at: bottom`.
- Built-in remesh: disabled for both variants to compare raw Meshy output before
  Blender optimization.
- Balance before: `1446`
- Balance after: `1401`
- Observed credit delta: `45`

Tooling note:

- The Meshy MCP wrapper rejected the `meshy-5` call because it sent
  `remove_lighting`, a Meshy-6-only parameter, even when false.
- No task was created by those rejected MCP calls; the successful comparison was
  run through the direct API to omit Meshy-6-only fields for the 15-credit
  variant.

15-credit output:

- Task ID: `019eb80f-b877-7718-bf99-91aa522054dd`
- Status: `SUCCEEDED`
- Local folder:
  `meshy_output/20260611_210143_15-credit-meshy-5-textured_019eb80f/`
- FBX:
  `meshy_output/20260611_210143_15-credit-meshy-5-textured_019eb80f/model.fbx`
- Thumbnail:
  `meshy_output/20260611_210143_15-credit-meshy-5-textured_019eb80f/thumbnail.png`
- Texture maps: base color, metallic, roughness, normal.
- FBX size: `18,978,860` bytes.
- Blender metrics:
  - Mesh objects: `1`
  - Vertices: `199,579`
  - Faces / estimated triangles: `399,314`
  - Materials: `1`
  - Bounds: about `1.70m x 0.42m x 1.70m`

30-credit output:

- Task ID: `019eb813-2566-77e5-84a6-11487589cffc`
- Status: `SUCCEEDED`
- Local folder:
  `meshy_output/20260611_210528_30-credit-meshy-6-textured_019eb813/`
- FBX:
  `meshy_output/20260611_210528_30-credit-meshy-6-textured_019eb813/model.fbx`
- Thumbnail:
  `meshy_output/20260611_210528_30-credit-meshy-6-textured_019eb813/thumbnail.png`
- Texture maps: base color, metallic, roughness, normal, emission.
- FBX size: `19,182,572` bytes.
- Blender metrics:
  - Mesh objects: `1`
  - Vertices: `177,142`
  - Faces / estimated triangles: `354,370`
  - Materials: `1`
  - Bounds: about `1.71m x 0.40m x 1.75m`

Comparison artifacts:

- Summary:
  `_bmad-output/meshy-captures/myb-95-character-image-to-3d-comparison-summary.json`
- Blender metrics:
  `_bmad-output/meshy-captures/myb-95-character-image-to-3d-fbx-metrics.json`
- Thumbnail comparison:
  `_bmad-output/meshy-captures/myb-95-character-image-to-3d-15-vs-30-thumbnail-comparison.png`

Visual verdict:

- The 30-credit Meshy-6 output is clearly better: cleaner face, stronger
  lantern read, better satchel/belt detail, better preserved pendant, and more
  coherent costume proportions.
- The 15-credit output keeps the broad silhouette and colors, but the face is
  smeared, the lantern is weaker, and the costume reads more melted/noisy.
- Both outputs still inherit the weak hands/fingers from the reference.

Technical verdict:

- Both outputs are too dense for direct Unity route-side use or rigging without
  optimization: `399k` vs `354k` triangles.
- The 30-credit output is slightly lighter despite being visually better.
- The 30-credit output also includes an emission texture, which may help keep
  the cyan pendant / magical accents alive in Unity.

Recommendation:

- Keep the 30-credit Meshy-6 model as the candidate for the medium POC.
- Do not rig either output yet: both exceed Meshy's rigging limit of 300k faces.
- Next best step is a Blender optimization pass on the 30-credit model, targeting
  a rig/import candidate below 100k triangles and a lightweight LOD around
  20k-30k triangles.

## Model 2 - Blender optimization pass

Date: 2026-06-11
Goal: reduce the selected 30-credit Meshy-6 humanoid from `354k` triangles to
practical rig/import/LOD candidates without spending additional Meshy credits.

Source asset:

- Meshy task ID: `019eb813-2566-77e5-84a6-11487589cffc`
- Source FBX:
  `meshy_output/20260611_210528_30-credit-meshy-6-textured_019eb813/model.fbx`
- Source metrics:
  - Vertices: `177,142`
  - Faces / estimated triangles: `354,370`
  - FBX size: `19,182,572` bytes

Method:

- Script:
  `_bmad-output/scripts/optimize_meshy_character_blender.py`
- Imported the Meshy-6 FBX in Blender.
- Rebuilt a clean PBR material from downloaded texture maps instead of relying
  on Meshy's temporary `temp.fbm/texture_0.png` reference.
- Created decimated copies with Blender's Decimate modifier (`COLLAPSE`) and
  applied Weighted Normals.
- Exported FBX for Unity-side import and GLB for possible Meshy rigging/API
  follow-up.
- Copied texture maps next to the optimized outputs.
- Meshy credits consumed: `0`.

Output folder:

- `meshy_output/20260611_210528_30-credit-meshy-6-textured_019eb813/blender_optimized/`

Generated files:

- `myb95_character_rig_safe_250k.fbx` / `.glb`
- `myb95_character_candidate_120k.fbx` / `.glb`
- `myb95_character_candidate_80k.fbx` / `.glb`
- `myb95_character_lod0_50k.fbx` / `.glb`
- `myb95_character_lod1_30k.fbx` / `.glb`
- `myb95_character_optimization.blend`
- `optimization_metrics.json`
- copied texture maps:
  - `texture_0_base_color.png`
  - `texture_0_metallic.png`
  - `texture_0_roughness.png`
  - `texture_0_normal.png`
  - `texture_0_emission.png`

Metrics:

| Variant | Faces / tris | Vertices | Reduction | FBX size | GLB size |
| --- | ---: | ---: | ---: | ---: | ---: |
| Source Meshy-6 | 354,370 | 177,142 | - | 18.3 MB | - |
| Rig-safe 250k | 249,999 | 124,955 | 29.45% | 8.4 MB | 22.1 MB |
| Candidate 120k | 119,999 | 59,955 | 66.14% | 4.2 MB | 18.4 MB |
| Candidate 80k | 79,999 | 39,955 | 77.43% | 2.8 MB | 16.8 MB |
| LOD0 50k | 49,999 | 24,955 | 85.89% | 1.8 MB | 16.1 MB |
| LOD1 30k | 29,999 | 14,955 | 91.53% | 1.1 MB | 15.6 MB |

Captures:

- Contact sheet:
  `_bmad-output/meshy-captures/myb-95-character-blender-optimized-contact-sheet.png`
- Comparison alias:
  `_bmad-output/meshy-captures/myb-95-character-blender-optimized-comparison.png`
- Individual previews:
  `_bmad-output/meshy-captures/myb-95-character-blender-optimized-source_354k.png`
  and one PNG per variant.

Visual verdict:

- `Rig-safe 250k` is closest to source and now under Meshy's 300k rigging limit,
  but still heavier than needed for a Unity route-side character.
- `Candidate 120k` preserves the silhouette, face, hood, cape, lantern, belt and
  pendant very well; it is the safest current candidate for a first Unity import
  or rigging experiment.
- `Candidate 80k` remains surprisingly close visually and is the best current
  quality/performance compromise if we want to be more aggressive.
- `LOD0 50k` is still readable but begins to lose softness in the cloak and
  accessory details.
- `LOD1 30k` remains usable as a distance LOD only.

Recommendation:

- Use `candidate_120k` for the next rigging/import attempt if we want maximum
  chance of preserving the character.
- Use `candidate_80k` if we want to stress-test a more game-ready budget.
- Do not use the original 354k source directly for Meshy rigging; it exceeds the
  documented 300k face limit.

## Model 2 - Candidate 120k rigging

Date: 2026-06-11
Goal: rig the selected `candidate_120k` humanoid and download the model plus
basic walking/running animation files before Meshy asset URLs expire.

Decision utilisateur: use `candidate_120k` for the rigging attempt.

Execution:

- API: Meshy `/openapi/v1/rigging`
- Script:
  `_bmad-output/scripts/run_meshy_candidate_120k_rigging_via_1password_connect.py`
- Secret source: 1Password Connect item `Personal Ops / Meshy API /
  MESHY_API_KEY`, injected in memory only.
- Source GLB:
  `meshy_output/20260611_210528_30-credit-meshy-6-textured_019eb813/blender_optimized/myb95_character_candidate_120k.glb`
- Source metrics from Blender optimization: `119,999` tris, `59,955`
  vertices.
- Source GLB size: `19,318,840` bytes.
- Height sent to Meshy: `1.75m`.
- Rigging task ID: `019eb82f-f8ed-7b6d-85c1-4d99a5c3d553`
- Cost confirmed before call: `5` credits.
- Balance before: `1401`
- Balance after: `1396`
- Observed credit delta: `5`
- Status: `SUCCEEDED`

Output folder:

- `meshy_output/20260611_210528_30-credit-meshy-6-textured_019eb813/blender_optimized/rigging_candidate_120k/`

Downloaded outputs:

- `rigged_character.glb` - `13,126,768` bytes
- `rigged_character.fbx` - `15,074,332` bytes
- `walking.glb` - `13,139,540` bytes
- `walking.fbx` - `15,098,652` bytes
- `walking_armature.glb` - `65,480` bytes
- `running.glb` - `13,134,932` bytes
- `running.fbx` - `15,090,508` bytes
- `running_armature.glb` - `60,868` bytes

Artifacts:

- Sanitized summary:
  `_bmad-output/meshy-captures/myb-95-character-candidate-120k-rigging-summary.json`
- Per-output metadata:
  `meshy_output/20260611_210528_30-credit-meshy-6-textured_019eb813/blender_optimized/rigging_candidate_120k/metadata.json`

Import recommendation:

- User can now import `rigged_character.fbx`, `walking.fbx` and `running.fbx`
  through the Meshy Unity plugin or Unity import flow.
- The GLB files are kept as fallback/source comparison, but Unity validation
  should prioritize FBX first because MYB-95 has standardized Unity import
  measurements around FBX so far.
- Next validation after plugin import: confirm avatar/rig import, animation clip
  preservation, material mapping, bounds/scale, prefab creation and a short
  in-engine capture.

## Model 2 - Meshy Unity plugin import attempt

Date: 2026-06-11
Goal: test the Meshy web `Send to Unity` bridge after manually adding the model
to the Meshy workspace.

Imported Unity folder:

- `unity/Echapee4D/Assets/MeshyImports/Meshy_Model_20260611_215549/`

Imported files:

- `Meshy_AI__0611195544_texture.fbx`
- `Meshy_AI__0611195544_texture.png`
- `Material.001.mat`

Unity inspection:

- Unity refresh: success.
- Recent Unity errors: `0`.
- Recent Unity warnings during plugin import: `0`.
- Texture size: `2048x2048`.
- Material shader: `Universal Render Pipeline/Lit`.
- Imported mesh: `118,355` triangles, `76,381` vertices.
- Import type: `Generic`.
- Animation clips: `0`.
- `SkinnedMeshRenderer`: `0`.
- `Animator`: `0`.
- `bindposes`: `0`.
- `boneWeights`: `0`.
- Mesh bounds: about `(0.02, 0.00, 0.02)`, visually tiny / flattened.

Verdict:

- The plugin bridge works as a static textured import path.
- This specific bridge transfer did not preserve the rig or walking/running
  animations, so it is not sufficient for the animated character POC.
- Do not rely on the Meshy Unity plugin for animated MYB-95 validation until we
  identify the correct animated bridge workflow.

## Model 2 - Direct Unity import from local rigged FBX

Date: 2026-06-11
Goal: bypass the Meshy Unity plugin and import the downloaded local rigged FBX
outputs directly into `unity/Echapee4D`.

Source folder:

- `meshy_output/20260611_210528_30-credit-meshy-6-textured_019eb813/blender_optimized/rigging_candidate_120k/`

Unity asset folder:

- `unity/Echapee4D/Assets/Echappee/Art/MYB95MeshyCharacter/`

Copied source files:

- `MYB95_RouteGuardian_Rigged.fbx`
- `MYB95_RouteGuardian_Walking.fbx`
- `MYB95_RouteGuardian_Running.fbx`
- `MYB95_RouteGuardian_BaseColor.png`
- `MYB95_RouteGuardian_Metallic.png`
- `MYB95_RouteGuardian_Roughness.png`
- `MYB95_RouteGuardian_Normal.png`
- `MYB95_RouteGuardian_Emission.png`

Unity implementation:

- Editor builder/validator:
  `Assets/MYB95/Editor/MYB95MeshyCharacterDirectImporter.cs`
- Material:
  `Assets/Echappee/Art/MYB95MeshyCharacter/Materials/MYB95_RouteGuardian_PBR.mat`
- Animator controller:
  `Assets/Echappee/Art/MYB95MeshyCharacter/Animations/MYB95_RouteGuardian.controller`
- Prefab:
  `Assets/Echappee/Art/MYB95MeshyCharacter/Prefabs/MYB95_RouteGuardian_Direct.prefab`
- Scene:
  `Assets/Scenes/MYB95MeshyCharacterDirectImportPoc.unity`
- Validation report:
  `_bmad-output/unity-test-results/myb-95-meshy-character-direct-import.txt`
- Validation capture:
  `_bmad-output/unity-test-results/myb-95-meshy-character-direct-import.png`
- Walking animation frames:
  `_bmad-output/unity-test-results/myb-95-meshy-character-direct-import-frames/`
- Walking animation strip:
  `_bmad-output/unity-test-results/myb-95-meshy-character-direct-import-walking-strip.png`
- Walking animation video:
  `_bmad-output/unity-test-results/myb-95-meshy-character-direct-import-walking.mp4`

Validation:

- `MYB95MeshyCharacterDirectImporter.BuildAndValidateCli`: PASS.
- Recent Unity errors: `0`.
- Humanoid avatar loaded: `True`.
- Humanoid avatar valid: `True`.
- Humanoid avatar isHuman: `True`.
- `SkinnedMeshRenderer` count: `1`.
- `MeshFilter` count: `0`.
- Walking clip: `1.033s`.
- Running clip: `0.633s`.
- Scene preview samples Walking at `t=0.393s`.
- Walking animation preview: `12` sampled frames over the `1.033s` clip.
- Walking MP4 preview: H.264, `640x360`, `12 fps`, `48` frames, `4.0s`
  looped from the sampled frames.
- Prefab source triangles: `118,355`.
- Prefab source bounds: about `(1.825, 2.074, 0.91)`.
- Texture maps: `2048x2048`.

Visual verdict:

- The direct import path is usable for the animated character POC.
- The capture shows the character textured, front-facing, at route-side scale,
  and sampled in a walking pose.
- The animation strip/video confirms that Unity can sample and render the
  walking clip visually, not only list it as an imported clip.
- This path is reproducible from local downloaded Meshy outputs and does not
  depend on the Meshy Unity bridge.

Technical caveats:

- The rigged base FBX emits a Unity warning about an empty `0` frame clip on the
  rigged model itself; the actual `Walking` and `Running` clips import
  correctly from their dedicated FBX files.
- Triangle count `118,355` is acceptable for this medium POC hero/signal test,
  but still heavy for repeated route-side use. Future production use should
  validate `candidate_80k` or an LOD chain.
- Roughness is imported but not packed into Unity metallic/smoothness yet.

## Model 3 - Super premium relic-fountain plan

Date: 2026-06-11
Goal: prepare the "super chiade" POC case before spending the next Meshy
credits.

Local visual reference:

- Generated locally with Codex image generation in chat, no Meshy credits
  consumed.
- Stored at:
  `_bmad-output/meshy-captures/myb-95-model-3-relic-fountain-reference.png`
- Concept: sacred roadside relic-fountain / small portal for light forest and
  paved countryside village biomes.
- Visual read: warm limestone arch, carved paving base, moss, brass trims,
  blue crystal accents, suspended lanterns, glowing blue rune/water spiral.

Reference prompt:

```text
Premium stylized fantasy 3D game asset concept, single object on a clean neutral
background, full object visible, front three-quarter view: a spectacular sacred
roadside relic for a scenic cycling route, blending light forest and paved
countryside village aesthetics. An ancient warm limestone mini-portal fountain
with carved paving stones at the base, moss in cracks, polished brass bands,
floating translucent blue rune rings orbiting a small hovering water flame, tiny
suspended lantern beads, subtle magical water spiral rising from a basin,
tactile handcrafted materials, high detail but readable silhouette,
production-quality Unity game prop, not cheap, not placeholder, no text, no UI,
no people, no environment clutter.
```

Recommended implementation direction:

- Meshy should generate the physical premium prop only: limestone arch, paved
  base, basin, brass ornaments, carved details, moss and blue crystal anchors.
- Unity should own the fragile magical layer: animated blue rune rings, water
  spiral, emissive flicker, point lights, particles and bloom-friendly material
  tuning.
- Avoid asking Meshy to bake transparent water/rune rings as final geometry for
  the first pass. It is likely to increase mesh cleanup and make the result less
  controllable than Unity-authored VFX.

Proposed next paid step:

- Tool: `meshy_text_to_3d`.
- Target: preview only, `target_formats: ["fbx"]`.
- Exact proposed Meshy prompt:

```text
Premium stylized fantasy Unity game prop, single physical object on neutral
background: a sacred roadside relic fountain / small stone portal for a scenic
cycling route. Warm aged limestone block arch, carved paved base, small stone
basin, moss in cracks, polished brass bands and ornaments, small hanging lantern
hooks, embedded blue crystal sockets and carved rune grooves prepared for later
Unity glow/VFX. High detail handcrafted materials, readable silhouette,
production-quality, not cheap, not placeholder. No characters, no text, no UI,
no environment, no transparent water spiral, no floating rune rings, no particle
effects baked into geometry.
```

- Maximum cost to confirm before call: `20` Meshy credits.
- If preview is accepted: refine for `10` credits.
- First optimization path after refine: Blender decimate/LOD, not Meshy remesh
  by default.
- Unity integration after refine: prefab, material setup, animated VFX rings /
  water spiral / light flicker, capture and technical report.

Decision needed before any paid Meshy call:

- Validate or reject this relic-fountain concept as the third MYB-95 case.
- Confirm the next call budget: `20` credits max for preview only.

## Resume validation - 2026-06-11

Goal: resume the `myb-95-meshy-preflight` branch without spending additional
Meshy credits, verify the current Unity sandbox state, and make the local
tooling boundaries explicit.

Validation rerun:

- `unity-mcp-cli status unity/Echapee4D --timeout 10000`: PASS.
- `unity-mcp-cli run-tool assets-refresh unity/Echapee4D --input '{"options":"ForceSynchronousImport"}' --timeout 180000`: PASS.
- `MYB95MeshyLanternUnityImporter.BuildAndValidateCli`: PASS.
  - Report regenerated at `2026-06-11T21:28:27Z`.
  - Prefab total triangles: `20,779`.
  - Flicker intensity sample remains dynamic: `1.673 -> 1.733`.
- `MYB95MeshyCharacterDirectImporter.BuildAndValidateCli`: PASS.
  - Report regenerated at `2026-06-11T21:28:39Z`.
  - Humanoid avatar valid/isHuman: `True`.
  - Walking clip: `1.033s`; Running clip: `0.633s`.
  - Prefab source triangles: `118,355`.
- `git diff --check`: PASS before documentation updates.

Repository hygiene decision:

- `meshy_output/` is kept as local source/intermediate output storage and is now
  ignored because it is about `624 MB`.
- `.onepassword-mcp.json` is kept local and ignored; it contains workspace tool
  configuration rather than a secret, but should not be treated as a repo
  artifact.
- `unity/Echapee4D/Assets/Packages/ai.meshy/` and
  `unity/Echapee4D/Assets/MeshyImports/` are kept local and ignored for now.
  Reason: the plugin remains a GPL-3.0/versioning decision, and the bridge
  import attempt did not preserve rig/animation for the character POC.

Current blocker:

- The next MYB-95 action that advances the third "super premium" case is a paid
  Meshy preview call for the relic-fountain concept.
- Required explicit confirmation before proceeding: `20` Meshy credits maximum,
  preview only, target format `fbx`.

## Model 3 - Relic-fountain preview

Date: 2026-06-11
Goal: run the third "super chiade" MYB-95 case as a paid Meshy preview only,
after explicit user confirmation, without refine/remesh.

Decision utilisateur: preview Meshy confirmed, maximum `20` credits, target
format `fbx`, no refine.

Execution:

- Tool: `meshy_text_to_3d`
- Task ID: `019eb899-d4c1-7cd1-be10-99d53450dfe5`
- Model: `latest` / Meshy-6 preview
- Format: `fbx`
- Cost confirmed before call: `20` credits
- Balance before: `1396`
- Balance after: `1376`
- Observed credit delta: `20`
- Status: `SUCCEEDED`
- Wait time: `119` seconds
- Local folder:
  `meshy_output/20260611_233231_premium-stylized-fantasy-unity_019eb899/`
- Local FBX:
  `meshy_output/20260611_233231_premium-stylized-fantasy-unity_019eb899/preview.fbx`
- Local thumbnail:
  `meshy_output/20260611_233231_premium-stylized-fantasy-unity_019eb899/thumbnail.png`
- BMAD thumbnail copy:
  `_bmad-output/meshy-captures/myb-95-model-3-relic-fountain-preview-thumbnail.png`
- BMAD metrics:
  `_bmad-output/meshy-captures/myb-95-model-3-relic-fountain-preview-metrics.json`

Prompt used:

```text
Premium stylized fantasy Unity game prop: sacred roadside relic fountain / small stone portal for a scenic cycling route. Warm aged limestone arch, carved paving base, small stone basin, moss in cracks, polished brass bands and ornaments, small lantern hooks, embedded blue crystal sockets, carved rune grooves for later Unity glow/VFX. High detail handcrafted materials, readable silhouette, production quality. No characters, text, UI, environment, transparent water, floating rune rings, or baked particles.
```

Tooling note:

- The originally planned prompt exceeded Meshy's `600` character limit and was
  rejected by validation before task creation. No Meshy credit was consumed by
  that rejected attempt.

Blender inspection:

- FBX size: `27,355,404` bytes.
- Mesh objects: `1`.
- Vertices: `372,469`.
- Faces / estimated triangles: `745,209`.
- Materials: `0` in the untextured preview FBX.
- Bounds: about `1.83m x 1.23m x 2.0m` after auto-size.

Visual verdict:

- Positive: the silhouette strongly reads as a sacred roadside portal/fountain:
  stone arch, hanging lanterns, central basin, carved base and rune-like trim are
  all visible.
- Positive: Meshy followed the requested split reasonably well: the output is a
  physical prop, not baked transparent water/rune VFX geometry.
- Missing at preview stage: warm limestone, moss, brass, blue crystal and other
  material direction cannot be validated until refine.
- Technical risk: the preview mesh is much too dense for direct Unity import or
  repeated use. Any accepted refine must be followed by Blender optimization/LOD
  before Unity import.

Recommendation before next paid step:

- Ask for explicit approval before refine: `10` additional Meshy credits.
- If approved, refine with PBR and a texture prompt focused on warm limestone,
  moss cracks, polished brass, blue crystal sockets and carved rune grooves.
- Do not use Meshy remesh by default; use Blender optimization first if refine
  produces a keeper.

## Model 3 - Relic-fountain refine, Blender optimization and Unity import

Date: 2026-06-11
Goal: finish the third MYB-95 "super chiade" case after explicit approval:
refine the relic-fountain preview, optimize locally in Blender, and prove direct
Unity import with a sandbox prefab and validation scene.

Decision utilisateur: approved `10` Meshy credits for refine plus local Blender
optimization, direct Unity import, and the planned validation work.

Refine execution:

- Tool: `meshy_text_to_3d_refine`
- Preview task ID: `019eb899-d4c1-7cd1-be10-99d53450dfe5`
- Refine task ID: `019eb89e-7ed1-7dbf-b1d5-42f5d1931b44`
- Model: `latest` / Meshy-6 refine
- Format: `fbx`
- PBR: enabled
- Cost confirmed before call: `10` credits
- Balance before: `1376`
- Balance after: `1366`
- Observed credit delta: `10`
- Status: `SUCCEEDED`
- Local FBX:
  `meshy_output/20260611_233231_premium-stylized-fantasy-unity_019eb899/refined.fbx`
- Local textures:
  - `refined_base_color.png`
  - `refined_metallic.png`
  - `refined_roughness.png`
  - `refined_normal.png`
  - `refined_emission.png`
- BMAD refined metrics:
  `_bmad-output/meshy-captures/myb-95-model-3-relic-fountain-refined-metrics.json`

Refine prompt:

```text
Warm aged limestone blocks, moss in cracks, carved paving stones, polished brass bands and hanging lantern hooks, subtle blue crystal sockets and carved rune grooves, handcrafted premium fantasy Unity prop materials, tactile stone and metal, no cheap plastic, no baked transparent water or particle effects.
```

Refined Blender metrics:

- FBX size: `33,613,340` bytes.
- Mesh objects: `1`.
- Vertices: `360,354`.
- Faces / estimated triangles: `720,861`.
- Materials: `1` (`Material.001` in the source; rebuilt as
  `MYB95_RelicFountain_PBR` during optimization).
- Texture maps: 2048 x 2048.
- Bounds: about `1.83m x 1.23m x 2.0m` after auto-size.

Blender optimization:

- Script:
  `_bmad-output/scripts/optimize_meshy_relic_fountain_blender.py`
- Source:
  `meshy_output/20260611_233231_premium-stylized-fantasy-unity_019eb899/refined.fbx`
- Output folder:
  `meshy_output/20260611_233231_premium-stylized-fantasy-unity_019eb899/blender_optimized/`
- Meshy credits consumed by optimization: `0`.
- BMAD optimization metrics:
  `_bmad-output/meshy-captures/myb-95-model-3-relic-fountain-blender-optimized-metrics.json`
- BMAD comparison capture:
  `_bmad-output/meshy-captures/myb-95-model-3-relic-fountain-blender-optimized-comparison.png`

Optimization metrics:

| Variant | Faces / tris | Vertices | Reduction | FBX size |
| --- | ---: | ---: | ---: | ---: |
| Source refined | 720,861 | 360,354 | - | 32.1 MB |
| Hero 80k | 79,999 | 39,921 | 88.90% | 2.9 MB |
| LOD0 50k | 50,000 | 24,920 | 93.06% | 1.8 MB |
| LOD1 30k | 29,998 | 14,918 | 95.84% | 1.1 MB |
| LOD2 15k | 14,998 | 7,418 | 97.92% | 615 KB |

Unity import:

- Unity asset folder:
  `unity/Echapee4D/Assets/Echappee/Art/MYB95MeshyRelicFountain/`
- Runtime script:
  `Assets/MYB95/Runtime/MYB95RelicFountainPulse.cs`
- Editor builder/validator:
  `Assets/MYB95/Editor/MYB95MeshyRelicFountainUnityImporter.cs`
- Imported models:
  - `Assets/Echappee/Art/MYB95MeshyRelicFountain/Models/MYB95_RelicFountain_Hero_80k.fbx`
  - `Assets/Echappee/Art/MYB95MeshyRelicFountain/Models/MYB95_RelicFountain_LOD0_50k.fbx`
  - `Assets/Echappee/Art/MYB95MeshyRelicFountain/Models/MYB95_RelicFountain_LOD1_30k.fbx`
- Prefab:
  `Assets/Echappee/Art/MYB95MeshyRelicFountain/Prefabs/MYB95_RelicFountain.prefab`
- Scene:
  `Assets/Scenes/MYB95MeshyRelicFountainPoc.unity`
- Validation report:
  `_bmad-output/unity-test-results/myb-95-meshy-relic-fountain-unity-import.txt`
- Validation capture:
  `_bmad-output/unity-test-results/myb-95-meshy-relic-fountain-unity-import.png`

Unity validation:

- `unity-mcp-cli run-tool assets-refresh unity/Echapee4D --input '{"options":"ForceSynchronousImport"}' --timeout 180000`: PASS.
- `MYB95MeshyRelicFountainUnityImporter.BuildAndValidateCli`: PASS.
- Unity version: `6000.4.10f1`.
- LODGroup levels: `3`.
- Hero triangles: `79,999`.
- LOD0 triangles: `50,000`.
- LOD1 triangles: `29,998`.
- Prefab renderers: `13`.
- Prefab lights: `2`.
- Pulse intensity sample: `1.589 -> 1.235`.
- Canonical build scene remains `Assets/Scenes/MYB89UnityMcpProbe.unity`;
  MYB-95 relic scene is intentionally not added to Build Settings.

Visual verdict:

- The refined asset is the strongest Meshy result in MYB-95 so far: readable
  stone arch, brass/black metal bands, hanging lanterns, central basin, carved
  base and route-side landmark silhouette.
- Meshy captured the physical prop well but the raw refined mesh remains much
  too dense for Unity use without optimization.
- Blender optimization is effective: the close LOD keeps a premium read at
  about `80k` triangles, while the `50k` and `30k` variants are practical
  fallback/distance LODs.
- Unity-side VFX ownership works better than asking Meshy to bake magic: blue
  energy frames, water glow, point lights and pulsing animation are controllable
  in the prefab and validate the intended production split.

Technical caveats:

- The roughness map is imported and kept, but Unity still uses fixed smoothness
  rather than a packed metallic/smoothness workflow.
- 2K maps are acceptable for this POC/hero landmark, but production needs a
  texture compression/LOD policy before repeated route use.
- The Unity-authored rune frames are intentionally simple validator-grade VFX;
  production art should replace them with shader/VFX graph or mesh-authored
  rings if this asset is adopted.

## Final verdict - ready for review

Date: 2026-06-12
Status: ready for review / no further Meshy call recommended in MYB-95.

Scope completion:

- Cas simple: borne-lanterne village pavee, generated/refined, optimized in
  Blender, imported directly into Unity with prefab, light flicker, capture and
  validator.
- Cas moyen: route guardian humanoid, image-to-3D compared across cost tiers,
  optimized in Blender, rigged via Meshy, imported directly into Unity with
  Humanoid avatar, walking/running clips, capture strip/video and validator.
- Cas super chiade: relic-fountain / small stone portal, generated/refined,
  optimized in Blender, imported directly into Unity with LODGroup, Unity-side
  energy/water/lights/pulse VFX, capture and validator.

Final cost:

- Total Meshy credits consumed in MYB-95: `124`.
- All paid calls were preceded by explicit user approval.
- No additional paid Meshy call is recommended for this ticket.

Adoption recommendation:

- Meshy is recommended as a **punctual premium landmark tool**, not as a bulk
  asset pipeline.
- Best supported asset classes for V1:
  - static/pseudo-animated hero props such as lanterns, relics, small portals,
    fountain markers and scenic route signals;
  - humanoid route-side figures only after image/reference control and a
    Blender optimization step before rigging/import.
- Recommended production-ish pipeline:
  1. prompt/reference discipline with explicit target format `fbx`;
  2. Meshy preview/refine only when the silhouette is worth keeping;
  3. local Blender decimation/LOD first;
  4. direct Unity FBX import from local downloaded outputs;
  5. Unity-authored lights, emissive accents and VFX for fragile magical layers;
  6. validator/capture evidence before adoption.

Do not adopt:

- Meshy raw refined meshes directly in Unity; they are consistently too dense.
- Meshy remesh by default; the tested lantern remesh respected triangle target
  but produced visual/texture artifacts and a heavier FBX than Blender.
- The Meshy Unity bridge as the validation path for animated assets yet; the
  tested bridge import preserved a static textured model but not rig/clips.
- `unity/Echapee4D/Assets/Packages/ai.meshy/` as a committed dependency until
  the GPL-3.0/versioning decision is explicit.

Best candidate from the POC:

- `MYB95_RelicFountain` is the strongest showcase result and the best candidate
  for a future integration ticket into a real ride scene.
- `MYB95_MeshyLantern` is a practical secondary candidate for repeated
  route-side premium markers after material/texture policy cleanup.
- `MYB95_RouteGuardian_Direct` proves the animated character path, but remains
  heavier and riskier than the prop/landmark workflow.

Recommended next ticket:

- Create or select a Unity art-integration ticket that takes one adopted MYB-95
  asset, probably the relic-fountain, into an actual ride scene with placement,
  camera readability, performance budget, material cleanup, LOD policy and
  macOS capture evidence.

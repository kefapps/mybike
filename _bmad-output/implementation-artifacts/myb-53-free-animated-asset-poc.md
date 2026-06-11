# MYB-53 - POC pack d'assets gratuits anime compatible Unity

Date: 2026-06-11
Ticket Linear: https://linear.app/kefjbo/issue/MYB-53/myb-021-poc-pack-dassets-gratuits-anime-compatible-unity
Branche: `myb-53-free-animated-asset-poc`
Statut local: implemented

## Objectif

Tester un petit subset d'assets gratuits dans `unity/Echapee4D`, avec une
preuve de scene Unity macOS locale et une vraie animation importee, avant toute
adoption artistique V1.

## Decisions appliquees

- Scene de POC dediee: `Assets/Scenes/MYB53AnimatedAssetPoc.unity`.
- La scene canonique `Assets/Scenes/MYB89UnityMcpProbe.unity` reste la seule
  scene active dans les Build Settings.
- Aucun asset dont la source, la licence ou la frontiere free/pro restait
  ambigue n'a ete importe.
- Le statut manifest `approved` reste une validation legale/technique POC; le
  `Verdict Artistique V1` reste separe.

## Sources verifiees

### KayKit Medieval Hexagon Pack

- Source: https://github.com/KayKit-Game-Assets/KayKit-Medieval-Hexagon-Pack-1.0
- Auteur: Kay Lousberg / KayKit
- Licence: CC0
- Preuve source: README GitHub annonce 200 modeles low-poly optimises, atlas
  1024x1024, fichiers FBX/glTF et usage personnel/commercial libre; le depot
  expose `LICENSE.txt`.
- Usage MYB-53: subset village/campagne pavee.

Fichiers importes:

- `building_home_A_yellow.fbx`
- `building_market_yellow.fbx`
- `building_well_yellow.fbx`
- `fence_stone_straight.fbx`
- `barrel.fbx`
- `hexagons_medieval.png`

### Quaternius Horse

- Source: https://poly.pizza/m/qvTrSG9pZF
- Auteur: Quaternius
- Licence: Public Domain / CC0
- Preuve source: page Poly Pizza du modele Horse avec format FBX/GLTF et
  licence CC0; le GLB expose par la page contient de vraies animations.
- Usage MYB-53: accent de vie anime place hors trajectoire.

Traitement:

- GLB source converti en FBX via Blender 5.1.1 pour import Unity natif.
- Aucune edition intentionnelle de geometrie, texture, rig ou keyframes.

## Fichiers principaux

- Manifest: `unity/Echapee4D/Assets/Echappee/Art/ThirdPartyAssets.assetmanifest.json`
- Credits: `THIRD_PARTY_ASSETS.md`
- Glossaire: `CONTEXT.md` avec `Verdict Artistique V1`
- Validateur: `unity/Echapee4D/Assets/MYB53/Editor/MYB53AnimatedAssetPocValidator.cs`
- Scene: `unity/Echapee4D/Assets/Scenes/MYB53AnimatedAssetPoc.unity`
- Rapport Unity: `_bmad-output/unity-test-results/myb-53-animated-asset-poc.txt`
- Capture Unity: `_bmad-output/unity-test-results/myb-53-animated-asset-poc.png`

## Resultat Unity

Le validateur MYB-53 construit une mini-scene `Village / campagne pavee`:

- route pavee avec materiau Paving Stones 141 reutilise depuis MYB-42;
- sol village/campagne;
- maison, marche, puits, cloture pierre et tonneau KayKit;
- cheval Quaternius anime avec controller idle;
- eclairage chaud, brouillard leger, points de lumiere de validation.

La scene n'est pas ajoutee aux Build Settings.

## Verdict Artistique V1

| Asset / groupe | Verdict | Raison |
| --- | --- | --- |
| KayKit Medieval Hexagon subset | `support/fallback` | Tres utile pour composer vite un village lisible, mais l'identite grid/board-game et les couleurs atlas necessitent un vrai polish pour porter seules la V1 premium. |
| Quaternius Horse animated | `support/fallback` | L'import animation est reussi et l'asset donne de la vie, mais le modele reste trop simple pour devenir un hero asset premium. |
| Paving Stones 141 reuse | `support/fallback` | Le signal de surface rugueuse fonctionne, mais le rendu PBR reste a styliser avant adoption finale. |

Conclusion: MYB-53 valide le flux technique et confirme que ces assets gratuits
sont exploitables comme support/fallback. Il ne valide pas encore un pack gratuit
capable de porter seul le rendu `Stylise Premium`.

## Validation executee

```bash
unity-mcp-cli status unity/Echapee4D --timeout 10000
unity-mcp-cli run-tool assets-refresh unity/Echapee4D --input '{"options":"ForceSynchronousImport"}' --timeout 180000
unity-mcp-cli run-tool script-execute unity/Echapee4D --input '{"className":"MYB53RunValidator","methodName":"Main","csharpCode":"using MYB53.Editor; public static class MYB53RunValidator { public static void Main() { MYB53AnimatedAssetPocValidator.BuildAndValidateCli(); } }"}' --timeout 240000
unity-mcp-cli run-tool script-execute unity/Echapee4D --input '{"className":"MYB91RunValidator","methodName":"Main","csharpCode":"using MYB91.Editor; public static class MYB91RunValidator { public static void Main() { MYB91CanonicalBaselineValidator.ValidateCanonicalBaselineCli(); } }"}' --timeout 180000
jq empty unity/Echapee4D/Assets/Echappee/Art/ThirdPartyAssets.assetmanifest.json
git diff --check
```

Resultats:

- MYB-53 validator: PASS
- MYB-91 canonical baseline: PASS
- Unity-MCP status: PASS
- Manifest JSON: PASS
- No zip/rar/7z imported under `ThirdParty`: PASS
- `git diff --check`: PASS

## Risques ouverts

- Le rendu gratuit teste ici reste trop support/fallback pour valider une DA
  premium finale.
- Le cheval FBX converti pese 7.0 MB; acceptable pour un POC unique, mais a
  optimiser ou remplacer avant multiplication d'assets animes.
- Le prochain cran doit comparer avec Meshy ou un pipeline de creation custom
  si on veut des signaux premium vraiment distinctifs.

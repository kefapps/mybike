# MYB-42 - Import assets 3D Unity avec garde-fous licence/perf

Date: 2026-06-11
Ticket Linear: https://linear.app/kefjbo/issue/MYB-42/myb-010-importer-des-assets-3d-dans-unity-avec-garde-fous-licenceperf
Statut: implementation

## Objectif

Valider un premier flux d'import Unity macOS-first dans `unity/Echapee4D` sans
basculer dans un import massif ni dans du level art premium.

MYB-42 prouve:

- source, licence, attribution et manifest;
- extraction minimale de fichiers utiles;
- import Unity de modeles FBX et textures JPG 1K;
- scene de validation dediee;
- capture visuelle et rapport de validation;
- non-regression de la scene canonique `MYB89UnityMcpProbe`.

## Decisions confirmees pendant le cadrage

- MYB-42 est un ticket de pipeline d'import robuste avec preuve visuelle legere,
  pas une scene premium finale.
- Import initial limite aux sources directement telechargeables sans compte ni
  paiement.
- Les assets versionnes sont uniquement les fichiers utiles, jamais les zips ni
  les packs complets.
- Le statut manifest `approved` signifie approuve pour import legal/technique
  POC, pas adoption artistique finale.
- La scene canonique reste intacte; la validation vit dans
  `Assets/Scenes/MYB42AssetImportValidation.unity`.

## Assets importes

| ID manifest | Source | Licence | Fichiers importes | Usage POC |
| --- | --- | --- | --- | --- |
| `kenney-nature-kit-myb42-subset` | https://kenney.nl/assets/nature-kit | CC0 | `tree_tall.fbx`, `rock_smallA.fbx`, `License.txt` | Foret claire: echelle et import modele simple. |
| `kenney-fantasy-town-kit-myb42-subset` | https://kenney.nl/assets/fantasy-town-kit | CC0 | `road.fbx`, `wall-window-stone.fbx`, `License.txt` | Village / campagne pavee: route et mur de validation. |
| `ambientcg-paving-stones-141-1k-jpg-myb42` | https://ambientcg.com/view?id=pavingstones141 | CC0 | Color, NormalGL, Roughness 1K JPG | Surface de Route pavee. |
| `polyhaven-forest-ground-03-1k-jpg-myb42` | https://polyhaven.com/a/forrest_ground_03 | CC0 | Diffuse, NormalGL, Roughness 1K JPG | Sol foret / chemin naturel. |

Taille versionnee du subset tiers: environ `7.1M`.

## Fichiers Unity ajoutes

- `unity/Echapee4D/Assets/Echappee/Art/ThirdParty/**`
- `unity/Echapee4D/Assets/Echappee/Art/MYB42Validation/Materials/**`
- `unity/Echapee4D/Assets/MYB42/Editor/MYB42AssetImportValidator.cs`
- `unity/Echapee4D/Assets/Scenes/MYB42AssetImportValidation.unity`

Les archives source restent hors repo dans `/tmp/myb42-assets` pendant
l'implementation et ne sont pas versionnees.

## Traçabilite

- Manifest Unity:
  `unity/Echapee4D/Assets/Echappee/Art/ThirdPartyAssets.assetmanifest.json`
- Credits:
  `THIRD_PARTY_ASSETS.md`
- Glossaire:
  `CONTEXT.md`

`CONTEXT.md` clarifie maintenant que `Asset Tiers Approuve` ne signifie pas
adoption finale par la direction artistique.

## Validation Unity

Validateur ajoute:

```text
Tools/MYB-42/Build Asset Import Validation Scene
MYB42.Editor.MYB42AssetImportValidator.BuildAndValidateCli
```

Rapport genere:

```text
_bmad-output/unity-test-results/myb-42-asset-import-validation.txt
```

Capture generee:

```text
_bmad-output/unity-test-results/myb-42-asset-import-validation.png
```

Resultat:

- `Status: PASS`
- Unity `6000.4.10f1`
- textures importees en `1024x1024`
- scene de validation sauvee
- scene canonique encore seule scene active en Build Settings
- aucune failure dans le rapport MYB-42

## Verdict POC

Le flux d'import est valide.

Les modeles Kenney et les materiaux 1K sont utiles comme assets de validation,
fallback ou base de composition, mais ne suffisent pas seuls a porter la DA
`Stylise Premium`. Ils servent surtout a prouver que le pipeline fonctionne:
sources, extraction minimale, manifest, import Unity, materiaux, scene,
capture, validation.

Pour la suite, MYB-53 doit chercher la valeur visuelle plus ambitieuse:

- tester un pack village/foret plus premium si la frontiere gratuite/licence est
  claire;
- travailler le rendu material/stylisation des surfaces pavees et sol foret;
- evaluer un accent anime ou un signal premium seulement si son import reste
  propre et mesurable.

## Risques ouverts

- Les textures PBR ambientCG/Poly Haven sont realistes; elles devront etre
  stylisees pour ne pas casser la DA.
- Les modeles Kenney importes sont techniquement propres mais restent visuels de
  support/fallback.
- Quaternius et autres packs plus premium restent hors import MYB-42 tant que
  la frontiere free/pro ou les conditions de telechargement ne sont pas
  verifiees sans ambiguite.
- Le ticket ne definit pas encore un budget FPS formel; ce role revient a un
  ticket de budget/performance dedie.

## Commandes de validation executees

```bash
unity-mcp-cli status unity/Echapee4D --timeout 10000
unity-mcp-cli run-tool assets-refresh unity/Echapee4D --input '{"options":"ForceSynchronousImport"}' --timeout 120000
unity-mcp-cli run-tool script-execute unity/Echapee4D --input '{"className":"MYB42RunValidator","methodName":"Main","csharpCode":"using MYB42.Editor; public static class MYB42RunValidator { public static void Main() { MYB42AssetImportValidator.BuildAndValidateCli(); } }"}' --timeout 180000
```

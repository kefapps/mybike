# MYB-91 - Baseline Unity canonique Echapee4D

Date: 2026-06-11
Statut local: review
Linear: https://linear.app/kefjbo/issue/MYB-91/myb-008-baseline-unity-canonique-echapee4d-hygiene-repo-validation-et

## Objectif

Transformer `unity/Echapee4D` en baseline Unity canonique propre avant de
reprendre le gameplay.

## Implementation

- `unity/Echapee4D/ProjectSettings/EditorBuildSettings.asset` pointe maintenant
  vers la scene canonique `Assets/Scenes/MYB89UnityMcpProbe.unity`.
- `unity/Echapee4D/README.md` documente le projet canonique, les frontieres de
  source control, la validation Unity-MCP, le validator MYB-91, le rebuild WebGL
  et la capture browser.
- `unity/Echapee4D/Assets/MYB91/Editor/MYB91CanonicalBaselineValidator.cs`
  ajoute un validator Unity dedie a la baseline canonique.
- `.gitignore` ignore les dossiers Unity generes de `unity/Echapee4D` et les
  builds WebGL locaux sous `_bmad-output/unity-webgl-builds/`.
- `_bmad-output/unity-test-results/myb-91-canonical-baseline.txt` est le rapport
  reproductible du validator.
- `MYB-92` a ete cree pour supprimer `unity/Echappee3D` juste apres MYB-91.

## Scene canonique

La scene canonique reste `Assets/Scenes/MYB89UnityMcpProbe.unity`.

Ce nom est conserve volontairement: c'est la scene prouvee par `MYB-89` puis par
le build/capture WebGL de `MYB-90`. La renommer maintenant forcerait une mise a
jour de tous les builders, captures et rapports alors que le but de MYB-91 est
de stabiliser la baseline, pas de faire une migration nominale cosmetique.

## Validation Unity

Validator MYB-91:

- Rapport: `_bmad-output/unity-test-results/myb-91-canonical-baseline.txt`
- Statut: `PASS`
- Unity: `6000.4.10f1`
- Package Unity-MCP: `0.80.0`
- URP: `17.4.0`
- Scene active: `Assets/Scenes/MYB89UnityMcpProbe.unity`
- Build Settings: `Assets/Scenes/MYB89UnityMcpProbe.unity`
- WebGL support: `True`
- Previous MYB-90 WebGL build status: `Succeeded`
- Route markers: `9`
- Route length: `245.0 m`
- Renderer count: `130`
- Main camera, road mesh, HUD canvas and HUD labels: present/wired.

Rebuild WebGL:

- Builder: `unity/Echapee4D/Assets/MYB90/Editor/MYB90WebGLReadinessBuilder.cs`
- Rapport: `_bmad-output/unity-test-results/myb-90-unity-webgl-build.txt`
- Statut: `Succeeded`
- Warnings/errors reportes par build summary: `0` / `0`
- Build output: `_bmad-output/unity-webgl-builds/myb-90-unity-mcp-probe`
- Taille build directory: `75M`
- Total reported size: `78,075,757` bytes
- `.wasm`: `61,032,893` bytes
- `.data`: `16,505,871` bytes

Capture browser apres rebuild:

- Summary: `_bmad-output/video-captures/myb-90-unity-webgl-readiness-2026-06-11T09-37-30-711Z/capture-summary.json`
- Video: `_bmad-output/video-captures/myb-90-unity-webgl-readiness-2026-06-11T09-37-30-711Z/unity-webgl-readiness.mp4`
- Contact sheet: `_bmad-output/video-captures/myb-90-unity-webgl-readiness-2026-06-11T09-37-30-711Z/unity-webgl-readiness-contact-sheet.jpg`
- HTTP status: `200`
- Failed requests: `0`
- Page errors: `0`
- Console errors/warnings: `0` / `7`
- Canvas screenshot nonblank: `true`
- Video: H.264, `1280x720`, `25 fps`, `33.68 s`

## Caveats

- Les warnings WebGL/URP deja observes dans MYB-90 restent presents dans le
  navigateur: `INVALID_ENUM getInternalformatParameter` et shader FSR non
  supporte.
- La lecture directe finale du canvas via WebGL retourne encore une frame noire,
  mais la screenshot canvas, la screenshot page et la video sont non blank.
- `Library`, `Logs`, `Temp` et `UserSettings` existent localement dans
  `unity/Echapee4D`; ils sont generes par Unity et doivent rester ignores.
- `unity/Echappee3D` n'est pas supprime dans MYB-91. La suppression est suivie
  par `MYB-92`.

## Scope garde

- Pas de modification de `src/**`.
- Pas de modification de `unity/Echappee3D/**`.
- Pas de nouveau gameplay, asset externe, Meshy, BLE/FTMS, backend ou pipeline
  lourd.

## Prochaine action

Revue de MYB-91, puis execution de `MYB-92` pour supprimer le projet Unity
legacy `unity/Echappee3D`.

## Review

Statut review: approved
Linear review comment: `c4732066-3def-4aa1-b092-51a0dd9128b0`

Finding corrige:

- Le validator MYB-91 verifiait que le rapport WebGL MYB-90 existait, mais pas
  que son `Status` etait `Succeeded`. Une baseline aurait pu passer avec un
  rapport `BLOCKED` ou `EXCEPTION` deja present.

Correction:

- `MYB91CanonicalBaselineValidator` lit maintenant la ligne `Status: ...` dans
  `_bmad-output/unity-test-results/myb-90-unity-webgl-build.txt`.
- La validation echoue si ce statut n'est pas `Succeeded`.
- Le rapport MYB-91 inclut `Previous MYB-90 WebGL build status: Succeeded`.

Validation review:

- `Assets refresh`: pass.
- `MYB91CanonicalBaselineValidator.ValidateCanonicalBaseline()`: pass.
- `_bmad-output/unity-test-results/myb-91-canonical-baseline.txt`: `PASS`.
- `git diff --check`: pass.

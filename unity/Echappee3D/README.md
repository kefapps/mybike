# Echappee 3D Unity Scaffold

## Version cible

- Cible MYB-11: Unity `6000.4.10f1`.
- Chemin local attendu:
  `/Applications/Unity/Hub/Editor/6000.4.10f1/Unity.app/Contents/MacOS/Unity`.
- Ancienne reference architecture: `6000.3.17f1`, supersedee ici parce que
  l'Editor installe localement est `6000.4.10f1`.
- Unity Hub est installe, mais `unityhub` et `Unity` ne sont pas exposes dans le
  `PATH`.
- MCP Unity officiel verifie dans Codex: `mcp__unity_mybike`, projet
  `/Users/jbodin/personnel/apps/mybike/unity/Echappee3D`, scene
  `RideMock.unity`, console Unity sans erreur ni warning pendant le preflight.

## Ouverture projet

Depuis le repo:

```bash
/Applications/Unity/Hub/Editor/6000.4.10f1/Unity.app/Contents/MacOS/Unity \
  -projectPath unity/Echappee3D
```

La scene cible est `Assets/Scenes/RideMock.unity`. Elle est generee par
`Assets/Echappee/Editor/RideMockSceneBuilder.cs`; le fichier
`Assets/Scenes/RideMock.scene-plan.md` garde le contrat lisible de la scene.

## Validation

Validation Editor MYB-11:

```bash
/Applications/Unity/Hub/Editor/6000.4.10f1/Unity.app/Contents/MacOS/Unity \
  -batchmode \
  -nographics \
  -projectPath unity/Echappee3D \
  -executeMethod MyBike.Echappee3D.EditorTools.RideMockValidator.Validate \
  -logFile ../../_bmad-output/unity-test-results/myb-11-editor-validation.log \
  -quit
```

Le rapport attendu est
`_bmad-output/unity-test-results/myb-11-editor-validation.txt`.

EditMode Test Runner attendu quand `-runTests` produit un XML:

```bash
/Applications/Unity/Hub/Editor/6000.4.10f1/Unity.app/Contents/MacOS/Unity \
  -batchmode \
  -projectPath unity/Echappee3D \
  -runTests \
  -testPlatform EditMode \
  -testResults ../../_bmad-output/unity-test-results/myb-11-editmode.xml \
  -quit
```

PlayMode smoke attendu pour une prochaine passe:

```bash
/Applications/Unity/Hub/Editor/6000.4.10f1/Unity.app/Contents/MacOS/Unity \
  -batchmode \
  -projectPath unity/Echappee3D \
  -runTests \
  -testPlatform PlayMode \
  -testResults ../../_bmad-output/unity-test-results/myb-11-playmode.xml \
  -quit
```

## Limites MYB-11

- Scaffold Unity isole, pas migration complete.
- Prototype React/Three conserve comme reference comportementale.
- Mock input seulement: pas de BLE, FTMS, velo reel, backend ou deploiement.
- Pas de Meshy, asset externe, Unity AI generation, humains, vehicules ou
  oiseaux.
- Pas de Cinemachine, Addressables, ECS/DOTS ou framework lourd.
- Route, camera, HUD et fog/depth sont prepares par scripts et plan de scene.

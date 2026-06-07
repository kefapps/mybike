# Story MYB-12: Unity playable parity mock loop

## Metadata

- Linear issue: `MYB-12`
- Linear URL: https://linear.app/kefjbo/issue/MYB-12/unity-playable-parity-mock-loop
- Local status: `ready-for-dev`
- Linear status: `In Progress`
- Created: `2026-06-07`
- Baseline commit: `c81ee147a5fb53da0d4bccae2524099cc4cdaf9e`
- Depends on: `MYB-11`
- Unity project: `/Users/jbodin/personnel/apps/mybike/unity/Echappee3D`
- Unity scene: `Assets/Scenes/RideMock.unity`
- Source story: `_bmad-output/implementation-artifacts/myb-11-unity-vertical-slice-mock-scaffold.md`
- Architecture source: `_bmad-output/planning-artifacts/echappee-3d-architecture-unity-mince-2026-06-07.md`
- Change proposal source: `_bmad-output/planning-artifacts/sprint-change-proposal-unity-migration-2026-06-07.md`

## Story

En tant que dev sur la migration Unity, je veux rendre la scene `RideMock.unity`
jouable au niveau minimum afin de verifier que Unity peut reproduire la boucle
coeur du prototype web sans glisser vers une migration complete.

## Contexte

MYB-11 a pose un scaffold Unity isole sous `unity/Echappee3D`. MYB-12 transforme
ce scaffold en mock jouable minimum: start, ride, pause, resume, finish, puis
summary ou etat final minimal. Le prototype React/Three reste la reference
comportementale, mais il ne doit pas etre modifie.

Le but n'est pas la qualite finale. Le but est de prouver que la scene Unity
peut porter la boucle mock de base avec route visible, controle mock simple,
HUD minimal, camera stable et brouillard/profondeur preserves.

## Preuve MCP de creation

MCP Unity est obligatoire pour MYB-12. Apres approbation de
`codex-unity-mcp-direct-preflight`, un client MCP direct vers le relais Unity a
confirme le 2026-06-07:

- Project root: `/Users/jbodin/personnel/apps/mybike/unity/Echappee3D`
- Editor state: pas Play Mode, pas pause, pas compilation, pas update
- Active scene: `Assets/Scenes/RideMock.unity`
- Hierarchy: `Main Camera`, `Route`, `Fog`, `Canvas`, `EventSystem`, `RideSession`
- Console: 0 error, 0 warning

Note tooling: les namespaces Codex `mcp__unity_mybike` / `mcp__unity_mcp`
peuvent rester stale apres reload ou changement d'approbation. L'acceptance
MYB-12 exige quand meme MCP: utiliser les namespaces si disponibles, sinon un
client MCP direct approuve vers le relais Unity. Ne pas remplacer cette preuve
par batchmode.

## Acceptance Criteria

1. MCP Unity est utilisable avant implementation et en validation finale contre
   `/Users/jbodin/personnel/apps/mybike/unity/Echappee3D`.
2. `RideMock.unity` s'ouvre sur un etat initial lisible avec route visible,
   camera stable, HUD minimal et brouillard/profondeur preserves.
3. La boucle `start -> ride -> pause -> resume -> finish -> summary/end state`
   est jouable dans Unity.
4. Un input mock simple existe, idealement un slider Unity UI, et influence
   l'effort ou la vitesse affichee.
5. Le HUD minimal affiche au moins vitesse, effort, progression et etat de
   session.
6. `pause` fige la progression et les updates de ride visibles; `resume` reprend
   depuis la progression courante sans reset involontaire.
7. `finish` produit un etat final clair et un resume minimal, par exemple temps,
   distance/progression, vitesse moyenne ou valeurs equivalentes deja disponibles.
8. La route reste stable et visible pendant la boucle mock.
9. La camera ride reste simple, lisible et stable, sans saut perceptible ou
   dependance a un package lourd.
10. Le brouillard/profondeur de MYB-11 est preserve.
11. Les scripts restent coherents avec l'architecture Unity mince: logique
    ride/route testable en C# pur, orchestration mince dans le bootstrap/session,
    rendu et UI pilotes par snapshots ou donnees simples.
12. `RideMockValidator` est etendu, ou un test equivalent est ajoute, pour
    verifier la scene, la hierarchie minimale, les controles, le HUD, la camera,
    le brouillard, l'etat initial et les transitions minimales.
13. Aucun scope interdit n'est introduit: pas Meshy, pas asset externe, pas
    Unity AI generation, pas pente, pas oiseaux/humains/vehicules, pas BLE/FTMS,
    pas backend, pas deploiement public, pas migration complete du prototype web.
14. Aucun changement n'est attendu dans `src/ride/*`, `src/render/*` ou
    `src/app/*`.

## Tasks

- [ ] Relancer le preflight MCP Unity obligatoire avant implementation.
- [ ] Etendre la boucle de session Unity pour `start`, `ride`, `pause`,
      `resume`, `finish` et summary/end state minimal.
- [ ] Ajouter ou brancher un controle mock simple dans la scene, idealement un
      slider Unity UI.
- [ ] Ajouter ou completer le HUD minimal: vitesse, effort, progression, etat.
- [ ] Conserver une route visible et stable dans `RideMock.unity`.
- [ ] Stabiliser la camera ride simple sans package lourd.
- [ ] Preserver le brouillard/profondeur de MYB-11.
- [ ] Etendre `RideMockSceneBuilder` si necessaire pour reconstruire la scene
      minimale coherente.
- [ ] Etendre `RideMockValidator` ou ajouter un test equivalent pour couvrir les
      nouveaux invariants.
- [ ] Documenter la validation Unity/MCP et les limites restantes dans BMAD et
      Linear.
- [ ] Verifier qu'aucun changement web ni scope interdit n'a ete ajoute.

## Fichiers Unity probables

- `unity/Echappee3D/Assets/Echappee/Bootstrap/RideSessionController.cs`
- `unity/Echappee3D/Assets/Echappee/Core/RidePhase.cs`
- `unity/Echappee3D/Assets/Echappee/Core/RideSnapshots.cs`
- `unity/Echappee3D/Assets/Echappee/Ride/MockRideInput.cs`
- `unity/Echappee3D/Assets/Echappee/Ride/RideMath.cs`
- `unity/Echappee3D/Assets/Echappee/Route/RouteMath.cs`
- `unity/Echappee3D/Assets/Echappee/Rendering/RideCameraController.cs`
- `unity/Echappee3D/Assets/Echappee/Rendering/DepthFogController.cs`
- `unity/Echappee3D/Assets/Echappee/UI/HudController.cs`
- `unity/Echappee3D/Assets/Echappee/UI/MockRideInput.cs`
- `unity/Echappee3D/Assets/Echappee/Editor/RideMockSceneBuilder.cs`
- `unity/Echappee3D/Assets/Echappee/Editor/RideMockValidator.cs`
- `unity/Echappee3D/Assets/Scenes/RideMock.unity`

Nouveaux scripts possibles, si cela reste plus clair que de surcharger les
classes existantes:

- `unity/Echappee3D/Assets/Echappee/UI/RideControlPanel.cs`
- `unity/Echappee3D/Assets/Echappee/UI/RideSummaryController.cs`
- `unity/Echappee3D/Assets/Echappee/Tests/EditMode/RideSessionLoopTests.cs`

## Validation attendue

- MCP Unity obligatoire:
  - confirmer project root `/Users/jbodin/personnel/apps/mybike/unity/Echappee3D`;
  - confirmer scene active `Assets/Scenes/RideMock.unity`;
  - confirmer Editor idle, pas compiling, pas Play Mode;
  - lire la hierarchie scene;
  - lire la console Unity et confirmer 0 error / 0 warning.
- Validation Unity:
  - compilation/import Editor sans erreur;
  - `RideMockValidator` etendu ou test equivalent;
  - verification manuelle courte de la boucle jouable dans `RideMock.unity`.
- Validation repo:
  - `git diff --check`;
  - scan anti-secret haute confiance du diff;
  - verifier aucun changement dans `src/ride/*`, `src/render/*`, `src/app/*`;
  - verifier aucune capture video staged.
- Ne pas lancer `npm run typecheck`, `npm run test` ou `npm run build` sauf si
  un changement web est detecte.

## Garde-fous de scope

- Unity uniquement sous `unity/Echappee3D`.
- Le prototype web reste preserve comme reference.
- Pas de Meshy et pas d'asset externe.
- Pas d'Unity AI generation.
- Pas de pentes dans MYB-12.
- Pas d'oiseaux, humains ou vehicules.
- Pas de BLE/FTMS, backend ou deploiement public.
- Pas de migration complete et pas de qualite finale visee.

## Risques et investigation MCP

- Les namespaces MCP Codex peuvent rester stale apres reload Unity ou changement
  d'approbation. Si c'est le cas, investiguer avant implementation.
- L'entree MCP directe approuvee `codex-unity-mcp-direct-preflight` est acceptee
  comme chemin MCP obligatoire tant que les appels `Unity_ManageEditor`,
  `Unity_ManageScene` et `Unity_ReadConsole` reussissent.
- Si MCP redevient indisponible, stopper la validation, verifier
  `Project Settings > AI > Unity MCP`, puis relire
  `unity/Echappee3D/Library/AI.MCP/connections-v2.asset` et les logs
  `unity/Echappee3D/Logs/relay.txt`. Ne pas basculer vers batchmode comme preuve
  d'acceptance.
- Eviter `get_components` large sur `Canvas`: le troubleshooting Unity MCP
  signale des freezes possibles sur les objets UI. Preferer hierarchy,
  validators Unity cibles et checks de scripts explicites.

## Dev Agent Record

### Implementation Notes

To be filled during MYB-12 implementation.

### Validation Evidence

To be filled during MYB-12 implementation.

### File List

To be filled during MYB-12 implementation.

## QA / Review Notes

To be filled during MYB-12 review.

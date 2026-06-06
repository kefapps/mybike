---
story_id: "1.1"
story_key: "1-1-app-shell-mvp-et-etats-de-session"
linear_id: "MYB-2"
epic_linear_id: "MYB-1"
title: "App shell MVP et etats de session"
status: "done"
created: "2026-06-06"
scope: "vertical slice mock - app shell only"
source_epic: "_bmad-output/planning-artifacts/echappee-3d-mvp-epic-stories.md"
baseline_commit: "NO_VCS"
---

# Story 1.1: App shell MVP et etats de session

Status: done

## Story

As a joueur de la vertical slice mock,
I want lancer une app web minimale, passer entre les etats de session, mettre en pause, reprendre, finir et voir un resume simple,
so that la boucle applicative de base existe avant d'ajouter la logique ride, la route et le rendu Three.js complet.

## Contexte

`MYB-2` est la premiere story de developpement de l'epic `MYB-1`. Elle installe le socle Vite/React/TypeScript et les etats d'ecran necessaires au MVP, sans implementer les stories suivantes.

Le repo ne contient pas encore d'app Vite, `package.json`, `src/`, `vite.config.*` ou `tsconfig*.json`. Cette story est donc un demarrage greenfield minimal.

## Scope strict MYB-2

Inclus:

- App Vite + React + TypeScript minimale.
- Ecrans `StartScreen`, `RideScreen`, `SummaryScreen`.
- Fallback WebGL simple et comprehensible.
- Etat de session en memoire uniquement: `idle`, `running`, `paused`, `finished`, `error`.
- Actions de session: `start`, `pause`, `resume`, `finish`, `fail`, `reset`.
- Reducer/session state pur et testable hors React.
- Points d'integration futurs nommes, mais non implementes, pour `MYB-3` a `MYB-6`.

Exclus:

- Logique ride avancee: input mock, vitesse, smoothing, progression, distance reelle, stats calculees.
- Route, biomes, camera sur rail, `cameraOnRail`, `RouteProgress`.
- Scene Three.js complete, `SceneController`, meshes, lighting, assets placeholders.
- HUD final, controles mock, happy path Playwright.
- Backend, persistance, compte utilisateur, BLE, Web Bluetooth, FTMS, Meshy, GPX.

## Acceptance Criteria

1. Depuis l'ecran de depart, le joueur peut lancer une balade mock et atteindre l'ecran ride en phase `running`.
2. La session expose un etat strictement borne a `idle`, `running`, `paused`, `finished` ou `error`.
3. Pendant la phase `running`, le joueur peut mettre en pause; la phase devient `paused` sans perdre l'etat de session courant.
4. Depuis la phase `paused`, le joueur peut reprendre; la phase redevient `running`.
5. Depuis `running` ou `paused`, le joueur peut finir la session et atteindre un ecran de resume en phase `finished`.
6. Le resume affiche au minimum un etat de fin comprehensible et des valeurs placeholder explicites pour duree, distance et vitesse moyenne tant que `MYB-3` n'a pas fourni les stats reelles.
7. Si WebGL est indisponible ou si le montage de l'experience ride signale une erreur, l'app passe en phase `error` et affiche un fallback comprehensible au lieu d'un canvas vide.
8. L'app ne depend d'aucun backend, compte, materiel externe, source autre que `mock`, ni service reseau.
9. Le reducer/session state est couvert par des tests unitaires pour `idle -> running -> paused -> running -> finished`, `running -> finished`, `paused -> finished`, `running -> error` et `reset`.
10. Aucune implementation de `MYB-3`, `MYB-4`, `MYB-5` ou `MYB-6` n'est ajoutee au-dela de types/placeholders necessaires a l'integration future.

## Tasks / Subtasks

- [x] Initialiser le shell Vite/React/TypeScript minimal. (AC: 1, 8)
  - [x] Creer les fichiers de configuration npm/Vite/TypeScript/Vitest necessaires.
  - [x] Ajouter les scripts attendus: `npm run typecheck`, `npm run test`, `npm run build`.
  - [x] Monter l'app React depuis `src/main.tsx`.

- [x] Definir le modele de session pur. (AC: 2, 3, 4, 5, 7, 9)
  - [x] Creer le type `RidePhase = "idle" | "running" | "paused" | "finished" | "error"`.
  - [x] Creer un `SessionState` minimal avec phase, timestamp de debut optionnel, timestamp de fin optionnel et message d'erreur optionnel.
  - [x] Creer un `sessionReducer` ou equivalent pur avec actions `start`, `pause`, `resume`, `finish`, `fail`, `reset`.
  - [x] Refuser les transitions incoherentes de maniere deterministe, sans exception runtime non geree.

- [x] Creer les ecrans applicatifs MVP. (AC: 1, 3, 4, 5, 6, 7)
  - [x] `StartScreen`: nom de l'experience et action principale pour lancer une balade mock.
  - [x] `RideScreen`: etat ride minimal avec actions pause, reprise et fin.
  - [x] `SummaryScreen`: resume placeholder explicite tant que les stats reelles arriveront avec `MYB-3`.
  - [x] `WebGlFallback`: message clair quand WebGL est indisponible ou quand l'experience ride ne peut pas se monter.

- [x] Brancher l'orchestration d'ecran dans `App`. (AC: 1, 2, 3, 4, 5, 7)
  - [x] Utiliser le reducer/session state comme source unique de phase.
  - [x] Afficher `StartScreen` en `idle`, `RideScreen` en `running` ou `paused`, `SummaryScreen` en `finished`, `WebGlFallback` en `error`.
  - [x] Garder tout l'etat en memoire; ne pas utiliser storage, API, routeur multi-page ou backend.

- [x] Ajouter la detection WebGL simple. (AC: 7)
  - [x] Implementer un helper testable qui detecte la possibilite d'obtenir un contexte WebGL/WebGL2 depuis un canvas.
  - [x] Ne pas importer `three` et ne pas creer de scene 3D dans cette story.
  - [x] Si la detection echoue, declencher l'action `fail` et afficher le fallback.

- [x] Ajouter les tests attendus. (AC: 2, 3, 4, 5, 7, 9)
  - [x] Tests unitaires du reducer/session state pour toutes les transitions d'AC 9.
  - [x] Test de transition d'erreur `fail`.
  - [x] Test du helper WebGL avec mocks de canvas si l'environnement de test le permet.
  - [x] Optionnel si peu couteux: test React de rendu de l'ecran correspondant a chaque phase.

- [x] Verifier localement la story. (AC: 1-10)
  - [x] `npm run typecheck`
  - [x] `npm run test`
  - [x] `npm run build`
  - [x] Verification manuelle rapide: start, pause, resume, finish, summary, fallback force si possible.

## Dev Notes

### Contraintes d'architecture

- React gere l'interface et les etats d'ecran.
- La logique de session doit rester testable hors React et hors Three.js.
- Three.js ne doit pas etre implemente dans `MYB-2`; cette story prepare seulement le fallback WebGL simple.
- Les donnees de session restent en memoire et le resume n'est pas persiste.
- Le mode mock doit etre visible comme intention produit, mais le controle mock et les stats reelles appartiennent a `MYB-3` et `MYB-6`.

### Decisions de scope pour eviter les derives

- Ne pas creer `MockRideInputSource`, `mapMockInputToSpeed`, `smoothSpeed`, `advanceRouteProgress`, `cameraOnRail`, `SceneController`, `RideHud` ou `MockRideControls`.
- Ne pas ajouter de route prefabriquee, biome, camera, mesh, lumiere, fog ou asset placeholder.
- Ne pas ajouter Playwright dans cette story sauf si le scaffold de test du projet l'impose deja; le happy path Playwright est porte par `MYB-6`.
- Ne pas ajouter de persistance locale pour la session.

### Points d'integration futurs a preparer seulement

- `RideScreen` peut exposer une zone ou un composant placeholder pour recevoir plus tard le canvas/rendu de `MYB-5`.
- `SummaryScreen` doit accepter ou isoler des valeurs placeholder afin que `MYB-3` puisse remplacer la source par de vraies stats sans refondre l'ecran.
- Le reducer doit rester assez simple pour que `MYB-3` puisse ajouter les donnees de ride sans casser les phases.

### Fichiers probables a creer

Configuration et entree app:

- `package.json`
- `package-lock.json` si npm est utilise pendant l'installation
- `index.html`
- `vite.config.ts`
- `tsconfig.json`
- `tsconfig.node.json` ou equivalent selon le template Vite retenu
- `vitest.config.ts` si la config de test n'est pas integree a `vite.config.ts`

Source applicative:

- `src/main.tsx`
- `src/App.tsx`
- `src/app/session/sessionTypes.ts`
- `src/app/session/sessionReducer.ts`
- `src/app/session/sessionReducer.test.ts`
- `src/app/webgl/webglSupport.ts`
- `src/app/webgl/webglSupport.test.ts`
- `src/app/screens/StartScreen.tsx`
- `src/app/screens/RideScreen.tsx`
- `src/app/screens/SummaryScreen.tsx`
- `src/app/screens/WebGlFallback.tsx`
- `src/styles.css` ou `src/App.css`

Tests optionnels selon ergonomie:

- `src/App.test.tsx`
- `src/test/setup.ts`

### Fichiers a ne pas creer dans MYB-2

- `src/ride/MockRideInputSource.ts`
- `src/ride/speed*.ts`
- `src/route/*`
- `src/render/SceneController.ts`
- `src/render/ThreeCanvasHost.tsx`
- `tests/e2e/*`

### Validation attendue

Les commandes de validation attendues pour cette story sont:

```bash
npm run typecheck
npm run test
npm run build
```

Si une commande n'existe pas encore, l'implementation de `MYB-2` doit l'ajouter ou expliquer explicitement pourquoi elle est impossible dans le scaffold retenu.

## Project Structure Notes

- Aucun fichier applicatif existant n'a ete detecte au moment de creation de cette story.
- La structure proposee respecte les modules de l'architecture: `app` pour ecrans/session, sans encore creer les modules `ride`, `route` et `render`.
- Si le developpeur choisit une structure legerement differente, elle doit rester compatible avec les frontieres: UI/React d'un cote, logique pure de session de l'autre.

## Project Context Rules

- Respecter `AGENTS.md`: scope strict Linear, mock mode preserve, ride logic pure et testable, React pour UI/screen state, Three.js pour rendu seulement plus tard.
- Ne pas creer ou modifier Linear sans confirmation.
- Ne pas importer le backlog complet ni anticiper les stories post-MYB-2.
- Ne jamais committer de secret.

## References

- `_bmad-output/planning-artifacts/echappee-3d-mvp-epic-stories.md` - `Story 1.1 - App shell MVP et etats de session`
- `_bmad-output/planning-artifacts/echappee-3d-mvp-epic-stories.md` - `Couverture du scope obligatoire`
- `_bmad-output/planning-artifacts/echappee-3d-architecture-mince.md` - `Modules MVP / app`
- `_bmad-output/planning-artifacts/echappee-3d-architecture-mince.md` - `Modules MVP / ride`
- `_bmad-output/planning-artifacts/echappee-3d-architecture-mince.md` - `Contrats minimum / RidePhase`
- `_bmad-output/planning-artifacts/implementation-readiness-report-2026-06-06.md` - `Recommandation MYB-2`
- `_bmad-output/linear-sync.md` - `Linear Issues` et `Readiness Caveats Synced`

## Questions ouvertes sauvegardees

- Le nom UI final avec accentuation exacte (`Echappee 3D` ou `Échappée 3D`) reste ouvert dans le GDD. Pour `MYB-2`, utiliser une constante ou un texte facile a ajuster plus tard; ne pas bloquer l'implementation.

## Dev Agent Record

### Agent Model Used

GPT-5 Codex

### Debug Log References

- 2026-06-06T14:23:14+02:00 - RED: `npm run test` echoue comme attendu car `sessionReducer` et `webglSupport` n'existent pas encore.
- 2026-06-06T14:31:13+02:00 - GREEN: `npm run test` passe avec 12 tests dans 3 fichiers.
- 2026-06-06T14:31:13+02:00 - `npm run typecheck` passe.
- 2026-06-06T14:31:24+02:00 - `npm run build` passe; Vite genere `dist/`.
- 2026-06-06T14:26:43+02:00 - Serveur Vite local disponible en HTTP 200 sur `http://127.0.0.1:5174/`; verification navigateur integre bloquee par un profil Chrome DevTools deja actif, sans ajout de Playwright.

### Completion Notes List

- Shell Vite/React/TypeScript initialise avec scripts `typecheck`, `test`, `build` et point d'entree `src/main.tsx`.
- Reducer de session pur ajoute avec phases bornees `idle`, `running`, `paused`, `finished`, `error` et actions `start`, `pause`, `resume`, `finish`, `fail`, `reset`.
- Ecrans MVP ajoutes: `StartScreen`, `RideScreen`, `SummaryScreen`, `WebGlFallback`; le resume affiche des valeurs explicitement placeholder pour MYB-3.
- Detection WebGL testable ajoutee sans `three`; l'echec de detection declenche `fail` et affiche le fallback.
- Tests ajoutes pour les transitions AC 9, l'action `fail`, le helper WebGL et le flux React start/pause/resume/finish/fallback en jsdom.
- Verification de scope effectuee: aucun fichier `ride`, `route`, `render`, Three.js, Playwright, storage ou appel reseau applicatif ajoute.

### File List

- `.gitignore`
- `_bmad-output/implementation-artifacts/1-1-app-shell-mvp-et-etats-de-session.md`
- `_bmad-output/implementation-artifacts/sprint-status.yaml`
- `index.html`
- `package-lock.json`
- `package.json`
- `src/App.test.tsx`
- `src/App.tsx`
- `src/app/screens/RideScreen.tsx`
- `src/app/screens/StartScreen.tsx`
- `src/app/screens/SummaryScreen.tsx`
- `src/app/screens/WebGlFallback.tsx`
- `src/app/session/sessionReducer.test.ts`
- `src/app/session/sessionReducer.ts`
- `src/app/session/sessionTypes.ts`
- `src/app/webgl/webglSupport.test.ts`
- `src/app/webgl/webglSupport.ts`
- `src/main.tsx`
- `src/styles.css`
- `tsconfig.json`
- `vite.config.ts`

### Change Log

- 2026-06-06 - Implementation MYB-2: scaffold Vite/React/TypeScript, session reducer pur, ecrans MVP, fallback WebGL et validations unitaires/build.

---
title: "Sprint Change Proposal - Retour Three.js pour demo privee"
project: "mybike / Echappee 3D"
date: "2026-06-07"
workflow: "gds-correct-course"
status: "approved-direction"
source_commit: "6794394b5d878ac431a888dcad94a783dca59577"
source_commit_label: "MYB-17 Unity private demo package and launch checklist"
source_capture: "_bmad-output/video-captures/unity-webgl-running-2026-06-07T19-59-58-250Z/"
next_action: "create-threejs-private-demo-recovery-story"
linear_sync: "project-status-update"
---

# Sprint Change Proposal - Retour Three.js pour demo privee

## 1. Issue Summary

MYB-17 a transforme la vertical slice Unity en demo locale WebGL lancable.
Le build charge correctement sur:

```text
http://127.0.0.1:8087/
```

Une capture reelle d'une minute a ete produite:

```text
_bmad-output/video-captures/unity-webgl-running-2026-06-07T19-59-58-250Z/
```

Le signal utilisateur est clair:

- la session avance bien;
- le HUD confirme la progression et la distance;
- le ressenti donne pourtant l'impression de voler;
- la route ressemble a un ruban dans le vide;
- l'ancrage au sol et les reperes proches sont insuffisants;
- l'iteration Unity est beaucoup plus lente que l'iteration React/Vite/Three.js;
- le choix de migrer vers Unity aussi tot est remis en question.

Probleme precise:

```text
Unity a permis de prouver des contrats techniques, mais il ralentit la boucle
de creation et ne produit pas encore une demo privee plus convaincante que le
prototype Three.js. Pour la demo privee, continuer Unity maintenant augmente le
risque de perdre l'elan sans ameliorer le ressenti assez vite.
```

Type de changement:

- failed approach requiring different solution;
- correction de strategie de production demo;
- pas un rollback destructif.

## 2. Impact Analysis

### Epic Impact

Epic 1 reste `done`: la boucle mock web a deja prouve la valeur principale.

MYB-11 a MYB-17 restent utiles comme spikes/vertical slice Unity, mais ne
doivent plus imposer Unity comme chemin critique de la demo privee.

Aucun nouvel epic n'est necessaire maintenant. Le changement tient dans une
story unique de recovery demo cote web.

### Story Impact

Stories conservees:

- MYB-2 a MYB-10: reference jouable React/Vite/Three.js.
- MYB-11 a MYB-17: apprentissages Unity, validation de concepts, preuves de
  limites d'iteration.

Story suivante recommandee:

```text
MYB-18 Three.js private demo recovery and ride grounding
```

Cette story doit rapatrier dans le prototype web uniquement ce qui a ete utile
cote Unity:

- pentes simples controlees;
- vie legere stylisee;
- fog/profondeur preservee;
- validation par video et screenshots reels;
- criteres de ressenti: route ancree, camera moins flottante, reperes proches.

### Artifact Conflicts

Le GDD court original verrouille:

```text
Stack cible verrouillee: Vite, React, TypeScript, Three.js.
```

Le correct-course post-MYB-10 avait ensuite deplace Unity en cible de production.
Cette decision est maintenant requalifiee:

```text
Three.js redevient la cible principale de la demo privee.
Unity reste en parking/recherche, non supprime, non poursuivi comme chemin
critique tant que la demo web n'est pas convaincante.
```

Architecture:

- `echappee-3d-architecture-mince.md` redevient la base active pour la demo.
- `echappee-3d-architecture-unity-mince-2026-06-07.md` devient une reference
  de spike Unity, pas la cible active.

Linear:

- ne pas creer de nouvelle issue dans cette passe;
- ajouter une note projet / status update pour documenter la correction;
- prochaine passe seulement: creer une story unique MYB-18 si validee.

## 3. Options Comparees

| Option | Valeur | Risque | Verdict |
| --- | --- | --- | --- |
| Continuer Unity et corriger camera/grounding | Conserve le cap Unity recent. | Iteration lente, build WebGL lourd, MCP fragile, risque de passer encore plusieurs stories avant une demo agreable. | Non recommande maintenant. |
| Rollback Unity | Nettoie mentalement la direction. | Detruit des apprentissages et peut provoquer du churn inutile. | Non recommande. |
| Revenir Three.js pour la demo privee, garder Unity en parking | Retrouve la vitesse d'iteration et s'appuie sur la reference jouable qui marchait deja mieux. | Il faudra accepter que Unity n'est pas la cible court terme. | Recommande. |

## 4. Recommended Approach

Approche recommandee: direct adjustment.

Decision:

```text
Three.js redevient la cible principale de la demo privee Echappee 3D.
Unity est conserve en parking/recherche et comme source d'apprentissages, sans
suppression du projet Unity et sans nouvelles stories Unity immediates.
```

Rationale:

- la demo privee a besoin d'un ressenti convaincant plus que d'un packaging
  engine;
- le prototype web avait deja une boucle d'iteration rapide avec capture video;
- les validations Unity ont montre des composants corrects mais pas assez
  l'experience percue;
- la prochaine valeur utilisateur se trouve dans le grounding visuel, pas dans
  un nouveau packaging Unity.

Effort estime: low to medium.

Risque: medium si la story web tente de rapatrier trop de choses. Mitigation:
une seule story, scope strict, acceptance basee sur capture video.

## 5. Detailed Change Proposals

### Sprint Status

OLD:

```yaml
next_action: "commit-MYB-17"
post_mvp_after_myb10_unity_migration_direction: "React/Three.js prototype de reference; Unity cible de production"
```

NEW:

```yaml
next_action: "create-threejs-private-demo-recovery-story"
post_mvp_after_myb17_direction_decision: "Three.js redevient cible demo privee; Unity reste parking/recherche, non supprime."
post_mvp_after_myb17_threejs_recovery_next_action: "create-MYB-18-threejs-private-demo-recovery-story"
```

Rationale: MYB-17 est termine; la prochaine action doit creer une story web
unique qui corrige le ressenti demo.

### Architecture

OLD:

```text
Unity devient la cible de production.
```

NEW:

```text
Three.js est la cible active de la demo privee.
Unity reste un spike de production potentiel, conserve mais non prioritaire.
```

Rationale: la demo WebGL Unity a montre une limite de qualite ressentie et
d'iteration; l'architecture active doit suivre le chemin de livraison le plus
rapide.

### Prochaine Story

Titre candidat:

```text
MYB-18 Three.js private demo recovery and ride grounding
```

Scope strict:

- web uniquement: `src/ride/*`, `src/render/*`, `src/app/*` si necessaire;
- pas de Unity;
- pas de Meshy;
- pas d'asset externe;
- pas de backend;
- pas de BLE/FTMS;
- rapatrier seulement les apprentissages Unity utiles a la demo:
  - pentes simples;
  - route mieux ancree au sol;
  - camera plus basse / moins flottante;
  - bords/epaules/reperes proches;
  - vie legere stylisee si elle aide la perception;
  - fog/profondeur conservee.

Acceptance principale:

```text
Une capture video de 60 secondes du prototype web montre une avancee lisible,
une route ancree, une camera confortable et une absence de flickering/disparition
majeure sur le parcours demo.
```

## 6. Checklist gds-correct-course

- [x] 1.1 Trigger identifie: demo WebGL MYB-17 et capture reelle.
- [x] 1.2 Probleme defini: Unity ralentit la demo et produit un ressenti moins
  convaincant que le prototype Three.js.
- [x] 1.3 Evidence collectee: build WebGL, capture video, screenshots, feedback
  utilisateur sur impression de vol.
- [x] 2.1 Epic courant evalue: Epic web mock reste valide et termine.
- [x] 2.2 Changement epic: aucun nouvel epic.
- [x] 2.3 Futures priorites: stopper nouvelles stories Unity immediates.
- [x] 2.4 Backlog: ne pas creer de backlog large.
- [x] 2.5 Priorite: story unique Three.js recovery.
- [x] 3.1 GDD: retour vers la stack initiale pour la demo privee.
- [x] 3.2 Architecture: architecture Three.js mince redevient active; Unity
  passe en parking/recherche.
- [x] 3.3 UX: grounding, confort camera, lisibilite route et capture video
  deviennent les criteres centraux.
- [x] 3.4 Autres artifacts: sprint-status et linear-sync a mettre a jour.
- [x] 4.1 Direct adjustment evalue et retenu.
- [x] 4.2 Rollback evalue et rejete.
- [x] 4.3 MVP review evalue: MVP reste la demo mock privee, pas une release.
- [x] 4.4 Path forward choisi: Three.js private demo recovery.
- [x] 5.1 a 5.5 Proposal et handoff definis.
- [x] 6.1 a 6.5 Final review: direction approuvee dans le prompt utilisateur.

## 7. Implementation Handoff

Scope classification: moderate.

Routage:

- `gds-create-story` pour creer une seule story MYB-18.
- `gds-dev-story` ensuite pour implementation web bornee.
- `gds-code-review` avec validation video obligatoire.

Success criteria du handoff:

- pas de nouvelle story Unity tant que la demo web n'est pas a nouveau
  convaincante;
- pas de suppression du projet Unity;
- pas de backlog large;
- story MYB-18 creee avec AC precis et capture video comme preuve principale.

## 8. Prompt suivant

```text
gds-create-story

  Creer une story unique : MYB-18 Three.js private demo recovery and ride grounding.

  Contexte :
  - MYB-17 Unity private demo package and launch checklist : Done.
  - Commit MYB-17 :
    6794394b5d878ac431a888dcad94a783dca59577
  - Correct-course post-MYB-17 :
    _bmad-output/planning-artifacts/sprint-change-proposal-threejs-demo-recovery-2026-06-07.md
  - Build Unity WebGL local :
    http://127.0.0.1:8087/
  - Capture Unity WebGL :
    _bmad-output/video-captures/unity-webgl-running-2026-06-07T19-59-58-250Z/
  - Observation :
    - la session avance ;
    - le ressenti donne l'impression de voler ;
    - route trop ruban dans le vide ;
    - manque d'ancrage au sol et de reperes proches ;
    - iteration Unity trop lente pour la demo privee.

  Decision :
  - Three.js redevient la cible principale de la demo privee.
  - Unity reste conserve en parking/recherche, sans suppression.
  - Ne pas creer de nouveau backlog large.

  Objectif MYB-18 :
  - Recuperer une demo privee web convaincante.
  - Ameliorer le grounding visuel : route ancree, camera moins flottante, reperes proches.
  - Rapatrier seulement les acquis utiles de Unity : pentes simples, vie legere si utile, fog/profondeur, validation video.

  Scope strict :
  - Web uniquement React/Vite/Three.js.
  - Ne pas modifier unity/Echappee3D.
  - Pas de Meshy.
  - Pas d'asset externe.
  - Pas de backend.
  - Pas de BLE/FTMS.
  - Pas de nouvelle migration moteur.
  - Pas de suppression du prototype Unity.

  Attendus story :
  - AC precis.
  - Taches d'implementation web.
  - Fichiers probables.
  - Validation attendue :
    - npm run typecheck ;
    - npm run test ;
    - npm run build ;
    - serveur local Vite ;
    - capture video 60 secondes + screenshots ;
    - preuve que la route reste visible, ancree, sans flickering/disparition majeure.

  Tracking :
  - Creer Linear MYB-18 dans l'equipe MYB / projet existant.
  - Passer MYB-18 en In Progress.
  - Mettre sprint-status.yaml avec next_action: implement-MYB-18.
  - Mettre a jour _bmad-output/linear-sync.md.

  A la fin :
  - Donner l'URL Linear MYB-18.
  - Donner le chemin de la story locale.
  - Donner le prompt suivant pour commit story/tracking + implementation MYB-18.
```

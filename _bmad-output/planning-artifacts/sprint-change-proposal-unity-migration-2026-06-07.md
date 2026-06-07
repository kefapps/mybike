---
title: "Sprint Change Proposal - Migration Unity progressive"
project: "mybike / Echappee 3D"
date: "2026-06-07"
workflow: "gds-correct-course"
status: "approved-direction"
source_commit: "0a580f9890d2772497ff1d22688fb10c6500e95e"
source_commit_label: "MYB-10 motion density pass"
source_feedback: "_bmad-output/implementation-artifacts/playtest-human-lite-post-myb10-resultats-2026-06-07.md"
next_action: "create-unity-migration-architecture"
linear_sync: "project-note"
---

# Sprint Change Proposal - Migration Unity progressive

## 1. Resume de l'enjeu

MYB-10 est termine et committe:
`0a580f9890d2772497ff1d22688fb10c6500e95e` (`MYB-10 motion density pass`).

Le prototype React + Vite + Three.js a atteint son objectif de reference:
une vertical slice mock jouable, validable, avec route, camera, progression,
HUD, controles mock, biomes proceduraux, brouillard et captures video.

Le playtest humain post-MYB-10 a revele les signaux suivants:

- la route disparait parfois;
- le rendu presente pas mal de flickering;
- les pentes montantes/descendantes deviennent un besoin produit;
- la vie autour de la route est desiree: oiseaux, humains, vehicules qui
  arrivent ou doublent;
- le rendu est acceptable pour un POC, mais tres loin d'une version finale;
- le brouillard / effet de longue distance est apprecie.

Decision utilisateur a acter: ne plus continuer a optimiser Three.js comme
destination finale. React/Three.js devient le prototype de reference.
Unity devient la cible de production.

## 2. Analyse d'impact

### Epic et stories

- Epic 1 reste `done`: il a prouve la boucle de ride mock.
- MYB-2 a MYB-10 restent des preuves et specifications de migration.
- Le prochain pas `create-render-stability-story` est remplace par
  `create-unity-migration-architecture`.
- Aucune nouvelle issue Linear n'est creee dans cette passe.
- Aucun gros backlog Unity n'est cree dans cette passe.

### GDD

Le GDD court verrouille encore `Vite, React, TypeScript, Three.js` comme stack
cible. Cette decision devient historique: elle decrit le prototype, plus la
cible de production.

Le prochain artifact d'architecture Unity devra clarifier:

- prototype de reference: React + Vite + Three.js jusqu'a MYB-10;
- cible de production: Unity;
- experience conservee: balade mock en premiere personne sur rail, HUD minimal,
  progression simple, brouillard/profondeur.

### Architecture

L'architecture mince actuelle reste utile car elle separe deja:

- logique mock;
- vitesse/progression/stats;
- route/camera;
- rendu depuis snapshot;
- HUD et resume.

Elle ne doit pas etre jetee. Elle doit servir de spec fonctionnelle pour une
architecture Unity mince, en traduisant les contrats plutot qu'en reprenant la
stack web comme cible finale.

### Technique

- Aucun code applicatif ne change dans cette passe.
- Aucun test npm/build n'est lance, car la passe est docs/tracking uniquement.
- Meshy reste disponible mais non utilise.
- Unity AI / Unity MCP restent des pistes possibles, non configurees localement.
- Les captures video, retours playtest et learnings camera/route deviennent des
  inputs de migration.

## 3. Options comparees

| Option | Avantage | Risque | Verdict |
| --- | --- | --- | --- |
| Continuer le hardening Three.js | Corrige potentiellement route/flickering sans changer de moteur. | Investit dans une destination qui n'est plus souhaitee; risque de repousser les vrais sujets Unity: relief, vie, assets, rendu final. | Non recommande comme direction principale. |
| Migration Unity progressive | Garde les acquis, reduit le risque, prouve vite une vertical slice mock equivalente. | Demande une architecture de migration et un projet Unity isole avant d'implementer. | Recommande. |
| Reecriture Unity complete immediate | Permet de repartir sur la cible finale sans compromis. | Trop large, risque de perdre les specs validees, gros backlog implicite, faible signal rapide. | Non recommande maintenant. |

## 4. Recommandation

Recommandation: migration Unity progressive.

Le premier increment Unity doit etre une vertical slice mock equivalente, pas
une version finale. L'objectif n'est pas encore de produire le rendu final, ni
de brancher BLE/FTMS, ni de generer des assets Meshy. L'objectif est de prouver
que Unity peut reprendre la boucle validee:

`input mock -> vitesse/progression -> route/camera -> HUD minimal -> profondeur`

Cette approche conserve la valeur deja creee et evite de prolonger un hardening
Three.js qui ne serait plus la cible de production.

## 5. Scope du premier increment Unity

Inclus:

- projet Unity isole dans le repo ou sous-dossier clairement separe;
- route visible stable;
- camera premiere personne ou camera route;
- slider/input mock equivalent;
- progression simple;
- HUD minimal;
- brouillard / profondeur preserves;
- logique de migration lisible depuis les artifacts MYB-2 a MYB-10.

Exclus:

- Meshy;
- BLE, Web Bluetooth, FTMS, velo reel;
- vehicules, humains, oiseaux, sauf placeholders tres simples si vraiment
  utiles au cadrage;
- assets lourds;
- pipeline asset IA;
- reecriture complete de toutes les ambitions produit;
- gros backlog complet;
- optimisation Three.js comme destination finale.

## 6. Propositions de changement d'artifacts

### Sprint status

OLD:

```yaml
next_action: "create-render-stability-story"
```

NEW:

```yaml
next_action: "create-unity-migration-architecture"
```

Rationale: le hardening rendu Three.js n'est plus le prochain pas principal;
il devient un apprentissage du prototype a transmettre a Unity.

### Architecture

OLD:

```text
Stack MVP: Vite + React + TypeScript + Three.js.
```

NEW attendu dans le prochain artifact:

```text
Prototype de reference: React + Vite + Three.js jusqu'a MYB-10.
Cible de production: Unity.
Premier increment: vertical slice mock Unity equivalente.
```

Rationale: ne pas modifier brutalement l'architecture existante dans cette
passe; creer plutot une architecture Unity mince qui reference les acquis.

### Linear

Action actuelle:

- ajouter une note au projet Linear existant;
- ne pas creer de nouvelle issue;
- ne pas creer de nouveau projet, epic ou backlog.

Action suivante apres architecture:

- seulement si l'architecture Unity mince est approuvee, cadrer une petite
  story Unity de premier increment.

## 7. Checklist gds-correct-course

- [x] 1.1 Trigger identifie: retours humains post-MYB-10.
- [x] 1.2 Probleme defini: changement strategique de cible moteur, du prototype
  web vers Unity production.
- [x] 1.3 Evidence collectee: route qui disparait, flickering, besoin de pentes,
  vie du monde, rendu POC, brouillard apprecie.
- [x] 2.1 Epic courant evalue: Epic 1 reste termine et utile.
- [x] 2.2 Changement epic: pas de nouvel epic dans cette passe.
- [x] 2.3 Futures priorites: hardening Three.js remplace par architecture Unity.
- [x] 2.4 Backlog: pas de backlog complet cree.
- [x] 2.5 Priorite: architecture Unity mince avant story.
- [x] 3.1 GDD: stack cible a requalifier dans le prochain artifact.
- [x] 3.2 Architecture: architecture Unity mince requise.
- [x] 3.3 UX: conserver HUD, controls mock, profondeur, comprehension immediate.
- [x] 3.4 Autres artifacts: sprint-status et linear-sync a mettre a jour.
- [x] 4.1 Option hardening Three.js evaluee.
- [x] 4.2 Option rollback evaluee: non utile, rien a jeter.
- [x] 4.3 Option review MVP evaluee: MVP valide comme prototype de reference.
- [x] 4.4 Option recommandee: migration Unity progressive.
- [x] 5.1 a 5.5 Proposal et handoff definis.
- [x] 6.1 a 6.5 Final review: decision utilisateur deja fournie; handoff vers
  `gds-game-architecture`.

## 8. Handoff

Scope classification: moderate.

Routage: `gds-game-architecture`.

Handoff attendu: creer une architecture Unity mince pour le premier increment
de migration, en restant au niveau vertical slice mock equivalente.

Prompt suivant:

```text
gds-game-architecture

Creer une architecture Unity mince pour la migration progressive d'Echappee 3D / mybike.

Sources:
- _bmad-output/planning-artifacts/sprint-change-proposal-unity-migration-2026-06-07.md
- _bmad-output/planning-artifacts/echappee-3d-gdd-court.md
- _bmad-output/planning-artifacts/echappee-3d-architecture-mince.md
- _bmad-output/planning-artifacts/echappee-3d-mvp-epic-stories.md
- _bmad-output/implementation-artifacts/playtest-human-lite-post-myb10-resultats-2026-06-07.md

Decision:
- React/Vite/Three.js devient prototype de reference.
- Unity devient cible de production.
- Ne pas jeter les artifacts existants: les traduire en specs de migration.

Premier increment Unity:
- projet Unity isole dans le repo ou sous-dossier clairement separe;
- route visible stable;
- camera premiere personne ou camera route;
- input mock/slider equivalent;
- progression simple;
- HUD minimal;
- brouillard/profondeur preserves;
- pas encore Meshy;
- pas encore BLE/FTMS;
- pas encore assets lourds;
- pas de gros backlog.

Sortie attendue:
- architecture Unity mince sous _bmad-output/planning-artifacts/;
- contrats de modules Unity minimum;
- risques et validations;
- handoff pour creer une seule petite story Unity de vertical slice mock equivalente.
```

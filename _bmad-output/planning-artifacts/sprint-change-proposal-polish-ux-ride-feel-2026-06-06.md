---
title: "Sprint Change Proposal - Polish UX / sensation de ride"
project: "mybike"
date: "2026-06-06"
workflow: "gds-correct-course"
status: "decision-ready"
source_playtest_plan: "_bmad-output/implementation-artifacts/playtest-polish-ux-ride-feel-2026-06-06.md"
source_playtest_results: "_bmad-output/implementation-artifacts/playtest-polish-ux-ride-feel-results-2026-06-06.md"
source_retrospective: "_bmad-output/implementation-artifacts/epic-1-retro-2026-06-06.md"
---

# Sprint Change Proposal - Polish UX / sensation de ride

## 1. Declencheur

Le self-playtest post-MVP a valide la boucle mock jouable, mais a fait ressortir
trois irritants de ressenti:

- le HUD mobile occupe trop d'espace et reduit la lecture de la scene;
- la reponse du slider fonctionne, mais son effet reste trop indirect a cause du
  smoothing;
- le changement de biome n'est pas visible dans un playtest court.

## 2. Decision

Selectionner un mini-scope unique: une seule petite story polish regroupant les
3 ajustements proposes par le self-playtest.

Ce n'est pas un nouveau chantier ni un backlog post-MVP large. C'est un passage
de finition coherent sur la sensation de ride mock, directement issu de preuves
observables.

## 3. Ajustements retenus

1. Compacter le HUD mobile pour liberer la scene.
2. Rendre la reponse du slider plus comprehensible.
3. Rendre le changement de biome observable pendant un playtest court.

## 4. Impact sur les artefacts

### GDD

Pas de changement de vision produit. Le mode mock reste le centre du MVP, sans
hardware connecte ni nouvelle boucle de gameplay.

### Architecture

Les frontieres existantes restent valides:

- React gere HUD, controles et etats d'ecran;
- `ride` conserve la logique pure de vitesse, smoothing et stats;
- `route` conserve progression, sampling et biomes;
- Three.js rend seulement un snapshot deja calcule.

La story future devra eviter de melanger logique ride, UI React et rendu Three.js
dans un meme module.

### Epics / stories

Ne pas rouvrir Epic 1. La story future doit etre une petite story polish
post-MVP, bornee aux 3 ajustements selectionnes.

### Linear

Ne pas creer d'issue Linear pendant cette decision. La prochaine action est de
creer une story locale BMAD, puis de decider explicitement si elle doit etre
sync dans Linear.

## 5. Hors scope

- BLE, FTMS, Web Bluetooth et velo connecte.
- Backend, compte utilisateur, cloud ou persistance historique.
- Meshy, assets externes lourds ou pipeline asset.
- Nouvelle route complete ou extension de contenu.
- Refonte UI/HUD globale.
- Nouveau mode de gameplay.
- Optimisation bundle Three.js.
- Hardening `WebGLRenderer`, sauf si un echec reel est reproduit.
- Creation de backlog large ou de plusieurs issues Linear.

## 6. Handoff implementation

Changement recommande: petite story polish unique.

Titre propose:

```text
Polish UX / sensation de ride mock
```

Prompt de creation de story:

```text
gds-create-story

Creer une petite story polish post-MVP pour Echappee 3D.

Sources:
- _bmad-output/planning-artifacts/sprint-change-proposal-polish-ux-ride-feel-2026-06-06.md
- _bmad-output/implementation-artifacts/polish-ux-ride-feel-mini-scope-2026-06-06.md
- _bmad-output/implementation-artifacts/playtest-polish-ux-ride-feel-results-2026-06-06.md
- _bmad-output/implementation-artifacts/sprint-status.yaml
- _bmad-output/linear-sync.md

Objectif:
- Creer une seule petite story polish, pas un backlog.
- Inclure exactement ces 3 ajustements:
  1. HUD mobile plus compact pour liberer la scene.
  2. Feedback slider plus comprehensible.
  3. Biome observable pendant un playtest court.
- Garder mock mode obligatoire.
- Respecter les frontieres: React pour UI, ride/route purs et testables, Three.js
  seulement pour le rendu.
- Ne pas creer d'issue Linear sauf demande explicite.

Sortie attendue:
- story locale BMAD dans _bmad-output/implementation-artifacts/
- sprint-status.yaml avec next_action: implement-polish-story
- linear-sync.md mis a jour sans creation d'issue.
```

## 7. Prochaine action

`create-polish-story`

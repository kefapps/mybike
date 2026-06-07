---
title: "Playtest humain leger post-MYB-9"
project: "mybike"
date: "2026-06-07"
workflow: "gds-playtest-plan"
status: "ready-for-human-session"
source_commit: "4963300704e51da458f714809498aa355fba869b"
source_retro: "_bmad-output/implementation-artifacts/myb-9-visual-mini-retro-2026-06-07.md"
source_visual_proof: "_bmad-output/video-captures/ride-visual-audit-2026-06-06T23-54-18-055Z/"
target_url: "http://127.0.0.1:5174/"
next_action_after_session: "record-human-playtest-results"
preflight_status: "passed"
---

# Playtest humain leger post-MYB-9

## Objectif

Verifier avec une personne humaine si la vertical slice mock post-MYB-9 est
comprehensible, agreable et visuellement assez evocatrice pour guider la suite.

Decision a informer:
- playtest prive seulement ou deploiement public leger;
- Meshy cible pour 1 a 2 landmarks ou non;
- nouveau pass procedural ou arret du polish visuel pour l'instant.

## Format

- Type: playtest humain leger, prive.
- Participant: 1 personne proche projet ou non specialiste.
- Plateforme prioritaire: desktop navigateur avec WebGL.
- Plateforme optionnelle: mobile ou viewport etroit si disponible.
- Intervention: ne pas expliquer le HUD ou les controles avant observation,
  sauf blocage technique.
- Sortie attendue: 3 observations maximum et une decision simple.

## Preflight technique

- `npm run build`: passe.
- Warning connu: chunk Three.js > 500 kB apres minification.
- Serveur local: `http://127.0.0.1:5174/`.
- Check HTTP: `200 OK`.
- Aucune modification de code applicatif pour lancer ce playtest.

## Setup facilitateur

1. Ouvrir `http://127.0.0.1:5174/`.
2. Dire seulement: "C'est une balade velo 3D mock. Lance la balade, explore
   les controles visibles, puis dis ce que tu comprends et ressens."
3. Laisser le participant jouer sans correction immediate.
4. Noter les reactions courtes et les moments de confusion.

## Parcours

1. Lancer la balade mock.
2. Observer la scene sans toucher au slider pendant quelques instants.
3. Changer le slider vers bas, moyen, puis haut.
4. Observer si la route, le decor, le HUD et la progression restent lisibles.
5. Mettre en pause, reprendre, puis terminer.
6. Lire le resume.
7. Optionnel: refaire rapidement le debut sur mobile ou viewport etroit.

## Grille d'observation

| Zone | Question | Note 1-3 | Observation courte |
| --- | --- | ---: | --- |
| Premier contact | Le participant comprend-il quoi faire sans explication ? |  |  |
| Ride feel | Le lien effort, vitesse et progression semble-t-il clair ? |  |  |
| HUD / controles | Les informations utiles restent-elles lisibles pendant le ride ? |  |  |
| Identite visuelle | Coast, forest, relief et mood sont-ils percus comme un lieu compose ? |  |  |
| Attrait public | La personne accepterait-elle de montrer ce prototype a quelqu'un d'autre ? |  |  |
| Meshy potentiel | La personne nomme-t-elle un objet/landmark precis qui manque ? |  |  |

Notation:
- 1 = confusion ou friction nette.
- 2 = acceptable mais perfectible.
- 3 = clair, agreable, pas d'action evidente.

## Questions apres session

1. Qu'as-tu compris du jeu sans qu'on te l'explique ?
2. Quel moment ou element visuel t'a le plus marque ?
3. Qu'est-ce qui parait encore prototype ou pas pret ?
4. Le controle d'effort change-t-il quelque chose pour toi ?
5. Montrerais-tu cette version a quelqu'un comme demo courte ?
6. Si un seul objet 3D devait etre ajoute, lequel serait utile ?

## Regles de decision

- Deploiement public leger: seulement si aucune zone critique n'est notee `1`
  et si l'attrait public est note `2` ou `3`.
- Meshy cible: seulement si le participant nomme un landmark precis et
  recurrent dans le feedback, avec maximum 1 a 2 assets et confirmation du
  cout avant tout appel.
- Nouveau pass procedural: seulement si l'identite visuelle ou la lisibilite du
  lieu est notee `1`.
- Pas de nouvelle action: possible si les notes sont majoritairement `3` et
  aucun manque concret n'est nomme.

## Template resultats

```text
Participant:
Plateforme:
Blocage technique: oui/non

Scores:
- Premier contact:
- Ride feel:
- HUD / controles:
- Identite visuelle:
- Attrait public:
- Meshy potentiel:

3 observations maximum:
1.
2.
3.

Decision:
- public leger / playtest prive seulement:
- Meshy cible:
- nouveau pass procedural:
- prochaine action recommandee:
```

## Garde-fous

- Ne pas creer de nouvelle issue Linear pendant la session.
- Ne pas transformer les retours en backlog large.
- Ne pas appeler Meshy pendant la session.
- Garder le mock mode comme experience testee.

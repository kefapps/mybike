---
title: "Playtest humain leger post-MYB-10"
project: "Echappee 3D - Vertical Slice Mock"
date: "2026-06-07"
workflow: "gds-playtest-plan"
status: "ready-for-human-session"
source_commit: "d20b554"
source_retro: "_bmad-output/implementation-artifacts/myb-10-visual-mini-retro-2026-06-07.md"
source_previous_plan: "_bmad-output/implementation-artifacts/playtest-human-lite-post-myb9-2026-06-07.md"
source_visual_proof: "_bmad-output/video-captures/ride-visual-audit-2026-06-07T06-39-46-859Z/"
target_url: "http://127.0.0.1:5174/"
session_duration: "10-15 minutes"
next_action_after_session: "collect-human-playtest-feedback-post-MYB-10"
preflight_status: "not rerun; MYB-10 review capture evidence reused"
---

# Playtest humain leger post-MYB-10

## Objectif

Verifier avec une personne humaine si la vertical slice mock post-MYB-10 est
immediatement comprehensible, assez agreable a conduire, lisible visuellement et
suffisante pour une demo privee controlee.

Decision a prendre en sortie:

1. `demo-privee-ok`
2. `hardening-technique-necessaire`
3. `meshy-cible-utile`
4. `ajustement-visuel-precis-necessaire`

## Build et preuves disponibles

- Build vise: Echappee 3D mock vertical slice apres MYB-10 Motion & Density Pass.
- URL cible: `http://127.0.0.1:5174/`.
- Preuve visuelle MYB-10:
  `_bmad-output/video-captures/ride-visual-audit-2026-06-07T06-39-46-859Z/`.
- Capture: HTTP `200`, `pageErrors: []`, 30 s, 6 frames, HUD masque pour audit
  visuel, un 404 isole connu.
- Validation MYB-10 deja effectuee pendant la revue: `npm run typecheck`,
  `npm run test`, `npm run build`, `npm run capture:ride-video`.

Aucun nouveau pass visuel, aucun appel Meshy et aucune validation npm ne sont
necessaires pour preparer ce plan.

## Participant

- Nombre: 1 personne.
- Profil: proche projet ou non specialiste, pas besoin d'experience velo.
- Plateforme prioritaire: desktop navigateur avec WebGL.
- Plateforme optionnelle: mobile ou viewport etroit seulement si disponible sans
  rallonger la session.

## Script facilitateur

Dire uniquement:

> C'est une balade velo 3D mock. Lance la balade, explore les controles visibles,
> puis dis ce que tu comprends et ressens.

Ne pas expliquer le HUD, le slider ou la pause avant observation. Intervenir
seulement en cas de blocage technique, detresse, ou si le participant demande
explicitement de l'aide.

## Session 10-15 minutes

| Temps | Etape | A observer |
| --- | --- | --- |
| 0:00-1:00 | Accueil court | Le participant sait qu'on teste le jeu, pas lui. |
| 1:00-3:00 | Premier contact sans aide | Comprend-il comment lancer et quoi regarder ? |
| 3:00-7:00 | Ride libre | Ride feel, vitesse percue, lisibilite route/decor. |
| 7:00-10:00 | Slider bas/moyen/haut | Le lien effort, vitesse, progression reste-t-il clair ? |
| 10:00-12:00 | Pause, reprise, fin/resume | HUD, controles et resume restent-ils comprehensibles ? |
| 12:00-15:00 | Questions rapides | Decision de sortie et prochain pas. |

## Grille de prise de notes

Notation:

- `1` = confusion, friction ou blocage net.
- `2` = acceptable mais perfectible.
- `3` = clair, agreable, pas d'action evidente.

| Zone | Question | Note 1-3 | Observation en une phrase |
| --- | --- | ---: | --- |
| Comprehension immediate | Comprend-il quoi faire sans explication ? |  |  |
| Ride feel | La vitesse et l'effort donnent-ils une sensation de balade ? |  |  |
| Lisibilite route | La route et les reperes proches restent-ils lisibles ? |  |  |
| HUD / controles | Les infos et controles utiles sont-ils compris pendant le ride ? |  |  |
| Valeur demo privee | Montrerait-il cette version comme demo courte controlee ? |  |  |
| Manque precis | Nomme-t-il un objet, landmark ou ajustement concret ? |  |  |

## Questions apres session

1. Qu'as-tu compris du jeu sans explication ?
2. A quel moment as-tu senti le plus le mouvement ou la vitesse ?
3. Qu'est-ce qui t'a paru encore prototype ou fragile ?
4. Le HUD et les controles t'ont-ils aide ou distrait ?
5. Montrerais-tu cette version a quelqu'un en demo privee courte ?
6. S'il fallait ajouter ou corriger une seule chose, laquelle ?

## Criteres de decision

### 1. Demo privee OK

Choisir cette sortie si:

- aucun blocage technique n'empeche le parcours;
- comprehension immediate, ride feel, route et HUD/controles sont notes `2` ou
  `3`;
- valeur demo privee est notee `2` ou `3`;
- aucun manque precis ne bloque la comprehension ou l'interet.

### 2. Hardening technique necessaire

Choisir cette sortie si:

- WebGL, chargement, controles, pause/reprise ou fin de ride bloquent la session;
- le participant ne peut pas terminer le parcours a cause d'un probleme
  technique;
- le ressenti est negatif principalement a cause de stabilite, performance,
  taille d'ecran ou fiabilite.

### 3. Meshy cible utile

Choisir cette sortie seulement si:

- le participant nomme spontanement un objet ou landmark 3D precis qui manque;
- ce besoin semble difficile a traiter proprement en procedural simple;
- le scope reste limite a 1 ou 2 assets maximum;
- un futur appel Meshy ferait l'objet d'une confirmation de cout avant execution.

### 4. Ajustement visuel precis necessaire

Choisir cette sortie si:

- route, decor, profondeur, vitesse percue ou HUD recoivent une note `1`;
- le probleme n'est pas d'abord technique;
- le feedback pointe un ajustement concret, par exemple densite trop forte,
  route trop peu lisible, vitesse insuffisamment ressentie, HUD qui distrait, ou
  biome pas identifiable.

## Template resultats

```text
Participant:
Date:
Plateforme:
URL:
Blocage technique: oui/non

Scores:
- Comprehension immediate:
- Ride feel:
- Lisibilite route:
- HUD / controles:
- Valeur demo privee:
- Manque precis:

3 observations maximum:
1.
2.
3.

Decision unique:
- demo-privee-ok / hardening-technique-necessaire / meshy-cible-utile / ajustement-visuel-precis-necessaire

Justification en 2 phrases:

Prochaine action recommandee:
```

## Garde-fous

- Ne pas creer de nouvelle issue Linear pendant ou apres ce plan.
- Ne pas transformer un retour unique en backlog large.
- Ne pas appeler Meshy pendant la preparation ou la session.
- Ne pas lancer de nouveau pass visuel procedural avant retour humain.
- Garder le mock mode comme experience testee.

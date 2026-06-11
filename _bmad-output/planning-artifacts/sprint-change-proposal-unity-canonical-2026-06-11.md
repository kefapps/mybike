# Sprint Change Proposal - Unity/Echapee4D devient canonique

Date: 2026-06-11

Projet: mybike / Echappee 3D

Note de mise a jour MYB-94: ce document reste la trace du pivot moteur
MYB-39/MYB-90 vers Unity. La priorite plateforme active a ensuite ete precisee
par `MYB-94`: Unity macOS-first; WebGL devient une validation secondaire quand
un ticket le demande explicitement.

Decision utilisateur: travailler a partir de maintenant sur le projet Unity et
mettre Three.js de cote.

## 1. Issue Summary

La video MYB-90 a montre que le projet propre `unity/Echapee4D`, pilote par
IvanMurzak Unity-MCP, peut produire une preuve WebGL locale lisible. Cette
preuve change la decision moteur: la roadmap active quitte React/Vite/Three.js
et devient Unity-first. La priorite plateforme active est maintenant Unity
macOS-first, avec WebGL secondaire.

Trigger:

- `MYB-89`: Unity-MCP IvanMurzak a permis de creer une scene propre, validee et
  capturee dans Unity.
- `MYB-90`: cette scene a ete exportee en WebGL, servie localement et capturee
  dans Chromium avec HTTP 200, 0 page errors, 0 console errors et route lisible.
- Feedback utilisateur apres visionnage: Unity devient la cible active; Three.js
  est mis de cote.

## 2. Impact Analysis

Impact epic:

- L'ancien epic MVP web `MYB-1` reste historique et Done.
- `MYB-39` doit devenir l'ADR finale actant Unity/Echapee4D comme cible active.
- Les epics actifs `MYB-29` a `MYB-34` restent valables comme themes produit,
  mais leurs tickets ouverts doivent etre relus en Unity-first.

Impact stories/tickets:

- Tickets Done: ne pas rouvrir ni supprimer; ils restent evidence historique.
- Tickets ouverts explicitement Three.js/React/Vite: renommer ou reecrire en
  equivalents Unity.
- Tickets produit generiques: conserver, mais adapter criteres d'acceptation et
  validations vers Unity macOS-first; WebGL seulement quand le ticket le scope.

Impact artefacts:

- `AGENTS.md`: doit nommer `unity/Echapee4D` comme projet canonique.
- GDD, architecture mince et epic stories MVP: doivent retirer Three.js comme
  cible active.
- `_bmad-output/linear-sync.md`: doit enregistrer la bascule et les IDs Linear.
- `sprint-status.yaml`: doit indiquer Unity canonical comme prochain axe.

Impact technique:

- `src/**` devient parking historique.
- `unity/Echappee3D/**` devient reference historique.
- `unity/Echapee4D` doit etre hygiene: ne pas tracker `Library`, `Temp`,
  `Logs`, `UserSettings`, builds ou captures.
- Les validations actives deviennent Unity-MCP, validators Unity, checks macOS
  first, et preuves WebGL/captures navigateur seulement quand elles sont
  explicitement scopees.

## 3. Recommended Approach

Approche retenue: rebaseline directe, sans rollback.

Rationale:

- Supprimer ou revert l'historique Three.js detruirait des preuves utiles.
- Annuler tout le backlog ferait perdre des besoins produit encore valables.
- Reecrire les tickets ouverts en Unity preserve l'intention produit tout en
  nettoyant l'ambiguite moteur.

Classification: changement majeur de direction, implementation en deux temps:

1. Sync documentaire et Linear.
2. Premier ticket Unity canonical/hardening.

## 4. Detailed Change Proposals

Documents locaux:

- `AGENTS.md`: Unity macOS-first, `unity/Echapee4D` canonical, `src/**` parked.
- `echappee-3d-gdd-court.md`: stack cible Unity macOS-first; WebGL secondaire.
- `echappee-3d-architecture-mince.md`: architecture Unity runtime/validators/build.
- `echappee-3d-mvp-epic-stories.md`: stories U1-U5 Unity.
- `linear-sync.md`: sync des statuts et commentaires.

Linear:

- `MYB-39`: renommer et passer Done comme ADR finale.
- `MYB-89` / `MYB-90`: passer Done, en expliquant qu'ils fondent la decision.
- Tickets ouverts: appliquer label `unity`, conserver les parents, et reecrire
  titres/descriptions pour Unity quand ils parlent encore de React/Three.js.
- Ne pas archiver les tickets historiques Done.

## 5. Implementation Handoff

Responsabilites:

- Developer agent: appliquer les docs locales, synchroniser Linear, valider
  hygiene repo et tracker.
- Future Developer agent: implementer le prochain ticket Unity canonical.
- Product/Architecture: ne rouvrir React/Three.js que par ticket explicite.

Prochain travail recommande:

1. Baseline Unity canonical: scene, README local, validator, hygiene repo.
2. Validation Unity macOS-first; WebGL secondaire seulement si un ticket le scope.
3. Scenic corridor Unity.
4. HUD / UX Unity.
5. Pipeline assets Unity.

## Checklist Correct-Course

- [x] Trigger identifie: video MYB-90 + decision utilisateur.
- [x] Probleme categorise: strategic pivot / failed previous engine direction.
- [x] Evidence collectee: MYB-89, MYB-90, video/capture/build report.
- [x] Impact epic/story analyse.
- [x] Artefacts locaux identifies.
- [x] Path forward choisi: rebaseline directe sans rollback.
- [x] Handoff et validations definis.

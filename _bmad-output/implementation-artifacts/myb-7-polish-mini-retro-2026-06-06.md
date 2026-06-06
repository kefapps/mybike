---
title: "Mini-retro polish - MYB-7"
project: "mybike"
date: "2026-06-06"
story: "MYB-7 - Polish UX / sensation de ride mock"
status: "done"
scope: "post-MVP polish UX / sensation de ride mock"
---

# Mini-retro polish - MYB-7

## Ce qui est livre

`MYB-7` a livre les 3 ajustements retenus apres le self-playtest polish:

1. HUD mobile plus compact pour liberer la scene.
2. Slider plus comprehensible via cible d'allure et vitesse affichee.
3. Changement de biome observable pendant un playtest court.

Le tout reste une story polish courte: pas de nouvelle boucle ride, pas de route
multiple, pas de settings system, pas de BLE/FTMS, pas de deploiement et pas de
nouveau backlog.

## Validations finales

- `npm run typecheck` OK.
- `npm run test` OK, 19 fichiers / 62 tests.
- `npm run build` OK, avec warning Vite attendu sur le chunk Three.js.
- Chrome/CDP desktop et mobile OK: WebGL actif, slider a 90%, cible/affichee
  visibles depuis le snapshot ride, `Ambiance forest` observable en environ 4 s,
  pause/reprise/finish/summary OK, aucun warning/error console.
- HUD mobile autour de 20% du viewport ride apres correction de lisibilite.

## Corrections de revue utiles

- Le feedback slider ne calcule plus de vitesse cible dans React: il lit
  `latestSnapshot.ride.targetSpeedMps`.
- Le feedback de vitesse n'utilise plus `aria-live`, pour eviter les annonces
  continues.
- Les tailles du HUD mobile ont ete remontees sans annuler le compactage.

## Lecons utiles

- Le meilleur polish est reste proche des snapshots existants: `ride` produit les
  valeurs, `route` choisit le biome, React affiche.
- Un seuil biome a 1% est acceptable ici comme outil de playtest mock, parce que
  la story cherche l'observabilite courte et ne pretend pas definir le pacing
  final d'une route.
- Les validations browser legeres capturent des details que les tests unitaires
  ne voient pas: lisibilite HUD, etat pause/reprise, et observabilite du biome.

## Suite recommandee

Committer `MYB-7`, puis garder la prochaine decision post-polish explicite avant
de creer une nouvelle issue: hardening WebGL, automatisation navigateur, contenu
visuel, ou preparation BLE/FTMS, mais un seul axe a la fois.

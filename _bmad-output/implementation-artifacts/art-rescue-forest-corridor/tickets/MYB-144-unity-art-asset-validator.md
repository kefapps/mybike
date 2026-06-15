# MYB-144 — Validator Unity des assets artistiques

## Linear metadata

- **Priority**: P0
- **Labels**: unity, tooling, validation, asset-pipeline
- **Estimate**: 5 pts
- **Depends on**: MYB-143
- **Parent / Project suggéré**: `Art Rescue — Forest Corridor Vertical Slice`

## Contexte

Le projet a besoin d’un douanier. Gentil, mais avec une matraque. Les modèles trop lourds, mal nommés, sans manifest, avec matériaux cassés ou colliders absurdes ne doivent plus passer.

## Objectif

Ajouter un validator Editor Unity qui scanne les assets artistiques et produit un rapport bloquant/non-bloquant.

## Tâches


- Ajouter un outil Editor `MyBikeArtAssetValidator`.
- Scanner `Assets/Echappee/Art`, `Assets/MYB*` et dossiers explicitement configurés.
- Vérifier noms, triangles, matériaux, textures, bounds, colliders, pivots approximatifs, manifest tiers/IA.
- Écrire un rapport dans `_bmad-output/unity-test-results/myb-art-asset-validation.md`.
- Exposer une méthode batchmode appelable par Codex/CI.
- Documenter les seuils dans `docs/validators/unity-art-asset-validator-spec.md`.


## Critères d'acceptation


- Le validator sort un rapport Markdown lisible.
- Le rapport distingue `ERROR`, `WARNING`, `INFO`.
- Les assets sans manifest, matériaux manquants ou textures énormes sont signalés.
- Les limites de triangles sont configurables par catégorie.
- Le ticket n’échoue pas si aucun dossier de production n’existe encore : il doit le signaler proprement.


## Validation attendue


- Lancer Unity en batchmode avec la méthode du validator.
- Vérifier que le rapport est généré même sur une scène vide.
- Ajouter le chemin du rapport au résumé du ticket.


## Hors scope


- Pas de refonte gameplay.
- Pas de nouveau projet Unity.
- Pas de modification de `src/**` sauf besoin explicitement justifié.
- Pas d'appel à Meshy/Tripo sauf ticket dédié.
- Pas de clôture automatique du ticket Linear sans validation visuelle utilisateur.


## Prompt Codex prêt à coller

```txt
Implémente un validator Editor Unity pour assets artistiques.

Projet: `unity/Echapee4D`.

Contraintes:
- Préserver le mode mock.
- Ne pas importer d’assets externes.
- Ne pas appeler Meshy/Tripo.
- Ne pas créer de nouveau projet Unity.

Livrables:
- Script Editor: `Assets/Echappee/Editor/Validation/MyBikeArtAssetValidator.cs` ou chemin cohérent existant.
- Rapport: `_bmad-output/unity-test-results/myb-art-asset-validation.md`
- Doc: `docs/validators/unity-art-asset-validator-spec.md`

Checks minimum:
- noms conformes;
- triangles par mesh;
- nombre de matériaux;
- matériaux manquants/rose;
- textures > seuil;
- bounds aberrants;
- MeshCollider complexe;
- asset tiers/IA sans manifest;
- prefabs sans preview/category si applicable.
```

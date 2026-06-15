# Meshy / Tripo Quarantine Workflow

## Principe

Meshy/Tripo sont autorisés comme générateurs de props isolés, pas comme générateurs du décor principal.

Leur sortie est toujours suspecte jusqu’à preuve du contraire. C’est sain. Ce n’est pas de la paranoïa, c’est du pipeline.

## Assets autorisés

- panneau forestier ;
- souche hero ;
- petit abri ;
- rocher hero ;
- vélo décoratif cassé ;
- portail/borne de départ.

## Assets interdits

- route ;
- terrain ;
- forêt complète ;
- caméra/cockpit ;
- gameplay core ;
- tout asset sans licence/source claire.

## Dossiers

```txt
Assets/Echappee/Art/Quarantine/AI/<provider>/<asset_id>/
Assets/Echappee/Art/Review/AI/<asset_id>/
Assets/Echappee/Art/Production/Forest/Props/<asset_id>/
```

## Étapes

1. Créer une fiche manifest avant l’import Unity.
2. Générer 1 à 3 variantes maximum.
3. Stocker le brut en Quarantine.
4. Ouvrir dans Blender.
5. Vérifier échelle, pivot, orientation, topologie, matériaux.
6. Supprimer géométrie inutile si nécessaire.
7. Réduire textures si nécessaire.
8. Exporter version nettoyée.
9. Importer en Unity Review.
10. Lancer validator.
11. Tester dans scène preview neutre.
12. Décider : approve, reject ou rework.

## Critères de rejet immédiat

- Licence/source floue.
- Mesh trop lourd pour son rôle.
- Matériaux nombreux et incohérents.
- Silhouette confuse.
- Style réaliste incompatible.
- Scale/pivot pénibles à corriger.
- Pas utile depuis caméra route.

## Règle de promotion

Un asset IA ne passe en Production que si :

- manifest complet ;
- validator sans ERROR ;
- capture preview ;
- reviewer humain ;
- statut `approved` ;
- usage limité documenté.

## Bon usage du prompt IA

Prompter des objets isolés, pas des scènes. Ajouter contraintes : `Stylisé
Premium de Production`, formes simples, géométrie propre, pas photoréaliste,
matériaux limités, silhouette claire.

Exemple :

```txt
Stylisé Premium de Production mossy forest signpost for a first-person cycling game, simple readable silhouette, 1-2 materials, no text, no photorealism, clean optimized geometry, game prop, isolated object.
```

## Validation visuelle

Les previews Meshy/Tripo sont des preuves source, pas une validation de
production.

Elles peuvent aider à décider si un asset mérite import, quarantaine, nettoyage
ou rejet immédiat. Elles ne peuvent pas décider que `Premium target` est atteint,
qu’un ticket visuel est `Done`, ou qu’un asset est promu production.

Tout asset IA accepté doit passer par la caméra route dans le corridor de ride
canonique avant d’être considéré production-valid pour Art Rescue.

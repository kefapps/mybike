# Prompt — Unity Art Asset Validator

```txt
Ajoute un validator Editor Unity pour les assets artistiques de MyBike.

Projet canonique: `unity/Echapee4D`.

Il doit scanner `Assets/Echappee/Art` et les dossiers MYB pertinents, puis signaler:
- modèles avec trop de triangles;
- matériaux manquants;
- textures > budget;
- fichiers/noms non conformes;
- bounds trop grands ou trop petits;
- MeshCollider sur mesh complexe;
- assets tiers/IA sans entrée manifest;
- assets en Production avec manifest non approved.

Le validator doit écrire:
`_bmad-output/unity-test-results/myb-art-asset-validation.md`

Contraintes:
- pas de nouveaux assets;
- pas de Meshy/Tripo;
- mode mock préservé;
- méthode batchmode statique pour CI/Codex.
```

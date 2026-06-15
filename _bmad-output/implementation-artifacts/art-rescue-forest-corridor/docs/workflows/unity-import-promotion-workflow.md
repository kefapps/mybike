# Unity Import and Promotion Workflow

## Objectif

Éviter que les assets entrent en production sans contrôle technique, visuel et légal.

## États

```txt
Quarantine -> Review -> Production
              ↘ Rejected
```

## Quarantine

Contient les fichiers bruts externes/IA.

Règles :

- jamais référencé par une scène candidate ;
- manifest obligatoire ;
- pas de confiance dans scale/materials ;
- pas de collider auto accepté.

## Review

Contient les assets nettoyés.

Règles :

- peut apparaître dans scène preview ;
- validator obligatoire ;
- capture preview recommandée ;
- statut `review` dans manifest.

## Production

Contient les assets approuvés.

Règles :

- validator sans ERROR ;
- manifest `approved` ;
- naming conforme ;
- budgets respectés ou exception documentée.
- pour les assets qui affectent le rendu Art Rescue, preuve en contexte de ride
  canonique avant validation visuelle de production.

## Rapport d’import

Chaque promotion doit noter :

- asset id ;
- source ;
- chemin brut ;
- chemin review ;
- chemin production ;
- modifications Blender ;
- résultats validator ;
- reviewer ;
- décision.

## Validation visuelle finale

Un asset n’est pas production-valid parce qu’il est beau en preview. Il devient
production-valid seulement s’il améliore la vue caméra route dans le corridor de
ride canonique.

Les previews asset, scènes sandbox, captures isolées, turntables, rendus Blender
et previews Meshy/Tripo peuvent valider la fabrication ou l’import, mais pas
`Premium target`.

Formule canonique :

```txt
Blender MCP produces candidates.
The ride camera validates production visual quality.
```

## Exception

Une exception est possible pour un asset hero, mais elle doit être explicite dans le manifest : raison, budget dépassé, risque accepté, usage limité.

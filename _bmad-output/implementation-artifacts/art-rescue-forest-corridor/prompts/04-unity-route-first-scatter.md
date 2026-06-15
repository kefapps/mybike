# Prompt — Unity Route-first Scatter

```txt
Dans `unity/Echapee4D`, crée une passe de scatter route-first pour le corridor forêt.

Objectif:
- enrichir les bords de route;
- préserver route readability;
- éviter l’effet alignement de poteaux;
- produire captures et rapport.

Bandes:
1. road: aucun prop;
2. shoulder: feuilles/mousse/racines basses;
3. close edge: troncs/root clusters/rochers moyens;
4. mid edge: densité forêt;
5. back wall: silhouettes/canopée simplifiée.

Contraintes:
- ne pas écraser MYB-137;
- créer scène preview dédiée;
- pas de Meshy/Tripo;
- utiliser assets validés si disponibles;
- générer route.png, overview.png et rapport.

Validation:
- la capture route doit venir de la caméra route canonique dans le corridor de ride canonique;
- overview est obligatoire mais secondaire;
- aucune scène preview, capture isolée ou overview seule ne valide Premium target.

Arbitrage visuel/perf:
- viser `Stylisé Premium de Production`, puis mesurer;
- si la cible FPS est tenue et aucun ERROR bloquant n’existe, enrichir ce qui améliore la caméra route;
- si la perf échoue, réduire d’abord collisions, textures/matériaux, densité hors caméra, LOD/culling/distance, puis densité proche route non essentielle;
- préserver les éléments premium visibles en dernier.
```

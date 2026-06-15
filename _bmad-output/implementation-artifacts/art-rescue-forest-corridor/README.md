# MyBike / Échappée 3D — Art Rescue Linear + Documentation Pack

Pack généré pour transformer le checkpoint MYB-137 en travail actionnable dans Linear, Codex, Unity MCP et Blender MCP.

## Contenu

- `tickets/` — tickets Markdown prêts à copier dans Linear.
- `docs/` — documentation de production : direction artistique, workflows, validateurs, budgets, garde-fous.
- `prompts/` — prompts prêts à coller dans Codex / Unity MCP / Blender MCP.
- `context/` — copie des rapports et captures fournis dans le brief MYB-137.

## Résumé très franc

MYB-137 améliore l'enveloppement du corridor, mais l'image reste pauvre parce que la scène manque de langage artistique complet. Elle a des objets verticaux, oui, mais pas encore une forêt.

Les faiblesses dominantes :

1. les troncs proches lisent comme des poteaux cylindriques ;
2. le sol est une masse brune/grise sans matière crédible ;
3. l'ambiance lumineuse est plate, uniforme et grisâtre ;
4. il manque des masses de canopée, des silhouettes fortes, des arches, des rochers, des souches, des racines mémorables ;
5. la scène a de la quantité, mais pas encore de hiérarchie visuelle.

Le plan recommandé : d'abord verrouiller l'art bible et les validateurs, puis produire un kit forestier procédural propre, puis densifier avec contrôle, puis seulement réintroduire Meshy/Tripo en quarantaine pour des props isolés.

## Ordre recommandé des tickets

1. `MYB-141` — verrouiller l'art bible et la définition du beau.
2. `MYB-142` — audit visuel + grille de validation par capture.
3. `MYB-143` — manifest tiers / IA.
4. `MYB-144` — validator Unity des assets.
5. `MYB-145` — workflow de captures et régression visuelle.
6. `MYB-146` — playbook Codex + MCP.
7. `MYB-147` — kit procédural Blender forêt v0.
8. `MYB-148` — scatter route-first contrôlé.
9. `MYB-149` — matière sol forestier.
10. `MYB-150` — silhouettes fortes.
11. `MYB-151` — lumière, fog, ambiance.
12. `MYB-152` — POC Meshy/Tripo en quarantaine.
13. `MYB-153` — budgets perf / LOD.

## Utilisation dans Linear

Chaque fichier dans `tickets/` contient la fiche de référence locale de l'issue
Linear correspondante : titre, priorité, labels, dépendances, contexte, tâches,
critères d'acceptation, validation et prompt Codex. Les descriptions Linear ont
été créées à partir de ces fichiers.

## Règle d'or

Meshy/Tripo ne doivent pas fabriquer le décor principal. Ils doivent produire quelques props isolés, validés, manifestés et promus après quarantaine. La forêt doit venir d'un kit contrôlé + scatter route-first + lumière/fog + composition.

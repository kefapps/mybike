# Executive Summary — Sauvetage artistique du corridor forêt

## Diagnostic

MYB-137 avance dans la bonne direction structurelle : la route est plus enveloppée, les bords ont plus de verticalité, et le corridor n’est plus seulement une bande basse au sol.

Mais visuellement, l’image reste très prototype parce que l’ajout de volume ne suffit pas à créer une forêt crédible ou premium.

Les défauts principaux :

- **Troncs-poteaux** : les verticales proches lisent comme des cylindres plantés, pas comme des arbres.
- **Sol pauvre** : la route et les bas-côtés manquent de transitions, matière, feuilles, mousse, racines fines.
- **Fog plat** : l’ambiance est grise, uniforme, sans plans de profondeur clairs.
- **Pas de canopée** : la scène a des troncs, mais très peu de masses supérieures.
- **Pas de setpieces** : rien ne marque la mémoire visuelle du segment.
- **Pas de hiérarchie** : les éléments ont presque tous le même niveau d’importance.

## Direction recommandée

Ne pas essayer de “corriger” ça en lançant plus de générations Meshy/Tripo. La route, le terrain, les bords et la forêt principale doivent venir d’un système contrôlé : kit procédural + scatter route-first + validateurs + captures canoniques.

Meshy/Tripo ne doivent revenir que pour des props isolés en quarantaine.

## Plan court

1. **Art bible** : définir la cible visuelle et les interdits.
2. **Validateurs** : empêcher les imports sales.
3. **Captures** : standardiser route/overview/before-after.
4. **Kit Blender** : remplacer les poteaux par des formes propres.
5. **Scatter route-first** : densifier avec bandes contrôlées.
6. **Sol** : donner une matière forestière lisible.
7. **Lumière/fog** : créer profondeur et mood.
8. **Silhouettes** : ajouter 3-5 moments mémorables.
9. **IA 3D** : tester seulement un prop isolé.

## Anti-pattern principal

> “On va générer une forêt complète avec Meshy/Tripo et l’importer dans Unity.”

Non. Mauvaise pente. Très glissante. Ça finit en soupe de géométrie, matériaux incohérents, colliders absurdes et style qui change tous les trois mètres.

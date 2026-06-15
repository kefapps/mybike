# Prompt — Codex Art Audit

```txt
Tu travailles dans `kefapps/mybike`, projet Unity canonique `unity/Echapee4D`.

Objectif: auditer la qualité visuelle et technique actuelle sans ajouter d’assets.

Contraintes:
- Ne pas appeler Meshy, Tripo ou tout service externe.
- Ne pas modifier `src/**`.
- Ne pas créer de nouveau projet Unity.
- Préserver le mode mock.
- Inspecter la scène canonique et les prefabs/assets visibles.
- Traiter MYB-137 comme volume proof, pas comme target final.

Produire:
- `docs/art-direction/myb-current-art-audit.md`
- `_bmad-output/art-audit/myb-current-art-audit-report.md`

Le rapport doit contenir:
1. Ce qui nuit le plus à la qualité visuelle.
2. Les problèmes d’échelle, matériaux, éclairage, caméra, composition.
3. Les assets à garder.
4. Les assets à supprimer/remplacer.
5. Un plan de sauvetage en 5 tickets maximum.
6. Les validations Unity à ajouter.
```

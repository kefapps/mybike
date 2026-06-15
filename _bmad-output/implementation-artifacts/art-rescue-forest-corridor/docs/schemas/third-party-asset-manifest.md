# Third Party Asset Manifest Schema — Markdown Spec

## But

Documenter chaque asset externe ou IA avec assez d’informations pour savoir :

- d’où il vient ;
- si on a le droit de l’utiliser ;
- où il est dans Unity ;
- qui l’a validé ;
- s’il est accepté ou rejeté.

## Champs obligatoires

| Champ | Type | Obligatoire | Description |
|---|---|---:|---|
| assetId | string | oui | ID stable |
| displayName | string | oui | Nom humain |
| sourceType | enum | oui | `ai_generated`, `third_party`, `internal_procedural`, `manual` |
| provider | string | si IA/tiers | Meshy, Tripo, Kenney, Poly Haven, Blender MCP, etc. |
| sourceUrlOrId | string | si IA/tiers | URL, job id, package id |
| license | string | si IA/tiers | Licence ou lien |
| allowedUsage | array | oui | prototype, vertical_slice, production, internal_only |
| status | enum | oui | quarantine, review, approved, rejected, deprecated |
| rawPath | string | si brut | Chemin brut |
| reviewPath | string | si review | Chemin review |
| productionPath | string|null | si approved | Chemin prod |
| createdAt | date | oui | Date génération/source |
| importedAt | date | oui | Date import |
| reviewer | string|null | si approved | Validateur humain |
| promptSummary | string|null | si IA | Résumé prompt, pas forcément prompt complet |
| notes | string | non | Notes |

## Exemple

```json
{
  "assetId": "myb_forest_stump_ai_a",
  "displayName": "Stylized Forest Stump AI A",
  "sourceType": "ai_generated",
  "provider": "tripo",
  "sourceUrlOrId": "tripo-job-xxxxx",
  "license": "Provider license captured 2026-06-15",
  "allowedUsage": ["prototype", "vertical_slice"],
  "status": "review",
  "rawPath": "Assets/Echappee/Art/Quarantine/AI/tripo/myb_forest_stump_ai_a/",
  "reviewPath": "Assets/Echappee/Art/Review/AI/myb_forest_stump_ai_a/",
  "productionPath": null,
  "createdAt": "2026-06-15",
  "importedAt": "2026-06-15",
  "reviewer": null,
  "promptSummary": "Stylisé Premium de Production mossy stump, simple silhouette, optimized geometry, no photorealism",
  "notes": "Needs material reduction and pivot cleanup"
}
```

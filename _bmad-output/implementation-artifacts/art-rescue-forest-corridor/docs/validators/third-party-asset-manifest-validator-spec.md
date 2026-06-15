# Third-party / AI Asset Manifest Validator Spec

## Objectif

Bloquer les assets tiers ou IA sans traçabilité.

## Manifest minimal

```json
{
  "assetId": "myb_forest_signpost_ai_a",
  "displayName": "Forest Signpost AI A",
  "provider": "meshy",
  "sourceType": "ai_generated",
  "sourceUrlOrId": "provider-job-id-or-url",
  "createdAt": "2026-06-15",
  "importedAt": "2026-06-15",
  "promptSummary": "Stylisé Premium de Production forest signpost, simple silhouette, optimized geometry, no text",
  "license": "provider-license-name-or-url",
  "allowedUsage": ["prototype", "vertical_slice"],
  "rawPath": "Assets/Echappee/Art/Quarantine/AI/meshy/myb_forest_signpost_ai_a/",
  "reviewPath": "Assets/Echappee/Art/Review/AI/myb_forest_signpost_ai_a/",
  "productionPath": null,
  "status": "review",
  "reviewer": "Julien",
  "notes": "Requires Blender cleanup before promotion"
}
```

## Statuts

- `quarantine` — brut, interdit en scène candidate.
- `review` — nettoyé, testable en preview.
- `approved` — utilisable en production.
- `rejected` — ne pas utiliser.
- `deprecated` — ancien asset à remplacer.

## Errors

- `sourceType = ai_generated` sans provider.
- License manquante.
- Status `approved` sans reviewer.
- Asset en Production avec status autre que approved.
- Chemin manifest inexistant.

## Warnings

- Prompt summary vide.
- Notes vides.
- Usage trop large.
- Pas de capture preview liée.

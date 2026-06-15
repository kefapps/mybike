# MYB-143 — Manifest tiers/IA et politique d’asset intake

## Linear metadata

- **Priority**: P0
- **Labels**: asset-pipeline, legal, ai-assets, foundation
- **Estimate**: 3 pts
- **Depends on**: MYB-141
- **Parent / Project suggéré**: `Art Rescue — Forest Corridor Vertical Slice`

## Contexte

Les assets Meshy/Tripo peuvent aider pour des props, mais ils ne doivent pas entrer dans le projet comme des météorites mystérieuses. Chaque asset doit avoir source, licence, date, usage, statut, chemin et validation.

## Objectif

Créer le format de manifest et le workflow d’entrée des assets tiers/IA afin de bloquer les imports sauvages.

## Tâches


- Créer `docs/workflows/meshy-tripo-quarantine-workflow.md`.
- Créer `docs/schemas/third-party-asset-manifest.md` avec un exemple JSON.
- Définir les statuts : `quarantine`, `review`, `approved`, `rejected`, `deprecated`.
- Définir les champs obligatoires pour Meshy/Tripo : provider, prompt résumé, source URL/id, license, creation date, local path, reviewer, allowed usage.
- Définir la règle : aucun asset IA direct dans `Assets/Echappee/Art/Production`.


## Critères d'acceptation


- Le manifest décrit au moins 10 champs obligatoires.
- Le workflow contient une étape Blender cleanup et une étape Unity validation.
- Les assets sans manifest sont considérés bloquants par le validator du ticket MYB-144.


## Validation attendue


- Créer un manifest d’exemple avec un asset IA fictif.
- Vérifier que les docs expliquent comment promouvoir ou rejeter l’asset.


## Hors scope


- Pas de refonte gameplay.
- Pas de nouveau projet Unity.
- Pas de modification de `src/**` sauf besoin explicitement justifié.
- Pas d'appel à Meshy/Tripo sauf ticket dédié.
- Pas de clôture automatique du ticket Linear sans validation visuelle utilisateur.


## Prompt Codex prêt à coller

```txt
Crée la documentation de manifest et d’intake pour assets tiers/IA.

Contraintes:
- Ne pas importer d’asset réel.
- Ne pas utiliser Meshy/Tripo dans ce ticket.
- Ne pas toucher aux scènes.

Livrables:
- `docs/workflows/meshy-tripo-quarantine-workflow.md`
- `docs/schemas/third-party-asset-manifest.md`

Le workflow doit imposer:
1. dossier Quarantine;
2. nettoyage Blender;
3. import Unity contrôlé;
4. validator;
5. promotion explicite;
6. rejet documenté.
```

# MYB-146 — Playbook Codex + Unity MCP + Blender MCP

## Linear metadata

- **Priority**: P0
- **Labels**: workflow, codex, unity-mcp, blender-mcp
- **Estimate**: 3 pts
- **Depends on**: MYB-141
- **Parent / Project suggéré**: `Art Rescue — Forest Corridor Vertical Slice`

## Contexte

Le problème n’est pas seulement les assets : c’est l’orchestration. Codex doit agir comme chef de chantier, Unity comme source de vérité runtime, Blender comme atelier contrôlé, Meshy/Tripo comme générateurs en quarantaine.

## Objectif

Documenter le workflow opératoire pour éviter que les outils ne se marchent dessus.

## Tâches


- Créer `docs/workflows/codex-unity-blender-mcp-playbook.md`.
- Définir responsabilités par outil.
- Définir ordre standard d’un ticket art.
- Ajouter prompts types.
- Ajouter garde-fous : pas de nouveaux projets, pas d’import sauvage, rapport obligatoire, captures obligatoires.
- Ajouter checklist de fin de ticket.


## Critères d'acceptation


- Le playbook permet de lancer un ticket art sans réinventer la procédure.
- Il contient les commandes/étapes attendues pour Unity batchmode si disponibles.
- Il explique quand utiliser Blender MCP vs Meshy/Tripo.


## Validation attendue


- Relecture humaine.
- Le playbook référence les docs validators et art bible.


## Hors scope


- Pas de refonte gameplay.
- Pas de nouveau projet Unity.
- Pas de modification de `src/**` sauf besoin explicitement justifié.
- Pas d'appel à Meshy/Tripo sauf ticket dédié.
- Pas de clôture automatique du ticket Linear sans validation visuelle utilisateur.


## Prompt Codex prêt à coller

```txt
Crée un playbook pour Codex + Unity MCP + Blender MCP.

Livrable:
- `docs/workflows/codex-unity-blender-mcp-playbook.md`

Inclure:
- rôles des outils;
- ordre d’exécution d’un ticket art;
- garde-fous;
- prompts types;
- checklist de validation;
- quand refuser Meshy/Tripo;
- où écrire les rapports et captures.
```

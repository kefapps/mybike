# Codex + Unity MCP + Blender MCP Playbook

## Rôles

| Outil | Rôle correct | Rôle à éviter |
|---|---|---|
| Codex | chef de chantier, tickets, scripts, validators, rapports | directeur artistique autonome qui improvise |
| Unity MCP | source de vérité runtime, scènes, captures, validators | dépotoir d’assets non contrôlés |
| Blender MCP | génération procédurale propre, cleanup, pivots, exports | réparation infinie de soupe IA |
| Meshy/Tripo | props isolés en quarantaine | route, terrain, forêt complète, gameplay core |

## Ordre standard d’un ticket art

1. Lire l’art bible.
2. Lire le ticket Linear.
3. Vérifier les garde-fous : pas de nouveau projet, pas d’import sauvage.
4. Créer ou modifier une scène preview dédiée.
5. Générer/mettre à jour assets seulement si le ticket le demande.
6. Lancer validators.
7. Capturer route + overview.
8. Écrire rapport dans `_bmad-output/implementation-artifacts/<ticket>/`.
9. Résumer les limites et ce qui reste non validé.
10. Attendre validation humaine avant clôture.

## Règles dures

- Le projet Unity canonique est `unity/Echapee4D`.
- Le mode mock doit rester fonctionnel.
- Les assets IA vont d’abord en `Quarantine`.
- Chaque ticket art doit produire un rapport.
- Les captures doivent être comparables entre tickets.
- Un asset sans manifest ne rentre pas en production.
- La capture route dans le corridor de ride canonique est la preuve bloquante
  pour `Premium target`.
- Les previews Blender/Meshy/Tripo, turntables, scènes sandbox et captures
  isolées ne sont que des preuves intermédiaires.

## Quand utiliser Blender MCP

Utiliser pour :

- variantes stylisées premium avec budgets géométriques maîtrisés ;
- troncs/racines/rochers/fougères ;
- pivots/échelle ;
- cleanup mesh ;
- exports propres.

Blender MCP produit des candidats. La caméra de ride valide la qualité visuelle
de production.

Éviter pour :

- compenser une absence d’art direction ;
- réparer 40 assets IA bruts ;
- créer des scènes runtime complexes.

## Quand utiliser Meshy/Tripo

Seulement quand le ticket dit explicitement :

- prop isolé ;
- source/usage/licence documentés ;
- passage en quarantaine ;
- cleanup Blender ;
- validation Unity.

## Checklist fin de ticket

- [ ] Le mode mock fonctionne encore.
- [ ] La scène preview est séparée ou le changement est clairement documenté.
- [ ] Les assets nouveaux ont un chemin propre.
- [ ] Les assets tiers/IA ont manifest.
- [ ] Validator exécuté.
- [ ] Captures route/overview générées.
- [ ] Rapport Markdown écrit.
- [ ] Limites connues listées.
- [ ] Pas de clôture automatique sans validation.

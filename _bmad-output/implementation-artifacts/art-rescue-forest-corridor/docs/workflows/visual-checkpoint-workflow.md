# Visual Checkpoint Workflow

## Objectif

Comparer les passes art sans débat flou. Chaque ticket doit produire les mêmes vues et un rapport comparable.

## Captures obligatoires

1. `route.png` — caméra première personne route.
2. `overview.png` — vue top/oblique du placement.
3. `before-after-route.png` — comparaison si baseline disponible.
4. `before-after-overview.png` — comparaison si baseline disponible.

## Chemin

```txt
_bmad-output/implementation-artifacts/<ticket-slug>/
```

Exemple :

```txt
_bmad-output/implementation-artifacts/myb-149-lighting-fog/
```

## Rapport obligatoire

```txt
_bmad-output/implementation-artifacts/<ticket-slug>/<ticket-slug>-report.md
```

Le rapport contient :

- objectif ;
- scènes utilisées ;
- assets générés/modifiés ;
- métriques ;
- captures ;
- verdict visuel ;
- limites ;
- statut de validation.

## Métriques minimales

- renderers ;
- triangles ;
- nombre d’assets par famille ;
- nombre de matériaux ;
- textures hors budget ;
- erreurs validator ;
- lighting/fog preset si applicable.

## Scoring visuel

| Critère | 1 | 3 | 5 |
|---|---|---|---|
| Route readability | route confuse | route lisible | direction claire et élégante |
| Foreground richness | vide/plat | matière correcte | riche sans bruit |
| Midground density | sparse | forêt lisible | dense et hiérarchisée |
| Background depth | mur gris | profondeur correcte | plans très lisibles |
| Silhouette quality | poteaux | formes variées | moments mémorables |
| Lighting mood | plat | ambiance correcte | premium, lisible, profond |
| Material coherence | incohérent | cohérent | harmonisé et stylisé |

## Seuils

- Prototype : moyenne < 3.
- Acceptable vertical slice : moyenne >= 3 et aucun critère critique à 1.
- Premium target : moyenne >= 4 avec silhouette, lighting et material coherence >= 4.

## Règle de fermeture visuelle

Pour un ticket Art Rescue qui produit ou modifie du rendu visible, `Acceptable
vertical slice` peut servir de checkpoint intermédiaire mais ne suffit pas pour
fermer en `Done`.

`Done` est autorisé uniquement si :

1. `Premium target` est atteint sur la capture route ;
2. ou Julien accepte explicitement une exception documentée.

Si le résultat progresse mais reste sous `Premium target`, le verdict est
`Checkpoint insuffisant`.

## Surface canonique de validation

La validation finale d’un ticket Art Rescue visible doit être faite dans le
contexte de ride canonique, depuis la caméra route canonique.

Preuves finales requises :

- capture route depuis la caméra de ride canonique ;
- capture overview du même état de scène ;
- score rubric ;
- validation humaine quand le jugement est subjectif.

Les scènes preview, rendus Blender, previews Meshy/Tripo, scènes de quarantaine,
turntables et captures isolées sont autorisés comme preuves intermédiaires
seulement. Elles ne valident pas `Premium target` et ne peuvent pas fermer un
ticket visuel de production.

La capture route est bloquante. La capture overview est obligatoire mais
secondaire.

## Visual Surface Exception

```md
## Visual Surface Exception

Status: Accepted / Rejected
Accepted by: Julien
Date:

Reason:
-

Why canonical ride validation was not used:
-

Alternative validation surface:
-

Risk accepted:
-

Required follow-up:
-

Can this ticket close as Done?
-
```

## Visual Checkpoint Verdict

À ajouter dans les rapports et tickets Linear concernés :

```md
## Visual Checkpoint Verdict

Status: Checkpoint insufficient

Premium target reached: No

Screenshots:
- Route:
- Overview:

Rubric:
- Global average:
- Silhouette quality:
- Lighting mood:
- Material coherence:

What improved:
-

Why it is not Done:
-

Primary blocker:
-

Decision:
- [ ] Create corrective sub-ticket
- [ ] Request documented exception
- [ ] Rollback / rework

Follow-up:
-
```

## Visual Closure Exception

```md
## Visual Closure Exception

Status: Accepted
Accepted by: Julien
Date:

Reason:
-

Scope:
-

Current rubric:
- Global average:
- Silhouette quality:
- Lighting mood:
- Material coherence:

Why Premium target is not required for this ticket:
-

Risk accepted:
-

Required follow-up:
-
```

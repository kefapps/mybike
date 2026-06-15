# Forest Corridor Shot Rubric

## Objectif

Évaluer chaque capture route/overview avec les mêmes critères, pour éviter le classique “je ne sais pas, ça fait encore un peu prototype” qui est vrai mais pas très actionnable.

## Échelle

- **1** — mauvais / prototype brut.
- **2** — amélioration visible mais faiblesse dominante.
- **3** — acceptable vertical slice.
- **4** — solide, présentable.
- **5** — cible premium stylisée.

## Critères

| Critère | 1 | 3 | 5 |
|---|---|---|---|
| Route readability | route confuse ou noyée | route lisible à 20–40 m | direction claire, confortable, belle |
| Foreground richness | vide, plat, ruban | matière correcte | riche sans bruit visuel |
| Midground density | sparse / faux décor | forêt lisible | forêt dense, hiérarchisée |
| Background depth | mur gris | plans lisibles | profondeur élégante, fog maîtrisé |
| Silhouette quality | poteaux/cylindres | formes variées | setpieces mémorables |
| Lighting mood | plat/gris | ambiance correcte | premium, contrastée, douce |
| Material coherence | assets qui jurent | cohérent | harmonisé et stylisé |
| Scale credibility | tailles absurdes | globalement plausible | naturel et maîtrisé |
| Composition rhythm | répétitif | alternance correcte | respiration + surprises visuelles |

## Seuils

### Prototype

- Moyenne < 3.
- Ou un critère critique à 1 : route readability, silhouette quality, lighting mood.

### Acceptable vertical slice

- Moyenne >= 3.
- Aucun critère critique à 1.
- Route readability >= 3.

### Premium target

- Moyenne >= 4.
- Silhouette quality >= 4.
- Lighting mood >= 4.
- Material coherence >= 4.
- Validation humaine requise pour clôture `Done`.

## Règle de fermeture

Pour un ticket Art Rescue qui produit ou modifie du rendu visible, `Acceptable
vertical slice` peut servir de checkpoint intermédiaire mais ne suffit pas pour
fermer en `Done`.

`Done` est autorisé uniquement si :

1. `Premium target` est atteint sur la capture route ;
2. ou Julien accepte explicitement une exception documentée.

Toute progression visuelle réelle sous `Premium target` devient un `Checkpoint
insuffisant`.

## Checkpoint insuffisant

Un `Checkpoint insuffisant` doit produire au minimum :

- un score rubric ;
- une capture route ;
- une capture overview ;
- un verdict court ;
- une cause principale d’échec ;
- une action suivante : sous-ticket correctif, exception documentée, ou
  rollback/rework.

## Utilisation

À remplir dans chaque rapport de checkpoint art.

## Surface de validation

La preuve bloquante pour `Premium target` est la capture route prise dans le
contexte de ride canonique, depuis la caméra de ride canonique.

La capture overview est obligatoire pour vérifier la cohérence globale et la
densité, mais elle reste secondaire. Overview explains; route decides.

Les scènes preview, rendus Blender, previews Meshy/Tripo, scènes de quarantaine,
turntables et captures isolées sont des preuves intermédiaires. Elles ne
valident pas `Premium target` et ne peuvent pas fermer un ticket visuel de
production.

```md
| Criterion | Score | Notes |
|---|---:|---|
| Route readability | | |
| Foreground richness | | |
| Midground density | | |
| Background depth | | |
| Silhouette quality | | |
| Lighting mood | | |
| Material coherence | | |
| Scale credibility | | |
| Composition rhythm | | |
```

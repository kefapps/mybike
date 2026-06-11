# MYB-92 - Supprimer le projet Unity legacy unity/Echappee3D

Date: 2026-06-11
Statut local: done
Linear: https://linear.app/kefjbo/issue/MYB-92/myb-009-supprimer-le-projet-unity-legacy-unityechappee3d

## Objectif

Supprimer le projet Unity legacy `unity/Echappee3D` apres la validation de la
baseline canonique `unity/Echapee4D`.

## Preconditions

- `MYB-91` est `Done` dans Linear.
- La baseline `unity/Echapee4D` est validee par
  `_bmad-output/unity-test-results/myb-91-canonical-baseline.txt`.
- Le rapport MYB-91 indique `Previous MYB-90 WebGL build status: Succeeded`.

## Implementation

- Suppression du dossier `unity/Echappee3D/`.
- Suppression des regles `.gitignore` devenues obsoletes pour les dossiers
  generes de `unity/Echappee3D`.
- Mise a jour de `AGENTS.md` pour indiquer que `unity/Echappee3D` a ete retire
  et ne doit pas etre recree.
- Mise a jour de `unity/Echapee4D/README.md` pour retirer l'ancien projet des
  surfaces de travail actives.

## References restantes

Les references restantes a `unity/Echappee3D` dans `_bmad-output/**` sont
historiques: elles documentent les tickets MYB-11 a MYB-17, les audits et les
decisions de pivot. Elles ne sont pas des consignes actives de developpement.

## Validation cible

- `test ! -e unity/Echappee3D`
- `git status --short -- unity/Echappee3D` montre uniquement les suppressions
  des fichiers versionnes legacy.
- `unity-mcp-cli status unity/Echapee4D --timeout 10000`
- `MYB91CanonicalBaselineValidator.ValidateCanonicalBaseline()`
- `git diff --check`
- YAML sprint status parse.
- Scan de references actives: aucune reference obsolete dans `.gitignore` ou
  `scripts/**`; les references restantes dans `AGENTS.md` et
  `unity/Echapee4D/README.md` indiquent explicitement que le projet a ete
  supprime par `MYB-92`.

## Validation executee

- `test ! -e unity/Echappee3D`: pass.
- `git status --short -- unity/Echappee3D | wc -l`: `124` suppressions
  versionnees.
- `unity-mcp-cli status unity/Echapee4D --timeout 10000`: pass, Unity running,
  config server connected, MCP server reachable.
- `MYB91.Editor.MYB91CanonicalBaselineValidator.ValidateCanonicalBaseline()`:
  pass.
- `_bmad-output/unity-test-results/myb-91-canonical-baseline.txt`: `Status:
  PASS`, `Previous MYB-90 WebGL build status: Succeeded`.
- `git diff --check`: pass.
- References actives: aucune occurrence dans `.gitignore` ou `scripts/**`; les
  seules occurrences restantes dans `AGENTS.md` et `unity/Echapee4D/README.md`
  sont des consignes explicites de suppression/historique.

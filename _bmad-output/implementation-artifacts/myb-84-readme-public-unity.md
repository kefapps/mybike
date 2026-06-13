# MYB-84 - README public oriente produit + dev Unity

## Objectif

Creer le README racine public du depot pour presenter MyBike - Échappée 3D et
orienter les contributeurs vers le projet Unity canonique.

## Decisions retenues

- README racine en francais.
- Titre public : `MyBike - Échappée 3D`.
- Format hybride : entree produit courte, puis guide dev compact.
- Pas de section d'etat courant ni de liste de features volatile.
- FTMS/ESP32 mentionne uniquement comme direction future non requise.
- `unity/Echapee4D` presente comme projet Unity canonique macOS-first.
- WebGL presente comme preuve secondaire uniquement quand un ticket le demande.
- `src/**` marque comme prototype historique.
- `unity/Echappee3D/**` marque comme supprime par `MYB-92`.

## Changements

- Ajout de `README.md` a la racine du depot.
- Ajout d'une section `Sources de vérité` pointant vers `AGENTS.md`, le README
  Unity actif, `_bmad-output/linear-sync.md` et les artefacts BMAD actifs.

## Validation

- `git diff --check`: PASS
- Relecture locale du README et du perimetre MYB-84: PASS

## Hors scope

- Pas de refonte du prototype web `src/**`.
- Pas de documentation marketing longue.
- Pas de modification Unity, WebGL, FTMS ou ESP32.

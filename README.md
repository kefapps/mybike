# MyBike - Échappée 3D

MyBike est une balade indoor premium en première personne. Le projet actif,
`Échappée 3D`, vise une expérience Unity macOS-first jouable en mode mock, sans
matériel vélo connecté requis.

Le but du dépôt est simple : faire avancer une vertical slice scenic cycling qui
reste testable localement, démontrable rapidement, et claire dans son périmètre.

## Projet actif

Le projet Unity canonique est :

```bash
unity/Echapee4D
```

Ouvrir ce dossier avec Unity `6000.4.10f1`. La scène de référence actuelle et
les détails techniques Unity vivent dans le README du projet Unity, pas dans ce
README racine.

Le mode mock doit toujours rester fonctionnel. Les travaux FTMS, ESP32,
CoreBluetooth ou contrôle de résistance réelle sont une direction produit, mais
ils ne sont pas requis pour lancer ou valider la vertical slice actuelle.

## Lancement local

1. Ouvrir `unity/Echapee4D` dans Unity Hub avec Unity `6000.4.10f1`.
2. Laisser Unity importer/compiler le projet.
3. Lancer la scène canonique depuis l'Editor.

Validation Unity-MCP rapide depuis la racine du dépôt :

```bash
unity-mcp-cli status unity/Echapee4D --timeout 10000
```

Validation locale complète quand un ticket Unity le demande :

```bash
npm run validate:local-ci
```

## Périmètre du dépôt

- `unity/Echapee4D` est la cible active pour le runtime, la scène, le ride loop,
  le HUD, les visuels et la livraison macOS.
- WebGL est une preuve secondaire, à lancer seulement quand un ticket le demande
  explicitement.
- `src/**` est l'ancien prototype React/Vite/Three.js. Il sert de référence
  historique et ne doit pas être étendu sans ticket explicite.
- `unity/Echappee3D/**` a été supprimé par `MYB-92`. Les références restantes
  dans de vieux artefacts BMAD sont historiques.

## Sources de vérité

- Règles projet et workflow agent : `AGENTS.md`
- Détails techniques Unity : `unity/Echapee4D/README.md`
- Sync Linear et historique des tickets : `_bmad-output/linear-sync.md`
- Planning BMAD actif :
  - `_bmad-output/planning-artifacts/echappee-3d-gdd-court.md`
  - `_bmad-output/planning-artifacts/echappee-3d-architecture-mince.md`
  - `_bmad-output/planning-artifacts/echappee-3d-mvp-epic-stories.md`
  - `_bmad-output/planning-artifacts/sprint-change-proposal-unity-canonical-2026-06-11.md`

Pour les décisions de scope, les tickets Linear `MYB-*` et
`_bmad-output/linear-sync.md` font foi.

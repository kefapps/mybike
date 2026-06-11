# MYB-94 - Decision plateforme cible prioritaire Unity macOS

Date: 2026-06-11
Ticket Linear: https://linear.app/kefjbo/issue/MYB-94/cadrer-plateforme-cible-prioritaire-unity-macos-first-webgl-secondaire
Statut: implementation/cadrage
Commentaires Linear: `0f59b35f-edf5-410c-8beb-1bd29438c516`,
`c8e1b231-13a4-475b-a2c6-22556999eda1`

## Decision

La cible prioritaire d'Echappee 3D est maintenant:

1. Unity macOS first.
2. Unity WebGL secondaire, pour preuve navigateur, demo privee ou regression
   visuelle quand un ticket le demande.
3. Android candidat secondaire, a recadrer dans un ticket dedie avant d'en
   faire une contrainte d'implementation.

Le projet Unity canonique reste `unity/Echapee4D`. La decision change la
plateforme cible prioritaire, pas le moteur.

## Implications produit

- Le mock mode reste obligatoire: aucun parcours MVP ne doit dependre d'un
  home trainer, d'un capteur ou d'une connexion BLE.
- La promesse connectee cible macOS/CoreBluetooth/FTMS, mais elle reste
  ticket-gated tant qu'un test peripherique reel n'a pas prouve la faisabilite.
- WebGL ne doit plus dicter les tickets assets, performance, terrain, input ou
  CI par defaut.
- Les preuves WebGL existantes de `MYB-89`, `MYB-90` et `MYB-91` restent utiles
  comme evidence technique et fallback de demo, sans etre la cible produit
  principale.

## Preflight local du 2026-06-11

- Machine: MacBook Pro `Mac14,9`, Apple M2 Pro, 16 GB.
- OS: macOS `26.5.1` build `25F80`.
- Bluetooth: On, chipset `BCM_4388`, services supportes incluant `GATT`.
- Unity: `6000.4.10f1`.
- Unity PlaybackEngines observes localement: `WebGLSupport` present,
  `AndroidPlayer` absent, pas de dossier `MacStandaloneSupport` separe observe.

Conclusion preflight: le socle macOS/Bluetooth local est compatible avec une
exploration CoreBluetooth/FTMS, mais la faisabilite FTMS n'est pas prouvee sans
peripherique reel. Android n'est pas validable localement sans installation de
module.

## Tickets impactes

- `MYB-39`: garder comme ADR moteur Unity, mais le libelle "Unity WebGL devient
  la cible active" est supersede par `MYB-94` pour la plateforme prioritaire.
- `MYB-90`: garder comme preuve WebGL readiness, secondaire.
- `MYB-91`: garder comme baseline Unity canonique, avec WebGL reproductible
  seulement comme preuve historique/secondaire.
- `MYB-93`: adapter le skill terrain-3d a Unity macOS-first, sans supposer
  WebGL comme cible principale.
- `MYB-41`: shortlister assets pour Unity/macOS first; renseigner risque WebGL
  seulement si l'asset doit aussi etre montre en navigateur.
- `MYB-51`: remplacer "budgets performance Unity WebGL" par budgets plateforme:
  macOS first, WebGL secondaire.
- `MYB-83`: remplacer "CI Unity validation / WebGL build" par validation Unity
  macOS-first, avec WebGL optionnel si faisable.
- `MYB-40`, `MYB-58`, `MYB-61`: requalifier les spikes hardware autour de
  macOS/CoreBluetooth/FTMS avant Web Bluetooth.

## Synchronisation Linear des tickets ouverts

Passe effectuee le 2026-06-11 apres demande explicite de coherence des tickets
restants.

Projet Linear mis a jour:

- `Echappee 3D - Vertical Slice Mock`: resume/description alignes sur Unity
  macOS-first, WebGL secondaire, Three.js historique.

Tickets ouverts mis a jour:

- `MYB-26`: regressions visuelles Unity macOS + smoke WebGL optionnel.
- `MYB-30`: epic assets aligne Unity macOS-first.
- `MYB-31`: epic difficulte/resistance aligne macOS/CoreBluetooth/FTMS futur.
- `MYB-34`: epic qualite/delivery renomme Unity macOS-first.
- `MYB-40`: spike velo connecte renomme macOS/CoreBluetooth.
- `MYB-41`: shortlist assets: risque plateforme, macOS prioritaire.
- `MYB-42`: import assets: validation macOS locale, WebGL secondaire.
- `MYB-44`: scenic corridor: capture locale macOS/Editor, WebGL explicite.
- `MYB-45`: biomes: budgets plateforme macOS-first.
- `MYB-47`: acteurs animes: cible macOS-first.
- `MYB-48`: indices visuels: capture macOS/Editor, WebGL explicite.
- `MYB-50`: pipeline optimisation assets renomme macOS-first.
- `MYB-51`: budgets performance renomme macOS-first.
- `MYB-53`: POC pack assets: rendu macOS local, WebGL secondaire.
- `MYB-58`: spike FTMS renomme macOS/CoreBluetooth.
- `MYB-61`: pairing velo/capteur aligne Unity macOS-first.
- `MYB-63`: HUD: capture locale macOS/Editor.
- `MYB-73`: fiche route: UI Unity macOS-first, overlay WebGL optionnel.
- `MYB-79`: ecran accueil: compatible macOS-first et mock mode.
- `MYB-80`: cadence HUD/accessibilite renomme macOS-first.
- `MYB-82`: lancement Unity renomme macOS.
- `MYB-83`: CI renomme Unity macOS-first validation.
- `MYB-84`: README public/dev: macOS-first, WebGL preuve secondaire.
- `MYB-87`: presets qualite renomme Unity macOS.
- `MYB-88`: checklist PR: impact plateforme macOS prioritaire.

Ticket verifie sans modification par cette passe:

- `MYB-93`: Done; adapte le skill terrain-3d a Unity/Echapee4D et reste
  coherent avec macOS-first/WebGL secondaire.

## Criteres de validation minimaux futurs

### Pour tout ticket Unity actif

- `unity-mcp-cli status unity/Echapee4D --timeout 10000` passe.
- La scene/validator lie au ticket ne casse pas le mock mode.
- La validation ne touche pas `src/**` sauf ticket explicite.

### Pour tickets macOS/demo-facing

- Documenter version Unity, version macOS et cible testee.
- Produire screenshot, video, contact sheet ou rapport de validator qui prouve
  route/HUD/mouvement quand le ticket touche le rendu.
- Documenter warnings/errors Unity.

### Pour tickets FTMS/macOS

- Tester avec un peripherique FTMS reel.
- Verifier scan, connexion, lecture service/characteristics, permissions macOS
  et comportement de fallback mock.
- Ne pas bloquer le MVP si le peripherique ou les permissions ne sont pas
  disponibles.

### Pour tickets WebGL

- Ne lancer build/capture WebGL que si le ticket le demande.
- Verifier HTTP 200, failed requests 0, page errors 0, console errors 0,
  warnings documentes et screenshot/canvas nonblank.

## Hors scope de MYB-94

- Implementer FTMS/CoreBluetooth.
- Installer le module Android.
- Produire un build macOS final.
- Modifier la scene Unity.
- Importer des assets.
- Supprimer les preuves WebGL existantes.

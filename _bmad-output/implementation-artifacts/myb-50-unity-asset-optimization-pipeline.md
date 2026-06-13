# MYB-50 - Pipeline d'optimisation assets Unity macOS-first

Date: 2026-06-13
Ticket Linear: https://linear.app/kefjbo/issue/MYB-50/myb-018-definir-le-pipeline-doptimisation-assets-unity-macos-first
Statut: implemented

## Objectif

Definir comment preparer, accepter, optimiser et ranger les assets 3D avant
leur adoption dans `unity/Echapee4D`, sans importer de nouvel asset ni modifier
les assets existants dans ce ticket.

Ce pipeline sert de base a `MYB-100`, qui appliquera les ajustements Unity aux
assets deja importes.

## Decisions de cadrage

- MYB-50 est un ticket de pipeline concret uniquement.
- Aucun nouvel asset, prefab, scene ou import Unity n'est modifie ici.
- Les ajustements Unity sur les assets existants sont suivis par `MYB-100`.
- Les decisions utilisent des bandes `ok / warning / reject`.
- `reject` bloque l'import ou l'adoption sauf exception documentee dans le
  ticket, le manifest et le rapport d'implementation.
- Les assets tiers gratuits et les assets custom/generes partagent le meme
  pipeline technique, mais gardent des regles de provenance separees.

## Pipeline Asset Unity

1. Qualifier la provenance.
2. Classer la famille d'asset.
3. Inspecter le poids technique source.
4. Choisir les import settings Unity cibles.
5. Creer ou mettre a jour prefab/materials dans la convention de rangement.
6. Evaluer `ok / warning / reject`.
7. Enregistrer le verdict legal, technique et artistique.
8. Valider avec les garde-fous macOS-first applicables.

## Provenance

### Tiers gratuit

Un asset tiers gratuit doit avoir:

- source URL canonique;
- nom exact de l'asset ou du pack;
- auteur;
- licence et URL de licence;
- date de recuperation;
- fichiers importes listes;
- statut manifest `approved`, `needs-review` ou `rejected`;
- attribution ou justification `none-required`.

Licences acceptees par defaut: `CC0`, `Public Domain`, `CC-BY`.

Licences refusees par defaut: `NC`, `ND`, `SA`, absence de licence, licence
ambigue ou source non canonique.

### Custom / genere

Un asset custom ou genere doit avoir:

- outil ou source de generation;
- ticket d'origine;
- parametres ou prompt utiles quand ils sont conservables;
- cout externe si applicable;
- source originale si l'asset est derive;
- etapes de transformation, par exemple Blender, remesh ou conversion;
- statut technique et artistique separe.

Pour Meshy ou tout outil a credits, le cout doit rester confirme avant chaque
generation dans le ticket qui genere l'asset. MYB-50 ne consomme aucun credit.

## Familles d'assets

| Famille | Role | Exemples |
| --- | --- | --- |
| `surface/materiau` | Surface de route, sol, mur, matiere reusable. | Paving stones, forest ground, atlas stylise. |
| `prop leger` | Decoration secondaire placee plusieurs fois. | Rocher, tonneau, cloture, panneau. |
| `acteur anime` | Element vivant avec rig ou clips d'animation. | Cheval, personnage, oiseau. |
| `signal premium` | Landmark ou hero asset qui porte une scene. | Relique, fontaine, arbre sacre, portail. |
| `biome kit` | Pack ou subset qui structure un biome. | Village kit, nature kit, pack route/campagne. |

## Bandes techniques

Ces seuils sont des garde-fous macOS-first. Ils servent a decider vite, pas a
remplacer le jugement visuel.

### Surface / materiau

| Mesure | ok | warning | reject |
| --- | --- | --- | --- |
| Texture max | 1024 par map | 2048 si surface principale | > 4096 sans exception |
| Maps par materiau | Color + Normal + Roughness max | AO/Height en plus si justifie | Set PBR complet lourd sans usage visible |
| Format source | JPG/PNG propre, maps separees | EXR/TIFF converti avant Unity | Archive brute massive importee telle quelle |
| Style | Stylisable dans Unity | Photorealiste a retraiter | Casse `Stylise Premium` sans plan |

### Prop leger

| Mesure | ok | warning | reject |
| --- | --- | --- | --- |
| Triangles par instance | <= 5k | 5k-20k | > 50k |
| Materiaux | 1-2 | 3-4 | > 4 sans raison |
| Texture max | 1024 | 2048 | > 2048 pour prop secondaire |
| Usage | Instanciable/repetable | A limiter par densite | Trop cher pour placement multiple |

### Acteur anime

| Mesure | ok | warning | reject |
| --- | --- | --- | --- |
| Triangles | <= 30k | 30k-100k | > 120k sans exception |
| Clips | idle/walk simple | plusieurs clips utiles | clips inutiles ou cassants non nettoyes |
| Rig | import Unity stable | conversion Blender requise | rig instable ou non reproductible |
| Texture max | 1024-2048 | 2048 par asset important | > 2048 sans role premium |

### Signal premium

| Mesure | ok | warning | reject |
| --- | --- | --- | --- |
| Triangles | <= 50k | 50k-150k | > 200k sans exception |
| Materiaux | 1-4 | 5-8 | > 8 non justifies |
| Texture max | 2048 | 4096 pour hero unique | > 4096 |
| Role | Landmark clair | Hero couteux mais justifie | Cher sans impact scenic fort |

### Biome kit

| Mesure | ok | warning | reject |
| --- | --- | --- | --- |
| Import | subset cible | pack partiel large | pack complet sans tri |
| Fichiers versionnes | uniquement utiles | quelques alternates documentes | zips/archives/full dumps |
| Cohesion | sert un biome precis | melange a trier | direction visuelle confuse |
| Performance | budget par subset | besoin de MYB-100 | impossible a borner |

## Import settings Unity recommandes

### Textures

- Max Size par defaut: `1024`.
- Max Size autorise pour signal premium: `2048`, voire `4096` avec exception
  documentee.
- Mip Maps: activees pour textures 3D.
- sRGB: actif pour Color/Base Map, inactif pour Normal/Roughness/AO/Mask.
- Texture Type: `Normal map` pour les normales.
- Compression: compressee par defaut; haute qualite seulement pour hero asset
  ou artefacts visibles.
- Les textures source lourdes ne doivent pas etre importees telles quelles si
  une version 1K/2K propre suffit.

### Modeles

- Importer uniquement les formats utiles a Unity, priorite `FBX` ou format deja
  prouve par le ticket.
- Ne pas versionner des exports redondants si un seul format est utilise.
- Conserver l'echelle lisible dans prefab plutot que multiplier les variantes.
- Desactiver ou nettoyer les animations inutiles.
- Generer/valider les colliders seulement quand le ticket en a besoin.
- Ne pas ajouter un asset a Build Settings via une scene POC.

### Materiaux

- Preferer un materiau partage par famille visuelle quand plusieurs props
  utilisent le meme atlas.
- Nommer les materiaux par asset/source ou usage scenic.
- Eviter les variantes de materiaux non utilisees.
- Toute stylisation lourde doit rester lisible comme decision artistique, pas
  comme correction cachee d'un asset inadapte.

### Prefabs

- Un prefab pret a placer doit etre le point d'entree pour les scenes.
- Les modeles source restent dans `Models`, les textures dans `Textures`, les
  materials dans `Materials`, les prefabs dans `Prefabs`.
- Les animations importees ou controllers dedies vont dans `Animations`.
- Les sources Blender ou notes de generation vont dans `Source`.
- Les preuves de licence vont dans `License`.

## Convention de rangement

Garder la structure existante:

- tiers gratuit: `Assets/Echappee/Art/ThirdParty/<Author>/<Pack>`;
- custom / genere / POC: `Assets/Echappee/Art/MYBxx<SourceOrPack>`;
- sous-dossiers standards:
  - `Models`;
  - `Textures`;
  - `Materials`;
  - `Prefabs`;
  - `Animations`;
  - `Source`;
  - `License`.

MYB-50 ne renomme pas les dossiers existants. Les corrections de rangement
eventuelles appartiennent a `MYB-100`.

## Criteres d'acceptation

Un asset peut etre accepte techniquement si:

- sa provenance est complete;
- sa famille est explicite;
- ses bandes techniques sont `ok` ou `warning` justifie;
- son rangement suit la convention ou une exception est notee;
- son impact reste compatible avec les budgets MYB-51;
- son statut artistique est separe de son statut legal/technique.

Un asset doit etre refuse ou bloque si:

- sa licence est absente, ambigue, `NC`, `ND` ou `SA`;
- la source n'est pas canonique;
- il impose un pack complet ou une archive brute dans Unity;
- il depasse une bande `reject` sans exception;
- il casse clairement `Stylise Premium` ou `Premium Aventure`;
- il oblige a faire de WebGL la cible principale.

## Validation pour les tickets d'application

Pour un ticket qui applique ce pipeline a des assets Unity, executer au minimum:

```bash
jq empty unity/Echapee4D/Assets/Echappee/Art/ThirdPartyAssets.assetmanifest.json
git diff --check
unity-mcp-cli status unity/Echapee4D --timeout 10000
npm run validate:local-ci
```

Pour un ticket strictement documentaire comme MYB-50, la validation minimale est:

```bash
jq empty unity/Echapee4D/Assets/Echappee/Art/ThirdPartyAssets.assetmanifest.json
node scripts/validate-myb-50-docs.mjs
git diff --check
```

## Hors scope MYB-50

- Importer ou modifier des assets Unity.
- Retoucher textures, materiaux, prefabs ou scenes.
- Faire du WebGL la cible d'optimisation principale.
- Creer un pipeline automatise lourd.
- Changer `src/**` ou recreer `unity/Echappee3D/**`.

## Validation executee

- `jq empty unity/Echapee4D/Assets/Echappee/Art/ThirdPartyAssets.assetmanifest.json`: PASS
- `node --check scripts/validate-myb-50-docs.mjs`: PASS
- `node scripts/validate-myb-50-docs.mjs`: PASS
- `git diff --check`: PASS

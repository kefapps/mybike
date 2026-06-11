# Crédits des assets tiers — MyBike / Echappee 3D

> Ticket source : **MYB-52** ([MYB-020] Creer un ecran / document d'attribution des assets tiers).
> Politique de licences : **MYB-38** ([MYB-004] Etablir une politique de licences pour assets Unity gratuits).
> Statut : maintenu a jour pour la vertical slice mock Unity.
> Projet actif : `unity/Echapee4D`.
> Manifest Unity actif : [`unity/Echapee4D/Assets/Echappee/Art/ThirdPartyAssets.assetmanifest.json`](./unity/Echapee4D/Assets/Echappee/Art/ThirdPartyAssets.assetmanifest.json).
> Manifest web historique : [`src/manifest/assets-manifest.json`](./src/manifest/assets-manifest.json).

## Etat actuel

La vertical slice mock active d'Echappee 3D n'integre **aucun asset artistique
tiers** au moment de cette passe. Le projet actif est Unity macOS-first
`unity/Echapee4D`; la scene de baseline utilise des primitives/geometries
internes et aucun modele 3D, texture, sample audio ou police personnalisee
externe n'est canonise comme asset de DA.

Le manifest Unity actif est vide et structurel. Il fixe le contrat de
metadonnees pour les prochains tickets d'assets, mais aucun asset reel n'est
encore approuve ou importe par MYB-38.

Les dependances Unity, packages et outils deja presents ne sont pas audites
retroactivement par MYB-38. Cette politique s'applique des qu'un ticket ajoute
volontairement une ressource tierce a la V1 Unity ou la rend visible/runtime
dans `unity/Echapee4D`.

Le manifest `src/manifest/assets-manifest.json` appartient au prototype web
historique React/Vite/Three.js. Il reste une reference utile pour la structure
des credits, mais il ne pilote plus la direction active Unity.

Les references d'inspiration MYB-37 sont textuelles uniquement. Elles ne sont
pas des assets importes et ne doivent pas etre creditees comme contenu utilise.

Cette politique est donc valide par defaut : le tableau ci-dessous est vide
jusqu'a la premiere contribution externe.

## Politique MYB-38 de licences

Cette politique couvre toute ressource tierce gratuite utilisee dans le projet
Unity : modeles 3D, textures, materiaux, shaders, animations, VFX, audio,
polices, icones/UI, scripts/outils importes et packages Unity gratuits.

### Licences acceptees par defaut

| Licence | Usage autorise | Conditions |
|---|---|---|
| `CC0` | Asset artistique ou technique. | Source verifiee et metadonnees completes. |
| `Public Domain` | Asset artistique ou technique. | Source verifiee et preuve suffisante du statut public-domain. |
| `CC-BY` | Asset artistique ou technique. | Source verifiee, attribution exacte, URL de licence et modifications documentees. |

### Licences techniques acceptees selon le type

| Licence | Usage autorise | Conditions |
|---|---|---|
| `MIT` | Code, outil, shader code ou package technique. | Ne vaut pas acceptation automatique d'un asset 3D, texture, audio ou contenu artistique. |
| `Apache-2.0` | Code, outil, shader code ou package technique. | Conserver les notices requises et documenter les modifications. |
| `OFL` | Police uniquement. | Conserver les informations de font et d'attribution attendues par la licence. |

### Refus par defaut

Les cas suivants ne peuvent pas etre importes dans Unity ni apparaitre dans une
build macOS/WebGL sans exception explicite :

- Licences ou clauses `NC`, `ND` ou `SA`, dont `CC-BY-SA`, `CC-BY-NC`,
  `CC-BY-NC-SA`, `CC-BY-ND` et `CC-BY-NC-ND`.
- Licence `Other`, custom, marketplace/EULA ambigu, ou conditions speciales non
  tranchees.
- Asset gratuit sans licence explicite pour l'asset exact.
- Reupload, collection, archive, Drive/Dropbox ou page communautaire qui ne
  renvoie pas a la source originale.
- Asset extrait d'un jeu, film, serie, marque, lieu sous licence ou personnage
  reconnaissable.
- Pack "AI generated" ou assiste par IA sans auteur identifie, licence claire
  et droits d'usage/build suffisamment explicites.
- Asset qui copie trop directement une reference d'inspiration MYB-37.

### Statuts de revue

| Statut | Sens | Regle |
|---|---|---|
| `approved` | Utilisable. | Source verifiee, licence acceptee, attribution et metadonnees completes. |
| `needs-review` | Candidat bloque. | Ne peut pas etre importe dans Unity ni etre inclus dans une build. |
| `rejected` | Refuse. | Conserver la raison si cela evite de reevaluer le meme asset. |

Toute exception doit citer la raison, la personne qui valide, la date de
validation et le ticket Linear lie. Une exception approuvee doit ensuite passer
en `approved` avant import.

### Sources verifiables

Une source est acceptable si elle permet de retrouver l'asset original, son
auteur et ses droits d'usage. Exemples acceptables : Unity Asset Store,
Sketchfab, Kenney, Quaternius, Poly Haven, site auteur, depot GitHub officiel
ou autre page canonique equivalent.

La capture minimale a conserver est :

- URL source canonique.
- Nom exact de l'asset ou du pack.
- Auteur ou equipe originale.
- Licence affichee au moment de la revue.
- URL de licence ou preuve equivalente sur la page source.
- Date de consultation.

### Attribution

- Chaque asset tiers `approved` est credite dans ce document et dans le manifest
  Unity actif.
- Les assets `CC-BY` et toute exception exigeant l'attribution doivent comporter
  nom, auteur, source, licence, modifications et URL du texte integral ou de la
  page source qui porte la licence.
- Les assets `CC0` ou du domaine public peuvent etre credites par courtoisie,
  mais l'attribution doit rester exacte si elle est ajoutee.
- Lorsqu'une surface UI de credits existe pour la cible active, les assets qui
  exigent attribution doivent y apparaitre. Avant cette surface, le repo et le
  manifest sont la source de verite.
- Le HUD de la ride n'affiche ni URLs longues ni panneaux de credits. Les URLs
  completes restent dans l'ecran/overlay Credits et dans ce document.
- Toute evolution de cette politique est tracee en BMAD et synchronisee Linear.

## Comment ajouter un asset

1. Ajouter une entree dans le manifest Unity actif :
   `unity/Echapee4D/Assets/Echappee/Art/ThirdPartyAssets.assetmanifest.json`.

   | Champ | Obligatoire | Description |
   |---|---|---|
   | `id` | oui | Identifiant unique et stable (slug en kebab-case). |
   | `name` | oui | Nom affiche de l'asset. |
   | `author` | oui | Auteur ou equipe originale. |
   | `sourceUrl` | oui | URL canonique de l'asset source. |
   | `license` | oui | Licence declaree : `CC0`, `Public Domain`, `CC-BY`, `MIT`, `Apache-2.0`, `OFL`, ou valeur a placer en `needs-review`. |
   | `licenseUrl` | oui | URL du texte integral de la licence, ou page source si la licence y est integree. |
   | `retrievedAt` | oui | Date ISO de consultation. |
   | `category` | oui | `3d-model`, `texture`, `material`, `shader`, `animation`, `vfx`, `audio`, `font`, `sprite`, `ui`, `code`, `package`, `other`. |
   | `intendedUse` | oui | Usage prevu dans Echappee 3D. |
   | `modifications` | oui | `none` ou description des modifications appliquees. |
   | `attributionText` | oui | Texte de credit attendu, ou `none-required` pour `CC0`/`Public Domain` sans attribution. |
   | `localPath` | oui si importe | Chemin dans `unity/Echapee4D`; vide tant que l'asset reste candidat. |
   | `status` | oui | `approved`, `needs-review` ou `rejected`. |
   | `reviewNotes` | non | Raison de blocage/refus ou note de validation. |
   | `approvedBy` | oui si exception | Personne ayant valide l'exception. |
   | `approvedAt` | oui si exception | Date ISO de validation de l'exception. |
   | `linearIssue` | oui | Ticket Linear qui justifie l'ajout, la revue ou l'exception. |

2. Reporter l'asset dans le tableau de ce document quand son statut devient
   `approved`.

3. Verifier dans la PR que l'attribution est conforme, que les assets
   `needs-review` ne sont pas importes, et que les screenshots/builds ne
   montrent pas d'URL longue dans le HUD.

## Tableau des assets approuves

Aucun asset tiers approuve par cette politique n'est enregistre a ce jour.

| ID | Nom | Auteur | Source | Categorie | Licence | Modifications |
|---|---|---|---|---|---|---|
| _aucun_ | _aucun_ | _aucun_ | _aucun_ | _aucun_ | _aucun_ | _aucun_ |

## Hors scope

- Audit juridique externe.
- Verification automatisee du respect des licences (rappel humain a chaque PR).
- Generation d'un site web dedie aux credits (la note markdown et l'overlay
  in-app suffisent pour la V1 mock).

## Liens utiles

- [OpenGameArt FAQ](https://opengameart.org/content/faq)
- [Poly Haven licence](https://polyhaven.com/license)
- [Kenney support / licence](https://kenney.nl/support)

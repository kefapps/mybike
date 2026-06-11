# MYB-95 - POC Meshy modeles 3D animes custom et integration Unity

Date: 2026-06-11
Ticket Linear: https://linear.app/kefjbo/issue/MYB-95/poc-meshy-modeles-3d-animes-custom-et-effort-dintegration-unity
Statut: backlog

## Objectif

Evaluer si Meshy peut produire des modeles 3D custom animes reellement utiles
pour `unity/Echapee4D`, et mesurer l'effort complet pour les integrer proprement
dans Unity macOS-first.

Le but n'est pas d'ouvrir un pipeline asset lourd. Le but est de savoir, preuves
a l'appui, si Meshy vaut le coup pour quelques assets premium cibles.

## Contexte

Le projet dispose de credits Meshy utilisables.

Avant chaque generation, confirmer explicitement:

- le cout estime en credits;
- l'objectif du test;
- le nombre maximal d'iterations autorisees.

Les credits ne doivent pas etre consommes en boucle ouverte.

MYB-41 couvre la shortlist d'assets gratuits. MYB-95 repond a une question
distincte: construire nos propres modeles 3D animes quand un asset gratuit ne
suffit pas.

## Cas a tester

### Cas simple

Creer un petit asset de bord de route ou de clairiere, anime principalement cote
Unity.

Exemple recommande: pierre runique ou petite lanterne fantasy stylisee avec
emissive, rotation lente, pulse de lumiere ou particules tres simples.

A mesurer: generation Meshy, export, import Unity, materiau, pivot, echelle,
collider simple, animation Unity, prefab.

### Cas moyen

Creer un modele vivant ou semi-vivant avec animation courte.

Exemple recommande: petit animal de campagne stylise ou oiseau fantasy discret
avec idle, walk ou wing flap court.

A mesurer: rig/animation disponible ou non, qualite des boucles, cleanup
eventuel Blender, import animation Unity, Animator ou AnimationClip, placement
en scene.

### Cas super chiade

Creer un vrai signal fantasy premium.

Exemple recommande: arbre sacre, portail-relique ou fontaine magique stylisee
high detail, avec animation visible via shader, emissive, feuilles, runes,
lumiere ou elements mobiles.

A mesurer: cout credits, temps de prompt/iteration, poids mesh/textures,
cleanup, LOD ou simplification necessaire, materiaux Unity, animation/shader,
impact scene et risque performance.

## Mesures obligatoires

Pour chaque cas, documenter:

- prompt exact et reglages utilises;
- cout credits annonce et cout reellement consomme;
- temps de generation et nombre d'iterations;
- formats exportes;
- taille fichiers, textures, polycount si disponible;
- qualite visuelle percue par rapport a MYB-37;
- effort d'integration Unity en etapes et temps reel;
- erreurs/import warnings Unity;
- travail manuel necessaire: Blender, materials, rig, animation, pivot, scale,
  colliders;
- verdict: utilisable direct, utilisable apres cleanup, trop cher, trop lent ou
  trop instable.

## Livrables

- Rapport local sous `_bmad-output/implementation-artifacts/`.
- Trois assets testes ou trois resultats documentes si une generation echoue.
- Une scene ou zone sandbox Unity dediee, sans polluer la scene principale.
- Prefabs Unity uniquement si l'import est suffisamment propre.
- Captures visuelles avant/apres import Unity.
- Recommandation finale: Meshy a utiliser pour quels types d'assets, avec quel
  budget et quelle limite.

## Hors scope

- Importer une librairie complete d'assets Meshy.
- Remplacer MYB-41 ou les packs gratuits retenus.
- Consommer des credits sans confirmation prealable.
- Mettre un asset Meshy dans la scene principale sans validation de provenance,
  licence et manifest.
- Construire un pipeline automatise Meshy complet.

## Criteres d'acceptation

- Les trois niveaux simple, moyen et super chiade sont testes ou explicitement
  bloques avec raison.
- Le cout credits est trace pour chaque tentative.
- L'effort d'integration Unity est mesure, pas seulement estime.
- `unity/Echapee4D` reste valide apres le POC.
- La conclusion permet de decider si Meshy devient un outil ponctuel recommande
  pour certains assets premium.

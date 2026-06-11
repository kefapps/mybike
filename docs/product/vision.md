# Vision produit - MyBike

## Promesse

MyBike transforme chaque session de velo en petite expedition heroique et
remplace l'ennui du home trainer par des missions courtes, lisibles et
excitantes.

Le produit n'essaie pas d'etre un RPG complet qu'on joue assis. Il transforme
l'effort physique reel en moteur d'aventure: le joueur choisit une mission,
pedale pour progresser, traverse un monde magique, surmonte des epreuves
rythmiques et repart avec le sentiment d'avoir accompli une aventure complete.

## Public cible

MyBike s'adresse d'abord aux personnes qui ont du mal a tenir 20 minutes sur
home trainer sans stimulation forte. Elles veulent bouger, mais l'effort pur
leur parait vite monotone. Le produit doit leur donner une raison immediate de
continuer: voir la suite, franchir une epreuve, recuperer une relique, debloquer
une nouvelle route.

Le succes principal est simple: la personne tient 20 minutes sans regarder
l'horloge. Le succes secondaire est plus ambitieux: elle accepte un effort plus
intense parce qu'elle veut voir la suite de la mission.

## Personas

### 1. L'echappe du quotidien

Il possede ou utilise un velo d'appartement, mais il s'ennuie vite. Sa
motivation n'est pas de battre un record: il veut tenir 15 a 20 minutes sans
subir chaque minute. MyBike doit lui donner l'impression de partir quelque part
des les premieres secondes.

### 2. Le regulier en manque de relief

Il accepte l'effort, mais il trouve les seances indoor repetitives. Il est pret
a pousser plus fort si le jeu donne un but clair: franchir une poursuite,
remplir une jauge, sauver une route, atteindre une relique. MyBike doit cacher
la structure d'entrainement derriere un rythme d'aventure lisible.

### 3. Le completiste casual

Il aime les deblocages, les tresors, les chemins optionnels et les objectifs
courts. Il relance parce qu'il veut recuperer une relique, voir une nouvelle
region ou tenter une mission plus risquee. MyBike doit lui donner une
progression legere, mais jamais un RPG qui prend plus de place que la ride.

## Boucle de session

La boucle coeur d'une session tient en cinq temps:

1. Choisir une mission courte dans la quete principale.
2. Choisir une difficulte adaptee au temps et a l'energie du moment.
3. Partir dans un decor premium qui donne d'abord une sensation d'evasion.
4. Traverser une montee de tension faite de relances, poursuites, jauges et
   recuperations.
5. Terminer par une epreuve forte, une recompense claire et un sentiment
   d'aventure accomplie.

Le chemin de lancement par defaut doit rester court: choisir la mission, choisir
la difficulte, partir.

## Objectifs produit et epics existants

| Epic existant | Objectif produit servi | Arbitrage de vision |
|---|---|---|
| `MYB-1` - Boucle de ride mock jouable | Prouver qu'une session courte peut etre lancee, jouee, mise en pause et terminee sans materiel. | Le mock sert a valider le plaisir et le rythme avant la complexite FTMS. |
| Epic conceptuel 1 - Boucle de ride mock | Donner une experience complete: effort, progression, HUD, camera, resume. | La session doit ressembler a une petite aventure, pas a un tableau de bord fitness. |
| Epic conceptuel 2 - Route scenique legere | Installer l'evasion visuelle avec une route continue, des biomes, de la lumiere et du relief. | Le decor premium sert d'abord l'evasion et l'envie de continuer; la lisibilite reste suffisante pour le ride sur rail, sans pipeline asset lourd en V1. |

## Campagne et missions

La campagne raconte une exploration a travers un monde magique deja menace. Le
joueur traverse des regions, retrouve plusieurs reliques et les utilise pour
liberer progressivement le monde d'une menace qui a deja beaucoup progresse.

La quete principale est persistante, mais elle se decoupe en petites expeditions
autonomes. Chaque session doit pouvoir exister seule, tout en faisant avancer le
voyage global.

Les premiers types d'objectifs sont:

- recuperer un tresor ou une relique protegee par un gardien;
- nettoyer une route maudite ou dissiper un brouillard;
- explorer jusqu'a un lieu rare ou sacre.

Les missions sont distinguees par trois criteres prioritaires: duree,
difficulte et type d'objectif. Le risque et la recompense doivent etre lisibles:
une mission plus difficile promet une meilleure recompense, parfois en quantite,
parfois en rarete.

## Smart trainer et FTMS

Le velo connecte est obligatoire pour la vision produit cible. MyBike doit
utiliser au maximum ce que FTMS permet pour enrichir l'experience: resistance,
rythme, variations d'effort, recuperations et moments de soulagement.

La premiere cible produit connectee est Unity macOS via CoreBluetooth/FTMS. Les
cibles WebGL et Android restent secondaires tant qu'un ticket dedie ne les
requalifie pas.

Pour le developpement et les tests, un slider de force peut simuler l'effort du
joueur et permettre de valider les boucles sans materiel. Ce mode reste un outil
de construction, pas le coeur de la promesse.

La sensation physique principale est rythmique: relances, poursuites, fenetres
d'effort et recuperations. La sensation secondaire est le soulagement immediat:
une potion, un bonus, un tresor ou une purification peut rendre temporairement
l'effort plus leger ou aider a survivre a une phase difficile.

## Difficulté et echec

La difficulte doit d'abord se ressentir dans le corps. Elle peut demander de
pousser plus fort, de tenir plus longtemps, ou de rester au-dessus d'un seuil
pendant une fenetre courte. Elle peut aussi augmenter la pression temporelle:
poursuite, jauge, phase finale, opportunite breve.

L'echec est arcade, clair et jamais humiliant. Une mission peut echouer et etre
relancee, mais le joueur doit sentir qu'il etait proche, comprendre comment
faire mieux, et pouvoir changer de difficulte ou de mission sans frustration.

## Boss flow

Un gardien est un climax rythmique. Le format de base contient plusieurs vagues
d'effort separees par de courtes recuperations, puis une phase finale avec le
vrai boss.

Les variantes principales de phase finale sont:

- remplir une jauge en maintenant l'effort pendant une fenetre courte;
- tenir au-dessus d'un seuil jusqu'a la fin d'une poursuite.

Le joueur peut faire un choix tactique simple: utiliser une potion maintenant ou
garder ses forces pour plus tard. Les potions et bonus doivent etre faciles a
comprendre, rapides a utiliser et directement lies au corps: effort allege,
recuperation facilitee, fenetre prolongee, poursuite rendue survivable.

## Recompenses

La V1 doit rester economiquement simple. Trois familles de recompenses sont
prioritaires:

- reliques principales, qui font avancer la campagne;
- potions ou bonus consommables, qui modifient temporairement l'effort ou aident
  pendant les moments de tension;
- deblocages de missions, routes ou regions, qui relancent la curiosite.

Les recompenses meta prolongent l'aventure, mais ne doivent pas remplacer le
sentiment principal: avoir vecu une petite expedition complete.

## Direction visuelle et ton

MyBike repose sur deux couches: un decor premium en fond, un gameplay plus
arcade au premier plan. Dans les moments d'evasion, le premium domine. Dans les
moments de tension, la lisibilite doit redevenir suffisante. Le premium doit
servir le gameplay sans s'auto-censurer: le monde peut etre magique,
spectaculaire et dense par moments, tant que le ride sur rail reste confortable
et comprehensible.

Le ton est arcade fantasy clair, avec une aventure heroique legere. Les
objectifs doivent etre immediatement comprehensibles et plus funs que solennels.
Le texte reste limite: chaque mission peut avoir un mini pitch narratif, mais la
ride elle-meme ne doit pas etre interrompue par du texte, sauf pour annoncer un
evenement important comme une transition, un boss ou un defi.

## Arbitrages assumes

- Accessible plutot que hardcore: la difficulte doit etre claire, ajustable et
  motivante, jamais punitive.
- Contemplatif avant competitif: l'evasion est le point d'entree; la tension
  vient rythmer la session, pas transformer MyBike en sport esport.
- Local-first avant compte utilisateur: la V1 doit pouvoir prouver la boucle
  sans backend, marketplace ou dependance sociale.
- Progression legere plutot que RPG complet: reliques, bonus et deblocages
  soutiennent la ride, mais ne doivent pas voler le temps de pedalage.
- Premium aventure plutot que spectacle confus: le beau decor doit donner envie
  d'avancer et peut etre spectaculaire, mais il ne doit pas casser le confort,
  la comprehension globale de la route ou les moments d'effort.

## Ce que MyBike n'est pas

MyBike n'est pas un RPG complet qu'on joue assis. Il ne doit pas devenir un RPG
trop bavard qui casse le rythme de la ride.

MyBike n'est pas un runner arcade nerveux qui oublie l'evasion. La tension est
la pour ponctuer le voyage, pas pour ecraser la sensation de partir quelque
part.

MyBike n'est pas un produit complexe a lancer quand on veut juste pedaler 20
minutes. La profondeur doit venir des missions, des rythmes d'effort et de la
campagne, pas d'une friction de menu.

## Questions ouvertes

- Quels niveaux de duree seront proposes en premier: 10, 15, 20 minutes, ou un
  autre cadrage?
- Quels seuils d'effort FTMS sont acceptables pour rester motivant sans devenir
  punitif?
- Quelle forme prend la menace principale: brouillard, corruption, gardiens,
  force naturelle, autre symbole?
- Combien de reliques principales faut-il pour une premiere campagne jouable?
- Quels bonus consommables sont autorises en V1, et lesquels sont repousses pour
  eviter une economie trop lourde?
- Jusqu'ou le mode slider doit-il rester disponible hors developpement: demo,
  accessibilite, fallback manuel, ou tests uniquement?

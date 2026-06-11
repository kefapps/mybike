# MYB-37 - Direction artistique stylisee premium Unity

Date: 2026-06-11
Statut local: done
Linear: https://linear.app/kefjbo/issue/MYB-37/myb-003-definir-une-direction-artistique-low-poly-premium-pour-unity

## Objectif

Definir la direction artistique de `unity/Echapee4D` avant la selection et
l'import d'assets Unity.

Ce document sert de mini-bible operationnelle pour les tickets suivants:

- `MYB-38` - politique de licences assets gratuits;
- `MYB-41` - shortlist de modeles 3D gratuits compatibles Unity;
- `MYB-50` - pipeline d'optimisation assets Unity/WebGL;
- `MYB-53` - POC pack d'assets gratuit;
- `MYB-44` - premier scenic corridor jouable.

## Decisions confirmees

### Direction canonique

La direction visuelle canonique est: `stylise premium avec socle low-poly de
production`.

Definition courte: balade scenique premium et aventureuse, avec des moments
fantasy genereux, des silhouettes memorables et une finition visuelle assumee.
Le socle low-poly est une contrainte d'optimisation et de stylisation, pas une
autorisation de rendu cheap ou placeholder. La route avance automatiquement sur
rail: elle peut donc devenir ponctuellement moins evidente si cela sert
l'emerveillement, tant que la camera reste confortable et que le joueur garde un
sentiment de progression.

### Definition du socle low-poly

Dans cette DA, `low-poly` signifie `low-poly de production`:

- geometrie economique mais intentionnelle;
- silhouettes propres et reconnaissables;
- proportions travaillees;
- materiaux, lumiere et couleurs capables de porter la qualite;
- densite de detail controlee;
- composition de scene lisible et premium;
- optimisation WebGL prise en compte des le choix des assets.

Le terme ne valide jamais:

- un asset qui ressemble a un placeholder;
- des primitives brutes posees comme rendu final;
- un pack gratuit incoherent assemble sans direction;
- une simplification qui detruit le charme, la profondeur ou le sens du lieu;
- un rendu "petit jeu cheap" sous pretexte d'economie de polygones.

Les signaux fantasy principaux peuvent sortir de ce socle et devenir plus
riches: high-poly, shaders, animation, hero prop ou effet premium, si leur role
et leur cout sont explicitement assumes dans le ticket qui les implemente.

### Priorite de qualite

Ordre d'arbitrage:

1. Sensation d'aventure / emerveillement.
2. Lisibilite suffisante de ride.
3. Beaute contemplative.

Un asset ou biome est refuse s'il casse le confort camera, masque durablement le
HUD/feedback d'effort, detruit le sentiment d'avancer, ou rend la scene
incomprehensible. Il n'est pas refuse uniquement parce que la route devient
moins lisible pendant un court moment spectaculaire. Il est aussi refuse si sa
seule qualite est d'etre "low-poly" mais qu'il parait pauvre, non fini ou sans
intention artistique.

### Biomes cadrés

La mini-bible cadre cinq biomes nominaux, mais seuls deux sont prioritaires
pour la V1.

Biomes prioritaires V1:

- `Foret claire`
- `Village / campagne pavee`

Biomes cadrés pour backlog:

- `Cote`
- `Montagne`
- `Nuit magique`

`MYB-44` doit viser deux biomes jouables/lisibles, pas cinq biomes complets.

Le fait de commencer par `Foret claire` et `Village / campagne pavee` ne doit
pas tirer la V1 vers un rendu cheap. Ces biomes restent des lieux premium: ils
doivent contenir au moins un signal principal fort, une composition de plans
lisible, des silhouettes memorables et un niveau de finition suffisant pour
donner envie d'explorer la suite.

### Signaux fantasy

Chaque biome peut contenir jusqu'a trois `signaux fantasy premium principaux`.

Cette limite concerne les signaux principaux, pas les micro-accents. Un signal
principal doit etre un vrai moment premium: landmark fort, asset ambitieux,
shader visible, animation, effet de lumiere ou element monumental. Il peut etre
high-poly ou techniquement plus riche que le reste de la scene si le ticket
suivant le budgete explicitement.

Les micro-accents secondaires sont autorises au-dela de ces trois signaux s'ils
soutiennent les signaux principaux sans creer de bruit visuel, d'inconfort ou
de promesse gameplay non cadree.

Exemples de signaux principaux autorises:

- porte de relique avec runes animees;
- arbre sacre monumental avec feuillage shaderise;
- fontaine ou puits magique anime;
- arche de passage avec lumiere volumetrique ou effet equivalent WebGL-safe;
- silhouette de sanctuaire lointain;
- passage enchante court avec brouillard/lumiere;
- element monumental visible a distance;
- asset hero high-poly si son cout/perf est justifie.

Exemples de micro-accents secondaires:

- petites pierres lumineuses;
- marques gravees repetees;
- lanternes simples;
- petits points lumineux;
- variation subtile de materiau;
- brume legere autour d'un signal principal.

Signaux a refuser:

- particules qui mettent en danger la performance ou le confort;
- props de combat dominants sans lien avec la balade;
- foule ou creature qui attire trop le regard sans etre un signal principal
  assume;
- effets qui cachent durablement la route, le HUD ou le feedback d'effort;
- objets fantasy generiques sans lien avec la balade;
- assets chers techniquement mais banals visuellement.

### Surfaces de route

La route n'a pas une surface unique canonique. Elle peut varier par biome:

- asphalte;
- route pavee;
- chemin de terre;
- gravier stylise;
- chemin scenic local.

La constante n'est pas le materiau: c'est la comprehension suffisante de la
progression.

Les surfaces doivent aussi suggerer un ressenti physique futur. Elles peuvent
faire comprendre au joueur, avant meme que la mecanique existe, qu'un passage
devrait etre plus doux, plus vibrant, plus lourd, plus rapide, plus instable ou
plus ceremoniel. Cette suggestion est desirable dans la DA.

Toute surface doit garder:

- une silhouette de route ou un cheminement suffisamment comprehensible;
- des bords, repères ou guides visuels quand la surface se confond avec le sol;
- un contraste suffisant dans les moments ordinaires;
- une lecture stable en premiere personne;
- une compatibilite WebGL simple.

Les surfaces sont un langage visuel de biome et un langage de ressenti futur.
Elles peuvent promettre une intention de sensation, mais ne doivent pas decrire
un effet gameplay precis tant qu'un ticket difficulte/resistance ne l'a pas
implemente.

Exemples:

- paves: vibration, ancien village, effort plus rugueux;
- terre: souplesse, nature, adherence moins nette;
- gravier: instabilite legere, bruit, route plus aventureuse;
- asphalte lisse: vitesse, confort, section de relance;
- dalle/relique: passage ceremoniel ou magique.

### Regles de sources et inspirations

Les references externes sont autorisees comme inspiration textuelle uniquement,
et leur statut doit rester explicite.

Regles:

- pas d'image externe ajoutee au repo dans MYB-37;
- pas d'asset externe ajoute dans MYB-37;
- pas de copie de style d'un jeu, film, studio ou artiste;
- chaque reference doit indiquer `A regarder pour`, `A refuser` et `Source`;
- les references retenues sont `validee partielle`, jamais des autorisations de
  copie de style;
- les references refusees restent documentees comme anti-references utiles.

## Biomes V1

### Foret claire

Intention:

Une foret magique et premium qui alterne les ambiances: passages ouverts et
respirables, zones plus denses, couloirs vegetaux, moments spectaculaires et
respirations lumineuses. La magie et le spectaculaire ne sont pas reserves aux
zones denses: une clairiere ouverte peut etre aussi forte qu'un tunnel vegetal.
Elle ne doit pas ressembler a un simple pack d'arbres gratuits disperse autour
de la route.

Rythme interne attendu:

- ouverture lisible pour installer le lieu;
- densification progressive ou ponctuelle;
- passage tunnel/couloir vegetal autorise s'il reste une sequence, pas l'etat
  permanent du biome;
- sortie vers une respiration, une clairiere ou un landmark fort;
- variation de lumiere/fog pour rendre les transitions sensibles.

Palette attendue:

- verts lumineux mais pas fluorescents;
- troncs chauds et lisibles;
- sol doux, pas trop sombre;
- fog leger bleu-gris ou vert pale;
- accents fantasy froids ou dores, potentiellement forts dans les moments
  signatures.

Surface de route possible:

- chemin de terre clair;
- gravier stylise;
- asphalte etroit borde de vegetation si la route doit rester plus cycliste.

Silhouettes:

- arbres low-poly simples;
- clairieres;
- talus doux;
- pierres de bord de route;
- canopee distante.
- au moins une silhouette signature qui donne une identite forte au biome.

Signaux fantasy premium principaux possibles, maximum trois:

- arbre sacre monumental;
- arche vegetale sacree animee;
- clairiere de relique;
- rideau de brume enchantee court avec lumiere;
- creature/silhouette fantastique lointaine si elle reste non intrusive.

Micro-accents possibles:

- pierres lumineuses en bord de route;
- lucioles ou points lumineux;
- marques gravees sur troncs/pierres;
- petites variations de mousse ou fleurs fantasy.

Refus:

- foret sombre d'horreur;
- densite qui cree une confusion durable ou un inconfort camera;
- plantes qui traversent la route;
- tunnel vegetal constant sans respiration;
- foret generique cheap faite de props repetes sans composition.

### Village / campagne pavee

Intention:

Un passage habite, chaleureux et premium, qui evoque une route de campagne ou
un petit village stylise sans devenir une simulation urbaine. Le village doit
etre un moment de voyage, pas trois maisons low-poly posees au hasard.

Rythme interne attendu:

- campagne ouverte pour annoncer le village;
- approche par champs, murets, lanternes, arches ou silhouettes lointaines;
- resserrement possible dans une rue, un pont, une arche ou une place plus
  dense;
- moment spectaculaire autour d'un signal principal: fontaine, place, clocher,
  arche de relique ou lumiere surnaturelle;
- sortie respirante vers la route, les collines ou une campagne plus ouverte.

Le spectaculaire n'est pas reserve au centre du village: une arrivee par champs
lumineux, un pont magique ou une silhouette lointaine peuvent aussi porter le
moment premium.

Palette attendue:

- pierre claire, beige froid ou gris chaud;
- toits/bois en accents chaleureux et distinctifs;
- vegetation de bord de route;
- ciel lisible;
- accents fantasy lumineux assumés.

Surface de route possible:

- paves stylises;
- route de campagne claire;
- chemin mixte terre/pierre si le village est plus ancien.

Silhouettes:

- murs bas;
- maisons simplifiees;
- murets de pierre;
- arches de village;
- champs ou collines proches;
- panneaux ou balises lisibles.
- au moins une silhouette signature de village visible avant d'y entrer.

Signaux fantasy premium principaux possibles, maximum trois:

- fontaine magique animee;
- arche de village marquee par une relique;
- place centrale avec lumiere surnaturelle;
- moulin, clocher ou tour stylisee avec element magique;
- borne sacree high-detail si elle devient landmark.

Micro-accents possibles:

- lanternes bleutees/dorees;
- marques de relique sur paves ou murs;
- bannieres simples liees a la route;
- petites lumieres de fenetres ou balises.

Refus:

- ville dense;
- details de facade qui detruisent la performance ou le confort;
- medieval-RPG generique sans lien avec la balade;
- props qui forment des obstacles visuels sur la route;
- densite constante sans approche ni respiration;
- village cheap compose de batiments generiques sans landmark ni mise en scene.

## Biomes backlog

### Cote

Intention: ouverture, air, horizon, roche et lumiere.

Signaux possibles: arche rocheuse, phare stylise, pierres bleutees, embruns
magiques, ruines cotieres, lumiere de relique.

### Montagne

Intention: relief, altitude, effort, panorama.

Signaux possibles: cairns lumineux, porte de col, cristaux, silhouettes de
sommet, ancien sanctuaire de passage.

### Nuit magique

Intention: calme, contraste, mystere lisible.

Signaux possibles: etoiles accentuees, balises de route lumineuses, brume
magique, relique lointaine, silhouettes lumineuses de paysage.

Regle speciale: la nuit magique peut etre spectaculaire, mais elle doit garder
des repères de progression et eviter l'inconfort camera.

## Criteres d'acceptation d'asset

Un asset candidat est acceptable s'il respecte tous les criteres suivants:

- compatible Unity WebGL ou budgetable explicitement pour le devenir;
- silhouette memorable a distance;
- style coherent avec `stylise premium avec socle low-poly de production`;
- qualite percue suffisante: l'asset ne doit pas faire cheap une fois place
  dans le biome;
- materiaux compatibles avec le style et la performance, mats ou brillants
  selon le biome;
- textures legeres par defaut; textures plus lourdes autorisees seulement pour
  un signal principal budgete;
- echelle claire ou volontairement spectaculaire par rapport a la route;
- compatible avec un placement hors trajectoire;
- licence verificable dans `MYB-38`;
- fallback possible avec primitive Unity ou placeholder provisoire pour les
  elements ordinaires; fallback simplifie obligatoire pour les signaux
  principaux. Ces fallbacks servent a valider le flux et les performances, pas a
  fixer la qualite DA finale.

## Criteres de refus d'asset

Refuser un asset si:

- la licence est absente ou ambigue;
- le style est trop realiste, trop cartoon, trop horror, trop RPG charge ou trop
  sci-fi;
- la silhouette est confuse en mouvement;
- il exige des shaders, animations ou textures lourdes sans etre un signal
  principal explicitement budgete;
- il attire l'attention sans servir l'aventure, le biome ou l'emerveillement;
- il ne peut pas etre reduit sans perdre son sens;
- il impose une mecanique gameplay non cadrée;
- il ressemble trop a une reference externe identifiable.

## References d'inspiration

Ces references sont des supports de discussion, pas des styles a copier. Chaque
reference doit rester encadree par deux questions:

- `A regarder pour`: ce que la reference peut nous aider a formuler.
- `A refuser`: ce qui ne doit pas entrer dans Echappee 3D.

Statuts possibles:

- `refusee`: reference explicitement refusee pour la DA;
- `validee partielle`: reference autorisee seulement pour les points listes.

### References validees partielles

#### Lonely Mountains: Downhill

Statut: `validee partielle`

Source: https://store.steampowered.com/app/711540/Lonely_Mountains_Downhill/

A regarder pour:

- rapport route / relief;
- outdoor cycliste;
- lecture de trajectoire;
- sensation de balade sportive;
- chemins, forets, descentes et reliefs lisibles.

A refuser:

- sobriete trop seche;
- ambiance trop sport de descente;
- absence de fantasy;
- copie de composition, marque ou identite visuelle.

#### Alto's Adventure / Alto's Odyssey

Statut: `validee partielle`

Sources:

- https://www.altosadventure.com/
- https://www.altosodyssey.com/

A regarder pour:

- rythme contemplatif;
- calme et respiration;
- horizons et silhouettes;
- transitions d'ambiance;
- sensation de voyage fluide.

A refuser:

- minimalisme 2D trop plat;
- lecture lateral-scroll;
- absence de profondeur de scene;
- DA trop silencieuse ou trop abstraite pour une route cyclable 3D.

#### Sable

Statut: `validee partielle`

Source: https://www.shed-works.co.uk/sable

A regarder pour:

- sentiment d'aventure;
- silhouettes monumentales;
- exploration contemplative;
- mystere et ruines;
- grands espaces qui respirent.

A refuser:

- ligne graphique trop identifiable;
- desert / sci-fi trop dominant;
- contours et aplats trop proches de son identite;
- copie de motifs narratifs ou architecturaux.

#### Jusant

Statut: `validee partielle`

Source: https://dont-nod.com/en/games/jusant/

A regarder pour:

- monumentalite lisible;
- progression physique;
- ruines integrees au paysage;
- ambiance respirable mais spectaculaire;
- verticalite comme sentiment d'effort et de destination.

A refuser:

- verticalite / escalade comme structure principale;
- secheresse trop minerale;
- ton trop solitaire si cela ecrase la fantaisie cycliste;
- decor qui devient trop tour/ascension au lieu de route.

#### RiME

Statut: `validee partielle`

Source: https://store.steampowered.com/app/493200/RiME/

A regarder pour:

- ruines stylisees;
- lumiere chaude;
- silhouettes simples mais premium;
- landmarks lisibles a distance;
- ile / nature / mystere doux.

A refuser:

- puzzle-adventure melancolique trop appuye;
- copie de la tour ou de l'ile;
- rythme trop lent;
- structure trop jeu d'enigmes.

#### The Pathless

Statut: `validee partielle`

Source: https://thepathless.com/

A regarder pour:

- mouvement rapide dans un paysage stylise;
- mythologie visuelle;
- forets mystiques;
- landmarks;
- effets premium lisibles en mouvement.

A refuser:

- combat / archerie;
- noirceur de la corruption;
- structure heroique trop action-aventure;
- creature-compagnon ou iconographie trop identifiable.

#### Omno

Statut: `validee partielle`

Source: https://www.playomno.com/

A regarder pour:

- voyage doux;
- variete de biomes;
- creatures etranges non agressives;
- sensation de decouverte;
- ancien monde merveilleux.

A refuser:

- cote puzzle-platformer;
- direction trop petit indie minimal;
- manque de finition premium selon les scenes;
- biomes trop cartes postales si non relies par une vraie route.

#### Journey

Statut: `validee partielle`

Source: https://annapurnainteractive.com/en/games/journey

A regarder pour:

- silhouette d'objectif lointain;
- transitions d'ambiance;
- sentiment de voyage;
- mystere sans surcharge;
- respiration et intensite emotionnelle.

A refuser:

- desert dominant;
- minimalisme extreme;
- identite visuelle trop iconique;
- copie de cape, montagne, dunes ou structure de voyage spirituel.

### References refusees

#### Dorfromantik

Statut: `refusee`

Source: https://www.toukana.com/dorfromantik

A regarder pour:

- douceur village / campagne;
- composition lisible;
- monde chaleureux;
- relation entre champs, forets et habitations.

A refuser:

- camera/isometrie;
- logique puzzle;
- cote trop miniature;
- tuiles et village-carte-postale qui eloignent du ride en premiere personne.

Decision utilisateur: refusee.

#### The Touryst

Statut: `refusee`

Source: https://thetouryst.shinen.com/

A regarder pour:

- formes simples mais finies;
- couleurs franches;
- landmarks;
- finition premium avec geometrie stylisee.

A refuser:

- style voxel trop identifiable;
- ambiance vacances/tourisme trop marquee;
- proportions blocky;
- copie de monuments ou d'iles.

Decision utilisateur: refusee.

## Sortie attendue de MYB-37

MYB-37 est termine quand:

- cette mini-bible existe;
- le glossaire racine contient les termes visuels canoniques;
- les references externes sont separees entre `refusee` et `validee partielle`;
- les criteres d'acceptation/refus peuvent guider `MYB-41`;
- aucun code Unity, asset ou scene n'a ete modifie.

## Decisions hors scope

MYB-37 ne decide pas:

- les licences autorisees definitives: `MYB-38`;
- la shortlist finale d'assets: `MYB-41`;
- l'import Unity ou les reglages d'import: `MYB-42` / `MYB-50`;
- le scenic corridor jouable: `MYB-44`;
- les effets gameplay des surfaces: tickets difficulte / resistance;
- la compatibilite velo connecte: post-baseline / `MYB-40`.

## Plan de verification

- Relire `CONTEXT.md` et ce document pour coherence des termes.
- Verifier qu'aucun code, asset ou scene Unity n'a ete modifie pour MYB-37.
- Verifier que les references externes gardent un statut clair: `validee
  partielle` ou `refusee`.
- Mettre a jour Linear avec le lien vers ce cadrage.

## Decisions validees avec l'utilisateur

La revue utilisateur a ajuste et valide les orientations suivantes:

- direction canonique `stylise premium avec socle low-poly de production`;
- refus explicite du low-poly cheap ou placeholder;
- biomes V1 prioritaires `Foret claire` et `Village / campagne pavee`;
- rythme interne des biomes, avec alternance ouvert/dense/spectaculaire;
- maximum trois signaux fantasy premium principaux par biome, avec micro-accents
  secondaires autorises;
- signaux principaux autorises a etre ambitieux: high-poly, shaders,
  animations et hero assets si budgetes;
- surfaces de route variables et capables de suggerer un ressenti physique
  futur sans promettre une mecanique deja implementee.
- references d'inspiration classees:
  - `validee partielle`: `Lonely Mountains: Downhill`, `Alto's Adventure /
    Alto's Odyssey`, `Sable`, `Jusant`, `RiME`, `The Pathless`, `Omno`,
    `Journey`;
  - `refusee`: `Dorfromantik`, `The Touryst`.

MYB-37 peut etre considere comme termine cote cadrage.

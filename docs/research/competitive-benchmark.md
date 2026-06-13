# MYB-36 - Benchmark indoor cycling

Date de consultation: 2026-06-12

## Resume executif

MYB-36 compare six solutions indoor cycling pour nourrir MyBike sans copier leur
surface visible. Le filtre de decision reste celui du GDD: MyBike est une
balade scenic premium, Unity macOS-first, mock-first, non competitive par
defaut, avec hardware reel et social temps reel parques derriere des tickets
dedies.

Le marche montre trois familles fortes:

- monde virtuel social et gamifie: Zwift, MyWhoosh;
- route reelle/video: ROUVY, FulGaz, Kinomap;
- entrainement/physique/coach data: TrainingPeaks Virtual.

Decision recommandee: MyBike ne doit pas chercher a devenir un Zwift miniature.
La meilleure transposition V1 est une experience locale plus intime: route
preparee, feedback d'effort clair, recompense douce, fiche route lisible,
resume plus emotionnel, quelques objectifs locaux, et prix produit plus simple
que les abonnements lourds du marche.

## Perimetre et methode

Concurrents obligatoires:

- Zwift
- ROUVY
- Kinomap
- MyWhoosh
- TrainingPeaks Virtual
- FulGaz

References annexes limitees a trois:

- TrainerRoad: reference pour plans adaptatifs et focus entrainement.
- Wahoo SYSTM: reference pour contenu guide et entrainement hors velo.
- Peloton: reference pour coaching humain et energie de session.

Lecture des prix:

- les prix sont releves depuis des pages publiques officielles quand possible;
- les variantes publiques visibles sont incluses: mensuel, annuel, multi-user,
  gratuit, essai, lifetime/5 ans, add-on famille, devise/pays si expose;
- les prix caches derriere login, promos personnalisees, regional pricing app
  store non reproductible et bundles hardware non centraux sont exclus;
- les prix peuvent changer, donc chaque ligne doit etre relue avant decision
  business.

## Matrice comparative courte

| Produit | Monde | Materiel | Modele | Force dominante | Pertinence MyBike |
| --- | --- | --- | --- | --- | --- |
| Zwift | mondes virtuels 3D, social massif | smart trainer ideal, capteurs possibles | abonnement | communaute, XP, events, statut | later |
| ROUVY | routes reelles video + avatars AR | smart trainer/capteurs | abonnement solo/duo/group | realisme de lieux, partage plan | now/later |
| Kinomap | videos geolocalisees multi-sport | large compatibilite machine/capteurs | abonnement + famille + long terme | volume de videos, compatibilite | later |
| MyWhoosh | mondes virtuels gratuits, esports | smart trainer/capteurs | gratuit | gratuit + racing + verification | later/avoid |
| TrainingPeaks Virtual | monde virtuel oriente performance | FTMS, Wahoo, Tacx, ANT+, WiFi, capteurs | inclus Premium | physique, coaching, donnees | now/later |
| FulGaz | videos realistes 4K/route | smart trainer/capteurs | abonnement | immersion premiere personne route reelle | now |

## Pricing public releve

### Zwift

Source principale: page officielle Zwift pricing et article officiel Zwift
"How much does Zwift cost". Sources secondaires: ZwiftInsider et Velo pour les
devises regionales publiees lors de la hausse 2024.

| Variante | Prix public releve | Notes |
| --- | --- | --- |
| Monthly US | USD 19.99/mo + taxes | 14-day free trial on monthly plan. |
| Annual US | USD 199.99/year + taxes | No free trial; first-time annual buyers can cancel within 30 days for refund. |
| Monthly EU | EUR 19.99/mo | Official Zwift article. |
| Annual EU | EUR 199.99/year | Official Zwift article and EU annual product page. |
| Monthly UK | GBP 17.99/mo | Official Zwift article. |
| Annual UK | GBP 179.99/year | Official Zwift article. |
| Family / multi-user | none found publicly | Subscription appears per account; no official family plan found. |

Pricing sources:

- https://www.zwift.com/pricing
- https://www.zwift.com/eu-de/news/33635-how-much-does-zwift-cost-and-whats-included-everything-you-need-to-know
- https://zwiftinsider.com/price-increase-2024/
- https://velo.outsideonline.com/news/zwift-increases-price-first-time-since-2017/

### ROUVY

Source principale: ROUVY pricing et ROUVY support pricing. ROUVY expose une
free trial, un mode gratuit limite, Single/Duo/Group, plusieurs devises, pause
jusqu'a 180 jours et loyalty program.

| Variante | EUR | USD | GBP | Notes |
| --- | --- | --- | --- | --- |
| Single monthly | EUR 19.99 | USD 19.99 + taxes | GBP 17.99 | 1 compte. |
| Single yearly | EUR 179.99 | USD 179.99 + taxes | GBP 159.99 | Equivalent affiche sur page: EUR 15.00/mo billed yearly. |
| Duo monthly | EUR 29.99 | USD 29.99 + taxes | GBP 26.99 | Jusqu'a 2 comptes. |
| Duo yearly | EUR 249.99 | USD 249.99 + taxes | GBP 229.99 | Equivalent affiche: EUR 20.83/mo billed yearly. |
| Group monthly | EUR 59.99 | USD 59.99 + taxes | GBP 53.99 | Jusqu'a 5 comptes. |
| Group yearly | EUR 499.99 | USD 499.99 + taxes | GBP 449.99 | Equivalent affiche: EUR 41.67/mo billed yearly. |
| Free trial | 7 jours | 7 days | 7 days | Payment method required. |
| Free limited | 20 km/mo | 20 km/mo | 20 km/mo | Apres essai ou pause/cancel, selon support/pricing page. |
| Pause | jusqu'a 180 jours/an | idem | idem | Peut encore rouler 20 km/mo. |

Autres devises publiques exposees par ROUVY support:

- CZK: Single 399 Kc/mo, 3,999 Kc/year; Duo 599 Kc/mo, 4,999 Kc/year; Group
  1,199 Kc/mo, 9,999 Kc/year.
- CAD: Single CAD 24.99/mo, 224.99/year; Duo CAD 39.99/mo, 349.99/year; Group
  CAD 79.99/mo, 699.99/year, plus taxes.
- AUD: Single AUD 29.99/mo, 269.99/year; Duo AUD 44.99/mo, 379.99/year; Group
  AUD 89.99/mo, 749.99/year.
- NZD: Single NZD 29.99/mo, 269.99/year; Duo NZD 49.99/mo, 429.99/year; Group
  NZD 99.99/mo, 849.99/year.

Pricing sources:

- https://rouvy.com/pricing
- https://support.rouvy.com/hc/en-us/articles/35894370548369-ROUVY-Pricing
- https://support.rouvy.com/hc/en-us/articles/12967187882385-Subscription-FAQ

### Kinomap

Source principale: Kinomap site public et support. La page publique expose les
prix en USD; le support explique les variantes web/app, famille et lifetime.

| Variante | Prix public releve | Notes |
| --- | --- | --- |
| Monthly | USD 11.99/mo ou EUR 11.99/mo selon source/app store | No engagement; app store aussi monthly/annual. |
| Yearly | USD 89.99/year ou EUR 89.99/year selon source/app store | Economie indiquee par Kinomap. |
| Family add-on web | EUR 8/mo ou USD 8/mo selon page | Jusqu'a 4 utilisateurs additionnels, option web seulement, non compatible lifetime. |
| 5-year subscription | USD 359.00 | Plan one-time affiche par Kinomap, "Enjoy Kinomap for 5 years". |
| Lifetime | offre limitee jusqu'au 2025-12-31 | Plus disponible a l'achat apres cette date, mais acces conserve pour anciens acheteurs. |
| Free trial | 14 jours | Pour decouvrir le contenu Premium. |
| Free version | gratuite et illimitee selon App Store | Acces limite vs Premium. |

Sources:

- https://www.kinomap.com/
- https://www.kinomap.com/get-started
- https://support.kinomap.com/hc/en-us/articles/49658641249172-Free-version-of-Kinomap
- https://support.kinomap.com/hc/en-us/articles/115000932446-How-to-choose-your-Kinomap-subscription
- https://support.kinomap.com/hc/en-us/articles/5344442179988-What-s-the-family-option
- https://support.kinomap.com/hc/en-us/articles/44166401552532-What-is-the-Lifetime-plan-on-Kinomap
- https://apps.apple.com/af/app/kinomap-bike-run-row/id611177969

### MyWhoosh

Source principale: site officiel, pages app stores. MyWhoosh se presente comme
gratuit pour rouler, s'entrainer, explorer et participer aux events. Aucun plan
payant public n'a ete trouve sur les sources officielles consultees.

| Variante | Prix public releve | Notes |
| --- | --- | --- |
| App MyWhoosh | Free / zero cost | Windows, iOS, Android, macOS, Apple TV, MyWhoosh HD selon page officielle. |
| Training / routes / events | inclus dans app gratuite | Le site met en avant routes, training programs, special racing events. |
| Paid subscription | none found publicly | Pas de grille officielle payante trouvee. |

Sources:

- https://mywhoosh.com/
- https://mywhoosh.com/getting-started-mywhoosh-cycling-app/
- https://apps.apple.com/us/app/mywhoosh-indoor-cycling-app/id1498889644
- https://play.google.com/store/apps/details?id=com.mywhoosh.whooshgame

### TrainingPeaks Virtual

Source principale: TrainingPeaks pricing athletes et TrainingPeaks Virtual.
TrainingPeaks Virtual est inclus avec TrainingPeaks Premium.

| Variante | Prix public releve | Notes |
| --- | --- | --- |
| TrainingPeaks Basic | Free | Compte de base; utile pour Free 4 All Tuesdays selon blog TP. |
| Premium monthly | USD 19.95/mo | Inclut TrainingPeaks Virtual. |
| Premium yearly | USD 134.99/year | Equivalent affiche: USD 11.25/mo; economie 44%. |
| Premium trial | 14 jours | Pour les fonctions Premium. |
| Virtual standalone | none found | TP Virtual inclus dans Premium. |
| Free 4 All Tuesdays | Free first Tuesday monthly | Acces TP Virtual avec compte Basic, selon article TP events. |

Sources:

- https://www.trainingpeaks.com/pricing/for-athletes/
- https://www.trainingpeaks.com/virtual/
- https://www.trainingpeaks.com/blog/trainingpeaks-virtual-features/
- https://www.trainingpeaks.com/blog/trainingpeaks-virtual-races-and-events/

### FulGaz

Source principale: page FulGaz memberships et support FulGaz. La page publique
affiche AUD; le support liste plusieurs devises.

| Variante | Prix public releve | Notes |
| --- | --- | --- |
| Ride Free | Free 14-day trial | No credit card required selon page memberships. |
| Monthly AUD | AUD 21.99/mo | Page memberships. |
| Annual AUD | AUD 14.99/mo billed annually | Inclut un compte famille gratuit; support liste AUD 179.99/year. |
| Monthly USD | USD 14.99/mo | Support pricing. |
| Annual USD | USD 125.99/year | Support pricing. |
| Monthly GBP | GBP 11.99/mo | Support pricing. |
| Annual GBP | GBP 98.99/year | Support pricing. |
| Monthly CAD | CAD 19.99/mo | Support pricing. |
| Annual CAD | CAD 166.99/year | Support pricing. |
| Monthly EUR | EUR 12.99/mo | Support pricing. |
| Annual EUR | EUR 112.99/year | Support pricing. |
| Family | 1 additional account included with annual | Page memberships. |

Sources:

- https://fulgaz.com/memberships/
- https://support.fulgaz.com/hc/en-us/articles/14802111602573-How-much-does-a-FulGaz-subscription-cost
- https://support.fulgaz.com/hc/en-us/articles/360059453932-How-to-subscribe-purchase-a-FulGaz-subscription

## Analyse par solution

### Zwift

Positionnement: monde virtuel social et gamifie. Zwift vend moins une route
qu'une presence permanente: events, groupes, pace partners, races, XP, drops,
customisation et Companion app.

Trois bonnes idees:

1. Boucle statut + progression + personnalisation: XP, niveaux, monnaie
   virtuelle et unlocks rendent chaque session productive.
2. Evenements et presence sociale constante: meme sans objectif sportif, le
   monde parait habite.
3. Robo pacers / pace partners: une presence d'allure transforme une ride solo
   en rendez-vous accompagne.

Piege a eviter:

- Copier le poids social/competitif. MyBike doit rester balade scenic premium
  et non competitive; une foule d'avatars et de classements ferait basculer la
  promesse.

Transposition MyBike:

- later: progression locale et unlocks (`MYB-65`, `MYB-66`, `MYB-69`);
- later: pace partner local, sans multi-joueur (`MYB-76`);
- avoid V1: races, classements, social temps reel, economie complexe.

### ROUVY

Positionnement: routes reelles avec video, avatars AR, elevation et routes
officielles. Le produit vend "je roule quelque part" plus que "je joue dans un
monde".

Trois bonnes idees:

1. Route reelle comme promesse emotionnelle: le choix de destination vend la
   session avant meme le depart.
2. Fiche de parcours riche: lieu, distance, denivele, event officiel,
   difficulte et objectif donnent un contexte clair.
3. Plans Duo/Group et pause: le pricing reconnait l'usage saisonnier et
   familial de l'indoor cycling.

Piege a eviter:

- Dependre d'un pipeline de videos reelles. Pour MyBike, cela augmenterait trop
  vite les couts de contenu et casserait le style `Stylise Premium`.

Transposition MyBike:

- now: fiche route avant lancement (`MYB-73`);
- now/later: surface/biome/difficulte lisibles avant la ride (`MYB-45`,
  `MYB-80`);
- later: mode "route inspiree du reel", mais stylise, pas video (`MYB-54`).

### Kinomap

Positionnement: catalogue massif de videos geolocalisees et compatibilite
multi-sport/machines. Kinomap est moins "jeu" que "bibliotheque interactive de
parcours".

Trois bonnes idees:

1. Volume et variete: l'utilisateur choisit l'envie du jour par lieu et duree.
2. Compatibilite large: l'app se rend utile avec beaucoup d'equipements.
3. Offre famille web: le produit accepte le foyer comme unite d'usage.

Piege a eviter:

- La bibliotheque infinie. MyBike doit d'abord etre excellent sur une route
  courte et memorable; la quantite de routes est post-MVP.

Transposition MyBike:

- later: tags route/biome/duree pour preparer une future selection;
- later: partage foyer si pricing produit un jour;
- avoid V1: importer ou produire un grand catalogue.

### MyWhoosh

Positionnement: alternative gratuite avec mondes virtuels, training, esports,
UCI, prize money et verification de performance.

Trois bonnes idees:

1. Gratuit comme reduction de friction massive: le prix n'est pas un frein a
   l'essai.
2. Integrite competition: verification, categories et anti-triche construisent
   la confiance en racing.
3. Contenu events et training sans abonnement: l'app cree une valeur percue
   forte avant monétisation directe.

Piege a eviter:

- Se faire aspirer par l'esports. MyBike n'a pas encore besoin de categories,
  cash prizes, anti-doping, verification ou ranking.

Transposition MyBike:

- now: low-friction demo, pas de compte ni materiel obligatoire;
- later: objectifs locaux et defis doux (`MYB-68`, `MYB-77`);
- avoid V1: prize money, anti-cheat competitif, UCI-like legitimacy.

### TrainingPeaks Virtual

Positionnement: simulation et entrainement credible, branchee sur l'ecosysteme
TrainingPeaks. TP Virtual vend la precision, la physique et la continuite avec
les plans/coachs.

Trois bonnes idees:

1. Materiel flexible mais serieux: smart trainer, FTMS, Wahoo, Tacx, ANT+,
   Direct Connect WiFi, power meter, speed/cadence.
2. Physics comme confiance: drafting, braking, cornering, route GPX et fair play
   donnent une base "athlete".
3. Integration training plan: la ride n'est pas isolee, elle appartient a une
   progression.

Piege a eviter:

- Sur-specifier la physique. MyBike doit rester confortable, scenic et mock
  first; la simulation realiste peut attendre les spikes hardware.

Transposition MyBike:

- now: langage FTMS-friendly deja present dans MYB-57/MYB-59/MYB-60;
- later: `MYB-97` ESP32-WROOM, `MYB-58` FTMS, `MYB-40` compatibilite;
- later: analytics locales simples (`MYB-85`), pas coaching complet.

### FulGaz

Positionnement: video realiste premiere personne, routes filmees, immersion
route reelle, moins avatar/social que Zwift/ROUVY.

Trois bonnes idees:

1. Camera premiere personne: l'immersion vient du fait de "voir la route"
   plutot que de regarder un avatar.
2. Routes iconiques: chaque session peut etre vendue par une promesse de lieu.
3. Annual avec compte famille: l'offre reconnait un usage domestique partage.

Piege a eviter:

- Confondre realisme video et qualite MyBike. Notre force peut etre une
  echappee stylisee avec signaux fantasy premium, pas un substitut GoPro.

Transposition MyBike:

- now: renforcer la presentation de route avant depart (`MYB-73`, `MYB-79`);
- now/later: biomes lisibles et landmarks (`MYB-45`, `MYB-47`);
- avoid V1: pipeline video reel.

## Mouvements marche a surveiller

- Consolidation: Cyclingnews a rapporte le 2026-04-29 que Zwift a acquis
  ROUVY tout en annoncant des operations, roadmaps et abonnements separes.
  ROUVY avait aussi acquis FulGaz debut 2025. Cela renforce un axe "Zwift/Rouvy
  = virtuel social + routes reelles/video".
- Prix en hausse: Zwift a augmente en 2024; ROUVY a augmente au 2025-07-15.
  Le marche accepte des prix autour de USD/EUR 19.99/mo pour les leaders, mais
  MyWhoosh montre que le gratuit peut devenir un angle d'acquisition puissant.
- Esports plus serieux: MyWhoosh investit dans verification et anti-doping;
  TrainingPeaks Virtual devient plateforme d'events nationaux US. Cela ne doit
  pas tirer MyBike vers le racing V1.

Sources:

- https://www.cyclingnews.com/cycling-tech-components/we-have-seen-the-indoor-market-grow-at-the-fastest-rate-since-covid-zwift-completes-strategic-acquisition-of-indoor-cycling-competitor-rouvy/
- https://www.cyclingnews.com/news/trainingpeaks-virtual-confirmed-as-new-platform-for-echelon-racing-league-and-usa-cycling-esports-national-championships/
- https://www.cyclingweekly.com/news/our-goal-is-to-protect-clean-riders-virtual-cycling-platform-mywhoosh-introduces-randomly-selected-anti-doping-programme

## Dix opportunites actionnables

Les opportunites ci-dessous "convertissent" les idees en backlog exploitable.
Elles lient d'abord des tickets existants; aucun nouveau ticket n'est cree par
ce document.

| # | Opportunite | Inspiration | Ticket cible | Pertinence | Decision |
| --- | --- | --- | --- | --- | --- |
| 1 | Fiche route avant lancement: lieu, duree, difficulte, moments cles | ROUVY, FulGaz | `MYB-73` | now | Faire avant gamification lourde. |
| 2 | Ecran d'accueil qui vend l'echappee, pas le debug mock | FulGaz, ROUVY | `MYB-79` | now | Prioritaire pour demo. |
| 3 | HUD cadence lisible: moins de bruit, plus de sens | Zwift, TP Virtual | `MYB-80` | now | Garder vitesse/effort/difficulte comprehensibles. |
| 4 | Resume recompense: stats + moment de satisfaction | Zwift, TP | `MYB-70` | now/later | Peut venir juste apres fiche route. |
| 5 | Analytics locales par biome/difficulte/meilleur effort | TP, ROUVY | `MYB-85` | later | Base data pour resume et progression. |
| 6 | XP/niveaux locaux, sobres, non addictifs | Zwift | `MYB-65` | later | Apres resume plus riche. |
| 7 | Badges lies aux biomes et efforts | Zwift, MyWhoosh | `MYB-66` | later | Doit raconter la balade, pas juste compter. |
| 8 | Prompts coach contextuels courts | Peloton, TP, Zwift | `MYB-77` | later | Utile si non intrusif. |
| 9 | Biomes/landmarks qui vendent la destination | ROUVY, FulGaz, Kinomap | `MYB-45`, `MYB-47` | now/later | Differenciateur MyBike. |
| 10 | Hardware proof FTMS sans casser mock-first | TP Virtual, ROUVY | `MYB-97`, `MYB-58`, `MYB-40` | later | Spike seulement, pas dependance V1. |
| 11 | Collectibles legers sur route | Zwift/MyWhoosh gamification | `MYB-72` | later | A cadrer pour ne pas nuire au scenic ride. |
| 12 | Ghost/pace partner local | Zwift robo pacers | `MYB-76`, `MYB-71` | later | Local-first uniquement. |

## Idees low-cost a fort effet

1. Fiche route pre-ride avec trois pictos: duree, difficulte, ambiance.
2. "Moment cle" annonce avant la ride: climb, village, foret, descente.
3. Resume avec meilleur moment: segment le plus dur, biome prefere, meilleur
   effort estime.
4. Badge purement local "premiere echappee", "premiere cote", "ride sans pause".
5. Prompt coach minimal au changement de segment: court, humain, desactivable.
6. Surface de route plus expressive: pave, foret, cote, nuit magique.
7. Prix/positionnement futur simple: "jouable sans hardware" comme promesse.
8. Free/demo friction basse: aucun compte requis pour la vertical slice.
9. Pause saisonniere comme inspiration produit future si abonnement un jour.
10. Comparaison des modes: `mock`, `manual fallback`, `trainer later` dans le
    langage produit.

## Idees a eviter pour la V1

- Courses, classements globaux, categories et anti-cheat.
- Social temps reel et presence massive d'avatars.
- Pipeline de videos reelles type ROUVY/FulGaz/Kinomap.
- Grand catalogue de routes.
- Modele pricing complexe avant preuve de valeur.
- Dependances hardware obligatoires.
- Coaching data avance ou plan builder complet.
- Economie de jeu avec monnaie, boutique, loot ou unlocks trop nombreux.

## Decisions recommandees pour MyBike

### Trois idees a faire maintenant

1. `MYB-73` - Creer une fiche route Unity avant lancement.
2. `MYB-79` - Refondre l'ecran d'accueil Unity pour vendre la balade.
3. `MYB-80` - Stabiliser la cadence HUD Unity et accessibilite macOS-first.

### Trois idees apres vertical slice

1. `MYB-70` - Transformer le SummaryScreen en moment de recompense.
2. `MYB-65` + `MYB-66` - Progression locale et badges.
3. `MYB-97` - Spike ESP32-WROOM FTMS quand on veut reprendre hardware.

### Trois idees a eviter

1. Social/racing global facon Zwift/MyWhoosh.
2. Routes video reelles comme pipeline principal.
3. Abonnement/pricing complexe avant une demo qui donne envie.

### Prochain ticket recommande

`MYB-73` a deja converti cette recherche en valeur demo avec la fiche route
Unity. Le prochain ticket recommande est donc `MYB-79`, pour mettre l'accueil
au meme niveau et vendre l'echappee avant meme le lancement de la session.

## Impacts performance, accessibilite, licence

Performance:

- Les idees recommandees maintenant sont principalement UI/documentaires et ne
  devraient pas augmenter fortement la charge Unity.
- Les idees scenic/landmark devront repasser par `Budget performance` et les
  guardrails Unity deja documentes.

Accessibilite:

- Les inspirations a retenir doivent privilegier lisibilite, cadence de HUD
  controlee, textes courts et feedbacks non intrusifs.
- Les prompts et badges devront rester desactivables ou discrets si la ride
  doit rester calme.

Licence:

- MYB-36 n'importe aucun asset et ne copie aucune UI concurrente.
- Les futures inspirations visuelles doivent rester conceptuelles; tout asset
  tiers repasse par `Licence Verifiee`, `Scan Asset V1` et `Shortlist Asset V1`.

## Sources principales

- Zwift pricing: https://www.zwift.com/pricing
- Zwift official cost article: https://www.zwift.com/eu-de/news/33635-how-much-does-zwift-cost-and-whats-included-everything-you-need-to-know
- ROUVY pricing: https://rouvy.com/pricing
- ROUVY support pricing: https://support.rouvy.com/hc/en-us/articles/35894370548369-ROUVY-Pricing
- ROUVY subscription FAQ: https://support.rouvy.com/hc/en-us/articles/12967187882385-Subscription-FAQ
- Kinomap: https://www.kinomap.com/
- Kinomap support subscription: https://support.kinomap.com/hc/en-us/articles/115000932446-How-to-choose-your-Kinomap-subscription
- Kinomap family option: https://support.kinomap.com/hc/en-us/articles/5344442179988-What-s-the-family-option
- Kinomap lifetime: https://support.kinomap.com/hc/en-us/articles/44166401552532-What-is-the-Lifetime-plan-on-Kinomap
- MyWhoosh: https://mywhoosh.com/
- MyWhoosh getting started: https://mywhoosh.com/getting-started-mywhoosh-cycling-app/
- TrainingPeaks pricing: https://www.trainingpeaks.com/pricing/for-athletes/
- TrainingPeaks Virtual: https://www.trainingpeaks.com/virtual/
- TrainingPeaks Virtual features: https://www.trainingpeaks.com/blog/trainingpeaks-virtual-features/
- TrainingPeaks Virtual events: https://www.trainingpeaks.com/blog/trainingpeaks-virtual-races-and-events/
- FulGaz memberships: https://fulgaz.com/memberships/
- FulGaz subscription cost: https://support.fulgaz.com/hc/en-us/articles/14802111602573-How-much-does-a-FulGaz-subscription-cost
- Cyclingnews indoor apps 2026: https://www.cyclingnews.com/features/indoor-cycling-apps/
- Cyclingnews Zwift/Rouvy acquisition: https://www.cyclingnews.com/cycling-tech-components/we-have-seen-the-indoor-market-grow-at-the-fastest-rate-since-covid-zwift-completes-strategic-acquisition-of-indoor-cycling-competitor-rouvy/
- Cyclingnews TrainingPeaks Virtual esports: https://www.cyclingnews.com/news/trainingpeaks-virtual-confirmed-as-new-platform-for-echelon-racing-league-and-usa-cycling-esports-national-championships/
- Cycling Weekly MyWhoosh integrity: https://www.cyclingweekly.com/news/our-goal-is-to-protect-clean-riders-virtual-cycling-platform-mywhoosh-introduces-randomly-selected-anti-doping-programme

---
title: "Comparaison video - avant/apres MYB-8"
project: "mybike"
date: "2026-06-06"
status: "completed"
---

# Comparaison video - avant/apres MYB-8

## Captures comparees

Baseline pre-MYB-8:

- Dossier: `_bmad-output/video-captures/ride-visual-audit-2026-06-06T20-16-57-308Z/`
- Video: `ride-visual-audit-30s.mp4`
- Duree: 32.36 s
- Taille MP4: 490847 octets
- Bitrate video: 118746 bps
- Contact sheet: `ride-visual-audit-contact-sheet.jpg`

Post-MYB-8:

- Dossier: `_bmad-output/video-captures/ride-visual-audit-2026-06-06T21-15-32-492Z/`
- Video: `ride-visual-audit-30s.mp4`
- Duree: 31.96 s
- Taille MP4: 964845 octets
- Bitrate video: 238957 bps
- Contact sheet: `ride-visual-audit-contact-sheet.jpg`
- HTTP: 200 OK
- Page errors: aucune
- Console: un 404 isole deja connu, probablement favicon/asset absent

## Verdict

MYB-8 a nettement ameliore la scene. La route n'est plus un simple ruban,
les bas-cotes et marquages structurent la lecture, et les objets lateraux
donnent enfin des reperes de mouvement. La scene passe de "prototype plat" a
"premiere balade scenic stylisee".

Le rendu reste cependant encore trop systemique et trop propre. Il manque une
identite de lieu forte, une lumiere plus expressive, de la verticalite, du
relief terrain et quelques silhouettes memorables.

## Ce qui progresse

- **Route**: beaucoup plus lisible grace aux bas-cotes, bandes et marquages.
- **Mouvement**: les objets lateraux proches donnent un debut de parallax.
- **Profondeur**: horizon/fog/objets lointains rendent l'image moins vide.
- **Biomes**: meilleure separation visuelle, meme si encore trop timide.
- **Densite**: le centre de l'image est moins sterile.

## Ce qui manque encore

1. **Identite scenic**
   - Le monde reste generique: route, sol, poteaux, arbres simples.
   - Il manque un motif reconnaissable: colline, falaise, tunnel vegetal,
     village lointain, pont, lac, phare, belvedere ou autre repere fort.

2. **Relief et composition**
   - Le terrain reste tres horizontal.
   - Peu de variation de hauteur ou de silhouette sur les bords.
   - L'horizon est plus rempli, mais pas encore compose comme un paysage.

3. **Lumiere et atmosphere**
   - La scene est claire et lisible, mais encore plate.
   - Il manque un mood: lever de soleil, brume coloree, ombres, contraste,
     direction de lumiere plus assumee.

4. **Assets distinctifs**
   - Les props proceduraux font le travail de densite.
   - Quelques assets low-poly plus caracteristiques pourraient maintenant aider,
     mais seulement s'ils servent la composition.

## Recommandation

Ne pas partir tout de suite sur un pipeline Meshy complet. Le meilleur prochain
pas est un **Scenic Mood Pass** court:

1. ajouter un landmark ou motif fort par biome;
2. ajouter relief/bermes/silhouettes de terrain autour de la route;
3. travailler lumiere, fog et ciel pour un mood identifiable;
4. optionnel: generer 1 ou 2 assets Meshy maximum si un landmark simple est
   difficile a faire en procedural.

Meshy devient pertinent pour des objets precis et instanciables, par exemple:

- rocher/falaise stylisee;
- petit panneau/borne cyclable;
- tronc ou arbre low-poly distinctif;
- arche/pont/tunnel scenic.

Il ne doit pas remplacer le travail de composition: route, terrain, lumiere,
density placement et camera restent les leviers principaux.

## Prochaine action proposee

Utiliser `gds-correct-course` pour choisir entre:

1. **Scenic Mood Pass procedural** recommande;
2. **Meshy cible pour landmarks** si on veut tester 1-2 assets generes;
3. **Camera/speed feel pass** si la priorite devient la sensation plus que le
   decor.

Ne creer qu'une seule story suivante, ou aucune si la decision est de faire
un playtest humain avant.

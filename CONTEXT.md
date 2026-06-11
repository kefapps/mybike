# Context: MyBike Visual Direction

## Purpose

This context owns the shared visual language for MyBike's active Unity WebGL
work. It is a glossary, not an implementation plan.

## Terms

### Licence Verifiee

Definition: A third-party asset license is verified when the team has a
canonical source URL, the exact asset or pack name, the original author, the
license shown at review time, a license URL or equivalent source-page proof,
and the retrieval date.

Relationships:

- `Licence Verifiee` gates any free third-party asset before it can become an
  `Asset Tiers Approuve`.
- It supports MYB-38 and protects the MYB-37 art direction from untraceable
  free assets.

Examples:

- A Kenney, Poly Haven, Unity Asset Store, Sketchfab, GitHub, or original
  author page where the asset, author, license and usage terms are visible.

Non-examples:

- A reupload, a loose file, a collection with no original author link, or a
  "free asset" page that does not expose the license for that exact asset.

### Asset Tiers Approuve

Definition: A free third-party resource that has a verified source, an accepted
license, complete metadata, and status `approved` in the Unity asset manifest.

Relationships:

- Only an `Asset Tiers Approuve` may be imported into `unity/Echapee4D` or
  shipped in a WebGL build.
- Assets with unclear terms stay in `needs-review`; rejected assets stay
  documented only if that helps future decisions.

Examples:

- A CC0 model from a canonical author page with metadata, source URL,
  retrieval date, intended use, local path and attribution decision recorded.

Non-examples:

- An attractive model under `NC`, `ND`, or `SA` terms; an AI-generated pack
  with unclear commercial/build rights; a model copied too directly from a
  MYB-37 inspiration reference.

### Attribution Asset

Definition: The credit text and metadata required by a third-party asset
license, stored in `THIRD_PARTY_ASSETS.md` and in the Unity manifest. It appears
in the build when a Credits surface exists, but not in the ride HUD.

Relationships:

- `Attribution Asset` is mandatory for `CC-BY` and any approved exception that
  requires credit.
- It can be added by courtesy for `CC0` or public-domain assets only if it stays
  accurate.

Examples:

- `Asset Name` by `Author`, license `CC-BY 4.0`, source URL, license URL,
  modifications and retrieval date.

Non-examples:

- A vague "free asset pack" mention, a missing author, or a long URL displayed
  directly in the gameplay HUD.

### Stylise Premium

Definition: The canonical visual direction for MyBike: a stylized scenic
cycling world with generous fantasy moments, memorable silhouettes, strong
material choices, and a visible premium finish. The low-poly baseline is an
optimization and art-direction constraint, not a permission to look crude.

Aliases: scenic fantasy lisible, premium scenic fantasy.

Relationships:

- `Stylise Premium` guides `Biome`, `Signal Fantasy Premium`, `Surface de
  Route`, and `Low-poly de Production`.
- It favors adventure and wonder first, while keeping enough orientation for an
  automatic on-rail ride.

Examples:

- Clean stylized geometry with deliberate silhouettes, good lighting, rich
  materials, and one or more memorable premium signals.

Non-examples:

- Placeholder-looking low-poly, faceted objects with no composition, raw asset
  packs dropped into the scene, or cheap simplified meshes used as a final
  quality bar.

### Scenic Fantasy Lisible

Definition: A readability guardrail inside `Stylise Premium`: a scenic cycling
world with generous fantasy moments, built to create wonder while keeping the
ride understandable enough.

Aliases: scenic fantasy readable, old name for the broader direction.

Relationships:

- `Scenic Fantasy Lisible` supports `Stylise Premium`.
- It favors adventure and wonder first, while keeping enough orientation for an
  automatic on-rail ride.

Examples:

- A forest ride with luminous stones, carved markers, a sacred tree, and a
  short magical passage that can briefly make the route less obvious.

Non-examples:

- Visual density that makes the ride uncomfortable, hides too much of the world,
  or breaks the player's sense of forward motion.
- A realistic cycling simulator with no sense of adventure.

### Low-poly de Production

Definition: A stylized optimization baseline that uses economical geometry,
clean silhouettes, controlled detail density, strong lighting/materials, and
careful composition. It must look intentional and finished, not like a
placeholder.

Aliases: optimized stylized geometry, production low-poly.

Relationships:

- `Low-poly de Production` sits under `Stylise Premium`.
- `Signal Fantasy Premium` elements may exceed this baseline when intentionally
  budgeted as hero moments.
- It is evaluated by perceived quality first, then polygon budget.

Examples:

- Simplified trees with elegant silhouettes and good materials, stylized rocks
  with readable planes and believable grounding, village buildings with clean
  proportions and a strong composition.

Non-examples:

- Blockout meshes, flat unlit primitives, noisy decimated assets, low-detail
  props that feel empty, or any asset that reads as "cheap low-poly".

### Premium Aventure

Definition: A quality bar where the scene first sells adventure, place, and
wonder. Ride readability remains a guardrail, not the dominant aesthetic rule.

Aliases: premium lisible is the old, stricter wording.

Relationships:

- `Premium Aventure` is the first aesthetic criterion for assets and biomes.
- It can accept temporary route ambiguity when the camera remains comfortable
  and the player can still feel forward progress.

Examples:

- Strong landmarks, generous fantasy lighting, memorable silhouettes, and route
  readability that is good enough for an on-rail ride.

Non-examples:

- Visual effects that cause discomfort, total loss of orientation, or hide the
  HUD/effort feedback.
- A biome that looks like a cheap collection of repeated free assets without a
  signature landmark, composition, or sense of place.

### Signal Fantasy Premium

Definition: A high-value fantasy feature used to make a biome memorable and
adventurous. A main signal can be visually ambitious: high-poly, shader-driven,
animated, or otherwise premium, as long as it is intentionally budgeted later.

Relationships:

- A biome may contain up to three main `Signal Fantasy Premium` elements.
- Secondary micro-accents are allowed when they support the main signals and do
  not create noise.
- A main signal can be a landmark, animated element, shader moment, lighting
  motif, monumental silhouette, or premium prop.

Examples:

- A luminous relic gate with animated runes, an oversized sacred tree with
  shader-driven leaves, a magical village fountain, or a mountain shrine with
  moving light.

Non-examples:

- Effects or props that are visually expensive without being a main signal,
  create discomfort, or promise gameplay that does not exist yet.

### Biome

Definition: A route region with a coherent identity, surface language,
landscape silhouettes, and fantasy signals. A biome can contain several internal
moods, such as open, dense, calm, magical, or spectacular passages.

Relationships:

- A biome can vary `Surface de Route`, palette, fog, vegetation, landmarks, and
  lighting while preserving `Premium Aventure`.
- A biome is not required to keep one constant density or one constant intensity
  level throughout the ride.

Examples:

- Foret claire, village/campagne pavee, cote, montagne, nuit magique.

Non-examples:

- A random asset cluster that does not change the rider's sense of place.

### Surface de Route

Definition: The visible riding surface and its edge language. It can change by
biome and should suggest a future physical feel, such as smooth, rough, heavy,
soft, fast, or ceremonial.

Aliases: road material, route surface.

Relationships:

- `Surface de Route` is visual language and future feel language.
- It may strongly suggest future effort/resistance sensations, but it does not
  create a gameplay contract until a dedicated issue implements that behavior.

Examples:

- Smooth asphalt, paved village road that suggests vibration, dirt path that
  suggests softness, gravel-like scenic path that suggests rougher effort.

Non-examples:

- A surface that blends into the ground or implies a precise implemented
  mechanic that the current build does not actually have.

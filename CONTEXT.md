# Context: MyBike Visual Direction

## Purpose

This context owns the shared visual language for MyBike's active Unity
macOS-first work. It is a glossary, not an implementation plan.

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
This approves legal and technical import/use for a POC; it does not mean the
asset is adopted as final V1 art direction.

Relationships:

- Only an `Asset Tiers Approuve` may be imported into `unity/Echapee4D` or
  shipped in a macOS/WebGL build.
- Assets with unclear terms stay in `needs-review`; rejected assets stay
  documented only if that helps future decisions.
- Final art-direction adoption stays a separate POC verdict.

Examples:

- A CC0 model from a canonical author page with metadata, source URL,
  retrieval date, intended use, local path and attribution decision recorded.

Non-examples:

- An attractive model under `NC`, `ND`, or `SA` terms; an AI-generated pack
  with unclear commercial/build rights; a model copied too directly from a
  MYB-37 inspiration reference.

### Verdict Artistique V1

Definition: The art-direction verdict for an imported or inspected asset after
it is seen in context. It answers whether the asset can support the V1 visual
quality bar, independently from legal/technical import approval.

Relationships:

- `Verdict Artistique V1` is separate from `Asset Tiers Approuve`.
- `adoptable-v1` means the asset can plausibly ship in the V1 direction after
  normal integration polish.
- `support/fallback` means the asset is useful for validation, blockout,
  fallback, or secondary support, but should not carry the premium look alone.
- `reject` means the asset should not be used for the V1 visual direction even
  if its license and import are technically valid.

Examples:

- A village landmark that looks premium in the Unity scene can be
  `adoptable-v1`.
- A clean but plain road module can be `support/fallback`.

Non-examples:

- Treating manifest status `approved` as automatic final art adoption.
- Keeping an asset because it is legally free while it still looks cheap in the
  scene.

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

### Scan Asset V1

Definition: The broad review pool for MYB-41-style asset research: candidates
that may be sources, packs, or exact assets, scanned for art fit, license
clarity, Unity/macOS usefulness, and likely integration effort.

Relationships:

- A `Scan Asset V1` can become a `Shortlist Asset V1`, stay `needs-review`, or
  be rejected.
- The scan may include 25 to 30 candidates so the final shortlist is not too
  narrow.

Examples:

- A biome kit source, a paved-road material pack, a forest prop set, or a hero
  landmark model with a visible source and license page.

Non-examples:

- An asset imported into Unity before license review, or a vague inspiration
  image with no reusable asset source.

### Shortlist Asset V1

Definition: A candidate selected from the broad scan for near-term Unity POC
evaluation. It has a verified source, an accepted MYB-38 license by default,
clear intended use, and a known integration effort.

Relationships:

- A `Shortlist Asset V1` is not imported automatically; it becomes an
  `Asset Tiers Approuve` only after manifest metadata and approval are complete.
- The final MYB-41 shortlist should stay balanced across biome kits, route
  surfaces/materials, ambience props, and `Signal Fantasy Premium` candidates.

Examples:

- A CC0 forest kit suitable for `Foret claire`, a CC-BY cobblestone material for
  `Village / campagne pavee`, or a premium landmark model with acceptable
  attribution.

Non-examples:

- A visually promising Unity Asset Store free asset that requires a policy
  exception, or a model with unclear author/license metadata.

### Inspection Asset V1

Definition: The evidence pass required before an asset candidate can enter the
MYB-41 final shortlist. It combines visual inspection of previews or renders
with technical inspection of source, license, formats, file size, texture needs,
animation state, and likely Unity/macOS integration effort.

Relationships:

- `Inspection Asset V1` protects `Shortlist Asset V1` from relying only on pack
  descriptions or marketing copy.
- It does not import the asset into Unity; import and scene validation belong to
  later POC/import tickets.

Examples:

- Checking preview images for silhouette, density and premium fit, then checking
  that the download offers FBX/OBJ/GLB, texture files, accepted license, author,
  source URL, and reasonable integration risk.

Non-examples:

- Copying a pack description into the shortlist without looking at the preview,
  license page, or technical file information.

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

### Route Difficulty Cue

Definition: A lightweight in-world visual cue that helps the rider anticipate
or understand a difficult route segment, such as a climb, sprint, descent, or
recovery. It belongs beside or above the route and must preserve route, HUD, and
effort-feedback readability.

Relationships:

- A `Route Difficulty Cue` is tied to route difficulty or segment data.
- It is gameplay feedback, not a `Signal Fantasy Premium`; it should be
  readable and repeatable, not a dominant landmark.
- It complements the HUD effort/slope/segment readouts instead of replacing
  them.

Aliases: indice visuel de difficulte, route difficulty marker, segment cue.

### Fiche Route

Definition: A lightweight pre-ride surface that previews the single canonical
route before the mock ride starts. It gives the rider route facts and intent:
distance, estimated duration, biomes, overall difficulty, and up to three key
moments.

Relationships:

- A `Fiche Route` describes one `Scenic Corridor`; it is not a route browser.
- It can use calculated route stats and small editorial labels, but it must not
  promise hardware telemetry, training plans, or unimplemented route variants.
- It should hand off directly to the ride loop and keep mock mode playable.

Aliases: apercu de l'echappee, route preview, pre-ride route card.

### Moment Cle

Definition: A short pre-ride promise for a memorable or mechanically relevant
part of the route, such as the warmup, the main climb, or the short sprint.

Relationships:

- A `Moment Cle` can reference `Route Difficulty Cue`, `Biome`, or effort
  pacing, but it remains descriptive rather than a quest objective.
- A `Fiche Route` should surface at most three `Moment Cle` entries for the MVP.

Aliases: moment cle, route highlight, key moment.

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

### Scenic Corridor

Definition: A playable route stretch that turns one or more biomes into a
continuous first-person ride space with readable edges, surface changes,
roadside silhouettes, and a few memorable accents.

Aliases: corridor scenic, visual corridor, ride corridor.

Relationships:

- A `Scenic Corridor` uses `Biome`, `Surface de Route`, `Scenic Fantasy
  Lisible`, and selective `Signal Fantasy Premium` language.
- A `Scenic Corridor` must stay playable first: route, camera, HUD, mock mode,
  and difficulty feedback remain readable while the environment becomes richer.

Examples:

- A village/countryside paved stretch with warmer road surface, low walls,
  cottages, barrels, trees, subtle lights, and a clear ride lane.

Non-examples:

- A decorative asset gallery, a disconnected set of props, or a fully separate
  open-world level.

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

### Baseline performance

Definition: A dated performance measurement for the Unity macOS-first vertical
slice on a named local machine and Unity version.

Relationships:

- A `Baseline performance` records what the project currently does; it is not a
  promise that every supported Mac will match it.
- It can be compared against a `Budget performance` in later visual, HUD, asset,
  or delivery work.

Examples:

- A MYB-51 Editor measurement on MacBook Pro Mac14,9 / M2 Pro / macOS 26.5.1 /
  Unity 6000.4.10f1.

### Budget performance

Definition: A target, warning, or red threshold used to evaluate whether the
Unity macOS-first vertical slice is staying healthy as visuals and assets grow.

Relationships:

- A `Budget performance` is a decision aid, not an automatic blocker unless a
  specific issue or validator explicitly makes it blocking.
- A red result means "needs review before relying on this direction", not
  "must fail the ticket" by default.
- WebGL budgets are secondary and apply only when a ticket explicitly asks for a
  browser proof.

Examples:

- 60 fps target, warning below 45 fps, red below 30 fps.
- macOS build size target <= 500 MB, warning above 750 MB, red above 1 GB.

### Garde-fou performance

Definition: A lightweight rule, report, or validator that helps catch obvious
performance drift without becoming a full profiling or optimization pipeline.

Relationships:

- A `Garde-fou performance` can reference a `Baseline performance` and a
  `Budget performance`.
- It should preserve mock mode and Unity macOS-first delivery while keeping
  WebGL secondary unless explicitly scoped.

Examples:

- A report that counts renderers, estimated triangles, active lights, materials,
  console errors or warnings, and measured fps bands for the current Unity scene.

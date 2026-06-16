# Context: MyBike Visual Direction

## Purpose

This context owns the shared visual language for MyBike's active Unity
macOS-first work. It is a glossary, not an implementation plan.

## Art Direction Scope

The global MyBike visual direction lives in this document, `AGENTS.md`, and
applicable ADRs.

The forest corridor has a dedicated product art bible:

`docs/art-direction/mybike-forest-art-bible-v0.md`

That file is specific to the Art Rescue forest corridor. It inherits global
visual rules such as `Stylisé Premium de Production`, route-camera-first
validation, `Premium target`, `Surface Canonique de Validation Visuelle`,
`Checkpoint insuffisant`, and `Ambition Visuelle Mesurée`.

It must not be treated as the final art bible for every future MyBike biome.
Imported Art Rescue docs under
`_bmad-output/implementation-artifacts/art-rescue-forest-corridor/` are source
material and implementation evidence only. If they conflict with the canonical
forest art bible, the canonical art bible wins.

## Art Rescue Visual Validation

MYB-142 separates diagnostic evidence from reusable validation rules.

Diagnostic:

- `docs/art-direction/myb-137-visual-diagnosis.md`

Reusable rubric:

- `docs/validation/forest-corridor-shot-rubric.md`

The MYB-137 diagnosis is a dated calibration baseline. It is a `Diagnostic
Surface`, not final production validation.

The forest corridor shot rubric is the canonical review tool for future Art
Rescue visual tickets. It defines the 9 criteria, blocking/contributive split,
thresholds, review templates, and validation surfaces.

MYB-137 calibrates the rubric. The canonical route camera validates production
quality.

## Terms

### Diagnostic Surface

Definition: Evidence used to audit, calibrate, compare, and learn from a visual
state without treating that state as final production validation.

Relationships:

- `Diagnostic Surface` can support scoring, problem discovery, before/after
  explanation, and follow-up routing.
- It cannot prove `Premium target`, close a visible production ticket, or promote
  an asset.
- `docs/art-direction/myb-137-visual-diagnosis.md` treats MYB-137 as a
  `Diagnostic Surface`.
- The reusable scoring rules live in
  `docs/validation/forest-corridor-shot-rubric.md`.

Examples:

- MYB-137 route and overview captures used to calibrate the visual rubric.
- A proof-of-volume scene used as a counter-example and follow-up map.

Non-examples:

- A canonical ride-corridor route screenshot used to close a production visual
  ticket after `Premium target` and human validation.
- An isolated asset preview used as a substitute for route-camera validation.

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
license, complete metadata, and `Intake Status` `approved` in the Unity asset
manifest. This approves legal and technical import/use for a POC; it does not
mean the asset is adopted as final V1 art direction or promoted to production.

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

### Intake Status

Definition: The manifest status that describes whether an asset has enough
traceability, provenance, license, cleanup, and review evidence to continue
through the asset intake pipeline.

Relationships:

- `Intake Status` is separate from `Promotion Status`.
- `approved` means the asset intake record is acceptable; it does not mean the
  asset belongs in production scenery.
- MYB-143 uses `quarantine`, `review`, `approved`, `rejected`, and `deprecated`
  as intake statuses.
- `reviewStatus` is not a canonical manifest field. If it appears in the real
  manifest, MYB-144 should treat it as an error.

Examples:

- A Tripo prop with provider job id, prompt summary, license note, local
  quarantine path, reviewer, cleanup notes, and validator evidence can move from
  `quarantine` to `review`, then to `approved`.

Non-examples:

- Treating an `approved` manifest as permission to place the asset directly in
  the canonical ride corridor.
- Using one `reviewStatus` field to mix provenance trust and production
  promotion.

### Asset Manifest Source Type

Definition: The source category recorded in the asset manifest so the intake
pipeline can apply the right provenance, license, cleanup, and validation rules
without changing the promotion gate.

Relationships:

- MYB-143 covers every Art Rescue asset candidate that may be promoted, not only
  Meshy/Tripo or third-party assets.
- The same manifest can track `third_party`, `ai_generated`, `blender_mcp`,
  `in_house_authored`, and `unity_builtin_or_procedural` candidates.
- Source-specific evidence differs, but `Promotion Status` still requires
  controlled review before production use.

Examples:

- A Poly Haven model is `third_party`.
- A Tripo output is `ai_generated`.
- A reusable root arch authored through Blender MCP is `blender_mcp`.
- A hand-authored local prop is `in_house_authored`.
- A Unity primitive or generated mesh used as a controlled candidate is
  `unity_builtin_or_procedural`.

Non-examples:

- Exempting Blender MCP or in-house assets from the manifest because they were
  not downloaded from a marketplace.

### Art Rescue Asset Manifest

Definition: The versioned, machine-readable manifest that records intake and
promotion evidence for Art Rescue asset candidates.

Relationships:

- The canonical schema documentation lives at
  `docs/schemas/third-party-asset-manifest.md`.
- The canonical manifest data lives at
  `docs/manifests/art-rescue-asset-manifest.json`.
- The canonical MYB-144 Unity validator specification lives at
  `docs/validators/unity-art-asset-validator-spec.md`.
- MYB-144 validators should read the canonical manifest data, not scrape
  Markdown examples.
- The manifest covers all Art Rescue asset candidates that may be promoted,
  regardless of `Asset Manifest Source Type`.
- The real manifest is a versioned object with `schemaVersion`, `updatedAt`,
  and `assets`; it is not a flat asset list.
- Documentation examples live in `docs/schemas/third-party-asset-manifest.md`,
  not in the production manifest data.
- `example: true` is allowed in documentation or clearly named fixtures only. It
  is forbidden in the real manifest.

Examples:

- A Blender MCP root arch candidate has one manifest entry with source type,
  local paths, cleanup status, validator evidence, `Intake Status`, and
  `Promotion Status`.
- The initial real manifest can be:
  `{"schemaVersion":1,"updatedAt":"2026-06-16","assets":[]}`.

Non-examples:

- A Markdown-only example that cannot be used by validators.
- A production manifest that contains a fake example asset.

### MYB-144 Art Asset Validator

Definition: The bounded Unity Editor validator that enforces the MYB-143 Art
Rescue asset manifest gate and runs safe technical checks for asset promotion.

Relationships:

- The V1 code lives at
  `unity/Echapee4D/Assets/MYB144/Editor/MYB144ArtAssetValidator.cs`.
- The Unity menu entry is
  `Tools/MyBike/Validation/MYB-144 Art Asset Validator`.
- The report path is
  `_bmad-output/unity-test-results/myb-144-art-asset-validator-report.md`.
- The canonical spec lives at
  `docs/validators/unity-art-asset-validator-spec.md`.
- The validator reads `docs/manifests/art-rescue-asset-manifest.json`.

Examples:

- Manifest schema errors are `ERROR`.
- A missing material on a `promotionStatus: promoted` asset is `ERROR`.
- A texture above the V1 warning threshold is `WARNING`.
- Missing Art Rescue scan roots are `INFO`.

Non-examples:

- A subjective `Premium target` visual review.
- A broad scan of every Unity asset in the project.
- An auto-repair tool that edits assets, materials, scenes, or the manifest.

### Art Rescue Asset Zones

Definition: The canonical Unity folder zones that separate raw, reviewable, and
production-promoted Art Rescue asset files.

Relationships:

- The canonical Unity project root is `unity/Echapee4D`.
- The manifest stores Unity-relative `Assets/...` paths.
- `Assets/Echappee/Art/Quarantine/...` holds raw or untrusted candidates.
- `Assets/Echappee/Art/Review/...` holds cleaned candidates that can be tested
  in controlled previews or candidate scenes.
- `Assets/Echappee/Art/Production/...` holds explicitly promoted assets only.
- MYB-144 should block visible production assets that bypass these zones or
  appear in `Production` without `Promotion Status` `promoted`.

Examples:

- `Assets/Echappee/Art/Quarantine/AI/tripo/myb_root_arch_a/`
- `Assets/Echappee/Art/Review/BlenderMCP/roots/myb_root_arch_a/`
- `Assets/Echappee/Art/Production/Forest/Roots/myb_root_arch_a/`

Non-examples:

- Importing a generated prop directly into a random scene-owned folder.
- Placing an unreviewed AI asset directly under `Production`.

### Promotion Status

Definition: The production-use status that describes whether an asset is still
outside production, under consideration as a scene candidate, or explicitly
promoted for controlled production use.

Relationships:

- `Promotion Status` is gated by manifest intake, validators, and visual review.
- `promoted` is stronger than `approved`: it means the asset has an accepted
  production use, not only valid provenance.
- MYB-143 uses `not_promoted`, `candidate`, and `promoted` as promotion
  statuses.
- `candidate` and `promoted` require `Intake Status` `approved`.
- `quarantine`, `review`, `rejected`, and `deprecated` assets must remain
  `not_promoted`.
- A visible asset cannot become `promoted` from manifest and technical validator
  evidence alone; it needs canonical route-camera evidence or a documented
  visual-surface exception.
- An invisible or purely technical asset may be promoted without route capture
  only when `visualImpact` is `none` and the exception is documented.

Examples:

- A cleaned Blender candidate can be `approved` for intake but remain
  `not_promoted` until it works in Unity validation and route-camera review.

Non-examples:

- Promoting an asset because it looks good in a Meshy, Tripo, Blender, or
  isolated preview.
- Promoting a visible prop because its provenance and import settings are clean
  while it has no route-camera evidence.

### Pipeline Asset Unity

Definition: The concrete preparation flow used before an asset can be adopted
or adjusted in `unity/Echapee4D`: provenance check, asset family classification,
technical bands, Unity import settings, folder convention, manifest traceability,
and validation evidence.

Relationships:

- `Pipeline Asset Unity` applies to both third-party free assets and custom or
  generated assets, but their provenance rules stay separate.
- It turns `Budget performance` and `Garde-fou performance` into asset-level
  decisions without replacing `Verdict Artistique V1`.
- It produces `Asset Optimise Unity` candidates for later scene or biome work.
- For a family of custom assets, sub-tickets should record asset-level pressure
  such as mesh, material, texture, and visual evidence, while the parent family
  ticket validates the integrated scene against the 60 fps target.

Examples:

- Classifying a horse as `acteur anime`, applying texture and triangle warning
  bands, recording its conversion path, and validating it before MYB-100 adjusts
  its Unity import settings.

Non-examples:

- Importing an entire asset pack because it is free.
- Treating a legal manifest approval as proof that an asset is visually premium.

### Asset Optimise Unity

Definition: An asset that has passed the `Pipeline Asset Unity` checks with an
`ok` result or a documented `warning`, and whose Unity import settings, source
metadata, folder placement, materials, prefabs, and validation evidence are
ready for controlled use in a macOS-first scene.

Relationships:

- `Asset Optimise Unity` can be an `Asset Tiers Approuve`, a custom Blender MCP
  asset, or a Meshy-generated asset.
- It remains separate from `Verdict Artistique V1`: an optimized asset can still
  be `support/fallback` rather than `adoptable-v1`.
- A `reject` technical band blocks optimization unless a ticket documents an
  explicit exception.

Examples:

- A CC0 prop with 1K textures, one shared material, a prefab under `Prefabs`,
  complete manifest metadata, and passing validation.

Non-examples:

- A raw source archive, a full downloaded pack, an unreviewed generated model,
  or a prefab with missing provenance.

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

### Famille Visuelle

Definition: A review grouping for assets or surface elements that read as the
same visual type in a scene, even when they have multiple instances, variants,
materials, or generated placements.

Relationships:

- A `Famille Visuelle` can include placed 3D props, vegetation, surfaces,
  materials, or repeated composition elements.
- It is reviewed with representative isolated and in-context captures when the
  team needs an art-direction decision without judging every scene instance
  individually.

Examples:

- Forest trees, undergrowth ribbons, road-edge rocks, forest floor material, or
  small accent props inside one Passage.

Non-examples:

- A single placed instance that only fails because of a one-off transform bug.
- A legal asset source or manifest entry; those belong to asset provenance.

### Verdict de Revue Asset

Definition: A short art-direction decision applied to a `Famille Visuelle` or
asset candidate after visible review evidence. The standard choices are
`supprimer`, `refaire/remplacer`, or `garder mais ajuster`.

Relationships:

- `Verdict de Revue Asset` refines `Verdict Artistique V1` for scene cleanup
  work where the team must decide what to do with already visible elements.
- `garder mais ajuster` must name concrete adjustment areas, such as material,
  color, scale, density, placement, grounding, or lighting integration.

Examples:

- A tree family marked `refaire/remplacer` because its silhouette reads too
  crude in the ride camera.
- A ground material marked `garder mais ajuster` because the texture is usable
  but the color and tiling break the forest palette.

Non-examples:

- A vague "looks fine" or "looks cheap" note without visible evidence.
- A performance-only verdict with no art-direction decision.

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
material choices, and a visible premium finish. Performance is a measured
constraint, not a visual target or a reason to make the world look crude.

Aliases: scenic fantasy lisible, premium scenic fantasy.

Relationships:

- `Stylise Premium` guides `Biome`, `Signal Fantasy Premium`, `Surface de
  Route`, and `Stylisé Premium de Production`.
- It favors adventure and wonder first, while keeping enough orientation for an
  automatic on-rail ride.

Examples:

- Clean stylized geometry with deliberate silhouettes, good lighting, rich
  materials, and one or more memorable premium signals.

Non-examples:

- Placeholder-looking simplified geometry, faceted objects with no composition,
  raw asset packs dropped into the scene, or cheap simplified meshes used as a
  final quality bar.

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

### Stylisé Premium de Production

Definition: The canonical visual direction for MyBike production art. It means
a stylized, readable, performant, and controlled image: strong silhouettes,
clear route, camera-first composition, coherent lighting/fog, credible
materials, and controlled density. The target is not cheaper-looking art.
Performance is handled through `Budget performance`, `Garde-fou performance`,
and geometry constraints that serve the visual direction instead of replacing
it.

Aliases: Stylise Premium de Production, production stylise premium, optimized
stylized production art.

Forbidden/legacy aliases: Socle Low-Poly de Production, Low-poly de
Production, production low-poly, low-poly premium.

Relationships:

- `Stylisé Premium de Production` sits under `Stylise Premium`.
- `Signal Fantasy Premium` elements may exceed this baseline when intentionally
  budgeted as hero moments.
- It is evaluated by perceived quality first, then by measured performance
  guardrails.
- `Low-poly` may remain only as a local technical qualifier for geometry budget
  or modeling method. It must never be used as the canonical art direction,
  final aesthetic target, or quality bar in project governance.

Examples:

- Stylized trees with strong silhouettes, layered forms, and good materials;
  forest ground surfaces with modeled relief and believable grounding; rocks or
  route edges that feel intentional from the ride camera.

Non-examples:

- Blockout meshes, flat unlit primitives, noisy decimated assets, low-detail
  props that feel empty, using low-poly as the final quality bar, or spending
  geometry without visible benefit or performance evidence.

### Lecture Prototype

Definition: A negative visual verdict for a scene or Passage that shows its
temporary construction instead of reading as an intentional place. It can come
from visible primitives, opportunistic prop placement, flat lighting, raw
materials, weak camera composition, missing grounding, or absent place identity.

Relationships:

- `Lecture Prototype` is the main rejection signal for `Stylise Premium de
  Production`.
- Each audit item must justify why it reads as `prototype`, `limite`, or
  `stylise premium de production` from visible capture or video evidence.
- A simplified scene does not automatically have `Lecture Prototype` if it still
  feels composed, lit, grounded, readable, and intentional.

Examples:

- A road segment with no foreground/midground/background structure, props
  dropped beside the road, no useful shadows, and flat material colors.
- A Passage where bloom, fog, or color grading tries to compensate for an empty
  or poorly staged scene.

Non-examples:

- Economical geometry with deliberate silhouettes, strong light direction,
  coherent material separation, anchored props, and a readable scenic identity.
- A sparse scene that still feels authored because the camera view is composed
  around depth, horizon, shadow shapes, and one memorable premium signal.

### Checkpoint Insuffisant

Definition: A documented visual checkpoint where an Art Rescue ticket produced
an observable improvement, comparable route and overview captures, and a filled
rubric, but did not reach `Premium target`.

Relationships:

- `Checkpoint Insuffisant` preserves useful evidence without lowering the
  `Stylisé Premium de Production` closure bar.
- It cannot close a visible Art Rescue ticket as `Done` unless Julien explicitly
  accepts a documented exception.
- It must lead to a targeted corrective sub-ticket, a documented exception, or
  rollback/rework when the change moves the scene in the wrong direction.

Examples:

- Silhouettes improve but still score below 4.
- Lighting is more coherent but the mood remains weak.
- Route readability improves while composition stays too flat.

Non-examples:

- A ticket that reaches `Premium target` and has human visual validation.
- A technical-only ticket that does not change visible rendering.
- A vague "better but not enough" note without captures, rubric, blocker, and
  next action.

### Surface Canonique de Validation Visuelle

Definition: The player-facing validation surface for Art Rescue production
quality: a route screenshot taken in the canonical ride corridor from the
canonical ride camera.

Relationships:

- `Surface Canonique de Validation Visuelle` is required to validate
  `Premium target` for visible Art Rescue work.
- The overview screenshot is required for context, density, and global
  coherence, but it is secondary. Overview explains; route decides.
- Preview scenes, Blender renders, Meshy/Tripo previews, quarantine scenes,
  turntables, and isolated asset screenshots are intermediate evidence only.
  They cannot close a production visual ticket.

Examples:

- A route-camera screenshot from `unity/Echapee4D` in the canonical ride
  corridor, with the same scene state also captured in overview.
- An integrated asset that improves the actual ride-camera composition,
  lighting, density, and scale in context.

Non-examples:

- A beautiful Blender turntable, Meshy/Tripo preview, isolated asset screenshot,
  sandbox scene, quarantine scene, hero shot, or overview-only capture.
- An asset that looks good alone but reads wrong at ride-camera distance,
  speed, scale, lighting, or density.

### Visual Checkpoint Evidence

Definition: Comparable capture evidence produced by the MYB-145 workflow for
Art Rescue visual reviews.

Canonical path:

- `_bmad-output/visual-checkpoints/<ticket-id>/`

Relationships:

- `Visual Checkpoint Evidence` contains route screenshots, overview screenshots,
  before/after sheets, capture reports, and capture metadata JSON.
- It is the standard output location for MYB-145 and future MYB-147+ visual
  tickets.
- `_bmad-output/implementation-artifacts/` remains valid for general
  implementation artifacts, imported docs, ticket notes, and historical reports.
- Visual checkpoint evidence provides comparable proof for review. It does not
  by itself declare `Premium target`.

Examples:

- `_bmad-output/visual-checkpoints/MYB-150/2026-06-16-after-route.png`
- `_bmad-output/visual-checkpoints/MYB-150/2026-06-16-route-before-after.png`
- `_bmad-output/visual-checkpoints/MYB-150/2026-06-16-capture-report.md`
- `_bmad-output/visual-checkpoints/MYB-150/2026-06-16-capture-metadata.json`

Non-examples:

- Imported Art Rescue source docs under `_bmad-output/implementation-artifacts/`.
- Unity batch logs under `_bmad-output/unity-test-results/`.
- A screenshot without scene, camera, state, or baseline metadata.

### Ambition Visuelle Mesurée

Definition: The Art Rescue arbitration rule that MyBike aims for `Stylisé
Premium de Production` first, then measures performance. Performance is a
guardrail, not an aesthetic target.

Relationships:

- `Ambition Visuelle Mesurée` allows richer geometry, materials, density, and
  composition when the gain is visible from the `Surface Canonique de Validation
  Visuelle`.
- The default Art Rescue target is 60 FPS unless a ticket defines another
  target.
- If FPS falls below target or a blocking validator `ERROR` appears, reduce
  cost in this order: unnecessary collisions, oversized textures/material
  excess, density outside the route camera, LOD/culling/distance, non-essential
  near-route density, premium route-camera-visible elements last.
- No visible premium downgrade is allowed without measurement.

Examples:

- Spending more triangles on near-route tree silhouettes that improve the route
  capture while the scene still reaches 60 FPS.
- Removing decorative colliders or off-camera scatter before simplifying a
  validated hero silhouette.

Non-examples:

- Optimizing the scene into sparse, cheap visuals because a budget exists.
- Adding expensive detail only visible in editor overview or an isolated asset
  preview.
- Downgrading route-camera-visible premium elements without FPS or validator
  evidence.

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

### Passage

Definition: A short pre-ride label for a notable step of the balade, such as
the warmup, the main climb, or the short sprint.

Relationships:

- A `Passage` can reference `Route Difficulty Cue`, `Biome`, or effort pacing,
  but it remains descriptive rather than a quest objective.
- A `Fiche Route` should surface at most three `Passage` entries for the MVP.

Aliases: etape de la balade, old name: moment cle, route highlight, key moment.

### Vue Cible

Definition: A playable visual reference frame inside a specific `Passage` used
to judge whether local composition, lighting, atmosphere, material hierarchy,
and scenic identity escape `Lecture Prototype` and reach the intended
`Stylise Premium` direction.

Relationships:

- A `Vue Cible` is local lookdev evidence, not a project-wide rendering
  decision.
- It can guide local art passes for composition, lights, probes, fog, palette,
  and asset hierarchy inside one `Passage`.
- It remains separate from `Socle de Rendu Projet`, which owns global Unity
  rendering defaults.

Examples:

- A dramatic stylized premium undergrowth view in Passage 01 with cool dense
  sides, warm low-angle light on the ride axis, subtle haze, readable road, and
  stronger foreground/midground/background separation.

Non-examples:

- A global URP preset, a marketing screenshot detached from the playable ride,
  or a post-process-only mask over an unchanged prototype composition.

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

### Socle de Rendu Projet

Definition: The project-level Unity rendering foundation that influences the
whole active vertical slice before any Passage-specific polish. It covers global
rendering defaults, quality tier choices, renderer features, lighting baseline,
probe strategy, and post-process baseline.

Aliases: project rendering foundation, global rendering baseline, URP defaults.

Relationships:

- `Socle de Rendu Projet` supports `Stylise Premium` and `Stylise Premium de
  Production` by making the whole Unity scene read more intentional.
- It remains separate from Passage composition, asset placement, and local
  scene dressing.
- It is evaluated with `Garde-fou performance`; a better look is not enough if
  it creates unclear performance or readability risk.

Examples:

- A project-wide rendering decision that changes shadow quality, renderer
  features, ambient lighting, probe use, or the default post-process baseline.

Non-examples:

- Repositioning trees for one Passage.
- Hiding a weak scene behind heavy bloom, fog, vignette, or color grading.

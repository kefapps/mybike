# MYB-88 - Unity PR Checklist

Status: implemented
Linear: https://linear.app/kefjbo/issue/MYB-88/myb-079-ajouter-une-checklist-pr-pour-assets-et-features-unity
Branch: `myb-88-unity-pr-checklist`

## Goal

Add a compact pull request checklist that protects the Unity macOS-first
direction without turning every PR into a release process.

## Grill-with-docs decisions

- Primary artifact: `.github/PULL_REQUEST_TEMPLATE.md`.
- Use a single general PR template with a conditional Unity macOS-first block.
- Allow `N/A` with a short reason.
- Include Linear/tracking, scope, Unity platform impact, asset provenance,
  performance/budget impact, validation, and repo hygiene.
- Third-party assets without complete provenance are not adoptable/buildable;
  they must stay `needs-review` and out of active scenes/builds.
- Locally generated assets need generation provenance plus performance/budget
  notes.
- Performance budget language stays generic and does not depend on MYB-51 being
  merged first.
- Explicitly check Unity/PackageManager/ProjectSettings generated changes so
  accidental package bumps or serialization drift do not slip into PRs.

## Implementation

- Added `.github/PULL_REQUEST_TEMPLATE.md`.
- The template covers:
  - issue/scope/out-of-scope;
  - Linear and sync expectations;
  - Unity macOS-first guardrails;
  - third-party and generated asset provenance;
  - performance and validation evidence;
  - generated Unity file review;
  - repo hygiene around `src/**` and `unity/Echappee3D/**`.

## Validation

- `git diff --check`: PASS.
- Targeted secret/path scan: PASS; only historical `anti-secret scan` text in
  sync artifacts matched.

## Scope notes

- No Unity scene, runtime, asset, package, Project Settings, or WebGL changes.
- No release-process automation or CI changes.

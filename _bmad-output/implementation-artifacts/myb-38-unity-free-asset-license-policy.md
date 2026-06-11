# MYB-38 - Politique de licences pour assets Unity gratuits

Status: Done
Branch: `myb-38-asset-license-policy`
Date: 2026-06-11

## Scope

MYB-38 defines the policy for free third-party resources used by the active
Unity project `unity/Echapee4D`. It does not import real assets, buy assets, or
retroactively audit every existing Unity/tooling dependency.

## Decisions

- Accept free assets by default only under `CC0`, `Public Domain`, or `CC-BY`
  when the source is verified and metadata is complete.
- Accept `MIT`, `Apache-2.0`, and `OFL` only for the technical content types
  they fit: code, tooling, shader code, packages, or fonts.
- Refuse by default `NC`, `ND`, `SA`, ambiguous/custom marketplace terms,
  reuploads, assets with no explicit license, unclear AI-generated packs,
  extracted IP, and assets that copy MYB-37 inspirations too directly.
- Use `approved`, `needs-review`, and `rejected` status values.
- Block `needs-review` assets from Unity import and WebGL builds.
- Require any exception to cite reason, validator, date, and Linear issue.
- Store attribution in `THIRD_PARTY_ASSETS.md` and the Unity manifest; expose it
  in the build when a Credits surface exists, not in the ride HUD.

## Local Artifacts

- `THIRD_PARTY_ASSETS.md`
- `CONTEXT.md`
- `AGENTS.md`
- `unity/Echapee4D/Assets/Echappee/Art/ThirdPartyAssets.assetmanifest.json`
- `_bmad-output/implementation-artifacts/sprint-status.yaml`
- `_bmad-output/linear-sync.md`

## Validation

- `jq empty unity/Echapee4D/Assets/Echappee/Art/ThirdPartyAssets.assetmanifest.json`: PASS
- `git diff --check`: PASS
- `unity-mcp-cli status unity/Echapee4D --timeout 10000`: PASS
- `_bmad-output/implementation-artifacts/sprint-status.yaml` YAML parse: not
  rerun because the local runner blocks Ruby inline and stdin script modes

## Notes

MYB-38 was moved to `Done` in Linear on 2026-06-11 with closure comment
`b2f34fda-9921-4cf1-9517-14d951196b02`. The cleanup flow commits the branch in
logical lots and merges it back to `main` as the completion step.

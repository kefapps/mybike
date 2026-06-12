## Summary

- Linear issue: MYB-
- Scope:
- Out of scope:

## Tracking

- [ ] Branch starts with the Linear issue ID, or this PR explains why not.
- [ ] Linear status and `_bmad-output/linear-sync.md` will be kept aligned before merge.
- [ ] Merge/Done sync plan is clear, or this PR is explicitly not a ticket completion.

## Unity macOS-first

Mark `N/A` with a short reason when a row does not apply.

- [ ] Unity runtime changes stay in `unity/Echapee4D/**`.
- [ ] macOS is treated as the priority target.
- [ ] WebGL proof/build is included only if the Linear issue explicitly asks for it.
- [ ] Mock mode remains playable; no real-bike hardware dependency was added.
- [ ] `src/**` was not changed unless the issue explicitly owns parked web prototype work.
- [ ] `unity/Echappee3D/**` was not recreated.

## Assets And Provenance

- [ ] Third-party assets have source URL, author, exact asset/pack name, license, license URL or proof, retrieval date, local path, and attribution decision.
- [ ] Third-party assets without complete provenance are marked `needs-review` and are excluded from active scenes/builds.
- [ ] Generated assets have generation provenance, source/intermediate handling notes, and performance/budget impact noted.
- [ ] New archives, raw downloads, generated intermediates, or local tool outputs are excluded unless the ticket explicitly requires them.
- [ ] `THIRD_PARTY_ASSETS.md` and the Unity asset manifest were updated when third-party asset attribution changed.

## Performance And Validation

- [ ] Performance/budget impact is described, or marked `N/A`.
- [ ] Unity-generated changes in `Packages/**`, `ProjectSettings/**`, scenes, prefabs, materials, and `.meta` files were reviewed and only intentional changes remain.
- [ ] `unity-mcp-cli status unity/Echapee4D --timeout 10000` ran, or `N/A` is explained.
- [ ] A targeted Unity validator ran when scene, asset, HUD, gameplay, or build settings changed.
- [ ] `MYB91CanonicalBaselineValidator` ran when the canonical scene, Build Settings, packages, Project Settings, or Unity baseline assumptions changed.
- [ ] `git diff --check` passed.
- [ ] Targeted secret/path scan passed, or `N/A` is explained.

## Review Notes

- Risks / caveats:
- Follow-up tickets:
- Evidence files:

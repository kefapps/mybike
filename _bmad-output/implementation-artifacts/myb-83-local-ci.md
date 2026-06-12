# MYB-83 Local CI Unity macOS-First

## Decision

MYB-83 implements a local, reproducible CI-style validation for the canonical
Unity project. The ticket is explicitly not a GitHub Actions or hosted-runner
ticket.

## Scope Implemented

- Added `npm run validate:local-ci`.
- Added `scripts/validate-local-ci.mjs`.
- Added focused tests in `scripts/validate-local-ci.test.mjs`.
- Documented the command in `unity/Echapee4D/README.md`.
- The local CI report is written to
  `_bmad-output/unity-test-results/myb-83-local-ci.txt`.

## Checks

The local CI command covers:

- required canonical Unity paths under `unity/Echapee4D`;
- absence of the legacy `unity/Echappee3D` project;
- generated Unity folders remain untracked and show no untracked drift in the
  validation path;
- JSON parsing for `Packages/manifest.json` and `Packages/packages-lock.json`;
- working tree, staged, and branch-diff whitespace checks;
- drift checks for `Packages/` and `ProjectSettings/`;
- Unity-MCP reachability;
- `MYB91CanonicalBaselineValidator.ValidateCanonicalBaselineCli()`.

## Out Of Scope

- GitHub Actions workflows;
- hosted macOS runners;
- CI secrets or billing configuration;
- full Unity build automation;
- WebGL/browser proof unless a future ticket explicitly asks for it.

## Validation

```bash
npm run test -- scripts/validate-local-ci.test.mjs
npm run validate:local-ci
git diff --check
```

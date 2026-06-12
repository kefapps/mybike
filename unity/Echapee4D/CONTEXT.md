# Context: Unity Ride Runtime

## Purpose

This context owns the shared gameplay language for the Unity mock ride runtime.
It keeps mock effort, route difficulty, and future trainer resistance language
separate without depending on connected-bike hardware.

## Terms

### Demande de Resistance

Definition: The resistance target requested by route difficulty or ride logic.
It expresses what the experience asks for, before any controller or trainer
accepts, clamps, ignores, or applies it.

Aliases: resistance cible, target resistance.

Relationships:

- `Demande de Resistance` can become `Resistance Appliquee` when the local
  runtime accepts it.
- `Demande de Resistance` is distinct from `Resistance Mesuree`, which is read
  from a trainer or simulated trainer sample.

Examples:

- A climb segment asks for a high `Demande de Resistance`.
- A recovery segment asks for a low `Demande de Resistance`.

Non-examples:

- The rider's displayed effort.
- A guarantee that real hardware changed resistance.

### Resistance Appliquee

Definition: The latest resistance value accepted by the runtime controller for
the current ride context. In mock mode, this can be simulated locally.

Aliases: applied resistance.

Relationships:

- `Resistance Appliquee` follows `Demande de Resistance` when the controller can
  accept the request.
- `Resistance Appliquee` can differ from `Demande de Resistance` if a controller
  clamps, delays, rejects, or falls back.

Examples:

- The mock runtime accepts a requested resistance of 66 and reports 66 as the
  applied resistance.

Non-examples:

- Proof that a physical trainer accepted the command.
- The measured resistance from a trainer telemetry sample.

### Resistance Mesuree

Definition: Resistance reported by a trainer or simulated trainer sample.

Aliases: measured resistance, trainer resistance.

Relationships:

- `Resistance Mesuree` is evidence from a source, not a command.
- `Resistance Mesuree` may be absent while `Demande de Resistance` and
  `Resistance Appliquee` still exist in mock mode.

Examples:

- A trainer sample reports cadence plus measured resistance.

Non-examples:

- The resistance target requested by route difficulty.
- The value most recently accepted by the runtime controller.

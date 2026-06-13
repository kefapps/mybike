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

### Trainer ReadOnly

Definition: A trainer mode where MyBike reads trainer telemetry without writing
resistance commands to the device.

Aliases: read-only trainer, readOnly.

Relationships:

- `Trainer ReadOnly` may provide `Resistance Mesuree`, cadence, power, speed,
  or elapsed time.
- `Demande de Resistance` can still be calculated locally while the device stays
  read-only.
- Losing telemetry during a `Trainer ReadOnly` session pauses the active ride
  instead of silently falling back to mock movement.
- After that pause, the ride may resume through reconnection or through an
  explicit user choice to continue without a trainer.
- `Trainer ReadOnly` is distinct from controllable trainer behavior, where a
  runtime boundary may later attempt to send resistance changes to hardware.

Examples:

- A trainer or ESP32 proof reports cadence and power while MyBike keeps route
  resistance as local demand only.
- A telemetry loss pauses the ride until the source reconnects or the user
  explicitly switches to no-trainer fallback.

Non-examples:

- Proof that a physical trainer accepted a resistance command.
- A mode that keeps advancing a trainer-backed ride after telemetry disappears.

### Trainer Telemetry Source

Definition: A source that provides trainer-like telemetry to the ride runtime.

Relationships:

- `Trainer Telemetry Source` can feed cadence, power, speed, elapsed time, and
  `Resistance Mesuree` into the effort model.
- It is separate from the resistance controller boundary, which owns
  `Demande de Resistance` application or fallback.
- A `Trainer ReadOnly` setup uses a `Trainer Telemetry Source` without sending
  resistance commands back to hardware.

Examples:

- An ESP32 proof that emits cadence and power for the Unity ride runtime.
- A cadence-first proof that emits cadence and elapsed time while power or speed
  are estimated locally.
- A future CoreBluetooth/FTMS adapter that reads Indoor Bike Data.

Non-examples:

- A component whose main job is to apply a requested resistance.
- A visual HUD label with no telemetry source behind it.

### Trainer Controllable

Definition: A trainer mode where MyBike may attempt to send resistance commands
to hardware through a controller boundary.

Aliases: controllable trainer, controllable.

Relationships:

- `Trainer Controllable` starts from `Demande de Resistance`.
- A future hardware adapter may translate that demand into trainer commands and
  report whether the request was applied, delayed, rejected, or unavailable.
- It is distinct from `Trainer ReadOnly`, which only reads telemetry.

Examples:

- A future FTMS proof attempts to send a resistance command and records the
  controller status.

Non-examples:

- Reading cadence, power, or elapsed time without writing to the trainer.
- Assuming that a physical trainer applied resistance because MyBike requested
  it.

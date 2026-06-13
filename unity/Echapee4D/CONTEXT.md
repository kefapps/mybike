# Context: Unity Ride Runtime

## Purpose

This context owns the shared gameplay language for the Unity mock ride runtime.
It keeps mock effort, route difficulty, and future trainer resistance language
separate without depending on connected-bike hardware.

## Terms

### Cadence HUD

Definition: The frequency at which visible HUD text is refreshed during the
ride. It is a readability concern, not the simulation tick.

Relationships:

- `Cadence HUD` can be slower than the ride loop, camera pose, effort
  simulation, and resistance controller updates.
- It protects the HUD from visual noise while preserving responsive ride feel.

Examples:

- Updating distance and speed text four times per second while refreshing
  segment and resistance status twice per second.

Non-examples:

- Slowing the rider trajectory, camera motion, or resistance calculations.
- A full HUD redesign or native macOS accessibility layer.

### Trajectoire de Ride

Definition: The continuous path followed by the rider and camera during the
mock ride. It describes navigation and view comfort, not only the visible road
mesh.

Aliases: ride trajectory, camera/rider path.

Relationships:

- `Trajectoire de Ride` should stay aligned with the visible `Surface de Route`
  and route difficulty cues.
- It can be smoothed from route control points, but it should remain close
  enough to preserve route readability and HUD/effort coherence.

Examples:

- A gently anticipated curve where the rider and camera rotate progressively
  before the visible bend.

Non-examples:

- Decorative roadside placement.
- A disconnected cinematic camera path that no longer feels like cycling on the
  route.

### Virage Lisible

Definition: A curved route passage where trajectory, camera orientation,
visible road surface, and route-bound cues remain comfortable and
understandable.

Aliases: readable turn, natural curve.

Relationships:

- A `Virage Lisible` depends on both `Trajectoire de Ride` and `Surface de
  Route`.
- It favors bounded smoothing over perfect spline fluidity when stronger
  smoothing would desync road visuals, cues, HUD progression, or rider comfort.

Examples:

- A stylized bend with rounded road edges, aligned center dashes, and a rider
  view that anticipates the curve without sudden snapping.

Non-examples:

- A sharp segment-to-segment camera snap.
- A smooth camera line that cuts across the road or misses difficulty cues.

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

# ADR 0001: Resistance Controller Boundary

Date: 2026-06-12
Status: Accepted

## Context

MYB-57 added a mock effort and difficulty simulator for the Unity ride runtime.
It converts route segment and grade into a resistance demand, derives effort,
fatigue, and speed, and keeps the MVP playable without connected-bike hardware.

MYB-59 introduces a `ResistanceController` abstraction so the runtime can
separate the request for resistance from whatever applies, simulates, rejects,
or later forwards that request to trainer hardware. BLE, FTMS, CoreBluetooth,
and real trainer control remain outside MYB-59.

## Decision

`ResistanceController` will be an autonomous Unity runtime boundary, planned
under `Assets/MYB59/Runtime`, not an extension of the MYB-57 simulator.

MYB-57 remains responsible for calculating the resistance demand, mock effort,
fatigue, and speed. The resistance controller receives that demand and exposes a
snapshot containing applied resistance and controller status.

The contract uses normalized resistance `0..1` as the canonical internal value
and exposes a derived `0..100` level for HUD display and future trainer mapping.

The controller must support mock/fallback behavior so the ride loop remains
playable when no hardware controller is available. A missing or unavailable
controller is visible in status and validation evidence, not a runtime blocker
for the MVP mock ride.

## Consequences

The Unity mock ride keeps a clear boundary between route difficulty, resistance
application, and future hardware integration.

Future hardware work can add an adapter behind the same boundary without
changing the MYB-57 effort equations first.

MYB-59 should not change speed or fatigue equations. It should provide a pure
controller validator plus scene or probe evidence that resistance demand reaches
the controller and applied resistance/status are observable.

The implementation adds some plumbing between MYB-57 and the current probe ride,
but that plumbing prevents the HUD, simulator, and future trainer adapter from
sharing one overloaded meaning of resistance.

## Alternatives considered

- Extend MYB-57 directly -- rejected because it would mix demand calculation,
  effort simulation, and controller execution in one module.
- Promise immediate hardware application -- rejected because future FTMS or
  trainer control may reject, delay, or fail to confirm resistance changes.
- Feed applied resistance back into speed and fatigue during MYB-59 -- rejected
  because MYB-59 is an abstraction boundary, not a gameplay-model refactor.

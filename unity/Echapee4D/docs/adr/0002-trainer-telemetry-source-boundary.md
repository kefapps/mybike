# ADR 0002: Trainer Telemetry Source Boundary

Date: 2026-06-13
Status: Accepted

## Context

MYB-57 introduced an FTMS-friendly effort model with trainer modes
`Simulated`, `ReadOnly`, and `Controllable`, while only implementing simulated
inputs. MYB-59 then separated resistance demand from resistance application
through the `ResistanceController` boundary. MYB-60 added resistance smoothing
before that controller, and MYB-64 preserved a no-trainer fallback.

MYB-97 prepares an ESP32-WROOM proof for trainer modes beyond the mock
simulator. The proof must keep the MVP playable without hardware, avoid a full
Unity BLE/CoreBluetooth implementation, and avoid promising trainer control
before a real-device write path is proven.

## Decision

Trainer telemetry reading will use a separate `TrainerTelemetrySource`
boundary. It is responsible for providing trainer-like telemetry such as
cadence, power, speed, elapsed time, and measured resistance when available.

`TrainerTelemetrySource` is separate from `ResistanceController`.
`ResistanceController` remains responsible for applying, falling back, or later
forwarding `Demande de Resistance`.

MYB-97 will prioritize an ESP32-WROOM read-only proof that is cadence-first:
cadence and elapsed time are enough for the minimum proof, while power and speed
may be estimated locally and measured resistance may be absent.

If telemetry is lost during a trainer-backed read-only session, the active ride
will pause instead of silently advancing through mock or manual fallback data.
The ride may resume after reconnection, or the user may explicitly choose to
continue without a trainer through the manual fallback path. Mock and manual
fallback remain explicit user choices.

`Trainer Controllable` remains a future mode. MYB-97 may document the command
mapping path from `Demande de Resistance` to future trainer commands, but it
does not require BLE writes, Control Point handling, or proof that physical
hardware accepted a resistance command.

## Consequences

The Unity ride runtime keeps a clean separation between telemetry ingestion,
effort calculation, resistance mapping, and resistance application.

Future CoreBluetooth/FTMS work can add a telemetry adapter without changing the
MYB-59 controller boundary first.

The ESP32 proof can validate the data channel and pause behavior without
depending on a full smart trainer.

Telemetry loss handling must make the source transition visible: reconnecting
keeps the session in `Trainer ReadOnly`, while continuing without a trainer
switches the session to the manual fallback path by explicit user action.

Future controllable trainer work must explicitly prove write behavior,
acknowledgement or rejection, fallback status, and macOS permission behavior in
a dedicated ticket.

## Alternatives considered

- Put ESP32 or FTMS reads inside `ResistanceController` -- rejected because it
  mixes measured telemetry with resistance application.
- Require power and measured resistance for MYB-97 -- rejected because it turns
  a bounded spike into a sensor and firmware project.
- Prove controllable trainer writes in MYB-97 -- rejected because BLE write
  behavior, FTMS Control Point handling, and real hardware acceptance need a
  dedicated proof ticket.

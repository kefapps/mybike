import { describe, expect, it } from "vitest";

import { withZenMode } from "../preferences";
import {
  createInitialSessionState,
  sessionReducer
} from "./sessionReducer";

describe("sessionReducer", () => {
  it("transitions idle -> running -> paused -> running -> finished", () => {
    const idle = createInitialSessionState();
    const running = sessionReducer(idle, { type: "start", now: 1000 });
    const paused = sessionReducer(running, { type: "pause" });
    const resumed = sessionReducer(paused, { type: "resume" });
    const finished = sessionReducer(resumed, { type: "finish", now: 5000 });

    expect(running.phase).toBe("running");
    expect(running.startedAt).toBe(1000);
    expect(paused).toMatchObject({ phase: "paused", startedAt: 1000 });
    expect(resumed).toMatchObject({ phase: "running", startedAt: 1000 });
    expect(finished).toMatchObject({
      phase: "finished",
      startedAt: 1000,
      finishedAt: 5000
    });
  });

  it("transitions running -> finished", () => {
    const running = sessionReducer(createInitialSessionState(), {
      type: "start",
      now: 1000
    });

    expect(sessionReducer(running, { type: "finish", now: 4000 })).toMatchObject({
      phase: "finished",
      finishedAt: 4000
    });
  });

  it("transitions paused -> finished", () => {
    const running = sessionReducer(createInitialSessionState(), {
      type: "start",
      now: 1000
    });
    const paused = sessionReducer(running, { type: "pause" });

    expect(sessionReducer(paused, { type: "finish", now: 4000 })).toMatchObject({
      phase: "finished",
      startedAt: 1000,
      finishedAt: 4000
    });
  });

  it("transitions running -> error", () => {
    const running = sessionReducer(createInitialSessionState(), {
      type: "start",
      now: 1000
    });

    expect(
      sessionReducer(running, {
        type: "fail",
        message: "WebGL indisponible"
      })
    ).toMatchObject({
      phase: "error",
      errorMessage: "WebGL indisponible"
    });
  });

  it("resets to idle while keeping the current preferences", () => {
    const initial = createInitialSessionState();
    const zen = sessionReducer(initial, { type: "setZenMode", zenMode: true });
    const running = sessionReducer(zen, { type: "start", now: 1000 });
    const failed = sessionReducer(running, {
      type: "fail",
      message: "WebGL indisponible"
    });

    const reset = sessionReducer(failed, { type: "reset" });

    expect(reset.phase).toBe("idle");
    expect(reset.preferences).toEqual({ zenMode: true });
    expect(reset.preferences).not.toBe(initial.preferences);
  });

  it("updates zen mode preferences without changing phase", () => {
    const initial = createInitialSessionState();

    const zen = sessionReducer(initial, { type: "setZenMode", zenMode: true });
    const off = sessionReducer(zen, { type: "setZenMode", zenMode: false });

    expect(zen.phase).toBe("idle");
    expect(zen.preferences).toEqual({ zenMode: true });
    expect(off.preferences).toEqual({ zenMode: false });
    expect(off.phase).toBe("idle");
  });

  it("ignores non-boolean zen mode payloads", () => {
    const initial = createInitialSessionState();

    const next = sessionReducer(initial, {
      type: "setZenMode",
      zenMode: "yes" as unknown as boolean
    });

    expect(next.preferences).toEqual({ zenMode: false });
  });

  it("preserves custom initial preferences", () => {
    const custom = withZenMode(
      { zenMode: false },
      true
    );
    const initial = createInitialSessionState(custom);

    expect(initial.preferences).toEqual({ zenMode: true });
  });

  it("keeps incoherent transitions deterministic", () => {
    const idle = createInitialSessionState();

    expect(sessionReducer(idle, { type: "pause" })).toEqual(idle);
    expect(sessionReducer(idle, { type: "resume" })).toEqual(idle);
    expect(sessionReducer(idle, { type: "finish", now: 2000 })).toEqual(idle);
  });
});

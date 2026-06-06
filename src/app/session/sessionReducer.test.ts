import { describe, expect, it } from "vitest";

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

  it("resets any phase to idle", () => {
    const running = sessionReducer(createInitialSessionState(), {
      type: "start",
      now: 1000
    });
    const failed = sessionReducer(running, {
      type: "fail",
      message: "WebGL indisponible"
    });

    expect(sessionReducer(failed, { type: "reset" })).toEqual(
      createInitialSessionState()
    );
  });

  it("keeps incoherent transitions deterministic", () => {
    const idle = createInitialSessionState();

    expect(sessionReducer(idle, { type: "pause" })).toEqual(idle);
    expect(sessionReducer(idle, { type: "resume" })).toEqual(idle);
    expect(sessionReducer(idle, { type: "finish", now: 2000 })).toEqual(idle);
  });
});

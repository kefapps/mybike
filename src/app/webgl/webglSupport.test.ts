import { describe, expect, it } from "vitest";

import { isWebGlAvailable, probeWebGlAvailability } from "./webglSupport";

describe("isWebGlAvailable", () => {
  it("returns true when WebGL2 is available", () => {
    expect(
      isWebGlAvailable(() => ({
        getContext: (contextId) => (contextId === "webgl2" ? {} : null)
      }))
    ).toBe(true);
  });

  it("returns true when only WebGL1 is available", () => {
    expect(
      isWebGlAvailable(() => ({
        getContext: (contextId) => (contextId === "webgl" ? {} : null)
      }))
    ).toBe(true);
  });

  it("returns false when no WebGL context is available", () => {
    expect(
      isWebGlAvailable(() => ({
        getContext: () => null
      }))
    ).toBe(false);
  });

  it("returns false when canvas creation fails", () => {
    expect(
      isWebGlAvailable(() => {
        throw new Error("canvas blocked");
      })
    ).toBe(false);
  });

  it("returns webgl2 when available", () => {
    const result = probeWebGlAvailability(() => ({
      getContext: (contextId) => (contextId === "webgl2" ? {} : null)
    }));

    expect(result.available).toBe(true);
    expect(result.contextId).toBe("webgl2");
    expect(result.reason).toBeUndefined();
  });

  it("falls back to webgl when webgl2 is unavailable", () => {
    const result = probeWebGlAvailability(() => ({
      getContext: (contextId) => (contextId === "webgl" ? {} : null)
    }));

    expect(result.available).toBe(true);
    expect(result.contextId).toBe("webgl");
  });

  it("returns a reason when all webgl contexts are unavailable", () => {
    const result = probeWebGlAvailability(() => ({
      getContext: () => null
    }));

    expect(result.available).toBe(false);
    expect(result.contextId).toBeUndefined();
    expect(result.reason).toBe(
      "Aucun contexte WebGL (webgl2/webgl/experimental-webgl) n'est disponible."
    );
  });

  it("returns a reason when canvas creation fails", () => {
    const result = probeWebGlAvailability(() => {
      throw new Error("blocked");
    });

    expect(result.available).toBe(false);
    expect(result.reason).toBe("La creation du canvas WebGL a echoue.");
  });
});

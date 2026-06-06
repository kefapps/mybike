import { describe, expect, it } from "vitest";

import { isWebGlAvailable } from "./webglSupport";

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
});

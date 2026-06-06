import { describe, expect, it } from "vitest";

import { createMockRideInputSource } from "./mockRideInputSource";

describe("MockRideInputSource", () => {
  it("returns deterministic mock samples for the provided timestamp", () => {
    const source = createMockRideInputSource(0.25);

    expect(source.kind).toBe("mock");
    expect(source.getEffort01()).toBe(0.25);
    expect(source.read(1234)).toEqual({
      source: "mock",
      effort01: 0.25,
      nowMs: 1234
    });
    expect(source.read(1234)).toEqual(source.read(1234));
  });

  it("clamps effort on writes and reads", () => {
    const source = createMockRideInputSource(-4);

    expect(source.read(100).effort01).toBe(0);

    source.setEffort01(2);
    expect(source.read(200).effort01).toBe(1);

    source.setEffort01(Number.NaN);
    expect(source.read(300).effort01).toBe(0);

    source.setEffort01(Number.POSITIVE_INFINITY);
    expect(source.read(400).effort01).toBe(1);
  });
});

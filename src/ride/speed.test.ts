import { describe, expect, it } from "vitest";

import type { RideInputSample } from "./rideTypes";
import { mapMockInputToSpeed, smoothSpeed } from "./speed";

function sample(effort01: number): RideInputSample {
  return { source: "mock", effort01, nowMs: 0 };
}

describe("mapMockInputToSpeed", () => {
  const config = { minSpeedMps: 1.5, maxSpeedMps: 9 };

  it("maps effort to configured min, max, and midpoint speeds", () => {
    expect(mapMockInputToSpeed(sample(0), config)).toBe(1.5);
    expect(mapMockInputToSpeed(sample(1), config)).toBe(9);
    expect(mapMockInputToSpeed(sample(0.5), config)).toBe(5.25);
  });

  it("clamps effort outside 0..1 without invalid or negative speeds", () => {
    expect(mapMockInputToSpeed(sample(-2), config)).toBe(1.5);
    expect(mapMockInputToSpeed(sample(4), config)).toBe(9);
    expect(mapMockInputToSpeed(sample(Number.NaN), config)).toBe(1.5);
  });
});

describe("smoothSpeed", () => {
  const config = {
    accelerationPerSecond: 2.5,
    decelerationPerSecond: 4
  };

  it("accelerates progressively without overshooting target speed", () => {
    expect(smoothSpeed(8, { speedMps: 2 }, 1, config)).toEqual({
      speedMps: 4.5
    });
    expect(smoothSpeed(8, { speedMps: 2 }, 10, config)).toEqual({
      speedMps: 8
    });
  });

  it("decelerates progressively without overshooting target speed", () => {
    expect(smoothSpeed(1, { speedMps: 9 }, 1, config)).toEqual({
      speedMps: 5
    });
    expect(smoothSpeed(1, { speedMps: 9 }, 10, config)).toEqual({
      speedMps: 1
    });
  });

  it("keeps speed stable for non-positive dt or invalid smoothing config", () => {
    expect(smoothSpeed(8, { speedMps: 2 }, 0, config)).toEqual({
      speedMps: 2
    });
    expect(
      smoothSpeed(8, { speedMps: 2 }, 1, {
        accelerationPerSecond: -1,
        decelerationPerSecond: -1
      })
    ).toEqual({ speedMps: 2 });
  });
});

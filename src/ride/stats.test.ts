import { describe, expect, it } from "vitest";

import { calculateRideStats } from "./stats";

describe("calculateRideStats", () => {
  it("returns elapsed, distance, and average speed in meters per second", () => {
    expect(calculateRideStats(10_000, 50)).toEqual({
      elapsedMs: 10_000,
      distanceMeters: 50,
      averageSpeedMps: 5
    });
  });

  it("keeps zero or invalid inputs coherent", () => {
    expect(calculateRideStats(0, 50)).toEqual({
      elapsedMs: 0,
      distanceMeters: 50,
      averageSpeedMps: 0
    });
    expect(calculateRideStats(-1, -5)).toEqual({
      elapsedMs: 0,
      distanceMeters: 0,
      averageSpeedMps: 0
    });
  });
});

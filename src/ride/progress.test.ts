import { describe, expect, it } from "vitest";

import { advanceRouteProgress, createInitialRideProgress } from "./progress";

describe("advanceRouteProgress", () => {
  it("integrates speed over dt into bounded distance and progress", () => {
    const progress = advanceRouteProgress(
      createInitialRideProgress(),
      10,
      2,
      { totalDistanceMeters: 100 }
    );

    expect(progress).toEqual({
      distanceMeters: 20,
      progress01: 0.2,
      completed: false
    });
  });

  it("clamps at the route length and marks completion exactly at the end", () => {
    const progress = advanceRouteProgress(
      { distanceMeters: 95, progress01: 0.95, completed: false },
      10,
      1,
      { totalDistanceMeters: 100 }
    );

    expect(progress).toEqual({
      distanceMeters: 100,
      progress01: 1,
      completed: true
    });
  });

  it("does not advance for invalid speed or dt", () => {
    const previous = { distanceMeters: 10, progress01: 0.1, completed: false };

    expect(
      advanceRouteProgress(previous, Number.NaN, 1, {
        totalDistanceMeters: 100
      })
    ).toEqual(previous);
    expect(
      advanceRouteProgress(previous, 10, -1, { totalDistanceMeters: 100 })
    ).toEqual(previous);
  });

  it("treats invalid total distance as already complete without invalid values", () => {
    expect(
      advanceRouteProgress(createInitialRideProgress(), 10, 1, {
        totalDistanceMeters: 0
      })
    ).toEqual({
      distanceMeters: 0,
      progress01: 1,
      completed: true
    });
  });
});

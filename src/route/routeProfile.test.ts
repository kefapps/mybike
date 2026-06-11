import { describe, expect, it } from "vitest";
import { mockRouteDefinition } from "./mockRouteDefinition";
import { sampleRouteDifficulty } from "./routeProfile";
import type { RouteProfile } from "./routeTypes";

function makeBufferProfileFromMock(): RouteProfile {
  return mockRouteDefinition.profile!;
}

describe("sampleRouteDifficulty", () => {
  const profile = makeBufferProfileFromMock();

  it("returns flat fallback when profile is undefined or null", () => {
    expect(sampleRouteDifficulty(undefined, 0.5)).toEqual({
      elevationMeters: 0,
      grade: 0,
      segmentType: "flat"
    });

    expect(sampleRouteDifficulty(null, 0.5)).toEqual({
      elevationMeters: 0,
      grade: 0,
      segmentType: "flat"
    });
  });

  it("interpolates elevation at boundaries correctly", () => {
    const start = sampleRouteDifficulty(profile, 0);

    expect(start.elevationMeters).toBe(0);

    const end = sampleRouteDifficulty(profile, 1);

    expect(end.elevationMeters).toBe(3);
  });

  it("interpolates elevation at intermediate distances", () => {
    const midway = sampleRouteDifficulty(profile, 0.5);

    expect(midway.elevationMeters).toBeCloseTo(10, 4);
  });

  it("returns finite grade values across the full route", () => {
    const samples = [0, 0.1, 0.25, 0.5, 0.75, 0.9, 1].map((p) =>
      sampleRouteDifficulty(profile, p)
    );

    for (const sample of samples) {
      expect(Number.isFinite(sample.grade)).toBe(true);
    }
  });

  it("detects positive grades during the climb segment", () => {
    const inClimb = sampleRouteDifficulty(profile, 0.35);

    expect(inClimb.segmentType).toBe("climb");
    expect(inClimb.grade).toBeGreaterThan(0);
  });

  it("detects negative grades during the descent segment", () => {
    const inDescent = sampleRouteDifficulty(profile, 0.6);

    expect(inDescent.segmentType).toBe("descent");
    expect(inDescent.grade).toBeLessThan(0);
  });

  it("returns correct segment types across transitions", () => {
    expect(sampleRouteDifficulty(profile, 0).segmentType).toBe("flat");
    expect(sampleRouteDifficulty(profile, 0.14).segmentType).toBe("flat");
    expect(sampleRouteDifficulty(profile, 0.15).segmentType).toBe("climb");
    expect(sampleRouteDifficulty(profile, 0.49).segmentType).toBe("climb");
    expect(sampleRouteDifficulty(profile, 0.5).segmentType).toBe("descent");
    expect(sampleRouteDifficulty(profile, 0.69).segmentType).toBe("descent");
    expect(sampleRouteDifficulty(profile, 0.7).segmentType).toBe("sprint");
    expect(sampleRouteDifficulty(profile, 0.84).segmentType).toBe("sprint");
    expect(sampleRouteDifficulty(profile, 0.85).segmentType).toBe("recovery");
    expect(sampleRouteDifficulty(profile, 1).segmentType).toBe("recovery");
  });

  it("clamps out-of-bounds progress01 gracefully", () => {
    const below = sampleRouteDifficulty(profile, -0.2);

    expect(Number.isFinite(below.elevationMeters)).toBe(true);
    expect(Number.isFinite(below.grade)).toBe(true);

    const above = sampleRouteDifficulty(profile, 1.5);

    expect(Number.isFinite(above.elevationMeters)).toBe(true);
    expect(Number.isFinite(above.grade)).toBe(true);
  });

  it("returns flat fallback for empty elevation points", () => {
    const emptyProfile: RouteProfile = {
      elevationPoints: [],
      difficultySegments: []
    };

    const result = sampleRouteDifficulty(emptyProfile, 0.5);

    expect(result.elevationMeters).toBe(0);
    expect(result.grade).toBe(0);
    expect(result.segmentType).toBe("flat");
  });

  it("covers the full difficulty segment range without gaps", () => {
    const segments = profile.difficultySegments;
    const sorted = [...segments].sort(
      (a, b) => a.fromProgress01 - b.fromProgress01
    );

    expect(sorted[0].fromProgress01).toBe(0);
    expect(sorted[sorted.length - 1].toProgress01).toBe(1);

    let coveredUntil = 0;
    for (const segment of sorted) {
      expect(segment.fromProgress01).toBeLessThanOrEqual(coveredUntil + 0.0001);
      coveredUntil = Math.max(coveredUntil, segment.toProgress01);
    }
    expect(coveredUntil).toBeGreaterThanOrEqual(1);
  });
});

import { describe, expect, it } from "vitest";

import {
  formatDistance,
  formatDuration,
  formatEffortPercent,
  formatSpeed
} from "./rideStatsFormat";

describe("ride stats formatting", () => {
  it("formats elapsed time as minutes and seconds", () => {
    expect(formatDuration(65_400)).toBe("1 min 05 s");
    expect(formatDuration(-1)).toBe("0 min 00 s");
  });

  it("formats distance in meters then kilometers", () => {
    expect(formatDistance(42.4)).toBe("42 m");
    expect(formatDistance(1_250)).toBe("1,3 km");
  });

  it("formats speed from meters per second to kilometers per hour", () => {
    expect(formatSpeed(3)).toBe("10,8 km/h");
    expect(formatSpeed(Number.NaN)).toBe("0,0 km/h");
  });

  it("formats effort as a clamped percentage", () => {
    expect(formatEffortPercent(0.555)).toBe("56 %");
    expect(formatEffortPercent(-1)).toBe("0 %");
    expect(formatEffortPercent(2)).toBe("100 %");
  });
});

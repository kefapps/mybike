import type { MockRideInputSource, RideInputSample } from "./rideTypes";

export function clampEffort01(effort01: number): number {
  if (Number.isNaN(effort01) || effort01 === Number.NEGATIVE_INFINITY) {
    return 0;
  }

  if (effort01 === Number.POSITIVE_INFINITY) {
    return 1;
  }

  return Math.min(1, Math.max(0, effort01));
}

export function createMockRideInputSource(
  initialEffort01 = 0
): MockRideInputSource {
  let currentEffort01 = clampEffort01(initialEffort01);

  return {
    kind: "mock",
    getEffort01() {
      return currentEffort01;
    },
    setEffort01(effort01: number) {
      currentEffort01 = clampEffort01(effort01);
    },
    read(nowMs: number): RideInputSample {
      return {
        source: "mock",
        effort01: clampEffort01(currentEffort01),
        nowMs
      };
    }
  };
}

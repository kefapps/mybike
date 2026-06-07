import { describe, expect, it } from "vitest";

import { isCaptureCleanHudEnabled } from "./RideScreen";

describe("RideScreen capture mode", () => {
  it("enables clean capture only through an explicit query flag", () => {
    expect(isCaptureCleanHudEnabled("?captureHud=hidden")).toBe(true);
    expect(isCaptureCleanHudEnabled("?captureHud=visible")).toBe(false);
    expect(isCaptureCleanHudEnabled("")).toBe(false);
  });
});

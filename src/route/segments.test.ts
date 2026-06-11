import { describe, expect, it } from "vitest";

import { mockRouteDefinition } from "./mockRouteDefinition";
import { resolveRouteDefinition } from "./routeValidation";
import {
  EMPTY_SEGMENT_SELECTION,
  isSegmentDefinition,
  isSegmentKind,
  selectSegmentAtProgress,
  type SegmentDefinition
} from "./segments";

describe("segments helpers", () => {
  it("classifies known segment kinds", () => {
    expect(isSegmentKind("warmup")).toBe(true);
    expect(isSegmentKind("climb")).toBe(true);
    expect(isSegmentKind("sprint")).toBe(true);
    expect(isSegmentKind("recovery")).toBe(true);
    expect(isSegmentKind("cooldown")).toBe(true);
    expect(isSegmentKind("rest")).toBe(false);
    expect(isSegmentKind(undefined)).toBe(false);
  });

  it("validates a well-formed segment definition", () => {
    const valid: SegmentDefinition = {
      id: "warmup",
      kind: "warmup",
      fromProgress01: 0,
      toProgress01: 0.2,
      targetEffort01: 0.4
    };

    expect(isSegmentDefinition(valid)).toBe(true);
  });

  it("rejects malformed segment definitions", () => {
    expect(
      isSegmentDefinition({
        id: "",
        kind: "warmup",
        fromProgress01: 0,
        toProgress01: 0.2,
        targetEffort01: 0.4
      })
    ).toBe(false);
    expect(
      isSegmentDefinition({
        id: "x",
        kind: "rest",
        fromProgress01: 0,
        toProgress01: 0.2,
        targetEffort01: 0.4
      })
    ).toBe(false);
    expect(
      isSegmentDefinition({
        id: "x",
        kind: "warmup",
        fromProgress01: 0.3,
        toProgress01: 0.2,
        targetEffort01: 0.4
      })
    ).toBe(false);
    expect(
      isSegmentDefinition({
        id: "x",
        kind: "warmup",
        fromProgress01: 0,
        toProgress01: 0.2,
        targetEffort01: 1.5
      })
    ).toBe(false);
  });
});

describe("selectSegmentAtProgress", () => {
  it("returns the mock warmup segment at the start of the route", () => {
    const selection = selectSegmentAtProgress(mockRouteDefinition, 0.05);

    expect(selection.current).not.toBeNull();
    expect(selection.current?.kind).toBe("warmup");
    expect(selection.current?.id).toBe("warmup");
    expect(selection.next?.kind).toBe("climb");
    expect(selection.index).toBe(0);
    expect(selection.count).toBe(5);
    expect(selection.targetEffort01).toBeCloseTo(0.3, 5);
    expect(selection.progress01).toBeGreaterThan(0);
    expect(selection.remainingProgress01).toBeGreaterThan(0);
    expect(selection.remainingDistanceMeters).toBeGreaterThan(0);
  });

  it("selects the climb segment in the middle of the route", () => {
    const selection = selectSegmentAtProgress(mockRouteDefinition, 0.3);

    expect(selection.current?.kind).toBe("climb");
    expect(selection.next?.kind).toBe("sprint");
  });

  it("selects the sprint segment between 45% and 55%", () => {
    const selection = selectSegmentAtProgress(mockRouteDefinition, 0.5);

    expect(selection.current?.kind).toBe("sprint");
    expect(selection.next?.kind).toBe("recovery");
    expect(selection.targetEffort01).toBeCloseTo(0.95, 5);
  });

  it("selects the cooldown segment at the end of the route", () => {
    const selection = selectSegmentAtProgress(mockRouteDefinition, 0.95);

    expect(selection.current?.kind).toBe("cooldown");
    expect(selection.next).toBeNull();
    expect(selection.index).toBe(4);
  });

  it("clamps invalid progress before selecting a segment", () => {
    const negative = selectSegmentAtProgress(mockRouteDefinition, -0.1);
    const nanSelection = selectSegmentAtProgress(
      mockRouteDefinition,
      Number.NaN
    );
    const infiniteSelection = selectSegmentAtProgress(
      mockRouteDefinition,
      Number.POSITIVE_INFINITY
    );

    expect(negative.current?.kind).toBe("warmup");
    expect(nanSelection.current?.kind).toBe("warmup");
    expect(infiniteSelection.current?.kind).toBe("cooldown");
  });

  it("returns an empty selection for routes without segments", () => {
    const selection = selectSegmentAtProgress(
      {
        ...mockRouteDefinition,
        segments: []
      },
      0.5
    );

    expect(selection).toEqual(EMPTY_SEGMENT_SELECTION);
  });

  it("returns an empty selection when the placeholder route is used", () => {
    const selection = selectSegmentAtProgress(undefined, 0.5);

    expect(selection).toEqual(EMPTY_SEGMENT_SELECTION);
  });

  it("falls back to a valid selection when segments are invalid", () => {
    const invalidRoute = {
      ...mockRouteDefinition,
      segments: [
        {
          id: "bad",
          kind: "warmup",
          fromProgress01: 0.1,
          toProgress01: 0.2,
          targetEffort01: 0.5
        }
      ]
    } as typeof mockRouteDefinition;

    const resolution = resolveRouteDefinition(invalidRoute);

    expect(resolution.usedFallback).toBe(true);
    expect(resolution.reason).toBe("invalid-segments");

    const selection = selectSegmentAtProgress(invalidRoute, 0.5);

    expect(selection).toEqual(EMPTY_SEGMENT_SELECTION);
  });
});

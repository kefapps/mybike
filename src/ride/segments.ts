import { clampEffort01 } from "./mockRideInputSource";
import type { SegmentDefinition, SegmentKind } from "../route";

export type SegmentStatsEntry = {
  segmentId: string;
  kind: SegmentKind;
  targetEffort01: number;
  fromProgress01: number;
  toProgress01: number;
  effortSum: number;
  sampleCount: number;
  distanceMeters: number;
};

export type SegmentStatsState = {
  currentSegmentId: string | null;
  entries: Record<string, SegmentStatsEntry>;
};

export type SegmentAccumulationInput = {
  segment: SegmentDefinition | null;
  effort01: number;
  distanceDeltaMeters: number;
};

export type SegmentStatsResult = {
  segmentId: string;
  kind: SegmentKind;
  targetEffort01: number;
  fromProgress01: number;
  toProgress01: number;
  averageEffort01: number;
  distanceMeters: number;
  sampleCount: number;
  success01: number;
  attempted: boolean;
};

const SUCCESS_TOLERANCE = 0.3;

export function createInitialSegmentStatsState(): SegmentStatsState {
  return {
    currentSegmentId: null,
    entries: {}
  };
}

export function accumulateSegmentStats(
  previous: SegmentStatsState,
  input: SegmentAccumulationInput
): SegmentStatsState {
  const { segment, effort01, distanceDeltaMeters } = input;
  const safeEffort = clampEffort01(effort01);
  const safeDistance = Number.isFinite(distanceDeltaMeters)
    ? Math.max(0, distanceDeltaMeters)
    : 0;

  if (segment === null) {
    return previous;
  }

  const entries: Record<string, SegmentStatsEntry> = { ...previous.entries };
  const existing = entries[segment.id];
  const updated: SegmentStatsEntry = existing
    ? {
        ...existing,
        effortSum: existing.effortSum + safeEffort,
        sampleCount: existing.sampleCount + 1,
        distanceMeters: existing.distanceMeters + safeDistance
      }
    : {
        segmentId: segment.id,
        kind: segment.kind,
        targetEffort01: segment.targetEffort01,
        fromProgress01: segment.fromProgress01,
        toProgress01: segment.toProgress01,
        effortSum: safeEffort,
        sampleCount: 1,
        distanceMeters: safeDistance
      };

  entries[segment.id] = updated;

  return {
    currentSegmentId: segment.id,
    entries
  };
}

export function finalizeSegmentStats(
  state: SegmentStatsState
): SegmentStatsResult[] {
  return Object.values(state.entries)
    .map((entry) => toResult(entry))
    .sort((left, right) => left.fromProgress01 - right.fromProgress01);
}

function toResult(entry: SegmentStatsEntry): SegmentStatsResult {
  const averageEffort01 =
    entry.sampleCount > 0 ? entry.effortSum / entry.sampleCount : 0;
  const deviation = Math.abs(averageEffort01 - entry.targetEffort01);
  const success01 = clampSuccess(1 - deviation / SUCCESS_TOLERANCE);
  const attempted = entry.sampleCount > 0;

  return {
    segmentId: entry.segmentId,
    kind: entry.kind,
    targetEffort01: entry.targetEffort01,
    fromProgress01: entry.fromProgress01,
    toProgress01: entry.toProgress01,
    averageEffort01,
    distanceMeters: entry.distanceMeters,
    sampleCount: entry.sampleCount,
    success01,
    attempted
  };
}

function clampSuccess(value: number): number {
  if (!Number.isFinite(value)) {
    return 0;
  }

  return Math.min(1, Math.max(0, value));
}

export type Rng = {
  next(): number;
  nextInt(maxExclusive: number): number;
  nextRange(minInclusive: number, maxInclusive: number): number;
  pick<T>(values: readonly T[]): T;
};

function safeSeed(seed: number): number {
  if (!Number.isFinite(seed)) {
    return 1;
  }

  return Math.max(1, Math.floor(seed)) >>> 0;
}

export function createLcgRng(seed: number): Rng {
  let state = safeSeed(seed) || 1;

  const next = (): number => {
    state = (Math.imul(state, 1664525) + 1013904223) >>> 0;

    return state / 0x1_0000_0000;
  };

  const nextInt = (maxExclusive: number): number => {
    if (!Number.isFinite(maxExclusive) || maxExclusive <= 0) {
      return 0;
    }

    return Math.floor(next() * maxExclusive);
  };

  const nextRange = (minInclusive: number, maxInclusive: number): number => {
    if (
      !Number.isFinite(minInclusive) ||
      !Number.isFinite(maxInclusive) ||
      maxInclusive < minInclusive
    ) {
      return minInclusive;
    }

    const range = maxInclusive - minInclusive + 1;

    return minInclusive + nextInt(range);
  };

  const pick = <T,>(values: readonly T[]): T => {
    if (values.length === 0) {
      throw new Error("createLcgRng.pick requires a non-empty array");
    }

    return values[nextInt(values.length)] as T;
  };

  return { next, nextInt, nextRange, pick };
}

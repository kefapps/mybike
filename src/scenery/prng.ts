export function mulberry32(seed: number): () => number {
  let state = seed | 0;

  return () => {
    state = (state + 0x6d2b79f5) | 0;
    let t = Math.imul(state ^ (state >>> 15), 1 | state);
    t = (t + Math.imul(t ^ (t >>> 7), 61 | t)) ^ t;

    return ((t ^ (t >>> 14)) >>> 0) / 4294967296;
  };
}

export function hashString(str: string): number {
  let h = 0;

  for (let index = 0; index < str.length; index += 1) {
    h = ((h << 5) - h + str.charCodeAt(index)) | 0;
  }

  return h;
}

export function positionKey(
  seed: string,
  distance: number,
  side: -1 | 1,
  index: number
): string {
  return `${seed}:${Math.round(distance * 1000)}:${side}:${index}`;
}

export function sampleUniform(rand: () => number, min: number, max: number): number {
  return min + rand() * (max - min);
}

export function pickRandom<T>(
  rand: () => number,
  items: readonly T[]
): T {
  return items[Math.floor(rand() * items.length)];
}

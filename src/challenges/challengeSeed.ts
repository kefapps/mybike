function toLocalDateKey(nowMs: number): string {
  const date = new Date(nowMs);

  const year = date.getFullYear();
  const month = (date.getMonth() + 1).toString().padStart(2, "0");
  const day = date.getDate().toString().padStart(2, "0");

  return `${year}-${month}-${day}`;
}

function startOfLocalDayMs(nowMs: number): number {
  const date = new Date(nowMs);

  date.setHours(0, 0, 0, 0);

  return date.getTime();
}

function isFiniteNumber(value: unknown): value is number {
  return typeof value === "number" && Number.isFinite(value);
}

function safeNumber(value: unknown, fallback: number): number {
  return isFiniteNumber(value) ? value : fallback;
}

export function localDateKey(nowMs: number): string {
  if (!isFiniteNumber(nowMs)) {
    return toLocalDateKey(Date.now());
  }

  return toLocalDateKey(nowMs);
}

export function localDayWindow(nowMs: number): {
  startMs: number;
  endMs: number;
  dateKey: string;
} {
  const safeNowMs = safeNumber(nowMs, Date.now());
  const startMs = startOfLocalDayMs(safeNowMs);
  const endMs = startOfLocalDayMs(safeNowMs + 24 * 60 * 60 * 1000);

  return {
    startMs,
    endMs,
    dateKey: toLocalDateKey(safeNowMs)
  };
}

export function hashStringToUint32(input: string): number {
  let hash = 0x811c9dc5;

  for (let index = 0; index < input.length; index += 1) {
    hash ^= input.charCodeAt(index);
    hash = Math.imul(hash, 0x01000193) >>> 0;
  }

  return hash >>> 0;
}

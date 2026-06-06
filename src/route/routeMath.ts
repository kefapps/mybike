import type { Vec3 } from "./routeTypes";

export function isFiniteNumber(value: unknown): value is number {
  return typeof value === "number" && Number.isFinite(value);
}

export function isFiniteVec3(value: unknown): value is Vec3 {
  if (typeof value !== "object" || value === null) {
    return false;
  }

  const candidate = value as Partial<Vec3>;

  return (
    isFiniteNumber(candidate.x) &&
    isFiniteNumber(candidate.y) &&
    isFiniteNumber(candidate.z)
  );
}

export function clampProgress01(value: number): number {
  if (value === Number.POSITIVE_INFINITY) {
    return 1;
  }

  if (!Number.isFinite(value) || value <= 0) {
    return 0;
  }

  if (value >= 1) {
    return 1;
  }

  return value;
}

export function clampFinite(
  value: number,
  min: number,
  max: number,
  fallback: number
): number {
  if (!Number.isFinite(value)) {
    return fallback;
  }

  return Math.min(max, Math.max(min, value));
}

export function lerp(start: number, end: number, t: number): number {
  return start + (end - start) * t;
}

export function lerpVec3(start: Vec3, end: Vec3, t: number): Vec3 {
  return {
    x: lerp(start.x, end.x, t),
    y: lerp(start.y, end.y, t),
    z: lerp(start.z, end.z, t)
  };
}

export function subtractVec3(end: Vec3, start: Vec3): Vec3 {
  return {
    x: end.x - start.x,
    y: end.y - start.y,
    z: end.z - start.z
  };
}

export function normalizeVec3(value: Vec3, fallback: Vec3): Vec3 {
  const length = Math.hypot(value.x, value.y, value.z);

  if (!Number.isFinite(length) || length <= 0) {
    return fallback;
  }

  return {
    x: value.x / length,
    y: value.y / length,
    z: value.z / length
  };
}

export function addVec3(left: Vec3, right: Vec3): Vec3 {
  return {
    x: left.x + right.x,
    y: left.y + right.y,
    z: left.z + right.z
  };
}

export function scaleVec3(value: Vec3, scale: number): Vec3 {
  return {
    x: value.x * scale,
    y: value.y * scale,
    z: value.z * scale
  };
}

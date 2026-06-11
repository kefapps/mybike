import { BufferGeometry } from "three/src/core/BufferGeometry.js";
import { Float32BufferAttribute } from "three/src/core/BufferAttribute.js";
import { Material } from "three/src/materials/Material.js";
import { Points } from "three/src/objects/Points.js";
import { PointsMaterial } from "three/src/materials/PointsMaterial.js";
import { Group } from "three/src/objects/Group.js";

const DEFAULT_RAIN_PARTICLE_COUNT = 220;
const RAIN_SPREAD_X = 26;
const RAIN_SPREAD_Y = 18;
const RAIN_SPREAD_Z = 60;
const RAIN_BASE_Y = 14;
const RAIN_BASE_Z_OFFSET = 8;
const RAIN_FALL_DISTANCE = 26;

export type WeatherKind = "none" | "rain" | "fog";

export type WeatherParticlesOptions = {
  color: number;
  opacity: number;
  density: number;
  particleCount?: number;
};

export type WeatherParticles = {
  group: Group;
  points: Points;
  geometry: BufferGeometry;
  material: Material;
};

export function createWeatherParticles(
  kind: WeatherKind,
  options: WeatherParticlesOptions
): WeatherParticles | undefined {
  if (kind !== "rain") {
    return undefined;
  }

  if (!(options.density > 0)) {
    return undefined;
  }

  const particleCount = clampParticleCount(
    options.particleCount ?? DEFAULT_RAIN_PARTICLE_COUNT,
    options.density
  );

  const positions = new Float32Array(particleCount * 3);

  for (let index = 0; index < particleCount; index += 1) {
    const x = (Math.random() * 2 - 1) * RAIN_SPREAD_X;
    const y = Math.random() * RAIN_SPREAD_Y + RAIN_BASE_Y;
    const z = Math.random() * RAIN_SPREAD_Z + RAIN_BASE_Z_OFFSET;
    positions[index * 3] = x;
    positions[index * 3 + 1] = y;
    positions[index * 3 + 2] = z;
  }

  const geometry = new BufferGeometry();
  geometry.setAttribute("position", new Float32BufferAttribute(positions, 3));

  const material = new PointsMaterial({
    color: options.color,
    size: 0.32,
    transparent: true,
    opacity: clampOpacity(options.opacity),
    depthWrite: false,
    sizeAttenuation: true,
    fog: true
  });

  const points = new Points(geometry, material);
  points.name = "environment-weather-particles";
  points.frustumCulled = false;

  const group = new Group();
  group.name = "environment-weather";
  group.add(points);

  return { group, points, geometry, material };
}

export function animateWeatherParticles(
  weather: WeatherParticles | undefined,
  dtSeconds: number
): void {
  if (weather === undefined) {
    return;
  }

  const positions = weather.geometry.attributes.position;
  const safeDt = Number.isFinite(dtSeconds) ? Math.max(0, dtSeconds) : 0;
  const fallStep = (RAIN_FALL_DISTANCE * safeDt) / 4;

  if (fallStep === 0) {
    return;
  }

  for (let index = 0; index < positions.count; index += 1) {
    const yIndex = index * 3 + 1;
    const zIndex = index * 3 + 2;
    let y = positions.getY(index) - fallStep;
    let z = positions.getZ(index) - fallStep * 0.18;

    if (y < 0) {
      y = RAIN_BASE_Y + Math.random() * RAIN_SPREAD_Y * 0.4;
      z = Math.random() * RAIN_SPREAD_Z + RAIN_BASE_Z_OFFSET;
    }

    positions.setY(index, y);
    positions.setZ(zIndex, z);
  }

  positions.needsUpdate = true;
}

export function disposeWeatherParticles(
  weather: WeatherParticles | undefined
): void {
  if (weather === undefined) {
    return;
  }

  weather.geometry.dispose();
  weather.material.dispose();
}

function clampParticleCount(value: number, density: number): number {
  if (!Number.isFinite(value) || value <= 0) {
    return DEFAULT_RAIN_PARTICLE_COUNT;
  }

  const safeDensity = Number.isFinite(density)
    ? Math.min(1, Math.max(0, density))
    : 0.5;

  return Math.max(8, Math.floor(value * safeDensity));
}

function clampOpacity(value: number): number {
  if (!Number.isFinite(value)) {
    return 0.2;
  }

  return Math.min(1, Math.max(0.05, value));
}

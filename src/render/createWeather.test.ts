import { describe, expect, it, vi } from "vitest";
import type { Object3D } from "three";

import {
  animateWeatherParticles,
  createWeatherParticles,
  disposeWeatherParticles
} from "./createWeather";

describe("createWeatherParticles", () => {
  it("returns undefined for non-rain weather kinds", () => {
    expect(
      createWeatherParticles("none", {
        color: 0xc8d6e2,
        opacity: 0.3,
        density: 0.5
      })
    ).toBeUndefined();
    expect(
      createWeatherParticles("fog", {
        color: 0xc8d6e2,
        opacity: 0.3,
        density: 0.5
      })
    ).toBeUndefined();
  });

  it("returns undefined when rain density is zero", () => {
    expect(
      createWeatherParticles("rain", {
        color: 0xc8d6e2,
        opacity: 0.3,
        density: 0
      })
    ).toBeUndefined();
  });

  it("builds a rain particle group with bounded count and tracking", () => {
    const weather = createWeatherParticles("rain", {
      color: 0xc8d6e2,
      opacity: 0.32,
      density: 0.55,
      particleCount: 120
    });

    expect(weather).toBeDefined();
    if (!weather) {
      throw new Error("Weather should be defined");
    }

    expect(weather.group.name).toBe("environment-weather");
    expect(weather.points.name).toBe("environment-weather-particles");
    expect(weather.geometry.attributes.position.count).toBeGreaterThan(8);
    expect(weather.geometry.attributes.position.count).toBeLessThanOrEqual(120);
    expect(weather.material.transparent).toBe(true);
    expect(weather.material.opacity).toBeCloseTo(0.32, 5);

    const positions = weather.geometry.attributes.position;
    let minX = Number.POSITIVE_INFINITY;
    let maxX = Number.NEGATIVE_INFINITY;

    for (let index = 0; index < positions.count; index += 1) {
      const x = positions.getX(index);
      minX = Math.min(minX, x);
      maxX = Math.max(maxX, x);
    }

    expect(minX).toBeGreaterThanOrEqual(-27);
    expect(maxX).toBeLessThanOrEqual(27);
  });

  it("scales the particle count with density", () => {
    const low = createWeatherParticles("rain", {
      color: 0xc8d6e2,
      opacity: 0.2,
      density: 0.2,
      particleCount: 200
    });
    const high = createWeatherParticles("rain", {
      color: 0xc8d6e2,
      opacity: 0.4,
      density: 1,
      particleCount: 200
    });

    expect(low?.geometry.attributes.position.count ?? 0).toBeLessThan(
      high?.geometry.attributes.position.count ?? 0
    );
  });

  it("animates particles falling without producing NaN values", () => {
    const weather = createWeatherParticles("rain", {
      color: 0xc8d6e2,
      opacity: 0.3,
      density: 0.5,
      particleCount: 60
    });

    if (!weather) {
      throw new Error("Weather should be defined");
    }

    animateWeatherParticles(weather, 0.5);
    animateWeatherParticles(weather, 0.1);
    animateWeatherParticles(weather, 0);

    const positions = weather.geometry.attributes.position;
    for (let index = 0; index < positions.count; index += 1) {
      expect(Number.isFinite(positions.getX(index))).toBe(true);
      expect(Number.isFinite(positions.getY(index))).toBe(true);
      expect(Number.isFinite(positions.getZ(index))).toBe(true);
      expect(positions.getY(index)).toBeGreaterThanOrEqual(-1);
    }
  });

  it("disposes geometries and materials on cleanup", () => {
    const weather = createWeatherParticles("rain", {
      color: 0xc8d6e2,
      opacity: 0.3,
      density: 0.5,
      particleCount: 40
    });

    if (!weather) {
      throw new Error("Weather should be defined");
    }

    const geometryDispose = vi.spyOn(weather.geometry, "dispose");
    const materialDispose = vi.spyOn(weather.material, "dispose");

    disposeWeatherParticles(weather);
    disposeWeatherParticles(undefined);

    expect(geometryDispose).toHaveBeenCalledTimes(1);
    expect(materialDispose).toHaveBeenCalledTimes(1);
  });

  it("finds the rain points under the environment group", () => {
    const weather = createWeatherParticles("rain", {
      color: 0xc8d6e2,
      opacity: 0.3,
      density: 0.5,
      particleCount: 30
    });

    if (!weather) {
      throw new Error("Weather should be defined");
    }

    const found = findObjectByName(weather.group, "environment-weather-particles");
    expect(found).toBe(weather.points);
  });
});

function findObjectByName(root: Object3D, name: string): Object3D | undefined {
  let match: Object3D | undefined;
  root.traverse((object) => {
    if (object.name === name) {
      match = object;
    }
  });

  return match;
}

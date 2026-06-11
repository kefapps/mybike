import { DoubleSide } from "three/src/constants.js";
import { Color } from "three/src/math/Color.js";
import { PerspectiveCamera } from "three/src/cameras/PerspectiveCamera.js";
import { Fog } from "three/src/scenes/Fog.js";
import { Scene } from "three/src/scenes/Scene.js";
import { AmbientLight } from "three/src/lights/AmbientLight.js";
import { DirectionalLight } from "three/src/lights/DirectionalLight.js";
import { Mesh } from "three/src/objects/Mesh.js";
import { PlaneGeometry } from "three/src/geometries/PlaneGeometry.js";
import { MeshStandardMaterial } from "three/src/materials/MeshStandardMaterial.js";
import { MeshBasicMaterial } from "three/src/materials/MeshBasicMaterial.js";
import { WebGLRenderer } from "three/src/renderers/WebGLRenderer.js";

import { generateSceneryInstances, type SceneryQuality } from "../scenery";
import { mockRouteDefinition, type RouteDefinition } from "../route";
import { createBiomeVisuals, type BiomeVisuals } from "./createBiomeVisuals";
import {
  createProceduralScenery,
  type ProceduralSceneryResult
} from "./createProceduralScenery";
import { createRouteMesh, type RouteMeshResult } from "./createRouteMesh";
import {
  animateWeatherParticles,
  createWeatherParticles,
  disposeWeatherParticles,
  type WeatherParticles
} from "./createWeather";
import {
  getEnvironmentPreset,
  isWeatherEnabled,
  type EnvironmentPreset,
  type EnvironmentPresetId
} from "./environmentPresets";
import { calculateGroundHeight } from "./groundHeight";
import {
  resolveRenderPaletteForPresetId,
  toThreeVector3
} from "./renderHelpers";
import type {
  RenderFrameSnapshot,
  SceneController as SceneControllerApi
} from "./sceneTypes";

const DEFAULT_WIDTH = 640;
const DEFAULT_HEIGHT = 360;
const CAMERA_NEAR = 0.1;
const CAMERA_FAR = 1400;

type PointsMaterialWithColor = {
  color: Color;
  opacity: number;
};

export type SceneControllerOptions = {
  route?: RouteDefinition;
  scenerySeed?: string;
  sceneryQuality?: SceneryQuality;
};

export class ThreeSceneController implements SceneControllerApi {
  private readonly route: RouteDefinition;
  private readonly scene = new Scene();
  private readonly backgroundColor = new Color(0xb7d9ec);
  private readonly fog = new Fog(0xd4eef5, 22, 380);
  private readonly camera = new PerspectiveCamera(
    70,
    DEFAULT_WIDTH / DEFAULT_HEIGHT,
    CAMERA_NEAR,
    CAMERA_FAR
  );
  private readonly ambientLight = new AmbientLight(0xffffff, 0.75);
  private readonly sunLight = new DirectionalLight(0xffffff, 1.15);
  private readonly routeMesh: RouteMeshResult;
  private readonly ground: Mesh<
    PlaneGeometry,
    MeshStandardMaterial
  >;
  private readonly horizon: Mesh<
    PlaneGeometry,
    MeshBasicMaterial
  >;
  private readonly biomeVisuals: BiomeVisuals;
  private readonly proceduralScenery: ProceduralSceneryResult;
  private weather: WeatherParticles | undefined;
  private currentPreset: EnvironmentPreset = getEnvironmentPreset("morning");
  private currentPresetId: EnvironmentPresetId = "morning";
  private currentWeatherEnabled = true;
  private renderer: WebGLRenderer | undefined;
  private disposed = false;

  constructor(options: SceneControllerOptions = {}) {
    this.route = options.route ?? mockRouteDefinition;
    this.routeMesh = createRouteMesh(this.route);
    this.ground = this.createGround();
    this.horizon = this.createHorizon();
    this.biomeVisuals = createBiomeVisuals(this.route);

    const sceneryInstances = generateSceneryInstances(
      this.route,
      options.scenerySeed ?? "default-scenery-seed",
      options.sceneryQuality ?? "medium"
    );
    this.proceduralScenery = createProceduralScenery(sceneryInstances);

    this.scene.background = this.backgroundColor;
    this.scene.fog = this.fog;
    this.sunLight.position.set(-14, 38, -18);
    this.scene.add(
      this.ambientLight,
      this.sunLight,
      this.ground,
      this.horizon,
      this.routeMesh.group,
      this.biomeVisuals.group,
      this.proceduralScenery.group
    );
  }

  mount(canvas: HTMLCanvasElement): boolean {
    if (this.disposed) {
      return false;
    }

    try {
      this.renderer = new WebGLRenderer({
        canvas,
        antialias: true,
        alpha: false
      });
    } catch {
      this.renderer = undefined;
      return false;
    }

    if (this.renderer === undefined) {
      return false;
    }

    this.renderer.setPixelRatio(Math.min(window.devicePixelRatio || 1, 1.5));
    this.renderer.setClearColor(0xb7d9ec, 1);
    this.resize(
      canvas.clientWidth || DEFAULT_WIDTH,
      canvas.clientHeight || DEFAULT_HEIGHT
    );

    return true;
  }

  resize(width: number, height: number): void {
    if (this.disposed) {
      return;
    }

    const safeWidth = sanitizeDimension(width, DEFAULT_WIDTH);
    const safeHeight = sanitizeDimension(height, DEFAULT_HEIGHT);
    this.camera.aspect = safeWidth / safeHeight;
    this.camera.updateProjectionMatrix();
    this.renderer?.setSize(safeWidth, safeHeight, false);
  }

  update(snapshot: RenderFrameSnapshot): void {
    if (this.disposed) {
      return;
    }

    const presetId: EnvironmentPresetId | null | undefined =
      snapshot.environmentPresetId ?? this.currentPresetId;
    const weatherToggle = snapshot.weatherEnabled ?? this.currentWeatherEnabled;
    const preset = getEnvironmentPreset(presetId);
    const palette = resolveRenderPaletteForPresetId(
      snapshot.route.biomeId,
      snapshot.route.usedFallback,
      presetId
    );

    this.applyPalette(palette, snapshot.route.usedFallback);
    this.applyEnvironment(preset, weatherToggle);
    this.applyCamera(snapshot);
    this.renderer?.render(this.scene, this.camera);
  }

  private applyPalette(
    palette: ReturnType<typeof resolveRenderPaletteForPresetId>,
    usedFallback: boolean
  ): void {
    this.backgroundColor.setHex(palette.sky);
    this.fog.color.setHex(palette.fog);
    this.fog.near = palette.fogNear;
    this.fog.far = palette.fogFar;
    this.routeMesh.material.color.setHex(palette.route);
    this.ground.material.color.setHex(palette.ground);
    this.horizon.material.color.setHex(palette.horizon);
    this.horizon.material.opacity = palette.horizonOpacity;
    this.biomeVisuals.coastMaterial.color.setHex(
      usedFallback ? palette.accent : 0x277fa8
    );
    this.biomeVisuals.forestMaterial.color.setHex(
      usedFallback ? palette.accent : 0x1a5f39
    );
    for (const tint of this.biomeVisuals.fallbackTintMaterials) {
      tint.material.color.setHex(
        usedFallback ? tint.fallbackColor : tint.defaultColor
      );
    }
  }

  private applyEnvironment(preset: EnvironmentPreset, weatherToggle: boolean): void {
    this.ambientLight.color.setHex(preset.ambientColor);
    this.ambientLight.intensity = preset.ambientIntensity;
    this.sunLight.color.setHex(preset.sunColor);
    this.sunLight.intensity = preset.sunIntensity;
    this.sunLight.position.set(
      preset.sunPosition.x,
      preset.sunPosition.y,
      preset.sunPosition.z
    );
    this.sunLight.castShadow = preset.shadows;
    this.sunLight.shadow.mapSize.width = 1024;
    this.sunLight.shadow.mapSize.height = 1024;

    this.syncWeather(preset, weatherToggle);
  }

  private applyCamera(snapshot: RenderFrameSnapshot): void {
    this.camera.fov = snapshot.route.camera.fovDegrees;
    this.camera.position.copy(toThreeVector3(snapshot.route.camera.position));
    this.camera.lookAt(toThreeVector3(snapshot.route.camera.lookAt));
    this.camera.updateProjectionMatrix();
  }

  private syncWeather(preset: EnvironmentPreset, weatherToggle: boolean): void {
    const shouldShowWeather = isWeatherEnabled(preset, weatherToggle);
    this.currentPreset = preset;
    this.currentPresetId = preset.id;
    this.currentWeatherEnabled = weatherToggle;

    if (!shouldShowWeather) {
      if (this.weather !== undefined) {
        this.scene.remove(this.weather.group);
        disposeWeatherParticles(this.weather);
        this.weather = undefined;
      }
      return;
    }

    if (
      this.weather !== undefined &&
      (this.weather.points.material as unknown as PointsMaterialWithColor).color.getHex() !==
        preset.weatherParticleColor
    ) {
      this.scene.remove(this.weather.group);
      disposeWeatherParticles(this.weather);
      this.weather = undefined;
    }

    if (this.weather === undefined) {
      this.weather =
        createWeatherParticles(preset.weather, {
          color: preset.weatherParticleColor,
          opacity: preset.weatherParticleOpacity,
          density: preset.weatherDensity
        }) ?? undefined;

      if (this.weather !== undefined) {
        this.scene.add(this.weather.group);
      }
      return;
    }

    const material = this.weather.points.material as unknown as PointsMaterialWithColor;
    const nextOpacity = Math.min(
      1,
      Math.max(0.05, preset.weatherParticleOpacity)
    );
    material.color.setHex(preset.weatherParticleColor);
    material.opacity = nextOpacity;
    animateWeatherParticles(this.weather, 0);
  }

  dispose(): void {
    if (this.disposed) {
      return;
    }

    this.disposed = true;
    this.renderer?.setAnimationLoop(null);
    this.renderer?.dispose();
    if (this.weather !== undefined) {
      disposeWeatherParticles(this.weather);
      this.weather = undefined;
    }
    this.disposeTrackedResources();
    this.scene.clear();
    this.renderer = undefined;
  }

  private createGround(): Mesh<
    PlaneGeometry,
    MeshStandardMaterial
  > {
    const geometry = new PlaneGeometry(420, 1400, 72, 80);
    const positions = geometry.attributes.position;

    for (let index = 0; index < positions.count; index += 1) {
      const x = positions.getX(index);
      const y = positions.getY(index);

      positions.setZ(index, calculateGroundHeight(this.route, x, y));
    }

    geometry.computeVertexNormals();
    const material = new MeshStandardMaterial({
      color: 0x8cc7a5,
      roughness: 0.94,
      metalness: 0
    });
    const mesh = new Mesh(geometry, material);
    mesh.name = "mvp-ground";
    mesh.rotation.x = -Math.PI / 2;
    mesh.position.set(0, -0.04, 560);

    return mesh;
  }

  private createHorizon(): Mesh<
    PlaneGeometry,
    MeshBasicMaterial
  > {
    const geometry = new PlaneGeometry(760, 70);
    const material = new MeshBasicMaterial({
      color: 0xb7d9ec,
      side: DoubleSide,
      transparent: true,
      opacity: 0.48
    });
    const mesh = new Mesh(geometry, material);
    mesh.name = "mvp-horizon";
    mesh.position.set(0, 26, 1060);

    return mesh;
  }

  private disposeTrackedResources(): void {
    for (const geometry of this.routeMesh.geometries) {
      geometry.dispose();
    }

    for (const material of this.routeMesh.materials) {
      material.dispose();
    }

    this.ground.geometry.dispose();
    this.ground.material.dispose();
    this.horizon.geometry.dispose();
    this.horizon.material.dispose();

    for (const geometry of this.biomeVisuals.geometries) {
      geometry.dispose();
    }

    for (const material of this.biomeVisuals.materials) {
      material.dispose();
    }

    for (const geometry of this.proceduralScenery.geometries) {
      geometry.dispose();
    }

    for (const material of this.proceduralScenery.materials) {
      material.dispose();
    }
  }
}

export function createSceneController(
  options?: SceneControllerOptions
): SceneControllerApi {
  return new ThreeSceneController(options);
}

function sanitizeDimension(value: number, fallback: number): number {
  if (!Number.isFinite(value) || value <= 0) {
    return fallback;
  }

  return Math.max(1, Math.floor(value));
}

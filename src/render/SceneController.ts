import * as THREE from "three";

import { mockRouteDefinition, type RouteDefinition } from "../route";
import { createBiomeVisuals, type BiomeVisuals } from "./createBiomeVisuals";
import { createRouteMesh, type RouteMeshResult } from "./createRouteMesh";
import { calculateGroundHeight } from "./groundHeight";
import {
  getBiomePalette,
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

export type SceneControllerOptions = {
  route?: RouteDefinition;
};

export class ThreeSceneController implements SceneControllerApi {
  private readonly route: RouteDefinition;
  private readonly scene = new THREE.Scene();
  private readonly backgroundColor = new THREE.Color(0xb7d9ec);
  private readonly fog = new THREE.Fog(0xd4eef5, 22, 380);
  private readonly camera = new THREE.PerspectiveCamera(
    70,
    DEFAULT_WIDTH / DEFAULT_HEIGHT,
    CAMERA_NEAR,
    CAMERA_FAR
  );
  private readonly ambientLight = new THREE.AmbientLight(0xffffff, 0.75);
  private readonly sunLight = new THREE.DirectionalLight(0xffffff, 1.15);
  private readonly routeMesh: RouteMeshResult;
  private readonly ground: THREE.Mesh<
    THREE.PlaneGeometry,
    THREE.MeshStandardMaterial
  >;
  private readonly horizon: THREE.Mesh<
    THREE.PlaneGeometry,
    THREE.MeshBasicMaterial
  >;
  private readonly biomeVisuals: BiomeVisuals;
  private renderer: THREE.WebGLRenderer | undefined;
  private disposed = false;

  constructor(options: SceneControllerOptions = {}) {
    this.route = options.route ?? mockRouteDefinition;
    this.routeMesh = createRouteMesh(this.route);
    this.ground = this.createGround();
    this.horizon = this.createHorizon();
    this.biomeVisuals = createBiomeVisuals(this.route);

    this.scene.background = this.backgroundColor;
    this.scene.fog = this.fog;
    this.sunLight.position.set(-14, 38, -18);
    this.scene.add(
      this.ambientLight,
      this.sunLight,
      this.ground,
      this.horizon,
      this.routeMesh.group,
      this.biomeVisuals.group
    );
  }

  mount(canvas: HTMLCanvasElement): boolean {
    if (this.disposed) {
      return false;
    }

    try {
      this.renderer = new THREE.WebGLRenderer({
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

    const palette = getBiomePalette(
      snapshot.route.biomeId,
      snapshot.route.usedFallback
    );

    this.backgroundColor.setHex(palette.sky);
    this.fog.color.setHex(palette.fog);
    this.fog.near = palette.fogNear;
    this.fog.far = palette.fogFar;
    this.routeMesh.material.color.setHex(palette.route);
    this.ground.material.color.setHex(palette.ground);
    this.horizon.material.color.setHex(palette.horizon);
    this.horizon.material.opacity = palette.horizonOpacity;
    this.biomeVisuals.coastMaterial.color.setHex(
      snapshot.route.usedFallback ? palette.accent : 0x277fa8
    );
    this.biomeVisuals.forestMaterial.color.setHex(
      snapshot.route.usedFallback ? palette.accent : 0x1a5f39
    );
    for (const tint of this.biomeVisuals.fallbackTintMaterials) {
      tint.material.color.setHex(
        snapshot.route.usedFallback ? tint.fallbackColor : tint.defaultColor
      );
    }
    this.ambientLight.color.setHex(palette.ambientColor);
    this.ambientLight.intensity = palette.ambientIntensity;
    this.sunLight.color.setHex(palette.sunColor);
    this.sunLight.intensity = palette.sunIntensity;
    this.sunLight.position.set(
      palette.sunPosition.x,
      palette.sunPosition.y,
      palette.sunPosition.z
    );

    this.camera.fov = snapshot.route.camera.fovDegrees;
    this.camera.position.copy(toThreeVector3(snapshot.route.camera.position));
    this.camera.lookAt(toThreeVector3(snapshot.route.camera.lookAt));
    this.camera.updateProjectionMatrix();

    this.renderer?.render(this.scene, this.camera);
  }

  dispose(): void {
    if (this.disposed) {
      return;
    }

    this.disposed = true;
    this.renderer?.setAnimationLoop(null);
    this.renderer?.dispose();
    this.disposeTrackedResources();
    this.scene.clear();
    this.renderer = undefined;
  }

  private createGround(): THREE.Mesh<
    THREE.PlaneGeometry,
    THREE.MeshStandardMaterial
  > {
    const geometry = new THREE.PlaneGeometry(420, 1400, 72, 80);
    const positions = geometry.attributes.position;

    for (let index = 0; index < positions.count; index += 1) {
      const x = positions.getX(index);
      const y = positions.getY(index);

      positions.setZ(index, calculateGroundHeight(this.route, x, y));
    }

    geometry.computeVertexNormals();
    const material = new THREE.MeshStandardMaterial({
      color: 0x8cc7a5,
      roughness: 0.94,
      metalness: 0
    });
    const mesh = new THREE.Mesh(geometry, material);
    mesh.name = "mvp-ground";
    mesh.rotation.x = -Math.PI / 2;
    mesh.position.set(0, -0.04, 560);

    return mesh;
  }

  private createHorizon(): THREE.Mesh<
    THREE.PlaneGeometry,
    THREE.MeshBasicMaterial
  > {
    const geometry = new THREE.PlaneGeometry(760, 70);
    const material = new THREE.MeshBasicMaterial({
      color: 0xb7d9ec,
      side: THREE.DoubleSide,
      transparent: true,
      opacity: 0.48
    });
    const mesh = new THREE.Mesh(geometry, material);
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

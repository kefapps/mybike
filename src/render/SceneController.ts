import * as THREE from "three";

import { mockRouteDefinition, type RouteDefinition } from "../route";
import { createBiomeVisuals, type BiomeVisuals } from "./createBiomeVisuals";
import { createRouteMesh, type RouteMeshResult } from "./createRouteMesh";
import { getBiomePalette, toThreeVector3 } from "./renderHelpers";
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
    this.biomeVisuals = createBiomeVisuals();

    this.sunLight.position.set(-14, 38, -18);
    this.scene.add(
      this.ambientLight,
      this.sunLight,
      this.ground,
      this.horizon,
      this.routeMesh.mesh,
      this.biomeVisuals.group
    );
  }

  mount(canvas: HTMLCanvasElement): void {
    if (this.disposed) {
      return;
    }

    try {
      this.renderer = new THREE.WebGLRenderer({
        canvas,
        antialias: true,
        alpha: false
      });
    } catch {
      this.renderer = undefined;
      return;
    }

    this.renderer.setPixelRatio(Math.min(window.devicePixelRatio || 1, 1.5));
    this.renderer.setClearColor(0xb7d9ec, 1);
    this.resize(
      canvas.clientWidth || DEFAULT_WIDTH,
      canvas.clientHeight || DEFAULT_HEIGHT
    );
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

    this.scene.background = new THREE.Color(palette.sky);
    this.scene.fog = new THREE.Fog(palette.fog, 35, 260);
    this.routeMesh.material.color.setHex(palette.route);
    this.ground.material.color.setHex(palette.ground);
    this.horizon.material.color.setHex(palette.sky);
    this.biomeVisuals.coastMaterial.color.setHex(
      snapshot.route.usedFallback ? palette.accent : 0x4f9fcf
    );
    this.biomeVisuals.forestMaterial.color.setHex(
      snapshot.route.usedFallback ? palette.accent : 0x174d2e
    );
    this.ambientLight.intensity = snapshot.route.biomeId === "forest" ? 0.62 : 0.82;
    this.sunLight.intensity = snapshot.route.biomeId === "forest" ? 0.9 : 1.2;

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
    const geometry = new THREE.PlaneGeometry(260, 1300);
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
    const geometry = new THREE.PlaneGeometry(500, 20);
    const material = new THREE.MeshBasicMaterial({
      color: 0xb7d9ec,
      side: THREE.DoubleSide,
      transparent: true,
      opacity: 0.35
    });
    const mesh = new THREE.Mesh(geometry, material);
    mesh.name = "mvp-horizon";
    mesh.position.set(0, 12, 1050);

    return mesh;
  }

  private disposeTrackedResources(): void {
    this.routeMesh.geometry.dispose();
    this.routeMesh.material.dispose();
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

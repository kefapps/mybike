import * as THREE from "three";

import type { RouteDefinition } from "../route";
import { getRouteRenderPoints } from "./renderHelpers";

const ROUTE_WIDTH_METERS = 5;

export type RouteMeshResult = {
  mesh: THREE.Mesh;
  geometry: THREE.BufferGeometry;
  material: THREE.MeshStandardMaterial;
};

export function createRouteMesh(route: RouteDefinition): RouteMeshResult {
  const points = getRouteRenderPoints(route);
  const vertices: number[] = [];
  const indices: number[] = [];

  for (let index = 0; index < points.length; index += 1) {
    const previous = points[Math.max(0, index - 1)];
    const current = points[index];
    const next = points[Math.min(points.length - 1, index + 1)];
    const tangent = next.clone().sub(previous);
    const perpendicular = new THREE.Vector3(-tangent.z, 0, tangent.x);

    if (perpendicular.lengthSq() === 0) {
      perpendicular.set(1, 0, 0);
    }

    perpendicular.normalize().multiplyScalar(ROUTE_WIDTH_METERS / 2);

    const left = current.clone().add(perpendicular);
    const right = current.clone().sub(perpendicular);

    vertices.push(left.x, left.y, left.z, right.x, right.y, right.z);

    if (index < points.length - 1) {
      const leftCurrent = index * 2;
      const rightCurrent = leftCurrent + 1;
      const leftNext = leftCurrent + 2;
      const rightNext = leftCurrent + 3;

      indices.push(leftCurrent, rightCurrent, leftNext);
      indices.push(rightCurrent, rightNext, leftNext);
    }
  }

  const geometry = new THREE.BufferGeometry();
  geometry.setAttribute(
    "position",
    new THREE.Float32BufferAttribute(vertices, 3)
  );
  geometry.setIndex(indices);
  geometry.computeVertexNormals();

  const material = new THREE.MeshStandardMaterial({
    color: 0xf2d58b,
    roughness: 0.86,
    metalness: 0,
    side: THREE.DoubleSide
  });
  const mesh = new THREE.Mesh(geometry, material);
  mesh.name = "mvp-route-ribbon";

  return { mesh, geometry, material };
}

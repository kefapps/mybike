import * as THREE from "three";

import { resolveRouteDefinition, type RouteDefinition } from "../route";
import { toThreeVector3 } from "./renderHelpers";

const ROUTE_WIDTH_METERS = 5;
const SHOULDER_WIDTH_METERS = 3.8;

export type RouteMeshResult = {
  group: THREE.Group;
  mesh: THREE.Mesh;
  geometry: THREE.BufferGeometry;
  material: THREE.MeshStandardMaterial;
  geometries: THREE.BufferGeometry[];
  materials: THREE.Material[];
};

export function createRouteMesh(route: RouteDefinition): RouteMeshResult {
  const frames = getRouteFrames(route);
  const group = new THREE.Group();
  group.name = "scenic-route";

  const roadGeometry = createRibbonGeometry(frames, ROUTE_WIDTH_METERS / 2);
  const leftShoulderGeometry = createSideRibbonGeometry(
    frames,
    ROUTE_WIDTH_METERS / 2 + 0.2,
    ROUTE_WIDTH_METERS / 2 + SHOULDER_WIDTH_METERS,
    1
  );
  const rightShoulderGeometry = createSideRibbonGeometry(
    frames,
    ROUTE_WIDTH_METERS / 2 + 0.2,
    ROUTE_WIDTH_METERS / 2 + SHOULDER_WIDTH_METERS,
    -1
  );
  const leftEdgeGeometry = createSideRibbonGeometry(
    frames,
    ROUTE_WIDTH_METERS / 2 - 0.18,
    ROUTE_WIDTH_METERS / 2 - 0.02,
    1
  );
  const rightEdgeGeometry = createSideRibbonGeometry(
    frames,
    ROUTE_WIDTH_METERS / 2 - 0.18,
    ROUTE_WIDTH_METERS / 2 - 0.02,
    -1
  );
  const dashGeometry = new THREE.BoxGeometry(0.18, 0.035, 7);
  const surfaceBandGeometry = new THREE.BoxGeometry(
    ROUTE_WIDTH_METERS - 0.45,
    0.028,
    0.42
  );
  const shoulderRhythmGeometry = new THREE.BoxGeometry(0.46, 0.035, 2.4);
  const surfaceGrainGeometry = new THREE.BoxGeometry(0.72, 0.024, 0.18);

  const roadMaterial = new THREE.MeshStandardMaterial({
    color: 0xf2d58b,
    roughness: 0.9,
    metalness: 0,
    side: THREE.DoubleSide
  });
  const shoulderMaterial = new THREE.MeshStandardMaterial({
    color: 0xb5b36f,
    roughness: 0.96,
    metalness: 0,
    side: THREE.DoubleSide
  });
  const edgeMaterial = new THREE.MeshStandardMaterial({
    color: 0xfff3c2,
    roughness: 0.7,
    metalness: 0,
    side: THREE.DoubleSide
  });
  const dashMaterial = new THREE.MeshStandardMaterial({
    color: 0xfdf4d0,
    roughness: 0.74,
    metalness: 0
  });
  const surfaceBandMaterial = new THREE.MeshStandardMaterial({
    color: 0xd7b96d,
    roughness: 0.95,
    metalness: 0,
    transparent: true,
    opacity: 0.42
  });
  const shoulderRhythmMaterial = new THREE.MeshStandardMaterial({
    color: 0xe8d793,
    roughness: 0.9,
    metalness: 0,
    transparent: true,
    opacity: 0.62
  });
  const surfaceGrainMaterial = new THREE.MeshStandardMaterial({
    color: 0xa88345,
    roughness: 0.98,
    metalness: 0,
    transparent: true,
    opacity: 0.34
  });

  const roadMesh = new THREE.Mesh(roadGeometry, roadMaterial);
  roadMesh.name = "scenic-route-surface";

  const leftShoulder = new THREE.Mesh(leftShoulderGeometry, shoulderMaterial);
  leftShoulder.name = "scenic-route-left-shoulder";

  const rightShoulder = new THREE.Mesh(rightShoulderGeometry, shoulderMaterial);
  rightShoulder.name = "scenic-route-right-shoulder";

  const leftEdge = new THREE.Mesh(leftEdgeGeometry, edgeMaterial);
  leftEdge.name = "scenic-route-left-edge";

  const rightEdge = new THREE.Mesh(rightEdgeGeometry, edgeMaterial);
  rightEdge.name = "scenic-route-right-edge";

  const centerMarkings = createRepeatedRouteMarkers(
    frames,
    dashGeometry,
    dashMaterial,
    "scenic-route-center-markings",
    26,
    38,
    0
  );
  const surfaceBands = createRepeatedRouteMarkers(
    frames,
    surfaceBandGeometry,
    surfaceBandMaterial,
    "scenic-route-surface-bands",
    12,
    47,
    0.12
  );
  const shoulderRhythm = createMirroredRouteMarkers(
    frames,
    shoulderRhythmGeometry,
    shoulderRhythmMaterial,
    "scenic-route-shoulder-rhythm",
    18,
    29,
    ROUTE_WIDTH_METERS / 2 + 2.55
  );
  const surfaceGrain = createAlternatingRouteMarkers(
    frames,
    surfaceGrainGeometry,
    surfaceGrainMaterial,
    "scenic-route-surface-grain",
    8,
    22,
    1.35
  );

  group.add(
    leftShoulder,
    rightShoulder,
    roadMesh,
    leftEdge,
    rightEdge,
    centerMarkings,
    surfaceBands,
    shoulderRhythm,
    surfaceGrain
  );

  return {
    group,
    mesh: roadMesh,
    geometry: roadGeometry,
    material: roadMaterial,
    geometries: [
      roadGeometry,
      leftShoulderGeometry,
      rightShoulderGeometry,
      leftEdgeGeometry,
      rightEdgeGeometry,
      dashGeometry,
      surfaceBandGeometry,
      shoulderRhythmGeometry,
      surfaceGrainGeometry
    ],
    materials: [
      roadMaterial,
      shoulderMaterial,
      edgeMaterial,
      dashMaterial,
      surfaceBandMaterial,
      shoulderRhythmMaterial,
      surfaceGrainMaterial
    ]
  };
}

type RouteRenderFrame = {
  center: THREE.Vector3;
  tangent: THREE.Vector3;
  perpendicular: THREE.Vector3;
  distanceMeters: number;
};

function getRouteFrames(route: RouteDefinition): RouteRenderFrame[] {
  const { route: resolvedRoute } = resolveRouteDefinition(route);
  const points = resolvedRoute.points.map((point) => ({
    center: toThreeVector3(point.position, 0.04),
    distanceMeters: point.distanceMeters
  }));

  return points.map((point, index) => {
    const previous = points[Math.max(0, index - 1)].center;
    const next = points[Math.min(points.length - 1, index + 1)].center;
    const tangent = next.clone().sub(previous);
    const perpendicular = new THREE.Vector3(-tangent.z, 0, tangent.x);

    if (tangent.lengthSq() === 0) {
      tangent.set(0, 0, 1);
    }

    if (perpendicular.lengthSq() === 0) {
      perpendicular.set(1, 0, 0);
    }

    return {
      center: point.center,
      tangent: tangent.normalize(),
      perpendicular: perpendicular.normalize(),
      distanceMeters: point.distanceMeters
    };
  });
}

function createRibbonGeometry(
  frames: RouteRenderFrame[],
  halfWidth: number
): THREE.BufferGeometry {
  return createSideRibbonGeometry(frames, halfWidth, halfWidth, 0);
}

function createSideRibbonGeometry(
  frames: RouteRenderFrame[],
  innerOffset: number,
  outerOffset: number,
  side: -1 | 0 | 1
): THREE.BufferGeometry {
  const vertices: number[] = [];
  const indices: number[] = [];

  for (let index = 0; index < frames.length; index += 1) {
    const frame = frames[index];
    const left =
      side === 0
        ? frame.center.clone().addScaledVector(frame.perpendicular, innerOffset)
        : frame.center
            .clone()
            .addScaledVector(frame.perpendicular, innerOffset * side);
    const right =
      side === 0
        ? frame.center.clone().addScaledVector(frame.perpendicular, -innerOffset)
        : frame.center
            .clone()
            .addScaledVector(frame.perpendicular, outerOffset * side);

    vertices.push(left.x, left.y, left.z, right.x, right.y, right.z);

    if (index < frames.length - 1) {
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

  return geometry;
}

function createRepeatedRouteMarkers(
  frames: RouteRenderFrame[],
  geometry: THREE.BufferGeometry,
  material: THREE.Material,
  groupName: string,
  startMeters: number,
  stepMeters: number,
  lateralOffset: number
): THREE.Group {
  const group = new THREE.Group();
  group.name = groupName;
  const lastDistance = frames.at(-1)?.distanceMeters ?? 0;

  for (
    let distanceMeters = startMeters;
    distanceMeters < lastDistance - 20;
    distanceMeters += stepMeters
  ) {
    const frame = sampleFrameAtDistance(frames, distanceMeters);
    const marker = new THREE.Mesh(geometry, material);
    marker.position
      .copy(frame.center)
      .addScaledVector(frame.perpendicular, lateralOffset);
    marker.position.y += 0.045;
    marker.rotation.y = Math.atan2(frame.tangent.x, frame.tangent.z);
    group.add(marker);
  }

  return group;
}

function createMirroredRouteMarkers(
  frames: RouteRenderFrame[],
  geometry: THREE.BufferGeometry,
  material: THREE.Material,
  groupName: string,
  startMeters: number,
  stepMeters: number,
  lateralOffset: number
): THREE.Group {
  const group = new THREE.Group();
  group.name = groupName;
  const lastDistance = frames.at(-1)?.distanceMeters ?? 0;

  for (
    let distanceMeters = startMeters;
    distanceMeters < lastDistance - 20;
    distanceMeters += stepMeters
  ) {
    const frame = sampleFrameAtDistance(frames, distanceMeters);

    for (const side of [-1, 1] as const) {
      const marker = new THREE.Mesh(geometry, material);
      marker.position
        .copy(frame.center)
        .addScaledVector(frame.perpendicular, lateralOffset * side);
      marker.position.y += 0.052;
      marker.rotation.y = Math.atan2(frame.tangent.x, frame.tangent.z);
      marker.rotation.z = side * 0.04;
      group.add(marker);
    }
  }

  return group;
}

function createAlternatingRouteMarkers(
  frames: RouteRenderFrame[],
  geometry: THREE.BufferGeometry,
  material: THREE.Material,
  groupName: string,
  startMeters: number,
  stepMeters: number,
  maxLateralOffset: number
): THREE.Group {
  const group = new THREE.Group();
  group.name = groupName;
  const lastDistance = frames.at(-1)?.distanceMeters ?? 0;
  let markerIndex = 0;

  for (
    let distanceMeters = startMeters;
    distanceMeters < lastDistance - 20;
    distanceMeters += stepMeters
  ) {
    const frame = sampleFrameAtDistance(frames, distanceMeters);
    const side = markerIndex % 2 === 0 ? -1 : 1;
    const lateralOffset =
      side * (0.35 + (markerIndex % 4) * ((maxLateralOffset - 0.35) / 3));
    const marker = new THREE.Mesh(geometry, material);
    marker.position
      .copy(frame.center)
      .addScaledVector(frame.perpendicular, lateralOffset);
    marker.position.y += 0.052;
    marker.rotation.y =
      Math.atan2(frame.tangent.x, frame.tangent.z) +
      (markerIndex % 3 === 0 ? 0.07 : -0.04);
    marker.scale.set(0.76 + (markerIndex % 5) * 0.06, 1, 1);
    group.add(marker);
    markerIndex += 1;
  }

  return group;
}

function sampleFrameAtDistance(
  frames: RouteRenderFrame[],
  distanceMeters: number
): RouteRenderFrame {
  for (let index = 0; index < frames.length - 1; index += 1) {
    const current = frames[index];
    const next = frames[index + 1];

    if (
      distanceMeters >= current.distanceMeters &&
      distanceMeters <= next.distanceMeters
    ) {
      const segmentLength = next.distanceMeters - current.distanceMeters || 1;
      const progress = (distanceMeters - current.distanceMeters) / segmentLength;
      const center = current.center.clone().lerp(next.center, progress);
      const tangent = next.center.clone().sub(current.center);

      if (tangent.lengthSq() === 0) {
        tangent.set(0, 0, 1);
      }

      tangent.normalize();
      const perpendicular = new THREE.Vector3(-tangent.z, 0, tangent.x);

      if (perpendicular.lengthSq() === 0) {
        perpendicular.set(1, 0, 0);
      }

      return {
        center,
        tangent,
        perpendicular: perpendicular.normalize(),
        distanceMeters
      };
    }
  }

  return frames.at(-1) ?? frames[0];
}

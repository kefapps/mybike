import {
  BoxGeometry
} from "three/src/geometries/BoxGeometry.js";
import { BufferGeometry } from "three/src/core/BufferGeometry.js";
import { DoubleSide } from "three/src/constants.js";
import { Float32BufferAttribute } from "three/src/core/BufferAttribute.js";
import { Material } from "three/src/materials/Material.js";
import { Group } from "three/src/objects/Group.js";
import { Mesh } from "three/src/objects/Mesh.js";
import { MeshStandardMaterial } from "three/src/materials/MeshStandardMaterial.js";
import { Vector3 } from "three/src/math/Vector3.js";

import {
  resolveRouteDefinition,
  sampleRouteAt,
  type RouteDefinition
} from "../route";
import { toThreeVector3 } from "./renderHelpers";

const ROUTE_WIDTH_METERS = 5;
const SHOULDER_WIDTH_METERS = 3.8;
const VERGE_WIDTH_METERS = 2.9;
const CONTACT_SHADOW_WIDTH_METERS = 0.55;
const ROUTE_FRAME_STEP_METERS = 9;

export type RouteMeshResult = {
  group: Group;
  mesh: Mesh;
  geometry: BufferGeometry;
  material: MeshStandardMaterial;
  geometries: BufferGeometry[];
  materials: Material[];
};

export function createRouteMesh(route: RouteDefinition): RouteMeshResult {
  const frames = getRouteFrames(route);
  const group = new Group();
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
  const leftVergeGeometry = createSideRibbonGeometry(
    frames,
    ROUTE_WIDTH_METERS / 2 + SHOULDER_WIDTH_METERS + 0.22,
    ROUTE_WIDTH_METERS / 2 + SHOULDER_WIDTH_METERS + VERGE_WIDTH_METERS,
    1
  );
  const rightVergeGeometry = createSideRibbonGeometry(
    frames,
    ROUTE_WIDTH_METERS / 2 + SHOULDER_WIDTH_METERS + 0.22,
    ROUTE_WIDTH_METERS / 2 + SHOULDER_WIDTH_METERS + VERGE_WIDTH_METERS,
    -1
  );
  const leftContactShadowGeometry = createSideRibbonGeometry(
    frames,
    ROUTE_WIDTH_METERS / 2 + 0.04,
    ROUTE_WIDTH_METERS / 2 + CONTACT_SHADOW_WIDTH_METERS,
    1
  );
  const rightContactShadowGeometry = createSideRibbonGeometry(
    frames,
    ROUTE_WIDTH_METERS / 2 + 0.04,
    ROUTE_WIDTH_METERS / 2 + CONTACT_SHADOW_WIDTH_METERS,
    -1
  );
  leftVergeGeometry.translate(0, 0.012, 0);
  rightVergeGeometry.translate(0, 0.012, 0);
  leftContactShadowGeometry.translate(0, 0.028, 0);
  rightContactShadowGeometry.translate(0, 0.028, 0);
  const dashGeometry = new BoxGeometry(0.18, 0.035, 7);
  const surfaceBandGeometry = new BoxGeometry(
    ROUTE_WIDTH_METERS - 0.45,
    0.028,
    0.42
  );
  const shoulderRhythmGeometry = new BoxGeometry(0.46, 0.035, 2.4);
  const surfaceGrainGeometry = new BoxGeometry(0.72, 0.024, 0.18);

  const roadMaterial = new MeshStandardMaterial({
    color: 0xf2d58b,
    roughness: 0.9,
    metalness: 0,
    side: DoubleSide
  });
  const shoulderMaterial = new MeshStandardMaterial({
    color: 0xb5b36f,
    roughness: 0.96,
    metalness: 0,
    side: DoubleSide
  });
  const edgeMaterial = new MeshStandardMaterial({
    color: 0xfff3c2,
    roughness: 0.7,
    metalness: 0,
    side: DoubleSide
  });
  const vergeMaterial = new MeshStandardMaterial({
    color: 0x5b7d4a,
    roughness: 0.98,
    metalness: 0,
    side: DoubleSide,
    transparent: true,
    opacity: 0.78
  });
  const contactShadowMaterial = new MeshStandardMaterial({
    color: 0x2d2618,
    roughness: 1,
    metalness: 0,
    side: DoubleSide,
    transparent: true,
    opacity: 0.32,
    depthWrite: false
  });
  const dashMaterial = new MeshStandardMaterial({
    color: 0xfdf4d0,
    roughness: 0.74,
    metalness: 0
  });
  const surfaceBandMaterial = new MeshStandardMaterial({
    color: 0xd7b96d,
    roughness: 0.95,
    metalness: 0,
    transparent: true,
    opacity: 0.42
  });
  const shoulderRhythmMaterial = new MeshStandardMaterial({
    color: 0xe8d793,
    roughness: 0.9,
    metalness: 0,
    transparent: true,
    opacity: 0.62
  });
  const surfaceGrainMaterial = new MeshStandardMaterial({
    color: 0xa88345,
    roughness: 0.98,
    metalness: 0,
    transparent: true,
    opacity: 0.34
  });

  const roadMesh = new Mesh(roadGeometry, roadMaterial);
  roadMesh.name = "scenic-route-surface";

  const leftShoulder = new Mesh(leftShoulderGeometry, shoulderMaterial);
  leftShoulder.name = "scenic-route-left-shoulder";

  const rightShoulder = new Mesh(rightShoulderGeometry, shoulderMaterial);
  rightShoulder.name = "scenic-route-right-shoulder";

  const leftEdge = new Mesh(leftEdgeGeometry, edgeMaterial);
  leftEdge.name = "scenic-route-left-edge";

  const rightEdge = new Mesh(rightEdgeGeometry, edgeMaterial);
  rightEdge.name = "scenic-route-right-edge";

  const leftVerge = new Mesh(leftVergeGeometry, vergeMaterial);
  leftVerge.name = "scenic-route-left-verge";

  const rightVerge = new Mesh(rightVergeGeometry, vergeMaterial);
  rightVerge.name = "scenic-route-right-verge";

  const leftContactShadow = new Mesh(
    leftContactShadowGeometry,
    contactShadowMaterial
  );
  leftContactShadow.name = "scenic-route-left-contact-shadow";
  leftContactShadow.renderOrder = 1;

  const rightContactShadow = new Mesh(
    rightContactShadowGeometry,
    contactShadowMaterial
  );
  rightContactShadow.name = "scenic-route-right-contact-shadow";
  rightContactShadow.renderOrder = 1;

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
    leftVerge,
    rightVerge,
    leftShoulder,
    rightShoulder,
    leftContactShadow,
    rightContactShadow,
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
      leftVergeGeometry,
      rightVergeGeometry,
      leftContactShadowGeometry,
      rightContactShadowGeometry,
      dashGeometry,
      surfaceBandGeometry,
      shoulderRhythmGeometry,
      surfaceGrainGeometry
    ],
    materials: [
      roadMaterial,
      shoulderMaterial,
      edgeMaterial,
      vergeMaterial,
      contactShadowMaterial,
      dashMaterial,
      surfaceBandMaterial,
      shoulderRhythmMaterial,
      surfaceGrainMaterial
    ]
  };
}

type RouteRenderFrame = {
  center: Vector3;
  tangent: Vector3;
  perpendicular: Vector3;
  distanceMeters: number;
};

function getRouteFrames(route: RouteDefinition): RouteRenderFrame[] {
  const { route: resolvedRoute } = resolveRouteDefinition(route);
  const points: { center: Vector3; distanceMeters: number }[] = [];

  for (
    let distanceMeters = 0;
    distanceMeters < resolvedRoute.lengthMeters;
    distanceMeters += ROUTE_FRAME_STEP_METERS
  ) {
    const sample = sampleRouteAt(
      resolvedRoute,
      distanceMeters / resolvedRoute.lengthMeters
    );
    points.push({
      center: toThreeVector3(sample.position, 0.04),
      distanceMeters
    });
  }

  const endSample = sampleRouteAt(resolvedRoute, 1);
  points.push({
    center: toThreeVector3(endSample.position, 0.04),
    distanceMeters: resolvedRoute.lengthMeters
  });

  return points.map((point, index) => {
    const previous = points[Math.max(0, index - 1)].center;
    const next = points[Math.min(points.length - 1, index + 1)].center;
    const tangent = next.clone().sub(previous);
    const perpendicular = new Vector3(-tangent.z, 0, tangent.x);

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
): BufferGeometry {
  return createSideRibbonGeometry(frames, halfWidth, halfWidth, 0);
}

function createSideRibbonGeometry(
  frames: RouteRenderFrame[],
  innerOffset: number,
  outerOffset: number,
  side: -1 | 0 | 1
): BufferGeometry {
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

  const geometry = new BufferGeometry();
  geometry.setAttribute(
    "position",
    new Float32BufferAttribute(vertices, 3)
  );
  geometry.setIndex(indices);
  geometry.computeVertexNormals();

  return geometry;
}

function createRepeatedRouteMarkers(
  frames: RouteRenderFrame[],
  geometry: BufferGeometry,
  material: Material,
  groupName: string,
  startMeters: number,
  stepMeters: number,
  lateralOffset: number
): Group {
  const group = new Group();
  group.name = groupName;
  const lastDistance = frames.at(-1)?.distanceMeters ?? 0;

  for (
    let distanceMeters = startMeters;
    distanceMeters < lastDistance - 20;
    distanceMeters += stepMeters
  ) {
    const frame = sampleFrameAtDistance(frames, distanceMeters);
    const marker = new Mesh(geometry, material);
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
  geometry: BufferGeometry,
  material: Material,
  groupName: string,
  startMeters: number,
  stepMeters: number,
  lateralOffset: number
): Group {
  const group = new Group();
  group.name = groupName;
  const lastDistance = frames.at(-1)?.distanceMeters ?? 0;

  for (
    let distanceMeters = startMeters;
    distanceMeters < lastDistance - 20;
    distanceMeters += stepMeters
  ) {
    const frame = sampleFrameAtDistance(frames, distanceMeters);

    for (const side of [-1, 1] as const) {
      const marker = new Mesh(geometry, material);
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
  geometry: BufferGeometry,
  material: Material,
  groupName: string,
  startMeters: number,
  stepMeters: number,
  maxLateralOffset: number
): Group {
  const group = new Group();
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
    const marker = new Mesh(geometry, material);
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
      const perpendicular = new Vector3(-tangent.z, 0, tangent.x);

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

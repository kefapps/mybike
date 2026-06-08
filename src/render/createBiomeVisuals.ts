import * as THREE from "three";

import { mockRouteDefinition, type RouteDefinition } from "../route";
import { calculateGroundHeight, GROUND_MESH_CENTER_Z } from "./groundHeight";
import { getRouteCenterXAtZ } from "./renderHelpers";

type FallbackTintMaterial = {
  material: THREE.MeshStandardMaterial;
  defaultColor: number;
  fallbackColor: number;
};

export type BiomeVisuals = {
  group: THREE.Group;
  geometries: THREE.BufferGeometry[];
  materials: THREE.Material[];
  coastMaterial: THREE.MeshStandardMaterial;
  forestMaterial: THREE.MeshStandardMaterial;
  fallbackTintMaterials: FallbackTintMaterial[];
};

export function createBiomeVisuals(
  route: RouteDefinition = mockRouteDefinition
): BiomeVisuals {
  const group = new THREE.Group();
  group.name = "scenic-biome-visuals";

  const geometries: THREE.BufferGeometry[] = [];
  const materials: THREE.Material[] = [];
  const routeSideX = (z: number, side: -1 | 1, offset: number): number =>
    getRouteCenterXAtZ(route, z) + side * offset;

  const coastGeometry = new THREE.BoxGeometry(0.65, 2.4, 0.65);
  const trunkGeometry = new THREE.CylinderGeometry(0.55, 0.75, 5, 8);
  const canopyGeometry = new THREE.ConeGeometry(3.2, 8, 8);
  const grassGeometry = new THREE.ConeGeometry(0.32, 1.55, 5);
  const rockGeometry = new THREE.DodecahedronGeometry(1, 0);
  const signPlateGeometry = new THREE.BoxGeometry(3.6, 1.4, 0.22);
  const signPostGeometry = new THREE.CylinderGeometry(0.13, 0.13, 3, 8);
  const farTreeGeometry = new THREE.ConeGeometry(4.8, 12, 7);
  const farHillGeometry = new THREE.BoxGeometry(36, 6, 18);
  const lighthouseBaseGeometry = new THREE.CylinderGeometry(
    1.25,
    1.75,
    10,
    10
  );
  const lighthouseStripeGeometry = new THREE.CylinderGeometry(
    1.34,
    1.52,
    0.52,
    10
  );
  const lighthouseLanternGeometry = new THREE.CylinderGeometry(
    1.75,
    1.95,
    1.6,
    10
  );
  const lighthouseBeaconGeometry = new THREE.BoxGeometry(3.8, 0.85, 3.8);
  const lighthouseRoofGeometry = new THREE.ConeGeometry(2.35, 2.1, 10);
  const archPillarGeometry = new THREE.BoxGeometry(4.6, 12, 4.4);
  const archBridgeGeometry = new THREE.BoxGeometry(15.5, 4.2, 4.6);
  const bermGeometry = new THREE.BoxGeometry(18, 2.4, 26);
  const ridgeGeometry = new THREE.BoxGeometry(56, 8, 30);
  const canopyBridgeGeometry = new THREE.BoxGeometry(26, 4.2, 5.4);
  const nearPostGeometry = new THREE.BoxGeometry(0.24, 2.1, 0.24);
  const nearBladeGeometry = new THREE.ConeGeometry(0.16, 1.25, 4);
  const nearStoneGeometry = new THREE.DodecahedronGeometry(0.45, 0);
  const coastReedGeometry = new THREE.BoxGeometry(0.12, 1.85, 0.12);
  const forestFernGeometry = new THREE.ConeGeometry(0.42, 1.15, 6);
  const birdWingGeometry = new THREE.BoxGeometry(1.8, 0.08, 0.18);
  const birdBodyGeometry = new THREE.BoxGeometry(0.5, 0.18, 0.5);
  const humanBodyGeometry = new THREE.BoxGeometry(0.62, 1.65, 0.28);
  const humanHeadGeometry = new THREE.SphereGeometry(0.3, 10, 8);
  const humanBaseGeometry = new THREE.BoxGeometry(0.86, 0.1, 0.42);
  const humanVestGeometry = new THREE.BoxGeometry(0.68, 0.34, 0.3);

  const coastMaterial = new THREE.MeshStandardMaterial({
    color: 0x4f9fcf,
    roughness: 0.72
  });
  const forestMaterial = new THREE.MeshStandardMaterial({
    color: 0x174d2e,
    roughness: 0.82
  });
  const trunkMaterial = new THREE.MeshStandardMaterial({
    color: 0x6d4c30,
    roughness: 0.9
  });
  const grassMaterial = new THREE.MeshStandardMaterial({
    color: 0x6aa35f,
    roughness: 0.94
  });
  const rockMaterial = new THREE.MeshStandardMaterial({
    color: 0x7f8178,
    roughness: 0.88
  });
  const signMaterial = new THREE.MeshStandardMaterial({
    color: 0xf8f0c8,
    roughness: 0.68
  });
  const distantMaterial = new THREE.MeshStandardMaterial({
    color: 0x315c46,
    roughness: 0.98
  });
  const lighthouseMaterial = new THREE.MeshStandardMaterial({
    color: 0xf4efd9,
    roughness: 0.6
  });
  const beaconMaterial = new THREE.MeshStandardMaterial({
    color: 0xffdc7a,
    emissive: 0xffc75f,
    emissiveIntensity: 0.24,
    roughness: 0.35
  });
  const lighthouseRoofMaterial = new THREE.MeshStandardMaterial({
    color: 0x35515b,
    roughness: 0.72
  });
  const rockArchMaterial = new THREE.MeshStandardMaterial({
    color: 0x8a8f86,
    roughness: 0.9
  });
  const bermMaterial = new THREE.MeshStandardMaterial({
    color: 0x6f9b68,
    roughness: 0.96
  });
  const ridgeMaterial = new THREE.MeshStandardMaterial({
    color: 0x486f5d,
    roughness: 0.98
  });
  const nearPostMaterial = new THREE.MeshStandardMaterial({
    color: 0xf7e8b6,
    roughness: 0.74
  });
  const nearBladeMaterial = new THREE.MeshStandardMaterial({
    color: 0x89b66c,
    roughness: 0.96
  });
  const nearStoneMaterial = new THREE.MeshStandardMaterial({
    color: 0x696d66,
    roughness: 0.9
  });
  const coastReedMaterial = new THREE.MeshStandardMaterial({
    color: 0xb58d55,
    roughness: 0.9
  });
  const forestFernMaterial = new THREE.MeshStandardMaterial({
    color: 0x2f7a42,
    roughness: 0.94
  });
  const birdWingMaterial = new THREE.MeshBasicMaterial({
    color: 0x19272d,
    fog: true
  });
  const birdBodyMaterial = new THREE.MeshBasicMaterial({
    color: 0xf2e8c4,
    fog: true
  });
  const humanBodyMaterial = new THREE.MeshBasicMaterial({
    color: 0x202825,
    fog: true
  });
  const humanHeadMaterial = new THREE.MeshBasicMaterial({
    color: 0xe8d0a8,
    fog: true
  });
  const humanVestMaterial = new THREE.MeshBasicMaterial({
    color: 0xf2d36b,
    fog: true
  });

  geometries.push(
    coastGeometry,
    trunkGeometry,
    canopyGeometry,
    grassGeometry,
    rockGeometry,
    signPlateGeometry,
    signPostGeometry,
    farTreeGeometry,
    farHillGeometry,
    lighthouseBaseGeometry,
    lighthouseStripeGeometry,
    lighthouseLanternGeometry,
    lighthouseBeaconGeometry,
    lighthouseRoofGeometry,
    archPillarGeometry,
    archBridgeGeometry,
    bermGeometry,
    ridgeGeometry,
    canopyBridgeGeometry,
    nearPostGeometry,
    nearBladeGeometry,
    nearStoneGeometry,
    coastReedGeometry,
    forestFernGeometry,
    birdWingGeometry,
    birdBodyGeometry,
    humanBodyGeometry,
    humanHeadGeometry,
    humanBaseGeometry,
    humanVestGeometry
  );
  materials.push(
    coastMaterial,
    forestMaterial,
    trunkMaterial,
    grassMaterial,
    rockMaterial,
    signMaterial,
    distantMaterial,
    lighthouseMaterial,
    beaconMaterial,
    lighthouseRoofMaterial,
    rockArchMaterial,
    bermMaterial,
    ridgeMaterial,
    nearPostMaterial,
    nearBladeMaterial,
    nearStoneMaterial,
    coastReedMaterial,
    forestFernMaterial,
    birdWingMaterial,
    birdBodyMaterial,
    humanBodyMaterial,
    humanHeadMaterial,
    humanVestMaterial
  );
  const fallbackTintMaterials: FallbackTintMaterial[] = [
    {
      material: lighthouseMaterial,
      defaultColor: 0xf4efd9,
      fallbackColor: 0xa8afb2
    },
    {
      material: beaconMaterial,
      defaultColor: 0xffdc7a,
      fallbackColor: 0x879096
    },
    {
      material: lighthouseRoofMaterial,
      defaultColor: 0x35515b,
      fallbackColor: 0x69757b
    },
    {
      material: rockArchMaterial,
      defaultColor: 0x8a8f86,
      fallbackColor: 0x7d8586
    },
    {
      material: bermMaterial,
      defaultColor: 0x6f9b68,
      fallbackColor: 0x7d8882
    },
    {
      material: ridgeMaterial,
      defaultColor: 0x486f5d,
      fallbackColor: 0x66746f
    }
  ];

  const nearRoadMotionDetails = new THREE.Group();
  nearRoadMotionDetails.name = "scenic-near-road-motion-details";

  for (const z of [
    24, 74, 124, 174, 224, 274, 324, 374, 424, 474, 524, 574, 624, 674, 724,
    774, 824, 874, 924, 974
  ]) {
    for (const side of [-1, 1] as const) {
      const post = new THREE.Mesh(nearPostGeometry, nearPostMaterial);
      post.name = "scenic-near-speed-post";
      post.position.set(routeSideX(z, side, 6.35 + (z % 5) * 0.12), 1.05, z);
      post.scale.set(1, 0.82 + (z % 4) * 0.04, 1);
      post.rotation.set(0.04 * side, side * 0.08, -0.03 * side);
      nearRoadMotionDetails.add(post);
    }
  }

  for (const z of [
    18, 38, 58, 82, 108, 134, 162, 190, 222, 256, 292, 330, 370, 412, 456,
    502, 550, 600, 652, 706, 762, 820, 880, 942
  ]) {
    for (const side of [-1, 1] as const) {
      const blade = new THREE.Mesh(nearBladeGeometry, nearBladeMaterial);
      blade.name = "scenic-near-edge-grass";
      blade.position.set(routeSideX(z, side, 7.4 + (z % 7) * 0.18), 0.62, z);
      blade.scale.set(0.72 + (z % 3) * 0.1, 0.86 + (z % 5) * 0.07, 0.72);
      blade.rotation.set(0.08 * side, z * 0.041, 0.06 * side);
      nearRoadMotionDetails.add(blade);
    }
  }

  for (const z of [
    42, 118, 188, 268, 342, 428, 516, 612, 710, 812, 916
  ]) {
    for (const side of [-1, 1] as const) {
      const stone = new THREE.Mesh(nearStoneGeometry, nearStoneMaterial);
      stone.name = "scenic-near-edge-stone";
      stone.position.set(routeSideX(z, side, 10.2 + (z % 9) * 0.42), 0.36, z);
      stone.scale.set(1.15 + (z % 4) * 0.18, 0.46, 0.72 + (z % 5) * 0.09);
      stone.rotation.set(0.1, z * 0.019, -0.12 * side);
      nearRoadMotionDetails.add(stone);
    }
  }

  for (const z of [36, 66, 98, 132, 168]) {
    for (const side of [-1, 1] as const) {
      const reed = new THREE.Mesh(coastReedGeometry, coastReedMaterial);
      reed.name = "scenic-coast-motion-reed";
      reed.position.set(routeSideX(z, side, 11.1 + (z % 4) * 0.28), 0.92, z);
      reed.scale.set(1, 0.82 + (z % 5) * 0.08, 1);
      reed.rotation.set(0.08 * side, side * 0.14, 0.11 * side);
      nearRoadMotionDetails.add(reed);
    }
  }

  for (const z of [248, 286, 326, 368, 414, 462, 516, 574, 640, 712]) {
    for (const side of [-1, 1] as const) {
      const fern = new THREE.Mesh(forestFernGeometry, forestFernMaterial);
      fern.name = "scenic-forest-motion-fern";
      fern.position.set(routeSideX(z, side, 8.6 + (z % 6) * 0.22), 0.58, z);
      fern.scale.set(0.78 + (z % 4) * 0.06, 0.78 + (z % 5) * 0.08, 0.78);
      fern.rotation.set(0.1 * side, z * 0.031, -0.07 * side);
      nearRoadMotionDetails.add(fern);
    }
  }

  group.add(nearRoadMotionDetails);

  const sceneLife = new THREE.Group();
  sceneLife.name = "scenic-life";

  for (const [z, sideOffset, height, scale] of [
    [54, -5.4, 8.6, 1.08],
    [112, 7.2, 10.8, 1.18],
    [205, -8.8, 9.4, 1.28],
    [336, 6.5, 12.2, 1.36],
    [516, -3.6, 11.2, 1.24]
  ] as const) {
    const bird = new THREE.Group();
    bird.name = "scenic-life-bird";
    bird.position.set(getRouteCenterXAtZ(route, z) + sideOffset, height, z);
    bird.scale.setScalar(scale);
    bird.rotation.set(0.08, sideOffset < 0 ? 0.22 : -0.18, sideOffset * 0.012);

    const leftWing = new THREE.Mesh(birdWingGeometry, birdWingMaterial);
    leftWing.name = "scenic-life-bird-wing";
    leftWing.position.set(-0.72, 0, 0);
    leftWing.rotation.z = -0.34;

    const rightWing = new THREE.Mesh(birdWingGeometry, birdWingMaterial);
    rightWing.name = "scenic-life-bird-wing";
    rightWing.position.set(0.72, 0, 0);
    rightWing.rotation.z = 0.34;

    const body = new THREE.Mesh(birdBodyGeometry, birdBodyMaterial);
    body.name = "scenic-life-bird-body";
    body.scale.set(0.75, 0.9, 0.72);

    bird.add(leftWing, rightWing, body);
    sceneLife.add(bird);
  }

  for (const [z, side, offset, scale] of [
    [96, -1, 7.4, 1.18],
    [184, 1, 7.8, 1.3],
    [332, -1, 8.4, 1.42]
  ] as const) {
    const x = routeSideX(z, side, offset);
    const spectator = new THREE.Group();
    spectator.name = "scenic-life-human";
    spectator.position.set(
      x,
      calculateGroundHeight(route, x, GROUND_MESH_CENTER_Z - z) + 0.02,
      z
    );
    spectator.scale.setScalar(scale);
    spectator.rotation.y = side < 0 ? 0.34 : -0.34;

    const base = new THREE.Mesh(humanBaseGeometry, humanBodyMaterial);
    base.name = "scenic-life-human-base";
    base.position.y = 0.04;

    const body = new THREE.Mesh(humanBodyGeometry, humanBodyMaterial);
    body.name = "scenic-life-human-body";
    body.position.y = 0.82;

    const head = new THREE.Mesh(humanHeadGeometry, humanHeadMaterial);
    head.name = "scenic-life-human-head";
    head.position.y = 1.92;

    const vest = new THREE.Mesh(humanVestGeometry, humanVestMaterial);
    vest.name = "scenic-life-human-vest";
    vest.position.y = 1.18;

    spectator.add(base, body, vest, head);
    sceneLife.add(spectator);
  }

  group.add(sceneLife);

  for (const z of [52, 86, 122, 158, 196, 236, 278]) {
    for (const side of [-1, 1] as const) {
      const marker = new THREE.Mesh(coastGeometry, coastMaterial);
      marker.name = "scenic-coast-post";
      marker.position.set(routeSideX(z, side, 12.5), 1.2, z);
      marker.rotation.y = side < 0 ? -0.16 : 0.16;
      group.add(marker);
    }
  }

  const lighthouse = new THREE.Group();
  lighthouse.name = "scenic-coast-lighthouse";
  lighthouse.position.set(routeSideX(138, -1, 74), 0, 138);
  lighthouse.rotation.y = 0.22;
  lighthouse.scale.setScalar(1.35);

  const lighthouseBase = new THREE.Mesh(
    lighthouseBaseGeometry,
    lighthouseMaterial
  );
  lighthouseBase.name = "scenic-coast-lighthouse-body";
  lighthouseBase.position.y = 5;

  const lighthouseLowerStripe = new THREE.Mesh(
    lighthouseStripeGeometry,
    coastMaterial
  );
  lighthouseLowerStripe.name = "scenic-coast-lighthouse-stripe";
  lighthouseLowerStripe.position.y = 3.4;

  const lighthouseUpperStripe = new THREE.Mesh(
    lighthouseStripeGeometry,
    coastMaterial
  );
  lighthouseUpperStripe.name = "scenic-coast-lighthouse-stripe";
  lighthouseUpperStripe.position.y = 7.2;

  const lighthouseLantern = new THREE.Mesh(
    lighthouseLanternGeometry,
    lighthouseMaterial
  );
  lighthouseLantern.name = "scenic-coast-lighthouse-lantern";
  lighthouseLantern.position.y = 10.9;

  const lighthouseBeacon = new THREE.Mesh(
    lighthouseBeaconGeometry,
    beaconMaterial
  );
  lighthouseBeacon.name = "scenic-coast-lighthouse-beacon";
  lighthouseBeacon.position.y = 10.9;

  const lighthouseRoof = new THREE.Mesh(
    lighthouseRoofGeometry,
    lighthouseRoofMaterial
  );
  lighthouseRoof.name = "scenic-coast-lighthouse-roof";
  lighthouseRoof.position.y = 12.65;

  lighthouse.add(
    lighthouseBase,
    lighthouseLowerStripe,
    lighthouseUpperStripe,
    lighthouseLantern,
    lighthouseBeacon,
    lighthouseRoof
  );
  group.add(lighthouse);

  const rockArch = new THREE.Group();
  rockArch.name = "scenic-coast-rock-arch";
  rockArch.position.set(routeSideX(185, 1, 32), 0, 185);
  rockArch.rotation.y = -0.26;
  rockArch.scale.setScalar(1.12);

  for (const x of [-5.2, 5.2]) {
    const pillar = new THREE.Mesh(archPillarGeometry, rockArchMaterial);
    pillar.name = "scenic-coast-rock-arch-pillar";
    pillar.position.set(x, 6, 0);
    pillar.rotation.z = x < 0 ? -0.08 : 0.08;
    rockArch.add(pillar);
  }

  const archBridge = new THREE.Mesh(archBridgeGeometry, rockArchMaterial);
  archBridge.name = "scenic-coast-rock-arch-bridge";
  archBridge.position.set(0, 13.2, 0);

  const archCap = new THREE.Mesh(rockGeometry, rockArchMaterial);
  archCap.name = "scenic-coast-rock-arch-cap";
  archCap.position.set(0, 15.3, 0);
  archCap.scale.set(4.8, 1.2, 2.2);
  archCap.rotation.y = 0.3;

  rockArch.add(archBridge, archCap);
  group.add(rockArch);

  for (const z of [70, 138, 218, 305, 394, 492, 604, 724, 848, 976]) {
    for (const side of [-1, 1] as const) {
      const berm = new THREE.Mesh(bermGeometry, bermMaterial);
      berm.name = "scenic-near-berm";
      berm.position.set(routeSideX(z, side, 30 + (z % 4) * 1.7), 1.15, z);
      berm.scale.set(
        0.72 + (z % 5) * 0.05,
        0.82,
        0.82 + (z % 7) * 0.035
      );
      berm.rotation.set(
        0.02 * side,
        side * (0.16 + (z % 3) * 0.04),
        -0.04 * side
      );
      group.add(berm);
    }
  }

  for (const z of [165, 325, 505, 665, 825, 1005]) {
    for (const side of [-1, 1] as const) {
      const ridge = new THREE.Mesh(ridgeGeometry, ridgeMaterial);
      ridge.name = "scenic-terrain-ridge";
      ridge.position.set(routeSideX(z, side, 88 + (z % 6) * 3), 4.2, z);
      ridge.scale.set(0.82 + (z % 5) * 0.08, 0.7 + (z % 4) * 0.08, 1);
      ridge.rotation.y = side * (0.24 + (z % 4) * 0.03);
      group.add(ridge);
    }
  }

  for (const z of [
    28, 46, 64, 82, 104, 126, 148, 172, 196, 222, 252, 286, 325, 366, 410, 456,
    504, 554, 606, 660, 716, 774, 834, 896, 956
  ]) {
    for (const side of [-1, 1] as const) {
      const offset = 8.5 + ((z / 23) % 4);
      const tuft = new THREE.Mesh(grassGeometry, grassMaterial);
      tuft.name = "scenic-grass-tuft";
      tuft.position.set(routeSideX(z, side, offset), 0.78, z);
      tuft.scale.setScalar(0.75 + ((z % 37) / 90));
      tuft.rotation.y = z * 0.037;
      group.add(tuft);
    }
  }

  for (const z of [110, 178, 244, 338, 430, 548, 678, 802, 930]) {
    for (const side of [-1, 1] as const) {
      const rock = new THREE.Mesh(rockGeometry, rockMaterial);
      rock.name = "scenic-rock";
      rock.position.set(routeSideX(z, side, 14 + (z % 5)), 0.8, z);
      rock.scale.set(1.2 + (z % 3) * 0.22, 0.6, 0.85 + (z % 4) * 0.08);
      rock.rotation.set(0.12, z * 0.021, -0.08 * side);
      group.add(rock);
    }
  }

  for (const z of [235, 288, 345, 403, 466, 532, 601, 672, 746]) {
    for (const offset of [-31, -22, 20, 30]) {
      const x = getRouteCenterXAtZ(route, z) + offset;
      const scale = 0.84 + ((Math.abs(offset) + z) % 5) * 0.08;
      const trunk = new THREE.Mesh(trunkGeometry, trunkMaterial);
      trunk.name = "scenic-tree-trunk";
      trunk.position.set(x, 2.5 * scale, z);
      trunk.scale.setScalar(scale);

      const canopy = new THREE.Mesh(canopyGeometry, forestMaterial);
      canopy.name = "scenic-tree-canopy";
      canopy.position.set(x, 8.4 * scale, z);
      canopy.scale.set(scale * 0.95, scale, scale * 0.95);

      group.add(trunk, canopy);
    }
  }

  const forestTunnel = new THREE.Group();
  forestTunnel.name = "scenic-forest-canopy-tunnel";

  for (const z of [245, 278, 312, 348, 385]) {
    for (const x of [-13.5, 13.5]) {
      const trunk = new THREE.Mesh(trunkGeometry, trunkMaterial);
      trunk.name = "scenic-forest-tunnel-trunk";
      trunk.position.set(getRouteCenterXAtZ(route, z) + x, 3, z);
      trunk.scale.set(0.78, 1.2, 0.78);
      trunk.rotation.z = x < 0 ? -0.08 : 0.08;
      forestTunnel.add(trunk);
    }

    const canopyBridge = new THREE.Mesh(canopyBridgeGeometry, forestMaterial);
    canopyBridge.name = "scenic-forest-tunnel-canopy";
    canopyBridge.position.set(getRouteCenterXAtZ(route, z), 8.4, z);
    canopyBridge.scale.set(0.95, 0.9 + (z % 4) * 0.05, 0.9);
    canopyBridge.rotation.y = (z % 2 === 0 ? 1 : -1) * 0.05;
    forestTunnel.add(canopyBridge);
  }

  group.add(forestTunnel);

  for (const [offset, z] of [
    [13, 315],
    [-13, 392],
    [13, 585],
    [-13, 755]
  ]) {
    const x = getRouteCenterXAtZ(route, z) + offset;
    const post = new THREE.Mesh(signPostGeometry, trunkMaterial);
    post.name = "scenic-road-sign";
    post.position.set(x, 1.5, z);

    const plate = new THREE.Mesh(signPlateGeometry, signMaterial);
    plate.name = "scenic-road-sign";
    plate.position.set(x, 3.2, z);
    plate.rotation.y = x < 0 ? 0.22 : -0.22;

    group.add(post, plate);
  }

  for (const z of [720, 820, 920, 1030]) {
    for (const offset of [-62, -46, 48, 66]) {
      const x = getRouteCenterXAtZ(route, z) + offset;
      const tree = new THREE.Mesh(farTreeGeometry, distantMaterial);
      tree.name = "scenic-far-tree";
      tree.position.set(x, 6, z);
      tree.scale.setScalar(0.86 + (Math.abs(offset) % 7) * 0.03);
      group.add(tree);
    }
  }

  for (const [offset, z, scale] of [
    [-78, 610, 1.1],
    [76, 690, 0.9],
    [-90, 880, 1.25],
    [92, 990, 1.05]
  ]) {
    const x = getRouteCenterXAtZ(route, z) + offset;
    const hill = new THREE.Mesh(farHillGeometry, distantMaterial);
    hill.name = "scenic-distant-hill";
    hill.position.set(x, 2.6, z);
    hill.scale.set(scale, 1, 1);
    hill.rotation.y = x < 0 ? 0.22 : -0.18;
    group.add(hill);
  }

  return {
    group,
    geometries,
    materials,
    coastMaterial,
    forestMaterial,
    fallbackTintMaterials
  };
}

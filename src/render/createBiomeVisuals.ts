import * as THREE from "three";

export type BiomeVisuals = {
  group: THREE.Group;
  geometries: THREE.BufferGeometry[];
  materials: THREE.Material[];
  coastMaterial: THREE.MeshStandardMaterial;
  forestMaterial: THREE.MeshStandardMaterial;
};

export function createBiomeVisuals(): BiomeVisuals {
  const group = new THREE.Group();
  group.name = "scenic-biome-visuals";

  const geometries: THREE.BufferGeometry[] = [];
  const materials: THREE.Material[] = [];

  const coastGeometry = new THREE.BoxGeometry(0.65, 2.4, 0.65);
  const trunkGeometry = new THREE.CylinderGeometry(0.55, 0.75, 5, 8);
  const canopyGeometry = new THREE.ConeGeometry(3.2, 8, 8);
  const grassGeometry = new THREE.ConeGeometry(0.32, 1.55, 5);
  const rockGeometry = new THREE.DodecahedronGeometry(1, 0);
  const signPlateGeometry = new THREE.BoxGeometry(3.6, 1.4, 0.22);
  const signPostGeometry = new THREE.CylinderGeometry(0.13, 0.13, 3, 8);
  const farTreeGeometry = new THREE.ConeGeometry(4.8, 12, 7);
  const farHillGeometry = new THREE.BoxGeometry(36, 6, 18);

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

  geometries.push(
    coastGeometry,
    trunkGeometry,
    canopyGeometry,
    grassGeometry,
    rockGeometry,
    signPlateGeometry,
    signPostGeometry,
    farTreeGeometry,
    farHillGeometry
  );
  materials.push(
    coastMaterial,
    forestMaterial,
    trunkMaterial,
    grassMaterial,
    rockMaterial,
    signMaterial,
    distantMaterial
  );

  for (const z of [52, 86, 122, 158, 196, 236, 278]) {
    for (const x of [-12.5, 12.5]) {
      const marker = new THREE.Mesh(coastGeometry, coastMaterial);
      marker.name = "scenic-coast-post";
      marker.position.set(x, 1.2, z);
      marker.rotation.y = x < 0 ? -0.16 : 0.16;
      group.add(marker);
    }
  }

  for (const z of [
    28, 46, 64, 82, 104, 126, 148, 172, 196, 222, 252, 286, 325, 366, 410, 456,
    504, 554, 606, 660, 716, 774, 834, 896, 956
  ]) {
    for (const side of [-1, 1]) {
      const offset = side * (8.5 + ((z / 23) % 4));
      const tuft = new THREE.Mesh(grassGeometry, grassMaterial);
      tuft.name = "scenic-grass-tuft";
      tuft.position.set(offset, 0.78, z);
      tuft.scale.setScalar(0.75 + ((z % 37) / 90));
      tuft.rotation.y = z * 0.037;
      group.add(tuft);
    }
  }

  for (const z of [110, 178, 244, 338, 430, 548, 678, 802, 930]) {
    for (const side of [-1, 1]) {
      const rock = new THREE.Mesh(rockGeometry, rockMaterial);
      rock.name = "scenic-rock";
      rock.position.set(side * (14 + (z % 5)), 0.8, z);
      rock.scale.set(1.2 + (z % 3) * 0.22, 0.6, 0.85 + (z % 4) * 0.08);
      rock.rotation.set(0.12, z * 0.021, -0.08 * side);
      group.add(rock);
    }
  }

  for (const z of [435, 488, 545, 603, 666, 732, 801, 872, 946]) {
    for (const x of [-31, -22, 20, 30]) {
      const scale = 0.84 + ((Math.abs(x) + z) % 5) * 0.08;
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

  for (const [x, z] of [
    [13, 315],
    [-13, 392],
    [13, 585],
    [-13, 755]
  ]) {
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
    for (const x of [-62, -46, 48, 66]) {
      const tree = new THREE.Mesh(farTreeGeometry, distantMaterial);
      tree.name = "scenic-far-tree";
      tree.position.set(x, 6, z);
      tree.scale.setScalar(0.86 + (Math.abs(x) % 7) * 0.03);
      group.add(tree);
    }
  }

  for (const [x, z, scale] of [
    [-78, 610, 1.1],
    [76, 690, 0.9],
    [-90, 880, 1.25],
    [92, 990, 1.05]
  ]) {
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
    forestMaterial
  };
}

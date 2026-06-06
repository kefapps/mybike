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
  group.name = "mvp-biome-markers";

  const geometries: THREE.BufferGeometry[] = [];
  const materials: THREE.Material[] = [];

  const coastGeometry = new THREE.BoxGeometry(5, 0.5, 38);
  const coastMaterial = new THREE.MeshStandardMaterial({
    color: 0x4f9fcf,
    roughness: 0.72
  });
  geometries.push(coastGeometry);
  materials.push(coastMaterial);

  for (const x of [-18, 18]) {
    for (const z of [82, 145, 210]) {
      const marker = new THREE.Mesh(coastGeometry, coastMaterial);
      marker.position.set(x, 0.15, z);
      marker.rotation.y = x < 0 ? -0.25 : 0.25;
      group.add(marker);
    }
  }

  const trunkGeometry = new THREE.CylinderGeometry(0.55, 0.75, 5, 8);
  const canopyGeometry = new THREE.ConeGeometry(3.2, 8, 8);
  const trunkMaterial = new THREE.MeshStandardMaterial({
    color: 0x6d4c30,
    roughness: 0.9
  });
  const forestMaterial = new THREE.MeshStandardMaterial({
    color: 0x174d2e,
    roughness: 0.82
  });
  geometries.push(trunkGeometry, canopyGeometry);
  materials.push(trunkMaterial, forestMaterial);

  for (const x of [-28, -18, 20, 32]) {
    for (const z of [500, 610, 720, 850]) {
      const trunk = new THREE.Mesh(trunkGeometry, trunkMaterial);
      trunk.position.set(x, 2.5, z);

      const canopy = new THREE.Mesh(canopyGeometry, forestMaterial);
      canopy.position.set(x, 8.6, z);

      group.add(trunk, canopy);
    }
  }

  return {
    group,
    geometries,
    materials,
    coastMaterial,
    forestMaterial
  };
}

import { BoxGeometry } from "three/src/geometries/BoxGeometry.js";
import { BufferGeometry } from "three/src/core/BufferGeometry.js";
import { ConeGeometry } from "three/src/geometries/ConeGeometry.js";
import { CylinderGeometry } from "three/src/geometries/CylinderGeometry.js";
import { DodecahedronGeometry } from "three/src/geometries/DodecahedronGeometry.js";
import { Material } from "three/src/materials/Material.js";
import { Group } from "three/src/objects/Group.js";
import { Mesh } from "three/src/objects/Mesh.js";
import { MeshStandardMaterial } from "three/src/materials/MeshStandardMaterial.js";

import type { SceneryAssetKind, SceneryInstance } from "../scenery";

type GeometryPool = Record<SceneryAssetKind, BufferGeometry>;
type MaterialPool = Record<SceneryAssetKind, Material>;

export type ProceduralSceneryResult = {
  group: Group;
  geometries: BufferGeometry[];
  materials: Material[];
};

export function createProceduralScenery(
  instances: ReadonlyArray<SceneryInstance>
): ProceduralSceneryResult {
  const group = new Group();
  group.name = "scenic-procedural";

  const geometries = buildGeometryPool();
  const materials = buildMaterialPool();
  const allGeoms = new Set<BufferGeometry>();
  const allMats = new Set<Material>();

  for (const instance of instances) {
    const geom = geometries[instance.kind];
    const mat = materials[instance.kind];

    if (!geom || !mat) {
      continue;
    }

    const mesh = new Mesh(geom, mat);
    mesh.name = `scenic-procedural-${instance.kind}`;
    mesh.position.set(
      instance.position.x,
      instance.position.y,
      instance.position.z
    );
    mesh.rotation.set(
      instance.rotation.x,
      instance.rotation.y,
      instance.rotation.z
    );
    mesh.scale.set(instance.scale.x, instance.scale.y, instance.scale.z);
    group.add(mesh);

    allGeoms.add(geom);
    allMats.add(mat);
  }

  return {
    group,
    geometries: [...allGeoms],
    materials: [...allMats]
  };
}

function buildGeometryPool(): GeometryPool {
  return {
    treeTrunk: new CylinderGeometry(0.55, 0.75, 5, 8),
    treeCanopy: new ConeGeometry(3.2, 8, 8),
    rock: new DodecahedronGeometry(1, 0),
    grassTuft: new ConeGeometry(0.32, 1.55, 5),
    roadSign: new CylinderGeometry(0.13, 0.13, 3, 8),
    berm: new BoxGeometry(18, 2.4, 26),
    ridge: new BoxGeometry(56, 8, 30),
    farTree: new ConeGeometry(4.8, 12, 7),
    nearPost: new BoxGeometry(0.24, 2.1, 0.24),
    nearBlade: new ConeGeometry(0.16, 1.25, 4),
    nearStone: new DodecahedronGeometry(0.45, 0),
    reed: new BoxGeometry(0.12, 1.85, 0.12),
    fern: new ConeGeometry(0.42, 1.15, 6)
  };
}

function buildMaterialPool(): MaterialPool {
  return {
    treeTrunk: new MeshStandardMaterial({ color: 0x6d4c30, roughness: 0.9 }),
    treeCanopy: new MeshStandardMaterial({ color: 0x174d2e, roughness: 0.82 }),
    rock: new MeshStandardMaterial({ color: 0x7f8178, roughness: 0.88 }),
    grassTuft: new MeshStandardMaterial({ color: 0x6aa35f, roughness: 0.94 }),
    roadSign: new MeshStandardMaterial({ color: 0x6d4c30, roughness: 0.9 }),
    berm: new MeshStandardMaterial({ color: 0x6f9b68, roughness: 0.96 }),
    ridge: new MeshStandardMaterial({ color: 0x486f5d, roughness: 0.98 }),
    farTree: new MeshStandardMaterial({ color: 0x315c46, roughness: 0.98 }),
    nearPost: new MeshStandardMaterial({ color: 0xf7e8b6, roughness: 0.74 }),
    nearBlade: new MeshStandardMaterial({ color: 0x89b66c, roughness: 0.96 }),
    nearStone: new MeshStandardMaterial({ color: 0x696d66, roughness: 0.9 }),
    reed: new MeshStandardMaterial({ color: 0xb58d55, roughness: 0.9 }),
    fern: new MeshStandardMaterial({ color: 0x2f7a42, roughness: 0.94 })
  };
}

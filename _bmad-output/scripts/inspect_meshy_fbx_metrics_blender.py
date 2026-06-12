#!/usr/bin/env python3
import json
import sys
from pathlib import Path

import bpy
from mathutils import Vector


def inspect_fbx(path):
    bpy.ops.object.select_all(action="SELECT")
    bpy.ops.object.delete()
    bpy.ops.import_scene.fbx(filepath=str(path))

    meshes = [obj for obj in bpy.context.scene.objects if obj.type == "MESH"]
    vertices = sum(len(obj.data.vertices) for obj in meshes)
    faces = sum(len(obj.data.polygons) for obj in meshes)
    triangles = sum(
        sum(max(1, len(poly.vertices) - 2) for poly in obj.data.polygons)
        for obj in meshes
    )
    materials = sorted({slot.material.name for obj in meshes for slot in obj.material_slots if slot.material})

    min_corner = [float("inf"), float("inf"), float("inf")]
    max_corner = [float("-inf"), float("-inf"), float("-inf")]
    for obj in meshes:
        for corner in obj.bound_box:
            world = obj.matrix_world @ Vector(corner)
            for index in range(3):
                min_corner[index] = min(min_corner[index], world[index])
                max_corner[index] = max(max_corner[index], world[index])

    if not meshes:
        min_corner = [0, 0, 0]
        max_corner = [0, 0, 0]

    dimensions = [max_corner[index] - min_corner[index] for index in range(3)]
    return {
        "path": str(path),
        "bytes": path.stat().st_size,
        "mesh_objects": len(meshes),
        "vertices": vertices,
        "faces": faces,
        "estimated_triangles": triangles,
        "materials": materials,
        "bounds_min": min_corner,
        "bounds_max": max_corner,
        "dimensions": dimensions,
    }


def main():
    args = sys.argv[sys.argv.index("--") + 1 :] if "--" in sys.argv else sys.argv[1:]
    output = Path(args[0])
    paths = [Path(value) for value in args[1:]]
    metrics = [inspect_fbx(path) for path in paths]
    output.parent.mkdir(parents=True, exist_ok=True)
    output.write_text(json.dumps(metrics, indent=2), encoding="utf-8")
    print(json.dumps(metrics, indent=2))


if __name__ == "__main__":
    main()

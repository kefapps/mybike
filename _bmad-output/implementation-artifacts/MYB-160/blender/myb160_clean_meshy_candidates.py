import argparse
import json
import math
import os
import sys
from pathlib import Path

import bpy
from mathutils import Vector


def clear_scene():
    bpy.ops.object.select_all(action="SELECT")
    bpy.ops.object.delete()


def import_fbx(path):
    bpy.ops.import_scene.fbx(filepath=str(path))
    return [obj for obj in bpy.context.scene.objects if obj.type == "MESH"]


def mesh_counts(mesh_objects):
    vertices = sum(len(obj.data.vertices) for obj in mesh_objects)
    faces = sum(len(obj.data.polygons) for obj in mesh_objects)
    materials = set()
    for obj in mesh_objects:
        for slot in obj.material_slots:
            if slot.material:
                materials.add(slot.material.name)
    return vertices, faces, len(materials)


def combined_bounds(mesh_objects):
    points = []
    for obj in mesh_objects:
        matrix = obj.matrix_world
        points.extend(matrix @ Vector(corner) for corner in obj.bound_box)
    if not points:
        return Vector((0, 0, 0)), Vector((0, 0, 0))
    min_corner = Vector((min(point.x for point in points), min(point.y for point in points), min(point.z for point in points)))
    max_corner = Vector((max(point.x for point in points), max(point.y for point in points), max(point.z for point in points)))
    return min_corner, max_corner


def apply_transforms(mesh_objects):
    bpy.ops.object.select_all(action="DESELECT")
    for obj in mesh_objects:
        obj.select_set(True)
        bpy.context.view_layer.objects.active = obj
        bpy.ops.object.transform_apply(location=False, rotation=True, scale=True)


def remove_tiny_fragments(mesh_objects, min_diagonal):
    removed = []
    for obj in list(mesh_objects):
        min_corner, max_corner = combined_bounds([obj])
        diagonal = (max_corner - min_corner).length
        face_count = len(obj.data.polygons)
        if diagonal < min_diagonal or face_count < 4:
            removed.append({"name": obj.name, "diagonal": diagonal, "faces": face_count})
            bpy.data.objects.remove(obj, do_unlink=True)
    return removed


def recenter_bottom(mesh_objects):
    min_corner, max_corner = combined_bounds(mesh_objects)
    center_x = (min_corner.x + max_corner.x) * 0.5
    center_y = (min_corner.y + max_corner.y) * 0.5
    bottom_z = min_corner.z
    delta = Vector((-center_x, -center_y, -bottom_z))
    for obj in mesh_objects:
        obj.location += delta


def decimate_if_needed(mesh_objects, target_faces):
    _, before_faces, _ = mesh_counts(mesh_objects)
    if before_faces <= target_faces or before_faces == 0:
        return 1.0, before_faces

    ratio = max(0.01, min(1.0, target_faces / before_faces))
    for obj in mesh_objects:
        if len(obj.data.polygons) < 20:
            continue
        bpy.context.view_layer.objects.active = obj
        obj.select_set(True)
        modifier = obj.modifiers.new(name="MYB160_TargetedDecimate", type="DECIMATE")
        modifier.ratio = ratio
        modifier.use_collapse_triangulate = True
        bpy.ops.object.modifier_apply(modifier=modifier.name)
        obj.select_set(False)
    return ratio, before_faces


def shade_flat(mesh_objects):
    for obj in mesh_objects:
        for polygon in obj.data.polygons:
            polygon.use_smooth = False


def make_material(name, color):
    material = bpy.data.materials.new(name)
    material.diffuse_color = color
    return material


def assign_scene_materials(mesh_objects, asset_name):
    bark = make_material("MYB160_BarkWarm", (0.34, 0.21, 0.12, 1.0))
    root = make_material("MYB160_RootDark", (0.14, 0.09, 0.055, 1.0))
    leaf = make_material("MYB160_LeafDeep", (0.10, 0.27, 0.11, 1.0))
    moss = make_material("MYB160_MossDeep", (0.08, 0.23, 0.10, 1.0))
    min_corner, max_corner = combined_bounds(mesh_objects)
    height = max(0.001, max_corner.z - min_corner.z)

    for obj in mesh_objects:
        obj.data.materials.clear()
        if "tree" in asset_name:
            obj.data.materials.append(bark)
            obj.data.materials.append(root)
            obj.data.materials.append(leaf)
            obj.data.materials.append(moss)
        else:
            obj.data.materials.append(root)
            obj.data.materials.append(bark)
            obj.data.materials.append(moss)

        for polygon in obj.data.polygons:
            center = obj.matrix_world @ polygon.center
            z01 = (center.z - min_corner.z) / height
            radial = math.sqrt(center.x * center.x + center.y * center.y)
            if "tree" in asset_name:
                if z01 > 0.56 and radial > 0.8:
                    polygon.material_index = 2
                elif z01 < 0.18 and radial > 0.9:
                    polygon.material_index = 3
                elif z01 < 0.35:
                    polygon.material_index = 1
                else:
                    polygon.material_index = 0
            else:
                if z01 < 0.20 and radial > 0.8:
                    polygon.material_index = 2
                elif z01 > 0.55:
                    polygon.material_index = 1
                else:
                    polygon.material_index = 0


def export_fbx(path):
    bpy.ops.export_scene.fbx(
        filepath=str(path),
        use_selection=False,
        apply_unit_scale=True,
        bake_space_transform=False,
        object_types={"MESH"},
        add_leaf_bones=False,
        axis_forward="-Z",
        axis_up="Y",
    )


def render_preview(path, asset_name):
    camera_data = bpy.data.cameras.new("MYB160_PreviewCamera")
    camera = bpy.data.objects.new("MYB160_PreviewCamera", camera_data)
    bpy.context.collection.objects.link(camera)
    min_corner, max_corner = combined_bounds([obj for obj in bpy.context.scene.objects if obj.type == "MESH"])
    center = (min_corner + max_corner) * 0.5
    size = max(1.0, (max_corner - min_corner).length)
    camera.location = (center.x - size * 0.7, center.y - size * 1.15, center.z + size * 0.55)
    camera.rotation_euler = (math.radians(62), 0, math.radians(-32))
    camera_data.lens = 35
    bpy.context.scene.camera = camera

    light_data = bpy.data.lights.new("MYB160_PreviewKey", type="AREA")
    light = bpy.data.objects.new("MYB160_PreviewKey", light_data)
    bpy.context.collection.objects.link(light)
    light.location = (center.x - size * 0.4, center.y - size * 0.7, center.z + size)
    light_data.energy = 450
    light_data.size = max(3.0, size * 0.25)

    bpy.context.scene.render.engine = "BLENDER_WORKBENCH"
    bpy.context.scene.view_settings.view_transform = "Standard"
    bpy.context.scene.render.resolution_x = 1280
    bpy.context.scene.render.resolution_y = 900
    bpy.context.scene.render.filepath = str(path)
    bpy.ops.render.render(write_still=True)


def clean_asset(input_path, output_path, preview_path, metrics_path, name, target_faces):
    clear_scene()
    imported = import_fbx(input_path)
    raw_vertices, raw_faces, raw_materials = mesh_counts(imported)
    raw_min, raw_max = combined_bounds(imported)

    apply_transforms(imported)
    removed = remove_tiny_fragments(imported, min_diagonal=0.015)
    cleaned_objects = [obj for obj in bpy.context.scene.objects if obj.type == "MESH"]
    ratio, pre_decimate_faces = decimate_if_needed(cleaned_objects, target_faces)
    cleaned_objects = [obj for obj in bpy.context.scene.objects if obj.type == "MESH"]
    shade_flat(cleaned_objects)
    recenter_bottom(cleaned_objects)
    assign_scene_materials(cleaned_objects, name)
    clean_vertices, clean_faces, clean_materials = mesh_counts(cleaned_objects)
    clean_min, clean_max = combined_bounds(cleaned_objects)

    export_fbx(output_path)
    render_preview(preview_path, name)

    metrics = {
        "name": name,
        "inputPath": str(input_path),
        "outputPath": str(output_path),
        "previewPath": str(preview_path),
        "targetFaces": target_faces,
        "raw": {
            "meshObjects": len(imported),
            "vertices": raw_vertices,
            "faces": raw_faces,
            "materials": raw_materials,
            "dimensions": [round(raw_max.x - raw_min.x, 4), round(raw_max.y - raw_min.y, 4), round(raw_max.z - raw_min.z, 4)],
        },
        "cleaned": {
            "meshObjects": len(cleaned_objects),
            "vertices": clean_vertices,
            "faces": clean_faces,
            "materials": clean_materials,
            "dimensions": [round(clean_max.x - clean_min.x, 4), round(clean_max.y - clean_min.y, 4), round(clean_max.z - clean_min.z, 4)],
            "bottomAtOriginZ": round(clean_min.z, 5),
        },
        "removedFragments": removed,
        "decimateRatio": round(ratio, 5),
        "facesBeforeDecimate": pre_decimate_faces,
    }
    metrics_path.parent.mkdir(parents=True, exist_ok=True)
    metrics_path.write_text(json.dumps(metrics, indent=2), encoding="utf-8")
    return metrics


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--input", required=True)
    parser.add_argument("--output", required=True)
    parser.add_argument("--preview", required=True)
    parser.add_argument("--metrics", required=True)
    parser.add_argument("--name", required=True)
    parser.add_argument("--target-faces", type=int, required=True)
    argv = sys.argv
    if "--" in argv:
        argv = argv[argv.index("--") + 1 :]
    else:
        argv = []
    args = parser.parse_args(argv)

    clean_asset(
        Path(args.input),
        Path(args.output),
        Path(args.preview),
        Path(args.metrics),
        args.name,
        args.target_faces,
    )


if __name__ == "__main__":
    main()

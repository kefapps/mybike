#!/usr/bin/env python3
import json
import math
import shutil
from pathlib import Path

import bpy
from mathutils import Vector


SOURCE_FBX = Path("meshy_output/20260611_233231_premium-stylized-fantasy-unity_019eb899/refined.fbx")
SOURCE_DIR = SOURCE_FBX.parent
OUTPUT_DIR = SOURCE_DIR / "blender_optimized"
METRICS_PATH = OUTPUT_DIR / "optimization_metrics.json"
COMPARISON_PATH = Path("_bmad-output/meshy-captures/myb-95-model-3-relic-fountain-blender-optimized-comparison.png")

VARIANTS = [
    ("hero_80k", 80000),
    ("lod0_50k", 50000),
    ("lod1_30k", 30000),
    ("lod2_15k", 15000),
]

TEXTURE_NAMES = [
    "refined_base_color.png",
    "refined_metallic.png",
    "refined_roughness.png",
    "refined_normal.png",
    "refined_emission.png",
]


def texture_path(name):
    path = SOURCE_DIR / name
    return path if path.exists() else None


def load_image(name, color_space="sRGB"):
    path = texture_path(name)
    if not path:
        return None
    image = bpy.data.images.load(str(path.resolve()), check_existing=True)
    image.colorspace_settings.name = color_space
    return image


def build_material():
    material = bpy.data.materials.new("MYB95_RelicFountain_PBR")
    material.use_nodes = True
    nodes = material.node_tree.nodes
    links = material.node_tree.links
    principled = nodes.get("Principled BSDF")
    if not principled:
        return material

    base_color = load_image("refined_base_color.png", "sRGB")
    if base_color and "Base Color" in principled.inputs:
        node = nodes.new("ShaderNodeTexImage")
        node.name = "MYB95_Relic_BaseColor"
        node.image = base_color
        links.new(node.outputs["Color"], principled.inputs["Base Color"])

    metallic = load_image("refined_metallic.png", "Non-Color")
    if metallic and "Metallic" in principled.inputs:
        node = nodes.new("ShaderNodeTexImage")
        node.name = "MYB95_Relic_Metallic"
        node.image = metallic
        links.new(node.outputs["Color"], principled.inputs["Metallic"])

    roughness = load_image("refined_roughness.png", "Non-Color")
    if roughness and "Roughness" in principled.inputs:
        node = nodes.new("ShaderNodeTexImage")
        node.name = "MYB95_Relic_Roughness"
        node.image = roughness
        links.new(node.outputs["Color"], principled.inputs["Roughness"])

    normal = load_image("refined_normal.png", "Non-Color")
    if normal and "Normal" in principled.inputs:
        image_node = nodes.new("ShaderNodeTexImage")
        image_node.name = "MYB95_Relic_Normal"
        image_node.image = normal
        normal_node = nodes.new("ShaderNodeNormalMap")
        normal_node.name = "MYB95_Relic_NormalMap"
        links.new(image_node.outputs["Color"], normal_node.inputs["Color"])
        links.new(normal_node.outputs["Normal"], principled.inputs["Normal"])

    emission = load_image("refined_emission.png", "sRGB")
    if emission:
        image_node = nodes.new("ShaderNodeTexImage")
        image_node.name = "MYB95_Relic_Emission"
        image_node.image = emission
        emission_input = principled.inputs.get("Emission Color") or principled.inputs.get("Emission")
        if emission_input:
            links.new(image_node.outputs["Color"], emission_input)
        if "Emission Strength" in principled.inputs:
            principled.inputs["Emission Strength"].default_value = 0.9

    return material


def clean_scene():
    bpy.ops.object.select_all(action="SELECT")
    bpy.ops.object.delete()


def mesh_stats(obj):
    mesh = obj.data
    faces = len(mesh.polygons)
    triangles = sum(max(1, len(poly.vertices) - 2) for poly in mesh.polygons)
    return {
        "vertices": len(mesh.vertices),
        "faces": faces,
        "estimated_triangles": triangles,
    }


def materials_for(obj):
    return sorted({slot.material.name for slot in obj.material_slots if slot.material})


def bounds_for_objects(objects):
    min_corner = Vector((math.inf, math.inf, math.inf))
    max_corner = Vector((-math.inf, -math.inf, -math.inf))
    for obj in objects:
        for corner in obj.bound_box:
            world = obj.matrix_world @ Vector(corner)
            min_corner.x = min(min_corner.x, world.x)
            min_corner.y = min(min_corner.y, world.y)
            min_corner.z = min(min_corner.z, world.z)
            max_corner.x = max(max_corner.x, world.x)
            max_corner.y = max(max_corner.y, world.y)
            max_corner.z = max(max_corner.z, world.z)
    return min_corner, max_corner


def import_source():
    clean_scene()
    bpy.ops.import_scene.fbx(filepath=str(SOURCE_FBX))
    meshes = [obj for obj in bpy.context.scene.objects if obj.type == "MESH"]
    if len(meshes) != 1:
        raise RuntimeError(f"Expected 1 mesh object, got {len(meshes)}")
    source = meshes[0]
    source.name = "MYB95_Source_RelicFountain_721k"
    source.data.name = "MYB95_Source_RelicFountain_721k_Mesh"
    source.data.materials.clear()
    source.data.materials.append(build_material())
    return source


def duplicate_variant(source, name, target_faces, source_faces):
    copy = source.copy()
    copy.data = source.data.copy()
    copy.animation_data_clear()
    copy.name = "MYB95_RelicFountain_" + name
    copy.data.name = "MYB95_RelicFountain_" + name + "_Mesh"
    bpy.context.collection.objects.link(copy)

    ratio = min(1.0, max(0.01, target_faces / source_faces))
    modifier = copy.modifiers.new(name="MYB95_Decimate_To_" + name, type="DECIMATE")
    modifier.decimate_type = "COLLAPSE"
    modifier.ratio = ratio
    modifier.use_collapse_triangulate = True

    bpy.context.view_layer.objects.active = copy
    copy.select_set(True)
    bpy.ops.object.modifier_apply(modifier=modifier.name)

    normal_modifier = copy.modifiers.new(name="MYB95_Weighted_Normals", type="WEIGHTED_NORMAL")
    normal_modifier.keep_sharp = True
    try:
        bpy.ops.object.modifier_apply(modifier=normal_modifier.name)
    except RuntimeError:
        copy.modifiers.remove(normal_modifier)

    copy.select_set(False)
    return copy


def export_fbx(obj, path):
    bpy.ops.object.select_all(action="DESELECT")
    obj.select_set(True)
    bpy.context.view_layer.objects.active = obj
    bpy.ops.export_scene.fbx(
        filepath=str(path),
        use_selection=True,
        apply_unit_scale=True,
        bake_space_transform=False,
        object_types={"MESH"},
        path_mode="COPY",
        embed_textures=False,
        add_leaf_bones=False,
    )


def copy_textures():
    copied = []
    for texture_name in TEXTURE_NAMES:
        source = SOURCE_DIR / texture_name
        if not source.exists():
            continue
        target = OUTPUT_DIR / texture_name
        shutil.copy2(source, target)
        copied.append(str(target))
    return copied


def setup_comparison_scene():
    meshes = [obj for obj in bpy.context.scene.objects if obj.type == "MESH"]
    spacing = 2.35
    offset = -spacing * (len(meshes) - 1) / 2
    for index, obj in enumerate(meshes):
        obj.location.x = offset + index * spacing
        obj.location.y = 0
        obj.location.z = 0

    min_corner, max_corner = bounds_for_objects(meshes)
    center = (min_corner + max_corner) / 2
    width = max_corner.x - min_corner.x
    height = max_corner.z - min_corner.z

    bpy.ops.object.light_add(type="AREA", location=(center.x, -4.5, max_corner.z + 3.2))
    light = bpy.context.object
    light.name = "MYB95_RelicFountain_Comparison_Key_Light"
    light.data.energy = 800
    light.data.size = 7

    bpy.ops.object.camera_add(location=(center.x, -8.2, center.z + 0.35))
    camera = bpy.context.object
    bpy.context.scene.camera = camera
    direction = center - camera.location
    camera.rotation_euler = direction.to_track_quat("-Z", "Y").to_euler()
    camera.data.type = "ORTHO"
    camera.data.ortho_scale = max(height * 1.2, width * 0.30)

    try:
        bpy.context.scene.render.engine = "BLENDER_EEVEE_NEXT"
    except TypeError:
        bpy.context.scene.render.engine = "BLENDER_EEVEE"

    world = bpy.context.scene.world or bpy.data.worlds.new("World")
    bpy.context.scene.world = world
    world.color = (0.045, 0.045, 0.045)

    bpy.context.scene.view_settings.view_transform = "Filmic"
    bpy.context.scene.view_settings.look = "Medium High Contrast"
    bpy.context.scene.render.resolution_x = 2600
    bpy.context.scene.render.resolution_y = 900
    bpy.context.scene.render.film_transparent = False


def render_comparison():
    COMPARISON_PATH.parent.mkdir(parents=True, exist_ok=True)
    setup_comparison_scene()
    bpy.ops.wm.save_as_mainfile(filepath=str(OUTPUT_DIR / "myb95_relic_fountain_optimization.blend"))
    bpy.context.scene.render.filepath = str(COMPARISON_PATH)
    bpy.ops.render.render(write_still=True)


def main():
    OUTPUT_DIR.mkdir(parents=True, exist_ok=True)
    source = import_source()
    source_faces = mesh_stats(source)["faces"]
    source.select_set(False)

    metrics = {
        "source": {
            "path": str(SOURCE_FBX),
            "bytes": SOURCE_FBX.stat().st_size,
            **mesh_stats(source),
            "materials": materials_for(source),
        },
        "variants": [],
        "copied_textures": copy_textures(),
        "comparison_capture": str(COMPARISON_PATH),
    }

    for name, target_faces in VARIANTS:
        variant = duplicate_variant(source, name, target_faces, source_faces)
        output_fbx = OUTPUT_DIR / f"myb95_relic_fountain_{name}.fbx"
        export_fbx(variant, output_fbx)
        stats = mesh_stats(variant)
        metrics["variants"].append({
            "name": name,
            "target_faces": target_faces,
            "fbx": str(output_fbx),
            "fbx_bytes": output_fbx.stat().st_size,
            **stats,
            "face_reduction_percent": round(100 * (1 - stats["faces"] / source_faces), 2),
            "materials": materials_for(variant),
        })

    render_comparison()
    METRICS_PATH.write_text(json.dumps(metrics, indent=2), encoding="utf-8")
    print(json.dumps(metrics, indent=2))


if __name__ == "__main__":
    main()

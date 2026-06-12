#!/usr/bin/env python3
import json
import math
import sys
from pathlib import Path

import bpy
from mathutils import Vector


CAPTURE_DIR = Path("_bmad-output/meshy-captures")
SOURCE = {
    "name": "source_354k",
    "label": "Source 354k",
    "path": Path("meshy_output/20260611_210528_30-credit-meshy-6-textured_019eb813/model.fbx"),
}
VARIANT_DIR = Path("meshy_output/20260611_210528_30-credit-meshy-6-textured_019eb813/blender_optimized")
VARIANTS = [
    SOURCE,
    {"name": "rig_safe_250k", "label": "Rig-safe 250k", "path": VARIANT_DIR / "myb95_character_rig_safe_250k.fbx"},
    {"name": "candidate_120k", "label": "Candidate 120k", "path": VARIANT_DIR / "myb95_character_candidate_120k.fbx"},
    {"name": "candidate_80k", "label": "Candidate 80k", "path": VARIANT_DIR / "myb95_character_candidate_80k.fbx"},
    {"name": "lod0_50k", "label": "LOD0 50k", "path": VARIANT_DIR / "myb95_character_lod0_50k.fbx"},
    {"name": "lod1_30k", "label": "LOD1 30k", "path": VARIANT_DIR / "myb95_character_lod1_30k.fbx"},
]
MANIFEST = CAPTURE_DIR / "myb-95-character-blender-optimized-preview-manifest.json"


def clean_scene():
    bpy.ops.object.select_all(action="SELECT")
    bpy.ops.object.delete()


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


def look_at(obj, target):
    direction = target - obj.location
    obj.rotation_euler = direction.to_track_quat("-Z", "Y").to_euler()


def setup_render(meshes):
    min_corner, max_corner = bounds_for_objects(meshes)
    center = (min_corner + max_corner) / 2
    dimensions = max_corner - min_corner
    aspect = 700 / 900
    ortho_scale = max(dimensions.z * 1.15, dimensions.x / aspect * 1.12, dimensions.y * 2.0)

    bpy.ops.object.light_add(type="AREA", location=(center.x, center.y - 4.0, max_corner.z + 3.0))
    light = bpy.context.object
    light.data.energy = 700
    light.data.size = 5

    bpy.ops.object.camera_add(location=(center.x, center.y - 6.5, center.z + 0.2))
    camera = bpy.context.object
    look_at(camera, center)
    camera.data.type = "ORTHO"
    camera.data.ortho_scale = ortho_scale
    bpy.context.scene.camera = camera

    try:
        bpy.context.scene.render.engine = "BLENDER_EEVEE_NEXT"
    except TypeError:
        bpy.context.scene.render.engine = "BLENDER_EEVEE"
    bpy.context.scene.render.resolution_x = 700
    bpy.context.scene.render.resolution_y = 900
    bpy.context.scene.render.film_transparent = False

    world = bpy.context.scene.world or bpy.data.worlds.new("World")
    bpy.context.scene.world = world
    world.color = (0.045, 0.045, 0.045)

    bpy.context.scene.view_settings.view_transform = "Filmic"
    bpy.context.scene.view_settings.look = "Medium High Contrast"


def render_variant(variant):
    clean_scene()
    bpy.ops.import_scene.fbx(filepath=str(variant["path"]))
    meshes = [obj for obj in bpy.context.scene.objects if obj.type == "MESH"]
    if not meshes:
        raise RuntimeError(f"No mesh imported from {variant['path']}")
    setup_render(meshes)
    output = CAPTURE_DIR / f"myb-95-character-blender-optimized-{variant['name']}.png"
    bpy.context.scene.render.filepath = str(output)
    bpy.ops.render.render(write_still=True)
    return str(output)


def main():
    CAPTURE_DIR.mkdir(parents=True, exist_ok=True)
    rendered = []
    for variant in VARIANTS:
        output = render_variant(variant)
        rendered.append({
            "name": variant["name"],
            "label": variant["label"],
            "fbx": str(variant["path"]),
            "preview": output,
        })
    MANIFEST.write_text(json.dumps(rendered, indent=2), encoding="utf-8")
    print(json.dumps(rendered, indent=2))


if __name__ == "__main__":
    sys.exit(main())

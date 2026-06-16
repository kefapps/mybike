import json
import math
import os
from datetime import datetime, timezone

import bpy
from mathutils import Matrix, Vector


REPO_ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), "../../.."))
UNITY_DIR = os.path.join(
    REPO_ROOT,
    "unity",
    "Echapee4D",
    "Assets",
    "Echappee",
    "Art",
    "Candidates",
    "MYB_ForestKit_V0",
)
UNITY_REL_DIR = "Assets/Echappee/Art/Candidates/MYB_ForestKit_V0"
ARTIFACT_DIR = os.path.join(REPO_ROOT, "_bmad-output", "implementation-artifacts", "MYB-147")
CHECKPOINT_DIR = os.path.join(REPO_ROOT, "_bmad-output", "visual-checkpoints", "MYB-147")
KIT_NAME = "MYB_ForestKit_V0"
TIMESTAMP = datetime.now(timezone.utc).strftime("%Y-%m-%dT%H-%M-%SZ")


MATERIAL_SPECS = {
    "myb_bark_warm": (0.42, 0.28, 0.16, 1.0),
    "myb_bark_dark": (0.26, 0.17, 0.11, 1.0),
    "myb_root_moss": (0.20, 0.34, 0.18, 1.0),
    "myb_rock_cool": (0.38, 0.40, 0.39, 1.0),
    "myb_rock_moss": (0.18, 0.38, 0.20, 1.0),
    "myb_fern_deep": (0.12, 0.34, 0.18, 1.0),
    "myb_fern_lit": (0.22, 0.49, 0.23, 1.0),
    "myb_leaf_floor": (0.38, 0.25, 0.13, 1.0),
    "myb_moss_floor": (0.18, 0.42, 0.21, 1.0),
    "myb_canopy_deep": (0.11, 0.29, 0.16, 1.0),
    "myb_canopy_lit": (0.20, 0.42, 0.20, 1.0),
}


ASSET_SPECS = [
    {
        "id": "myb_forest_trunk_ancient_a",
        "name": "MYB Forest Trunk Ancient A",
        "family": "trunks",
        "variant": "ancient_a",
        "budget": 1200,
    },
    {
        "id": "myb_forest_trunk_broken_a",
        "name": "MYB Forest Trunk Broken A",
        "family": "trunks",
        "variant": "broken_a",
        "budget": 1200,
    },
    {
        "id": "myb_forest_trunk_leaning_a",
        "name": "MYB Forest Trunk Leaning A",
        "family": "trunks",
        "variant": "leaning_a",
        "budget": 1200,
    },
    {
        "id": "myb_forest_trunk_knotted_a",
        "name": "MYB Forest Trunk Knotted A",
        "family": "trunks",
        "variant": "knotted_a",
        "budget": 1200,
    },
    {
        "id": "myb_forest_root_cluster_lateral_a",
        "name": "MYB Forest Root Cluster Lateral A",
        "family": "roots",
        "variant": "lateral_a",
        "budget": 900,
    },
    {
        "id": "myb_forest_root_cluster_ground_a",
        "name": "MYB Forest Root Cluster Ground A",
        "family": "roots",
        "variant": "ground_a",
        "budget": 900,
    },
    {
        "id": "myb_forest_root_arch_a",
        "name": "MYB Forest Root Arch A",
        "family": "roots",
        "variant": "arch_a",
        "budget": 900,
    },
    {
        "id": "myb_forest_rock_mossy_a",
        "name": "MYB Forest Rock Mossy A",
        "family": "rocks",
        "variant": "mossy_a",
        "budget": 700,
    },
    {
        "id": "myb_forest_rock_mossy_b",
        "name": "MYB Forest Rock Mossy B",
        "family": "rocks",
        "variant": "mossy_b",
        "budget": 700,
    },
    {
        "id": "myb_forest_rock_marker_a",
        "name": "MYB Forest Rock Marker A",
        "family": "rocks",
        "variant": "marker_a",
        "budget": 700,
    },
    {
        "id": "myb_forest_fern_a",
        "name": "MYB Forest Fern A",
        "family": "ferns",
        "variant": "a",
        "budget": 600,
    },
    {
        "id": "myb_forest_fern_b",
        "name": "MYB Forest Fern B",
        "family": "ferns",
        "variant": "b",
        "budget": 600,
    },
    {
        "id": "myb_forest_fern_c",
        "name": "MYB Forest Fern C",
        "family": "ferns",
        "variant": "c",
        "budget": 600,
    },
    {
        "id": "myb_forest_leaf_moss_mat_a",
        "name": "MYB Forest Leaf Moss Mat A",
        "family": "floor_mats",
        "variant": "a",
        "budget": 200,
    },
    {
        "id": "myb_forest_leaf_moss_mat_b",
        "name": "MYB Forest Leaf Moss Mat B",
        "family": "floor_mats",
        "variant": "b",
        "budget": 200,
    },
    {
        "id": "myb_forest_leaf_moss_mat_c",
        "name": "MYB Forest Leaf Moss Mat C",
        "family": "floor_mats",
        "variant": "c",
        "budget": 200,
    },
    {
        "id": "myb_forest_dead_branch_a",
        "name": "MYB Forest Dead Branch A",
        "family": "branches",
        "variant": "a",
        "budget": 400,
    },
    {
        "id": "myb_forest_dead_branch_b",
        "name": "MYB Forest Dead Branch B",
        "family": "branches",
        "variant": "b",
        "budget": 400,
    },
    {
        "id": "myb_forest_canopy_mass_a",
        "name": "MYB Forest Canopy Mass A",
        "family": "canopy",
        "variant": "a",
        "budget": 1000,
    },
    {
        "id": "myb_forest_canopy_mass_b",
        "name": "MYB Forest Canopy Mass B",
        "family": "canopy",
        "variant": "b",
        "budget": 1000,
    },
    {
        "id": "myb_forest_fallen_log_a",
        "name": "MYB Forest Fallen Log A",
        "family": "logs",
        "variant": "a",
        "budget": 1200,
    },
]


def ensure_dirs():
    os.makedirs(UNITY_DIR, exist_ok=True)
    os.makedirs(ARTIFACT_DIR, exist_ok=True)
    os.makedirs(CHECKPOINT_DIR, exist_ok=True)


def clear_scene():
    bpy.ops.object.select_all(action="SELECT")
    bpy.ops.object.delete()


def make_material(name):
    mat = bpy.data.materials.new(name)
    mat.diffuse_color = MATERIAL_SPECS[name]
    mat.use_nodes = True
    principled = mat.node_tree.nodes.get("Principled BSDF")
    if principled is not None:
        principled.inputs["Base Color"].default_value = MATERIAL_SPECS[name]
        principled.inputs["Roughness"].default_value = 0.78
    return mat


def materials():
    return {name: make_material(name) for name in MATERIAL_SPECS}


def set_flat(object_):
    if object_.type == "MESH":
        for polygon in object_.data.polygons:
            polygon.use_smooth = False


def apply_transform(object_):
    bpy.context.view_layer.objects.active = object_
    object_.select_set(True)
    bpy.ops.object.transform_apply(location=False, rotation=True, scale=True)
    object_.select_set(False)


def cylinder_between(name, start, end, radius, vertices, material):
    start_vec = Vector(start)
    end_vec = Vector(end)
    direction = end_vec - start_vec
    length = direction.length
    center = start_vec + direction * 0.5
    bpy.ops.mesh.primitive_cylinder_add(vertices=vertices, radius=radius, depth=length, location=center)
    obj = bpy.context.object
    obj.name = name
    obj.data.name = name + "_mesh"
    obj.data.materials.append(material)
    obj.rotation_euler = direction.to_track_quat("Z", "Y").to_euler()
    apply_transform(obj)
    set_flat(obj)
    return obj


def cone_between(name, start, end, radius_start, radius_end, vertices, material):
    start_vec = Vector(start)
    end_vec = Vector(end)
    direction = end_vec - start_vec
    length = direction.length
    center = start_vec + direction * 0.5
    bpy.ops.mesh.primitive_cone_add(
        vertices=vertices,
        radius1=radius_start,
        radius2=radius_end,
        depth=length,
        location=center,
    )
    obj = bpy.context.object
    obj.name = name
    obj.data.name = name + "_mesh"
    obj.data.materials.append(material)
    obj.rotation_euler = direction.to_track_quat("Z", "Y").to_euler()
    apply_transform(obj)
    set_flat(obj)
    return obj


def tapered_polyline(name, points, radius_start, radius_end, vertices, material):
    parts = []
    count = len(points) - 1
    for index in range(count):
        t0 = index / count
        t1 = (index + 1) / count
        r0 = radius_start * (1.0 - t0) + radius_end * t0
        r1 = radius_start * (1.0 - t1) + radius_end * t1
        parts.append(cone_between(name + "_seg_" + str(index), points[index], points[index + 1], r0, r1, vertices, material))
    return parts


def make_buttress(name, angle, base_radius, reach, height, material):
    side = Vector((-math.sin(angle), math.cos(angle), 0.0))
    inner = Vector((math.cos(angle) * base_radius * 0.42, math.sin(angle) * base_radius * 0.42, 0.04))
    outer = Vector((math.cos(angle) * reach, math.sin(angle) * reach, 0.03))
    top = Vector((math.cos(angle) * base_radius * 0.82, math.sin(angle) * base_radius * 0.82, height))
    width_inner = base_radius * 0.18
    width_outer = base_radius * 0.09
    verts = [
        tuple(inner - side * width_inner),
        tuple(inner + side * width_inner),
        tuple(outer + side * width_outer),
        tuple(outer - side * width_outer),
        tuple(top),
    ]
    faces = [
        (0, 1, 4),
        (1, 2, 4),
        (2, 3, 4),
        (3, 0, 4),
        (0, 3, 2, 1),
    ]
    mesh = bpy.data.meshes.new(name + "_mesh")
    mesh.from_pydata(verts, [], faces)
    mesh.update()
    obj = bpy.data.objects.new(name, mesh)
    bpy.context.collection.objects.link(obj)
    obj.data.materials.append(material)
    set_flat(obj)
    return obj


def make_irregular_trunk(asset_id, height, base_radius, top_radius, lean, broken, mats, knots=2):
    verts = []
    faces = []
    sides = 14
    rings = 8
    for ring in range(rings):
        t = ring / (rings - 1)
        z = height * t
        center_x = lean[0] * t + math.sin(t * math.pi * 2.0) * 0.065 + math.sin(t * math.pi * 4.6 + base_radius) * 0.025
        center_y = lean[1] * t + math.cos(t * math.pi * 1.5) * 0.055 + math.cos(t * math.pi * 3.7 + height) * 0.025
        radius = base_radius * (1.0 - t) + top_radius * t
        if t < 0.22:
            radius *= 1.0 + (0.22 - t) * 1.45
        if broken and ring == rings - 1:
            radius *= 0.64
        for side in range(sides):
            angle = side / sides * math.tau
            irregular = 1.0 + 0.20 * math.sin(side * 1.7 + ring * 0.9) + 0.07 * math.cos(side * 2.6 - ring * 1.4)
            verts.append((
                center_x + math.cos(angle) * radius * irregular,
                center_y + math.sin(angle) * radius * (1.0 + 0.14 * math.cos(side * 2.1 + ring)),
                z + (0.28 * math.sin(angle * 2.0) if broken and ring == rings - 1 else 0.0),
            ))
    for ring in range(rings - 1):
        for side in range(sides):
            a = ring * sides + side
            b = ring * sides + (side + 1) % sides
            c = (ring + 1) * sides + (side + 1) % sides
            d = (ring + 1) * sides + side
            faces.append((a, b, c, d))
    faces.append(tuple(range(sides - 1, -1, -1)))
    faces.append(tuple((rings - 1) * sides + side for side in range(sides)))
    mesh = bpy.data.meshes.new(asset_id + "_mesh")
    mesh.from_pydata(verts, [], faces)
    mesh.update()
    obj = bpy.data.objects.new(asset_id, mesh)
    bpy.context.collection.objects.link(obj)
    obj.data.materials.append(mats["myb_bark_warm"])
    set_flat(obj)

    parts = [obj]
    for index in range(knots):
        angle = (index * 2.2 + height) % math.tau
        z = height * (0.28 + 0.18 * index)
        radius = base_radius * (0.34 + 0.06 * (index % 2))
        bpy.ops.mesh.primitive_uv_sphere_add(segments=8, ring_count=4, radius=radius, location=(
            math.cos(angle) * base_radius * 0.92 + lean[0] * (z / height),
            math.sin(angle) * base_radius * 0.92 + lean[1] * (z / height),
            z,
        ))
        knot = bpy.context.object
        knot.name = asset_id + "_knot_" + str(index + 1)
        knot.scale = (0.75, 0.35, 0.5)
        knot.rotation_euler.z = angle
        knot.data.materials.append(mats["myb_bark_dark"])
        apply_transform(knot)
        set_flat(knot)
        parts.append(knot)

    for index, angle in enumerate((0.2, 1.9, 3.45, 4.8)):
        parts.append(make_buttress(asset_id + "_buttress_" + str(index + 1), angle, base_radius, base_radius + 0.68 + index * 0.08, height * (0.22 + 0.03 * (index % 2)), mats["myb_bark_dark"]))
        root = cone_between(
            asset_id + "_base_root_" + str(index + 1),
            (math.cos(angle) * base_radius * 0.25, math.sin(angle) * base_radius * 0.25, 0.08),
            (math.cos(angle + 0.16 * math.sin(index)) * (base_radius + 0.95 + 0.10 * index), math.sin(angle + 0.18 * math.cos(index)) * (base_radius + 0.72), 0.03),
            base_radius * 0.22,
            base_radius * 0.07,
            8,
            mats["myb_bark_dark"],
        )
        parts.append(root)
    for index, angle in enumerate((0.55, 1.45, 2.7, 3.75, 5.25)):
        z0 = height * (0.09 + 0.035 * index)
        z1 = min(height * 0.92, z0 + height * (0.35 + 0.04 * (index % 3)))
        parts.append(cone_between(
            asset_id + "_bark_ridge_" + str(index + 1),
            (math.cos(angle) * base_radius * 1.04 + lean[0] * (z0 / height), math.sin(angle) * base_radius * 1.04 + lean[1] * (z0 / height), z0),
            (math.cos(angle + 0.16) * top_radius * 1.25 + lean[0] * (z1 / height), math.sin(angle + 0.14) * top_radius * 1.25 + lean[1] * (z1 / height), z1),
            base_radius * 0.035,
            top_radius * 0.025,
            5,
            mats["myb_bark_dark"],
        ))
    return join_asset(asset_id, parts)


def make_root_cluster(asset_id, style, mats):
    parts = []
    if style == "arch":
        spine = [
            (-1.48, -0.18, 0.05),
            (-1.18, -0.03, 0.34),
            (-0.78, 0.12, 0.70),
            (-0.24, 0.19, 1.05),
            (0.34, 0.12, 1.18),
            (0.88, -0.08, 0.82),
            (1.23, -0.20, 0.38),
            (1.52, -0.24, 0.05),
        ]
        parts.extend(tapered_polyline(asset_id + "_main_arch", spine, 0.26, 0.12, 10, mats["myb_bark_dark"]))
        twin = [
            (-1.20, 0.28, 0.04),
            (-0.78, 0.36, 0.36),
            (-0.18, 0.34, 0.70),
            (0.42, 0.25, 0.58),
            (0.96, 0.16, 0.24),
            (1.28, 0.10, 0.04),
        ]
        parts.extend(tapered_polyline(asset_id + "_secondary_arch", twin, 0.15, 0.055, 8, mats["myb_bark_dark"]))
        for index, angle in enumerate((0.15, 1.55, 2.75, 3.65, 5.25)):
            points = [
                (math.cos(angle) * 0.20, math.sin(angle) * 0.12, 0.12),
                (math.cos(angle + 0.12) * 0.58, math.sin(angle + 0.20) * 0.28, 0.13),
                (math.cos(angle + 0.28) * 1.02, math.sin(angle + 0.15) * 0.56, 0.075),
                (math.cos(angle + 0.18) * 1.58, math.sin(angle + 0.20) * 0.82, 0.025),
            ]
            parts.extend(tapered_polyline(asset_id + "_ground_foot_" + str(index), points, 0.14, 0.035, 8, mats["myb_bark_dark"]))
        parts.append(lowpoly_ellipsoid(asset_id + "_moss_threshold", (-0.34, -0.04, 0.12), (0.42, 0.22, 0.08), mats["myb_root_moss"], subdivisions=1))
        parts.append(lowpoly_ellipsoid(asset_id + "_moss_side", (0.62, 0.18, 0.09), (0.34, 0.18, 0.06), mats["myb_root_moss"], subdivisions=1))
    else:
        count = 8 if style == "ground" else 7
        spread_y = 1.04 if style == "ground" else 0.62
        for idx in range(count):
            angle = -1.45 + idx * (2.9 / max(1, count - 1))
            length = 1.34 + 0.28 * ((idx + 1) % 3)
            bend = 0.32 * math.sin(idx * 1.4)
            points = [
                (math.cos(angle) * 0.10, math.sin(angle) * 0.06, 0.18),
                (math.cos(angle + bend * 0.45) * length * 0.30, math.sin(angle - bend * 0.25) * spread_y * 0.34, 0.15 + 0.04 * (idx % 2)),
                (math.cos(angle + bend) * length * 0.64, math.sin(angle - bend * 0.50) * spread_y * 0.62, 0.09),
                (math.cos(angle + bend * 0.5) * length, math.sin(angle + bend * 0.4) * spread_y, 0.030),
            ]
            parts.extend(tapered_polyline(asset_id + "_root_" + str(idx), points, 0.18 - idx * 0.006, 0.035, 9, mats["myb_bark_dark"]))
        mound = lowpoly_ellipsoid(asset_id + "_moss_mound", (0, 0, 0.10), (0.72, 0.44, 0.16), mats["myb_root_moss"], subdivisions=1)
        distort_mesh(mound, 0.11)
        parts.append(mound)
        parts.append(lowpoly_ellipsoid(asset_id + "_side_moss", (-0.35, 0.22, 0.08), (0.32, 0.18, 0.06), mats["myb_root_moss"], subdivisions=1))
    return join_asset(asset_id, parts)


def lowpoly_ellipsoid(name, location, scale, material, subdivisions=1):
    bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=subdivisions, radius=1.0, location=location)
    obj = bpy.context.object
    obj.name = name
    obj.scale = scale
    obj.data.materials.append(material)
    apply_transform(obj)
    set_flat(obj)
    return obj


def make_rock(asset_id, marker, mats):
    parts = []
    base_scale = (0.95, 0.58, 0.50) if not marker else (0.78, 0.62, 1.02)
    rock = lowpoly_ellipsoid(asset_id + "_body", (0, 0, base_scale[2] * 0.88), base_scale, mats["myb_rock_cool"], subdivisions=2)
    distort_mesh(rock, 0.24 if marker else 0.20)
    parts.append(rock)
    moss = lowpoly_ellipsoid(asset_id + "_moss_cap", (-0.14, 0.08, base_scale[2] * 1.58), (base_scale[0] * 0.58, base_scale[1] * 0.42, 0.12), mats["myb_rock_moss"], subdivisions=1)
    distort_mesh(moss, 0.08)
    parts.append(moss)
    side_moss = lowpoly_ellipsoid(asset_id + "_moss_side", (base_scale[0] * 0.22, -base_scale[1] * 0.45, base_scale[2] * 1.02), (base_scale[0] * 0.28, 0.10, 0.16), mats["myb_rock_moss"], subdivisions=1)
    parts.append(side_moss)
    if marker:
        ridge = cone_between(asset_id + "_ancient_lip", (-0.18, -0.06, base_scale[2] * 1.58), (0.26, 0.02, base_scale[2] * 1.86), 0.065, 0.035, 5, mats["myb_rock_moss"])
        parts.append(ridge)
    return join_asset(asset_id, parts)


def distort_mesh(obj, amount):
    for vertex in obj.data.vertices:
        factor = 1.0 + amount * math.sin(vertex.co.x * 2.31 + vertex.co.y * 4.17 + vertex.co.z * 1.73)
        vertex.co.x *= factor
        vertex.co.y *= 1.0 + amount * 0.5 * math.cos(vertex.co.z * 2.4)
    obj.data.update()


def make_fern(asset_id, scale, mats):
    parts = []
    for stem_index, base_offset in enumerate((-0.10, 0.08)):
        stem_height = (0.78 + 0.08 * stem_index) * scale
        stem = cone_between(asset_id + "_stem_" + str(stem_index), (base_offset, 0, 0.02), (base_offset * 0.45, 0.02 * stem_index, stem_height), 0.035 * scale, 0.018 * scale, 6, mats["myb_fern_deep"])
        parts.append(stem)
        for idx in range(7):
            side = -1 if idx % 2 == 0 else 1
            z = 0.16 * scale + idx * 0.075 * scale
            length = (0.46 - idx * 0.024) * scale
            angle = side * (0.82 + idx * 0.045) + base_offset * 0.45
            parts.append(make_leaf_plane(
                asset_id + "_frond_" + str(stem_index) + "_" + str(idx),
                (base_offset * (1.0 - idx * 0.04), 0, z),
                angle,
                length,
                (0.115 - idx * 0.006) * scale,
                mats["myb_fern_lit" if idx % 3 == 0 else "myb_fern_deep"],
            ))
    return join_asset(asset_id, parts)


def make_leaf_plane(name, origin, angle, length, width, material):
    ox, oy, oz = origin
    direction = Vector((math.cos(angle), math.sin(angle), 0.18)).normalized()
    side = Vector((-math.sin(angle), math.cos(angle), 0.0)).normalized()
    base = Vector((ox, oy, oz))
    tip = base + direction * length
    verts = [
        tuple(base - side * width * 0.35),
        tuple(base + side * width * 0.35),
        tuple(tip + side * width),
        tuple(tip - side * width),
    ]
    mesh = bpy.data.meshes.new(name + "_mesh")
    mesh.from_pydata(verts, [], [(0, 1, 2, 3)])
    mesh.update()
    obj = bpy.data.objects.new(name, mesh)
    bpy.context.collection.objects.link(obj)
    obj.data.materials.append(material)
    return obj


def make_floor_mat(asset_id, variant, mats):
    verts = []
    faces = []
    verts.append((0, 0, 0.015))
    edge_count = 12
    for idx in range(edge_count):
        angle = idx / edge_count * math.tau
        radius_x = 0.88 + 0.18 * math.sin(idx * 1.7 + variant)
        radius_y = 0.58 + 0.14 * math.cos(idx * 1.3 - variant)
        notch = 1.0 - (0.20 if idx in (2 + variant % 3, 8) else 0.0)
        x = math.cos(angle) * radius_x * notch
        y = math.sin(angle) * radius_y * notch
        z = 0.018 + 0.035 * (0.5 + 0.5 * math.sin(idx * 2.1 + variant))
        verts.append((x, y, z))
    for idx in range(1, len(verts)):
        faces.append((0, idx, 1 if idx == len(verts) - 1 else idx + 1))
    mesh = bpy.data.meshes.new(asset_id + "_mesh")
    mesh.from_pydata(verts, [], faces)
    mesh.update()
    obj = bpy.data.objects.new(asset_id, mesh)
    bpy.context.collection.objects.link(obj)
    obj.data.materials.append(mats["myb_leaf_floor"])
    set_flat(obj)
    parts = [obj]
    for index, offset in enumerate(((0.12, 0.05), (-0.34, 0.18), (0.42, -0.20))):
        moss = lowpoly_ellipsoid(asset_id + "_moss_patch_" + str(index), (offset[0], offset[1], 0.075 + 0.01 * index), (0.36 - index * 0.045, 0.20 + index * 0.02, 0.042), mats["myb_moss_floor"], subdivisions=1)
        distort_mesh(moss, 0.07)
        parts.append(moss)
    rib = cone_between(asset_id + "_leaf_ridge", (-0.42, -0.14, 0.065), (0.48, 0.18, 0.085), 0.025, 0.016, 5, mats["myb_leaf_floor"])
    parts.append(rib)
    return join_asset(asset_id, parts)


def make_dead_branch(asset_id, twist, mats):
    parts = [
        cylinder_between(asset_id + "_main", (-0.65, 0, 0.09), (0.65, 0.10 * twist, 0.16), 0.055, 7, mats["myb_bark_dark"]),
        cylinder_between(asset_id + "_fork_a", (0.12, 0.03, 0.14), (0.48, 0.38 * twist, 0.25), 0.032, 6, mats["myb_bark_warm"]),
        cylinder_between(asset_id + "_fork_b", (-0.18, 0.0, 0.12), (-0.42, -0.34 * twist, 0.22), 0.026, 6, mats["myb_bark_warm"]),
    ]
    return join_asset(asset_id, parts)


def make_canopy(asset_id, variant, mats):
    parts = []
    offsets = [(-0.72, -0.10, 0.64), (-0.24, 0.22, 0.92), (0.38, 0.04, 0.76), (0.82, -0.22, 0.58), (0.08, 0.56, 0.58), (-0.46, 0.44, 0.52)]
    for idx, offset in enumerate(offsets):
        scale = (
            0.64 - (idx % 3) * 0.055,
            0.36 + (idx % 4) * 0.045,
            0.26 + (idx % 2) * 0.065,
        )
        part = lowpoly_ellipsoid(
            asset_id + "_mass_" + str(idx),
            (offset[0] * variant, offset[1], offset[2]),
            scale,
            mats["myb_canopy_lit" if idx == 1 else "myb_canopy_deep"],
            subdivisions=2,
        )
        distort_mesh(part, 0.17)
        parts.append(part)
    for idx, offset in enumerate(((-0.74, 0.18, 0.46), (0.72, 0.30, 0.42), (0.05, -0.42, 0.50))):
        shelf = make_leaf_plane(
            asset_id + "_leaf_shelf_" + str(idx),
            (offset[0] * variant, offset[1], offset[2]),
            variant * (0.2 + idx * 0.75),
            0.54 - idx * 0.06,
            0.18,
            mats["myb_canopy_lit" if idx == 1 else "myb_canopy_deep"],
        )
        parts.append(shelf)
    return join_asset(asset_id, parts)


def make_fallen_log(asset_id, mats):
    parts = [
        cylinder_between(asset_id + "_body", (-1.25, 0, 0.28), (1.25, 0.18, 0.34), 0.28, 12, mats["myb_bark_warm"]),
        cylinder_between(asset_id + "_broken_end_a", (-1.45, -0.02, 0.28), (-1.18, 0, 0.28), 0.22, 8, mats["myb_bark_warm"]),
        cylinder_between(asset_id + "_branch_stub", (0.28, 0.05, 0.44), (0.48, 0.52, 0.74), 0.08, 7, mats["myb_bark_warm"]),
        lowpoly_ellipsoid(asset_id + "_moss_patch", (-0.20, 0.02, 0.58), (0.62, 0.18, 0.06), mats["myb_root_moss"], subdivisions=1),
    ]
    return join_asset(asset_id, parts)


def join_asset(asset_id, parts):
    bpy.ops.object.select_all(action="DESELECT")
    for part in parts:
        part.select_set(True)
    bpy.context.view_layer.objects.active = parts[0]
    bpy.ops.object.join()
    obj = bpy.context.object
    obj.name = asset_id
    obj.data.name = asset_id + "_mesh"
    set_flat(obj)
    obj.location = (0, 0, 0)
    return obj


def create_asset(spec, mats):
    asset_id = spec["id"]
    if asset_id == "myb_forest_trunk_ancient_a":
        return make_irregular_trunk(asset_id, 3.4, 0.34, 0.19, (0.05, 0.08), False, mats, knots=3)
    if asset_id == "myb_forest_trunk_broken_a":
        return make_irregular_trunk(asset_id, 2.35, 0.32, 0.22, (-0.02, 0.05), True, mats, knots=2)
    if asset_id == "myb_forest_trunk_leaning_a":
        return make_irregular_trunk(asset_id, 3.1, 0.30, 0.16, (0.68, 0.12), False, mats, knots=2)
    if asset_id == "myb_forest_trunk_knotted_a":
        return make_irregular_trunk(asset_id, 3.0, 0.46, 0.23, (0.02, -0.08), False, mats, knots=4)
    if asset_id == "myb_forest_root_cluster_lateral_a":
        return make_root_cluster(asset_id, "lateral", mats)
    if asset_id == "myb_forest_root_cluster_ground_a":
        return make_root_cluster(asset_id, "ground", mats)
    if asset_id == "myb_forest_root_arch_a":
        return make_root_cluster(asset_id, "arch", mats)
    if asset_id == "myb_forest_rock_mossy_a":
        return make_rock(asset_id, False, mats)
    if asset_id == "myb_forest_rock_mossy_b":
        obj = make_rock(asset_id, False, mats)
        obj.scale.x = 0.78
        obj.scale.y = 1.15
        apply_transform(obj)
        return obj
    if asset_id == "myb_forest_rock_marker_a":
        return make_rock(asset_id, True, mats)
    if asset_id == "myb_forest_fern_a":
        return make_fern(asset_id, 1.0, mats)
    if asset_id == "myb_forest_fern_b":
        return make_fern(asset_id, 0.82, mats)
    if asset_id == "myb_forest_fern_c":
        return make_fern(asset_id, 1.12, mats)
    if asset_id == "myb_forest_leaf_moss_mat_a":
        return make_floor_mat(asset_id, 1, mats)
    if asset_id == "myb_forest_leaf_moss_mat_b":
        obj = make_floor_mat(asset_id, 2, mats)
        obj.scale = (0.85, 1.12, 1.0)
        apply_transform(obj)
        return obj
    if asset_id == "myb_forest_leaf_moss_mat_c":
        obj = make_floor_mat(asset_id, 3, mats)
        obj.scale = (1.2, 0.78, 1.0)
        apply_transform(obj)
        return obj
    if asset_id == "myb_forest_dead_branch_a":
        return make_dead_branch(asset_id, 1.0, mats)
    if asset_id == "myb_forest_dead_branch_b":
        return make_dead_branch(asset_id, -1.0, mats)
    if asset_id == "myb_forest_canopy_mass_a":
        return make_canopy(asset_id, 1.0, mats)
    if asset_id == "myb_forest_canopy_mass_b":
        return make_canopy(asset_id, -1.0, mats)
    if asset_id == "myb_forest_fallen_log_a":
        return make_fallen_log(asset_id, mats)
    raise ValueError("Unknown asset " + asset_id)


def collect_metrics(obj, spec):
    depsgraph = bpy.context.evaluated_depsgraph_get()
    mesh = obj.evaluated_get(depsgraph).to_mesh()
    mesh.calc_loop_triangles()
    triangle_count = len(mesh.loop_triangles)
    obj.evaluated_get(depsgraph).to_mesh_clear()
    dimensions = obj.dimensions
    material_names = [slot.material.name for slot in obj.material_slots if slot.material]
    warnings = []
    if triangle_count > spec["budget"]:
        warnings.append("Triangle count exceeds fallback V0 budget; review silhouette before reducing.")
    if len(material_names) > 2:
        warnings.append("Material count exceeds V0 max 2.")
    return {
        "id": spec["id"],
        "name": spec["name"],
        "family": spec["family"],
        "variant": spec["variant"],
        "unityPath": UNITY_REL_DIR + "/" + spec["id"] + ".fbx",
        "dimensionsMeters": {
            "x": round(dimensions.x, 3),
            "y": round(dimensions.y, 3),
            "z": round(dimensions.z, 3),
        },
        "triangleCount": triangle_count,
        "triangleBudgetV0": spec["budget"],
        "materialCount": len(material_names),
        "materialNames": material_names,
        "pivotOriginNote": "Origin at ground/base center, Unity scale 1 unit = 1 meter.",
        "boundsNote": "Bounds generated from procedural mesh; base intended to sit on ground plane.",
        "exportFormat": "FBX",
        "knownWarnings": warnings,
    }


def export_asset(obj, spec):
    bpy.ops.object.select_all(action="DESELECT")
    obj.select_set(True)
    bpy.context.view_layer.objects.active = obj
    export_path = os.path.join(UNITY_DIR, spec["id"] + ".fbx")
    bpy.ops.export_scene.fbx(
        filepath=export_path,
        use_selection=True,
        apply_unit_scale=True,
        apply_scale_options="FBX_SCALE_UNITS",
        bake_space_transform=False,
        object_types={"MESH"},
        mesh_smooth_type="FACE",
        add_leaf_bones=False,
    )


def layout_assets(objects):
    spacing_x = 3.2
    spacing_y = 3.0
    for index, obj in enumerate(objects):
        row = index // 5
        col = index % 5
        obj.location.x = (col - 2) * spacing_x
        obj.location.y = (2 - row) * spacing_y
        label = bpy.data.curves.new(obj.name + "_label_curve", "FONT")
        label.body = obj.name.replace("myb_forest_", "")
        label.size = 0.22
        label.align_x = "CENTER"
        label_obj = bpy.data.objects.new(obj.name + "_label", label)
        label_obj.location = (obj.location.x, obj.location.y - 1.05, 0.02)
        label_obj.rotation_euler = (math.radians(75), 0, 0)
        bpy.context.collection.objects.link(label_obj)


def setup_lighting_and_camera():
    bpy.context.scene.render.engine = "BLENDER_EEVEE"
    bpy.context.scene.view_settings.view_transform = "Standard"
    bpy.context.scene.view_settings.look = "Medium High Contrast"
    bpy.context.scene.view_settings.exposure = 0.2
    bpy.context.scene.view_settings.gamma = 1.0
    bpy.ops.object.light_add(type="AREA", location=(0, -8, 10))
    light = bpy.context.object
    light.name = "MYB147_Preview_Key_Light"
    light.data.energy = 2200
    light.data.size = 8
    bpy.ops.object.camera_add(location=(0, -18, 10.0), rotation=(math.radians(58), 0, 0))
    camera = bpy.context.object
    camera.name = "MYB147_Preview_Camera"
    camera.data.type = "ORTHO"
    camera.data.ortho_scale = 23
    bpy.context.scene.camera = camera
    bpy.context.scene.render.resolution_x = 1800
    bpy.context.scene.render.resolution_y = 1200
    bpy.context.scene.world.color = (0.42, 0.47, 0.43)


def render_preview(path):
    bpy.context.scene.render.filepath = path
    bpy.ops.render.render(write_still=True)


def write_local_manifests(metrics):
    json_path = os.path.join(ARTIFACT_DIR, "myb-forest-kit-v0-manifest.json")
    md_path = os.path.join(ARTIFACT_DIR, "myb-forest-kit-v0-manifest.md")
    payload = {
        "kit": KIT_NAME,
        "generatedAt": datetime.now(timezone.utc).isoformat(),
        "generator": "_bmad-output/implementation-artifacts/MYB-147/generate_myb_forest_kit_v0.py",
        "unityDirectory": UNITY_REL_DIR,
        "assets": metrics,
    }
    with open(json_path, "w", encoding="utf-8") as handle:
        json.dump(payload, handle, indent=2)
        handle.write("\n")
    lines = [
        "# MYB Forest Kit V0 Manifest",
        "",
        "Status: local MYB-147 kit manifest.",
        "",
        "Generated by: `_bmad-output/implementation-artifacts/MYB-147/generate_myb_forest_kit_v0.py`",
        "",
        "Unity directory:",
        f"`{UNITY_REL_DIR}/`",
        "",
        "| id | family | variant | unity path | dimensions m | triangles | materials | warnings |",
        "|---|---|---|---|---:|---:|---:|---|",
    ]
    for item in metrics:
        dims = item["dimensionsMeters"]
        dim_text = f"{dims['x']} x {dims['y']} x {dims['z']}"
        warnings = "; ".join(item["knownWarnings"]) if item["knownWarnings"] else "None"
        lines.append(
            f"| `{item['id']}` | {item['family']} | {item['variant']} | `{item['unityPath']}` | {dim_text} | {item['triangleCount']} / {item['triangleBudgetV0']} | {item['materialCount']} | {warnings} |"
        )
    lines.extend([
        "",
        "Governance notes:",
        "- Generated procedurally in Blender.",
        "- No Meshy, Tripo, external text-to-3D, or external asset source used.",
        "- Assets are candidate kit pieces, not promoted production assets.",
        "- Isolated previews are intermediate evidence only.",
    ])
    with open(md_path, "w", encoding="utf-8") as handle:
        handle.write("\n".join(lines) + "\n")


def main():
    ensure_dirs()
    clear_scene()
    mats = materials()
    objects = []
    metrics = []
    collection = bpy.data.collections.new(KIT_NAME)
    bpy.context.scene.collection.children.link(collection)
    for spec in ASSET_SPECS:
        obj = create_asset(spec, mats)
        bpy.context.collection.objects.unlink(obj)
        collection.objects.link(obj)
        metrics.append(collect_metrics(obj, spec))
        export_asset(obj, spec)
        objects.append(obj)
    layout_assets(objects)
    setup_lighting_and_camera()
    write_local_manifests(metrics)
    bpy.ops.wm.save_as_mainfile(filepath=os.path.join(ARTIFACT_DIR, "MYB_ForestKit_V0.blend"))
    render_preview(os.path.join(CHECKPOINT_DIR, TIMESTAMP + "-kit-contact-sheet.png"))
    print(json.dumps({
        "kit": KIT_NAME,
        "assetCount": len(metrics),
        "unityDirectory": UNITY_REL_DIR,
        "manifest": os.path.join(ARTIFACT_DIR, "myb-forest-kit-v0-manifest.json"),
        "preview": os.path.join(CHECKPOINT_DIR, TIMESTAMP + "-kit-contact-sheet.png"),
    }, indent=2))


if __name__ == "__main__":
    main()

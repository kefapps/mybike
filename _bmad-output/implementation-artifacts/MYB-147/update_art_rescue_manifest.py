import json
import os
from glob import glob
from datetime import date


REPO_ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), "../../.."))
LOCAL_MANIFEST_PATH = os.path.join(
    REPO_ROOT,
    "_bmad-output",
    "implementation-artifacts",
    "MYB-147",
    "myb-forest-kit-v0-manifest.json",
)
CANONICAL_MANIFEST_PATH = os.path.join(
    REPO_ROOT,
    "docs",
    "manifests",
    "art-rescue-asset-manifest.json",
)
VALIDATOR_REPORT_RELATIVE = "_bmad-output/unity-test-results/myb-144-art-asset-validator-report.md"
CHECKPOINT_DIR = os.path.join(REPO_ROOT, "_bmad-output", "visual-checkpoints", "MYB-147")


def latest_contact_sheet_relative():
    matches = sorted(glob(os.path.join(CHECKPOINT_DIR, "*-kit-contact-sheet.png")))
    if not matches:
        return ""
    return os.path.relpath(matches[-1], REPO_ROOT)


def load_json(path):
    with open(path, "r", encoding="utf-8") as handle:
        return json.load(handle)


def save_json(path, payload):
    with open(path, "w", encoding="utf-8") as handle:
        json.dump(payload, handle, indent=2)
        handle.write("\n")


def build_entry(asset):
    contact_sheet = latest_contact_sheet_relative()
    return {
        "id": asset["id"],
        "name": asset["name"],
        "sourceType": "internal",
        "provider": "Blender MCP / procedural",
        "sourceUrl": "",
        "license": "Project-owned",
        "licenseUrl": "",
        "author": "Kefapps / procedural Blender MCP",
        "acquiredAt": "2026-06-16",
        "intakeStatus": "approved",
        "promotionStatus": "candidate",
        "usageScope": "forest_corridor",
        "assetPaths": [asset["unityPath"]],
        "derivedFrom": [],
        "aiGenerated": False,
        "requiresAttribution": False,
        "attributionText": "",
        "visualImpact": "visible",
        "routeEvidence": [],
        "overviewEvidence": [contact_sheet] if contact_sheet else [],
        "validatorEvidence": [VALIDATOR_REPORT_RELATIVE],
        "notes": (
            "Generated procedurally for MYB-147 as part of MYB_ForestKit_V0. "
            "No external text-to-3D, Meshy, Tripo, or third-party asset source used. "
            "Candidate kit piece only; not promoted production. "
            "Route-camera validation deferred to MYB-148/MYB-150/MYB-151. "
            "Dimensions meters: {x} x {y} x {z}; triangles: {triangles}; materials: {materials}."
        ).format(
            x=asset["dimensionsMeters"]["x"],
            y=asset["dimensionsMeters"]["y"],
            z=asset["dimensionsMeters"]["z"],
            triangles=asset["triangleCount"],
            materials=asset["materialCount"],
        ),
    }


def main():
    local_manifest = load_json(LOCAL_MANIFEST_PATH)
    canonical = load_json(CANONICAL_MANIFEST_PATH)
    existing = {
        asset["id"]: asset
        for asset in canonical.get("assets", [])
        if isinstance(asset, dict) and "id" in asset
    }
    for asset in local_manifest["assets"]:
        existing[asset["id"]] = build_entry(asset)
    canonical["schemaVersion"] = 1
    canonical["updatedAt"] = date.today().isoformat()
    canonical["assets"] = [existing[key] for key in sorted(existing)]
    save_json(CANONICAL_MANIFEST_PATH, canonical)
    print("Updated", CANONICAL_MANIFEST_PATH, "with", len(local_manifest["assets"]), "MYB-147 assets")


if __name__ == "__main__":
    main()

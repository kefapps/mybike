#!/usr/bin/env python3
import base64
import json
import os
import re
import sys
import time
from datetime import datetime
from pathlib import Path
from urllib.parse import urlparse

import requests


BASE_URL = "https://api.meshy.ai"
CONNECT_HOST = os.environ.get("OP_CONNECT_HOST", "http://127.0.0.1:8080").rstrip("/")
CONNECT_TOKEN_ENV_NAMES = ("OP_CONNECT_TOKEN", "ONEPASSWORD_CONNECT_TOKEN", "CONNECT_TOKEN")
VAULT_ID = "5nymhqpsiuw7qfvfglrtze4wdm"
ITEM_ID = "s43ubhxj5ebzjsfxzrdubp25mu"
FIELD_LABEL = "MESHY_API_KEY"

SOURCE_IMAGE = Path("_bmad-output/meshy-captures/myb-95-character-reference-meshy-text-to-image.png")
OUTPUT_ROOT = Path("meshy_output")
CAPTURE_ROOT = Path("_bmad-output/meshy-captures")
SUMMARY_PATH = CAPTURE_ROOT / "myb-95-character-image-to-3d-comparison-summary.json"

RUNS = [
    {
        "label": "15-credit-meshy-5-textured",
        "expected_cost": 15,
        "ai_model": "meshy-5",
        "payload": {
            "ai_model": "meshy-5",
            "model_type": "standard",
            "should_texture": True,
            "enable_pbr": True,
            "should_remesh": False,
            "pose_mode": "t-pose",
            "symmetry_mode": "auto",
            "target_formats": ["fbx"],
            "auto_size": True,
            "origin_at": "bottom",
        },
    },
    {
        "label": "30-credit-meshy-6-textured",
        "expected_cost": 30,
        "ai_model": "meshy-6",
        "payload": {
            "ai_model": "meshy-6",
            "model_type": "standard",
            "should_texture": True,
            "enable_pbr": True,
            "should_remesh": False,
            "pose_mode": "t-pose",
            "symmetry_mode": "auto",
            "target_formats": ["fbx"],
            "auto_size": True,
            "origin_at": "bottom",
            "image_enhancement": True,
            "remove_lighting": True,
        },
    },
]


def fail(message):
    print("ERROR: " + message, flush=True)
    return 1


def connect_token():
    for name in CONNECT_TOKEN_ENV_NAMES:
        token = os.environ.get(name)
        if token:
            return token
    return None


def field_matches(field):
    values = [field.get("id"), field.get("label"), field.get("purpose"), field.get("reference")]
    return any(FIELD_LABEL in str(value) for value in values if value)


def concealed_value(field):
    value = field.get("value")
    if not isinstance(value, str) or not value or value == "REPLACE_WITH_MESHY_API_KEY":
        return None
    return value


def extract_meshy_api_key(item):
    for field in item.get("fields", []):
        if field_matches(field):
            value = concealed_value(field)
            if value:
                return value
    for field in item.get("fields", []):
        if str(field.get("type", "")).upper() == "CONCEALED":
            value = concealed_value(field)
            if value:
                return value
    return None


def load_secret_from_connect():
    token = connect_token()
    if not token:
        return None, "1Password Connect token is not available in the environment"

    session = requests.Session()
    session.trust_env = False
    session.headers.update({"Authorization": "Bearer " + token})
    response = session.get(f"{CONNECT_HOST}/v1/vaults/{VAULT_ID}/items/{ITEM_ID}", timeout=30)
    if response.status_code != 200:
        return None, f"1Password Connect item read failed with HTTP {response.status_code}"

    api_key = extract_meshy_api_key(response.json())
    if not api_key:
        return None, "Meshy API key field is empty or still contains the placeholder"

    return api_key, None


def image_data_uri(path):
    encoded = base64.b64encode(path.read_bytes()).decode("ascii")
    return "data:image/png;base64," + encoded


def balance(session):
    response = session.get(f"{BASE_URL}/openapi/v1/balance", timeout=30)
    response.raise_for_status()
    return response.json().get("balance")


def create_task(session, payload):
    response = session.post(f"{BASE_URL}/openapi/v1/image-to-3d", json=payload, timeout=60)
    if response.status_code >= 400:
        return None, {
            "status_code": response.status_code,
            "body": response.text[:1000],
        }
    return response.json()["result"], None


def poll_task(session, task_id, timeout_seconds=600):
    started = time.monotonic()
    delay = 5
    polls = []

    while True:
        elapsed = int(time.monotonic() - started)
        response = session.get(f"{BASE_URL}/openapi/v1/image-to-3d/{task_id}", timeout=60)
        response.raise_for_status()
        task = response.json()
        status = task.get("status")
        progress = task.get("progress", 0)
        print(f"{task_id} {status} {progress}% after {elapsed}s", flush=True)
        polls.append({"elapsed_seconds": elapsed, "status": status, "progress": progress})

        if status == "SUCCEEDED":
            return task, polls
        if status in {"FAILED", "CANCELED"}:
            return task, polls
        if elapsed >= timeout_seconds:
            task["status"] = "TIMEOUT"
            task["task_error"] = {"message": f"Timeout after {timeout_seconds}s"}
            return task, polls

        current_delay = 15 if progress >= 95 else delay
        time.sleep(current_delay)
        if progress < 95:
            delay = min(int(delay * 1.5), 30)


def slugify(value):
    return re.sub(r"[^a-z0-9]+", "-", value.lower()).strip("-")[:48]


def make_project_dir(label, task_id):
    path = OUTPUT_ROOT / f"{datetime.now().strftime('%Y%m%d_%H%M%S')}_{slugify(label)}_{task_id[:8]}"
    path.mkdir(parents=True, exist_ok=True)
    return path


def is_download_url(value):
    return isinstance(value, str) and urlparse(value).scheme in {"http", "https"}


def download_file(session, url, path):
    response = session.get(url, timeout=300, stream=True)
    response.raise_for_status()
    path.parent.mkdir(parents=True, exist_ok=True)
    with path.open("wb") as file_handle:
        for chunk in response.iter_content(chunk_size=1024 * 256):
            if chunk:
                file_handle.write(chunk)
    return path.stat().st_size


def sanitize_url(url):
    if not is_download_url(url):
        return url
    parsed = urlparse(url)
    return f"{parsed.scheme}://{parsed.netloc}{parsed.path}?[redacted]"


def download_outputs(session, task, project_dir):
    files = []
    model_urls = task.get("model_urls") or {}
    texture_urls = task.get("texture_urls") or []

    fbx_url = model_urls.get("fbx")
    if is_download_url(fbx_url):
        files.append({
            "kind": "model",
            "format": "fbx",
            "path": str(project_dir / "model.fbx"),
            "bytes": download_file(session, fbx_url, project_dir / "model.fbx"),
        })

    thumbnail_url = task.get("thumbnail_url")
    if is_download_url(thumbnail_url):
        files.append({
            "kind": "thumbnail",
            "format": "png",
            "path": str(project_dir / "thumbnail.png"),
            "bytes": download_file(session, thumbnail_url, project_dir / "thumbnail.png"),
        })

    if texture_urls and isinstance(texture_urls, list):
        for texture_index, texture_group in enumerate(texture_urls):
            if not isinstance(texture_group, dict):
                continue
            for texture_name, texture_url in texture_group.items():
                if not is_download_url(texture_url):
                    continue
                extension = Path(urlparse(texture_url).path).suffix or ".png"
                path = project_dir / f"texture_{texture_index}_{texture_name}{extension}"
                files.append({
                    "kind": "texture",
                    "format": texture_name,
                    "path": str(path),
                    "bytes": download_file(session, texture_url, path),
                })

    return files


def summarize_task(task):
    return {
        "status": task.get("status"),
        "progress": task.get("progress"),
        "face_count": task.get("face_count"),
        "created_at": task.get("created_at"),
        "started_at": task.get("started_at"),
        "finished_at": task.get("finished_at"),
        "model_url_formats": sorted((task.get("model_urls") or {}).keys()),
        "texture_url_groups": [
            sorted(group.keys()) for group in task.get("texture_urls", []) if isinstance(group, dict)
        ],
        "thumbnail_url": sanitize_url(task.get("thumbnail_url")),
        "task_error": task.get("task_error"),
    }


def main():
    if not SOURCE_IMAGE.exists():
        return fail(f"Missing source image: {SOURCE_IMAGE}")

    api_key, error = load_secret_from_connect()
    if error:
        return fail(error)

    for name in CONNECT_TOKEN_ENV_NAMES:
        os.environ.pop(name, None)

    session = requests.Session()
    session.trust_env = False
    session.headers.update({"Authorization": "Bearer " + api_key})

    source_data_uri = image_data_uri(SOURCE_IMAGE)
    balance_before = balance(session)
    print(f"BALANCE_BEFORE: {balance_before}", flush=True)

    results = []
    for run in RUNS:
        payload = dict(run["payload"])
        payload["image_url"] = source_data_uri
        print(f"CREATING: {run['label']} expected_cost={run['expected_cost']}", flush=True)
        task_id, create_error = create_task(session, payload)
        if create_error:
            results.append({
                "label": run["label"],
                "expected_cost": run["expected_cost"],
                "ai_model": run["ai_model"],
                "created": False,
                "create_error": create_error,
            })
            print(f"CREATE_FAILED: {run['label']} HTTP {create_error['status_code']}", flush=True)
            continue

        project_dir = make_project_dir(run["label"], task_id)
        task, polls = poll_task(session, task_id)
        files = []
        if task.get("status") == "SUCCEEDED":
            files = download_outputs(session, task, project_dir)

        metadata = {
            "label": run["label"],
            "expected_cost": run["expected_cost"],
            "ai_model": run["ai_model"],
            "task_id": task_id,
            "created": True,
            "source_image": str(SOURCE_IMAGE),
            "project_dir": str(project_dir),
            "payload_without_image": run["payload"],
            "task": summarize_task(task),
            "polls": polls,
            "files": files,
        }
        (project_dir / "metadata.json").write_text(json.dumps(metadata, indent=2), encoding="utf-8")
        results.append(metadata)

    balance_after = balance(session)
    summary = {
        "source_image": str(SOURCE_IMAGE),
        "balance_before": balance_before,
        "balance_after": balance_after,
        "expected_total_cost": sum(run["expected_cost"] for run in RUNS),
        "observed_credit_delta": None
        if balance_before is None or balance_after is None
        else balance_before - balance_after,
        "results": results,
    }
    SUMMARY_PATH.parent.mkdir(parents=True, exist_ok=True)
    SUMMARY_PATH.write_text(json.dumps(summary, indent=2), encoding="utf-8")
    print(json.dumps({
        "balance_before": balance_before,
        "balance_after": balance_after,
        "observed_credit_delta": summary["observed_credit_delta"],
        "summary_path": str(SUMMARY_PATH),
        "tasks": [
            {
                "label": result["label"],
                "task_id": result.get("task_id"),
                "status": result.get("task", {}).get("status"),
                "project_dir": result.get("project_dir"),
                "files": result.get("files", []),
                "create_error": result.get("create_error"),
            }
            for result in results
        ],
    }, indent=2), flush=True)
    return 0


if __name__ == "__main__":
    sys.exit(main())

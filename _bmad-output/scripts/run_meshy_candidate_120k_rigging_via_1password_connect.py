#!/usr/bin/env python3
import base64
import json
import os
import sys
import time
from pathlib import Path
from urllib.parse import urlparse

import requests


BASE_URL = "https://api.meshy.ai"
CONNECT_HOST = os.environ.get("OP_CONNECT_HOST", "http://127.0.0.1:8080").rstrip("/")
CONNECT_TOKEN_ENV_NAMES = ("OP_CONNECT_TOKEN", "ONEPASSWORD_CONNECT_TOKEN", "CONNECT_TOKEN")
VAULT_ID = "5nymhqpsiuw7qfvfglrtze4wdm"
ITEM_ID = "s43ubhxj5ebzjsfxzrdubp25mu"
FIELD_LABEL = "MESHY_API_KEY"

SOURCE_GLB = Path(
    "meshy_output/20260611_210528_30-credit-meshy-6-textured_019eb813/"
    "blender_optimized/myb95_character_candidate_120k.glb"
)
OUTPUT_DIR = SOURCE_GLB.parent / "rigging_candidate_120k"
CAPTURE_ROOT = Path("_bmad-output/meshy-captures")
SUMMARY_PATH = CAPTURE_ROOT / "myb-95-character-candidate-120k-rigging-summary.json"

HEIGHT_METERS = 1.75
EXPECTED_COST = 5

DIRECT_URL_FIELDS = {
    "rigged_character_glb_url": "rigged_character.glb",
    "rigged_character_fbx_url": "rigged_character.fbx",
}

ANIMATION_URL_FIELDS = {
    "walking_glb_url": "walking.glb",
    "walking_fbx_url": "walking.fbx",
    "walking_armature_glb_url": "walking_armature.glb",
    "walking_armature_fbx_url": "walking_armature.fbx",
    "running_glb_url": "running.glb",
    "running_fbx_url": "running.fbx",
    "running_armature_glb_url": "running_armature.glb",
    "running_armature_fbx_url": "running_armature.fbx",
}


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
    identifiers = [
        field.get("id"),
        field.get("label"),
        field.get("purpose"),
        field.get("reference"),
    ]
    return any(FIELD_LABEL in str(value) for value in identifiers if value)


def concealed_value(field):
    value = field.get("value")
    if not isinstance(value, str) or not value:
        return None
    if value == "REPLACE_WITH_MESHY_API_KEY":
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
    response = session.get(
        f"{CONNECT_HOST}/v1/vaults/{VAULT_ID}/items/{ITEM_ID}",
        timeout=30,
    )
    if response.status_code != 200:
        return None, f"1Password Connect item read failed with HTTP {response.status_code}"

    api_key = extract_meshy_api_key(response.json())
    if not api_key:
        return None, "Meshy API key field is empty or still contains the placeholder"

    return api_key, None


def glb_data_uri(path):
    encoded = base64.b64encode(path.read_bytes()).decode("ascii")
    return "data:model/gltf-binary;base64," + encoded


def balance(session):
    response = session.get(f"{BASE_URL}/openapi/v1/balance", timeout=30)
    response.raise_for_status()
    return response.json().get("balance")


def create_rigging_task(session, payload):
    response = session.post(f"{BASE_URL}/openapi/v1/rigging", json=payload, timeout=180)
    if response.status_code >= 400:
        return None, {
            "status_code": response.status_code,
            "body": response.text[:1200],
        }

    body = response.json()
    result = body.get("result")
    if isinstance(result, str):
        return result, None
    if isinstance(result, dict):
        for key in ("id", "task_id"):
            if isinstance(result.get(key), str):
                return result[key], None
    for key in ("id", "task_id"):
        if isinstance(body.get(key), str):
            return body[key], None

    return None, {
        "status_code": response.status_code,
        "body": json.dumps(body)[:1200],
    }


def poll_rigging_task(session, task_id, timeout_seconds=900):
    started = time.monotonic()
    delay = 5
    polls = []

    while True:
        elapsed = int(time.monotonic() - started)
        response = session.get(f"{BASE_URL}/openapi/v1/rigging/{task_id}", timeout=60)
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

        current_delay = 20 if progress >= 95 else delay
        time.sleep(current_delay)
        if progress < 95:
            delay = min(int(delay * 1.5), 30)


def is_download_url(value):
    return isinstance(value, str) and urlparse(value).scheme in {"http", "https"}


def sanitize_url(url):
    if not is_download_url(url):
        return url
    parsed = urlparse(url)
    return f"{parsed.scheme}://{parsed.netloc}{parsed.path}?[redacted]"


def download_file(session, url, path):
    response = session.get(url, timeout=300, stream=True)
    response.raise_for_status()
    path.parent.mkdir(parents=True, exist_ok=True)
    with path.open("wb") as file_handle:
        for chunk in response.iter_content(chunk_size=1024 * 256):
            if chunk:
                file_handle.write(chunk)
    return path.stat().st_size


def task_payloads(task):
    payloads = [task]
    result = task.get("result")
    if isinstance(result, dict):
        payloads.append(result)
    return payloads


def first_download_url(payloads, key):
    for payload in payloads:
        value = payload.get(key)
        if is_download_url(value):
            return value
    return None


def first_animation_payload(payloads):
    for payload in payloads:
        value = payload.get("basic_animations")
        if isinstance(value, dict):
            return value
    return {}


def download_outputs(session, task):
    OUTPUT_DIR.mkdir(parents=True, exist_ok=True)
    payloads = task_payloads(task)
    files = []

    for key, filename in DIRECT_URL_FIELDS.items():
        url = first_download_url(payloads, key)
        if not url:
            continue
        path = OUTPUT_DIR / filename
        files.append({
            "key": key,
            "path": str(path),
            "bytes": download_file(session, url, path),
        })

    animations = first_animation_payload(payloads)
    for key, filename in ANIMATION_URL_FIELDS.items():
        url = animations.get(key)
        if not is_download_url(url):
            continue
        path = OUTPUT_DIR / filename
        files.append({
            "key": f"basic_animations.{key}",
            "path": str(path),
            "bytes": download_file(session, url, path),
        })

    return files


def sanitize_task(task):
    sanitized = {}
    for key, value in task.items():
        if isinstance(value, str) and key.endswith("_url"):
            sanitized[key] = sanitize_url(value)
        elif key == "basic_animations" and isinstance(value, dict):
            sanitized[key] = {
                animation_key: sanitize_url(animation_value)
                if isinstance(animation_value, str) and animation_key.endswith("_url")
                else animation_value
                for animation_key, animation_value in value.items()
            }
        elif key == "result" and isinstance(value, dict):
            sanitized[key] = sanitize_task(value)
        elif key in {
            "id",
            "task_id",
            "status",
            "progress",
            "created_at",
            "started_at",
            "finished_at",
            "task_error",
        }:
            sanitized[key] = value
    return sanitized


def write_json(path, payload):
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(payload, indent=2), encoding="utf-8")


def main():
    if not SOURCE_GLB.exists():
        return fail(f"Missing source GLB: {SOURCE_GLB}")

    api_key, error = load_secret_from_connect()
    if error:
        return fail(error)

    for name in CONNECT_TOKEN_ENV_NAMES:
        os.environ.pop(name, None)

    session = requests.Session()
    session.trust_env = False
    session.headers.update({"Authorization": "Bearer " + api_key})

    balance_before = balance(session)
    print(f"BALANCE_BEFORE: {balance_before}", flush=True)

    payload = {
        "model_url": glb_data_uri(SOURCE_GLB),
        "height_meters": HEIGHT_METERS,
    }
    print(
        "CREATING_RIGGING: "
        f"source={SOURCE_GLB} source_bytes={SOURCE_GLB.stat().st_size} "
        f"height_meters={HEIGHT_METERS} expected_cost={EXPECTED_COST}",
        flush=True,
    )
    task_id, create_error = create_rigging_task(session, payload)
    if create_error:
        balance_after = balance(session)
        summary = {
            "source_glb": str(SOURCE_GLB),
            "output_dir": str(OUTPUT_DIR),
            "height_meters": HEIGHT_METERS,
            "expected_cost": EXPECTED_COST,
            "balance_before": balance_before,
            "balance_after": balance_after,
            "observed_credit_delta": None
            if balance_before is None or balance_after is None
            else balance_before - balance_after,
            "created": False,
            "create_error": create_error,
        }
        write_json(SUMMARY_PATH, summary)
        print(json.dumps(summary, indent=2), flush=True)
        return 1

    task, polls = poll_rigging_task(session, task_id)
    files = []
    if task.get("status") == "SUCCEEDED":
        files = download_outputs(session, task)

    balance_after = balance(session)
    metadata = {
        "source_glb": str(SOURCE_GLB),
        "output_dir": str(OUTPUT_DIR),
        "height_meters": HEIGHT_METERS,
        "expected_cost": EXPECTED_COST,
        "balance_before": balance_before,
        "balance_after": balance_after,
        "observed_credit_delta": None
        if balance_before is None or balance_after is None
        else balance_before - balance_after,
        "task_id": task_id,
        "created": True,
        "task": sanitize_task(task),
        "polls": polls,
        "files": files,
    }
    write_json(OUTPUT_DIR / "metadata.json", metadata)
    write_json(SUMMARY_PATH, metadata)
    print(json.dumps({
        "balance_before": balance_before,
        "balance_after": balance_after,
        "observed_credit_delta": metadata["observed_credit_delta"],
        "task_id": task_id,
        "status": task.get("status"),
        "output_dir": str(OUTPUT_DIR),
        "summary_path": str(SUMMARY_PATH),
        "files": files,
        "task_error": task.get("task_error"),
    }, indent=2), flush=True)

    if task.get("status") != "SUCCEEDED":
        return 1
    return 0


if __name__ == "__main__":
    sys.exit(main())

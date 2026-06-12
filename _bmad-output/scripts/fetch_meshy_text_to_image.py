#!/usr/bin/env python3
import json
import os
import sys
from pathlib import Path
from urllib.parse import urlparse

import requests


BASE_URL = "https://api.meshy.ai"
TASK_ID = "019eb7f6-b539-7e2c-aecf-814dce6d70fd"
OUTPUT_IMAGE = Path("_bmad-output/meshy-captures/myb-95-character-reference-meshy-text-to-image.png")
OUTPUT_SUMMARY = Path("_bmad-output/meshy-captures/myb-95-character-reference-meshy-text-to-image-summary.json")


def fail(message):
    print("ERROR: " + message)
    return 1


def is_probable_image_url(value):
    if not isinstance(value, str):
        return False

    parsed = urlparse(value)
    if parsed.scheme not in {"http", "https"}:
        return False

    lower_path = parsed.path.lower()
    return lower_path.endswith((".png", ".jpg", ".jpeg", ".webp")) or "assets.meshy.ai" in parsed.netloc


def find_urls(value, path="$"):
    found = []
    if isinstance(value, dict):
        for key, child in value.items():
            found.extend(find_urls(child, path + "." + str(key)))
    elif isinstance(value, list):
        for index, child in enumerate(value):
            found.extend(find_urls(child, f"{path}[{index}]"))
    elif is_probable_image_url(value):
        found.append((path, value))
    return found


def sanitize(value):
    if isinstance(value, dict):
        return {key: sanitize(child) for key, child in value.items()}
    if isinstance(value, list):
        return [sanitize(child) for child in value]
    if is_probable_image_url(value):
        parsed = urlparse(value)
        return f"{parsed.scheme}://{parsed.netloc}{parsed.path}?[redacted]"
    return value


def task_get(session, endpoint):
    url = f"{BASE_URL}{endpoint}/{TASK_ID}"
    response = session.get(url, timeout=30)
    payload = None
    try:
        payload = response.json()
    except ValueError:
        payload = {"raw_text": response.text[:500]}

    return {
        "endpoint": endpoint,
        "status_code": response.status_code,
        "payload": payload,
        "image_urls": find_urls(payload),
    }


def stream_get(session, endpoint):
    url = f"{BASE_URL}{endpoint}/{TASK_ID}/stream"
    response = session.get(url, headers={"Accept": "text/event-stream"}, timeout=30, stream=True)
    events = []
    image_urls = []

    for line in response.iter_lines():
        if not line:
            continue

        decoded = line.decode("utf-8", errors="replace")
        if not decoded.startswith("data:"):
            continue

        try:
            payload = json.loads(decoded[5:].strip())
        except ValueError:
            payload = {"raw_event": decoded[:500]}

        events.append(payload)
        image_urls.extend(find_urls(payload))
        if payload.get("status") in {"SUCCEEDED", "FAILED", "CANCELED"}:
            break

    response.close()
    return {
        "endpoint": endpoint + "/stream",
        "status_code": response.status_code,
        "payload": events[-1] if events else {"event_count": 0},
        "event_count": len(events),
        "image_urls": image_urls,
    }


def download_image(session, url):
    OUTPUT_IMAGE.parent.mkdir(parents=True, exist_ok=True)
    response = session.get(url, timeout=120, stream=True)
    response.raise_for_status()
    with OUTPUT_IMAGE.open("wb") as file_handle:
        for chunk in response.iter_content(chunk_size=1024 * 128):
            if chunk:
                file_handle.write(chunk)

    return OUTPUT_IMAGE.stat().st_size


def main():
    api_key = os.environ.get("MESHY_API_KEY")
    if not api_key:
        return fail("MESHY_API_KEY is not set")

    session = requests.Session()
    session.trust_env = False
    session.headers.update({"Authorization": "Bearer " + api_key})

    balance_response = session.get(f"{BASE_URL}/openapi/v1/balance", timeout=30)
    if balance_response.status_code != 200:
        return fail(f"Balance check failed with HTTP {balance_response.status_code}")

    balance = balance_response.json().get("balance")
    checks = []
    endpoints = ["/openapi/v1/text-to-image", "/openapi/v1/image-to-image"]
    for endpoint in endpoints:
        checks.append(task_get(session, endpoint))
        checks.append(stream_get(session, endpoint))

    image_urls = []
    for check in checks:
        image_urls.extend(check["image_urls"])

    seen = set()
    unique_urls = []
    for path, url in image_urls:
        if url in seen:
            continue
        seen.add(url)
        unique_urls.append((path, url))

    downloaded = None
    if unique_urls:
        downloaded = {
            "field_path": unique_urls[0][0],
            "bytes": download_image(session, unique_urls[0][1]),
            "local_path": str(OUTPUT_IMAGE),
        }

    summary = {
        "task_id": TASK_ID,
        "balance": balance,
        "downloaded": downloaded,
        "checks": [
            {
                "endpoint": check["endpoint"],
                "status_code": check["status_code"],
                "event_count": check.get("event_count"),
                "image_url_paths": [path for path, _ in check["image_urls"]],
                "payload": sanitize(check["payload"]),
            }
            for check in checks
        ],
    }

    OUTPUT_SUMMARY.parent.mkdir(parents=True, exist_ok=True)
    OUTPUT_SUMMARY.write_text(json.dumps(summary, indent=2), encoding="utf-8")

    print(json.dumps({
        "task_id": TASK_ID,
        "balance": balance,
        "downloaded": downloaded,
        "summary_path": str(OUTPUT_SUMMARY),
        "checked_endpoints": [check["endpoint"] for check in checks],
    }, indent=2))
    return 0 if downloaded else 2


if __name__ == "__main__":
    sys.exit(main())

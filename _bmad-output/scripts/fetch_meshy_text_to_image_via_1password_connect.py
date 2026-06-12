#!/usr/bin/env python3
import importlib.util
import os
import sys
from pathlib import Path

import requests


CONNECT_HOST = os.environ.get("OP_CONNECT_HOST", "http://127.0.0.1:8080").rstrip("/")
CONNECT_TOKEN_ENV_NAMES = ("OP_CONNECT_TOKEN", "ONEPASSWORD_CONNECT_TOKEN", "CONNECT_TOKEN")
VAULT_ID = "5nymhqpsiuw7qfvfglrtze4wdm"
ITEM_ID = "s43ubhxj5ebzjsfxzrdubp25mu"
FIELD_LABEL = "MESHY_API_KEY"
FETCH_SCRIPT = Path(__file__).with_name("fetch_meshy_text_to_image.py")


def fail(message):
    print("ERROR: " + message)
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
        field_type = str(field.get("type", "")).upper()
        if field_type == "CONCEALED":
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


def run_fetch_script():
    spec = importlib.util.spec_from_file_location("fetch_meshy_text_to_image", FETCH_SCRIPT)
    module = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(module)
    return module.main()


def main():
    api_key, error = load_secret_from_connect()
    if error:
        return fail(error)

    os.environ["MESHY_API_KEY"] = api_key
    for name in CONNECT_TOKEN_ENV_NAMES:
        os.environ.pop(name, None)

    return run_fetch_script()


if __name__ == "__main__":
    sys.exit(main())

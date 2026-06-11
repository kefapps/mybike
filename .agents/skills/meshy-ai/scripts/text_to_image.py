#!/usr/bin/env python3
"""Generate images via local Codex image-generation script and save outputs locally.

Usage:
  python3 text_to_image.py --prompt "a cute robot" --out-dir ./out

Env:
  CODEX_HOME (optional, default: ~/.codex)
  CODEX_IMAGE_GEN_SCRIPT (optional path override)
  OPENAI_API_KEY (required by Codex imagegen fallback path)

Docs:
  https://github.com/openai/codex/blob/main/codex-rs/skills/src/assets/samples/imagegen
"""

from __future__ import annotations

import argparse
import os
import re
import subprocess
import sys
import time
from pathlib import Path
from typing import List, Optional

def _slug(s: str, max_len: int = 64) -> str:
    s = s.strip().lower()
    s = re.sub(r"[^a-z0-9]+", "-", s)
    s = re.sub(r"-+", "-", s).strip("-")
    return (s[:max_len] or "codex")


def _resolve_imagegen_script() -> Path:
    override = os.environ.get("CODEX_IMAGE_GEN_SCRIPT")
    if override:
        path = Path(override).expanduser()
    else:
        code_home = Path(os.environ.get("CODEX_HOME", os.path.expanduser("~/.codex"))).expanduser()
        path = code_home / "skills/.system/imagegen/scripts/image_gen.py"

    if not path.exists():
        raise RuntimeError(
            "Codex imagegen script not found. Set CODEX_IMAGE_GEN_SCRIPT to a valid "
            "path (for example ~/.codex/skills/.system/imagegen/scripts/image_gen.py)."
        )
    return path


def _map_meshy_model_to_codex(model: str) -> str:
    """Keep historical compatibility for the old Meshy CLI flag."""
    normalized = (model or "nano-banana").strip().lower()
    if normalized in {"nano-banana", "nano-banana-v2"}:
        return "gpt-image-2"
    if normalized in {"latest", "", "default"}:
        return "gpt-image-2"
    if normalized.startswith("gpt-image-"):
        return normalized
    raise RuntimeError(
        f"Unsupported --ai-model={model!r} for local Codex flow. "
        "Use a GPT image model such as gpt-image-2."
    )


def _map_aspect_ratio(aspect_ratio: Optional[str]) -> Optional[str]:
    if not aspect_ratio:
        return None
    normalized = aspect_ratio.strip().replace(" ", "")
    ratio_to_size = {
        "1:1": "1024x1024",
        "16:9": "1536x864",
        "9:16": "864x1536",
        "4:3": "1536x1152",
        "3:4": "1152x1536",
        "3:2": "1536x1024",
        "2:3": "1024x1536",
        "16/9": "1536x864",
        "9/16": "864x1536",
        "4/3": "1536x1152",
        "3/4": "1152x1536",
        "3/2": "1536x1024",
        "2/3": "1024x1536",
    }
    return ratio_to_size.get(normalized)


def _run_local_codex_text_to_image(
    *,
    prompt: str,
    model: str,
    out_dir: Path,
    aspect_ratio: Optional[str],
    multi_view: bool,
    timeout: int,
) -> list:
    imagegen = _resolve_imagegen_script()
    size = _map_aspect_ratio(aspect_ratio) or "auto"
    count = 4 if multi_view else 1

    cmd = [
        sys.executable,
        str(imagegen),
        "generate",
        "--prompt",
        prompt,
        "--out-dir",
        str(out_dir),
        "--n",
        str(count),
        "--model",
        model,
        "--size",
        size,
        "--no-augment",
    ]

    proc = subprocess.run(
        cmd,
        check=False,
        text=True,
        capture_output=True,
        timeout=timeout,
    )

    if proc.returncode != 0:
        raise RuntimeError(
            f"Codex image generation failed (exit {proc.returncode}).\nSTDOUT:\n{proc.stdout}\nSTDERR:\n{proc.stderr}"
        )

    outputs: List[str] = []
    for line in proc.stdout.splitlines():
        line = line.strip()
        if line.startswith("Wrote "):
            outputs.append(line[len("Wrote "):].strip())

    if not outputs and out_dir.is_dir():
        outputs = [
            str(path)
            for path in sorted(out_dir.glob("*.png"))
            if path.name.startswith("image_") or "image-" in path.name
        ]
    if not outputs:
        raise RuntimeError(
            "Codex image generation completed but no output paths were produced.\n"
            f"STDOUT:\n{proc.stdout}\nSTDERR:\n{proc.stderr}"
        )
    return outputs


def main() -> None:
    ap = argparse.ArgumentParser()
    ap.add_argument("--prompt", required=True)
    ap.add_argument("--ai-model", default="nano-banana", help="Legacy Meshy model flag; mapped to GPT image model when possible.")
    ap.add_argument("--aspect-ratio", default=None, help="Optional e.g. 1:1, 16:9, 4:3.")
    ap.add_argument("--generate-multi-view", action="store_true")
    ap.add_argument("--out-dir", default="./meshy-out")
    ap.add_argument("--timeout", type=int, default=900)
    args = ap.parse_args()

    out_dir = Path(args.out_dir) / f"text-to-image_{_slug(args.prompt)}_{int(time.time())}"
    out_dir.mkdir(parents=True, exist_ok=True)

    model = _map_meshy_model_to_codex(args.ai_model)
    outputs = _run_local_codex_text_to_image(
        prompt=args.prompt,
        model=model,
        out_dir=out_dir,
        aspect_ratio=args.aspect_ratio,
        multi_view=args.generate_multi_view,
        timeout=args.timeout,
    )
    for out_path in outputs:
        print(out_path)


if __name__ == "__main__":
    main()

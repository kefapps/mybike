# meshy-ai

Hybrid helpers for this skill:

- Text → Image: local Codex image generation via `imagegen` script.
- Image → 3D: Meshy REST API and OBJ download.

## Features

- Text-to-Image (2D) generation with optional multi-view output.
- Image-to-3D generation with OBJ (and optional MTL) download.
- Simple polling + download helpers in a shared client.

## Requirements

- Python 3.8+
- OpenAI API key for text → image local generation (`OPENAI_API_KEY`)
- Meshy API key for image → 3D conversions (`MESHY_API_KEY`)

## Setup

Set the required keys in your environment:

```bash
export OPENAI_API_KEY=...
export MESHY_API_KEY=msy-...
```

Optional base URL override:

```bash
export MESHY_BASE_URL=https://api.meshy.ai
```

Optional imagegen path override (local Codex):

```bash
export CODEX_HOME="$HOME/.codex"
export CODEX_IMAGE_GEN_SCRIPT="$HOME/.codex/skills/.system/imagegen/scripts/image_gen.py"
```

## Usage

### Text → Image

```bash
python3 scripts/text_to_image.py \
  --prompt "a cute robot mascot, flat vector style" \
  --out-dir ./meshy-out
```

Optional flags:

- `--ai-model` (default: `nano-banana`)
- `--aspect-ratio` (only when not using multi-view)
- `--generate-multi-view`
- `--timeout` (seconds; default 900)

Outputs are saved to:

```sh
./meshy-out/text-to-image_<taskId>_<slug>/image_1.png
```

### Image → 3D (OBJ)

Local file:

```bash
python3 scripts/image_to_3d_obj.py \
  --image ./input.png \
  --out-dir ./meshy-out
```

Public URL:

```bash
python3 scripts/image_to_3d_obj.py \
  --image-url "https://example.com/input.png" \
  --out-dir ./meshy-out
```

Optional flags:

- `--ai-model` (default: `latest`)
- `--model-type` (`standard` | `lowpoly`)
- `--topology` (`triangle` | `quad`)
- `--target-polycount` (int)
- `--should-texture` / `--no-texture`
- `--timeout` (seconds; default 1800)

Outputs are saved to:

```sh
./meshy-out/image-to-3d_<taskId>_<slug>/model.obj
./meshy-out/image-to-3d_<taskId>_<slug>/model.mtl   # if provided
```

## Notes

- Text → Image is run through local Codex generation synchronously.
- Image → 3D keeps the Meshy async task/create → poll → download flow.
- API docs for Meshy: <https://docs.meshy.ai/en>

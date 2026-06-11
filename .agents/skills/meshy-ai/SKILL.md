---
name: meshy-ai
description: "Generate assets with a hybrid flow: Text-to-2D via local Codex imagegen, and Image-to-3D via Meshy (download OBJ/MTL locally). Use when image-to-3d mesh output is needed and local imagegen is available."
---

# Meshy + local Codex imagegen

Generate local 2D images with Codex and convert images to 3D meshes via Meshy.

## Setup

- Text → Image (local Codex):
  - Set `OPENAI_API_KEY`.
  - Optional overrides:
    - `CODEX_HOME` (default `~/.codex`)
    - `CODEX_IMAGE_GEN_SCRIPT` (explicit script path)
- Image → 3D (Meshy):
  - Set `MESHY_API_KEY=msy-...`
  - Optional: `MESHY_BASE_URL` (defaults to `https://api.meshy.ai`)

## Text → 2D (Text to Image)

Use `scripts/text_to_image.py`.

```bash
python3 skills/public/meshy-ai/scripts/text_to_image.py \
  --prompt "a cute robot mascot, flat vector style" \
  --out-dir ./meshy-out
```

- Downloaded images are written to `./meshy-out/text-to-image_<slug>_<unix_timestamp>/`.

## Image → 3D (always save OBJ)

Use `scripts/image_to_3d_obj.py`.

### Local image

```bash
python3 skills/public/meshy-ai/scripts/image_to_3d_obj.py \
  --image ./input.png \
  --out-dir ./meshy-out
```

### Public URL

```bash
python3 skills/public/meshy-ai/scripts/image_to_3d_obj.py \
  --image-url "https://.../input.png" \
  --out-dir ./meshy-out
```

- Always downloads `model.obj` (and `model.mtl` if provided by Meshy) into `./meshy-out/image-to-3d_<taskId>_<slug>/`.

## Notes

- Text → Image runs local Codex imagegen synchronously (no Meshy task polling).  
- Image → 3D remains Meshy async: create → poll until `status=SUCCEEDED` → download.
- API reference for this skill: `references/api-notes.md`.

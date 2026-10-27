"""Check GameView reveal clipping; the full shot supplies Unity-font glyph pixels."""
from pathlib import Path
import datetime
import hashlib
import json
import math
from PIL import Image, ImageChops

root = Path(__file__).resolve().parents[2]
folder = root / "Reference/Validation"
names = ["black-screen-start", "black-screen-partial", "black-screen-complete", "black-screen-page-image"]
shots = {name: Image.open(folder / (name + ".png")).convert("RGB") for name in names}
checks, errors = [], []

try:
    start, partial, full = [shots[name] for name in names[:3]]
    assert start.size == partial.size == full.size
    assert start.getbbox() is None, "Initial zero-width black screen contains nonblack pixels"
    checks.append({"name": "zero_width_is_completely_black", "passed": True})

    scale = min(full.width / 533, full.height / 320)
    ox, oy = (full.width - 533 * scale) / 2, (full.height - 320 * scale) / 2
    # Source origin x10/y60; capture at ten cycles =>20px revealed.
    # Font line height28 is the observed Unity value, not an original-device measurement.
    left, top = math.ceil(ox + 10 * scale), math.ceil(oy + 60 * scale)
    right, bottom = math.floor(ox + 30 * scale), math.floor(oy + 88 * scale)
    assert partial.getbbox() is not None, "Partial reveal contains no text"
    assert ImageChops.difference(partial.crop((left + 1, top + 1, right - 1, bottom - 1)),
                                 full.crop((left + 1, top + 1, right - 1, bottom - 1))).getbbox() is None
    # Allow one output pixel at the clip boundary for raster rounding.
    outside = partial.copy()
    outside.paste((0, 0, 0), (left - 1, top - 1, right + 1, bottom + 1))
    assert outside.getbbox() is None, "Revealed pixels escaped the 20px/first-line clip"
    checks.append({"name": "partial_reveal_matches_full_glyphs_inside_original_clip", "passed": True,
                   "source_clip": [10, 60, 20, 28], "nonblack_bounds": partial.getbbox()})

    allowed = (math.floor(ox + 10 * scale) - 1, math.floor(oy + 60 * scale) - 1,
               math.ceil(ox + 523 * scale) + 1, math.ceil(oy + (60 + 3 * 28) * scale) + 1)
    outside = full.copy()
    outside.paste((0, 0, 0), allowed)
    assert outside.getbbox() is None, "Full black-screen shot contains UI or scene pixels outside its three lines"
    assert all(full.crop((allowed[0], int(oy + (60 + i * 28) * scale), allowed[2],
                          int(oy + (60 + (i + 1) * 28) * scale))).getbbox() for i in range(3))
    checks.append({"name": "three_lines_on_black_background_without_other_ui", "passed": True})

    image = shots[names[3]]
    expected = Image.open(root / "UnityProject/Assets/Resources/Original/pic/t_00.png").convert("RGBA")
    samples, maximum = 0, 0
    # Pixel centers away from both text and image/viewport edges.
    for y in range(20, min(expected.height - 2, 300), 7):
        for x in range(80, min(expected.width - 2, 530), 7):
            if not 0 <= y + 17 < 320:
                continue
            actual = image.getpixel((int(ox + (x + .5) * scale), int(oy + (y + 17 + .5) * scale)))
            rgba = expected.getpixel((x, y))
            want = tuple((rgba[c] * rgba[3] + 127) // 255 for c in range(3))
            maximum = max(maximum, max(abs(a - b) for a, b in zip(actual, want)))
            samples += 1
    assert maximum <= 2, ("Image pixels differ from source t_00", maximum)
    checks.append({"name": "optional_original_image_position_and_pixels", "passed": True,
                   "source_origin": [0, 17], "samples": samples, "maximum_rgb_difference": maximum})
except Exception as error:
    errors.append(str(error))

inputs = [folder / (name + ".png") for name in names] + [root / "UnityProject/Assets/Resources/Original/pic/t_00.png"]
report = {"utc": datetime.datetime.now(datetime.timezone.utc).isoformat(), "passed": not errors,
          "scope": "Actual Unity GameView black fill, 20px first-line reveal clip, full three-line bounds, original optional-image pixels. Glyph comparison uses the same Unity font; original-device font/raster and physical input remain unverified.",
          "checks": checks, "errors": errors,
          "sha256": {str(p.relative_to(root)): hashlib.sha256(p.read_bytes()).hexdigest() for p in inputs}}
(folder / "black-screen-pixels.json").write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n")
print(json.dumps(report, ensure_ascii=False, indent=2))
raise SystemExit(0 if report["passed"] else 1)

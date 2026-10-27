"""Verify native spirit-menu focus, marker and text clipping in GameView captures."""
import json
from pathlib import Path
from PIL import Image, ImageChops, ImageDraw

ROOT = Path(__file__).resolve().parents[2]
OUT = ROOT / "Reference/Validation"


def point(image, x, y):
    scale = min(image.width / 533, image.height / 320)
    return (int((image.width - 533 * scale) / 2 + (66.5 + x * 1.25) * scale),
            int((image.height - 320 * scale) / 2 + (10 + y * 1.25) * scale))


def main():
    checks = []
    start = Image.open(OUT / "spirit-menu-scroll-start.png").convert("RGB")
    visible = Image.open(OUT / "spirit-menu-scroll-visible.png").convert("RGB")
    delta = ImageChops.difference(start, visible)
    changed = sum(any(p) for p in delta.getdata())
    lo, hi = point(start, 18, 164), point(start, 159, 203)
    allowed = (lo[0] - 2, lo[1] - 2, hi[0] + 2, hi[1] + 2)
    outside = delta.copy()
    ImageDraw.Draw(outside).rectangle(allowed, fill=(0, 0, 0))
    assert changed > 100, "Text did not visibly scroll"
    assert outside.getbbox() is None, f"Scrolling changed pixels outside original clip: {outside.getbbox()}"
    checks.append({"name": "Long text visibly scrolls only within native141x39 clipping region", "changedPixels": changed, "outsideChangedPixels": 0, "allowedPixelBounds": allowed})

    last = Image.open(OUT / "spirit-menu-candidates-last.png").convert("RGB")
    actual = last.getpixel(point(last, 288, 190))
    assert max(abs(a - b) for a, b in zip(actual, (198, 181, 142))) <= 1, f"Candidate focus missing: {actual}"
    checks.append({"name": "Actual fifth visible candidate row uses source198/181/142 focus fill", "actualRGB": actual})

    sign = Image.open(ROOT / "UnityProject/Assets/Resources/Original/pic/u_27.png").convert("RGBA")
    errors = []
    for y in range(sign.height):
        for x in range(sign.width):
            pixel = sign.getpixel((x, y))
            if pixel[3] < 255:
                continue
            actual = last.getpixel(point(last, 296 + x + .5, 51 + 139 - sign.height + y + .5))
            errors.append(max(abs(a - b) for a, b in zip(actual, pixel[:3])))
    assert errors and max(errors) <= 1, f"Original saved scroll marker mismatch: {max(errors) if errors else 'no samples'}"
    checks.append({"name": "Last candidate positions saved u_27 marker at original139px track bottom", "samples": len(errors), "maxRGBDifference": max(errors)})

    names = ["status", "candidates", "candidates-last", "tip", "result", "farewell", "empty", "scroll-start", "scroll-visible"]
    contact = Image.new("RGB", (1440, 3 * 292), (24, 24, 24))
    draw = ImageDraw.Draw(contact)
    for index, name in enumerate(names):
        image = Image.open(OUT / f"spirit-menu-{name}.png").convert("RGB")
        image.thumbnail((480, 270))
        x, y = (index % 3) * 480, (index // 3) * 292
        contact.paste(image, (x, y))
        draw.text((x + 8, y + 272), name, fill="white")
    contact.save(OUT / "spirit-menu-contact.png")
    report = {"passed": True, "checks": checks}
    (OUT / "spirit-menu-pixels.json").write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n")
    print(json.dumps(report, ensure_ascii=False))


if __name__ == "__main__":
    main()

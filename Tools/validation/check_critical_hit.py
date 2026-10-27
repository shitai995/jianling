"""Compare actual GameView captures with the recovered s_34 RGBA texture."""
from datetime import datetime, timezone
from pathlib import Path
import json
import numpy as np
from PIL import Image, ImageDraw

ROOT = Path(__file__).resolve().parents[2]
OUT = ROOT / 'Reference/Validation'
frames = [np.array(Image.open(OUT / f'critical-hit-{i}.png').convert('RGB')) for i in range(7)]
source = np.array(Image.open(ROOT / 'UnityProject/Assets/Resources/Original/action/s_34.png').convert('RGBA'))
checks, metrics, error = [], {}, ''
try:
    assert np.all(source == np.array([255, 255, 255, 77])), 'Original texture is not the expected uniform RGBA tile'
    assert all(np.array_equal(frames[0], frames[i]) for i in [2, 4, 5, 6])
    assert np.array_equal(frames[1], frames[3])
    checks.append('Seven actual GameView captures follow original blank/flash/blank/flash/blank/blank/closed phases exactly')
    height, width, _ = frames[0].shape
    scale = min(width / 533, height / 320)
    offset_x, offset_y = (width - 533 * scale) / 2, (height - 320 * scale) / 2
    deltas = []
    # These source regions lie below the later stage-name/notice/control layers.
    for left, top, right, bottom in [(10, 58, 522, 105), (150, 170, 400, 200), (10, 8, 220, 45)]:
        for y in range(top, bottom, 3):
            for x in range(left, right, 3):
                px, py = int(offset_x + (x + .5) * scale), int(offset_y + (y + .5) * scale)
                expected = np.rint(frames[0][py, px].astype(float) * (178 / 255) + 77)
                deltas.append(float(np.abs(frames[1][py, px].astype(float) - expected).max()))
    metrics.update(samples=len(deltas), maxRgbDifference=max(deltas))
    assert max(deltas) <= 1, metrics
    checks.append(f'{len(deltas)} independent scene/HUD samples match original RGBA(255,255,255,77) alpha blending; maximum RGB difference {max(deltas):g}')
    changed = np.any(frames[0] != frames[1], axis=2)
    metrics['changedPixels'] = int(changed.sum())
    assert metrics['changedPixels'] > 1000
    checks.append('Critical overlay visibly changes the scene and HUD in actual GameView')
    for frame in frames:
        assert not frame[:, :int(offset_x)].any()
        assert not frame[:, width-int(offset_x):].any()
    checks.append('All seven GameView captures retain black letterbox bars')
except Exception as exc:
    error = f'{type(exc).__name__}: {exc}'

contact = Image.new('RGB', (533 * 3, 350), '#171b24')
draw = ImageDraw.Draw(contact)
for col, (index, label) in enumerate([(0, 'Before'), (1, 'Critical flash'), (6, 'After')]):
    draw.text((col * 533 + 10, 8), label, fill='white')
    image = Image.fromarray(frames[index])
    image = image.crop((61, 0, 1859, 1080)).resize((533, 320), Image.Resampling.NEAREST)
    contact.paste(image, (col * 533, 30))
contact.save(OUT / 'critical-hit-contact.png')
report = dict(passed=not error, utc=datetime.now(timezone.utc).isoformat(), checks=checks, metrics=metrics, error=error)
(OUT / 'critical-hit-pixels.json').write_text(json.dumps(report, ensure_ascii=False, indent=2) + '\n')
print(json.dumps(report, ensure_ascii=False))
if error:
    raise SystemExit(1)

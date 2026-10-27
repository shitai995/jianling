"""Check and assemble frozen GameView sword-break captures; uses installed Pillow."""
from pathlib import Path
from datetime import datetime, timezone
import json
from PIL import Image, ImageChops, ImageDraw

ROOT = Path(__file__).resolve().parents[2]
OUT = ROOT / 'Reference/Validation'
frames = [Image.open(OUT / f'sword-break-{i}.png').convert('RGB') for i in range(6)]
checks = []
error = ''
try:
    assert all(f.size == (1920, 1080) for f in frames), 'Unexpected GameView target size'
    checks.append('All six captures retain the existing 1920x1080 GameView target')
    assert ImageChops.difference(frames[0], frames[2]).getbbox() is None, 'Initial counter0 must use abs(-1)%4, source frame4'
    assert ImageChops.difference(frames[4], frames[5]).getbbox() is None, 'Final break frame must settle on the same source broken image'
    checks.append('Actual counter0 matches source index1 and counter4 matches the static broken frame after the flag clears')
    distinct = [f.crop((230, 68, 745, 165)).tobytes() for f in frames[1:5]]
    assert len(set(distinct)) == 4, 'Four source phases were not distinct'
    checks.append('All four ordinary break phases have distinct rendered pixels at the original HUD location')
    for f in frames:
        assert f.crop((0, 0, 60, 1080)).getbbox() is None and f.crop((1860, 0, 1920, 1080)).getbbox() is None, 'Viewport black bars changed'
    for f in frames[1:]:
        bbox = ImageChops.difference(frames[0], f).getbbox()
        if bbox is not None:
            # Saved source frames3..6 include flying fragments: original bounds (38,5)-(204,55).
            assert bbox[0] >= 188 and bbox[1] >= 16 and bbox[2] <= 750 and bbox[3] <= 186, f'Unexpected changes outside sword HUD: {bbox}'
    checks.append('Frozen captures differ only around the sword HUD; both viewport side bars remain black')
except Exception as exc:
    error = str(exc)

# QA contact sheet: crops of actual output, not generated game artwork.
contact = Image.new('RGB', (1520, 660), '#171b24')
draw = ImageDraw.Draw(contact)
labels = ['counter0: initial', 'counter1: frame3', 'counter2: frame4', 'counter3: frame5', 'counter4: frame6', 'cleared: static']
for i, (f, label) in enumerate(zip(frames, labels)):
    x, y = (i % 2) * 760, (i // 2) * 220
    draw.text((x + 8, y + 4), label, fill='white')
    contact.paste(f.crop((60, 0, 820, 190)), (x, y + 25))
contact.save(OUT / 'sword-break-contact.png')
report = dict(passed=not error, utc=datetime.now(timezone.utc).isoformat(), checks=checks, error=error)
(OUT / 'sword-break-pixels.json').write_text(json.dumps(report, ensure_ascii=False, indent=2) + '\n')
print(json.dumps(report, ensure_ascii=False))
if error:
    raise SystemExit(1)

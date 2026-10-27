"""Check the source type11 aura in frozen Unity GameView captures."""
from pathlib import Path
from datetime import datetime, timezone
import json
from PIL import Image, ImageChops, ImageDraw

OUT = Path(__file__).resolve().parents[2] / 'Reference/Validation'
frames = {n: Image.open(OUT / f'enemy-aura-{n}.png').convert('RGB')
          for n in ['blank', 'visible', 'covered-blank', 'covered', 'critical', 'hideui']}
checks, metrics, error = [], {}, ''
def mask(a, b):
    r, g, b = ImageChops.difference(a, b).split()
    return ImageChops.lighter(ImageChops.lighter(r, g), b).point(lambda v: 255 if v else 0)

visible = mask(frames['visible'], frames['blank'])
covered = mask(frames['covered'], frames['covered-blank'])
box = visible.getbbox()
try:
    metrics = {'visiblePixels': visible.histogram()[255], 'coveredPixels': covered.histogram()[255]}
    assert metrics['visiblePixels'] > 1000, metrics
    checks.append('The saved source aura produces visible pixels in actual GameView')
    assert 0 < metrics['coveredPixels'] < metrics['visiblePixels'], metrics
    assert ImageChops.subtract(covered, visible).getbbox() is None, 'Occlusion introduced pixels outside the original aura'
    checks.append('A source layer1 golem physically obscures part of the layer2 elite aura; its remaining visible pixels are a subset of the unoccluded aura')
    assert frames['critical'].crop(box).tobytes() == frames['blank'].crop(box).tobytes(), 'Critical aura region differs from the aura-free baseline'
    checks.append('Player critical24 removes the aura while the body and background within the complete aura bounds match the aura-free baseline')
    assert frames['hideui'].crop(box).tobytes() == frames['visible'].crop(box).tobytes()
    for f in frames.values():
        assert f.crop((0,0,60,1080)).getbbox() is None and f.crop((1860,0,1920,1080)).getbbox() is None
    checks.append('Hide-UI preserves every aura pixel and all captures retain the logical viewport black side bars')
except Exception as exc:
    error = f'{type(exc).__name__}: {exc}'

if box:
    crop = (box[0]-20, box[1]-20, box[2]+20, box[3]+20)
    width, height = crop[2]-crop[0], crop[3]-crop[1]
    contact = Image.new('RGB', (width*3, height+30), '#171b24')
    draw = ImageDraw.Draw(contact)
    for i, name in enumerate(['visible', 'covered', 'critical']):
        draw.text((i*width+8,8), name, fill='white')
        contact.paste(frames[name].crop(crop), (i*width,30))
    contact.save(OUT/'enemy-aura-contact.png')
r = dict(passed=not error, utc=datetime.now(timezone.utc).isoformat(), checks=checks, metrics=metrics, error=error)
(OUT/'enemy-aura-pixels.json').write_text(json.dumps(r, indent=2)+'\n')
print(json.dumps(r))
if error:
    raise SystemExit(1)

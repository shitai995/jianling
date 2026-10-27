"""Compare frozen GameView feedback pixels across actual actor sorting states."""
from pathlib import Path
from datetime import datetime, timezone
import json
from PIL import Image, ImageChops, ImageDraw
ROOT = Path(__file__).resolve().parents[2]
OUT = ROOT / 'Reference/Validation'
checks, metrics, error = [], {}, ''
frames = {}
for name in ['blank', 'visible', 'behind-blank', 'behind', 'front-blank', 'front']:
    frames[name] = Image.open(OUT / f'world-number-{name}.png').convert('RGB')
def mask(a, b):
    return ImageChops.difference(a, b).convert('RGB').point(lambda v: 255 if v else 0).convert('L').point(lambda v: 255 if v else 0)
try:
    masks = {n: mask(frames[n], frames['blank' if n == 'visible' else n+'-blank']) for n in ['visible', 'behind', 'front']}
    metrics = {n: m.histogram()[255] for n, m in masks.items()}
    assert metrics['visible'] > 100, 'Number fixture contains no rendered digits'
    assert ImageChops.difference(masks['visible'], masks['behind']).getbbox() is None, 'Rear actor obscured the number'
    checks.append('The original golem on source layer1 leaves every player-number pixel visible')
    assert 0 < metrics['front'] < metrics['visible'], f'Front actor must obscure part of the number: {metrics}'
    assert ImageChops.subtract(masks['front'], masks['visible']).getbbox() is None, 'Front state introduced non-number changes'
    checks.append('The original golem on source layer2 physically occludes part of the same number in actual GameView pixels')
    miss = {n: Image.open(OUT / f'world-miss-{n}.png').convert('RGB') for n in ['visible','hideui','rise']}
    # Fixed crop excludes HUD, buttons and animated scene: actual miss at the player head.
    crop = (550, 450, 1050, 720)
    assert ImageChops.difference(miss['visible'].crop(crop),miss['hideui'].crop(crop)).getbbox() is None, 'Hide UI suppressed world miss'
    assert ImageChops.difference(miss['visible'].crop(crop),miss['rise'].crop(crop)).getbbox() is not None, 'Miss did not rise'
    checks.append('Hide-UI retains the world miss and the next source tick changes its rendered height')
    for f in list(frames.values())+list(miss.values()):
        assert f.crop((0,0,60,1080)).getbbox() is None and f.crop((1860,0,1920,1080)).getbbox() is None, 'Effect escaped the logical viewport'
    checks.append('Every captured feedback state keeps both logical viewport side bars black')
    maps = {n: Image.open(OUT / f'world-map-{n}.png').convert('RGB') for n in ['blank','covered-blank','covered','above-blank','above']}
    box = ImageChops.difference(maps['above'],maps['above-blank']).getbbox()
    assert box is not None, 'Map fixture contains no number'
    overlap = hidden = 0
    for y in range(box[1],box[3]):
        for x in range(box[0],box[2]):
            p=(x,y)
            if maps['above'].getpixel(p)!=maps['above-blank'].getpixel(p) and maps['covered-blank'].getpixel(p)!=maps['blank'].getpixel(p):
                overlap+=1
                hidden+=maps['covered'].getpixel(p)==maps['covered-blank'].getpixel(p)
    assert overlap>100 and hidden==overlap, f'Original bridge foreground did not obscure all overlapping digits: {hidden}/{overlap}'
    metrics['mapForegroundOverlap']=overlap
    checks.append('Original map100 bridge rail completely covers every overlapping digit pixel on source layer1; layer2 numbers render above it')
except Exception as exc:
    error = str(exc)
contact = Image.new('RGB',(1120,600),'#171b24');draw=ImageDraw.Draw(contact)
for i,name in enumerate(['visible','behind','front']):
    draw.text((i*370+8,8),name,fill='white')
    contact.paste(frames[name].crop((570,530,940,1080)),(i*370,32))
contact.save(OUT/'world-presentation-contact.png')
map_contact=Image.new('RGB',(1110,440),'#171b24');md=ImageDraw.Draw(map_contact)
for i,name in enumerate(['visible','covered','above']):
    md.text((i*370+8,8),name,fill='white')
    map_contact.paste(Image.open(OUT/f'world-map-{name}.png').crop((815,535,1185,935)),(i*370,32))
map_contact.save(OUT/'world-map-contact.png')
report=dict(passed=not error,utc=datetime.now(timezone.utc).isoformat(),checks=checks,metrics=metrics,error=error)
(OUT/'world-presentation-pixels.json').write_text(json.dumps(report,indent=2)+'\n')
print(json.dumps(report))
if error: raise SystemExit(1)

"""Check original enemy-health geometry and colors in frozen Unity GameView captures."""
from pathlib import Path
from datetime import datetime, timezone
import json, math
from PIL import Image, ImageChops, ImageDraw
OUT = Path(__file__).resolve().parents[2] / 'Reference/Validation'
info = json.loads((OUT / 'enemy-health-capture.json').read_text())
bounds = info['logicalBounds']
frames = {hp: Image.open(OUT / f'enemy-health-{hp}.png').convert('RGB') for hp in [0,1,52,53,500,999,1000]}
blank = Image.open(OUT / 'enemy-health-blank.png').convert('RGB')
checks, error = [], ''
scale = min(info['width']/533, info['height']/320)
ox, oy = (info['width']-533*scale)/2, (info['height']-320*scale)/2
point = lambda x,y: (int(ox+(bounds['x']+x+.5)*scale),int(oy+(bounds['y']+y+.5)*scale))
rgb = lambda n: ((n>>16)&255,(n>>8)&255,n&255)
background, fill, highlight, border = map(rgb, [4013368,16776960,15976556,15960172])
rect = (math.floor(ox+bounds['x']*scale),math.floor(oy+bounds['y']*scale),math.ceil(ox+(bounds['x']+21)*scale),math.ceil(oy+(bounds['y']+5)*scale))
try:
    assert (bounds['x'], bounds['y'], bounds['width'], bounds['height']) == (220,176,21,5), bounds
    assert bounds['x'] == 210+(info['footWidth']-20)//2 and bounds['y']==250+info['edgeY']-8
    checks.append('Actual GameView bar bounds are 21x5 logical pixels at source foot-centered x and current edgeRect.y-8')
    for hp, image in frames.items():
        assert image.size == blank.size == (info['width'],info['height'])
        width = hp*19//1000
        for x in range(1,20):
            assert image.getpixel(point(x,2)) == (fill if x<=width else background), (hp,x,'fill')
            for y in [1,3]:
                assert image.getpixel(point(x,y)) == (highlight if x<=max(1,width) else background), (hp,x,y,'highlight')
        for x in range(1,20):
            for y in [0,4]: assert image.getpixel(point(x,y))==border,(hp,x,y,'border')
        for y in range(1,4):
            for x in [0,20]: assert image.getpixel(point(x,y))==border,(hp,x,y,'side')
        for x,y in [(0,0),(20,0),(0,4),(20,4)]:
            assert image.getpixel(point(x,y))==blank.getpixel(point(x,y)),(hp,'transparent corner')
        diff=ImageChops.difference(image,blank);box=diff.getbbox()
        assert box and box[0]>=rect[0] and box[1]>=rect[1] and box[2]<=rect[2] and box[3]<=rect[3],(hp,box)
        assert image.crop((0,0,60,1080)).getbbox() is None and image.crop((1860,0,1920,1080)).getbbox() is None
    checks.append('Seven real captures verify exact original background/yellow/highlight/border RGB values, one-pixel borders, transparent corners and all three interior rows')
    assert frames[1].tobytes()==frames[52].tobytes() and frames[52].tobytes()!=frames[53].tobytes()
    checks.append('1 and52 HP share integer width0; 53 HP gains its first fill pixel, half health has9 pixels, 999 HP has18 and full health has19')
    for name in ['mirror','hideui']:
        image=Image.open(OUT/f'enemy-health-{name}.png').convert('RGB')
        assert image.crop(rect).tobytes()==frames[500].crop(rect).tobytes(),name
    checks.append('Actual enemy mirroring and hide-UI preserve every bar pixel; changes among health values stay inside its source bounds and viewport side bars stay black')
    mi=json.loads((OUT/'enemy-health-map-capture.json').read_text())
    mb=mi['logicalBounds']; ms=min(mi['width']/533,mi['height']/320)
    mx,my=(mi['width']-533*ms)/2,(mi['height']-320*ms)/2
    maps={n:Image.open(OUT/f'enemy-health-map-{n}.png').convert('RGB') for n in ['covered-blank','covered','above-blank','above']}
    alpha=Image.open(OUT.parents[1]/'UnityProject/Assets/Resources/Original/Maps/map100_1.png').convert('RGBA').getchannel('A')
    opaque=transparent=0
    for y in range(5):
        for x in range(21):
            if (x in [0,20]) and (y in [0,4]): continue
            p=(int(mx+(mb['x']+x+.5)*ms),int(my+(mb['y']+y+.5)*ms))
            expected=border if y in [0,4] or x in [0,20] else ((highlight if y in [1,3] else fill) if x<=9 else background)
            assert maps['above'].getpixel(p)==expected,('above',x,y,maps['above'].getpixel(p),expected)
            a=alpha.getpixel((310+x,166+y))
            if a==255:
                opaque+=1
                assert maps['covered'].getpixel(p)==maps['covered-blank'].getpixel(p),('opaque',x,y)
            elif a==0:
                transparent+=1
                assert maps['covered'].getpixel(p)==expected,('transparent',x,y)
    assert opaque>0 and transparent>0,(opaque,transparent)
    checks.append(f'Original map100 foreground alpha obscures all {opaque} sampled opaque bar pixels and preserves {transparent} transparent samples; all 101 non-corner samples render in source layer2')
    contact_map=Image.new('RGB',(960,340),'#171b24');md=ImageDraw.Draw(contact_map)
    cx=int(mx+(mb['x']+10.5)*ms);cy=int(my+(mb['y']+2.5)*ms)
    for i,name in enumerate(['covered-blank','covered','above']):
        md.text((i*320+8,8),name,fill='white')
        contact_map.paste(maps[name].crop((cx-160,cy-100,cx+160,cy+200)),(i*320,32))
    contact_map.save(OUT/'enemy-health-map-contact.png')
except Exception as exc:
    error=f'{type(exc).__name__}: {exc}'
contact=Image.new('RGB',(960,780),'#171b24');draw=ImageDraw.Draw(contact)
for i,hp in enumerate([1,53,500,999,1000,0]):
    x,y=(i%3)*320,(i//3)*390
    draw.text((x+10,y+5),f'HP {hp}/1000, width {hp*19//1000}',fill='white')
    contact.paste(frames[hp].crop((700,560,1020,920)),(x,y+26))
contact.save(OUT/'enemy-health-contact.png')
r=dict(passed=not error,utc=datetime.now(timezone.utc).isoformat(),checks=checks,error=error)
(OUT/'enemy-health-pixels.json').write_text(json.dumps(r,indent=2)+'\n')
print(json.dumps(r))
if error: raise SystemExit(1)

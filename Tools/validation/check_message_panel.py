from pathlib import Path
import json, math, hashlib, datetime
from PIL import Image, ImageDraw

root = Path(__file__).resolve().parents[2]
folder = root / 'Reference/Validation'
meta = json.loads((folder/'message-panel-geometry.json').read_text())['data']['result']['shots']
atlas = Image.open(root/'UnityProject/Assets/Resources/Original/pic/u_40.png').convert('RGBA')
base = Image.open(folder/'message-panel-base.png').convert('RGB')
checks, errors = [], []

def reference(bounds):
    # Independent raster reference for GameCanvas.drawRoundRectBGByTile.
    x,y,w,h = bounds
    tw=th=atlas.height
    out=Image.new('RGBA',(533,320),(0,0,0,0))
    ImageDraw.Draw(out).rectangle((x+2,y+2,x+w-7,y+h-11),fill=(214,140,90,255))
    m=max(0,int((w-2*tw)/tw)); n=max(0,int((h-2*th)/th))
    cw=min(w//2,tw); ch=min(h//2,th)
    right=x+w-tw if w//2>tw else x+w//2
    bottom=y+h-th if h//2>th else y+h//2
    def draw(sx,sy,sw,sh,dx,dy):
        if sw>0 and sh>0: out.alpha_composite(atlas.crop((sx,sy,sx+sw,sy+sh)),(dx,dy))
    if w>=2*tw and m*tw<w-2*tw:
        remain=w-2*tw-m*tw
        draw(4*tw,0,remain,th,x+(m+1)*tw,y)
        draw(5*tw,th-ch,remain,ch,x+(m+1)*tw,bottom)
    if h>=2*th and n*th<h-2*th:
        remain=h-2*th-n*th
        draw(7*tw,0,tw,remain,x,y+(n+1)*th)
        draw(6*tw,0,tw,remain,x+w-tw,y+(n+1)*th)
    for i in range(m):
        draw(4*tw,0,tw,ch,x+(i+1)*tw,y)
        draw(5*tw,th-ch,tw,ch,x+(i+1)*tw,bottom)
    for i in range(n):
        draw(7*tw,0,cw,th,x,y+(i+1)*th)
        draw(6*tw,0,cw,th,right,y+(i+1)*tw)
    for sx,sy,dx,dy in [(0,0,x,y),(tw,0,right,y),(2*tw,th-ch,x,bottom),(3*tw,th-ch,right,bottom)]:
        draw(sx,sy,cw,ch,dx,dy)
    return out

for item in meta[1:]:
    try:
        rendered=Image.open(folder/(item['name']+'.png')).convert('RGB')
        expected=reference(item['panel'])
        scale=min(rendered.width/533,rendered.height/320)
        ox=(rendered.width-533*scale)/2; oy=(rendered.height-320*scale)/2
        x0,y0,w,h=item['panel']; tx,ty,ww,hh=item['text']
        total=0; bad=[]; maximum=0
        for y in range(y0,y0+h):
            for x in range(x0,x0+w):
                if tx-2<=x+.5<=tx+ww+2 and ty-2<=y+.5<=ty+hh+2: continue
                px=int(ox+(x+.5)*scale); py=int(oy+(y+.5)*scale)
                rgba=expected.getpixel((x,y)); bg=base.getpixel((px,py)); actual=rendered.getpixel((px,py))
                want=tuple((rgba[c]*rgba[3]+bg[c]*(255-rgba[3])+127)//255 for c in range(3))
                difference=max(abs(a-b) for a,b in zip(want,actual)); maximum=max(maximum,difference); total+=1
                if difference>2 and len(bad)<12: bad.append({'xy':[x,y],'expected':want,'actual':actual})
        assert total>3000, f'Only {total} samples'
        assert not bad, json.dumps(bad,ensure_ascii=False)
        checks.append({'name':item['name'],'samples':total,'maximum_difference':maximum,'passed':True})
    except Exception as exc:
        errors.append(f'{type(exc).__name__}: {exc}')

try:
    item=meta[1]; im=Image.open(folder/(item['name']+'.png')).convert('RGB')
    scale=min(im.width/533,im.height/320);ox=(im.width-533*scale)/2;oy=(im.height-320*scale)/2
    x,y,w,h=item['text']; counts={'yellow':0,'green':0,'magenta':0}
    crop=im.crop((int(ox+x*scale),int(oy+y*scale),int(ox+(x+w)*scale),int(oy+(y+h)*scale)))
    for r,g,b in crop.getdata():
        if r>245 and g>245 and b<10: counts['yellow']+=1
        if r<10 and g>245 and b<10: counts['green']+=1
        if r>245 and g<10 and b>245: counts['magenta']+=1
    assert all(v>20 for v in counts.values()),counts
    checks.append({'name':'rendered_original_foreground_colors','pixels':counts,'passed':True})
except Exception as exc:
    errors.append(f'{type(exc).__name__}: {exc}')
report={'passed':not errors,'utc':datetime.datetime.now(datetime.timezone.utc).isoformat(),'checks':checks,'errors':errors}
report['inputs_sha256']={p.name:hashlib.sha256(p.read_bytes()).hexdigest() for p in [folder/'message-panel-geometry.json',folder/'message-panel-base.png',folder/'message-panel-short.png',folder/'message-panel-tall.png',root/'UnityProject/Assets/Resources/Original/pic/u_40.png']}
(folder/'message-panel-pixels.json').write_text(json.dumps(report,ensure_ascii=False,indent=2)+'\n')
print(json.dumps(report,ensure_ascii=False,indent=2))
raise SystemExit(0 if report['passed'] else 1)

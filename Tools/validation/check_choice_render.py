from pathlib import Path
import json, math, hashlib, datetime
from PIL import Image, ImageDraw
root=Path(__file__).resolve().parents[2]
folder=root/'Reference/Validation'
atlas=Image.open(root/'UnityProject/Assets/Resources/Original/pic/u_40.png').convert('RGBA')
base=Image.open(folder/'choice-render-base.png').convert('RGB')
meta=json.loads((folder/'choice-render.json').read_text())['data']['result']['shots']
checks=[];errors=[]
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

def box(draw,x,y,w,h,color):
    if w>0 and h>0: draw.rectangle((x,y,x+w-1,y+h-1),fill=color)

for item in meta[1:]:
    try:
        im=Image.open(folder/(item['name']+'.png')).convert('RGB')
        x,y,w,h=map(int,item['list']);fh=item['fontHeight'];ih=fh+5
        count=2 if item['name']=='choice-render-empty-info' else 5
        focus=item['focus'];first=item['first'];infoh=item['infoHeight']
        expected=Image.new('RGBA',(533,320),(0,0,0,0))
        if infoh: expected.alpha_composite(reference((166,40,200,infoh+20)))
        expected.alpha_composite(reference((x-30,y-15,180,h+30)))
        d=ImageDraw.Draw(expected);fy=y+(focus-first)*ih;purple=(131,105,159,255)
        box(d,x-5,fy,130,1,purple);box(d,x-5,fy+ih-1,130,1,purple)
        box(d,x-5,fy,1,ih,purple);box(d,x+124,fy,1,ih,purple)
        box(d,x-3,fy+2,126,ih-4,purple)
        totalh=h-2;bh=max(3,totalh//count);by=min(focus*totalh//count,totalh-bh)
        box(d,x+127,y,6,h,(210,214,205,255));box(d,x+128,y+by+1,4,bh,(87,91,101,255))
        scale=min(im.width/533,im.height/320);ox=(im.width-533*scale)/2;oy=(im.height-320*scale)/2
        samples=0;maximum=0;bad=[]
        for yy in range(320):
            for xx in range(533):
                rgba=expected.getpixel((xx,yy))
                if not rgba[3]: continue
                if infoh and 175<=xx<357 and 49<=yy<132: continue
                if x-1<=xx<x+121 and any(y+i*ih+1<=yy<y+i*ih+2+fh for i in range(min(count,3))): continue
                px=int(ox+(xx+.5)*scale);py=int(oy+(yy+.5)*scale)
                bg=base.getpixel((px,py));actual=im.getpixel((px,py))
                want=tuple((rgba[c]*rgba[3]+bg[c]*(255-rgba[3])+127)//255 for c in range(3))
                diff=max(abs(a-b) for a,b in zip(want,actual));samples+=1;maximum=max(maximum,diff)
                if diff>2 and len(bad)<12:bad.append({'xy':[xx,yy],'expected':want,'actual':actual})
        assert samples>4000,samples
        assert not bad,bad
        checks.append({'name':item['name'],'passed':True,'samples':samples,'maximum_difference':maximum})
    except Exception as e: errors.append(str(e))

try:
    im=Image.open(folder/'choice-render-empty-info.png').convert('RGB')
    scale=min(im.width/533,im.height/320);ox=(im.width-533*scale)/2;oy=(im.height-320*scale)/2
    counts={'white':0,'unselected':0,'black_shadow':0}
    for yy in range(int(oy+82*scale),int(oy+118*scale)):
        for xx in range(int(ox+206*scale),int(ox+326*scale)):
            r,g,b=im.getpixel((xx,yy))
            if min(r,g,b)>245:counts['white']+=1
            if abs(r-212)<3 and abs(g-210)<3 and abs(b-76)<3:counts['unselected']+=1
            if max(r,g,b)<5:counts['black_shadow']+=1
    assert all(n>20 for n in counts.values()),counts
    checks.append({'name':'choice_text_foregrounds_and_black_shadow','passed':True,'pixels':counts})
except Exception as e: errors.append(str(e))
report={'utc':datetime.datetime.now(datetime.timezone.utc).isoformat(),'passed':not errors,'checks':checks,'errors':errors,'scope':'Source panel/list rectangle geometry and solid colors excluding glyph rasterization; visible text foreground and black shadow pixels. Font metrics and physical input remain pending.'}
inputs=[folder/'choice-render.json',folder/'choice-render-base.png',folder/'choice-render-empty-info.png',folder/'choice-render-info.png',folder/'choice-render-last-page.png',root/'UnityProject/Assets/Resources/Original/pic/u_40.png']
report['inputs_sha256']={str(p.relative_to(root)):hashlib.sha256(p.read_bytes()).hexdigest() for p in inputs}
(folder/'choice-render-pixels.json').write_text(json.dumps(report,ensure_ascii=False,indent=2)+'\n')
print(json.dumps(report,ensure_ascii=False,indent=2))
raise SystemExit(0 if report['passed'] else 1)

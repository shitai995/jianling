from pathlib import Path
import json, hashlib, datetime, math
from PIL import Image, ImageChops
root=Path(__file__).resolve().parents[2]
folder=root/'Reference/Validation'
meta=json.loads((folder/'message-queue-capture.json').read_text())['data']['result']['shots']
anim=json.loads((root/'UnityProject/Assets/Resources/Original/Animations/action/jiantou.json').read_text())
atlas=Image.open(root/'UnityProject/Assets/Resources/Original/action/jiantou.png').convert('RGBA')
base=Image.open(folder/'message-hint-none.png').convert('RGB')
checks=[];errors=[];images={i['name']:Image.open(folder/(i['name']+'.png')).convert('RGB') for i in meta}
for item in meta[1:5]:
    try:
        frame=anim['actions'][4]['steps'][item['tick']%4]['frame']
        piece=anim['frames'][frame]['pieces'][0]
        assert piece['transform']==0 and piece['image']==0
        rect=anim['sheets'][0]['rects'][piece['rect']]
        im=images[item['name']];scale=min(im.width/533,im.height/320);ox=(im.width-533*scale)/2;oy=(im.height-320*scale)/2
        left=item['origin'][0]+piece['x'];top=item['origin'][1]+piece['y']
        maximum=0;samples=0;bad=[]
        for y in range(rect['h']):
            for x in range(rect['w']):
                vx=left+x+.5;vy=top+y+.5
                if not (0<=vx<533 and 0<=vy<320):continue
                px=int(ox+vx*scale);py=int(oy+vy*scale)
                rgba=atlas.getpixel((rect['x']+x,rect['y']+y));bg=base.getpixel((px,py));actual=im.getpixel((px,py))
                want=tuple((rgba[c]*rgba[3]+bg[c]*(255-rgba[3])+127)//255 for c in range(3))
                diff=max(abs(a-b) for a,b in zip(want,actual));maximum=max(maximum,diff);samples+=1
                if diff>2 and len(bad)<8:bad.append({'xy':[x,y],'expected':want,'actual':actual})
        assert not bad,bad
        bbox=ImageChops.difference(im,base).getbbox();assert bbox is not None
        allowed=(math.floor(ox+(left-1)*scale),math.floor(oy+(top-1)*scale),math.ceil(ox+min(533,left+rect['w']+1)*scale),math.ceil(oy+(top+rect['h']+1)*scale))
        assert bbox[0]>=allowed[0] and bbox[1]>=allowed[1] and bbox[2]<=allowed[2] and bbox[3]<=allowed[3],(bbox,allowed)
        checks.append({'name':item['name'],'passed':True,'source_frame':frame,'samples':samples,'maximum_difference':maximum,'change_bounds':bbox})
    except Exception as e:errors.append(str(e))
try:
    assert ImageChops.difference(images['message-hint-0'],images['message-hint-1']).getbbox() is None
    assert ImageChops.difference(images['message-hint-2'],images['message-hint-3']).getbbox() is None
    assert ImageChops.difference(images['message-hint-0'],images['message-hint-2']).getbbox() is not None
    checks.append({'name':'source_two_frames_held_two_cycles_each','passed':True})
    assert ImageChops.difference(images['message-hint-return'],base).getbbox() is None
    checks.append({'name':'return_message_has_no_hint','passed':True})
    assert ImageChops.difference(images['message-hint-fullscreen-0'],images['message-hint-fullscreen-2']).getbbox() is None
    checks.append({'name':'fullscreen_text_has_no_hint','passed':True})
except Exception as e:errors.append(str(e))
report={'utc':datetime.datetime.now(datetime.timezone.utc).isoformat(),'passed':not errors,'checks':checks,'errors':errors,'scope':'Original jiantou action4 atlas pixels, four80ms phases, source-relative location under current Unity font metrics, no hint for timed/return/fullscreen text; original-device font and frame recording remain pending.'}
inputs=[folder/'message-queue-capture.json',root/'UnityProject/Assets/Resources/Original/Animations/action/jiantou.json',root/'UnityProject/Assets/Resources/Original/action/jiantou.png']+[folder/(i['name']+'.png') for i in meta]
report['inputs_sha256']={str(p.relative_to(root)):hashlib.sha256(p.read_bytes()).hexdigest() for p in inputs}
(folder/'message-hint-pixels.json').write_text(json.dumps(report,ensure_ascii=False,indent=2)+'\n')
print(json.dumps(report,ensure_ascii=False,indent=2));raise SystemExit(0 if report['passed'] else 1)

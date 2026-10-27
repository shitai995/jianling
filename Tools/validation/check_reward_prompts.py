from pathlib import Path
import json, hashlib, datetime
from PIL import Image
root=Path(__file__).resolve().parents[2]; folder=root/'Reference/Validation'
checks=[]; errors=[]; crops=[]
expected={'money':['magenta'],'item':['magenta','white'],'equipment':['green'],'xp':['green']}
for name,colors in expected.items():
    path=folder/('reward-message-'+name+'.png')
    im=Image.open(path).convert('RGB'); scale=min(im.width/533,im.height/320)
    ox=(im.width-533*scale)/2; oy=(im.height-320*scale)/2
    crop=im.crop((int(ox+80*scale),int(oy+138*scale),int(ox+450*scale),int(oy+159*scale)))
    counts={'magenta':0,'green':0,'white':0}
    for r,g,b in crop.getdata():
        if r>245 and g<10 and b>245:counts['magenta']+=1
        if r<10 and g>245 and b<10:counts['green']+=1
        if min(r,g,b)>245:counts['white']+=1
    try:
        assert all(counts[c]>20 for c in colors),counts
        checks.append({'name':name,'passed':True,'expected':colors,'pixels':counts})
    except Exception as e:errors.append(str(e))
    crops.append(im.crop((int(ox),int(oy+125*scale),int(ox+533*scale),int(oy+175*scale))))
contact=Image.new('RGB',(crops[0].width,sum(c.height for c in crops)))
y=0
for crop in crops:contact.paste(crop,(0,y));y+=crop.height
contact.save(folder/'reward-messages-contact.png')
paths=[folder/('reward-message-'+name+'.png') for name in expected]
report={'utc':datetime.datetime.now(datetime.timezone.utc).isoformat(),'passed':not errors,'checks':checks,'errors':errors,'scope':'Actual GameView foreground-color pixels in four script reward messages; source text and VM behavior checked separately. Font outlines and original-device glyph matching remain pending.','sha256':{str(p.relative_to(root)):hashlib.sha256(p.read_bytes()).hexdigest() for p in paths}}
(folder/'reward-message-pixels.json').write_text(json.dumps(report,ensure_ascii=False,indent=2)+'\n')
print(json.dumps(report,ensure_ascii=False,indent=2));raise SystemExit(0 if report['passed'] else 1)

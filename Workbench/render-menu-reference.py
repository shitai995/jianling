import json
from pathlib import Path
from PIL import Image,ImageDraw
base=Path('UnityProject/Assets/Resources/Original'); d=json.loads((base/'Animations/action/u_01.json').read_text()); sheets=[Image.open(base/('action/'+n+'.png')).convert('RGBA') for n in ['u_04','u_02','u_06','u_11','s_34','f_07','u_00','u_46']]
ops={1:Image.Transpose.FLIP_TOP_BOTTOM,2:Image.Transpose.FLIP_LEFT_RIGHT,3:Image.Transpose.ROTATE_180,4:Image.Transpose.TRANSPOSE,5:Image.Transpose.ROTATE_270,6:Image.Transpose.ROTATE_90,7:Image.Transpose.TRANSVERSE}
preview=Image.new('RGBA',(960,560),(45,45,45,255)); draw=ImageDraw.Draw(preview)
for idx,a in enumerate([1,2,3,4,5,13]):
 im=Image.new('RGBA',(320,240),(0,0,0,255)); f=d['frames'][d['actions'][a]['steps'][0]['frame']]
 for p in f['pieces']:
  if p['image']<0:
   col=p['color']&0xffffff; ImageDraw.Draw(im).rectangle((160+p['x'],120+p['y'],160+p['x']+p['w']-1,120+p['y']+p['h']-1),fill=((col>>16)&255,(col>>8)&255,col&255));continue
  r=d['sheets'][p['image']]['rects'][p['rect']]; piece=sheets[p['image']].crop((r['x'],r['y'],r['x']+r['w'],r['y']+r['h']))
  if p['transform']:piece=piece.transpose(ops[p['transform']])
  im.alpha_composite(piece,(160+p['x'],120+p['y']))
 x=idx%3*320;y=idx//3*280;preview.alpha_composite(im,(x,y));draw.text((x+3,y+245),str(a),fill='white')
preview.save('Workbench/menu-reference.png')

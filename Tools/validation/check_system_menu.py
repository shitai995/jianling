"""Check system-menu focus, help clipping and original settings arrows in GameView."""
import json
from pathlib import Path
from PIL import Image, ImageChops, ImageDraw

ROOT = Path(__file__).resolve().parents[2]
OUT = ROOT / 'Reference/Validation'

def point(im, x, y):
    scale = min(im.width / 533, im.height / 320)
    return (int((im.width - 533 * scale) / 2 + (66.5 + x * 1.25) * scale),
            int((im.height - 320 * scale) / 2 + (10 + y * 1.25) * scale))

def clipped_delta(a, b, areas):
    delta = ImageChops.difference(a, b)
    count = sum(any(p) for p in delta.get_flattened_data())
    outside = delta.copy()
    for x, y, w, h in areas:
        lo, hi = point(a, x, y), point(a, x+w, y+h)
        ImageDraw.Draw(outside).rectangle((lo[0]-2, lo[1]-2, hi[0]+2, hi[1]+2), fill=(0, 0, 0))
    assert count > 100, 'Expected visible change'
    assert outside.getbbox() is None, f'Changed pixels outside source bounds: {outside.getbbox()}'
    return count

def main():
    checks = []
    load = lambda name: Image.open(OUT / f'system-{name}.png').convert('RGB')
    source = json.loads((ROOT / 'UnityProject/Assets/Resources/Original/Animations/action/u_01.json').read_text())
    first, last = (source['frames'][n]['pieces'] for n in (28,33))
    areas = []
    for a,b in ((first,last),(last,first)):
        for piece in a:
            if piece in b:
                continue
            rect = piece if piece['image'] < 0 else source['sheets'][piece['image']]['rects'][piece['rect']]
            w,h = rect['w'],rect['h']
            if piece['transform'] >= 4:
                w,h = h,w
            areas.append((160+piece['x'],120+piece['y'],w,h))
    count = clipped_delta(load('root-0'), load('root-5'), areas)
    checks.append({'name': 'Root focus0 to5 changes only source selected rows', 'changedPixels': count})
    count = clipped_delta(load('help-start'), load('help-visible'), [(20,46,281,148)])
    checks.append({'name': 'Help text visibly scrolls only in original viewport', 'changedPixels': count})
    actual = load('settings-recovery')
    arrow = Image.open(ROOT / 'UnityProject/Assets/Resources/Original/pic/u_44.png').convert('RGBA')
    errors = []
    for y in range(arrow.height):
        for x in range(arrow.width):
            p = arrow.getpixel((x,y))
            if p[3] != 255:
                continue
            a = actual.getpixel(point(actual, 218+x+.5, 168+y+.5))
            errors.append(max(abs(u-v) for u,v in zip(a,p[:3])))
    assert errors and max(errors) <= 1, f'Saved settings arrow mismatch: {max(errors) if errors else "no samples"}'
    checks.append({'name': 'Settings uses saved original u44 arrow at source position', 'samples': len(errors), 'maxRGBDifference': max(errors)})
    report = {'passed': True, 'checks': checks}
    (OUT / 'system-menu-pixels.json').write_text(json.dumps(report, ensure_ascii=False, indent=2)+'\n')
    print(json.dumps(report, ensure_ascii=False))

if __name__ == '__main__':
    main()

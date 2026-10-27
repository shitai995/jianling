"""Recompute original map layers and compare them with the editable Unity data."""
from pathlib import Path
from datetime import datetime, timezone
import sys, json
from PIL import Image, ImageChops
sys.path.insert(0, str(Path(__file__).resolve().parents[1]))
from import_original import read_map, render_map, DECODED, OUTPUT, ROOT
checks, counts, error = [], [0, 0, 0], ''
try:
    files = sorted((DECODED / 'map').glob('*.m'))
    assert len(files) == 52
    for p in files:
        data = read_map(p.read_bytes())
        saved = json.loads((OUTPUT / 'Maps' / f'{p.stem}.json').read_text())
        saved.pop('id')
        assert data == saved, p.name
        for i, layer in enumerate(render_map(data)):
            actual = Image.open(OUTPUT / 'Maps' / f'{p.stem}_{i}.png').convert('RGBA')
            assert layer.size == actual.size and layer.tobytes() == actual.tobytes(), f'{p.name} layer{i}'
            counts[i] += bool(layer.getchannel('A').getbbox())
    checks.append('All 52 original map binaries exactly match saved collision/tile/layer JSON and all 156 saved layer images')
    assert counts[1:] == [15, 0], counts
    checks.append('Source binaries contain fifteen visible layer1 maps and no layer2 flags/parts; all transparent layer2 PNGs match the original data')
except Exception as exc:
    error = str(exc)
report = dict(passed=not error, utc=datetime.now(timezone.utc).isoformat(), checks=checks, visibleLayers=counts, error=error)
(ROOT / 'Reference/Validation/world-map-source.json').write_text(json.dumps(report, indent=2) + '\n')
print(json.dumps(report))
if error:
    raise SystemExit(1)

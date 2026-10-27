from pathlib import Path
import datetime
import hashlib
import json
import numpy as np
from PIL import Image

root = Path(__file__).resolve().parents[2]
folder = root / 'Reference/Validation'
checks = []
paths = []
for name in ('outline', 'shadow'):
    actual_path = folder / ('text-style-' + name + '.png')
    reference_path = folder / ('text-style-reference-' + name + '.png')
    actual = np.asarray(Image.open(actual_path).convert('RGB')).astype(np.int16)
    reference = np.asarray(Image.open(reference_path).convert('RGB')).astype(np.int16)
    assert actual.shape == reference.shape, name + ': capture size mismatch'
    difference = np.abs(actual - reference)
    changed = int(np.any(difference != 0, axis=2).sum())
    checks.append(dict(name=name, passed=changed == 0,
                       compared_pixels=int(actual.shape[0] * actual.shape[1]),
                       nonblack_pixels=int(np.any(actual != 0, axis=2).sum()),
                       differing_pixels=changed, max_rgb_difference=int(difference.max())))
    paths.extend((actual_path, reference_path))

report = dict(utc=datetime.datetime.now(datetime.timezone.utc).isoformat(),
              passed=all(c['passed'] for c in checks), checks=checks,
              scope='Actual message rendering versus an editor-only reference using literal Tool.drawString offsets, without the production markup parser or StyledLabel. Same Unity font, layout and rasterizer; original-device font equivalence and multiline span overlap order remain unverified.',
              sha256={str(p.relative_to(root)): hashlib.sha256(p.read_bytes()).hexdigest() for p in paths})
(folder / 'text-style-pixels.json').write_text(json.dumps(report, ensure_ascii=False, indent=2) + '\n')
print(json.dumps(report, ensure_ascii=False, indent=2))
raise SystemExit(0 if report['passed'] else 1)

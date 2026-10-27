#!/usr/bin/env python3
"""Summarize original script opcode usage without executing game or APK code."""
import collections
import json
import pathlib
import re

ROOT = pathlib.Path(__file__).resolve().parents[1]
script_root = ROOT / "UnityProject/Assets/Resources/Original/Scripts"
usage = collections.defaultdict(lambda: {"count": 0, "scripts": []})
for path in sorted(script_root.glob("*.json")):
    counts = collections.Counter()
    for record in json.loads(path.read_text())["instructions"]:
        # Text in dialogue strings is data, not an instruction.
        code = re.sub(r'"[^"\\]*(?:\\.[^"\\]*)*"', '', record)
        counts.update(int(v, 16) for v in re.findall(r"\bx([0-9a-fA-F]{2})\s*\(", code))
    for opcode, count in counts.items():
        usage[opcode]["count"] += count
        usage[opcode]["scripts"].append(path.stem)
source = "\n".join(path.read_text() for path in (ROOT / "UnityProject/Assets/Scripts").glob("*.cs"))
handled = {int(value, 16) for value in re.findall(r"case 0x([0-9A-Fa-f]+):", source)}
# A dispatch branch alone does not imply full behavioral fidelity.
rows = [{"opcode": f"x{opcode:02X}", **value, "dispatch_branch": opcode <= 10 or opcode in handled}
        for opcode, value in sorted(usage.items())]
output = ROOT / "Reference/script-opcodes.json"
output.write_text(json.dumps({"scope": "768 primary scripts; dispatch presence does not verify implementation completeness", "rows": rows}, ensure_ascii=False, indent=2))
missing = [row["opcode"] for row in rows if not row["dispatch_branch"]]
print(json.dumps({"opcodes": len(rows), "missing_dispatch": missing}, ensure_ascii=False))

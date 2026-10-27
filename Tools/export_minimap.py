"""Recover only static minimap tables; Unity saves the result in an editable asset."""
import json
import re
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
source = (ROOT / "Reference/Decompiled/sources/com/dobetter/client/GameCanvas.java").read_text()
constants = {"STATE_CG": -1, "WebNetEvent.GotoWeb_Event_OK": 1000, "WebNetEvent.GotoWeb_Event_Error": 1001}
values = dict(constants)


def expression(text):
    text = re.sub(r"new (?:int|byte)(?:\[\])+", "", text.strip())
    sized = re.fullmatch(r"new (?:int|byte)\[(\d+)\]", text)
    if sized:
        return [0] * int(sized[1])
    if text.startswith("{"):
        parts, start, depth = [], 1, 0
        for i, char in enumerate(text[1:-1], 1):
            depth += char == "{"
            depth -= char == "}"
            if char == "," and depth == 0:
                parts.append(expression(text[start:i]))
                start = i + 1
        if text[start:-1].strip():
            parts.append(expression(text[start:-1]))
        return parts
    return values[text] if text in values else int(text)


stages = expression(re.search(r"this.stageData = (.*?);", source)[1])
tables = source[source.index("byte[] bArr = new byte[4];"):source.index('this.mainMissionTitle = "";')]
for statement in tables.split(";"):
    statement = statement.strip()
    declaration = re.fullmatch(r"byte(?:\[\])+ (\w+) = (.*)", statement)
    assignment = re.fullmatch(r"(\w+)\[(\d+)\] = (.*)", statement)
    table = re.fullmatch(r"this\.(bStageExit|bSaveStage) = (.*)", statement)
    if declaration:
        values[declaration[1]] = expression(declaration[2])
    elif assignment:
        values[assignment[1]][int(assignment[2])] = expression(assignment[3])
    elif table:
        values[table[1]] = expression(table[2])
    elif statement:
        raise ValueError(statement)

sizes = [(98, 30), (98, 30), (257, 106), (257, 106), (257, 106), (257, 106),
         (220, 204), (45, 30), (257, 144), (363, 258), (257, 30), (363, 220), (257, 182)]
assert len(stages) == len(values["bStageExit"]) == len(values["bSaveStage"]) == len(sizes) == 13
areas = []
for index, (rows, exits, saves, size) in enumerate(zip(stages, values["bStageExit"], values["bSaveStage"], sizes)):
    assert len(rows) == len(exits), index
    # Original area 7 allocates 20 save flags although it draws only one room.
    assert len(saves) >= len(rows), index
    areas.append({"width": size[0], "height": size[1], "rooms": [
        {"stage": row[0], "x": row[1], "y": row[2], "exits": exit_row, "save": saves[i] == 1}
        for i, (row, exit_row) in enumerate(zip(rows, exits))]})
target = ROOT / "Reference/minimap.json"
target.write_text(json.dumps({"areas": areas}, ensure_ascii=False, indent=2) + "\n")
print(f"Recovered {len(areas)} areas / {sum(len(a['rooms']) for a in areas)} rooms to {target}")

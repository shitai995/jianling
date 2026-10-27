"""Convert original image, map, animation and data formats into Unity assets.

Schemas follow the recovered Tool, GameMap, ActionSet and GameCanvas classes.
Every supported binary format is checked for complete consumption.
"""
from pathlib import Path
import io
import json
import re
import shutil
import struct
from PIL import Image
from recover_assets import ROOT, write_json

SOURCE = ROOT / "Reference/Extracted/assets"
DECODED = ROOT / "Reference/Decoded"
OUTPUT = ROOT / "UnityProject/Assets/Resources/Original"


class Reader:
    def __init__(self, data):
        self.data, self.pos = data, 0

    def take(self, fmt):
        fmt = ">" + fmt
        result = struct.unpack_from(fmt, self.data, self.pos)
        self.pos += struct.calcsize(fmt)
        return result[0] if len(result) == 1 else list(result)

    def utf(self):
        size = self.take("H")
        value = self.data[self.pos:self.pos + size].decode("utf-8")
        self.pos += size
        return value

    def end(self):
        if self.pos != len(self.data):
            raise ValueError(f"Unread bytes: {len(self.data) - self.pos}")


def decode_image(data):
    length = struct.unpack_from(">I", data)[0]
    if length != len(data) - 4:
        raise ValueError("Image length mismatch")
    body = bytearray(length)
    offset, dest = 4, length
    remainder = length % 100
    if remainder:
        dest -= remainder
        body[dest:dest + remainder] = data[offset:offset + remainder]
        offset += remainder
    while dest:
        dest -= 100
        body[dest:dest + 100] = data[offset:offset + 100]
        offset += 100
    png = b"\x89PNG\r\n\x1a\n\x00\x00\x00\rIHDR" + body + bytes.fromhex("0000000049454e44ae426082")
    with Image.open(io.BytesIO(png)) as image:
        image.verify()
    return png


def read_map(data):
    r = Reader(data)
    result = {"type": r.take("B"), "images": []}
    for _ in range(r.take("B")):
        result["images"].append("map/" + Path(r.utf()).stem)
    for key in ["cellWidth", "cellHeight", "columns", "rows"]:
        result[key] = r.take("h")
    result["backgroundColor"] = r.take("i")
    cells = []
    for x in range(result["columns"]):
        for y in range(result["rows"]):
            if result["type"] == 0:
                a, b, t0, t1, t2, flags, flags2, land = r.take("8B")
                cells.append({"x": x, "y": y, "blocked": bool(flags & 8), "land": land,
                              "tiles": [t0 - 1, t1 - 1, t2 - 1],
                              "images": [a & 15, a >> 4, b],
                              "transforms": [flags & 7, (flags >> 4) & 7, (flags2 & 7) >> 1],
                              "above": [False, bool(flags & 128), (flags2 >> 3) == 1]})
            else:
                flags, land = r.take("2B")
                cells.append({"x": x, "y": y, "blocked": bool(flags & 1), "land": land})
    result["cells"] = cells
    result["parts"] = []
    if result["type"] == 1:
        for layer in range(3):
            for _ in range(r.take("B")):
                flags, x, y = r.take("Bhh")
                result["parts"].append({"layer": layer, "image": flags >> 4,
                                        "transform": flags & 15, "x": x, "y": y})
    r.end()
    return result


def read_animation(data, legacy=False):
    r = Reader(data)
    result = {"sheets": [], "frames": [], "actions": []}
    for _ in range(r.take("B")):
        result["sheets"].append({"rects": [dict(zip(["x", "y", "w", "h"], r.take("hhBB")))
                                           for _ in range(r.take("h"))]})
    for _ in range(r.take("h")):
        frame = {"pieces": []}
        for _ in range(r.take("B")):
            piece = dict(zip(["image", "rect", "transform", "x", "y"], r.take("bhbhh")))
            if piece["image"] < 0:
                piece.update(zip(["w", "h", "color"], r.take("hhi")))
            frame["pieces"].append(piece)
        for key in ["attackRects", "bodyRects"]:
            frame[key] = [dict(zip(["x", "y", "w", "h"], r.take("4h")))
                          for _ in range(r.take("i"))] if r.take("?") else []
        frame["edgeRect"] = dict(zip(["x", "y", "w", "h"], r.take("4h")))
        result["frames"].append(frame)
    result["footWidth"], result["footHeight"] = r.take("2B")
    for _ in range(r.take("h")):
        result["actions"].append({"steps": [dict(zip(
            ["frame", "vx", "vy", "attack", "vulnerable", "attackType"],
            r.take("hhh??") + [0] if legacy else r.take("hhh??b")))
            for _ in range(r.take("h"))]})
    r.end()
    result["legacy"] = legacy
    return result


def read_table(name):
    r = Reader((DECODED / name).read_bytes())
    rows = []
    count = r.take("i")
    for _ in range(count):
        if name == "action/sprite.tbd":
            row = {"id": r.take("h"), "name": r.utf(), "animation": r.utf(),
                   "imageList": r.utf(), "type": r.take("b")}
            row["images"] = ["action/" + p.split(",")[0] for p in row["imageList"].split(";")]
        elif name == "equip.tbd":
            row = {"id": r.take("b"), "name": r.utf(), "grade": r.take("b"),
                   "description": r.utf(), "image": r.utf(), "level": r.take("b"), "skill": r.take("b")}
            row.update(zip(["durability", "hp", "attack", "defense", "hit", "dodge", "critical"], r.take("7h")))
            row.update(zip(["buy", "sell"], r.take("2i")))
            row.update(effects=r.utf(), icon=r.utf())
        elif name == "item.tbd":
            row = {"id": r.take("b"), "name": r.utf(), "description": r.utf()}
            row.update(zip(["buy", "sell", "hp", "durability"], r.take("iihh")))
            row.update(effect=r.utf(), type=r.take("b"))
        elif name == "skill.tbd":
            row = {"id": r.take("b"), "name": r.utf(), "description": r.utf(), "rank": r.take("b"),
                   "damage": r.utf(), "amend": r.utf(), "cost": r.utf(),
                   "cooldown": r.take("h"), "holdDamage": r.take("h")}
        elif name == "enemy.tbd":
            row = dict(zip(["id", "hold", "hpCoefficient", "attackCoefficient", "hit", "dodge", "vision"],
                           r.take("hhhhbbh")))
            for key in ["attackRanges", "damageCoefficients", "holdDamage", "attackIntervals"]:
                row[key] = r.utf()
            row.update(experience=r.take("h"), money=r.take("h"), equipmentDrops=r.utf(), itemDrops=r.utf())
        elif name in ("role.tbd", "ego.tbd", "enemybase.tbd"):
            fields = {"role.tbd": ["hp", "attack", "defense", "hit", "dodge", "critical"],
                      "ego.tbd": ["levels", "durability", "hp", "attack", "defense", "hit", "dodge", "critical"],
                      "enemybase.tbd": ["hp", "attack", "defense", "hit", "dodge"]}[name]
            row = {key: r.utf() for key in fields}
            if name == "enemybase.tbd":
                row["injuryCoefficient"] = r.take("b")
        else:
            raise ValueError(f"Unsupported table: {name}")
        rows.append(row)
    r.end()
    return rows


def transform(image, mode):
    operations = {1: Image.Transpose.FLIP_TOP_BOTTOM, 2: Image.Transpose.FLIP_LEFT_RIGHT,
                  3: Image.Transpose.ROTATE_180, 4: Image.Transpose.TRANSPOSE,
                  5: Image.Transpose.ROTATE_270, 6: Image.Transpose.ROTATE_90,
                  7: Image.Transpose.TRANSVERSE}
    return image.transpose(operations[mode]) if mode else image


def render_map(data):
    w, h = data["columns"] * data["cellWidth"], data["rows"] * data["cellHeight"]
    layers = [Image.new("RGBA", (w, h)) for _ in range(3)]
    sheets = [Image.open(OUTPUT / (p + ".png")).convert("RGBA") for p in data["images"]]
    if data["type"] == 1:
        for part in data["parts"]:
            image = transform(sheets[part["image"]], part["transform"])
            layers[part["layer"]].alpha_composite(image, (part["x"], part["y"]))
    else:
        tw, th = data["cellWidth"], data["cellHeight"]
        for cell in data["cells"]:
            for index, tile in enumerate(cell["tiles"]):
                if tile < 0:
                    continue
                sheet = sheets[cell["images"][index]]
                cols = sheet.width // tw
                sx, sy = (tile % cols) * tw, (tile // cols) * th
                if sx + tw > sheet.width or sy + th > sheet.height:
                    raise ValueError(f"Tile outside image: {tile}")
                image = transform(sheet.crop((sx, sy, sx + tw, sy + th)), cell["transforms"][index])
                layer = index if cell["above"][index] else 0
                layers[layer].alpha_composite(image, (cell["x"] * tw, cell["y"] * th))
    return layers


def main():
    summary = {"images": [], "maps": [], "animations": [], "tables": {}, "warnings": []}
    for source in sorted(SOURCE.rglob("*.p")):
        relative = source.relative_to(SOURCE).with_suffix(".png")
        target = OUTPUT / relative
        target.parent.mkdir(parents=True, exist_ok=True)
        target.write_bytes(decode_image(source.read_bytes()))
        with Image.open(target) as image:
            summary["images"].append({"path": relative.as_posix(), "width": image.width, "height": image.height})
    for source in sorted((DECODED / "map").glob("*.m")):
        data = read_map(source.read_bytes())
        data["id"] = source.stem
        write_json(OUTPUT / "Maps" / (source.stem + ".json"), data)
        layers = render_map(data)
        for index, layer in enumerate(layers):
            layer.save(OUTPUT / "Maps" / f"{source.stem}_{index}.png")
        summary["maps"].append({"id": source.stem, "width": layers[0].width,
                                "height": layers[0].height, "type": data["type"]})
    for folder in ["action", "key"]:
        for source in sorted((DECODED / folder).glob("*.n")):
            try:
                data = read_animation(source.read_bytes())
            except (struct.error, ValueError):
                try:
                    data = read_animation(source.read_bytes(), legacy=True)
                    summary["warnings"].append({"path": str(source.relative_to(DECODED)), "note": "Legacy animation records have no attackType field"})
                except (struct.error, ValueError) as error:
                    summary["warnings"].append({"path": str(source.relative_to(DECODED)), "error": str(error)})
                    continue
            write_json(OUTPUT / "Animations" / folder / (source.stem + ".json"), data)
            summary["animations"].append({"id": f"{folder}/{source.stem}", "frames": len(data["frames"]),
                                          "actions": len(data["actions"])})
    for name in ["action/sprite.tbd", "equip.tbd", "item.tbd", "skill.tbd", "role.tbd", "ego.tbd", "enemy.tbd", "enemybase.tbd"]:
        rows = read_table(name)
        write_json(OUTPUT / "Data" / (Path(name).stem + ".json"), {"rows": rows})
        summary["tables"][name] = len(rows)
    stages = []
    for script in sorted((OUTPUT / "script").glob("*.txt")):
        text = script.read_text(encoding="utf-8")
        name, map_id = re.search(r'x21\("([^"]+)"\)', text), re.search(r'x23\("/([^".]+)\.m"\)', text)
        if name and map_id:
            stages.append({"script": script.stem, "name": name[1], "map": map_id[1]})
    write_json(OUTPUT / "Data/stages.json", {"rows": stages})
    for source in (ROOT / "Reference/Extracted/res/raw").iterdir():
        target = OUTPUT / "Audio" / source.name
        target.parent.mkdir(parents=True, exist_ok=True)
        if source.suffix == ".mid":
            target = target.with_suffix(".mid.bytes")
        shutil.copyfile(source, target)
    write_json(ROOT / "Reference/recovery-summary.json", summary)
    print(json.dumps({key: len(value) if isinstance(value, list) else value for key, value in summary.items()}, ensure_ascii=False))


if __name__ == "__main__":
    main()

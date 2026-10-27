"""Reproducible static recovery of the original APK. Never executes APK code."""
from pathlib import Path
import argparse
import gzip
import hashlib
import json
import struct
import zipfile

ROOT = Path(__file__).resolve().parents[1]


def read_utf_records(data):
    records, pos = [], 0
    while pos < len(data):
        if pos + 2 > len(data):
            raise ValueError(f"Truncated UTF length at {pos}")
        length = struct.unpack_from(">H", data, pos)[0]
        pos += 2
        if pos + length > len(data):
            raise ValueError(f"Truncated UTF record at {pos}")
        records.append(data[pos:pos + length].decode("utf-8"))
        pos += length
    return records


def write_json(path, value):
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(value, ensure_ascii=False, indent=2), encoding="utf-8")


def recover(apk):
    reference = ROOT / "Reference"
    output = ROOT / "UnityProject/Assets/Resources/Original"
    inventory, scripts, warnings = [], [], []
    with zipfile.ZipFile(apk) as archive:
        for entry in archive.infolist():
            if entry.is_dir():
                continue
            relative = Path(entry.filename)
            if relative.is_absolute() or ".." in relative.parts:
                raise ValueError(f"Unsafe archive entry: {entry.filename}")
            data = archive.read(entry)
            target = reference / "Extracted" / relative
            target.parent.mkdir(parents=True, exist_ok=True)
            target.write_bytes(data)
            inventory.append({"path": entry.filename, "bytes": len(data),
                              "sha256": hashlib.sha256(data).hexdigest()})
            if not entry.filename.startswith("assets/"):
                continue
            relative = Path(*relative.parts[1:])
            try:
                decoded = gzip.decompress(data) if data.startswith(b"\x1f\x8b") else data
            except (EOFError, gzip.BadGzipFile) as error:
                warnings.append({"path": entry.filename, "error": str(error)})
                continue
            if entry.filename.endswith(".s"):
                lines = read_utf_records(decoded)
                path = output / relative.with_suffix(".txt")
                path.parent.mkdir(parents=True, exist_ok=True)
                path.write_text("\n".join(lines) + "\n", encoding="utf-8")
                write_json(output / "Scripts" / relative.relative_to("script").with_suffix(".json"),
                           {"instructions": lines})
                scripts.append({"path": relative.as_posix(), "instructions": len(lines)})
            elif entry.filename.endswith((".m", ".n", ".tbd")):
                path = reference / "Decoded" / relative
                path.parent.mkdir(parents=True, exist_ok=True)
                path.write_bytes(decoded)
    write_json(reference / "inventory.json", {
        "source": str(apk), "sourceSha256": hashlib.sha256(apk.read_bytes()).hexdigest(),
        "files": inventory, "scripts": scripts, "warnings": warnings})
    print(f"Recovered {len(inventory)} files, decoded {len(scripts)} scripts, "
          f"{sum(s['instructions'] for s in scripts)} instructions, {len(warnings)} warnings")


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("apk", type=Path)
    recover(parser.parse_args().apk)

"""Extrae textos Unity sin modificar assets; valida el catálogo chino y la cobertura de glifos."""
import argparse
from bisect import bisect_right
from collections import Counter
import hashlib
import json
from pathlib import Path
import re
import struct
import unicodedata

ROOT = Path(__file__).resolve().parents[2]
OUT = ROOT / "outputs/localization"
CATALOG = ROOT / "Assets/Resources/Localization/zh-Hans.json"
TEXT_FIELDS = {"m_text", "m_Text", "textoEspanol", "textoEs", "es"}
TAGS = re.compile(r"</?[A-Za-z][^>]*>|<#[0-9A-Fa-f]+>")
PLACEHOLDERS = re.compile(r"(?<!\{)\{\d+(?:,[^}:]+)?(?::[^}]+)?\}(?!\})")


def balanced_tags(text):
    paired = {"b", "i", "u", "s", "color", "size", "font", "font-weight", "mark", "link", "align",
        "cspace", "mspace", "voffset", "indent", "line-indent", "margin", "noparse", "nobr", "style"}
    stack = []
    for token in TAGS.findall(text):
        match = re.match(r"<(/?)([A-Za-z][A-Za-z-]*)", token)
        if not match or match[2].lower() not in paired:
            continue
        name = match[2].lower()
        if match[1]:
            if not stack or stack.pop() != name:
                return False
        else:
            stack.append(name)
    return not stack


def write_json(name, value):
    OUT.mkdir(parents=True, exist_ok=True)
    (OUT / name).write_text(json.dumps(value, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")


def extract_assets():
    import yaml  # PyYAML, disponible en el entorno local del proyecto.
    loader = getattr(yaml, "CSafeLoader", yaml.SafeLoader)
    entries, failures, tutorial_steps, binary_files = [], [], [], []
    paths = sorted(p for p in (ROOT / "Assets").rglob("*") if p.suffix in {".unity", ".prefab", ".asset"})
    for path in paths:
        raw = path.read_bytes()
        if not raw.startswith((b"%YAML", b"\xef\xbb\xbf%YAML")):
            binary_files.append(path.relative_to(ROOT).as_posix())
            continue
        text = raw.decode("utf-8-sig")
        if not re.search(r"^\s+(?:m_text|m_Text|textoEspanol|textoEs|es|propertyPath):", text, re.M):
            continue
        newlines = [m.start() for m in re.finditer("\n", text)]
        documents = list(re.finditer(r"^--- !u!\d+ &(-?\d+)[^\n]*\n", text, re.M))
        for index, match in enumerate(documents):
            end = documents[index + 1].start() if index + 1 < len(documents) else len(text)
            body = text[match.end():end]
            if not re.search(r"^\s+(?:m_text|m_Text|textoEspanol|textoEs|es|propertyPath):", body, re.M):
                continue
            try:
                data = yaml.load(body, Loader=loader)
            except yaml.YAMLError as error:
                failures.append({"path": path.relative_to(ROOT).as_posix(), "fileID": match[1], "error": str(error)})
                continue

            def visit(node):
                if isinstance(node, list):
                    for item in node:
                        visit(item)
                elif isinstance(node, dict):
                    for field, value in node.items():
                        localized = (field == "es" and node.get("key")) or (field == "textoEs" and node.get("id"))
                        if field in TEXT_FIELDS and (localized or isinstance(value, str) and value.strip()):
                            value = value if isinstance(value, str) else ""
                            key = node.get("key") if field == "es" else ("tooltip." + str(node["id"]) if field == "textoEs" and "id" in node else value)
                            entries.append({"key": key or value, "es": value,
                                "en": node.get("en", node.get("textoEn", node.get("textoIngles", ""))) or "",
                                "pt": node.get("pt", node.get("textoPt", node.get("textoPortugues", ""))) or "",
                                "path": path.relative_to(ROOT).as_posix(), "fileID": match[1],
                                "line": bisect_right(newlines, match.start()) + 1, "field": field,
                                "scriptGuid": data.get("MonoBehaviour", {}).get("m_Script", {}).get("guid", "")})
                        visit(value)
                    if node.get("propertyPath") in TEXT_FIELDS and isinstance(node.get("value"), str) and node["value"].strip():
                        entries.append({"key": node["value"], "es": node["value"], "en": "", "pt": "",
                            "path": path.relative_to(ROOT).as_posix(), "fileID": match[1],
                            "line": bisect_right(newlines, match.start()) + 1, "field": "prefab override"})
            visit(data)
    # Guarda la definición completa para detectar cambios accidentales de secuencia/condiciones/voz.
    definition = ROOT / "Assets/Resources/Tutoriales/TutorialVerticalSlice.asset"
    definition_text = re.sub(r"^%[^\n]*\n|^--- !u![^\n]*\n", "", definition.read_text(encoding="utf-8-sig"), flags=re.M)
    tutorial_steps = yaml.load(definition_text, Loader=loader)["MonoBehaviour"]["steps"]
    write_json("asset-inventory.json", {"entries": entries, "parseErrors": failures, "binaryFilesSkipped": binary_files})
    write_json("tutorial-baseline.json", {"sha256": hashlib.sha256(definition.read_bytes()).hexdigest(), "steps": tutorial_steps})
    print(json.dumps({"serializedTexts": len(entries), "serializedFiles": len({e['path'] for e in entries}),
        "tutorialSteps": len(tutorial_steps), "assetParseErrors": len(failures)}))
    if failures:
        raise SystemExit(1)


def font_has_char(path):
    """Consulta cmap 4/12 de fuentes TTF/OTF sin depender de fuentes instaladas en Windows."""
    data = path.read_bytes()
    u16 = lambda offset: struct.unpack_from(">H", data, offset)[0]
    u32 = lambda offset: struct.unpack_from(">I", data, offset)[0]
    tables = {data[12 + i * 16:16 + i * 16]: u32(20 + i * 16) for i in range(u16(4))}
    cmap = tables[b"cmap"]
    subtables = []
    for i in range(u16(cmap + 2)):
        record = cmap + 4 + 8 * i
        platform, encoding = u16(record), u16(record + 2)
        if platform == 0 or (platform == 3 and encoding in (1, 10)):
            offset = cmap + u32(record + 4)
            if offset not in subtables:
                subtables.append(offset)

    def has(codepoint):
        for table in subtables:
            fmt = u16(table)
            if fmt == 12:
                for i in range(u32(table + 12)):
                    start, end, glyph = struct.unpack_from(">III", data, table + 16 + 12 * i)
                    if start <= codepoint <= end and glyph + codepoint - start != 0:
                        return True
            elif fmt == 4 and codepoint <= 0xFFFF:
                count = u16(table + 6) // 2
                for i in range(count):
                    end = u16(table + 14 + 2 * i)
                    start = u16(table + 16 + 2 * count + 2 * i)
                    if not start <= codepoint <= end:
                        continue
                    delta = u16(table + 16 + 4 * count + 2 * i)
                    address = table + 16 + 6 * count + 2 * i
                    offset = u16(address)
                    glyph = u16(address + offset + 2 * (codepoint - start)) if offset else codepoint
                    if (glyph + delta) & 0xFFFF and (offset == 0 or glyph != 0):
                        return True
        return False
    return has


def validate():
    catalog = json.loads(CATALOG.read_text(encoding="utf-8"))
    errors, keys, source_by_key = [], set(), {}
    if catalog.get("locale") != "zh-Hans":
        errors.append("locale debe ser zh-Hans")
    assets = OUT / "asset-inventory.json"
    if assets.exists():
        for entry in json.loads(assets.read_text(encoding="utf-8"))["entries"]:
            source_by_key[entry["key"]] = entry["es"]
    characters = set()
    for entry in catalog["entries"]:
        key, value = entry["key"], entry["value"]
        normalized = unicodedata.normalize("NFC", key)
        if normalized in keys:
            errors.append(f"Clave duplicada: {key!r}")
        keys.add(normalized)
        if not key or not value.strip():
            errors.append(f"Entrada vacía: {key!r}")
        if key.startswith(("tutorial.", "tooltip.")) and key not in source_by_key:
            errors.append(f"Clave tutorial desconocida (ejecutar --extract-assets): {key!r}")
        source = source_by_key.get(key, key)
        if Counter(PLACEHOLDERS.findall(source)) != Counter(PLACEHOLDERS.findall(value)):
            errors.append(f"Variables distintas: {key!r}")
        if Counter(TAGS.findall(source)) != Counter(TAGS.findall(value)):
            errors.append(f"Etiquetas TMP distintas: {key!r}")
        if balanced_tags(source) and not balanced_tags(value):
            errors.append(f"Etiquetas TMP mal anidadas: {key!r}")
        characters.update(c for c in TAGS.sub("", value) if not c.isspace())
    font_path = ROOT / "Assets/Resources/Fuentes/NotoSansCJK/NotoSansCJKsc-Regular.otf"
    has_char = font_has_char(font_path)
    missing = sorted(c for c in characters if not has_char(ord(c)))
    if missing:
        errors.append("Glifos ausentes: " + " ".join(f"U+{ord(c):04X} {c}" for c in missing))
    code = OUT / "code-inventory.json"
    known = set()
    if code.exists():
        known = {unicodedata.normalize("NFC", e["key"]) for e in json.loads(code.read_text(encoding="utf-8"))["strings"]}
    report = {"entries": len(catalog["entries"]), "characters": len(characters), "missingGlyphs": missing,
        "codeKeysTranslated": len(keys & known), "codeKeysTotal": len(known), "errors": errors}
    write_json("validation.json", report)
    print(json.dumps(report, ensure_ascii=False, indent=2))
    return 1 if errors else 0


if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument("--extract-assets", action="store_true")
    parser.add_argument("--validate", action="store_true")
    args = parser.parse_args()
    if args.extract_assets:
        extract_assets()
    if args.validate:
        raise SystemExit(validate())

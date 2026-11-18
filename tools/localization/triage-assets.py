"""Ubica textos pendientes en su jerarquía real; no deduce visibilidad de m_IsActive."""
import json
from pathlib import Path
import re
import unicodedata
import yaml

ROOT = Path(__file__).resolve().parents[2]
OUT = ROOT / 'outputs/localization'
loader = getattr(yaml, 'CSafeLoader', yaml.SafeLoader)
catalog = {unicodedata.normalize('NFC', e['key']) for e in json.loads(
    (ROOT / 'Assets/Resources/Localization/zh-Hans.json').read_text(encoding='utf-8'))['entries']}
inventory = json.loads((OUT / 'asset-inventory.json').read_text(encoding='utf-8'))['entries']
pending = [e for e in inventory if not e['path'].startswith('Assets/_Recovery/')
    and e['es'].strip() and unicodedata.normalize('NFC', e['key']) not in catalog]

scripts = {}
for path in (ROOT / 'Assets').rglob('*.cs.meta'):
    match = re.search(r'^guid: (\w+)', path.read_text(encoding='utf-8-sig'), re.M)
    if match:
        scripts[match[1]] = path.relative_to(ROOT).as_posix()[:-5]

documents = {}
for name in sorted({e['path'] for e in pending}):
    raw = (ROOT / name).read_text(encoding='utf-8-sig')
    matches = list(re.finditer(r'^--- !u!\d+ &(-?\d+)[^\n]*\n', raw, re.M))
    data = {}
    for i, match in enumerate(matches):
        body = raw[match.end():matches[i + 1].start() if i + 1 < len(matches) else len(raw)]
        if not re.search(r'^  (?:m_GameObject|m_Component|m_Modification):', body, re.M):
            continue
        doc = yaml.load(body, Loader=loader)
        if doc:
            data[int(match[1])] = next(iter(doc.values()))
    documents[name] = data

rows = []
for entry in pending:
    docs = documents[entry['path']]
    component = docs.get(int(entry['fileID']), {})
    go_id = component.get('m_GameObject', {}).get('fileID', 0)
    go = docs.get(go_id, {})
    hierarchy, owners, seen = [], [], set()
    while go and go_id not in seen:
        seen.add(go_id)
        hierarchy.insert(0, go.get('m_Name', str(go_id)))
        parent = 0
        for item in go.get('m_Component', []):
            comp = docs.get(item['component']['fileID'], {})
            if 'm_Father' in comp:
                parent = comp['m_Father'].get('fileID', 0)
            script = scripts.get(comp.get('m_Script', {}).get('guid', ''))
            if script:
                owners.append(script)
        go_id = docs.get(parent, {}).get('m_GameObject', {}).get('fileID', 0)
        go = docs.get(go_id, {})
    rows.append({**entry, 'hierarchy': '/'.join(hierarchy), 'ancestorScripts': sorted(set(owners)),
        'classification': 'requires_source_or_play_mode_review'})

(OUT / 'asset-triage.json').write_text(json.dumps(rows, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')
print(json.dumps({'pendingOccurrences': len(rows), 'distinctValues': len({e['key'] for e in rows}),
    'files': len(documents), 'report': 'outputs/localization/asset-triage.json'}))

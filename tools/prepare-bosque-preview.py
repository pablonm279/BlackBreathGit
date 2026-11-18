"""Creates an isolated Unity rendering fixture with copies of forest assets."""
from pathlib import Path
import json
import re
import shutil
import sys

root = Path(__file__).resolve().parents[1]
target = root / "Temp/BosquePreview"
(target / "Assets/Editor").mkdir(parents=True, exist_ok=True)
(target / "Packages").mkdir(exist_ok=True)
(target / "ProjectSettings").mkdir(exist_ok=True)
# El decorador real necesita uGUI; se usa una copia local del paquete ya instalado.
ugui = next((root / "Library/PackageCache").glob("com.unity.ugui@*"))
shutil.copytree(ugui, target / "Packages/com.unity.ugui", dirs_exist_ok=True,
                ignore=shutil.ignore_patterns("Tests", "Tests.meta", "Documentation~", "Samples~"))
(target / "Packages/manifest.json").write_text(json.dumps({"dependencies": {
    "com.unity.modules.particlesystem": "1.0.0", "com.unity.modules.physics": "1.0.0",
    "com.unity.modules.imageconversion": "1.0.0", "com.unity.modules.audio": "1.0.0",
    "com.unity.modules.ui": "1.0.0", "com.unity.modules.imgui": "1.0.0"
}}))
for name in ["ProjectVersion.txt", "ProjectSettings.asset", "QualitySettings.asset", "TagManager.asset"]:
    shutil.copy2(root / "ProjectSettings" / name, target / "ProjectSettings" / name)
# La prueba no debe compartir PlayerPrefs con las partidas ni con las opciones del usuario.
settings = target / "ProjectSettings/ProjectSettings.asset"
text = settings.read_text(encoding="utf-8-sig")
text = re.sub(r"(?m)^  companyName:.*$", "  companyName: CodexForestPreview", text)
text = re.sub(r"(?m)^  productName:.*$", "  productName: BosquePulidoAislado", text)
settings.write_text(text, encoding="utf-8")

guids = {}
for meta in (root / "Assets").rglob("*.meta"):
    match = re.search(r"^guid: (\w+)", meta.read_text(encoding="utf-8-sig", errors="replace"), re.M)
    if match:
        guids[match[1]] = meta.with_suffix("")

copied = set()
def copy_asset(source):
    if source in copied or not source.is_file() or source.suffix in {".cs", ".asmdef", ".dll"}:
        return
    copied.add(source)
    destination = target / source.relative_to(root)
    destination.parent.mkdir(parents=True, exist_ok=True)
    shutil.copy2(source, destination)
    if source.with_name(source.name + ".meta").exists():
        shutil.copy2(source.with_name(source.name + ".meta"), destination.with_name(destination.name + ".meta"))
    if source.suffix in {".mat", ".prefab", ".asset", ".controller"}:
        for guid in re.findall(r"guid: ([a-f0-9]{32})", source.read_text(encoding="utf-8-sig", errors="replace")):
            if guid in guids:
                copy_asset(guids[guid])

for name in ["Llama1GO.prefab", "LlamaEspectral.prefab", "BosqueArdiente - Arbol1.prefab", "BosqueArdiente - Piedra1.prefab", "MatBosqueArdienteSuelo.mat"]:
    copy_asset(root / "Assets/Resources/ObjetosMapa" / name)
for name in ["BosqueArdienteAmbiente.shader", "BosqueArdienteSuelo.shader", "BosqueArdienteLlamas.shader",
             "BosqueArdienteMadera.shader", "BosqueArdienteFondo.shader"]:
    copy_asset(root / "Assets/Resources" / name)

# Export one real, authored map flame as an isolated fixture (never save the scene).
scene = (root / "Assets/Scenes/ES-Campaña.unity").read_text(encoding="utf-8-sig")
blocks = {m[2]: m[0] for m in re.finditer(r"(?ms)^--- !u!(\d+) &(-?\d+).*?(?=^--- !u!|\Z)", scene)}
def hierarchy(key, found):
    if key in found or key not in blocks:
        return
    found.add(key)
    block = blocks[key]
    for child in re.findall(r"component: \{fileID: (-?\d+)", block):
        hierarchy(child, found)
    owner = re.search(r"m_GameObject: \{fileID: (-?\d+)", block)
    if owner:
        hierarchy(owner[1], found)
    children = re.search(r"(?ms)^  m_Children:(.*?)(?=^  [A-Za-z_])", block)
    if children:
        for child in re.findall(r"fileID: (-?\d+)", children[1]):
            hierarchy(child, found)

container = next(k for k, b in blocks.items() if re.search(r"(?m)^  m_Name: LlamasContainer$", b))
initial = set()
hierarchy(container, initial)
particle = next(blocks[k] for k in sorted(initial) if blocks[k].startswith("--- !u!198 "))
owner = re.search(r"m_GameObject: \{fileID: (-?\d+)", particle)[1]
single = set()
hierarchy(owner, single)
root_transform = next(k for k in single if blocks[k].startswith("--- !u!4 ")
    and f"m_GameObject: {{fileID: {owner}}}" in blocks[k])
fixture = []
for key in sorted(single):
    block = blocks[key]
    if key == root_transform:
        block = re.sub(r"m_Father: \{fileID: -?\d+\}", "m_Father: {fileID: 0}", block)
        block = re.sub(r"m_LocalPosition: \{[^}]+\}", "m_LocalPosition: {x: 0, y: 0, z: 0}", block)
    fixture.append(block)
    for guid in re.findall(r"guid: ([a-f0-9]{32})", block):
        if guid in guids:
            copy_asset(guids[guid])
(target / "Assets/Resources/LlamaInicial.prefab").write_text(
    "%YAML 1.1\n%TAG !u! tag:unity3d.com,2011:\n" + "".join(fixture), encoding="utf-8")
print(f"Initial map hierarchy: {sum(blocks[k].startswith('--- !u!198 ') for k in initial)} particle systems; fixture: {len(single)} components.")
shutil.copy2(root / "Assets/Scripts/Campania/AmbienteBosqueArdiente.cs", target / "Assets/AmbienteBosqueArdiente.cs")
for name in ["AdornosBosqueArdiente.cs", "FondoBosqueArdiente.cs"]:
    shutil.copy2(root / "Assets/Scripts/Campania" / name, target / "Assets" / name)
shutil.copy2(root / "Assets/SunController.cs", target / "Assets/SunController.cs")
shutil.copy2(root / "Assets/MapDecorator.cs", target / "Assets/MapDecorator.cs")
for name in ["ProbabilidadDeAparicion.cs", "RandomizeScaleAnchorBottom.cs"]:
    shutil.copy2(root / "Assets" / name, target / "Assets" / name)
    shutil.copy2(root / "Assets" / (name + ".meta"), target / "Assets" / (name + ".meta"))
shutil.copy2(root / "tools/bosque-preview/ContextoPreview.cs", target / "Assets/ContextoPreview.cs")
shutil.copy2(root / "tools/bosque-preview/BosqueRender.cs", target / "Assets/Editor/BosqueRender.cs")
if "--pulido" in sys.argv:
    renderer = target / "Assets/Editor/BosqueRender.cs"
    renderer.write_text(renderer.read_text(encoding="utf-8-sig").replace(
        "../../output/ambiente-bosque", "../../output/bosque-pulido-v1/regresion"), encoding="utf-8")
    shutil.copy2(root / "tools/bosque-preview/BosquePulidoRender.cs", target / "Assets/Editor/BosquePulidoRender.cs")
    antes = root / "output/bosque-pulido-v1/fuentes-antes/BosqueArdienteSuelo.shader"
    if antes.exists():
        (target / "Assets/Resources/BosqueArdienteSueloAntes.shader").write_text(
            antes.read_text(encoding="utf-8-sig").replace('Shader "Campania/Bosque Ardiente/Suelo"',
                'Shader "Campania/Bosque Ardiente/Suelo Antes"'), encoding="utf-8")
# Extract the real quality/LOD path verbatim, without booting unrelated UI/postprocessing.
quality = (root / "Assets/Scripts/Visual/VisualPolishRuntime.cs").read_text(encoding="utf-8-sig")
def quality_member(pattern):
    start = re.search(pattern, quality, re.M).start()
    opening = quality.index("{", start)
    depth = 1
    end = opening + 1
    while depth:
        depth += (quality[end] == "{") - (quality[end] == "}")
        end += 1
    return quality[start:end]
members = [quality_member(r"^  private struct ParticleAmountDefaults")]
for method in ["ApplyGeneratedCampaignVfxQualityScale", "ApplyGeneratedCampaignVfxLod",
               "ApplyGeneratedCampaignVfxLightBudget", "ApplyParticleSystemAmountScale",
               "ScaleMinMaxCurve", "ResolveQualityAmountMultiplier", "IsCampaignScene"]:
    members.append(quality_member(r"^  (?:public|private) static [^\n]+ " + method + r"\("))
for field in ["PrefGraficosIndex", "CalidadGraficaBaja", "CampaignGeneratedVfxBaseMultiplier",
              "CampaignParticleReductionPerQualityLevel", "particleAmountDefaultsById", "campaignGeneratedVfxRootCounter"]:
    members.append(next(line for line in quality.splitlines() if re.search(r"\b"+field+r"\s*(?:=|;)", line)))
(target / "Assets/VisualPolishRuntime.cs").write_text(
    "using UnityEngine; using UnityEngine.SceneManagement; using System.Collections.Generic;\n"
    + "public static class VisualPolishRuntime {\n" + "\n".join(members) + "\n}\n", encoding="utf-8")
print(f"Preview: {len(copied)} assets, {sum(p.stat().st_size for p in copied)/1048576:.1f} MiB. {target}")

if "--pulido" in sys.argv:
    # Copia visual de la escena real: conserva transforms, luces, partículas, terrenos y cámara.
    # Excluye gestores, UI, tutorial y cualquier ejecución de lógica de campaña.
    zone = next(v for v in blocks.values() if "bosqueardienteContenedorGameObjects:" in v)
    selected = set()
    forest_id = re.search(r"bosqueardienteContenedorGameObjects: \{fileID: (-?\d+)", zone)[1]
    hierarchy(forest_id, selected)
    for field in ["TexturaTerreno", "TexturaTerrenoExtension"]:
        hierarchy(re.search(field + r": \{fileID: (-?\d+)", zone)[1], selected)
    camera = next(k for k, v in blocks.items() if v.startswith("--- !u!1 ") and "m_Name: Main Camera\n" in v)
    hierarchy(camera, selected)
    for key in list(selected):
        block = blocks[key]
        if not block.startswith(("--- !u!4 ", "--- !u!224 ")):
            continue
        parent = re.search(r"m_Father: \{fileID: (-?\d+)", block)[1]
        while parent != "0" and parent in blocks and parent not in selected:
            selected.add(parent)
            owner = re.search(r"m_GameObject: \{fileID: (-?\d+)", blocks[parent])[1]
            selected.add(owner)
            for component in re.findall(r"component: \{fileID: (-?\d+)", blocks[owner]):
                selected.add(component)
            parent = re.search(r"m_Father: \{fileID: (-?\d+)", blocks[parent])[1]
    keep_scripts = set()
    for name in ["MapDecorator.cs", "SunController.cs"]:
        meta = root / "Assets" / (name + ".meta")
        keep_scripts.add(re.search(r"guid: (\w+)", meta.read_text())[1])
        shutil.copy2(meta, target / "Assets" / meta.name)
    allowed = {1, 4, 20, 23, 33, 64, 65, 108, 114, 198, 199, 224}
    for key in list(selected):
        kind = int(re.match(r"--- !u!(\d+)", blocks[key])[1])
        if kind not in allowed or (kind == 114 and not any(g in blocks[key] for g in keep_scripts)):
            selected.remove(key)
    visual = []
    roots = []
    for key in sorted(selected):
        block = blocks[key]
        kind = int(re.match(r"--- !u!(\d+)", block)[1])
        if kind == 1:
            block = re.sub(r"(?m)^  - component: \{fileID: (-?\d+)\}\n", lambda m: m[0] if m[1] in selected else "", block)
            if key == forest_id:
                block = block.replace("m_IsActive: 0", "m_IsActive: 1")
        if kind in {4, 224}:
            parent = re.search(r"m_Father: \{fileID: (-?\d+)", block)[1]
            if parent not in selected:
                block = re.sub(r"m_Father: \{fileID: -?\d+\}", "m_Father: {fileID: 0}", block)
                roots.append(key)
            children = re.search(r"(?ms)^  m_Children:(.*?)(?=^  [A-Za-z_])", block)
            if children:
                block = block[:children.start()] + "  m_Children:" + re.sub(r"(?m)^  - \{fileID: (-?\d+)\}\n", lambda m: m[0] if m[1] in selected else "", children[1]) + block[children.end():]
        for guid in re.findall(r"guid: ([a-f0-9]{32})", block):
            if guid in guids:
                copy_asset(guids[guid])
        visual.append(block)
    for block in blocks.values():
        if block.startswith("--- !u!104 "):
            visual.insert(0, re.sub(r"m_Sun: \{fileID: -?\d+\}", "m_Sun: {fileID: 0}", block))
    visual.append("--- !u!1660057539 &9223372036854775807\nSceneRoots:\n  m_ObjectHideFlags: 0\n  m_Roots:\n" + "".join("  - {fileID: " + key + "}\n" for key in roots))
    (target / "Assets/BosqueEscenarioReal.unity").write_text("%YAML 1.1\n%TAG !u! tag:unity3d.com,2011:\n" + "".join(visual), encoding="utf-8")
    source = (root / "Assets/Scripts/Campania/AtributosZona.cs").read_text(encoding="utf-8-sig")
    start = source.index("IEnumerator AdornarBosqueArdienteConFadeAsync()")
    stop = source.index("AmbienteBosqueArdiente.Preparar(this,", start)
    calls = re.findall(r"yield return scMapDecorator.GenerarAsyncCR\((.*?)\);", source[start:stop], re.S)
    generated = []
    for call in calls:
        field = re.match(r"\s*(\w+)", call)[1]
        guid = re.search(r"\b" + field + r": \{fileID: -?\d+, guid: (\w+)", zone)[1]
        prefab = guids[guid]
        copy_asset(prefab)
        call = re.sub(r"^\s*" + field, 'UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("' + prefab.relative_to(root).as_posix() + '")', call)
        generated.append("yield return decorador.GenerarAsyncCR(" + call + ");")
    (target / "Assets/Editor/BosqueDecoracionReal.cs").write_text("using UnityEngine; using System.Collections;\npublic static class BosqueDecoracionReal { public static IEnumerator Generar(MapDecorator decorador) {\n" + "\n".join(generated) + "\n} }", encoding="utf-8")
    for name in ["BosqueEscenarioRender.cs"]:
        shutil.copy2(root / "tools/bosque-preview" / name, target / "Assets/Editor" / name)
    original = root / "output/bosque-pulido-v1/fuentes-antes/AmbienteBosqueArdiente.cs"
    if original.exists():
        source = original.read_text(encoding="utf-8-sig").replace("AmbienteBosqueArdiente", "AmbienteBosqueArdienteAntes")
        source = source.replace('"BosqueArdienteSuelo"', '"BosqueArdienteSueloAntes"')
        (target / "Assets/AmbienteBosqueArdienteAntes.cs").write_text(source, encoding="utf-8")
    print(f"Escenario real: {len(selected)} componentes visuales y {len(calls)} tandas originales de decoración.")

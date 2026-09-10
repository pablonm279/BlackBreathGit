"""Creates an isolated Unity rendering fixture with copies of forest assets."""
from pathlib import Path
import json
import re
import shutil

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
for name in ["BosqueArdienteAmbiente.shader", "BosqueArdienteSuelo.shader", "BosqueArdienteLlamas.shader"]:
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
shutil.copy2(root / "Assets/MapDecorator.cs", target / "Assets/MapDecorator.cs")
for name in ["ProbabilidadDeAparicion.cs", "RandomizeScaleAnchorBottom.cs"]:
    shutil.copy2(root / "Assets" / name, target / "Assets" / name)
    shutil.copy2(root / "Assets" / (name + ".meta"), target / "Assets" / (name + ".meta"))
shutil.copy2(root / "tools/bosque-preview/ContextoPreview.cs", target / "Assets/ContextoPreview.cs")
shutil.copy2(root / "tools/bosque-preview/BosqueRender.cs", target / "Assets/Editor/BosqueRender.cs")
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

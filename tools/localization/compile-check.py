"""Compila con las referencias reales de Unity sin abrir otra instancia ni escribir en Library."""
from pathlib import Path
import subprocess
import sys

root = Path(__file__).resolve().parents[2]
out = root / "outputs/localization/compile"
out.mkdir(parents=True, exist_ok=True)
editor = Path(sys.argv[1]) if len(sys.argv) > 1 else Path("C:/Program Files/Unity/Hub/Editor/6000.3.20f1/Editor")
csc = editor / "Data/DotNetSdkRoslyn/csc.dll"
for assembly in ("Assembly-CSharp", "Assembly-CSharp-Editor"):
    responses = [p for p in (root / "Library/Bee/artifacts").glob(f"*/{assembly}.rsp")
        if "-define:UNITY_EDITOR\n" in p.read_text(encoding="utf-8-sig")]
    if not responses:
        raise SystemExit(f"No existe el response file de Unity para {assembly}.")
    response = max(responses, key=lambda p: p.stat().st_mtime)
    lines = []
    for line in response.read_text(encoding="utf-8-sig").splitlines():
        if line.startswith(("-out:", "-refout:", "/out:", "/refout:", "/analyzer:", "-analyzer:")):
            continue
        if assembly.endswith("-Editor") and line.startswith(("-r:", "/reference:", "-reference:")) and any(name in line for name in ("Assembly-CSharp.dll", "Assembly-CSharp.ref.dll")):
            line = '-r:"' + (out / "Assembly-CSharp.dll").as_posix() + '"'
        lines.append(line)
    if assembly == "Assembly-CSharp":
        for source in sorted((root / "Assets/Scripts/Localization").glob("*.cs")):
            relative = source.relative_to(root).as_posix()
            if not any(relative in line.replace("\\", "/") for line in lines):
                lines.append('"' + relative + '"')
    lines.append('-out:"' + (out / (assembly + ".dll")).as_posix() + '"')
    generated = out / (assembly + ".rsp")
    generated.write_text("\n".join(lines) + "\n", encoding="utf-8")
    result = subprocess.run(["dotnet", str(csc), "@" + str(generated)], cwd=root, capture_output=True, text=True, encoding="utf-8")
    (out / (assembly + ".log")).write_text(result.stdout + result.stderr, encoding="utf-8")
    errors = [line for line in (result.stdout + result.stderr).splitlines() if "error " in line.lower()]
    print(f"{assembly}: exit {result.returncode}, {len(errors)} errors")
    for error in errors:
        print(error)
    if result.returncode:
        raise SystemExit(result.returncode)

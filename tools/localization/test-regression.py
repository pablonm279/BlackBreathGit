"""Compara el traductor actual contra una copia de git HEAD; no altera el archivo protegido."""
from pathlib import Path
import re
import subprocess

root = Path(__file__).resolve().parents[2]
out = root / "outputs/localization/regression"
out.mkdir(parents=True, exist_ok=True)
baseline = subprocess.run(["git", "show", "HEAD:Assets/TRADU.cs"], cwd=root, capture_output=True, check=True).stdout.decode("utf-8-sig")
(out / "LegacyTRADU.cs").write_text(re.sub(r"\bTRADU\b", "LegacyTRADU", baseline), encoding="utf-8")
result = subprocess.run(["dotnet", "run", "--project", "tools/localization/tests/LocalizationChecks.csproj", "--", str(root)],
    cwd=root, capture_output=True, text=True, encoding="utf-8")
(out / "results.log").write_text(result.stdout + result.stderr, encoding="utf-8")
print(result.stdout + result.stderr)
raise SystemExit(result.returncode)

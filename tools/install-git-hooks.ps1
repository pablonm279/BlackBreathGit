param(
    [switch]$CheckOnly
)

$repoRoot = (& git rev-parse --show-toplevel 2>$null)
if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($repoRoot)) {
    throw "No se encontro un repositorio Git."
}

$repoRoot = $repoRoot.Trim()
$expectedHooksPath = ".githooks"
$currentHooksPath = (& git -C $repoRoot config --get core.hooksPath 2>$null)

if ($CheckOnly) {
    if ($currentHooksPath -ne $expectedHooksPath) {
        Write-Error "Hook de encoding inactivo. Ejecuta: powershell -File tools/install-git-hooks.ps1"
        exit 1
    }

    Write-Host "Hook de encoding activo: $expectedHooksPath" -ForegroundColor Green
    exit 0
}

& git -C $repoRoot config core.hooksPath $expectedHooksPath
if ($LASTEXITCODE -ne 0) {
    throw "No se pudo configurar core.hooksPath."
}

$isWindows = $env:OS -eq "Windows_NT"
if (-not $isWindows) {
    & chmod +x ([System.IO.Path]::Combine($repoRoot, ".githooks", "pre-commit"))
    if ($LASTEXITCODE -ne 0) {
        throw "No se pudo marcar el hook pre-commit como ejecutable."
    }
}

Write-Host "Hook de encoding activado en $expectedHooksPath." -ForegroundColor Green
Write-Host "Los commits con archivos no UTF-8 o mojibake seran bloqueados."

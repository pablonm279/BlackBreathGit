param(
    [string]$Root = "."
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Resolve-ProjectPath([string]$projectRoot, [string]$relativePath) {
    return Join-Path $projectRoot $relativePath
}

function Write-Section([string]$title) {
    Write-Host ""
    Write-Host $title -ForegroundColor Cyan
}

function Decode-UnityEscapes([string]$text) {
    return [System.Text.RegularExpressions.Regex]::Replace(
        $text,
        "\\x([0-9A-Fa-f]{2})",
        {
            param($match)
            [char][Convert]::ToInt32($match.Groups[1].Value, 16)
        }
    )
}

$projectRoot = (Resolve-Path $Root).Path
$projectVersionFile = Resolve-ProjectPath $projectRoot "ProjectSettings/ProjectVersion.txt"
$buildSettingsFile = Resolve-ProjectPath $projectRoot "ProjectSettings/EditorBuildSettings.asset"
$packagesFile = Resolve-ProjectPath $projectRoot "Packages/manifest.json"
$scriptsRoot = Resolve-ProjectPath $projectRoot "Assets/Scripts"
$nestedCopyRoot = Resolve-ProjectPath $projectRoot "GDD - Untitled"

if (-not (Test-Path $projectVersionFile)) {
    throw "No se encontro ProjectSettings/ProjectVersion.txt en '$projectRoot'."
}

$unityVersion = ((Get-Content $projectVersionFile) | Where-Object { $_ -match "^m_EditorVersion:" }) -replace "^m_EditorVersion:\s*", ""
$sceneEntries = @()
if (Test-Path $buildSettingsFile) {
    $enabled = $null
    foreach ($line in Get-Content $buildSettingsFile) {
        if ($line -match "^\s*- enabled: (\d)") {
            $enabled = [int]$Matches[1]
            continue
        }

        if ($line -match "^\s*path: (.+)$") {
            $scenePath = Decode-UnityEscapes($Matches[1].Trim('"'))
            $sceneEntries += [PSCustomObject]@{
                Enabled = $enabled
                Path = $scenePath
            }
            $enabled = $null
        }
    }
}

$topLevelAssetDirs = @()
if (Test-Path (Resolve-ProjectPath $projectRoot "Assets")) {
    $topLevelAssetDirs = Get-ChildItem (Resolve-ProjectPath $projectRoot "Assets") -Directory | Select-Object -ExpandProperty Name
}

$scriptDirs = @()
if (Test-Path $scriptsRoot) {
    $scriptDirs = Get-ChildItem $scriptsRoot -Directory | Select-Object -ExpandProperty Name
}

$asmdefCount = @(Get-ChildItem (Resolve-ProjectPath $projectRoot "Assets") -Recurse -Filter *.asmdef -File -ErrorAction SilentlyContinue).Count
$csCount = @(Get-ChildItem (Resolve-ProjectPath $projectRoot "Assets") -Recurse -Filter *.cs -File -ErrorAction SilentlyContinue).Count
$sceneCount = @(Get-ChildItem (Resolve-ProjectPath $projectRoot "Assets") -Recurse -Filter *.unity -File -ErrorAction SilentlyContinue).Count

$dependencies = @()
if (Test-Path $packagesFile) {
    $manifest = Get-Content $packagesFile | ConvertFrom-Json
    $dependencies = $manifest.dependencies.PSObject.Properties |
        Sort-Object Name |
        Select-Object -First 12 |
        ForEach-Object { "{0}: {1}" -f $_.Name, $_.Value }
}

Write-Host "Proyecto Unity: $projectRoot" -ForegroundColor Green

Write-Section "Version"
Write-Host "Unity: $unityVersion"
Write-Host "Escenas detectadas: $sceneCount"
Write-Host "Scripts C#: $csCount"
Write-Host "Asmdefs propios en Assets: $asmdefCount"

Write-Section "Build Settings"
if ($sceneEntries.Count -eq 0) {
    Write-Host "Sin escenas configuradas."
}
else {
    foreach ($scene in $sceneEntries) {
        $flag = if ($scene.Enabled -eq 1) { "[x]" } else { "[ ]" }
        Write-Host "$flag $($scene.Path)"
    }
}

Write-Section "Assets"
if ($topLevelAssetDirs.Count -eq 0) {
    Write-Host "Sin directorios en Assets."
}
else {
    $topLevelAssetDirs | ForEach-Object { Write-Host "- $_" }
}

Write-Section "Modulos de Scripts"
if ($scriptDirs.Count -eq 0) {
    Write-Host "Assets/Scripts no existe o no tiene subdirectorios."
}
else {
    $scriptDirs | ForEach-Object { Write-Host "- $_" }
}

Write-Section "Packages destacados"
if ($dependencies.Count -eq 0) {
    Write-Host "Sin dependencias leidas desde Packages/manifest.json."
}
else {
    $dependencies | ForEach-Object { Write-Host "- $_" }
}

Write-Section "Alertas"
if (Test-Path (Resolve-ProjectPath $nestedCopyRoot "Assets")) {
    Write-Host "- Existe una copia parcial en 'GDD - Untitled/Assets'. Evitar editar ahi salvo pedido explicito." -ForegroundColor Yellow
}
if ($asmdefCount -eq 0) {
    Write-Host "- No hay asmdefs propios en Assets; los scripts compilan en Assembly-CSharp." -ForegroundColor Yellow
}
Write-Host "- Revisar 'git status --short' antes de editar, especialmente en escenas/prefabs." -ForegroundColor Yellow

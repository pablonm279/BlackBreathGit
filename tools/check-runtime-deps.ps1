param(
    [string]$Root = ".",
    [switch]$FailOnHit
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function IsRuntimeScript([System.IO.FileInfo]$file) {
    $normalized = $file.FullName.Replace('\', '/')
    if (-not $normalized.EndsWith(".cs")) {
        return $false
    }

    if ($normalized -notmatch "/Assets/") {
        return $false
    }

    if ($normalized -match "/Editor/") {
        return $false
    }

    return $true
}

function Get-DirectiveCondition([string]$trimmedLine) {
    if ($trimmedLine -match '^#(if|elif)\s+(.+)$') {
        return $Matches[2].Trim()
    }

    return $null
}

function IsEditorOnlyCondition([string]$condition) {
    if ([string]::IsNullOrWhiteSpace($condition)) {
        return $false
    }

    return $condition -eq "UNITY_EDITOR"
}

$projectRoot = (Resolve-Path $Root).Path
$assetRoot = Join-Path $projectRoot "Assets"
if (-not (Test-Path $assetRoot)) {
    throw "No se encontro la carpeta Assets en '$projectRoot'."
}

$patterns = @(
    @{
        Name = "UnityEditor using"
        Regex = '^\s*using\s+UnityEditor(\.|;)' 
    },
    @{
        Name = "NUnit using"
        Regex = '^\s*using\s+NUnit(\.|;)' 
    },
    @{
        Name = "UnityEditor reference"
        Regex = '\bUnityEditor\.'
    },
    @{
        Name = "EditorUtility reference"
        Regex = '\bEditorUtility\.'
    },
    @{
        Name = "AssetDatabase reference"
        Regex = '\bAssetDatabase\.'
    },
    @{
        Name = "PrefabUtility reference"
        Regex = '\bPrefabUtility\.'
    },
    @{
        Name = "NUnit reference"
        Regex = '\bNUnit\.'
    }
)

$hits = New-Object System.Collections.Generic.List[string]
$files = Get-ChildItem $assetRoot -Recurse -File -Filter *.cs -ErrorAction SilentlyContinue |
    Where-Object { IsRuntimeScript $_ }

foreach ($file in $files) {
    $lines = Get-Content $file.FullName
    $editorGuardStack = New-Object System.Collections.Generic.List[bool]

    for ($i = 0; $i -lt $lines.Length; $i++) {
        $line = $lines[$i]
        $trimmed = $line.Trim()

        $directiveCondition = Get-DirectiveCondition $trimmed
        if ($null -ne $directiveCondition) {
            $editorGuardStack.Add((IsEditorOnlyCondition $directiveCondition))
            continue
        }

        if ($trimmed -match '^#else\b') {
            if ($editorGuardStack.Count -gt 0) {
                $lastIndex = $editorGuardStack.Count - 1
                $editorGuardStack[$lastIndex] = -not $editorGuardStack[$lastIndex]
            }
            continue
        }

        if ($trimmed -match '^#endif\b') {
            if ($editorGuardStack.Count -gt 0) {
                $editorGuardStack.RemoveAt($editorGuardStack.Count - 1)
            }
            continue
        }

        if ($trimmed.StartsWith("//")) {
            continue
        }

        $insideEditorOnlyBlock = $false
        if ($editorGuardStack.Count -gt 0) {
            $insideEditorOnlyBlock = $editorGuardStack.Contains($true)
        }

        if ($insideEditorOnlyBlock) {
            continue
        }

        foreach ($pattern in $patterns) {
            if ($line -match $pattern.Regex) {
                $hits.Add(("{0}:{1}: [{2}] {3}" -f @($file.FullName, ($i + 1), $pattern.Name, $line.Trim())))
            }
        }
    }
}

if ($hits.Count -gt 0) {
    Write-Host "Dependencias de editor/test sospechosas en scripts runtime:" -ForegroundColor Red
    $hits | Select-Object -First 200 | ForEach-Object { Write-Host "  $_" -ForegroundColor Red }
}
else {
    Write-Host "No se detectaron dependencias de editor/test en scripts runtime." -ForegroundColor Green
}

if ($FailOnHit -and $hits.Count -gt 0) {
    exit 1
}

exit 0

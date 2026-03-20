param(
    [string]$Root = ".",
    [switch]$FailOnHit
)

$extensions = @(
    ".cs", ".txt", ".md", ".json", ".xml", ".yml", ".yaml",
    ".csv", ".shader", ".cginc", ".hlsl", ".compute", ".uss",
    ".uxml", ".asmdef", ".asmref", ".meta"
)

$charReplacement = [char]0xFFFD # �

$suspectTokens = @(
    [string]::Concat([char]0x00C3, [char]0x00A1), # Ã¡
    [string]::Concat([char]0x00C3, [char]0x00A9), # Ã©
    [string]::Concat([char]0x00C3, [char]0x00AD), # Ã­
    [string]::Concat([char]0x00C3, [char]0x00B3), # Ã³
    [string]::Concat([char]0x00C3, [char]0x00BA), # Ãº
    [string]::Concat([char]0x00C3, [char]0x00B1), # Ã±
    [string]::Concat([char]0x00C3, [char]0x0081), # Ã
    [string]::Concat([char]0x00C3, [char]0x0089), # Ã‰
    [string]::Concat([char]0x00C3, [char]0x0093), # Ã“
    [string]::Concat([char]0x00C3, [char]0x009A), # Ãš
    [string]::Concat([char]0x00C3, [char]0x0091), # Ã‘
    [string]::Concat([char]0x00C2, [char]0x00BF), # Â¿
    [string]::Concat([char]0x00C2, [char]0x00A1), # Â¡
    [string]::Concat([char]0x00C3, [char]0x0192, [char]0x00C2), # ÃƒÂ
    [string]::Concat([char]0x00E2, [char]0x20AC) # â€
)

$utf8Strict = New-Object System.Text.UTF8Encoding($false, $true)
$hits = New-Object System.Collections.Generic.List[string]
$invalidUtf8 = New-Object System.Collections.Generic.List[string]

function IsTextExtension([string]$path) {
    $ext = [System.IO.Path]::GetExtension($path)
    return $extensions -contains $ext
}

function GetTargetFiles([string]$rootPath) {
    if (Test-Path $rootPath -PathType Leaf) {
        if (IsTextExtension $rootPath) {
            return ,(Get-Item $rootPath)
        }
        return @()
    }

    return Get-ChildItem -Path $rootPath -Recurse -File -ErrorAction SilentlyContinue |
        Where-Object { IsTextExtension $_.FullName }
}

$files = GetTargetFiles $Root

foreach ($file in $files) {
    try {
        $bytes = [System.IO.File]::ReadAllBytes($file.FullName)
        $text = $utf8Strict.GetString($bytes)
    }
    catch {
        $invalidUtf8.Add($file.FullName)
        continue
    }

    $lines = $text -split "`r?`n"
    for ($i = 0; $i -lt $lines.Length; $i++) {
        $line = $lines[$i]
        $hasMojibake = $line.Contains($charReplacement)
        if (-not $hasMojibake) {
            foreach ($token in $suspectTokens) {
                if ($line.Contains($token)) {
                    $hasMojibake = $true
                    break
                }
            }
        }

        if ($hasMojibake) {
            $hits.Add(("{0}:{1}: {2}" -f @($file.FullName, ($i + 1), $line.Trim())))
        }
    }
}

if ($invalidUtf8.Count -gt 0) {
    Write-Host "Archivos no UTF-8 valido:" -ForegroundColor Yellow
    $invalidUtf8 | ForEach-Object { Write-Host "  $_" -ForegroundColor Yellow }
}

if ($hits.Count -gt 0) {
    Write-Host "Posible mojibake detectado:" -ForegroundColor Red
    $hits | Select-Object -First 200 | ForEach-Object { Write-Host "  $_" -ForegroundColor Red }
}
else {
    Write-Host "No se detecto mojibake en los patrones revisados." -ForegroundColor Green
}

if ($FailOnHit -and (($hits.Count -gt 0) -or ($invalidUtf8.Count -gt 0))) {
    exit 1
}

exit 0

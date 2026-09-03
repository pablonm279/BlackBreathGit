param(
    [string]$Root = ".",
    [switch]$FailOnHit
)

$ErrorActionPreference = "Stop"

$projectRoot = (Resolve-Path -LiteralPath $Root).Path
$assetsRoot = Join-Path $projectRoot "Assets"
if (-not (Test-Path -LiteralPath $assetsRoot -PathType Container)) {
    throw "No se encontro la carpeta Assets en: $projectRoot"
}

$exactPaths = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::Ordinal)
$pathsByLowercase = @{}

Get-ChildItem -LiteralPath $assetsRoot -Recurse -File | ForEach-Object {
    if ($_.Extension -eq ".meta") {
        return
    }

    $relativePath = $_.FullName.Substring($assetsRoot.Length).TrimStart('\', '/')
    $segments = $relativePath -split '[\\/]'
    $resourcesIndex = [Array]::IndexOf($segments, "Resources")
    if ($resourcesIndex -lt 0 -or $resourcesIndex -ge ($segments.Length - 1)) {
        return
    }

    $resourcePath = ($segments[($resourcesIndex + 1)..($segments.Length - 1)] -join '/')
    $resourcePath = [System.IO.Path]::ChangeExtension($resourcePath, $null).TrimEnd('.')
    [void]$exactPaths.Add($resourcePath)

    $lowercasePath = $resourcePath.ToLowerInvariant()
    if (-not $pathsByLowercase.ContainsKey($lowercasePath)) {
        $pathsByLowercase[$lowercasePath] = $resourcePath
    }
}

$loadPattern = [regex]'Resources\.(?:Load|LoadAll)(?:<[^>]+>)?\s*\(\s*"([^"]+)"'
$caseMismatches = [System.Collections.Generic.List[object]]::new()
$missingPaths = [System.Collections.Generic.List[object]]::new()

Get-ChildItem -LiteralPath $assetsRoot -Recurse -Filter "*.cs" -File | ForEach-Object {
    $sourceFile = $_
    $lineNumber = 0
    Get-Content -LiteralPath $sourceFile.FullName | ForEach-Object {
        $lineNumber++
        foreach ($match in $loadPattern.Matches($_)) {
            $requestedPath = $match.Groups[1].Value.Replace('\', '/').TrimEnd('/')
            if ($exactPaths.Contains($requestedPath)) {
                continue
            }

            $lowercasePath = $requestedPath.ToLowerInvariant()
            $relativeSource = $sourceFile.FullName.Substring($projectRoot.Length).TrimStart('\', '/')
            $result = [pscustomobject]@{
                Source = $relativeSource
                Line = $lineNumber
                Requested = $requestedPath
                Actual = if ($pathsByLowercase.ContainsKey($lowercasePath)) { $pathsByLowercase[$lowercasePath] } else { $null }
            }

            if ($null -ne $result.Actual) {
                $caseMismatches.Add($result)
            }
            else {
                $missingPaths.Add($result)
            }
        }
    }
}

foreach ($item in $caseMismatches) {
    Write-Host "ERROR case: $($item.Source):$($item.Line): '$($item.Requested)' -> '$($item.Actual)'"
}

foreach ($item in $missingPaths) {
    Write-Host "WARN missing: $($item.Source):$($item.Line): '$($item.Requested)'"
}

Write-Host "Resources.Load literales: $($caseMismatches.Count) errores de mayusculas/minusculas; $($missingPaths.Count) rutas no encontradas."

if ($FailOnHit -and $caseMismatches.Count -gt 0) {
    exit 1
}

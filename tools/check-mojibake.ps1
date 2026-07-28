param(
    [string[]]$Root = @("."),
    [switch]$Staged,
    [switch]$FailOnHit
)

$textExtensions = @(
    ".cs", ".txt", ".md", ".json", ".xml", ".yml", ".yaml",
    ".csv", ".shader", ".cginc", ".hlsl", ".compute", ".uss",
    ".uxml", ".asmdef", ".asmref", ".meta", ".ps1", ".sh"
)

$unityYamlExtensions = @(".asset", ".prefab", ".unity")
$textFileNames = @(
    ".editorconfig", ".gitattributes", ".gitignore", ".gitmodules",
    "pre-commit", "pre-push", "commit-msg"
)
$charReplacement = [char]0xFFFD
$utf8Strict = [System.Text.UTF8Encoding]::new($false, $true)
$utf8 = [System.Text.UTF8Encoding]::new($false)
$windows1252 = [System.Text.Encoding]::GetEncoding(1252)
$hits = [System.Collections.Generic.List[string]]::new()
$invalidUtf8 = [System.Collections.Generic.List[string]]::new()
$suspectTokens = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::Ordinal)

# Genera las secuencias tipicas creadas al interpretar bytes UTF-8 como Windows-1252.
# Esto cubre espanol, portugues, Latin Extended y puntuacion tipografica sin mantener
# una lista manual incompleta.
$codePoints = [System.Collections.Generic.List[int]]::new()
for ($codePoint = 0x00A0; $codePoint -le 0x017F; $codePoint++) {
    $codePoints.Add($codePoint)
}

foreach ($codePoint in @(0x2013, 0x2014, 0x2018, 0x2019, 0x201A, 0x201C, 0x201D, 0x201E, 0x2026, 0x20AC)) {
    $codePoints.Add($codePoint)
}

foreach ($codePoint in $codePoints) {
    $correct = [string][char]$codePoint
    $broken = $windows1252.GetString($utf8.GetBytes($correct))
    if ($broken -cne $correct) {
        $null = $suspectTokens.Add($broken)
    }
}

$patternBuilder = [System.Text.StringBuilder]::new()
$null = $patternBuilder.Append("(?:")
$isFirstToken = $true
foreach ($token in $suspectTokens) {
    if (-not $isFirstToken) {
        $null = $patternBuilder.Append("|")
    }

    $null = $patternBuilder.Append([System.Text.RegularExpressions.Regex]::Escape($token))
    $isFirstToken = $false
}
$null = $patternBuilder.Append("|")
$null = $patternBuilder.Append(
    [System.Text.RegularExpressions.Regex]::Escape([string]$charReplacement)
)
# Caracteres invisibles que no tienen uso valido en el codigo ni en los textos
# del juego y que pueden alterar silenciosamente las claves de traduccion.
$null = $patternBuilder.Append("|\x00|\u00AD|\u200B|\u200C|\u200D|\u2060)")
$suspectRegex = [System.Text.RegularExpressions.Regex]::new(
    $patternBuilder.ToString(),
    ([System.Text.RegularExpressions.RegexOptions]::Compiled `
        -bor [System.Text.RegularExpressions.RegexOptions]::CultureInvariant)
)

function Is-TargetFile([string]$path) {
    $extension = [System.IO.Path]::GetExtension($path).ToLowerInvariant()
    $fileName = [System.IO.Path]::GetFileName($path).ToLowerInvariant()
    return ($textExtensions -contains $extension) `
        -or ($unityYamlExtensions -contains $extension) `
        -or ($textFileNames -contains $fileName)
}

function Is-UnityYamlExtension([string]$path) {
    $extension = [System.IO.Path]::GetExtension($path).ToLowerInvariant()
    return $unityYamlExtensions -contains $extension
}

function Is-TextSerializedUnityAsset([System.IO.FileStream]$stream) {
    if ($stream.Length -eq 0) {
        return $true
    }

    $headerLength = [Math]::Min($stream.Length, 512)
    $headerBytes = [byte[]]::new($headerLength)
    $bytesRead = $stream.Read($headerBytes, 0, $headerLength)
    $stream.Position = 0
    $header = [System.Text.Encoding]::ASCII.GetString($headerBytes, 0, $bytesRead)
    return $header.StartsWith("%YAML", [System.StringComparison]::Ordinal) `
        -or $header.Contains("--- !u!")
}

function Get-TargetFiles([string[]]$rootPaths) {
    $files = [System.Collections.Generic.List[System.IO.FileInfo]]::new()
    $seen = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::OrdinalIgnoreCase)

    foreach ($rootPath in $rootPaths) {
        if (-not (Test-Path -LiteralPath $rootPath)) {
            Write-Warning "No existe la ruta a revisar: $rootPath"
            continue
        }

        if (Test-Path -LiteralPath $rootPath -PathType Leaf) {
            $file = Get-Item -LiteralPath $rootPath
            if ((Is-TargetFile $file.FullName) -and $seen.Add($file.FullName)) {
                $files.Add($file)
            }
            continue
        }

        foreach ($file in Get-ChildItem -LiteralPath $rootPath -Recurse -File -ErrorAction SilentlyContinue) {
            if ((Is-TargetFile $file.FullName) -and $seen.Add($file.FullName)) {
                $files.Add($file)
            }
        }
    }

    return $files
}

function Get-StagedPaths([string]$repoRoot) {
    $startInfo = [System.Diagnostics.ProcessStartInfo]::new()
    $startInfo.FileName = "git"
    $startInfo.Arguments = "-c core.quotepath=false diff --cached --name-only --diff-filter=ACMR -z"
    $startInfo.WorkingDirectory = $repoRoot
    $startInfo.UseShellExecute = $false
    $startInfo.CreateNoWindow = $true
    $startInfo.RedirectStandardOutput = $true
    $startInfo.RedirectStandardError = $true
    $startInfo.StandardOutputEncoding = $utf8
    $startInfo.StandardErrorEncoding = $utf8

    $process = [System.Diagnostics.Process]::Start($startInfo)
    $output = $process.StandardOutput.ReadToEnd()
    $errorOutput = $process.StandardError.ReadToEnd()
    $process.WaitForExit()

    if ($process.ExitCode -ne 0) {
        throw "No se pudo leer el indice de Git: $errorOutput"
    }

    return $output.Split(
        [char[]]@([char]0),
        [System.StringSplitOptions]::RemoveEmptyEntries
    )
}

$stagedExportRoot = $null
try {
    $normalizedRoots = [System.Collections.Generic.List[string]]::new()

    if ($Staged) {
        $repoRoot = (& git rev-parse --show-toplevel 2>$null)
        if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($repoRoot)) {
            throw "No se encontro un repositorio Git para revisar el indice."
        }

        $repoRoot = $repoRoot.Trim()
        $stagedExportRoot = [System.IO.Path]::Combine(
            [System.IO.Path]::GetTempPath(),
            ("encoding-guard-" + [guid]::NewGuid().ToString("N"))
        )
        $null = [System.IO.Directory]::CreateDirectory($stagedExportRoot)
        $exportPrefix = $stagedExportRoot + [System.IO.Path]::DirectorySeparatorChar

        foreach ($stagedPath in (Get-StagedPaths $repoRoot)) {
            if (-not (Is-TargetFile $stagedPath)) {
                continue
            }

            & git -C $repoRoot checkout-index "--prefix=$exportPrefix" -- $stagedPath
            if ($LASTEXITCODE -ne 0) {
                throw "No se pudo exportar el archivo staged: $stagedPath"
            }
        }

        $normalizedRoots.Add($stagedExportRoot)
    }
    else {
        foreach ($rootPath in $Root) {
            foreach ($rootPart in $rootPath.Split(",")) {
                $trimmedRoot = $rootPart.Trim()
                if (-not [string]::IsNullOrWhiteSpace($trimmedRoot)) {
                    $normalizedRoots.Add($trimmedRoot)
                }
            }
        }
    }

    $files = Get-TargetFiles $normalizedRoots

    foreach ($file in $files) {
        $stream = $null
        $reader = $null
        try {
            $stream = [System.IO.File]::Open(
                $file.FullName,
                [System.IO.FileMode]::Open,
                [System.IO.FileAccess]::Read,
                [System.IO.FileShare]::ReadWrite
            )

            # Force Text convierte escenas/prefabs/assets serializados a YAML. Algunos
            # paquetes aun pueden contener .asset binarios; se omiten por cabecera.
            if ((Is-UnityYamlExtension $file.FullName) -and -not (Is-TextSerializedUnityAsset $stream)) {
                continue
            }

            $reader = [System.IO.StreamReader]::new($stream, $utf8Strict, $true)
            $lineIndex = 0
            while (($line = $reader.ReadLine()) -ne $null) {
                $lineIndex++
                if ($suspectRegex.IsMatch($line)) {
                    $hits.Add(("{0}:{1}: {2}" -f @($file.FullName, $lineIndex, $line.Trim())))
                }
            }
        }
        catch {
            if ($_.Exception -is [System.Text.DecoderFallbackException] `
                -or $_.Exception.InnerException -is [System.Text.DecoderFallbackException]) {
                $invalidUtf8.Add($file.FullName)
            }
            else {
                throw
            }
        }
        finally {
            if ($null -ne $reader) {
                $reader.Dispose()
            }
            elseif ($null -ne $stream) {
                $stream.Dispose()
            }
        }
    }
}
finally {
    if ($null -ne $stagedExportRoot `
        -and $stagedExportRoot.StartsWith(
            [System.IO.Path]::GetTempPath(),
            [System.StringComparison]::OrdinalIgnoreCase
        ) `
        -and [System.IO.Directory]::Exists($stagedExportRoot)) {
        [System.IO.Directory]::Delete($stagedExportRoot, $true)
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

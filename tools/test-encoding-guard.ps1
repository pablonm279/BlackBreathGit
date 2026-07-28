param()

$repoRoot = (& git rev-parse --show-toplevel 2>$null)
if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($repoRoot)) {
    throw "No se encontro un repositorio Git."
}

$repoRoot = $repoRoot.Trim()
$guardPath = [System.IO.Path]::Combine($repoRoot, "tools", "check-mojibake.ps1")
$hookPath = [System.IO.Path]::Combine($repoRoot, ".githooks", "pre-commit")
$attributesPath = [System.IO.Path]::Combine($repoRoot, ".gitattributes")
$powerShellPath = [System.Diagnostics.Process]::GetCurrentProcess().MainModule.FileName
$utf8 = [System.Text.UTF8Encoding]::new($false)
$testRoot = [System.IO.Path]::Combine(
    [System.IO.Path]::GetTempPath(),
    ("encoding-guard-test-" + [guid]::NewGuid().ToString("N"))
)
$fixturePath = [System.IO.Path]::Combine($testRoot, "Assets", "EncodingFixture.cs")

function Invoke-Guard([string]$targetPath) {
    & $powerShellPath -NoProfile -ExecutionPolicy Bypass `
        -File $guardPath -Root $targetPath -FailOnHit *> $null
    return $LASTEXITCODE
}

function Remove-TestDirectory([string]$path) {
    $tempRoot = [System.IO.Path]::GetFullPath([System.IO.Path]::GetTempPath())
    $fullPath = [System.IO.Path]::GetFullPath($path)
    if (-not $fullPath.StartsWith($tempRoot, [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "Se rechazo borrar una ruta fuera del directorio temporal: $fullPath"
    }

    if (-not [System.IO.Directory]::Exists($fullPath)) {
        return
    }

    foreach ($file in [System.IO.Directory]::EnumerateFiles(
        $fullPath,
        "*",
        [System.IO.SearchOption]::AllDirectories
    )) {
        [System.IO.File]::SetAttributes($file, [System.IO.FileAttributes]::Normal)
    }

    [System.IO.Directory]::Delete($fullPath, $true)
}

try {
    $null = [System.IO.Directory]::CreateDirectory(
        [System.IO.Path]::Combine($testRoot, ".githooks")
    )
    $null = [System.IO.Directory]::CreateDirectory(
        [System.IO.Path]::Combine($testRoot, "tools")
    )
    $null = [System.IO.Directory]::CreateDirectory(
        [System.IO.Path]::Combine($testRoot, "Assets")
    )

    [System.IO.File]::Copy(
        $guardPath,
        [System.IO.Path]::Combine($testRoot, "tools", "check-mojibake.ps1")
    )
    [System.IO.File]::Copy(
        $hookPath,
        [System.IO.Path]::Combine($testRoot, ".githooks", "pre-commit")
    )
    [System.IO.File]::Copy(
        $attributesPath,
        [System.IO.Path]::Combine($testRoot, ".gitattributes")
    )

    & git -C $testRoot init -q
    if ($LASTEXITCODE -ne 0) {
        throw "No se pudo crear el repositorio temporal."
    }

    & git -C $testRoot config core.hooksPath .githooks
    if ($LASTEXITCODE -ne 0) {
        throw "No se pudo activar el hook en el repositorio temporal."
    }

    # Windows-1252: el byte E9 aislado no es UTF-8 valido.
    [System.IO.File]::WriteAllBytes(
        $fixturePath,
        [byte[]]@(0x2F, 0x2F, 0x20, 0xE9, 0x0A)
    )

    if ((Invoke-Guard $fixturePath) -eq 0) {
        throw "El detector acepto un archivo con UTF-8 invalido."
    }

    $null = (& git -C $testRoot add -- Assets/EncodingFixture.cs 2>&1)
    if ($LASTEXITCODE -ne 0) {
        throw "No se pudo preparar el caso de UTF-8 invalido."
    }

    $null = (& git -C $testRoot hook run pre-commit 2>&1)
    if ($LASTEXITCODE -eq 0) {
        throw "El hook acepto un archivo staged con UTF-8 invalido."
    }

    # Texto valido como UTF-8, pero ya corrompido.
    $mojibake = "// descripci" + [char]0x00C3 + [char]0x00B3 + "n"
    [System.IO.File]::WriteAllText($fixturePath, $mojibake, $utf8)
    $null = (& git -C $testRoot add -- Assets/EncodingFixture.cs 2>&1)
    if ($LASTEXITCODE -ne 0) {
        throw "No se pudo preparar el caso de mojibake."
    }

    $null = (& git -C $testRoot hook run pre-commit 2>&1)
    if ($LASTEXITCODE -eq 0) {
        throw "El hook acepto mojibake valido como UTF-8."
    }

    # Un guion blando es invisible, pero cambia la clave y rompe la traduccion.
    $hiddenCharacter = "// ni" + [char]0x00AD + "vel"
    [System.IO.File]::WriteAllText($fixturePath, $hiddenCharacter, $utf8)
    $null = (& git -C $testRoot add -- Assets/EncodingFixture.cs 2>&1)
    if ($LASTEXITCODE -ne 0) {
        throw "No se pudo preparar el caso de caracter invisible."
    }

    $null = (& git -C $testRoot hook run pre-commit 2>&1)
    if ($LASTEXITCODE -eq 0) {
        throw "El hook acepto un guion blando invisible."
    }

    $correctText = "// descripci" + [char]0x00F3 + "n v" + [char]0x00E1 + "lida"
    [System.IO.File]::WriteAllText($fixturePath, $correctText, $utf8)
    $null = (& git -C $testRoot add -- Assets/EncodingFixture.cs 2>&1)
    if ($LASTEXITCODE -ne 0) {
        throw "No se pudo preparar el caso UTF-8 correcto."
    }

    $hookOutput = (& git -C $testRoot hook run pre-commit 2>&1)
    if ($LASTEXITCODE -ne 0) {
        throw "El hook rechazo texto UTF-8 correcto: $($hookOutput -join [Environment]::NewLine)"
    }

    Write-Host "Encoding guard validado: UTF-8 invalido, mojibake y caracteres invisibles bloqueados; UTF-8 correcto aceptado." `
        -ForegroundColor Green
}
finally {
    Remove-TestDirectory $testRoot
}

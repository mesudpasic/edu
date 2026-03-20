# Run from repo root if attack scripts fail in Docker with "illegal option" or "expecting do"
# (Windows often saves CRLF; Alpine /bin/sh needs LF.)

$Root = Split-Path -Parent $PSScriptRoot
Get-ChildItem (Join-Path $Root "attacks\*.sh") | ForEach-Object {
    $t = [System.IO.File]::ReadAllText($_.FullName) -replace "`r`n", "`n" -replace "`r", "`n"
    [System.IO.File]::WriteAllText($_.FullName, $t.TrimEnd("`n") + "`n", [System.Text.UTF8Encoding]::new($false))
    Write-Host "LF: $($_.Name)"
}

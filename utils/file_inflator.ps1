param(
    [Parameter(Mandatory = $true, Position = 0)]
    [string]$File,

    [Parameter(Mandatory = $true, Position = 1)]
    [long]$Size
)

if (-not (Test-Path -LiteralPath $File)) {
    Write-Error "File not found: $File"
    exit 1
}

$item = Get-Item -LiteralPath $File
if (-not $item.PSIsContainer) {
    try {
        $stream = [System.IO.File]::Open($File, [System.IO.FileMode]::Open, [System.IO.FileAccess]::Write)
        if ($Size -lt $stream.Length) {
            throw "New size ($Size bytes) is smaller than current size ($($stream.Length) bytes)."
        }
        $stream.SetLength($Size)
        $stream.Close()

        Write-Host "Updated: $File"
        Write-Host "New size: $Size bytes"
    }
    catch {
        Write-Error $_.Exception.Message
        exit 1
    }
}
else {
    Write-Error "Path is a folder, not a file: $File"
    exit 1
}
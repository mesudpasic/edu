<#
.SYNOPSIS
    Creates a directory tree with empty files of a specified size.

.DESCRIPTION
    Creates a root directory, nested subdirectories up to the requested depth,
    and an empty file in the root and in each subdirectory.

.PARAMETER RootPath
    Path of the root directory to create.

.PARAMETER Levels
    Number of nested subdirectory levels to create.

.PARAMETER FileSize
    Size of each empty file. Supports PowerShell size suffixes such as KB, MB, GB.

.PARAMETER FileName
    Name of the file created in each directory. Default: empty.bin

.PARAMETER FolderPrefix
    Prefix used for subdirectory names. Default: dir
    Example with Levels=3: dir1\dir2\dir3

.EXAMPLE
    .\Create-NestedEmptyFiles.ps1 -RootPath "C:\Lab\TestTree" -Levels 3 -FileSize 5MB

.EXAMPLE
    .\Create-NestedEmptyFiles.ps1 "C:\Lab\TestTree" 2 1048576
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory = $true, Position = 0, HelpMessage = "Root directory path")]
    [string]$RootPath,

    [Parameter(Mandatory = $true, Position = 1, HelpMessage = "Number of nested subdirectory levels")]
    [ValidateRange(0, 1000)]
    [int]$Levels,

    [Parameter(Mandatory = $true, Position = 2, HelpMessage = "Size of each empty file, e.g. 1024 or 5MB")]
    [string]$FileSize,

    [Parameter(Mandatory = $false)]
    [string]$FileName = "empty.bin",

    [Parameter(Mandatory = $false)]
    [string]$FolderPrefix = "dir"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function ConvertTo-ByteSize {
    param(
        [Parameter(Mandatory = $true)]
        [string]$InputSize
    )

    $normalized = $InputSize.Trim()
    if ($normalized -match '^\d+$') {
        return [int64]$normalized
    }

    if ($normalized -match '^(?<value>\d+(?:\.\d+)?)\s*(?<unit>[KMG]?B)$') {
        $value = [double]$Matches.value
        switch ($Matches.unit.ToUpperInvariant()) {
            "B"  { return [int64]$value }
            "KB" { return [int64]($value * 1KB) }
            "MB" { return [int64]($value * 1MB) }
            "GB" { return [int64]($value * 1GB) }
            default { throw "Unsupported size unit in '$InputSize'." }
        }
    }

    throw "Invalid size value '$InputSize'. Use bytes or suffixes such as 1024, 5MB, 1GB."
}

function New-EmptyFile {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path,

        [Parameter(Mandatory = $true)]
        [long]$Size
    )

    $directory = Split-Path -Path $Path -Parent
    if ($directory -and -not (Test-Path -LiteralPath $directory)) {
        New-Item -ItemType Directory -Path $directory -Force | Out-Null
    }

    $stream = [System.IO.File]::Create($Path)
    try {
        $stream.SetLength($Size)
    }
    finally {
        $stream.Close()
    }
}

if ([string]::IsNullOrWhiteSpace($FileName)) {
    Write-Error "FileName cannot be empty."
    exit 1
}

if ([string]::IsNullOrWhiteSpace($FolderPrefix)) {
    Write-Error "FolderPrefix cannot be empty."
    exit 1
}

$rootPathFull = [System.IO.Path]::GetFullPath($RootPath)
$fileSizeBytes = ConvertTo-ByteSize -InputSize $FileSize

if ($fileSizeBytes -lt 0) {
    Write-Error "File size cannot be negative."
    exit 1
}

New-Item -ItemType Directory -Path $rootPathFull -Force | Out-Null

$createdFiles = New-Object System.Collections.Generic.List[string]
$currentPath = $rootPathFull

$rootFile = Join-Path $currentPath $FileName
New-EmptyFile -Path $rootFile -Size $fileSizeBytes
[void]$createdFiles.Add($rootFile)

for ($level = 1; $level -le $Levels; $level++) {
    $currentPath = Join-Path $currentPath ("{0}{1}" -f $FolderPrefix, $level)
    New-Item -ItemType Directory -Path $currentPath -Force | Out-Null

    $filePath = Join-Path $currentPath $FileName
    New-EmptyFile -Path $filePath -Size $fileSizeBytes
    [void]$createdFiles.Add($filePath)
}

Write-Host "Root directory : $rootPathFull"
Write-Host "Levels created : $Levels"
Write-Host "File size      : $fileSizeBytes bytes"
Write-Host "Files created  : $($createdFiles.Count)"
Write-Host ""

foreach ($file in $createdFiles) {
    Write-Host $file
}

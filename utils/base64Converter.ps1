param(
    [Parameter(Mandatory = $true)]
    [string]$InputFile,

    [Parameter(Mandatory = $true)]
    [string]$OutputFile
)

try {
	# usage: .\base64Converter.ps1 -InputFile "test.exe" -OutputFile "base64.txt"
    # Read binary file
    $bytes = [System.IO.File]::ReadAllBytes($InputFile)

    # Convert to Base64
    $base64String = [System.Convert]::ToBase64String($bytes)

    # Write as UTF-8 text
    [System.IO.File]::WriteAllText(
        $OutputFile,
        $base64String,
        [System.Text.Encoding]::UTF8
    )

    Write-Host "Successfully converted '$InputFile' to Base64 UTF-8 text file '$OutputFile'"
}
catch {
    Write-Error "Error: $_"
}
# Test Wazuh manager API auth (POST /security/user/authenticate). Expects HTTP 200 and a JWT.
# Requires manager up and port 55000 published.

$ErrorActionPreference = "Stop"
$user = "wazuh-wui"
$pass = '#SETEC.doo26#'
$url = "https://127.0.0.1:55000/security/user/authenticate"

$curl = Get-Command curl.exe -ErrorAction SilentlyContinue
if (-not $curl) {
    Write-Host "curl.exe not found."
    exit 1
}
& curl.exe -k -sS -w "`nhttp_code:%{http_code}`n" -u "${user}:${pass}" -X POST $url -H "Content-Type: application/json" -d "{}"
exit $LASTEXITCODE

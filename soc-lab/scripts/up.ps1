$ErrorActionPreference = "Stop"
$Root = Split-Path -Parent $PSScriptRoot
$WazuhCompose = Join-Path (Join-Path (Join-Path $Root ".vendor") "wazuh-docker") "single-node\docker-compose.yml"
$LabCompose = Join-Path $Root "compose.lab.yml"

if (-not (Test-Path $WazuhCompose)) {
    Write-Error "Wazuh vendor not found. Run .\scripts\init-lab.ps1 first."
}

docker compose -p soclab -f $WazuhCompose -f $LabCompose @args

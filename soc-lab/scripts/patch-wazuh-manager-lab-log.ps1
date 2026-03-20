$ErrorActionPreference = "Stop"
$Root = Split-Path -Parent $PSScriptRoot
$Mgr = Join-Path $Root ".vendor\wazuh-docker\single-node\config\wazuh_cluster\wazuh_manager.conf"
if (-not (Test-Path $Mgr)) {
    Write-Error "Missing $Mgr - run .\scripts\init-lab.ps1 first."
}
$body = [System.IO.File]::ReadAllText($Mgr)
if ($body -match 'lab-attack/events\.log') {
    Write-Host "Manager config already includes lab telemetry localfile."
    exit 0
}
$frag = @'

<ossec_config>
  <localfile>
    <log_format>syslog</log_format>
    <location>/var/ossec/logs/lab-attack/events.log</location>
  </localfile>
</ossec_config>
'@
[System.IO.File]::AppendAllText($Mgr, $frag + "`n", [System.Text.UTF8Encoding]::new($false))
Write-Host "Appended lab localfile to wazuh_manager.conf. Restart manager: docker compose restart wazuh.manager"

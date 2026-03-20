# Ensure per-year log subdirs exist so wazuh-analysisd can start (fixes CRITICAL 1107 on new year / fresh volume).
# Run after `docker compose up` if the dashboard shows API 400 / error 1017.

$ErrorActionPreference = "Stop"
$mgr = docker ps --filter "name=wazuh.manager" --format "{{.Names}}" | Select-Object -First 1
if (-not $mgr) {
    Write-Error "No running container matching name wazuh.manager"
}
$year = (Get-Date).Year
$dirs = @(
    "/var/ossec/logs/alerts/$year",
    "/var/ossec/logs/archives/$year",
    "/var/ossec/logs/firewall/$year"
)
foreach ($d in $dirs) {
    docker exec $mgr mkdir -p $d 2>$null
}
docker exec $mgr chown -R wazuh:wazuh /var/ossec/logs/alerts /var/ossec/logs/archives /var/ossec/logs/firewall 2>$null
Write-Host "Created year dirs under /var/ossec/logs for $year. Starting any stopped daemons..."
docker exec $mgr /var/ossec/bin/wazuh-control start

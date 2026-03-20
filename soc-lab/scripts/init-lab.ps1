$ErrorActionPreference = "Stop"
$Root = Split-Path -Parent $PSScriptRoot
$Vendor = Join-Path (Join-Path $Root ".vendor") "wazuh-docker"
$SingleNode = Join-Path $Vendor "single-node"

if (-not (Get-Command git -ErrorAction SilentlyContinue)) {
    Write-Error "Git is required. Install Git for Windows: https://git-scm.com/download/win"
}

if (-not (Test-Path $Vendor)) {
    Write-Host "Cloning wazuh-docker v4.8.2..."
    New-Item -ItemType Directory -Force -Path (Split-Path $Vendor) | Out-Null
    git clone --depth 1 --branch v4.8.2 https://github.com/wazuh/wazuh-docker.git $Vendor
}

Push-Location $SingleNode
try {
    Write-Host "Generating Wazuh indexer TLS certificates (one-time)..."
    docker compose -f generate-indexer-certs.yml run --rm generator
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

    $DashYaml = Join-Path $Root "config\wazuh_dashboard\opensearch_dashboards.yml"
    $DashDst = Join-Path $SingleNode "config\wazuh_dashboard\opensearch_dashboards.yml"
    Copy-Item -Force $DashYaml $DashDst

    $WazuhYml = Join-Path $Root "config\wazuh_dashboard\wazuh.yml"
    $WazuhDst = Join-Path $SingleNode "config\wazuh_dashboard\wazuh.yml"
    Copy-Item -Force $WazuhYml $WazuhDst

    $InternalUsers = Join-Path $Root "config\wazuh_indexer\internal_users.yml"
    $InternalDst = Join-Path $SingleNode "config\wazuh_indexer\internal_users.yml"
    Copy-Item -Force $InternalUsers $InternalDst

    $Dc = Join-Path $SingleNode "docker-compose.yml"
    (Get-Content $Dc) | ForEach-Object {
        if ($_ -match '^\s+-\s+443:5601\s*$') { '      - "5601:5601"' } else { $_ }
    } | Set-Content $Dc

    # Lab passwords (quoted for YAML: # would otherwise start a comment)
    $dcText = [System.IO.File]::ReadAllText($Dc)
    $dcText = $dcText.Replace('      - INDEXER_PASSWORD=SecretPassword', '      - "INDEXER_PASSWORD=#SETEC.doo26#"')
    $dcText = $dcText.Replace('      - API_PASSWORD=MyS3cr37P450r.*-', '      - "API_PASSWORD=#SETEC.doo26#"')
    $dcText = $dcText.Replace('      - DASHBOARD_PASSWORD=kibanaserver', '      - "DASHBOARD_PASSWORD=#SETEC.doo26#"')
    [System.IO.File]::WriteAllText($Dc, $dcText, [System.Text.UTF8Encoding]::new($false))

    $PyPatch = Join-Path $Root "scripts\patch-vendor-dashboard-compose.py"
    $py = $null
    foreach ($name in @("python", "python3")) {
        $c = Get-Command $name -ErrorAction SilentlyContinue
        if ($c) { $py = $c; break }
    }
    if (-not $py) { Write-Error "Python is required for scripts/patch-vendor-dashboard-compose.py" }
    & $py.Name $PyPatch $Dc
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

    $Mgr = Join-Path $SingleNode "config\wazuh_cluster\wazuh_manager.conf"
    $mBody = [System.IO.File]::ReadAllText($Mgr)
    if ($mBody -notmatch 'lab-attack/events\.log') {
        $frag = @'

<ossec_config>
  <localfile>
    <log_format>syslog</log_format>
    <location>/var/ossec/logs/lab-attack/events.log</location>
  </localfile>
</ossec_config>
'@
        [System.IO.File]::AppendAllText($Mgr, $frag + "`n", [System.Text.UTF8Encoding]::new($false))
        Write-Host "Patched wazuh_manager.conf: lab attack telemetry localfile."
    }

    Write-Host "Done. Dashboard: http://127.0.0.1:5601 - run: .\scripts\up.ps1 - restart manager if needed: docker compose restart wazuh.manager"
} finally {
    Pop-Location
}

#!/usr/bin/env bash
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
MGR="$ROOT/.vendor/wazuh-docker/single-node/config/wazuh_cluster/wazuh_manager.conf"
if [[ ! -f "$MGR" ]]; then
  echo "Missing $MGR - run ./scripts/init-lab.sh first." >&2
  exit 1
fi
if grep -q 'lab-attack/events.log' "$MGR" 2>/dev/null; then
  echo "Manager config already includes lab telemetry localfile."
  exit 0
fi
cat >>"$MGR" <<'EOF'

<ossec_config>
  <localfile>
    <log_format>syslog</log_format>
    <location>/var/ossec/logs/lab-attack/events.log</location>
  </localfile>
</ossec_config>
EOF
echo "Appended lab localfile. Restart manager: docker compose restart wazuh.manager"

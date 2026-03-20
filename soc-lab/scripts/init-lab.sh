#!/usr/bin/env bash
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
VENDOR="$ROOT/.vendor/wazuh-docker"
SN="$VENDOR/single-node"

if ! command -v git >/dev/null 2>&1; then
  echo "Git is required." >&2
  exit 1
fi

if [[ ! -d "$VENDOR" ]]; then
  echo "Cloning wazuh-docker v4.8.2..."
  mkdir -p "$(dirname "$VENDOR")"
  git clone --depth 1 --branch v4.8.2 https://github.com/wazuh/wazuh-docker.git "$VENDOR"
fi

echo "Generating Wazuh indexer TLS certificates (one-time)..."
cd "$SN"
docker compose -f generate-indexer-certs.yml run --rm generator

cp "$ROOT/config/wazuh_dashboard/opensearch_dashboards.yml" \
  "$SN/config/wazuh_dashboard/opensearch_dashboards.yml"

cp "$ROOT/config/wazuh_dashboard/wazuh.yml" \
  "$SN/config/wazuh_dashboard/wazuh.yml"

cp "$ROOT/config/wazuh_indexer/internal_users.yml" \
  "$SN/config/wazuh_indexer/internal_users.yml"

DC="$SN/docker-compose.yml"
if grep -q '443:5601' "$DC" 2>/dev/null; then
  TMP="${DC}.tmp.$$"
  cp "$DC" "$TMP"
  sed 's/- 443:5601/- 5601:5601/' "$TMP" >"$DC"
  rm -f "$TMP"
fi

# Lab passwords (quoted for YAML)
sed -i.bak \
  -e 's|      - INDEXER_PASSWORD=SecretPassword|      - "INDEXER_PASSWORD=#SETEC.doo26#"|g' \
  -e 's|      - API_PASSWORD=MyS3cr37P450r.*-|      - "API_PASSWORD=#SETEC.doo26#"|g' \
  -e 's|      - DASHBOARD_PASSWORD=kibanaserver|      - "DASHBOARD_PASSWORD=#SETEC.doo26#"|g' \
  "$DC"
rm -f "${DC}.bak"

if command -v python3 >/dev/null 2>&1; then
  python3 "$ROOT/scripts/patch-vendor-dashboard-compose.py" "$DC"
elif command -v python >/dev/null 2>&1; then
  python "$ROOT/scripts/patch-vendor-dashboard-compose.py" "$DC"
else
  echo "Python 3 is required for scripts/patch-vendor-dashboard-compose.py" >&2
  exit 1
fi

MGR="$SN/config/wazuh_cluster/wazuh_manager.conf"
if ! grep -q 'lab-attack/events.log' "$MGR" 2>/dev/null; then
  cat >>"$MGR" <<'EOF'

<ossec_config>
  <localfile>
    <log_format>syslog</log_format>
    <location>/var/ossec/logs/lab-attack/events.log</location>
  </localfile>
</ossec_config>
EOF
  echo "Patched wazuh_manager.conf: lab attack telemetry localfile."
fi

echo "Done. Dashboard: http://127.0.0.1:5601 - Run: ./scripts/up.sh - restart manager if already running: docker compose restart wazuh.manager"

#!/usr/bin/env bash
# Ensure per-year log subdirs exist so wazuh-analysisd can start (fixes CRITICAL 1107).
set -euo pipefail
mgr="$(docker ps --filter "name=wazuh.manager" --format '{{.Names}}' | head -1)"
if [[ -z "$mgr" ]]; then
  echo "No running container matching wazuh.manager" >&2
  exit 1
fi
year="$(date +%Y)"
for d in "/var/ossec/logs/alerts/$year" "/var/ossec/logs/archives/$year" "/var/ossec/logs/firewall/$year"; do
  docker exec "$mgr" mkdir -p "$d" || true
done
docker exec "$mgr" chown -R wazuh:wazuh /var/ossec/logs/alerts /var/ossec/logs/archives /var/ossec/logs/firewall || true
echo "Starting any stopped daemons in $mgr..."
docker exec "$mgr" /var/ossec/bin/wazuh-control start

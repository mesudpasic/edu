#!/usr/bin/env bash
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
WAZUH="$ROOT/.vendor/wazuh-docker/single-node/docker-compose.yml"
LAB="$ROOT/compose.lab.yml"

if [[ ! -f "$WAZUH" ]]; then
  echo "Run ./scripts/init-lab.sh first." >&2
  exit 1
fi

docker compose -p soclab -f "$WAZUH" -f "$LAB" "$@"

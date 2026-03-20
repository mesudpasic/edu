#!/bin/sh
# Generates HTTP hits against Gophish phishing listener for web/access style noise.
apk add --no-cache curl >/dev/null || exit 1
. /lab-telemetry.sh
echo "[phish-traffic] waiting for gophish..."
sleep 35
while true; do
  for path in / /login /track /rp; do
    curl -sS -m 5 -A 'Mozilla/5.0 LabPhishBot' "http://gophish:80${path}" -o /dev/null || true
  done
  lab_ssh_brute_event LAB_PHISH_HTTP 30 3100
  sleep 45
done

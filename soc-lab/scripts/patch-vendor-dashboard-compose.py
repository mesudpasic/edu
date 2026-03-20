"""Apply lab fixes to vendor single-node docker-compose.yml (idempotent)."""
from __future__ import annotations

import pathlib
import sys


def main() -> int:
    if len(sys.argv) != 2:
        print("usage: patch-vendor-dashboard-compose.py <path/to/docker-compose.yml>", file=sys.stderr)
        return 2
    path = pathlib.Path(sys.argv[1])
    text = path.read_text(encoding="utf-8")
    text = text.replace("\r\n", "\n")
    orig = text

    vol_old = """      - ./config/wazuh_dashboard/opensearch_dashboards.yml:/usr/share/wazuh-dashboard/config/opensearch_dashboards.yml
      - ./config/wazuh_dashboard/wazuh.yml:/usr/share/wazuh-dashboard/data/wazuh/config/wazuh.yml
      - wazuh-dashboard-config:/usr/share/wazuh-dashboard/data/wazuh/config"""
    vol_new = """      - ./config/wazuh_dashboard/opensearch_dashboards.yml:/usr/share/wazuh-dashboard/config/opensearch_dashboards.yml
      - wazuh-dashboard-config:/usr/share/wazuh-dashboard/data/wazuh/config
      - ./config/wazuh_dashboard/wazuh.yml:/usr/share/wazuh-dashboard/data/wazuh/config/wazuh.yml"""
    if vol_old in text:
        text = text.replace(vol_old, vol_new)
        print("Patched wazuh.dashboard volume order (wazuh.yml overlay).")

    tls_old = """      - "API_PASSWORD=#SETEC.doo26#"
    volumes:
      - ./config/wazuh_indexer_ssl_certs/wazuh.dashboard.pem:"""
    tls_new = """      - "API_PASSWORD=#SETEC.doo26#"
      - NODE_TLS_REJECT_UNAUTHORIZED=0
    volumes:
      - ./config/wazuh_indexer_ssl_certs/wazuh.dashboard.pem:"""
    if "NODE_TLS_REJECT_UNAUTHORIZED" not in text and tls_old in text:
        text = text.replace(tls_old, tls_new)
        print("Patched wazuh.dashboard NODE_TLS_REJECT_UNAUTHORIZED=0.")

    dep_old = """    depends_on:
      - wazuh.indexer
    links:
      - wazuh.indexer:wazuh.indexer
      - wazuh.manager:wazuh.manager"""
    dep_new = """    depends_on:
      - wazuh.indexer
      - wazuh.manager
    links:
      - wazuh.indexer:wazuh.indexer
      - wazuh.manager:wazuh.manager"""
    if dep_old in text:
        text = text.replace(dep_old, dep_new)
        print("Patched wazuh.dashboard depends_on: wazuh.manager.")

    if text != orig:
        path.write_text(text, encoding="utf-8", newline="\n")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())

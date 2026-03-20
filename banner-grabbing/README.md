# Banner grabbing
## Author: Mesud Pasic (Setec d.o.o. & Tranchulas d.o.o.)
## Contact: office@setec.ba

Python tool for banner grabbing across a large set of common TCP ports. It can scan a single host or an entire IPv4 range, optionally routing connections through SOCKS4 proxies fetched from a public list. Results (open ports with banners) are written as JSON files; WHOIS and HTTP(S) header enrichment are included for relevant services.

## Requirements

- Python 3.x  
- Dependencies: see [`requirements.txt`](requirements.txt)

```bash
pip install -r requirements.txt
```

Copy `.env.example` to `.env` and set the variables you need (see [Configuration](#configuration)).

## Command-line arguments

The CLI is defined in `main.py` with `argparse`:

```python
parser = argparse.ArgumentParser()
parser.add_argument(
    "-ip",
    "--ip",
    required=False,
    type=str,
    help="The ip or hostname which is subject to scan",
)
parser.add_argument(
    "-p",
    "--proxy",
    action="store_true",
    help="Boolean value which indicates if the programme should use a proxy for scanning, if excluded the programme WILL NOT use a proxy.",
)
```

| Short | Long   | Type   | Required | Effect |
|-------|--------|--------|----------|--------|
| `-ip` | `--ip` | string | No       | Target hostname or IP to scan. If omitted, the tool scans the IPv4 network defined by `START_IP` in `.env` (see below). |
| `-p`  | `--proxy` | flag (store_true) | No | When present, connections use a random SOCKS4 proxy from the list loaded at startup (see [Proxies](#proxies)). When omitted, no proxy is used. |

### Usage examples

```bash
# Scan one host (resolves hostname if needed)
python main.py --ip example.com

# Same with short form
python main.py -ip 192.0.2.10

# Scan every address in the CIDR from .env (no --ip)
python main.py

# Use SOCKS4 proxies for the scan
python main.py --ip 192.0.2.10 --proxy
```

## Configuration

Environment variables are loaded from `.env` in the project root (see [`utils/config.py`](utils/config.py)). Example template:

```env
START_IP=
SOCKET_TIMEOUT=
WORKER_THREADS=
RESULT_DIR=
```

| Variable         | Used in | Purpose |
|------------------|---------|---------|
| `START_IP`       | [`Scanner`](utils/scanner_utils.py) | CIDR notation for the IPv4 network to scan when `--ip` is **not** passed (e.g. `192.0.2.0/24`). |
| `SOCKET_TIMEOUT` | `Scanner` | Socket and HTTP request timeout in seconds. Default: `3` if unset. |
| `WORKER_THREADS` | `Scanner` | Thread pool size when scanning the full range (`scan_range`). Default: `5` if unset. |
| `RESULT_DIR`     | `Scanner.save_result` | Directory where `{ip}.json` files are written for hosts with at least one open port with a banner. |

## Behavior overview

- **Ports**: All keys in [`COMMON_PORTS`](utils/common_ports.py) are probed per target (large dictionary of port → service name).
- **Banners**: Raw TCP receive; HTTP ports get a minimal `HEAD` probe; port 443 is reported as TLS without decrypting; ports 80/443 can append HTTP(S) header lines to the saved banner string.
- **Output**: JSON per IP under `RESULT_DIR`, e.g. `{"ip": "...", "ports": { ... }}`.
- **WHOIS**: Logged to the console after the port scan (not stored in the JSON in the current code).

## Proxies

With `--proxy`, [`ProxyUtil`](utils/proxy_utils.py) downloads a SOCKS proxy table from `https://free-proxy-list.net/en/socks-proxy.html` and each connection picks a random `(host, port)` pair for SOCKS4. Reliability depends on third-party proxies; use only where appropriate and legal.

## Example JSON result
```
{
    "ip": "35.209.198.222",
    "ports": {
        "22": "SSH-2.0-OpenSSH_8.2p1 Ubuntu-4ubuntu0.13"
    }
}
```

## License / ethics

Use only on networks and systems you are authorized to test. Banner grabbing can be intrusive; comply with applicable laws and policies.

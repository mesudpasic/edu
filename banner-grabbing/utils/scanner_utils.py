import ipaddress
from datetime import datetime
import os
import socks
import random
import socket
import requests
import whois
import threading
import json
from utils.proxy_utils import ProxyUtil
from concurrent.futures import ThreadPoolExecutor
from utils.common_ports import COMMON_PORTS
from utils.config import get_env
from utils.logger import logger


class Scanner:
    start_ip = get_env("START_IP")
    SOCKET_TIMEOUT = int(get_env("SOCKET_TIMEOUT", 3))
    # Sweet spot for number of worker threads is around 20.
    # Takes value of "WORKER_THREADS" variable in .env, if
    # variable not present it defaults to 5
    WORKER_THREADS = int(get_env("WORKER_THREADS", 5))
    network = ipaddress.IPv4Network(start_ip)
    use_proxy: bool
    proxies = []
    all_ips = [str(ip) for ip in network]

    def __init__(self, use_proxy=False):
        self.use_proxy = use_proxy
        if use_proxy:
            self.proxies = ProxyUtil.get_proxy_list()

    # -----------------------------
    # Helper functions
    # -----------------------------
    def is_valid_ip(self, ip):
        try:
            ipaddress.ip_address(ip)
            return True
        except ValueError:
            return False

    def resolve_ip(self, target):
        try:
            return socket.gethostbyname(target)
        except socket.gaierror as e:
            logger.error(e)
            return None

    def reverse_dns(self, ip):
        try:
            return socket.gethostbyaddr(ip)[0]
        except socket.herror as e:
            logger.error(e)
            return None

    def grab_banner(self, ip, port):
        try:
            sock = socks.socksocket()
            sock.settimeout(self.SOCKET_TIMEOUT)
            if self.use_proxy:
                random_proxy = random.choice(self.proxies)
                logger.info(f"Using proxy ==> {random_proxy[0]}:{random_proxy[1]}")
                sock.set_proxy(socks.SOCKS4, random_proxy[0], int(random_proxy[1]))

            sock.connect((ip, port))
            # Send minimal probe for text-based services
            if port in (80, 8080):
                sock.sendall(b"HEAD / HTTP/1.1\r\nHost: %b\r\n\r\n" % ip.encode())
            elif port == 443:
                sock.close()
                return "TLS service (encrypted)"

            banner = sock.recv(4096)
            sock.close()
            return banner.decode(errors="ignore").strip()

        except Exception as e:
            # Line 83 commented because of ridiculous amounts of output
            # Every port that does not "answer" is logged with a "TIMEOUT"
            # logger.error(e)
            return None

    def http_headers(self, ip, https=False):
        try:
            scheme = "https" if https else "http"
            url = f"{scheme}://{ip}"
            r = requests.head(url, timeout=self.SOCKET_TIMEOUT, allow_redirects=True)
            return dict(r.headers)
        except Exception as e:
            logger.error(e)
            return None

    def whois_lookup(self, ip):
        try:
            return whois.whois(ip)
        except Exception as e:
            logger.error(e)
            return None

    def save_result(self, data):
        try:
            json_str = json.dumps(data, indent=4)
            ip = data["ip"]
            result_dir = get_env("RESULT_DIR")
            os.makedirs(result_dir, exist_ok=True)
            with open(f"{result_dir}/{ip}.json", "w") as f:
                f.write(json_str)
        except Exception as e:
            logger.error(e)

    # -----------------------------
    # Function which will be used inside the thread pool,
    # each function call will be ran on a different thread.
    # Parameters are as follows:
    #   - ip :: the ip which is the subject of scan
    #   - port :: the port which is subject of scan for the given IP
    #   - ports :: dictionary which will be later incorporated into "data" dictionary which is dumped to JSON
    # -----------------------------
    def grab_banner_and_log_entry(self, ip, port, ports):
        try:
            banner = self.grab_banner(ip, port)
            if banner:
                logger.info(f"\n--- {ip}: {COMMON_PORTS[port]} ({port}) ---")
                logger.info(banner)
                ports[port] = banner

                if port == 80:
                    headers = self.http_headers(ip)
                    if headers:
                        logger.info("HTTP Headers:")
                        for k, v in headers.items():
                            logger.info(f"  {k}: {v}")
                            ports[port] = f"{ports[port]} | {k}: {v}"

                if port == 443:
                    headers = self.http_headers(ip, https=True)
                    if headers:
                        logger.info("HTTPS Headers:")
                        for k, v in headers.items():
                            logger.info(f"  {k}: {v}")
                            ports[port] = f"{ports[port]} | {k}: {v}"
        except Exception as e:
            logger.error(e)

    # -----------------------------
    # Main scan logic
    # -----------------------------
    def scan_target(self, target):
        try:
            logger.info(f"\n=== Banner Scan Started ===")
            logger.info(f"Target: {target}")
            logger.info(f"Time: {datetime.utcnow()} UTC")

            ip = self.resolve_ip(target)
            if not ip or not self.is_valid_ip(ip):
                logger.error("❌ Unable to resolve target")
                return

            logger.info(f"\n[+] IP Address: {ip}")

            rdns = self.reverse_dns(ip)
            logger.info(f"[+] Reverse DNS({ip}): {rdns or 'Not found'}")

            logger.info("\n[+] Port Scan & Banner Grabbing")
            # -----------------------------
            # Dictionary which will be used as "ports" dictionary within the JSON for the given IP address
            # -----------------------------
            ports = {}
            threads = []

            for port in COMMON_PORTS.keys():
                t = threading.Thread(
                    target=self.grab_banner_and_log_entry, args=(ip, port, ports)
                )
                threads.append(t)

            for t in threads:
                t.start()

            for t in threads:
                t.join()

            # -----------------------------
            # If ports are opened on a given IP address a JSON file must be created
            # -----------------------------
            if len(ports) > 0:
                data = {"ip": ip, "ports": ports}
                self.save_result(data)

            logger.info(f"\n[+] WHOIS Information {ip}")
            w = self.whois_lookup(ip)
            if w:
                for key in ["org", "country", "asn", "netname", "address"]:
                    value = getattr(w, key, None)
                    if value:
                        logger.info(f"{key.upper()}: {value}")
            else:
                logger.info(f"WHOIS lookup {ip} failed")

            logger.info(f"\n=== Scan Complete for {ip} ===")
        except Exception as e:
            logger.error(e)

    # -----------------------------
    # Scans range/single IP generated from provided START_IP variable in .env file
    # -----------------------------
    def scan_range(self):
        try:
            with ThreadPoolExecutor(max_workers=self.WORKER_THREADS) as e:
                e.map(self.scan_target, self.all_ips)
        except Exception as e:
            logger.error(e)

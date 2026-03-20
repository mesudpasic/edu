from utils.scanner_utils import Scanner
import argparse

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

# -----------------------------
# Entry point
# -----------------------------

if __name__ == "__main__":
    args = parser.parse_args()
    scanner = Scanner(args.proxy)
    # If the ip in args is None it will scan the ip or network
    # provided in the .env file. Otherwise, scan provided host/ip.
    if args.ip is None:
        scanner.scan_range()
    else:
        scanner.scan_target(args.ip)

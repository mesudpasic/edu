import requests
from bs4 import BeautifulSoup
from utils.logger import logger

PROXY_URL = "https://free-proxy-list.net/en/socks-proxy.html"


class ProxyUtil:
    @staticmethod
    def get_proxy_list():
        try:
            response = requests.get(PROXY_URL)
            soup = BeautifulSoup(response.text, "html.parser")
            table = soup.find("table")
            proxy_list = []
            for row in table.find_all("tr")[1:]:
                columns = row.find_all("td")
                proxy_list.append((columns[0].text, columns[1].text))
            return proxy_list
        except Exception as e:
            logger.error(e)
            return []

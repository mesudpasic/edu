import logging


def get_logger():
    logging.basicConfig(level=logging.INFO, format="[%(levelname)s] %(message)s")
    logger = logging.getLogger()
    return logger


logger = get_logger()

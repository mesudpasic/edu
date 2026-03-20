import os
from pathlib import Path
from dotenv import load_dotenv

# This was necessary to avoid error of missing utils module
_DOTENV_PATH = Path(__file__).resolve().parent.parent / ".env"
load_dotenv(dotenv_path=_DOTENV_PATH, override=False)


def get_env(key: str, default_value: str = "") -> str:
    return os.getenv(key, default_value)

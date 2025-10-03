import os

OPENROUTER_API_KEY = os.getenv("OPENROUTER_API_KEY")
HUGGINGFACE_API_KEY = os.getenv("HUGGINGFACE_API_KEY")

# Models
OPENROUTER_MODEL = os.getenv("OPENROUTER_MODEL", "openai/gpt-oss-20b:free")
HF_MODEL = os.getenv("HF_MODEL", "mistralai/Mistral-7B-Instruct-v0.2")

# Networking
HTTP_TIMEOUT = float(os.getenv("HTTP_TIMEOUT", "60"))

USE_EXTERNAL_LLM = os.getenv("USE_EXTERNAL_LLM", "true").lower() == "true"

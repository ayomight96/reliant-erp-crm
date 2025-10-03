from __future__ import annotations
import json
import httpx
from typing import Dict, List
from .config import (
    OPENROUTER_API_KEY,
    OPENROUTER_MODEL,
    HUGGINGFACE_API_KEY,
    HF_MODEL,
    HTTP_TIMEOUT,
)


async def call_openrouter(messages: List[Dict[str, str]], max_tokens: int = 400) -> str:
    if not OPENROUTER_API_KEY:
        raise RuntimeError("Missing OPENROUTER_API_KEY")
    headers = {
        "Authorization": f"Bearer {OPENROUTER_API_KEY}",
        "Content-Type": "application/json",
        "HTTP-Referer": "https://reliant.local",
        "X-Title": "Reliant CRM",
    }
    body = {
        "model": OPENROUTER_MODEL,
        "temperature": 0.2,
        "max_tokens": max_tokens,
        "response_format": {"type": "json_object"},
        "messages": messages,
    }
    to = httpx.Timeout(
        HTTP_TIMEOUT,
        connect=min(5.0, HTTP_TIMEOUT),
        read=HTTP_TIMEOUT,
        write=HTTP_TIMEOUT,
    )
    async with httpx.AsyncClient(timeout=to) as client:
        r = await client.post(
            "https://openrouter.ai/api/v1/chat/completions", headers=headers, json=body
        )
        r.raise_for_status()
        return r.json()["choices"][0]["message"]["content"]


async def call_hf(messages: List[Dict[str, str]], max_new_tokens: int = 400) -> str:
    if not HUGGINGFACE_API_KEY:
        raise RuntimeError("Missing HUGGINGFACE_API_KEY")
    prompt = ""
    for m in messages:
        role, content = m["role"], m["content"]
        prompt += f"[{role.upper()}] {content}\n"
    prompt += '\n[ASSISTANT] Return strictly JSON: {"text": string}.'
    headers = {"Authorization": f"Bearer {HUGGINGFACE_API_KEY}"}
    payload = {
        "inputs": prompt,
        "parameters": {
            "max_new_tokens": max_new_tokens,
            "temperature": 0.2,
            "return_full_text": False,
        },
    }
    to = httpx.Timeout(
        HTTP_TIMEOUT,
        connect=min(5.0, HTTP_TIMEOUT),
        read=HTTP_TIMEOUT,
        write=HTTP_TIMEOUT,
    )
    async with httpx.AsyncClient(timeout=to) as client:
        r = await client.post(
            f"https://api-inference.huggingface.co/models/{HF_MODEL}",
            headers=headers,
            json=payload,
        )
        r.raise_for_status()
        out = r.json()
        if isinstance(out, list) and out and "generated_text" in out[0]:
            return out[0]["generated_text"]
        if isinstance(out, dict) and "generated_text" in out:
            return out["generated_text"]
        return json.dumps({"text": str(out)})

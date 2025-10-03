from __future__ import annotations

import asyncio
import json
import logging
from typing import Iterable, List

import pandas as pd
from fastapi import APIRouter

from .facts import gather_facts
from .llm import call_hf, call_openrouter
from .models import (
    PredictBatchRequest,
    PredictBatchResponse,
    PredictItemOut,
    QuoteSummaryRequest,
    QuoteSummaryResponse,
    _norm_glazing,
    _norm_material,
    _norm_product_type,
)
from .pricing import model

# --- config flags (optional env switch) ---
try:
    from .config import USE_EXTERNAL_LLM  # bool if you added it to config.py
except Exception:  # default to True if not present
    USE_EXTERNAL_LLM = True

# --- timeouts ---
PROVIDER_DEADLINE_S = 6.0  # per provider hard ceiling
ROUTE_DEADLINE_S = 8.0  # overall ceiling (race + parse)

logger = logging.getLogger(__name__)
router = APIRouter()


# -------------------------- utils --------------------------


def _predict_row(it) -> PredictItemOut:
    """Predict one item and return API payload."""
    pt = _norm_product_type(it.product_type)
    mat = _norm_material(it.material)
    glz = _norm_glazing(it.glazing)
    area = (it.width_mm * it.height_mm) / 1_000_000.0
    X = dict(
        product_type=pt,
        material=mat,
        glazing=glz,
        color_tier=it.color_tier,
        hardware_tier=it.hardware_tier,
        install_complexity=it.install_complexity,
        area=area,
        qty=it.qty,
    )
    pred = float(model.predict(pd.DataFrame([X]))[0])
    return PredictItemOut(
        unit_price=round(max(80.0, pred), 2),
        confidence=0.85,
        features={"productId": it.product_id, **X},
    )


def _make_prompt(req: QuoteSummaryRequest) -> tuple[str, str]:
    """Build (system, user) prompts from request including in-RAG facts."""
    name = req.customer_name or "the customer"
    # RAG lines
    lines: List[str] = []
    for it in req.items:
        pt = _norm_product_type(it.product_type)
        area = (it.width_mm * it.height_mm) / 1_000_000.0
        lines.append(
            " ".join(
                [
                    f"- product_id={it.product_id or 'n/a'}",
                    f"type={pt}",
                    f"size={it.width_mm}x{it.height_mm}mm ({area:.2f} m²)",
                    f"material={_norm_material(it.material)}",
                    f"glazing={_norm_glazing(it.glazing)}",
                    f"color={it.color_tier or '—'}",
                    f"hardware={it.hardware_tier or '—'}",
                    f"install={it.install_complexity or '—'}",
                    f"qty={it.qty}",
                ]
            )
        )
    rag = "\n".join(lines)

    # dedup cross-item facts
    seen, facts_block = set(), []
    for it in req.items:
        for s in gather_facts(
            _norm_product_type(it.product_type),
            _norm_material(it.material),
            _norm_glazing(it.glazing),
        ):
            if s not in seen:
                seen.add(s)
                facts_block.append(s)
    facts_text = "\n".join(f"- {s}" for s in facts_block)

    system = (
        "You are a sales assistant for a UK windows & doors company (Reliant). "
        "Write a concise, factual quotation summary in UK English. Mention quantities, "
        "dimensions (mm), materials, glazing. Avoid prices and hype. "
        "End with a short VAT & standards note. "
        'Return strictly JSON: {"text": string}.'
    )
    user = (
        f"Customer: {name}\n"
        f"Items:\n{rag}\n\n"
        + (f"Relevant facts:\n{facts_text}\n\n" if facts_text else "")
        + f"VAT rate: {int(req.vat_rate*100)}%"
    )
    return system, user


def _parse_json_text(content: str) -> str:
    """Accept either JSON {text: ...} or raw text from a model."""
    try:
        data = json.loads(content)
        text = data.get("text", "")
        return text.strip()
    except Exception:
        return content.strip()


def _det_summary_line(i: int, it) -> str:
    """One line of the deterministic summary."""
    pt = _norm_product_type(it.product_type)
    area = (it.width_mm * it.height_mm) / 1_000_000.0
    parts = [
        f"{i}) {it.qty}× {pt} {_norm_material(it.material)} {_norm_glazing(it.glazing)}",
        f"({it.width_mm}×{it.height_mm}mm ~ {area:.2f} m²)",
    ]
    if it.color_tier:
        parts.append(f", colour {it.color_tier}")
    if it.hardware_tier:
        parts.append(f", hardware {it.hardware_tier}")
    if it.install_complexity:
        parts.append(f", install {it.install_complexity}")
    return " ".join(parts)


async def _race_first_success(coros: Iterable[asyncio.Future]) -> str | None:
    """
    Run multiple coroutines concurrently, return the first non-empty parsed
    text or None if all fail/time out. Cancels the rest upon first success.
    """
    tasks = [asyncio.create_task(c) for c in coros]
    try:
        for task in asyncio.as_completed(tasks, timeout=ROUTE_DEADLINE_S):
            try:
                content = await task
                text = _parse_json_text(content)
                if text:
                    # Cancel remaining tasks
                    for t in tasks:
                        if t is not task and not t.done():
                            t.cancel()
                    return text
            except Exception as e:
                logger.warning("LLM branch failed: %s", e)
        return None
    finally:
        for t in tasks:
            if not t.done():
                t.cancel()


# -------------------------- routes --------------------------


@router.get("/health")
def health():
    return {"status": "ok", "service": "ai"}


@router.post("/predict-quote/batch", response_model=PredictBatchResponse)
def predict_batch(req: PredictBatchRequest):
    items = [_predict_row(it) for it in req.items]
    return PredictBatchResponse(items=items)


@router.post("/summarize-quote", response_model=QuoteSummaryResponse)
async def summarize(req: QuoteSummaryRequest):
    name = req.customer_name or "the customer"

    # Build prompts / RAG once
    system, user = _make_prompt(req)

    # Optionally skip external calls (offline/CI)
    if USE_EXTERNAL_LLM is False:
        logger.info("Summarize: external LLMs disabled; using deterministic fallback.")
        lines = [_det_summary_line(i, it) for i, it in enumerate(req.items, start=1)]
        text = (
            f"Quotation for {name}:\n"
            + "\n".join(lines)
            + f"\nPrices exclude VAT; VAT rate {int(req.vat_rate*100)}% applied on invoice. "
            "All installations FENSA-compliant; A+ energy-rated options available."
        )
        return QuoteSummaryResponse(text=text)

    # Build the two provider calls with per-branch ceilings
    msgs = [{"role": "system", "content": system}, {"role": "user", "content": user}]
    coros: list[asyncio.Future] = [
        asyncio.wait_for(
            call_openrouter(msgs, max_tokens=400), timeout=PROVIDER_DEADLINE_S
        ),
        asyncio.wait_for(
            call_hf(msgs, max_new_tokens=400), timeout=PROVIDER_DEADLINE_S
        ),
    ]

    # Race; first valid text wins
    try:
        winner = await _race_first_success(coros)
        if winner:
            return QuoteSummaryResponse(text=winner)
    except asyncio.TimeoutError:
        logger.warning(
            "Summarize: overall timeout (%.1fs). Falling back.", ROUTE_DEADLINE_S
        )
    except Exception as e:
        logger.exception("Summarize: unexpected error: %s", e)

    # Deterministic fallback
    lines = [_det_summary_line(i, it) for i, it in enumerate(req.items, start=1)]
    text = (
        f"Quotation for {name}:\n"
        + "\n".join(lines)
        + f"\nPrices exclude VAT; VAT rate {int(req.vat_rate*100)}% applied on invoice. "
        "All installations FENSA-compliant; A+ energy-rated options available."
    )
    return QuoteSummaryResponse(text=text)

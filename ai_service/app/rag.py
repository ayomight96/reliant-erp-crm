from __future__ import annotations
from typing import Tuple
from .models import (
    QuoteSummaryRequest,
    _norm_product_type,
    _norm_material,
    _norm_glazing,
)


def build_rag_context(req: QuoteSummaryRequest) -> Tuple[str, int]:
    lines, n_tokens = [], 0
    for it in req.items:
        pt = _norm_product_type(it.product_type)
        area = (it.width_mm * it.height_mm) / 1_000_000.0
        ln = (
            f"- product_id={it.product_id or 'n/a'} "
            f"type={pt} size={it.width_mm}x{it.height_mm}mm ({area:.2f} m²) "
            f"material={_norm_material(it.material)} glazing={_norm_glazing(it.glazing)} "
            f"color={it.color_tier or '—'} hardware={it.hardware_tier or '—'} "
            f"install={it.install_complexity or '—'} qty={it.qty}"
        )
        lines.append(ln)
        n_tokens += len(ln.split())
    return "\n".join(lines), n_tokens

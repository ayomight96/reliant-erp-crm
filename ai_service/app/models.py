from __future__ import annotations
from typing import Any, Dict, List, Optional
from pydantic import BaseModel, Field, ConfigDict, model_validator

# ----- Normalisers -----
VALID_PRODUCT_TYPES = {"window", "door", "conservatory"}

def _norm_product_type(x: Optional[str]) -> str:
    if not x:
        return "window"
    t = x.strip().lower()
    if t in VALID_PRODUCT_TYPES:
        return t
    if any(k in t for k in ("bifold", "french", "patio", "door")):
        return "door"
    if "conserv" in t or "orangery" in t:
        return "conservatory"
    return "window"

def _norm_material(x: Optional[str]) -> str:
    s = (x or "").strip()
    if s.lower() == "upvc":
        return "uPVC"
    if s.lower() == "aluminium":
        return "Aluminium"
    if s.lower() == "composite":
        return "Composite"
    return "uPVC"

def _norm_glazing(x: Optional[str]) -> str:
    s = (x or "").strip().lower()
    if s in {"double", "triple"}:
        return s
    return "double"

# ----- IO models -----
class QuoteItemIn(BaseModel):
    product_type: Optional[str] = None
    product_id: Optional[int] = None
    width_mm: int = Field(ge=300, le=4000)
    height_mm: int = Field(ge=300, le=4000)
    material: str
    glazing: str
    color_tier: Optional[str] = None
    hardware_tier: Optional[str] = None
    install_complexity: Optional[str] = None
    qty: int = Field(ge=1)

    model_config = ConfigDict(extra="ignore", str_strip_whitespace=True)

    @model_validator(mode="before")
    @classmethod
    def _accept_camel(cls, data: Any) -> Any:
        if not isinstance(data, dict):
            return data
        mapping = {
            "productType": "product_type",
            "productId": "product_id",
            "widthMm": "width_mm",
            "heightMm": "height_mm",
            "colorTier": "color_tier",
            "hardwareTier": "hardware_tier",
            "installComplexity": "install_complexity",
        }
        for camel, snake in mapping.items():
            if camel in data and snake not in data:
                data[snake] = data[camel]
        return data

class PredictBatchRequest(BaseModel):
    items: List[QuoteItemIn]

class PredictItemOut(BaseModel):
    unit_price: float
    confidence: float
    features: Dict[str, Any]

class PredictBatchResponse(BaseModel):
    items: List[PredictItemOut]

class QuoteSummaryRequest(BaseModel):
    customer_name: Optional[str] = None
    items: List[QuoteItemIn]
    vat_rate: float = 0.20

    model_config = ConfigDict(extra="ignore", str_strip_whitespace=True)

    @model_validator(mode="before")
    @classmethod
    def _accept_camel(cls, data: Any) -> Any:
        if not isinstance(data, dict):
            return data
        if "customerName" in data and "customer_name" not in data:
            data["customer_name"] = data["customerName"]
        if "vatRate" in data and "vat_rate" not in data:
            data["vat_rate"] = data["vatRate"]
        return data

class QuoteSummaryResponse(BaseModel):
    text: str

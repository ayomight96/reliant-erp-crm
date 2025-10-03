from __future__ import annotations
import numpy as np, pandas as pd
from sklearn.compose import ColumnTransformer
from sklearn.preprocessing import OneHotEncoder
from sklearn.linear_model import LinearRegression
from sklearn.pipeline import Pipeline

def _training(n=5000, seed=42) -> pd.DataFrame:
    rng = np.random.default_rng(seed)
    product_types = ["window", "door", "conservatory"]
    materials = ["uPVC", "Aluminium", "Composite"]
    glazings = ["double", "triple"]
    color_tiers = [None, "Standard", "Premium"]
    hardware_tiers = [None, "Standard", "Premium"]
    install_lvls = [None, "Standard", "Complex"]
    rows = []
    for _ in range(n):
        pt = rng.choice(product_types)
        mat = rng.choice(materials)
        glz = rng.choice(glazings)
        w = int(rng.integers(600, 2400))
        h = int(rng.integers(600, 2400))
        qty = int(rng.integers(1, 5))
        col = rng.choice(color_tiers)
        hw = rng.choice(hardware_tiers)
        ins = rng.choice(install_lvls)
        area = (w * h) / 1_000_000.0
        base = {"window": 260, "door": 850, "conservatory": 400}[pt]
        area_coeff = 0.5 if pt != "conservatory" else 2.0
        mat_mult = {"uPVC": 1.0, "Aluminium": 1.30, "Composite": 1.15}[mat]
        glz_mult = {"double": 1.00, "triple": 1.18}[glz]
        col_mult = {"Standard": 1.00, "Premium": 1.10, None: 1.00}[col]
        hw_mult = {"Standard": 1.00, "Premium": 1.08, None: 1.00}[hw]
        ins_mult = {"Standard": 1.00, "Complex": 1.20, None: 1.00}[ins]
        unit = base * (1.0 + area_coeff * area) * mat_mult * glz_mult * col_mult * hw_mult * ins_mult
        unit = max(80, unit + rng.normal(0, unit * 0.05))
        rows.append(dict(product_type=pt, material=mat, glazing=glz, width_mm=w, height_mm=h, area=area, qty=qty,
                         color_tier=col, hardware_tier=hw, install_complexity=ins, unit_price=unit))
    return pd.DataFrame(rows)

_df = _training()
pre = ColumnTransformer([
    ("cat", OneHotEncoder(handle_unknown="ignore"),
     ["product_type","material","glazing","color_tier","hardware_tier","install_complexity"]),
    ("num", "passthrough", ["area","qty"])
])
model = Pipeline([("pre", pre), ("lin", LinearRegression())])
model.fit(
    _df[["product_type","material","glazing","color_tier","hardware_tier","install_complexity","area","qty"]],
    _df["unit_price"]
)

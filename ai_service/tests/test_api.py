import pytest


def test_health(client):
    r = client.get("/health")
    assert r.status_code == 200
    assert r.json()["status"] == "ok"


def test_predict_batch_snake_case(client):
    payload = {
        "items": [
            {
                "product_type": "Window",
                "width_mm": 1200,
                "height_mm": 900,
                "material": "uPVC",
                "glazing": "double",
                "qty": 2,
                "color_tier": "Standard",
                "hardware_tier": "Standard",
                "install_complexity": "Standard",
            }
        ]
    }
    r = client.post("/predict-quote/batch", json=payload)
    assert r.status_code == 200
    data = r.json()
    assert "items" in data and len(data["items"]) == 1
    assert data["items"][0]["unit_price"] > 0
    assert 0.0 <= data["items"][0]["confidence"] <= 1.0
    assert "features" in data["items"][0]


def test_predict_batch_camel_case_and_normalization(client):
    # CamelCase + normalization (upvc -> uPVC, "bifold door" -> door)
    payload = {
        "items": [
            {
                "productType": "window",
                "widthMm": 1000,
                "heightMm": 1000,
                "material": "upvc",
                "glazing": "triple",
                "qty": 1,
                "colorTier": "Premium",
            },
            {
                "productType": "bifold door",
                "widthMm": 900,
                "heightMm": 2100,
                "material": "Aluminium",
                "glazing": "double",
                "qty": 1,
            },
        ]
    }
    r = client.post("/predict-quote/batch", json=payload)
    assert r.status_code == 200
    items = r.json()["items"]
    assert len(items) == 2
    assert items[0]["unit_price"] > 0
    assert items[1]["unit_price"] > 0


def test_predict_defaults_when_missing_or_unknown(client):
    # No product_type → defaults to "window"
    # Unknown material/glazing → defaults to "uPVC" / "double"
    payload = {
        "items": [
            {
                "width_mm": 900,
                "height_mm": 900,
                "material": "totally-unknown",
                "glazing": "???",
                "qty": 1,
            }
        ]
    }
    r = client.post("/predict-quote/batch", json=payload)
    assert r.status_code == 200
    feat = r.json()["items"][0]["features"]
    assert feat["product_type"] == "window"
    assert feat["material"] == "uPVC"
    assert feat["glazing"] == "double"


@pytest.mark.parametrize(
    "field,value",
    [
        ("width_mm", 299),  # below min
        ("height_mm", 5001),  # above max
        ("qty", 0),  # below min
    ],
)
def test_predict_batch_validation_422(client, field, value):
    base = {
        "product_type": "window",
        "width_mm": 300,
        "height_mm": 900,
        "material": "uPVC",
        "glazing": "double",
        "qty": 1,
    }
    base[field] = value
    r = client.post("/predict-quote/batch", json={"items": [base]})
    assert r.status_code == 422


def test_summarize_snake_case(client):
    payload = {
        "customer_name": "Smith Family",
        "items": [
            {
                "product_type": "Window",
                "width_mm": 1200,
                "height_mm": 900,
                "material": "uPVC",
                "glazing": "double",
                "qty": 2,
            }
        ],
        "vat_rate": 0.2,
    }
    r = client.post("/summarize-quote", json=payload)
    assert r.status_code == 200
    text = r.json()["text"]
    assert "Quotation for Smith Family" in text
    assert "20%" in text


def test_summarize_camel_case(client):
    payload = {
        "customerName": "Jones Ltd",
        "items": [
            {
                "productType": "door",
                "widthMm": 900,
                "heightMm": 2100,
                "material": "Composite",
                "glazing": "double",
                "qty": 1,
            }
        ],
        "vatRate": 0.05,
    }
    r = client.post("/summarize-quote", json=payload)
    assert r.status_code == 200
    text = r.json()["text"]
    assert "Jones Ltd" in text
    assert "5%" in text


def test_global_error_handler(client, monkeypatch):
    from ai_service import main as m

    def boom(*args, **kwargs):
        raise RuntimeError("kaboom")

    monkeypatch.setattr(m, "model", type("M", (), {"predict": boom})())

    # Use a dedicated TestClient here to disable exception bubbling
    from fastapi.testclient import TestClient

    with TestClient(m.app, raise_server_exceptions=False) as c:
        r = c.post(
            "/predict-quote/batch",
            json={
                "items": [
                    {
                        "product_type": "window",
                        "width_mm": 1200,
                        "height_mm": 900,
                        "material": "uPVC",
                        "glazing": "double",
                        "qty": 1,
                    }
                ]
            },
        )
        assert r.status_code == 500
        assert "kaboom" in r.json()["detail"]

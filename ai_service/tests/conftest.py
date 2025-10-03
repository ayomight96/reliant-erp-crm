import pytest
from fastapi.testclient import TestClient
from ai_service.main import app


@pytest.fixture(scope="session")
def client():
    # Plain TestClient; you'll see the httpx deprecation warning (which is fine).
    with TestClient(app) as c:
        yield c

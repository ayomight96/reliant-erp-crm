from fastapi import FastAPI
from .routes import router


def create_app() -> FastAPI:
    app = FastAPI(title="Reliant AI", version="0.2.0")
    app.include_router(router, prefix="")
    return app

FROM python:3.11-slim

WORKDIR /app
ENV PYTHONDONTWRITEBYTECODE=1 \
  PYTHONUNBUFFERED=1

# OS deps (numpy/pandas/scikit-learn will use wheels on slim)
RUN apt-get update && apt-get install -y --no-install-recommends build-essential && \
  rm -rf /var/lib/apt/lists/*

COPY requirements.txt .
RUN pip install --no-cache-dir -r requirements.txt

# 👇 copy *all* source, not just main.py
COPY . .

EXPOSE 8000
# 👇 use the factory from app/app.py
CMD ["uvicorn", "app.app:create_app", "--host", "0.0.0.0", "--port", "8000", "--factory"]

# Reliant ERP/CRM

A full‑stack ERP/CRM demo with **AI‑assisted quotations**. It manages customers, products and quotes, predicts **unit prices** with a Python AI service, and generates **natural‑language summaries** for customers.

**Stack**
- **Frontend:** Angular 17 + PrimeNG, served by Nginx (Docker)
- **API:** ASP.NET Core 8 (minimal APIs), EF Core (PostgreSQL or SQLite fallback), JWT auth
- **AI Service:** FastAPI (Python 3.11), scikit‑learn for pricing, optional LLMs for summaries
- **DB:** PostgreSQL 16 (Docker) or SQLite in local dev
- **Orchestration:** Docker Compose

---

## Table of Contents
- [Repository Layout](#repository-layout)
- [Prerequisites](#prerequisites)
- [Quick Start (Docker Compose)](#quick-start-docker-compose)
- [Local Development (without Docker)](#local-development-without-docker)
  - [Run the API](#run-the-api)
  - [Run the Angular Frontend](#run-the-angular-frontend)
  - [Run the AI Service](#run-the-ai-service)
- [Using the App (Frontend)](#using-the-app-frontend)
- [API Usage & Swagger](#api-usage--swagger)
  - [Authentication](#authentication)
  - [Key Endpoints](#key-endpoints)
  - [Sample cURL](#sample-curl)
- [AI Service (Endpoints & Config)](#ai-service-endpoints--config)
- [Database & Migrations](#database--migrations)
- [Configuration & Environment Variables](#configuration--environment-variables)
- [Troubleshooting](#troubleshooting)
- [License](#license)

---

## Repository Layout

```
reliant-erp-crm.sln
ai_service/
  Dockerfile
  requirements.txt
  main.py
  app/
    app.py        # FastAPI factory
    config.py     # settings (LLM providers, timeouts)
    routes.py     # /health, /predict-quote/batch, /summarize-quote
    pricing.py    # ML pricing logic
    llm.py, rag.py, facts.py, models.py
deploy/
  docker-compose.yml
  ai.env
docs/
src/
  Reliant.Api/
    Program.cs
    Endpoints/*.cs  # Auth, Products, Customers, Quotes, Admin
    Integration/AiPricing/*.cs
    Dockerfile
  Reliant.Infrastructure/
    AppDbContext.cs
    SeedData.cs
    InfrastructureExtensions.cs
    DesignTimeDbContextFactory.cs
  Reliant.Domain/     # Entities
  Reliant.Application/ # DTOs, validation, services
  Reliant.Angular/
    Dockerfile
    nginx.conf
    package.json
    proxy.conf.json
    src/app/...       # pages, guards, interceptors, services
tests/                # API tests (integration/unit)
```

---

## Prerequisites

- **Docker** & **Docker Compose** (for the one‑command stack)
- *(Optional local dev)*
  - **.NET SDK 8**
  - **Node.js 20** + **npm**
  - **Python 3.11**
  - **PostgreSQL 16** (or use SQLite fallback)

---

## Quick Start (Docker Compose)

Bring up **DB + AI + API + Web**:

```bash
cd deploy
# (optional) add keys to ai.env for LLM providers
docker compose up --build
```

**Services & Ports**
- **db**: Postgres on `5432`
- **ai**: FastAPI on `8000` (`/health` liveness)
- **api**: ASP.NET Core on `8080`
- **web**: Angular via Nginx on `8081`

**Open the apps**
- Frontend: http://localhost:8081
- Swagger (direct): http://localhost:8080/swagger
- Swagger (via Nginx): http://localhost:8081/swagger

**Demo accounts (seeded)**
- **Manager:** `manager@demo.local` / `Passw0rd!`
- **Sales:** `sales@demo.local` / `Passw0rd!`

> Nginx proxies `/api`, `/auth` and `/swagger` from the web container to the API container.

---

## Local Development (without Docker)

### Run the API

**A) Quick (SQLite fallback)**
```bash
dotnet run --project src/Reliant.Api
```
- Defaults to SQLite (`app.db`) if `ConnectionStrings__Default` isn’t a Postgres connection.

**B) With local Postgres**
```bash
# macOS/Linux
export ConnectionStrings__Default="Host=localhost;Port=5432;Database=reliant;Username=postgres;Password=postgres"
dotnet run --project src/Reliant.Api
```
> The API redirects `/` → `/swagger`, exposes JWT Bearer security, and auto‑migrates + seeds data in non‑Testing environments.

### Run the Angular Frontend
```bash
cd src/Reliant.Angular
npm ci
npm start -- --proxy-config proxy.conf.json --port 4200
# open http://localhost:4200
```
- Dev proxy forwards `/auth` and `/api` to `http://localhost:8080`.

### Run the AI Service
```bash
cd ai_service
python -m venv .venv && source .venv/bin/activate  # (Windows: .venv\Scripts\activate)
pip install -r requirements.txt
uvicorn app.app:create_app --host 0.0.0.0 --port 8000 --factory --reload
# health check: http://localhost:8000/health
```

---

## Using the App (Frontend)

1. **Sign in** with a demo account (above). Tokens are stored client‑side and attached via an interceptor. If a 401 occurs, you’ll be redirected to login.
2. **Customers** – list/search, create, and update customers.
3. **Products** – view the catalogue with base prices (e.g., windows, doors).
4. **Quotes**
   - **Create** a quote: pick a customer, add line items (dimensions, material, glazing, qty).  
     If `unitPrice` is omitted, the API calls the AI service to **predict** it.
   - **Summarize** a quote: the API asks the AI service to generate a **customer‑ready summary**. If the AI is unreachable, the API returns a `502 Bad Gateway` with details.
5. **Admin** (Manager role): list users and assign roles.

Nginx in the web image rewrites SPA routes and proxies `/api`, `/auth`, `/swagger` to the API.

---

## API Usage & Swagger

OpenAPI UI at **`/swagger`** (direct on `:8080` or proxied on `:8081`).

### Authentication
- **Login:** `POST /auth/login`
  - **Body:** `{"email":"sales@demo.local","password":"Passw0rd!"}`
  - **Response:** `{ "accessToken": "...jwt...", "expiresAtUtc": "...", "roles": ["Sales"] }`
- Use the token: `Authorization: Bearer <token>`

### Key Endpoints
- **Health**
  - `GET /healthz` – liveness
  - `GET /debug/counts` – quick row counts (requires auth)
- **Products** (Sales or Manager)
  - `GET /api/products`
- **Customers** (Sales or Manager)
  - `GET /api/customers?q=...`
  - `POST /api/customers`
  - `PUT /api/customers/{id}`
- **Quotes** (Sales or Manager)
  - `GET /api/customers/{customerId}/quotes`
  - `POST /api/quotes` – create; AI fills missing `unitPrice`
  - `POST /api/quotes/summary` – generate NL summary
- **Admin** (Manager only)
  - `GET /admin/users`
  - `POST /admin/users/{userId}/roles/{roleName}` – assign single role

### Sample cURL

```bash
# Get a JWT
TOKEN=$(curl -s http://localhost:8080/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"sales@demo.local","password":"Passw0rd!"}' | jq -r .accessToken)

# List products
curl -s http://localhost:8080/api/products -H "Authorization: Bearer $TOKEN" | jq .

# Create a quote (AI fills unit price)
curl -s http://localhost:8080/api/quotes \
  -H "Authorization: Bearer $TOKEN" -H "Content-Type: application/json" \
  -d '{"customerId":1,"items":[{"productId":1,"widthMm":1200,"heightMm":900,"material":"uPVC","glazing":"double","qty":2}]}' | jq .

# Summarize a quote
curl -s http://localhost:8080/api/quotes/summary \
  -H "Authorization: Bearer $TOKEN" -H "Content-Type: application/json" \
  -d '{"customerId":1,"items":[{"productId":1,"widthMm":1200,"heightMm":900,"material":"uPVC","glazing":"double","qty":1}]}' | jq .
```

---

## AI Service (Endpoints & Config)

**Base URL (Docker):** `http://ai:8000` (API reads `AI:BaseUrl` / `AI__BaseUrl`, defaults to this)

**Endpoints**
- `GET /health` – liveness
- `POST /predict-quote/batch` – returns unit prices for items
- `POST /summarize-quote` – returns `{"text": "...summary..."}`; includes **deterministic fallback** when external LLMs are disabled or timeout

**Configuration (`deploy/ai.env`)**
```env
OPENROUTER_MODEL=openai/gpt-oss-20b:free
OPENROUTER_API_KEY=
HF_MODEL=mistralai/Mistral-7B-Instruct-v0.2
HUGGINGFACE_API_KEY=
HTTP_TIMEOUT=60
USE_EXTERNAL_LLM=true
```

> When `USE_EXTERNAL_LLM=false` (or keys are missing), the service will still summarize using deterministic templates and embedded facts.

---

## Database & Migrations

- **Provider Selection**
  - If `ConnectionStrings__Default` **contains** `Host=...` → **PostgreSQL (Npgsql)**
  - Otherwise → **SQLite** (`app.db`)
- **Auto‑migrate & Seed**
  - On API startup (non‑Testing), the schema is **migrated/created** and **seeded** with roles, demo users, customers and products.
- **Precision**
  - Prices use `decimal(12,2)`; indexes & length limits on key fields.

**Design‑time factory**
- `DesignTimeDbContextFactory` reads `MIGRATIONS_CS` or `ConnectionStrings__Default` for tooling.

---

## Configuration & Environment Variables

### API
- `ConnectionStrings__Default` – database connection
- `AI__BaseUrl` / `AI:BaseUrl` – base URL for the AI service (default `http://ai:8000`)
- `Jwt:Key` – HMAC secret for JWT
- `Jwt:Issuer` – token issuer (default `reliant`)
- `Jwt:Audience` – token audience (default `reliant.clients`)

### Web (Nginx)
- Proxies:
  - `/api` → `http://api:8080/api`
  - `/auth` → `http://api:8080/auth`
  - `/swagger` → `http://api:8080/swagger`

### Angular Dev Proxy
- `proxy.conf.json` forwards `/auth` and `/api` to `http://localhost:8080`

---

## Troubleshooting

- **Can’t reach the API from the web container**
  - Ensure Compose is up; Nginx proxies `/api`, `/auth`, `/swagger` to `api:8080`.
- **401 / Forbidden**
  - Obtain a JWT via `/auth/login` and ensure your user has the required role (Sales or Manager).
- **Summary request returns 502**
  - The API returns **502 Bad Gateway** on upstream AI failures; verify `AI__BaseUrl` and the AI health endpoint.
- **Switching DB providers**
  - Set/unset `ConnectionStrings__Default`. Provider auto‑selects based on presence of `Host=`.
- **Ports clash**
  - Stop other services or override Compose port mappings.

---

## License

For demo/educational use. Replace or add a SPDX license identifier as needed.

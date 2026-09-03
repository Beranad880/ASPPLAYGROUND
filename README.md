# ⚡ ASPNET PLAYGROUND – SignalR Chat, Person CRUD & Redis Link Share (.NET 10)

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/C%23-13.0-239120?style=for-the-badge&logo=csharp&logoColor=white)](https://learn.microsoft.com/dotnet/csharp/)
[![SignalR](https://img.shields.io/badge/SignalR-Real--Time-blueviolet?style=for-the-badge&logo=socketdotio&logoColor=white)](https://learn.microsoft.com/aspnet/core/signalr/introduction)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-EF%20Core-336791?style=for-the-badge&logo=postgresql&logoColor=white)](https://www.postgresql.org/)
[![Redis](https://img.shields.io/badge/Redis-StackExchange.Redis-DC382D?style=for-the-badge&logo=redis&logoColor=white)](https://redis.io/)
[![Docker](https://img.shields.io/badge/Docker-Ready-2496ED?style=for-the-badge&logo=docker&logoColor=white)](https://www.docker.com/)
[![Railway](https://img.shields.io/badge/Deploy-Railway-0B0D0E?style=for-the-badge&logo=railway&logoColor=white)](https://railway.com/)
[![Swagger](https://img.shields.io/badge/Swagger-OpenAPI-85EA2D?style=for-the-badge&logo=swagger&logoColor=black)](https://swagger.io/)

**ASPNET PLAYGROUND** je moderní a vysoce výkonná webová aplikace postavená na **ASP.NET Core (.NET 10)**. Nabízí rozdělený rozcestník (Split View) se třemi plnohodnotnými moduly: **SignalR Real-Time Global Chat**, **Person CRUD REST API** pro správu osob napojenou na **PostgreSQL**, a **Redis Cross-Device Link Share** pro bleskové sdílení textů a odkazů mezi zařízeními.

---

## 🌟 Klíčové vlastnosti

- 🏠 **ASPNET PLAYGROUND Hub** – Vstupní rozcestník s 3-sloupcovým výběrem mezi real-time chatem, CRUD aplikací a Redis sdílením odkazů včetně globální navigační lišty.
- 💬 **SignalR Real-Time Chat (`/chat`)** – Obousměrná komunikace přes WebSockets, živé počítadlo online uživatelů, ukládání historie zpráv do Redis listu (`global:chat:messages`, limit 100 zpráv, TTL 7d), syntetizované audio efekty a emoji reakce.
- 👥 **Person CRUD App (`/persons`)** – Brutalistická / Glassmorphism tabulka s živým vyhledáváním, tvorbou, editací a mazáním osob, validacemi a generátorem náhodných českých osob.
- 🔗 **Redis Cross-Device Link Share (`/link`)** – Okamžité sdílení textů, poznámek a URL mezi zařízeními pomocí Redis listu (`shared:links`). K synchronizaci napříč zařízeními využívá **SignalR** pro bleskové real-time překreslení bez pollingu.
- 🛡️ **Graceful Fallback & Error Handling** – Režim in-memory při výpadku spojení s infrastrukturou. Všechny HTTP chyby jsou unifikovaně vraceny přes moderní **ProblemDetails**.
- 🐘 **PostgreSQL & EF Core** – Plně asynchronní servisní vrstva se stránkováním (Pagination) a automatickými asynchronními migracemi (`MigrateAsync`).
- 📜 **Swagger / OpenAPI** – Interaktivní dokumentace a testování všech REST endpointů přímo v prohlížeči na `/swagger`.
- 🎨 **Pure CSS Brutalist / Glassmorphism Design** – Responsivní design, vysoký kontrast, Space Grotesk & Space Mono typografie a matné skleněné komponenty.
- ☁️ **Railway & Docker Ready** – Optimalizovaný multi-stage Dockerfile a `railway.json` pro okamžité nasazení s podporou PostgreSQL a Redis pluginů.

---

## 🛠️ Použité technologie

| Oblast | Technologie |
|---|---|
| **Backend & Framework** | ASP.NET Core (.NET 10), C# 13 |
| **Real-time Engine** | ASP.NET Core SignalR (WebSockets) |
| **ORM & RDBMS** | Entity Framework Core, Npgsql (PostgreSQL) |
| **Cache & Key-Value Store** | StackExchange.Redis (Redis 7+) |
| **API Dokumentace** | Swashbuckle Swagger / OpenAPI (.NET 10) |
| **Frontend** | Razor Pages, Vanilla JavaScript, Pure CSS (Brutalist Design System) |
| **Kontejnerizace & Cloud** | Docker (Multi-stage build), Railway |

---

## 📁 Struktura projektu

```text
WebApplicationASP01/
├── App/
│   ├── AppDbContext.cs        # EF Core DbContext mapující entitu Person
│   ├── Person.cs              # Entita Person + DTO modely (CreatePersonDto, UpdatePersonDto)
│   ├── PersonService.cs       # Asynchronní servisní vrstva pro CRUD logiku osob
│   ├── PersonsController.cs   # API Controller (/api/persons)
│   └── LinksController.cs     # API Controller pro Redis sdílení odkazů (/api/links)
├── Hubs/
│   └── ChatHub.cs             # SignalR Hub pro WebSocket komunikaci
├── Models/
│   ├── ChatMessage.cs         # Record reprezentující chatovou zprávu
│   └── LinkEntry.cs           # Model LinkEntry, CreateLinkDto a LinkServiceStatus
├── Services/
│   ├── ChatHistoryService.cs  # In-memory správa a limitování historie chatu
│   └── LinkService.cs         # Servisní vrstva pro Redis list (LPUSH, LTRIM 50, TTL 7d, fallback)
├── Pages/
│   ├── Index.cshtml           # ASPNET PLAYGROUND rozcestník (3 moduly)
│   ├── Index.cshtml.cs        # PageModel pro Index
│   ├── Chat.cshtml            # SignalR Real-Time Chat aplikace (/chat)
│   ├── Chat.cshtml.cs         # PageModel pro Chat
│   ├── Persons.cshtml         # Person CRUD aplikace (/persons)
│   ├── Persons.cshtml.cs      # PageModel pro Persons
│   ├── Link.cshtml            # Redis Cross-Device Link Share aplikace (/link)
│   ├── Link.cshtml.cs         # PageModel pro Link
│   └── Shared/_Layout.cshtml  # Hlavní layout s globální navigační lištou a tickerem
├── Properties/
│   └── launchSettings.json    # Profily pro lokální spouštění
├── wwwroot/
│   ├── css/chat.css           # Brutalist & Glassmorphism styly, animace a responzivita
│   ├── js/chat.js             # SignalR klient, obsluha chatu a syntetizované zvuky
│   ├── js/persons.js          # Klient pro Person CRUD, vyhledávání a modály
│   ├── js/link.js             # Klient pro Link Share, schránku, clipboard a auto-sync
│   └── js/signalr.min.js      # SignalR klientská knihovna
├── appsettings.json           # Výchozí konfigurace aplikace s placeholdery
├── Dockerfile                 # Multi-stage Dockerfile (.NET 10)
├── railway.json               # Konfigurace sestavení a nasazení pro Railway
├── Program.cs                 # Konfigurace služeb, DB, Redis, Swaggeru a HTTP pipeline
└── WebApplicationASP01.csproj # Projektový soubor .NET 10 (StackExchange.Redis, Npgsql, SignalR)
```

---

## 📡 REST API Dokumentace

- **Interaktivní Swagger UI:**
  - Lokálně: `http://localhost:5129/swagger`
  - V produkci: `https://<vasedomena>.up.railway.app/swagger`

### 1. Redis Link Share API (`/api/links`)

| Metoda | Endpoint | Popis | Návratový kód |
|---|---|---|---|
| `GET` | `/api/links` | Vrátí seznam uložených textů/URL z Redis seřazený od nejnovějšího (`LRANGE`) | `200 OK` |
| `POST` | `/api/links` | Uloží nový text nebo URL odkaz do Redis (`LPUSH`, `LTRIM 50`, `EXPIRE 7d`) | `201 Created` / `400 Bad Request` |
| `DELETE` | `/api/links/{id}` | Smaže konkrétní text/URL podle ID (GUID) nebo číselného indexu | `204 NoContent` / `404 Not Found` |
| `DELETE` | `/api/links/clear` | Smaže celou historii odkazů (odstraní klíč `shared:links` z Redis) | `200 OK` |
| `GET` | `/api/links/status` | Vrátí stav připojení k Redis, počet položek a režim úložiště | `200 OK` |

#### Příklady pro `/api/links`

**Vložení nového odkazu (`POST /api/links`):**
```json
{
  "content": "https://github.com/dotnet/aspnetcore"
}
```
*(Podporuje i aliasy `{ "text": "..." }` a `{ "url": "..." }`)*

**Odpověď (`201 Created`):**
```json
{
  "id": "e4a78c19283f4b59b910123456789abc",
  "content": "https://github.com/dotnet/aspnetcore",
  "createdAt": "2026-09-03T11:45:00.123Z",
  "isUrl": true
}
```

**Získání všech odkazů (`GET /api/links`):**
```json
[
  {
    "id": "e4a78c19283f4b59b910123456789abc",
    "content": "https://github.com/dotnet/aspnetcore",
    "createdAt": "2026-09-03T11:45:00.123Z",
    "isUrl": true
  },
  {
    "id": "a1b2c3d4e5f6789012345678abcdef01",
    "content": "Poznámka: Nákupní seznam pro projekt",
    "createdAt": "2026-09-03T11:40:12.456Z",
    "isUrl": false
  }
]
```

---

### 2. Person CRUD API (`/api/persons`)

| Metoda | Endpoint | Popis | Návratový kód |
|---|---|---|---|
| `GET` | `/api/persons?page=1&pageSize=50` | Vrátí stránkovaný seznam osob (PagedResult) z PostgreSQL | `200 OK` |
| `GET` | `/api/persons/{id}` | Vrátí detail konkrétní osoby podle GUID | `200 OK` / `404 Not Found` |
| `POST` | `/api/persons` | Vytvoří novou osobu | `201 Created` / `400 Bad Request` |
| `PUT` | `/api/persons/{id}` | Upraví existující osobu podle GUID | `200 OK` / `400 Bad Request` / `404 Not Found` |
| `DELETE` | `/api/persons/{id}` | Smaže osobu podle GUID | `204 NoContent` / `404 Not Found` |
| `GET` | `/api/persons/ahoj` | Testovací endpoint / Healthcheck | `200 OK` |

---

### 3. System Diagnostics & Health Check API (`/api/check`, `/check`)

| Metoda | Endpoint | Popis | Návratový kód |
|---|---|---|---|
| `GET` | `/check` | Interaktivní webová stránka s diagnostikou PostgreSQL & Redis v reálném čase | `200 OK` (HTML) |
| `GET` | `/api/check` | Vrátí JSON stav konektivity PostgreSQL a Redis včetně latence, verzí a chyb | `200 OK` (JSON) |
| `GET` | `/check/status` | Alias pro `/api/check` vracející JSON diagnostiku | `200 OK` (JSON) |
| `GET` | `/api/check/ping` | Rychlý healthcheck ping vracející `{ "status": "healthy" }` | `200 OK` (JSON) |

#### Příklad odpovědi (`GET /api/check`):
```json
{
  "overallStatus": "Healthy",
  "timestamp": "2026-09-03T11:49:16.341Z",
  "totalCheckDurationMs": 14.25,
  "postgres": {
    "name": "PostgreSQL",
    "type": "Relational Database (EF Core / Npgsql)",
    "isConnected": true,
    "status": "Online",
    "latencyMs": 12.8,
    "errorMessage": null,
    "details": {
      "Database": "persondb",
      "DataSource": "localhost:5432",
      "ServerVersion": "PostgreSQL 16.2"
    }
  },
  "redis": {
    "name": "Redis",
    "type": "In-Memory Cache & Key-Value (StackExchange.Redis)",
    "isConnected": true,
    "status": "Online",
    "latencyMs": 1.45,
    "errorMessage": null,
    "details": {
      "Endpoint": "localhost:6379",
      "ClientName": "ASPNET_PLAYGROUND",
      "PingLatency": "1.45 ms"
    }
  },
  "environment": {
    "framework": ".NET 10.0 (C# 13)",
    "environmentName": "Production",
    "osPlatform": "Linux x86_64",
    "serverTimeUtc": "2026-09-03T11:49:16.341Z"
  }
}
```

---

## ⚙️ Konfigurace a proměnné prostředí

Aplikace inteligentně detekuje běhové prostředí a automaticky parsuje standardní i cloudové connection stringy pro PostgreSQL i Redis.

| Proměnná | Popis | Výchozí hodnota |
|---|---|---|
| `PORT` | Port, na kterém aplikace naslouchá (poskytováno Railway) | `8080` / `5129` |
| `REDIS_URL` | Redis connection URL (`redis://[:pass@]host:port` nebo `rediss://...`) | `localhost:6379` |
| `REDIS_PRIVATE_URL` | Interní privátní Redis URL v rámci Railway sítě | - |
| `REDIS_PUBLIC_URL` | Veřejná URL Redis instance | - |
| `REDISHOST` | Hostitel Redis serveru (poskytováno Railway Redis pluginem) | - |
| `REDISPORT` | Port Redis serveru | `6379` |
| `REDISPASSWORD` | Heslo pro autentizaci do Redis | - |
| `REDISUSER` | Uživatel pro Redis ACL (volitelné) | `default` |
| `DATABASE_URL` | PostgreSQL připojení ve formátu URL (`postgresql://user:pass@host:port/db`) | Z `appsettings.json` |
| `DATABASE_PRIVATE_URL` | Privátní interní URL databáze v rámci privátní sítě Railway | - |
| `DATABASE_PUBLIC_URL` | Veřejná URL PostgreSQL databáze | - |
| `PGHOST` / `PGPORT` / `PGDATABASE` / `PGUSER` / `PGPASSWORD` | Jednotlivé PostgreSQL parametry | `localhost:5432` |
| `ASPNETCORE_ENVIRONMENT` | Běhové prostředí (`Development` / `Production`) | `Production` |
| `DOTNET_USE_POLLING_FILE_WATCHER` | Zabraňuje inotify limit chybám v Linux kontejnerech | `true` |

---

## 💻 Lokální spuštění

### Požadavky
- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [PostgreSQL](https://www.postgresql.org/) (volitelně, aplikace má in-memory / warning ošetření)
- [Redis](https://redis.io/) (volitelně, pokud Redis neběží, `/link` automaticky použije in-memory zálohu)

### 1. Spuštění přes .NET CLI
```bash
# Obnovení závislostí a sestavení
dotnet build

# Spuštění aplikace
dotnet run --project WebApplicationASP01.csproj
```

- **Přehled & Hub:** [http://localhost:5129](http://localhost:5129)
- **Redis Link Share:** [http://localhost:5129/link](http://localhost:5129/link)
- **SignalR Chat:** [http://localhost:5129/chat](http://localhost:5129/chat)
- **Person CRUD:** [http://localhost:5129/persons](http://localhost:5129/persons)
- **Swagger UI:** [http://localhost:5129/swagger](http://localhost:5129/swagger)

### 2. Spuštění s Redisem přes Docker
```bash
# Spuštění lokálního Redis kontejneru
docker run -d -p 6379:6379 --name local-redis redis:7-alpine

# Sestavení a spuštění aplikace
docker build -t aspnet-playground .
docker run -d -p 8080:8080 -e REDIS_URL=host.docker.internal:6379 --name aspnet-playground-app aspnet-playground
```
Aplikace bude dostupná na `http://localhost:8080`.

---

## ☁️ Návod na nasazení na Railway

1. **Odešlete kód na GitHub**:
   ```bash
   git add .
   git commit -m "Add Redis Link Share feature"
   git push origin main
   ```

2. **Vytvořte projekt na Railway**:
   - Přejděte na [railway.com](https://railway.com/) a přihlaste se.
   - Klikněte na **New Project** -> **Deploy from GitHub repo** a zvolte váš repozitář.

3. **Přidejte PostgreSQL databázi**:
   - V projektu klikněte na **+ Create** -> **Database** -> **Add PostgreSQL**.

4. **Přidejte Redis databázi**:
   - V projektu klikněte na **+ Create** -> **Database** -> **Add Redis**.
   - Railway okamžitě vytvoří samostatnou instanci Redis 7+.

5. **Propojení proměnných prostředí s aplikací**:
   - Přejděte do nastavení vašeho webového servisu na záložku **Variables**.
   - Přidejte nebo ověřte referenční proměnné:
     ```text
     DATABASE_URL = ${{Postgres.DATABASE_URL}}
     REDIS_URL    = ${{Redis.REDIS_URL}}
     ```
     *(případně `${{Redis.REDIS_PRIVATE_URL}}` a `${{Postgres.DATABASE_PRIVATE_URL}}` pro interní propojení v rámci privátní sítě).*

6. **Generování veřejné HTTPS domény**:
   - V nastavení webového servisu přejděte na **Settings** -> **Networking** -> **Generate Domain**.
   - Aplikace okamžitě získá zabezpečenou HTTPS doménu (např. `https://vasesluzba.up.railway.app`).

7. **Hotovo**:
   - Railway zkompiluje aplikaci v multi-stage Dockeru.
   - Databázové tabulky se automaticky inicializují při prvním startu.
   - Redis spojení je navázáno a `/link` je okamžitě připraven k synchronizaci textů a odkazů mezi všemi vašimi zařízeními!

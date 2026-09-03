# ⚡ SignalR Global Chat & Person CRUD REST API

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/C%23-13.0-239120?style=for-the-badge&logo=csharp&logoColor=white)](https://learn.microsoft.com/dotnet/csharp/)
[![SignalR](https://img.shields.io/badge/SignalR-Real--Time-blueviolet?style=for-the-badge&logo=socketdotio&logoColor=white)](https://learn.microsoft.com/aspnet/core/signalr/introduction)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-EF%20Core-336791?style=for-the-badge&logo=postgresql&logoColor=white)](https://www.postgresql.org/)
[![Docker](https://img.shields.io/badge/Docker-Ready-2496ED?style=for-the-badge&logo=docker&logoColor=white)](https://www.docker.com/)
[![Railway](https://img.shields.io/badge/Deploy-Railway-0B0D0E?style=for-the-badge&logo=railway&logoColor=white)](https://railway.com/)
[![Swagger](https://img.shields.io/badge/Swagger-OpenAPI-85EA2D?style=for-the-badge&logo=swagger&logoColor=black)](https://swagger.io/)

Moderní a vysoce výkonná webová aplikace postavená na **ASP.NET Core (.NET 10)**. Spojuje **SignalR Real-Time Global Chat** s plnohodnotným **REST API (CRUD)** pro správu osob, asynchronní servisní architekturou, perzistencí v **PostgreSQL** a moderním uživatelským rozhraním v designovém stylu **Glassmorphism**.

---

## 🌟 Klíčové vlastnosti

- 💬 **SignalR Real-Time Chat** – Obousměrná komunikace přes WebSockets s automatickým fallbackem, in-memory historií a zvukovými notifikacemi.
- 👥 **Person CRUD REST API** – Kompletní správa osob s validacemi datových anotací, Guid identifikátory a asynchronním zpracováním.
- 🐘 **PostgreSQL & EF Core** – Automatické migrace a tvorba schématu (`EnsureCreated`), podpora lokálního PostgreSQL i cloudového napojení přes Railway (`DATABASE_URL`).
- 📜 **Swagger / OpenAPI** – Interaktivní dokumentace a testování všech REST endpointů přímo v prohlížeči.
- 🎨 **Moderní Glassmorphism UI** – Responsivní design, animované ambientní světelné koule, matné skleněné prvky a podpora pro mobilní zařízení.
- ☁️ **Railway & Docker Ready** – Optimalizovaný multi-stage Dockerfile a konfigurační soubor `railway.json` pro okamžité nasazení.

---

## 🛠️ Použité technologie

| Oblast | Technologie |
|---|---|
| **Backend & Framework** | ASP.NET Core (.NET 10), C# 13 |
| **Real-time Engine** | ASP.NET Core SignalR |
| **ORM & Databáze** | Entity Framework Core, Npgsql (PostgreSQL) |
| **API Dokumentace** | Swashbuckle Swagger / OpenAPI |
| **Frontend** | Razor Pages, Vanilla JavaScript (SignalR Client), Pure CSS (Glassmorphism) |
| **Kontejnerizace & Cloud** | Docker (Multi-stage build), Railway |

---

## 📁 Struktura projektu

```text
WebApplicationASP01/
├── App/
│   ├── AppDbContext.cs        # EF Core DbContext mapující entitu Person
│   ├── Person.cs              # Entita Person + DTO modely (CreatePersonDto, UpdatePersonDto)
│   ├── PersonService.cs       # Asynchronní servisní vrstva pro CRUD logiku
│   └── PersonsController.cs   # API Controller (/api/persons)
├── Hubs/
│   └── ChatHub.cs             # SignalR Hub pro WebSocket komunikaci
├── Models/
│   └── ChatMessage.cs         # Record reprezentující chatovou zprávu
├── Services/
│   └── ChatHistoryService.cs  # In-memory správa a limitování historie chatu
├── Pages/
│   ├── Index.cshtml           # Razor šablona chatu
│   ├── Index.cshtml.cs        # PageModel
│   └── Shared/_Layout.cshtml  # Hlavní HTML layout
├── Properties/
│   └── launchSettings.json    # Profily pro lokální spouštění
├── wwwroot/
│   ├── css/chat.css           # Glassmorphism styly, animace a responzivita
│   └── js/chat.js             # SignalR klient, obsluha chatu a zvuky
├── appsettings.json           # Výchozí konfigurace aplikace
├── Dockerfile                 # Multi-stage Dockerfile (.NET 10)
├── railway.json               # Konfigurace sestavení a nasazení pro Railway
├── Program.cs                 # Konfigurace služeb, DB, Swaggeru a HTTP pipeline
└── WebApplicationASP01.csproj # Projektový soubor .NET 10
```

---

## 📡 REST API Dokumentace

- **Základní cesta API:** `/api/persons`
- **Interaktivní Swagger UI:**
  - Lokálně: `http://localhost:5129/swagger`
  - V produkci: `https://<vasedomena>.up.railway.app/swagger`

### Přehled endpointů

| Metoda | Endpoint | Popis | Návratový kód |
|---|---|---|---|
| `GET` | `/api/persons` | Vrátí seznam všech osob | `200 OK` |
| `GET` | `/api/persons/{id}` | Vrátí detail konkrétní osoby podle GUID | `200 OK` / `404 Not Found` |
| `POST` | `/api/persons` | Vytvoří novou osobu | `201 Created` / `400 Bad Request` |
| `PUT` | `/api/persons/{id}` | Upraví existující osobu podle GUID | `200 OK` / `400 Bad Request` / `404 Not Found` |
| `DELETE` | `/api/persons/{id}` | Smaže osobu podle GUID | `204 NoContent` / `404 Not Found` |
| `GET` | `/api/persons/ahoj` | Testovací endpoint / Healthcheck | `200 OK` |

---

### 📋 Příklady požadavků a odpovědí

#### 1. Vytvoření osoby (`POST /api/persons`)
**Request Body (`application/json`):**
```json
{
  "jmeno": "Jan Novák",
  "datumNarozeni": "1990-05-15",
  "trvalaAdresa": "Václavské náměstí 1, 110 00 Praha 1",
  "rodneCislo": "900515/1234",
  "telefon": "+420 777 123 456",
  "email": "jan.novak@example.com"
}
```

**Response (`201 Created`):**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "jmeno": "Jan Novák",
  "datumNarozeni": "1990-05-15",
  "trvalaAdresa": "Václavské náměstí 1, 110 00 Praha 1",
  "rodneCislo": "900515/1234",
  "telefon": "+420 777 123 456",
  "email": "jan.novak@example.com"
}
```

---

#### 2. Úprava osoby (`PUT /api/persons/{id}`)
**Request Body (`application/json`):**
```json
{
  "jmeno": "Jan Novák",
  "datumNarozeni": "1990-05-15",
  "trvalaAdresa": "Nová ulice 123, 602 00 Brno",
  "rodneCislo": "900515/1234",
  "telefon": "+420 777 999 888",
  "email": "jan.novak.novy@example.com"
}
```

---

#### 3. Získání všech osob (`GET /api/persons`)
**Response (`200 OK`):**
```json
[
  {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "jmeno": "Jan Novák",
    "datumNarozeni": "1990-05-15",
    "trvalaAdresa": "Václavské náměstí 1, 110 00 Praha 1",
    "rodneCislo": "900515/1234",
    "telefon": "+420 777 123 456",
    "email": "jan.novak@example.com"
  }
]
```

---

## ⚙️ Konfigurace a proměnné prostředí

Aplikace inteligentně detekuje běhové prostředí a automaticky parsuje standardní i cloudové connection stringy.

| Proměnná | Popis | Výchozí hodnota |
|---|---|---|
| `PORT` | Port, na kterém aplikace naslouchá (poskytováno Railway) | `8080` / `5129` |
| `DATABASE_URL` | PostgreSQL připojení ve formátu URL (`postgresql://user:pass@host:port/db`) | Z `appsettings.json` |
| `DATABASE_PRIVATE_URL` | Privátní interní URL databáze v rámci privátní sítě Railway | - |
| `DATABASE_PUBLIC_URL` | Veřejná URL PostgreSQL databáze | - |
| `ASPNETCORE_ENVIRONMENT` | Běhové prostředí (`Development` / `Production`) | `Production` |
| `DOTNET_USE_POLLING_FILE_WATCHER` | Zabraňuje inotify limit chybám v Linux kontejnerech | `true` |

---

## 💻 Lokální spuštění

### Požadavky
- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [PostgreSQL](https://www.postgresql.org/) (volitelně, běží i lokálně)

### 1. Spuštění přes .NET CLI
```bash
# Obnovení závislostí a sestavení
dotnet build

# Spuštění aplikace
dotnet run --project WebApplicationASP01.csproj
```

- **Web Chat:** [http://localhost:5129](http://localhost:5129)
- **Swagger UI:** [http://localhost:5129/swagger](http://localhost:5129/swagger)

### 2. Spuštění v Dockeru
```bash
# Sestavení Docker image
docker build -t signalr-chat .

# Spuštění kontejneru
docker run -d -p 8080:8080 --name signalr-chat-app signalr-chat
```
Aplikace bude dostupná na `http://localhost:8080`.

---

## ☁️ Návod na nasazení na Railway

1. **Odešlete kód na GitHub**:
   ```bash
   git add .
   git commit -m "Deploy to Railway"
   git push origin main
   ```

2. **Vytvořte projekt na Railway**:
   - Přejděte na [railway.com](https://railway.com/) a přihlaste se.
   - Klikněte na **New Project** -> **Deploy from GitHub repo** a zvolte váš repozitář.

3. **Přidejte PostgreSQL databázi**:
   - V projektu klikněte na **+ Create** -> **Database** -> **Add PostgreSQL**.
   - Railway vytvoří samostatnou instanci PostgreSQL.

4. **Propojení databáze s aplikací**:
   - Přejděte do nastavení vašeho webového servisu na záložku **Variables**.
   - Ověřte nebo přidejte proměnnou `DATABASE_URL`:
     ```text
     DATABASE_URL = ${{Postgres.DATABASE_URL}}
     ```
     *(případně `${{Postgres.DATABASE_PRIVATE_URL}}` pro interní propojení v rámci privátní sítě).*

5. **Generování veřejné domény**:
   - V nastavení webového servisu přejděte na **Settings** -> **Networking** -> **Generate Domain**.
   - Aplikace okamžitě získá HTTPS doménu (např. `https://vasesluzba.up.railway.app`).

6. **Hotovo**:
   - Railway sestaví aplikaci přes `Dockerfile`.
   - Databázové tabulky se automaticky vytvoří při prvním startu.

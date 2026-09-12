# ASPNET PLAYGROUND

Tři nezávislé fullstack moduly v jedné aplikaci. Čistý výkon .NET 10, SignalR WebSockets a PostgreSQL. Hostováno na Railway s využitím multi-stage Docker buildů.

## 🚀 Technologie
- **Backend:** C# 13, .NET 10.0, ASP.NET Core
- **Databáze:** PostgreSQL (Npgsql + Entity Framework Core 10)
- **Real-time:** SignalR (WebSockets duplex streaming)
- **Frontend:** Razor Pages, čisté brutalistní UI, vanilla JS
- **API Dokumentace:** Swagger / OpenAPI
- **Ochrana:** Rate Limiting (15 req/s)
- **Kontejnerizace:** Docker
- **Nasazení:** Railway (CI/CD)

---

## 🧩 Moduly aplikace

### 1. 📝 Notes App (Real-time CRUD)
Plnohodnotný asynchronní CRUD pro správu poznámek s živou synchronizací napříč všemi klienty.

- **Clean Architecture:** Controller → Service → Repository → DbContext.
- **Entity Framework Core:** Využití PostgreSQL tabulek (sloupce: id, title, content, createdAt, updatedAt).
- **SignalR Real-Time:** Když kdokoli založí, upraví nebo smaže poznámku, přes `NotesHub` dojde k okamžité aktualizaci UI všem ostatním, aniž by museli obnovit stránku.
- **Search (ILIKE):** Rychlé fulltext vyhledávání přes `/api/notes/search` rovnou v DB.

### 2. ⚡ SignalR Chat
Real-time chatovací místnost fungující přes WebSockets s historií uchovávanou v paměti procesu.
- **Auto Reconnect:** SignalR hlídá stabilitu spojení.
- **Live Presence:** Ukazuje, kolik uživatelů je zrovna připojených (zelený indikátor).
- **Audio FX:** Syntetizátor pípání ve Web Audio API upozorňující na novou zprávu.
- **In-Memory Historie:** Posledních 100 zpráv v `ConcurrentQueue` — živá po dobu běhu aplikace.

### 3. 🔗 Link Share
Nástroj pro bleskové sdílení odkazů mezi telefonem a počítačem, pokud jsou oba klienti na webu.
- Data persistentně uložená v **PostgreSQL** tabulce `shared_links`.
- Jakmile přidáte text, odkaz se přes WebSocket (SignalR) hned ukáže u všech ostatních.
- Kapacita omezena na 50 odkazů — nejstarší se automaticky odstraní.

### 4. 🩺 Diagnostika & Zabezpečení
Na adrese `/checks` naleznete "Brutalist" diagnostický dashboard, který v reálném čase reportuje:
- Latenci a stav PostgreSQL připojení
- Stav a počet odkazů v `shared_links`
- Využití paměti RAM a CPU Time
- Stav integrovaného .NET **Rate Limiteru** (15 req/sec)
- Možnost vyčíst hrubá data ve formátu JSON na `/api/check`

---

## 🗄️ Notes API (`/api/notes`)

| Metoda | Endpoint | Popis | Návratový kód |
|---|---|---|---|
| `GET` | `/api/notes` | Vrátí seznam všech poznámek, řazené sestupně dle `UpdatedAt` | `200 OK` |
| `GET` | `/api/notes/{id}` | Vrátí detail konkrétní poznámky podle GUID | `200 OK` / `404 Not Found` |
| `GET` | `/api/notes/search?query=...`| Vyhledávání přes `ILIKE` v `Title` a `Content` | `200 OK` |
| `POST` | `/api/notes` | Vytvoří novou poznámku | `201 Created` / `400 Bad Request` |
| `PUT` | `/api/notes/{id}` | Upraví existující poznámku podle GUID | `200 OK` / `400 Bad Request` / `404 Not Found` |
| `DELETE` | `/api/notes/{id}` | Smaže poznámku podle GUID | `204 NoContent` / `404 Not Found` |

## 🔗 Link Share API (`/api/links`)

| Metoda | Endpoint | Popis | Návratový kód |
|---|---|---|---|
| `GET` | `/api/links` | Vrátí posledních 50 odkazů, řazené od nejnovějšího | `200 OK` |
| `POST` | `/api/links` | Přidá nový text nebo URL | `201 Created` / `400 Bad Request` |
| `GET` | `/api/links/status` | Stav úložiště a počet záznamů | `200 OK` |
| `DELETE` | `/api/links/clear` | Smaže všechny záznamy | `200 OK` |
| `DELETE` | `/api/links/{id}` | Smaže záznam podle GUID nebo indexu | `204 NoContent` / `404 Not Found` |

---

## 💻 Lokální vývoj

### Požadavky
- .NET 10.0 SDK
- PostgreSQL

### Spuštění
```bash
# 1. Obnova a build
dotnet build

# 2. Aplikace schématu do DB (migrace)
dotnet ef database update

# 3. Spuštění serveru
dotnet run
```
Aplikace běží primárně na adrese http://localhost:5129. Zahrnuje jak UI (Razor Pages), tak přístup ke Swaggeru na `/swagger`.

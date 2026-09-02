# ⚡ SignalR Global Chat & Person CRUD REST API (ASP.NET Core .NET 10)

Moderní webová aplikace postavená na **ASP.NET Core (.NET 10)** kombinující **SignalR Real-Time Chat** a plnohodnotné **REST API CRUD** pro správu osob s napojením na **PostgreSQL (Entity Framework Core & Render.com)**.

---

## 🌟 Hlavní funkce

- ⚡ **Real-time komunikace (SignalR / WebSockets)** – bleskové doručování zpráv všem připojeným klientům v reálném čase.
- 👥 **Person CRUD REST API** – kompletní správa osob s validacemi dat a asynchronním zpracováním.
- 🐘 **PostgreSQL & EF Core** – automatické mapování entit, podpora lokální i cloudové Render databáze (`DATABASE_URL` s automatickým SSL).
- 📜 **Swagger / OpenAPI** – interaktivní testovací rozhraní na `/swagger`.
- 🎨 **Moderní Pure CSS Glassmorphism** – animované ambientní světelné koule, matné skleněné karty a responzivní design pro mobily.
- 🚀 **Připraveno pro Render.com & Docker** – obsahuje multi-stage Dockerfile a `render.yaml`.

---

## 📁 Struktura projektu

```text
WebApplicationASP01/
├── App/
│   ├── AppDbContext.cs        # EF Core kontext mapující entitu Person na tabulku persons
│   ├── Person.cs              # Entita Person a DTO modely (CreatePersonDto, UpdatePersonDto)
│   ├── PersonService.cs       # Asynchronní servisní vrstva pro CRUD logiku
│   └── PersonsController.cs   # API Controller s REST endpointy (/api/persons)
├── Hubs/
│   └── ChatHub.cs             # SignalR Hub pro správu WebSocket spojení a rozesílání zpráv
├── Models/
│   └── ChatMessage.cs         # Záznam (record) pro formát chatové zprávy
├── Services/
│   └── ChatHistoryService.cs  # In-memory správa historie chatu
├── Pages/
│   ├── Index.cshtml           # Razor šablona chatu
│   ├── Index.cshtml.cs        # PageModel pro Index
│   └── Shared/_Layout.cshtml  # Hlavní layout
├── wwwroot/
│   ├── css/chat.css           # Pure CSS styly, animace, glassmorphism
│   └── js/chat.js             # SignalR klient, zvuky, notifikace
├── appsettings.json           # Konfigurace připojení k databázi
├── Dockerfile                 # Multi-stage Dockerfile (.NET 10)
├── render.yaml                # Render Blueprint pro nasazení
├── Program.cs                 # Konfigurace služeb, DB, Swaggeru a pipeline
└── WebApplicationASP01.csproj # Projektový soubor .NET 10
```

---

## 📡 REST API Dokumentace (Person CRUD)

Základní URL cesta: **`/api/persons`**  
Interaktivní testovací rozhraní (Swagger): **`http://localhost:5129/swagger`** (nebo v cloudu na `https://<vasedomena>.onrender.com/swagger`)

### Přehled endpointů:

| Metoda | URL | Popis | Návratový kód |
|---|---|---|---|
| `GET` | `/api/persons` | Získá seznam všech osob | `200 OK` |
| `GET` | `/api/persons/{id}` | Získá konkrétní osobu podle GUID | `200 OK` / `404 Not Found` |
| `POST` | `/api/persons` | Vytvoří novou osobu | `201 Created` / `400 Bad Request` |
| `PUT` | `/api/persons/{id}` | Upraví existující osobu podle GUID | `200 OK` / `400 Bad Request` / `404 Not Found` |
| `DELETE` | `/api/persons/{id}` | Smaže osobu podle GUID | `204 NoContent` / `404 Not Found` |
| `GET` | `/api/persons/ahoj` | Testovací uvítací zpráva | `200 OK` |

---

### 📋 JSON formáty a příklady

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
**URL parametr:** `{id}` = např. `3fa85f64-5717-4562-b3fc-2c963f66afa6`  
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

**Response (`200 OK`):**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
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

#### 4. Smazání osoby (`DELETE /api/persons/{id}`)
**Response (`204 NoContent`):**
*Tělo odpovědi je prázdné.*

---

## 🐘 Konfigurace databáze (PostgreSQL)

Aplikace podporuje standardní ADO.NET formát i cloudový URL formát (Render/Heroku).

### 1. Lokální konfigurace v `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "postgresql://postgresrender_stv7_user:JMNbY97TnCvqfvjXCfEVTgWsDSR0aQ4m@dpg-dabtsb3tqb8s73diao30-a.frankfurt-postgres.render.com/postgresrender_stv7"
  }
}
```

Nebo klasický formát:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=persondb;Username=postgres;Password=test"
  }
}
```

---

## ☁️ Nasazení na Render.com

1. **Uložte a odešlete kód na GitHub**:
   ```bash
   git add .
   git commit -m "Update Person CRUD and documentation"
   git push origin main
   ```

2. **Vytvořte Web Service na Render.com**:
   - Přejděte na [dashboard.render.com](https://dashboard.render.com/) -> **New +** -> **Web Service**.
   - Vyberte váš repozitář.
   - **Runtime**: `Docker`
   - **Region**: `Frankfurt (EU Central)` *(stejný jako vaše databáze)*
   - **Plan**: `Free`

3. **Nastavte proměnnou prostředí (Environment Variables)**:
   - **Klíč**: `DATABASE_URL`
   - **Hodnota**: URL vaší Render PostgreSQL databáze, např.:
     ```text
     postgresql://postgresrender_stv7_user:JMNbY97TnCvqfvjXCfEVTgWsDSR0aQ4m@dpg-dabtsb3tqb8s73diao30-a.frankfurt-postgres.render.com/postgresrender_stv7
     ```
     *(V rámci Renderu lze pro ještě vyšší rychlost použít i Internal Database URL z detailu databáze).*

4. **Spuštění**:
   - Klikněte na **Deploy Web Service**.
   - Render automaticky sestaví Docker kontejner, spustí `db.Database.EnsureCreated()` a vytvoří tabulky v PostgreSQL.

---

## 💻 Lokální spuštění

```bash
dotnet run --project WebApplicationASP01.csproj
```

- **Chat UI**: [http://localhost:5129](http://localhost:5129)
- **Swagger API**: [http://localhost:5129/swagger](http://localhost:5129/swagger)

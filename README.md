# ⚡ SignalR Global Chat (ASP.NET Core & Razor)

Moderní **real-time globální chatovací aplikace** postavená na **ASP.NET Core (.NET 10)**, **SignalR** a **Razor Pages** s prémiovým UI ve stylu **Glassmorphism v čistém CSS (Pure CSS)**. Umožňuje okamžitou komunikaci napříč jakýmikoli zařízeními (PC, mobil, tablet) bez nutnosti instalace jakékoli aplikace.

---

## 🌟 Hlavní funkce

- ⚡ **Real-time komunikace (SignalR / WebSockets)** – bleskové doručování zpráv všem připojeným klientům v reálném čase.
- 👥 **Živý čítač online uživatelů** – pulzující indikátor aktivně připojených zařízení.
- 📜 **Uchování historie zpráv v paměti** – nově připojené zařízení ihned vidí předchozí konverzaci.
- 🎨 **Moderní Pure CSS Glassmorphism** – animované ambientní světelné koule (orbs), matné skleněné karty (`backdrop-filter`), plynulé animace a typografie *Plus Jakarta Sans*.
- 🎲 **Dynamický avatar & Kostka náhodných jmen** – barva avatara generovaná z přezdívky, generátor náhodných cool jmen (*CyberNinja*, *NeonVoyager*...).
- 🔊 **Zvuková upozornění (Web Audio API)** – syntetizované příjemné tóny při odeslání a příjmu zprávy (s možností ztlumení 🔊/🔇).
- 📱 **100% responzivní design pro mobily** – optimalizováno pro dotykové ovládání, virtuální klávesnice (`100dvh`) a safe area insets.
- 🔗 **Tlačítko pro rychlé sdílení odkazu** – zkopíruje odkaz na chat do schránky s toast notifikací.
- 📋 **Kopírování zpráv kliknutím** – kliknutím na bublinu zprávy se text zkopíruje.
- 🚀 **Připraveno pro Render.com & Docker** – obsahuje optimalizovaný multi-stage Dockerfile a `render.yaml`.

---

## 📁 Struktura projektu

```text
WebApplicationASP01/
├── Hubs/
│   └── ChatHub.cs             # SignalR Hub pro správu WebSocket spojení a rozesílání zpráv
├── Models/
│   └── ChatMessage.cs         # Záznam (record) pro formát zprávy
├── Services/
│   └── ChatHistoryService.cs  # In-memory správa historie posledních zpráv
├── Pages/
│   ├── _ViewImports.cshtml    # Importy jmenných prostorů a TagHelperů
│   ├── _ViewStart.cshtml      # Nastavení výchozího layoutu pro Razor Pages
│   ├── Index.cshtml           # Razor šablona chatu
│   ├── Index.cshtml.cs        # PageModel pro Index
│   └── Shared/
│       └── _Layout.cshtml     # Hlavní HTML kostra s Google Fonts a meta tagy
├── wwwroot/
│   ├── css/
│   │   └── chat.css           # Kompletní Pure CSS styly, animace, glassmorphism
│   └── js/
│       ├── chat.js            # SignalR klient, správa avatarů, zvuky, toast notifikace
│       └── signalr.min.js     # Oficiální SignalR JS knihovna
├── Dockerfile                 # Multi-stage Dockerfile (.NET 10)
├── render.yaml                # Render Blueprint pro nasazení na 1 klik
├── Program.cs                 # Registrace služeb, SignalR, Razor Pages a portů
└── WebApplicationASP01.csproj # Projektový soubor .NET 10
```

---

## 💻 Lokální spuštění

### Požadavky:
- [.NET 10.0 SDK](https://dotnet.microsoft.com/download)

### 1. Klonování a přechod do složky:
```bash
cd WebApplicationASP01
```

### 2. Sestavení a spuštění:
```bash
dotnet run
```

Aplikace se spustí na adrese:
👉 [**`http://localhost:5129`**](http://localhost:5129)

### 3. Testování napříč zařízeními v lokální síti (Wi-Fi):
1. Zjistěte svou lokální IP adresu v síti (např. pomocí `ipconfig` ve Windows – např. `192.168.1.50`).
2. Otevřete na mobilním telefonu připojeném ke stejné Wi-Fi:
   ```text
   http://192.168.1.50:5129
   ```
3. Pište si mezi PC a mobilem v reálném čase!

---

## ☁️ Nasazení na Render.com

Aplikace je plně připravena pro bezplatný hosting na [Render.com](https://render.com) pomocí **Dockeru**.

### Možnost A: Nasazení přes GitHub (Doporučeno)

1. **Nahrajte projekt na GitHub**:
   ```bash
   git add .
   git commit -m "Initial commit for Render deployment"
   git push origin main
   ```

2. **Vytvořte Web Service na Render.com**:
   - Přihlaste se na [dashboard.render.com](https://dashboard.render.com/).
   - Klikněte na **New +** -> **Web Service**.
   - Propojte svůj GitHub repozitář s tímto projektem.
   - Nastavte parametry:
     - **Name**: `signalr-global-chat` (nebo libovolný název)
     - **Language / Runtime**: `Docker`
     - **Region**: `Frankfurt (EU)` (nebo vám nejbližší)
     - **Instance Type**: `Free`
   - Klikněte na **Deploy Web Service**.

3. Render automaticky sestaví Docker image a spustí aplikaci na vygenerované URL (např. `https://signalr-global-chat.onrender.com`).

---

### Možnost B: Nasazení přes Render Blueprint (`render.yaml`)

Projekt již obsahuje soubor `render.yaml`. Na Render.com stačí:
1. Přejít na **New +** -> **Blueprint**.
2. Zvolit váš GitHub repozitář.
3. Render automaticky načte konfiguraci z `render.yaml` a spustí nasazení.

---

## 🐳 Lokální spuštění přes Docker

Pokud chcete otestovat Docker kontejner lokálně:

```bash
# Sestavení image
docker build -t signalr-global-chat .

# Spuštění kontejneru na portu 8080
docker run -d -p 8080:8080 -e PORT=8080 --name chat-app signalr-global-chat
```

Aplikace bude dostupná na `http://localhost:8080`.

---

## 🛠️ Použité technologie

- **Backend**: ASP.NET Core 10.0, C#, Microsoft.AspNetCore.SignalR
- **Frontend**: Razor Pages, Pure Modern CSS (CSS Variables, Flexbox, Grid, Glassmorphism, Animations), Vanilla JavaScript (ES6+), Web Audio API
- **DevOps**: Docker (Multi-stage build), Render Blueprint YAML

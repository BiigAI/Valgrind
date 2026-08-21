# ValheimPortal - Web Management Portal Architecture & Plan

**ValheimPortal** is a standalone, server-side Valheim administration web portal. It compiles a modern React/Vite web application directly into a single self-contained BepInEx `.dll`, running an embedded HTTP server to provide live server management, real-time telemetry, and modular integrations for custom mods (e.g. *CharactersVault*, *Valgrind*, and future plugins).

---

## 1. Feasibility & Distribution

* **Single-File Zero-Dependency:** The React frontend (HTML, JS, CSS, assets) is compiled and embedded into the `.dll` via MSBuild `<EmbeddedResource Include="dist\**\*" />`.
* **Thunderstore Compliant:** Server administration tools and web dashboard mods (e.g., *ValheimServerDashboard*, *ValheimHTTP*, *DiscordConnector*, *ServerPanel*) are first-class, accepted packages on Thunderstore under the **Server-side**, **Tools**, and **Utilities** categories.
* **Server-Only Mod:** Connecting game clients do not need to install `ValheimPortal.dll`. It runs exclusively on the dedicated server.

---

## 2. High-Level Architecture

```
┌─────────────────────────────────────────────────────────────────────────┐
│                    Web Browser (Admin Client)                           │
│             React / Vite SPA Dashboard (Served from DLL)                │
└────────────────────────────────────┬────────────────────────────────────┘
                                     │ HTTP / REST / WebSockets (Port 8080)
┌────────────────────────────────────▼────────────────────────────────────┐
│                    ValheimPortal BepInEx Plugin                         │
├─────────────────────────────────────────────────────────────────────────┤
│ 1. Embedded HTTP Server & Static Asset Streamer (Memory-backed)         │
│ 2. JWT / Session Token Authentication Layer (Admin Password)            │
│ 3. Core REST Controllers (Players, Telemetry, Console, Lifecycle)       │
│ 4. Dynamic Module Discovery & Module SDK Registry                       │
└──────┬─────────────────────────────┬─────────────────────────────┬──────┘
       │ BepInEx Chainloader         │ In-Memory Hook              │ In-Memory Hook
┌──────▼─────────────────────┐ ┌─────▼───────────────────────┐ ┌───▼──────┐
│ Valheim Dedicated Server   │ │ CharactersVault (Module)    │ │ Valgrind │
│ (ZNet, ZDOs, Server Engine)│ │ [Online / Bound Players]    │ │ [Online] │
└────────────────────────────┘ └─────────────────────────────┘ └──────────┘
```

---

## 3. Dynamic Module Discovery Engine

The portal automatically detects which of your mods are installed and running, rendering them as **Active Modules** with dedicated tabs, or **Offline Modules** when absent.

### Detection Mechanism:
1. At startup, `ValheimPortal` queries `BepInEx.Bootstrap.Chainloader.PluginInfos`.
2. Matches registered mod GUIDs:
   * `com.charactervault.valheim` (CharactersVault)
   * `com.bigai.valgrind` (Valgrind)
   * *Future custom modules...*
3. Active mods can optionally register custom REST endpoints and data providers via the `IWebPortalModule` SDK interface.

### Extensible Module SDK (`IWebPortalModule`):
```csharp
public interface IWebPortalModule
{
    string ModuleGuid { get; }
    string DisplayName { get; }
    string Version { get; }
    string Description { get; }

    // Register custom REST API routes for this module
    void RegisterRoutes(IHttpRouteRegistry routes);

    // Provide real-time telemetry/status snapshot
    object GetStatusSnapshot();
}
```

---

## 4. Core Base Admin Suite (Out of the Box)

Even when no optional custom modules are installed, `ValheimPortal` delivers a complete server management suite:

| Feature Area | Capabilities |
| :--- | :--- |
| **Real-Time Player Manager** | Live player list with Steam/Platform IDs, latency/ping, world coordinates, and one-click kick/ban/unban actions. |
| **Live Telemetry & Metrics** | Real-time server FPS, tick rate, active ZNet peer count, memory usage, uptime, and active ZDO count. |
| **Interactive Console & Logs** | Live-streamed BepInEx console log viewer with interactive command execution bar (`save`, `kick`, `event`, etc.). |
| **World & Server Lifecycle** | Instant manual world save triggers, global server broadcast announcements (shouts), and scheduled restart warnings. |

---

## 5. Security & Authentication Model

* **Configurable Admin Password:** Configured in `BepInEx/config/ValheimPortal.cfg` (with automatic secure random password generation on first boot).
* **JWT / Session Token Auth:** Clean login modal in the React web app. Upon valid authentication, a secure session token is returned with configurable expiry.
* **Network Binding Control:** Configurable port (default: `8080`) and host binding (`0.0.0.0` for external/LAN access, or `127.0.0.1` for local/reverse-proxy/SSH-tunnel access).

---

## 6. Frontend Build & Packaging Pipeline

```
[React / TypeScript Frontend (Vite)]
             │
             ▼ npm run build
      [dist/ Bundle Files]
             │
             ▼ Embedded into .csproj
[<EmbeddedResource Include="dist\**\*" />]
             │
             ▼ dotnet build
      [ValheimPortal.dll] (Single file, 100% self-contained)
```

---

## 7. Implementation Roadmap

### Phase 1: Core C# Server Engine
1. Scaffold `ValheimPortal` BepInEx project with embedded C# `HttpListener`.
2. Implement embedded static asset streaming with correct MIME type detection and caching.
3. Build authentication controller with password verification and session token management.

### Phase 2: Base Admin REST APIs
1. **Players API:** `/api/players` (GET online players, POST kick, POST ban/unban).
2. **Server Metrics API:** `/api/server/metrics` (FPS, memory, uptime, ZDO count).
3. **Console API:** `/api/console/logs` (Live stream logs) and `/api/console/exec` (Execute server commands).
4. **Lifecycle API:** `/api/server/save`, `/api/server/broadcast`, `/api/server/restart`.

### Phase 3: Module Discovery Engine
1. Create `IWebPortalModule` contract and discovery scanner using `Chainloader.PluginInfos`.
2. Add module status endpoint `/api/modules` returning all detected vs offline modules.

### Phase 4: Modern React Web Frontend
1. Scaffold Vite + React + TypeScript web application with responsive dark theme.
2. Build login screen and authentication state management.
3. Build Core Dashboards:
   * Overview / Telemetry dashboard with live charts.
   * Player management table with action modals.
   * Interactive console log viewer.
   * Modules grid displaying Active and Offline modules.

### Phase 5: Packaging & Testing
1. Configure MSBuild build pipeline to auto-compile React assets into embedded resources.
2. Verify dedicated server startup, memory footprint, and network security.

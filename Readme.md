# CPPR Project

## Overview
This solution is a comprehensive web application demonstrating various ASP.NET Core technologies, including Web API, MVC, Blazor Server, Blazor WebAssembly, Identity (Keycloak), and SignalR.

## Prerequisites
- **.NET 8.0 SDK**
- **Docker Desktop** (or Docker Engine + Docker Compose)

## 1. Infrastructure Setup (Docker)

Before running the applications, you need to start the required infrastructure services: **PostgreSQL**, **Redis**, and **Keycloak**.

1.  Create a file named `compose.yaml` (or `docker-compose.yml`) in the root directory of the solution with the following content:

    ```yaml
    version: '3.8'

    services:
      postgres:
        image: postgres:latest
        container_name: postgres
        environment:
          POSTGRES_USER: admin
          POSTGRES_PASSWORD: 123456
          POSTGRES_DB: MenuDvb_V01
        ports:
          - "5432:5432"
        volumes:
          - postgres_data:/var/lib/postgresql/data

      redis:
        image: redis:latest
        container_name: redis
        ports:
          - "6379:6379"

      keycloak:
        image: quay.io/keycloak/keycloak:latest
        container_name: keycloak
        environment:
          KEYCLOAK_ADMIN: admin
          KEYCLOAK_ADMIN_PASSWORD: admin
        command: start-dev
        ports:
          - "8080:8080"

    volumes:
      postgres_data:
    ```

2.  Start the services:
    ```bash
    docker compose up -d
    ```

## 2. Keycloak Configuration

Once Keycloak is running, you need to configure the Realm and Clients.

1.  Open **[http://localhost:8080](http://localhost:8080)** in your browser.
2.  Login to the **Administration Console** with `admin` / `admin`.
3.  **Create Realm**:
    -   Click on the dropdown in the top-left corner and select **Create Realm**.
    -   Name: `myapp-reaam` (as configured in `appsettings.json`).
    -   Click **Create**.
4.  **Create Client for MVC UI**:
    -   Go to **Clients** -> **Create client**.
    -   Client ID: `myapp-web`.
    -   Click **Next**.
    -   **Capability config**: Ensure **Client authentication** is **On** (Confidential).
    -   Click **Next**.
    -   **Login settings**:
        -   Valid redirect URIs: `http://localhost:5197/*` (and `https://localhost:7197/*` if using HTTPS).
        -   Web origins: `+`.
    -   Click **Save**.
    -   Go to the **Credentials** tab and copy the **Client Secret**.
    -   Update `Project/appsettings.json` with this secret if necessary (though usually, it's set in User Secrets or env vars).
5.  **Create Client for Blazor WASM**:
    -   Go to **Clients** -> **Create client**.
    -   Client ID: `blazor-client`.
    -   Click **Next**.
    -   **Capability config**: **Client authentication** should be **Off** (Public).
    -   Click **Next**.
    -   **Login settings**:
        -   Valid redirect URIs: `http://localhost:5046/*`, `https://localhost:7157/*`, `authentication/login-callback`.
        -   Web origins: `+`.
    -   Click **Save**.
6.  **Create Users**:
    -   Go to **Users** -> **Add user**.
    -   Username: `user`.
    -   Click **Create**.
    -   Go to **Credentials** tab -> **Set password**.
    -   Password: `123`, Toggle **Temporary** to **Off**.
    -   Click **Save**.

## 3. Database Setup

Apply Entity Framework Core migrations to create the database schema.

1.  Open a terminal in the solution root.
2.  Run the migrations for the API project:
    ```bash
    cd CPPR.API
    dotnet ef database update
    ```

## 4. Running the Applications

You need to run multiple projects simultaneously. Open separate terminal windows for each.

### Backend API
```bash
cd CPPR.API
dotnet run
```
-   **URL**: `http://localhost:5002` (HTTP), `https://localhost:7002` (HTTPS)
-   **Swagger UI**: `http://localhost:5002/swagger`

### MVC / Razor Pages UI (Admin Panel)
```bash
cd Project
dotnet run
```
-   **URL**: `http://localhost:5197`

### Blazor WebAssembly Client
```bash
cd CPPR.BlazorWasm
dotnet run
```
-   **URL**: `http://localhost:5046`

## Troubleshooting

-   **Database Connection**: Ensure the `Postgres` connection string in `CPPR.API/appsettings.json` matches your Docker container settings.
-   **Keycloak Errors**: Verify the Realm name (`myapp-reaam`) and Client IDs match exactly what you created in Keycloak.
-   **CORS**: If the browser blocks requests, ensure the API allows the origins of the UI apps (`http://localhost:5197`, `http://localhost:5046`).

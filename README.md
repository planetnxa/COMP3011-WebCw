# Chez D'Arome — Fragrance Collection Manager

A full-stack ASP.NET Core Razor Pages application for discovering, collecting, and getting personalised recommendations for fragrances. Built on **.NET 10** with a **SQLite** database and a RESTful API documented via **Swagger**.

## Swagger / API Documentation

When running locally in Development mode, the interactive Swagger UI is available at:

```
https://localhost:7227/swagger
```

The raw OpenAPI specification can be found at:

```
https://localhost:7227/openapi/v1.json
```

## Features

### Fragrance Search & Discovery
Browse a database of fragrances and search by **name**, **brand**, **accord**, or **note**. Results include full details such as country of origin, gender, rating, year, accords, perfumers, and top/middle/base notes.

### User Accounts
Register and log in with a username and password. Passwords are securely hashed before storage.

### Fragrance Cabinet
Logged-in users can add fragrances to their personal cabinet — a curated collection of perfumes they own or are interested in — and attach comments to each entry.

### Preference Setup
First-time users are guided through a setup flow to select their fragrance preferences (e.g. favourite accords or notes), which are stored for future use.

### Personalised Recommendations
The Recommendations page uses saved user preferences to query the fragrance database and surface matching perfumes tailored to the user's taste.

## API Endpoints

The application exposes four REST API controllers, all accessible under `/api/`:

| Controller | Base Route | Description |
|---|---|---|
| **Frag** | `/api/Frag` | CRUD operations on fragrances, plus search by name, brand, accord, or note |
| **UserProfile** | `/api/UserProfile` | Manage user profiles (create, read, update, delete, lookup by username) |
| **UserCabinet** | `/api/UserCabinet` | Manage a user's fragrance cabinet entries |
| **UserPreference** | `/api/UserPreference` | Manage user scent preferences |

### Fragrance API (`/api/Frag`)

| Method | Route | Description |
|---|---|---|
| `GET` | `/api/Frag` | Get all fragrances |
| `GET` | `/api/Frag/{id}` | Get a fragrance by ID |
| `GET` | `/api/Frag/name/{name}` | Search fragrances by name |
| `GET` | `/api/Frag/brand/{brandName}` | Search fragrances by brand |
| `GET` | `/api/Frag/accord/{accord}` | Search fragrances by accord |
| `GET` | `/api/Frag/note/{note}` | Search fragrances by note |
| `POST` | `/api/Frag` | Create a new fragrance |
| `PUT` | `/api/Frag/{id}` | Update an existing fragrance |
| `DELETE` | `/api/Frag/{id}` | Delete a fragrance |

### User Profile API (`/api/UserProfile`)

| Method | Route | Description |
|---|---|---|
| `GET` | `/api/UserProfile` | Get all user profiles |
| `GET` | `/api/UserProfile/{id}` | Get a user profile by ID |
| `GET` | `/api/UserProfile/username/{username}` | Get a user profile by username |
| `POST` | `/api/UserProfile` | Register a new user |
| `PUT` | `/api/UserProfile/{id}` | Update a user profile |
| `DELETE` | `/api/UserProfile/{id}` | Delete a user profile |

### User Cabinet API (`/api/UserCabinet`)

| Method | Route | Description |
|---|---|---|
| `GET` | `/api/UserCabinet` | Get all cabinet entries |
| `GET` | `/api/UserCabinet/{id}` | Get a cabinet entry by ID |
| `GET` | `/api/UserCabinet/user/{userId}` | Get cabinet entries for a user |
| `POST` | `/api/UserCabinet` | Add a fragrance to a user's cabinet |
| `DELETE` | `/api/UserCabinet/{id}` | Remove a cabinet entry |

### User Preference API (`/api/UserPreference`)

| Method | Route | Description |
|---|---|---|
| `GET` | `/api/UserPreference` | Get all preferences |
| `GET` | `/api/UserPreference/{id}` | Get a preference by ID |
| `GET` | `/api/UserPreference/user/{userId}` | Get preferences for a user |
| `POST` | `/api/UserPreference` | Create a new preference |
| `PUT` | `/api/UserPreference/{id}` | Update a preference |
| `DELETE` | `/api/UserPreference/{id}` | Delete a preference |

## Tech Stack

- **Runtime:** .NET 10
- **Backend:** ASP.NET Core Razor Pages + API Controllers
- **Database:** SQLite (via `Microsoft.Data.Sqlite`)
- **API Docs:** Swagger / OpenAPI (Swashbuckle)
- **Frontend:** Bootstrap 5.3, Google Fonts (Gelasio)
- **Testing:** xUnit + `Microsoft.AspNetCore.Mvc.Testing` (integration tests with in-memory SQLite)

## Getting Started

1. **Clone the repository**
   ```bash
   git clone https://github.com/planetnxa/COMP3011-WebCw.git
   cd COMP3011-WebCw
   ```

2. **Run the application**
   ```bash
   dotnet run
   ```

3. **Open in your browser**
   - App: `https://localhost:7227`
   - Swagger: `https://localhost:7227/swagger`

4. **Run the tests**
   ```bash
   dotnet test
   ```

# SimpleInventory

Lightweight ASP.NET Core inventory app (Products, Categories) configured for local development with SQLite.

This README is short and specific to this repository.

Checklist
- Restore/build the solution
- Ensure the local SQLite DB exists (the app will create and seed it automatically)
- Run the web app and use Swagger or the MVC/UI to interact with products/categories

Prerequisites
- .NET SDK 8.0 (match the solution SDK)

Project highlights (what matters for running)
- The web project: `src/SimpleInventory.Web` (uses MVC controllers and serves a small admin UI)
- Database: SQLite via Entity Framework Core. Connection string is in `src/SimpleInventory.Web/appsettings.json` as `Data Source=inventory.db`.
- On startup the app calls `EnsureCreated()` and seeds sample data — no manual migrations required to get a runnable DB.
- Swagger UI is enabled and available at `/swagger` for quick API exploration.

Quick start (run locally)

From the repository root:

```powershell
dotnet restore
dotnet build
```

Run the web app (explicit project):

```powershell
dotnet run --project src/SimpleInventory.Web/SimpleInventory.Web.csproj
```

By default the project is configured to listen on these local URLs (see `Properties/launchSettings.json`):
- HTTP: http://localhost:5106

When the app starts it will:
- create `src/SimpleInventory.Web/inventory.db` if it doesn't exist and seed sample categories/products
- enable Swagger at `/swagger`

Use the UI or API
- Web UI (product management): http://localhost:5106/
- Swagger UI: http://localhost:5106/swagger

API examples (keep these simple)

Search products (pagination/search via MVC or API):

```http
GET http://localhost:5106/api/Products?q=monitor&page=1&pageSize=10
```

Create a product (example JSON body)

```http
POST http://localhost:5106/api/Products
Content-Type: application/json
X-Api-Key: super-secret-key

{
  "sku": "ELEC010",
  "name": "USB-C Dock",
  "price": 89.99,
  "quantity": 25,
  "categoryId": 1
}
```

Short curl (PowerShell) examples

```powershell
curl "http://localhost:5106/api/Products?q=monitor&page=1&pageSize=10"

curl -X POST "http://localhost:5106/api/Products" -H "Content-Type: application/json"   -H "X-Api-Key: super-secret-key" -d '{"sku":"ELEC010","name":"USB-C Dock","price":89.99,"quantity":25,"categoryId":1}'
```


Testing
- Run tests with:

```powershell
dotnet test
```


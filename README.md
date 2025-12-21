# Warehouse Backend API

A robust .NET-based RESTful API for warehouse management, featuring authentication, inventory tracking, section management, and voucher operations.

## 📋 Overview

This is a multi-layered ASP.NET Core Web API built with .NET 10.0, implementing clean architecture principles with separate projects for API, Data Access, Domain, and Entities.

## 🏗️ Architecture

The solution consists of four main projects:

- **Warehouse.Api** - Web API layer with controllers (Auth, Items, Sections, ItemVouchers), validators, extensions, and API configuration
- **Warehouse.DataAccess** - Data access layer with DbContext, entity configurations, Mapster mappings, EF Core migrations, and domain services
- **Warehouse.Domain** - Domain layer for business logic and domain models
- **Warehouse.Entities** - Entity models, DTOs, shared utilities, and response handling

## 🚀 Features

- **Authentication & Authorization** - JWT-based authentication with ASP.NET Core Identity
- **Items Management** - Full CRUD operations for warehouse inventory items
- **Sections Management** - Organize warehouse into logical sections with timestamps
- **Item Vouchers** - Track item movements, transactions, and audit trails
- **Request Validation** - FluentValidation for input validation on all API endpoints
- **Auto-mapping** - Mapster for efficient DTO-to-Entity mapping
- **Structured Logging** - Serilog with console and rolling file output
- **API Documentation** - Swagger/OpenAPI integration for interactive API exploration
- **Database Migrations** - Entity Framework Core migrations for version control

## 🛠️ Technology Stack

- **Framework**: .NET 10.0
- **ORM**: Entity Framework Core 10.0
- **Database**: SQL Server
- **Authentication**: ASP.NET Core Identity + JWT Bearer
- **Logging**: Serilog
- **Validation**: FluentValidation
- **Mapping**: Mapster
- **API Documentation**: Swagger/Swashbuckle

## 📦 Prerequisites

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- SQL Server (LocalDB, Express, or full version)
- Visual Studio 2022 or Visual Studio Code
- Git

## ⚙️ Configuration

### Database Connection

Update the connection string in `Warehouse.Api/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DevCS": "Server=.;Database=WarehouseDB;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
  },
  "ConnectionMode": "Dev"
}
```

### JWT Settings

Configure JWT settings in `appsettings.json`:

```json
{
  "JWT": {
    "SigningKey": "YourSecretKeyThatIsAtLeast32CharactersLong!",
    "Issuer": "WarehouseAPI",
    "Audience": "WarehouseClients",
    "ExpiryInMinutes": 60
  }
}
```

> ⚠️ **Security Note**: Always use a strong, unique signing key in production and store it securely (e.g., in Azure Key Vault or environment variables).

## 🚀 Getting Started

### 1. Clone the Repository

```bash
git clone <repository-url>
cd Warehouse-Backend
```

### 2. Restore Dependencies

```bash
dotnet restore
```

### 3. Update Database

Run Entity Framework migrations to create the database:

```bash
cd Warehouse.Api
dotnet ef database update
```

### 4. Run the Application

```bash
dotnet run --project Warehouse.Api
```

The API will be available at:
- HTTPS: `https://localhost:5001`
- HTTP: `http://localhost:5000`
- Swagger UI: `https://localhost:5001/swagger`

## 📚 API Endpoints

### Authentication
- `POST /api/Auth/register` - Register new user
- `POST /api/Auth/login` - User login (returns JWT token)

### Items
- `GET /api/Items` - Get all items
- `GET /api/Items/{id}` - Get item by ID
- `POST /api/Items` - Create new item
- `PUT /api/Items/{id}` - Update item
- `DELETE /api/Items/{id}` - Delete item

### Sections
- `GET /api/Sections` - Get all sections
- `GET /api/Sections/{id}` - Get section by ID
- `POST /api/Sections` - Create new section
- `PUT /api/Sections/{id}` - Update section
- `DELETE /api/Sections/{id}` - Delete section

### Item Vouchers
- `GET /api/ItemVouchers` - Get all vouchers
- `GET /api/ItemVouchers/{id}` - Get voucher by ID
- `POST /api/ItemVouchers` - Create new voucher
- `PUT /api/ItemVouchers/{id}` - Update voucher
- `DELETE /api/ItemVouchers/{id}` - Delete voucher

> 📖 For detailed API documentation, visit the Swagger UI at `/swagger` when running the application.

## 🗄️ Database Migrations

### Create a New Migration

```bash
cd Warehouse.Api
dotnet ef migrations add <MigrationName> --project ../Warehouse.DataAccess
```

### Update Database

```bash
dotnet ef database update
```

### Remove Last Migration

```bash
dotnet ef migrations remove --project ../Warehouse.DataAccess
```

## 📝 Logging

Logs are written to:
- **Console** - Real-time console output
- **File** - `Warehouse.Api/Logs/warehouse-log-YYYYMMDD.txt`
  - Rolling daily logs
  - Retains last 30 days

Configure logging levels in `appsettings.json`:

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning"
      }
    }
  }
}
```

## 🔐 Security

- JWT authentication required for protected endpoints
- Password hashing with ASP.NET Core Identity
- CORS configuration in `Program.cs`
- HTTPS enforced in production

## 🧪 Testing

```bash
# Run all tests
dotnet test

# Run tests with coverage
dotnet test /p:CollectCoverage=true
```

## 📁 Project Structure

```
Warehouse-Backend/
├── Warehouse.Api/                   # Web API layer
│   ├── Controllers/                 # API endpoints (Auth, Items, Sections, ItemVouchers)
│   ├── Extensions/                  # Service collection extensions
│   ├── Validators/                  # FluentValidation validators
│   ├── Logs/                        # Rolling daily application logs
│   ├── Properties/                  # Launch settings
│   ├── appsettings.json             # Configuration (development & production)
│   └── Program.cs                   # Application entry point
├── Warehouse.DataAccess/            # Data access layer
│   ├── ApplicationDbContext/        # EF Core DbContext (WarehouseDbContext)
│   ├── EntitiesConfigurations/      # Entity configurations
│   ├── Mappings/                    # Mapster mapping profiles (Auth, Item, ItemVoucher, Section)
│   ├── Migrations/                  # EF Core database migrations
│   ├── Services/                    # Domain services (AuthService, ItemService, ItemVoucherService, SectionService)
│   └── Extensions/                  # Data access service extensions
├── Warehouse.Entities/              # Entities and DTOs
│   ├── Entities/                    # Domain models (ApplicationUser, Item, ItemVoucher, Section)
│   ├── DTO/                         # Data transfer objects by entity
│   ├── Shared/                      # Response handling and shared models
│   └── Utilities/                   # Helper utilities
└── Warehouse-Backend.slnx           # Solution file
```

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.
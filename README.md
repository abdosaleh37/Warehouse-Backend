# Warehouse Backend API

A robust .NET-based RESTful API for warehouse management, featuring authentication, inventory tracking, categories, sections management, and voucher operations.

**🌐 Live API:** [http://shamtex-warehouse.runasp.net](http://shamtex-warehouse.runasp.net)  
**📖 API Documentation:** [http://shamtex-warehouse.runasp.net/swagger](http://shamtex-warehouse.runasp.net/swagger)

## 📋 Overview

This is a multi-layered ASP.NET Core Web API built with .NET 10.0, implementing clean architecture principles with separate projects for API, Data Access, and Entities.

## 🏗️ Architecture

The solution consists of three main projects:

- **Warehouse.Api** - Web API layer with controllers, validators, extensions, and API configuration
- **Warehouse.DataAccess** - Data access layer with DbContext, entity configurations, Mapster mappings, EF Core migrations, and domain services
- **Warehouse.Entities** - Entity models, DTOs, shared utilities, and response handling

## 🚀 Features

- **Authentication & Authorization** - JWT-based authentication with refresh tokens using ASP.NET Core Identity
- **Warehouse Management** - Multi-warehouse support per user
- **Categories Management** - Organize items into categories within warehouses
- **Items Management** - Full CRUD operations for warehouse inventory items
- **Sections Management** - Organize warehouse into logical sections
- **Item Vouchers** - Track item movements, transactions, and audit trails
- **Request Validation** - FluentValidation for input validation on all API endpoints
- **Auto-mapping** - Mapster for efficient DTO-to-Entity mapping
- **Structured Logging** - Serilog with console and rolling file output
- **API Documentation** - NSwag/OpenAPI integration for interactive API exploration
- **Database Migrations** - Entity Framework Core migrations for version control

## 🛠️ Technology Stack

| Category | Technology |
|----------|------------|
| Framework | .NET 10.0 |
| ORM | Entity Framework Core 10.0 |
| Database | SQL Server |
| Authentication | ASP.NET Core Identity + JWT Bearer |
| Logging | Serilog |
| Validation | FluentValidation |
| Mapping | Mapster |
| API Documentation | NSwag |

## 📦 Prerequisites

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- SQL Server (LocalDB, Express, or full version)
- Visual Studio 2022 or Visual Studio Code
- Git

## ⚙️ Configuration

### Environment Variables

The application uses the following configuration sections that should be set via environment variables or user secrets for production:

| Configuration | Description |
|--------------|-------------|
| `ConnectionStrings:DevCS` | SQL Server connection string for development |
| `ConnectionStrings:ProdCS` | SQL Server connection string for production |
| `ConnectionMode` | Set to `Dev` or `Prod` to select connection string |
| `JWT:SigningKey` | Secret key for JWT token signing (min 32 characters) |
| `JWT:Issuer` | JWT token issuer |
| `JWT:Audience` | JWT token audience |
| `JWT:AccessTokenExpiryInMinutes` | Access token expiration time |
| `JWT:RefreshTokenExpiryInDays` | Refresh token expiration time |


### Setting Up User Secrets (Development)

```bash
cd Warehouse.Api
dotnet user-secrets init
dotnet user-secrets set "JWT:SigningKey" "your-secret-key-here"
dotnet user-secrets set "ConnectionStrings:DevCS" "your-connection-string"
```

## 🚀 Getting Started

### 1. Clone the Repository

```bash
git clone https://github.com/yourusername/Warehouse-Backend.git
cd Warehouse-Backend
```

### 2. Configure the Application

Set up your configuration using user secrets or environment variables as described above.

### 3. Restore Dependencies

```bash
dotnet restore
```

### 4. Update Database

Run Entity Framework migrations to create the database:

```bash
cd Warehouse.Api
dotnet ef database update
```

### 5. Run the Application

```bash
dotnet run --project Warehouse.Api
```

The API will be available at the configured URLs. Access Swagger UI at `/swagger` for interactive API documentation.

## 📚 API Endpoints

**Base URL:** `http://shamtex-warehouse.runasp.net/api`

All endpoints except authentication require a valid JWT token in the `Authorization` header as `Bearer {token}`.

### Authentication
| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/Auth/register` | Register a new user |
| POST | `/api/Auth/login` | User login (returns JWT tokens) |
| POST | `/api/Auth/refresh-token` | Refresh access token |

### Categories
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/Categories` | Get all categories |
| GET | `/api/Categories/{id}` | Get category by ID |
| POST | `/api/Categories` | Create new category |
| PUT | `/api/Categories/{id}` | Update category |
| DELETE | `/api/Categories/{id}` | Delete category |

### Items
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/Items` | Get all items |
| GET | `/api/Items/{id}` | Get item by ID |
| POST | `/api/Items` | Create new item |
| PUT | `/api/Items/{id}` | Update item |
| DELETE | `/api/Items/{id}` | Delete item |

### Sections
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/Sections` | Get all sections |
| GET | `/api/Sections/{id}` | Get section by ID |
| POST | `/api/Sections` | Create new section |
| PUT | `/api/Sections/{id}` | Update section |
| DELETE | `/api/Sections/{id}` | Delete section |

### Item Vouchers
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/ItemVouchers` | Get all vouchers |
| GET | `/api/ItemVouchers/{id}` | Get voucher by ID |
| POST | `/api/ItemVouchers` | Create new voucher |
| PUT | `/api/ItemVouchers/{id}` | Update voucher |
| DELETE | `/api/ItemVouchers/{id}` | Delete voucher |

> 📖 For detailed API documentation including request/response schemas, visit the **[Live Swagger UI](http://shamtex-warehouse.runasp.net/swagger)**.

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

Logs are configured using Serilog with the following outputs:

- **Console** - Real-time console output for development
- **File** - Rolling daily log files
  - Location: `Warehouse.Api/Logs/`
  - Format: `warehouse-log-YYYYMMDD.txt`
  - Retention: 30 days

## 🔐 Security Best Practices

- JWT authentication required for all protected endpoints
- Refresh token rotation for enhanced security
- Password hashing with ASP.NET Core Identity
- CORS policy configured for allowed origins
- HTTPS enforced in production
- User secrets for development configuration
- Environment variables for production secrets

## 📁 Project Structure

```
Warehouse-Backend/
├── Warehouse.Api/                   # Web API layer
│   ├── Controllers/                 # API endpoints
│   │   ├── AuthController.cs
│   │   ├── CategoriesController.cs
│   │   ├── ItemsController.cs
│   │   ├── ItemVouchersController.cs
│   │   └── SectionsController.cs
│   ├── Extensions/                  # Service collection extensions
│   ├── Validators/                  # FluentValidation validators
│   │   ├── Auth/
│   │   ├── Category/
│   │   ├── Item/
│   │   ├── ItemVoucher/
│   │   └── Section/
│   ├── Properties/                  # Launch settings
│   └── Program.cs                   # Application entry point
│
├── Warehouse.DataAccess/            # Data access layer
│   ├── ApplicationDbContext/        # EF Core DbContext
│   ├── EntitiesConfigurations/      # Entity type configurations
│   ├── Mappings/                    # Mapster mapping profiles
│   ├── Migrations/                  # EF Core migrations
│   ├── Services/                    # Domain services
│   │   ├── AuthService/
│   │   ├── CategoryService/
│   │   ├── ItemService/
│   │   ├── ItemVoucherService/
│   │   ├── SectionService/
│   │   └── TokenService/
│   └── Extensions/                  # DI extensions
│
├── Warehouse.Entities/              # Entities and DTOs
│   ├── Entities/                    # Domain models
│   │   ├── ApplicationUser.cs
│   │   ├── Category.cs
│   │   ├── Item.cs
│   │   ├── ItemVoucher.cs
│   │   ├── Section.cs
│   │   ├── UserRefreshToken.cs
│   │   └── Warehouse.cs
│   ├── DTO/                         # Data transfer objects
│   ├── Shared/                      # Response handling
│   └── Utilities/                   # Helper utilities
│
└── Warehouse-Backend.slnx           # Solution file
```
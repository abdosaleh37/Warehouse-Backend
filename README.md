# 📦 Warehouse Backend API

<div align="center">

**A robust, production-ready .NET RESTful API for comprehensive warehouse management**

Featuring authentication, inventory tracking, categories, sections management, and voucher operations

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](#)
[![API Status](https://img.shields.io/badge/API-Live-success)](http://shamtex-warehouse.runasp.net)

**🌐 Live API:** [http://shamtex-warehouse.runasp.net](http://shamtex-warehouse.runasp.net)  
**📖 Interactive Docs:** [Swagger UI](http://shamtex-warehouse.runasp.net/swagger)  
**🎨 Frontend App:** [https://warehouse-frontend-beige.vercel.app](https://warehouse-frontend-beige.vercel.app)

</div>

---

## 📋 Overview

This is a multi-layered ASP.NET Core Web API built with .NET 10.0, designed for reliable warehouse operations. It follows clean architecture with separate projects for API, Data Access, and Entities, providing a scalable foundation for inventory workflows.

## 🏗️ Architecture

Built on **clean architecture** principles with clear separation of concerns:

<table>
<tr>
<td width="33%">

### 🌐 Warehouse.Api
**Presentation Layer**
- RESTful Controllers
- Request Validators
- Middleware & Filters
- Dependency Injection
- API Configuration

</td>
<td width="33%">

### 💾 Warehouse.DataAccess
**Data Layer**
- EF Core DbContext
- Entity Configurations
- Repository Pattern
- Mapster Mappings
- Domain Services
- Migrations

</td>
<td width="33%">

### 📐 Warehouse.Entities
**Domain Layer**
- Entity Models
- DTOs
- Response Wrappers
- Shared Utilities
- Business Logic

</td>
</tr>
</table>

## 🚀 Features

### ✅ Core Business Capabilities

- **User Authentication & Security**
  - JWT access tokens with refresh tokens (ASP.NET Core Identity)
  - Secure login, registration, and token refresh flows

- **Warehouse Management**
  - Multi-warehouse support per user

- **Inventory Operations**
  - Items CRUD with lookup and search
  - Barcode and name-based search
  - Fetch items by section and by voucher activity period

- **Categories & Sections**
  - Category management for organizing item types
  - Section management for physical warehouse layout

- **Item Vouchers & Movements**
  - Track item movements and transactions
  - Maintain audit trails for item activity

### 🛡️ Reliability & Engineering Features

- **Validation Layer** - FluentValidation on all request models
- **Auto-Mapping** - Mapster DTO ↔ Entity mapping
- **Structured Logging** - Serilog with rolling daily logs
- **Database Migrations** - EF Core migration support
- **OpenAPI Docs** - NSwag-powered Swagger UI

## 🛠️ Technology Stack

<table>
<tr>
<td>

### Backend Framework
- **.NET 10.0** - Latest .NET runtime
- **ASP.NET Core** - Web API framework
- **C# 13** - Modern language features

</td>
<td>

### Data & Persistence
- **Entity Framework Core 10.0** - ORM
- **SQL Server** - Relational database
- **EF Core Migrations** - Schema versioning

</td>
</tr>
<tr>
<td>

### Security & Auth
- **ASP.NET Core Identity** - User management
- **JWT Bearer Tokens** - Stateless auth
- **Refresh Tokens** - Token rotation

</td>
<td>

### Quality & Tooling
- **FluentValidation** - Input validation
- **Mapster** - Object mapping
- **Serilog** - Structured logging
- **NSwag** - OpenAPI/Swagger

</td>
</tr>
</table>

## 📦 Prerequisites

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- SQL Server (LocalDB, Express, or full version)
- Visual Studio 2022 or Visual Studio Code
- Git

## ⚙️ Configuration

### 🔐 Environment Variables

Configure the application using environment variables or user secrets:

| Configuration | Type | Description |
|--------------|------|-------------|
| `ConnectionStrings:DevCS` | String | SQL Server connection string (Development) |
| `ConnectionStrings:ProdCS` | String | SQL Server connection string (Production) |
| `ConnectionMode` | Enum | `Dev` or `Prod` - Selects active connection |
| `JWT:SigningKey` | String | Secret key for JWT signing (≥32 chars) ⚠️ |
| `JWT:Issuer` | String | Token issuer identifier |
| `JWT:Audience` | String | Token audience identifier |
| `JWT:AccessTokenExpiryInMinutes` | Integer | Access token lifetime (e.g., 60) |
| `JWT:RefreshTokenExpiryInDays` | Integer | Refresh token lifetime (e.g., 30) |

> ⚠️ **Security Note:** Never commit secrets to source control. Use user secrets for development and secure vaults for production.

### 🔧 Setting Up User Secrets (Development)

```bash
# Navigate to API project
cd Warehouse.Api

# Initialize user secrets
dotnet user-secrets init

# Configure JWT settings
dotnet user-secrets set "JWT:SigningKey" "your-super-secret-key-minimum-32-characters"
dotnet user-secrets set "JWT:Issuer" "WarehouseAPI"
dotnet user-secrets set "JWT:Audience" "WarehouseClients"

# Configure database connection
dotnet user-secrets set "ConnectionStrings:DevCS" "Server=(localdb)\\mssqllocaldb;Database=WarehouseDB;Trusted_Connection=True;"
dotnet user-secrets set "ConnectionMode" "Dev"
```

## 🚀 Getting Started

### Step 1️⃣: Clone the Repository

```bash
git clone https://github.com/yourusername/Warehouse-Backend.git
cd Warehouse-Backend
```

### Step 2️⃣: Configure the Application

Set up your configuration using user secrets or environment variables as described in the [Configuration](#️-configuration) section above.

### Step 3️⃣: Restore Dependencies

```bash
# Restore all NuGet packages
dotnet restore
```

### Step 4️⃣: Apply Database Migrations

Create and seed the database using Entity Framework migrations:

```bash
cd Warehouse.Api

# Apply all pending migrations
dotnet ef database update

# Verify migration status
dotnet ef migrations list
```

### Step 5️⃣: Run the Application

```bash
# Run in development mode
dotnet run --project Warehouse.Api

# Or run with watch (auto-reload on changes)
dotnet watch run --project Warehouse.Api
```

### Step 6️⃣: Access the API

- **API Base URL:** `https://localhost:7xxx` or `http://localhost:5xxx`
- **Swagger UI:** Navigate to `/swagger` for interactive API documentation
- **Health Check:** `/api/health` (if configured)

> 💡 **Tip:** Check the console output for the exact URLs after starting the application.

## 📚 API Endpoints

**Base URL:** `http://shamtex-warehouse.runasp.net/api`

> 🔒 **Authorization Required:** All endpoints except authentication require a valid JWT token in the `Authorization` header:
> ```
> Authorization: Bearer {your-access-token}
> ```

### 🔐 Authentication

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| `POST` | `/api/Auth/register` | Register a new user account | ❌ |
| `POST` | `/api/Auth/login` | Authenticate user and receive JWT tokens | ❌ |
| `POST` | `/api/Auth/refresh-token` | Obtain new access token using refresh token | ❌ |

### 📁 Categories

Manage item categories for organization and classification.

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| `GET` | `/api/Categories` | Retrieve all categories for authenticated user | ✅ |
| `GET` | `/api/Categories/{id}` | Retrieve specific category by ID | ✅ |
| `POST` | `/api/Categories` | Create a new category | ✅ |
| `PUT` | `/api/Categories/{id}` | Update existing category | ✅ |
| `DELETE` | `/api/Categories/{id}` | Delete category (if not in use) | ✅ |

### 📦 Items

Manage inventory items with full CRUD operations, search, and filtering.

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| `GET` | `/api/Items/section/{id}` | Retrieve all items in a specific section | ✅ |
| `GET` | `/api/Items/{id}` | Retrieve specific item by ID | ✅ |
| `GET` | `/api/Items/vouchers/{year}/{month}` | Get items with voucher activity for specified period | ✅ |
| `GET` | `/api/Items/search?searchTerm={term}` | Search items by name or barcode | ✅ |
| `POST` | `/api/Items` | Create a new inventory item | ✅ |
| `PUT` | `/api/Items/{id}` | Update existing item details | ✅ |
| `DELETE` | `/api/Items/{id}` | Delete item from inventory | ✅ |

### 🏪 Sections

Organize warehouse layout into logical sections for better inventory management.

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| `GET` | `/api/Sections` | Retrieve all sections in warehouse | ✅ |
| `GET` | `/api/Sections/{id}` | Retrieve specific section by ID | ✅ |
| `POST` | `/api/Sections` | Create a new warehouse section | ✅ |
| `PUT` | `/api/Sections/{id}` | Update existing section | ✅ |
| `DELETE` | `/api/Sections/{id}` | Delete section (if empty) | ✅ |

### 📝 Item Vouchers

Track item movements, transactions, and maintain comprehensive audit trails.

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| `GET` | `/api/ItemVouchers` | Retrieve all vouchers for user | ✅ |
| `GET` | `/api/ItemVouchers/{id}` | Retrieve specific voucher by ID | ✅ |
| `POST` | `/api/ItemVouchers` | Create new item movement voucher | ✅ |
| `PUT` | `/api/ItemVouchers/{id}` | Update existing voucher | ✅ |
| `DELETE` | `/api/ItemVouchers/{id}` | Delete voucher record | ✅ |

---

> 📖 **For detailed API documentation** including request/response schemas, data models, and interactive testing, visit the **[Live Swagger UI](http://shamtex-warehouse.runasp.net/swagger)**.

## 🗄️ Database Migrations

Entity Framework Core migrations provide version control for your database schema.

### 📝 Create a New Migration

```bash
cd Warehouse.Api

# Create migration with descriptive name
dotnet ef migrations add AddItemBarcodeField --project ../Warehouse.DataAccess

# View the generated migration files in Warehouse.DataAccess/Migrations/
```

### ⬆️ Apply Migrations to Database

```bash
# Apply all pending migrations
dotnet ef database update

# Apply specific migration
dotnet ef database update <MigrationName>

# Revert to specific migration
dotnet ef database update <PreviousMigrationName>
```

### 📋 List All Migrations

```bash
# View migration history
dotnet ef migrations list
```

### ❌ Remove Last Migration

```bash
# Remove the most recent unapplied migration
dotnet ef migrations remove --project ../Warehouse.DataAccess
```

### 🔄 Generate Migration Script

```bash
# Generate SQL script for migrations
dotnet ef migrations script --output ./Scripts/migrations.sql
```

## 📝 Logging

Structured logging powered by **Serilog** for comprehensive application monitoring.

### 📊 Log Outputs

<table>
<tr>
<td width="50%">

#### 🖥️ Console Sink
- Real-time output for development
- Colored log levels
- Detailed exception formatting
- Template: `[{Timestamp:HH:mm:ss} {Level:u3}] {Message}{NewLine}{Exception}`

</td>
<td width="50%">

#### 📁 File Sink (Rolling)
- Daily log file rotation
- **Location:** `Warehouse.Api/Logs/`
- **Format:** `warehouse-log-YYYYMMDD.txt`
- **Retention:** 30 days (auto-cleanup)
- **Max Size:** 10MB per file

</td>
</tr>
</table>

### 🎯 Log Levels

| Level | Usage | Example |
|-------|-------|----------|
| `Verbose` | Trace-level debugging | Loop iterations, variable states |
| `Debug` | Development diagnostics | Method entry/exit, debug info |
| `Information` | General flow | Request received, processing complete |
| `Warning` | Unexpected but handled | Deprecated API used, retry attempt |
| `Error` | Recoverable errors | Validation failed, external API error |
| `Fatal` | Critical failures | Database unavailable, startup failure |

### 📍 Log Location

```
Warehouse.Api/Logs/
├── warehouse-log-20260202.txt  (today)
├── warehouse-log-20260201.txt
├── warehouse-log-20260131.txt
└── ...
```

## 🔐 Security Best Practices

This API implements industry-standard security measures:

### 🛡️ Authentication & Authorization

- ✅ **JWT Bearer Tokens** - Stateless authentication mechanism
- ✅ **Refresh Token Rotation** - Automatic token refresh with rotation
- ✅ **Password Hashing** - ASP.NET Core Identity with secure hashing (PBKDF2)
- ✅ **Role-Based Access** - Authorization policies per endpoint

### 🌐 Network Security

- ✅ **HTTPS Enforcement** - TLS 1.2+ required in production
- ✅ **CORS Configuration** - Restricted cross-origin access
- ✅ **Rate Limiting** - Protection against abuse (if configured)

### 🔒 Data Protection

- ✅ **Input Validation** - FluentValidation on all requests
- ✅ **SQL Injection Prevention** - Parameterized queries via EF Core
- ✅ **XSS Protection** - Output encoding

### 🔑 Secret Management

- ✅ **User Secrets** - Development environment (never committed)
- ✅ **Environment Variables** - Production secrets
- ✅ **Azure Key Vault** - Recommended for production (configurable)

### 📋 Security Headers

```
X-Content-Type-Options: nosniff
X-Frame-Options: DENY
X-XSS-Protection: 1; mode=block
Strict-Transport-Security: max-age=31536000
```

## 📁 Project Structure

```
Warehouse-Backend/
├── 🌐 Warehouse.Api/                   # Presentation Layer (Web API)
│   ├── Controllers/                    # RESTful API Controllers
│   │   ├── AuthController.cs           # Authentication & authorization endpoints
│   │   ├── CategoriesController.cs     # Category management endpoints
│   │   ├── ItemsController.cs          # Inventory item CRUD & search
│   │   ├── ItemVouchersController.cs   # Item movement tracking
│   │   ├── SectionsController.cs       # Warehouse section management
│   │   └── ErrorController.cs          # Global error handling
│   │
│   ├── Validators/                     # FluentValidation Request Validators
│   │   ├── Auth/                       # Login, Register, RefreshToken validators
│   │   ├── Category/                   # Category DTO validators
│   │   ├── Item/                       # Item DTO validators
│   │   ├── ItemVoucher/                # ItemVoucher DTO validators
│   │   └── Section/                    # Section DTO validators
│   │
│   ├── Extensions/                     # Dependency Injection Extensions
│   │   └── ApiServiceCollectionExtensions.cs
│   │
│   ├── Properties/                     # Development Settings
│   │   └── launchSettings.json         # Kestrel launch configuration
│   │
│   ├── Logs/                           # Serilog Rolling Logs
│   │   └── warehouse-log-YYYYMMDD.txt
│   │
│   ├── appsettings.json                # Base configuration
│   ├── appsettings.Development.json    # Development overrides
│   ├── appsettings.Production.json     # Production overrides
│   ├── Program.cs                      # Application entry point & middleware
│   └── Warehouse.Api.csproj            # Project file
│
├── 💾 Warehouse.DataAccess/            # Data Access Layer
│   ├── ApplicationDbContext/           # EF Core Database Context
│   │   └── ApplicationDbContext.cs     # DbContext with DbSets
│   │
│   ├── EntitiesConfigurations/         # Fluent API Entity Configurations
│   │   ├── ApplicationUserConfiguration.cs
│   │   ├── CategoryConfiguration.cs
│   │   ├── ItemConfiguration.cs
│   │   ├── ItemVoucherConfiguration.cs
│   │   ├── SectionConfiguration.cs
│   │   ├── UserRefreshTokenConfiguration.cs
│   │   └── WarehouseConfiguration.cs
│   │
│   ├── Mappings/                       # Mapster Object Mappings
│   │   └── MappingConfig.cs            # DTO ↔ Entity mappings
│   │
│   ├── Migrations/                     # EF Core Migrations (Version Control)
│   │   ├── 20YYMMDDHHMMSS_InitialCreate.cs
│   │   └── ApplicationDbContextModelSnapshot.cs
│   │
│   ├── Services/                       # Domain Business Logic Services
│   │   ├── AuthService/                # User authentication & registration
│   │   ├── CategoryService/            # Category business logic
│   │   ├── ItemService/                # Item operations & search
│   │   ├── ItemVoucherService/         # Voucher management
│   │   ├── SectionService/             # Section operations
│   │   └── TokenService/               # JWT & refresh token generation
│   │
│   ├── Projections/                    # Query projections & specifications
│   ├── Extensions/                     # Data access DI extensions
│   └── Warehouse.DataAccess.csproj     # Project file
│
├── 📦 Warehouse.Entities/              # Domain Layer (Entities & DTOs)
│   ├── Entities/                       # Core Domain Models
│   │   ├── ApplicationUser.cs          # User entity (Identity)
│   │   ├── Category.cs                 # Item category
│   │   ├── Item.cs                     # Inventory item
│   │   ├── ItemVoucher.cs              # Item movement/transaction
│   │   ├── Section.cs                  # Warehouse section
│   │   ├── UserRefreshToken.cs         # Refresh token storage
│   │   └── Warehouse.cs                # Warehouse entity
│   │
│   ├── DTO/                            # Data Transfer Objects
│   │   ├── Auth/                       # Authentication DTOs
│   │   ├── Category/                   # Category request/response DTOs
│   │   ├── Item/                       # Item request/response DTOs
│   │   ├── ItemVoucher/                # Voucher request/response DTOs
│   │   └── Section/                    # Section request/response DTOs
│   │
│   ├── Shared/                         # Common Response Wrappers
│   │   ├── ApiResponse.cs              # Standardized API response
│   │   └── Result.cs                   # Operation result wrapper
│   │
│   ├── Utilities/                      # Helper Utilities & Extensions
│   └── Warehouse.Entities.csproj       # Project file
│
└── 📄 Warehouse-Backend.slnx            # Visual Studio Solution File
```
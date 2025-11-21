# FreshMarket API

A comprehensive API solution for managing fresh market store operations, inventory, and customer interactions. This project follows a clean architecture pattern with a layered approach to ensure scalability, maintainability, and testability.

## 📋 Table of Contents

- [Project Overview](#project-overview)
- [Architecture](#architecture)
- [Project Structure](#project-structure)
- [Technology Stack](#technology-stack)
- [Getting Started](#getting-started)
- [Project Layers](#project-layers)
- [Features](#features)
- [Contributing](#contributing)
- [License](#license)

## 🎯 Project Overview

FreshMarket is a robust backend API built with .NET Core that provides comprehensive functionality for managing:
- Product inventory and catalog management
- Store operations and inventory tracking
- Customer order management and interactions
- Real-time market data and pricing
- Store analytics and reporting

## 🏗️ Architecture

This project implements a **Clean Architecture** pattern with clear separation of concerns:

```
┌─────────────────────────────────────┐
│   API Layer (Controllers)           │
├─────────────────────────────────────┤
│   Service Layer (Business Logic)    │
├─────────────────────────────────────┤
│   Domain Layer (Entities & Rules)   │
├─────────────────────────────────────┤
│   Infrastructure Layer (Data Access)│
├─────────────────────────────────────┤
│   Shared Layer (Common Utilities)   │
└─────────────────────────────────────┘
```

## 📁 Project Structure

```
FreshMarket/
├── FreshMarket.sln                    # Solution file
├── FreshMarket.Domain/                # Domain layer
│   ├── Entities/                      # Business entities
│   ├── Interfaces/                    # Repository contracts
│   ├── Exceptions/                    # Custom exceptions
│   └── ValueObjects/                  # Value objects
├── FreshMarket.Infrastructure/        # Infrastructure layer
│   ├── Persistence/                   # Database configuration
│   ├── Repositories/                  # Repository implementations
│   ├── External Services/             # Third-party integrations
│   └── Migrations/                    # Database migrations
├── FreshMarket.Service/               # Business/Service layer
│   ├── Services/                      # Business logic implementations
│   ├── DTOs/                          # Data transfer objects
│   ├── Mappings/                      # AutoMapper configurations
│   └── Validators/                    # Input validators
├── FreshMarket.API/                   # API layer
│   ├── Controllers/                   # API endpoints
│   ├── Middleware/                    # Custom middleware
│   ├── Program.cs                     # Application entry point
│   ├── appsettings.json               # Configuration
│   └── appsettings.Development.json   # Development settings
└── FreshMarket.Shared/                # Shared utilities
    ├── Constants/                     # Application constants
    ├── Enums/                         # Enumerations
    ├── Helpers/                       # Utility helpers
    ├── Exceptions/                    # Shared exceptions
    └── Extensions/                    # Extension methods
```

## 🛠️ Technology Stack

- **Framework**: .NET Core 8 (or latest)
- **Language**: C# 12
- **Database**: SQL Server / PostgreSQL
- **ORM**: Entity Framework Core
- **API**: ASP.NET Core Web API
- **Mapping**: AutoMapper
- **Validation**: FluentValidation
- **Logging**: Serilog / NLog
- **Testing**: xUnit / NUnit
- **API Documentation**: Swagger/OpenAPI

## 🚀 Getting Started

### Prerequisites

- .NET SDK 8.0 or higher
- SQL Server or PostgreSQL
- Visual Studio 2022 or VS Code

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/Laith-AlShamasneh/FreshMarket.git
   cd FreshMarket
   ```

2. **Restore NuGet packages**
   ```bash
   dotnet restore
   ```

3. **Update database connection**
   - Update `appsettings.json` with your database connection string
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=YOUR_SERVER;Database=FreshMarket;Trusted_Connection=true;"
     }
   }
   ```

4. **Apply migrations**
   ```bash
   dotnet ef database update
   ```

5. **Run the application**
   ```bash
   dotnet run --project FreshMarket.API
   ```

6. **Access Swagger UI**
   - Navigate to: `https://localhost:5001/swagger`

## 📦 Project Layers

### Domain Layer (FreshMarket.Domain)
The core business logic and domain models:
- Contains entities that represent business concepts
- Defines repository interfaces
- Contains business rules and validations
- Independent of any framework or external dependencies
- **Responsibility**: Define what the business does

### Infrastructure Layer (FreshMarket.Infrastructure)
Data access and external service implementations:
- Entity Framework Core DbContext configuration
- Repository pattern implementations
- Database migrations
- Third-party service integrations
- Dependency injection configurations
- **Responsibility**: How the business data is persisted and accessed

### Service/Business Layer (FreshMarket.Service)
Business logic and orchestration:
- Service classes implementing business operations
- Data Transfer Objects (DTOs)
- AutoMapper profiles for entity mapping
- Input validation using FluentValidation
- Business logic that uses repositories from Infrastructure
- **Responsibility**: Orchestrate domain logic and coordinate between layers

### API Layer (FreshMarket.API)
REST API controllers and endpoints:
- ASP.NET Core controllers
- Route definitions and HTTP methods
- Request/Response handling
- Authentication and authorization
- API middleware and filters
- Swagger/OpenAPI documentation
- **Responsibility**: Expose business functionality as REST endpoints

### Shared Layer (FreshMarket.Shared)
Common utilities and cross-cutting concerns:
- Shared constants and enumerations
- Helper utility classes
- Extension methods
- Common exceptions
- Shared DTOs or base classes
- **Responsibility**: Provide reusable components across all layers

## ✨ Features

- ✅ RESTful API endpoints for fresh market operations
- ✅ Clean architecture with clear separation of concerns
- ✅ Repository pattern for data access abstraction
- ✅ Dependency injection configuration
- ✅ Entity Framework Core with database migrations
- ✅ Input validation and comprehensive error handling
- ✅ API documentation with Swagger/OpenAPI
- ✅ Comprehensive logging and monitoring
- ✅ Database transaction support
- ✅ CORS support for frontend integration
- ✅ Scalable and maintainable codebase

## 🤝 Contributing

Contributions are welcome! Please follow these guidelines:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📝 License

This project is licensed under the MIT License - see the LICENSE file for details.

---

**Last Updated**: 2025-11-21  
**Maintained by**: Laith-AlShamasneh

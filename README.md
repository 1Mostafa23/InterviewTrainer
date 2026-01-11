# 🎯 Interview Trainer API

A modern, production-ready RESTful API built with .NET 9.0 following Clean Architecture principles. This application helps users prepare for technical interviews by creating and managing interactive interview sessions with scoring and feedback capabilities.

---

## 📋 Table of Contents

- [Features](#-features)
- [Technology Stack](#-technology-stack)
- [Architecture](#-architecture)
- [Project Structure](#-project-structure)
- [Getting Started](#-getting-started)
- [API Documentation](#-api-documentation)
- [Database](#-database)
- [Testing](#-testing)
- [Docker Setup](#-docker-setup)
- [Development Guidelines](#-development-guidelines)
- [Contributing](#-contributing)

---

## ✨ Features

- **Session Management**: Create, start, and complete interview sessions
- **State Management**: Robust state machine for session lifecycle (Started → InProgress → Completed/Cancelled)
- **Domain-Driven Design**: Rich domain model with business logic encapsulation
- **Clean Architecture**: Clear separation of concerns across layers
- **RESTful API**: Standard HTTP methods with proper status codes
- **Entity Framework Core**: Code-first migrations with PostgreSQL
- **Swagger Integration**: Interactive API documentation
- **Comprehensive Testing**: Unit tests with xUnit and Moq

---

## 🛠 Technology Stack

### Backend Framework
- **.NET 9.0** - Latest LTS version with performance improvements
- **ASP.NET Core Web API** - High-performance web framework
- **C# 12** - Modern language features (records, pattern matching, nullable reference types)

### Database & ORM
- **PostgreSQL 16** - Robust, open-source relational database
- **Entity Framework Core 9.0** - Modern ORM with code-first approach
- **Npgsql.EntityFrameworkCore.PostgreSQL 9.0** - PostgreSQL provider for EF Core

### API Documentation
- **Swashbuckle.AspNetCore 10.1.0** - Swagger/OpenAPI integration

### Testing
- **xUnit 2.9.2** - Modern testing framework
- **Moq 4.20.72** - Mocking library for unit tests
- **Microsoft.NET.Test.Sdk 17.12.0** - Test SDK
- **coverlet.collector 6.0.2** - Code coverage tool

### Development Tools
- **Docker & Docker Compose** - Containerization for PostgreSQL
- **Entity Framework Core Tools** - Database migrations

### Architecture Patterns
- **Clean Architecture** - Domain, Application, Infrastructure, Presentation layers
- **Repository Pattern** - Data access abstraction
- **Dependency Injection** - Built-in .NET DI container
- **DTO Pattern** - Data transfer objects for API contracts
- **Value Objects** - Immutable domain values (SessionStatus)
- **Aggregate Root** - Domain entity encapsulation (InterviewSession)

---

## 🏗 Architecture

This project follows **Clean Architecture** principles, ensuring:

- **Independence from frameworks**: Business logic doesn't depend on external libraries
- **Testability**: Business logic can be tested without UI, database, or web server
- **Independence from UI**: Business logic is independent of the presentation layer
- **Independence from database**: Business logic doesn't depend on database implementation
- **Independence from external services**: Business logic is isolated from external concerns

### Layer Responsibilities

```
┌─────────────────────────────────────────────────────────┐
│                  Presentation Layer                     │
│  ┌──────────────────────────────────────────────────┐  │
│  │  Controllers (InterviewController)                │  │
│  │  - HTTP request/response handling                 │  │
│  │  - Input validation                               │  │
│  │  - Status code mapping                            │  │
│  └──────────────────────────────────────────────────┘  │
└───────────────────────┬─────────────────────────────────┘
                        │ depends on
                        ↓
┌─────────────────────────────────────────────────────────┐
│                 Application Layer                       │
│  ┌──────────────────────────────────────────────────┐  │
│  │  Services (InterviewService)                      │  │
│  │  - Use case orchestration                         │  │
│  │  - DTO mapping                                    │  │
│  │  ┌────────────────────────────────────────────┐  │  │
│  │  │  Interfaces (IInterviewRepository)         │  │  │
│  │  │  - Repository contracts                     │  │  │
│  │  └────────────────────────────────────────────┘  │  │
│  │  ┌────────────────────────────────────────────┐  │  │
│  │  │  DTOs (Data Transfer Objects)              │  │  │
│  │  │  - API contracts                            │  │  │
│  │  └────────────────────────────────────────────┘  │  │
│  └──────────────────────────────────────────────────┘  │
└───────────────────────┬─────────────────────────────────┘
                        │ depends on
                        ↓
┌─────────────────────────────────────────────────────────┐
│                    Domain Layer                         │
│  ┌──────────────────────────────────────────────────┐  │
│  │  Entities (InterviewSession - Aggregate Root)    │  │
│  │  - Business logic                                │  │
│  │  - State transitions                             │  │
│  │  - Invariants enforcement                        │  │
│  │  ┌────────────────────────────────────────────┐  │  │
│  │  │  Value Objects (SessionStatus)             │  │  │
│  │  │  - Immutable values                        │  │  │
│  │  └────────────────────────────────────────────┘  │  │
│  │  ┌────────────────────────────────────────────┐  │  │
│  │  │  Domain Exceptions (DomainException)       │  │  │
│  │  │  - Business rule violations                │  │  │
│  │  └────────────────────────────────────────────┘  │  │
│  └──────────────────────────────────────────────────┘  │
└───────────────────────┬─────────────────────────────────┘
                        │ implemented by
                        ↓
┌─────────────────────────────────────────────────────────┐
│              Infrastructure Layer                       │
│  ┌──────────────────────────────────────────────────┐  │
│  │  Persistence (InterviewRepository)               │  │
│  │  - EF Core implementation                       │  │
│  │  - Database operations                          │  │
│  │  ┌────────────────────────────────────────────┐  │  │
│  │  │  DbContext (AppDbContext)                  │  │  │
│  │  │  - Entity configuration                    │  │  │
│  │  │  - Value object conversions                │  │  │
│  │  └────────────────────────────────────────────┘  │  │
│  └──────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────┘
```

### Dependency Flow

- **Domain**: No dependencies (pure business logic)
- **Application**: Depends only on Domain
- **Infrastructure**: Depends on Application and Domain
- **Presentation**: Depends on Application (not directly on Infrastructure)

---

## 📁 Project Structure

```
InterviewTrainer/
├── InterviewTrainer.Api/                    # Main API project
│   ├── Controllers/                         # API Controllers
│   │   └── InterviewController.cs          # Interview endpoints
│   ├── Domain/                              # Domain layer (business logic)
│   │   ├── InterviewSession.cs             # Aggregate root entity
│   │   ├── SessionStatus.cs                # Value object (state machine)
│   │   └── DomainException.cs              # Domain-specific exceptions
│   ├── Application/                         # Application layer (use cases)
│   │   ├── DTOs/                           # Data Transfer Objects
│   │   │   ├── InterviewSessionDto.cs
│   │   │   ├── StartSessionRequest.cs
│   │   │   └── CompleteSessionRequest.cs
│   │   ├── Interfaces/                     # Application contracts
│   │   │   ├── IInterviewService.cs
│   │   │   └── IInterviewRepository.cs
│   │   └── Services/                       # Application services
│   │       └── InterviewService.cs
│   ├── Infrastructure/                     # Infrastructure layer
│   │   ├── Persistence/                    # Data access
│   │   │   └── InterviewRepository.cs      # EF Core repository
│   │   ├── Migrations/                     # EF Core migrations
│   │   │   └── 20260106104049_InitialCreate.cs
│   │   └── AppDbContext.cs                 # EF Core DbContext
│   ├── Program.cs                          # Application entry point
│   ├── appsettings.json                    # Configuration
│   └── InterviewTrainer.Api.csproj         # Project file
│
├── InterviewTrainer.Tests/                 # Test project
│   ├── Controllers/
│   │   └── InterviewControllerTests.cs
│   ├── Domain/
│   │   └── InterviewSessionTests.cs
│   ├── Services/
│   │   └── InterviewServiceTests.cs
│   └── InterviewTrainer.Tests.csproj
│
├── docker/                                 # Docker configuration
│   └── docker-compose.yml                  # PostgreSQL container
│
├── InterviewTrainer.sln                    # Solution file
├── README.md                                # This file
└── .gitignore                              # Git ignore rules
```

---

##  Getting Started

### Prerequisites

- **.NET 9.0 SDK** - [Download](https://dotnet.microsoft.com/download/dotnet/9.0)
- **Docker Desktop** (for PostgreSQL) - [Download](https://www.docker.com/products/docker-desktop)
- **PostgreSQL 16** (optional, if not using Docker)
- **IDE**: Visual Studio 2022, Rider, or VS Code with C# extension

### Installation Steps

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd startup
   ```

2. **Start PostgreSQL with Docker**
   ```bash
   cd docker
   docker-compose up -d
   ```
   This will start PostgreSQL on port `5433` with:
   - Database: `interview_trainer`
   - Username: `interview_user`
   - Password: `interview_pass`

3. **Restore dependencies**
   ```bash
   dotnet restore
   ```

4. **Apply database migrations**
   ```bash
   cd InterviewTrainer.Api
   dotnet ef database update
   ```
   > **Note**: If EF Core tools are not installed globally:
   > ```bash
   > dotnet tool install --global dotnet-ef
   > ```

5. **Run the application**
   ```bash
   dotnet run --project InterviewTrainer.Api
   ```

6. **Access the API**
   - API: `http://localhost:5000` or `https://localhost:5001`
   - Swagger UI: `http://localhost:5000/swagger`

### Configuration

Update `appsettings.json` or `appsettings.Development.json` to customize:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5433;Database=interview_trainer;Username=interview_user;Password=interview_pass"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

---

## 📚 API Documentation

### Base URL
```
http://localhost:5000/api
```

### Endpoints

#### 1. Start Interview Session
Creates a new interview session.

**POST** `/api/Interview/Start`

**Request Body:**
```json
{
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
}
```

**Response:** `201 Created`
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "status": "Started",
  "createdAt": "2024-01-06T10:40:49Z",
  "finishedAt": null,
  "score": null,
  "summary": null,
  "tips": null
}
```

#### 2. Get Session by ID
Retrieves an interview session by its ID.

**GET** `/api/Interview/{id}`

**Response:** `200 OK`
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "status": "Started",
  "createdAt": "2024-01-06T10:40:49Z",
  "finishedAt": null,
  "score": null,
  "summary": null,
  "tips": null
}
```

**Response:** `404 Not Found`
```json
{
  "error": "Сессия с ID {id} не найдена"
}
```

#### 3. Start Interview
Transitions session from `Started` to `InProgress` status.

**PATCH** `/api/Interview/{id}/start`

**Response:** `200 OK`
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "status": "InProgress",
  "createdAt": "2024-01-06T10:40:49Z",
  "finishedAt": null,
  "score": null,
  "summary": null,
  "tips": null
}
```

**Response:** `400 Bad Request` (invalid state transition)
```json
{
  "error": "Нельзя сменить статус с Completed на InProgress"
}
```

#### 4. Complete Session
Completes an interview session with results.

**PATCH** `/api/Interview/{id}/complete`

**Request Body:**
```json
{
  "score": 85,
  "summary": "Good performance overall, strong technical skills",
  "tips": "Work on system design questions"
}
```

**Response:** `200 OK`
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "status": "Completed",
  "createdAt": "2024-01-06T10:40:49Z",
  "finishedAt": "2024-01-06T11:30:00Z",
  "score": 85,
  "summary": "Good performance overall, strong technical skills",
  "tips": "Work on system design questions"
}
```

### Status Codes

- `200 OK` - Successful request
- `201 Created` - Resource created successfully
- `400 Bad Request` - Invalid request or business rule violation
- `404 Not Found` - Resource not found
- `500 Internal Server Error` - Server error

### Session Status State Machine

```
Started → InProgress → Completed
   ↓           ↓
Cancelled   Cancelled
```

**Valid Transitions:**
- `Started` → `InProgress` or `Cancelled`
- `InProgress` → `Completed` or `Cancelled`
- `Completed` → (terminal state)
- `Cancelled` → (terminal state)

---

## 🗄 Database

### Schema

**InterviewSessions Table:**
```sql
CREATE TABLE "InterviewSessions" (
    "Id" UUID PRIMARY KEY,
    "UserId" UUID NOT NULL,
    "Status" VARCHAR(20) NOT NULL,
    "CreatedAt" TIMESTAMP NOT NULL,
    "FinishedAt" TIMESTAMP NULL,
    "Score" INTEGER NULL,
    "Summary" TEXT NULL,
    "Tips" TEXT NULL
);
```

### Migrations

**Create a new migration:**
```bash
dotnet ef migrations add <MigrationName> --project InterviewTrainer.Api
```

**Apply migrations:**
```bash
dotnet ef database update --project InterviewTrainer.Api
```

**Rollback migration:**
```bash
dotnet ef database update <PreviousMigrationName> --project InterviewTrainer.Api
```

### Value Object Conversion

`SessionStatus` is stored as a string in the database but converted to/from the value object using EF Core's value converter:

```csharp
entity.Property(e => e.Status)
    .HasConversion(
        status => status.Value,
        value => SessionStatus.FromValue(value))
```

---

## 🧪 Testing

### Running Tests

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover

# Run specific test project
dotnet test InterviewTrainer.Tests
```

### Test Structure

Tests follow the **AAA (Arrange-Act-Assert)** pattern:

```csharp
[Fact]
public async Task StartSession_ValidRequest_ReturnsSessionDto()
{
    // Arrange
    var repository = new Mock<IInterviewRepository>();
    var service = new InterviewService(repository.Object);
    
    // Act
    var result = await service.StartSessionAsync(new StartSessionRequest { UserId = Guid.NewGuid() });
    
    // Assert
    Assert.NotNull(result);
    Assert.Equal("Started", result.Status);
}
```

### Test Coverage

- **Domain Tests**: Business logic, state transitions, invariants
- **Service Tests**: Use case orchestration, DTO mapping
- **Controller Tests**: HTTP handling, status codes, error responses

---

## 🐳 Docker Setup

### PostgreSQL Container

The `docker/docker-compose.yml` file defines a PostgreSQL 16 container:

```yaml
services:
  postgres:
    image: postgres:16
    container_name: interview-postgres
    environment:
      POSTGRES_DB: interview_trainer
      POSTGRES_USER: interview_user
      POSTGRES_PASSWORD: interview_pass
    ports:
      - "5433:5432"
    volumes:
      - postgres_data:/var/lib/postgresql/data
```

**Commands:**
```bash
# Start container
docker-compose -f docker/docker-compose.yml up -d

# Stop container
docker-compose -f docker/docker-compose.yml down

# View logs
docker-compose -f docker/docker-compose.yml logs -f

# Remove volumes (clean database)
docker-compose -f docker/docker-compose.yml down -v
```

---

## 📖 Development Guidelines

### Code Style

- **C# Naming Conventions**: PascalCase for classes, methods, properties; camelCase for parameters, fields with `_` prefix
- **Async/Await**: All I/O operations use async/await pattern
- **Nullable Reference Types**: Enabled project-wide
- **Records**: Used for DTOs and value objects (immutability)

### Best Practices

1. **Domain Logic**: Keep business rules in domain entities, not in services
2. **Dependency Injection**: Use constructor injection, register services in `Program.cs`
3. **Error Handling**: Use domain exceptions for business rule violations
4. **State Management**: Encapsulate state transitions in domain entities
5. **DTO Mapping**: Keep mapping logic in application services
6. **Repository Pattern**: Abstract data access behind interfaces

### Adding New Features

1. **Domain Layer**: Add entities, value objects, or domain exceptions
2. **Application Layer**: Define DTOs, interfaces, and implement services
3. **Infrastructure Layer**: Implement repository interfaces
4. **Presentation Layer**: Add controllers and endpoints
5. **Tests**: Write unit tests for each layer

---

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

---

## 📄 License

This project is licensed under the MIT License.

---

---

# 🎯 Interview Trainer API

Современный, готовый к продакшену RESTful API, построенный на .NET 9.0 с использованием принципов Чистой Архитектуры. Это приложение помогает пользователям готовиться к техническим собеседованиям, создавая и управляя интерактивными сессиями интервью с системой оценки и обратной связи.

---

## 📋 Содержание

- [Возможности](#-возможности)
- [Технологический стек](#-технологический-стек)
- [Архитектура](#-архитектура)
- [Структура проекта](#-структура-проекта)
- [Начало работы](#-начало-работы)
- [Документация API](#-документация-api)
- [База данных](#-база-данных)
- [Тестирование](#-тестирование)
- [Настройка Docker](#-настройка-docker)
- [Руководство по разработке](#-руководство-по-разработке)
- [Участие в проекте](#-участие-в-проекте)

---

## ✨ Возможности

- **Управление сессиями**: Создание, запуск и завершение сессий интервью
- **Управление состоянием**: Надежная машина состояний для жизненного цикла сессии (Started → InProgress → Completed/Cancelled)
- **Domain-Driven Design**: Богатая доменная модель с инкапсуляцией бизнес-логики
- **Чистая Архитектура**: Четкое разделение ответственности между слоями
- **RESTful API**: Стандартные HTTP методы с корректными статус-кодами
- **Entity Framework Core**: Миграции code-first с PostgreSQL
- **Интеграция Swagger**: Интерактивная документация API
- **Комплексное тестирование**: Модульные тесты с xUnit и Moq

---

## 🛠 Технологический стек

### Backend Framework
- **.NET 9.0** - Последняя LTS версия с улучшениями производительности
- **ASP.NET Core Web API** - Высокопроизводительный веб-фреймворк
- **C# 12** - Современные возможности языка (records, pattern matching, nullable reference types)

### База данных и ORM
- **PostgreSQL 16** - Надежная, open-source реляционная база данных
- **Entity Framework Core 9.0** - Современный ORM с подходом code-first
- **Npgsql.EntityFrameworkCore.PostgreSQL 9.0** - Провайдер PostgreSQL для EF Core

### Документация API
- **Swashbuckle.AspNetCore 10.1.0** - Интеграция Swagger/OpenAPI

### Тестирование
- **xUnit 2.9.2** - Современный фреймворк тестирования
- **Moq 4.20.72** - Библиотека моков для модульных тестов
- **Microsoft.NET.Test.Sdk 17.12.0** - Test SDK
- **coverlet.collector 6.0.2** - Инструмент покрытия кода

### Инструменты разработки
- **Docker & Docker Compose** - Контейнеризация для PostgreSQL
- **Entity Framework Core Tools** - Миграции базы данных

### Архитектурные паттерны
- **Чистая Архитектура** - Слои Domain, Application, Infrastructure, Presentation
- **Паттерн Repository** - Абстракция доступа к данным
- **Внедрение зависимостей** - Встроенный DI контейнер .NET
- **Паттерн DTO** - Объекты передачи данных для контрактов API
- **Value Objects** - Неизменяемые доменные значения (SessionStatus)
- **Aggregate Root** - Инкапсуляция доменных сущностей (InterviewSession)

---

## 🏗 Архитектура

Этот проект следует принципам **Чистой Архитектуры**, обеспечивая:

- **Независимость от фреймворков**: Бизнес-логика не зависит от внешних библиотек
- **Тестируемость**: Бизнес-логику можно тестировать без UI, базы данных или веб-сервера
- **Независимость от UI**: Бизнес-логика независима от слоя представления
- **Независимость от базы данных**: Бизнес-логика не зависит от реализации базы данных
- **Независимость от внешних сервисов**: Бизнес-логика изолирована от внешних зависимостей

### Ответственность слоев

```
┌─────────────────────────────────────────────────────────┐
│              Слой представления (Presentation)           │
│  ┌──────────────────────────────────────────────────┐  │
│  │  Контроллеры (InterviewController)              │  │
│  │  - Обработка HTTP запросов/ответов              │  │
│  │  - Валидация входных данных                     │  │
│  │  - Маппинг статус-кодов                         │  │
│  └──────────────────────────────────────────────────┘  │
└───────────────────────┬─────────────────────────────────┘
                        │ зависит от
                        ↓
┌─────────────────────────────────────────────────────────┐
│            Слой приложения (Application)                │
│  ┌──────────────────────────────────────────────────┐  │
│  │  Сервисы (InterviewService)                      │  │
│  │  - Оркестрация use cases                        │  │
│  │  - Маппинг DTO                                  │  │
│  │  ┌────────────────────────────────────────────┐  │  │
│  │  │  Интерфейсы (IInterviewRepository)        │  │  │
│  │  │  - Контракты репозиториев                  │  │  │
│  │  └────────────────────────────────────────────┘  │  │
│  │  ┌────────────────────────────────────────────┐  │  │
│  │  │  DTOs (Объекты передачи данных)           │  │  │
│  │  │  - Контракты API                          │  │  │
│  │  └────────────────────────────────────────────┘  │  │
│  └──────────────────────────────────────────────────┘  │
└───────────────────────┬─────────────────────────────────┘
                        │ зависит от
                        ↓
┌─────────────────────────────────────────────────────────┐
│              Доменный слой (Domain)                     │
│  ┌──────────────────────────────────────────────────┐  │
│  │  Сущности (InterviewSession - Aggregate Root)   │  │
│  │  - Бизнес-логика                                │  │
│  │  - Переходы состояний                           │  │
│  │  - Обеспечение инвариантов                      │  │
│  │  ┌────────────────────────────────────────────┐  │  │
│  │  │  Value Objects (SessionStatus)             │  │  │
│  │  │  - Неизменяемые значения                    │  │  │
│  │  └────────────────────────────────────────────┘  │  │
│  │  ┌────────────────────────────────────────────┐  │  │
│  │  │  Доменные исключения (DomainException)    │  │  │
│  │  │  - Нарушения бизнес-правил                │  │  │
│  │  └────────────────────────────────────────────┘  │  │
│  └──────────────────────────────────────────────────┘  │
└───────────────────────┬─────────────────────────────────┘
                        │ реализуется
                        ↓
┌─────────────────────────────────────────────────────────┐
│        Инфраструктурный слой (Infrastructure)          │
│  ┌──────────────────────────────────────────────────┐  │
│  │  Персистентность (InterviewRepository)          │  │
│  │  - Реализация через EF Core                      │  │
│  │  - Операции с базой данных                       │  │
│  │  ┌────────────────────────────────────────────┐  │  │
│  │  │  DbContext (AppDbContext)                  │  │  │
│  │  │  - Конфигурация сущностей                  │  │  │
│  │  │  - Конвертация value objects               │  │  │
│  │  └────────────────────────────────────────────┘  │  │
│  └──────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────┘
```

### Поток зависимостей

- **Domain**: Нет зависимостей (чистая бизнес-логика)
- **Application**: Зависит только от Domain
- **Infrastructure**: Зависит от Application и Domain
- **Presentation**: Зависит от Application (не напрямую от Infrastructure)

---

## 📁 Структура проекта

```
InterviewTrainer/
├── InterviewTrainer.Api/                    # Основной API проект
│   ├── Controllers/                         # API Контроллеры
│   │   └── InterviewController.cs          # Эндпоинты интервью
│   ├── Domain/                              # Доменный слой (бизнес-логика)
│   │   ├── InterviewSession.cs             # Aggregate root сущность
│   │   ├── SessionStatus.cs                # Value object (машина состояний)
│   │   └── DomainException.cs             # Доменные исключения
│   ├── Application/                         # Слой приложения (use cases)
│   │   ├── DTOs/                           # Объекты передачи данных
│   │   │   ├── InterviewSessionDto.cs
│   │   │   ├── StartSessionRequest.cs
│   │   │   └── CompleteSessionRequest.cs
│   │   ├── Interfaces/                     # Контракты приложения
│   │   │   ├── IInterviewService.cs
│   │   │   └── IInterviewRepository.cs
│   │   └── Services/                       # Сервисы приложения
│   │       └── InterviewService.cs
│   ├── Infrastructure/                     # Инфраструктурный слой
│   │   ├── Persistence/                    # Доступ к данным
│   │   │   └── InterviewRepository.cs      # Репозиторий EF Core
│   │   ├── Migrations/                     # Миграции EF Core
│   │   │   └── 20260106104049_InitialCreate.cs
│   │   └── AppDbContext.cs                 # EF Core DbContext
│   ├── Program.cs                          # Точка входа приложения
│   ├── appsettings.json                    # Конфигурация
│   └── InterviewTrainer.Api.csproj         # Файл проекта
│
├── InterviewTrainer.Tests/                 # Тестовый проект
│   ├── Controllers/
│   │   └── InterviewControllerTests.cs
│   ├── Domain/
│   │   └── InterviewSessionTests.cs
│   ├── Services/
│   │   └── InterviewServiceTests.cs
│   └── InterviewTrainer.Tests.csproj
│
├── docker/                                 # Конфигурация Docker
│   └── docker-compose.yml                  # Контейнер PostgreSQL
│
├── InterviewTrainer.sln                    # Файл решения
├── README.md                                # Этот файл
└── .gitignore                              # Правила игнорирования Git
```

---

## 🚀 Начало работы

### Требования

- **.NET 9.0 SDK** - [Скачать](https://dotnet.microsoft.com/download/dotnet/9.0)
- **Docker Desktop** (для PostgreSQL) - [Скачать](https://www.docker.com/products/docker-desktop)
- **PostgreSQL 16** (опционально, если не используете Docker)
- **IDE**: Visual Studio 2022, Rider или VS Code с расширением C#

### Шаги установки

1. **Клонируйте репозиторий**
   ```bash
   git clone <repository-url>
   cd startup
   ```

2. **Запустите PostgreSQL с Docker**
   ```bash
   cd docker
   docker-compose up -d
   ```
   Это запустит PostgreSQL на порту `5433` с:
   - База данных: `interview_trainer`
   - Пользователь: `interview_user`
   - Пароль: `interview_pass`

3. **Восстановите зависимости**
   ```bash
   dotnet restore
   ```

4. **Примените миграции базы данных**
   ```bash
   cd InterviewTrainer.Api
   dotnet ef database update
   ```
   > **Примечание**: Если инструменты EF Core не установлены глобально:
   > ```bash
   > dotnet tool install --global dotnet-ef
   > ```

5. **Запустите приложение**
   ```bash
   dotnet run --project InterviewTrainer.Api
   ```

6. **Откройте API**
   - API: `http://localhost:5000` или `https://localhost:5001`
   - Swagger UI: `http://localhost:5000/swagger`

### Конфигурация

Обновите `appsettings.json` или `appsettings.Development.json` для настройки:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5433;Database=interview_trainer;Username=interview_user;Password=interview_pass"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

---

## 📚 Документация API

### Базовый URL
```
http://localhost:5000/api
```

### Эндпоинты

#### 1. Создать сессию интервью
Создает новую сессию интервью.

**POST** `/api/Interview/Start`

**Тело запроса:**
```json
{
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
}
```

**Ответ:** `201 Created`
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "status": "Started",
  "createdAt": "2024-01-06T10:40:49Z",
  "finishedAt": null,
  "score": null,
  "summary": null,
  "tips": null
}
```

#### 2. Получить сессию по ID
Получает сессию интервью по её ID.

**GET** `/api/Interview/{id}`

**Ответ:** `200 OK`
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "status": "Started",
  "createdAt": "2024-01-06T10:40:49Z",
  "finishedAt": null,
  "score": null,
  "summary": null,
  "tips": null
}
```

**Ответ:** `404 Not Found`
```json
{
  "error": "Сессия с ID {id} не найдена"
}
```

#### 3. Начать интервью
Переводит сессию из статуса `Started` в `InProgress`.

**PATCH** `/api/Interview/{id}/start`

**Ответ:** `200 OK`
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "status": "InProgress",
  "createdAt": "2024-01-06T10:40:49Z",
  "finishedAt": null,
  "score": null,
  "summary": null,
  "tips": null
}
```

**Ответ:** `400 Bad Request` (недопустимый переход состояния)
```json
{
  "error": "Нельзя сменить статус с Completed на InProgress"
}
```

#### 4. Завершить сессию
Завершает сессию интервью с результатами.

**PATCH** `/api/Interview/{id}/complete`

**Тело запроса:**
```json
{
  "score": 85,
  "summary": "Хорошая работа в целом, сильные технические навыки",
  "tips": "Поработайте над вопросами по системному дизайну"
}
```

**Ответ:** `200 OK`
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "status": "Completed",
  "createdAt": "2024-01-06T10:40:49Z",
  "finishedAt": "2024-01-06T11:30:00Z",
  "score": 85,
  "summary": "Хорошая работа в целом, сильные технические навыки",
  "tips": "Поработайте над вопросами по системному дизайну"
}
```

### Статус-коды

- `200 OK` - Успешный запрос
- `201 Created` - Ресурс успешно создан
- `400 Bad Request` - Неверный запрос или нарушение бизнес-правил
- `404 Not Found` - Ресурс не найден
- `500 Internal Server Error` - Ошибка сервера

### Машина состояний сессии

```
Started → InProgress → Completed
   ↓           ↓
Cancelled   Cancelled
```

**Допустимые переходы:**
- `Started` → `InProgress` или `Cancelled`
- `InProgress` → `Completed` или `Cancelled`
- `Completed` → (терминальное состояние)
- `Cancelled` → (терминальное состояние)

---

## 🗄 База данных

### Схема

**Таблица InterviewSessions:**
```sql
CREATE TABLE "InterviewSessions" (
    "Id" UUID PRIMARY KEY,
    "UserId" UUID NOT NULL,
    "Status" VARCHAR(20) NOT NULL,
    "CreatedAt" TIMESTAMP NOT NULL,
    "FinishedAt" TIMESTAMP NULL,
    "Score" INTEGER NULL,
    "Summary" TEXT NULL,
    "Tips" TEXT NULL
);
```

### Миграции

**Создать новую миграцию:**
```bash
dotnet ef migrations add <MigrationName> --project InterviewTrainer.Api
```

**Применить миграции:**
```bash
dotnet ef database update --project InterviewTrainer.Api
```

**Откатить миграцию:**
```bash
dotnet ef database update <PreviousMigrationName> --project InterviewTrainer.Api
```

### Конвертация Value Object

`SessionStatus` хранится как строка в базе данных, но конвертируется в/из value object с помощью value converter EF Core:

```csharp
entity.Property(e => e.Status)
    .HasConversion(
        status => status.Value,
        value => SessionStatus.FromValue(value))
```

---

## 🧪 Тестирование

### Запуск тестов

```bash
# Запустить все тесты
dotnet test

# Запустить с покрытием кода
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover

# Запустить конкретный тестовый проект
dotnet test InterviewTrainer.Tests
```

### Структура тестов

Тесты следуют паттерну **AAA (Arrange-Act-Assert)**:

```csharp
[Fact]
public async Task StartSession_ValidRequest_ReturnsSessionDto()
{
    // Arrange
    var repository = new Mock<IInterviewRepository>();
    var service = new InterviewService(repository.Object);
    
    // Act
    var result = await service.StartSessionAsync(new StartSessionRequest { UserId = Guid.NewGuid() });
    
    // Assert
    Assert.NotNull(result);
    Assert.Equal("Started", result.Status);
}
```

### Покрытие тестами

- **Доменные тесты**: Бизнес-логика, переходы состояний, инварианты
- **Тесты сервисов**: Оркестрация use cases, маппинг DTO
- **Тесты контроллеров**: Обработка HTTP, статус-коды, обработка ошибок

---

## 🐳 Настройка Docker

### Контейнер PostgreSQL

Файл `docker/docker-compose.yml` определяет контейнер PostgreSQL 16:

```yaml
services:
  postgres:
    image: postgres:16
    container_name: interview-postgres
    environment:
      POSTGRES_DB: interview_trainer
      POSTGRES_USER: interview_user
      POSTGRES_PASSWORD: interview_pass
    ports:
      - "5433:5432"
    volumes:
      - postgres_data:/var/lib/postgresql/data
```

**Команды:**
```bash
# Запустить контейнер
docker-compose -f docker/docker-compose.yml up -d

# Остановить контейнер
docker-compose -f docker/docker-compose.yml down

# Просмотреть логи
docker-compose -f docker/docker-compose.yml logs -f

# Удалить volumes (очистить базу данных)
docker-compose -f docker/docker-compose.yml down -v
```

---

## 📖 Руководство по разработке

### Стиль кода

- **Соглашения именования C#**: PascalCase для классов, методов, свойств; camelCase для параметров, поля с префиксом `_`
- **Async/Await**: Все I/O операции используют паттерн async/await
- **Nullable Reference Types**: Включены на уровне проекта
- **Records**: Используются для DTO и value objects (неизменяемость)

### Лучшие практики

1. **Доменная логика**: Храните бизнес-правила в доменных сущностях, а не в сервисах
2. **Внедрение зависимостей**: Используйте внедрение через конструктор, регистрируйте сервисы в `Program.cs`
3. **Обработка ошибок**: Используйте доменные исключения для нарушений бизнес-правил
4. **Управление состоянием**: Инкапсулируйте переходы состояний в доменных сущностях
5. **Маппинг DTO**: Храните логику маппинга в сервисах приложения
6. **Паттерн Repository**: Абстрагируйте доступ к данным за интерфейсами

### Добавление новых функций

1. **Доменный слой**: Добавьте сущности, value objects или доменные исключения
2. **Слой приложения**: Определите DTO, интерфейсы и реализуйте сервисы
3. **Инфраструктурный слой**: Реализуйте интерфейсы репозиториев
4. **Слой представления**: Добавьте контроллеры и эндпоинты
5. **Тесты**: Напишите модульные тесты для каждого слоя

---

## 🤝 Участие в проекте

1. Форкните репозиторий
2. Создайте ветку функции (`git checkout -b feature/amazing-feature`)
3. Зафиксируйте изменения (`git commit -m 'Add some amazing feature'`)
4. Отправьте в ветку (`git push origin feature/amazing-feature`)
5. Откройте Pull Request

---

## 📄 Лицензия

Этот проект лицензирован под MIT License.

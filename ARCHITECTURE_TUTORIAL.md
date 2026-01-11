# 🎓 Полный туториал по архитектуре Interview Trainer API

## 📋 Содержание

1. [Обзор проекта](#обзор-проекта)
2. [Архитектурные паттерны](#архитектурные-паттерны)
3. [Порядок изучения (от простого к сложному)](#порядок-изучения)
4. [Критические связи между компонентами](#критические-связи)
5. [Best Practices](#best-practices)
6. [Микросервисы и RabbitMQ](#микросервисы-и-rabbitmq)
7. [Практические задания](#практические-задания)

---

## 🏗️ Обзор проекта

### Текущая структура проекта

```
InterviewTrainer.Api/
├── Domain/                    # Доменный слой (бизнес-логика)
│   ├── SessionStatus.cs       # Value Object (статус сессии)
│   └── DomainException.cs     # Доменное исключение
├── Entities/                  # Сущности (Aggregate Roots)
│   └── InterviewSession.cs    # Агрегат интервью-сессии
├── Application/               # Слой приложения (Use Cases)
│   └── Interfaces/
│       └── IInterviewRepository.cs  # Интерфейс репозитория
├── Infrastructure/            # Инфраструктурный слой
│   ├── AppDbContext.cs        # DbContext для EF Core
│   └── Persistence/
│       └── InMemoryInterviewRepository.cs  # Реализация репозитория
├── Controllers/               # API контроллеры
│   └── InterviewController.cs
└── Program.cs                 # Точка входа приложения
```

### Технологический стек

- **.NET 9.0** - Платформа разработки
- **Entity Framework Core 9.0** - ORM для работы с БД
- **PostgreSQL 16** - Реляционная БД
- **Docker Compose** - Контейнеризация БД
- **Swagger/OpenAPI** - Документация API

---

## 🎯 Архитектурные паттерны

### 1. Clean Architecture (Чистая архитектура)

**Что это:** Разделение кода на слои с четкими зависимостями.

**Слои в твоем проекте:**

```
┌─────────────────────────────────────┐
│   Controllers (Presentation)        │  ← Внешний слой
├─────────────────────────────────────┤
│   Application (Use Cases)           │  ← Бизнес-логика приложения
├─────────────────────────────────────┤
│   Domain (Business Logic)           │  ← Ядро бизнес-логики
├─────────────────────────────────────┤
│   Infrastructure (Data Access)     │  ← Технические детали
└─────────────────────────────────────┘
```

**Правило зависимостей:** Внутренние слои НЕ знают о внешних. Domain не зависит от Infrastructure.

**Почему это важно:**
- Легко тестировать бизнес-логику
- Можно менять БД без изменения бизнес-логики
- Код понятнее и поддерживаемее

---

### 2. Domain-Driven Design (DDD)

**Что это:** Подход к проектированию, где бизнес-логика живет в доменных моделях.

#### 2.1. Value Object (SessionStatus)

**Файл:** `Domain/SessionStatus.cs`

**Что это:** Объект, который определяется только своими значениями, а не идентификатором.

```csharp
public record SessionStatus
{
    public static SessionStatus Started => new("Started");
    // ...
}
```

**Характеристики:**
- ✅ Неизменяемый (immutable)
- ✅ Имеет бизнес-правила (CanChangeTo)
- ✅ Валидация внутри класса

**Почему record:**
- Автоматически реализует равенство по значению
- Неизменяемость по умолчанию
- Краткий синтаксис

**Best Practice:** Value Objects должны валидировать себя сами.

---

#### 2.2. Aggregate Root (InterviewSession)

**Файл:** `Entities/InterviewSession.cs`

**Что это:** Корневая сущность агрегата, которая контролирует доступ к своим частям.

**Характеристики:**
- ✅ Имеет уникальный идентификатор (Id)
- ✅ Инкапсулирует бизнес-логику (Start, Complete)
- ✅ Контролирует изменения состояния (TransitionTo)
- ✅ Приватные сеттеры для защиты инвариантов

**Инварианты (бизнес-правила):**
- Статус можно менять только по определенным правилам
- Completed/Cancelled - финальные статусы
- Score можно установить только при Complete

**Почему это важно:**
- Невозможно нарушить бизнес-правила извне
- Вся логика в одном месте
- Легко тестировать

---

#### 2.3. Domain Exception

**Файл:** `Domain/DomainException.cs`

**Что это:** Специальный тип исключения для бизнес-ошибок.

**Отличие от технических исключений:**
- DomainException = бизнес-правило нарушено (например, нельзя сменить статус)
- Technical Exception = техническая проблема (БД недоступна, сеть упала)

**Best Practice:** Обрабатывать DomainException на уровне API и возвращать понятные HTTP статусы (400 Bad Request).

---

### 3. Repository Pattern

**Что это:** Абстракция над доступом к данным.

**Структура:**

```
IInterviewRepository (интерфейс)
    ↓
InterviewRepository (реализация с EF Core)
```

**Почему это нужно:**
- ✅ Изоляция бизнес-логики от деталей БД
- ✅ Легко заменить EF Core на другую технологию
- ✅ Легко мокировать для тестов

**Текущая реализация:**

```csharp
public interface IInterviewRepository
{
    Task<InterviewSession?> GetByIdAsync(Guid id);
    Task AddAsync(InterviewSession session);
    Task UpdateAsync(InterviewSession session);
    Task DeleteAsync(Guid id);
}
```

**Best Practice:** Репозиторий работает с Aggregate Root, а не с отдельными сущностями.

---

### 4. Dependency Injection (DI)

**Файл:** `Program.cs`

**Что это:** Паттерн, где зависимости передаются извне, а не создаются внутри класса.

**Регистрация в Program.cs:**

```csharp
builder.Services.AddDbContext<AppDbContext>(...);
builder.Services.AddScoped<IInterviewRepository, InterviewRepository>();
```

**Типы жизненного цикла:**
- **Scoped** - один экземпляр на HTTP запрос (для репозиториев и DbContext)
- **Singleton** - один экземпляр на все приложение (для сервисов без состояния)
- **Transient** - новый экземпляр каждый раз (редко используется)

**Почему Scoped для репозитория:**
- DbContext должен жить один запрос
- Все операции в рамках одного запроса используют один контекст
- Автоматический SaveChanges в конце запроса

---

### 5. Entity Framework Core

**Файл:** `Infrastructure/AppDbContext.cs`

**Что это:** ORM (Object-Relational Mapping) - мост между объектами C# и таблицами БД.

#### 5.1. DbContext

**Роль:**
- Отслеживает изменения сущностей
- Выполняет SQL запросы
- Управляет транзакциями

**Конфигурация:**

```csharp
modelBuilder.Entity<InterviewSession>(entity =>
{
    entity.HasKey(e => e.Id);
    entity.Property(e => e.Status)
        .HasConversion(...)  // Конвертация Value Object ↔ строка БД
        .HasMaxLength(20)
        .IsRequired();
});
```

#### 5.2. Value Object Conversion

**Проблема:** БД хранит строки, а в коде - объекты SessionStatus.

**Решение:** HasConversion - автоматическая конвертация при сохранении/загрузке.

```csharp
.HasConversion(
    status => status.Value,              // C# → БД
    value => SessionStatus.FromValue(value)  // БД → C#
)
```

**Почему это важно:**
- БД остается простой (строки)
- Код остается типобезопасным (объекты)
- Валидация на уровне домена

---

### 6. Docker и Containerization

**Файл:** `docker/docker-compose.yml`

**Что это:** Контейнеризация - упаковка приложения и его зависимостей в изолированную среду.

**Компоненты docker-compose.yml:**

```yaml
services:
  postgres:
    image: postgres:16          # Образ PostgreSQL
    ports:
      - "5433:5432"             # Маппинг портов (хост:контейнер)
    volumes:
      - postgres_data:/var/lib/postgresql/data  # Постоянное хранилище
```

**Почему порт 5433:**
- Избегаем конфликта с локальным PostgreSQL
- Каждый сервис может иметь свой порт
- Легко запускать несколько окружений

**Volumes:**
- Данные сохраняются между перезапусками контейнера
- `docker compose down` НЕ удаляет volumes (данные остаются)
- `docker compose down -v` удаляет volumes (данные теряются)

---

## 📚 Порядок изучения

### Уровень 1: Основы (Ты уже здесь ✅)

**Что изучить:**
1. ✅ C# основы (классы, методы, async/await)
2. ✅ Entity Framework Core базовые операции
3. ✅ Dependency Injection в ASP.NET Core
4. ✅ Docker основы (docker compose up/down)

**Практика:**
- Создать простой CRUD API
- Подключить БД через EF Core
- Запустить БД в Docker

---

### Уровень 2: Архитектурные паттерны

**Что изучить:**
1. **Clean Architecture**
   - Разделение на слои
   - Правило зависимостей
   - Зачем это нужно

2. **Repository Pattern**
   - Абстракция доступа к данным
   - Интерфейсы vs реализации
   - Unit тесты с моками

3. **DDD основы**
   - Entity vs Value Object
   - Aggregate Root
   - Domain Events (следующий шаг)

**Практика:**
- Рефакторинг кода под Clean Architecture
- Выделение Value Objects
- Создание репозиториев

---

### Уровень 3: Продвинутые паттерны

**Что изучить:**
1. **CQRS (Command Query Responsibility Segregation)**
   - Разделение чтения и записи
   - Commands (изменяют состояние)
   - Queries (только читают)

2. **Mediator Pattern**
   - Централизованная обработка команд
   - MediatR библиотека

3. **Specification Pattern**
   - Переиспользуемые условия запросов
   - Композиция условий

**Практика:**
- Разделить контроллеры на Commands и Queries
- Внедрить MediatR
- Создать спецификации для фильтрации

---

### Уровень 4: Микросервисы

**Что изучить:**
1. **Микросервисная архитектура**
   - Когда использовать
   - Преимущества и недостатки
   - Service boundaries

2. **Межсервисная коммуникация**
   - HTTP REST API
   - Message Queue (RabbitMQ)
   - Event-Driven Architecture

3. **Распределенные системы**
   - Idempotency (идемпотентность)
   - Saga Pattern (распределенные транзакции)
   - Circuit Breaker

**Практика:**
- Разделить монолит на микросервисы
- Настроить RabbitMQ
- Реализовать асинхронную коммуникацию

---

## 🔗 Критические связи между компонентами

### Схема зависимостей

```
┌─────────────────────────────────────────────────────────┐
│                    Controllers                          │
│  ┌──────────────────────────────────────────────────┐  │
│  │  InterviewController                             │  │
│  │  ↓ зависит от                                    │  │
│  └──────────────────────────────────────────────────┘  │
└───────────────────────┬─────────────────────────────────┘
                        │ использует
                        ↓
┌─────────────────────────────────────────────────────────┐
│                  Application Layer                      │
│  ┌──────────────────────────────────────────────────┐  │
│  │  IInterviewRepository (интерфейс)                │  │
│  └──────────────────────────────────────────────────┘  │
└───────────────────────┬─────────────────────────────────┘
                        │ реализует
                        ↓
┌─────────────────────────────────────────────────────────┐
│              Infrastructure Layer                       │
│  ┌──────────────────────────────────────────────────┐  │
│  │  InterviewRepository                             │  │
│  │  ↓ использует                                    │  │
│  └──────────────────────────────────────────────────┘  │
└───────────────────────┬─────────────────────────────────┘
                        │ работает с
                        ↓
┌─────────────────────────────────────────────────────────┐
│                  Infrastructure Layer                   │
│  ┌──────────────────────────────────────────────────┐  │
│  │  AppDbContext                                    │  │
│  │  ↓ отслеживает                                   │  │
│  └──────────────────────────────────────────────────┘  │
└───────────────────────┬─────────────────────────────────┘
                        │ хранит
                        ↓
┌─────────────────────────────────────────────────────────┐
│                    Domain Layer                         │
│  ┌──────────────────────────────────────────────────┐  │
│  │  InterviewSession (Aggregate Root)               │  │
│  │  ↓ использует                                    │  │
│  └──────────────────────────────────────────────────┘  │
└───────────────────────┬─────────────────────────────────┘
                        │ содержит
                        ↓
┌─────────────────────────────────────────────────────────┐
│                    Domain Layer                         │
│  ┌──────────────────────────────────────────────────┐  │
│  │  SessionStatus (Value Object)                    │  │
│  │  DomainException                                 │  │
│  └──────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────┘
```

### Критические моменты

1. **Domain не зависит от Infrastructure**
   - DomainException, SessionStatus, InterviewSession - чистые классы
   - Нет using на EF Core или Infrastructure

2. **Application зависит только от Domain**
   - IInterviewRepository работает с InterviewSession (из Domain)
   - Интерфейсы в Application, реализации в Infrastructure

3. **Infrastructure знает о Domain**
   - AppDbContext конфигурирует InterviewSession
   - Repository реализует интерфейс из Application

4. **Controllers зависят от Application**
   - Используют IInterviewRepository через DI
   - Не знают о EF Core или БД напрямую

---

## ✨ Best Practices

### 1. Naming Conventions

**✅ Правильно:**
- `InterviewSession` - PascalCase для классов
- `GetByIdAsync` - Async суффикс для асинхронных методов
- `_appDbContext` - underscore для приватных полей
- `IInterviewRepository` - I префикс для интерфейсов

**❌ Неправильно:**
- `interviewSession` - camelCase для классов
- `GetById` - нет Async суффикса
- `appDbContext` - нет underscore

---

### 2. Async/Await

**✅ Правильно:**
```csharp
public async Task<InterviewSession?> GetByIdAsync(Guid id)
    => await _appDbContext.interviewSessions.FindAsync(id);
```

**❌ Неправильно:**
```csharp
public InterviewSession? GetById(Guid id)
    => _appDbContext.interviewSessions.Find(id);  // Блокирует поток!
```

**Почему важно:**
- Не блокирует потоки
- Лучшая производительность под нагрузкой
- ASP.NET Core построен на async

---

### 3. Error Handling

**✅ Правильно:**
```csharp
try
{
    session.Start();
    await _repository.UpdateAsync(session);
}
catch (DomainException ex)
{
    return BadRequest(ex.Message);  // 400 - ошибка клиента
}
catch (Exception ex)
{
    _logger.LogError(ex, "Unexpected error");
    return StatusCode(500);  // 500 - ошибка сервера
}
```

**❌ Неправильно:**
```csharp
session.Start();  // Может выбросить DomainException
await _repository.UpdateAsync(session);
// Нет обработки ошибок!
```

---

### 4. Value Objects

**✅ Правильно:**
```csharp
public record SessionStatus
{
    private SessionStatus(string value) { ... }  // Приватный конструктор
    public static SessionStatus Started => new("Started");  // Фабричные методы
}
```

**❌ Неправильно:**
```csharp
public class SessionStatus
{
    public SessionStatus(string value) { ... }  // Публичный конструктор
    // Можно создать невалидный статус!
}
```

---

### 5. Aggregate Root Protection

**✅ Правильно:**
```csharp
public class InterviewSession
{
    public SessionStatus Status { get; private set; }  // Приватный сеттер
    public void Start() => TransitionTo(SessionStatus.InProgress);  // Публичный метод
}
```

**❌ Неправильно:**
```csharp
public class InterviewSession
{
    public SessionStatus Status { get; set; }  // Публичный сеттер
    // Можно нарушить бизнес-правила извне!
}
```

---

### 6. Repository Pattern

**✅ Правильно:**
```csharp
public async Task UpdateAsync(InterviewSession session)
{
    _appDbContext.Update(session);  // EF отслеживает изменения
    await _appDbContext.SaveChangesAsync();
}
```

**❌ Неправильно:**
```csharp
public async Task UpdateAsync(InterviewSession session)
{
    var existing = await GetByIdAsync(session.Id);
    existing.Status = session.Status;  // Нарушает инкапсуляцию!
    await _appDbContext.SaveChangesAsync();
}
```

---

### 7. DbContext Lifecycle

**✅ Правильно:**
```csharp
builder.Services.AddDbContext<AppDbContext>(options => ...);
builder.Services.AddScoped<IInterviewRepository, InterviewRepository>();
// Один DbContext на HTTP запрос
```

**❌ Неправильно:**
```csharp
builder.Services.AddSingleton<AppDbContext>(...);  // Singleton!
// DbContext не thread-safe!
```

---

## 🚀 Микросервисы и RabbitMQ

### Что такое микросервисы?

**Монолит (текущее состояние):**
```
┌─────────────────────────────────────┐
│     Interview Trainer API            │
│  ┌───────────────────────────────┐  │
│  │  Controllers                  │  │
│  │  Business Logic               │  │
│  │  Database Access              │  │
│  └───────────────────────────────┘  │
└─────────────────────────────────────┘
```

**Микросервисы (будущее):**
```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│  Interview API  │    │  Analytics API  │    │  Notification   │
│                 │    │                 │    │     Service     │
│  - CRUD         │    │  - Statistics   │    │  - Email        │
│  - Validation   │    │  - Reports      │    │  - SMS          │
└────────┬────────┘    └────────┬────────┘    └────────┬────────┘
         │                      │                      │
         └──────────────────────┼──────────────────────┘
                                │
                    ┌───────────▼───────────┐
                    │     RabbitMQ          │
                    │   Message Broker      │
                    └───────────────────────┘
```

---

### Когда использовать микросервисы?

**✅ Используй когда:**
- Разные команды работают над разными частями
- Нужно масштабировать части независимо
- Разные технологии для разных сервисов
- Высокая нагрузка на определенные части

**❌ НЕ используй когда:**
- Маленькая команда (1-3 разработчика)
- Простое приложение
- Нет опыта с распределенными системами
- Premature optimization

---

### Паттерны коммуникации

#### 1. Synchronous (HTTP REST)

**Когда использовать:**
- Нужен немедленный ответ
- Простые операции
- Низкая латентность критична

**Пример:**
```
Client → Interview API → Get Session
         ← Response (200 OK)
```

**Проблемы:**
- Связанность сервисов
- Каскадные сбои
- Медленнее при цепочке вызовов

---

#### 2. Asynchronous (Message Queue)

**Когда использовать:**
- Долгие операции
- Не нужен немедленный ответ
- События, которые могут обрабатываться позже

**Пример:**
```
Interview API → RabbitMQ → Analytics Service
              (publish)    (consume)
```

**Преимущества:**
- Развязка сервисов
- Устойчивость к сбоям
- Масштабируемость

---

### RabbitMQ - основы

#### Что это?

**RabbitMQ** - брокер сообщений (Message Broker). Сервисы отправляют сообщения в очередь, другие сервисы их читают.

**Аналогия:** Почтовый ящик. Отправитель кладет письмо, получатель забирает когда готов.

---

#### Основные концепции

1. **Producer (Производитель)**
   - Сервис, который отправляет сообщения
   - В твоем случае: Interview API

2. **Consumer (Потребитель)**
   - Сервис, который читает сообщения
   - В твоем случае: Analytics Service, Notification Service

3. **Queue (Очередь)**
   - Хранилище сообщений
   - FIFO (First In, First Out)

4. **Exchange (Обменник)**
   - Маршрутизирует сообщения в очереди
   - Типы: Direct, Topic, Fanout, Headers

5. **Binding (Привязка)**
   - Связь между Exchange и Queue

---

#### Архитектура с RabbitMQ

```
┌─────────────────────────────────────────────────────────┐
│              Interview Trainer API                       │
│  ┌──────────────────────────────────────────────────┐  │
│  │  InterviewController                              │  │
│  │  ↓                                                │  │
│  │  InterviewService                                │  │
│  │  ↓                                                │  │
│  │  RabbitMQ Publisher                              │  │
│  └──────────────────────────────────────────────────┘  │
└───────────────────────┬─────────────────────────────────┘
                        │ Publish Event
                        ↓
        ┌───────────────────────────────┐
        │         RabbitMQ               │
        │  ┌─────────────────────────┐  │
        │  │  Exchange: interviews   │  │
        │  └───────────┬─────────────┘  │
        │              │                 │
        │    ┌─────────┴─────────┐      │
        │    │                   │       │
        │  Queue:        Queue:          │
        │  analytics     notifications   │
        └────┬───────────────┬───────────┘
             │               │
             ↓               ↓
┌──────────────────┐  ┌──────────────────┐
│ Analytics Service│  │Notification      │
│                  │  │Service           │
│ - Consume events │  │- Consume events  │
│ - Calculate stats│  │- Send emails     │
└──────────────────┘  └──────────────────┘
```

---

### Реализация RabbitMQ в проекте

#### Шаг 1: Добавить RabbitMQ в docker-compose.yml

```yaml
version: "3.9"

services:
  postgres:
    image: postgres:16
    container_name: interview-postgres
    # ... существующая конфигурация

  rabbitmq:
    image: rabbitmq:3-management
    container_name: interview-rabbitmq
    ports:
      - "5672:5672"      # AMQP порт
      - "15672:15672"    # Management UI
    environment:
      RABBITMQ_DEFAULT_USER: admin
      RABBITMQ_DEFAULT_PASS: admin
    volumes:
      - rabbitmq_data:/var/lib/rabbitmq

volumes:
  postgres_data:
  rabbitmq_data:
```

---

#### Шаг 2: Установить NuGet пакет

```bash
dotnet add package RabbitMQ.Client
```

---

#### Шаг 3: Создать Event Publisher

**Файл:** `Infrastructure/Messaging/RabbitMqEventPublisher.cs`

```csharp
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace InterviewTrainer.Api.Infrastructure.Messaging;

public interface IEventPublisher
{
    Task PublishAsync<T>(string exchange, string routingKey, T message);
}

public class RabbitMqEventPublisher : IEventPublisher, IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly ILogger<RabbitMqEventPublisher> _logger;

    public RabbitMqEventPublisher(IConfiguration configuration, ILogger<RabbitMqEventPublisher> logger)
    {
        _logger = logger;
        var factory = new ConnectionFactory
        {
            HostName = configuration["RabbitMQ:HostName"] ?? "localhost",
            Port = int.Parse(configuration["RabbitMQ:Port"] ?? "5672"),
            UserName = configuration["RabbitMQ:UserName"] ?? "admin",
            Password = configuration["RabbitMQ:Password"] ?? "admin"
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
    }

    public Task PublishAsync<T>(string exchange, string routingKey, T message)
    {
        try
        {
            // Объявляем exchange
            _channel.ExchangeDeclare(exchange, ExchangeType.Topic, durable: true);

            var json = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(json);

            var properties = _channel.CreateBasicProperties();
            properties.Persistent = true; // Сохранять сообщения на диск

            _channel.BasicPublish(
                exchange: exchange,
                routingKey: routingKey,
                basicProperties: properties,
                body: body);

            _logger.LogInformation("Published message to {Exchange} with routing key {RoutingKey}", 
                exchange, routingKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish message");
            throw;
        }

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
    }
}
```

---

#### Шаг 4: Создать Domain Events

**Файл:** `Domain/Events/InterviewSessionCompletedEvent.cs`

```csharp
namespace InterviewTrainer.Api.Domain.Events;

public record InterviewSessionCompletedEvent
{
    public Guid SessionId { get; init; }
    public Guid UserId { get; init; }
    public int Score { get; init; }
    public DateTime CompletedAt { get; init; }
}
```

---

#### Шаг 5: Публиковать события при завершении сессии

**Модификация:** `Entities/InterviewSession.cs`

```csharp
using InterviewTrainer.Api.Domain.Events;

public class InterviewSession
{
    // ... существующие поля

    private readonly List<IDomainEvent> _domainEvents = new();

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public void Complete(int score, string summary, string tips)
    {
        TransitionTo(SessionStatus.Completed);
        Score = score;
        Summary = summary;
        Tips = tips;
        FinishedAt = DateTime.UtcNow;

        // Добавляем доменное событие
        _domainEvents.Add(new InterviewSessionCompletedEvent
        {
            SessionId = Id,
            UserId = UserId,
            Score = score,
            CompletedAt = FinishedAt.Value
        });
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}
```

**Файл:** `Domain/Events/IDomainEvent.cs`

```csharp
namespace InterviewTrainer.Api.Domain.Events;

public interface IDomainEvent
{
    DateTime OccurredAt { get; }
}
```

---

#### Шаг 6: Обработка событий в контроллере/сервисе

**Файл:** `Application/Services/InterviewService.cs`

```csharp
using InterviewTrainer.Api.Application.Interface;
using InterviewTrainer.Api.Domain.Events;
using InterviewTrainer.Api.Infrastructure.Messaging;

namespace InterviewTrainer.Api.Application.Services;

public class InterviewService
{
    private readonly IInterviewRepository _repository;
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<InterviewService> _logger;

    public InterviewService(
        IInterviewRepository repository,
        IEventPublisher eventPublisher,
        ILogger<InterviewService> logger)
    {
        _repository = repository;
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task CompleteSessionAsync(Guid sessionId, int score, string summary, string tips)
    {
        var session = await _repository.GetByIdAsync(sessionId);
        if (session == null)
            throw new DomainException("Session not found");

        session.Complete(score, summary, tips);
        await _repository.UpdateAsync(session);

        // Публикуем доменные события
        foreach (var domainEvent in session.DomainEvents)
        {
            if (domainEvent is InterviewSessionCompletedEvent completedEvent)
            {
                await _eventPublisher.PublishAsync(
                    exchange: "interviews",
                    routingKey: "session.completed",
                    message: completedEvent);
            }
        }

        session.ClearDomainEvents();
    }
}
```

---

#### Шаг 7: Регистрация в DI

**Модификация:** `Program.cs`

```csharp
using InterviewTrainer.Api.Infrastructure.Messaging;

// ... существующий код

builder.Services.AddSingleton<IEventPublisher, RabbitMqEventPublisher>();
builder.Services.AddScoped<InterviewService>();
```

---

#### Шаг 8: Конфигурация

**Добавить в appsettings.json:**

```json
{
  "RabbitMQ": {
    "HostName": "localhost",
    "Port": "5672",
    "UserName": "admin",
    "Password": "admin"
  }
}
```

---

### Consumer Service (отдельный микросервис)

**Структура нового проекта:**

```
InterviewTrainer.Analytics/
├── Program.cs
├── Services/
│   └── InterviewCompletedConsumer.cs
└── InterviewTrainer.Analytics.csproj
```

**Файл:** `Services/InterviewCompletedConsumer.cs`

```csharp
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace InterviewTrainer.Analytics.Services;

public class InterviewCompletedConsumer : BackgroundService
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly ILogger<InterviewCompletedConsumer> _logger;

    public InterviewCompletedConsumer(ILogger<InterviewCompletedConsumer> logger)
    {
        _logger = logger;
        var factory = new ConnectionFactory
        {
            HostName = "localhost",
            Port = 5672,
            UserName = "admin",
            Password = "admin"
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _channel.ExchangeDeclare("interviews", ExchangeType.Topic, durable: true);
        var queueName = _channel.QueueDeclare("analytics_queue", durable: true).QueueName;
        _channel.QueueBind(queueName, "interviews", "session.completed");

        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            var routingKey = ea.RoutingKey;

            _logger.LogInformation("Received message: {Message} with routing key {RoutingKey}", 
                message, routingKey);

            try
            {
                var eventData = JsonSerializer.Deserialize<InterviewSessionCompletedEvent>(message);
                
                // Обработка события
                await ProcessCompletedSession(eventData);
                
                _channel.BasicAck(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message");
                _channel.BasicNack(ea.DeliveryTag, false, true); // Повторить
            }
        };

        _channel.BasicConsume(queueName, autoAck: false, consumer);

        await Task.CompletedTask;
    }

    private async Task ProcessCompletedSession(InterviewSessionCompletedEvent eventData)
    {
        // Логика аналитики
        _logger.LogInformation("Processing completed session {SessionId} with score {Score}", 
            eventData.SessionId, eventData.Score);
        
        // Здесь можно сохранить в БД аналитики, отправить в другую систему и т.д.
        await Task.CompletedTask;
    }

    public override void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
        base.Dispose();
    }
}

public record InterviewSessionCompletedEvent
{
    public Guid SessionId { get; init; }
    public Guid UserId { get; init; }
    public int Score { get; init; }
    public DateTime CompletedAt { get; init; }
}
```

**Файл:** `Program.cs`

```csharp
using InterviewTrainer.Analytics.Services;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddHostedService<InterviewCompletedConsumer>();

var host = builder.Build();
host.Run();
```

---

### Best Practices для RabbitMQ

1. **Idempotency (Идемпотентность)**
   - Обработка одного сообщения дважды должна давать тот же результат
   - Используй уникальные ID сообщений
   - Сохраняй обработанные ID в БД

2. **Dead Letter Queue (DLQ)**
   - Очередь для сообщений, которые не удалось обработать
   - Защита от бесконечных повторов

3. **Message Persistence**
   - `properties.Persistent = true` для важных сообщений
   - Сообщения сохраняются на диск

4. **Acknowledgment**
   - `autoAck: false` - подтверждай обработку вручную
   - `BasicAck` - сообщение обработано
   - `BasicNack` - сообщение не обработано, повторить

5. **Error Handling**
   - Retry с экспоненциальной задержкой
   - Логирование всех ошибок
   - Мониторинг очередей

---

## 📝 Практические задания

### Задание 1: Рефакторинг (Уровень 2)

**Цель:** Улучшить текущий код по Best Practices.

**Задачи:**
1. Исправить `AppDbContext` - сделать `DbSet` публичным свойством
2. Добавить обработку `DomainException` в контроллере
3. Добавить валидацию входных данных
4. Добавить логирование

---

### Задание 2: CQRS (Уровень 3)

**Цель:** Разделить чтение и запись.

**Задачи:**
1. Создать `Commands` папку с командами:
   - `CreateInterviewSessionCommand`
   - `StartInterviewSessionCommand`
   - `CompleteInterviewSessionCommand`

2. Создать `Queries` папку с запросами:
   - `GetInterviewSessionQuery`
   - `GetUserInterviewSessionsQuery`

3. Использовать MediatR для обработки команд/запросов

---

### Задание 3: RabbitMQ Integration (Уровень 4)

**Цель:** Добавить асинхронную коммуникацию.

**Задачи:**
1. Добавить RabbitMQ в docker-compose.yml
2. Создать `IEventPublisher` и реализацию
3. Публиковать событие при завершении сессии
4. Создать отдельный Consumer Service для аналитики

---

### Задание 4: Микросервисы (Уровень 4)

**Цель:** Разделить монолит на микросервисы.

**Задачи:**
1. Выделить Analytics Service
2. Выделить Notification Service
3. Настроить межсервисную коммуникацию через RabbitMQ
4. Добавить API Gateway (Ocelot или YARP)

---

## 🎯 Чеклист изучения

### Базовый уровень
- [ ] Понимаю Clean Architecture
- [ ] Понимаю Repository Pattern
- [ ] Понимаю DDD (Entity, Value Object, Aggregate)
- [ ] Умею работать с EF Core
- [ ] Понимаю Dependency Injection

### Средний уровень
- [ ] Реализовал CQRS
- [ ] Использую MediatR
- [ ] Понимаю Domain Events
- [ ] Умею писать Unit тесты
- [ ] Понимаю async/await

### Продвинутый уровень
- [ ] Понимаю микросервисную архитектуру
- [ ] Умею работать с RabbitMQ
- [ ] Понимаю Event-Driven Architecture
- [ ] Знаю паттерны распределенных систем (Saga, Circuit Breaker)
- [ ] Умею проектировать API Gateway

---

## 📚 Дополнительные ресурсы

### Книги
- "Clean Architecture" - Robert C. Martin
- "Domain-Driven Design" - Eric Evans
- "Building Microservices" - Sam Newman
- "RabbitMQ in Action" - Alvaro Videla, Jason J.W. Williams

### Документация
- [ASP.NET Core Documentation](https://learn.microsoft.com/en-us/aspnet/core/)
- [Entity Framework Core](https://learn.microsoft.com/en-us/ef/core/)
- [RabbitMQ Tutorials](https://www.rabbitmq.com/getstarted.html)
- [Docker Documentation](https://docs.docker.com/)

### Онлайн курсы
- Pluralsight: "Building Microservices with .NET"
- Udemy: "Microservices Architecture and Implementation on .NET"
- YouTube: "RabbitMQ Tutorials"

---

## 🎓 Заключение

Этот туториал покрывает все основные концепции, используемые в твоем проекте:

1. ✅ **Clean Architecture** - разделение на слои
2. ✅ **DDD** - доменное моделирование
3. ✅ **Repository Pattern** - абстракция доступа к данным
4. ✅ **Dependency Injection** - управление зависимостями
5. ✅ **Entity Framework Core** - работа с БД
6. ✅ **Docker** - контейнеризация
7. ✅ **Микросервисы** - распределенная архитектура
8. ✅ **RabbitMQ** - асинхронная коммуникация

**Следующие шаги:**
1. Изучи каждый паттерн подробнее
2. Реализуй практические задания
3. Постепенно переходи к микросервисам
4. Экспериментируй и задавай вопросы!

Удачи в изучении! 🚀


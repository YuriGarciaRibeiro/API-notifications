# Notification System API - Project Memory

**Last update**: 02/05/2026  
**Version**: .NET 10.0  
**Environment**: Clean Architecture + DDD + CQRS

---

## 🎯 Project Overview

Self-hosted and production-ready system for centralized management of notifications across multiple channels (Email, SMS, Push). Built with **.NET 10.0**, **ASP.NET Core**, **RabbitMQ**, **PostgreSQL**.

**Main Stack**:
- Backend: ASP.NET Core (Minimal APIs)
- Database: PostgreSQL 16 + EF Core 10.0.1
- Messaging: RabbitMQ 3 + Dead Letter Queue
- Auth: JWT + BCrypt
- Logging: Serilog
- Patterns: CQRS (MediatR), Repository, DI

---

## 📁 Project Structure

```
src/
├── NotificationSystem.Domain/          # Entities, Value Objects, Events
├── NotificationSystem.Application/     # Use Cases, DTOs, Validators (MediatR)
├── NotificationSystem.Infrastructure/  # EF Core, RabbitMQ, Services
├── NotificationSystem.Api/             # Minimal API, Endpoints, Auth
└── Consumers/
    ├── NotificationSystem.Consumer.Email/
    ├── NotificationSystem.Consumer.Sms/
    ├── NotificationSystem.Consumer.Push/
    └── NotificationSystem.Consumer.Dlq/
```

**Layered Architecture**:
```
Presentation (API + Workers)
    ↓
Infrastructure (RabbitMQ, DB, Services)
    ↓
Application (MediatR Handlers, DTOs)
    ↓
Domain (Entities, Events, Rules)
```

---

## 🔧 Core Patterns

### CQRS + MediatR
- **Commands**: `CreateNotificationCommand`, `CreateUserCommand`
- **Queries**: `GetAllNotificationsQuery`, `GetNotificationByIdQuery`
- **Handlers**: Implement `IRequestHandler<TRequest, TResponse>`

### Repository Pattern
- Interfaces: `Application/Interfaces/I*Repository.cs`
- Implementations: `Infrastructure/Persistence/Repositories/*Repository.cs`

### Multi-Channel Architecture
- Base: `Notification` (aggregate)
- Channels: `EmailChannel`, `SmsChannel`, `PushChannel`
- Independent status per channel
- Polymorphism: Table-Per-Type (TPT) in EF Core

### Domain Events
- Published by entities
- Dispatched via `DomainEventDispatcherBehavior`
- Handlers in `Application/EventHandlers/`

---

## 🔐 Security & Authentication

**JWT Configuration**:
```json
{
  "Jwt": {
    "Secret": "min-32-chars-in-production",
    "Issuer": "NotificationSystem",
    "Audience": "NotificationSystemUsers",
    "ExpiryMinutes": 15
  }
}
```

**Password Hashing**: BCrypt.Net-Next 4.0.3

**Authorization**: 
- Role-based (RBAC)
- Permission-based policies (claims)
- Custom `RequirePermissionAttribute`

---

## 📊 Main Flow: Create Notification

```
POST /api/notifications (CreateNotificationCommand)
    ↓ ValidationBehavior
    ↓ CreateNotificationHandler
    ↓ Create Notification + Channels
    ↓ Save to PostgreSQL
    ↓ RabbitMQ Producer
    ↓ Consumers (Email/SMS/Push)
    ↓ External Services (SMTP/Twilio/Firebase)
    ↓ Update Channel Status
    ↓ Dead Letter Queue (if failed)
```

---

## 🚀 Essential Commands

```bash
# Build
dotnet build NotificationSystem.slnx

# Run API (watch mode)
dotnet watch run --project src/NotificationSystem.Api

# Run Consumer
dotnet run --project src/Consumers/NotificationSystem.Consumer.Email

# Database
cd scripts/database/
./migrate.sh

# Docker
docker-compose up -d
```

---

## 📋 Conventions

**Naming**:
- Classes/Methods: `PascalCase`
- Variables: `camelCase`
- Interfaces: `IPascalCase`
- Constants: `UPPER_SNAKE_CASE`

**Code**:
- Nullable: ✅ Enabled
- ImplicitUsings: ✅ Enabled
- Async/Await: Always for I/O
- SOLID: Each class has one responsibility

**Dependencies**:
- Always inject via constructor
- Use Options pattern for config
- Register in DependencyInjection.cs of each layer

---

## ⚠️ Critical Points

1. **JWT Secret**: Minimum 32 characters in production
2. **CORS**: Currently `AllowAnyOrigin` - restrict in production
3. **Migrations**: Always test in staging first
4. **Dead Letter Queue**: Monitor regularly
5. **Entity Relationships**: TPT - each channel has its own table
6. **Config Secrets**: Use appsettings.Production.json + env vars

---

## 📚 Reference Documentation

- `README.md` - Complete documentation
- `docs/QUICKSTART.md` - Quick start
- `docs/CHANNEL_SYSTEM.md` - Channel system
- `docs/DEPLOYMENT.md` - Production deployment
- `.claude/rules/` - Topic-specific guides

---

## 🔗 Development URLs

- **API**: http://localhost:5000
- **Swagger**: http://localhost:5000/swagger
- **RabbitMQ**: http://localhost:15672 (guest/guest)
- **pgAdmin**: http://localhost:5050 (admin@admin.com/admin)
- **Mailpit**: http://localhost:8025

---

**This file is shared with the team via git.** For personal preferences, use `CLAUDE.local.md` (not committed).

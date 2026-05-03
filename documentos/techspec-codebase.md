# Tech Spec: NotificationSystem API

## 1. Visão Geral

O **NotificationSystem** é uma API de notificações **self-hosted** e **production-ready**, desenvolvida em **.NET 10** com **ASP.NET Core Minimal APIs**. O sistema centraliza o envio de notificações por múltiplos canais (Email, SMS, Push) de forma assíncrona via **RabbitMQ**, com persistência em **PostgreSQL**.

O problema de negócio principal é permitir que qualquer empresa hospede sua própria infraestrutura de notificações, com suporte a envio multi-canal simultâneo, rastreamento independente de status por canal, processamento assíncrono escalável e auditoria completa de operações. A arquitetura segue **Clean Architecture** + **DDD** + **CQRS**, com consumers independentes por canal que podem ser escalados horizontalmente.

---

## 2. Stack Tecnológico

| Categoria | Tecnologia |
|---|---|
| **Core** | C# / .NET 10 + ASP.NET Core (Minimal APIs) |
| **Build/Package** | dotnet CLI / NuGet |
| **Banco de Dados** | PostgreSQL 16 (via EF Core + Npgsql) |
| **Message Broker** | RabbitMQ 3.x (com DLX/DLQ) |
| **Interface** | REST (OpenAPI/Swagger) |
| **Job Scheduler** | Hangfire (PostgreSQL storage) |
| **Containerização** | Docker + Docker Compose |
| **Logging** | Serilog (structured logging) |

### Dependências NuGet Chave

| Pacote | Camada | Propósito |
|---|---|---|
| **MediatR 14.x** | Application | CQRS, Mediator pattern, Pipeline Behaviors |
| **FluentValidation 12.x** | Application | Validação declarativa integrada ao pipeline MediatR |
| **FluentResults 4.x** | Application | Result pattern (substitui exceptions para fluxo de erros) |
| **RabbitMQ.Client 7.x** | Infrastructure | Cliente oficial RabbitMQ (async API) |
| **MailKit** | Application/Services | Envio SMTP |
| **Twilio 7.x** | Application/Services | Envio SMS |
| **FirebaseAdmin** | Application/Services | Push Notifications (FCM) |
| **Entity Framework Core** | Infrastructure | ORM + Migrations |
| **Npgsql.EFCore.PostgreSQL** | Infrastructure | Provider PostgreSQL para EF Core |
| **Hangfire + Hangfire.PostgreSql** | API | Job scheduling para bulk/campaigns |
| **Serilog** | Cross-cutting | Logging estruturado |
| **Microsoft.AspNetCore.OpenApi** | API | Documentação Swagger |

---

## 3. Arquitetura e Padrões

### 3.1. Padrões Predominantes

| Módulo/Diretório | Padrão Arquitetural | Notas |
|---|---|---|
| `src/NotificationSystem.Domain/` | DDD (Domain Layer) | Entities, Value Objects, Enums, Domain Events. Sem dependências externas. |
| `src/NotificationSystem.Application/` | Clean Architecture + CQRS | UseCases organizados por feature (Command/Query + Handler + Response + Validator). MediatR pipeline. |
| `src/NotificationSystem.Infrastructure/` | Infrastructure Layer | Repositories (EF Core), RabbitMQ Publisher, EF Configurations, Auth Services, Provider Factories. |
| `src/NotificationSystem.Api/` | Presentation Layer (Minimal APIs) | Endpoints organizados por domínio, Middleware global, DI extensions. |
| `src/Consumers/` | Worker Services (BackgroundService) | Um consumer por canal (Email, SMS, Push, Bulk). Herdam de `RabbitMqConsumerBase<T>`. |

### 3.2. Dependência entre Projetos

```
NotificationSystem.Api         → Application + Infrastructure
Consumers/*                    → Application + Infrastructure
NotificationSystem.Infrastructure → Application + Domain
NotificationSystem.Application   → Domain
NotificationSystem.Domain        → (sem dependências)
```

### 3.3. Fluxo de Dados Principal

```
Client HTTP Request
  → Minimal API Endpoint
    → MediatR.Send(Command/Query)
      → ValidationBehavior (FluentValidation)
        → Handler (lógica de negócio)
          → Repository (persistência EF Core)
          → Domain Event (NotificationCreatedEvent)
            → DomainEventDispatcher (MediatR pipeline)
              → NotificationCreatedEventHandler
                → RabbitMQ Publisher (publica nas filas por canal)

RabbitMQ Queue
  → Consumer (BackgroundService)
    → MessageProcessingMiddleware (retry + error handling)
      → Provider Factory (resolve provider ativo do banco)
        → External Service (SMTP / Twilio / Firebase)
          → Atualiza status do canal no banco
```

### 3.4. Engines e Abstrações Core

| Abstração | Como Funciona |
|---|---|
| **RabbitMqConsumerBase\<TMessage\>** | Classe abstrata genérica que configura conexão RabbitMQ, declara filas com DLX/DLQ, deserializa mensagens e delega processamento. Cada consumer herda e define `QueueName`, `ProcessMessageAsync`, `GetNotificationIdsAsync` e `GetChannelType`. |
| **MessageProcessingMiddleware\<TMessage\>** | Middleware de processamento com retry strategy (exponential backoff). Executa a lógica, faz retry configurável, e atualiza status para `Failed` no banco via reflection se todas tentativas falharem. |
| **ProviderFactoryBase\<TService\>** | Factory abstrata que resolve o provider ativo (SMTP, Twilio, Firebase, SendGrid) a partir de `ProviderConfiguration` no banco. Suporta múltiplos providers por canal com prioridade e flag `IsPrimary`. |
| **ValidationBehavior\<TRequest, TResponse\>** | MediatR Pipeline Behavior que intercepta todos os requests, executa validators do FluentValidation e retorna erros via FluentResults sem lançar exceptions. |
| **DomainEventDispatcherBehavior\<TRequest, TResponse\>** | MediatR Pipeline Behavior que após handler bem-sucedido, extrai Domain Events de `Notification` (via reflection) e publica via MediatR. |
| **ResultExtensions** | Extension methods que convertem `FluentResults.Result<T>` para `IResult` HTTP, mapeando `DomainError` subclasses para status codes e ProblemDetails (RFC 7807). |
| **DomainError hierarchy** | Classe base abstrata com `Code` + `StatusCode`. Subclasses: `NotFoundError`, `ValidationError`, `ConflictError`, `ForbiddenError`, `UnauthorizedError`, `InternalError`. |

---

## 4. Design de Código e Convenções

### 4.1. Nomenclatura

| Elemento | Padrão | Exemplo |
|---|---|---|
| **DTOs** | Sufixo `Dto` ou `Request`/`Response` | `NotificationDto`, `ChannelDto`, `ChannelRequest`, `GetAllNotificationsResponse` |
| **Commands** | Sufixo `Command` | `CreateNotificationCommand`, `CreateBulkNotificationCommand` |
| **Queries** | Sufixo `Query` | `GetAllNotificationsQuery`, `GetNotificationByIdQuery` |
| **Handlers** | Sufixo `Handler` | `CreateNotificationHandler`, `GetAllNotificationsHandler` |
| **Validators** | Sufixo `Validator` | `GetAllNotificationsValidator` |
| **Entities** | Nome direto (PascalCase) | `Notification`, `NotificationChannel`, `EmailChannel` |
| **Interfaces** | Prefixo `I` | `INotificationRepository`, `IMessagePublisher`, `IAuditable` |
| **Repositories** | Sufixo `Repository` | `NotificationRepository`, `UserRepository` |
| **Services** | Sufixo `Service` | `AuthenticationService`, `SmtpService`, `DeadLetterQueueService` |
| **Factories** | Sufixo `Factory` ou `ProviderFactory` | `EmailProviderFactory`, `SmsProviderFactory` |
| **Settings/Config** | Sufixo `Settings` | `SmtpSettings`, `RabbitMqSettings`, `JwtSettings` |
| **Enums** | Nome direto (PascalCase) | `NotificationStatus`, `ChannelType`, `ProviderType` |
| **Endpoints** | Sufixo `Endpoints` (static class) | `NotificationEndpoints`, `AuthEndpoints` |
| **Behaviors** | Sufixo `Behavior` | `ValidationBehavior`, `DomainEventDispatcherBehavior` |
| **Messages (RabbitMQ)** | Sufixo `Message` (records) | `EmailChannelMessage`, `SmsChannelMessage`, `PushChannelMessage` |
| **Permissions** | Formato `resource.action` (kebab-case) | `notification.create`, `user.assign-roles`, `dlq.reprocess` |

### 4.2. Organização de UseCases

Cada use case segue a estrutura de pasta por feature:

```
UseCases/
└── CreateNotification/
    ├── CreateNotificationCommand.cs    # record : IRequest<Result<T>>
    ├── CreateNotificationHandler.cs    # : IRequestHandler<Command, Result<T>>
    └── CreateNotificationValidator.cs  # : AbstractValidator<Command> (quando necessário)
```

- **Commands** retornam `Result<T>` (FluentResults)
- **Queries** retornam `Result<T>` (FluentResults)
- Validators são opcionais e auto-registrados via assembly scanning

### 4.3. Tratamento de Erros

O sistema usa uma **abordagem dupla** para tratamento de erros:

**1. Result Pattern (FluentResults) - Fluxo principal:**
- Handlers retornam `Result<T>` em vez de lançar exceptions
- `DomainError` subclasses encapsulam código de erro + HTTP status code
- `ResultExtensions.ToIResult()` converte para HTTP responses com `ProblemDetails`

**2. Global Exception Handler - Fallback:**
- `GlobalExceptionHandlerMiddleware` captura exceptions não tratadas
- Mapeia tipos de exception para HTTP status codes (ValidationException → 400, KeyNotFoundException → 404, etc.)
- Retorna `ProblemDetails` (RFC 7807) com `traceId` e `timestamp`

**Padrão de resposta de erro da API (ProblemDetails):**
```json
{
  "type": "https://api.example.com/errors/not-found",
  "title": "Not Found",
  "detail": "Notification with ID ... was not found",
  "status": 404,
  "instance": "/api/notifications/...",
  "traceId": "...",
  "timestamp": "2025-01-01T00:00:00Z",
  "errors": {}
}
```

### 4.4. Padrões de DI e Registro

- **Assembly scanning** para MediatR handlers e FluentValidation validators
- **Extension methods** `AddApplication()`, `AddInfrastructure()`, `AddApiServices()` para registro modular
- **IOptions\<T\>** pattern para configurações (Settings classes com `SectionName`)
- **Scoped** para repositories e services
- **Singleton** para `IMessagePublisher` (RabbitMQ connection)
- **Primary constructors** (C# 12) amplamente utilizados

### 4.5. Polimorfismo e Serialização

- **Domain:** `NotificationChannel` (abstract) → `EmailChannel`, `SmsChannel`, `PushChannel` (herança TPH no EF Core)
- **DTOs:** `ChannelDto` (abstract record) com `[JsonPolymorphic]` + `[JsonDerivedType]` para serialização polimórfica automática
- **Messages:** Records imutáveis separados por canal para mensagens RabbitMQ

### 4.6. Autenticação e Autorização

- **JWT Bearer Authentication** com access token + refresh token
- **Permission-based Authorization** via claims (não role-based)
- Permissões centralizadas em `Permissions.cs` como constantes `string`
- Cada endpoint usa `.RequireAuthorization(Permissions.XxxYyy)`
- Entidades: `User` → `UserRole` → `Role` → `RolePermission` → `Permission`

### 4.7. Auditoria

- **AuditLogInterceptor** (EF Core SaveChanges interceptor) captura alterações automaticamente
- Apenas entidades que implementam `IAuditable` (marker interface) são auditadas
- Registra: quem, quando, o quê, valores antigos/novos, propriedades alteradas, IP, user agent, request path
- Endpoints dedicados para consulta de audit logs com filtros

---

## 5. Integrações Externas

| Sistema | Objetivo | Protocolo | Status |
|---|---|---|---|
| **SMTP (MailKit)** | Envio de emails | SMTP (porta 587/465) | ✅ Production-ready |
| **SendGrid** | Envio de emails (alternativo) | REST API | ✅ Implementado |
| **Twilio** | Envio de SMS | REST API (SDK oficial) | ✅ Production-ready |
| **Firebase Cloud Messaging** | Push Notifications | REST API (SDK Admin) | 🔄 Código preparado |
| **RabbitMQ** | Message broker assíncrono | AMQP 0-9-1 | ✅ Production-ready |
| **PostgreSQL** | Persistência principal | TCP (EF Core + Npgsql) | ✅ Production-ready |
| **Hangfire** | Job scheduling (bulk/campaigns) | PostgreSQL storage | ✅ Implementado |

### Provider Factory Pattern

Os consumers **não** possuem referência direta a um serviço de envio. Em vez disso, usam **Provider Factories** que resolvem dinamicamente qual provider usar baseado na tabela `ProviderConfiguration` no banco:

```
Consumer → ProviderFactory.CreateXxxProvider()
  → Consulta ProviderConfiguration (IsActive=true, IsPrimary=true)
    → Deserializa ConfigurationJson
      → Instancia serviço concreto (SmtpService, SendGridService, TwilioSmsService, etc.)
```

---

## 6. Pontos Críticos ("Gotchas")

1. **Provider Configuration é obrigatório:** Consumers ignoram mensagens se não houver provider ativo configurado no banco. Sem seed de `ProviderConfiguration`, nenhuma notificação é enviada.

2. **Domain Events são disparados APÓS SaveChanges:** O `NotificationDbContext` despacha domain events após persistir, e o `DomainEventDispatcherBehavior` também faz dispatch via reflection. Há duplicação potencial de dispatch — verificar fluxo.

3. **RabbitMQ Publisher usa `.GetAwaiter().GetResult()`** no construtor para setup de conexão síncrona. Pode causar deadlock em cenários específicos de DI.

4. **MessageProcessingMiddleware usa reflection** para chamar `UpdateNotificationChannelStatusAsync<TChannel>` genérico. Mudanças na assinatura do método quebram silenciosamente.

5. **CreateNotificationHandler usa `Dictionary<string, object>`** para dados de canal (ao invés de DTOs tipados), parsing manual de `JsonElement`. Erros de campo silenciosos (campo errado → valor vazio).

6. **Stats carregam TODAS as notificações em memória** (`GetStatsAsync` faz `ToListAsync()` antes de contar). Problema de performance com volume alto.

7. **Hangfire storage compartilha o PostgreSQL** da aplicação. Em alta carga de jobs, pode impactar queries da aplicação.

8. **CORS está `AllowAnyOrigin`** — restringir antes de ir para produção.

9. **DatabaseSeeder roda no startup** (`InitializeDatabaseAsync`). Se o banco não estiver pronto, a API não sobe.

10. **Consumers são projetos separados** com seus próprios `Program.cs` e `Dockerfile`. Cada um precisa ser buildado e deployado independentemente.

---

## 7. Mapa de Navegação

| O que procura? | Onde encontrar |
|---|---|
| **Regras de Negócio (Domain)** | `src/NotificationSystem.Domain/Entities/` |
| **Domain Events** | `src/NotificationSystem.Domain/Events/` |
| **Use Cases (Commands/Queries)** | `src/NotificationSystem.Application/UseCases/{FeatureName}/` |
| **DTOs** | `src/NotificationSystem.Application/DTOs/{Domínio}/` |
| **Interfaces/Contratos** | `src/NotificationSystem.Application/Interfaces/` |
| **Validators** | Dentro de cada UseCase ou `src/NotificationSystem.Application/Validators/` |
| **MediatR Behaviors (Pipeline)** | `src/NotificationSystem.Application/Common/Behaviors/` |
| **Error Types** | `src/NotificationSystem.Application/Common/Errors/` |
| **Mappings (Entity → DTO)** | `src/NotificationSystem.Application/Common/Mappings/` |
| **Messages (RabbitMQ contracts)** | `src/NotificationSystem.Application/Messages/` |
| **Consumer Base + Middleware** | `src/NotificationSystem.Application/Consumers/` |
| **Permissions** | `src/NotificationSystem.Application/Authorization/Permissions.cs` |
| **Settings/Configuration classes** | `src/NotificationSystem.Application/Configuration/` |
| **Application Services** | `src/NotificationSystem.Application/Services/` |
| **Endpoints (API)** | `src/NotificationSystem.Api/Endpoints/` |
| **Middleware (Exception Handler)** | `src/NotificationSystem.Api/Middlewares/` |
| **DI Registration (API)** | `src/NotificationSystem.Api/DependencyInjection.cs` |
| **DI Registration (Application)** | `src/NotificationSystem.Application/DependencyInjection.cs` |
| **DI Registration (Infrastructure)** | `src/NotificationSystem.Infrastructure/DependencyInjection.cs` |
| **Result → HTTP Extensions** | `src/NotificationSystem.Api/Extensions/ResultExtensions.cs` |
| **Entry Point** | `src/NotificationSystem.Api/Program.cs` |
| **Repositories** | `src/NotificationSystem.Infrastructure/Persistence/Repositories/` |
| **EF Core Configurations** | `src/NotificationSystem.Infrastructure/Persistence/Configurations/` |
| **DbContext** | `src/NotificationSystem.Infrastructure/Persistence/NotificationDbContext.cs` |
| **Migrations** | `src/NotificationSystem.Infrastructure/Migrations/` |
| **RabbitMQ Publisher** | `src/NotificationSystem.Infrastructure/Messaging/RabbitMQPublisher.cs` |
| **Provider Factories** | `src/NotificationSystem.Infrastructure/Factories/` |
| **Auth Services (JWT, Password)** | `src/NotificationSystem.Infrastructure/Services/` |
| **Database Seeder** | `src/NotificationSystem.Infrastructure/Persistence/DatabaseSeeder.cs` |
| **Consumers (Workers)** | `src/Consumers/NotificationSystem.Consumer.{Canal}/` |
| **Docker (Dev)** | `docker-compose.yml` |
| **Docker (Prod)** | `docker-compose.production.yml` |
| **Scripts de Migration** | `scripts/database/` |
| **Configuração de ambiente** | `appsettings.json`, `.env.example`, `.env.production.example` |

---

## 8. Estrutura de Projetos da Solution

```
NotificationSystem.slnx
├── src/
│   ├── NotificationSystem.Domain/           # Camada de Domínio (0 dependências)
│   │   ├── Entities/                        # Notification, NotificationChannel (TPH), User, Role, AuditLog, etc.
│   │   ├── Events/                          # IDomainEvent, NotificationCreatedEvent
│   │   └── Interfaces/                      # IAuditable (marker)
│   │
│   ├── NotificationSystem.Application/      # Camada de Aplicação (depende: Domain)
│   │   ├── UseCases/                        # 36 use cases (CQRS pattern)
│   │   ├── DTOs/                            # Agrupados por domínio (Notifications, Auth, Users, Roles, etc.)
│   │   ├── Interfaces/                      # 25 contratos (Repositories, Services, Factories)
│   │   ├── Common/                          # Behaviors, Errors, Mappings, Exceptions
│   │   ├── Consumers/                       # RabbitMqConsumerBase<T>, MessageProcessingMiddleware<T>
│   │   ├── Services/                        # Auth, DLQ, Email, SMS, Push, Campaign
│   │   ├── Messages/                        # Contratos de mensagens RabbitMQ
│   │   ├── Configuration/                   # Settings classes (SMTP, RabbitMQ, JWT, etc.)
│   │   ├── Authorization/                   # Permissions constants
│   │   ├── EventHandlers/                   # NotificationCreatedEventHandler
│   │   └── Validators/                      # Validators avulsos
│   │
│   ├── NotificationSystem.Infrastructure/   # Camada de Infraestrutura (depende: Application, Domain)
│   │   ├── Persistence/                     # DbContext, Repositories (8), Configurations (13), Seeder, Interceptors
│   │   ├── Messaging/                       # RabbitMQPublisher
│   │   ├── Factories/                       # ProviderFactoryBase, Email/Sms/PushProviderFactory
│   │   └── Services/                        # JWT, Password, Encryption, CurrentUser
│   │
│   ├── NotificationSystem.Api/              # Presentation Layer - API (depende: Application, Infrastructure)
│   │   ├── Endpoints/                       # 8 grupos de endpoints (Minimal APIs)
│   │   ├── Middlewares/                     # GlobalExceptionHandlerMiddleware
│   │   ├── Extensions/                      # ResultExtensions, ProblemDetails, Swagger, Permissions
│   │   ├── Infrastructure/                  # Hangfire filters
│   │   ├── Authorization/                   # Permission policy extensions
│   │   └── Program.cs                       # Entry point
│   │
│   └── Consumers/                           # Presentation Layer - Workers
│       ├── NotificationSystem.Consumer.Email/
│       ├── NotificationSystem.Consumer.Sms/
│       ├── NotificationSystem.Consumer.Push/
│       └── NotificationSystem.Consumer.Bulk/
```

---

## 9. Endpoints da API

| Grupo | Método | Rota | Permissão |
|---|---|---|---|
| **Notifications** | GET | `/api/notifications` | `notification.view` |
| | GET | `/api/notifications/{id}` | `notification.view` |
| | GET | `/api/notifications/stats` | `notification.stats` |
| | POST | `/api/notifications` | `notification.create` |
| **Bulk Notifications** | GET/POST/DELETE | `/api/notifications/bulk/*` | `bulk-notification.*` |
| **Bulk Realtime (SignalR)** | WS | `/hubs/bulk-progress` | `bulk-notification.view` |
| **Auth** | POST | `/api/auth/login`, `/register`, `/refresh`, `/revoke` | Público (login/register) |
| **Users** | CRUD | `/api/users/*` | `user.*` |
| **Roles** | CRUD | `/api/roles/*` | `role.*` |
| **Providers** | CRUD + configuração segura + test connection | `/api/admin/providers/*`, `GET /api/admin/providers/{id}/configuration`, `PUT /api/admin/providers/{id}`, `POST /api/admin/providers/{id}/test-connection` | `provider.*` |
| **Dead Letter Queue** | GET/POST/DELETE | `/api/dlq/*` | `dlq.*` |
| **Audit Logs** | GET | `/api/audit-logs/*` | `audit.*` |
| **Hangfire Dashboard** | - | `/hangfire` | Dev: sem auth / Prod: com auth |

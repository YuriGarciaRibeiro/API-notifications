# Tech Spec: NotificationSystem API

## 1. Visão Geral

Sistema **centralizador de notificações self-hosted** desenvolvido em **.NET 10** com **ASP.NET Core Minimal APIs** e arquitetura **event-driven** baseada em **RabbitMQ**. A aplicação segue **Clean Architecture** e **Domain-Driven Design (DDD)**, organizando o código em 4 camadas bem definidas (Domain → Application → Infrastructure → Presentation).

O sistema permite o envio de notificações por múltiplos canais simultaneamente (**Email**, **SMS**, **Push**) com rastreamento independente de status por canal. Suporta notificações individuais, em **bulk** e via **campanhas agendadas** (Hangfire). Cada canal é processado por um **Consumer/Worker** independente, escalável horizontalmente, conectado ao RabbitMQ com Dead Letter Queue (DLQ) para resiliência.

---

## 2. Stack Tecnológico

| Categoria | Tecnologia |
|---|---|
| **Linguagem** | C# / .NET 10 |
| **Framework Web** | ASP.NET Core (Minimal APIs) |
| **Build/Package** | .NET SDK / NuGet / Solution (.slnx) |
| **Banco de Dados** | PostgreSQL (via EF Core 10 + Npgsql) |
| **Message Broker** | RabbitMQ 7.2 (com DLX/DLQ) |
| **Job Scheduler** | Hangfire 1.8.14 (PostgreSql Storage) |
| **Interface API** | REST (JSON) + OpenAPI/Swagger (Swashbuckle) |
| **Autenticação** | JWT Bearer (symmetric key) |
| **Logging** | Serilog (Console + File sinks) |

### Dependências Chave (NuGet)

| Pacote | Versão | Camada | Uso |
|---|---|---|---|
| **MediatR** | 14.0.0 | Application | CQRS + Mediator + Pipeline Behaviors |
| **FluentValidation** | 12.1.1 | Application | Validação declarativa no pipeline MediatR |
| **FluentResults** | 4.0.0 | Application | Result Pattern (sem exceções para controle de fluxo) |
| **RabbitMQ.Client** | 7.2.0 | Application/Infra | Publisher + Consumer base genérico |
| **Entity Framework Core** | 10.0.1 | Infrastructure | ORM + Migrations + Interceptors |
| **Npgsql.EFCore.PostgreSQL** | 10.0.0 | Infrastructure | Provider PostgreSQL |
| **BCrypt.Net-Next** | 4.0.3 | Infrastructure | Hashing de senhas |
| **MailKit** | 4.14.1 | Application | Envio de Email via SMTP |
| **SendGrid** | 9.29.3 | Application | Envio de Email via API SendGrid |
| **Twilio** | 7.14.0 | Application | Envio de SMS |
| **FirebaseAdmin** | 3.4.0 | Application | Push Notifications (FCM) |
| **Hangfire** | 1.8.14 | API | Agendamento de Campanhas |
| **Serilog.AspNetCore** | 10.0.0 | API | Logging estruturado |
| **Swashbuckle.AspNetCore** | 10.0.1 | API | Documentação OpenAPI |

---

## 3. Arquitetura e Padrões

### 3.1. Padrões Predominantes

| Módulo/Diretório | Padrão Arquitetural | Notas |
|---|---|---|
| `src/NotificationSystem.Domain/` | **DDD - Domain Layer** | Aggregate Root (Notification), Entities, Value Objects, Domain Events, Marker Interfaces. Zero dependências externas |
| `src/NotificationSystem.Application/` | **CQRS + Mediator + Clean Architecture** | Use Cases por pasta, MediatR Handlers, Pipeline Behaviors (Validation, DomainEvent), Result Pattern (FluentResults) |
| `src/NotificationSystem.Infrastructure/` | **Repository + Factory + Interceptor** | EF Core Repositories, Provider Factories (Abstract Factory), AuditLog Interceptor, RabbitMQ Publisher |
| `src/NotificationSystem.Api/` | **Minimal API + Middleware** | Endpoint Groups com MediatR, Global Exception Handler, JWT Auth, Hangfire Dashboard |
| `src/Consumers/` | **Worker Service + Template Method** | `RabbitMqConsumerBase<TMessage>` genérico com DLX/DLQ, retry middleware, BackgroundService |

### 3.2. Engines e Abstrações Core

#### `RabbitMqConsumerBase<TMessage>` — Template Method para Consumers
- **Localização**: `Application/Consumers/RabbitMqConsumerBase.cs`
- **Como funciona**: Classe base genérica (`BackgroundService`) que gerencia conexão RabbitMQ, declaração de filas com DLX/DLQ, deserialização de mensagens e ciclo de vida (start/stop). Workers concretos implementam apenas:
  - `QueueName` — nome da fila
  - `ProcessMessageAsync()` — lógica de envio
  - `GetNotificationIdsAsync()` — extrai IDs para tracking
  - `GetChannelType()` — tipo do canal (Email/SMS/Push)
- **Middleware**: `MessageProcessingMiddleware<TMessage>` com retry (`ExponentialBackoffRetryStrategy`) e error handling automático

#### `ProviderFactoryBase<TService>` — Abstract Factory para Providers Dinâmicos
- **Localização**: `Infrastructure/Factories/ProviderFactoryBase.cs`
- **Como funciona**: Carrega configuração de provider do banco de dados (tabela `ProviderConfiguration`), deserializa JSON criptografado, e instancia o serviço correto. Factories concretas: `EmailProviderFactory`, `SmsProviderFactory`, `PushProviderFactory`
- **Troca hot de provider**: Permite trocar provider (ex: SMTP → SendGrid) sem redeploy, apenas alterando a configuração no DB

#### `ValidationBehavior<TRequest, TResponse>` — Pipeline MediatR
- **Localização**: `Application/Common/Behaviors/ValidationBehavior.cs`
- **Como funciona**: Intercepta todas as requests MediatR, executa os `IValidator<T>` registrados via FluentValidation, e retorna `Result` com erros caso a validação falhe (sem lançar exceções)

#### `DomainEventDispatcherBehavior<TRequest, TResponse>` — Pipeline MediatR
- **Localização**: `Application/Common/Behaviors/DomainEventDispatcherBehavior.cs`
- **Como funciona**: Após execução bem-sucedida de um handler, extrai `DomainEvents` da `Notification` (via reflection) e os despacha via `IMediator.Publish()`. Usado para disparar publicação no RabbitMQ após criação de notificação

#### `ResultExtensions` — Conversão FluentResults → HTTP Response
- **Localização**: `Api/Extensions/ResultExtensions.cs`
- **Como funciona**: Converte `Result<T>` e `Result` em `IResult` do Minimal APIs. Mapeia `DomainError` subclasses (NotFoundError, ConflictError, ForbiddenError, UnauthorizedError, ValidationError, InternalError) para status codes HTTP + ProblemDetails (RFC 7807)

---

## 4. Design de Código e Convenções

### 4.1. Nomenclatura

| Elemento | Padrão | Exemplo |
|---|---|---|
| **Entities (Domain)** | PascalCase, sem sufixo | `Notification`, `EmailChannel`, `User` |
| **DTOs** | Sufixo `Dto`, `Request`, `Response` | `NotificationDto`, `CreateRoleRequest`, `LoginResponse` |
| **Commands (CQRS)** | Sufixo `Command` | `CreateNotificationCommand` |
| **Queries (CQRS)** | Sufixo `Query` | `GetAllNotificationsQuery` |
| **Handlers** | Sufixo `Handler` | `GetAllNotificationsHandler` |
| **Validators** | Sufixo `Validator` | `GetAllNotificationsValidator` |
| **Interfaces** | Prefixo `I` | `INotificationRepository`, `IAuditable` |
| **Implementações** | Sem sufixo `Impl` | `NotificationRepository` (implementa `INotificationRepository`) |
| **Settings/Config** | Sufixo `Settings` | `RabbitMqSettings`, `SmtpSettings`, `JwtSettings` |
| **Enums** | PascalCase, singular | `NotificationStatus`, `ChannelType`, `NotificationOrigin` |
| **Errors** | Sufixo `Error` extends `DomainError` | `NotFoundError`, `ConflictError`, `ValidationError` |
| **Testes** | (planejados) Sufixo `Tests` no projeto | `NotificationSystem.Domain.Tests` |

### 4.2. Organização de Use Cases

Cada Use Case é uma **pasta individual** dentro de `Application/UseCases/`:
```
UseCases/
├── CreateNotification/
│   ├── CreateNotificationCommand.cs      # IRequest<Result<T>>
│   ├── CreateNotificationHandler.cs      # IRequestHandler
│   ├── CreateNotificationResponse.cs     # DTO de saída
│   └── CreateNotificationValidator.cs    # FluentValidation
├── GetAllNotifications/
│   ├── GetAllNotificationsQuery.cs
│   ├── GetAllNotificationsHandler.cs
│   ├── GetAllNotificationsResponse.cs
│   └── GetAllNotificationsValidator.cs
└── ... (padrão repete para cada Use Case)
```

### 4.3. Tratamento de Erros (Dual-Layer)

**Camada 1 — Result Pattern (FluentResults):**
- Handlers retornam `Result<T>` ou `Result` (nunca lançam exceções por fluxo lógico)
- Erros tipados: `NotFoundError(404)`, `ConflictError(409)`, `ForbiddenError(403)`, `UnauthorizedError(401)`, `ValidationError(400)`, `InternalError(500)`
- `ResultExtensions.ToIResult()` converte para HTTP response com `ProblemDetails`

**Camada 2 — Global Exception Handler (Middleware):**
- `GlobalExceptionHandlerMiddleware` captura exceções não tratadas
- Pattern matching: `ValidationException` → 400, `ArgumentException` → 400, `KeyNotFoundException` → 404, `UnauthorizedAccessException` → 401, `_ (default)` → 500
- Resposta padronizada com `ProblemDetails` (RFC 7807), incluindo `traceId` e `timestamp`
- Em `Development` expõe a mensagem da exceção; em `Production` mensagem genérica

### 4.4. Padrão de Resposta de Erro da API

```json
{
  "type": "https://httpstatuses.com/400",
  "title": "Validation Error",
  "status": 400,
  "detail": "One or more validation errors occurred",
  "instance": "/api/notifications",
  "traceId": "0HMVFE...",
  "timestamp": "2025-12-10T10:30:00Z",
  "errors": {
    "PageNumber": ["Page number must be greater than 0"],
    "PageSize": ["Page size must not exceed 100"]
  }
}
```

---

## 5. Integrações Externas

| Sistema | Objetivo | Protocolo | Provider |
|---|---|---|---|
| **SMTP (MailKit)** | Envio de emails via SMTP | TCP/SMTP | Qualquer servidor SMTP (Gmail, SES, etc.) |
| **SendGrid** | Envio de emails via API | REST/HTTPS | SendGrid API |
| **Twilio** | Envio de SMS | REST/HTTPS | Twilio API |
| **Firebase Cloud Messaging** | Push Notifications | REST/HTTPS | Google FCM |
| **RabbitMQ** | Message Broker (filas) | AMQP 0.9.1 | Self-hosted |
| **PostgreSQL** | Banco de dados relacional | TCP/SQL | Self-hosted / Cloud |
| **Hangfire** | Agendamento de Jobs | In-process (PostgreSQL storage) | — |

---

## 6. Pontos Críticos ("Gotchas")

### ⚠️ Provider Factory via DB
- Os providers de envio (SMTP, SendGrid, Twilio, Firebase) são **carregados dinamicamente do banco de dados** via `ProviderFactoryBase`. Se não houver um provider ativo configurado na tabela `ProviderConfiguration`, o consumer **ignora silenciosamente** a mensagem (apenas loga warning)
- A configuração JSON do provider é armazenada como texto no banco, e precisa estar no formato correto para desserialização

### ⚠️ DLX/DLQ Dual Declaration
- Tanto o `RabbitMQPublisher` quanto o `RabbitMqConsumerBase` declaram as filas e exchanges (DLX/DLQ). A declaração precisa ser **idempotente** e **idêntica** em ambos os lados, caso contrário o RabbitMQ rejeita com erro de precondition

### ⚠️ Domain Events via Reflection
- `DomainEventDispatcherBehavior` usa **reflection** para extrair `Notification` de dentro de `Result<Notification>`. Isso só funciona se o handler retornar exatamente `Result<Notification>` — qualquer outro tipo de resultado (ex: Result<Guid>) não terá os domain events despachados automaticamente

### ⚠️ ConnectionFactory síncrono no Publisher
- `RabbitMQPublisher` cria a conexão no construtor usando `.GetAwaiter().GetResult()` (blocking). Isso funciona para Singleton mas pode causar deadlock se o escopo for alterado

### ⚠️ Hangfire + Autenticação
- O dashboard Hangfire tem filtros de autenticação diferentes por ambiente: `HangfireDashboardNoAuthFilter` (dev) e `HangfireAuthorizationFilter` (produção). Em dev, o dashboard é **aberto sem autenticação**

### ⚠️ IAuditable é Marker Interface
- Entidades que implementam `IAuditable` são **automaticamente rastreadas** pelo `AuditLogInterceptor` do EF Core. Adicionar `IAuditable` a uma entidade sem saber habilita auditoria automática

---

## 7. Mapa de Navegação

| O que procurar | Caminho |
|---|---|
| **Regras de Negócio (Domain)** | `src/NotificationSystem.Domain/Entities/` |
| **Domain Events** | `src/NotificationSystem.Domain/Events/` |
| **Use Cases (Commands/Queries)** | `src/NotificationSystem.Application/UseCases/` |
| **DTOs** | `src/NotificationSystem.Application/DTOs/` |
| **Validadores** | `src/NotificationSystem.Application/UseCases/*/Validator.cs` e `Application/Validators/` |
| **Pipeline Behaviors (MediatR)** | `src/NotificationSystem.Application/Common/Behaviors/` |
| **Errors Tipados** | `src/NotificationSystem.Application/Common/Errors/` |
| **Interfaces de Repositório** | `src/NotificationSystem.Application/Interfaces/` |
| **Consumer Base (RabbitMQ)** | `src/NotificationSystem.Application/Consumers/` |
| **Settings/Configuration** | `src/NotificationSystem.Application/Configuration/` |
| **Serviços de Aplicação** | `src/NotificationSystem.Application/Services/` |
| **Repositórios (EF Core)** | `src/NotificationSystem.Infrastructure/Persistence/Repositories/` |
| **Entity Configurations** | `src/NotificationSystem.Infrastructure/Persistence/Configurations/` |
| **Migrations** | `src/NotificationSystem.Infrastructure/Migrations/` |
| **Provider Factories** | `src/NotificationSystem.Infrastructure/Factories/` |
| **RabbitMQ Publisher** | `src/NotificationSystem.Infrastructure/Messaging/` |
| **DbContext** | `src/NotificationSystem.Infrastructure/Persistence/NotificationDbContext.cs` |
| **Audit Interceptor** | `src/NotificationSystem.Infrastructure/Persistence/Interceptors/` |
| **Database Seeder** | `src/NotificationSystem.Infrastructure/Persistence/DatabaseSeeder.cs` |
| **Endpoints (Minimal API)** | `src/NotificationSystem.Api/Endpoints/` |
| **Middleware (Exceptions)** | `src/NotificationSystem.Api/Middlewares/` |
| **Extensions** | `src/NotificationSystem.Api/Extensions/` |
| **DI Registration (API)** | `src/NotificationSystem.Api/DependencyInjection.cs` |
| **Program.cs (Entrypoint)** | `src/NotificationSystem.Api/Program.cs` |
| **Workers (Email, SMS, Push, Bulk)** | `src/Consumers/NotificationSystem.Consumer.*/` |
| **Docker** | `docker-compose.yml`, `docker-compose.production.yml`, `src/*/Dockerfile` |
| **Scripts de Migrations** | `scripts/database/` |
| **Documentação** | `docs/` |

---

## 8. Endpoints da API

| Grupo | Endpoints | Autenticação |
|---|---|---|
| **Notifications** | `GET /api/notifications`, `GET /api/notifications/{id}`, `GET /api/notifications/stats`, `POST /api/notifications` | JWT (Permission-based) |
| **Bulk Notifications** | Endpoints de envio em lote | JWT |
| **Auth** | `POST /api/auth/login`, `POST /api/auth/register`, `POST /api/auth/refresh` | Público/JWT |
| **Users** | CRUD de usuários + atribuição de roles | JWT (Admin) |
| **Roles** | CRUD de roles e permissões | JWT (Admin) |
| **Dead Letter Queue** | Visualização e reprocessamento de mensagens DLQ | JWT |
| **Providers** | Configuração dinâmica de providers (SMTP, SendGrid, Twilio, FCM) | JWT |
| **Audit Logs** | Consulta de logs de auditoria | JWT |
| **Hangfire** | Dashboard em `/hangfire` | Dev: sem auth / Prod: com auth |

---

## 9. Fluxo de Dados Principal

```
Client → Minimal API Endpoint → MediatR Pipeline:
    ├── [1] ValidationBehavior (FluentValidation)
    ├── [2] Handler (CreateNotification)
    │     ├── Salva Notification + Channels no PostgreSQL (EF Core)
    │     └── Retorna Result<Notification> com DomainEvents
    └── [3] DomainEventDispatcherBehavior
          └── Publica NotificationCreatedEvent via MediatR
                └── EventHandler publica mensagens no RabbitMQ (por canal)
                      ├── Queue: email-notifications
                      ├── Queue: sms-notifications
                      └── Queue: push-notifications

Workers (BackgroundService) → RabbitMQ Consumer:
    ├── Deserializa mensagem (EmailChannelMessage, etc.)
    ├── MessageProcessingMiddleware (retry com backoff exponencial)
    ├── ProviderFactory → cria provider dinâmico do DB
    ├── Envia via provider (SMTP/SendGrid/Twilio/FCM)
    ├── Atualiza status do canal no PostgreSQL (Sent/Failed)
    └── ACK (sucesso) ou NACK → DLQ (falha permanente)
```

---

## 10. Infraestrutura e Deploy

| Componente | Dev (docker-compose.yml) | Prod (docker-compose.production.yml) |
|---|---|---|
| **API** | dotnet run (local) | Container Docker |
| **Consumer Email** | dotnet run (local) | Container Docker (escalável) |
| **Consumer SMS** | dotnet run (local) | Container Docker (escalável) |
| **Consumer Push** | dotnet run (local) | Container Docker (escalável) |
| **Consumer Bulk** | dotnet run (local) | Container Docker (escalável) |
| **PostgreSQL** | Container Docker local | Externo (RDS, Azure DB, etc.) |
| **RabbitMQ** | Container Docker local | Externo (CloudAMQP, AWS MQ, etc.) |
| **Hangfire** | In-process (PostgreSQL) | In-process (PostgreSQL) |

---

## 11. Modelo de Domínio (Entidades)

```
Notification (Aggregate Root)
├── Id: Guid
├── UserId: Guid
├── CreatedAt: DateTime (UTC)
├── Origin: NotificationOrigin [User | Api | System | Scheduled]
├── Type: NotificationType [Unique | Bulk | Campaign]
├── Channels: List<NotificationChannel>  ←── TPH (Table Per Hierarchy)
│   ├── EmailChannel { To, Subject, Body, IsBodyHtml }
│   ├── SmsChannel { To, Message, SenderId }
│   └── PushChannel { To, Content, Data, Android, Apns, Webpush, Platform, IsRead, ... }
└── DomainEvents: IReadOnlyCollection<IDomainEvent>

User
├── Roles: List<UserRole> → Role → List<RolePermission> → Permission

BulkNotificationJob → List<BulkNotificationItem>

ProviderConfiguration { ChannelType, ProviderType, ConfigurationJson (encrypted), IsActive }

AuditLog { EntityName, EntityId, Action, Changes, Timestamp, UserId }
```

---

## 12. Segurança

| Mecanismo | Status | Detalhes |
|---|---|---|
| **JWT Authentication** | ✅ Implementado | Symmetric key, Token + RefreshToken, Claims-based |
| **Permission-based Authorization** | ✅ Implementado | Policies dinâmicas por permissão, `RequirePermissionAttribute` |
| **RBAC (Roles + Permissions)** | ✅ Implementado | User → Roles → Permissions |
| **Password Hashing** | ✅ Implementado | BCrypt |
| **Data Protection** | ✅ Implementado | ASP.NET Data Protection API (criptografia de configs) |
| **ProblemDetails (RFC 7807)** | ✅ Implementado | Respostas de erro padronizadas |
| **Global Exception Handler** | ✅ Implementado | Sem leak de stack traces em produção |
| **CORS** | ✅ Implementado | AllowAny (dev) — requer configuração para produção |
| **API Key Authentication** | 🔄 Planejado | — |
| **Rate Limiting** | 🔄 Planejado | — |

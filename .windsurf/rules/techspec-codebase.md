---
trigger: model_decision
description: Usar sempre que precisar tomar decisão arquitetural ou técnicas
---

# Tech Spec Reference - NotificationSystem API

Consulte sempre o arquivo `documentos/techspec-codebase.md` para decisões arquiteturais e técnicas. Abaixo um resumo executivo:

## Stack e Arquitetura
- **.NET 10** + **ASP.NET Core Minimal APIs** + **PostgreSQL 16** + **RabbitMQ 3.x** + **Hangfire**
- **Clean Architecture** (4 camadas): Domain → Application → Infrastructure → Presentation (API + Consumers)
- **CQRS** via MediatR com Pipeline Behaviors (Validation + DomainEventDispatcher)
- **Result Pattern** via FluentResults (sem exceptions para fluxo de erros)
- **DDD**: Entities com Domain Events, Aggregate Root (Notification), IAuditable marker interface

## Padrões Obrigatórios
- **UseCases**: pasta por feature com Command/Query (record : IRequest<Result<T>>) + Handler + Validator (opcional)
- **DTOs**: sufixo `Dto`, `Request`, `Response`. Records com `[JsonPolymorphic]` para polimorfismo
- **Interfaces**: prefixo `I` (INotificationRepository, IMessagePublisher)
- **Erros**: hierarquia `DomainError` → NotFoundError, ValidationError, ConflictError, etc. Convertidos para ProblemDetails (RFC 7807) via `ResultExtensions.ToIResult()`
- **Middleware**: `GlobalExceptionHandlerMiddleware` como fallback para exceptions não tratadas
- **Settings**: classes com `SectionName` registradas via `IOptions<T>`

## Consumers e Messaging
- Consumers são **projetos separados** (BackgroundService) que herdam `RabbitMqConsumerBase<TMessage>`
- Filas: `email-notifications`, `sms-notifications`, `push-notifications`, `bulk-notification` (todas com DLX/DLQ)
- `MessageProcessingMiddleware<T>` gerencia retry (exponential backoff) e atualização de status no banco
- **Provider Factories** resolvem dinamicamente qual serviço usar baseado em `ProviderConfiguration` no banco

## Auth e Segurança
- JWT Bearer + Refresh Token
- Authorization por **permissions** (claims), não por roles diretamente
- Permissões centralizadas em `Authorization/Permissions.cs` (formato `resource.action`)
- Endpoints protegidos com `.RequireAuthorization(Permissions.XxxYyy)`

## Auditoria
- `AuditLogInterceptor` (EF Core) captura changes automaticamente para entidades `IAuditable`
- Registra: userId, entityName, entityId, actionType, oldValues, newValues, changedProperties, timestamp, IP

## Convenções de Código
- **Primary constructors** (C# 12) para DI
- **File-scoped namespaces**
- **Records** para Commands, Queries, DTOs, Messages
- **Extension methods** estáticos para DI registration (`AddApplication()`, `AddInfrastructure()`, `AddApiServices()`)
- **Mappings manuais** (extension methods `ToDto()`) — sem AutoMapper

## Mapa de Navegação Rápido
- Regras de negócio: `src/NotificationSystem.Domain/Entities/`
- Use Cases: `src/NotificationSystem.Application/UseCases/`
- Interfaces: `src/NotificationSystem.Application/Interfaces/`
- Pipeline Behaviors: `src/NotificationSystem.Application/Common/Behaviors/`
- Error Types: `src/NotificationSystem.Application/Common/Errors/`
- Endpoints: `src/NotificationSystem.Api/Endpoints/`
- Repositories: `src/NotificationSystem.Infrastructure/Persistence/Repositories/`
- RabbitMQ Publisher: `src/NotificationSystem.Infrastructure/Messaging/RabbitMQPublisher.cs`
- Provider Factories: `src/NotificationSystem.Infrastructure/Factories/`
- Consumers: `src/Consumers/NotificationSystem.Consumer.{Canal}/`
- Config: `src/NotificationSystem.Application/Configuration/`
- Permissions: `src/NotificationSystem.Application/Authorization/Permissions.cs`

## Gotchas Críticos
1. Consumers ignoram mensagens sem `ProviderConfiguration` ativo no banco
2. Stats carregam tudo em memória — problema com volume alto
3. `CreateNotificationHandler` usa `Dictionary<string,object>` com parsing manual de JsonElement
4. RabbitMQ Publisher faz setup síncrono no construtor (`.GetAwaiter().GetResult()`)
5. DatabaseSeeder roda no startup — se banco não estiver pronto, API não sobe

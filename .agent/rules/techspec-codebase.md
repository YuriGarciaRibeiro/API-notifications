---
trigger: always_on
description: Usar sempre que precisar tomar decisão arquitetural ou técnicas
---

# Regra: Tech Spec do Codebase NotificationSystem

Sempre que precisar tomar uma decisão técnica ou arquitetural neste projeto, consulte a documentação completa em `documentos/techspec-codebase.md`.

## Resumo da Arquitetura

- **Stack**: .NET 10 + ASP.NET Core Minimal APIs + RabbitMQ + PostgreSQL + Hangfire
- **Arquitetura**: Clean Architecture + DDD + CQRS (MediatR) + Event-Driven
- **Camadas**: Domain → Application → Infrastructure → Presentation (API + Consumers)

## Regras de Decisão

### Ao criar novos Use Cases
1. Criar pasta em `Application/UseCases/{NomeDoUseCase}/`
2. Arquivos obrigatórios: Command/Query (`IRequest<Result<T>>`), Handler, Response (DTO), Validator (FluentValidation)
3. Retornar `Result<T>` (FluentResults), nunca lançar exceções para controle de fluxo
4. Usar erros tipados de `Application/Common/Errors/` (NotFoundError, ConflictError, etc.)

### Ao criar novos Endpoints
1. Criar classe estática em `Api/Endpoints/{Recurso}Endpoints.cs`
2. Usar Minimal APIs com `MapGroup` e injeção de `IMediator`
3. Converter resultado com `.ToIResult()` (ResultExtensions)
4. Adicionar `.RequireAuthorization(Permissions.X)` para segurança
5. Documentar com `.WithName()`, `.WithSummary()`, `.Produces<T>()`

### Ao adicionar novo Canal de Notificação
1. Criar entidade herdando de `NotificationChannel` (Domain/Entities)
2. Criar `ChannelDto`, `ChannelMessage` (Application/DTOs)
3. Criar Worker herdando de `RabbitMqConsumerBase<TMessage>` (Consumers)
4. Criar Factory herdando de `ProviderFactoryBase<TService>` (Infrastructure/Factories)
5. Adicionar fila no `RabbitMQPublisher.DeclareQueuesAsync()`
6. Adicionar tipo ao enum `ChannelType`

### Ao alterar Entidades
1. Verificar se implementa `IAuditable` (habilita auditoria automática)
2. Criar EF Core Configuration em `Infrastructure/Persistence/Configurations/`
3. Gerar migration: `./scripts/database/add-migration.sh NomeDaMigration`
4. Testar migration: `./scripts/database/migrate.sh`

### Tratamento de Erros
1. **Handlers**: Retornar `Result.Fail(new NotFoundError(...))` — nunca throw
2. **Endpoints**: Usar `result.ToIResult()` para conversão automática
3. **Exceptions não tratadas**: Capturadas pelo `GlobalExceptionHandlerMiddleware`
4. **Formato de resposta**: ProblemDetails (RFC 7807) com traceId e timestamp

### Nomenclatura
- DTOs: sufixo `Dto`, `Request`, `Response`
- Commands: sufixo `Command`
- Queries: sufixo `Query`
- Handlers: sufixo `Handler`
- Validators: sufixo `Validator`
- Settings: sufixo `Settings`
- Interfaces: prefixo `I` (ex: `INotificationRepository`)
- Errors: sufixo `Error` extends `DomainError`

### Injeção de Dependência
- **Application**: registrar em `Application/DependencyInjection.cs`
- **Infrastructure**: registrar em `Infrastructure/DependencyInjection.cs`
- **API-specific**: registrar em `Api/DependencyInjection.cs`
- **Repositories**: Scoped. **Publisher**: Singleton. **Factories**: Scoped

### Documentação completa
Consulte `documentos/techspec-codebase.md` para detalhes sobre stack, dependências, abstrações core, fluxo de dados, integrações, gotchas e mapa de navegação.
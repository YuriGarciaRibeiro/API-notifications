# Arquitetura do Sistema de Notificações

## 📐 Visão Geral

Este projeto implementa **Clean Architecture** (Arquitetura Limpa) combinada com princípios de **Domain-Driven Design (DDD)**, organizando o código em camadas concêntricas onde as dependências sempre apontam para dentro (em direção ao domínio).

## 🎯 Princípios Fundamentais

### 1. Independência de Frameworks
O domínio e a aplicação não dependem de frameworks externos. Você pode trocar ASP.NET por outro framework sem alterar a lógica de negócio.

### 2. Testabilidade
Cada camada pode ser testada isoladamente. O domínio e a aplicação são testáveis sem precisar de banco de dados ou serviços externos.

### 3. Independência de UI
A lógica de negócio não conhece a UI. Pode-se ter API REST, gRPC, GraphQL usando a mesma lógica.

### 4. Independência de Banco de Dados
A lógica de negócio não conhece o banco de dados. Pode-se trocar PostgreSQL por MongoDB sem impacto.

### 5. Independência de Serviços Externos
As integrações (RabbitMQ, Twilio, Firebase) são detalhes de implementação, não parte do core.

## 📚 Camadas Detalhadas

### 🎯 Domain Layer (Núcleo)

**Localização**: `src/NotificationSystem.Domain/`

**Responsabilidade**: Representa as regras de negócio fundamentais do sistema.

**Estrutura**:
```
Domain/
├── Entities/                    # Entidades do domínio
│   ├── Notification.cs          # Entidade principal
│   └── NotificationHistory.cs
├── ValueObjects/                # Objetos de valor imutáveis
│   ├── Email.cs
│   ├── PhoneNumber.cs
│   └── Priority.cs
├── Enums/                       # Enumerações
│   ├── NotificationType.cs
│   ├── NotificationStatus.cs
│   └── DeliveryChannel.cs
├── Events/                      # Domain Events
│   ├── NotificationSentEvent.cs
│   └── NotificationFailedEvent.cs
└── Interfaces/                  # Contratos do domínio
    ├── INotificationRepository.cs
    └── IDomainEventHandler.cs
```

**Características**:
- ❌ Nenhuma dependência externa
- ✅ Apenas lógica de negócio pura
- ✅ Entidades ricas (não anêmicas)
- ✅ Value Objects para garantir invariantes

**Exemplo de Entidade**:
```csharp
public class Notification
{
    public Guid Id { get; private set; }
    public Email Recipient { get; private set; }
    public string Content { get; private set; }
    public Priority Priority { get; private set; }
    public NotificationStatus Status { get; private set; }

    // Lógica de negócio
    public void MarkAsSent()
    {
        if (Status == NotificationStatus.Sent)
            throw new InvalidOperationException("Notification already sent");

        Status = NotificationStatus.Sent;
        AddDomainEvent(new NotificationSentEvent(this));
    }
}
```

---

### 💼 Application Layer

**Localização**: `src/NotificationSystem.Application/`

**Responsabilidade**: Orquestrar os casos de uso da aplicação.

**Estrutura**:
```
Application/
├── UseCases/                    # Casos de uso (CQRS)
│   ├── SendEmailNotification/
│   │   ├── SendEmailNotificationCommand.cs
│   │   ├── SendEmailNotificationHandler.cs
│   │   └── SendEmailNotificationValidator.cs
│   ├── SendSmsNotification/
│   └── GetNotificationHistory/
├── DTOs/                        # Data Transfer Objects
│   ├── NotificationDto.cs
│   ├── EmailRequestDto.cs
│   └── SmsRequestDto.cs
├── Interfaces/                  # Contratos de serviços
│   ├── IEmailService.cs
│   ├── ISmsService.cs
│   ├── IMessagePublisher.cs
│   └── INotificationService.cs
├── Services/                    # Serviços de aplicação
│   └── NotificationService.cs
├── Validators/                  # FluentValidation
│   ├── EmailRequestValidator.cs
│   └── SmsRequestValidator.cs
└── Mappings/                    # AutoMapper profiles
    └── NotificationProfile.cs
```

**Características**:
- ✅ Depende apenas do Domain
- ✅ Define interfaces para serviços externos
- ✅ Implementa casos de uso (Commands/Queries)
- ✅ Validação de entrada com FluentValidation

**Exemplo de Use Case (CQRS)**:
```csharp
public class SendEmailNotificationCommand : IRequest<Result>
{
    public string To { get; set; }
    public string Subject { get; set; }
    public string Body { get; set; }
}

public class SendEmailNotificationHandler
    : IRequestHandler<SendEmailNotificationCommand, Result>
{
    private readonly IMessagePublisher _publisher;
    private readonly INotificationRepository _repository;

    public async Task<Result> Handle(SendEmailNotificationCommand request)
    {
        // 1. Criar entidade do domínio
        var notification = Notification.CreateEmail(
            new Email(request.To),
            request.Subject,
            request.Body
        );

        // 2. Salvar no repositório
        await _repository.AddAsync(notification);

        // 3. Publicar mensagem no RabbitMQ
        await _publisher.PublishAsync("notification.email", notification);

        return Result.Success();
    }
}
```

---

### 🔧 Infrastructure Layer

**Localização**: `src/NotificationSystem.Infrastructure/`

**Responsabilidade**: Implementar detalhes técnicos e integrações.

**Estrutura**:
```
Infrastructure/
├── Messaging/
│   ├── RabbitMQ/                # Configuração RabbitMQ
│   │   ├── RabbitMQConnection.cs
│   │   ├── RabbitMQSettings.cs
│   │   └── RabbitMQHealthCheck.cs
│   ├── Producers/               # Publishers
│   │   └── RabbitMQPublisher.cs
│   └── Consumers/               # Base para consumers
│       └── RabbitMQConsumer.cs
├── Services/
│   ├── Email/                   # Implementação SMTP
│   │   └── SmtpEmailService.cs
│   ├── Sms/                     # Implementação Twilio
│   │   └── TwilioSmsService.cs
│   ├── Push/                    # Implementação Firebase
│   │   └── FirebasePushService.cs
│   └── Webhook/                 # Cliente HTTP
│       └── WebhookService.cs
├── Persistence/
│   ├── Repositories/            # Implementação de repositórios
│   │   └── NotificationRepository.cs
│   ├── Configurations/          # EF Core entity configs
│   │   └── NotificationConfiguration.cs
│   └── NotificationDbContext.cs
└── DependencyInjection.cs       # Registro de serviços
```

**Características**:
- ✅ Implementa interfaces da Application
- ✅ Integrações com serviços externos
- ✅ Persistência com Entity Framework Core
- ✅ Mensageria com RabbitMQ

**Exemplo de Implementação**:
```csharp
public class SmtpEmailService : IEmailService
{
    private readonly SmtpSettings _settings;

    public async Task SendAsync(EmailDto email)
    {
        using var client = new SmtpClient(_settings.Host, _settings.Port);
        var message = new MailMessage
        {
            From = new MailAddress(_settings.From),
            To = { email.To },
            Subject = email.Subject,
            Body = email.Body
        };

        await client.SendMailAsync(message);
    }
}
```

---

### 🌐 Presentation Layer (API + Consumers)

**Localização**: `src/NotificationSystem.Api/` e `src/Consumers/`

**Responsabilidade**: Interface com o mundo externo.

#### API (ASP.NET Core)

```
Api/
├── Controllers/
│   └── NotificationsController.cs
├── Middleware/
│   ├── AuthenticationMiddleware.cs
│   ├── RateLimitingMiddleware.cs
│   └── ExceptionHandlingMiddleware.cs
├── Filters/
│   └── ValidationFilter.cs
├── Extensions/
│   └── ServiceCollectionExtensions.cs
└── Program.cs
```

**Exemplo de Controller**:
```csharp
[ApiController]
[Route("api/[controller]")]
public class NotificationsController : ControllerBase
{
    private readonly IMediator _mediator;

    [HttpPost("email")]
    public async Task<IActionResult> SendEmail(
        [FromBody] SendEmailNotificationCommand command)
    {
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok() : BadRequest(result.Error);
    }
}
```

#### Consumers (Workers)

```
Consumer.Email/
├── Worker.cs                    # BackgroundService
├── EmailMessageHandler.cs       # Processa mensagens
└── Program.cs
```

**Exemplo de Consumer**:
```csharp
public class EmailWorker : BackgroundService
{
    private readonly IRabbitMQConsumer _consumer;
    private readonly IEmailService _emailService;

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        await _consumer.ConsumeAsync<EmailMessage>(
            "notifications.email",
            async (message) =>
            {
                await _emailService.SendAsync(message);
            },
            ct
        );
    }
}
```

---

## 🔄 Fluxo de Dados Completo

### 1️⃣ Envio de Notificação (API → RabbitMQ)

```
Cliente
  ↓ [HTTP POST]
Controller (Presentation)
  ↓ [Command]
UseCase Handler (Application)
  ↓ [cria Entidade]
Domain Entity
  ↓ [salva via interface]
Repository (Infrastructure)
  ↓ [publica via interface]
RabbitMQ Publisher (Infrastructure)
  ↓ [mensagem na fila]
RabbitMQ
```

### 2️⃣ Processamento (RabbitMQ → Consumer → Serviço Externo)

```
RabbitMQ
  ↓ [consume mensagem]
Consumer Worker (Presentation)
  ↓ [chama serviço]
EmailService (Infrastructure)
  ↓ [envia email]
SMTP Server
```

---

## ✅ Vantagens desta Arquitetura

### Testabilidade
```csharp
// Domain: testa lógica pura
[Fact]
public void Notification_MarkAsSent_ShouldChangeStatus()
{
    var notification = new Notification(...);
    notification.MarkAsSent();
    Assert.Equal(NotificationStatus.Sent, notification.Status);
}

// Application: testa com mocks
[Fact]
public async Task Handler_ShouldPublishMessage()
{
    var mockPublisher = new Mock<IMessagePublisher>();
    var handler = new SendEmailHandler(mockPublisher.Object);

    await handler.Handle(command);

    mockPublisher.Verify(x => x.PublishAsync(...), Times.Once);
}
```

### Manutenibilidade
- Cada camada tem responsabilidade clara
- Mudanças em frameworks não afetam o core
- Fácil adicionar novos canais de notificação

### Escalabilidade
- Consumers podem rodar em múltiplas instâncias
- Cada tipo de notificação pode escalar independentemente
- RabbitMQ distribui mensagens automaticamente

---

## 🎯 Regras de Dependência

### ✅ Permitido
- Application → Domain
- Infrastructure → Application, Domain
- API → Application, Infrastructure
- Consumers → Application, Infrastructure

### ❌ Proibido
- Domain → qualquer outra camada
- Application → Infrastructure
- Application → API ou Consumers

---

## 📦 Pacotes NuGet por Camada

### Domain
- Nenhum (puro C#)

### Application
- `MediatR` - CQRS pattern
- `FluentValidation` - Validações
- `AutoMapper` - Mapeamentos

### Infrastructure
- `RabbitMQ.Client` - Mensageria
- `MailKit` - Email
- `Twilio` - SMS
- `FirebaseAdmin` - Push
- `EntityFrameworkCore` - ORM
- `Npgsql.EntityFrameworkCore.PostgreSQL` - Provider PostgreSQL

### API
- `Microsoft.AspNetCore.OpenApi` - Swagger
- `Serilog.AspNetCore` - Logging

---

## 🔜 Próximos Passos

1. Implementar entidades do Domain
2. Criar DTOs na Application
3. Implementar Use Cases com MediatR
4. Configurar RabbitMQ na Infrastructure
5. Implementar serviços de notificação
6. Criar controllers na API
7. Implementar consumers
8. Adicionar testes unitários e de integração

---

**Nota**: Esta arquitetura prioriza manutenibilidade e testabilidade sobre simplicidade inicial. Para projetos menores, pode ser excessiva, mas para sistemas que precisam escalar e evoluir, os benefícios compensam.

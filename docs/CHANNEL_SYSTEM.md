# Sistema de Canais - Arquitetura Multi-Canal

## Visão Geral

O sistema de notificações utiliza uma arquitetura baseada em **canais**, permitindo que uma única notificação seja enviada através de múltiplos meios de comunicação simultaneamente (Email, SMS, Push Notification).

### Conceitos Principais

- **Notification**: Representa a intenção de enviar uma notificação. É o "container" que agrupa um ou mais canais.
- **NotificationChannel**: Representa um meio específico de entrega (Email, SMS ou Push). Cada canal tem seu próprio status, data de envio e tratamento de erro independente.

## Arquitetura de Domínio

### Entidades

#### Notification
Entidade principal que representa uma notificação.

**Localização**: [src/NotificationSystem.Domain/Entities/Notification.cs](../src/NotificationSystem.Domain/Entities/Notification.cs)

```csharp
public class Notification
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public List<NotificationChannel> Channels { get; set; } = new();
}
```

**Campos:**
- `Id`: Identificador único da notificação
- `UserId`: ID do usuário que receberá a notificação
- `CreatedAt`: Data/hora de criação da notificação
- `Channels`: Lista de canais de entrega associados

#### NotificationChannel (Abstrata)
Classe base abstrata para todos os tipos de canal.

**Localização**: [src/NotificationSystem.Domain/Entities/NotificationChannel.cs](../src/NotificationSystem.Domain/Entities/NotificationChannel.cs)

```csharp
public abstract class NotificationChannel
{
    public Guid Id { get; set; }
    public Guid NotificationId { get; set; }
    public Notification Notification { get; set; } = null!;
    public ChannelType Type { get; set; }
    public NotificationStatus Status { get; set; } = NotificationStatus.Pending;
    public string? ErrorMessage { get; set; }
    public DateTime? SentAt { get; set; }
}
```

**Campos Comuns:**
- `Id`: Identificador único do canal
- `NotificationId`: Foreign key para a notificação pai
- `Notification`: Navegação para a notificação pai
- `Type`: Tipo do canal (Email, SMS, Push)
- `Status`: Status de entrega (Pending, Sent, Failed)
- `ErrorMessage`: Mensagem de erro em caso de falha
- `SentAt`: Data/hora de envio bem-sucedido

#### EmailChannel
Canal de entrega via Email.

```csharp
public class EmailChannel : NotificationChannel
{
    public string To { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public bool IsBodyHtml { get; set; } = false;
}
```

**Campos Específicos:**
- `To`: Endereço de email do destinatário
- `Subject`: Assunto do email
- `Body`: Corpo do email (texto ou HTML)
- `IsBodyHtml`: Indica se o corpo é HTML

#### SmsChannel
Canal de entrega via SMS.

```csharp
public class SmsChannel : NotificationChannel
{
    public string To { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? SenderId { get; set; }
}
```

**Campos Específicos:**
- `To`: Número de telefone do destinatário (formato internacional)
- `Message`: Texto da mensagem SMS (máx. 1600 caracteres)
- `SenderId`: ID do remetente (opcional)

#### PushChannel
Canal de entrega via Push Notification.

```csharp
public class PushChannel : NotificationChannel
{
    public string To { get; set; } = string.Empty;
    public NotificationContent Content { get; set; } = new();
    public Dictionary<string, string> Data { get; set; } = new();
    public AndroidConfig? Android { get; set; }
    public ApnsConfig? Apns { get; set; }
    public WebpushConfig? Webpush { get; set; }
    public string? Condition { get; set; }
    public int? TimeToLive { get; set; }
    public string? Priority { get; set; }
    public bool? MutableContent { get; set; }
    public bool? ContentAvailable { get; set; }
    public bool IsRead { get; set; } = false;
}
```

**Campos Específicos:**
- `To`: Token do dispositivo
- `Content`: Conteúdo da notificação (título, corpo, ação de clique)
- `Data`: Dados customizados (chave-valor)
- `Android`: Configurações específicas do Android
- `Apns`: Configurações específicas do iOS (Apple Push Notification Service)
- `Webpush`: Configurações específicas do Web Push
- `Condition`: Condição para envio (ex: "TopicA && TopicB")
- `TimeToLive`: Tempo de vida da mensagem em segundos
- `Priority`: Prioridade de entrega (high, normal)
- `MutableContent`: Permite modificação do conteúdo (iOS)
- `ContentAvailable`: Notificação silenciosa (iOS)
- `IsRead`: Indica se a notificação foi lida

### Enums

#### ChannelType
```csharp
public enum ChannelType
{
    Email,
    Sms,
    Push
}
```

#### NotificationStatus
```csharp
public enum NotificationStatus
{
    Pending,  // Aguardando envio
    Sent,     // Enviado com sucesso
    Failed    // Falha no envio
}
```

## Estrutura do Banco de Dados

O sistema utiliza a estratégia **Table Per Hierarchy (TPH)** do Entity Framework Core, criando 2 tabelas principais:

### Tabela: `notifications`

Armazena as notificações (containers).

```sql
CREATE TABLE notifications (
    id UUID PRIMARY KEY,
    user_id UUID NOT NULL,
    created_at TIMESTAMP NOT NULL DEFAULT NOW()
);

CREATE INDEX ix_notifications_user_id ON notifications(user_id);
CREATE INDEX ix_notifications_created_at ON notifications(created_at);
```

### Tabela: `notification_channels`

Armazena todos os canais usando herança polimórfica com discriminador no campo `type`.

```sql
CREATE TABLE notification_channels (
    -- Campos comuns
    id UUID PRIMARY KEY,
    notification_id UUID NOT NULL REFERENCES notifications(id) ON DELETE CASCADE,
    type VARCHAR(20) NOT NULL,  -- Discriminador: 'Email', 'Sms', 'Push'
    status VARCHAR(20) NOT NULL,
    error_message VARCHAR(1000),
    sent_at TIMESTAMP,

    -- Campos específicos do EmailChannel
    to VARCHAR(500),
    subject VARCHAR(500),
    body TEXT,
    is_body_html BOOLEAN,

    -- Campos específicos do SmsChannel
    message VARCHAR(1600),
    sender_id VARCHAR(50),

    -- Campos específicos do PushChannel
    content_title VARCHAR(100),
    content_body VARCHAR(500),
    content_click_action VARCHAR(200),
    data JSONB,
    android_config JSONB,
    apns_config JSONB,
    webpush_config JSONB,
    condition VARCHAR(200),
    time_to_live INTEGER,
    priority VARCHAR(20),
    mutable_content BOOLEAN,
    content_available BOOLEAN,
    is_read BOOLEAN DEFAULT FALSE
);

CREATE INDEX ix_channels_notification_id ON notification_channels(notification_id);
CREATE INDEX ix_channels_status ON notification_channels(status);
CREATE INDEX ix_channels_notification_id_type ON notification_channels(notification_id, type);
```

### Relacionamento 1:N

- **1 Notification** pode ter **N Channels**
- Cada **Channel** pertence a **1 Notification**
- Foreign Key: `notification_id` em `notification_channels`
- **Cascade Delete**: Ao deletar uma Notification, todos os seus Channels são deletados automaticamente

### Como funciona o Table Per Hierarchy (TPH)?

1. **Uma única tabela**: Todos os tipos de canal (Email, SMS, Push) ficam na mesma tabela `notification_channels`
2. **Discriminador**: O campo `type` indica qual tipo de canal é (`Email`, `Sms`, `Push`)
3. **Campos nullable**: Campos específicos de cada tipo ficam NULL quando não aplicáveis
4. **Filtragem automática**: O EF Core adiciona automaticamente `WHERE type = 'Email'` quando você consulta `_context.EmailChannels`

**Exemplo de dados:**

| id | notification_id | type | status | to | subject | body | message | sender_id |
|----|----------------|------|--------|-----|---------|------|---------|-----------|
| ch-1 | notif-1 | Email | Sent | user@email.com | "Alert" | "You have..." | NULL | NULL |
| ch-2 | notif-1 | Sms | Sent | +5511999... | NULL | NULL | "Alert: You..." | "MyApp" |
| ch-3 | notif-1 | Push | Sent | device-token | NULL | NULL | NULL | NULL |

## Configuração do Entity Framework Core

### NotificationConfiguration

**Localização**: [src/NotificationSystem.Infrastructure/Persistence/Configurations/NotificationConfiguration.cs](../src/NotificationSystem.Infrastructure/Persistence/Configurations/NotificationConfiguration.cs)

```csharp
public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("notifications");
        builder.HasKey(n => n.Id);

        builder.Property(n => n.Id).HasColumnName("id").IsRequired();
        builder.Property(n => n.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(n => n.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired()
            .HasDefaultValueSql("NOW()");

        // Relacionamento 1:N com Cascade Delete
        builder.HasMany(n => n.Channels)
            .WithOne(c => c.Notification)
            .HasForeignKey(c => c.NotificationId)
            .OnDelete(DeleteBehavior.Cascade);

        // Índices para performance
        builder.HasIndex(n => n.UserId).HasDatabaseName("ix_notifications_user_id");
        builder.HasIndex(n => n.CreatedAt).HasDatabaseName("ix_notifications_created_at");
    }
}
```

### NotificationChannelConfiguration

**Localização**: [src/NotificationSystem.Infrastructure/Persistence/Configurations/NotificationChannelConfiguration.cs](../src/NotificationSystem.Infrastructure/Persistence/Configurations/NotificationChannelConfiguration.cs)

```csharp
public class NotificationChannelConfiguration : IEntityTypeConfiguration<NotificationChannel>
{
    public void Configure(EntityTypeBuilder<NotificationChannel> builder)
    {
        builder.ToTable("notification_channels");
        builder.HasKey(c => c.Id);

        // Campos comuns
        builder.Property(c => c.Type)
            .HasColumnName("type")
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(c => c.Status)
            .HasColumnName("status")
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        // Configuração do discriminador (TPH)
        builder.HasDiscriminator(c => c.Type)
            .HasValue<EmailChannel>(ChannelType.Email)
            .HasValue<SmsChannel>(ChannelType.Sms)
            .HasValue<PushChannel>(ChannelType.Push);

        // Índices
        builder.HasIndex(c => c.NotificationId).HasDatabaseName("ix_channels_notification_id");
        builder.HasIndex(c => c.Status).HasDatabaseName("ix_channels_status");
        builder.HasIndex(c => new { c.NotificationId, c.Type })
            .HasDatabaseName("ix_channels_notification_id_type");
    }
}
```

### Configurações Específicas de Canais

- **EmailChannelConfiguration**: [src/NotificationSystem.Infrastructure/Persistence/Configurations/EmailChannelConfiguration.cs](../src/NotificationSystem.Infrastructure/Persistence/Configurations/EmailChannelConfiguration.cs)
- **SmsChannelConfiguration**: [src/NotificationSystem.Infrastructure/Persistence/Configurations/SmsChannelConfiguration.cs](../src/NotificationSystem.Infrastructure/Persistence/Configurations/SmsChannelConfiguration.cs)
- **PushChannelConfiguration**: [src/NotificationSystem.Infrastructure/Persistence/Configurations/PushChannelConfiguration.cs](../src/NotificationSystem.Infrastructure/Persistence/Configurations/PushChannelConfiguration.cs)

## Camada de Aplicação

### DTOs

**Localização**: [src/NotificationSystem.Application/UseCases/GetAllNotifications/GetAllNotificationsResponse.cs](../src/NotificationSystem.Application/UseCases/GetAllNotifications/GetAllNotificationsResponse.cs)

#### NotificationDto
```csharp
public record NotificationDto
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public DateTime CreatedAt { get; init; }
    public List<ChannelDto> Channels { get; init; } = new();
}
```

#### ChannelDto (Polimórfico)
```csharp
[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(EmailChannelDto), "Email")]
[JsonDerivedType(typeof(SmsChannelDto), "Sms")]
[JsonDerivedType(typeof(PushChannelDto), "Push")]
public abstract record ChannelDto
{
    public Guid Id { get; init; }
    public NotificationStatus Status { get; init; }
    public string? ErrorMessage { get; init; }
    public DateTime? SentAt { get; init; }
}
```

**Serializaçã JSON com discriminador:**
```json
{
  "id": "abc-123",
  "userId": "user-456",
  "createdAt": "2025-12-10T10:00:00Z",
  "channels": [
    {
      "type": "Email",
      "id": "ch-1",
      "status": "Sent",
      "to": "user@example.com",
      "subject": "Alert",
      "body": "You have a new message"
    },
    {
      "type": "Sms",
      "id": "ch-2",
      "status": "Sent",
      "to": "+5511999999999",
      "message": "Alert: You have a new message"
    }
  ]
}
```

### Mappings

**Localização**: [src/NotificationSystem.Application/Common/Mappings/NotificationMappings.cs](../src/NotificationSystem.Application/Common/Mappings/NotificationMappings.cs)

```csharp
public static class NotificationMappings
{
    public static NotificationDto ToDto(this Notification notification)
    {
        return new NotificationDto
        {
            Id = notification.Id,
            UserId = notification.UserId,
            CreatedAt = notification.CreatedAt,
            Channels = notification.Channels.Select(c => c.ToDto()).ToList()
        };
    }

    public static ChannelDto ToDto(this NotificationChannel channel)
    {
        return channel switch
        {
            EmailChannel email => new EmailChannelDto { /* ... */ },
            SmsChannel sms => new SmsChannelDto { /* ... */ },
            PushChannel push => new PushChannelDto { /* ... */ },
            _ => throw new InvalidOperationException($"Unknown channel type: {channel.GetType().Name}")
        };
    }
}
```

## Repositório

**Localização**: [src/NotificationSystem.Infrastructure/Persistence/Repositories/NotificationRepository.cs](../src/NotificationSystem.Infrastructure/Persistence/Repositories/NotificationRepository.cs)

### Queries Comuns

#### Buscar notificação com todos os canais
```csharp
public async Task<Notification?> GetByIdAsync(Guid id)
{
    return await _context.Notifications
        .Include(n => n.Channels)  // Eager loading dos canais
        .FirstOrDefaultAsync(n => n.Id == id);
}
```

#### Buscar notificações com canais pendentes
```csharp
public async Task<IEnumerable<Notification>> GetPendingNotificationsAsync(int maxCount)
{
    return await _context.Notifications
        .Include(n => n.Channels)
        .Where(n => n.Channels.Any(c => c.Status == NotificationStatus.Pending))
        .Take(maxCount)
        .ToListAsync();
}
```

#### Buscar apenas canais de Email pendentes
```csharp
var emailChannels = await _context.EmailChannels
    .Where(e => e.Status == NotificationStatus.Pending)
    .Include(e => e.Notification)  // Se precisar dos dados da notificação
    .ToListAsync();
```

#### Criar notificação multi-canal
```csharp
var notification = new Notification
{
    Id = Guid.NewGuid(),
    UserId = userId,
    Channels = new List<NotificationChannel>
    {
        new EmailChannel
        {
            Id = Guid.NewGuid(),
            To = "user@example.com",
            Subject = "Security Alert",
            Body = "New login detected"
        },
        new SmsChannel
        {
            Id = Guid.NewGuid(),
            To = "+5511999999999",
            Message = "Security Alert: New login"
        }
    }
};

await _context.Notifications.AddAsync(notification);
await _context.SaveChangesAsync();
```

## Exemplos de Uso

### 1. Notificação com 1 canal (Email)
```csharp
var notification = new Notification
{
    UserId = userId,
    Channels = new List<NotificationChannel>
    {
        new EmailChannel
        {
            To = "user@example.com",
            Subject = "Welcome!",
            Body = "Welcome to our platform"
        }
    }
};
```

### 2. Notificação multi-canal (Email + SMS)
```csharp
var notification = new Notification
{
    UserId = userId,
    Channels = new List<NotificationChannel>
    {
        new EmailChannel
        {
            To = "user@example.com",
            Subject = "Appointment Reminder",
            Body = "You have an appointment tomorrow at 2 PM"
        },
        new SmsChannel
        {
            To = "+5511999999999",
            Message = "Reminder: Appointment tomorrow at 2 PM"
        }
    }
};
```

### 3. Notificação completa (Email + SMS + Push)
```csharp
var notification = new Notification
{
    UserId = userId,
    Channels = new List<NotificationChannel>
    {
        new EmailChannel
        {
            To = "user@example.com",
            Subject = "Security Alert",
            Body = "New login detected from unknown device",
            IsBodyHtml = true
        },
        new SmsChannel
        {
            To = "+5511999999999",
            Message = "Security Alert: New login detected",
            SenderId = "MyApp"
        },
        new PushChannel
        {
            To = "device-token-123",
            Content = new NotificationContent
            {
                Title = "Security Alert",
                Body = "New login detected",
                ClickAction = "/security"
            },
            Priority = "high"
        }
    }
};
```

### 4. Atualizar status de um canal específico
```csharp
var notification = await _context.Notifications
    .Include(n => n.Channels)
    .FirstOrDefaultAsync(n => n.Id == notificationId);

var emailChannel = notification.Channels.OfType<EmailChannel>().First();
emailChannel.Status = NotificationStatus.Sent;
emailChannel.SentAt = DateTime.UtcNow;

await _context.SaveChangesAsync();
```

### 5. Tratar falha em um canal
```csharp
var smsChannel = notification.Channels.OfType<SmsChannel>().First();
smsChannel.Status = NotificationStatus.Failed;
smsChannel.ErrorMessage = "Invalid phone number";

await _context.SaveChangesAsync();
```

## Migrations

### Criar Migration
```bash
cd src/NotificationSystem.Infrastructure
dotnet ef migrations add RefactorToChannelModel -s ../NotificationSystem.Api
```

### Aplicar Migration
```bash
dotnet ef database update -s ../NotificationSystem.Api
```

### Reverter Migration
```bash
dotnet ef database update PreviousMigrationName -s ../NotificationSystem.Api
```

## Vantagens da Arquitetura

### Flexibilidade
- Uma notificação pode ter 1 ou mais canais
- Fácil adicionar novos tipos de canal (ex: WhatsApp, Telegram)
- Canais independentes: Email pode ter sucesso enquanto SMS falha

### Status Independente
- Cada canal tem seu próprio status de entrega
- Permite rastreamento granular de falhas
- Facilita retry de canais específicos

### Escalabilidade
- Canais podem ser processados em paralelo
- Cada consumer (Email, SMS, Push) pode escalar independentemente
- Fácil implementar priorização por tipo de canal

### Manutenibilidade
- Código bem estruturado e organizado
- Validação independente por tipo de canal
- Testes unitários focados em cada canal

## Boas Práticas

### 1. Sempre use Include para carregar canais
```csharp
//  Correto
var notification = await _context.Notifications
    .Include(n => n.Channels)
    .FirstOrDefaultAsync(n => n.Id == id);

// L Errado - Lazy loading não está habilitado
var notification = await _context.Notifications.FindAsync(id);
// notification.Channels será vazio!
```

### 2. Valide os canais antes de salvar
```csharp
// Use FluentValidation para validar cada tipo de canal
public class EmailChannelValidator : AbstractValidator<EmailChannel>
{
    public EmailChannelValidator()
    {
        RuleFor(x => x.To).NotEmpty().EmailAddress();
        RuleFor(x => x.Subject).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Body).NotEmpty();
    }
}
```

### 3. Use transações para operações complexas
```csharp
using var transaction = await _context.Database.BeginTransactionAsync();
try
{
    await _context.Notifications.AddAsync(notification);
    await _context.SaveChangesAsync();

    // Outras operações...

    await transaction.CommitAsync();
}
catch
{
    await transaction.RollbackAsync();
    throw;
}
```

### 4. Não exponha entidades de domínio na API
```csharp
//  Correto - Use DTOs
return notification.ToDto();

// L Errado - Expõe entidade de domínio
return notification;
```

### 5. Use queries específicas quando possível
```csharp
//  Mais eficiente - Filtra no banco
var emailChannels = await _context.EmailChannels
    .Where(e => e.Status == NotificationStatus.Pending)
    .ToListAsync();

// L Menos eficiente - Carrega tudo e filtra em memória
var channels = await _context.NotificationChannels.ToListAsync();
var emailChannels = channels.OfType<EmailChannel>()
    .Where(e => e.Status == NotificationStatus.Pending);
```

## Troubleshooting

### Canal não está sendo incluído
**Problema**: `notification.Channels` está vazio após buscar do banco.

**Solução**: Use `.Include(n => n.Channels)` na query.

### Discriminador não funciona
**Problema**: EF Core não está filtrando por tipo corretamente.

**Solução**: Verifique se a configuração do discriminador está correta em `NotificationChannelConfiguration`.

### Campos específicos sempre NULL
**Problema**: Campos de `EmailChannel` estão NULL mesmo após salvar.

**Solução**: Verifique se a configuração específica (`EmailChannelConfiguration`) está sendo aplicada.

### Migration falha
**Problema**: Erro ao executar migration.

**Solução**:
1. Verifique se todas as configurações estão corretas
2. Use `dotnet ef migrations remove` para remover a última migration
3. Recrie a migration

## Próximos Passos

- [ ] Implementar consumers para processar cada tipo de canal
- [ ] Adicionar validação com FluentValidation para cada canal
- [ ] Implementar retry logic para canais falhados
- [ ] Adicionar logging e telemetria
- [ ] Criar testes de integração para o sistema de canais
- [ ] Implementar rate limiting por tipo de canal

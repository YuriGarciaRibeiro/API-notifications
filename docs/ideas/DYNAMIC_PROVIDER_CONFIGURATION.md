# Dynamic Provider Configuration - Configuração Dinâmica de Provedores

## 📋 Visão Geral

Esta documentação descreve uma **melhoria futura** para permitir que os usuários configurem dinamicamente os provedores de notificação (Twilio, AWS SNS, Firebase, etc.) via interface administrativa/API, em vez de configurar tudo no `appsettings.json` antes de iniciar a aplicação.

### Status
🔄 **Planejado** - Não implementado

### Motivação

Atualmente, para trocar de provedor (ex: de Twilio para AWS SNS), é necessário:
1. Editar `appsettings.json` ou variáveis de ambiente
2. Reiniciar a aplicação/container
3. Não há suporte para múltiplos provedores simultâneos
4. Credenciais ficam em arquivos de configuração

Com esta melhoria, será possível:
- ✅ Cadastrar múltiplos provedores via API
- ✅ Trocar entre provedores sem reiniciar a aplicação
- ✅ Gerenciar credenciais de forma centralizada no banco de dados
- ✅ Ter fallback automático se o provedor primário falhar
- ✅ Interface administrativa para gerenciar configurações

---

## 🏗️ Arquitetura Proposta

### Fluxo Atual (Hard-coded)
```
appsettings.json → TwilioService (fixo) → Envio SMS
```

### Fluxo Proposto (Dinâmico)
```
Banco de Dados → ProviderFactory → TwilioService | AwsSnsService | NexmoService
                                        ↓
                                    Envio SMS
```

---

## 📦 1. Domain Layer - Novas Entidades

### `ProviderConfiguration.cs`
Entidade para armazenar configurações de provedores no banco de dados.

```csharp
// src/NotificationSystem.Domain/Entities/ProviderConfiguration.cs
public class ProviderConfiguration
{
    public Guid Id { get; set; }

    /// <summary>
    /// Tipo de canal (Email, Sms, Push)
    /// </summary>
    public ChannelType ChannelType { get; set; }

    /// <summary>
    /// Nome do provedor (Twilio, AwsSns, Firebase, etc)
    /// </summary>
    public string ProviderType { get; set; } = string.Empty;

    /// <summary>
    /// Configuração em JSON (credenciais, settings)
    /// IMPORTANTE: Deve ser criptografado em produção
    /// </summary>
    public string ConfigurationJson { get; set; } = string.Empty;

    /// <summary>
    /// Se o provedor está ativo
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Se é o provedor primário para este canal
    /// Apenas um provedor pode ser primário por canal
    /// </summary>
    public bool IsPrimary { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
}
```

### Novos Enums

```csharp
// src/NotificationSystem.Domain/Enums/SmsProviderType.cs
public enum SmsProviderType
{
    Twilio,
    AwsSns,
    Nexmo,
    MessageBird,
    Vonage
}

// src/NotificationSystem.Domain/Enums/PushProviderType.cs
public enum PushProviderType
{
    Firebase,
    AwsSns,
    OneSignal,
    ApplePushNotificationService
}

// src/NotificationSystem.Domain/Enums/EmailProviderType.cs
public enum EmailProviderType
{
    Smtp,           // Genérico (atual)
    SendGrid,
    AwsSes,
    Mailgun,
    Postmark
}
```

---

## 🗄️ 2. Infrastructure Layer

### Repository Pattern

```csharp
// src/NotificationSystem.Application/Interfaces/IProviderConfigurationRepository.cs
public interface IProviderConfigurationRepository
{
    /// <summary>
    /// Obtém o provedor ativo (primário) para um canal específico
    /// </summary>
    Task<ProviderConfiguration?> GetActiveProviderAsync(ChannelType channelType);

    /// <summary>
    /// Lista todos os provedores, opcionalmente filtrados por canal
    /// </summary>
    Task<List<ProviderConfiguration>> GetAllProvidersAsync(ChannelType? channelType = null);

    Task<ProviderConfiguration?> GetByIdAsync(Guid id);
    Task<ProviderConfiguration> CreateAsync(ProviderConfiguration config);
    Task<ProviderConfiguration> UpdateAsync(ProviderConfiguration config);
    Task DeleteAsync(Guid id);

    /// <summary>
    /// Define um provedor como primário, desativando os outros do mesmo canal
    /// </summary>
    Task SetAsPrimaryAsync(Guid id, ChannelType channelType);
}
```

### Provider Factory (Strategy Pattern)

```csharp
// src/NotificationSystem.Application/Interfaces/ISmsProviderFactory.cs
public interface ISmsProviderFactory
{
    /// <summary>
    /// Cria uma instância do provedor SMS ativo dinamicamente
    /// </summary>
    Task<ISmsService> CreateProviderAsync();
}

// src/NotificationSystem.Infrastructure/Services/Sms/SmsProviderFactory.cs
public class SmsProviderFactory : ISmsProviderFactory
{
    private readonly IProviderConfigurationRepository _providerRepo;
    private readonly ILogger<SmsProviderFactory> _logger;
    private readonly IServiceProvider _serviceProvider;

    public async Task<ISmsService> CreateProviderAsync()
    {
        // 1. Busca provedor ativo no banco
        var config = await _providerRepo.GetActiveProviderAsync(ChannelType.Sms);

        if (config == null)
            throw new InvalidOperationException("No active SMS provider configured");

        // 2. Cria instância baseado no tipo
        return config.ProviderType switch
        {
            "Twilio" => CreateTwilioProvider(config),
            "AwsSns" => CreateAwsSnsProvider(config),
            "Nexmo" => CreateNexmoProvider(config),
            _ => throw new NotSupportedException($"Provider '{config.ProviderType}' not supported")
        };
    }

    private ISmsService CreateTwilioProvider(ProviderConfiguration config)
    {
        var settings = JsonSerializer.Deserialize<TwilioSettings>(config.ConfigurationJson);
        return new TwilioService(Options.Create(settings), _logger);
    }

    // Métodos similares para outros provedores...
}
```

### Implementação AWS SNS (Exemplo de Novo Provedor)

```csharp
// src/NotificationSystem.Infrastructure/Services/Sms/AwsSnsService.cs
public class AwsSnsService : ISmsService
{
    private readonly AwsSnsSettings _settings;
    private readonly ILogger<AwsSnsService> _logger;
    private readonly AmazonSimpleNotificationServiceClient _snsClient;

    public AwsSnsService(IOptions<AwsSnsSettings> settings, ILogger<AwsSnsService> logger)
    {
        _settings = settings.Value;
        _logger = logger;

        var awsCredentials = new BasicAWSCredentials(
            _settings.AccessKeyId,
            _settings.SecretAccessKey
        );

        _snsClient = new AmazonSimpleNotificationServiceClient(
            awsCredentials,
            RegionEndpoint.GetBySystemName(_settings.Region)
        );
    }

    public async Task SendSmsAsync(string to, string message)
    {
        try
        {
            var request = new PublishRequest
            {
                PhoneNumber = to,
                Message = message,
                MessageAttributes = new Dictionary<string, MessageAttributeValue>
                {
                    {
                        "AWS.SNS.SMS.SenderID",
                        new MessageAttributeValue
                        {
                            StringValue = _settings.SenderId,
                            DataType = "String"
                        }
                    },
                    {
                        "AWS.SNS.SMS.SMSType",
                        new MessageAttributeValue
                        {
                            StringValue = "Transactional",
                            DataType = "String"
                        }
                    }
                }
            };

            var response = await _snsClient.PublishAsync(request);

            _logger.LogInformation(
                "SMS sent via AWS SNS. MessageId: {MessageId}, To: {To}",
                response.MessageId, to);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending SMS via AWS SNS to {To}", to);
            throw;
        }
    }
}

public class AwsSnsSettings
{
    public string AccessKeyId { get; set; } = string.Empty;
    public string SecretAccessKey { get; set; } = string.Empty;
    public string Region { get; set; } = "us-east-1";
    public string? SenderId { get; set; }
}
```

---

## 🌐 3. API - Endpoints Administrativos

### Provider Management Endpoints

```csharp
// src/NotificationSystem.Api/Endpoints/ProviderEndpoints.cs
public static class ProviderEndpoints
{
    public static void MapProviderEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/admin/providers")
            .WithTags("Provider Configuration")
            .RequireAuthorization(); // Apenas admins

        // GET /api/admin/providers
        // Lista todos os provedores configurados
        group.MapGet("/", async (
            IProviderConfigurationRepository repo,
            [FromQuery] ChannelType? channelType) =>
        {
            var providers = await repo.GetAllProvidersAsync(channelType);
            return Results.Ok(providers);
        });

        // GET /api/admin/providers/sms/active
        // Retorna o provedor ativo de SMS
        group.MapGet("/{channelType}/active", async (
            ChannelType channelType,
            IProviderConfigurationRepository repo) =>
        {
            var provider = await repo.GetActiveProviderAsync(channelType);
            return provider != null ? Results.Ok(provider) : Results.NotFound();
        });

        // POST /api/admin/providers
        // Cria novo provedor
        group.MapPost("/", async (
            CreateProviderRequest request,
            IProviderConfigurationRepository repo) =>
        {
            var config = new ProviderConfiguration
            {
                ChannelType = request.ChannelType,
                ProviderType = request.ProviderType,
                ConfigurationJson = JsonSerializer.Serialize(request.Configuration),
                IsActive = request.IsActive,
                IsPrimary = request.IsPrimary,
                CreatedBy = "admin" // TODO: pegar do contexto de autenticação
            };

            var created = await repo.CreateAsync(config);
            return Results.Created($"/api/admin/providers/{created.Id}", created);
        });

        // PUT /api/admin/providers/{id}
        // Atualiza configuração de provedor existente
        group.MapPut("/{id}", async (
            Guid id,
            UpdateProviderRequest request,
            IProviderConfigurationRepository repo) =>
        {
            var existing = await repo.GetByIdAsync(id);
            if (existing == null)
                return Results.NotFound();

            existing.ConfigurationJson = JsonSerializer.Serialize(request.Configuration);
            existing.IsActive = request.IsActive;

            var updated = await repo.UpdateAsync(existing);
            return Results.Ok(updated);
        });

        // POST /api/admin/providers/{id}/set-primary
        // Define provedor como primário (ativa e desativa outros)
        group.MapPost("/{id}/set-primary", async (
            Guid id,
            IProviderConfigurationRepository repo) =>
        {
            var provider = await repo.GetByIdAsync(id);
            if (provider == null)
                return Results.NotFound();

            await repo.SetAsPrimaryAsync(id, provider.ChannelType);
            return Results.NoContent();
        });

        // DELETE /api/admin/providers/{id}
        group.MapDelete("/{id}", async (
            Guid id,
            IProviderConfigurationRepository repo) =>
        {
            await repo.DeleteAsync(id);
            return Results.NoContent();
        });

        // POST /api/admin/providers/{id}/test
        // Testa se o provedor está funcionando
        group.MapPost("/{id}/test", async (
            Guid id,
            IProviderConfigurationRepository repo) =>
        {
            // TODO: Implementar teste de conectividade
            return Results.Ok(new { message = "Test not implemented yet" });
        });
    }
}
```

### DTOs

```csharp
public record CreateProviderRequest(
    ChannelType ChannelType,
    string ProviderType,
    object Configuration,  // JSON livre baseado no provedor
    bool IsActive = true,
    bool IsPrimary = false
);

public record UpdateProviderRequest(
    object Configuration,
    bool IsActive
);
```

---

## 🔄 4. Consumer - Uso do Factory

### Atualização do Worker SMS

```csharp
// src/Consumers/NotificationSystem.Consumer.Sms/Worker.cs
public class Worker : RabbitMqConsumerBase<SmsChannelMessage>
{
    private readonly ISmsProviderFactory _providerFactory;  // ✅ Factory em vez de service fixo
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public Worker(
        IOptions<RabbitMqOptions> rabbitMqOptions,
        ISmsProviderFactory providerFactory,
        IServiceScopeFactory serviceScopeFactory,
        ILogger<Worker> logger)
        : base(rabbitMqOptions, "sms-notifications", logger)
    {
        _providerFactory = providerFactory;
        _serviceScopeFactory = serviceScopeFactory;
    }

    protected override async Task ProcessMessageAsync(SmsChannelMessage message)
    {
        try
        {
            // ✅ Cria provider dinamicamente baseado na config do banco
            var smsService = await _providerFactory.CreateProviderAsync();

            _logger.LogInformation(
                "Sending SMS via {ProviderType} for channel {ChannelId}",
                smsService.GetType().Name,
                message.ChannelId);

            await smsService.SendSmsAsync(message.To, message.Message);

            // Atualizar status como Sent
            using var scope = _serviceScopeFactory.CreateScope();
            var repository = scope.ServiceProvider
                .GetRequiredService<INotificationRepository>();

            await repository.UpdateNotificationChannelStatusAsync<SmsChannel>(
                message.NotificationId,
                message.ChannelId,
                NotificationStatus.Sent
            );

            _logger.LogInformation("SMS sent successfully via {ProviderType}",
                smsService.GetType().Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending SMS");
            // Atualizar como Failed e re-throw para retry...
            throw;
        }
    }
}
```

### Registro de Dependências

```csharp
// src/Consumers/NotificationSystem.Consumer.Sms/Program.cs
var builder = Host.CreateApplicationBuilder(args);

// Serilog
builder.Services.AddSerilog(...);

// Database
builder.Services.AddDbContext<ApplicationDbContext>(...);
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<IProviderConfigurationRepository, ProviderConfigurationRepository>();

// ✅ Registrar Factory em vez de Service específico
builder.Services.AddScoped<ISmsProviderFactory, SmsProviderFactory>();

// Worker
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
```

---

## 💾 5. Database Migration

```csharp
// dotnet ef migrations add AddProviderConfiguration

public partial class AddProviderConfiguration : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "provider_configurations",
            columns: table => new
            {
                id = table.Column<Guid>(nullable: false),
                channel_type = table.Column<int>(nullable: false),
                provider_type = table.Column<string>(maxLength: 50, nullable: false),
                configuration_json = table.Column<string>(type: "jsonb", nullable: false),
                is_active = table.Column<bool>(nullable: false),
                is_primary = table.Column<bool>(nullable: false),
                created_at = table.Column<DateTime>(nullable: false),
                updated_at = table.Column<DateTime>(nullable: true),
                created_by = table.Column<string>(maxLength: 100, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_provider_configurations", x => x.id);
            });

        // Índice único: apenas um provedor primário por canal
        migrationBuilder.CreateIndex(
            name: "ix_provider_configurations_channel_type_is_primary",
            table: "provider_configurations",
            columns: new[] { "channel_type", "is_primary" },
            unique: true,
            filter: "is_primary = true");

        // Índice para busca por canal
        migrationBuilder.CreateIndex(
            name: "ix_provider_configurations_channel_type",
            table: "provider_configurations",
            column: "channel_type");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "provider_configurations");
    }
}
```

### Entity Framework Configuration

```csharp
// src/NotificationSystem.Infrastructure/Persistence/Configurations/ProviderConfigurationConfiguration.cs
public class ProviderConfigurationConfiguration : IEntityTypeConfiguration<ProviderConfiguration>
{
    public void Configure(EntityTypeBuilder<ProviderConfiguration> builder)
    {
        builder.ToTable("provider_configurations");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.ChannelType)
            .IsRequired()
            .HasColumnName("channel_type");

        builder.Property(p => p.ProviderType)
            .IsRequired()
            .HasMaxLength(50)
            .HasColumnName("provider_type");

        builder.Property(p => p.ConfigurationJson)
            .IsRequired()
            .HasColumnType("jsonb")  // PostgreSQL JSONB para performance
            .HasColumnName("configuration_json");

        builder.Property(p => p.IsActive)
            .HasColumnName("is_active");

        builder.Property(p => p.IsPrimary)
            .HasColumnName("is_primary");

        builder.Property(p => p.CreatedAt)
            .HasColumnName("created_at");

        builder.Property(p => p.UpdatedAt)
            .HasColumnName("updated_at");

        builder.Property(p => p.CreatedBy)
            .HasMaxLength(100)
            .HasColumnName("created_by");

        // Índices
        builder.HasIndex(p => new { p.ChannelType, p.IsPrimary })
            .IsUnique()
            .HasFilter("is_primary = true");

        builder.HasIndex(p => p.ChannelType);
    }
}
```

---

## 📱 6. Exemplos de Uso

### Cadastrar Twilio

```bash
curl -X POST http://localhost:5000/api/admin/providers \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer {token}" \
  -d '{
    "channelType": 1,
    "providerType": "Twilio",
    "configuration": {
      "accountSid": "ACxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx",
      "authToken": "your-auth-token",
      "fromPhoneNumber": "+15551234567"
    },
    "isActive": true,
    "isPrimary": true
  }'
```

**Resposta:**
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "channelType": 1,
  "providerType": "Twilio",
  "configurationJson": "{\"accountSid\":\"ACxxxx\",\"authToken\":\"xxx\",\"fromPhoneNumber\":\"+15551234567\"}",
  "isActive": true,
  "isPrimary": true,
  "createdAt": "2025-12-30T10:00:00Z",
  "createdBy": "admin@example.com"
}
```

### Cadastrar AWS SNS como Alternativa

```bash
curl -X POST http://localhost:5000/api/admin/providers \
  -H "Content-Type: application/json" \
  -d '{
    "channelType": 1,
    "providerType": "AwsSns",
    "configuration": {
      "accessKeyId": "AKIAXXXXXXXXXXXXXXXX",
      "secretAccessKey": "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx",
      "region": "us-east-1",
      "senderId": "MyApp"
    },
    "isActive": false,
    "isPrimary": false
  }'
```

### Listar Provedores de SMS

```bash
curl http://localhost:5000/api/admin/providers?channelType=1
```

**Resposta:**
```json
[
  {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "channelType": 1,
    "providerType": "Twilio",
    "isActive": true,
    "isPrimary": true,
    "createdAt": "2025-12-30T10:00:00Z"
  },
  {
    "id": "660e8400-e29b-41d4-a716-446655440001",
    "channelType": 1,
    "providerType": "AwsSns",
    "isActive": false,
    "isPrimary": false,
    "createdAt": "2025-12-30T11:00:00Z"
  }
]
```

### Trocar de Twilio para AWS SNS

```bash
# Ativa AWS SNS e define como primário (desativa Twilio automaticamente)
curl -X POST http://localhost:5000/api/admin/providers/660e8400-e29b-41d4-a716-446655440001/set-primary
```

**Resultado:** Próximas notificações SMS usarão AWS SNS em vez de Twilio, **sem restart**.

### Obter Provedor Ativo

```bash
curl http://localhost:5000/api/admin/providers/sms/active
```

**Resposta:**
```json
{
  "id": "660e8400-e29b-41d4-a716-446655440001",
  "channelType": 1,
  "providerType": "AwsSns",
  "isActive": true,
  "isPrimary": true
}
```

---

## 🔒 7. Segurança

### Criptografia de Configurações

**IMPORTANTE:** As credenciais no `ConfigurationJson` devem ser criptografadas.

```csharp
// src/NotificationSystem.Infrastructure/Security/ConfigurationEncryption.cs
public class ConfigurationEncryption
{
    private readonly IDataProtector _protector;

    public ConfigurationEncryption(IDataProtectionProvider provider)
    {
        _protector = provider.CreateProtector("ProviderConfiguration");
    }

    public string Encrypt(string plainText)
    {
        return _protector.Protect(plainText);
    }

    public string Decrypt(string cipherText)
    {
        return _protector.Unprotect(cipherText);
    }
}

// Uso no Repository
public async Task<ProviderConfiguration> CreateAsync(ProviderConfiguration config)
{
    // Criptografar antes de salvar
    config.ConfigurationJson = _encryption.Encrypt(config.ConfigurationJson);

    await _context.ProviderConfigurations.AddAsync(config);
    await _context.SaveChangesAsync();

    return config;
}

// Uso no Factory
public async Task<ISmsService> CreateProviderAsync()
{
    var config = await _providerRepo.GetActiveProviderAsync(ChannelType.Sms);

    // Descriptografar antes de usar
    var decryptedJson = _encryption.Decrypt(config.ConfigurationJson);
    var settings = JsonSerializer.Deserialize<TwilioSettings>(decryptedJson);

    return new TwilioService(Options.Create(settings), _logger);
}
```

### Autorização

```csharp
// Apenas administradores podem gerenciar provedores
var group = app.MapGroup("/api/admin/providers")
    .WithTags("Provider Configuration")
    .RequireAuthorization("AdminOnly");  // Policy que verifica role Admin
```

### Auditoria

```csharp
// Registrar quem criou/modificou configurações
config.CreatedBy = httpContext.User.Identity?.Name ?? "unknown";
config.UpdatedBy = httpContext.User.Identity?.Name ?? "unknown";
config.UpdatedAt = DateTime.UtcNow;

_logger.LogWarning(
    "Provider configuration changed. Channel: {Channel}, Provider: {Provider}, ChangedBy: {User}",
    config.ChannelType, config.ProviderType, config.UpdatedBy);
```

---

## 🚀 8. Melhorias Futuras

### Fallback Automático

```csharp
public class ResilientSmsProviderFactory : ISmsProviderFactory
{
    public async Task<ISmsService> CreateProviderAsync()
    {
        // Tenta provedor primário
        var primary = await _repo.GetActiveProviderAsync(ChannelType.Sms);

        try
        {
            var service = CreateProvider(primary);
            await TestConnection(service);  // Health check
            return service;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Primary provider failed, trying fallback");

            // Tenta fallback
            var fallback = await _repo.GetFallbackProviderAsync(ChannelType.Sms);
            if (fallback != null)
            {
                return CreateProvider(fallback);
            }

            throw;
        }
    }
}
```

### Health Check Endpoint

```csharp
group.MapPost("/{id}/test", async (
    Guid id,
    IProviderConfigurationRepository repo,
    ISmsProviderFactory factory) =>
{
    try
    {
        var provider = await repo.GetByIdAsync(id);
        var service = CreateProviderFromConfig(provider);

        // Tentar enviar SMS de teste
        await service.SendSmsAsync("+15551234567", "Test message");

        return Results.Ok(new { status = "healthy", message = "Provider is working" });
    }
    catch (Exception ex)
    {
        return Results.Ok(new { status = "unhealthy", error = ex.Message });
    }
});
```

### Rate Limiting por Provider

```csharp
public class ProviderConfiguration
{
    // ... propriedades existentes

    public int? MaxRequestsPerMinute { get; set; }
    public int? MaxRequestsPerDay { get; set; }
}
```

### Webhooks para Mudanças

```csharp
// Notificar sistemas externos quando provedor mudar
public class ProviderChangedEvent
{
    public ChannelType ChannelType { get; set; }
    public string OldProvider { get; set; }
    public string NewProvider { get; set; }
    public DateTime ChangedAt { get; set; }
}

// Publicar evento quando mudar provedor primário
await _eventPublisher.PublishAsync(new ProviderChangedEvent
{
    ChannelType = ChannelType.Sms,
    OldProvider = "Twilio",
    NewProvider = "AwsSns",
    ChangedAt = DateTime.UtcNow
});
```

---

## 📊 9. Comparação

### Antes (Configuração Estática)

**Vantagens:**
- ✅ Simples de configurar
- ✅ Não requer banco de dados adicional

**Desvantagens:**
- ❌ Precisa restart para mudar provedor
- ❌ Não suporta múltiplos provedores
- ❌ Credenciais em arquivos de configuração
- ❌ Sem fallback automático
- ❌ Sem interface de gerenciamento

### Depois (Configuração Dinâmica)

**Vantagens:**
- ✅ Troca de provedor sem restart
- ✅ Múltiplos provedores cadastrados
- ✅ Credenciais centralizadas e criptografadas
- ✅ Fallback automático possível
- ✅ Interface administrativa
- ✅ Auditoria de mudanças
- ✅ Health checks por provedor

**Desvantagens:**
- ❌ Maior complexidade inicial
- ❌ Requer tabela adicional no banco
- ❌ Factory pattern adiciona overhead pequeno

---

## 🎯 10. Roadmap de Implementação

### Fase 1: Fundação
- [ ] Criar entidade `ProviderConfiguration`
- [ ] Criar migration
- [ ] Implementar repository
- [ ] Criar interfaces de factory

### Fase 2: Provedores SMS
- [ ] Implementar `SmsProviderFactory`
- [ ] Adicionar suporte AWS SNS
- [ ] Adicionar suporte Nexmo
- [ ] Atualizar Consumer.Sms para usar factory

### Fase 3: API Admin
- [ ] Criar endpoints de administração
- [ ] Adicionar autenticação/autorização
- [ ] Implementar criptografia de configurações
- [ ] Adicionar validação de configurações

### Fase 4: Provedores Push
- [ ] Implementar `PushProviderFactory`
- [ ] Adicionar suporte OneSignal
- [ ] Adicionar suporte AWS SNS Push
- [ ] Atualizar Consumer.Push

### Fase 5: Provedores Email
- [ ] Implementar `EmailProviderFactory`
- [ ] Adicionar suporte SendGrid
- [ ] Adicionar suporte AWS SES
- [ ] Atualizar Consumer.Email

### Fase 6: Melhorias
- [ ] Fallback automático
- [ ] Health checks
- [ ] Rate limiting por provider
- [ ] Webhooks para mudanças
- [ ] UI administrativa (frontend)

---

## 📦 11. Pacotes NuGet Necessários

```xml
<!-- AWS SNS Support -->
<PackageReference Include="AWSSDK.SimpleNotificationService" Version="3.7.x" />
<PackageReference Include="AWSSDK.Core" Version="3.7.x" />

<!-- Criptografia -->
<PackageReference Include="Microsoft.AspNetCore.DataProtection" Version="8.0.x" />

<!-- Outros provedores (quando implementar) -->
<PackageReference Include="Vonage" Version="7.x.x" />
<PackageReference Include="OneSignalApi" Version="1.x.x" />
<PackageReference Include="SendGrid" Version="9.x.x" />
```

---

## ✅ Conclusão

Esta feature permitirá que o sistema de notificações seja muito mais flexível e profissional, permitindo gerenciamento dinâmico de provedores através de uma interface administrativa, sem necessidade de restarts ou edição manual de arquivos de configuração.

A implementação seguirá os mesmos padrões de Clean Architecture e DDD já utilizados no projeto, mantendo a consistência e qualidade do código.

### Quando Implementar?

Esta feature é ideal para:
- ✅ Ambientes multi-tenant onde cada cliente quer seu provedor
- ✅ Quando há necessidade de failover automático
- ✅ Quando credenciais mudam frequentemente
- ✅ Quando há requisito de interface administrativa

### Complexidade Estimada

- **Backend:** Médio (3-5 dias)
- **Testes:** Médio (2-3 dias)
- **Frontend Admin (futuro):** Alto (5-7 dias)

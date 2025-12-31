namespace NotificationSystem.Api.Endpoints;

public static class NotificationEndpointsDocumentation
{
    public const string GetAllNotificationsDescription = @"
Retorna uma lista paginada de todas as notificações do sistema.

**Arquitetura Multi-Canal:**
Cada notificação pode ter um ou mais canais de entrega (Email, SMS, Push).
Os canais são retornados de forma polimórfica, onde cada tipo possui seus campos específicos.

**Estrutura da Notificação:**
- **id**: ID único da notificação
- **userId**: ID do usuário que receberá a notificação
- **createdAt**: Data/hora de criação
- **channels**: Lista de canais de entrega (pode conter múltiplos canais)

**Tipos de Canal:**
- **Email**: to, subject, body, isBodyHtml
- **SMS**: to, message, senderId
- **Push**: to, content (title, body, clickAction), data, platform, priority, timeToLive, condition, mutableContent, contentAvailable, android, apns, webpush, isRead

**Campos Comuns dos Canais:**
- id, status, errorMessage, sentAt

**Exemplos de Uso:**
- Notificação apenas por Email: 1 canal Email
- Lembrete de consulta: 2 canais (Email + SMS)
- Alerta de segurança: 3 canais (Email + SMS + Push)
";

    public const string CreateNotificationDescription = @"
Cria uma notificação que pode ser enviada através de múltiplos canais simultaneamente.

**Arquitetura Multi-Canal:**
Uma única notificação pode ter um ou mais canais de entrega (Email, SMS, Push).
Cada canal é processado de forma independente e assíncrona através de consumers RabbitMQ dedicados.

**Estrutura da Request:**
- **userId**: ID do usuário que receberá a notificação (GUID obrigatório)
- **channels**: Array com um ou mais canais de entrega

**Tipos de Canal Disponíveis:**

**1. Email** (type: ""Email"")
- **to**: Endereço de email do destinatário (obrigatório, max 500 chars)
- **subject**: Assunto do email (obrigatório, max 500 chars)
- **body**: Corpo do email em texto ou HTML (obrigatório)
- **isBodyHtml**: Se true, o body será renderizado como HTML (opcional, default: false)

**2. SMS** (type: ""Sms"")
- **to**: Número de telefone com código do país (obrigatório, max 500 chars, ex: +5511999999999)
- **message**: Texto da mensagem (obrigatório, max 1600 chars)
- **senderId**: Identificador do remetente (opcional, max 50 chars)

**3. Push** (type: ""Push"")
- **to**: Token do dispositivo FCM (obrigatório, max 500 chars)
- **title**: Título da notificação (obrigatório, max 100 chars)
- **body**: Texto da notificação (obrigatório, max 500 chars)
- **clickAction**: Ação ao clicar (opcional, max 200 chars)
- **data**: Dados customizados key-value (opcional, armazenado como JSONB)
- **platform**: Plataforma alvo (""android"", ""ios"", ""web"") (opcional)
- **priority**: Prioridade geral (opcional: ""high"", ""normal"")
- **timeToLive**: Tempo de vida em segundos (opcional)
- **condition**: Condição para envio (opcional, ex: ""TopicA && TopicB"")
- **mutableContent**: Permite modificação de conteúdo no iOS (opcional)
- **contentAvailable**: Notificação silenciosa no iOS (opcional)
- **android**: Configurações específicas do Android/FCM (opcional)
  - **priority**: Prioridade da mensagem (""high"", ""normal"")
  - **ttl**: Time to live (ex: ""3600s"")
- **apns**: Configurações específicas do iOS/APNs (opcional)
  - **headers**: Headers customizados do APNs (ex: apns-priority, apns-expiration)
- **webpush**: Configurações para Web Push (opcional)
  - **headers**: Headers customizados do Web Push

**Exemplos de Uso:**

**Email simples:**
```json
{
  ""userId"": ""123e4567-e89b-12d3-a456-426614174000"",
  ""channels"": [
    {
      ""type"": ""Email"",
      ""data"": {
        ""to"": ""usuario@example.com"",
        ""subject"": ""Bem-vindo!"",
        ""body"": ""<h1>Seja bem-vindo!</h1>"",
        ""isBodyHtml"": true
      }
    }
  ]
}
```

**Multi-canal (Email + SMS + Push):**
```json
{
  ""userId"": ""123e4567-e89b-12d3-a456-426614174000"",
  ""channels"": [
    {
      ""type"": ""Email"",
      ""data"": {
        ""to"": ""usuario@example.com"",
        ""subject"": ""Alerta de Segurança"",
        ""body"": ""Login detectado em novo dispositivo.""
      }
    },
    {
      ""type"": ""Sms"",
      ""data"": {
        ""to"": ""+5511999999999"",
        ""message"": ""Alerta: Login em novo dispositivo.""
      }
    },
    {
      ""type"": ""Push"",
      ""data"": {
        ""to"": ""device-token-xyz"",
        ""title"": ""Alerta de Segurança"",
        ""body"": ""Login detectado"",
        ""platform"": ""android"",
        ""priority"": ""high""
      }
    }
  ]
}
```

**Push com configurações específicas de plataforma:**
```json
{
  ""userId"": ""123e4567-e89b-12d3-a456-426614174000"",
  ""channels"": [
    {
      ""type"": ""Push"",
      ""data"": {
        ""to"": ""device-token-fcm"",
        ""title"": ""Nova mensagem"",
        ""body"": ""Você tem uma nova mensagem!"",
        ""clickAction"": ""OPEN_CHAT"",
        ""platform"": ""android"",
        ""data"": {
          ""chatId"": ""12345"",
          ""senderId"": ""67890""
        },
        ""android"": {
          ""priority"": ""high"",
          ""ttl"": ""3600s""
        },
        ""priority"": ""high"",
        ""timeToLive"": 3600
      }
    }
  ]
}
```

**Push para iOS com APNs:**
```json
{
  ""userId"": ""123e4567-e89b-12d3-a456-426614174000"",
  ""channels"": [
    {
      ""type"": ""Push"",
      ""data"": {
        ""to"": ""device-token-fcm"",
        ""title"": ""Atualização disponível"",
        ""body"": ""Uma nova versão está disponível!"",
        ""platform"": ""ios"",
        ""mutableContent"": true,
        ""contentAvailable"": false,
        ""apns"": {
          ""headers"": {
            ""apns-priority"": ""10"",
            ""apns-push-type"": ""alert""
          }
        },
        ""priority"": ""high""
      }
    }
  ]
}
```

**Fluxo de Processamento:**
1. A notificação é criada no banco de dados
2. Um evento de domínio é disparado (NotificationCreatedEvent)
3. Para cada canal, uma mensagem é publicada na fila RabbitMQ correspondente
4. Consumers dedicados processam cada canal de forma independente
5. O status de cada canal é atualizado após o envio (Sent/Failed)
";
}

# Sistema de Notificações - .NET

## 📚 Sobre o Projeto

Sistema de notificações assíncrono desenvolvido em **.NET** com **ASP.NET Core** e **RabbitMQ**. Este projeto implementa um sistema production-ready de envio de notificações por múltiplos canais utilizando arquitetura de mensageria.

### 🎯 Objetivo

Criar um sistema escalável e resiliente para envio de notificações através de:
- 📧 **Email** - Via SMTP
- 📱 **SMS** - Via Twilio
- 🔔 **Push Notifications** - Via Firebase Cloud Messaging
- 🔗 **Webhooks** - HTTP callbacks

## 🏗️ Arquitetura

```
Cliente → API (ASP.NET Core) → RabbitMQ → Consumers → Serviços Externos
                                   ↓
                              PostgreSQL
```

### Componentes

- **API (ASP.NET Core)**: Recebe requisições REST e publica mensagens no RabbitMQ
- **Consumers (Workers)**: Processos independentes que consomem mensagens e enviam notificações
- **RabbitMQ**: Message broker para garantir processamento assíncrono e confiável
- **PostgreSQL**: Persistência do histórico de notificações (opcional)

## 📁 Estrutura do Projeto

```
API-notifications/
├── src/
│   ├── NotificationSystem.Api/              # API REST (ASP.NET Core)
│   │   ├── Controllers/                     # Endpoints da API
│   │   ├── Middleware/                      # Auth, RateLimit, Logging
│   │   ├── Services/                        # RabbitMQ Producer
│   │   └── appsettings.json                 # Configurações da API
│   │
│   ├── NotificationSystem.Shared/           # Biblioteca compartilhada
│   │   ├── Models/                          # DTOs e modelos
│   │   ├── Configuration/                   # Classes de configuração
│   │   ├── RabbitMQ/                        # Cliente RabbitMQ base
│   │   └── Interfaces/                      # Contratos
│   │
│   └── Consumers/                           # Workers (Consumers)
│       ├── NotificationSystem.Consumer.Email/
│       ├── NotificationSystem.Consumer.Sms/
│       ├── NotificationSystem.Consumer.Push/
│       └── NotificationSystem.Consumer.Webhook/
│
├── NotificationSystem.sln                   # Solution file
├── appsettings.Example.json                 # Template de configuração
├── docker-compose.yml                       # Orquestração local
└── README.md
```

## 🚀 Começando

### Pré-requisitos

- **.NET SDK 8.0+** ([Download](https://dotnet.microsoft.com/download))
- **Docker** e **Docker Compose** (para RabbitMQ e PostgreSQL)
- **Visual Studio 2022**, **VS Code** ou **Rider**

### Instalação

1. **Clone o repositório**
```bash
git clone https://github.com/YuriGarciaRibeiro/API-notifications.git
cd API-notifications
```

2. **Restaurar dependências**
```bash
dotnet restore
```

3. **Configure as variáveis de ambiente**
```bash
cp appsettings.Example.json src/NotificationSystem.Api/appsettings.Development.json
# Edite appsettings.Development.json com suas credenciais
```

4. **Iniciar RabbitMQ com Docker**
```bash
docker run -d --name rabbitmq \\
  -p 5672:5672 \\
  -p 15672:15672 \\
  -e RABBITMQ_DEFAULT_USER=guest \\
  -e RABBITMQ_DEFAULT_PASS=guest \\
  rabbitmq:3-management
```

5. **Iniciar PostgreSQL com Docker** (opcional)
```bash
docker run -d --name postgres \\
  -p 5432:5432 \\
  -e POSTGRES_PASSWORD=postgres \\
  -e POSTGRES_DB=notifications \\
  postgres:15
```

6. **Executar a API**
```bash
dotnet run --project src/NotificationSystem.Api
```

7. **Executar os Consumers** (em terminais separados)
```bash
dotnet run --project src/Consumers/NotificationSystem.Consumer.Email
dotnet run --project src/Consumers/NotificationSystem.Consumer.Sms
dotnet run --project src/Consumers/NotificationSystem.Consumer.Push
dotnet run --project src/Consumers/NotificationSystem.Consumer.Webhook
```

## 📖 Uso da API

### Endpoints

#### Health Check
```bash
GET /health
```

#### Enviar Notificação por Email
```bash
POST /api/notifications/email
Content-Type: application/json
X-API-Key: your-api-key

{
  "to": "user@example.com",
  "subject": "Teste",
  "body": "Mensagem de teste",
  "priority": "normal"
}
```

#### Enviar SMS
```bash
POST /api/notifications/sms
Content-Type: application/json
X-API-Key: your-api-key

{
  "to": "+5511999999999",
  "message": "Sua mensagem aqui",
  "priority": "high"
}
```

#### Enviar Push Notification
```bash
POST /api/notifications/push
Content-Type: application/json
X-API-Key: your-api-key

{
  "deviceToken": "fcm-device-token",
  "title": "Título",
  "body": "Corpo da notificação",
  "data": {
    "action": "open_app"
  }
}
```

#### Webhook
```bash
POST /api/notifications/webhook
Content-Type: application/json
X-API-Key: your-api-key

{
  "url": "https://seu-webhook.com/endpoint",
  "method": "POST",
  "payload": {
    "event": "user.created",
    "data": { ... }
  }
}
```

## 🔧 Configuração

Edite o arquivo `appsettings.json` ou `appsettings.Development.json`:

```json
{
  "RabbitMQ": {
    "HostName": "localhost",
    "UserName": "guest",
    "Password": "guest"
  },
  "Services": {
    "Email": {
      "Smtp": {
        "Host": "smtp.gmail.com",
        "Port": 587,
        "UserName": "your-email@gmail.com",
        "Password": "your-app-password"
      }
    }
  }
}
```

## 🎓 Tecnologias Utilizadas

### Stack Principal
- **.NET 8** - Framework principal
- **ASP.NET Core** - Web API
- **RabbitMQ** - Message broker
- **PostgreSQL** - Banco de dados (opcional)
- **Docker** - Containerização

### Bibliotecas NuGet Principais
- **RabbitMQ.Client** - Cliente oficial RabbitMQ
- **MailKit** - Envio de emails via SMTP
- **Twilio** - SDK para envio de SMS
- **FirebaseAdmin** - Firebase Cloud Messaging
- **Entity Framework Core** - ORM (se usar banco de dados)
- **Serilog** - Logging estruturado
- **FluentValidation** - Validação de dados
- **Swashbuckle** - Documentação Swagger/OpenAPI

## 🧪 Testes

```bash
# Executar todos os testes
dotnet test

# Com cobertura
dotnet test /p:CollectCoverage=true
```

## 🐳 Docker

### Build das imagens
```bash
docker build -t notification-api -f src/NotificationSystem.Api/Dockerfile .
docker build -t notification-consumer-email -f src/Consumers/NotificationSystem.Consumer.Email/Dockerfile .
```

### Docker Compose (ambiente completo)
```bash
docker-compose up -d
```

## 📊 Monitoramento

- **RabbitMQ Management**: http://localhost:15672 (guest/guest)
- **API Health Check**: http://localhost:5000/health
- **Swagger UI**: http://localhost:5000/swagger

## 🔒 Segurança

- **API Key Authentication**: Protege endpoints da API
- **Rate Limiting**: Previne abuso
- **Input Validation**: Valida todos os inputs com FluentValidation
- **CORS**: Configurável por ambiente
- **Secrets**: Usar User Secrets ou Azure Key Vault em produção

```bash
# Configurar User Secrets localmente
dotnet user-secrets init --project src/NotificationSystem.Api
dotnet user-secrets set "Services:Email:Smtp:Password" "your-password"
```

## 📝 TODO / Roadmap

### Fase 1: MVP ✅
- [x] Estrutura da solução .NET
- [x] Projetos criados (API + Consumers)
- [ ] Implementar models no Shared
- [ ] Implementar RabbitMQ Producer na API
- [ ] Implementar RabbitMQ Consumers
- [ ] Health checks básicos

### Fase 2: Integrações
- [ ] Integração SMTP (Email)
- [ ] Integração Twilio (SMS)
- [ ] Integração Firebase (Push)
- [ ] Cliente HTTP para Webhooks
- [ ] Retry logic e DLQ

### Fase 3: Production-Ready
- [ ] Logging estruturado (Serilog)
- [ ] Métricas e observabilidade
- [ ] Testes unitários e de integração
- [ ] CI/CD pipeline
- [ ] Docker images otimizadas
- [ ] Documentação completa

## 🤝 Contribuindo

1. Fork o projeto
2. Crie uma branch para sua feature (`git checkout -b feature/MinhaFeature`)
3. Commit suas mudanças (`git commit -m 'Adiciona MinhaFeature'`)
4. Push para a branch (`git push origin feature/MinhaFeature`)
5. Abra um Pull Request

## 📄 Licença

Este projeto está sob a licença MIT. Veja o arquivo [LICENSE](LICENSE) para mais detalhes.

## 📧 Contato

Yuri Garcia Ribeiro - [@YuriGarciaRibeiro](https://github.com/YuriGarciaRibeiro)

Link do Projeto: [https://github.com/YuriGarciaRibeiro/API-notifications](https://github.com/YuriGarciaRibeiro/API-notifications)

---

**Nota**: Este é um projeto de estudo focado em boas práticas de desenvolvimento .NET e arquitetura de microserviços.

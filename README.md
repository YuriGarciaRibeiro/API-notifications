# Sistema de Notificações - .NET

## 📚 Sobre o Projeto

**Sistema centralizador de notificações self-hosted** desenvolvido em **.NET** com **ASP.NET Core** e **RabbitMQ**. Solução production-ready para gerenciamento e envio de notificações por múltiplos canais simultaneamente, projetada para ser hospedada internamente por qualquer empresa via Docker.

### 🎯 Objetivo

Prover uma **solução self-hosted de notificações** que permite:
- 📧 **Email** - Envio via SMTP (qualquer provedor)
- 📱 **SMS** - Integração com Twilio
- 🔔 **Push Notifications** - Via Firebase Cloud Messaging
- 🎯 **Multi-canal** - Uma notificação com Email + SMS + Push simultaneamente
- 📊 **Rastreamento** - Status independente por canal de entrega
- 🏢 **Self-Hosted** - Deploy via Docker com infraestrutura própria da empresa

### 🌟 Características

- **Self-Hosted**: Deploy via Docker, sem dependências externas obrigatórias
- **Multi-Channel Architecture**: Envie para múltiplos canais em uma única requisição
- **Independent Status Tracking**: Cada canal tem status próprio (Email ✅ / SMS ❌)
- **Scalable Consumers**: Escale processadores de Email, SMS e Push independentemente
- **Event-Driven**: Processamento assíncrono com RabbitMQ + Dead Letter Queue
- **Production-Ready**: Retry logic, error handling, health checks
- **Type-safe API**: DTOs polimórficos + OpenAPI/Swagger
- **CQRS Pattern**: Separação clara entre comandos e queries usando MediatR
- **Clean Architecture**: Manutenível, testável e extensível

## 🏗️ Arquitetura

Este projeto segue os princípios de **Clean Architecture** (Arquitetura Limpa) e **Domain-Driven Design (DDD)**, organizando o código em camadas bem definidas:

```
┌─────────────────────────────────────────────────────────┐
│                    Presentation Layer                    │
│              (API + Consumers/Workers)                   │
├─────────────────────────────────────────────────────────┤
│                  Infrastructure Layer                    │
│     (RabbitMQ, SMTP, Twilio, Firebase, PostgreSQL)     │
├─────────────────────────────────────────────────────────┤
│                   Application Layer                      │
│    (Use Cases, DTOs, Validators, MediatR Handlers)     │
├─────────────────────────────────────────────────────────┤
│                     Domain Layer                         │
│        (Entities, Value Objects, Interfaces)            │
└─────────────────────────────────────────────────────────┘
```

### Camadas

#### 🎯 Domain (Core)
- **Responsabilidade**: Lógica de negócio central, independente de frameworks
- **Contém**: Entities (Notification, NotificationChannel, EmailChannel, SmsChannel, PushChannel), Value Objects, Enums, Domain Events
- **Dependências**: Nenhuma (núcleo da aplicação)
- **Padrão**: Channel-based architecture (um Notification pode ter múltiplos Channels)

#### 💼 Application
- **Responsabilidade**: Casos de uso e orquestração da lógica de negócio
- **Contém**: MediatR Handlers, Queries/Commands, DTOs polimórficos, FluentValidation Validators, Mappings
- **Dependências**: Domain
- **Padrões**: CQRS, Mediator, Pipeline Behavior

#### 🔧 Infrastructure
- **Responsabilidade**: Implementações técnicas e integrações externas
- **Contém**: RabbitMQ, SMTP, Twilio, Firebase, Repositórios, EF Core, DbContext
- **Dependências**: Application, Domain

#### 🌐 Presentation (API + Consumers)
- **Responsabilidade**: Entrada/saída da aplicação
- **Contém**: Minimal API Endpoints, Middleware (Exception Handler), Workers/Consumers
- **Dependências**: Application, Infrastructure

### Fluxo de Dados

```
Frontend/Client → API Endpoint → MediatR → Handler → Repository
                       ↓
                   Validator (FluentValidation)
                       ↓
                   RabbitMQ Producer
                       ↓
                   Message Queue
                       ↓
              Consumers/Workers → External Services (SMTP/Twilio/Firebase)
```

### Vantagens desta Arquitetura

✅ **Testabilidade**: Domain e Application podem ser testados sem dependências externas
✅ **Manutenibilidade**: Mudanças em frameworks não afetam a lógica de negócio
✅ **Escalabilidade**: Componentes desacoplados facilitam escalonamento horizontal
✅ **Flexibilidade**: Fácil substituir implementações (ex: trocar RabbitMQ por Kafka)
✅ **Clareza**: Estrutura organizada facilita onboarding de novos desenvolvedores
✅ **Type Safety**: DTOs polimórficos garantem contratos bem definidos

## 📁 Estrutura do Projeto

```
API-notifications/
├── src/
│   ├── NotificationSystem.Domain/           # 🎯 Camada de Domínio
│   │   ├── Entities/
│   │   │   ├── Notification.cs              # Aggregate root
│   │   │   ├── NotificationChannel.cs       # Base abstrata
│   │   │   ├── EmailChannel.cs              # Herança polimórfica
│   │   │   ├── SmsChannel.cs
│   │   │   └── PushChannel.cs
│   │   └── Events/
│   │       └── NotificationCreatedEvent.cs
│   │
│   ├── NotificationSystem.Application/      # 💼 Camada de Aplicação
│   │   ├── UseCases/
│   │   │   └── GetAllNotifications/
│   │   │       ├── GetAllNotificationsQuery.cs
│   │   │       ├── GetAllNotificationsHandler.cs
│   │   │       ├── GetAllNotificationsResponse.cs  # DTOs polimórficos
│   │   │       └── GetAllNotificationsValidator.cs
│   │   ├── Common/
│   │   │   ├── Behaviors/
│   │   │   │   └── ValidationBehavior.cs    # MediatR pipeline
│   │   │   └── Mappings/
│   │   │       └── NotificationMappings.cs
│   │   └── DependencyInjection.cs
│   │
│   ├── NotificationSystem.Infrastructure/   # 🔧 Camada de Infraestrutura
│   │   ├── Messaging/
│   │   │   └── RabbitMQ/
│   │   ├── Services/
│   │   │   ├── Email/
│   │   │   ├── Sms/
│   │   │   └── Push/
│   │   └── Persistence/
│   │       ├── Repositories/
│   │       └── Configurations/
│   │
│   ├── NotificationSystem.Api/              # 🌐 API REST (Presentation)
│   │   ├── Endpoints/
│   │   │   └── NotificationEndpoints.cs     # Minimal API
│   │   ├── Middlewares/
│   │   │   └── GlobalExceptionHandlerMiddleware.cs
│   │   ├── Extensions/
│   │   │   ├── ProblemDetailsExtensions.cs
│   │   │   └── ResultExtensions.cs
│   │   ├── Program.cs
│   │   └── appsettings.json
│   │
│   └── Consumers/                           # 🌐 Workers (Presentation)
│       ├── NotificationSystem.Consumer.Email/
│       ├── NotificationSystem.Consumer.Sms/
│       ├── NotificationSystem.Consumer.Push/
│       └── NotificationSystem.Consumer.Bulk/
│
├── documentos/                              # 📖 Documentação técnica e revisões
│   ├── techspec-codebase.md
│   └── review-status-projeto/
│
├── scripts/                                 # 🛠️ Scripts utilitários (build/migrations)
│   └── database/
│
├── NotificationSystem.slnx
└── README.md
```

### Dependências entre Projetos

```
┌─────────────────────────────────────────────────────────┐
│  API + Consumers  →  Infrastructure + Application       │
│  Infrastructure   →  Application + Domain               │
│  Application      →  Domain                             │
│  Domain           →  (sem dependências)                 │
└─────────────────────────────────────────────────────────┘
```

## 🚀 Deployment (Para Empresas)

### Self-Hosted via Docker

Este sistema é distribuído como **imagens Docker** prontas para uso. Cada empresa hospeda sua própria instância com infraestrutura independente.

**📘 Documentação disponível no repositório:**
- [Tech Spec da codebase](documentos/techspec-codebase.md) - visão técnica consolidada
- [Scripts de suporte](scripts/README.md) - comandos auxiliares de build e banco

**🗺️ Planejado (ainda não versionado neste repositório):**
- Quick Start Guide
- Deployment Guide

### Infraestrutura Necessária

- **PostgreSQL** (local, AWS RDS, Azure Database, etc.)
- **RabbitMQ** (local, CloudAMQP, AWS MQ, etc.)
- **SMTP Server** (Gmail, SendGrid, AWS SES, Office365, etc.)
- **Docker & Docker Compose**

### Deploy Rápido

```bash
# 1. Configurar ambiente
cp .env.production.example .env
# Editar .env com suas credenciais

# 2. Executar migrations
docker run --rm \
  -e ConnectionStrings__DefaultConnection="$DATABASE_CONNECTION_STRING" \
  your-registry/notification-system-api:latest \
  dotnet ef database update

# 3. Iniciar serviços
docker-compose -f docker-compose.production.yml up -d
```

---

## 🛠️ Desenvolvimento Local

### Pré-requisitos

- **.NET SDK 10.0+** ([Download](https://dotnet.microsoft.com/download))
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
dotnet restore NotificationSystem.slnx
```

3. **Configure as variáveis de ambiente**
```bash
cp appsettings.Example.json src/NotificationSystem.Api/appsettings.Development.json
# Edite appsettings.Development.json com suas credenciais
```

4. **Iniciar dependências com Docker**
```bash
# Subir PostgreSQL e RabbitMQ
docker-compose up -d
```

5. **Aplicar Migrations no Banco de Dados**
```bash
# Usar o script pronto
./scripts/database/migrate.sh

# OU manualmente
dotnet ef database update --project src/NotificationSystem.Infrastructure --startup-project src/NotificationSystem.Api
```

6. **Executar a API**
```bash
dotnet run --project src/NotificationSystem.Api
```

A API estará disponível em:
- **HTTP**: http://localhost:5000
- **HTTPS**: https://localhost:5001
- **Swagger**: http://localhost:5000/swagger

## 📖 Documentação da API

Acesse a documentação interativa em: **http://localhost:5000/swagger**

### Endpoints Disponíveis

#### Listar Notificações (com paginação)

```bash
GET /api/notifications?pageNumber=1&pageSize=10
```

**Query Parameters:**
- `pageNumber` (int, default: 1): Número da página
- `pageSize` (int, default: 10, max: 100): Tamanho da página

**Resposta de Sucesso (200 OK):**

```json
{
  "notifications": [
    {
      "type": "Email",
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "userId": "550e8400-e29b-41d4-a716-446655440000",
      "createdAt": "2025-12-10T10:30:00Z",
      "status": "Sent",
      "errorMessage": null,
      "sentAt": "2025-12-10T10:31:00Z",
      "to": "user@example.com",
      "subject": "Welcome!",
      "body": "Welcome to our notification system",
      "isBodyHtml": false
    },
    {
      "type": "Sms",
      "id": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
      "userId": "550e8400-e29b-41d4-a716-446655440000",
      "createdAt": "2025-12-10T10:30:00Z",
      "status": "Pending",
      "errorMessage": null,
      "sentAt": null,
      "to": "+5511999999999",
      "message": "Your code is 123456",
      "senderId": "MyApp"
    },
    {
      "type": "Push",
      "id": "8f1e1f99-8765-40de-955c-e17fc2f91bf8",
      "userId": "550e8400-e29b-41d4-a716-446655440000",
      "createdAt": "2025-12-10T10:30:00Z",
      "status": "Sent",
      "errorMessage": null,
      "sentAt": "2025-12-10T10:31:00Z",
      "to": "device-token-123",
      "content": {
        "title": "New Message",
        "body": "You have a new message",
        "clickAction": "/messages"
      },
      "data": {},
      "priority": null,
      "timeToLive": null,
      "isRead": false
    }
  ],
  "totalCount": 3,
  "pageNumber": 1,
  "pageSize": 10
}
```

**Resposta de Erro de Validação (400 Bad Request):**

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

### Sistema de Canais Multi-Envio

Uma notificação pode ter **múltiplos canais** simultaneamente:

```json
{
  "userId": "550e8400-e29b-41d4-a716-446655440000",
  "channels": [
    {
      "type": "Email",
      "to": "user@example.com",
      "subject": "Welcome!",
      "body": "<h1>Welcome!</h1>",
      "isBodyHtml": true
    },
    {
      "type": "Sms",
      "to": "+5511999999999",
      "message": "Welcome to our platform!"
    },
    {
      "type": "Push",
      "to": "device-token-fcm",
      "content": {
        "title": "Welcome",
        "body": "Your account is ready!"
      }
    }
  ]
}
```

**Status Independente:** Se Email enviar com sucesso mas SMS falhar, cada canal terá seu próprio status.

> 📘 A documentação dedicada de canais (`CHANNEL_SYSTEM.md`) está planejada e ainda não foi versionada no repositório.

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
    },
    "Sms": {
      "Twilio": {
        "AccountSid": "your-account-sid",
        "AuthToken": "your-auth-token",
        "FromNumber": "+1234567890"
      }
    },
    "Push": {
      "Firebase": {
        "ProjectId": "your-project-id",
        "PrivateKey": "your-private-key"
      }
    }
  }
}
```

## 🎓 Tecnologias Utilizadas

### Stack Principal
- **.NET 10** - Framework principal
- **ASP.NET Core** - Web API (Minimal APIs)
- **RabbitMQ** - Message broker para processamento assíncrono
- **PostgreSQL** - Banco de dados relacional
- **Docker** - Containerização

### Bibliotecas NuGet Principais

#### Application Layer
- **MediatR (14.0.0)** - CQRS e Mediator pattern
- **FluentValidation (12.1.1)** - Validação declarativa com pipeline behavior
- **FluentResults (4.0.0)** - Result pattern para tratamento de erros
- **FluentValidation.DependencyInjectionExtensions (12.1.1)** - Integração com DI

#### Infrastructure Layer
- **RabbitMQ.Client (7.2.0)** - Cliente oficial RabbitMQ com DLX/DLQ
- **MailKit** - Envio de emails via SMTP
- **Twilio (7.14.0)** - SDK oficial para envio de SMS (production-ready)
- **FirebaseAdmin** - Firebase Cloud Messaging
- **Entity Framework Core** - ORM para persistência
- **Npgsql.EntityFrameworkCore.PostgreSQL** - Provider PostgreSQL

#### Presentation Layer
- **Microsoft.AspNetCore.OpenApi** - Documentação OpenAPI/Swagger

#### Cross-cutting
- **Serilog** - Logging estruturado (planejado)

## 🗄️ Gerenciamento de Migrations

O projeto inclui scripts prontos para gerenciar migrations do Entity Framework Core.

### Scripts Disponíveis

```bash
# Aplicar todas as migrations pendentes
./scripts/database/migrate.sh

# Criar uma nova migration
./scripts/database/add-migration.sh NomeDaMigration

# Listar migrations (aplicadas e pendentes)
./scripts/database/list-migrations.sh

# Reverter última migration
./scripts/database/rollback-migration.sh

# Resetar banco de dados completamente (⚠️ apaga todos os dados!)
./scripts/database/reset-database.sh
```

### Exemplos Práticos

```bash
# Após modificar uma entidade
./scripts/database/add-migration.sh AddUserEmailColumn
./scripts/database/migrate.sh

# Ver status das migrations
./scripts/database/list-migrations.sh

# Corrigir uma migration com erro
./scripts/database/rollback-migration.sh
# (corrigir o código)
./scripts/database/add-migration.sh FixedMigration
./scripts/database/migrate.sh
```

📖 **Documentação completa:** [scripts/README.md](scripts/README.md)

## 🧪 Testes

```bash
# Executar todos os testes
dotnet test

# Com cobertura
dotnet test /p:CollectCoverage=true

# Testes específicos
dotnet test --filter "FullyQualifiedName~NotificationSystem.Application.Tests"
```

## 🐳 Docker

### Build das Imagens (Para Desenvolvedores)

```bash
# Build todas as imagens de uma vez
./scripts/build-and-push.sh 1.0.0

# Ou manualmente
docker build -t notification-system-api -f src/NotificationSystem.Api/Dockerfile .
docker build -t notification-system-consumer-email -f src/Consumers/NotificationSystem.Consumer.Email/Dockerfile .
docker build -t notification-system-consumer-sms -f src/Consumers/NotificationSystem.Consumer.Sms/Dockerfile .
docker build -t notification-system-consumer-push -f src/Consumers/NotificationSystem.Consumer.Push/Dockerfile .
```

### Docker Compose

```bash
# Desenvolvimento (com PostgreSQL, RabbitMQ, Mailpit)
docker-compose up -d

# Produção (apenas aplicação, infra externa)
docker-compose -f docker-compose.production.yml up -d
```

## 📊 Monitoramento

- **RabbitMQ Management**: http://localhost:15672 (guest/guest)
- **Swagger UI**: http://localhost:5000/swagger
- **Health Checks**: Planejado

## 🔒 Segurança

Implementações atuais e planejadas:

- ✅ **ProblemDetails (RFC 7807)**: Respostas de erro padronizadas
- ✅ **FluentValidation**: Validação de entrada robusta
- ✅ **Global Exception Handler**: Tratamento centralizado de exceções
- 🔄 **API Key Authentication**: Em planejamento
- 🔄 **Rate Limiting**: Em planejamento
- 🔄 **CORS**: Configurável por ambiente
- 🔄 **Secrets**: User Secrets para dev, Azure Key Vault para produção

```bash
# Configurar User Secrets localmente
dotnet user-secrets init --project src/NotificationSystem.Api
dotnet user-secrets set "Services:Email:Smtp:Password" "your-password"
dotnet user-secrets set "Services:Sms:Twilio:AuthToken" "your-token"
```

## 📝 Status do Projeto

### ✅ Implementado e Production-Ready

#### Arquitetura e Padrões
- [x] Clean Architecture com 4 camadas bem definidas
- [x] Domain-Driven Design (DDD)
- [x] CQRS com MediatR
- [x] Result Pattern com FluentResults
- [x] Repository Pattern
- [x] Domain Events
- [x] Event-Driven Architecture

#### Domain Layer
- [x] Entidades: Notification (Aggregate Root)
- [x] NotificationChannel (base abstrata) + EmailChannel, SmsChannel, PushChannel
- [x] Table Per Hierarchy (TPH) para polimorfismo
- [x] Enums: NotificationType, NotificationStatus
- [x] Domain Events: NotificationCreatedEvent
- [x] Channel-based architecture (multi-canal)

#### Application Layer
- [x] MediatR configurado com pipeline behaviors
- [x] FluentValidation integrado ao pipeline
- [x] DTOs polimórficos para todos os canais
- [x] Mappings de entidades para DTOs
- [x] Use Cases: CreateNotification, GetAllNotifications
- [x] Domain Event Handlers

#### Infrastructure Layer
- [x] Repository implementations (NotificationRepository)
- [x] EF Core DbContext e Configurations
- [x] Migrations aplicadas (PostgreSQL)
- [x] RabbitMQ Producer com Dead Letter Exchange (DLX)
- [x] Integração SMTP (MailKit) - Email consumer
- [x] Integração Twilio (SMS) - SMS consumer production-ready
- [x] Retry logic e Dead Letter Queue (DLQ) para todos os canais
- [x] Serilog configurado com logs estruturados

#### Presentation Layer
- [x] Minimal API configurada
- [x] Global Exception Handler com ProblemDetails
- [x] ResultExtensions para conversão automática
- [x] Endpoint: GET /api/notifications (com paginação)
- [x] Endpoint: POST /api/notifications (multi-canal)
- [x] RabbitMQ Consumers (Email, SMS, Push) como BackgroundServices

#### DevOps & Deploy
- [x] Dockerfiles para API e todos os Consumers
- [x] docker-compose.yml (desenvolvimento)
- [x] docker-compose.production.yml (produção)
- [x] Scripts de build e push
- [x] Scripts de database migrations
- [x] .env.production.example

#### Documentação
- [x] README completo
- [x] Tech spec da codebase (`documentos/techspec-codebase.md`)
- [ ] Documentação do sistema de canais (CHANNEL_SYSTEM.md)
- [ ] Guia de deployment (DEPLOYMENT.md)
- [ ] Quick start guide (QUICKSTART.md)

### 🔄 Planejado / Melhorias Futuras

#### External Services
- [x] ~~Integração Twilio (SMS)~~ - ✅ **Implementado e production-ready**
- [ ] Integração Firebase (Push) - código preparado
- [ ] Circuit Breaker pattern

#### Security
- [ ] API Key Authentication (parcialmente implementado)
- [ ] Rate Limiting
- [ ] HTTPS enforcement em produção

#### Observability
- [ ] Health checks endpoint
- [ ] Métricas (Prometheus/OpenTelemetry)
- [ ] Distributed tracing
- [ ] Dashboards (Grafana)

#### Testing
- [ ] Unit tests (Domain, Application)
- [ ] Integration tests (API, Infrastructure)
- [ ] E2E tests
- [ ] Test coverage > 80%

#### DevOps
- [ ] CI/CD pipeline (GitHub Actions)
- [ ] Kubernetes manifests
- [ ] Helm charts

#### Features Avançados
- [ ] MongoDB para dados não estruturados e filtros customizados
- [ ] Templates de notificação
- [ ] Scheduling de notificações
- [ ] Webhook callbacks
- [ ] Admin UI (frontend)

## 🎯 Integrações Front-end

Esta API foi projetada para ser consumida por aplicações front-end. Para gerar automaticamente tipos TypeScript:

### Usando NSwag

```bash
# Instalar NSwag CLI
dotnet tool install -g NSwag.ConsoleCore

# Gerar cliente TypeScript
nswag openapi2tsclient /input:http://localhost:5000/swagger/v1/swagger.json /output:api-client.ts
```

### Usando OpenAPI Generator

```bash
# Instalar OpenAPI Generator
npm install -g @openapitools/openapi-generator-cli

# Gerar cliente
openapi-generator-cli generate \
  -i http://localhost:5000/swagger/v1/swagger.json \
  -g typescript-fetch \
  -o ./src/api
```

### Exemplo de uso no front-end (TypeScript)

```typescript
// Tipos gerados automaticamente
import { NotificationDto, EmailNotificationDto, GetAllNotificationsResponse } from './api-client';

// Type-safe!
const response: GetAllNotificationsResponse = await fetch('/api/notifications').then(r => r.json());

response.notifications.forEach(notification => {
  switch (notification.type) {
    case "Email":
      const email = notification as EmailNotificationDto;
      console.log(email.subject); // ✅ TypeScript sabe que existe
      break;
    case "Sms":
      // ...
  }
});
```

### Configuração segura de providers (Settings/Admin)

Para edição segura de credenciais no frontend/admin:

- `GET /api/admin/providers/{id}/configuration`
  - Retorna apenas campos não sensíveis.
  - Segredos são representados por flags (`passwordConfigured`, `authTokenConfigured`, `apiKeyConfigured`, `credentialsJsonConfigured`).
- `PUT /api/admin/providers/{id}`
  - Atualiza configuração parcialmente.
  - Campos sensíveis omitidos ou vazios preservam o valor já salvo.
- `POST /api/admin/providers/upload`
  - Upload multipart para credenciais em arquivo (ex.: Firebase JSON).
- `POST /api/admin/providers/{id}/test-connection`
  - Executa validação de conectividade/credencial sem envio real (SMTP/Twilio/Firebase/SendGrid).
  - Requer `provider.update`.

### Realtime de progresso bulk (SignalR)

- Hub: `/hubs/bulk-progress`
- Autenticação: JWT (mesmo token da API, via `access_token` no handshake do SignalR).
- Permissão: `bulk-notification.view`.
- Fluxo:
  - Cliente chama `Subscribe(jobId)` no hub.
  - API publica eventos `BulkProgressUpdated` por grupo `jobId`.
  - Payload inclui status, percent, total, processados, sucesso/falha e timestamps.

## 🤝 Contribuindo

1. Fork o projeto
2. Crie uma branch para sua feature (`git checkout -b feature/MinhaFeature`)
3. Commit suas mudanças (`git commit -m 'feat: adiciona MinhaFeature'`)
4. Push para a branch (`git push origin feature/MinhaFeature`)
5. Abra um Pull Request

### Conventional Commits

Este projeto usa Conventional Commits:

- `feat:` Nova funcionalidade
- `fix:` Correção de bug
- `docs:` Documentação
- `refactor:` Refatoração
- `test:` Testes
- `chore:` Tarefas de build/configuração

## 📄 Licença

Este projeto está sob a licença MIT. Veja o arquivo [LICENSE](LICENSE) para mais detalhes.

## 📧 Contato

Yuri Garcia Ribeiro - [@YuriGarciaRibeiro](https://github.com/YuriGarciaRibeiro)

Link do Projeto: [https://github.com/YuriGarciaRibeiro/API-notifications](https://github.com/YuriGarciaRibeiro/API-notifications)

---

## 🎯 Sobre o Projeto

Sistema de notificações **self-hosted production-ready** desenvolvido seguindo as melhores práticas de arquitetura .NET:

- ✅ **Clean Architecture** - Separação clara de responsabilidades
- ✅ **Domain-Driven Design** - Modelo de domínio rico e expressivo
- ✅ **Event-Driven** - Processamento assíncrono escalável
- ✅ **Multi-Channel** - Email + SMS + Push em uma única notificação
- ✅ **Docker-Ready** - Imagens otimizadas para produção
- ✅ **Self-Hosted** - Deploy independente por empresa

**Status:** Production-ready para deploy via Docker com infraestrutura própria.

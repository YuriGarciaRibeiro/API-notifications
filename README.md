# Sistema de Notificações - .NET

## 📚 Sobre o Projeto

**Backend API REST** para sistema de notificações assíncrono desenvolvido em **.NET** com **ASP.NET Core** e **RabbitMQ**. Este projeto implementa um serviço production-ready de gerenciamento e envio de notificações por múltiplos canais, projetado para ser consumido por aplicações front-end (web/mobile) e outros serviços.

### 🎯 Objetivo

Criar uma **API backend escalável e resiliente** que oferece:
- 📧 **Email** - Envio via SMTP
- 📱 **SMS** - Integração com Twilio
- 🔔 **Push Notifications** - Via Firebase Cloud Messaging
- 📊 **Gerenciamento** - Consulta e rastreamento de notificações enviadas

### 🌟 Características da API

- **Backend-only**: Serviço REST puro, sem interface gráfica (UI separada)
- **Contract-first**: API bem definida com OpenAPI/Swagger
- **Type-safe**: DTOs polimórficos compatíveis com geração automática de tipos TypeScript
- **Docker-ready**: Containerizado e pronto para deploy em Kubernetes/Cloud
- **Consumível por**: Web apps, mobile apps, SPAs, outros microserviços
- **CQRS Pattern**: Separação clara entre comandos e queries usando MediatR
- **Result Pattern**: Respostas padronizadas com FluentResults
- **Error Handling**: Tratamento global de erros com ProblemDetails (RFC 7807)

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
- **Contém**: Entities (Notification, EmailNotification, SmsNotification, PushNotification), Value Objects, Enums
- **Dependências**: Nenhuma (núcleo da aplicação)

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
│   │   └── Entities/
│   │       ├── Notification.cs              # Entidade base
│   │       ├── EmailNotification.cs         # Herança polimórfica
│   │       ├── SmsNotification.cs
│   │       └── PushNotification.cs
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
│       └── NotificationSystem.Consumer.Push/
│
├── docs/                                    # 📖 Documentação
│   ├── EXCEPTION_HANDLING.md
│   └── POLYMORPHIC_DTOS.md
│
├── tests/                                   # 🧪 Testes
│   ├── NotificationSystem.Domain.Tests/
│   ├── NotificationSystem.Application.Tests/
│   └── NotificationSystem.Api.Tests/
│
├── NotificationSystem.sln
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

## 🚀 Começando

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
dotnet restore
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

### DTOs Polimórficos

A API retorna notificações de forma polimórfica, onde cada tipo tem seus próprios campos específicos:

- **Email**: `to`, `subject`, `body`, `isBodyHtml`
- **SMS**: `to`, `message`, `senderId`
- **Push**: `to`, `content`, `data`, `priority`, `timeToLive`, `isRead`

Todos os tipos compartilham campos comuns: `id`, `userId`, `createdAt`, `status`, `errorMessage`, `sentAt`

> 📘 Para mais detalhes sobre DTOs polimórficos, veja [docs/POLYMORPHIC_DTOS.md](docs/POLYMORPHIC_DTOS.md)

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
- **RabbitMQ.Client** - Cliente oficial RabbitMQ
- **MailKit** - Envio de emails via SMTP
- **Twilio** - SDK para envio de SMS
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

### Build das imagens

```bash
# API
docker build -t notification-api -f src/NotificationSystem.Api/Dockerfile .

# Consumers
docker build -t notification-consumer-email -f src/Consumers/NotificationSystem.Consumer.Email/Dockerfile .
docker build -t notification-consumer-sms -f src/Consumers/NotificationSystem.Consumer.Sms/Dockerfile .
docker build -t notification-consumer-push -f src/Consumers/NotificationSystem.Consumer.Push/Dockerfile .
```

### Docker Compose (ambiente completo)

```bash
docker-compose up -d
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

### ✅ Implementado

#### Arquitetura e Padrões
- [x] Clean Architecture com 4 camadas bem definidas
- [x] Domain-Driven Design (DDD)
- [x] CQRS com MediatR
- [x] Result Pattern com FluentResults
- [x] Repository Pattern

#### Application Layer
- [x] MediatR configurado com pipeline behaviors
- [x] FluentValidation integrado ao pipeline
- [x] DTOs polimórficos (EmailNotificationDto, SmsNotificationDto, PushNotificationDto)
- [x] Mappings de entidades para DTOs
- [x] Use Case: GetAllNotifications com paginação

#### Presentation Layer
- [x] Minimal API configurada
- [x] Global Exception Handler com ProblemDetails
- [x] ResultExtensions para conversão automática
- [x] Endpoint: GET /api/notifications

#### Domain Layer
- [x] Entidades: Notification, EmailNotification, SmsNotification, PushNotification
- [x] Enums: NotificationType, NotificationStatus
- [x] Herança polimórfica

#### Documentação
- [x] README completo
- [x] Documentação de DTOs polimórficos
- [x] Documentação de tratamento de exceções

### 🔄 Em Desenvolvimento / Planejado

#### Persistence
- [ ] Repository implementations
- [ ] EF Core DbContext e Configurations
- [ ] Migrations
- [ ] Seed data

#### Messaging
- [ ] RabbitMQ Producer na Infrastructure
- [ ] RabbitMQ Consumers (Email, SMS, Push)
- [ ] Retry logic e Dead Letter Queue (DLQ)
- [ ] Message contracts e serialização

#### External Services
- [ ] Integração SMTP (MailKit)
- [ ] Integração Twilio (SMS)
- [ ] Integração Firebase (Push Notifications)
- [ ] Circuit Breaker pattern

#### API Endpoints
- [ ] POST /api/notifications/email
- [ ] POST /api/notifications/sms
- [ ] POST /api/notifications/push
- [ ] GET /api/notifications/{id}
- [ ] DELETE /api/notifications/{id}

#### Security & Observability
- [ ] API Key Authentication
- [ ] Rate Limiting
- [ ] CORS configuration
- [ ] Health checks
- [ ] Logging estruturado (Serilog)
- [ ] Métricas (Prometheus/OpenTelemetry)
- [ ] Distributed tracing

#### Testing
- [ ] Unit tests (Domain, Application)
- [ ] Integration tests (API, Infrastructure)
- [ ] E2E tests
- [ ] Test coverage > 80%

#### DevOps
- [ ] CI/CD pipeline (GitHub Actions)
- [ ] Docker Compose para ambiente completo
- [ ] Kubernetes manifests
- [ ] Helm charts

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

**Nota**: Este é um projeto de estudo focado em boas práticas de desenvolvimento .NET, Clean Architecture, e design de APIs RESTful modernas. O projeto serve como backend para um sistema de notificações completo, com front-end sendo desenvolvido separadamente.

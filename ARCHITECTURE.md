# Sistema de Notificações - Arquitetura e Planejamento

## Visão Geral

Sistema de notificações **production-ready** em Go com arquitetura de filas distribuída, projetado para alta disponibilidade, escalabilidade e observabilidade.

### Componentes Principais

- **API REST** para receber requisições de notificações
- **RabbitMQ** para processamento assíncrono e confiável
- **Workers especializados** para cada tipo de notificação (Email, SMS, Push, Webhook)
- **Dead Letter Queue** para tratamento de falhas e retry logic
- **PostgreSQL** para persistência e auditoria
- **Observabilidade completa** (Logs, Métricas, Traces)

### Requisitos de Produção

Este sistema foi arquitetado considerando:
- ✅ **Alta Disponibilidade** - Workers podem escalar horizontalmente
- ✅ **Resiliência** - Retry automático, circuit breakers, timeouts
- ✅ **Segurança** - Autenticação, rate limiting, validação de inputs
- ✅ **Observabilidade** - Logs estruturados, métricas Prometheus, tracing
- ✅ **Performance** - Connection pooling, concorrência eficiente
- ✅ **Manutenibilidade** - Código testável, documentado, com padrões claros

---

## Arquitetura do Sistema

```
┌─────────────┐         ┌─────────────┐         ┌──────────────────┐
│  Sistemas   │────────>│   API REST  │────────>│    RabbitMQ      │
│  Externos   │         │   (Go/Gin)  │         │   Exchange       │
└─────────────┘         └─────────────┘         │   (Topic)        │
                                                 └────────┬─────────┘
                                                          │
                        ┌─────────────────────────────────┼──────────────────┐
                        │                                 │                  │
                  ┌─────▼─────┐                    ┌─────▼─────┐     ┌─────▼─────┐
                  │   Queue   │                    │   Queue   │     │   Queue   │
                  │   Email   │                    │    SMS    │     │   Push    │
                  └─────┬─────┘                    └─────┬─────┘     └─────┬─────┘
                        │                                │                  │
                  ┌─────▼─────┐                    ┌─────▼─────┐     ┌─────▼─────┐
                  │  Worker   │                    │  Worker   │     │  Worker   │
                  │   Email   │                    │    SMS    │     │   Push    │
                  └───────────┘                    └───────────┘     └───────────┘
                        │                                │                  │
                        └────────────────┬───────────────┘                  │
                                         │                                  │
                                   ┌─────▼─────┐                           │
                                   │    DLQ    │<──────────────────────────┘
                                   │  (Failed) │
                                   └───────────┘
```

---

## Fluxo de Funcionamento

1. **Sistema externo chama API** → `POST /api/v1/notifications`
2. **API valida requisição** → Verifica payload e cria notificação
3. **API publica no RabbitMQ** → Envia para exchange com routing key (ex: `notification.email`)
4. **Exchange roteia mensagem** → Direciona para fila específica baseado no tipo
5. **Worker consome fila** → Worker especializado processa a notificação
6. **Envio da notificação** → Worker chama serviço externo (SMTP, Twilio, Firebase)
7. **ACK ou NACK** → Worker confirma sucesso ou rejeita para retry/DLQ
8. **Callback/Log** → Atualiza status e notifica sistema via webhook (opcional)

---

## Tecnologias Utilizadas

### Sistema de Filas: RabbitMQ ✅

**Por que RabbitMQ?**
- ✅ Retry automático com exponential backoff
- ✅ Dead Letter Queue (DLQ) nativo
- ✅ Confirmação de entrega (ACK/NACK)
- ✅ Suporte a prioridades de mensagens
- ✅ Management UI para monitoramento
- ✅ Alta confiabilidade e durabilidade
- ✅ Perfeito para produção crítica

### Bibliotecas Go Utilizadas

#### API e Framework
- **API REST**: `gin-gonic/gin` - Framework web popular e performático
- **Validação**: `go-playground/validator/v10` - Validação de structs
- **UUID**: `google/uuid` - Geração de IDs únicos

#### Fila
- **RabbitMQ**: `rabbitmq/amqp091-go` - Cliente oficial RabbitMQ

#### Notificações
- **Email**: `go-gomail/gomail` ou `mailgun/mailgun-go`
- **SMS**: `twilio/twilio-go` ou `vonage/vonage-go-sdk`
- **Push**: `firebase/firebase-admin-go` ou `sideshow/apns2`

#### Infraestrutura
- **Configuração**: `spf13/viper` ou `joho/godotenv`
- **Logging**: `sirupsen/logrus` ou `uber-go/zap`
- **Métricas**: `prometheus/client_golang`

---

## Estrutura de Pastas Proposta

```
api-notifications/
├── cmd/
│   ├── api/                    # Servidor API
│   │   └── main.go
│   └── workers/                # Workers
│       ├── email/
│       │   └── main.go
│       ├── sms/
│       │   └── main.go
│       ├── push/
│       │   └── main.go
│       └── webhook/
│           └── main.go
│
├── internal/                   # Código privado da aplicação
│   ├── models/                 # Estruturas de dados
│   │   ├── notification.go
│   │   └── response.go
│   │
│   ├── api/                    # Lógica da API
│   │   ├── handlers/           # Handlers HTTP
│   │   │   ├── notification.go
│   │   │   └── health.go
│   │   ├── middleware/         # Middlewares
│   │   │   ├── auth.go
│   │   │   ├── ratelimit.go
│   │   │   └── logger.go
│   │   └── router/
│   │       └── router.go
│   │
│   ├── queue/                  # Lógica de filas
│   │   ├── producer.go         # Publicar mensagens
│   │   ├── consumer.go         # Consumir mensagens
│   │   └── redis.go / rabbitmq.go
│   │
│   ├── workers/                # Lógica dos workers
│   │   ├── email_worker.go
│   │   ├── sms_worker.go
│   │   ├── push_worker.go
│   │   └── webhook_worker.go
│   │
│   ├── services/               # Serviços externos
│   │   ├── email/
│   │   │   ├── smtp.go
│   │   │   └── mailgun.go
│   │   ├── sms/
│   │   │   └── twilio.go
│   │   └── push/
│   │       └── firebase.go
│   │
│   ├── repository/             # Acesso a dados (opcional)
│   │   └── notification_repo.go
│   │
│   └── config/                 # Configurações
│       └── config.go
│
├── pkg/                        # Código reutilizável público
│   ├── logger/
│   │   └── logger.go
│   ├── utils/
│   │   └── validators.go
│   └── errors/
│       └── errors.go
│
├── configs/                    # Arquivos de configuração
│   ├── config.yaml
│   ├── config.dev.yaml
│   └── config.prod.yaml
│
├── migrations/                 # Migrações de banco (opcional)
│   └── 001_create_notifications.sql
│
├── scripts/                    # Scripts úteis
│   ├── setup.sh
│   └── deploy.sh
│
├── docker/                     # Dockerfiles
│   ├── Dockerfile.api
│   └── Dockerfile.worker
│
├── docker-compose.yml          # Setup local (Redis/RabbitMQ)
├── .env.example
├── go.mod
├── go.sum
├── Makefile
└── README.md
```

---

## Modelo de Dados

### Estrutura de Notificação

```go
package models

import "time"

type NotificationType string

const (
    TypeEmail   NotificationType = "email"
    TypeSMS     NotificationType = "sms"
    TypePush    NotificationType = "push"
    TypeWebhook NotificationType = "webhook"
)

type NotificationStatus string

const (
    StatusPending  NotificationStatus = "pending"
    StatusQueued   NotificationStatus = "queued"
    StatusSending  NotificationStatus = "sending"
    StatusSent     NotificationStatus = "sent"
    StatusFailed   NotificationStatus = "failed"
    StatusRetrying NotificationStatus = "retrying"
)

type Notification struct {
    ID          string                 `json:"id" db:"id"`
    Type        NotificationType       `json:"type" binding:"required" db:"type"`
    To          string                 `json:"to" binding:"required" db:"to_address"`
    From        string                 `json:"from,omitempty" db:"from_address"`
    Subject     string                 `json:"subject,omitempty" db:"subject"`
    Body        string                 `json:"body" binding:"required" db:"body"`
    BodyHTML    string                 `json:"body_html,omitempty" db:"body_html"`
    Metadata    map[string]interface{} `json:"metadata,omitempty" db:"metadata"`
    Priority    int                    `json:"priority" db:"priority"` // 1-5
    Status      NotificationStatus     `json:"status" db:"status"`
    Attempts    int                    `json:"attempts" db:"attempts"`
    MaxAttempts int                    `json:"max_attempts" db:"max_attempts"`
    ErrorMsg    string                 `json:"error_message,omitempty" db:"error_message"`
    ScheduledAt *time.Time             `json:"scheduled_at,omitempty" db:"scheduled_at"`
    SentAt      *time.Time             `json:"sent_at,omitempty" db:"sent_at"`
    CreatedAt   time.Time              `json:"created_at" db:"created_at"`
    UpdatedAt   time.Time              `json:"updated_at" db:"updated_at"`
}

type NotificationRequest struct {
    Type        NotificationType       `json:"type" binding:"required,oneof=email sms push webhook"`
    To          string                 `json:"to" binding:"required"`
    From        string                 `json:"from,omitempty"`
    Subject     string                 `json:"subject,omitempty"`
    Body        string                 `json:"body" binding:"required"`
    BodyHTML    string                 `json:"body_html,omitempty"`
    Metadata    map[string]interface{} `json:"metadata,omitempty"`
    Priority    int                    `json:"priority" binding:"min=1,max=5"`
    ScheduledAt *time.Time             `json:"scheduled_at,omitempty"`
}

type NotificationResponse struct {
    ID        string             `json:"id"`
    Status    NotificationStatus `json:"status"`
    Message   string             `json:"message"`
    CreatedAt time.Time          `json:"created_at"`
}

type BatchNotificationRequest struct {
    Notifications []NotificationRequest `json:"notifications" binding:"required,min=1,max=100"`
}
```

---

## Endpoints da API

### Notificações

```
POST   /api/v1/notifications              # Criar notificação única
POST   /api/v1/notifications/batch        # Criar múltiplas notificações
GET    /api/v1/notifications/:id          # Obter status da notificação
GET    /api/v1/notifications              # Listar notificações (filtros)
DELETE /api/v1/notifications/:id          # Cancelar notificação pendente
```

### Sistema

```
GET    /api/v1/health                     # Health check
GET    /api/v1/metrics                    # Métricas (Prometheus)
GET    /api/v1/stats                      # Estatísticas das filas
```

### Exemplos de Request/Response

#### POST /api/v1/notifications

**Request:**
```json
{
  "type": "email",
  "to": "usuario@example.com",
  "from": "noreply@app.com",
  "subject": "Bem-vindo!",
  "body": "Olá, bem-vindo ao nosso sistema.",
  "body_html": "<h1>Olá</h1><p>Bem-vindo ao nosso sistema.</p>",
  "priority": 3,
  "metadata": {
    "user_id": "12345",
    "campaign": "welcome"
  }
}
```

**Response:**
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "status": "queued",
  "message": "Notification queued successfully",
  "created_at": "2025-11-02T10:30:00Z"
}
```

#### GET /api/v1/notifications/:id

**Response:**
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "type": "email",
  "to": "usuario@example.com",
  "subject": "Bem-vindo!",
  "status": "sent",
  "attempts": 1,
  "sent_at": "2025-11-02T10:30:15Z",
  "created_at": "2025-11-02T10:30:00Z"
}
```

---

## Configuração do RabbitMQ

### Estrutura de Exchanges e Queues

```
Exchange: notifications.exchange (type: topic)
  │
  ├─ Routing Key: notification.email
  │  └─> Queue: notifications.email
  │      ├─ DLX: notifications.dlx.exchange
  │      ├─ Max Retries: 3
  │      └─> Worker Email (múltiplos consumers)
  │
  ├─ Routing Key: notification.sms
  │  └─> Queue: notifications.sms
  │      ├─ DLX: notifications.dlx.exchange
  │      ├─ Max Retries: 3
  │      └─> Worker SMS (múltiplos consumers)
  │
  ├─ Routing Key: notification.push
  │  └─> Queue: notifications.push
  │      ├─ DLX: notifications.dlx.exchange
  │      ├─ Max Retries: 3
  │      └─> Worker Push (múltiplos consumers)
  │
  └─ Routing Key: notification.webhook
     └─> Queue: notifications.webhook
         ├─ DLX: notifications.dlx.exchange
         ├─ Max Retries: 3
         └─> Worker Webhook (múltiplos consumers)

Exchange: notifications.dlx.exchange (Dead Letter Exchange)
  └─> Queue: notifications.dlq
      └─> Manual reprocessing or monitoring
```

### Características das Queues

**Queues Principais:**
- **Durabilidade**: `durable: true` - Sobrevive a restart do RabbitMQ
- **Auto-delete**: `false` - Não deleta automaticamente
- **TTL**: Configurável por mensagem (ex: 1 hora)
- **Max Priority**: 10 (suporte a priorização)
- **Dead Letter Exchange**: Configurado para retry automático

**Dead Letter Queue (DLQ):**
- Recebe mensagens que falharam após X tentativas
- Permite análise manual e reprocessamento
- Não tem consumidores automáticos (processamento manual)

### docker-compose.yml

```yaml
version: '3.8'

services:
  rabbitmq:
    image: rabbitmq:3.12-management-alpine
    container_name: rabbitmq-notifications
    hostname: rabbitmq
    ports:
      - "5672:5672"    # AMQP protocol
      - "15672:15672"  # Management UI
    environment:
      RABBITMQ_DEFAULT_USER: admin
      RABBITMQ_DEFAULT_PASS: admin123
      RABBITMQ_DEFAULT_VHOST: /
    volumes:
      - rabbitmq_data:/var/lib/rabbitmq
      - rabbitmq_logs:/var/log/rabbitmq
    healthcheck:
      test: rabbitmq-diagnostics -q ping
      interval: 30s
      timeout: 10s
      retries: 5
    networks:
      - notifications-network

  # PostgreSQL para persistência (opcional)
  postgres:
    image: postgres:15-alpine
    container_name: postgres-notifications
    environment:
      POSTGRES_USER: notifications
      POSTGRES_PASSWORD: notifications123
      POSTGRES_DB: notifications_db
    ports:
      - "5432:5432"
    volumes:
      - postgres_data:/var/lib/postgresql/data
    networks:
      - notifications-network

volumes:
  rabbitmq_data:
  rabbitmq_logs:
  postgres_data:

networks:
  notifications-network:
    driver: bridge
```

### Acessando RabbitMQ Management UI

Após rodar `docker-compose up -d`:
- **URL**: http://localhost:15672
- **Usuário**: admin
- **Senha**: admin123

No Management UI você pode:
- Visualizar filas e mensagens
- Monitorar taxa de processamento
- Ver mensagens na DLQ
- Republicar mensagens manualmente
- Visualizar conexões e canais ativos

---

## Features e Funcionalidades

### Core Features (MVP)

- ✅ API REST para receber notificações
- ✅ Sistema de filas (Redis ou RabbitMQ)
- ✅ Workers para Email, SMS, Push
- ✅ Validação de payloads
- ✅ Health checks
- ✅ Logging estruturado

### Features Avançadas

#### 1. **Retry Logic**
```go
// Tentar reenviar automaticamente em caso de falha
MaxAttempts: 3
RetryDelay: [1min, 5min, 15min] // Exponential backoff
```

#### 2. **Dead Letter Queue (DLQ)**
```go
// Notificações que falharam após todas as tentativas
// Permite análise manual e reprocessamento
```

#### 3. **Rate Limiting**
```go
// Controlar taxa de envio por tipo
EmailRateLimit: 100/min
SMSRateLimit: 50/min
```

#### 4. **Templates**
```go
// Templates reutilizáveis
POST /api/v1/notifications/template
{
  "template_id": "welcome_email",
  "to": "user@example.com",
  "variables": {
    "name": "João",
    "code": "ABC123"
  }
}
```

#### 5. **Webhooks de Status**
```go
// Callback quando notificação for enviada/falhar
{
  "callback_url": "https://app.com/webhook",
  "events": ["sent", "failed"]
}
```

#### 6. **Agendamento**
```go
// Agendar notificação para envio futuro
{
  "scheduled_at": "2025-11-03T10:00:00Z"
}
```

#### 7. **Priorização**
```go
// Fila prioritária para notificações urgentes
Priority: 1 (lowest) - 5 (highest)
```

#### 8. **Dashboard de Monitoramento**
- Status das filas em tempo real
- Gráficos de envios (por tipo, status)
- Taxa de sucesso/falha
- Latência média

#### 9. **Métricas (Prometheus)**
```go
// Exportar métricas
- notifications_total{type, status}
- notifications_duration_seconds
- queue_size{type}
- worker_processing_duration_seconds
```

#### 10. **Persistência (Banco de Dados)**
```go
// Histórico de notificações
PostgreSQL ou MongoDB para armazenar:
- Histórico completo
- Auditoria
- Relatórios
```

---

## Configuração da Aplicação

### Exemplo: config.yaml

```yaml
app:
  name: "Notification Service"
  env: "development"
  port: 8080

queue:
  type: "rabbitmq"

  rabbitmq:
    url: "amqp://admin:admin123@localhost:5672/"
    exchange: "notifications.exchange"
    exchange_type: "topic"
    dlx_exchange: "notifications.dlx.exchange"
    max_retries: 3
    retry_delay: 5000  # ms - delay entre retries

workers:
  email:
    enabled: true
    concurrency: 5
    rate_limit: 100 # por minuto

  sms:
    enabled: true
    concurrency: 3
    rate_limit: 50

  push:
    enabled: true
    concurrency: 10
    rate_limit: 200

  webhook:
    enabled: true
    concurrency: 5

services:
  email:
    provider: "smtp" # ou "mailgun", "sendgrid"
    smtp:
      host: "smtp.gmail.com"
      port: 587
      username: "user@gmail.com"
      password: "secret"
      from: "noreply@app.com"

  sms:
    provider: "twilio"
    twilio:
      account_sid: "ACxxxxx"
      auth_token: "xxxxx"
      from_number: "+1234567890"

  push:
    provider: "firebase"
    firebase:
      credentials_file: "./firebase-credentials.json"

database:
  enabled: true
  driver: "postgres"
  host: "localhost"
  port: 5432
  user: "postgres"
  password: "secret"
  dbname: "notifications"

logging:
  level: "info" # debug, info, warn, error
  format: "json" # json ou text

security:
  api_key_enabled: true
  jwt_enabled: false
  allowed_origins: ["*"]
```

---

## Como Executar (Fluxo Proposto)

### 1. Setup Inicial

```bash
# Clonar e instalar dependências
git clone https://github.com/yurir/api-notifications.git
cd api-notifications
go mod download

# Copiar configuração
cp .env.example .env
cp configs/config.example.yaml configs/config.yaml

# Subir Redis/RabbitMQ localmente
docker-compose up -d

# Verificar se está rodando
docker ps
```

### 2. Rodar API

```bash
# Terminal 1: API
cd cmd/api
go run main.go

# Output esperado:
# [GIN] Listening on :8080
# API ready to receive notifications
```

### 3. Rodar Workers

```bash
# Terminal 2: Worker de Email
cd cmd/workers/email
go run main.go

# Terminal 3: Worker de SMS
cd cmd/workers/sms
go run main.go

# Terminal 4: Worker de Push
cd cmd/workers/push
go run main.go
```

### 4. Testar

```bash
# Enviar notificação de teste
curl -X POST http://localhost:8080/api/v1/notifications \
  -H "Content-Type: application/json" \
  -d '{
    "type": "email",
    "to": "test@example.com",
    "subject": "Teste",
    "body": "Mensagem de teste"
  }'

# Verificar status
curl http://localhost:8080/api/v1/notifications/{id}

# Health check
curl http://localhost:8080/api/v1/health
```

---

## Deploy e Produção

### Docker

```dockerfile
# Dockerfile.api
FROM golang:1.21-alpine AS builder
WORKDIR /app
COPY go.mod go.sum ./
RUN go mod download
COPY . .
RUN go build -o api ./cmd/api

FROM alpine:latest
RUN apk --no-cache add ca-certificates
WORKDIR /root/
COPY --from=builder /app/api .
COPY configs/ ./configs/
EXPOSE 8080
CMD ["./api"]
```

### Kubernetes

```yaml
# deployment.yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: notification-api
spec:
  replicas: 3
  selector:
    matchLabels:
      app: notification-api
  template:
    metadata:
      labels:
        app: notification-api
    spec:
      containers:
      - name: api
        image: notification-api:latest
        ports:
        - containerPort: 8080
        env:
        - name: REDIS_HOST
          value: "redis-service"
---
apiVersion: apps/v1
kind: Deployment
metadata:
  name: notification-worker-email
spec:
  replicas: 2
  selector:
    matchLabels:
      app: notification-worker-email
  template:
    metadata:
      labels:
        app: notification-worker-email
    spec:
      containers:
      - name: worker
        image: notification-worker:latest
        env:
        - name: WORKER_TYPE
          value: "email"
```

---

## Segurança

### Autenticação

```go
// API Key
X-API-Key: your-secret-key

// Ou JWT
Authorization: Bearer eyJhbGc...
```

### Rate Limiting

```go
// Por IP ou API Key
100 requests/minute
```

### Validação de Input

```go
// Sanitizar emails, telefones, URLs
// Prevenir injection attacks
```

### Secrets Management

```bash
# Não commitar secrets
# Usar variáveis de ambiente ou Vault
export TWILIO_AUTH_TOKEN=xxx
export SMTP_PASSWORD=xxx
```

---

## Monitoramento e Observabilidade

### Logs Estruturados

```json
{
  "level": "info",
  "time": "2025-11-02T10:30:00Z",
  "msg": "notification sent",
  "notification_id": "550e8400...",
  "type": "email",
  "duration_ms": 234,
  "status": "sent"
}
```

### Métricas (Prometheus)

```
# HELP notifications_total Total notifications processed
# TYPE notifications_total counter
notifications_total{type="email",status="sent"} 1523

# HELP notification_duration_seconds Time to process notification
# TYPE notification_duration_seconds histogram
notification_duration_seconds_bucket{type="email",le="0.5"} 1200
```

### Alertas

```yaml
# alerts.yaml
- alert: HighFailureRate
  expr: |
    rate(notifications_total{status="failed"}[5m]) > 0.1
  annotations:
    summary: "Alta taxa de falha nas notificações"
```

---

## Próximos Passos

### Fase 1: MVP
1. Setup do projeto Go com estrutura de pastas
2. Docker Compose com RabbitMQ
3. API básica com Gin (endpoints de notificação)
4. Sistema de filas com RabbitMQ (producer)
5. Worker de Email (SMTP) com consumer
6. Testes locais end-to-end

### Fase 2: Expandir
7. Workers de SMS, Push e Webhook
8. Retry logic e Dead Letter Queue
9. Persistência em PostgreSQL
10. Templates de notificações
11. Sistema de prioridades

### Fase 3: Produção
12. Autenticação (API Keys)
13. Rate limiting
14. Métricas (Prometheus)
15. Logging estruturado (Zap)
16. Dockerfiles para API e Workers
17. Documentação completa (Swagger)

---

## Decisões Técnicas Tomadas

### Definições do Projeto:

1. **Sistema de filas**: ✅ **RabbitMQ** - Escolhido por robustez e features de produção
2. **Tipos de notificação**: ✅ **Email, SMS, Push, Webhook** - Suporte completo
3. **Persistência**: ✅ **PostgreSQL** (opcional) - Para histórico e auditoria
4. **Autenticação**: ✅ **API Key** - Simples e efetivo
5. **Deploy**: ✅ **Docker + Docker Compose** - Fácil setup local e produção
6. **Provedores** (implementar conforme necessidade):
   - Email: SMTP genérico (configurável para Gmail, Mailgun, SendGrid)
   - SMS: Twilio (popular e bem documentado)
   - Push: Firebase Cloud Messaging (multiplataforma)

---

## Recursos Úteis

### Documentação
- [Gin Framework](https://gin-gonic.com/docs/)
- [Go Redis](https://redis.uptrace.dev/)
- [RabbitMQ Go](https://www.rabbitmq.com/tutorials/tutorial-one-go.html)
- [Twilio Go](https://www.twilio.com/docs/libraries/go)
- [Firebase Admin Go](https://firebase.google.com/docs/admin/setup)

### Exemplos de Projetos Similares
- [github.com/caio/notifyd](https://github.com)
- [github.com/mercari/notification-service](https://github.com)

---

## 🔒 Segurança em Produção

### Camadas de Segurança

#### 1. **Autenticação e Autorização**

```go
// API Key (simples e efetivo)
type AuthMiddleware struct {
    validKeys map[string]bool
}

func (a *AuthMiddleware) Authenticate(c *gin.Context) {
    apiKey := c.GetHeader("X-API-Key")
    if !a.validKeys[apiKey] {
        c.AbortWithStatus(http.StatusUnauthorized)
        return
    }
    c.Next()
}

// JWT (para autenticação mais complexa)
claims := jwt.MapClaims{
    "user_id": userID,
    "exp": time.Now().Add(time.Hour * 24).Unix(),
}
```

**Implementação:**
- API Keys armazenados em variáveis de ambiente ou secret manager
- Rotação periódica de keys
- Escopo de permissões por key (ex: key só para email)
- Rate limit por key

#### 2. **Validação de Inputs**

```go
// Validar TODOS os inputs
type NotificationRequest struct {
    Type    string `json:"type" binding:"required,oneof=email sms push webhook"`
    To      string `json:"to" binding:"required,email"` // usa tag de validação
    Body    string `json:"body" binding:"required,max=10000"` // limite de tamanho
}

// Sanitização adicional
func SanitizeInput(input string) string {
    // Remove caracteres perigosos
    // Previne injection attacks
    return html.EscapeString(strings.TrimSpace(input))
}
```

**Checklist de Validação:**
- ✅ Validar formato de emails
- ✅ Validar formato de telefones (E.164)
- ✅ Validar URLs de webhooks (allow-list de domínios)
- ✅ Limitar tamanho de payloads (max 1MB)
- ✅ Sanitizar HTML em emails
- ✅ Validar Firebase device tokens

#### 3. **Rate Limiting**

```go
// Rate limiter por IP ou API Key
import "golang.org/x/time/rate"

type RateLimiter struct {
    limiters map[string]*rate.Limiter
    mu       sync.RWMutex
}

func (rl *RateLimiter) Allow(key string) bool {
    rl.mu.RLock()
    limiter, exists := rl.limiters[key]
    rl.mu.RUnlock()

    if !exists {
        limiter = rate.NewLimiter(rate.Limit(100), 200) // 100 req/s, burst 200
        rl.mu.Lock()
        rl.limiters[key] = limiter
        rl.mu.Unlock()
    }

    return limiter.Allow()
}
```

**Configuração Recomendada:**
- Por IP: 100 req/min
- Por API Key: 1000 req/min
- Global: 10000 req/min
- Resposta 429 (Too Many Requests) com header Retry-After

#### 4. **Secrets Management**

```bash
# ❌ NUNCA fazer isso
const TWILIO_AUTH_TOKEN = "SK123abc..."

# ✅ Usar variáveis de ambiente
export TWILIO_AUTH_TOKEN="SK123abc..."
export SMTP_PASSWORD="secret123"
export JWT_SECRET="random-long-string"

# ✅ Ou usar secret managers
# AWS Secrets Manager
# HashiCorp Vault
# Google Secret Manager
```

**Boas Práticas:**
- Nunca commitar secrets no git
- Usar .env para dev, secret manager para prod
- Rotacionar secrets periodicamente
- Logs nunca devem expor secrets
- Encriptar secrets em banco de dados

#### 5. **HTTPS/TLS**

```go
// Forçar HTTPS em produção
func RequireHTTPS() gin.HandlerFunc {
    return func(c *gin.Context) {
        if c.Request.Header.Get("X-Forwarded-Proto") != "https" {
            c.Redirect(http.StatusPermanentRedirect,
                "https://" + c.Request.Host + c.Request.RequestURI)
            c.Abort()
            return
        }
        c.Next()
    }
}
```

**Requisitos TLS:**
- TLS 1.3 mínimo
- Certificados válidos (Let's Encrypt)
- HSTS headers
- Desabilitar ciphers inseguros

#### 6. **CORS**

```go
// Configurar CORS adequadamente
import "github.com/gin-contrib/cors"

config := cors.Config{
    AllowOrigins:     []string{"https://app.example.com"},
    AllowMethods:     []string{"GET", "POST", "DELETE"},
    AllowHeaders:     []string{"Origin", "Content-Type", "X-API-Key"},
    AllowCredentials: true,
    MaxAge:           12 * time.Hour,
}
router.Use(cors.New(config))
```

#### 7. **Logs de Auditoria**

```go
// Logar todas as ações críticas
logger.Info("notification_created",
    zap.String("id", notif.ID),
    zap.String("type", string(notif.Type)),
    zap.String("api_key", apiKeyID), // não logar a key completa
    zap.String("ip", c.ClientIP()),
    zap.Time("timestamp", time.Now()),
)

// Logar falhas de autenticação
logger.Warn("authentication_failed",
    zap.String("ip", c.ClientIP()),
    zap.String("attempted_key", apiKey[:8]+"..."), // apenas primeiros chars
)
```

---

## 🔍 Observabilidade Production-Grade

### 1. **Logging Estruturado (Zap)**

```go
// Configuração de produção
config := zap.NewProductionConfig()
config.OutputPaths = []string{"stdout", "/var/log/app/notifications.log"}
config.Level = zap.NewAtomicLevelAt(zap.InfoLevel)
logger, _ := config.Build()

// Logs contextuais
logger.Info("processing_notification",
    zap.String("notification_id", id),
    zap.String("type", typ),
    zap.Duration("queue_time", queueTime),
    zap.Int("attempt", attempt),
)
```

**Padrões de Log:**
- `info` - Eventos normais (notificação criada, enviada)
- `warn` - Situações suspeitas (retry, rate limit atingido)
- `error` - Falhas recuperáveis (falha ao enviar, será retentado)
- `fatal` - Falhas críticas (não consegue conectar ao RabbitMQ)

### 2. **Métricas (Prometheus)**

```go
var (
    notificationsTotal = promauto.NewCounterVec(
        prometheus.CounterOpts{
            Name: "notifications_total",
            Help: "Total notifications processed",
        },
        []string{"type", "status"},
    )

    notificationDuration = promauto.NewHistogramVec(
        prometheus.HistogramOpts{
            Name: "notification_duration_seconds",
            Help: "Time to process notification",
            Buckets: []float64{.1, .5, 1, 2, 5, 10},
        },
        []string{"type"},
    )

    queueSize = promauto.NewGaugeVec(
        prometheus.GaugeOpts{
            Name: "queue_size",
            Help: "Current queue size",
        },
        []string{"type"},
    )
)

// Uso
notificationsTotal.WithLabelValues("email", "sent").Inc()
notificationDuration.WithLabelValues("email").Observe(duration.Seconds())
```

**Métricas Essenciais:**
- Taxa de notificações (por tipo, status)
- Latência (p50, p95, p99)
- Taxa de erro (por tipo)
- Tamanho das filas
- Workers ativos
- Tentativas de retry
- Rate limit hits

### 3. **Health Checks**

```go
// Health check robusto
func HealthCheck(db *sql.DB, rmq *amqp.Connection) gin.HandlerFunc {
    return func(c *gin.Context) {
        health := map[string]interface{}{
            "status": "healthy",
            "timestamp": time.Now(),
            "checks": map[string]string{},
        }

        // Check database
        if err := db.Ping(); err != nil {
            health["checks"]["database"] = "unhealthy"
            health["status"] = "degraded"
        } else {
            health["checks"]["database"] = "healthy"
        }

        // Check RabbitMQ
        if rmq.IsClosed() {
            health["checks"]["rabbitmq"] = "unhealthy"
            health["status"] = "unhealthy"
            c.JSON(503, health)
            return
        }
        health["checks"]["rabbitmq"] = "healthy"

        statusCode := 200
        if health["status"] == "degraded" {
            statusCode = 200 // ainda pode servir tráfego
        } else if health["status"] == "unhealthy" {
            statusCode = 503
        }

        c.JSON(statusCode, health)
    }
}
```

**Endpoints de Health:**
- `/health` - Status geral (liveness probe)
- `/ready` - Pronto para receber tráfego (readiness probe)
- `/metrics` - Métricas Prometheus

### 4. **Tracing Distribuído (OpenTelemetry)**

```go
// Configurar tracing
import "go.opentelemetry.io/otel"

tracer := otel.Tracer("notification-service")

// Criar span
ctx, span := tracer.Start(ctx, "send_notification")
defer span.End()

span.SetAttributes(
    attribute.String("notification.id", id),
    attribute.String("notification.type", typ),
)

// Propagar contexto para workers
// Permite rastrear notificação da API até envio final
```

---

## ⚡ Performance e Escalabilidade

### 1. **Connection Pooling**

```go
// PostgreSQL Pool
db, err := sql.Open("postgres", dsn)
db.SetMaxOpenConns(25)
db.SetMaxIdleConns(25)
db.SetConnMaxLifetime(5 * time.Minute)

// RabbitMQ Channel Pool
type ChannelPool struct {
    pool chan *amqp.Channel
}

func NewChannelPool(conn *amqp.Connection, size int) *ChannelPool {
    pool := make(chan *amqp.Channel, size)
    for i := 0; i < size; i++ {
        ch, _ := conn.Channel()
        pool <- ch
    }
    return &ChannelPool{pool: pool}
}
```

### 2. **Timeouts e Contexts**

```go
// Timeout em todas as operações externas
ctx, cancel := context.WithTimeout(context.Background(), 10*time.Second)
defer cancel()

// HTTP client com timeout
httpClient := &http.Client{
    Timeout: 10 * time.Second,
    Transport: &http.Transport{
        MaxIdleConns:        100,
        MaxIdleConnsPerHost: 10,
        IdleConnTimeout:     90 * time.Second,
    },
}
```

### 3. **Graceful Shutdown**

```go
// Desligar workers de forma limpa
func GracefulShutdown(worker *Worker) {
    sigChan := make(chan os.Signal, 1)
    signal.Notify(sigChan, os.Interrupt, syscall.SIGTERM)

    <-sigChan
    logger.Info("shutdown signal received")

    // Parar de aceitar novas mensagens
    worker.Stop()

    // Esperar workers atuais terminarem (max 30s)
    ctx, cancel := context.WithTimeout(context.Background(), 30*time.Second)
    defer cancel()

    worker.WaitForCompletion(ctx)
    logger.Info("shutdown complete")
}
```

### 4. **Escalabilidade Horizontal**

```yaml
# Múltiplas instâncias de workers
# Kubernetes Deployment
apiVersion: apps/v1
kind: Deployment
metadata:
  name: notification-worker-email
spec:
  replicas: 5  # Escala conforme carga
  template:
    spec:
      containers:
      - name: worker
        image: notification-worker:latest
        resources:
          requests:
            memory: "128Mi"
            cpu: "100m"
          limits:
            memory: "512Mi"
            cpu: "500m"
```

---

## 🧪 Testes Production-Ready

### 1. **Testes Unitários**

```go
func TestNotificationHandler(t *testing.T) {
    // Setup
    mockProducer := &MockProducer{}
    handler := NewNotificationHandler(mockProducer)

    // Test
    req := NotificationRequest{
        Type: "email",
        To:   "test@example.com",
        Body: "Test message",
    }

    resp, err := handler.CreateNotification(req)

    // Assert
    assert.NoError(t, err)
    assert.NotEmpty(t, resp.ID)
    assert.Equal(t, "queued", resp.Status)
    assert.Equal(t, 1, mockProducer.PublishCallCount)
}
```

### 2. **Testes de Integração**

```go
func TestEmailWorkerIntegration(t *testing.T) {
    // Setup real RabbitMQ (test container)
    rabbitmq := testcontainers.RunRabbitMQ(t)
    defer rabbitmq.Terminate()

    // Setup worker
    worker := NewEmailWorker(rabbitmq.ConnectionString())

    // Publish test message
    producer.Publish("notification.email", testNotification)

    // Assert message was consumed and email sent
    time.Sleep(2 * time.Second)
    assert.Equal(t, 1, mockSMTP.SentCount())
}
```

### 3. **Testes E2E**

```go
func TestEndToEndFlow(t *testing.T) {
    // Setup sistema completo
    api := StartTestAPI(t)
    workers := StartTestWorkers(t)

    // Call API
    resp := api.POST("/api/v1/notifications", payload)
    assert.Equal(t, 200, resp.StatusCode)

    // Wait for processing
    time.Sleep(5 * time.Second)

    // Verify notification was sent
    status := api.GET("/api/v1/notifications/" + resp.ID)
    assert.Equal(t, "sent", status.Status)
}
```

### 4. **Testes de Carga**

```bash
# k6 load test
k6 run --vus 100 --duration 5m load-test.js

# Verificar:
# - Latência se mantém < 500ms no p95
# - Zero erros 5xx
# - Taxa de sucesso > 99%
# - CPU < 70%
# - Memória estável (sem leaks)
```

---

## 📋 Checklist de Deploy para Produção

### Antes do Deploy

- [ ] Todos os testes passando (unit + integration + e2e)
- [ ] Code coverage > 80%
- [ ] Linting OK (golangci-lint)
- [ ] Secrets configurados (não no código)
- [ ] TLS/HTTPS configurado
- [ ] Rate limiting ativado
- [ ] Logging estruturado configurado
- [ ] Métricas expostas (/metrics)
- [ ] Health checks implementados (/health, /ready)
- [ ] Graceful shutdown funcionando
- [ ] Documentação API atualizada (Swagger)

### Infraestrutura

- [ ] RabbitMQ em cluster (HA)
- [ ] PostgreSQL com replicação
- [ ] Backups automáticos configurados
- [ ] Disaster recovery testado
- [ ] Monitoramento configurado (Grafana)
- [ ] Alertas configurados (PagerDuty/Slack)
- [ ] Logs centralizados (ELK/Loki)

### Operações

- [ ] Runbook documentado
- [ ] Procedimento de rollback definido
- [ ] Limite de recursos definido (CPU, RAM)
- [ ] Auto-scaling configurado (se aplicável)
- [ ] Estratégia de deploy (blue/green, canary)

---

**Este sistema está sendo construído para produção desde o primeiro dia.** 🚀

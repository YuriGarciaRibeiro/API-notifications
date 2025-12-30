# Quick Start Guide - Notification System

Guia rápido para empresas que querem hospedar o sistema de notificações.

## 🚀 Início Rápido (5 minutos)

### 1. Pré-requisitos

Você precisa ter configurado:
- ✅ PostgreSQL (local, AWS RDS, Azure Database, etc.)
- ✅ RabbitMQ (local, CloudAMQP, AWS MQ, etc.)
- ✅ Servidor SMTP para envio de emails

### 2. Obter as Imagens

```bash
# Opção 1: Pull do registry (quando disponível)
docker pull your-registry/notification-system-api:latest
docker pull your-registry/notification-system-consumer-email:latest
docker pull your-registry/notification-system-consumer-sms:latest
docker pull your-registry/notification-system-consumer-push:latest

# Opção 2: Build local
git clone https://github.com/yourcompany/notification-system.git
cd notification-system
./scripts/build-and-push.sh
```

### 3. Configurar Ambiente

```bash
# Copiar arquivo de exemplo
cp .env.production.example .env

# Editar com suas credenciais
nano .env
```

**Configuração mínima:**
```bash
# Database
DATABASE_CONNECTION_STRING=Host=your-db.com;Port=5432;Database=notifications;Username=user;Password=pass

# RabbitMQ
RABBITMQ_HOST=your-rabbitmq.com
RABBITMQ_USERNAME=user
RABBITMQ_PASSWORD=pass

# SMTP
SMTP_HOST=smtp.yourcompany.com
SMTP_USERNAME=notifications@yourcompany.com
SMTP_PASSWORD=your-password
SMTP_FROM_EMAIL=noreply@yourcompany.com
```

### 4. Executar Migrations

```bash
docker run --rm \
  -e ConnectionStrings__DefaultConnection="$DATABASE_CONNECTION_STRING" \
  your-registry/notification-system-api:latest \
  dotnet ef database update
```

### 5. Iniciar Serviços

```bash
docker-compose -f docker-compose.production.yml up -d
```

### 6. Verificar

```bash
# Health check
curl http://localhost:5000/health

# Ver logs
docker-compose -f docker-compose.production.yml logs -f
```

---

## 📤 Enviar Primeira Notificação

### Apenas Email

```bash
curl -X POST http://localhost:5000/api/notifications \
  -H "Content-Type: application/json" \
  -d '{
    "userId": "550e8400-e29b-41d4-a716-446655440000",
    "channels": [
      {
        "type": "Email",
        "to": "user@example.com",
        "subject": "Test Notification",
        "body": "Hello from Notification System!",
        "isBodyHtml": false
      }
    ]
  }'
```

### Email + SMS + Push

```bash
curl -X POST http://localhost:5000/api/notifications \
  -H "Content-Type: application/json" \
  -d '{
    "userId": "550e8400-e29b-41d4-a716-446655440000",
    "channels": [
      {
        "type": "Email",
        "to": "user@example.com",
        "subject": "Welcome!",
        "body": "<h1>Welcome!</h1><p>Your account is ready.</p>",
        "isBodyHtml": true
      },
      {
        "type": "Sms",
        "to": "+5511999999999",
        "message": "Welcome! Your account is ready.",
        "senderId": "MyApp"
      },
      {
        "type": "Push",
        "to": "device-token-fcm",
        "content": {
          "title": "Welcome!",
          "body": "Your account is ready.",
          "clickAction": "/dashboard"
        },
        "priority": "high"
      }
    ]
  }'
```

---

## 🔍 Consultar Notificações

```bash
# Listar todas (paginado)
curl http://localhost:5000/api/notifications?pageNumber=1&pageSize=10

# Buscar por ID
curl http://localhost:5000/api/notifications/{id}
```

---

## ⚙️ Configurações Avançadas

### Escalar Consumers

Edite `.env`:
```bash
EMAIL_CONSUMER_REPLICAS=5    # Para alto volume de emails
SMS_CONSUMER_REPLICAS=2
PUSH_CONSUMER_REPLICAS=3
```

Reinicie:
```bash
docker-compose -f docker-compose.production.yml up -d
```

### Usar Twilio (SMS)

Configure no `.env`:
```bash
TWILIO_ACCOUNT_SID=ACxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
TWILIO_AUTH_TOKEN=your-auth-token
TWILIO_FROM_NUMBER=+15551234567
```

### Usar Firebase (Push)

1. Baixe credenciais do Firebase Console
2. Salve em `/etc/notification-system/firebase-credentials.json`
3. Configure no `.env`:
```bash
FIREBASE_CREDENTIALS_HOST_PATH=/etc/notification-system/firebase-credentials.json
```

---

## 📊 Monitoramento

### Logs em Tempo Real

```bash
docker-compose -f docker-compose.production.yml logs -f api
```

### RabbitMQ Management

```
http://your-rabbitmq:15672
```

### Health Check

```bash
curl http://localhost:5000/health
```

---

## 🐛 Problemas Comuns

### API não inicia

```bash
# Ver erro
docker logs notification-api

# Comum: connection string errada
# Solução: verificar DATABASE_CONNECTION_STRING no .env
```

### Emails não enviam

```bash
# Ver logs do consumer
docker logs notification-consumer-email

# Comum: credenciais SMTP incorretas
# Solução: verificar SMTP_* no .env
```

### Consumer não processa

```bash
# Verificar se RabbitMQ está acessível
docker exec notification-consumer-email ping your-rabbitmq-host

# Verificar filas no RabbitMQ
# Acessar: http://your-rabbitmq:15672
```

---

## 📚 Documentação Completa

- [Deployment Guide](./DEPLOYMENT.md) - Guia completo de deploy
- [Channel System](./CHANNEL_SYSTEM.md) - Documentação dos canais
- [README](../README.md) - Visão geral do projeto

---

## 🎯 Arquitetura Simplificada

```
┌──────────┐
│   API    │ ← Recebe requisições HTTP
└────┬─────┘
     │
     ▼
┌──────────┐
│ RabbitMQ │ ← Distribui para workers
└──┬───┬───┘
   │   │
   ▼   ▼
┌──────┬──────┐
│Email │ SMS  │ ← Processam em paralelo
└──────┴──────┘
```

**Fluxo:**
1. Cliente envia POST para `/api/notifications`
2. API salva no PostgreSQL
3. API publica mensagens no RabbitMQ
4. Consumers processam (Email, SMS, Push)
5. Status atualizado no banco

---

## 🔒 Segurança (Opcional)

### Adicionar API Key

Configure no `.env`:
```bash
API_KEY=seu-segredo-aqui-min-32-caracteres
```

Use nas requisições:
```bash
curl -H "X-API-Key: seu-segredo-aqui" \
     http://localhost:5000/api/notifications
```

---

## 📞 Suporte

Para dúvidas técnicas, consulte a [documentação completa](./DEPLOYMENT.md) ou entre em contato com o time de desenvolvimento.

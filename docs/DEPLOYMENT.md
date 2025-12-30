# Deployment Guide - Notification System

Este guia descreve como fazer deploy do sistema de notificações em ambiente de produção.

## 📋 Pré-requisitos

### Infraestrutura Necessária

A empresa que for utilizar o sistema precisa fornecer:

1. **PostgreSQL 14+**
   - Database criado
   - Usuário com permissões de CREATE/ALTER (para migrations)
   - Connection string disponível

2. **RabbitMQ 3.x**
   - Acesso ao servidor RabbitMQ
   - Credenciais (username/password)
   - Virtual Host configurado (opcional)

3. **SMTP Server** (para notificações por email)
   - Servidor SMTP da empresa
   - Credenciais de autenticação
   - Porta e configuração SSL/TLS

4. **Twilio Account** (para SMS) - ✅ Production-Ready
   - Account SID
   - Auth Token
   - Número de telefone Twilio ativo

5. **Firebase Project** (para Push) - Opcional
   - Arquivo de credenciais JSON
   - Projeto configurado no Firebase Console

6. **Docker & Docker Compose**
   - Docker 20.10+
   - Docker Compose 2.0+

---

## 🚀 Instalação

### Passo 1: Obter as Imagens Docker

#### Opção A: Build Local
```bash
# Clone o repositório
git clone https://github.com/yourcompany/notification-system.git
cd notification-system

# Build das imagens
./build-and-push.sh 1.0.0
```

#### Opção B: Pull do Registry (quando disponível)
```bash
docker pull your-registry.azurecr.io/notification-system-api:1.0.0
docker pull your-registry.azurecr.io/notification-system-consumer-email:1.0.0
docker pull your-registry.azurecr.io/notification-system-consumer-sms:1.0.0
docker pull your-registry.azurecr.io/notification-system-consumer-push:1.0.0
```

### Passo 2: Configurar Variáveis de Ambiente

```bash
# Copiar arquivo de exemplo
cp .env.example .env

# Editar com suas configurações
nano .env
```

**Configurações obrigatórias:**
```bash
# Database
DATABASE_CONNECTION_STRING=Host=your-postgres-server;Port=5432;Database=notifications;Username=user;Password=pass

# RabbitMQ
RABBITMQ_HOST=your-rabbitmq-server
RABBITMQ_USERNAME=your-user
RABBITMQ_PASSWORD=your-password

# SMTP
SMTP_HOST=smtp.yourcompany.com
SMTP_USERNAME=notifications@yourcompany.com
SMTP_PASSWORD=your-smtp-password
SMTP_FROM_EMAIL=noreply@yourcompany.com
SMTP_FROM_NAME=Company Notifications

# Twilio SMS (Production-Ready)
TWILIO_ACCOUNT_SID=ACxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
TWILIO_AUTH_TOKEN=your-twilio-auth-token
TWILIO_FROM_NUMBER=+15551234567
```

### Passo 3: Executar Migrations do Banco de Dados

#### Opção A: Via Docker (Recomendado)
```bash
docker run --rm \
  -e ConnectionStrings__DefaultConnection="$DATABASE_CONNECTION_STRING" \
  your-registry/notification-system-api:1.0.0 \
  dotnet ef database update
```

#### Opção B: Manualmente com Script SQL
```bash
# Scripts SQL estão em src/NotificationSystem.Infrastructure/Migrations/
# Executar na ordem cronológica
psql -h your-postgres -U user -d notifications -f 20251211102720_InitialMigration.sql
```

### Passo 4: Iniciar os Serviços

```bash
# Usando docker-compose.production.yml
docker-compose -f docker-compose.production.yml up -d

# Verificar status
docker-compose -f docker-compose.production.yml ps

# Ver logs
docker-compose -f docker-compose.production.yml logs -f
```

### Passo 5: Verificar Saúde da Aplicação

```bash
# Health check da API
curl http://localhost:5000/health

# Verificar logs
docker logs notification-api
docker logs notification-consumer-email
```

---

## 🔧 Configurações Avançadas

### Escalar Consumers

Edite `.env` para ajustar número de réplicas:

```bash
EMAIL_CONSUMER_REPLICAS=5    # Aumentar para alto volume de emails
SMS_CONSUMER_REPLICAS=2      # Ajustar conforme volume de SMS
PUSH_CONSUMER_REPLICAS=3     # Ajustar conforme volume de push
```

Reinicie os serviços:
```bash
docker-compose -f docker-compose.production.yml up -d --scale consumer-email=5
```

### Configurar Firebase (Push Notifications)

1. Baixe o arquivo de credenciais do Firebase Console
2. Salve em local seguro (ex: `/etc/notification-system/firebase-credentials.json`)
3. Configure no `.env`:
```bash
FIREBASE_CREDENTIALS_HOST_PATH=/etc/notification-system/firebase-credentials.json
```

### Usar Managed Services na Cloud

#### Azure PostgreSQL
```bash
DATABASE_CONNECTION_STRING=Host=yourserver.postgres.database.azure.com;Port=5432;Database=notifications;Username=admin@yourserver;Password=pass;SslMode=Require
```

#### AWS RDS PostgreSQL
```bash
DATABASE_CONNECTION_STRING=Host=yourinstance.xxxx.us-east-1.rds.amazonaws.com;Port=5432;Database=notifications;Username=postgres;Password=pass;SslMode=Require
```

#### CloudAMQP (RabbitMQ as a Service)
```bash
RABBITMQ_HOST=pelican.rmq.cloudamqp.com
RABBITMQ_USERNAME=xxxxx
RABBITMQ_PASSWORD=xxxxx
```

---

## 📊 Monitoramento

### Logs

```bash
# Ver logs em tempo real
docker-compose -f docker-compose.production.yml logs -f api
docker-compose -f docker-compose.production.yml logs -f consumer-email

# Logs de todos os serviços
docker-compose -f docker-compose.production.yml logs -f
```

### Health Checks

A API expõe endpoint de health:
```bash
GET /health

Response:
{
  "status": "healthy",
  "timestamp": "2025-12-29T10:00:00Z"
}
```

### Métricas RabbitMQ

Acesse o Management UI do RabbitMQ:
```
http://your-rabbitmq-server:15672
```

Monitore:
- Tamanho das filas (email-notifications, sms-notifications, push-notifications)
- Taxa de consumo
- Mensagens em retry (DLQ)

---

## 🔒 Segurança

### API Key (Opcional)

Configure uma API Key no `.env`:
```bash
API_KEY=your-secure-random-key-here
```

Use no header das requisições:
```bash
curl -H "X-API-Key: your-secure-random-key-here" \
     http://localhost:5000/api/notifications
```

### Network Isolation

Para maior segurança, crie uma rede Docker isolada:
```yaml
networks:
  notification-network:
    driver: bridge
    internal: true  # Bloqueia acesso externo
```

### Secrets Management

**Produção:** Não usar `.env` em produção. Usar:
- Azure Key Vault
- AWS Secrets Manager
- HashiCorp Vault
- Docker Secrets

Exemplo com Docker Secrets:
```bash
echo "your-password" | docker secret create postgres_password -
```

---

## 🔄 Atualizações

### Deploy de Nova Versão

```bash
# Pull nova imagem
docker pull your-registry/notification-system-api:1.1.0

# Atualizar VERSION no .env
VERSION=1.1.0

# Restart com downtime mínimo
docker-compose -f docker-compose.production.yml up -d --no-deps api

# Verificar health
curl http://localhost:5000/health
```

### Rollback

```bash
# Voltar para versão anterior
VERSION=1.0.0
docker-compose -f docker-compose.production.yml up -d --no-deps api
```

---

## 🐛 Troubleshooting

### API não inicia

```bash
# Verificar logs
docker logs notification-api

# Problemas comuns:
# 1. Connection string incorreta
# 2. Migrations não executadas
# 3. Porta já em uso
```

### Consumers não processam mensagens

```bash
# Verificar logs do consumer
docker logs notification-consumer-email

# Verificar RabbitMQ
docker exec notification-rabbitmq rabbitmqctl list_queues

# Verificar conectividade
docker exec notification-consumer-email ping rabbitmq-host
```

### Emails não sendo enviados

```bash
# Verificar logs do consumer email
docker logs notification-consumer-email

# Testar SMTP manualmente
telnet smtp.yourcompany.com 587

# Verificar credenciais SMTP no .env
```

### SMS não sendo enviados (Twilio)

```bash
# Verificar logs do consumer SMS
docker logs notification-consumer-sms

# Verificar credenciais Twilio
# Account SID deve começar com "AC"
# Número deve estar no formato E.164: +[country code][number]

# Verificar saldo da conta Twilio
# https://console.twilio.com/

# Testar manualmente via Twilio Console
# https://www.twilio.com/console/sms/getting-started/test-credentials
```

---

## 📞 Exemplo de Uso

### Enviar Notificação Multi-Canal

```bash
curl -X POST http://localhost:5000/api/notifications \
  -H "Content-Type: application/json" \
  -H "X-API-Key: your-api-key" \
  -d '{
    "userId": "550e8400-e29b-41d4-a716-446655440000",
    "channels": [
      {
        "type": "Email",
        "to": "user@example.com",
        "subject": "Welcome!",
        "body": "<h1>Welcome to our platform!</h1>",
        "isBodyHtml": true
      },
      {
        "type": "Sms",
        "to": "+5511999999999",
        "message": "Welcome! Your account is ready."
      },
      {
        "type": "Push",
        "to": "device-token-here",
        "content": {
          "title": "Welcome",
          "body": "Your account is ready!"
        }
      }
    ]
  }'
```

### Listar Notificações

```bash
curl http://localhost:5000/api/notifications?pageNumber=1&pageSize=10 \
  -H "X-API-Key: your-api-key"
```

---

## 📚 Arquitetura

```
┌─────────────────┐
│   Load Balancer │
└────────┬────────┘
         │
    ┌────▼─────┐
    │   API    │───────┐
    │ (Scaled) │       │
    └────┬─────┘       │
         │             │
    ┌────▼─────────────▼──────┐
    │      RabbitMQ           │
    └──┬────────┬──────────┬──┘
       │        │          │
  ┌────▼───┐ ┌─▼─────┐ ┌─▼──────┐
  │ Email  │ │  SMS  │ │  Push  │
  │Consumer│ │Consumer│ │Consumer│
  │(Scaled)│ │       │ │(Scaled)│
  └────┬───┘ └───┬───┘ └───┬────┘
       │         │          │
  ┌────▼───┐ ┌──▼────┐ ┌───▼─────┐
  │  SMTP  │ │Twilio │ │Firebase │
  └────────┘ └───────┘ └─────────┘
       │         │          │
  ┌────▼─────────▼──────────▼────┐
  │       PostgreSQL              │
  └───────────────────────────────┘
```

---

## 🤝 Suporte

Para dúvidas e suporte, entre em contato com o time de desenvolvimento.

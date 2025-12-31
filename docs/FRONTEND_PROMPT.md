# Projeto: Frontend para Sistema de Notificações Self-Hosted

## 📋 Contexto do Projeto

Preciso que você crie o frontend para um **Sistema Centralizador de Notificações Self-Hosted** desenvolvido em .NET. O backend já está pronto e funcionando.

### O que o sistema faz:
- Sistema **self-hosted** que empresas hospedam internamente via Docker
- Envio de notificações por **múltiplos canais simultaneamente**:
  - 📧 **Email** (via SMTP)
  - 📱 **SMS** (via Twilio)
  - 🔔 **Push Notifications** (via Firebase)
- Uma única notificação pode ter Email + SMS + Push ao mesmo tempo
- Cada canal tem **status independente** (Email ✅ enviado / SMS ❌ falhou)
- Processamento assíncrono com RabbitMQ

---

## 🎯 Objetivo do Frontend

Criar um **painel administrativo (Admin Dashboard)** para:

1. **Visualizar notificações** - listar, filtrar, paginar
2. **Criar notificações** - formulário para enviar via múltiplos canais
3. **Monitorar status** - ver status de cada canal (Pending, Sent, Failed)
4. **Gerenciar configurações** - (futuro) configurar provedores dinamicamente

---

## 🔌 API Disponível

### Base URL: `http://localhost:5000`

### Endpoints:

#### 1. Listar Notificações (GET)
```
GET /api/notifications?pageNumber=1&pageSize=10
```

**Resposta:**
```json
{
  "notifications": [
    {
      "id": "uuid",
      "userId": "uuid",
      "createdAt": "2025-12-10T10:30:00Z",
      "channels": [
        {
          "type": "Email",
          "id": "uuid",
          "status": "Sent",
          "errorMessage": null,
          "sentAt": "2025-12-10T10:31:00Z",
          "to": "user@example.com",
          "subject": "Welcome!",
          "body": "Hello world"
        },
        {
          "type": "Sms",
          "id": "uuid",
          "status": "Failed",
          "errorMessage": "Invalid phone number",
          "sentAt": null,
          "to": "+5511999999999",
          "message": "Your code is 123456"
        },
        {
          "type": "Push",
          "id": "uuid",
          "status": "Pending",
          "to": "device-token",
          "content": {
            "title": "New Message",
            "body": "You have a notification"
          }
        }
      ]
    }
  ],
  "totalCount": 150,
  "pageNumber": 1,
  "pageSize": 10
}
```

#### 2. Criar Notificação (POST)
```
POST /api/notifications
Content-Type: application/json
```

**Body (multi-canal):**
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
      "message": "Your verification code is 123456"
    },
    {
      "type": "Push",
      "to": "device-fcm-token",
      "content": {
        "title": "Welcome",
        "body": "Your account is ready!",
        "clickAction": "/dashboard"
      },
      "priority": "high"
    }
  ]
}
```

---

## 🎨 Requisitos do Frontend

### Páginas necessárias:

#### 1. **Dashboard Principal**
- Estatísticas rápidas: total de notificações, por status (Pending/Sent/Failed)
- Gráfico de notificações por canal (Email/SMS/Push)
- Lista das últimas notificações

#### 2. **Lista de Notificações**
- Tabela com paginação
- Filtros: por status, por canal, por data
- Cada linha mostra: ID, userId, data, canais com badges de status
- Expandir linha para ver detalhes dos canais

#### 3. **Criar Notificação**
- Formulário com seleção de canais (checkboxes)
- Campos dinâmicos baseado nos canais selecionados:
  - **Email**: to, subject, body (rich text editor), isBodyHtml
  - **SMS**: to (telefone), message
  - **Push**: deviceToken, title, body, clickAction, priority
- Preview antes de enviar

#### 4. **Detalhes da Notificação**
- Ver todos os dados da notificação
- Status de cada canal com timeline
- Mensagens de erro (se houver)
- Retry manual (futuro)

### Componentes reutilizáveis:
- **StatusBadge**: Pending (amarelo), Sent (verde), Failed (vermelho)
- **ChannelIcon**: Ícone para Email, SMS, Push
- **DataTable**: Tabela genérica com paginação
- **ChannelForm**: Formulários dinâmicos por tipo de canal

---

## 🛠️ Stack Sugerida

Escolha uma das opções:

**Opção 1 - React/Next.js**
- Next.js 14+ (App Router)
- TypeScript
- Tailwind CSS
- shadcn/ui para componentes
- TanStack Query para data fetching
- React Hook Form + Zod para formulários

**Opção 2 - Vue/Nuxt**
- Nuxt 3
- TypeScript
- Tailwind CSS
- Headless UI ou PrimeVue

**Opção 3 - Angular**
- Angular 17+
- TypeScript
- Angular Material ou Tailwind

---

## 📁 Estrutura Esperada

```
frontend/
├── src/
│   ├── components/
│   │   ├── ui/              # Componentes base (Button, Input, etc)
│   │   ├── notifications/   # Componentes específicos
│   │   └── layout/          # Header, Sidebar, etc
│   ├── pages/
│   │   ├── dashboard/
│   │   ├── notifications/
│   │   └── settings/
│   ├── services/
│   │   └── api.ts           # Cliente HTTP para a API
│   ├── types/
│   │   └── notification.ts  # TypeScript types
│   └── hooks/               # Custom hooks
├── public/
└── package.json
```

---

## 📝 Types TypeScript Esperados

```typescript
type ChannelType = "Email" | "Sms" | "Push";
type NotificationStatus = "Pending" | "Sent" | "Failed";

interface BaseChannel {
  id: string;
  type: ChannelType;
  status: NotificationStatus;
  errorMessage?: string;
  sentAt?: string;
}

interface EmailChannel extends BaseChannel {
  type: "Email";
  to: string;
  subject: string;
  body: string;
  isBodyHtml: boolean;
}

interface SmsChannel extends BaseChannel {
  type: "Sms";
  to: string;
  message: string;
  senderId?: string;
}

interface PushChannel extends BaseChannel {
  type: "Push";
  to: string;
  content: {
    title: string;
    body: string;
    clickAction?: string;
  };
  priority?: "high" | "normal";
}

type Channel = EmailChannel | SmsChannel | PushChannel;

interface Notification {
  id: string;
  userId: string;
  createdAt: string;
  channels: Channel[];
}

interface PaginatedResponse<T> {
  notifications: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
}
```

---

## 🎨 Design

- Layout limpo e moderno
- Tema claro com opção de dark mode
- Sidebar com navegação
- Responsivo (mobile-friendly)
- Cores sugeridas:
  - Primary: Azul (#3B82F6)
  - Success/Sent: Verde (#22C55E)
  - Warning/Pending: Amarelo (#EAB308)
  - Error/Failed: Vermelho (#EF4444)

---

## ⚡ Funcionalidades Prioritárias

### MVP (Primeira versão):
1. ✅ Dashboard com estatísticas básicas
2. ✅ Listar notificações com paginação
3. ✅ Ver detalhes de uma notificação
4. ✅ Criar notificação (pelo menos Email)

### Fase 2:
- Filtros avançados na listagem
- Formulário completo para SMS e Push
- Gráficos de analytics
- Dark mode

### Fase 3 (Futuro):
- Gerenciamento de provedores (Twilio, Firebase, etc)
- Templates de notificação
- Scheduling de notificações
- Webhooks

---

## 🚀 Como Começar

1. Crie o projeto com a stack escolhida
2. Configure o cliente HTTP apontando para `http://localhost:5000`
3. Implemente os tipos TypeScript
4. Comece pelo Dashboard e Lista de Notificações
5. Depois adicione o formulário de criação

Por favor, me apresente primeiro a estrutura do projeto e os componentes principais antes de começar a implementação.

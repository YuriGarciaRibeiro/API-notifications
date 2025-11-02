# Roadmap de Desenvolvimento - Sistema de Notificações

> **Guia Prático: Aprender Go Fazendo**
>
> Este documento é um passo a passo para construir o sistema enquanto aprende Go. Não pare para estudar teoria - aprenda o necessário conforme codifica. Cada etapa diz o que fazer, onde pesquisar quando travar, e por que aquilo importa.

---

## 📚 Como Usar Este Guia

**Abordagem: Deep Dive (Pesquisa → Entende → Implementa)**

```
┌─────────────────┐
│  Lê o Passo     │
└────────┬────────┘
         │
         v
┌─────────────────┐
│ 📚 PESQUISA     │◄─── Você está aqui (modo Deep Dive)
│ (lê docs/videos)│     Estude ANTES de codar
└────────┬────────┘
         │
         v
┌─────────────────┐
│ Entendeu?       │───── Não? Pesquisa mais!
│ Explica sozinho?│         │
└────────┬────────┘         │
         │ Sim              │
         v                  │
┌─────────────────┐         │
│ Tenta Codar     │<────────┘
└────────┬────────┘
         │
         v
┌─────────────────┐      Sim
│  Funciona?      │─────────> Próximo Passo!
└────────┬────────┘
         │ Não
         v
    Debugar/Pesquisa mais
```

**O projeto evolui em 3 fases para Production-Ready**:
1. **MVP** (Passos 1-12) → Sistema funcionando localmente
2. **Expandir** (Passos 13-18) → Features completas
3. **Production Hardening** (Passos 19-30) → Segurança, testes, observabilidade, deploy

> **Meta**: Ao fim do Passo 30, o sistema estará 100% pronto para produção com todas as garantias necessárias.

---

## 🎓 Modo Deep Dive: Como Estudar Cada Passo

Você escolheu aprender a fundo. Excelente! Cada passo tem esta estrutura:

### 📚 PESQUISE PRIMEIRO (tempo estimado)
Links específicos, vídeos, documentação. **Leia/assista tudo antes de codar.**

**Tipos de recursos**:
- 📺 = Vídeo (YouTube)
- 📖 = Documentação/Artigo
- 🎯 = Objetivo específico de aprendizado
- ✅ = Checkpoint (teste seu entendimento)

### ✅ Checkpoint de Entendimento
Perguntas para você responder **ANTES de codar**. Se não consegue responder, volte e pesquise mais.

### 💻 AGORA IMPLEMENTE
Só depois de entender, comece a codar. Aplique o que aprendeu.

### 💡 Por que importa
Contexto do mundo real. Como isso é usado em produção.

---

## ⏱️ Gestão do Tempo

**Deep Dive não significa lento!**

- **Pesquisa**: 30min - 2h por passo (dependendo da complexidade)
- **Implementação**: 30min - 2h
- **Total por passo**: 1-4h

**Dica**: Use técnica Pomodoro
- 25min estudo → 5min pausa
- 25min código → 5min pausa
- A cada 4 pomodoros: pausa de 15min

**Não se perca em rabbit holes!**
- Se pesquisa passou de 2h, está indo fundo demais
- Marque para aprofundar depois, mas continue o projeto

---

## 🎬 O Que Você Vai Construir

```
          Seu Sistema Completo
          ═══════════════════

    ┌─────────────────────────┐
    │   Aplicação Externa     │
    │   (faz POST /api...)    │
    └──────────┬──────────────┘
               │ HTTP
               v
    ┌─────────────────────────┐
    │    API REST (Go/Gin)    │◄─── Você constrói isso!
    │  - Recebe notificações  │
    │  - Valida dados         │
    └──────────┬──────────────┘
               │
               v
    ┌─────────────────────────┐
    │      RabbitMQ           │◄─── Docker
    │  - Filas por tipo       │
    │  - Retry automático     │
    └──────────┬──────────────┘
               │
         ┌─────┴─────┬────────┐
         v           v        v
    ┌────────┐  ┌────────┐  ┌────────┐
    │Worker  │  │Worker  │  │Worker  │◄─── Você constrói!
    │Email   │  │SMS     │  │Push    │
    └────┬───┘  └───┬────┘  └───┬────┘
         │          │           │
         v          v           v
    Gmail/SMTP  Twilio   Firebase
```

**No final**: Uma requisição HTTP vira email/SMS automaticamente!

---

## 🎯 Fase 1: Fundamentos e Setup (MVP Básico)

### Passo 1: Configurar o Módulo Go

**📁 Arquivo**: `go.mod`

**🎯 Objetivo**: Inicializar o projeto Go e entender o sistema de módulos.

**📚 PESQUISE PRIMEIRO (30min - 1h)**:

1. **O que são Go Modules?**
   - 📺 YouTube: "Go Modules Tutorial" (qualquer vídeo de 10-15min)
   - 📖 Leia: https://go.dev/blog/using-go-modules
   - 🎯 Entenda: O que é `go.mod`, `go.sum`, por que existem

2. **Como funcionam dependências em Go?**
   - 📖 Leia: https://go.dev/ref/mod
   - 🎯 Entenda: Diferença entre `go get`, `go install`, `go mod tidy`

3. **Versionamento semântico**
   - 📖 Pesquise: "semantic versioning 2.0"
   - 🎯 Entenda: O que é v1.2.3 (major.minor.patch)

**✅ Checkpoint de Entendimento**:
Antes de codar, responda mentalmente:
- O que `go mod init` faz?
- Por que preciso de um `go.mod`?
- O que acontece quando rodo `go get`?

**💻 AGORA IMPLEMENTE**:
```bash
go mod init github.com/yurir/api-notifications
go get github.com/gin-gonic/gin
go get github.com/rabbitmq/amqp091-go
```
Olhe os arquivos criados (`go.mod`, `go.sum`) e entenda o que tem neles.

**💡 Por que importa**:
Go Modules revolucionaram o ecossistema Go em 2019. Todo projeto profissional usa.

---

### Passo 2: Criar Estrutura de Configuração

**📁 Arquivos**:
- `internal/config/config.go`
- `configs/config.yaml`
- `.env.example`

**🎯 Objetivo**: Aprender a gerenciar configurações de forma segura e flexível.

**📚 PESQUISE PRIMEIRO (1-2h)**:

1. **Structs em Go** (fundamental!)
   - 📺 YouTube: "Golang Structs Tutorial"
   - 📖 Leia: https://gobyexample.com/structs
   - 📖 Leia: https://go.dev/tour/moretypes/2
   - 🎯 Entenda: Como declarar, inicializar, acessar campos
   - 🎯 Pratique: Crie uma struct `Person` com nome e idade no Go Playground

2. **Struct Tags**
   - 📖 Leia: https://www.digitalocean.com/community/tutorials/how-to-use-struct-tags-in-go
   - 🎯 Entenda: O que é `yaml:"nome" json:"name"`
   - 🎯 Entenda: Como as libs usam reflection para ler tags

3. **Biblioteca Viper**
   - 📖 Leia: https://github.com/spf13/viper (README completo)
   - 📺 YouTube: "Viper configuration golang"
   - 🎯 Entenda: Por que usar Viper vs ler arquivo manualmente
   - 🎯 Entenda: Ordem de precedência (arquivo → env var → flag)

4. **12-Factor App Config**
   - 📖 Leia: https://12factor.net/config (5min de leitura)
   - 🎯 Entenda: Por que não commitar secrets no git

**✅ Checkpoint de Entendimento**:
- Consegue criar uma struct com 3 campos?
- Sabe o que são struct tags?
- Entende a diferença entre YAML, JSON, ENV vars?
- Por que Viper é melhor que `os.ReadFile()`?

**💻 AGORA IMPLEMENTE**:
1. Instale: `go get github.com/spf13/viper`
2. Abra `internal/config/config.go`
3. Crie struct `Config` com todos os campos necessários (veja ARCHITECTURE.md)
4. Implemente `LoadConfig()` usando Viper
5. Crie `configs/config.yaml` com valores de exemplo
6. Teste no `main.go`: carregue e imprima a config

**💡 Por que importa**:
Configuração externa é um dos 12 fatores de apps cloud-native. Essencial para qualquer sistema sério.

**🔗 Dependências**: Passo 1

---

### Passo 3: Setup do Logger

**📁 Arquivo**: `pkg/logger/logger.go`

**🎯 Objetivo**: Implementar logging estruturado para debug e monitoramento.

**📚 PESQUISE PRIMEIRO (1h)**:

1. **Por que logging estruturado?**
   - 📖 Leia: https://www.honeycomb.io/blog/structured-logging-and-your-team
   - 🎯 Entenda: Diferença entre `fmt.Println` e logs estruturados
   - 🎯 Entenda: Por que JSON logs são melhores para produção

2. **Níveis de Log**
   - 📖 Pesquise: "log levels debug info warn error"
   - 🎯 Entenda: Quando usar cada nível
   - 🎯 Entenda: Como filtrar logs por nível em produção

3. **Biblioteca Zap (Uber)**
   - 📖 Leia: https://pkg.go.dev/go.uber.org/zap
   - 📺 YouTube: "Golang Zap Logger Tutorial"
   - 🎯 Entenda: Zap vs Logrus vs log padrão
   - 🎯 Entenda: `Logger` vs `SugaredLogger`
   - 📖 Veja exemplos: https://github.com/uber-go/zap#quick-start

4. **Observabilidade básica**
   - 📖 Leia: https://sre.google/sre-book/monitoring-distributed-systems/
   - 🎯 Entenda: Logs, Métricas, Traces (3 pilares)

**✅ Checkpoint de Entendimento**:
- Por que JSON logs são melhores que texto puro?
- Quando usar `logger.Debug()` vs `logger.Info()`?
- O que significa "structured logging"?
- Qual a diferença entre `Logger` e `SugaredLogger` no Zap?

**💻 AGORA IMPLEMENTE**:
1. Instale: `go get go.uber.org/zap`
2. Abra `pkg/logger/logger.go`
3. Crie função `New(env string)` que retorna logger configurado
4. Para dev: logger legível. Para prod: JSON
5. Teste no main:
   ```go
   logger := logger.New("development")
   logger.Info("App iniciada", zap.String("versão", "1.0"))
   ```

**💡 Por que importa**:
Sem logs estruturados, debugar em produção é um pesadelo. ELK Stack, Datadog, e outras ferramentas dependem de JSON logs.

**🔗 Dependências**: Passo 2 (pra ler nível de log da config)

---

### Passo 4: Criar Modelos de Dados

**📁 Arquivos**:
- `internal/models/notification.go`
- `internal/models/response.go`

**🎯 Objetivo**: Definir as estruturas de dados que o sistema vai manipular.

**O que fazer**:
1. Abra `internal/models/notification.go`
2. Copie a struct Notification do [ARCHITECTURE.md](ARCHITECTURE.md) (linhas 173-240)
3. Compile: `go build ./internal/models`
4. Deu erro? Leia e resolva (provavelmente import faltando)

**🔧 Se travar, pesquise**:
- "golang struct" - Sintaxe básica
- "golang json tags" - O que é `json:"id"`
- "golang time.Time" - Como usar datas
- "golang const" - Para os tipos (email, sms, push)
- Erro de import? `go get` da biblioteca que está faltando

**💡 Por que importa**:
Essa struct é o DNA do seu sistema. Todo mundo (API, Worker, Banco) vai usar ela.

**🔗 Dependências**: Nenhuma (só copiar e colar!)

---

### Passo 5: Subir RabbitMQ com Docker

**📁 Arquivo**: `docker-compose.yml`

**🎯 Objetivo**: Entender message brokers e ter infraestrutura rodando.

**📚 PESQUISE PRIMEIRO (2-3h)** - Este é um passo CRUCIAL!

1. **O que é Message Broker?**
   - 📺 YouTube: "Message Queue Explained"
   - 📖 Leia: https://aws.amazon.com/message-queue/benefits/
   - 🎯 Entenda: Por que filas existem (decoupling, async, resilience)
   - 🎯 Entenda: Diferença entre fila e pub/sub

2. **RabbitMQ Fundamentos** (MUITO IMPORTANTE!)
   - 📺 YouTube: "RabbitMQ in 100 Seconds" (Fireship)
   - 📺 YouTube: "RabbitMQ Tutorial for Beginners" (vídeo de 20-30min)
   - 📖 Leia: https://www.rabbitmq.com/tutorials/tutorial-one-go.html
   - 🎯 Entenda: Producer → Exchange → Queue → Consumer
   - 🎯 Entenda: Exchange types (direct, topic, fanout, headers)
   - 🎯 Entenda: Routing keys e bindings

3. **Conceitos Avançados**
   - 📖 Leia: https://www.rabbitmq.com/confirms.html
   - 🎯 Entenda: ACK/NACK (confirmação de processamento)
   - 🎯 Entenda: Dead Letter Exchange (DLX)
   - 🎯 Entenda: Message durability e persistence

4. **Docker básico** (se não souber)
   - 📺 YouTube: "Docker in 100 Seconds"
   - 📖 Leia: https://docs.docker.com/get-started/
   - 🎯 Entenda: Containers vs VMs
   - 🎯 Entenda: docker-compose.yml

**✅ Checkpoint de Entendimento**:
- O que é um Exchange? E uma Queue?
- Diferença entre Exchange tipo "topic" vs "direct"?
- O que acontece se worker não der ACK na mensagem?
- Para que serve Dead Letter Exchange?
- O que significa "message persistence"?

**💻 AGORA IMPLEMENTE**:
1. Copie o docker-compose.yml do [ARCHITECTURE.md](ARCHITECTURE.md)
2. Rode: `docker-compose up -d`
3. Acesse: http://localhost:15672 (admin/admin123)
4. **Explore manualmente**:
   - Crie um exchange "test.exchange" (tipo topic)
   - Crie uma queue "test.queue"
   - Faça binding com routing key "test.#"
   - Publique mensagem manual no exchange
   - Veja mensagem chegando na queue

**💡 Por que importa**:
RabbitMQ é o coração do seu sistema. Se não entender bem, vai sofrer debugando depois. Vale investir tempo aqui! Este é o diferencial entre dev júnior e pleno - entender arquitetura de filas.

**🔗 Dependências**: Nenhuma (infraestrutura independente)

---

### Passo 6: Conectar ao RabbitMQ

**📁 Arquivo**: `internal/queue/rabbitmq.go`

**🎯 Objetivo**: Estabelecer conexão com RabbitMQ e criar exchanges/queues.

**O que fazer**:
- Criar struct `RabbitMQ` que mantém conexão e canal
- Implementar `NewRabbitMQ(config)` - conecta ao RabbitMQ
- Implementar `SetupExchangesAndQueues()` - cria estrutura
- Implementar `Close()` - fecha conexão gracefully
- Tratar reconexão em caso de falha

**📖 O que estudar**:
- Biblioteca `amqp091-go`
- Connection pools e canais
- Declaração de exchanges (tipo "topic")
- Declaração de queues com DLX (Dead Letter Exchange)
- Arguments especiais (x-dead-letter-exchange, x-message-ttl)

**💡 Por que é importante**:
Gerenciar conexões corretamente evita memory leaks. Setup de exchanges/queues garante que mensagens não se percam.

**🔗 Dependências**: Passo 2 (Config), Passo 5 (RabbitMQ rodando)

---

### Passo 7: Implementar Producer (Publicador)

**📁 Arquivo**: `internal/queue/producer.go`

**🎯 Objetivo**: Publicar mensagens no RabbitMQ a partir da API.

**O que fazer**:
- Criar struct `Producer` com referência ao RabbitMQ
- Implementar `Publish(notification Notification)`
- Serializar notification para JSON
- Definir routing key baseado no tipo (`notification.email`)
- Adicionar propriedades (priority, content-type, persistent)

**📖 O que estudar**:
- Serialização JSON com `json.Marshal`
- Context para timeout nas operações
- Confirmações de publicação (publisher confirms)
- Propriedades de mensagens AMQP
- Error handling em Go (pattern `if err != nil`)

**💡 Por que é importante**:
O Producer é a ponte entre a API e as filas. Precisa ser confiável para não perder mensagens.

**🔗 Dependências**: Passo 4 (Models), Passo 6 (RabbitMQ)

---

### Passo 8: Criar API REST Básica

**📁 Arquivos**:
- `internal/api/router/router.go`
- `internal/api/handlers/health.go`
- `cmd/api/main.go`

**🎯 Objetivo**: Criar servidor HTTP que responde requisições.

**O que fazer**:
- Inicializar Gin no `main.go`
- Criar rota GET `/health` que retorna `{"status": "ok"}`
- Configurar CORS
- Adicionar middleware de logging
- Rodar e testar: `go run cmd/api/main.go`

**📖 O que estudar**:
- Framework Gin (http router)
- HTTP handlers e contexts
- Middlewares (chain of responsibility)
- Status codes HTTP
- Graceful shutdown

**💡 Por que é importante**:
A API é a porta de entrada do sistema. Health checks são usados por load balancers e Kubernetes.

**🔗 Dependências**: Passo 2 (Config), Passo 3 (Logger)

---

### Passo 9: Endpoint de Criar Notificação

**📁 Arquivo**: `internal/api/handlers/notification.go`

**🎯 Objetivo**: Receber requisição HTTP e enfileirar notificação.

**O que fazer**:
- Criar handler `CreateNotification`
- Fazer binding do JSON request para struct
- Validar dados (validator)
- Gerar UUID para a notificação
- Chamar Producer para publicar no RabbitMQ
- Retornar response com ID e status

**📖 O que estudar**:
- Gin binding e validation
- UUID generation (`google/uuid`)
- HTTP request/response cycle
- Status codes (201 Created, 400 Bad Request)
- Dependency injection (passar Producer pro handler)

**💡 Por que é importante**:
Este é o ponto crítico onde o sistema recebe trabalho. Validação aqui previne dados ruins nas filas.

**🔗 Dependências**: Passo 4 (Models), Passo 7 (Producer), Passo 8 (API)

---

### Passo 10: Implementar Consumer (Consumidor)

**📁 Arquivo**: `internal/queue/consumer.go`

**🎯 Objetivo**: Consumir mensagens da fila e processar.

**O que fazer**:
- Criar struct `Consumer` com callback function
- Implementar `Consume(queueName, handler)`
- Loop infinito consumindo mensagens
- Deserializar JSON para Notification
- Chamar função handler passada
- ACK se sucesso, NACK se erro (para retry)

**📖 O que estudar**:
- Go channels e goroutines
- Callback patterns (funções como parâmetros)
- ACK/NACK no RabbitMQ
- Requeue e retry logic
- Context para cancelamento graceful

**💡 Por que é importante**:
O Consumer é o "cérebro" dos workers. Ele precisa tratar erros corretamente para não perder mensagens.

**🔗 Dependências**: Passo 4 (Models), Passo 6 (RabbitMQ)

---

### Passo 11: Implementar Worker de Email

**📁 Arquivos**:
- `internal/workers/email_worker.go`
- `internal/services/email/smtp.go`
- `cmd/workers/email/main.go`

**🎯 Objetivo**: Processar notificações de email e enviar via SMTP.

**O que fazer**:
- Criar `EmailWorker` struct com config SMTP
- Implementar método `Process(notification)` que envia email
- Usar biblioteca `gomail` para SMTP
- Conectar Consumer ao EmailWorker no `main.go`
- Rodar worker: `go run cmd/workers/email/main.go`

**📖 O que estudar**:
- Protocol SMTP (Simple Mail Transfer Protocol)
- Biblioteca `gomail` ou `net/smtp`
- Goroutines (um por mensagem ou pool?)
- Error handling e retries
- Templates de email (HTML)

**💡 Por que é importante**:
Este é o primeiro worker real! Mostra como o sistema processa trabalho assíncrono de verdade.

**🔗 Dependências**: Passo 4 (Models), Passo 10 (Consumer)

---

### Passo 12: Teste End-to-End

**🎯 Objetivo**: Validar que todo o fluxo funciona.

**O que fazer**:
1. Subir RabbitMQ: `docker-compose up -d`
2. Rodar API: `go run cmd/api/main.go`
3. Rodar Worker Email: `go run cmd/workers/email/main.go`
4. Enviar request:
   ```bash
   curl -X POST http://localhost:8080/api/v1/notifications \
     -H "Content-Type: application/json" \
     -d '{"type":"email","to":"test@example.com","subject":"Test","body":"Hello"}'
   ```
5. Ver logs do worker processando
6. Verificar no RabbitMQ Management UI

**📖 O que estudar**:
- cURL ou Postman para testar APIs
- Como ler logs para debug
- Monitorar filas no RabbitMQ UI

**💡 Por que é importante**:
Ver o sistema funcionando end-to-end é motivador! Você já tem um sistema de filas funcional.

**🔗 Dependências**: Todos os passos anteriores

---

## 🚀 Fase 2: Expandir Funcionalidades

### Passo 13: Adicionar Mais Workers

**📁 Arquivos**:
- `internal/workers/sms_worker.go`
- `internal/services/sms/twilio.go`
- `cmd/workers/sms/main.go`

**🎯 Objetivo**: Suportar múltiplos tipos de notificação.

**O que fazer**:
- Seguir o mesmo padrão do Email Worker
- Implementar SMS com Twilio API
- Implementar Push com Firebase
- Implementar Webhook (HTTP POST)

**📖 O que estudar**:
- HTTP clients em Go (`net/http`)
- APIs REST externas
- Autenticação em APIs (API keys, OAuth)
- Rate limiting externo

**💡 Por que é importante**:
Mostra a flexibilidade da arquitetura. Adicionar novos tipos é fácil graças ao desacoplamento.

**🔗 Dependências**: Passo 11 (padrão de worker estabelecido)

---

### Passo 14: Implementar Retry Logic

**📁 Arquivo**: `internal/queue/consumer.go` (melhorar)

**🎯 Objetivo**: Retentar automaticamente notificações que falharam.

**O que fazer**:
- Adicionar header `x-retry-count` nas mensagens
- No NACK, incrementar contador
- Se atingir max_retries (ex: 3), enviar para DLQ
- Implementar exponential backoff (delay entre retries)

**📖 O que estudar**:
- Headers AMQP
- Dead Letter Queue pattern
- Exponential backoff strategy
- Idempotência (processar mesma mensagem 2x)

**💡 Por que é importante**:
Serviços externos falham. Retry automático aumenta a confiabilidade sem intervenção manual.

**🔗 Dependências**: Passo 10 (Consumer base), Passo 11 (Worker testado)

---

### Passo 15: Adicionar Persistência em Banco

**📁 Arquivos**:
- `migrations/001_create_notifications.sql`
- `internal/repository/notification_repo.go`

**🎯 Objetivo**: Guardar histórico de notificações para auditoria.

**O que fazer**:
- Criar tabela `notifications` no PostgreSQL
- Usar biblioteca `pgx` ou `gorm`
- Implementar CRUD (Create, Read, Update)
- Salvar notificação quando criada na API
- Atualizar status quando worker processar

**📖 O que estudar**:
- SQL básico (CREATE TABLE, INSERT, UPDATE)
- ORMs vs SQL puro
- Connection pools
- Migrations (versionamento de schema)
- Transações

**💡 Por que é importante**:
Banco de dados permite consultar histórico, gerar relatórios e auditar falhas.

**🔗 Dependências**: Passo 4 (Models), Passo 5 (adicionar Postgres ao docker-compose)

---

### Passo 16: Endpoint para Consultar Status

**📁 Arquivo**: `internal/api/handlers/notification.go` (expandir)

**🎯 Objetivo**: Cliente pode verificar se notificação foi enviada.

**O que fazer**:
- Criar rota GET `/api/v1/notifications/:id`
- Buscar no banco de dados
- Retornar JSON com status, timestamps, tentativas

**📖 O que estudar**:
- Path parameters no Gin (`:id`)
- Queries ao banco
- Error handling (404 Not Found)

**💡 Por que é importante**:
Visibilidade! Sistemas externos precisam saber se a notificação foi entregue.

**🔗 Dependências**: Passo 15 (Repository)

---

### Passo 17: Sistema de Templates

**🎯 Objetivo**: Reutilizar layouts de email/SMS.

**O que fazer**:
- Criar tabela `templates` no banco
- Usar `text/template` ou `html/template` do Go
- Permitir variáveis (ex: `{{.Name}}`, `{{.Code}}`)
- Criar endpoint POST `/api/v1/notifications/template`

**📖 O que estudar**:
- Template engines em Go
- Template parsing e execution
- Segurança (evitar XSS em templates HTML)

**💡 Por que é importante**:
Templates tornam o sistema muito mais útil. Marketing pode criar emails sem dev.

**🔗 Dependências**: Passo 15 (Banco)

---

### Passo 18: Sistema de Prioridades

**🎯 Objetivo**: Notificações urgentes são processadas primeiro.

**O que fazer**:
- Usar priority queues do RabbitMQ
- Adicionar campo `priority` (1-10) na mensagem
- Configurar workers para consumir por prioridade

**📖 O que estudar**:
- Priority queues
- x-max-priority argument
- Quando usar prioridades (cuidado com starvation)

**💡 Por que é importante**:
Alertas críticos (senha resetada) devem ser enviados antes de newsletters.

**🔗 Dependências**: Passo 6 (setup de queues)

---

## 🏭 Fase 3: Production Hardening (Pronto para Produção)

> **Objetivo desta fase**: Transformar o sistema funcional em um sistema **production-ready** com todas as garantias de segurança, qualidade, observabilidade e operações que um sistema real exige.
>
> **Importante**: Estes passos não são opcionais para produção. São requisitos mínimos para rodar com confiança.

### Passo 19: Autenticação com API Key ⚠️ CRÍTICO

**📁 Arquivo**: `internal/api/middleware/auth.go`

**🎯 Objetivo**: Proteger a API de acessos não autorizados.

**O que fazer**:
- Criar middleware que verifica header `X-API-Key`
- Comparar com chaves válidas (banco ou config)
- Retornar 401 Unauthorized se inválida
- Aplicar middleware nas rotas protegidas

**📖 O que estudar**:
- Middlewares em Gin
- HTTP headers
- Hashing de secrets (bcrypt)
- JWT (alternativa mais robusta)

**💡 Por que é importante**:
Segurança básica! Sem auth, qualquer um pode enviar notificações.

**🔗 Dependências**: Passo 8 (Router)

---

### Passo 20: Rate Limiting ⚠️ CRÍTICO

**📁 Arquivo**: `internal/api/middleware/ratelimit.go`

**🎯 Objetivo**: Prevenir abuso da API.

**O que fazer**:
- Implementar algoritmo Token Bucket ou Sliding Window
- Limitar por IP ou API Key (ex: 100 req/min)
- Retornar 429 Too Many Requests
- Usar Redis para contador distribuído

**📖 O que estudar**:
- Algoritmos de rate limiting
- Redis para state compartilhado
- HTTP 429 status code
- Header `Retry-After`

**💡 Por que é importante**:
Protege infraestrutura de DDoS acidental ou intencional.

**🔗 Dependências**: Passo 19 (Auth - rate limit por key)

---

### Passo 21: Métricas com Prometheus

**🎯 Objetivo**: Observabilidade - saber o que está acontecendo.

**O que fazer**:
- Adicionar `prometheus/client_golang`
- Criar métricas:
  - `notifications_total` (counter)
  - `notification_duration_seconds` (histogram)
  - `queue_size` (gauge)
- Expor endpoint `/metrics`
- Visualizar no Grafana

**📖 O que estudar**:
- Tipos de métricas (counter, gauge, histogram)
- Prometheus query language (PromQL)
- Grafana dashboards
- SLIs e SLOs

**💡 Por que é importante**:
Métricas são essenciais para detectar problemas antes dos usuários reclamarem.

**🔗 Dependências**: Passo 8 (API), pode adicionar em paralelo

---

### Passo 22: Logging Estruturado Avançado

**📁 Arquivo**: `pkg/logger/logger.go` (melhorar)

**🎯 Objetivo**: Logs que facilitam debugging em produção.

**O que fazer**:
- Adicionar trace_id em toda request (middleware)
- Propagar trace_id para workers via message headers
- Adicionar campos contextuais (user_id, notification_id)
- Configurar levels por ambiente

**📖 O que estudar**:
- Distributed tracing
- Correlation IDs
- Log aggregation (ELK, Loki)
- Structured logging best practices

**💡 Por que é importante**:
Seguir uma requisição através de API → Queue → Worker é impossível sem trace IDs.

**🔗 Dependências**: Passo 3 (Logger base)

---

### Passo 23: Dockerizar Aplicação

**📁 Arquivos**:
- `docker/Dockerfile.api`
- `docker/Dockerfile.worker`

**🎯 Objetivo**: Rodar aplicação em containers.

**O que fazer**:
- Criar multi-stage build (builder + runtime)
- Usar imagem Alpine (pequena)
- Copiar binário compilado
- Configurar via env vars
- Adicionar API e Workers ao `docker-compose.yml`

**📖 O que estudar**:
- Multi-stage Docker builds
- .dockerignore
- Container best practices
- Health checks em containers

**💡 Por que é importante**:
Containers garantem que aplicação roda igual em dev e prod. Base para Kubernetes.

**🔗 Dependências**: Todo o código funcional

---

### Passo 24: Graceful Shutdown

**📁 Arquivo**: `cmd/api/main.go` e workers (melhorar)

**🎯 Objetivo**: Desligar aplicação sem perder mensagens.

**O que fazer**:
- Capturar signals (SIGTERM, SIGINT)
- Parar de aceitar novas requests/mensagens
- Terminar trabalho em andamento
- Fechar conexões (DB, RabbitMQ)
- Retornar exit code correto

**📖 O que estudar**:
- Signal handling em Go
- Context cancellation
- Sync.WaitGroup para goroutines
- Graceful shutdown patterns

**💡 Por que é importante**:
Em prod, deployments acontecem o tempo todo. Shutdown graceful evita perda de dados.

**🔗 Dependências**: Qualquer código que mantém conexões

---

### Passo 25: Testes Automatizados ⚠️ CRÍTICO

**📁 Arquivos**:
- `internal/queue/producer_test.go`
- `internal/api/handlers/notification_test.go`
- `internal/workers/email_worker_test.go`

**🎯 Objetivo**: Garantir que código funciona e prevenir regressões. **Meta: > 80% de cobertura**.

**O que fazer**:
1. **Testes Unitários** - Funções isoladas com mocks
2. **Testes de Integração** - Com RabbitMQ/DB (testcontainers)
3. **Testes E2E** - Fluxo completo (API → Worker → Enviado)
4. **Testes de Carga** - Performance sob stress (k6, vegeta)

**📖 O que estudar**:
- `testing` package do Go
- Table-driven tests pattern
- Mocking com testify/mock ou mockery
- Testcontainers para infra real
- Benchmarks (`go test -bench`)
- Race detector (`go test -race`)

**💡 Por que é CRÍTICO para produção**:
Sem testes, você não tem confiança no código. Cada deploy é roleta russa. Testes são o que permite evoluir sistema sem quebrar.

**🔗 Dependências**: Todo o código implementado

---

### Passo 26: CI/CD Pipeline ⚠️ CRÍTICO

**📁 Arquivo**: `.github/workflows/ci.yml` ou `.gitlab-ci.yml`

**🎯 Objetivo**: Automatizar build, test e deploy. **Zero deploy manual**.

**O que fazer**:
1. **CI (Continuous Integration)**:
   - Rodar linter (golangci-lint)
   - Rodar todos os testes
   - Verificar code coverage
   - Build binários
   - Bloquear merge se falhar

2. **CD (Continuous Deployment)**:
   - Build imagens Docker
   - Push para registry (DockerHub, ECR, GCR)
   - Deploy para staging automaticamente
   - Deploy para prod após aprovação manual

**Pipeline Completo**:
```
Push/PR → Lint → Test → Build → Push Image → Deploy Staging → [Approval] → Deploy Prod
```

**📖 O que estudar**:
- GitHub Actions ou GitLab CI
- Docker multi-stage builds
- Container registries
- Blue/Green deployment
- Canary releases
- Rollback strategies

**💡 Por que é CRÍTICO para produção**:
Deploy manual = erro humano garantido. CI/CD garante que código quebrado nunca chega em prod e permite deploy múltiplas vezes ao dia com segurança.

**🔗 Dependências**: Passo 23 (Docker), Passo 25 (Testes)

---

### Passo 27: Documentação da API (Swagger)

**🎯 Objetivo**: Documentar endpoints para outros desenvolvedores.

**O que fazer**:
- Adicionar annotations `swaggo/swag`
- Gerar spec OpenAPI 3.0
- Servir Swagger UI em `/swagger`
- Documentar todos endpoints, responses, errors

**📖 O que estudar**:
- OpenAPI/Swagger specification
- Swagger annotations em Go
- API design best practices
- Versionamento de API

**💡 Por que é importante**:
Documentação atualizada automaticamente. Outros times podem integrar sem perguntar "como funciona?".

**🔗 Dependências**: Passo 8 (API completa)

---

### Passo 28: Monitoramento de Filas

**🎯 Objetivo**: Alertar quando filas estão crescendo demais.

**O que fazer**:
- Exportar métricas do RabbitMQ para Prometheus
- Criar alertas (ex: fila > 1000 mensagens)
- Dashboard Grafana com gráficos de filas
- PagerDuty ou similar para on-call

**📖 O que estudar**:
- RabbitMQ exporter para Prometheus
- Alertmanager
- SLOs (Service Level Objectives)
- On-call best practices

**💡 Por que é importante**:
Fila crescendo = workers não estão dando conta. Precisa escalar ou tem bug.

**🔗 Dependências**: Passo 21 (Prometheus)

---

### Passo 29: Escalar Workers Horizontalmente

**🎯 Objetivo**: Processar mais mensagens rodando múltiplas instâncias.

**O que fazer**:
- Rodar 3+ instâncias do mesmo worker
- RabbitMQ distribui mensagens (round-robin)
- Testar que não há race conditions
- Configurar auto-scaling (Kubernetes HPA)

**📖 O que estudar**:
- Horizontal vs vertical scaling
- Concurrency vs parallelism
- Consumer prefetch count
- Kubernetes Horizontal Pod Autoscaler

**💡 Por que é importante**:
Escalabilidade é o motivo de usar filas! Adicionar workers aumenta throughput linearmente.

**🔗 Dependências**: Passo 23 (Docker), sistema funcionando

---

### Passo 30: DLQ Monitoring e Reprocessing

**🎯 Objetivo**: Gerenciar mensagens que falharam definitivamente.

**O que fazer**:
- Criar dashboard para visualizar DLQ
- Implementar endpoint admin para republicar mensagens
- Analisar padrões de falha
- Alertar quando DLQ não está vazia

**📖 O que estudar**:
- Dead letter queue patterns
- Admin tools design
- Root cause analysis
- Chaos engineering

**💡 Por que é importante**:
DLQ é onde vão mensagens com bugs reais. Monitorar é crítico para qualidade.

**🔗 Dependências**: Passo 14 (Retry logic)

---

## 🎓 Go: O Que Você Vai Usar (e Aprender no Processo)

**Não decore isso! Você vai aprender conforme usar no projeto.**

### Primeiros Passos (vai usar logo)
- **Structs** - Passo 2 e 4 (Config e Models)
- **Ponteiros** - Passo 4 (campos opcionais)
- **Error handling** - Todo lugar! (`if err != nil`)
- **JSON** - Passo 9 (API recebe e envia JSON)

### Meio do Projeto (vai usar depois)
- **Goroutines** - Passo 10 (Consumer processa em paralelo)
- **Channels** - Passo 10 (comunicação entre goroutines)
- **Interfaces** - Passo 11 (Workers diferentes, mesma interface)
- **Context** - Passo 24 (shutdown graceful)

### Final/Avançado (só se quiser ir além)
- **Reflection** - Passo 15 (ORMs usam isso internamente)
- **Benchmarks** - Passo 25 (testar performance)
- **Race detector** - `go run -race` para detectar bugs de concorrência

**Dica**: Marque com ✅ conforme for usando cada conceito no projeto!

---

## 📊 Trilhas de Aprendizado

Escolha seu ritmo:

### 🏃 Fast Track (1-2 dias)
**Objetivo**: Ver o sistema funcionando o mais rápido possível
- Passos **1, 4, 5, 8, 9, 10, 11, 12**
- Pule logs, config avançada, persistence
- Foque: API recebe → RabbitMQ → Worker processa

### 🚶 Steady Pace (1-2 semanas)
**Objetivo**: Aprender direito sem pressa
- **Semana 1**: Passos 1-12 (MVP completo)
- **Semana 2**: Passos 13-18 (Expandir)
- 30min-1h por dia é suficiente

### 🐢 Deep Dive (1 mês+)
**Objetivo**: Dominar Go e arquitetura
- Todos os 30 passos
- Implemente variações (ex: worker de WhatsApp)
- Refatore e melhore o código depois

---

## 🎯 Checkpoint: Você Realmente Aprendeu?

Após cada passo, se pergunte:

1. **Funciona?** - Consegue rodar sem erro?
2. **Entende o que faz?** - Explica com suas palavras?
3. **Sabe debugar?** - Se quebrar, consegue achar o problema?

Se respondeu SIM às 3, próximo passo! Não precisa entender cada detalhe.

---

## 📚 Recursos Para Pesquisa Sob Demanda

### Quando Precisar de Sintaxe Rápida
- [Go by Example](https://gobyexample.com/) - Copie e cole exemplos
- [Cheat Sheet Go](https://devhints.io/go) - Sintaxe resumida
- [pkg.go.dev](https://pkg.go.dev/) - Documentação oficial de libs

### Quando Travar em Algo Específico
- **Google**: "golang [seu problema]" - Stack Overflow sempre tem resposta
- **ChatGPT/Claude**: Cole seu erro e pergunte
- **Go Playground**: [play.golang.org](https://play.golang.org) - Teste código rápido

### Se Quiser Aprofundar Depois (Opcional)
- [Tour of Go](https://go.dev/tour/) - Tutorial interativo oficial
- [Effective Go](https://go.dev/doc/effective_go) - Best practices
- Livro: "The Go Programming Language" - Quando quiser teoria

---

## 💡 Mentalidade: Aprender Fazendo

1. **Não tenha medo de copiar código** - Depois você entende. Primeiro faça funcionar.
2. **Erro é progresso** - Se compilou e deu erro, você está aprendendo. Leia a mensagem.
3. **Google é seu melhor amigo** - "golang [erro que deu]" resolve 90% dos problemas
4. **Commits pequenos** - Funcionou? Commita. Quebrou? Volta pro último commit.
5. **Teste sempre** - `go run` toda hora. Não escreva 100 linhas sem testar.
6. **Não busque perfeição** - Código feio que funciona > código bonito que não existe
7. **Reaprenda depois** - Primeiro faça rodar. Depois você refatora e entende melhor.

---

## 🐛 Debug: O Que Fazer Quando Travar

### Erro de Compilação
```bash
# Erro comum: "undefined: Gin"
# Solução: import faltando
import "github.com/gin-gonic/gin"
```
1. **Leia o erro** - Go diz exatamente o que está errado e em qual linha
2. **Google literal** - Copie a mensagem de erro inteira
3. **Verifique imports** - 80% dos erros são import faltando ou errado

### Código Compila mas Não Funciona
1. **Adicione prints**: `fmt.Printf("Chegou aqui! %+v\n", minhaVariavel)`
2. **Comente metade do código** - Vai eliminando até achar onde quebra
3. **Use o debugger** - VSCode tem debug visual (breakpoints)

### Travou de Verdade
1. **Pare e respire** - Às vezes você só precisa de 5 minutos longe do código
2. **Cole no ChatGPT/Claude** - "Este código Go dá erro X, como resolver?"
3. **r/golang ou Stack Overflow** - Comunidade é receptiva com iniciantes

---

## 🎉 Você Vai Aprender Isso Tudo (Fazendo!)

Ao final, você terá:

✅ **Um sistema real rodando** - Não é tutorial, é projeto de verdade
✅ **Go na prática** - Structs, goroutines, channels, interfaces (sem decoreba)
✅ **Arquitetura de filas** - RabbitMQ, workers, retry, DLQ
✅ **Portfolio** - Link no GitHub para mostrar em entrevistas
✅ **Confiança** - Se fez isso, consegue fazer outros sistemas

---

## 🎯 Próximos Passos AGORA

**Modo Deep Dive Ativado!**

1. Comece pelo **Passo 1** (Go Modules)
2. Siga a estrutura: 📚 Pesquise → ✅ Teste entendimento → 💻 Implemente
3. **Não pule a pesquisa!** É onde o aprendizado real acontece
4. Faça anotações em um caderno/Notion do que aprender

**⚠️ NOTA IMPORTANTE**:
Ajustei os **Passos 1, 2, 3 e 5** para o formato Deep Dive com links de pesquisa detalhados. Os demais passos (6-30) ainda estão no formato antigo.

**Quando chegar no Passo 6**, me avise que vou ajustar os próximos passos para o formato Deep Dive também!

---

## 📓 Dica: Caderno de Aprendizado

Crie um arquivo `APRENDIZADO.md` no seu projeto e anote:

```markdown
# Diário de Aprendizado - API Notifications

## Passo 1: Go Modules
**Data**: 2025-11-02
**Tempo**: 1h

### O que aprendi:
- go.mod é o gerenciador de dependências
- go.sum guarda checksums para segurança
- Módulos evitam o antigo GOPATH

### Dúvidas/Para aprofundar depois:
- Como funciona replace no go.mod?
- Versionamento com major versions (v2, v3)

### Código chave que escrevi:
[link para commit]
```

Isso ajuda a consolidar o aprendizado e ter referência futura!

---

**Bora começar!** 🚀

*Deep Dive significa: entender REALMENTE, não apenas fazer funcionar. Você está no caminho certo!*

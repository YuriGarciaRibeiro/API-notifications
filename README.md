# Sistema de Notificações - Projeto de Estudo Completo

## 📚 Contexto do Projeto

Este é um **sistema de notificações production-ready** desenvolvido como projeto de aprendizado, mas com foco em qualidade, segurança e escalabilidade necessárias para uso em produção.

O projeto combina **aprendizado prático** com **desenvolvimento profissional** - cada decisão arquitetural considera requisitos reais de produção, incluindo alta disponibilidade, observabilidade, segurança e performance.

### 🎯 Objetivos do Projeto

**Dual Purpose: Aprender + Produção**

Este projeto tem dois objetivos simultâneos:

1. **Aprendizado profundo** de desenvolvimento backend moderno
2. **Sistema pronto para produção** com todas as garantias necessárias

Este projeto abrange múltiplas áreas do desenvolvimento backend moderno:

**🔤 Linguagem Go**
- Sintaxe, tipos, structs e interfaces
- Goroutines, channels e concorrência
- Gerenciamento de dependências com Go Modules
- Testing e error handling idiomático

**🏗️ Arquitetura de Sistemas**
- Producer-Consumer Pattern
- Event-Driven Architecture
- Clean Architecture / Separation of Concerns
- Conceitos de microserviços (workers independentes)

**📨 Message Brokers & Filas**
- RabbitMQ: Exchanges, Queues, Routing Keys
- Publish/Subscribe patterns
- Dead Letter Queues (DLQ)
- ACK/NACK e garantias de entrega
- Retry logic e exponential backoff

**🔔 Sistemas de Notificação**
- Push Notifications (Firebase Cloud Messaging)
- SMTP e envio de emails
- SMS via Twilio
- Webhooks e callbacks HTTP

**🔧 Integrações & APIs Externas**
- REST API design
- Autenticação com API keys
- Rate limiting
- HTTP clients e error handling

**🐳 DevOps & Containerização**
- Docker e Docker Compose
- Configuração de ambientes (dev/prod)
- Variáveis de ambiente
- Health checks e observabilidade

**💾 Persistência & Dados**
- PostgreSQL
- Migrations
- Repository pattern

**🔒 Segurança & Produção**
- Autenticação e autorização (API Keys, JWT)
- Validação de inputs e sanitização
- Rate limiting e proteção contra abuso
- Secrets management (variáveis de ambiente)
- HTTPS/TLS
- Auditoria e logs de segurança

**📊 Observabilidade & Monitoramento**
- Logging estruturado (Zap)
- Métricas (Prometheus)
- Health checks
- Tracing distribuído
- Alertas e SLOs

**🧪 Qualidade & Testes**
- Unit tests
- Integration tests
- E2E tests
- Code coverage
- Linting e formatação (golangci-lint)

### 🧠 Metodologia de Aprendizado: Deep Dive

Este projeto segue a metodologia **Deep Dive**:

1. **📚 Pesquisar PRIMEIRO** - Estudar conceitos e ler documentação
2. **✅ Checkpoint** - Validar entendimento antes de codificar
3. **💻 Implementar com Qualidade** - Aplicar o conhecimento seguindo padrões de produção
4. **🧪 Testar** - Garantir qualidade com testes automatizados
5. **🔄 Revisar & Refatorar** - Melhorar código mantendo qualidade

> **Importante**: Este projeto é desenvolvido com padrões de produção desde o início. Cada feature implementada considera segurança, performance, observabilidade e manutenibilidade.

## 📖 Documentação do Projeto

### Arquivos Principais

- **[ARCHITECTURE.md](ARCHITECTURE.md)** - Arquitetura completa do sistema com decisões técnicas e design RabbitMQ
- **[ROADMAP.md](ROADMAP.md)** - Guia passo-a-passo de implementação (formato Deep Dive nos Passos 1-5)
- **[GO-REFERENCE.md](GO-REFERENCE.md)** - Referência rápida de sintaxe Go e boas práticas

### Como Usar Esta Documentação

1. Comece pelo **ROADMAP.md** no Passo 1
2. Use **GO-REFERENCE.md** quando tiver dúvidas de sintaxe
3. Consulte **ARCHITECTURE.md** para entender decisões de design
4. Pesquise os links fornecidos em cada passo do ROADMAP
5. Só implemente após entender os conceitos

## 🏗️ O Que Este Sistema Faz?

### Visão Geral

Sistema de notificações assíncrono que:
1. Recebe requisições via API REST
2. Enfileira mensagens no RabbitMQ
3. Workers processam as notificações
4. Envia notificações por diferentes canais

### Tipos de Notificação

- 📧 **Email** - Via SMTP
- 📱 **SMS** - Via Twilio
- 🔔 **Push** - Via Firebase
- 🔗 **Webhook** - HTTP POST para URLs externas

### Arquitetura Simplificada

```
Cliente → API (Gin) → RabbitMQ → Workers → Serviços Externos
                          ↓
                     PostgreSQL (opcional)
```

## 🚀 Começando

### Pré-requisitos

- Go 1.21+
- Docker e Docker Compose
- Editor de código (VS Code recomendado)

### Primeiro Passo

Abra o [ROADMAP.md](ROADMAP.md) e comece pelo **Passo 1: Go Modules**.

Cada passo tem:
- 📚 Materiais para pesquisar
- ✅ Perguntas para validar entendimento
- 💻 Tarefas de implementação

## 📊 Progresso Atual

### Fase 1: Fundação (MVP)
- [x] Planejamento e arquitetura
- [x] Estrutura de pastas criada
- [x] Documentação completa
- [ ] Passo 1: Go Modules
- [ ] Passo 2: Configuração (Viper)
- [ ] Passo 3: Logging (Zap)
- [ ] Passo 4: Health Check
- [ ] Passo 5: RabbitMQ Setup (CRUCIAL)
- [ ] Passo 6-12: API + Workers básicos

### Fase 2: Features (Expandir)
- [ ] Passo 13-18: Mais tipos de notificação + features

### Fase 3: Production-Ready (Hardening)
- [ ] Segurança (autenticação, rate limiting, input validation)
- [ ] Testes (unit, integration, e2e)
- [ ] Observabilidade (métricas, traces, dashboards)
- [ ] CI/CD pipeline
- [ ] Documentação de deploy
- [ ] Performance tuning
- [ ] Disaster recovery & backups

## 🎓 Tecnologias e Ferramentas

### Stack Principal

- **Go 1.21+** - Linguagem de programação
- **RabbitMQ** - Message broker (AMQP)
- **PostgreSQL** - Banco de dados (opcional)
- **Docker** - Containerização

### Bibliotecas Go

**Core**
- **Gin** (`github.com/gin-gonic/gin`) - Web framework HTTP
- **Viper** (`github.com/spf13/viper`) - Gerenciamento de configuração
- **Zap** (`go.uber.org/zap`) - Logging estruturado de alta performance
- **RabbitMQ** (`github.com/rabbitmq/amqp091-go`) - Cliente oficial RabbitMQ

**Segurança**
- **bcrypt** (`golang.org/x/crypto/bcrypt`) - Hash de senhas
- **JWT** (`golang-jwt/jwt`) - Tokens de autenticação
- **validator** (`go-playground/validator`) - Validação de dados

**Testes & Qualidade**
- **testify** (`stretchr/testify`) - Assertions e mocks
- **golangci-lint** - Linter agregador
- **mockery** - Geração de mocks

**Observabilidade**
- **Prometheus** (`prometheus/client_golang`) - Métricas
- **OpenTelemetry** - Tracing distribuído

### Serviços Externos (APIs)

- **Firebase Cloud Messaging (FCM)** - Push notifications mobile/web
- **Twilio API** - Envio de SMS
- **SMTP** - Servidores de email (Gmail, SendGrid, etc.)
- **Webhooks** - Callbacks HTTP customizados

### DevOps

- **Docker Compose** - Orquestração local
- **Make** - Automação de comandos
- **Bash scripts** - Setup e deploy

## 💡 Princípios de Desenvolvimento

### Aprendizado
1. **Não pule as pesquisas** - O aprendizado está na exploração profunda dos conceitos
2. **Faça os checkpoints** - Validar entendimento evita código confuso e bugs
3. **Entenda o "porquê"** - Não copie código sem entender as decisões arquiteturais
4. **Leia código de outros** - Veja projetos reais com Go + RabbitMQ no GitHub
5. **Estude os erros** - Quando algo quebrar, investigue a fundo antes de corrigir

### Qualidade (Production-Ready)
6. **Segurança desde o início** - Nunca deixe segurança para depois
7. **Teste conforme desenvolve** - Escreva testes junto com o código
8. **Logs estruturados sempre** - Todo evento importante deve ser logado
9. **Valide todos os inputs** - Nunca confie em dados externos
10. **Pense em falhas** - O que acontece se o RabbitMQ cair? E o banco?
11. **Monitore tudo** - Métricas são essenciais para produção
12. **Documente decisões** - README, ADRs, comentários no código

## ✅ Production Readiness Checklist

Requisitos para considerar o sistema pronto para produção:

### Funcionalidades Core
- [ ] API REST funcionando com todos os endpoints
- [ ] Workers processando Email, SMS, Push, Webhook
- [ ] RabbitMQ configurado com DLQ e retry logic
- [ ] Persistência de histórico de notificações (PostgreSQL)

### Segurança
- [ ] Autenticação de API (API Keys ou JWT)
- [ ] Rate limiting implementado
- [ ] Validação de todos os inputs
- [ ] Secrets em variáveis de ambiente (nunca no código)
- [ ] HTTPS configurado (TLS)
- [ ] CORS configurado corretamente
- [ ] Logs de auditoria para ações críticas

### Qualidade & Testes
- [ ] Cobertura de testes > 80%
- [ ] Testes unitários para toda lógica de negócio
- [ ] Testes de integração com RabbitMQ
- [ ] Testes E2E dos fluxos principais
- [ ] CI pipeline rodando testes automaticamente
- [ ] Linting passando (golangci-lint)

### Observabilidade
- [ ] Logging estruturado (JSON) com níveis corretos
- [ ] Métricas expostas (Prometheus format)
- [ ] Health checks (/health, /ready)
- [ ] Tracing distribuído configurado
- [ ] Dashboard de monitoramento (Grafana)
- [ ] Alertas configurados para erros críticos

### Operações & Deploy
- [ ] Docker images otimizadas
- [ ] Docker Compose para ambiente completo
- [ ] Variáveis de ambiente documentadas (.env.example)
- [ ] Scripts de migração de banco
- [ ] Documentação de deploy
- [ ] Graceful shutdown implementado
- [ ] Backup e restore documentados

### Performance & Escalabilidade
- [ ] Conexões com pool (banco, RabbitMQ)
- [ ] Timeouts configurados
- [ ] Workers podem escalar horizontalmente
- [ ] Testes de carga realizados
- [ ] Limites de recursos documentados (CPU, RAM)

### Documentação
- [ ] README completo
- [ ] API documentada (Swagger/OpenAPI)
- [ ] Guia de troubleshooting
- [ ] Runbook para operações
- [ ] Decisões arquiteturais documentadas (ADRs)

---

## 🔄 Status Atual

**Fase Atual**: Planejamento completo ✅

**Próximo Passo**: Passo 1 - Inicialização do projeto com Go Modules

**Meta**: Construir MVP funcional → Expandir features → Hardening para produção

---

**Nota**: Este checklist será preenchido conforme o projeto avança. O objetivo é ter todos os itens ✅ antes de considerar production-ready.

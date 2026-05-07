# Design: Segregação de Acesso da API (UI + Integration)

Data: 2026-05-07  
Projeto: API-notifications  
Objetivo principal: separar claramente o acesso da API entre uso humano (front/UI) e integração entre sistemas internos, mantendo simplicidade operacional em Docker.

## 1. Contexto e objetivo

O sistema será o hub interno de notificações da empresa. Múltiplas soluções internas irão enviar notificações por integração, enquanto a visualização e gestão operacional continuará concentrada no front do projeto.

A segregação deve:

1. Manter baixo custo operacional (sem introduzir OAuth/IdP neste momento)
2. Tornar explícito no contrato HTTP quem é UI e quem é integração
3. Permitir governança de acesso por sistema de origem

## 2. Escopo

Incluído:

1. Prefixos de rota por domínio de acesso (`/api/ui` e `/api/integration`)
2. Autenticação JWT para UI e API Key para integração
3. Criação de notificação por ambos os canais
4. Isolamento de visualização por sistema de origem no front
5. Modelo de identidade para clientes de integração e vínculo de acesso por usuário

Excluído:

1. OAuth2 Client Credentials neste ciclo
2. API de sistema separada (`/api/system`) para processos internos
3. Split por host em múltiplas APIs

## 3. Arquitetura de fronteiras

### 3.1 API de UI

Prefixo: `/api/ui/*`

1. Consumo humano via front
2. Autenticação por JWT de usuário
3. Endpoints de criação, consulta, monitoramento e gestão

### 3.2 API de Integration

Prefixo: `/api/integration/*`

1. Consumo por sistemas internos
2. Autenticação por `X-API-Key`
3. Escopo funcional restrito a envio de notificação

### 3.3 Processamento interno

Consumers, jobs e serviços internos permanecem sem superfície HTTP dedicada, usando código compartilhado da solução.

## 4. Modelo de identidade e autorização

### 4.1 Entidade de integração

`IntegrationClient`:

1. `Id`
2. `Name`
3. `ClientCode`
4. `ApiKeyHash`
5. `IsActive`
6. Campos operacionais opcionais (expiração, rotação, metadata)

### 4.2 Vínculo de visualização por origem

`UserSourceAccess`:

1. `UserId`
2. `SourceSystemId` (referência ao cliente/sistema de origem)

### 4.3 Regras de autorização

1. UI: usuário autenticado + permissões atuais (`notification.*`) + filtro por origem permitida
2. Integration: API Key válida e ativa, vinculada a um único sistema
3. Permissão global opcional para perfil administrativo visualizar todas as origens (`notification.view-all`)

## 5. Fluxos e contratos

### 5.1 Criação via UI

Endpoint: `POST /api/ui/notifications`

1. Autentica JWT
2. Valida `notification.create`
3. Persiste `Origin = User`
4. `SourceSystemId` pode ser informado apenas se usuário tiver acesso à origem

### 5.2 Criação via Integration

Endpoint: `POST /api/integration/notifications`

1. Valida `X-API-Key`
2. Resolve `IntegrationClient` ativo
3. Persiste `Origin = Api`
4. Persiste `SourceSystemId` derivado da chave (não livre no payload)

### 5.3 Consulta via UI

Endpoint: `GET /api/ui/notifications`

1. Usuário comum: retorno filtrado por origens permitidas
2. Usuário com `notification.view-all`: retorno sem filtro por origem

### 5.4 Contrato de payload

Payloads de criação permanecem próximos entre UI e Integration para reduzir fricção de uso, com diferença de identidade de origem derivada do canal de autenticação.

## 6. Segurança e observabilidade

1. API Key armazenada apenas como hash
2. Rotação de chave prevista (nova chave + revogação controlada)
3. `401` para token/chave inválidos
4. `403` para acesso não autorizado à origem
5. `400/422` para validação de payload
6. Padronização de erro com `ProblemDetails`
7. Auditoria com `Origin`, `SourceSystemId`, `IntegrationClientId`, `UserId`, `CorrelationId`

## 7. Estratégia de rollout

1. Introduzir `/api/integration/notifications` em paralelo
2. Introduzir prefixo `/api/ui/*` preservando comportamento funcional atual
3. Ajustar front para consumir rotas `ui`
4. Endurecer políticas e remover/encaminhar rotas legadas quando houver adoção completa

## 8. Testes mínimos de aceitação

1. Integração: envio com API Key válida retorna sucesso
2. Integração: envio com API Key inválida/inativa retorna `401`
3. UI: criação com JWT e permissão adequada retorna sucesso
4. UI: consulta filtra por `SourceSystemId` conforme vínculo de acesso
5. UI: perfil global visualiza todas as origens
6. Regressão de fronteira: endpoints `integration` não expõem leitura operacional

## 9. Decisões registradas

1. OAuth2 foi descartado no ciclo atual por simplicidade operacional
2. `/api/system` foi removido por não haver necessidade de exposição HTTP para processos internos da solução
3. Criação de notificação permanece disponível tanto via UI quanto via Integration

## 10. Próxima etapa

Com este design aprovado, a próxima etapa é elaborar o plano detalhado de implementação com o skill `superpowers:writing-plans`.

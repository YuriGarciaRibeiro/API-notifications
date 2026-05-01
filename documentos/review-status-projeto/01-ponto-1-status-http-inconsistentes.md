# Ponto 1 (Alta): status HTTP inconsistentes com contrato

## Resumo executivo

Hoje alguns endpoints anunciam um status (ex: `404`) na documentacao/Swagger, mas retornam outro (geralmente `400` ou `500`) em runtime.  
Esse e o problema mais importante porque quebra contrato de API, confunde frontend e dificulta observabilidade.

---

## Sintoma principal

- Endpoint promete `404` para recurso nao encontrado.
- Handler devolve `FluentResults.Error` generico (ou exception de infraestrutura).
- `ResultExtensions` so reconhece `DomainError`.
- Resultado: status incorreto no response final.

---

## Causa raiz (tecnica)

### 1) Mapeamento de erro incompleto no `ResultExtensions`

`ResultExtensions` extrai status apenas de erros do tipo `DomainError`.  
Quando o handler faz `Result.Fail(new Error(...))`, o tipo nao e `DomainError`, logo cai no fallback.

Arquivo:  
`src/NotificationSystem.Api/Extensions/ResultExtensions.cs`

### 2) Use cases de bulk retornando `Error` generico

Exemplos:
- `GetBulkNotificationJobHandler`
- `GetBulkNotificationProgressHandler`
- `GetBulkNotificationItemsHandler`
- `CancelBulkNotificationHandler`

Todos usam `Result.Fail(new Error("NotFound")...)` em vez de `NotFoundError` (ou outro `DomainError` especifico).

### 3) Fluxos de provider retornando sucesso silencioso ou exception 500

- `ToggleActiveStatusAsync` e `DeleteAsync` usam `ExecuteUpdateAsync`/`ExecuteDeleteAsync`, mas o retorno (linhas afetadas) nao e validado.
- `SetAsPrimaryAsync` faz `throw new InvalidOperationException` se nao achar provider.

Consequencia:
- pode retornar `204` mesmo sem registro;
- ou gerar `500` onde deveria ser `404`.

---

## Impacto pratico

- Frontend nao consegue tratar erros de forma confiavel.
- Clientes gerados por OpenAPI ficam semanticamente errados.
- Logs e metricas por status ficam poluidos.
- Contratos de integracao externa ficam instaveis.

---

## Onde atacar no codigo

### Camada Application (handlers)

- `src/NotificationSystem.Application/UseCases/GetNotificationById/GetNotificationByIdHandler.cs`
- `src/NotificationSystem.Application/UseCases/GetBulkNotificationJob/GetBulkNotificationJobHandler.cs`
- `src/NotificationSystem.Application/UseCases/GetBulkNotificationProgress/GetBulkNotificationProgressHandler.cs`
- `src/NotificationSystem.Application/UseCases/GetBulkNotificationItems/GetBulkNotificationItemsHandler.cs`
- `src/NotificationSystem.Application/UseCases/CancelBulkNotification/CancelBulkNotificationHandler.cs`

### Camada Infrastructure (repositorio provider)

- `src/NotificationSystem.Infrastructure/Persistence/Repositories/ProviderConfigurationRepository.cs`

### Camada API (mapeamento final)

- `src/NotificationSystem.Api/Extensions/ResultExtensions.cs`

---

## Estrategia de correcao recomendada

## Fase A - padronizar handlers para `DomainError`

Objetivo: parar de usar `Error` generico para cenarios de negocio esperados.

1. Trocar `Result.Fail(new Error("NotFound")...)` por `Result.Fail(new NotFoundError(...))`.
2. Para regra de negocio (ex: job nao pode ser cancelado), usar `ConflictError` ou `ValidationError` conforme semantica.
3. Evitar `throw` para fluxo esperado de dominio.

Beneficio: `ResultExtensions` ja sabe mapear status automaticamente.

## Fase B - corrigir repositorio de provider para nao mentir status

1. `ToggleActiveStatusAsync`: capturar linhas afetadas e falhar com `NotFoundError` quando `0`.
2. `DeleteAsync`: mesma ideia.
3. `SetAsPrimaryAsync`: nao lancar `InvalidOperationException` para "nao encontrado"; retornar erro de dominio.

## Fase C - hardening do `ResultExtensions`

Mesmo com Fase A/B, vale robustecer:

1. Se receber `Error` sem `DomainError`, tentar ler metadata de status (`StatusCode`) se existir.
2. Sem metadata, manter fallback consistente (ex: `400`) mas com log claro.

---

## Plano de implementacao (ordem segura)

1. Corrigir handlers de leitura (`GetNotificationById`, `GetBulk...`).
2. Corrigir `CancelBulkNotificationHandler`.
3. Corrigir repositorio e handlers de provider.
4. Ajustar `ResultExtensions` para resiliencia.
5. Rodar build e smoke manual dos endpoints criticos.

---

## Checklist de teste rapido

- `GET /api/notifications/{id-inexistente}` retorna `404`.
- `GET /api/notifications/bulk/{jobId-inexistente}` retorna `404`.
- `DELETE /api/admin/providers/{id-inexistente}` retorna `404` (nao `204`).
- `POST /api/admin/providers/{id-inexistente}/set-primary` retorna `404` (nao `500`).
- Erros de validacao continuam `400`.

---

## Definicao de pronto para o ponto 1

- Nenhum endpoint prometendo `404` retorna `400/500` para "nao encontrado".
- Exceptions de fluxo esperado substituidas por erros de dominio.
- Swagger e runtime coerentes.

---

## Status de execucao (2026-05-01)

Implementacao concluida para as fases A, B e C deste ponto:

- `GetNotificationByIdHandler` agora retorna `NotFoundError` (sem `Error` generico).
- `GetBulkNotificationItemsHandler` diferencia `job` inexistente (404) de lista vazia (200 com array vazio).
- `CancelBulkNotificationHandler` padronizado com `NotFoundError` e `ConflictError`.
- `ProviderConfigurationRepository` nao usa mais `throw` para "nao encontrado" em `SetAsPrimaryAsync`.
- `ToggleActiveStatusAsync` e `DeleteAsync` validam `affectedRows == 0` e retornam erro de dominio.
- `DeleteProvider` e `ToggleProviderActive` passaram a usar `Result`, permitindo retorno `404` real no endpoint.
- `ProviderEndpoints` (`toggle-active` e `delete`) agora usam `result.ToIResult()`.
- `ResultExtensions` foi robustecido para ler `StatusCode` em metadata de `Error` generico.

Validacao executada:

- `dotnet build NotificationSystem.slnx` com sucesso (0 erros, 0 avisos).
- Smoke manual executado em `2026-05-01` com autenticação admin:
  - `GET /api/notifications/{id-inexistente}` => `404`
  - `GET /api/notifications/bulk/{jobId-inexistente}` => `404`
  - `DELETE /api/admin/providers/{id-inexistente}` => `404`
  - `POST /api/admin/providers/{id-inexistente}/set-primary` => `404`

Conclusao operacional:

- Checklist de smoke do ponto 1 concluido com sucesso.

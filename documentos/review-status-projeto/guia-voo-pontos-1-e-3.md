# Guia de voo: executar ponto 1 e 3 com foco total

Este guia e para voce resolver offline, com baixa carga cognitiva.

## Objetivo do voo

1. Corrigir inconsistencia de status HTTP (ponto 1).
2. Corrigir alinhamento contrato back vs exemplo front no README (ponto 3).

---

## Bloco 1 - Ponto 1 (runtime contract)

## Passo 1: padronizar erros de "nao encontrado"

Trocar em handlers:
- `Result.Fail(new Error("NotFound")...)`
- por `Result.Fail(new NotFoundError(...))`

Arquivos alvo:
- `GetNotificationByIdHandler`
- `GetBulkNotificationJobHandler`
- `GetBulkNotificationProgressHandler`
- `GetBulkNotificationItemsHandler`
- `CancelBulkNotificationHandler`

## Passo 2: padronizar erro de regra de negocio

No cancelamento de bulk:
- trocar erro generico `CannotCancelCompletedJob` por `ConflictError` (ou `ValidationError`, se preferir semantica 400).

## Passo 3: provider repository sem falso positivo

- em `ToggleActiveStatusAsync` e `DeleteAsync`, verificar linhas afetadas;
- se `0`, retornar erro de dominio de nao encontrado;
- em `SetAsPrimaryAsync`, remover exception para fluxo esperado.

## Passo 4: reforco no `ResultExtensions`

- opcional mas recomendado: fallback lendo metadata de status quando vier `Error` generico.

## Passo 5: smoke tests manuais

Validar:
- recurso inexistente -> `404`;
- regra de conflito -> `409` (ou `400` se voce optar);
- sem `500` em fluxo de negocio esperado.

---

## Bloco 2 - Ponto 3 (documentation contract)

## Passo 1: revisar DTO real

Confirmar no backend:
- `NotificationDto` com `Channels[]`;
- discriminador `type` em `ChannelDto`.

## Passo 2: corrigir exemplo TS no README

Trocar exemplo para:
- iterar `response.notifications`;
- dentro de cada notificacao iterar `notification.channels`;
- fazer switch por `channel.type`.

## Passo 3: revisar nomes de tipos

Remover tipos que nao existem mais no exemplo (como `EmailNotificationDto`) e usar os realmente gerados pelo OpenAPI atual.

## Passo 4: validar coerencia final

Regra simples:
- qualquer campo exibido no README precisa existir na resposta real da API.

---

## Sugestao de commits (separar para facilitar review)

1. `fix(api): padroniza erros de dominio e status HTTP`
2. `fix(providers): evita 204/500 incorretos em recursos inexistentes`
3. `docs(readme): alinha exemplo frontend ao contrato atual de notificacoes`

---

## Criterio de sucesso do voo

- ponto 1 fechado com comportamento HTTP previsivel;
- ponto 3 fechado com README fiel ao contrato atual;
- build verde ao final.


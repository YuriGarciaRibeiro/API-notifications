# Ponto 5 (Baixa): convencoes mistas e debitos tecnicos pequenos

## Itens observados

1. Autorizacao com string literal em alguns endpoints (`"user.view"`, `"role.view"`) e constantes `Permissions.*` em outros.
2. Warning de API obsoleta do Hangfire.
3. Parametro nao usado em `RabbitMqConsumerBase`.

## Impacto

- impacto funcional baixo no curto prazo;
- aumenta chance de erro humano em manutencao;
- aciona warnings recorrentes no build.

## Correcao recomendada

1. Padronizar tudo para `Permissions.*`.
2. Migrar chamada obsoleta do Hangfire para overload atual recomendado.
3. Remover parametro nao usado ou justificar uso.

## Definicao de pronto

- sem strings de permissao hardcoded nos endpoints;
- build limpo ou com warnings intencionais documentados.

## Status em 2026-05-01

- concluido.

## Evidencias da correcao aplicada

1. Endpoints de `Users` e `Roles` padronizados para `Permissions.*` (sem `RequireAuthorization("...")` hardcoded).
2. `UsePostgreSqlStorage` migrado para overload atual recomendado com `Action<PostgreSqlBootstrapperOptions>`.
3. Parametro nao usado removido de `RabbitMqConsumerBase` e construtores dos workers ajustados.
4. Build validado com `dotnet build NotificationSystem.slnx -v minimal`: `0 Warning(s)` e `0 Error(s)`.

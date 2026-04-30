# Provider Rollout

## Sequência sugerida

1. Implementar novo provider sem alterar rotas públicas.
2. Registrar no container DI.
3. Habilitar configuração no banco com `IsActive=false`.
4. Validar em ambiente controlado.
5. Ativar provider e monitorar erros/retry/DLQ.

## Critérios de aceite

- Mensagens enviadas com sucesso no canal alvo.
- Status de canal atualizado corretamente.
- Falhas transitórias seguem retry.

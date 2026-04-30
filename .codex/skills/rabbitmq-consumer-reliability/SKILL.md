---
name: rabbitmq-consumer-reliability
description: Criar ou evoluir consumers RabbitMQ resilientes para o sistema de notificações, com fila principal, DLQ, retry strategy, observabilidade e atualização de status por canal. Usar quando Codex precisar implementar novo worker, ajustar confiabilidade de processamento assíncrono, investigar falhas recorrentes de consumo, ou padronizar comportamento de erro entre consumers Email/Sms/Push/Bulk.
---

# RabbitMQ Consumer Reliability

## Objetivo

Manter comportamento consistente de consumo assíncrono: processar mensagem, aplicar retry, atualizar status e encaminhar falhas definitivas para DLQ.

## Fluxo Padrão

1. Herdar de `RabbitMqConsumerBase<TMessage>`.
2. Definir `QueueName` e tipo de canal.
3. Implementar `ProcessMessageAsync` sem acoplamento HTTP.
4. Delegar retry e tratamento para `MessageProcessingMiddleware<TMessage>`.
5. Atualizar status de canal com `INotificationRepository`.
6. Validar baseline com `scripts/check_consumer_baseline.sh`.

## Checklist de Confiabilidade

- Declarar DLQ/DLX por fila.
- Confirmar `BasicAck` apenas em sucesso.
- Confirmar `BasicNack(requeue:false)` em falha permanente.
- Garantir logs com `NotificationId` e `ChannelId`.
- Evitar `catch` que engole exceção silenciosamente.

## Referências

- Fluxo de lifecycle: [references/consumer-lifecycle.md](references/consumer-lifecycle.md)
- Heurísticas de falha e retry: [references/retry-and-dlq-playbook.md](references/retry-and-dlq-playbook.md)

## Scripts

- `scripts/check_consumer_baseline.sh`: verifica sinais mínimos de robustez em `Worker.cs`.

# Consumer Lifecycle

## Sequência

1. Abrir conexão e canal.
2. Declarar exchange/queue de DLQ.
3. Declarar fila principal com argumentos de dead-letter.
4. Consumir mensagem sem auto-ack.
5. Deserializar payload.
6. Processar via middleware com retry.
7. Ack em sucesso; Nack para DLQ em falha permanente.

## Responsabilidades

- Base consumer: infraestrutura de fila e entrega.
- Worker específico: regra de processamento do canal.
- Middleware: retry, observabilidade e atualização de status de falha.

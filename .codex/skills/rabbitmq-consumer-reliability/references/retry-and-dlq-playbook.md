# Retry and DLQ Playbook

## Quando aplicar retry

- Falha transitória de provedor externo.
- Timeout de rede.
- Saturação temporária de recurso.

## Quando nao reprocessar

- Payload inválido.
- Falha de serialização não recuperável.
- Erro de regra que exige correção de código/dados.

## Resultado esperado

- Mensagem com falha permanente deve ir para DLQ.
- Status de canal deve ser `Failed` no repositório.
- Logs devem permitir correlação por IDs.

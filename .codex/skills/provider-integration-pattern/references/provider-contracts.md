# Provider Contracts

## Camadas

- Application: definir interfaces e fluxo de uso.
- Infrastructure: implementar acesso a SDK/API externa.
- Worker/UseCase: consumir abstrações já registradas em DI.

## Regras

- Evitar lógica de seleção de provider em endpoint ou use case.
- Encapsular fallback e prioridade no factory.
- Expor comportamento com logs claros (provider escolhido, canal, resultado).
